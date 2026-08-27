namespace Plugin.Maui.PermissionFlow;

/// <summary>
/// A named feature gate: required and optional permissions plus orchestration overrides.
/// </summary>
public sealed class PermissionFlowDefinition
{
	public PermissionFlowDefinition(
		string id,
		string? title,
		string? description,
		IReadOnlyList<PermissionStep> permissions,
		IReadOnlyList<DeviceService> requiredServices,
		TimeSpan? denialCooldown = null,
		RationalePolicy? rationalePolicy = null,
		bool? offerSettingsWhenPermanentlyDenied = null,
		bool? acceptLimited = null)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(id);

		Id = id;
		Title = title;
		Description = description;
		Permissions = permissions ?? throw new ArgumentNullException(nameof(permissions));
		RequiredServices = requiredServices ?? throw new ArgumentNullException(nameof(requiredServices));
		DenialCooldown = denialCooldown;
		RationalePolicy = rationalePolicy;
		OfferSettingsWhenPermanentlyDenied = offerSettingsWhenPermanentlyDenied;
		AcceptLimited = acceptLimited;
	}

	public string Id { get; }

	public string? Title { get; }

	public string? Description { get; }

	public IReadOnlyList<PermissionStep> Permissions { get; }

	public IReadOnlyList<DeviceService> RequiredServices { get; }

	public TimeSpan? DenialCooldown { get; }

	public RationalePolicy? RationalePolicy { get; }

	public bool? OfferSettingsWhenPermanentlyDenied { get; }

	public bool? AcceptLimited { get; }
}
