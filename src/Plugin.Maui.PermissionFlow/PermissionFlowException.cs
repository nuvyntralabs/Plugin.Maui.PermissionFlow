namespace Plugin.Maui.PermissionFlow;

/// <summary>
/// Thrown when a permission flow cannot be resolved or platform state cannot be read.
/// </summary>
public sealed class PermissionFlowException : Exception
{
	public PermissionFlowException(PermissionFlowError error, string message, Exception? innerException = null)
		: base(message, innerException)
	{
		Error = error;
	}

	public PermissionFlowError Error { get; }
}
