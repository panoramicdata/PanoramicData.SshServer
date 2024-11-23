using PanoramicData.SshServer;
using PanoramicData.SshServer.Services;
using PanoramicData.SshServer.Test;
using PanoramicData.SshServer.Test.MiniTerm;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;


namespace ExampleApp;

class Program
{
	static async Task Main()
	{
		// Create a cancellation token source
		using var cts = new CancellationTokenSource();
		var cancellationToken = cts.Token;

		var sshServer = new SshServer(new StartingInfo(IPAddress.Any, 1022, "SSH-2.0-ExampleApp"));

		// Register Ctrl+C handler
		Console.CancelKeyPress += (sender, e) =>
		{
			Console.WriteLine("Exiting...");
			sshServer.Stop();
			e.Cancel = true;
			cts.Cancel();
			Environment.Exit(0);
		};

		sshServer.AddHostKey("rsa-sha2-256", await GetPrivateKeyBase64Async("rsa-sha2-256", cancellationToken));
		sshServer.SessionStart += SshServerSessionStart;
		sshServer.SessionEnd += SshServerSessionEnd;
		sshServer.Start();

		// This will block until the cancellation token is triggered by Ctrl+C
		await Task.Delay(-1, cancellationToken);
	}

	private static async Task<string> GetPrivateKeyBase64Async(string fileNameRoot, CancellationToken cancellationToken)
	{
		var publicKeyFileInfo = new FileInfo($"{fileNameRoot}.public.txt");
		var privateKeyFileInfo = new FileInfo($"{fileNameRoot}.private.txt");

		string privateKeyBase64;

		// Does the private key file exist?
		if (privateKeyFileInfo.Exists)
		{
			// Read the private key
			privateKeyBase64 = await File.ReadAllTextAsync(privateKeyFileInfo.FullName, cancellationToken);
		}
		else
		{
			// Generate a new key pair
			var rsa = RSA.Create(2048);

			// Save the private key
			var privateKey = rsa.ExportRSAPrivateKey();
			privateKeyBase64 = Convert.ToBase64String(privateKey);
			await File.WriteAllTextAsync(privateKeyFileInfo.FullName, privateKeyBase64, cancellationToken);

			// Save the public key for reference
			var publicKey = rsa.ExportRSAPublicKey();
			var publicKeyBase64 = Convert.ToBase64String(publicKey);
			await File.WriteAllTextAsync(publicKeyFileInfo.FullName, publicKeyBase64, cancellationToken);
		}

		return privateKeyBase64;
	}

	static void SshServerSessionStart(object sshServerObject, Session session)
	{
		var sshServer = sshServerObject as SshServer
			?? throw new InvalidOperationException($"Expected {nameof(SshServer)}, but got {sshServerObject.GetType().Name}.");

		Console.WriteLine(
			"SSH Server {0} session opened {1}",
			sshServer.Id,
			session.Id);

		session.ServiceRegistered += ServiceRegistered;
		session.KeysExchanged += KeysExchanged;
	}

	static void SshServerSessionEnd(object sshServerObject, Session session)
	{
		var sshServer = sshServerObject as SshServer
			?? throw new InvalidOperationException($"Expected {nameof(SshServer)}, but got {sshServerObject.GetType().Name}.");

		Console.WriteLine(
			"SSH Server {0} session closed {1}",
			sshServer.Id,
			session.Id);

		session.ServiceRegistered -= ServiceRegistered;
		session.KeysExchanged -= KeysExchanged;
	}

	private static void KeysExchanged(object sessionObject, KeyExchangeArgs e)
	{
		var session = sessionObject as Session ?? throw new InvalidOperationException($"Expected {nameof(Session)}, but got {sessionObject.GetType().Name}.");

		foreach (var keyExchangeAlg in e.KeyExchangeAlgorithms)
		{
			Console.WriteLine("Session {0} Key exchange algorithm: {1}", session.Id, keyExchangeAlg);
		}
	}

	static void ServiceRegistered(object sessionObject, SshService sshService)
	{
		var session = sessionObject as Session ?? throw new InvalidOperationException($"Expected {nameof(Session)}, but got {sessionObject.GetType().Name}.");

		Console.WriteLine("Session {0} Requesting {1}.",
			session.Id,
			sshService.GetType().Name);

		switch (sshService)
		{
			case UserAuthService userAuthService:
				{
					userAuthService.Userauth += Service_Userauth;
					break;
				}

			case ConnectionService service:
				service.CommandOpened += CommandOpened;
				service.EnvReceived += EnvReceived;
				service.PtyReceived += PtyReceived;
				service.TcpForwardRequest += TcpForwardRequest;
				service.WindowChange += WindowChange;
				break;

			default:
				Console.WriteLine("Service {0} is not supported.", sshService.GetType().Name);
				break;
		}
	}

