#region References

#endregion

namespace Bloodhound.Models.Data
{
	/// <summary>
	/// Represents data for a pie chart.
	/// </summary>
	public class PieChartData
	{
		#region Constructors

		/// <summary>
		/// Instantiates a new instance of the class.
		/// </summary>
		public PieChartData()
			: this(string.Empty, 0, Global.HtmlColors[0])
		{
		}

		/// <summary>
		/// Instantiates a new instance of the class.
		/// </summary>
		/// <param name="label"> The label for the item. </param>
		/// <param name="value"> The value for this item. </param>
		/// <param name="color"> The color of this piece of data. </param>
		public PieChartData(string label, decimal value, string color)
		{
			Color = color;
			HighlightColor = color.FromHtmlToRgb(0.5m);
			Label = label;
			Value = value;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the color for this piece of data.
		/// </summary>
		public string Color { get; set; }

		/// <summary>
		/// Gets or sets the highlight color for this piece of data.
		/// </summary>
		public string HighlightColor { get; set; }

		/// <summary>
		/// Gets or sets the label.
		/// </summary>
		public string Label { get; set; }

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		public decimal Value { get; set; }

		#endregion
	}
}