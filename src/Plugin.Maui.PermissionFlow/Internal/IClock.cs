namespace Plugin.Maui.PermissionFlow;

interface IClock
{
	DateTimeOffset UtcNow { get; }
}
