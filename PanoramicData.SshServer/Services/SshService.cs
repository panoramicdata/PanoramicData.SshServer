using System;

namespace PanoramicData.SshServer.Services;

public abstract class SshService(Session session)
{
	protected internal readonly Session _session = session ?? throw new ArgumentNullException(nameof(session));

	public Guid SessionId { get; } = session.Id;

	internal protected abstract void CloseService();
}
