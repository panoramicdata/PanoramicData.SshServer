﻿using System;
using System.Runtime.InteropServices;
using static ExampleApp.MiniTerm.Native.ProcessApi;

namespace ExampleApp.MiniTerm.Processes;

/// <summary>
/// Support for starting and configuring processes.
/// </summary>
/// <remarks>
/// Possible to replace with managed code? The key is being able to provide the PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE attribute
/// </remarks>
static class ProcessFactory
{
	/// <summary>
	/// Start and configure a process. The return value represents the process and should be disposed.
	/// </summary>
	internal static Process Start(string command, nint attributes, nint hPC)
	{
		var startupInfo = ConfigureProcessThread(hPC, attributes);
		var processInfo = RunProcess(ref startupInfo, command);
		return new Process(startupInfo, processInfo);
	}

	private static STARTUPINFOEX ConfigureProcessThread(nint hPC, nint attributes)
	{
		// this method implements the behavior described in https://docs.microsoft.com/en-us/windows/console/creating-a-pseudoconsole-session#preparing-for-creation-of-the-child-process

		var lpSize = nint.Zero;
		var success = InitializeProcThreadAttributeList(
			lpAttributeList: nint.Zero,
			dwAttributeCount: 1,
			dwFlags: 0,
			lpSize: ref lpSize
		);
		if (success || lpSize == nint.Zero) // we're not expecting `success` here, we just want to get the calculated lpSize
		{
			throw new InvalidOperationException("Could not calculate the number of bytes for the attribute list. " + Marshal.GetLastWin32Error());
		}

		var startupInfo = new STARTUPINFOEX();
		startupInfo.StartupInfo.cb = Marshal.SizeOf<STARTUPINFOEX>();
		startupInfo.lpAttributeList = Marshal.AllocHGlobal(lpSize);

		success = InitializeProcThreadAttributeList(
			lpAttributeList: startupInfo.lpAttributeList,
			dwAttributeCount: 1,
			dwFlags: 0,
			lpSize: ref lpSize
		);
		if (!success)
		{
			throw new InvalidOperationException("Could not set up attribute list. " + Marshal.GetLastWin32Error());
		}

		success = UpdateProcThreadAttribute(
			lpAttributeList: startupInfo.lpAttributeList,
			dwFlags: 0,
			attribute: attributes,
			lpValue: hPC,
			cbSize: nint.Size,
			lpPreviousValue: nint.Zero,
			lpReturnSize: nint.Zero
		);
		if (!success)
		{
			throw new InvalidOperationException("Could not set pseudoconsole thread attribute. " + Marshal.GetLastWin32Error());
		}

		return startupInfo;
	}

	private static PROCESS_INFORMATION RunProcess(ref STARTUPINFOEX sInfoEx, string commandLine)
	{
		var securityAttributeSize = Marshal.SizeOf<SECURITY_ATTRIBUTES>();
		var pSec = new SECURITY_ATTRIBUTES { nLength = securityAttributeSize };
		var tSec = new SECURITY_ATTRIBUTES { nLength = securityAttributeSize };
		var success = CreateProcess(
			lpApplicationName: null,
			lpCommandLine: commandLine,
			lpProcessAttributes: ref pSec,
			lpThreadAttributes: ref tSec,
			bInheritHandles: false,
			dwCreationFlags: EXTENDED_STARTUPINFO_PRESENT,
			lpEnvironment: nint.Zero,
			lpCurrentDirectory: null,
			lpStartupInfo: ref sInfoEx,
			lpProcessInformation: out var pInfo
		);
		if (!success)
		{
			throw new InvalidOperationException("Could not create process. " + Marshal.GetLastWin32Error());
		}

		return pInfo;
	}
}
