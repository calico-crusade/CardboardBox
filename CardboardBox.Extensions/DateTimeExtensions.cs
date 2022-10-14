namespace CardboardBox
{
	public static class DateTimeExtensions
	{
		private const DayOfWeek START_OF_WEEK = DayOfWeek.Sunday;

		/// <summary>
		/// Gets the start of the given date's hour
		/// </summary>
		/// <param name="date">The target date</param>
		/// <returns>The top of the hour of the target date</returns>
		public static DateTime StartOfHour(this DateTime date)
		{
			return new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0);
		}

		/// <summary>
		/// Gets the last second of the given date's current hour
		/// </summary>
		/// <param name="date">The target date</param>
		/// <returns>The last second of the target date</returns>
		public static DateTime EndOfHour(this DateTime date)
		{
			return date.StartOfHour().AddHours(1);
		}

		/// <summary>
		/// Gets the first second of the given date's current day
		/// </summary>
		/// <param name="date">The target date</param>
		/// <returns>The first second of the given date's current day</returns>
		public static DateTime StartOfDay(this DateTime date)
		{
			return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
		}

		/// <summary>
		/// Gets the last second of the given date's current day
		/// </summary>
		/// <param name="date">The target date</param>
		/// <returns>The last second of the given date's current day</returns>
		public static DateTime EndOfDay(this DateTime date)
		{
			return date.AddDays(1).StartOfDay().AddSeconds(-1);
		}

		/// <summary>
		/// Gets the start of the target date's current week
		/// </summary>
		/// <param name="date">The target date</param>
		/// <param name="startOfWeek">The day that marks the start of a week. Defaults to: <see cref="START_OF_WEEK"/></param>
		/// <returns>The start of the given date's current week</returns>
		public static DateTime StartOfWeek(this DateTime date, DayOfWeek startOfWeek = START_OF_WEEK)
		{
			int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
			return date.AddDays(-1 * diff).Date.StartOfDay();
		}

		/// <summary>
		/// Gets the end of the target date's current week
		/// </summary>
		/// <param name="date">The target date</param>
		/// <param name="startOfWeek">The day that marks the start of a week. Defaults to: <see cref="START_OF_WEEK"/></param>
		/// <returns>The end of the given date's current week</returns>
		public static DateTime EndOfWeek(this DateTime date, DayOfWeek startOfWeek = START_OF_WEEK)
		{
			return date.StartOfWeek(startOfWeek).AddDays(7).EndOfDay();
		}

		/// <summary>
		/// Gets the start of the target date's current month
		/// </summary>
		/// <param name="date">The target date</param>
		/// <returns>The start of the given date's current month</returns>
		public static DateTime StartOfMonth(this DateTime date)
		{
			return new DateTime(date.Year, date.Month, 1);
		}

		/// <summary>
		/// Gets the end of the target date's current month
		/// </summary>
		/// <param name="date">The target date</param>
		/// <returns>The end of the given date's current month</returns>
		public static DateTime EndOfMonth(this DateTime date)
		{
			return date.StartOfMonth()
				.AddMonths(1)
				.AddDays(-1);
		}

		/// <summary>
		/// Converts the given date to an epoch
		/// </summary>
		/// <param name="time">The target date</param>
		/// <param name="miliseconds">Whether or not to use miliseconds or seconds (defaults to seconds)</param>
		/// <returns>The given date's epoch</returns>
		public static long ToEpoch(this DateTime time, bool miliseconds = false)
		{
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
			var dt = time.ToUniversalTime() - epoch;
			return (long)(miliseconds ? dt.TotalMilliseconds : dt.TotalSeconds);
		}

		/// <summary>
		/// Converts the given epoch to a <see cref="DateTime"/>
		/// </summary>
		/// <param name="time">The epoch (can be seconds or miliseconds)</param>
		/// <param name="miliseconds">Whether or not the epoch is in miliseconds (defaults to false)</param>
		/// <returns>The converted epoch</returns>
		public static DateTime FromEpoch(this long time, bool miliseconds = false)
		{
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
			return miliseconds ? epoch.AddMilliseconds(time) : epoch.AddSeconds(time);
		}

		/// <summary>
		/// Determines whether or not the target date is within (or equal) to the given range
		/// </summary>
		/// <param name="target">The target date</param>
		/// <param name="start">The start date</param>
		/// <param name="end">The end date</param>
		/// <returns>Whether or not the target date is within (or equal)</returns>
		public static bool IsEffective(this DateTime target, DateTime? start, DateTime? end)
		{
			if (start == null || end == null) return false;
			return target >= start && target <= end;
		}
	}
}
