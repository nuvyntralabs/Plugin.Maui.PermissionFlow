using Plugin.Maui.PermissionFlow;

namespace PermissionFlow.Sample;

public partial class MainPage : ContentPage, IPermissionFlowLogger
{
	readonly IPermissionFlow _flow;
	readonly List<string> _logLines = [];

	public MainPage()
	{
		InitializeComponent();
		_flow = Plugin.Maui.PermissionFlow.PermissionFlow.Current;
		_flow.FlowStarted += OnFlowStarted;
		_flow.FlowCompleted += OnFlowCompleted;
		_flow.PermissionChanged += OnPermissionChanged;
		_flow.EnableLogging(true, this);
		_ = RefreshAsync();
	}

	async void OnScanClicked(object? sender, EventArgs e) => await RunAsync("scan");

	async void OnLocationClicked(object? sender, EventArgs e) => await RunAsync("location");

	async void OnAlertsClicked(object? sender, EventArgs e) => await RunAsync("alerts");

	async void OnLibraryClicked(object? sender, EventArgs e) => await RunAsync("library");

	async void OnForceScanClicked(object? sender, EventArgs e) => await RunAsync("scan", new EnsureOptions { Force = true });

	async void OnRefreshClicked(object? sender, EventArgs e) => await RefreshAsync();

	async void OnSettingsClicked(object? sender, EventArgs e)
	{
		await _flow.OpenSettingsAsync();
		AppendLog("Opened application settings.");
	}

	async void OnResetClicked(object? sender, EventArgs e)
	{
		_flow.ResetHistory();
		AppendLog("Cleared permission history.");
		await RefreshAsync();
	}

	void OnLoggingToggled(object? sender, ToggledEventArgs e)
	{
		_flow.EnableLogging(e.Value, this);
		AppendLog(e.Value ? "Logging enabled by user." : "Logging disabled by user.");
	}

	void OnFlowStarted(object? sender, FlowStartedEventArgs e) =>
		MainThread.BeginInvokeOnMainThread(() => AppendLog($"STARTED {e.FlowId}"));

	void OnFlowCompleted(object? sender, FlowCompletedEventArgs e) =>
		MainThread.BeginInvokeOnMainThread(() =>
		{
			AppendLog($"COMPLETED {e.Result.FlowId} satisfied={e.Result.IsSatisfied}");
			ShowResult(e.Result);
		});

	void OnPermissionChanged(object? sender, PermissionChangedEventArgs e) =>
		MainThread.BeginInvokeOnMainThread(() =>
			AppendLog($"CHANGED {e.Permission}: {e.Previous} -> {e.Current}"));

	async Task RunAsync(string flowId, EnsureOptions? options = null)
	{
		try
		{
			var result = await _flow.EnsureAsync(flowId, options);
			ShowResult(result);
			await RefreshAsync();
		}
		catch (Exception ex)
		{
			AppendLog($"ERROR {ex.Message}");
			ResultLabel.Text = ex.Message;
		}
	}

	async Task RefreshAsync()
	{
		PlatformLabel.Text = _flow.IsSupported
			? $"Platform: {_flow.Platform.Name}  ·  don't-ask-again={_flow.Platform.UsesDontAskAgain}  ·  denial-is-permanent={_flow.Platform.DenialIsPermanent}"
			: "Platform: not supported";

		var snapshot = await _flow.GetSnapshotAsync(
		[
			AppPermission.Camera,
			AppPermission.Photos,
			AppPermission.LocationWhenInUse,
			AppPermission.Notifications
		]);

		SnapshotLabel.Text = string.Join(Environment.NewLine, snapshot.Permissions.Select(state =>
		{
			var cooldown = state.CooldownRemaining is { } remaining
				? $"  ·  cooldown {remaining:hh\\:mm\\:ss}"
				: string.Empty;
			return $"{state.Permission}: {state.Status}  ·  requests={state.RequestCount}{cooldown}";
		}));
	}

	void ShowResult(PermissionFlowResult result)
	{
		var lines = new List<string>
		{
			$"{result.FlowId}: satisfied={result.IsSatisfied} retry={result.CanRetry} settings={result.ShouldOpenSettings}"
		};

		lines.AddRange(result.Services.Select(service =>
			$"  service {service.Service}: {service.Outcome}"));
		lines.AddRange(result.Decisions.Select(decision =>
			$"  {decision.Permission}: {decision.Outcome} ({decision.Status})"));

		ResultLabel.Text = string.Join(Environment.NewLine, lines);
	}

	public void Log(PermissionFlowLogLevel level, string message, Exception? exception = null)
	{
		var line = exception is null
			? $"{DateTime.Now:HH:mm:ss} {level}: {message}"
			: $"{DateTime.Now:HH:mm:ss} {level}: {message} ({exception.GetType().Name})";

		MainThread.BeginInvokeOnMainThread(() => AppendLog(line));
	}

	void AppendLog(string line)
	{
		_logLines.Insert(0, line);
		if (_logLines.Count > 40)
			_logLines.RemoveAt(_logLines.Count - 1);

		LogLabel.Text = string.Join(Environment.NewLine, _logLines);
	}
}
