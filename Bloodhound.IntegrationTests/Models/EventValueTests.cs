#region References

using System;
using System.Management.Automation;
using Bloodhound.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestR.PowerShell;

#endregion

namespace Bloodhound.IntegrationTests.Models
{
	[TestClass]
	[Cmdlet(VerbsDiagnostic.Test, "EventValue")]
	public class EventValueTests : TestCmdlet
	{
		#region Methods

		[TestMethod]
		public void AddEventValueWithEmptyName()
		{
			TestHelper.ExpectedException<ArgumentException>(() => new EventValue("", null), "The name is required.");
		}

		[TestMethod]
		public void AddEventValueWithNullName()
		{
			TestHelper.ExpectedException<ArgumentNullException>(() => new EventValue(null, null), "The name cannot be null.");
		}

		[TestMethod]
		public void AddEventValueWithNullValue()
		{
			TestHelper.ExpectedException<ArgumentException>(() => new EventValue("Foo", null), "The value cannot be null.");
		}

		[TestMethod]
		public void AddEventValueWithWhiteSpaceName()
		{
			TestHelper.ExpectedException<ArgumentException>(() => new EventValue(" ", null), "The name is required.");
		}

		#endregion
	}
}