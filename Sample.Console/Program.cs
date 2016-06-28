#region References

using System;
using System.Threading;
using Bloodhound;
using Bloodhound.Data;
using Bloodhound.Web;
using Speedy;

#endregion

namespace Sample.Console
{
	internal class Program
	{
		#region Methods

		private static void Main(string[] args)
		{
			var localPath = @"C:\Users\bobby\AppData\Local\Temp\BobsToolbox";
			var client = new WebDataChannel("http://localhost", 30000);
			var provider = new RepositoryProvider(localPath, TimeSpan.FromDays(1), 30000);

			using (var tracker = new Tracker(client, provider))
			{
				tracker.Log += (x,y) => System.Console.WriteLine(x);
				tracker.Start();

				System.Console.WriteLine("Press any key");
				System.Console.ReadKey();
			}
		}

		#endregion
	}
}