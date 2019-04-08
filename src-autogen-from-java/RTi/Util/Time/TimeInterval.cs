using System;
using System.Collections.Generic;

// TimeInterval - time interval class

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
// TimeInterval - time interval class
// ----------------------------------------------------------------------------
// History:
//
// 22 Sep 1997	Steven A. Malers, RTi	First version.
// 13 Apr 1999	SAM, RTi		Add finalize.
// 01 May 2001	SAM, RTi		Add toString(), compatible with C++.
//					Add equals().
// 2001-11-06	SAM, RTi		Review javadoc.  Verify that variables
//					are set to null when no longer used.
//					Change set methods to have void return
//					type.
// 2001-12-13	SAM, RTi		Copy TSInterval to TimeInterval and
//					make changes to make the class more
//					generic.  parseInterval() now throws an
//					exception if unable to parse.
// 2001-04-19	SAM, RTi		Add constructor to take integer base and
//					multiplier.
// 2002-05-30	SAM, RTi		Add getMultiplierString() to better
//					support exact lookups of interval parts
//					(e.g., for database queries that require
//					parts).
// 2003-05-30	SAM, RTi		Add multipliersForIntervalBase()
//					to return reasonable multipliers for a
//					base string.  Add support for seconds
//					in parseInterval().
// 2003-10-27	SAM, RTi		Add UNKNOWN for time interval. 
// 2005-02-16	SAM, RTi		Add getTimeIntervalChoices() to
//					facilitate use in interfaces.
// 2005-03-03	SAM, RTi		Add lessThan(), lessThanOrEquivalent(),
//					greaterThan(),greaterThanOrEquivalent(),
//					equivalent().  REVISIT - only put in the
//					comments but did not implement.
// 2005-08-26	SAM, RTi		Overload getTimeIntervalChoices() to
//					include Irregular.
// 2005-09-27	SAM, RTi		Add isRegularInterval().
// 2006-04-24	SAM, RTi		Change parseInterval() to throw an
//					InvalidTimeIntervalException if the
//					interval is not recognized.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------
// EndHeader

namespace RTi.Util.Time
{

	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// The TimeInterval class provide methods to convert intervals from
	/// integer to string representations.  Common usage is to call the parseInterval()
	/// method to parse a string and then use the integer values to increase
	/// performance.  The TimeInterval data members can be used when creating DateTime instances.
	/// A lookup of string interval names from the integer values may not return
	/// exactly the string that is allowed in a parse (due to case being ignored, etc.).
	/// </summary>
	public class TimeInterval
	{
	/// <summary>
	/// Time interval base values.  These intervals are guaranteed to have values less
	/// than 256 (this should allow for addition of other intervals if necessary).  The
	/// interval values may change in the future.  The values assigned to intervals
	/// increase with the magnitude of the interval (e.g., YEAR > MONTH).  Only irregular has no place in
	/// the order.  Flags above >= 256 are reserved for DateTime constructor flags.
	/// These values are set as the DateTime.PRECISION* values to maintain consistency.
	/// </summary>
	public const int UNKNOWN = -1; // Unknown, e.g., for initialization
	public const int IRREGULAR = 0;
	public const int HSECOND = 5;
	public const int SECOND = 10;
	public const int MINUTE = 20;
	public const int HOUR = 30;
	public const int DAY = 40;
	public const int WEEK = 50;
	public const int MONTH = 60;
	public const int YEAR = 70;

	/// <summary>
	/// The string associated with the base interval (e.g, "Month").
	/// </summary>
	private string __intervalBaseString;
	/// <summary>
	/// The string associated with the interval multiplier (may be "" if
	/// not specified in string used with the constructor).
	/// </summary>
	private string __intervalMultString;
	/// <summary>
	/// The base data interval.
	/// </summary>
	private int __intervalBase;
	/// <summary>
	/// The data interval multiplier.
	/// </summary>
	private int __intervalMult;

	/// <summary>
	/// Construct and initialize data to zeros and empty strings.
	/// </summary>
	public TimeInterval()
	{
		init();
	}

	/// <summary>
	/// Copy constructor. </summary>
	/// <param name="interval"> TSInterval to copy. </param>
	public TimeInterval(TimeInterval interval)
	{
		init();
		__intervalBase = interval.getBase();
		__intervalMult = interval.getMultiplier();
		__intervalBaseString = interval.getBaseString();
		__intervalMultString = interval.getMultiplierString();
	}

