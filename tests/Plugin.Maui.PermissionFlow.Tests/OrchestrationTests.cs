namespace Plugin.Maui.PermissionFlow.Tests;

public sealed class OrchestrationTests
{
	[Fact]
	public async Task AlreadyGranted_DoesNotRequest()
	{
		var probe = new FakePermissionProbe();
		probe.Set(AppPermission.Camera, PermissionStatusKind.Granted);
		var flow = FlowHarness.Create(probe: probe);

		var result = await flow.EnsureAsync(FlowHarness.CameraFlow());

		Assert.True(result.IsSatisfied);
		Assert.Equal(PermissionOutcome.AlreadyGranted, result[AppPermission.Camera]?.Outcome);
		Assert.Empty(probe.Requested);
	}

	[Fact]
	public async Task RequestGranted_SatisfiesFlow()
	{
		var probe = new FakePermissionProbe();
		probe.RequestResults[AppPermission.Camera] = PermissionStatusKind.Granted;
		var flow = FlowHarness.Create(probe: probe);

		var result = await flow.EnsureAsync(FlowHarness.CameraFlow());

		Assert.True(result.IsSatisfied);
		Assert.Equal(PermissionOutcome.Granted, result[AppPermission.Camera]?.Outcome);
		Assert.Equal(new[] { AppPermission.Camera }, probe.Requested);
	}

	[Fact]
	public async Task RequestDenied_IsNotSatisfied_AndStartsCooldown()
	{
		var probe = new FakePermissionProbe();
		probe.RequestResults[AppPermission.Camera] = PermissionStatusKind.Denied;
		probe.ShouldShowRationaleFor.Add(AppPermission.Camera);
		var flow = FlowHarness.Create(probe: probe);

		var result = await flow.EnsureAsync(FlowHarness.CameraFlow());

		Assert.False(result.IsSatisfied);
		Assert.True(result.CanRetry);
		Assert.Equal(PermissionOutcome.Denied, result[AppPermission.Camera]?.Outcome);
		Assert.Equal(PermissionStatusKind.Denied, result[AppPermission.Camera]?.Status);
	}

	[Fact]
	public async Task Cooldown_SkipsSecondRequest()
	{
		var probe = new FakePermissionProbe();
		probe.RequestResults[AppPermission.Camera] = PermissionStatusKind.Denied;
		probe.ShouldShowRationaleFor.Add(AppPermission.Camera);
		var clock = new FakeClock();
		var store = new MemoryHistoryStore();
		var flow = FlowHarness.Create(probe: probe, store: store, clock: clock);

		await flow.EnsureAsync(FlowHarness.CameraFlow());
		probe.Requested.Clear();

		var second = await flow.EnsureAsync(FlowHarness.CameraFlow());

		Assert.Equal(PermissionOutcome.SkippedCooldown, second[AppPermission.Camera]?.Outcome);
		Assert.Empty(probe.Requested);
		Assert.True(second[AppPermission.Camera]?.CooldownRemaining > TimeSpan.Zero);
	}

	[Fact]
	public async Task CooldownElapsed_RequestsAgain()
	{
		var probe = new FakePermissionProbe();
		probe.RequestResults[AppPermission.Camera] = PermissionStatusKind.Denied;
		probe.ShouldShowRationaleFor.Add(AppPermission.Camera);
		var clock = new FakeClock();
		var store = new MemoryHistoryStore();
		var flow = FlowHarness.Create(probe: probe, store: store, clock: clock);

		await flow.EnsureAsync(FlowHarness.CameraFlow());
		clock.Advance(TimeSpan.FromHours(25));
		probe.Requested.Clear();
		probe.Set(AppPermission.Camera, PermissionStatusKind.Denied);
		probe.RequestResults[AppPermission.Camera] = PermissionStatusKind.Granted;

		var second = await flow.EnsureAsync(FlowHarness.CameraFlow());

		Assert.True(second.IsSatisfied);
		Assert.Equal(new[] { AppPermission.Camera }, probe.Requested);
	}

