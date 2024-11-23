﻿using System.Security.Cryptography;
using System.Text;

namespace PanoramicData.SshServer.Algorithms;

public class DssKey(string key = null) : PublicKeyAlgorithm(key)
{
	private readonly DSACryptoServiceProvider _algorithm = new();

	public override string Name
	{
		get { return "ssh-dss"; }
	}

	public override void ImportKey(byte[] bytes)
	{
		_algorithm.ImportCspBlob(bytes);
	}

	public override byte[] ExportKey()
	{
		return _algorithm.ExportCspBlob(true);
	}

	public override void LoadKeyAndCertificatesData(byte[] data)
	{
		using var worker = new SshDataWorker(data);
		if (worker.ReadString(Encoding.ASCII) != Name)
			throw new CryptographicException("Key and certificates were not created with this algorithm.");

		var args = new DSAParameters
		{
			P = worker.ReadMpint(),
			Q = worker.ReadMpint(),
			G = worker.ReadMpint(),
			Y = worker.ReadMpint()
		};

		_algorithm.ImportParameters(args);
	}

	public override byte[] CreateKeyAndCertificatesData()
	{
		using var worker = new SshDataWorker();
		var args = _algorithm.ExportParameters(false);

		worker.Write(Name, Encoding.ASCII);
		worker.WriteMpint(args.P);
		worker.WriteMpint(args.Q);
		worker.WriteMpint(args.G);
		worker.WriteMpint(args.Y);

		return worker.ToByteArray();
	}

	public override bool VerifyData(byte[] data, byte[] signature)
	{
		return _algorithm.VerifyData(data, signature);
	}

	public override bool VerifyHash(byte[] hash, byte[] signature)
	{
		return _algorithm.VerifyHash(hash, "SHA1", signature);
	}

	public override byte[] SignData(byte[] data)
	{
		return _algorithm.SignData(data);
	}

	public override byte[] SignHash(byte[] hash)
	{
		return _algorithm.SignHash(hash, "SHA1");
	}
}
