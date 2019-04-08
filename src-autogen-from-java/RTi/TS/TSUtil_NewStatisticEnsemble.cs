using System;
using System.Collections.Generic;

// TSUtil_NewStatisticEnsemble - compute an ensemble with time series that each contain a statistic computed from the input sample time series.

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

	/// <summary>
	/// Compute an ensemble with time series that each contain a statistic computed from the input sample time series.
	/// This is intended to compute multiple output time series, for example CountLE(A), CountLE(B), etc.  Computing a "simple"
	/// statistic from an ensemble can be done using the TSUtil_NewStatisticTimeSeriesFromEnsemble class.
	/// </summary>
	public class TSUtil_NewStatisticEnsemble
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
	/// List of time series to analyze.
	/// </summary>
	private IList<TS> __tslist = null;

	/// <summary>
	/// New ensemble identifier to assign.
	/// </summary>
	private string __newEnsembleID = null;

	/// <summary>
	/// New ensemble name.
	/// </summary>
	private string __newEnsembleName = null;

	/// <summary>
	/// Alias for new time series.
	/// </summary>
	private string __alias = null;

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
	/// Test values used when analyzing the statistic.
	/// </summary>
	private double[] __testValues = null;

	/// <summary>
	/// Number of missing allowed to compute sample (default is no limit).
	/// </summary>
	private int __allowMissingCount = -1;

	/// <summary>
	/// Minimum required sample size (default is no limit).
	/// </summary>
	private int __minimumSampleSize = -1;

	/// <summary>
	/// Construct the analysis object with required input.  Values will be checked for validity.
	/// Execute the newStatisticYearTS() method to perform the analysis. </summary>
	/// <param name="tslist"> list of time series to analyze </param>
	/// <param name="newEnsembleID"> new ensemble identifier </param>
	/// <param name="newEnsembleName"> new ensemble name </param>
	/// <param name="alias"> the alias to be assigned to each created time series </param>
	/// <param name="newTSID"> the new time series identifier to be assigned to each created time series </param>
	/// <param name="statisticType"> the statistic type for the output time series </param>
	/// <param name="testValues"> test value array (e.g., threshold value) </param>
	/// <param name="allowMissingCount"> the number of values allowed to be missing in the sample </param>
	/// <param name="minimumSampleSize"> the minimum sample size to allow to compute the statistic </param>
	/// <param name="analysisStart"> Starting date/time for analysis, in precision of the original data </param>
	/// <param name="analysisEnd"> Ending date for analysis, in precision of the original data </param>
	public TSUtil_NewStatisticEnsemble(IList<TS> tslist, string newEnsembleID, string newEnsembleName, string alias, string newTSID, TSStatisticType statisticType, double[] testValues, int? allowMissingCount, int? minimumSampleSize, DateTime analysisStart, DateTime analysisEnd)
	{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		string routine = this.GetType().FullName;
		string message;

		if (tslist == null)
		{
			// Nothing to do...
			message = "Time series list is null - cannot calculate statistic ensemble.";
			Message.printWarning(3, routine, message);
			throw new System.ArgumentException(message);
		}
		else if (tslist.Count == 0)
		{
			// Nothing to do...
			message = "Time series list is empty - cannot calculate statistic ensemble.";
			Message.printWarning(3, routine, message);
			throw new System.ArgumentException(message);
		}
		setTimeSeriesList(tslist);
		setNewEnsembleID(newEnsembleID);
		setNewEnsembleName(newEnsembleName);
		setAlias(alias);
		setNewTSID(newTSID);
		try
		{
			// TODO SAM 
		}
		catch (Exception e)
		{
			throw new System.ArgumentException("New time series identifier \"" + newTSID + "\" is invalid (" + e + ").");
		}
		setStatisticType(statisticType);
		setTestValues(testValues);
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
		if (!isStatisticSupported(statisticType))
		{
			throw new System.ArgumentException("Statistic \"" + statisticType + "\" is not supported.");
		}
	}

	/// <summary>
	/// Calculate the number of values in the sample that meet the statistic criteria </summary>
	/// <param name="countNonMissing"> number of nonmissing values in the start of the sampleData array </param>
	/// <param name="sampleData"> the sample data array to analyze </param>
	/// <param name="testValue"> the test value to use when evaluating the statistic </param>
	/// <param name="testType"> the type of test when comparing a sample value and test value </param>
	/// <returns> the count of sample values that meet the statistic criteria </returns>
	private int calculateCount(int countNonMissing, double[] sampleData, double testValue, TestType testType)
	{
		int count = 0;
		for (int i = 0; i < countNonMissing; i++)
		{
			if ((testType == TestType.GE) && (sampleData[i] >= testValue))
			{
				++count;
			}
			else if ((testType == TestType.GT) && (sampleData[i] > testValue))
			{
				++count;
			}
			else if ((testType == TestType.LE) && (sampleData[i] <= testValue))
			{
				++count;
			}
			else if ((testType == TestType.LT) && (sampleData[i] < testValue))
			{
				++count;
			}
		}
		return count;
	}

	/// <summary>
	/// Process a list of input time series to create output statistic time series. </summary>
	/// <param name="tslist"> time series to analyze </param>
	/// <param name="stattsList"> list of output statistic time series, previously constructed and memory allocated </param>
	/// <param name="statisticType"> statistic to calculate </param>
	/// <param name="testValues"> one or more numbers to test against for some statistics (e.g., COUNT_LE). </param>
	/// <param name="analysisStart"> Start of the analysis (precision matching tslist) </param>
	/// <param name="analysisEnd"> End of the analysis (precision matching tslist) </param>
	/// <param name="allowMissingCount"> the number of missing values allowed in input and still compute the statistic. </param>
	/// <param name="minimumSampleSize"> the minimum number of values required in input to compute the statistic. </param>
	private void calculateStatistic(IList<TS> tslist, IList<TS> stattsList, TSStatisticType statisticType, double[] testValues, DateTime analysisStart, DateTime analysisEnd, int allowMissingCount, int minimumSampleSize)
	{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		string routine = this.GetType().FullName + ".calculateStatistic";
		int size = tslist.Count;
		Message.printStatus(2,routine,"Have " + size + " time series to analyze.");
		TS ts; // Time series in the ensemble
		// To improve performance, initialize an array of time series...
		TS[] ts_array = tslist.ToArray();
		if (ts_array.Length == 0)
		{
			throw new Exception("Ensemble has 0 traces - cannot analyze statistic.");
		}
		DateTime date = new DateTime(analysisStart);
		int i; // Index for time series in loop.
		double value; // Value from the input time series
		double[] sampleData = new double[size]; // One value from each ensemble
		int countNonMissing; // Count of sampleData that are non-missing
		int countMissing; // Count of sampleData that are missing
		Message.printStatus(2, routine, "Analyzing time series list for period " + analysisStart + " to " + analysisEnd);
		// Loop through the analysis period
		int intervalBase = ts_array[0].getDataIntervalBase();
		int intervalMult = ts_array[0].getDataIntervalMult();
		int statCount;
		bool statisticIsCount = isStatisticCount(statisticType);
		bool statisticIsPercent = isStatisticPercent(statisticType);
		TestType testType = getStatisticTestType(statisticType);
		for (; date.lessThanOrEqualTo(analysisEnd); date.addInterval(intervalBase, intervalMult))
		{
			// Loop through the time series for the date/time...
			countNonMissing = 0;
			countMissing = 0;
			for (i = 0; i < size; i++)
			{
				ts = ts_array[i];
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
				}
			}
			// Now analyze the statistic for each output time series
			TS statts;
			for (i = 0; i < testValues.Length; i++)
			{
				statts = stattsList[i];
				// Now compute the statistic from the sample if missing values are not a problem.
				if ((allowMissingCount >= 0) && (countMissing > allowMissingCount))
				{
					// Too many missing values to compute statistic
					Message.printStatus(2, routine, "Not computing time series statistic at " + date + " because number of missing values " + countMissing + " is > allowed (" + allowMissingCount + ").");
					continue;
				}
				if ((minimumSampleSize >= 0) && (countNonMissing < minimumSampleSize))
				{
					// Sample size too small to compute statistic
					Message.printStatus(2, routine, "Not computing time series statistic at " + date + " because sample size " + countNonMissing + " is < minimum required (" + minimumSampleSize + ").");
					continue;
				}
				// Else have enough data so compute the statistic.
				statCount = calculateCount(countNonMissing,sampleData,testValues[i],testType);
				if (statisticIsCount)
				{
					statts.setDataValue(date, (double)statCount);
				}
				else if (statisticIsPercent && (countNonMissing > 0))
				{
					// Default is percent of non-missing stations
					// TODO SAM 2012-07-13 Evaluate whether should be percent of total stations?
					statts.setDataValue(date, 100.0 * (double)statCount / (double)countNonMissing);
				}
			}
		}
	}

	/// <summary>
	/// Return the alias for new time series. </summary>
	/// <returns> the alias for new time series. </returns>
	private string getAlias()
	{
		return __alias;
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
	/// Return the minimum sample size allowed to compute the statistic. </summary>
	/// <returns> the minimum sample size allowed to compute the statistic. </returns>
	private int? getMinimumSampleSize()
	{
		return __minimumSampleSize;
	}

	/// <summary>
	/// Return the new ensemble identifier </summary>
	/// <returns> the new ensemble identifier </returns>
	private string getNewEnsembleID()
	{
		return __newEnsembleID;
	}

	/// <summary>
	/// Return the new ensemble name </summary>
	/// <returns> the new ensemble name </returns>
	private string getNewEnsembleName()
	{
		return __newEnsembleName;
	}

	/// <summary>
	/// Return the time series identifier for the new time series. </summary>
	/// <returns> the time series identifier for the new time series. </returns>
	private string getNewTSID()
	{
		return __newTSID;
	}

	/// <summary>
	/// Return a list of statistic choices.
	/// These strings are suitable for listing in a user interface.  The statistics are
	/// listed in ascending alphabetical order.
	/// </summary>
	public static IList<TSStatisticType> getStatisticChoices()
	{
		IList<TSStatisticType> statistics = new List<TSStatisticType>();
		// Add in alphabetical order
		statistics.Add(TSStatisticType.GE_COUNT);
		statistics.Add(TSStatisticType.GE_PERCENT);
		statistics.Add(TSStatisticType.GT_COUNT);
		statistics.Add(TSStatisticType.GT_PERCENT);
		statistics.Add(TSStatisticType.LE_COUNT);
		statistics.Add(TSStatisticType.LE_PERCENT);
		statistics.Add(TSStatisticType.LT_COUNT);
		statistics.Add(TSStatisticType.LT_PERCENT);
		// TODO SAM 2012-07-12 Should (non)exceedance probability be included?
		return statistics;
	}

	/// <summary>
	/// Return a list of statistic choices.
	/// These strings are suitable for listing in a user interface.  The statistics are
	/// listed in ascending alphabetical order.
	/// </summary>
	public static IList<string> getStatisticChoicesAsStrings()
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
	/// Determine the statistic test type, when comparing against a test value. </summary>
	/// <param name="statisticType"> a statistic type to check. </param>
	/// <returns> the test type for the statistic or NOT_USED if the test is not used for a statistic. </returns>
	private TestType getStatisticTestType(TSStatisticType statisticType)
	{
		if ((statisticType == TSStatisticType.GE_COUNT) || (statisticType == TSStatisticType.GE_PERCENT))
		{
			return TestType.GE;
		}
		else if ((statisticType == TSStatisticType.GT_COUNT) || (statisticType == TSStatisticType.GT_PERCENT))
		{
			return TestType.GT;
		}
		else if ((statisticType == TSStatisticType.LE_COUNT) || (statisticType == TSStatisticType.LE_PERCENT))
		{
			return TestType.LE;
		}
		else if ((statisticType == TSStatisticType.LT_COUNT) || (statisticType == TSStatisticType.LT_PERCENT))
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
	/// <returns> the data units for the time series, given the statistic. </returns>
	private string getStatisticTimeSeriesDataUnits(TSStatisticType statisticType)
	{
		if (isStatisticPercent(statisticType))
		{
			return "Percent";
		}
		else if (isStatisticCount(statisticType))
		{
			return "Count";
		}
		else
		{
			return "";
		}
	}

	/// <summary>
	/// Determine the statistic time series description. </summary>
	/// <param name="statisticType"> a statistic type to check. </param>
	/// <param name="testType"> test type that is being performed. </param>
	/// <param name="testValue"> the test value to be checked. </param>
	/// <param name="statisticIsCount"> if true, then the statistic is a count </param>
	/// <param name="statisticIsPercent"> if true, the statistic is a percent </param>
	/// <returns> the description for the time series, given the statistic and test types. </returns>
	private string getStatisticTimeSeriesDescription(TSStatisticType statisticType, TestType testType, double? testValue)
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
		bool statisticIsCount = isStatisticCount(statisticType);
		bool statisticIsPercent = isStatisticPercent(statisticType);
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
		else if (statisticIsPercent)
		{
			if (statisticType == TSStatisticType.MISSING_COUNT)
			{
				desc = "Percent of missing values";
			}
			else if (statisticType == TSStatisticType.NONMISSING_COUNT)
			{
				desc = "Percent of nonmissing values";
			}
			else
			{
				desc = "Percent of values " + testString + " " + testValueString;
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
	/// Return the test values used to calculate some statistics. </summary>
	/// <returns> the test values used to calculate some statistics. </returns>
	private double [] getTestValues()
	{
		return __testValues;
	}

	/// <summary>
	/// Return the list of time series being analyzed. </summary>
	/// <returns> the list of time series being analyzed. </returns>
	public virtual IList<TS> getTimeSeriesList()
	{
		return __tslist;
	}

	/// <summary>
	/// Indicate whether the statistic is a count. </summary>
	/// <param name="statisticType"> a statistic type to check. </param>
	private bool isStatisticCount(TSStatisticType statisticType)
	{
		if ((statisticType == TSStatisticType.GE_COUNT) || (statisticType == TSStatisticType.GT_COUNT) || (statisticType == TSStatisticType.LE_COUNT) || (statisticType == TSStatisticType.LT_COUNT))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Indicate whether the statistic is a percent. </summary>
	/// <param name="statisticType"> a statistic type to check. </param>
	private bool isStatisticPercent(TSStatisticType statisticType)
	{
		if ((statisticType == TSStatisticType.GE_PERCENT) || (statisticType == TSStatisticType.GT_PERCENT) || (statisticType == TSStatisticType.LE_PERCENT) || (statisticType == TSStatisticType.LT_PERCENT))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Indicate whether a statistic is supported. </summary>
	/// <param name="statisticType"> a statistic type to check. </param>
	/// <param name="interval"> time interval base to check, or TimeInterval.UNKNOWN if interval is not to be considered. </param>
	/// <param name="timeScale"> time scale to check, or null if not considered. </param>
	public static bool isStatisticSupported(TSStatisticType statisticType)
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
	/// Indicate whether the statistic requires that a test value be supplied. </summary>
	/// <param name="statisticType"> a statistic type to check. </param>
	public static bool isTestValueNeeded(TSStatisticType statisticType)
	{
		if ((statisticType == TSStatisticType.GE_COUNT) || (statisticType == TSStatisticType.GE_PERCENT) || (statisticType == TSStatisticType.GT_COUNT) || (statisticType == TSStatisticType.GT_PERCENT) || (statisticType == TSStatisticType.LE_COUNT) || (statisticType == TSStatisticType.LE_PERCENT) || (statisticType == TSStatisticType.LT_COUNT) || (statisticType == TSStatisticType.LT_PERCENT))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Create an ensemble that contains time series statistics in each data value (e.g.,
	/// percent greater than a threshold value). </summary>
	/// <param name="createData"> if true, calculate the data value array; if false, only assign metadata </param>
	/// <returns> the statistics ensemble </returns>
	public virtual TSEnsemble newStatisticEnsemble(bool createData)
	{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		string routine = this.GetType().FullName + ".newStatisticEnsemble";
		int dl = 10;

		// Get the data needed for the analysis - originally provided in the constructor

		IList<TS> tslist = getTimeSeriesList();
		string newEnsembleID = getNewEnsembleID();
		string newEnsembleName = getNewEnsembleName();
		string newTSID = getNewTSID();
		TSStatisticType statisticType = getStatisticType();
		double[] testValues = getTestValues();
		int? allowMissingCount = getAllowMissingCount();
		int? minimumSampleSize = getMinimumSampleSize();
		DateTime analysisStart = getAnalysisStart();
		DateTime analysisEnd = getAnalysisEnd();

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Trying to create statistic ensemble for " + tslist.Count + " input time series.");
		}

		// Get valid dates to use for the output time series because the ones passed in may have been null.
		// Use the overlapping period of all the time series

		if ((analysisStart != null) && (analysisEnd != null))
		{
			// Create a local copy to protect from passed parameters
			analysisStart = new DateTime(analysisStart);
			analysisEnd = new DateTime(analysisEnd);
		}
		else
		{
			// Get the analysis start and end from the overall period
			TSLimits validDates = null;
			try
			{
				validDates = TSUtil.getPeriodFromTS(tslist, TSUtil.MAX_POR);
			}
			catch (Exception e)
			{
				throw new Exception("Error getting period from input time series (" + e + ").");
			}
			analysisStart = new DateTime(validDates.getDate1());
			analysisEnd = new DateTime(validDates.getDate2());
		}

		// Create a statistic time series for each value being processed
		// Also create an ensemble and add the individual time series to the ensemble, if requested

		IList<TS> stattsList = new List<TS>();
		for (int i = 0; i < testValues.Length; i++)
		{
			// Create the time series identifier using the newTSID, but may need to expand for the statistic
			string tsid = newTSID;
			TS statts = null;
			try
			{
				statts = TSUtil.newTimeSeries(tsid, true);
				statts.setIdentifier(tsid);
			}
			catch (Exception e)
			{
				throw new Exception("Error creating statistic time series using TSID=\"" + tsid + "\" uniquetempvar.");
			}
			statts.addToGenesis("Initialized statistic time series using TSID=\"" + statts.getIdentifierString() + "\"");
			// Set the period to that determined above
			statts.setDate1(analysisStart);
			statts.setDate1Original(analysisStart);
			statts.setDate2(analysisEnd);
			statts.setDate2Original(analysisEnd);
			if (createData)
			{
				statts.allocateDataSpace();
			}
			// Set the units and description based on the statistic
			statts.setDataUnits(getStatisticTimeSeriesDataUnits(statisticType));
			statts.setDataUnitsOriginal(statts.getDataUnits());
			statts.setDescription(getStatisticTimeSeriesDescription(statisticType, getStatisticTestType(statisticType), testValues[i]));
			stattsList.Add(statts);
		}

		// Always create an ensemble even if the ID is empty.  Calling code can ignore the ensemble and use the
		// list of time series directly if needed.

		TSEnsemble ensemble = null;
		if (string.ReferenceEquals(newEnsembleID, null))
		{
			newEnsembleID = "";
		}
		if (string.ReferenceEquals(newEnsembleName, null))
		{
			newEnsembleName = "";
		}
		ensemble = new TSEnsemble(newEnsembleID, newEnsembleName, stattsList);

		if (!createData)
		{
			return ensemble;
		}

		// Calculate the statistic time series...
		calculateStatistic(tslist, stattsList, statisticType, testValues, analysisStart, analysisEnd, allowMissingCount.Value, minimumSampleSize.Value);

		return ensemble;
	}

	/// <summary>
	/// Set the alias for new time series. </summary>
	/// <param name="alias"> the alias for new time series </param>
	private void setAlias(string alias)
	{
		__alias = alias;
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
	/// Set the minimum sample size. </summary>
	/// <param name="minimumSampleSize"> the minimum sample size. </param>
	private void setMinimumSampleSize(int minimumSampleSize)
	{
		__minimumSampleSize = minimumSampleSize;
	}

	/// <summary>
	/// Set the new ensemble identifier. </summary>
	/// <param name="newEnsembleID"> the new ensemble identifier. </param>
	private void setNewEnsembleID(string newEnsembleID)
	{
		__newEnsembleID = newEnsembleID;
	}

	/// <summary>
	/// Set the new ensemble name. </summary>
	/// <param name="newEnsembleName"> the new ensemble name. </param>
	private void setNewEnsembleName(string newEnsembleName)
	{
		__newEnsembleName = newEnsembleName;
	}

	/// <summary>
	/// Set the new time series identifier. </summary>
	/// <param name="newTSID"> the new time series identifier. </param>
	private void setNewTSID(string newTSID)
	{
		__newTSID = newTSID;
	}

	/// <summary>
	/// Set the statistic type. </summary>
	/// <param name="statisticType"> statistic type to calculate. </param>
	private void setStatisticType(TSStatisticType statisticType)
	{
		__statisticType = statisticType;
	}

	/// <summary>
	/// Set the test values used for statistics. </summary>
	/// <param name="testValue"> the test values used with statistics. </param>
	private void setTestValues(double[] testValues)
	{
		__testValues = testValues;
	}

	/// <summary>
	/// Set the time series list being analyzed. </summary>
	/// <param name="ts"> time series list being analyzed. </param>
	private void setTimeSeriesList(IList<TS> tslist)
	{
		__tslist = tslist;
	}

	}

}