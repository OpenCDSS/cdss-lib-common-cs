using System;

// GRPolylineZMList - class 2 store a list of GRPolylineZM

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
	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// This class stores a list of GRPolylineZM, which allows storage of ESRI Arc
	/// shapes (with Z and measure).  Data are public to
	/// increase performance during draws but the set methods should be used to set
	/// data.  Currently, the number of polylines cannot be dynamically extended.
	/// </summary>
	public class GRPolylineZMList : GRShape
	{

	/// <summary>
	/// Number of polylines.
	/// </summary>
	public int npolylines = 0;

	/// <summary>
	/// Total number of points.
	/// </summary>
	public int total_npts = 0;

	/// <summary>
	/// List of polylines.
	/// </summary>
	public GRPolylineZM[] polylines = null;

	/// <summary>
	/// Construct with zero polylines.
	/// </summary>
	public GRPolylineZMList() : base()
	{
		type = POLYLINE_ZM_LIST;
		total_npts = 0;
		xmin = xmax = 0.0;
		ymin = ymax = 0.0;
	}

	/// <summary>
	/// Construct with zero polylines and set index. </summary>
	/// <param name="att_index"> attribute index. </param>
	public GRPolylineZMList(long att_index) : base(att_index)
	{
		type = POLYLINE_ZM_LIST;
		total_npts = 0;
		xmin = xmax = 0.0;
		ymin = ymax = 0.0;
	}

	/// <summary>
	/// Construct with the specified number of polylines.
	/// The array space for the polylines
	/// is created but not initialized.  setPolyline should then be called to set the polyline. </summary>
	/// <param name="npolylines_set"> Number of polylines. </param>
	public GRPolylineZMList(int npolylines_set) : base()
	{
		type = POLYLINE_ZM_LIST;
		setNumPolylines(npolylines_set);
	}

	/// <summary>
	/// Copy constructor.  A deep copy is made. </summary>
	/// <param name="polylinelist"> the polylineList to copy. </param>
	public GRPolylineZMList(GRPolylineZMList polylinelist) : base(polylinelist.index)
	{
		type = POLYLINE_ZM_LIST;
		setNumPolylines(polylinelist.npolylines);
		for (int i = 0; i < npolylines; i++)
		{
			setPolyline(i, new GRPolylineZM(polylinelist.polylines[i]));
		}
		// Set base class data here...
		xmin = polylinelist.xmin;
		xmax = polylinelist.xmax;
		ymin = polylinelist.ymin;
		ymax = polylinelist.ymax;
		limits_found = polylinelist.limits_found;
		is_visible = polylinelist.is_visible;
		is_selected = polylinelist.is_selected;
		associated_object = polylinelist.associated_object;
	}

	/// <summary>
	/// Returns true if the polylineList matches the one being compared.
	/// Each polyline is compared.  The number of polylines must agree. </summary>
	/// <returns> true if the polylineList matches the one being compared.   </returns>
	public virtual bool Equals(GRPolylineZMList polylinelist)
	{
		if (npolylines != polylinelist.npolylines)
		{
			return false;
		}
		for (int i = 0; i < npolylines; i++)
		{
			if (!polylines[i].Equals(polylinelist.polylines[i]))
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
	~GRPolylineZMList()
	{
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the number of polylines. </summary>
	/// <returns> the number of polylines. </returns>
	public virtual int getNumPolylines()
	{
		return npolylines;
	}

	/// <summary>
	/// Returns a polyline from the array or null if outside the bounds of the array.
	/// A reference to the polyline is returned.  Reference the public data directly to speed performance. </summary>
	/// <param name="i"> index position in polyline array (starting at zero). </param>
	/// <returns> a polyline from the array or null if outside the bounds of the array. </returns>
	public virtual GRPolylineZM getPolyline(int i)
	{
		if ((i < 0) || (i > (npolylines - 1)))
		{
			return null;
		}
		else
		{
			return polylines[i];
		}
	}

	/// <summary>
	/// Reinitialize the polylines array to the specified size.  The polyline data must be re-set. </summary>
	/// <param name="npolylines_set"> Number of polylines to size the polylines array. </param>
	public virtual void setNumPolylines(int npolylines_set)
	{
		try
		{
			polylines = new GRPolylineZM[npolylines_set];
			npolylines = npolylines_set;
			xmin = xmax = ymin = ymax = 0.0;
			limits_found = false;
		}
		catch (Exception)
		{
			Message.printWarning(2, "GRPolylineZMList.setNumPolylines", "Error allocating memory for " + npolylines_set + " polylines.");
		}
	}

	/// <summary>
	/// Set the polyline at an index.  It is assumed that the number of polylines has
	/// already been specified, thus allocating space for the polylines.  A reference 
	/// to the given polyline is saved, not a copy of the data. </summary>
	/// <param name="i"> Array position for polyline (starting at zero). </param>
	/// <param name="polyline"> Polyline to set (null polylines are allowed). </param>
	public virtual void setPolyline(int i, GRPolylineZM polyline)
	{
		if ((i < 0) || (i > (npolylines - 1)))
		{
			return;
		}
		polylines[i] = polyline;
		if (!limits_found)
		{
			// Set the limits...
			xmin = polyline.xmin;
			xmax = polyline.xmax;
			ymin = polyline.ymin;
			ymax = polyline.ymax;
			limits_found = true;
		}
		else
		{
			if (polyline.xmax > xmax)
			{
				xmax = polyline.xmax;
			}
			if (polyline.xmin < xmin)
			{
				xmin = polyline.xmin;
			}
			if (polyline.ymax > ymax)
			{
				ymax = polyline.ymax;
			}
			if (polyline.ymin < ymin)
			{
				ymin = polyline.ymin;
			}
		}
	}

	}

}