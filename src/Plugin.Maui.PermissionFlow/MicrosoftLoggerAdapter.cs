using Microsoft.Extensions.Logging;

namespace Plugin.Maui.PermissionFlow;

sealed class MicrosoftLoggerAdapter(ILogger logger) : IPermissionFlowLogger
{
	public void Log(PermissionFlowLogLevel level, string message, Exception? exception = null)
	{
		logger.Log(ToLogLevel(level), exception, "{Message}", message);
	}

	static LogLevel ToLogLevel(PermissionFlowLogLevel level) => level switch
	{
		PermissionFlowLogLevel.Trace => LogLevel.Trace,
		PermissionFlowLogLevel.Debug => LogLevel.Debug,
		PermissionFlowLogLevel.Information => LogLevel.Information,
		PermissionFlowLogLevel.Warning => LogLevel.Warning,
		PermissionFlowLogLevel.Error => LogLevel.Error,
		_ => LogLevel.Information
	};
}
