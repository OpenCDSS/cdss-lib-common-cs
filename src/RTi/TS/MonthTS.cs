using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// MonthTS - base class from which all monthly time series are derived

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
// MonthTS - base class from which all monthly time series are derived
// ----------------------------------------------------------------------------
// Notes:	(1)	This base class is provided so that specific monthly
//			time series can derived from this class.
//		(2)	Data for this time series interval is stored as follows:
//
//			month in year  ->
//
//			------------------------
//              	|XX|  |.......|  |  |  |     first year in period
//			------------------------
//			|  |  |.......|  |  |  |
//			------------------------
//			|  |  |.......|  |  |  |
//			------------------------
//			|  |  |.......|  |  |  |
//			------------------------
//			.
//			.
//			.
//			------------------------
//			|  |  |.......|  |  |  |
//			------------------------
//			|  |  |.......|  |XX|XX|     last year in period
//			------------------------
//
//			The base block of storage is the year.  This lends
//			itself to very fast data retrieval but may waste some
//			memory for short time series in which full months are
//			not stored.  This is considered a reasonable tradeoff.
// ----------------------------------------------------------------------------
// History:
//
// 11 Mar 1997	Steven A. Malers, RTi	Copy HourTS and modify as appropriate.
// 06 Jun 1997	SAM, RTi		Add third positional argument to
//					getDataPosition to agree with the base
//					class.  It is not used here.
// 16 Jun 1997  MJR, RTi                Added overload of calcMaxMinValues.
// 09 Jan 1998	SAM, RTi		Update to agree with C++.
// 05 May 1998	SAM, RTi		Update formatOutput to have
//					UseCommentsForHeader.
// 06 Jul 1998	SAM, RTi		Eliminate the getDataPosition and
//					getDataPointer code and global data to
//					set the position data.  This class only
//					uses that internally.
// 12 Aug 1998	SAM, RTi		Update formatOutput.
// 20 Aug 1998	SAM, RTi		Add a new version of getDataPosition
//					that can be used by derived classes.
// 18 Nov 1998	SAM, RTi		Add copyData().
// 13 Apr 1999	SAM, RTi		Add finalize().
// 21 Feb 2001	SAM, RTi		Add clone().  Start setting unused
//					variables to null to improve memory use.
//					Update addGenesis().  Remove
//					printSample().  Remove
//					readPersistent*() and
//					writePersistent*().
// 04 May 2001	SAM, RTi		Recognize OutputPrecision property,
//					which is more consistent with TS
//					notation.
// 28 Aug 2001	SAM, RTi		Fix the clone() method and the copy
//					constructor.  Was not being rigorous
//					before.  Clean up Javadoc.
// 2001-11-06	SAM, RTi		Review javadoc.  Verify that variables
//					are set to null when no longer used.
//					Set return type for some methods to void
//					to agree with base class.  Remove
//					constructor that takes a file.
// 2003-01-08	SAM, RTi		Add hasData().
// 2003-02-05	SAM, RTi		Add support for data flags to be
//					consistent with DayTS.
// 2003-06-02	SAM, RTi		Upgrade to use generic classes.
//					* Change TSDate to DateTime.
//					* Change TSUnits to DataUnits.
//					* Change TS.INTERVAL* to TimeInterval.
// 2004-01-26	SAM, RTi		* Add OutputStart and OutputEnd
//					  properties to formatOutput().
//					* In formatOutput(), convert the file
//					  name to a full path.
// 2004-03-04	J. Thomas Sapienza, RTi	* Class now implements Serializable.
//					* Class now implements Transferable.
//					* Class supports being dragged or 
//					  copied to clipboard.
// 2005-05-12	SAM, RTi		* Add allocateDataFlagSpace().
// 2005-05-16	SAM, RTi		* Update allocateDataFlagSpace() to
//					  resize in addition to simply
//					  allocating the array.
// 2005-06-02	SAM, RTi		* Cleanup in allocateDataFlagSpace() and
//					  allocateDataSpace() - a DateTime was
//					  being iterated unnecessarily, causing
//					  a performance hit.
//					* Add _tsdata to improve performance
//					  getting data points.
//					* Fix getDataPoint() to return a TSData
//					  with missing data if outside the
//					  period of record.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------
// EndHeader

