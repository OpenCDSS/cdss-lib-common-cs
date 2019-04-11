using System;
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
    using Message = Message.Message;

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
        public static readonly string[] MONTH_ABBREVIATIONS = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        /// <summary>
        /// Full names for months.
        /// </summary>
        public static readonly string[] MONTH_NAMES = new string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

        /// <summary>
        /// Abbreviations for days.
        /// </summary>
        public static readonly string[] DAY_ABBREVIATIONS = new string[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };

        /// <summary>
        /// Full names for months.
        /// </summary>
        public static readonly string[] DAY_NAMES = new string[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

        /// <summary>
        /// Days in months (non-leap year).
        /// </summary>
        public static readonly int[] MONTH_DAYS = new int[] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

        /// <summary>
        /// For a month, the number of days in the year passed on the first day of the
        /// month (non-leap year).
        /// </summary>
        public static readonly int[] MONTH_YEARDAYS = new int[] { 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334 };

        // Static data shared in package (so DateTime can get to easily)...

        protected internal static TimeZone _local_time_zone = null;
        protected internal static string _local_time_zone_string = "";
        protected internal static bool _local_time_zone_retrieved = false;
        protected internal static int _time_zone_lookup_method = LOOKUP_TIME_ZONE_ONCE;

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
        //public static string formatTimeString(System.DateTime d0, string format0)
        //{
        //    string routine = "TimeUtil.formatTimeString";
        //    string default_format = "%a %b %d %H:%M:%S %Z %Y";
        //    string format;
        //    int dl = 50;

        //    if (string.ReferenceEquals(format0, null))
        //    {
        //        format = default_format;
        //    }
        //    else if (format0.Length == 0)
        //    {
        //        format = default_format;
        //    }
        //    else
        //    {
        //        format = format0;
        //    }

        //    System.DateTime d;
        //    if (d0 == null)
        //    {
        //        // Get the current time...
        //        d = System.DateTime.Now;
        //    }
        //    else
        //    {
        //        d = d0;
        //    }

        //    if (Message.isDebugOn)
        //    {
        //        Message.printDebug(dl, routine, "Formatting \"" + d0 + "\" using \"" + format + "\"");
        //    }

        //    if (format.Equals("datum_seconds"))
        //    {
        //        /*
        //        ** Want the number of seconds since the standard time datum	
        //        */
        //        long seconds = d.Ticks;
        //        return Convert.ToString(seconds / 1000);
        //    }
        //    else
        //    {
        //        // Convert format to string...
        //        //

        //        // First we get a Gregorian Calendar from the date...

        //        GregorianCalendar cal = new GregorianCalendar();
        //        cal.setTime(d);
        //        int len = format.Length;
        //        StringBuilder formatted_string = new StringBuilder();
        //        char c = '\0';
        //        int ifield; // integer field value
        //        SimpleDateFormat sdf = new SimpleDateFormat();
        //        DateFormatSymbols dfs = sdf.getDateFormatSymbols();
        //        string[] short_weekdays = dfs.getShortWeekdays();
        //        string[] short_months = dfs.getShortMonths();
        //        string[] months = dfs.getMonths();
        //        string[] weekdays = dfs.getWeekdays();
        //        string[] am_pm = dfs.getAmPmStrings();
        //        // The values returned are as follows:
        //        //
        //        //		Java		We use
        //        //
        //        // Year:	since 1900	4-digit
        //        // Month:	0 to 11		1 to 12
        //        // Day:		1 to 31		1 to 31
        //        // Hour:	0 to 59		same
        //        // Minute:	0 to 59		same
        //        // Second:	0 to 59		same
        //        // DayOfWeek:	0 to 7 with 0	same
        //        //		being Sunday
        //        //		in U.S.
        //        for (int i = 0; i < len; i++)
        //        {
        //            c = format[i];
        //            if (c == '%')
        //            {
        //                // We have a format character...
        //                ++i;
        //                if (i >= len)
        //                {
        //                    break; // this will exit the whole loop
        //                }
        //                c = format[i];
        //                if (c == 'a')
        //                {
        //                    // Abbreviated weekday name.
        //                    ifield = cal.get(System.DateTime.DayOfWeek);
        //                    formatted_string.Append(short_weekdays[ifield]);
        //                    if (Message.isDebugOn)
        //                    {
        //                        Message.printDebug(dl, routine, "Day of week =" + ifield + " \"" + short_weekdays[ifield] + "\"");
        //                    }
        //                }
        //                else if (c == 'A')
        //                {
        //                    // Full weekday name.
        //                    ifield = cal.get(System.DateTime.DAY_OF_WEEK);
        //                    formatted_string.Append(weekdays[ifield]);
        //                    if (Message.isDebugOn)
        //                    {
        //                        Message.printDebug(dl, routine, "Day of week =" + ifield + " \"" + weekdays[ifield] + "\"");
        //                    }
        //                }
        //                else if (c == 'b')
        //                {
        //                    // Abbreviated month name.
        //                    ifield = cal.get(System.DateTime.MONTH);
        //                    formatted_string.Append(short_months[ifield]);
        //                    if (Message.isDebugOn)
        //                    {
        //                        Message.printDebug(dl, routine, "Month=" + ifield + " \"" + short_months[ifield] + "\"");
        //                    }
        //                }
        //                else if (c == 'B')
        //                {
        //                    // Long month name.
        //                    ifield = cal.get(System.DateTime.MONTH);
        //                    formatted_string.Append(months[ifield]);
        //                    if (Message.isDebugOn)
        //                    {
        //                        Message.printDebug(dl, routine, "Month=" + ifield + " \"" + months[ifield] + "\"");
        //                    }
        //                }
        //                else if (c == 'c')
        //                {
        //                    formatted_string.Append("%c not supported");
        //                }
        //                else if (c == 'd')
        //                {
        //                    // Day of month
        //                    ifield = cal.get(System.DateTime.DAY_OF_MONTH);
        //                    formatted_string.Append(StringUtil.formatString(ifield, "%02d"));
        //                    if (Message.isDebugOn)
        //                    {
        //                        Message.printDebug(dl, routine, "Day =" + ifield);
        //                    }
        //                }
        //                else if (c == 'H')
        //                {
        //                    // Hour of day...
        //                    ifield = cal.get(System.DateTime.HOUR_OF_DAY);
        //                    formatted_string.Append(StringUtil.formatString(ifield, "%02d"));
        //                    if (Message.isDebugOn)
        //                    {
        //                        Message.printDebug(dl, routine, "Hour=" + ifield);
        //                    }
        //                }
        //                else if (c == 'I')
        //                {
        //                    // Hour of day 1-12
        //                    ifield = cal.get(System.DateTime.HOUR);
        //                    formatted_string.Append(StringUtil.formatString(ifield, "%02d"));
        //                    if (Message.isDebugOn)
        //                    {
        //                        Message.printDebug(dl, routine, "Hour =" + ifield);
        //                    }
        //                }
        //                else if (c == 'j')
        //                {
        //                    // Day of year...
        //                    ifield = cal.get(System.DateTime.DAY_OF_YEAR);
        //                    formatted_string.Append(StringUtil.formatString(ifield, "%03d"));
        //                    if (Message.isDebugOn)
        //                    {
        //                        Message.printDebug(dl, routine, "DayofYear=" + ifield);
        //                    }
        //                }
        //                else if (c == 'm')
        //                {
        //                    // Month of year...
        //                    ifield = cal.get(System.DateTime.MONTH) + 1;
        //                    formatted_string.Append(StringUtil.formatString(ifield, "%02d"));
        //                    if (Message.isDebugOn)
        //                    {
        //                        Message.printDebug(dl, routine, "Month =" + ifield);
        //                    }
        //                }
        //                else if (c == 'M')
        //                {
        //                    // Minute of hour...
        //                    ifield = cal.get(System.DateTime.MINUTE);
        //                    formatted_string.Append(StringUtil.formatString(ifield, "%02d"));
        //                    if (Message.isDebugOn)
        //                    {
        //                        Message.printDebug(dl, routine, "Minute =" + ifield);
        //                    }
        //                }
        //                else if (c == 'p')
        //                {
        //                    // AM or PM...
        //                    ifield = cal.get(System.DateTime.AM_PM);
        //                    formatted_string.Append(am_pm[ifield]);
        //                    if (Message.isDebugOn)
        //                    {
        //                        Message.printDebug(dl, routine, "AM/PM=" + ifield + " \"" + am_pm[ifield] + "\"");
        //                    }
        //                }
        //                else if (c == 'S')
        //                {
        //                    // Seconds of minute...
        //                    ifield = cal.get(System.DateTime.SECOND);
        //                    formatted_string.Append(StringUtil.formatString(ifield, "%02d"));
        //                    if (Message.isDebugOn)
        //                    {
        //                        Message.printDebug(dl, routine, "Second =" + ifield);
        //                    }
        //                }
        //                else if ((c == 'U') || (c == 'W'))
        //                {
        //                    // Week of year...
        //                    // Don't worry now about whether Sunday or Monday are the start of the week.
        //                    ifield = cal.get(System.DateTime.WEEK_OF_YEAR);
        //                    formatted_string.Append(StringUtil.formatString(ifield, "%02d"));
        //                    if (Message.isDebugOn)
        //                    {
        //                        Message.printDebug(dl, routine, "Weekofyear =" + ifield);
        //                    }
        //                }
        //                else if (c == 'x')
        //                {
        //                    formatted_string.Append("%x not supported");
        //                }
        //                else if (c == 'X')
        //                {
        //                    formatted_string.Append("%X not supported");
        //                }
        //                else if (c == 'y')
        //                {
        //                    // Two digit year...
        //                    ifield = cal.get(System.DateTime.YEAR);
        //                    if (ifield > 60000)
        //                    {
        //                        // Borland database bug...
        //                        ifield -= 65536;
        //                    }
        //                    ifield = formatYear(ifield, 2, true);
        //                    formatted_string.Append(StringUtil.formatString(ifield, "%02d"));
        //                    if (Message.isDebugOn)
        //                    {
        //                        Message.printDebug(dl, routine, "year =" + ifield);
        //                    }
        //                }
        //                else if (c == 'Y')
        //                {
        //                    // Four digit year...
        //                    ifield = cal.get(System.DateTime.YEAR);
        //                    if (ifield > 60000)
        //                    {
        //                        // Borland database bug...
        //                        ifield -= 65536;
        //                    }
        //                    ifield = formatYear(ifield, 4, true);
        //                    formatted_string.Append(StringUtil.formatString(ifield, "%04d"));
        //                    if (Message.isDebugOn)
        //                    {
        //                        Message.printDebug(dl, routine, "Year =" + ifield);
        //                    }
        //                }
        //                else if (c == 'Z')
        //                {
        //                    // Time zone offset from GMT to local time, in milliseconds...
        //                    formatted_string.Append(cal.getTimeZone().getID());
        //                }
        //                else if (c == '%')
        //                {
        //                    // Literal percent...
        //                    formatted_string.Append('%');
        //                }
        //            }
        //            else
        //            {
        //                // Just add the character to the string...
        //                formatted_string.Append(c);
        //            }
        //        }
        //        return formatted_string.ToString();
        //    }
        //}

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
        /// Return the absolute month, which is the year*12 + month. </summary>
        /// <returns> The absolute month, which is the year*12 + month. </returns>
        /// <param name="month"> Month number. </param>
        /// <param name="year"> Year. </param>
        public static int absoluteMonth(int month, int year)
        {
            return (year * 12 + month);
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
    }
}
