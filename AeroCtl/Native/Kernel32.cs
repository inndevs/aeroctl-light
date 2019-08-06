using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace AeroCtl.Native
{
	public static class Kernel32
	{
		private const string lib = "kernel32.dll";

		public const uint GENERIC_READ = 1u << 31;
		public const uint GENERIC_WRITE = 1u << 30;

		public const uint FILE_SHARE_READ = 1;
		public const uint FILE_SHARE_WRITE = 2;

		public const uint CREATE_NEW = 1;
		public const uint CREATE_ALWAYS = 2;

		public const uint FILE_FLAG_OVERLAPPED = 0x40000000u;

		[DllImport(lib)]
		public static extern IntPtr LoadLibrary(string path);

		[DllImport(lib)]
		public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

		[DllImport(lib)]
		public static extern bool FreeLibrary(IntPtr hModule);

		[DllImport(lib, CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern SafeFileHandle CreateFile(
			string lpFileName,
			uint dwDesiredAccess,
			uint dwShareMode,
			IntPtr lpSecurityAttributes,
			uint dwCreationDisposition,
			uint dwFlagsAndAttributes,
			IntPtr hTemplateFile);

		[DllImport(lib, CharSet = CharSet.Unicode)]
		public static extern void CloseHandle(SafeHandle handle);

		[DllImport(lib)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CancelIo(SafeFileHandle hFile);

		[DllImport(lib, SetLastError = true)]
		public static extern bool LockFile(
			SafeFileHandle hFile,
			uint fileOffsetLow,
			uint fileOffsetHigh,
			uint numberOfBytesToLockLow,
			uint numberOfBytesToLockHigh);

		[DllImport(lib)]
		public static extern bool UnlockFile(
			SafeFileHandle hFile,
			uint fileOffsetLow,
			uint fileOffsetHigh,
			uint numberOfBytesToUnlockLow,
			uint numberOfBytesToUnlockHigh);


		[DllImport(lib, SetLastError = true)]
		private static extern int CreateFile(
			string lpFileName,
			uint dwDesiredAccess,
			uint dwShareMode,
			uint lpSecurityAttributes,
			uint dwCreationDisposition,
			uint dwFlagsAndAttributes,
			uint hTemplateFile);

		[DllImport(lib)]
		public static extern int CloseHandle(int hObject);

		[DllImport(lib)]
		public static extern void GetSystemPowerStatus(out SYSTEM_POWER_STATUS lpSystemPowerStatus);

	}
}