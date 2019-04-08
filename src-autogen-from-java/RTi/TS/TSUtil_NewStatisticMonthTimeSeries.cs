using System;
using System.Collections.Generic;

// TSUtil_NewStatisticMonthTimeSeries - compute a MonthTS that has a statistic for each month in the period.

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

namespace RTi.TS
{

	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using DateTime = RTi.Util.Time.DateTime;
	using TimeInterval = RTi.Util.Time.TimeInterval;
	using TimeScaleType = RTi.Util.Time.TimeScaleType;
	using TimeUtil = RTi.Util.Time.TimeUtil;

	/// <summary>
	/// Compute a MonthTS that has a statistic for each month in the period.
	/// </summary>
	public class TSUtil_NewStatisticMonthTimeSeries
	{

	/// <summary>
	/// Enumerations used when processing the statistic.
	/// </summary>
	private enum TestType
	{
		GE, // Test >=
		GT, // Test >
		LE, // Test <=
		LT, // Test <
		NOT_USED
	} // Unknown test

	/// <summary>
	/// Time series to analyze.
	/// </summary>
	private TS __ts = null;

	/// <summary>
	/// New time series identifier to assign.
	/// </summary>
	private string __newTSID = null;

	/// <summary>
	/// Starting date/time for analysis.
	/// </summary>
	private DateTime __analysisStart = null;

	/// <summary>
	/// Ending date/time for analysis.
	/// </summary>
	private DateTime __analysisEnd = null;

	/// <summary>
	/// Statistic to analyze.
	/// </summary>
	private TSStatisticType __statisticType = null;

	/// <summary>
	/// Test value used when analyzing the statistic.
	/// </summary>
	private double? __testValue = null;

	/// <summary>
	/// Monthly test values used when analyzing the statistic.
	/// </summary>
	private double?[] __monthTestValues = null;

	/// <summary>
	/// Number of missing allowed to compute sample (default is no limit).
	/// </summary>
	private int __allowMissingCount = -1;

	/// <summary>
	/// Minimum required sample size (default is no limit).
	/// </summary>
	private int __minimumSampleSize = -1;

	/// <summary>
	/// Starting date/time for analysis window, within a month.
	/// </summary>
	private DateTime __analysisWindowStart = null;

	/// <summary>
	/// Ending date/time for analysis window, within a month.
	/// </summary>
	private DateTime __analysisWindowEnd = null;

	/// <summary>
	/// Search start date/time for analysis window, within a month.
	/// </summary>
	private DateTime __searchStart = null;

	/// <summary>
	/// Construct the analysis object with required input.  Values will be checked for validity.
	/// Execute the newStatisticMonthTS() method to perform the analysis. </summary>
	/// <param name="ts"> time series to analyze </param>
	/// <param name="analysisStart"> Starting date/time for analysis, in precision of the original data. </param>
	/// <param name="analysisEnd"> Ending date for analysis, in precision of the original data. </param>
	/// <param name="newTSID"> the new time series identifier to be assigned to the time series. </param>
	/// <param name="statisticType"> the statistic type for the output time series. </param>
	/// <param name="testValue"> test value (e.g., threshold value) needed to process some statistics. </param>
	/// <param name="monthTestValues"> monthly test values </param>
	/// <param name="allowMissingCount"> the number of values allowed to be missing in the sample. </param>
	/// <param name="minimumSampleSize"> the minimum sample size to allow to compute the statistic. </param>
	/// <param name="analysisWindowStart"> Starting date/time (year and month are ignored) for analysis within the month,
	/// in precision of the original data.  If null, the entire year of data will be analyzed. </param>
	/// <param name="analysisWindowEnd"> Ending date (year and month are ignored) for analysis within the month,
	/// in precision of the original data.  If null, the entire year of data will be analyzed. </param>
	/// <param name="searchStart"> search start date (year and month are ignored) for analysis within the month,
	/// in precision of the original data.  If null, the entire month of data will be analyzed.
	/// This is used when a starting point is needed, such as when first and last values >, < in a month. </param>
	public TSUtil_NewStatisticMonthTimeSeries(TS ts, string newTSID, TSStatisticType statisticType, double? testValue, Double[] monthTestValues, int? allowMissingCount, int? minimumSampleSize, DateTime analysisStart, DateTime analysisEnd, DateTime analysisWindowStart, DateTime analysisWindowEnd, DateTime searchStart)
	{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		string routine = this.GetType().FullName;
		string message;

		if (ts == null)
		{
			// Nothing to do...
			message = "Null input time series - cannot calculate statistic time series.";
			Message.printWarning(3, routine, message);
			throw new InvalidParameterException(message);
		}
		// TODO SAM 2014-04-03 Enable for other than daily time series
		if (ts.getDataIntervalBase() != TimeInterval.DAY)
		{
			message = "Input time series must have an interval of day (support for additional intervals will be added in the future).";
			Message.printWarning(3, routine, message);
			throw new InvalidParameterException(message);
		}
		setTimeSeries(ts);
		setNewTSID(newTSID);
		try
		{
			if ((!string.ReferenceEquals(newTSID, null)) && (newTSID.Length > 0))
			{
				TSIdent tsident = new TSIdent(newTSID);
				// Make sure that the output interval is Month
				if (!tsident.getInterval().Equals("Month", StringComparison.OrdinalIgnoreCase))
				{
					throw new System.ArgumentException("New time series identifier \"" + newTSID + "\" must have an interval of Month - " + "cannot calculate statistic time series.");
				}
			}
		}
		catch (Exception e)
		{
			throw new System.ArgumentException("New time series identifier \"" + newTSID + "\" is invalid (" + e + ").");
		}
		setStatisticType(statisticType);
		setTestValue(testValue);
		setMonthTestValues(monthTestValues);
		if (allowMissingCount == null)
		{
			allowMissingCount = new int?(-1); // default
		}
		setAllowMissingCount(allowMissingCount.Value);
		if (minimumSampleSize == null)
		{
			minimumSampleSize = new int?(-1); // default
		}
		setMinimumSampleSize(minimumSampleSize.Value);
		setAnalysisStart(analysisStart);
		setAnalysisEnd(analysisEnd);

		// FIXME SAM 2009-11-04 Need to make this check specific to the time series interval and time scale
		if (!isStatisticSupported(statisticType, TimeInterval.UNKNOWN, null))
		{
			throw new InvalidParameterException("Statistic \"" + statisticType + "\" is not supported.");
		}

		setAnalysisWindowStart(analysisWindowStart);
		setAnalysisWindowEnd(analysisWindowEnd);
		setSearchStart(searchStart);
	}

