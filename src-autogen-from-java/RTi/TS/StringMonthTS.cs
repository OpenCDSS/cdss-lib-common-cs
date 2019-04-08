using System;
using System.Collections.Generic;

// StringMonthTS - store a monthly time series as strings

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
// StringMonthTS - store a monthly time series as strings
// ----------------------------------------------------------------------------
// Notes:	(1)	A string time series contains monthly data as strings.
//			It is used, for examplem by the
//			TSUtil.fillUsingPattern routine.  For example, a pattern
//			may consist of "WET", "DRY", and "AVG" strings for each
//			month in the time series.
// ----------------------------------------------------------------------------
// History:
//
// 06 Jul 1998	Steven A. Malers, RTi	Copy StateModMonth TS and update to
//					merge with the TSPattern code previously
//					developed by Catherine.
// 20 Aug 1998	SAM, RTi		Modify signature for getDataPosition.
// 13 Apr 1999	SAM, RTi		Add finalize.
// 2001-11-06	SAM, RTi		Review javadoc.  Verify that variables
//					are set to null when no longer used.
//					Change some methods to have a void
//					return type to agree with base class.
// 2003-01-08	SAM, RTi		Add hasData().
// 2003-06-02	SAM, RTi		Upgrade to use generic classes.
//					* Change TSDate to DateTime.
// 2004-03-04	J. Thomas Sapienza, RTi	* Class now implements Serializable.
//					* Class now implements Transferable.
//					* Class supports being dragged or 
//					  copied to clipboard.
// 2005-04-27	SAM, RTi		* Remove Warning 2 message in
//					  getDataValueAsString() when date is
//					  outside of the data period.
// 2005-05-06	SAM, RTi		Add ability to handle missing data like
//					numeric time series.
//					* Add isDataMissing ( String ).
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------
// EndHeader

namespace RTi.TS
{


	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using DateTime = RTi.Util.Time.DateTime;

	/// <summary>
	/// This class is provided as an extension to MonthTS and holds monthly time series
	/// as strings.  An example is:
	/// <para>
	/// <pre>
	/// Year   Jan  Feb  Mar  Apr  May  Jun  Jul  Aug  Sep  Oct  Nov  Dec
	/// 1990   wet  wet  dry  dry  avg  avg  wet  avg  dry  wet  dry  dry
	/// 1991   dry  dry  wet  wet  wet  wet  avg  dry  avg  dry  avg  avg
	/// </pre>
	/// The strings within the pattern are referred to as data strings.
	/// </para>
	/// <para>
	/// 
	/// The time series can then be used, for example, for filling data.  Because the
	/// data are treated as strings, they can in fact be used for anything (e.g., counts
	/// within a month, indicators, etc.).  Because strings are used to store data,
	/// different set/get routines are implemented compared to the standard time series
	/// classes.  Currently, there are no read/write methods because it is anticipated
	/// that the time series file formats will match those of other time series classes
	/// (e.g., StateModMonthTS) and the read/write routines should be implemented in those classes.
	/// </para>
	/// </summary>
	/// <seealso cref= MonthTS </seealso>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class StringMonthTS extends MonthTS implements Cloneable, java.io.Serializable, java.awt.datatransfer.Transferable
	[Serializable]
	public class StringMonthTS : MonthTS, ICloneable, Transferable
	{
	// Data members...

	// The location part of the TSIdent is used as the time series name.
	// The multiplier default of 1 is currently the only multiplier that is supported.

	/// <summary>
	/// The DataFlavor for transferring this specific class.
	/// </summary>
	public static DataFlavor stringMonthTSFlavor = new DataFlavor(typeof(RTi.TS.StringMonthTS), "RTi.TS.StringMonthTS");

	/// <summary>
	/// Array to hold data.
	/// </summary>
	private string[][] _data = null;
	// TODO SAM 2010-07-30 Can String.intern() be used instead here?
	/// <summary>
	/// Unique strings in data.  Used when processing TSPatternStats.
	/// </summary>
	private IList<string> _unique_data = new List<string>(10);
	/// <summary>
	/// Default for missing data.
	/// </summary>
	private string __missing_string = "";

