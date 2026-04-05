namespace PanoramicData.SshServer.Messages;

/// <summary>
/// Represents an SSH new keys message.
/// </summary>
[Message("SSH_MSG_NEWKEYS", MessageNumber)]
public class NewKeysMessage : Message
{
	private const byte MessageNumber = 21;

	/// <inheritdoc />
	public override byte MessageType => MessageNumber;

	/// <inheritdoc />
	protected override void OnLoad(SshDataWorker reader)
	{
	}

	/// <inheritdoc />
	protected override void OnGetPacket(SshDataWorker writer)
	{
	}
}