	/// <summary>
	/// Process a time series to create a statistic from getStatisticChoices(). </summary>
	/// <param name="ts"> Time series to analyze. </param>
	/// <param name="monthts"> MonthTS to fill with the statistic. </param>
	/// <param name="statisticType"> statistic to calculate. </param>
	/// <param name="testValue"> a number to test against for some statistics (e.g., COUNT_LE). </param>
	/// <param name="monthTestValues"> monthly test values that should be used instead of a single test value </param>
	/// <param name="analysisStart"> Start of the analysis (precision matching ts). </param>
	/// <param name="analysisEnd"> End of the analysis (precision matching ts). </param>
	/// <param name="allowMissingCount"> the number of missing values allowed in input and still compute the statistic. </param>
	/// <param name="minimumSampleSize"> the minimum number of values required in input to compute the statistic. </param>
	/// <param name="analysisWindowStart"> If not null, specify the start of the window within
	/// the month for data, , for example an operational constraint.
	/// Currently only Month... to precision are evaluated (not day... etc.). </param>
	/// <param name="analysisWindowEnd"> If not null, specify the end of the window within
	/// the month for data, , for example an operational constraint.
	/// Currently only Month... to precision are evaluated (not day... etc.). </param>
	/// <param name="searchStart"> starting date/time in the month to analyze, for example an operational constraint. </param>
	private void calculateStatistic(TS ts, MonthTS monthts, TSStatisticType statisticType, double? testValue, Double[] monthTestValues, DateTime analysisStart, DateTime analysisEnd, int allowMissingCount, int minimumSampleSize, DateTime analysisWindowStart, DateTime analysisWindowEnd, DateTime searchStart)
	{
		string routine = this.GetType().Name + ".calculateStatistic";
		// Initialize the settings to evaluate the statistic and set appropriate information in the time series
		bool statisticIsCount = isStatisticCount(statisticType);
		bool statisticIsDayOf = isStatisticDayOf(statisticType);
		bool statisticIsFirst = isStatisticFirst(statisticType);
		bool statisticIsLast = isStatisticLast(statisticType);
		bool iterateForward = isStatisticIterateForward(statisticType);
		TestType testType = getStatisticTestType(statisticType);
		monthts.setDescription(getStatisticTimeSeriesDescription(statisticType, testType, testValue, monthTestValues, statisticIsCount, statisticIsDayOf, statisticIsFirst, statisticIsLast));
		monthts.setDataUnits(getStatisticTimeSeriesDataUnits(statisticType, statisticIsCount, statisticIsDayOf, ts.getDataUnits()));

		//Message.printStatus ( 2, routine, "Overall analysis period is " + analysisStart + " to " + analysisEnd );

		double testValueDouble = Double.NaN; // OK to initialize to this because checks will have verified real value
		if (isTestValueNeeded(statisticType))
		{
			if (testValue != null)
			{
				testValueDouble = testValue.Value;
			}
		}
		// For debugging...
		//Message.printStatus(2,routine,"StatisticType=" + statisticType + " TestType=" + testType +
		//    " testValueDouble=" + testValueDouble );

		double value; // The value in the original time series
		// Dates for iteration in a year, adjusted for the analysis window
		// Use these internal to the loop below so as to not interfere with the full-year iteration
		//DateTime yearStartForAnalysisWindow = null;
		//DateTime yearEndForAnalysisWindow = null;
		// Dates for the start and end of the month worth of data
		DateTime date = new DateTime(ts.getDate1(),DateTime.DATE_FAST), dataStart = new DateTime(ts.getDate1(),DateTime.DATE_FAST), dataEnd = new DateTime(ts.getDate2(),DateTime.DATE_FAST);
		// Loop through the monthly analysis period.  The sample will be retrieved for the month
		for (DateTime monthDate = new DateTime(analysisStart); monthDate.lessThanOrEqualTo(analysisEnd); monthDate.addMonth(1))
		{
			// Initialize for the new month
			double sum = 0.0;
			int nMissing = 0;
			int nNotMissing = 0;
			TSData data; // Data from the input time series
			// Initialize the value of the statistic in the year to missing.  Do this each time an output month is processed.
			double monthValue = monthts.getMissing(); // Data value for output time series
			double dayMoment = 0.0; // Day of month * value, for DayOfCentroid statistic
			double extremeValue = monthts.getMissing(); // Used for DayOfMin, etc., where check and day are tracked.
			// Dates bounding month, with precision of the input time series
			// TODO SAM 2014-04-03 Need to enable other than daily input.  Need to handle offset if odd hour, etc.
			dataStart.setDate(monthDate);
			dataStart.setDay(1);
			dataStart.setHour(0);
			dataStart.setMinute(0);
			dataEnd.setDate(monthDate);
			dataEnd.setDay(TimeUtil.numDaysInMonth(monthDate));
			TSIterator tsi = null;
			try
			{
				tsi = ts.iterator(dataStart,dataEnd);
			}
			catch (Exception e)
			{
				throw new Exception("Exception initializing time series iterator for month's data period " + dataStart + " to " + dataEnd + " uniquetempvar.");
			}
			bool doneAnalyzing = false; // If true, there is no more data or a statistic is complete
			while (true)
			{
				if (iterateForward)
				{
					// First call will initialize and return first point.
					data = tsi.next();
				}
				else
				{
					// First call will initialize and return last point.
					data = tsi.previous();
				}
				if (data != null)
				{
					// Still analyzing data in the analysis window
					date = tsi.getDate();
					if (monthTestValues != null)
					{
						// Test value varies by month
						testValueDouble = monthTestValues[date.getMonth() - 1];
					}
					value = tsi.getDataValue();
					// For debugging...
					if (Message.isDebugOn)
					{
						Message.printDebug(10, routine, "Data value on " + date + " is " + value);
					}
					if (ts.isDataMissing(value))
					{
						// Data value is missing so increment the counter and continue to the next value.
						// "data == null" will be true even for missing values and therefore when data
						// run out, statistics that depend on the missing count will be processed correctly below.
						++nMissing;
						continue;
					}
					else
					{
						// Data value is not missing so process below.
						++nNotMissing;
						// Always increment the sum because it is easy to compute and is needed by
						// some statistics...
						sum += value;
						// Evaluate the test and/or statistic type to compute the value to be assigned to
						// the year.  In some cases this is a single value and then done.  In other cases
						// the value gets updated as more values are examined.
						// The year value is examined to see if it is missing in order to know whether to
						// initialize or update the value.
						// If evaluating statistics like FIRST_DAY_GE, missing data will accumulate prior
						// to the non-missing value, and the first non-missing value will cause further
						// evaluation of input data to be skipped.  The determination on whether missing
						// data are an issue is made when setting the yearly value at the end of the year loop.
						if ((testType == TestType.GE) && (value >= testValueDouble))
						{
							if ((statisticType == TSStatisticType.GE_COUNT) || (statisticType == TSStatisticType.GE_PERCENT))
							{
								if (monthts.isDataMissing(monthValue))
								{
									monthValue = 1.0;
								}
								else
								{
									monthValue += 1.0;
								}
							}
							else if ((statisticType == TSStatisticType.DAY_OF_FIRST_GE) || (statisticType == TSStatisticType.DAY_OF_LAST_GE))
							{
								monthValue = date.getDay();
								doneAnalyzing = true; // Found value for the month.
							}
						}
						else if ((testType == TestType.GT) && (value > testValueDouble))
						{
							if ((statisticType == TSStatisticType.GT_COUNT) || (statisticType == TSStatisticType.GT_PERCENT))
							{
								if (monthts.isDataMissing(monthValue))
								{
									monthValue = 1.0;
								}
								else
								{
									monthValue += 1.0;
								}
							}
							else if ((statisticType == TSStatisticType.DAY_OF_FIRST_GT) || (statisticType == TSStatisticType.DAY_OF_LAST_GT))
							{
								monthValue = date.getDay();
								doneAnalyzing = true; // Found value for the year.
							}
						}
						else if ((testType == TestType.LE) && (value <= testValueDouble))
						{
							//Message.printStatus(2, routine, "Found value " + value + " <= " +
							//    testValueDouble + " on " + date );
							if ((statisticType == TSStatisticType.LE_COUNT) || (statisticType == TSStatisticType.LE_PERCENT))
							{
								if (monthts.isDataMissing(monthValue))
								{
									monthValue = 1.0;
								}
								else
								{
									monthValue += 1.0;
								}
							}
							else if ((statisticType == TSStatisticType.DAY_OF_FIRST_LE) || (statisticType == TSStatisticType.DAY_OF_LAST_LE))
							{
								monthValue = date.getDay();
								doneAnalyzing = true; // Found value for the year.
							}
						}
						else if ((testType == TestType.LT) && (value < testValueDouble))
						{
							if ((statisticType == TSStatisticType.LT_COUNT) || (statisticType == TSStatisticType.LT_PERCENT))
							{
								if (monthts.isDataMissing(monthValue))
								{
									monthValue = 1.0;
								}
								else
								{
									monthValue += 1.0;
								}
							}
							else if ((statisticType == TSStatisticType.DAY_OF_FIRST_LT) || (statisticType == TSStatisticType.DAY_OF_LAST_LT))
							{
								monthValue = date.getDay();
								doneAnalyzing = true; // Found value for the year.
							}
						}
						else if ((statisticType == TSStatisticType.DAY_OF_CENTROID))
						{
							dayMoment += date.getDay() * value;
						}
						else if ((statisticType == TSStatisticType.DAY_OF_MAX) || (statisticType == TSStatisticType.MAX))
						{
							if (monthts.isDataMissing(extremeValue) || (value > extremeValue))
							{
								// Set the max...
								if (statisticType == TSStatisticType.DAY_OF_MAX)
								{
									extremeValue = value;
									monthValue = date.getDay();
								}
								else if (statisticType == TSStatisticType.MAX)
								{
									extremeValue = value;
									monthValue = value;
								}
							}
							// Need to continue analyzing period so do not set doneAnalyzing to false.
						}
						else if ((statisticType == TSStatisticType.DAY_OF_MIN) || (statisticType == TSStatisticType.MIN))
						{
							if (monthts.isDataMissing(extremeValue) || (value < extremeValue))
							{
								//Message.printStatus(2,routine,"Found new min " + value + " on " + date );
								// Set the min...
								if (statisticType == TSStatisticType.DAY_OF_MIN)
								{
									extremeValue = value;
									monthValue = date.getDay();
								}
								else if (statisticType == TSStatisticType.MIN)
								{
									extremeValue = value;
									monthValue = value;
								}
							}
							// Need to continue analyzing period so do not set doneAnalyzing to false.
						}
						else if ((statisticType == TSStatisticType.MEAN) || (statisticType == TSStatisticType.TOTAL))
						{
							// Need to accumulate the value (for Mean or Total)
							// Accumulate into the year_value
							if (monthts.isDataMissing(monthValue))
							{
								monthValue = value;
							}
							else
							{
								monthValue += value;
							}
						}
					}
				}
				else
				{
					// End of the data.  Compute the statistic and assign for the year
					doneAnalyzing = true;
				}
				if (doneAnalyzing)
				{
					// Save the results
					//Message.printStatus(2, routine, "For " + monthDate + ", sum=" + sum + ", nMissing=" +
					//    nMissing + ", nNotMissing=" + nNotMissing );
					if (statisticType == TSStatisticType.DAY_OF_CENTROID)
					{
						if ((nNotMissing > 0) && okToSetMonthStatistic(nMissing, nNotMissing, allowMissingCount, minimumSampleSize))
						{
							monthts.setDataValue(monthDate, dayMoment / sum);
						}
					}
					else if (statisticType == TSStatisticType.MEAN)
					{
						if ((nNotMissing > 0) && okToSetMonthStatistic(nMissing, nNotMissing, allowMissingCount, minimumSampleSize))
						{
							monthts.setDataValue(monthDate, sum / nNotMissing);
						}
					}
					else if (statisticType == TSStatisticType.MISSING_COUNT)
					{
						// Always assign
						monthts.setDataValue(monthDate, (double)nMissing);
					}
					else if (statisticType == TSStatisticType.MISSING_PERCENT)
					{
						// Always assign
						if ((nMissing + nNotMissing) > 0)
						{
							monthts.setDataValue(monthDate, 100.0 * (double)nMissing / (double)(nMissing + nNotMissing));
						}
					}
					else if (statisticType == TSStatisticType.NONMISSING_COUNT)
					{
						// Always assign
						monthts.setDataValue(monthDate, (double)nNotMissing);
					}
					else if (statisticType == TSStatisticType.NONMISSING_PERCENT)
					{
						// Always assign
						if ((nMissing + nNotMissing) > 0)
						{
							monthts.setDataValue(monthDate, 100.0 * (double)nNotMissing / (double)(nMissing + nNotMissing));
						}
					}
					else if (statisticType == TSStatisticType.TOTAL)
					{
						if ((nNotMissing > 0) && okToSetMonthStatistic(nMissing, nNotMissing, allowMissingCount, minimumSampleSize))
						{
							monthts.setDataValue(monthDate, sum);
						}
					}
					else
					{
						//Message.printStatus(2,routine,"Year " + monthDate + " value=" + monthValue );
						if (!monthts.isDataMissing(monthValue) && okToSetMonthStatistic(nMissing, nNotMissing, allowMissingCount, minimumSampleSize))
						{
							// No additional processing is needed.
							if ((statisticType == TSStatisticType.GE_PERCENT) || (statisticType == TSStatisticType.GT_PERCENT) || (statisticType == TSStatisticType.LE_PERCENT) || (statisticType == TSStatisticType.LT_PERCENT))
							{
								if ((nMissing + nNotMissing) > 0)
								{
									// Note that these statistics are for total number of points (not just
									// non-missing points).
									monthts.setDataValue(monthDate, 100.0 * monthValue / (nMissing + nNotMissing));
								}
							}
							else
							{
								// Statistic has already been calculated.
								monthts.setDataValue(monthDate, monthValue);
							}
						}
					}
					break; // Will go to the next month of input, with new iterator
				}
			}
		}
	}

