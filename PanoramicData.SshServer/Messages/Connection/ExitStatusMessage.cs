namespace PanoramicData.SshServer.Messages.Connection;

/// <summary>
/// Represents an SSH exit status message.
/// </summary>
public class ExitStatusMessage : ChannelRequestMessage
{
	/// <summary>
	/// Gets or sets the exit status code.
	/// </summary>
	public uint ExitStatus { get; set; }

	/// <inheritdoc />
	protected override void OnGetPacket(SshDataWorker writer)
	{
		RequestType = "exit-status";
		WantReply = false;

		base.OnGetPacket(writer);

		writer.Write(ExitStatus);
	}
}
