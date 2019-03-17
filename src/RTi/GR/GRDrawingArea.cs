using System;
using System.Collections.Generic;
using System.Text;

// GRDrawingArea - GR drawing area base class

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
// GRDrawingArea - GR drawing area base class
// ----------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
// ----------------------------------------------------------------------------
// Notes:	(1)	This is the base class for drawing areas.  It maintains
//			all of the generic data for drawing ares (scaling
//			information, etc.).  The set/get methods handle data
//			for the class.  The drawing routines should be
//			overruled in the derived class since drawing is specific
//			to the device.
// ----------------------------------------------------------------------------
// History:
//
// 10 Aug 1997	Steven A. Malers, RTi	Initial Java version as port of C++/C
//					code.
// 28 Mar 1998	SAM, RTi		Revisit the code and implement a
//					a totally new design that is 100%
//					object oriented.
// 08 Jul 1999	SAM, RTi		Add the _global_xs and _global_ys arrays
//					for use by drawing code so that
//					memory is not abused.  _global_len_s
//					is the length of the arrays.  The
//					arrays are guaranteed to be at least
//					10 elements so that many small drawing
//					tasks such as points, rectangles, and
//					line segments can use the arrays.  The
//					arrays are totally dynamic and should
//					not be used for persistent (multi-step
//					tasks).
// 08 Nov 1999	SAM			Verify setLineDash works.
// 25 May 2000	SAM			Add getTextExtents().
// 2002-01-07	SAM, RTi		Update so that there is no real-time
//					dependence on the device.  The _graphics
//					reference and _reverse_y value from the
//					GR device is also saved here.  This
//					increases performance some but is being
//					done mainly to allow GR devices to be
//					created for Canvas and JComponent.
//					Change setFont() to take a style.
//					Change getPlotLimits() flags and add
//					support for COORD_DATA to clarify the
//					flag.  Remove the default
//					getPlotLimits() to make calls explicit.
// ----------------------------------------------------------------------------
// 2003-05-02	J. Thomas Sapienza, RTi	Made class abstract.
// 2003-05-07	JTS, RTi		Made changes following SAM's review.
// 2004-05-25	JTS, RTi		For some reason the log scaling code 
//					was not transferred over when the
//					library was redesigned last year.  
//					Reinstated the log scaling.
// 2004-06-03	JTS, RTi		
// 2005-04-20	JTS, RTi		* Added setClip().
//					* Added getClip().
// 2005-04-26	JTS, RTi		Added finalize().
// 2005-04-29	JTS, RTi		Added isDeviceAntiAliased() and
//					setDeviceAntiAlias().
// 2006-08-22	SAM, RTi		Increased debug level to 100 for X,Y
//					scaling messages.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------
// EndHeader

namespace RTi.GR
{

	using PropList = RTi.Util.IO.PropList;

	using MathUtil = RTi.Util.Math.MathUtil;
	using StandardNormal = RTi.Util.Math.StandardNormal;

	using Message = RTi.Util.Message.Message;


	/// <summary>
	/// This class is the base class for GR drawing areas, which are virtual drawing 
	/// areas within a GRDevice.  Open a GRDrawingArea and then draw to it using the
	/// GRDrawingAreaUtil class draw functions. </summary>
	/// <seealso cref= GR </seealso>
	/// <seealso cref= GRDevice </seealso>
	public abstract class GRDrawingArea
	{

	/// <summary>
	/// Flag used to indicate raw device coordinates (used by getPlotLimits() and getDataXY()).
	/// </summary>
	public const int COORD_DEVICE = 0;

	/// <summary>
	/// Flag used to indicate GR plotting coordinates (same units as COORD_DEVICE but Y-axis may be flipped).
	/// </summary>
	public const int COORD_PLOT = 1;

	/// <summary>
	/// Flag used to indicate data coordinates.
	/// </summary>
	public const int COORD_DATA = 2;

	/// <summary>
	/// Shared data arrays to minimize dynamic memory issues by drawing code.  
	/// Will be dimensioned to 10 upon initial object instantiation.  Later
	/// may increase the dimension to support polylines and polygons if necessary.
	/// </summary>
	protected internal static double[] _global_xs = null;
	protected internal static double[] _global_ys = null;
	protected internal static int _global_len_s = 0;
	protected internal static int[] _global_ixs = null;
	protected internal static int[] _global_iys = null;
	protected internal static int _global_len_is = 0;

	/// <summary>
	/// Indicates whether the data limits have been set.
	/// </summary>
	protected internal bool _dataset;
	/// <summary>
	/// Indicates whether drawing limits have been set.
	/// </summary>
	protected internal bool _drawset;
	/// <summary>
	/// Use to reverse y-axis to normal coordinates (it will be true for devices where Y is zero at the top).
	/// </summary>
	protected internal bool _reverse_y;

	/// <summary>
	/// Left-most X coordinate (data units)
	/// </summary>
	protected internal double _datax1;
	/// <summary>
	/// Right-most X coordinate (data units)
	/// </summary>
	protected internal double _datax2;
	/// <summary>
	/// Bottom-most Y coordinate (data units)
	/// </summary>
	protected internal double _datay1;
	/// <summary>
	/// Top-most Y coordinate (data units)
	/// </summary>
	protected internal double _datay2;
	/// <summary>
	/// Left-most plotting X coordinate (device units)
	/// </summary>
	protected internal double _drawx1;
	/// <summary>
	/// Right-most plotting X coordinate (device units)
	/// </summary>
	protected internal double _drawx2;
	/// <summary>
	/// Bottom-most plotting Y coordinate (device units)
	/// </summary>
	protected internal double _drawy1;
	/// <summary>
	/// Top-most plotting Y value (device units)
	/// </summary>
	protected internal double _drawy2;


	/// <summary>
	/// Last X coordinate drawn to in data units.
	/// </summary>
	protected internal double _lastx;
	/// <summary>
	/// Last X coordinate drawn to in plotting units.
	/// </summary>
	protected internal double _lastxp;
	/// <summary>
	/// Last Y coordinate drawn to in data units.
	/// </summary>
	protected internal double _lasty;
	/// <summary>
	/// Last Y coordinate drawn to in plotting units.
	/// </summary>
	protected internal double _lastyp;

