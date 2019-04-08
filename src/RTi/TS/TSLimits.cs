using System;
using System.Collections.Generic;

// TSLimits - base class for time series data limits

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
// TSLimits - simple class for managing time series data limits
// ----------------------------------------------------------------------------
// History:
//
// 24 Sep 1997	Steven A. Malers, RTi	Initial version.
// 05 Jan 1998	SAM, RTi		Update based on C++ port.  Add
//					_date1 and _date2 so that we can use
//					TSLimits as a return from routines that
//					return date limits.  Add
//					_non_missing_data_count,
//					getNonMissingDataCount(), and
//					hasNonMissingData().
// 19 Mar 1998	SAM, RTi		Add javadoc.
// 20 Aug 1998	SAM, RTi		Add setMissingDataCount().
// 02 Jan 1999	SAM, RTi		Add sum and mean to the data that are
//					tracked to support searches, etc.
//					Change the return type of set routines
//					to void and don't return anything (the
//					methods always returned 0).  Add
//					constructors and getDataLimits methods
//					so that this routine actually does the
//					computations.  This allows independent
//					use from TSUtil.getDataLimits, which
//					is desirable but may not perform well
//					in some cases.
// 12 Apr 1999	SAM, RTi		Add finalize.
// 28 Oct 1999	SAM, RTi		Add flag to ignore <= 0 values in
//					computations (treat as missing).
// 22 Mar 2001	SAM, RTi		Change toString() to print nicer so
//					output can be used in reports.
// 28 Aug 2001	SAM, RTi		Implement clone().  A copy constructor
//					is already implemented but clone() is
//					used by TS and might be preferred by
//					some developers.
// 2001-11-06	SAM, RTi		Review javadoc.  Verify that variables
//					are set to null when no longer used.
// 2003-03-25	SAM, RTi		Add data units as a data member and
//					add getDataUnits().  This is needed
//					because TSTool now has a
//					convertDataUnits() command and the units
//					for TSLimits need to be set for the
//					current and original data.
// 2003-06-02	SAM, RTi		Upgrade to use generic classes.
//					* Change TSDate to DateTime.
//					* Change TS.INTERVAL* to TimeInterval.
// 2004-02-17	SAM, RTi		* Change private data members to start
//					  with __, as per RTi standards.
//					* Change the private getDataLimits() to
//					  calculateDataLimits() to more
//					  accurately reflect its purpose.
//					* Fix copy constructor to set the _flags
//					  and __ts, but do not copy the time
//					  series.
// 2004-03-04	J. Thomas Sapienza, RTi	Class is now serializable.
// 2005-02-04	SAM, RTi		Add getTimeSeries() to return the time
//					series associated with the limits.
//					Add setTimeSeries() also.
//					When limits cannot be calculated in
//					calculateDataLimits(), only print the
//					warning message at debug level 2.
// 2005-05-25	SAM, RTi		Change so "low level" warning messages
//					are handled and level 3 instead of 2, to
//					mininimize visibility in the log viewer.
// ----------------------------------------------------------------------------
// EndHeader

namespace RTi.TS
{
    using DateTime = Util.Time.DateTime;

    /// <summary>
    /// The TSLimits class stores information about the data and date limits of a time
    /// series, including maximum, minimum, and mean, and important date/times.
    /// An instance is used by the TS base class and TSUtil routines use TSLimits to
    /// pass information.  In general, code outside of the TS package will only use the
    /// get*() methods (because TS or TSUtil methods will set the data).
    /// This TSLimits base class can be used for any time series.  More detailed limits
    /// like MonthTSLimits can be extended to contain more information.  The
    /// toString() method should be written to provide output suitable for use in a report.
    /// </summary>
    //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
    //ORIGINAL LINE: @SuppressWarnings("serial") public class TSLimits implements Cloneable, java.io.Serializable
    [Serializable]
    public class TSLimits //: ICloneable
    {
        // Public flags...

        /// <summary>
        /// Flags used to indicate how limits are to be computed.
        /// The following indicates that a time series' full limits should be refreshed.
        /// This is generally used only by code internal to the TS library.
        /// </summary>
        public const int REFRESH_TS = 0x1;

        /// <summary>
        /// Do not compute the total limits (using this TSLimits class).  This is used by
        /// classes such as MonthTSLimits to increase performance.
        /// </summary>
        public const int NO_COMPUTE_TOTALS = 0x2;

        /// <summary>
        /// Do not compute the detailed limits (e.g., using MonthTSLimits).  This is used by
        /// classes such as MonthTSLimits to increase performance.
        /// </summary>
        public const int NO_COMPUTE_DETAIL = 0x4;

