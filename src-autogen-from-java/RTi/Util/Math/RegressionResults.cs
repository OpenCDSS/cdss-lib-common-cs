// RegressionResults - this class provides storage for regression results

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
	/// <summary>
	/// <para>
	/// This class provides storage for regression results, including intermediate statistics and final
	/// regression equation coefficients.  Data for the analysis are stored in RegressionData objects, error
	/// estimates from the equations are stored in RegressionErrors, and checks to evaluate whether the equations
	/// are valid are stored in RegressionChecks.
	/// </para>
	/// <para>
	/// The regression relationships are calculated by calling MathUtil.regress() and other MathUtil methods,
	/// often with control by higher level code such as that in the time series package.
	/// </para>
	/// <para>
	/// The regression determines the values of "a" and "b" in the relationship
	/// Y = a + b * X, where Y is a value to be determined (e.g., missing data in a
	/// dependent time series), a is the Y-intercept, b is the slope, and X is the known
	/// value (e.g., known value in independent time series).  See the documentation for the TSTool FillRegression()
	/// command for descriptions of equations.
	/// </para>
	/// <para>
	/// When analyzing for filling the X values are the independent and the Y values are the dependent.
	/// </para>
	/// <para>
	/// This class does not know whether data have been transformed or not - that is controlled by higher level code.
	/// </para>
	/// </summary>
	public class RegressionResults
	{

	/// <summary>
	/// Regression data used to perform the analysis.
	/// </summary>
	private RegressionData __data = null;

	/// <summary>
	/// Forced intercept A in the computation code.
	/// </summary>
	private double? __forcedIntercept = null;

	/// <summary>
	/// Y-intercept of best fit line.
	/// </summary>
	private double? __a = null;

	/// <summary>
	/// Slope of best fit line.
	/// </summary>
	private double? __b = null;

	/// <summary>
	/// Correlation coefficient.
	/// </summary>
	private double? __r = null;

	/// <summary>
	/// Indicates whether the analysis was performed OK.
	/// </summary>
	private bool __isAnalysisPerformedOK;

	// TODO SAM 2012-01-17 Evaluate whether data is required - may lead to memory bloat (allow null?)
	/// <summary>
	/// Constructor.  Typically the values in this class are set by the MathUtil.regress*() or similar methods.
	/// The default values are all null, indicating that an analysis was not completed. </summary>
	/// <param name="transformation"> the transformation that is applied to data before analysis </param>
	/// <param name="forcedIntercept"> the forced intercept (must be zero), or null to not force the intercept - can only
	/// be specified when no transform </param>
	public RegressionResults(RegressionData data, double? forcedIntercept, double? a, double? b, double? r)
	{
		if (forcedIntercept != null)
		{
			// TODO SAM 2012-01-15 Evaluate if this is important here, given that transformation already will have
			// happened and transform checked against intercept
			//if ( transformation != DataTransformationType.NONE ) {
			//    throw new InvalidParameterException ( "The forced intercept can only be specified with no transformation." );
			//}
			if (forcedIntercept != 0.0)
			{
				throw new System.ArgumentException("The forced intercept if specified can only be set to 0.0 (" + forcedIntercept + " was provided).");
			}
		}
		__data = data;
		__forcedIntercept = forcedIntercept;
		setA(a);
		setB(b);
		setCorrelationCoefficient(r);
		// If any of the values are not valid, set the boolean indicating that the results are not OK.
		__isAnalysisPerformedOK = true;
		if ((a == null) || a.isNaN() || (b == null) || b.isNaN() || (r == null) || r.isNaN())
		{
			__isAnalysisPerformedOK = false;
		}
	}

	/// <summary>
	/// Return data for regression equation
	/// Used so that we can clear it after it is no longer needed. </summary>
	/// <returns> data for regression equation </returns>
	public virtual RegressionData get__data()
	{
		return __data;
	}

	/// <summary>
	/// Return A (intercept) in regression equation, or null if not calculated. </summary>
	/// <returns> A (intercept) in regression equation, or null if not calculated. </returns>
	public virtual double? getA()
	{
		return __a;
	}

	/// <summary>
	/// Return B (slope) in regression equation, or null if not calculated. </summary>
	/// <returns> B (slope) in regression equation, or null if not calculated. </returns>
	public virtual double? getB()
	{
		return __b;
	}

	/// <summary>
	/// Return the coefficient of determination (r*r) for the relationship (0 to 1), or null if not calculated. </summary>
	/// <returns> The coefficient of determination (r*r) for the the relationship (-1 to 1), or null if not calculated. </returns>
	public virtual double? getCoefficientOfDetermination()
	{
		double? r = getCorrelationCoefficient();
		if (r == null)
		{
			return null;
		}
		else if (r.isNaN())
		{
			return Double.NaN;
		}
		else
		{
			return new double?(r * r);
		}
	}

	/// <summary>
	/// Return the correlation coefficient (r) for the relationship (-1 to 1), or null if not calculated. </summary>
	/// <returns> The correlation coefficient for (r) the relationship (-1 to 1), or null if not calculated. </returns>
	public virtual double? getCorrelationCoefficient()
	{
		return __r;
	}

	/// <summary>
	/// Get the forced intercept (A) for the relationship, or null if not forced. </summary>
	/// <returns> the forced intercept or null if not forced. </returns>
	public virtual double? getForcedIntercept()
	{
		return __forcedIntercept;
	}

	/// <summary>
	/// Indicate whether the analysis was performed OK. </summary>
	/// <returns> true if the analysis was performed OK, false if not. </returns>
	public virtual bool getIsAnalysisPerformedOK()
	{
		return __isAnalysisPerformedOK;
	}

	/// <summary>
	/// Set the A value (Y intercept). </summary>
	/// <param name="a"> value to save as A. </param>
	private void setA(double? a)
	{
		__a = a;
	}

	/// <summary>
	/// Set the B value (slope). </summary>
	/// <param name="b"> Value to save as B. </param>
	private void setB(double? b)
	{
		__b = b;
	}

	/// <summary>
	/// Set the correlation coefficient. </summary>
	/// <param name="r"> Correlation coefficient. </param>
	private void setCorrelationCoefficient(double? r)
	{
		__r = r;
	}

	/// <summary>
	/// Return a string representation of the data. </summary>
	/// <returns> a string representation of the object, which is a verbose listing
	/// of A, B, etc.  Typically this method needs to be overloaded in a more specific class. </returns>
	public override string ToString()
	{
			return "Intercept A = " + __a + ", " +
			"Slope B = " + __b + ", " +
			"R = " + __r;
	}

	}

}