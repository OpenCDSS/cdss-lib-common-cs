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
    using Message = Message.Message;

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
                string intervalMultString = intervalString.Substring(0, digitCount);
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
                //throw new InvalidTimeIntervalException("Unrecognized time interval \"" + intervalString + "\"");
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

    }
}
