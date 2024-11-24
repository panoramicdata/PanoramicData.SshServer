namespace PanoramicData.SshServer.Config;

public class SshServerConfiguration
{
	/// <summary>
	/// The address on which the SSH server should listen.
	/// Use:
	/// - an IP address
	/// - "IPv6Any" to listen dual stack
	/// - "Any" to listen on all network interfaces.
	/// </summary>
	public string LocalAddress { get; set; } = string.Empty;

	/// <summary>
	/// The port on which the SSH server should listen.
	/// You probably want to use 22 in direct mode, or 2222 with port mapping in a docker environment.
	/// </summary>
	public int Port { get; set; }

	/// <summary>
	///	The server banner to display to the client.
	///	This is typically the SSH server software version.
	///	It should be in the format "SSH-2.0-software".
	public string ServerBanner { get; set; } = string.Empty;
}
