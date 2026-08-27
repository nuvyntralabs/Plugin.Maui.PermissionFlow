namespace Plugin.Maui.PermissionFlow;

static class PageResolver
{
	public static Page? GetCurrentPage()
	{
		var application = Application.Current;
		if (application is null)
			return null;

		var page = application.Windows.FirstOrDefault()?.Page;
		return page is null ? null : GetVisiblePage(page);
	}

	static Page GetVisiblePage(Page page) => page switch
	{
		FlyoutPage { Detail: { } detail } => GetVisiblePage(detail),
		TabbedPage { CurrentPage: { } current } => GetVisiblePage(current),
		NavigationPage { CurrentPage: { } current } => GetVisiblePage(current),
		Shell { CurrentPage: { } current } => current,
		_ => page
	};
}
