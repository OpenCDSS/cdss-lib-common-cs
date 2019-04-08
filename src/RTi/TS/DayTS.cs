using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// DayTS - base class from which all daily time series are derived

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
// DayTS - base class from which all daily time series are derived
// ----------------------------------------------------------------------------
// Notes:	(1)	This base class is provided so that specific daily
//			time series can derive from this class.
//		(2)	Data for this time series interval is stored as follows:
//
//			day within month  ->
//
//			------------------------
//              	|  |  |.......|  |  |  |     first month in period
//			------------------------
//			|  |  |.......|  |  |
//			------------------------
//			|  |  |.......|  |  |  |
//			------------------------
//			|  |  |.......|  |  |
//			---------------------
//			.
//			.
//			.
//			------------------------
//			|  |  |.......|  |  |  |
//			------------------------
//			|  |  |.......|  |  |        last month in period
//			---------------------
//
//			The base block of storage is the month.  This lends
//			itself to very fast data retrieval but may waste some
//			memory for short time series in which full months are
//			not stored.  This is considered a reasonable tradeoff.
// ----------------------------------------------------------------------------
// History:
//
// 09 Apr 1998	Steven A. Malers, RTi	Copy the HourTS code and modify as
//					necessary.  Start to use this instead
//					of a 24-hour time series to make
//					storage more straightforward.
// 22 Aug 1998	SAM, RTi		In-line up getDataPosition in this
//					class to increase performance. Change
//					so that getDataPosition returns an
//					array.
// 09 Jan 1999	SAM, RTi		Add more exception handling due to
//					changes in other classes.
// 05 Apr 1999	CEN, RTi		Optimize by adding _row, _column, 
//					similar to HourTS.
// 21 Apr 1999	SAM, RTi		Add precision lookup for formatOutput.
//					Add genesis to output.
// 09 Aug 1999	SAM, RTi		Add changePeriodOfRecord to support
//					regression.
// 21 Feb 2001	SAM, RTi		Add clone().  Add copy constructor.
//					Remove printSample(), read and write
//					methods.
// 04 May 2001	SAM, RTi		Add OutputPrecision property, which is
//					more consistent with TS notation.
// 29 Aug 2001	SAM, RTi		Fix clone() to work correctly.  Remove
//					old C-style documentation.  Change _pos
//					from static - trying to minimize the
//					amount of static data that is used.
// 2001-11-06	SAM, RTi		Review javadoc.  Verify that variables
//					are set to null when no longer used.
//					Remove constructor that takes a file
//					name.  Change some methods to have void
//					return type to agree with base class.
// 2002-01-31	SAM, RTi		Add support for data flags.  Change so
//					getDataPoint() returns a reference to an
//					internal object that is reused.
// 2002-05-24	SAM, RTi		Add total period statistics for each
//					month.
// 2002-09-05	SAM, RTi		Remove hasDataFlags().  Let the base TS
//					class method suffice.  Change so that
//					hasDataFlags() does not allocate the
//					data flags memory but instead do it in
//					allocateDataSpace().
// 2003-01-08	SAM, RTi		Add hasData().
// 2003-05-02	SAM, RTi		Fix bug in getDataPoint() - was not
//					recalculationg the row/column position
//					for the data flag.
// 2003-06-02	SAM, RTi		Upgrade to use generic classes.
//					* Change TSDate to DateTime.
//					* Change TSUnits to DataUnits.
//					* Change TS.INTERVAL* to TimeInterval.
// 2003-10-21	SAM, RTi		Overload allocateDataSpace(), similar
//					to MonthTS to take an initial value.
// 2003-12-09	SAM, RTi		* Handle data flags in clone().
// 2004-01-26	SAM, RTi		* Add OutputStart and OutputEnd
//					  properties to formatOutput().
//					* In formatOutput(), convert the file
//					  name to a full path.
// 2004-03-04	J. Thomas Sapienza, RTi	* Class now implements Serializable.
//					* Class now implements Transferable.
//					* Class supports being dragged or 
//					  copied to clipboard.
// 2005-06-02	SAM, RTi		* Add allocateDataFlagSpace(), similar
//					  to MonthTS.
//					* Remove warning about reallocating data
//					  space.
// 2005-12-07	JTS, RTi		Added copy constructor to create a DayTS
//					from an HourTS.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------
// EndHeader

namespace RTi.TS
{
    using DateTime = Util.Time.DateTime;
    using Message = Util.Message.Message;
    using TimeInterval = Util.Time.TimeInterval;
    using TimeUtil = Util.Time.TimeUtil;

    /// <summary>
    /// The DayTS class is the base class for daily time series.  The class can be
    /// extended for variations on daily data.  Override the allocateDataSpace() and set/get methods to do so.
    /// </summary>
    //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
    //ORIGINAL LINE: @SuppressWarnings("serial") public class DayTS extends TS implements Cloneable, java.io.Serializable, java.awt.datatransfer.Transferable
    [Serializable]
    public class DayTS : TS //, ICloneable, Transferable
    {

