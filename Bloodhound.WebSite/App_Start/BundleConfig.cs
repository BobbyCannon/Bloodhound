#region References

using System.Web.Optimization;

#endregion

namespace Bloodhound.WebSite
{
	public static class BundleConfig
	{
		#region Methods

		public static void RegisterBundles(BundleCollection bundles)
		{
			bundles.Add(new ScriptBundle("~/scripts/js")
				.Include("~/Scripts/jquery-{version}.js")
				.Include("~/Scripts/angular.js")
				.Include("~/Scripts/chart.js")
				.Include("~/Scripts/toastr.js")
				.Include("~/Scripts/pickadate/picker.js")
				.Include("~/Scripts/pickadate/picker.date.js")
				.Include("~/Scripts/pickadate/picker.time.js")
				.Include("~/Scripts/site.js"));

			bundles.Add(new StyleBundle("~/content/css")
				.Include("~/Scripts/pickadate/themes/classic.css")
				.Include("~/Scripts/pickadate/themes/classic.date.css")
				.Include("~/Scripts/pickadate/themes/classic.time.css")
				.Include("~/Content/font-awesome.css")
				.Include("~/Content/toastr.css")
				.Include("~/Content/site.css"));
		}

		#endregion
	}
}