namespace PanoramicData.SshServer.Messages.Connection;

[Message("SSH_MSG_IGNORE", MessageNumber)]
public class ShouldIgnoreMessage : ConnectionServiceMessage
{
	private const byte MessageNumber = 2;

	public override byte MessageType => MessageNumber;

	protected override void OnLoad(SshDataWorker reader)
	{
	}
}
