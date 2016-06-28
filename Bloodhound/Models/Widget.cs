#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#endregion

namespace Bloodhound.Models
{
	/// <summary>
	/// Represents a widget item for the Bloodhound website.
	/// </summary>
	public class Widget : Entity
	{
		#region Constructors

		/// <summary>
		/// Instantiates a new instance of the class.
		/// </summary>
		[SuppressMessage("ReSharper", "DoNotCallOverridableMethodsInConstructor")]
		public Widget()
		{
			Filters = new List<WidgetFilter>();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the value to aggregate by.
		/// </summary>
		public string AggregateBy { get; set; }

		/// <summary>
		/// Gets or sets the format value for aggregate by.
		/// </summary>
		public string AggregateByFormat { get; set; }

		/// <summary>
		/// Gets or sets the type of aggregate. Default to count if no type provided.
		/// </summary>
		public string AggregateType { get; set; }

		/// <summary>
		/// Gets or sets the item limit.
		/// </summary>
		public int ChartLimit { get; set; }

		/// <summary>
		/// Gets or sets the chart size.
		/// </summary>
		public ChartSize ChartSize { get; set; }

		/// <summary>
		/// Gets or sets the type of the chart.
		/// </summary>
		public ChartType ChartType { get; set; }

		/// <summary>
		/// Gets or sets the data.
		/// </summary>
		public object Data { get; set; }

		/// <summary>
		/// Gets or sets the optional end date.
		/// </summary>
		public DateTime? EndDate { get; set; }

		/// <summary>
		/// Gets or sets the event type this widget is for.
		/// </summary>
		public EventType EventType { get; set; }

		/// <summary>
		/// Gets or sets the filters.
		/// </summary>
		public virtual ICollection<WidgetFilter> Filters { get; set; }

		/// <summary>
		/// Gets or sets the value to group by.
		/// </summary>
		public string GroupBy { get; set; }

		/// <summary>
		/// Gets or sets the format value for group by.
		/// </summary>
		public string GroupByFormat { get; set; }

		/// <summary>
		/// Gets or sets the name for the widget.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the order in which the widgets will be display.
		/// </summary>
		public int Order { get; set; }

		/// <summary>
		/// Gets or sets the optional start date.
		/// </summary>
		public DateTime? StartDate { get; set; }

		/// <summary>
		/// Gets or sets the optional time period.
		/// </summary>
		public TimeSpan TimePeriod
		{
			get { return TimeSpan.FromTicks(TimePeriodTicks); }
			set { TimePeriodTicks = value.Ticks; }
		}

		/// <summary>
		/// Gets or set the time period ticks.
		/// </summary>
		public long TimePeriodTicks { get; set; }

		#endregion
	}

	/// <summary>
	/// Represents the different sizes of charts.
	/// </summary>
	public enum ChartSize
	{
		/// <summary>
		/// Small Chart
		/// </summary>
		Small,

		/// <summary>
		/// Medium Chart
		/// </summary>
		Medium,

		/// <summary>
		/// Large Chart
		/// </summary>
		Large
	}

	/// <summary>
	/// Represents the different types of charts.
	/// </summary>
	public enum ChartType
	{
		/// <summary>
		/// Bar Chart
		/// </summary>
		Bar,

		/// <summary>
		/// Line Chart
		/// </summary>
		Line,

		/// <summary>
		/// Pie Chart
		/// </summary>
		Pie,

		/// <summary>
		/// Donut Chart
		/// </summary>
		Donut,

		/// <summary>
		/// List Chart
		/// </summary>
		List
	}
}