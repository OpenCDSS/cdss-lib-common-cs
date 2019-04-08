using System.Collections.Generic;
using System.Text;

// TSPrincipalComponentAnalysis

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

/*
 * These routines help manipulate data for Principal Component Analysis
 * by arranging the data in arrays suitable for calculations.  Also, a
 * fill routine is provided to use the results from PCA to fill missing data
 * in a TS.
 */

namespace RTi.TS
{
	using MathUtil = RTi.Util.Math.MathUtil;
	using PrincipalComponentAnalysis = RTi.Util.Math.PrincipalComponentAnalysis;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using DateTime = RTi.Util.Time.DateTime;

	/// 
	/// <summary>
	/// @author cen
	/// </summary>
	public class TSPrincipalComponentAnalysis
	{

	internal TS _dependentTS;
	internal IList<TS> _independentTS;

	private DateTime _analysis_period_start;
	private DateTime _analysis_period_end; // Analysis period
	private bool[] _analyze_month; // Indicates the months to analyze.
	private int[] _analyze_month_list; // List of month numbers to analyze.
	private int _maxCombinations; // Maximum combination of solutions to retain.
	internal double[][] _xMatrix = null; // matrix representation of independentTS data
	internal double[] _yArray = null; // array representation of dependentTS data
	internal PrincipalComponentAnalysis _pca; // Analysis, including statistics, etc.


	/// 
	/// <param name="dependentTS"> The dependent time series (Y). </param>
	/// <param name="independentTS"> A list of the independent time series (X). </param>
	/// <param name="AnalysisPeriodStart"> Date/time indicating analysis period start (full period of dependent TS if null) </param>
	/// <param name="AnalysisPeriodEnd"> Date/time indicating analysis period end (full period of dependent TS if null) </param>
	/// <param name="AnalysisMonths"> Indicate the months that are to be analyzed.
	/// If monthly equations are being used, indicate the one month to analyze.
	/// Specify the months separated by commas or white space.
	/// </param>
	/// <exception cref="java.lang.Exception"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TSPrincipalComponentAnalysis(TS dependentTS, java.util.List<TS> independentTS, RTi.Util.Time.DateTime AnalysisPeriodStart, RTi.Util.Time.DateTime AnalysisPeriodEnd, int maxCombinations, String analysisMonths) throws Exception
	public TSPrincipalComponentAnalysis(TS dependentTS, IList<TS> independentTS, DateTime AnalysisPeriodStart, DateTime AnalysisPeriodEnd, int maxCombinations, string analysisMonths)
	{

		initialize(dependentTS, independentTS, AnalysisPeriodStart, AnalysisPeriodEnd, maxCombinations, analysisMonths);
		analyze();

	}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void analyze() throws Exception
	private void analyze()
	{

		double[] xArray = null;

		int numTS = _independentTS.Count;
		bool includeAnalyzeMonthList = ((_analyze_month_list == null) || (_analyze_month_list.Length == 0)) ? false : true;

		// Get data specified or all data if analysis months weren't specified
		_yArray = includeAnalyzeMonthList ? TSUtil.toArray(_dependentTS, _analysis_period_start, _analysis_period_end, _analyze_month_list) : TSUtil.toArray(_dependentTS, _analysis_period_start, _analysis_period_end);

		// Save independent observations in xMatrix[nObservations][numTS]
		int nObservations = _yArray.Length;
		// now we know number of stations and number of observations
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: _xMatrix = new double[nObservations][numTS];
		_xMatrix = RectangularArrays.RectangularDoubleArray(nObservations, numTS);

		// fill all data into xMatrix
		for (int nTS = 0; nTS < numTS; nTS++)
		{
			TS ts = _independentTS[nTS];
			xArray = includeAnalyzeMonthList ? TSUtil.toArray(ts, _analysis_period_start, _analysis_period_end, _analyze_month_list) : TSUtil.toArray(ts, _analysis_period_start, _analysis_period_end);
			for (int i = 0; i < nObservations; i++)
			{
				_xMatrix[i][nTS] = xArray[i];
			}
		}

		_pca = MathUtil.performPrincipalComponentAnalysis(_yArray, _xMatrix, _dependentTS.getMissing(), _independentTS[0].getMissing(), _maxCombinations);
	}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void initialize(TS dependentTS, java.util.List<TS> independentTS, RTi.Util.Time.DateTime start, RTi.Util.Time.DateTime end, int maxCombinations, String analysisMonths) throws Exception
	private void initialize(TS dependentTS, IList<TS> independentTS, DateTime start, DateTime end, int maxCombinations, string analysisMonths)
	{

		string rtn = "TSPrincipalComponentAnalysis.initialize";
		_dependentTS = dependentTS;
		_independentTS = independentTS;

		// verify all TS have same interval
		int interval_base = _dependentTS.getDataIntervalBase();
		int interval_mult = _dependentTS.getDataIntervalMult();
		int numTS = _independentTS.Count;
		for (int nTS = 0; nTS < numTS; nTS++)
		{
			TS ts = _independentTS[nTS];
			if (ts.getDataIntervalBase() != interval_base || ts.getDataIntervalMult() != interval_mult)
			{
				Message.printWarning(1, rtn, "All TS mult have same data interval.  Cannot continue analysis.");
				return;
			}
		}

		_analyze_month = new bool[12];
		_analyze_month_list = null;
		if ((!string.ReferenceEquals(analysisMonths, null)) && !analysisMonths.Equals("*") && !analysisMonths.Equals(""))
		{
			IList<string> tokens = StringUtil.breakStringList(analysisMonths, " ,\t", StringUtil.DELIM_SKIP_BLANKS);
			int size = 0;
			if (tokens != null)
			{
				size = tokens.Count;
			}
			for (int i = 0; i < 12; i++)
			{
				_analyze_month[i] = true; // Default is to analyze all months.
			}
			if (size > 0)
			{
				// Reset all to false.  Selected months will be set to true below.
				_analyze_month_list = new int[size];
				for (int i = 0; i < 12; i++)
				{
					_analyze_month[i] = false;
				}
			}
			int imon;
			for (int i = 0; i < size; i++)
			{
				imon = int.Parse((string)tokens[i]);
				if ((imon >= 1) && (imon <= 12))
				{
					_analyze_month[imon - 1] = true;
				}
				_analyze_month_list[i] = imon;
			}
		}

		_analysis_period_end = (end == null) ? dependentTS.getDate2() : end;

		_analysis_period_start = (start == null) ? dependentTS.getDate1() : start;

		_maxCombinations = maxCombinations == 0 ? 20 : maxCombinations;


	}

