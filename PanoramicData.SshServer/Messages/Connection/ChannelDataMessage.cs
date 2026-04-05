namespace PanoramicData.SshServer.Messages.Connection;

/// <summary>
/// Represents an SSH channel data message.
/// </summary>
[Message("SSH_MSG_CHANNEL_DATA", MessageNumber)]
public class ChannelDataMessage : ConnectionServiceMessage
{
	private const byte MessageNumber = 94;

	/// <summary>
	/// Gets or sets the recipient channel ID.
	/// </summary>
	public uint RecipientChannel { get; set; }

	/// <summary>
	/// Gets or sets the data payload.
	/// </summary>
	public byte[]? Data { get; set; }

	/// <inheritdoc />
	public override byte MessageType => MessageNumber;

	/// <inheritdoc />
	protected override void OnLoad(SshDataWorker reader)
	{
		RecipientChannel = reader.ReadUInt32();
		Data = reader.ReadBinary();
	}

	/// <inheritdoc />
	protected override void OnGetPacket(SshDataWorker writer)
	{
		writer.Write(RecipientChannel);
		writer.WriteBinary(Data!);
	}
}
