#region References

using System;

#endregion

namespace Bloodhound.WebSite.Models.Views
{
	public class ExceptionView
	{
		#region Properties

		public DateTime CreatedOn { get; set; }

		public string Message { get; set; }

		public string StackTrace { get; set; }

		public string Type { get; set; }

		#endregion
	}
}