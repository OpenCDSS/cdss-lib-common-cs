using System;
using System.Text;

// TSData - class for storing data value and date.

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

//------------------------------------------------------------------------------
// TSData - class for storing data value and date.
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 22 Sep 1997	Matthew J. Rutherford,	Created initial version of class.
//		Riverside Technology,
//		inc.
// 05 Jan 1998	Steven A. Malers, RTi	Update to be more generic for use with
//					all time series classes.  This will
//					allow, for example, a way to graph time
//					series and then inquire about the data
//					flag.  Change _quality_flag to
//					_data_flag.  Make it so the date that
//					is set has its own instance here and
//					return a new instance when a date is
//					requested.  This protects the integrity
//					of the data.
// 13 Apr 1999	SAM, RTi		Add finalize.
// 21 Feb 2001	SAM, RTi		Change so clone() is a deep clone and
//					add a copy constructor.
// 30 Aug 2001	SAM, RTi		Make sure clone() is working as
//					expected.  Clean up Javadoc. Set unused
//					variables to null.  Fix the copy
//					constructor.
// 2001-11-06	SAM, RTi		Change so set methods have void return
//					type.
// 2002-01-31	SAM, RTi		Add support for duration, similar to
//					C++.  The duration is normally only used
//					with irregular time series.
// 2003-06-02	SAM, RTi		Upgrade to use generic classes.
//					* Change TSDate to DateTime.
//					* Change TSUnits to DataUnits.
// 2004-03-08	J. Thomas Sapienza, RTi	Class is now serializable.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//------------------------------------------------------------------------------
// EndHeader

namespace RTi.TS
{

	using StringUtil = RTi.Util.String.StringUtil;
	using DateTime = RTi.Util.Time.DateTime;
	using TimeUtil = RTi.Util.Time.TimeUtil;

	/// <summary>
	/// This class provides a simple class for storing a time series data point
	/// consisting of a date, a data value, units and a data flag.  It is used by
	/// IrregularTS to store data returned by getDataPoint() methods. </summary>
	/// <seealso cref= IrregularTS </seealso>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class TSData implements Cloneable, java.io.Serializable
	[Serializable]
	public class TSData : ICloneable
	{

	/// <summary>
	/// Data value.
	/// </summary>
	private double _dataValue;
	/// <summary>
	/// Data flag (often used for quality, etc.).
	/// </summary>
	private string _data_flag;
	/// <summary>
	/// Date/time associated with data value.
	/// </summary>
	private DateTime _date;
	/// <summary>
	/// Duration of the value (seconds)
	/// </summary>
	private int _duration;
	/// <summary>
	/// Pointer to next TSData in list (an internally maintained linked list).
	/// </summary>
	[NonSerialized]
	private TSData _next;
	/// <summary>
	/// Pointer to previous TSData in list (an internally maintained linked list).
	/// </summary>
	[NonSerialized]
	private TSData _previous;
	/// <summary>
	/// Units of data.
	/// </summary>
	private string _units;

	/// <summary>
	/// Default constructor.  The date is set to null and the data value is set to zero.
	/// </summary>
	public TSData() : base()
	{
		initialize();
	}

	/// <summary>
	/// Construct and set the data values. </summary>
	/// <param name="date"> date for data value. </param>
	/// <param name="value"> data value. </param>
	public TSData(DateTime date, double value) : base()
	{
		initialize();
		setDate(date);
		setDataValue(value);
	}

	/// <summary>
	/// Construct and set the data values. </summary>
	/// <param name="date"> date for data value. </param>
	/// <param name="value"> data value. </param>
	/// <param name="units"> data units. </param>
	/// <seealso cref= DataUnits </seealso>
	public TSData(DateTime date, double value, string units) : base()
	{
		initialize();
		setDate(date);
		setDataValue(value);
		setUnits(units);
	}

