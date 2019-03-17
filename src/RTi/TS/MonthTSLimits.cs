using System;
using System.Text;

// MonthTSLimits - simple class for returning time series data limits

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
// MonthTSLimits - simple class for returning time series data limits
// ----------------------------------------------------------------------------
// Notes:	(1)	This class stores the data limits for data space.
//			It stores the maximum and minimum values and the dates
//			associated with the values.
//		(2)	This class extends the TSLimits class.  Monthly time
//			series are meant to have the overall limits and limits
//			by month.
// ----------------------------------------------------------------------------
// History:
//
// 30 Dec 1998	Steven A. Malers, RTi	Initial version.  Extend TSLimits.
// 13 Apr 1999	SAM, RTi		Add finalize.
// 2001-11-06	SAM, RTi		Review javadoc.  Verify that variables
//					are set to null when no longer used.
// 2003-03-25	SAM, RTi		Update to use new data units member
//					in the base class, to deal with current
//					and original units better.
// 2003-06-02	SAM, RTi		Upgrade to use generic classes.
//					* Change TSDate to DateTime.
// 2004-05-23	SAM, RTi		* Add percent to the toString() output
//					  for missing and non-missing
//					  (requested by Leonard Rice and makes
//					  sense since other intervals show it
//					  in output).
// 2005-02-04	SAM, RTi		Add merge() to merge two MonthTSLimits()
//					together.
//					The toString() method for missing
//					monthly data was not handling null min
//					and max dates for the period - fix it.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------
// EndHeader

namespace RTi.TS
{
	using MathUtil = RTi.Util.Math.MathUtil;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using DateTime = RTi.Util.Time.DateTime;
	using TimeUtil = RTi.Util.Time.TimeUtil;

	/// <summary>
	/// This class stores information about the data and date limits of a monthly time series.
	/// If detailed information is not necessary, use the TSLimits class for overall limits.
	/// In summary, since this class extends TSLimits, both full period limits and limits by month are computed.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class MonthTSLimits extends TSLimits
	[Serializable]
	public class MonthTSLimits : TSLimits
	{

	// TODO SAM 2005-02-07 This also seems to be in the base class!  Need to use
	// the single copy.  Only merge() seems to use both.
	// Time series that is being studied...

	internal MonthTS _ts = null;

	// Data are by month (12 values)...

	private double[] _max_value_by_month = null;
	private DateTime[] _max_value_date_by_month = null;
	private double[] _mean_by_month = null;
	private double[] __medianByMonth = null;
	private double[] _min_value_by_month = null;
	private DateTime[] _min_value_date_by_month = null;
	private int[] _missing_data_count_by_month = null;
	private int[] _non_missing_data_count_by_month = null;
	private DateTime[] _non_missing_data_date1_by_month = null;
	private DateTime[] _non_missing_data_date2_by_month = null;
	private double[] __skewByMonth = null;
	private double[] __stdDevByMonth = null;
	private double[] _sum_by_month = null;

	/// <summary>
	/// Default constructor.  Initialize the dates to null and the limits to zeros. </summary>
	/// <exception cref="TSException"> if there is error an creating the limits. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MonthTSLimits() throws TSException
	public MonthTSLimits() : base()
	{
		try
		{
			initialize();
		}
		catch (Exception)
		{
			string message = "Error creating MonthTSLimits";
			Message.printWarning(3, "MonthTSLimits", message);
			throw new TSException(message);
		}
	}

	/// <summary>
	/// Copy constructor. </summary>
	/// <param name="limits"> Instance to copy. </param>
	/// <exception cref="TSException"> if there is an error c creating the limits. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MonthTSLimits(MonthTSLimits limits) throws TSException
	public MonthTSLimits(MonthTSLimits limits) : base()
	{
		try
		{
			initialize();

			for (int i = 0; i < 12; i++)
			{
				_max_value_by_month[i] = limits.getMaxValue(i + 1);
				_min_value_by_month[i] = limits.getMaxValue(i + 1);
				_max_value_date_by_month[i] = new DateTime(limits.getMaxValueDate(i + 1));
				_min_value_date_by_month[i] = new DateTime(limits.getMinValueDate(i + 1));
				_mean_by_month[i] = limits.getMean(i + 1);
				__medianByMonth[i] = limits.getMedian(i + 1);
				_missing_data_count_by_month[i] = limits.getMissingDataCount(i + 1);
				_non_missing_data_count_by_month[i] = limits.getNonMissingDataCount(i + 1);
				_non_missing_data_date1_by_month[i] = new DateTime(limits.getNonMissingDataDate1(i + 1));
				_non_missing_data_date2_by_month[i] = new DateTime(limits.getNonMissingDataDate2(i + 1));
				__skewByMonth[i] = limits.getSkew(i + 1);
				__stdDevByMonth[i] = limits.getStdDev(i + 1);
				_sum_by_month[i] = limits.getSum(i + 1);
			}
		}
		catch (Exception)
		{
			string message = "Error creating MonthTSLimits";
			Message.printWarning(2, "MonthTSLimits.copy", message);
			throw new TSException(message);
		}
	}

