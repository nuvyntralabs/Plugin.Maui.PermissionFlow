namespace Plugin.Maui.PermissionFlow;

/// <summary>
/// One permission in a named flow, including rationale copy.
/// </summary>
public sealed class PermissionStep
{
	public PermissionStep(
		AppPermission permission,
		PermissionRequirement requirement,
		string? title = null,
		string? rationale = null,
		bool isImplied = false)
	{
		Permission = permission;
		Requirement = requirement;
		Title = title;
		Rationale = rationale;
		IsImplied = isImplied;
	}

	public AppPermission Permission { get; }

	public PermissionRequirement Requirement { get; }

	public string? Title { get; }

	public string? Rationale { get; }

	/// <summary>
	/// Gets a value indicating whether this step was inserted as a dependency
	/// (for example <see cref="AppPermission.LocationWhenInUse"/> before Always).
	/// </summary>
	public bool IsImplied { get; }

	internal PermissionStep With(
		AppPermission? permission = null,
		PermissionRequirement? requirement = null,
		bool? isImplied = null) =>
		new(
			permission ?? Permission,
			requirement ?? Requirement,
			Title,
			Rationale,
			isImplied ?? IsImplied);
}
