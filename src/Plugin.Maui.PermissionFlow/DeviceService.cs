namespace Plugin.Maui.PermissionFlow;

/// <summary>
/// Device-wide services a flow can require in addition to runtime permissions.
/// </summary>
public enum DeviceService
{
	/// <summary>
	/// System location services (GPS / location mode).
	/// </summary>
	Location,

	/// <summary>
	/// Bluetooth radio is powered on.
	/// </summary>
	Bluetooth
}