	/// <summary>
	/// Return the number of missing values allowed in sample. </summary>
	/// <returns> the number of missing values allowed in sample. </returns>
	private int? getAllowMissingCount()
	{
		return __allowMissingCount;
	}

	/// <summary>
	/// Return the analysis end date/time. </summary>
	/// <returns> the analysis end date/time. </returns>
	private DateTime getAnalysisEnd()
	{
		return __analysisEnd;
	}

	/// <summary>
	/// Return the analysis start date/time. </summary>
	/// <returns> the analysis start date/time. </returns>
	private DateTime getAnalysisStart()
	{
		return __analysisStart;
	}

	/// <summary>
	/// Return the analysis window end date/time. </summary>
	/// <returns> the analysis window end date/time. </returns>
	private DateTime getAnalysisWindowEnd()
	{
		return __analysisWindowEnd;
	}

	/// <summary>
	/// Return the analysis window start date/time. </summary>
	/// <returns> the analysis window start date/time. </returns>
	private DateTime getAnalysisWindowStart()
	{
		return __analysisWindowStart;
	}

	/// <summary>
	/// Return the minimum sample size allowed to compute the statistic. </summary>
	/// <returns> the minimum sample size allowed to compute the statistic. </returns>
	private int? getMinimumSampleSize()
	{
		return __minimumSampleSize;
	}

