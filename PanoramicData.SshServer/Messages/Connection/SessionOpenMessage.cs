using System;

namespace PanoramicData.SshServer.Messages.Connection;

/// <summary>
/// Represents an SSH session channel open message.
/// </summary>
public class SessionOpenMessage : ChannelOpenMessage
{
	/// <inheritdoc />
	protected override void OnLoad(SshDataWorker reader)
	{
		base.OnLoad(reader);

		if (ChannelType != "session")
			throw new ArgumentException(string.Format("Channel type {0} is not valid.", ChannelType));
	}
}
