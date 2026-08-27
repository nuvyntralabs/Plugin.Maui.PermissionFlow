namespace Plugin.Maui.PermissionFlow;

/// <summary>
/// Receives diagnostic messages from the PermissionFlow plugin.
/// </summary>
public interface IPermissionFlowLogger
{
	void Log(PermissionFlowLogLevel level, string message, Exception? exception = null);
}
