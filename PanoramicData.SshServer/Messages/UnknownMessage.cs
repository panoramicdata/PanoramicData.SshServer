using System;

namespace PanoramicData.SshServer.Messages;

/// <summary>
/// Represents an unknown SSH message.
/// </summary>
public class UnknownMessage : Message
{
	/// <summary>
	/// Gets or sets the sequence number.
	/// </summary>
	public uint SequenceNumber { get; set; }

	/// <summary>
	/// Gets or sets the unknown message type number.
	/// </summary>
	public byte UnknownMessageType { get; set; }

	/// <inheritdoc />
	public override byte MessageType => throw new NotSupportedException();

	/// <summary>
	/// Creates an unimplemented message from this unknown message.
	/// </summary>
	/// <returns>An <see cref="UnimplementedMessage"/>.</returns>
	public UnimplementedMessage MakeUnimplementedMessage() => new()
	{
		SequenceNumber = SequenceNumber
	};
}
