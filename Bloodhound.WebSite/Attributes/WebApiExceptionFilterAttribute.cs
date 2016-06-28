#region References

using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;

#endregion

namespace Bloodhound.WebSite.Attributes
{
	public class WebApiExceptionFilterAttribute : ExceptionFilterAttribute
	{
		#region Methods

		public override void OnException(HttpActionExecutedContext context)
		{
			var debugging = HttpContext.Current.IsDebuggingEnabled;

			if (context.Exception is NotImplementedException)
			{
				context.Response = context.Request.CreateResponse(HttpStatusCode.NotImplemented);
				return;
			}

			if (context.Exception is ArgumentException)
			{
				context.Response = debugging
					? context.Request.CreateErrorResponse(HttpStatusCode.BadRequest, context.Exception.ToDetailedString(), context.Exception)
					: context.Request.CreateErrorResponse(HttpStatusCode.BadRequest, context.Exception.CleanMessage());

				return;
			}

			context.Response = debugging
				? context.Request.CreateErrorResponse(HttpStatusCode.BadRequest, context.Exception.ToDetailedString(), context.Exception)
				: context.Request.CreateErrorResponse(HttpStatusCode.BadRequest, context.Exception.Message);
		}

		#endregion
	}
}