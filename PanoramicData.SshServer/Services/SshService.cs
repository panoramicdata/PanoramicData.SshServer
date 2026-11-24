using System;

namespace PanoramicData.SshServer.Services;

/// <summary>
/// Base class for SSH services.
/// </summary>
public abstract class SshService
{
	/// <summary>
	/// Initializes a new instance of the <see cref="SshService"/> class.
	/// </summary>
	/// <param name="session">The SSH session.</param>
	protected SshService(Session session)
	{
		_session = session ?? throw new ArgumentNullException(nameof(session));
		SessionId = session.Id;
	}

	/// <summary>
	/// The SSH session.
	/// </summary>
	protected internal readonly Session _session;

	/// <summary>
	/// Gets the session identifier.
	/// </summary>
	public Guid SessionId { get; }

	/// <summary>
	/// Closes the service.
	/// </summary>
	internal protected abstract void CloseService();
}
