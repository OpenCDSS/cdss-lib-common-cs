// Regression - linear regression

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

//------------------------------------------------------------------------------
// Regression - math regression class.
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 27 May 1998	Catherine E.		Created initial version.
//		Nutting-Lane, RTi
// 04 Nov 1998	Steven A. Malers, RTi	Add javadoc.  Still need to add
//					constructors, etc. that actually do the
//					regression.
// 09 Jan 1999	SAM, RTi		Add number of points.
// 13 Apr 1999	SAM, RTi		Add finalize.
// 28 Oct 2000	SAM, RTi		Add "lag_intervals" and set/get methods
//					for use by code that analyzes lagged
//					time series.  Add code to store and
//					access the minimum and maximum values
//					used in the analysis.  This is useful
//					when setting axis labels in graphs.
// 2002-03-18	SAM, RTi		Clarify documentation to use X and Y
//					rather than Q and S.
// 2002-03-24	SAM, RTi		Change methods around to be compatible
//					with general X versus Y regression.  For
//					example, change *Max1() to *MaxX().  Add
//					data and methods to support more complex
//					derived classes (e.g., add n1, n2).
// 2002-04-03	SAM, RTi		Add transformed RMSE so that both
//					transformed and untransformed error can
//					be reported.
// 2002-04-08	SAM, RTi		Update to have storage for Y mean and
//					standard deviation.
// 2002-05-14	SAM, RTi		Add parameter for forced intercept.
// 2005-04-26	J. Thomas Sapienza, RTi	Added all data members to finalize().
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//------------------------------------------------------------------------------
// EndHeader

namespace RTi.Util.Math
{
	/// <summary>
	/// This class provides storage for regression data and results.  This class can be
	/// extended for more complicated analysis.  At this time, regression is not
	/// implemented (only storage is).  This class was implemented as a base class for
	/// the RTi.TS.TSRegression class.  The actual regression occurs by calling
	/// MathUtil.regress() (or TSUtil regression methods).  This generic code is
	/// typically then called to fill in the information in this class and possibly a derived class.
	/// <para>
	/// 
	/// The regression determines the values of "a" and "b" in the relationship
	/// Y = a + b * X, where Y is a value to be determined (e.g., missing data in a
	/// dependent time series), a is the Y-intercept, b is the slope, and X is the known
	/// value (e.g., known value in independent time series).  The correlation
	/// coefficient (r) and RMS error (RMSE) are assumed to be computed in the normal fashion.
	/// </para>
	/// </summary>
	public class Regression
	{

	/// <summary>
	/// Forced intercept A in the computation code.
	/// </summary>
	protected internal double? _intercept = null;

	/// <summary>
	/// Y-intercept of best fit line.
	/// </summary>
	protected internal double _a = 0.0;

	/// <summary>
	/// Slope of best fit line.
	/// </summary>
	protected internal double _b = 0.0;

	/// <summary>
	/// Number of non-missing points in X and Y
	/// </summary>
	protected internal int _n1 = 0;

	/// <summary>
	/// Number of non-missing points in X that are not in _n1.
	/// </summary>
	protected internal int _n2 = 0;

	/// <summary>
	/// Standard error of estimate for untransformed data.
	/// </summary>
	protected internal double _see = 0.0;

	/// <summary>
	/// Standard error of estimate for transformed data.
	/// </summary>
	protected internal double _seeTransformed = 0.0;

	/// <summary>
	/// Correlation coefficient.
	/// </summary>
	protected internal double _r = 0.0;

	/// <summary>
	/// RMS Error for untransformed data.
	/// </summary>
	protected internal double _rmse = 0.0;

	/// <summary>
	/// RMS Error for transformed data.
	/// </summary>
	protected internal double _rmseTransformed = 0.0;

	/// <summary>
	/// Indicates whether the analysis has been completed.
	/// </summary>
	protected internal bool _is_analyzed = false;

	/// <summary>
	/// Intervals that the second time series is lagged compared to the first when performing the regression
	/// analysis.
	/// TODO SAM 2009-03-10 Need to be double?
	/// </summary>
	protected internal int _lag_intervals = 0;

	/// <summary>
	/// Maximum value of X in N1.
	/// </summary>
	protected internal double _max_x1 = 0.0;

	/// <summary>
	/// Maximum value of Y in N1.
	/// </summary>
	protected internal double _max_y1 = 0.0;

	/// <summary>
	/// Minimum value of X in N1.
	/// </summary>
	protected internal double _min_x1 = 0.0;

