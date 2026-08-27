namespace Plugin.Maui.PermissionFlow;

/// <summary>
/// Outcome of a device-service precondition (location services, Bluetooth).
/// </summary>
public sealed class ServiceDecision
{
	public ServiceDecision(DeviceService service, bool isEnabled, PermissionOutcome outcome, string? message = null)
	{
		Service = service;
		IsEnabled = isEnabled;
		Outcome = outcome;
		Message = message;
	}

	public DeviceService Service { get; }

	public bool IsEnabled { get; }

	public PermissionOutcome Outcome { get; }

	public string? Message { get; }
}
