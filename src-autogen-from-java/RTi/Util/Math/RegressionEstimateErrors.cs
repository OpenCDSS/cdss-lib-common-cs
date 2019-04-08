using System;

// RegressionEstimateErrors - provides storage for regression analysis errors

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

namespace RTi.Util.Math
{
	using TDistribution = org.apache.commons.math3.distribution.TDistribution;

	/// <summary>
	/// This class provides storage for regression analysis errors, determined by using the
	/// results of the regression analysis to estimate values that were previously known (in N1) and then measuring the
	/// error between the original value and the estimated value.  This class currently only stores the results of
	/// the estimation - the calculations are performed elsewhere due to complications with transformations, etc.
	/// </summary>
	public class RegressionEstimateErrors
	{

	/// <summary>
	/// Regression data used as input to compute the relationships.
	/// </summary>
	private RegressionData __regressionData = null;

	/// <summary>
	/// Regression results analyzing the relationships.
	/// </summary>
	private RegressionResults __regressionResults = null;

	/// <summary>
	/// Array of estimated dependent values in N1.
	/// </summary>
	private double[] __Y1est = new double[0];

	/// <summary>
	/// Mean of estimated Y in N1.
	/// </summary>
	private double? __meanY1est = null;

	/// <summary>
	/// Standard deviation of estimated Y in N1.
	/// </summary>
	private double? __stddevY1est = null;

	/// <summary>
	/// RMS Error for untransformed data.
	/// </summary>
	private double? __rmse = null;

	/// <summary>
	/// Standard error of estimate for untransformed data.
	/// </summary>
	private double? __see = null;

	/// <summary>
	/// Standard error of slope, for untransformed values.
	/// </summary>
	private double? __seSlope = null;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="Y1est"> dependent time series values estimated using the regression relationship </param>
	/// <param name="rmse"> RMSE computed for the estimated values </param>
	/// <param name="see"> standard error of estimate computed for the estimated values </param>
	/// <param name="seSlope"> standard error of the slope computed for the estimated values </param>
	public RegressionEstimateErrors(RegressionData data, RegressionResults results, double[] Y1est, double? rmse, double? see, double? seSlope)
	{
		setRegressionData(data);
		setRegressionResults(results);
		setY1est(Y1est);
		setRMSE(rmse);
		setStandardErrorOfEstimate(see);
		setStandardErrorOfSlope(seSlope);
	}

	/// <summary>
	/// Finalize before garbage collection. </summary>
	/// <exception cref="Throwable"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~RegressionEstimateErrors()
	{
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}
	*/ public virtual double? getMeanY1est()
	{
		double? meanY1est = __meanY1est;
		if (meanY1est == null)
		{
			meanY1est = MathUtil.mean(getY1est());
			setMean1Yest(meanY1est);
		}
		return meanY1est;
	}

	/// <summary>
	/// Return the RMS error (in original data units), or null if not analyzed. </summary>
	/// <returns> The RMS error (in original data units), or null if not analyzed. </returns>
	public virtual double? getRMSE()
	{
		return __rmse;
	}

	/// <summary>
	/// Return the regression data used for the error estimate.
	/// </summary>
	public virtual RegressionData getRegressionData()
	{
		return __regressionData;
	}

	/// <summary>
	/// Return the regression results used for the error estimate.
	/// </summary>
	public virtual RegressionResults getRegressionResults()
	{
		return __regressionResults;
	}

	/// <summary>
	/// Return the standard deviation for the estimated dependent array, or null if not analyzed. </summary>
	/// <returns> the standard deviation for the estimated dependent array, or null if not analyzed. </returns>
	public virtual double? getStandardDeviationY1est()
	{
		double? stddevY1est = __stddevY1est;
		try
		{
			if (stddevY1est == null && getY1est() != null && getY1est().Length > 1)
			{
				stddevY1est = MathUtil.standardDeviation(getY1est());
				setStandardDeviationY1est(stddevY1est);
			}
		}
		catch (System.ArgumentException)
		{
			//problem calculating it....
			stddevY1est = Double.NaN;
		}
		return stddevY1est;
	}

	/// <summary>
	/// Return the standard error of estimate, or null if not analyzed. </summary>
	/// <returns> the standard error of estimate, or null if not analyzed. </returns>
	public virtual double? getStandardErrorOfEstimate()
	{
		return __see;
	}

	/// <summary>
	/// Return the standard error of the slope in original data units, or null if not analyzed. </summary>
	/// <returns> the standard error of the slope in original data units, or null if not analyzed. </returns>
	public virtual double? getStandardErrorOfSlope()
	{
		return __seSlope;
	}

