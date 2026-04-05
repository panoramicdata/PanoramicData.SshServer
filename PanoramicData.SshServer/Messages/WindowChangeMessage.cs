using PanoramicData.SshServer.Messages.Connection;

namespace PanoramicData.SshServer.Messages;

/// <summary>
/// Represents an SSH window change message.
/// </summary>
public class WindowChangeMessage : ChannelRequestMessage
{
	/// <summary>
	/// Gets the width in columns.
	/// </summary>
	public uint WidthColumns { get; private set; }

	/// <summary>
	/// Gets the height in rows.
	/// </summary>
	public uint HeightRows { get; private set; }

	/// <summary>
	/// Gets the width in pixels.
	/// </summary>
	public uint WidthPixels { get; private set; }

	/// <summary>
	/// Gets the height in pixels.
	/// </summary>
	public uint HeightPixels { get; private set; }

	/// <inheritdoc />
	protected override void OnLoad(SshDataWorker reader)
	{
		base.OnLoad(reader);

		WidthColumns = reader.ReadUInt32();
		HeightRows = reader.ReadUInt32();
		WidthPixels = reader.ReadUInt32();
		HeightPixels = reader.ReadUInt32();
	}
}
