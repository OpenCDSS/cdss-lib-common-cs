using System;
using System.Text;

// TSRegression - wrapper for the RTi.Util.Math.Regression class

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
// TSRegression - this class is a wrapper for the RTi.Util.Math.Regression
//		class.  It allows analysis methods related to regression to be
//		applied to time series data.
// ----------------------------------------------------------------------------
// History:
//
// 27 May 1998	Catherine E.
//		Nutting-Lane, RTi	Created initial version.
// 04 Nov 1998	Steven A. Malers, RTi	Alphabetize methods.  Add a toString
//					method to get a nice summary of the
//					regression statistics (for log files,
//					etc.).  Enable the regression on
//					construction (CEN wanted to do this
//					before and I have wised up to the idea).
//					Add exceptions to the get routines if
//					the regression was not successful.
// 08 Jan 1999	SAM, RTi		Update to give better messages in
//					toString when there is a data problem.
//					Track the number of points used so we
//					can see the sample size.
// 13 Apr 1999	SAM, RTi		Add finalize.
// 05 Nov 2000	SAM, RTi		Use setMax*(), etc. in the base class to
//					store max/min values that can be used
//					for graphing.  Clean up the
//					documentation some.  Update toString()
//					to have a non-data-filling version.
// 13 Apr 2001	SAM, RTi		Update to format regression results to
//					6-digits of precision (still quite a
//					lot but better than default of many
//					more digits).  Fix bug where min/max
//					values for the second time series were
//					not getting set correctly.
// 26 Apr 2001	SAM, RTi		Change the line separator in toString()
//					to "\n".
// 2001-11-06	SAM, RTi		Review javadoc.  Verify that variables
//					are set to null when no longer used.
// 2002-02-25	SAM, RTi		Add support for the MOVE2 algorithm.
//					Rename variables to be more clear what
//					the is independent (X) and what is the
//					dependent (Y) variable.  The previous
//					logic had them reversed, at least in the
//					variable names versus documentation!
//					This did not impact the filling as long
//					as the output period was set but was
//					causing problems for general use.
// 2002-03-22	SAM, RTi		Add support for the MOVE1 algorithm.
//					Add more information to the results.
//					Support monthly and log analysis for
//					MOVE1 and MOVE2.  Add an analysis
//					period.  Generalize the code to not be
//					so focused on regression.  For example,
//					change the RegressMethod property to
//					AnalysisMethod and change the regress()
//					method to analyze().  Change variable
//					names to be generic for the analysis
//					methods (e.g., use N1 for the number of
//					points in the overlapping period).
// 2002-03-31	SAM, RTi		Change in plan... the MOVE2 method needs
//					2 analysis periods, one each for the
//					independent and dependent time series.
//					The periods passed in to the method are
//					done so via properties.
// 2002-04-03	SAM, RTi		Expand the RMSE to show both the
//					transformed and untransformed data
//					coordinates.
// 2002-04-04	SAM, RTi		Complete the new analyzeOLSRegression()
//					method.
// 2002-04-05	SAM, RTi		Add more statistics MeanY and SY and
//					split the output into two tables to
//					make it easier to fit on a page of
//					output.
// 2002-05-20	SAM, RTi		Add AnalysisMonth property to support
//					seasonal comparisons.  Add getPropList()
//					to allow properties to be compared with
//					user-defined properties (e.g, in
//					TSViewPropertiesFrame).  Add
//					getAnalyzeMonth() to return boolean
//					array indicating whether months are
//					analyzed.  Add more error handling to
//					more gracefully indicate when an
//					analysis could not be completed.  The
//					isAnalyzed() method should be called
//					rather than catching an exception on
//					the constructor because a partial
//					monthly analysis may be possible.
// 2003-05-14	SAM, RTi		* Add an Intercept property, to be used
//					  with linear regression.
// 2003-06-02	SAM, RTi		Upgrade to use generic classes.
//					* Change TSDate to DateTime.
// 2003-09-26	SAM, RTi		* Tracking down a problem in the plot -
//					  display more information in toString()
//					  when isAnalyzed() is false. 
// 2005-04-14	SAM, RTi		* Add getPropList() method to get the
//					  properties that were provided at
//					  construction and which control the
//					  logic.
//					* Add getIndependentAnalysisStart(),
//					  getIndependentAnalysisEnd(),
//					  getDependentAnalysisStart(),
//					  getDependnetAnalysisEnd() to
//					  facilitate processing.
// 2005-04-18	SAM, RTi		* Overload the constructor to pass in
//					  the data arrays.  This will allow
//					  chaining of processing so that
//					  conversion from time series to arrays
//					  can be minimized.  Additional
//					  optimization may occur as performance
//					  is evaluated (e.g., if the arrays
//					  stay the same, then there may be no
//					  need to recompute basic statistics).
// 2005-05-03	Luiz Teixeira, RTi	Added the method createPredictedTS().
//					Added the method getPredictedTS().
// 2005-05-04	LT, RTi			Update the method createPredictedTS().
//					to compute the residual.
//					Added the method getResidualTS().
// 2005-05-06	SAM, RTi		Clean up some of the properties to be
//					more consistent with parameters used
//					in TSTool and other software:
//					* FillPeriodStart -> FillStart
//					* FillPeriodEnd -> FillEnd
//					* IndependentAnalysisPeriodStart ->
//					  IndependentAnalysisStart
//					* IndependentAnalysisPeriodEnd ->
//					  IndependentAnalysisEnd
//					* DependentAnalysisPeriodStart ->
//					  DependentAnalysisStart
//					* DependentAnalysisPeriodEnd ->
//					  DependentAnalysisEnd
//					Support the old versions but print a
//					warning level 2.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------
// EndHeader

namespace RTi.TS
{
	using DataTransformationType = RTi.Util.Math.DataTransformationType;
	using MathUtil = RTi.Util.Math.MathUtil;
	using NumberOfEquationsType = RTi.Util.Math.NumberOfEquationsType;
	using Regression = RTi.Util.Math.Regression;
	using RegressionType = RTi.Util.Math.RegressionType;
	using StudentTTest = RTi.Util.Math.StudentTTest;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using DateTime = RTi.Util.Time.DateTime;
	using TimeUtil = RTi.Util.Time.TimeUtil;

	/// <summary>
	/// <para>
	/// The TSRegression class performs ordinary least squares (OLS) regression,
	/// MOVE1, and MOVE2 analysis on time series and saves the results.
	/// The RTi.Util.Math.Regression base class stores some data, as
	/// appropriate (e.g., total-period, non-monthly, parameters and results).
	/// The results can then be used for filling (e.g., by the TSUtil.fillRegress() method.
	/// </para>
	/// <para>
	/// TSRegression allows provides a relatively simple interface to programmers,
	/// minimizing the need to understand time series.  However, this can lead to
	/// performance problems if many combinations of time series are analyzed.  To allow
	/// for performance optimization, a constructor is provided that accepts the
	/// data arrays.  The calling code must track when a time series is reused and
	/// should request the arrays from a previous TSRegression in order to pass to a
	/// new TSRegression.  This form of the constructor will likely be used only by advanced tools.
	/// </para>
	/// </summary>
	public class TSRegression : Regression
	{

	/// <summary>
	/// The predicted Y time series, using the regression relationship.
	/// </summary>
	private TS __yTSpredicted = null;
	/// <summary>
	/// The residual (Ypredicted  - Y) time series, using the regression relationship.
	/// </summary>
	private TS __yTSresidual = null;
	/// <summary>
	/// Analysis method (regression type).
	/// </summary>
	private RegressionType __analysisMethod = RegressionType.OLS_REGRESSION;
	/// <summary>
	/// Dependent time series, Y.
	/// </summary>
	private TS _yTS;
	/// <summary>
	/// Independent, X.
	/// </summary>
	private TS _xTS;
	/// <summary>
	/// One equation X array (independent).
	/// </summary>
	private double[] __X;
	/// <summary>
	/// Monthly equation X array (independent).
	/// </summary>
	private double[][] __X_monthly;
	/// <summary>
	/// One equation X1 array (independent).
	/// </summary>
	private double[] __X1;
	/// <summary>
	/// Monthly equation X1 array (independent).
	/// </summary>
	private double[][] __X1_monthly;
	/// <summary>
	/// One equation Y1 array (dependent).
	/// </summary>
	private double[] __Y1;
	/// <summary>
	/// Monthly equation Y1 array.
	/// </summary>
	private double[][] __Y1_monthly;
	// TODO SAM 2005-08-05 The following arrays are envisioned to be used by the
	// mixed station analysis to optimize data processing.  Basically the data
	// arrays will be extracted up front and re-used.
	/// <summary>
	/// Indicate whether the dependent data are provided in array format (instead of just the time series format).
	/// </summary>
	private bool __dependent_arrays_provided = false;
	/// <summary>
	/// Indicate whether the independent data are provided in array format (instead of just the time series format).
	/// </summary>
	private bool __independent_arrays_provided = false;
	/// <summary>
	/// Analysis period start for the dependent (Y) time series.
	/// </summary>
	private DateTime _dep_analysis_period_start;
	/// <summary>
	/// Analysis period end for the dependent (Y) time series.
	/// </summary>
	private DateTime _dep_analysis_period_end;
	/// <summary>
	/// Analysis period start for the independent (X) time series.  For OLS and MOVE2,
	/// this is the same as the dependent analysis period.  For MOVE2 it can be different.
	/// </summary>
	private DateTime _ind_analysis_period_start;
	/// <summary>
	/// Analysis period end for the independent (X) time series.  For OLS and MOVE2,
	/// this is the same as the dependent analysis period.  For MOVE2 it can be different.
	/// </summary>
	private DateTime _ind_analysis_period_end;
	/// <summary>
	/// Fill period start for the dependent (Y) - used to compute error when _filling is true.
	/// </summary>
	private DateTime _fill_period_start;
	/// <summary>
	/// Fill period end for the dependent (Y) - used to compute error when _filling is true.
	/// </summary>
	private DateTime _fill_period_end;
	/// <summary>
	/// _n1 on a monthly basis - the number of non-missing points in X and Y in the analysis period.
	/// </summary>
	private int[] _n1_monthly;
	/// <summary>
	/// _n2 on a monthly basis - the number of non-missing points in X and Y in the analysis period.
	/// </summary>
	private int[] _n2_monthly;
	/// <summary>
	/// The intercept to force, or null if not forcing.
	/// </summary>
	private double? __intercept = null;
	/// <summary>
	/// _a on a monthly basis.
	/// </summary>
	private double[] _a_monthly;
	/// <summary>
	/// _b on a monthly basis.
	/// </summary>
	private double[] _b_monthly;
	/// <summary>
	/// RMSE on a monthly basis.
	/// </summary>
	private double[] _rmseMonthly;
	/// <summary>
	/// Transformed RMS error on a monthly basis, for transformed values.
	/// </summary>
	private double[] _rmseTransformedMonthly;
	/// <summary>
	/// Standard error of estimate (SEE) on a monthly basis.
	/// </summary>
	private double[] __seeMonthly;
	/// <summary>
	/// Standard error of estimate (SEE) on a monthly basis, for transformed values.
	/// </summary>
	private double[] __seeTransformedMonthly;
	/// <summary>
	/// Standard error of prediction (SEP) on a monthly basis.
	/// </summary>
	private double[] __sepMonthly;
	/// <summary>
	/// Standard error of prediction (SEP) on a monthly basis, for transformed values.
	/// </summary>
	private double[] __sepTransformedMonthly;
	/// <summary>
	/// Standard error (SE) of slope on a monthly basis.
	/// </summary>
	private double[] __seSlopeMonthly;
	/// <summary>
	/// Standard error (SE) of slope on a monthly basis, for transformed values.
	/// </summary>
	private double[] __seSlopeTransformedMonthly;
	/// <summary>
	/// T-test score b/SE on a monthly basis.
	/// </summary>
	private double[] __testSlopeMonthly;
	/// <summary>
	/// Standard error (SE) of slope on a monthly basis, for transformed values.
	/// </summary>
	private double[] __testSlopeTransformedMonthly;
	/// <summary>
	/// Indicates whether analysis results are available for monthly analysis.
	/// </summary>
	private bool[] _is_analyzed_monthly;
	/// <summary>
	/// _correlationCoeff on a monthly basis
	/// </summary>
	private double[] _r_monthly;
	/// <summary>
	/// Confidence level for slope that is requested.
	/// </summary>
	private double? __confidenceInterval = null;
	/// <summary>
	/// Whether the confidence level for slope has been met (one equation).
	/// </summary>
	private bool __confidenceIntervalMet = true;
	/// <summary>
	/// Whether the confidence level for slope has been met (monthly equations).
	/// </summary>
	private bool[] __confidenceIntervalMetMonthly = null;
	/// <summary>
	/// Data value to substitute for the original when using a log transform and the original value is <= 0.
	/// Can be any number > 0.
	/// TODO SAM 2010-12-17 Allow NaN to throw the number away, but this changes counts, etc.
	/// </summary>
	private double? __leZeroLogValue = new double?(getDefaultLEZeroLogValue()); // Default
	/// <summary>
	/// Indicates the data transformation.
	/// </summary>
	private DataTransformationType __transformation = null;
	/// <summary>
	/// Indicates if the time series are correlated on a monthly basis.
	/// </summary>
	private NumberOfEquationsType __numberOfEquations = null;
	/// <summary>
	/// Indicates the months to analyze, 12 values for Jan - Dec, were true means the month is included in the analysis.
	/// </summary>
	private bool[] _analyze_month;
	/// <summary>
	/// List of month numbers to analyze, where each month is 1-12 (Jan - Dec), or null to analyze all months.
	/// </summary>
	private int[] _analyze_month_list;

	private double[] _X_max_monthly;
	private double[] _X1_max_monthly;
	private double[] _X2_max_monthly;
	private double[] _Y1_max_monthly;

	private double[] _X_min_monthly;
	private double[] _X1_min_monthly;
	private double[] _X2_min_monthly;
	private double[] _Y1_min_monthly;

	private double[] _X_mean_monthly;
	private double[] _Y_mean_monthly;
	private double[] _X1_mean_monthly;
	private double[] _X2_mean_monthly;
	private double[] _Y1_mean_monthly;
	private double[] _Y1_estimated_mean_monthly;

	private double[] _X_stddev_monthly;
	private double[] _Y_stddev_monthly;
	private double[] _X1_stddev_monthly;
	private double[] _X2_stddev_monthly;
	private double[] _Y1_stddev_monthly;
	private double[] _Y1_estimated_stddev_monthly;

	/// <summary>
	/// Indicates whether regression is being computed for filling (or just comparison).
	/// </summary>
	private bool _filling = false;

	/// <summary>
	/// Default constructor. </summary>
	/// @deprecated Use a version that specifies time series. 
	public TSRegression()
	{
	}

