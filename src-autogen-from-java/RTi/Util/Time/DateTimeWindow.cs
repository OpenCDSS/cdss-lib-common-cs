﻿// DateTimeWindow - class to store a window defined by two DateTime end-points

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

namespace RTi.Util.Time
{
	/// <summary>
	/// This class stores a window defined by two DateTime end-points, which is used to specify a processing window
	/// within a year, for example to represent as season.
	/// It is NOT the same as DateTimeRange, which is a period that may span multiple years.
	/// Currently the instance is immutable and copies of the DateTime data are copied at construction.
	/// Null date/times are allowed.  Currently there is no validation done.
	/// </summary>
	public class DateTimeWindow
	{

	/// <summary>
	/// Year for the window, which can be used when parsing window strings.
	/// Using 2000 allows for a leap year to be considered.  For example, a window string MM-DD can be appended
	/// to WINDOW_YEAR + "-" to parse the date used in the window.  This is necessary because the window internally
	/// uses DateTime instances, which require a year.
	/// </summary>
	public const int WINDOW_YEAR = 2000;

	/// <summary>
	/// Starting DateTime for the window.
	/// </summary>
	private DateTime __start = null;

	/// <summary>
	/// Ending DateTime for the window.
	/// </summary>
	private DateTime __end = null;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="start"> starting date/time in the window.  Can be null to indicate open-ended window (from start
	/// of year to the specified end). </param>
	/// <param name="end"> ending date/time in the window.  Can be null to indicate open-ended range (from the specified start
	/// to the end of the year). </param>
	public DateTimeWindow(DateTime start, DateTime end)
	{
		if (start == null)
		{
			__start = null;
		}
		else
		{
			__start = new DateTime(start);
		}
		if (end == null)
		{
			__end = null;
		}
		else
		{
			__end = new DateTime(end);
		}
	}

	/// <summary>
	/// Return the ending date/time in the window (can be null) if open-ended. </summary>
	/// <returns> the ending date/time in the window (can be null) if open-ended. </returns>
	public virtual DateTime getEnd()
	{
		return __end;
	}

	/// <summary>
	/// Return the first matching date/time for a period that is within the window.  The period is
	/// iterated through and the first matching date/time in the window is returned. </summary>
	/// <param name="start"> starting date/time in a period </param>
	/// <param name="end"> ending date/time in a period </param>
	/// <param name="intervalBase"> base time interval from DateTime </param>
	/// <param name="intervalMult"> interval multiplier </param>
	/// <returns> first date/time in window for period, or null if no date/time is found </returns>
	public virtual DateTime getFirstMatchingDateTime(DateTime start, DateTime end, int intervalBase, int intervalMult)
	{
		for (DateTime d = new DateTime(start); d.lessThanOrEqualTo(end); d.addInterval(intervalBase,intervalMult))
		{
			if (isDateTimeInWindow(d))
			{
				return d;
			}
		}
		return null;
	}

	/// <summary>
	/// Return the last matching date/time for a period that is within the window.  The period is
	/// iterated through and the first matching date/time in the window is returned. </summary>
	/// <param name="start"> starting date/time in a period </param>
	/// <param name="end"> ending date/time in a period </param>
	/// <param name="intervalBase"> base time interval from DateTime </param>
	/// <param name="intervalMult"> interval multiplier </param>
	/// <returns> last date/time in window for period, or null if no date/time is found </returns>
	public virtual DateTime getLastMatchingDateTime(DateTime start, DateTime end, int intervalBase, int intervalMult)
	{
		for (DateTime d = new DateTime(end); d.greaterThanOrEqualTo(start); d.addInterval(intervalBase,-intervalMult))
		{
			if (isDateTimeInWindow(d))
			{
				return d;
			}
		}
		return null;
	}

	/// <summary>
	/// Return the starting date/time in the window (can be null) if open-ended. </summary>
	/// <returns> the starting date/time in the window (can be null) if open-ended. </returns>
	public virtual DateTime getStart()
	{
		return __start;
	}

	// TODO SAM 2015-06-21 Modify the following so DateTime instance is re-used and evaluate if it improves performance
	/// <summary>
	/// Determine whether the specified date/time is in the requested window.
	/// The DateTimeWindow year will be adjusted to the specific date before the comparison. </summary>
	/// <param name="dt"> a DateTime to compare with the window </param>
	public virtual bool isDateTimeInWindow(DateTime dt)
	{
		DateTime start = __start;
		DateTime end = __end;
		if (dt == null)
		{
			return false;
		}
		if ((start == null) && (end == null))
		{
			// No constraint...
			return true;
		}
		// Set start and end to original window but year of the date being checked
		if (start != null)
		{
			start = new DateTime(__start);
			start.setYear(dt.getYear());
			start.setPrecision(dt.getPrecision());
		}
		if (end != null)
		{
			end = new DateTime(__end);
			end.setYear(dt.getYear());
			end.setPrecision(dt.getPrecision());
		}
		bool afterStart = false;
		bool beforeEnd = false;
		if ((start == null) || dt.greaterThanOrEqualTo(start))
		{
			afterStart = true;
		}
		if ((end == null) || dt.lessThanOrEqualTo(end))
		{
			beforeEnd = true;
		}
		if (afterStart && beforeEnd)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Return string representation of window.
	/// </summary>
	public override string ToString()
	{
		return __start + " " + __end;
	}

	}

}