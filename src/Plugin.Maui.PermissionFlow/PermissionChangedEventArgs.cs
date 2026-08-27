namespace Plugin.Maui.PermissionFlow;

public sealed class PermissionChangedEventArgs : EventArgs
{
	public PermissionChangedEventArgs(AppPermission permission, PermissionStatusKind previous, PermissionStatusKind current)
	{
		Permission = permission;
		Previous = previous;
		Current = current;
	}

	public AppPermission Permission { get; }

	public PermissionStatusKind Previous { get; }

	public PermissionStatusKind Current { get; }
}
