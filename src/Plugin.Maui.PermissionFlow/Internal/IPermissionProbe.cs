namespace Plugin.Maui.PermissionFlow;

interface IPermissionProbe
{
	bool IsSupported { get; }

	PermissionFlowPlatformInfo Platform { get; }

	bool IsAvailable(AppPermission permission);

	bool ShouldShowRationale(AppPermission permission);

	Task<PermissionStatusKind> CheckAsync(AppPermission permission, CancellationToken cancellationToken);

	Task<PermissionStatusKind> RequestAsync(AppPermission permission, CancellationToken cancellationToken);
}
