using Microsoft.Extensions.Logging;
using Plugin.Maui.PermissionFlow;

namespace PermissionFlow.Sample;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UsePermissionFlow(options =>
			{
				options.EnableLogging = true;
				options.DefaultDenialCooldown = TimeSpan.FromHours(1);
				options.DefaultRationalePolicy = RationalePolicy.FirstRequest;
				options.OfferSettingsWhenPermanentlyDenied = true;

				options.AddFlow("scan", flow =>
				{
					flow.Title = "Scan a code";
					flow.Description = "Camera access is required to scan QR codes.";
					flow.Require(AppPermission.Camera, "We use the camera only while a code is in the viewfinder.");
					flow.Optional(AppPermission.Photos, "Optionally pick a photo that already contains a code.");
				});

				options.AddFlow("location", flow =>
				{
					flow.Title = "Share location";
					flow.Require(AppPermission.LocationWhenInUse, "Location is used to show nearby places.");
					flow.RequireService(DeviceService.Location);
				});

				options.AddFlow("alerts", flow =>
				{
					flow.Title = "Stay notified";
					flow.Require(AppPermission.Notifications, "Notifications tell you when a flow needs attention.");
				});

				options.AddFlow("library", flow =>
				{
					flow.Title = "Photo library";
					flow.Require(AppPermission.Photos, "Photos are used to attach an image to a report.");
					flow.AcceptLimited = true;
				});
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
