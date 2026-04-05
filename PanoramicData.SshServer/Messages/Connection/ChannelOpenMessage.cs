using System.Text;

namespace PanoramicData.SshServer.Messages.Connection;

/// <summary>
/// Represents an SSH channel open message.
/// </summary>
[Message("SSH_MSG_CHANNEL_OPEN", MessageNumber)]
public class ChannelOpenMessage : ConnectionServiceMessage
{
	private const byte MessageNumber = 90;

	/// <summary>
	/// Gets the channel type.
	/// </summary>
	public string? ChannelType { get; private set; }

	/// <summary>
	/// Gets the sender channel ID.
	/// </summary>
	public uint SenderChannel { get; private set; }

	/// <summary>
	/// Gets the initial window size.
	/// </summary>
	public uint InitialWindowSize { get; private set; }

	/// <summary>
	/// Gets the maximum packet size.
	/// </summary>
	public uint MaximumPacketSize { get; private set; }

	/// <inheritdoc />
	public override byte MessageType => MessageNumber;

	/// <inheritdoc />
	protected override void OnLoad(SshDataWorker reader)
	{
		ChannelType = reader.ReadString(Encoding.ASCII);
		SenderChannel = reader.ReadUInt32();
		InitialWindowSize = reader.ReadUInt32();
		MaximumPacketSize = reader.ReadUInt32();
	}
}
