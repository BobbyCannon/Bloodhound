#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using Bloodhound.Data;
using Bloodhound.Models;
using Bloodhound.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Speedy;
using TestR.PowerShell;

#endregion

namespace Bloodhound.IntegrationTests
{
	[TestClass]
	[Cmdlet(VerbsDiagnostic.Test, "Tracker")]
	public class TrackerTests : TestCmdlet
	{
		#region Methods

		[TestMethod]
		public void DisposeInsideUsing()
		{
			var provider = new RepositoryProvider(TestHelper.Directory, TimeSpan.FromDays(1), 10000);
			var channel = new Mock<IDataChannel>();

			using (TestHelper.CreateDataContext())
			{
				using (var tracker = new Tracker(channel.Object, provider))
				{
					tracker.Dispose();
				}
			}
		}

		[TestMethod]
		public void Event()
		{
			var provider = new RepositoryProvider(TestHelper.Directory, TimeSpan.FromDays(1), 10000);
			var channel = new Mock<IDataChannel>();
			var actual = new List<Event>();

			channel.Setup(x => x.WriteEvents(It.IsAny<IEnumerable<Event>>()))
				.Callback<IEnumerable<Event>>(x => actual.AddRange(x));

			using (var tracker = Tracker.Start(channel.Object, provider))
			{
				tracker.AddEvent("Event");
			}

			actual.Wait(x => x.Count == 2);
			Assert.AreEqual(2, actual.Count);
			Assert.AreEqual("Session", actual[0].Name);
			Assert.AreEqual("Event", actual[1].Name);
			Assert.AreEqual(0, actual[1].ElapsedTime.TotalMilliseconds);
		}

		[TestMethod]
		[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
		public void EventProcessingDelay()
		{
			var channel = new Mock<IDataChannel>();
			var provider = new RepositoryProvider(TestHelper.Directory, TimeSpan.FromDays(1), 10000);
			var actual = new List<Event>();

			channel.Setup(x => x.WriteEvents(It.IsAny<IEnumerable<Event>>()))
				.Callback<IEnumerable<Event>>(x => actual.AddRange(x));

			var watch = Stopwatch.StartNew();

			using (var tracker = new Tracker(channel.Object, provider))
			{
				tracker.EventProcessingDelay = 250;
				tracker.Start();
				tracker.Wait(x => x.EventProcessorRunning);
				actual.Wait(x => x.Count == 1);
				Thread.Sleep(50);
				tracker.AddEvent("Event");
				actual.Wait(x => x.Count == 2);
			}

			watch.Stop();
			Console.WriteLine(watch.Elapsed.TotalMilliseconds);
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds > 250);
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds < 1000);

			actual.Wait(x => x.Count == 2);
			Assert.AreEqual(2, actual.Count);
			Assert.AreEqual("Session", actual[0].Name);
			Assert.AreEqual("Event", actual[1].Name);
			Assert.AreEqual(0, actual[1].ElapsedTime.TotalMilliseconds);
		}

		[TestMethod]
		[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
		public void EventProcessingDelayCustomValue()
		{
			var channel = new Mock<IDataChannel>();
			var provider = new RepositoryProvider(TestHelper.Directory, TimeSpan.FromDays(1), 10000);
			var actual = new List<Event>();

			channel.Setup(x => x.WriteEvents(It.IsAny<IEnumerable<Event>>()))
				.Callback<IEnumerable<Event>>(x => actual.AddRange(x));

			var watch = Stopwatch.StartNew();

			using (var tracker = new Tracker(channel.Object, provider))
			{
				tracker.EventProcessingDelay = 1000;
				tracker.Start();
				tracker.Wait(x => x.EventProcessorRunning);
				actual.Wait(x => x.Count == 1);
				Thread.Sleep(50);
				tracker.AddEvent("Event");
				actual.Wait(x => x.Count == 2);
			}

			watch.Stop();
			Console.WriteLine(watch.Elapsed.TotalMilliseconds);
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds > 1000);
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds < 2000);

