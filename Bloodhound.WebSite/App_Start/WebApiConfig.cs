#region References

using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.Routing.Conventions;
using Bloodhound.Models;
using Bloodhound.WebSite.Attributes;
using Microsoft.Data.Edm;

#endregion

namespace Bloodhound.WebSite
{
	public static class WebApiConfig
	{
		#region Methods

		public static void Register(HttpConfiguration config)
		{
			config.MapHttpAttributeRoutes();

			var builder = new ODataConventionModelBuilder();
			builder.EntitySet<Event>("Events");
			builder.EntitySet<EventValue>("EventValues");

			var conventions = ODataRoutingConventions.CreateDefault();
			conventions.Insert(0, new NavigationIndexRoutingConvention());

			config.Routes.MapODataServiceRoute("ODataRoute", "odata", builder.GetEdmModel(), new DefaultODataPathHandler(), conventions);
			config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{action}/{id}", new { id = RouteParameter.Optional });

			config.Filters.Add(new WebApiExceptionFilterAttribute());
		}

		#endregion

		#region Classes

		public class NavigationIndexRoutingConvention : EntitySetRoutingConvention
		{
			#region Methods

			public override string SelectAction(ODataPath odataPath, HttpControllerContext context, ILookup<string, HttpActionDescriptor> actionMap)
			{
				if (context.Request.Method == HttpMethod.Get && odataPath.PathTemplate == "~/entityset/key/navigation/key")
				{
					var navigationSegment = odataPath.Segments[2] as NavigationPathSegment;
					var navigationProperty = navigationSegment?.NavigationProperty.Partner;
					var declaringType = navigationProperty?.DeclaringType as IEdmEntityType;

					var actionName = "Get" + declaringType?.Name;
					if (actionMap.Contains(actionName))
					{
						// Add keys to route data, so they will bind to action parameters.
						var keyValueSegment = odataPath.Segments[1] as KeyValuePathSegment;
						context.RouteData.Values[ODataRouteConstants.Key] = keyValueSegment?.Value;

						var relatedKeySegment = odataPath.Segments[3] as KeyValuePathSegment;
						context.RouteData.Values[ODataRouteConstants.RelatedKey] = relatedKeySegment?.Value;

						return actionName;
					}
				}

				if (context.Request.Method == HttpMethod.Get && odataPath.PathTemplate == "~/entityset/key/navigation/key/navigation")
				{
					var navigationSegment1 = odataPath.Segments[2] as NavigationPathSegment;
					var navigationProperty1 = navigationSegment1?.NavigationProperty.Partner;
					var declaringType1 = navigationProperty1?.DeclaringType as IEdmEntityType;
					var navigationSegment2 = odataPath.Segments[4] as NavigationPathSegment;
					var navigationProperty2 = navigationSegment2?.NavigationProperty.Partner;
					var declaringType2 = navigationProperty2?.DeclaringType as IEdmEntityType;

					var actionName = "Get" + declaringType1?.Name + navigationSegment2?.NavigationPropertyName;
					if (actionMap.Contains(actionName))
					{
						// Add keys to route data, so they will bind to action parameters.
						var keyValueSegment = odataPath.Segments[1] as KeyValuePathSegment;
						context.RouteData.Values[ODataRouteConstants.Key] = keyValueSegment?.Value;

						var relatedKeySegment = odataPath.Segments[3] as KeyValuePathSegment;
						context.RouteData.Values[ODataRouteConstants.RelatedKey] = relatedKeySegment?.Value;

						return actionName;
					}
				}
				// Not a match.
				return null;
			}

			#endregion
		}

		#endregion
	}
}