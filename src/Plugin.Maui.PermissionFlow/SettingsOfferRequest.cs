namespace Plugin.Maui.PermissionFlow;

/// <summary>
/// Copy and context for offering to open the system app settings page.
/// </summary>
public sealed class SettingsOfferRequest
{
	public SettingsOfferRequest(
		string flowId,
		string? flowTitle,
		AppPermission permission,
		string title,
		string message,
		string openSettingsText,
		string notNowText)
	{
		FlowId = flowId;
		FlowTitle = flowTitle;
		Permission = permission;
		Title = title;
		Message = message;
		OpenSettingsText = openSettingsText;
		NotNowText = notNowText;
	}

	public string FlowId { get; }

	public string? FlowTitle { get; }

	public AppPermission Permission { get; }

	public string Title { get; }

	public string Message { get; }

	public string OpenSettingsText { get; }

	public string NotNowText { get; }
}
