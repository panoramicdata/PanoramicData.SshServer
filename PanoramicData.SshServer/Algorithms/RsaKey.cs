using System.Security.Cryptography;
using System.Text;

namespace PanoramicData.SshServer.Algorithms;

/// <summary>
/// Implements the RSA public key algorithm.
/// </summary>
/// <param name="key">The optional base64-encoded key.</param>
public class RsaKey(string? key = null) : PublicKeyAlgorithm(key)
{
	private readonly RSACryptoServiceProvider _algorithm = new();

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
		if (worker.ReadString(Encoding.ASCII) != Name)
			throw new CryptographicException("Key and certificates were not created with this algorithm.");

		var args = new RSAParameters
		{
			Exponent = worker.ReadMpint(),
			Modulus = worker.ReadMpint()
		};

		_algorithm.ImportParameters(args);
	}

	/// <inheritdoc />
	public override byte[] CreateKeyAndCertificatesData()
	{
		using var worker = new SshDataWorker();
		var args = _algorithm.ExportParameters(false);

		worker.Write(Name, Encoding.ASCII);
		worker.WriteMpint(args.Exponent!);
		worker.WriteMpint(args.Modulus!);

		return worker.ToByteArray();
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
