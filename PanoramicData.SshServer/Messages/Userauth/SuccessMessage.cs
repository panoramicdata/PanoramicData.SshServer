namespace PanoramicData.SshServer.Messages.Userauth;

[Message("SSH_MSG_USERAUTH_SUCCESS", MessageNumber)]
public class SuccessMessage : UserauthServiceMessage
{
	private const byte MessageNumber = 52;

	public override byte MessageType => MessageNumber;

	protected override void OnGetPacket(SshDataWorker writer)
	{
	}
}
