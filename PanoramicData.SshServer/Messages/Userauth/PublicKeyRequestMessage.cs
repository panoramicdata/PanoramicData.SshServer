using System;
using System.Linq;
using System.Text;

namespace PanoramicData.SshServer.Messages.Userauth;

/// <summary>
/// Represents an SSH public key authentication request message.
/// </summary>
public class PublicKeyRequestMessage : RequestMessage
{
	/// <summary>
	/// Gets a value indicating whether the request has a signature.
	/// </summary>
	public bool HasSignature { get; private set; }

	/// <summary>
	/// Gets the key algorithm name.
	/// </summary>
	public string? KeyAlgorithmName { get; private set; }

	/// <summary>
	/// Gets the public key data.
	/// </summary>
	public byte[]? PublicKey { get; private set; }

	/// <summary>
	/// Gets the signature data.
	/// </summary>
	public byte[]? Signature { get; private set; }

	/// <summary>
	/// Gets the payload without the signature.
	/// </summary>
	public byte[]? PayloadWithoutSignature { get; private set; }

	/// <inheritdoc />
	protected override void OnLoad(SshDataWorker reader)
	{
		base.OnLoad(reader);

		if (MethodName != "publickey")
			throw new ArgumentException(string.Format("Method name {0} is not valid.", MethodName));

		HasSignature = reader.ReadBoolean();
		KeyAlgorithmName = reader.ReadString(Encoding.ASCII);
		PublicKey = reader.ReadBinary();

		if (HasSignature)
		{
			Signature = reader.ReadBinary();
			PayloadWithoutSignature = [.. RawBytes!.Take(RawBytes!.Length - Signature!.Length - 5)];
		}
	}
}
