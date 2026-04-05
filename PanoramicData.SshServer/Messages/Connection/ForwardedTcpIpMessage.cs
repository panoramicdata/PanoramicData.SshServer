using System;
using System.Text;

namespace PanoramicData.SshServer.Messages.Connection;

/// <summary>
/// Represents an SSH forwarded TCP/IP channel open message.
/// </summary>
public class ForwardedTcpIpMessage : ChannelOpenMessage
{
	/// <summary>
	/// Gets the address that was connected.
	/// </summary>
	public string? Address { get; private set; }

	/// <summary>
	/// Gets the port that was connected.
	/// </summary>
	public uint Port { get; private set; }

	/// <summary>
	/// Gets the originator IP address.
	/// </summary>
	public string? OriginatorIPAddress { get; private set; }

	/// <summary>
	/// Gets the originator port.
	/// </summary>
	public uint OriginatorPort { get; private set; }

	/// <inheritdoc />
	protected override void OnLoad(SshDataWorker reader)
	{
		base.OnLoad(reader);

		if (ChannelType != "forwarded-tcpip")
			throw new ArgumentException(string.Format("Channel type {0} is not valid.", ChannelType));

		Address = reader.ReadString(Encoding.ASCII);
		Port = reader.ReadUInt32();
		OriginatorIPAddress = reader.ReadString(Encoding.ASCII);
		OriginatorPort = reader.ReadUInt32();
	}
}
