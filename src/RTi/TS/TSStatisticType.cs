using System;
using System.Collections.Generic;

// TSStatisticType - this class stores time series statistic types, which are a value or values determined
// from the sample given by the time series data points.

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
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This class stores time series statistic types, which are a value or values determined
	/// from the sample given by the time series data points.  Some statistics are general in
	/// nature and could be applied outside of time series.  Some are specific to time series
	/// (e.g., day of year that some condition occurs).  This enumeration only provides a list of potential
	/// statistic types.  Computation and management of results occurs in other classes.  Code that uses
	/// these statistics types should typically decide which statistics are supported.
	/// </summary>
	public sealed class TSStatisticType
	{
		/// <summary>
		/// Count of missing and non-missing values (total count).
		/// </summary>
		public static readonly TSStatisticType COUNT = new TSStatisticType("COUNT", InnerEnum.COUNT, "Count", "Count", "Count of missing and nonmissing values in a sample.");
		/// <summary>
		/// Day of centroid (equal sum(Day*Value) on each side).
		/// </summary>
		public static readonly TSStatisticType DAY_OF_CENTROID = new TSStatisticType("DAY_OF_CENTROID", InnerEnum.DAY_OF_CENTROID, "DayOfCentroid", "DayOfCentroid", "Day in time series where sum(day*value) is the same on each side of the day.");
		/// <summary>
		/// Day of first value >= test value.
		/// </summary>
		public static readonly TSStatisticType DAY_OF_FIRST_GE = new TSStatisticType("DAY_OF_FIRST_GE", InnerEnum.DAY_OF_FIRST_GE, "DayOfFirstGE", "DayOfFirstGE", "Day in time series for first value >= a specified value.");
		/// <summary>
		/// Day of first value > test value.
		/// </summary>
		public static readonly TSStatisticType DAY_OF_FIRST_GT = new TSStatisticType("DAY_OF_FIRST_GT", InnerEnum.DAY_OF_FIRST_GT, "DayOfFirstGT", "DayOfFirstGT", "Day in time series for first value > a specified value.");
		/// <summary>
		/// Day of first value <= test value.
		/// </summary>
		public static readonly TSStatisticType DAY_OF_FIRST_LE = new TSStatisticType("DAY_OF_FIRST_LE", InnerEnum.DAY_OF_FIRST_LE, "DayOfFirstLE", "DayOfFirstLE", "Day in time series for first value <= a specified value.");
		/// <summary>
		/// Day of first value < test value.
		/// </summary>
		public static readonly TSStatisticType DAY_OF_FIRST_LT = new TSStatisticType("DAY_OF_FIRST_LT", InnerEnum.DAY_OF_FIRST_LT, "DayOfFirstLT", "DayOfFirstLT", "Day in time series for first value < a specified value.");
		/// <summary>
		/// Day of last value >= test value.
		/// </summary>
		public static readonly TSStatisticType DAY_OF_LAST_GE = new TSStatisticType("DAY_OF_LAST_GE", InnerEnum.DAY_OF_LAST_GE, "DayOfLastGE", "DayOfLastGE", "Day in time series for last value >= a specified value.");
		/// <summary>
		/// Day of last value > test value.
		/// </summary>
		public static readonly TSStatisticType DAY_OF_LAST_GT = new TSStatisticType("DAY_OF_LAST_GT", InnerEnum.DAY_OF_LAST_GT, "DayOfLastGT", "DayOfLastGT", "Day in time series for last value > a specified value.");
		/// <summary>
		/// Day of last value <= test value.
		/// </summary>
		public static readonly TSStatisticType DAY_OF_LAST_LE = new TSStatisticType("DAY_OF_LAST_LE", InnerEnum.DAY_OF_LAST_LE, "DayOfLastLE", "DayOfLastLE", "Day in time series for last value <= a specified value.");
		/// <summary>
		/// Day of last value < test value.
		/// </summary>
		public static readonly TSStatisticType DAY_OF_LAST_LT = new TSStatisticType("DAY_OF_LAST_LT", InnerEnum.DAY_OF_LAST_LT, "DayOfLastLT", "DayOfLastLT", "Day in time series for last value < a specified value.");
		/// <summary>
		/// Day of maximum value.
		/// </summary>
		public static readonly TSStatisticType DAY_OF_MAX = new TSStatisticType("DAY_OF_MAX", InnerEnum.DAY_OF_MAX, "DayOfMax", "DayOfMax", "Day in time series for the maximum value (Max).");
		/// <summary>
		/// Day of minimum value.
		/// </summary>
		public static readonly TSStatisticType DAY_OF_MIN = new TSStatisticType("DAY_OF_MIN", InnerEnum.DAY_OF_MIN, "DayOfMin", "DayOfMin", "Day in time series for the minimum value (Min).");
		/// <summary>
		/// Maximum of (mean - value) when value is below the mean.
		/// </summary>
		public static readonly TSStatisticType DEFICIT_MAX = new TSStatisticType("DEFICIT_MAX", InnerEnum.DEFICIT_MAX, "DeficitMax", "DeficitMax", "Maximum of (Mean - value) when value is below the mean.");
		/// <summary>
		/// Mean of (mean - value) when value is below the mean.
		/// </summary>
		public static readonly TSStatisticType DEFICIT_MEAN = new TSStatisticType("DEFICIT_MEAN", InnerEnum.DEFICIT_MEAN, "DeficitMean", "DeficitMean", "Mean of (Mean - value) when value is below the mean.");
		/// <summary>
		/// Minimum of (mean - value) when value is below the mean.
		/// </summary>
		public static readonly TSStatisticType DEFICIT_MIN = new TSStatisticType("DEFICIT_MIN", InnerEnum.DEFICIT_MIN, "DeficitMin", "DeficitMin", "Minimum of (Mean = value) when value is below the mean");
		/// <summary>
		/// Maximum of number of sequential intervals with values below the mean.
		/// </summary>
		public static readonly TSStatisticType DEFICIT_SEQ_LENGTH_MAX = new TSStatisticType("DEFICIT_SEQ_LENGTH_MAX", InnerEnum.DEFICIT_SEQ_LENGTH_MAX, "DeficitSeqLengthMax", "DeficitSeqLengthMax", "Maximum of number of sequential intervals with values below the mean.");
		/// <summary>
		/// Mean of number of sequential intervals with values below the mean.
		/// </summary>
		public static readonly TSStatisticType DEFICIT_SEQ_LENGTH_MEAN = new TSStatisticType("DEFICIT_SEQ_LENGTH_MEAN", InnerEnum.DEFICIT_SEQ_LENGTH_MEAN, "DeficitSeqLengthMean", "DeficitSeqLengthMean", "Mean of number of sequential intervals with values below the mean.");
		/// <summary>
		/// Mean of number of sequential intervals with values below the mean.
		/// </summary>
		public static readonly TSStatisticType DEFICIT_SEQ_LENGTH_MIN = new TSStatisticType("DEFICIT_SEQ_LENGTH_MIN", InnerEnum.DEFICIT_SEQ_LENGTH_MIN, "DeficitSeqLengthMin", "DeficitSeqLengthMin", "Mean of number of sequential intervals with values below the mean.");
		/// <summary>
		/// Maximum of (mean - value) sum when sequential values are below the mean.
		/// </summary>
		public static readonly TSStatisticType DEFICIT_SEQ_MAX = new TSStatisticType("DEFICIT_SEQ_MAX", InnerEnum.DEFICIT_SEQ_MAX, "DeficitSeqMax", "DeficitSeqMax", "Maximum of (Mean - value) sum when sequential values are below the mean.");
		/// <summary>
		/// Mean of (mean - value) sum when sequential values are above the mean.
		/// </summary>
		public static readonly TSStatisticType DEFICIT_SEQ_MEAN = new TSStatisticType("DEFICIT_SEQ_MEAN", InnerEnum.DEFICIT_SEQ_MEAN, "DeficitSeqMean", "DeficitSeqMean", "Mean of (Mean - value) sum when sequential values are above the mean.");
		/// <summary>
		/// Minimum of (mean - value) sum when sequential values are above the mean.
		/// </summary>
		public static readonly TSStatisticType DEFICIT_SEQ_MIN = new TSStatisticType("DEFICIT_SEQ_MIN", InnerEnum.DEFICIT_SEQ_MIN, "DeficitSeqMin", "DeficitSeqMin", "Minimum of (mean - value) sum when sequential values are above the mean.");
		/// <summary>
		/// Exceedance probability, may be percent or fraction.
		/// </summary>
		public static readonly TSStatisticType EP = new TSStatisticType("EP", InnerEnum.EP, "EP", "Exceedance Probability", "Exceedance probability, may be percent or fraction.");
		/// <summary>
		/// Exceedance probability, may be percent or fraction.
		/// </summary>
		public static readonly TSStatisticType EXCEEDANCE_PROBABILITY = new TSStatisticType("EXCEEDANCE_PROBABILITY", InnerEnum.EXCEEDANCE_PROBABILITY, "ExceedanceProbability", "ExceedanceProbability", "Exceedance probability, may be percent or fraction.");
		/// <summary>
		/// Value for which there is a 10% probability of exceeding the value (0 to 10% chance).
		/// </summary>
		public static readonly TSStatisticType EXCEEDANCE_PROBABILITY_10 = new TSStatisticType("EXCEEDANCE_PROBABILITY_10", InnerEnum.EXCEEDANCE_PROBABILITY_10, "ExceedanceProbability10", "ExceedanceProbability10", "Value for which there is a 10% probability of exceeding the value (0 to 10% chance).");
		/// <summary>
		/// Value for which there is a 30% probability of exceeding the value (0 to 30% chance).
		/// </summary>
		public static readonly TSStatisticType EXCEEDANCE_PROBABILITY_30 = new TSStatisticType("EXCEEDANCE_PROBABILITY_30", InnerEnum.EXCEEDANCE_PROBABILITY_30, "ExceedanceProbability30", "ExceedanceProbability30", "Value for which there is a 30% probability of exceeding the value (0 to 30% chance).");
		/// <summary>
		/// Value for which there is a 50% probability of exceeding the value (0 to 50% chance).
		/// </summary>
		public static readonly TSStatisticType EXCEEDANCE_PROBABILITY_50 = new TSStatisticType("EXCEEDANCE_PROBABILITY_50", InnerEnum.EXCEEDANCE_PROBABILITY_50, "ExceedanceProbability50", "ExceedanceProbability50", "Value for which there is a 50% probability of exceeding the value (0 to 50% chance).");
		/// <summary>
		/// Value for which there is a 70% probability of exceeding the value (0 to 70% chance).
		/// </summary>
		public static readonly TSStatisticType EXCEEDANCE_PROBABILITY_70 = new TSStatisticType("EXCEEDANCE_PROBABILITY_70", InnerEnum.EXCEEDANCE_PROBABILITY_70, "ExceedanceProbability70", "ExceedanceProbability70", "Value for which there is a 70% probability of exceeding the value (0 to 70% chance).");
		/// <summary>
		/// Value for which there is a 90% probability of exceeding the value (0 to 90% chance).
		/// </summary>
		public static readonly TSStatisticType EXCEEDANCE_PROBABILITY_90 = new TSStatisticType("EXCEEDANCE_PROBABILITY_90", InnerEnum.EXCEEDANCE_PROBABILITY_90, "ExceedanceProbability90", "ExceedanceProbability90", "Value for which there is a 90% probability of exceeding the value (0 to 90% chance).");
		/// <summary>
		/// Count of values >= test value.
		/// </summary>
		public static readonly TSStatisticType GE_COUNT = new TSStatisticType("GE_COUNT", InnerEnum.GE_COUNT, "GECount", "GECount", "Count of values >= test value.");
		/// <summary>
		/// Percent of values >= test value.
		/// </summary>
		public static readonly TSStatisticType GE_PERCENT = new TSStatisticType("GE_PERCENT", InnerEnum.GE_PERCENT, "GEPercent", "GEPercent", "Percent of values >= test value.");
		/// <summary>
		/// Count of values > test value.
		/// </summary>
		public static readonly TSStatisticType GT_COUNT = new TSStatisticType("GT_COUNT", InnerEnum.GT_COUNT, "GTCount", "GTCount", "Count of values > test value.");
		/// <summary>
		/// Percent of values > test value.
		/// </summary>
		public static readonly TSStatisticType GT_PERCENT = new TSStatisticType("GT_PERCENT", InnerEnum.GT_PERCENT, "GTPercent", "GTPercent", "Percent of values > test value.");
		/// <summary>
		/// Geometric mean of sample values.
		/// </summary>
		public static readonly TSStatisticType GEOMETRIC_MEAN = new TSStatisticType("GEOMETRIC_MEAN", InnerEnum.GEOMETRIC_MEAN, "GeometricMean", "Geometric Mean", "Geometric mean of sample values.");
		/// <summary>
		/// Auto-correlation with previous interval's value.
		/// </summary>
		public static readonly TSStatisticType LAG1_AUTO_CORRELATION = new TSStatisticType("LAG1_AUTO_CORRELATION", InnerEnum.LAG1_AUTO_CORRELATION, "Lag-1AutoCorrelation", "Lag-1AutoCorrelation", "Auto-correlation with previous interval's value.");
		/// <summary>
		/// Last value in the sample (typically the last value in an analysis period).
		/// </summary>
		public static readonly TSStatisticType LAST = new TSStatisticType("LAST", InnerEnum.LAST, "Last", "Last", "Last value in the sample (typically the last value in an analysis period).");
		/// <summary>
		/// Last non-missing value in the sample.
		/// </summary>
		public static readonly TSStatisticType LAST_NONMISSING = new TSStatisticType("LAST_NONMISSING", InnerEnum.LAST_NONMISSING, "LastNonmissing", "LastNonmissing", "Last non-missing value in the sample.");
		/// <summary>
		/// Count of values <= test value.
		/// </summary>
		public static readonly TSStatisticType LE_COUNT = new TSStatisticType("LE_COUNT", InnerEnum.LE_COUNT, "LECount", "LECount", "Count of values <= test value.");
		/// <summary>
		/// Percent of values <= test value.
		/// </summary>
		public static readonly TSStatisticType LE_PERCENT = new TSStatisticType("LE_PERCENT", InnerEnum.LE_PERCENT, "LEPercent", "LEPercent", "Percent of values <= test value.");
		/// <summary>
		/// Count of values < test value.
		/// </summary>
		public static readonly TSStatisticType LT_COUNT = new TSStatisticType("LT_COUNT", InnerEnum.LT_COUNT, "LTCount", "LTCount", "Count of values < test value.");
		/// <summary>
		/// Percent of values < test value.
		/// </summary>
		public static readonly TSStatisticType LT_PERCENT = new TSStatisticType("LT_PERCENT", InnerEnum.LT_PERCENT, "LTPercent", "LTPercent", "Percent of values < test value.");
		/// <summary>
		/// Maximum value in the sample.
		/// </summary>
		public static readonly TSStatisticType MAX = new TSStatisticType("MAX", InnerEnum.MAX, "Max", "Maximum", "Maximum value in the sample.");
		/// <summary>
		/// Median value in the sample.
		/// </summary>
		public static readonly TSStatisticType MEDIAN = new TSStatisticType("MEDIAN", InnerEnum.MEDIAN, "Median", "Median", "Median value in the sample.");
		/// <summary>
		/// Mean value in the sample.
		/// </summary>
		public static readonly TSStatisticType MEAN = new TSStatisticType("MEAN", InnerEnum.MEAN, "Mean", "Mean", "Mean value in the sample.");
		/// <summary>
		/// Minimum value in the sample.
		/// </summary>
		public static readonly TSStatisticType MIN = new TSStatisticType("MIN", InnerEnum.MIN, "Min", "Min", "Minimum value in the sample.");
		/// <summary>
		/// Count of missing values in the sample.
		/// </summary>
		public static readonly TSStatisticType MISSING_COUNT = new TSStatisticType("MISSING_COUNT", InnerEnum.MISSING_COUNT, "MissingCount", "MissingCount", "Count of missing values in the sample.");
		/// <summary>
		/// Percent of missing values in the sample.
		/// </summary>
		public static readonly TSStatisticType MISSING_PERCENT = new TSStatisticType("MISSING_PERCENT", InnerEnum.MISSING_PERCENT, "MissingPercent", "MissingPercent", "Percent of missing values in the sample.");
		/// <summary>
		/// Maximum sequence length of missing values.
		/// </summary>
		public static readonly TSStatisticType MISSING_SEQ_LENGTH_MAX = new TSStatisticType("MISSING_SEQ_LENGTH_MAX", InnerEnum.MISSING_SEQ_LENGTH_MAX, "MissingSeqLengthMax", "MissingSeqLengthMax", "Maximum sequence length of missing values.");
		/// <summary>
		/// Month of centroid (equal sum(Month x Value) on each side).
		/// </summary>
		public static readonly TSStatisticType MONTH_OF_CENTROID = new TSStatisticType("MONTH_OF_CENTROID", InnerEnum.MONTH_OF_CENTROID, "MonthOfCentroid", "MonthOfCentroid", "Month in time series where sum(month*value) is the same on each side of the month.");
		/// <summary>
		/// Month of first value >= test value.
		/// </summary>
		public static readonly TSStatisticType MONTH_OF_FIRST_GE = new TSStatisticType("MONTH_OF_FIRST_GE", InnerEnum.MONTH_OF_FIRST_GE, "MonthOfFirstGE", "MonthOfFirstGE", "Month in time series for first value >= a specified value.");
		/// <summary>
		/// Day of first value > test value.
		/// </summary>
		public static readonly TSStatisticType MONTH_OF_FIRST_GT = new TSStatisticType("MONTH_OF_FIRST_GT", InnerEnum.MONTH_OF_FIRST_GT, "MonthOfFirstGT", "MonthOfFirstGT", "Month in time series for first value > a specified value.");
		/// <summary>
		/// Month of first value <= test value.
		/// </summary>
		public static readonly TSStatisticType MONTH_OF_FIRST_LE = new TSStatisticType("MONTH_OF_FIRST_LE", InnerEnum.MONTH_OF_FIRST_LE, "MonthOfFirstLE", "MonthOfFirstLE", "Month in time series for first value <= a specified value.");
		/// <summary>
		/// Month of first value < test value.
		/// </summary>
		public static readonly TSStatisticType MONTH_OF_FIRST_LT = new TSStatisticType("MONTH_OF_FIRST_LT", InnerEnum.MONTH_OF_FIRST_LT, "MonthOfFirstLT", "MonthOfFirstLT", "Month in time series for first value < a specified value.");
		/// <summary>
		/// Month of last value >= test value.
		/// </summary>
		public static readonly TSStatisticType MONTH_OF_LAST_GE = new TSStatisticType("MONTH_OF_LAST_GE", InnerEnum.MONTH_OF_LAST_GE, "MonthOfLastGE", "MonthOfLastGE", "Month in time series for last value >= a specified value.");
		/// <summary>
		/// Month of last value > test value.
		/// </summary>
		public static readonly TSStatisticType MONTH_OF_LAST_GT = new TSStatisticType("MONTH_OF_LAST_GT", InnerEnum.MONTH_OF_LAST_GT, "MonthOfLastGT", "MonthOfLastGT", "Month in time series for last value > a specified value.");
		/// <summary>
		/// Month of last value <= test value.
		/// </summary>
		public static readonly TSStatisticType MONTH_OF_LAST_LE = new TSStatisticType("MONTH_OF_LAST_LE", InnerEnum.MONTH_OF_LAST_LE, "MonthOfLastLE", "MonthOfLastLE", "Month in time series for last value <= a specified value.");
		/// <summary>
		/// Month of last value < test value.
		/// </summary>
		public static readonly TSStatisticType MONTH_OF_LAST_LT = new TSStatisticType("MONTH_OF_LAST_LT", InnerEnum.MONTH_OF_LAST_LT, "MonthOfLastLT", "MonthOfLastLT", "Month in time series for last value < a specified value.");
		/// <summary>
		/// Month of maximum value.
		/// </summary>
		public static readonly TSStatisticType MONTH_OF_MAX = new TSStatisticType("MONTH_OF_MAX", InnerEnum.MONTH_OF_MAX, "MonthOfMax", "MonthOfMax", "Month of maximum value.");
		/// <summary>
		/// Month of minimum value.
		/// </summary>
		public static readonly TSStatisticType MONTH_OF_MIN = new TSStatisticType("MONTH_OF_MIN", InnerEnum.MONTH_OF_MIN, "MonthOfMin", "MonthOfMin", "Month of minimum value.");
		/// <summary>
		/// Nonexceedance probability, can be percent or fraction.
		/// </summary>
		public static readonly TSStatisticType NEP = new TSStatisticType("NEP", InnerEnum.NEP, "NEP", "Nonexceedance Probability", "Nonexceedance probability, can be percent or fraction."); // Make the same as NONEXCEEDANCE_PROBABILITY
		/// <summary>
		/// Nonexceedance probability, percent.
		/// </summary>
		public static readonly TSStatisticType NONEXCEEDANCE_PROBABILITY = new TSStatisticType("NONEXCEEDANCE_PROBABILITY", InnerEnum.NONEXCEEDANCE_PROBABILITY, "NonexceedanceProbability", "Nonexceedance Probability", "Nonexceedance probability, can be percent or fraction.");
		/// <summary>
		/// Value for which there is a 10% probability of not exceeding the value (0 to 10% chance).
		/// </summary>
		public static readonly TSStatisticType NONEXCEEDANCE_PROBABILITY_10 = new TSStatisticType("NONEXCEEDANCE_PROBABILITY_10", InnerEnum.NONEXCEEDANCE_PROBABILITY_10, "NonexceedanceProbability10", "NonexceedanceProbability10", "Value for which there is a 10% probability of not exceeding the value (0 to 10% chance).");
		/// <summary>
		/// Value for which there is a 30% probability of not exceeding the value (0 to 30% chance).
		/// </summary>
		public static readonly TSStatisticType NONEXCEEDANCE_PROBABILITY_30 = new TSStatisticType("NONEXCEEDANCE_PROBABILITY_30", InnerEnum.NONEXCEEDANCE_PROBABILITY_30, "NonexceedanceProbability30", "NonexceedanceProbability30", "Value for which there is a 30% probability of not exceeding the value (0 to 30% chance).");
		/// <summary>
		/// Value for which there is a 50% probability of not exceeding the value (0 to 50% chance).
		/// </summary>
		public static readonly TSStatisticType NONEXCEEDANCE_PROBABILITY_50 = new TSStatisticType("NONEXCEEDANCE_PROBABILITY_50", InnerEnum.NONEXCEEDANCE_PROBABILITY_50, "NonexceedanceProbability50", "NonexceedanceProbability50", "Value for which there is a 50% probability of not exceeding the value (0 to 50% chance).");
		/// <summary>
		/// Value for which there is a 70% probability of not exceeding the value (0 to 70% chance).
		/// </summary>
		public static readonly TSStatisticType NONEXCEEDANCE_PROBABILITY_70 = new TSStatisticType("NONEXCEEDANCE_PROBABILITY_70", InnerEnum.NONEXCEEDANCE_PROBABILITY_70, "NonexceedanceProbability70", "NonexceedanceProbability70", "Value for which there is a 70% probability of not exceeding the value (0 to 70% chance).");
		/// <summary>
		/// Value for which there is a 90% probability of not exceeding the value (0 to 90% chance).
		/// </summary>
		public static readonly TSStatisticType NONEXCEEDANCE_PROBABILITY_90 = new TSStatisticType("NONEXCEEDANCE_PROBABILITY_90", InnerEnum.NONEXCEEDANCE_PROBABILITY_90, "NonexceedanceProbability90", "NonexceedanceProbability90", "Value for which there is a 90% probability of not exceeding the value (0 to 90% chance).");
		/// <summary>
		/// Count of non-missing values in the sample.
		/// </summary>
		public static readonly TSStatisticType NONMISSING_COUNT = new TSStatisticType("NONMISSING_COUNT", InnerEnum.NONMISSING_COUNT, "NonmissingCount", "NonmissingCount", "Count of non-missing values in the sample.");
		/// <summary>
		/// Percent of non-missing values.
		/// </summary>
		public static readonly TSStatisticType NONMISSING_PERCENT = new TSStatisticType("NONMISSING_PERCENT", InnerEnum.NONMISSING_PERCENT, "NonmissingPercent", "NonmissingPercent", "Percent of non-missing values.");
		/// <summary>
		/// For daily data: 10-year recurrence interval for lowest 7-day average flow (each year) is 7q10
		/// </summary>
		public static readonly TSStatisticType NQYY = new TSStatisticType("NQYY", InnerEnum.NQYY, "NqYY", "NqYY", "For example: 10-year recurrence interval for lowest 7-day average flow (each year) is 7q10");
		/// <summary>
		/// Percent of max for a value.
		/// </summary>
		public static readonly TSStatisticType PERCENT_OF_MAX = new TSStatisticType("PERCENT_OF_MAX", InnerEnum.PERCENT_OF_MAX, "PercentOfMax", "PercentOfMax", "Percent of Max for a value.");
		/// <summary>
		/// Percent of min.
		/// </summary>
		public static readonly TSStatisticType PERCENT_OF_MIN = new TSStatisticType("PERCENT_OF_MIN", InnerEnum.PERCENT_OF_MIN, "PercentOfMin", "PercentOfMin", "Percent of Min for a value.");
		/// <summary>
		/// Percent of mean.
		/// </summary>
		public static readonly TSStatisticType PERCENT_OF_MEAN = new TSStatisticType("PERCENT_OF_MEAN", InnerEnum.PERCENT_OF_MEAN, "PercentOfMean", "PercentOfMean", "Percent of Mean for a value.");
		/// <summary>
		/// Percent of median.
		/// </summary>
		public static readonly TSStatisticType PERCENT_OF_MEDIAN = new TSStatisticType("PERCENT_OF_MEDIAN", InnerEnum.PERCENT_OF_MEDIAN, "PercentOfMedian", "PercentOfMedian", "Percent of Median for a value.");
		/// <summary>
		/// Plotting position (depends on distribution and sort order imposed by other code).
		/// TODO SAM 2016-06-17 need to confirm whether this should be used or the other variations.
		/// </summary>
		public static readonly TSStatisticType PLOTTING_POSITION = new TSStatisticType("PLOTTING_POSITION", InnerEnum.PLOTTING_POSITION, "PlottingPosition", "PlottingPosition", "Plotting position for a distribution, values sorted low to high.");
		/// <summary>
		/// Plotting position based on ascending sort.
		/// TODO SAM 2016-06-17 evaluate phasing out.
		/// </summary>
		public static readonly TSStatisticType PLOTTING_POSITION_ASCENDING = new TSStatisticType("PLOTTING_POSITION_ASCENDING", InnerEnum.PLOTTING_POSITION_ASCENDING, "PlotPosAscending", "PlotPosAscending", "Plotting position based on ascending sort.");
		/// <summary>
		/// Plotting position based on descending sort.
		/// TODO SAM 2016-06-17 evaluate phasing out.
		/// </summary>
		public static readonly TSStatisticType PLOTTING_POSITION_DESCENDING = new TSStatisticType("PLOTTING_POSITION_DESCENDING", InnerEnum.PLOTTING_POSITION_DESCENDING, "PlotPosDescending", "PlotPosDescending", "Plotting position based on descending sort.");
		/// <summary>
		/// Rank (depends on sort order imposed by other code). </summary>
		/// @deprecated confusing because sort order is not tied to statistic.  Use RANK_ASCENDING or RANK_DESCENDING. 
		public static readonly TSStatisticType RANK = new TSStatisticType("RANK", InnerEnum.RANK, "Rank", "Rank", "Rank (depends on sort order imposed by other code).");
		/// <summary>
		/// Rank based on minimum to maximum sort.
		/// </summary>
		public static readonly TSStatisticType RANK_ASCENDING = new TSStatisticType("RANK_ASCENDING", InnerEnum.RANK_ASCENDING, "RankAscending", "RankAscending", "Rank based on minimum to maximum sort.");
		/// <summary>
		/// Rank based on maximum to minimum sort.
		/// </summary>
		public static readonly TSStatisticType RANK_DESCENDING = new TSStatisticType("RANK_DESCENDING", InnerEnum.RANK_DESCENDING, "RankDescending", "RankDescending", "Rank based on maximum to minimum sort.");
		/// <summary>
		/// Coefficient of skew.
		/// </summary>
		public static readonly TSStatisticType SKEW = new TSStatisticType("SKEW", InnerEnum.SKEW, "Skew", "Skew", "Coefficient of skew.");
		/// <summary>
		/// Standard deviation.
		/// </summary>
		public static readonly TSStatisticType STD_DEV = new TSStatisticType("STD_DEV", InnerEnum.STD_DEV, "StdDev", "StdDev", "Standard deviation.");
		/// <summary>
		/// Maximum of of (value - mean), if a surplus.
		/// </summary>
		public static readonly TSStatisticType SURPLUS_MAX = new TSStatisticType("SURPLUS_MAX", InnerEnum.SURPLUS_MAX, "SurplusMax", "SurplusMax", "Maximum of of (value - Mean), if a surplus.");
		/// <summary>
		/// Mean of (value - mean), if a surplus.
		/// </summary>
		public static readonly TSStatisticType SURPLUS_MEAN = new TSStatisticType("SURPLUS_MEAN", InnerEnum.SURPLUS_MEAN, "SurplusMean", "SurplusMean", "Mean of (value - Mean), if a surplus.");
		/// <summary>
		/// Minimum of (value - mean), if a surplus.
		/// </summary>
		public static readonly TSStatisticType SURPLUS_MIN = new TSStatisticType("SURPLUS_MIN", InnerEnum.SURPLUS_MIN, "SurplusMin", "SurplusMin", "Minimum of (value - Mean), if a surplus.");
		/// <summary>
		/// Maximum of number of sequential intervals with values above the mean.
		/// </summary>
		public static readonly TSStatisticType SURPLUS_SEQ_LENGTH_MAX = new TSStatisticType("SURPLUS_SEQ_LENGTH_MAX", InnerEnum.SURPLUS_SEQ_LENGTH_MAX, "SurplusSeqLengthMax", "SurplusSeqLengthMax", "Maximum of number of sequential intervals with values above the mean.");
		/// <summary>
		/// Mean of number of sequential intervals with values above the mean.
		/// </summary>
		public static readonly TSStatisticType SURPLUS_SEQ_LENGTH_MEAN = new TSStatisticType("SURPLUS_SEQ_LENGTH_MEAN", InnerEnum.SURPLUS_SEQ_LENGTH_MEAN, "SurplusSeqLengthMean", "SurplusSeqLengthMean", "Mean of number of sequential intervals with values above the mean.");
		/// <summary>
		/// Mean of number of sequential intervals with values above the mean.
		/// </summary>
		public static readonly TSStatisticType SURPLUS_SEQ_LENGTH_MIN = new TSStatisticType("SURPLUS_SEQ_LENGTH_MIN", InnerEnum.SURPLUS_SEQ_LENGTH_MIN, "SurplusSeqLengthMin", "SurplusSeqLengthMin", "Mean of number of sequential intervals with values above the mean.");
		/// <summary>
		/// Maximum of (value - mean) sum when sequential values are above the mean.
		/// </summary>
		public static readonly TSStatisticType SURPLUS_SEQ_MAX = new TSStatisticType("SURPLUS_SEQ_MAX", InnerEnum.SURPLUS_SEQ_MAX, "SurplusSeqMax", "SurplusSeqMax", "Maximum of (value - Mean) sum when sequential values are above the mean.");
		/// <summary>
		/// Mean of (value - mean) sum when sequential values are above the mean.
		/// </summary>
		public static readonly TSStatisticType SURPLUS_SEQ_MEAN = new TSStatisticType("SURPLUS_SEQ_MEAN", InnerEnum.SURPLUS_SEQ_MEAN, "SurplusSeqMean", "SurplusSeqMean", "Mean of (value - Mean) sum when sequential values are above the mean.");
		/// <summary>
		/// Minimum of (value - mean) sum when sequential values are above the mean.
		/// </summary>
		public static readonly TSStatisticType SURPLUS_SEQ_MIN = new TSStatisticType("SURPLUS_SEQ_MIN", InnerEnum.SURPLUS_SEQ_MIN, "SurplusSeqMin", "SurplusSeqMin", "Minimum of (value - mean) sum when sequential values are above the mean.");
		/// <summary>
		/// Total of values in the sample.
		/// </summary>
		public static readonly TSStatisticType TOTAL = new TSStatisticType("TOTAL", InnerEnum.TOTAL, "Total", "Total", "Total of values in the sample.");
		/// <summary>
		/// Trend represented as ordinary least squares line of best fit.  Output statistics are
		/// TrendOLS_Intercept, TrendOLS_Slope, and TrendOLS_R2.  If analyzing time series values over time,
		/// the X-coordinate (time) generally is computed as year, absolute month, or absolute day.
		/// </summary>
		public static readonly TSStatisticType TREND_OLS = new TSStatisticType("TREND_OLS", InnerEnum.TREND_OLS, "TrendOLS", "TrendOLS", "Trend represented as ordinary least squares line of best fit.");
		/// <summary>
		/// Sample variance.
		/// </summary>
		public static readonly TSStatisticType VARIANCE = new TSStatisticType("VARIANCE", InnerEnum.VARIANCE, "Variance", "Variance", "Sample variance.");

		private static readonly IList<TSStatisticType> valueList = new List<TSStatisticType>();

		static TSStatisticType()
		{
			valueList.Add(COUNT);
			valueList.Add(DAY_OF_CENTROID);
			valueList.Add(DAY_OF_FIRST_GE);
			valueList.Add(DAY_OF_FIRST_GT);
			valueList.Add(DAY_OF_FIRST_LE);
			valueList.Add(DAY_OF_FIRST_LT);
			valueList.Add(DAY_OF_LAST_GE);
			valueList.Add(DAY_OF_LAST_GT);
			valueList.Add(DAY_OF_LAST_LE);
			valueList.Add(DAY_OF_LAST_LT);
			valueList.Add(DAY_OF_MAX);
			valueList.Add(DAY_OF_MIN);
			valueList.Add(DEFICIT_MAX);
			valueList.Add(DEFICIT_MEAN);
			valueList.Add(DEFICIT_MIN);
			valueList.Add(DEFICIT_SEQ_LENGTH_MAX);
			valueList.Add(DEFICIT_SEQ_LENGTH_MEAN);
			valueList.Add(DEFICIT_SEQ_LENGTH_MIN);
			valueList.Add(DEFICIT_SEQ_MAX);
			valueList.Add(DEFICIT_SEQ_MEAN);
			valueList.Add(DEFICIT_SEQ_MIN);
			valueList.Add(EP);
			valueList.Add(EXCEEDANCE_PROBABILITY);
			valueList.Add(EXCEEDANCE_PROBABILITY_10);
			valueList.Add(EXCEEDANCE_PROBABILITY_30);
			valueList.Add(EXCEEDANCE_PROBABILITY_50);
			valueList.Add(EXCEEDANCE_PROBABILITY_70);
			valueList.Add(EXCEEDANCE_PROBABILITY_90);
			valueList.Add(GE_COUNT);
			valueList.Add(GE_PERCENT);
			valueList.Add(GT_COUNT);
			valueList.Add(GT_PERCENT);
			valueList.Add(GEOMETRIC_MEAN);
			valueList.Add(LAG1_AUTO_CORRELATION);
			valueList.Add(LAST);
			valueList.Add(LAST_NONMISSING);
			valueList.Add(LE_COUNT);
			valueList.Add(LE_PERCENT);
			valueList.Add(LT_COUNT);
			valueList.Add(LT_PERCENT);
			valueList.Add(MAX);
			valueList.Add(MEDIAN);
			valueList.Add(MEAN);
			valueList.Add(MIN);
			valueList.Add(MISSING_COUNT);
			valueList.Add(MISSING_PERCENT);
			valueList.Add(MISSING_SEQ_LENGTH_MAX);
			valueList.Add(MONTH_OF_CENTROID);
			valueList.Add(MONTH_OF_FIRST_GE);
			valueList.Add(MONTH_OF_FIRST_GT);
			valueList.Add(MONTH_OF_FIRST_LE);
			valueList.Add(MONTH_OF_FIRST_LT);
			valueList.Add(MONTH_OF_LAST_GE);
			valueList.Add(MONTH_OF_LAST_GT);
			valueList.Add(MONTH_OF_LAST_LE);
			valueList.Add(MONTH_OF_LAST_LT);
			valueList.Add(MONTH_OF_MAX);
			valueList.Add(MONTH_OF_MIN);
			valueList.Add(NEP);
			valueList.Add(NONEXCEEDANCE_PROBABILITY);
			valueList.Add(NONEXCEEDANCE_PROBABILITY_10);
			valueList.Add(NONEXCEEDANCE_PROBABILITY_30);
			valueList.Add(NONEXCEEDANCE_PROBABILITY_50);
			valueList.Add(NONEXCEEDANCE_PROBABILITY_70);
			valueList.Add(NONEXCEEDANCE_PROBABILITY_90);
			valueList.Add(NONMISSING_COUNT);
			valueList.Add(NONMISSING_PERCENT);
			valueList.Add(NQYY);
			valueList.Add(PERCENT_OF_MAX);
			valueList.Add(PERCENT_OF_MIN);
			valueList.Add(PERCENT_OF_MEAN);
			valueList.Add(PERCENT_OF_MEDIAN);
			valueList.Add(PLOTTING_POSITION);
			valueList.Add(PLOTTING_POSITION_ASCENDING);
			valueList.Add(PLOTTING_POSITION_DESCENDING);
			valueList.Add(RANK);
			valueList.Add(RANK_ASCENDING);
			valueList.Add(RANK_DESCENDING);
			valueList.Add(SKEW);
			valueList.Add(STD_DEV);
			valueList.Add(SURPLUS_MAX);
			valueList.Add(SURPLUS_MEAN);
			valueList.Add(SURPLUS_MIN);
			valueList.Add(SURPLUS_SEQ_LENGTH_MAX);
			valueList.Add(SURPLUS_SEQ_LENGTH_MEAN);
			valueList.Add(SURPLUS_SEQ_LENGTH_MIN);
			valueList.Add(SURPLUS_SEQ_MAX);
			valueList.Add(SURPLUS_SEQ_MEAN);
			valueList.Add(SURPLUS_SEQ_MIN);
			valueList.Add(TOTAL);
			valueList.Add(TREND_OLS);
			valueList.Add(VARIANCE);
		}

		public enum InnerEnum
		{
			COUNT,
			DAY_OF_CENTROID,
			DAY_OF_FIRST_GE,
			DAY_OF_FIRST_GT,
			DAY_OF_FIRST_LE,
			DAY_OF_FIRST_LT,
			DAY_OF_LAST_GE,
			DAY_OF_LAST_GT,
			DAY_OF_LAST_LE,
			DAY_OF_LAST_LT,
			DAY_OF_MAX,
			DAY_OF_MIN,
			DEFICIT_MAX,
			DEFICIT_MEAN,
			DEFICIT_MIN,
			DEFICIT_SEQ_LENGTH_MAX,
			DEFICIT_SEQ_LENGTH_MEAN,
			DEFICIT_SEQ_LENGTH_MIN,
			DEFICIT_SEQ_MAX,
			DEFICIT_SEQ_MEAN,
			DEFICIT_SEQ_MIN,
			EP,
			EXCEEDANCE_PROBABILITY,
			EXCEEDANCE_PROBABILITY_10,
			EXCEEDANCE_PROBABILITY_30,
			EXCEEDANCE_PROBABILITY_50,
			EXCEEDANCE_PROBABILITY_70,
			EXCEEDANCE_PROBABILITY_90,
			GE_COUNT,
			GE_PERCENT,
			GT_COUNT,
			GT_PERCENT,
			GEOMETRIC_MEAN,
			LAG1_AUTO_CORRELATION,
			LAST,
			LAST_NONMISSING,
			LE_COUNT,
			LE_PERCENT,
			LT_COUNT,
			LT_PERCENT,
			MAX,
			MEDIAN,
			MEAN,
			MIN,
			MISSING_COUNT,
			MISSING_PERCENT,
			MISSING_SEQ_LENGTH_MAX,
			MONTH_OF_CENTROID,
			MONTH_OF_FIRST_GE,
			MONTH_OF_FIRST_GT,
			MONTH_OF_FIRST_LE,
			MONTH_OF_FIRST_LT,
			MONTH_OF_LAST_GE,
			MONTH_OF_LAST_GT,
			MONTH_OF_LAST_LE,
			MONTH_OF_LAST_LT,
			MONTH_OF_MAX,
			MONTH_OF_MIN,
			NEP,
			NONEXCEEDANCE_PROBABILITY,
			NONEXCEEDANCE_PROBABILITY_10,
			NONEXCEEDANCE_PROBABILITY_30,
			NONEXCEEDANCE_PROBABILITY_50,
			NONEXCEEDANCE_PROBABILITY_70,
			NONEXCEEDANCE_PROBABILITY_90,
			NONMISSING_COUNT,
			NONMISSING_PERCENT,
			NQYY,
			PERCENT_OF_MAX,
			PERCENT_OF_MIN,
			PERCENT_OF_MEAN,
			PERCENT_OF_MEDIAN,
			PLOTTING_POSITION,
			PLOTTING_POSITION_ASCENDING,
			PLOTTING_POSITION_DESCENDING,
			RANK,
			RANK_ASCENDING,
			RANK_DESCENDING,
			SKEW,
			STD_DEV,
			SURPLUS_MAX,
			SURPLUS_MEAN,
			SURPLUS_MIN,
			SURPLUS_SEQ_LENGTH_MAX,
			SURPLUS_SEQ_LENGTH_MEAN,
			SURPLUS_SEQ_LENGTH_MIN,
			SURPLUS_SEQ_MAX,
			SURPLUS_SEQ_MEAN,
			SURPLUS_SEQ_MIN,
			TOTAL,
			TREND_OLS,
			VARIANCE
		}

		public readonly InnerEnum innerEnumValue;
		private readonly string nameValue;
		private readonly int ordinalValue;
		private static int nextOrdinal = 0;

		/// <summary>
		/// The name that should be displayed when the best fit type is used in UIs and reports,
		/// typically an abbreviation or short name without spaces.
		/// </summary>
		private readonly string displayName;

		/// <summary>
		/// A longer name than the display name, for example expanded acronym.
		/// </summary>
		private readonly string longDisplayName;

		/// <summary>
		/// Definition of the statistic, for use in tool tips, etc.
		/// </summary>
		private readonly string definition;

		/// <summary>
		/// Construct a time series statistic enumeration value. </summary>
		/// <param name="displayName"> name that should be displayed in choices, etc., typically a terse but understandable abbreviation,
		/// guaranteed to be unique. </param>
		/// <param name="longDisplayName"> long name that can be used for displays, for example expanded abbreviation. </param>
		/// <param name="definition"> the definition of the statistic, for example for use in help tooltips. </param>
		private TSStatisticType(string name, InnerEnum innerEnum, string displayName, string longDisplayName, string definition)
		{
			this.displayName = displayName;
			this.longDisplayName = longDisplayName;
			this.definition = definition;

			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

	// TODO SAM 2005-09-30
	// Need to add:
	//
	// Sum/Total
	// Mean
	// Median
	// StdDev
	// Var
	// Delta (Prev/Next?)
	// DataCoveragePercent
	// MissingPercent
	// Percentile01 (.01), etc.
	//
	// Need to store in time series history as analyzing?

	// TODO SAM 2005-09-12
	// Need to figure out how to handle irregular data.

		/// <summary>
		/// Return the definition.
		/// </summary>
		public string getDefinition()
		{
			return this.definition;
		}

		/// <summary>
		/// Return the statistic long name.
		/// </summary>
		public string getLongName()
		{
			return this.longDisplayName;
		}

	// TODO SAM 2009-07-27 evaluate using enumeration, etc. to have properties for statistic or add this method
	// to specific calculation classes.
	/// <summary>
	/// Return the statistic data type as double, integer, etc., to facilitate handling by other code. </summary>
	/// <param name="statistic"> name of statistic </param>
	/// <returns> the statistic data type. </returns>
	public static Type getStatisticDataType(string statistic)
	{
		if ((StringUtil.indexOfIgnoreCase(statistic, "Count", 0) >= 0) || StringUtil.startsWithIgnoreCase(statistic,"Day"))
		{
			return typeof(Integer);
		}
		else
		{
			return typeof(Double);
		}
	}

	/// <summary>
	/// Return the display name for the statistic.  This is usually the same as the
	/// value but using appropriate mixed case. </summary>
	/// <returns> the display name. </returns>
	public override string ToString()
	{
		return displayName;
	}

	/// <summary>
	/// Return the enumeration value given a string name (case-independent). </summary>
	/// <returns> the enumeration value given a string name (case-independent), or null if not matched. </returns>
	public static TSStatisticType valueOfIgnoreCase(string name)
	{
		if (string.ReferenceEquals(name, null))
		{
			return null;
		}
		TSStatisticType[] values = values();
		// Legacy conversions
		if (name.Equals("COUNT_GE", StringComparison.OrdinalIgnoreCase))
		{
			return GE_COUNT;
		}
		else if (name.Equals("COUNT_GT", StringComparison.OrdinalIgnoreCase))
		{
			return GT_COUNT;
		}
		else if (name.Equals("COUNT_LE", StringComparison.OrdinalIgnoreCase))
		{
			return LE_COUNT;
		}
		else if (name.Equals("COUNT_LT", StringComparison.OrdinalIgnoreCase))
		{
			return LT_COUNT;
		}
		// Currently supported values
		foreach (TSStatisticType t in values)
		{
			if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				return t;
			}
		}
		return null;
	}


		public static IList<TSStatisticType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static TSStatisticType valueOf(string name)
		{
			foreach (TSStatisticType enumInstance in TSStatisticType.valueList)
			{
				if (enumInstance.nameValue == name)
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException(name);
		}
	}

}