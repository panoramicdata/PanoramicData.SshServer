﻿using PanoramicData.SshServer.Algorithms;
using PanoramicData.SshServer.Messages;
using PanoramicData.SshServer.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace PanoramicData.SshServer;

public partial class Session
{
	private const byte CarriageReturn = 0x0d;
	private const byte LineFeed = 0x0a;
	internal const int MaximumSshPacketSize = LocalChannelDataPacketSize;
	internal const int InitialLocalWindowSize = LocalChannelDataPacketSize * 32;
	internal const int LocalChannelDataPacketSize = 1024 * 32;

	private static readonly Dictionary<byte, Type> _messagesMetadata;
	internal static readonly Dictionary<string, Func<KexAlgorithm>> _keyExchangeAlgorithms = [];
	internal static readonly Dictionary<string, Func<string?, PublicKeyAlgorithm>> _publicKeyAlgorithms = [];
	internal static readonly Dictionary<string, Func<CipherInfo>> _encryptionAlgorithms = [];
	internal static readonly Dictionary<string, Func<HmacInfo>> _hmacAlgorithms = [];
	internal static readonly Dictionary<string, Func<CompressionAlgorithm>> _compressionAlgorithms = [];

	private readonly ConcurrentDictionary<string, object?> _sessionVariables = [];

	private readonly Lock _locker = new();
	private readonly Socket _socket;
	private readonly TimeSpan _inactivityTimeout;
	private readonly Dictionary<string, string> _hostKey;

	private uint _outboundPacketSequence;
	private uint _inboundPacketSequence;
	private uint _outboundFlow;
	private uint _inboundFlow;
	private Algorithms? _algorithms = null;
	private ExchangeContext? _exchangeContext = null;
	private readonly List<SshService> _services = [];
	private readonly ConcurrentQueue<Message> _blockedMessages = new();
	private readonly ManualResetEvent _hasBlockedMessagesWaitHandle = new(true);

	public string ServerVersion { get; private set; }

	public string ClientVersion { get; private set; }

	public Guid Id { get; } = Guid.NewGuid();

	public byte[] ExchangeHash { get; private set; }

	private readonly ConcurrentDictionary<uint, TerminalSize> _terminalSizes = new();

	public TerminalSize GetTerminalSize(uint serverChannelId)
	{
		if (!_terminalSizes.TryGetValue(serverChannelId, out var size))
		{
			_terminalSizes[serverChannelId] = size = new TerminalSize(80, 25, 640, 480);
		}

		return size;
	}

	public void SetTerminalSize(uint serverChannelId, TerminalSize size)
		=> _terminalSizes[serverChannelId] = size;

	public T? GetService<T>() where T : SshService => (T?)_services.FirstOrDefault(x => x is T);

	static Session()
	{
		_keyExchangeAlgorithms.Add("diffie-hellman-group18-sha512", () => new DiffieHellmanGroupSha512(new DiffieHellman(8192)));
		_keyExchangeAlgorithms.Add("diffie-hellman-group16-sha512", () => new DiffieHellmanGroupSha512(new DiffieHellman(4096)));
		_keyExchangeAlgorithms.Add("diffie-hellman-group14-sha256", () => new DiffieHellmanGroupSha256(new DiffieHellman(2048)));
		_keyExchangeAlgorithms.Add("diffie-hellman-group14-sha1", () => new DiffieHellmanGroupSha1(new DiffieHellman(2048)));
		_keyExchangeAlgorithms.Add("diffie-hellman-group1-sha1", () => new DiffieHellmanGroupSha1(new DiffieHellman(1024)));

		_publicKeyAlgorithms.Add("rsa-sha2-256", x => new RsaKey(x));
		_publicKeyAlgorithms.Add("ssh-dss", x => new DssKey(x));

		_encryptionAlgorithms.Add("aes256-ctr", () => new CipherInfo(Aes.Create(), 256, CipherModeEx.CTR));
		_encryptionAlgorithms.Add("aes256-cbc", () => new CipherInfo(Aes.Create(), 256, CipherModeEx.CBC));

		_hmacAlgorithms.Add("hmac-sha2-256", () => new HmacInfo(new HMACSHA256(), 256));
		_hmacAlgorithms.Add("hmac-sha2-512", () => new HmacInfo(new HMACSHA512(), 512));

		_compressionAlgorithms.Add("none", () => new NoCompression());

		_messagesMetadata = (from t in typeof(Message).Assembly.GetTypes()
							 let attrib = (MessageAttribute?)t.GetCustomAttributes(typeof(MessageAttribute), false).FirstOrDefault()
							 where attrib != null
							 select new { attrib.Number, Type = t })
							 .ToDictionary(x => x.Number, x => x.Type);
	}

