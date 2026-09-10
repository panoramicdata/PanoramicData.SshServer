using System.Security.Cryptography;

namespace PanoramicData.SshServer.Algorithms;

/// <summary>
/// Implements the DSS (Digital Signature Standard) public key algorithm.
/// </summary>
public class DssKey : PublicKeyAlgorithm
{
	private readonly DSACryptoServiceProvider _algorithm = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="DssKey"/> class.
	/// </summary>
	/// <param name="key">The optional base64-encoded key.</param>
	public DssKey(string? key = null)
		: base(key) => ImportConstructorKey();

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
		ReadAndVerifyName(worker);

		_algorithm.ImportParameters(new DSAParameters
		{
			P = worker.ReadMpint(),
			Q = worker.ReadMpint(),
			G = worker.ReadMpint(),
			Y = worker.ReadMpint()
		});
	}

	/// <inheritdoc />
	public override byte[] CreateKeyAndCertificatesData()
	{
		var args = _algorithm.ExportParameters(false);
		return WriteKeyAndCertificatesData(args.P!, args.Q!, args.G!, args.Y!);
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