	/// <summary>
	/// Perform a regression using the specified time series.  The results can be used
	/// to fill data using the relationship:
	/// <pre>
	///     Y = a + bX;
	/// </pre> </summary>
	/// <exception cref="Exception"> if an error occurs performing the analysis.  However, it
	/// is best to check the isAnalyzed() value to determine whether results can be used. </exception>
	/// <param name="independentTS"> The independent time series (X). </param>
	/// <param name="dependentTS"> The dependent time series (Y). </param>
	/// <param name="analyzeForFilling"> Set to "true" if analysis is being done as part of a data filling process,
	/// in which case the FillStart and FillEnd can be specified to limit the fill period.
	/// The RMSE is computed as the difference between Y and Y-estimated where Y is known in the original sample.
	/// The default ("false") is used to compare time series (e.g., for calibration),
	/// in which case the RMSE is computed using the difference between X and Y. </param>
	/// <param name="analysisMethod"> the analysis method to be used to determine the regression relationship. </param>
	/// <param name="intercept"> If specified, indicate the intercept (A-value in best fit equation) that
	/// should be forced when analyzing the data.  This is currently only implemented
	/// for Linear (no) transformation and can currently only have a value of 0.
	/// If specified as null, no intercept is used.  This feature is typically only
	/// used when analyzing data for filling. </param>
	/// <param name="numberOfEquations"> Set to "OneEquation" to calculate one relationship.  Set to
	/// "MonthlyEquations" if a monthly analysis should be done (in which case 12 relationships will be
	/// determined).  In the future support for seasonal equations may be added. </param>
	/// <param name="analysisMonths"> If one equation is being used, indicate the months that are to be analyzed.
	/// If monthly equations are being used, indicate the one month to analyze. </param>
	/// <param name="transformation"> Set to "Log" if a log10 regression should be done, or "None" if no transformation. </param>
	/// <param name="leZeroLogValue"> if the log transform is used this is the value used to replace values <= zero (if null
	/// use the default given by TSRegression.getDefaultLEZeroLogValue() and NaN will cause the values to be discarded. </param>
	/// <param name="confidenceInterval"> the confidence interval % to impose - relationships that do not pass the
	/// corresponding level T-test (not a strong relationship) are not used.  If null, the confidence interval
	/// is not examined. </param>
	/// <param name="dependentAnalysisStart"> Date/time as a string indicating analysis period start for the dependent
	/// time series.  This can be specified for all analysis methods.  For OLS and
	/// MOVE1, this period will also be used for the independent time series.  If null, the full period is analyzed. </param>
	/// <param name="dependentAnalysisEnd"> Date/time as a string indicating analysis period end for the dependent time
	/// series.  This can be specified for all analysis methods.  For OLS and MOVE1,
	/// this period will also be used for the independent time series.  If null, the full period is analyzed. </param>
	/// <param name="independentAnalysisStart"> Date/time as a string indicating analysis period start for the independent
	/// time series.  This can be specified for the MOVE2 analysis method.  If null, the full period is analyzed. </param>
	/// <param name="independentAnalysisEnd"> Date/time as a string indicating analysis period end for the independent
	/// time series.  This can be specified for the MOVE2 analysis method.  If null, the full period is analyzed. </param>
	/// <param name="Date">/time as a string indicating filling period start.  Specify when analyzeForFilling is true.
	/// If null, the full period is filled. </param>
	/// <param name="fillEnd"> Date/time as a string indicating filling period end.  Specify when analyzeForFilling is true.
	/// If null, the full period is filled. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TSRegression(TS independentTS, TS dependentTS, boolean analyzeForFilling, RTi.Util.Math.RegressionType analysisMethod, System.Nullable<double> intercept, RTi.Util.Math.NumberOfEquationsType numberOfEquations, int [] analysisMonths, RTi.Util.Math.DataTransformationType transformation, System.Nullable<double> leZeroLogValue, System.Nullable<double> confidenceInterval, RTi.Util.Time.DateTime dependentAnalysisStart, RTi.Util.Time.DateTime dependentAnalysisEnd, RTi.Util.Time.DateTime independentAnalysisStart, RTi.Util.Time.DateTime independentAnalysisEnd, RTi.Util.Time.DateTime fillStart, RTi.Util.Time.DateTime fillEnd) throws Exception
	public TSRegression(TS independentTS, TS dependentTS, bool analyzeForFilling, RegressionType analysisMethod, double? intercept, NumberOfEquationsType numberOfEquations, int[] analysisMonths, DataTransformationType transformation, double? leZeroLogValue, double? confidenceInterval, DateTime dependentAnalysisStart, DateTime dependentAnalysisEnd, DateTime independentAnalysisStart, DateTime independentAnalysisEnd, DateTime fillStart, DateTime fillEnd) : base()
	{
		initialize(independentTS, dependentTS, analyzeForFilling, analysisMethod, intercept, numberOfEquations, analysisMonths, transformation, leZeroLogValue, confidenceInterval, dependentAnalysisStart, dependentAnalysisEnd, independentAnalysisStart, independentAnalysisEnd, fillStart, fillEnd);
		analyze();
	}

	// FIXME SAM 2010-06-10 Why are all the parameters needed?  Can't the parameters just be extracted
	// from the existing object?  Presumably the parameters that are passed in are consistent with the
	// original computation and its really the internal information (A, B, etc.) that are being reused
	// and not recomputed.
	/// <summary>
	/// Construct a TSRegression object and perform the analysis.  See the overloaded
	/// version for a description of the analysis.
	/// This version should be used when an initial analysis has occurred (e.g., with
	/// a previous TSRegression) and data arrays can be re-used for a subsequent
	/// analysis.  For example, the same dependent and independent time series may be
	/// used, but with different analysis method or transformation.  In this case, there
	/// is no reason to recreate the data arrays from the time series.  An example of use is as follows:
	/// <pre>
	/// // First analysis...
	/// TSRegression reg1 = new TSRegression ( ts_ind, ts_dep, props );
	/// // Change the properties...
	/// props.set ( ... );
	/// // Analyze again, using the same data arrays as in the previous analysis...
	/// TSRegression reg2 = new TSRegression ( ts_ind, ts_dep, props,
	/// reg1.getX(), reg1.getXMonthly(), reg1.getX1(), reg1.getX1Monthly(), reg1.getY1(), reg1.getY1Monthly() );
	/// </pre>
	/// In this way, the data arrays can be used repeatedly, thus improving performance.
	/// Note that the "1" and "2" arrays are computed based on the overlap between the
	/// independent and dependent time series.  Therefore, changing the combination will
	/// require that the arrays are recreated (use the first version of the constructor). </summary>
	/// <exception cref="Exception"> if an error occurs performing the analysis.  However, it
	/// is best to check the isAnalyzed() value to determine whether results can be used. </exception>
	/// <param name="independentTS"> The independent time series (X). </param>
	/// <param name="dependentTS"> The dependent time series (Y). </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TSRegression(TS independentTS, TS dependentTS, double[] X, double[][] XMonthly, boolean analyzeForFilling, RTi.Util.Math.RegressionType analysisMethod, System.Nullable<double> intercept, RTi.Util.Math.NumberOfEquationsType numberOfEquations, int [] analysisMonths, RTi.Util.Math.DataTransformationType transformation, System.Nullable<double> leZeroLogValue, System.Nullable<double> confidenceInterval, RTi.Util.Time.DateTime dependentAnalysisStart, RTi.Util.Time.DateTime dependentAnalysisEnd, RTi.Util.Time.DateTime independentAnalysisStart, RTi.Util.Time.DateTime independentAnalysisEnd, RTi.Util.Time.DateTime fillStart, RTi.Util.Time.DateTime fillEnd) throws Exception
	public TSRegression(TS independentTS, TS dependentTS, double[] X, double[][] XMonthly, bool analyzeForFilling, RegressionType analysisMethod, double? intercept, NumberOfEquationsType numberOfEquations, int[] analysisMonths, DataTransformationType transformation, double? leZeroLogValue, double? confidenceInterval, DateTime dependentAnalysisStart, DateTime dependentAnalysisEnd, DateTime independentAnalysisStart, DateTime independentAnalysisEnd, DateTime fillStart, DateTime fillEnd) : base()
	{
		initialize(independentTS, dependentTS, analyzeForFilling, analysisMethod, intercept, numberOfEquations, analysisMonths, transformation, leZeroLogValue, confidenceInterval, dependentAnalysisStart, dependentAnalysisEnd, independentAnalysisStart, independentAnalysisEnd, fillStart, fillEnd);
		analyze();
	}

	/// <summary>
	/// This routine is the most general analysis routine and should be called in all
	/// cases.  It evaluates the property list set in the constructor and calls the
	/// appropriate secondary routines.  See the constructor documentation for possible property values. </summary>
	/// <exception cref="RTi.TS.Exception"> if there is a problem performing regression. </exception>
	private void analyze()
	{
		if (__analysisMethod == RegressionType.MOVE2)
		{
			analyzeMOVE2();
		}
		else if (__analysisMethod == RegressionType.OLS_REGRESSION)
		{
			analyzeOLSRegression();
		}
	}

