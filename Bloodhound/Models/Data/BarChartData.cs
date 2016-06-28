#region References

using System.Collections.Generic;

#endregion

namespace Bloodhound.Models.Data
{
	/// <summary>
	/// Represents data for a bar chart.
	/// </summary>
	public class BarChartData
	{
		#region Constructors

		/// <summary>
		/// Instantiates a new instance of the class.
		/// </summary>
		public BarChartData()
			: this(string.Empty, Global.HtmlColors[0], new decimal[0])
		{
		}

		/// <summary>
		/// Instantiates a new instance of the class.
		/// </summary>
		/// <param name="label"> The label for the data. </param>
		/// <param name="color"> The color of this piece of data. </param>
		/// <param name="data"> The data points for this group. </param>
		public BarChartData(string label, string color, IEnumerable<decimal> data)
		{
			Data = data;
			Label = label;
			FillColor = color.FromHtmlToRgb(0.5m);
			HighlightFill = color.FromHtmlToRgb(0.5m);
			HighlightStroke = color;
			StrokeColor = color;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the data points for this group.
		/// </summary>
		public IEnumerable<decimal> Data { get; set; }

		/// <summary>
		/// Gets or sets the color to fill the bars.
		/// </summary>
		public string FillColor { get; set; }

		/// <summary>
		/// Gets or sets the color to highlight the bars.
		/// </summary>
		public string HighlightFill { get; set; }

		/// <summary>
		/// Gets or sets the color to outline the bars.
		/// </summary>
		public string HighlightStroke { get; set; }

		/// <summary>
		/// Gets or sets the label for this dataset.
		/// </summary>
		public string Label { get; set; }

		/// <summary>
		/// Gets or sets the stroke color of the bars.
		/// </summary>
		public string StrokeColor { get; set; }

		#endregion
	}
}