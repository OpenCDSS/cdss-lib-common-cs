using System;

// GRLimits - class to store the limits of a rectangular area

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
	/// This class stores the limits of a rectangular area.  The coordinate systems
	/// can be in either direction.  This allows directions to be swapped during
	/// projection (e.g., to correct for downward Y-axis for screen graphics).
	/// Note that all data are specified in terms of the left, right, top, and bottom
	/// coordinates and that minimum, maximum, center, width, and height are computed
	/// from these values.  <b>This object should not be treated as a drawing primitive
	/// but can be passed as a shape if necessary.</b>
	/// </summary>
	public class GRLimits : GRShape, ICloneable
	{

	/// <summary>
	/// Indicates that the limits are in device units.  These settings are usually
	/// used as parameters to methods in other classes.
	/// </summary>
	public const int DEVICE = 1;

	/// <summary>
	/// Indicates that the limits are for a unit square.
	/// </summary>
	public const int UNIT = 2;

	/// <summary>
	/// Bottom-most Y.
	/// </summary>
	protected internal double _bottom_y;
	/// <summary>
	/// Left-most X.
	/// </summary>
	protected internal double _left_x;
	/// <summary>
	/// Right-most X.
	/// </summary>
	protected internal double _right_x;
	/// <summary>
	/// Top-most Y.
	/// </summary>
	protected internal double _top_y;

	/// <summary>
	/// Overall height.
	/// </summary>
	protected internal double _height;
	/// <summary>
	/// Overall width.
	/// </summary>
	protected internal double _width;

	/// <summary>
	/// Center X.
	/// </summary>
	protected internal double _center_x;
	/// <summary>
	/// Center Y.
	/// </summary>
	protected internal double _center_y;

	/// <summary>
	/// Maximum X.
	/// </summary>
	protected internal double _max_x;
	/// <summary>
	/// Maximum Y.
	/// </summary>
	protected internal double _max_y;
	/// <summary>
	/// Minimum X.
	/// </summary>
	protected internal double _min_x;
	/// <summary>
	/// Minimum Y.
	/// </summary>
	protected internal double _min_y;

	/// <summary>
	/// Constructor.  Initialize to a (0,0) to (1,1) square.
	/// </summary>
	public GRLimits()
	{
		initialize();
	}

	/// <summary>
	/// Constructor.  Build a GRLimits. using the given width and height.  
	/// The origin is (0.0). </summary>
	/// <param name="width"> Width of limits. </param>
	/// <param name="height"> Height of limits. </param>
	public GRLimits(double width, double height)
	{
		_left_x = 0.0;
		_right_x = width;
		_bottom_y = 0.0;
		_top_y = height;
		type = LIMITS;
		reset();
	}

	/// <summary>
	/// Constructor.  Use the points for the corners. </summary>
	/// <param name="left_bottom_pt"> Left, bottom point. </param>
	/// <param name="right_top_pt"> Right, top point. </param>
	public GRLimits(GRPoint left_bottom_pt, GRPoint right_top_pt)
	{
		_left_x = left_bottom_pt.getX();
		_bottom_y = left_bottom_pt.getY();
		_right_x = right_top_pt.getX();
		_top_y = right_top_pt.getY();
		type = LIMITS;
		reset();
	}

	/// <summary>
	/// Constructor.  Use the coordinates for the corners, as integers. </summary>
	/// <param name="left_x"> Left X-coordinate. </param>
	/// <param name="bottom_y"> Bottom Y-coordinate. </param>
	/// <param name="right_x"> Right X-coordinate. </param>
	/// <param name="top_y"> Top Y-coordinate. </param>
	public GRLimits(int left_x, int bottom_y, int right_x, int top_y)
	{
		_left_x = (double)left_x;
		_bottom_y = (double)bottom_y;
		_right_x = (double)right_x;
		_top_y = (double)top_y;
		type = LIMITS;
		reset();
	}

	/// <summary>
	/// Constructor.  Use the coordinates for the corners. </summary>
	/// <param name="left_x"> Left X-coordinate. </param>
	/// <param name="bottom_y"> Bottom Y-coordinate. </param>
	/// <param name="right_x"> Right X-coordinate. </param>
	/// <param name="top_y"> Top Y-coordinate. </param>
	public GRLimits(double left_x, double bottom_y, double right_x, double top_y)
	{
		_left_x = left_x;
		_bottom_y = bottom_y;
		_right_x = right_x;
		_top_y = top_y;
		type = LIMITS;
		reset();
	}

	public GRLimits(Rectangle r)
	{
		_left_x = r.x;
		_bottom_y = r.y;
		_right_x = r.x + r.width;
		_top_y = r.y + r.height;
		type = LIMITS;
		reset();
	}

	/// <summary>
	/// Copy constructor. </summary>
	/// <param name="limits"> GRlimits to copy. </param>
	public GRLimits(GRLimits limits)
	{
		type = LIMITS;
		if (limits != null)
		{
			_bottom_y = limits.getBottomY();
			_left_x = limits.getLeftX();
			_right_x = limits.getRightX();
			_top_y = limits.getTopY();
			reset();
		}
	}

	/// <summary>
	/// Clones this object. </summary>
	/// <returns> a clone of this object. </returns>
	public override object clone()
	{
		try
		{
			return (GRLimits)base.clone();
		}
		catch (Exception)
		{
			return null;
		}
	}

	/// <summary>
	/// Indicate whether the limits contain the point in question. </summary>
	/// <param name="pt"> GRPoint of interest. </param>
	/// <returns> true if the GRLimits region contains the specified point.
	/// The orientation of the GRLimits axes can be in either direction. </returns>
	public virtual bool contains(GRPoint pt)
	{
		if ((((pt.x >= _left_x) && (pt.x <= _right_x)) || ((pt.x <= _left_x) && (pt.x >= _right_x))) && (((pt.y >= _bottom_y) && (pt.y <= _top_y)) || ((pt.y <= _bottom_y) && (pt.y >= _top_y))))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Indicate whether the limits contain the point in question.
	/// The orientation of the GRLimits axes can be in either direction. </summary>
	/// <param name="x"> X-coordinate of interest. </param>
	/// <param name="y"> Y-coordinate of interest. </param>
	/// <returns> true if the GRLimits region contains the specified point. </returns>
	public virtual bool contains(double x, double y)
	{
		if ((((x >= _left_x) && (x <= _right_x)) || ((x <= _left_x) && (x >= _right_x))) && (((y >= _bottom_y) && (y <= _top_y)) || ((y <= _bottom_y) && (y >= _top_y))))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Indicate whether the limits contain the region in question.
	/// Currently the orientation of both regions needs to be min x on the left and min y on the bottom. </summary>
	/// <param name="xmin"> the lowest x value of the region </param>
	/// <param name="ymin"> the lowest y value of the region </param>
	/// <param name="xmax"> the highest x value of the region </param>
	/// <param name="ymax"> the highest y value of the region </param>
	/// <param name="contains_completely"> If true, the region must completely be contained.  If
	/// false, the region must only intersect.  This parameter is not currently checked. </param>
	/// <returns> true if the GRLimits region contains the specified region. </returns>
	public virtual bool contains(double xmin, double ymin, double xmax, double ymax, bool contains_completely)
	{
		if ((xmax < _left_x) || (xmin > _right_x) || (ymax < _bottom_y) || (ymin > _top_y))
		{
			return false;
		}
		return true;
	}

	/// <summary>
	/// Indicate whether the limits contain the point in question, considering only the x-axis.
	/// The orientation of the GRLimits x-axis can be in either direction. </summary>
	/// <param name="x"> X-coordinate of interest. </param>
	/// <returns> true if the GRLimits x-axis contains the specified point. </returns>
	public virtual bool containsX(double x)
	{
		if (((x >= _left_x) && (x <= _right_x)) || ((x <= _left_x) && (x >= _right_x)))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Indicate whether the limits contain the point in question, considering only the y-axis.
	/// The orientation of the GRLimits y-axis can be in either direction. </summary>
	/// <param name="y"> Y-coordinate of interest. </param>
	/// <returns> true if the GRLimits y-axis contains the specified point. </returns>
	public virtual bool containsY(double y)
	{
		if (((y >= _bottom_y) && (y <= _top_y)) || ((y <= _bottom_y) && (y >= _top_y)))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Returns true if the limits are the same as those passed in.  The corner
	/// coordinates are checked but if coordinates systems for the limits are not the
	/// same then the limits will not match. </summary>
	/// <returns> true if the limits are the same as those passed in.   </returns>
	public virtual bool Equals(GRLimits limits)
	{
		if (limits.getLeftX() != _left_x)
		{
			return false;
		}
		if (limits.getRightX() != _right_x)
		{
			return false;
		}
		if (limits.getBottomY() != _bottom_y)
		{
			return false;
		}
		if (limits.getTopY() != _top_y)
		{
			return false;
		}
		return true;
	}

	/// <summary>
	/// Finalize before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~GRLimits()
	{
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the left X-coordinate. </summary>
	/// <returns> The left X-coordinate. </returns>
	public virtual double getLeftX()
	{
		return _left_x;
	}

	/// <summary>
	/// Return the right X-coordinate. </summary>
	/// <returns> The right X-coordinate. </returns>
	public virtual double getRightX()
	{
		return _right_x;
	}

	/// <summary>
	/// Return the bottom Y-coordinate. </summary>
	/// <returns> The bottom Y-coordinate. </returns>
	public virtual double getBottomY()
	{
		return _bottom_y;
	}

	/// <summary>
	/// Return the top Y-coordinate. </summary>
	/// <returns> The top Y-coordinate. </returns>
	public virtual double getTopY()
	{
		return _top_y;
	}

	/// <summary>
	/// Return the maximum X-coordinate. </summary>
	/// <returns> The maximum X-coordinate. </returns>
	public virtual double getMaxX()
	{
		return _max_x;
	}

	/// <summary>
	/// Return the maximum Y-coordinate. </summary>
	/// <returns> The maximum Y-coordinate. </returns>
	public virtual double getMaxY()
	{
		return _max_y;
	}

	/// <summary>
	/// Return the minimum X-coordinate. </summary>
	/// <returns> The minimum X-coordinate. </returns>
	public virtual double getMinX()
	{
		return _min_x;
	}

	/// <summary>
	/// Return the minimum Y-coordinate. </summary>
	/// <returns> The minimum Y-coordinate. </returns>
	public virtual double getMinY()
	{
		return _min_y;
	}

	/// <summary>
	/// Return the width. </summary>
	/// <returns> The width. </returns>
	public virtual double getWidth()
	{
		return _width;
	}

	/// <summary>
	/// Return the height. </summary>
	/// <returns> The height. </returns>
	public virtual double getHeight()
	{
		return _height;
	}

	/// <summary>
	/// Return the center X-coordinate. </summary>
	/// <returns> The center X-coordinate. </returns>
	public virtual double getCenterX()
	{
		return _center_x;
	}

	/// <summary>
	/// Return the center Y-coordinate. </summary>
	/// <returns> The center Y-coordinate. </returns>
	public virtual double getCenterY()
	{
		return _center_y;
	}

	/// <summary>
	/// Increase the size of the limits.  The left and right limits are widened by
	/// increase_x/2.  The top and bottom limits are widened by increase_y/2. </summary>
	/// <param name="increase_x"> Amount to increase width. </param>
	/// <param name="increase_y"> Amount to increase height. </param>
	public virtual void increase(double increase_x, double increase_y)
	{
		if (_left_x <= _right_x)
		{
			_left_x -= increase_x / 2.0;
			_right_x += increase_x / 2.0;
		}
		else
		{
			_right_x -= increase_x / 2.0;
			_left_x += increase_x / 2.0;
		}
		if (_bottom_y <= _top_y)
		{
			_bottom_y -= increase_x / 2.0;
			_top_y += increase_x / 2.0;
		}
		else
		{
			_top_y -= increase_x / 2.0;
			_bottom_y += increase_x / 2.0;
		}
		reset();
	}

	/// <summary>
	/// Initialize the data.
	/// </summary>
	private void initialize()
	{
		_bottom_y = 0.0;
		_left_x = 0.0;
		_right_x = 1.0;
		_top_y = 1.0;
		reset();
	}

	/// <summary>
	/// Return the maximum combined extents of the current limits and another GRLimits.
	/// All coordinates are compared and the maximum bounds are used.
	/// Therefore, orientation of the limits is ignored.  A new GRLimits instance is returned. </summary>
	/// <returns> the maximum combined extents of the current limits and another GRLimits instance. </returns>
	/// <param name="other"> Other GRLimits instance. </param>
	public virtual GRLimits max(GRLimits other)
	{
		return max(other, false);
	}

	/// <summary>
	/// Return the maximum of two GRLimits. All coordinates are compared and the 
	/// maximum bounds are used.  Therefore, orientation of the limits is ignored. </summary>
	/// <returns> the maximum combined extents of the current limits and another GRLimits instance. </returns>
	/// <param name="other"> Other GRLimits instance. </param>
	/// <param name="reuse_limits"> If true, the limits will be reused; if false, a new
	/// instance will be created. </param>
	public virtual GRLimits max(GRLimits other, bool reuse_limits)
	{
		if (other == null)
		{
			if (reuse_limits)
			{
				return (this);
			}
			else
			{
				return new GRLimits(this);
			}
		}
		return max(other._min_x, other._min_y, other._max_x, other._max_y, reuse_limits);
	}

	/// <summary>
	/// Return the maximum of two GRLimits. </summary>
	/// <returns> the maximum combined extents of the current limits and another GRLimits
	/// instance.  All coordinates are compared and the maximum bounds are used.
	/// Therefore, orientation of the limits is ignored. </returns>
	/// <param name="xmin"> Minimum X value to check. </param>
	/// <param name="ymin"> Minimum Y value to check. </param>
	/// <param name="xmax"> Maximum X value to check. </param>
	/// <param name="ymax"> Maximum Y value to check. </param>
	/// <param name="reuseLimits"> If true, the limits will be reused; if false, a new instance will be created. </param>
	public virtual GRLimits max(double xmin, double ymin, double xmax, double ymax, bool reuseLimits)
	{
		if (_min_x < xmin)
		{
			xmin = _min_x;
		}
		if (_min_y < ymin)
		{
			ymin = _min_y;
		}
		if (_max_x > xmax)
		{
			xmax = _max_x;
		}
		if (_max_y > ymax)
		{
			ymax = _max_y;
		}
		if (reuseLimits)
		{
			_left_x = xmin;
			_bottom_y = ymin;
			_right_x = xmax;
			_top_y = ymax;
			reset();
			return this;
		}
		else
		{
			return new GRLimits(xmin, ymin, xmax, ymax);
		}
	}

	/// <summary>
	/// Reset the secondary data, including minimum and maximum, width and height;
	/// </summary>
	private void reset()
	{
		_center_x = (_left_x + _right_x) / 2.0;
		_center_y = (_bottom_y + _top_y) / 2.0;
		if (_left_x <= _right_x)
		{
			_min_x = _left_x;
			_max_x = _right_x;
		}
		else
		{
			_min_x = _right_x;
			_max_x = _left_x;
		}
		if (_bottom_y <= _top_y)
		{
			_min_y = _bottom_y;
			_max_y = _top_y;
		}
		else
		{
			_min_y = _top_y;
			_max_y = _bottom_y;
		}
		_width = _max_x - _min_x;
		_height = _max_y - _min_y;
		// Base class...
		xmin = _left_x;
		xmax = _right_x;
		ymin = _bottom_y;
		ymax = _top_y;
	}

	/// <summary>
	/// Reverse the y-axis values (flip).  For example, this might be used to
	/// cause a graph to plot with Y increasing downward. </summary>
	/// <returns> the updated GRLimits instance </returns>
	public virtual GRLimits reverseY()
	{
		double temp = _top_y;
		_top_y = _bottom_y;
		_bottom_y = temp;
		reset();
		return this;
	}

	/// <summary>
	/// Set the left X-coordinate. </summary>
	/// <param name="left_x"> The left X-coordinate. </param>
	public virtual void setLeftX(double left_x)
	{
		_left_x = left_x;
		reset();
	}

	/// <summary>
	/// Set the right X-coordinate. </summary>
	/// <param name="right_x"> The right X-coordinate. </param>
	public virtual void setRightX(double right_x)
	{
		_right_x = right_x;
		reset();
	}

	/// <summary>
	/// Set the top Y-coordinate. </summary>
	/// <param name="top_y"> The top Y-coordinate. </param>
	public virtual void setTopY(double top_y)
	{
		_top_y = top_y;
		reset();
	}

	/// <summary>
	/// Set the bottom Y-coordinate. </summary>
	/// <param name="bottom_y"> The bottom Y-coordinate. </param>
	public virtual void setBottomY(double bottom_y)
	{
		_bottom_y = bottom_y;
		reset();
	}

	/// <summary>
	/// Set the limits using the corner points </summary>
	/// <param name="left_x"> Left X-coordinate. </param>
	/// <param name="bottom_y"> Bottom Y-coordinate. </param>
	/// <param name="right_x"> Right X-coordinate. </param>
	/// <param name="top_y"> Top Y-coordinate. </param>
	public virtual void setLimits(double left_x, double bottom_y, double right_x, double top_y)
	{
		_left_x = left_x;
		_bottom_y = bottom_y;
		_right_x = right_x;
		_top_y = top_y;
		reset();
	}

	/// <summary>
	/// Return a string representation of the object.
	/// </summary>
	public override string ToString()
	{
		return "(" + _left_x + "," + _bottom_y + ") (" + _right_x + "," + _top_y + ") (" + _center_x + "," + _center_y + ") " + _width + "x" + _height;
	}

	}

}