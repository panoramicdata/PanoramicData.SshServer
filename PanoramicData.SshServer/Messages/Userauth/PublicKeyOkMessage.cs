﻿using System.Text;

namespace PanoramicData.SshServer.Messages.Userauth;

[Message("SSH_MSG_USERAUTH_PK_OK", MessageNumber)]
public class PublicKeyOkMessage : UserAuthServiceMessage
{
	private const byte MessageNumber = 60;

	public string KeyAlgorithmName { get; set; }
	public byte[] PublicKey { get; set; }

	public override byte MessageType => MessageNumber;

	protected override void OnGetPacket(SshDataWorker writer)
	{
		writer.Write(KeyAlgorithmName, Encoding.ASCII);
		writer.WriteBinary(PublicKey);
	}
}
