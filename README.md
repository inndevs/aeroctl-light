AeroCtl
=======

This is a replacement for the Gigabyte "SmartManager" and/or "ControlCenter" found on the Gigabyte AERO series of notebooks. These apps can not simply be uninstalled without losing some functionality, such as Fn key support (Wifi toggle, display brightness, ...). Since these programs contain a lot of bloat and even require Intel XTU to be running at all times, and are generally pretty bad (how did they even pass QA with typos all over the place?), there was a need to replace them with something minimalist that covers everything not already covered by either standard Windows settings or dedicated tools like ThrottleStop, HWiNFO, etc. It currently implements:

* Querying system information such as Model/SKU strings, BIOS/firmware versions and CPU/GPU temperature
* Changing display brightness
* Querying battery information and setting the charging policy / charge stop.
* Fan info and control, including all hardware modes present in the ControlCenter and a fully customizable software fan controller
* Handling all the non-standard Fn keys such as wifi, touchpad and fan toggle
* Keyboard RGB LED control, albeit without a fancy UI
* GPU boost settings on the 2019 AERO (SKU P75*)

It does not do:
* Overclocking/undervolting
* Managing power plans, just use the Windows UI.
* Whatever that "Azure AI" nonsense is
* Updating BIOS, keyboard controller firmware or any other driver. You can actually download these yourself (the ControlCenter does nothing else anyway), they contain a standard setup executable that can be run standalone.
* Applying display color management profiles. Again, you can do this yourself by installing the `.icc` files for your model found in the ControlCenter installation directory via the standard Windows color management tool. It even shows them all in a handy dropdown in the usual Windows display settings.
* Fancy UI for customizing the keyboard RGB LEDs. Pull requests welcome. Otherwise just create your own little program, see the Samples directory.

Beware, this tool talks to various APIs, most of them proprietary and undocumented, so use at your own risk. As of now, it has been tested on an AERO 15Xv8 and AERO 15-SA. Their APIs differ in some areas such as fan control and GPU settings, but I doubt the other AERO models will be much different. From what I can tell Aorus is also very similar, but someone will need to verify this.

This program likely will not run on a clean Windows installation as it depends on the Gigabyte ACPI WMI driver. I believe the only thing you need is `C:\Windows\SysWOW64\acpimof.dll` and its respective registry entry (see the "Installation" part [here](https://github.com/microsoft/Windows-driver-samples/tree/master/wmi/wmiacpi#installation)), but I have not tested this. The easiest way is to just install the Gigabyte app and disable all its autostarts and services.

License
-------

GPLv3