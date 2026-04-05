using System.Text;

namespace PanoramicData.SshServer.Messages.Connection;

/// <summary>
/// Represents an SSH channel open failure message.
/// </summary>
[Message("SSH_MSG_CHANNEL_OPEN_FAILURE", MessageNumber)]
public class ChannelOpenFailureMessage : ConnectionServiceMessage
{
	private const byte MessageNumber = 92;

	/// <summary>
	/// Gets or sets the recipient channel ID.
	/// </summary>
	public uint RecipientChannel { get; set; }

	/// <summary>
	/// Gets or sets the failure reason code.
	/// </summary>
	public ChannelOpenFailureReason ReasonCode { get; set; }

	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Gets or sets the language tag.
	/// </summary>
	public string? Language { get; set; }

	/// <inheritdoc />
	public override byte MessageType => MessageNumber;

	/// <inheritdoc />
	protected override void OnGetPacket(SshDataWorker writer)
	{
		writer.Write(RecipientChannel);
		writer.Write((uint)ReasonCode);
		writer.Write(Description ?? string.Empty, Encoding.ASCII);
		writer.Write(Language ?? "en", Encoding.ASCII);
	}
}
