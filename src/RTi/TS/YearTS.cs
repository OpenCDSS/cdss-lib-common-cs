using System;
using System.Collections.Generic;
using System.IO;

// YearTS - base class from which all yearly time series are derived

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
// YearTS - base class from which all yearly time series are derived
// ----------------------------------------------------------------------------
// Notes:	(1)	This base class is provided so that specific yearly
//			time series can derived from this class.
//		(2)	Data for this time series interval is stored as follows:
//
//			----
//              	|  |	first year in period
//			----
//			|  |
//			----
//			|  |
//			----
//			|  |
//			----
//			.
//			.
//			.
//			----
//			|  |
//			----
//			|  |	last year in period
//			----
// ----------------------------------------------------------------------------
// History:
//
// 22 Feb 1998	Steven A. Malers, RTi	Copy MonthTS and modify as appropriate.
// 06 Aug 1998	SAM, RTi		Optimize by removing getDataPosition
//					code.
// 13 Apr 1999	SAM, RTi		Add finalize.
// 21 Feb 2001	SAM, RTi		Add clone() and copy constructor.
//					Remove printSample() and read/write
//					methods.
// 30 Aug 2001	SAM, RTi		Fix clone() to be more robust.  Clean up
//					Javadoc.  Set unused variables to null.
// 09 Sep 2001	SAM, RTi		Update formatOutput() to actually do
//					something.  Get the code from HourTS and
//					modify, so some output properties are
//					currently irrelevant.
// 2001-11-06	SAM, RTi		Review javadoc.  Verify that variables
//					are set to null when no longer used.
//					Remove constructor that takes input file
//					name.  Change some methods to have void
//					return type to agree with base class.
// 2003-01-08	SAM, RTi		Add hasData().
// 2003-06-02	SAM, RTi		Upgrade to use generic classes.
//					* Change TSDate to DateTime.
//					* Change TSUnits to DataUnits.
//					* Change TS.INTERVAL* to TimeInterval.
// 2003-12-09	SAM, RTi		* Enable a data flag, using DayTS as
//					  a starting poing.
//					* Add changePeriodOfRecord().
//					* Add _tsdata to optimize performance.
// 2004-01-26	SAM, RTi		* Add OutputStart and OutputEnd
//					  properties to formatOutput().
//					* In formatOutput(), convert the file
//					  name to a full path.
// 2004-02-18	SAM, RTi		* Fix bug in formatOutput() - missing
//					  data needed 5 more blanks to line up.
// 2004-02-22	SAM, RTi		* Fix bug in changePeriodOfRecord() -
//					  was not calculating the row position
//					  correctly.
// 2004-03-04	J. Thomas Sapienza, RTi	* Class now implements Serializable.
//					* Class now implements Transferable.
//					* Class supports being dragged or 
//					  copied to clipboard.
// 2005-05-18	SAM, RTi		* Add allocateDataFlagSpace().
//					* Fix bug in getDataPoint() where out
//					  of bounds year was causing an
//					  exception getting the data flag.
// 2005-06-02	SAM, RTi		* Update getDataPoint() to return
//					  a TSData with missing data if the date
//					  is outside the period of record.
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
	using YearType = RTi.Util.Time.YearType;

	/// <summary>
	/// The YearTS class efficiently stores and manipulates time series for yearly data.
	/// If a class extends YearTS, override methods to allocate memory and access data.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class YearTS extends TS implements Cloneable, java.io.Serializable, java.awt.datatransfer.Transferable
	[Serializable]
	public class YearTS : TS, ICloneable, Transferable
	{

	// Data members...

	// FIXME SAM 2010-06-08 Need to move the following to a separate transfer class
	/// <summary>
	/// The DataFlavor for transferring this specific class.
	/// </summary>
	public static DataFlavor yearTSFlavor = new DataFlavor(typeof(RTi.TS.YearTS), "RTi.TS.YearTS");

	private double[] _data; // This is the data space for yearly data.
	private string[] _dataFlags; // Data flags for each yearly value.
	private int _year1; // Bounds for allocated data.
	private int _year2; // Bounds for allocated data.

	/// <summary>
	/// Default constructor.  Set dates and use allocateDataSpace() to create memory for data.
	/// </summary>
	public YearTS() : base()
	{
		init();
	}

	/// <summary>
	/// Copy constructor.  Everything is copied by calling copyHeader() and then copying the data values. </summary>
	/// <param name="ts"> YearTS to copy. </param>
	public YearTS(YearTS ts)
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
	/// <param name="initialValue"> Initial value (null is allowed and will result in the flags being initialized to spaces). </param>
	/// <param name="retainPreviousValues"> If true, the array size will be increased if necessary, but
	/// previous data values will be retained.  If false, the array will be reallocated and initialized to spaces. </param>
	/// <exception cref="Exception"> if there is an error allocating the memory. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void allocateDataFlagSpace(String initialValue, boolean retainPreviousValues) throws Exception
	public override void allocateDataFlagSpace(string initialValue, bool retainPreviousValues)
	{
		string routine = "YearTS.allocateDataFlagSpace", message;

		if ((_date1 == null) || (_date2 == null))
		{
			message = "Dates have not been set.  Cannot allocate data flag space";
			Message.printWarning(2, routine, message);
			throw new Exception(message);
		}
		if (_data_interval_mult != 1)
		{
			// Do not know how to handle N-year interval...
			message = "Only know how to handle 1 year data, not " + _data_interval_mult + "-year";
			Message.printWarning(3, routine, message);
			throw new Exception(message);
		}

		if (string.ReferenceEquals(initialValue, null))
		{
			initialValue = "";
		}

		int nyears = _date2.getYear() - _date1.getYear() + 1;

		if (nyears == 0)
		{
			message = "TS has 0 years POR, maybe dates haven't been set yet.";
			Message.printWarning(2, routine, message);
			throw new Exception(message);
		}

		string[] dataFlagsPrev = null;
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
		_dataFlags = new string[nyears];

		// Allocate memory...

		bool internDataFlagStrings = getInternDataFlagStrings();
		for (int iYear = 0; iYear < nyears; iYear++)
		{
			if (internDataFlagStrings)
			{
				_dataFlags[iYear] = initialValue.intern();
			}
			else
			{
				_dataFlags[iYear] = initialValue;
			}
			if (retainPreviousValues && (dataFlagsPrev != null))
			{
				// Copy the old values (typically shorter character arrays)...
				if (internDataFlagStrings)
				{
					_dataFlags[iYear] = dataFlagsPrev[iYear].intern();
				}
				else
				{
					_dataFlags[iYear] = dataFlagsPrev[iYear];
				}
			}
		}
	}

	/// <summary>
	/// Allocate the data space and initialize using the default missing data value. </summary>
	/// <returns> Zero if successful, non-zero if not. </returns>
	public override int allocateDataSpace()
	{
		return allocateDataSpace(_missing);
	}

	/// <summary>
	/// Allocate the data space and initialize using the specified data value. </summary>
	/// <returns> Zero if successful, non-zero if not. </returns>
	/// <param name="value"> Value used to initialize data space. </param>
	public virtual int allocateDataSpace(double value)
	{
		if ((_date1 == null) || (_date2 == null))
		{
			Message.printWarning(2, "YearTS.allocateDataSpace", "Dates have not been set.  Cannot allocate data space");
			return 1;
		}

		int nyears = _date2.getYear() - _date1.getYear() + 1;

		if (nyears == 0)
		{
			Message.printWarning(2, "YearTS.allocateDataSpace", "TS has 0 years POR, maybe Dates haven't been set yet");
			return 1;
		}

		_data = new double[nyears];

		if (_has_data_flags)
		{
			_dataFlags = new string[nyears];
		}

		for (int iYear = 0; iYear < nyears; iYear++)
		{
			_data[iYear] = value;
			if (_has_data_flags)
			{
				_dataFlags[iYear] = "";
			}
		}

		// Calculate the data size...

		int datasize = calculateDataSize(_date1,_date2,_data_interval_mult);
		setDataSize(datasize);

		// Calculate the date limits to optimize the set/get routines...

		_year1 = _date1.getYear();
		_year2 = _date2.getYear();

		if (Message.isDebugOn)
		{
			Message.printDebug(10, "YearTS.allocateDataSpace", "Successfully allocated " + nyears + " years of memory " + _year1 + " to " + _year2 + " (" + datasize + " values)");
		}

		return 0;
	}

	/// <summary>
	/// Determine the number of data intervals in a period. </summary>
	/// <returns> The number of data points for a year time series.
	/// given the data interval multiplier for the specified period. </returns>
	/// <param name="start_date"> The first date of the period. </param>
	/// <param name="end_date"> The last date of the period. </param>
	/// <param name="interval_mult"> The time series data interval multiplier. </param>
	public static int calculateDataSize(DateTime start_date, DateTime end_date, int interval_mult)
	{
		string routine = "YearTS.calculateDataSize";

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
		if (interval_mult != 1)
		{
			Message.printWarning(1, routine, "Do not know how to handle N-year (" + interval_mult + ") time series");
			routine = null;
			return 0;
		}
		int datasize = end_date.getYear() - start_date.getYear() + 1;
		routine = null;
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
		string routine = "YearTS.changePeriodOfRecord";
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
			return;
		}

		// To transfer the data (later), get the old position and then set in
		// the new position.  To get the right data position, declare a
		// temporary YearTS with the old dates and save a reference to the old data...

		double[] dataSave = _data;
		string[] dataFlagsSave = _dataFlags;
		YearTS temp_ts = new YearTS();
		temp_ts.setDataInterval(TimeInterval.YEAR, _data_interval_mult);
		temp_ts.setDate1(_date1);
		temp_ts.setDate2(_date2);
		int temp_year1 = _date1.getYear();

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

		int row;
		bool internDataFlagStrings = getInternDataFlagStrings();
		for (DateTime date = new DateTime(transfer_date1,DateTime.DATE_FAST); date.lessThanOrEqualTo(transfer_date2); date.addInterval(_data_interval_base, _data_interval_mult))
		{
			// Get the data position for the old data...
			row = date.getYear() - temp_year1;
			// Also transfer the data flag...
			if (_has_data_flags)
			{
				// Transfer the value and flag...
				if (internDataFlagStrings)
				{
					setDataValue(date, dataSave[row], dataFlagsSave[row].intern(), 1);
				}
				else
				{
					setDataValue(date, dataSave[row], dataFlagsSave[row], 1);
				}
			}
			else
			{
				// Just transfer the value...
				setDataValue(date, dataSave[row]);
			}
		}

		// Add to the genesis...

		addToGenesis("Changed period from: " + temp_ts.getDate1() + " - " + temp_ts.getDate2() + " to " + new_date1 + " - " + new_date2);
		routine = null;
		message = null;
		new_date1 = null;
		new_date2 = null;
		temp_ts = null;
		transfer_date1 = null;
		transfer_date2 = null;
		dataSave = null;
	}

	/// <summary>
	/// Clone the object.
	/// </summary>
	public override object clone()
	{
		YearTS ts = (YearTS)base.clone();
		// Does not appear to work...
		//ts._data = (double [])_data.clone();
		if (_data == null)
		{
			ts._data = null;
		}
		else
		{
			ts._data = new double[_data.Length];
			Array.Copy(_data, 0, ts._data, 0, _data.Length);
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
				ts._dataFlags = new string[_dataFlags.Length];
				for (int iYear = 0; iYear < _dataFlags.Length; iYear++)
				{
					if (internDataFlagStrings)
					{
						ts._dataFlags[iYear] = _dataFlags[iYear].intern();
					}
					else
					{
						ts._dataFlags[iYear] = _dataFlags[iYear];
					}
				}
			}
		}
		ts._year1 = _year1;
		ts._year2 = _year2;
		return ts;
	}

	/// <summary>
	/// Finalize before garbage collection. </summary>
	/// <exception cref="Throwable"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~YearTS()
	{
		_data = null;
		_dataFlags = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Format the time series for output.  This is not meant to be used for time
	/// series conversion but to produce a general summary of the output.  At this time,
	/// year time series are always output in a column format. </summary>
	/// <returns> List of strings that can be displayed, printed, etc. </returns>
	/// <param name="proplist"> Properties of the output, as described in the following table:
	/// 
	/// <table width=100% cellpadding=10 cellspacing=0 border=2>
	/// <tr>
	/// <td><b>Property</b></td>	<td><b>Description</b></td>	<td><b>Default</b></td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>CalendarType</b></td>
	/// <td>The type of calendar, from YearType enumeration.
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
		string message = "", routine = "YearTS.formatOutput", year_column = "";
		IList<string> strings = new List<string>();
		PropList props = null;
		string data_format = "%13.1f", prop_value = null;

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
				data_format = "%13." + u.getOutputPrecision() + "f";
				u = null;
			}
			catch (Exception)
			{
				// Default...
				data_format = "%13.1f";
			}
		}
		else
		{
			// Set to requested precision...
			data_format = "%13." + prop_value + "f";
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
				start_date.setPrecision(DateTime.PRECISION_YEAR);
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
				end_date.setPrecision(DateTime.PRECISION_YEAR);
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
		// Currently only one output format but in the future may add others...
		formatOutputNYear(strings, props, calendar, data_format, start_date, end_date, req_units, year_column);
		return strings;
	}

	/// <summary>
	/// Format the time series for output. </summary>
	/// <returns> List of strings that are written to the file. </returns>
	/// <param name="fp"> PrintWriter to receive output. </param>
	/// <param name="props"> Properties to modify output. </param>
	/// <exception cref="RTi.TS.TSException"> Throws if there is an error writing the output. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.util.List<String> formatOutput(java.io.PrintWriter fp, RTi.Util.IO.PropList props) throws TSException
	public override IList<string> formatOutput(PrintWriter fp, PropList props)
	{
		IList<string> formatted_output = null;
		string routine = "YearTS.formatOutput";
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
					fp.print((string)formatted_output[i] + newline);
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
		finally
		{
			if (stream != null)
			{
				stream.close();
			}
		}

		// Also return the list (consistent with C++ single return type.

		return formatted_output;
	}

	/// <summary>
	/// Format the body of the report for an N-Year data.
	/// The output is a simple format with YYYY on the left and then the values
	/// filling out the row.  The data values are always a maximum of 13 characters. </summary>
	/// <param name="strings"> list of strings to be used as output. </param>
	/// <param name="props"> Properties to control output. </param>
	/// <param name="calendar"> year type to use for output. </param>
	/// <param name="data_format"> Format for data values (C printf style). </param>
	/// <param name="start_date"> Start date for output. </param>
	/// <param name="end_date"> End date for output. </param>
	/// <param name="req_units"> Requested units for output. </param>
	/// <param name="total_column"> indicates whether total column is total or average. </param>
	private void formatOutputNYear(IList<string> strings, PropList props, YearType calendar, string data_format, DateTime start_date, DateTime end_date, string req_units, string total_column)
	{
		DateTime date = new DateTime(start_date);
		DateTime end = new DateTime(end_date);
		strings.Add("");
		if (_has_data_flags)
		{
			strings.Add("Year     Value     Flag");
			strings.Add("---- ------------- ----");
		}
		else
		{
			strings.Add("Year     Value    ");
			strings.Add("---- -------------");
		}
		double value = 0.0;
		TSData tsdata = new TSData();
		for (; date.lessThanOrEqualTo(end); date.addInterval(_data_interval_base,_data_interval_mult))
		{
			value = getDataValue(date);
			if (_has_data_flags)
			{
				if (isDataMissing(value))
				{
					strings.Add(date.ToString(DateTime.FORMAT_YYYY) + "               " + StringUtil.formatString(getDataPoint(date,tsdata).getDataFlag(),"%4.4s"));
				}
				else
				{
					strings.Add(date.ToString(DateTime.FORMAT_YYYY) + " " + StringUtil.formatString(value,data_format) + " " + StringUtil.formatString(getDataPoint(date,tsdata).getDataFlag(),"%4.4s"));
				}
			}
			else
			{
				if (isDataMissing(value))
				{
					strings.Add(date.ToString(DateTime.FORMAT_YYYY) + "               ");
				}
				else
				{
					strings.Add(date.ToString(DateTime.FORMAT_YYYY) + " " + StringUtil.formatString(value,data_format));
				}
			}
		}
	}

	/// <summary>
	/// Return a data point for the date.
	/// <pre>
	///              Yearly data is stored in a one-dimensional array:
	///  		     |
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
	{ // Initialize data to most of what we need...
		if (tsdata == null)
		{
			// Allocate it...
			tsdata = new TSData();
		}
		if ((_data == null) || (date == null))
		{
			tsdata.setDate(null);
			tsdata.setDataValue(_missing);
			tsdata.setDataFlag("");
			return tsdata;
		}
		if ((date.lessThan(_date1)) || (date.greaterThan(_date2)))
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(50, "YearTS.getDataValue", date + " not within POR (" + _date1 + " - " + _date2 + ")");
			}
			tsdata.setValues(date, _missing, _data_units, "", 0);
			return tsdata;
		}
		if (_has_data_flags)
		{
			int index = date.getYear() - _year1;
			if ((index < 0) || (index >= _dataFlags.Length))
			{
				tsdata.setValues(date, getDataValue(date), _data_units, "", 0);
			}
			else
			{
				if (_internDataFlagStrings)
				{
					tsdata.setValues(date, getDataValue(date), _data_units, _dataFlags[date.getYear() - _year1].intern(), 0);
				}
				else
				{
					tsdata.setValues(date, getDataValue(date), _data_units, _dataFlags[date.getYear() - _year1], 0);
				}
			}
		}
		else
		{
			tsdata.setValues(date, getDataValue(date), _data_units, "", 0);
		}
		return tsdata;
	}

	/// <summary>
	/// Return a data value for the date.
	///             	Year data is stored in a one-dimensional array:
	///  		     |
	///  		     |
	///  		    \|/
	///  		   year </summary>
	/// <returns> The data value corresponding to the specified date.  If the date is
	/// not found in the period, a missing data value is returned. </returns>
	/// <param name="date"> Date of interest. </param>
	public override double getDataValue(DateTime date)
	{ // Check the date coming in...

		if ((_data == null) || (date == null))
		{
			return _missing;
		}

		int year = date.getYear();

		if ((year < _year1) || (year > _year2))
		{
			// Wrap in debug to improve performance...
			if (Message.isDebugOn)
			{
				Message.printWarning(2, "YearTS.getDataValue", year + " not within POR (" + _year1 + " - " + _year2 + ")");
			}
			return _missing;
		}

		// THIS CODE MUST MATCH THAT IN setDataValue...

		int row = year - _year1;

		// ... END MATCHING CODE

		if (Message.isDebugOn)
		{
			Message.printDebug(30, "YearTS.getDataValue", _data[row] + " for " + year + " from _data[" + row + "]");
		}

		return _data[row];
	}

	/// <summary>
	/// Returns the data in the specified DataFlavor, or null if no matching flavor
	/// exists.  From the Transferable interface.  Supported data-flavors are:<br>
	/// <ul>
	/// <li>YearTS - YearTS.class / RTi.TS.YearTS</li>
	/// <li>TS - TS.class / RTi.TS.TS</li>
	/// <li>TSIdent - TSIdent.class / RTi.TS.TSIdent</li></ul> </summary>
	/// <param name="flavor"> the flavor in which to return the data. </param>
	/// <returns> the data in the specified DataFlavor, or null if no matching flavor exists. </returns>
	public override object getTransferData(DataFlavor flavor)
	{
		if (flavor.Equals(yearTSFlavor))
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
	/// The order of the data-flavors that are returned are:<br>
	/// <ul>
	/// <li>YearTS - YearTS.class / RTi.TS.YearTS</li>
	/// <li>TS - TS.class / RTi.TS.TS</li>
	/// <li>TSIdent - TSIdent.class / RTi.TS.TSIdent</li></ul> </summary>
	/// <returns> the flavors in which data can be transferred. </returns>
	public override DataFlavor[] getTransferDataFlavors()
	{
		DataFlavor[] flavors = new DataFlavor[3];
		flavors[0] = yearTSFlavor;
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
		_data_interval_base = TimeInterval.YEAR;
		_data_interval_mult = 1;
		_data_interval_base_original = TimeInterval.YEAR;
		_data_interval_mult_original = 1;
		_year1 = 0;
		_year2 = 0;
	}

	/// <summary>
	/// Determines whether the specified flavor is supported as a transfer flavor.
	/// From the Transferable interface.  Supported data-flavors are:<br>
	/// <ul>
	/// <li>YearTS - YearTS.class / RTi.TS.YearTS</li>
	/// <li>TS - TS.class / RTi.TS.TS</li>
	/// <li>TSIdent - TSIdent.class / RTi.TS.TSIdent</li></ul> </summary>
	/// <param name="flavor"> the flavor to check. </param>
	/// <returns> true if data can be transferred in the specified flavor, false if not. </returns>
	public override bool isDataFlavorSupported(DataFlavor flavor)
	{
		if (flavor.Equals(yearTSFlavor))
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
	/// Refresh the dependent time series data, such as limits.  This routine is called
	/// when retrieving secondary data (like limits) to make sure that the values are current.
	/// </summary>
	public override void refresh()
	{ // If the data is not dirty, then we do not have to refresh the other information...

		if (!_dirty)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(10, "TSLimits.refresh", "Time series is not dirty.  Not recomputing limits");
			}
			return;
		}

		// Else we need to refresh...

		if (Message.isDebugOn)
		{
			Message.printDebug(10, "TSLimits.refresh", "Time Series is dirty. Recomputing limits");
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
	/// Set the data value for the given date. </summary>
	/// <param name="date"> Date to set value. </param>
	/// <param name="value"> Value for the date. </param>
	public override void setDataValue(DateTime date, double value)
	{
		if (date == null)
		{
			return;
		}

		int year = date.getYear();

		if ((year < _year1) || (year > _year2))
		{
			// Wrap in debug to improve performance...
			if (Message.isDebugOn)
			{
				Message.printWarning(2, "YearTS.setDataValue", year + " not within POR (" + _year1 + " - " + _year2 + ")");
			}
			return;
		}

		// THIS CODE MUST MATCH THAT IN setDataValue...

		int row = year - _year1;

		// ... END MATCHING CODE

		if (Message.isDebugOn)
		{
			Message.printDebug(30, "YearTS.setDataValue", "Setting " + value + " " + year + " at " + row);
		}

		// Set the dirty flag so that we know to recompute the limits if desired...

		_dirty = true;

		_data[row] = value;
	}

	/// <summary>
	/// Set the data value for the given date. </summary>
	/// <param name="date"> Date to set value. </param>
	/// <param name="value"> Value for the date. </param>
	/// <param name="data_flag"> data_flag Data flag for value. </param>
	/// <param name="duration"> Duration for value (ignored - assumed to be 1-day or
	/// instantaneous depending on data type). </param>
	public override void setDataValue(DateTime date, double value, string data_flag, int duration)
	{
		if (date == null)
		{
			return;
		}

		int year = date.getYear();

		if ((year < _year1) || (year > _year2))
		{
			// Wrap in debug to improve performance...
			if (Message.isDebugOn)
			{
				Message.printWarning(2, "YearTS.setDataValue", year + " not within POR (" + _year1 + " - " + _year2 + ")");
			}
			return;
		}

		// THIS CODE MUST MATCH THAT IN setDataValue...

		int row = year - _year1;

		// ... END MATCHING CODE

		if (Message.isDebugOn)
		{
			Message.printDebug(30, "YearTS.setDataValue", "Setting " + value + " " + year + " at " + row);
		}

		// Set the dirty flag so that we know to recompute the limits if desired...

		_dirty = true;

		_data[row] = value;
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
						Message.printDebug(30, "YearTS.setDataValue", "Error allocating data flag space (" + e + ") - will not use flags.");
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
				_dataFlags[row] = data_flag.intern();
			}
			else
			{
				_dataFlags[row] = data_flag;
			}
		}
	}

	}

}