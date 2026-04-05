namespace PanoramicData.SshServer.Services;

/// <summary>
/// Contains arguments for a key exchange event.
/// </summary>
public class KeyExchangeArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="KeyExchangeArgs"/> class.
	/// </summary>
	/// <param name="s">The SSH session.</param>
	public KeyExchangeArgs(Session s)
	{
		Session = s;
	}

	/// <summary>
	/// Gets the SSH session.
	/// </summary>
	public Session Session { get; private set; }

	/// <summary>
	/// Gets the exchange cookie.
	/// </summary>
	public byte[]? Cookie { get; private set; }

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
}
