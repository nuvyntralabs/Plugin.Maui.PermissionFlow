#if ANDROID
#pragma warning disable CA1416, CA1422
using Android.Bluetooth;
using Android.Content;
using Android.Locations;
using Microsoft.Maui.ApplicationModel;

namespace Plugin.Maui.PermissionFlow;

sealed class AndroidDeviceServiceProbe : IDeviceServiceProbe
{
	public bool IsEnabled(DeviceService service) => service switch
	{
		DeviceService.Location => IsLocationEnabled(),
		DeviceService.Bluetooth => BluetoothAdapter.DefaultAdapter?.IsEnabled == true,
		_ => true
	};

	static bool IsLocationEnabled()
	{
		var manager = Platform.AppContext.GetSystemService(Context.LocationService) as LocationManager;
		if (manager is null)
			return false;

		if (OperatingSystem.IsAndroidVersionAtLeast(28))
			return manager.IsLocationEnabled;

		return manager.IsProviderEnabled(LocationManager.GpsProvider)
			|| manager.IsProviderEnabled(LocationManager.NetworkProvider);
	}
}
#endif
