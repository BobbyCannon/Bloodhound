#region References

using System.Collections.Generic;

#endregion

namespace Bloodhound.Models.Data
{
	/// <summary>
	/// Represents a pie chart.
	/// </summary>
	public class PieChart
	{
		#region Properties

		/// <summary>
		/// Gets or sets the datasets.
		/// </summary>
		public IEnumerable<PieChartData> Datasets { get; set; }

		/// <summary>
		/// Gets or sets the flag indicating if this chart is a donut chart.
		/// </summary>
		public bool IsDonut { get; set; }

		#endregion
	}
}