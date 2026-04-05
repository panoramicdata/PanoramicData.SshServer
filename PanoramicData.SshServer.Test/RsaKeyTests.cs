using PanoramicData.SshServer.Algorithms;
using System.Text;

namespace PanoramicData.SshServer.Test;

public class RsaKeyTests
{
	[Fact]
	public void NameReturnsExpectedAlgorithm()
	{
		var key = new RsaKey();

		Assert.Equal("rsa-sha2-256", key.Name);
	}

	[Fact]
	public void ExportKeyReturnsNonEmptyBytes()
	{
		var key = new RsaKey();

		var exported = key.ExportKey();

		Assert.NotNull(exported);
		Assert.NotEmpty(exported);
	}

	[Fact]
	public void SignDataVerifyDataRoundtrips()
	{
		var key = new RsaKey();
		var data = Encoding.UTF8.GetBytes("test data to sign");

		var signature = key.SignData(data);

		Assert.NotNull(signature);
		Assert.NotEmpty(signature);
		Assert.True(key.VerifyData(data, signature));
	}

	[Fact]
	public void VerifyDataWrongDataReturnsFalse()
	{
		var key = new RsaKey();
		var data = Encoding.UTF8.GetBytes("original data");
		var signature = key.SignData(data);

		var tamperedData = Encoding.UTF8.GetBytes("tampered data");

		Assert.False(key.VerifyData(tamperedData, signature));
	}

	[Fact]
	public void CreateKeyAndCertificatesDataLoadKeyAndCertificatesDataRoundtrips()
	{
		var key1 = new RsaKey();
		var certData = key1.CreateKeyAndCertificatesData();

		var key2 = new RsaKey();
		key2.LoadKeyAndCertificatesData(certData);

		// The loaded key should be able to verify data signed by the original
		// (public key operations only - LoadKeyAndCertificatesData loads public key)
		var data = Encoding.UTF8.GetBytes("test data");
		var signature = key1.SignData(data);
		Assert.True(key2.VerifyData(data, signature));
	}

	[Fact]
	public void GetFingerprintReturnsNonEmpty()
	{
		var key = new RsaKey();

		var fingerprint = key.GetFingerprint();

		Assert.False(string.IsNullOrWhiteSpace(fingerprint));
	}

	[Fact]
	public void GetSignatureExtractsSignatureFromData()
	{
		var key = new RsaKey();
		var data = Encoding.UTF8.GetBytes("test");
		var signatureData = key.CreateSignatureData(data);

		var signature = key.GetSignature(signatureData);

		Assert.NotNull(signature);
		Assert.NotEmpty(signature);
	}
}
