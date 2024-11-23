using PanoramicData.SshServer.Algorithms;
using System;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;

namespace PanoramicData.SshServer;

public static class KeyUtils
{
	public static string GetFingerprint(string sshkey)
	{
		Contract.Requires(sshkey != null);
		var bytes = Convert.FromBase64String(sshkey);
		bytes = MD5.HashData(bytes);
		return BitConverter.ToString(bytes).Replace('-', ':');
	}

	private static PublicKeyAlgorithm GetKeyAlgorithm(string type) => type switch
	{
		"rsa-sha2-256" => new RsaKey(),
		_ => throw new ArgumentOutOfRangeException(nameof(type)),
	};

	public static string GeneratePrivateKey(string type)
	{
		Contract.Requires(type != null);

		var alg = GetKeyAlgorithm(type);
		var bytes = alg.ExportKey();
		return Convert.ToBase64String(bytes);
	}

	public static string[] SupportedAlgorithms => ["rsa-sha2-256"];
}
