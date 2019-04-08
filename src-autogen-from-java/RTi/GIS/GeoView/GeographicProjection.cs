﻿using System;

// GeographicProjection - class for geographic projection

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
// GeographicProjection.java
// ---------------------------------------------------------------------------- 
// Copyright:  See the COPYRIGHT file.
// ---------------------------------------------------------------------------- 
// History:
//
// 2001		Steven A Malers, RTi	Add this projection to allow on-the-fly
//					projections.
// 2001-12-10	SAM, RTi		Add getKilometersForUnit() to support
//					scaling.
// ---------------------------------------------------------------------------- 

namespace RTi.GIS.GeoView
{
	using GRPoint = RTi.GR.GRPoint;

	/// <summary>
	/// The Geographic projection corresponds to latitude and longitude.  Currently this
	/// class does not handle variations in datums and is used more as a place-holder
	/// (i.e., to know when a geographic projection is being used).
	/// </summary>
	public class GeographicProjection : GeoProjection
	{

	/// <summary>
	/// This projection does not do any conversions but acts as a place-holder for
	/// geographic data.
	/// </summary>
	public GeographicProjection() : base("Geographic")
	{
	}

	/// <summary>
	/// Finalize and clean up. </summary>
	/// <exception cref="Throwable"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~GeographicProjection()
	{
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Get the number of kilometers for a unit of the projection grid.  The
	/// point that is used can be reused if necessary to increase performance.
	/// <b>This is currently a rough estimate!</b> </summary>
	/// <param name="p"> As input, specifies the location (in HRAP units) at which to determine the scale. </param>
	/// <param name="reuse_point"> Indicates whether the point that is passed in should be
	/// re-used for the output (doing so saves memory). </param>
	public override GRPoint getKilometersForUnit(GRPoint p, bool reuse_point)
	{ // Don't really know the right formula yet but estimate:
		// Earth radius = 6378 KM
		// Circumferance = 2PiR = 40,074 KM
		// KM/degree = 40,074/360 = 111.3 KM
		// Need to find the correct equation accounting for the starting coordinate.
		double xscale = 111.3 * Math.Cos(p.y * 1.745329251994328e-2);
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
	/// Project latitude and longitude to the geographic coordinate system.  This just
	/// returns the original coordinates. </summary>
	/// <returns> the projected (to longitude and latitude) points. </returns>
	/// <param name="p"> Point to project from latitude and longitude. Assumes point comes in format (lon, lat) </param>
	/// <param name="reuse_point"> Indicates whether the point that is passed in should be re-used for the output 
	/// (doing so saves memory). </param>
	public override GRPoint project(GRPoint p, bool reuse_point)
	{
		if (reuse_point)
		{
			return p;
		}
		// create a new point to return
		else
		{
			return new GRPoint(p);
		}
	}

	/// <summary>
	/// Un-project coordinates back to latitude and longitude.  This returns the same coordinates. </summary>
	/// <returns> the un-projected points.  Assumes point comes in as (longitude, latitude) </returns>
	/// <param name="p"> Point to un-project to latitude and longitude. </param>
	/// <param name="reuse_point"> Indicates whether the point that is passed in should be
	/// re-used for the output (doing so saves memory). </param>
	public override GRPoint unProject(GRPoint p, bool reuse_point)
	{
		if (reuse_point)
		{
			return p;
		}
		else
		{
			return new GRPoint(p);
		}
	}

	}

}