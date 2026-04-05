using System;
using System.Security.Cryptography;
using System.Text;

namespace PanoramicData.SshServer.Algorithms;

/// <summary>
/// Base class for SSH public key algorithms.
/// </summary>
public abstract class PublicKeyAlgorithm
{
	/// <summary>
	/// Initializes a new instance of the <see cref="PublicKeyAlgorithm"/> class.
	/// </summary>
	/// <param name="key">The optional base64-encoded key.</param>
	public PublicKeyAlgorithm(string? key)
	{
		if (!string.IsNullOrEmpty(key))
		{
			var bytes = Convert.FromBase64String(key);
			ImportKey(bytes);
		}
	}

	/// <summary>
	/// Gets the algorithm name.
	/// </summary>
	public abstract string Name { get; }

	/// <summary>
	/// Gets the key fingerprint.
	/// </summary>
	/// <returns>The fingerprint string.</returns>
	public string GetFingerprint()
	{
		var bytes = MD5.HashData(CreateKeyAndCertificatesData());
		return BitConverter.ToString(bytes).Replace('-', ':');
	}

	/// <summary>
	/// Extracts the signature from signature data.
	/// </summary>
	/// <param name="signatureData">The signature data.</param>
	/// <returns>The extracted signature.</returns>
	public byte[] GetSignature(byte[] signatureData)
	{
		ArgumentNullException.ThrowIfNull(signatureData);

		using var worker = new SshDataWorker(signatureData);
		if (worker.ReadString(Encoding.ASCII) != Name)
			throw new CryptographicException("Signature was not created with this algorithm.");

		var signature = worker.ReadBinary();
		return signature;
	}

	/// <summary>
	/// Creates signature data from the given data.
	/// </summary>
	/// <param name="data">The data to sign.</param>
	/// <returns>The signature data.</returns>
	public byte[] CreateSignatureData(byte[] data)
	{
		ArgumentNullException.ThrowIfNull(data);

		using var worker = new SshDataWorker();
		var signature = SignData(data);

		worker.Write(Name, Encoding.ASCII);
		worker.WriteBinary(signature);

		return worker.ToByteArray();
	}

	/// <summary>
	/// Imports a key from bytes.
	/// </summary>
	/// <param name="bytes">The key bytes.</param>
	public abstract void ImportKey(byte[] bytes);

	/// <summary>
	/// Exports the key as bytes.
	/// </summary>
	/// <returns>The key bytes.</returns>
	public abstract byte[] ExportKey();

	/// <summary>
	/// Loads key and certificates data.
	/// </summary>
	/// <param name="data">The key and certificates data.</param>
	public abstract void LoadKeyAndCertificatesData(byte[] data);

	/// <summary>
	/// Creates key and certificates data.
	/// </summary>
	/// <returns>The key and certificates data.</returns>
	public abstract byte[] CreateKeyAndCertificatesData();

	/// <summary>
	/// Verifies data against a signature.
	/// </summary>
	/// <param name="data">The data to verify.</param>
	/// <param name="signature">The signature.</param>
	/// <returns>True if the signature is valid.</returns>
	public abstract bool VerifyData(byte[] data, byte[] signature);

	/// <summary>
	/// Verifies a hash against a signature.
	/// </summary>
	/// <param name="hash">The hash to verify.</param>
	/// <param name="signature">The signature.</param>
	/// <returns>True if the signature is valid.</returns>
	public abstract bool VerifyHash(byte[] hash, byte[] signature);

	/// <summary>
	/// Signs data.
	/// </summary>
	/// <param name="data">The data to sign.</param>
	/// <returns>The signature.</returns>
	public abstract byte[] SignData(byte[] data);

	/// <summary>
	/// Signs a hash.
	/// </summary>
	/// <param name="hash">The hash to sign.</param>
	/// <returns>The signature.</returns>
	public abstract byte[] SignHash(byte[] hash);
}
