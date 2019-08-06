using System;
using System.Threading.Tasks;

namespace AeroCtl
{
	public class TouchpadController
	{
		public bool Enabled
		{
			get => this.wmi.InvokeGet<byte>("GetTouchPad") != 0;
			set
			{
				if (value)
					EnableTouchpad();
				else
					DisableTouchpad();
			}
		}

		private readonly AeroWmi wmi;

		public TouchpadController(AeroWmi wmi)
		{
			this.wmi = wmi;
		}

		public void EnableTouchpad()
		{
			this.wmi.InvokeSet("SetTouchPad", (byte)1);
		}

		public void DisableTouchpad()
		{
			this.wmi.InvokeSet("SetTouchPad", (byte)0);
		}

		public void ToggleTouchpad()
		{
			Enabled = !Enabled;
		}
	}
}