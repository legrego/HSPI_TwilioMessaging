Homeseer Weather Underground PlugIn
=====================================
HomeSeer plugin to get weather data from Weather Underground into HomeSeer.

Overview
--------
This plugin gets Current condition, US Alerts, today forecast, tomorrow forecast, 2nd & 3rd day forecast and yesterday weather to the devices in Homeseer. You can choose which devices are created and updated.

The plugin in written in C# and is based on a sample from http://board.homeseer.com/showthread.php?t=178122.

Compatibility
------------
Tested on the following platforms:
* Windows 10 

Installation
-----------
Make sure that dotNet 4.6.2 is installed on machine. [Link](https://support.microsoft.com/en-us/help/3151802/the-.net-framework-4.6.2-web-installer-for-windows)

Place the compiled [executable](https://ci.appveyor.com/project/dk307/hspi-wuweather/build/artifacts?branch=master) and [config file](https://ci.appveyor.com/project/dk307/hspi-wuweather/build/artifacts?branch=master) in the HomeSeer installation directory. Restart HomeSeer. HomeSeer will recognize the plugin and will add plugin in disable state to its Plugins. Go to HomeSeer -> PlugIns -> Manage and enable this plugin. Create directory *wuweather* under *HomeSeer installation directory\html\images*. Extract Icons([icons.zip](/asserts/Icons.zip)) to this new directory.

Open Menu Item PlugIns->WU Weather->Configuration. You should see page like this:

![Initial Configuration Page](/asserts/Initial.png "Initial Configuration Setting Page")

Enter API Key and Station Id. API Home link takes to Weather Underground API Home to manage API Key. Find Station shows webpage useful to select Station.

![Station Configured Page](/asserts/Configured.png "Configuration Set Page")

If StationId is correct, the image will show location of the station.

Select Devices to be created from other tabs.

If API Key and Station Id are valid, the devices with correct values will show up.

![Devices](/asserts/Devices.png "Devices")

If API Key is wrong, it would show in Homeseer logs as:
*Warning:Failed to Fetch Data with Invalid API Key*

Build State
-----------
[![Build State](https://ci.appveyor.com/api/projects/status/github/dk307/HSPI_WUWeather?branch=master&svg=true)](https://ci.appveyor.com/project/dk307/hspi-wuweather/build/artifacts?branch=master)

 Icons made by  [Freepik](http://www.freepik.com) from [Flaticon](http://www.flaticon.com) is licensed by [CC 3.0 BY](http://creativecommons.org/licenses/by/3.0/)
