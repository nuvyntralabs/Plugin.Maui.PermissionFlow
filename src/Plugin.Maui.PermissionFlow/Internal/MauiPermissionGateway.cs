using Microsoft.Maui.ApplicationModel;

namespace Plugin.Maui.PermissionFlow;

static class MauiPermissionGateway
{
	public static async Task<PermissionStatusKind> CheckAsync(AppPermission permission, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		try
		{
			var status = await InvokeAsync(() => CheckNativeAsync(permission)).ConfigureAwait(false);
			return Map(status);
		}
		catch (Exception ex) when (ex is not OperationCanceledException and not PermissionFlowException)
		{
			throw new PermissionFlowException(PermissionFlowError.PlatformFailure, $"Failed to check {permission}.", ex);
		}
	}

	public static async Task<PermissionStatusKind> RequestAsync(AppPermission permission, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		try
		{
			var status = await InvokeAsync(() => RequestNativeAsync(permission)).ConfigureAwait(false);
			return Map(status);
		}
		catch (Exception ex) when (ex is not OperationCanceledException and not PermissionFlowException)
		{
			throw new PermissionFlowException(PermissionFlowError.PlatformFailure, $"Failed to request {permission}.", ex);
		}
	}

	public static bool ShouldShowRationale(AppPermission permission)
	{
		try
		{
			return permission switch
			{
				AppPermission.Camera => Permissions.ShouldShowRationale<Permissions.Camera>(),
				AppPermission.Microphone => Permissions.ShouldShowRationale<Permissions.Microphone>(),
				AppPermission.LocationWhenInUse => Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>(),
				AppPermission.LocationAlways => Permissions.ShouldShowRationale<Permissions.LocationAlways>(),
				AppPermission.Photos => Permissions.ShouldShowRationale<Permissions.Photos>(),
				AppPermission.PhotosAddOnly => Permissions.ShouldShowRationale<Permissions.PhotosAddOnly>(),
				AppPermission.Media => Permissions.ShouldShowRationale<Permissions.Media>(),
				AppPermission.StorageRead => Permissions.ShouldShowRationale<Permissions.StorageRead>(),
				AppPermission.StorageWrite => Permissions.ShouldShowRationale<Permissions.StorageWrite>(),
				AppPermission.ContactsRead => Permissions.ShouldShowRationale<Permissions.ContactsRead>(),
				AppPermission.ContactsWrite => Permissions.ShouldShowRationale<Permissions.ContactsWrite>(),
				AppPermission.CalendarRead => Permissions.ShouldShowRationale<Permissions.CalendarRead>(),
				AppPermission.CalendarWrite => Permissions.ShouldShowRationale<Permissions.CalendarWrite>(),
				AppPermission.Reminders => Permissions.ShouldShowRationale<Permissions.Reminders>(),
				AppPermission.Bluetooth => Permissions.ShouldShowRationale<Permissions.Bluetooth>(),
				AppPermission.Notifications => Permissions.ShouldShowRationale<Permissions.PostNotifications>(),
				AppPermission.Sensors => Permissions.ShouldShowRationale<Permissions.Sensors>(),
				AppPermission.Speech => Permissions.ShouldShowRationale<Permissions.Speech>(),
				AppPermission.Phone => Permissions.ShouldShowRationale<Permissions.Phone>(),
				AppPermission.Sms => Permissions.ShouldShowRationale<Permissions.Sms>(),
				AppPermission.NearbyWifi => Permissions.ShouldShowRationale<Permissions.NearbyWifiDevices>(),
				_ => false
			};
		}
		catch
		{
			return false;
		}
	}

	public static PermissionStatusKind Map(PermissionStatus status) => status switch
	{
		PermissionStatus.Granted => PermissionStatusKind.Granted,
		PermissionStatus.Denied => PermissionStatusKind.Denied,
		PermissionStatus.Disabled => PermissionStatusKind.Denied,
		PermissionStatus.Restricted => PermissionStatusKind.Restricted,
		PermissionStatus.Limited => PermissionStatusKind.Limited,
		PermissionStatus.Unknown => PermissionStatusKind.NotDetermined,
		_ => PermissionStatusKind.Unknown
	};

