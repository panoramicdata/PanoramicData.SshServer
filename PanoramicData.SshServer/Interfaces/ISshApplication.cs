namespace PanoramicData.SshServer.Interfaces;

public interface ISshApplication
{
	void SshServerSessionEnd(object sender, Session e);

	void SshServerSessionStart(object sender, Session e);
}
