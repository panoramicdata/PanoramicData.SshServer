using System.Text;

namespace PanoramicData.SshServer.Messages.Connection;

/// <summary>
/// Represents an SSH channel request message.
/// </summary>
[Message("SSH_MSG_CHANNEL_REQUEST", MessageNumber)]
public class ChannelRequestMessage : ConnectionServiceMessage
{
	private const byte MessageNumber = 98;

	/// <summary>
	/// Gets or sets the recipient channel ID.
	/// </summary>
	public uint RecipientChannel { get; set; }

	/// <summary>
	/// Gets or sets the request type.
	/// </summary>
	public string? RequestType { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether a reply is wanted.
	/// </summary>
	public bool WantReply { get; set; }

	/// <inheritdoc />
	public override byte MessageType => MessageNumber;

	/// <inheritdoc />
	protected override void OnLoad(SshDataWorker reader)
	{
		RecipientChannel = reader.ReadUInt32();
		RequestType = reader.ReadString(Encoding.ASCII);
		WantReply = reader.ReadBoolean();
	}

	/// <inheritdoc />
	protected override void OnGetPacket(SshDataWorker writer)
	{
		writer.Write(RecipientChannel);
		writer.Write(RequestType ?? string.Empty, Encoding.ASCII);
		writer.Write(WantReply);
	}
}
