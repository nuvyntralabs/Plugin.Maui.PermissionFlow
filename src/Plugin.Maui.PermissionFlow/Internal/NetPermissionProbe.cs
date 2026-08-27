#if !ANDROID && !IOS
namespace Plugin.Maui.PermissionFlow;

sealed class NetPermissionProbe : IPermissionProbe
{
	public bool IsSupported => false;

	public PermissionFlowPlatformInfo Platform => PermissionFlowPlatformInfo.Net;

	public bool IsAvailable(AppPermission permission) => true;

	public bool ShouldShowRationale(AppPermission permission) => false;

	public Task<PermissionStatusKind> CheckAsync(AppPermission permission, CancellationToken cancellationToken) =>
		Task.FromResult(PermissionStatusKind.NotDetermined);

	public Task<PermissionStatusKind> RequestAsync(AppPermission permission, CancellationToken cancellationToken) =>
		Task.FromResult(PermissionStatusKind.Denied);
}
#endif