	/// <summary>
	/// Analyze two time series using the MOVE2 method.
	/// The data in the dependent time series can then be filled using
	/// <pre>
	/// Y = a + bX
	/// </pre>
	/// </summary>
	private void analyzeMOVE2()
	{
		string routine = "TSRegression.analyzeMOVE2";

		int num_equations = 1;
		if (__numberOfEquations == NumberOfEquationsType.MONTHLY_EQUATIONS)
		{
			num_equations = 12;
		}

		// The data value to use when doing a log transform and the original value is <= 0
		double leZeroSubstituteDataValue = getLEZeroLogValue().Value;
		// Calculate the log here for use in transformed data
		double leZeroSubstituteDataValueLog = Math.Log10(leZeroSubstituteDataValue);

		// Loop through the equations.

		double[] x1Array = null;
		double[] y1Array = null;
		double[] xArray = null;
		int n1 = 0;
		int n2 = 0;
		int ind_interval_base = _xTS.getDataIntervalBase();
		int ind_interval_mult = _xTS.getDataIntervalMult();
		DateTime date = null; // For iteration
		double data_value = 0.0;
		double rmseTotal = 0.0, rmseTransformedTotal = 0.0;
		int n1_total = 0;
		for (int ieq = 1; ieq <= num_equations; ieq++)
		{
			try
			{
			// Get the data array for the dependent analysis period (N1).
			if ((num_equations == 1) && ((_analyze_month_list == null) || (_analyze_month_list.Length == 0)))
			{
				// Input data is for all the months to be analyzed...
				x1Array = TSUtil.toArray(_xTS, _dep_analysis_period_start, _dep_analysis_period_end);
				y1Array = TSUtil.toArray(_yTS, _dep_analysis_period_start, _dep_analysis_period_end);
				xArray = TSUtil.toArray(_xTS, _ind_analysis_period_start, _ind_analysis_period_end);
			}
			else if ((num_equations == 1) && (_analyze_month_list != null))
			{
				// Only get the months to analyze...
				x1Array = TSUtil.toArray(_xTS, _ind_analysis_period_start, _ind_analysis_period_end, _analyze_month_list);
				y1Array = TSUtil.toArray(_yTS, _dep_analysis_period_start, _dep_analysis_period_end, _analyze_month_list);
				xArray = TSUtil.toArray(_xTS, _xTS.getDate1(), _xTS.getDate2(), _analyze_month_list);
			}
			else
			{
				if (!_analyze_month[ieq - 1])
				{
					continue;
				}
				// Get the input data by month...
				x1Array = TSUtil.toArrayByMonth(_xTS, _dep_analysis_period_start, _dep_analysis_period_end, ieq);
				y1Array = TSUtil.toArrayByMonth(_yTS, _dep_analysis_period_start, _dep_analysis_period_end, ieq);
				xArray = TSUtil.toArrayByMonth(_xTS, _ind_analysis_period_start, _ind_analysis_period_end, ieq);
			}

			// Initially indicate that the analysis is not complete...

			if (num_equations == 1)
			{
				isAnalyzed(false);
			}
			else
			{
				isAnalyzed(ieq, false);
			}

			if ((x1Array == null) || (y1Array == null))
			{
				// Not enough data...
				if (num_equations == 1)
				{
					throw new TSException("No data.  Not performing analysis.");
				}
				else
				{
					Message.printWarning(3, routine, "No data.  Not performing analysis for month " + ieq + ".");
					continue;
				}
			}

			// The array lengths should be the same, even if padded with missing data...

			if ((x1Array.Length != y1Array.Length))
			{
				if (num_equations == 1)
				{
					throw new TSException("Data set lengths are not equal.");
				}
				else
				{
					Message.printWarning(3, routine, "Data set lengths are not equal.  Not performing analysis for month " + ieq);
					continue;
				}
			}

			// First get the data arrays for the N1 space, which is the
			// overlapping data in the dependent analysis period...

			// First get the counts where X and Y are non-missing...

			n1 = 0;
			for (int i = 0; i < x1Array.Length; i++)
			{
				if (!_xTS.isDataMissing(x1Array[i]) && !_yTS.isDataMissing(y1Array[i]))
				{
					// both are not missing...
					++n1;
				}
			}

			if (n1 == 0)
			{
				if (num_equations == 1)
				{
					string message = "The number of overlapping points is 0.  Cannot perform MOVE2 analysis.";
					Message.printWarning(3, routine, message);
					throw new Exception(message);
				}
				else
				{
					string message = "The number of overlapping points is 0 for month " + ieq +
					".  Cannot perform MOVE2 analysis.";
					Message.printWarning(3, routine, message);
					continue;
				}
			}

			// Set the number of overlapping values because this is used
			// in other code (e.g., plotting) to allow a single point plot...

			if (num_equations == 1)
			{
				setN1(n1);
			}
			else
			{
				setN1(ieq, n1);
			}

			// Now transfer the values to temporary arrays for the
			// independent variable for N1 and N2 values.  If doing a log
			// transformation, do it here and then treat all other
			// operations as if no transformation...

			double[] X1_data = new double[n1]; // non-missing X data where non-missing Y for n1
			double[] Y1_data = new double[n1]; // non-missing Y data where non-missing X for n1
			double[] orig_X1_data = null;
			double[] orig_Y1_data = null;
			if (__transformation == DataTransformationType.LOG)
			{
				orig_X1_data = new double[n1];
				orig_Y1_data = new double[n1];
			}

			n1 = 0; // Can reuse
			for (int i = 0; i < x1Array.Length; i++)
			{
				if (!_xTS.isDataMissing(x1Array[i]) && !_yTS.isDataMissing(y1Array[i]))
				{
					// both are not missing...
					if (__transformation == DataTransformationType.LOG)
					{
						if (x1Array[i] <= 0.0)
						{
							// Substitute value
							X1_data[n1] = leZeroSubstituteDataValueLog;
						}
						else
						{
							X1_data[n1] = Math.Log10(x1Array[i]);
						}
						if (y1Array[i] <= 0.0)
						{
							// Substitute value
							Y1_data[n1] = leZeroSubstituteDataValueLog;
						}
						else
						{
							Y1_data[n1] = Math.Log10(y1Array[i]);
						}
						orig_X1_data[n1] = x1Array[i];
						orig_Y1_data[n1] = y1Array[i];
					}
					else
					{
						X1_data[n1] = x1Array[i];
						Y1_data[n1] = y1Array[i];
					}
					++n1;
				}
			}

			// Now evaluate the independent time series analysis period...

			// First loop through and determine in this period where there
			// is a non-missing X value and a missing Y value.  The period
			// outside the dependent analysis period is considered to have missing Y data.

			// Because N2 values are not used to evaluate RMSE, only need
			// one set of data (the original or transformed)...

			n2 = 0;
			for (date = new DateTime(_ind_analysis_period_start); date.lessThanOrEqualTo(_ind_analysis_period_end); date.addInterval(ind_interval_base, ind_interval_mult))
			{
				if ((num_equations != 1) && (date.getMonth() != ieq))
				{
					continue;
				}
				if (!_xTS.isDataMissing(_xTS.getDataValue(date)))
				{
					if (date.lessThan(_dep_analysis_period_start) || date.greaterThan(_dep_analysis_period_end) || _yTS.isDataMissing(_yTS.getDataValue(date)))
					{
						// OK to increment...
						++n2;
					}
				}
			}

			if (n2 == 0)
			{
				if (num_equations == 1)
				{
					string message = "The number of non-overlapping points is 0.  Cannot perform MOVE2.";
					Message.printWarning(3, routine, message);
					throw new Exception(message);
				}
				else
				{
					string message = "The number of non-overlapping points for month " + ieq + " is 0.  Cannot perform MOVE2.";
					Message.printWarning(3, routine, message);
					throw new Exception(message);
				}
			}

			double[] X2_data = new double[n2]; // non-missing X data in n2

			// Now loop through again and transfer the data...

			n2 = 0;
			for (date = new DateTime(_ind_analysis_period_start); date.lessThanOrEqualTo(_ind_analysis_period_end); date.addInterval(ind_interval_base,ind_interval_mult))
			{
				if ((num_equations != 1) && (date.getMonth() != ieq))
				{
					continue;
				}
				data_value = _xTS.getDataValue(date);
				if (!_xTS.isDataMissing(data_value))
				{
					if (date.lessThan(_dep_analysis_period_start) || date.greaterThan(_dep_analysis_period_end) || _yTS.isDataMissing(_yTS.getDataValue(date)))
					{
						if (__transformation == DataTransformationType.LOG)
						{
							if (data_value <= 0.0)
							{
								// Substitute value
								X2_data[n2] = leZeroSubstituteDataValueLog;
							}
							else
							{
								X2_data[n2] = Math.Log10(data_value);
							}
						}
						else
						{
							X2_data[n2] = data_value;
						}
						++n2;
					}
				}
			}

			// Compute the mean values...

			double X1_mean = MathUtil.mean(X1_data);
			double Y1_mean = MathUtil.mean(Y1_data);
			double X2_mean = MathUtil.mean(X2_data);

			// Compute the standard deviations...

			double X1_stddev = MathUtil.standardDeviation(X1_data);
			double X2_stddev = MathUtil.standardDeviation(X2_data);
			double Y1_stddev = MathUtil.standardDeviation(Y1_data);

			// Do the regression to get the correlation coefficient.
			// Missing data should not have come through so don't check again...

			Regression rd = null;
			try
			{
				rd = MathUtil.regress(X1_data, Y1_data, false, _xTS.getMissing(), _yTS.getMissing(), null);
			}
			catch (Exception)
			{
				if (num_equations == 1)
				{
					throw new TSException("Error performing regression on N1.");
				}
				else
				{
					Message.printWarning(3, routine, "Error performing regression on N1 for month " + ieq);
				}
			}
			if (rd == null)
			{
				if (num_equations == 1)
				{
					throw new TSException("Error performing regression on N1.");
				}
				else
				{
					Message.printWarning(3, routine, "Error performing regression on N1 for month " + ieq);
				}
			}

			double r = rd.getCorrelationCoefficient();
			double b = r * Y1_stddev / X1_stddev;

			double Sy_sq = (1.0 / ((double)n1 + (double)n2 - 1.0)) * (((double)n1 - 1.0) * Y1_stddev * Y1_stddev + ((double)n2 - 1.0) * b * b * X2_stddev * X2_stddev + (double)n2 * ((double)n1 - 4) * ((double)n1 - 1) * (1 - r * r) * Y1_stddev * Y1_stddev / (((double)n1 - 3) * ((double)n1 - 2)) + (double)n1 * (double)n2 / ((double)n1 + (double)n2) * b * b * (X2_mean - X1_mean) * (X2_mean - X1_mean));

			double Ybar = Y1_mean + (double)n2 * b * (X2_mean - X1_mean) / ((double)n1 + (double)n2);

			if (__transformation == DataTransformationType.LOG)
			{
				// Update the X array to logs...
				for (int j = 0; j < xArray.Length; j++)
				{
					if (_xTS.isDataMissing(xArray[j]))
					{
						continue;
					}
					if (xArray[j] <= 0.0)
					{
						// Substitute value
						xArray[j] = leZeroSubstituteDataValueLog;
					}
					else
					{
						xArray[j] = Math.Log10(xArray[j]);
					}
				}
			}
			double X_mean = MathUtil.mean(xArray.Length, xArray, _xTS.getMissing());
			double X_stddev = MathUtil.standardDeviation(xArray.Length, xArray, _xTS.getMissing());

			b = Math.Sqrt(Sy_sq) / X_stddev;
			double a = Ybar - b * X_mean;

			double rmse = 0.0, rmseTransformed = 0.0;
			double[] Y1_estimated = null; // Estimated Y1 if filling data.
			if (_filling)
			{
				// Now if filling, estimate Y1 using A and B and compute
				// the RMSE from Y1 - Y.  Just loop through the X1
				// because we know these points originally lined up with Y1...

				Y1_estimated = new double[n1];
				double ytemp1, ytemp2;
				for (int i = 0; i < n1; i++)
				{
					if (__transformation == DataTransformationType.LOG)
					{
						// Estimate Y in log10 space.  X1 was transformed to log above...
						Y1_estimated[i] = a + X1_data[i] * b;
						rmseTransformed += ((Y1_estimated[i] - Y1_data[i]) * (Y1_estimated[i] - Y1_data[i]));
						rmseTransformedTotal += ((Y1_estimated[i] - Y1_data[i]) * (Y1_estimated[i] - Y1_data[i]));
						// Always do untransformed data.  To do so, un-transform the estimated
						// log10 Y value and compare to the original untransformed Y value...
						ytemp1 = Math.Pow(10.0, Y1_estimated[i]);
						ytemp2 = orig_Y1_data[i];
						rmse += ((ytemp1 - ytemp2) * (ytemp1 - ytemp2));
						rmseTotal += ((ytemp1 - ytemp2) * (ytemp1 - ytemp2));
					}
					else
					{
						Y1_estimated[i] = a + X1_data[i] * b;
						rmse += ((Y1_estimated[i] - Y1_data[i]) * (Y1_estimated[i] - Y1_data[i]));

						rmseTotal += ((Y1_estimated[i] - Y1_data[i]) * (Y1_estimated[i] - Y1_data[i]));
					}
				}
			}
			else
			{
				// Just use available data...
				double ytemp, xtemp;
				for (int i = 0; i < n1; i++)
				{
					if (__transformation == DataTransformationType.LOG)
					{
						rmseTransformed += ((Y1_data[i] - X1_data[i]) * (Y1_data[i] - X1_data[i]));
						rmseTransformedTotal += ((Y1_data[i] - X1_data[i]) * (Y1_data[i] - X1_data[i]));
						// Always do untransformed data...
						ytemp = Math.Pow(10.0, Y1_data[i]);
						xtemp = Math.Pow(10.0, X1_data[i]);
						rmse += ((ytemp - xtemp) * (ytemp - xtemp));
						rmseTotal += ((ytemp - xtemp) * (ytemp - xtemp));
					}
					else
					{
						rmse += ((Y1_data[i] - X1_data[i]) * (Y1_data[i] - X1_data[i]));
						rmseTotal += ((Y1_data[i] - X1_data[i]) * (Y1_data[i] - X1_data[i]));
					}
				}
			}
			if (__transformation == DataTransformationType.LOG)
			{
				rmseTransformed = Math.Sqrt(rmseTransformed / (double)n1);
			}
			// Always do untransformed data...
			rmse = Math.Sqrt(rmse / (double)n1);
			n1_total += n1; // Need below to compute total RMSE

			// Transfer results from local object to the base class...

			if (num_equations == 1)
			{
				isAnalyzed(true);
				setA(a);
				setB(b);
				setCorrelationCoefficient(r);
				setN1(n1);
				setN2(n2);
				if (__transformation == DataTransformationType.LOG)
				{
					setRMSETransformed(rmseTransformed);
					setStandardErrorOfEstimateTransformed(calculateStandardErrorOfEstimateFromRMSE(rmseTransformed,n1));
				}
				setRMSE(rmse);
				setStandardErrorOfEstimate(calculateStandardErrorOfEstimateFromRMSE(rmse,n1));
				setMaxX1(rd.getMaxX1());
				setMinX1(rd.getMinX1());
				setMaxY1(rd.getMaxY1());
				setMinY1(rd.getMinY1());

				setMeanX(X_mean);
				setStandardDeviationX(X_stddev);
				setMeanY(Ybar);
				setStandardDeviationY(Math.Sqrt(Sy_sq));
				setMeanX1(X1_mean);
				setStandardDeviationX1(X1_stddev);
				setMeanX2(X2_mean);
				setStandardDeviationX2(X2_stddev);
				setMeanY1(Y1_mean);
				setStandardDeviationY1(Y1_stddev);
				if (_filling)
				{
					setMeanY1Estimated(MathUtil.mean(Y1_estimated));
					setStandardDeviationY1Estimated(MathUtil.standardDeviation(Y1_estimated));
				}

				setLagIntervals(rd.getLagIntervals());
			}
			else
			{
				isAnalyzed(ieq, true);
				setA(ieq, a);
				setB(ieq, b);
				setCorrelationCoefficient(ieq, r);
				setN1(ieq, n1);
				setN2(ieq, n2);
				if (__transformation == DataTransformationType.LOG)
				{
					setRMSETransformed(ieq, rmseTransformed);
					setStandardErrorOfEstimateTransformed(ieq,calculateStandardErrorOfEstimateFromRMSE(rmseTransformed,n1));
				}
				setRMSE(ieq, rmse);
				setStandardErrorOfEstimate(ieq, calculateStandardErrorOfEstimateFromRMSE(rmse,n1));
				if (ieq == 12)
				{
					// Save the total in the non-monthly data values...
					rmseTotal = Math.Sqrt(rmseTotal / (double)n1_total);
					rmseTransformedTotal = Math.Sqrt(rmseTransformedTotal / (double)n1_total);
					setRMSE(rmseTotal);
					setStandardErrorOfEstimate(calculateStandardErrorOfEstimateFromRMSE(rmseTotal,n1_total));
					if (__transformation == DataTransformationType.LOG)
					{
						setRMSETransformed(rmseTransformedTotal);
						setStandardErrorOfEstimateTransformed(calculateStandardErrorOfEstimateFromRMSE(rmseTransformedTotal,n1_total));
					}
				}
				setMaxX1(ieq, rd.getMaxX1());
				setMinX1(ieq, rd.getMinX1());
				setMaxY1(ieq, rd.getMaxY1());
				setMinY1(ieq, rd.getMinY1());

				setMeanX(ieq, X_mean);
				setStandardDeviationX(ieq, X_stddev);
				setMeanY(ieq, Ybar);
				setStandardDeviationY(ieq, Math.Sqrt(Sy_sq));
				setMeanX1(ieq, X1_mean);
				setStandardDeviationX1(ieq, X1_stddev);
				setMeanX2(ieq, X2_mean);
				setStandardDeviationX2(ieq, X2_stddev);
				setMeanY1(ieq, Y1_mean);
				setStandardDeviationY1(ieq, Y1_stddev);
				if (_filling)
				{
					setMeanY1Estimated(ieq, MathUtil.mean(Y1_estimated));
					setStandardDeviationY1Estimated(ieq, MathUtil.standardDeviation(Y1_estimated));
				}
			}

			//Message.printStatus ( 1, "", "sy = " + Math.sqrt(_MOVE2_Sy2));

			X1_data = null;
			X2_data = null;
			Y1_data = null;
			Y1_estimated = null;
			rd = null;
			}
		catch (Exception)
		{
			// Error doing the analysis.
			if (num_equations == 1)
			{
				isAnalyzed(false);
			}
			else
			{
				isAnalyzed(ieq, false);
			}
		}
		}

		x1Array = null;
		y1Array = null;
		xArray = null;
	}

