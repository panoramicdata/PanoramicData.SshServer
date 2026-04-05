namespace PanoramicData.SshServer;

/// <summary>
/// Represents the size of an SSH terminal.
/// </summary>
/// <param name="width">The width in columns.</param>
/// <param name="height">The height in rows.</param>
/// <param name="pixelWidth">The width in pixels.</param>
/// <param name="pixelHeight">The height in pixels.</param>
public class TerminalSize(uint width, uint height, uint pixelWidth, uint pixelHeight)
{
	/// <summary>
	/// Gets the width in columns.
	/// </summary>
	public uint WidthColumns { get; } = width;

	/// <summary>
	/// Gets the height in rows.
	/// </summary>
	public uint HeightRows { get; } = height;

	/// <summary>
	/// Gets the width in pixels.
	/// </summary>
	public uint PixelWidth { get; } = pixelWidth;

	/// <summary>
	/// Gets the height in pixels.
	/// </summary>
	public uint PixelHeight { get; } = pixelHeight;
}