#region References

using System;
using System.IO;
using System.Management.Automation;
using System.Reflection;
using Bloodhound.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestR.PowerShell;

#endregion

namespace Bloodhound.IntegrationTests.Website
{
	[TestClass]
	[Cmdlet(VerbsDiagnostic.Test, "OData")]
	public class ODataTests : TestCmdlet
	{
		#region Methods

		[TestMethod]
		public void Metadata()
		{
			var expected = ReadEmbeddedFile("Bloodhound.IntegrationTests.Website.ODataMetadata.txt");
			var actual = GetData("odata/$metadata#Events");
			Assert.AreEqual(expected, actual);
		}

		private string GetData(string location)
		{
			return WebClient.Get("http://localhost", location);
		}

		private static string ReadEmbeddedFile(string path)
		{
			var assembly = Assembly.GetExecutingAssembly();

			using (var stream = assembly.GetManifestResourceStream(path))
			{
				if (stream == null)
				{
					throw new Exception("Embedded file not found.");
				}

				using (var reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
		}

		#endregion
	}
}