	/// <summary>
	/// Return the monthly test values used to calculate some statistics. </summary>
	/// <returns> the monthly test values used to calculate some statistics. </returns>
	private double? [] getMonthTestValues()
	{
		return __monthTestValues;
	}

	/// <summary>
	/// Return the time series identifier for the new time series. </summary>
	/// <returns> the time series identifier for the new time series. </returns>
	private string getNewTSID()
	{
		return __newTSID;
	}

	/// <summary>
	/// Return the search start date/time. </summary>
	/// <returns> the search start date/time. </returns>
	private DateTime getSearchStart()
	{
		return __searchStart;
	}

	/// <summary>
	/// Return a list of statistic choices for the requested input time series interval and scale.
	/// These strings are suitable for listing in a user interface.  The statistics are
	/// listed in ascending alphabetical order.  Parameters can be used to limit the
	/// choices (these features will be phased in over time as statistics are added). </summary>
	/// <param name="interval"> TimeInterval.DAY, etc., indicating the interval of data for the
	/// statistic (e.g., average value for the year).  Pass TimeInterval.UNKNOWN to get all choices. </param>
	/// <param name="timescale"> indicates whether the statistic is
	/// expected on accumulated, mean, instantaneous data.  Pass null to get all choices.  CURRENTLY NOT USED. </param>
	public static IList<TSStatisticType> getStatisticChoicesForInterval(int interval, TimeScaleType timescale)
	{
		IList<TSStatisticType> statistics = new List<TSStatisticType>();
		// Add in alphabetical order, splitting up by interval as appropriate.
		// Daily or finer...
		if ((interval <= TimeInterval.DAY) || (interval == TimeInterval.UNKNOWN))
		{
			statistics.Add(TSStatisticType.DAY_OF_CENTROID);
			statistics.Add(TSStatisticType.DAY_OF_FIRST_GE);
			statistics.Add(TSStatisticType.DAY_OF_FIRST_GT);
			statistics.Add(TSStatisticType.DAY_OF_FIRST_LE);
			statistics.Add(TSStatisticType.DAY_OF_FIRST_LT);
			statistics.Add(TSStatisticType.DAY_OF_LAST_GE);
			statistics.Add(TSStatisticType.DAY_OF_LAST_GT);
			statistics.Add(TSStatisticType.DAY_OF_LAST_LE);
			statistics.Add(TSStatisticType.DAY_OF_LAST_LT);
			statistics.Add(TSStatisticType.DAY_OF_MAX);
			statistics.Add(TSStatisticType.DAY_OF_MIN);
		}
		// All intervals...
		statistics.Add(TSStatisticType.GE_COUNT);
		statistics.Add(TSStatisticType.GE_PERCENT);
		statistics.Add(TSStatisticType.GT_COUNT);
		statistics.Add(TSStatisticType.GT_PERCENT);
		statistics.Add(TSStatisticType.LE_COUNT);
		statistics.Add(TSStatisticType.LE_PERCENT);
		statistics.Add(TSStatisticType.LT_COUNT);
		statistics.Add(TSStatisticType.LT_PERCENT);
		statistics.Add(TSStatisticType.MAX);
		// TODO SAM 2009-10-14 Need to support median
		//statistics.add ( TSStatisticType.MEDIAN );
		statistics.Add(TSStatisticType.MEAN);
		statistics.Add(TSStatisticType.MIN);
		statistics.Add(TSStatisticType.MISSING_COUNT);
		statistics.Add(TSStatisticType.MISSING_PERCENT);
		statistics.Add(TSStatisticType.NONMISSING_COUNT);
		statistics.Add(TSStatisticType.NONMISSING_PERCENT);
		statistics.Add(TSStatisticType.TOTAL);
		return statistics;
	}

