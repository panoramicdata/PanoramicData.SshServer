namespace PanoramicData.SshServer.Messages.Connection;

/// <summary>
/// Represents an SSH channel failure message.
/// </summary>
[Message("SSH_MSG_CHANNEL_FAILURE", MessageNumber)]
public class ChannelFailureMessage : ChannelMessage
{
	private const byte MessageNumber = 100;

	/// <inheritdoc />
	public override byte MessageType => MessageNumber;
}
