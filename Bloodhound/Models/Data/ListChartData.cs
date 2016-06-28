namespace Bloodhound.Models.Data
{
	/// <summary>
	/// Represents data for a list chart.
	/// </summary>
	public class ListChartData
	{
		#region Constructors

		/// <summary>
		/// Instantiates a new instance of the class.
		/// </summary>
		public ListChartData()
			: this(string.Empty, 0)
		{
		}

		/// <summary>
		/// Instantiates a new instance of the class.
		/// </summary>
		/// <param name="label"> The label for the item. </param>
		/// <param name="value"> The value of the item. </param>
		public ListChartData(string label, decimal value)
		{
			Label = label;
			Value = value;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the label for this item.
		/// </summary>
		public string Label { get; set; }

		/// <summary>
		/// Gets or sets the value for this item.
		/// </summary>
		public decimal Value { get; set; }

		#endregion
	}
}