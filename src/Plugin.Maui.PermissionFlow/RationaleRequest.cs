namespace Plugin.Maui.PermissionFlow;

/// <summary>
/// Copy and context for an in-app rationale shown before the OS permission dialog.
/// </summary>
public sealed class RationaleRequest
{
	public RationaleRequest(
		string flowId,
		string? flowTitle,
		AppPermission permission,
		PermissionRequirement requirement,
		string title,
		string message,
		string continueText,
		string notNowText)
	{
		FlowId = flowId;
		FlowTitle = flowTitle;
		Permission = permission;
		Requirement = requirement;
		Title = title;
		Message = message;
		ContinueText = continueText;
		NotNowText = notNowText;
	}

	public string FlowId { get; }

	public string? FlowTitle { get; }

	public AppPermission Permission { get; }

	public PermissionRequirement Requirement { get; }

	public string Title { get; }

	public string Message { get; }

	public string ContinueText { get; }

	public string NotNowText { get; }
}
