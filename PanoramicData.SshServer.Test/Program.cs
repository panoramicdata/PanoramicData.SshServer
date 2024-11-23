using PanoramicData.SshServer.Services;
using PanoramicData.SshServer.Test.MiniTerm;
using System;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace PanoramicData.SshServer.Test;

class Program
{
	static int windowWidth, windowHeight;

	static void Main()
	{
		var server = new SshServer(new StartingInfo(IPAddress.Any, 1022, "SSH-2.0-PanoramicDataSshServerTest"));

		// Generate a new RSA key pair using rsa-sha2-256
		var rsa = RSA.Create(2048);
		var privateKey = rsa.ExportRSAPrivateKey();
		var publicKey = rsa.ExportRSAPublicKey();

		var publicKeyBase64 = Convert.ToBase64String(publicKey);
		var privateKeyBase64 = Convert.ToBase64String(privateKey);

		server.AddHostKey("rsa-sha2-256", privateKeyBase64);
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
