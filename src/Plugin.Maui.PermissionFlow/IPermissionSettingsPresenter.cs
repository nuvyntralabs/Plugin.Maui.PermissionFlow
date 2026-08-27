namespace Plugin.Maui.PermissionFlow;

/// <summary>
/// Offers to open the system Settings page after a permanent denial.
/// </summary>
public interface IPermissionSettingsPresenter
{
	Task<RationaleDecision> PresentAsync(SettingsOfferRequest request, CancellationToken cancellationToken = default);
}
