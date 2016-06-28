#region References

using System;
using System.Windows.Forms;
using Bloodhound;
using Bloodhound.Models;
using Bloodhound.Web;
using Speedy;

#endregion

namespace Sample.WinForm
{
	internal static class Program
	{
		#region Fields

		private static Tracker _tracker;

		#endregion

		#region Methods

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main()
		{
			var client = new WebDataChannel("http://localhost", 5000);
			var provider = new RepositoryProvider(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));

			_tracker = Tracker.Start(client, provider);

			using (_tracker)
			{
				Application.AddMessageFilter(new MouseMessageFilter());
				MouseMessageFilter.Click += MouseEvent;

				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new MainForm(_tracker));
			}
		}

		private static void MouseEvent(object sender, MouseEventArgs e)
		{
			try
			{
				_tracker.AddEvent("Mouse Click", new EventValue("Point", e.Location));
			}
			catch (Exception ex)
			{
				// We will just publish all analytics exception because we do not want to negatively affect the user experience by 
				// tell them an error occurred on something not important to their business work flow.
				_tracker.AddException(ex);
			}
		}

		#endregion
	}
}