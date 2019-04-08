﻿using System;
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


	using DataUnits = RTi.Util.IO.DataUnits;
	using IOUtil = RTi.Util.IO.IOUtil;
	using PropList = RTi.Util.IO.PropList;
	using MathUtil = RTi.Util.Math.MathUtil;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using DateTime = RTi.Util.Time.DateTime;
	using TimeInterval = RTi.Util.Time.TimeInterval;
	using YearType = RTi.Util.Time.YearType;

	/// <summary>
	/// The MonthTS class is the base class for monthly time series.  Derive from this
	/// class for specific monthly time series formats (override allocateDataSpace() to control memory management).
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class MonthTS extends TS implements Cloneable, java.io.Serializable, java.awt.datatransfer.Transferable
	[Serializable]
	public class MonthTS : TS, ICloneable, Transferable
	{
	// Data members...

	// FIXME SAM 2009-02-25 Need to move the following to a wrapper class
	/// <summary>
	/// The DataFlavor for transferring this specific class.
	/// </summary>
	public static DataFlavor monthTSFlavor = new DataFlavor(typeof(RTi.TS.MonthTS), "RTi.TS.MonthTS");

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

	/// <summary>
	/// Copy constructor.  Everything is copied by calling copyHeader() and then copying the data values. </summary>
	/// <param name="ts"> MonthTS to copy. </param>
	public MonthTS(MonthTS ts)
	{
		if (ts == null)
		{
			return;
		}
		copyHeader(ts);
		allocateDataSpace();
		DateTime date2 = new DateTime(_date2);
		DateTime date = new DateTime(_date1);
		for (; date.lessThanOrEqualTo(date2); date.addInterval(_data_interval_base,_data_interval_mult))
		{
			setDataValue(date, ts.getDataValue(date));
		}
		date2 = null;
		date = null;
	}

	/// <summary>
	/// Allocate the data flag space for the time series.  This requires that the data
	/// interval base and multiplier are set correctly and that _date1 and _date2 have
	/// been set.  The allocateDataSpace() method will allocate the data flags if
	/// appropriate.  Use this method when the data flags need to be allocated after the initial allocation. </summary>
	/// <param name="initialValue"> Initial value (null will be converted to an empty string). </param>
	/// <param name="retainPreviousValues"> If true, the array size will be increased if necessary, but
	/// previous data values will be retained.  If false, the array will be reallocated and initialized to spaces. </param>
	/// <exception cref="Exception"> if there is an error allocating the memory. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void allocateDataFlagSpace(String initialValue, boolean retainPreviousValues) throws Exception
	public override void allocateDataFlagSpace(string initialValue, bool retainPreviousValues)
	{
		string routine = "MonthTS.allocateDataFlagSpace", message;
		int i, nyears = 0;

		if ((_date1 == null) || (_date2 == null))
		{
			message = "Dates have not been set.  Cannot allocate data space";
			Message.printWarning(3, routine, message);
			throw new Exception(message);
		}
		if (_data_interval_mult != 1)
		{
			// Do not know how to handle N-month interval...
			message = "Only know how to handle 1 month data, not " + _data_interval_mult + "-month";
			Message.printWarning(3, routine, message);
			throw new Exception(message);
		}

		if (string.ReferenceEquals(initialValue, null))
		{
			initialValue = "";
		}

		nyears = _date2.getYear() - _date1.getYear() + 1;

		if (nyears == 0)
		{
			message = "TS has 0 years POR, maybe Dates haven't been set yet";
			Message.printWarning(3, routine, message);
			throw new Exception(message);
		}

		string[][] dataFlagsPrev = null;
		if (_has_data_flags && retainPreviousValues)
		{
			// Save the reference to the old flags array...
			dataFlagsPrev = _dataFlags;
		}
		else
		{
			// Turn on the flags...
			_has_data_flags = true;
		}
		// Top-level allocation...
		_dataFlags = new string[nyears][];

		// Allocate memory...

		int j, nvals = 12;
		bool internDataFlagStrings = getInternDataFlagStrings();
		for (i = 0; i < nyears; i++)
		{
			_dataFlags[i] = new string[nvals];

			// Now fill with the initial data value...

			for (j = 0; j < nvals; j++)
			{
				// Initialize with initial value...
				if (internDataFlagStrings)
				{
					_dataFlags[i][j] = initialValue.intern();
				}
				else
				{
					_dataFlags[i][j] = initialValue;
				}
				if (retainPreviousValues && (dataFlagsPrev != null))
				{
					// Copy over the old values (typically shorter character arrays)...
					if (internDataFlagStrings)
					{
						_dataFlags[i][j] = dataFlagsPrev[i][j].intern();
					}
					else
					{
						_dataFlags[i][j] = dataFlagsPrev[i][j];
					}
				}
			}
		}
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

		_data = new double [nyears][];
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
	/// Change the period of record
	/// to the specified dates.  If the period is extended, missing data will be used
	/// to fill the time series.  If the period is shortened, data will be lost. </summary>
	/// <param name="date1"> New start date of time series. </param>
	/// <param name="date2"> New end date of time series. </param>
	/// <exception cref="RTi.TS.TSException"> if there is a problem extending the data. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void changePeriodOfRecord(RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2) throws TSException
	public override void changePeriodOfRecord(DateTime date1, DateTime date2)
	{
		string routine = "MonthTS.changePeriodOfRecord";
		string message;

		// To transfer, we need to allocate a new data space.  In any case, we
		// need to get the dates established...
		if ((date1 == null) && (date2 == null))
		{
			// No dates.  Cannot change.
			message = "\"" + _id + "\": period dates are null.  Cannot change the period.";
			Message.printWarning(2, routine, message);
			throw new TSException(message);
		}

		DateTime new_date1 = null;
		if (date1 == null)
		{
			// Use the original date...
			new_date1 = new DateTime(_date1);
		}
		else
		{
			// Use the date passed in...
			new_date1 = new DateTime(date1);
		}
		DateTime new_date2 = null;
		if (date2 == null)
		{
			// Use the original date...
			new_date2 = new DateTime(_date2);
		}
		else
		{
			// Use the date passed in...
			new_date2 = new DateTime(date2);
		}

		// Do not change the period if the dates are the same...

		if (_date1.Equals(new_date1) && _date2.Equals(new_date2))
		{
			// No need to change period...
			new_date1 = null;
			new_date2 = null;
			return;
		}

		// To transfer the data (later), get the old position and then set in the new position.  To get the right
		// data position, declare a temporary MonthTS with the old dates and save a reference to the old data...

		double[][] data_save = _data;
		string[][] dataFlagsSave = _dataFlags;
		MonthTS temp_ts = new MonthTS();
		temp_ts.setDataInterval(TimeInterval.MONTH, _data_interval_mult);
		temp_ts.setDate1(_date1);
		temp_ts.setDate2(_date2);

		// Also compute limits for the transfer to optimize performance...

		DateTime transfer_date1 = null;
		DateTime transfer_date2 = null;
		if (new_date1.lessThan(_date1))
		{
			// Extending so use the old date...
			transfer_date1 = new DateTime(_date1);
		}
		else
		{
			// Shortening so use the new...
			transfer_date1 = new DateTime(new_date1);
		}
		if (new_date2.greaterThan(_date2))
		{
			// Extending so use the old date...
			transfer_date2 = new DateTime(_date2);
		}
		else
		{
			// Shortening so use the new...
			transfer_date2 = new DateTime(new_date2);
		}

		// Now reset the dates and reallocate the period...

		setDate1(new_date1);
		setDate2(new_date2);
		allocateDataSpace();

		// At this point the data space will be completely filled with missing data.

		// Now transfer the data.  To do so, get the
		// old position and then set in the new position.  We are only concerned
		// with transferring the values for the the old time series that are within the new period...

		int column, row, temp_ts_year1 = temp_ts.getDate1().getYear();
		bool internDataFlagStrings = getInternDataFlagStrings();
		for (DateTime date = new DateTime(transfer_date1,DateTime.DATE_FAST); date.lessThanOrEqualTo(transfer_date2); date.addInterval(_data_interval_base, _data_interval_mult))
		{
			// Get the data position for the old data...
			row = date.getYear() - temp_ts_year1;
			column = date.getMonth() - 1; // Zero offset!
			if (_has_data_flags)
			{
				// Transfer the value and flag...
				if (internDataFlagStrings)
				{
					setDataValue(date, data_save[row][column], dataFlagsSave[row][column].intern(), 1);
				}
				else
				{
					setDataValue(date, data_save[row][column], dataFlagsSave[row][column], 1);
				}
			}
			else
			{
				// Transfer just the value...
				setDataValue(date, data_save[row][column]);
			}
		}

		// Add to the genesis...

		addToGenesis("Changed period: " + new_date1 + " to " + new_date2);
	}

	/// <summary>
	/// Clone the object.  The TS base class clone() method is called and then the
	/// the data array is cloned.  The result is a complete deep copy.
	/// </summary>
	public override object clone()
	{
		MonthTS ts = (MonthTS)base.clone(); // Clone data stored in the base class, including properties, limits, etc.
		// This does not seem to do a copy of the primitive data, in the array, as hoped
		//ts._data = (double[][])_data.clone();	// Clone the data (the
							// actual values will
							// be copied since they
							// are primitives).
		//ts._pos = (int [])_pos.clone();
		// Do it the "hard" way...
		if (_data == null)
		{
			ts._data = null;
		}
		else
		{
			ts._data = new double[_data.Length][];
			for (int i = 0; i < _data.Length; i++)
			{
				ts._data[i] = new double[_data[i].Length];
				Array.Copy(_data[i], 0, ts._data[i], 0,_data[i].Length);
			}
		}
		bool internDataFlagStrings = getInternDataFlagStrings();
		if (_has_data_flags)
		{
			if (_dataFlags == null)
			{
				ts._dataFlags = null;
			}
			else
			{
				// Allocate months...
				ts._dataFlags = new string[_dataFlags.Length][];
				for (int iYear = 0; iYear < _dataFlags.Length; iYear++)
				{
					ts._dataFlags[iYear] = new string[_dataFlags[iYear].Length];
					for (int iMonth = 0; iMonth < _dataFlags[iYear].Length; iMonth++)
					{
						if (internDataFlagStrings)
						{
							ts._dataFlags[iYear][iMonth] = _dataFlags[iYear][iMonth].intern();
						}
						else
						{
							ts._dataFlags[iYear][iMonth] = _dataFlags[iYear][iMonth];
						}
					}
				}
			}
		}
		ts._pos = new int[2];
		ts._pos[0] = _pos[0];
		ts._pos[1] = _pos[1];
		// Now the rest of the data (just set the primitive data types)
		ts._min_amon = _min_amon;
		ts._max_amon = _max_amon;
		return ts;
	}

	/// <summary>
	/// Copy the data array from one time series to another. </summary>
	/// <param name="ts"> The time series to copy the data from. </param>
	/// <param name="start_date"> The time to start copying the data (if null, use the first date from this instance). </param>
	/// <param name="end_date"> The time to end copying the data (if null, use the last date from this instance). </param>
	/// <param name="copy_missing"> If true, copy missing data.  If false, ignore missing data. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void copyData(MonthTS ts, RTi.Util.Time.DateTime start_date, RTi.Util.Time.DateTime end_date, boolean copy_missing) throws TSException
	public virtual void copyData(MonthTS ts, DateTime start_date, DateTime end_date, bool copy_missing)
	{
		string message = null, routine = "MonthTS.copyData";

		// Check for null time series...

		if (ts == null)
		{
			message = "Time series to copy is null.";
			Message.printWarning(3, routine, message);
			throw new TSException(message);
		}

		// Get the dates for the loop...

		TSLimits valid_dates = TSUtil.getValidPeriod(this, start_date, end_date);
		DateTime start = valid_dates.getDate1();
		DateTime end = valid_dates.getDate2();

		if (start == null)
		{
			message = "Start date for copy is not set";
			Message.printWarning(2, routine, message);
			end = null;
			valid_dates = null;
			throw new TSException(message);
		}
		if (end == null)
		{
			message = "End date for copy is not set";
			Message.printWarning(2, routine, message);
			start = null;
			valid_dates = null;
			throw new TSException(message);
		}

		if (_data_interval_mult != ts.getDataIntervalMult())
		{
			message = "Data interval multiplier for source does not match copy";
			Message.printWarning(2, routine, message);
			start = null;
			end = null;
			valid_dates = null;
			throw new TSException(message);
		}

		// Now do the transfer...

		DateTime date = new DateTime(start);
		double data_value = 0.0;
		for (; date.lessThanOrEqualTo(end); date.addInterval(_data_interval_base, _data_interval_mult))
		{
			// Get the data value and the position in the matrix...
			data_value = getDataValue(date);
			if (copy_missing)
			{
				// Just do it...
				setDataValue(date, data_value);
			}
			else
			{
				// Only copy if not missing...
				if (!ts.isDataMissing(data_value))
				{
					setDataValue(date, data_value);
				}
			}
		}
		routine = null;
		message = null;
		date = null;
		start = null;
		end = null;
		valid_dates = null;
	}

	/// <summary>
	/// Copy the data array from one time series to another.  The period 
	/// from this time series instance is used for the copy. </summary>
	/// <param name="ts"> The time series to copy the data from. </param>
	/// <param name="copy_missing"> If true, copy missing data.  If false, ignore missing data. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void copyData(MonthTS ts, boolean copy_missing) throws TSException
	public virtual void copyData(MonthTS ts, bool copy_missing)
	{
		try
		{
		copyData(ts, _date1, _date2, copy_missing);
		}
		catch (TSException e)
		{
			throw e;
		}
	}

	/// <summary>
	/// Finalize before garbage collection. </summary>
	/// <exception cref="Throwable"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~MonthTS()
	{
		_data = null;
		_dataFlags = null;
		_pos = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Format the time series for output.  This is not meant to be used for time
	/// series conversion but to produce a general summary of the output.  At this time,
	/// monthly time series are always output in a matrix summary format. </summary>
	/// <returns> Vector of strings that can be displayed, printed, etc. </returns>
	/// <param name="proplist"> Properties of the output, as described in the following table:
	/// 
	/// <table width=100% cellpadding=10 cellspacing=0 border=2>
	/// <tr>
	/// <td><b>Property</b></td>	<td><b>Description</b></td>	<td><b>Default</b></td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>CalendarType</b></td>
	/// <td>The type of calendar, either "Water" (Oct through Sep), "NovToOct"
	/// (Nov through Oct), or "Calendar" (Jan through Dec), consistent with YearType enumeration.
	/// </td>
	/// <td>CalenderYear (but may be made sensitive to the data type or units in the future).</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>Delimiter</b></td>
	/// <td>The delimiter to use for spreadsheet output.  Can be a single or multiple characters.
	/// </td>
	/// <td>|</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>Format</b></td>
	/// <td>The overall format of the output.  Can be either "Summary" for a matrix
	/// similar to the USGS flow data reports or "Spreadsheet" for a delimited
	/// file suitable for a spreadsheet (see the "Delimiter" property).  At this time,
	/// the determination of whether total or average annual values are shown is made
	/// based on units.  AF, ACFT, IN, INCH, FEET, and FOOT are treated as totals and
	/// all others as averages.  A more rigorous handling is being implemented to
	/// use the units dimension to determine totals, etc.</td>
	/// <td>Summary</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>OutputEnd</b></td>
	/// <td>
	/// The ending date/time for output, in a format that can be parsed by DateTime.parse().
	/// </td>
	/// <td>null - output all available data.
	/// </td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>OutputPrecision (also Precision, which is being phased out)</b></td>
	/// <td>The precision of numbers as printed.  All data values are printed in a
	/// 9-digit column.  The precision controls how many digits are shown after the decimal.
	/// </td>
	/// <td>
	/// If not specified, an attempt will be made to use the units to
	/// look up the precision.  If that fails, a default of 1 will be used.
	/// </td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>OutputStart</b></td>
	/// <td>
	/// The starting date/time for output, in a format that can be parsed by DateTime.parse().
	/// </td>
	/// <td>null - output all available data.
	/// </td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>PrintHeader</b></td>
	/// <td>Print the time series header information in a format as follows:
	/// <para>
	/// <pre>
	/// Time Series Identifier  = 07126500.CRDSS_USGS.streamflow.MONTH
	/// Description             = PURGATOIRE RIVER AT NINEMILE DAM, NR HIGBEE, CO.
	/// Data source             = CRDSS_USGS
	/// Data type               = streamflow
	/// Data units              = ACFT
	/// Data interval           = MONTH
	/// Requested Period        = 1980-01 to 1990-01
	/// Available Period        = 1924-01 to 1995-12
	/// </pre>
	/// </td>
	/// <td>true</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>PrintComments</b></td>
	/// <td>Print the comments associated with the time series.  This may contain
	/// information about the quality of data, station information, etc.  This
	/// information is usually viarable-length text, and may not be available.
	/// </td>
	/// <td>true</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>PrintAllStats</b></td>
	/// <td>Print all the statistics (currently maximum, minimum, and mean, although
	/// standard deviation and others are being added).  Because statistics are being
	/// added to output, it is advised that if formatting is to remain the same over
	/// time, that output items be individually specified.  One way of doing this is
	/// to turn all the statistics off and then turn specific items on (to true).
	/// </td>
	/// <td>false</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>PrintGenesis</b></td>
	/// <td>Print the time series genesis information after the header in a format as follows:
	/// </para>
	/// <para>
	/// <pre>
	/// Time series genesis (creation history):
	/// Read from XXXX database.
	/// Filled missing data with...
	/// etc.
	/// </pre>
	/// </td>
	/// <td>true</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>PrintMinStats</b></td>
	/// <td>Print the minimum value statistics.
	/// </td>
	/// <td>true</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>PrintMaxStats</b></td>
	/// <td>Print the maximum value statistics.
	/// </td>
	/// <td>true</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>PrintMeanStats</b></td>
	/// <td>Print the mean value statistics.
	/// </td>
	/// <td>true</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>PrintNotes</b></td>
	/// <td>Print notes about the output.  This consists of helpful information used
	/// to understand the output (but does not consist of data).  For example:
	/// </para>
	/// <para>
	/// <pre>
	/// Notes:
	///  Years shown are calendar years.
	///  Annual values and statistics are computed only on non-missing data.
	///  NC indicates that a value is not computed because of missing data or the data value itself is missing.
	/// </pre>
	/// </td>
	/// <td>true</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>UseCommentsForHeader</b></td>
	/// <td>Use the time series comments for the header and do not print other header
	/// information.  This can be used when the entire header is formatted elsewhere.
	/// </td>
	/// <td>false</td>
	/// </tr>
	/// 
	/// </table>
	/// </para>
	/// </param>
	/// <exception cref="RTi.TS.TSException"> Throws if there is a problem formatting the output. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.util.List<String> formatOutput(RTi.Util.IO.PropList proplist) throws TSException
	public override IList<string> formatOutput(PropList proplist)
	{
		string message = null, routine = "MonthTS.formatOutput";
		if (_data_interval_mult != 1)
		{
			message = "Can only do summary for 1-month time series";
			Message.printWarning(2, "MonthTS.formatOutput", message);
			throw new TSException(message);
		}
		int column, dl = 20, row;
		IList<string> strings = new List<string>();
		PropList props = null;
		string format = "", prop_value = null, year_column = "";
		string data_format = "%9.1f";

		// If the property list is null, allocate one here so we don't have to constantly check for null...

		if (proplist == null)
		{
			// Create a PropList so we don't have to check for nulls all the time.
			props = new PropList("formatOutput");
		}
		else
		{
			props = proplist;
		}

		// Get the important formatting information from the property list...

		// Get the overall format...

		prop_value = props.getValue("Format");
		if (string.ReferenceEquals(prop_value, null))
		{
			// Default to "Summary"...
			format = "Summary";
		}
		else
		{
			// Set to requested format...
			format = prop_value;
		}

		// Determine the units to output.  For now use what is in the time series...

		string req_units = _data_units;

		// Get the precision...

		prop_value = props.getValue("OutputPrecision");
		if (string.ReferenceEquals(prop_value, null))
		{
			// Try older...
			prop_value = props.getValue("Precision");
			if (!string.ReferenceEquals(prop_value, null))
			{
				Message.printWarning(3, routine, "Need to switch Precision property to OutputPrecision");
			}
		}
		if (string.ReferenceEquals(prop_value, null))
		{
			// Try to get units information for default...
			try
			{
				DataUnits u = DataUnits.lookupUnits(req_units);
				data_format = "%9." + u.getOutputPrecision() + "f";
			}
			catch (Exception)
			{
				// Default...
				data_format = "%9.1f";
			}
		}
		else
		{
			// Set to requested precision...
			data_format = "%9." + prop_value + "f";
		}

		// Determine whether water or calendar year...

		prop_value = props.getValue("CalendarType");
		if (string.ReferenceEquals(prop_value, null))
		{
			// Default to "CalendarYear"...
			prop_value = "" + YearType.CALENDAR;
		}
		YearType calendar = YearType.valueOfIgnoreCase(prop_value);

		// Determine the period to output.  For now always output the total...

		if ((_date1 == null) || (_date2 == null))
		{
			message = "Null period dates for time series";
			Message.printWarning(2, routine, message);
			throw new TSException(message);
		}
		DateTime start_date = new DateTime(_date1);
		prop_value = props.getValue("OutputStart");
		if (!string.ReferenceEquals(prop_value, null))
		{
			try
			{
				start_date = DateTime.parse(prop_value);
				start_date.setPrecision(DateTime.PRECISION_MONTH);
			}
			catch (Exception)
			{
				// Default to the time series...
				start_date = new DateTime(_date1);
			}
		}
		DateTime end_date = new DateTime(_date2);
		prop_value = props.getValue("OutputEnd");
		if (!string.ReferenceEquals(prop_value, null))
		{
			try
			{
				end_date = DateTime.parse(prop_value);
				end_date.setPrecision(DateTime.PRECISION_MONTH);
			}
			catch (Exception)
			{
				// Default to the time series...
				end_date = new DateTime(_date2);
			}
		}

		// Now generate the output based on the format...

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Creating output in format \"" + format + "\"");
		}
		if (format.Equals("Spreadsheet", StringComparison.OrdinalIgnoreCase))
		{
			// Spreadsheet
			prop_value = props.getValue("Delimiter");
			if (string.ReferenceEquals(prop_value, null))
			{
				// Default to "|"...
				format = "|";
			}
			else
			{
				// Set to requested delimiter...
				format = prop_value;
			}
			Message.printWarning(3, routine, "Spreadsheet output format is not implemented");
			return strings;
		}
		else if (format.Equals("Summary", StringComparison.OrdinalIgnoreCase))
		{
			// The default output.  In this case, produce a matrix like the one produced by the legacy
			// TSPrintSummaryMatrix.  However, since we now limit the formatting to only monthly
			// time series and the data are already in a monthly time step, do not have to change interval.

			// Print the header for the matrix...

			prop_value = props.getValue("PrintHeader");
			string print_header = null;
			if (string.ReferenceEquals(prop_value, null))
			{
				// Default is true...
				print_header = "true";
			}
			else
			{
				print_header = prop_value;
			}
			prop_value = props.getValue("UseCommentsForHeader");
			string use_comments_for_header = null;
			if (string.ReferenceEquals(prop_value, null))
			{
				// Default is false...
				use_comments_for_header = "false";
			}
			else
			{
				use_comments_for_header = prop_value;
			}
			if (print_header.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				if (!use_comments_for_header.Equals("true", StringComparison.OrdinalIgnoreCase))
				{
					// Format the header...
					strings.Add("");
					IList<string> strings2 = formatHeader();
					StringUtil.addListToStringList(strings, strings2);
				}
			}
			print_header = null;

			// Add comments if available...

			prop_value = props.getValue("PrintComments");
			string print_comments = null;
			if (string.ReferenceEquals(prop_value, null))
			{
				// Default is true...
				print_comments = "true";
			}
			else
			{
				print_comments = prop_value;
			}
			if (print_comments.Equals("true", StringComparison.OrdinalIgnoreCase) || use_comments_for_header.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				strings.Add("");
				if (_comments != null)
				{
					int ncomments = _comments.Count;
					if (!use_comments_for_header.Equals("true", StringComparison.OrdinalIgnoreCase))
					{
						strings.Add("Comments:");
					}
					if (ncomments > 0)
					{
						for (int i = 0; i < ncomments; i++)
						{
							strings.Add(_comments[i]);
						}
					}
					else
					{
						strings.Add("No comments available.");
					}
				}
				else
				{
					strings.Add("No comments available.");
				}
			}

			// Print the genesis information...

			prop_value = props.getValue("PrintGenesis");
			string print_genesis = null;
			if (string.ReferenceEquals(prop_value, null))
			{
				// Default is true...
				print_genesis = "true";
			}
			else
			{
				print_genesis = prop_value;
			}
			if ((_genesis != null) && print_genesis.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				int size = _genesis.Count;
				if (size > 0)
				{
					strings.Add("");
					strings.Add("Time series creation history:");
					strings = StringUtil.addListToStringList(strings, _genesis);
				}
			}
			print_genesis = null;

			// Print the body of the summary...

			// Need to check the data type to determine if it is an average
			// or a total.  For now, make some guesses based on the units...

			strings.Add("");

			if (_id.getType().Equals("ResEOM", StringComparison.OrdinalIgnoreCase))
			{
				// Special cases hard-coded in the short term.
				year_column = "Average";
			}
			else if (req_units.Equals("AF", StringComparison.OrdinalIgnoreCase) || req_units.Equals("ACFT", StringComparison.OrdinalIgnoreCase) || req_units.Equals("FT", StringComparison.OrdinalIgnoreCase) || req_units.Equals("FEET", StringComparison.OrdinalIgnoreCase) || req_units.Equals("FOOT", StringComparison.OrdinalIgnoreCase) || req_units.Equals("IN", StringComparison.OrdinalIgnoreCase) || req_units.Equals("INCH", StringComparison.OrdinalIgnoreCase))
			{
				// FIXME SAM 2009-02-25 Need to remove hard-coded units here
				// Assume totals...
				year_column = "Total";
			}
			else
			{
				// Assume averages...
				year_column = "Average";
			}

			if (calendar == YearType.WATER)
			{
				// Water year...
				strings.Add("Year    Oct       Nov       Dec       Jan       Feb       Mar       Apr       May       Jun       Jul        Aug      Sep     " + year_column);
				strings.Add("---- --------- --------- --------- --------- --------- --------- --------- --------- --------- --------- --------- --------- ---------");
			}
			else if (calendar == YearType.NOV_TO_OCT)
			{
				// Irrigation year...
				strings.Add("Year    Nov       Dec       Jan       Feb       Mar       Apr       May       Jun       Jul       Aug        Sep      Oct     " + year_column);
				strings.Add("---- --------- --------- --------- --------- --------- --------- --------- --------- --------- --------- --------- --------- ---------");
			}
			else
			{
				// Calendar year...
				strings.Add("Year    Jan       Feb       Mar       Apr       May       Jun       Jul        Aug      Sep       Oct       Nov       Dec     " + year_column);
				strings.Add("---- --------- --------- --------- --------- --------- --------- --------- --------- --------- --------- --------- --------- ---------");
			}

			// Now transfer the monthly data into a summary matrix, which looks like:
			//
			// 0 - 11 Total/Ave
			// ...
			// statistics

			// Adjust the start and end dates to be on full years for the calendar that is requested...

			int year_offset = 0;
			if (calendar == YearType.CALENDAR)
			{
				// Just need to output for the full year...
				start_date.setMonth(1);
				end_date.setMonth(12);
			}
			else if (calendar == YearType.NOV_TO_OCT)
			{
				// Need to adjust for the irrigation year to make sure
				// that the first month is Nov and the last is Oct...
				if (start_date.getMonth() < 11)
				{
					// Need to shift to include the previous irrigation year...
					start_date.addYear(-1);
				}
				// Always set the start month to Nov...
				start_date.setMonth(11);
				if (end_date.getMonth() > 11)
				{
					// Need to include the next irrigation year...
					end_date.addYear(1);
				}
				// Always set the end month to Oct...
				end_date.setMonth(10);
				// The year that is printed in the summary is actually
				// later than the calendar for the Nov month...
				year_offset = 1;
			}
			else if (calendar == YearType.WATER)
			{
				// Need to adjust for the water year to make sure that
				// the first month is Oct and the last is Sep...
				if (start_date.getMonth() < 10)
				{
					// Need to shift to include the previous water year...
					start_date.addYear(-1);
				}
				// Always set the start month to Oct...
				start_date.setMonth(10);
				if (end_date.getMonth() > 9)
				{
					// Need to include the next water year...
					end_date.addYear(1);
				}
				// Always set the end month to Sep...
				end_date.setMonth(9);
				// The year that is printed in the summary is actually
				// later than the calendar for the Oct month...
				year_offset = 1;
			}
			// Calculate the number of years...
			// up with the same month as the start month...
			int num_years = (end_date.getAbsoluteMonth() - start_date.getAbsoluteMonth() + 1) / 12;
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "Printing " + num_years + " years of summary for " + start_date.ToString(DateTime.FORMAT_YYYY_MM) + " to " + end_date.ToString(DateTime.FORMAT_YYYY_MM));
			}
			// Allow for total column...
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: double[][] data = new double[num_years][13];
			double[][] data = RectangularArrays.RectangularDoubleArray(num_years, 13);

			// Now loop through the time series and transfer to the proper location in the matrix...
			double year_total = getMissing();
			double data_value;
			DateTime date = new DateTime(start_date,DateTime.DATE_FAST);
			StringBuilder buffer = null;
			int non_missing_in_row = 0;
			// We have adjusted the dates above, so we always start in column 0 (first month in year)...
			column = 0;
			row = 0;
			for (; date.lessThanOrEqualTo(end_date); date.addInterval(_data_interval_base,_data_interval_mult))
			{
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, routine, "Processing " + date.ToString(DateTime.FORMAT_YYYY_MM) + " row:" + row + " column:" + column);
				}
				// Get the data value and the position in the matrix...
				data_value = getDataValue(date);
				// Save the data value for later use in statistics.  Allow missing data values to be saved...
				data[row][column] = data_value;
				// Print out the data value and total/ave, if necessary...
				if (column == 0)
				{
					// Allocate a new buffer and print the year...
					buffer = new StringBuilder();
					buffer.Append(StringUtil.formatString((date.getYear() + year_offset), "%04d") + " ");
					non_missing_in_row = 0;
				}
				// Do not use an else here because if on column 0 still want to print the data value...
				// Print the monthly value...
				if (isDataMissing(data_value))
				{
					buffer.Append("    NC    ");
					data[row][column] = getMissing();
				}
				else
				{
					buffer.Append(StringUtil.formatString(data_value, data_format) + " ");
					if (isDataMissing(year_total))
					{
						year_total = 0.0;
					}
					year_total += data_value;
					++non_missing_in_row;
				}
				if (column == 11)
				{
					// Have processed the last month in the year so process the total or average.  We have been
					// adding to the total, so divide by the number of non-missing for the year if averaging...
					// Now reset the year-value to zero...
					if (isDataMissing(year_total) || (non_missing_in_row != 12))
					{
						buffer.Append("    NC    ");
						data[row][12] = getMissing();
					}
					else
					{
						if (year_column.Equals("Total"))
						{
							buffer.Append(StringUtil.formatString(year_total, data_format));
							data[row][12] = year_total;
						}
						else
						{
							buffer.Append(StringUtil.formatString(year_total / (double)non_missing_in_row,data_format));
							data[row][12] = year_total / (double)non_missing_in_row;
						}
					}
					// Add the row...
					strings.Add(buffer.ToString());
					column = -1; // Incremented at end of loop.
					year_total = getMissing();
					++row;
				}
				++column;
			}
			strings.Add("---- --------- --------- --------- --------- --------- --------- --------- --------- --------- --------- --------- --------- ---------");
			// Now need to do the statistics.  Loop through each column...
			// First check to see if all statistics should be printed (can be dangerous if new statistics are added)..
			prop_value = props.getValue("PrintAllStats");
			string print_all_stats = null;
			if (string.ReferenceEquals(prop_value, null))
			{
				// Default is false...
				print_all_stats = "false";
			}
			else
			{
				print_all_stats = prop_value;
			}
			// Now start on the minimum...
			prop_value = props.getValue("PrintMinStats");
			string print_min = null;
			if (string.ReferenceEquals(prop_value, null))
			{
				// Default is true...
				print_min = "true";
			}
			else
			{
				print_min = prop_value;
			}
			if (print_min.Equals("true", StringComparison.OrdinalIgnoreCase) || print_all_stats.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				strings = StringUtil.addListToStringList(strings, formatOutputStats(data,num_years,"Min ", data_format));
			}
			print_min = null;
			prop_value = props.getValue("PrintMaxStats");
			string print_max = null;
			if (string.ReferenceEquals(prop_value, null))
			{
				// Default is true...
				print_max = "true";
			}
			else
			{
				print_max = prop_value;
			}
			if (print_max.Equals("true", StringComparison.OrdinalIgnoreCase) || print_all_stats.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				strings = StringUtil.addListToStringList(strings, formatOutputStats(data,num_years,"Max ", data_format));
			}
			print_max = null;
			prop_value = props.getValue("PrintMeanStats");
			string print_mean = null;
			if (string.ReferenceEquals(prop_value, null))
			{
				// Default is true...
				print_mean = "true";
			}
			else
			{
				print_mean = prop_value;
			}
			if (print_mean.Equals("true", StringComparison.OrdinalIgnoreCase) || print_all_stats.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				strings = StringUtil.addListToStringList(strings, formatOutputStats(data,num_years,"Mean", data_format));
			}
			print_mean = null;
			print_all_stats = null;

			// Now do the notes...

			prop_value = props.getValue("PrintNotes");
			string print_notes = null;
			if (string.ReferenceEquals(prop_value, null))
			{
				// Default is true...
				print_notes = "true";
			}
			else
			{
				print_notes = prop_value;
			}
			if (print_notes.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				strings.Add("");
				strings.Add("Notes:");
				if (calendar == YearType.WATER)
				{
					strings.Add("  Years shown are water years.");
					strings.Add("  A water year spans Oct of the previous calendar year to Sep of the current calendar year (all within the indicated water year).");
				}
				else if (calendar == YearType.NOV_TO_OCT)
				{
					strings.Add("  Years shown span Nov of the previous calendar year to Oct of the current calendar year.");
				}
				else
				{
					strings.Add("  Years shown are calendar years.");
				}
				strings.Add("  Annual values and statistics are computed only on non-missing data.");
				strings.Add("  NC indicates that a value is not computed because of missing data or the data value itself is missing.");
			}
		}
		else
		{
			message = "Unrecognized format: \"" + format + "\"";
			Message.printWarning(1, routine, message);
			throw new TSException(message);
		}

		return strings;
	}

	/// <summary>
	/// Format the time series for output. </summary>
	/// <returns> list of strings that are written to the file. </returns>
	/// <param name="fp"> Writer to receive output. </param>
	/// <param name="props"> Properties to modify output. </param>
	/// <exception cref="RTi.TS.TSException"> Throws if there is an error writing the output. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.util.List<String> formatOutput(java.io.PrintWriter fp, RTi.Util.IO.PropList props) throws TSException
	public override IList<string> formatOutput(PrintWriter fp, PropList props)
	{
		IList<string> formatted_output = null;
		string routine = "MonthTS.formatOutput(Writer,props)";
		int dl = 20;
		string message;

		if (fp == null)
		{
			message = "Null PrintWriter for output";
			Message.printWarning(2, routine, message);
			throw new TSException(message);
		}

		// First get the formatted output...

		try
		{
			formatted_output = formatOutput(props);
			if (formatted_output != null)
			{
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, routine, "Formatted output is " + formatted_output.Count + " lines");
				}

				// Now write each string to the writer...

				string newline = System.getProperty("line.separator");
				int size = formatted_output.Count;
				for (int i = 0; i < size; i++)
				{
					fp.print(formatted_output[i] + newline);
				}
				newline = null;
			}
		}
		catch (TSException e)
		{
			// Rethrow...
			throw e;
		}

		// Also return the list (consistent with C++ single return type.

		return formatted_output;
	}

	/// <summary>
	/// Format the time series for output. </summary>
	/// <returns> list of strings that are written to the file. </returns>
	/// <param name="fname"> Name of output. </param>
	/// <param name="props"> Property list containing output modifiers. </param>
	/// <exception cref="RTi.TS.TSException"> Throws if there is an error writing the output. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.util.List<String> formatOutput(String fname, RTi.Util.IO.PropList props) throws TSException
	public override IList<string> formatOutput(string fname, PropList props)
	{
		string message = null;
		IList<string> formatted_output = null;
		PrintWriter stream = null;
		string full_fname = IOUtil.getPathUsingWorkingDir(fname);

		// First open the output file...

		try
		{
			stream = new PrintWriter(new StreamWriter(full_fname));
		}
		catch (Exception)
		{
			message = "Unable to open file \"" + full_fname + "\"";
			throw new TSException(message);
		}

		try
		{
			formatted_output = formatOutput(stream, props);
		}
		catch (TSException e)
		{
			// Rethrow...
			throw e;
		}
		finally
		{
			if (stream != null)
			{
				stream.close();
			}
		}

		// Also return the list (consistent with C++ single return type)...

		return formatted_output;
	}

	/// <summary>
	/// Format the output statistics row given the data array.  May use the TSUtil version in the future.
	/// </summary>
	private IList<string> formatOutputStats(double[][] data, int num_years, string label, string data_format)
	{
		IList<string> strings = new List<string>();
		double stat;
		StringBuilder buffer = null;
		double[] array = new double[num_years];
		int column, row;

		for (column = 0; column < 13; column++)
		{
			if (column == 0)
			{
				buffer = new StringBuilder();
				buffer.Append(label);
			}
			// Extract the non-missing values...
			int num_not_missing = 0;
			for (row = 0; row < num_years; row++)
			{
				if (!isDataMissing(data[row][column]))
				{
					++num_not_missing;
				}
			}
			if (num_not_missing > 0)
			{
				// Transfer to an array...
				array = new double[num_not_missing];
				num_not_missing = 0;
				for (row = 0; row < num_years; row++)
				{
					if (!isDataMissing(data[row][column]))
					{
						array[num_not_missing] = data[row][column];
						++num_not_missing;
					}
				}
				stat = 0.0;
				try
				{
					if (label.StartsWith("Min", StringComparison.Ordinal))
					{
						stat = MathUtil.min(array);
					}
					else if (label.StartsWith("Max", StringComparison.Ordinal))
					{
						stat = MathUtil.max(array);
					}
					else if (label.StartsWith("Mean", StringComparison.Ordinal))
					{
						stat = MathUtil.mean(array);
					}
				}
				catch (Exception)
				{
				}
				buffer.Append(StringUtil.formatString(stat," " + data_format));
			}
			else
			{
				buffer.Append("     NC   ");
			}
		}
		strings.Add(buffer.ToString());
		strings.Add("---- --------- --------- --------- --------- --------- --------- --------- --------- --------- --------- --------- --------- ---------");

		return strings;
	}

	/// <summary>
	/// Return the data point for a date.
	/// <pre>
	///             Monthly data is stored in a two-dimensional array:
	///  		     |----------------> 12 calendar months
	///  		     |
	///  		    \|/
	///  		   year 
	/// </pre> </summary>
	/// <param name="date"> date/time to get data. </param>
	/// <param name="tsdata"> if null, a new instance of TSData will be returned.  If non-null, the provided
	/// instance will be used (this is often desirable during iteration to decrease memory use and
	/// increase performance). </param>
	/// <returns> a TSData for the specified date/time. </returns>
	/// <seealso cref= TSData </seealso>
	public override TSData getDataPoint(DateTime date, TSData tsdata)
	{
		if (tsdata == null)
		{
			// Allocate it (this is the only method that uses it and don't want to waste memory)...
			tsdata = new TSData();
		}
		if ((date.lessThan(_date1)) || (date.greaterThan(_date2)))
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(50, "MonthTS.getDataValue", date + " not within POR (" + _date1 + " - " + _date2 + ")");
			}
			tsdata.setValues(date, _missing, _data_units, "", 0);
			return tsdata;
		}
		getDataPosition(date);
		if (_has_data_flags)
		{
			if (_internDataFlagStrings)
			{
				tsdata.setValues(date, getDataValue(date), _data_units, _dataFlags[_pos[0]][_pos[1]].intern(), 0);
			}
			else
			{
				tsdata.setValues(date, getDataValue(date), _data_units, _dataFlags[_pos[0]][_pos[1]], 0);
			}
		}
		else
		{
			tsdata.setValues(date, getDataValue(date), _data_units, "", 0);
		}
		return tsdata;
	}

	// TODO SAM 2010-07-30 Evaluate how to make private - currently StringMonthTS uses
	/// <summary>
	/// Return the data position.
	/// <pre>
	///              Monthly data is stored in a two-dimensional array:
	///  		     |----------------> 12 calendar months
	///  		     |
	///  		    \|/
	///  		   year 
	/// </pre> </summary>
	/// <returns> An array of integers containing the position in the data array
	/// corresponding to the date.  Return null if the date is outside the period of
	/// record.  The array that is used is re-used in order to increase performance.  You
	/// must copy the information after the return to ensure protecting the data. </returns>
	/// <param name="date"> Date of interest. </param>
	protected internal virtual int [] getDataPosition(DateTime date)
	{ // Do not define routine here to increase performance.

		// We don't care if data exists, but do care if dates are null...

		if (date == null)
		{
			return null;
		}
		if ((_date1 == null) || (_date2 == null))
		{
			return null;
		}

		// Check the date coming in...

		int amon = date.getAbsoluteMonth();

		if ((amon < _min_amon) || (amon > _max_amon))
		{
			// Print within debug to optimize performance...
			if (Message.isDebugOn)
			{
				Message.printWarning(50, "MonthTS.getDataPosition", date + " not within POR (" + _date1 + " - " + _date2 + ")");
			}
			return null;
		}

		_pos[0] = date.getYear() - _date1.getYear();
		_pos[1] = date.getMonth() - 1; // Zero offset!

		return _pos;
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
	/// Returns the data in the specified DataFlavor, or null if no matching flavor
	/// exists.  From the Transferable interface.
	/// Supported data flavors are:<br>
	/// <ul>
	/// <li>MonthTS - MonthTS.class / RTi.TS.MonthTS</li>
	/// <li>TS - TS.class / RTi.TS.TS</li>
	/// <li>TSIdent - TSIdent.class / RTi.TS.TSIdent</li></ul> </summary>
	/// <param name="flavor"> the flavor in which to return the data. </param>
	/// <returns> the data in the specified DataFlavor, or null if no matching flavor exists. </returns>
	public override object getTransferData(DataFlavor flavor)
	{
		if (flavor.Equals(monthTSFlavor))
		{
			return this;
		}
		else if (flavor.Equals(TS.tsFlavor))
		{
			return this;
		}
		else if (flavor.Equals(TSIdent.tsIdentFlavor))
		{
			return _id;
		}
		else
		{
			return null;
		}
	}

	/// <summary>
	/// Returns the flavors in which data can be transferred.  From the Transferable interface.  
	/// The order of the dataflavors that are returned are:<br>
	/// <ul>
	/// <li>MonthTS - MonthTS.class / RTi.TS.MonthTS</li>
	/// <li>TS - TS.class / RTi.TS.TS</li>
	/// <li>TSIdent - TSIdent.class / RTi.TS.TSIdent</li></ul> </summary>
	/// <returns> the flavors in which data can be transferred. </returns>
	public override DataFlavor[] getTransferDataFlavors()
	{
		DataFlavor[] flavors = new DataFlavor[3];
		flavors[0] = monthTSFlavor;
		flavors[1] = TS.tsFlavor;
		flavors[2] = TSIdent.tsIdentFlavor;
		return flavors;
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
	/// Determines whether the specified flavor is supported as a transfer flavor.
	/// From the Transferable interface.  Supported data flavors are:<br>
	/// <ul>
	/// <li>MonthTS - MonthTS.class / RTi.TS.MonthTS</li>
	/// <li>TS - TS.class / RTi.TS.TS</li>
	/// <li>TSIdent - TSIdent.class / RTi.TS.TSIdent</li></ul> </summary>
	/// <param name="flavor"> the flavor to check. </param>
	/// <returns> true if data can be transferred in the specified flavor, false if not. </returns>
	public override bool isDataFlavorSupported(DataFlavor flavor)
	{
		if (flavor.Equals(monthTSFlavor))
		{
			return true;
		}
		else if (flavor.Equals(TS.tsFlavor))
		{
			return true;
		}
		else if (flavor.Equals(TSIdent.tsIdentFlavor))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Refresh derived data (e.g., data limits) if data have been set.  This is called from other package methods.
	/// </summary>
	public override void refresh()
	{ // If the data is not dirty, then we do not have to refresh the other information...

		if (!_dirty)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(30, "MonthTS.refresh", "Time series is not dirty.  Not recomputing limits");
			}
			return;
		}

		// Else we need to refresh...

		if (Message.isDebugOn)
		{
			Message.printDebug(30, "MonthTS.refresh", "Time Series is dirty. Recomputing limits");
		}

		TSLimits limits = TSUtil.getDataLimits(this, _date1, _date2, false);
		if (limits.areLimitsFound())
		{
			// Now reset the limits for the time series...
			setDataLimits(limits);
		}

		_dirty = false;
		limits = null;
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

	/// <summary>
	/// Set the data value for the specified date. </summary>
	/// <param name="date"> Date of interest. </param>
	/// <param name="value"> Value corresponding to date. </param>
	/// <param name="data_flag"> Data flag for value. </param>
	/// <param name="duration"> Duration for value (ignored - assumed to be 1-month or instantaneous depending on data type). </param>
	public override void setDataValue(DateTime date, double value, string data_flag, int duration)
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

		// THIS CODE NEEDS TO BE EQUIVALENT IN getDataValue...

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
		if ((!string.ReferenceEquals(data_flag, null)) && (data_flag.Length > 0))
		{
			if (!_has_data_flags)
			{
				// Trying to set a data flag but space has not been allocated, so allocate the flag space
				try
				{
					allocateDataFlagSpace(null, false);
				}
				catch (Exception e)
				{
					// Generally should not happen - log as debug because could generate a lot of warnings
					if (Message.isDebugOn)
					{
						Message.printDebug(30, "MonthTS.setDataValue", "Error allocating data flag space (" + e + ") - will not use flags.");
					}
					// Make sure to turn flags off
					_has_data_flags = false;
				}
			}
		}
		if (_has_data_flags && (!string.ReferenceEquals(data_flag, null)))
		{
			if (_internDataFlagStrings)
			{
				_dataFlags[row][column] = data_flag.intern();
			}
			else
			{
				_dataFlags[row][column] = data_flag;
			}
		}
	}

	public virtual double getMaxValue()
	{
		double max = -999.0;
		for (int i = 0; i < _data.Length; i++)
		{
			for (int j = 0; j < _data[i].Length; j++)
			{
				if (_data[i][j] > max)
				{
					max = _data[i][j];
				}
			}
		}
		return max;
	}

	}

}