using System;
using System.ComponentModel;
using System.Security.Cryptography;

namespace PanoramicData.SshServer.Algorithms;

/// <summary>
/// Implements an SSH encryption algorithm.
/// </summary>
public class EncryptionAlgorithm
{
	private readonly SymmetricAlgorithm _algorithm;
	private readonly CipherModeEx _mode;
	private readonly ICryptoTransform _transform;

	/// <summary>
	/// Initializes a new instance of the <see cref="EncryptionAlgorithm"/> class.
	/// </summary>
	/// <param name="algorithm">The symmetric algorithm.</param>
	/// <param name="keySize">The key size in bits.</param>
	/// <param name="mode">The cipher mode.</param>
	/// <param name="key">The encryption key.</param>
	/// <param name="iv">The initialization vector.</param>
	/// <param name="isEncryption">Whether this is for encryption (true) or decryption (false).</param>
	public EncryptionAlgorithm(SymmetricAlgorithm algorithm, int keySize, CipherModeEx mode, byte[] key, byte[] iv, bool isEncryption)
	{
		ArgumentNullException.ThrowIfNull(algorithm);
		ArgumentNullException.ThrowIfNull(key);
		ArgumentNullException.ThrowIfNull(iv);
		if (keySize != key.Length << 3)
			throw new ArgumentException($"Key size {keySize} does not match key length.", nameof(keySize));

		algorithm.KeySize = keySize;
		algorithm.Key = key;
		algorithm.IV = iv;
		algorithm.Padding = PaddingMode.None;

		_algorithm = algorithm;
		_mode = mode;

		_transform = CreateTransform(isEncryption);
	}

	/// <summary>
	/// Gets the block size in bytes.
	/// </summary>
	public int BlockBytesSize => _algorithm.BlockSize >> 3;

	/// <summary>
	/// Transforms the input data.
	/// </summary>
	/// <param name="input">The data to transform.</param>
	/// <returns>The transformed data.</returns>
	public byte[] Transform(byte[] input)
	{
		var output = new byte[input.Length];
		_transform.TransformBlock(input, 0, input.Length, output, 0);
		return output;
	}

	private ICryptoTransform CreateTransform(bool isEncryption)
	{
		switch (_mode)
		{
			case CipherModeEx.CBC:
					// CBC mode is used as required by SSH protocol specification
					_algorithm.Mode = CipherMode.CBC;
				return isEncryption
					? _algorithm.CreateEncryptor()
					: _algorithm.CreateDecryptor();
			case CipherModeEx.CTR:
				return new CtrModeCryptoTransform(_algorithm);
			default:
				throw new InvalidEnumArgumentException(string.Format("Invalid mode: {0}", _mode));
		}
	}
}
