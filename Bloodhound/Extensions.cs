#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Bloodhound.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Speedy;

#endregion

namespace Bloodhound
{
	/// <summary>
	/// Represents extensions for classes used in bloodhound.
	/// </summary>
	public static class Extensions
	{
		#region Fields

		/// <summary>
		/// Gets the default settings for the JSON serializer.
		/// </summary>
		public static readonly JsonSerializerSettings DefaultSerializerSettings;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates the extensions.
		/// </summary>
		static Extensions()
		{
			// Setup the JSON formatter.
			DefaultSerializerSettings = GetSerializerSettings();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Adds or updates the item in the collection.
		/// </summary>
		/// <param name="collection"> The collection to be updated. </param>
		/// <param name="key"> The key of the item. </param>
		/// <param name="value"> The value of the item. </param>
		public static void AddOrUpdate(this ICollection<EventValue> collection, string key, object value)
		{
			var foundItem = collection.FirstOrDefault(x => x.Name == key);
			if (foundItem != null)
			{
				foundItem.Value = value.ToString();
				return;
			}

			collection.Add(new EventValue(key, value));
		}

		/// <summary>
		/// Adds or updates the item in the collection.
		/// </summary>
		/// <param name="collection"> The collection to be updated. </param>
		/// <param name="items"> The items to be added or updated. </param>
		public static void AddOrUpdateRange(this ICollection<EventValue> collection, params EventValue[] items)
		{
			foreach (var item in items)
			{
				collection.AddOrUpdate(item);
			}
		}

		/// <summary>
		/// Gets a new date time from the provided value using the format provided.
		/// </summary>
		/// <param name="date"> The date time to convert. </param>
		/// <param name="format"> The format to use during conversion. </param>
		/// <returns> The converted date time. </returns>
		public static DateTime FromFormat(this DateTime date, string format)
		{
			switch (format)
			{
				case "yy":
				case "yyyy":
					return new DateTime(date.Year, 1, 1);

				case "MM/yy":
				case "MM/yyyy":
				case "yy/MM":
				case "yyyy/MM":
					return new DateTime(date.Year, date.Month, 1);

				default:
					return date;
			}
		}

		/// <summary>
		/// Converts an HTML value (#000000) to an RGBA value (rgba(0,0,0,0.5)).
		/// </summary>
		/// <param name="htmlColor"> The HTML color value to convert. </param>
		/// <param name="alpha"> The alpha value for the RGBA value. </param>
		/// <returns> The RGBA value of the HTML color. </returns>
		public static string FromHtmlToRgb(this string htmlColor, decimal alpha)
		{
			var r = int.Parse(htmlColor.Substring(1, 2), NumberStyles.AllowHexSpecifier);
			var g = int.Parse(htmlColor.Substring(3, 2), NumberStyles.AllowHexSpecifier);
			var b = int.Parse(htmlColor.Substring(5, 2), NumberStyles.AllowHexSpecifier);
			return $"rgba({r},{g},{b},{alpha})";
		}

		/// <summary>
		/// Deserialize an object from a string.
		/// </summary>
		/// <typeparam name="T"> The type to convert to. </typeparam>
		/// <param name="data"> The serialized data for the object. </param>
		/// <returns> The object value. </returns>
		public static T FromJson<T>(this string data)
		{
			return JsonConvert.DeserializeObject<T>(data, DefaultSerializerSettings);
		}

		/// <summary>
		/// Hashes the value using SHA256.
		/// </summary>
		/// <param name="value"> The value to hash. </param>
		/// <param name="additional"> An optional additional value to include. </param>
		public static string Hash(this string value, string additional = null)
		{
			SHA256 sha = new SHA256Managed();
			var hash = sha.ComputeHash(Encoding.ASCII.GetBytes(additional != null ? value + additional : value));

			var stringBuilder = new StringBuilder();
			foreach (var b in hash)
			{
				stringBuilder.AppendFormat("{0:x2}", b);
			}

			return stringBuilder.ToString();
		}

		/// <summary>
		/// Increment a date time using the provided format.
		/// </summary>
		/// <param name="dateTime"> The date time to increment. </param>
		/// <param name="format"> The format to use for incrementing. </param>
		/// <returns> The incremented date time. </returns>
		public static DateTime Increment(this DateTime dateTime, string format)
		{
			switch (format)
			{
				case "yy":
				case "yyyy":
					return dateTime.AddYears(1);

				case "MM/yy":
				case "MM/yyyy":
				case "yy/MM":
				case "yyyy/MM":
					return dateTime.AddMonths(1);

				default:
					return dateTime.AddDays(1);
			}
		}

		/// <summary>
		/// Creates a repository and writes the first session event.
		/// </summary>
		/// <param name="provider"> The provider to start a new repository on. </param>
		/// <param name="session"> The session event to start the repository with. </param>
		/// <returns> The repository containing the session event. </returns>
		public static IRepository OpenRepository(this IRepositoryProvider provider, Event session)
		{
			var repository = provider.OpenRepository(session.SessionId.ToString());
			repository.WriteAndSave(session);
			return repository;
		}

		/// <summary>
		/// Gets a detailed string of the provided exception.
		/// </summary>
		/// <param name="ex"> The exception to get the details for. </param>
		/// <returns> The details of the exception. </returns>
		public static string ToDetailedString(this Exception ex)
		{
			var builder = new StringBuilder();
			AddExceptionToBuilder(builder, ex);
			return builder.ToString();
		}

		/// <summary>
		/// Converts the item to JSON.
		/// </summary>
		/// <typeparam name="T"> The type of the item to convert. </typeparam>
		/// <param name="item"> The item to convert. </param>
		/// <param name="camelCase"> The flag to determine if we should use camel case or not. </param>
		/// <param name="indented"> The flag to determine if the JSON should be indented or not. </param>
		/// <returns> The JSON value of the item. </returns>
		public static string ToJson<T>(this T item, bool camelCase = true, bool indented = false)
		{
			return JsonConvert.SerializeObject(item, indented ? Formatting.Indented : Formatting.None, GetSerializerSettings(camelCase));
		}

		/// <summary>
		/// Runs the action until the action returns true or the timeout is reached. Will delay in between actions of the provided
		/// time.
		/// </summary>
		/// <param name="input"> The item to wait on. </param>
		/// <param name="action"> The action to call. </param>
		/// <param name="timeout"> The timeout to attempt the action. This value is in milliseconds. </param>
		/// <param name="delay"> The delay in between actions. This value is in milliseconds. </param>
		/// <returns> Returns true of the call completed successfully or false if it timed out. </returns>
		public static bool Wait<T>(this T input, Func<T, bool> action, double timeout = 1000, int delay = 50)
		{
			var watch = Stopwatch.StartNew();
			var watchTimeout = TimeSpan.FromMilliseconds(timeout);
			var result = false;

			while (!result)
			{
				if (watch.Elapsed > watchTimeout)
				{
					return false;
				}

				result = action(input);
				if (!result)
				{
					Thread.Sleep(delay);
				}
			}

			return true;
		}

		/// <summary>
		/// Write the event to the repository and save it.
		/// </summary>
		/// <param name="repository"> The repository to write to. </param>
		/// <param name="value"> The event to be written to the repository. </param>
		public static void WriteAndSave(this IRepository repository, Event value)
		{
			repository.Write(value.UniqueId.ToString(), value.ToJson());
			repository.Save();
		}

		internal static Dictionary<string, decimal> FillDates(this IDictionary<string, decimal> input, string format, DateTime startDate, DateTime endDate)
		{
			var currentDate = startDate.FromFormat(format);
			var response = new Dictionary<string, decimal>();

			while (currentDate < endDate)
			{
				var key = currentDate.ToString(format);
				response.Add(key, input.ContainsKey(key) ? input[key] : 0);
				currentDate = currentDate.Increment(format);
			}

			return response;
		}

		private static void AddExceptionToBuilder(StringBuilder builder, Exception ex)
		{
			builder.Append(builder.Length > 0 ? "\r\n" + ex.Message : ex.Message);

			if (ex.InnerException != null)
			{
				AddExceptionToBuilder(builder, ex.InnerException);
			}
		}

		private static void AddOrUpdate(this ICollection<EventValue> collection, EventValue eventValue)
		{
			var foundItem = collection.FirstOrDefault(x => x.Name == eventValue.Name);
			if (foundItem != null)
			{
				foundItem.Value = eventValue.Value;
				return;
			}

			collection.Add(eventValue);
		}

		private static JsonSerializerSettings GetSerializerSettings(bool camelCase = true)
		{
			var response = new JsonSerializerSettings();
			response.Converters.Add(new IsoDateTimeConverter());
			response.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
			response.PreserveReferencesHandling = PreserveReferencesHandling.Objects;

			if (camelCase)
			{
				response.Converters.Add(new StringEnumConverter { CamelCaseText = true });
				response.ContractResolver = new CamelCasePropertyNamesContractResolver();
			}

			return response;
		}

		#endregion
	}
}