	/// <summary>
	/// Perform OLS regression analysis using either a single or 12 monthly
	/// relationships.  Data are also optionally transformed using Log10.
	/// </summary>
	private void analyzeOLSRegression()
	{
		string routine = "TSRegression.analyzeOLSRegression";

		Regression rd = null;
		int num_equations = 1;
		if (__numberOfEquations == NumberOfEquationsType.MONTHLY_EQUATIONS)
		{
			num_equations = 12;
		}

		// The data value to use when doing a log transform and the original value is <= 0
		double leZeroSubstituteDataValue = getLEZeroLogValue().Value;
		// Calculate the log here for use in transformed data
		double leZeroSubstituteDataValueLog = Math.Log10(leZeroSubstituteDataValue);

		double[] x1Array = null;
		double[] xArray = null;
		double[] y1Array = null;
		int n1 = 0; // Number of points where X and Y are non-missing
		int n2 = 0; // Number of points where X is not missing and Y is missing
		bool confidenceIntervalMet = true; // Whether requested confidence level has been met for slope
		int ind_interval_base = _xTS.getDataIntervalBase();
		int ind_interval_mult = _xTS.getDataIntervalMult();
		DateTime date = null;
		double data_value = 0.0;
		for (int ieq = 1; ieq <= 12; ieq++)
		{
			try
			{
			// Get the data array for the dependent analysis period (N1).
			// For OLS regression the independent and dependent analysis period are the same.
			if ((num_equations == 1) && ((_analyze_month_list == null) || (_analyze_month_list.Length == 0)))
			{
				// Get all of the data because one equation is used and it is not a specific month.
				if (__independent_arrays_provided)
				{
					x1Array = __X1;
					xArray = __X;
				}
				else
				{
					x1Array = TSUtil.toArray(_xTS, _ind_analysis_period_start, _ind_analysis_period_end);
					xArray = TSUtil.toArray(_xTS, _xTS.getDate1(), _xTS.getDate2());
				}
				if (__dependent_arrays_provided)
				{
					y1Array = __Y1;
				}
				else
				{
					y1Array = TSUtil.toArray(_yTS, _dep_analysis_period_start, _dep_analysis_period_end);
				}
			}
			else if ((num_equations == 1) && (_analyze_month_list != null))
			{
				// This analyzes a "season" of data consisting of one or more months.
				if (__independent_arrays_provided)
				{
					x1Array = __X1;
					xArray = __X;
				}
				else
				{
					x1Array = TSUtil.toArray(_xTS, _ind_analysis_period_start, _ind_analysis_period_end, _analyze_month_list);
					xArray = TSUtil.toArray(_xTS, _xTS.getDate1(), _xTS.getDate2(), _analyze_month_list);
				}
				if (__independent_arrays_provided)
				{
					y1Array = __Y1;
				}
				else
				{
					y1Array = TSUtil.toArray(_yTS, _dep_analysis_period_start, _dep_analysis_period_end, _analyze_month_list);
				}
			}
			else
			{
				// Analyze data for the month indicated by the loop index...
				if (!_analyze_month[ieq - 1])
				{
					continue;
				}
				if (__independent_arrays_provided)
				{
					x1Array = __X1_monthly[ieq - 1];
					xArray = __X_monthly[ieq - 1];
				}
				else
				{
					x1Array = TSUtil.toArrayByMonth(_xTS, _ind_analysis_period_start, _ind_analysis_period_end, ieq);
					xArray = TSUtil.toArrayByMonth(_xTS, _xTS.getDate1(), _xTS.getDate2(), ieq);
				}
				if (__independent_arrays_provided)
				{
					y1Array = __Y1_monthly[ieq - 1];
				}
				else
				{
					y1Array = TSUtil.toArrayByMonth(_yTS, _dep_analysis_period_start, _dep_analysis_period_end, ieq);
				}
			}

			// Initially indicate that the analysis is not complete...

			if (num_equations == 1)
			{
				isAnalyzed(false);
			}
			else
			{
				isAnalyzed(ieq, false);
			}

			if ((x1Array == null) || (y1Array == null))
			{
				// Not enough data...
				if (num_equations == 1)
				{
					Message.printWarning(10, routine, "No data.  Not performing analysis.");
					throw new TSException("No data.  Not performing analysis.");
				}
				else
				{
					Message.printWarning(10, routine, "No data.  Not performing analysis for month " + ieq + ".");
				}
				continue;
			}

			// The array lengths should be the same, even if padded with missing data...

			if (x1Array.Length != y1Array.Length)
			{
				// Not enough data...
				if (num_equations == 1)
				{
					Message.printWarning(10, routine, "Data set lengths are not equal.");
					throw new TSException("Data set lengths are not equal.");
				}
				else
				{
					Message.printWarning(10, routine, "Data set lengths are not the same.  " + "Not performing analysis for month " + ieq + ".");
					continue;
				}
			}

			// First get the data arrays for the N1 space, which is the
			// overlapping data in the dependent analysis period...

			// First get the counts where X and Y are non-missing...

			n1 = 0;
			for (int i = 0; i < x1Array.Length; i++)
			{
				if (!_xTS.isDataMissing(x1Array[i]) && !_yTS.isDataMissing(y1Array[i]))
				{
					// both are not missing...
					++n1;
				}
			}

			if (n1 == 0)
			{
				if (num_equations == 1)
				{
					string message = "The number of overlapping points is 0.  Cannot perform OLS regression.";
					Message.printWarning(3, routine, message);
					throw new TSException(message);
				}
				else
				{
					string message = "The number of overlapping points is 0 for month " + ieq +
					".  Cannot perform OLS regression.";
					Message.printWarning(3, routine, message);
					continue;
				}
			}

			// Set the number of overlapping values because this is used
			// in other code (e.g., plotting) to allow a single point plot...

			if (num_equations == 1)
			{
				setN1(n1);
			}
			else
			{
				setN1(ieq, n1);
			}

			// Now transfer the values to temporary arrays for the
			// independent variable for N1 and N2 values.  If doing a log
			// transformation, do it here and then treat all other operations as if no transformation...

			double[] X1_data = new double[n1]; // non-missing X data where non-missing Y for n1
			double[] Y1_data = new double[n1]; // non-missing Y data where non-missing X for n1
			double[] orig_X1_data = null;
			double[] orig_Y1_data = null;
			if (__transformation == DataTransformationType.LOG)
			{
				orig_X1_data = new double[n1];
				orig_Y1_data = new double[n1];
			}

			n1 = 0; // Can reuse
			for (int i = 0; i < x1Array.Length; i++)
			{
				if (!_xTS.isDataMissing(x1Array[i]) && !_yTS.isDataMissing(y1Array[i]))
				{
					// both are not missing...
					if (__transformation == DataTransformationType.LOG)
					{
						if (x1Array[i] <= 0.0)
						{
							// Substitute value
							X1_data[n1] = leZeroSubstituteDataValueLog;
						}
						else
						{
							X1_data[n1] = Math.Log10(x1Array[i]);
						}
						if (y1Array[i] <= 0.0)
						{
							// Substitute value
							Y1_data[n1] = leZeroSubstituteDataValueLog;
						}
						else
						{
							Y1_data[n1] = Math.Log10(y1Array[i]);
						}
						orig_X1_data[n1] = x1Array[i];
						orig_Y1_data[n1] = y1Array[i];
					}
					else
					{
						X1_data[n1] = x1Array[i];
						Y1_data[n1] = y1Array[i];
					}
					++n1;
				}
			}

			// Now evaluate the independent time series analysis period...

			// First loop through and determine in this period where there
			// is a non-missing X value and a missing Y value.  The period
			// outside the dependent analysis period is considered to have missing Y data.

			// Because N2 values are not used to evaluate RMSE, only need
			// one set of data (the original or transformed)...

			n2 = 0;
			for (date = new DateTime(_ind_analysis_period_start); date.lessThanOrEqualTo(_ind_analysis_period_end); date.addInterval(ind_interval_base,ind_interval_mult))
			{
				if ((num_equations != 1) && (date.getMonth() != ieq))
				{
					continue;
				}
				if (!_xTS.isDataMissing(_xTS.getDataValue(date)))
				{
					if (date.lessThan(_dep_analysis_period_start) || date.greaterThan(_dep_analysis_period_end) || _yTS.isDataMissing(_yTS.getDataValue(date)))
					{
						// OK to increment...
						++n2;
					}
				}
			}

			// N2 of 0 is not fatal.

			double[] X2_data = null;
			if (n2 > 0)
			{
				X2_data = new double[n2]; // non-missing X data in n2

				// Now loop through again and transfer the data...

				n2 = 0;
				for (date = new DateTime(_ind_analysis_period_start); date.lessThanOrEqualTo(_ind_analysis_period_end); date.addInterval(ind_interval_base, ind_interval_mult))
				{
					if ((num_equations != 1) && (date.getMonth() != ieq))
					{
						continue;
					}
					data_value = _xTS.getDataValue(date);
					if (!_xTS.isDataMissing(data_value))
					{
						if (date.lessThan(_dep_analysis_period_start) || date.greaterThan(_dep_analysis_period_end) || _yTS.isDataMissing(_yTS.getDataValue(date)))
						{
							if (__transformation == DataTransformationType.LOG)
							{
								if (data_value <= 0.0)
								{
									// Substitute value
									X2_data[n2] = leZeroSubstituteDataValueLog;
								}
								else
								{
									X2_data[n2] = Math.Log10(data_value);
								}
							}
							else
							{
								X2_data[n2] = data_value;
							}
							++n2;
						}
					}
				}
			}

			// Compute the mean values...

			double X1_mean = MathUtil.mean(X1_data);
			double Y1_mean = MathUtil.mean(Y1_data);
			double X2_mean = 0.0;
			if (n2 > 0)
			{
				X2_mean = MathUtil.mean(X2_data);
			}

			// Compute the standard deviations...

			double X1_stddev = MathUtil.standardDeviation(X1_data);
			double Y1_stddev = MathUtil.standardDeviation(Y1_data);
			double X2_stddev = 0.0;
			if (n2 > 0)
			{
				try
				{
					X2_stddev = MathUtil.standardDeviation(X2_data);
				}
				catch (Exception)
				{
					// Not used anywhere so set to zero.
					X2_stddev = 0.0;
				}
			}

			// Data are already transformed so just use the normal log.
			// There is no reason to consider missing data because missing values were removed above...

			try
			{
				rd = MathUtil.regress(X1_data, Y1_data, false, _xTS.getMissing(), _yTS.getMissing(), __intercept);
			}
			catch (Exception e)
			{
				if (num_equations == 1)
				{
					Message.printWarning(10, routine, "Error performing analysis.");
					isAnalyzed(false);
					Message.printWarning(10, routine, e);
					throw new TSException("Error performing analysis");
				}
				else
				{
					// Non-fatal (just set the analysis flag to false...
					Message.printWarning(10, routine, "Error performing analysis for month " + ieq + ".");
					isAnalyzed(ieq, false);
					continue;
				}
			}
			if (rd == null)
			{
				if (num_equations == 1)
				{
					Message.printWarning(10, routine, "Error performing analysis.");
					isAnalyzed(false);
					throw new TSException("Error performing analysis");
				}
				else
				{
					// Non-fatal (just set the analysis flag to false...
					Message.printWarning(10, routine, "Error performing analysis for month " + ieq + ".");
					isAnalyzed(ieq, false);
					continue;
				}
			}

			if (__transformation == DataTransformationType.LOG)
			{
				// Update the X array to logs...
				for (int j = 0; j < xArray.Length; j++)
				{
					if (_xTS.isDataMissing(xArray[j]))
					{
						continue;
					}
					if (xArray[j] <= 0.0)
					{
						// Substitute value
						xArray[j] = leZeroSubstituteDataValueLog;
					}
					else
					{
						xArray[j] = Math.Log10(xArray[j]);
					}
				}
			}
			double X_mean = MathUtil.mean(xArray.Length, xArray, _xTS.getMissing());
			double X_stddev = MathUtil.standardDeviation(xArray.Length, xArray, _xTS.getMissing());

			double a = rd.getA();
			double b = rd.getB();
			double rmse = 0.0, rmseTransformed = 0.0;
			double[] Y1_estimated = null; // Estimated Y1 if filling data.
			if (_filling)
			{
				// Now if filling, estimate Y1 using A and B and compute the RMSE from Y1 - Y.
				// Just loop through the X1 because these points originally lined up with Y1...

				Y1_estimated = new double[n1];
				double ytemp1, ytemp2;
				for (int i = 0; i < n1; i++)
				{
					if (__transformation == DataTransformationType.LOG)
					{
						Y1_estimated[i] = a + X1_data[i] * b;
						rmseTransformed += ((Y1_estimated[i] - Y1_data[i]) * (Y1_estimated[i] - Y1_data[i]));
						// Always do untransformed data...
						ytemp1 = Math.Pow(10.0, Y1_estimated[i]);
						ytemp2 = orig_Y1_data[i];
						rmse += ((ytemp1 - ytemp2) * (ytemp1 - ytemp2));
					}
					else
					{
						Y1_estimated[i] = a + X1_data[i] * b;
						rmse += ((Y1_estimated[i] - Y1_data[i]) * (Y1_estimated[i] - Y1_data[i]));
					}
				}
				// Check to see if the relationship is within the confidence level...
				confidenceIntervalMet = true;
				if (__confidenceInterval != null)
				{
					// Get the limiting value given the confidence interval
					double alpha = (1.0 - __confidenceInterval.Value / 100.0); // double-tailed
					StudentTTest t = new StudentTTest();
					double tMet = t.getStudentTQuantile(alpha / 2.0, n1 - 2); // Single-tailed so divide by 2
					Message.printStatus(2, routine, "T based on confidence interval = " + tMet);
					// Compute the statistic based on standard error of the estimate;
					//double ssxy = sxy - sx*my1;
					//double t = ssxy/see/Math.sqrt(ssx);
					//if ( t >= tMet ) {
					//    confidenceIntervalMet = true;
					//}

				}
			}
			else
			{
				// Just use available data...
				double ytemp, xtemp;
				for (int i = 0; i < n1; i++)
				{
					if (__transformation == DataTransformationType.LOG)
					{
						rmseTransformed += ((Y1_data[i] - X1_data[i]) * (Y1_data[i] - X1_data[i]));
						// Always do untransformed data...
						ytemp = Math.Pow(10.0, Y1_data[i]);
						xtemp = Math.Pow(10.0, X1_data[i]);
						rmse += ((ytemp - xtemp) * (ytemp - xtemp));
					}
					else
					{
						rmse += ((Y1_data[i] - X1_data[i]) * (Y1_data[i] - X1_data[i]));
					}
				}
			}
			if (__transformation == DataTransformationType.LOG)
			{
				rmseTransformed = Math.Sqrt(rmseTransformed / (double)n1);
			}
			// Always do untransformed data...
			rmse = Math.Sqrt(rmse / (double)n1);

			// Save the results in this instance...

			if (num_equations == 1)
			{
				isAnalyzed(true);
				setA(rd.getA());
				setB(rd.getB());
				setCorrelationCoefficient(rd.getCorrelationCoefficient());
				setConfidenceIntervalMet(confidenceIntervalMet);
				setN1(rd.getN1());
				setN2(n2);
				if (__transformation == DataTransformationType.LOG)
				{
					setRMSETransformed(rmseTransformed);
					setStandardErrorOfEstimateTransformed(calculateStandardErrorOfEstimateFromRMSE(rmseTransformed,n1));
				}
				setRMSE(rmse);
				setStandardErrorOfEstimate(calculateStandardErrorOfEstimateFromRMSE(rmse,n1));
				setMaxX1(rd.getMaxX1());
				setMinX1(rd.getMinX1());
				setMaxY1(rd.getMaxY1());
				setMinY1(rd.getMinY1());

				setMeanX(X_mean);
				setStandardDeviationX(X_stddev);
				setMeanX1(X1_mean);
				setStandardDeviationX1(X1_stddev);
				setMeanX2(X2_mean);
				setStandardDeviationX2(X2_stddev);
				setMeanY1(Y1_mean);
				setStandardDeviationY1(Y1_stddev);
				if (_filling)
				{
					setMeanY1Estimated(MathUtil.mean(Y1_estimated));
					setStandardDeviationY1Estimated(MathUtil.standardDeviation(Y1_estimated));
				}
				setLagIntervals(rd.getLagIntervals());
			}
			else
			{
				isAnalyzed(ieq, true);
				setA(ieq, rd.getA());
				setB(ieq, rd.getB());
				setCorrelationCoefficient(ieq, rd.getCorrelationCoefficient());
				setConfidenceIntervalMet(ieq, confidenceIntervalMet);
				setN1(ieq, n1);
				setN2(ieq, n2);
				// There is no N2...
				if (__transformation == DataTransformationType.LOG)
				{
					setRMSETransformed(ieq, rmseTransformed);
					setStandardErrorOfEstimateTransformed(ieq, calculateStandardErrorOfEstimateFromRMSE(rmseTransformed,n1));
				}
				setRMSE(ieq, rmse);
				setStandardErrorOfEstimate(ieq, calculateStandardErrorOfEstimateFromRMSE(rmse,n1));
				setMaxX1(ieq, rd.getMaxX1());
				setMinX1(ieq, rd.getMinX1());
				setMaxY1(ieq, rd.getMaxY1());
				setMinY1(ieq, rd.getMinY1());

				setMeanX(ieq, X_mean);
				setStandardDeviationX(ieq, X_stddev);
				setMeanX1(ieq, X1_mean);
				setStandardDeviationX1(ieq, X1_stddev);
				setMeanX2(ieq, X2_mean);
				setStandardDeviationX2(ieq, X2_stddev);
				setMeanY1(ieq, Y1_mean);
				setStandardDeviationY1(ieq, Y1_stddev);
				if (_filling)
				{
					setMeanY1Estimated(ieq, MathUtil.mean(Y1_estimated));
					setStandardDeviationY1Estimated(ieq, MathUtil.standardDeviation(Y1_estimated));
				}
				setLagIntervals(rd.getLagIntervals());

				// TODO SAM 2010-12-18 Why does MOVE2 also set the total at when ieq = 12?
			}
			}
		catch (Exception)
		{
			// Error doing the analysis.
			if (num_equations == 1)
			{
				isAnalyzed(false);
			}
			else
			{
				isAnalyzed(ieq, false);
			}
		}
		}
		rd = null;
		routine = null;
		x1Array = null;
		y1Array = null;
	}

	/// <summary>
	/// Calculate the standard error of estimate from RMSE.  The only difference is that RMSE divides by sqrt(n) and
	/// SEE divides by sqrt(n - 2) so can calculate directly.
	/// </summary>
	private double calculateStandardErrorOfEstimateFromRMSE(double rmse, int n)
	{
		return rmse * (Math.Sqrt((double)n)) / Math.Sqrt((double)(n - 2));
	}

	/// <summary>
	/// Returns a time series containing the predicted values for the dependent time
	/// computed using the Y = a + bX where X is the values from the independent time
	/// series. The time series has the same header as the dependent time series but
	/// the location contains the "_predicted" extension.  The method also computes
	/// the residual values between the dependent predicted and actual values. These
	/// residual values are saved in a time series that has the same header as the
	/// dependent time series but the location contains the "_residual" extension.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TS createPredictedTS() throws Exception
	public virtual TS createPredictedTS()
	{
		string mthd = "TSRegression.createPredictedTS", mssg;

		// The data value to use when doing a log transform and the original value is <= 0
		double leZeroSubstituteDataValue = getLEZeroLogValue().Value;
		// Calculate the log here for use in transformed data
		double leZeroSubstituteDataValueLog = Math.Log10(leZeroSubstituteDataValue);

		// If the time series were already created, just return it.
		if (__yTSpredicted != null && __yTSresidual != null)
		{
			return __yTSpredicted;
		}

		// Create the time series for the predicted and residual time series.
		TSIdent predictedIdent = null;
		TSIdent residualIdent = null;
		try
		{
			// Create a time series that has the same header as the dependent
			// time series, but with a location that has + "_predicted".
			predictedIdent = new TSIdent(_yTS.getIdentifier());
			predictedIdent.setLocation(predictedIdent.getLocation() + "_predicted");
			// Create the new time series using the full identifier.
			__yTSpredicted = TSUtil.newTimeSeries(predictedIdent.getIdentifier(), true);
			if (__yTSpredicted == null)
			{
				mssg = "Could not create the predicted time series.";
				throw new TSException(mssg);
			}

			// Create a time series that has the same header as the dependent
			// time series, but with a location that has + "_residual".
			residualIdent = new TSIdent(_yTS.getIdentifier());
			residualIdent.setLocation(residualIdent.getLocation() + "_residual");
			// Create the new time series using the full identifier.
			__yTSresidual = TSUtil.newTimeSeries(residualIdent.getIdentifier(), true);
			if (__yTSresidual == null)
			{
				mssg = "Could not create the residual time series.";
				throw new TSException(mssg);
			}

		}
		catch (Exception e)
		{
			mssg = "Error creating new time series (" + e + ").";
			Message.printWarning(3, mthd, mssg);
			Message.printWarning(3, mthd, e);
			throw new TSException(mssg);
		}

		// Update the new time series properties with all required information.
		// Notice: CopyHeader() overwrites, among several other things,
		//	   the Identifier, the DataInterval (Base and Multiplier).
		//         It also set the dates, from the old time series. Make sure to
		//         reset these properties to the values needed by the new time
		//	   series. Finally allocate data space.
		__yTSpredicted.copyHeader(_yTS);
		__yTSpredicted.setIdentifier(predictedIdent);
		__yTSpredicted.allocateDataSpace();
		__yTSresidual.copyHeader(_yTS);
		__yTSresidual.setIdentifier(residualIdent);
		__yTSresidual.allocateDataSpace();

		// Define the number of equations to be used in the loop
		int num_equations = 1;
		if (__numberOfEquations == NumberOfEquationsType.MONTHLY_EQUATIONS)
		{
			num_equations = 12;
		}

		// Use the equation to set the data values in the predicted time series
		DateTime startDate = new DateTime(__yTSpredicted.getDate1());
		DateTime endDate = new DateTime(__yTSpredicted.getDate2());
		int interval_base = _yTS.getDataIntervalBase();
		int interval_mult = _yTS.getDataIntervalMult();
		DateTime date;
		double Xvalue, Yvalue, preYvalue, resYvalue;
		double A, B;

		for (int ieq = 1; ieq <= num_equations; ieq++)
		{

			try
			{
				if (num_equations == 1)
				{
					A = getA();
					B = getB();
				}
				else
				{
					A = getA(ieq);
					B = getB(ieq);
				}

				for (date = new DateTime(startDate); date.lessThanOrEqualTo(endDate); date.addInterval(interval_base, interval_mult))
				{

					// Process only for a single month if monthly
					if ((num_equations != 1) && (date.getMonth() != ieq))
					{
						continue;
					}

					// If not in the dependent analysis period let missing in the __yTSpredicted.
					if (date.lessThan(_dep_analysis_period_start) || date.greaterThan(_dep_analysis_period_end))
					{
						continue;
					}

					// Get the value from the independentTS.
					Xvalue = _xTS.getDataValue(date);
					Yvalue = _yTS.getDataValue(date);
					if (_xTS.isDataMissing(Xvalue))
					{
						// If missing let missing in the __yTSpredicted.
						continue;
					}

					// Now if filling, estimate Y1 using A and B
					if (__transformation == DataTransformationType.LOG)
					{
						// double Ytemp, Xtemp;
						// Estimate in log10 space.
						if (Xvalue <= 0.0)
						{
							// Substitute value
							Xvalue = leZeroSubstituteDataValueLog;
						}
						else
						{
							Xvalue = Math.Log10(Xvalue);
						}
						// Compute the estimated value.
						preYvalue = A + Xvalue * B;
						// Un-transform from log space.  
						preYvalue = Math.Pow(10.0, preYvalue);
					}
					else
					{
						preYvalue = A + Xvalue * B;
					}

					// Saving the predicted value.
					__yTSpredicted.setDataValue(date, preYvalue);

					// Computing and saving the residual value, if the YValue is not missing.
					if (!_yTS.isDataMissing(Yvalue))
					{
						resYvalue = preYvalue - Yvalue;
						__yTSresidual.setDataValue(date, resYvalue);
					}
				}
			}
			catch (Exception e)
			{
				// Error computing the predicted values.'
				mssg = "Error computing the predicted/residual values (" + e + ").";
				Message.printWarning(3, mthd, mssg);
				Message.printWarning(3, mthd, e);
				continue;
			}
		}

		return __yTSpredicted;
	}

