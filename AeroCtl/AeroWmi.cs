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
		public string SerialNumber { get; }
		public IReadOnlyList<string> BiosVersions { get; }

		public AeroWmi()
		{
			ManagementObject win32BaseBoard = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BaseBoard")
				.Get()
				.OfType<ManagementObject>()
				.FirstOrDefault();

			ManagementObject win32Bios = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BIOS")
				.Get()
				.OfType<ManagementObject>()
				.FirstOrDefault();

			if (win32BaseBoard != null)
			{
				this.BaseBoard = (string)win32BaseBoard["Product"];
			}

			if (win32Bios != null)
			{
				this.SerialNumber = (string)win32Bios["SerialNumber"];
				this.BiosVersions = (string[])win32Bios["BIOSVersion"];
			}

			ManagementScope scope = new ManagementScope("root\\WMI", new ConnectionOptions
			{
				EnablePrivileges = true,
				Impersonation = ImpersonationLevel.Impersonate
			});

			this.GetClass = new ManagementClass(scope, new ManagementPath("GB_WMIACPI_Get"), null);
			this.SetClass = new ManagementClass(scope, new ManagementPath("GB_WMIACPI_Set"), null);

			foreach (ManagementObject obj in this.GetClass.GetInstances().OfType<ManagementObject>())
			{
				this.Get = obj;
				break;
			}

			foreach (ManagementObject obj in this.SetClass.GetInstances().OfType<ManagementObject>())
			{
				this.Set = obj;
				break;
			}

			if (this.Get == null)
				throw new InvalidOperationException("Failed to find instance for GB_WMIACPI_Get. Your device is probably not supported.");

			if (this.Set == null)
				throw new InvalidOperationException("Failed to find instance for GB_WMIACPI_Set. Your device is probably not supported.");
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
			//ManagementBaseObject inParams = this.GetClass.GetMethodParameters(methodName);
			ManagementBaseObject outParams = this.Get.InvokeMethod(methodName, null, null);
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