#region References

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

#endregion

namespace Bloodhound.Models
{
	/// <summary>
	/// Represents an event. There are three types (<seealso cref="EventType" />) of events of Session, Event, and Exception.
	/// </summary>
	[Serializable]
	public class Event : Entity
	{
		#region Constructors

		/// <summary>
		/// Instantiates a new instance of the class.
		/// </summary>
		[SuppressMessage("ReSharper", "DoNotCallOverridableMethodsInConstructor")]
		public Event()
		{
			Children = new Collection<Event>();
			CreatedOn = DateTime.UtcNow;
			CompletedOn = CreatedOn;
			Name = string.Empty;
			SessionId = Guid.Empty;
			Type = EventType.Event;
			UniqueId = Guid.NewGuid();
			Values = new List<EventValue>();
		}

		/// <summary>
		/// Instantiates a new instance of the class.
		/// </summary>
		[SuppressMessage("ReSharper", "DoNotCallOverridableMethodsInConstructor")]
		public Event(Guid sessionId, string name, params EventValue[] values)
			: this()
		{
			if (sessionId == Guid.Empty)
			{
				throw new ArgumentException("The session ID cannot be empty.", nameof(sessionId));
			}

			if (name == null)
			{
				throw new ArgumentNullException(nameof(name), "The name cannot be null.");
			}

			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("The name is required.", nameof(name));
			}

			SessionId = sessionId;
			Name = name;
			Values.AddOrUpdateRange(values);
		}

		/// <summary>
		/// Instantiates a new instance of the class.
		/// </summary>
		[SuppressMessage("ReSharper", "DoNotCallOverridableMethodsInConstructor")]
		public Event(Guid sessionId, string name, TimeSpan elapsedTime, params EventValue[] values)
			: this(sessionId, name, values)
		{
			CreatedOn = CompletedOn.Subtract(elapsedTime);
			ElapsedTime = elapsedTime;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or set the child events.
		/// </summary>
		public virtual Collection<Event> Children { get; set; }

		/// <summary>
		/// Gets or set the date and time the event was completed.
		/// </summary>
		public DateTime CompletedOn { get; set; }

		/// <summary>
		/// Gets or sets the date and time the event was created.
		/// </summary>
		public DateTime CreatedOn { get; set; }

		/// <summary>
		/// Gets the elapsed ticks. The CompletedOn property will be updated.
		/// </summary>
		public long ElapsedTicks
		{
			get { return (CompletedOn - CreatedOn).Ticks; }
			set { CompletedOn = CreatedOn.AddTicks(value); }
		}

		/// <summary>
		/// Gets the elapsed time between the created and completed date and time.
		/// </summary>
		public TimeSpan ElapsedTime
		{
			get { return TimeSpan.FromTicks(ElapsedTicks); }
			set { ElapsedTicks = value.Ticks; }
		}

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the parent.
		/// </summary>
		public virtual Event Parent { get; set; }

		/// <summary>
		/// Gets or sets the parent ID.
		/// </summary>
		public int? ParentId { get; set; }

		/// <summary>
		/// Gets or sets the session ID.
		/// </summary>
		public Guid SessionId { get; set; }

		/// <summary>
		/// Gets or sets the event type.
		/// </summary>
		public EventType Type { get; set; }

		/// <summary>
		/// Gets or sets the unique ID.
		/// </summary>
		public Guid UniqueId { get; set; }

		/// <summary>
		/// Gets or sets the values.
		/// </summary>
		public virtual ICollection<EventValue> Values { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Adds a child event to this event.
		/// </summary>
		/// <param name="name"> The name of the event. </param>
		/// <param name="values"> Optional values for this event. </param>
		public void AddEvent(string name, params EventValue[] values)
		{
			Children.Add(new Event(SessionId, name, values));
		}

		/// <summary>
		/// Completes the event and adds it to the event or tracker.
		/// </summary>
		public void Complete()
		{
			CompletedOn = DateTime.UtcNow;
			Completed?.Invoke(this);
			Completed = null;
		}

		/// <summary>
		/// Starts a new event. The event will need to be completed or disposed before it will be added to the tracker.
		/// </summary>
		/// <param name="sessionId"> The ID of the session for this event. </param>
		/// <param name="ex"> The exception to be turned into an event. </param>
		/// <param name="values"> Optional values for this event. </param>
		/// <returns> The event for tracking an event. </returns>
		public static Event FromException(Guid sessionId, Exception ex, params EventValue[] values)
		{
			if (ex == null)
			{
				throw new ArgumentNullException(nameof(ex), "The exception cannot be null.");
			}

			var eventValues = new List<EventValue>(values);
			eventValues.AddOrUpdateRange(new EventValue("Message", ex.Message), new EventValue("Stack Trace", ex.StackTrace ?? string.Empty));

			var response = new Event(sessionId, ex.GetType().Name, eventValues.ToArray());
			response.Type = EventType.Exception;

			if (ex.InnerException == null)
			{
				return response;
			}

			var childException = FromException(sessionId, ex.InnerException);
			response.Children.Add(childException);
			return response;
		}

		/// <summary>
		/// Starts a new event. Once the event is done be sure to call <seealso cref="Complete" />.
		/// </summary>
		/// <param name="name"> The name of the event. </param>
		/// <param name="values"> Optional values for this event. </param>
		/// <returns> The event for tracking an event. </returns>
		public Event StartEvent(string name, params EventValue[] values)
		{
			var response = new Event(SessionId, name, values);
			response.Completed += x => { Children.Add(x); };
			return response;
		}

		#endregion

		#region Events

		/// <summary>
		/// Occurs when the event is completed.
		/// </summary>
		internal event Action<Event> Completed;

		#endregion
	}
}