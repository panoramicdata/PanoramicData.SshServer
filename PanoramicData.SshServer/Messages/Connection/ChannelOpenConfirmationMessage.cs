namespace PanoramicData.SshServer.Messages.Connection;

/// <summary>
/// Represents an SSH channel open confirmation message.
/// </summary>
[Message("SSH_MSG_CHANNEL_OPEN_CONFIRMATION", MessageNumber)]
public class ChannelOpenConfirmationMessage : ConnectionServiceMessage
{
	private const byte MessageNumber = 91;

	/// <summary>
	/// Gets or sets the recipient channel ID.
	/// </summary>
	public uint RecipientChannel { get; set; }

	/// <summary>
	/// Gets or sets the sender channel ID.
	/// </summary>
	public uint SenderChannel { get; set; }

	/// <summary>
	/// Gets or sets the initial window size.
	/// </summary>
	public uint InitialWindowSize { get; set; }

	/// <summary>
	/// Gets or sets the maximum packet size.
	/// </summary>
	public uint MaximumPacketSize { get; set; }

	/// <inheritdoc />
	public override byte MessageType => MessageNumber;

	/// <inheritdoc />
	protected override void OnGetPacket(SshDataWorker writer)
	{
		writer.Write(RecipientChannel);
		writer.Write(SenderChannel);
		writer.Write(InitialWindowSize);
		writer.Write(MaximumPacketSize);
	}
}
