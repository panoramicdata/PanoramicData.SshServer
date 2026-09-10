using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace ExampleApp.MiniTerm.Native;

/// <summary>
/// PInvoke signatures for win32 pseudo console api
/// </summary>
static class PseudoConsoleApi
{
	internal const uint PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE = 0x00020016;

	[StructLayout(LayoutKind.Sequential)]
	internal struct COORD : IEquatable<COORD>
	{
		public short X;
		public short Y;

		public readonly bool Equals(COORD other) => X == other.X && Y == other.Y;

		public override readonly bool Equals(object? obj) => obj is COORD other && Equals(other);

		public override readonly int GetHashCode() => HashCode.Combine(X, Y);

		public static bool operator ==(COORD left, COORD right) => left.Equals(right);

		public static bool operator !=(COORD left, COORD right) => !left.Equals(right);
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	internal static extern int CreatePseudoConsole(COORD size, SafeFileHandle hInput, SafeFileHandle hOutput, uint dwFlags, out nint phPC);

	[DllImport("kernel32.dll", SetLastError = true)]
	internal static extern int ResizePseudoConsole(nint hPC, COORD size);

	[DllImport("kernel32.dll", SetLastError = true)]
	internal static extern int ClosePseudoConsole(nint hPC);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	internal static extern bool CreatePipe(out SafeFileHandle hReadPipe, out SafeFileHandle hWritePipe, nint lpPipeAttributes, int nSize);
}
