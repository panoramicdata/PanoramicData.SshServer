using PanoramicData.SshServer.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace ExampleApp;

internal class ExampleKeyManager : IKeyManager
{
	public async Task<Dictionary<string, string>> GetHostKeysAsync(CancellationToken cancellationToken)
		=> new Dictionary<string, string>
		{
			{ "rsa-sha2-256", await GetPrivateKeyBase64Async("rsa-sha2-256", cancellationToken) },
			{ "ssh-dss", await GetPrivateKeyBase64Async("ssh-dss", cancellationToken) },
		};

	private static async Task<string> GetPrivateKeyBase64Async(string fileNameRoot, CancellationToken cancellationToken)
	{
		var publicKeyFileInfo = new FileInfo($"{fileNameRoot}.public.txt");
		var privateKeyFileInfo = new FileInfo($"{fileNameRoot}.private.txt");

		string privateKeyBase64;

		// Does the private key file exist?
		if (privateKeyFileInfo.Exists)
		{
			// Read the private key
			privateKeyBase64 = await File.ReadAllTextAsync(privateKeyFileInfo.FullName, cancellationToken);
		}
		else
		{
			switch (fileNameRoot)
			{
				case "rsa-sha2-256":
					{
						// Generate a new key pair
						var rsa = RSA.Create(2048);

						// Save the private key
						var privateKey = rsa.ExportRSAPrivateKey();
						privateKeyBase64 = Convert.ToBase64String(privateKey);
						await File.WriteAllTextAsync(privateKeyFileInfo.FullName, privateKeyBase64, cancellationToken);

						// Save the public key for reference
						var publicKey = rsa.ExportRSAPublicKey();
						var publicKeyBase64 = Convert.ToBase64String(publicKey);
						await File.WriteAllTextAsync(publicKeyFileInfo.FullName, publicKeyBase64, cancellationToken);
						break;
					}
				case "ssh-dss":
					{
						// Generate a new key pair
						var dsa = DSA.Create(2048);
						// Save the private key
						var privateKey = dsa.ExportPkcs8PrivateKey();
						privateKeyBase64 = Convert.ToBase64String(privateKey);
						await File.WriteAllTextAsync(privateKeyFileInfo.FullName, privateKeyBase64, cancellationToken);
						// Save the public key for reference
						var publicKey = dsa.ExportSubjectPublicKeyInfo();
						var publicKeyBase64 = Convert.ToBase64String(publicKey);
						await File.WriteAllTextAsync(publicKeyFileInfo.FullName, publicKeyBase64, cancellationToken);
						break;
					}
				default:
					throw new InvalidOperationException($"Unknown key type {fileNameRoot}");
			}
		}

		return privateKeyBase64;
	}
}