using System.Text;

namespace PanoramicData.SshServer.Messages;

[Message("SSH_MSG_SERVICE_REQUEST", MessageNumber)]
public class ServiceRequestMessage : Message
{
	private const byte MessageNumber = 5;

	public string ServiceName { get; private set; }

	public override byte MessageType => MessageNumber;

	protected override void OnLoad(SshDataWorker reader) => ServiceName = reader.ReadString(Encoding.ASCII);
}
