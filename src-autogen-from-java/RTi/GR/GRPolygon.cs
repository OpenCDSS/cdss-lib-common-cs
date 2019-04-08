using System;
using System.Text;

// GRPolygon - GR polygon class

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
// GRPolygon - GR polygon class
// ----------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
// ----------------------------------------------------------------------------
// History:
//
// 09 Jul 1996	Steven A. Malers	Initial version.
// 21 Jun 1999	SAM, RTi		Update to make data public to increase
//					performance.  Update to set bounds on
//					data.  Add finalize, equals.
// 2001-12-07	SAM, RTI		Add to copy is_selected and
//					associated_object.
// 2005-04-26	J. Thomas Sapienza, RTi	finalize() now uses IOUtil.nullArray().
// ----------------------------------------------------------------------------

namespace RTi.GR
{
	using IOUtil = RTi.Util.IO.IOUtil;

	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// This class stores a sequence of closing points.  Data are public to
	/// increase performance during draws but the set methods should be used to set
	/// data.  Currently, the number of points cannot be dynamically extended.
	/// </summary>
	public class GRPolygon : GRShape
	{

	/// <summary>
	/// Number of points.
	/// </summary>
	public int npts = 0;

	/// <summary>
	/// List of points.
	/// </summary>
	public GRPoint[] pts = null;

	/// <summary>
	/// Construct with zero points.
	/// </summary>
	public GRPolygon() : base()
	{
		type = POLYGON;
		npts = 0;
		xmin = xmax = 0.0;
		ymin = ymax = 0.0;
	}

	/// <summary>
	/// Constructor.  Construct with zero points and set index. </summary>
	/// <param name="att_index"> attribute index. </param>
	public GRPolygon(long att_index) : base(att_index)
	{
		type = POLYGON;
		npts = 0;
		xmin = xmax = 0.0;
		ymin = ymax = 0.0;
	}

	/// <summary>
	/// Constructor.
	/// Construct with the specified number of points.  The array space for the points
	/// is created but not initialized.  setPoint should then be called to set the
	/// points. </summary>
	/// <param name="npts_set"> Number of points. </param>
	public GRPolygon(int npts_set) : base()
	{
		type = POLYGON;
		setNumPoints(npts_set);
	}

	/// <summary>
	/// Copy constructor.  A deep copy is made.
	/// </summary>
	public GRPolygon(GRPolygon polygon) : base(polygon.index)
	{
		type = POLYGON;
		setNumPoints(polygon.npts);
		for (int i = 0; i < npts; i++)
		{
			setPoint(i, new GRPoint(polygon.pts[i]));
		}
		// Set base class data here...
		xmin = polygon.xmin;
		xmax = polygon.xmax;
		ymin = polygon.ymin;
		ymax = polygon.ymax;
		limits_found = polygon.limits_found;
		is_visible = polygon.is_visible;
		is_selected = polygon.is_selected;
		associated_object = polygon.associated_object;
	}

	/// <summary>
	/// Construct from a GRPolyline.  A deep copy is made. </summary>
	/// <param name="polyline"> GRPolyline to be copied.  An extra closing point is added. </param>
	public GRPolygon(GRPolyline polyline) : base(polyline.index)
	{
		type = POLYGON;
		setNumPoints(polyline.npts + 1);
		int nm1 = npts - 1;
		for (int i = 0; i < nm1; i++)
		{
			setPoint(i, new GRPoint(polyline.pts[i]));
		}
		// Add a point to close the polygon...
		pts[nm1] = new GRPoint(polyline.pts[0]);
		// Set base class data here...
		xmin = polyline.xmin;
		xmax = polyline.xmax;
		ymin = polyline.ymin;
		ymax = polyline.ymax;
		limits_found = polyline.limits_found;
		is_visible = polyline.is_visible;
	}

