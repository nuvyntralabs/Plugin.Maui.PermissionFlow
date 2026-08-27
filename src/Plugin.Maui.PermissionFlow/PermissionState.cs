namespace Plugin.Maui.PermissionFlow;

/// <summary>
/// Current refined status plus history for a single permission.
/// </summary>
public sealed class PermissionState
{
	public PermissionState(
		AppPermission permission,
		PermissionStatusKind status,
		bool isAvailable,
		bool canRequest,
		bool isPermanentlyDenied,
		int requestCount,
		DateTimeOffset? lastDeniedAt,
		TimeSpan? cooldownRemaining)
	{
		Permission = permission;
		Status = status;
		IsAvailable = isAvailable;
		CanRequest = canRequest;
		IsPermanentlyDenied = isPermanentlyDenied;
		RequestCount = requestCount;
		LastDeniedAt = lastDeniedAt;
		CooldownRemaining = cooldownRemaining;
	}

	public AppPermission Permission { get; }

	public PermissionStatusKind Status { get; }

	public bool IsAvailable { get; }

	public bool CanRequest { get; }

	public bool IsPermanentlyDenied { get; }

	public int RequestCount { get; }

	public DateTimeOffset? LastDeniedAt { get; }

	public TimeSpan? CooldownRemaining { get; }

	public bool IsGranted => Status is PermissionStatusKind.Granted or PermissionStatusKind.Limited;
}
