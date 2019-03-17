using System;

// GRShape - base class for all GR shape classes

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

namespace RTi.GR
{
	/// <summary>
	/// <para>
	/// GRShape is the base class for all GR shape classes.  At some point, additional
	/// information like color may be added to this class but currently shapes only
	/// store geometry information.  Set/get methods are not implemented to keep objects
	/// small and to optimize performance.  Access the data directly.
	/// </para>
	/// <para>
	/// GRShape represents common shapes for drawing and also can be used as geometry objects with
	/// geographic information system (GIS) data.  For example, see the RTi.GIS.GeoView.GeoRecord class 
	/// </para>
	/// </summary>
	public class GRShape : ICloneable
	{

	// TODO SAM 2013-12-01 Need to convert these to an enumeration and cross-reference with Well Known Text geometries
	/// <summary>
	/// Types of shapes.  Where there is compatibility with ESRI shapes, use the
	/// ESRI shape number.  Values below 100 are reserved for internal GR use.
	/// </summary>
	public const sbyte UNKNOWN = 0;

	/// <summary>
	/// Corresponds to GRArc shape type.
	/// </summary>
	public const sbyte ARC = 23;

	/// <summary>
	/// Corresponds to GRLocatorArc.
	/// </summary>
	public const sbyte LOCATOR_ARC = 25;

	/// <summary>
	/// Corresponds to GRGrid (regular or irregular grid).
	/// </summary>
	public const sbyte GRID = 22;

	/// <summary>
	/// Corresponds to GRLimits.
	/// </summary>
	public const sbyte LIMITS = 24;

	/// <summary>
	/// Corresponds to GRPoint (single point).
	/// </summary>
	public const sbyte POINT = 1;

	/// <summary>
	/// Corresponds to GRPointZM (single point with Z and measure).
	/// </summary>
	public const sbyte POINT_ZM = 11;

	/// <summary>
	/// Corresponds to GRPolyline (line segment).
	/// </summary>
	public const sbyte POLYLINE = 20;

	/// <summary>
	/// Corresponds to GRPolylineZM (line segment with Z and measure).
	/// </summary>
	public const sbyte POLYLINE_ZM = 99;

	/// <summary>
	/// Corresponds to GRPolylineList (list of line segments, suitable for storing to ESRI Arc).
	/// </summary>
	public const sbyte POLYLINE_LIST = 3;

	/// <summary>
	/// Corresponds to GRPolylineZMList (list of line segments, suitable for storing to ESRI Arc), with Z and measure.
	/// </summary>
	public const sbyte POLYLINE_ZM_LIST = 13;

	/// <summary>
	/// Corresponds to GRPolygon.
	/// </summary>
	public const sbyte POLYGON = 21;

	/// <summary>
	/// Corresponds to GRPolygonList (list of polygons, suitable for storing to ESRI Polygon).
	/// </summary>
	public const sbyte POLYGON_LIST = 5;

	/// <summary>
	/// Corresponds to GRPolypoint (multiple points, suitable for storing to ESRI MultPoint).
	/// </summary>
	public const sbyte POLYPOINT = 8;

	/// <summary>
	/// GRShape data are public to optimize performance.
	/// </summary>

	/// <summary>
	/// Object to associate with the shape.  Use this, for example, to relate to a
	/// database or application object.
	/// </summary>
	public object associated_object;

	/// <summary>
	/// Index to attribute to GIS/DB information.
	/// </summary>
	public long index;

	/// <summary>
	/// Shape type (see shape types defined in this class).
	/// </summary>
	public sbyte type;

	/// <summary>
	/// Minimum x data coordinate.
	/// </summary>
	public double xmin;

	/// <summary>
	/// Maximum x data coordinate.
	/// </summary>
	public double xmax;

	/// <summary>
	/// Minimum y data coordinate.
	/// </summary>
	public double ymin;

	/// <summary>
	/// Maximum y data coordinate.
	/// </summary>
	public double ymax;

	/// <summary>
	/// True if the shape is selected.  False if not.  This is used by higher-level
	/// code to select shapes from displays, etc.  The default is not selected.
	/// </summary>
	public bool is_selected = false;

	/// <summary>
	/// True if the shape is visible.  False if not.  This is used to hide shapes
	/// to increase performance or make displays less busy.
	/// </summary>
	public bool is_visible = true;

	/// <summary>
	/// The following should only need to be used by derived classes and indicates
	/// if the limits have been found.  The default limits are 0.0, 0.0.
	/// </summary>
	public bool limits_found = false;

	/// <summary>
	/// Construct without assigning the attribute lookup key.  The shape will be
	/// visible and the limits are set to zeros.
	/// </summary>
	public GRShape()
	{
		index = -1;
		is_visible = true;
		type = UNKNOWN;
		xmin = xmax = ymin = ymax = 0.0;
		associated_object = null;
	}

	/// <summary>
	/// Construct and set the attribute table lookup index.  The shape will be
	/// visible and the limits are set to zeros. </summary>
	/// <param name="lookup_index"> Attribute lookup key. </param>
	public GRShape(long lookup_index)
	{
		index = lookup_index;
		type = 0;
		xmin = xmax = ymin = ymax = 0.0;
		is_visible = true;
		is_selected = false;
		associated_object = null;
	}

	/// <summary>
	/// Clones the object. </summary>
	/// <returns> a clone of this object. </returns>
	public virtual object clone()
	{
		try
		{
			return (GRShape)base.clone();
		}
		catch (Exception)
		{
			return null;
		}
	}

	/// <summary>
	/// Determine whether a shape contains another shape.  The minimum and maximum
	/// coordinates of the shape are used to make selections.  This should work in a
	/// course fashion for all shapes; however, if a shape is non-rectangular, then
	/// a more complex method must be implemented to indicate if an intersection occurs.
	/// To do so, override this method in derived classes. </summary>
	/// <param name="shape"> GRShape to evaluate. </param>
	/// <param name="contains_completely"> If true, then the shape s must be completely within
	/// this shape to return true. </param>
	/// <returns> true if this shape contains the specified shape </returns>
	public virtual bool contains(GRShape shape, bool contains_completely)
	{ // Check the overall limits...
		if ((shape.xmax < xmin) || (shape.xmin > xmax) || (shape.ymax < ymin) || (shape.ymin > ymax))
		{
			// Definitely not in...
			return false;
		}
		if ((shape.xmin >= xmin) && (shape.xmax <= xmax) && (shape.ymin >= ymin) && (shape.ymax <= ymax))
		{
			// Totally in...
			return true;
		}
		if (contains_completely)
		{
			return false;
		}
		else
		{
			return true;
		}
	}

	/// <summary>
	/// Return the attribute lookup index. </summary>
	/// @deprecated Use the public data directly to increase performance. 
	/// <returns> The attribute lookup index.  Use the public data directly to increase performance. </returns>
	public virtual long getIndex()
	{
		return index;
	}

	}

}