	/// <summary>
	/// Constructor to compute the limits given a MonthTS.  This is the main constructor
	/// and is overloaded in a variety of ways.  If a variant of this
	/// constructor that does not take a MonthTS is used, the limits are not computed
	/// in this class and must be set from calling code. </summary>
	/// <param name="ts"> Time series of interest. </param>
	/// <param name="startdate"> Starting date for the check. </param>
	/// <param name="enddate"> Ending date for the check. </param>
	/// <param name="flags"> Indicates special operations performed during computations.
	/// See the TSLimits.REFRESH_TS, TSLimits.NO_COMPUTE_DETAIL, and
	/// TSLimits.NO_COMPUTE_TOTALS flags for explanations. </param>
	/// <exception cref="TSException"> if there is an error c creating the limits. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MonthTSLimits(MonthTS ts, RTi.Util.Time.DateTime startdate, RTi.Util.Time.DateTime enddate, int flags) throws TSException
	public MonthTSLimits(MonthTS ts, DateTime startdate, DateTime enddate, int flags) : base(ts, startdate, enddate, flags)
	{ // Compute the total limits...
		try
		{
			// Compute the monthly limits...
			initialize();
			_ts = ts;
			bool refresh_flag = false;
			if ((flags & TSLimits.REFRESH_TS) != 0)
			{
				refresh_flag = true;
			}
			calculateDataLimits(ts, startdate, enddate, refresh_flag);
		}
		catch (Exception e)
		{
			string message = "Error creating MonthTSLimits (" + e + ")";
			Message.printWarning(3, "MonthTSLimits(MonthTS,...)", message);
			throw new TSException(message);
		}
	}

	/// <summary>
	/// Construct the MonthTS limits for the full period. </summary>
	/// <param name="ts"> Time series of interest. </param>
	/// <exception cref="TSException"> if there is an error computing the limits. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MonthTSLimits(MonthTS ts) throws TSException
	public MonthTSLimits(MonthTS ts) : base(ts)
	{ // Compute the total limits...
		try
		{
			// Compute the monthly limits...
			initialize();
			_ts = ts;
			calculateDataLimits(ts, (DateTime)null, (DateTime)null, false);
		}
		catch (Exception)
		{
			string message = "Error creating MonthTSLimits";
			Message.printWarning(2, "MonthTSLimits(MonthTS)", message);
			throw new TSException(message);
		}
	}

	/// <summary>
	/// Construct the MonthTS limits between two dates. </summary>
	/// <param name="ts"> Time series of interest. </param>
	/// <param name="startdate"> Starting date for the check. </param>
	/// <param name="enddate"> Ending date for the check. </param>
	/// <exception cref="TSException"> if there is an error computing the limits. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MonthTSLimits(MonthTS ts, RTi.Util.Time.DateTime startdate, RTi.Util.Time.DateTime enddate) throws TSException
	public MonthTSLimits(MonthTS ts, DateTime startdate, DateTime enddate) : base(ts, startdate, enddate)
	{ // Compute the total limits...
		try
		{
			// Compute the monthly limits...
			initialize();
			_ts = ts;
			calculateDataLimits(ts, startdate, enddate, false);
		}
		catch (Exception)
		{
			string message = "Error creating MonthTSLimits";
			Message.printWarning(2, "MonthTSLimits(MonthTS)", message);
			throw new TSException(message);
		}
	}

