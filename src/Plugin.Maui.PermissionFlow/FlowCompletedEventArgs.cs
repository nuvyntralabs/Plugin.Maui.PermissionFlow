namespace Plugin.Maui.PermissionFlow;

public sealed class FlowCompletedEventArgs : EventArgs
{
	public FlowCompletedEventArgs(PermissionFlowResult result)
	{
		Result = result;
	}

	public PermissionFlowResult Result { get; }
}
