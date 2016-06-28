#region References

using System.Collections.Generic;

#endregion

namespace Bloodhound.Models.Data
{
	/// <summary>
	/// Represents a bar chart.
	/// </summary>
	public class BarChart
	{
		#region Properties

		/// <summary>
		/// Gets or sets the datasets.
		/// </summary>
		public IEnumerable<BarChartData> Datasets { get; set; }

		/// <summary>
		/// Gets or sets the labels for the datasets.
		/// </summary>
		public IEnumerable<string> Labels { get; set; }

		#endregion
	}
}