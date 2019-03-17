using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// MinuteTS - base class from which all minute time series are derived

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
// MinuteTS - base class from which all minute time series are derived
// ----------------------------------------------------------------------------
// Notes:	(1)	This base class is provided so that specific minute
//			time series can derived from this class.
//		(2)	Data for this time series interval is stored as follows:
//
//			The first dimension is the absolute month, similar to
//			HourTS.  The second dimension is the days in the month.
//			The third dimension is the number of minute data
//			intervals within the day.
//
//			So, the base block of storage is the month and day.
//			This lends itself to very fast data retrieval but may
//			waste some memory for short time series in which full
//			days are not stored.  This is considered a reasonable
//			tradeoff.
// ----------------------------------------------------------------------------
// History:
//
// 26 Feb 1998	Steven A. Malers, RTi	Copy HourTS and modify.  Compare to the
//					existing C++ version.
// 06 Aug 1998	SAM, RTi		Optimize data set/get by modifying
//					getDataPosition to use class data.
// 22 Aug 1998	SAM, RTi		Change getDataPosition to return int[].
// 13 Apr 1999	SAM, RTi		Add finalize.
// 21 Feb 2001	SAM, RTi		Add clone() and copy constructor.
//					Remove printSample() and read/write
//					methods.
// 28 Aug 2001	SAM, RTi		Fix clone().  Clean up javadoc.  Set
//					unused variables to null.
// 09 Sep 2001	SAM, RTi		Update formatOutput() to actually do a
//					nice report (previously was not
//					supported - copy the HourTS code and
//					modify as needed).  Only support summary
//					output since the delimited and other
//					formats can be produced by DateValueTS,
//					etc.
// 2001-11-06	SAM, RTi		Review javadoc.  Verify that variables
//					are set to null when no longer used.
//					Remove constructor that takes a file
//					name.  Change some methods to have void
//					return type to agree with base class.
// 2003-01-08	SAM, RTi		Add hasData().
// 2003-06-02	SAM, RTi		Upgrade to use generic classes.
//					* Change TSDate to DateTime.
//					* Change TSUnits to DataUnits.
//					* Change TSTimeZone to TZ.
//					* Change TS.INTERVAL* to TimeInterval.
// 2004-01-26	SAM, RTi		* Add OutputStart and OutputEnd
//					  properties to formatOutput().
//					* In formatOutput(), convert the file
//					  name to a full path.
// 2004-03-04	J. Thomas Sapienza, RTi	* Class now implements Serializable.
//					* Class now implements Transferable.
//					* Class supports being dragged or 
//					  copied to clipboard.
// 2005-06-02	SAM, RTi		* Fix getDataPoint() to return a TSData
//					  with missing data if the date is
//					  outside the period of record.
//					* Add _tsdata to increase peformance.
//					* Remove warning about reallocating
//					  memory.
//					* Add support for data flags.
//					* Fix bug in clone - it was not properly
//					  handling unallocated space at the
//					  beginning and ends of months.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------
// EndHeader

namespace RTi.TS
{


	using DataUnits = RTi.Util.IO.DataUnits;
	using IOUtil = RTi.Util.IO.IOUtil;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using DateTime = RTi.Util.Time.DateTime;
	using TimeInterval = RTi.Util.Time.TimeInterval;
	using TimeUtil = RTi.Util.Time.TimeUtil;
	using YearType = RTi.Util.Time.YearType;

	/// <summary>
	/// The MinuteTS class is the base class for time series used to store minute data.
	/// Derive classes from this class for specific minute time series data.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class MinuteTS extends TS implements Cloneable, java.io.Serializable, java.awt.datatransfer.Transferable
	[Serializable]
	public class MinuteTS : TS, ICloneable, Transferable
	{

	// Data members...

	//FIXME SAM 2009-01-15 Need to separate the data transfer code from normal classes
	/// <summary>
	/// The DataFlavor for transferring this specific class.
	/// </summary>
	public static DataFlavor minuteTSFlavor = new DataFlavor(typeof(RTi.TS.MinuteTS), "RTi.TS.MinuteTS");

	/// <summary>
	/// Data space for minute time series.  The dimensions are [month][day][minute value].
	/// </summary>
	private double[][][] _data;

	/// <summary>
	/// Data flags for each data value.  The dimensions are [month][day][minute value].
	/// </summary>
	private string[][][] _dataFlags;

	/// <summary>
	/// Month position in data array, set by getDataPosition() and used internally.
	/// </summary>
	private int _month_pos;

	/// <summary>
	/// Day position in data array, set by getDataPosition() and used internally.
	/// </summary>
	private int _day_pos;

	/// <summary>
	/// Interval position in data array, set by getDataPosition() and used internally.
	/// </summary>
	private int _interval_pos;

	/// <summary>
	/// Return for the following to optimize memory use.
	/// </summary>
	private int[] _pos = null;

	/// <summary>
	/// Default constructor.
	/// </summary>
	public MinuteTS() : base()
	{
		init();
	}

