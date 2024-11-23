using System.Diagnostics.Contracts;
using System.Security.Cryptography;

namespace PanoramicData.SshServer.Algorithms;

public class DiffieHellmanGroupSha512 : KexAlgorithm
{
	private readonly DiffieHellman _exchangeAlgorithm;

	public DiffieHellmanGroupSha512(DiffieHellman algorithm)
	{
		Contract.Requires(algorithm != null);

		_exchangeAlgorithm = algorithm;
		_hashAlgorithm = SHA512.Create();
	}

	public override byte[] CreateKeyExchange() => _exchangeAlgorithm.CreateKeyExchange();

	public override byte[] DecryptKeyExchange(byte[] exchangeData) => _exchangeAlgorithm.DecryptKeyExchange(exchangeData);
}
