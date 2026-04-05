using System;
using System.Security.Cryptography;

namespace PanoramicData.SshServer.Algorithms;

/// <summary>
/// Contains HMAC algorithm configuration information.
/// </summary>
public class HmacInfo
{
	/// <summary>
	/// Initializes a new instance of the <see cref="HmacInfo"/> class.
	/// </summary>
	/// <param name="algorithm">The keyed hash algorithm.</param>
	/// <param name="keySize">The key size in bits.</param>
	public HmacInfo(KeyedHashAlgorithm algorithm, int keySize)
	{
		ArgumentNullException.ThrowIfNull(algorithm);

		KeySize = keySize;
		Hmac = key => new HmacAlgorithm(algorithm, keySize, key);
	}

	/// <summary>
	/// Gets the key size in bits.
	/// </summary>
	public int KeySize { get; private set; }

	/// <summary>
	/// Gets the HMAC factory function.
	/// </summary>
	public Func<byte[], HmacAlgorithm> Hmac { get; private set; }
}
