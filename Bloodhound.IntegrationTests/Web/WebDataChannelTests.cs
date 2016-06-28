#region References

using System;
using System.Management.Automation;
using Bloodhound.Models;
using Bloodhound.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestR.PowerShell;

#endregion

namespace Bloodhound.IntegrationTests.Web
{
	[TestClass]
	[Cmdlet(VerbsDiagnostic.Test, "WebDataChannel")]
	public class WebDataChannelTests : TestCmdlet
	{
		#region Methods

		[TestMethod]
		public void AddEventWithInvalidSession()
		{
			var client = new WebDataChannel("http://localhost", 30000);
			var createdOn = DateTime.UtcNow;
			var expected = new Event
			{
				CreatedOn = createdOn,
				CompletedOn = createdOn,
				Name = "Event1",
				UniqueId = Guid.NewGuid()
			};
			try
			{
				client.WriteEvents(new[] { expected });
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToDetailedString());
				throw;
			}
		}

		#endregion
	}
}