using System;
using System.Text;

// FrostDatesYearTSLimits - data limits for FrostDatesYearTS

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
// FrostDatesYearTSLimits - data limits for FrostDatesYearTS
// ----------------------------------------------------------------------------
// Notes:	(1)	This class stores the data limits for a
//			FrostDatesYearTS time series.
//			It stores the maximum and minimum values and the dates
//			associated with the values, as well as the average
//			values.
// ----------------------------------------------------------------------------
// History:
//
// 10 Jan 1999	Steven A. Malers, RTi	Initial version.  Extend TSLimits.
// 12 Apr 1999	SAM, RTi		Add finalize.
// 2001-11-06	SAM, RTi		Review javadoc.  Verify that variables
//					are set to null when no longer used.
// 2003-06-02	SAM, RTi		Upgrade to use generic classes.
//					* Change TSDate to DateTime.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------
// EndHeader

namespace RTi.TS
{
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using DateTime = RTi.Util.Time.DateTime;

	/// <summary>
	/// The FrostDatesYearTSLimits class stores information about the data and date
	/// limits of a FrostDatesYearTS time series.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class FrostDatesYearTSLimits extends TSLimits
	[Serializable]
	public class FrostDatesYearTSLimits : TSLimits
	{

	// Time series that is being studied...

	internal FrostDatesYearTS _ts = null;

	// Data are for the total period but since we are not dealing with floating
	// point numbers (like TSLimits), use special data here.

	private DateTime _max_last_28F_spring;
	private DateTime _max_last_32F_spring;
	private DateTime _max_first_32F_fall;
	private DateTime _max_first_28F_fall;

	private DateTime _min_last_28F_spring;
	private DateTime _min_last_32F_spring;
	private DateTime _min_first_32F_fall;
	private DateTime _min_first_28F_fall;

	private DateTime _mean_last_28F_spring;
	private DateTime _mean_last_32F_spring;
	private DateTime _mean_first_32F_fall;
	private DateTime _mean_first_28F_fall;

	private int _count_last_28F_spring;
	private int _count_last_32F_spring;
	private int _count_first_32F_fall;
	private int _count_first_28F_fall;

	private int _missing_count_last_28F_spring;
	private int _missing_count_last_32F_spring;
	private int _missing_count_first_32F_fall;
	private int _missing_count_first_28F_fall;

	/// <summary>
	/// Constructor.  Initialize the dates to null and the limits to zeros. </summary>
	/// <exception cref="TSException"> if there is an creating the limits. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public FrostDatesYearTSLimits() throws TSException
	public FrostDatesYearTSLimits() : base()
	{
		try
		{
			initialize();
		}
		catch (Exception)
		{
			string message = "Error creating FrostDatesYearTSLimits";
			Message.printWarning(2, "FrostDatesYearTSLimits", message);
			throw new TSException(message);
		}
	}

	/// <summary>
	/// Copy constructor. </summary>
	/// <param name="limits"> Instance to copy. </param>
	/// <exception cref="TSException"> if there is an error c creating the limits. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public FrostDatesYearTSLimits(FrostDatesYearTSLimits limits) throws TSException
	public FrostDatesYearTSLimits(FrostDatesYearTSLimits limits) : base()
	{
		try
		{
			initialize();

		_max_last_28F_spring = limits._max_last_28F_spring;
		_max_last_32F_spring = limits._max_last_32F_spring;
		_max_first_32F_fall = limits._max_first_32F_fall;
		_max_first_28F_fall = limits._max_first_28F_fall;

		_min_last_28F_spring = limits._min_last_28F_spring;
		_min_last_32F_spring = limits._min_last_32F_spring;
		_min_first_32F_fall = limits._min_first_32F_fall;
		_min_first_28F_fall = limits._min_first_28F_fall;

		_mean_last_28F_spring = limits._mean_last_28F_spring;
		_mean_last_32F_spring = limits._mean_last_32F_spring;
		_mean_first_32F_fall = limits._mean_first_32F_fall;
		_mean_first_28F_fall = limits._mean_first_28F_fall;

		_count_last_28F_spring = limits._count_last_28F_spring;
		_count_last_32F_spring = limits._count_last_32F_spring;
		_count_first_32F_fall = limits._count_first_32F_fall;
		_count_first_28F_fall = limits._count_first_28F_fall;

		_missing_count_last_28F_spring = limits._missing_count_last_28F_spring;
		_missing_count_last_32F_spring = limits._missing_count_last_32F_spring;
		_missing_count_first_32F_fall = limits._missing_count_first_32F_fall;
		_missing_count_first_28F_fall = limits._missing_count_first_28F_fall;

		}
		catch (Exception)
		{
			string message = "Error creating FrostDatesYearTSLimits";
			Message.printWarning(2, "FrostDatesYearTSLimits(copy)", message);
			throw new TSException(message);
		}
	}

