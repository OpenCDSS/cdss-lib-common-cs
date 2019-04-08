using System.Collections.Generic;
using System.Text;

// TSPatternStats - simple class to hold and manipulate statistics associated with time series patterns

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
// TSPatternStats - simple class to hold and manipulate statistics associated
//			with time series patterns
// ----------------------------------------------------------------------------
// History:
//
// 06 Jul 1998	Steven A. Malers, RTi	Initial version.
// 05 Oct 1998	SAM, RTi		Add pattern name so that it can be
//					printed in the output.
// 13 Apr 1999	SAM, RTi		Add finalize.
// 2001-11-06	SAM, RTi		Review javadoc.  Verify that variables
//					are set to null when no longer used.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.TS
{

	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// The TSPatternStats class stores statistical information time series when
	/// analyzed using a pattern.  An instance of this class is used, for example, by
	/// TSUtil.fillPattern. </summary>
	/// <seealso cref= StringMonthTS </seealso>
	/// <seealso cref= TSUtil#fillPattern </seealso>
	public class TSPatternStats
	{

	// Data...

	private TS _ts = null; // Time series being analyzed.
	private TS _pattern_ts; // Time series used for pattern.
	private int _num_indicators = 0; // Number of rows.
	private double[][] _average = null; // Averages by pattern and month.
	private double[][] _sum = null; // Sums by pattern and month.
	private int[][] _count = null; // Count by pattern and month (number
						// of non-missing data).
	private string[] _indicator = null; // Indicator strings for rows.
	private bool _dirty = false; // Indicates if data have been modified.

	/// <summary>
	/// Default constructor.  Initialize with missing data for the number of rows
	/// shown (data are stored with the number of rows being equal to the number of
	/// patterns and the columns being the number of months (12). </summary>
	/// <param name="indicators"> list of indicator strings (e.g., "WET", "DRY", "AVG"). </param>
	/// <param name="ts"> Time series to analyze. </param>
	public TSPatternStats(IList<string> indicators, TS ts)
	{
		initialize(indicators, ts, null);
	}

	/// <summary>
	/// Initialize with missing data for the number of rows shown (data are stored with
	/// the number of rows being equal to the number of patterns and the columns being
	/// the number of months (12). </summary>
	/// <param name="indicators"> list of indicator strings (e.g., "WET", "DRY", "AVG"). </param>
	/// <param name="ts"> Time series to analyze. </param>
	/// <param name="pattern_ts"> Existing pattern time series. </param>
	public TSPatternStats(IList<string> indicators, TS ts, TS pattern_ts)
	{
		initialize(indicators, ts, pattern_ts);
	}

	/// <summary>
	/// For an indicator string and the month, add a data value to the statistics... </summary>
	/// <param name="indicator"> Indicator string corresponding to row in statistics. </param>
	/// <param name="value"> Data value for the indicated month. </param>
	/// <param name="month"> Calendar month for data value (1-12). </param>
	public virtual void add(string indicator, double value, int month)
	{ // First find the indicator row...
		if (_ts.isDataMissing(value))
		{
			return;
		}
		int row = findRow(indicator);
		if (row >= 0)
		{
			// Found the row...  Add the value...
			if (_ts.isDataMissing(_sum[row][month - 1]))
			{
				// Just reset...
				_sum[row][month - 1] = value;
			}
			else
			{ // Add...
				_sum[row][month - 1] += value;
			}
			_dirty = true;
			++_count[row][month - 1];
		}
	}

	/// <summary>
	/// Finalize before garbage collection. </summary>
	/// <exception cref="Throwable"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~TSPatternStats()
	{
		_ts = null;
		_pattern_ts = null;
		_average = null;
		_sum = null;
		_count = null;
		_indicator = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// The row corresponding to the indicator or -1 if the row is not found. </summary>
	/// <returns> The row corresponding to the indicator. </returns>
	public virtual int findRow(string indicator)
	{
		for (int i = 0; i < _num_indicators; i++)
		{
			if (_indicator[i].Equals(indicator))
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// Return a computed average value for the given indicator string and month. </summary>
	/// <returns> A computed average value for the given indicator string and month. </returns>
	/// <exception cref="TSException"> if the indicator cannot be found in the pattern. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double getAverage(String indicator, int month) throws TSException
	public virtual double getAverage(string indicator, int month)
	{
		refresh();
		int _row = findRow(indicator);
		if (_row >= 0)
		{
			return _average[_row][month - 1];
		}
		else
		{
			throw new TSException("Can't find indicator");
		}
	}

	/// <summary>
	/// Initialize the data. </summary>
	/// <param name="indicators"> list of indicator strings (e.g., "WET", "DRY", "AVG"). </param>
	/// <param name="ts"> Time series to analyze. </param>
	/// <param name="pattern_ts"> Existing pattern time series. </param>
	private void initialize(IList<string> indicators, TS ts, TS pattern_ts)
	{
		_num_indicators = indicators.Count;
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: _average = new double[_num_indicators][12];
		_average = RectangularArrays.RectangularDoubleArray(_num_indicators, 12);
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: _sum = new double[_num_indicators][12];
		_sum = RectangularArrays.RectangularDoubleArray(_num_indicators, 12);
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: _count = new int[_num_indicators][12];
		_count = RectangularArrays.RectangularIntArray(_num_indicators, 12);
		double missing = ts.getMissing();
		_indicator = new string[_num_indicators];
		_ts = ts;
		_pattern_ts = pattern_ts;

		for (int i = 0; i < _num_indicators; i++)
		{
			for (int j = 0; j < 12; j++)
			{
				_sum[i][j] = missing;
				_average[i][j] = missing;
				_count[i][j] = 0;
			}
			_indicator[i] = (string)indicators[i];
		}
	}

	/// <summary>
	/// Refresh the derived values (averages).
	/// </summary>
	public virtual void refresh()
	{
		if (!_dirty)
		{
			return;
		}
		for (int i = 0; i < _num_indicators; i++)
		{
			for (int j = 0; j < 12; j++)
			{
				if ((_count[i][j] > 0) && !_ts.isDataMissing(_sum[i][j]))
				{
					_average[i][j] = _sum[i][j] / (double)_count[i][j];
				}
			}
		}
		_dirty = false;
	}

	/// <summary>
	/// Return a string representation of the object.
	/// </summary>
	public override string ToString()
	{
		StringBuilder buffer = new StringBuilder();
		string newline = System.getProperty("line.separator");

		refresh();
		if (_pattern_ts != null)
		{
			buffer.Append("Pattern statistics for \"" + _ts.getIdentifierString() + "\" using pattern \"" + _pattern_ts.getLocation() + "\"" + newline);
		}
		else
		{
			buffer.Append("Pattern statistics for \"" + _ts.getIdentifierString() + "\"" + newline);
		}
		buffer.Append("        Jan       Feb       Mar       Apr       May       Jun       Jul       Aug       Sep       Oct       Nov       Dec       Total" + newline);
		int total;
		double dtotal;
		double missing = _ts.getMissing();
		for (int i = 0; i < _num_indicators; i++)
		{
			buffer.Append("Indicator:  \"" + _indicator[i] + "\"" + newline);
			buffer.Append("SUM: ");
			dtotal = missing;
			for (int j = 0; j < 12; j++)
			{
				if (!_ts.isDataMissing(_sum[i][j]))
				{
					if (_ts.isDataMissing(dtotal))
					{
						dtotal = _sum[i][j];
					}
					else
					{
						dtotal += _sum[i][j];
					}
				}
				buffer.Append(StringUtil.formatString(_sum[i][j], "%10.2f"));
			}
			buffer.Append(StringUtil.formatString(dtotal, "%10.2f"));
			buffer.Append(newline);
			buffer.Append("NUM: ");
			total = 0;
			for (int j = 0; j < 12; j++)
			{
				total += _count[i][j];
				buffer.Append(StringUtil.formatString(_count[i][j], "%10d"));
			}
			buffer.Append(StringUtil.formatString(total,"%10d"));
			buffer.Append(newline);
			dtotal = missing;
			buffer.Append("AVE: ");
			for (int j = 0; j < 12; j++)
			{
				if (!_ts.isDataMissing(_average[i][j]))
				{
					if (_ts.isDataMissing(dtotal))
					{
						dtotal = _average[i][j];
					}
					else
					{
						dtotal += _average[i][j];
					}
				}
				buffer.Append(StringUtil.formatString(_average[i][j], "%10.2f"));
			}
			buffer.Append(StringUtil.formatString(dtotal, "%10.2f"));
			buffer.Append(newline);
		}

		string s = buffer.ToString();
		buffer = null;
		newline = null;
		return s;
	}

	} // End of TSPatternStats class definition

}