	/// <summary>
	/// Constructor from the integer base and multiplier.  The string base name is
	/// set to defaults.  The multiplier is not relevant if the base is IRREGULAR, although in the future
	/// intervals like IrregMonth may be allowed. </summary>
	/// <param name="base"> Interval base. </param>
	/// <param name="mult"> Interval multiplier.  If set to <= 0, the multiplier string returned
	/// from getMultiplierString() will be set to "" and the integer multiplier will be set to 1. </param>
	public TimeInterval(int @base, int mult)
	{
		init();
		__intervalBase = @base;
		__intervalMult = mult;
		__intervalBaseString = getName(@base);
		if (__intervalBase == IRREGULAR)
		{
			__intervalMultString = "";
		}
		else if (mult <= 0)
		{
			__intervalMultString = "";
			__intervalMult = 1;
		}
		else
		{
			__intervalMultString = "" + mult;
		}
	}

	/// <summary>
	/// Determine if two instances are equal.  The base and multiplier are checked.  This
	/// method does not check for cases like 60Minute = 1Hour (false will be returned).
	/// Instead use equivalent(), lessThanOrEqualTo(), or greterThanOrEqualTo(). </summary>
	/// <param name="interval"> TimeInterval to compare. </param>
	/// <returns> true if the integer interval base and multiplier are equal, false otherwise. </returns>
	public virtual bool Equals(TimeInterval interval)
	{
		if ((__intervalBase == interval.getBase()) && (__intervalMult == interval.getMultiplier()))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Determine if two instances are equal.  The base and multiplier are checked.  This
	/// method does not check for cases like 60Minute = 1Hour (false will be returned).
	/// Instead use equivalent(), lessThanOrEqualTo(), or greterThanOrEqualTo().
	/// Makes sure the object passed in is a TimeInterval and then calls equals(TimeInterval). </summary>
	/// <param name="o"> an Object to compare with </param>
	/// <returns> true if the object is a time interval and if the integer interval base 
	/// and multiplier are equal, false otherwise. </returns>
	public override bool Equals(object o)
	{
		bool eq = o is TimeInterval;
		if (eq)
		{
			eq = Equals((TimeInterval) o);
		}
		return eq;
	}

	/*
	Determine if two instances are equivalent.  The intervals are equivalent if the
	interval information matches exactly and in cases like the following:
	60Minute = 1Hour.
	@param interval TimeInterval to compare.
	@return true if the integer interval base and multiplier are equivalent, false
	otherwise.
	*/
	/* TODO SAM 2005-03-03 Need to implement when there is time
	public boolean equivalent ( TimeInterval interval )
	{	// Do simple check...
		if ( equals(interval) ) {
			return true;
		}
		// Else check for equivalence...
	}
	*/

	/// <summary>
	/// Finalize before garbage collection. </summary>
	/// <exception cref="Throwable"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~TimeInterval()
	{
		__intervalBaseString = null;
		__intervalMultString = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the interval base (see TimeInterval.INTERVAL*). </summary>
	/// <returns> The interval base (see TimeInterval.INTERVAL*). </returns>
	public virtual int getBase()
	{
		return __intervalBase;
	}

	/// <summary>
	/// Return the interval base as a string. </summary>
	/// <returns> The interval base as a string. </returns>
	public virtual string getBaseString()
	{
		return __intervalBaseString;
	}

	/// <summary>
	/// Return the interval multiplier. </summary>
	/// <returns> The interval multiplier. </returns>
	public virtual int getMultiplier()
	{
		return __intervalMult;
	}

	/// <summary>
	/// Return the interval base as a string. </summary>
	/// <returns> The interval base as a string. </returns>
	public virtual string getMultiplierString()
	{
		return __intervalMultString;
	}

	/// <summary>
	/// Look up an interval name as a string (e.g., "MONTH").  Note that the string is
	/// upper-case.  Call the version with the format if other format is desired. </summary>
	/// <returns> The interval string, or an empty string if not found. </returns>
	/// <param name="interval"> Time series interval to look up). </param>
	/// @deprecated the version that specifies format should be used. 
	public static string getName(int interval)
	{
		return getName(interval, 1); // Historical default
	}

	/// <summary>
	/// Look up an interval name as a string (e.g., "MONTH").  Note that the string is
	/// upper-case.  Convert the 2nd+ characters to lower-case if necessary. </summary>
	/// <returns> The interval string, or an empty string if not found. </returns>
	/// <param name="interval"> Time series interval to look up). </param>
	/// <param name="format"> if -1 return lowercase (e.g., "day"),
	/// if 0 return mixed/camel case (e.g., "Day", which will be useful if additional irregular data interval strings are supported),
	/// if 1 return uppercase (e.g., "DAY"). </param>
	public static string getName(int interval, int format)
	{
		string name = "";
		if (interval == YEAR)
		{
			name = "Year";
		}
		else if (interval == MONTH)
		{
			name = "Month";
		}
		else if (interval == WEEK)
		{
			name = "Week";
		}
		else if (interval == DAY)
		{
			name = "Day";
		}
		else if (interval == HOUR)
		{
			name = "Hour";
		}
		else if (interval == MINUTE)
		{
			name = "Minute";
		}
		else if (interval == SECOND)
		{
			name = "Second";
		}
		else if (interval == HSECOND)
		{
			name = "Hsecond";
		}
		else if (interval == IRREGULAR)
		{
			name = "Irregular";
		}
		if (format > 0)
		{
			// Legacy default
			return name.ToUpper();
		}
		else if (format == 0)
		{
			// Trying to move to this as default
			return name;
		}
		else
		{
			return name.ToLower();
		}
	}

	/// <summary>
	/// Return a list of interval strings (e.g., "Year", "6Hour").  Only evenly
	/// divisible choices are returned (no "5Hour" because it does not divide into the
	/// day).  This version does NOT include the Irregular time step. </summary>
	/// <returns> a list of interval strings. </returns>
	/// <param name="start_interval"> The starting (smallest) interval base to return. </param>
	/// <param name="end_interval"> The ending (largest) interval base to return. </param>
	/// <param name="pad_zeros"> If true, pad the strings with zeros (e.g., "06Hour").  If false do not pad (e.g., "6Hour"). </param>
	/// <param name="sort_order"> Specify zero or 1 to sort ascending (small interval to large), -1 to sort descending. </param>
	public static IList<string> getTimeIntervalChoices(int start_interval, int end_interval, bool pad_zeros, int sort_order)
	{
		return getTimeIntervalChoices(start_interval, end_interval, pad_zeros, sort_order, false);
	}

	/// <summary>
	/// Return a list of base time interval strings (e.g., "Year", "Hour"), optionally
	/// including the Irregular time step.  No multipliers are prefixed on the time intervals. </summary>
	/// <returns> a list of interval strings. </returns>
	/// <param name="startInterval"> The starting (smallest) interval base to return. </param>
	/// <param name="endInterval"> The ending (largest) interval base to return. </param>
	/// <param name="sortOrder"> Specify zero or 1 to sort ascending (small interval to large), -1 to sort descending. </param>
	/// <param name="includeIrregular"> Indicate whether the "Irregular" time step should be
	/// included.  If included, "Irregular" is always at the end of the list. </param>
	public static IList<string> getTimeIntervalBaseChoices(int startInterval, int endInterval, int sortOrder, bool includeIrregular)
	{ // Add in ascending order and sort to descending later if requested...
		IList<string> v = new List<string>();
		if (startInterval > endInterval)
		{
			// Swap (only rely on sort_order for ordering)...
			int temp = endInterval;
			endInterval = startInterval;
			startInterval = temp;
		}
		if ((HSECOND >= startInterval) && (HSECOND <= endInterval))
		{
			// TODO SAM 2005-02-16 We probably don't need to support this at all.
		}
		if ((SECOND >= startInterval) && (SECOND <= endInterval))
		{
			v.Add("Second");
		}
		if ((MINUTE >= startInterval) && (MINUTE <= endInterval))
		{
			v.Add("Minute");
		}
		if ((HOUR >= startInterval) && (HOUR <= endInterval))
		{
			v.Add("Hour");
		}
		if ((DAY >= startInterval) && (DAY <= endInterval))
		{
			v.Add("Day");
		}
		// TODO SAM 2005-02-16 Week is not yet supported
		//if ( (WEEK >= start_interval) && (WEEK <= end_interval) ) {
		//}
		if ((MONTH >= startInterval) && (MONTH <= endInterval))
		{
			v.Add("Month");
		}
		if ((YEAR >= startInterval) && (YEAR <= endInterval))
		{
			v.Add("Year");
		}
		if (sortOrder >= 0)
		{
			if (includeIrregular)
			{
				v.Add("Irregular");
			}
			return v;
		}
		else
		{
			// Change to descending order...
			int size = v.Count;
			IList<string> v2 = new List<string> (size);
			for (int i = size -1; i >= 0; i--)
			{
				v2.Add(v[i]);
			}
			if (includeIrregular)
			{
				v2.Add("Irregular");
			}
			return v2;
		}
	}

	/// <summary>
	/// Return a list of interval strings (e.g., "Year", "6Hour"), optionally
	/// including the Irregular time step.  Only evenly divisible choices are returned
	/// (no "5Hour" because it does not divide into the day). </summary>
	/// <returns> a list of interval strings. </returns>
	/// <param name="start_interval"> The starting (smallest) interval base to return. </param>
	/// <param name="end_interval"> The ending (largest) interval base to return. </param>
	/// <param name="pad_zeros"> If true, pad the strings with zeros (e.g., "06Hour").  If false
	/// do not pad (e.g., "6Hour"). </param>
	/// <param name="sort_order"> Specify zero or 1 to sort ascending (small interval to large), -1 to sort descending. </param>
	/// <param name="include_irregular"> Indicate whether the "Irregular" time step should be
	/// included.  If included, "Irregular" is always at the end of the list. </param>
	public static IList<string> getTimeIntervalChoices(int start_interval, int end_interval, bool pad_zeros, int sort_order, bool include_irregular)
	{ // Add in ascending order and sort to descending later if requested...
		IList<string> v = new List<string>();
		if (start_interval > end_interval)
		{
			// Swap (only rely on sort_order for ordering)...
			int temp = end_interval;
			end_interval = start_interval;
			start_interval = temp;
		}
		if ((HSECOND >= start_interval) && (HSECOND <= end_interval))
		{
			// TODO SAM 2005-02-16 We probably don't need to support this at all.
		}
		if ((SECOND >= start_interval) && (SECOND <= end_interval))
		{
			v.Add("Second");
			if (pad_zeros)
			{
				v.Add("01Second");
				v.Add("02Second");
				v.Add("03Second");
				v.Add("04Second");
				v.Add("05Second");
				v.Add("06Second");
			}
			else
			{
				v.Add("1Second");
				v.Add("2Second");
				v.Add("3Second");
				v.Add("4Second");
				v.Add("5Second");
				v.Add("6Second");
			}
			v.Add("10Second");
			v.Add("15Second");
			v.Add("20Second");
			v.Add("30Second");
			v.Add("60Second");
		}
		if ((MINUTE >= start_interval) && (MINUTE <= end_interval))
		{
			v.Add("Minute");
			if (pad_zeros)
			{
				v.Add("01Minute");
				v.Add("02Minute");
				v.Add("03Minute");
				v.Add("04Minute");
				v.Add("05Minute");
				v.Add("06Minute");
			}
			else
			{
				v.Add("1Minute");
				v.Add("2Minute");
				v.Add("3Minute");
				v.Add("4Minute");
				v.Add("5Minute");
				v.Add("6Minute");
			}
			v.Add("10Minute");
			v.Add("15Minute");
			v.Add("20Minute");
			v.Add("30Minute");
			v.Add("60Minute");
		}
		if ((HOUR >= start_interval) && (HOUR <= end_interval))
		{
			v.Add("Hour");
			if (pad_zeros)
			{
				v.Add("01Hour");
				v.Add("02Hour");
				v.Add("03Hour");
				v.Add("04Hour");
				v.Add("06Hour");
				v.Add("08Hour");
			}
			else
			{
				v.Add("1Hour");
				v.Add("2Hour");
				v.Add("3Hour");
				v.Add("4Hour");
				v.Add("6Hour");
				v.Add("8Hour");
			}
			v.Add("12Hour");
			// Add this because often hourly data aggregate up to 24-hour data.
			v.Add("24Hour");
		}
		if ((DAY >= start_interval) && (DAY <= end_interval))
		{
			v.Add("Day");
		}
		// TODO SAM 2005-02-16 Week is not yet supported
		//if ( (WEEK >= start_interval) && (WEEK <= end_interval) ) {
		//}
		if ((MONTH >= start_interval) && (MONTH <= end_interval))
		{
			v.Add("Month");
		}
		if ((YEAR >= start_interval) && (YEAR <= end_interval))
		{
			v.Add("Year");
		}
		if (sort_order >= 0)
		{
			if (include_irregular)
			{
				v.Add("Irregular");
			}
			return v;
		}
		else
		{
			// Change to descending order...
			int size = v.Count;
			IList<string> v2 = new List<string>(size);
			for (int i = size -1; i >= 0; i--)
			{
				v2.Add(v[i]);
			}
			if (include_irregular)
			{
				v2.Add("Irregular");
			}
			return v2;
		}
	}

	/*
	Determine whether the given TimeInterval is greater than the instance based on
	a comparison of the length of the interval.
	Only intervals that can be explicitly compared should be evaluated with this method.
	@return true if the instance is greater than the given TimeInterval.
	@param interval The TimeInterval to compare to the instance.
	*/
	/* TODO 2005-03-03 SAM do later no time.
	public boolean greaterThan ( TimeInterval interval )
	{	int seconds1 = toSeconds();
		int seconds2 = interval.toSeconds();
		if ( (seconds1 >= 0) && (seconds2 >= 0) ) {
			// Intervals are less than month so a simple comparison can be made...
			if ( seconds1 > seconds2 ) {
				return true;
			}
			else {
			    return false;
			}
		}
	}
	*/

	/// <summary>
	/// Determine whether the interval is regular. </summary>
	/// <param name="intervalBase"> the time interval base to check </param>
	/// <returns> true if the interval is regular, false if not (unknown or irregular). </returns>
	public virtual bool isRegularInterval()
	{
		if ((__intervalBase >= HSECOND) && (__intervalBase <= YEAR))
		{
			return true;
		}
		// Irregular and unknown are what are left.
		return false;
	}

	/// <summary>
	/// Determine whether an interval is regular. </summary>
	/// <param name="intervalBase"> the time interval base to check </param>
	/// <returns> true if the interval is regular, false if not (unknown or irregular). </returns>
	public static bool isRegularInterval(int intervalBase)
	{
		if ((intervalBase >= HSECOND) && (intervalBase <= YEAR))
		{
			return true;
		}
		// Irregular and unknown are what are left.
		return false;
	}

	// TODO need to put in lessThanOrEquivalent()

	/*
	Determine whether the given TimeInterval is less than the instance based on
	a comparison of the length of the interval, for intervals with base Second to Year.
	For the sake of comparing largely different intervals, months are assumed to
	have 30 days.  Time intervals of 28Day, 29Day, and 31Day will explicitly be
	treated as 1Month.  Comparisons for intervals < Month are done using the number
	of seconds in the interval.  Comparisons
	@return true if the instance is less than the given TimeInterval.
	@param interval The TimeInterval to compare to the instance.
	*/
	/* TODO SAM 2005-03-03 No time - do later.
	public boolean lessThan ( TimeInterval interval )
	{	int seconds1 = toSecondsApproximate();
		int seconds2 = interval.toSecondsApproximate();
		if ( (seconds1 >= 0) && (seconds2 >= 0) ) {
			// Intervals are less than month so a simple comparison can be made...
			if ( seconds1 < seconds2 ) {
				return true;
			}
			else {
			    return false;
			}
		}
		// Check comparison between intervals involving only month and year...
		int base1 = interval.getBase();
		int mult1 = interval.getMultiplier();
		int base2 = interval.getBase();
		int mult2 = interval.getMultiplier();
	}
	*/

	// TODO need to put in lessThanOrEquivalent()

	/// <summary>
	/// Initialize the data.
	/// </summary>
	private void init()
	{
		__intervalBase = 0;
		__intervalBaseString = "";
		__intervalMult = 0;
		__intervalMultString = "";
	}

	/// <summary>
	/// Determine the time interval multipliers for a base interval string.
	/// This is useful in code where the user picks the interval base and a reasonable
	/// multiplier is given as a choice.  Currently some general decisions are made.
	/// For example, year, week, and regular interval always returns a multiple of 1.
	/// Day interval always returns 1-31 and should be limited by the calling code
	/// based on a specific month, if necessary.
	/// Note that this method returns valid interval multipliers, which may not be the
	/// same as the maximum value for a date/time component.  For example, the maximum
	/// multiplier for an hourly interval is 24 whereas the maximum hour value is 23. </summary>
	/// <returns> an array of multipliers for for the interval string, or null if the
	/// interval_base string is not recognized. </returns>
	/// <param name="interval_base"> An interval base string that is recognized by parseInterval(). </param>
	/// <param name="divisible"> If true, the interval multipliers that are returned will result
	/// in intervals that divide evenly into the next interval base.  If false, then all
	/// valid multipliers for the base are returned. </param>
	/// <param name="include_zero"> If true, then a zero multiplier is included with all
	/// returned output.  Normally zero is not included. </param>
	public static int [] multipliersForIntervalBase(string interval_base, bool divisible, bool include_zero)
	{
		TimeInterval interval = null;
		try
		{
			interval = parseInterval(interval_base);
		}
		catch (Exception)
		{
			return null;
		}
		int @base = interval.getBase();
		int[] mult = null;
		int offset = 0; // Used when include_zero is true
		int size = 0;
		if (include_zero)
		{
			offset = 1;
		}
		if (@base == YEAR)
		{
			mult = new int[1 + offset];
			if (include_zero)
			{
				mult[0] = 0;
			}
			mult[offset] = 1;
		}
		else if (@base == MONTH)
		{
			if (divisible)
			{
				mult = new int[6 + offset];
				if (include_zero)
				{
					mult[0] = 0;
				}
				mult[offset] = 1;
				mult[1 + offset] = 2;
				mult[2 + offset] = 3;
				mult[3 + offset] = 4;
				mult[4 + offset] = 6;
				mult[5 + offset] = 12;
			}
			else
			{
				size = 12 + offset;
				mult = new int[size];
				if (include_zero)
				{
					mult[0] = 0;
				}
				for (int i = 0; i < 12; i++)
				{
					mult[i + offset] = i + 1;
				}
			}
		}
		else if (@base == WEEK)
		{
			mult = new int[1 + offset];
			if (include_zero)
			{
				mult[0] = 0;
			}
			mult[offset] = 1;
		}
		else if (@base == DAY)
		{
			size = 31 + offset;
			mult = new int[size];
			if (include_zero)
			{
				mult[0] = 0;
			}
			for (int i = 0; i < 31; i++)
			{
				mult[i + offset] = i + 1;
			}
		}
		else if (@base == HOUR)
		{
			if (divisible)
			{
				size = 8 + offset;
				mult = new int[size];
				if (include_zero)
				{
					mult[0] = 0;
				}
				mult[offset] = 1;
				mult[1 + offset] = 2;
				mult[2 + offset] = 3;
				mult[3 + offset] = 4;
				mult[4 + offset] = 6;
				mult[5 + offset] = 8;
				mult[6 + offset] = 12;
				mult[7 + offset] = 24;
			}
			else
			{
				size = 24 + offset;
				mult = new int[size];
				if (include_zero)
				{
					mult[0] = 0;
				}
				for (int i = 0; i < 24; i++)
				{
					mult[i + offset] = i + 1;
				}
			}
		}
		else if ((@base == MINUTE) || (@base == SECOND))
		{
			if (divisible)
			{
				size = 12 + offset;
				mult = new int[size];
				if (include_zero)
				{
					mult[0] = 0;
				}
				mult[offset] = 1;
				mult[1 + offset] = 2;
				mult[2 + offset] = 3;
				mult[3 + offset] = 4;
				mult[4 + offset] = 5;
				mult[5 + offset] = 6;
				mult[6 + offset] = 10;
				mult[7 + offset] = 12;
				mult[8 + offset] = 15;
				mult[9 + offset] = 20;
				mult[10 + offset] = 30;
				mult[11 + offset] = 60;
			}
			else
			{
				size = 60 + offset;
				mult = new int[size];
				if (include_zero)
				{
					mult[0] = 0;
				}
				for (int i = 0; i < 60; i++)
				{
					mult[i + offset] = i + 1;
				}
			}
		}
		else if (@base == HSECOND)
		{
			if (divisible)
			{
				size = 8 + offset;
				mult = new int[size];
				if (include_zero)
				{
					mult[0] = 0;
				}
				mult[offset] = 1;
				mult[1 + offset] = 2;
				mult[2 + offset] = 4;
				mult[3 + offset] = 5;
				mult[4 + offset] = 10;
				mult[5 + offset] = 20;
				mult[6 + offset] = 50;
				mult[7 + offset] = 100;
			}
			else
			{
				size = 100 + offset;
				mult = new int[size];
				if (include_zero)
				{
					mult[0] = 0;
				}
				for (int i = 0; i < 100; i++)
				{
					mult[i + offset] = i + 1;
				}
			}
		}
		else if (@base == IRREGULAR)
		{
			mult = new int[1];
			mult[0] = 1;
		}
		return mult;
	}

	/// <summary>
	/// Parse an interval string like "6Day" into its parts and return as a
	/// TimeInterval.  If the multiplier is not specified, the value returned from
	/// getMultiplierString() will be "", even if the getMultiplier() is 1. </summary>
	/// <returns> The TimeInterval that is parsed from the string. </returns>
	/// <param name="intervalString"> Time series interval as a string, containing an
	/// interval string and an optional multiplier. </param>
	/// <exception cref="InvalidTimeIntervalException"> if the interval string cannot be parsed. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static TimeInterval parseInterval(String intervalString) throws InvalidTimeIntervalException
	public static TimeInterval parseInterval(string intervalString)
	{
		string routine = "TimeInterval.parseInterval";
		int digitCount = 0; // Count of digits at start of the interval string
		int dl = 50;
		int i = 0;
		int length = intervalString.Length;

		TimeInterval interval = new TimeInterval();

		// Need to strip of any leading digits.

		while (i < length)
		{
			if (char.IsDigit(intervalString[i]))
			{
				digitCount++;
				i++;
			}
			else
			{
				// We have reached the end of the digit part of the string.
				break;
			}
		}

		if (digitCount == 0)
		{
			//
			// The string had no leading digits, interpret as one.
			//
			interval.setMultiplier(1);
		}
		else if (digitCount == length)
		{
			//
			// The whole string is a digit, default to hourly (legacy behavior)
			//
			interval.setBase(HOUR);
			interval.setMultiplier(int.Parse(intervalString));
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, interval.getMultiplier() + " Hourly");
			}
			return interval;
		}
		else
		{
			string intervalMultString = intervalString.Substring(0,digitCount);
			interval.setMultiplier(int.Parse((intervalMultString)));
			interval.setMultiplierString(intervalMultString);
		}

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Multiplier: " + interval.getMultiplier());
		}

