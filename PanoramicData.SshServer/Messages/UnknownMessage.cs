using System;

namespace PanoramicData.SshServer.Messages;

public class UnknownMessage : Message
{
	public uint SequenceNumber { get; set; }

	public byte UnknownMessageType { get; set; }

	public override byte MessageType => throw new NotSupportedException();

	public UnimplementedMessage MakeUnimplementedMessage() => new UnimplementedMessage()
	{
		SequenceNumber = SequenceNumber
	};
}