	[Fact]
	public async Task Force_BypassesCooldown()
	{
		var probe = new FakePermissionProbe();
		probe.RequestResults[AppPermission.Camera] = PermissionStatusKind.Denied;
		probe.ShouldShowRationaleFor.Add(AppPermission.Camera);
		var flow = FlowHarness.Create(probe: probe);

		await flow.EnsureAsync(FlowHarness.CameraFlow());
		probe.Requested.Clear();
		probe.Set(AppPermission.Camera, PermissionStatusKind.Denied);
		probe.RequestResults[AppPermission.Camera] = PermissionStatusKind.Granted;

		var second = await flow.EnsureAsync(FlowHarness.CameraFlow(), new EnsureOptions { Force = true });

		Assert.True(second.IsSatisfied);
		Assert.Equal(new[] { AppPermission.Camera }, probe.Requested);
	}

	[Fact]
	public async Task RationaleDeclined_DoesNotRequest()
	{
		var probe = new FakePermissionProbe();
		var rationale = new FakeRationalePresenter { Decision = RationaleDecision.Decline };
		var flow = FlowHarness.Create(
			configure: options => options.DefaultRationalePolicy = RationalePolicy.Always,
			probe: probe,
			rationale: rationale);

		var result = await flow.EnsureAsync(FlowHarness.CameraFlow());

		Assert.False(result.IsSatisfied);
		Assert.Equal(PermissionOutcome.RationaleDeclined, result[AppPermission.Camera]?.Outcome);
		Assert.Empty(probe.Requested);
		Assert.Single(rationale.Requests);
	}

	[Fact]
	public async Task FirstRequestPolicy_ShowsRationaleOnce()
	{
		var probe = new FakePermissionProbe();
		probe.RequestResults[AppPermission.Camera] = PermissionStatusKind.Denied;
		probe.ShouldShowRationaleFor.Add(AppPermission.Camera);
		var rationale = new FakeRationalePresenter();
		var store = new MemoryHistoryStore();
		var flow = FlowHarness.Create(
			configure: options => options.DefaultRationalePolicy = RationalePolicy.FirstRequest,
			probe: probe,
			store: store,
			rationale: rationale);

		await flow.EnsureAsync(FlowHarness.CameraFlow());
		Assert.Single(rationale.Requests);

		rationale.Requests.Clear();
		probe.Set(AppPermission.Camera, PermissionStatusKind.Denied);
		probe.RequestResults[AppPermission.Camera] = PermissionStatusKind.Granted;

		await flow.EnsureAsync(FlowHarness.CameraFlow(), new EnsureOptions { Force = true });

		Assert.Empty(rationale.Requests);
	}

	[Fact]
	public async Task OptionalDenial_StillSatisfiesRequired()
	{
		var probe = new FakePermissionProbe();
		probe.RequestResults[AppPermission.Camera] = PermissionStatusKind.Granted;
		probe.RequestResults[AppPermission.Photos] = PermissionStatusKind.Denied;
		probe.ShouldShowRationaleFor.Add(AppPermission.Photos);
		var definition = new PermissionFlowBuilder("scan")
			.Require(AppPermission.Camera)
			.Optional(AppPermission.Photos)
			.Build();
		var flow = FlowHarness.Create(probe: probe);

		var result = await flow.EnsureAsync(definition);

		Assert.True(result.IsSatisfied);
		Assert.Equal(PermissionOutcome.Denied, result[AppPermission.Photos]?.Outcome);
	}

	[Fact]
	public async Task UnknownFlow_Throws()
	{
		var flow = FlowHarness.Create();

		var ex = await Assert.ThrowsAsync<PermissionFlowException>(() => flow.EnsureAsync("missing"));

		Assert.Equal(PermissionFlowError.UnknownFlow, ex.Error);
	}

	[Fact]
	public async Task LocationAlways_RequestsWhenInUseFirst()
	{
		var probe = new FakePermissionProbe();
		probe.RequestResults[AppPermission.LocationWhenInUse] = PermissionStatusKind.Granted;
		probe.RequestResults[AppPermission.LocationAlways] = PermissionStatusKind.Granted;
		var definition = new PermissionFlowBuilder("track")
			.Require(AppPermission.LocationAlways)
			.Build();
		var flow = FlowHarness.Create(probe: probe);

		var result = await flow.EnsureAsync(definition);

		Assert.True(result.IsSatisfied);
		Assert.Equal(
			new[] { AppPermission.LocationWhenInUse, AppPermission.LocationAlways },
			probe.Requested);
	}