	static Task<PermissionStatus> CheckNativeAsync(AppPermission permission) => permission switch
	{
		AppPermission.Camera => Permissions.CheckStatusAsync<Permissions.Camera>(),
		AppPermission.Microphone => Permissions.CheckStatusAsync<Permissions.Microphone>(),
		AppPermission.LocationWhenInUse => Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>(),
		AppPermission.LocationAlways => Permissions.CheckStatusAsync<Permissions.LocationAlways>(),
		AppPermission.Photos => Permissions.CheckStatusAsync<Permissions.Photos>(),
		AppPermission.PhotosAddOnly => Permissions.CheckStatusAsync<Permissions.PhotosAddOnly>(),
		AppPermission.Media => Permissions.CheckStatusAsync<Permissions.Media>(),
		AppPermission.StorageRead => Permissions.CheckStatusAsync<Permissions.StorageRead>(),
		AppPermission.StorageWrite => Permissions.CheckStatusAsync<Permissions.StorageWrite>(),
		AppPermission.ContactsRead => Permissions.CheckStatusAsync<Permissions.ContactsRead>(),
		AppPermission.ContactsWrite => Permissions.CheckStatusAsync<Permissions.ContactsWrite>(),
		AppPermission.CalendarRead => Permissions.CheckStatusAsync<Permissions.CalendarRead>(),
		AppPermission.CalendarWrite => Permissions.CheckStatusAsync<Permissions.CalendarWrite>(),
		AppPermission.Reminders => Permissions.CheckStatusAsync<Permissions.Reminders>(),
		AppPermission.Bluetooth => Permissions.CheckStatusAsync<Permissions.Bluetooth>(),
		AppPermission.Notifications => Permissions.CheckStatusAsync<Permissions.PostNotifications>(),
		AppPermission.Sensors => Permissions.CheckStatusAsync<Permissions.Sensors>(),
		AppPermission.Speech => Permissions.CheckStatusAsync<Permissions.Speech>(),
		AppPermission.Phone => Permissions.CheckStatusAsync<Permissions.Phone>(),
		AppPermission.Sms => Permissions.CheckStatusAsync<Permissions.Sms>(),
		AppPermission.NearbyWifi => Permissions.CheckStatusAsync<Permissions.NearbyWifiDevices>(),
		_ => Task.FromResult(PermissionStatus.Unknown)
	};

	static Task<PermissionStatus> RequestNativeAsync(AppPermission permission) => permission switch
	{
		AppPermission.Camera => Permissions.RequestAsync<Permissions.Camera>(),
		AppPermission.Microphone => Permissions.RequestAsync<Permissions.Microphone>(),
		AppPermission.LocationWhenInUse => Permissions.RequestAsync<Permissions.LocationWhenInUse>(),
		AppPermission.LocationAlways => Permissions.RequestAsync<Permissions.LocationAlways>(),
		AppPermission.Photos => Permissions.RequestAsync<Permissions.Photos>(),
		AppPermission.PhotosAddOnly => Permissions.RequestAsync<Permissions.PhotosAddOnly>(),
		AppPermission.Media => Permissions.RequestAsync<Permissions.Media>(),
		AppPermission.StorageRead => Permissions.RequestAsync<Permissions.StorageRead>(),
		AppPermission.StorageWrite => Permissions.RequestAsync<Permissions.StorageWrite>(),
		AppPermission.ContactsRead => Permissions.RequestAsync<Permissions.ContactsRead>(),
		AppPermission.ContactsWrite => Permissions.RequestAsync<Permissions.ContactsWrite>(),
		AppPermission.CalendarRead => Permissions.RequestAsync<Permissions.CalendarRead>(),
		AppPermission.CalendarWrite => Permissions.RequestAsync<Permissions.CalendarWrite>(),
		AppPermission.Reminders => Permissions.RequestAsync<Permissions.Reminders>(),
		AppPermission.Bluetooth => Permissions.RequestAsync<Permissions.Bluetooth>(),
		AppPermission.Notifications => Permissions.RequestAsync<Permissions.PostNotifications>(),
		AppPermission.Sensors => Permissions.RequestAsync<Permissions.Sensors>(),
		AppPermission.Speech => Permissions.RequestAsync<Permissions.Speech>(),
		AppPermission.Phone => Permissions.RequestAsync<Permissions.Phone>(),
		AppPermission.Sms => Permissions.RequestAsync<Permissions.Sms>(),
		AppPermission.NearbyWifi => Permissions.RequestAsync<Permissions.NearbyWifiDevices>(),
		_ => Task.FromResult(PermissionStatus.Unknown)
	};

	static Task<T> InvokeAsync<T>(Func<Task<T>> action)
	{
		if (MainThread.IsMainThread)
			return action();

		return MainThread.InvokeOnMainThreadAsync(action);
	}
}