	static void WindowChange(object connectionServiceObject, WindowChangeArgs windowChangeArgs)
	{
		var connectionService = connectionServiceObject as ConnectionService
				?? throw new InvalidOperationException($"Expected {nameof(ConnectionService)}.  Received {connectionServiceObject.GetType().Name}");

		Console.WriteLine("ConnectionService {0}: Server Channel {1}, Client Channel {2} Window size changed to {3}x{4} ({5}x{6}).",
			connectionService.SessionId,
			windowChangeArgs.Channel.ServerChannelId,
			windowChangeArgs.Channel.ClientChannelId,
			windowChangeArgs.WidthColumns,
			windowChangeArgs.HeightRows,
			windowChangeArgs.WidthPixels,
			windowChangeArgs.HeightPixels);

		// TODO - record against service and channel id
	}

	static void TcpForwardRequest(object sender, TcpRequestArgs e)
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

	static void PtyReceived(object connectionServiceObject, PtyArgs ptyArgs)
	{
		var connectionService = connectionServiceObject as ConnectionService
			?? throw new InvalidOperationException($"Expected {nameof(ConnectionService)}.  Received {connectionServiceObject.GetType().Name}");

		Console.WriteLine("ConnectionService {0} Request to create a PTY received for terminal type {1} ({2}x{3} / {4}x{5})",
			connectionService.SessionId,
			ptyArgs.Terminal,
			ptyArgs.WidthChars,
			ptyArgs.HeightRows,
			ptyArgs.WidthPx,
			ptyArgs.HeightPx);
		//WindowSize[sender] = new WindowSize(e.WidthChars, e.HeightRows);
	}

	static void EnvReceived(object connectionServiceObject, EnvironmentArgs e)
	{
		var connectionService = connectionServiceObject as ConnectionService
			?? throw new InvalidOperationException($"Expected {nameof(ConnectionService)}.  Received {connectionServiceObject.GetType().Name}");

		Console.WriteLine("ConnectionService {0} Received environment variable {1}:{2}",
			connectionService.SessionId,
			e.Name,
			e.Value);
	}

	static void Service_Userauth(object userAuthServiceObject, UserauthArgs userAuthArgs)
	{
		var userAuthService = userAuthServiceObject as UserAuthService
				?? throw new InvalidOperationException($"Expected {nameof(UserAuthService)}.  Received {userAuthServiceObject.GetType().Name}");

		Console.WriteLine(
			"Session {0} KeyAlgorithm {1}, Key {2}, Fingerprint: {3}, Username {4}, Password {5}.",
			userAuthArgs.Session.Id,
			userAuthArgs.KeyAlgorithm,
			userAuthArgs.Key,
			userAuthArgs.Fingerprint,
			userAuthArgs.Username,
			userAuthArgs.Password,
			userAuthArgs.Key,
			userAuthArgs.KeyAlgorithm
			);

		// TODO - actually check this!
		userAuthArgs.Result = true;
	}

	static void CommandOpened(object connectionServiceObject, CommandRequestedArgs commandRequestArgs)
	{
		var connectionService = connectionServiceObject as ConnectionService
			?? throw new InvalidOperationException($"Expected {nameof(ConnectionService)}, but got {connectionServiceObject.GetType().Name}.");

		Console.WriteLine("ConnectionService {0} Channel {1} runs {2}: \"{3}\".",
		connectionService.SessionId,
		commandRequestArgs.Channel.ServerChannelId,
		commandRequestArgs.ShellType,
		commandRequestArgs.CommandText);

		// If this is not supported, just return;

		switch (commandRequestArgs.ShellType)
		{
			case "shell":
				{
					// requirements: Windows 10 RedStone 5, 1809
					// also, you can call powershell.exe
					var terminal = new Terminal("cmd.exe", 80, 25); // windowWidth, windowHeight);

					commandRequestArgs.Channel.DataReceived += (ss, ee) => terminal.OnInput(ee);
					commandRequestArgs.Channel.CloseReceived += (ss, ee) => terminal.OnClose();
					terminal.DataReceived += (ss, ee) => commandRequestArgs.Channel.SendData(ee);
					terminal.CloseReceived += (ss, ee) => commandRequestArgs.Channel.SendClose(ee);

					terminal.Run();
					break;
				}

			case "exec":
				{
					var parser = new Regex(@"(?<cmd>git-receive-pack|git-upload-pack|git-upload-archive) \'/?(?<proj>.+)\.git\'");
					var match = parser.Match(commandRequestArgs.CommandText);
					var command = match.Groups["cmd"].Value;
					var project = match.Groups["proj"].Value;

					var git = new GitService(command, project);

					commandRequestArgs.Channel.DataReceived += (ss, ee) => git.OnData(ee);
					commandRequestArgs.Channel.CloseReceived += (ss, ee) => git.OnClose();
					git.DataReceived += (ss, ee) => commandRequestArgs.Channel.SendData(ee);
					git.CloseReceived += (ss, ee) => commandRequestArgs.Channel.SendClose(ee);

					git.Start();
					break;
				}

			default:
				// Nothing communicated back
				break;
		}
	}
}
