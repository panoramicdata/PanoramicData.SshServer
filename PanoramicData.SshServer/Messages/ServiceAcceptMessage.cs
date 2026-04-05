using System.Text;

namespace PanoramicData.SshServer.Messages;

/// <summary>
/// Represents an SSH service accept message.
/// </summary>
/// <param name="name">The accepted service name.</param>
[Message("SSH_MSG_SERVICE_ACCEPT", MessageNumber)]
public class ServiceAcceptMessage(string name) : Message
{
	private const byte MessageNumber = 6;

	/// <summary>
	/// Gets the accepted service name.
	/// </summary>
	public string ServiceName { get; private set; } = name;

	/// <inheritdoc />
	public override byte MessageType => MessageNumber;

	/// <inheritdoc />
	protected override void OnGetPacket(SshDataWorker writer) => writer.Write(ServiceName, Encoding.ASCII);
}
