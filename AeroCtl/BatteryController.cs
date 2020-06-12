using System;
using System.CodeDom;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using AeroCtl.Native;

namespace AeroCtl
{
	/// <summary>
	/// Laptop battery controller
	/// </summary>
	public class BatteryController
	{
		private readonly AeroWmi wmi;
		private bool healthSupported = true;
		
		public PowerLineStatus PowerLineStatus
		{
			get
			{
				Kernel32.GetSystemPowerStatus(out SYSTEM_POWER_STATUS status);
				return status.ACLineStatus;
			}
		}

		public BatteryController(AeroWmi wmi)
		{
			this.wmi = wmi;
		}

		/// <summary>
		/// Returns the current battery status.
		/// </summary>
		/// <returns></returns>
		public async Task<BatteryStatus> GetStatusAsync()
		{
			return await Task.Run(() =>
			{
				ManagementObject batt = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Battery")
					.Get()
					.OfType<ManagementObject>()
					.FirstOrDefault();

				ManagementObject status = new ManagementObjectSearcher("root\\WMI", "SELECT * FROM BatteryStatus")
					.Get()
					.OfType<ManagementObject>()
					.FirstOrDefault();

				int percent = 0;
				double charge = 0;
				double chargeRate = 0;
				double dischargeRate = 0;
				double voltage = 0;
				
				if (batt != null)
				{
					percent = (ushort)batt["EstimatedChargeRemaining"];
				}

				if (status != null)
				{
					charge = (uint)status.GetPropertyValue("RemainingCapacity") / 1000.0;
					chargeRate = (int)status.GetPropertyValue("ChargeRate") / 1000.0;
					dischargeRate = (int)status.GetPropertyValue("DischargeRate") / 1000.0;
					voltage = (uint)status.GetPropertyValue("Voltage") / 1000.0;
				}

				return new BatteryStatus(charge, percent, chargeRate, dischargeRate, voltage);
			});
		}

		public async Task<ChargePolicy> GetChargePolicyAsync()
		{
			return (ChargePolicy)await this.wmi.InvokeGetAsync<ushort>("GetChargePolicy");
		}

		public async Task SetChargePolicyAsync(ChargePolicy policy)
		{
			await this.wmi.InvokeSetAsync<byte>("SetChargePolicy", (byte)policy);
		}

		public async Task<int> GetChargeStopAsync()
		{
			return await this.wmi.InvokeGetAsync<ushort>("GetChargeStop");
		}

		public async Task SetChargeStopAsync(int percent)
		{
			if (percent <= 0 || percent > 100)
				throw new ArgumentOutOfRangeException(nameof(percent));

			await this.wmi.InvokeSetAsync<byte>("SetChargeStop", (byte)percent);
		}

		public async Task<bool> GetSmartChargeAsync()
		{
			return await this.wmi.InvokeGetAsync<byte>("GetSmartCharge") != 0;
		}

		public async Task SetSmargeChargeAsync(bool enabled)
		{
			await this.wmi.InvokeSetAsync<byte>("SetSmartCharge", enabled ? (byte)1 : (byte)0);
		}

		public async Task<int?> GetHealthAsync()
		{
			if (!this.healthSupported)
				return null;

			try
			{
				return await this.wmi.InvokeGetAsync<byte>("GetBatteryHealth");
			}
			catch (ManagementException ex) when (ex.ErrorCode == ManagementStatus.InvalidMethod)
			{
				this.healthSupported = false;
				return null;
			}
		}

		public async Task<int> GetCyclesAsync()
		{
			int v1 = await this.wmi.InvokeGetAsync<ushort>("getBattCyc");
			int v2 = await this.wmi.InvokeGetAsync<ushort>("getBattCyc1");

			return Math.Max(v1, v2);
		}
	}
}
