using System;
using System.Management;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace AeroCtl
{
	public class TouchpadController : IDisposable
	{
		private readonly RegistryKey key;
		private readonly ManagementEventWatcher keyWatcher;

		private static string escape(string str)
		{
			str = str.Replace("'", "\\'");
			str = str.Replace("\"", "\\\"");
			str = str.Replace("\\", "\\\\");
			return str;
		}

		public TouchpadController()
		{
			const string path = @"Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad\Status";
			this.key = Registry.CurrentUser.OpenSubKey(path);

			WindowsIdentity currentUser = WindowsIdentity.GetCurrent();
			WqlEventQuery query = new WqlEventQuery($"SELECT * FROM RegistryValueChangeEvent WHERE Hive='HKEY_USERS' AND KeyPath='{escape(currentUser.User.Value)}\\\\{escape(path)}' AND ValueName='Enabled'");
			this.keyWatcher = new ManagementEventWatcher(query);
			this.keyWatcher.EventArrived += (s, e) => { this.onEnabledChanged(); };
			this.keyWatcher.Start();
		}

		public event EventHandler EnabledChanged;

		private void onEnabledChanged()
		{
			this.EnabledChanged?.Invoke(this, EventArgs.Empty);
		}

		public ValueTask<bool> GetEnabledAsync()
		{
			object value = this.key.GetValue("Enabled", 0);
			bool state = false;
			if (value is int i)
				state = i != 0;
			return new ValueTask<bool>(state);
		}

		public ValueTask SetEnabledAsync(bool enabled)
		{
			// has no effect:
			// Registry.CurrentUser.SetValue("Enabled", enabled ? 1 : 0, RegistryValueKind.DWord);
			return default;
		}

		public void Dispose()
		{
			this.keyWatcher.Stop();
			this.keyWatcher.Dispose();
			this.key?.Dispose();
		}
	}
}