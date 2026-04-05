namespace PanoramicData.SshServer.Test;

public class SshConnectionExceptionTests
{
	[Fact]
	public void DefaultConstructorHasNoMessage()
	{
		var ex = new SshConnectionException();

		Assert.NotNull(ex);
		Assert.Equal(DisconnectReason.None, ex.DisconnectReason);
	}

	[Fact]
	public void ConstructorWithMessageSetsMessage()
	{
		var ex = new SshConnectionException("test error");

		Assert.Equal("test error", ex.Message);
		Assert.Equal(DisconnectReason.None, ex.DisconnectReason);
	}

	[Fact]
	public void ConstructorWithMessageAndReasonSetsBoth()
	{
		var ex = new SshConnectionException("protocol error", DisconnectReason.ProtocolError);

		Assert.Equal("protocol error", ex.Message);
		Assert.Equal(DisconnectReason.ProtocolError, ex.DisconnectReason);
	}

	[Fact]
	public void ToStringContainsDisconnectReason()
	{
		var ex = new SshConnectionException("test", DisconnectReason.ConnectionLost);

		var result = ex.ToString();

		Assert.Contains("ConnectionLost", result);
	}

	[Theory]
	[InlineData(DisconnectReason.ProtocolError)]
	[InlineData(DisconnectReason.KeyExchangeFailed)]
	[InlineData(DisconnectReason.ByApplication)]
	[InlineData(DisconnectReason.ConnectionLost)]
	public void ConstructorAllReasonsRoundtrip(DisconnectReason reason)
	{
		var ex = new SshConnectionException("test", reason);

		Assert.Equal(reason, ex.DisconnectReason);
	}
}
