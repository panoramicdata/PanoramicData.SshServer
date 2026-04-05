namespace PanoramicData.SshServer.Messages;

/// <summary>
/// Represents an SSH key exchange DH reply message.
/// </summary>
[Message("SSH_MSG_KEXDH_REPLY", MessageNumber)]
public class KeyExchangeDhReplyMessage : Message
{
	private const byte MessageNumber = 31;

	/// <summary>
	/// Gets or sets the host key.
	/// </summary>
	public byte[]? HostKey { get; set; }

	/// <summary>
	/// Gets or sets the server exchange value.
	/// </summary>
	public byte[]? F { get; set; }

	/// <summary>
	/// Gets or sets the signature.
	/// </summary>
	public byte[]? Signature { get; set; }

	/// <inheritdoc />
	public override byte MessageType => MessageNumber;

	/// <inheritdoc />
	protected override void OnGetPacket(SshDataWorker writer)
	{
		writer.WriteBinary(HostKey!);
		writer.WriteMpint(F!);
		writer.WriteBinary(Signature!);
	}
}
