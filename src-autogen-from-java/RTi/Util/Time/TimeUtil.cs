﻿using System;
using System.Text;
using System.Threading;

// TimeUtil - date/time utility data and methods

/* NoticeStart

CDSS Common Java Library
CDSS Common Java Library is a part of Colorado's Decision Support Systems (CDSS)
Copyright (C) 1994-2019 Colorado Department of Natural Resources

CDSS Common Java Library is free software:  you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    CDSS Common Java Library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with CDSS Common Java Library.  If not, see <https://www.gnu.org/licenses/>.

NoticeEnd */

// ----------------------------------------------------------------------------
// TimeUtil - date/time utility data and methods
// ----------------------------------------------------------------------------
// History:
//
// 05 Jan 1998	Steven A. Malers,	Start getting documentation in order.
//		Riverside Technology,	Move TSDate.computeDayOfWeek to
//		inc.			TimeUtil.getCurrentDayOfWeek.
// 04 Mar 1998	SAM, RTi		Add overload for numDaysInMonths.
// 14 Mar 1998	SAM, RTi		Add javadoc.
// 08 May 1998  DLG, RTi		Added setLocalTimeZone functions.
// 15 May 1998  DLG, RTi		Fixed bug in isValidMinute, isValidHour
//					to restrict the range to 0-59 and 0-23
//					respectively.
// 21 Jun 1998	SAM, RTi		Add getDayAndMonthFromYearDay.
// 23 Jun 1998	SAM, RTi		Remove dependence on TS by adding time
//					zone information here.  Just copy the
//					TSTimeZone.getDefinedCode function to
//					here.
// 30 Jun 1998	SAM, RTi		Add getAbsoluteDay.
// 12 Jan 1999	SAM, RTi		Deprecate the old version of
//					getMonthAndDayFromDayOfYear and
//					replace with a more robust version.
// 28 Mar 2001	SAM, RTi		Change getAbsoluteDay() to
//					absoluteDay().  Add absoluteMinute().
//					Get rid of * imports.
// 08 Jul 2001	SAM, RTi		Add irrigation*FromCalendar() and
//					water*FromCalendar() methods.  Although
//					these are not generic to all code, they
//					are common enough in what RTi does to
//					put here.
// 22 Aug 2001	SAM, RTi		Add waitForFile().
// 2001-12-13	SAM, RTi		Add formatDateTime().  Transfer the
//					following from DateTime:
//						getDateTimeFromIndex()
//						getLocalTimeZone()
//						getNumIntervals()
//						isDateTime()
//					Use TZ and TZData instead of
//					TimeZoneData.
//					Verify that variables are set to null
//					when no longer used.  Change
//					getSystemTimeString() to use
//					formatDateTime().
//					Remove deprecated getTimeString(),
//					getAbsoluteMonth(), getDefinedCode().
//					Change setLocalTimeZone() to set the
//					same data used by
//					getLocalTimeZoneAbbr().
//					Add subtract() from DateTime.
//					Change %Z when dealing with dates to
//					use the TimeZone.getID() rather than
//					look up from a GMT offset (too
//					complicated).
// 2002-02-02	SAM, RTi		Change getDateFromIndex() to
//					addIntervals().
// 2002-05-21	SAM, RTi		Update formatDateTime() to pass through
//					unrecognized %X format specifiers so
//					that the string can be formatted in a
//					secondary formatter.
//					Add getDateTimeFormatSpecifiers().
// 2003-02-20	SAM, RTi		Fix numDaysInMonth(), which was not
//					checking for month 0 properly.
// 2003-06-03	SAM, RTi		Change diff() methods to be static.
// 2003-09-19	SAM, RTi		* formatDateTime() was not handling day
//					  of week abbreviations - was returning
//					  full day.
//					* Update formatDateTime() to use
//					  GregorianCalendar.
// 2003-10-10	SAM, RTi		* Add numDaysInMonth(DateTime).
// 2003-12-08	SAM, RTi		* Add dayOfYear(DateTime).
// 2005-02-24	SAM, RTi		Add highestPrecision() and
//					lowestPrecision().
// 2005-09-27	SAM, RTi		* Add max() and min() to simplify
//					  handling of DateTime comparisons.
//					* Change warning levels from 2 to 3 to
//					  facilitate use of the log file viewer.
// 2005-11-16	SAM, RTi		* Add
//					  convertCalendarMonthToCustomMonth().
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------
// EndHeader

namespace RTi.Util.Time
{
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using IOUtil = RTi.Util.IO.IOUtil;

	using  = long?;

	/// <summary>
	/// The TimeUtil class provides time utility methods for date/time data, independent
	/// of use in time series or other classes.  There is no "Time" or "Date" class
	/// other than what is supplied by Java or RTi's DateTime class (TSDate is being
	/// phased out).  Conventions used for all methods are:
	/// <para>
	/// Years are 4-digit.<br>
	/// Months are 1-12.<br>
	/// Days are 1-31.<br>
	/// Hours are 0-23.<br>
	/// Minutes are 0-59.<br>
	/// Seconds are 0-59.<br>
	/// HSeconds are 0-99.<br>
	/// </para>
	/// <para>
	/// </para>
	/// </summary>

	public abstract class TimeUtil
	{

	/// <summary>
	/// Datum for absolute day = days inclusive of Dec 31, 1799.
	/// This has been computed by looping through years 1-1799 adding numDaysInYear.
	/// This constant can be used when computing absolute days (e.g., to calculate the
	/// number of days in a period).
	/// </summary>
	public const int ABSOLUTE_DAY_DATUM = 657071;

	/// <summary>
	/// Blank values for DateTime parts, used to "mask out" unused information.
	/// If these are used as data values, then DateTime.DATE_FAST should be used to
	/// prevent exceptions for invalid values.  For example, it may be necessary to show a DateTime
	/// as a string parameter to represent a window in a year ("MM-DD").  In this case the other
	/// date/time components are not used, but are needed in the string to allow for proper parsing.
	/// </summary>
	public const int BLANK_YEAR = 9999;
	public const int BLANK_MONTH = 99;
	public const int BLANK_DAY = 99;
	public const int BLANK_HOUR = 99;
	public const int BLANK_MINUTE = 99;
	public const int BLANK_SECOND = 99;

	/// <summary>
	/// The following indicates how time zones are handled when getLocalTimeZone() is
	/// called (which is used when DateTime instances are created).  The default is
	/// LOOKUP_TIME_ZONE_ONCE, which results in the best performance when the time
	/// zone is not expected to change within a run.  However, if a time zone change
	/// will cause a problem, LOOKUP_TIME_ZONE_ALWAYS should be used (however, this
	/// results in slower performance).
	/// </summary>
	public const int LOOKUP_TIME_ZONE_ONCE = 1;

	/// <summary>
	/// The following indicates that for DateTime construction the local time zone is
	/// looked up each time a DateTime is created.  This should be considered when
	/// running a real-time application that runs continuously between time zone changes.
	/// </summary>
	public const int LOOKUP_TIME_ZONE_ALWAYS = 2;

	/// <summary>
	/// Abbreviations for months.
	/// </summary>
	public static readonly string[] MONTH_ABBREVIATIONS = new string[] {"Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"};

	/// <summary>
	/// Full names for months.
	/// </summary>
	public static readonly string[] MONTH_NAMES = new string[] {"January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"};

	/// <summary>
	/// Abbreviations for days.
	/// </summary>
	public static readonly string[] DAY_ABBREVIATIONS = new string[] {"Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"};

	/// <summary>
	/// Full names for months.
	/// </summary>
	public static readonly string[] DAY_NAMES = new string[] {"Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"};

	/// <summary>
	/// Days in months (non-leap year).
	/// </summary>
	public static readonly int[] MONTH_DAYS = new int[] {31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31};

	/// <summary>
	/// For a month, the number of days in the year passed on the first day of the
	/// month (non-leap year).
	/// </summary>
	public static readonly int[] MONTH_YEARDAYS = new int[] {0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334};

	// Static data shared in package (so DateTime can get to easily)...

	protected internal static TimeZone _local_time_zone = null;
	protected internal static string _local_time_zone_string = "";
	protected internal static bool _local_time_zone_retrieved = false;
	protected internal static int _time_zone_lookup_method = LOOKUP_TIME_ZONE_ONCE;

	/// <summary>
	/// Compute the absolute day.  This can be used for determining the difference between dates. </summary>
	/// <returns> The absolute day, with respect to Dec 31, 1799.  The datum may change
	/// in the future and should be used only in a dynamic fashion. </returns>
	/// <param name="year"> Year (4-digit). </param>
	/// <param name="month"> Month number (1-12). </param>
	/// <param name="day"> Day number (1-31). </param>
	public static int absoluteDay(int year, int month, int day)
	{
		int leap = 0;
		if (isLeapYear(year))
		{
			leap = 1;
		}
		int aday = day + numDaysInMonths(1, year, (month - 1)) + 365 * year + year / 4 - year / 100 + year / 400 - ABSOLUTE_DAY_DATUM - leap; // cancel /4 term - will be added by numDaysInMonths
		return aday;
	}

	/// <summary>
	/// Compute the absolute minute.  This can be used for determining the difference between dates. </summary>
	/// <returns> The absolute minute, with respect to Dec 31, 1799.  The datum may change
	/// in the future and should be used only in a dynamic fashion. </returns>
	/// <param name="year"> Year (4-digit). </param>
	/// <param name="month"> Month number (1-12). </param>
	/// <param name="day"> Day number (1-31). </param>
	/// <param name="hour"> Hour (0-23). </param>
	/// <param name="minute"> Minute (0-59). </param>
	public static long absoluteMinute(int year, int month, int day, int hour, int minute)
	{
		int aday = absoluteDay(year, month, day);
		long aminute = aday * 1440 + hour * 60 + minute;
		return aminute;
	}

	/// <summary>
	/// Return the absolute month, which is the year*12 + month. </summary>
	/// <returns> The absolute month, which is the year*12 + month. </returns>
	/// <param name="month"> Month number. </param>
	/// <param name="year"> Year. </param>
	public static int absoluteMonth(int month, int year)
	{
		return (year * 12 + month);
	}

	/// <summary>
	/// Given a starting DateTime, an interval, and the number of intervals, increment
	/// the DateTime, returning the final DateTime. </summary>
	/// <returns> incremented DateTime (new instance is returned). </returns>
	/// <param name="t1"> Initial DateTime. </param>
	/// <param name="base"> Interval base (see TimeInterval). </param>
	/// <param name="mult"> Interval multiplier. </param>
	/// <param name="nintervals"> Number of times to increment. </param>
	public static DateTime addIntervals(DateTime t1, int @base, int mult, int nintervals)
	{
		DateTime t = new DateTime(t1);
		for (int count = 0; count < nintervals; count++)
		{
			t.addInterval(@base, mult);
		}
		return t;
	}

	/// <summary>
	/// Indicate whether a DateTime's precision matches the specified TimeInterval string.
	/// This is useful, for example, in confirming that DateTime's for a time series are
	/// consistent with the time series data interval.  This only checks the precision of the base
	/// but does not check the multiplier. </summary>
	/// <param name="dt"> DateTime to check. </param>
	/// <param name="intervalString"> TimeInterval string to check. </param>
	/// <returns> 0 if the precision value of the DateTime is the same as the interval (or interval is irregular),
	/// -1 if the precision value of the DateTime is less than the interval (date/time is more precise),
	/// 1 if the precision value of the DateTime is greater than the interval (date/time is less precise),
	/// and null if the input is invalid. </returns>
	public static int? compareDateTimePrecisionToTimeInterval(DateTime dt, string intervalString)
	{
		TimeInterval ti = null;
		try
		{
			ti = TimeInterval.parseInterval(intervalString);
		}
		catch (Exception)
		{
			return null;
		}
		return compareDateTimePrecisionToTimeInterval(dt,ti);
	}

