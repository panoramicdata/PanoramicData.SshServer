using System.Security.Cryptography;

namespace PanoramicData.SshServer.Algorithms;

/// <summary>
/// Implements an SSH HMAC algorithm.
/// </summary>
public class HmacAlgorithm
{
	private readonly KeyedHashAlgorithm _algorithm;

	/// <summary>
	/// Initializes a new instance of the <see cref="HmacAlgorithm"/> class.
	/// </summary>
	/// <param name="algorithm">The keyed hash algorithm.</param>
	/// <param name="keySize">The key size in bits.</param>
	/// <param name="key">The key bytes.</param>
	public HmacAlgorithm(KeyedHashAlgorithm algorithm, int keySize, byte[] key)
	{
		ArgumentNullException.ThrowIfNull(algorithm);
		ArgumentNullException.ThrowIfNull(key);

		_algorithm = algorithm;
		algorithm.Key = key;
	}

	/// <summary>
	/// Gets the digest length in bytes.
	/// </summary>
	public int DigestLength => _algorithm.HashSize >> 3;

	/// <summary>
	/// Computes the hash of the input.
	/// </summary>
	/// <param name="input">The input data.</param>
	/// <returns>The computed hash.</returns>
	public byte[] ComputeHash(byte[] input)
	{
		ArgumentNullException.ThrowIfNull(input);

		return _algorithm.ComputeHash(input);
	}
}