	/// <summary>
	/// Constructor to compute the limits given a FrostDatesYearTS.  This is the main
	/// constructor and is overloaded in a variety of ways.  If a variant of this
	/// constructor that does not take a FrostDatesYearTS is used,
	/// the limits are not computed
	/// in this class and must be set from calling code. </summary>
	/// <param name="ts"> Time series of interest. </param>
	/// <param name="startdate"> Starting date for the check. </param>
	/// <param name="enddate"> Ending date for the check. </param>
	/// <exception cref="TSException"> if there is an error c creating the limits. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public FrostDatesYearTSLimits(FrostDatesYearTS ts, RTi.Util.Time.DateTime startdate, RTi.Util.Time.DateTime enddate) throws TSException
	public FrostDatesYearTSLimits(FrostDatesYearTS ts, DateTime startdate, DateTime enddate) : base()
	{
		try
		{
			initialize();
			_ts = ts;
			getDataLimits(ts, startdate, enddate);
		}
		catch (Exception)
		{
			string message = "Error creating FrostDatesYearTSLimits";
			Message.printWarning(2, "FrostDatesYearTSLimits(...)", message);
			throw new TSException(message);
		}
	}

	/// <summary>
	/// Construct the FrostDatesYearTS limits for the full period. </summary>
	/// <param name="ts"> Time series of interest. </param>
	/// <exception cref="TSException"> if there is an error computing the limits. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public FrostDatesYearTSLimits(FrostDatesYearTS ts) throws TSException
	public FrostDatesYearTSLimits(FrostDatesYearTS ts) : base()
	{
		try
		{ // Compute the monthly limits...
			initialize();
			_ts = ts;
			getDataLimits(ts, (DateTime)null, (DateTime)null);
		}
		catch (Exception)
		{
			string message = "Error creating FrostDatesYearTSLimits";
			Message.printWarning(2, "FrostDatesYearTSLimits(ts)",message);
			throw new TSException(message);
		}
	}

