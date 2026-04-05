using System;

namespace PanoramicData.SshServer.Services;

/// <summary>
/// Base class for SSH services.
/// </summary>
/// <param name="session">The SSH session.</param>
public abstract class SshService(Session session)
{
	/// <summary>
	/// The SSH session.
	/// </summary>
	protected internal readonly Session _session = session ?? throw new ArgumentNullException(nameof(session));

	/// <summary>
	/// Gets the session identifier.
	/// </summary>
	public Guid SessionId { get; } = session.Id;

	/// <summary>
	/// Closes the service.
	/// </summary>
	internal protected abstract void CloseService();
}
