﻿using System.Text;

namespace PanoramicData.SshServer.Messages.Userauth;

[Message("SSH_MSG_USERAUTH_REQUEST", MessageNumber)]
public class RequestMessage : UserAuthServiceMessage
{
	protected const byte MessageNumber = 50;

	public string Username { get; protected set; }
	public string ServiceName { get; protected set; }
	public string MethodName { get; protected set; }

	public override byte MessageType => MessageNumber;

	protected override void OnLoad(SshDataWorker reader)
	{
		Username = reader.ReadString(Encoding.UTF8);
		ServiceName = reader.ReadString(Encoding.ASCII);
		MethodName = reader.ReadString(Encoding.ASCII);
	}
}
