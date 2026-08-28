# Plugin.Maui.PermissionFlow

[NuGet](https://www.nuget.org/packages/Plugin.Maui.PermissionFlow) · [GitHub](https://github.com/NiladriPadhy/Plugin.Maui.PermissionFlow)

Intelligent permission orchestration for .NET MAUI on **Android** and **iOS**.

Instead of scattering `Permissions.RequestAsync<T>()` calls across the app, you declare **named flows** for each feature. The orchestrator then:

- Shows an in-app rationale before the OS dialog
- Requests permissions one at a time (and expands `LocationAlways` to when-in-use first)
- Honors a denial cooldown so users are not spammed
- Detects Android "Don't ask again" and iOS permanent denials
- Offers to open Settings when the OS will not prompt again
- Distinguishes required vs optional steps and required device services

## Install

Package: [https://www.nuget.org/packages/Plugin.Maui.PermissionFlow](https://www.nuget.org/packages/Plugin.Maui.PermissionFlow)

```bash
dotnet add package Plugin.Maui.PermissionFlow
```

Or reference the project:

```xml
<ProjectReference Include="..\src\Plugin.Maui.PermissionFlow\Plugin.Maui.PermissionFlow.csproj" />
```

Target frameworks:

- `net10.0` (unit tests / shared)
- `net10.0-android`
- `net10.0-ios`

## Register the plugin

```csharp
builder
    .UseMauiApp<App>()
    .UsePermissionFlow(options =>
    {
        options.EnableLogging = true;
        options.DefaultDenialCooldown = TimeSpan.FromHours(24);
        options.DefaultRationalePolicy = RationalePolicy.FirstRequest;
        options.OfferSettingsWhenPermanentlyDenied = true;

        options.AddFlow("scan", flow =>
        {
            flow.Title = "Scan a code";
            flow.Require(AppPermission.Camera, "We use the camera only to scan QR codes.");
            flow.Optional(AppPermission.Photos, "Optionally pick a photo of a code.");
        });

        options.AddFlow("share-location", flow =>
        {
            flow.Require(AppPermission.LocationWhenInUse);
            flow.RequireService(DeviceService.Location);
        });
    });
```

Resolve `IPermissionFlow` from dependency injection, or use `PermissionFlow.Current`.

## Ensure a feature

```csharp
var result = await PermissionFlow.Current.EnsureAsync("scan");

if (result.IsSatisfied)
{
    // Open the scanner.
}
else if (result.ShouldOpenSettings)
{
    await PermissionFlow.Current.OpenSettingsAsync();
}
```

`EnsureAsync` is serialized globally so two features never present overlapping OS dialogs.

Per-call overrides:

```csharp
await PermissionFlow.Current.EnsureAsync("scan", new EnsureOptions
{
    Force = true,              // ignore cooldown
    SkipRationale = true,
    SkipSettingsOffer = true
});
```

Ad-hoc (no named flow):

```csharp
await PermissionFlow.Current.EnsureAsync([AppPermission.Camera, AppPermission.Microphone]);
```

## What you get

| Capability | Behavior |
| --- | --- |
| Named flows | Required + optional permissions, optional device services |
| Rationale | `Never`, `FirstRequest`, `AfterDenial`, `Always` |
| Cooldown | Default 24 hours after a denial or "Not now" |
| Permanent denial | Android don't-ask-again and iOS post-denial |
| Settings handoff | In-app offer, then `AppInfo.ShowSettingsUI()` |
| Limited grants | iOS Limited / Android partial can satisfy a step (`AcceptLimited`) |
| LocationAlways | Automatically requests when-in-use first |
| Snapshot | `CheckAsync` / `GetSnapshotAsync` without prompting |

Events: `FlowStarted`, `FlowCompleted`, `PermissionChanged`.

## Custom UI

Replace the default `DisplayAlert` presenters:

```csharp
options.RationalePresenter = new DelegateRationalePresenter(async request =>
{
    var page = Shell.Current;
    var proceed = await page.DisplayAlert(request.Title, request.Message, request.ContinueText, request.NotNowText);
    return proceed ? RationaleDecision.Continue : RationaleDecision.Decline;
});
```

## Host app setup

The plugin requests permissions. The host app must declare them.

### Android

Add the matching `<uses-permission>` entries to `AndroidManifest.xml`. Examples:

```xml
<uses-permission android:name="android.permission.CAMERA" />
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
<uses-permission android:name="android.permission.POST_NOTIFICATIONS" />
```

Notifications are requested only on Android 13+.

### iOS

Add usage descriptions to `Info.plist`:

```xml
<key>NSCameraUsageDescription</key>
<string>The camera is used to scan codes.</string>
<key>NSLocationWhenInUseUsageDescription</key>
<string>Your location is used to show nearby places.</string>
```

History is stored in `Preferences` (User Defaults). Add the User Defaults reason to the iOS privacy manifest if you have not already:

```xml
<key>NSPrivacyAccessedAPIType</key>
<string>NSPrivacyAccessedAPICategoryUserDefaults</string>
```

## Platform notes

| | Android | iOS | `net10.0` |
| --- | --- | --- | --- |
| Runtime prompts | Yes | Yes | Test fakes |
| Don't ask again | Yes | Denial is permanent | Configurable |
| Location services check | Yes | Yes | Always on |
| Unavailable permissions | Treated as not applicable (flow can still succeed) | Same | Same |

Supported `AppPermission` values include Camera, Microphone, LocationWhenInUse, LocationAlways, Photos, PhotosAddOnly, Media, Storage, Contacts, Calendar, Reminders, Bluetooth, Notifications, Sensors, Speech, Phone, Sms, and NearbyWifi.

## Sample

`samples/PermissionFlow.Sample` registers scan, location, notification, and photo flows and shows snapshots, cooldown, force-retry, and Settings.

```bash
dotnet build src/Plugin.Maui.PermissionFlow/Plugin.Maui.PermissionFlow.csproj
dotnet pack src/Plugin.Maui.PermissionFlow/Plugin.Maui.PermissionFlow.csproj -c Release
dotnet test tests/Plugin.Maui.PermissionFlow.Tests/Plugin.Maui.PermissionFlow.Tests.csproj
dotnet build samples/PermissionFlow.Sample/PermissionFlow.Sample.csproj -f net10.0-android
```

## Pack

```bash
dotnet pack src/Plugin.Maui.PermissionFlow/Plugin.Maui.PermissionFlow.csproj -c Release
```

Packages are written to `artifacts/`.

## Support

> If this plugin saved you a weekend of native plumbing, consider buying me a coffee.
> Your support keeps it maintained, documented, and free.

[![Buy Me A Coffee](https://img.shields.io/badge/Buy%20Me%20a%20Coffee-ffdd00?style=for-the-badge&logo=buy-me-a-coffee&logoColor=black)](https://buymeacoffee.com/npadhy)

This library stays open source. A coffee helps cover time for bug fixes, new features, and docs.
