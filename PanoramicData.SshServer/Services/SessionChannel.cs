namespace PanoramicData.SshServer.Services;

public class SessionChannel(
	ConnectionService connectionService,
	uint clientChannelId,
	uint clientInitialWindowSize,
	uint clientMaxPacketSize,
	uint serverChannelId) : Channel(
		connectionService,
		clientChannelId,
		clientInitialWindowSize,
		clientMaxPacketSize,
		serverChannelId)
{
}
