using System;
using System.Globalization;

namespace LocalCommons.Utilities
{
	/// <summary>
	/// Game dates are always Gregorian (e.g. 2026) in DB and on the wire.
	/// Thai OS / th-TH culture reports Buddhist years (2569 = 2026+543);
	/// normalize those back to Gregorian before timestamps or comparisons.
	/// </summary>
	public static class GameDate
	{
		public const int BuddhistOffset = 543;

		public static bool IsBuddhistYear(int year)
		{
			return year >= 2400;
		}

		public static DateTime ToGregorian(DateTime value)
		{
			if (!IsBuddhistYear(value.Year))
			{
				return value;
			}
			return value.AddYears(-BuddhistOffset);
		}

		public static DateTime NowGregorian()
		{
			return ToGregorian(DateTime.Now);
		}

		public static bool TryParse(string text, out DateTime gregorian)
		{
			gregorian = default(DateTime);
			if (string.IsNullOrWhiteSpace(text))
			{
				return false;
			}
			string t = text.Trim();
			DateTime raw;
			if (!DateTime.TryParseExact(t,
				new[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm", "yyyy-MM-dd", "yyyy/MM/dd HH:mm:ss", "yyyy/MM/dd" },
				CultureInfo.InvariantCulture, DateTimeStyles.None, out raw)
				&& !DateTime.TryParse(t, CultureInfo.InvariantCulture, DateTimeStyles.None, out raw))
			{
				return false;
			}
			gregorian = ToGregorian(raw);
			return true;
		}

		public static string Format(DateTime gregorian)
		{
			DateTime g = ToGregorian(gregorian);
			return g.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
		}

		public static long ToTimestamp(DateTime gregorian)
		{
			return Utility.ConvertToTimestamp(ToGregorian(gregorian));
		}
	}
}