	/// <summary>
	/// Linearized value corresponding to the Z-value for probability axes, the log10
	/// value for log axes, and the data value for linear axes.
	/// </summary>
	protected internal double _linearx1;
	/// <summary>
	/// Linearized value corresponding to the Z-value for probability axes, the log10
	/// value for log axes, and the data value for linear axes.
	/// </summary>
	protected internal double _linearx2;
	/// <summary>
	/// Linearized value corresponding to the Z-value for probability axes, the log10
	/// value for log axes, and the data value for linear axes.
	/// </summary>
	protected internal double _lineary1;
	/// <summary>
	/// Linearized value corresponding to the Z-value for probability axes, the log10
	/// value for log axes, and the data value for linear axes.
	/// </summary>
	protected internal double _lineary2;

	/// <summary>
	/// Use to shift Y-axis to normal coordinates.
	/// </summary>
	protected internal double _devyshift;

	/// <summary>
	/// Font height used for text.
	/// </summary>
	protected internal double _fontht;

	/// <summary>
	/// Line width.
	/// </summary>
	protected internal double _linewidth;

	/// <summary>
	/// Left-most plotting X value (device units), taking into account the aspect of the axes.
	/// </summary>
	protected internal double _plotx1;
	/// <summary>
	/// Right-most plotting X value (device units), taking into account the aspect of the axes.
	/// </summary>
	protected internal double _plotx2;
	/// <summary>
	/// Bottom-most plotting Y value (device units), taking into account the aspect of the axes.
	/// </summary>
	protected internal double _ploty1;
	/// <summary>
	/// Top-most plotting Y value (device units), taking into account the aspect of the axes.
	/// </summary>
	protected internal double _ploty2;

	/// <summary>
	/// Graphics instance to use for drawing.  This is set every time a drawing area
	/// is created or when the device's graphics are set.
	/// </summary>
	protected internal Graphics _graphics;

	/// <summary>
	/// Drawing color.
	/// </summary>
	protected internal GRColor _color;
	/// <summary>
	/// Device associated with drawing area.  This is a canvas.
	/// </summary>
	protected internal GRDevice _dev;

	/// <summary>
	/// Aspect as in GRAspect_*.
	/// </summary>
	protected internal int _aspect;
	/// <summary>
	/// Type of X axis as in GRAxis.
	/// </summary>
	protected internal int _axisx;
	/// <summary>
	/// Type of Y axis as in GRAxis.
	/// </summary>
	protected internal int _axisy;

	/// <summary>
	/// Line cap type as defined in GRDrawingAreaUtil.CAP*
	/// </summary>
	protected internal int _linecap;
	/// <summary>
	/// Line join type as defined in GRDrawingAreaUtil.JOIN*
	/// </summary>
	protected internal int _linejoin;
	/// <summary>
	/// Drawing area status as defined in GRUtil.STAT*
	/// </summary>
	protected internal int _status;

	/// <summary>
	/// Name of font to use for text.
	/// </summary>
	protected internal string _font;
	/// <summary>
	/// Drawing area name.
	/// </summary>
	protected internal string _name;

