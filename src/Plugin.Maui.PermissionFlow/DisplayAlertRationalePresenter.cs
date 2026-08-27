namespace Plugin.Maui.PermissionFlow;

/// <summary>
/// Default rationale UI using the current page's display-alert API.
/// </summary>
public sealed class DisplayAlertRationalePresenter : IPermissionRationalePresenter
{
	public async Task<RationaleDecision> PresentAsync(RationaleRequest request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);
		cancellationToken.ThrowIfCancellationRequested();

		var page = PageResolver.GetCurrentPage();
		if (page is null)
			return RationaleDecision.Continue;

		var proceed = await page.DisplayAlertAsync(request.Title, request.Message, request.ContinueText, request.NotNowText).ConfigureAwait(true);
		return proceed ? RationaleDecision.Continue : RationaleDecision.Decline;
	}
}
