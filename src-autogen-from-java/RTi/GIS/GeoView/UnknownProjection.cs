﻿// UnknownProjection - class to indicate unknown projection

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
// UnknownProjection.java
// ---------------------------------------------------------------------------- 
// Copyright:  See the COPYRIGHT file.
// ---------------------------------------------------------------------------- 
// History:
//
// 2001 Steven A Malers, RTi	Add this projection to allow on-the-fly
//				projections.
// ---------------------------------------------------------------------------- 

namespace RTi.GIS.GeoView
{
	using GRPoint = RTi.GR.GRPoint;

	/// <summary>
	/// The unknown projection can be used as a place-holder when a projection is not
	/// know.  Many times for a GeoView project no projections will be defined so
	/// a place-holder is needed just to do comparisons.
	/// </summary>
	public class UnknownProjection : GeoProjection
	{

	/// <summary>
	/// This projection does not do any conversions and acts as a place-holder for
	/// projections.
	/// </summary>
	public UnknownProjection() : base("Unknown")
	{
	}

	/// <summary>
	/// Finalize and clean up. </summary>
	/// <exception cref="Throwable"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~UnknownProjection()
	{
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// This just returns the original coordinates.  If a comparison of projections is
	/// made, this method will likely never be called. </summary>
	/// <returns> the projected points. </returns>
	/// <param name="p"> Point to project from latitude and longitude. Assumes
	/// point comes in format (lon, lat) </param>
	/// <param name="reuse_point"> Indicates whether the point that 
	/// is passed in should be re-used for the output 
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
	/// Un-project coordinates back to latitude and longitude.  This returns the same
	/// coordinates. </summary>
	/// <returns> the un-projected points. </returns>
	/// <param name="p"> Point to un-project to latitude and longitude. </param>
	/// <param name="reuse_point"> Indicates whether the point that 
	/// is passed in should be
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

	} // End UnknownProjection

}