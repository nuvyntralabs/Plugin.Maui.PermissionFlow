namespace Plugin.Maui.PermissionFlow;

/// <summary>
/// Rationale presenter backed by an app-supplied callback.
/// </summary>
public sealed class DelegateRationalePresenter : IPermissionRationalePresenter
{
	readonly Func<RationaleRequest, CancellationToken, Task<RationaleDecision>> _handler;

	public DelegateRationalePresenter(Func<RationaleRequest, CancellationToken, Task<RationaleDecision>> handler)
	{
		_handler = handler ?? throw new ArgumentNullException(nameof(handler));
	}

	public DelegateRationalePresenter(Func<RationaleRequest, Task<RationaleDecision>> handler)
	{
		ArgumentNullException.ThrowIfNull(handler);
		_handler = (request, _) => handler(request);
	}

	public Task<RationaleDecision> PresentAsync(RationaleRequest request, CancellationToken cancellationToken = default) =>
		_handler(request, cancellationToken);
}
