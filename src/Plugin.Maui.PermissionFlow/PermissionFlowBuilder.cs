namespace Plugin.Maui.PermissionFlow;

/// <summary>
/// Fluent builder for a <see cref="PermissionFlowDefinition"/>.
/// </summary>
public sealed class PermissionFlowBuilder
{
	readonly List<PermissionStep> _permissions = [];
	readonly List<DeviceService> _services = [];

	public PermissionFlowBuilder(string id)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(id);
		Id = id;
	}

	public string Id { get; }

	public string? Title { get; set; }

	public string? Description { get; set; }

	public TimeSpan? DenialCooldown { get; set; }

	public RationalePolicy? RationalePolicy { get; set; }

	public bool? OfferSettingsWhenPermanentlyDenied { get; set; }

	public bool? AcceptLimited { get; set; }

	public PermissionFlowBuilder Require(AppPermission permission, string? rationale = null, string? title = null)
	{
		_permissions.Add(new PermissionStep(permission, PermissionRequirement.Required, title, rationale));
		return this;
	}

	public PermissionFlowBuilder Optional(AppPermission permission, string? rationale = null, string? title = null)
	{
		_permissions.Add(new PermissionStep(permission, PermissionRequirement.Optional, title, rationale));
		return this;
	}

	public PermissionFlowBuilder RequireService(DeviceService service)
	{
		if (!_services.Contains(service))
			_services.Add(service);

		return this;
	}

	public PermissionFlowDefinition Build() =>
		new(
			Id,
			Title,
			Description,
			_permissions.ToArray(),
			_services.ToArray(),
			DenialCooldown,
			RationalePolicy,
			OfferSettingsWhenPermanentlyDenied,
			AcceptLimited);
}
