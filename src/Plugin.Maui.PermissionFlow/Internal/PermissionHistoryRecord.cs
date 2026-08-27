namespace Plugin.Maui.PermissionFlow;

sealed class PermissionHistoryRecord
{
	public bool WasRequested { get; set; }

	public bool IsPermanentlyDenied { get; set; }

	public int RequestCount { get; set; }

	public DateTimeOffset? LastRequestedAt { get; set; }

	public DateTimeOffset? LastDeniedAt { get; set; }
}
