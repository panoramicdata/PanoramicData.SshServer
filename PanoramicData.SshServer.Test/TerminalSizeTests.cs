namespace PanoramicData.SshServer.Test;

public class TerminalSizeTests
{
	[Fact]
	public void ConstructorSetsAllProperties()
	{
		var size = new TerminalSize(120, 40, 960, 640);

		Assert.Equal(120u, size.WidthColumns);
		Assert.Equal(40u, size.HeightRows);
		Assert.Equal(960u, size.PixelWidth);
		Assert.Equal(640u, size.PixelHeight);
	}

	[Fact]
	public void ConstructorDefaultTerminalSize()
	{
		var size = new TerminalSize(80, 25, 640, 480);

		Assert.Equal(80u, size.WidthColumns);
		Assert.Equal(25u, size.HeightRows);
		Assert.Equal(640u, size.PixelWidth);
		Assert.Equal(480u, size.PixelHeight);
	}

	[Fact]
	public void ConstructorZeroValues()
	{
		var size = new TerminalSize(0, 0, 0, 0);

		Assert.Equal(0u, size.WidthColumns);
		Assert.Equal(0u, size.HeightRows);
		Assert.Equal(0u, size.PixelWidth);
		Assert.Equal(0u, size.PixelHeight);
	}
}
