#if !ANDROID && !IOS
namespace Plugin.Maui.PermissionFlow;

sealed class NetDeviceServiceProbe : IDeviceServiceProbe
{
	public bool IsEnabled(DeviceService service) => true;
}
#endif
