namespace PanoramicData.SshServer.Messages;

/// <summary>
/// Represents an SSH key exchange DH init message.
/// </summary>
[Message("SSH_MSG_KEXDH_INIT", MessageNumber)]
public class KeyExchangeDhInitMessage : Message
{
	private const byte MessageNumber = 30;

	/// <summary>
	/// Gets the client exchange value.
	/// </summary>
	public byte[]? E { get; private set; }

	/// <inheritdoc />
	public override byte MessageType => MessageNumber;

	/// <inheritdoc />
	protected override void OnLoad(SshDataWorker reader) => E = reader.ReadMpint();
}
