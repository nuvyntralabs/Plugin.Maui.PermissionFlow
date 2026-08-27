using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;

namespace Plugin.Maui.PermissionFlow;

sealed class PermissionFlowInitializer : IMauiInitializeService
{
	public void Initialize(IServiceProvider services)
	{
		var options = services.GetService<PermissionFlowOptions>() ?? new PermissionFlowOptions();
		var flow = services.GetService<IPermissionFlow>() ?? PermissionFlow.Current;

		if (options.EnableLogging)
		{
			var logger = options.Logger
				?? MauiAppBuilderExtensions.CreateLoggerAdapter(services)
				?? new DebugPermissionFlowLogger();
			flow.EnableLogging(true, logger);
		}
	}
}
