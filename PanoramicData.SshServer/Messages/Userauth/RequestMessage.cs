using System.Text;

namespace PanoramicData.SshServer.Messages.Userauth;

/// <summary>
/// Represents an SSH user authentication request message.
/// </summary>
[Message("SSH_MSG_USERAUTH_REQUEST", MessageNumber)]
public class RequestMessage : UserAuthServiceMessage
{
	/// <summary>
	/// The message number for user authentication requests.
	/// </summary>
	protected const byte MessageNumber = 50;

	/// <summary>
	/// Gets the username.
	/// </summary>
	public string? Username { get; protected set; }

	/// <summary>
	/// Gets the service name.
	/// </summary>
	public string? ServiceName { get; protected set; }

	/// <summary>
	/// Gets the authentication method name.
	/// </summary>
	public string? MethodName { get; protected set; }

	/// <inheritdoc />
	public override byte MessageType => MessageNumber;

	/// <inheritdoc />
	protected override void OnLoad(SshDataWorker reader)
	{
		Username = reader.ReadString(Encoding.UTF8);
		ServiceName = reader.ReadString(Encoding.ASCII);
		MethodName = reader.ReadString(Encoding.ASCII);
	}
}