        // Data members...

        /// <summary>
        /// The DataFlavor for transferring this specific class.
        /// </summary>
        //public static DataFlavor dayTSFlavor = new DataFlavor(typeof(RTi.TS.DayTS), "RTi.TS.DayTS");

        protected internal double[][] _data; // This is the data space for daily time series.
        protected internal string[][] _dataFlags; // Data flags for each daily value, with dimensions [month][day_in_month]
        private int[] _pos = null; // Used to optimize performance when getting data.
        protected internal int _row, _column; // Column position in data.

        /// <summary>
        /// Constructor.
        /// </summary>
        public DayTS() : base()
        {
            init();
        }

        /// <summary>
        /// Allocate the data space for the time series.  The start and end dates and the
        /// data interval multiplier must have been set.  Initialize the space with the missing data value.
        /// </summary>
        public override int allocateDataSpace()
        {
            return allocateDataSpace(_missing);
        }

        /// <summary>
        /// Allocate the data space.  The start and end dates and the interval multiplier should have been set. </summary>
        /// <param name="value"> The value to initialize the time series. </param>
        /// <returns> 0 if successful, 1 if failure. </returns>
        public virtual int allocateDataSpace(double value)
        {
            string routine = "DayTS.allocateDataSpace";
            int ndays_in_month, nmonths = 0, nvals;

            if ((_date1 == null) || (_date2 == null))
            {
                Message.printWarning(2, routine, "No dates set for memory allocation");
                return 1;
            }
            if (_data_interval_mult != 1)
            {
                // Do not know how to handle N-day interval...
                string message = "Only know how to handle 1-day data, not " + _data_interval_mult + "Day";
                Message.printWarning(3, routine, message);
                return 1;
            }
            nmonths = _date2.getAbsoluteMonth() - _date1.getAbsoluteMonth() + 1;

            if (nmonths == 0)
            {
                Message.printWarning(2, routine, "TS has 0 months POR, maybe dates haven't been set yet");
                return 1;
            }

            _data = new double[nmonths][];
            if (_has_data_flags)
            {
                _dataFlags = new string[nmonths][];
            }

            // May need to catch an exception here in case we run out of memory.

            // Set the counter date to match the starting month.  This date is used
            // to determine the number of days in each month.

            DateTime date = new DateTime(DateTime.DATE_FAST);
            date.setMonth(_date1.getMonth());
            date.setYear(_date1.getYear());

            int iday = 0;
            for (int imon = 0; imon < nmonths; imon++, date.addMonth(1))
            {
                ndays_in_month = TimeUtil.numDaysInMonth(date);
                // Handle 1-day data, otherwise an exception was thrown above.
                // Here would change the number of values if N-day was supported.
                nvals = ndays_in_month;
                _data[imon] = new double[nvals];
                if (_has_data_flags)
                {
                    _dataFlags[imon] = new string[nvals];
                }

                // Now fill with the missing data value for each day in the month...

                for (iday = 0; iday < nvals; iday++)
                {
                    _data[imon][iday] = value;
                    if (_has_data_flags)
                    {
                        _dataFlags[imon][iday] = "";
                    }
                }
            }

            int nactual = calculateDataSize(_date1, _date2, _data_interval_mult);
            setDataSize(nactual);

            if (Message.isDebugOn)
            {
                Message.printDebug(10, routine, "Allocated " + nmonths + " months of memory for daily data from " + _date1 + " to " + _date2);
            }

            date = null;
            routine = null;
            return 0;
        }

        /// <summary>
        /// Determine the number of points between two dates for the given interval multiplier. </summary>
        /// <returns> The number of data points for a day time series
        /// given the data interval multiplier for the specified period. </returns>
        /// <param name="start_date"> The first date of the period. </param>
        /// <param name="end_date"> The last date of the period. </param>
        /// <param name="interval_mult"> The time series data interval multiplier. </param>
        public static int calculateDataSize(DateTime start_date, DateTime end_date, int interval_mult)
        {
            string routine = "DayTS.calculateDataSize";
            int datasize = 0;

            if (start_date == null)
            {
                Message.printWarning(2, routine, "Start date is null");
                return 0;
            }
            if (end_date == null)
            {
                Message.printWarning(2, routine, "End date is null");
                return 0;
            }
            if (interval_mult > 1)
            {
                Message.printWarning(1, routine, "Greater than 1-day TS not supported");
                return 0;
            }
            // First set to the number of data in the months...
            datasize = TimeUtil.numDaysInMonths(start_date.getMonth(), start_date.getYear(), end_date.getMonth(), end_date.getYear());
            // Now subtract off the data at the ends that are missing...
            // Start by subtracting the full day at the beginning of the month that is not included...
            datasize -= (start_date.getDay() - 1);
            // Now subtract off the data at the end...
            // Start by subtracting the full days off the end of the month...
            int ndays_in_month = TimeUtil.numDaysInMonth(end_date.getMonth(), end_date.getYear());
            datasize -= (ndays_in_month - end_date.getDay());
            routine = null;
            return datasize;
        }

