// TSRegressionChecks - Immutable data indicating whether regression relationships meet
// criteria at an individual check and overall level (all checks must pass).

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
	using RegressionChecks = RTi.Util.Math.RegressionChecks;

	/// <summary>
	/// Immutable data indicating whether regression relationships meet criteria at an individual check and overall
	/// level (all checks must pass).
	/// </summary>
	public class TSRegressionChecks
	{

	/// <summary>
	/// Regression checks using a single equation.
	/// </summary>
	internal RegressionChecks __singleEquationChecks = null;

	/// <summary>
	/// Regression data using monthly equations.
	/// </summary>
	internal RegressionChecks[] __monthlyEquationChecks = null;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="singleEquationChecks"> regression data for a single equation (may contain a subset of months) </param>
	/// <param name="monthlyEquationChecks"> regression data for each month (may only be complete for a subset of months). </param>
	public TSRegressionChecks(RegressionChecks singleEquationChecks, RegressionChecks[] monthlyEquationChecks)
	{
		__singleEquationChecks = singleEquationChecks;
		__monthlyEquationChecks = monthlyEquationChecks;
	}

	/// <summary>
	/// Return a monthly equation regression checks. </summary>
	/// <returns> a monthly equation regression checks. </returns>
	/// <param name="month"> the month of interest (1-12). </param>
	public virtual RegressionChecks getMonthlyEquationRegressionChecks(int month)
	{
		return __monthlyEquationChecks[month - 1];
	}

	/// <summary>
	/// Return the single equation regression checks. </summary>
	/// <returns> the single equation regression checks. </returns>
	public virtual RegressionChecks getSingleEquationRegressionChecks()
	{
		return __singleEquationChecks;
	}

	}

}