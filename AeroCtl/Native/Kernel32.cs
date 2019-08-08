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
		public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

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
		public static extern int CloseHandle(int hObject);

		[DllImport(lib)]
		public static extern void GetSystemPowerStatus(out SYSTEM_POWER_STATUS lpSystemPowerStatus);

	}
}