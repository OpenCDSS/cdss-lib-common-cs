using System;
using System.Collections.Generic;

// TSUtil_FillRegression - helper class to help with FillRegression and FillMOVE2 commands.

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

	using BestFitIndicatorType = RTi.Util.Math.BestFitIndicatorType;
	using DataTransformationType = RTi.Util.Math.DataTransformationType;
	using NumberOfEquationsType = RTi.Util.Math.NumberOfEquationsType;
	using RegressionFilledValues = RTi.Util.Math.RegressionFilledValues;
	using RegressionType = RTi.Util.Math.RegressionType;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using DataTable = RTi.Util.Table.DataTable;
	using TableField = RTi.Util.Table.TableField;
	using TableRecord = RTi.Util.Table.TableRecord;
	using DateTime = RTi.Util.Time.DateTime;
	using TimeUtil = RTi.Util.Time.TimeUtil;

	/// <summary>
	/// Helper class to help with FillRegression and FillMOVE2 commands.
	/// </summary>
	public class TSUtil_FillRegression
	{

	/// <summary>
	/// Time series being filled.
	/// </summary>
	private TS __tsToFill = null;

	/// <summary>
	/// Analysis method (regression type).
	/// </summary>
	private RegressionType __analysisMethod = RegressionType.OLS_REGRESSION; // Default

	/// <summary>
	/// Indicates whether to fill with one equation or monthly equations.
	/// </summary>
	private NumberOfEquationsType __numberOfEquations = null;

	/// <summary>
	/// List of month numbers to analyze when using one equation, where each month is 1-12 (Jan - Dec),
	/// or null to analyze all months.
	/// </summary>
	private int[] __analysisMonths = null;

	/// <summary>
	/// Data value to substitute for the original when using a log transform and the original value is <= 0.
	/// Can be any number > 0.
	/// </summary>
	private double? __leZeroLogValue = new double?(TSRegressionAnalysis.getLEZeroLogValueDefault()); // Default

	/// <summary>
	/// The intercept to force, or null if not forcing.  If set, only zero is allowed and it is only used with
	/// OLS regression.
	/// </summary>
	private double? __forcedIntercept = null;

	/// <summary>
	/// Analysis period start for the dependent (Y) time series.
	/// </summary>
	private DateTime __dependentAnalysisStart = null;

	/// <summary>
	/// Analysis period end for the dependent (Y) time series.
	/// </summary>
	private DateTime __dependentAnalysisEnd = null;

	/// <summary>
	/// Analysis period start for the independent (X) time series.  For OLS and MOVE2,
	/// this is the same as the dependent analysis period.  For MOVE2 it can be different.
	/// </summary>
	private DateTime __independentAnalysisStart = null;

	/// <summary>
	/// Analysis period end for the independent (X) time series.  For OLS and MOVE2,
	/// this is the same as the dependent analysis period.  For MOVE2 it can be different.
	/// </summary>
	private DateTime __independentAnalysisEnd = null;

	/// <summary>
	/// Minimum sample size needed for a good relationship.
	/// </summary>
	private int? __minimumSampleSize = null;

	/// <summary>
	/// Minimum R needed for a good relationship;
	/// </summary>
	private double? __minimumR = null;

	/// <summary>
	/// Start of filling.
	/// </summary>
	private DateTime __fillStart;

	/// <summary>
	/// End of filling.
	/// </summary>
	private DateTime __fillEnd;

	/// <summary>
	/// Flag to mark filled values.
	/// </summary>
	private string __fillFlag = null;

	/// <summary>
	/// Fill flag description.
	/// </summary>
	private string __fillFlagDesc = null;

	/// <summary>
	/// Whether to actually do the filling (or just compute the relationships).
	/// </summary>
	private bool __doFill = true;

	/// <summary>
	/// Description string to append to time series description, rather than using default.
	/// </summary>
	private string __descriptionString = null;

	/// <summary>
	/// The list of TSRegressionAnalysis objects for mixed station.
	/// </summary>
	private IList<TSRegressionAnalysis> __tsRegressionAnalysisList = null;

	/// <summary>
	/// The TSRegressionFilledValues object that contains information about filled values.
	/// </summary>
	private TSRegressionFilledValues __tsRegressionFilledValues = null;

	/// <summary>
	/// List of runtime errors generated by this command, guaranteed to be non-null.
	/// </summary>
	private IList<string> __problems = new List<string>();

	/// <summary>
	/// How should values be sorted for fill mixed station?
	/// </summary>
	private BestFitIndicatorType __bestFit;

	/// <summary>
	/// Should values of 0 be ignored?
	/// Default is false
	/// Set by LEZeroLogValue being "missing"
	/// </summary>
	private bool __ignoreZero = false;

	/// <summary>
	///Constructor.
	/// </summary>
	/// <param name="analysisMethod"> What kind of analysis to use? Currently only OLS regression is supported. </param>
	/// <param name="numberOfEquations"> Monthly, single, or both? </param>
	/// <param name="analysisMonths"> Which months should I be looking at? </param>
	/// <param name="transformation"> Untransformed data or log? (more might be supported later) </param>
	/// <param name="leZeroLogValue"> If the value is 0 and it's a log transform, what should I use? </param>
	/// <param name="forcedIntercept"> Force the regression line to go through this point. </param>
	/// <param name="dependentAnalysisStart"> When to start looking for data? </param>
	/// <param name="dependentAnalysisEnd"> When to end? </param>
	/// <param name="independentAnalysisStart"> Currently should be same as dependent; exists to ease implementation of other analysis types. </param>
	/// <param name="independentAnalysisEnd"> Currently should be same as dependent; exists to ease implementation of other analysis types. </param>
	/// <param name="minimumSampleSize"> How much overlap between dependent and independent is necessary (if unspecified, defaults to 3). </param>
	/// <param name="minimumR"> How large does the correlation coefficient need to be? </param>
	/// <param name="confidenceIntervalPercent"> What t-test value does it need to pass? </param>
	/// <param name="fillStart"> When to start filling? </param>
	/// <param name="fillEnd"> When to end? </param>
	/// <param name="fillFlag"> What should mark filled values? </param>
	/// <param name="fillFlagDesc"> What describes that flag? </param>
	/// <param name="descriptionString"> </param>
	/// <param name="tsRegressionAnalysisList"> The analyses to use to fill. </param>
	public TSUtil_FillRegression(TS dependent, RegressionType analysisMethod, int[] analysisMonths, string leZeroLogValue, double? forcedIntercept, DateTime dependentAnalysisStart, DateTime dependentAnalysisEnd, DateTime independentAnalysisStart, DateTime independentAnalysisEnd, int? minimumSampleSize, double? minimumR, double? confidenceIntervalPercent, DateTime fillStart, DateTime fillEnd, string fillFlag, string fillFlagDesc, string descriptionString, BestFitIndicatorType sortMethod, IList<TSRegressionAnalysis> tsRegressionAnalysisList)
	{

		__tsToFill = dependent;
		__analysisMethod = analysisMethod;
		__analysisMonths = analysisMonths;
		if (!string.ReferenceEquals(leZeroLogValue, null))
		{
			if (leZeroLogValue.Equals("Missing", StringComparison.OrdinalIgnoreCase))
			{
				__ignoreZero = true;
			}
			else
			{
				__leZeroLogValue = Convert.ToDouble(leZeroLogValue);
			}
		}
		__forcedIntercept = forcedIntercept;
		// Dependent analysis period
		// If dates are null, get from the time series
		if (dependentAnalysisStart == null)
		{
			__dependentAnalysisStart = new DateTime(__tsToFill.getDate1());
		}
		else
		{
			__dependentAnalysisStart = new DateTime(dependentAnalysisStart);
		}
		if (dependentAnalysisEnd == null)
		{
			__dependentAnalysisEnd = new DateTime(__tsToFill.getDate2());
		}
		else
		{
			__dependentAnalysisEnd = new DateTime(dependentAnalysisEnd);
		}
		// Independent analysis period...
		if (__analysisMethod == RegressionType.OLS_REGRESSION)
		{
			// Independent analysis period is the same as the dependent...
			__independentAnalysisStart = new DateTime(__dependentAnalysisStart);
			__independentAnalysisEnd = new DateTime(__dependentAnalysisEnd);
		}
		//no clue what to do for non-OLS

		if (minimumSampleSize != null && minimumSampleSize >= 3)
		{
			__minimumSampleSize = minimumSampleSize;
		}
		else
		{
			//necessary because at one point we divide by sample size - 2....
			__minimumSampleSize = 3;
		}
		__minimumR = minimumR;
		if (fillStart == null)
		{
			__fillStart = new DateTime(__tsToFill.getDate1());
		}
		else
		{
			__fillStart = new DateTime(fillStart);
		}
		if (fillEnd == null)
		{
			__fillEnd = new DateTime(__tsToFill.getDate2());
		}
		else
		{
			__fillEnd = new DateTime(fillEnd);
		}
		__fillFlag = fillFlag;
		__fillFlagDesc = fillFlagDesc;
		__descriptionString = descriptionString;

		__bestFit = sortMethod;
		__tsRegressionAnalysisList = tsRegressionAnalysisList;
	}

	/// <summary>
	/// Determine the flag to tag the filled time series values. </summary>
	/// <param name="numEquations"> 1 or 12 if monthly equations </param>
	/// <param name="iEquation"> equation being processed (1+). </param>
	/// <param name="rank"> rank for independent time series relationship. </param>
	/// <returns> string flag to tag time series </returns>
	private string determineFillFlag(int numEquations, int iEquation, int rank)
	{
		if (numEquations == 1)
		{
			return "" + rank;
		}
		else
		{
			// Monthly
			return TimeUtil.monthAbbreviation(iEquation) + rank;
		}
	}

	/// <summary>
	/// Fill the dependent time series
	/// </summary>
	public virtual void fill()
	{
		string routine = "TSUtil_FillRegression.fill";
		int dl = 10; // Debug level

		IList<string> problems = getProblems();
		TS tsToFill = getTSToFill();
		string fillFlag = getFillFlag();

		double? leZeroLogValue = getLEZeroLogValue();
		double leZeroLogValue2 = Math.Log10(leZeroLogValue);

		int intervalBase = tsToFill.getDataIntervalBase();
		int intervalMult = tsToFill.getDataIntervalMult();

		bool fillFlag_boolean = false; // Indicate whether to use flag
		if ((!string.ReferenceEquals(fillFlag, null)) && (fillFlag.Length > 0))
		{
			fillFlag_boolean = true;
		}

		DateTime fillStart = getFillStart();
		DateTime fillEnd = getFillEnd();

		int errorCount = 0; // Count of errors filling (limited to 100 because most likely a logic issue)
		int fillCountSingle = 0; // To know whether to add to genesis
		int[] fillCountMonthly = new int[12];
		// Arrays for filled values, used to compute post-filling statistics
		int fullDataCount = TSUtil.calculateDataSize(tsToFill, fillStart, fillEnd);
		double[] singleEquationFilledValues = new double[fullDataCount];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: double[][] monthlyEquationFilledValues = new double[12][fullDataCount];
		double[][] monthlyEquationFilledValues = RectangularArrays.RectangularDoubleArray(12, fullDataCount);
		Message.printStatus(2, routine, "Filling dependent time series " + tsToFill.getIdentifier().toStringAliasAndTSID() + " from " + fillStart + " to " + fillEnd);
		for (DateTime date = new DateTime(fillStart); date.lessThanOrEqualTo(fillEnd); date.addInterval(intervalBase, intervalMult))
		{
			int month = date.getMonth();
			try
			{
				// TODO SAM - need to evaluate this - use isAnalyzed() to improve performance
				if (tsToFill.isDataMissing(tsToFill.getDataValue(date)))
				{
					// Try to fill the value...

					double? SEP = double.MaxValue;
					double? R = -double.MaxValue;
					bool firstTime = true;

					foreach (TSRegressionAnalysis analysis in __tsRegressionAnalysisList)
					{

						double newval = 0.0, x = 0.0;
						TS tsIndependent = analysis.getIndependentTS();
						bool[] analysisMonthsMask = analysis.getAnalysisMonthsMask();
						bool fillSingle = analysis.getAnalyzeSingleEquation();

						bool[] regressionChecksMask = null;

						if (fillSingle)
						{
							regressionChecksMask = analysis.getTSRegressionChecksMaskSingle();
						}
						else
						{
							regressionChecksMask = analysis.getTSRegressionChecksMaskMonthly();
						}

						x = tsIndependent.getDataValue(date);
						if (tsIndependent.isDataMissing(x))
						{
							// No independent value so can't fill
							continue;
						}
						if (__ignoreZero && x == 0)
						{
							//ignoring 0, so can't fill
							continue;
						}
						// Skip the month if not requested
						if (!analysisMonthsMask[month - 1])
						{
							continue;
						}
						if (!regressionChecksMask[month - 1])
						{
							// Don't have a valid relationship to do filling for the specific month so skip value
							continue;
						}

						//go through each independent that passed the requirements
						TSRegressionResults analysisResults = null;
						TSRegressionData analysisData = null;
						TSRegressionEstimateErrors errors = null;
						double a, b; // Coefficients for regression relationship
						double r; //correlation coefficient, used only for sorting
						int n1; //number of overlapping values
						double xbar; //mean
						double var; //variance
						double? see; //standard error of estimate
						DataTransformationType transformation = analysis.getTransformation();

						if (transformation == DataTransformationType.NONE)
						{
							analysisResults = analysis.getTSRegressionResults();
							analysisData = analysis.getTSRegressionData();
							errors = analysis.getTSRegressionEstimateErrors();
						}
						else
						{
							analysisResults = analysis.getTSRegressionResultsTransformed();
							analysisData = analysis.getTSRegressionDataTransformed();
							errors = analysis.getTSRegressionErrorsTransformed();
						}
						if (Message.isDebugOn)
						{
							Message.printDebug(2, routine, "Dependent " + tsToFill.getLocation() + " and independent " + tsIndependent.getLocation() + " " + !fillSingle + " have a valid relationship at " + date);
						}
						if (Message.isDebugOn)
						{
							Message.printDebug(dl, routine, "Filling dependent, found nonmissing independant data at " + date + " - value: " + x);
						}
						if (transformation == DataTransformationType.LOG)
						{
							// Need to work on the log of the X value...
							if (x <= 0.0)
							{
								// Use the specified value for small independent
								x = leZeroLogValue2;
							}
							else
							{
								x = Math.Log10(x);
							}
							if (Message.isDebugOn)
							{
								Message.printDebug(dl, routine, "Using log10(independent value): " + x);
							}
						}

						if (fillSingle)
						{
							a = analysisResults.getSingleEquationRegressionResults().getA().Value;
							b = analysisResults.getSingleEquationRegressionResults().getB().Value;
							r = analysisResults.getSingleEquationRegressionResults().getCorrelationCoefficient().Value;
							n1 = analysisData.getSingleEquationRegressionData().getN1();
							xbar = analysisData.getSingleEquationRegressionData().getMeanX1().Value;
							var = Math.Pow(analysisData.getSingleEquationRegressionData().getStandardDeviationX1(),2);
							see = errors.getSingleEquationRegressionErrors().getStandardErrorOfEstimate();
						}
						else
						{
							a = analysisResults.getMonthlyEquationRegressionResults(month).getA().Value;
							b = analysisResults.getMonthlyEquationRegressionResults(month).getB().Value;
							r = analysisResults.getMonthlyEquationRegressionResults(month).getCorrelationCoefficient().Value;
							n1 = analysisData.getMonthlyEquationRegressionData(month).getN1();
							xbar = analysisData.getMonthlyEquationRegressionData(month).getMeanX1().Value;
							var = Math.Pow(analysisData.getMonthlyEquationRegressionData(month).getStandardDeviationX1(), 2);
							see = errors.getMonthlyEquationRegressionErrors(month).getStandardErrorOfEstimate();
						}
						newval = a + b * x;

						if (Message.isDebugOn)
						{
							Message.printDebug(dl, routine, "Calculated dependent value a + b*x=" + a + "+" + b + "*" + x + "=" + newval);
						}

						if (transformation == DataTransformationType.LOG)
						{
							// Now convert Y back from log10 space...
							if (Message.isDebugOn)
							{
								Message.printDebug(dl, routine, "Calculated untransformed dependent value pow(10," + newval + ")=" + Math.Pow(10.0,newval));
							}
							newval = Math.Pow(10.0,newval);
						}


						if (__bestFit == BestFitIndicatorType.SEP)
						{
							double? newSEP = see * Math.Sqrt(1 + (1.0 / n1) + (Math.Pow((x - xbar),2) / (n1 * var)));
							newSEP *= 2.3026; //not sure why we're doing this, but it is in the older program

							//consistent comparisons between transformed and untransformed....
							if (transformation == DataTransformationType.NONE && newSEP != 0)
							{
								newSEP = Math.Log10(newSEP);
							}

							//SEP has to exist and be smaller than a certain size....
							if (newSEP != null && newSEP != Double.NaN && (newSEP * newSEP - 1) <= 10 && (Math.Exp(newSEP * newSEP) - 1) >= 0)
							{
								//get percentage error, not raw error
								newSEP = 100 * Math.Sqrt(Math.Exp(newSEP * newSEP) - 1);
								if (newSEP != null && newSEP != Double.NaN && newSEP <= SEP)
								{
									SEP = newSEP;
								}
								else
								{
									//this value is worse than previous, don't fill, just keep going....
									continue;
								}
							}
							else
							{
								//SEP wasn't OK, go around and try again
								if (Message.isDebugOn)
								{
									Message.printDebug(2, routine, "SEP unacceptably high: " + newSEP);
								}
								continue;
							}
						}
						else if (__bestFit == BestFitIndicatorType.R)
						{
							if (r >= R.Value)
							{
								R = r;
							}
							else
							{
								//new r worse than old, go around and try again
								continue;
							}
						}

						//fill with this one....
						//we're assuming that if it shouldn't be filled, this code won't be called in the first place
						if (fillFlag_boolean)
						{

							string fillFlag2 = fillFlag;
							if ((!string.ReferenceEquals(fillFlag, null)) && fillFlag.Equals("auto", StringComparison.OrdinalIgnoreCase))
							{
								// Use the found relationship for the fill flag
								int equations;
								if (fillSingle)
								{
									equations = 1;
								}
								else
								{
									equations = 12;
								}
								//with how we're sorting, always filling with best available
								fillFlag2 = determineFillFlag(equations, month, 1);
							}
							else if (fillFlag.Equals("i", StringComparison.OrdinalIgnoreCase))
							{
								//note which time series exactly is being used to fill
								if (fillSingle)
								{
									fillFlag2 = "" + analysis.getIndependentTS().getLocation();
								}
								else
								{
									fillFlag2 = TimeUtil.monthAbbreviation(month) + analysis.getIndependentTS().getLocation();
								}
							}
							// Set the flag...
							tsToFill.setDataValue(date, newval, fillFlag2, 1);
						}
						else
						{
							// No flag...
							tsToFill.setDataValue(date, newval);
						}

						// Increment the counter on the number of values filled
						if (fillSingle)
						{
							singleEquationFilledValues[fillCountSingle] = newval;
							if (firstTime)
							{
								firstTime = false;
								++fillCountSingle;
							}
						}
						else
						{
							monthlyEquationFilledValues[month - 1][fillCountMonthly[month - 1]] = newval;
							if (firstTime)
							{
								firstTime = false;
								++fillCountMonthly[month - 1];
							}
						}


					 // Fill in the genesis information...

						if (fillCountSingle > 0)
						{
							tsToFill.addToGenesis("Filled " + fillCountSingle + " missing values " + fillStart + " to " + fillEnd + " using analysis results:");

							// The following comes back as multiple strings but to handle genesis
							// information nicely, break into separate strings...

							IList<string> strings = StringUtil.breakStringList(analysis.ToString(), System.getProperty("line.separator"), StringUtil.DELIM_SKIP_BLANKS);
							foreach (string @string in strings)
							{
								tsToFill.addToGenesis(@string);
							}

							string descriptionString = getDescriptionString();
							if (!string.ReferenceEquals(descriptionString, null))
							{
								// Description has been specified...
								tsToFill.setDescription(tsToFill.getDescription() + descriptionString);
							}
							else
							{
								// Automatically add to the description...
								string monthString = "";
								if (analysis.getAnalyzeMonthlyEquations())
								{
									monthString = " monthly";
								}
								if ((__analysisMonths != null) && (__analysisMonths.Length == 1))
								{
									// Filling one month so be specific in the description
									monthString = " " + TimeUtil.monthAbbreviation(__analysisMonths[0]);
								}
								if (transformation == DataTransformationType.LOG)
								{
									if (!tsToFill.getDescription().Contains(tsIndependent.getIdentifierString()))
									{
										//if it's already been set, don't set it again
										tsToFill.setDescription(tsToFill.getDescription() + ", fill log " + __analysisMethod + monthString + " using " + tsIndependent.getIdentifierString());
									}
								}
								else
								{
									if (!tsToFill.getDescription().Contains(tsIndependent.getAlias()))
									{
										//if it's already been set, don't set it again
										tsToFill.setDescription(tsToFill.getDescription() + ", fill " + __analysisMethod + monthString + " using " + tsIndependent.getAlias());
									}
								}
							}
						}
						if (fillCountMonthly[month - 1] > 0)
						{
							tsToFill.addToGenesis("Filled " + fillCountMonthly[month - 1] + " missing values " + fillStart + " to " + fillEnd + " using analysis results:");

							// The following comes back as multiple strings but to handle genesis
							// information nicely, break into separate strings...

							IList<string> strings = StringUtil.breakStringList(analysis.ToString(), System.getProperty("line.separator"), StringUtil.DELIM_SKIP_BLANKS);
							foreach (string @string in strings)
							{
								tsToFill.addToGenesis(@string);
							}

							string descriptionString = getDescriptionString();
							if (!string.ReferenceEquals(descriptionString, null))
							{
								// Description has been specified...
								tsToFill.setDescription(tsToFill.getDescription() + descriptionString);
							}
							else
							{
								// Automatically add to the description...
								string monthString = "";
								if (analysis.getAnalyzeMonthlyEquations())
								{
									monthString = " monthly";
								}
								if ((__analysisMonths != null) && (__analysisMonths.Length == 1))
								{
									// Filling one month so be specific in the description
									monthString = " " + TimeUtil.monthAbbreviation(__analysisMonths[0]);
								}
								if (transformation == DataTransformationType.LOG)
								{
									if (!tsToFill.getDescription().Contains(tsIndependent.getIdentifierString()))
									{
										//if it's already been set, don't set it again
										tsToFill.setDescription(tsToFill.getDescription() + ", fill log " + __analysisMethod + monthString + " using " + tsIndependent.getIdentifierString());
									}
								}
								else
								{
									if (!tsToFill.getDescription().Contains(tsIndependent.getAlias()))
									{
										//if it's already been set, don't set it again
										tsToFill.setDescription(tsToFill.getDescription() + ", fill " + __analysisMethod + monthString + " using " + tsIndependent.getAlias());
									}
								}
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				// Error filling interval - this likely is a logic or error-handling issue and needs
				// to be resolved
				problems.Add("Error filling value at " + date + " uniquetempvar.");
				if (errorCount++ < 100)
				{
					Message.printWarning(3, routine, e);
				}
			}
		}

		// TODO SAM 2012-05-21 Figure out what to do with transformed values
		// Set the filled values arrays.  Resize based on the actual number of filled values.
		double[] singleEquationFilledValues2 = new double[0];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: double[][] monthlyEquationFilledValues2 = new double[12][0];
		double[][] monthlyEquationFilledValues2 = RectangularArrays.RectangularDoubleArray(12, 0);
		RegressionFilledValues[] monthlyRegressionFilledValues = new RegressionFilledValues[12];
		singleEquationFilledValues2 = new double[fillCountSingle];
		Array.Copy(singleEquationFilledValues, 0, singleEquationFilledValues2, 0, fillCountSingle);
		for (int i = 0; i < 12; i++)
		{
			monthlyEquationFilledValues2[i] = new double[fillCountMonthly[i]];
			Array.Copy(monthlyEquationFilledValues[i], 0, monthlyEquationFilledValues2[i], 0, fillCountMonthly[i]);
			monthlyRegressionFilledValues[i] = new RegressionFilledValues(monthlyEquationFilledValues2[i]);
		}
		setTSRegressionFilledValues(new TSRegressionFilledValues(tsToFill, new RegressionFilledValues(singleEquationFilledValues2), monthlyRegressionFilledValues));
	}

	/// <summary>
	/// Return the analysis method. </summary>
	/// <returns> the analysis method. </returns>
	private RegressionType getAnalysisMethod()
	{
		return __analysisMethod;
	}

	/// <summary>
	/// Return an array indicating the months to be analyzed, each value 1-12.  This information
	/// corresponds to the AnalysisMonth property that is passed in at construction. </summary>
	/// <returns> the array containing the months (1-12) to be analyzed, or null if all months will be analyzed. </returns>
	public virtual int [] getAnalysisMonths()
	{
		return __analysisMonths;
	}

	/// <summary>
	/// Return the dependent time series analysis end. </summary>
	/// <returns> the dependent time series analysis end. </returns>
	public virtual DateTime getDependentAnalysisEnd()
	{
		return __dependentAnalysisEnd;
	}

	/// <summary>
	/// Return the dependent time series analysis start. </summary>
	/// <returns> the dependent time series analysis start. </returns>
	public virtual DateTime getDependentAnalysisStart()
	{
		return __dependentAnalysisStart;
	}

	/// <summary>
	/// Return the description string.
	/// </summary>
	public virtual string getDescriptionString()
	{
		return __descriptionString;
	}

	/// <summary>
	/// Return whether to fill. </summary>
	/// <returns> whether to fill. </returns>
	public virtual bool getDoFill()
	{
		return __doFill;
	}

	/// <summary>
	/// Return the fill end. </summary>
	/// <returns> the fill end. </returns>
	public virtual DateTime getFillEnd()
	{
		return __fillEnd;
	}

	/// <summary>
	/// Return the fill flag. </summary>
	/// <returns> the fill flag </returns>
	public virtual string getFillFlag()
	{
		return __fillFlag;
	}

	/// <summary>
	/// Return the fill flag description. </summary>
	/// <returns> the fill flag description </returns>
	public virtual string getFillFlagDesc()
	{
		return __fillFlagDesc;
	}

	/// <summary>
	/// Return the independent time series analysis start. </summary>
	/// <returns> the independent time series analysis start. </returns>
	public virtual DateTime getFillStart()
	{
		return __fillStart;
	}

	/// <summary>
	/// Return the forced intercept. </summary>
	/// <returns> the forced intercept. </returns>
	public virtual double? getForcedIntercept()
	{
		return __forcedIntercept;
	}

	/// <summary>
	/// Return the independent time series analysis end. </summary>
	/// <returns> the independent time series analysis end. </returns>
	public virtual DateTime getIndependentAnalysisEnd()
	{
		return __independentAnalysisEnd;
	}

	/// <summary>
	/// Return the independent time series analysis start. </summary>
	/// <returns> the independent time series analysis start. </returns>
	public virtual DateTime getIndependentAnalysisStart()
	{
		return __independentAnalysisStart;
	}

	/// <summary>
	/// Return the value that will be used for the log transform if the original is <= 0. </summary>
	/// <returns> the value that will be used for the log transform if the original is <= 0. </returns>
	private double getLEZeroLogValue()
	{
		return __leZeroLogValue.Value;
	}

	/// <summary>
	/// Return the minimum acceptable R - if null then R is not checked. </summary>
	/// <returns> the minimum acceptable R. </returns>
	public virtual double? getMinimumR()
	{
		return __minimumR;
	}

	/// <summary>
	/// Return the minimum acceptable sample size - if null then sample size is not checked. </summary>
	/// <returns> the minimum acceptable sample size. </returns>
	public virtual int? getMinimumSampleSize()
	{
		return __minimumSampleSize;
	}

	/// <summary>
	/// Indicate whether the analysis is performed using one equation or monthly equations. </summary>
	/// <returns> the number of equations used for the analysis. </returns>
	public virtual NumberOfEquationsType getNumberOfEquations()
	{
		return __numberOfEquations;
	}

	/// <summary>
	/// Return a list of problems for the time series. </summary>
	/// <returns> a list of problems for the time series </returns>
	public virtual IList<string> getProblems()
	{
		return __problems;
	}

	/// <summary>
	/// Return the TSRegressionFilledValues object. </summary>
	/// <returns> the TSRegressionFilledValues object. </returns>
	public virtual TSRegressionFilledValues getTSRegressionFilledValues()
	{
		return __tsRegressionFilledValues;
	}

	/// <summary>
	/// Return the time series to fill (Y). </summary>
	/// <returns> the time series to fill (Y). </returns>
	public virtual TS getTSToFill()
	{
		return __tsToFill;
	}

	/// <summary>
	/// Save the statistics from the regression analysis to a table object. </summary>
	/// <param name="ts"> dependent time series </param>
	/// <param name="table"> the table to save the results </param>
	/// <param name="tableTSIDColumnName"> the name of the table column containing the TSID/alias to match </param>
	/// <param name="tableTSIDFormat"> the format string for the time series - to allow matching the contents of the
	/// tableTSIDColumnName </param>
	/// <param name="regressionType"> the regression type, which will impact formatting of results (independent analysis period
	/// is specific to MOVE2) </param>
	/// <param name="numberOfEquations"> the number of equations in the analysis, which will impact the number of columns
	/// in the output (whether or not monthly statistics are shown) </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void saveStatisticsToTable(TS ts, RTi.Util.Table.DataTable table, String tableTSIDColumnName, String tableTSIDFormat, RTi.Util.Math.RegressionType regressionType, RTi.Util.Math.NumberOfEquationsType numberOfEquations) throws Exception
	public virtual void saveStatisticsToTable(TS ts, DataTable table, string tableTSIDColumnName, string tableTSIDFormat, RegressionType regressionType, NumberOfEquationsType numberOfEquations)
	{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		string routine = this.GetType().FullName + ".saveStatisticsToTable";
		// Verify that the TSID table columns are available for dependent and independent time series
		string tableTSIDColumnNameIndependent = tableTSIDColumnName + "_Independent";
		int tableTSIDColumnNumber = -1;
		int tableTSIDColumnNumberIndependent = -1;
		// If the TSID column name does not exist, add it to the table
		try
		{
			tableTSIDColumnNumber = table.getFieldIndex(tableTSIDColumnName);
		}
		catch (Exception)
		{
			// Automatically add to the table, initialize with null (not nonValue)
			table.addField(new TableField(TableField.DATA_TYPE_STRING,tableTSIDColumnName,-1,-1), null);
			// Get the corresponding column number for row-edits below
			tableTSIDColumnNumber = table.getFieldIndex(tableTSIDColumnName);
			// Set the description, which is used as the tool tip in the UI
			table.getTableField(tableTSIDColumnNumber).setDescription("Dependent time series identifier");
		}
		// If the TSID_Independent column does not exist, add it to the table
		try
		{
			tableTSIDColumnNumberIndependent = table.getFieldIndex(tableTSIDColumnNameIndependent);
		}
		catch (Exception)
		{
			// Automatically add to the table, initialize with null (not nonValue)
			table.addField(new TableField(TableField.DATA_TYPE_STRING,tableTSIDColumnNameIndependent,-1,-1), null);
			// Get the corresponding column number for row-edits below
			tableTSIDColumnNumberIndependent = table.getFieldIndex(tableTSIDColumnNameIndependent);
			// Set the description, which is used as the tool tip in the UI
			table.getTableField(tableTSIDColumnNumberIndependent).setDescription("Independent time series identifier");
		}
		// Add additional generic columns, for informational purposes - put these here to avoid
		// duplication for each month if monthly analysis
		IList<string> columnsToAdd = new List<string>();
		IList<string> descriptionsToAdd = new List<string>();
		columnsToAdd.Add("AnalysisMethod");
		descriptionsToAdd.Add("Regression analysis method (Ordinary Least Squares or Maintenance of Variation 2)");
		columnsToAdd.Add("DependentAnalysisStart");
		descriptionsToAdd.Add("Dependent time series analysis period start, used to determine relationships");
		columnsToAdd.Add("DependentAnalysisEnd");
		descriptionsToAdd.Add("Dependent time series analysis period end, used to determine relationships");
		// TODO SAM 2012-05-24 Evaluate whether the following 2 are confusing for normal OLS regression
		columnsToAdd.Add("IndependentAnalysisStart");
		columnsToAdd.Add("IndependentAnalysisEnd");
		if (regressionType == RegressionType.OLS_REGRESSION)
		{
			descriptionsToAdd.Add("Independent time series analysis period start (same as DependentAnalysisStart for OLS regression), used to determine relationships");
			descriptionsToAdd.Add("Independent time series analysis period end (same as DependentAnalysisStart for OLS regression), used to determine relationships");
		}
		else if (regressionType == RegressionType.MOVE2)
		{
			descriptionsToAdd.Add("Independent time series analysis period start, used to determine relationships");
			descriptionsToAdd.Add("Independent time series analysis period end, used to determine relationships");
		}
		else
		{
			descriptionsToAdd.Add("");
			descriptionsToAdd.Add("");
		}
		columnsToAdd.Add("FillStart");
		descriptionsToAdd.Add("Fill period start");
		columnsToAdd.Add("FillEnd");
		descriptionsToAdd.Add("Fill period end");
		columnsToAdd.Add("Transformation");
		descriptionsToAdd.Add("Transformation performed on data prior to determining relationship");
		columnsToAdd.Add("MinimumSampleSize");
		descriptionsToAdd.Add("Minimum sample size (N1) to consider the relationship valid");
		columnsToAdd.Add("MinimumR");
		descriptionsToAdd.Add("Minimum R to consider the relationship valid");
		columnsToAdd.Add("ConfidenceInterval");
		descriptionsToAdd.Add("Confidence interval (%) required for relationship line slope");
		for (int i = 0; i < columnsToAdd.Count; i++)
		{
			try
			{
				// See if column is already in the table
				table.getFieldIndex(columnsToAdd[i]);
			}
			catch (Exception)
			{
				// Automatically add to the table, initialize with null (not nonValue)
				if (columnsToAdd[i].Equals("ConfidenceInterval", StringComparison.OrdinalIgnoreCase) || columnsToAdd[i].Equals("MinimumR", StringComparison.OrdinalIgnoreCase))
				{
					// Floating point
					table.addField(new TableField(TableField.DATA_TYPE_DOUBLE,columnsToAdd[i],-1,8), null);
				}
				else if (columnsToAdd[i].Equals("MinimumSampleSize", StringComparison.OrdinalIgnoreCase))
				{
					// Integer
					table.addField(new TableField(TableField.DATA_TYPE_INT,columnsToAdd[i],-1,-1), null);
				}
				else
				{
					// Rest are strings
					table.addField(new TableField(TableField.DATA_TYPE_STRING,columnsToAdd[i],-1,-1), null);
				}
				// Set the description, which is used as the tool tip in the UI
				table.getTableField(table.getFieldIndex(columnsToAdd[i])).setDescription(descriptionsToAdd[i]);
			}
		}
		// Loop through the statistics, creating table column names if necessary
		// Do this first so that all columns are fully defined.  Then process the row values below.
		int numEquations = 1;
		if (numberOfEquations == NumberOfEquationsType.MONTHLY_EQUATIONS)
		{
			numEquations = 12;
		}
		// List in a reasonable order - see the command documentation for more
		// X=independent
		// Y=dependent
		IList<string> statsToOutput = new List<string>();
		// Statistics that have _trans are for transformed data and will only be output if a transformation
		// has been specified
		// _trans means in units of log10(original units), therefore unitless quantities will not have _trans tags
		// mean, standard deviation, etc. probably aren't needed by end user, but are helpful along the way, so they'll be outputted for now

		//need to get statistics from every combination
		foreach (TSRegressionAnalysis analysis in __tsRegressionAnalysisList)
		{
			DataTransformationType transformation = analysis.getTransformation();
			// Statistics from the input data...
			statsToOutput.Add("N1"); // Same for raw and transformed
			statsToOutput.Add("MeanX1");
			if (transformation != DataTransformationType.NONE)
			{
				statsToOutput.Add("MeanX1_trans");
			}
			statsToOutput.Add("SX1");
			if (transformation != DataTransformationType.NONE)
			{
				statsToOutput.Add("SX1_trans");
			}
			statsToOutput.Add("N2");
			statsToOutput.Add("MeanX2");
			if (transformation != DataTransformationType.NONE)
			{
				statsToOutput.Add("MeanX2_trans");
			}
			statsToOutput.Add("SX2");
			if (transformation != DataTransformationType.NONE)
			{
				statsToOutput.Add("SX2_trans");
			}
			statsToOutput.Add("MeanY1");
			if (transformation != DataTransformationType.NONE)
			{
				statsToOutput.Add("MeanY1_trans");
			}
			statsToOutput.Add("SY1");
			if (transformation != DataTransformationType.NONE)
			{
				statsToOutput.Add("SY1_trans");
			}
			statsToOutput.Add("NY");
			statsToOutput.Add("MeanY");
			if (transformation != DataTransformationType.NONE)
			{
				statsToOutput.Add("MeanY_trans");
			}
			statsToOutput.Add("SY");
			if (transformation != DataTransformationType.NONE)
			{
				statsToOutput.Add("SY_trans");
			};
			statsToOutput.Add("SkewY");
			if (transformation != DataTransformationType.NONE)
			{
				statsToOutput.Add("SkewY_trans");
			};
			// Statistics from the analysis results...
			if (transformation == DataTransformationType.NONE)
			{
				statsToOutput.Add("a");
				statsToOutput.Add("b");
				statsToOutput.Add("R");
				statsToOutput.Add("R2");
			}
			else
			{
				statsToOutput.Add("a_trans");
				statsToOutput.Add("b_trans");
				//does R really have units?
				statsToOutput.Add("R_trans");
				statsToOutput.Add("R2_trans");
			}
			// Statistics from the error estimates...
			statsToOutput.Add("MeanY1est");
			if (transformation != DataTransformationType.NONE)
			{
				statsToOutput.Add("MeanY1est_trans");
			}
			statsToOutput.Add("SY1est");
			if (transformation != DataTransformationType.NONE)
			{
				statsToOutput.Add("SY1est_trans");
			}
			statsToOutput.Add("RMSE");
			if (transformation != DataTransformationType.NONE)
			{
				statsToOutput.Add("RMSE_trans");
			}
			statsToOutput.Add("SEE");
			if (transformation != DataTransformationType.NONE)
			{
				statsToOutput.Add("SEE_trans");
			}

			//unitless quantities don't have _trans variants
			statsToOutput.Add("SESlope");
			statsToOutput.Add("TestScore");
			statsToOutput.Add("TestQuantile");
			statsToOutput.Add("TestOK");
			statsToOutput.Add("SampleSizeOK");
			statsToOutput.Add("ROK");

			// Statistics from the filled data, always in data space
			// (include for comparison with mixed station analysis)...
			statsToOutput.Add("NYfilled");
			statsToOutput.Add("MeanYfilled");
			statsToOutput.Add("SYfilled");
			statsToOutput.Add("SkewYfilled");

			// The following comments parallel the statistics names are used to create comments in the table header
			string[] mainComments = new string[] {"", "The following statistics are computed to determine and evaluate the the regression relationships.", "The regression type performed was:  " + getAnalysisMethod(), "X indicates the independent time series and Y indicates the dependent time series.", "Some statistics are ignored for some regression approaches, but are provided for comparison.", ""};
			string[] mainCommentsMonthly = new string[] {"", "Monthly statistics (for case where NumberOfEquations=MonthlyEquatations) will have a _M subscript, " + "where M is the month (1=January, 12=December).", ""};
			Dictionary<string, string> statisticComments = new Dictionary<string, string>();
			statisticComments["N1"] = "Count of non-missing data points overlapping in the dependent and independent time series in the analysis period";
			statisticComments["MeanX1"] = "Mean of the independent N1 values";
			statisticComments["SX1"] = "Standard deviation of the independent N1 values";
			statisticComments["N2"] = "Count of the non-missing data points in the independent time series outside of N1 in the analysis period";
			statisticComments["MeanX2"] = "Mean of the independent N2 values";
			statisticComments["SX2"] = "Standard deviation of the independent N2 values";
			statisticComments["MeanY1"] = "Mean of the dependent N1 values";
			statisticComments["SY1"] = "Standard deviation of the dependent N1 values";
			statisticComments["NY"] = "Count of the non-missing dependent values in the analysis period";
			statisticComments["MeanY"] = "Mean of the NY dependent values in the analysis period";
			statisticComments["SY"] = "Standard deviation of the NY dependent values in the analysis period";
			statisticComments["a"] = "The intercept for the relationship equation";
			statisticComments["b"] = "The slope of the relationship equation";
			statisticComments["R"] = "The correlation coefficient for N1 values";
			statisticComments["R2"] = "R-squared, coefficient of determination for N1 values";
			statisticComments["MeanY1est"] = "Mean of N1 values computed from the relationship (estimate dependent N1 values where previously known)";
			statisticComments["SY1est"] = "Standard deviation of N1 values computed from the relationship (estimate dependent N1 values where previously known";
			statisticComments["RMSE"] = "Root mean squared error for N1 values, computed from regression relationship estimated values";
			statisticComments["SEE"] = "Standard error of estimate for N1 values, computed from regression relationship estimated values";
			statisticComments["SEP"] = "Standard error of prediction for N1 values, computed from regression relationship estimated values";
			statisticComments["SEslope"] = "Standard error (SE) of the slope (b) for N1 values, computed from regression relationship estimated values";
			statisticComments["TestScore"] = "b/SE";
			statisticComments["TestQuantile"] = "From the Student's T-test, function of confidence interval and degrees of freedom, DF (N1 - 2)";
			statisticComments["TestOK"] = "Yes if TestScore >= TestQuantile, No if otherwise";
			statisticComments["SampleSizeOK"] = "Yes if sample size >= minimum sample size, No if otherwise";
			statisticComments["ROK"] = "Yes if R >= minimum R, No if otherwise";
			// Put these in for comparison with mixed station analysis
			statisticComments["NYfilled"] = "Number of dependent values filled in the fill period";
			statisticComments["MeanYfilled"] = "Mean of the filled values";
			statisticComments["SYfilled"] = "Standard deviation of the filled values";
			statisticComments["SkewYfilled"] = "Skew of the filled values";
			// Add comments to the table header
			table.addToComments(Arrays.asList(mainComments));
			if (numberOfEquations == NumberOfEquationsType.MONTHLY_EQUATIONS)
			{
				table.addToComments(Arrays.asList(mainCommentsMonthly));
			}
			// FIXME SAM 2012-01-16 Need to add statistics comments
			//int i = 0;
			//for ( String comment: statisticComments ) {
			//    table.addToComments(statistics[i++] + " - " + comment);
			//}
			table.addToComments("");
			int countStatisticTotal = statsToOutput.Count * numEquations; // The total number of statistics columns to add
			string[] statisticColumnNames = new string[countStatisticTotal]; // names in table
			int[] statisticFieldType = new int[countStatisticTotal]; // value types
			// Arrays for the statistics.  Using multiple arrays will result in some statistic
			// values being null; however this is easier than dealing with casts later in the code
			double?[] statisticValueDouble = new double?[countStatisticTotal];
			int?[] statisticValueInteger = new int?[countStatisticTotal];
			string[] statisticValueString = new string[countStatisticTotal];

			// The count of statistics added (0-index), necessary because when dealing with monthly statistics
			// the 12 months are flattened into a linear array matching column headings
			//needs to be reset every analysis cycle...
			int countStatistic = -1;

			// Get the main sub-objects associated with the analysis
			TSRegressionData tsRegressionData = analysis.getTSRegressionData();
			TSRegressionData tsRegressionDataTransformed = analysis.getTSRegressionDataTransformed();
			TSRegressionResults tsRegressionResults = analysis.getTSRegressionResults();
			TSRegressionResults tsRegressionResultsTransformed = analysis.getTSRegressionResultsTransformed();
			TSRegressionEstimateErrors tsRegressionEstimateErrors = analysis.getTSRegressionEstimateErrors();
			TSRegressionEstimateErrors tsRegressionEstimateErrorsTransformed = analysis.getTSRegressionErrorsTransformed();
			TSRegressionChecks tsRegressionChecks = analysis.getTSRegressionChecksTransformed();
			TSRegressionFilledValues tsRegressionFilledValues = getTSRegressionFilledValues();
			string statisticName; // Statistic to output
			for (int iEquation = 1; iEquation <= numEquations; iEquation++)
			{
				for (int iStatistic = 0; iStatistic < statsToOutput.Count; iStatistic++)
				{
					statisticName = statsToOutput[iStatistic];
					try
					{
						// Set statistics to null (one will be set below).
						++countStatistic;
						statisticValueDouble[countStatistic] = null;
						statisticValueInteger[countStatistic] = null;
						// Column name for the statistic (list alphabetically)...
						if (numEquations == 1)
						{
							statisticColumnNames[countStatistic] = statisticName;
						}
						else
						{
							statisticColumnNames[countStatistic] = statisticName + "_" + iEquation;
						}
						//these are essentially a switch statement, grabbing the right statistic based on the name
						//TODO 7-29-2013: Java 7 can use a switch statement for strings
						//so when that switch is done, this should be changed!
						if (statisticName.Equals("a"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								statisticValueDouble[countStatistic] = tsRegressionResults.getSingleEquationRegressionResults().getA();
							}
							else
							{
								statisticValueDouble[countStatistic] = tsRegressionResults.getMonthlyEquationRegressionResults(iEquation).getA();
							}
						}
						else if (statisticName.Equals("AnalysisStart"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								statisticValueDouble[countStatistic] = tsRegressionResults.getSingleEquationRegressionResults().getA();
							}
							else
							{
								statisticValueDouble[countStatistic] = tsRegressionResults.getMonthlyEquationRegressionResults(iEquation).getA();
							}
						}
						else if (statisticName.Equals("a_trans"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								statisticValueDouble[countStatistic] = tsRegressionResultsTransformed.getSingleEquationRegressionResults().getA();
							}
							else
							{
								statisticValueDouble[countStatistic] = tsRegressionResultsTransformed.getMonthlyEquationRegressionResults(iEquation).getA();
							}
						}
						else if (statisticName.Equals("b"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								statisticValueDouble[countStatistic] = tsRegressionResults.getSingleEquationRegressionResults().getB();
							}
							else
							{
								statisticValueDouble[countStatistic] = tsRegressionResults.getMonthlyEquationRegressionResults(iEquation).getB();
							}
						}
						else if (statisticName.Equals("b_trans"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								statisticValueDouble[countStatistic] = tsRegressionResultsTransformed.getSingleEquationRegressionResults().getB();
							}
							else
							{
								statisticValueDouble[countStatistic] = tsRegressionResultsTransformed.getMonthlyEquationRegressionResults(iEquation).getB();
							}
						}
						else if (statisticName.Equals("MeanX"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								statisticValueDouble[countStatistic] = tsRegressionData.getSingleEquationRegressionData().getMeanX();
							}
							else
							{
								statisticValueDouble[countStatistic] = tsRegressionData.getMonthlyEquationRegressionData(iEquation).getMeanX();
							}
						}
						else if (statisticName.Equals("MeanX1"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								statisticValueDouble[countStatistic] = tsRegressionData.getSingleEquationRegressionData().getMeanX1();
							}
							else
							{
								statisticValueDouble[countStatistic] = tsRegressionData.getMonthlyEquationRegressionData(iEquation).getMeanX1();
							}
						}
						else if (statisticName.Equals("MeanX1_trans"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								statisticValueDouble[countStatistic] = tsRegressionDataTransformed.getSingleEquationRegressionData().getMeanX1();
							}
							else
							{
								statisticValueDouble[countStatistic] = tsRegressionDataTransformed.getMonthlyEquationRegressionData(iEquation).getMeanX1();
							}
						}
						else if (statisticName.Equals("MeanX2"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								if (tsRegressionData.getSingleEquationRegressionData().getN2() == 0)
								{
									statisticValueDouble[countStatistic] = null;
								}
								else
								{
									statisticValueDouble[countStatistic] = tsRegressionData.getSingleEquationRegressionData().getMeanX2();
								}
							}
							else
							{
								if (tsRegressionData.getMonthlyEquationRegressionData(iEquation).getN2() == 0)
								{
									statisticValueDouble[countStatistic] = null;
								}
								else
								{
									statisticValueDouble[countStatistic] = tsRegressionData.getMonthlyEquationRegressionData(iEquation).getMeanX2();
								}
							}
						}
						else if (statisticName.Equals("MeanX2_trans"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								if (tsRegressionDataTransformed.getSingleEquationRegressionData().getN2() == 0)
								{
									statisticValueDouble[countStatistic] = null;
								}
								else
								{
									statisticValueDouble[countStatistic] = tsRegressionDataTransformed.getSingleEquationRegressionData().getMeanX2();
								}
							}
							else
							{
								if (tsRegressionDataTransformed.getMonthlyEquationRegressionData(iEquation).getN2() == 0)
								{
									statisticValueDouble[countStatistic] = null;
								}
								else
								{
									statisticValueDouble[countStatistic] = tsRegressionDataTransformed.getMonthlyEquationRegressionData(iEquation).getMeanX2();
								}
							}
						}
						else if (statisticName.Equals("MeanY"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								statisticValueDouble[countStatistic] = tsRegressionData.getSingleEquationRegressionData().getMeanY();
							}
							else
							{
								statisticValueDouble[countStatistic] = tsRegressionData.getMonthlyEquationRegressionData(iEquation).getMeanY();
							}
						}
						else if (statisticName.Equals("MeanY_trans"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								statisticValueDouble[countStatistic] = tsRegressionDataTransformed.getSingleEquationRegressionData().getMeanY();
							}
							else
							{
								statisticValueDouble[countStatistic] = tsRegressionDataTransformed.getMonthlyEquationRegressionData(iEquation).getMeanY();
							}
						}
						else if (statisticName.Equals("MeanYfilled"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								statisticValueDouble[countStatistic] = tsRegressionFilledValues.getSingleEquationRegressionFilledValues().getMeanYFilled();
							}
							else
							{
								statisticValueDouble[countStatistic] = tsRegressionFilledValues.getMonthlyEquationRegressionFilledValues(iEquation).getMeanYFilled();
							}
						}
						else if (statisticName.Equals("MeanY1"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								statisticValueDouble[countStatistic] = tsRegressionData.getSingleEquationRegressionData().getMeanY1();
							}
							else
							{
								statisticValueDouble[countStatistic] = tsRegressionData.getMonthlyEquationRegressionData(iEquation).getMeanY1();
							}
						}
						else if (statisticName.Equals("MeanY1_trans"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								statisticValueDouble[countStatistic] = tsRegressionDataTransformed.getSingleEquationRegressionData().getMeanY1();
							}
							else
							{
								statisticValueDouble[countStatistic] = tsRegressionDataTransformed.getMonthlyEquationRegressionData(iEquation).getMeanY1();
							}
						}
						else if (statisticName.Equals("MeanY1est"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
//JAVA TO C# CONVERTER TODO TASK: The following line could not be converted:
								statisticValueDouble[countStatistic] = tsRegressionEstimateErrors.getSingleEquationRegressionErrors().getMeanY1est();
							}
							else
							{
//JAVA TO C# CONVERTER TODO TASK: The following line could not be converted:
								statisticValueDouble[countStatistic] = tsRegressionEstimateErrors.getMonthlyEquationRegressionErrors(iEquation).getMeanY1est();
							}
						}
						else if (statisticName.Equals("MeanY1est_trans"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
//JAVA TO C# CONVERTER TODO TASK: The following line could not be converted:
								statisticValueDouble[countStatistic] = tsRegressionEstimateErrorsTransformed.getSingleEquationRegressionErrors().getMeanY1est();
							}
							else
							{
//JAVA TO C# CONVERTER TODO TASK: The following line could not be converted:
								statisticValueDouble[countStatistic] = tsRegressionEstimateErrorsTransformed.getMonthlyEquationRegressionErrors(iEquation).getMeanY1est();
							}
						}
						else if (statisticName.Equals("NX"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_INT;
							if (numEquations == 1)
							{
								statisticValueInteger[countStatistic] = tsRegressionData.getSingleEquationRegressionData().getN();
							}
							else
							{
								statisticValueInteger[countStatistic] = tsRegressionData.getMonthlyEquationRegressionData(iEquation).getN();
							}
						}
						else if (statisticName.Equals("N1"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_INT;
							if (numEquations == 1)
							{
								statisticValueInteger[countStatistic] = tsRegressionData.getSingleEquationRegressionData().getN1();
							}
							else
							{
								statisticValueInteger[countStatistic] = tsRegressionData.getMonthlyEquationRegressionData(iEquation).getN1();
							}
						}
						else if (statisticName.Equals("N2"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_INT;
							if (numEquations == 1)
							{
								statisticValueInteger[countStatistic] = tsRegressionData.getSingleEquationRegressionData().getN2();
							}
							else
							{
								statisticValueInteger[countStatistic] = tsRegressionData.getMonthlyEquationRegressionData(iEquation).getN2();
							}
						}
						else if (statisticName.Equals("NY"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_INT;
							if (numEquations == 1)
							{
								statisticValueInteger[countStatistic] = tsRegressionData.getSingleEquationRegressionData().getNY();
							}
							else
							{
								statisticValueInteger[countStatistic] = tsRegressionData.getMonthlyEquationRegressionData(iEquation).getNY();
							}
						}
						else if (statisticName.Equals("NYfilled"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_INT;
							if (numEquations == 1)
							{
								statisticValueInteger[countStatistic] = tsRegressionFilledValues.getSingleEquationRegressionFilledValues().getNFilled();
							}
							else
							{
								statisticValueInteger[countStatistic] = tsRegressionFilledValues.getMonthlyEquationRegressionFilledValues(iEquation).getNFilled();
							}
						}
						else if (statisticName.Equals("R"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								statisticValueDouble[countStatistic] = tsRegressionResults.getSingleEquationRegressionResults().getCorrelationCoefficient();
							}
							else
							{
								statisticValueDouble[countStatistic] = tsRegressionResults.getMonthlyEquationRegressionResults(iEquation).getCorrelationCoefficient();
							}
						}
						else if (statisticName.Equals("R_trans"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								statisticValueDouble[countStatistic] = tsRegressionResultsTransformed.getSingleEquationRegressionResults().getCorrelationCoefficient();
							}
							else
							{
								statisticValueDouble[countStatistic] = tsRegressionResultsTransformed.getMonthlyEquationRegressionResults(iEquation).getCorrelationCoefficient();
							}
						}
						else if (statisticName.Equals("R2"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							double? r = null;
							if (numEquations == 1)
							{
								r = tsRegressionResults.getSingleEquationRegressionResults().getCorrelationCoefficient();
							}
							else
							{
								r = tsRegressionResults.getMonthlyEquationRegressionResults(iEquation).getCorrelationCoefficient();
							}
							if (r != null)
							{
								double? r2 = new double?(r * r);
								statisticValueDouble[countStatistic] = r2;
							}
							else
							{
								statisticValueDouble[countStatistic] = null;
							}
						}
						else if (statisticName.Equals("R2_trans"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							double? r = null;
							if (numEquations == 1)
							{
								r = tsRegressionResultsTransformed.getSingleEquationRegressionResults().getCorrelationCoefficient();
							}
							else
							{
								r = tsRegressionResultsTransformed.getMonthlyEquationRegressionResults(iEquation).getCorrelationCoefficient();
							}
							if (r != null)
							{
								double? r2 = new double?(r * r);
								statisticValueDouble[countStatistic] = r2;
							}
							else
							{
								statisticValueDouble[countStatistic] = null;
							}
						}
						else if (statisticName.Equals("RMSE"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								statisticValueDouble[countStatistic] = tsRegressionEstimateErrors.getSingleEquationRegressionErrors().getRMSE();
							}
							else
							{
								statisticValueDouble[countStatistic] = tsRegressionEstimateErrors.getMonthlyEquationRegressionErrors(iEquation).getRMSE();
							}
						}
						else if (statisticName.Equals("RMSE_trans"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								statisticValueDouble[countStatistic] = tsRegressionEstimateErrorsTransformed.getSingleEquationRegressionErrors().getRMSE();
							}
							else
							{
								statisticValueDouble[countStatistic] = tsRegressionEstimateErrorsTransformed.getMonthlyEquationRegressionErrors(iEquation).getRMSE();
							}
						}
						else if (statisticName.Equals("ROK"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_STRING;
							// Only have a value if the minimum R has been specified
							if (getMinimumR() == null)
							{
								statisticValueString[countStatistic] = "";
							}
							else
							{
								bool rok = tsRegressionChecks.getSingleEquationRegressionChecks().getIsROK();
								if (numEquations == 12)
								{
									rok = tsRegressionChecks.getMonthlyEquationRegressionChecks(iEquation).getIsROK();
								}
								if (rok)
								{
									statisticValueString[countStatistic] = "Yes";
								}
								else
								{
									statisticValueString[countStatistic] = "No";
								}
							}
						}
						else if (statisticName.Equals("SampleSizeOK"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_STRING;
							// Minimum sample size will always be specified internally so always show
							bool? ssOk = null;
							if (numEquations == 1)
							{
								ssOk = tsRegressionChecks.getSingleEquationRegressionChecks().getIsSampleSizeOK();
							}
							if (numEquations == 12)
							{
								ssOk = tsRegressionChecks.getMonthlyEquationRegressionChecks(iEquation).getIsSampleSizeOK();
							}
							if (ssOk.Value)
							{
								statisticValueString[countStatistic] = "Yes";
							}
							else
							{
								statisticValueString[countStatistic] = "No";
							}
						}
						else if (statisticName.Equals("SEE"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								statisticValueDouble[countStatistic] = tsRegressionEstimateErrors.getSingleEquationRegressionErrors().getStandardErrorOfEstimate();
							}
							else
							{
								statisticValueDouble[countStatistic] = tsRegressionEstimateErrors.getMonthlyEquationRegressionErrors(iEquation).getStandardErrorOfEstimate();
							}
						}
						else if (statisticName.Equals("SEE_trans"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								statisticValueDouble[countStatistic] = tsRegressionEstimateErrorsTransformed.getSingleEquationRegressionErrors().getStandardErrorOfEstimate();
							}
							else
							{
								statisticValueDouble[countStatistic] = tsRegressionEstimateErrorsTransformed.getMonthlyEquationRegressionErrors(iEquation).getStandardErrorOfEstimate();
							}
						}
						else if (statisticName.Equals("SESlope"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								statisticValueDouble[countStatistic] = tsRegressionEstimateErrors.getSingleEquationRegressionErrors().getStandardErrorOfSlope();
							}
							else
							{
								statisticValueDouble[countStatistic] = tsRegressionEstimateErrors.getMonthlyEquationRegressionErrors(iEquation).getStandardErrorOfSlope();
							}
						}
						/*
					else if ( statisticName.equals("SX") ) {
						statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
						if ( numEquations == 1 ) {
							statisticValueDouble[countStatistic] = new Double(regressionResults.getStandardDeviationX());
						}
						else {
							statisticValueDouble[countStatistic] = new Double(regressionResults.getStandardDeviationX(iEquation));
						}
					}*/
						else if (statisticName.Equals("SkewY"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								statisticValueDouble[countStatistic] = tsRegressionData.getSingleEquationRegressionData().getSkewY();
							}
							else
							{
								statisticValueDouble[countStatistic] = tsRegressionData.getMonthlyEquationRegressionData(iEquation).getSkewY();
							}
						}
						else if (statisticName.Equals("SkewY_trans"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								statisticValueDouble[countStatistic] = tsRegressionDataTransformed.getSingleEquationRegressionData().getSkewY();
							}
							else
							{
								statisticValueDouble[countStatistic] = tsRegressionDataTransformed.getMonthlyEquationRegressionData(iEquation).getSkewY();
							}
						}
						else if (statisticName.Equals("SkewYfilled"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								statisticValueDouble[countStatistic] = tsRegressionFilledValues.getSingleEquationRegressionFilledValues().getSkewYFilled();
							}
							else
							{
								statisticValueDouble[countStatistic] = tsRegressionFilledValues.getMonthlyEquationRegressionFilledValues(iEquation).getSkewYFilled();
							}
						}
						else if (statisticName.Equals("SX1"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								statisticValueDouble[countStatistic] = tsRegressionData.getSingleEquationRegressionData().getStandardDeviationX1();
							}
							else
							{
								statisticValueDouble[countStatistic] = tsRegressionData.getMonthlyEquationRegressionData(iEquation).getStandardDeviationX1();
							}
						}
						else if (statisticName.Equals("SX1_trans"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								statisticValueDouble[countStatistic] = tsRegressionDataTransformed.getSingleEquationRegressionData().getStandardDeviationX1();
							}
							else
							{
								statisticValueDouble[countStatistic] = tsRegressionDataTransformed.getMonthlyEquationRegressionData(iEquation).getStandardDeviationX1();
							}
						}
						else if (statisticName.Equals("SX2"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								if (tsRegressionData.getSingleEquationRegressionData().getN2() == 0)
								{
									statisticValueDouble[countStatistic] = null;
								}
								else
								{
									statisticValueDouble[countStatistic] = tsRegressionData.getSingleEquationRegressionData().getStandardDeviationX2();
								}
							}
							else
							{
								if ((tsRegressionData.getMonthlyEquationRegressionData(iEquation).getN2() == 0))
								{
									statisticValueDouble[countStatistic] = null;
								}
								else
								{
									statisticValueDouble[countStatistic] = tsRegressionData.getMonthlyEquationRegressionData(iEquation).getStandardDeviationX2();
								}
							}
						}
						else if (statisticName.Equals("SX2_trans"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								if (tsRegressionDataTransformed.getSingleEquationRegressionData().getN2() == 0)
								{
									statisticValueDouble[countStatistic] = null;
								}
								else
								{
									statisticValueDouble[countStatistic] = tsRegressionDataTransformed.getSingleEquationRegressionData().getStandardDeviationX2();
								}
							}
							else
							{
								if ((tsRegressionDataTransformed.getMonthlyEquationRegressionData(iEquation).getN2() == 0))
								{
									statisticValueDouble[countStatistic] = null;
								}
								else
								{
									statisticValueDouble[countStatistic] = tsRegressionDataTransformed.getMonthlyEquationRegressionData(iEquation).getStandardDeviationX2();
								}
							}
						}
						else if (statisticName.Equals("SY"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								statisticValueDouble[countStatistic] = tsRegressionData.getSingleEquationRegressionData().getStandardDeviationY();
							}
							else
							{
								statisticValueDouble[countStatistic] = tsRegressionData.getMonthlyEquationRegressionData(iEquation).getStandardDeviationY();
							}
						}
						else if (statisticName.Equals("SY_trans"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								statisticValueDouble[countStatistic] = tsRegressionDataTransformed.getSingleEquationRegressionData().getStandardDeviationY();
							}
							else
							{
								statisticValueDouble[countStatistic] = tsRegressionDataTransformed.getMonthlyEquationRegressionData(iEquation).getStandardDeviationY();
							}
						}
						else if (statisticName.Equals("SY1"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								statisticValueDouble[countStatistic] = tsRegressionData.getSingleEquationRegressionData().getStandardDeviationY1();
							}
							else
							{
								statisticValueDouble[countStatistic] = tsRegressionData.getMonthlyEquationRegressionData(iEquation).getStandardDeviationY1();
							}
						}
						else if (statisticName.Equals("SY1_trans"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								statisticValueDouble[countStatistic] = tsRegressionDataTransformed.getSingleEquationRegressionData().getStandardDeviationY1();
							}
							else
							{
								statisticValueDouble[countStatistic] = tsRegressionDataTransformed.getMonthlyEquationRegressionData(iEquation).getStandardDeviationY1();
							}
						}
						else if (statisticName.Equals("SY1est"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								statisticValueDouble[countStatistic] = tsRegressionEstimateErrors.getSingleEquationRegressionErrors().getStandardDeviationY1est();
							}
							else
							{
								statisticValueDouble[countStatistic] = tsRegressionEstimateErrors.getMonthlyEquationRegressionErrors(iEquation).getStandardDeviationY1est();
							}
						}
						else if (statisticName.Equals("SY1est_trans"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								statisticValueDouble[countStatistic] = tsRegressionEstimateErrorsTransformed.getSingleEquationRegressionErrors().getStandardDeviationY1est();
							}
							else
							{
								statisticValueDouble[countStatistic] = tsRegressionEstimateErrorsTransformed.getMonthlyEquationRegressionErrors(iEquation).getStandardDeviationY1est();
							}
						}
						else if (statisticName.Equals("SYfilled"))
						{
							// Actually filled values
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								statisticValueDouble[countStatistic] = tsRegressionFilledValues.getSingleEquationRegressionFilledValues().getStandardDeviationYFilled();
							}
							else
							{
								statisticValueDouble[countStatistic] = tsRegressionFilledValues.getMonthlyEquationRegressionFilledValues(iEquation).getStandardDeviationYFilled();
							}
						}
						else if (statisticName.Equals("TestScore"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								statisticValueDouble[countStatistic] = tsRegressionEstimateErrors.getSingleEquationRegressionErrors().getTestScore(tsRegressionResults.getSingleEquationRegressionResults().getB());
							}
							else
							{
								statisticValueDouble[countStatistic] = tsRegressionEstimateErrors.getMonthlyEquationRegressionErrors(iEquation).getTestScore(tsRegressionResults.getMonthlyEquationRegressionResults(iEquation).getB());
							}
						}
						else if (statisticName.Equals("TestQuantile"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							if (numEquations == 1)
							{
								statisticValueDouble[countStatistic] = tsRegressionEstimateErrors.getSingleEquationRegressionErrors().getStudentTTestQuantile(tsRegressionChecks.getSingleEquationRegressionChecks().getConfidenceIntervalPercent());
							}
							else
							{
								statisticValueDouble[countStatistic] = tsRegressionEstimateErrors.getMonthlyEquationRegressionErrors(iEquation).getStudentTTestQuantile(tsRegressionChecks.getMonthlyEquationRegressionChecks(iEquation).getConfidenceIntervalPercent());
							}
						}
						else if (statisticName.Equals("TestOK"))
						{
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_STRING;
							// Only have a value if the confidence interval has been specified
							if (analysis.getConfidenceIntervalPercent() == null)
							{
								statisticValueString[countStatistic] = "";
							}
							else
							{
								bool? related = null;
								if (numEquations == 1)
								{
									related = tsRegressionChecks.getSingleEquationRegressionChecks().getIsTestOK();
								}
								else
								{
									related = tsRegressionChecks.getMonthlyEquationRegressionChecks(iEquation).getIsTestOK();
								}
								if (related == null)
								{
									statisticValueString[countStatistic] = "";
								}
								else if (related.Value)
								{
									statisticValueString[countStatistic] = "Yes";
								}
								else
								{
									statisticValueString[countStatistic] = "No";
								}
							}
						}
						else
						{
							// Fall through for unrecognized statistics
							statisticFieldType[countStatistic] = TableField.DATA_TYPE_DOUBLE;
							statisticValueDouble[countStatistic] = null;
							Message.printStatus(2, routine, "Don't know how to process statistic \"" + statisticName + "\"");

						}
					}
					catch (Exception e)
					{
						// TODO SAM 2013-04-21 Need to figure out how to handle invalid sample size as null, NaN, etc.
						// Could be zero sample size
						// statisticFieldType[countStatistic] should have been set above so just set value
						statisticValueDouble[countStatistic] = null;
						if (Message.isDebugOn)
						{
							// TODO SAM for now wrap this but need better check to know when to print
							Message.printWarning(3, routine, "Error computing \"" + statisticName + "\" (" + e + ")");
						}
					}
				}
			}
			// By here the statistics will have been computed and are matched with the column name array
			// Now loop through again and process the row for the dependent and independent time series
			// First format the dependent and independent time series identifiers for to match the table...
			// Dependent time series identifier is configurable from parameter
			string tableTSIDDependent = null;
			if ((!string.ReferenceEquals(tableTSIDFormat, null)) && !tableTSIDFormat.Equals(""))
			{
				// Format the TSID using the specified format
				tableTSIDDependent = ts.formatLegend(tableTSIDFormat);
			}
			else
			{
				// Use the alias if available and then the TSID
				tableTSIDDependent = ts.getAlias();
				if ((string.ReferenceEquals(tableTSIDDependent, null)) || tableTSIDDependent.Equals(""))
				{
					tableTSIDDependent = ts.getIdentifierString();
				}
			}
			// Get the independent time series identifier
			string tableTSIDIndependent = null;
			if ((!string.ReferenceEquals(tableTSIDFormat, null)) && !tableTSIDFormat.Equals(""))
			{
				// Format the TSID using the specified format
				tableTSIDIndependent = analysis.getIndependentTS().formatLegend(tableTSIDFormat);
			}
			else
			{
				// Use the alias if available and then the TSID
				tableTSIDIndependent = analysis.getIndependentTS().getAlias();
				if ((string.ReferenceEquals(tableTSIDIndependent, null)) || tableTSIDIndependent.Equals(""))
				{
					tableTSIDIndependent = analysis.getIndependentTS().getIdentifierString();
				}
			}
			// Need to make sure that the table has the statistic columns of the correct type,
			// and look up the column numbers from the names in order to do the insert...
			int[] statisticColumnNumbers = new int[countStatisticTotal]; // columns in table
			countStatistic = -1;
			for (int iEquation = 0; iEquation < numEquations; iEquation++)
			{
				for (int iStatistic = 0; iStatistic < statsToOutput.Count; iStatistic++)
				{
					++countStatistic;
					try
					{
						statisticColumnNumbers[countStatistic] = table.getFieldIndex(statisticColumnNames[countStatistic]);
					}
					catch (Exception)
					{
						statisticColumnNumbers[countStatistic] = -1; // Indicates no column name matched in table
					}
					if (statisticColumnNumbers[countStatistic] < 0)
					{
						// The statistic column does not exist, so add and initialize with null (not nonValue)
						// The value will be set below.
						if (statisticFieldType[countStatistic] == TableField.DATA_TYPE_DOUBLE)
						{
							// Use precision of 8, which should cover most statistics without roundoff
							// (although this may be too many significant digits for some input).
							statisticColumnNumbers[countStatistic] = table.addField(new TableField(TableField.DATA_TYPE_DOUBLE,statisticColumnNames[countStatistic],-1,8), null);
						}
						else if (statisticFieldType[countStatistic] == TableField.DATA_TYPE_INT)
						{
							statisticColumnNumbers[countStatistic] = table.addField(new TableField(TableField.DATA_TYPE_INT,statisticColumnNames[countStatistic],-1,-1), null);
						}
						else if (statisticFieldType[countStatistic] == TableField.DATA_TYPE_STRING)
						{
							statisticColumnNumbers[countStatistic] = table.addField(new TableField(TableField.DATA_TYPE_STRING,statisticColumnNames[countStatistic],-1,-1), null);
						}
						Message.printStatus(2,routine,"Added column \"" + statisticColumnNames[countStatistic] + "\" at index [" + statisticColumnNumbers[countStatistic] + "]");
						// Set the description for the column so that it can be displayed in table tooltips, etc.
						if (numEquations == 1)
						{
							table.getTableField(statisticColumnNumbers[countStatistic]).setDescription(statisticComments[statisticColumnNames[countStatistic]]);
						}
						else
						{
							table.getTableField(statisticColumnNumbers[countStatistic]).setDescription(statisticComments[statisticColumnNames[countStatistic]] + " (Month " + (iEquation + 1) + "=" + TimeUtil.monthAbbreviation(iEquation + 1) + ")");
						}
					}
				}
			}
			// Next, find the record that has the dependent and independent identifiers...
			// Find the record that matches the dependent and independent identifiers (should only be one but
			// handle multiple matches)
			IList<string> tableColumnNames = new List<string>(); // The dependent and independent TSID column names
			tableColumnNames.Add(tableTSIDColumnName);
			tableColumnNames.Add(tableTSIDColumnNameIndependent);
			IList<string> tableColumnValues = new List<string>(); // The dependent and independent TSID values
			tableColumnValues.Add(tableTSIDDependent);
			tableColumnValues.Add(tableTSIDIndependent);
			IList<TableRecord> recList = table.getRecords(tableColumnNames, tableColumnValues);
			if (Message.isDebugOn)
			{
				Message.printStatus(2,routine,"Searched for records with columns matching \"" + tableTSIDColumnName + "\"=\"" + tableTSIDDependent + "\" " + tableTSIDColumnNameIndependent + "\"=\"" + tableTSIDIndependent + "\"... found " + recList.Count);
			}
			if (recList.Count == 0)
			{
				// No record in the table so add one with TSID column values and blank statistic values...
				TableRecord rec = null;
				table.addRecord(rec = table.emptyRecord());
				rec.setFieldValue(tableTSIDColumnNumber, tableTSIDDependent);
				rec.setFieldValue(tableTSIDColumnNumberIndependent, tableTSIDIndependent);
				recList.Add(rec);
			}
			// Finally loop through the statistics and insert into the rows matched above.  Although multiple
			// records may have been matched, the normal case will be that one record is matched.
			foreach (TableRecord rec in recList)
			{
				// Set the static column contents
				rec.setFieldValue(table.getFieldIndex("AnalysisMethod"), getAnalysisMethod());
				rec.setFieldValue(table.getFieldIndex("DependentAnalysisStart"), getDependentAnalysisStart());
				rec.setFieldValue(table.getFieldIndex("DependentAnalysisEnd"), getDependentAnalysisEnd());
				rec.setFieldValue(table.getFieldIndex("IndependentAnalysisStart"), getIndependentAnalysisStart());
				rec.setFieldValue(table.getFieldIndex("IndependentAnalysisEnd"), getIndependentAnalysisEnd());
				rec.setFieldValue(table.getFieldIndex("FillStart"), getFillStart());
				rec.setFieldValue(table.getFieldIndex("FillEnd"), getFillEnd());
				rec.setFieldValue(table.getFieldIndex("MinimumSampleSize"), getMinimumSampleSize());
				rec.setFieldValue(table.getFieldIndex("MinimumR"), getMinimumR());
				rec.setFieldValue(table.getFieldIndex("ConfidenceInterval"), analysis.getConfidenceIntervalPercent());
				rec.setFieldValue(table.getFieldIndex("Transformation"), analysis.getTransformation());
				countStatistic = -1;
				// Set the statistic columns for values that were actually used.
				for (int iEquation = 0; iEquation < numEquations; iEquation++)
				{
					for (int iStatistic = 0; iStatistic < statsToOutput.Count; iStatistic++)
					{
						// Set the value based on the object type for the statistic.
						// There can only be one non-null statistic value
						++countStatistic;
						if (statisticValueDouble[countStatistic] != null)
						{
							rec.setFieldValue(statisticColumnNumbers[countStatistic], statisticValueDouble[countStatistic]);
						}
						else if (statisticValueInteger[countStatistic] != null)
						{
							rec.setFieldValue(statisticColumnNumbers[countStatistic], statisticValueInteger[countStatistic]);
						}
						else if (!string.ReferenceEquals(statisticValueString[countStatistic], null))
						{
							rec.setFieldValue(statisticColumnNumbers[countStatistic], statisticValueString[countStatistic]);
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// Set the TSRegressionFilledValues object.
	/// </summary>
	private void setTSRegressionFilledValues(TSRegressionFilledValues fv)
	{
		__tsRegressionFilledValues = fv;
	}

	}

}