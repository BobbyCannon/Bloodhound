#region References

using System;
using System.Linq;
using System.Management.Automation;
using Bloodhound.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestR.PowerShell;

#endregion

namespace Bloodhound.IntegrationTests.Models
{
	[TestClass]
	[Cmdlet(VerbsDiagnostic.Test, "Event")]
	public class EventTests : TestCmdlet
	{
		#region Methods

		[TestMethod]
		public void AddEventWithEmptyGuid()
		{
			TestHelper.ExpectedException<ArgumentException>(() => new Event(Guid.Empty, "Foo"), "The session ID cannot be empty.");
		}

		[TestMethod]
		public void AddEventWithEmptyString()
		{
			TestHelper.ExpectedException<ArgumentException>(() => new Event(Guid.NewGuid(), ""), "The name is required.");
		}

		[TestMethod]
		public void AddEventWithNull()
		{
			TestHelper.ExpectedException<ArgumentNullException>(() => new Event(Guid.NewGuid(), null), "The name cannot be null.");
		}

		[TestMethod]
		public void AddEventWithWhiteSpaceString()
		{
			TestHelper.ExpectedException<ArgumentException>(() => new Event(Guid.NewGuid(), " "), "The name is required.");
		}

		[TestMethod]
		public void CreateEvent()
		{
			var sessionId = Guid.NewGuid();

			try
			{
				var innerException = new Exception("Inner message...");
				throw new InvalidOperationException("Not supposed to happen...", innerException);
			}
			catch (Exception ex)
			{
				var actual = Event.FromException(sessionId, ex);
				Assert.AreEqual("InvalidOperationException", actual.Name);
				Assert.AreEqual(@"Not supposed to happen...", actual.Values.First(x => x.Name == "Message").Value);
				Assert.AreEqual(@"   at Bloodhound.IntegrationTests.Models.EventTests.CreateEvent() in C:\Workspaces\GitHub\Bloodhound\Bloodhound.IntegrationTests\Models\EventTests.cs:line 52", actual.Values.First(x => x.Name == "Stack Trace").Value);
				Assert.AreEqual(1, actual.Children.Count);
				Assert.AreEqual("Exception", actual.Children[0].Name);
				Assert.AreEqual("Inner message...", actual.Children[0].Values.First(x => x.Name == "Message").Value);
				Assert.AreEqual("", actual.Children[0].Values.First(x => x.Name == "Stack Trace").Value);
			}
		}

		[TestMethod]
		public void CreateEventWithElapsedTime()
		{
			var id = Guid.NewGuid();
			var elapsedTime = TimeSpan.FromTicks(456132468796543);
			var actual = new Event(id, "Test", elapsedTime, new EventValue("Foo", "Bar"));
			Assert.AreEqual(id, actual.SessionId);
			Assert.AreNotEqual(id, actual.UniqueId);
			Assert.AreNotEqual(Guid.Empty, actual.UniqueId);
			Assert.AreEqual(elapsedTime, actual.ElapsedTime);
			Assert.AreEqual(elapsedTime, actual.CompletedOn - actual.CreatedOn);
		}

		[TestMethod]
		public void FromExceptionWithNullException()
		{
			TestHelper.ExpectedException<ArgumentNullException>(() => Event.FromException(Guid.NewGuid(), null), "The exception cannot be null.");
		}

		#endregion
	}
}