	/// <summary>
	/// Copy constructor.  Everything is copied by calling copyHeader() and then copying the data values. </summary>
	/// <param name="ts"> MinuteTS to copy. </param>
	public MinuteTS(MinuteTS ts)
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
	}

	/// <summary>
	/// Allocate the data flag space for the time series.  This requires that the data
	/// interval base and multiplier are set correctly and that _date1 and _date2 have
	/// been set.  The allocateDataSpace() method will allocate the data flags if
	/// appropriate.  Use this method when the data flags need to be allocated after the initial allocation. </summary>
	/// <param name="initialValue"> Initial value (null is allowed and will result in the flags being initialized to spaces). </param>
	/// <param name="retain_previous_values"> If true, the array size will be increased if necessary, but
	/// previous data values will be retained.  If false, the array will be reallocated and initialized to spaces. </param>
	/// <exception cref="Exception"> if there is an error allocating the memory. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void allocateDataFlagSpace(String initialValue, boolean retain_previous_values) throws Exception
	public override void allocateDataFlagSpace(string initialValue, bool retain_previous_values)
	{
		string routine = "MinuteTS.allocateDataFlagSpace", message;
		int i;

		if ((_date1 == null) || (_date2 == null))
		{
			message = "Dates have not been set.  Cannot allocate data space";
			Message.printWarning(2, routine, message);
			throw new Exception(message);
		}
		if ((_data_interval_mult < 1) || (_data_interval_mult > 60))
		{
			message = "Only know how to handle 1-60 minute data, not " + _data_interval_mult + "-minute";
			Message.printWarning(3, routine, message);
			throw new Exception(message);
		}

		if (string.ReferenceEquals(initialValue, null))
		{
			initialValue = "";
		}

		int nmonths = _date2.getAbsoluteMonth() - _date1.getAbsoluteMonth() + 1;

		if (nmonths == 0)
		{
			message = "TS has 0 months POR, maybe Dates haven't been set yet";
			Message.printWarning(2, routine, message);
			throw new Exception(message);
		}

		string[][][] dataFlagsPrev = null;
		if (_has_data_flags && retain_previous_values)
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
		_dataFlags = new string[nmonths][][];

		// Set the counter date to match the starting month.  This date is used
		// to determine the number of days in each month.

		DateTime date = new DateTime(DateTime.DATE_FAST);
		date.setMonth(_date1.getMonth());
		date.setYear(_date1.getYear());

		// Allocate memory...

		int j = 0, k = 0;
		int nvals = 0;
		int ndays_in_month = 0;
		int day;
		bool internDataFlagStrings = getInternDataFlagStrings();
		for (i = 0; i < nmonths; i++, date.addMonth(1))
		{
			ndays_in_month = TimeUtil.numDaysInMonth(date);

			_dataFlags[i] = new string[ndays_in_month][];
			for (j = 0; j < ndays_in_month; j++)
			{
				if (i == 0)
				{
					// In the first month.  If the day is less than
					// the first day in the period, do not use up memory...
					day = j + 1;
					if (day < _date1.getDay())
					{
						continue;
					}
				}
				else if ((i + 1) == nmonths)
				{
					// In the last month.  If the day is greater
					// than the last day in the period, do not use up memory...
					day = j + 1;
					if (day > _date2.getDay())
					{
						continue;
					}
				}
				// Else we do allocate memory for some data during the day.
				// If a non-valid interval, an exception was thrown above...
				nvals = 24 * (60 / _data_interval_mult);
				_dataFlags[i][j] = new string[nvals];

				// Now fill with the initial data value...

				for (k = 0; k < nvals; k++)
				{
					if (internDataFlagStrings)
					{
						_dataFlags[i][j][k] = initialValue.intern();
					}
					else
					{
						_dataFlags[i][j][k] = initialValue;
					}
					if (retain_previous_values && (dataFlagsPrev != null))
					{
						// Copy over the old values (typically shorter character arrays)...
						if (internDataFlagStrings)
						{
							_dataFlags[i][j][k] = dataFlagsPrev[i][j][k].intern();
						}
						else
						{
							_dataFlags[i][j][k] = dataFlagsPrev[i][j][k];
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// Allocate the data space for the time series.  The beginning and ending dates
	/// and interval multiplier must have been set. </summary>
	/// <returns> 0 if successful, 1 if failure. </returns>
	public override int allocateDataSpace()
	{
		string routine = "MinuteTS.allocateDataSpace";
		int dl = 10, imon, ndays_in_month, nmonths = 0, nvals;

		if ((_date1 == null) || (_date2 == null))
		{
			Message.printWarning(3, routine, "No dates set for memory allocation");
			return 1;
		}
		if ((_data_interval_mult < 1) || (_data_interval_mult > 60))
		{
			Message.printWarning(3, routine, "Only know how to handle 1-60 minute data, not " + _data_interval_mult + "-minute");
			return 1;
		}
		nmonths = _date2.getAbsoluteMonth() - _date1.getAbsoluteMonth() + 1;

		if (nmonths == 0)
		{
			Message.printWarning(2, routine, "TS has 0 months POR, maybe Dates haven't been set yet");
			return 1;
		}

		_data = new double[nmonths][][];
		if (_has_data_flags)
		{
			_dataFlags = new string[nmonths][][];
		}

		// Probably need to catch an exception here in case we run out of memory.

		// Set the counter date to match the starting month.  This date is used
		// to determine the number of days in each month.

		DateTime date = new DateTime(DateTime.DATE_FAST);
		date.setMonth(_date1.getMonth());
		date.setYear(_date1.getYear());

		int day, iday, k;
		for (imon = 0; imon < nmonths; imon++, date.addMonth(1))
		{
			ndays_in_month = TimeUtil.numDaysInMonth(date);

			// Allocate the memory for the number of days in the month...

			_data[imon] = new double [ndays_in_month][];
			if (_has_data_flags)
			{
				_dataFlags[imon] = new string[ndays_in_month][];
			}

			// Now allocate the memory for the number of intervals in the
			// day.  Need to do this to save on memory:  if a day is not in
			// our period, skip this step but do allocate the full day for
			// any day that does occur in the period.

			for (iday = 0; iday < ndays_in_month; iday++)
			{
				if (imon == 0)
				{
					// In the first month.  If the day is less than
					// the first day in the period, do not use up
					// memory.  The position will never be accessed.
					day = iday + 1;
					if (day < _date1.getDay())
					{
						continue;
					}
				}
				else if ((imon + 1) == nmonths)
				{
					// In the last month.  If the day is greater than the last day in the period, do not use
					// up memory.  The position will never be accessed.
					day = iday + 1;
					if (day > _date2.getDay())
					{
						continue;
					}
				}
				// Else we do allocate memory for some data during the day...
				// Easy to handle 1-60 minute data...
				// 24 = number of hours in the day...
				nvals = 24 * (60 / _data_interval_mult);
				_data[imon][iday] = new double[nvals];
				if (_has_data_flags)
				{
					_dataFlags[imon][iday] = new string[nvals];
				}

				// Now fill the entire month with the missing data value...

				for (k = 0; k < nvals; k++)
				{
					_data[imon][iday][k] = _missing;
					if (_has_data_flags)
					{
						_dataFlags[imon][iday][k] = "";
					}
				}
			}
		}

		// Use the static routine to compute the data size...

		int nactual = calculateDataSize(_date1, _date2, _data_interval_mult);
		setDataSize(nactual);

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Allocated " + _data_interval_mult + "-minute data space from " + _date1.ToString() + " to " + _date2.ToString() + " (" + nactual + " values)");
		}
		return 0;
	}

	/// <summary>
	/// Determine the number of points between two dates. </summary>
	/// <returns> The number of data points for a minute time series
	/// given the data interval multiplier for the specified period. </returns>
	/// <param name="start_date"> The first date of the period. </param>
	/// <param name="end_date"> The last date of the period. </param>
	/// <param name="interval_mult"> The time series data interval multiplier. </param>
	public static int calculateDataSize(DateTime start_date, DateTime end_date, int interval_mult)
	{
		string routine = "MinuteTS.calculateDataSize";
		int datasize = 0;

		if (start_date == null)
		{
			Message.printWarning(2, routine, "Start date is null");
			routine = null;
			return 0;
		}
		if (end_date == null)
		{
			Message.printWarning(2, routine, "End date is null");
			routine = null;
			return 0;
		}

		// First set to the number of data in the months...
		datasize = TimeUtil.numDaysInMonths(start_date.getMonth(), start_date.getYear(), end_date.getMonth(), end_date.getYear()) * 24 * 60 / interval_mult;
		// Now subtract off the data at the ends that are missing...
		// Start by subtracting the full day at the beginning of the
		// month that is not included...
		datasize -= (start_date.getDay() - 1) * 24 * 60 / interval_mult;
		// Now subtract the recordings on the first day before the first time...
		datasize -= start_date.getHour() * 60 / interval_mult;
		// Now subtract off the data at the end...
		// Start by subtracting the full days off the end of the month...
		int ndays_in_month = TimeUtil.numDaysInMonth(end_date.getMonth(), end_date.getYear());
		datasize -= (ndays_in_month - end_date.getDay()) * 24 * 60 / interval_mult;
		// Now subtract the readings at the end of the last day...
		datasize -= (23 - end_date.getHour()) * 60 / interval_mult;
		return datasize;
	}

	/// <summary>
	/// Change the period of record to the specified dates.  If the period is extended,
	/// missing data will be used to fill the time series.  If the period is shortened, data will be lost. </summary>
	/// <param name="date1"> New start date of time series. </param>
	/// <param name="date2"> New end date of time series. </param>
	/// <exception cref="RTi.TS.TSException"> if there is a problem extending the data. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void changePeriodOfRecord(RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2) throws TSException
	public override void changePeriodOfRecord(DateTime date1, DateTime date2)
	{
		string routine = "MinuteTS.changePeriodOfRecord";
		string message;

		// To transfer, allocate a new data space.  In any case, need to get the dates established...
		if ((date1 == null) && (date2 == null))
		{
			// No dates.  Cannot change.
			message = "\"" + _id + "\": period dates are null.  Cannot change the period.";
			Message.printWarning(2, routine, message);
			throw new TSException(message);
		}

		// Allowed to have one be null...

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
			return;
		}

		// To transfer the data (later), get the old position and then set in
		// the new position.  To get the right data position, declare a
		// temporary HourTS with the old dates and save a reference to the old data...

		double[][][] dataSave = _data;
		string[][][] dataFlagsSave = _dataFlags;
		MinuteTS temp_ts = new MinuteTS();
		temp_ts.setDataInterval(TimeInterval.MINUTE, _data_interval_mult);
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

		double value;
		int[] data_pos;
		for (DateTime date = new DateTime(transfer_date1,DateTime.DATE_FAST); date.lessThanOrEqualTo(transfer_date2); date.addInterval(_data_interval_base, _data_interval_mult))
		{
			// Get the data position for the old data...
			data_pos = temp_ts.getDataPosition(date);
			// Now get the value...
			value = dataSave[data_pos[0]][data_pos[1]][data_pos[2]];
			// Now set in the new period...
			// Also transfer the data flag...
			if (_has_data_flags)
			{
				// Transfer the value and flag...
				if (_internDataFlagStrings)
				{
					setDataValue(date, value, dataFlagsSave[data_pos[0]][data_pos[1]][data_pos[2]].intern(), 1);
				}
				else
				{
					setDataValue(date, value, dataFlagsSave[data_pos[0]][data_pos[1]][data_pos[2]], 1);
				}
			}
			else
			{
				// Transfer the value...
				setDataValue(date, value);
			}
		}

		// Add to the genesis...

		addToGenesis("Changed period from: " + temp_ts.getDate1() + " - " + temp_ts.getDate2() + " to " + new_date1 + " - " + new_date2);
	}

	/// <summary>
	/// Clone the object.  The TS base class clone() method is called and then the
	/// the data array is cloned.  The result is a complete deep copy.
	/// </summary>
	public override object clone()
	{
		MinuteTS ts = (MinuteTS)base.clone(); // Clone data stored in the base
		// Does not appear to work...
		//ts._data = (double[][][])_data.clone();
		//ts._pos = (int[])_pos.clone();
		if (_data == null)
		{
			ts._data = null;
		}
		else
		{
			ts._data = new double[_data.Length][][];
			for (int imon = 0; imon < _data.Length; imon++)
			{ // Months
				ts._data[imon] = new double[_data[imon].Length][];
				for (int iday = 0; iday < _data[imon].Length; iday++)
				{ // Days in month
					if (_data[imon][iday] == null)
					{
						// Memory is not allocated for days at the beginning and
						// ends of months (outside the period), in order
						// to save memory and increase performance.
						continue;
					}
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: ts._data[imon][iday] = new double[_data[imon][iday].Length];
					ts._data[imon][iday] = RectangularArrays.RectangularDoubleArray(_data[imon][iday].Length);
					Array.Copy(_data[imon][iday], 0, ts._data[imon][iday], 0,_data[imon][iday].Length);
				}
			}
		}
		bool internDataFlagStrings = getInternDataFlagStrings();
		if (_has_data_flags)
		{
			if (_dataFlags == null)
			{
				// Original time series did not have flags so can't copy
				// TODO SAM 2011-12-07 Should this be an error?
				ts._dataFlags = null;
			}
			else
			{
				// Allocate months...
				ts._dataFlags = new string[_dataFlags.Length][][];
				for (int imon = 0; imon < _dataFlags.Length; imon++)
				{
					// Allocate days in month...
					ts._dataFlags[imon] = new string[_dataFlags[imon].Length][];
					for (int iday = 0; iday < _dataFlags[imon].Length; iday++)
					{
						if (_dataFlags[imon][iday] == null)
						{
							// Memory is not allocated for days at the beginning and
							// ends of months (outside the period), in order
							// to save memory and increase performance.
							continue;
						}
						// Allocate data values in day...
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: ts._dataFlags[imon][iday] = new string[_dataFlags[imon][iday].Length];
						ts._dataFlags[imon][iday] = RectangularArrays.RectangularStringArray(_dataFlags[imon][iday].Length);
						for (int ival = 0; ival < _dataFlags[imon][iday].Length; ival++)
						{
							// Allocate values for minute intervals in day...
							if (internDataFlagStrings)
							{
								ts._dataFlags[imon][iday][ival] = _dataFlags[imon][iday][ival].intern();
							}
							else
							{
								ts._dataFlags[imon][iday][ival] = _dataFlags[imon][iday][ival];
							}
						}
					}
				}
			}
		}
		ts._pos = new int[3];
		ts._pos[0] = _pos[0];
		ts._pos[1] = _pos[1];
		ts._pos[2] = _pos[2];
		ts._month_pos = _month_pos;
		ts._day_pos = _day_pos;
		ts._interval_pos = _interval_pos;
		return ts;
	}

	/// <summary>
	/// Finalize before garbage collection. </summary>
	/// <exception cref="Throwable"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~MinuteTS()
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
	/// minute time series are always output in a matrix summary format. </summary>
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
	/// <td>CalanderYear (but may be made sensitive to the data type or units in the future).</td>
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
	/// Time Series Identifier  = 07126500.CRDSS_USGS.streamflow.24
	/// Description             = PURGATOIRE RIVER AT NINEMILE DAM, NR HIGBEE, CO.
	/// Data source             = CRDSS_USGS
	/// Data type               = streamflow
	/// Data interval           = 24Hour
	/// Data units              = CFS
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
	/// information is usually variable-length text, and may not be available.
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
	/// <td>Print the time series genesis information after the header in a
	/// format as follows:
	/// </para>
	/// <para>
	/// <pre>
	/// Time series creation history:
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
		string message = "", routine = "MinuteTS.formatOutput", year_column = "";
		IList<string> strings = new List<string>();
		PropList props = null;
		string data_format = "%9.1f", prop_value = null;

		// Only know how to do this for 24-hour time series (in the future may
		// automatically convert to correct interval)...

		if (_data_interval_mult > 60)
		{
			message = "Can only do summary for <= 60 minute time series";
			Message.printWarning(2, routine, message);
			throw new TSException(message);
		}

		// If the property list is null, allocate one here so we don't have to
		// constantly check for null...

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

		// Determine the units to output.  For now use what is in the time series...

		string req_units = _data_units;

		// Need to check the data type to determine if it is an average
		// or a total.  For now, make some guesses based on the units...

		if (req_units.Equals("AF", StringComparison.OrdinalIgnoreCase) || req_units.Equals("ACFT", StringComparison.OrdinalIgnoreCase) || req_units.Equals("FT", StringComparison.OrdinalIgnoreCase) || req_units.Equals("FEET", StringComparison.OrdinalIgnoreCase) || req_units.Equals("FOOT", StringComparison.OrdinalIgnoreCase) || req_units.Equals("IN", StringComparison.OrdinalIgnoreCase) || req_units.Equals("INCH", StringComparison.OrdinalIgnoreCase))
		{
			// Assume totals...
			year_column = "Total";
		}
		else
		{
			// Assume averages...
			year_column = "Average";
		}

		// Get the precision...

		prop_value = props.getValue("OutputPrecision");
		if (string.ReferenceEquals(prop_value, null))
		{
			// Older, being phased out...
			Message.printWarning(2, routine, "Need to switch Precision property to OutputPrecision");
			prop_value = props.getValue("Precision");
		}
		if (string.ReferenceEquals(prop_value, null))
		{
			// Try to get units information for default...
			try
			{
				DataUnits u = DataUnits.lookupUnits(req_units);
				data_format = "%9." + u.getOutputPrecision() + "f";
				u = null;
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
				start_date.setPrecision(DateTime.PRECISION_MINUTE);
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
				end_date.setPrecision(DateTime.PRECISION_MINUTE);
			}
			catch (Exception)
			{
				// Default to the time series...
				end_date = new DateTime(_date2);
			}
		}

		// Now generate the output based on the format...

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
			if (!use_comments_for_header.Equals("true", StringComparison.OrdinalIgnoreCase) || (_comments.Count == 0))
			{
				// Format the header from data (not comments)...
				strings.Add("");
				IList<string> strings2 = formatHeader();
				StringUtil.addListToStringList(strings, strings2);
			}
		}

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
						strings.Add((string)_comments[i]);
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
		// Currently no difference in how the output is formatted but in the
		// future might do it differently for different intervals...
		formatOutputNMinute(strings, props, calendar, data_format, start_date, end_date, req_units, year_column);

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
		string routine = "MinuteTS.formatOutput";
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

		// Also return the list...

		return formatted_output;
	}

	/// <summary>
	/// Format the time series for output. </summary>
	/// <returns> List of strings that are written to the file. </returns>
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

		// Also return the list (consistent with C++ single return type).

		return formatted_output;
	}

	/// <summary>
	/// Format the body of the report for an N-minute data, assuming <= 60 minutes.
	/// The output is a simple format with YYYY-MM-DD HH on the left and then the
	/// minutes filling out the row.  If 1 minute data, two rows of values are used
	/// (might need to change later but 1 minute is unlikely).  Otherwise,
	/// a single row is used.  The right column is the total for the hour.  The data
	/// values are always a maximum of 9 characters. </summary>
	/// <param name="strings"> Vector of strings to be used as output. </param>
	/// <param name="props"> Properties to control output. </param>
	/// <param name="calendar"> Calendar to use for output. </param>
	/// <param name="data_format"> Format for data values (C printf style). </param>
	/// <param name="start_date"> Start date for output. </param>
	/// <param name="end_date"> End date for output. </param>
	/// <param name="req_units"> Requested units for output. </param>
	/// <param name="total_column"> indicates whether total column is total or average. </param>
	private void formatOutputNMinute(IList<string> strings, PropList props, YearType calendar, string data_format, DateTime start_date, DateTime end_date, string req_units, string total_column)
	{
		StringBuilder b = new StringBuilder();
		// Loop through the data starting at the appropriate first hour for the period...
		DateTime date = new DateTime(start_date);
		date.setMinute(0); // Always want full hours
		DateTime end = new DateTime(end_date);
		end.setMinute(60 - _data_interval_mult);
		int col = 0; // col=0 is date, col=1 is first data column
		int row = 1; // Row within a day
		double total = 0.0, value = 0.0;
		int count = 0;
		bool do_total = false;
		if (total_column.Equals("Total", StringComparison.OrdinalIgnoreCase))
		{
			do_total = true;
		}
		bool first_header = true;
		int numcol = 60 / _data_interval_mult;
		if (_data_interval_mult == 1)
		{
			numcol = 30;
		}
		for (; date.lessThanOrEqualTo(end); date.addInterval(_data_interval_base,_data_interval_mult))
		{
			// Print a header if the first time or the day is 1...
			if (first_header || ((date.getHour() == 0) && (date.getMinute() == 0)))
			{
				first_header = false;
				strings.Add("");
				b.Length = 0;
				b.Append("  Date/Hour  ");
				for (int i = 0; i < numcol; i++)
				{
					if (_data_interval_mult == 1)
					{
						b.Append("   " + StringUtil.formatString(i,"%2d") + "/" + StringUtil.formatString((i + 12),"%2d") + "  ");
					}
					else
					{
						b.Append("    " + StringUtil.formatString(i * _data_interval_mult,"%2d") + "    ");
					}
				}
				b.Append(" " + total_column);
				strings.Add(b.ToString());
				// Now add the underlines for the headings...
				b.Length = 0;
				b.Append("-------------");
				for (int i = 0; i < (numcol + 1); i++)
				{
					b.Append(" ---------");
				}
				strings.Add(b.ToString());
			}
			// Now do the data...
			if (col == 0)
			{
				b.Length = 0;
				if (row == 1)
				{
					// Add the date at the start of the line...
					b.Append(date.ToString(DateTime.FORMAT_YYYY_MM_DD_HH));
				}
				else
				{
					b.Append("          ");
				}
				++col;
			}
			// Add a data value...
			value = getDataValue(date);
			if (isDataMissing(value))
			{
				b.Append("          ");
			}
			else
			{
				b.Append(" " + StringUtil.formatString(value, data_format));
				total += value;
				++count;
			}
			++col;
			// Now check for the end of the line.  The last data value for
			// the day will be in numcol (e.g. 30 for 1minute).  The above
			// will then have incremented the value to 31.
			if (col > numcol)
			{
				if ((_data_interval_mult == 1) && (row == 1))
				{
					// Need to start a new row...
					row = 2;
					strings.Add(b.ToString());
				}
				else
				{
					// Need to output the row and the total...
					if (do_total || (count == 0))
					{
						b.Append(" " + StringUtil.formatString(total,data_format));
					}
					else
					{
						b.Append(" " + StringUtil.formatString(total / count, data_format));
					}
					strings.Add(b.ToString());
					row = 1;
					total = 0.0;
					count = 0;
				}
				col = 0;
			}
		}
	}

	/// <summary>
	/// Return a data point for the date. </summary>
	/// <param name="date"> date/time to get data. </param>
	/// <param name="tsdata"> if null, a new instance of TSData will be returned.  If non-null, the provided
	/// instance will be used (this is often desirable during iteration to decrease memory use and
	/// increase performance). </param>
	/// <returns> a TSData for the specified date/time. </returns>
	public override TSData getDataPoint(DateTime date, TSData tsdata)
	{
		if (tsdata == null)
		{
			// Allocate it...
			tsdata = new TSData();
		}
		if ((date.lessThan(_date1)) || (date.greaterThan(_date2)))
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(50, "MinuteTS.getDataValue", date + " not within POR (" + _date1 + " - " + _date2 + ")");
			}
			tsdata.setValues(date, _missing, _data_units, "", 0);
			return tsdata;
		}
		// This computes the _month_pos, _day_pos and _interval_pos...
		getDataPosition(date);
		if (_has_data_flags)
		{
			if (_internDataFlagStrings)
			{
				tsdata.setValues(date, _data[_month_pos][_day_pos][_interval_pos], _data_units, _dataFlags[_month_pos][_day_pos][_interval_pos].intern(), 0);
			}
			else
			{
				tsdata.setValues(date, _data[_month_pos][_day_pos][_interval_pos], _data_units, _dataFlags[_month_pos][_day_pos][_interval_pos], 0);
			}
		}
		else
		{
			tsdata.setValues(date, _data[_month_pos][_day_pos][_interval_pos], _data_units, "", 0);
		}
		return tsdata;
	}

	/// <summary>
	/// Compute the data position and set in class data.  This method is used primarily by get/set routines.
	/// Minute data are stored as: [absolute month][days in month][interval in day].
	/// The position array is re-used and values should be copied if there is a need to use between calls. </summary>
	/// <param name="date"> Date of interest. </param>
	private int [] getDataPosition(DateTime date)
	{ // Do not define routine here to improve performance.
		string tz, tz1;

		// This is where we would calculate the shift between the requested
		// time zone and the time zone that we have stored.  For now, just check
		// the time zones against each other and print a warning if not
		// compatible.  When there is time, calculate the shift.

		tz = date.getTimeZoneAbbreviation();
		tz1 = _date1.getTimeZoneAbbreviation();
		if ((tz.Length == 0) || tz.Equals(tz1))
		{
			// We are OK doing a straight retrieval
			//tzshift = 0;
		}
		else
		{
			if (Message.isDebugOn)
			{
				Message.printWarning(10, "MinuteTS.getDataPosition", "Do not know how to shift time zones yet (\"" + tz1 + "\" to \"" + tz + "\"");
				//tzshift = 0;
			}
		}

		// Calculate the row position of the data...

		if (Message.isDebugOn)
		{
			Message.printDebug(50, "MinuteTS.getDataPosition", "Using " + date + "(" + date.getAbsoluteMonth() + ") and start date: " + _date1 + "(" + _date1.getAbsoluteMonth() + ") for row-col calculation.");
		}

		_month_pos = date.getAbsoluteMonth() - _date1.getAbsoluteMonth();

		// Calculate the day position of the data...

		_day_pos = date.getDay() - 1;

		// Calculate the interval position of the data.  We know that minute
		// data are stored in a 3D array with the last dimension being the
		// minute data by interval.  Note that the recording at 00:00 of the
		// current day is the first reading of the day!

		_interval_pos = (date.getHour() * 60 + date.getMinute()) / _data_interval_mult;

		if (Message.isDebugOn)
		{
			Message.printDebug(50, "MinuteTS.getDataPosition", "Month=[" + _month_pos + "] Day=[" + _day_pos + "] interval=[" + _interval_pos + "]");
		}

		_pos[0] = _month_pos;
		_pos[1] = _day_pos;
		_pos[2] = _interval_pos;
		return _pos;
	}

	/// <summary>
	/// Return the data value for a date.
	/// Minute data are stored in a three-dimensional array:
	/// [absolute month][days in month][interval]. </summary>
	/// <returns> The data value corresponding to the date. </returns>
	/// <param name="date"> Date of interest. </param>
	public override double getDataValue(DateTime date)
	{ // Do not define routine here to increase performance.

		if ((date == null) || (_data == null))
		{
			return _missing;
		}

		// Check the date coming in...

		if ((date.lessThan(_date1)) || (date.greaterThan(_date2)))
		{
			if (Message.isDebugOn)
			{
				// Wrap in debug to increase performance...
				Message.printWarning(3, "MinuteTS.getDataValue", date + " not within POR (" + _date1 + " - " + _date2 + ")");
			}
			return _missing;
		}

		// Set the data position in the class data.  There should be no problem
		// since we already checked the dates above...

		getDataPosition(date);

		if (Message.isDebugOn)
		{
			Message.printDebug(50, "MinuteTS.getDataValue", _data[_month_pos][_day_pos][_interval_pos] + " for " + date + " from _data[" + _month_pos + "][" + _day_pos + "][" + _interval_pos + "]");
		}

		return _data[_month_pos][_day_pos][_interval_pos];
	}

	/// <summary>
	/// Returns the data in the specified DataFlavor, or null if no matching flavor
	/// exists.  From the Transferable interface.  Supported data flavors are:<br>
	/// <ul>
	/// <li>MinuteTS - MinuteTS.class / RTi.TS.MinuteTS</li>
	/// <li>TS - TS.class / RTi.TS.TS</li>
	/// <li>TSIdent - TSIdent.class / RTi.TS.TSIdent</li></ul> </summary>
	/// <param name="flavor"> the flavor in which to return the data. </param>
	/// <returns> the data in the specified DataFlavor, or null if no matching flavor exists. </returns>
	public override object getTransferData(DataFlavor flavor)
	{
		if (flavor.Equals(minuteTSFlavor))
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
	/// <li>MinuteTS - MinuteTS.class / RTi.TS.MinuteTS</li>
	/// <li>TS - TS.class / RTi.TS.TS</li>
	/// <li>TSIdent - TSIdent.class / RTi.TS.TSIdent</li></ul> </summary>
	/// <returns> the flavors in which data can be transferred. </returns>
	public override DataFlavor[] getTransferDataFlavors()
	{
		DataFlavor[] flavors = new DataFlavor[3];
		flavors[0] = minuteTSFlavor;
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
	/// Initialize the data.
	/// </summary>
	private void init()
	{
		_data = null;
		_data_interval_base = TimeInterval.MINUTE;
		_data_interval_mult = 1;
		_data_interval_base_original = TimeInterval.MINUTE;
		_data_interval_mult_original = 1;
		_pos = new int[3];
		_pos[0] = 0;
		_pos[1] = 0;
		_pos[2] = 0;
		_month_pos = 0;
		_day_pos = 0;
		_interval_pos = 0;
	}

	/// <summary>
	/// Determines whether the specified flavor is supported as a transfer flavor.
	/// From the Transferable interface.  Supported data flavors are:<br>
	/// <ul>
	/// <li>MinuteTS - MinuteTS.class / RTi.TS.MinuteTS</li>
	/// <li>TS - TS.class / RTi.TS.TS</li>
	/// <li>TSIdent - TSIdent.class / RTi.TS.TSIdent</li></ul> </summary>
	/// <param name="flavor"> the flavor to check. </param>
	/// <returns> true if data can be transferred in the specified flavor, false if not. </returns>
	public override bool isDataFlavorSupported(DataFlavor flavor)
	{
		if (flavor.Equals(minuteTSFlavor))
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
	/// Refresh the derived data (e.g., data limits) if the time series has changed.
	/// This is generally only called by methods within the package.
	/// </summary>
	public override void refresh()
	{
		TSLimits limits = null;

		// If the data is not dirty, then we do not have to refresh the other information...

		if (!_dirty)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(30, "MinuteTS.refresh", "Time series is not dirty.  Not recomputing limits");
			}
			return;
		}

		// Else we need to refresh...

		if (Message.isDebugOn)
		{
			Message.printDebug(30, "MinuteTS.refresh", "Time Series is dirty. Recomputing limits");
		}

		limits = TSUtil.getDataLimits(this, _date1, _date2, false);
		if (limits.areLimitsFound())
		{
			// Now reset the limits for the time series...
			setDataLimits(limits);
		}

		_dirty = false;
		limits = null;
	}

	/// <summary>
	/// Set the data value at a date. </summary>
	/// <param name="date"> Date of interest. </param>
	/// <param name="value"> Data value corresponding to date. </param>
	public override void setDataValue(DateTime date, double value)
	{ // Do not define routine here to increase performance.

		if (date == null)
		{
			return;
		}

		if ((date.lessThan(_date1)) || (date.greaterThan(_date2)))
		{
			if (Message.isDebugOn)
			{
				// Wrap in debug to perform better...
				Message.printWarning(10, "MinuteTS.setDataValue", "Date " + date + " is outside bounds " + _date1 + " - " + _date2);
			}
			return;
		}

		// Get the data position...

		getDataPosition(date);

		if (Message.isDebugOn)
		{
			Message.printDebug(50, "MinuteTS.setDataValue", "Setting " + value + " for " + date + " at [" + _month_pos + "][" + _day_pos + "][" + _interval_pos + "]");
		}

		// Set the dirty flag so that we know to recompute the limits if desired...

		_dirty = true;
		_data[_month_pos][_day_pos][_interval_pos] = value;
	}

	/// <summary>
	/// Set the data value and associated information for the date. </summary>
	/// <param name="date"> Date of interest. </param>
	/// <param name="value"> Data value corresponding to date. </param>
	/// <param name="dataFlag"> data_flag Data flag for value. </param>
	/// <param name="duration"> Duration for value (ignored - assumed to be 1-day or
	/// instantaneous depending on data type). </param>
	public override void setDataValue(DateTime date, double value, string dataFlag, int duration)
	{
		if ((date.lessThan(_date1)) || (date.greaterThan(_date2)))
		{
			if (Message.isDebugOn)
			{
				Message.printWarning(10, "MinuteTS.setDataValue", "Date " + date + " is outside bounds " + _date1 + " - " + _date2);
			}
			return;
		}

		getDataPosition(date);

		if (Message.isDebugOn)
		{
			Message.printDebug(30, "MinuteTS.setDataValue", "Setting " + value + " for " + date + " at [" + _month_pos + "][" + _day_pos + "][" + _interval_pos + "]");
		}

		// Set the dirty flag so that we know to recompute the limits if desired...

		_dirty = true;

		_data[_month_pos][_day_pos][_interval_pos] = value;
		if ((!string.ReferenceEquals(dataFlag, null)) && (dataFlag.Length > 0))
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
						Message.printDebug(30, "MinuteTS.setDataValue", "Error allocating data flag space (" + e + ") - will not use flags.");
					}
					// Make sure to turn flags off
					_has_data_flags = false;
				}
			}
		}
		if (_has_data_flags && (!string.ReferenceEquals(dataFlag, null)))
		{
			if (_internDataFlagStrings)
			{
				_dataFlags[_month_pos][_day_pos][_interval_pos] = dataFlag.intern();
			}
			else
			{
				_dataFlags[_month_pos][_day_pos][_interval_pos] = dataFlag;
			}
		}
	}

	}

}