	public Session(Socket socket, Dictionary<string, string> hostKey, string serverBanner, TimeSpan inactivityTimeout)
	{
		ArgumentNullException.ThrowIfNull(socket, nameof(socket));
		ArgumentNullException.ThrowIfNull(hostKey, nameof(hostKey));
		ArgumentNullException.ThrowIfNull(serverBanner, nameof(serverBanner));

		_socket = socket;
		_hostKey = hostKey.ToDictionary(s => s.Key, s => s.Value);
		_inactivityTimeout = inactivityTimeout > TimeSpan.FromDays(365)
			? throw new ArgumentOutOfRangeException(nameof(inactivityTimeout), "Inactivity Timeout must be less than 1 year.")
			: inactivityTimeout;
		ServerVersion = serverBanner;
	}

	public event EventHandler<EventArgs> Disconnected;

	public event EventHandler<SshService> ServiceRegistered;

	public event EventHandler<KeyExchangeArgs> KeysExchanged;

	internal void EstablishConnection()
	{
		if (!_socket.Connected)
		{
			return;
		}

		SetSocketOptions();

		SocketWriteProtocolVersion();
		ClientVersion = SocketReadProtocolVersion();
		if (!SshVersionRegex().IsMatch(ClientVersion))
		{
			throw new SshConnectionException(
				string.Format("Not supported for client SSH version {0}. This server only supports SSH v2.0.", ClientVersion),
				DisconnectReason.ProtocolVersionNotSupported);
		}

		ConsiderReExchange(true);

		try
		{
			while (_socket != null && _socket.Connected)
			{
				var message = ReceiveMessage();
				switch (message)
				{
					case UnknownMessage unknownMessage:
						SendMessage(unknownMessage.MakeUnimplementedMessage());
						break;
					default:
						HandleMessageCore(message);
						break;
				}
			}
		}
		finally
		{
			foreach (var service in _services)
			{
				service.CloseService();
			}
		}
	}

	public void Disconnect(DisconnectReason reason = DisconnectReason.ByApplication, string description = "Connection terminated by the server.")
	{
		if (reason == DisconnectReason.ByApplication)
		{
			var message = new DisconnectMessage(reason, description);
			TrySendMessage(message);
		}

		try
		{
			_socket.Shutdown(SocketShutdown.Both);
			_socket.Close();
			_socket.Dispose();
		}
		catch { }

		Disconnected?.Invoke(this, EventArgs.Empty);
	}

