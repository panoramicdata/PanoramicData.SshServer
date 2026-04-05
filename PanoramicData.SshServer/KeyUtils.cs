using PanoramicData.SshServer.Algorithms;
using System;
using System.Security.Cryptography;

namespace PanoramicData.SshServer;

/// <summary>
/// Provides utility methods for SSH key operations.
/// </summary>
public static class KeyUtils
{
	/// <summary>
	/// Gets the fingerprint for the specified SSH key.
	/// </summary>
	/// <param name="sshkey">The base64-encoded SSH key.</param>
	/// <returns>The fingerprint string.</returns>
	public static string GetFingerprint(string sshkey)
	{
		ArgumentNullException.ThrowIfNull(sshkey);
		var bytes = Convert.FromBase64String(sshkey);
		bytes = MD5.HashData(bytes);
		return BitConverter.ToString(bytes).Replace('-', ':');
	}

	private static PublicKeyAlgorithm GetKeyAlgorithm(string type) => type switch
	{
		"rsa-sha2-256" => new RsaKey(),
		"ssh-dss" => new DssKey(),
		_ => throw new ArgumentOutOfRangeException(nameof(type)),
	};

	/// <summary>
	/// Generates a new private key of the specified type.
	/// </summary>
	/// <param name="type">The key type.</param>
	/// <returns>The base64-encoded private key.</returns>
	public static string GeneratePrivateKey(string type)
	{
		ArgumentNullException.ThrowIfNull(type);

		var alg = GetKeyAlgorithm(type);
		var bytes = alg.ExportKey();
		return Convert.ToBase64String(bytes);
	}

	/// <summary>
	/// Gets the list of supported key algorithms.
	/// </summary>
	public static string[] SupportedAlgorithms => ["rsa-sha2-256", "ssh-dss"];
}
