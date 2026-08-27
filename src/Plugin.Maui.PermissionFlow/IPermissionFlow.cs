namespace Plugin.Maui.PermissionFlow;

/// <summary>
/// Intelligent permission orchestration: named flows, rationales, cooldowns, and settings handoff.
/// </summary>
public interface IPermissionFlow
{
	/// <summary>
	/// Gets a value indicating whether this target can request OS permissions.
	/// Always <c>true</c> on Android and iOS. The shared <c>net10.0</c> surface is for tests.
	/// </summary>
	bool IsSupported { get; }

	/// <summary>
	/// Gets denial semantics for the current platform.
	/// </summary>
	PermissionFlowPlatformInfo Platform { get; }

	/// <summary>
	/// Gets the flows registered through <c>UsePermissionFlow</c> or <see cref="RegisterFlow"/>.
	/// </summary>
	IReadOnlyList<PermissionFlowDefinition> Flows { get; }

	/// <summary>
	/// Raised when <see cref="EnsureAsync(string, EnsureOptions?, CancellationToken)"/> starts.
	/// </summary>
	event EventHandler<FlowStartedEventArgs>? FlowStarted;

	/// <summary>
	/// Raised after a flow finishes, including skipped and denied outcomes.
	/// </summary>
	event EventHandler<FlowCompletedEventArgs>? FlowCompleted;

	/// <summary>
	/// Raised when a permission status changes during a request.
	/// </summary>
	event EventHandler<PermissionChangedEventArgs>? PermissionChanged;

	/// <summary>
	/// Adds or replaces a named flow at runtime.
	/// </summary>
	void RegisterFlow(PermissionFlowDefinition definition);

	/// <summary>
	/// Checks one permission without prompting.
	/// </summary>
	Task<PermissionState> CheckAsync(AppPermission permission, CancellationToken cancellationToken = default);

	/// <summary>
	/// Checks many permissions without prompting. When <paramref name="permissions"/> is omitted,
	/// every registered flow's permissions are included.
	/// </summary>
	Task<PermissionSnapshot> GetSnapshotAsync(IEnumerable<AppPermission>? permissions = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Runs a registered flow: rationale, OS prompt, cooldown, and settings offer as configured.
	/// </summary>
	Task<PermissionFlowResult> EnsureAsync(string flowId, EnsureOptions? options = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Runs an inline flow definition without registering it.
	/// </summary>
	Task<PermissionFlowResult> EnsureAsync(PermissionFlowDefinition definition, EnsureOptions? options = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Ensures a set of required permissions as an ad-hoc flow.
	/// </summary>
	Task<PermissionFlowResult> EnsureAsync(IEnumerable<AppPermission> permissions, EnsureOptions? options = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Opens the OS application settings page.
	/// </summary>
	Task OpenSettingsAsync();

	/// <summary>
	/// Clears persisted request/denial history. When <paramref name="permission"/> is omitted, all history is cleared.
	/// </summary>
	void ResetHistory(AppPermission? permission = null);

	/// <summary>
	/// Enables or disables plugin diagnostics.
	/// </summary>
	void EnableLogging(bool enabled, IPermissionFlowLogger? logger = null);
}
