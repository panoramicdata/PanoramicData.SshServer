﻿using System.Text;

namespace PanoramicData.SshServer.Messages.Connection;

public class EnvMessage : ChannelRequestMessage
{
	public string Name { get; private set; }
	public string Value { get; private set; }

	protected override void OnLoad(SshDataWorker reader)
	{
		base.OnLoad(reader);

		Name = reader.ReadString(Encoding.ASCII);
		Value = reader.ReadString(Encoding.ASCII);
	}
}
