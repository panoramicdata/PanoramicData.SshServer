using System.Text;

namespace PanoramicData.SshServer.Messages.Connection;

/// <summary>
/// Represents an SSH pseudo-terminal request message.
/// </summary>
public class PtyRequestMessage : ChannelRequestMessage
{
	/// <summary>
	/// Gets or sets the terminal type.
	/// </summary>
	public string Terminal { get; set; } = "";

	/// <summary>
	/// Gets or sets the width in characters.
	/// </summary>
	public uint widthChars { get; set; }

	/// <summary>
	/// Gets or sets the height in rows.
	/// </summary>
	public uint heightRows { get; set; }

	/// <summary>
	/// Gets or sets the width in pixels.
	/// </summary>
	public uint widthPx { get; set; }

	/// <summary>
	/// Gets or sets the height in pixels.
	/// </summary>
	public uint heightPx { get; set; }

	/// <summary>
	/// Gets or sets the terminal modes.
	/// </summary>
	public string modes { get; set; } = "";

	/// <inheritdoc />
	protected override void OnLoad(SshDataWorker reader)
	{
		base.OnLoad(reader);

		Terminal = reader.ReadString(Encoding.ASCII);
		widthChars = reader.ReadUInt32();
		heightRows = reader.ReadUInt32();
		widthPx = reader.ReadUInt32();
		heightPx = reader.ReadUInt32();
		modes = reader.ReadString(Encoding.ASCII);
	}
}
