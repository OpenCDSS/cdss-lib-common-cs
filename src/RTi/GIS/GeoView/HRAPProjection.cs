using System;

// HRAPProjection - implement National Weather Service HRAP projection

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
// HRAPProjection - implement National Weather Service HRAP projection
// ----------------------------------------------------------------------------
// Copyright:  See the COPYRIGHT file.
// ----------------------------------------------------------------------------
// History:
//
// 09 Aug 2001	Morgan Sheedy,RTi	Initial implementation.
// 10 Aug 2001	AMS,RTi			Changed GRPoint methods used from using
//					grpoint.getX() and .setXY(x,y) to
//					simply: grpoint.x= Xvalue to improve
//					performance.
// 15 Aug 2001	Steve Malers, RTi	Changed the value for longitude to be
//					negative since current data is in USA.
// 2001-10-15	SAM, RTi		Remove unnecessary imports for classes
//					that are not used.  Clean up javadoc.
// 2001-11-27	SAM, RTi		Set the max/min coordinates when
//					projecting.
// 2001-12-09	SAM, RTi		Add getKilometersForUnit().
//----------------------------------------------------------------------------

namespace RTi.GIS.GeoView
{
	using GRPoint = RTi.GR.GRPoint;

	/// <summary>
	/// HRAP data points are laid out in a grid with the original in the SouthWest
	/// corner.  The grid increment increases to the East in the X-direction and to the
	/// North in the Y-direction.  The conversion equation between HRAP and lat/long was
	/// obtained from the National Weather Service:
	/// http://www.nws.noaa.gov/oh/hrl/dmip/lat_lon.txt
	/// The conversions assume that points have longitude,latitude and HRAP X, HRAP Y.
	/// </summary>
	public class HRAPProjection : GeoProjection
	{

	// Earth radius used in calculations.
	internal double _earthrad = 6371.2;

	/// <summary>
	/// Constructor.
	/// </summary>
	public HRAPProjection() : base("HRAP")
	{
	}