namespace RTi.TS
{
    using DateTime = Util.Time.DateTime;
    using Message = Util.Message.Message;
    using TimeInterval = Util.Time.TimeInterval;

    /// <summary>
    /// The MonthTS class is the base class for monthly time series.  Derive from this
    /// class for specific monthly time series formats (override allocateDataSpace() to control memory management).
    /// </summary>
    //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
    //ORIGINAL LINE: @SuppressWarnings("serial") public class MonthTS extends TS implements Cloneable, java.io.Serializable, java.awt.datatransfer.Transferable
    [Serializable]
    public class MonthTS : TS //, ICloneable
    {
        // Data members...

        // FIXME SAM 2009-02-25 Need to move the following to a wrapper class
        /// <summary>
        /// The DataFlavor for transferring this specific class.
        /// </summary>
        //public static DataFlavor monthTSFlavor = new DataFlavor(typeof(RTi.TS.MonthTS), "RTi.TS.MonthTS");

        private double[][] _data; // This is the data space for monthly values.  The dimensions are [year][month]
        protected internal string[][] _dataFlags; // Data flags for each monthly value.  The dimensions are [year][month]
        protected internal int _min_amon; // Minimum absolute month stored.
        protected internal int _max_amon; // Maximum absolute month stored.
        private int[] _pos = null; // Use to return data without creating memory all the time.

        /// <summary>
        /// Constructor.  Set the dates and call allocateDataSpace() to create space for data.
        /// </summary>
        public MonthTS() : base()
        {
            init();
        }

        /*
	Allocate the data space for the time series.  The start and end dates and the
	data interval multiplier must have been set.  Initialize the space with the missing data value.
	*/
        public override int allocateDataSpace()
        {
            return allocateDataSpace(_missing);
        }

        /// <summary>
        /// Allocate the data space for the time series.  The start and end dates and the
        /// data interval multiplier must have been set.  Fill with the specified data value. </summary>
        /// <param name="value"> Value to initialize data space. </param>
        /// <returns> 1 if the allocation fails, 0 if a success. </returns>
        public virtual int allocateDataSpace(double value)
        {
            string routine = "MonthTS.allocateDataSpace";
            int iYear, nyears = 0;

            if ((_date1 == null) || (_date2 == null))
            {
                Message.printWarning(2, routine, "Dates have not been set.  Cannot allocate data space");
                return 1;
            }
            if (_data_interval_mult != 1)
            {
                // Do not know how to handle N-month interval...
                Message.printWarning(2, routine, "Only know how to handle 1 month data, not " + _data_interval_mult + "-month");
                return 1;
            }

            nyears = _date2.getYear() - _date1.getYear() + 1;

            if (nyears == 0)
            {
                Message.printWarning(3, routine, "TS has 0 years POR, maybe dates haven't been set yet");
                return 1;
            }

            _data = new double[nyears][];
            if (_has_data_flags)
            {
                _dataFlags = new string[nyears][];
            }

            // Allocate memory...

            int iMonth, nvals = 12;
            for (iYear = 0; iYear < nyears; iYear++)
            {
                _data[iYear] = new double[nvals];
                if (_has_data_flags)
                {
                    _dataFlags[iYear] = new string[nvals];
                }

                // Now fill with the missing data value...

                for (iMonth = 0; iMonth < nvals; iMonth++)
                {
                    _data[iYear][iMonth] = value;
                    if (_has_data_flags)
                    {
                        _dataFlags[iYear][iMonth] = "";
                    }
                }
            }

            // Set the data size...

            int datasize = calculateDataSize(_date1, _date2, _data_interval_mult);
            setDataSize(datasize);

            // Set the limits used for set/get routines...

            _min_amon = _date1.getAbsoluteMonth();
            _max_amon = _date2.getAbsoluteMonth();

            if (Message.isDebugOn)
            {
                Message.printDebug(10, routine, "Successfully allocated " + nyears + " yearsx12 months of memory (" + datasize + " values)");
            }

            routine = null;
            return 0;
        }

