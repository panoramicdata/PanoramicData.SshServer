using System.Text;

namespace PanoramicData.SshServer.Messages.Connection;

/// <summary>
/// Represents an SSH command request message.
/// </summary>
public class CommandRequestMessage : ChannelRequestMessage
{
	/// <summary>
	/// Gets the command string.
	/// </summary>
	public string? Command { get; private set; }

	/// <inheritdoc />
	protected override void OnLoad(SshDataWorker reader)
	{
		base.OnLoad(reader);

		Command = reader.ReadString(Encoding.ASCII);
	}
}
