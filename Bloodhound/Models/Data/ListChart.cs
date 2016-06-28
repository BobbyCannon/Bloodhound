#region References

using System.Collections.Generic;

#endregion

namespace Bloodhound.Models.Data
{
	/// <summary>
	/// Represents a list chart.
	/// </summary>
	public class ListChart
	{
		#region Properties

		/// <summary>
		/// Gets or sets the datasets.
		/// </summary>
		public IEnumerable<ListChartData> Datasets { get; set; }

		#endregion
	}
}