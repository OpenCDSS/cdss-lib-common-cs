﻿using System;
using System.Collections.Generic;

// TSUtil_NewStatisticYearTS - compute a YearTS that has a statistic for each year in the period.

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
	using DateTimeRange = RTi.Util.Time.DateTimeRange;
	using TimeInterval = RTi.Util.Time.TimeInterval;
	using TimeScaleType = RTi.Util.Time.TimeScaleType;
	using TimeUtil = RTi.Util.Time.TimeUtil;
	using YearType = RTi.Util.Time.YearType;

	// TODO SAM 2009-10-14 Migrate computational code from TSAnalyst to here
	/// <summary>
	/// Compute a YearTS that has a statistic for each year in the period.
	/// </summary>
	public class TSUtil_NewStatisticYearTS
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
	/// Output year type when new interval is year.
	/// </summary>
	private YearType __outputYearType = null;

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
	/// Test value used when analyzing the statistic (may add other values later for other statistics).
	/// </summary>
	private double? __value1 = null;

	/// <summary>
	/// Number of missing allowed to compute sample (default is no limit).
	/// </summary>
	private int __allowMissingCount = -1;

	/// <summary>
	/// Minimum required sample size (default is no limit).
	/// </summary>
	private int __minimumSampleSize = -1;

	/// <summary>
	/// Starting date/time for analysis window, within a year.
	/// </summary>
	private DateTime __analysisWindowStart = null;

	/// <summary>
	/// Ending date/time for analysis window, within a year.
	/// </summary>
	private DateTime __analysisWindowEnd = null;

	/// <summary>
	/// Search start date/time for analysis window, within a year.
	/// </summary>
	private DateTime __searchStart = null;

	/// <summary>
	/// Construct the analysis object with required input.  Values will be checked for validity.
	/// Execute the newStatisticYearTS() method to perform the analysis. </summary>
	/// <param name="ts"> time series to analyze </param>
	/// <param name="analysisStart"> Starting date/time for analysis, in precision of the original data. </param>
	/// <param name="analysisEnd"> Ending date for analysis, in precision of the original data. </param>
	/// <param name="newTSID"> the new time series identifier to be assigned to the time series. </param>
	/// <param name="statisticType"> the statistic type for the output time series. </param>
	/// <param name="value1"> test value (e.g., threshold value) needed to process some statistics. </param>
	/// <param name="allowMissingCount"> the number of values allowed to be missing in the sample. </param>
	/// <param name="minimumSampleSize"> the minimum sample size to allow to compute the statistic. </param>
	/// <param name="outputYearType"> output year type for annual time series - if null output will be calendar year. </param>
	/// <param name="analysisWindowStart"> Starting date/time (year is ignored) for analysis within the year,
	/// in precision of the original data.  If null, the entire year of data will be analyzed. </param>
	/// <param name="analysisWindowEnd"> Ending date (year is ignored) for analysis within the year,
	/// in precision of the original data.  If null, the entire year of data will be analyzed. </param>
	/// <param name="searchStart"> search start date (year is ignored) for analysis within the year,
	/// in precision of the original data.  If null, the entire year of data will be analyzed.
	/// This is used when a starting point is needed, such as when first and last values >, < in a year. </param>
	public TSUtil_NewStatisticYearTS(TS ts, string newTSID, TSStatisticType statisticType, double? value1, int? allowMissingCount, int? minimumSampleSize, YearType outputYearType, DateTime analysisStart, DateTime analysisEnd, DateTime analysisWindowStart, DateTime analysisWindowEnd, DateTime searchStart)
	{
		string routine = this.GetType().Name;
		string message;

		if (ts == null)
		{
			// Nothing to do...
			message = "Null input time series - cannot calculate statistic time series.";
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
				// Make sure that the output interval is Year
				if (!tsident.getInterval().Equals("Year", StringComparison.OrdinalIgnoreCase))
				{
					throw new System.ArgumentException("New time series identifier \"" + newTSID + "\" must have an interval of Year - " + "cannot calculate statistic time series.");
				}
			}
		}
		catch (Exception e)
		{
			throw new System.ArgumentException("New time series identifier \"" + newTSID + "\" is invalid (" + e + ").");
		}
		setStatisticType(statisticType);
		setValue1(value1);
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

		if (outputYearType == null)
		{
			outputYearType = YearType.CALENDAR;
		}
		setOutputYearType(outputYearType);
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
	/// Process a time series to create the the following annual statistics:
	/// <ol>
	/// <li>    CountGE</li>
	/// <li>    CountGT</li>
	/// <li>    CountLE</li>
	/// <li>    CountLT</li>
	/// <li>    CountMissing</li>
	/// <li>    CountNotMissing</li>
	/// <li>    DayOfFirstGE</li>
	/// <li>    DayOfFirstGT</li>
	/// <li>    DayOfFirstLE</li>
	/// <li>    DayOfFirstLT</li>
	/// <li>    DayOfLastGE</li>
	/// <li>    DayOfLastGT</li>
	/// <li>    DayOfLastLE</li>
	/// <li>    DayOfLastLT</li>
	/// <li>    DayOfMax</li>
	/// <li>    DayOfMin</li>
	/// <li>    Max</li>
	/// <li>    Mean</li>
	/// <li>    Min</li>
	/// <li>    Total</li>
	/// </ol> </summary>
	/// <param name="ts"> Time series to analyze. </param>
	/// <param name="yearts"> YearTS to fill with the statistic. </param>
	/// <param name="statisticType"> statistic to calculate. </param>
	/// <param name="value1"> a number to test against for some statistics (e.g., COUNT_LE). </param>
	/// <param name="analysisStart"> Start of the analysis (precision matching ts). </param>
	/// <param name="analysisEnd"> End of the analysis (precision matching ts). </param>
	/// <param name="allowMissingCount"> the number of missing values allowed in input and still compute the statistic. </param>
	/// <param name="minimumSampleSize"> the minimum number of values required in input to compute the statistic. </param>
	/// <param name="analysisWindowStart"> If not null, specify the start of the window within
	/// the year for data, for example to specify a season.
	/// Currently only Month... to precision are evaluated (not day... etc.). </param>
	/// <param name="analysisWindowEnd"> If not null, specify the end of the window within
	/// the year for data, for example to specify a season.
	/// Currently only Month... to precision are evaluated (not day... etc.). </param>
	/// <param name="searchStart"> starting date/time in the year to analyze, in particular to condition
	/// seasonal data processing. </param>
	private void calculateStatistic(TS ts, YearTS yearts, TSStatisticType statisticType, double? value1, YearType outputYearType, DateTime analysisStart, DateTime analysisEnd, int allowMissingCount, int minimumSampleSize, DateTime analysisWindowStart, DateTime analysisWindowEnd, DateTime searchStart)
	{
		string routine = this.GetType().Name + ".calculateStatistic";
		// Initialize the settings to evaluate the statistic and set appropriate information in the time series
		bool statisticIsCount = isStatisticCount(statisticType);
		bool statisticIsDayOf = isStatisticDayOf(statisticType);
		bool statisticIsMonthOf = isStatisticMonthOf(statisticType);
		bool statisticIsFirst = isStatisticFirst(statisticType);
		bool statisticIsLast = isStatisticLast(statisticType);
		bool iterateForward = isStatisticIterateForward(statisticType);
		TestType testType = getStatisticTestType(statisticType);
		yearts.setDescription(getStatisticTimeSeriesDescription(statisticType, testType, value1, statisticIsCount, statisticIsDayOf, statisticIsMonthOf, statisticIsFirst, statisticIsLast));
		yearts.setDataUnits(getStatisticTimeSeriesDataUnits(statisticType, statisticIsCount, statisticIsDayOf, statisticIsMonthOf, ts.getDataUnits()));

		Message.printStatus(2, routine, "Overall analysis period is " + analysisStart + " to " + analysisEnd);

		double testValueDouble = Double.NaN; // OK to initialize to this because checks will have verified real value
		if (isTestValueNeeded(statisticType))
		{
			testValueDouble = value1.Value;
		}
		// For debugging...
		//Message.printStatus(2,routine,"StatisticType=" + statisticType + " TestType=" + testType +
		//    " testValueDouble=" + testValueDouble );

		// Create dates that have the correct precision (matching the analysis period) and initialize for
		// the first year.  These are used to step through the input time series one full year at a time.
		// The iteration year based on the old time series dates may start one year too early but that
		// is OK since the output time series will simply not have a value added out of period.
		// yearStart and yearEnd are in calendar year
		// but outputYear is the output year in the output year type
		DateTime yearStart = new DateTime(ts.getDate1()); // To set precision to that of the input time series
		yearStart.setYear(yearts.getDate1().getYear());
		yearStart.addYear(outputYearType.getStartYearOffset());
		yearStart.setMonth(outputYearType.getStartMonth());
		yearStart.setDay(1); // Will not be used if monthly input
		yearStart.setHour(0);
		yearStart.setMinute(0);
		DateTime yearEnd = new DateTime(yearStart);
		yearEnd.addMonth(11);
		yearEnd.setDay(TimeUtil.numDaysInMonth(yearEnd.getMonth(), yearEnd.getYear()));
		yearEnd.setHour(23);
		yearEnd.setMinute(59);
		// Always create a new instance of date for the iterator to protect original data
		double value; // The value in the original time series
		// Year for output (reverse the year offset from the window)...
		DateTime outputYear = new DateTime(yearStart,DateTime.PRECISION_YEAR);
		outputYear.addYear(-1 * outputYearType.getStartYearOffset());
		// Dates for iteration in a year, adjusted for the analysis window
		// Use these internal to the loop below so as to not interfere with the full-year iteration
		DateTime yearStartForAnalysisWindow = null;
		DateTime yearEndForAnalysisWindow = null;
		// Loop until all the new years are processed
		// The new time series period should have been defined to align with the output year type
		// Checking the year start will allow any period - with missing values filling in if necessary
		while (yearStart.lessThanOrEqualTo(analysisEnd))
		{
			// Initialize for the new year
			double sum = 0.0;
			int nMissing = 0;
			int nNotMissing = 0;
			// Extract data from the old time series for the new year
			// This loops over the full year so that missing values on the end will impact results
			Message.printStatus(2, routine, "Processing old time series for " + yearStart + " to " + yearEnd + " to create output year " + outputYear);
			// Change the window within the year based on the analysis window.  This will shorten the
			// number of iterations and potentially lessen the missing data count.  This is OK to set here
			// because the year is initialized outside the loop above and is incremented at the end of the
			// loop below.  If not using calendar year, the analysis window start may have a month that is
			// later than the end month, but that is OK because the calendar years for the start and end
			// will be different.
			if (analysisWindowStart == null)
			{
				// OK to use full years
				yearStartForAnalysisWindow = yearStart;
			}
			else
			{
				// Make a copy of the date/time for iteration within years
				yearStartForAnalysisWindow = new DateTime(yearStart);
				int analysisWindowStartMonth = analysisWindowStart.getMonth();
				if ((outputYearType.getStartYearOffset() < 0) && (analysisWindowStartMonth < outputYearType.getStartMonth()) && (yearStart.getYear() < yearEnd.getYear()))
				{
					// Need to adjust the year because the default start of year is in the previous year
					// The check on the years above is redundant but put in just in case
					yearStartForAnalysisWindow.setYear(yearEnd.getYear());
				}
				yearStartForAnalysisWindow.setMonth(analysisWindowStart.getMonth());
				yearStartForAnalysisWindow.setDay(analysisWindowStart.getDay());
				yearStartForAnalysisWindow.setHour(analysisWindowStart.getHour());
				yearStartForAnalysisWindow.setMinute(analysisWindowStart.getMinute());
			}
			if (analysisWindowEnd == null)
			{
				// OK to use full years
				yearEndForAnalysisWindow = yearEnd;
			}
			else
			{
				// Make a copy of the date/time for iteration within years
				yearEndForAnalysisWindow = new DateTime(yearEnd);
				int analysisWindowEndMonth = analysisWindowEnd.getMonth();
				if ((outputYearType.getStartYearOffset() < 0) && (analysisWindowEndMonth >= outputYearType.getStartMonth()) && (yearStart.getYear() < yearEnd.getYear()))
				{
					// Need to adjust the year because the default end of year is in the next year
					// The check on the years above is redundant but put in just in case
					yearEndForAnalysisWindow.setYear(yearStart.getYear());
				}
				yearEndForAnalysisWindow.setMonth(analysisWindowEnd.getMonth());
				yearEndForAnalysisWindow.setDay(analysisWindowEnd.getDay());
				yearEndForAnalysisWindow.setHour(analysisWindowEnd.getHour());
				yearEndForAnalysisWindow.setMinute(analysisWindowEnd.getMinute());
			}
			if ((analysisWindowStart != null) || (analysisWindowEnd != null))
			{
				Message.printStatus(2, routine, "Resetting input analysis window to requested " + yearStartForAnalysisWindow + " to " + yearEndForAnalysisWindow);
			}
			// Further adjust the analysis window based on the search start
			if (searchStart != null)
			{
				// Reset the start or end of the analysis window depending on whether
				// iterating forward (reset start) or backward (reset end).
				if (iterateForward)
				{
					yearStartForAnalysisWindow = new DateTime(yearStart);
					int searchStartMonth = searchStart.getMonth();
					if ((outputYearType.getStartYearOffset() < 0) && (searchStartMonth < outputYearType.getStartMonth()) && (yearStart.getYear() < yearEnd.getYear()))
					{
						// Need to adjust the year because the default start of year is in the previous year
						// The check on the years above is redundant but put in just in case
						yearStartForAnalysisWindow.setYear(yearEnd.getYear());
					}
					yearStartForAnalysisWindow.setMonth(searchStart.getMonth());
					yearStartForAnalysisWindow.setDay(searchStart.getDay());
					yearStartForAnalysisWindow.setHour(searchStart.getHour());
					yearStartForAnalysisWindow.setMinute(searchStart.getMinute());
				}
				else
				{
					yearEndForAnalysisWindow = new DateTime(yearEnd);
					int searchStartMonth = searchStart.getMonth();
					if ((outputYearType.getStartYearOffset() < 0) && (searchStartMonth >= outputYearType.getStartMonth()) && (yearStart.getYear() < yearEnd.getYear()))
					{
						// Need to adjust the year because the default end of year is in the next year
						// The check on the years above is redundant but put in just in case
						yearEndForAnalysisWindow.setYear(yearStart.getYear());
					}
					yearEndForAnalysisWindow.setMonth(searchStart.getMonth());
					yearEndForAnalysisWindow.setDay(searchStart.getDay());
					yearEndForAnalysisWindow.setHour(searchStart.getHour());
					yearEndForAnalysisWindow.setMinute(searchStart.getMinute());
				}
				Message.printStatus(2, routine, "Resetting input analysis window using SearchStart to requested " + yearStartForAnalysisWindow + " to " + yearEndForAnalysisWindow);
			}
			// Create an iterator for the data...
			TSIterator tsi = null;
			try
			{
				   tsi = ts.iterator(yearStartForAnalysisWindow,yearEndForAnalysisWindow);
			}
			catch (Exception e)
			{
				throw new Exception("Exception initializing time series iterator for analysis window " + yearStartForAnalysisWindow + " to " + yearEndForAnalysisWindow + " uniquetempvar.");
			}
			TSData data; // Data from the iterator
			DateTime date; // Date of the data value
			// Initialize the value of the statistic in the year to missing.  Do this each time an input
			// year is processed.
			double yearValue = yearts.getMissing(); // Data value for output time series
			double dayMoment = 0.0; // Day of year * value, for DayOfCentroid statistic
			double monthMoment = 0.0; // Month of year * value, for MonthOfCentroid statistic
			double extremeValue = yearts.getMissing(); // Used for DayOfMin, etc., where check and day are tracked.
			bool doneAnalyzing = false; // If true, there is no more data or a statistic is complete
			// Loop through the data in the analysis window
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
						// run out, statistics that depend on the missing count will be processed correctly
						// below.
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
								if (yearts.isDataMissing(yearValue))
								{
									yearValue = 1.0;
								}
								else
								{
									yearValue += 1.0;
								}
							}
							else if ((statisticType == TSStatisticType.DAY_OF_FIRST_GE) || (statisticType == TSStatisticType.DAY_OF_LAST_GE))
							{
								yearValue = TimeUtil.dayOfYear(date, outputYearType);
								doneAnalyzing = true; // Found value for the year.
							}
							else if ((statisticType == TSStatisticType.MONTH_OF_FIRST_GE) || (statisticType == TSStatisticType.MONTH_OF_LAST_GE))
							{
								yearValue = TimeUtil.monthOfYear(date, outputYearType);
								doneAnalyzing = true; // Found value for the year.
							}
						}
						else if ((testType == TestType.GT) && (value > testValueDouble))
						{
							if ((statisticType == TSStatisticType.GT_COUNT) || (statisticType == TSStatisticType.GT_PERCENT))
							{
								if (yearts.isDataMissing(yearValue))
								{
									yearValue = 1.0;
								}
								else
								{
									yearValue += 1.0;
								}
							}
							else if ((statisticType == TSStatisticType.DAY_OF_FIRST_GT) || (statisticType == TSStatisticType.DAY_OF_LAST_GT))
							{
								yearValue = TimeUtil.dayOfYear(date, outputYearType);
								doneAnalyzing = true; // Found value for the year.
							}
							else if ((statisticType == TSStatisticType.MONTH_OF_FIRST_GT) || (statisticType == TSStatisticType.MONTH_OF_LAST_GT))
							{
								yearValue = TimeUtil.monthOfYear(date, outputYearType);
								doneAnalyzing = true; // Found value for the year.
							}
						}
						else if ((testType == TestType.LE) && (value <= testValueDouble))
						{
							Message.printStatus(2, routine, "Found value " + value + " <= " + testValueDouble + " on " + date);
							if ((statisticType == TSStatisticType.LE_COUNT) || (statisticType == TSStatisticType.LE_PERCENT))
							{
								if (yearts.isDataMissing(yearValue))
								{
									yearValue = 1.0;
								}
								else
								{
									yearValue += 1.0;
								}
							}
							else if ((statisticType == TSStatisticType.DAY_OF_FIRST_LE) || (statisticType == TSStatisticType.DAY_OF_LAST_LE))
							{
								yearValue = TimeUtil.dayOfYear(date, outputYearType);
								doneAnalyzing = true; // Found value for the year.
							}
							else if ((statisticType == TSStatisticType.MONTH_OF_FIRST_LE) || (statisticType == TSStatisticType.MONTH_OF_LAST_LE))
							{
								yearValue = TimeUtil.monthOfYear(date, outputYearType);
								doneAnalyzing = true; // Found value for the year.
							}
						}
						else if ((testType == TestType.LT) && (value < testValueDouble))
						{
							if ((statisticType == TSStatisticType.LT_COUNT) || (statisticType == TSStatisticType.LT_PERCENT))
							{
								if (yearts.isDataMissing(yearValue))
								{
									yearValue = 1.0;
								}
								else
								{
									yearValue += 1.0;
								}
							}
							else if ((statisticType == TSStatisticType.DAY_OF_FIRST_LT) || (statisticType == TSStatisticType.DAY_OF_LAST_LT))
							{
								yearValue = TimeUtil.dayOfYear(date, outputYearType);
								doneAnalyzing = true; // Found value for the year.
							}
							else if ((statisticType == TSStatisticType.MONTH_OF_FIRST_LT) || (statisticType == TSStatisticType.MONTH_OF_LAST_LT))
							{
								yearValue = TimeUtil.monthOfYear(date, outputYearType);
								doneAnalyzing = true; // Found value for the year.
							}
						}
						else if ((statisticType == TSStatisticType.DAY_OF_CENTROID))
						{
							dayMoment += TimeUtil.dayOfYear(date,outputYearType) * value;
						}
						else if ((statisticType == TSStatisticType.MONTH_OF_CENTROID))
						{
							monthMoment += TimeUtil.monthOfYear(date,outputYearType) * value;
						}
						else if ((statisticType == TSStatisticType.DAY_OF_MAX) || (statisticType == TSStatisticType.MONTH_OF_MAX) || (statisticType == TSStatisticType.MAX))
						{
							if (yearts.isDataMissing(extremeValue) || (value > extremeValue))
							{
								// Set the max...
								if (statisticType == TSStatisticType.DAY_OF_MAX)
								{
									extremeValue = value;
									yearValue = TimeUtil.dayOfYear(date, outputYearType);
								}
								else if (statisticType == TSStatisticType.MONTH_OF_MAX)
								{
									extremeValue = value;
									yearValue = TimeUtil.monthOfYear(date, outputYearType);
								}
								else if (statisticType == TSStatisticType.MAX)
								{
									extremeValue = value;
									yearValue = value;
								}
							}
							// Need to continue analyzing period so do not set doneAnalyzing to false.
						}
						else if ((statisticType == TSStatisticType.DAY_OF_MIN) || (statisticType == TSStatisticType.MONTH_OF_MIN) || (statisticType == TSStatisticType.MIN))
						{
							if (yearts.isDataMissing(extremeValue) || (value < extremeValue))
							{
								Message.printStatus(2,routine,"Found new min " + value + " on " + date);
								// Set the min...
								if (statisticType == TSStatisticType.DAY_OF_MIN)
								{
									extremeValue = value;
									yearValue = TimeUtil.dayOfYear(date, outputYearType);
								}
								else if (statisticType == TSStatisticType.MONTH_OF_MIN)
								{
									extremeValue = value;
									yearValue = TimeUtil.monthOfYear(date, outputYearType);
								}
								else if (statisticType == TSStatisticType.MIN)
								{
									extremeValue = value;
									yearValue = value;
								}
							}
							// Need to continue analyzing period so do not set doneAnalyzing to false.
						}
						else if ((statisticType == TSStatisticType.MEAN) || (statisticType == TSStatisticType.TOTAL))
						{
							// Need to accumulate the value (for Mean or Total)
							// Accumulate into the year_value
							if (yearts.isDataMissing(yearValue))
							{
								yearValue = value;
							}
							else
							{
								yearValue += value;
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
					Message.printStatus(2, routine, "For output year " + outputYear + ", sum=" + sum + ", nMissing=" + nMissing + ", nNotMissing=" + nNotMissing);
					if (statisticType == TSStatisticType.DAY_OF_CENTROID)
					{
						if ((nNotMissing > 0) && okToSetYearStatistic(nMissing, nNotMissing, allowMissingCount, minimumSampleSize))
						{
							yearts.setDataValue(outputYear, dayMoment / sum);
						}
					}
					else if (statisticType == TSStatisticType.MEAN)
					{
						if ((nNotMissing > 0) && okToSetYearStatistic(nMissing, nNotMissing, allowMissingCount, minimumSampleSize))
						{
							yearts.setDataValue(outputYear, sum / nNotMissing);
						}
					}
					else if (statisticType == TSStatisticType.MISSING_COUNT)
					{
						// Always assign
						yearts.setDataValue(outputYear, (double)nMissing);
					}
					else if (statisticType == TSStatisticType.MISSING_PERCENT)
					{
						// Always assign
						if ((nMissing + nNotMissing) > 0)
						{
							yearts.setDataValue(outputYear, 100.0 * (double)nMissing / (double)(nMissing + nNotMissing));
						}
					}
					else if (statisticType == TSStatisticType.MONTH_OF_CENTROID)
					{
						if ((nNotMissing > 0) && okToSetYearStatistic(nMissing, nNotMissing, allowMissingCount, minimumSampleSize))
						{
							yearts.setDataValue(outputYear, monthMoment / sum);
						}
					}
					else if (statisticType == TSStatisticType.NONMISSING_COUNT)
					{
						// Always assign
						yearts.setDataValue(outputYear, (double)nNotMissing);
					}
					else if (statisticType == TSStatisticType.NONMISSING_PERCENT)
					{
						// Always assign
						if ((nMissing + nNotMissing) > 0)
						{
							yearts.setDataValue(outputYear, 100.0 * (double)nNotMissing / (double)(nMissing + nNotMissing));
						}
					}
					else if (statisticType == TSStatisticType.TOTAL)
					{
						if ((nNotMissing > 0) && okToSetYearStatistic(nMissing, nNotMissing, allowMissingCount, minimumSampleSize))
						{
							yearts.setDataValue(outputYear, sum);
						}
					}
					else
					{
						Message.printStatus(2,routine,"Year " + outputYear + " value=" + yearValue);
						if (!yearts.isDataMissing(yearValue) && okToSetYearStatistic(nMissing, nNotMissing, allowMissingCount, minimumSampleSize))
						{
							// No additional processing is needed.
							if ((statisticType == TSStatisticType.GE_PERCENT) || (statisticType == TSStatisticType.GT_PERCENT) || (statisticType == TSStatisticType.LE_PERCENT) || (statisticType == TSStatisticType.LT_PERCENT))
							{
								if ((nMissing + nNotMissing) > 0)
								{
									// Note that these statistics are for total number of points (not just
									// non-missing points).
									yearts.setDataValue(outputYear, 100.0 * yearValue / (nMissing + nNotMissing));
								}
							}
							else
							{
								// Statistic has already been calculated.
								yearts.setDataValue(outputYear, yearValue);
							}
						}
					}
					// Increment the dates in the full output years.
					// The dates may be changed for the analysis window in the next loop.
					yearStart.addYear(1);
					// Also reset the day because the number of days in the month may depend on the year
					yearEnd.addYear(1);
					yearEnd.setDay(TimeUtil.numDaysInMonth(yearEnd.getMonth(), yearEnd.getYear()));
					outputYear.addYear(1);
					break; // Will go to the next year of input, with new iterator
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
	/// Return the time series identifier for the new time series. </summary>
	/// <returns> the time series identifier for the new time series. </returns>
	private string getNewTSID()
	{
		return __newTSID;
	}

	/// <summary>
	/// Return the output year type. </summary>
	/// <returns> the output year type. </returns>
	private YearType getOutputYearType()
	{
		return __outputYearType;
	}

	/// <summary>
	/// Return the search start date/time. </summary>
	/// <returns> the search start date/time. </returns>
	private DateTime getSearchStart()
	{
		return __searchStart;
	}

	/// <summary>
	/// Return a list of statistic choices for the requested interval and scale.
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
		// Monthly or finer...
		if ((interval <= TimeInterval.MONTH) || (interval == TimeInterval.UNKNOWN))
		{
			statistics.Add(TSStatisticType.MONTH_OF_CENTROID);
			statistics.Add(TSStatisticType.MONTH_OF_FIRST_GE);
			statistics.Add(TSStatisticType.MONTH_OF_FIRST_GT);
			statistics.Add(TSStatisticType.MONTH_OF_FIRST_LE);
			statistics.Add(TSStatisticType.MONTH_OF_FIRST_LT);
			statistics.Add(TSStatisticType.MONTH_OF_LAST_GE);
			statistics.Add(TSStatisticType.MONTH_OF_LAST_GT);
			statistics.Add(TSStatisticType.MONTH_OF_LAST_LE);
			statistics.Add(TSStatisticType.MONTH_OF_LAST_LT);
			statistics.Add(TSStatisticType.MONTH_OF_MAX);
			statistics.Add(TSStatisticType.MONTH_OF_MIN);
		}
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
		if ((statisticType == TSStatisticType.GE_COUNT) || (statisticType == TSStatisticType.GE_PERCENT) || (statisticType == TSStatisticType.DAY_OF_FIRST_GE) || (statisticType == TSStatisticType.DAY_OF_LAST_GE) || (statisticType == TSStatisticType.MONTH_OF_FIRST_GE) || (statisticType == TSStatisticType.MONTH_OF_LAST_GE))
		{
			return TestType.GE;
		}
		else if ((statisticType == TSStatisticType.GT_COUNT) || (statisticType == TSStatisticType.GT_PERCENT) || (statisticType == TSStatisticType.DAY_OF_FIRST_GT) || (statisticType == TSStatisticType.DAY_OF_LAST_GT) || (statisticType == TSStatisticType.MONTH_OF_FIRST_GT) || (statisticType == TSStatisticType.MONTH_OF_LAST_GT))
		{
			return TestType.GT;
		}
		else if ((statisticType == TSStatisticType.LE_COUNT) || (statisticType == TSStatisticType.LE_PERCENT) || (statisticType == TSStatisticType.DAY_OF_FIRST_LE) || (statisticType == TSStatisticType.DAY_OF_LAST_LE) || (statisticType == TSStatisticType.MONTH_OF_FIRST_LE) || (statisticType == TSStatisticType.MONTH_OF_LAST_LE))
		{
			return TestType.LE;
		}
		else if ((statisticType == TSStatisticType.LT_COUNT) || (statisticType == TSStatisticType.LT_PERCENT) || (statisticType == TSStatisticType.DAY_OF_FIRST_LT) || (statisticType == TSStatisticType.DAY_OF_LAST_LT) || (statisticType == TSStatisticType.MONTH_OF_FIRST_LT) || (statisticType == TSStatisticType.MONTH_OF_LAST_LT))
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
	private string getStatisticTimeSeriesDataUnits(TSStatisticType statisticType, bool statisticIsCount, bool statisticIsDayOf, bool statisticIsMonthOf, string tsUnits)
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
			return "DayOfYear";
		}
		else if (statisticIsMonthOf)
		{
			return "MonthOfYear";
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
	private string getStatisticTimeSeriesDescription(TSStatisticType statisticType, TestType testType, double? testValue, bool statisticIsCount, bool statisticIsDayOf, bool statisticIsMonthOf, bool statisticIsFirst, bool statisticIsLast)
	{
		string testString = "?test?";
		string testValueString = "?testValue?";
		string desc = "?";
		if (testValue != null)
		{
			testValueString = StringUtil.formatString(testValue.Value,"%.6f");
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
				desc = "Day of year for first value " + testString + " " + testValueString;
			}
			else if (statisticIsLast)
			{
				desc = "Day of year for last value " + testString + " " + testValueString;
			}
			else if (statisticType == TSStatisticType.DAY_OF_CENTROID)
			{
				desc = "Day of year for centroid";
			}
			else if (statisticType == TSStatisticType.DAY_OF_MAX)
			{
				desc = "Day of year for maximum value";
			}
			else if (statisticType == TSStatisticType.DAY_OF_MIN)
			{
				desc = "Day of year for minimum value";
			}
		}
		else if (statisticIsMonthOf)
		{
			if (statisticIsFirst)
			{
				desc = "Month of year for first value " + testString + " " + testValueString;
			}
			else if (statisticIsLast)
			{
				desc = "Month of year for last value " + testString + " " + testValueString;
			}
			else if (statisticType == TSStatisticType.MONTH_OF_CENTROID)
			{
				desc = "Month of year for centroid";
			}
			else if (statisticType == TSStatisticType.MONTH_OF_MAX)
			{
				desc = "Month of year for maximum value";
			}
			else if (statisticType == TSStatisticType.MONTH_OF_MIN)
			{
				desc = "Month of year for minimum value";
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
	private double? getValue1()
	{
		return __value1;
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
		if ((statisticType == TSStatisticType.DAY_OF_FIRST_GE) || (statisticType == TSStatisticType.DAY_OF_FIRST_GT) || (statisticType == TSStatisticType.DAY_OF_FIRST_LE) || (statisticType == TSStatisticType.DAY_OF_FIRST_LT) || (statisticType == TSStatisticType.MONTH_OF_FIRST_GE) || (statisticType == TSStatisticType.MONTH_OF_FIRST_GT) || (statisticType == TSStatisticType.MONTH_OF_FIRST_LE) || (statisticType == TSStatisticType.MONTH_OF_FIRST_LT))
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
		if ((statisticType == TSStatisticType.DAY_OF_LAST_GE) || (statisticType == TSStatisticType.DAY_OF_LAST_GT) || (statisticType == TSStatisticType.DAY_OF_LAST_LE) || (statisticType == TSStatisticType.DAY_OF_LAST_LT) || (statisticType == TSStatisticType.MONTH_OF_LAST_GE) || (statisticType == TSStatisticType.MONTH_OF_LAST_GT) || (statisticType == TSStatisticType.MONTH_OF_LAST_LE) || (statisticType == TSStatisticType.MONTH_OF_LAST_LT))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Indicate whether the statistic is a "MonthOf" statistic. </summary>
	/// <param name="statisticType"> a statistic type to check. </param>
	private bool isStatisticMonthOf(TSStatisticType statisticType)
	{
		if ((statisticType == TSStatisticType.MONTH_OF_CENTROID) || (statisticType == TSStatisticType.MONTH_OF_FIRST_GE) || (statisticType == TSStatisticType.MONTH_OF_FIRST_GT) || (statisticType == TSStatisticType.MONTH_OF_FIRST_LE) || (statisticType == TSStatisticType.MONTH_OF_FIRST_LT) || (statisticType == TSStatisticType.MONTH_OF_LAST_GE) || (statisticType == TSStatisticType.MONTH_OF_LAST_GT) || (statisticType == TSStatisticType.MONTH_OF_LAST_LE) || (statisticType == TSStatisticType.MONTH_OF_LAST_LT) || (statisticType == TSStatisticType.MONTH_OF_MAX) || (statisticType == TSStatisticType.MONTH_OF_MIN))
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
		if ((statisticType == TSStatisticType.DAY_OF_LAST_GE) || (statisticType == TSStatisticType.DAY_OF_LAST_GT) || (statisticType == TSStatisticType.DAY_OF_LAST_LE) || (statisticType == TSStatisticType.DAY_OF_LAST_LT) || (statisticType == TSStatisticType.MONTH_OF_LAST_GE) || (statisticType == TSStatisticType.MONTH_OF_LAST_GT) || (statisticType == TSStatisticType.MONTH_OF_LAST_LE) || (statisticType == TSStatisticType.MONTH_OF_LAST_LT))
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
		if ((statisticType == TSStatisticType.DAY_OF_FIRST_GE) || (statisticType == TSStatisticType.DAY_OF_FIRST_GT) || (statisticType == TSStatisticType.DAY_OF_FIRST_LE) || (statisticType == TSStatisticType.DAY_OF_FIRST_LT) || (statisticType == TSStatisticType.DAY_OF_LAST_GE) || (statisticType == TSStatisticType.DAY_OF_LAST_GT) || (statisticType == TSStatisticType.DAY_OF_LAST_LE) || (statisticType == TSStatisticType.DAY_OF_LAST_LT) || (statisticType == TSStatisticType.GE_COUNT) || (statisticType == TSStatisticType.GE_PERCENT) || (statisticType == TSStatisticType.GT_COUNT) || (statisticType == TSStatisticType.GT_PERCENT) || (statisticType == TSStatisticType.LE_COUNT) || (statisticType == TSStatisticType.LE_PERCENT) || (statisticType == TSStatisticType.LT_COUNT) || (statisticType == TSStatisticType.LT_PERCENT) || (statisticType == TSStatisticType.MONTH_OF_FIRST_GE) || (statisticType == TSStatisticType.MONTH_OF_FIRST_GT) || (statisticType == TSStatisticType.MONTH_OF_FIRST_LE) || (statisticType == TSStatisticType.MONTH_OF_FIRST_LT) || (statisticType == TSStatisticType.MONTH_OF_LAST_GE) || (statisticType == TSStatisticType.MONTH_OF_LAST_GT) || (statisticType == TSStatisticType.MONTH_OF_LAST_LE) || (statisticType == TSStatisticType.MONTH_OF_LAST_LT))
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
	public virtual YearTS newStatisticYearTS(bool createData)
	{
		string message, routine = this.GetType().Name + ".newStatisticYearTS";
		int dl = 10;

		// Get the data needed for the analysis - originally provided in the constructor

		TS ts = getTimeSeries();
		string newTSID = getNewTSID();
		TSStatisticType statisticType = getStatisticType();
		double? testValue = getValue1();
		int? allowMissingCount = getAllowMissingCount();
		int? minimumSampleSize = getMinimumSampleSize();
		DateTime analysisStart = getAnalysisStart();
		DateTime analysisEnd = getAnalysisEnd();
		YearType outputYearType = getOutputYearType();
		DateTime analysisWindowStart = getAnalysisWindowStart();
		DateTime analysisWindowEnd = getAnalysisWindowEnd();
		DateTime searchStart = getSearchStart();

		if (Message.isDebugOn)
		{
		   Message.printDebug(dl, routine, "Trying to create statistic year TS for time series \"" + ts.getIdentifierString() + "\"");
		}

		// Get valid dates for the output time series because the ones passed in may have been null...

		TSLimits validDates = TSUtil.getValidPeriod(ts, analysisStart, analysisEnd);
		// Reset because these are handled generically below whether passed in or defaulted to "ts"
		analysisStart = new DateTime(validDates.getDate1());
		analysisEnd = new DateTime(validDates.getDate2());

		// Create a year time series to be filled...

		YearTS yearts = new YearTS();
		yearts.addToGenesis("Initialized statistic year time series from \"" + ts.getIdentifierString() + "\"");
		yearts.copyHeader(ts);

		// Reset the identifier if the user has specified it...

		try
		{
			if ((!string.ReferenceEquals(newTSID, null)) && (newTSID.Length > 0))
			{
				// Expand the new TSID using the input time series
				string newTSIDExpanded = ts.formatLegend(newTSID);
				TSIdent tsident = new TSIdent(newTSIDExpanded);
				// Make sure that the output interval is Year
				if (!tsident.getInterval().Equals("Year", StringComparison.OrdinalIgnoreCase))
				{
					tsident.setInterval("Year");
				}
				yearts.setIdentifier(tsident);
			}
			else
			{
				// Default is to set the scenario to the statistic...
				yearts.getIdentifier().setScenario("" + statisticType);
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
		yearts.setDataInterval(TimeInterval.YEAR, 1);

		// Automatically sets the precision to Year for these dates...
		DateTimeRange yeartsDateRange = TimeUtil.determineOutputYearTypeRange(analysisStart, analysisEnd, outputYearType);
		yearts.setDate1(yeartsDateRange.getStart());
		yearts.setDate2(yeartsDateRange.getEnd());
		if (Message.isDebugOn)
		{
			Message.printDebug(1, routine, "Output year type " + outputYearType + " period is " + yearts.getDate1() + " to " + yearts.getDate2());
		}

		if (!createData)
		{
			return yearts;
		}

		// This will fill with missing data...
		yearts.allocateDataSpace();

		// Process the statistic of interest...

		// FIXME SAM 2009-11-06 Legacy method that seems too complicated.
		//    processStatistic ( ts, yearts, statisticType, testValue, start, end,
		//        allowMissingCount, minimumSampleSize, analysisWindowStart, analysisWindowEnd );
		calculateStatistic(ts, yearts, statisticType, testValue, outputYearType, analysisStart, analysisEnd, allowMissingCount.Value, minimumSampleSize.Value, analysisWindowStart, analysisWindowEnd, searchStart);

		// Return the statistic result...
		return yearts;
	}

	/// <summary>
	/// Determine whether it is OK to set a yearly statistic based on handling of missing data. </summary>
	/// <param name="missingCount"> number of missing values in the year. </param>
	/// <param name="nonMissingCount"> number of non-missing values in the year. </param>
	/// <param name="allowMissingCount"> the number of values allowed to be missing in a year (or -1 if no limit). </param>
	/// <param name="minimumSampleSize"> the minimum sample size, or -1 if no limit. </param>
	private bool okToSetYearStatistic(int missingCount, int nonMissingCount, int allowMissingCount, int minimumSampleSize)
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
	/// Process a time series to create the the following annual statistics:
	/// <ol>
	/// <li>    CountGE</li>
	/// <li>    CountGT</li>
	/// <li>    CountLE</li>
	/// <li>    CountLT</li>
	/// <li>    CountMissing</li>
	/// <li>    CountNotMissing</li>
	/// <li>    DayOfFirstGE</li>
	/// <li>    DayOfFirstGT</li>
	/// <li>    DayOfFirstLE</li>
	/// <li>    DayOfFirstLT</li>
	/// <li>    DayOfLastGE</li>
	/// <li>    DayOfLastGT</li>
	/// <li>    DayOfLastLE</li>
	/// <li>    DayOfLastLT</li>
	/// <li>    DayOfMax</li>
	/// <li>    DayOfMin</li>
	/// <li>    Max</li>
	/// <li>    Mean</li>
	/// <li>    Min</li>
	/// <li>    Total</li>
	/// </ol> </summary>
	/// <param name="ts"> Time series to analyze. </param>
	/// <param name="yearts"> YearTS to fill with the statistic. </param>
	/// <param name="start"> Start of the analysis (precision matching ts). </param>
	/// <param name="end"> End of the analysis (precision matching ts). </param>
	/// <param name="analysisWindowStart"> If not null, specify the start of the window within
	/// the year for data, for example to specify a season.
	/// Currently only Month... to precision are evaluated (not day... etc.). </param>
	/// <param name="analysisWindowEnd"> If not null, specify the end of the window within
	/// the year for data, for example to specify a season.
	/// Currently only Month... to precision are evaluated (not day... etc.). </param>
	/// <param name="searchStart"> date/time to start search within each year. </param>
	/*
	private void processStatistic ( TS ts, YearTS yearts, TSStatisticType statisticType,
	    Double testValue, DateTime start, DateTime end,
	    int allowMissingCount, int minimumSampleSize,
	    DateTime analysisWindowStart, DateTime analysisWindowEnd, DateTime searchStart )
	{   String routine = getClass().getName() + ".processStatistic", message;
	    DateTime yearDate = new DateTime ( end, DateTime.PRECISION_YEAR );
	    double yearValue = 0.0,    // Statistic value for year.
	        value = 0.0,        // Time series data value.
	        extremeValue = 0.0;    // Extreme value in a year.
	    boolean iterateForward = true; // Direction for iteration (true = forward, false = backward)
	    int dl = 1; // Debug level for this method
	
	    int interval_base = ts.getDataIntervalBase();
	    int interval_mult = ts.getDataIntervalMult();
	    if ( interval_base != TimeInterval.DAY ) {
	        message = "Only daily time series can be processed.";
	        Message.printWarning ( 3, routine, message );
	        throw new InvalidParameterException ( message );
	    }
	    boolean isRegular = TimeInterval.isRegularInterval(interval_base);
	    
	    double testValueDouble = -1; // OK to initialize to this because checks will have verified real value
	    if ( isTestValueNeeded( statisticType ) ) {
	        testValueDouble = testValue.doubleValue();
	    }
	
	    String searchStartString = ""; // For output messages
	    if ( searchStart != null ) {
	        searchStartString = "" + searchStart;
	    }
	    
	    TestType testType = TestType.NOT_USED;
	
	    boolean statisticIsCount = isStatisticCount(statisticType);
	    if ( statisticType == TSStatisticType.COUNT_GE ) {
	        iterateForward = true;
	        testType = TestType.GE;
	        yearts.setDescription ( "Count of values >= " + testValue );
	        searchStartString = "01-01";  // Always default to first of year
	        yearts.setDataUnits ( "Count" );
	    }
	    else if ( statisticType == TSStatisticType.COUNT_GT ) {
	        iterateForward = true;
	        testType = TestType.GT;
	        yearts.setDescription ( "Count of values > " + testValue );
	        searchStartString = "01-01";  // Always default to first of year
	        yearts.setDataUnits ( "Count" );
	    }
	    else if ( statisticType == TSStatisticType.COUNT_LE ) {
	        iterateForward = true;
	        testType = TestType.LE;
	        yearts.setDescription ( "Count of values <= " + testValue );
	        searchStartString = "01-01";  // Always default to first of year
	        yearts.setDataUnits ( "Count" );
	    }
	    else if ( statisticType == TSStatisticType.COUNT_LT ) {
	        iterateForward = true;
	        testType = TestType.LT;
	        yearts.setDescription ( "Count of values < " + testValue );
	        searchStartString = "01-01";  // Always default to first of year
	        yearts.setDataUnits ( "Count" );
	    }
	    else if ( statisticType == TSStatisticType.DAY_OF_FIRST_GE ) {
	        iterateForward = true;
	        testType = TestType.GE;
	        yearts.setDescription ( "Day of year for first value >= " + testValue );
	        yearts.setDataUnits ( "DayOfYear" );
	    }
	    else if ( statisticType == TSStatisticType.DAY_OF_FIRST_GT ) {
	        iterateForward = true;
	        testType = TestType.GT;
	        yearts.setDescription ( "Day of year for first value > " + testValue );
	        yearts.setDataUnits ( "DayOfYear" );
	    }
	    else if ( statisticType == TSStatisticType.DAY_OF_FIRST_LE ) {
	        iterateForward = true;
	        testType = TestType.LE;
	        yearts.setDescription ( "Day of year for first value <= " + testValue );
	        yearts.setDataUnits ( "DayOfYear" );
	    }
	    else if ( statisticType == TSStatisticType.DAY_OF_FIRST_LT ) {
	        iterateForward = true;
	        testType = TestType.LT;
	        yearts.setDescription ( "Day of year for first value < " + testValue );
	        yearts.setDataUnits ( "DayOfYear" );
	    }
	    else if ( statisticType == TSStatisticType.DAY_OF_LAST_GE ) {
	        iterateForward = false;
	        testType = TestType.GE;
	        yearts.setDescription ( "Day of year for last value >= " + testValue );
	        yearts.setDataUnits ( "DayOfYear" );
	    }
	    else if ( statisticType == TSStatisticType.DAY_OF_LAST_GT ) {
	        iterateForward = false;
	        testType = TestType.GT;
	        yearts.setDescription ( "Day of year for last value > " + testValue );
	        yearts.setDataUnits ( "DayOfYear" );
	    }
	    else if ( statisticType == TSStatisticType.DAY_OF_LAST_LE ) {
	        iterateForward = false;
	        testType = TestType.LE;
	        yearts.setDescription ( "Day of year for last value <= " + testValue );
	        yearts.setDataUnits ( "DayOfYear" );
	    }
	    else if ( statisticType == TSStatisticType.DAY_OF_LAST_LT ) {
	        iterateForward = false;
	        testType = TestType.LT;
	        yearts.setDescription ( "Day of year for last value < " + testValue );
	        yearts.setDataUnits ( "DayOfYear" );
	    }
	    // TODO SAM 2005-09-28 Need to decide if iteration direction and SearchStart should be a
	    // parameter for max and min
	    else if ( statisticType == TSStatisticType.DAY_OF_MAX ) {
	        iterateForward = true;
	        testType = TestType.MAX;
	        yearts.setDescription ( "Day of year for maximum value" );
	        searchStartString = "01-01";  // Always default to first of year
	        yearts.setDataUnits ( "DayOfYear" );
	    }
	    else if ( statisticType == TSStatisticType.DAY_OF_MIN ) {
	        iterateForward = true;
	        testType = TestType.MIN;
	        yearts.setDescription ( "Day of year for minimum value" );
	        searchStartString = "01-01";  // Always default to first of year
	        yearts.setDataUnits ( "DayOfYear" );
	    }
	    else if ( statisticType == TSStatisticType.MAX ) {
	        iterateForward = true;
	        testType = TestType.MAX;
	        yearts.setDescription ( "Maximum value" );
	        searchStartString = "01-01";  // Always default to first of year
	        yearts.setDataUnits ( ts.getDataUnits() );
	    }
	    else if ( statisticType == TSStatisticType.MEAN ) {
	        iterateForward = true;
	        testType = TestType.ACCUMULATE;
	        yearts.setDescription ( "Mean value" );
	        searchStartString = "01-01";  // Always default to first of year
	        yearts.setDataUnits ( ts.getDataUnits() );
	        //Statistic_isCount = true;   // Needed for computations
	    }
	    else if ( statisticType == TSStatisticType.MEDIAN ) {
	        iterateForward = true;
	        testType = TestType.SAMPLE;
	        yearts.setDescription ( "Median value" );
	        searchStartString = "01-01";  // Always default to first of year
	        yearts.setDataUnits ( ts.getDataUnits() );
	        //Statistic_isCount = true;   // Needed for computations
	    }
	    else if ( statisticType == TSStatisticType.MIN ) {
	        iterateForward = true;
	        testType = TestType.MIN;
	        yearts.setDescription ( "Minimum value" );
	        searchStartString = "01-01";  // Always default to first of year
	        yearts.setDataUnits ( ts.getDataUnits() );
	    }
	    else if ( statisticType == TSStatisticType.TOTAL ) {
	        iterateForward = true;
	        testType = TestType.ACCUMULATE;
	        yearts.setDescription ( "Total value" );
	        searchStartString = "01-01";  // Always default to first of year
	        yearts.setDataUnits ( ts.getDataUnits() );
	        //Statistic_isCount = true;   // Needed for computation checks
	    }
	    else {
	        message = "Unknown statistic (" + statisticType + ").";
	        Message.printWarning ( 3, routine, message );
	        throw new InvalidParameterException ( message );
	    }
	
	    // Loop through data, starting at the front of the time series...
	    DateTime date = null;
	    DateTime dateSearch = null; // DateTime corresponding to SearchStart for a particular year
	    if ( iterateForward ) {
	        date = new DateTime ( start );
	    }
	    else {
	        date = new DateTime ( end );
	    }
	    int yearPrev = date.getYear();
	    int year = 0;
	    TSIterator tsi = null;
	    try {
	        tsi = ts.iterator();
	    }
	    catch ( Exception e ) {
	        throw new RuntimeException ( "Error creating time series iterator (" + e + ").");
	    }
	    TSData data = null;
	    DateTime AnalysisWindowEndInYear_DateTime = null; // End of analysis window in a year
	    boolean needToAnalyze = true; // Need to analyze value for current year
	    boolean firstInterval = true;
	    int missingCount = 0; // missing count in a year
	    int nonMissingCount = 0; // non-missing count in a year
	    int gap = 0; // Number of missing values in a gap at the end of the period.
	    boolean endOfData = false; // Use to indicate end of data because
	                    // checking for data == null directly can't be done with SearchStart logic.
	    while ( true ) {
	        if ( iterateForward ) {
	            // First call will initialize and return first point.
	            data = tsi.next();  
	        }
	        else {
	            // First call will initialize and return last point.
	            data = tsi.previous();  
	        }
	        if ( data != null ) {
	            date = tsi.getDate();
	            year = date.getYear();
	        }
	        else {
	            endOfData = true;
	        }
	        if ( (year != yearPrev) || // New year so save previous year's data value.
	            (endOfData) ||    // End of data so save previous year's data value.
	            firstInterval ) {  // First interval so initialize (but do not save).
	            // New year or last interval so save the results from the previous interval analysis...
	            if ( !firstInterval ) {
	                yearDate.setYear ( yearPrev );
	                if ( statisticIsCount && yearts.isDataMissing(yearValue) &&
	                    okToSetYearStatistic(missingCount,nonMissingCount,allowMissingCount,minimumSampleSize) ) {
	                    // Never assigned a count but missing data were not an issue so assign the value to 0.
	                    yearValue = 0.0;
	                }
	                // Now re-check to see if the value should be set (not missing)...
	                if ( !yearts.isDataMissing(yearValue) ) {
	                    // Have a value to assign to the statistic...
	                    if ( statisticType == TSStatisticType.TOTAL ) {
	                        if ( !yearts.isDataMissing(yearValue) &&
	                            okToSetYearStatistic(missingCount,nonMissingCount,allowMissingCount,minimumSampleSize)) {
	                            if ( Message.isDebugOn ) {
	                                Message.printDebug ( dl, routine, "Setting " + yearDate + " year value=" + yearValue );
	                            }
	                            yearts.setDataValue ( yearDate, yearValue );
	                        }
	                    }
	                    else if ( statisticType == TSStatisticType.MEAN ) {
	                        if ( !yearts.isDataMissing(yearValue) &&
	                            okToSetYearStatistic(missingCount,nonMissingCount,allowMissingCount,minimumSampleSize)) {
	                            yearValue = yearValue/(double)nonMissingCount;
	                            if ( Message.isDebugOn ) {
	                                Message.printDebug ( dl, routine, "Setting " + yearDate + " year value=" + yearValue );
	                            }
	                            yearts.setDataValue ( yearDate, yearValue );
	                        }
	                    }
	                    else {
	                        // Simple assignment of a statistic.
	                        yearts.setDataValue ( yearDate, yearValue );
	                        if ( Message.isDebugOn ) {
	                            Message.printDebug ( dl, routine, "Setting value for "+ yearPrev + " to " + yearValue );
	                        }
	                    }
	                }
	            }
	            if ( endOfData ) {
	                // All data have been processed.
	                break;
	            }
	            // Do the following for the first interval or if a new year has started...
	            // Initialize for next processing interval...
	            firstInterval = false; // Other checks will now control
	            yearValue = yearts.getMissing();
	            extremeValue = yearts.getMissing();
	            missingCount = 0;
	            nonMissingCount = 0;
	            yearPrev = year;
	            needToAnalyze = true;
	            // Adjust the starting point if necessary.  Find the
	            // nearest value later than or equal to the search start...
	            // FIXME SAM 2008-02-05 Need to phase out SearchStart and just use the analysis window.
	            // For now use the search start to be the start of the search.
	            if ( (searchStart != null) || (analysisWindowStart != null) ) {
	                dateSearch = new DateTime ( date );
	                if ( searchStart != null ) {
	                    dateSearch.setMonth( searchStart.getMonth());
	                    dateSearch.setDay ( searchStart.getDay());
	                    if (Message.isDebugOn) {
	                        Message.printDebug ( dl, routine, "Will start processing in year on SearchStart: " + dateSearch );
	                    }
	                }
	                if ( analysisWindowStart != null ) {
	                    // The AnalysisWindow takes precedence.
	                    dateSearch.setMonth( analysisWindowStart.getMonth());
	                    dateSearch.setDay ( analysisWindowStart.getDay());
	                    // Also set the end date in the year to include.
	                    AnalysisWindowEndInYear_DateTime = new DateTime(dateSearch);
	                    AnalysisWindowEndInYear_DateTime.setMonth( analysisWindowEnd.getMonth());
	                    AnalysisWindowEndInYear_DateTime.setDay ( analysisWindowEnd.getDay());
	                    if (Message.isDebugOn) {
	                        Message.printDebug ( dl, routine,
	                        "Will start processing in year on AnalysisWindowStart: " + dateSearch );
	                        Message.printDebug ( dl, routine,
	                        "Will end processing in year on AnalysisWindowEnd: " + AnalysisWindowEndInYear_DateTime );
	                    }
	                }
	                data = tsi.goTo ( dateSearch, false );
	                if ( data == null ) {
	                    // Did not find the requested starting date so must have run out of data.
	                    // The original date still applies in some cases.
	                    // Also evaluate for missing data if a regular time series.
	                    if (Message.isDebugOn) {
	                        Message.printDebug ( dl, routine, "Did not find search start using " + dateSearch );
	                    }
	                    if ( isRegular ) {
	                        // Need to skip over the end period to a date in the period,
	                        // keeping track of the missing count...
	                        if ( iterateForward ) {
	                            if ( dateSearch.greaterThan( date) ) {
	                                // Ran out of data at end...
	                                gap = -1;
	                            }
	                            else {
	                                // Not enough data before search start...
	                                gap = TimeUtil.getNumIntervals( TimeUtil.min(dateSearch,start),
	                                TimeUtil.max( dateSearch, start),
	                                interval_base, interval_mult );
	                            }
	                        }
	                        else {
	                            // Iterate backward
	                            if ( dateSearch.lessThan( start) ) {
	                                // Ran out of data at end...
	                                gap = -1;
	                            }
	                            else {
	                                // Not enough data before search start...
	                                gap = TimeUtil.getNumIntervals( TimeUtil.min( end, dateSearch),
	                                TimeUtil.max( end, dateSearch),
	                                interval_base, interval_mult );
	                            }
	                        }
	                        if ( gap >= 0 ) {
	                            if (Message.isDebugOn) {
	                                Message.printDebug ( dl, routine,
	                                "Found " + gap + " missing values between search start and data." );
	                            }
	                            missingCount += gap;
	                        }
	                        else {
	                            // Don't have enough data...
	                            needToAnalyze = false;
	                            endOfData = true;
	                            if (Message.isDebugOn) {
	                                Message.printDebug ( dl, routine, "Don't have data at end of period to analyze." );
	                            }
	                        }
	                    }
	                    // Irregular...
	                    // just process what is available.
	                    // TODO SAM 2005-09-27 Need to review this when other than Day interval data are supported.
	                }
	                else {
	                    // Able to position the iterator so reset the date of the iterator and process below...
	                    date = data.getDate();
	                }
	            }
	        }
	        // Analyze...
	        // If need_to_analyze is false, then the data can be skipped.
	        // This will occur either if the value is found or too much
	        // missing data have been found and the result cannot be used.
	        // TODO SAM 2005-09-22 Some of the following is in place because a TSIterator is
	        // being used to handle regular and irregular data.  It
	        // should be possible to jump over data but for now the brute
	        // force search is performed.
	        if ( needToAnalyze && !endOfData ) {
	            if ( (AnalysisWindowEndInYear_DateTime != null) && date.greaterThan(AnalysisWindowEndInYear_DateTime) ) {
	                // Just skip the data.
	                continue;
	            }
	            value = tsi.getDataValue ();
	            if ( Message.isDebugOn ) {
	                Message.printDebug ( dl, routine, "Processing " + date + " value=" + value +
	                        " year value (before this value)=" + yearValue );
	            }
	            // Put an initial check because the missing data count
	            // could have been set while setting the SearchStart...
	            if ( okToSetYearStatistic(missingCount,nonMissingCount,allowMissingCount,minimumSampleSize) ) {
	                // Have too much missing data to generate the statistic...
	                needToAnalyze = false;
	                Message.printDebug ( dl, "",
	                "Will not analyze year because more than " + allowMissingCount +
	                " missing values were found (" + missingCount + ") or sample size (" + nonMissingCount +
	                " ) is less than " + minimumSampleSize );
	                continue;
	            }
	            // If missing data have not been a problem so far, continue with the check...
	            if ( ts.isDataMissing ( value ) ) {
	                ++missingCount;
	                if ( okToSetYearStatistic(missingCount,nonMissingCount,allowMissingCount,minimumSampleSize) ) {
	                    // Have too much missing data to generate the statistic...
	                    needToAnalyze = false;
	                    Message.printDebug ( dl, "",
	                    "Will not analyze year because more than " + allowMissingCount +
	                    " missing values were found (" + missingCount + ") or sample size (" + nonMissingCount +
	                    ") is less than " + minimumSampleSize );
	                }
	            }
	            else {
	                // Data value is not missing so evaluate the test...
	                ++nonMissingCount;
	                if ( (testType == TestType.GE) && (value >= testValueDouble) ) {
	                    if (statisticType == TSStatisticType.COUNT_GE ){
	                        if(yearts.isDataMissing( yearValue) ) {
	                            yearValue = 1.0;
	                        }
	                        else {
	                            yearValue += 1.0;
	                        }
	                    }
	                    else if ((statisticType == TSStatisticType.DAY_OF_FIRST_GE) ||
	                        (statisticType == TSStatisticType.DAY_OF_LAST_GE) ){
	                        yearValue = TimeUtil.dayOfYear(date);
	                        needToAnalyze = false;    
	                        // Found value for the year.
	                    }
	                }
	                else if((testType == TestType.GT) && (value > testValueDouble) ) {
	                    if (statisticType == TSStatisticType.COUNT_GT ){
	                        if(yearts.isDataMissing( yearValue) ) {
	                            yearValue = 1.0;
	                        }
	                        else {
	                            yearValue += 1.0;
	                        }
	                    }
	                    else if ((statisticType == TSStatisticType.DAY_OF_FIRST_GT) ||
	                        (statisticType == TSStatisticType.DAY_OF_LAST_GT) ){
	                        yearValue = TimeUtil.dayOfYear( date);
	                        needToAnalyze = false;    
	                        // Found value for the year.
	                    }
	                }
	                else if((testType == TestType.LE) && (value <= testValueDouble) ) {
	                    if (statisticType == TSStatisticType.COUNT_LE) {
	                        if(yearts.isDataMissing( yearValue) ) {
	                            yearValue = 1.0;
	                        }
	                        else {
	                            yearValue += 1.0;
	                        }
	                    }
	                    else if ((statisticType == TSStatisticType.DAY_OF_FIRST_LE) ||
	                        (statisticType == TSStatisticType.DAY_OF_LAST_LE) ){
	                        yearValue = TimeUtil.dayOfYear( date);
	                        needToAnalyze = false;    
	                        // Found value for the year.
	                    }
	                }
	                else if((testType == TestType.LT) && (value < testValueDouble) ) {
	                    if (statisticType == TSStatisticType.COUNT_LT){
	                        if(yearts.isDataMissing( yearValue) ) {
	                            yearValue = 1.0;
	                        }
	                        else {
	                            yearValue += 1.0;
	                        }
	                    }
	                    else if ((statisticType == TSStatisticType.DAY_OF_FIRST_LT) ||
	                        (statisticType == TSStatisticType.DAY_OF_LAST_LT) ){
	                        yearValue = TimeUtil.dayOfYear( date);
	                        needToAnalyze = false;    
	                        // Found value for the year.
	                    }
	                }
	                else if ( testType == TestType.MAX ) {
	                    if(yearts.isDataMissing(extremeValue)||(value > extremeValue) ) {
	                        // Set the max...
	                        if ( statisticType == TSStatisticType.DAY_OF_MAX ) {
	                            yearValue = TimeUtil.dayOfYear( date);
	                        }
	                        else if ( statisticType == TSStatisticType.MAX ) {
	                            yearValue = value;
	                        }
	                        extremeValue = value;
	                    }
	                    // Need to continue analyzing period so do not set need_to_analyze to false.
	                }
	                else if ( testType == TestType.MIN ) {
	                    if(yearts.isDataMissing(extremeValue)||(value < extremeValue) ) {
	                        // Set the min...
	                        if ( statisticType == TSStatisticType.DAY_OF_MIN ) {
	                            yearValue = TimeUtil.dayOfYear( date);
	                        }
	                        else if ( statisticType == TSStatisticType.MIN ) {
	                            yearValue = value;
	                        }
	                        extremeValue = value;
	                    }
	                    // Need to continue analyzing period so do not set need_to_analyze to false.
	                }
	                else if( testType == TestType.ACCUMULATE ) {
	                    // Need to accumulate the value (for Mean or Total)
	                    // Accumulate into the year_value
	                    if(yearts.isDataMissing( yearValue) ) {
	                        yearValue = value;
	                    }
	                    else {
	                        yearValue += value;
	                    }
	                }
	            }
	        }
	    }
	    if ( ts.getAlias().length() > 0 ) {
	        yearts.addToGenesis ( "Created " + statisticType + " statistic time series (TestValue=" + testValue +
	            ",SearchStart=" + searchStartString + ",AllowMissingCount=" + allowMissingCount +
	            ") from input: " + ts.getAlias() );
	    }
	    else {
	        yearts.addToGenesis ( "Created " + statisticType + " statistic time series (TestValue=" + testValue +
	            ",SearchStart=" + searchStartString + ",AllowMissingCount=" + allowMissingCount +
	            ") from input: " + ts.getIdentifier() );
	    }
	}
	*/

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
	/// Set the new time series identifier. </summary>
	/// <param name="newTSID"> the new time series identifier. </param>
	private void setNewTSID(string newTSID)
	{
		__newTSID = newTSID;
	}

	/// <summary>
	/// Set the output year type. </summary>
	/// <param name="outputYearType"> output year type. </param>
	private void setOutputYearType(YearType outputYearType)
	{
		__outputYearType = outputYearType;
	}

	/// <summary>
	/// Set the search start. </summary>
	/// <param name="searchStart"> start date/time processing in a year. </param>
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
	private void setValue1(double? testValue)
	{
		__value1 = testValue;
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