using System;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;
using System.Text;

namespace FxSsh.Algorithms;

[ContractClass(typeof(PublicKeyAlgorithmContract))]
public abstract class PublicKeyAlgorithm
{
	public PublicKeyAlgorithm(string key)
	{
		if (!string.IsNullOrEmpty(key))
		{
			var bytes = Convert.FromBase64String(key);
			ImportKey(bytes);
		}
	}

	public abstract string Name { get; }

	public string GetFingerprint()
	{
		var bytes = MD5.HashData(CreateKeyAndCertificatesData());
		return BitConverter.ToString(bytes).Replace('-', ':');
	}

	public byte[] GetSignature(byte[] signatureData)
	{
		Contract.Requires(signatureData != null);

		using var worker = new SshDataWorker(signatureData);
		if (worker.ReadString(Encoding.ASCII) != Name)
			throw new CryptographicException("Signature was not created with this algorithm.");

		var signature = worker.ReadBinary();
		return signature;
	}

	public byte[] CreateSignatureData(byte[] data)
	{
		Contract.Requires(data != null);

		using var worker = new SshDataWorker();
		var signature = SignData(data);

		worker.Write(Name, Encoding.ASCII);
		worker.WriteBinary(signature);

		return worker.ToByteArray();
	}

	public abstract void ImportKey(byte[] bytes);

	public abstract byte[] ExportKey();

	public abstract void LoadKeyAndCertificatesData(byte[] data);

	public abstract byte[] CreateKeyAndCertificatesData();

	public abstract bool VerifyData(byte[] data, byte[] signature);

	public abstract bool VerifyHash(byte[] hash, byte[] signature);

	public abstract byte[] SignData(byte[] data);

	public abstract byte[] SignHash(byte[] hash);
}
