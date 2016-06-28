#region References

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Http;
using Bloodhound.Data;
using Bloodhound.Models;
using KellermanSoftware.CompareNetObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

#endregion

namespace Bloodhound.IntegrationTests
{
	public static class TestHelper
	{
		#region Fields

		public static readonly DirectoryInfo Directory;

		private static readonly string _clearDatabaseSql;

		#endregion

		#region Constructors

		static TestHelper()
		{
			Directory = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\BloodhoundIntegrationTests");

			// Clears the database of all entities.
			_clearDatabaseSql = @"
				DELETE FROM EventValues;
				DELETE FROM Events;
				DELETE FROM WidgetFilters;
				DELETE FROM Widgets;
			";
		}

		#endregion

		#region Methods

		public static void AreEqual<T>(T expected, T actual, bool compareChildren = false)
		{
			var logic = new CompareLogic();
			logic.Config.CompareChildren = compareChildren;
			logic.Config.IgnoreObjectTypes = true;
			logic.Config.MaxDifferences = 100;

			var result = logic.Compare(expected, actual);
			Assert.IsTrue(result.AreEqual, result.DifferencesString);
		}

		public static void AreEqualUsingJson<T>(T expected, T actual)
		{
			var expectedLines = expected.ToJson(indented: true);
			var actualLines = actual.ToJson(indented: true);
			AreEqual(expectedLines, actualLines);
		}

		public static IDataContext CreateDataContext(bool clearDatabase = true)
		{
			var entityFrameworkRepository = new DataContext();
			if (clearDatabase)
			{
				entityFrameworkRepository.Database.ExecuteSqlCommand(_clearDatabaseSql);
			}

			return entityFrameworkRepository;
		}

		public static IEnumerable<IDataContextProvider> CreateDataContextProviders()
		{
			CreateDataContext().Dispose();
			var provider = new Mock<IDataContextProvider>();
			provider.Setup(x => x.GetContext()).Returns(() => CreateDataContext(false));
			return new[] { provider.Object };
		}

		public static Event CreateSession(DateTime? createdOn = null, DateTime? completedOn = null, params EventValue[] values)
		{
			var response = new Event
			{
				CreatedOn = createdOn ?? DateTime.UtcNow,
				Name = "Session",
				Type = EventType.Session,
				UniqueId = Guid.NewGuid(),
				Values = values.ToList()
			};

			response.CompletedOn = completedOn ?? response.CreatedOn;
			response.SessionId = response.UniqueId;

			response.Values.AddOrUpdate(".NET Framework Version", Environment.Version.ToString());
			response.Values.AddOrUpdate("Amount Of Memory", 32);
			response.Values.AddOrUpdate("Application Bitness", 32);
			response.Values.AddOrUpdate("Application Name", "Test Application");
			response.Values.AddOrUpdate("Application Version", "1.2.3.4");
			response.Values.AddOrUpdate("Bloodhound Version", Global.Version.ToString());
			response.Values.AddOrUpdate("IP Address", "127.0.0.1");
			response.Values.AddOrUpdate("Machine Name", Environment.MachineName);
			response.Values.AddOrUpdate("Number Of Processors", Environment.ProcessorCount);
			response.Values.AddOrUpdate("Operating System Bitness", 64);
			response.Values.AddOrUpdate("Operating System Name", "Windows 10 Pro");
			response.Values.AddOrUpdate("Operating System Service Pack", "Service Pack 1");
			response.Values.AddOrUpdate("Operating System Version", "6.2.9200.0");
			response.Values.AddOrUpdate("User Name", Environment.UserDomainName != Environment.MachineName ? Environment.UserDomainName + "\\" + Environment.UserName : Environment.UserName);

			return response;
		}

		public static void ExpectedException<T>(Action work, string errorMessage) where T : Exception
		{
			try
			{
				work();
			}
			catch (HttpResponseException ex)
			{
				// todo: Can we make this better? blah...
				var exception = ex.Response.Content.ToJson();
				Assert.IsTrue(exception.Contains(errorMessage));
				return;
			}
			catch (T ex)
			{
				if (!ex.ToDetailedString().Contains(errorMessage))
				{
					Assert.Fail("Expected <" + ex.Message + "> to contain <" + errorMessage + ">.");
				}
				return;
			}

			Assert.Fail("The expected exception was not thrown.");
		}

		/// <summary>
		/// Safely create a directory.
		/// </summary>
		/// <param name="directory"> The information of the directory to delete. </param>
		public static void SafeCreate(this DirectoryInfo directory)
		{
			directory.Refresh();
			if (!directory.Exists)
			{
				return;
			}

			directory.Create();

			directory.Wait(x =>
			{
				x.Refresh();
				return x.Exists;
			});
		}

		/// <summary>
		/// Safely delete a directory.
		/// </summary>
		/// <param name="directory"> The information of the directory to delete. </param>
		public static void SafeDelete(this DirectoryInfo directory)
		{
			directory.Refresh();
			if (!directory.Exists)
			{
				return;
			}

			directory.Delete(true);

			directory.Wait(x =>
			{
				x.Refresh();
				return !x.Exists;
			});
		}

		#endregion
	}
}