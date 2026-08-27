using System.Diagnostics;

namespace Plugin.Maui.PermissionFlow;

/// <summary>
/// Writes plugin diagnostics to <see cref="Debug.WriteLine(string?)"/>.
/// </summary>
public sealed class DebugPermissionFlowLogger : IPermissionFlowLogger
{
	public void Log(PermissionFlowLogLevel level, string message, Exception? exception = null)
	{
		var line = exception is null
			? $"[PermissionFlow] {level}: {message}"
			: $"[PermissionFlow] {level}: {message}{Environment.NewLine}{exception}";

		Debug.WriteLine(line);
	}
}
