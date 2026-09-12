using PanoramicData.SshServer.Algorithms;
using PanoramicData.SshServer.Messages;
using PanoramicData.SshServer.Messages.Userauth;
using System;

namespace PanoramicData.SshServer.Services;

/// <summary>
/// Handles SSH user authentication service messages.
/// </summary>
/// <param name="session">The SSH session.</param>
public class UserAuthService(Session session) : SshService(session)
{
	/// <summary>
	/// Occurs when user authentication is requested.
	/// </summary>
	public event EventHandler<UserAuthArgs>? UserAuth;

	/// <summary>
	/// Occurs when authentication succeeds.
	/// </summary>
	public event EventHandler<string>? Succeed;

	/// <inheritdoc />
	protected internal override void CloseService()
	{
		// Nothing to release: this service owns no channels, sockets or background loops.
	}

	internal void HandleMessageCore(UserAuthServiceMessage message)
	{
		switch (message)
		{
			case PublicKeyRequestMessage publicKeyRequestMessage:
				HandleMessage(publicKeyRequestMessage);
				break;
			case PasswordRequestMessage passwordRequestMessage:
				HandleMessage(passwordRequestMessage);
				break;
			case RequestMessage requestMessage:
				HandleMessage(requestMessage);
				break;
			default:
				_session.SendMessage(new FailureMessage());
				break;
		}
	}

	private void HandleMessage(RequestMessage message)
	{
		switch (message.MethodName)
		{
			case "publickey":
				var keyMsg = Message.LoadFrom<PublicKeyRequestMessage>(message);
				HandleMessage(keyMsg);
				break;
			case "password":
				var pswdMsg = Message.LoadFrom<PasswordRequestMessage>(message);
				HandleMessage(pswdMsg);
				break;
			default:
				_session.SendMessage(new FailureMessage());
				break;
		}
	}

	private void HandleMessage(PasswordRequestMessage message)
	{
		var verifed = false;
		if (message.Username is null || message.Password is null || message.ServiceName is null)
		{
			_session.SendMessage(new FailureMessage());
			return;
		}

		var args = new UserAuthArgs(_session, message.Username, message.Password);
		if (UserAuth != null)
		{
			UserAuth(this, args);
			verifed = args.Result;
		}

		if (verifed)
		{
			Accept(message.ServiceName, args);
		}
		else
		{
			_session.SendMessage(new FailureMessage());
		}
	}

	/// <summary>
	/// Registers the service the client authenticated for and tells it authentication succeeded.
	/// </summary>
	private void Accept(string serviceName, UserAuthArgs args)
	{
		_session.RegisterService(serviceName, args);

		Succeed?.Invoke(this, serviceName);

		_session.SendMessage(new SuccessMessage());
	}

	private void HandleMessage(PublicKeyRequestMessage message)
	{
		var keyAlg = LoadRequestKey(message);
		if (keyAlg is null)
		{
			_session.SendMessage(new FailureMessage());
			return;
		}

		var args = new UserAuthArgs(_session, message.Username!, message.KeyAlgorithmName!, keyAlg.GetFingerprint(), message.PublicKey!);
		UserAuth?.Invoke(this, args);

		if (!args.Result)
		{
			_session.SendMessage(new FailureMessage());
			return;
		}

		if (!message.HasSignature)
		{
			// The client is only asking whether this key would be accepted, so say so and
			// wait for it to send the same request again, signed this time (RFC 4252 section 7).
			_session.SendMessage(new PublicKeyOkMessage { KeyAlgorithmName = message.KeyAlgorithmName, PublicKey = message.PublicKey });
			return;
		}

		if (!VerifySignature(message, keyAlg))
		{
			_session.SendMessage(new FailureMessage());
			return;
		}

		Accept(message.ServiceName!, args);
	}

	/// <summary>
	/// Loads the public key a request carries.
	/// </summary>
	/// <returns>
	/// The loaded key, or null if the request is missing a field it needs or names a key
	/// algorithm this server does not support.
	/// </returns>
	private static PublicKeyAlgorithm? LoadRequestKey(PublicKeyRequestMessage message)
	{
		if (message.KeyAlgorithmName is null
			|| message.PublicKey is null
			|| message.Username is null
			|| message.ServiceName is null
			|| !Session._publicKeyAlgorithms.TryGetValue(message.KeyAlgorithmName, out var value))
		{
			return null;
		}

		var keyAlg = value(null);
		keyAlg.LoadKeyAndCertificatesData(message.PublicKey);

		return keyAlg;
	}

	/// <summary>
	/// Checks the signature the client sent over the session's exchange hash and its own payload.
	/// </summary>
	/// <returns>True if the signature is present and valid.</returns>
	private bool VerifySignature(PublicKeyRequestMessage message, PublicKeyAlgorithm keyAlg)
	{
		if (message.Signature is null || message.PayloadWithoutSignature is null || _session.ExchangeHash is null)
		{
			return false;
		}

		var sig = keyAlg.GetSignature(message.Signature);

		using var worker = new SshDataWorker();
		worker.WriteBinary(_session.ExchangeHash);
		worker.Write(message.PayloadWithoutSignature);

		return keyAlg.VerifyData(worker.ToByteArray(), sig);
	}
}