	/// <summary>
	/// Return a list of statistic choices for the requested interval and scale.
	/// These strings are suitable for listing in a user interface.  The statistics are
	/// listed in ascending alphabetical order.  Parameters can be used to limit the
	/// choices (these features will be phased in over time as statistics are added). </summary>
	/// <param name="interval"> TimeInterval.DAY, etc., indicating the interval of data for the
	/// statistic (e.g., average value for the year).  Pass TimeInterval.UNKNOWN to get all choices. </param>
	/// <param name="timescale"> MeasTimeScale.ACCM, etc., indicating whether the statistic is
	/// expected on accumulated, mean, instantaneous data.  Pass null to get all choices.  CURRENTLY NOT USED. </param>
	public static IList<string> getStatisticChoicesForIntervalAsStrings(int interval, TimeScaleType timescale)
	{
		IList<TSStatisticType> choices = getStatisticChoicesForInterval(interval, timescale);
		IList<string> stringChoices = new List<string>();
		for (int i = 0; i < choices.Count; i++)
		{
			stringChoices.Add("" + choices[i]);
		}
		return stringChoices;
	}

	/// <summary>
	/// Determine the statistic test type, when comparing against a test value. </summary>
	/// <param name="statisticType"> a statistic type to check. </param>
	/// <returns> the test type for the statistic or NOT_USED if the test is not used for a statistic. </returns>
	private TestType getStatisticTestType(TSStatisticType statisticType)
	{
		if ((statisticType == TSStatisticType.GE_COUNT) || (statisticType == TSStatisticType.GE_PERCENT) || (statisticType == TSStatisticType.DAY_OF_FIRST_GE) || (statisticType == TSStatisticType.DAY_OF_LAST_GE))
		{
			return TestType.GE;
		}
		else if ((statisticType == TSStatisticType.GT_COUNT) || (statisticType == TSStatisticType.GT_PERCENT) || (statisticType == TSStatisticType.DAY_OF_FIRST_GT) || (statisticType == TSStatisticType.DAY_OF_LAST_GT))
		{
			return TestType.GT;
		}
		else if ((statisticType == TSStatisticType.LE_COUNT) || (statisticType == TSStatisticType.LE_PERCENT) || (statisticType == TSStatisticType.DAY_OF_FIRST_LE) || (statisticType == TSStatisticType.DAY_OF_LAST_LE))
		{
			return TestType.LE;
		}
		else if ((statisticType == TSStatisticType.LT_COUNT) || (statisticType == TSStatisticType.LT_PERCENT) || (statisticType == TSStatisticType.DAY_OF_FIRST_LT) || (statisticType == TSStatisticType.DAY_OF_LAST_LT))
		{
			return TestType.LT;
		}
		else
		{
			return TestType.NOT_USED;
		}
	}

