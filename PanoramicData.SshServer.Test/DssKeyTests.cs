using PanoramicData.SshServer.Algorithms;
using System;

namespace PanoramicData.SshServer.Test;

public class DssKeyTests
{
	[Fact]
	public void NameReturnsExpectedAlgorithm()
	{
		var key = new DssKey();

		Assert.Equal("ssh-dss", key.Name);
	}

	[Fact]
	public void ConstructorWithBase64KeyImportsThatKey()
	{
		var exported = Convert.ToBase64String(new DssKey().ExportKey());

		var key = new DssKey(exported);

		// The key the constructor imported must be the one that was exported, not a fresh one.
		Assert.Equal(exported, Convert.ToBase64String(key.ExportKey()));
	}

	[Fact]
	public void ConstructorWithBase64KeyProducesUsableSigningKey()
	{
		var source = new DssKey();
		var key = new DssKey(Convert.ToBase64String(source.ExportKey()));
		var data = "the quick brown fox"u8.ToArray();

		var signature = key.SignData(data);

		Assert.True(source.VerifyData(data, signature));
	}

	[Fact]
	public void ConstructorWithoutKeyGeneratesItsOwnKey()
	{
		var key1 = new DssKey();
		var key2 = new DssKey();

		Assert.NotEqual(key1.GetFingerprint(), key2.GetFingerprint());
	}

	[Fact]
	public void KeyAndCertificatesDataRoundtrips()
	{
		var source = new DssKey();
		var data = source.CreateKeyAndCertificatesData();

		var loaded = new DssKey();
		loaded.LoadKeyAndCertificatesData(data);

		Assert.Equal(source.GetFingerprint(), loaded.GetFingerprint());
	}
}
