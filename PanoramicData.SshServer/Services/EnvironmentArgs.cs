using System.Diagnostics.Contracts;

namespace PanoramicData.SshServer.Services;

public class EnvironmentArgs
{
	public EnvironmentArgs(SessionChannel channel, string name, string value, UserAuthArgs userauthArgs)
	{
		Contract.Requires(channel != null);
		Contract.Requires(name != null);
		Contract.Requires(value != null);
		Contract.Requires(userauthArgs != null);

		Channel = channel;
		Name = name;
		Value = value;
		AttachedUserauthArgs = userauthArgs;
	}

	public SessionChannel Channel { get; private set; }
	public string Name { get; private set; }
	public string Value { get; private set; }
	public UserAuthArgs AttachedUserauthArgs { get; private set; }
}
