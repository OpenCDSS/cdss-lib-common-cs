using System;

// GRPolygonList - GR polygon list

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
// GRPolygonList - GR polygon list
// ----------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
// ----------------------------------------------------------------------------
// History:
//
// 23 Jun 1999	Steven A. Malers	Initial version.  Copy GRPolylineList
//					and update.
// 2001-12-07	SAM, RTI		Add to copy is_selected and
//					associated_object.
// 2005-04-26	J. Thomas Sapienza, RTi	finalize() uses IOUtil.nullArray().
// ----------------------------------------------------------------------------

namespace RTi.GR
{
	using IOUtil = RTi.Util.IO.IOUtil;

	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// This class stores a list of GRPolygon, which allows storage of ESRI Arc
	/// shapes.  Data are public to
	/// increase performance during draws but the set methods should be used to set
	/// data.  Currently, the number of polygons cannot be dynamically extended.
	/// </summary>
	public class GRPolygonList : GRShape
	{

	/// <summary>
	/// Number of polygons.
	/// </summary>
	public int npolygons = 0;

	/// <summary>
	/// Total number of points.
	/// </summary>
	public int total_npts = 0;

	/// <summary>
	/// List of polygons.
	/// </summary>
	public GRPolygon[] polygons = null;

	/// <summary>
	/// Construct with zero polygons.
	/// </summary>
	public GRPolygonList() : base()
	{
		type = POLYGON_LIST;
		total_npts = 0;
		xmin = xmax = 0.0;
		ymin = ymax = 0.0;
	}

	/// <summary>
	/// Construct with zero polygons and set index. </summary>
	/// <param name="att_index"> attribute index. </param>
	public GRPolygonList(long att_index) : base(att_index)
	{
		type = POLYGON_LIST;
		total_npts = 0;
		xmin = xmax = 0.0;
		ymin = ymax = 0.0;
	}

	/// <summary>
	/// Construct with the specified number of polygons.
	/// The array space for the polygons
	/// is created but not initialized.  setPolygon should then be called to set the
	/// polygon. </summary>
	/// <param name="npolygons_set"> Number of polygons. </param>
	public GRPolygonList(int npolygons_set) : base()
	{
		type = POLYGON_LIST;
		setNumPolygons(npolygons_set);
	}

	/// <summary>
	/// Copy constructor.  A deep copy is made.
	/// </summary>
	public GRPolygonList(GRPolygonList polygonlist) : base(polygonlist.index)
	{
		type = POLYGON_LIST;
		setNumPolygons(polygonlist.npolygons);
		for (int i = 0; i < npolygons; i++)
		{
			setPolygon(i, new GRPolygon(polygonlist.polygons[i]));
		}
		// Set base class data here...
		xmin = polygonlist.xmin;
		xmax = polygonlist.xmax;
		ymin = polygonlist.ymin;
		ymax = polygonlist.ymax;
		limits_found = polygonlist.limits_found;
		is_visible = polygonlist.is_visible;
		is_selected = polygonlist.is_selected;
		associated_object = polygonlist.associated_object;
	}

	/// <summary>
	/// Returns true if the shape matches the one being compared.  Each polygon is
	/// compared.  The number of polygons must agree. </summary>
	/// <returns> true if the shape matches the one being compared.   </returns>
	public virtual bool Equals(GRPolygonList polygonlist)
	{
		if (npolygons != polygonlist.npolygons)
		{
			return false;
		}
		for (int i = 0; i < npolygons; i++)
		{
			if (!polygons[i].Equals(polygonlist.polygons[i]))
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
	~GRPolygonList()
	{
		IOUtil.nullArray(polygons);
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the number of polygons. </summary>
	/// <returns> the number of polygons. </returns>
	public virtual int getNumPolygons()
	{
		return npolygons;
	}

	/// <summary>
	/// Returns a polygon from the array or null if outside the bounds of the array.
	/// Each polygon is compared.  The number of polygons must agree. </summary>
	/// <returns> a polygon from the array or null if outside the bounds of the array. </returns>
	/// <param name="i"> index position in polygon array (starting at zero). </param>
	public virtual GRPolygon getPolygon(int i)
	{
		if ((i < 0) || (i > (npolygons - 1)))
		{
			return null;
		}
		else
		{
			return polygons[i];
		}
	}

	/// <summary>
	/// Reinitialize the polygons array to the specified size.  The polygon data must
	/// then be re-set. </summary>
	/// <param name="npolygons_set"> Number of polygons to size the polygons array. </param>
	public virtual void setNumPolygons(int npolygons_set)
	{
		try
		{
			polygons = new GRPolygon[npolygons_set];
			npolygons = npolygons_set;
			xmin = xmax = ymin = ymax = 0.0;
			limits_found = false;
		}
		catch (Exception)
		{
			Message.printWarning(2, "GRPolygonList.setNumPolygons", "Error allocating memory for " + npolygons_set + " polygons.");
		}
	}

	/// <summary>
	/// Set the polygon at an index.  It is assumed that the number of polygons has
	/// already
	/// been specified, thus allocating space for the polygons.  A reference to the
	/// given polygon is saved, not a copy of the data. </summary>
	/// <param name="i"> Array position for polygon (starting at zero). </param>
	/// <param name="polygon"> Polygon to set (null polygons are allowed). </param>
	public virtual void setPolygon(int i, GRPolygon polygon)
	{
		if ((i < 0) || (i > (npolygons - 1)))
		{
			return;
		}
		polygons[i] = polygon;
		if (!limits_found)
		{
			// Set the limits...
			xmin = polygon.xmin;
			xmax = polygon.xmax;
			ymin = polygon.ymin;
			ymax = polygon.ymax;
			limits_found = true;
		}
		else
		{
			if (polygon.xmax > xmax)
			{
				xmax = polygon.xmax;
			}
			if (polygon.xmin < xmin)
			{
				xmin = polygon.xmin;
			}
			if (polygon.ymax > ymax)
			{
				ymax = polygon.ymax;
			}
			if (polygon.ymin < ymin)
			{
				ymin = polygon.ymin;
			}
		}
	}

	} // End of GRPolygonList

}