	public virtual PrincipalComponentAnalysis getPrincipalComponentAnalysis()
	{
		return _pca;
	}

	/*
	 * This routine uses the results from the Principal Component Analysis
	 */
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TS fill(int regressionEqIndex, RTi.Util.Time.DateTime fillStart, RTi.Util.Time.DateTime fillEnd) throws Exception
	public virtual TS fill(int regressionEqIndex, DateTime fillStart, DateTime fillEnd)
	{
		bool changed = false;
		TS tsToFill = (TS) _dependentTS.clone();
		// regressionEqIndex is 1-based, bcomb is 0-based.
		double[] regressionEquation = _pca.getBcombForIndex(regressionEqIndex - 1);

		// at this point, _analysis_period_start correlates with the first data value
		// in regressionEquation
		TSData ydata = null;
		double xdata, value, missing = tsToFill.getMissing();
		int numTS = _independentTS.Count;
		TSIterator tsi = new TSIterator(_dependentTS, _analysis_period_start, _analysis_period_end);
		int regressionIndex; // index to regression equation
		int timeIndex = 0; // index to time; used to step through xMatrix

		while ((ydata = tsi.next()) != null)
		{
			if (ydata.getDataValue() == missing)
			{
				// regressionEquation[0] is the intercept.  Start with that.
				value = regressionEquation[0];
				regressionIndex = 0;
				for (int nTS = 0; nTS < numTS; nTS++)
				{
					xdata = _xMatrix[timeIndex][nTS];
					if (xdata == missing)
					{
						value = missing;
						// end the loop
						nTS = numTS;
					}
					else
					{
						if (regressionEquation[++regressionIndex] != missing)
						{
							value += xdata * regressionEquation[regressionIndex];
						}
					}
				}
				if (value != missing)
				{
					tsToFill.setDataValue(tsi.getDate(), value);
					changed = true;
				}
			}
			timeIndex++;
		}

		// add genesis if missing values were filled
		if (changed)
		{
			StringBuilder genesis = new StringBuilder("Filled missing values using PCA, regression equation ");
			for (int nTS = 1; nTS <= numTS; nTS++)
			{
				genesis.Append("" + (regressionEquation[nTS] == tsToFill.getMissing()? 0 : regressionEquation[nTS]));
				if (nTS != numTS)
				{
					genesis.Append(", ");
				}
			}
			tsToFill.addToGenesis(genesis.ToString());

		}
		return tsToFill;
	}

	}

}