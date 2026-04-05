namespace PanoramicData.SshServer.Services;

/// <summary>
/// Contains arguments for a PTY request event.
/// </summary>
public class PtyArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="PtyArgs"/> class.
	/// </summary>
	/// <param name="channel">The session channel.</param>
	/// <param name="terminal">The terminal type.</param>
	/// <param name="heightPx">The terminal height in pixels.</param>
	/// <param name="heightRows">The terminal height in rows.</param>
	/// <param name="widthPx">The terminal width in pixels.</param>
	/// <param name="widthChars">The terminal width in characters.</param>
	/// <param name="modes">The terminal modes.</param>
	/// <param name="userauthArgs">The user authentication arguments.</param>
	public PtyArgs(SessionChannel channel, string terminal, uint heightPx, uint heightRows, uint widthPx, uint widthChars, string modes, UserAuthArgs userauthArgs)
	{
		ArgumentNullException.ThrowIfNull(channel);
		ArgumentNullException.ThrowIfNull(terminal);
		ArgumentNullException.ThrowIfNull(modes);
		ArgumentNullException.ThrowIfNull(userauthArgs);

		Channel = channel;
		Terminal = terminal;
		HeightPx = heightPx;
		HeightRows = heightRows;
		WidthPx = widthPx;
		WidthChars = widthChars;
		Modes = modes;

		AttachedUserauthArgs = userauthArgs;
	}

	/// <summary>
	/// Gets the session channel.
	/// </summary>
	public SessionChannel Channel { get; private set; }

	/// <summary>
	/// Gets the terminal type.
	/// </summary>
	public string Terminal { get; private set; }

	/// <summary>
	/// Gets the terminal height in pixels.
	/// </summary>
	public uint HeightPx { get; private set; }

	/// <summary>
	/// Gets the terminal height in rows.
	/// </summary>
	public uint HeightRows { get; private set; }

	/// <summary>
	/// Gets the terminal width in pixels.
	/// </summary>
	public uint WidthPx { get; private set; }

	/// <summary>
	/// Gets the terminal width in characters.
	/// </summary>
	public uint WidthChars { get; private set; }

	/// <summary>
	/// Gets the terminal modes.
	/// </summary>
	public string Modes { get; private set; }

	/// <summary>
	/// Gets the attached user authentication arguments.
	/// </summary>
	public UserAuthArgs AttachedUserauthArgs { get; private set; }
}
