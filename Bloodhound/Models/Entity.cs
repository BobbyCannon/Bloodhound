#region References

using System;

#endregion

namespace Bloodhound.Models
{
	/// <summary>
	/// Represents the base entity.
	/// </summary>
	[Serializable]
	public class Entity
	{
		#region Properties

		/// <summary>
		/// Gets or set the ID.
		/// </summary>
		public int Id { get; set; }

		#endregion
	}
}