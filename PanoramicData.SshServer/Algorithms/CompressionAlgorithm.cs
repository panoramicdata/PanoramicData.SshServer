using System.Diagnostics.Contracts;

namespace PanoramicData.SshServer.Algorithms;

/// <summary>
/// Base class for SSH compression algorithms.
/// </summary>
[ContractClass(typeof(CompressionAlgorithmContract))]
public abstract class CompressionAlgorithm
{
	/// <summary>
	/// Compresses the input data.
	/// </summary>
	/// <param name="input">The data to compress.</param>
	/// <returns>The compressed data.</returns>
	public abstract byte[] Compress(byte[] input);

	/// <summary>
	/// Decompresses the input data.
	/// </summary>
	/// <param name="input">The data to decompress.</param>
	/// <returns>The decompressed data.</returns>
	public abstract byte[] Decompress(byte[] input);
}
