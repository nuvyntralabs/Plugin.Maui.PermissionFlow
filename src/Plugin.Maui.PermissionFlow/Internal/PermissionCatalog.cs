namespace Plugin.Maui.PermissionFlow;

static class PermissionCatalog
{
	public static string GetDisplayName(AppPermission permission) => permission switch
	{
		AppPermission.Camera => "Camera",
		AppPermission.Microphone => "Microphone",
		AppPermission.LocationWhenInUse => "Location",
		AppPermission.LocationAlways => "Background location",
		AppPermission.Photos => "Photos",
		AppPermission.PhotosAddOnly => "Photo library (add only)",
		AppPermission.Media => "Media",
		AppPermission.StorageRead => "Storage",
		AppPermission.StorageWrite => "Storage",
		AppPermission.ContactsRead => "Contacts",
		AppPermission.ContactsWrite => "Contacts",
		AppPermission.CalendarRead => "Calendar",
		AppPermission.CalendarWrite => "Calendar",
		AppPermission.Reminders => "Reminders",
		AppPermission.Bluetooth => "Bluetooth",
		AppPermission.Notifications => "Notifications",
		AppPermission.Sensors => "Motion sensors",
		AppPermission.Speech => "Speech recognition",
		AppPermission.Phone => "Phone",
		AppPermission.Sms => "SMS",
		AppPermission.NearbyWifi => "Nearby devices",
		_ => permission.ToString()
	};

	public static IReadOnlyList<AppPermission> Expand(AppPermission permission) =>
		permission == AppPermission.LocationAlways
			? [AppPermission.LocationWhenInUse, AppPermission.LocationAlways]
			: [permission];

	public static string DefaultRationale(AppPermission permission) =>
		$"This feature needs {GetDisplayName(permission).ToLowerInvariant()} access.";

	public static string DefaultSettingsMessage(AppPermission permission) =>
		$"{GetDisplayName(permission)} access is turned off. You can enable it in Settings.";
}
