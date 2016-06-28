#region References

using System.Web.Mvc;
using Bloodhound.WebSite.Attributes;

#endregion

namespace Bloodhound.WebSite
{
	public static class FilterConfig
	{
		#region Methods

		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
			filters.Add(new ActionTimerAttribute());
		}

		#endregion
	}
}