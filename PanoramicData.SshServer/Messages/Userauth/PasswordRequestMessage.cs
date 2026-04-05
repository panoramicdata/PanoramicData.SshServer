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
			throw new ArgumentException(string.Format("Method name {0} is not valid.", MethodName));

		var isFalse = reader.ReadBoolean();
		Password = reader.ReadString(Encoding.ASCII);
	}
}