	/// <summary>
	/// Indicate whether a DateTime's precision matches the specified TimeInterval string.
	/// This is useful, for example, in confirming that DateTime's for a time series are
	/// consistent with the time series data interval.  This only checks the precision of the base
	/// but does not check the multiplier. </summary>
	/// <param name="dt"> DateTime to check. </param>
	/// <param name="interval"> interval to check. </param>
	/// <returns> 0 if the precision value of the DateTime is the same as the interval (or interval is irregular),
	/// -1 if the precision value of the DateTime is less than the interval (date/time is more precise),
	/// 1 if the precision value of the DateTime is greater than the interval (date/time is less precise),
	/// and null if the input is invalid. </returns>
	public static int? compareDateTimePrecisionToTimeInterval(DateTime dt, TimeInterval interval)
	{
		int precision = dt.getPrecision();
		int intervalBase = interval.getBase();
		if (intervalBase == TimeInterval.IRREGULAR)
		{
			return 0;
		}
		else if (precision < intervalBase)
		{
			return -1;
		}
		else if (precision > intervalBase)
		{
			return 1;
		}
		else
		{
			return 0;
		}
	}

	/// <summary>
	/// Indicate whether a DateTime's precision matches the specified TimeInterval,
	/// including base and multiplier.
	/// This is useful, for example, in confirming that DateTime's for a time series are
	/// consistent with the time series data interval.  Optionally, allow the precisions to
	/// be different when the DateTime is more precise but "remainder" date/time parts are
	/// zero.  This may be the case when a date/time is read from a source that always provides
	/// more precision in the data (e.g., databases). </summary>
	/// <param name="dt"> DateTime to check. </param>
	/// <param name="interval"> TimeInterval to check. </param>
	/// <param name="checkParts"> if true, then a date/time that is more precise than the interval is
	/// allowed as long as the higher precision values are zeros. </param>
	/// <returns> 0 if the precision value of the DateTime is the same as the interval (or interval is irregular),
	/// -1 if the precision value of the DateTime is less than the interval (date/time is more precise),
	/// 1 if the precision value of the DateTime is greater than the interval (date/time is less precise),
	/// and null if the input is invalid. </returns>
	public static int? compareDateTimePrecisionToTimeInterval(DateTime dt, TimeInterval interval, bool checkParts)
	{
		int intervalBase = interval.getBase();
		int intervalMult = interval.getMultiplier();
		int? compare = compareDateTimePrecisionToTimeInterval(dt, interval);
		if ((compare == null) || (compare == 1) || (intervalBase == TimeInterval.IRREGULAR))
		{
			// Cases that are independent of checkParts value
			return compare;
		}
		if (!checkParts)
		{
			// No need to check parts
			return compare;
		}
		else
		{
			// Precision of date/time is >= than specified interval but may be OK if:
			//   1) the remaining parts are DateTime initial values (0 or 1, depending on part)
			//   2) the date/time part at the same precision must evenly divide into the interval
			//      (e.g., 15Min data will allow minute values only 0, 15, 30, 45)
			// If the extra information is OK, return 0
			// First check to see if any date/time information in the remainder is not the initial values
			// Numerical value of the interval is bigger for larger intervals (e.g., YEAR > MONTH)
			int dtMorePrecise = -1;
			if (compare.Value == dtMorePrecise)
			{
				// Date/time precision is greater than the interval so need to check if the end parts 
				if (intervalBase >= TimeInterval.MINUTE)
				{
					if (dt.getSecond() != 0)
					{
						return dtMorePrecise;
					}
				}
				if (intervalBase >= TimeInterval.HOUR)
				{
					if (dt.getMinute() != 0)
					{
						return dtMorePrecise;
					}
				}
				if (intervalBase >= TimeInterval.DAY)
				{
					if (dt.getHour() != 0)
					{
						return dtMorePrecise;
					}
				}
				if (intervalBase >= TimeInterval.MONTH)
				{
					if (dt.getDay() > 1)
					{
						return dtMorePrecise;
					}
				}
				if (intervalBase >= TimeInterval.YEAR)
				{
					if (dt.getMonth() > 1)
					{
						return dtMorePrecise;
					}
				}
			}
			// Now check the multiplier on the date/time part of the interval base...
			if (intervalBase == TimeInterval.MINUTE)
			{
				if ((dt.getMinute() % intervalMult) != 0)
				{
					return dtMorePrecise;
				}
			}
			else if (intervalBase == TimeInterval.HOUR)
			{
				if ((dt.getHour() % intervalMult) != 0)
				{
					return dtMorePrecise;
				}
			}
			else if (intervalBase == TimeInterval.DAY)
			{
				if ((dt.getDay() % intervalMult) != 0)
				{
					return dtMorePrecise;
				}
			}
			else if (intervalBase == TimeInterval.MONTH)
			{
				if (dt.getMonth() != 1)
				{
					return dtMorePrecise;
				}
			}
			else if (intervalBase == TimeInterval.YEAR)
			{
				if (dt.getYear() != 1)
				{
					return dtMorePrecise;
				}
			}
			// More detailed parts do not introduce any additional data
			return 0;
		}
	}

	/// <summary>
	/// Convert a calendar month (1=January,...,12=December) to a month in a special
	/// calendar.  For example, water years are Oct to Sep.  To determine the month
	/// number (1+) in a water year given a calendar year month, do the following:
	/// <pre>
	/// int month = convertCalendarMonthToSpecialMonth ( cal_month, 10 );
	/// </pre> </summary>
	/// <param name="cal_month"> The calendar month (1=January, etc.). </param>
	/// <param name="first_cal_month_in_year"> The calendar month corresponding to the first
	/// month in the special calendar.  For example, for water years, specify 10 for October. </param>
	/// <returns> the month number in the custom calendar (1 to 12). </returns>
	public static int convertCalendarMonthToCustomMonth(int cal_month, int first_cal_month_in_year)
	{
		if (cal_month >= first_cal_month_in_year)
		{
			// This is the only clause that is processed for calendar year
			// and is used for high calendar months in custom calendars.
			// For example for water year (first_cal_month_in_year = 10), return cal_month - 3
			return (cal_month - (first_cal_month_in_year - 1));
		}
		else
		{
			// This will be processed for non-calendar year for early months in the calendar year.
			// For example for water year, return cal_month + 3
			return (cal_month + (12 - first_cal_month_in_year + 1));
		}
	}

	/// <summary>
	/// Determine whether the DateTime intervals align.
	/// For example, for data recorded on 6hour interval but offset at hours 1, 7, 13, and 20 will only
	/// align with similarly offset DateTime instances. </summary>
	/// <param name="dt1"> first DateTime to compare </param>
	/// <param name="dt2"> second DateTime to compare </param>
	/// <param name="interval"> TimeInterval indicating precision of  </param>
	public static bool dateTimeIntervalsAlign(DateTime dt1, DateTime dt2, TimeInterval interval)
	{
		int precision = dt1.getPrecision();
		if (precision != dt2.getPrecision())
		{
			return false;
		}
		if (interval.isRegularInterval())
		{
			// Irregular is not checked by this method
			return true;
		}
		// List in likely order of occurrence, for example 6Hour, 3min are more likely
		if (precision == DateTime.PRECISION_HOUR)
		{
			if (((dt1.getHour() - dt2.getHour()) % interval.getMultiplier()) != 0)
			{
				return false;
			}
			else
			{
				return true;
			}
		}
		else if (precision == DateTime.PRECISION_MINUTE)
		{
			if (((dt1.getMinute() - dt2.getMinute()) % interval.getMultiplier()) != 0)
			{
				return false;
			}
			else
			{
				return true;
			}
		}
		if (precision == DateTime.PRECISION_YEAR)
		{
			if (((dt1.getYear() - dt2.getYear()) % interval.getMultiplier()) != 0)
			{
				return false;
			}
			else
			{
				return true;
			}
		}
		else if (precision == DateTime.PRECISION_MONTH)
		{
			if (((dt1.getMonth() - dt2.getMonth()) % interval.getMultiplier()) != 0)
			{
				return false;
			}
			else
			{
				return true;
			}
		}
		else if (precision == DateTime.PRECISION_DAY)
		{
			if (((dt1.getDay() - dt2.getDay()) % interval.getMultiplier()) != 0)
			{
				return false;
			}
			else
			{
				return true;
			}
		}
		else
		{
			// Interval is not handled
			throw new InvalidTimeIntervalException("Interval " + interval + " is not handled");
		}
	}

	/// <summary>
	/// Return the day of the year. </summary>
	/// <returns> The day of the year (where 1 is the first day of the year and 365 or
	/// 366 is the last).  Return -1 if an error. </returns>
	/// <param name="d"> Java Date. </param>
	public static int dayOfYear(System.DateTime d)
	{
		if (d == null)
		{
			return -1;
		}
		// First get a string...
		string s = formatTimeString(d, "%j");
		// Now convert to an integer...
		int? i = Convert.ToInt32(s);
		int day = i.Value;
		s = null;
		i = null;
		return day;
	}

	/// <summary>
	/// Return the day of the year. </summary>
	/// <returns> The day of the year (where 1 is the first day of the year and 365 or
	/// 366 is the last).  Return -1 if an error. </returns>
	/// <param name="d"> Datetime to evaluate. </param>
	public static int dayOfYear(DateTime d)
	{
		if (d == null)
		{
			return -1;
		}
		int days = 0;
		// Days in previous months...
		if (d.getMonth() > 1)
		{
			days += numDaysInMonths(1, d.getYear(), (d.getMonth() - 1));
		}
		// Add the days from this month...
		days += d.getDay();
		return days;
	}

	/// <summary>
	/// Return the day of the year, where day 1 is the first day in the year type (Jan 1 for calendar year type).
	/// For example, for calendar year day 1 is Jan 1 and for water year day 1 is Oct 1. </summary>
	/// <returns> The day of the year (where 1 is the first day of the year and 365 or 366 is the last). </returns>
	/// <param name="d"> Datetime to evaluate, in calendar year. </param>
	/// <param name="yearType"> year type. </param>
	/// <exception cref="IllegalArgumentException"> if there is an error. </exception>
	public static int dayOfYear(DateTime d, YearType yearType)
	{
		if ((yearType == YearType.CALENDAR) || (yearType == null))
		{
			return dayOfYear(d);
		}
		// Else have non-calendar year so evaluation is a bit more complex
		if (d == null)
		{
			throw new System.ArgumentException("Date is null - cannot calculate day of year.");
		}
		int days = 0;
		// Else have non-calendar year so evaluation is a bit more complex
		int calMonth = d.getMonth();
		int startMonth = yearType.getStartMonth();
		if (calMonth < startMonth)
		{
			// First add the number of days in the months in the previous year
			days += numDaysInMonths(startMonth, d.getYear() + yearType.getStartYearOffset(), (12 - startMonth + 1));
			// Add the days in previous months of the current year
			if (calMonth > 1)
			{
				// Add the days from the previous months...
				days += numDaysInMonths(1, d.getYear(), (calMonth - 1));
			}
			// Add the days from the current month...
			days += d.getDay();
		}
		else
		{
			// At the start of the year (previous calendar year)
			if (calMonth > startMonth)
			{
				// Add the days from the previous months...
				days += numDaysInMonths(startMonth, d.getYear() + yearType.getStartYearOffset(), (calMonth - startMonth));
			}
			// Add the days from the current month...
			days += d.getDay();
		}
		return days;
	}