		// Now parse out the Base interval

		string intervalBaseString = intervalString.Substring(digitCount).Trim();
		string intervalBaseStringUpper = intervalBaseString.ToUpper();
		if (intervalBaseStringUpper.StartsWith("MIN", StringComparison.Ordinal))
		{
			interval.setBaseString(intervalBaseString);
			interval.setBase(MINUTE);
		}
		else if (intervalBaseStringUpper.StartsWith("HOUR", StringComparison.Ordinal) || intervalBaseStringUpper.StartsWith("HR", StringComparison.Ordinal))
		{
			interval.setBaseString(intervalBaseString);
			interval.setBase(HOUR);
		}
		else if (intervalBaseStringUpper.StartsWith("DAY", StringComparison.Ordinal) || intervalBaseStringUpper.StartsWith("DAI", StringComparison.Ordinal))
		{
			interval.setBaseString(intervalBaseString);
			interval.setBase(DAY);
		}
		else if (intervalBaseStringUpper.StartsWith("SEC", StringComparison.Ordinal))
		{
			interval.setBaseString(intervalBaseString);
			interval.setBase(SECOND);
		}
		else if (intervalBaseStringUpper.StartsWith("WEEK", StringComparison.Ordinal) || intervalBaseStringUpper.StartsWith("WK", StringComparison.Ordinal))
		{
			interval.setBaseString(intervalBaseString);
			interval.setBase(WEEK);
		}
		else if (intervalBaseStringUpper.StartsWith("MON", StringComparison.Ordinal))
		{
			interval.setBaseString(intervalBaseString);
			interval.setBase(MONTH);
		}
		else if (intervalBaseStringUpper.StartsWith("YEAR", StringComparison.Ordinal) || intervalBaseStringUpper.StartsWith("YR", StringComparison.Ordinal))
		{
			interval.setBaseString(intervalBaseString);
			interval.setBase(YEAR);
		}
		else if (intervalBaseStringUpper.StartsWith("IRR", StringComparison.Ordinal))
		{
			interval.setBaseString(intervalBaseString);
			interval.setBase(IRREGULAR);
		}
		else
		{
			if (intervalString.Length == 0)
			{
				Message.printWarning(2, routine, "No interval specified.");
			}
			else
			{
				Message.printWarning(2, routine, "Unrecognized interval \"" + intervalString.Substring(digitCount) + "\"");
			}
			throw new InvalidTimeIntervalException("Unrecognized time interval \"" + intervalString + "\"");
		}

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Base: " + interval.getBase() + " (" + interval.getBaseString() + "), Mult: " + interval.getMultiplier());
		}

