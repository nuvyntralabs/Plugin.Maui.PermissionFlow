namespace Plugin.Maui.PermissionFlow;

public sealed class FlowStartedEventArgs : EventArgs
{
	public FlowStartedEventArgs(string flowId, string? flowTitle)
	{
		FlowId = flowId;
		FlowTitle = flowTitle;
	}

	public string FlowId { get; }

	public string? FlowTitle { get; }
}
