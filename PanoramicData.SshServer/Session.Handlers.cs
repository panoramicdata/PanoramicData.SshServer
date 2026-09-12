using PanoramicData.SshServer.Messages;
using PanoramicData.SshServer.Services;
using System;

namespace PanoramicData.SshServer;

/// <summary>
/// Handling of the messages this session processes itself.
/// </summary>
public partial class Session
{
	/// <summary>
	/// Occurs when a service is registered.
	/// </summary>
	public event EventHandler<SshService>? ServiceRegistered;

	/// <summary>
	/// Occurs when keys are exchanged.
	/// </summary>
	public event EventHandler<KeyExchangeArgs>? KeysExchanged;

	private void HandleMessageCore(Message message)
	{
		if (TryHandleTransportMessage(message))
		{
			return;
		}

		switch (message)
		{
			case UserAuthServiceMessage userAuthServiceMessage:
				HandleMessage(userAuthServiceMessage);
				break;
			case ConnectionServiceMessage connectionServiceMessage:
				HandleMessage(connectionServiceMessage);
				break;
			default:
				throw new NotImplementedException();
		}
	}

	/// <summary>
	/// Handles the transport-layer messages this session deals with itself.
	/// </summary>
	/// <returns>True if the message was a transport-layer message and has been handled.</returns>
	private bool TryHandleTransportMessage(Message message)
	{
		switch (message)
		{
			case DisconnectMessage disconnectMessage:
				HandleMessage(disconnectMessage);
				return true;
			case KeyExchangeInitMessage keyExchangeInitMessage:
				HandleMessage(keyExchangeInitMessage);
				return true;
			case KeyExchangeDhInitMessage keyExchangeDhInitMessage:
				HandleMessage(keyExchangeDhInitMessage);
				return true;
			case NewKeysMessage:
				HandleNewKeys();
				return true;
			case UnimplementedMessage:
				// The peer is telling us it did not implement one of our messages. There is
				// nothing to do about it beyond not treating it as an unknown message type.
				return true;
			case ServiceRequestMessage serviceRequestMessage:
				HandleMessage(serviceRequestMessage);
				return true;
			default:
				return false;
		}
	}

	private void HandleMessage(DisconnectMessage message)
		=> Disconnect(message.ReasonCode, message.Description);

	private void HandleMessage(KeyExchangeInitMessage message)
	{
		ConsiderReExchange(true);

		KeysExchanged?.Invoke(this, new KeyExchangeArgs(this, message));

		_exchangeContext!.KeyExchange = ChooseAlgorithm([.. _keyExchangeAlgorithms.Keys], message.KeyExchangeAlgorithms);
		_exchangeContext.PublicKey = ChooseAlgorithm([.. _publicKeyAlgorithms.Keys], message.ServerHostKeyAlgorithms);
		_exchangeContext.ClientEncryption = ChooseAlgorithm([.. _encryptionAlgorithms.Keys], message.EncryptionAlgorithmsClientToServer);
		_exchangeContext.ServerEncryption = ChooseAlgorithm([.. _encryptionAlgorithms.Keys], message.EncryptionAlgorithmsServerToClient);
		_exchangeContext.ClientHmac = ChooseAlgorithm([.. _hmacAlgorithms.Keys], message.MacAlgorithmsClientToServer);
		_exchangeContext.ServerHmac = ChooseAlgorithm([.. _hmacAlgorithms.Keys], message.MacAlgorithmsServerToClient);
		_exchangeContext.ClientCompression = ChooseAlgorithm([.. _compressionAlgorithms.Keys], message.CompressionAlgorithmsClientToServer);
		_exchangeContext.ServerCompression = ChooseAlgorithm([.. _compressionAlgorithms.Keys], message.CompressionAlgorithmsServerToClient);

		_exchangeContext!.ClientKexInitPayload = message.GetPacket();
	}

