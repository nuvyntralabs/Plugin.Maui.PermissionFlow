using Microsoft.Maui.Storage;

namespace Plugin.Maui.PermissionFlow;

sealed class PreferencesHistoryStore : IPermissionHistoryStore
{
	public const string SharedName = "plugin.maui.permissionflow";

	public PermissionHistoryRecord Get(AppPermission permission)
	{
		try
		{
			return new PermissionHistoryRecord
			{
				WasRequested = GetBool(Key(permission, "requested")),
				IsPermanentlyDenied = GetBool(Key(permission, "permanent")),
				RequestCount = GetInt(Key(permission, "count")),
				LastRequestedAt = GetTime(Key(permission, "requestedAt")),
				LastDeniedAt = GetTime(Key(permission, "deniedAt"))
			};
		}
		catch (Exception ex)
		{
			throw new PermissionFlowException(PermissionFlowError.StoreFailure, $"Failed to read history for {permission}.", ex);
		}
	}

	public void MarkRequested(AppPermission permission, DateTimeOffset utcNow)
	{
		try
		{
			var count = GetInt(Key(permission, "count")) + 1;
			Preferences.Set(Key(permission, "requested"), true, SharedName);
			Preferences.Set(Key(permission, "count"), count, SharedName);
			Preferences.Set(Key(permission, "requestedAt"), utcNow.ToString("O"), SharedName);
		}
		catch (Exception ex)
		{
			throw new PermissionFlowException(PermissionFlowError.StoreFailure, $"Failed to record a request for {permission}.", ex);
		}
	}

	public void MarkDenied(AppPermission permission, DateTimeOffset utcNow)
	{
		try
		{
			Preferences.Set(Key(permission, "deniedAt"), utcNow.ToString("O"), SharedName);
		}
		catch (Exception ex)
		{
			throw new PermissionFlowException(PermissionFlowError.StoreFailure, $"Failed to record a denial for {permission}.", ex);
		}
	}

	public void MarkPermanentlyDenied(AppPermission permission)
	{
		try
		{
			Preferences.Set(Key(permission, "permanent"), true, SharedName);
		}
		catch (Exception ex)
		{
			throw new PermissionFlowException(PermissionFlowError.StoreFailure, $"Failed to record a permanent denial for {permission}.", ex);
		}
	}

	public void Clear(AppPermission? permission = null)
	{
		try
		{
			if (permission is { } one)
			{
				foreach (var suffix in new[] { "requested", "permanent", "count", "requestedAt", "deniedAt" })
					Preferences.Remove(Key(one, suffix), SharedName);
			}
			else
			{
				Preferences.Clear(SharedName);
			}
		}
		catch (Exception ex)
		{
			throw new PermissionFlowException(PermissionFlowError.StoreFailure, "Failed to clear permission history.", ex);
		}
	}

	static string Key(AppPermission permission, string suffix) => $"{permission}.{suffix}";

	static bool GetBool(string key) =>
		Preferences.ContainsKey(key, SharedName) && Preferences.Get(key, false, SharedName);

	static int GetInt(string key) =>
		Preferences.ContainsKey(key, SharedName) ? Preferences.Get(key, 0, SharedName) : 0;

	static DateTimeOffset? GetTime(string key)
	{
		if (!Preferences.ContainsKey(key, SharedName))
			return null;

		var raw = Preferences.Get(key, default(string), SharedName);
		return DateTimeOffset.TryParse(raw, out var value) ? value : null;
	}
}
