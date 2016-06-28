#region References

using System;

#endregion

namespace Bloodhound.WebSite.Models.Views
{
	public class SessionView
	{
		#region Properties

		public string ApplicationVersion { get; set; }
		public DateTime CreatedOn { get; set; }
		public int Id { get; set; }
		public string Location { get; set; }
		public string MachineNameAndUser { get; set; }

		public string MachineSpecifications { get; set; }
		public string Runtime { get; set; }

		#endregion
	}
}