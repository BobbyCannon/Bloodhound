namespace Bloodhound
{
	/// <summary>
	/// The different level of log values.
	/// </summary>
	public enum LogLevel
	{
		/// <summary>
		/// This level is for critical issues of the up most importance. 
		/// </summary>
		Critical,
		
		/// <summary>
		/// This level is for error issues.
		/// </summary>
		Error,
		
		/// <summary>
		/// This level is for warnings issues.
		/// </summary>
		Warning,

		/// <summary>
		/// This level is for general information.
		/// </summary>
		Information,

		/// <summary>
		/// This level is for debug information.
		/// </summary>
		Debug,

		/// <summary>
		/// This level is for verbose information.
		/// </summary>
		Verbose
	}
}