	/// <summary>
	/// Determine the inclusive output years given a time range in calendar years.  This is used to ensure that
	/// the output year fully includes any data from the calendar dates, even if partial output years.
	/// </summary>
	public static DateTimeRange determineOutputYearTypeRange(DateTime start, DateTime end, YearType outputYearType)
	{
		DateTime outputStart = new DateTime(start);
		outputStart.setPrecision(DateTime.PRECISION_YEAR);
		DateTime outputEnd = new DateTime(end);
		outputEnd.setPrecision(DateTime.PRECISION_YEAR);
		if ((outputYearType == YearType.CALENDAR))
		{
			// Just return the original dates...
			return new DateTimeRange(outputStart, outputEnd);
		}
		// Otherwise, deal with offsets

		// Start offset will be either 0 or -1
		if (outputYearType.getStartYearOffset() < 0)
		{
			// Year types where the year starts in the previous calendar year and ends in the current calendar year.
			if (start.getMonth() >= outputYearType.getStartMonth())
			{
				// The old time series starts >= after the beginning of the output year and therefore the first
				// output year actually starts a year later.
				//
				//              ++++++++++++  Calendar +++++++++++++++++++++
				//              Jan                            start     Dec
				// Oct                                   Sep   start
				// Water
				outputStart.addYear(1);
			}
			// Else the start is within the output year so no need to adjust the output year
			if (end.getMonth() >= outputYearType.getStartMonth())
			{
				// The old time series ends after the beginning of the next output year so increment the year
				//
				//               ++++++++++++  Calendar +++++++++++++++++++++
				//               Jan                             end      Dec
				// Oct                                    Sep    end  
				// Water
				outputEnd.addYear(1);
			}
		}
		else
		{
			// Year types where the year starts in the current calendar year and ends in the next calendar year
			if (start.getMonth() < outputYearType.getStartMonth())
			{
				// The old time series starts before the beginning of the output year and would result
				// in a missed year at the start so decrement the first year, as shown in the following example:
				//
				//           ++++++++++++  Calendar +++++++++++++++++++++
				//           Jan   start                              Dec
				//                 start    May                                   Apr
				// YearMayToApr
				outputStart.addYear(-1);
			}
			if (end.getMonth() < outputYearType.getStartMonth())
			{
				// The old time series ends in the previous output year so decrement the last year,
				// as shown in the following example:
				//
				//           ++++++++++++  Calendar +++++++++++++++++++++
				//           Jan   end                                Dec
				//                 end      May                                   Apr
				// YearMayToApr
				outputEnd.addYear(-1);
			}
		}
		return new DateTimeRange(outputStart,outputEnd);
	}

	/// <summary>
	/// Compute the difference between dates.  See overloaded routine for more information. </summary>
	/// <returns> The offset from the date instance to the given date.  For example, if
	/// "date" is before the instance date, then the offset will be positive. </returns>
	/// <param name="date1"> The date to be subtracted from. </param>
	/// <param name="date2"> The date to subtract from date1.
	/// REVISIT JAVADOC: see RTi.Util.Time.DateTime.add </param>
	/// <exception cref="Exception"> if either date is null. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static DateTime diff(DateTime date1, DateTime date2) throws Exception
	public static DateTime diff(DateTime date1, DateTime date2)
	{
		return diff(date1, date2, false);
	}

	/// <summary>
	/// Subtract a date from another and return the offset.  This can then be used
	/// with the DateTime.add method to shift a date.  It is important to only compare
	/// dates of the same precision. </summary>
	/// <returns> The offset from the date instance to the given date.  For example, if
	/// "date" is before the instance date, then the offset will be positive. </returns>
	/// <param name="date1"> The DateTime to be subtracted from. </param>
	/// <param name="date2"> The DateTime to subtract from date1. </param>
	/// <param name="use_month"> If true, the offset will be computed by setting the month also.
	/// In many cases, this is not valid because the relationship between days and
	/// months is dynamic.  If false, the month will be set to zero and the day will be
	/// set to the total number of days in the year from the difference. </param>
	/// <exception cref="Exception"> if either date is null. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static DateTime diff(DateTime date1, DateTime date2, boolean use_month) throws Exception
	public static DateTime diff(DateTime date1, DateTime date2, bool use_month)
	{
		if (date1 == null)
		{
			Message.printWarning(3, "TimeUtil.subtract", "Null date");
			throw new Exception("Null date for diff");
		}
		if (date2 == null)
		{
			Message.printWarning(3, "TimeUtil.subtract", "Null date");
			throw new Exception("Null date for diff");
		}

		DateTime offset = new DateTime(DateTime.DATE_ZERO | DateTime.DATE_FAST);
		offset.setYear(0);
		offset.setMonth(0);
		offset.setDay(0);
		offset.setHour(0);
		offset.setMinute(0);
		offset.setSecond(0);
		offset.setHSecond(0);
		DateTime datecopy = new DateTime(date2);

		// First see if we need to add or subtract...

		if (datecopy.Equals(date1))
		{
			// Special case is no difference in the dates so the offset
			// date must be handled accordingly. This means setting the
			// month and day to zero instead month 1 and day 1 for a
			// DateTime(DateTime.DATE_ZERO) object.

			offset.setMonth(0);
			offset.setDay(0);
			return offset;
		}

		// Handle some special cases that are often encountered and handled the
		// others generically...

		if (date1.getPrecision() == DateTime.PRECISION_DAY)
		{
			// If Day and month are the same, return the year as the offset...
			if ((date1.getDay() == date2.getDay()) && (date1.getMonth() == date2.getMonth()))
			{
				offset.setYear(date1.getYear() - date2.getYear());
			}
		}
		else
		{
			// All other cases...
			// For the moment, ignore the old logic and use floating
			// point numbers to do the offset.  Test to see if it works...
			double date_copy_double = datecopy.toDouble();
			double this_double = date1.toDouble();
			double offset_double = this_double - date_copy_double;
			offset = new DateTime(offset_double, use_month);
		}
		return offset;
	}

	/// <summary>
	/// Format a DateTime using the given format.  The year type defaults to CALENDAR. </summary>
	/// <param name="dt"> DateTime object to format </param>
	/// <param name="format"> format string (see overloaded version for details) </param>
	public static string formatDateTime(DateTime dt, string format)
	{
		return formatDateTime(dt, YearType.CALENDAR, format);
	}

	/// <summary>
	/// Format a DateTime using the given format. </summary>
	/// <returns> The date/time as a string for the specified date using the specified format. </returns>
	/// <param name="d0"> The date to format.  If the date is null, the current time is used. </param>
	/// <param name="yearType"> the year type, for example NOV_TO_OCT will result in date/times in Nov-Dec of the first year having a year
	/// of the following calendar year for ${dt:YearForYearType} property </param>
	/// <param name="format0"> The date format.  If the format is null,
	/// the default is as follows:  "Fri Jan 03 16:05:14 MST 1998" (the UNIX date
	/// command output).  The date can be formatted using the format modifiers of the
	/// C "strftime" routine, as follows:
	/// <para>
	/// 
	/// <table width=100% cellpadding=10 cellspacing=0 border=2>
	/// <tr>
	/// <td><b>Format Specifier</b></td>	<td><b>Description</b></td>
	/// </tr
	/// 
	/// <tr>
	/// <td><b>%a</b></td>
	/// <td>The abbreviated weekday.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%A</b></td>
	/// <td>The full weekday.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%b</b></td>
	/// <td>The abbreviated month.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%B</b></td>
	/// <td>The full month.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%c</b></td>
	/// <td>Not supported.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%d</b></td>
	/// <td>Day of month in range 1-31.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%H</b></td>
	/// <td>Hour of day in range 0-23.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%I</b></td>
	/// <td>Hour of day in range 0-12.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%j</b></td>
	/// <td>Day of year in range 1-366.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%m</b></td>
	/// <td>Month of year in range 1-12.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%M</b></td>
	/// <td>Minute of hour in range 0-59.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%p</b></td>
	/// <td>"AM" or "PM".</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%S</b></td>
	/// <td>Seconds of minute in range 0-59.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%U</b> or <b>%W</b></td>
	/// <td>Week of year in range 1-52.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%x</b></td>
	/// <td>Not supported.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%X</b></td>
	/// <td>Not supported.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%y</b></td>
	/// <td>Two digit year since 1900 (this is use discouraged because the datum is ambiguous).</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%Y</b></td>
	/// <td>Four digit year.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%Z</b></td>
	/// <td>Three character time zone abbreviation.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%%</b></td>
	/// <td>Literal % character.</td>
	/// </tr>
	/// 
	/// </table> </param>
	public static string formatDateTime(DateTime d0, YearType yearType, string format0)
	{
		string default_format = "%a %b %d %H:%M:%S %Z %Y";
		string format;

		if (string.ReferenceEquals(format0, null))
		{
			format = default_format;
		}
		else if (format0.Length == 0)
		{
			format = default_format;
		}
		else
		{
			format = format0;
		}

		DateTime dateTime;
		if (d0 == null)
		{
			// Get the current time...
			dateTime = new DateTime(DateTime.DATE_CURRENT);
		}
		else
		{
			dateTime = d0;
		}
		// Use the date to format and then use DateTime time zone
		System.DateTime date = null;
		// TODO SAM 2016-05-02 really need to handle time zone more explicitly here
		// Get the date using the DateTime's time zone or if not specified use the local time
		// This matches legacy behavior
		date = dateTime.getDate(TimeZoneDefaultType.LOCAL);

		if (format.Equals("datum_seconds"))
		{
			// Want the number of seconds since the standard time datum	
			// Need to work on this...
			//long seconds = d.getTime ();
			//return Long.toString ( seconds/1000 );
			return "0";
		}
		// Convert format to string...
		GregorianCalendar cal = new GregorianCalendar();
		cal.setTime(date);
		SimpleDateFormat sdf = new SimpleDateFormat();
		DateFormatSymbols dfs = sdf.getDateFormatSymbols();
		string[] short_weekdays = dfs.getShortWeekdays();
		string[] short_months = dfs.getShortMonths();
		string[] months = dfs.getMonths();
		string[] weekdays = dfs.getWeekdays();
		int len = format.Length;
		StringBuilder formatted_string = new StringBuilder();
		char c = '\0';
		int ifield;
		// The values returned are as follows:
		//
		//            Java              This code
		//
		// Year:      since 1900        4-digit
		// Month:     0 to 11           1 to 12
		// Day:       1 to 31           1 to 31
		// Hour:      0 to 59           same
		// Minute:    0 to 59           same
		// Second:    0 to 59           same
		// DayOfWeek: 0 to 7 with 0     same
		//            being Sunday
		//            in U.S.
		// First go through the % format specifiers
		for (int i = 0; i < len; i++)
		{
			c = format[i];
			if (c == '%')
			{
				// We have a format character...
				++i;
				if (i >= len)
				{
					break; // this will exit the whole loop
				}
				c = format[i];
				if (c == 'a')
				{
					// Abbreviated weekday name.
					ifield = cal.get(System.DateTime.DAY_OF_WEEK);
					formatted_string.Append(short_weekdays[ifield]);
				}
				else if (c == 'A')
				{
					// Full weekday name.
					ifield = cal.get(System.DateTime.DAY_OF_WEEK);
					formatted_string.Append(weekdays[ifield]);
				}
				else if (c == 'b')
				{
					// Abbreviated month name.
					ifield = cal.get(System.DateTime.MONTH);
					formatted_string.Append(short_months[ifield]);
				}
				else if (c == 'B')
				{
					// Long month name.
					ifield = cal.get(System.DateTime.MONTH);
					formatted_string.Append(months[ifield]);
				}
				else if (c == 'c')
				{
					formatted_string.Append("%c not supported");
				}
				else if (c == 'd')
				{
					// Day of month
					formatted_string.Append(StringUtil.formatString(dateTime.getDay(),"%02d"));
				}
				else if (c == 'H')
				{
					// Hour of day...
					formatted_string.Append(StringUtil.formatString(dateTime.getHour(),"%02d"));
				}
				else if (c == 'I')
				{
					// Hour of day 1-12
					if (dateTime.getHour() > 12)
					{
						formatted_string.Append(StringUtil.formatString((dateTime.getHour() - 12),"%02d"));
					}
					else
					{
						formatted_string.Append(StringUtil.formatString(dateTime.getHour(),"%02d"));
					}
				}
				else if (c == 'j')
				{
					// Day of year...
					formatted_string.Append(StringUtil.formatString(dateTime.getYearDay(),"%03d"));
				}
				else if (c == 'm')
				{
					// Month of year...
					formatted_string.Append(StringUtil.formatString(dateTime.getMonth(),"%02d"));
				}
				else if (c == 'M')
				{
					// Minute of hour...
					formatted_string.Append(StringUtil.formatString(dateTime.getMinute(),"%02d"));
				}
				else if (c == 'p')
				{
					// AM or PM...
					if (dateTime.getHour() < 12)
					{
						formatted_string.Append("AM");
					}
					else
					{
						formatted_string.Append("PM");
					}
				}
				else if (c == 's')
				{
					// Seconds since 1970-01-01 00:00:00+0000 (UTC)...
					formatted_string.Append("" + date.Ticks / 1000);
				}
				else if (c == 'S')
				{
					// Seconds of minute...
					formatted_string.Append(StringUtil.formatString(dateTime.getSecond(),"%02d"));
				}
				else if ((c == 'U') || (c == 'W'))
				{
					// Week of year...
					ifield = cal.get(System.DateTime.WEEK_OF_YEAR);
					formatted_string.Append(StringUtil.formatString(ifield,"%02d"));
				}
				else if (c == 'x')
				{
					formatted_string.Append("%x not supported");
				}
				else if (c == 'X')
				{
					formatted_string.Append("%X not supported");
				}
				else if (c == 'y')
				{
					// Two digit year...
					formatted_string.Append(StringUtil.formatString(formatYear(dateTime.getYear(),2,true),"%02d"));
				}
				else if (c == 'Y')
				{
					formatted_string.Append(StringUtil.formatString(dateTime.getYear(),"%04d"));
				}
				else if (c == 'Z')
				{
					formatted_string.Append(dateTime.getTimeZoneAbbreviation());
				}
				else if (c == '%')
				{
					// Literal percent...
					formatted_string.Append('%');
				}
				else
				{
					// Go ahead and add the % and the character
					// (e.g., so the format can be passed to a secondary formatter).
					formatted_string.Append('%');
					formatted_string.Append(c);
				}
			}
			else if ((c == '$') && (yearType != null))
			{
				// TODO SAM 2013-12-23 For now hard-code one check but as more properties are added, make the code more elegant
				string prop = "${dt:YearForYearType}";
				int iEnd = i + prop.Length; // Last char, zero index
				string year = "";
				if (iEnd <= format.Length)
				{
					//Message.printStatus(2, "", "substring=\"" + format.substring(i,iEnd) + "\" prop=\"" + prop + "\"");
					if (format.Substring(i, iEnd - i).Equals(prop, StringComparison.OrdinalIgnoreCase))
					{
						year = "" + TimeUtil.getYearForYearType(dateTime, yearType);
						formatted_string.Append(year);
						i = i + prop.Length - 1; // -1 because the iterator will increment by one
					}
					else
					{
						// Just add the $ and march on
						formatted_string.Append(c);
					}
				}
				else
				{
					// Just add the $ and march on
					formatted_string.Append(c);
				}
			}
			else
			{
				// Just add the character to the string...
				formatted_string.Append(c);
			}
		}
		// Next go through the ${dt:property} specifiers

		return formatted_string.ToString();
	}

