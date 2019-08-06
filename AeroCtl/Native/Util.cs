using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AeroCtl.Native
{
	public static class Util
	{
		public static T CreateDelegate<T>(IntPtr hModule, string name) where T : Delegate
		{
			IntPtr procAddr = Kernel32.GetProcAddress(hModule, name);
			if (procAddr == IntPtr.Zero)
				return null;

			return Marshal.GetDelegateForFunctionPointer<T>(procAddr);
		}
	}
}