	/// <summary>
	/// Minimum value of Y in N1.
	/// </summary>
	protected internal double _min_y1 = 0.0;

	/// <summary>
	/// Mean of X in N1 + N2.
	/// </summary>
	protected internal double _mean_x = 0.0;

	/// <summary>
	/// Mean of X in N1.
	/// </summary>
	protected internal double _mean_x1 = 0.0;

	/// <summary>
	/// Mean of X in N2.
	/// </summary>
	protected internal double _mean_x2 = 0.0;

	/// <summary>
	/// Mean of Y in N1 + N2.
	/// </summary>
	protected internal double _mean_y = 0.0;

	/// <summary>
	/// Mean of Y in N1.
	/// </summary>
	protected internal double _mean_y1 = 0.0;

	/// <summary>
	/// Mean of estimated Y in N1.
	/// </summary>
	protected internal double _mean_y1_estimated = 0.0;

	/// <summary>
	/// Standard deviation of X in N1 + N2.
	/// </summary>
	protected internal double _stddev_x = 0.0;

	/// <summary>
	/// Standard deviation of X in N1.
	/// </summary>
	protected internal double _stddev_x1 = 0.0;

	/// <summary>
	/// Standard deviation of X in N2.
	/// </summary>
	protected internal double _stddev_x2 = 0.0;

	/// <summary>
	/// Standard deviation of X in N1 + N2.
	/// </summary>
	protected internal double _stddev_y = 0.0;

	/// <summary>
	/// Standard deviation of Y in N1.
	/// </summary>
	protected internal double _stddev_y1 = 0.0;

	/// <summary>
	/// Standard deviation of estimated Y in N1.
	/// </summary>
	protected internal double _stddev_y1_estimated = 0.0;

	/// <summary>
	/// Array of X1 values.
	/// </summary>
	protected internal double[] _X1;

	/// <summary>
	/// Array of Y1 values.
	/// </summary>
	protected internal double[] _Y1;

	/// <summary>
	/// Default constructor.  Typically the data in this class are set by the MathUtil.regress*() or similar methods.
	/// </summary>
	public Regression()
	{
	}

