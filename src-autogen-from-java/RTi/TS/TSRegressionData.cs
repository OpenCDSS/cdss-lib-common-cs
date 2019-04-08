// TSRegressionData - Immutable data used as input to a TSRegressionAnalysis

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
	using MathUtil = RTi.Util.Math.MathUtil;
	using RegressionData = RTi.Util.Math.RegressionData;

	/// <summary>
	/// Immutable data used as input to a TSRegressionAnalysis.  Data for a single relationship as well as monthly
	/// relationships are stored.
	/// </summary>
	public class TSRegressionData
	{

	/// <summary>
	/// Dependent time series.
	/// </summary>
	internal TS __dependentTS = null;

	/// <summary>
	/// Independent time series.
	/// </summary>
	internal TS __independentTS = null;

	/// <summary>
	/// Regression data using a single equation.
	/// </summary>
	internal RegressionData __singleEquationData = null;

	/// <summary>
	/// Regression data using monthly equations.
	/// </summary>
	internal RegressionData[] __monthlyEquationData = null;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="singleEquationData"> regression data for a single equation (may contain a subset of months) </param>
	/// <param name="monthlyEquationData"> regression data for each month (may only be complete for a subset of months). </param>
	public TSRegressionData(TS independentTS, TS dependentTS, RegressionData singleEquationData, RegressionData[] monthlyEquationData)
	{
		__independentTS = independentTS;
		__dependentTS = dependentTS;
		__singleEquationData = singleEquationData;
		__monthlyEquationData = monthlyEquationData;
	}

	/// <summary>
	/// Return the dependent (Y) time series. </summary>
	/// <returns> the dependent (Y) time series. </returns>
	public virtual TS getDependentTS()
	{
		return __dependentTS;
	}

	/// <summary>
	/// Return the independent (X) time series. </summary>
	/// <returns> the independent (X) time series. </returns>
	public virtual TS getIndependentTS()
	{
		return __independentTS;
	}

	/// <summary>
	/// Return a monthly equation regression data. </summary>
	/// <returns> a monthly equation regression data. </returns>
	/// <param name="month"> the month of interest (1-12). </param>
	public virtual RegressionData getMonthlyEquationRegressionData(int month)
	{
		return __monthlyEquationData[month - 1];
	}

	/// <summary>
	/// Return the single equation regression data. </summary>
	/// <returns> the single equation regression data. </returns>
	public virtual RegressionData getSingleEquationRegressionData()
	{
		return __singleEquationData;
	}

	/// <summary>
	/// Return a new copy of this instance where all of the data arrays have been transformed by log10. </summary>
	/// <param name="leZeroLog10"> the value to assign when the input data are <= 0 (e.g., .001 or Double.NaN). </param>
	/// <param name="monthly"> whether or not monthly equations are being used (so we don't transform data that we aren't using). </param>
	/// <param name="single"> whether or not a single equation is being used (so we don't transform data that we aren't using). </param>
	public virtual TSRegressionData transformLog10(double leZeroLog10, bool monthly, bool single)
	{
		//initialize as empty and add data if it is needed
		RegressionData singleEquationDataTransformed = null;
		RegressionData[] monthlyEquationDataTransformed = new RegressionData[12];
		if (monthly)
		{
			//transform monthly data
			for (int iMonth = 1; iMonth <= 12; iMonth++)
			{
				double[] x1Transformed = MathUtil.log10(getMonthlyEquationRegressionData(iMonth).getX1(), leZeroLog10);
				double[] y1Transformed = MathUtil.log10(getMonthlyEquationRegressionData(iMonth).getY1(), leZeroLog10);
				double[] x2Transformed = MathUtil.log10(getMonthlyEquationRegressionData(iMonth).getX2(), leZeroLog10);
				double[] y3Transformed = MathUtil.log10(getMonthlyEquationRegressionData(iMonth).getY3(), leZeroLog10);
				monthlyEquationDataTransformed[iMonth - 1] = new RegressionData(x1Transformed, y1Transformed, x2Transformed, y3Transformed);
			}
		}
		if (single)
		{
			//transform single data
			double[] x1Transformed = MathUtil.log10(getSingleEquationRegressionData().getX1(), leZeroLog10);
			double[] y1Transformed = MathUtil.log10(getSingleEquationRegressionData().getY1(), leZeroLog10);
			double[] x2Transformed = MathUtil.log10(getSingleEquationRegressionData().getX2(), leZeroLog10);
			double[] y3Transformed = MathUtil.log10(getSingleEquationRegressionData().getY3(), leZeroLog10);
			singleEquationDataTransformed = new RegressionData(x1Transformed, y1Transformed, x2Transformed, y3Transformed);
		}
		return new TSRegressionData(getIndependentTS(), getDependentTS(), singleEquationDataTransformed, monthlyEquationDataTransformed);
	}

	}

}