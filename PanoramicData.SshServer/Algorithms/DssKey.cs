using System.Security.Cryptography;
using System.Text;

namespace PanoramicData.SshServer.Algorithms;

/// <summary>
/// Implements the DSS (Digital Signature Standard) public key algorithm.
/// </summary>
/// <param name="key">The optional base64-encoded key.</param>
public class DssKey(string? key = null) : PublicKeyAlgorithm(key)
{
	private readonly DSACryptoServiceProvider _algorithm = new();

	/// <inheritdoc />
	public override string Name => "ssh-dss";

	/// <inheritdoc />
	public override void ImportKey(byte[] bytes) => _algorithm.ImportCspBlob(bytes);

	/// <inheritdoc />
	public override byte[] ExportKey() => _algorithm.ExportCspBlob(true);

	/// <inheritdoc />
	public override void LoadKeyAndCertificatesData(byte[] data)
	{
		using var worker = new SshDataWorker(data);
		if (worker.ReadString(Encoding.ASCII) != Name)
			throw new CryptographicException("Key and certificates were not created with this algorithm.");

		var args = new DSAParameters
		{
			P = worker.ReadMpint(),
			Q = worker.ReadMpint(),
			G = worker.ReadMpint(),
			Y = worker.ReadMpint()
		};

		_algorithm.ImportParameters(args);
	}

	/// <inheritdoc />
	public override byte[] CreateKeyAndCertificatesData()
	{
		using var worker = new SshDataWorker();
		var args = _algorithm.ExportParameters(false);

		worker.Write(Name, Encoding.ASCII);
		worker.WriteMpint(args.P!);
		worker.WriteMpint(args.Q!);
		worker.WriteMpint(args.G!);
		worker.WriteMpint(args.Y!);

		return worker.ToByteArray();
	}

	/// <inheritdoc />
	public override bool VerifyData(byte[] data, byte[] signature) => _algorithm.VerifyData(data, signature);

	/// <inheritdoc />
	public override bool VerifyHash(byte[] hash, byte[] signature) => _algorithm.VerifyHash(hash, "SHA1", signature);

	/// <inheritdoc />
	public override byte[] SignData(byte[] data) => _algorithm.SignData(data);

	/// <inheritdoc />
	public override byte[] SignHash(byte[] hash) => _algorithm.SignHash(hash, "SHA1");
}
