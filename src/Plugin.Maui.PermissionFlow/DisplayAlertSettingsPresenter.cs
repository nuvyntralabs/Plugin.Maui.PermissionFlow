namespace Plugin.Maui.PermissionFlow;

/// <summary>
/// Default Settings offer using the current page's display-alert API.
/// </summary>
public sealed class DisplayAlertSettingsPresenter : IPermissionSettingsPresenter
{
	public async Task<RationaleDecision> PresentAsync(SettingsOfferRequest request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);
		cancellationToken.ThrowIfCancellationRequested();

		var page = PageResolver.GetCurrentPage();
		if (page is null)
			return RationaleDecision.Decline;

		var open = await page.DisplayAlertAsync(request.Title, request.Message, request.OpenSettingsText, request.NotNowText).ConfigureAwait(true);
		return open ? RationaleDecision.Continue : RationaleDecision.Decline;
	}
}
