#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Bloodhound.Data;
using Bloodhound.Models;
using Bloodhound.Models.Data;

#endregion

namespace Bloodhound.Services
{
	/// <summary>
	/// The service to handle all the data channel request. This service also handles all
	/// widget handling for the website.
	/// </summary>
	public class DataService : IDataChannel
	{
		#region Fields

		private readonly IDataContext _context;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a new instance of the class.
		/// </summary>
		/// <param name="context"> The data context for the service. </param>
		public DataService(IDataContext context)
		{
			_context = context;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Adds the widget.
		/// </summary>
		/// <param name="widget"> The widget to be added. </param>
		/// <returns> The widget that was added. </returns>
		public Widget AddWidget(Widget widget)
		{
			widget.Order = _context.Widgets.Select(x => x.Order).DefaultIfEmpty().Max() + 1;
			_context.Widgets.Add(widget);
			_context.SaveChanges();
			return widget;
		}

		/// <summary>
		/// Gets a list of populated widgets.
		/// </summary>
		/// <param name="startDate"> The start date of the session. </param>
		/// <param name="endDate"> The end date of the session. </param>
		/// <returns> The list of populated widgets. </returns>
		public IEnumerable<Widget> GetWidgets(DateTime startDate, DateTime endDate)
		{
			return _context.Widgets.OrderBy(x => x.Order).ToList().Select(x => PopulateWidgetData(x, startDate, endDate)).ToList();
		}

		/// <summary>
		/// Moves the widget order.
		/// </summary>
		/// <param name="id"> The ID of the widget to move. </param>
		/// <param name="moveUp"> The flag to move up otherwise move down. </param>
		public void MoveWidget(int id, bool moveUp)
		{
			var foundWidget = _context.Widgets.FirstOrDefault(x => x.Id == id);
			if (foundWidget == null)
			{
				throw new ArgumentException("The widget could not be found by the provided ID.", nameof(id));
			}

			var otherWidget = moveUp
				? _context.Widgets.OrderByDescending(x => x.Order).FirstOrDefault(x => x.Order < foundWidget.Order)
				: _context.Widgets.OrderBy(x => x.Order).FirstOrDefault(x => x.Order > foundWidget.Order);

			if (otherWidget == null)
			{
				return;
			}

			if (moveUp)
			{
				foundWidget.Order--;
				otherWidget.Order++;
			}
			else
			{
				foundWidget.Order++;
				otherWidget.Order--;
			}

			_context.SaveChanges();
		}

		/// <summary>
		/// Populates a widget with chart data.
		/// </summary>
		/// <param name="widget"> The widget to populate. </param>
		/// <param name="startDate"> The start date of the query. </param>
		/// <param name="endDate"> The end date of the query. </param>
		/// <returns> The widget after populated. </returns>
		/// <remarks> The start and end date only overrides the widget's properties that are null. </remarks>
		public Widget PopulateWidgetData(Widget widget, DateTime startDate, DateTime endDate)
		{
			if (widget.StartDate == null)
			{
				widget.StartDate = startDate;
			}

			if (widget.EndDate == null)
			{
				widget.EndDate = endDate;
			}

			if (widget.TimePeriod.Ticks > 0)
			{
				widget.StartDate = widget.EndDate.Value.AddTicks(widget.TimePeriod.Ticks * -1);
			}

			switch (widget.ChartType)
			{
				case ChartType.Bar:
					widget.Data = GetBarChartData(widget);
					break;

				case ChartType.Line:
					widget.Data = GetLineChartData(widget);
					break;

				case ChartType.List:
					widget.Data = GetListChartData(widget);
					break;

				case ChartType.Pie:
					widget.Data = GetPieChartData(widget);
					break;

				case ChartType.Donut:
					widget.Data = GetPieChartData(widget);
					break;
			}

			return widget;
		}

		/// <summary>
		/// Removes the widget.
		/// </summary>
		/// <param name="id"> The ID of the widget to remove. </param>
		public void RemoveWidget(int id)
		{
			var foundWidget = _context.Widgets.FirstOrDefault(x => x.Id == id);
			if (foundWidget == null)
			{
				throw new ArgumentException("The widget could not be found by the provided ID.", nameof(id));
			}

			_context.Widgets.Remove(foundWidget);
			var order = foundWidget.Order;

			foreach (var reorder in _context.Widgets.Where(x => x.Order > foundWidget.Order).OrderBy(x => x.Order))
			{
				reorder.Order = order++;
			}

			_context.SaveChanges();
		}

		/// <summary>
		/// Writes events for the session.
		/// </summary>
		/// <param name="events"> The events to write. </param>
		public void WriteEvents(IEnumerable<Event> events)
		{
			WriteEvents(events, null);
		}

		/// <summary>
		/// Writes events for the session.
		/// </summary>
		/// <param name="events"> The events to write. </param>
		/// <param name="ipAddress"> The IP address to associate all sessions with. </param>
		public void WriteEvents(IEnumerable<Event> events, string ipAddress)
		{
			foreach (var group in events.GroupBy(x => x.SessionId))
			{
				var existingSession = _context.Events.Include(x => x.Values).FirstOrDefault(x => x.SessionId == group.Key && x.Type == EventType.Session);
				var sessionEvent = existingSession ?? group.FirstOrDefault(x => x.SessionId == group.Key && x.Type == EventType.Session);

				if (sessionEvent != null && !string.IsNullOrWhiteSpace(ipAddress) && !sessionEvent.Values.Any(x => x.Name == "IP Address" && !string.IsNullOrWhiteSpace(x.Value)))
				{
					sessionEvent.Values.AddOrUpdate("IP Address", ipAddress);
				}

				foreach (var entry in group)
				{
					ValidateValues(entry);
					ValidateRelationships(entry);

					if (entry != sessionEvent)
					{
						entry.Parent = sessionEvent;
					}

					_context.Events.AddOrUpdate(entry);
				}

				var completedOn = group.Max(x => x.CompletedOn);
				if (sessionEvent != null && completedOn > sessionEvent.CompletedOn && completedOn > sessionEvent.CreatedOn)
				{
					sessionEvent.CompletedOn = completedOn;
				}
			}

			_context.SaveChanges();
		}

		private static IQueryable<Event> FilterQuery(IQueryable<Event> events, WidgetFilter filter)
		{
			if (string.IsNullOrWhiteSpace(filter.Value))
			{
				return events;
			}

			switch (filter.Name)
			{
				case "Name":
					return events.Where(x => x.Name.Contains(filter.Value));

				case "Date":
					return events.Where(x => x.CreatedOn.ToString().Contains(filter.Value));

				case "UniqueId":
					return events.Where(x => x.UniqueId.ToString().Contains(filter.Value));

				default:
					return events.Where(x => x.Values.Any(y => y.Name == filter.Name && y.Value.Contains(filter.Value)));
			}
		}

		private static decimal FormatAggregateValue(decimal value, Widget request)
		{
			var response = value;

			switch (request.AggregateBy)
			{
				case "ElapsedTime":
					switch (request.AggregateByFormat)
					{
						case "ms":
						case "millisecond":
						case "milliseconds":
							response = (decimal) TimeSpan.FromTicks((long) value).TotalMilliseconds;
							break;

						case "s":
						case "second":
						case "seconds":
							response = (decimal) TimeSpan.FromTicks((long) value).TotalSeconds;
							break;

						case "m":
						case "minute":
						case "minutes":
							response = (decimal) TimeSpan.FromTicks((long) value).TotalMinutes;
							break;

						case "h":
						case "hour":
						case "hours":
							response = (decimal) TimeSpan.FromTicks((long) value).TotalHours;
							break;

						case "d":
						case "day":
						case "days":
							response = (decimal) TimeSpan.FromTicks((long) value).TotalDays;
							break;
					}
					break;
			}

			return Math.Round(response, 2, MidpointRounding.AwayFromZero);
		}

		private object GetBarChartData(Widget request)
		{
			var data = GetData(request);
			return new BarChart { Labels = data.Keys.ToArray(), Datasets = new[] { new BarChartData("Data", Global.HtmlColors[0], data.Values.ToArray()) } };
		}

		private Dictionary<string, decimal> GetData(Widget request)
		{
			var filterStartDate = request.StartDate.GetValueOrDefault();
			var filterEndDate = request.EndDate.GetValueOrDefault().AddDays(1);

			var events = _context.Events
				.Include(x => x.Values)
				.Where(x => x.CreatedOn >= filterStartDate && x.CreatedOn < filterEndDate)
				.Where(x => x.Type == request.EventType);

			if (request.Filters != null && request.Filters.Any())
			{
				events = request.Filters.Aggregate(events, FilterQuery);
			}

			if (request.GroupBy.StartsWith("Session."))
			{
				var filter = request.GroupBy.Replace("Session.", "");
				var eventForSession = events
					.Where(x => x.Type != EventType.Session)
					.Join(_context.Events, x => x.SessionId, x => x.UniqueId, (x, y) => new { Event = x, Session = y })
					.Where(x => x.Session.Values.Any(y => y.Name == filter))
					.GroupBy(x => x.Session.Values.FirstOrDefault(y => y.Name == filter).Value)
					.SelectMany(x => x.Select(y => new { x.Key, Value = y.Event }))
					.GroupBy(x => x.Key, x => x.Value);

				return GetData(eventForSession, request);
			}

			switch (request.GroupBy)
			{
				case "Name":
					return GetData(events.GroupBy(x => x.Name), request);

				case "ElapsedTime":
					return GetData(events.GroupBy(x => x.ElapsedTime), request);

				case "SessionId":
					return GetData(events.GroupBy(x => x.SessionId), request);

				case "Date":
					switch (request.GroupByFormat)
					{
						case "MM/yy":
						case "MM/yyyy":
							return GetData(events.GroupBy(x => new { x.CreatedOn.Year, x.CreatedOn.Month }), request, arg => new DateTime(arg.Year, arg.Month, 1).ToString(request.GroupByFormat))
								.FillDates(request.GroupByFormat, filterStartDate, filterEndDate);

						case "yy":
						case "yyyy":
							return GetData(events.GroupBy(x => new { x.CreatedOn.Year }), request, arg => new DateTime(arg.Year, 1, 1).ToString(request.GroupByFormat))
								.FillDates(request.GroupByFormat, filterStartDate, filterEndDate);

						default:
							return GetData(events.GroupBy(x => new { x.CreatedOn.Year, x.CreatedOn.Day, x.CreatedOn.Month }), request, arg => new DateTime(arg.Year, arg.Month, arg.Day).ToString("MM/dd/yyyy"))
								.FillDates("MM/dd/yyyy", filterStartDate, filterEndDate);
					}

				default:
					var grouping = events.Where(x => x.Values.Any(y => y.Name == request.GroupBy))
						.GroupBy(x => x.Values.FirstOrDefault(y => y.Name == request.GroupBy).Value);
					return GetData(grouping, request, arg => arg);
			}
		}

		private static Dictionary<string, decimal> GetData<T1>(IQueryable<IGrouping<T1, Event>> group, Widget request, Func<T1, string> formatKey = null)
		{
			IQueryable<WidgetDataSet<T1, double>> data;

			switch (request.AggregateBy)
			{
				case "ElapsedTime":
					switch (request.AggregateType)
					{
						case "Average":
							data = group.Select(x => new WidgetDataSet<T1, double> { Key = x.Key, Value = x.Average(y => y.ElapsedTicks) });
							break;

						case "Maximum":
							data = group.Select(x => new WidgetDataSet<T1, double> { Key = x.Key, Value = x.Max(y => y.ElapsedTicks) });
							break;

						case "Minimum":
							data = group.Select(x => new WidgetDataSet<T1, double> { Key = x.Key, Value = x.Min(y => y.ElapsedTicks) });
							break;

						default:
							data = group.Select(x => new WidgetDataSet<T1, double> { Key = x.Key, Value = x.Sum(y => y.ElapsedTicks) });
							break;
					}
					break;

				default:
					data = group.Select(x => new WidgetDataSet<T1, double> { Key = x.Key, Value = x.Count() });
					break;
			}

			var topData = data
				.OrderByDescending(x => x.Value)
				.ToList();

			return topData.ToDictionary(x => formatKey == null ? x.Key.ToString() : formatKey(x.Key), x => FormatAggregateValue((decimal) x.Value, request));
		}

		private object GetLineChartData(Widget request)
		{
			var data = GetData(request);
			return new LineChart { Labels = data.Keys.ToArray(), Datasets = new[] { new LineChartData("Data", Global.HtmlColors[0], data.Values.ToArray()) } };
		}

		private object GetListChartData(Widget request)
		{
			var data = GetData(request)
				.OrderByDescending(x => x.Value)
				.ThenBy(x => x.Key)
				.Take(request.ChartLimit);

			return new ListChart { Datasets = data.Select(x => new ListChartData(x.Key, x.Value)).ToArray() };
		}

		private object GetPieChartData(Widget request)
		{
			var data = GetData(request)
				.OrderByDescending(x => x.Value)
				.ThenBy(x => x.Key)
				.Take(request.ChartLimit);

			var index = 0;
			return new PieChart { Datasets = data.Select(x => new PieChartData(x.Key, x.Value, Global.HtmlColors[index++ % Global.HtmlColors.Length])).ToArray() };
		}

		/// <summary>
		/// Validates the relationships for event.
		/// </summary>
		/// <param name="entry"> The event to update. </param>
		private static void ValidateRelationships(Event entry)
		{
			foreach (var value in entry.Values)
			{
				value.Event = entry;
				value.EventId = 0;
			}

			foreach (var child in entry.Children)
			{
				child.Parent = entry;
				child.ParentId = 0;
				child.SessionId = entry.SessionId;
				ValidateRelationships(child);
			}
		}

		/// <summary>
		/// Validates the value of the event values.
		/// </summary>
		/// <param name="entry"> The event to update. </param>
		private static void ValidateValues(Event entry)
		{
			// Make sure the completed on date is actually after the created on date.
			if (entry.CompletedOn < entry.CreatedOn)
			{
				entry.CompletedOn = entry.CreatedOn;
			}

			// Make sure the unique id is set.
			if (entry.UniqueId == Guid.Empty)
			{
				entry.UniqueId = Guid.NewGuid();
			}
		}

		#endregion

		#region Classes

		private class WidgetDataSet<T1, T2>
		{
			#region Properties

			public T1 Key { get; set; }
			public T2 Value { get; set; }

			#endregion
		}

		#endregion
	}
}