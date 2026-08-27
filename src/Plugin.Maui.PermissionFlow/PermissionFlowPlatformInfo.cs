namespace Plugin.Maui.PermissionFlow;

/// <summary>
/// Describes how this OS treats runtime permission denials.
/// </summary>
public sealed class PermissionFlowPlatformInfo
{
	public static PermissionFlowPlatformInfo Android { get; } = new("Android", usesDontAskAgain: true, denialIsPermanent: false);

	public static PermissionFlowPlatformInfo iOS { get; } = new("iOS", usesDontAskAgain: false, denialIsPermanent: true);

	public static PermissionFlowPlatformInfo Net { get; } = new("net", usesDontAskAgain: false, denialIsPermanent: false);

	public PermissionFlowPlatformInfo(string name, bool usesDontAskAgain, bool denialIsPermanent)
	{
		Name = name;
		UsesDontAskAgain = usesDontAskAgain;
		DenialIsPermanent = denialIsPermanent;
	}

	public string Name { get; }

	/// <summary>
	/// Android-style "Don't ask again" after a prior request.
	/// </summary>
	public bool UsesDontAskAgain { get; }

	/// <summary>
	/// iOS-style: a user denial cannot be prompted again from the app.
	/// </summary>
	public bool DenialIsPermanent { get; }
}
