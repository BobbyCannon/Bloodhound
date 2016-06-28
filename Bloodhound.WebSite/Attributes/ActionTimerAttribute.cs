#region References

using System.Diagnostics;
using System.Web.Mvc;

#endregion

namespace Bloodhound.WebSite.Attributes
{
	public class ActionTimerAttribute : ActionFilterAttribute
	{
		#region Constructors

		public ActionTimerAttribute()
		{
			// Make sure this attribute executes after every other one!
			Order = int.MaxValue;
		}

		#endregion

		#region Methods

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			var controller = filterContext.Controller;
			var timer = (Stopwatch) controller?.ViewData["ActionTimer"];
			if (timer == null)
			{
				return;
			}

			timer.Stop();
			controller.ViewData["ElapsedTime"] = timer.Elapsed.ToHumanReadableString();
		}

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			var controller = filterContext.Controller;
			if (controller != null)
			{
				var timer = new Stopwatch();
				controller.ViewData["ActionTimer"] = timer;
				timer.Start();
			}
			base.OnActionExecuting(filterContext);
		}

		#endregion
	}
}