	/// <summary>
	/// Default constructor.  <b>Do not use this!</b>
	/// </summary>
	public GRDrawingArea()
	{
		string routine = "GRDrawingArea()";

		if (Message.isDebugOn)
		{
			Message.printDebug(10, routine, "Constructing using no arguments");
		}
		Message.printWarning(2, routine, "Should not use void constructor");
		//initialize ( null, "", GRAspect.TRUE, null, 0, 0, null );
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dev"> GRDevice associated with the drawing area. </param>
	/// <param name="name"> A name for the drawing area. </param>
	/// <param name="aspect"> Aspect for the axes of the drawing area. </param>
	/// <param name="draw_limits"> Drawing limits (device coordinates to attach the lower-left
	/// and upper-right corner of the drawing area). </param>
	/// <param name="units"> Units of the limits (will be converted to device units). </param>
	/// <param name="flag"> Modifier for drawing limits.  If GRLimits.UNIT, then the limits are
	/// assumed to be percentages of the device (0.0 to 1.0) and the units are not used. </param>
	/// <param name="data_limits"> Data limits associated with the lower-left and upper-right corners of the drawing area. </param>
	/// <seealso cref= GRAspect </seealso>
	public GRDrawingArea(GRDevice dev, string name, int aspect, GRLimits draw_limits, int units, int flag, GRLimits data_limits)
	{
		string routine = "GRDrawingArea(...)";

		if (Message.isDebugOn)
		{
			Message.printDebug(10, routine, "Constructing using all arguments, name=\"" + name + "\"");
		}
		initialize(dev, name, aspect, draw_limits, units, flag, data_limits);
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dev"> the device associated with the drawing area </param>
	/// <param name="props"> a PropList containing the settings for the drawing area. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public GRDrawingArea(GRDevice dev, RTi.Util.IO.PropList props) throws GRException
	public GRDrawingArea(GRDevice dev, PropList props)
	{
		string routine = "GRDrawingArea(PropList)";
		if (Message.isDebugOn)
		{
			Message.printDebug(10, routine, "Constructing using PropList");
		}
		try
		{
			initialize(dev, props);
		}
		catch (GRException e)
		{
			throw e;
		}
	}

	/// <summary>
	/// Clear the drawing area (draw in the current color).
	/// </summary>
	public abstract void clear();

	/// <summary>
	/// Set the clip on/off for the drawing area. </summary>
	/// <param name="flag"> Indicates whether clipping should be on or off. </param>
	public abstract void clip(bool flag);

	/// <summary>
	/// Comment (not really applicable other than for hard-copy) </summary>
	/// <param name="comment"> the comment to set </param>
	public abstract void comment(string comment);

	/// <summary>
	/// Draw an arc using the current color, line, etc. </summary>
	/// <param name="x"> X-coordinate of center. </param>
	/// <param name="y"> Y-coordinate of center. </param>
	/// <param name="rx"> X-radius. </param>
	/// <param name="ry"> Y-radius. </param>
	/// <param name="a1"> Initial angle to start drawing (0 is at 3 o'clock, then counter-clockwise). </param>
	/// <param name="a2"> Ending angle. </param>
	public abstract void drawArc(double x, double y, double rx, double ry, double a1, double a2);

	/// <summary>
	/// Draw compound text. </summary>
	/// <param name="text"> list of text to draw. </param>
	/// <param name="color"> the color to draw the text in. </param>
	/// <param name="x"> the x coordinate from which to start drawing the text </param>
	/// <param name="y"> the y coordinate from which to start drawing the text </param>
	/// <param name="angle"> the angle at which to draw the text </param>
	/// <param name="flag"> GRText.* value denoting where to draw the text </param>
	public abstract void drawCompoundText(IList<string> text, GRColor color, double x, double y, double angle, int flag);

	/// <summary>
	/// Draw a line. </summary>
	/// <param name="x"> X-coordinates in data units. </param>
	/// <param name="y"> Y-coordinates in data units. </param>
	public abstract void drawLine(double[] x, double[] y);

	/// <summary>
	/// Draw a polygon in the current color. </summary>
	/// <param name="npts"> the number of points in the polygon. </param>
	/// <param name="x"> array of x coordinates </param>
	/// <param name="y"> array of y coordinates </param>
	public abstract void drawPolygon(int npts, double[] x, double[] y);

	/// <summary>
	/// Draws a polyline in the current color. </summary>
	/// <param name="npts"> the number of points in the polyline </param>
	/// <param name="x"> array of x coordinates </param>
	/// <param name="y"> array of y coordinates </param>
	public abstract void drawPolyline(int npts, double[] x, double[] y);

	/// <summary>
	/// Draws a rectangle in the current color. </summary>
	/// <param name="xll"> the lower-left x coordinate </param>
	/// <param name="yll"> the lower-left y coordinate </param>
	/// <param name="width"> the width of the rectangle </param>
	/// <param name="height"> the height of the rectangle. </param>
	public virtual void drawRectangle(double xll, double yll, double width, double height)
	{
		double[] x = new double[4], y = new double[4];

		x[0] = xll;
		y[0] = yll;
		x[1] = xll + width;
		y[1] = yll;
		x[2] = x[1];
		y[2] = yll + height;
		x[3] = xll;
		y[3] = y[2];
		drawPolygon(4, x, y);
		_lastx = x[0];
		_lasty = y[0];
	}

	/// <summary>
	/// Draws text. </summary>
	/// <param name="text"> the text to draw </param>
	/// <param name="x"> the x location of the text </param>
	/// <param name="y"> the y location of the text </param>
	/// <param name="a"> the alpha value of the text </param>
	/// <param name="flag"> the GRText.* flag to determine how text is drawn </param>
	public abstract void drawText(string text, double x, double y, double a, int flag);

	/// <summary>
	/// Draws text. </summary>
	/// <param name="text"> the text to draw </param>
	/// <param name="x"> the x location of the text </param>
	/// <param name="y"> the y location of the text </param>
	/// <param name="a"> the alpha value of the text (transparency) </param>
	/// <param name="flag"> the GRText.* flag to determine how text is drawn </param>
	/// <param name="degrees"> the number of degrees to rotate the text clock-wise from east. </param>
	public abstract void drawText(string text, double x, double y, double a, int flag, double degrees);

	/// <summary>
	/// Fills an arc using the current color, line, etc. </summary>
	/// <param name="x"> X-coordinate of center. </param>
	/// <param name="y"> Y-coordinate of center. </param>
	/// <param name="rx"> X-radius. </param>
	/// <param name="ry"> Y-radius. </param>
	/// <param name="a1"> Initial angle to start drawing (0 is at 3 o'clock, then counterclockwise). </param>
	/// <param name="a2"> Ending angle. </param>
	public abstract void fillArc(double x, double y, double rx, double ry, double a1, double a2, int fillmode);

	/// <summary>
	/// Draw a polygon in the current color. </summary>
	/// <param name="npts"> the number of points in the polygon. </param>
	/// <param name="x"> array of x coordinates </param>
	/// <param name="y"> array of y coordinates </param>
	/// <param name="transparency"> transparency with which to draw the polygon </param>
	public abstract void fillPolygon(int npts, double[] x, double[] y, int transparency);

	/// <summary>
	/// Draw a polygon in the current color. </summary>
	/// <param name="npts"> the number of points in the polygon. </param>
	/// <param name="x"> array of x coordinates </param>
	/// <param name="y"> array of y coordinates </param>
	public abstract void fillPolygon(int npts, double[] x, double[] y);

	/// <summary>
	/// Fills a rectangle in the current color. </summary>
	/// <param name="xll"> the lower-left x coordinate </param>
	/// <param name="yll"> the lower-left y coordinate </param>
	/// <param name="width"> the width of the rectangle </param>
	/// <param name="height"> the height of the rectangle. </param>
	public abstract void fillRectangle(double xll, double yll, double width, double height);

	/// <summary>
	/// Fills a rectangle in the current color. </summary>
	/// <param name="limits"> GRLimits denoting the bounds of the rectangle. </param>
	public abstract void fillRectangle(GRLimits limits);

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~GRDrawingArea()
	{
		_graphics = null;
		_color = null;
		_dev = null;
		_font = null;
		_name = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Flushes the drawing.
	/// </summary>
	public abstract void flush();

	/// <summary>
	/// Returns the clipping shape on the current graphics context. </summary>
	/// <returns> the clipping shape on the current graphics context. </returns>
	public abstract Shape getClip();

	/// <summary>
	/// The the data extents of the drawing area. </summary>
	/// <param name="limits"> limits for drawing area. </param>
	public abstract GRLimits getDataExtents(GRLimits limits, int flag);

	/// <summary>
	/// Gets the data limits for the drawing area as a new copy of the data limits.
	/// </summary>
	public virtual GRLimits getDataLimits()
	{
		return new GRLimits(_datax1, _datay1, _datax2, _datay2);
	}

	/// <summary>
	/// Get the data values for device coordinates. </summary>
	/// <returns> A GRPoint with the data point. </returns>
	/// <param name="devx"> Device x-coordinate. </param>
	/// <param name="devy"> Device y-coordinate. </param>
	/// <param name="flag"> GR.COORD_DEVICE if the coordinates are originating with the device
	/// (e.g., a mouse) or GR.COORD_PLOT if the coordinates are plotting coordinates
	/// (this flag affects how the y-axis is reversed on some devices). </param>
	public abstract GRPoint getDataXY(double devx, double devy, int flag);

	/// <summary>
	/// Gets the drawing limits for the drawing area as a new copy of the limits.
	/// </summary>
	public virtual GRLimits getDrawingLimits()
	{
		return new GRLimits(_drawx1, _drawy1, _drawx2, _drawy2);
	}

	/// <summary>
	/// Return the name of the drawing area. </summary>
	/// <returns> The name of the drawing area. </returns>
	public virtual string getName()
	{
		return _name;
	}

	/// <summary>
	/// Return the plotting limits for the drawing area in plotting or data units. </summary>
	/// <param name="flag"> COORD_PLOT if the plotting units should be returned,
	/// COORD_DEVICE if the limits should be returned in device units,
	/// COORD_DATA if the limits should be returned in data units. </param>
	/// <returns> The plotting limits for the drawing area. </returns>
	public virtual GRLimits getPlotLimits(int flag)
	{
		// Get the limits in device units...

		GRLimits limits = new GRLimits(_plotx1, _ploty1, _plotx2, _ploty2);
		if (Message.isDebugOn)
		{
			Message.printDebug(10, "GR.getPlotLimits", "Plot limits in device units are " + limits);
		}

		if (flag == COORD_PLOT)
		{
			return limits;
		}
		else if (flag == COORD_DATA)
		{
			GRPoint p1 = getDataXY(_plotx1, _ploty1, COORD_PLOT);
			GRPoint p2 = getDataXY(_plotx2, _ploty2, COORD_PLOT);
			limits = new GRLimits(p1.x, p1.y, p2.x, p2.y);
			if (Message.isDebugOn)
			{
				Message.printDebug(10, "GR.getPlotLimits", "Plot limits in data units are " + limits);
			}
			return limits;
		}
		else
		{
			// Raw device units.  Correct for flipped y-axis...
			if (_dev.getReverseY())
			{
				GRLimits devlimits = _dev.getLimits();
				double maxy = devlimits.getMaxY();
				limits = new GRLimits(_drawx1, (maxy - _drawy1), _drawx2, (maxy - _drawy2));
				return limits;
			}
			else
			{
				// Return limits computed in first step...
				return limits;
			}
		}
	}

	/// <summary>
	/// Returns the extents of a string. </summary>
	/// <param name="text"> the text to get the extents for </param>
	/// <param name="flag"> GRUnits.DATA or GRUnits.DEV, indicating units for returned size. </param>
	/// <returns> the extents of a string. </returns>
	public abstract GRLimits getTextExtents(string text, int flag);

	/// <summary>
	/// Returns the units. </summary>
	/// <returns> the units. </returns>
	public virtual int getUnits()
	{
		return 0;
	}

	/// <summary>
	/// Returns the x-axis type.
	/// </summary>
	public virtual int getXAxisType()
	{
		return this._axisx;
	}

	/// <summary>
	/// Returns the x data point for the x device point. </summary>
	/// <param name="xdev"> the x device point </param>
	/// <returns> the x data point for the x device point. </returns>
	public abstract double getXData(double xdev);

	/// <summary>
	/// Returns the y-axis type.
	/// </summary>
	public virtual int getYAxisType()
	{
		return this._axisy;
	}

	/// <summary>
	/// Returns the y data point for the y device point. </summary>
	/// <param name="ydev"> the y device point </param>
	/// <returns> the y data point for the y device point. </returns>
	public abstract double getYData(double ydev);

	/// <summary>
	/// Initialize the drawing area data.
	/// Rely on default fonts, etc., for now. </summary>
	/// <param name="dev"> the GRDevice associated with this drawing area </param>
	/// <param name="name"> the name of the drawing area </param>
	/// <param name="aspect"> the aspect of the drawing area, as defined in GRAspect.* </param>
	/// <param name="draw_limits"> the drawing limits of the drawing area </param>
	/// <param name="units"> the units of the drawing area </param>
	/// <param name="flag"> passed to setDrawingLimits() </param>
	/// <param name="data_limits"> the drawing area data limits </param>
	private void initialize(GRDevice dev, string name, int aspect, GRLimits draw_limits, int units, int flag, GRLimits data_limits)
	{
		string routine = "GRDrawingArea.initialize";
		int dl = 10;

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Initializing");
		}

		if (dev == null)
		{
			Message.printWarning(2, routine, "Null device.");
		}

		// Initialize the basic data members...

		initializeCommon();

		_plotx1 = 0.0;
		_plotx2 = 0.0;
		_ploty1 = 0.0;
		_ploty2 = 0.0;

		if (!string.ReferenceEquals(name, null))
		{
			_name = name;
		}

		// Set the device and let the device know that the drawing area is associated with the device...
		_dev = dev;
		_dev.addDrawingArea(this);
		_aspect = aspect;
		setDrawingLimits(draw_limits, units, flag);
		setDataLimits(data_limits);
		// Save local copies of data...
		_devyshift = _dev.getLimits().getTopY();
		_reverse_y = _dev.getReverseY();
		_graphics = _dev.getPaintGraphics();
		if (Message.isDebugOn)
		{
			Message.printDebug(10, routine, "Device height is " + _devyshift);
		}

		// Initialize the global arrays used for plotting...

		if (_global_xs == null)
		{
			_global_xs = new double[10];
			_global_ys = new double[10];
			_global_len_s = 10;
			_global_ixs = new int[10];
			_global_iys = new int[10];
			_global_len_is = 10;
		}
	}

	/// <summary>
	/// Initialize the drawing area. </summary>
	/// <param name="dev"> the GRDevice this drawing area is associated with </param>
	/// <param name="props"> PropList with drawing area settings. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void initialize(GRDevice dev, RTi.Util.IO.PropList props) throws GRException
	private void initialize(GRDevice dev, PropList props)
	{
		string message, routine = "GRDrawingArea.initialize";
		int dl = 10;

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Initializing");
		}

		if (dev == null)
		{
			message = "Null GRDevice.  Cannot create drawing area.";
			Message.printWarning(2, routine, message);
			throw new GRException(message);
		}

		// Initialize the basic data members which are not configurable at creation...

		initializeCommon();

		// Set passed-in values...

		_dev = dev;

		string prop_value;
		prop_value = props.getValue("Name");
		if (!string.ReferenceEquals(prop_value, null))
		{
			_name = prop_value;
		}
		prop_value = props.getValue("Aspect");
		if (!string.ReferenceEquals(prop_value, null))
		{
			if (prop_value.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				_aspect = GRAspect.TRUE;
			}
			if (prop_value.Equals("fill", StringComparison.OrdinalIgnoreCase))
			{
				_aspect = GRAspect.FILL;
			}
			if (prop_value.Equals("FIllX", StringComparison.OrdinalIgnoreCase))
			{
				_aspect = GRAspect.FILLX;
			}
			if (prop_value.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				_aspect = GRAspect.TRUE;
			}
		}

		// Initialize the global arrays used for plotting...

		if (_global_xs == null)
		{
			_global_xs = new double[10];
			_global_ys = new double[10];
			_global_len_s = 10;
			_global_ixs = new int[10];
			_global_iys = new int[10];
			_global_len_is = 10;
		}
	}

	/// <summary>
	/// Common initialization to both initialize methods above
	/// </summary>
	private void initializeCommon()
	{
		_status = GRUtil.STAT_OPEN;
		_axisx = GRAxis.LINEAR;
		_axisy = GRAxis.LINEAR;
		_color = GRColor.white;
		_dataset = false;
		// Initialize values in case there are problems setting from limits below...
		_datax1 = 0.0;
		_datax2 = 0.0;
		_datay1 = 0.0;
		_datay2 = 0.0;
		_drawset = false;
		_drawx1 = 0.0;
		_drawx2 = 0.0;
		_drawy1 = 0.0;
		_drawy2 = 0.0;
		_font = "Helvetica";
		_fontht = 0.0;
		_lastx = 0.0;
		_lasty = 0.0;
		_linearx1 = 0.0;
		_linearx2 = 0.0;
		_lineary1 = 0.0;
		_lineary2 = 0.0;
		_name = "";
	}

	/// <summary>
	/// Do a linear, logarithmic, or probability interpolation for a drawing area.
	/// This method determines what type of axes a drawing area has.  It then performs
	/// the proper interpolation necessary to map the data value to the proper 
	/// coordinate in the output. This method can also be used independently of a
	/// drawing area by specifying a value of zero for 'axis'. </summary>
	/// <param name="x"> the point to interpolate. </param>
	/// <param name="xmin"> the known minimum of the x data. </param>
	/// <param name="xmax"> the known maximum of the x data. </param>
	/// <param name="ymin"> the known minimum of the y data. </param>
	/// <param name="ymax"> the known maximum of the y data. </param>
	/// <param name="axis"> the axis to interpolate for. </param>
	/// <returns> the interpolated value. </returns>
	/*
	public double interp ( double x, double xmin, double xmax, double ymin, double ymax, int axis )
	{
		double	y = x, z, zmax, zmin;
		int	flag;
	
		if ( axis == 0 ) {
			flag = GRAxis.LINEAR;
		}
		if ( ((_axisx == GRAxis.LOG) && (axis == GRAxis.X)) || ((_axisy == GRAxis.LOG) && (axis == GRAxis.Y)) ) {
			flag = GRAxis.LOG;
		}
		else if(((_axisx == GRAxis.STANDARD_NORMAL_PROBABILITY) && (axis == GRAxis.X)) ||
			((_axisy == GRAxis.STANDARD_NORMAL_PROBABILITY) && (axis == GRAxis.Y)) ) {
			flag = GRAxis.STANDARD_NORMAL_PROBABILITY;
		}
		else {
			flag = GRAxis.LINEAR;
		}
	
		if ( (xmax - xmin) == 0.0 ) {
			y = ymin;
		}
		else if ( flag == GRAxis.LINEAR ) {
			y = ymin + (ymax - ymin)*(x - xmin)/(xmax - xmin);
		}
		else if ( flag == GRAxis.LOG ) {
			/*
			** Axes look like:
			**
			**		|
			**		|
			** y=linear	|
			**   scale	|
			**		------------------
			**		 x=log scale
			*/
			/*
			if ( (x <= 0.0) || (xmin <= 0.0) || (xmax <= 0.0) ) {
				y = ymin;
			}
			else {
			    y = ymin + (ymax - ymin)*(MathUtil.log10(x/xmin))/(Math.log10(xmax/xmin));
			}
		}
		else if ( flag == GRAxis.STANDARD_NORMAL_PROBABILITY ) {
			/*
			** Probability scale...
			**
			** We are given a probability in the range 0.0 to 1.0.  To
			** calculate the plotting point, we need to back-calculate the
			** linearized data value ("z") and map this onto the axis.  By
			** doing so for the tenth probability values, we will end up
			** with a scale that has more "tic-marks" in the center of the
			** page, with wider spacing farther from the center.
			**
			** If the "left" probability limit for the axis is less than the
			** "right" limit, the value calculated is the probability of the
			** value being less than the given value.  If the "left"
			** probability limit is greater than the "right" value, then the
			** value calculated is the probability of the value being exceeded.
			*/
			/*
			NOT SUPPORTED AT THIS TIME
			if ( x == xmin ) {
				y = ymin;
			}
			else if ( x == xmax ) {
				y = ymax;
			}
			else {
			    if ( axis == GRAxis.X ) {
					zmin = _linearx1;
					zmax = _linearx2;
				}
				else {
				    zmin = _lineary1;
					zmax = _lineary2;
				}
				/*
				z = GRFuncZStdNormCum ( x );
				y = GRInterp ( z, zmin, zmax, ymin, ymax );
				*/
			/*
			}
			*/
			/*
		}
		return y;
	}
	*/

	/// <summary>
	/// Checks to see if this drawing area's device is drawing anti-aliased. </summary>
	/// <returns> true if the device is drawing anti aliased, false if not. </returns>
	public virtual bool isDeviceAntiAliased()
	{
		return _dev.isAntiAliased();
	}

	/// <summary>
	/// Draw a line from the current pen position to a point. </summary>
	/// <param name="x"> the x coordinate to draw to </param>
	/// <param name="y"> the y coordinate to draw to </param>
	public abstract void lineTo(double x, double y);

	/// <summary>
	/// Draw a line from the current pen position to a point. </summary>
	/// <param name="point"> the GRPoint to draw to </param>
	public abstract void lineTo(GRPoint point);

	/// <summary>
	/// Move the pen to a point. </summary>
	/// <param name="x"> x coordinate to move to </param>
	/// <param name="y"> y coordinate to move to </param>
	public abstract void moveTo(double x, double y);

	/// <summary>
	/// Move the pen position to a point </summary>
	/// <param name="point"> point to move to </param>
	public abstract void moveTo(GRPoint point);

	/// <summary>
	/// Scale x data value to device plotting coordinates. </summary>
	/// <param name="xdata"> value to scale in data coordinates. </param>
	public virtual double scaleXData(double xdata)
	{
		double xdev;

		if (_axisx == GRAxis.LOG)
		{
			if ((_datax2 - _datax1) == 0.0)
			{
				xdev = _plotx1;
			}
			else
			{
				/*
				** Axes look like:
				**
				**		|
				**		|
				** y=linear	|
				**   scale	|
				**		------------------
				**		 x=log scale
				*/	
				if ((xdata <= 0.0) || (_datax1 <= 0.0) || (_datax2 <= 0.0))
				{
					xdev = _plotx1;
				}
				else
				{
					xdev = _plotx1 + (_plotx2 - _plotx1) * (Math.Log10(xdata / _datax1)) / (Math.Log10(_datax2 / _datax1));
				}
			}
		}
		else if (_axisx == GRAxis.STANDARD_NORMAL_PROBABILITY)
		{
			/*
			** Probability scale...
			**
			** We are given a probability in the range 0.0 to 1.0.  To
			** calculate the plotting point, we need to back-calculate the
			** linearized data value ("z") and map this onto the axis.  By
			** doing so for the tenth probability values, we will end up
			** with a scale that has more "tic-marks" in the center of the
			** page, with wider spacing farther from the center.
			**
			** If the "left" probability limit for the axis is less than the
			** "right" limit, the value calculated is the probability of the
			** value being less than the given value.  If the "left"
			** probability limit is greater than the "right" value, then the
			** value calculated is the probability of the value being exceeded.
			*/
			double xtemp = -1;
			if (xdata == _datax1)
			{
				xdev = _plotx1;
			}
			else if (xdata == _datax2)
			{
				xdev = _plotx2;
			}
			else
			{
				double dataMin = 0;
				double dataMax = 0;
				dataMin = _linearx1;
				dataMax = _linearx2;

				try
				{
					xtemp = MathUtil.interpolate(xdata, 0, 1, -5, 5);
					xtemp = StandardNormal.cumulativeStandardNormal(xtemp);
					xdev = MathUtil.interpolate(xtemp, dataMin, dataMax, _plotx1, _plotx2);
				}
				catch (Exception e)
				{
					string routine = "GRDrawingArea.scaleXData";
					Message.printWarning(2, routine, "Error calculating standard normal value.");
					Message.printWarning(2, routine, e);
					xdev = _plotx1;
				}
			}
		}
		else
		{
			xdev = MathUtil.interpolate(xdata, _datax1, _datax2, _plotx1, _plotx2);
		}

		if (Message.isDebugOn)
		{
			string routine = "GRDrawingArea.scaleXData";
			Message.printDebug(100, routine, "Scaled X data " + xdata + " to dev " + xdev);
		}
		return xdev;
	}

	/// <summary>
	/// Scale y data value to device plotting coordinate. </summary>
	/// <param name="ydata"> value to scale in data coordinates. </param>
	public virtual double scaleYData(double ydata)
	{
		double ydev;

		if (_axisy == GRAxis.LOG)
		{
			if ((_datay2 - _datay1) == 0.0)
			{
				ydev = _ploty1;
			}
			else
			{
				/*
				** Axes look like:
				**
				**		|
				**		|
				** y=linear	|
				**   scale	|
				**		------------------
				**		 x=log scale
				*/	
				if ((ydata <= 0.0) || (_datay1 <= 0.0) || (_datay2 <= 0.0))
				{
					ydev = _ploty1;
				}
				else
				{
					ydev = _ploty1 + (_ploty2 - _ploty1) * (Math.Log10(ydata / _datay1)) / (Math.Log10(_datay2 / _datay1));
				}
			}
		}
		else if (_axisx == GRAxis.STANDARD_NORMAL_PROBABILITY)
		{
			/*
			** Probability scale...
			**
			** We are given a probability in the range 0.0 to 1.0.  To
			** calculate the plotting point, we need to back-calculate the
			** linearized data value ("z") and map this onto the axis.  By
			** doing so for the tenth probability values, we will end up
			** with a scale that has more "tic-marks" in the center of the
			** page, with wider spacing farther from the center.
			**
			** If the "left" probability limit for the axis is less than the
			** "right" limit, the value calculated is the probability of the
			** value being less than the given value.  If the "left"
			** probability limit is greater than the "right" value, then the
			** value calculated is the probability of the value being exceeded.
			*/
			if (ydata == _datay1)
			{
				ydev = _ploty1;
			}
			else if (ydata == _datay2)
			{
				ydev = _ploty2;
			}
			else
			{
				double dataMin = 0;
				double dataMax = 0;
				dataMin = _lineary1;
				dataMax = _lineary2;

				try
				{
					double ytemp = StandardNormal.cumulativeStandardNormal(ydata);
					ydev = MathUtil.interpolate(ytemp, dataMin, dataMax, _ploty1, _ploty2);
				}
				catch (Exception e)
				{
					string routine = "GRDrawingArea.scaleYData";
					Message.printWarning(2, routine, "Error calculating standard normal value.");
					Message.printWarning(2, routine, e);
					ydev = _ploty1;
				}
			}
		}
		else
		{
			ydev = MathUtil.interpolate(ydata, _datay1, _datay2, _ploty1, _ploty2);
		}

		if (Message.isDebugOn)
		{
			string routine = "GRDrawingArea.scaleYData";
			Message.printDebug(100, routine, "Scaled Y data " + ydata + " to dev " + ydev);
		}
		return ydev;
	}

	/// <summary>
	/// Sets the axes </summary>
	/// <param name="axisx"> the x axis to set (see GRAxis.LINEAR, etc) </param>
	/// <param name="axisy"> the y axis to set (see GRAxis.LINEAR, etc) </param>
	public virtual void setAxes(int axisx, int axisy)
	{
		_axisx = axisx;
		_axisy = axisy;

		if (_drawset)
		{
			setPlotLimits();
		}
	}

	/// <summary>
	/// Sets the clipping area, in data limits.  If null, the clip is removed. </summary>
	/// <param name="dataLimits"> the limits of the data area that will be clipped. </param>
	public abstract void setClip(GRLimits dataLimits);

	/// <summary>
	/// Sets the clipping area, in data limits.  If null, the clip is removed. </summary>
	/// <param name="clip"> the shape to clip to </param>
	public abstract void setClip(Shape clip);

	/// <summary>
	/// Set the current color. </summary>
	/// <param name="color"> GRColor to use. </param>
	/// <seealso cref= GRColor </seealso>
	public abstract void setColor(GRColor color);

	/// <summary>
	/// Set the current color. </summary>
	/// <param name="r"> Red component in range 0.0 to 1.0. </param>
	/// <param name="g"> Green component in range 0.0 to 1.0. </param>
	/// <param name="b"> Blue component in range 0.0 to 1.0. </param>
	public abstract void setColor(float r, float g, float b);

	/// <summary>
	/// Set the current color. </summary>
	/// <param name="r"> Red component in range 0.0 to 1.0. </param>
	/// <param name="g"> Green component in range 0.0 to 1.0. </param>
	/// <param name="b"> Blue component in range 0.0 to 1.0. </param>
	public abstract void setColor(double r, double g, double b);

	/// <summary>
	/// Set the data limits for the drawing area. </summary>
	/// <param name="xleft"> the leftmost x coordinate </param>
	/// <param name="ybottom"> the bottom y coordinate </param>
	/// <param name="xright"> the rightmost x coordinate </param>
	/// <param name="ytop"> the top y coordinate </param>
	public virtual void setDataLimits(double xleft, double ybottom, double xright, double ytop)
	{
		int dl = 10;

		_datax1 = xleft;
		_datay1 = ybottom;
		_datax2 = xright;
		_datay2 = ytop;
		_dataset = true;
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, "GRDrawingArea.setDataLimits(x1,y1,x2,y2)", "Set data limits to (" + _datax1 + "," + _datay1 + ") (" + _datax2 + "," + _datay2 + ")");
		}
		if (_drawset)
		{
			setPlotLimits();
		}
	}

