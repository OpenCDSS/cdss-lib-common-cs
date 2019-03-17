// GRPoint2M - class to store a single 3D point with an optional 4th "measure"

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
	/// This class stores a single 3D point with an optional 4th "measure".
	/// Data are public but the set methods should be called to set data.
	/// </summary>
	public class GRPointZM : GRPoint
	{

	/// <summary>
	/// Third dimension value.
	/// </summary>
	public double z;

	/// <summary>
	/// Additional measure value.
	/// </summary>
	public double m;

	// TODO SAM 2010-12-23 Maybe should initialize to NaN.
	/// <summary>
	/// Construct and initialize to (0,0,0,0).
	/// </summary>
	public GRPointZM() : base(0.0, 0.0)
	{
		type = POINT_ZM;
		z = 0.0;
		m = 0.0;
	}

	/// <summary>
	/// Construct given an (X,Y,Z,M) point. </summary>
	/// <param name="xset"> X-coordinate </param>
	/// <param name="yset"> Y-coordinate </param>
	/// <param name="zset"> Z-coordinate </param>
	/// <param name="mset"> measure value </param>
	public GRPointZM(double xset, double yset, double zset, double mset) : base(xset, yset)
	{
		type = POINT_ZM;
		z = zset;
		m = mset;
		limits_found = true;
	}

	/// <summary>
	/// Construct given the attribute lookup key and an (X,Y) pair. </summary>
	/// <param name="xset"> X-coordinate. </param>
	/// <param name="yset"> Y-coordinate. </param>
	/// <param name="attkey"> Attribute lookup key. </param>
	public GRPointZM(long attkey, double xset, double yset, double zset, double mset) : base(attkey, xset, yset)
	{
		type = POINT_ZM;
		z = zset;
		m = mset;
		limits_found = true;
	}

	/// <summary>
	/// Copy constructor. </summary>
	/// <param name="point"> Point to copy. </param>
	public GRPointZM(GRPointZM point) : base(point)
	{
		type = POINT_ZM;
		z = point.z;
		m = point.m;
	}

	/// <summary>
	/// Returns true if the x, y, z coordinates for the shapes are equal. </summary>
	/// <returns> true if the x, y, z coordinates for the shapes are equal. </returns>
	public virtual bool Equals(GRPointZM pt)
	{
		if ((pt.x == x) && (pt.y == y) && (pt.z == z))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Returns true if the x, y, and z coordinates for the shapes are equal. </summary>
	/// <param name="xpt"> x coordinate to compare </param>
	/// <param name="ypt"> y coordinate to compare </param>
	/// <param name="zpt"> z coordinate to compare </param>
	/// <returns> true if the x, y, and z coordinates for the shapes are equal. </returns>
	public virtual bool Equals(double xpt, double ypt, double zpt)
	{
		if ((xpt == x) && (ypt == y) && (zpt == z))
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
	~GRPointZM()
	{
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}


	/// <summary>
	/// Returns the measure value.  Access the public data directly to speed performance. </summary>
	/// <returns> The measure value. </returns>
	public virtual double getM()
	{
		return m;
	}

	/// <summary>
	/// Returns the z coordinate.  Access the public data directly to speed performance. </summary>
	/// <returns> the Z-coordinate.   </returns>
	public virtual double getZ()
	{
		return z;
	}

	/// <summary>
	/// Set the X and Y-coordinates. </summary>
	/// <param name="xset"> X-coordinate to set. </param>
	/// <param name="yset"> Y-coordinate to set. </param>
	public virtual void setXYZ(double xset, double yset, double zset)
	{
		setXY(xset, yset);
		z = zset;
	}

	/// <summary>
	/// Set the X and Y-coordinates. </summary>
	/// <param name="xset"> X-coordinate to set. </param>
	/// <param name="yset"> Y-coordinate to set. </param>
	public virtual void setXYZM(double xset, double yset, double zset, double mset)
	{
		setXY(xset, yset);
		z = zset;
		m = mset;
	}

	/// <summary>
	/// Returns a String representation of the point in the format "(x,y,z,m)". </summary>
	/// <returns> A string representation of the point in the format "(x,y,z,m)". </returns>
	public override string ToString()
	{
		return "(" + x + "," + y + "," + z + "," + m + ")";
	}

	}

}