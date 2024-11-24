using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security.Cryptography;

namespace PanoramicData.SshServer.Algorithms;

public class CipherInfo
{
	public CipherInfo(SymmetricAlgorithm algorithm, int keySize, CipherModeEx mode)
	{
		ArgumentNullException.ThrowIfNull(algorithm, nameof(algorithm));
		Contract.Requires(algorithm.LegalKeySizes.Any(x => x.MinSize <= keySize && keySize <= x.MaxSize && keySize % x.SkipSize == 0));

		algorithm.KeySize = keySize;
		KeySize = algorithm.KeySize;
		BlockSize = algorithm.BlockSize;
		Cipher = (key, vi, isEncryption) => new EncryptionAlgorithm(algorithm, keySize, mode, key, vi, isEncryption);
	}

	public int KeySize { get; private set; }

	public int BlockSize { get; private set; }

	public Func<byte[], byte[], bool, EncryptionAlgorithm> Cipher { get; private set; }
}
