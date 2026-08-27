namespace Plugin.Maui.PermissionFlow;

/// <summary>
/// Settings-offer presenter backed by an app-supplied callback.
/// </summary>
public sealed class DelegateSettingsPresenter : IPermissionSettingsPresenter
{
	readonly Func<SettingsOfferRequest, CancellationToken, Task<RationaleDecision>> _handler;

	public DelegateSettingsPresenter(Func<SettingsOfferRequest, CancellationToken, Task<RationaleDecision>> handler)
	{
		_handler = handler ?? throw new ArgumentNullException(nameof(handler));
	}

	public DelegateSettingsPresenter(Func<SettingsOfferRequest, Task<RationaleDecision>> handler)
	{
		ArgumentNullException.ThrowIfNull(handler);
		_handler = (request, _) => handler(request);
	}

	public Task<RationaleDecision> PresentAsync(SettingsOfferRequest request, CancellationToken cancellationToken = default) =>
		_handler(request, cancellationToken);
}
