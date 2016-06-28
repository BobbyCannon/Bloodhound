#region References

using System;
using System.Reflection;

#endregion

namespace Bloodhound
{
	/// <summary>
	/// Represents global values for Bloodhound.
	/// </summary>
	public static class Global
	{
		#region Fields

		/// <summary>
		/// Gets the HTML colors used for chart data.
		/// </summary>
		public static readonly string[] HtmlColors =
		{
			"#0085dc", "#35aa47", "#d84a38", "#852b99", "#ff9000", "#8d3939", "#826b5b", "#565656"
		};

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates the global values.
		/// </summary>
		static Global()
		{
			Version = Assembly.GetExecutingAssembly().GetName().Version;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the version of Bloodhound.
		/// </summary>
		public static Version Version { get; private set; }

		#endregion
	}
}