        /// <summary>
        /// Return the position corresponding to the date.  The position array is volatile
        /// and is reused for each call.  Copy the values to make persistent.
        /// <pre>
        ///             	Day data is stored in a two-dimensional array:
        ///  		     |----------------> days
        ///  		     |
        ///  		    \|/
        ///  		   month 
        /// 
        /// </pre> </summary>
        /// <returns> The data position corresponding to the date. </returns>
        /// <param name="date"> Date of interest. </param>
        private int[] getDataPosition(DateTime date)
        { // Do not define the routine or debug level here so we can optimize.

            // Note that unlike HourTS, do not need to check the time zone!

            // Check the date coming in...

            if (date == null)
            {
                return null;
            }

            // Calculate the row position of the data...

            if (Message.isDebugOn)
            {
                Message.printDebug(50, "DayTS.getDataPosition", "Using " + date + "(" + date.getAbsoluteMonth() + ") and start date: " + _date1 + "(" + _date1.getAbsoluteMonth() + ") for row-col calculation.");
            }

            _row = date.getAbsoluteMonth() - _date1.getAbsoluteMonth();

            // Calculate the column position of the data. We know that Daily data
            // is stored in a 2 dimensional array with the column being the daily data by interval.

            _column = date.getDay() - 1;
            if (Message.isDebugOn)
            {
                Message.printDebug(50, "DayTS.getDataPosition", "Row=" + _row + " Column=" + _column);
            }

            _pos[0] = _row;
            _pos[1] = _column;
            return _pos;
        }

        /// <summary>
        /// Return the data value for a date.
        /// <pre>
        ///             	Day data is stored in a two-dimensional array:
        ///  		     |----------------> days
        ///  		     |
        ///  		    \|/
        ///  		   month 
        /// </pre> </summary>
        /// <returns> The data value corresponding to the date, or the missing data value if the date is not found. </returns>
        /// <param name="date"> Date of interest. </param>
        public override double getDataValue(DateTime date)
        { // Do not define routine here to improve performance.

            // Check the date coming in 

            if ((date == null) || !hasData())
            {
                return _missing;
            }
            if ((date.lessThan(_date1)) || (date.greaterThan(_date2)))
            {
                if (Message.isDebugOn)
                {
                    Message.printDebug(50, "DayTS.getDataValue", date + " not within POR (" + _date1 + " - " + _date2 + ")");
                }
                return _missing;
            }

            getDataPosition(date);

            if (Message.isDebugOn)
            {
                Message.printDebug(50, "DayTS.getDataValue", _data[_row][_column] + " for " + date + " from _data[" + _row + "][" + _column + "]");
            }

            double value = 0;
            // FIXME SAM 2010-08-20 Possible to throw exceptions if the date is not the right precision and
            // illegal math results in negative values in arrays
            //try {
            value = _data[_row][_column];
            //}
            //catch ( Exception e ) {
            //    Message.printWarning(3, "", "Error getting value for date " + date + "date1=" + _date1 + "_date2=" + _date2 + " row=" + _row + " col=" + _column);
            //    Message.printWarning(3, "", e);
            //}
            return value;
        }

        /// <summary>
        /// Indicate whether the time series has data, determined by checking to see whether
        /// the data space has been allocated.  This method can be called after a time
        /// series has been read - even if no data are available, the header information
        /// may be complete.  The alternative of returning a null time series from a read
        /// method if no data are available results in the header information being
        /// unavailable.  Instead, return a TS with only the header information and call
        /// hasData() to check to see if the data space has been assigned. </summary>
        /// <returns> true if data are available (the data space has been allocated).
        /// Note that true will be returned even if all the data values are set to the missing data value. </returns>
        public override bool hasData()
        {
            if (_data != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Initialize data members.
        /// </summary>
        private void init()
        {
            _data = null;
            _data_interval_base = TimeInterval.DAY;
            _data_interval_mult = 1;
            _data_interval_base_original = TimeInterval.DAY;
            _data_interval_mult_original = 1;
            _pos = new int[2];
            _pos[0] = 0;
            _pos[1] = 0;
            _row = 0;
            _column = 0;
        }

        /// <summary>
        /// Set the data value for the date. </summary>
        /// <param name="date"> Date of interest. </param>
        /// <param name="value"> Data value corresponding to date. </param>
        public override void setDataValue(DateTime date, double value)
        {
            if ((date.lessThan(_date1)) || (date.greaterThan(_date2)))
            {
                if (Message.isDebugOn)
                {
                    Message.printWarning(10, "DayTS.setDataValue", "Date " + date + " is outside bounds " + _date1 + " - " + _date2);
                }
                return;
            }

            getDataPosition(date);

            if (Message.isDebugOn)
            {
                Message.printDebug(30, "DayTS.setDataValue", "Setting " + value + " for " + date + " at " + _row + "," + _column);
            }

            // Set the dirty flag so that we know to recompute the limits if desired...

            _dirty = true;

            _data[_row][_column] = value;
        }
    }
}
