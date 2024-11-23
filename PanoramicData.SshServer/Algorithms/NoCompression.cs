namespace PanoramicData.SshServer.Algorithms;

public class NoCompression : CompressionAlgorithm
{
	public override byte[] Compress(byte[] input) => input;

	public override byte[] Decompress(byte[] input) => input;
}
