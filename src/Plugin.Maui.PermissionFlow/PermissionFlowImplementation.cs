namespace Plugin.Maui.PermissionFlow;

sealed class PermissionFlowImplementation : IPermissionFlow
{
	readonly SemaphoreSlim _gate = new(1, 1);
	readonly PermissionFlowOptions _options;
	readonly Dictionary<string, PermissionFlowDefinition> _flows;
	readonly IPermissionProbe _probe;
	readonly IDeviceServiceProbe _services;
	readonly IPermissionHistoryStore _store;
	readonly IClock _clock;
	readonly IPermissionRationalePresenter _rationale;
	readonly IPermissionSettingsPresenter _settings;
	readonly IPermissionSettingsNavigator _navigator;

	IPermissionFlowLogger? _logger;
	bool _logging;

	public PermissionFlowImplementation(
		PermissionFlowOptions options,
		IPermissionProbe probe,
		IDeviceServiceProbe services,
		IPermissionHistoryStore store,
		IClock clock,
		IPermissionRationalePresenter rationale,
		IPermissionSettingsPresenter settings,
		IPermissionSettingsNavigator navigator)
	{
		_options = options ?? throw new ArgumentNullException(nameof(options));
		_probe = probe ?? throw new ArgumentNullException(nameof(probe));
		_services = services ?? throw new ArgumentNullException(nameof(services));
		_store = store ?? throw new ArgumentNullException(nameof(store));
		_clock = clock ?? throw new ArgumentNullException(nameof(clock));
		_rationale = rationale ?? throw new ArgumentNullException(nameof(rationale));
		_settings = settings ?? throw new ArgumentNullException(nameof(settings));
		_navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));

		_flows = new Dictionary<string, PermissionFlowDefinition>(StringComparer.OrdinalIgnoreCase);
		foreach (var flow in options.Flows)
			_flows[flow.Id] = flow;

		if (options.EnableLogging)
			EnableLogging(true, options.Logger);
	}

	public bool IsSupported => _probe.IsSupported;

	public PermissionFlowPlatformInfo Platform => _probe.Platform;

	public IReadOnlyList<PermissionFlowDefinition> Flows => _flows.Values.ToArray();

	public event EventHandler<FlowStartedEventArgs>? FlowStarted;

	public event EventHandler<FlowCompletedEventArgs>? FlowCompleted;

	public event EventHandler<PermissionChangedEventArgs>? PermissionChanged;

	public void RegisterFlow(PermissionFlowDefinition definition)
	{
		ArgumentNullException.ThrowIfNull(definition);
		_flows[definition.Id] = definition;
		Log(PermissionFlowLogLevel.Debug, $"Registered flow '{definition.Id}' ({definition.Permissions.Count} permissions).");
	}

	public async Task<PermissionState> CheckAsync(AppPermission permission, CancellationToken cancellationToken = default)
	{
		var cooldown = _options.DefaultDenialCooldown;
		return await ReadStateAsync(permission, cooldown, cancellationToken).ConfigureAwait(false);
	}

	public async Task<PermissionSnapshot> GetSnapshotAsync(IEnumerable<AppPermission>? permissions = null, CancellationToken cancellationToken = default)
	{
		var set = permissions?.Distinct().ToArray()
			?? _flows.Values.SelectMany(flow => flow.Permissions.Select(step => step.Permission)).Distinct().ToArray();

		if (set.Length == 0)
			set = Enum.GetValues<AppPermission>();

		var states = new List<PermissionState>(set.Length);
		foreach (var permission in set)
			states.Add(await ReadStateAsync(permission, _options.DefaultDenialCooldown, cancellationToken).ConfigureAwait(false));

		return new PermissionSnapshot(_clock.UtcNow, states);
	}

	public Task<PermissionFlowResult> EnsureAsync(string flowId, EnsureOptions? options = null, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(flowId);

		if (!_flows.TryGetValue(flowId, out var definition))
			throw new PermissionFlowException(PermissionFlowError.UnknownFlow, $"No permission flow named '{flowId}' is registered.");

		return EnsureAsync(definition, options, cancellationToken);
	}

	public Task<PermissionFlowResult> EnsureAsync(IEnumerable<AppPermission> permissions, EnsureOptions? options = null, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(permissions);

		var builder = new PermissionFlowBuilder("_adhoc");
		foreach (var permission in permissions)
			builder.Require(permission);

		return EnsureAsync(builder.Build(), options, cancellationToken);
	}

	public async Task<PermissionFlowResult> EnsureAsync(PermissionFlowDefinition definition, EnsureOptions? options = null, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(definition);
		options ??= new EnsureOptions();

		await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
		try
		{
			Log(PermissionFlowLogLevel.Information, $"Ensuring flow '{definition.Id}'.");
			FlowStarted?.Invoke(this, new FlowStartedEventArgs(definition.Id, definition.Title));

			var acceptLimited = definition.AcceptLimited ?? _options.AcceptLimited;
			var cooldown = definition.DenialCooldown ?? _options.DefaultDenialCooldown;
			var rationalePolicy = definition.RationalePolicy ?? _options.DefaultRationalePolicy;
			var offerSettings = definition.OfferSettingsWhenPermanentlyDenied ?? _options.OfferSettingsWhenPermanentlyDenied;

			var services = definition.RequiredServices
				.Select(service =>
				{
					var enabled = _services.IsEnabled(service);
					return new ServiceDecision(
						service,
						enabled,
						enabled ? PermissionOutcome.ServiceEnabled : PermissionOutcome.ServiceDisabled,
						enabled ? null : $"{service} services are turned off.");
				})
				.ToArray();

			var decisions = new List<PermissionDecision>();
			foreach (var step in FlowExpander.Expand(definition.Permissions))
			{
				cancellationToken.ThrowIfCancellationRequested();
				decisions.Add(await ProcessStepAsync(
					definition,
					step,
					options,
					acceptLimited,
					cooldown,
					rationalePolicy,
					offerSettings,
					cancellationToken).ConfigureAwait(false));
			}

			var result = PermissionFlowResult.Create(definition, decisions, services, acceptLimited);
			Log(PermissionFlowLogLevel.Information,
				$"Flow '{definition.Id}' finished satisfied={result.IsSatisfied} retry={result.CanRetry} settings={result.ShouldOpenSettings}.");
			FlowCompleted?.Invoke(this, new FlowCompletedEventArgs(result));
			return result;
		}
		finally
		{
			_gate.Release();
		}
	}

	public Task OpenSettingsAsync()
	{
		Log(PermissionFlowLogLevel.Information, "Opening application settings.");
		return _navigator.OpenSettingsAsync();
	}

	public void ResetHistory(AppPermission? permission = null)
	{
		_store.Clear(permission);
		Log(PermissionFlowLogLevel.Information, permission is { } one
			? $"Cleared history for {one}."
			: "Cleared all permission history.");
	}

	public void EnableLogging(bool enabled, IPermissionFlowLogger? logger = null)
	{
		_logging = enabled;
		_logger = enabled ? logger ?? _logger ?? new DebugPermissionFlowLogger() : logger;
	}

	async Task<PermissionDecision> ProcessStepAsync(
		PermissionFlowDefinition definition,
		PermissionStep step,
		EnsureOptions options,
		bool acceptLimited,
		TimeSpan cooldown,
		RationalePolicy rationalePolicy,
		bool offerSettings,
		CancellationToken cancellationToken)
	{
		if (!_probe.IsAvailable(step.Permission))
		{
			Log(PermissionFlowLogLevel.Debug, $"{step.Permission} is not applicable on {Platform.Name}.");
			return new PermissionDecision(step.Permission, step.Requirement, PermissionStatusKind.Unavailable, PermissionOutcome.Unavailable);
		}

		var history = _store.Get(step.Permission);
		var raw = await _probe.CheckAsync(step.Permission, cancellationToken).ConfigureAwait(false);
		var refined = StatusRefiner.Refine(raw, _probe.ShouldShowRationale(step.Permission), history.WasRequested, Platform);

		if (IsSatisfied(refined, acceptLimited))
			return new PermissionDecision(step.Permission, step.Requirement, refined, PermissionOutcome.AlreadyGranted);

		if (refined == PermissionStatusKind.Restricted)
			return new PermissionDecision(step.Permission, step.Requirement, refined, PermissionOutcome.Restricted, "This permission is restricted by the device or organization.");

		if (refined == PermissionStatusKind.PermanentlyDenied)
			return await HandlePermanentAsync(definition, step, offerSettings, options, cancellationToken).ConfigureAwait(false);

		if (!options.Force && history.LastDeniedAt is { } deniedAt)
		{
			var remaining = cooldown - (_clock.UtcNow - deniedAt);
			if (remaining > TimeSpan.Zero)
			{
				Log(PermissionFlowLogLevel.Debug, $"Skipping {step.Permission}; cooldown {remaining} remaining.");
				return new PermissionDecision(
					step.Permission,
					step.Requirement,
					refined,
					PermissionOutcome.SkippedCooldown,
					$"Waiting {Format(remaining)} before asking again.",
					remaining);
			}
		}

		if (!options.SkipRationale && StatusRefiner.ShouldPresentRationale(rationalePolicy, history, _probe.ShouldShowRationale(step.Permission)))
		{
			var request = new RationaleRequest(
				definition.Id,
				definition.Title,
				step.Permission,
				step.Requirement,
				step.Title ?? definition.Title ?? PermissionCatalog.GetDisplayName(step.Permission),
				step.Rationale ?? definition.Description ?? PermissionCatalog.DefaultRationale(step.Permission),
				_options.DefaultContinueText,
				_options.DefaultNotNowText);

			Log(PermissionFlowLogLevel.Debug, $"Showing rationale for {step.Permission}.");
			var decision = await _rationale.PresentAsync(request, cancellationToken).ConfigureAwait(false);
			if (decision == RationaleDecision.Decline)
			{
				_store.MarkDenied(step.Permission, _clock.UtcNow);
				return new PermissionDecision(step.Permission, step.Requirement, refined, PermissionOutcome.RationaleDeclined);
			}
		}

		Log(PermissionFlowLogLevel.Information, $"Requesting {step.Permission}.");
		var requested = await _probe.RequestAsync(step.Permission, cancellationToken).ConfigureAwait(false);
		_store.MarkRequested(step.Permission, _clock.UtcNow);

		var after = StatusRefiner.Refine(requested, _probe.ShouldShowRationale(step.Permission), previouslyRequested: true, Platform);
		RecordDenial(step.Permission, after);

		if (after != refined)
			PermissionChanged?.Invoke(this, new PermissionChangedEventArgs(step.Permission, refined, after));

		if (after == PermissionStatusKind.PermanentlyDenied && offerSettings && !options.SkipSettingsOffer)
			return await OfferSettingsAsync(definition, step, after, cancellationToken).ConfigureAwait(false);

		return new PermissionDecision(step.Permission, step.Requirement, after, ToOutcome(after));
	}

	async Task<PermissionDecision> HandlePermanentAsync(
		PermissionFlowDefinition definition,
		PermissionStep step,
		bool offerSettings,
		EnsureOptions options,
		CancellationToken cancellationToken)
	{
		_store.MarkPermanentlyDenied(step.Permission);

		if (!offerSettings || options.SkipSettingsOffer)
			return new PermissionDecision(step.Permission, step.Requirement, PermissionStatusKind.PermanentlyDenied, PermissionOutcome.PermanentlyDenied);

		return await OfferSettingsAsync(definition, step, PermissionStatusKind.PermanentlyDenied, cancellationToken).ConfigureAwait(false);
	}

	async Task<PermissionDecision> OfferSettingsAsync(
		PermissionFlowDefinition definition,
		PermissionStep step,
		PermissionStatusKind status,
		CancellationToken cancellationToken)
	{
		var request = new SettingsOfferRequest(
			definition.Id,
			definition.Title,
			step.Permission,
			step.Title ?? definition.Title ?? PermissionCatalog.GetDisplayName(step.Permission),
			step.Rationale ?? PermissionCatalog.DefaultSettingsMessage(step.Permission),
			_options.DefaultOpenSettingsText,
			_options.DefaultNotNowText);

		var decision = await _settings.PresentAsync(request, cancellationToken).ConfigureAwait(false);
		if (decision != RationaleDecision.Continue)
			return new PermissionDecision(step.Permission, step.Requirement, status, PermissionOutcome.PermanentlyDenied);

		await _navigator.OpenSettingsAsync().ConfigureAwait(false);
		Log(PermissionFlowLogLevel.Information, $"Opened Settings for {step.Permission}.");
		return new PermissionDecision(step.Permission, step.Requirement, status, PermissionOutcome.SettingsOpened, "Opened application settings.");
	}

	void RecordDenial(AppPermission permission, PermissionStatusKind status)
	{
		if (status is PermissionStatusKind.Denied or PermissionStatusKind.PermanentlyDenied)
			_store.MarkDenied(permission, _clock.UtcNow);

		if (status == PermissionStatusKind.PermanentlyDenied)
			_store.MarkPermanentlyDenied(permission);
	}

	async Task<PermissionState> ReadStateAsync(AppPermission permission, TimeSpan cooldown, CancellationToken cancellationToken)
	{
		if (!_probe.IsAvailable(permission))
		{
			return new PermissionState(
				permission,
				PermissionStatusKind.Unavailable,
				isAvailable: false,
				canRequest: false,
				isPermanentlyDenied: false,
				requestCount: 0,
				lastDeniedAt: null,
				cooldownRemaining: null);
		}

		var history = _store.Get(permission);
		var raw = await _probe.CheckAsync(permission, cancellationToken).ConfigureAwait(false);
		var refined = StatusRefiner.Refine(raw, _probe.ShouldShowRationale(permission), history.WasRequested, Platform);
		var remaining = RemainingCooldown(history.LastDeniedAt, cooldown);

		return new PermissionState(
			permission,
			refined,
			isAvailable: true,
			canRequest: refined is PermissionStatusKind.NotDetermined or PermissionStatusKind.Denied && remaining is null,
			isPermanentlyDenied: refined == PermissionStatusKind.PermanentlyDenied,
			history.RequestCount,
			history.LastDeniedAt,
			remaining);
	}

	TimeSpan? RemainingCooldown(DateTimeOffset? lastDeniedAt, TimeSpan cooldown)
	{
		if (lastDeniedAt is null || cooldown <= TimeSpan.Zero)
			return null;

		var remaining = cooldown - (_clock.UtcNow - lastDeniedAt.Value);
		return remaining > TimeSpan.Zero ? remaining : null;
	}

	static bool IsSatisfied(PermissionStatusKind status, bool acceptLimited) =>
		status is PermissionStatusKind.Granted
		|| (acceptLimited && status == PermissionStatusKind.Limited);

	static PermissionOutcome ToOutcome(PermissionStatusKind status) => status switch
	{
		PermissionStatusKind.Granted or PermissionStatusKind.Limited => PermissionOutcome.Granted,
		PermissionStatusKind.PermanentlyDenied => PermissionOutcome.PermanentlyDenied,
		PermissionStatusKind.Restricted => PermissionOutcome.Restricted,
		PermissionStatusKind.Unavailable => PermissionOutcome.Unavailable,
		_ => PermissionOutcome.Denied
	};

	static string Format(TimeSpan value) =>
		value.TotalHours >= 1 ? $"{value.TotalHours:0.#} hours" :
		value.TotalMinutes >= 1 ? $"{value.TotalMinutes:0} minutes" :
		$"{Math.Max(1, (int)value.TotalSeconds)} seconds";

	void Log(PermissionFlowLogLevel level, string message)
	{
		if (_logging)
			(_logger ?? new DebugPermissionFlowLogger()).Log(level, message);
	}
}
