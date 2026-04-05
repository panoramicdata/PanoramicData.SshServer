namespace PanoramicData.SshServer.Messages.Connection;

/// <summary>
/// Represents an SSH channel window adjust message.
/// </summary>
[Message("SSH_MSG_CHANNEL_WINDOW_ADJUST", MessageNumber)]
public class ChannelWindowAdjustMessage : ConnectionServiceMessage
{
	private const byte MessageNumber = 93;

	/// <summary>
	/// Gets or sets the recipient channel ID.
	/// </summary>
	public uint RecipientChannel { get; set; }

	/// <summary>
	/// Gets or sets the number of bytes to add to the window.
	/// </summary>
	public uint BytesToAdd { get; set; }

	/// <inheritdoc />
	public override byte MessageType => MessageNumber;

	/// <inheritdoc />
	protected override void OnLoad(SshDataWorker reader)
	{
		RecipientChannel = reader.ReadUInt32();
		BytesToAdd = reader.ReadUInt32();
	}

	/// <inheritdoc />
	protected override void OnGetPacket(SshDataWorker writer)
	{
		writer.Write(RecipientChannel);
		writer.Write(BytesToAdd);
	}
}
