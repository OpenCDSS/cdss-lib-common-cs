// NqYYFrequencyAnalysis - perform a low flow frequency analysis consistent with NqYY, conventions (e.g., 7q10).

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

	using LogPearsonType3Distribution = RTi.Util.Math.LogPearsonType3Distribution;
	using Message = RTi.Util.Message.Message;
	using DateTime = RTi.Util.Time.DateTime;
	using TimeUtil = RTi.Util.Time.TimeUtil;

	/// <summary>
	/// Perform a low flow frequency analysis consistent with NqYY, conventions (e.g., 7q10).
	/// In this case the analysis is applied to daily streamflow as follows (is not valid for other
	/// than daily data):
	/// <ol>
	/// <li>Determine the 7-day average within a year.  For each day in a year, use 3 days prior to the
	/// day and 3 days after the day to compute the average.  For end-points, use 3 days from an adjoining
	/// year if necessary (4 days in the current year).  Select the lowest 7-day average for each year.</li>
	/// <li>Rank the values for each year from low to high.</li>
	/// <li>Use a log-Pearson Type III probability distribution (by default) to determine the 10-year recurrence
	/// interval (nonexceedance probability of 10 percent).
	/// </ol>
	/// </summary>
	public class NqYYFrequencyAnalysis
	{

	/// <summary>
	/// Analysis result (the value of NqYY).
	/// </summary>
	private double? __analysisResult = null;

	/// <summary>
	/// Daily time series being analyzed.
	/// </summary>
	private DayTS __ts = null;

	/// <summary>
	/// Number of values to average.
	/// </summary>
	private int __numberInAverage = 7;

	/// <summary>
	/// Recurrence interval.
	/// </summary>
	private double __recurrenceInterval = 10;

	/// <summary>
	/// Start of analysis or null to analyze all data.
	/// </summary>
	private DateTime __analysisStart = null;

	/// <summary>
	/// End of analysis or null to analyze all data.
	/// </summary>
	private DateTime __analysisEnd = null;

	/// <summary>
	/// Number of missing values allowed in each averaging period.
	/// </summary>
	private int __allowMissingCount = 0;

	/// <summary>
	/// Construct the analysis object. </summary>
	/// <param name="ts"> daily time series to analyze. </param>
	/// <param name="n"> Number of daily values to average (7 for 7q10). </param>
	/// <param name="yy"> Recurrence interval in years (10 for 7q10). </param>
	/// <param name="analysisStart"> the starting date/time for analysis (only the year is used). </param>
	/// <param name="analysisEnd"> the ending date/time for analysis (only the year is used). </param>
	/// <param name="allowMissingCount"> number of values allowed to be missing in the averaging period </param>
	public NqYYFrequencyAnalysis(DayTS ts, int n, double yy, DateTime analysisStart, DateTime analysisEnd, int allowMissingCount)
	{
		setTS(ts);
		// For now only allow odd number of days so that even number is on each side.
		if (n % 2 != 1)
		{
			throw new InvalidParameterException("Number of values to average must be odd.");
		}
		setNumberInAverage(n);
		setRecurrenceInterval(yy);
		if (analysisStart != null)
		{
			setAnalysisStart(new DateTime(analysisStart));
		}
		if (analysisEnd != null)
		{
			setAnalysisEnd(new DateTime(analysisEnd));
		}
		setAllowMissingCount(allowMissingCount);
	}

	/// <summary>
	/// Perform the analysis.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void analyze() throws Exception
	public virtual void analyze()
	{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		string routine = this.GetType().FullName + ".analyze";
		DayTS ts = getTS();
		int numberInAverage = getNumberInAverage(); // Number expected to be included in each averaging period
		int allowMissingCount = getAllowMissingCount(); // Number of values allowed to be missing in each averaging period
		int bracket = numberInAverage / 2; // number on each side to average
		double recurrenceInterval = getRecurrenceInterval();
		DateTime analysisStart = getAnalysisStart();
		DateTime analysisEnd = getAnalysisEnd();

		// Adjust the analysis period to be full years.

		if (analysisStart == null)
		{
			analysisStart = new DateTime(ts.getDate1());
		}
		if (analysisEnd == null)
		{
			analysisEnd = new DateTime(ts.getDate2());
		}

		// Number of average values to analyze (one per year) - may have missing values after initial fill.
		double[] yearMinimumArray = new double[analysisEnd.getYear() - analysisStart.getYear() + 1];

		int yearEnd = analysisEnd.getYear();
		double value; // time series value to analyze
		double sum;
		int sumCount = 0; // number of values to average
		double average; // average for values bracketing day
		DateTime dt = new DateTime(analysisStart); // used for iteration
		int dayIncrement; // Value to increment day at start of "day" loop.
		DateTime dtSample = new DateTime(dt); // Date/time for sample loop
		DateTime yearMinimumDateTime = null; // DateTime for minimum N day average in year (center day)
		// Loop through the years to analyze
		for (int year = analysisStart.getYear(); year <= yearEnd; year++)
		{
			// Loop through the days in the year
			double yearMinimum = double.MaxValue;
			dt.setYear(year);
			dt.setMonth(1);
			dt.setDay(1);
			dayIncrement = 0;
			int daysInYear = TimeUtil.numDaysInYear(year);
			for (int day = 1; day <= daysInYear; day++)
			{
				// Get the values needed for the average...
				dt.addDay(dayIncrement);
				if (dayIncrement == 0)
				{
					dayIncrement = 1;
				}
				// For troubleshooting
				// Message.printStatus(2, routine, "Starting 7-day analysis on " + dt );
				dtSample.setMonth(dt.getMonth());
				dtSample.setYear(dt.getYear());
				dtSample.setDay(dt.getDay()); // Set day last because leap year may cause problem otherwise
				sum = 0;
				sumCount = 0;
				// Shift to initial offset day...
				dtSample.addDay(-bracket);
				for (int dayOffset = -bracket; dayOffset <= bracket; dayOffset++, dtSample.addDay(1))
				{
					value = ts.getDataValue(dtSample);
					if (!ts.isDataMissing(value))
					{
						sum += value;
						++sumCount;
					}
				}
				// Compute the average value and check for the minimum
				if (sumCount >= (numberInAverage - allowMissingCount))
				{
					average = sum / sumCount;
					if (average < yearMinimum)
					{
						yearMinimum = average;
						yearMinimumDateTime = new DateTime(dt);
					}
				}
				// Add to the main array
				yearMinimumArray[year - analysisStart.getYear()] = yearMinimum;
			}
			Message.printStatus(2, routine, "Minimum " + numberInAverage + " average for " + year + " is " + yearMinimum + " centered on day " + yearMinimumDateTime);
		}
		// Discard values in the array for years that could not be computed
		int discardCount = 0;
		for (int i = 0; i < yearMinimumArray.Length; i++)
		{
			if (yearMinimumArray[i] == double.MaxValue)
			{
				++discardCount;
			}
		}
		double[] yearMinimumArray2 = yearMinimumArray;
		if (discardCount > 0)
		{
			int nonMissingCount = 0;
			yearMinimumArray2 = new double[yearMinimumArray.Length - discardCount];
			for (int i = 0; i < yearMinimumArray.Length; i++)
			{
				if (yearMinimumArray[i] != double.MaxValue)
				{
					yearMinimumArray2[nonMissingCount++] = yearMinimumArray[i];
				}
			}
			Message.printStatus(2, routine, "Ignored " + discardCount + " years because not enough data.");
		}

		// Determine the YY recurrence interval value...

		LogPearsonType3Distribution dist = new LogPearsonType3Distribution(yearMinimumArray2);
		double result = dist.calculateForRecurrenceInterval(recurrenceInterval);
		setAnalysisResult(result);
	}

	/// <summary>
	/// Return the number of values allowed in each averaging period. </summary>
	/// <returns> the number of values allowed in each averaging period. </returns>
	public virtual int getAllowMissingCount()
	{
		return __allowMissingCount;
	}

	/// <summary>
	/// Return the analysis end. </summary>
	/// <returns> the analysis end, or null if the entire period is being analyzed. </returns>
	public virtual DateTime getAnalysisEnd()
	{
		return __analysisEnd;
	}

	/// <summary>
	/// Return the analysis result. </summary>
	/// <returns> the analysis result, or null if not computed (e.g., too much missing data). </returns>
	public virtual double? getAnalysisResult()
	{
		return __analysisResult;
	}

	/// <summary>
	/// Return the analysis start. </summary>
	/// <returns> the analysis start, or null if the entire period is being analyzed. </returns>
	public virtual DateTime getAnalysisStart()
	{
		return __analysisStart;
	}

	/// <summary>
	/// Return the number of values in the average. </summary>
	/// <returns> the number of values in the average. </returns>
	public virtual int getNumberInAverage()
	{
		return __numberInAverage;
	}

	/// <summary>
	/// Return the recurrence interval. </summary>
	/// <returns> the recurrence interval. </returns>
	public virtual double getRecurrenceInterval()
	{
		return __recurrenceInterval;
	}

	/// <summary>
	/// Return the time series being analyzed. </summary>
	/// <returns> the time series being analyzed. </returns>
	public virtual DayTS getTS()
	{
		return __ts;
	}

	/// <summary>
	/// Set the number of values allowed to be missing in each averaging period. </summary>
	/// <param name="the"> number of values allowed to be missing in each averaging period. </param>
	private void setAllowMissingCount(int allowMissingCount)
	{
		__allowMissingCount = allowMissingCount;
	}

	/// <summary>
	/// Set the end of the analysis period. </summary>
	/// <param name="analysisEnd"> ending date/time for the analysis. </param>
	private void setAnalysisEnd(DateTime analysisEnd)
	{
		__analysisEnd = analysisEnd;
	}

	/// <summary>
	/// Set the analysis result. </summary>
	/// <param name="analysisResult"> result of the analysis. </param>
	private void setAnalysisResult(double analysisResult)
	{
		__analysisResult = new double?(analysisResult);
	}

	/// <summary>
	/// Set the start of the analysis period. </summary>
	/// <param name="analysisStart"> starting date/time for the analysis. </param>
	private void setAnalysisStart(DateTime analysisStart)
	{
		__analysisStart = analysisStart;
	}

	/// <summary>
	/// Set the number of values in the average. </summary>
	/// <param name="numberInAverage"> number of values in the average for analysis. </param>
	private void setNumberInAverage(int numberInAverage)
	{
		__numberInAverage = numberInAverage;
	}

	/// <summary>
	/// Set the recurrence interval. </summary>
	/// <param name="recurrenceInterval"> recurrence interval for analysis. </param>
	private void setRecurrenceInterval(double recurrenceInterval)
	{
		__recurrenceInterval = recurrenceInterval;
	}

	/// <summary>
	/// Set the time series being analyzed. </summary>
	/// <param name="ts"> time series being analyzed. </param>
	private void setTS(DayTS ts)
	{
		__ts = ts;
	}

	}

}