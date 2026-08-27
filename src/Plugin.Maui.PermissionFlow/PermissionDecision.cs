namespace Plugin.Maui.PermissionFlow;

/// <summary>
/// Outcome of one permission step inside a flow.
/// </summary>
public sealed class PermissionDecision
{
	public PermissionDecision(
		AppPermission permission,
		PermissionRequirement requirement,
		PermissionStatusKind status,
		PermissionOutcome outcome,
		string? message = null,
		TimeSpan? cooldownRemaining = null)
	{
		Permission = permission;
		Requirement = requirement;
		Status = status;
		Outcome = outcome;
		Message = message;
		CooldownRemaining = cooldownRemaining;
	}

	public AppPermission Permission { get; }

	public PermissionRequirement Requirement { get; }

	public PermissionStatusKind Status { get; }

	public PermissionOutcome Outcome { get; }

	public string? Message { get; }

	public TimeSpan? CooldownRemaining { get; }

	public bool IsSatisfied(bool acceptLimited) =>
		Status is PermissionStatusKind.Unavailable or PermissionStatusKind.Granted
		|| (acceptLimited && Status == PermissionStatusKind.Limited);
}