	/// <summary>
	/// Return the Student T Test quantile for the confidence interval (to be evaluated against the test score
	/// for the slope of the regression line), to determine if it is a good relationship </summary>
	/// <param name="confidenceIntervalPercent"> the confidence interval as a percent </param>
	/// <returns> null if unable to compute the quantile </returns>
	public virtual double? getStudentTTestQuantile(double? confidenceIntervalPercent)
	{
		if (confidenceIntervalPercent == null)
		{
			//Message.printStatus(2,routine,"confidenceIntervalPercent is null - not computing quantile");
			return null;
		}
		if (getRegressionData() == null)
		{
			return null;
		}
		// Single tail exceedance probability in range 0 to 1.0
		// For example, a confidence interval of 95% will have p = .05
		double alpha = (100 - confidenceIntervalPercent) / 100.0;
		double alpha2 = alpha / 2.0;
		double[] Y1 = getRegressionData().getY1();
		double[] Y1est = getY1est();
		if (Y1 == null)
		{
			return null;
		}
		if (Y1est == null)
		{
			return null;
		}
		// Length of array will be N1 by definition - subtract 2 for the intercept and slope to get
		// the degrees of freedom
		int dof = Y1est.Length - 2;
		double? quantile = null;
		//commented out to make looking through logs easier
		//Message.printStatus(2,routine, "alpha=" + alpha );
		//Message.printStatus(2,routine, "n=" + Y1est.length );
		//Message.printStatus(2,routine, "dof=" + dof );
		try
		{
			bool useApacheMath = true;
			if (useApacheMath)
			{
				// Why is degrees of freedom a double?
				TDistribution tDist = new TDistribution(dof);
				//Get the value at which this confidence interval is satisfied
				quantile = -1 * tDist.inverseCumulativeProbability(alpha2);
			}
			else
			{
				StudentTTest t = new StudentTTest();
				quantile = t.getStudentTQuantile(alpha2, dof);
			}
		}
		catch (Exception)
		{
			// typically dof too small
			quantile = null; // Not computed
		}
		return quantile;
	}

	/// <summary>
	/// Return the Student T Test score as b/SEslope
	/// See https://en.wikipedia.org/wiki/Student%27s_t-test#Slope_of_a_regression_line or
	/// http://stattrek.com/regression/slope-test.aspx for more details on why this works </summary>
	/// <param name="b"> the slope of the regression line </param>
	/// <returns> null if unable to compute the test score or Double.POSITIVE_INFINITY if division by zero. </returns>
	public virtual double? getTestScore(double? b)
	{
		double? SESlope = getStandardErrorOfSlope();
		if ((b == null) || (SESlope == null))
		{
			return null;
		}
		if (SESlope == 0.0)
		{
			return double.PositiveInfinity;
		}
		return new double?(b / SESlope);
	}

	/// <summary>
	/// Determine whether the relationship is valid for the confidence interval specified during analysis </summary>
	/// <returns> true if the result of getTestScore() is >= the result from getStudentTTestQuantile()
	/// null if the inputs are not specified </returns>
	public virtual bool? getTestRelated(double? testScore, double? testQuantile)
	{
		//if either of these was not calculated, the test is bad, so return false
		if ((testScore == null) || (testQuantile == null))
		{
			return false;
		}
		else
		{
			return (testScore >= testQuantile);
		}
	}

	/// <summary>
	/// Return the estimated dependent array.
	/// </summary>
	public virtual double [] getY1est()
	{
		return __Y1est;
	}

	/// <summary>
	/// Remove the estimated dependent array for memory purposes.
	/// </summary>
	public virtual void clearY1est()
	{
		__Y1est = null;
	}

	/// <summary>
	/// Set the mean for the estimated dependent data. </summary>
	/// <param name="meanY1est"> Mean for the estimated dependent data. </param>
	private void setMean1Yest(double? meanY1est)
	{
		__meanY1est = meanY1est;
	}

	/// <summary>
	/// Set the regression data.
	/// </summary>
	private void setRegressionData(RegressionData regressionData)
	{
		__regressionData = regressionData;
	}

	/// <summary>
	/// Set the regression results.
	/// </summary>
	private void setRegressionResults(RegressionResults regressionResults)
	{
		__regressionResults = regressionResults;
	}

	/// <summary>
	/// Set the RMS error. </summary>
	/// <param name="rmse"> RMS error </param>
	private void setRMSE(double? rmse)
	{
		__rmse = rmse;
	}

	/// <summary>
	/// Set the standard deviation for the estimated dependent data. </summary>
	/// <param name="stddevY1est"> Standard deviation for the dependent data. </param>
	private void setStandardDeviationY1est(double? stddevY1est)
	{
		__stddevY1est = stddevY1est;
	}

	/// <summary>
	/// Set the standard error of estimate, in original data units. </summary>
	/// <param name="see"> the standard error of estimate, in original data units </param>
	private void setStandardErrorOfEstimate(double? see)
	{
		__see = see;
	}

	/// <summary>
	/// Set the standard error of the slope, in original data units. </summary>
	/// <param name="seSlope"> the standard error of the slope, in original data units </param>
	private void setStandardErrorOfSlope(double? seSlope)
	{
		__seSlope = seSlope;
	}

	/// <summary>
	/// Set the Yest array - this is only called by the setYest() method if the array has not been constructed. </summary>
	/// <param name="Y1est"> Yest array </param>
	private void setY1est(double[] Y1est)
	{
		__Y1est = Y1est;
	}

	}

}