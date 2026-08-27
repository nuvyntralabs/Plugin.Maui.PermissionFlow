namespace Plugin.Maui.PermissionFlow;

sealed class MemoryHistoryStore : IPermissionHistoryStore
{
	readonly Dictionary<AppPermission, PermissionHistoryRecord> _records = [];

	public PermissionHistoryRecord Get(AppPermission permission)
	{
		if (_records.TryGetValue(permission, out var record))
			return Clone(record);

		return new PermissionHistoryRecord();
	}

	public void MarkRequested(AppPermission permission, DateTimeOffset utcNow)
	{
		var record = GetOrCreate(permission);
		record.WasRequested = true;
		record.RequestCount++;
		record.LastRequestedAt = utcNow;
	}

	public void MarkDenied(AppPermission permission, DateTimeOffset utcNow)
	{
		var record = GetOrCreate(permission);
		record.LastDeniedAt = utcNow;
	}

	public void MarkPermanentlyDenied(AppPermission permission)
	{
		GetOrCreate(permission).IsPermanentlyDenied = true;
	}

	public void Clear(AppPermission? permission = null)
	{
		if (permission is { } one)
			_records.Remove(one);
		else
			_records.Clear();
	}

	PermissionHistoryRecord GetOrCreate(AppPermission permission)
	{
		if (_records.TryGetValue(permission, out var record))
			return record;

		record = new PermissionHistoryRecord();
		_records[permission] = record;
		return record;
	}

	static PermissionHistoryRecord Clone(PermissionHistoryRecord record) =>
		new()
		{
			WasRequested = record.WasRequested,
			IsPermanentlyDenied = record.IsPermanentlyDenied,
			RequestCount = record.RequestCount,
			LastRequestedAt = record.LastRequestedAt,
			LastDeniedAt = record.LastDeniedAt
		};
}
