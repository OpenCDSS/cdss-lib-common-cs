using System.Collections.Generic;

// TSExceedanceProbabilityAnalysis - Exceedance probability analysis

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

	/// <summary>
	/// Exceedance probability analysis.  The original time series data are processed to
	/// create new time series with values that correspond to the exceedance probability values.
	/// The time series can then be graphed using appropriate visualization techniques, including
	/// line graph, stacked-area plot, or stacked bar chart.
	/// </summary>
	public class TSExceedanceProbabilityAnalysis
	{

	/// <summary>
	/// The time series ensemble that is being processed.
	/// </summary>
	private TSEnsemble __ensemble = null;

	/// <summary>
	/// The input time series used to compute the exceedance probability time series.
	/// </summary>
	private IList<TS> __tsList = new List<TS>();

	/// <summary>
	/// The exceedance probabilities to calculate output time series.
	/// </summary>
	private double[] __probabilities = new double[0];

	/// <summary>
	/// The time series that correspond to the exceedance probabilities.
	/// </summary>
	private IList<TS> __probtsList = new List<TS>();

	/// <summary>
	/// Construct an exceedance probability analysis from an ensemble.
	/// Weibull plotting positions are used to calculate the probabilities and numerical values are
	/// interpolated to fit the exact probabilities that are passed in. </summary>
	/// <param name="ensemble"> the time series ensemble for which to compute the exceedance probability time series </param>
	/// @param  </param>
	public TSExceedanceProbabilityAnalysis(TSEnsemble ensemble, double[] probabilities)
	{
		__ensemble = ensemble;
		if (__ensemble != null)
		{
			__tsList = ensemble.getTimeSeriesList(false);
		}
		__probabilities = probabilities;
		if (__probabilities == null)
		{
			__probtsList = new List<TS>();
		}
		else
		{
			__probtsList = new List<TS>(__probabilities.Length);
		}

		analyze();
	}

	/// <summary>
	/// Analyze the time series, resulting in the probability time series.
	/// </summary>
	private void analyze()
	{
		// Create time series for the minimum, maximum, and probability exceedance values
		// The output time series have the same header information as the input time series except:
		//   - for probabilities the data type is appended with '-N%' where N is the exceedance probability
		//   - for statistics the min, max, etc. are added to the data type
		// What about the alias?
	}

	/// <summary>
	/// Return the ensemble that is used as input for the analysis. </summary>
	/// <returns> the ensemble that is used as input for the analysis (null if a simple list of time series
	/// is used as input). </returns>
	public virtual TSEnsemble getEnsemble()
	{
		return __ensemble;
	}

	/// <summary>
	/// Return the list of probability time series. </summary>
	/// <returns> the list of probability time series. </returns>
	public virtual IList<TS> getProbabilityTimeSeriesList()
	{
		return __probtsList;
	}

	/// <summary>
	/// Return the list of time series that is used as input for the analysis. </summary>
	/// <returns> the list of time series that is used as input for the analysis (guaranteed to be non-null). </returns>
	public virtual IList<TS> getTimeSeriesList()
	{
		return __tsList;
	}

	}

}