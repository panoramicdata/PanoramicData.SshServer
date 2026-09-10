namespace PanoramicData.SshServer.Messages.Connection;

/// <summary>
/// Represents an SSH channel close message.
/// </summary>
[Message("SSH_MSG_CHANNEL_CLOSE", MessageNumber)]
public class ChannelCloseMessage : ChannelMessage
{
	private const byte MessageNumber = 97;

	/// <inheritdoc />
	public override byte MessageType => MessageNumber;
}
