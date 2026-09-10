using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ExampleApp;

public class CommandService
{
	private Process? _process;
	private readonly ProcessStartInfo _startInfo;

	public CommandService(string command, string args)
	{
		_startInfo = new ProcessStartInfo(command, args)
		{
			CreateNoWindow = true,
			RedirectStandardError = true,
			RedirectStandardInput = true,
			RedirectStandardOutput = true,
			UseShellExecute = false,
		};
	}

	public event EventHandler<byte[]>? DataReceived;
	public event EventHandler? EofReceived;
	public event EventHandler<uint>? CloseReceived;

	public void Start()
	{
		// The command and arguments come from whatever the caller passed to the constructor.
		// This class is a sample of how to wire a process to an SSH channel; it is not used by
		// ExampleSshApplication, which denies "exec" requests outright. Anything that does use
		// it must decide for itself which commands an authenticated user may run — passing an
		// exec request straight through from the wire would hand the client a shell.
		// nosemgrep: csharp_injection_rule-CommandInjection
		_process = Process.Start(_startInfo)
			?? throw new InvalidOperationException("Failed to start process.");
		Task.Run(() => MessageLoop());
	}

	public void OnData(byte[] data)
	{
		if (_process is null)
		{
			return;
		}

		_process.StandardInput.BaseStream.Write(data, 0, data.Length);
		_process.StandardInput.BaseStream.Flush();
	}

	public void OnClose()
	{
		if (_process is null)
		{
			return;
		}

		_process.StandardInput.BaseStream.Close();
	}

	private void MessageLoop()
	{
		if (_process is null)
		{
			return;
		}

		var bytes = new byte[1024 * 64];
		while (true)
		{
			var len = _process.StandardOutput.BaseStream.Read(bytes, 0, bytes.Length);
			if (len <= 0)
			{
				break;
			}

			var data = bytes.Length != len
				? [.. bytes.Take(len)]
				: bytes;
			DataReceived?.Invoke(this, data);
		}

		EofReceived?.Invoke(this, EventArgs.Empty);
		CloseReceived?.Invoke(this, (uint)_process.ExitCode);
	}
}
