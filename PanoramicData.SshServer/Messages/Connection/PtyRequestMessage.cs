using System.Text;

namespace PanoramicData.SshServer.Messages.Connection;

public class PtyRequestMessage : ChannelRequestMessage
{
	public string Terminal { get; set; } = "";
	public uint widthChars { get; set; }
	public uint heightRows { get; set; }
	public uint widthPx { get; set; }
	public uint heightPx { get; set; }
	public string modes { get; set; } = "";

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
