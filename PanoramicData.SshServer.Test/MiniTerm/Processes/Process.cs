using ExampleApp.MiniTerm.Native;
using System;
using System.Runtime.InteropServices;

namespace ExampleApp.MiniTerm.Processes;

/// <summary>
/// Represents an instance of a process.
/// </summary>
internal sealed class Process(Native.ProcessApi.STARTUPINFOEX startupInfo, Native.ProcessApi.PROCESS_INFORMATION processInfo) : IDisposable
{
	public ProcessApi.STARTUPINFOEX StartupInfo { get; } = startupInfo;
	public ProcessApi.PROCESS_INFORMATION ProcessInfo { get; } = processInfo;

	#region IDisposable Support

	private bool _disposedValue = false; // To detect redundant calls

	void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects).
			}

			// dispose unmanaged state

			// Free the attribute list
			if (StartupInfo.lpAttributeList != nint.Zero)
			{
				ProcessApi.DeleteProcThreadAttributeList(StartupInfo.lpAttributeList);
				Marshal.FreeHGlobal(StartupInfo.lpAttributeList);
			}

			// Close process and thread handles
			if (ProcessInfo.hProcess != nint.Zero)
			{
				ProcessApi.CloseHandle(ProcessInfo.hProcess);
			}

			if (ProcessInfo.hThread != nint.Zero)
			{
				ProcessApi.CloseHandle(ProcessInfo.hThread);
			}

			_disposedValue = true;
		}
	}

	~Process()
	{
		// Do not change this code. Put clean-up code in Dispose(bool disposing) above.
		Dispose(false);
	}

	// This code added to correctly implement the disposable pattern.
	public void Dispose()
	{
		// Do not change this code. Put clean-up code in Dispose(bool disposing) above.
		Dispose(true);
		// use the following line if the finalizer is overridden above.
		GC.SuppressFinalize(this);
	}

	#endregion
}
