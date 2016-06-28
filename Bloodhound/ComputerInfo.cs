#region References

using System;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Principal;
using System.Windows.Forms;

#endregion

namespace Bloodhound
{
	/// <summary>
	/// Provides information for this computer.
	/// </summary>
	public static class ComputerInfo
	{
		#region Constants

		/// <summary>
		/// Number to divide bytes by to get Gigabyte value.
		/// </summary>
		private const int GigabyteDivisor = 1073741824;

		#endregion

		#region Constructors

		static ComputerInfo()
		{
			FrameworkVersion = Environment.Version.ToString();
			MachineName = Environment.MachineName;
			MachineId = new SecurityIdentifier((byte[]) new DirectoryEntry($"WinNT://{Environment.MachineName},Computer")
				.Children.Cast<DirectoryEntry>().First().InvokeGet("objectSID"), 0).AccountDomainSid.Value.Hash(Environment.MachineName);

			Memory = (uint) new ManagementObjectSearcher("SELECT Capacity FROM Win32_PhysicalMemory")
				.Get()
				.OfType<ManagementObject>()
				.Select(x => (int) (Convert.ToUInt64(x.GetPropertyValue("Capacity")) / GigabyteDivisor))
				.Sum();

			NumberOfProcessors = Environment.ProcessorCount;

			OperatingSystemName = (new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem")
				.Get()
				.OfType<ManagementObject>()
				.Select(x => x.GetPropertyValue("Caption"))
				.FirstOrDefault()?.ToString() ?? "Unknown")
				.Replace("Microsoft", string.Empty)
				.Trim();

			OperatingSystemServicePack = Environment.OSVersion.ServicePack;
			OperatingSystemVersion = Environment.OSVersion.Version.ToString();
			OperatingSystemBitness = Environment.Is64BitOperatingSystem ? "64" : "32";

			var area = Screen.PrimaryScreen.Bounds;
			ScreenResolution = (area.Width + "x" + area.Height);

			var drive = DriveInfo.GetDrives().FirstOrDefault(x => x.Name.Equals(Path.GetPathRoot(Environment.SystemDirectory), StringComparison.OrdinalIgnoreCase));
			StorageTotalSpace = (drive?.TotalSize / GigabyteDivisor)?.ToString() ?? string.Empty;
			StorageAvailableSpace = (drive?.AvailableFreeSpace / GigabyteDivisor)?.ToString() ?? string.Empty;

			UserName = Environment.UserDomainName != Environment.MachineName ? Environment.UserDomainName + "\\" + Environment.UserName : Environment.UserName;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the version of the .NET framework.
		/// </summary>
		public static string FrameworkVersion { get; }

		/// <summary>
		/// The unique id machine. The ID is a HASH of the combination operating system ID and the machine name.
		/// </summary>
		public static string MachineId { get; set; }

		/// <summary>
		/// The name of computer.
		/// </summary>
		public static string MachineName { get; }

		/// <summary>
		/// The memory value in GB.
		/// </summary>
		public static uint Memory { get; }

		/// <summary>
		/// Get the number of processor.
		/// </summary>
		public static int NumberOfProcessors { get; }

		/// <summary>
		/// Gets the bitness of the operating system. Possible values are 64 or 32.
		/// </summary>
		public static string OperatingSystemBitness { get; }

		/// <summary>
		/// Gets the name of the operating system.
		/// </summary>
		public static string OperatingSystemName { get; }

		/// <summary>
		/// Gets the name of the operating system service pack.
		/// </summary>
		public static string OperatingSystemServicePack { get; }

		/// <summary>
		/// Gets the version of the operating system.
		/// </summary>
		public static string OperatingSystemVersion { get; }

		/// <summary>
		/// Gets the primary screen resolution.
		/// </summary>
		public static string ScreenResolution { get; }

		/// <summary>
		/// Gets the primary drive's total storage space in GB.
		/// </summary>
		public static string StorageAvailableSpace { get; }

		/// <summary>
		/// Gets the primary drive's total available storage space in GB.
		/// </summary>
		public static string StorageTotalSpace { get; }

		/// <summary>
		/// Gets the name of the user. If the machine is attached to a domain the user name will also contain the domain.
		/// </summary>
		public static string UserName { get; }

		#endregion
	}
}