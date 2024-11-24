using ExampleApp.MiniTerm;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PanoramicData.SshServer;
using PanoramicData.SshServer.Interfaces;
using PanoramicData.SshServer.Services;
using System;

namespace ExampleApp;
internal class ExampleSshApplication(
	IOptions<ExampleSshApplicationConfig> options,
	ILogger<Program> logger) : ISshApplication
{
	private readonly ExampleSshApplicationConfig _config = options.Value;

	public void SshServerSessionStart(object sshServerObject, Session session)
	{
		var sshServer = sshServerObject as SshServer
			?? throw new InvalidOperationException($"Expected {nameof(SshServer)}, but got {sshServerObject.GetType().Name}.");

		logger.LogInformation(
			"Session {SessionId} opened",
			session.Id);

		session.ServiceRegistered += ServiceRegistered;
		session.KeysExchanged += KeysExchanged;
	}

	public void SshServerSessionEnd(object sshServerObject, Session session)
	{
		var sshServer = sshServerObject as SshServer
			?? throw new InvalidOperationException($"Expected {nameof(SshServer)}, but got {sshServerObject.GetType().Name}.");

		logger.LogInformation(
			"Session {SessionId} closed",
			session.Id);

		session.ServiceRegistered -= ServiceRegistered;
		session.KeysExchanged -= KeysExchanged;
	}

	private void KeysExchanged(object sessionObject, KeyExchangeArgs keyExchangeArgs)
	{
		var session = sessionObject as Session ?? throw new InvalidOperationException($"Expected {nameof(Session)}, but got {sessionObject.GetType().Name}.");

		logger.LogInformation(
			"Session {SessionId} Key exchange algorithms: {KeyExchangeAlg}",
			session.Id,
			string.Join(',', keyExchangeArgs.KeyExchangeAlgorithms));
	}

	private void ServiceRegistered(object sessionObject, SshService sshService)
	{
		var session = sessionObject as Session ?? throw new InvalidOperationException($"Expected {nameof(Session)}, but got {sessionObject.GetType().Name}.");

		logger.LogInformation("Session {SessionId} Requesting {ServiceType}.",
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
				logger.LogInformation("Service {ServiceType} is not supported.", sshService.GetType().Name);
				break;
		}
	}

	private void WindowChange(object sessionObject, WindowChangeArgs windowChangeArgs)
	{
		var session = sessionObject as Session
				?? throw new InvalidOperationException($"Expected {nameof(Session)}.  Received {sessionObject.GetType().Name}");

		logger.LogInformation("Session {SessionId} Server Channel {ServerChannel}, Client Channel {ClientChannel} Window size changed to {WidthColumns}x{HeightRows} ({WidthPixels}x{HeightPixels}).",
			session.Id,
			windowChangeArgs.Channel.ServerChannelId,
			windowChangeArgs.Channel.ClientChannelId,
			windowChangeArgs.WidthColumns,
			windowChangeArgs.HeightRows,
			windowChangeArgs.WidthPixels,
			windowChangeArgs.HeightPixels);

		session.SetTerminalSize(
			windowChangeArgs.Channel.ServerChannelId,
			new TerminalSize(
				windowChangeArgs.WidthColumns,
				windowChangeArgs.HeightRows,
				windowChangeArgs.WidthPixels,
				windowChangeArgs.HeightPixels)
			);
	}

	private void TcpForwardRequest(object sessionObject, TcpRequestArgs e)
	{
		var session = sessionObject as Session
			?? throw new InvalidOperationException($"Expected {nameof(Session)}.  Received {sessionObject.GetType().Name}");

		logger.LogInformation("Session {SessionId} Received a request to forward data to {Host}:{Port}",
			session.Id,
			e.Host,
			e.Port);

		if (!_config.PermitTcpForwarding)
		{
			return;
		}

		var tcp = new TcpForwardService(e.Host, e.Port);
		e.Channel.DataReceived += (ss, ee) => tcp.OnData(ee);
		e.Channel.CloseReceived += (ss, ee) => tcp.OnClose();
		tcp.DataReceived += (ss, ee) => e.Channel.SendData(ee);
		tcp.CloseReceived += (ss, ee) => e.Channel.SendClose();
		tcp.Start();
	}

	private void PtyReceived(object sessionObject, PtyArgs ptyArgs)
	{
		var session = sessionObject as Session
			?? throw new InvalidOperationException($"Expected {nameof(Session)}.  Received {sessionObject.GetType().Name}");

		logger.LogInformation("Session {SessionId} Request to create a PTY received for terminal type {TerminalType} ({WidthColumns}x{HeightRows} / {WidthPixels}x{HeightPixels})",
			session.Id,
			ptyArgs.Terminal,
			ptyArgs.WidthChars,
			ptyArgs.HeightRows,
			ptyArgs.WidthPx,
			ptyArgs.HeightPx);

		session.SetTerminalSize(0, new TerminalSize(ptyArgs.WidthChars, ptyArgs.HeightRows, ptyArgs.WidthPx, ptyArgs.HeightPx));
	}

	private void EnvReceived(object sessionObject, EnvironmentArgs e)
	{
		var session = sessionObject as Session
			?? throw new InvalidOperationException($"Expected {nameof(Session)}.  Received {sessionObject.GetType().Name}");

		logger.LogInformation("Session {SessionId} Received environment variable {Name}:{Value}",
			session.Id,
			e.Name,
			e.Value);
	}

	private void Service_Userauth(object userAuthServiceObject, UserauthArgs userAuthArgs)
	{
		var userAuthService = userAuthServiceObject as UserAuthService
			?? throw new InvalidOperationException($"Expected {nameof(UserAuthService)}.  Received {userAuthServiceObject.GetType().Name}");

		logger.LogInformation(
			"Session {SessionId} KeyAlgorithm {KeyAlgorithm}, Key {Key}, Fingerprint: {Fingerprint}, Username {Username}, Password {Password}.",
			userAuthArgs.Session.Id,
			userAuthArgs.KeyAlgorithm,
			userAuthArgs.Key,
			userAuthArgs.Fingerprint,
			userAuthArgs.Username,
			string.IsNullOrEmpty(userAuthArgs.Password) ? "NONE PROVIDED" : "***REDACTED***"
			);

		// TODO - check username and password
		// Return false if username/password don't match
		userAuthArgs.Result = true;
	}

	private void CommandOpened(object sessionObject, CommandRequestedArgs commandRequestArgs)
	{
		var session = sessionObject as Session
			?? throw new InvalidOperationException($"Expected {nameof(Session)}, but got {sessionObject.GetType().Name}.");

		logger.LogInformation("Session {SessionId} Server Channel {ServerChannelId} Client Channel {ClientChannelId} runs {ShellType}: \"{CommandText}\".",
		session.Id,
		commandRequestArgs.Channel.ServerChannelId,
		commandRequestArgs.Channel.ClientChannelId,
		commandRequestArgs.ShellType,
		commandRequestArgs.CommandText);

		var terminalSize = session.GetTerminalSize(commandRequestArgs.Channel.ServerChannelId);

		switch (commandRequestArgs.ShellType)
		{
			case "shell":
				{
					// You should implement your own shell here.

					// DANGER! The following is a simple pass-through to CMD, running as the user that the SSH server is running as:
					var terminal = new Terminal("cmd.exe", terminalSize.WidthColumns, terminalSize.HeightRows);
					commandRequestArgs.Channel.DataReceived += (ss, ee) => terminal.OnInput(ee);
					commandRequestArgs.Channel.CloseReceived += (ss, ee) => terminal.OnClose();
					terminal.DataReceived += (ss, ee) => commandRequestArgs.Channel.SendData(ee);
					terminal.CloseReceived += (ss, ee) => commandRequestArgs.Channel.SendClose(ee);

					terminal.Run();
					break;
				}

			case "exec":
				{
					// This is where you would implement your own command execution.
					// For example, you could allow the user to run /bin/bash, but nothing else

					// Now, just because they've asked for this, doesn't mean that they're getting it.
					// If you allow this, implement it similarly to the "shell" case above.

					// Log for security audit
					logger.LogWarning("User was denied request to execute {CommandText}", commandRequestArgs.CommandText);

					commandRequestArgs.Channel.SendClose();
					break;
				}

			default:
				// Just no.
				commandRequestArgs.Channel.SendClose();
				break;
		}
	}
}