namespace PanoramicData.SshServer.Interfaces;

/// <summary>
/// Defines the interface for an SSH application.
/// </summary>
public interface ISshApplication
{
	/// <summary>
	/// Called when an SSH session ends.
	/// </summary>
	/// <param name="sender">The event sender.</param>
	/// <param name="e">The session.</param>
	void SshServerSessionEnd(object? sender, Session e);

	/// <summary>
	/// Called when an SSH session starts.
	/// </summary>
	/// <param name="sender">The event sender.</param>
	/// <param name="e">The session.</param>
	void SshServerSessionStart(object? sender, Session e);
}
