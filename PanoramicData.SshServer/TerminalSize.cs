namespace PanoramicData.SshServer;

public class TerminalSize(uint width, uint height, uint pixelWidth, uint pixelHeight)
{
	public uint WidthColumns { get; } = width;

	public uint HeightRows { get; } = height;

	public uint PixelWidth { get; } = pixelWidth;

	public uint PixelHeight { get; } = pixelHeight;
}