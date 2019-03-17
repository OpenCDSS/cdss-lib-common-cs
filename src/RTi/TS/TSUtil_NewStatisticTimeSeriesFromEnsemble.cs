﻿using System;
using System.Collections.Generic;

// TSUtil_NewStatisticTimeSeriesFromEnsemble - compute a time series that has a statistic for each interval in the period,
// where the sample that is analyzed is taken from the same interval of each ensemble.

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

	using DistributionType = RTi.Util.Math.DistributionType;
	using MathUtil = RTi.Util.Math.MathUtil;
	using Message = RTi.Util.Message.Message;
	using DateTime = RTi.Util.Time.DateTime;

	/// <summary>
	/// Compute a time series that has a statistic for each interval in the period, where the sample that is analyzed
	/// is taken from the same interval of each ensemble.
	/// </summary>
	public class TSUtil_NewStatisticTimeSeriesFromEnsemble
	{

	/// <summary>
	/// Time series ensemble to analyze.
	/// </summary>
	private TSEnsemble __ensemble = null;

	/// <summary>
	/// Starting date/time for analysis.
	/// </summary>
	private DateTime __analysisStart = null;

	/// <summary>
	/// Ending date/time for analysis.
	/// </summary>
	private DateTime __analysisEnd = null;

	/// <summary>
	/// Description for new time series.
	/// </summary>
	private string __description = null;

	/// <summary>
	/// Starting date/time for output.
	/// </summary>
	private DateTime __outputStart = null;

	/// <summary>
	/// Ending date/time for output.
	/// </summary>
	private DateTime __outputEnd = null;

	/// <summary>
	/// New time series identifier to assign.
	/// </summary>
	private string __newTSID = null;

	/// <summary>
	/// Statistic to analyze.
	/// </summary>
	private TSStatisticType __statisticType = null;

	/// <summary>
	/// Value to use as input when evaluating statistic.
	/// </summary>
	private double? __value1;

	/// <summary>
	/// Number of missing allowed to compute sample.
	/// </summary>
	private int? __allowMissingCount = null;

	/// <summary>
	/// Minimum required sample size.
	/// </summary>
	private int? __minimumSampleSize = null;

	/// <summary>
	/// Minimal constructor to allow access to some utility methods such as getStatisticChoices().
	/// </summary>
	public TSUtil_NewStatisticTimeSeriesFromEnsemble()
	{
	}

	/// <summary>
	/// Create a new time series that contains statistics in each data value.  The period of
	/// the result is by default the same as the source.  Each value in the result corresponds to a
	/// statistic computed from a sample taken from the the same date/time in all time series in the ensemble.
	/// After constructing an instance of this class, call the
	/// newStatisticTimeSeriesFromEnsemble() method to perform the analysis. </summary>
	/// <param name="ensemble"> Time series to analyze (must be a regular interval). </param>
	/// <param name="analysisStart"> Starting date/time for analysis, in precision of the original data. </param>
	/// <param name="analysisEnd"> Ending date for analysis, in precision of the original data. </param>
	/// <param name="outputStart"> Output start date/time.
	/// If null, the period of the original time series will be output.  CURRENTLY NOT USED. </param>
	/// <param name="outputEnd"> Output end date/time.
	/// If null, the period of the original time series will be output.  CURRENTLY NOT USED. </param>
	/// <param name="newTSID"> the new time series identifier to be assigned to the time series. </param>
	/// <param name="description"> description to use for created time series </param>
	/// <param name="statisticType"> the statistic type for the output time series. </param>
	/// <param name="allowMissingCount"> the number of values allowed to be missing in the sample. </param>
	/// <param name="minimumSampleSize"> the minimum sample size to allow to compute the statistic. </param>
	/// <exception cref="Exception"> if there is an error analyzing the time series. </exception>
	public TSUtil_NewStatisticTimeSeriesFromEnsemble(TSEnsemble ensemble, DateTime analysisStart, DateTime analysisEnd, DateTime outputStart, DateTime outputEnd, string newTSID, string description, TSStatisticType statisticType, double? value1, int? allowMissingCount, int? minimumSampleSize)
	{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		string routine = this.GetType().FullName;
		string message;

		__ensemble = ensemble;
		__analysisStart = analysisStart;
		__analysisEnd = analysisEnd;
		__outputStart = outputStart;
		__outputEnd = outputEnd;
		__newTSID = newTSID;
		__description = description;
		__statisticType = statisticType;
		__value1 = value1;
		int n = getRequiredNumberOfValuesForStatistic(__statisticType);
		if (n >= 1)
		{
			if (__value1 == null)
			{
				throw new InvalidParameterException("Statistic " + __statisticType + " requires n additional input values.");
			}
		}
		__allowMissingCount = allowMissingCount;
		__minimumSampleSize = minimumSampleSize;

		if (ensemble == null)
		{
			// Nothing to do...
			message = "Null input time series";
			Message.printWarning(3, routine, message);
			throw new InvalidParameterException(message);
		}

		if (!isStatisticSupported(statisticType))
		{
			throw new InvalidParameterException("Statistic \"" + statisticType + "\" is not supported.");
		}
	}

	/// <summary>
	/// Create a time series that contains statistics in each data value.  The period of
	/// the result by default is the same as the source.  Each value in the result corresponds to a
	/// statistic computed from a sample taken from the interval value for each ensemble trace. </summary>
	/// <returns> The statistics time series. </returns>
	/// <param name="ts"> Time series to analyze (must be a regular interval). </param>
	/// <param name="AnalysisStart_DateTime"> Starting date/time for analysis, in precision of the original data. </param>
	/// <param name="AnalysisEnd_DateTime"> Ending date for analysis, in precision of the original data. </param>
	/// <param name="OutputStart_DateTime"> Output start date/time.
	/// If null, the period of the original time series will be output. </param>
	/// <param name="OutputEnd_DateTime"> Output end date/time.  If null, the entire period will be analyzed. </param>
	/// <param name="createData"> if true, actually process the data; if false, just create the empty results time series (this
	/// is appropriate when calling software is running in discovery mode to configure the workflow) </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TS newStatisticTimeSeriesFromEnsemble(boolean createData) throws Exception
	public virtual TS newStatisticTimeSeriesFromEnsemble(bool createData)
	{
		string message, routine = this.GetType().Name + ".newStatisticTimeSeriesFromEnsemble";
		int dl = 10;

		// Use the first time series in the ensemble for properties to generate the output time series.
		TSEnsemble ensemble = getTimeSeriesEnsemble();
		IList<TS> tslist = ensemble.getTimeSeriesList(false);
		TS ts = null;
		if (ensemble.size() > 0)
		{
			ts = ensemble.get(0);
		}
		DateTime analysisStart0 = getAnalysisStart();
		DateTime analysisEnd0 = getAnalysisEnd();
		DateTime outputStart0 = getOutputStart();
		DateTime outputEnd0 = getOutputEnd();
		string newTSID = getNewTSID();
		string description = getDescription();
		TSStatisticType statisticType = getStatisticType();
		double value1 = Double.NaN;
		if (getValue1() != null)
		{
			value1 = getValue1().Value;
		}
		int? allowMissingCount = getAllowMissingCount();
		int? minimumSampleSize = getMinimumSampleSize();

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Trying to create statistic time series using time series \"" + ts.getIdentifierString() + "\" as input.");
		}

		// Get valid dates for the output time series because the ones passed in may have been null...

		// The period over which to analyze the time series...
		TSLimits validDates = TSUtil.getValidPeriod(tslist, analysisStart0, analysisEnd0);
		DateTime analysisStart = new DateTime(validDates.getDate1());
		DateTime analysisEnd = new DateTime(validDates.getDate2());

		// The period to create the output statistic time series...
		validDates = TSUtil.getValidPeriod(ts, outputStart0, outputEnd0);
		DateTime outputStart = new DateTime(validDates.getDate1());
		DateTime outputEnd = new DateTime(validDates.getDate2());

		// Create an output time series to be filled, using the input time series for the interval...

		TS output_ts = null;
		string outputTSID = null;
		try
		{
			if ((!string.ReferenceEquals(newTSID, null)) && (newTSID.Length > 0))
			{
				// New TSID should be required by commands.
				outputTSID = newTSID;
				output_ts = TSUtil.newTimeSeries(newTSID,true);
			}
			else if (ts != null)
			{
				// TODO SAM 2012-07-17 Evaluate - unlikely code?
				outputTSID = ts.getIdentifierString();
				output_ts = TSUtil.newTimeSeries(outputTSID, true);
			}
		}
		catch (Exception)
		{
			message = "Unable to create initial new time series using identifier \"" + outputTSID + "\".";
			Message.printWarning(3, routine,message);
			throw new InvalidParameterException(message);
		}

		output_ts.addToGenesis("Initialized statistic time series for period " + outputStart + " to " + outputEnd);

		// Copying the header across from the first station is bad because the ensemble may not
		// have data from a single station.  Therefore take a little more care.
		output_ts.setDataUnits(getTimeSeriesDataUnits(ts,statisticType));
		output_ts.setDescription(getTimeSeriesDescription(ensemble,statisticType,description));

		// Reset the identifier if the user has specified it...

		try
		{
			if ((!string.ReferenceEquals(newTSID, null)) && (newTSID.Length > 0))
			{
				TSIdent tsident = new TSIdent(newTSID);
				output_ts.setIdentifier(tsident);
			}
			else
			{
				// Default is to reset the scenario to the statistic...
				// FIXME SAM 2009-10-20 probably should not allow a default
				output_ts.getIdentifier().setScenario("" + statisticType);
			}
		}
		catch (Exception)
		{
			message = "Unable to set new time series identifier \"" + newTSID + "\".";
			Message.printWarning(3, routine,message);
			throw new InvalidParameterException(message);
		}

		// Automatically sets the precision...
		Message.printStatus(2, routine, "Creating statistic time series with period " + outputStart + " to " + outputEnd);
		output_ts.setDate1(outputStart);
		output_ts.setDate2(outputEnd);

		if (!createData)
		{
			// Done processing (have an empty output time series)
			return output_ts;
		}

		// This will fill with missing data...
		output_ts.allocateDataSpace();

		// Process the statistic of interest...

		// Analyze the ensemble to get a time series with statistic for each interval...
		return computeStatisticTS(ensemble, output_ts, analysisStart, analysisEnd, statisticType, value1, allowMissingCount, minimumSampleSize);
	}

	/// <summary>
	/// Create the statistic data from another time series.  The results are saved in a time
	/// series having the interval of the input data, from January 1, 2000 to December 31, 2000 (one interval from
	/// the start of the next year).  Year 2000 is used because it was a leap year.  This allows
	/// February 29 data to be computed as a statistic, although it may not be used in the final output if the
	/// final time series period does not span a leap year.
	/// This version uses the TSUtil.toArrayForDateTime() method to extract samples, which allows more
	/// flexibility for other statistics. </summary>
	/// <param name="ts"> Time series to be analyzed. </param>
	/// <param name="analysisStart"> Start of period to analyze. </param>
	/// <param name="analysisEnd"> End of period to analyze. </param>
	/// <param name="statisticType"> Statistic to compute. </param>
	/// <param name="allowMissingCount0"> number of missing values allowed in sample to compute statistic. </param>
	/// <param name="minimumSampleSize0"> minimum sample size required to compute statistic. </param>
	/// <returns> The statistics in a single-year time series. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private TS computeStatisticTS(TSEnsemble ensemble, TS stat_ts, RTi.Util.Time.DateTime analysisStart, RTi.Util.Time.DateTime analysisEnd, TSStatisticType statisticType, System.Nullable<double> value1, System.Nullable<int> allowMissingCount0, System.Nullable<int> minimumSampleSize0) throws Exception
	private TS computeStatisticTS(TSEnsemble ensemble, TS stat_ts, DateTime analysisStart, DateTime analysisEnd, TSStatisticType statisticType, double? value1, int? allowMissingCount0, int? minimumSampleSize0)
	{
		string routine = this.GetType().Name + ".computeStatisticTS";
		// Get the controlling parameters as simple integers to simplify code
		int allowMissingCount = -1;
		if (allowMissingCount0 != null)
		{
			allowMissingCount = allowMissingCount0.Value;
		}
		int minimumSampleSize = -1;
		if (minimumSampleSize0 != null)
		{
			minimumSampleSize = minimumSampleSize0.Value;
		}
		bool isCountStatistic = getIsCountStatistic(statisticType);

		// Initialize the iterators using the analysis period...
		TSIterator tsi_stat = null;
		try
		{
			tsi_stat = stat_ts.iterator(analysisStart, analysisEnd);
		}
		catch (Exception e)
		{
			throw new Exception("Error creating time series iterator (" + e + ").");
		}
		int size = ensemble.size();
		Message.printStatus(2,routine,"Have " + size + " time series in ensemble to analyze.");
		TS ts; // Time series in the ensemble
		// To improve performance, initialize an array of time series...
		TS[] ts_array = ensemble.toArray();
		if (ts_array.Length == 0)
		{
			throw new Exception("Ensemble has 0 traces - cannot analyze statistic.");
		}
		// Now iterate through all of the traces and get data for each date/time...
		DateTime date;
		int i; // Index for time series in loop.
		double value; // Value from the input time series
		double sum_value = ts_array[0].getMissing(); // Value in the sum time series
		double[] sampleData = new double[size]; // One value from each ensemble
		int countNonMissing; // Count of sampleData that are non-missing
		int countMissing; // Count of sampleData that are missing
		Message.printStatus(2, routine, "Analyzing time series ensemble for period " + analysisStart + " to " + analysisEnd);
		while (tsi_stat.next() != null)
		{
			date = tsi_stat.getDate();
			// Loop through the time series...
			countNonMissing = 0;
			countMissing = 0;
			for (i = 0; i < size; i++)
			{
				// Loop through the time series at the date/time
				ts = ts_array[i];
				// Initialize for the list of time series - put here to make sure
				// that the missing value is consistent with the time series
				if (i == 0)
				{
					sum_value = ts.getMissing();
				}
				value = ts.getDataValue(date);
				if (ts.isDataMissing(value))
				{
					// Ignore missing data...
					++countMissing;
					continue;
				}
				else
				{
					// Save non-missing value in the sample...
					sampleData[countNonMissing++] = value;
					if (ts.isDataMissing(sum_value))
					{
						// Just assign the value
						sum_value = value;
					}
					else
					{
						// Increment the total...
						sum_value += value;
					}
				}
			}
			// Now compute the statistic from the sample if missing values are not a problem.
			if (!isCountStatistic)
			{
				// Have to check to see if enough data points are available
				if ((allowMissingCount >= 0) && (countMissing > allowMissingCount))
				{
					// Too many missing values to compute statistic
					Message.printStatus(2, routine, "Not computing time series statistic at " + date + " because number of missing values " + countMissing + " is > allowed (" + allowMissingCount + ").");
					stat_ts.setDataValue(date, stat_ts.getMissing());
					continue;
				}
				if ((minimumSampleSize >= 0) && (countNonMissing < minimumSampleSize))
				{
					// Sample size too small to compute statistic
					Message.printStatus(2, routine, "Not computing time series statistic at " + date + " because sample size " + countNonMissing + " is < minimum required (" + minimumSampleSize + ").");
					stat_ts.setDataValue(date, stat_ts.getMissing());
					continue;
				}
			}
			// Else have enough data so compute the statistic.
			// TODO SAM 2009-10-26 Will need to rework if the time series that results in the statistic is needed
			// (currently this is lost when calling the MathUtil methods).
			// TODO SAM 2009-10-26 Trying to set the result in a time series that has a shorter period could
			// generate low-level warnings, but for now allow this to occur rather than incurring the overhead
			// of checking the output period.
			if (statisticType == TSStatisticType.EXCEEDANCE_PROBABILITY)
			{
				if (countNonMissing > 0)
				{
					stat_ts.setDataValue(date, MathUtil.exceedanceProbabilityValue(countNonMissing, sampleData, value1.Value));
				}
			}
			else if (statisticType == TSStatisticType.EXCEEDANCE_PROBABILITY_10)
			{
				if (countNonMissing > 0)
				{
					stat_ts.setDataValue(date, MathUtil.exceedanceProbabilityValue(countNonMissing,sampleData,.1));
				}
			}
			else if (statisticType == TSStatisticType.EXCEEDANCE_PROBABILITY_30)
			{
				if (countNonMissing > 0)
				{
					stat_ts.setDataValue(date, MathUtil.exceedanceProbabilityValue(countNonMissing,sampleData,.3));
				}
			}
			else if (statisticType == TSStatisticType.EXCEEDANCE_PROBABILITY_50)
			{
				if (countNonMissing > 0)
				{
					stat_ts.setDataValue(date, MathUtil.exceedanceProbabilityValue(countNonMissing,sampleData,.5));
				}
			}
			else if (statisticType == TSStatisticType.EXCEEDANCE_PROBABILITY_70)
			{
				if (countNonMissing > 0)
				{
					stat_ts.setDataValue(date, MathUtil.exceedanceProbabilityValue(countNonMissing,sampleData,.7));
				}
			}
			else if (statisticType == TSStatisticType.EXCEEDANCE_PROBABILITY_90)
			{
				if (countNonMissing > 0)
				{
					stat_ts.setDataValue(date, MathUtil.exceedanceProbabilityValue(countNonMissing,sampleData,.9));
				}
			}
			else if (statisticType == TSStatisticType.GE_COUNT)
			{
				int count = 0;
				for (int isamp = 0; isamp < countNonMissing; isamp++)
				{
					if (sampleData[isamp] >= value1.Value)
					{
						++count;
					}
				}
				stat_ts.setDataValue(date, count);
			}
			else if (statisticType == TSStatisticType.GT_COUNT)
			{
				int count = 0;
				for (int isamp = 0; isamp < countNonMissing; isamp++)
				{
					if (sampleData[isamp] > value1.Value)
					{
						++count;
					}
				}
				stat_ts.setDataValue(date, count);
			}
			else if (statisticType == TSStatisticType.LT_COUNT)
			{
				int count = 0;
				for (int isamp = 0; isamp < countNonMissing; isamp++)
				{
					if (sampleData[isamp] < value1.Value)
					{
						++count;
					}
				}
				stat_ts.setDataValue(date, count);
			}
			else if (statisticType == TSStatisticType.LE_COUNT)
			{
				int count = 0;
				for (int isamp = 0; isamp < countNonMissing; isamp++)
				{
					if (sampleData[isamp] <= value1.Value)
					{
						++count;
					}
				}
				stat_ts.setDataValue(date, count);
			}
			else if (statisticType == TSStatisticType.GEOMETRIC_MEAN)
			{
				if (countNonMissing > 0)
				{
					stat_ts.setDataValue(date, MathUtil.geometricMean(countNonMissing,sampleData));
				}
			}
			else if (statisticType == TSStatisticType.MAX)
			{
				if (countNonMissing > 0)
				{
					stat_ts.setDataValue(date, MathUtil.max(countNonMissing,sampleData));
				}
			}
			else if (statisticType == TSStatisticType.MEAN)
			{
				if (countNonMissing > 0)
				{
					stat_ts.setDataValue(date, MathUtil.mean(countNonMissing,sampleData));
				}
			}
			else if (statisticType == TSStatisticType.MEDIAN)
			{
				// Remove the missing values and analyze only non-missing values
				if (countNonMissing > 0)
				{
					stat_ts.setDataValue(date, MathUtil.median(countNonMissing,sampleData));
				}
			}
			else if (statisticType == TSStatisticType.MIN)
			{
				if (countNonMissing > 0)
				{
					stat_ts.setDataValue(date, MathUtil.min(countNonMissing,sampleData));
				}
			}
			else if (statisticType == TSStatisticType.MISSING_COUNT)
			{
				stat_ts.setDataValue(date, countMissing);
			}
			else if (statisticType == TSStatisticType.MISSING_PERCENT)
			{
				stat_ts.setDataValue(date, 100.0 * countMissing / (double)(countMissing + countNonMissing));
			}
			else if (statisticType == TSStatisticType.NONEXCEEDANCE_PROBABILITY)
			{
				if (countNonMissing > 0)
				{
					stat_ts.setDataValue(date, MathUtil.nonexceedanceProbabilityValue(countNonMissing, sampleData, value1.Value));
				}
			}
			else if (statisticType == TSStatisticType.NONEXCEEDANCE_PROBABILITY_10)
			{
				if (countNonMissing > 0)
				{
					stat_ts.setDataValue(date, MathUtil.nonexceedanceProbabilityValue(countNonMissing,sampleData,.1));
				}
			}
			else if (statisticType == TSStatisticType.NONEXCEEDANCE_PROBABILITY_30)
			{
				if (countNonMissing > 0)
				{
					stat_ts.setDataValue(date, MathUtil.nonexceedanceProbabilityValue(countNonMissing,sampleData,.3));
				}
			}
			else if (statisticType == TSStatisticType.NONEXCEEDANCE_PROBABILITY_50)
			{
				if (countNonMissing > 0)
				{
					stat_ts.setDataValue(date, MathUtil.nonexceedanceProbabilityValue(countNonMissing,sampleData,.5));
				}
			}
			else if (statisticType == TSStatisticType.NONEXCEEDANCE_PROBABILITY_70)
			{
				if (countNonMissing > 0)
				{
					stat_ts.setDataValue(date, MathUtil.nonexceedanceProbabilityValue(countNonMissing,sampleData,.7));
				}
			}
			else if (statisticType == TSStatisticType.NONEXCEEDANCE_PROBABILITY_90)
			{
				if (countNonMissing > 0)
				{
					stat_ts.setDataValue(date, MathUtil.nonexceedanceProbabilityValue(countNonMissing,sampleData,.9));
				}
			}
			else if (statisticType == TSStatisticType.NONMISSING_COUNT)
			{
				stat_ts.setDataValue(date, countNonMissing);
			}
			else if (statisticType == TSStatisticType.NONMISSING_PERCENT)
			{
				stat_ts.setDataValue(date, 100.0 * countNonMissing / (double)(countMissing + countNonMissing));
			}
			else if (statisticType == TSStatisticType.SKEW)
			{
				if (countNonMissing >= 3)
				{ // Need 3 values to do calculation
					stat_ts.setDataValue(date, MathUtil.skew(countNonMissing,sampleData));
				}
			}
			else if (statisticType == TSStatisticType.STD_DEV)
			{
				if (countNonMissing >= 2)
				{ // Need 2 values to do calculation
					stat_ts.setDataValue(date, MathUtil.standardDeviation(countNonMissing,sampleData));
				}
			}
			else if (statisticType == TSStatisticType.TOTAL)
			{
				if (countNonMissing > 0)
				{
					stat_ts.setDataValue(date, sum_value);
				}
			}
		}

		// Return the result.
		return stat_ts;
	}

	/// <summary>
	/// Return the number of missing values allowed in sample. </summary>
	/// <returns> the number of missing values allowed in sample. </returns>
	public virtual int? getAllowMissingCount()
	{
		return __allowMissingCount;
	}

	/// <summary>
	/// Return the analysis end date/time. </summary>
	/// <returns> the analysis end date/time. </returns>
	public virtual DateTime getAnalysisEnd()
	{
		return __analysisEnd;
	}

	/// <summary>
	/// Return the analysis start date/time. </summary>
	/// <returns> the analysis start date/time. </returns>
	public virtual DateTime getAnalysisStart()
	{
		return __analysisStart;
	}

	/// <summary>
	/// Return the description for output time series. </summary>
	/// <returns> the description for output time series. </returns>
	public virtual string getDescription()
	{
		return __description;
	}

	/// <summary>
	/// Get the list of distributions that can be used in calculations.
	/// </summary>
	public virtual IList<DistributionType> getDistributionChoices()
	{
		IList<DistributionType> choices = new List<DistributionType>();
		choices.Add(DistributionType.EMPERICAL);
		choices.Add(DistributionType.LOG_NORMAL);
		choices.Add(DistributionType.LOG_PEARSON_TYPE3);
		choices.Add(DistributionType.NORMAL);
		choices.Add(DistributionType.SAMPLE);
		choices.Add(DistributionType.WAKEBY);
		choices.Add(DistributionType.WEIBULL);
		return choices;
	}

	/// <summary>
	/// Get the list of distributions that can be calculated. </summary>
	/// <returns> the distribution display names as strings. </returns>
	public virtual IList<string> getDistributionChoicesAsStrings()
	{
		IList<DistributionType> choices = getDistributionChoices();
		IList<string> stringChoices = new List<string>();
		for (int i = 0; i < choices.Count; i++)
		{
			stringChoices.Add("" + choices[i]);
		}
		return stringChoices;
	}

	/// <summary>
	/// Indicate whether the statistic is a count of missing or non-missing.
	/// </summary>
	private bool getIsCountStatistic(TSStatisticType statisticType)
	{
		if ((statisticType == TSStatisticType.MISSING_COUNT) || (statisticType == TSStatisticType.NONMISSING_COUNT) || (statisticType == TSStatisticType.MISSING_PERCENT) || (statisticType == TSStatisticType.NONMISSING_PERCENT))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Return the minimum sample size allowed to compute the statistic. </summary>
	/// <returns> the minimum sample size allowed to compute the statistic. </returns>
	public virtual int? getMinimumSampleSize()
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
	/// Return the output start date/time. </summary>
	/// <returns> the output start date/time. </returns>
	public virtual DateTime getOutputStart()
	{
		return __outputStart;
	}

	/// <summary>
	/// Return the output end date/time. </summary>
	/// <returns> the output end date/time. </returns>
	public virtual DateTime getOutputEnd()
	{
		return __outputEnd;
	}

	/// <summary>
	/// Return the number of values that are required to evaluate a statistic. </summary>
	/// <returns> the number of values that are required to evaluate a statistic. </returns>
	/// <param name="statisticType"> the statistic type that is being evaluated. </param>
	public virtual int getRequiredNumberOfValuesForStatistic(TSStatisticType statisticType)
	{
		// Many basic statistics do not need additional input...
		if ((statisticType == TSStatisticType.COUNT) || (statisticType == TSStatisticType.DEFICIT_MAX) || (statisticType == TSStatisticType.DEFICIT_MEAN) || (statisticType == TSStatisticType.DEFICIT_MIN) || (statisticType == TSStatisticType.DEFICIT_SEQ_LENGTH_MAX) || (statisticType == TSStatisticType.DEFICIT_SEQ_LENGTH_MEAN) || (statisticType == TSStatisticType.DEFICIT_SEQ_LENGTH_MIN) || (statisticType == TSStatisticType.DEFICIT_SEQ_MAX) || (statisticType == TSStatisticType.DEFICIT_SEQ_MEAN) || (statisticType == TSStatisticType.DEFICIT_SEQ_MIN) || (statisticType == TSStatisticType.EXCEEDANCE_PROBABILITY_10) || (statisticType == TSStatisticType.EXCEEDANCE_PROBABILITY_30) || (statisticType == TSStatisticType.EXCEEDANCE_PROBABILITY_50) || (statisticType == TSStatisticType.EXCEEDANCE_PROBABILITY_70) || (statisticType == TSStatisticType.EXCEEDANCE_PROBABILITY_90) || (statisticType == TSStatisticType.GEOMETRIC_MEAN) || (statisticType == TSStatisticType.LAG1_AUTO_CORRELATION) || (statisticType == TSStatisticType.LAST) || (statisticType == TSStatisticType.LAST_NONMISSING) || (statisticType == TSStatisticType.MAX) || (statisticType == TSStatisticType.MEAN) || (statisticType == TSStatisticType.MEDIAN) || (statisticType == TSStatisticType.MIN) || (statisticType == TSStatisticType.MISSING_COUNT) || (statisticType == TSStatisticType.MISSING_PERCENT) || (statisticType == TSStatisticType.MISSING_SEQ_LENGTH_MAX) || (statisticType == TSStatisticType.NONEXCEEDANCE_PROBABILITY_10) || (statisticType == TSStatisticType.NONEXCEEDANCE_PROBABILITY_30) || (statisticType == TSStatisticType.NONEXCEEDANCE_PROBABILITY_50) || (statisticType == TSStatisticType.NONEXCEEDANCE_PROBABILITY_70) || (statisticType == TSStatisticType.NONEXCEEDANCE_PROBABILITY_90) || (statisticType == TSStatisticType.NONMISSING_COUNT) || (statisticType == TSStatisticType.NONMISSING_PERCENT) || (statisticType == TSStatisticType.SKEW) || (statisticType == TSStatisticType.STD_DEV) || (statisticType == TSStatisticType.SURPLUS_SEQ_LENGTH_MAX) || (statisticType == TSStatisticType.SURPLUS_SEQ_LENGTH_MEAN) || (statisticType == TSStatisticType.SURPLUS_SEQ_LENGTH_MIN) || (statisticType == TSStatisticType.SURPLUS_MAX) || (statisticType == TSStatisticType.SURPLUS_MEAN) || (statisticType == TSStatisticType.SURPLUS_MIN) || (statisticType == TSStatisticType.SURPLUS_SEQ_MAX) || (statisticType == TSStatisticType.SURPLUS_SEQ_MEAN) || (statisticType == TSStatisticType.SURPLUS_SEQ_MIN) || (statisticType == TSStatisticType.TOTAL) || (statisticType == TSStatisticType.TREND_OLS) || (statisticType == TSStatisticType.VARIANCE))
		{
			return 0;
		}
		else if ((statisticType == TSStatisticType.EXCEEDANCE_PROBABILITY) || (statisticType == TSStatisticType.GE_COUNT) || (statisticType == TSStatisticType.GT_COUNT) || (statisticType == TSStatisticType.LE_COUNT) || (statisticType == TSStatisticType.LT_COUNT) || (statisticType == TSStatisticType.NONEXCEEDANCE_PROBABILITY))
		{
			return 1;
		}
		else
		{
			string message = "Requested statistic is not recognized to get number of required values: " + statisticType;
			string routine = "TSUtil_NewStatisticTimeSeriesFromEnsemble.getRequiredNumberOfValuesForStatistic";
			Message.printWarning(3, routine, message);
			throw new InvalidParameterException(message);
		}
	}

	/// <summary>
	/// Get the list of statistics that can be performed.
	/// </summary>
	public virtual IList<TSStatisticType> getStatisticChoices()
	{
		// TODO SAM 2009-10-14 Need to enable more statistics
		IList<TSStatisticType> choices = new List<TSStatisticType>();
		choices.Add(TSStatisticType.EXCEEDANCE_PROBABILITY); // Requires Value1
		choices.Add(TSStatisticType.EXCEEDANCE_PROBABILITY_10);
		choices.Add(TSStatisticType.EXCEEDANCE_PROBABILITY_30);
		choices.Add(TSStatisticType.EXCEEDANCE_PROBABILITY_50);
		choices.Add(TSStatisticType.EXCEEDANCE_PROBABILITY_70);
		choices.Add(TSStatisticType.EXCEEDANCE_PROBABILITY_90);
		choices.Add(TSStatisticType.GE_COUNT);
		choices.Add(TSStatisticType.GT_COUNT);
		choices.Add(TSStatisticType.GEOMETRIC_MEAN);
		choices.Add(TSStatisticType.LE_COUNT);
		choices.Add(TSStatisticType.LT_COUNT);
		choices.Add(TSStatisticType.MAX);
		choices.Add(TSStatisticType.MEAN);
		choices.Add(TSStatisticType.MEDIAN);
		choices.Add(TSStatisticType.MIN);
		choices.Add(TSStatisticType.MISSING_COUNT);
		choices.Add(TSStatisticType.MISSING_PERCENT);
		choices.Add(TSStatisticType.NONEXCEEDANCE_PROBABILITY); // Requires Value1
		choices.Add(TSStatisticType.NONEXCEEDANCE_PROBABILITY_10);
		choices.Add(TSStatisticType.NONEXCEEDANCE_PROBABILITY_30);
		choices.Add(TSStatisticType.NONEXCEEDANCE_PROBABILITY_50);
		choices.Add(TSStatisticType.NONEXCEEDANCE_PROBABILITY_70);
		choices.Add(TSStatisticType.NONEXCEEDANCE_PROBABILITY_90);
		choices.Add(TSStatisticType.NONMISSING_COUNT);
		choices.Add(TSStatisticType.NONMISSING_PERCENT);
		choices.Add(TSStatisticType.RANK_ASCENDING);
		choices.Add(TSStatisticType.RANK_DESCENDING);
		choices.Add(TSStatisticType.SKEW);
		choices.Add(TSStatisticType.STD_DEV);
		//choices.add ( TSStatisticType.VARIANCE );
		choices.Add(TSStatisticType.TOTAL);
		return choices;
	}

	/// <summary>
	/// Get the list of statistics that can be performed. </summary>
	/// <returns> the statistic display names as strings. </returns>
	public virtual IList<string> getStatisticChoicesAsStrings()
	{
		IList<TSStatisticType> choices = getStatisticChoices();
		IList<string> stringChoices = new List<string>();
		for (int i = 0; i < choices.Count; i++)
		{
			stringChoices.Add("" + choices[i]);
		}
		return stringChoices;
	}

	/// <summary>
	/// Return the name of the statistic being calculated. </summary>
	/// <returns> the name of the statistic being calculated. </returns>
	public virtual TSStatisticType getStatisticType()
	{
		return __statisticType;
	}

	/// <summary>
	/// Determine the data units for the new time series.
	/// </summary>
	private string getTimeSeriesDataUnits(TS inputts, TSStatisticType statisticType)
	{
		if (inputts == null)
		{
			// Possible, especially when in discovery mode in TSTool
			return "";
		}
		else if ((statisticType == TSStatisticType.EXCEEDANCE_PROBABILITY) || (statisticType == TSStatisticType.EXCEEDANCE_PROBABILITY_10) || (statisticType == TSStatisticType.EXCEEDANCE_PROBABILITY_30) || (statisticType == TSStatisticType.EXCEEDANCE_PROBABILITY_50) || (statisticType == TSStatisticType.EXCEEDANCE_PROBABILITY_70) || (statisticType == TSStatisticType.EXCEEDANCE_PROBABILITY_90) || (statisticType == TSStatisticType.GEOMETRIC_MEAN) || (statisticType == TSStatisticType.MAX) || (statisticType == TSStatisticType.MEAN) || (statisticType == TSStatisticType.MEDIAN) || (statisticType == TSStatisticType.MIN) || (statisticType == TSStatisticType.NONEXCEEDANCE_PROBABILITY) || (statisticType == TSStatisticType.NONEXCEEDANCE_PROBABILITY_10) || (statisticType == TSStatisticType.NONEXCEEDANCE_PROBABILITY_30) || (statisticType == TSStatisticType.NONEXCEEDANCE_PROBABILITY_50) || (statisticType == TSStatisticType.NONEXCEEDANCE_PROBABILITY_70) || (statisticType == TSStatisticType.NONEXCEEDANCE_PROBABILITY_90) || (statisticType == TSStatisticType.STD_DEV) || (statisticType == TSStatisticType.TOTAL))
		{
			// Use the units from the original time series
			return inputts.getDataUnits();
		}
		else if ((statisticType == TSStatisticType.GE_COUNT) || (statisticType == TSStatisticType.GT_COUNT) || (statisticType == TSStatisticType.LE_COUNT) || (statisticType == TSStatisticType.LT_COUNT) || (statisticType == TSStatisticType.MISSING_COUNT) || (statisticType == TSStatisticType.NONMISSING_COUNT))
		{
			return "Count";
		}
		else if ((statisticType == TSStatisticType.MISSING_PERCENT) || (statisticType == TSStatisticType.NONMISSING_PERCENT))
		{
			return "Percent";
		}
		return "";
	}

	/// <summary>
	/// Determine the description for the new time series. </summary>
	/// <param name="ensemble"> ensemble being processed </param>
	/// <param name="statisticType"> statistic type </param>
	/// <param name="description"> description (if specified, use it) </param>
	private string getTimeSeriesDescription(TSEnsemble ensemble, TSStatisticType statisticType, string description)
	{
		if ((!string.ReferenceEquals(description, null)) && description.Length > 0)
		{
			return description;
		}
		// If all the ensemble time series have the same description, then it is a true ensemble so
		// use the same description with the statistic
		bool same = true;
		string descPrev = "xxx";
		string desc = "";
		foreach (TS ts in ensemble.getTimeSeriesList(false))
		{
			desc = ts.getDescription();
			if (!descPrev.Equals("xxx") && !descPrev.Equals(desc))
			{
				same = false;
				break;
			}
			descPrev = desc;
		}
		if (same)
		{
			return desc + "," + statisticType;
		}
		// Otherwise, use the ensemble name with the statistic
		string ensembleName = ensemble.getEnsembleName();
		if (ensembleName.Equals(""))
		{
			return "" + statisticType;
		}
		else
		{
			return ensembleName + "," + statisticType;
		}
	}

	/// <summary>
	/// Return the time series being analyzed. </summary>
	/// <returns> the time series being analyzed. </returns>
	public virtual TSEnsemble getTimeSeriesEnsemble()
	{
		return __ensemble;
	}

	/// <summary>
	/// Return the value1, used to provide additional data to compute statistic. </summary>
	/// <returns> value1 </returns>
	public virtual double? getValue1()
	{
		return __value1;
	}

	/// <summary>
	/// Indicate whether a statistic is supported.
	/// </summary>
	public virtual bool isStatisticSupported(TSStatisticType statisticType)
	{
		IList<TSStatisticType> choices = getStatisticChoices();
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
	/// Indicate whether a statistic is supported.
	/// </summary>
	public virtual bool isStatisticSupportedForDistribution(TSStatisticType statisticType, DistributionType distType)
	{
		if (distType == null)
		{
			return true;
		}
		bool isStatSupported = isStatisticSupported(statisticType);
		if (!isStatSupported)
		{
			return false; // Should have been checked elsewhere
		}
		if ((statisticType == TSStatisticType.EXCEEDANCE_PROBABILITY) || (statisticType == TSStatisticType.EXCEEDANCE_PROBABILITY_10) || (statisticType == TSStatisticType.EXCEEDANCE_PROBABILITY_30) || (statisticType == TSStatisticType.EXCEEDANCE_PROBABILITY_50) || (statisticType == TSStatisticType.EXCEEDANCE_PROBABILITY_70) || (statisticType == TSStatisticType.EXCEEDANCE_PROBABILITY_90) || (statisticType == TSStatisticType.NONEXCEEDANCE_PROBABILITY) || (statisticType == TSStatisticType.NONEXCEEDANCE_PROBABILITY_10) || (statisticType == TSStatisticType.NONEXCEEDANCE_PROBABILITY_30) || (statisticType == TSStatisticType.NONEXCEEDANCE_PROBABILITY_50) || (statisticType == TSStatisticType.NONEXCEEDANCE_PROBABILITY_70) || (statisticType == TSStatisticType.NONEXCEEDANCE_PROBABILITY_90) || (statisticType == TSStatisticType.PLOTTING_POSITION_ASCENDING) || (statisticType == TSStatisticType.PLOTTING_POSITION_DESCENDING))
		{
			return true;
		}
		return false;
	}

	}

}