using PanoramicData.SshServer.Algorithms;
using PanoramicData.SshServer.Services;
using System;
using System.Text;

namespace PanoramicData.SshServer;

/// <summary>
/// Algorithm negotiation, exchange hashes and key derivation.
/// </summary>
public partial class Session
{
	private static string ChooseAlgorithm(string[] serverAlgorithms, string[]? clientAlgorithms)
	{
		if (clientAlgorithms is null)
		{
			throw new SshConnectionException("Failed to negotiate algorithm.", DisconnectReason.KeyExchangeFailed);
		}

		foreach (var client in clientAlgorithms)
		{
			foreach (var server in serverAlgorithms)
			{
				if (client == server)
				{
					return client;
				}
			}
		}

		throw new SshConnectionException("Failed to negotiate algorithm.", DisconnectReason.KeyExchangeFailed);
	}

	private byte[] ComputeExchangeHash(KexAlgorithm kexAlg, byte[] hostKeyAndCerts, byte[] clientExchangeValue, byte[] serverExchangeValue, byte[] sharedSecret)
	{
		using var worker = new SshDataWorker();
		worker.Write(ClientVersion, Encoding.ASCII);
		worker.Write(ServerVersion, Encoding.ASCII);
		worker.WriteBinary(_exchangeContext!.ClientKexInitPayload!);
		worker.WriteBinary(_exchangeContext.ServerKexInitPayload!);
		worker.WriteBinary(hostKeyAndCerts);
		worker.WriteMpint(clientExchangeValue);
		worker.WriteMpint(serverExchangeValue);
		worker.WriteMpint(sharedSecret);

		return kexAlg.ComputeHash(worker.ToByteArray());
	}

	private byte[] ComputeEncryptionKey(
		KexAlgorithm kexAlg,
		byte[] exchangeHash,
		int blockSize,
		byte[] sharedSecret,
		char letter)
	{
		var keyBuffer = new byte[blockSize];
		var keyBufferIndex = 0;
		byte[]? currentHash = null;

		while (keyBufferIndex < blockSize)
		{
			using (var worker = new SshDataWorker())
			{
				worker.WriteMpint(sharedSecret);
				worker.Write(exchangeHash);

				if (currentHash == null)
				{
					worker.Write((byte)letter);
					worker.Write(ExchangeHash!);
				}
				else
				{
					worker.Write(currentHash);
				}

				currentHash = kexAlg.ComputeHash(worker.ToByteArray());
			}

			var currentHashLength = Math.Min(currentHash.Length, blockSize - keyBufferIndex);
			Array.Copy(currentHash, 0, keyBuffer, keyBufferIndex, currentHashLength);

			keyBufferIndex += currentHashLength;
		}

		return keyBuffer;
	}

	private static byte[] ComputeHmac(HmacAlgorithm alg, byte[] payload, uint seq)
	{
		using var worker = new SshDataWorker();
		worker.Write(seq);
		worker.Write(payload);

		return alg.ComputeHash(worker.ToByteArray());
	}
}
