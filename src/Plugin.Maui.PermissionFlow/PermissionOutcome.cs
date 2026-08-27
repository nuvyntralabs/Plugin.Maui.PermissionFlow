namespace Plugin.Maui.PermissionFlow;

/// <summary>
/// What the orchestrator did for a single step.
/// </summary>
public enum PermissionOutcome
{
	AlreadyGranted,
	Granted,
	Denied,
	PermanentlyDenied,
	Restricted,
	Unavailable,
	SkippedCooldown,
	RationaleDeclined,
	SettingsOpened,
	ServiceDisabled,
	ServiceEnabled
}