	/// <summary>
	/// Construct and set the data values. </summary>
	/// <param name="date"> date for data value. </param>
	/// <param name="value"> data value. </param>
	/// <param name="units"> data units. </param>
	/// <param name="flag"> Data flag. </param>
	/// <seealso cref= DataUnits </seealso>
	public TSData(DateTime date, double value, string units, string flag) : base()
	{
		initialize();
		setDate(date);
		setDataValue(value);
		setUnits(units);
		setDataFlag(flag);
	}

	/// <summary>
	/// Create a copy of the object.
	/// A deep copy is made except for the next and
	/// previous pointers, which are copied as is.  If a sequence of new data are
	/// being created, the next/previous pointers will need to be reset accordingly. </summary>
	/// <param name="tsdata"> the instance that is being copied. </param>
	public TSData(TSData tsdata)
	{
		_data_flag = tsdata._data_flag;
		_duration = tsdata._duration;
		_units = tsdata._units;
		_dataValue = tsdata._dataValue;
		_date = new DateTime(tsdata._date);
		_next = tsdata._next;
		_previous = tsdata._previous;
	}

	/// <summary>
	/// Append a data flag string to an existing flag.  If the first character of the flag is "+",
	/// then the flag will be appended with flag (without the +).  If the first two characters are "+,",
	/// then the flag will be appended and a comma will be included only if a previous flag was set. </summary>
	/// <param name="flagOrig"> original data flag </param>
	/// <param name="flag"> data flag to append </param>
	/// <returns> the new merged data flag string </returns>
	public static string appendDataFlag(string flagOrig, string flag)
	{ //Message.printStatus(2, "", "Before append flagOrig= \"" + flagOrig + "\" flag=\"" + flag + "\"" );
		if (string.ReferenceEquals(flagOrig, null))
		{
			flagOrig = "";
		}
		if ((!string.ReferenceEquals(flag, null)) && (flag.Length > 0))
		{
			if (flag.StartsWith("+", StringComparison.Ordinal) && (flag.Length > 1))
			{
				// Have +X... so append to the flag...
				if ((flag[1] == ',') && (flagOrig.Length == 0))
				{
					// Original flag is empty so append without leading comma
					if (flag.Length > 2)
					{
						flagOrig += flag.Substring(2);
					}
				}
				else
				{
					// Append the string after the +, including the comma if provided
					flagOrig += flag.Substring(1);
				}
			}
			else
			{
				// Just set the flag, overriding the previous flag...
				flagOrig = flag;
			}
		}
		//Message.printStatus(2, "", "Flag after append = \"" + flagOrig + "\"" );
		return flagOrig;
	}

	/// <summary>
	/// Clone the object.   The Object base class clone() method is called and then the local data are cloned.
	/// A deep copy is made except for the next and
	/// previous pointers, which are copied as is.  If a sequence of new data are
	/// being created, the next/previous pointers will need to be reset accordingly.
	/// </summary>
	public virtual object clone()
	{
		try
		{
			TSData tsdata = (TSData)base.clone();
			tsdata._date = (DateTime)_date.clone();
			tsdata._next = _next;
			tsdata._previous = _previous;
			tsdata._duration = _duration;
			return tsdata;
		}
		catch (CloneNotSupportedException)
		{
			// Should not happen because everything is cloneable.
			throw new InternalError();
		}
	}