	#region Socket operations
	private void SetSocketOptions()
	{
		const int socketBufferSize = 2 * MaximumSshPacketSize;
		_socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
		_socket.LingerState = new LingerOption(enable: false, seconds: 0);
		_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, socketBufferSize);
		_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, socketBufferSize);
		_socket.ReceiveTimeout = (int)_inactivityTimeout.TotalMilliseconds;
	}

	private string SocketReadProtocolVersion()
	{
		// http://tools.ietf.org/html/rfc4253#section-4.2
		var buffer = new byte[255];
		var dummy = new byte[255];
		var pos = 0;
		var len = 0;

		while (pos < buffer.Length)
		{
			var ar = _socket.BeginReceive(buffer, pos, buffer.Length - pos, SocketFlags.Peek, null, null);
			WaitHandle(ar);
			len = _socket.EndReceive(ar);

			if (len == 0)
			{
				throw new SshConnectionException("Couldn't read the protocol version", DisconnectReason.ProtocolError);
			}

			for (var i = 0; i < len; i++, pos++)
			{
				if (pos > 0 && buffer[pos - 1] == CarriageReturn && buffer[pos] == LineFeed)
				{
					_socket.Receive(dummy, 0, i + 1, SocketFlags.None);
					return Encoding.ASCII.GetString(buffer, 0, pos - 1);
				}
				else if (pos > 0 && buffer[pos] == LineFeed) // Non-RFC case
				{
					_socket.Receive(dummy, 0, i + 1, SocketFlags.None);
					return Encoding.ASCII.GetString(buffer, 0, pos);
				}
			}

			_socket.Receive(dummy, 0, len, SocketFlags.None);
		}

		throw new SshConnectionException("Couldn't read the protocol version", DisconnectReason.ProtocolError);
	}

	private void SocketWriteProtocolVersion() => SocketWrite(Encoding.ASCII.GetBytes(ServerVersion + "\r\n"));

	private byte[] SocketRead(int length)
	{
		var pos = 0;
		var buffer = new byte[length];

		var msSinceLastData = 0;

		while (pos < length)
		{
			try
			{
				var ar = _socket.BeginReceive(buffer, pos, length - pos, SocketFlags.None, null, null);
				WaitHandle(ar);
				var len = _socket.EndReceive(ar);
				if (!_socket.Connected)
				{
					throw new SshConnectionException("Connection lost", DisconnectReason.ConnectionLost);
				}

				if (len == 0 && _socket.Available == 0)
				{
					if (msSinceLastData >= _inactivityTimeout.TotalMilliseconds)
					{
						throw new SshConnectionException("Connection lost", DisconnectReason.ConnectionLost);
					}

					msSinceLastData += 50;
					Thread.Sleep(50);
				}
				else
				{
					msSinceLastData = 0;
				}

				pos += len;
			}
			catch (SocketException exp)
			{
				if (exp.SocketErrorCode is SocketError.WouldBlock or
					SocketError.IOPending or
					SocketError.NoBufferSpaceAvailable)
				{
					Thread.Sleep(30);
				}
				else
					throw new SshConnectionException("Connection lost", DisconnectReason.ConnectionLost);
			}
		}

		return buffer;
	}

	private void SocketWrite(byte[] data)
	{
		var pos = 0;
		var length = data.Length;

		while (pos < length)
		{
			try
			{
				var ar = _socket.BeginSend(data, pos, length - pos, SocketFlags.None, null, null);
				WaitHandle(ar);
				pos += _socket.EndSend(ar);
			}
			catch (SocketException ex)
			{
				if (ex.SocketErrorCode is SocketError.WouldBlock or
					SocketError.IOPending or
					SocketError.NoBufferSpaceAvailable)
				{
					Thread.Sleep(30);
				}
				else
					throw new SshConnectionException("Connection lost", DisconnectReason.ConnectionLost);
			}
		}
	}

	private void WaitHandle(IAsyncResult ar)
	{
		if (!ar.AsyncWaitHandle.WaitOne(_inactivityTimeout))
			throw new SshConnectionException(string.Format("Socket operation has timed out after {0:F0} milliseconds.",
				_inactivityTimeout.TotalMilliseconds),
				DisconnectReason.ConnectionLost);
	}
	#endregion

	#region Message operations
	private Message ReceiveMessage()
	{
		var useAlg = _algorithms is not null;

		var blockSize = (byte)(useAlg ? Math.Max(8, _algorithms.ClientEncryption.BlockBytesSize) : 8);
		var firstBlock = SocketRead(blockSize);
		if (useAlg)
		{
			firstBlock = _algorithms.ClientEncryption.Transform(firstBlock);
		}

		var packetLength = firstBlock[0] << 24 | firstBlock[1] << 16 | firstBlock[2] << 8 | firstBlock[3];
		var paddingLength = firstBlock[4];
		var bytesToRead = packetLength - blockSize + 4;

		var followingBlocks = SocketRead(bytesToRead);
		if (useAlg)
		{
			followingBlocks = _algorithms.ClientEncryption.Transform(followingBlocks);
		}

		var fullPacket = firstBlock.Concat(followingBlocks).ToArray();
		var data = fullPacket.Skip(5).Take(packetLength - paddingLength).ToArray();
		if (useAlg)
		{
			var clientMac = SocketRead(_algorithms.ClientHmac.DigestLength);
			var mac = ComputeHmac(_algorithms.ClientHmac, fullPacket, _inboundPacketSequence);
			if (!clientMac.SequenceEqual(mac))
			{
				throw new SshConnectionException("Invalid MAC", DisconnectReason.MacError);
			}

			data = _algorithms.ClientCompression.Decompress(data);
		}

		var typeNumber = data[0];
		var implemented = _messagesMetadata.ContainsKey(typeNumber);
		var message = implemented
			? Activator.CreateInstance(_messagesMetadata[typeNumber]) as Message ?? throw new InvalidOperationException()
			: new UnknownMessage { SequenceNumber = _inboundPacketSequence, UnknownMessageType = typeNumber };

		if (implemented)
		{
			message.Load(data);
		}

		lock (_locker)
		{
			_inboundPacketSequence++;
			_inboundFlow += (uint)packetLength;
		}

		ConsiderReExchange();

		return message;
	}

	internal void SendMessage(Message message)
	{
		Contract.Requires(message != null);

		if (_exchangeContext != null
			&& message.MessageType > 4 && (message.MessageType < 20 || message.MessageType > 49))
		{
			_blockedMessages.Enqueue(message);
			return;
		}

		_hasBlockedMessagesWaitHandle.WaitOne();
		lock (_locker)
			SendMessageInternal(message);
	}

	private void SendMessageInternal(Message message)
	{
		var useAlg = _algorithms != null;

		var blockSize = (byte)(useAlg ? Math.Max(8, _algorithms.ServerEncryption.BlockBytesSize) : 8);
		var payload = message.GetPacket();
		if (useAlg)
			payload = _algorithms.ServerCompression.Compress(payload);

		// http://tools.ietf.org/html/rfc4253
		// 6.  Binary Packet Protocol
		// the total length of (packet_length || padding_length || payload || padding)
		// is a multiple of the cipher block size or 8,
		// padding length must between 4 and 255 bytes.
		var paddingLength = (byte)(blockSize - (payload.Length + 5) % blockSize);
		if (paddingLength < 4)
			paddingLength += blockSize;

		var packetLength = (uint)payload.Length + paddingLength + 1;

		var padding = new byte[paddingLength];
		RandomNumberGenerator.Fill(padding);

		using (var worker = new SshDataWorker())
		{
			worker.Write(packetLength);
			worker.Write(paddingLength);
			worker.Write(payload);
			worker.Write(padding);

			payload = worker.ToByteArray();
		}

		if (useAlg)
		{
			var mac = ComputeHmac(_algorithms.ServerHmac, payload, _outboundPacketSequence);
			payload = [.. _algorithms.ServerEncryption.Transform(payload), .. mac];
		}

		SocketWrite(payload);

		lock (_locker)
		{
			_outboundPacketSequence++;
			_outboundFlow += packetLength;
		}

		ConsiderReExchange();
	}

	private void ConsiderReExchange(bool force = false)
	{
		var kex = false;
		lock (_locker)
			if (_exchangeContext == null
				&& (force || _inboundFlow + _outboundFlow > 1024 * 1024 * 512)) // 0.5 GiB
			{
				_exchangeContext = new ExchangeContext();
				kex = true;
			}

		if (kex)
		{
			var kexInitMessage = LoadKexInitMessage();
			_exchangeContext.ServerKexInitPayload = kexInitMessage.GetPacket();

			SendMessage(kexInitMessage);
		}
	}

	private void ContinueSendBlockedMessages()
	{
		if (!_blockedMessages.IsEmpty)
		{
			while (_blockedMessages.TryDequeue(out var message))
			{
				SendMessageInternal(message);
			}
		}
	}

	internal bool TrySendMessage(Message message)
	{
		Contract.Requires(message != null);

		try
		{
			SendMessage(message);
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static KeyExchangeInitMessage LoadKexInitMessage()
		=> new()
		{
			KeyExchangeAlgorithms = [.. _keyExchangeAlgorithms.Keys],
			ServerHostKeyAlgorithms = [.. _publicKeyAlgorithms.Keys],
			EncryptionAlgorithmsClientToServer = [.. _encryptionAlgorithms.Keys],
			EncryptionAlgorithmsServerToClient = [.. _encryptionAlgorithms.Keys],
			MacAlgorithmsClientToServer = [.. _hmacAlgorithms.Keys],
			MacAlgorithmsServerToClient = [.. _hmacAlgorithms.Keys],
			CompressionAlgorithmsClientToServer = [.. _compressionAlgorithms.Keys],
			CompressionAlgorithmsServerToClient = [.. _compressionAlgorithms.Keys],
			LanguagesClientToServer = [""],
			LanguagesServerToClient = [""],
			FirstKexPacketFollows = false,
			Reserved = 0
		};
	#endregion

	#region Handle messages
	private void HandleMessageCore(Message message)
	{
		switch (message)
		{
			case DisconnectMessage disconnectMessage:
				HandleMessage(disconnectMessage);
				break;
			case KeyExchangeInitMessage keyExchangeInitMessage:
				HandleMessage(keyExchangeInitMessage);
				break;
			case KeyExchangeDhInitMessage keyExchangeDhInitMessage:
				HandleMessage(keyExchangeDhInitMessage);
				break;
			case NewKeysMessage newKeysMessage:
				HandleMessage(newKeysMessage);
				break;
			case UnimplementedMessage unimplementedMessage:
				HandleMessage(unimplementedMessage);
				break;
			case ServiceRequestMessage serviceRequestMessage:
				HandleMessage(serviceRequestMessage);
				break;
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

	private void HandleMessage(DisconnectMessage message)
		=> Disconnect(message.ReasonCode, message.Description);

	private void HandleMessage(KeyExchangeInitMessage message)
	{
		ConsiderReExchange(true);

		KeysExchanged?.Invoke(this, new KeyExchangeArgs(this)
		{
			CompressionAlgorithmsClientToServer = message.CompressionAlgorithmsClientToServer,
			CompressionAlgorithmsServerToClient = message.CompressionAlgorithmsServerToClient,
			EncryptionAlgorithmsClientToServer = message.EncryptionAlgorithmsClientToServer,
			EncryptionAlgorithmsServerToClient = message.EncryptionAlgorithmsServerToClient,
			KeyExchangeAlgorithms = message.KeyExchangeAlgorithms,
			LanguagesClientToServer = message.LanguagesClientToServer,
			LanguagesServerToClient = message.LanguagesServerToClient,
			MacAlgorithmsClientToServer = message.MacAlgorithmsClientToServer,
			MacAlgorithmsServerToClient = message.MacAlgorithmsServerToClient,
			ServerHostKeyAlgorithms = message.ServerHostKeyAlgorithms
		});

		_exchangeContext.KeyExchange = ChooseAlgorithm([.. _keyExchangeAlgorithms.Keys], message.KeyExchangeAlgorithms);
		_exchangeContext.PublicKey = ChooseAlgorithm([.. _publicKeyAlgorithms.Keys], message.ServerHostKeyAlgorithms);
		_exchangeContext.ClientEncryption = ChooseAlgorithm([.. _encryptionAlgorithms.Keys], message.EncryptionAlgorithmsClientToServer);
		_exchangeContext.ServerEncryption = ChooseAlgorithm([.. _encryptionAlgorithms.Keys], message.EncryptionAlgorithmsServerToClient);
		_exchangeContext.ClientHmac = ChooseAlgorithm([.. _hmacAlgorithms.Keys], message.MacAlgorithmsClientToServer);
		_exchangeContext.ServerHmac = ChooseAlgorithm([.. _hmacAlgorithms.Keys], message.MacAlgorithmsServerToClient);
		_exchangeContext.ClientCompression = ChooseAlgorithm([.. _compressionAlgorithms.Keys], message.CompressionAlgorithmsClientToServer);
		_exchangeContext.ServerCompression = ChooseAlgorithm([.. _compressionAlgorithms.Keys], message.CompressionAlgorithmsServerToClient);

		_exchangeContext.ClientKexInitPayload = message.GetPacket();
	}

	private void HandleMessage(KeyExchangeDhInitMessage message)
	{
		var kexAlg = _keyExchangeAlgorithms[_exchangeContext.KeyExchange]();
		var hostKeyAlg = _publicKeyAlgorithms[_exchangeContext.PublicKey](_hostKey[_exchangeContext.PublicKey].ToString());
		var clientCipher = _encryptionAlgorithms[_exchangeContext.ClientEncryption]();
		var serverCipher = _encryptionAlgorithms[_exchangeContext.ServerEncryption]();
		var serverHmac = _hmacAlgorithms[_exchangeContext.ServerHmac]();
		var clientHmac = _hmacAlgorithms[_exchangeContext.ClientHmac]();

		var clientExchangeValue = message.E;
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
			ClientCompression = _compressionAlgorithms[_exchangeContext.ClientCompression](),
			ServerCompression = _compressionAlgorithms[_exchangeContext.ServerCompression](),
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

	private void HandleMessage(NewKeysMessage _)
	{
		_hasBlockedMessagesWaitHandle.Reset();

		lock (_locker)
		{
			_inboundFlow = 0;
			_outboundFlow = 0;
			_algorithms = _exchangeContext.NewAlgorithms;
			_exchangeContext = null;
		}

		ContinueSendBlockedMessages();
		_hasBlockedMessagesWaitHandle.Set();
	}

	private static void HandleMessage(UnimplementedMessage _) { }

	private void HandleMessage(ServiceRequestMessage message)
	{
		var service = RegisterService(message.ServiceName);
		if (service != null)
		{
			SendMessage(new ServiceAcceptMessage(message.ServiceName));
			return;
		}

		throw new SshConnectionException(string.Format("Service \"{0}\" not available.", message.ServiceName),
			DisconnectReason.ServiceNotAvailable);
	}

	private void HandleMessage(UserAuthServiceMessage message)
		=> GetService<UserAuthService>()?.HandleMessageCore(message);

	private void HandleMessage(ConnectionServiceMessage message)
		=> GetService<ConnectionService>()?.HandleMessageCore(message);
	#endregion

	private static string ChooseAlgorithm(string[] serverAlgorithms, string[] clientAlgorithms)
	{
		foreach (var client in clientAlgorithms)
		{
			foreach (var server in serverAlgorithms)
			{
				if (client == server)
				{
					return client;
				}
			}
		}

		throw new SshConnectionException("Failed to negotiate algorithm.", DisconnectReason.KeyExchangeFailed);
	}

	private byte[] ComputeExchangeHash(KexAlgorithm kexAlg, byte[] hostKeyAndCerts, byte[] clientExchangeValue, byte[] serverExchangeValue, byte[] sharedSecret)
	{
		using var worker = new SshDataWorker();
		worker.Write(ClientVersion, Encoding.ASCII);
		worker.Write(ServerVersion, Encoding.ASCII);
		worker.WriteBinary(_exchangeContext.ClientKexInitPayload);
		worker.WriteBinary(_exchangeContext.ServerKexInitPayload);
		worker.WriteBinary(hostKeyAndCerts);
		worker.WriteMpint(clientExchangeValue);
		worker.WriteMpint(serverExchangeValue);
		worker.WriteMpint(sharedSecret);

		return kexAlg.ComputeHash(worker.ToByteArray());
	}

	private byte[] ComputeEncryptionKey(
		KexAlgorithm kexAlg,
		byte[] exchangeHash,
		int blockSize,
		byte[] sharedSecret,
		char letter)
	{
		var keyBuffer = new byte[blockSize];
		var keyBufferIndex = 0;
		byte[]? currentHash = null;

		while (keyBufferIndex < blockSize)
		{
			using (var worker = new SshDataWorker())
			{
				worker.WriteMpint(sharedSecret);
				worker.Write(exchangeHash);

				if (currentHash == null)
				{
					worker.Write((byte)letter);
					worker.Write(ExchangeHash);
				}
				else
				{
					worker.Write(currentHash);
				}

				currentHash = kexAlg.ComputeHash(worker.ToByteArray());
			}

			var currentHashLength = Math.Min(currentHash.Length, blockSize - keyBufferIndex);
			Array.Copy(currentHash, 0, keyBuffer, keyBufferIndex, currentHashLength);

			keyBufferIndex += currentHashLength;
		}

		return keyBuffer;
	}

	private static byte[] ComputeHmac(HmacAlgorithm alg, byte[] payload, uint seq)
	{
		using var worker = new SshDataWorker();
		worker.Write(seq);
		worker.Write(payload);

		return alg.ComputeHash(worker.ToByteArray());
	}

	internal SshService? RegisterService(string serviceName, UserAuthArgs? auth = null)
	{
		SshService? service = null;
		switch (serviceName)
		{
			case "ssh-userauth":
				if (GetService<UserAuthService>() == null)
					service = new UserAuthService(this);
				break;
			case "ssh-connection":
				if (auth != null && GetService<ConnectionService>() == null)
					service = new ConnectionService(this, auth);
				break;
		}

		if (service is not null)
		{
			ServiceRegistered?.Invoke(this, service);
			_services.Add(service);
		}

		return service;
	}

	private class Algorithms
	{
		public KexAlgorithm KeyExchange;
		public PublicKeyAlgorithm PublicKey;
		public EncryptionAlgorithm ClientEncryption;
		public EncryptionAlgorithm ServerEncryption;
		public HmacAlgorithm ClientHmac;
		public HmacAlgorithm ServerHmac;
		public CompressionAlgorithm ClientCompression;
		public CompressionAlgorithm ServerCompression;
	}

	private class ExchangeContext
	{
		public string? KeyExchange;
		public string? PublicKey;
		public string? ClientEncryption;
		public string? ServerEncryption;
		public string? ClientHmac;
		public string? ServerHmac;
		public string? ClientCompression;
		public string? ServerCompression;

		public byte[]? ClientKexInitPayload;
		public byte[]? ServerKexInitPayload;

		public Algorithms? NewAlgorithms;
	}

	[GeneratedRegex("SSH-2.0-.+")]
	private static partial Regex SshVersionRegex();

	public bool TryGetSessionVariable<T>(string name, out T value)
	{
		if (_sessionVariables.TryGetValue(name, out var obj) && obj is T t)
		{
			value = t;
			return true;
		}

		value = default!;

		return false;
	}

	public void SetSessionVariable<T>(string name, T value)
		=> _sessionVariables[name] = value;
}
