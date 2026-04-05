using System.Text;

namespace PanoramicData.SshServer.Messages;

/// <summary>
/// Represents an SSH service request message.
/// </summary>
[Message("SSH_MSG_SERVICE_REQUEST", MessageNumber)]
public class ServiceRequestMessage : Message
{
	private const byte MessageNumber = 5;

	/// <summary>
	/// Gets the requested service name.
	/// </summary>
	public string? ServiceName { get; private set; }

	/// <inheritdoc />
	public override byte MessageType => MessageNumber;

	/// <inheritdoc />
	protected override void OnLoad(SshDataWorker reader) => ServiceName = reader.ReadString(Encoding.ASCII);
}
