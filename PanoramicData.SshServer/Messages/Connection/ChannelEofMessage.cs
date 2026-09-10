namespace PanoramicData.SshServer.Messages.Connection;

/// <summary>
/// Represents an SSH channel EOF message.
/// </summary>
[Message("SSH_MSG_CHANNEL_EOF", MessageNumber)]
public class ChannelEofMessage : ChannelMessage
{
	private const byte MessageNumber = 96;

	/// <inheritdoc />
	public override byte MessageType => MessageNumber;
}
