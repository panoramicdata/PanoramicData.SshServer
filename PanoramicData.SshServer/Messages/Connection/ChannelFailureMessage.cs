namespace PanoramicData.SshServer.Messages.Connection;

/// <summary>
/// Represents an SSH channel failure message.
/// </summary>
[Message("SSH_MSG_CHANNEL_FAILURE", MessageNumber)]
public class ChannelFailureMessage : ConnectionServiceMessage
{
	private const byte MessageNumber = 100;

	/// <summary>
	/// Gets or sets the recipient channel ID.
	/// </summary>
	public uint RecipientChannel { get; set; }

	/// <inheritdoc />
	public override byte MessageType => MessageNumber;

	/// <inheritdoc />
	protected override void OnGetPacket(SshDataWorker writer) => writer.Write(RecipientChannel);
}
