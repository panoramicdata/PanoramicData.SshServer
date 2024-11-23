using PanoramicData.SshServer.Services;
using PanoramicData.SshServer.Test.MiniTerm;
using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PanoramicData.SshServer.Test;

class Program
{
	static int windowWidth, windowHeight;

	static void Main()
	{
		var server = new SshServer(new StartingInfo(IPAddress.Any, 1022, "SSH-2.0-SshServerLoader"));
		server.AddHostKey("rsa-sha2-256", """
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
""".Replace("\r\n", ""));
		server.ConnectionAccepted += Server_ConnectionAccepted;

		server.Start();

		Task.Delay(-1).Wait();
	}

	static void Server_ConnectionAccepted(object sender, Session e)
	{
		Console.WriteLine("Accepted a client.");

		e.ServiceRegistered += E_ServiceRegistered;
		e.KeysExchanged += E_KeysExchanged;
	}

	private static void E_KeysExchanged(object sender, KeyExchangeArgs e)
	{
		foreach (var keyExchangeAlg in e.KeyExchangeAlgorithms)
		{
			Console.WriteLine("Key exchange algorithm: {0}", keyExchangeAlg);
		}
	}

	static void E_ServiceRegistered(object sender, SshService e)
	{
		var session = (Session)sender;
		Console.WriteLine("Session {0} requesting {1}.",
			Convert.ToHexString(session.SessionId), e.GetType().Name);

		switch (e)
		{
			case UserauthService:
				{
					var service = (UserauthService)e;
					service.Userauth += Service_Userauth;
					break;
				}

			case ConnectionService service:
				service.CommandOpened += Service_CommandOpened;
				service.EnvReceived += Service_EnvReceived;
				service.PtyReceived += Service_PtyReceived;
				service.TcpForwardRequest += Service_TcpForwardRequest;
				service.WindowChange += Service_WindowChange;
				break;
		}
	}

	static void Service_WindowChange(object sender, WindowChangeArgs e)
	{
		// DEMO MiniTerm not support change window size
	}

	static void Service_TcpForwardRequest(object sender, TcpRequestArgs e)
	{
		Console.WriteLine("Received a request to forward data to {0}:{1}", e.Host, e.Port);

		var allow = true;  // func(e.Host, e.Port, e.AttachedUserauthArgs);

		if (!allow)
			return;

		var tcp = new TcpForwardService(e.Host, e.Port, e.OriginatorIP, e.OriginatorPort);
		e.Channel.DataReceived += (ss, ee) => tcp.OnData(ee);
		e.Channel.CloseReceived += (ss, ee) => tcp.OnClose();
		tcp.DataReceived += (ss, ee) => e.Channel.SendData(ee);
		tcp.CloseReceived += (ss, ee) => e.Channel.SendClose();
		tcp.Start();
	}

	static void Service_PtyReceived(object sender, PtyArgs e)
	{
		Console.WriteLine("Request to create a PTY received for terminal type {0}", e.Terminal);
		windowWidth = (int)e.WidthChars;
		windowHeight = (int)e.HeightRows;
	}

	static void Service_EnvReceived(object sender, EnvironmentArgs e)
	{
		Console.WriteLine("Received environment variable {0}:{1}", e.Name, e.Value);
	}

	static void Service_Userauth(object sender, UserauthArgs e)
	{
		Console.WriteLine("Client {0} fingerprint: {1}.", e.KeyAlgorithm, e.Fingerprint);

		e.Result = true;
	}

	static void Service_CommandOpened(object sender, CommandRequestedArgs e)
	{
		Console.WriteLine($"Channel {e.Channel.ServerChannelId} runs {e.ShellType}: \"{e.CommandText}\".");

		var allow = true;  // func(e.ShellType, e.CommandText, e.AttachedUserauthArgs);

		if (!allow)
			return;

		if (e.ShellType == "shell")
		{
			// requirements: Windows 10 RedStone 5, 1809
			// also, you can call powershell.exe
			var terminal = new Terminal("cmd.exe", windowWidth, windowHeight);

			e.Channel.DataReceived += (ss, ee) => terminal.OnInput(ee);
			e.Channel.CloseReceived += (ss, ee) => terminal.OnClose();
			terminal.DataReceived += (ss, ee) => e.Channel.SendData(ee);
			terminal.CloseReceived += (ss, ee) => e.Channel.SendClose(ee);

			terminal.Run();
		}
		else if (e.ShellType == "exec")
		{
			var parser = new Regex(@"(?<cmd>git-receive-pack|git-upload-pack|git-upload-archive) \'/?(?<proj>.+)\.git\'");
			var match = parser.Match(e.CommandText);
			var command = match.Groups["cmd"].Value;
			var project = match.Groups["proj"].Value;

			var git = new GitService(command, project);

			e.Channel.DataReceived += (ss, ee) => git.OnData(ee);
			e.Channel.CloseReceived += (ss, ee) => git.OnClose();
			git.DataReceived += (ss, ee) => e.Channel.SendData(ee);
			git.CloseReceived += (ss, ee) => e.Channel.SendClose(ee);

			git.Start();
		}
		else if (e.ShellType == "subsystem")
		{
			// do something more
		}
	}
}
