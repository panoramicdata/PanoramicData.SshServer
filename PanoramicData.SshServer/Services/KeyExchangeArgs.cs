using PanoramicData.SshServer.Messages;
using System;

namespace PanoramicData.SshServer.Services;

/// <summary>
/// Contains arguments for a key exchange event.
/// </summary>
public class KeyExchangeArgs : KeyExchangeInitMessage
{
	/// <summary>
	/// Initializes a new instance of the <see cref="KeyExchangeArgs"/> class.
	/// </summary>
	/// <param name="s">The SSH session.</param>
	public KeyExchangeArgs(Session s)
	{
		Session = s;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="KeyExchangeArgs"/> class from the
	/// algorithm lists a client offered in its SSH_MSG_KEXINIT.
	/// </summary>
	/// <param name="s">The SSH session.</param>
	/// <param name="message">The key exchange init message the client sent.</param>
	public KeyExchangeArgs(Session s, KeyExchangeInitMessage message)
		: this(s)
	{
		ArgumentNullException.ThrowIfNull(message);

		Cookie = message.Cookie;
		KeyExchangeAlgorithms = message.KeyExchangeAlgorithms;
		ServerHostKeyAlgorithms = message.ServerHostKeyAlgorithms;
		EncryptionAlgorithmsClientToServer = message.EncryptionAlgorithmsClientToServer;
		EncryptionAlgorithmsServerToClient = message.EncryptionAlgorithmsServerToClient;
		MacAlgorithmsClientToServer = message.MacAlgorithmsClientToServer;
		MacAlgorithmsServerToClient = message.MacAlgorithmsServerToClient;
		CompressionAlgorithmsClientToServer = message.CompressionAlgorithmsClientToServer;
		CompressionAlgorithmsServerToClient = message.CompressionAlgorithmsServerToClient;
		LanguagesClientToServer = message.LanguagesClientToServer;
		LanguagesServerToClient = message.LanguagesServerToClient;
	}

	/// <summary>
	/// Gets the SSH session.
	/// </summary>
	public Session Session { get; private set; }

	/// <inheritdoc />
	public new byte[]? Cookie { get; private set; }

	/// <inheritdoc />
	public new string[]? KeyExchangeAlgorithms { get; set; }

	/// <inheritdoc />
	public new string[]? ServerHostKeyAlgorithms { get; set; }

	/// <inheritdoc />
	public new string[]? EncryptionAlgorithmsClientToServer { get; set; }

	/// <inheritdoc />
	public new string[]? EncryptionAlgorithmsServerToClient { get; set; }

	/// <inheritdoc />
	public new string[]? MacAlgorithmsClientToServer { get; set; }

	/// <inheritdoc />
	public new string[]? MacAlgorithmsServerToClient { get; set; }

	/// <inheritdoc />
	public new string[]? CompressionAlgorithmsClientToServer { get; set; }

	/// <inheritdoc />
	public new string[]? CompressionAlgorithmsServerToClient { get; set; }

	/// <inheritdoc />
	public new string[]? LanguagesClientToServer { get; set; }

	/// <inheritdoc />
	public new string[]? LanguagesServerToClient { get; set; }
}
