using System;

namespace AeroCtl
{
	public class FnKeyEventArgs : EventArgs
	{
		public FnKey Key { get; }

		public FnKeyEventArgs(FnKey key)
		{
			this.Key = key;
		}
	}
}