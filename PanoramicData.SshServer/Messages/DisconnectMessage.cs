using System.Text;

namespace PanoramicData.SshServer.Messages;

/// <summary>
/// Represents an SSH disconnect message.
/// </summary>
[Message("SSH_MSG_DISCONNECT", MessageNumber)]
public class DisconnectMessage : Message
{
	private const byte MessageNumber = 1;

	/// <summary>
	/// Initializes a new instance of the <see cref="DisconnectMessage"/> class.
	/// </summary>
	public DisconnectMessage()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DisconnectMessage"/> class with the specified reason.
	/// </summary>
	/// <param name="reasonCode">The disconnect reason code.</param>
	/// <param name="description">The description.</param>
	/// <param name="language">The language tag.</param>
	public DisconnectMessage(DisconnectReason reasonCode, string description = "", string language = "en")
	{
		ArgumentNullException.ThrowIfNull(description);
		ArgumentNullException.ThrowIfNull(language);

		ReasonCode = reasonCode;
		Description = description;
		Language = language;
	}

	/// <summary>
	/// Gets the disconnect reason code.
	/// </summary>
	public DisconnectReason ReasonCode { get; private set; }

	/// <summary>
	/// Gets the disconnect description.
	/// </summary>
	public string Description { get; private set; } = string.Empty;

	/// <summary>
	/// Gets the language tag.
	/// </summary>
	public string Language { get; private set; } = string.Empty;

	/// <inheritdoc />
	public override byte MessageType => MessageNumber;

	/// <inheritdoc />
	protected override void OnLoad(SshDataWorker reader)
	{
		ReasonCode = (DisconnectReason)reader.ReadUInt32();
		Description = reader.ReadString(Encoding.UTF8);
		if (reader.DataAvailable >= 4)
			Language = reader.ReadString(Encoding.UTF8);
	}

	/// <inheritdoc />
	protected override void OnGetPacket(SshDataWorker writer)
	{
		writer.Write((uint)ReasonCode);
		writer.Write(Description, Encoding.UTF8);
		writer.Write(Language ?? "en", Encoding.UTF8);
	}
}