	/// <summary>
	/// Determine the statistic time series data units. </summary>
	/// <param name="statisticType"> a statistic type to check. </param>
	/// <param name="testType"> test type that is being performed. </param>
	/// <param name="testValue"> the test value to be checked. </param>
	/// <returns> the description for the time series, given the statistic and test types. </returns>
	private string getStatisticTimeSeriesDataUnits(TSStatisticType statisticType, bool statisticIsCount, bool statisticIsDayOf, string tsUnits)
	{
		if (statisticIsCount)
		{
			if ((statisticType == TSStatisticType.GE_PERCENT) || (statisticType == TSStatisticType.GT_PERCENT) || (statisticType == TSStatisticType.LE_PERCENT) || (statisticType == TSStatisticType.LT_PERCENT) || (statisticType == TSStatisticType.MISSING_PERCENT) || (statisticType == TSStatisticType.NONMISSING_PERCENT))
			{
				return "Percent";
			}
			else
			{
				return "Count";
			}
		}
		else if (statisticIsDayOf)
		{
			return "DayOfMonth";
		}
		else
		{
			return tsUnits;
		}
	}

	/// <summary>
	/// Determine the statistic time series description. </summary>
	/// <param name="statisticType"> a statistic type to check. </param>
	/// <param name="testType"> test type that is being performed. </param>
	/// <param name="testValue"> the test value to be checked. </param>
	/// <returns> the description for the time series, given the statistic and test types. </returns>
	private string getStatisticTimeSeriesDescription(TSStatisticType statisticType, TestType testType, double? testValue, Double[] monthTestValues, bool statisticIsCount, bool statisticIsDayOf, bool statisticIsFirst, bool statisticIsLast)
	{
		string testString = "?test?";
		string testValueString = "?testValue?";
		string desc = "?";
		if (testValue != null)
		{
			testValueString = StringUtil.formatString(testValue.Value,"%.6f");
		}
		else if (monthTestValues != null)
		{
			testValueString = "monthly values";
		}
		if (testType == TestType.GE)
		{
			testString = ">=";
		}
		else if (testType == TestType.GT)
		{
			testString = ">";
		}
		else if (testType == TestType.LE)
		{
			testString = "<=";
		}
		else if (testType == TestType.LT)
		{
			testString = "<";
		}
		if (statisticIsCount)
		{
			if (statisticType == TSStatisticType.MISSING_COUNT)
			{
				desc = "Count of missing values";
			}
			else if (statisticType == TSStatisticType.NONMISSING_COUNT)
			{
				desc = "Count of nonmissing values";
			}
			else
			{
				desc = "Count of values " + testString + " " + testValueString;
			}
		}
		else if (statisticIsDayOf)
		{
			if (statisticIsFirst)
			{
				desc = "Day of month for first value " + testString + " " + testValueString;
			}
			else if (statisticIsLast)
			{
				desc = "Day of month for last value " + testString + " " + testValueString;
			}
			else if (statisticType == TSStatisticType.DAY_OF_CENTROID)
			{
				desc = "Day of month for centroid";
			}
			else if (statisticType == TSStatisticType.DAY_OF_MAX)
			{
				desc = "Day of month for maximum value";
			}
			else if (statisticType == TSStatisticType.DAY_OF_MIN)
			{
				desc = "Day of month for minimum value";
			}
		}
		else
		{
			// MAX, MEAN, etc.
			desc = "" + statisticType;
		}
		// If not set will fall through to default
		return desc;
	}

	/// <summary>
	/// Return the name of the statistic being calculated. </summary>
	/// <returns> the name of the statistic being calculated. </returns>
	public virtual TSStatisticType getStatisticType()
	{
		return __statisticType;
	}

	/// <summary>
	/// Return the test value used to calculate some statistics. </summary>
	/// <returns> the test value used to calculate some statistics. </returns>
	private double? getTestValue()
	{
		return __testValue;
	}

	/// <summary>
	/// Return the time series being analyzed. </summary>
	/// <returns> the time series being analyzed. </returns>
	public virtual TS getTimeSeries()
	{
		return __ts;
	}

