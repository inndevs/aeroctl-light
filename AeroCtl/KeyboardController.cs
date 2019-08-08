using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AeroCtl.Native;
using Microsoft.Win32.SafeHandles;

namespace AeroCtl
{
	public class KeyboardController : IDisposable
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

		private DummyForm form;
		private readonly HidDevice[] usbDevs;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the RGB LED controller, if present.
		/// </summary>
		public IRgbController Rgb { get; }

		#endregion

		#region Constructors

		public KeyboardController()
		{
			Guid guid = deviceGuid;
			Hid.HidD_GetHidGuid(ref guid);

			List<HidDevice> devs = new List<HidDevice>();
			IntPtr classDevs = SetupApi.SetupDiGetClassDevs(ref guid, IntPtr.Zero, IntPtr.Zero, DiGetClassFlags.Present | DiGetClassFlags.DeviceInterface);
			try
			{
				// Enumerable all devices for that GUID.
				for (uint index = 0;; ++index)
				{
					SP_DEVICE_INTERFACE_DATA deviceInterfaceData = new SP_DEVICE_INTERFACE_DATA { cbSize = (uint)Marshal.SizeOf<SP_DEVICE_INTERFACE_DATA>() };
					if (!SetupApi.SetupDiEnumDeviceInterfaces(classDevs, IntPtr.Zero, ref guid, index, ref deviceInterfaceData))
						break; // End of list.

					SP_DEVICE_INTERFACE_DETAIL_DATA deviceInterfaceDetailData = new SP_DEVICE_INTERFACE_DETAIL_DATA { cbSize = IntPtr.Size == 8 ? 8U : (uint)(4 + Marshal.SystemDefaultCharSize) };
					SP_DEVINFO_DATA deviceInfoData = new SP_DEVINFO_DATA { cbSize = (uint)Marshal.SizeOf<SP_DEVINFO_DATA>() };
					if (SetupApi.SetupDiGetDeviceInterfaceDetail(classDevs, ref deviceInterfaceData, ref deviceInterfaceDetailData, 256U, out uint requiredSize, ref deviceInfoData))
					{
						// Found one.
						// Try to open device.
						string devPath = deviceInterfaceDetailData.DevicePath;
						SafeFileHandle devHandle = Kernel32.CreateFile(devPath, Kernel32.GENERIC_READ | Kernel32.GENERIC_WRITE,
							Kernel32.FILE_SHARE_READ | Kernel32.FILE_SHARE_WRITE, IntPtr.Zero, Kernel32.CREATE_NEW | Kernel32.CREATE_ALWAYS, Kernel32.FILE_FLAG_OVERLAPPED, IntPtr.Zero);

						// Get HID attributes.
						HIDD_ATTRIBUTES attributes = default;
						HIDP_CAPS caps;
						Hid.HidD_GetAttributes(devHandle, ref attributes);

						// Match against Gigabyte keyboard product IDs.
						if (!supportedKeyboards.TryGetValue(attributes.VendorID, out ushort[] pids))
							continue;

						if (!pids.Contains(attributes.ProductID))
							continue;

						// Get HID capabilities.
						IntPtr preparsedData = IntPtr.Zero;
						try
						{
							if (!Hid.HidD_GetPreparsedData(devHandle, out preparsedData))
								throw new Win32Exception(Marshal.GetLastWin32Error());

							if (Hid.HidP_GetCaps(preparsedData, out caps) != Hid.HIDP_STATUS_SUCCESS)
								throw new Win32Exception(Marshal.GetLastWin32Error());
						}
						finally
						{
							if (preparsedData != IntPtr.Zero)
								Hid.HidD_FreePreparsedData(preparsedData);
						}

						// Store device.
						HidDevice dev = new HidDevice(devPath, attributes, caps, devHandle);
						devs.Add(dev);

						// Find RGB controller.
						if (caps.UsagePage == 0xFF01 && caps.Usage == 1 && caps.FeatureReportByteLength == 9)
						{
							this.Rgb = new Ite8291RgbController(dev);
						}
					}
				}

				this.usbDevs = devs.ToArray();
			}
			finally
			{
				SetupApi.SetupDiDestroyDeviceInfoList(classDevs);
				if (this.usbDevs == null)
				{
					this.usbDevs = devs.ToArray();
					this.Dispose();
				}
			}

		}

		private event EventHandler<FnKeyEventArgs> fnKeyPressed;

