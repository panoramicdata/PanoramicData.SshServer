using System;

namespace PanoramicData.SshServer;

/// <summary>
/// Represents an SSH connection exception.
/// </summary>
public class SshConnectionException : Exception
{
	/// <summary>
	/// Initializes a new instance of the <see cref="SshConnectionException"/> class.
	/// </summary>
	public SshConnectionException()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SshConnectionException"/> class with a message.
	/// </summary>
	/// <param name="message">The exception message.</param>
	public SshConnectionException(string message)
		: this(message, DisconnectReason.None)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SshConnectionException"/> class with a message and an inner exception.
	/// </summary>
	/// <param name="message">The exception message.</param>
	/// <param name="innerException">The exception that caused this one.</param>
	public SshConnectionException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SshConnectionException"/> class with a message and a disconnect reason.
	/// </summary>
	/// <param name="message">The exception message.</param>
	/// <param name="disconnectReason">The disconnect reason.</param>
	public SshConnectionException(string message, DisconnectReason disconnectReason)
		: base(message)
	{
		DisconnectReason = disconnectReason;
	}

	/// <summary>
	/// Gets the disconnect reason.
	/// </summary>
	public DisconnectReason DisconnectReason { get; private set; }

	/// <inheritdoc />
	public override string ToString() => string.Format("SSH connection disconnected because {0}", DisconnectReason);
}
