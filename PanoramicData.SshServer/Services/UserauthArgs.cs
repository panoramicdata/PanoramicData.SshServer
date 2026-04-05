namespace PanoramicData.SshServer.Services;

/// <summary>
/// Contains arguments for a user authentication event.
/// </summary>
public class UserAuthArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="UserAuthArgs"/> class for public key authentication.
	/// </summary>
	/// <param name="session">The SSH session.</param>
	/// <param name="username">The username.</param>
	/// <param name="keyAlgorithm">The key algorithm name.</param>
	/// <param name="fingerprint">The key fingerprint.</param>
	/// <param name="key">The public key bytes.</param>
	public UserAuthArgs(Session session, string username, string keyAlgorithm, string fingerprint, byte[] key)
	{
		ArgumentNullException.ThrowIfNull(session);
		ArgumentNullException.ThrowIfNull(username);
		ArgumentNullException.ThrowIfNull(keyAlgorithm);
		ArgumentNullException.ThrowIfNull(fingerprint);
		ArgumentNullException.ThrowIfNull(key);

		AuthMethod = "publickey";
		KeyAlgorithm = keyAlgorithm;
		Fingerprint = fingerprint;
		Key = key;
		Session = session;
		Username = username;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="UserAuthArgs"/> class for password authentication.
	/// </summary>
	/// <param name="session">The SSH session.</param>
	/// <param name="username">The username.</param>
	/// <param name="password">The password.</param>
	public UserAuthArgs(Session session, string username, string password)
	{
		ArgumentNullException.ThrowIfNull(session);
		ArgumentNullException.ThrowIfNull(username);
		ArgumentNullException.ThrowIfNull(password);

		AuthMethod = "password";
		Username = username;
		Password = password;
		Session = session;
	}

	/// <summary>
	/// Gets the authentication method.
	/// </summary>
	public string AuthMethod { get; private set; }

	/// <summary>
	/// Gets the SSH session.
	/// </summary>
	public Session Session { get; private set; }

	/// <summary>
	/// Gets the username.
	/// </summary>
	public string Username { get; private set; }

	/// <summary>
	/// Gets the password.
	/// </summary>
	public string? Password { get; private set; }

	/// <summary>
	/// Gets the key algorithm name.
	/// </summary>
	public string? KeyAlgorithm { get; private set; }

	/// <summary>
	/// Gets the key fingerprint.
	/// </summary>
	public string? Fingerprint { get; private set; }

	/// <summary>
	/// Gets the public key bytes.
	/// </summary>
	public byte[]? Key { get; private set; }

	/// <summary>
	/// Gets or sets the authentication result.
	/// </summary>
	public bool Result { get; set; }
}
