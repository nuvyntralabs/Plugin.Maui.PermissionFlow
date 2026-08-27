namespace Plugin.Maui.PermissionFlow;

/// <summary>
/// Aggregated result of ensuring a named or ad-hoc permission flow.
/// </summary>
public sealed class PermissionFlowResult
{
	public PermissionFlowResult(
		string flowId,
		string? flowTitle,
		bool isSatisfied,
		bool canRetry,
		bool shouldOpenSettings,
		IReadOnlyList<PermissionDecision> decisions,
		IReadOnlyList<ServiceDecision> services)
	{
		FlowId = flowId;
		FlowTitle = flowTitle;
		IsSatisfied = isSatisfied;
		CanRetry = canRetry;
		ShouldOpenSettings = shouldOpenSettings;
		Decisions = decisions;
		Services = services;
	}

	public string FlowId { get; }

	public string? FlowTitle { get; }

	/// <summary>
	/// All required permissions are granted (or limited when accepted) and required services are on.
	/// Platform-unavailable required permissions are treated as not applicable.
	/// </summary>
	public bool IsSatisfied { get; }

	/// <summary>
	/// Another attempt might change the outcome (cooldown elapsed, user changed settings, or a retryable denial).
	/// </summary>
	public bool CanRetry { get; }

	/// <summary>
	/// At least one required permission is permanently denied and Settings was not opened this pass.
	/// </summary>
	public bool ShouldOpenSettings { get; }

	public IReadOnlyList<PermissionDecision> Decisions { get; }

	public IReadOnlyList<ServiceDecision> Services { get; }

	public PermissionDecision? this[AppPermission permission] =>
		Decisions.FirstOrDefault(decision => decision.Permission == permission);

	internal static PermissionFlowResult Create(
		PermissionFlowDefinition definition,
		IReadOnlyList<PermissionDecision> decisions,
		IReadOnlyList<ServiceDecision> services,
		bool acceptLimited)
	{
		var requiredOk = decisions
			.Where(decision => decision.Requirement == PermissionRequirement.Required)
			.All(decision => decision.IsSatisfied(acceptLimited));
		var servicesOk = services.All(service => service.IsEnabled);
		var isSatisfied = requiredOk && servicesOk;

		var canRetry = !isSatisfied && (
			services.Any(service => !service.IsEnabled)
			|| decisions.Any(decision => decision.Outcome is
				PermissionOutcome.Denied
				or PermissionOutcome.RationaleDeclined
				or PermissionOutcome.SkippedCooldown
				or PermissionOutcome.SettingsOpened
				or PermissionOutcome.ServiceDisabled));

		var shouldOpenSettings = !isSatisfied && decisions.Any(decision =>
			decision.Requirement == PermissionRequirement.Required
			&& decision.Status == PermissionStatusKind.PermanentlyDenied
			&& decision.Outcome != PermissionOutcome.SettingsOpened);

		return new PermissionFlowResult(
			definition.Id,
			definition.Title,
			isSatisfied,
			canRetry,
			shouldOpenSettings,
			decisions,
			services);
	}
}
