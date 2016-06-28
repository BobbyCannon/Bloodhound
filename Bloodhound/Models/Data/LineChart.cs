#region References

using System.Collections.Generic;

#endregion

namespace Bloodhound.Models.Data
{
	/// <summary>
	/// Represents a line chart.
	/// </summary>
	public class LineChart
	{
		#region Properties

		/// <summary>
		/// Gets or sets the datasets.
		/// </summary>
		public IEnumerable<LineChartData> Datasets { get; set; }

		/// <summary>
		/// Gets or sets the labels for the datasets.
		/// </summary>
		public IEnumerable<string> Labels { get; set; }

		#endregion
	}
}