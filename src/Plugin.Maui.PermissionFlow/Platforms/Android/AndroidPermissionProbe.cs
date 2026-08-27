#if ANDROID
namespace Plugin.Maui.PermissionFlow;

sealed class AndroidPermissionProbe : IPermissionProbe
{
	public bool IsSupported => true;

	public PermissionFlowPlatformInfo Platform => PermissionFlowPlatformInfo.Android;

	public bool IsAvailable(AppPermission permission) => permission switch
	{
		AppPermission.Reminders => false,
		AppPermission.Notifications => OperatingSystem.IsAndroidVersionAtLeast(33),
		AppPermission.NearbyWifi => OperatingSystem.IsAndroidVersionAtLeast(33),
		AppPermission.Media => OperatingSystem.IsAndroidVersionAtLeast(33),
		_ => true
	};

	public bool ShouldShowRationale(AppPermission permission) =>
		IsAvailable(permission) && MauiPermissionGateway.ShouldShowRationale(permission);

	public Task<PermissionStatusKind> CheckAsync(AppPermission permission, CancellationToken cancellationToken) =>
		IsAvailable(permission)
			? MauiPermissionGateway.CheckAsync(permission, cancellationToken)
			: Task.FromResult(PermissionStatusKind.Unavailable);

	public Task<PermissionStatusKind> RequestAsync(AppPermission permission, CancellationToken cancellationToken) =>
		IsAvailable(permission)
			? MauiPermissionGateway.RequestAsync(permission, cancellationToken)
			: Task.FromResult(PermissionStatusKind.Unavailable);
}
#endif
