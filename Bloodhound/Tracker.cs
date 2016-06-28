#region References

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using Bloodhound.Data;
using Bloodhound.Models;
using Speedy;

#endregion

namespace Bloodhound
{
	/// <summary>
	/// A tracker to track events and exceptions. Each tracker instance represents a new sessions.
	/// </summary>
	public class Tracker : IDisposable
	{
		#region Fields

		private readonly IDataChannel _channel;
		private BackgroundWorker _eventProcessor;
		private readonly IRepositoryProvider _provider;
		private IRepository _repository;
		private Event _session;

		#endregion

		#region Constructors

		/// <summary>
		/// A tracker to capture, store, and transmit events to a data channel.
		/// </summary>
		/// <param name="channel"> The channel used to store the data remotely. </param>
		/// <param name="provider"> The repository used to store the data locally. </param>
		public Tracker(IDataChannel channel, IRepositoryProvider provider)
		{
			_channel = channel;
			_provider = provider;
			_eventProcessor = new BackgroundWorker();
			_eventProcessor.WorkerSupportsCancellation = true;
			_eventProcessor.DoWork += EventProcessorOnDoWork;

			EventProcessingDelay = 250;
			EventProcessorRunning = false;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the delay in milliseconds between processing events. The event processor will
		/// delay this time between processing of events. There will be a delay 4x this amount when an
		/// error occurs during processing.
		/// </summary>
		public int EventProcessingDelay { get; set; }

		/// <summary>
		/// Gets the running status of the event processor.
		/// </summary>
		public bool EventProcessorRunning { get; private set; }

		#endregion

		#region Methods

		/// <summary>
		/// Adds an event to the tracking session.
		/// </summary>
		/// <param name="name"> The name of the event. </param>
		/// <param name="values"> Optional values for this event. </param>
		public void AddEvent(string name, params EventValue[] values)
		{
			ValidateTrackerState();
			_repository.WriteAndSave(new Event(_session.SessionId, name, values));
		}

		/// <summary>
		/// Adds an event with an existing timespan to the tracking session.
		/// </summary>
		/// <param name="name"> The name of the event. </param>
		/// <param name="elapsedTime"> The elapsed time of the event. </param>
		/// <param name="values"> Optional values for this event. </param>
		public void AddEvent(string name, TimeSpan elapsedTime, params EventValue[] values)
		{
			ValidateTrackerState();
			_repository.WriteAndSave(new Event(_session.SessionId, name, elapsedTime, values));
		}

		/// <summary>
		/// Adds an exception to the tracking session.
		/// </summary>
		/// <param name="exception"> The exception to be added. </param>
		/// <param name="values"> Optional values for this event. </param>
		public void AddException(Exception exception, params EventValue[] values)
		{
			ValidateTrackerState();
			_repository.WriteAndSave(Event.FromException(_session.SessionId, exception, values));
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// A tracker to capture, store, and transmit events to a data channel.
		/// </summary>
		/// <param name="channel"> The channel used to store the data remotely. </param>
		/// <param name="provider"> The repository used to store the data locally. </param>
		/// <param name="values"> The values to associate with this session. </param>
		public static Tracker Start(IDataChannel channel, IRepositoryProvider provider, params EventValue[] values)
		{
			var application = (Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly()).GetName();
			var tracker = new Tracker(channel, provider);
			tracker.Start(application, TimeSpan.Zero, values);
			return tracker;
		}

		/// <summary>
		/// A tracker to capture, store, and transmit events to a data channel.
		/// </summary>
		/// <param name="channel"> The channel used to store the data remotely. </param>
		/// <param name="provider"> The repository used to store the data locally. </param>
		/// <param name="elapsedTime"> The amount of time this tracker should have already been running. </param>
		/// <param name="values"> The values to associate with this session. </param>
		public static Tracker Start(IDataChannel channel, IRepositoryProvider provider, TimeSpan elapsedTime, params EventValue[] values)
		{
			var application = (Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly()).GetName();
			var tracker = new Tracker(channel, provider);
			tracker.Start(application, elapsedTime, values);
			return tracker;
		}

		/// <summary>
		/// Starts a new session.
		/// </summary>
		/// <param name="values"> The values to associate with this session. </param>
		public void Start(params EventValue[] values)
		{
			var application = (Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly()).GetName();
			Start(application, TimeSpan.Zero, values);
		}

		/// <summary>
		/// Starts a new session.
		/// </summary>
		/// <param name="elapsedTime"> The amount of time this tracker has already been running. </param>
		/// <param name="values"> The values to associate with this session. </param>
		public void Start(TimeSpan elapsedTime, params EventValue[] values)
		{
			var application = (Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly()).GetName();
			Start(application, elapsedTime, values);
		}

		/// <summary>
		/// Starts a new event. Once the event is done be sure to call <seealso cref="Event.Complete" />.
		/// </summary>
		/// <param name="name"> The name of the event. </param>
		/// <param name="values"> Optional values for this event. </param>
		/// <returns> The event for tracking an event. </returns>
		public Event StartEvent(string name, params EventValue[] values)
		{
			ValidateTrackerState();
			var response = new Event(_session.SessionId, name, values);
			response.Completed += x => _repository.WriteAndSave(x);
			return response;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <param name="disposing"> A flag determining if we are currently disposing. </param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposing || _repository == null)
			{
				return;
			}

			try
			{
				if (EventProcessorRunning && !_eventProcessor.CancellationPending)
				{
					_eventProcessor.CancelAsync();
					Speedy.Extensions.Wait(() => !EventProcessorRunning, 5000, 10);
				}

				_repository.Flush();

				if (_repository.Count <= 0)
				{
					_repository.Delete();
				}

				_repository.Dispose();
			}
			finally
			{
				_session = null;
				_repository = null;
				_eventProcessor = null;
			}
		}

		/// <summary>
		/// Log a message.
		/// </summary>
		/// <param name="message"> The message to log. </param>
		/// <param name="level"> The level of the message. </param>
		protected virtual void OnLog(string message, LogLevel level = LogLevel.Information)
		{
			Log?.Invoke(message, level);
		}

		private static void EventProcessorOnDoWork(object sender, DoWorkEventArgs args)
		{
			var worker = (BackgroundWorker) sender;
			var tracker = (Tracker) args.Argument;
			var delayWatch = new Stopwatch();
			var shutdownWatch = new Stopwatch();
			var shutdownDelay = tracker.EventProcessingDelay * 4;

			tracker.EventProcessorRunning = true;
			tracker.OnLog("Event processor started.", LogLevel.Debug);

			while (shutdownWatch.Elapsed.TotalMilliseconds < shutdownDelay)
			{
				try
				{
					if (!shutdownWatch.IsRunning && worker.CancellationPending)
					{
						tracker.OnLog("Event processor shutting down...", LogLevel.Debug);
						shutdownWatch.Start();
					}

					if (tracker._repository?.Name == null)
					{
						continue;
					}

					tracker.OnLog("Event processor loop...", LogLevel.Verbose);

					if (tracker.ProcessSession() <= 0)
					{
						// No data to process for our current session so try and send old session data 
						// that may not been able to transmit earlier.
						if (tracker.ProcessOldSessions() <= 0)
						{
							if (shutdownWatch.IsRunning)
							{
								break;
							}

							delayWatch.Restart();
							while (delayWatch.Elapsed.TotalMilliseconds < tracker.EventProcessingDelay && !worker.CancellationPending)
							{
								Thread.Sleep(50);
							}
						}
					}
				}
				catch (Exception ex)
				{
					// Do not log connection issues.
					var message = ex.ToDetailedString();
					tracker.OnLog(message, LogLevel.Critical);

					// An issue occurred and we need to log it and delay.
					Thread.Sleep(shutdownDelay / 2);
				}
			}

			tracker.OnLog("Event processor stopped.", LogLevel.Debug);
			tracker.EventProcessorRunning = false;
		}

		private static Event NewSession(TimeSpan elapsedTime, AssemblyName application, params EventValue[] values)
		{
			var response = new Event
			{
				CompletedOn = DateTime.UtcNow,
				Name = "Session",
				SessionId = Guid.NewGuid(),
				Type = EventType.Session,
				Values = values.ToList()
			};

			response.CreatedOn = response.CompletedOn.Subtract(elapsedTime);
			response.ElapsedTime = elapsedTime;
			response.UniqueId = response.SessionId;
			response.Values.AddOrUpdate(".NET Framework Version", ComputerInfo.FrameworkVersion);
			response.Values.AddOrUpdate("Amount Of Memory", ComputerInfo.Memory);
			response.Values.AddOrUpdate("Application Bitness", Environment.Is64BitProcess ? "64" : "32");
			response.Values.AddOrUpdate("Application Name", application.Name);
			response.Values.AddOrUpdate("Application Version", application.Version.ToString());
			response.Values.AddOrUpdate("Bloodhound Version", Global.Version.ToString());
			response.Values.AddOrUpdate("Machine ID", ComputerInfo.MachineId);
			response.Values.AddOrUpdate("Machine Name", ComputerInfo.MachineName);
			response.Values.AddOrUpdate("Machine User Name", ComputerInfo.UserName);
			response.Values.AddOrUpdate("Number Of Processors", ComputerInfo.NumberOfProcessors);
			response.Values.AddOrUpdate("Operating System Bitness", ComputerInfo.OperatingSystemBitness);
			response.Values.AddOrUpdate("Operating System Name", ComputerInfo.OperatingSystemName);
			response.Values.AddOrUpdate("Operating System Service Pack", ComputerInfo.OperatingSystemServicePack);
			response.Values.AddOrUpdate("Operating System Version", ComputerInfo.OperatingSystemVersion);
			response.Values.AddOrUpdate("Screen Resolution", ComputerInfo.ScreenResolution);
			response.Values.AddOrUpdate("Storage Available Space", ComputerInfo.StorageAvailableSpace);
			response.Values.AddOrUpdate("Storage Total Space", ComputerInfo.StorageTotalSpace);

			return response;
		}

		private int ProcessOldSessions()
		{
			OnLog("Processing old sessions...", LogLevel.Debug);

			using (var repository = _provider.OpenAvailableRepository(_repository?.Name))
			{
				if (repository == null)
				{
					return 0;
				}

				OnLog($"Processing old session: {repository.Name}.", LogLevel.Debug);

				try
				{
					var result = ProcessRepository(this, repository, _channel);
					if (result > 0)
					{
						return result;
					}

					if (repository.Count <= 0)
					{
						repository.Delete();
					}

					return 0;
				}
				catch (Exception ex)
				{
					// Get the detailed issue with the processing.
					var message = ex.ToDetailedString();

					// Determine if the issue was a connection issue
					if (!message.Contains("Unable to connect to the remote server"))
					{
						// Delete the repository on any issue other than connection issue.
						OnLog($"Error processing repository {repository.Name}: {message}.", LogLevel.Critical);
						AddException(ex, new EventValue("Repository Name", repository.Name));
						repository.Archive();
						return 0;
					}

					OnLog($"Error processing repository {repository.Name}: {message}.", LogLevel.Debug);
					return 0;
				}
			}
		}

		private static int ProcessRepository(Tracker tracker, IRepository repository, IDataChannel client)
		{
			if (string.IsNullOrWhiteSpace(repository?.Name))
			{
				return 0;
			}

			tracker.OnLog($"Processing repository: {repository.Name}.", LogLevel.Debug);

			var count = 0;
			var chunk = 300;
			var data = repository.Read().Take(chunk).ToList();

			while (data.Any())
			{
				client.WriteEvents(data.Select(x => x.Value.FromJson<Event>()));
				var keys = new HashSet<string>(data.Select(x => x.Key));
				repository.Remove(keys);
				repository.Save();
				count += data.Count;
				tracker.OnLog($"Wrote {data.Count} from {repository.Name}...", LogLevel.Verbose);
				data = repository.Read().Take(chunk).ToList();
			}

			tracker.OnLog($"Processed repository with total of {count} from {repository.Name}.", LogLevel.Debug);
			return count;
		}

		/// <summary>
		/// Give access to process the session for the event processor worker.
		/// </summary>
		/// <returns> The number of events processed. </returns>
		private int ProcessSession()
		{
			return ProcessRepository(this, _repository, _channel);
		}

		private void Start(AssemblyName application, TimeSpan elapsedTime, EventValue[] values)
		{
			_session = NewSession(elapsedTime, application, values);
			_repository = _provider.OpenRepository(_session);
			_eventProcessor.RunWorkerAsync(this);
			Speedy.Extensions.Wait(() => EventProcessorRunning, 5000, 10);
		}

		/// <summary>
		/// Check to see if the tracker is in a good working state.
		/// </summary>
		private void ValidateTrackerState()
		{
			if (_session != null)
			{
				return;
			}

			const string message = "You must first start the tracker before using it.";
			OnLog(message, LogLevel.Warning);
			throw new InvalidOperationException(message);
		}

		#endregion

		#region Events

		/// <summary>
		/// Event for when the tracker needs to write information.
		/// </summary>
		public event Action<string, LogLevel> Log;

		#endregion
	}
}