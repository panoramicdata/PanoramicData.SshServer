using System;
using System.Runtime.InteropServices;

namespace ExampleApp.MiniTerm.Native;

/// <summary>
/// PInvoke signatures for win32 process api
/// </summary>
static class ProcessApi
{
	internal const uint EXTENDED_STARTUPINFO_PRESENT = 0x00080000;

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct STARTUPINFOEX : IEquatable<STARTUPINFOEX>
	{
		public STARTUPINFO StartupInfo;
		public nint lpAttributeList;

		public readonly bool Equals(STARTUPINFOEX other) =>
			StartupInfo.Equals(other.StartupInfo) && lpAttributeList == other.lpAttributeList;

		public override readonly bool Equals(object? obj) => obj is STARTUPINFOEX other && Equals(other);

		public override readonly int GetHashCode() => HashCode.Combine(StartupInfo, lpAttributeList);

		public static bool operator ==(STARTUPINFOEX left, STARTUPINFOEX right) => left.Equals(right);

		public static bool operator !=(STARTUPINFOEX left, STARTUPINFOEX right) => !left.Equals(right);
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct STARTUPINFO
	{
		public int cb;
		public string lpReserved;
		public string lpDesktop;
		public string lpTitle;
		public int dwX;
		public int dwY;
		public int dwXSize;
		public int dwYSize;
		public int dwXCountChars;
		public int dwYCountChars;
		public int dwFillAttribute;
		public int dwFlags;
		public short wShowWindow;
		public short cbReserved2;
		public nint lpReserved2;
		public nint hStdInput;
		public nint hStdOutput;
		public nint hStdError;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct PROCESS_INFORMATION : IEquatable<PROCESS_INFORMATION>
	{
		public nint hProcess;
		public nint hThread;
		public int dwProcessId;
		public int dwThreadId;

		public readonly bool Equals(PROCESS_INFORMATION other) =>
			hProcess == other.hProcess && hThread == other.hThread &&
			dwProcessId == other.dwProcessId && dwThreadId == other.dwThreadId;

		public override readonly bool Equals(object? obj) => obj is PROCESS_INFORMATION other && Equals(other);

		public override readonly int GetHashCode() => HashCode.Combine(hProcess, hThread, dwProcessId, dwThreadId);

		public static bool operator ==(PROCESS_INFORMATION left, PROCESS_INFORMATION right) => left.Equals(right);

		public static bool operator !=(PROCESS_INFORMATION left, PROCESS_INFORMATION right) => !left.Equals(right);
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct SECURITY_ATTRIBUTES : IEquatable<SECURITY_ATTRIBUTES>
	{
		public int nLength;
		public nint lpSecurityDescriptor;
		public int bInheritHandle;

		public readonly bool Equals(SECURITY_ATTRIBUTES other) =>
			nLength == other.nLength && lpSecurityDescriptor == other.lpSecurityDescriptor &&
			bInheritHandle == other.bInheritHandle;

		public override readonly bool Equals(object? obj) => obj is SECURITY_ATTRIBUTES other && Equals(other);

		public override readonly int GetHashCode() => HashCode.Combine(nLength, lpSecurityDescriptor, bInheritHandle);

		public static bool operator ==(SECURITY_ATTRIBUTES left, SECURITY_ATTRIBUTES right) => left.Equals(right);

		public static bool operator !=(SECURITY_ATTRIBUTES left, SECURITY_ATTRIBUTES right) => !left.Equals(right);
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool InitializeProcThreadAttributeList(
		nint lpAttributeList, int dwAttributeCount, int dwFlags, ref nint lpSize);

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool UpdateProcThreadAttribute(
		nint lpAttributeList, uint dwFlags, nint attribute, nint lpValue,
		nint cbSize, nint lpPreviousValue, nint lpReturnSize);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool CreateProcess(
		string lpApplicationName, string lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes,
		ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags,
		nint lpEnvironment, string lpCurrentDirectory, [In] ref STARTUPINFOEX lpStartupInfo,
		out PROCESS_INFORMATION lpProcessInformation);

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool DeleteProcThreadAttributeList(nint lpAttributeList);

	[DllImport("kernel32.dll", SetLastError = true)]
	internal static extern bool CloseHandle(nint hObject);
}
