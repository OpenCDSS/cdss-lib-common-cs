// MonthTotals - simple class for returning time series totals by month

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
// MonthTotals - simple class for returning time series totals by month
// ----------------------------------------------------------------------------
// History:
//
// 05 Jun 1998	Steven A. Malers, RTi	Initial version.
// 13 Apr 1999	SAM, RTi		Add finalize.
// 2001-11-06	SAM, RTi		Review javadoc.  Verify that variables
//					are set to null when no longer used.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.TS
{
	/// <summary>
	/// This class stores information about time series by month.  It is returned
	/// by TSUtil.getMonthTotals.  This class may be deprecated in the future (include
	/// in TSLimits?).
	/// </summary>
	public class MonthTotals
	{

	private int[] _numsummed;
	private int[] _nummissing;
	private double[] _avgvals;
	private double[] _summedvals;

	/// <summary>
	/// Default constructor.  Initialize to null data.  The arrays must be set by
	/// other code.
	/// </summary>
	public MonthTotals()
	{
		initialize();
	}

	/// <summary>
	/// Finalize before garbage collection. </summary>
	/// <exception cref="Throwable"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~MonthTotals()
	{
		_numsummed = null;
		_nummissing = null;
		_avgvals = null;
		_summedvals = null;
	}

	/// <summary>
	/// Return the monthly averages. </summary>
	/// <returns> the monthly averages. </returns>
	public virtual double [] getAverages()
	{
		return _avgvals;
	}

	/// <summary>
	/// Return the number of missing for months. </summary>
	/// <returns> the number of missing for months. </returns>
	public virtual int [] getNumMissing()
	{
		return _nummissing;
	}

	/// <summary>
	/// Return the number summed for months. </summary>
	/// <returns> the number summed for months. </returns>
	public virtual int [] getNumSummed()
	{
		return _numsummed;
	}

	/// <summary>
	/// Return the monthly sums. </summary>
	/// <returns> the monthly sums. </returns>
	public virtual double [] getSums()
	{
		return _summedvals;
	}

	/// <summary>
	/// Initialize the data.
	/// </summary>
	private void initialize()
	{
		_numsummed = null;
		_nummissing = null;
		_avgvals = null;
		_summedvals = null;
	}

	/// <summary>
	/// Set the monthly averages. </summary>
	/// <param name="avgvals"> Monthly averages. </param>
	public virtual void setAverages(double[] avgvals)
	{
		_avgvals = avgvals;
	}

	/// <summary>
	/// Set the monthly number of missing. </summary>
	/// <param name="nummissing"> Monthly number of missing. </param>
	public virtual void setNumMissing(int[] nummissing)
	{
		_nummissing = nummissing;
	}

	/// <summary>
	/// Set the monthly number summed. </summary>
	/// <param name="numsummed"> Monthly number summed. </param>
	public virtual void setNumSummed(int[] numsummed)
	{
		_numsummed = numsummed;
	}

	/// <summary>
	/// Set the monthly sums. </summary>
	/// <param name="summedvals"> Monthly sums. </param>
	public virtual void setSums(double[] summedvals)
	{
		_summedvals = summedvals;
	}

	} // End of MonthTotals

}