	/// <summary>
	/// Indicate whether the statistic is a count.
	/// Percents are handled as counts initially and are then converted to percent as the last assignment </summary>
	/// <param name="statisticType"> a statistic type to check. </param>
	private bool isStatisticCount(TSStatisticType statisticType)
	{
		if ((statisticType == TSStatisticType.GE_COUNT) || (statisticType == TSStatisticType.GE_PERCENT) || (statisticType == TSStatisticType.GT_COUNT) || (statisticType == TSStatisticType.GT_PERCENT) || (statisticType == TSStatisticType.LE_COUNT) || (statisticType == TSStatisticType.LE_PERCENT) || (statisticType == TSStatisticType.LT_COUNT) || (statisticType == TSStatisticType.LT_PERCENT) || (statisticType == TSStatisticType.MISSING_COUNT) || (statisticType == TSStatisticType.NONMISSING_COUNT))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Indicate whether the statistic is a "DayOf" statistic. </summary>
	/// <param name="statisticType"> a statistic type to check. </param>
	private bool isStatisticDayOf(TSStatisticType statisticType)
	{
		if ((statisticType == TSStatisticType.DAY_OF_CENTROID) || (statisticType == TSStatisticType.DAY_OF_FIRST_GE) || (statisticType == TSStatisticType.DAY_OF_FIRST_GT) || (statisticType == TSStatisticType.DAY_OF_FIRST_LE) || (statisticType == TSStatisticType.DAY_OF_FIRST_LT) || (statisticType == TSStatisticType.DAY_OF_LAST_GE) || (statisticType == TSStatisticType.DAY_OF_LAST_GT) || (statisticType == TSStatisticType.DAY_OF_LAST_LE) || (statisticType == TSStatisticType.DAY_OF_LAST_LT) || (statisticType == TSStatisticType.DAY_OF_MAX) || (statisticType == TSStatisticType.DAY_OF_MIN))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Indicate whether the statistic is a "First" statistic. </summary>
	/// <param name="statisticType"> a statistic type to check. </param>
	private bool isStatisticFirst(TSStatisticType statisticType)
	{
		if ((statisticType == TSStatisticType.DAY_OF_FIRST_GE) || (statisticType == TSStatisticType.DAY_OF_FIRST_GT) || (statisticType == TSStatisticType.DAY_OF_FIRST_LE) || (statisticType == TSStatisticType.DAY_OF_FIRST_LT))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Indicate whether the statistic is a "Last" statistic. </summary>
	/// <param name="statisticType"> a statistic type to check. </param>
	private bool isStatisticLast(TSStatisticType statisticType)
	{
		if ((statisticType == TSStatisticType.DAY_OF_LAST_GE) || (statisticType == TSStatisticType.DAY_OF_LAST_GT) || (statisticType == TSStatisticType.DAY_OF_LAST_LE) || (statisticType == TSStatisticType.DAY_OF_LAST_LT))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Indicate whether the statistic is determined by iterating forward or backward. </summary>
	/// <param name="statisticType"> a statistic type to check. </param>
	/// <returns> true if the statistic is determined by iterating forward, false if iterating backward. </returns>
	private bool isStatisticIterateForward(TSStatisticType statisticType)
	{
		if ((statisticType == TSStatisticType.DAY_OF_LAST_GE) || (statisticType == TSStatisticType.DAY_OF_LAST_GT) || (statisticType == TSStatisticType.DAY_OF_LAST_LE) || (statisticType == TSStatisticType.DAY_OF_LAST_LT))
		{
			// Iterate backward
			return false;
		}
		else
		{
			return true; // By default most statistics are determined by iterating forward
		}
	}

	/// <summary>
	/// Indicate whether a statistic is supported. </summary>
	/// <param name="statisticType"> a statistic type to check. </param>
	/// <param name="interval"> time interval base to check, or TimeInterval.UNKNOWN if interval is not to be considered. </param>
	/// <param name="timeScale"> time scale to check, or null if not considered. </param>
	public static bool isStatisticSupported(TSStatisticType statisticType, int interval, TimeScaleType timeScale)
	{
		IList<TSStatisticType> choices = getStatisticChoicesForInterval(interval, timeScale);
		for (int i = 0; i < choices.Count; i++)
		{
			if (choices[i] == statisticType)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Indicate whether the statistic requires that a test value be supplied. </summary>
	/// <param name="statisticType"> a statistic type to check. </param>
	public static bool isTestValueNeeded(TSStatisticType statisticType)
	{
		if ((statisticType == TSStatisticType.DAY_OF_FIRST_GE) || (statisticType == TSStatisticType.DAY_OF_FIRST_GT) || (statisticType == TSStatisticType.DAY_OF_FIRST_LE) || (statisticType == TSStatisticType.DAY_OF_FIRST_LT) || (statisticType == TSStatisticType.DAY_OF_LAST_GE) || (statisticType == TSStatisticType.DAY_OF_LAST_GT) || (statisticType == TSStatisticType.DAY_OF_LAST_LE) || (statisticType == TSStatisticType.DAY_OF_LAST_LT) || (statisticType == TSStatisticType.GE_COUNT) || (statisticType == TSStatisticType.GE_PERCENT) || (statisticType == TSStatisticType.GT_COUNT) || (statisticType == TSStatisticType.GT_PERCENT) || (statisticType == TSStatisticType.LE_COUNT) || (statisticType == TSStatisticType.LE_PERCENT) || (statisticType == TSStatisticType.LT_COUNT) || (statisticType == TSStatisticType.LT_PERCENT))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Create a year time series that contains statistics in each data value (e.g.,
	/// percent missing, percent not missing). </summary>
	/// <param name="createData"> if true, calculate the data value array; if false, only assign metadata </param>
	/// <returns> The statistics time series. </returns>
	public virtual MonthTS newStatisticMonthTS(bool createData)
	{
		string message, routine = this.GetType().Name + ".newStatisticMonthTS";
		int dl = 10;

		// Get the data needed for the analysis - originally provided in the constructor

		TS ts = getTimeSeries();
		string newTSID = getNewTSID();
		TSStatisticType statisticType = getStatisticType();
		double? testValue = getTestValue();
		double?[] monthTestValues = getMonthTestValues();
		int? allowMissingCount = getAllowMissingCount();
		int? minimumSampleSize = getMinimumSampleSize();
		DateTime analysisStart = getAnalysisStart();
		DateTime analysisEnd = getAnalysisEnd();
		DateTime analysisWindowStart = getAnalysisWindowStart();
		DateTime analysisWindowEnd = getAnalysisWindowEnd();
		DateTime searchStart = getSearchStart();

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Trying to create statistic month TS for \"" + ts.getIdentifierString() + "\"");
		}

		// Get valid dates for the output time series because the ones passed in may have been null...

		TSLimits valid_dates = TSUtil.getValidPeriod(ts, analysisStart, analysisEnd);
		// Reset because these are handled generically below whether passed in or defaulted to "ts"
		analysisStart = new DateTime(valid_dates.getDate1());
		analysisEnd = new DateTime(valid_dates.getDate2());

		// Create a month time series to be calculated...

		MonthTS monthts = new MonthTS();
		monthts.addToGenesis("Initialized statistic month time series from \"" + ts.getIdentifierString() + "\"");
		monthts.copyHeader(ts);

		// Reset the identifier if the user has specified it...

		try
		{
			if ((!string.ReferenceEquals(newTSID, null)) && (newTSID.Length > 0))
			{
				TSIdent tsident = new TSIdent(newTSID);
				// Make sure that the output interval is Month
				if (!tsident.getInterval().Equals("Month", StringComparison.OrdinalIgnoreCase))
				{
					tsident.setInterval("Month");
				}
				monthts.setIdentifier(tsident);
			}
			else
			{
				// Default is to set the scenario to the statistic...
				monthts.getIdentifier().setScenario("" + statisticType);
			}
		}
		catch (Exception)
		{
			message = "Unable to set new time series identifier \"" + newTSID + "\".";
			Message.printWarning(3, routine,message);
			// IllegalArgumentException would have be thrown in the constructor so this must be something else.
			throw new Exception(message);
		}

		// Need to make sure the base and multiplier are correct...
		monthts.setDataInterval(TimeInterval.MONTH, 1);

		// Automatically sets the precision to month for these dates...
		// Dates determined above
		monthts.setDate1(analysisStart);
		monthts.setDate2(analysisEnd);

		if (!createData)
		{
			return monthts;
		}

		// This will fill with missing data...
		monthts.allocateDataSpace();

		// Process the statistic of interest...

		calculateStatistic(ts, monthts, statisticType, testValue, monthTestValues, analysisStart, analysisEnd, allowMissingCount.Value, minimumSampleSize.Value, analysisWindowStart, analysisWindowEnd, searchStart);

		// Return the statistic result...
		return monthts;
	}

	/// <summary>
	/// Determine whether it is OK to set a monthly statistic based on handling of missing data. </summary>
	/// <param name="missingCount"> number of missing values in the month. </param>
	/// <param name="nonMissingCount"> number of non-missing values in the month. </param>
	/// <param name="allowMissingCount"> the number of values allowed to be missing in a month (or -1 if no limit). </param>
	/// <param name="minimumSampleSize"> the minimum sample size, or -1 if no limit. </param>
	private bool okToSetMonthStatistic(int missingCount, int nonMissingCount, int allowMissingCount, int minimumSampleSize)
	{
		// Check the missing count...
		if ((allowMissingCount < 0) || (missingCount <= allowMissingCount))
		{
			// So far OK to set, but check sample size
			if ((minimumSampleSize < 0) || (nonMissingCount >= minimumSampleSize))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Set the number of values allowed to be missing in the sample. </summary>
	/// <param name="allowMissingCount"> the number of values allowed to be missing in the sample. </param>
	private void setAllowMissingCount(int allowMissingCount)
	{
		__allowMissingCount = allowMissingCount;
	}

	/// <summary>
	/// Set the end for the analysis. </summary>
	/// <param name="analysisEnd"> end date/time for the analysis. </param>
	private void setAnalysisEnd(DateTime analysisEnd)
	{
		__analysisEnd = analysisEnd;
	}

	/// <summary>
	/// Set the start for the analysis. </summary>
	/// <param name="analysisStart"> start date/time for the analysis. </param>
	private void setAnalysisStart(DateTime analysisStart)
	{
		__analysisStart = analysisStart;
	}

	/// <summary>
	/// Set the end for the analysis window. </summary>
	/// <param name="analysisEnd"> end date/time for the analysis window. </param>
	private void setAnalysisWindowEnd(DateTime analysisWindowEnd)
	{
		__analysisWindowEnd = analysisWindowEnd;
	}

	/// <summary>
	/// Set the start for the analysis window. </summary>
	/// <param name="analysisStart"> start date/time for the analysis window. </param>
	private void setAnalysisWindowStart(DateTime analysisWindowStart)
	{
		__analysisWindowStart = analysisWindowStart;
	}

	/// <summary>
	/// Set the minimum sample size. </summary>
	/// <param name="minimumSampleSize"> the minimum sample size. </param>
	private void setMinimumSampleSize(int minimumSampleSize)
	{
		__minimumSampleSize = minimumSampleSize;
	}

	/// <summary>
	/// Set the monthly test values used with some statistics. </summary>
	/// <param name="testValue"> the monthly test values used with some statistics. </param>
	private void setMonthTestValues(double?[] monthTestValues)
	{
		__monthTestValues = monthTestValues;
	}

	/// <summary>
	/// Set the new time series identifier. </summary>
	/// <param name="newTSID"> the new time series identifier. </param>
	private void setNewTSID(string newTSID)
	{
		__newTSID = newTSID;
	}

	/// <summary>
	/// Set the search start. </summary>
	/// <param name="searchStart"> start date/time processing in a month. </param>
	private void setSearchStart(DateTime searchStart)
	{
		__searchStart = searchStart;
	}

	/// <summary>
	/// Set the statistic type. </summary>
	/// <param name="statisticType"> statistic type to calculate. </param>
	private void setStatisticType(TSStatisticType statisticType)
	{
		__statisticType = statisticType;
	}

	/// <summary>
	/// Set the test value used with some statistics. </summary>
	/// <param name="testValue"> the test value used with some statistics. </param>
	private void setTestValue(double? testValue)
	{
		__testValue = testValue;
	}

	/// <summary>
	/// Set the time series being analyzed. </summary>
	/// <param name="ts"> time series being analyzed. </param>
	private void setTimeSeries(TS ts)
	{
		__ts = ts;
	}

	}

}