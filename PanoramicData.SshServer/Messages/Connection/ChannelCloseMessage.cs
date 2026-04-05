namespace PanoramicData.SshServer.Messages.Connection;

/// <summary>
/// Represents an SSH channel close message.
/// </summary>
[Message("SSH_MSG_CHANNEL_CLOSE", MessageNumber)]
public class ChannelCloseMessage : ConnectionServiceMessage
{
	private const byte MessageNumber = 97;

	/// <summary>
	/// Gets or sets the recipient channel ID.
	/// </summary>
	public uint RecipientChannel { get; set; }

	/// <inheritdoc />
	public override byte MessageType => MessageNumber;

	/// <inheritdoc />
	protected override void OnLoad(SshDataWorker reader) => RecipientChannel = reader.ReadUInt32();

	/// <inheritdoc />
	protected override void OnGetPacket(SshDataWorker writer) => writer.Write(RecipientChannel);
}
