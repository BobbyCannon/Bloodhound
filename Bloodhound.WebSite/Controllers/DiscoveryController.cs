#region References

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using Bloodhound.WebSite.Models.Data;

#endregion

namespace Bloodhound.WebSite.Controllers
{
	[AllowAnonymous]
	[ApiExplorerSettings(IgnoreApi = true)]
	public class DiscoveryController : ApiController
	{
		#region Methods

		[HttpGet]
		[ActionName("Actions")]
		public IEnumerable<string> GetActionList()
		{
			return Configuration
				.Properties
				.GetOrAdd("actionList", k => Configuration
					.Services
					.GetApiExplorer()
					.ApiDescriptions
					.Select(apiDescription => apiDescription.HttpMethod.Method + " - " + apiDescription.RelativePath)
					.ToArray()
				) as string[];
		}

		[HttpGet]
		[ActionName("Postman")]
		public PostmanCollection GetDescriptionForPostman()
		{
			var collection = Configuration.Properties.GetOrAdd("postmanCollection", k =>
			{
				var requestUri = Request.RequestUri;
				var baseUri = requestUri.Scheme + "://" + requestUri.Host + ":" + requestUri.Port + HttpContext.Current.Request.ApplicationPath;

				var postManCollection = new PostmanCollection();
				postManCollection.Name = "Bloodhound API Service";
				postManCollection.Id = Guid.NewGuid();
				postManCollection.Timestamp = DateTime.UtcNow.Ticks;
				postManCollection.Requests = new Collection<PostmanRequest>();

				foreach (var apiDescription in Configuration.Services.GetApiExplorer().ApiDescriptions)
				{
					var request = new PostmanRequest
					{
						CollectionId = postManCollection.Id,
						Id = Guid.NewGuid(),
						Method = apiDescription.HttpMethod.Method,
						Url = baseUri.TrimEnd('/') + "/" + apiDescription.RelativePath,
						Description = apiDescription.Documentation,
						Name = apiDescription.RelativePath,
						Data = "",
						Headers = "",
						DataMode = "params",
						Timestamp = 0
					};

					postManCollection.Requests.Add(request);
				}

				return postManCollection;
			}) as PostmanCollection;

			return collection;
		}

		#endregion
	}
}