	/// <summary>
	/// Finalize before garbage collection. </summary>
	/// <exception cref="Throwable"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~FrostDatesYearTSLimits()
	{
		_ts = null;
		_max_last_28F_spring = null;
		_max_last_32F_spring = null;
		_max_first_32F_fall = null;
		_max_first_28F_fall = null;

		_min_last_28F_spring = null;
		_min_last_32F_spring = null;
		_min_first_32F_fall = null;
		_min_first_28F_fall = null;

		_mean_last_28F_spring = null;
		_mean_last_32F_spring = null;
		_mean_first_32F_fall = null;
		_mean_first_28F_fall = null;

//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Compute the data limits for the time series given a period. </summary>
	/// <param name="ts"> Time series of interest. </param>
	/// <param name="start0"> Starting date for the check. </param>
	/// <param name="end0"> Ending date for the check. </param>
	/// <exception cref="TSException"> if there is an error computing the detailed limits. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void getDataLimits(FrostDatesYearTS ts, RTi.Util.Time.DateTime start0, RTi.Util.Time.DateTime end0) throws TSException
	private void getDataLimits(FrostDatesYearTS ts, DateTime start0, DateTime end0)
	{
		string message, routine = "FrostDatesYearTSLimits.getDataLimits";
		int @base = 0, mult = 0;
		DateTime t = null;

		try
		{ // Overall try...

		if (ts == null)
		{
			message = "NULL time series";
			Message.printWarning(2, routine, message);
			throw new TSException(message);
		}

		// Get valid date limits because the ones passed in may have been
		// null...

		TSLimits valid_dates = TSUtil.getValidPeriod(ts, start0, end0);
		DateTime start = valid_dates.getDate1();
		DateTime end = valid_dates.getDate2();
		valid_dates = null;
		setDate1(start);
		setDate2(end);

		// Make sure that the time series has current limits...

		@base = ts.getDataIntervalBase();
		mult = ts.getDataIntervalMult();

		// Get the variables that are used often in this function

		// Dates to track max and min without the year.

		DateTime short_max_last_28F_spring = null;
		DateTime short_max_last_32F_spring = null;
		DateTime short_max_first_32F_fall = null;
		DateTime short_max_first_28F_fall = null;

		DateTime short_min_last_28F_spring = null;
		DateTime short_min_last_32F_spring = null;
		DateTime short_min_first_32F_fall = null;
		DateTime short_min_first_28F_fall = null;

		// First loop through and find the data limits and the
		// minimum non-missing date...

		t = new DateTime(start, DateTime.DATE_FAST);
		double sum_last_28F_spring = 0.0;
		double sum_last_32F_spring = 0.0;
		double sum_first_32F_fall = 0.0;
		double sum_first_28F_fall = 0.0;
		DateTime full_date = null, short_date = null; // Working dates.
		int year;
		for (; t.lessThanOrEqualTo(end); t.addInterval(@base, mult))
		{
			year = t.getYear();

			// Process each date...

			// During processing we don't really care about the year so
			// set it to zero.  However, to show the full year for max,
			// min, etc., we need to add and subtract the year as
			// appropriate.  If this is a memory problem, we can always just
			// save the year and add/subtract as necessary.  The comparisons
			// are made using the short dates but the final result is a
			// full date to show the year that the max/min was recorded.

			short_date = ts.getLast28Spring(year);
			if (short_date == null)
			{
				++_missing_count_last_28F_spring;
			}
			else
			{
				full_date = new DateTime(short_date);
				short_date.setYear(0);
				sum_last_28F_spring += short_date.toDouble();
				++_count_last_28F_spring;
				if (short_max_last_28F_spring == null)
				{
					short_max_last_28F_spring = new DateTime(short_date);
					_max_last_28F_spring = new DateTime(full_date);
				}
				else if (short_date.greaterThan(short_max_last_28F_spring))
				{
					short_max_last_28F_spring = new DateTime(short_date);
					_max_last_28F_spring = new DateTime(full_date);
				}
				if (short_min_last_28F_spring == null)
				{
					short_min_last_28F_spring = new DateTime(short_date);
					_min_last_28F_spring = new DateTime(full_date);
				}
				else if (short_date.lessThan(short_min_last_28F_spring))
				{
					short_min_last_28F_spring = new DateTime(short_date);
					_min_last_28F_spring = new DateTime(full_date);
				}
			}

			short_date = ts.getLast32Spring(year);
			if (short_date == null)
			{
				++_missing_count_last_32F_spring;
			}
			else
			{
				full_date = new DateTime(short_date);
				short_date.setYear(0);
				sum_last_32F_spring += short_date.toDouble();
				++_count_last_32F_spring;
				if (short_max_last_32F_spring == null)
				{
					short_max_last_32F_spring = new DateTime(short_date);
					_max_last_32F_spring = new DateTime(full_date);
				}
				else if (short_date.greaterThan(short_max_last_32F_spring))
				{
					short_max_last_32F_spring = new DateTime(short_date);
					_max_last_32F_spring = new DateTime(full_date);
				}
				if (short_min_last_32F_spring == null)
				{
					short_min_last_32F_spring = new DateTime(short_date);
					_min_last_32F_spring = new DateTime(full_date);
				}
				else if (short_date.lessThan(short_min_last_32F_spring))
				{
					short_min_last_32F_spring = new DateTime(short_date);
					_min_last_32F_spring = new DateTime(full_date);
				}
			}

			short_date = ts.getFirst32Fall(year);
			if (short_date == null)
			{
				++_missing_count_first_32F_fall;
			}
			else
			{
				full_date = new DateTime(short_date);
				short_date.setYear(0);
				sum_first_32F_fall += short_date.toDouble();
				++_count_first_32F_fall;
				if (short_max_first_32F_fall == null)
				{
					short_max_first_32F_fall = new DateTime(short_date);
					_max_first_32F_fall = new DateTime(full_date);
				}
				else if (short_date.greaterThan(short_max_first_32F_fall))
				{
					short_max_first_32F_fall = new DateTime(short_date);
					_max_first_32F_fall = new DateTime(full_date);
				}
				if (short_min_first_32F_fall == null)
				{
					short_min_first_32F_fall = new DateTime(short_date);
					_min_first_32F_fall = new DateTime(full_date);
				}
				else if (short_date.lessThan(short_min_first_32F_fall))
				{
					short_min_first_32F_fall = new DateTime(short_date);
					_min_first_32F_fall = new DateTime(full_date);
				}
			}

			short_date = ts.getFirst28Fall(year);
			if (short_date == null)
			{
				++_missing_count_first_28F_fall;
			}
			else
			{
				full_date = new DateTime(short_date);
				short_date.setYear(0);
				sum_first_28F_fall += short_date.toDouble();
				++_count_first_28F_fall;
				if (short_max_first_28F_fall == null)
				{
					short_max_first_28F_fall = new DateTime(short_date);
					_max_first_28F_fall = new DateTime(full_date);
				}
				else if (short_date.greaterThan(short_max_first_28F_fall))
				{
					short_max_first_28F_fall = new DateTime(short_date);
					_max_first_28F_fall = new DateTime(full_date);
				}
				if (short_min_first_28F_fall == null)
				{
					short_min_first_28F_fall = new DateTime(short_date);
					_min_first_28F_fall = new DateTime(full_date);
				}
				else if (short_date.lessThan(short_min_first_28F_fall))
				{
					short_min_first_28F_fall = new DateTime(short_date);
					_min_first_28F_fall = new DateTime(full_date);
				}
			}
		}

	/*
		if( !found ){
			message = "\"" + ts.getIdentifierString() +
			"\": problems finding limits, whole POR missing!";
			Message.printWarning( 2, routine, message );
			throw new TSException ( message );
		}
	*/

		// Now compute the mean...

		if (_count_last_28F_spring != 0)
		{
			_mean_last_28F_spring = new DateTime(sum_last_28F_spring / _count_last_28F_spring, true);
		}
		if (_count_last_32F_spring != 0)
		{
			_mean_last_32F_spring = new DateTime(sum_last_32F_spring / _count_last_32F_spring, true);
		}
		if (_count_first_32F_fall != 0)
		{
			_mean_first_32F_fall = new DateTime(sum_first_32F_fall / _count_first_32F_fall, true);
		}
		if (_count_first_28F_fall != 0)
		{
			_mean_first_28F_fall = new DateTime(sum_first_28F_fall / _count_first_28F_fall, true);
		}
		// Clean up...
		t = null;
		start = null;
		end = null;
		full_date = null;
		short_date = null;
		short_max_last_28F_spring = null;
		short_max_last_32F_spring = null;
		short_max_first_32F_fall = null;
		short_max_first_28F_fall = null;

		short_min_last_28F_spring = null;
		short_min_last_32F_spring = null;
		short_min_first_32F_fall = null;
		short_min_first_28F_fall = null;
		}
		catch (Exception e)
		{
			message = "Error computing data limits.";
			Message.printWarning(2, routine, message);
			Message.printWarning(2, routine, e);
			throw new TSException(message);
		}
		message = null;
		routine = null;
	}

	/// <summary>
	/// Return the maximum date corresponding to the first 28 F temperature. </summary>
	/// <returns> The maximum date corresponding to the first 28 F temperature in the
	/// fall, or null if not available. </returns>
	public virtual DateTime getMaxFirst28Fall()
	{
		return _max_first_28F_fall;
	}

	/// <summary>
	/// Return the maximum date corresponding to the first 32 F temperature. </summary>
	/// <returns> The maximum date corresponding to the first 32 F temperature in the
	/// fall, or null if not available. </returns>
	public virtual DateTime getMaxFirst32Fall()
	{
		return _max_first_32F_fall;
	}

	/// <summary>
	/// Return the maximum date corresponding to the last 28 F temperature. </summary>
	/// <returns> The maximum date corresponding to the last 28 F temperature in the
	/// spring, or null if not available. </returns>
	public virtual DateTime getMaxLast28Spring()
	{
		return _max_last_28F_spring;
	}

	/// <summary>
	/// Return the maximum date corresponding to the last 32 F temperature. </summary>
	/// <returns> The maximum date corresponding to the last 32 F temperature in the
	/// spring, or null if not available. </returns>
	public virtual DateTime getMaxLast32Spring()
	{
		return _max_last_32F_spring;
	}

	/// <summary>
	/// Return the mean date corresponding to the first 28 F temperature. </summary>
	/// <returns> The mean date corresponding to the first 28 F temperature in the
	/// fall, or null if not available. </returns>
	public virtual DateTime getMeanFirst28Fall()
	{
		return _mean_first_28F_fall;
	}

	/// <summary>
	/// Return the mean date corresponding to the first 32 F temperature. </summary>
	/// <returns> The mean date corresponding to the first 32 F temperature in the
	/// fall, or null if not available. </returns>
	public virtual DateTime getMeanFirst32Fall()
	{
		return _mean_first_32F_fall;
	}

	/// <summary>
	/// Return The mean date corresponding to the last 28 F temperature. </summary>
	/// <returns> The mean date corresponding to the last 28 F temperature in the
	/// spring, or null if not available. </returns>
	public virtual DateTime getMeanLast28Spring()
	{
		return _mean_last_28F_spring;
	}

	/// <summary>
	/// Return the mean date corresponding to the last 32 F temperature. </summary>
	/// <returns> The mean date corresponding to the last 32 F temperature in the
	/// spring, or null if not available. </returns>
	public virtual DateTime getMeanLast32Spring()
	{
		return _mean_last_32F_spring;
	}

	/// <summary>
	/// Return the minimum date corresponding to the first 28 F temperature. </summary>
	/// <returns> The minimum date corresponding to the first 28 F temperature in the
	/// fall, or null if not available. </returns>
	public virtual DateTime getMinFirst28Fall()
	{
		return _min_first_28F_fall;
	}

	/// <summary>
	/// Return the minimum date corresponding to the first 32 F temperature. </summary>
	/// <returns> The minimum date corresponding to the first 32 F temperature in the
	/// fall, or null if not available. </returns>
	public virtual DateTime getMinFirst32Fall()
	{
		return _min_first_32F_fall;
	}

	/// <summary>
	/// Return the minimum date corresponding to the last 28 F temperature. </summary>
	/// <returns> The minimum date corresponding to the last 28 F temperature in the
	/// spring, or null if not available. </returns>
	public virtual DateTime getMinLast28Spring()
	{
		return _min_last_28F_spring;
	}

	/// <summary>
	/// Return the minimum date corresponding to the last 32 F temperature. </summary>
	/// <returns> The minimum date corresponding to the last 32 F temperature in the
	/// spring, or null if not available. </returns>
	public virtual DateTime getMinLast32Spring()
	{
		return _min_last_32F_spring;
	}

	/// <summary>
	/// Initialize the instance data.
	/// </summary>
	private void initialize()
	{
		_ts = null; // No time series specified in constructor.

		_max_last_28F_spring = null;
		_max_last_32F_spring = null;
		_max_first_32F_fall = null;
		_max_first_28F_fall = null;

		_min_last_28F_spring = null;
		_min_last_32F_spring = null;
		_min_first_32F_fall = null;
		_min_first_28F_fall = null;

		_mean_last_28F_spring = null;
		_mean_last_32F_spring = null;
		_mean_first_32F_fall = null;
		_mean_first_28F_fall = null;

		_count_last_28F_spring = 0;
		_count_last_32F_spring = 0;
		_count_first_32F_fall = 0;
		_count_first_28F_fall = 0;

		_missing_count_last_28F_spring = 0;
		_missing_count_last_32F_spring = 0;
		_missing_count_first_32F_fall = 0;
		_missing_count_first_28F_fall = 0;
	}

	/// <summary>
	/// Return A verbose string representation of the limits. </summary>
	/// <returns> A verbose string representation of the limits. </returns>
	public override string ToString()
	{
		string nl = System.getProperty("line.separator");

		StringBuilder buffer = new StringBuilder();
		if (_ts != null)
		{
			buffer.Append("Time series:  " + _ts.getIdentifierString() + " (" + _ts.getDataUnits() + ")" + nl);
		}
		buffer.Append("Data limits for period " + getDate1() + " to " + getDate2() + " are as follows.  Maximum and minimum" + nl + "dates shown were computed using month and day." + nl + "                                                     Number    Number Not" + nl + "Frost Temp.        MinDate    MaxDate    MeanDate    Missing   Missing" + nl + "------------------------------------------------------------------------------" + nl);
		string empty_date_string = "          ";
		// Handle the individual data that may be null...
		// First row...
		buffer.Append("Last 28 F Spring ");
		if (_min_last_28F_spring == null)
		{
			buffer.Append(empty_date_string + " ");
		}
		else
		{
			buffer.Append(_min_last_28F_spring.ToString(DateTime.FORMAT_YYYY_MM_DD) + " ");
		}
		if (_max_last_28F_spring == null)
		{
			buffer.Append(empty_date_string + " ");
		}
		else
		{
			buffer.Append(_max_last_28F_spring.ToString(DateTime.FORMAT_YYYY_MM_DD) + " ");
		}
		if (_mean_last_28F_spring == null)
		{
			buffer.Append(empty_date_string);
		}
		else
		{
			buffer.Append("    " + _mean_last_28F_spring.ToString(DateTime.FORMAT_MM_DD) + " ");
		}
		buffer.Append(StringUtil.formatString(_missing_count_last_28F_spring, "%9d"));
		buffer.Append("    " + StringUtil.formatString(_count_last_28F_spring, "%9d") + nl);
		// Second row...
		buffer.Append("Last 32 F Spring ");
		if (_min_last_32F_spring == null)
		{
			buffer.Append(empty_date_string + " ");
		}
		else
		{
			buffer.Append(_min_last_32F_spring.ToString(DateTime.FORMAT_YYYY_MM_DD) + " ");
		}
		if (_max_last_32F_spring == null)
		{
			buffer.Append(empty_date_string + " ");
		}
		else
		{
			buffer.Append(_max_last_32F_spring.ToString(DateTime.FORMAT_YYYY_MM_DD) + " ");
		}
		if (_mean_last_32F_spring == null)
		{
			buffer.Append(empty_date_string);
		}
		else
		{
			buffer.Append("    " + _mean_last_32F_spring.ToString(DateTime.FORMAT_MM_DD) + " ");
		}
		buffer.Append(StringUtil.formatString(_missing_count_last_32F_spring, "%9d"));
		buffer.Append("    " + StringUtil.formatString(_count_last_32F_spring, "%9d") + nl);
		// Third row...
		buffer.Append("First 32 F Fall  ");
		if (_min_last_32F_spring == null)
		{
			buffer.Append(empty_date_string + " ");
		}
		else
		{
			buffer.Append(_min_first_32F_fall.ToString(DateTime.FORMAT_YYYY_MM_DD) + " ");
		}
		if (_max_first_32F_fall == null)
		{
			buffer.Append(empty_date_string + " ");
		}
		else
		{
			buffer.Append(_max_first_32F_fall.ToString(DateTime.FORMAT_YYYY_MM_DD) + " ");
		}
		if (_mean_first_32F_fall == null)
		{
			buffer.Append(empty_date_string);
		}
		else
		{
			buffer.Append("    " + _mean_first_32F_fall.ToString(DateTime.FORMAT_MM_DD) + " ");
		}
		buffer.Append(StringUtil.formatString(_missing_count_first_32F_fall, "%9d"));
		buffer.Append("    " + StringUtil.formatString(_count_first_32F_fall, "%9d") + nl);
		// Fourth row...
		buffer.Append("First 28 F Fall  ");
		if (_min_last_28F_spring == null)
		{
			buffer.Append(empty_date_string + " ");
		}
		else
		{
			buffer.Append(_min_first_28F_fall.ToString(DateTime.FORMAT_YYYY_MM_DD) + " ");
		}
		if (_max_first_28F_fall == null)
		{
			buffer.Append(empty_date_string + " ");
		}
		else
		{
			buffer.Append(_max_first_28F_fall.ToString(DateTime.FORMAT_YYYY_MM_DD) + " ");
		}
		if (_mean_first_28F_fall == null)
		{
			buffer.Append(empty_date_string);
		}
		else
		{
			buffer.Append("    " + _mean_first_28F_fall.ToString(DateTime.FORMAT_MM_DD) + " ");
		}
		buffer.Append(StringUtil.formatString(_missing_count_first_28F_fall, "%9d"));
		buffer.Append("    " + StringUtil.formatString(_count_first_28F_fall, "%9d") + nl);
		buffer.Append("------------------------------------------------------------------------------" + nl);
		string s = buffer.ToString();
		nl = null;
		empty_date_string = null;
		buffer = null;
		return s;
	}

	} // End of FrostDatesYearTSLimits class definition

}