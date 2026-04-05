using System;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;

namespace PanoramicData.SshServer.Algorithms;

/// <summary>
/// Implements CTR mode encryption as an <see cref="ICryptoTransform"/>.
/// </summary>
public class CtrModeCryptoTransform : ICryptoTransform
{
	private readonly SymmetricAlgorithm _algorithm;
	private readonly ICryptoTransform _transform;
	private readonly byte[] _iv;
	private readonly byte[] _block;


	/// <summary>
	/// Initializes a new instance of the <see cref="CtrModeCryptoTransform"/> class.
	/// </summary>
	/// <param name="algorithm">The symmetric algorithm to use.</param>
	public CtrModeCryptoTransform(SymmetricAlgorithm algorithm)
	{
		ArgumentNullException.ThrowIfNull(algorithm);

		// ECB mode is intentionally used here as the base cipher for CTR mode implementation
		algorithm.Mode = CipherMode.ECB;
		algorithm.Padding = PaddingMode.None;

		_algorithm = algorithm;
		_transform = algorithm.CreateEncryptor();
		_iv = algorithm.IV;
		_block = new byte[algorithm.BlockSize >> 3];
	}

	/// <inheritdoc />
	public bool CanReuseTransform => true;

	/// <inheritdoc />
	public bool CanTransformMultipleBlocks => true;

	/// <inheritdoc />
	public int InputBlockSize => _algorithm.BlockSize;

	/// <inheritdoc />
	public int OutputBlockSize => _algorithm.BlockSize;

	/// <inheritdoc />
	public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
	{
		var written = 0;
		var bytesPerBlock = InputBlockSize >> 3;

		for (var i = 0; i < inputCount; i += bytesPerBlock)
		{
			written += _transform.TransformBlock(_iv, 0, bytesPerBlock, _block, 0);

			for (var j = 0; j < bytesPerBlock; j++)
			{
				outputBuffer[outputOffset + i + j] = (byte)(_block[j] ^ inputBuffer[inputOffset + i + j]);
			}

			var k = _iv.Length;
			while (--k >= 0 && ++_iv[k] == 0)
			{
				// Increment IV counter with carry propagation
			}
		}

		return written;
	}

	/// <inheritdoc />
	public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
	{
		var output = new byte[inputCount];
		TransformBlock(inputBuffer, inputOffset, inputCount, output, 0);
		return output;
	}

	/// <inheritdoc />
	public void Dispose()
	{
		_transform.Dispose();
		GC.SuppressFinalize(this);
	}
}
