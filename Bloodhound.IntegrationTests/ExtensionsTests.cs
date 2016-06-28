#region References

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Management.Automation;
using Bloodhound.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestR.PowerShell;

#endregion

namespace Bloodhound.IntegrationTests
{
	[TestClass]
	[Cmdlet(VerbsDiagnostic.Test, "Extensions")]
	public class ExtensionsTests : TestCmdlet
	{
		#region Methods

		[TestMethod]
		public void AddOrUpdateExistingItem()
		{
			var collection = new Collection<EventValue>(new[] { new EventValue("Foo", "Bar") });
			collection.AddOrUpdate("Foo", "Bar2");
			Assert.AreEqual(1, collection.Count);
			Assert.AreEqual("Foo", collection[0].Name);
			Assert.AreEqual("Bar2", collection[0].Value);
		}

		[TestMethod]
		public void AddOrUpdateNewItem()
		{
			var collection = new Collection<EventValue>();
			collection.AddOrUpdate("Foo", "Bar");
			Assert.AreEqual(1, collection.Count);
			Assert.AreEqual("Foo", collection[0].Name);
			Assert.AreEqual("Bar", collection[0].Value);
		}

		[TestMethod]
		public void AddOrUpdateRangeExistingItem()
		{
			var collection = new Collection<EventValue>(new[] { new EventValue("Foo", "Bar") });
			collection.AddOrUpdateRange(new EventValue("Foo", "Bar2"));
			Assert.AreEqual(1, collection.Count);
			Assert.AreEqual("Foo", collection[0].Name);
			Assert.AreEqual("Bar2", collection[0].Value);
		}

		[TestMethod]
		public void FromFilter()
		{
			var date = DateTime.Parse("2015/10/19");
			Assert.AreEqual(DateTime.Parse("2015/10/19"), date.FromFormat(""));
			Assert.AreEqual(DateTime.Parse("2015/10/1"), date.FromFormat("MM/yy"));
			Assert.AreEqual(DateTime.Parse("2015/10/1"), date.FromFormat("MM/yyyy"));
			Assert.AreEqual(DateTime.Parse("2015/10/1"), date.FromFormat("yy/MM"));
			Assert.AreEqual(DateTime.Parse("2015/10/1"), date.FromFormat("yyyy/MM"));
			Assert.AreEqual(DateTime.Parse("2015/1/1"), date.FromFormat("yy"));
			Assert.AreEqual(DateTime.Parse("2015/1/1"), date.FromFormat("yyyy"));
		}

		[TestMethod]
		public void Increment()
		{
			var date = DateTime.Parse("2015/10/19");
			Assert.AreEqual(DateTime.Parse("2015/10/20"), date.Increment(""));
			Assert.AreEqual(DateTime.Parse("2015/11/19"), date.Increment("MM/yy"));
			Assert.AreEqual(DateTime.Parse("2015/11/19"), date.Increment("MM/yyyy"));
			Assert.AreEqual(DateTime.Parse("2015/11/19"), date.Increment("yy/MM"));
			Assert.AreEqual(DateTime.Parse("2015/11/19"), date.Increment("yyyy/MM"));
			Assert.AreEqual(DateTime.Parse("2016/10/19"), date.Increment("yy"));
			Assert.AreEqual(DateTime.Parse("2016/10/19"), date.Increment("yyyy"));
		}

		[TestMethod]
		public void WaitShouldExpire()
		{
			var watch = Stopwatch.StartNew();
			var actual = watch.Wait(x => x.Elapsed.TotalMilliseconds >= 1000, 100, 10);
			Assert.IsFalse(actual);
		}

		#endregion
	}
}