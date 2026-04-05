using System;
using System.Text;

namespace PanoramicData.SshServer.Messages.Connection;

/// <summary>
/// Represents an SSH direct TCP/IP channel open message.
/// </summary>
public class DirectTcpIpMessage : ChannelOpenMessage
{
	/// <summary>
	/// Gets the target host.
	/// </summary>
	public string Host { get; private set; } = string.Empty;

	/// <summary>
	/// Gets the target port.
	/// </summary>
	public uint Port { get; private set; }

	/// <summary>
	/// Gets the originator IP address.
	/// </summary>
	public string OriginatorIPAddress { get; private set; } = string.Empty;

	/// <summary>
	/// Gets the originator port.
	/// </summary>
	public uint OriginatorPort { get; private set; }

	/// <inheritdoc />
	protected override void OnLoad(SshDataWorker reader)
	{
		base.OnLoad(reader);

		if (ChannelType != "direct-tcpip")
		{
			throw new ArgumentException(string.Format("Channel type {0} is not valid.", ChannelType));
		}

		Host = reader.ReadString(Encoding.ASCII);
		Port = reader.ReadUInt32();
		OriginatorIPAddress = reader.ReadString(Encoding.ASCII);
		OriginatorPort = reader.ReadUInt32();
	}
}