	/// <summary>
	/// Set the data limits for the drawing area. </summary>
	/// <param name="limits"> GRLimits containing data limits. </param>
	public virtual void setDataLimits(GRLimits limits)
	{
		if (limits == null)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(10, "GRDrawingArea.setDataLimits", "Null GRLimits");
			}
			return;
		}
		setDataLimits(limits.getLeftX(), limits.getBottomY(), limits.getRightX(), limits.getTopY());
	}

	/// <summary>
	/// Sets whether this drawing area's device should begin drawing anti-aliased.
	/// Currently, on GRJComponentDevice Objects support this. </summary>
	/// <param name="antiAlias"> if true, the device will be told to begin drawing anti-aliased. </param>
	public virtual void setDeviceAntiAlias(bool antiAlias)
	{
		_dev.setAntiAlias(antiAlias);
	}

	/// <summary>
	/// Set the drawing limits (device limits) for the drawing area. </summary>
	/// <param name="limits"> Drawing limits (device coordinates to attach the lower-left
	/// and upper-right corner of the drawing area). </param>
	/// <param name="units"> Units of the limits (will be converted to device units). </param>
	/// <param name="flag"> Modifier for drawing limits.  If GRLimits.UNIT, then the limits are
	/// assumed to be percentages of the device (0.0 to 1.0) and the units are not used. </param>
	/// <seealso cref= GRUnits </seealso>
	public virtual void setDrawingLimits(GRLimits limits, int units, int flag)
	{
		if (limits == null)
		{
			return;
		}
		if (Message.isDebugOn)
		{
			Message.printDebug(10, "GRDrawingArea.setDrawingLimits(GRLimits)", "Limits values are: " + limits);
		}
		setDrawingLimits(limits.getLeftX(), limits.getBottomY(), limits.getRightX(), limits.getTopY(), units, flag);
	}

	/// <summary>
	/// Sets the drawing limits (device limits) for the drawing area. </summary>
	/// <param name="xmin"> the lowest x value </param>
	/// <param name="ymin"> the lowest y value </param>
	/// <param name="xmax"> the highest x value </param>
	/// <param name="ymax"> the highest y value </param>
	/// <param name="units"> the units for the drawing area </param>
	/// <param name="flag"> kind of limits (either GRLimits.DEVICE or GRLimits.UNIT) </param>
	public virtual void setDrawingLimits(double xmin, double ymin, double xmax, double ymax, int units, int flag)
	{
		string routine = "GRDrawingArea.setDrawingLimits(x1,y1,x2,y2)";
		int dl = 10;
		if (flag == GRLimits.DEVICE)
		{
			int dev_units = _dev.getUnits();
			if (units == GRUnits.DEVICE)
			{
				// Just in case developer passes in generic device units instead of actual units...
				units = dev_units;
			}
			_drawx1 = GRUnits.convert(xmin, units, dev_units);
			_drawy1 = GRUnits.convert(ymin, units, dev_units);
			_drawx2 = GRUnits.convert(xmax, units, dev_units);
			_drawy2 = GRUnits.convert(ymax, units, dev_units);
		}
		else if (flag == GRLimits.UNIT)
		{
			GRLimits devlimits = _dev.getLimits();
			_drawx1 = MathUtil.interpolate(xmin, 0.0, 1.0, devlimits.getLeftX(), devlimits.getRightX());
			_drawy1 = MathUtil.interpolate(ymin, 0.0, 1.0, devlimits.getBottomY(), devlimits.getTopY());
			_drawx2 = MathUtil.interpolate(xmax, 0.0, 1.0, devlimits.getLeftX(), devlimits.getRightX());
			_drawy2 = MathUtil.interpolate(ymax, 0.0, 1.0, devlimits.getBottomY(), devlimits.getTopY());
		}
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Set drawing limits to (" + _drawx1 + "," + _drawy1 + ") (" + _drawx2 + "," + _drawy2 + ")");
		}
		_drawset = true;
		if (_dataset)
		{
			setPlotLimits();
		}
	}

	//	int setDrawMask ( int drawmask );

	/// <summary>
	/// Set the font for the drawing area. </summary>
	/// <param name="name"> Font name (e.g., "Helvetica"). </param>
	/// <param name="style"> Font style ("Plain", "Bold", or "Italic"). </param>
	/// <param name="size"> Font height in points. </param>
	public abstract void setFont(string name, string style, double size);

	/// <summary>
	/// Set the last X data value drawn. </summary>
	/// <param name="lastx"> the lastx data value drawn </param>
	public virtual void setLastX(double lastx)
	{
		_lastx = lastx;
	}

	/// <summary>
	/// Set the last X, Y data value drawn. </summary>
	/// <param name="lastx"> the last x data value drawn </param>
	/// <param name="lasty"> the last y data value drawn </param>
	public virtual void setLastXY(double lastx, double lasty)
	{
		setLastX(lastx);
		setLastY(lasty);
	}

	/// <summary>
	/// Set the last Y data value drawn. </summary>
	/// <param name="lasty"> the last y data value drawn </param>
	public virtual void setLastY(double lasty)
	{
		_lasty = lasty;
	}

	/// <summary>
	/// Set the line pattern </summary>
	/// <param name="dash"> an array defining the dash pattern </param>
	/// <param name="offset"> the line offset </param>
	public abstract void setLineDash(double[] dash, double offset);
	/*
	{	String routine = "GRDrawingArea.setLineDash";
		Message.printWarning ( 2, routine, "Define in derived class" );
	}
	*/

	/// <summary>
	/// Set the line join style. </summary>
	/// <param name="join"> the line join style, one of GRDrawingAreaUtil.JOIN*
	/// public abstract void setLineJoin ( int join );
	/// /*
	/// {	String routine = "GRDrawingArea.setLineJoin";
	/// Message.printWarning ( 2, routine, "Define in derived class" );
	/// } </param>

	/// <summary>
	/// Set the line width.
	/// TODO (SAM - 2003-05-07) Need to standardize on points, pixels, etc.  Both, depending on output? </summary>
	/// <param name="linewidth"> the width of the line </param>
	public abstract void setLineWidth(double linewidth);

	/// <summary>
	/// Set the plot limits knowing that the data and device limits are set.
	/// </summary>
	public virtual void setPlotLimits()
	{
		string routine = "GRDrawingArea.setPlotLimits";
		double height, width, xpercent, xrange, ypercent, yrange;
		int dl = 10;

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Setting the plot limits._axisx=" + _axisx + " _axisy=" + _axisy);
		}

		if (!_dataset)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "Data limits are not set.  Not setting plot limits.");
			}
			return;
		}
		if (!_drawset)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "Drawing limits are not set.  Not setting plot limits.");
			}
			return;
		}
		if (_aspect == GRAspect.TRUE)
		{
			// Plot is to maintain true scale aspect of data in horizontal and vertical directions...
			width = _drawx2 - _drawx1;
			height = _drawy2 - _drawy1;
			xrange = _datax2 - _datax1;
			if (xrange < 0.0)
			{
				xrange *= -1.0;
			}
			yrange = _datay2 - _datay1;
			if (yrange < 0.0)
			{
				yrange *= -1.0;
			}
			if ((xrange / yrange) > (width / height))
			{
				xpercent = 100.0;
				ypercent = 100.0 * yrange * width / (xrange * height);
			}
			else
			{
				ypercent = 100.0;
				xpercent = 100.0 * xrange * height / (yrange * width);
			}
			_ploty1 = height / 2.0 - height * ypercent / 200.0 + _drawy1;
			_ploty2 = height / 2.0 + height * ypercent / 200.0 + _drawy1;
			_plotx1 = width / 2.0 - width * xpercent / 200.0 + _drawx1;
			_plotx2 = width / 2.0 + width * xpercent / 200.0 + _drawx1;
		}
		else if (_aspect == GRAspect.FILL)
		{
			// Fill the drawing area in both directions...
			_plotx1 = _drawx1;
			_ploty1 = _drawy1;
			_plotx2 = _drawx2;
			_ploty2 = _drawy2;
		}
		else if (_aspect == GRAspect.FILLX)
		{
			// Fill the drawing area in the X-direction...
			_plotx1 = _drawx1;
			_plotx2 = _drawx2;
		}
		else if (_aspect == GRAspect.FILLY)
		{
			// Only fill the drawing area in the Y-direction...
			_ploty1 = _drawy1;
			_ploty2 = _drawy2;
		}
		// Save the linearized data limits used for interpolating plotting
		// positions.  For a linear scale, these are just the normal limits.
		// For a log scale, these are the log10 values.  If we are using a
		// probability axis, need to calculate linearized Z data values
		// corresponding to probabilities (to be used in interpolating plotting coordinates)...
		if (_axisx == GRAxis.LINEAR)
		{
			_linearx1 = _datax1;
			_linearx2 = _datax2;
		}
		else if (_axisx == GRAxis.LOG)
		{
			_linearx1 = Math.Log10(_datax1);
			_linearx2 = Math.Log10(_datax2);
		}
		else if (_axisx == GRAxis.STANDARD_NORMAL_PROBABILITY)
		{
			try
			{
				_linearx1 = StandardNormal.cumulativeStandardNormal(-5);
				_linearx2 = StandardNormal.cumulativeStandardNormal(5);
			}
			catch (Exception e)
			{
				Message.printWarning(2, routine, "Error calculating data limits for standard normal X.  Drawing will not work properly --" + " find and fix the error!");
				Message.printWarning(2, routine, e);
			}
			if (Message.isDebugOn)
			{
				Message.printDebug(10, routine, "Set linearized X data limits to " + _linearx1 + " to " + _linearx2);
			}
		}
		else
		{
			Message.printWarning(2, routine, "X axis type " + _axisx + " is not recognized.  Big problem!");
		}
		if (_axisy == GRAxis.LINEAR)
		{
			_lineary1 = _datay1;
			_lineary2 = _datay2;
		}
		else if (_axisy == GRAxis.LOG)
		{
			_lineary1 = Math.Log10(_datay1);
			_lineary2 = Math.Log10(_datay2);
		}
		else if (_axisy == GRAxis.STANDARD_NORMAL_PROBABILITY)
		{
			try
			{
				_lineary1 = StandardNormal.cumulativeStandardNormal(-5);
				_lineary2 = StandardNormal.cumulativeStandardNormal(5);
			}
			catch (Exception e)
			{
				Message.printWarning(2, routine, "Error calculating data limits for standard " + "normal X.  Drawing will not work properly -- find and fix the error!");
				Message.printWarning(2, routine, e);
			}
			if (Message.isDebugOn)
			{
				Message.printDebug(10, routine, "Set linearized Y data limits to " + _lineary1 + " to " + _lineary2);
			}
		}
		else
		{
			Message.printWarning(2, routine, "Y axis type " + _axisy + " is not recognized.  Big problem!");
		}

		if (Message.isDebugOn)
		{
			Message.printDebug(10, routine, "Plot limits are: " + _plotx1 + "," + _ploty1 + " " + _plotx2 + "," + _ploty2);
		}
	}

	public abstract void pageEnd();
	public abstract void pageStart();

	/// <summary>
	/// Return string representation of drawing area as property=value list,
	/// delimited by newlines. </summary>
	/// <returns> list of properties </returns>
	public override string ToString()
	{
		StringBuilder s = new StringBuilder();
		string nl = "\n";
		s.Append("dataset = " + _dataset + nl);
		s.Append("datax1 = " + _datax1 + nl);
		s.Append("datax2 = " + _datax2 + nl);
		s.Append("datay1 = " + _datay1 + nl);
		s.Append("datay2 = " + _datay2 + nl);
		if (_dev == null)
		{
			s.Append("dev = null");
		}
		else if (_dev.getLimits() != null)
		{
			s.Append("dev.height = " + _dev.getLimits().getHeight() + nl);
		}
		s.Append("devyshift = " + _devyshift + nl);
		s.Append("drawset = " + _drawset + nl);
		s.Append("drawx1 = " + _drawx1 + nl);
		s.Append("drawx2 = " + _drawx2 + nl);
		s.Append("drawy1 = " + _drawy1 + nl);
		s.Append("drawy2 = " + _drawy2 + nl);
		s.Append("name = \"" + _name + "\"" + nl);
		s.Append("plotx1 = " + _plotx1 + nl);
		s.Append("plotx2 = " + _plotx2 + nl);
		s.Append("ploty1 = " + _ploty1 + nl);
		s.Append("ploty2 = " + _ploty2 + nl);
		s.Append("reverse_y = " + _reverse_y);
		return s.ToString();
	}

	}

}