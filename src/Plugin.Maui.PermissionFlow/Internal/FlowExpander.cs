namespace Plugin.Maui.PermissionFlow;

static class FlowExpander
{
	public static IReadOnlyList<PermissionStep> Expand(IEnumerable<PermissionStep> steps)
	{
		var result = new List<PermissionStep>();
		var indexByPermission = new Dictionary<AppPermission, int>();

		foreach (var step in steps)
		{
			foreach (var permission in PermissionCatalog.Expand(step.Permission))
			{
				var implied = permission != step.Permission;
				if (indexByPermission.TryGetValue(permission, out var existingIndex))
				{
					var existing = result[existingIndex];
					if (existing.Requirement == PermissionRequirement.Optional && step.Requirement == PermissionRequirement.Required)
						result[existingIndex] = existing.With(requirement: PermissionRequirement.Required);

					continue;
				}

				indexByPermission[permission] = result.Count;
				result.Add(step.With(permission: permission, isImplied: implied));
			}
		}

		return result;
	}
}
