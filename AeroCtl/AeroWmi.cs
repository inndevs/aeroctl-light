using System;
using System.Management;

namespace AeroCtl
{
	public class AeroWmi : IDisposable
	{
		public ManagementClass GetClass { get; }
		public ManagementObject Get { get; }

		public ManagementClass SetClass { get; }
		public ManagementObject Set { get; }

		public AeroWmi()
		{
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

		public void Dispose()
		{
			this.Get?.Dispose();
			this.GetClass?.Dispose();

			this.Set?.Dispose();
			this.SetClass?.Dispose();
		}
	}
}