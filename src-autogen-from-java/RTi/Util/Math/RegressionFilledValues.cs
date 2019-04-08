// RegressionFilledValues - filled (previously missing) values resulting from a regression analysis.

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
	/// Filled (previously missing) values resulting from a regression analysis.
	/// The information is useful to compare the statistics of filled values compared to the non-filled values.
	/// The data are simple arrays; however, when combined to handle monthly
	/// regression or other complex analysis, the object help manage data.
	/// </summary>
	public class RegressionFilledValues
	{

	// Data for filled Y values...

	/// <summary>
	/// The dependent data array, for points that have been filled/estimated using the regression equation.
	/// </summary>
	private double[] __yFilled = new double[0];

	/// <summary>
	/// Minimum filled Y values.
	/// </summary>
	private double? __minYfilled = null;

	/// <summary>
	/// Maximum filled Y values.
	/// </summary>
	private double? __maxYfilled = null;

	/// <summary>
	/// Mean of filled Y values.
	/// </summary>
	private double? __meanYfilled = null;

	/// <summary>
	/// Skew of filled Y values.
	/// </summary>
	private double? __skewYfilled = null;

	/// <summary>
	/// Standard deviation of filled Y values.
	/// </summary>
	private double? __stddevYfilled = null;

	/// <summary>
	/// Constructor.  Set the dependent filled data array, which should contain values estimated using the regression
	/// relationship (and which were previously missing).
	/// The data array is used to compute basic statistics such as mean and standard deviation,
	/// but only when the get methods are called.  Null arrays will be set to empty arrays. </summary>
	/// <param name="yFilled"> dependent data array, for values that have been filled (estimated). </param>
	public RegressionFilledValues(double[] yFilled)
	{
		if (yFilled == null)
		{
			__yFilled = new double[0];
		}
		else
		{
			__yFilled = yFilled;
		}
		// Basic statistics will be calculated lazily (if requested)
	}

	/// <summary>
	/// Return the maximum data value for the filled dependent array, or null if not analyzed. </summary>
	/// <returns> the maximum data value for the filled dependent array, or null if not analyzed. </returns>
	public virtual double? getMaxYFilled()
	{
		double? maxYfilled = __maxYfilled;
		if ((maxYfilled == null) && (getYFilled().Length > 0))
		{
			maxYfilled = MathUtil.max(getYFilled());
			setMaxYFilled(maxYfilled);
		}
		return maxYfilled;
	}

	/// <summary>
	/// Return the mean for the filled dependent array, or null if not analyzed. </summary>
	/// <returns> the mean for the filled dependent array, or null if not analyzed. </returns>
	public virtual double? getMeanYFilled()
	{
		double? meanYFilled = __meanYfilled;
		if ((meanYFilled == null) && (getYFilled().Length > 0))
		{
			meanYFilled = MathUtil.mean(getYFilled());
			setMeanYFilled(meanYFilled);
		}
		return meanYFilled;
	}

	/// <summary>
	/// Return the minimum data value for the filled dependent array, or null if not analyzed. </summary>
	/// <returns> the minimum data value for the filled dependent array, or null if not analyzed. </returns>
	public virtual double? getMinYFilled()
	{
		double? minYFilled = __minYfilled;
		if ((minYFilled == null) && (getYFilled().Length > 0))
		{
			minYFilled = MathUtil.min(getYFilled());
			setMinYFilled(minYFilled);
		}
		return minYFilled;
	}

	/// <summary>
	/// Return the size of the filled dependent array.
	/// </summary>
	public virtual int getNFilled()
	{
		if (__yFilled == null)
		{
			return 0;
		}
		else
		{
			return __yFilled.Length;
		}
	}

	/// <summary>
	/// Return the skew for the filled dependent array, or null if not analyzed. </summary>
	/// <returns> the skew for the filled dependent array, or null if not analyzed. </returns>
	public virtual double? getSkewYFilled()
	{
		double? skewYfilled = __skewYfilled;
		if ((skewYfilled == null) && (getYFilled().Length >= 3))
		{
			skewYfilled = MathUtil.skew(getYFilled().Length, getYFilled());
			setSkewYFilled(skewYfilled);
		}
		return skewYfilled;
	}

	/// <summary>
	/// Return the standard deviation for the filled dependent array, or null if not analyzed. </summary>
	/// <returns> the standard deviation for the filled dependent array, or null if not analyzed. </returns>
	public virtual double? getStandardDeviationYFilled()
	{
		double? stddevYfilled = __stddevYfilled;
		if ((stddevYfilled == null) && getYFilled() != null && (getYFilled().Length >= 2))
		{
			stddevYfilled = MathUtil.standardDeviation(getYFilled());
			setStandardDeviationYFilled(stddevYfilled);
		}
		return stddevYfilled;
	}

	/// <summary>
	/// Return the filled dependent data array.
	/// </summary>
	public virtual double [] getYFilled()
	{
		return __yFilled;
	}

	/// <summary>
	/// Set the maximum data value for the filled dependent data. </summary>
	/// <param name="maxYFilled"> Maximum data value for the filled dependent data. </param>
	private void setMaxYFilled(double? maxYFilled)
	{
		__maxYfilled = maxYFilled;
	}

	/// <summary>
	/// Set the mean for the filled dependent data. </summary>
	/// <param name="meanYFilled"> Mean for the filled dependent data. </param>
	private void setMeanYFilled(double? meanYFilled)
	{
		__meanYfilled = meanYFilled;
	}

	/// <summary>
	/// Set the minimum data value for the filled dependent data. </summary>
	/// <param name="minYFilled"> Minimum data value for the filled dependent data. </param>
	private void setMinYFilled(double? minYFilled)
	{
		__minYfilled = minYFilled;
	}

	/// <summary>
	/// Set the skew for the filled dependent data. </summary>
	/// <param name="skewYFilled"> skew for the filled dependent data. </param>
	private void setSkewYFilled(double? skewYFilled)
	{
		__skewYfilled = skewYFilled;
	}

	/// <summary>
	/// Set the standard deviation for the filled dependent data. </summary>
	/// <param name="stddevYFilled"> Standard deviation for the filled dependent data. </param>
	private void setStandardDeviationYFilled(double? stddevYFilled)
	{
		__stddevYfilled = stddevYFilled;
	}

	}

}