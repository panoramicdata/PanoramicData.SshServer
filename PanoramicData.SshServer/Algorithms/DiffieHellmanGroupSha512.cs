using System.Security.Cryptography;

namespace PanoramicData.SshServer.Algorithms;

/// <summary>
/// Implements Diffie-Hellman key exchange using SHA-512.
/// </summary>
public class DiffieHellmanGroupSha512 : KexAlgorithm
{
	private readonly DiffieHellman _exchangeAlgorithm;

	/// <summary>
	/// Initializes a new instance of the <see cref="DiffieHellmanGroupSha512"/> class.
	/// </summary>
	/// <param name="algorithm">The Diffie-Hellman algorithm instance.</param>
	public DiffieHellmanGroupSha512(DiffieHellman algorithm)
	{
		ArgumentNullException.ThrowIfNull(algorithm);

		_exchangeAlgorithm = algorithm;
		_hashAlgorithm = SHA512.Create();
	}

	/// <inheritdoc />
	public override byte[] CreateKeyExchange() => _exchangeAlgorithm.CreateKeyExchange();

	/// <inheritdoc />
	public override byte[] DecryptKeyExchange(byte[] exchangeData) => _exchangeAlgorithm.DecryptKeyExchange(exchangeData);
}
