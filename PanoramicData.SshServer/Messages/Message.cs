using System;

namespace PanoramicData.SshServer.Messages;

/// <summary>
/// Base class for all SSH messages.
/// </summary>
public abstract class Message
{	
	/// <summary>
	/// Gets the message type number.
	/// </summary>
	public abstract byte MessageType { get; }

	/// <summary>
	/// Gets or sets the raw bytes of the message.
	/// </summary>
	protected byte[]? RawBytes { get; set; }

	/// <summary>
	/// Loads the message from the specified bytes.
	/// </summary>
	/// <param name="bytes">The raw message bytes.</param>
	public void Load(byte[] bytes)
	{
		ArgumentNullException.ThrowIfNull(bytes);

		RawBytes = bytes;
		using var worker = new SshDataWorker(bytes);
		var number = worker.ReadByte();
		if (number != MessageType)
			throw new ArgumentException(string.Format("Message type {0} is not valid.", number));

		OnLoad(worker);
	}

	/// <summary>
	/// Gets the packet bytes for this message.
	/// </summary>
	/// <returns>The packet bytes.</returns>
	public byte[] GetPacket()
	{
		using var worker = new SshDataWorker();
		worker.Write(MessageType);

		OnGetPacket(worker);

		return worker.ToByteArray();
	}

	/// <summary>
	/// Loads a message of type <typeparamref name="T"/> from the specified message's raw bytes.
	/// </summary>
	/// <typeparam name="T">The message type to load.</typeparam>
	/// <param name="message">The source message.</param>
	/// <returns>The loaded message.</returns>
	public static T LoadFrom<T>(Message message) where T : Message, new()
	{
		ArgumentNullException.ThrowIfNull(message);

		var msg = new T();
		msg.Load(message.RawBytes!);
		return msg;
	}

	/// <summary>
	/// Called when the message is loaded from the reader.
	/// </summary>
	/// <param name="reader">The data reader.</param>
	protected virtual void OnLoad(SshDataWorker reader)
	{
		ArgumentNullException.ThrowIfNull(reader);

		throw new NotSupportedException();
	}

	/// <summary>
	/// Called when the message packet is being built.
	/// </summary>
	/// <param name="writer">The data writer.</param>
	protected virtual void OnGetPacket(SshDataWorker writer)
	{
		ArgumentNullException.ThrowIfNull(writer);

		throw new NotSupportedException();
	}
}
