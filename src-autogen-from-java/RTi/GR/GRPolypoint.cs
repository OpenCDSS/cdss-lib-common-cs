using System;

// GRPolypoint - GR Polypoint class

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

// -----------------------------------------------------------------------------
// GRPolypoint - GR Polypoint class
// -----------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
// -----------------------------------------------------------------------------
// History:
//
// 01 Nov 1998	Steven A. Malers, RTi	Initial version.
// 2001-12-07	SAM, RTi		Make data public to speed performance,
//					similar to other classes.
// 2005-04-26	J. Thomas Sapienza, RTi	Added finalize().
// ----------------------------------------------------------------------------

namespace RTi.GR
{
	using IOUtil = RTi.Util.IO.IOUtil;

	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// GR Polypoint class.
	/// </summary>
	public class GRPolypoint : GRShape
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
	public GRPolypoint() : base()
	{
		type = POLYPOINT;
		xmin = xmax = 0.0;
		ymin = ymax = 0.0;
		npts = 0;
	}

	/// <summary>
	/// Construct with the specified number of points.  The array space for the points
	/// is created but not initialized.  setPoint() should be called to set the points. </summary>
	/// <param name="npts"> Number of points. </param>
	public GRPolypoint(int npts) : base()
	{
		type = POLYPOINT;
		setNumPoints(npts);
	}

	/// <summary>
	/// Constuct and set the shape index and number of points. </summary>
	/// <param name="index"> Attribute index. </param>
	/// <param name="npts"> Number of points. </param>
	public GRPolypoint(long index, int npts) : base(index)
	{
		type = POLYPOINT;
		setNumPoints(npts);
	}

	/// <summary>
	/// Copy constructor.  A deep copy is made. </summary>
	/// <param name="polypoint"> the polypoint to duplicate. </param>
	public GRPolypoint(GRPolypoint polypoint) : base(polypoint.index)
	{
		type = POLYPOINT;
		setNumPoints(polypoint.npts);
		for (int i = 0; i < npts; i++)
		{
			setPoint(i, new GRPoint(polypoint.pts[i]));
		}
		// Set base class data here...
		xmin = polypoint.xmin;
		xmax = polypoint.xmax;
		ymin = polypoint.ymin;
		ymax = polypoint.ymax;
		limits_found = polypoint.limits_found;
		is_visible = polypoint.is_visible;
		is_selected = polypoint.is_selected;
		associated_object = polypoint.associated_object;
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~GRPolypoint()
	{
		IOUtil.nullArray(pts);
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Reinitialize the points array to the specified size.  The point data must
	/// be re-set. </summary>
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
			Message.printWarning(2, "GRPolypoint.setNumPoints", "Error allocating array for " + npts_set + " points.");
		}
	}

	/// <summary>
	/// Set the point at an index in the list. </summary>
	/// <param name="i"> Point index. </param>
	/// <param name="pt"> Point to set. </param>
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
	/// Set the point at an index in the list. </summary>
	/// <param name="i"> Point index. </param>
	/// <param name="x"> X coordinate of point. </param>
	/// <param name="y"> Y coordinate of point. </param>
	public virtual void setPoint(int i, double x, double y)
	{
		if ((i < 0) || (i > (npts - 1)))
		{
			return;
		}
		pts[i].setXY(x, y);
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

	} // End of GRPolypoint

}