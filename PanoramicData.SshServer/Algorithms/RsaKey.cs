using System.Security.Cryptography;

namespace PanoramicData.SshServer.Algorithms;

/// <summary>
/// Implements the RSA public key algorithm.
/// </summary>
public class RsaKey : PublicKeyAlgorithm
{
	private readonly RSACryptoServiceProvider _algorithm = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="RsaKey"/> class.
	/// </summary>
	/// <param name="key">The optional base64-encoded key.</param>
	public RsaKey(string? key = null)
		: base(key) => ImportConstructorKey();

	/// <inheritdoc />
	public override string Name => "rsa-sha2-256";

	/// <inheritdoc />
	public override void ImportKey(byte[] bytes) => _algorithm.ImportRSAPrivateKey(bytes, out var _);

	/// <inheritdoc />
	public override byte[] ExportKey() => _algorithm.ExportCspBlob(true);

	/// <inheritdoc />
	public override void LoadKeyAndCertificatesData(byte[] data)
	{
		using var worker = new SshDataWorker(data);
		ReadAndVerifyName(worker);

		_algorithm.ImportParameters(new RSAParameters
		{
			Exponent = worker.ReadMpint(),
			Modulus = worker.ReadMpint()
		});
	}

	/// <inheritdoc />
	public override byte[] CreateKeyAndCertificatesData()
	{
		var args = _algorithm.ExportParameters(false);
		return WriteKeyAndCertificatesData(args.Exponent!, args.Modulus!);
	}

	/// <inheritdoc />
	public override bool VerifyData(byte[] data, byte[] signature) => _algorithm.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

	/// <inheritdoc />
	public override bool VerifyHash(byte[] hash, byte[] signature) => _algorithm.VerifyHash(hash, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

	/// <inheritdoc />
	public override byte[] SignData(byte[] data) => _algorithm.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

	/// <inheritdoc />
	public override byte[] SignHash(byte[] hash) => _algorithm.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
}
