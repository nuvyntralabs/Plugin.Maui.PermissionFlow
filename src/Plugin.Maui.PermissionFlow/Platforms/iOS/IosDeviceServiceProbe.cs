#if IOS
using CoreLocation;

namespace Plugin.Maui.PermissionFlow;

sealed class IosDeviceServiceProbe : IDeviceServiceProbe
{
	public bool IsEnabled(DeviceService service) => service switch
	{
		DeviceService.Location => CLLocationManager.LocationServicesEnabled,
		DeviceService.Bluetooth => true,
		_ => true
	};
}
#endif
