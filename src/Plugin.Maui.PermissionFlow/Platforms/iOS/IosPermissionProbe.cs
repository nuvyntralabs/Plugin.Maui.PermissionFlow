#if IOS
namespace Plugin.Maui.PermissionFlow;

sealed class IosPermissionProbe : IPermissionProbe
{
	public bool IsSupported => true;

	public PermissionFlowPlatformInfo Platform => PermissionFlowPlatformInfo.iOS;

	public bool IsAvailable(AppPermission permission) => permission switch
	{
		AppPermission.Phone => false,
		AppPermission.Sms => false,
		AppPermission.StorageRead => false,
		AppPermission.StorageWrite => false,
		AppPermission.NearbyWifi => false,
		AppPermission.Media => false,
		_ => true
	};

	public bool ShouldShowRationale(AppPermission permission) => false;

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
