namespace Plugin.Maui.PermissionFlow;

/// <summary>
/// Normalized permission status after platform-specific refinement.
/// </summary>
public enum PermissionStatusKind
{
	Unknown = 0,
	NotDetermined,
	Denied,
	Granted,
	Restricted,
	Limited,
	PermanentlyDenied,
	Unavailable
}