	/// <summary>
	/// Return the current date/time formatted using the default format. </summary>
	/// <returns> The current date date/time as a string using the default format
	/// (see the version that accepts a date and format). </returns>
	public static string formatTimeString()
	{
		return formatTimeString(System.DateTime.Now);
	}

	/// <summary>
	/// Return The current date/time formatted using the specified format string. </summary>
	/// <returns> The current date/time formatted using the specified format string. </returns>
	/// <param name="format"> The date format (see the version that accepts a date and format). </param>
	public static string formatTimeString(string format)
	{
		return formatTimeString(System.DateTime.Now, format);
	}

	/// <summary>
	/// Return the date/time formatted using the default format string. </summary>
	/// <returns> The date/time formatted using the default format string. </returns>
	/// <param name="d0"> The date to format (see the version that accepts a date and format). </param>
	public static string formatTimeString(System.DateTime d0)
	{
		return formatTimeString(d0, null);
	}

	/// <summary>
	/// Format a Date using the given format.  See also formatDateTime(). </summary>
	/// <returns> The date/time formatted using the specified format. </returns>
	/// <param name="d0"> The date to format.  If the date is null, the current time is used. </param>
	/// <param name="format0"> The date format.  If the format is null,
	/// the default is as follows:  "Fri Jan 03 16:05:14 MST 1998" (the UNIX date
	/// command output).  The date can be formatted using the format modifiers of the
	/// C "strftime" routine, as follows:
	/// <para>
	/// 
	/// <table width=100% cellpadding=10 cellspacing=0 border=2>
	/// <tr>
	/// <td><b>Format Specifier</b></td>	<td><b>Description</b></td>
	/// </tr
	/// 
	/// <tr>
	/// <td><b>%a</b></td>
	/// <td>The abbreviated weekday.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%A</b></td>
	/// <td>The full weekday.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%b</b></td>
	/// <td>The abbreviated month.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%B</b></td>
	/// <td>The full month.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%c</b></td>
	/// <td>Not supported.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%d</b></td>
	/// <td>Day of month in range 1-31.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%H</b></td>
	/// <td>Hour of day in range 0-23.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%I</b></td>
	/// <td>Hour of day in range 0-12.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%j</b></td>
	/// <td>Day of year in range 1-366.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%m</b></td>
	/// <td>Month of year in range 1-12.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%M</b></td>
	/// <td>Minute of hour in range 0-59.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%p</b></td>
	/// <td>"AM" or "PM".</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%S</b></td>
	/// <td>Seconds of minute in range 0-59.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%U</b> or <b>%W</b></td>
	/// <td>Week of year in range 1-52.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%x</b></td>
	/// <td>Not supported.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%X</b></td>
	/// <td>Not supported.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%y</b></td>
	/// <td>Two digit year since 1900 (this is use discouraged because the datum is ambiguous).</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%Y</b></td>
	/// <td>Four digit year.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%Z</b></td>
	/// <td>Three character time zone abbreviation.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%%</b></td>
	/// <td>Literal % character.</td>
	/// </tr>
	/// 
	/// </table> </param>
	public static string formatTimeString(System.DateTime d0, string format0)
	{
		string routine = "TimeUtil.formatTimeString";
		string default_format = "%a %b %d %H:%M:%S %Z %Y";
		string format;
		int dl = 50;

		if (string.ReferenceEquals(format0, null))
		{
			format = default_format;
		}
		else if (format0.Length == 0)
		{
			format = default_format;
		}
		else
		{
			format = format0;
		}

		System.DateTime d;
		if (d0 == null)
		{
			// Get the current time...
			d = System.DateTime.Now;
		}
		else
		{
			d = d0;
		}

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Formatting \"" + d0 + "\" using \"" + format + "\"");
		}

