namespace PanoramicData.SshServer.Test;

public class KeyUtilsTests
{
	[Theory]
	[InlineData("rsa-sha2-256")]
	[InlineData("ssh-dss")]
	public void GeneratePrivateKeySupportedTypeReturnsBase64(string type)
	{
		var key = KeyUtils.GeneratePrivateKey(type);

		Assert.False(string.IsNullOrWhiteSpace(key));
		// Should be valid base64
		var bytes = Convert.FromBase64String(key);
		Assert.NotEmpty(bytes);
	}

	[Fact]
	public void GeneratePrivateKeyUnsupportedTypeThrows()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => KeyUtils.GeneratePrivateKey("unsupported-key-type"));
	}

	[Fact]
	public void GetFingerprintReturnsColonSeparatedHex()
	{
		var key = KeyUtils.GeneratePrivateKey("rsa-sha2-256");
		var fingerprint = KeyUtils.GetFingerprint(key);

		Assert.False(string.IsNullOrWhiteSpace(fingerprint));
		Assert.Contains(':', fingerprint);
		// MD5 fingerprint should be 16 hex pairs separated by colons
		var parts = fingerprint.Split(':');
		Assert.Equal(16, parts.Length);
		Assert.All(parts, part => Assert.Equal(2, part.Length));
	}

	[Fact]
	public void GetFingerprintSameKeyReturnsSameFingerprint()
	{
		var key = KeyUtils.GeneratePrivateKey("rsa-sha2-256");

		var fingerprint1 = KeyUtils.GetFingerprint(key);
		var fingerprint2 = KeyUtils.GetFingerprint(key);

		Assert.Equal(fingerprint1, fingerprint2);
	}

	[Fact]
	public void GetFingerprintDifferentKeysReturnDifferentFingerprints()
	{
		var key1 = KeyUtils.GeneratePrivateKey("rsa-sha2-256");
		var key2 = KeyUtils.GeneratePrivateKey("rsa-sha2-256");

		var fingerprint1 = KeyUtils.GetFingerprint(key1);
		var fingerprint2 = KeyUtils.GetFingerprint(key2);

		Assert.NotEqual(fingerprint1, fingerprint2);
	}

	[Fact]
	public void SupportedAlgorithmsContainsExpectedTypes()
	{
		var algorithms = KeyUtils.SupportedAlgorithms;

		Assert.Contains("rsa-sha2-256", algorithms);
		Assert.Contains("ssh-dss", algorithms);
		Assert.Equal(2, algorithms.Length);
	}
}
