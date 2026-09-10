using System;
using System.Security.Cryptography;
using System.Text;

namespace PanoramicData.SshServer.Algorithms;

/// <summary>
/// Base class for SSH public key algorithms.
/// </summary>
public abstract class PublicKeyAlgorithm
{
	private readonly string? _base64Key;

	/// <summary>
	/// Initializes a new instance of the <see cref="PublicKeyAlgorithm"/> class.
	/// </summary>
	/// <param name="key">The optional base64-encoded key.</param>
	/// <remarks>
	/// The key is only stored here, not imported. Importing it would mean calling the
	/// overridable <see cref="ImportKey"/> while the derived type is still being constructed,
	/// so derived types call <see cref="ImportConstructorKey"/> from their own constructor
	/// instead, once their own state is in place.
	/// </remarks>
	protected PublicKeyAlgorithm(string? key) => _base64Key = key;

	/// <summary>
	/// Imports the base64-encoded key that was passed to the constructor, if there was one.
	/// </summary>
	/// <remarks>Derived types must call this from their own constructor.</remarks>
	protected void ImportConstructorKey()
	{
		if (!string.IsNullOrEmpty(_base64Key))
		{
			ImportKey(Convert.FromBase64String(_base64Key));
		}
	}

	/// <summary>
	/// Reads the algorithm name from the given reader and checks that it names this algorithm.
	/// </summary>
	/// <param name="reader">The reader positioned at the algorithm name.</param>
	/// <exception cref="CryptographicException">The name does not match <see cref="Name"/>.</exception>
	protected void ReadAndVerifyName(SshDataWorker reader)
	{
		ArgumentNullException.ThrowIfNull(reader);

		if (reader.ReadString(Encoding.ASCII) != Name)
		{
			throw new CryptographicException("Key and certificates were not created with this algorithm.");
		}
	}

	/// <summary>
	/// Writes this algorithm name followed by the given key components as SSH mpints.
	/// </summary>
	/// <param name="components">The key components, in wire order.</param>
	/// <returns>The key and certificates data.</returns>
	protected byte[] WriteKeyAndCertificatesData(params byte[][] components)
	{
		ArgumentNullException.ThrowIfNull(components);

		using var worker = new SshDataWorker();
		worker.Write(Name, Encoding.ASCII);
		foreach (var component in components)
		{
			worker.WriteMpint(component);
		}

		return worker.ToByteArray();
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
		{
			throw new CryptographicException("Signature was not created with this algorithm.");
		}

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
