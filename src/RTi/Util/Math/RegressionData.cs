using System;

// RegressionData - data used in a regression analysis

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
	/// Data used in a regression analysis.  The data are simple arrays; however, when combined to handle monthly
	/// regression or other complex analysis, the object helps manage data.
	/// </summary>
	public class RegressionData
	{

	// Data for overlapping X (X1)...

	/// <summary>
	/// The independent data array, for points that overlap the dependent data array.
	/// </summary>
	private double[] __x1 = new double[0];

	/// <summary>
	/// Minimum value of X in N1.
	/// </summary>
	private double? __minX1 = null;

	/// <summary>
	/// Maximum value of X in N1.
	/// </summary>
	private double? __maxX1 = null;

	/// <summary>
	/// Mean of X in N1.
	/// </summary>
	private double? __meanX1 = null;

	/// <summary>
	/// Standard deviation of X in N1.
	/// </summary>
	private double? __stddevX1 = null;

	// Data for overlapping Y (Y1)...

	/// <summary>
	/// The dependent data array, for points that overlap the independent data array.
	/// </summary>
	private double[] __y1 = new double[0];

	/// <summary>
	/// Minimum value of Y in N1.
	/// </summary>
	private double? __minY1 = null;

	/// <summary>
	/// Maximum value of Y in N1.
	/// </summary>
	private double? __maxY1 = null;

	/// <summary>
	/// Mean of Y in N1.
	/// </summary>
	private double? __meanY1 = null;

	/// <summary>
	/// Standard deviation of Y in N1.
	/// </summary>
	private double? __stddevY1 = null;

	// Data for non-overlapping X (X2)...

	/// <summary>
	/// The independent data array, for points that DO NOT overlap the dependent data array.
	/// </summary>
	private double[] __x2 = new double[0];

	/// <summary>
	/// Mean of X in N2.
	/// </summary>
	private double? __meanX2 = null;

	/// <summary>
	/// Standard deviation of X in N2.
	/// </summary>
	private double? __stddevX2 = null;

	// Data for non-overlapping dependent (Y3)...

	/// <summary>
	/// The dependent data array, for points that do not overlap the independent.
	/// </summary>
	private double[] __y3 = new double[0];

	// Data for full X (X1 and X2)...

	/// <summary>
	/// The independent data array, for all points.
	/// This is constructed from getX1() and getX2() if requested by the get method.
	/// </summary>
	private double[] __x = new double[0];

	/// <summary>
	/// Mean of X in N1 + N2.
	/// </summary>
	private double? __meanX = null;

	/// <summary>
	/// Standard deviation of X in N1 + N2.
	/// </summary>
	private double? __stddevX = null;

	// Data for full Y (Y1 and Y3)...

	/// <summary>
	/// The dependent data array, for all original points.
	/// This is constructed from getY1() and getY3() if requested by the get method.
	/// </summary>
	private double[] __y = new double[0];

	/// <summary>
	/// Mean of Y.
	/// </summary>
	private double? __meanY = null;

	/// <summary>
	/// Standard deviation of Y.
	/// </summary>
	private double? __stddevY = null;

	/// <summary>
	/// Skew of Y.
	/// </summary>
	private double? __skewY = null;

	/// <summary>
	/// Number of data points.
	/// </summary>
	private int __N = -1;

	/// <summary>
	/// Number of overlapping data points.
	/// </summary>
	private int __N1 = -1;

	/// <summary>
	/// Number of non-overlapping X data points.
	/// </summary>
	private int __N2 = -1;

	/// <summary>
	/// Number of non-overlapping Y data points.
	/// </summary>
	private int __N3 = -1;

	/// <summary>
	/// Number of Y data points.
	/// </summary>
	private int __NY = -1;

	/// <summary>
	/// Constructor.  Set the independent and dependent data arrays, which should exactly overlap and not contain
	/// missing values.  The data arrays are used to compute basic statistics such as mean and standard deviation for
	/// each array, but only when the get methods are called.  Null arrays will be set to empty arrays. </summary>
	/// <param name="x1"> independent data array, for points that DO overlap the dependent data array </param>
	/// <param name="y1"> dependent data array, for points that DO overlap the independent data array </param>
	/// <param name="x2"> independent data array, for points that DO NOT overlap the dependent data array </param>
	/// <param name="y3"> dependent data array, for points that DO NOT overlap the independent data array (this sample should
	/// not be confused with the "2" designation) </param>
	public RegressionData(double[] x1, double[] y1, double[] x2, double[] y3)
	{
		if (x1 == null)
		{
			__x1 = new double[0];
		}
		else
		{
			__x1 = x1;
		}
		if (y1 == null)
		{
			__y1 = new double[0];
		}
		else
		{
			__y1 = y1;
		}
		if (x2 == null)
		{
			x2 = new double[0];
		}
		else
		{
			__x2 = x2;
		}
		if (y3 == null)
		{
			y3 = new double[0];
		}
		else
		{
			__y3 = y3;
		}
		// The lengths of the overlapping arrays are required to be same
		if (__x1.Length != __y1.Length)
		{
			throw new System.ArgumentException("Independent and dependent arrays are not the same length.");
		}
		// Basic statistics will be calculated lazily (if requested)
	}

	/// <summary>
	/// Return the maximum data value for the independent array in the N1 sample, or null if not analyzed. </summary>
	/// <returns> the maximum data value for the independent array in the N1 sample, or null if not analyzed. </returns>
	public virtual double? getMaxX1()
	{
		double? maxX1 = __maxX1;
		if (maxX1 == null)
		{
			maxX1 = MathUtil.max(getX1());
			setMaxX1(maxX1);
		}
		return maxX1;
	}

	/// <summary>
	/// Return the maximum data value for the dependent array in the N1 sample, or null if not analyzed. </summary>
	/// <returns> the maximum data value for the dependent array in the N1 sample, or null if not analyzed. </returns>
	public virtual double? getMaxY1()
	{
		double? maxY1 = __maxY1;
		if (maxY1 == null)
		{
			maxY1 = MathUtil.max(getY1());
			setMaxY1(maxY1);
		}
		return maxY1;
	}

	/// <summary>
	/// Return the mean for the independent array in the N1 + N2 sample, or null if not analyzed. </summary>
	/// <returns> the mean for the independent array in the N1 + N2 sample, or null if not analyzed. </returns>
	public virtual double? getMeanX()
	{
		double? meanX = __meanX;
		if (meanX == null && getX().Length > 0)
		{
			meanX = MathUtil.mean(getX());
			setMeanX(meanX);
		}
		return meanX;
	}

	/// <summary>
	/// Return the mean for the independent array in the N1 sample, or null if not analyzed. </summary>
	/// <returns> the mean for the independent array in the N1 sample, or null if not analyzed. </returns>
	public virtual double? getMeanX1()
	{
		double? meanX1 = __meanX1;
		if (meanX1 == null && getX1().Length > 0)
		{
			meanX1 = MathUtil.mean(getX1());
			setMeanX1(meanX1);
		}
		return meanX1;
	}

	/// <summary>
	/// Return the mean for the independent array in the N2 sample, or null if not analyzed. </summary>
	/// <returns> the mean for the independent array in the N2 sample, or null if not analyzed. </returns>
	public virtual double? getMeanX2()
	{
		double? meanX2 = __meanX2;
		if (meanX2 == null && getX2().Length > 0)
		{
			meanX2 = MathUtil.mean(getX2());
			setMeanX2(meanX2);
		}
		return meanX2;
	}

	/// <summary>
	/// Return the mean for the dependent array in the N1 + N2 sample, or null if not analyzed. </summary>
	/// <returns> the mean for the dependent array in the N1 + N2 sample, or null if not analyzed. </returns>
	public virtual double? getMeanY()
	{
		double? meanY = __meanY;
		if (meanY == null && getY().Length > 0)
		{
			meanY = MathUtil.mean(getY());
			setMeanY(meanY);
		}
		return meanY;
	}

	/// <summary>
	/// Return the mean for the dependent array in the N1 sample, or null if not analyzed. </summary>
	/// <returns> the mean for the dependent array in the N1 sample, or null if not analyzed. </returns>
	public virtual double? getMeanY1()
	{
		double? meanY1 = __meanY1;
		if (meanY1 == null && getY1().Length > 0)
		{
			meanY1 = MathUtil.mean(getY1());
			setMeanY1(meanY1);
		}
		return meanY1;
	}

	/// <summary>
	/// Return the minimum data value for the independent array in the N1 sample, or null if not analyzed. </summary>
	/// <returns> the minimum data value for the independent array in the N1 sample, or null if not analyzed. </returns>
	public virtual double? getMinX1()
	{
		double? minX1 = __minX1;
		if (minX1 == null)
		{
			minX1 = MathUtil.min(getX1());
			setMinX1(minX1);
		}
		return minX1;
	}

	/// <summary>
	/// Return the minimum data value for the dependent array in the N1 sample, or null if not analyzed. </summary>
	/// <returns> the minimum data value for the dependent array in the N1 sample, or null if not analyzed. </returns>
	public virtual double? getMinY1()
	{
		double? minY1 = __minY1;
		if (minY1 == null)
		{
			minY1 = MathUtil.min(getY1());
			setMinY1(minY1);
		}
		return minY1;
	}

	/// <summary>
	/// Return the full data size (N1 + N2). </summary>
	/// <returns> the number of data points in the full set (N1 + N2) </returns>
	public virtual int getN()
	{
		if (__N == -1)
		{
			//not yet calculated....
			__N = getN1() + getN2();
		}
		return __N;
	}

	/// <summary>
	/// Return the size of the overlapping arrays. </summary>
	/// <returns> the number of data points that overlap </returns>
	public virtual int getN1()
	{
		if (__N1 == -1)
		{
			//not yet calculated....
			if (__x1 == null)
			{
				__N1 = 0;
			}
			else
			{
				__N1 = __x1.Length;
			}
		}
		return __N1;
	}

	/// <summary>
	/// Return the size of the non-overlapping independent (X2) array. </summary>
	/// <returns> the number of data points that do not overlap in the X array </returns>
	public virtual int getN2()
	{
		if (__N2 == -1)
		{
			//not yet calculated....
			if (__x2 == null)
			{
				__N2 = 0;
			}
			else
			{
				__N2 = __x2.Length;
			}
		}
		return __N2;
	}

	/// <summary>
	/// Return the size of the non-overlapping dependent array (Y3). </summary>
	/// <returns> the number of data points that do not overlap in the Y array </returns>
	public virtual int getN3()
	{
		if (__N3 == -1)
		{
			//not yet calculated....
			if (__y3 == null)
			{
				__N3 = 0;
			}
			else
			{
				__N3 = __y3.Length;
			}
		}
		return __N3;
	}

	/// <summary>
	/// Return the size of the original Y data array (Y). </summary>
	/// <returns> the number of data points in the original Y array </returns>
	public virtual int getNY()
	{
		if (__NY == -1)
		{
			//not yet calculated....
			if (__y == null)
			{
				__NY = 0;
			}
			else if (__y.Length == 0)
			{
				//not yet calculated
				__NY = getY().Length;
			}
			else
			{
				__NY = __y.Length;
			}
		}
		return __NY;
	}

	/// <summary>
	/// Return the skew for the dependent array in the N1 + N2 sample, or null if not analyzed. </summary>
	/// <returns> the skew for the dependent array in the N1 + N2 sample, or null if not analyzed. </returns>
	public virtual double? getSkewY()
	{
		double? skewY = __skewY;
		try
		{
			if (skewY == null)
			{
				skewY = MathUtil.skew(getY().Length,getY());
				setSkewY(skewY);
			}
		}
		catch (System.ArgumentException)
		{
			//problem calculating it....
			skewY = Double.NaN;
		}
		return skewY;
	}

	/// <summary>
	/// Return the standard deviation for the independent array in the N1 + N2 sample, or null if not analyzed. </summary>
	/// <returns> the standard deviation for the independent array in the N1 + N2 sample, or null if not analyzed. </returns>
	public virtual double? getStandardDeviationX()
	{
		double? stddevX = __stddevX;
		try
		{
			if (stddevX == null && getX() != null && getX().Length > 1)
			{
				stddevX = MathUtil.standardDeviation(getX());
				setStandardDeviationX(stddevX);
			}
		}
		catch (System.ArgumentException)
		{
			//problem calculating it....
			stddevX = Double.NaN;
		}
		return stddevX;
	}

	/// <summary>
	/// Return the standard deviation for the independent array in the N1 sample, or null if not analyzed. </summary>
	/// <returns> the standard deviation for the independent array in the N1 sample, or null if not analyzed. </returns>
	public virtual double? getStandardDeviationX1()
	{
		double? stddevX1 = __stddevX1;
		try
		{
			if (stddevX1 == null && getX1() != null && getX1().Length > 1)
			{
				stddevX1 = MathUtil.standardDeviation(getX1());
				setStandardDeviationX1(stddevX1);
			}
		}
		catch (System.ArgumentException)
		{
			//problem calculating it....
			stddevX1 = Double.NaN;
		}
		return stddevX1;
	}

	/// <summary>
	/// Return the standard deviation for the independent array in the N2 sample, or null if not analyzed. </summary>
	/// <returns> the standard deviation for the independent array in the N2 sample, or null if not analyzed. </returns>
	public virtual double? getStandardDeviationX2()
	{
		double? stddevX2 = __stddevX2;
		try
		{
			if (stddevX2 == null && getX2() != null && getX2().Length > 1)
			{
				stddevX2 = MathUtil.standardDeviation(getX2());
				setStandardDeviationX2(stddevX2);
			}
		}
		catch (System.ArgumentException)
		{
			//problem calculating it....
			stddevX2 = Double.NaN;
		}
		return stddevX2;
	}

	/// <summary>
	/// Return the standard deviation for the dependent array in the N1 + N2 sample, or null if not analyzed. </summary>
	/// <returns> the standard deviation for the dependent array in the N1 + N2 sample, or null if not analyzed. </returns>
	public virtual double? getStandardDeviationY()
	{
		double? stddevY = __stddevY;
		try
		{
			if (stddevY == null && getY() != null && getY().Length > 1)
			{
				stddevY = MathUtil.standardDeviation(getY());
				setStandardDeviationY(stddevY);
			}
		}
		catch (System.ArgumentException)
		{
			//problem calculating it....
			stddevY = Double.NaN;
		}
		return stddevY;
	}

	/// <summary>
	/// Return the standard deviation for the dependent array in the N1 sample, or null if not analyzed. </summary>
	/// <returns> the standard deviation for the dependent array in the N1 sample, or null if not analyzed. </returns>
	public virtual double? getStandardDeviationY1()
	{
		double? stddevY1 = __stddevY1;
		try
		{
			if (stddevY1 == null && getY1() != null && getY1().Length > 1)
			{
				stddevY1 = MathUtil.standardDeviation(getY1());
				setStandardDeviationY1(stddevY1);
			}
		}
		catch (System.ArgumentException)
		{
			stddevY1 = Double.NaN;
		}
		return stddevY1;
	}

	/// <summary>
	/// Return the full independent data array (X1 and X2). </summary>
	/// <returns> the full independent data array (X1 and X2). </returns>
	public virtual double [] getX()
	{
		double[] x = __x;
		if (x == null)
		{
			// Allocate an array that is the size of X1 and X2
			x = new double[getN()];
			Array.Copy(getX1(), 0, x, 0, getN1());
			Array.Copy(getX2(), 0, x, getN1(), getN2());
			setX(x);
		}
		return x;
	}

	/// <summary>
	/// Return the independent data array that overlaps the dependent array. </summary>
	/// <returns> the independent data array that overlaps the dependent array </returns>
	public virtual double [] getX1()
	{
		return __x1;
	}

	/// <summary>
	/// Return the independent data array that DOES NOT overlap the dependent array. </summary>
	/// <returns> the independent data array that DOES NOT overlap the dependent array </returns>
	public virtual double [] getX2()
	{
		return __x2;
	}

	/// <summary>
	/// Return the full dependent data array (Y1 and Y3). </summary>
	/// <returns> the full dependent data array (Y1 and Y3) </returns>
	public virtual double [] getY()
	{
		double[] y = __y;
		if (y.Length == 0)
		{
			// Allocate an array that is the size of Y1 and Y3
			y = new double[getN1() + getN3()];
			Array.Copy(getY1(), 0, y, 0, getN1());
			Array.Copy(getY3(), 0, y, getN1(), getN3());
			setY(y);
		}
		return y;
	}

	/// <summary>
	/// Return the dependent data array that overlaps the independent array. </summary>
	/// <returns> the dependent data array that overlaps the independent array </returns>
	public virtual double [] getY1()
	{
		return __y1;
	}

	/// <summary>
	/// Return the dependent data array that DOES NOT overlap the dependent array. </summary>
	/// <returns> the dependent data array that DOES NOT overlap the dependent array </returns>
	public virtual double [] getY3()
	{
		return __y3;
	}

	/// <summary>
	/// Set the maximum data value for the independent data in the N1 sample. </summary>
	/// <param name="maxX1"> Maximum data value for the independent data in the N1 sample. </param>
	private void setMaxX1(double? maxX1)
	{
		__maxX1 = maxX1;
	}

	/// <summary>
	/// Set the maximum data value for the dependent data in the N1 sample. </summary>
	/// <param name="maxY1"> Maximum data value for the dependent data in the N1 sample. </param>
	private void setMaxY1(double? maxY1)
	{
		__maxY1 = maxY1;
	}

	/// <summary>
	/// Set the mean for the independent data in the N1 + N2 sample. </summary>
	/// <param name="meanX"> Mean for the independent data in the N1 + N2 sample. </param>
	private void setMeanX(double? meanX)
	{
		__meanX = meanX;
	}

	/// <summary>
	/// Set the mean for the independent data in the N1 sample. </summary>
	/// <param name="meanX1"> Mean for the independent data in the N1 sample. </param>
	private void setMeanX1(double? meanX1)
	{
		__meanX1 = meanX1;
	}

	/// <summary>
	/// Set the mean for the independent data in the N2 sample. </summary>
	/// <param name="meanX2"> Mean for the independent data in the N2 sample. </param>
	private void setMeanX2(double? meanX2)
	{
		__meanX2 = meanX2;
	}

	/// <summary>
	/// Set the mean for the dependent data in the N1 + N3 sample. </summary>
	/// <param name="meanY"> Mean for the dependent data in the N1 + N3 sample. </param>
	private void setMeanY(double? meanY)
	{
		__meanY = meanY;
	}

	/// <summary>
	/// Set the mean for the dependent data in the N1 sample. </summary>
	/// <param name="meanY1"> Mean for the dependent data in the N1 sample. </param>
	private void setMeanY1(double? meanY1)
	{
		__meanY1 = meanY1;
	}

	/// <summary>
	/// Set the minimum data value for the independent data in the N1 sample. </summary>
	/// <param name="minX1"> Minimum data value for the independent data in the N1 sample. </param>
	private void setMinX1(double? minX1)
	{
		__minX1 = minX1;
	}

	/// <summary>
	/// Set the minimum data value for the dependent data in the N1 sample. </summary>
	/// <param name="minY1"> Minimum data value for the dependent data in the N1 sample. </param>
	private void setMinY1(double? minY1)
	{
		__minY1 = minY1;
	}

	/// <summary>
	/// Set the skew for the dependent data in the N1 + N3 sample. </summary>
	/// <param name="skewY"> skew for the dependent data in the N1 + N3 sample. </param>
	private void setSkewY(double? skewY)
	{
		__skewY = skewY;
	}

	/// <summary>
	/// Set the standard deviation for the independent data in the N1 + N2 sample. </summary>
	/// <param name="stddevX"> Standard deviation for the independent data in the N1 + N2 sample. </param>
	private void setStandardDeviationX(double? stddevX)
	{
		__stddevX = stddevX;
	}

	/// <summary>
	/// Set the standard deviation for the independent data in the N1 sample. </summary>
	/// <param name="stddevX1"> Standard deviation for the independent data in the N1 sample. </param>
	private void setStandardDeviationX1(double? stddevX1)
	{
		__stddevX1 = stddevX1;
	}

	/// <summary>
	/// Set the standard deviation for the independent data in the N2 sample. </summary>
	/// <param name="stddevX2"> Standard deviation for the independent data in the N2 sample. </param>
	private void setStandardDeviationX2(double? stddevX2)
	{
		__stddevX2 = stddevX2;
	}

	/// <summary>
	/// Set the standard deviation for the dependent data in the N1 + N2 sample. </summary>
	/// <param name="stddevY"> Standard deviation for the dependent data in the N1 + N2 sample. </param>
	private void setStandardDeviationY(double? stddevY)
	{
		__stddevY = stddevY;
	}

	/// <summary>
	/// Set the standard deviation for the dependent data in the N1 sample. </summary>
	/// <param name="stddevY1"> Standard deviation for the dependent data in the N1 sample. </param>
	private void setStandardDeviationY1(double? stddevY1)
	{
		__stddevY1 = stddevY1;
	}

	/// <summary>
	/// Set the X array - this is only called by the getX() method if the array has not been constructed. </summary>
	/// <param name="X"> X1 and X2. </param>
	private void setX(double[] x)
	{
		__x = x;
	}

	/// <summary>
	/// Set the Y array - this is only called by the getY() method if the array has not been constructed. </summary>
	/// <param name="y"> Y1 and Y3. </param>
	private void setY(double[] y)
	{
		__y = y;
	}

	/// <summary>
	/// Clear out the double[]s that can be recalculated, aren't necessary after filling, and take up a lot of room.
	/// Necessary because in large tests, FillMixedStation can run out of memory.
	/// </summary>
	public virtual void cleanMemory()
	{
		__x = null;
		__y = null;
		__x1 = null;
		__y1 = null;
		__x2 = null;
		__y3 = null;
	}

	/// <summary>
	/// Calculate all statistics.
	/// Exists because we want the statistics to be saved before we clear the double[]s, but we don't need them at
	/// that point, so calling get() for each of them would be overkill and annoying.
	/// </summary>
	public virtual void calculateStats()
	{
		getMeanX();
		getMeanX1();
		getMeanX2();
		getMeanY();
		getMeanY1();
		getN();
		getN1();
		getN2();
		getN3();
		getNY();
		getSkewY();
		getStandardDeviationX();
		getStandardDeviationX1();
		getStandardDeviationX2();
		getStandardDeviationY();
		getStandardDeviationY1();
	}
	}

}