	/// <summary>
	/// Returns true if the shape matches the one being compared.  Each point is
	/// compared.  The number of points must agree. </summary>
	/// <returns> true if the shape matches the one being compared.   </returns>
	public virtual bool Equals(GRPolygon polygon)
	{
		if (npts != polygon.npts)
		{
			return false;
		}
		for (int i = 0; i < npts; i++)
		{
			if (!pts[i].Equals(polygon.pts[i]))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Finalize before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~GRPolygon()
	{
		IOUtil.nullArray(pts);
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the number of points. </summary>
	/// <returns> the number of points. </returns>
	public virtual int getNumPoints()
	{
		return npts;
	}

	/// <summary>
	/// Returns a point from the array or null if outside the bounds of the array.
	/// A reference to the point is returned.
	/// Reference the public data directly to speed performance. </summary>
	/// <returns> a point from the array or null if outside the bounds of the array.
	/// A reference to the point is returned. </returns>
	/// <param name="i"> index position in point array (starting at zero). </param>
	public virtual GRPoint getPoint(int i)
	{
		if ((i < 0) || (i > (npts - 1)))
		{
			return null;
		}
		else
		{
			return pts[i];
		}
	}

	/// <summary>
	/// Returns the x coordinate for a point or zero if the array bounds is exceeded.
	/// Reference the public data directly to speed performance. </summary>
	/// <returns> the x coordinate for a point or zero if the array bounds is exceeded. </returns>
	public virtual double getX(int i)
	{
		if ((i < 0) || (i > (npts - 1)))
		{
			return 0.0;
		}
		else
		{
			return pts[i].x;
		}
	}

	/// <summary>
	/// Returns the y coordinate for a point or zero if the arrray bounds is exceeded.
	/// Reference the public data directly to speed performance. </summary>
	/// <returns> the y coordinate for a point or zero if the array bounds is exceeded. </returns>
	public virtual double getY(int i)
	{
		if ((i < 0) || (i > (npts - 1)))
		{
			return 0.0;
		}
		else
		{
			return pts[i].y;
		}
	}

	/// <summary>
	/// Reinitialize the points array to the specified size.  You must reset the point
	/// data. </summary>
	/// <param name="npts_set"> Number of points to size the points array. </param>
	public virtual void setNumPoints(int npts_set)
	{
		try
		{
			pts = new GRPoint[npts_set];
			npts = npts_set;
			xmin = xmax = ymin = ymax = 0.0;
			limits_found = false;
		}
		catch (Exception)
		{
			Message.printWarning(2, "GRPolygon.setNumPoints", "Error allocating array for " + npts_set + " points.");
		}
	}

	/// <summary>
	/// Set the point at an index.  It is assumed that the number of points has already
	/// been specified, thus allocating space for the points.  A reference to the
	/// given point is saved, not a copy of the data. </summary>
	/// <param name="i"> Array position for point (starting at zero). </param>
	/// <param name="pt"> Point to set (null points are allowed). </param>
	public virtual void setPoint(int i, GRPoint pt)
	{
		if ((i < 0) || (i > (npts - 1)))
		{
			return;
		}
		pts[i] = pt;
		if (!limits_found)
		{
			// Set the limits...
			xmin = xmax = pt.x;
			ymin = ymax = pt.y;
			limits_found = true;
		}
		else
		{
			if (pt.x > xmax)
			{
				xmax = pt.x;
			}
			if (pt.x < xmin)
			{
				xmin = pt.x;
			}
			if (pt.y > ymax)
			{
				ymax = pt.y;
			}
			if (pt.y < ymin)
			{
				ymin = pt.y;
			}
		}
	}

	/// <summary>
	/// Set the point at an index.  It is assumed that the number of points has already
	/// been specified, thus allocating space for the points.  A reference to the
	/// given point is saved, not a copy of the data. </summary>
	/// <param name="i"> Array position for point (starting at zero). </param>
	/// <param name="x"> x-position of point to set. </param>
	/// <param name="y"> y-position of point to set. </param>
	public virtual void setPoint(int i, double x, double y)
	{
		if ((i < 0) || (i > (npts - 1)))
		{
			return;
		}
		else
		{
			pts[i].setXY(x, y);
		}
		if (!limits_found)
		{
			// Set the limits...
			xmin = xmax = x;
			ymin = ymax = y;
			limits_found = true;
		}
		else
		{
			if (x > xmax)
			{
				xmax = x;
			}
			if (x < xmin)
			{
				xmin = x;
			}
			if (y > ymax)
			{
				ymax = y;
			}
			if (y < ymin)
			{
				ymin = y;
			}
		}
	}

	/// <summary>
	/// Return string representation of polygon, consisting of point coordinates
	/// separated by newlines.  This is suitable for debugging.  Example:<br>
	/// 1,2<br>
	/// 2,3<br>
	/// 3,1<br> </summary>
	/// <returns> string representation of polygon, consisting of point coordinates
	/// separated by new lines.   </returns>
	public override string ToString()
	{
		StringBuilder b = new StringBuilder();
		for (int i = 0; i < pts.Length; i++)
		{
			b.Append("" + pts[i].x + "," + pts[i].y + "\n");
		}
		return b.ToString();
	}

	} // End of GRPolygon class

}