﻿namespace PanoramicData.SshServer.Messages.Connection;

[Message("SSH_MSG_CHANNEL_EOF", MessageNumber)]
public class ChannelEofMessage : ConnectionServiceMessage
{
	private const byte MessageNumber = 96;

	public uint RecipientChannel { get; set; }

	public override byte MessageType => MessageNumber;

	protected override void OnLoad(SshDataWorker reader) => RecipientChannel = reader.ReadUInt32();

	protected override void OnGetPacket(SshDataWorker writer) => writer.Write(RecipientChannel);
}
