namespace PanoramicData.SshServer.Services;

/// <summary>
/// Contains arguments for an environment variable event.
/// </summary>
public class EnvironmentArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="EnvironmentArgs"/> class.
	/// </summary>
	/// <param name="channel">The session channel.</param>
	/// <param name="name">The environment variable name.</param>
	/// <param name="value">The environment variable value.</param>
	/// <param name="userauthArgs">The user authentication arguments.</param>
	public EnvironmentArgs(SessionChannel channel, string name, string value, UserAuthArgs userauthArgs)
	{
		ArgumentNullException.ThrowIfNull(channel);
		ArgumentNullException.ThrowIfNull(name);
		ArgumentNullException.ThrowIfNull(value);
		ArgumentNullException.ThrowIfNull(userauthArgs);

		Channel = channel;
		Name = name;
		Value = value;
		AttachedUserauthArgs = userauthArgs;
	}

	/// <summary>
	/// Gets the session channel.
	/// </summary>
	public SessionChannel Channel { get; private set; }

	/// <summary>
	/// Gets the environment variable name.
	/// </summary>
	public string Name { get; private set; }

	/// <summary>
	/// Gets the environment variable value.
	/// </summary>
	public string Value { get; private set; }

	/// <summary>
	/// Gets the attached user authentication arguments.
	/// </summary>
	public UserAuthArgs AttachedUserauthArgs { get; private set; }
}
