using Microsoft.Maui.ApplicationModel;

namespace Plugin.Maui.PermissionFlow;

/// <summary>
/// Opens application settings through <see cref="AppInfo.ShowSettingsUI"/>.
/// </summary>
public sealed class MauiSettingsNavigator : IPermissionSettingsNavigator
{
	public Task OpenSettingsAsync()
	{
		AppInfo.Current.ShowSettingsUI();
		return Task.CompletedTask;
	}
}
