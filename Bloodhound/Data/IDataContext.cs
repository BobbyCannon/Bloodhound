#region References

using System;
using Bloodhound.Models;

#endregion

namespace Bloodhound.Data
{
	/// <summary>
	/// Represents a data context of repositories.
	/// </summary>
	public interface IDataContext : IDisposable
	{
		#region Properties

		/// <summary>
		/// Gets the list of events.
		/// </summary>
		IRepository<Event> Events { get; }

		/// <summary>
		/// Gets the list of event values.
		/// </summary>
		IRepository<EventValue> EventValues { get; }

		/// <summary>
		/// Gets the list of widget filters.
		/// </summary>
		IRepository<WidgetFilter> WidgetFilters { get; }

		/// <summary>
		/// Gets the list of widgets.
		/// </summary>
		IRepository<Widget> Widgets { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Save all changes (add, updates, removes) in the context.
		/// </summary>
		/// <returns> The number of changes successfully saved. </returns>
		int SaveChanges();

		#endregion
	}
}