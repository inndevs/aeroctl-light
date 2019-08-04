using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AeroCtl.Native;
using Microsoft.Win32.SafeHandles;

namespace AeroCtl
{
	public class KeyHandler : IDisposable
	{
		#region Fields

		/// <summary>
		/// Probably the keyboard device GUID.
		/// </summary>
		private static readonly Guid deviceGuid = new Guid("4D1E55B2-F16F-11CF-88CB-001111000030");

		/// <summary>
		/// Keyboard vendor IDs and product IDs for Gigabyte laptop keyboards.
		/// Taken from Gigabyte ControlCenter.
		/// </summary>
		private static readonly IReadOnlyDictionary<ushort, ushort[]> supportedKeyboards = new Dictionary<ushort, ushort[]>()
		{
			{
				1241, 
				new ushort[]
				{
					32776
				}
			},
			{
				4164,
				new ushort[]
				{
					31288,
					31289,
					31290,
					31291,
					31292,
					31293,
					31292,
					31293,
					31294,
					31295
				}
			}
		};
		
		private readonly DummyForm form;

		#endregion

		#region Properties

		#endregion

		#region Constructors

		public KeyHandler()
		{
			Guid guid = deviceGuid;
			Hid.HidD_GetHidGuid(ref guid);

			IntPtr classDevs = SetupApi.SetupDiGetClassDevs(ref guid, IntPtr.Zero, IntPtr.Zero, DiGetClassFlags.Present | DiGetClassFlags.DeviceInterface);
			try
			{
				uint memberIndex = 0;

				SafeFileHandle file = null;

				try
				{
					// No idea why this has to be done in a loop, but Gigabyte does this as well.
					do
					{
						SP_DEVICE_INTERFACE_DATA deviceInterfaceData;
						SP_DEVINFO_DATA deviceInfoData;
						SP_DEVICE_INTERFACE_DETAIL_DATA deviceInterfaceDetailData;
						do
						{
							deviceInterfaceData = new SP_DEVICE_INTERFACE_DATA { cbSize = (uint)Marshal.SizeOf<SP_DEVICE_INTERFACE_DATA>() };

							if (!SetupApi.SetupDiEnumDeviceInterfaces(classDevs, IntPtr.Zero, ref guid, memberIndex, ref deviceInterfaceData))
								throw new NotSupportedException("Keyboard device not found.");

							++memberIndex;
							deviceInfoData = new SP_DEVINFO_DATA { cbSize = (uint)Marshal.SizeOf<SP_DEVINFO_DATA>() };
							deviceInterfaceDetailData = new SP_DEVICE_INTERFACE_DETAIL_DATA { cbSize = IntPtr.Size == 8 ? 8U : (uint)(4 + Marshal.SystemDefaultCharSize) };
						} while (!SetupApi.SetupDiGetDeviceInterfaceDetail(classDevs, ref deviceInterfaceData, ref deviceInterfaceDetailData, 256U, out uint requiredSize, ref deviceInfoData));

						file = Kernel32.CreateFile(deviceInterfaceDetailData.DevicePath, 3221225472U, 3U, IntPtr.Zero, 3U, 1073741824U, IntPtr.Zero);
					} while (file.IsInvalid);

					HIDD_ATTRIBUTES attributse = default;
					Hid.HidD_GetAttributes(file, ref attributse);

					if (!supportedKeyboards.TryGetValue(attributse.VendorID, out ushort[] pids))
						throw new NotSupportedException($"Keyboard VID \"{attributse.VendorID}\" not supported.");

					if (!pids.Contains(attributse.ProductID))
						throw new NotSupportedException($"Keyboard VID \"{attributse.VendorID}\", PID \"{attributse.ProductID}\" not supported.");
				}
				finally
				{
					file?.Close();
					file?.Dispose();
				}
			}
			finally
			{
				SetupApi.SetupDiDestroyDeviceInfoList(classDevs);
			}

			// Create a dummy form to capture the input events.
			this.form = new DummyForm();
			this.form.RawInputReceived += this.onRawInput;
		}

		private void onRawInput(object sender, RAWINPUT e)
		{
			Debug.WriteLine($"type={e.header.dwType} MakeCode={e.data.keyboard.MakeCode} Flags={e.data.keyboard.Flags} VKey={e.data.keyboard.VKey} Message={e.data.keyboard.Message:X8} Extra={e.data.keyboard.ExtraInformation}");
		}

		#endregion

		#region Methods

		public void Dispose()
		{
			this.form?.Dispose();
		}

		#endregion

		#region Nested Types

		/// <summary>
		/// Dummy form that receives the raw input events.
		/// </summary>
		private sealed class DummyForm : Form
		{
			public event EventHandler<RAWINPUT> RawInputReceived;

			public DummyForm()
			{
				this.CreateHandle();
			}

			protected override void OnHandleCreated(EventArgs e)
			{
				base.OnHandleCreated(e);

				RAWINPUTDEVICE[] pRawInputDevice = new RAWINPUTDEVICE[1];
				pRawInputDevice[0].usUsagePage = 65282;
				pRawInputDevice[0].usUsage = 1;
				pRawInputDevice[0].dwFlags = RAWINPUTDEVICE.RIDEV_INPUTSINK | RAWINPUTDEVICE.RIDEV_DEVNOTIFY;
				pRawInputDevice[0].hwndTarget = this.Handle;
				if (!User32.RegisterRawInputDevices(pRawInputDevice, (uint)pRawInputDevice.Length, (uint)Marshal.SizeOf<RAWINPUTDEVICE>()))
					throw new ApplicationException("Failed to register raw input device(s).");
			}

			protected override void WndProc(ref Message m)
			{
				if (m.Msg == 0xFF) // WM_INPUT
				{
					int sizeOfRawInput = Marshal.SizeOf<RAWINPUTHEADER>();
					int size = 0;

					if (User32.GetRawInputData(m.LParam, User32.RID_INPUT, IntPtr.Zero, ref size, sizeOfRawInput) == -1)
						throw new Win32Exception(Marshal.GetLastWin32Error());

					if (User32.GetRawInputData(m.LParam, User32.RID_INPUT, out RAWINPUT rawInput, ref size, sizeOfRawInput) == -1)
						throw new Win32Exception(Marshal.GetLastWin32Error());

					this.RawInputReceived?.Invoke(this, rawInput);

					return;
				}

				base.WndProc(ref m);
			}
		}

		#endregion
	}
}
