#region References

using System.Collections.Generic;

#endregion

namespace Bloodhound.Models.Data
{
	/// <summary>
	/// Represents data for a line chart.
	/// </summary>
	public class LineChartData
	{
		#region Constructors

		/// <summary>
		/// Instantiates a new instance of the class.
		/// </summary>
		public LineChartData()
			: this(string.Empty, Global.HtmlColors[0], new decimal[0])
		{
		}

		/// <summary>
		/// Instantiates a new instance of the class.
		/// </summary>
		/// <param name="label"> The label for the data. </param>
		/// <param name="color"> The color of this piece of data. </param>
		/// <param name="data"> The data points for this group. </param>
		public LineChartData(string label, string color, IEnumerable<decimal> data)
		{
			Data = data;
			Label = label;
			FillColor = color.FromHtmlToRgb(0.5m);
			PointColor = color;
			PointHighlightFill = color.FromHtmlToRgb(0.5m);
			PointHighlightStroke = color.FromHtmlToRgb(0.5m);
			PointStrokeColor = color;
			StrokeColor = color;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the data points for this group.
		/// </summary>
		public IEnumerable<decimal> Data { get; set; }

		/// <summary>
		/// Gets or sets the color to fill the line.
		/// </summary>
		public string FillColor { get; set; }

		/// <summary>
		/// Gets or sets the label for this dataset.
		/// </summary>
		public string Label { get; set; }

		/// <summary>
		/// Gets or sets the color of the line point.
		/// </summary>
		public string PointColor { get; set; }

		/// <summary>
		/// Gets or sets the highlight color of the line point.
		/// </summary>
		public string PointHighlightFill { get; set; }

		/// <summary>
		/// Gets or sets the highlight color of the line point.
		/// </summary>
		public string PointHighlightStroke { get; set; }

		/// <summary>
		/// Gets or sets the stroke color of the line point.
		/// </summary>
		public string PointStrokeColor { get; set; }

		/// <summary>
		/// Gets or sets the stroke color of the line.
		/// </summary>
		public string StrokeColor { get; set; }

		#endregion
	}
}