#region References

using System.Collections.Generic;
using Bloodhound.Models;

#endregion

namespace Bloodhound.Data
{
	/// <summary>
	/// Contract for communicating Bloodhound data between locations.
	/// </summary>
	public interface IDataChannel
	{
		#region Methods

		/// <summary>
		/// Writes events for the session.
		/// </summary>
		/// <param name="events"> The events to write. </param>
		void WriteEvents(IEnumerable<Event> events);

		#endregion
	}
}