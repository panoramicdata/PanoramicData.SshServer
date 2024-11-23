using System.Net;

namespace PanoramicData.SshServer;

public class StartingInfo(IPAddress localAddress, int port, string serverBanner)
{
	public IPAddress LocalAddress { get; } = localAddress;

	public int Port { get; } = port;

	public string ServerBanner { get; } = serverBanner;
}
