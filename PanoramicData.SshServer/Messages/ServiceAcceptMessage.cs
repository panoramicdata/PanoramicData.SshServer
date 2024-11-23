using System.Text;

namespace PanoramicData.SshServer.Messages;

[Message("SSH_MSG_SERVICE_ACCEPT", MessageNumber)]
public class ServiceAcceptMessage(string name) : Message
{
	private const byte MessageNumber = 6;

	public string ServiceName { get; private set; } = name;

	public override byte MessageType => MessageNumber;

	protected override void OnGetPacket(SshDataWorker writer) => writer.Write(ServiceName, Encoding.ASCII);
}
