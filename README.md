# forked from https://gitlab.com/wtwrp/aeroctl

I forked this, because the Fan Control is faulty. I don't mean the Creator of AeroCtl, but also wmi / Gigabyte.
I noticed CPU spikes that indicated that two mechanisms fought against each other.

We only need fn keys + Charge Stop. Anything else is NOT needed IMHO.

HAVE FUN!!

## Installation
1. Uninstall Control Center
2. Uninstall Intel XTU (Extreme Tuning Utility)
3. Go to folder `supply/` and run the .bat as admin - this placed the .dll that control center would also use to communicate with WMI (= Gigabyte Hardware API)
4. Install Gigabyte ControlCenter or SmartManager if it isn't already.
    * Install [.NET 5.0 Runtime](https://dotnet.microsoft.com/download/dotnet/5.0/runtime) if it isn't already.
5. Run `AeroCtl.UI.exe` as administrator from the unpacked directory.
6. (Optional) Use Windows' task scheduler to add a task to autostart aeroctl as administrator on logon.

# Build
1. Build in IDE
2. Go to AeroCtl.Ui
3. Run  `dotnet publish -r win-x64 --self-contained false -p:PublishSingleFile=true -p:IncludeAllContentForSelfExtract=true`
4. .exe is in `AeroCtl.Ui/bin/Debug/.../publish`

## Supported devices

### Fully tested

These laptops have been fully tested by the development team.

* Aero 15-SA
* Aero 15Xv8

### Positive feedback

Reports by users having a good experience.

* Aero 17-XB
* Aero 15-YA
* Aero 15-YB
* Aero 15-KB

### Negative feedback

Laptops with open issues or other reports of limited functionality.

* Aorus 15G-YB
* Aorus 15G-KB

## License

GPLv3
