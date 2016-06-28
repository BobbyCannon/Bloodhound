#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace Bloodhound.WebSite
{
	public static class Extensions
	{
		#region Methods

		public static string CleanMessage(this Exception ex)
		{
			var offset = ex.Message.IndexOf("\r\nParameter");
			return offset > 0 ? ex.Message.Substring(0, offset) : ex.Message;
		}

		public static string ConvertShortUnitToLongUnit(string unit, bool plural)
		{
			var data = new Dictionary<string, string>();
			data.Add("y", "year");
			data.Add("M", "month");
			data.Add("d", "day");
			data.Add("h", "hour");
			data.Add("m", "minute");
			data.Add("s", "second");

			if (data.ContainsKey(unit))
			{
				return plural ? data[unit] + "s" : data[unit];
			}

			return string.Empty;
		}

		public static IEnumerable<string> GetNonEntityPropertyNames(this Type item, IEnumerable<string> additionalNames = null, IEnumerable<string> exceptNames = null)
		{
			var response = item.GetProperties()
				.Where(p => !(p.CanRead ? p.GetGetMethod() : p.GetSetMethod()).IsVirtual)
				.Select(x => x.Name)
				.ToList();

			if (additionalNames != null)
			{
				response.AddRange(additionalNames);
			}

			return exceptNames == null ? response : response.Except(exceptNames);
		}

		/// <summary>
		/// Formats the time span into a human readable format.
		/// </summary>
		/// <param name="time"> The time span to convert. </param>
		/// <param name="format"> The format to use to generate the string. </param>
		/// <returns> A human readable format of the time span. </returns>
		public static string ToHumanReadableString(this TimeSpan time, string format = "yMdhms")
		{
			var thresholds = new SortedList<long, string>();
			var secondsPerMinute = 60;
			var secondsPerHour = 60 * secondsPerMinute;
			var secondsPerDay = 24 * secondsPerHour;

			thresholds.Add(secondsPerDay * 365, "y");
			thresholds.Add(secondsPerDay * 30, "M");
			thresholds.Add(secondsPerDay, "d");
			thresholds.Add(secondsPerHour, "h");
			thresholds.Add(secondsPerMinute, "m");
			thresholds.Add(1, "s");

			var builder = new StringBuilder();
			var secondsRemaining = time.TotalSeconds;
			var thresholdsHit = 0;

			for (var i = thresholds.Keys.Count - 1; i >= 0 && thresholdsHit < 2; i--)
			{
				var threshold = thresholds.Keys[i];
				var unit = thresholds[threshold];
				if (!(secondsRemaining >= threshold))
				{
					continue;
				}

				if (!format.Contains(unit))
				{
					continue;
				}

				var count = (int) (secondsRemaining / threshold);
				secondsRemaining %= threshold;

				var unitText = ConvertShortUnitToLongUnit(unit, count > 1);
				builder.AppendFormat(", {0} {1}", count, unitText);
				thresholdsHit++;
			}

			if (builder.Length <= 0)
			{
				var lastUnit = format.Last().ToString();
				return "less than a " + ConvertShortUnitToLongUnit(lastUnit, false);
			}

			var response = builder.Remove(0, 2).ToString();
			var lastIndex = response.LastIndexOf(", ");

			if (lastIndex > 0)
			{
				response = response.Remove(lastIndex, 2);
				response = response.Insert(lastIndex, " and ");
			}

			return response;
		}

		/// <summary>
		/// Formats the time in day.hours:minutes:seconds:milliseconds.
		/// </summary>
		/// <param name="time"> The time to format. </param>
		/// <returns> The time in the string format. </returns>
		public static string ToTimeString(this TimeSpan time)
		{
			return time.ToString(time.TotalDays >= 1 ? @"d\.hh\:mm\:ss\:fff" : @"hh\:mm\:ss\:fff");
		}

		#endregion
	}
}