	/// <summary>
	/// Constructor. </summary>
	/// <param name="tsident_string"> Time series identifier string (this is the pattern
	/// name that will be used - generally only the location needs to be specified). </param>
	/// <param name="date1"> Starting date for time series. </param>
	/// <param name="date2"> Ending date for time series. </param>
	/// <exception cref="Exception"> if there is a problem allocating the data space. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StringMonthTS(String tsident_string, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2) throws Exception
	public StringMonthTS(string tsident_string, DateTime date1, DateTime date2) : base()
	{
		try
		{
			init(tsident_string, date1, date2);
			allocateDataSpace();
		}
		catch (TSException e)
		{
			throw e;
		}
	}

	/// <summary>
	/// Copy constructor - not implemented. </summary>
	/// <param name="ts"> Time series to copy. </param>
	public StringMonthTS(StringMonthTS ts)
	{
		string routine = "StringMonthTS.StringMonthTS(ts)";

		Message.printWarning(1, routine, "Not implemented");
	}

	/// <summary>
	/// Allocate the data space for the time series.  The start and end dates and the
	/// data interval multiplier must have been set.  Initialize the data with
	/// blank string attributes. </summary>
	/// <returns> 1 if failure, 0 if success. </returns>
	public override int allocateDataSpace()
	{
		return allocateDataSpace("");
	}

	/// <summary>
	/// Allocate the data space for the time series.  The start and end dates and the
	/// data interval multiplier must have been set.  Fill with the specified data value. </summary>
	/// <param name="value"> Value to initialize data space. </param>
	/// <returns> 1 if failure, 0 if success. </returns>
	public virtual int allocateDataSpace(string value)
	{
		string routine = "StringMonthTS.allocateDataSpace";
		int i, nvals, nyears = 0;
		DateTime date;

		if ((_date1 == null) || (_date2 == null))
		{
			Message.printWarning(2, routine, "Dates have not been set.  Cannot allocate data space");
			return 1;
		}

		nyears = _date2.getYear() - _date1.getYear() + 1;

		if (nyears == 0)
		{
			Message.printWarning(2, routine, "TS has 0 years POR, maybe Dates haven't been set yet");
			return 1;
		}

		_data = new string [nyears][];

		// Allocate memory...

		for (i = 0, date = new DateTime(_date1,DateTime.DATE_FAST); i < nyears; i++, date.addInterval(_data_interval_base, _data_interval_mult))
		{
			if (_data_interval_mult == 1)
			{
				// Easy to handle 1 month data...
				nvals = 12;
			}
			else
			{
				// Do not know how to handle N-month interval...
				Message.printWarning(2, routine, "Only know how to handle 1 month data, not " + _data_interval_mult + "-month");
				return 1;
			}
			_data[i] = new string[nvals];

			// Now fill the entire year with the missing data value...

			for (int j = 0; j < nvals; j++)
			{
				_data[i][j] = value;
			}
		}

		// Set the data size...

		int datasize = calculateDataSize(_date1, _date2, _data_interval_mult);
		setDataSize(datasize);

		// Set the limits used for set/get routines...  These are in the MonthTS class...

		_min_amon = _date1.getAbsoluteMonth();
		_max_amon = _date2.getAbsoluteMonth();

		if (Message.isDebugOn)
		{
			Message.printDebug(10, routine, "Successfully allocated " + nyears + " years of memory (" + datasize + " month values)");
		}

		return 0;
	}

	/// <summary>
	/// Finalize before garbage collection. </summary>
	/// <exception cref="Throwable"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StringMonthTS()
	{
		_data = null;
		_unique_data = null;
	}

	/// <summary>
	/// Dummy routine to prevent warnings.  This is mainly called when getting data limits. </summary>
	/// <param name="date"> Date to get data. </param>
	/// <returns> 0.0 always. </returns>
	public override double getDataValue(DateTime date)
	{
		return 0.0;
	}

	/// <summary>
	/// Return the data value as an double. </summary>
	/// <param name="date"> Date of interest. </param>
	/// <exception cref="RTi.TS.TSException"> if the data string is null. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double getDataValueAsDouble(RTi.Util.Time.DateTime date) throws TSException
	public virtual double getDataValueAsDouble(DateTime date)
	{
		string value = getDataValueAsString(date);
		if (!string.ReferenceEquals(value, null))
		{
			throw new TSException("Null data value");
		}
		return StringUtil.atod(value);
	}

