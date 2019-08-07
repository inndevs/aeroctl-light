using System;
using System.Threading.Tasks;

namespace AeroCtl
{
	public class TouchpadController
	{
		public bool Enabled
		{
			get => this.wmi.InvokeGet<byte>("GetTouchPad") != 0;
			set => this.wmi.InvokeSet("SetTouchPad", value ? (byte)1 : (byte)0);
		}

		private readonly AeroWmi wmi;

		public TouchpadController(AeroWmi wmi)
		{
			this.wmi = wmi;
		}

		public void ToggleTouchpad()
		{
			this.Enabled = !this.Enabled;
		}
	}
}