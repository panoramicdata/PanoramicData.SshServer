namespace PanoramicData.SshServer.Messages.Connection;

/// <summary>
/// Base class for connection service messages addressed to a single channel.
/// </summary>
/// <remarks>
/// Every SSH_MSG_CHANNEL_* payload starts with the recipient channel id, so it is read
/// and written here rather than repeated in each message type.
/// </remarks>
public abstract class ChannelMessage : ConnectionServiceMessage
{
	/// <summary>
	/// Gets or sets the recipient channel ID.
	/// </summary>
	public uint RecipientChannel { get; set; }

	/// <inheritdoc />
	protected override void OnLoad(SshDataWorker reader)
	{
		ArgumentNullException.ThrowIfNull(reader);

		RecipientChannel = reader.ReadUInt32();
	}

	/// <inheritdoc />
	protected override void OnGetPacket(SshDataWorker writer)
	{
		ArgumentNullException.ThrowIfNull(writer);

		writer.Write(RecipientChannel);
	}
}
