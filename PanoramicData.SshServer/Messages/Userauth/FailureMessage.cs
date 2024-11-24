using System.Text;

namespace PanoramicData.SshServer.Messages.Userauth;

[Message("SSH_MSG_USERAUTH_FAILURE", MessageNumber)]
public class FailureMessage : UserAuthServiceMessage
{
	private const byte MessageNumber = 51;

	public override byte MessageType => MessageNumber;

	protected override void OnGetPacket(SshDataWorker writer)
	{
		writer.Write("publickey,password", Encoding.ASCII);
		writer.Write(false);
	}
}
