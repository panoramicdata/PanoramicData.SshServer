namespace PanoramicData.SshServer.Messages.Connection;

/// <summary>
/// Represents an SSH channel success message.
/// </summary>
[Message("SSH_MSG_CHANNEL_SUCCESS", MessageNumber)]
public class ChannelSuccessMessage : ChannelMessage
{
	private const byte MessageNumber = 99;

	/// <inheritdoc />
	public override byte MessageType => MessageNumber;
}