	/// <summary>
	/// Finalize before garbage collection. </summary>
	/// <exception cref="Throwable"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~TSRegression()
	{
		__yTSpredicted = null;
		__yTSresidual = null;

		_xTS = null;
		_yTS = null;

		_b_monthly = null;
		_a_monthly = null;
		_n1_monthly = null;
		_rmseMonthly = null;
		_r_monthly = null;
		_is_analyzed_monthly = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Free the resources computed during the analysis, including arrays of numbers.  Normally these are left in
	/// memory to facilitate reporting or further analysis (although currently there are no getter methods).
	/// Freeing the resources may be necessary in large analysis.
	/// </summary>
	public virtual void freeResources()
	{
		__X = null;
		__X_monthly = null;
		__X1 = null;
		__X1_monthly = null;
		__Y1 = null;
		__Y1_monthly = null;
	}

	/// <summary>
	/// Return The "a" value in the equation Y = a + b * X where Y is the estimated
	/// time series value, a is the intercept of the equation, b is the slope, and X is
	/// the known value.  This is a value that has been calculated for each month.
	/// The base class has a getA() when only one relationship is used. </summary>
	/// <returns> A intercept value. </returns>
	/// <param name="monthIndex"> The integer representation for the month of interest (1 is January). </param>
	/// <exception cref="TSException"> if there is no regression data available for the month. </exception>
	/// <seealso cref= RTi.Util.Math.Regression#getA </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double getA(int monthIndex) throws TSException
	public virtual double getA(int monthIndex)
	{
		if (!isAnalyzed(monthIndex))
		{
			throw new TSException("No regression computed for month " + monthIndex);
		}
		if ((monthIndex < 1) || (monthIndex > 12))
		{
			throw new TSException("Month index " + monthIndex + " out of range 1-12.");
		}
		return _a_monthly[monthIndex - 1];
	}

	/// <summary>
	/// Return the analysis method. </summary>
	/// <returns> the analysis method. </returns>
	public virtual RegressionType getAnalysisMethod()
	{
		return __analysisMethod;
	}

	/// <summary>
	/// Return an array indicating if a month is to be analyzed.  This information
	/// corresponds to the AnalysisMonth property that is passed in at construction.
	/// </summary>
	public virtual bool [] getAnalyzeMonth()
	{
		return _analyze_month;
	}

	/// <summary>
	/// Return an array indicating the months to be analyzed, each value 1-12.  This information
	/// corresponds to the AnalysisMonth property that is passed in at construction. </summary>
	/// <returns> the array containing the months (1-12) to be analyzed, or null if all months will be analyzed. </returns>
	public virtual int [] getAnalysisMonths()
	{
		return _analyze_month_list;
	}

	/// <summary>
	/// Return the "b" value in the equation Y = a + b * X where Y is the estimated ts
	/// value, a is the intercept of the equation, b is the slope, and X is the known
	/// value.  This is a value which has been calculated for each month.  The base
	/// class has a getB when only one relationship is used. </summary>
	/// <returns> B slope value. </returns>
	/// <param name="monthIndex"> The integer representation for the month of interest (1 is January). </param>
	/// <exception cref="TSException"> if there is no regression data available for the month. </exception>
	/// <seealso cref= RTi.Util.Math.Regression#getB </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double getB(int monthIndex) throws TSException
	public virtual double getB(int monthIndex)
	{
		if (!isAnalyzed(monthIndex))
		{
			throw new TSException("No regression computed for month " + monthIndex);
		}
		if ((monthIndex < 1) || (monthIndex > 12))
		{
			throw new TSException("Month index " + monthIndex + " out of range 1-12.");
		}
		return _b_monthly[monthIndex - 1];
	}

	/// <summary>
	/// Determine if the confidence level for the slope has been met, for one equation. </summary>
	/// <returns> true if the slope is within the specified confidence level. </returns>
	/// <param name="monthIndex"> Index for month. </param>
	public virtual bool getConfidenceIntervalMet()
	{
		return __confidenceIntervalMet;
	}

	/// <summary>
	/// Determine if the confidence level for the slope has been met, for a monthly equation. </summary>
	/// <returns> true if the slope is within the specified confidence level. </returns>
	/// <param name="monthIndex"> Index for month. </param>
	public virtual bool getConfidenceIntervalMet(int monthIndex)
	{
		if ((monthIndex < 1) || (monthIndex > 12))
		{
			return false;
		}
		return __confidenceIntervalMetMonthly[monthIndex - 1];
	}

	/// <summary>
	/// Return the correlation coefficient between the two time series that have been
	/// analyzed.  This is a value that has been calculated for each month.  The base
	/// class has a getCorrelationCoefficient when only one relationship is used. </summary>
	/// <returns> The correlation coefficient R. </returns>
	/// <param name="monthIndex"> The integer representation for the month of interest (1 is January). </param>
	/// <exception cref="TSException"> if there is no regression data available for the month. </exception>
	/// <seealso cref= RTi.Util.Math.Regression#getCorrelationCoefficient </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double getCorrelationCoefficient(int monthIndex) throws TSException
	public virtual double getCorrelationCoefficient(int monthIndex)
	{
		if (!isAnalyzed(monthIndex))
		{
			throw new TSException("No regression computed for month " + monthIndex);
		}
		if ((monthIndex < 1) || (monthIndex > 12))
		{
			throw new TSException("Month index " + monthIndex + " out of range 1-12.");
		}
		return _r_monthly[monthIndex - 1];
	}

	/// <summary>
	/// Return the dependent time series analysis end. </summary>
	/// <returns> the dependent time series analysis end. </returns>
	public virtual DateTime getDependentAnalysisEnd()
	{
		return _dep_analysis_period_end;
	}

	/// <summary>
	/// Return the dependent time series analysis start. </summary>
	/// <returns> the dependent time series analysis start. </returns>
	public virtual DateTime getDependentAnalysisStart()
	{
		return _dep_analysis_period_start;
	}

	/// <summary>
	/// Return the dependent (Y) time series. </summary>
	/// <returns> the dependent (Y) time series. </returns>
	public virtual TS getDependentTS()
	{
		return _yTS;
	}

	/// <summary>
	/// Return the fill end (used when analyzing for filling). </summary>
	/// <returns> the fill end. </returns>
	public virtual DateTime getFillEnd()
	{
		return _fill_period_end;
	}

	/// <summary>
	/// Return the fill start (used when analyzing for filling). </summary>
	/// <returns> the fill start. </returns>
	public virtual DateTime getFillStart()
	{
		return _fill_period_start;
	}

	/// <summary>
	/// Return the independent time series analysis end. </summary>
	/// <returns> the independent time series analysis end. </returns>
	public virtual DateTime getIndependentAnalysisEnd()
	{
		return _ind_analysis_period_end;
	}

	/// <summary>
	/// Return the independent time series analysis start. </summary>
	/// <returns> the independent time series analysis start. </returns>
	public virtual DateTime getIndependentAnalysisStart()
	{
		return _ind_analysis_period_start;
	}

	/// <summary>
	/// Return the independent (X) time series. </summary>
	/// <returns> the independent (X) time series. </returns>
	public virtual TS getIndependentTS()
	{
		return _xTS;
	}

	/// <summary>
	/// Return the default value that will be used for the log transform if the original is <= 0.
	/// </summary>
	public static double getDefaultLEZeroLogValue()
	{
		return.001;
	}

	/// <summary>
	/// Return the predicted (Y) time series. </summary>
	/// <returns> the predicted (Y) time series or null if the method createPredictedTS() was not called yet. </returns>
	public virtual TS getPredictedTS()
	{
		return __yTSpredicted;
	}

	/// <summary>
	/// Return the residual TS ( difference between the predicted and the original dependent time series. </summary>
	/// <returns> the residual TS ( difference between the predicted and the original dependent time series. </returns>
	public virtual TS getResidualTS()
	{
		return __yTSresidual;
	}

	/// <summary>
	/// Get the value that is substituted for data if using the log transform and the original value is <= 0. </summary>
	/// <returns> data value to use in place of the original for calculations. </returns>
	private double? getLEZeroLogValue()
	{
		return __leZeroLogValue;
	}

	/// <summary>
	/// Return the maximum for X. </summary>
	/// <returns> the maximum for X. </returns>
	/// <param name="monthIndex"> The integer representation for the month of interest (1 is January). </param>
	/// <exception cref="TSException"> if there is no analysis data available for the month. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double getMaxX(int monthIndex) throws TSException
	public virtual double getMaxX(int monthIndex)
	{
		if (!isAnalyzed(monthIndex))
		{
			throw new TSException("No analysis results available for month " + monthIndex);
		}
		if ((monthIndex < 1) || (monthIndex > 12))
		{
			throw new TSException("Month index " + monthIndex + " out of range 1-12.");
		}
		return _X_max_monthly[monthIndex - 1];
	}

	/// <summary>
	/// Return the maximum for X1. </summary>
	/// <returns> the maximum for X1. </returns>
	/// <param name="monthIndex"> The integer representation for the month of interest (1 is January). </param>
	/// <exception cref="TSException"> if there is no analysis data available for the month. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double getMaxX1(int monthIndex) throws TSException
	public virtual double getMaxX1(int monthIndex)
	{
		if (!isAnalyzed(monthIndex))
		{
			throw new TSException("No analysis results available for month " + monthIndex);
		}
		if ((monthIndex < 1) || (monthIndex > 12))
		{
			throw new TSException("Month index " + monthIndex + " out of range 1-12.");
		}
		return _X1_max_monthly[monthIndex - 1];
	}

	/// <summary>
	/// Return the maximum for X2. </summary>
	/// <returns> the maximum for X2. </returns>
	/// <param name="monthIndex"> The integer representation for the month of interest (1 is January). </param>
	/// <exception cref="TSException"> if there is no analysis data available for the month. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double getMaxX2(int monthIndex) throws TSException
	public virtual double getMaxX2(int monthIndex)
	{
		if (!isAnalyzed(monthIndex))
		{
			throw new TSException("No analysis results available for month " + monthIndex);
		}
		if ((monthIndex < 1) || (monthIndex > 12))
		{
			throw new TSException("Month index " + monthIndex + " out of range 1-12.");
		}
		return _X2_max_monthly[monthIndex - 1];
	}

	/// <summary>
	/// Return the maximum for Y1. </summary>
	/// <returns> the maximum for Y1. </returns>
	/// <param name="monthIndex"> The integer representation for the month of interest (1 is January). </param>
	/// <exception cref="TSException"> if there is no analysis data available for the month. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double getMaxY1(int monthIndex) throws TSException
	public virtual double getMaxY1(int monthIndex)
	{
		if (!isAnalyzed(monthIndex))
		{
			throw new TSException("No analysis results available for month " + monthIndex);
		}
		if ((monthIndex < 1) || (monthIndex > 12))
		{
			throw new TSException("Month index " + monthIndex + " out of range 1-12.");
		}
		return _Y1_max_monthly[monthIndex - 1];
	}

	/// <summary>
	/// Return the mean for X. </summary>
	/// <returns> the mean for X. </returns>
	/// <param name="monthIndex"> The integer representation for the month of interest (1 is January). </param>
	/// <exception cref="TSException"> if there is no analysis data available for the month. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double getMeanX(int monthIndex) throws TSException
	public virtual double getMeanX(int monthIndex)
	{
		if (!isAnalyzed(monthIndex))
		{
			throw new TSException("No analysis results available for month " + monthIndex);
		}
		if ((monthIndex < 1) || (monthIndex > 12))
		{
			throw new TSException("Month index " + monthIndex + " out of range 1-12.");
		}
		return _X_mean_monthly[monthIndex - 1];
	}

	/// <summary>
	/// Return the mean for X1. </summary>
	/// <returns> the mean for X1. </returns>
	/// <param name="monthIndex"> The integer representation for the month of interest (1 is January). </param>
	/// <exception cref="TSException"> if there is no analysis data available for the month. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double getMeanX1(int monthIndex) throws TSException
	public virtual double getMeanX1(int monthIndex)
	{
		if (!isAnalyzed(monthIndex))
		{
			throw new TSException("No analysis results available for month " + monthIndex);
		}
		if ((monthIndex < 1) || (monthIndex > 12))
		{
			throw new TSException("Month index " + monthIndex + " out of range 1-12.");
		}
		return _X1_mean_monthly[monthIndex - 1];
	}

	/// <summary>
	/// Return the mean for X2. </summary>
	/// <returns> the mean for X2. </returns>
	/// <param name="monthIndex"> The integer representation for the month of interest (1 is January). </param>
	/// <exception cref="TSException"> if there is no analysis data available for the month. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double getMeanX2(int monthIndex) throws TSException
	public virtual double getMeanX2(int monthIndex)
	{
		if (!isAnalyzed(monthIndex))
		{
			throw new TSException("No analysis results available for month " + monthIndex);
		}
		if ((monthIndex < 1) || (monthIndex > 12))
		{
			throw new TSException("Month index " + monthIndex + " out of range 1-12.");
		}
		return _X2_mean_monthly[monthIndex - 1];
	}

	/// <summary>
	/// Return the mean for Y. </summary>
	/// <returns> the mean for Y. </returns>
	/// <param name="monthIndex"> The integer representation for the month of interest (1 is January). </param>
	/// <exception cref="TSException"> if there is no analysis data available for the month. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double getMeanY(int monthIndex) throws TSException
	public virtual double getMeanY(int monthIndex)
	{
		if (!isAnalyzed(monthIndex))
		{
			throw new TSException("No analysis results available for month " + monthIndex);
		}
		if ((monthIndex < 1) || (monthIndex > 12))
		{
			throw new TSException("Month index " + monthIndex + " out of range 1-12.");
		}
		return _Y_mean_monthly[monthIndex - 1];
	}

	/// <summary>
	/// Return the mean for Y1. </summary>
	/// <returns> the mean for Y1. </returns>
	/// <param name="monthIndex"> The integer representation for the month of interest (1 is January). </param>
	/// <exception cref="TSException"> if there is no analysis data available for the month. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double getMeanY1(int monthIndex) throws TSException
	public virtual double getMeanY1(int monthIndex)
	{
		if (!isAnalyzed(monthIndex))
		{
			throw new TSException("No analysis results available for month " + monthIndex);
		}
		if ((monthIndex < 1) || (monthIndex > 12))
		{
			throw new TSException("Month index " + monthIndex + " out of range 1-12.");
		}
		return _Y1_mean_monthly[monthIndex - 1];
	}

	/// <summary>
	/// Return the mean for Y1_estimated. </summary>
	/// <returns> the mean for Y1_estimated. </returns>
	/// <param name="monthIndex"> The integer representation for the month of interest (1 is January). </param>
	/// <exception cref="TSException"> if there is no analysis data available for the month. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double getMeanY1Estimated(int monthIndex) throws TSException
	public virtual double getMeanY1Estimated(int monthIndex)
	{
		if (!isAnalyzed(monthIndex))
		{
			throw new TSException("No analysis results available for month " + monthIndex);
		}
		if ((monthIndex < 1) || (monthIndex > 12))
		{
			throw new TSException("Month index " + monthIndex + " out of range 1-12.");
		}
		return _Y1_estimated_mean_monthly[monthIndex - 1];
	}

	/// <summary>
	/// Return the minimum for X. </summary>
	/// <returns> the minimum for X. </returns>
	/// <param name="monthIndex"> The integer representation for the month of interest (1 is January). </param>
	/// <exception cref="TSException"> if there is no analysis data available for the month. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double getMinX(int monthIndex) throws TSException
	public virtual double getMinX(int monthIndex)
	{
		if (!isAnalyzed(monthIndex))
		{
			throw new TSException("No analysis results available for month " + monthIndex);
		}
		if ((monthIndex < 1) || (monthIndex > 12))
		{
			throw new TSException("Month index " + monthIndex + " out of range 1-12.");
		}
		return _X_min_monthly[monthIndex - 1];
	}

	/// <summary>
	/// Return the minimum for X1. </summary>
	/// <returns> the minimum for X1. </returns>
	/// <param name="monthIndex"> The integer representation for the month of interest (1 is January). </param>
	/// <exception cref="TSException"> if there is no analysis data available for the month. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double getMinX1(int monthIndex) throws TSException
	public virtual double getMinX1(int monthIndex)
	{
		if (!isAnalyzed(monthIndex))
		{
			throw new TSException("No analysis results available for month " + monthIndex);
		}
		if ((monthIndex < 1) || (monthIndex > 12))
		{
			throw new TSException("Month index " + monthIndex + " out of range 1-12.");
		}
		return _X1_min_monthly[monthIndex - 1];
	}

	/// <summary>
	/// Return the minimum for X2. </summary>
	/// <returns> the minimum for X2. </returns>
	/// <param name="monthIndex"> The integer representation for the month of interest (1 is January). </param>
	/// <exception cref="TSException"> if there is no analysis data available for the month. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double getMinX2(int monthIndex) throws TSException
	public virtual double getMinX2(int monthIndex)
	{
		if (!isAnalyzed(monthIndex))
		{
			throw new TSException("No analysis results available for month " + monthIndex);
		}
		if ((monthIndex < 1) || (monthIndex > 12))
		{
			throw new TSException("Month index " + monthIndex + " out of range 1-12.");
		}
		return _X2_min_monthly[monthIndex - 1];
	}

	/// <summary>
	/// Return the minimum for Y1. </summary>
	/// <returns> the minimum for Y1. </returns>
	/// <param name="monthIndex"> The integer representation for the month of interest (1 is January). </param>
	/// <exception cref="TSException"> if there is no analysis data available for the month. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double getMinY1(int monthIndex) throws TSException
	public virtual double getMinY1(int monthIndex)
	{
		if (!isAnalyzed(monthIndex))
		{
			throw new TSException("No analysis results available for month " + monthIndex);
		}
		if ((monthIndex < 1) || (monthIndex > 12))
		{
			throw new TSException("Month index " + monthIndex + " out of range 1-12.");
		}
		return _Y1_min_monthly[monthIndex - 1];
	}

	/// <summary>
	/// Return The number of points N1 used in the analysis for the two time series.
	/// The number is by month.  The base class has a getN1 when only one relationship is used. </summary>
	/// <returns> the number of data points N1 used in an analysis. </returns>
	/// <param name="monthIndex"> The integer representation for the month of interest (1 is January). </param>
	/// <exception cref="TSException"> if the month index is out of range. </exception>
	/// <seealso cref= RTi.Util.Math.Regression#getN1 </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int getN1(int monthIndex) throws TSException
	public virtual int getN1(int monthIndex)
	{ // Always return because the number of points may illustrate a data problem.
		if ((monthIndex < 1) || (monthIndex > 12))
		{
			throw new TSException("Month index " + monthIndex + " out of range 1-12.");
		}
		return _n1_monthly[monthIndex - 1];
	}

	/// <summary>
	/// Return The number of points N2 used in the analysis for the two time series.
	/// The number is by month.  The base class has a getN1 when only one relationship is used. </summary>
	/// <returns> the number of data points N2 used in an analysis. </returns>
	/// <param name="monthIndex"> The integer representation for the month of interest (1 is January). </param>
	/// <exception cref="TSException"> if the month index is out of range. </exception>
	/// <seealso cref= RTi.Util.Math.Regression#getRMSE </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int getN2(int monthIndex) throws TSException
	public virtual int getN2(int monthIndex)
	{ // Always return because the number of points may illustrate a data problem.
		if ((monthIndex < 1) || (monthIndex > 12))
		{
			throw new TSException("Month index " + monthIndex + " out of range 1-12.");
		}
		return _n2_monthly[monthIndex - 1];
	}

	// TODO SAM 2009-08-29 Replace this method with normal get methods
	/// <summary>
	/// Return the properties that are used to control the analysis.  The properties are
	/// a full list.  The original properties passed in to the constructor may not have
	/// defined all values.  However, default property values assigned internally are
	/// reflected in the returned PropList.  See also methods that return values 
	/// directly (e.g., getDependentAnalysisPeriodStart()). </summary>
	/// <returns> the properties that are used to control the analysis. </returns>
	/*
	public PropList getPropList ()
	{	// Create a copy of the main PropList...
		PropList props = new PropList ( _props );
		// Assign values that may have been determined internally as defaults...
		props.set ( "AnalysisMethod", _AnalysisMethod );
		if ( _analyze_month_list == null ) {
			props.set ( "AnalysisMonth", "" );
		}
		props.set ( "AnalyzeForFilling", "" + _filling );
		// Use the following instead of calling toString(), to allow null,
		// although hopefully that will not occur...
		props.set ( "DependentAnalysisStart", "" + _dep_analysis_period_start );
		props.set ( "DependentAnalysisEnd", "" + _dep_analysis_period_end );
		props.set ( "FillStart", "" + _fill_period_start );
		props.set ( "FillEnd", "" + _fill_period_end );
		props.set ( "IndependentAnalysisStart", ""+_ind_analysis_period_start );
		props.set ( "IndependentAnalysisEnd", "" + _ind_analysis_period_end );
		props.set ( "Intercept", "" + __intercept );
	}
	*/

	/// <summary>
	/// Indicate whether the analysis is performed using one equation or a monthly basis. </summary>
	/// <returns> the number of equations used for the analysis. </returns>
	public virtual NumberOfEquationsType getNumberOfEquations()
	{
		return __numberOfEquations;
	}

	/// <summary>
	/// Return the RMS error for the correlation between the two time series that have
	/// been analyzed.  This is a value which has been calculated for each month.
	/// The base class has a getRMSE() when only one relationship is used. </summary>
	/// <returns> the RMS error. </returns>
	/// <param name="monthIndex"> The integer representation for the month of interest (1 is January). </param>
	/// <exception cref="TSException"> if there is no regression data available for the month. </exception>
	/// <seealso cref= RTi.Util.Math.Regression#getRMSE </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double getRMSE(int monthIndex) throws TSException
	public virtual double getRMSE(int monthIndex)
	{
		if (!isAnalyzed(monthIndex))
		{
			throw new TSException("No analysis results available for month " + monthIndex);
		}
		if ((monthIndex < 1) || (monthIndex > 12))
		{
			throw new TSException("Month index " + monthIndex + " out of range 1-12.");
		}
		return _rmseMonthly[monthIndex - 1];
	}

	/// <summary>
	/// Return the RMS error for the transformed data for a month.
	/// The base class has a getTransformedRMSE() when only one relationship is used. </summary>
	/// <returns> the RMS error for the transformed data. </returns>
	/// <param name="monthIndex"> The integer representation for the month of interest (1 is January). </param>
	/// <exception cref="TSException"> if there is no data available for the month. </exception>
	/// <seealso cref= RTi.Util.Math.Regression#getRMSE </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double getRMSETransformed(int monthIndex) throws TSException
	public virtual double getRMSETransformed(int monthIndex)
	{
		if (!isAnalyzed(monthIndex))
		{
			throw new TSException("No analysis results available for month " + monthIndex);
		}
		if ((monthIndex < 1) || (monthIndex > 12))
		{
			throw new TSException("Month index " + monthIndex + " out of range 1-12.");
		}
		return _rmseTransformedMonthly[monthIndex - 1];
	}

	/// <summary>
	/// Return the standard deviation for X. </summary>
	/// <returns> the standard deviation for X. </returns>
	/// <param name="monthIndex"> The integer representation for the month of interest (1 is January). </param>
	/// <exception cref="TSException"> if there is no analysis data available for the month. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double getStandardDeviationX(int monthIndex) throws TSException
	public virtual double getStandardDeviationX(int monthIndex)
	{
		if (!isAnalyzed(monthIndex))
		{
			throw new TSException("No analysis results available for month " + monthIndex);
		}
		if ((monthIndex < 1) || (monthIndex > 12))
		{
			throw new TSException("Month index " + monthIndex + " out of range 1-12.");
		}
		return _X_stddev_monthly[monthIndex - 1];
	}

	/// <summary>
	/// Return the standard deviation for X1. </summary>
	/// <returns> the standard deviation for X1. </returns>
	/// <param name="monthIndex"> The integer representation for the month of interest (1 is January). </param>
	/// <exception cref="TSException"> if there is no analysis data available for the month. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double getStandardDeviationX1(int monthIndex) throws TSException
	public virtual double getStandardDeviationX1(int monthIndex)
	{
		if (!isAnalyzed(monthIndex))
		{
			throw new TSException("No analysis results available for month " + monthIndex);
		}
		if ((monthIndex < 1) || (monthIndex > 12))
		{
			throw new TSException("Month index " + monthIndex + " out of range 1-12.");
		}
		return _X1_stddev_monthly[monthIndex - 1];
	}

	/// <summary>
	/// Return the standard deviation for X2. </summary>
	/// <returns> the standard deviation for X2. </returns>
	/// <param name="monthIndex"> The integer representation for the month of interest (1 is January). </param>
	/// <exception cref="TSException"> if there is no analysis data available for the month. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double getStandardDeviationX2(int monthIndex) throws TSException
	public virtual double getStandardDeviationX2(int monthIndex)
	{
		if (!isAnalyzed(monthIndex))
		{
			throw new TSException("No analysis results available for month " + monthIndex);
		}
		if ((monthIndex < 1) || (monthIndex > 12))
		{
			throw new TSException("Month index " + monthIndex + " out of range 1-12.");
		}
		return _X2_stddev_monthly[monthIndex - 1];
	}

	/// <summary>
	/// Return the standard deviation for Y. </summary>
	/// <returns> the standard deviation for Y. </returns>
	/// <param name="monthIndex"> The integer representation for the month of interest (1 is January). </param>
	/// <exception cref="TSException"> if there is no analysis data available for the month. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double getStandardDeviationY(int monthIndex) throws TSException
	public virtual double getStandardDeviationY(int monthIndex)
	{
		if (!isAnalyzed(monthIndex))
		{
			throw new TSException("No analysis results available for month " + monthIndex);
		}
		if ((monthIndex < 1) || (monthIndex > 12))
		{
			throw new TSException("Month index " + monthIndex + " out of range 1-12.");
		}
		return _Y_stddev_monthly[monthIndex - 1];
	}

	/// <summary>
	/// Return the standard deviation for Y1. </summary>
	/// <returns> the standard deviation for Y1. </returns>
	/// <param name="monthIndex"> The integer representation for the month of interest (1 is January). </param>
	/// <exception cref="TSException"> if there is no analysis data available for the month. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double getStandardDeviationY1(int monthIndex) throws TSException
	public virtual double getStandardDeviationY1(int monthIndex)
	{
		if (!isAnalyzed(monthIndex))
		{
			throw new TSException("No analysis results available for month " + monthIndex);
		}
		if ((monthIndex < 1) || (monthIndex > 12))
		{
			throw new TSException("Month index " + monthIndex + " out of range 1-12.");
		}
		return _Y1_stddev_monthly[monthIndex - 1];
	}

	/// <summary>
	/// Return the standard deviation for Y1_estimated. </summary>
	/// <returns> the standard deviation for Y1_estimated. </returns>
	/// <param name="monthIndex"> The integer representation for the month of interest (1 is January). </param>
	/// <exception cref="TSException"> if there is no analysis data available for the month. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double getStandardDeviationY1Estimated(int monthIndex) throws TSException
	public virtual double getStandardDeviationY1Estimated(int monthIndex)
	{
		if (!isAnalyzed(monthIndex))
		{
			throw new TSException("No analysis results available for month " + monthIndex);
		}
		if ((monthIndex < 1) || (monthIndex > 12))
		{
			throw new TSException("Month index " + monthIndex + " out of range 1-12.");
		}
		return _Y1_estimated_stddev_monthly[monthIndex - 1];
	}

	/// <summary>
	/// Return the standard error of estimate for the correlation between the two time series that have
	/// been analyzed.  This is a value which has been calculated for each month.
	/// The base class has a getStandardErrorOfEstimate() when only one relationship is used. </summary>
	/// <returns> the SEE for the month </returns>
	/// <param name="monthIndex"> The integer representation for the month of interest (1 is January). </param>
	/// <exception cref="TSException"> if there is no regression data available for the month. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double getStandardErrorOfEstimate(int monthIndex) throws TSException
	public virtual double getStandardErrorOfEstimate(int monthIndex)
	{
		if (!isAnalyzed(monthIndex))
		{
			throw new TSException("No analysis results available for month " + monthIndex);
		}
		if ((monthIndex < 1) || (monthIndex > 12))
		{
			throw new TSException("Month index " + monthIndex + " out of range 1-12.");
		}
		return __seeMonthly[monthIndex - 1];
	}

	/// <summary>
	/// Return the standard error of estimate for the transformed data for a month.
	/// The base class has a getStandardErrorOfEstimateTransformed() when only one relationship is used. </summary>
	/// <returns> the SEE for the transformed data for the month of interest </returns>
	/// <param name="monthIndex"> The integer representation for the month of interest (1 is January). </param>
	/// <exception cref="TSException"> if there is no data available for the month. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double getStandardErrorOfEstimateTransformed(int monthIndex) throws TSException
	public virtual double getStandardErrorOfEstimateTransformed(int monthIndex)
	{
		if (!isAnalyzed(monthIndex))
		{
			throw new TSException("No analysis results available for month " + monthIndex);
		}
		if ((monthIndex < 1) || (monthIndex > 12))
		{
			throw new TSException("Month index " + monthIndex + " out of range 1-12.");
		}
		return __seeTransformedMonthly[monthIndex - 1];
	}

	/// <summary>
	/// Get the transformation that has been applied to the data prior to the analysis. </summary>
	/// <returns> the transformation that has been applied to the data prior to the analysis. </returns>
	public virtual DataTransformationType getTransformation()
	{
		return __transformation;
	}

	/// <summary>
	/// Initialize instance data. </summary>
	/// <param name="xTS"> Independent time series. </param>
	/// <param name="yTS"> Dependent time series. </param>
	private void initialize(TS xTS, TS yTS, bool analyzeForFilling, RegressionType analysisMethod, double? intercept, NumberOfEquationsType numberOfEquations, int[] analysisMonths, DataTransformationType transformation, double? leZeroLogValue, double? confidenceInterval, DateTime dependentAnalysisStart, DateTime dependentAnalysisEnd, DateTime independentAnalysisStart, DateTime independentAnalysisEnd, DateTime fillStart, DateTime fillEnd)
	{
		// FIXME SAM 2009-04-03 Evaluate going away from PropList - too easy to have errors.  Or, add check
		// to warn about properties that are not recognized - fixed when mixed station is fixed.

		__yTSpredicted = null;
		__yTSresidual = null;

		_xTS = xTS;
		_yTS = yTS;

		_filling = analyzeForFilling; // Default previously was false

		__confidenceInterval = confidenceInterval; // Confidence level to check for slope of the line
		__confidenceIntervalMet = true; // Whether the confidence level has been met

		// Check for analysis method...

		__analysisMethod = analysisMethod;
		if (__analysisMethod == null)
		{
			__analysisMethod = RegressionType.OLS_REGRESSION; // Default
		}

		__intercept = intercept;

		// Check for monthly or one relationship (later add seasonal)...

		__numberOfEquations = numberOfEquations;
		if (numberOfEquations == null)
		{
			__numberOfEquations = NumberOfEquationsType.ONE_EQUATION; // Default
		}

		// Check for log10 or normal regression...

		__transformation = transformation;
		if (__transformation == null)
		{
			__transformation = DataTransformationType.NONE; // Default
		}

		// Check dependent analysis period...

		_dep_analysis_period_start = yTS.getDate1(); // Default and parse below
		if (dependentAnalysisStart != null)
		{
			_dep_analysis_period_start = dependentAnalysisStart;
		}
		_dep_analysis_period_end = yTS.getDate2(); // Default and parse below
		if (dependentAnalysisEnd != null)
		{
			_dep_analysis_period_end = dependentAnalysisEnd;
		}

		// Check independent analysis period...

		if (__analysisMethod == RegressionType.MOVE2)
		{
			_ind_analysis_period_start = xTS.getDate1();
			if (independentAnalysisStart != null)
			{
				_ind_analysis_period_start = independentAnalysisStart;
			}
			_ind_analysis_period_end = xTS.getDate2();
			if (independentAnalysisEnd != null)
			{
				_ind_analysis_period_end = independentAnalysisEnd;
			}
		}
		else
		{
			// Independent analysis period is the same as the dependent...
			_ind_analysis_period_start = _dep_analysis_period_start;
			_ind_analysis_period_end = _dep_analysis_period_end;
		}

		// Check fill period...

		_fill_period_start = yTS.getDate1();
		if (fillStart != null)
		{
			_fill_period_start = fillStart;
		}
		_fill_period_end = yTS.getDate2();
		if (fillEnd != null)
		{
			_fill_period_end = fillEnd;
		}

		_b_monthly = new double[12];
		_a_monthly = new double[12];
		__confidenceIntervalMetMonthly = new bool[12];
		_n1_monthly = new int[12];
		_n2_monthly = new int[12];
		_X_max_monthly = new double[12];
		_X1_max_monthly = new double[12];
		_X2_max_monthly = new double[12];
		_Y1_max_monthly = new double[12];
		_X_min_monthly = new double[12];
		_X1_min_monthly = new double[12];
		_X2_min_monthly = new double[12];
		_Y1_min_monthly = new double[12];
		_X_mean_monthly = new double[12];
		_Y_mean_monthly = new double[12];
		_X1_mean_monthly = new double[12];
		_X2_mean_monthly = new double[12];
		_Y1_mean_monthly = new double[12];
		_Y1_estimated_mean_monthly = new double[12];
		_X_stddev_monthly = new double[12];
		_Y_stddev_monthly = new double[12];
		_X1_stddev_monthly = new double[12];
		_X2_stddev_monthly = new double[12];
		_Y1_stddev_monthly = new double[12];
		_Y1_estimated_stddev_monthly = new double[12];
		_rmseMonthly = new double[12];
		_rmseTransformedMonthly = new double[12];
		__seeMonthly = new double[12];
		__seeTransformedMonthly = new double[12];
		_r_monthly = new double[12];
		_is_analyzed = false;
		_is_analyzed_monthly = new bool[12];
		_analyze_month = new bool[12];
		_analyze_month_list = null;

		for (int i = 0; i < 12; i++)
		{
			_a_monthly[i] = 0;
			_b_monthly[i] = 0;
			__confidenceIntervalMetMonthly[i] = true;
			_n1_monthly[i] = 0;
			_n2_monthly[i] = 0;
			_X_max_monthly[i] = 0.0;
			_X1_max_monthly[i] = 0.0;
			_X2_max_monthly[i] = 0.0;
			_Y1_max_monthly[i] = 0.0;
			_X_min_monthly[i] = 0.0;
			_X1_min_monthly[i] = 0.0;
			_X2_min_monthly[i] = 0.0;
			_Y1_min_monthly[i] = 0.0;
			_X_mean_monthly[i] = 0.0;
			_Y_mean_monthly[i] = 0.0;
			_X1_mean_monthly[i] = 0.0;
			_X2_mean_monthly[i] = 0.0;
			_Y1_estimated_mean_monthly[i] = 0.0;
			_X_stddev_monthly[i] = 0.0;
			_Y_stddev_monthly[i] = 0.0;
			_X1_stddev_monthly[i] = 0.0;
			_X2_stddev_monthly[i] = 0.0;
			_Y1_stddev_monthly[i] = 0.0;
			_Y1_estimated_stddev_monthly[i] = 0.0;
			_rmseMonthly[i] = 0;
			_rmseTransformedMonthly[i] = 0;
			_is_analyzed_monthly[i] = false;
			_r_monthly[i] = 0;
			_analyze_month[i] = true; // Default is to analyze all months.
		}

		// Check the month to analyze (default is all)...

		if ((analysisMonths != null) && (analysisMonths.Length > 0))
		{
			_analyze_month_list = analysisMonths;
			if (analysisMonths.Length > 0)
			{
				// Reset all monthly flags to false.  Selected months will be set to true below.
				for (int i = 0; i < 12; i++)
				{
					_analyze_month[i] = false;
				}
				int imon;
				for (int i = 0; i < analysisMonths.Length; i++)
				{
					imon = analysisMonths[i];
					// TODO SAM what to do with list if not right?  Allow exception to be thrown
					//if ( (imon >= 1) && (imon <= 12) ) {
						_analyze_month[imon - 1] = true;
					//}
				}
			}
		}
	}

	/// <summary>
	/// Determine if monthly analysis results are available. </summary>
	/// <returns> true if the analysis relationship is available for the requested month (1 is January). </returns>
	/// <param name="monthIndex"> Index for month. </param>
	public virtual bool isAnalyzed(int monthIndex)
	{
		if ((monthIndex < 1) || (monthIndex > 12))
		{
			return false;
		}
		return _is_analyzed_monthly[monthIndex - 1];
	}

	/// <summary>
	/// Set the flag indicating whether a monthly analysis relationship is available. </summary>
	/// <param name="monthIndex"> month (1 is January). </param>
	/// <param name="flag"> true if the time series have been analyzed and results are available. </param>
	public virtual bool isAnalyzed(int monthIndex, bool flag)
	{
		_is_analyzed_monthly[monthIndex - 1] = flag;
		return flag;
	}

	/// <summary>
	/// Indicate whether the analysis is for monthly equations. </summary>
	/// <returns> true if the analysis is monthly. </returns>
	public virtual bool isMonthlyAnalysis()
	{
		if (__numberOfEquations == NumberOfEquationsType.MONTHLY_EQUATIONS)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Set the A value for a particular month. </summary>
	/// <param name="monthIndex"> The numerical value for the intended month (1 is January). </param>
	/// <param name="A"> Value of A. </param>
	public virtual void setA(int monthIndex, double A)
	{
		if ((monthIndex >= 1) && (monthIndex <= 12))
		{
			_a_monthly[monthIndex - 1] = A;
		}
	}

	/// <summary>
	/// Set the B value for a particular month. </summary>
	/// <param name="monthIndex"> The numerical value for the intended month (1 is January). </param>
	/// <param name="B"> Value of B. </param>
	public virtual void setB(int monthIndex, double B)
	{
		if ((monthIndex >= 1) && (monthIndex <= 12))
		{
			_b_monthly[monthIndex - 1] = B;
		}
	}

	/// <summary>
	/// Set the flag indicating whether single equation confidence level has been met. </summary>
	/// <param name="confidenceIntervalMet"> true if the slope is within the specified confidence level. </param>
	public virtual bool setConfidenceIntervalMet(bool confidenceIntervalMet)
	{
		__confidenceIntervalMet = confidenceIntervalMet;
		return confidenceIntervalMet;
	}

	/// <summary>
	/// Set the flag indicating whether a monthly confidence level has been met. </summary>
	/// <param name="monthIndex"> month (1 is January). </param>
	/// <param name="flag"> true if the slope is within the specified confidence level. </param>
	public virtual bool setConfidenceIntervalMet(int monthIndex, bool flag)
	{
		__confidenceIntervalMetMonthly[monthIndex - 1] = flag;
		return flag;
	}

	/// <summary>
	/// Set the correlation coefficient for a particular month. </summary>
	/// <param name="monthIndex"> The numerical value for the intended month (1 is January). </param>
	/// <param name="coeff"> The correlation coefficient for the month. </param>
	public virtual void setCorrelationCoefficient(int monthIndex, double coeff)
	{
		if ((monthIndex >= 1) && (monthIndex <= 12))
		{
			_r_monthly[monthIndex - 1] = coeff;
		}
	}

	/// <summary>
	/// Set the value that is substituted for data if using the log transform and the original value is <= 0. </summary>
	/// <param name="leZeroLogValue"> data value to use in place of the original for calculations. </param>
	private void setLEZeroLogValue(double? leZeroLogValue)
	{
		__leZeroLogValue = leZeroLogValue;
	}

	/// <summary>
	/// Set the maximum of X for a particular month. </summary>
	/// <param name="monthIndex"> The numerical value for the intended month (1 is January). </param>
	/// <param name="max"> maximum for the indicated month. </param>
	public virtual void setMaxX(int monthIndex, double max)
	{
		if ((monthIndex >= 1) && (monthIndex <= 12))
		{
			_X_max_monthly[monthIndex - 1] = max;
		}
	}

	/// <summary>
	/// Set the maximum of X1 for a particular month. </summary>
	/// <param name="monthIndex"> The numerical value for the intended month (1 is January). </param>
	/// <param name="max"> maximum for the indicated month. </param>
	public virtual void setMaxX1(int monthIndex, double max)
	{
		if ((monthIndex >= 1) && (monthIndex <= 12))
		{
			_X1_max_monthly[monthIndex - 1] = max;
		}
	}

	/// <summary>
	/// Set the maximum of X2 for a particular month. </summary>
	/// <param name="monthIndex"> The numerical value for the intended month (1 is January). </param>
	/// <param name="max"> maximum for the indicated month. </param>
	public virtual void setMaxX2(int monthIndex, double max)
	{
		if ((monthIndex >= 1) && (monthIndex <= 12))
		{
			_X2_max_monthly[monthIndex - 1] = max;
		}
	}

	/// <summary>
	/// Set the maximum of Y1 for a particular month. </summary>
	/// <param name="monthIndex"> The numerical value for the intended month (1 is January). </param>
	/// <param name="max"> maximum for the indicated month. </param>
	public virtual void setMaxY1(int monthIndex, double max)
	{
		if ((monthIndex >= 1) && (monthIndex <= 12))
		{
			_Y1_max_monthly[monthIndex - 1] = max;
		}
	}

	/// <summary>
	/// Set the mean of X for a particular month. </summary>
	/// <param name="monthIndex"> The numerical value for the intended month (1 is January). </param>
	/// <param name="mean"> mean for the indicated month. </param>
	public virtual void setMeanX(int monthIndex, double mean)
	{
		if ((monthIndex >= 1) && (monthIndex <= 12))
		{
			_X_mean_monthly[monthIndex - 1] = mean;
		}
	}

	/// <summary>
	/// Set the mean of X1 for a particular month. </summary>
	/// <param name="monthIndex"> The numerical value for the intended month (1 is January). </param>
	/// <param name="mean"> mean for the indicated month. </param>
	public virtual void setMeanX1(int monthIndex, double mean)
	{
		if ((monthIndex >= 1) && (monthIndex <= 12))
		{
			_X1_mean_monthly[monthIndex - 1] = mean;
		}
	}

	/// <summary>
	/// Set the mean of X2 for a particular month. </summary>
	/// <param name="monthIndex"> The numerical value for the intended month (1 is January). </param>
	/// <param name="mean"> mean for the indicated month. </param>
	public virtual void setMeanX2(int monthIndex, double mean)
	{
		if ((monthIndex >= 1) && (monthIndex <= 12))
		{
			_X2_mean_monthly[monthIndex - 1] = mean;
		}
	}

	/// <summary>
	/// Set the mean of Y for a particular month. </summary>
	/// <param name="monthIndex"> The numerical value for the intended month (1 is January). </param>
	/// <param name="mean"> mean for the indicated month. </param>
	public virtual void setMeanY(int monthIndex, double mean)
	{
		if ((monthIndex >= 1) && (monthIndex <= 12))
		{
			_Y_mean_monthly[monthIndex - 1] = mean;
		}
	}

	/// <summary>
	/// Set the mean of Y1 for a particular month. </summary>
	/// <param name="monthIndex"> The numerical value for the intended month (1 is January). </param>
	/// <param name="mean"> mean for the indicated month. </param>
	public virtual void setMeanY1(int monthIndex, double mean)
	{
		if ((monthIndex >= 1) && (monthIndex <= 12))
		{
			_Y1_mean_monthly[monthIndex - 1] = mean;
		}
	}

	/// <summary>
	/// Set the mean of Y1_estimated for a particular month. </summary>
	/// <param name="monthIndex"> The numerical value for the intended month (1 is January). </param>
	/// <param name="mean"> mean for the indicated month. </param>
	public virtual void setMeanY1Estimated(int monthIndex, double mean)
	{
		if ((monthIndex >= 1) && (monthIndex <= 12))
		{
			_Y1_estimated_mean_monthly[monthIndex - 1] = mean;
		}
	}

	/// <summary>
	/// Set the minimum of X for a particular month. </summary>
	/// <param name="monthIndex"> The numerical value for the intended month (1 is January). </param>
	/// <param name="min"> minimum for the indicated month. </param>
	public virtual void setMinX(int monthIndex, double min)
	{
		if ((monthIndex >= 1) && (monthIndex <= 12))
		{
			_X_min_monthly[monthIndex - 1] = min;
		}
	}

	/// <summary>
	/// Set the minimum of X1 for a particular month. </summary>
	/// <param name="monthIndex"> The numerical value for the intended month (1 is January). </param>
	/// <param name="min"> minimum for the indicated month. </param>
	public virtual void setMinX1(int monthIndex, double min)
	{
		if ((monthIndex >= 1) && (monthIndex <= 12))
		{
			_X1_min_monthly[monthIndex - 1] = min;
		}
	}

	/// <summary>
	/// Set the minimum of X2 for a particular month. </summary>
	/// <param name="monthIndex"> The numerical value for the intended month (1 is January). </param>
	/// <param name="min"> minimum for the indicated month. </param>
	public virtual void setMinX2(int monthIndex, double min)
	{
		if ((monthIndex >= 1) && (monthIndex <= 12))
		{
			_X2_min_monthly[monthIndex - 1] = min;
		}
	}

	/// <summary>
	/// Set the minimum of Y1 for a particular month. </summary>
	/// <param name="monthIndex"> The numerical value for the intended month (1 is January). </param>
	/// <param name="min"> minimum for the indicated month. </param>
	public virtual void setMinY1(int monthIndex, double min)
	{
		if ((monthIndex >= 1) && (monthIndex <= 12))
		{
			_Y1_min_monthly[monthIndex - 1] = min;
		}
	}

	/// <summary>
	/// Set the number of points N1 used in the analysis for a particular month. </summary>
	/// <param name="monthIndex"> The numerical value for the intended month (1 is January). </param>
	/// <param name="n1"> The number of points used in the analysis for the month (non-missing overlapping data). </param>
	public virtual void setN1(int monthIndex, int n1)
	{
		if ((monthIndex >= 1) && (monthIndex <= 12))
		{
			_n1_monthly[monthIndex - 1] = n1;
		}
	}

	/// <summary>
	/// Set the number of points N2 used in the analysis for a particular month. </summary>
	/// <param name="monthIndex"> The numerical value for the intended month (1 is January). </param>
	/// <param name="n2"> The number of points used in the analysis for the month (non-missing overlapping data). </param>
	public virtual void setN2(int monthIndex, int n2)
	{
		if ((monthIndex >= 1) && (monthIndex <= 12))
		{
			_n2_monthly[monthIndex - 1] = n2;
		}
	}

	/// <summary>
	/// Set the RMS error for a particular month. </summary>
	/// <param name="monthIndex"> The numerical value for the intended month (1 is January). </param>
	/// <param name="rmse"> RMS error for the indicated month. </param>
	public virtual void setRMSE(int monthIndex, double rmse)
	{
		if ((monthIndex >= 1) && (monthIndex <= 12))
		{
			_rmseMonthly[monthIndex - 1] = rmse;
		}
	}

	/// <summary>
	/// Set the RMS error for the transformed data for a particular month. </summary>
	/// <param name="monthIndex"> The numerical value for the intended month (1 is January). </param>
	/// <param name="rmseTransformed"> RMS error for the transformed data for the indicated month. </param>
	public virtual void setRMSETransformed(int monthIndex, double rmseTransformed)
	{
		if ((monthIndex >= 1) && (monthIndex <= 12))
		{
			_rmseTransformedMonthly[monthIndex - 1] = rmseTransformed;
		}
	}

	/// <summary>
	/// Set the standard deviation of X for a particular month. </summary>
	/// <param name="monthIndex"> The numerical value for the intended month (1 is January). </param>
	/// <param name="stddev"> Standard deviation for the indicated month. </param>
	public virtual void setStandardDeviationX(int monthIndex, double stddev)
	{
		if ((monthIndex >= 1) && (monthIndex <= 12))
		{
			_X_stddev_monthly[monthIndex - 1] = stddev;
		}
	}

	/// <summary>
	/// Set the standard deviation of X1 for a particular month. </summary>
	/// <param name="monthIndex"> The numerical value for the intended month (1 is January). </param>
	/// <param name="stddev"> Standard deviation for the indicated month. </param>
	public virtual void setStandardDeviationX1(int monthIndex, double stddev)
	{
		if ((monthIndex >= 1) && (monthIndex <= 12))
		{
			_X1_stddev_monthly[monthIndex - 1] = stddev;
		}
	}

	/// <summary>
	/// Set the standard deviation of X2 for a particular month. </summary>
	/// <param name="monthIndex"> The numerical value for the intended month (1 is January). </param>
	/// <param name="stddev"> Standard deviation for the indicated month. </param>
	public virtual void setStandardDeviationX2(int monthIndex, double stddev)
	{
		if ((monthIndex >= 1) && (monthIndex <= 12))
		{
			_X2_stddev_monthly[monthIndex - 1] = stddev;
		}
	}

	/// <summary>
	/// Set the standard deviation of Y for a particular month. </summary>
	/// <param name="monthIndex"> The numerical value for the intended month (1 is January). </param>
	/// <param name="stddev"> Standard deviation for the indicated month. </param>
	public virtual void setStandardDeviationY(int monthIndex, double stddev)
	{
		if ((monthIndex >= 1) && (monthIndex <= 12))
		{
			_Y_stddev_monthly[monthIndex - 1] = stddev;
		}
	}

	/// <summary>
	/// Set the standard deviation of Y1 for a particular month. </summary>
	/// <param name="monthIndex"> The numerical value for the intended month (1 is January). </param>
	/// <param name="stddev"> Standard deviation for the indicated month. </param>
	public virtual void setStandardDeviationY1(int monthIndex, double stddev)
	{
		if ((monthIndex >= 1) && (monthIndex <= 12))
		{
			_Y1_stddev_monthly[monthIndex - 1] = stddev;
		}
	}

	/// <summary>
	/// Set the standard deviation of Y1_estimated for a particular month. </summary>
	/// <param name="monthIndex"> The numerical value for the intended month (1 is January). </param>
	/// <param name="stddev"> Standard deviation for the indicated month. </param>
	public virtual void setStandardDeviationY1Estimated(int monthIndex, double stddev)
	{
		if ((monthIndex >= 1) && (monthIndex <= 12))
		{
			_Y1_estimated_stddev_monthly[monthIndex - 1] = stddev;
		}
	}

	/// <summary>
	/// Set the standard error of estimate for a particular month. </summary>
	/// <param name="monthIndex"> The numerical value for the intended month (1 is January). </param>
	/// <param name="see"> SEE for the indicated month. </param>
	public virtual void setStandardErrorOfEstimate(int monthIndex, double see)
	{
		if ((monthIndex >= 1) && (monthIndex <= 12))
		{
			__seeMonthly[monthIndex - 1] = see;
		}
	}

	/// <summary>
	/// Set the standard error of estimate for the transformed data for a particular month. </summary>
	/// <param name="monthIndex"> The numerical value for the intended month (1 is January). </param>
	/// <param name="seeTransformed"> SEE for the transformed data for the indicated month. </param>
	public virtual void setStandardErrorOfEstimateTransformed(int monthIndex, double seeTransformed)
	{
		if ((monthIndex >= 1) && (monthIndex <= 12))
		{
			__seeTransformedMonthly[monthIndex - 1] = seeTransformed;
		}
	}

	/// <summary>
	/// Return string representation of analysis data. </summary>
	/// <returns> A string representation of the regression data.  A multi-line table
	/// of results is returned, suitable for inclusion in a report.  The lines are separated by "\n". </returns>
	public override string ToString()
	{
		string nl = "\n";
		StringBuilder stats = new StringBuilder();

		// Print header information...

		if (_filling)
		{
			stats.Append("Independent time series (X, " + _xTS.getDate1() + " - " + _xTS.getDate2() + "): " + _xTS.getIdentifierString() + " (" + _xTS.getDescription() + ") " + _xTS.getDataUnits() + nl + "Time series being filled (Y, dependent, " + _yTS.getDate1() + " - " + _yTS.getDate2() + "): " + _yTS.getIdentifierString() + " (" + _yTS.getDescription() + ") " + _yTS.getDataUnits() + nl);
		}
		else
		{
			stats.Append("Independent time series (X, " + _xTS.getDate1() + " - " + _xTS.getDate2() + "): " + _xTS.getIdentifierString() + " (" + _xTS.getDescription() + ") " + _xTS.getDataUnits() + nl + "Dependent time series (Y, " + _yTS.getDate1() + " - " + _yTS.getDate2() + "): " + _yTS.getIdentifierString() + " (" + _yTS.getDescription() + ") " + _yTS.getDataUnits() + nl);
		}

		if (__analysisMethod == RegressionType.MOVE2)
		{
			stats.Append("Dependent analysis period:  " + _dep_analysis_period_start.ToString() + " to " + _dep_analysis_period_end.ToString() + nl);
			stats.Append("Independent analysis period:  " + _ind_analysis_period_start.ToString() + " to " + _ind_analysis_period_end.ToString() + nl);
		}
		else
		{
			// Analysis period applies to both dependent and independent
			stats.Append("Analysis period:  " + _dep_analysis_period_start.ToString() + " to " + _dep_analysis_period_end.ToString() + nl);
		}
		if (_filling)
		{
			stats.Append("Fill period:  " + _fill_period_start.ToString() + " to " + _fill_period_end.ToString() + nl);
		}
		if (__intercept != null)
		{
			stats.Append("Intercept (A) was assigned, not calculated." + nl);
		}
		if (__analysisMethod == RegressionType.OLS_REGRESSION)
		{
			stats.Append("Analysis method:  Ordinary Least Squares Regression" + nl);
		}
		else if (__analysisMethod == RegressionType.MOVE1)
		{
			stats.Append("Analysis method:  Maintenance of Variance Extension (MOVE.1)" + nl);
		}
		else if (__analysisMethod == RegressionType.MOVE2)
		{
			stats.Append("Analysis method:  Maintenance of Variance Extension (MOVE.2)" + nl);
		}

		stats.Append("Data transformation:  " + __transformation + nl);

		if (__numberOfEquations == NumberOfEquationsType.MONTHLY_EQUATIONS)
		{
			stats.Append("Number of equations:  12 (Monthly)" + nl);
			for (int i = 1; i <= 12; i++)
			{
				if (!isAnalyzed(i))
				{
					stats.Append("THE ANALYSIS FOR " + TimeUtil.monthAbbreviation(i) + " FAILED FOR SOME REASON.  The results shown are for information only." + nl);
				}
			}
			stats.Append(nl);
		}
		else
		{
			stats.Append("Number of equations:  1" + nl);
			if (!isAnalyzed())
			{
				stats.Append("THE ANALYSIS FAILED FOR SOME REASON.  The results " + "shown are for information only." + nl);
			}
			stats.Append(nl);
		}

		// Print a table with the independent station information...

		string format = "%12.2f"; // Normal data - probably should check units (SAMX)
		if (__transformation != DataTransformationType.NONE)
		{
			format = "%12.6f"; // Need more precision for transformed data
		}
		stats.Append("-------------------------------------------------------------------------------------------------");
		stats.Append(nl);
		stats.Append("|   |                                     Independent (X)                                       |" + nl);
		stats.Append("|Mon|  N1  |   MeanX1   |    SX1     |  N2  |   MeanX2   |    SX2     |    MeanX   |     SX     |" + nl);
		//	           "|XXX XXXXXX XXXXXXXXX.XX XXXXXXXXX.XX XXXXXX XXXXXXXXX.XX XXXXXXXXX.XX XXXXXXXXX.XX XXXXXXXXX.XX|"
		//      "XXXXXXXXX.XX XXXXXXXXX.XX sX.XXXX XXXXXXXXX.XX "
		//			       "XXXXXXXXX.XX XXXXXXXXX.XX "
		stats.Append("-------------------------------------------------------------------------------------------------" + nl);
		if (__numberOfEquations == NumberOfEquationsType.MONTHLY_EQUATIONS)
		{
			for (int i = 1; i <= 12; i++)
			{
				stats.Append("|" + TimeUtil.monthAbbreviation(i) + "|");
				try
				{
					stats.Append(StringUtil.formatString(getN1(i),"%6d") + "|" + StringUtil.formatString(getMeanX1(i),format) + "|" + StringUtil.formatString(getStandardDeviationX1(i),format) + "|" + StringUtil.formatString(getN2(i),"%6d") + "|" + StringUtil.formatString(getMeanX2(i),format) + "|" + StringUtil.formatString(getStandardDeviationX2(i),format) + "|" + StringUtil.formatString(getMeanX(i),format) + "|" + StringUtil.formatString(getStandardDeviationX(i),format) + "|" + nl);
				}
				catch (TSException)
				{
					// Should never get this since we checked isAnalyzed().
					stats.Append(nl);
				}
			}
		}
		else
		{
			// Single equation...
			try
			{
				// Else do full format...
				stats.Append("|All|" + StringUtil.formatString(getN1(),"%6d") + "|" + StringUtil.formatString(getMeanX1(),format) + "|" + StringUtil.formatString(getStandardDeviationX1(),format) + "|" + StringUtil.formatString(getN2(),"%6d") + "|" + StringUtil.formatString(getMeanX2(),format) + "|" + StringUtil.formatString(getStandardDeviationX2(),format) + "|" + StringUtil.formatString(getMeanX(),format) + "|" + StringUtil.formatString(getStandardDeviationX(),format) + "|" + nl);
			}
			catch (Exception)
			{
				// TODO SAM 2009-03-10 need to handle
			}
		}
		stats.Append("-------------------------------------------------------------------------------------------------" + nl);

		// Now print a table with dependent and line fit results...

		stats.Append("----------------------------------------------------------------------------------------------------------------");
		if (__transformation != DataTransformationType.NONE)
		{
			stats.Append("-------------");
		}
		if (_filling)
		{
			stats.Append("--------------------------");
		}
		stats.Append(nl);
		stats.Append("|   |                   Dependent (Y)                   |         Line Fit Results                |            ");
		if (__transformation != DataTransformationType.NONE)
		{
			stats.Append("             ");
		}
		if (_filling)
		{
			stats.Append("                          |");
		}
		else
		{
			stats.Append("|");
		}
		stats.Append(nl);
		stats.Append("|Mon|   MeanY1   |    SY1     |   MeanY    |     SY     |     A      |      B     |   R   |  R^2  |");
		if (__transformation != DataTransformationType.NONE)
		{
			stats.Append("RMSE (log10)|");
			stats.Append("RMSE (data) |");
		}
		else
		{
			stats.Append("    RMSE    |");
		}
		if (_filling)
		{
			stats.Append(" MeanY1_est |   SY1_est  |");
		}
		stats.Append(nl);
		//stats.append (
		//"         RMS Error  NumPoints" + nl );
		//	"XXX XXXXXX XXXXXXXXX.XX XXXXXXXXX.XX XXXXXX XXXXXXXXX.XX "
		//      "XXXXXXXXX.XX XXXXXXXXX.XX XXXXXXXXX.XX XXXXXXXXX.XX "
		//      "XXXXXXXXX.XX XXXXXXXXX.XX sX.XXXX XXXXXXXXX.XX "
		//			       "XXXXXXXXX.XX XXXXXXXXX.XX "
		stats.Append("----------------------------------------------------------------------------------------------------------------");
		if (__transformation != DataTransformationType.NONE)
		{
			stats.Append("-------------");
		}
		if (_filling)
		{
			stats.Append("--------------------------");
		}
		stats.Append(nl);
		if (__numberOfEquations == NumberOfEquationsType.MONTHLY_EQUATIONS)
		{
			for (int i = 1; i <= 12; i++)
			{
				stats.Append("|" + TimeUtil.monthAbbreviation(i) + "|");
				try
				{
					stats.Append(StringUtil.formatString(getMeanY1(i),format) + "|" + StringUtil.formatString(getStandardDeviationY1(i),format) + "|" + StringUtil.formatString(getMeanY(i),format) + "|" + StringUtil.formatString(getStandardDeviationY(i),format) + "|" + StringUtil.formatString(getA(i),format) + "|" + StringUtil.formatString(getB(i),format) + "|" + StringUtil.formatString(getCorrelationCoefficient(i),"%7.4f") + "|" + StringUtil.formatString(getCorrelationCoefficient(i) * getCorrelationCoefficient(i),"%7.4f") + "|");
					if (__transformation != DataTransformationType.NONE)
					{
						stats.Append(StringUtil.formatString(getRMSETransformed(i),"%12.4f") + "|");
					}
					stats.Append(StringUtil.formatString(getRMSE(i),"%12.2f") + "|");
					if (_filling)
					{
						stats.Append(StringUtil.formatString(getMeanY1Estimated(i),format) + "|" + StringUtil.formatString(getStandardDeviationY1Estimated(i),format) + "|");
					}
					stats.Append(nl);
				}
				catch (TSException)
				{
					// Should never get this since we checked isAnalyzed().
					stats.Append(nl);
				}
			}
		}
		else
		{
			// Single equation...
			try
			{
				stats.Append("|All|" + StringUtil.formatString(getMeanY1(),format) + "|" + StringUtil.formatString(getStandardDeviationY1(),format) + "|" + StringUtil.formatString(getMeanY(),format) + "|" + StringUtil.formatString(getStandardDeviationY(),format) + "|" + StringUtil.formatString(getA(),format) + "|" + StringUtil.formatString(getB(),format) + "|" + StringUtil.formatString(getCorrelationCoefficient(),"%7.4f") + "|" + StringUtil.formatString(getCorrelationCoefficient() * getCorrelationCoefficient(),"%7.4f") + "|");
				if (__transformation != DataTransformationType.NONE)
				{
					stats.Append(StringUtil.formatString(getRMSETransformed(),"%12.4f") + "|");
				}
				stats.Append(StringUtil.formatString(getRMSE(),"%12.2f") + "|");
				if (_filling)
				{
					stats.Append(StringUtil.formatString(getMeanY1Estimated(),format) + "|" + StringUtil.formatString(getStandardDeviationY1Estimated(),format) + "|");
				}
				stats.Append(nl);
			}
			catch (Exception)
			{
				// FIXME SAM 2009-03-10 need to handle
			}
		}
		stats.Append("----------------------------------------------------------------------------------------------------------------");
		if (__transformation != DataTransformationType.NONE)
		{
			stats.Append("-------------");
		}
		if (_filling)
		{
			stats.Append("--------------------------");
		}
		stats.Append(nl);

		if (__numberOfEquations == NumberOfEquationsType.MONTHLY_EQUATIONS)
		{
			// Include a total RMSE row...
			stats.Append("|All|                                                                                     |");
			if (__transformation != DataTransformationType.NONE)
			{
				stats.Append(StringUtil.formatString(getRMSETransformed(),"%12.4f") + "|");
			}
			stats.Append(StringUtil.formatString(getRMSE(),"%12.2f") + "|");
			if (_filling)
			{
				stats.Append("                         |");
			}
			stats.Append(nl);
			stats.Append("----------------------------------------------------------------------------------------------------------------");
			if (__transformation != DataTransformationType.NONE)
			{
				stats.Append("-------------");
			}
			if (_filling)
			{
				stats.Append("--------------------------");
			}
			stats.Append(nl);
		}

		// Common footnotes...

		//stats.append (
		//"Minimum/Maximum from independent (X) time series = " +
		//_min_x1 + "/" + StringUtil.formatString(_max_x1,"%.6f") + nl +
		//"Minimum/Maximum from dependent (Y) series = " +
		//_min_y1 + "/" + StringUtil.formatString(_max_y1,"%.6f") + nl +
		stats.Append("N1 indicates analysis period where X and Y are non-missing.  " + "N2 indicates analysis period where only X is non-missing." + nl);
		if (__analysisMethod == RegressionType.MOVE2)
		{
			stats.Append("The variance of the X2 values is considered in the MOVE.2 procedure." + nl);
			stats.Append("MeanX and SX are for the period N1 + N2." + nl);
		}
		else
		{
			stats.Append("The N2 and full period values are provided as" + " information but are not considered in the regression analysis." + nl);
			stats.Append("MeanX and SX are for the dependent available period (may be different than the analysis period)." + nl);
		}
		if (_filling)
		{
			stats.Append("RMSE = sqrt(sum((Y1_est - Y1)^2)/N1), where" + " Y1_est is estimated using X1, A, and B, and Y1 is observed." + nl);
		}
		else
		{
			stats.Append("RMSE = sqrt(sum((Y1 - X1)^2)/N1), where Y1 is dependent and X1 is independent." + nl);
		}
		if (__transformation == DataTransformationType.LOG)
		{
			stats.Append("RMSE is shown for original data units and log10 transform of data." + nl);
		}
		return stats.ToString();
	}

	}

}