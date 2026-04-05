using System;
using System.Security.Cryptography;

namespace PanoramicData.SshServer.Algorithms;

/// <summary>
/// Base class for SSH key exchange algorithms.
/// </summary>
public abstract class KexAlgorithm
{
	/// <summary>
	/// The hash algorithm used for key exchange.
	/// </summary>
	protected HashAlgorithm _hashAlgorithm = null!;

	/// <summary>
	/// Creates the key exchange value.
	/// </summary>
	/// <returns>The public key exchange value.</returns>
	public abstract byte[] CreateKeyExchange();

	/// <summary>
	/// Decrypts the key exchange to produce the shared secret.
	/// </summary>
	/// <param name="exchangeData">The peer's key exchange value.</param>
	/// <returns>The shared secret.</returns>
	public abstract byte[] DecryptKeyExchange(byte[] exchangeData);

	/// <summary>
	/// Computes a hash of the input data.
	/// </summary>
	/// <param name="input">The input data.</param>
	/// <returns>The computed hash.</returns>
	public byte[] ComputeHash(byte[] input)
	{
		ArgumentNullException.ThrowIfNull(input);

		return _hashAlgorithm.ComputeHash(input);
	}
}
