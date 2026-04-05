using System.Text;

namespace PanoramicData.SshServer.Messages.Userauth;

/// <summary>
/// Represents an SSH public key OK message.
/// </summary>
[Message("SSH_MSG_USERAUTH_PK_OK", MessageNumber)]
public class PublicKeyOkMessage : UserAuthServiceMessage
{
	private const byte MessageNumber = 60;

	/// <summary>
	/// Gets or sets the key algorithm name.
	/// </summary>
	public string? KeyAlgorithmName { get; set; }

	/// <summary>
	/// Gets or sets the public key data.
	/// </summary>
	public byte[]? PublicKey { get; set; }

	/// <inheritdoc />
	public override byte MessageType => MessageNumber;

	/// <inheritdoc />
	protected override void OnGetPacket(SshDataWorker writer)
	{
		writer.Write(KeyAlgorithmName!, Encoding.ASCII);
		writer.WriteBinary(PublicKey!);
	}
}
