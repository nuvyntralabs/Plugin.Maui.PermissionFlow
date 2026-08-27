namespace Plugin.Maui.PermissionFlow;

/// <summary>
/// When PermissionFlow shows an in-app rationale before the OS dialog.
/// </summary>
public enum RationalePolicy
{
	/// <summary>
	/// Never show an in-app rationale.
	/// </summary>
	Never,

	/// <summary>
	/// Show a rationale the first time a permission is requested.
	/// </summary>
	FirstRequest,

	/// <summary>
	/// Show a rationale after a previous denial, or when the OS says a rationale is useful.
	/// </summary>
	AfterDenial,

	/// <summary>
	/// Always show a rationale before requesting.
	/// </summary>
	Always
}
