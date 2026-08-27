namespace Plugin.Maui.PermissionFlow;

/// <summary>
/// Entry point for the PermissionFlow plugin when dependency injection is not used.
/// </summary>
public static class PermissionFlow
{
	static IPermissionFlow? _current;

	/// <summary>
	/// Gets the shared <see cref="IPermissionFlow"/> instance.
	/// </summary>
	public static IPermissionFlow Current => _current ??= Create(new PermissionFlowOptions());

	/// <summary>
	/// Creates a new instance using MAUI permissions, preferences, and display-alert presenters.
	/// </summary>
	public static IPermissionFlow Create(PermissionFlowOptions? options = null)
	{
		options ??= new PermissionFlowOptions();
		return new PermissionFlowImplementation(
			options,
			CreateProbe(),
			CreateServiceProbe(),
			new PreferencesHistoryStore(),
			SystemClock.Instance,
			options.RationalePresenter ?? new DisplayAlertRationalePresenter(),
			options.SettingsPresenter ?? new DisplayAlertSettingsPresenter(),
			options.SettingsNavigator ?? new MauiSettingsNavigator());
	}

	/// <summary>
	/// Replaces the shared instance. Intended for tests and custom implementations.
	/// </summary>
	public static void SetDefault(IPermissionFlow implementation) =>
		_current = implementation ?? throw new ArgumentNullException(nameof(implementation));

	internal static PermissionFlowImplementation Create(
		PermissionFlowOptions options,
		IPermissionProbe probe,
		IDeviceServiceProbe services,
		IPermissionHistoryStore store,
		IClock clock,
		IPermissionRationalePresenter rationale,
		IPermissionSettingsPresenter settings,
		IPermissionSettingsNavigator navigator) =>
		new(options, probe, services, store, clock, rationale, settings, navigator);

	static IPermissionProbe CreateProbe()
	{
#if ANDROID
		return new AndroidPermissionProbe();
#elif IOS
		return new IosPermissionProbe();
#else
		return new NetPermissionProbe();
#endif
	}

	static IDeviceServiceProbe CreateServiceProbe()
	{
#if ANDROID
		return new AndroidDeviceServiceProbe();
#elif IOS
		return new IosDeviceServiceProbe();
#else
		return new NetDeviceServiceProbe();
#endif
	}
}
