namespace Plugin.Maui.PermissionFlow;

static class StatusRefiner
{
	public static PermissionStatusKind Refine(
		PermissionStatusKind raw,
		bool shouldShowRationale,
		bool previouslyRequested,
		PermissionFlowPlatformInfo platform)
	{
		if (raw is PermissionStatusKind.Granted
			or PermissionStatusKind.Limited
			or PermissionStatusKind.Restricted
			or PermissionStatusKind.Unavailable
			or PermissionStatusKind.PermanentlyDenied)
		{
			return raw;
		}

		if (raw is PermissionStatusKind.Unknown or PermissionStatusKind.NotDetermined)
			return PermissionStatusKind.NotDetermined;

		if (platform.DenialIsPermanent)
			return previouslyRequested || raw == PermissionStatusKind.Denied
				? PermissionStatusKind.PermanentlyDenied
				: PermissionStatusKind.NotDetermined;

		if (platform.UsesDontAskAgain)
		{
			if (!previouslyRequested)
				return PermissionStatusKind.NotDetermined;

			return shouldShowRationale
				? PermissionStatusKind.Denied
				: PermissionStatusKind.PermanentlyDenied;
		}

		return raw;
	}

	public static bool ShouldPresentRationale(
		RationalePolicy policy,
		PermissionHistoryRecord history,
		bool shouldShowRationale)
	{
		return policy switch
		{
			RationalePolicy.Never => false,
			RationalePolicy.Always => true,
			RationalePolicy.FirstRequest => !history.WasRequested,
			RationalePolicy.AfterDenial => history.LastDeniedAt is not null || shouldShowRationale,
			_ => false
		};
	}
}