	/// <summary>
	/// Finalize before garbage collection. </summary>
	/// <exception cref="Throwable"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~TSData()
	{
		_data_flag = null;
		_date = null;
		_next = null;
		_previous = null;
		_units = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the data value. </summary>
	/// <returns> The data value. </returns>
	public virtual double getDataValue()
	{
		return _dataValue;
	}

	/// <summary>
	/// Return the data value. </summary>
	/// <returns> The data value. </returns>
	[Obsolete]
	public virtual double getData()
	{
		return getDataValue();
	}

	/// <summary>
	/// Return the data flag. </summary>
	/// <returns> The data flag. </returns>
	public virtual string getDataFlag()
	{
		return _data_flag;
	}

	/// <summary>
	/// Return the data for the data. </summary>
	/// <returns> The date associated with the value.  A copy is returned. </returns>
	public virtual DateTime getDate()
	{
		return new DateTime(_date);
	}

	/// <summary>
	/// Return the duration (seconds) for the data. </summary>
	/// <returns> The duration (seconds) associated with the value. </returns>
	public virtual int getDuration()
	{
		return _duration;
	}

	/// <summary>
	/// Return the reference to the next data item. </summary>
	/// <returns> Return the reference to the next data item (used when an
	/// internally-maintained linked list is used). </returns>
	public virtual TSData getNext()
	{
		return _next;
	}

	/// <summary>
	/// Return the reference to the previous data item. </summary>
	/// <returns> Return the reference to the previous data item (used when an
	/// internally-maintained linked list is used). </returns>
	public virtual TSData getPrevious()
	{
		return _previous;
	}

	/// <summary>
	/// Return the data units. </summary>
	/// <returns> The units for the data.
	/// REVISIT -- Incorrect tag usage: see DataUnits </returns>
	public virtual string getUnits()
	{
		return _units;
	}

	/// <summary>
	/// Initialize the data.
	/// </summary>
	private void initialize()
	{
		_data_flag = "";
		_date = null;
		_duration = 0;
		_units = "";
		_dataValue = 0.0;
		_next = null;
		_previous = null;
	}

	/// <summary>
	/// Set the data value. </summary>
	/// <param name="d"> Data value. </param>
	public virtual void setDataValue(double d)
	{
		_dataValue = d;
	}

	/// <summary>
	/// Set the data flag.  If the first character of the flag is "+", then the flag will be appended
	/// with flag (without the +).  If the first two characters are "+,", then the flag will be appended and a
	/// comma will be included only if a previous flag was set. </summary>
	/// <param name="flag"> Data flag. </param>
	public virtual void setDataFlag(string flag)
	{
		if ((!string.ReferenceEquals(flag, null)) && (flag.Length > 0) && (flag[0] == '+'))
		{
			// Appending the flag
			_data_flag = appendDataFlag(_data_flag,flag);
		}
		else
		{
			// Simple set.  Do this because calling only the above code can result in
			// previous flag values getting carried forward during iteration.
			_data_flag = flag;
		}
	}

	/// <summary>
	/// Set the date.  A copy of the date is made. </summary>
	/// <param name="d"> Date corresponding to data. </param>
	public virtual void setDate(DateTime d)
	{
		if (d != null)
		{
			_date = new DateTime(d);
		}
	}

	/// <summary>
	/// Set the duration (seconds). </summary>
	/// <param name="duration"> Duration corresponding to data. </param>
	public virtual void setDuration(int duration)
	{
		_duration = duration;
	}

	/// <summary>
	/// Set the next reference (used when maintaining and internal linked-list). </summary>
	/// <param name="d"> Reference to TSData. </param>
	public virtual void setNext(TSData d)
	{
		_next = d;
	}

	/// <summary>
	/// Set the previous reference (used when maintaining and internal linked-list). </summary>
	/// <param name="d"> Reference to TSData. </param>
	public virtual void setPrevious(TSData d)
	{
		_previous = d;
	}

	/// <summary>
	/// Set the units for the data value. </summary>
	/// <param name="units"> Data units. </param>
	/// <seealso cref= DataUnits </seealso>
	public virtual void setUnits(string units)
	{
		if (!string.ReferenceEquals(units, null))
		{
			_units = units;
		}
	}

	/// <summary>
	/// Set the data values. </summary>
	/// <param name="date"> Date for data value. </param>
	/// <param name="d"> Data value. </param>
	/// <param name="units"> Data units. </param>
	/// <param name="flag"> Data flag. </param>
	/// <seealso cref= DataUnits </seealso>
	public virtual void setValues(DateTime date, double d, string units, string flag)
	{
		setDate(date);
		setDataValue(d);
		setUnits(units);
		setDataFlag(flag);
	}

	/// <summary>
	/// Set the data values. </summary>
	/// <param name="date"> Date for data value. </param>
	/// <param name="d"> Data value. </param>
	/// <param name="units"> Data units. </param>
	/// <param name="flag"> Data flag. </param>
	/// <param name="duration"> Duration (seconds). </param>
	/// <seealso cref= DataUnits </seealso>
	public virtual void setValues(DateTime date, double d, string units, string flag, int duration)
	{
		setDate(date);
		_dataValue = d;
		setUnits(units);
		setDataFlag(flag);
		_duration = duration;
	}

	/// <summary>
	/// Return a string representation of the instance. </summary>
	/// <returns> A string representation of a TSData. </returns>
	public override string ToString()
	{
		return "TSData: " +
		" Date: \"" + _date +
		"\" Value: " + _dataValue +
		" Units: \"" + _units +
		"\" Flag: \"" + _data_flag + "\" Duration: " + _duration;
	}

	/// <summary>
	/// Return a string representation of the instance using the format.  The format
	/// can contain any of the format specifiers used in TimeUtil.formatDateTime() or
	/// any of the following specifiers:  %v (data value, formatted based on units),
	/// %U (data units), %q (data flag).  Any format information not recognized as a
	/// format specifier is treated as a literal string. </summary>
	/// <returns> A string representation of a TSData. </returns>
	/// <param name="full_format"> Format string containing TimeUtil.formatDateTime() format specifiers, %v, %U, or %q. </param>
	/// <param name="value_format"> Format to use for data value (e.g., %.4f).  This should
	/// in most cases be determined before calling this method repeatedly, in order to improve performance. </param>
	/// <param name="date"> Date for the data value (can be null if no date format specifiers are given). </param>
	/// <param name="value1"> Data value to format (can be ignored if %v is not specified). </param>
	/// <param name="value2"> Second value to format (can be ignored if %v is not specified) -
	/// use when plotting time series on each axis. </param>
	/// <param name="flag"> Data quality flag to format (can be ignored if %q is not specified). </param>
	/// <param name="units"> Data units (can be ignored if %v and %U are not specified. </param>
	public static string ToString(string full_format, string value_format, DateTime date, double value1, double value2, string flag, string units)
	{ // Format the date first...
		string format = TimeUtil.formatDateTime(date, full_format);
		// Now format the %v, %U, and %q (copy this code from
		// TimeUtil.formatDateTime() and modify...

		if (string.ReferenceEquals(format, null))
		{
			return "";
		}

		string value_format2 = value_format; // Used for %v
		if (string.ReferenceEquals(value_format, null))
		{
			value_format2 = "%f";
		}

		// Convert format to string...
		int len = format.Length;
		StringBuilder formatted_string = new StringBuilder();
		char c = '\0';
		bool value1_found = false;
		for (int i = 0; i < len; i++)
		{
			c = format[i];
			if (c == '%')
			{
				// We have a format character...
				++i;
				if (i >= len)
				{
					break; // this will exit the whole loop
				}
				c = format[i];
				if (c == 'v')
				{
					// Data value.
					if (!value1_found)
					{
						formatted_string.Append(StringUtil.formatString(value1,value_format2));
						value1_found = true;
					}
					else
					{
						formatted_string.Append(StringUtil.formatString(value2,value_format2));
					}
				}
				else if (c == 'U')
				{
					// Data units...
					formatted_string.Append(units);
				}
				else if (c == 'q')
				{
					// Data flag...
					formatted_string.Append(flag);
				}
				else if (c == '%')
				{
					// Literal percent...
					formatted_string.Append('%');
				}
			}
			else
			{
				// Just add the character to the string...
				formatted_string.Append(c);
			}
		}

		return formatted_string.ToString();
	}

	}

}