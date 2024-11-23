using System.Security.Cryptography;
using System.Text;

namespace PanoramicData.SshServer.Algorithms;

public class RsaKey(string key = null) : PublicKeyAlgorithm(key)
{
	private readonly RSACryptoServiceProvider _algorithm = new();

	public override string Name
	{
		get { return "rsa-sha2-256"; }
	}

	public override void ImportKey(byte[] bytes)
	{
		_algorithm.ImportRSAPrivateKey(bytes, out int bytesRead);
	}

	public override byte[] ExportKey()
	{
		return _algorithm.ExportCspBlob(true);
	}

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

	public override byte[] CreateKeyAndCertificatesData()
	{
		using var worker = new SshDataWorker();
		var args = _algorithm.ExportParameters(false);

		worker.Write(Name, Encoding.ASCII);
		worker.WriteMpint(args.Exponent);
		worker.WriteMpint(args.Modulus);

		return worker.ToByteArray();
	}

	public override bool VerifyData(byte[] data, byte[] signature)
	{
		return _algorithm.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
	}

	public override bool VerifyHash(byte[] hash, byte[] signature)
	{
		return _algorithm.VerifyHash(hash, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
	}

	public override byte[] SignData(byte[] data)
	{
		return _algorithm.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
	}

	public override byte[] SignHash(byte[] hash)
	{
		return _algorithm.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
	}
}
