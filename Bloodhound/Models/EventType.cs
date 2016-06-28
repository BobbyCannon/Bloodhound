namespace Bloodhound.Models
{
	/// <summary>
	/// Defines the types of event.
	/// </summary>
	public enum EventType
	{
		/// <summary>
		/// The session event type contains all values for a session. All other events should
		/// reference this events unique ID.
		/// </summary>
		Session = 0x01,

		/// <summary>
		/// The event type is just that... an event.
		/// </summary>
		Event = 0x02,

		/// <summary>
		/// The exception type is an event that represents an exception. The event values should
		/// contain the details of the exception. Any inner exceptions should also be children
		/// of this event.
		/// </summary>
		Exception = 0x03
	}
}