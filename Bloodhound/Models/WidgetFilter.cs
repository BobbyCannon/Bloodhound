namespace Bloodhound.Models
{
	/// <summary>
	/// Represents a filter for a widget.
	/// </summary>
	public class WidgetFilter : Entity
	{
		#region Properties

		/// <summary>
		/// Gets or sets the name of the widget filter.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the type of filter.
		/// </summary>
		public WidgetFilterType Type { get; set; }

		/// <summary>
		/// Gets or sets the value to filter by.
		/// </summary>
		public string Value { get; set; }

		/// <summary>
		/// Gets or sets the widget this filter is for.
		/// </summary>
		public virtual Widget Widget { get; set; }

		/// <summary>
		/// Gets or sets the ID of the widget.
		/// </summary>
		public int WidgetId { get; set; }

		#endregion

		#region Enumerations

		/// <summary>
		/// Represents the type of filter.
		/// </summary>
		public enum WidgetFilterType
		{
			/// <summary>
			/// Determines whether the data row is equal to the provided value.
			/// </summary>
			Equals
		}

		#endregion
	}
}