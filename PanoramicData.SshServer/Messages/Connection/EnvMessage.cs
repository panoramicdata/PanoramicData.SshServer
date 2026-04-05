using System.Text;

namespace PanoramicData.SshServer.Messages.Connection;

/// <summary>
/// Represents an SSH environment variable message.
/// </summary>
public class EnvMessage : ChannelRequestMessage
{
	/// <summary>
	/// Gets the environment variable name.
	/// </summary>
	public string? Name { get; private set; }

	/// <summary>
	/// Gets the environment variable value.
	/// </summary>
	public string? Value { get; private set; }

	/// <inheritdoc />
	protected override void OnLoad(SshDataWorker reader)
	{
		base.OnLoad(reader);

		Name = reader.ReadString(Encoding.ASCII);
		Value = reader.ReadString(Encoding.ASCII);
	}
}