	/// <summary>
	/// Return the data value as an int. </summary>
	/// <param name="date"> Date of interest. </param>
	/// <exception cref="RTi.TS.TSException"> if the data string is null. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int getDataValueAsInt(RTi.Util.Time.DateTime date) throws TSException
	public virtual int getDataValueAsInt(DateTime date)
	{
		string value = getDataValueAsString(date);
		if (!string.ReferenceEquals(value, null))
		{
			throw new TSException("Null data value");
		}
		return StringUtil.atoi(value);
	}

	/// <summary>
	/// Return the String data value for the date. </summary>
	/// <returns> The data value corresponding to the date.  This is very similar to the
	/// MonthTS method except that a String is returned. </returns>
	/// <param name="date"> Date of interest. </param>
	public virtual string getDataValueAsString(DateTime date)
	{
		string routine = "StringMonthTS.getDataValue";
		int column = 0, dl = 50, row = 0;

		//Check the date coming in 

		if ((date.lessThan(_date1)) || (date.greaterThan(_date2)))
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, date + " not within data period (" + _date1 + " - " + _date2 + ")");
			}
			return "";
		}

		// This is in the MonthTS base class...
		int[] pos = getDataPosition(date);
		if (pos == null)
		{
			if (Message.isDebugOn)
			{
				// Wrap in debug to boost performance...
				Message.printWarning(3, routine, "Unable to get data position for " + date);
			}
			return "";
		}
		row = pos[0];
		column = pos[1];

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, _data[row][column] + " for " + date + " from _data[" + row + "][" + column + "]");
		}

		return _data[row][column];
	}

	/// <summary>
	/// Return the number of unique data values. </summary>
	/// <returns> The number of unique data values. </returns>
	public virtual int getNumUniqueData()
	{
		refresh();
		return _unique_data.Count;
	}

	/// <summary>
	/// Return the unique data values. </summary>
	/// <returns> The unique data values. </returns>
	public virtual IList<string> getUniqueData()
	{
		refresh();
		return _unique_data;
	}

	/// <summary>
	/// Returns the data in the specified DataFlavor, or null if no matching flavor
	/// exists.  From the Transferable interface.  Supported dataflavors are:<br>
	/// <ul>
	/// <li>StringMonthTS - StringMonthTS.class / RTi.TS.StringMonthTS</li>
	/// <li>MonthTS - MonthTS.class / RTi.TS.MonthTS</li>
	/// <li>TS - TS.class / RTi.TS.TS</li>
	/// <li>TSIdent - TSIdent.class / RTi.TS.TSIdent</li></ul> </summary>
	/// <param name="flavor"> the flavor in which to return the data. </param>
	/// <returns> the data in the specified DataFlavor, or null if no matching flavor
	/// exists. </returns>
	public override object getTransferData(DataFlavor flavor)
	{
		if (flavor.Equals(stringMonthTSFlavor))
		{
			return this;
		}
		else if (flavor.Equals(MonthTS.monthTSFlavor))
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
	/// <li>StringMonthTS - StringMonthTS.class / RTi.TS.StringMonthTS</li>
	/// <li>MonthTS - MonthTS.class / RTi.TS.MonthTS</li>
	/// <li>TS - TS.class / RTi.TS.TS</li>
	/// <li>TSIdent - TSIdent.class / RTi.TS.TSIdent</li></ul> </summary>
	/// <returns> the flavors in which data can be transferred. </returns>
	public override DataFlavor[] getTransferDataFlavors()
	{
		DataFlavor[] flavors = new DataFlavor[4];
		flavors[0] = stringMonthTSFlavor;
		flavors[1] = MonthTS.tsFlavor;
		flavors[2] = TS.tsFlavor;
		flavors[3] = TSIdent.tsIdentFlavor;
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
	/// Initialize the private data members.  This method accepts all the possible
	/// parameters and handles null appropriately.  This method does not allocate memory. </summary>
	/// <param name="tsident_string"> Time series identifier string (this is the pattern
	/// name that will be used - generally only the location need be specified). </param>
	/// <param name="date1"> Starting date for time series. </param>
	/// <param name="date2"> Ending date for time series. </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void init(String tsident_string, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2) throws Exception
	private void init(string tsident_string, DateTime date1, DateTime date2)
	{
		if (string.ReferenceEquals(tsident_string, null))
		{
			// Big problem...
			string message = "Null identifier.";
			Message.printWarning(2, "StringMonthTS.init", message);
			throw new TSException(message);
		}
		// After setting, the following will be used by allocate data space...
		setIdentifier(tsident_string);
		setDate1(date1);
		setDate2(date2);
	}

	/// <summary>
	/// Determines whether the specified flavor is supported as a transfer flavor.
	/// From the Transferable interface.  Supported dataflavors are:<br>
	/// <ul>
	/// <li>StringMonthTS - StringMonthTS.class / RTi.TS.StringMonthTS</li>
	/// <li>MonthTS - MonthTS.class / RTi.TS.MonthTS</li>
	/// <li>TS - TS.class / RTi.TS.TS</li>
	/// <li>TSIdent - TSIdent.class / RTi.TS.TSIdent</li></ul> </summary>
	/// <param name="flavor"> the flavor to check. </param>
	/// <returns> true if data can be transferred in the specified flavor, false if not. </returns>
	public override bool isDataFlavorSupported(DataFlavor flavor)
	{
		if (flavor.Equals(stringMonthTSFlavor))
		{
			return true;
		}
		else if (flavor.Equals(MonthTS.monthTSFlavor))
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
	/// Determine if a data value for the time series is missing.  The missing value can
	/// be set using setMissing().
	/// There is no straightforward way to check to see if a value is equal to NaN
	/// (the code: if ( value == Double.NaN ) will always return false if one or both
	/// values are NaN).  Consequently there is no way to see know if only one or both
	/// values is NaN, using the standard operators.  Instead, we assume that NaN
	/// should be interpreted as missing and do the check if ( value != value ), which
	/// will return true if the value is NaN.  Consequently, code that uses time series
	/// data should not check for missing and treat NaN differently because the TS
	/// class treats NaN as missing. </summary>
	/// <returns> true if the data value is missing, false if not. </returns>
	/// <param name="value"> Value to check. </param>
	public virtual bool isDataMissing(string value)
	{
		if (value.Equals("NaN", StringComparison.OrdinalIgnoreCase))
		{
			// Check for NaN...
			return true;
		}
		if (value.Equals(__missing_string))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Refresh the derived data.  This will compute the number of unique attribute values.
	/// </summary>
	public override void refresh()
	{
		string routine = "StringMonthTS.refresh";

		// Call on the base class...

		if (!_dirty)
		{
			return;
		}

		base.refresh();
		// Will reset _dirty.

		// Now do our thing...

		// Figure out how many unique values there are (brute force)...

		string string_j, value;
		int j, size;
		DateTime date = null;
		bool found;
		for (date = new DateTime(_date1, DateTime.DATE_FAST); date.lessThanOrEqualTo(_date2); date.addInterval(_data_interval_base, _data_interval_mult))
		{
			// Get the value...
			value = getDataValueAsString(date);
			// Now search through the known list...
			size = _unique_data.Count;
			found = false;
			for (j = 0; j < size; j++)
			{
				string_j = (string)_unique_data[j];
				if (string_j.Equals(value))
				{
					// Same string.  Go to next string to compare...
					found = true;
					break;
				}
			}
			if (!found)
			{
				// A new string.  Add and break...
				_unique_data.Add(value);
				if (Message.isDebugOn)
				{
					Message.printStatus(1, routine, "Adding unique string to list: \"" + value + "\"");
				}
			}
		}
	}

	/// <summary>
	/// Set the attribute for a date.  This has very similar logic to the MonthTS
	/// function but uses a string instead. </summary>
	/// <param name="date"> Date to set value. </param>
	/// <param name="value"> Data value to set. </param>
	public virtual void setDataValue(DateTime date, string value)
	{
		int column, dl = 50, row;

		if (string.ReferenceEquals(value, null))
		{
			// Ignore the set.
			return;
		}

		int[] pos = getDataPosition(date);
		if (pos == null)
		{
			if (Message.isDebugOn)
			{
				// Wrap in debug to boost performance...
				Message.printWarning(2, "StringMonthTS.setDataValue", "Unable to get data position for " + date);
			}
			return;
		}
		row = pos[0];
		column = pos[1];

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, "StringMonthTS.setDataValue", "Setting " + value + " " + date + " at " + row + "," + column);
		}

		// Set the dirty flag so that we know to recompute the limits if desired...

		_dirty = true;

		// Save as a copy of the string...

		_data[row][column] = value;
	}

	/// <summary>
	/// Set the missing data value for the time series. </summary>
	/// <param name="missing"> Missing data value for time series. </param>
	public virtual void setMissing(string missing)
	{
		__missing_string = missing;
	}

	}

}