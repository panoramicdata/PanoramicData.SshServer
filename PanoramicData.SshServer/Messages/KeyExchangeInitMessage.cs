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

	private static string[] ReadAlgorithmList(SshDataWorker reader)
	{
		return reader.ReadString(Encoding.ASCII).Split(',');
	}

	private static void WriteAlgorithmList(SshDataWorker writer, string[]? algorithms)
	{
		writer.Write(string.Join(",", algorithms!), Encoding.ASCII);
	}

	/// <inheritdoc />
	protected override void OnLoad(SshDataWorker reader)
	{
		Cookie = reader.ReadBinary(16);
		KeyExchangeAlgorithms = ReadAlgorithmList(reader);
		ServerHostKeyAlgorithms = ReadAlgorithmList(reader);
		EncryptionAlgorithmsClientToServer = ReadAlgorithmList(reader);
		EncryptionAlgorithmsServerToClient = ReadAlgorithmList(reader);
		MacAlgorithmsClientToServer = ReadAlgorithmList(reader);
		MacAlgorithmsServerToClient = ReadAlgorithmList(reader);
		CompressionAlgorithmsClientToServer = ReadAlgorithmList(reader);
		CompressionAlgorithmsServerToClient = ReadAlgorithmList(reader);
		LanguagesClientToServer = ReadAlgorithmList(reader);
		LanguagesServerToClient = ReadAlgorithmList(reader);
		FirstKexPacketFollows = reader.ReadBoolean();
		Reserved = reader.ReadUInt32();
	}

	/// <inheritdoc />
	protected override void OnGetPacket(SshDataWorker writer)
	{
		writer.Write(Cookie);
		WriteAlgorithmList(writer, KeyExchangeAlgorithms);
		WriteAlgorithmList(writer, ServerHostKeyAlgorithms);
		WriteAlgorithmList(writer, EncryptionAlgorithmsClientToServer);
		WriteAlgorithmList(writer, EncryptionAlgorithmsServerToClient);
		WriteAlgorithmList(writer, MacAlgorithmsClientToServer);
		WriteAlgorithmList(writer, MacAlgorithmsServerToClient);
		WriteAlgorithmList(writer, CompressionAlgorithmsClientToServer);
		WriteAlgorithmList(writer, CompressionAlgorithmsServerToClient);
		WriteAlgorithmList(writer, LanguagesClientToServer);
		WriteAlgorithmList(writer, LanguagesServerToClient);
		writer.Write(FirstKexPacketFollows);
		writer.Write(Reserved);
	}
}
