namespace PanoramicData.SshServer.Algorithms;

/// <summary>
/// Implements a no-op compression algorithm.
/// </summary>
public class NoCompression : CompressionAlgorithm
{
	/// <inheritdoc />
	public override byte[] Compress(byte[] input) => input;

	/// <inheritdoc />
	public override byte[] Decompress(byte[] input) => input;
}