		public event EventHandler<FnKeyEventArgs> FnKeyPressed
		{
			add
			{
				this.fnKeyPressed += value;
				this.StartKeyHandling();
			}
			remove => this.fnKeyPressed -= value;
		}

		private void onRawInput(object sender, RAWINPUT e)
		{
			if(e.header.dwType == RAWINPUTHEADER.RIM_TYPEHID && e.data.keyboard.MakeCode == 4)
			{
				switch(e.data.keyboard.Message)
				{
					case 0x7C000004: // Wifi.
						this.fnKeyPressed?.Invoke(this, new FnKeyEventArgs(FnKey.ToggleWifi));
						return;

					case 0x7D000004: // Decrease brightness.
						this.fnKeyPressed?.Invoke(this, new FnKeyEventArgs(FnKey.DecreaseBrightness));
						return;

					case 0x7E000004: // Increase brightness.
						this.fnKeyPressed?.Invoke(this, new FnKeyEventArgs(FnKey.IncreaseBrightness));
						return;

					case 0x80000004: // Screen toggle.
						this.fnKeyPressed?.Invoke(this, new FnKeyEventArgs(FnKey.ToggleScreen));
						return;

					case 0x81000004: // Toggle touchpad on/off.
						this.fnKeyPressed?.Invoke(this, new FnKeyEventArgs(FnKey.ToggleTouchpad));
						return;

					case 0x84000004: // Max fan.
						this.fnKeyPressed?.Invoke(this, new FnKeyEventArgs(FnKey.ToggleFan));
						return;
				}
			}
			Debug.WriteLine($"Unhandled raw input: dwType={e.header.dwType} MakeCode={e.data.keyboard.MakeCode} Flags={e.data.keyboard.Flags} VKey={e.data.keyboard.VKey} Message={e.data.keyboard.Message:X8} Extra={e.data.keyboard.ExtraInformation}");
		}

		#endregion

		#region Methods

		public void StartKeyHandling()
		{
			if (this.form != null)
				return;

			// Create a dummy form to capture the input events.
			this.form = new DummyForm();
			this.form.RawInputReceived += this.onRawInput;
		}

		public void Dispose()
		{
			this.form?.Dispose();

			if (this.usbDevs != null)
			{
				foreach (HidDevice dev in this.usbDevs)
				{
					dev.Handle.Close();
					dev.Handle.Dispose();
				}
			}
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

				List<RAWINPUTDEVICE> pRawInputDevice = new List<RAWINPUTDEVICE>();

				//pRawInputDevice.Add(new RAWINPUTDEVICE
				//{
				//	usUsagePage = 1,
				//	usUsage = 6,
				//	dwFlags = RAWINPUTDEVICE.RIDEV_INPUTSINK | RAWINPUTDEVICE.RIDEV_DEVNOTIFY,
				//	hwndTarget = this.Handle
				//});

				//pRawInputDevice.Add(new RAWINPUTDEVICE
				//{
				//	usUsagePage = 1,
				//	usUsage = 2,
				//	dwFlags = RAWINPUTDEVICE.RIDEV_INPUTSINK | RAWINPUTDEVICE.RIDEV_DEVNOTIFY,
				//	hwndTarget = this.Handle
				//});

				pRawInputDevice.Add(new RAWINPUTDEVICE
				{
					usUsagePage = 0xFF00,
					usUsage = 0xFF00,
					dwFlags = RAWINPUTDEVICE.RIDEV_INPUTSINK | RAWINPUTDEVICE.RIDEV_DEVNOTIFY,
					hwndTarget = this.Handle
				});

				pRawInputDevice.Add(new RAWINPUTDEVICE
				{
					usUsagePage = 0xFF01,
					usUsage = 0x2209,
					dwFlags = RAWINPUTDEVICE.RIDEV_INPUTSINK | RAWINPUTDEVICE.RIDEV_DEVNOTIFY,
					hwndTarget = this.Handle
				});

				pRawInputDevice.Add(new RAWINPUTDEVICE
				{
					usUsagePage = 0xFF02,
					usUsage = 1,
					dwFlags = RAWINPUTDEVICE.RIDEV_INPUTSINK | RAWINPUTDEVICE.RIDEV_DEVNOTIFY,
					hwndTarget = this.Handle
				});

				if (!User32.RegisterRawInputDevices(pRawInputDevice.ToArray(), (uint)pRawInputDevice.Count, (uint)Marshal.SizeOf<RAWINPUTDEVICE>()))
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
