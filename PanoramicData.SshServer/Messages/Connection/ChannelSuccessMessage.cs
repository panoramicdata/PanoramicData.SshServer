namespace PanoramicData.SshServer.Messages.Connection;

/// <summary>
/// Represents an SSH channel success message.
/// </summary>
[Message("SSH_MSG_CHANNEL_SUCCESS", MessageNumber)]
public class ChannelSuccessMessage : ConnectionServiceMessage
{
	private const byte MessageNumber = 99;

	/// <summary>
	/// Gets or sets the recipient channel ID.
	/// </summary>
	public uint RecipientChannel { get; set; }

	/// <inheritdoc />
	public override byte MessageType => MessageNumber;

	/// <inheritdoc />
	protected override void OnGetPacket(SshDataWorker writer) => writer.Write(RecipientChannel);
}
