using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace AeroCtl
{
	public class AeroWmi : IDisposable
	{
		public ManagementClass GetClass { get; }
		public ManagementObject Get { get; }

		public ManagementClass SetClass { get; }
		public ManagementObject Set { get; }

		public string BaseBoard { get; }

		public AeroWmi()
		{
			ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BaseBoard");
			foreach (ManagementObject managementObject in managementObjectSearcher.Get())
			{
				this.BaseBoard = managementObject["Product"].ToString();
			}

			ManagementScope scope = new ManagementScope("root\\WMI", new ConnectionOptions
			{
				EnablePrivileges = true,
				Impersonation = ImpersonationLevel.Impersonate
			});

			this.GetClass = new ManagementClass(scope, new ManagementPath("GB_WMIACPI_Get"), null);
			using (var enumerator = this.GetClass.GetInstances().GetEnumerator())
			{
				if (!enumerator.MoveNext())
					throw new InvalidOperationException("Failed to find instance for GB_WMIACPI_Get. Your device is probably not supported.");

				this.Get = (ManagementObject)enumerator.Current;
			}

			this.SetClass = new ManagementClass(scope, new ManagementPath("GB_WMIACPI_Set"), null);
			using (var enumerator = this.SetClass.GetInstances().GetEnumerator())
			{
				if (!enumerator.MoveNext())
					throw new InvalidOperationException("Failed to find instance for GB_WMIACPI_Set. Your device is probably not supported.");

				this.Set = (ManagementObject)enumerator.Current;
			}
		}

		public T InvokeSet<T>(string methodName, T value)
		{
			ManagementBaseObject inParams = this.SetClass.GetMethodParameters(methodName);
			inParams["Data"] = value;
			ManagementBaseObject outParams = this.Set.InvokeMethod(methodName, inParams, null);

			if (outParams == null)
				return default;

			return (T)outParams["DataOut"];
		}

		public T InvokeGet<T>(string methodName)
		{
			ManagementBaseObject inParams = this.GetClass.GetMethodParameters(methodName);
			ManagementBaseObject outParams = this.Get.InvokeMethod(methodName, inParams, null);
			return (T)outParams["Data"];
		}

		public void Dispose()
		{
			this.Get?.Dispose();
			this.GetClass?.Dispose();

			this.Set?.Dispose();
			this.SetClass?.Dispose();
		}
	}
}