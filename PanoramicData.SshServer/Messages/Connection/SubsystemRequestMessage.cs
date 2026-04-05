using System.Text;

namespace PanoramicData.SshServer.Messages.Connection;

/// <summary>
/// Represents an SSH subsystem request message.
/// </summary>
public class SubsystemRequestMessage : ChannelRequestMessage
{
	/// <summary>
	/// Gets the subsystem name.
	/// </summary>
	public string? Name { get; private set; }

	/// <inheritdoc />
	protected override void OnLoad(SshDataWorker reader)
	{
		base.OnLoad(reader);

		Name = reader.ReadString(Encoding.ASCII);
	}
}
