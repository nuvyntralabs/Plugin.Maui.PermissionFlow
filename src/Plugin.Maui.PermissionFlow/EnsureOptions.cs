namespace Plugin.Maui.PermissionFlow;

/// <summary>
/// Per-call overrides for <see cref="IPermissionFlow.EnsureAsync(string, EnsureOptions?, CancellationToken)"/>.
/// </summary>
public sealed class EnsureOptions
{
	/// <summary>
	/// Ignores denial cooldown and attempts the OS prompt again.
	/// </summary>
	public bool Force { get; set; }

	/// <summary>
	/// Skips the in-app rationale even when the flow policy would show one.
	/// </summary>
	public bool SkipRationale { get; set; }

	/// <summary>
	/// Skips offering to open Settings after a permanent denial.
	/// </summary>
	public bool SkipSettingsOffer { get; set; }
}