	[Fact]
	public async Task UnavailableRequired_IsTreatedAsNotApplicable()
	{
		var probe = new FakePermissionProbe();
		probe.Unavailable.Add(AppPermission.Reminders);
		probe.RequestResults[AppPermission.ContactsRead] = PermissionStatusKind.Granted;
		var definition = new PermissionFlowBuilder("contacts")
			.Require(AppPermission.ContactsRead)
			.Require(AppPermission.Reminders)
			.Build();
		var flow = FlowHarness.Create(probe: probe);

		var result = await flow.EnsureAsync(definition);

		Assert.True(result.IsSatisfied);
		Assert.Equal(PermissionOutcome.Unavailable, result[AppPermission.Reminders]?.Outcome);
	}

	[Fact]
	public async Task AcceptLimited_SatisfiesRequired()
	{
		var probe = new FakePermissionProbe();
		probe.Set(AppPermission.Photos, PermissionStatusKind.Limited);
		var definition = new PermissionFlowBuilder("library")
			.Require(AppPermission.Photos)
			.Build();
		var flow = FlowHarness.Create(configure: options => options.AcceptLimited = true, probe: probe);

		var result = await flow.EnsureAsync(definition);

		Assert.True(result.IsSatisfied);
		Assert.Equal(PermissionOutcome.AlreadyGranted, result[AppPermission.Photos]?.Outcome);
	}

	[Fact]
	public async Task RejectLimited_DoesNotSatisfy()
	{
		var probe = new FakePermissionProbe();
		probe.Set(AppPermission.Photos, PermissionStatusKind.Limited);
		probe.RequestResults[AppPermission.Photos] = PermissionStatusKind.Limited;
		var definition = new PermissionFlowBuilder("library")
			.Require(AppPermission.Photos)
			.Build();
		var flow = FlowHarness.Create(configure: options => options.AcceptLimited = false, probe: probe);

		var result = await flow.EnsureAsync(definition);

		Assert.False(result.IsSatisfied);
		Assert.Equal(PermissionStatusKind.Limited, result[AppPermission.Photos]?.Status);
	}

	[Fact]
	public async Task ServiceDisabled_FailsRequiredFlow()
	{
		var services = new FakeServiceProbe();
		services.Enabled[DeviceService.Location] = false;
		var probe = new FakePermissionProbe();
		probe.Set(AppPermission.LocationWhenInUse, PermissionStatusKind.Granted);
		var definition = new PermissionFlowBuilder("here")
			.Require(AppPermission.LocationWhenInUse)
			.RequireService(DeviceService.Location)
			.Build();
		var flow = FlowHarness.Create(probe: probe, services: services);

		var result = await flow.EnsureAsync(definition);

		Assert.False(result.IsSatisfied);
		Assert.True(result.CanRetry);
		Assert.Equal(PermissionOutcome.ServiceDisabled, result.Services.Single().Outcome);
	}

	[Fact]
	public async Task AndroidPermanentDenial_OffersSettings()
	{
		var probe = new FakePermissionProbe { Platform = PermissionFlowPlatformInfo.Android };
		probe.Set(AppPermission.Camera, PermissionStatusKind.Denied);
		var store = new MemoryHistoryStore();
		store.MarkRequested(AppPermission.Camera, DateTimeOffset.UtcNow);
		var settings = new FakeSettingsPresenter { Decision = RationaleDecision.Continue };
		var navigator = new FakeSettingsNavigator();
		var flow = FlowHarness.Create(
			configure: options => options.OfferSettingsWhenPermanentlyDenied = true,
			probe: probe,
			store: store,
			settings: settings,
			navigator: navigator);

		var result = await flow.EnsureAsync(FlowHarness.CameraFlow());

		Assert.False(result.IsSatisfied);
		Assert.Equal(PermissionOutcome.SettingsOpened, result[AppPermission.Camera]?.Outcome);
		Assert.Equal(1, navigator.OpenCount);
		Assert.Single(settings.Requests);
	}

	[Fact]
	public async Task IosDenial_IsPermanent()
	{
		var probe = new FakePermissionProbe { Platform = PermissionFlowPlatformInfo.iOS };
		probe.RequestResults[AppPermission.Camera] = PermissionStatusKind.Denied;
		var settings = new FakeSettingsPresenter { Decision = RationaleDecision.Decline };
		var flow = FlowHarness.Create(
			configure: options => options.OfferSettingsWhenPermanentlyDenied = true,
			probe: probe,
			settings: settings);

		var result = await flow.EnsureAsync(FlowHarness.CameraFlow());

		Assert.Equal(PermissionStatusKind.PermanentlyDenied, result[AppPermission.Camera]?.Status);
		Assert.Equal(PermissionOutcome.PermanentlyDenied, result[AppPermission.Camera]?.Outcome);
		Assert.True(result.ShouldOpenSettings);
		Assert.Single(settings.Requests);
	}

