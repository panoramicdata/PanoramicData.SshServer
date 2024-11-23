using ExampleApp.MiniTerm.Processes;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ExampleApp.MiniTerm;

/// <summary>
/// The UI of the terminal. It's just a normal console window, but we're managing the input/output.
/// In a "real" project this could be some other UI.
/// </summary>
public sealed class Terminal : IDisposable
{
	private readonly PseudoConsolePipe _inputPipe;
	private readonly PseudoConsolePipe _outputPipe;
	private readonly PseudoConsole _pseudoConsole;
	private readonly Process _process;
	private readonly FileStream _writer;
	private readonly FileStream _reader;
	private bool _disposedValue;

	public Terminal(string command, int windowWidth, int windowHeight)
	{
		_inputPipe = new PseudoConsolePipe();
		_outputPipe = new PseudoConsolePipe();
		_pseudoConsole = PseudoConsole.Create(_inputPipe.ReadSide, _outputPipe.WriteSide, windowWidth, windowHeight);
		_process = ProcessFactory.Start(command, PseudoConsole.PseudoConsoleThreadAttribute, _pseudoConsole.Handle);
		_writer = new FileStream(_inputPipe.WriteSide, FileAccess.Write);
		_reader = new FileStream(_outputPipe.ReadSide, FileAccess.Read);
	}

	public event EventHandler<byte[]> DataReceived;
	public event EventHandler<uint> CloseReceived;

	/// <summary>
	/// Start the pseudo console and run the process as shown in 
	/// https://docs.microsoft.com/en-us/windows/console/creating-a-pseudoconsole-session#creating-the-pseudoconsole
	/// </summary>
	public void Run() =>
		// copy all pseudo console output to STDOUT
		Task.Run(() =>
		{
			var proc = System.Diagnostics.Process.GetProcessById(_process.ProcessInfo.dwProcessId);

			var buf = new byte[1024];
			while (!proc.HasExited)
			{
				var length = _reader.Read(buf, 0, buf.Length);
				if (length == 0)
					break;
				DataReceived?.Invoke(this, buf.Take(length).ToArray());
			}

			CloseReceived?.Invoke(this, 0);
		});

	public void OnInput(byte[] data)
	{
		_writer.Write(data, 0, data.Length);
		_writer.Flush();
	}

	public void OnClose()
	{
		_writer.WriteByte(0x03);
		_writer.Flush();
	}

	private void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				_writer.Dispose();
				_reader.Dispose();
				_pseudoConsole.Dispose();
				_process.Dispose();
				_inputPipe.Dispose();
				_outputPipe.Dispose();
			}

			_disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put clean-up code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
