using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security.Cryptography;

namespace PanoramicData.SshServer.Algorithms;

/// <summary>
/// Contains cipher configuration information.
/// </summary>
public class CipherInfo
{
	/// <summary>
	/// Initializes a new instance of the <see cref="CipherInfo"/> class.
	/// </summary>
	/// <param name="algorithm">The symmetric algorithm.</param>
	/// <param name="keySize">The key size in bits.</param>
	/// <param name="mode">The cipher mode.</param>
	public CipherInfo(SymmetricAlgorithm algorithm, int keySize, CipherModeEx mode)
	{
		ArgumentNullException.ThrowIfNull(algorithm, nameof(algorithm));
		Contract.Requires(algorithm.LegalKeySizes.Any(x => x.MinSize <= keySize && keySize <= x.MaxSize && keySize % x.SkipSize == 0));

		algorithm.KeySize = keySize;
		KeySize = algorithm.KeySize;
		BlockSize = algorithm.BlockSize;
		Cipher = (key, vi, isEncryption) => new EncryptionAlgorithm(algorithm, keySize, mode, key, vi, isEncryption);
	}

	/// <summary>
	/// Gets the key size in bits.
	/// </summary>
	public int KeySize { get; private set; }

	/// <summary>
	/// Gets the block size in bits.
	/// </summary>
	public int BlockSize { get; private set; }

	/// <summary>
	/// Gets the cipher factory function.
	/// </summary>
	public Func<byte[], byte[], bool, EncryptionAlgorithm> Cipher { get; private set; }
}