	/// <summary>
	/// Finalize before garbage collection. </summary>
	/// <exception cref="Throwable"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~MonthTSLimits()
	{
		_ts = null;
		for (int i = 0; i < 12; i++)
		{
			_max_value_date_by_month[i] = null;
			_min_value_date_by_month[i] = null;
			_non_missing_data_date1_by_month[i] = null;
			_non_missing_data_date2_by_month[i] = null;
		}
		_max_value_by_month = null;
		_min_value_by_month = null;
		_max_value_date_by_month = null;
		_min_value_date_by_month = null;
		_mean_by_month = null;
		_missing_data_count_by_month = null;
		_non_missing_data_count_by_month = null;
		_non_missing_data_date1_by_month = null;
		_non_missing_data_date2_by_month = null;
		_sum_by_month = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Compute the monthly data limits for a monthly time series between two dates.
	/// This code was taken from the TSUtil.getDataLimits method.  This method should
	/// be private.  Otherwise, the base class may call this method when it really needs to call its own version. </summary>
	/// <param name="ts"> Time series of interest. </param>
	/// <param name="start0"> Starting date for the check. </param>
	/// <param name="end0"> Ending date for the check. </param>
	/// <param name="refresh_flag"> Indicates whether the time series should be refreshed first
	/// (in general this is used only within the TS package and the version of this
	/// routine without the flag should be called). </param>
	/// <seealso cref= TSLimits </seealso>
	/// <exception cref="TSException"> if there is an error computing the detailed limits. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void calculateDataLimits(TS ts, RTi.Util.Time.DateTime start0, RTi.Util.Time.DateTime end0, boolean refresh_flag) throws TSException
	private void calculateDataLimits(TS ts, DateTime start0, DateTime end0, bool refresh_flag)
	{
		string message, routine = "MonthTSLimits.getDataLimits";
		double value = 0.0;
		double[] max_by_month = null, min_by_month = null, sum_by_month = null;
		int @base = 0, month_index = 0, mult = 0;
		int[] missing_count_by_month = null, non_missing_count_by_month = null;
		bool found = false;
		bool[] found_by_month = null;
		DateTime date, t = null;
		DateTime[] max_date_by_month = null, min_date_by_month = null, non_missing_data_date1_by_month = null, non_missing_data_date2_by_month = null;

		try
		{
			// Overall try...

			if (ts == null)
			{
				message = "NULL time series";
				Message.printWarning(3, routine, message);
				throw new TSException(message);
			}

			// Get valid date limits because the ones passed in may have been null...

			double missing = ts.getMissing();
			TSLimits valid_dates = TSUtil.getValidPeriod(ts, start0, end0);
			DateTime start = valid_dates.getDate1();
			DateTime end = valid_dates.getDate2();
			valid_dates = null;

			// Make sure that the time series has current limits...

			@base = ts.getDataIntervalBase();
			mult = ts.getDataIntervalMult();

			// Get the variables that are used often in this function.

			// Loop through the dates and get max and min data values;

			// A regular TS... easier to iterate...

			found_by_month = new bool[12];
			max_by_month = new double[12];
			max_date_by_month = new DateTime[12];
			min_by_month = new double[12];
			min_date_by_month = new DateTime[12];
			missing_count_by_month = new int[12];
			non_missing_data_date1_by_month = new DateTime[12];
			non_missing_data_date2_by_month = new DateTime[12];
			non_missing_count_by_month = new int[12];
			sum_by_month = new double[12];
			for (int i = 0; i < 12; i++)
			{
				found_by_month[i] = false;
				max_by_month[i] = missing;
				max_date_by_month[i] = null;
				min_by_month[i] = missing;
				min_date_by_month[i] = null;
				missing_count_by_month[i] = 0;
				non_missing_data_date1_by_month[i] = null;
				non_missing_data_date2_by_month[i] = null;
				non_missing_count_by_month[i] = 0;
				sum_by_month[i] = missing;
			}

			// Figure out if we are treating data <= 0 as missing...

			bool ignore_lezero = false;
			if ((_flags & IGNORE_LESS_THAN_OR_EQUAL_ZERO) != 0)
			{
				ignore_lezero = true;
			}

			// First loop through and find the data limits and the
			// minimum non-missing date...

			t = new DateTime(start, DateTime.DATE_FAST);
			for (; t.lessThanOrEqualTo(end); t.addInterval(@base, mult))
			{
				// Save the month index...
				month_index = t.getMonth() - 1;

				value = ts.getDataValue(t);

				if (ts.isDataMissing(value) || (ignore_lezero && (value <= 0.0)))
				{
					// The value is missing
					++missing_count_by_month[month_index];
					continue;
				}

				// Else, data value is not missing...

				if (ts.isDataMissing(sum_by_month[month_index]))
				{
					// Reset the sum...
					sum_by_month[month_index] = value;
				}
				else
				{
					// Add to the sum...
					sum_by_month[month_index] += value;
				}
				++non_missing_count_by_month[month_index];

				if (found_by_month[month_index])
				{
					// Already found the first non-missing point so
					// all we need to do is check the limits.  These
					// should only result in new DateTime a few times...
					if (value > max_by_month[month_index])
					{
						   max_by_month[month_index] = value;
						max_date_by_month[month_index] = new DateTime(t);
					}
					if (value < min_by_month[month_index])
					{
						   min_by_month[month_index] = value;
						min_date_by_month[month_index] = new DateTime(t);
					}
				}
				else
				{
					// First non-missing point so set the initial values...
					date = new DateTime(t);
					max_by_month[month_index] = value;
					max_date_by_month[month_index] = date;
					min_by_month[month_index] = value;
					min_date_by_month[month_index] = date;
					non_missing_data_date1_by_month[month_index] = date;
					non_missing_data_date2_by_month[month_index] = date;
					found_by_month[month_index] = true;
					found = true; // Overall check.
				}
			}

			if (!found)
			{
				message = "\"" + ts.getIdentifierString() + "\": problems finding limits, whole POR missing!";
				Message.printWarning(2, routine, message);
				throw new TSException(message);
			}

			// Set the monthly values...

			double mean;
			for (int i = 1; i <= 12; i++)
			{
				setMaxValue(i, max_by_month[i - 1], max_date_by_month[i - 1]);
				setMinValue(i, min_by_month[i - 1], min_date_by_month[i - 1]);
				setNonMissingDataDate1(i, non_missing_data_date1_by_month[i - 1]);
				setNonMissingDataDate2(i, non_missing_data_date2_by_month[i - 1]);
				setMissingDataCount(i, missing_count_by_month[i - 1]);
				setNonMissingDataCount(i, non_missing_count_by_month[i - 1]);
				if (((!ignore_lezero && !ts.isDataMissing(sum_by_month[i - 1])) || (ignore_lezero && ((sum_by_month[i - 1] > 0.0) && !ts.isDataMissing(sum_by_month[i - 1])))) && (non_missing_count_by_month[i - 1] > 0))
				{
					mean = sum_by_month[i - 1] / (double)non_missing_count_by_month[i - 1];
				}
				else
				{
					mean = missing;
				}
				setSum(i, sum_by_month[i - 1]);
				setMean(i, mean);

				// TODO SAM 2010-06-15 This is a performance hit, but not too bad
				// TODO SAM 2010-06-15 Consider treating other statistics similarly but need to define unit tests
				// TODO SAM 2010-06-15 This code would need to be changed if doing Lag-1 correlation because order matters
				// For newly added statistics, use helper method to get data, ignoring missing...
				double[] dataArray = TSUtil.toArray(ts, start, end, i, false);
				// Check for <= 0 values if necessary
				int nDataArray = dataArray.Length;
				if (ignore_lezero)
				{
					for (int iData = 0; iData < nDataArray; iData++)
					{
						if (dataArray[iData] <= 0.0)
						{
							// Just exchange with the last value and reduce the size
							double temp = dataArray[iData];
							dataArray[iData] = dataArray[nDataArray - 1];
							dataArray[nDataArray - 1] = temp;
							--nDataArray;
						}
					}
				}
				if (nDataArray > 0)
				{
					setMedian(i, MathUtil.median(nDataArray, dataArray));
				}
				if (nDataArray > 1)
				{
					// At least 2 values required
					try
					{
						setStdDev(i, MathUtil.standardDeviation(nDataArray, dataArray));
					}
					catch (Exception)
					{
						// Likely due to small sample size
					}
				}
				if (nDataArray > 2)
				{
					try
					{
						setSkew(i, MathUtil.skew(nDataArray, dataArray));
					}
					catch (Exception)
					{
						// Likely due to small sample size
					}
				}
			}
		}
		catch (Exception e)
		{
			message = "Error computing data limits.";
			Message.printWarning(3, routine, message);
			Message.printWarning(3, routine, e);
			throw new TSException(message);
		}
	}

	/// <summary>
	/// Return The maximum data value for the indicated month. </summary>
	/// <returns> The maximum data value for the indicated month, or -999 if the month is invalid. </returns>
	/// <param name="month"> Month of interest (1-12). </param>
	public virtual double getMaxValue(int month)
	{
		if ((month >= 1) && (month <= 12))
		{
			return _max_value_by_month[month - 1];
		}
		else
		{
			return -999.0;
		}
	}

	/// <summary>
	/// Return the date corresponding to the maximum data value for the indicated month. </summary>
	/// <returns> The date corresponding to the maximum data value for the indicated
	/// month, or null if the month is invalid.  A copy of the date is returned. </returns>
	/// <param name="month"> Month of interest (1-12). </param>
	public virtual DateTime getMaxValueDate(int month)
	{
		if ((month >= 1) && (month <= 12))
		{
			if (_max_value_date_by_month[month - 1] == null)
			{
				return _max_value_date_by_month[month - 1];
			}
			return new DateTime(_max_value_date_by_month[month - 1]);
		}
		else
		{
			return null;
		}
	}

	/// <summary>
	/// Return the mean data value for the indicated month. </summary>
	/// <returns> The mean data value for the indicated month, or -999 if the month is invalid. </returns>
	/// <param name="month"> Month of interest (1-12). </param>
	public virtual double getMean(int month)
	{
		if ((month >= 1) && (month <= 12))
		{
			return _mean_by_month[month - 1];
		}
		else
		{
			return -999.0;
		}
	}

	/// <summary>
	/// Return the mean data value array. </summary>
	/// <returns> The mean data value array (12 monthly values with the first value
	/// corresponding to January).  This can be used, for example, to pass the
	/// values to the TSUtil.fillConstant method to a fill the missing data in a
	/// time series with missing data.  The array will be null if a time series has not been analyzed. </returns>
	public virtual double [] getMeanArray()
	{
		return _mean_by_month;
	}

	/// <summary>
	/// Return the median data value for the indicated month. </summary>
	/// <returns> The median data value for the indicated month, or -999 if the month is invalid. </returns>
	/// <param name="month"> Month of interest (1-12). </param>
	public virtual double getMedian(int month)
	{
		if ((month >= 1) && (month <= 12))
		{
			return __medianByMonth[month - 1];
		}
		else
		{
			return -999.0;
		}
	}

	/// <summary>
	/// Return the median data value array. </summary>
	/// <returns> The median data value array (12 monthly values with the first value
	/// corresponding to January).  The array will be null if a time series has not been analyzed. </returns>
	public virtual double [] getMedianArray()
	{
		return __medianByMonth;
	}

	/// <summary>
	/// Return the minimum data value for the indicated month. </summary>
	/// <returns> The minimum data value for the indicated month, or -999 if the month is invalid. </returns>
	/// <param name="month"> Month of interest (1-12). </param>
	public virtual double getMinValue(int month)
	{
		if ((month >= 1) && (month <= 12))
		{
			return _min_value_by_month[month - 1];
		}
		else
		{
			return -999.0;
		}
	}

	/// <summary>
	/// Return the date corresponding to the minimum data value for the indicated month. </summary>
	/// <returns> The date corresponding to the minimum data value for the indicated
	/// month, or null if the month is invalid.  A copy is returned. </returns>
	/// <param name="month"> Month of interest (1-12). </param>
	public virtual DateTime getMinValueDate(int month)
	{
		if ((month >= 1) && (month <= 12))
		{
			if (_min_value_date_by_month[month - 1] == null)
			{
				return _min_value_date_by_month[month - 1];
			}
			return new DateTime(_min_value_date_by_month[month - 1]);
		}
		else
		{
			return null;
		}
	}

	/// <summary>
	/// Return the count for the number of missing data for the indicated month. </summary>
	/// <returns> The count for the number of missing data for the indicated month, or -1 if month is invalid. </returns>
	/// <param name="month"> Month of interest (1-12). </param>
	public virtual int getMissingDataCount(int month)
	{
		if ((month >= 1) && (month <= 12))
		{
			return _missing_data_count_by_month[month - 1];
		}
		else
		{
			return -1;
		}
	}

	/// <summary>
	/// Return the count for the number of non-missing data for the indicated month. </summary>
	/// <returns> The count for the number of non-missing data for the indicated month, or -1 if month is invalid. </returns>
	/// <param name="month"> Month of interest (1-12). </param>
	public virtual int getNonMissingDataCount(int month)
	{
		if ((month >= 1) && (month <= 12))
		{
			return _non_missing_data_count_by_month[month - 1];
		}
		else
		{
			return -1;
		}
	}

	/// <summary>
	/// Return the date corresponding to the first non-missing data for the indicated month. </summary>
	/// <returns> The date corresponding to the first non-missing data for the indicated
	/// month.  A copy of the date is returned. </returns>
	/// <param name="month"> Month of interest (1-12). </param>
	public virtual DateTime getNonMissingDataDate1(int month)
	{
		if ((month >= 1) && (month <= 12))
		{
			if (_non_missing_data_date1_by_month[month - 1] == null)
			{
				return _non_missing_data_date1_by_month[month - 1];
			}
			return new DateTime(_non_missing_data_date1_by_month[month - 1]);
		}
		else
		{
			return null;
		}
	}

	/// <summary>
	/// Return the date corresponding to the last non-missing data for the indicated month. </summary>
	/// <returns> The date corresponding to the last non-missing data for the indicated
	/// month or null if month is out of range.  A copy of the date is returned. </returns>
	/// <param name="month"> Month of interest (1-12). </param>
	public virtual DateTime getNonMissingDataDate2(int month)
	{
		if ((month >= 1) && (month <= 12))
		{
			if (_non_missing_data_date2_by_month[month - 1] == null)
			{
				return _non_missing_data_date2_by_month[month - 1];
			}
			return new DateTime(_non_missing_data_date2_by_month[month - 1]);
		}
		else
		{
			return null;
		}
	}

	/// <summary>
	/// Return the skew for the indicated month. </summary>
	/// <returns> The skew for the indicated month, or -999 if the month is invalid. </returns>
	/// <param name="month"> Month of interest (1-12). </param>
	public virtual double getSkew(int month)
	{
		if ((month >= 1) && (month <= 12))
		{
			return __skewByMonth[month - 1];
		}
		else
		{
			return -999.0;
		}
	}

	/// <summary>
	/// Return the skew array. </summary>
	/// <returns> The skew array (12 monthly values with the first value
	/// corresponding to January).  The array will be null if a time series has not been analyzed. </returns>
	public virtual double [] getSkewArray()
	{
		return __skewByMonth;
	}

	/// <summary>
	/// Return the standard deviation for the indicated month. </summary>
	/// <returns> The standard deviation for the indicated month, or -999 if the month is invalid. </returns>
	/// <param name="month"> Month of interest (1-12). </param>
	public virtual double getStdDev(int month)
	{
		if ((month >= 1) && (month <= 12))
		{
			return __stdDevByMonth[month - 1];
		}
		else
		{
			return -999.0;
		}
	}

	/// <summary>
	/// Return the standard deviation array. </summary>
	/// <returns> The standard deviation array (12 monthly values with the first value
	/// corresponding to January).  The array will be null if a time series has not been analyzed. </returns>
	public virtual double [] getStdDevArray()
	{
		return __stdDevByMonth;
	}

	/// <summary>
	/// Return the sum for the indicated month. </summary>
	/// <returns> The sum for the indicated month, or -999 if the month is invalid. </returns>
	/// <param name="month"> Month of interest (1-12). </param>
	public virtual double getSum(int month)
	{
		if ((month >= 1) && (month <= 12))
		{
			return _sum_by_month[month - 1];
		}
		else
		{
			return -999.0;
		}
	}

	/// <summary>
	/// Initialize the instance.
	/// </summary>
	private void initialize()
	{
		_ts = null; // No time series specified in constructor.
		_max_value_by_month = new double[12];
		_min_value_by_month = new double[12];
		_max_value_date_by_month = new DateTime[12];
		_min_value_date_by_month = new DateTime[12];
		_mean_by_month = new double[12];
		__medianByMonth = new double[12];
		_missing_data_count_by_month = new int[12];
		_non_missing_data_count_by_month = new int[12];
		_non_missing_data_date1_by_month = new DateTime[12];
		_non_missing_data_date2_by_month = new DateTime[12];
		__skewByMonth = new double[12];
		__stdDevByMonth = new double[12];
		_sum_by_month = new double[12];
		for (int i = 0; i < 12; i++)
		{
			_max_value_by_month[i] = 0.0;
			_min_value_by_month[i] = 0.0;
			_max_value_date_by_month[i] = null;
			_min_value_date_by_month[i] = null;
			__medianByMonth[i] = -999.0;
			_mean_by_month[i] = -999.0;
			_missing_data_count_by_month[i] = 0;
			_non_missing_data_count_by_month[i] = 0;
			_non_missing_data_date1_by_month[i] = null;
			_non_missing_data_date2_by_month[i] = null;
			__skewByMonth[i] = -999.0;
			__stdDevByMonth[i] = -999.0;
			_sum_by_month[i] = -999.0;
		}
	}

	/// <summary>
	/// Set the maximum data value for the indicated month. </summary>
	/// <param name="month"> Month of interest (1-12). </param>
	/// <param name="max_value"> The maximum data value. </param>
	public virtual void setMaxValue(int month, double max_value)
	{
		if ((month >= 1) && (month <= 12))
		{
			_max_value_by_month[month - 1] = max_value;
		}
	}

	/// <summary>
	/// Set the maximum data value for the indicated month. </summary>
	/// <param name="month"> Month of interest (1-12). </param>
	/// <param name="max_value"> The maximum data value. </param>
	/// <param name="max_value_date"> The date corresponding to the maximum data value. </param>
	public virtual void setMaxValue(int month, double max_value, DateTime max_value_date)
	{
		if ((month >= 1) && (month <= 12))
		{
			_max_value_by_month[month - 1] = max_value;
			setMaxValueDate(month, max_value_date);
		}
	}

	/// <summary>
	/// Set the date corresponding to the maximum data value for the indicated month. </summary>
	/// <param name="month"> Month of interest (1-12). </param>
	/// <param name="max_value_date"> The date corresponding to the maximum data value. </param>
	public virtual void setMaxValueDate(int month, DateTime max_value_date)
	{
		if ((max_value_date != null) && (month >= 1) && (month <= 12))
		{
			_max_value_date_by_month[month - 1] = new DateTime(max_value_date);
		}
		//checkDates();
	}

	/// <summary>
	/// Set the mean for data for the indicated month. </summary>
	/// <param name="month"> Month of interest (1-12). </param>
	/// <param name="mean"> The mean for the month. </param>
	public virtual void setMean(int month, double mean)
	{
		if ((month >= 1) && (month <= 12))
		{
			_mean_by_month[month - 1] = mean;
		}
	}

	/// <summary>
	/// Set the median for data for the indicated month. </summary>
	/// <param name="month"> Month of interest (1-12). </param>
	/// <param name="median"> The median for the month. </param>
	public virtual void setMedian(int month, double median)
	{
		if ((month >= 1) && (month <= 12))
		{
			__medianByMonth[month - 1] = median;
		}
	}

	/// <summary>
	/// Set the minimum data value for the indicated month. </summary>
	/// <param name="month"> Month of interest (1-12). </param>
	/// <param name="min_value"> The minimum data value. </param>
	public virtual void setMinValue(int month, double min_value)
	{
		if ((month >= 1) && (month <= 12))
		{
			_min_value_by_month[month - 1] = min_value;
		}
	}

	/// <summary>
	/// Set the minimum data value for the month of interest. </summary>
	/// <param name="month"> Month of interest (1-12). </param>
	/// <param name="min_value"> The minimum data value. </param>
	/// <param name="min_value_date"> The date corresponding to the minimum data value. </param>
	public virtual void setMinValue(int month, double min_value, DateTime min_value_date)
	{
		if ((month >= 1) && (month <= 12))
		{
			_min_value_by_month[month - 1] = min_value;
			setMinValueDate(month, min_value_date);
		}
	}

	/// <summary>
	/// Set the date corresponding to the minimum data value for the month of interest. </summary>
	/// <param name="month"> Month of interest (1-12). </param>
	/// <param name="min_value_date"> The date corresponding to the minimum data value. </param>
	public virtual void setMinValueDate(int month, DateTime min_value_date)
	{
		if ((month >= 1) && (month <= 12) && (min_value_date != null))
		{
			_min_value_date_by_month[month - 1] = new DateTime(min_value_date);
		}
		//checkDates();
	}

	/// <summary>
	/// Set the counter for missing data. </summary>
	/// <param name="month"> Month of interest (1-12). </param>
	/// <param name="missing_data_count"> The number of missing data in the time series. </param>
	public virtual void setMissingDataCount(int month, int missing_data_count)
	{
		if ((month >= 1) && (month <= 12))
		{
			_missing_data_count_by_month[month - 1] = missing_data_count;
		}
	}

	/// <summary>
	/// Set the counter for non-missing data. </summary>
	/// <param name="month"> Month of interest (1-12). </param>
	/// <param name="non_missing_data_count"> The number of non-missing data in the time series. </param>
	public virtual void setNonMissingDataCount(int month, int non_missing_data_count)
	{
		if ((month >= 1) && (month <= 12))
		{
			_non_missing_data_count_by_month[month - 1] = non_missing_data_count;
		}
	}

	/// <summary>
	/// Set the date for the first non-missing data value. </summary>
	/// <param name="month"> Month of interest (1-12). </param>
	/// <param name="date"> The date for the first non-missing data value. </param>
	public virtual void setNonMissingDataDate1(int month, DateTime date)
	{
		if ((date != null) && (month >= 1) && (month <= 12))
		{
			_non_missing_data_date1_by_month[month - 1] = new DateTime(date);
		}
		//checkDates();
	}

	/// <summary>
	/// Set the date for the last non-missing data value. </summary>
	/// <param name="month"> Month of interest (1-12). </param>
	/// <param name="date"> The date for the last non-missing data value. </param>
	public virtual void setNonMissingDataDate2(int month, DateTime date)
	{
		if ((date != null) && (month >= 1) && (month <= 12))
		{
			_non_missing_data_date2_by_month[month - 1] = new DateTime(date);
		}
		//checkDates();
	}

	/// <summary>
	/// Set the skew for data for the indicated month. </summary>
	/// <param name="month"> Month of interest (1-12). </param>
	/// <param name="skew"> The skew for the month. </param>
	public virtual void setSkew(int month, double skew)
	{
		if ((month >= 1) && (month <= 12))
		{
			__skewByMonth[month - 1] = skew;
		}
	}

	/// <summary>
	/// Set the standard deviation for data for the indicated month. </summary>
	/// <param name="month"> Month of interest (1-12). </param>
	/// <param name="stdDev"> The skew for the month. </param>
	public virtual void setStdDev(int month, double stdDev)
	{
		if ((month >= 1) && (month <= 12))
		{
			__stdDevByMonth[month - 1] = stdDev;
		}
	}

	/// <summary>
	/// Set the sum for data for the indicated month. </summary>
	/// <param name="month"> Month of interest (1-12). </param>
	/// <param name="sum"> The sum for the month. </param>
	public virtual void setSum(int month, double sum)
	{
		if ((month >= 1) && (month <= 12))
		{
			_sum_by_month[month - 1] = sum;
		}
	}

	/// <summary>
	/// Return a verbose string representation of the limits. </summary>
	/// <returns> A verbose string representation of the limits. </returns>
	public override string ToString()
	{
		string nl = System.getProperty("line.separator");

		StringBuilder buffer = new StringBuilder();
		if (_ts != null)
		{
			buffer.Append("Time series:  " + _ts.getIdentifierString() + " (" + getDataUnits() + ")" + nl);
		}
		buffer.Append("Monthly limits for period " + getDate1() + " to " + getDate2() + " are:" + nl + "                                                       #      %      # Not  % Not " + nl + "Month    Min    MinDate     Max    MaxDate     Sum     Miss.  Miss.  Miss.  Miss.     Mean  " + "    Median     StdDev      Skew" + nl + "--------------------------------------------------------------------------------------------" + "---------------------------------" + nl);
		string date_string = null;
		int num_values = 0; // Used for percents
		for (int i = 0; i < 12; i++)
		{
			// Get the individual data that may be null...
			// Now format the output line...
			buffer.Append(TimeUtil.monthAbbreviation(i + 1) + "  " + StringUtil.formatString(_min_value_by_month[i],"%10.1f") + " ");
			if (_min_value_date_by_month[i] == null)
			{
				date_string = "       ";
			}
			else
			{
				date_string = _min_value_date_by_month[i].ToString(DateTime.FORMAT_YYYY_MM);
			}
			buffer.Append(date_string + " ");
			buffer.Append(StringUtil.formatString(_max_value_by_month[i],"%10.1f") + " ");
			if (_max_value_date_by_month[i] == null)
			{
				date_string = "       ";
			}
			else
			{
				date_string = _max_value_date_by_month[i].ToString(DateTime.FORMAT_YYYY_MM);
			}
			buffer.Append(date_string + " ");
			buffer.Append(StringUtil.formatString(_sum_by_month[i],"%10.1f") + " " + StringUtil.formatString(_missing_data_count_by_month[i],"%6d") + " ");
			num_values = _missing_data_count_by_month[i] + _non_missing_data_count_by_month[i];
			if (num_values == 0)
			{
				buffer.Append(StringUtil.formatString(100.0,"%6.2f") + " ");
			}
			else
			{
				buffer.Append(StringUtil.formatString((100.0 * (double)_missing_data_count_by_month[i] / (double)num_values),"%6.2f") + " ");
			}
			buffer.Append(StringUtil.formatString(_non_missing_data_count_by_month[i],"%6d") + " ");
			if (num_values == 0)
			{
				buffer.Append(StringUtil.formatString(0.0,"%6.2f") + " ");
			}
			else
			{
				buffer.Append(StringUtil.formatString((100.0 * (double)_non_missing_data_count_by_month[i] / (double)num_values),"%6.2f") + " ");
			}
			buffer.Append(StringUtil.formatString(_mean_by_month[i],"%10.1f"));
			buffer.Append(StringUtil.formatString(__medianByMonth[i]," %10.1f"));
			buffer.Append(StringUtil.formatString(__stdDevByMonth[i]," %10.2f"));
			buffer.Append(StringUtil.formatString(__skewByMonth[i]," %10.4f") + nl);
		}
		buffer.Append("-----------------------------------------------------" + "------------------------------------------------------------------------" + nl);
		string mindate_string = "       ";
		string maxdate_string = "       ";
		if (getMinValueDate() != null)
		{
			mindate_string = getMinValueDate().ToString(DateTime.FORMAT_YYYY_MM);
		}
		if (getMaxValueDate() != null)
		{
			maxdate_string = getMaxValueDate().ToString(DateTime.FORMAT_YYYY_MM);
		}
		buffer.Append("Period" + StringUtil.formatString(getMinValue(),"%9.1f") + " " + mindate_string + " " + StringUtil.formatString(getMaxValue(),"%10.1f") + " " + maxdate_string + " " + StringUtil.formatString(getSum(),"%10.1f") + " " + StringUtil.formatString(getMissingDataCount(),"%6d") + " ");
		num_values = getMissingDataCount() + getNonMissingDataCount();
		if (num_values == 0)
		{
			buffer.Append(StringUtil.formatString(100.0,"%6.2f") + " ");
		}
		else
		{
			buffer.Append(StringUtil.formatString((100.0 * (double)getMissingDataCount() / (double)num_values),"%6.2f") + " ");
		}
		buffer.Append(StringUtil.formatString(getNonMissingDataCount(),"%6d") + " ");
		if (num_values == 0)
		{
			buffer.Append(StringUtil.formatString(0.0,"%6.2f") + " ");
		}
		else
		{
			buffer.Append(StringUtil.formatString((100.0 * (double)getNonMissingDataCount() / (double)num_values),"%6.2f") + " ");
		}
		buffer.Append(StringUtil.formatString(getMean(),"%10.1f"));
		buffer.Append(StringUtil.formatString(getMedian()," %10.1f"));
		buffer.Append(StringUtil.formatString(getStdDev()," %10.2f"));
		buffer.Append(StringUtil.formatString(getSkew()," %10.4f") + nl);
		buffer.Append("--------------------------------------------------------------------------------------------" + "---------------------------------" + nl);
		string r = buffer.ToString();
		return r;
	}

	}

}