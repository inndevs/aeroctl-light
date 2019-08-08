using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Threading.Tasks;

namespace AeroCtl
{
	/// <summary>
	/// Wraps the Gigabyte WMI interface.
	/// </summary>
	public class AeroWmi : IDisposable
	{
		#region Fields

		private readonly ManagementClass getClass;
		private readonly ManagementClass setClass;
		private readonly ManagementObject get;
		private readonly ManagementObject set;

		#endregion

		#region Properties

		public string BaseBoard { get; }
		public string SerialNumber { get; }
		public string Sku { get; }
		public IReadOnlyList<string> BiosVersions { get; }

		#endregion

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

			ManagementObject win32ComputerSystem = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_ComputerSystem")
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

			if (win32ComputerSystem != null)
			{
				this.Sku = (string)win32ComputerSystem["SystemSKUNumber"];
			}

			ManagementScope scope = new ManagementScope("root\\WMI", new ConnectionOptions
			{
				EnablePrivileges = true,
				Impersonation = ImpersonationLevel.Impersonate
			});

			this.getClass = new ManagementClass(scope, new ManagementPath("GB_WMIACPI_Get"), null);
			this.setClass = new ManagementClass(scope, new ManagementPath("GB_WMIACPI_Set"), null);

			foreach (ManagementObject obj in this.getClass.GetInstances().OfType<ManagementObject>())
			{
				this.get = obj;
				break;
			}

			foreach (ManagementObject obj in this.setClass.GetInstances().OfType<ManagementObject>())
			{
				this.set = obj;
				break;
			}

			if (this.get == null)
				throw new InvalidOperationException("Failed to find instance for GB_WMIACPI_Get. Your device is probably not supported.");

			if (this.set == null)
				throw new InvalidOperationException("Failed to find instance for GB_WMIACPI_Set. Your device is probably not supported.");
		}
		
		/// <summary>
		/// Invokes a WMI method.
		/// </summary>
		/// <param name="methodName"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public ImmutableDictionary<string, object> Invoke(string methodName, params (string, object)[] parameters)
		{
			ManagementObject target = this.get;
			MethodData m ;

			try
			{
				m = this.getClass.Methods[methodName];
			}
			catch (ManagementException ex) when (ex.ErrorCode == ManagementStatus.NotFound)
			{
				target = this.set;
				m = this.setClass.Methods[methodName];
			}

			ManagementBaseObject inParams = m.InParameters;

			if (inParams != null)
			{
				foreach ((string name, object value) in parameters)
				{
					inParams[name] = value;
				}
			}
			else
			{
				if (parameters.Length != 0)
					throw new TargetParameterCountException($"Method does not take any parameters, but {parameters.Length} were supplied.");
			}

			ManagementBaseObject outParams = target.InvokeMethod(methodName, inParams, null);
			ImmutableDictionary<string, object> result = ImmutableDictionary<string, object>.Empty;

			if (outParams != null)
			{
				foreach (var res in outParams.Properties)
				{
					result = result.Add(res.Name, res.Value);
				}
			}

			Debug.WriteLine($"{methodName}({string.Join(", ", parameters.Select(p => $"{p.Item1} = {p.Item2}"))}) = {string.Join(", ", result.Select(p => $"{p.Key} = {p.Value}"))}");

			return result;
		}

		/// <summary>
		/// Invokes a WMI method asynchronously.
		/// </summary>
		/// <param name="methodName"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public Task<ImmutableDictionary<string, object>> InvokeAsync(string methodName, params (string, object)[] parameters)
		{
			return Task.Run(() => this.Invoke(methodName, parameters));
		}

		public T InvokeSet<T>(string methodName, T value)
		{
			ManagementBaseObject inParams = this.set.GetMethodParameters(methodName);
			inParams["Data"] = value;
			ManagementBaseObject outParams = this.set.InvokeMethod(methodName, inParams, null);

			T res = default;
			if (outParams != null)
			{
				res = (T)outParams["DataOut"];
			}

			Debug.WriteLine($"{methodName}({value}) = {res}");
			return res;
		}

		public Task<T> InvokeSetAsync<T>(string methodName, T value)
		{
			return Task.Run(() => this.InvokeSet(methodName, value));
		}

		public T InvokeGet<T>(string methodName)
		{
			ManagementBaseObject outParams = this.get.InvokeMethod(methodName, null, null);
			return (T)outParams["Data"];
		}

		public Task<T> InvokeGetAsync<T>(string methodName)
		{
			return Task.Run(() => this.InvokeGet<T>(methodName));
		}

		public void Dispose()
		{
			this.get?.Dispose();
			this.getClass?.Dispose();

			this.set?.Dispose();
			this.setClass?.Dispose();
		}
	}
}