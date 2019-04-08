using System;

// TSDurationAnalysis - analyze time series and produce duration data (the percent of time a value is exceeded).

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
// TSDurationAnalysis - analyze time series and produce duration data (the
//			percent of time a value is exceeded).
// ----------------------------------------------------------------------------
// History:
//
// 30 Oct 2000	Steven A. Malers, RTi	Initial version to support TSTool.
// 2001-11-06	SAM, RTi		Review javadoc.  Verify that variables
//					are set to null when no longer used.
// ----------------------------------------------------------------------------

namespace RTi.TS
{
	using MathUtil = RTi.Util.Math.MathUtil;
	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// The TSDurationAnalysis class performs a duration analysis on a single time
	/// series.  The results include the percentage of time that a data value is exceeded.
	/// </summary>
	public class TSDurationAnalysis
	{

	// Data members...

	/// <summary>
	/// Time series to analyze.
	/// </summary>
	private TS _ts = null;

	/// <summary>
	/// Percent of time that values are exceeded.
	/// </summary>
	private double[] _percents = null;

	/// <summary>
	/// Data values.
	/// </summary>
	private double[] _values = null;

	/// <summary>
	/// Perform a duration analysis using the specified time series. </summary>
	/// <param name="ts"> The time series supplying values. </param>
	/// <exception cref="TSException"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TSDurationAnalysis(TS ts) throws TSException
	public TSDurationAnalysis(TS ts)
	{
		initialize(ts);
		analyze();
	}

	/// <summary>
	/// Perform a duration analysis using the specified time series. </summary>
	/// <param name="ts"> The time series supplying values. </param>
	/// <param name="analyze"> indicate whether analysis should occur. </param>
	/// <exception cref="TSException"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private TSDurationAnalysis(TS ts, boolean analyze) throws TSException
	private TSDurationAnalysis(TS ts, bool analyze)
	{
		initialize(ts);
		if (analyze)
		{
			analyze();
		}
	}

	/// <summary>
	/// Analyze the time series and produce the duration data. </summary>
	/// <exception cref="TSException"> if there is a problem performing the analysis. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void analyze() throws TSException
	private void analyze()
	{
		string message = "";

		if (_ts == null)
		{
			message = "Null time series for analysis";
			Message.printWarning(3, "TSDurationAnalysis.analyze",message);
			throw new TSException(message);
		}

		// Get the data as an array...

		double[] values0 = null;
		try
		{
			values0 = TSUtil.toArray(_ts, _ts.getDate1(), _ts.getDate2());
		}
		catch (Exception)
		{
			message = "Error converting time series " + _ts.getIdentifier() + " to array.";
			_values = null;
			values0 = null;
			Message.printWarning(3, "TSDurationAnalysis.analyze",message);
			throw new TSException(message);
		}

		if (values0 == null)
		{
			message = "Error converting time series " + _ts.getIdentifier() + " to array.";
			_values = null;
			Message.printWarning(3, "TSDurationAnalysis.analyze",message);
			throw new TSException(message);
		}

		// Count the missing values...

		int size = values0.Length;
		int nmissing = 0;
		for (int i = 0; i < size; i++)
		{
			if (_ts.isDataMissing(values0[i]))
			{
				++nmissing;
			}
		}

		// Now resize the array by throwing out missing values...

		int newsize = size;
		if (nmissing == 0)
		{
			// Just use what came back...
			_values = values0;
		}
		else
		{
			// Transfer only the non-missing values...
			newsize = size - nmissing;
			_values = new double[newsize];
			int j = 0;
			for (int i = 0; i < size; i++)
			{
				if (!_ts.isDataMissing(values0[i]))
				{
					_values[j++] = values0[i];
				}
			}
			// Don't need anymore...
			values0 = null;
		}

		// Sort into descending order.  Duplicates are OK...

		if (MathUtil.sort(_values, MathUtil.SORT_QUICK, MathUtil.SORT_DESCENDING, null, false) != 0)
		{
			_values = null;
			message = "Error sorting time series data " + _ts.getIdentifier();
			_values = null;
			Message.printWarning(3, "TSDurationAnalysis.analyze",message);
			throw new TSException(message);
		}

		// Now assign the percentages...

		_percents = new double[newsize];
		for (int i = 0; i < newsize; i++)
		{
			// Simple plotting positions...
			_percents[i] = (((double)(i) + 1.0) / (double)(newsize)) * 100.0;
		}
		message = null;
		values0 = null;

		// Quick test for the filtered analysis
		/*
		Message.printStatus(2, "", "Testing filtering..." );
		TSDurationAnalysis da = filterResultsUsingValueDifference ( (_ts.getDataLimits().getMaxValue() - _ts.getDataLimits().getMinValue())/100.0 );
		double [] percents = da.getPercents();
		double [] values = da.getValues();
		for ( int i = 0; i < percents.length; i++ ) {
		    Message.printStatus(2, "", "Filtered percent = " + percents[i] + " value=" + values[i]);
		}
		setPercents ( percents );
		setValues ( values );
		*/
	}

