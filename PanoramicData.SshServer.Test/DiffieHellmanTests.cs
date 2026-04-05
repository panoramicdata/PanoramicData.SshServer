using PanoramicData.SshServer.Algorithms;

namespace PanoramicData.SshServer.Test;

public class DiffieHellmanTests
{
	[Theory]
	[InlineData(1024)]
	[InlineData(2048)]
	public void CreateKeyExchangeReturnsNonEmptyBytes(int keySize)
	{
		var dh = new DiffieHellman(keySize);
		var kex = new DiffieHellmanGroupSha256(dh);

		var exchange = kex.CreateKeyExchange();

		Assert.NotNull(exchange);
		Assert.NotEmpty(exchange);
	}

	[Fact]
	public void DecryptKeyExchangeReturnsSharedSecret()
	{
		var dh1 = new DiffieHellman(1024);
		var kex1 = new DiffieHellmanGroupSha256(dh1);

		var dh2 = new DiffieHellman(1024);
		var kex2 = new DiffieHellmanGroupSha256(dh2);

		var exchange1 = kex1.CreateKeyExchange();
		var exchange2 = kex2.CreateKeyExchange();

		var secret1 = kex1.DecryptKeyExchange(exchange2);
		var secret2 = kex2.DecryptKeyExchange(exchange1);

		Assert.Equal(secret1, secret2);
	}

	[Fact]
	public void ComputeHashReturnsConsistentResult()
	{
		var dh = new DiffieHellman(1024);
		var kex = new DiffieHellmanGroupSha256(dh);

		var input = new byte[] { 1, 2, 3, 4, 5 };

		var hash1 = kex.ComputeHash(input);
		var hash2 = kex.ComputeHash(input);

		Assert.Equal(hash1, hash2);
		Assert.Equal(32, hash1.Length); // SHA-256 produces 32 bytes
	}

	[Fact]
	public void DiffieHellmanGroupSha512ComputeHashReturns64Bytes()
	{
		var dh = new DiffieHellman(1024);
		var kex = new DiffieHellmanGroupSha512(dh);

		var hash = kex.ComputeHash([1, 2, 3]);

		Assert.Equal(64, hash.Length); // SHA-512 produces 64 bytes
	}

	[Fact]
	public void DiffieHellmanGroupSha1ComputeHashReturns20Bytes()
	{
		var dh = new DiffieHellman(1024);
		var kex = new DiffieHellmanGroupSha1(dh);

		var hash = kex.ComputeHash([1, 2, 3]);

		Assert.Equal(20, hash.Length); // SHA-1 produces 20 bytes
	}
}
