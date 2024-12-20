﻿using System.Diagnostics.Contracts;

namespace PanoramicData.SshServer.Services;

public class TcpRequestArgs
{
	public TcpRequestArgs(SessionChannel channel, string host, int port, string originatorIP, int originatorPort, UserAuthArgs userauthArgs)
	{
		Contract.Requires(channel != null);
		Contract.Requires(host != null);
		Contract.Requires(originatorIP != null);

		Channel = channel;
		Host = host;
		Port = port;
		OriginatorIP = originatorIP;
		OriginatorPort = originatorPort;
		AttachedUserauthArgs = userauthArgs;
	}

	public SessionChannel Channel { get; private set; }
	public string Host { get; private set; }
	public int Port { get; private set; }
	public string OriginatorIP { get; private set; }
	public int OriginatorPort { get; private set; }
	public UserAuthArgs AttachedUserauthArgs { get; private set; }
}
