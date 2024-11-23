using PanoramicData.SshServer;
using System.Diagnostics.Contracts;

namespace PanoramicData.SshServer.Services;

public abstract class SshService
{
	protected internal readonly Session _session;

	public SshService(Session session)
	{
		Contract.Requires(session != null);

		_session = session;
	}

	internal protected abstract void CloseService();
}
