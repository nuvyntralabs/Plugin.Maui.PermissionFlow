namespace Plugin.Maui.PermissionFlow;

/// <summary>
/// Cross-platform permission kinds understood by PermissionFlow.
/// Unavailable kinds on a given OS are treated as not applicable.
/// </summary>
public enum AppPermission
{
	Camera,
	Microphone,
	LocationWhenInUse,
	LocationAlways,
	Photos,
	PhotosAddOnly,
	Media,
	StorageRead,
	StorageWrite,
	ContactsRead,
	ContactsWrite,
	CalendarRead,
	CalendarWrite,
	Reminders,
	Bluetooth,
	Notifications,
	Sensors,
	Speech,
	Phone,
	Sms,
	NearbyWifi
}
