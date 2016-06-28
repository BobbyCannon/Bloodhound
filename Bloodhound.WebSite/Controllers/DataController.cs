#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Bloodhound.Data;
using Bloodhound.Models;
using Bloodhound.Services;
using Bloodhound.WebSite.Attributes;
using Bloodhound.WebSite.Models.Data;
using Bloodhound.WebSite.Models.Views;

#endregion

namespace Bloodhound.WebSite.Controllers
{
	public class DataController : ApiController, IDataChannel
	{
		#region Fields

		private readonly DataContext _context;

		#endregion

		#region Constructors

		public DataController()
		{
			_context = new DataContext();
		}

		#endregion

		#region Methods

		[HttpPost]
		[Throttle(Name = "AddWidget", RequestsPerSecond = 2)]
		public Widget AddWidget([FromUri] DateTime startDate, [FromUri] DateTime endDate, Widget request)
		{
			var service = new DataService(_context);
			var widget = service.AddWidget(request);
			service.PopulateWidgetData(widget, startDate, endDate);
			return widget;
		}

		[HttpPost]
		public Widget GetWidgetPreview([FromUri] DateTime startDate, [FromUri] DateTime endDate, [FromBody] Widget request)
		{
			var widget = new DataService(_context).PopulateWidgetData(request, startDate, endDate);
			return widget;
		}

		[HttpPost]
		[Throttle(Name = "MoveWidget", RequestsPerSecond = 2)]
		public void MoveWidget([FromUri] int id, [FromUri] string direction)
		{
			new DataService(_context).MoveWidget(id, direction == "up");
		}

		[HttpPost]
		[Throttle(Name = "RefreshDashboard", RequestsPerSecond = 2)]
		public IndexView RefreshDashboard(DataFilter filter)
		{
			return IndexView.Create(new DataService(_context), filter);
		}

		[HttpPost]
		[Throttle(Name = "RefreshIndex", RequestsPerSecond = 2)]
		public IndexView RefreshIndex(DataFilter filter)
		{
			return IndexView.Create(new DataService(_context), filter);
		}

		[HttpPost]
		[Throttle(Name = "RemoveWidget", RequestsPerSecond = 2)]
		public void RemoveWidget([FromUri] int id)
		{
			new DataService(_context).RemoveWidget(id);
		}

		[HttpPost]
		[Throttle(Name = "WriteEvents", RequestsPerSecond = 500)]
		public void WriteEvents(IEnumerable<Event> events)
		{
			new DataService(_context).WriteEvents(events, GetIpAddress());
		}

		protected static string GetIpAddress()
		{
			var context = HttpContext.Current;
			var forwardedAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
			var remoteAddress = context.Request.ServerVariables["REMOTE_ADDR"];

			return forwardedAddress?.Split(',').FirstOrDefault() ?? remoteAddress;
		}

		#endregion
	}
}