namespace Bloodhound.Data
{
	/// <summary>
	/// Represents a provider for a data context.
	/// </summary>
	public interface IDataContextProvider
	{
		#region Methods

		/// <summary>
		/// Get a new instance of a data context.
		/// </summary>
		/// <returns> The instance of the data context. </returns>
		IDataContext GetContext();

		#endregion
	}
}