        /// <summary>
        /// Ignore values <= 0 when computing averages (treat as missing data).
        /// This make sense for time series
        /// such as reservoirs and flows.  It may be necessary at some point to allow any
        /// value to be ignored but <= 0 is considered a common and special case.
        /// </summary>
        public const int IGNORE_LESS_THAN_OR_EQUAL_ZERO = 0x8;

        // Data members...

        private TS __ts = null; // Time series being studied.
        private DateTime __date1;
        private DateTime __date2;
        protected internal int _flags; // Flags to control behavior.
        private double __max_value;
        private DateTime __max_value_date;
        private double __mean;
        private double __median;
        private double __min_value;
        private DateTime __min_value_date;
        private int __missing_data_count;
        private int __non_missing_data_count;
        private DateTime __non_missing_data_date1;
        private DateTime __non_missing_data_date2;
        private double __skew;
        private double __stdDev;
        private double __sum;
        private string __data_units = ""; // Data units (just copy from TS at the time of creation).

        private bool __found = false;

        /// <summary>
        /// Default constructor.  Initialize the dates to null and the limits to zeros.
        /// </summary>
        public TSLimits()
        {
            initialize();
        }

        /// <summary>
        /// Check to see if ALL the dates have been set (are non-null) and if so set the
        /// _found flag to true.  If a TSLimits is being used for something other than fill
        /// limits analysis, then external code may need to call setLimitsFound() to manually set the found flag.
        /// </summary>
        private void checkDates()
        {
            if ((__date1 != null) && (__date2 != null) && (__max_value_date != null) && (__min_value_date != null) && (__non_missing_data_date1 != null) && (__non_missing_data_date2 != null))
            {
                // The dates have been fully processed (set)...
                __found = true;
            }
        }

        /// <summary>
        /// Return the first date for the time series according to the memory allocation. </summary>
        /// <returns> The first date for the time series according to the memory allocation.
        /// A copy of the date is returned. </returns>
        public virtual DateTime getDate1()
        {
            if (__date1 == null)
            {
                return __date1;
            }
            return new DateTime(__date1);
        }

        /// <summary>
        /// Return the last date for the time series according to the memory allocation. </summary>
        /// <returns> The last date for the time series according to the memory allocation.
        /// A copy of the date is returned. </returns>
        public virtual DateTime getDate2()
        {
            if (__date2 == null)
            {
                return __date2;
            }
            return new DateTime(__date2);
        }

        /// <summary>
        /// Initialize the data.
        /// Need to rework code to use an instance of TS so we can initialize to missing
        /// data values used by the time series!
        /// </summary>
        private void initialize()
        {
            __ts = null;
            __data_units = "";
            __date1 = null;
            __date2 = null;
            _flags = 0;
            __max_value = 0.0;
            __max_value_date = null;
            __mean = -999.0; // Assume.
            __median = Double.NaN; // Assume.
            __min_value = 0.0;
            __min_value_date = null;
            __missing_data_count = 0;
            __non_missing_data_count = 0;
            __non_missing_data_date1 = null;
            __non_missing_data_date2 = null;
            __skew = Double.NaN;
            __stdDev = Double.NaN;
            __sum = -999.0; // Assume.
            __found = false;
        }

        /// <summary>
        /// Set the first date for the time series.  This is used for memory allocation. </summary>
        /// <param name="date1"> The first date for the time series. </param>
        /// <seealso cref= TS#allocateDataSpace </seealso>
        public virtual void setDate1(DateTime date1)
        {
            if (date1 != null)
            {
                __date1 = new DateTime(date1);
            }
            checkDates();
        }

        /// <summary>
        /// Set the last date for the time series.  This is used for memory allocation. </summary>
        /// <param name="date2"> The last date for the time series. </param>
        /// <seealso cref= TS#allocateDataSpace </seealso>
        public virtual void setDate2(DateTime date2)
        {
            if (date2 != null)
            {
                __date2 = new DateTime(date2);
            }
            checkDates();
        }

        /// <summary>
        /// Set whether the limits have been found.  This is mainly used by routines in
        /// the package when only partial limits are needed (as opposed to checkDates(),
        /// which checks all the dates in a TSLimits).  Call this method after any methods
        /// that set the date to offset the check done by checkDates(). </summary>
        /// <param name="flag"> Indicates whether the limits have been found (true or false). </param>
        protected internal virtual void setLimitsFound(bool flag)
        {
            __found = flag;
        }
    }
}