		return interval;
	}

	/// <summary>
	/// Set the interval base. </summary>
	/// <returns> Zero if successful, non-zero if not. </returns>
	/// <param name="base"> Time series interval. </param>
	public virtual void setBase(int @base)
	{
		__intervalBase = @base;
	}

	/// <summary>
	/// Set the interval base string.  This is normally only called by other methods within this class. </summary>
	/// <returns> Zero if successful, non-zero if not. </returns>
	/// <param name="base_string"> Time series interval base as string. </param>
	public virtual void setBaseString(string base_string)
	{
		if (!string.ReferenceEquals(base_string, null))
		{
			__intervalBaseString = base_string;
		}
	}

	/// <summary>
	/// Set the interval multiplier. </summary>
	/// <param name="mult"> Time series interval. </param>
	public virtual void setMultiplier(int mult)
	{
		__intervalMult = mult;
	}

	/// <summary>
	/// Set the interval multiplier string.  This is normally only called by other methods within this class. </summary>
	/// <param name="multiplier_string"> Time series interval base as string. </param>
	public virtual void setMultiplierString(string multiplier_string)
	{
		if (!string.ReferenceEquals(multiplier_string, null))
		{
			__intervalMultString = multiplier_string;
		}
	}

	/// <summary>
	/// Return the number of seconds in an interval, accounting for the base interval
	/// and multiplier.  Only regular intervals with a base less than or equal to a week
	/// can be processed because of the different number of days in a month.  See
	/// toSecondsApproximate() for a version that will handle all intervals. </summary>
	/// <returns> Number of seconds in an interval, or -1 if the interval cannot be processed. </returns>
	public virtual int toSeconds()
	{
		if (__intervalBase == SECOND)
		{
			return __intervalMult;
		}
		else if (__intervalBase == MINUTE)
		{
			return 60 * __intervalMult;
		}
		else if (__intervalBase == HOUR)
		{
			return 3600 * __intervalMult;
		}
		else if (__intervalBase == DAY)
		{
			return 86400 * __intervalMult;
		}
		else if (__intervalBase == WEEK)
		{
			return 604800 * __intervalMult;
		}
		else
		{
			return -1;
		}
	}

	/// <summary>
	/// Return the number of seconds in an interval, accounting for the base interval
	/// and multiplier.  For intervals greater than a day, the seconds are computed
	/// assuming 30 days per month (360 days per year).  Intervals of HSecond will
	/// return 0.  The result of this method can then be used to perform relative comparisons of intervals. </summary>
	/// <returns> Number of seconds in an interval. </returns>
	public virtual int toSecondsApproximate()
	{
		if (__intervalBase == HSECOND)
		{
			return 0;
		}
		else if (__intervalBase == SECOND)
		{
			return __intervalMult;
		}
		else if (__intervalBase == MINUTE)
		{
			return 60 * __intervalMult;
		}
		else if (__intervalBase == HOUR)
		{
			return 3600 * __intervalMult;
		}
		else if (__intervalBase == DAY)
		{
			return 86400 * __intervalMult;
		}
		else if (__intervalBase == WEEK)
		{
			return 604800 * __intervalMult;
		}
		else if (__intervalBase == MONTH)
		{
			// 86400*30
			return 2592000 * __intervalMult;
		}
		else if (__intervalBase == YEAR)
		{
			// 86400*30*13
			return 31104000 * __intervalMult;
		}
		else
		{
			// Should not happen...
			return -1;
		}
	}

	/// <summary>
	/// Return a string representation of the interval (e.g., "1Month").
	/// If irregular, the base string is returned.  If regular, the multiplier + the
	/// base string is returned (the multiplier may be "" or a number). </summary>
	/// <returns> a string representation of the interval (e.g., "1Month"). </returns>
	public override string ToString()
	{
		return __intervalMultString + __intervalBaseString;
	}

	}

}