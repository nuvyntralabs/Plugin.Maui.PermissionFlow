namespace Plugin.Maui.PermissionFlow;

sealed class SystemClock : IClock
{
	public static SystemClock Instance { get; } = new();

	public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
