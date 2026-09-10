using System;
using System.Text;

namespace PanoramicData.SshServer.Messages.Userauth;

/// <summary>
/// Represents an SSH password authentication request message.
/// </summary>
public class PasswordRequestMessage : RequestMessage
{
	/// <summary>
	/// Gets the password.
	/// </summary>
	public string? Password { get; private set; }

	/// <inheritdoc />
	protected override void OnLoad(SshDataWorker reader)
	{
		base.OnLoad(reader);

		if (MethodName != "password")
		{
			throw new ArgumentException(string.Format("Method name {0} is not valid.", MethodName));
		}

		// RFC 4252 section 8: FALSE here means "password", TRUE means "change password",
		// which this server does not support. The flag still has to be consumed so that
		// the reader stays aligned with the rest of the payload.
		_ = reader.ReadBoolean();
		Password = reader.ReadString(Encoding.ASCII);
	}
}
