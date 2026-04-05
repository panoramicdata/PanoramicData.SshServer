namespace PanoramicData.SshServer.Messages;

/// <summary>
/// Represents an SSH unimplemented message.
/// </summary>
[Message("SSH_MSG_UNIMPLEMENTED", MessageNumber)]
public class UnimplementedMessage : Message
{
	private const byte MessageNumber = 3;

	/// <summary>
	/// Gets or sets the sequence number.
	/// </summary>
	public uint SequenceNumber { get; set; }

	/// <inheritdoc />
	public override byte MessageType => MessageNumber;

	/// <inheritdoc />
	protected override void OnLoad(SshDataWorker reader) => SequenceNumber = reader.ReadUInt32();

	/// <inheritdoc />
	protected override void OnGetPacket(SshDataWorker writer) => writer.Write(SequenceNumber);
}
