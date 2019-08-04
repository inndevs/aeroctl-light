using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AeroCtl.UI
{
	/// <summary>
	/// Current state of the laptop for data binding.
	/// </summary>
	public class AeroState : INotifyPropertyChanged
	{
		private readonly Aero aero;
		private bool updating;

		private string baseBoard;
		public string BaseBoard
		{
			get => this.baseBoard;
			set
			{
				this.baseBoard = value;
				this.OnPropertyChanged();
			}
		}

		private int fanRpm1;
		public int FanRpm1
		{
			get => this.fanRpm1;
			set
			{
				this.fanRpm1 = value;
				this.OnPropertyChanged();
			}
		}

		private int fanRpm2;
		public int FanRpm2
		{
			get => this.fanRpm2;
			set
			{
				this.fanRpm2 = value;
				this.OnPropertyChanged();
			}
		}

		private int screenBrightness;
		public int ScreenBrightness
		{
			get => (int)this.screenBrightness;
			set
			{
				this.screenBrightness = value;
				this.OnPropertyChanged();

				if (!this.updating)
				{
					this.aero.Screen.Brightness = (uint)value;
				}
			}
		}

		public AeroState(Aero aero)
		{
			this.aero = aero;
		}

		public void Update()
		{
			this.updating = true;
			try
			{
				this.BaseBoard = this.aero.BaseBoard;
				this.FanRpm1 = this.aero.Fans.Rpm1;
				this.FanRpm2 = this.aero.Fans.Rpm2;
				this.ScreenBrightness = (int)this.aero.Screen.Brightness;
			}
			finally
			{
				this.updating = false;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