	/// <summary>
	/// Get the duration analysis filtered by throwing out intermediate points where the difference between two
	/// points has a data value difference that is less than the specified amount.  This is useful, for example,
	/// to improve visualization tools where fewer points are appropriate.  The returned object is a clone of the
	/// original analysis but with fewer points.  For analysis with many points, this typically reduced the number
	/// of points in the flat parts of the curve. </summary>
	/// <param name="requiredValueDiff"> required difference between data points' data values (e.g., take max - min/200). </param>
	/// <returns> a TSDurationAnalysis object that has been filtered to reduce the number of points </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TSDurationAnalysis filterResultsUsingValueDifference(double requiredValueDiff) throws TSException
	public virtual TSDurationAnalysis filterResultsUsingValueDifference(double requiredValueDiff)
	{
		// If the original arrays are null, the analysis may not have been performed, so do it
		double[] percents = getPercents();
		double[] values = getValues();
		if (percents == null)
		{
			analyze();
		}
		// Initially use an array size that is the same as the current results - change at end
		// Use primitives and not objects to improve performance
		double[] filteredPercents = new double[percents.Length];
		double[] filteredValues = new double[values.Length];
		int filteredCount = 0; // Count of retained points
		// Loop through and evaluate the percents...
		// Always add the first and last points
		filteredPercents[filteredCount] = percents[0];
		filteredValues[filteredCount++] = values[0];
		// TODO SAM 2009-08-03 Evaluate whether to also always add intermediate points (like exactly 10%), but if
		// all we wanted were even percents we'd probably add a different method
		int iend = percents.Length - 2;
		double diff;
		// Since first point has been added, examine the 2nd compared with the 1st, etc.
		int iVal = 0; // Location of last point that has been saved
		int iSearch = iVal + 1; // Location of point being checked
		while ((iVal < iend) && (iSearch < iend))
		{
			// Increment the index of the value being checked...
			iSearch = iVal + 1;
			while (iSearch < iend)
			{
				// Handle typical direction of change and negatives...
				diff = values[iVal] - values[iSearch];
				if (diff < 0)
				{
					diff += -1.0;
				}
				if (diff >= requiredValueDiff)
				{
					// The difference in values is >= the required so add the point
					// and reset the current value to the last saved value
					//Message.printStatus(2, "", "Found point to keep at " + iSearch + " iVal=" + iVal + " diff=" + diff);
					filteredPercents[filteredCount] = percents[iSearch];
					filteredValues[filteredCount++] = values[iSearch];
					iVal = iSearch;
					break; // To go to external loop to restart the search for the next point
				}
				else
				{
					// Else the point is not acceptable so advance the search
					//Message.printStatus(2, "", "Skipping value at " + iSearch + " iVal=" + iVal + " diff=" + diff);
					++iSearch;
				}
			}
		}
		Message.printStatus(3, "", "Filtered duration reduced from " + percents.Length + " points to " + filteredCount);
		// Always add the last point
		filteredPercents[filteredCount] = percents[percents.Length - 1];
		filteredValues[filteredCount++] = values[values.Length - 1];
		// Resize the arrays to final size and return the new TSDurationAnalysis object
		double[] filteredValuesSized = new double[filteredCount];
		double[] filteredPercentsSized = new double[filteredCount];
		Array.Copy(filteredValues, 0, filteredValuesSized, 0, filteredCount);
		Array.Copy(filteredPercents, 0, filteredPercentsSized, 0, filteredCount);
		TSDurationAnalysis filteredDA = new TSDurationAnalysis(this.getTS(), false);
		filteredDA.setValues(filteredValuesSized);
		filteredDA.setPercents(filteredPercentsSized);
		return filteredDA;
	}

	/// <summary>
	/// Return the time series that was analyzed. </summary>
	/// <returns> the time series that was analyzed. </returns>
	public virtual TS getTS()
	{
		return _ts;
	}

	/// <summary>
	/// Return the percents (0 to 100) or null if data have not been successfully analyzed. </summary>
	/// <returns> the percents array. </returns>
	public virtual double [] getPercents()
	{
		return _percents;
	}

	/// <summary>
	/// Return the values or null if data have not been successfully analyzed. </summary>
	/// <returns> data values array. </returns>
	public virtual double [] getValues()
	{
		return _values;
	}

	/// <summary>
	/// Initialize the object. </summary>
	/// <param name="ts"> Time series to analyze. </param>
	private void initialize(TS ts)
	{
		_ts = ts;
		_values = null;
		_percents = null;
	}

	/// <summary>
	/// Set the array of percents. </summary>
	/// <param name="percents"> array of percents to set. </param>
	private void setPercents(double[] percents)
	{
		_percents = percents;
	}

	/// <summary>
	/// Set the array of values. </summary>
	/// <param name="values"> array of values to set. </param>
	private void setValues(double[] values)
	{
		_values = values;
	}

	}

}