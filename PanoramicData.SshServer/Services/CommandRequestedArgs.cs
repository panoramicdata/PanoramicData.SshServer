namespace PanoramicData.SshServer.Services;

/// <summary>
/// Contains arguments for a command request event.
/// </summary>
public class CommandRequestedArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="CommandRequestedArgs"/> class.
	/// </summary>
	/// <param name="channel">The session channel.</param>
	/// <param name="type">The shell type.</param>
	/// <param name="command">The command text.</param>
	/// <param name="userauthArgs">The user authentication arguments.</param>
	public CommandRequestedArgs(SessionChannel channel, string type, string command, UserAuthArgs userauthArgs)
	{
		ArgumentNullException.ThrowIfNull(channel);
		ArgumentNullException.ThrowIfNull(command);
		ArgumentNullException.ThrowIfNull(userauthArgs);

		Channel = channel;
		ShellType = type;
		CommandText = command;
		AttachedUserauthArgs = userauthArgs;
	}

	/// <summary>
	/// Gets the session channel.
	/// </summary>
	public SessionChannel Channel { get; private set; }

	/// <summary>
	/// Gets the shell type.
	/// </summary>
	public string ShellType { get; private set; }

	/// <summary>
	/// Gets the command text.
	/// </summary>
	public string CommandText { get; private set; }

	/// <summary>
	/// Gets the attached user authentication arguments.
	/// </summary>
	public UserAuthArgs AttachedUserauthArgs { get; private set; }
}
