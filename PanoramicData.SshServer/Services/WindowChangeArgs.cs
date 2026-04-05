namespace PanoramicData.SshServer.Services;

/// <summary>
/// Contains arguments for a window change event.
/// </summary>
public class WindowChangeArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="WindowChangeArgs"/> class.
	/// </summary>
	/// <param name="channel">The session channel.</param>
	/// <param name="widthColumns">The terminal width in columns.</param>
	/// <param name="heightRows">The terminal height in rows.</param>
	/// <param name="widthPixels">The terminal width in pixels.</param>
	/// <param name="heightPixels">The terminal height in pixels.</param>
	public WindowChangeArgs(SessionChannel channel, uint widthColumns, uint heightRows, uint widthPixels, uint heightPixels)
	{
		ArgumentNullException.ThrowIfNull(channel);

		Channel = channel;
		WidthColumns = widthColumns;
		HeightRows = heightRows;
		WidthPixels = widthPixels;
		HeightPixels = heightPixels;
	}

	/// <summary>
	/// Gets the session channel.
	/// </summary>
	public SessionChannel Channel { get; private set; }

	/// <summary>
	/// Gets the terminal width in columns.
	/// </summary>
	public uint WidthColumns { get; private set; }

	/// <summary>
	/// Gets the terminal height in rows.
	/// </summary>
	public uint HeightRows { get; private set; }

	/// <summary>
	/// Gets the terminal width in pixels.
	/// </summary>
	public uint WidthPixels { get; private set; }

	/// <summary>
	/// Gets the terminal height in pixels.
	/// </summary>
	public uint HeightPixels { get; private set; }
}
