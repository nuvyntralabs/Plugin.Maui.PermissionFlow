namespace Plugin.Maui.PermissionFlow;

/// <summary>
/// Point-in-time status of one or more permissions.
/// </summary>
public sealed class PermissionSnapshot
{
	public PermissionSnapshot(DateTimeOffset capturedAt, IReadOnlyList<PermissionState> permissions)
	{
		CapturedAt = capturedAt;
		Permissions = permissions;
	}

	public DateTimeOffset CapturedAt { get; }

	public IReadOnlyList<PermissionState> Permissions { get; }

	public PermissionState? this[AppPermission permission] =>
		Permissions.FirstOrDefault(state => state.Permission == permission);
}
