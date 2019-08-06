﻿using NativeWifi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AeroCtl
{
	/// <summary>
	/// Implements the AERO interfaces.
	/// </summary>
	public class Aero : IDisposable
	{
		#region Fields

		private readonly WlanClient wlanClient;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the WMI interface.
		/// </summary>
		private AeroWmi Wmi { get; }

		/// <summary>
		/// Gets the base board / notebook model name.
		/// </summary>
		public string BaseBoard => this.Wmi.BaseBoard;

		/// <summary>
		/// Gets the serial number. Should match the one found on the underside of the notebook.
		/// </summary>
		public string SerialNumber => this.Wmi.SerialNumber;

		/// <summary>
		/// Gets the BIOS version strings.
		/// </summary>
		public IReadOnlyList<string> BiosVersions => this.Wmi.BiosVersions;

		/// <summary>
		/// Gets Keyboard Fn key handler.
		/// </summary>
		public KeyboardController Keyboard { get; }

		/// <summary>
		/// Gets the fan controller.
		/// </summary>
		public IFanController Fans { get; }

		/// <summary>
		/// Gets the screen controller.
		/// </summary>
		public ScreenController Screen { get; }

		/// <summary>
		/// Gets the battery stats / controller.
		/// </summary>
		public BatteryController Battery { get; }

		/// <summary>
		/// Gets or sets the software wifi enable state.
		/// </summary>
		public bool WifiEnabled
		{
			get
			{
				Wlan.Dot11RadioState state = this.wlanClient.Interfaces.FirstOrDefault()?.RadioState.PhyRadioState.FirstOrDefault().dot11SoftwareRadioState ?? Wlan.Dot11RadioState.Unknown;
				return state == Wlan.Dot11RadioState.On;
			}
			set
			{
				Wlan.WlanPhyRadioState newState;
				if (value)
				{
					newState = new Wlan.WlanPhyRadioState
					{
						dwPhyIndex = (int)Wlan.Dot11PhyType.Any,
						dot11SoftwareRadioState = Wlan.Dot11RadioState.On,
					};
				}
				else
				{
					newState = new Wlan.WlanPhyRadioState
					{
						dwPhyIndex = (int)Wlan.Dot11PhyType.Any,
						dot11SoftwareRadioState = Wlan.Dot11RadioState.Off,
					};
				}

				foreach (var iface in this.wlanClient.Interfaces)
				{
					iface.SetRadioState(newState);
				}
			}
		}

		#endregion

		#region Constructors

		public Aero(AeroWmi wmi)
		{
			this.Wmi = wmi;
			this.Keyboard = new KeyboardController();
			this.Fans = new Aero2019FanController(wmi);
			this.Screen = new ScreenController(wmi);
			this.Battery = new BatteryController(wmi);
			this.wlanClient = new WlanClient();
		}

		#endregion

		#region Methods

		public async Task<double> GetCpuTemperatureAsync()
		{
			return await this.Wmi.InvokeGetAsync<ushort>("getCpuTemp");
		}

		public void Dispose()
		{
			this.Wmi?.Dispose();
			this.Keyboard?.Dispose();
		}

		#endregion
	}
}
