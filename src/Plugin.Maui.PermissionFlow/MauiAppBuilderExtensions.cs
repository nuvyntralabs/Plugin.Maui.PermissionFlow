using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;

namespace Plugin.Maui.PermissionFlow;

/// <summary>
/// Registers the PermissionFlow plugin with the MAUI dependency injection container.
/// </summary>
public static class MauiAppBuilderExtensions
{
	/// <summary>
	/// Adds <see cref="IPermissionFlow"/> as a singleton and registers named flows.
	/// </summary>
	/// <example>
	/// <code>
	/// builder.UsePermissionFlow(options =>
	/// {
	///     options.EnableLogging = true;
	///     options.AddFlow("scan", flow =>
	///     {
	///         flow.Title = "Scan a code";
	///         flow.Require(AppPermission.Camera, "The camera is used only to scan QR codes.");
	///     });
	/// });
	/// </code>
	/// </example>
	public static MauiAppBuilder UsePermissionFlow(this MauiAppBuilder builder, Action<PermissionFlowOptions>? configure = null)
	{
		ArgumentNullException.ThrowIfNull(builder);

		var options = new PermissionFlowOptions();
		configure?.Invoke(options);

		builder.Services.AddSingleton(options);
		builder.Services.AddSingleton<IPermissionFlow>(services =>
		{
			options.Logger ??= CreateLoggerAdapter(services);
			var flow = PermissionFlow.Create(options);
			PermissionFlow.SetDefault(flow);
			return flow;
		});
		builder.Services.AddTransient<IMauiInitializeService, PermissionFlowInitializer>();

		return builder;
	}

	internal static IPermissionFlowLogger? CreateLoggerAdapter(IServiceProvider serviceProvider)
	{
		var factory = serviceProvider.GetService<ILoggerFactory>();
		return factory is null ? null : new MicrosoftLoggerAdapter(factory.CreateLogger("Plugin.Maui.PermissionFlow"));
	}
}
