namespace PanoramicData.SshServer.Services;

/// <summary>
/// Represents an SSH session channel.
/// </summary>
public class SessionChannel : Channel
{
	/// <summary>
	/// Initializes a new instance of the <see cref="SessionChannel"/> class.
	/// </summary>
	/// <param name="connectionService">The connection service.</param>
	/// <param name="clientChannelId">The client channel identifier.</param>
	/// <param name="clientInitialWindowSize">The client initial window size.</param>
	/// <param name="clientMaxPacketSize">The client maximum packet size.</param>
	/// <param name="serverChannelId">The server channel identifier.</param>
	public SessionChannel(
		ConnectionService connectionService,
		uint clientChannelId,
		uint clientInitialWindowSize,
		uint clientMaxPacketSize,
		uint serverChannelId)
		: base(
			connectionService,
			clientChannelId,
			clientInitialWindowSize,
			clientMaxPacketSize,
			serverChannelId)
	{
	}
}