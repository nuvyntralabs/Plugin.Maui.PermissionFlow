namespace Plugin.Maui.PermissionFlow;

/// <summary>
/// Opens the operating-system application settings page.
/// </summary>
public interface IPermissionSettingsNavigator
{
	Task OpenSettingsAsync();
}
