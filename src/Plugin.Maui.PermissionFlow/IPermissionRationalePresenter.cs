namespace Plugin.Maui.PermissionFlow;

/// <summary>
/// Shows an in-app explanation before the operating-system permission dialog.
/// </summary>
public interface IPermissionRationalePresenter
{
	Task<RationaleDecision> PresentAsync(RationaleRequest request, CancellationToken cancellationToken = default);
}
