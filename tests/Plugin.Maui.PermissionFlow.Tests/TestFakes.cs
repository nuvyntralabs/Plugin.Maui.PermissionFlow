namespace Plugin.Maui.PermissionFlow.Tests;

sealed class FakeClock : IClock
{
	public DateTimeOffset UtcNow { get; set; } = new(2026, 8, 27, 10, 0, 0, TimeSpan.Zero);

	public void Advance(TimeSpan duration) => UtcNow += duration;
}

sealed class FakePermissionProbe : IPermissionProbe
{
	public bool IsSupported { get; set; } = true;

	public PermissionFlowPlatformInfo Platform { get; set; } = PermissionFlowPlatformInfo.Android;

	public Dictionary<AppPermission, PermissionStatusKind> Statuses { get; } = [];

	public Dictionary<AppPermission, PermissionStatusKind> RequestResults { get; } = [];

	public HashSet<AppPermission> Unavailable { get; } = [];

	public HashSet<AppPermission> ShouldShowRationaleFor { get; } = [];

	public List<AppPermission> Requested { get; } = [];

	public bool IsAvailable(AppPermission permission) => !Unavailable.Contains(permission);

	public bool ShouldShowRationale(AppPermission permission) => ShouldShowRationaleFor.Contains(permission);

	public Task<PermissionStatusKind> CheckAsync(AppPermission permission, CancellationToken cancellationToken) =>
		Task.FromResult(Statuses.GetValueOrDefault(permission, PermissionStatusKind.NotDetermined));

	public Task<PermissionStatusKind> RequestAsync(AppPermission permission, CancellationToken cancellationToken)
	{
		Requested.Add(permission);
		var result = RequestResults.GetValueOrDefault(permission, PermissionStatusKind.Granted);
		Statuses[permission] = result;
		return Task.FromResult(result);
	}

	public void Set(AppPermission permission, PermissionStatusKind status) => Statuses[permission] = status;
}

sealed class FakeServiceProbe : IDeviceServiceProbe
{
	public Dictionary<DeviceService, bool> Enabled { get; } = new()
	{
		[DeviceService.Location] = true,
		[DeviceService.Bluetooth] = true
	};

	public bool IsEnabled(DeviceService service) => Enabled.GetValueOrDefault(service, true);
}

sealed class FakeRationalePresenter : IPermissionRationalePresenter
{
	public RationaleDecision Decision { get; set; } = RationaleDecision.Continue;

	public List<RationaleRequest> Requests { get; } = [];

	public Task<RationaleDecision> PresentAsync(RationaleRequest request, CancellationToken cancellationToken = default)
	{
		Requests.Add(request);
		return Task.FromResult(Decision);
	}
}

sealed class FakeSettingsPresenter : IPermissionSettingsPresenter
{
	public RationaleDecision Decision { get; set; } = RationaleDecision.Decline;

	public List<SettingsOfferRequest> Requests { get; } = [];

	public Task<RationaleDecision> PresentAsync(SettingsOfferRequest request, CancellationToken cancellationToken = default)
	{
		Requests.Add(request);
		return Task.FromResult(Decision);
	}
}

sealed class FakeSettingsNavigator : IPermissionSettingsNavigator
{
	public int OpenCount { get; private set; }

	public Task OpenSettingsAsync()
	{
		OpenCount++;
		return Task.CompletedTask;
	}
}

static class FlowHarness
{
	public static PermissionFlowImplementation Create(
		Action<PermissionFlowOptions>? configure = null,
		FakePermissionProbe? probe = null,
		FakeServiceProbe? services = null,
		IPermissionHistoryStore? store = null,
		FakeClock? clock = null,
		FakeRationalePresenter? rationale = null,
		FakeSettingsPresenter? settings = null,
		FakeSettingsNavigator? navigator = null)
	{
		var options = new PermissionFlowOptions
		{
			DefaultRationalePolicy = RationalePolicy.Never,
			OfferSettingsWhenPermanentlyDenied = false,
			DefaultDenialCooldown = TimeSpan.FromHours(24)
		};
		configure?.Invoke(options);

		return PermissionFlow.Create(
			options,
			probe ?? new FakePermissionProbe(),
			services ?? new FakeServiceProbe(),
			store ?? new MemoryHistoryStore(),
			clock ?? new FakeClock(),
			rationale ?? new FakeRationalePresenter(),
			settings ?? new FakeSettingsPresenter(),
			navigator ?? new FakeSettingsNavigator());
	}

	public static PermissionFlowDefinition CameraFlow(string id = "scan") =>
		new PermissionFlowBuilder(id)
			.Require(AppPermission.Camera, "Needed to scan codes.")
			.Build();
}
