#region References

using System.Data.Entity;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Bloodhound.Data;
using Bloodhound.Data.Migrations;

#endregion

namespace Bloodhound.WebSite
{
	public class MvcApplication : HttpApplication
	{
		#region Methods

		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();
			GlobalConfiguration.Configure(WebApiConfig.Register);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);

			GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings = Bloodhound.Extensions.DefaultSerializerSettings;
			GlobalConfiguration.Configuration.Formatters.Remove(GlobalConfiguration.Configuration.Formatters.XmlFormatter);
			GlobalConfiguration.Configuration.EnsureInitialized();

			Database.SetInitializer(new MigrateDatabaseToLatestVersion<DataContext, Configuration>());
		}

		#endregion
	}
}