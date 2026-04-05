namespace PanoramicData.SshServer.Messages.Connection;

/// <summary>
/// Represents an SSH ignore message.
/// </summary>
[Message("SSH_MSG_IGNORE", MessageNumber)]
public class ShouldIgnoreMessage : ConnectionServiceMessage
{
	private const byte MessageNumber = 2;

	/// <inheritdoc />
	public override byte MessageType => MessageNumber;

	/// <inheritdoc />
	protected override void OnLoad(SshDataWorker reader)
	{
	}
}
