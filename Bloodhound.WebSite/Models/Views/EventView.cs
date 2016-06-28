#region References

using System;

#endregion

namespace Bloodhound.WebSite.Models.Views
{
	public class EventView
	{
		#region Properties

		public DateTime CreatedOn { get; set; }
		public int Id { get; set; }
		public string Name { get; set; }
		public string Runtime { get; set; }
		public int SessionId { get; set; }
		public string Values { get; set; }

		#endregion
	}
}