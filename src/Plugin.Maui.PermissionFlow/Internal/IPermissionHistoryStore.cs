namespace Plugin.Maui.PermissionFlow;

interface IPermissionHistoryStore
{
	PermissionHistoryRecord Get(AppPermission permission);

	void MarkRequested(AppPermission permission, DateTimeOffset utcNow);

	void MarkDenied(AppPermission permission, DateTimeOffset utcNow);

	void MarkPermanentlyDenied(AppPermission permission);

	void Clear(AppPermission? permission = null);
}