	private void HandleMessage(KeyExchangeDhInitMessage message)
	{
		var kexAlg = _keyExchangeAlgorithms[_exchangeContext!.KeyExchange!]();
		var hostKeyAlg = _publicKeyAlgorithms[_exchangeContext.PublicKey!](_hostKey[_exchangeContext.PublicKey!]);
		var clientCipher = _encryptionAlgorithms[_exchangeContext.ClientEncryption!]();
		var serverCipher = _encryptionAlgorithms[_exchangeContext.ServerEncryption!]();
		var serverHmac = _hmacAlgorithms[_exchangeContext.ServerHmac!]();
		var clientHmac = _hmacAlgorithms[_exchangeContext.ClientHmac!]();

		var clientExchangeValue = message.E!;
		var serverExchangeValue = kexAlg.CreateKeyExchange();
		var sharedSecret = kexAlg.DecryptKeyExchange(clientExchangeValue);
		var hostKeyAndCerts = hostKeyAlg.CreateKeyAndCertificatesData();
		var exchangeHash = ComputeExchangeHash(kexAlg, hostKeyAndCerts, clientExchangeValue, serverExchangeValue, sharedSecret);

		ExchangeHash ??= exchangeHash;

		var clientCipherIV = ComputeEncryptionKey(kexAlg, exchangeHash, clientCipher.BlockSize >> 3, sharedSecret, 'A');
		var serverCipherIV = ComputeEncryptionKey(kexAlg, exchangeHash, serverCipher.BlockSize >> 3, sharedSecret, 'B');
		var clientCipherKey = ComputeEncryptionKey(kexAlg, exchangeHash, clientCipher.KeySize >> 3, sharedSecret, 'C');
		var serverCipherKey = ComputeEncryptionKey(kexAlg, exchangeHash, serverCipher.KeySize >> 3, sharedSecret, 'D');
		var clientHmacKey = ComputeEncryptionKey(kexAlg, exchangeHash, clientHmac.KeySize >> 3, sharedSecret, 'E');
		var serverHmacKey = ComputeEncryptionKey(kexAlg, exchangeHash, serverHmac.KeySize >> 3, sharedSecret, 'F');

		_exchangeContext.NewAlgorithms = new Algorithms
		{
			KeyExchange = kexAlg,
			PublicKey = hostKeyAlg,
			ClientEncryption = clientCipher.Cipher(clientCipherKey, clientCipherIV, false),
			ServerEncryption = serverCipher.Cipher(serverCipherKey, serverCipherIV, true),
			ClientHmac = clientHmac.Hmac(clientHmacKey),
			ServerHmac = serverHmac.Hmac(serverHmacKey),
			ClientCompression = _compressionAlgorithms[_exchangeContext.ClientCompression!](),
			ServerCompression = _compressionAlgorithms[_exchangeContext.ServerCompression!](),
		};

		var reply = new KeyExchangeDhReplyMessage
		{
			HostKey = hostKeyAndCerts,
			F = serverExchangeValue,
			Signature = hostKeyAlg.CreateSignatureData(exchangeHash),
		};

		SendMessage(reply);
		SendMessage(new NewKeysMessage());
	}

	private void HandleNewKeys()
	{
		_hasBlockedMessagesWaitHandle.Reset();

		lock (_locker)
		{
			_inboundFlow = 0;
			_outboundFlow = 0;
			_algorithms = _exchangeContext!.NewAlgorithms;
			_exchangeContext = null;
		}

		ContinueSendBlockedMessages();
		_hasBlockedMessagesWaitHandle.Set();
	}


	private void HandleMessage(ServiceRequestMessage message)
	{
		var service = RegisterService(message.ServiceName);
		if (service != null)
		{
			SendMessage(new ServiceAcceptMessage(message.ServiceName!));
			return;
		}

		throw new SshConnectionException(string.Format("Service \"{0}\" not available.", message.ServiceName),
			DisconnectReason.ServiceNotAvailable);
	}

	private void HandleMessage(UserAuthServiceMessage message)
		=> GetService<UserAuthService>()?.HandleMessageCore(message);

	private void HandleMessage(ConnectionServiceMessage message)
		=> GetService<ConnectionService>()?.HandleMessageCore(message);

	internal SshService? RegisterService(string? serviceName) => RegisterService(serviceName, null);

	internal SshService? RegisterService(string? serviceName, UserAuthArgs? auth)
	{
		var service = CreateService(serviceName, auth);

		if (service is not null)
		{
			ServiceRegistered?.Invoke(this, service);
			_services.Add(service);
		}

		return service;
	}

	/// <summary>
	/// Creates the service the client asked for, or null if it is unknown or already registered.
	/// </summary>
	private SshService? CreateService(string? serviceName, UserAuthArgs? auth)
	{
		if (serviceName == "ssh-userauth")
		{
			return GetService<UserAuthService>() is null ? new UserAuthService(this) : null;
		}

		if (serviceName == "ssh-connection")
		{
			return auth is not null && GetService<ConnectionService>() is null
				? new ConnectionService(this, auth)
				: null;
		}

		return null;
	}
}