	/// <summary>
	/// Finalize and clean up. </summary>
	/// <exception cref="Throwable"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~HRAPProjection()
	{
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Get the number of kilometers for a unit of the projection grid.  The
	/// point that is used can be reused if necessary to increase performance. </summary>
	/// <param name="p"> As input, specifies the location (in HRAP units) at which to
	/// determine the scale. </param>
	/// <param name="reuse_point"> Indicates whether the point that is passed in should be
	/// re-used for the output (doing so saves memory). </param>
	public override GRPoint getKilometersForUnit(GRPoint p, bool reuse_point)
	{ // This taken from the old RTi HMHRAPScaleAtLat() C routine - where
		// did that come from?  Apparently scale is the same in both
		// directions?  Note that the formula uses lat/long so we need to
		// convert to that from the HRAP coordinates...
		GRPoint p2 = (GRPoint)projectShape(this, GeoProjection.geographic_projection, p, false);
		double xscale = 4.7625 / ((1.0 + Math.Sin(60.0 * 1.745329251994328e-2)) / (1.0 + Math.Sin(p2.y * 1.745329251994328e-2)));
		p2 = null;
		if (reuse_point)
		{
			p.x = p.y = xscale;
			return p;
		}
		else
		{
			return new GRPoint(xscale, xscale);
		}
	}

	/// <summary>
	/// Project latitude and longitude to the HRAP coordinate system. </summary>
	/// <returns> the projected (to HRAP) points. </returns>
	/// <param name="p"> Point to project from longitude, latitude. </param>
	/// <param name="reuse_point"> Indicates whether the point that 
	/// is passed in should be re-used for the output 
	/// (doing so saves memory). </param>
	public override GRPoint project(GRPoint p, bool reuse_point)
	{ //will be retrieving HRAP values
		double hrap_x = -999.0;
		double hrap_y = -999.0;

		//get the lat and long out of the point passed in
		double rlon = 0.0;
		double rlat = 0.0;
		// HRAP needs positive longitude
		if (p.x < 0)
		{
			rlon = -p.x;
		}
		else
		{
			rlon = p.x;
		}
		rlat = p.y;

		//set variables for calc
		double pi = 3.141592654;
		double d2rad = 0.0;
		double ref_lat = 0.0;
		double ref_lon = 0.0;
		double rmesh = 0.0;
		double tlat = 0.0;
		double re = 0.0;
		double flat = 0.0;
		double flon = 0.0;
		double r = 0.0;
		double x = 0.0;
		double y = 0.0;

		d2rad = pi / 180.0;
		ref_lat = 60.0;
		ref_lon = 105.0;
		rmesh = 4.7625;
		tlat = ref_lat * d2rad;
		re = (_earthrad * (1.0 + Math.Sin(tlat))) / rmesh;
		flat = rlat * d2rad;
		flon = ((rlon + 180.0) - ref_lon) * d2rad;
		r = re * Math.Cos(flat) / (1.0 + Math.Sin(flat));
		x = r * Math.Sin(flon);
		y = r * Math.Cos(flon);

		//calc hraps x and y
		hrap_x = x + 401.0;
		hrap_y = y + 1601.0;

		//now we have a HRAP x and y (as doubles)
		//just reset point if reuse_point is true
		if (reuse_point)
		{
			p.x = hrap_x;
			p.y = hrap_y;
			p.xmax = p.x;
			p.xmin = p.x;
			p.ymax = p.y;
			p.ymin = p.y;
			return p;
		}
		//create a new point to return
		else
		{
			return new GRPoint(hrap_x,hrap_y);
		}
	}

	/// <summary>
	/// Un-project coordinates from HRAP back to longitude, latitude. </summary>
	/// <returns> the un-projected (from HRAP) points. </returns>
	/// <param name="p"> Point to un-project to longitude, latitude. </param>
	/// <param name="reuse_point"> Indicates whether the point that 
	/// is passed in should be re-used for the output (doing so saves memory). </param>
	public override GRPoint unProject(GRPoint p, bool reuse_point)
	{ //will determine lat and long points
		double rlon = 0.0;
		double rlat = 0.0;

		//get the HRAP-x and HRAP-y out of the point
		//assumes they come in as (x, y). HRAP coords are
		//ints but are converted to double for the calculations
		double hrap_x = -999.0;
		double hrap_y = -999.0;
		hrap_x = p.x;
		hrap_y = p.y;


		//set variables for calc
		double pi = 3.141592654;
		double stlon = 0.0;
		double raddeg = 0.0;
		double xmesh = 0.0;
		double tlat = 0.0;
		double x = 0.0;
		double y = 0.0;
		double rr = 0.0;
		double gi = 0.0;
		double ang = 0.0;

		stlon = 105.0;
		raddeg = 180.0 / pi;
		xmesh = 4.7625;
		tlat = 60.0 / raddeg;
		x = hrap_x - 401.0;
		y = hrap_y - 1601.0;
		rr = x * x + y * y;
		gi = ((_earthrad * (1.0 + Math.Sin(tlat))) / xmesh);
		gi = gi * gi;

		//get rlat
		rlat = Math.Asin((gi - rr) / (gi + rr)) * raddeg;

		ang = Math.Atan2(y,x) * raddeg;
		if (ang < 0)
		{
			ang = ang + 360.0;
		}

		//get rlong
		rlon = 270.0 + stlon - ang;
		if (rlon < 0)
		{
			rlon = rlon + 360.0;
		}
		if (rlon > 360.0)
		{
			rlon = rlon - 360.0;
		}

		// Assume for now that longitude should be negative since current
		// applications are in the USA.  Need to revisit if other than NW
		// hemisphere is used.

		if (reuse_point)
		{
			p.x = -rlon;
			p.y = rlat;
			p.xmax = p.x;
			p.xmin = p.x;
			p.ymax = p.y;
			p.ymin = p.y;
			return p;
		}
		//if reuse is false, create new GRPoint
		else
		{
			return new GRPoint(-rlon,rlat);
		}
	}

	} // HRAPProjection

}