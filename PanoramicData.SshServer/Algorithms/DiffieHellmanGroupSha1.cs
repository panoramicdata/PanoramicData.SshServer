using System.Security.Cryptography;

namespace PanoramicData.SshServer.Algorithms;

/// <summary>
/// Implements Diffie-Hellman key exchange using SHA-1.
/// </summary>
public class DiffieHellmanGroupSha1 : KexAlgorithm
{
	private readonly DiffieHellman _exchangeAlgorithm;

	/// <summary>
	/// Initializes a new instance of the <see cref="DiffieHellmanGroupSha1"/> class.
	/// </summary>
	/// <param name="algorithm">The Diffie-Hellman algorithm instance.</param>
	public DiffieHellmanGroupSha1(DiffieHellman algorithm)
	{
		ArgumentNullException.ThrowIfNull(algorithm);

		_exchangeAlgorithm = algorithm;
		_hashAlgorithm = SHA1.Create();
	}

	/// <inheritdoc />
	public override byte[] CreateKeyExchange() => _exchangeAlgorithm.CreateKeyExchange();

	/// <inheritdoc />
	public override byte[] DecryptKeyExchange(byte[] exchangeData) => _exchangeAlgorithm.DecryptKeyExchange(exchangeData);
}
