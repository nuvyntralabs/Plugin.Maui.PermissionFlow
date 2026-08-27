namespace Plugin.Maui.PermissionFlow;

/// <summary>
/// Classifies a <see cref="PermissionFlowException"/>.
/// </summary>
public enum PermissionFlowError
{
	InvalidOperation = 0,
	UnknownFlow = 1,
	StoreFailure = 2,
	PlatformFailure = 3
}
