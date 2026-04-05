using System.Security.Cryptography;
using System.Text;

namespace PanoramicData.SshServer.Messages;

/// <summary>
/// Represents an SSH key exchange init message.
/// </summary>
[Message("SSH_MSG_KEXINIT", MessageNumber)]
public class KeyExchangeInitMessage : Message
{
	private const byte MessageNumber = 20;

	/// <summary>
	/// Initializes a new instance of the <see cref="KeyExchangeInitMessage"/> class.
	/// </summary>
	public KeyExchangeInitMessage()
	{
		Cookie = new byte[16];
		RandomNumberGenerator.Fill(Cookie);
	}

	/// <summary>
	/// Gets the random cookie.
	/// </summary>
	public byte[] Cookie { get; private set; }

	/// <summary>
	/// Gets or sets the key exchange algorithms.
	/// </summary>
	public string[]? KeyExchangeAlgorithms { get; set; }

	/// <summary>
	/// Gets or sets the server host key algorithms.
	/// </summary>
	public string[]? ServerHostKeyAlgorithms { get; set; }

	/// <summary>
	/// Gets or sets the client-to-server encryption algorithms.
	/// </summary>
	public string[]? EncryptionAlgorithmsClientToServer { get; set; }

	/// <summary>
	/// Gets or sets the server-to-client encryption algorithms.
	/// </summary>
	public string[]? EncryptionAlgorithmsServerToClient { get; set; }

	/// <summary>
	/// Gets or sets the client-to-server MAC algorithms.
	/// </summary>
	public string[]? MacAlgorithmsClientToServer { get; set; }

	/// <summary>
	/// Gets or sets the server-to-client MAC algorithms.
	/// </summary>
	public string[]? MacAlgorithmsServerToClient { get; set; }

	/// <summary>
	/// Gets or sets the client-to-server compression algorithms.
	/// </summary>
	public string[]? CompressionAlgorithmsClientToServer { get; set; }

	/// <summary>
	/// Gets or sets the server-to-client compression algorithms.
	/// </summary>
	public string[]? CompressionAlgorithmsServerToClient { get; set; }

	/// <summary>
	/// Gets or sets the client-to-server languages.
	/// </summary>
	public string[]? LanguagesClientToServer { get; set; }

	/// <summary>
	/// Gets or sets the server-to-client languages.
	/// </summary>
	public string[]? LanguagesServerToClient { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the first key exchange packet follows.
	/// </summary>
	public bool FirstKexPacketFollows { get; set; }

	/// <summary>
	/// Gets or sets the reserved value.
	/// </summary>
	public uint Reserved { get; set; }

	/// <inheritdoc />
	public override byte MessageType => MessageNumber;

	/// <inheritdoc />
	protected override void OnLoad(SshDataWorker reader)
	{
		Cookie = reader.ReadBinary(16);
		KeyExchangeAlgorithms = reader.ReadString(Encoding.ASCII).Split(',');
		ServerHostKeyAlgorithms = reader.ReadString(Encoding.ASCII).Split(',');
		EncryptionAlgorithmsClientToServer = reader.ReadString(Encoding.ASCII).Split(',');
		EncryptionAlgorithmsServerToClient = reader.ReadString(Encoding.ASCII).Split(',');
		MacAlgorithmsClientToServer = reader.ReadString(Encoding.ASCII).Split(',');
		MacAlgorithmsServerToClient = reader.ReadString(Encoding.ASCII).Split(',');
		CompressionAlgorithmsClientToServer = reader.ReadString(Encoding.ASCII).Split(',');
		CompressionAlgorithmsServerToClient = reader.ReadString(Encoding.ASCII).Split(',');
		LanguagesClientToServer = reader.ReadString(Encoding.ASCII).Split(',');
		LanguagesServerToClient = reader.ReadString(Encoding.ASCII).Split(',');
		FirstKexPacketFollows = reader.ReadBoolean();
		Reserved = reader.ReadUInt32();
	}

	/// <inheritdoc />
	protected override void OnGetPacket(SshDataWorker writer)
	{
		writer.Write(Cookie);
		writer.Write(string.Join(",", KeyExchangeAlgorithms!), Encoding.ASCII);
		writer.Write(string.Join(",", ServerHostKeyAlgorithms!), Encoding.ASCII);
		writer.Write(string.Join(",", EncryptionAlgorithmsClientToServer!), Encoding.ASCII);
		writer.Write(string.Join(",", EncryptionAlgorithmsServerToClient!), Encoding.ASCII);
		writer.Write(string.Join(",", MacAlgorithmsClientToServer!), Encoding.ASCII);
		writer.Write(string.Join(",", MacAlgorithmsServerToClient!), Encoding.ASCII);
		writer.Write(string.Join(",", CompressionAlgorithmsClientToServer!), Encoding.ASCII);
		writer.Write(string.Join(",", CompressionAlgorithmsServerToClient!), Encoding.ASCII);
		writer.Write(string.Join(",", LanguagesClientToServer!), Encoding.ASCII);
		writer.Write(string.Join(",", LanguagesServerToClient!), Encoding.ASCII);
		writer.Write(FirstKexPacketFollows);
		writer.Write(Reserved);
	}
}