	/// <summary>
	/// Finalize before garbage collection. </summary>
	/// <exception cref="Throwable"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~Regression()
	{
		_X1 = null;
		_Y1 = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Indicate whether the intercept (A) for the relationship is forced. </summary>
	/// <returns> the forced intercept or null if not forced. </returns>
	public virtual double? getIntercept()
	{
		return _intercept;
	}

	/// <summary>
	/// Return A (intercept) in correlation equation. </summary>
	/// <returns> A (intercept) in correlation equation. </returns>
	public virtual double getA()
	{
		return _a;
	}

	/// <summary>
	/// Return B (slope) in correlation equation. </summary>
	/// <returns> B (slope) in correlation equation. </returns>
	public virtual double getB()
	{
		return _b;
	}

	/// <summary>
	/// Return the correlation coefficient (r) for the relationship (-1 to 1). </summary>
	/// <returns> The correlation coefficient for (r) the relationship (-1 to 1). </returns>
	public virtual double getCorrelationCoefficient()
	{
		return _r;
	}

	/// <summary>
	/// Return The number of data intervals that the second series has been lagged
	/// compared to the first.  This is used by higher-level code when performing an
	/// analysis.  This is a new feature that is being tested. </summary>
	/// <returns> The number of data intervals that the second series has been lagged compared to the first. </returns>
	public virtual int getLagIntervals()
	{
		return _lag_intervals;
	}

	/// <summary>
	/// Return the maximum data value for the independent array in the N1 space. </summary>
	/// <returns> the maximum data value for the independent array in the N1 space. </returns>
	public virtual double getMaxX1()
	{
		return _max_x1;
	}

	/// <summary>
	/// Return the maximum data value for the dependent array in the N1 space. </summary>
	/// <returns> the maximum data value for the dependent array in the N1 space. </returns>
	public virtual double getMaxY1()
	{
		return _max_y1;
	}

	/// <summary>
	/// Return the mean for the independent array in the N1 + N2 space. </summary>
	/// <returns> the mean for the independent array in the N1 + N2 space. </returns>
	public virtual double getMeanX()
	{
		return _mean_x;
	}

	/// <summary>
	/// Return the mean for the independent array in the N1 space. </summary>
	/// <returns> the mean for the independent array in the N1 space. </returns>
	public virtual double getMeanX1()
	{
		return _mean_x1;
	}

	/// <summary>
	/// Return the mean for the independent array in the N2 space. </summary>
	/// <returns> the mean for the independent array in the N2 space. </returns>
	public virtual double getMeanX2()
	{
		return _mean_x2;
	}

	/// <summary>
	/// Return the mean for the dependent array in the N1 + N2 space. </summary>
	/// <returns> the mean for the dependent array in the N1 + N2 space. </returns>
	public virtual double getMeanY()
	{
		return _mean_y;
	}

	/// <summary>
	/// Return the mean for the dependent array in the N1 space. </summary>
	/// <returns> the mean for the dependent array in the N1 space. </returns>
	public virtual double getMeanY1()
	{
		return _mean_y1;
	}

	/// <summary>
	/// Return the mean for the estimated dependent array in the N1 space. </summary>
	/// <returns> the mean for the estimated dependent array in the N1 space. </returns>
	public virtual double getMeanY1Estimated()
	{
		return _mean_y1_estimated;
	}

	/// <summary>
	/// Return the minimum data value for the independent array in the N1 space. </summary>
	/// <returns> the minimum data value for the independent array in the N1 space. </returns>
	public virtual double getMinX1()
	{
		return _min_x1;
	}

	/// <summary>
	/// Return the minimum data value for the dependent array in the N1 space. </summary>
	/// <returns> the minimum data value for the dependent array in the N1 space. </returns>
	public virtual double getMinY1()
	{
		return _min_y1;
	}

	/// <summary>
	/// Return the number of non-missing data points in X and Y. </summary>
	/// <returns> the number of non-missing data points in X and Y. </returns>
	public virtual int getN1()
	{
		return _n1;
	}

	/// <summary>
	/// Return the number of non-missing data points in X that are not in N1. </summary>
	/// <returns> the number of non-missing data points in X that are not in N1. </returns>
	public virtual int getN2()
	{
		return _n2;
	}

	/// <summary>
	/// Return the RMS error. </summary>
	/// <returns> The RMS error. </returns>
	public virtual double getRMSE()
	{
		return _rmse;
	}

	/// <summary>
	/// Return the RMS error for transformed data. </summary>
	/// <returns> The RMS error for transformed data. </returns>
	public virtual double getRMSETransformed()
	{
		return _rmseTransformed;
	}

	/// <summary>
	/// Return the standard deviation for the independent array in the N1 + N2 space. </summary>
	/// <returns> the standard deviation for the independent array in the N1 + N2 space. </returns>
	public virtual double getStandardDeviationX()
	{
		return _stddev_x;
	}

	/// <summary>
	/// Return the standard deviation for the independent array in the N1 space. </summary>
	/// <returns> the standard deviation for the independent array in the N1 space. </returns>
	public virtual double getStandardDeviationX1()
	{
		return _stddev_x1;
	}

	/// <summary>
	/// Return the standard deviation for the independent array in the N2 space. </summary>
	/// <returns> the standard deviation for the independent array in the N2 space. </returns>
	public virtual double getStandardDeviationX2()
	{
		return _stddev_x2;
	}

	/// <summary>
	/// Return the standard deviation for the dependent array in the N1 + N2 space. </summary>
	/// <returns> the standard deviation for the dependent array in the N1 + N2 space. </returns>
	public virtual double getStandardDeviationY()
	{
		return _stddev_y;
	}

	/// <summary>
	/// Return the standard deviation for the dependent array in the N1 space. </summary>
	/// <returns> the standard deviation for the dependent array in the N1 space. </returns>
	public virtual double getStandardDeviationY1()
	{
		return _stddev_y1;
	}

	/// <summary>
	/// Return the standard deviation for the estimated dependent array in the N1 space. </summary>
	/// <returns> the standard deviation for the estimated dependent array in the N1 space. </returns>
	public virtual double getStandardDeviationY1Estimated()
	{
		return _stddev_y1_estimated;
	}

	/// <summary>
	/// Return the standard error of estimate. </summary>
	/// <returns> the standard error of estimate </returns>
	public virtual double getStandardErrorOfEstimate()
	{
		return _see;
	}

	/// <summary>
	/// Return the standard error of estimate for transformed data. </summary>
	/// <returns> the standard error of estimate for transformed data. </returns>
	public virtual double getStandardErrorOfEstimateTransformed()
	{
		return _seeTransformed;
	}

	/// <summary>
	/// Return the X1 array. </summary>
	/// <returns> the X1 array. </returns>
	public virtual double [] getX1()
	{
		return _X1;
	}

	/// <summary>
	/// Return the Y1 array. </summary>
	/// <returns> the Y1 array. </returns>
	public virtual double [] getY1()
	{
		return _Y1;
	}

	/// <summary>
	/// Indicate whether the analysis has been completed. </summary>
	/// <returns> true if the analysis has been completed. </returns>
	public virtual bool isAnalyzed()
	{
		return _is_analyzed;
	}

	/// <summary>
	/// Set whether the analysis has been completed. </summary>
	/// <param name="is_analyzed"> true if the analysis is completed, false if not. </param>
	/// <returns> the analysis flag after setting. </returns>
	public virtual bool isAnalyzed(bool is_analyzed)
	{
		_is_analyzed = is_analyzed;
		return _is_analyzed;
	}

	/// <summary>
	/// Set the A value (Y intercept). </summary>
	/// <param name="a"> value to save as A. </param>
	public virtual void setA(double a)
	{
		_a = a;
	}

	/// <summary>
	/// Set the B value (slope). </summary>
	/// <param name="b"> Value to save as B. </param>
	public virtual void setB(double b)
	{
		_b = b;
	}

	/// <summary>
	/// Set the correlation coefficient. </summary>
	/// <param name="r"> Correlation coefficient. </param>
	public virtual void setCorrelationCoefficient(double r)
	{
		_r = r;
	}

	/// <summary>
	/// Set the intercept (A) for the relationship is forced. </summary>
	/// <param name="intercept"> Set the intercept value of A that has been forced.  The
	/// calculation of B should therefore use different equations (MathUtil.regress handles the option). </param>
	public virtual void setIntercept(double? intercept)
	{
		_intercept = intercept;
	}

	/// <summary>
	/// Set the number of data intervals that the second series has been lagged compared
	/// to the first.  This is used by higher-level code when performing an analysis. </summary>
	/// <param name="lag_intervals"> Number of intervals the second data set has been lagged compared to the first. </param>
	public virtual void setLagIntervals(int lag_intervals)
	{
		_lag_intervals = lag_intervals;
	}

	/// <summary>
	/// Set the maximum data value for the independent data in the N1 space. </summary>
	/// <param name="max_x1"> Maximum data value for the independent data in the N1 space. </param>
	public virtual void setMaxX1(double max_x1)
	{
		_max_x1 = max_x1;
	}

	/// <summary>
	/// Set the maximum data value for the dependent data in the N1 space. </summary>
	/// <param name="max_y1"> Maximum data value for the dependent data in the N1 space. </param>
	public virtual void setMaxY1(double max_y1)
	{
		_max_y1 = max_y1;
	}

	/// <summary>
	/// Set the mean for the independent data in the N1 + N2 space. </summary>
	/// <param name="mean_x"> Mean for the independent data in the N1 + N2 space. </param>
	public virtual void setMeanX(double mean_x)
	{
		_mean_x = mean_x;
	}

	/// <summary>
	/// Set the mean for the independent data in the N1 space. </summary>
	/// <param name="mean_x1"> Mean for the independent data in the N1 space. </param>
	public virtual void setMeanX1(double mean_x1)
	{
		_mean_x1 = mean_x1;
	}

	/// <summary>
	/// Set the mean for the independent data in the N2 space. </summary>
	/// <param name="mean_x2"> Mean for the independent data in the N2 space. </param>
	public virtual void setMeanX2(double mean_x2)
	{
		_mean_x2 = mean_x2;
	}

	/// <summary>
	/// Set the mean for the dependent data in the N1 + N2 space. </summary>
	/// <param name="mean_y"> Mean for the dependent data in the N1 + N2 space. </param>
	public virtual void setMeanY(double mean_y)
	{
		_mean_y = mean_y;
	}

	/// <summary>
	/// Set the mean for the dependent data in the N1 space. </summary>
	/// <param name="mean_y1"> Mean for the dependent data in the N1 space. </param>
	public virtual void setMeanY1(double mean_y1)
	{
		_mean_y1 = mean_y1;
	}

	/// <summary>
	/// Set the mean for the estimated dependent data in the N1 space. </summary>
	/// <param name="mean_y1_estimated"> Mean for the estimated dependent data in the N1 space. </param>
	public virtual void setMeanY1Estimated(double mean_y1_estimated)
	{
		_mean_y1_estimated = mean_y1_estimated;
	}

	/// <summary>
	/// Set the minimum data value for the independent data in the N1 space. </summary>
	/// <param name="min_x1"> Minimum data value for the independent data in the N1 space. </param>
	public virtual void setMinX1(double min_x1)
	{
		_min_x1 = min_x1;
	}

	/// <summary>
	/// Set the minimum data value for the dependent data in the N1 space. </summary>
	/// <param name="min_y1"> Minimum data value for the dependent data in the N1 space. </param>
	public virtual void setMinY1(double min_y1)
	{
		_min_y1 = min_y1;
	}

	/// <summary>
	/// Set the number of non-missing points in X and Y. </summary>
	/// <param name="n1"> Number of non-missing points in X and Y. </param>
	public virtual void setN1(int n1)
	{
		_n1 = n1;
	}

	/// <summary>
	/// Set the number of non-missing points in X that are not in the N1 space. </summary>
	/// <param name="n2"> Number of non-missing points in X that are not in the N1 space. </param>
	public virtual void setN2(int n2)
	{
		_n2 = n2;
	}

	/// <summary>
	/// Set the RMS error. </summary>
	/// <param name="rmse"> RMS error. </param>
	public virtual void setRMSE(double rmse)
	{
		_rmse = rmse;
	}

	/// <summary>
	/// Set the transformed RMS error. </summary>
	/// <param name="rmseTransformed"> transformed RMS error. </param>
	public virtual void setRMSETransformed(double rmseTransformed)
	{
		_rmseTransformed = rmseTransformed;
	}

	/// <summary>
	/// Set the standard error of estimate. </summary>
	/// <param name="see"> the standard error of estimate </param>
	public virtual void setStandardErrorOfEstimate(double see)
	{
		_see = see;
	}

	/// <summary>
	/// Set the standard error of estimate. </summary>
	/// <param name="seeTransformed"> transformed standard error of estimate </param>
	public virtual void setStandardErrorOfEstimateTransformed(double seeTransformed)
	{
		_seeTransformed = seeTransformed;
	}

	/// <summary>
	/// Set the standard deviation for the independent data in the N1 + N2 space. </summary>
	/// <param name="stddev_x"> Standard deviation for the independent data in the N1 + N2 space. </param>
	public virtual void setStandardDeviationX(double stddev_x)
	{
		_stddev_x = stddev_x;
	}

	/// <summary>
	/// Set the standard deviation for the independent data in the N1 space. </summary>
	/// <param name="stddev_x1"> Standard deviation for the independent data in the N1 space. </param>
	public virtual void setStandardDeviationX1(double stddev_x1)
	{
		_stddev_x1 = stddev_x1;
	}

	/// <summary>
	/// Set the standard deviation for the independent data in the N2 space. </summary>
	/// <param name="stddev_x2"> Standard deviation for the independent data in the N2 space. </param>
	public virtual void setStandardDeviationX2(double stddev_x2)
	{
		_stddev_x2 = stddev_x2;
	}

	/// <summary>
	/// Set the standard deviation for the dependent data in the N1 + N2 space. </summary>
	/// <param name="stddev_y"> Standard deviation for the dependent data in the N1 + N2 space. </param>
	public virtual void setStandardDeviationY(double stddev_y)
	{
		_stddev_y = stddev_y;
	}

	/// <summary>
	/// Set the standard deviation for the dependent data in the N1 space. </summary>
	/// <param name="stddev_y1"> Standard deviation for the dependent data in the N1 space. </param>
	public virtual void setStandardDeviationY1(double stddev_y1)
	{
		_stddev_y1 = stddev_y1;
	}

	/// <summary>
	/// Set the standard deviation for the estimated dependent data in the N1 space. </summary>
	/// <param name="stddev_y1_estimated"> Standard deviation for the dependent data in the N1 space. </param>
	public virtual void setStandardDeviationY1Estimated(double stddev_y1_estimated)
	{
		_stddev_y1_estimated = stddev_y1_estimated;
	}

	/// <summary>
	/// Set the X1 array. </summary>
	/// <param name="x1"> Data for X in N1. </param>
	public virtual void setX1(double[] x1)
	{
		_X1 = x1;
	}

	/// <summary>
	/// Set the Y1 array. </summary>
	/// <param name="y1"> Data for Y in N1. </param>
	public virtual void setY1(double[] y1)
	{
		_Y1 = y1;
	}

	/// <summary>
	/// Return a string representation of the data. </summary>
	/// <returns> a string representation of the object, which is a verbose listing
	/// of A, B, etc.  Typically this method needs to be overloaded in a more specific class. </returns>
	public override string ToString()
	{
		if (_is_analyzed)
		{
			return "Intercept A = " + _a + ", " +
			"Slope B = " + _b + ", " +
			"N1 = " + _n1 + ", " +
			"N2 = " + _n2 + ", " +
			"RMSE = " + _rmse + ", " +
			"R = " + _r;
		}
		else
		{
			return "Analysis not performed";
		}
	}

	}

}