	[Fact]
	public async Task RegisteredFlow_IsResolvedById()
	{
		var probe = new FakePermissionProbe();
		probe.Set(AppPermission.Notifications, PermissionStatusKind.Granted);
		var flow = FlowHarness.Create(
			configure: options => options.AddFlow("alerts", builder => builder.Require(AppPermission.Notifications)),
			probe: probe);

		var result = await flow.EnsureAsync("alerts");

		Assert.True(result.IsSatisfied);
		Assert.Equal("alerts", result.FlowId);
	}

	[Fact]
	public async Task AdHocEnsure_RequiresAllListedPermissions()
	{
		var probe = new FakePermissionProbe();
		probe.RequestResults[AppPermission.Camera] = PermissionStatusKind.Granted;
		probe.RequestResults[AppPermission.Microphone] = PermissionStatusKind.Granted;
		var flow = FlowHarness.Create(probe: probe);

		var result = await flow.EnsureAsync([AppPermission.Camera, AppPermission.Microphone]);

		Assert.True(result.IsSatisfied);
		Assert.Equal(2, probe.Requested.Count);
	}

	[Fact]
	public async Task ResetHistory_AllowsImmediateRetry()
	{
		var probe = new FakePermissionProbe();
		probe.RequestResults[AppPermission.Camera] = PermissionStatusKind.Denied;
		probe.ShouldShowRationaleFor.Add(AppPermission.Camera);
		var flow = FlowHarness.Create(probe: probe);

		await flow.EnsureAsync(FlowHarness.CameraFlow());
		flow.ResetHistory(AppPermission.Camera);
		probe.Requested.Clear();
		probe.Set(AppPermission.Camera, PermissionStatusKind.Denied);
		probe.RequestResults[AppPermission.Camera] = PermissionStatusKind.Granted;

		var second = await flow.EnsureAsync(FlowHarness.CameraFlow());

		Assert.True(second.IsSatisfied);
		Assert.Single(probe.Requested);
	}

	[Fact]
	public async Task Events_FireForFlowLifecycle()
	{
		var probe = new FakePermissionProbe();
		probe.RequestResults[AppPermission.Camera] = PermissionStatusKind.Granted;
		var flow = FlowHarness.Create(probe: probe);
		string? started = null;
		string? completed = null;
		AppPermission? changed = null;

		flow.FlowStarted += (_, e) => started = e.FlowId;
		flow.FlowCompleted += (_, e) => completed = e.Result.FlowId;
		flow.PermissionChanged += (_, e) => changed = e.Permission;

		await flow.EnsureAsync(FlowHarness.CameraFlow());

		Assert.Equal("scan", started);
		Assert.Equal("scan", completed);
		Assert.Equal(AppPermission.Camera, changed);
	}

	[Fact]
	public async Task Snapshot_IncludesCooldown()
	{
		var probe = new FakePermissionProbe();
		probe.RequestResults[AppPermission.Camera] = PermissionStatusKind.Denied;
		probe.ShouldShowRationaleFor.Add(AppPermission.Camera);
		var flow = FlowHarness.Create(
			configure: options => options.AddFlow("scan", builder => builder.Require(AppPermission.Camera)),
			probe: probe);

		await flow.EnsureAsync("scan");
		var snapshot = await flow.GetSnapshotAsync([AppPermission.Camera]);

		Assert.NotNull(snapshot[AppPermission.Camera]?.CooldownRemaining);
		Assert.False(snapshot[AppPermission.Camera]!.CanRequest);
	}

	[Fact]
	public void StatusRefiner_AndroidDontAskAgain()
	{
		var status = StatusRefiner.Refine(
			PermissionStatusKind.Denied,
			shouldShowRationale: false,
			previouslyRequested: true,
			PermissionFlowPlatformInfo.Android);

		Assert.Equal(PermissionStatusKind.PermanentlyDenied, status);
	}

	[Fact]
	public void StatusRefiner_AndroidFirstLaunchLooksUndetermined()
	{
		var status = StatusRefiner.Refine(
			PermissionStatusKind.Denied,
			shouldShowRationale: false,
			previouslyRequested: false,
			PermissionFlowPlatformInfo.Android);

		Assert.Equal(PermissionStatusKind.NotDetermined, status);
	}
}