		if (format.Equals("datum_seconds"))
		{
			/*
			** Want the number of seconds since the standard time datum	
			*/
			long seconds = d.Ticks;
			return Convert.ToString(seconds / 1000);
		}
		else
		{
			// Convert format to string...
			//

			// First we get a Gregorian Calendar from the date...

			GregorianCalendar cal = new GregorianCalendar();
			cal.setTime(d);
			int len = format.Length;
			StringBuilder formatted_string = new StringBuilder();
			char c = '\0';
			int ifield; // integer field value
			SimpleDateFormat sdf = new SimpleDateFormat();
			DateFormatSymbols dfs = sdf.getDateFormatSymbols();
			string[] short_weekdays = dfs.getShortWeekdays();
			string[] short_months = dfs.getShortMonths();
			string[] months = dfs.getMonths();
			string[] weekdays = dfs.getWeekdays();
			string[] am_pm = dfs.getAmPmStrings();
			// The values returned are as follows:
			//
			//		Java		We use
			//
			// Year:	since 1900	4-digit
			// Month:	0 to 11		1 to 12
			// Day:		1 to 31		1 to 31
			// Hour:	0 to 59		same
			// Minute:	0 to 59		same
			// Second:	0 to 59		same
			// DayOfWeek:	0 to 7 with 0	same
			//		being Sunday
			//		in U.S.
			for (int i = 0; i < len; i++)
			{
				c = format[i];
				if (c == '%')
				{
					// We have a format character...
					++i;
					if (i >= len)
					{
						break; // this will exit the whole loop
					}
					c = format[i];
					if (c == 'a')
					{
						// Abbreviated weekday name.
						ifield = cal.get(System.DateTime.DAY_OF_WEEK);
						formatted_string.Append(short_weekdays[ifield]);
						if (Message.isDebugOn)
						{
							Message.printDebug(dl, routine, "Day of week =" + ifield + " \"" + short_weekdays[ifield] + "\"");
						}
					}
					else if (c == 'A')
					{
						// Full weekday name.
						ifield = cal.get(System.DateTime.DAY_OF_WEEK);
						formatted_string.Append(weekdays[ifield]);
						if (Message.isDebugOn)
						{
							Message.printDebug(dl, routine, "Day of week =" + ifield + " \"" + weekdays[ifield] + "\"");
						}
					}
					else if (c == 'b')
					{
						// Abbreviated month name.
						ifield = cal.get(System.DateTime.MONTH);
						formatted_string.Append(short_months[ifield]);
						if (Message.isDebugOn)
						{
							Message.printDebug(dl, routine, "Month=" + ifield + " \"" + short_months[ifield] + "\"");
						}
					}
					else if (c == 'B')
					{
						// Long month name.
						ifield = cal.get(System.DateTime.MONTH);
						formatted_string.Append(months[ifield]);
						if (Message.isDebugOn)
						{
							Message.printDebug(dl, routine, "Month=" + ifield + " \"" + months[ifield] + "\"");
						}
					}
					else if (c == 'c')
					{
						formatted_string.Append("%c not supported");
					}
					else if (c == 'd')
					{
						// Day of month
						ifield = cal.get(System.DateTime.DAY_OF_MONTH);
						formatted_string.Append(StringUtil.formatString(ifield,"%02d"));
						if (Message.isDebugOn)
						{
							Message.printDebug(dl, routine, "Day =" + ifield);
						}
					}
					else if (c == 'H')
					{
						// Hour of day...
						ifield = cal.get(System.DateTime.HOUR_OF_DAY);
						formatted_string.Append(StringUtil.formatString(ifield,"%02d"));
						if (Message.isDebugOn)
						{
							Message.printDebug(dl, routine, "Hour=" + ifield);
						}
					}
					else if (c == 'I')
					{
						// Hour of day 1-12
						ifield = cal.get(System.DateTime.HOUR);
						formatted_string.Append(StringUtil.formatString(ifield,"%02d"));
						if (Message.isDebugOn)
						{
							Message.printDebug(dl, routine, "Hour =" + ifield);
						}
					}
					else if (c == 'j')
					{
						// Day of year...
						ifield = cal.get(System.DateTime.DAY_OF_YEAR);
						formatted_string.Append(StringUtil.formatString(ifield,"%03d"));
						if (Message.isDebugOn)
						{
							Message.printDebug(dl, routine, "DayofYear=" + ifield);
						}
					}
					else if (c == 'm')
					{
						// Month of year...
						ifield = cal.get(System.DateTime.MONTH) + 1;
						formatted_string.Append(StringUtil.formatString(ifield,"%02d"));
						if (Message.isDebugOn)
						{
							Message.printDebug(dl, routine, "Month =" + ifield);
						}
					}
					else if (c == 'M')
					{
						// Minute of hour...
						ifield = cal.get(System.DateTime.MINUTE);
						formatted_string.Append(StringUtil.formatString(ifield,"%02d"));
						if (Message.isDebugOn)
						{
							Message.printDebug(dl, routine, "Minute =" + ifield);
						}
					}
					else if (c == 'p')
					{
						// AM or PM...
						ifield = cal.get(System.DateTime.AM_PM);
						formatted_string.Append(am_pm[ifield]);
						if (Message.isDebugOn)
						{
							Message.printDebug(dl, routine, "AM/PM=" + ifield + " \"" + am_pm[ifield] + "\"");
						}
					}
					else if (c == 'S')
					{
						// Seconds of minute...
						ifield = cal.get(System.DateTime.SECOND);
						formatted_string.Append(StringUtil.formatString(ifield,"%02d"));
						if (Message.isDebugOn)
						{
							Message.printDebug(dl, routine, "Second =" + ifield);
						}
					}
					else if ((c == 'U') || (c == 'W'))
					{
						// Week of year...
						// Don't worry now about whether Sunday or Monday are the start of the week.
						ifield = cal.get(System.DateTime.WEEK_OF_YEAR);
						formatted_string.Append(StringUtil.formatString(ifield,"%02d"));
						if (Message.isDebugOn)
						{
							Message.printDebug(dl, routine, "Weekofyear =" + ifield);
						}
					}
					else if (c == 'x')
					{
						formatted_string.Append("%x not supported");
					}
					else if (c == 'X')
					{
						formatted_string.Append("%X not supported");
					}
					else if (c == 'y')
					{
						// Two digit year...
						ifield = cal.get(System.DateTime.YEAR);
						if (ifield > 60000)
						{
							// Borland database bug...
							ifield -= 65536;
						}
						ifield = formatYear(ifield, 2, true);
						formatted_string.Append(StringUtil.formatString(ifield,"%02d"));
						if (Message.isDebugOn)
						{
							Message.printDebug(dl, routine, "year =" + ifield);
						}
					}
					else if (c == 'Y')
					{
						// Four digit year...
						ifield = cal.get(System.DateTime.YEAR);
						if (ifield > 60000)
						{
							// Borland database bug...
							ifield -= 65536;
						}
						ifield = formatYear(ifield, 4, true);
						formatted_string.Append(StringUtil.formatString(ifield,"%04d"));
						if (Message.isDebugOn)
						{
							Message.printDebug(dl, routine, "Year =" + ifield);
						}
					}
					else if (c == 'Z')
					{
						// Time zone offset from GMT to local time, in milliseconds...
						formatted_string.Append(cal.getTimeZone().getID());
					}
					else if (c == '%')
					{
						// Literal percent...
						formatted_string.Append('%');
					}
				}
				else
				{
					// Just add the character to the string...
					formatted_string.Append(c);
				}
			}
			return formatted_string.ToString();
		}
	}

	/// <summary>
	/// Convert between 2 and 4 digit year representations, assuming that a future year
	/// is not allowed (this is mainly useful to convert a 4-digit year to 2-digit). </summary>
	/// <param name="year"> The year to convert. </param>
	/// <param name="len"> The length of the output year (either 2 or 4). </param>
	/// <returns> The formatted 2 or 4 digit year. </returns>
	public static int formatYear(int year, int len)
	{
		return formatYear(year, len, false);
	}

	/// <summary>
	/// Convert between 2 and 4 digit year representations. </summary>
	/// <param name="year0"> The year to convert. </param>
	/// <param name="len"> The length of the output year (either 2 or 4). </param>
	/// <param name="allow_future"> If false, indicates that the resulting 4-digit year cannot
	/// be a future year, based on the system clock. </param>
	/// <returns> The formatted 2 or 4 digit year.  Return -1 if there is an error. </returns>
	public static int formatYear(int year0, int len, bool allow_future)
	{
		int year;
		int year_offset;

		// Initialize return value...

		year = year0;

		if (len == 2)
		{
			if (year0 < 100)
			{
				// OK as is...
				year = year0;
				return year;
			}
			else
			{
				// Truncate the year to return only the last 2 digits...
				year = (year0 - ((year0 / 100) * 100));
				return year;
			}
		}
		else if (len == 4)
		{
			if (year0 > 100)
			{
				// OK as is...
				year = year0;
				return year;
			}
			else
			{
				// Get the year offset from the system (have to assume this so old data may have problems!).
				year_offset = getYearOffset();
				if (year_offset < 0)
				{
					Message.printWarning(3, "TimeUtil.formatYear", "Unable to get system year offset");
					return -1;
				}
				// Get the current system year...
				// This does not seem to work well - it converts to Pacific time.
				//String message = "{0,date,yyyy}";
				//MessageFormat mf = new MessageFormat ( message );
				//Date now = new Date();
				//Object [] o = { now };
				int t_year = int.Parse(formatTimeString("%Y"));
				year = year0 + year_offset;
				if ((year > t_year) && !allow_future)
				{
					// Don't allow future years so subract 100.
					// This comes up, for example, if the input
					// is 70 and the current year is 2002.  In this
					// case, using the system offset would give a
					// year 2070, which is in the future.  Instead,
					// we actually want 1970.  There is no simple
					// way to deal with data that is older than
					// 100 years (the user would have to supply
					// some extra information and in that case this
					// routine is pretty worthless)!
					year -= 100;
				}
				return year;
			}
		}
		else
		{
			// Unknown format request...
			Message.printWarning(3, "TimeUtil.formatYear", "Year ndigits " + len + " not 2 or 4!");
			return -1;
		}
	}

	/// <summary>
	/// Create a DateTime object from the specified UNIX time, which is the number of seconds since Jan 1, 1970 00:00:00. </summary>
	/// <param name="unixTimeMs"> UNIX time as milliseconds since Jan 1, 1970 00:00:00. </param>
	/// <param name="dt"> DateTime containing date/time to receive results. If null then a new DateTime will be returned.
	/// Specify to reuse a DateTime instance, such as when iterating.  Only data values computed from the UNIX time are set.
	/// Time zone, etc. are not modified. </param>
	/// <returns> DateTime object resulting from the conversion. A new instance will be created if dt=null.
	/// A new instance will have the time zone set to GMT. </returns>
	/// <exception cref="RuntimeException"> if there is an error processing the UNIX time </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static DateTime fromUnixTime(long unixTimeMs, DateTime dt) throws RuntimeException
	public static DateTime fromUnixTime(long unixTimeMs, DateTime dt)
	{
		// For now do not support negative
		if (unixTimeMs < 0)
		{
			throw new Exception("Negative UNIX time is not supported.");
		}
		DateTime dtout = null;
		if (dt != null)
		{
			dtout = dt;
		}
		else
		{
			dtout = new DateTime();
			dtout.setTimeZone("GMT");
		}
		// Number of seconds
		long unixTimeSeconds = unixTimeMs / 1000;
		long seconds = 0;
		int year = 1970;
		int month = 1, day = 1, hour = 0, minute = 0, second = 0;
		// For now loop, but could improve performance
		for (int i = 1970; ; i++)
		{
			if (isLeapYear(i))
			{
				seconds = seconds + 366 * 86400;
			}
			else
			{
				seconds = seconds + 365 * 86400;
			}
			if (seconds > unixTimeSeconds)
			{
				// Have gone too many years
				if (isLeapYear(i))
				{
					seconds = seconds - 366 * 86400;
				}
				else
				{
					seconds = seconds - 365 * 86400;
				}
				// Don't adjust year because time is in this year
				break;
			}
			else
			{
				// Add another year because time is in next year
				++year;
			}
		}
		// Loop through months of year
		bool foundDay = false;
		for (month = 1; month <= 12; month++)
		{
			for (day = 1; day <= numDaysInMonth(month,year); day++)
			{
				if ((unixTimeSeconds - seconds) < 86400)
				{
					// Need to break because time is in previous day
					foundDay = true;
					break;
				}
				seconds = seconds + 86400;
			}
			if (foundDay)
			{
				break;
			}
		}
		// Now are in the correct day so may for hours, minutes, and seconds is simple.
		long secondsLeft = unixTimeSeconds - seconds;
		hour = (int)secondsLeft / 3600;
		minute = (int)(secondsLeft % 3600) / 60;
		second = (int)(secondsLeft - hour * 3600 - minute * 60);
		dtout.setYear(year);
		dtout.setMonth(month);
		dtout.setDay(day);
		dtout.setHour(hour);
		dtout.setMinute(minute);
		dtout.setSecond(second);
		return dtout;
	}

	/// <summary>
	/// Get the current day of the week as a number. </summary>
	/// <returns> The current day of the week as a number, in the range 0-6, with 0 being
	/// Sunday.  The system clock is used to get the current time.  Return -1 if an error. </returns>
	public virtual int getCurrentDayOfWeek()
	{ // First get the day of week as a string...
		string day = formatTimeString("%a");
		// Now loop through and figure out the day...
		for (int i = 0; i < 7; i++)
		{
			if (day.Equals(DAY_ABBREVIATIONS[i], StringComparison.OrdinalIgnoreCase))
			{
				// Have a match...
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// Return an array of valid format specifiers for the formatDateTime() method, in
	/// the format "%X - Description" where X is the format specifier.  The specifiers correspond to the strftime
	/// formatting routine. </summary>
	/// <returns> an array of format specifiers. </returns>
	/// <param name="includeDescription"> If false, only the %X specifiers are returned.  if
	/// True, the description is also returned. </param>
	/// <param name="forOutput"> if true, return specifiers for formatting; if false only include formatters for parsing </param>
	/// <param name="includeProps"> if true, include properties like ${dt:YearForYearType}, currently only used for output </param>
	public static string[] getDateTimeFormatSpecifiers(bool includeDescription, bool forOutput, bool includeProps)
	{
		int nProps = 0;
		if (forOutput && includeProps)
		{
			nProps = 1;
		}
		string[] formats = new string[16 + nProps];
		int i = -1;
		formats[++i] = "%a - Weekday, abbreviation";
		formats[++i] = "%A - Weekday, full";
		formats[++i] = "%b - Month, abbreviation";
		formats[++i] = "%B - Month, full";
		//formats[++i] = "%c - Not supported";
		formats[++i] = "%d - Day (01-31)";
		formats[++i] = "%H - Hour (00-23)";
		formats[++i] = "%I - Hour (01-12)";
		formats[++i] = "%j - Day of year (001-366)";
		formats[++i] = "%m - Month (01-12)";
		formats[++i] = "%M - Minute (00-59)";
		formats[++i] = "%p - AM, PM";
		formats[++i] = "%s - seconds since Jan 1, 1970 UTC";
		formats[++i] = "%S - Second (00-59)";
		//formats[++i] = "%U, %W - not supported";
		//formats[++i] = "%x - not supported";
		//formats[++i] = "%X - not supported";
		formats[++i] = "%y - Year (00-99)";
		formats[++i] = "%Y - Year (0000-9999)";
		formats[++i] = "%Z - Time zone";
		if (forOutput && includeProps)
		{
			formats[++i] = "${dt:YearForYearType} - year for year type";
		}
		if (!forOutput)
		{
			// Only include formats suitable for parsing
			string[] formats2 = new string[] {"%S - Second (00-59)", "%M - Minute (00-59)", "%H - Hour (00-23)", "%d - Day (01-31)", "%b - Month, abbreviation", "%m - Month (01-12)", "%y - Year (00-99)", "%Y - Year (0000-9999)"};
			formats = formats2;
		}
		if (!includeDescription)
		{
			// Remove the text including and after the dash
			for (int j = 0; j < formats.Length; j++)
			{
				formats[j] = formats[j].Substring(0,formats[j].IndexOf("-", StringComparison.Ordinal)).Trim();
			}
		}
		return formats;
	}

	/// <summary>
	/// Return an array of valid format specifiers for the strftime formatter in
	/// the format "%X - Description" where X is the format specifier.  This is useful
	/// for building graphical interfaces. </summary>
	/// <returns> an array of format specifiers. </returns>
	/// <param name="includeDescription"> If false, only the %X specifiers are returned.  if
	/// True, the description is also returned. </param>
	/* TODO SAM 2011-11-14 Merge with the above method
	public static String[] getStrftimeFormatSpecifiers(boolean includeDescription )
	{   String [] formats = new String[23];
	    int i = 0;
	    // These are from Linux strftime
	    formats[i++] = "%a - abbreviated weekday name"; // Should depend on locale
	    formats[i++] = "%A - full weekday name"; // Should depend on locale
	    formats[i++] = "%b - abbreviated month name"; // Should depend on locale
	    formats[i++] = "%B - full month name"; // Should depend on locale
	    //formats[i++] = "%c - preferred date and time representation"; // Not supported here
	    formats[i++] = "%C - century number (year/100)r";
	    formats[i++] = "%d - day of month (01-31)";
	    formats[i++] = "%D - %m/%d/%y";
	    formats[i++] = "%e - like %d but pad with space";
	    //formats[i++] = "%E - modifier"; // Not supported here
	    formats[i++] = "%F - %Y-%m-%d";
	    //formats[i++] = "%G - week-based year"; // Not supported here
	    //formats[i++] = "%g - like %G but without century"; // Not supported here
	    //formats[i++] = "%h - equivalent to %b"; // Redundant, not used
	    formats[i++] = "%H - hour (00-23)";
	    //formats[i++] = "%I - hour (01-12)"; // TODO SAM 2011-11-14 add later
	    formats[i++] = "%j - day of year (001-366)";
	    //formats[i++] = "%l - hour (1-12)"; // TODO SAM 2011-11-14 add later
	    formats[i++] = "%m - month (01-12)";
	    formats[i++] = "%M - minute (00-59)";
	    formats[i++] = "%n - newline";
	    //formats[i++] = "%O - modifier"; // Not supported here
	    formats[i++] = "%p - AM or PM (noon=PM, midnight=AM)";
	    formats[i++] = "%P - am or pm (noon=pm, midnight=am)";
	    //formats[i++] = "%r - time as %I:%M:%S %p";
	    formats[i++] = "%R - time as %H:%M";
	    //formats[i++] = "%s - seconds since 1970-01-01 00:00:00"; // Not supported here
	    formats[i++] = "%S - seconds (00-59)";
	    formats[i++] = "%t - tab";
	    formats[i++] = "%T - time as %H:%M:%S";
	    //formats[i++] = "%u - day of week (1-7, Monday=1)";
	    //formats[i++] = "%U - week number (00-53, 1st Sunday=week 01)";
	    //formats[i++] = "%V - week number (01-53, 1st week has > 4 days)";
	    //formats[i++] = "%w - day of week (0-6, Sunday=0)";
	    //formats[i++] = "%W - week number (00-53, 1st Monday=week 01)";
	    //formats[i++] = "%x - preferred local date";
	    //formats[i++] = "%X - preferred local time";
	    formats[i++] = "%y - year (00-99)";
	    formats[i++] = "%Y - year (YYYY)";
	    //formats[i++] = "%z - +hhmm or -hhmm offset from UTC";
	    //formats[i++] = "%Z - time zone";
	    //formats[i++] = "%+ - date(1) format";
	    formats[i++] = "%% - literal %";
	    if ( !includeDescription ) {
	        // Remove the text including and after the dash
	        for ( int j = 0; j < formats.length; j++ ) {
	            formats[j] = formats[j].substring(0,formats[j].indexOf("-")).trim();
	        }
	    }
	    return formats;
	}
	*/

	/// <summary>
	/// Return the local time zone abbreviation.
	/// The method of lookup is either LOOKUP_TIME_ZONE_ONCE (default) or
	/// LOOKUP_TIME_ZONE_ALWAYS.  The method can be changed by calling the overloaded method. </summary>
	/// <returns> the local time zone abbreviation. </returns>
	public static string getLocalTimeZoneAbbr()
	{
		return getLocalTimeZoneAbbr(_time_zone_lookup_method);
	}

	/// <summary>
	/// Return the local time zone abbreviation.  This uses java.util.TimeZone.getDefault().getID(); </summary>
	/// <param name="time_zone_lookup_method"> If LOOKUP_TIME_ZONE_ONCE or
	/// LOOKUP_TIME_ZONE_ALWAYS, this method will control how time zones are determined
	/// for future time zone lookups, including those used in DateTime construction.
	/// The default is LOOKUP_TIME_ZONE_ONCE. </param>
	/// <returns> the local time zone abbreviation. </returns>
	public static string getLocalTimeZoneAbbr(int time_zone_lookup_method)
	{
		if ((time_zone_lookup_method == LOOKUP_TIME_ZONE_ONCE) || (time_zone_lookup_method == LOOKUP_TIME_ZONE_ALWAYS))
		{
			_time_zone_lookup_method = time_zone_lookup_method;
		}
		if (_time_zone_lookup_method == LOOKUP_TIME_ZONE_ONCE)
		{
			if (!_local_time_zone_retrieved)
			{
				_local_time_zone = TimeZone.getDefault();
				_local_time_zone_string = _local_time_zone.getID(); // Use this because zones like MDT are not actually valid for some code
				// The following commented out as of TSTool 11.09.02
				/*
				//String date_as_string = new Date().toString();
				//_local_time_zone_string =
				//	date_as_string.substring(
				//	(date_as_string.length() - 8),
				//	(date_as_string.length() - 5) );
				 */
				_local_time_zone_retrieved = true;
			}
			else
			{
				return _local_time_zone_string;
			}
		}
		else if (_time_zone_lookup_method == LOOKUP_TIME_ZONE_ALWAYS)
		{
			// Look up each time...
			_local_time_zone = TimeZone.getDefault();
			_local_time_zone_string = _local_time_zone.getID(); // Use this because zones like MDT are not actually valid for some code
			// The following commented out as of TSTool 11.09.02
			/*
			String date_as_string = new Date().toString();
			_local_time_zone_string =
				date_as_string.substring(
				(date_as_string.length() - 8),
				(date_as_string.length() - 5) );
				*/
			_local_time_zone_retrieved = true;
		}
		return _local_time_zone_string;
	}

	/// <summary>
	/// Get the month and day from the day of year. </summary>
	/// <returns> A array of two integers containing the month and day given the
	/// year and Julian day within the year, or null if there is a problem. </returns>
	/// <param name="year"> Four digit year (used to determine if leap year).  If the leap
	/// year is not important, use a non-leap year (e.g., 1997). </param>
	/// <param name="julian_day"> Julian day in a year where 1 = Jan 1. </param>
	public static int [] getMonthAndDayFromDayOfYear(int year, int julian_day)
	{
		int[] month_day = new int[2];

		try
		{
			bool isleap = isLeapYear(year);
			if (julian_day > 366)
			{
				month_day[0] = 12;
				month_day[0] = 31;
			}
			else
			{
				// Loop forwards subtracting the days for each month until
				// have only one complete or partial month.
				int daysleft = julian_day;
				int monthdays;
				for (int i = 0; i < 12; i++)
				{
					if ((i == 1) && isleap)
					{
						// Set the offset to 1 to account for extra days in February...
						monthdays = MONTH_DAYS[i] + 1;
					}
					else
					{
						// All other months...
						monthdays = MONTH_DAYS[i];
					}
					if (daysleft <= monthdays)
					{
						// We have found our month...
						month_day[0] = i + 1;
						month_day[1] = daysleft;
						break;
					}
					daysleft -= monthdays;
				}
			}
			return month_day;
		}
		catch (Exception)
		{
			Message.printWarning(3,"TimeUtil.getMonthAndDayFromDayOfYear", "Error getting month and day from year " + year + " Julian day " + julian_day);
			return null;
		}
	}

	/// <summary>
	/// Return the number of intervals between two dates.  This uses a loop to count
	/// the intervals and may be slow.  For time series, a faster method may be to call
	/// the TSUtil.getDataSize method.  Zero is returned if the start date is after the end date.
	/// Zero is returned if the end date equals the start date since no interval will be traversed. </summary>
	/// <returns> The number of intervals between two dates. </returns>
	/// <param name="t1"> Start date. </param>
	/// <param name="t2"> End date. </param>
	/// <param name="base"> The time series base interval. </param>
	/// <param name="mult"> The time series interval multiplier. </param>
	public static int getNumIntervals(DateTime t1, DateTime t2, int @base, int mult)
	{
		if (t2.lessThan(t1))
		{
			Message.printWarning(3, "TimeUtil.getNumIntervals", "End " + t2 + " is before start " + t1 + ".  Returning 0.");
			return 0;
		}

		// Want to remain less than t2, so if the two dates are the same return 0.
		int intervals = 0;
		for (DateTime t = new DateTime(t1); t.lessThan(t2); t.addInterval(@base, mult))
		{
			intervals++;
		}

		return intervals;
	}

	/// <summary>
	/// Return the current system time using the default format. </summary>
	/// <returns> The current system time as a string, using the default format used by formatDateTime.
	/// REVISIT JAVADOC: see RTi.Util.Time.TimeUtil.formatDateTime </returns>
	public static string getSystemTimeString()
	{
		return formatDateTime(null, null);
	}

	/// <summary>
	/// Return the current system time using the specified format. </summary>
	/// <returns> The current system time as a string, using the specified format, as used by formatDateTime. </returns>
	/// <param name="format"> Format for date (see formatDateTime). </param>
	public static string getSystemTimeString(string format)
	{
		return formatDateTime(null, format);
	}

	/// <summary>
	/// Get the time zone offset for a time zone, for use with OffsetDateTime.of(). </summary>
	/// <param name="timeZone"> time zone string like "-07:00", "MST" or "America/Denver". </param>
	/// <returns> the zone offset, or null if it can't be found. </returns>
	public static ZoneOffset getTimeZoneOffset(string timeZone)
	{
		try
		{
			System.DateTime dt = System.DateTime.Now;
			ZoneId zone = ZoneId.of(timeZone,ZoneId.SHORT_IDS);
			ZonedDateTime zdt = dt.atZone(zone);
			return zdt.getOffset();
		}
		catch (DateTimeException)
		{
			// Time zone abbreviation is not recognized
			return null;
		}
	}

	/// <summary>
	/// Determine the year type year given a DateTime and the year type.
	/// For example, return the water or irrigation year. </summary>
	/// <param name="dt"> DateTime to examine, should be in normal calendar year </param>
	/// <param name="yt"> year type </param>
	/// <returns> the year for the requested year type </returns>
	public static int getYearForYearType(DateTime dt, YearType yt)
	{
		int y = dt.getYear();
		int m = dt.getMonth();
		if ((m >= yt.getStartMonth()) && (m <= 12))
		{
			// Month is in the start of the year
			if (yt.yearMatchesStart())
			{
				return y;
			}
			else
			{
				return y - yt.getStartYearOffset(); // Subtract because offset is negative
			}
		}
		else
		{
			// Month is in the end of the year
			if (yt.yearMatchesStart())
			{
				return y + yt.getStartYearOffset(); // Add because offset is negative
			}
			else
			{
				// Year matches the end
				return y;
			}
		}
	}

	// jan1_1800_days pre-computed by looping from 1-1799 adding numDaysInYear.
	// Code taken from HMGetDateFromInternalJulianDay1900.  The logic is pretty
	// confusing but is necessary because of the leap year checks.
	/// <summary>
	/// Convert an absolute day into its components.  This routine does not at this
	/// time handle hours or time zone. </summary>
	/// <returns> The year, month, and day in an int[3] array. </returns>
	/// <param name="aday"> Absolute day with respect to Dec 31, 1799.  The datum may change
	/// in the future and should be used only in a dynamic fashion. </param>
	/// <seealso cref= #absoluteDay </seealso>
	public static int[] getYearMonthDayFromAbsoluteDay(int aday)
	{
		string routine = "TimeUtil.getYearMonthDayFromAbsoluteDay";
		int bflag, day, dl = 10, i, id1, jd = aday, leap, month, year;

		// First guess at the year.  146097 is the number of days in 400 years,
		// accounting for leap years.  Take an initial guess at the year...

		year = (jd * 400) / 146097 + 1800;

		while (true)
		{
			leap = 1;
			if ((year % 4) != 0)
			{
				leap = 0;
			}
			if (((year % 100) == 0) && ((year % 400) != 0))
			{
				leap = 0;
			}
			id1 = 365 * (year) + year / 4 - year / 100 + year / 400 - ABSOLUTE_DAY_DATUM - leap;
			if (id1 < jd)
			{
				break;
			}
			--year;
		}
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "After first pass, found year to be: " + year);
		}

		day = jd - id1;
		while (true)
		{
			leap = 1;
			if ((year % 4) != 0)
			{
				leap = 0;
			}
			if (((year % 100) == 0) && ((year % 400) != 0))
			{
				leap = 0;
			}
			if (day <= (365 + leap))
			{
				break;
			}
			++year;
			day = day - 365 - leap;
		}
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "After second pass, found year to be: " + year);
		}

		// Find month...

		month = 0;
		if (day <= 31)
		{
			month = 1;
		}
		if (month <= 0)
		{
			bflag = 0;
			for (i = 3; i <= 12; i++)
			{
				month = i - 1;
				if (day <= (MONTH_YEARDAYS[i - 1] + leap))
				{
					bflag = 1;
					break;
				}
			}
			if (bflag == 0)
			{
				month = 12;
			}
		}
		if (Message.isDebugOn)
		{
			Message.printDebug(1, routine, "Found month to be: " + month);
		}

		//  Month known, compute day offset from month

		day -= MONTH_YEARDAYS[month - 1];
		if (month >= 3)
		{
			day -= leap;
		}
		if (Message.isDebugOn)
		{
			Message.printDebug(1, routine, "Found day to be: " + day);
		}

		int[] date_info = new int[3];
		date_info[0] = year;
		date_info[1] = month;
		date_info[2] = day;
		routine = null;
		return date_info;
	}

	private static int _offset_year = -10000;

	/// <returns> The year offset for a 4-digit year (e.g., 1900 for 1981).  If a
	/// two-digit year is passed in, the offset is determined using the current system clock. </returns>
	public static int getYearOffset()
	{
		if (_offset_year == -10000)
		{
			// This routine really only needs to be called once per run!
			// We are now using the C version...
			//string = getSystemTimeString ( "yyyy" );
			string @string = getSystemTimeString("%Y");
				//HMPrintWarning ( 2, routine,
				//"Trouble getting year offset" );
				//return HMSTATUS_FAILURE;
			int year = int.Parse(@string);
			@string = null;
			year = year - (year / 100) * 100;
			_offset_year = year;
		}
		return _offset_year;
	}

	/// <summary>
	/// Determine the highest precision (smallest interval) for two DateTime instances. </summary>
	/// <returns> the highest precision (smallest interval) for two DateTime instances
	/// (see TimeInterval precision values). </returns>
	/// <param name="datetime1"> A DateTime instance to compare. </param>
	/// <param name="datetime2"> Another DateTime instance to compare. </param>
	/// <exception cref="Exception"> if either of the dates are null or have an imprecise
	/// precision (UNKNOWN or IRREGULAR). </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int highestPrecision(DateTime datetime1, DateTime datetime2) throws Exception
	public static int highestPrecision(DateTime datetime1, DateTime datetime2)
	{
		if (datetime1 == null)
		{
			throw new Exception("First DateTime is null");
		}
		if (datetime2 == null)
		{
			throw new Exception("Second DateTime is null");
		}
		int precision1 = datetime1.getPrecision();
		if ((precision1 < TimeInterval.HSECOND) || (precision1 > TimeInterval.YEAR))
		{
			throw new Exception("Precision for first DateTime is not between hsecond and year.");
		}
		int precision2 = datetime2.getPrecision();
		if ((precision2 < TimeInterval.HSECOND) || (precision2 > TimeInterval.YEAR))
		{
			throw new Exception("Precision for second DateTime is not between hsecond and year.");
		}
		if (precision1 < precision2)
		{
			return precision1;
		}
		return precision2;
	}

	// TODO SAM 2005-11-16 Should this be deprecated in favor of the convert*Month methods?
	/// <summary>
	/// Get the irrigation month given a calendar month.  The irrigation year starts in
	/// November of the previous calendar year and ends in October of the irrigation year. </summary>
	/// <returns> the irrigation month. </returns>
	public static int irrigationMonthFromCalendar(int month)
	{
		if (month >= 11)
		{
			return (month - 10);
		}
		else
		{
			return (month + 2);
		}
	}

	// TODO SAM 2005-11-16
	// Should this be deprecated in favor of the convert*Month methods?
	/// <summary>
	/// Get the irrigation year given a calendar month and year.  The irrigation year
	/// starts in November of the previous calendar year and ends in October of the irrigation year. </summary>
	/// <returns> the irrigation year. </returns>
	public static int irrigationYearFromCalendar(int month, int year)
	{
		if (month >= 11)
		{
			return (year - 1);
		}
		else
		{
			return year;
		}
	}

	/// <summary>
	/// Return true if the string is a date/time, false otherwise.  The format must also
	/// be supplied.  The DateTime class is used to parse the date and therefore only
	/// date/time formats recognized by DateTime are recognized. </summary>
	/// <returns> true if the string is a date (can be parsed). </returns>
	/// <param name="date_string"> Date string to parse. </param>
	/// <param name="format"> Format to use for parsing (see FORMAT_*). </param>
	public static bool isDateTime(string date_string, int format)
	{
		try
		{
			DateTime.parse(date_string, format);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	/// <summary>
	/// Return true if the string is a date, false otherwise.  The format is determined
	/// from the string.  The DateTime class is used to parse the date and therefore
	/// only date/time formats recognized by DateTime are recognized. </summary>
	/// <param name="date_string"> Date string to parse. </param>
	/// <returns> true if the string is a date (can be parsed). </returns>
	public static bool isDateTime(string date_string)
	{
		try
		{
			DateTime.parse(date_string);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	/// <summary>
	/// Determine whether a year is a leap year.
	/// Leap years occur on years evenly divisible by four.
	/// However, years evenly divisible by 100 are not leap
	/// years unless they are also evenly divisible by 400. </summary>
	/// <returns> true if the specified year is a leap year and false if not. </returns>
	/// <param name="year"> 4-digit year to check. </param>
	public static bool isLeapYear(int year)
	{
		if ((((year % 4) == 0) && ((year % 100) != 0)) || (((year % 100) == 0) && ((year % 400) == 0)))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Determine whether a day is valid. </summary>
	/// <returns> true if the day is valid (in the range 1-31), false if not. </returns>
	/// <param name="day"> Day to check. </param>
	public static bool isValidDay(int day)
	{
		if ((day >= 0) && (day <= 31))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Determine whether the day is valid. </summary>
	/// <returns> true if the day is valid (in the range 1-31), false if not. </returns>
	/// <param name="day"> Day to check (an integer as a String). </param>
	public static bool isValidDay(string day)
	{
		try
		{
			int iday = int.Parse(day);
			return isValidDay(iday);
		}
		catch (Exception)
		{
			return false;
		}
	}

	/// <summary>
	/// Determine whether the day is valid. </summary>
	/// <returns> true if the day is valid (in the range 1-31), false if not. </returns>
	/// <param name="day"> Day to check. </param>
	/// <param name="month"> Month corresponding to day to check. </param>
	/// <param name="year"> Year corresponding to day to check. </param>
	public static bool isValidDay(int day, int month, int year)
	{ // First check month and year...

		if (!isValidMonth(month))
		{
			return false;
		}
		if (!isValidYear(year))
		{
			return false;
		}

		// Now check day, accounting for leap years...

		int daysinmonth = MONTH_DAYS[month - 1];
		if ((month == 2) && isLeapYear(year))
		{
			++daysinmonth;
		}
		if ((day > 0) && (day <= daysinmonth))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Determine whether a day is valid. </summary>
	/// <returns> true if the day is valid (in the range 1-31), false if not. </returns>
	/// <param name="day"> Day to check, an integer as a String. </param>
	/// <param name="month"> Month corresponding to day to check, an integer as a String. </param>
	/// <param name="year"> Year corresponding to day to check, an integer as a String. </param>
	public static bool isValidDay(string day, string month, string year)
	{
		int iday, imonth, iyear;

		try
		{
			iday = int.Parse(day);
		}
		catch (Exception)
		{
			return false;
		}
		try
		{
			imonth = int.Parse(month);
		}
		catch (Exception)
		{
			return false;
		}
		try
		{
			iyear = int.Parse(year);
		}
		catch (Exception)
		{
			return false;
		}
		return isValidDay(iday, imonth, iyear);
	}

	/// <summary>
	/// Determine whether an hour is valid. </summary>
	/// <returns> true if the hour is valid (in the range 0-23), false if not. </returns>
	/// <param name="hour"> Hour to check. </param>
	public static bool isValidHour(int hour)
	{
		if ((hour >= 0) && (hour < 24))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Determine whether an hour is valid. </summary>
	/// <returns> true if the hour is valid (in the range 0-23), false if not. </returns>
	/// <param name="hour"> Hour to check, an integer as a String. </param>
	public static bool isValidHour(string hour)
	{
		try
		{
			int ihour = int.Parse(hour);
			return isValidHour(ihour);
		}
		catch (Exception)
		{
			return false;
		}
	}

	/// <summary>
	/// Determine whether a minute is valid. </summary>
	/// <returns> true if the minute is valid (in the range 0-59), false if not. </returns>
	/// <param name="minute"> Minute to check. </param>
	public static bool isValidMinute(int minute)
	{
		if ((minute >= 0) && (minute <= 59))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Determine whether a minute is valid. </summary>
	/// <returns> true if the minute is valid (in the range 0-59), false if not. </returns>
	/// <param name="minute"> Minute to check, an integer as a String. </param>
	public static bool isValidMinute(string minute)
	{
		try
		{
			int iminute = int.Parse(minute);
			return isValidMinute(iminute);
		}
		catch (Exception)
		{
			return false;
		}
	}

	/// <summary>
	/// Determine whether a month is valid. </summary>
	/// <returns> true if the month is valid (in the range 1-12), false if not. </returns>
	/// <param name="month"> Month to check. </param>
	public static bool isValidMonth(int month)
	{
		if ((month > 0) && (month < 13))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Determine whether a month is valid. </summary>
	/// <returns> true if the month is valid (in the range 1-12), false if not. </returns>
	/// <param name="month"> Month to check, an integer as a String. </param>
	public static bool isValidMonth(string month)
	{
		try
		{
			int imonth = int.Parse(month);
			return isValidMonth(imonth);
		}
		catch (Exception)
		{
			return false;
		}
	}

	private static string[] __validTimeZones = new string[0];
	/// <summary>
	/// Determine whether a time zone is valid.
	/// This is needed because ava.util.TimeZone.getTimeZone(tz) will return GMT if the time zone is invalid. </summary>
	/// <param name="timeZone"> time zone ID such as "MST" or "America/Denver". </param>
	public static bool isValidTimeZone(string timeZone)
	{
		// Surely the time zone data are static and there is no need to store another static copy?
		// However, to improve performance, save a static array of previously validated time zones
		// First check the valid time zones.
		for (int i = 0; i < __validTimeZones.Length; i++)
		{
			if (__validTimeZones[i].Equals(timeZone))
			{
				return true;
			}
		}
		// Next check the full list
		string[] validIDs = TimeZone.getAvailableIDs();
		foreach (string str in validIDs)
		{
			if (str.Equals(timeZone))
			{
				// Add to the valid list by resizing the array and adding the new value
				string[] validTimeZones2 = new string[__validTimeZones.Length + 1];
				Array.Copy(__validTimeZones, 0, validTimeZones2, 0, __validTimeZones.Length);
				__validTimeZones = validTimeZones2;
				__validTimeZones[__validTimeZones.Length - 1] = timeZone;
				   return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Determine whether a year is valid. </summary>
	/// <returns> true if the year is valid (> 100 and not 9999), false if not. </returns>
	/// <param name="year"> Year to check. </param>
	public static bool isValidYear(int year)
	{
		if ((year < 100) || (year == 9999))
		{
			return false;
		}
		else
		{
			return true;
		}
	}

	/// <summary>
	/// Determine whether a year is valid. </summary>
	/// <returns> true if the year is valid (in the range 1-12), false if not. </returns>
	/// <param name="year"> Year to check, an integer as a String. </param>
	public static bool isValidYear(string year)
	{
		try
		{
		int iyear = int.Parse(year);
			return isValidYear(iyear);
		}
		catch (Exception)
		{
			return false;
		}
	}

	/// <summary>
	/// Determine the lowest precision (largest interval) for two DateTime instances. </summary>
	/// <returns> the lowest precision (largest interval) for two DateTime instances
	/// (see TimeInterval precision values). </returns>
	/// <param name="datetime1"> A DateTime instance to compare. </param>
	/// <param name="datetime2"> Another DateTime instance to compare. </param>
	/// <exception cref="Exception"> if either of the dates are null or have an imprecise
	/// precision (UNKNOWN or IRREGULAR). </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int lowestPrecision(DateTime datetime1, DateTime datetime2) throws Exception
	public static int lowestPrecision(DateTime datetime1, DateTime datetime2)
	{
		if (datetime1 == null)
		{
			throw new Exception("First DateTime is null");
		}
		if (datetime2 == null)
		{
			throw new Exception("Second DateTime is null");
		}
		int precision1 = datetime1.getPrecision();
		if ((precision1 < TimeInterval.HSECOND) || (precision1 > TimeInterval.YEAR))
		{
			throw new Exception("Precision for first DateTime is not between hsecond and year.");
		}
		int precision2 = datetime2.getPrecision();
		if ((precision2 < TimeInterval.HSECOND) || (precision2 > TimeInterval.YEAR))
		{
			throw new Exception("Precision for second DateTime is not between hsecond and year.");
		}
		if (precision1 > precision2)
		{
			return precision1;
		}
		return precision2;
	}

	/// <summary>
	/// Return the maximum of two DateTime instances.  If one is null, return the
	/// non-null value.  If both are null, return null.  The comparison is made as follows:
	/// <pre>
	/// if ( dt1.greaterThan(dt2) ) {
	/// return dt1;
	/// }
	/// </pre> </summary>
	/// <param name="dt1"> First DateTime to compare. </param>
	/// <param name="dt2"> Second DateTime to compare. </param>
	/// <returns> the maximum of two DateTime instances. </returns>
	public static DateTime max(DateTime dt1, DateTime dt2)
	{
		if ((dt1 == null) && (dt2 == null))
		{
			return null;
		}
		else if (dt1 == null)
		{
			return dt2;
		}
		else if (dt2 == null)
		{
			return dt1;
		}
		else if (dt1.greaterThan(dt2))
		{
			return dt1;
		}
		else
		{
			return dt2;
		}
	}

	/// <summary>
	/// Return the minimum of two DateTime instances.  If one is null, return the
	/// non-null value.  If both are null, return null.  The comparison is made as follows:
	/// <pre>
	/// if ( dt1.lessThan(dt2) ) {
	/// return dt1;
	/// }
	/// </pre> </summary>
	/// <param name="dt1"> First DateTime to compare. </param>
	/// <param name="dt2"> Second DateTime to compare. </param>
	/// <returns> the minimum of two DateTime instances. </returns>
	public static DateTime min(DateTime dt1, DateTime dt2)
	{
		if ((dt1 == null) && (dt2 == null))
		{
			return null;
		}
		else if (dt1 == null)
		{
			return dt2;
		}
		else if (dt2 == null)
		{
			return dt1;
		}
		else if (dt1.lessThan(dt2))
		{
			return dt1;
		}
		else
		{
			return dt2;
		}
	}

	/// <summary>
	/// Return a string abbreviation for the month (e.g., "Jan"). </summary>
	/// <returns> A string abbreviation for the month, or "" if not a valid month. </returns>
	/// <param name="month"> Month number, in range 1-12. </param>
	public static string monthAbbreviation(int month)
	{
		if ((month < 1) || (month > 12))
		{
			return "";
		}
		else
		{
			return MONTH_ABBREVIATIONS[month - 1];
		}
	}

	/// <summary>
	/// Determine the integer month given the month abbreviation. </summary>
	/// <returns> An integer in the range 1-12 corresponding to the month abbreviation,
	/// or 0 if the abbreviation cannot be matched. </returns>
	/// <param name="abbrev"> Month abbreviation (currently limited to 3-letter abbreviations
	/// in the MONTH_ABBREVIATIONS array). </param>
	public static int monthFromAbbrev(string abbrev)
	{
		for (int i = 0; i < 12; i++)
		{
			if (abbrev.Equals(MONTH_ABBREVIATIONS[i], StringComparison.OrdinalIgnoreCase))
			{
				return i + 1;
			}
		}
		return 0;
	}

	/// <summary>
	/// Convert an absolute month to its year and month values. </summary>
	/// <returns> An array if int's indicating the month and year corresponding to the
	/// given absolute month.  The first value will be the year, the second will be the month. </returns>
	/// <param name="amon"> Absolute month (year*12 + month). </param>
	public static int[] monthFromAbsolute(int amon)
	{
		int month, year;
		int[] monthyear = new int[2];

		monthyear[0] = 0;
		monthyear[1] = 0;

		if (amon < 0)
		{
			return monthyear;
		}
		month = amon % 12;
		year = amon / 12;
		if (month == 0)
		{
			month = 12;
			--year;
		}
		monthyear[0] = month;
		monthyear[1] = year;
		return monthyear;
	}

	/// <summary>
	/// Return the month of the year, where month 1 is the first month in the year type.
	/// For example Jan is month 1 for calendar year type and Oct is month 1 for water year type. </summary>
	/// <returns> The month of the year (where 1 is the first month of the year and 12 is the last). </returns>
	/// <param name="d"> Datetime to evaluate, in calendar year. </param>
	/// <param name="yearType"> year type. </param>
	/// <exception cref="IllegalArgumentException"> if input is invalid. </exception>
	public static int monthOfYear(DateTime d, YearType yearType)
	{
		int calMonth = d.getMonth();
		if (yearType == YearType.CALENDAR)
		{
			return calMonth;
		}
		// Else have non-calendar year so evaluation is a bit more complex
		if (calMonth < yearType.getStartMonth())
		{
			return 12 - yearType.getStartMonth() + 1 + calMonth;
		}
		else
		{
			return calMonth - yearType.getStartMonth() + 1;
		}
	}

	/// <summary>
	/// Return the number of days in a month, checking for leap year for February. </summary>
	/// <returns> The number of days in a month, or zero if an error. </returns>
	/// <param name="dt"> The DateTime object to examine. </param>
	public static int numDaysInMonth(DateTime dt)
	{
		return numDaysInMonth(dt.getMonth(), dt.getYear());
	}

	/// <summary>
	/// Return the number of days in a month, checking for leap year for February. </summary>
	/// <returns> The number of days in a month, or zero if an error. </returns>
	/// <param name="month"> The month of interest (1-12). </param>
	/// <param name="year"> The year of interest. </param>
	public static int numDaysInMonth(int month, int year)
	{
		int ndays;

		if (month < 1)
		{
			// Assume that something is messed up...
			ndays = 0;
		}
		else if (month > 12)
		{
			// Project out into the future...
			return numDaysInMonth((month % 12), (year + month / 12));
		}
		else
		{
			// Usual case...
			ndays = MONTH_DAYS[month - 1];
			if ((month == 2) && isLeapYear(year))
			{
				++ndays;
			}
		}
		return ndays;
	}

	/// <summary>
	/// Calculate the number of days in several months. </summary>
	/// <returns> The number of days in several months. </returns>
	/// <param name="month0"> The initial month of interest (1-12). </param>
	/// <param name="year0"> The initial year of interest (4-digit). </param>
	/// <param name="n"> The number of months, inclusive of the initial month.  For example, a
	/// value of 1 would return the days in the initial month.  A value of 2 would
	/// return the number of days in the initial month and its following month. </param>
	public static int numDaysInMonths(int month0, int year0, int n)
	{
		int i, month, ndays = 0, year;

		month = month0;
		year = year0;
		for (i = 0; i < n; i++)
		{
			ndays += numDaysInMonth(month, year);
			++month;
			if (month == 13)
			{
				month = 1;
				++year;
			}
		}
		return ndays;
	}

	/// <summary>
	/// Calculate the number of days in several months. </summary>
	/// <returns> The number of days in several months. </returns>
	/// <param name="month0"> The initial month of interest. </param>
	/// <param name="year0"> The initial year of interest. </param>
	/// <param name="month1"> The last month of interest. </param>
	/// <param name="year1"> The last year of interest. </param>
	public static int numDaysInMonths(int month0, int year0, int month1, int year1)
	{
		int nmonths = absoluteMonth(month1, year1) - absoluteMonth(month0, year0) + 1;
		return numDaysInMonths(month0, year0, nmonths);
	}

	/// <summary>
	/// Determine the number of days in a year. </summary>
	/// <returns> The number of days in a year, accounting for leap years. </returns>
	/// <param name="year"> The year of interest. </param>
	public static int numDaysInYear(int year)
	{
		if (isLeapYear(year))
		{
			return 366;
		}
		else
		{
			return 365;
		}
	}


	/// <summary>
	/// Parse a 4-digit military time into its hour and minute and return in an array of int's. </summary>
	/// <returns> An array of int's with the hour (index 0) and minute (index 1). </returns>
	/// <param name="time"> 4-digit military time. </param>
	public static int [] parseMilitaryTime(int time)
	{
		int[] hour_min = new int[2];
		hour_min[0] = time / 100; // the hour
		hour_min[1] = time - hour_min[0] * 100; // the minutes
		return hour_min;
	}

	/// <summary>
	/// Sleep the given number of milliseconds.  This code just loops and checks the
	/// system clock during each loop.  The thread will be tied up during the sleep. </summary>
	/// <param name="milliseconds"> The number of milliseconds to sleep. </param>
	public static void sleep(long milliseconds)
	{
		if (milliseconds == 0)
		{
			return;
		}
		// TODO SAM 2014-05-12 Figure out how to handle exception
		try
		{
			Thread.Sleep(milliseconds);
		}
		catch (InterruptedException)
		{
			//Thread.currentThread().interrupt();
		}
		/*
		// Get the current date...
		Date now = new Date ();
	
		// Loop until we have slept long enough...
	
		Date newnow;
		for ( ;; ) {
			// Get the new date...
			newnow = new Date ();
			if ( (newnow.getTime() - now.getTime()) > milliseconds ) {
				break;
			} 
		}
		*/
	}

	/// <summary>
	/// Calculate the UNIX time, which is the number of seconds since Jan 1, 1970 00:00:00.
	/// The calculation is only implemented for 1970 to 2036. </summary>
	/// <param name="dt"> DateTime containing date/time to process.  All parts of the date/time are processed, to seconds. </param>
	/// <param name="ignoreTimeZone"> currently always assumed to be true.  If true, ignore the time zone in the DateTime
	/// and use the date/time values directly to compute UNIX time, equivalent to using a time zone of GMT. </param>
	/// <returns> the UNIX time as milliseconds since Jan 1, 1970 00:00:00. </returns>
	/// <exception cref="RuntimeException"> if there is an error computing the UNIX time </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static long toUnixTime(DateTime dt, boolean ignoreTimeZone) throws RuntimeException
	public static long toUnixTime(DateTime dt, bool ignoreTimeZone)
	{
		long seconds = 0;
		int year = dt.getYear();
		if ((year < 1970) || (year > 2036))
		{
			throw new Exception("Year (" + year + ") is not between 1970 and 2036");
		}
		// For now loop, but could save a static array to improve performance
		// Consider 1970 through previous year
		for (int i = 1970; i < year; i++)
		{
			if (isLeapYear(i))
			{
				seconds = seconds + 366 * 86400;
			}
			else
			{
				seconds = seconds + 365 * 86400;
			}
		}
		// Days of year...
		int daysPrev = dt.getYearDay() - 1;
		if (daysPrev > 0)
		{
			seconds = seconds + daysPrev * 86400;
		}
		// Hours in current day
		seconds = seconds + dt.getHour() * 3600;
		// Minutes in current day
		seconds = seconds + dt.getMinute() * 60;
		// Seconds in current day
		seconds = seconds + dt.getSecond();
		// Now multiply by 1000;
		return seconds * 1000;
	}

	/// <summary>
	/// Wait for a file to exist before continuing. </summary>
	/// <param name="filename"> Name of file to check for. </param>
	/// <param name="wait"> Number of milliseconds to wait for each time. </param>
	/// <param name="numtries"> Number of times to wait. </param>
	/// <returns> true if the file exists, false if the timeout period is exceeded without
	/// the file being detected. </returns>
	public static bool waitForFile(string filename, int wait, int numtries)
	{
		for (int i = 0; i < numtries; i++)
		{
			if (IOUtil.fileExists(filename))
			{
				Message.printStatus(1, "", "File \"" + filename + "\" DOES exist at " + formatTimeString());
				return true;
			}
			else
			{
				Message.printStatus(1, "", "File \"" + filename + "\" DOES NOT exist at " + formatTimeString());
				sleep(wait);
			}
		}
		return false;
	}

	// TODO SAM 2005-11-16 Should this be deprecated in favor of the convert*Month methods?
	/// <summary>
	/// Get the water year month given a calendar month.  The water year starts in
	/// October of the previous calendar year and ends in September of the irrigation year. </summary>
	/// <returns> the water year month. </returns>
	public static int waterMonthFromCalendar(int month)
	{
		if (month >= 10)
		{
			return (month - 9);
		}
		else
		{
			return (month + 3);
		}
	}

	// TODO SAM 2005-11-16 Should this be deprecated in favor of the convert*Month methods?
	/// <summary>
	/// Get the water year given a calendar month and year.  The water year
	/// starts in October of the previous calendar year and ends in September of the irrigation year. </summary>
	/// <returns> the water year. </returns>
	public static int waterYearFromCalendar(int month, int year)
	{
		if (month >= 10)
		{
			return (year - 1);
		}
		else
		{
			return year;
		}
	}

	}

}