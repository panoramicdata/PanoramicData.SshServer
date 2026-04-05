namespace PanoramicData.SshServer.Services;

/// <summary>
/// Contains arguments for a TCP forwarding request event.
/// </summary>
public class TcpRequestArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TcpRequestArgs"/> class.
	/// </summary>
	/// <param name="channel">The session channel.</param>
	/// <param name="host">The target host.</param>
	/// <param name="port">The target port.</param>
	/// <param name="originatorIP">The originator IP address.</param>
	/// <param name="originatorPort">The originator port.</param>
	/// <param name="userauthArgs">The user authentication arguments.</param>
	public TcpRequestArgs(SessionChannel channel, string host, int port, string originatorIP, int originatorPort, UserAuthArgs userauthArgs)
	{
		ArgumentNullException.ThrowIfNull(channel);
		ArgumentNullException.ThrowIfNull(host);
		ArgumentNullException.ThrowIfNull(originatorIP);

		Channel = channel;
		Host = host;
		Port = port;
		OriginatorIP = originatorIP;
		OriginatorPort = originatorPort;
		AttachedUserauthArgs = userauthArgs;
	}

	/// <summary>
	/// Gets the session channel.
	/// </summary>
	public SessionChannel Channel { get; private set; }

	/// <summary>
	/// Gets the target host.
	/// </summary>
	public string Host { get; private set; }

	/// <summary>
	/// Gets the target port.
	/// </summary>
	public int Port { get; private set; }

	/// <summary>
	/// Gets the originator IP address.
	/// </summary>
	public string OriginatorIP { get; private set; }

	/// <summary>
	/// Gets the originator port.
	/// </summary>
	public int OriginatorPort { get; private set; }

	/// <summary>
	/// Gets the attached user authentication arguments.
	/// </summary>
	public UserAuthArgs AttachedUserauthArgs { get; private set; }
}