			actual.Wait(x => x.Count == 2);
			Assert.AreEqual(2, actual.Count);
			Assert.AreEqual("Session", actual[0].Name);
			Assert.AreEqual("Event", actual[1].Name);
			Assert.AreEqual(0, actual[1].ElapsedTime.TotalMilliseconds);
		}

		[TestMethod]
		public void EventProcessingDelayShouldNotKeepTrackerFromShuttingDown()
		{
			var provider = new RepositoryProvider(TestHelper.Directory, TimeSpan.FromDays(1), 10000);
			var channel = new Mock<IDataChannel>();
			var actual = Stopwatch.StartNew();
			var events = new List<Event>();

			channel.Setup(x => x.WriteEvents(It.IsAny<IEnumerable<Event>>()))
				.Callback<IEnumerable<Event>>(x => events.AddRange(x));

			using (var tracker = new Tracker(channel.Object, provider))
			{
				tracker.EventProcessingDelay = 5000;
				tracker.Start();

				events.Wait(x => x.Count >= 1);
				Thread.Sleep(100);
			}

			Assert.IsTrue(actual.Elapsed.TotalMilliseconds < 5000);
		}

		[TestMethod]
		public void EventWithElapsedTime()
		{
			var channel = new Mock<IDataChannel>();
			var provider = new RepositoryProvider(TestHelper.Directory, TimeSpan.FromDays(1), 10000);
			var actual = new List<Event>();
			var elapsedTime = TimeSpan.FromTicks(5646134);

			channel.Setup(x => x.WriteEvents(It.IsAny<IEnumerable<Event>>()))
				.Callback<IEnumerable<Event>>(x => actual.AddRange(x));

			using (TestHelper.CreateDataContext())
			{
				using (var tracker = new Tracker(channel.Object, provider))
				{
					tracker.Start();
					tracker.AddEvent("Boom", elapsedTime);
				}

				Assert.AreEqual(2, actual.Count);
				Assert.AreEqual(TimeSpan.Zero, actual[0].ElapsedTime);
				Assert.AreEqual(elapsedTime, actual[1].ElapsedTime);
				Assert.AreEqual(elapsedTime, actual[1].CompletedOn - actual[1].CreatedOn);
			}
		}

		[TestMethod]
		public void Exception()
		{
			var channel = new Mock<IDataChannel>();
			var provider = new RepositoryProvider(TestHelper.Directory, TimeSpan.FromDays(1), 10000);
			var actual = new List<Event>();

			channel.Setup(x => x.WriteEvents(It.IsAny<IEnumerable<Event>>()))
				.Callback<IEnumerable<Event>>(x => actual.AddRange(x));

			using (var tracker = Tracker.Start(channel.Object, provider))
			{
				tracker.AddException(new Exception("Boom"));
			}

			Assert.IsTrue(actual.Wait(x => x.Count == 2));

			Assert.AreEqual(2, actual.Count);
			Assert.AreEqual("Session", actual[0].Name);
			Assert.AreEqual("Exception", actual[1].Name);
			Assert.AreEqual(0, actual[1].ElapsedTime.TotalMilliseconds);

			var actualValues = actual[1].Values;
			Assert.AreEqual(2, actualValues.Count);
		}

		[TestInitialize]
		public void Initialize()
		{
			TestHelper.Directory.SafeDelete();
		}

		[TestMethod]
		public void Requeue()
		{
			var mockChannel = new Mock<IDataChannel>();
			var provider = new RepositoryProvider(TestHelper.Directory, TimeSpan.FromDays(1), 10000);
			var count = 0;
			var expected = new List<string>();
			var actual = new List<Event>();

			mockChannel.Setup(x => x.WriteEvents(It.IsAny<IEnumerable<Event>>()))
				.Callback<IEnumerable<Event>>(events =>
				{
					count++;

					if (count == 1 || count == 3 || count == 5)
					{
						Console.WriteLine("Nope: " + count + "  - Events" + events.Count());
						throw new Exception("Nope");
					}

					actual.AddRange(events);
				});

			using (var tracker = Tracker.Start(mockChannel.Object, provider))
			{
				expected.Add("Session");

				for (var i = 0; i < 1500; i++)
				{
					tracker.AddEvent(i.ToString());
					expected.Add(i.ToString());
				}

				actual.Wait(x => x.Count == 1501);
			}

			TestHelper.AreEqual(expected, actual.Select(x => x.Name).ToList());
		}

		[TestMethod]
		public void SessionIpShouldNotBeOverwritten()
		{
			var provider = new RepositoryProvider(TestHelper.Directory, TimeSpan.FromDays(1), 10000);
			var channel = new Mock<IDataChannel>();
			var events = new List<Event>();

			channel.Setup(x => x.WriteEvents(It.IsAny<IEnumerable<Event>>()))
				.Callback<IEnumerable<Event>>(x => events.AddRange(x));

			Tracker.Start(channel.Object, provider, new EventValue("IP Address", "127.0.0.1")).Dispose();
			Assert.IsTrue(events.Wait(x => x.Count == 1));
			var actual = events.FirstOrDefault();
			Assert.IsNotNull(actual);
			var ipAddress = actual.Values.FirstOrDefault(x => x.Name == "IP Address");
			Assert.IsNotNull(ipAddress);
			Assert.AreEqual("127.0.0.1", ipAddress.Value);
		}

		[TestMethod]
		public void SessionValues()
		{
			var channel = new Mock<IDataChannel>();
			var provider = new RepositoryProvider(TestHelper.Directory, TimeSpan.FromDays(1), 10000);
			var events = new List<Event>();

			channel.Setup(x => x.WriteEvents(It.IsAny<IEnumerable<Event>>()))
				.Callback<IEnumerable<Event>>(x => events.AddRange(x));

			using (TestHelper.CreateDataContext())
			{
				using (var tracker = new Tracker(channel.Object, provider))
				{
					tracker.Start();
				}

				Assert.AreEqual(1, events.Count);
				Assert.AreEqual(0, events[0].ElapsedTime.Ticks);
			}

			var expected = new[]
			{
				".NET Framework Version", "Amount Of Memory", "Application Bitness", "Application Name",
				"Application Version", "Bloodhound Version", "Machine ID", "Machine Name", "Machine User Name",
				"Number Of Processors", "Operating System Bitness", "Operating System Name", "Operating System Service Pack",
				"Operating System Version", "Screen Resolution", "Storage Available Space", "Storage Total Space"
			};

			var actual = events[0].Values.Select(x => x.Name).OrderBy(x => x).ToArray();
			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void Start()
		{
			var channel = new Mock<IDataChannel>();
			var provider = new RepositoryProvider(TestHelper.Directory, TimeSpan.FromDays(1), 10000);
			var actual = new List<Event>();

			channel.Setup(x => x.WriteEvents(It.IsAny<IEnumerable<Event>>()))
				.Callback<IEnumerable<Event>>(x => actual.AddRange(x));

			using (TestHelper.CreateDataContext())
			{
				using (var tracker = new Tracker(channel.Object, provider))
				{
					tracker.Start();
				}

				Assert.AreEqual(1, actual.Count);
				Assert.AreEqual(0, actual[0].ElapsedTime.Ticks);
			}

			var applicationName = actual[0].Values.First(x => x.Name == "Application Name");
			Assert.AreEqual("Bloodhound.IntegrationTests", applicationName.Value);
		}

		[TestMethod]
		public void StartFactory()
		{
			var channel = new Mock<IDataChannel>();
			var provider = new RepositoryProvider(TestHelper.Directory, TimeSpan.FromDays(1), 10000);
			var actual = new List<Event>();

			channel.Setup(x => x.WriteEvents(It.IsAny<IEnumerable<Event>>()))
				.Callback<IEnumerable<Event>>(x => actual.AddRange(x));

			using (TestHelper.CreateDataContext())
			{
				using (Tracker.Start(channel.Object, provider))
				{
				}

				Assert.AreEqual(1, actual.Count);
				Assert.AreEqual(0, actual[0].ElapsedTime.Ticks);
			}

			var applicationName = actual[0].Values.First(x => x.Name == "Application Name");
			Assert.AreEqual("Bloodhound.IntegrationTests", applicationName.Value);
		}

		[TestMethod]
		public void StartFactoryWithCustomValues()
		{
			var channel = new Mock<IDataChannel>();
			var provider = new RepositoryProvider(TestHelper.Directory, TimeSpan.FromDays(1), 10000);
			var actual = new List<Event>();

			channel.Setup(x => x.WriteEvents(It.IsAny<IEnumerable<Event>>()))
				.Callback<IEnumerable<Event>>(x => actual.AddRange(x));

			using (TestHelper.CreateDataContext())
			{
				using (Tracker.Start(channel.Object, provider, new EventValue("Foo", "Bar")))
				{
				}

				Assert.AreEqual(1, actual.Count);
				Assert.AreEqual(0, actual[0].ElapsedTime.Ticks);
				Assert.AreEqual(18, actual[0].Values.Count);

				var eventValue = actual[0].Values.FirstOrDefault(x => x.Name == "Foo");
				Assert.IsNotNull(eventValue);
				Assert.AreEqual("Foo", eventValue.Name);
				Assert.AreEqual("Bar", eventValue.Value);
			}

			var applicationName = actual[0].Values.First(x => x.Name == "Application Name");
			Assert.AreEqual("Bloodhound.IntegrationTests", applicationName.Value);
		}

		[TestMethod]
		public void StartFactoryWithElapsedTime()
		{
			var channel = new Mock<IDataChannel>();
			var provider = new RepositoryProvider(TestHelper.Directory, TimeSpan.FromDays(1), 10000);
			var actual = new List<Event>();
			var elapsedTime = TimeSpan.FromTicks(5465421643);

			channel.Setup(x => x.WriteEvents(It.IsAny<IEnumerable<Event>>()))
				.Callback<IEnumerable<Event>>(x => actual.AddRange(x));

			using (TestHelper.CreateDataContext())
			{
				using (Tracker.Start(channel.Object, provider, elapsedTime))
				{
				}

				Assert.AreEqual(1, actual.Count);
				Assert.AreEqual(elapsedTime, actual[0].ElapsedTime);
				Assert.AreEqual(elapsedTime, actual[0].CompletedOn - actual[0].CreatedOn);
			}

			var applicationName = actual[0].Values.First(x => x.Name == "Application Name");
			Assert.AreEqual("Bloodhound.IntegrationTests", applicationName.Value);
		}

		[TestMethod]
		public void StartFactoryWithElapsedTimeAndCustomValues()
		{
			var channel = new Mock<IDataChannel>();
			var provider = new RepositoryProvider(TestHelper.Directory, TimeSpan.FromDays(1), 10000);
			var actual = new List<Event>();
			var elapsedTime = TimeSpan.FromTicks(413124564);

			channel.Setup(x => x.WriteEvents(It.IsAny<IEnumerable<Event>>()))
				.Callback<IEnumerable<Event>>(x => actual.AddRange(x));

			using (TestHelper.CreateDataContext())
			{
				using (Tracker.Start(channel.Object, provider, elapsedTime, new EventValue("Foo", "Bar")))
				{
				}

				Assert.AreEqual(1, actual.Count);
				Assert.AreEqual(elapsedTime, actual[0].ElapsedTime);
				Assert.AreEqual(18, actual[0].Values.Count);

				var eventValue = actual[0].Values.FirstOrDefault(x => x.Name == "Foo");
				Assert.IsNotNull(eventValue);
				Assert.AreEqual("Foo", eventValue.Name);
				Assert.AreEqual("Bar", eventValue.Value);
			}

			var applicationName = actual[0].Values.First(x => x.Name == "Application Name");
			Assert.AreEqual("Bloodhound.IntegrationTests", applicationName.Value);
		}

		[TestMethod]
		public void StartWithCustomValues()
		{
			var channel = new Mock<IDataChannel>();
			var provider = new RepositoryProvider(TestHelper.Directory, TimeSpan.FromDays(1), 10000);
			var actual = new List<Event>();

			channel.Setup(x => x.WriteEvents(It.IsAny<IEnumerable<Event>>()))
				.Callback<IEnumerable<Event>>(x => actual.AddRange(x));

			using (TestHelper.CreateDataContext())
			{
				using (var tracker = new Tracker(channel.Object, provider))
				{
					tracker.Start(new EventValue("Foo", "Bar"));
				}

				Assert.AreEqual(1, actual.Count);
				Assert.AreEqual(0, actual[0].ElapsedTime.Ticks);
				Assert.AreEqual(18, actual[0].Values.Count);

				var eventValue = actual[0].Values.FirstOrDefault(x => x.Name == "Foo");
				Assert.IsNotNull(eventValue);
				Assert.AreEqual("Foo", eventValue.Name);
				Assert.AreEqual("Bar", eventValue.Value);
			}

			var applicationName = actual[0].Values.First(x => x.Name == "Application Name");
			Assert.AreEqual("Bloodhound.IntegrationTests", applicationName.Value);
		}

		[TestMethod]
		public void StartWithElapsedTime()
		{
			var channel = new Mock<IDataChannel>();
			var provider = new RepositoryProvider(TestHelper.Directory, TimeSpan.FromDays(1), 10000);
			var actual = new List<Event>();
			var elapsedTime = TimeSpan.FromTicks(5465421643);

			channel.Setup(x => x.WriteEvents(It.IsAny<IEnumerable<Event>>()))
				.Callback<IEnumerable<Event>>(x => actual.AddRange(x));

			using (TestHelper.CreateDataContext())
			{
				using (var tracker = new Tracker(channel.Object, provider))
				{
					tracker.Start(elapsedTime);
				}

				Assert.AreEqual(1, actual.Count);
				Assert.AreEqual(elapsedTime, actual[0].ElapsedTime);
				Assert.AreEqual(elapsedTime, actual[0].CompletedOn - actual[0].CreatedOn);
			}

			var applicationName = actual[0].Values.First(x => x.Name == "Application Name");
			Assert.AreEqual("Bloodhound.IntegrationTests", applicationName.Value);
		}

		[TestMethod]
		public void StartWithElapsedTimeAndCustomValues()
		{
			var channel = new Mock<IDataChannel>();
			var provider = new RepositoryProvider(TestHelper.Directory, TimeSpan.FromDays(1), 10000);
			var actual = new List<Event>();
			var elapsedTime = TimeSpan.FromTicks(413124564);

			channel.Setup(x => x.WriteEvents(It.IsAny<IEnumerable<Event>>()))
				.Callback<IEnumerable<Event>>(x => actual.AddRange(x));

			using (TestHelper.CreateDataContext())
			{
				using (var tracker = new Tracker(channel.Object, provider))
				{
					tracker.Start(elapsedTime, new EventValue("Foo", "Bar"));
				}

				Assert.AreEqual(1, actual.Count);
				Assert.AreEqual(elapsedTime, actual[0].ElapsedTime);
				Assert.AreEqual(18, actual[0].Values.Count);

				var eventValue = actual[0].Values.FirstOrDefault(x => x.Name == "Foo");
				Assert.IsNotNull(eventValue);
				Assert.AreEqual("Foo", eventValue.Name);
				Assert.AreEqual("Bar", eventValue.Value);
			}

			var applicationName = actual[0].Values.First(x => x.Name == "Application Name");
			Assert.AreEqual("Bloodhound.IntegrationTests", applicationName.Value);
		}

		[TestMethod]
		public void TimedEvent()
		{
			var provider = new RepositoryProvider(TestHelper.Directory, TimeSpan.FromDays(1), 10000);
			var channel = new Mock<IDataChannel>();
			var actual = new List<Event>();

			channel.Setup(x => x.WriteEvents(It.IsAny<IEnumerable<Event>>()))
				.Callback<IEnumerable<Event>>(x => actual.AddRange(x));

			using (var tracker = Tracker.Start(channel.Object, provider))
			{
				var timedEvent = tracker.StartEvent("TimedEvent");
				Thread.Sleep(500);
				timedEvent.Complete();
			}

			Assert.IsTrue(actual.Wait(x => x.Count == 2));

			Assert.AreEqual(2, actual.Count);
			Assert.AreEqual("Session", actual[0].Name);
			Assert.AreEqual("TimedEvent", actual[1].Name);
			Assert.IsTrue(actual[1].ElapsedTime.TotalMilliseconds > 500);
			Assert.IsTrue(actual[1].ElapsedTime.TotalMilliseconds < 550);
		}

		[TestMethod]
		public void TimedEventCompleteShouldOnlyFireOnce()
		{
			var channel = new Mock<IDataChannel>();
			var provider = new RepositoryProvider(TestHelper.Directory, TimeSpan.FromDays(1), 10000);
			var count = 0;

			channel.Setup(x => x.WriteEvents(It.IsAny<IEnumerable<Event>>()))
				.Callback<IEnumerable<Event>>(events => count += events.Count());

			using (var tracker = Tracker.Start(channel.Object, provider))
			{
				var timedEvent = tracker.StartEvent("TimedEvent");
				timedEvent.Complete();

				// Change unique ID so the repository would create a new event.
				// Should not be added because complete should only fire once.
				timedEvent.UniqueId = Guid.NewGuid();
				timedEvent.Complete();
				timedEvent.UniqueId = Guid.NewGuid();
				timedEvent.Complete();
			}

			// Should only be two events.
			Assert.AreEqual(2, count);
		}

		[TestMethod]
		public void TimedEventEarlyComplete()
		{
			var provider = new RepositoryProvider(TestHelper.Directory, TimeSpan.FromDays(1), 10000);
			var channel = new Mock<IDataChannel>();
			var actual = new List<Event>();

			channel.Setup(x => x.WriteEvents(It.IsAny<IEnumerable<Event>>()))
				.Callback<IEnumerable<Event>>(x => actual.AddRange(x));

			using (var tracker = Tracker.Start(channel.Object, provider))
			{
				var timedEvent = tracker.StartEvent("TimedEvent");
				Thread.Sleep(500);
				timedEvent.Complete();
			}

			Assert.IsTrue(actual.Wait(x => x.Count == 2));

			Assert.AreEqual(2, actual.Count);
			Assert.AreEqual("Session", actual[0].Name);
			Assert.AreEqual("TimedEvent", actual[1].Name);
			Assert.IsTrue(actual[1].ElapsedTime.TotalMilliseconds > 500);
			Assert.IsTrue(actual[1].ElapsedTime.TotalMilliseconds < 550);
		}

		[TestMethod]
		public void TimedEventWithChild()
		{
			var channel = new WebDataChannel("http://localhost");
			var provider = new RepositoryProvider(TestHelper.Directory, TimeSpan.FromDays(1), 10000);

			using (var context = TestHelper.CreateDataContext())
			{
				using (var tracker = Tracker.Start(channel, provider))
				{
					var timedEvent = tracker.StartEvent("TimedEvent");
					timedEvent.AddEvent("ChildEvent");
					Thread.Sleep(500);
					timedEvent.Complete();
				}

				context.Wait(x => x.Events.Count() == 3, 5000);

				var actual = context.Events.ToList();
				Assert.AreEqual(3, actual.Count);
				Assert.AreEqual("Session", actual[0].Name);
				Assert.AreEqual("TimedEvent", actual[1].Name);
				Assert.IsTrue(actual[1].ElapsedTime.TotalMilliseconds > 500);
				Assert.IsTrue(actual[1].ElapsedTime.TotalMilliseconds < 550);
				Assert.AreEqual("ChildEvent", actual[2].Name);
				Assert.AreEqual(0, actual[2].ElapsedTime.TotalMilliseconds);
			}
		}

		[TestMethod]
		public void TimedEventWithTimedChild()
		{
			var channel = new WebDataChannel("http://localhost");
			var provider = new RepositoryProvider(TestHelper.Directory, TimeSpan.FromDays(1), 10000);

			using (var context = TestHelper.CreateDataContext())
			{
				using (var tracker = Tracker.Start(channel, provider))
				{
					var timedEvent = tracker.StartEvent("TimedEvent");
					var childTimedEvent = timedEvent.StartEvent("ChildEvent");
					Thread.Sleep(250);
					childTimedEvent.Complete();
					Thread.Sleep(250);
					timedEvent.Complete();
				}

				Assert.IsTrue(context.Wait(x => x.Events.Count() == 3, 5000));

				var actual = context.Events.ToList();
				Assert.AreEqual(3, actual.Count);
				Assert.AreEqual("Session", actual[0].Name);
				Assert.AreEqual("TimedEvent", actual[1].Name);
				Assert.IsTrue(actual[1].ElapsedTime.TotalMilliseconds > 500);
				Assert.IsTrue(actual[1].ElapsedTime.TotalMilliseconds < 550);
				Assert.AreEqual("ChildEvent", actual[2].Name);
				Assert.IsTrue(actual[2].ElapsedTime.TotalMilliseconds > 250);
				Assert.IsTrue(actual[2].ElapsedTime.TotalMilliseconds < 300);
			}
		}

		[TestMethod]
		public void WithOldSessions()
		{
			var provider = new RepositoryProvider(TestHelper.Directory, TimeSpan.FromDays(1), 10000);
			var channel = new Mock<IDataChannel>();
			var actual = new List<Event>();

			channel.Setup(x => x.WriteEvents(It.IsAny<IEnumerable<Event>>()))
				.Callback<IEnumerable<Event>>(x => actual.AddRange(x));

			TestHelper.Directory.SafeCreate();
			var session = TestHelper.CreateSession();
			using (var repository = provider.OpenRepository(session.SessionId.ToString()))
			{
				repository.WriteAndSave(session);
			}

			using (Tracker.Start(channel.Object, provider))
			{
				Thread.Sleep(500);
			}

			Assert.AreEqual(2, actual.Count);
			session.Id = actual[1].Id;
			TestHelper.AreEqual(session, actual[1]);
		}

		[TestMethod]
		public void WithOldSessionsWithConnectionIssues()
		{
			var channel = new Mock<IDataChannel>();
			var provider = new RepositoryProvider(TestHelper.Directory, TimeSpan.FromDays(1), 10000);
			TestHelper.Directory.SafeCreate();

			var expected = new List<Event>();
			var session = TestHelper.CreateSession();
			using (var repository = provider.OpenRepository(session.SessionId.ToString()))
			{
				repository.WriteAndSave(session);
			}

			channel.Setup(x => x.WriteEvents(It.IsAny<IEnumerable<Event>>()))
				.Callback<IEnumerable<Event>>(events =>
				{
					var list = events.ToList();
					if (list.Any(x => x.UniqueId == session.SessionId))
					{
						throw new Exception("Unable to connect to the remote server");
					}

					expected.AddRange(list);
				});

			using (Tracker.Start(channel.Object, provider))
			{
				Thread.Sleep(1000);
			}

			Assert.AreEqual(1, expected.Count);
			Assert.AreEqual(EventType.Session, expected[0].Type);
		}

		[TestMethod]
		public void WithOldSessionsWithIssues()
		{
			var channel = new Mock<IDataChannel>();
			var provider = new RepositoryProvider(TestHelper.Directory, TimeSpan.FromDays(1), 10000);
			TestHelper.Directory.SafeCreate();

			var expected = new List<Event>();
			var session = TestHelper.CreateSession();
			using (var repository = provider.OpenRepository(session.SessionId.ToString()))
			{
				repository.WriteAndSave(session);
			}

			channel.Setup(x => x.WriteEvents(It.IsAny<IEnumerable<Event>>()))
				.Callback<IEnumerable<Event>>(events =>
				{
					var list = events.ToList();
					if (list.Any(x => x.UniqueId == session.SessionId))
					{
						throw new Exception("Boom");
					}

					expected.AddRange(list);
				});

			using (Tracker.Start(channel.Object, provider))
			{
				Thread.Sleep(1000);
			}

			Assert.AreEqual(2, expected.Count);
			Assert.AreEqual(EventType.Exception, expected[1].Type);
			Assert.AreEqual("Exception", expected[1].Name);
			Assert.AreEqual("Boom", expected[1].Values.First(x => x.Name == "Message").Value);
		}

		[TestMethod]
		public void WithoutStart()
		{
			var channel = new Mock<IDataChannel>();
			var provider = new RepositoryProvider(TestHelper.Directory, TimeSpan.FromDays(1), 10000);

			using (TestHelper.CreateDataContext())
			{
				using (var tracker = new Tracker(channel.Object, provider))
				{
					var myTracker = tracker;
					TestHelper.ExpectedException<InvalidOperationException>(() => myTracker.AddEvent("Event"), "You must first start the tracker before using it.");
				}
			}
		}

		#endregion
	}
}