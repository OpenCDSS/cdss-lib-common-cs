using System;

// GRArc - GR arc (ellipsoid) 

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
// GRArc - GR arc (ellipsoid) 
// ----------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
// ----------------------------------------------------------------------------
// History:
//
// 2001-12-08	Steven A. Malers	Initial Java version.
// 2001-12-09	SAM			Change primitive X, Y to a GRPoint.
//					This takes up a little more overhead;
//					however, it is easier to do manipulation
//					because many other methods work on
//					GRPoint (e.g., the GeoProjection) code.
// ----------------------------------------------------------------------------

namespace RTi.GR
{
	/// <summary>
	/// The GRArc class stores an ellipsoid shape.  Data are public but the set methods
	/// should be called to set data so that coordinate limits can be computed.
	/// </summary>
	public class GRArc : GRShape
	{

	/// <summary>
	/// Angle to start drawing, in degrees counter-clockwise from East.
	/// </summary>
	public double angle1;

	/// <summary>
	/// Angle to stop drawing, in degrees counter-clockwise from East.
	/// </summary>
	public double angle2;

	/// <summary>
	/// Radius in X direction.
	/// </summary>
	public double xradius;

	/// <summary>
	/// Radius in Y direction.
	/// </summary>
	public double yradius;

	/// <summary>
	/// Coordinates of the center.
	/// </summary>
	public GRPoint pt;

	/// <summary>
	/// Construct and initialize to (0,0).
	/// </summary>
	public GRArc() : base()
	{
		type = ARC;
		xmax = xmin = xradius = yradius = 0.0;
		pt = new GRPoint();
		angle1 = 0.0;
		angle2 = 0.0;
	}

	/// <summary>
	/// Construct given necessary data pair. </summary>
	/// <param name="pt_set"> Coordinates of center. </param>
	/// <param name="xradius_set"> Radius in X direction. </param>
	/// <param name="yradius_set"> Radius in Y direction. </param>
	/// <param name="angle1_set"> Starting angle for draw. </param>
	/// <param name="angle2_set"> Ending angle for draw. </param>
	public GRArc(GRPoint pt_set, double xradius_set, double yradius_set, double angle1_set, double angle2_set) : base()
	{
		type = ARC;
		pt = pt_set;
		xradius = xradius_set;
		yradius = yradius_set;
		angle1 = angle1_set;
		angle2 = angle2_set;
		xmin = pt.x - xradius;
		xmax = pt.x + xradius;
		ymin = pt.y - yradius;
		ymax = pt.y + yradius;
		limits_found = true;
	}

	/// <summary>
	/// Construct given necessary data pair. </summary>
	/// <param name="x_set"> X-coordinate of center. </param>
	/// <param name="y_set"> Y-coordinate of center. </param>
	/// <param name="xradius_set"> Radius in X direction. </param>
	/// <param name="yradius_set"> Radius in Y direction. </param>
	/// <param name="angle1_set"> Starting angle for draw. </param>
	/// <param name="angle2_set"> Ending angle for draw. </param>
	public GRArc(double x_set, double y_set, double xradius_set, double yradius_set, double angle1_set, double angle2_set) : base()
	{
		type = ARC;
		pt = new GRPoint(x_set, y_set);
		xradius = xradius_set;
		yradius = yradius_set;
		angle1 = angle1_set;
		angle2 = angle2_set;
		xmin = pt.x - xradius;
		xmax = pt.x + xradius;
		ymin = pt.y - yradius;
		ymax = pt.y + yradius;
		limits_found = true;
	}

	/// <summary>
	/// Construct given the attribute lookup key and shape data. </summary>
	/// <param name="attkey"> Attribute lookup key. </param>
	/// <param name="pt_set"> Coordinates of center. </param>
	/// <param name="xradius_set"> Radius in X direction. </param>
	/// <param name="yradius_set"> Radius in Y direction. </param>
	/// <param name="angle1_set"> Starting angle for draw. </param>
	/// <param name="angle2_set"> Ending angle for draw. </param>
	public GRArc(long attkey, GRPoint pt_set, double xradius_set, double yradius_set, double angle1_set, double angle2_set) : base(attkey)
	{
		type = ARC;
		pt = pt_set;
		xradius = xradius_set;
		yradius = yradius_set;
		angle1 = angle1_set;
		angle2 = angle2_set;
		xmin = pt.x - xradius;
		xmax = pt.x + xradius;
		ymin = pt.y - yradius;
		ymax = pt.y + yradius;
		limits_found = true;
	}

	/// <summary>
	/// Copy constructor. </summary>
	/// <param name="arc"> GRArc to copy. </param>
	public GRArc(GRArc arc) : base(arc.index)
	{
		type = ARC;
		pt = new GRPoint(arc.pt);
		xradius = arc.xradius;
		yradius = arc.yradius;
		angle1 = arc.angle1;
		angle2 = arc.angle2;
		// Base class does not have a constructor for this yet...
		is_visible = arc.is_visible;
		is_selected = arc.is_selected;
		associated_object = arc.associated_object;
		limits_found = arc.limits_found;
	}

	/// <summary>
	/// Determine whether an arc contains a shape.
	/// Currently only GRPoint shapes are supported and the check only uses the X radius
	/// for the check.  Additional capability will be added later. </summary>
	/// <param name="shape"> Shape to check. </param>
	/// <returns> true if the arc contains the shape, false if it does not </returns>
	public override bool contains(GRShape shape, bool contains_completely)
	{
		if (shape.type == GRShape.POINT)
		{
			GRPoint pt2 = (GRPoint)shape;
			double dx = pt2.x - pt.x;
			double dy = pt2.y - pt.y;
			if ((Math.Sqrt(dx * dx + dy * dy)) < xradius)
			{
				return true;
			}
			pt2 = null;
			return false;
		}
		// For other shapes would need to loop through the coordinates and do
		// something similar.  For now return the more course method in the
		// base class...
		return base.contains(shape, contains_completely);
	}

	/// <summary>
	/// Determine whether shapes are equal.  The center coordinates, radii, and angles
	/// are checked. </summary>
	/// <param name="arc"> the arc to compare against this arc </param>
	/// <returns> true if the shapes are equal. </returns>
	public virtual bool Equals(GRArc arc)
	{
		if ((arc.pt == pt) && (arc.xradius == xradius) && (arc.yradius == yradius) && (arc.angle1 == angle1) && (arc.angle2 == angle2))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Finalize before garbage collection. </summary>
	/// <exception cref="Throwable"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~GRArc()
	{
		pt = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Set the center point.  It is assumed that the radius is in the same units as
	/// the center point so that the shape extents can be properly computed.
	/// A reference to the given point is saved, not a copy of the data. </summary>
	/// <param name="pt_set"> Point to set (null points are not allowed). </param>
	public virtual void setPoint(GRPoint pt_set)
	{
		pt = pt_set;
		xmin = pt.x - xradius;
		xmax = pt.x + xradius;
		ymin = pt.y - yradius;
		ymax = pt.y + yradius;
	}

	/// <summary>
	/// Return a string representation of the arc. </summary>
	/// <returns> A string representation of the arc in the format
	/// "x,y,xradius,yradius,angle1,angle2". </returns>
	public override string ToString()
	{
		return "GRArc(" + pt.x + "," + pt.y + "," + xradius + "," + yradius +
			"," + angle1 + "," + angle2 + ")";
	}

	} // End GRArc class

}