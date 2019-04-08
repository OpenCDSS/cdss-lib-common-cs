// GRPoint - GR point 

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
// GRPoint - GR point 
// ----------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
// ----------------------------------------------------------------------------
// History:
//
// 14 Aug 1997	Steven A. Malers	Initial Java version.
// 21 Jun 1999	Steven A. Malers, RTi	Make data public to increase
//					performance.  Inline code in
//					constructor.  Add max/min data for
//					parent.
// 2001-12-07	SAM, RTI		Add to copy is_selected and
//					associated_object.
// ----------------------------------------------------------------------------

namespace RTi.GR
{
	/// <summary>
	/// This class stores a single 2D point.  Data are public but the set methods should
	/// be called to set data.
	/// </summary>
	public class GRPoint : GRShape
	{

	// Data members...

	public double x, y; // Just one coordinate pair.

	// TODO SAM 2010-12-23 Maybe should initialize to NaN.
	/// <summary>
	/// Construct and initialize to (0,0).
	/// </summary>
	public GRPoint() : base()
	{
		type = POINT;
		x = xmin = xmax = 0.0;
		y = ymin = ymax = 0.0;
	}

	/// <summary>
	/// Construct given an (X,Y) pair. </summary>
	/// <param name="xset"> X-coordinate. </param>
	/// <param name="yset"> Y-coordinate. </param>
	public GRPoint(double xset, double yset) : base()
	{
		type = POINT;
		x = xmax = xmin = xset;
		y = ymax = ymin = yset;
		limits_found = true;
	}

	/// <summary>
	/// Construct given the attribute lookup key and an (X,Y) pair. </summary>
	/// <param name="xset"> X-coordinate. </param>
	/// <param name="yset"> Y-coordinate. </param>
	/// <param name="attkey"> Attribute lookup key. </param>
	public GRPoint(long attkey, double xset, double yset) : base(attkey)
	{
		type = POINT;
		x = xmax = xmin = xset;
		y = ymax = ymin = yset;
		limits_found = true;
	}

	/// <summary>
	/// Copy constructor. </summary>
	/// <param name="point"> Point to copy. </param>
	public GRPoint(GRPoint point) : base(point.index)
	{
		type = POINT;
		x = xmin = xmax = point.x;
		y = ymin = ymax = point.y;
		// Base class does not have a constructor for this yet...
		is_visible = point.is_visible;
		is_selected = point.is_selected;
		associated_object = point.associated_object;
		limits_found = point.limits_found;
	}

	/// <summary>
	/// Returns true if the x and y coordinates for the shapes are equal. </summary>
	/// <returns> true if the x and y coordinates for the shapes are equal. </returns>
	public virtual bool Equals(GRPoint pt)
	{
		if ((pt.x == x) && (pt.y == y))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Returns true if the x and y coordinates for the shapes are equal. </summary>
	/// <param name="xpt"> x coordinate to compare to. </param>
	/// <param name="ypt"> y coordinate to compare to. </param>
	/// <returns> true if the x and y coordinates for the shapes are equal. </returns>
	public virtual bool Equals(double xpt, double ypt)
	{
		if ((xpt == x) && (ypt == y))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Finalize before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~GRPoint()
	{
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the x coordinate.  Access the public data directly to speed performance. </summary>
	/// <returns> the X-coordinate.   </returns>
	public virtual double getX()
	{
		return x;
	}

	/// <summary>
	/// Returns the Y-coordinate.  Access the public data directly to speed performance. </summary>
	/// <returns> The Y-coordinate. </returns>
	public virtual double getY()
	{
		return y;
	}

	/// <summary>
	/// Set the X-coordinate. </summary>
	/// <param name="xset"> X-coordinate to set. </param>
	/// @deprecated Use the version that sets both coordinates because setting one
	/// makes it difficult to know if limits have been completely set. 
	public virtual void setX(double xset)
	{
		x = xmin = xmax = xset;
	}

	/// <summary>
	/// Set the Y-coordinate. </summary>
	/// <param name="yset"> Y-coordinate to set. </param>
	/// @deprecated Use the version that sets both coordinates because setting one
	/// makes it difficult to know if limits have been completely set. 
	public virtual void setY(double yset)
	{
		y = ymin = ymax = yset;
	}

	/// <summary>
	/// Set the X and Y-coordinates. </summary>
	/// <param name="xset"> X-coordinate to set. </param>
	/// <param name="yset"> Y-coordinate to set. </param>
	public virtual void setXY(double xset, double yset)
	{
		x = xmin = xmax = xset;
		y = ymin = ymax = yset;
		limits_found = true;
	}

	/// <summary>
	/// Returns a String representation of the point in the format "(x,y)". </summary>
	/// <returns> A string representation of the point in the format "(x,y)". </returns>
	public override string ToString()
	{
		return "(" + x + "," + y + ")";
	}

	}

}