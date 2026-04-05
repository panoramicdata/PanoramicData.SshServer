namespace PanoramicData.SshServer.Messages.Userauth;

/// <summary>
/// Represents an SSH user authentication success message.
/// </summary>
[Message("SSH_MSG_USERAUTH_SUCCESS", MessageNumber)]
public class SuccessMessage : UserAuthServiceMessage
{
	private const byte MessageNumber = 52;

	/// <inheritdoc />
	public override byte MessageType => MessageNumber;

	/// <inheritdoc />
	protected override void OnGetPacket(SshDataWorker writer)
	{
	}
}