        /// <summary>
        /// Calculate and return the number of data points that have been allocated. </summary>
        /// <returns> The number of data points for a month time series
        /// given the data interval multiplier for the specified period, including missing data. </returns>
        /// <param name="start_date"> The first date of the period. </param>
        /// <param name="end_date"> The last date of the period. </param>
        /// <param name="interval_mult"> The time series data interval multiplier. </param>
        public static int calculateDataSize(DateTime start_date, DateTime end_date, int interval_mult)
        {
            string routine = "MonthTS.calculateDataSize";
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

            if (interval_mult != 1)
            {
                Message.printWarning(3, routine, "Do not know how to handle N-month time series");
                return 0;
            }
            datasize = end_date.getAbsoluteMonth() - start_date.getAbsoluteMonth() + 1;
            routine = null;
            return datasize;
        }

        /// <summary>
        /// Return the data value for a date.
        /// <pre>
        ///             Monthly data is stored in a two-dimensional array:
        ///  		     |----------------> 12 calendar months
        ///  		     |
        ///  		    \|/
        ///  		   year 
        /// </pre> </summary>
        /// <returns> The data value corresponding to the date, or missing if the date is not found. </returns>
        /// <param name="date"> Date of interest. </param>
        public override double getDataValue(DateTime date)
        { // Do not define routine here to increase performance.

            if (_data == null)
            {
                return _missing;
            }

            // Check the date coming in...

            int amon = date.getAbsoluteMonth();

            if ((amon < _min_amon) || (amon > _max_amon))
            {
                // Print within debug to optimize performance...
                if (Message.isDebugOn)
                {
                    Message.printWarning(50, "MonthTS.getDataValue", date + " not within POR (" + _date1 + " - " + _date2 + ")");
                }
                return _missing;
            }

            // THIS CODE NEEDS TO BE EQUIVALENT IN setDataValue...

            int row = date.getYear() - _date1.getYear();
            int column = date.getMonth() - 1; // Zero offset!

            // ... END OF EQUIVALENT CODE.

            if (Message.isDebugOn)
            {
                Message.printDebug(50, "MonthTS.getDataValue", _data[row][column] + " for " + date + " from _data[" + row + "][" + column + "]");
            }

            return (_data[row][column]);
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
        /// Initialize instance.
        /// </summary>
        private void init()
        {
            _data = null;
            _data_interval_base = TimeInterval.MONTH;
            _data_interval_mult = 1;
            _data_interval_base_original = TimeInterval.MONTH;
            _data_interval_mult_original = 1;
            _pos = new int[2];
            _pos[0] = 0;
            _pos[1] = 0;
            _min_amon = 0;
            _max_amon = 0;
        }

        /// <summary>
        /// Set the data value for the specified date. </summary>
        /// <param name="date"> Date of interest. </param>
        /// <param name="value"> Value corresponding to date. </param>
        public override void setDataValue(DateTime date, double value)
        { // Do not define routine here to increase performance.

            // Check the date coming in...

            if (date == null)
            {
                return;
            }

            int amon = date.getAbsoluteMonth();

            if ((amon < _min_amon) || (amon > _max_amon))
            {
                // Print within debug to optimize performance...
                if (Message.isDebugOn)
                {
                    Message.printWarning(50, "MonthTS.setDataValue", date + " not within POR (" + _date1 + " - " + _date2 + ")");
                }
                return;
            }

            // THIS CODE NEEDS TO BE EQUIVALENT IN setDataValue...

            int row = date.getYear() - _date1.getYear();
            int column = date.getMonth() - 1; // Zero offset!

            // ... END OF EQUIVALENT CODE.

            if (Message.isDebugOn)
            {
                Message.printDebug(50, "MonthTS.setDataValue", "Setting " + value + " " + date + " at " + row + "," + column);
            }

            // Set the dirty flag so that we know to recompute the limits if desired...

            _dirty = true;
            _data[row][column] = value;
        }
    }
}
