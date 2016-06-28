#region References

using System.Collections.Generic;
using Bloodhound.Data;
using Bloodhound.Models;

#endregion

namespace Bloodhound.Web
{
	/// <summary>
	/// Represents a data channel for the Bloodhound website.
	/// </summary>
	public class WebDataChannel : IDataChannel
	{
		#region Fields

		private readonly string _serverUri;
		private readonly int _timeout;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates an instance of the class.
		/// </summary>
		/// <param name="serverUri"> The server to send data to. </param>
		/// <param name="timeout"> The timeout for each transaction. </param>
		public WebDataChannel(string serverUri, int timeout = 10000)
		{
			_serverUri = serverUri;
			_timeout = timeout;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the current server URI for this data channel.
		/// </summary>
		public string ServerUri => _serverUri;

		#endregion

		#region Methods

		/// <summary>
		/// Writes events for the session.
		/// </summary>
		/// <param name="events"> The events to write. </param>
		public void WriteEvents(IEnumerable<Event> events)
		{
			WebClient.Post(_serverUri, "api/Data/WriteEvents", events, _timeout);
		}

		#endregion
	}
}