using System.Text;

namespace PanoramicData.SshServer.Messages.Userauth;

/// <summary>
/// Represents an SSH user authentication failure message.
/// </summary>
[Message("SSH_MSG_USERAUTH_FAILURE", MessageNumber)]
public class FailureMessage : UserAuthServiceMessage
{
	private const byte MessageNumber = 51;

	/// <inheritdoc />
	public override byte MessageType => MessageNumber;

	/// <inheritdoc />
	protected override void OnGetPacket(SshDataWorker writer)
	{
		writer.Write("publickey,password", Encoding.ASCII);
		writer.Write(false);
	}
}
