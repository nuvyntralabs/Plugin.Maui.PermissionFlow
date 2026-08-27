namespace Plugin.Maui.PermissionFlow;

/// <summary>
/// Shared configuration applied when the plugin is registered with <c>UsePermissionFlow</c>.
/// </summary>
public sealed class PermissionFlowOptions
{
	public static readonly TimeSpan StandardDenialCooldown = TimeSpan.FromDays(1);

	readonly List<PermissionFlowDefinition> _flows = [];
	TimeSpan _defaultDenialCooldown = StandardDenialCooldown;

	/// <summary>
	/// Gets or sets a value indicating whether plugin logging starts enabled.
	/// </summary>
	public bool EnableLogging { get; set; }

	/// <summary>
	/// Gets or sets a custom logger. When <c>null</c>, the plugin uses Microsoft.Extensions.Logging if available, otherwise a debug logger.
	/// </summary>
	public IPermissionFlowLogger? Logger { get; set; }

	/// <summary>
	/// Gets or sets how long to wait after a denial before prompting again. Default is 24 hours.
	/// </summary>
	public TimeSpan DefaultDenialCooldown
	{
		get => _defaultDenialCooldown;
		set
		{
			if (value < TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(value), "DefaultDenialCooldown cannot be negative.");

			_defaultDenialCooldown = value;
		}
	}

	/// <summary>
	/// Gets or sets when in-app rationales are shown. Default is <see cref="RationalePolicy.FirstRequest"/>.
	/// </summary>
	public RationalePolicy DefaultRationalePolicy { get; set; } = RationalePolicy.FirstRequest;

	/// <summary>
	/// Gets or sets whether a Settings offer is shown after a permanent denial. Default is <c>true</c>.
	/// </summary>
	public bool OfferSettingsWhenPermanentlyDenied { get; set; } = true;

	/// <summary>
	/// Gets or sets whether iOS Limited / Android partial grants satisfy a required step. Default is <c>true</c>.
	/// </summary>
	public bool AcceptLimited { get; set; } = true;

	public string DefaultContinueText { get; set; } = "Continue";

	public string DefaultNotNowText { get; set; } = "Not now";

	public string DefaultOpenSettingsText { get; set; } = "Open Settings";

	public IPermissionRationalePresenter? RationalePresenter { get; set; }

	public IPermissionSettingsPresenter? SettingsPresenter { get; set; }

	public IPermissionSettingsNavigator? SettingsNavigator { get; set; }

	public IReadOnlyList<PermissionFlowDefinition> Flows => _flows;

	/// <summary>
	/// Registers a named feature flow.
	/// </summary>
	public PermissionFlowOptions AddFlow(string id, Action<PermissionFlowBuilder> configure)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(id);
		ArgumentNullException.ThrowIfNull(configure);

		var builder = new PermissionFlowBuilder(id);
		configure(builder);
		_flows.RemoveAll(flow => string.Equals(flow.Id, id, StringComparison.OrdinalIgnoreCase));
		_flows.Add(builder.Build());
		return this;
	}
}
