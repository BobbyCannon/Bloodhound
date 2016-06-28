#region References

using System;
using System.Collections.Generic;
using Bloodhound.Models;
using Bloodhound.Services;
using Bloodhound.WebSite.Models.Data;

#endregion

namespace Bloodhound.WebSite.Models.Views
{
	public class IndexView
	{
		#region Properties

		public IEnumerable<string> AggregateBy { get; set; }

		public IEnumerable<string> AggregateTypes { get; set; }
		public IEnumerable<string> ChartSizes { get; set; }
		public IEnumerable<string> ChartTypes { get; set; }
		public DateTime EndDate { get; set; }
		public IEnumerable<string> EventTypes { get; set; }
		public IEnumerable<string> FilterBy { get; set; }
		public IEnumerable<string> FilterTypes { get; set; }
		public IEnumerable<string> GroupBy { get; set; }
		public DateTime StartDate { get; set; }
		public IEnumerable<Widget> Widgets { get; set; }

		#endregion

		#region Methods

		public static IndexView Create(DataService service, DataFilter filter = null)
		{
			var response = new IndexView();

			response.EndDate = filter?.EndDate.Date ?? DateTime.Now.Date;
			response.StartDate = filter?.StartDate.Date ?? response.EndDate.AddMonths(-1);
			response.ChartSizes = Enum.GetNames(typeof (ChartSize));
			response.ChartTypes = Enum.GetNames(typeof (ChartType));
			response.EventTypes = Enum.GetNames(typeof (EventType));
			response.AggregateBy = new[] { "Count", "ElapsedTime" };
			response.AggregateTypes = new[] { "Average", "Maximum", "Minimum", "Sum" };
			response.GroupBy = typeof (Event).GetNonEntityPropertyNames(new[] { "Date" }, new[] { "Id", "CreatedOn", "CompletedOn", "ParentId", "Type", "UniqueId" });
			response.FilterBy = typeof (Event).GetNonEntityPropertyNames(new[] { "", "Date" }, new[] { "Id", "CreatedOn", "CompletedOn", "ParentId", "Type" });
			response.FilterTypes = typeof (WidgetFilter.WidgetFilterType).GetEnumNames();
			response.Widgets = service.GetWidgets(response.StartDate, response.EndDate);

			return response;
		}

		#endregion
	}
}