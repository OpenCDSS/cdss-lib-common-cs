using System;
using System.Collections.Generic;
using System.Text;

// GRPSDrawingArea - GR PostScript drawing area class

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
// GRPSDrawingArea - GR PostScript drawing area class
// ----------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
// ----------------------------------------------------------------------------
// Notes:	(1)	This class is the driver for drawing to PostScript
//			files.  These drawing capabilities are independent of
//			the Java API and can be used to create true PostScript
//			files (desirable for high resolution output).
//		(2)	All commands should assume that they can print with
//			no leading space and end with a newline.
// ----------------------------------------------------------------------------
// History:
//
// 10 Aug 1997	Steven A. Malers, RTi	Initial Java version as port of C++/C
//					code.
// 28 Mar 1998	SAM, RTi		Revisit the code and implement a
//					a totally new design that is 100%
//					object oriented.
// 29 Aug 1998	SAM, RTi		Copy the GRDrawingArea class and fill
//					in code from the C/C++ code.
// 08 Nov 1999	SAM, RTi		Enable line dash code.
// 2002-01-18	SAM, RTi		Change setFont() to take a style.
// 2003-05-01	J. Thomas Sapienza, RTi	Made changes to accomodate the massive
//					restructuring of GR.java.
// 2003-05-02	JTS, RTi		Added some methods because GRDrawingArea
//					was made abstract.
// 2005-04-20	JTS, RTi		* Added setClip().
//					* Added getClip().
// 2005-04-26	JTS, RTi		Added finalize().
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.GR
{


	using PropList = RTi.Util.IO.PropList;

	using MathUtil = RTi.Util.Math.MathUtil;

	using Message = RTi.Util.Message.Message;

	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This class implements a driver to PostScript files.
	/// TODO (SAM - 2003-05-08) Revisit code later -- it may have limited use.
	/// </summary>
	public class GRPSDrawingArea : GRDrawingArea
	{

	internal class GRPSFontData
	{
			private readonly GRPSDrawingArea outerInstance;

		internal string grname;
		internal string psname;
		internal GRPSFontData(GRPSDrawingArea outerInstance, string g, string p)
		{
			this.outerInstance = outerInstance;
			grname = g;
			psname = p;
		}
	}


	/// <summary>
	/// Set to the GRDevice file.
	/// </summary>
	private PrintWriter _fp = null;

	/// <summary>
	/// Line separator.
	/// </summary>
	private static string _nl = null;

	/// <summary>
	/// Postscript fonts.
	/// </summary>
	private static GRPSFontData[] _psfonts = null;

	/// <summary>
	/// GRPSDevice associated with this GRPSDrawingArea.  This is a cast of the 
	/// device stored in GRDevice.
	/// </summary>
	private GRPSDevice _psdev = null;

	/// <summary>
	/// General constructor. </summary>
	/// <param name="dev"> GRDevice associated with the drawing area. </param>
	/// <param name="name"> A name for the drawing area. </param>
	/// <param name="aspect"> Aspect for the axes of the drawing area. </param>
	/// <param name="draw_limits"> Drawing limits (device coordinates to attach the lower-left
	/// and upper-right corner of the drawing area). </param>
	/// <param name="units"> Units of the limits (will be converted to device units). </param>
	/// <param name="flag"> Modifier for drawing limits.  If GRLimits.UNIT, then the limits are
	/// assumed to be percentages of the device (0.0 to 1.0) and the units are not
	/// used. </param>
	/// <param name="data_limits"> Data limits associated with the lower-left and upper-right
	/// corners of the drawing area. </param>
	/// <seealso cref= GRAspect </seealso>
	public GRPSDrawingArea(GRPSDevice dev, string name, int aspect, GRLimits draw_limits, int units, int flag, GRLimits data_limits)
	{
		string routine = "GRPSDrawingArea(...)";

		if (Message.isDebugOn)
		{
			Message.printDebug(10, routine, "Constructing using all arguments, name=\"" + name + "\"");
		}
		initialize(dev, name, aspect, draw_limits, units, flag, data_limits);
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dev"> the GRDevice associated with the drawing area. </param>
	/// <param name="props"> PropList with drawing area settings. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public GRPSDrawingArea(GRPSDevice dev, RTi.Util.IO.PropList props) throws GRException
	public GRPSDrawingArea(GRPSDevice dev, PropList props) : base(dev, props)
	{
		// Call parent...

		string routine = "GRPSDrawingArea(GRPSDevice,PropList)";
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
	/// This has no effect in PostScript.
	/// </summary>
	public override void clear()
	{
	}

	/// <summary>
	/// Set the clip on/off for the drawing area.  The full drawing area is used for
	/// clipping (ignoring aspect). </summary>
	/// <param name="flag"> Indicates whether clipping should be on or off. </param>
	public override void clip(bool flag)
	{
		if (flag)
		{
			// Set clip path to drawing area limits...
			_fp.print("gsave" + " " + _drawx1 + " " + _drawy1 + " moveto " + _drawx2 + " " + _drawy1 + " lineto " + _drawx2 + " " + _drawy2 + " lineto " + _drawx1 + " " + _drawy2 + " lineto closepath clip newpath " + _nl);
		}
		else
		{ // Set the clip path to the previous limits (assume to be full
			// page)...
			_fp.print("grestore" + _nl);
		}
	}

	/// <summary>
	/// Add a comment to the output file. </summary>
	/// <param name="comment"> Comment to add to file. </param>
	public override void comment(string comment)
	{
		_fp.print("%% " + comment + _nl);
	}

	/// <summary>
	/// Draw an arc using the current color, line, etc. </summary>
	/// <param name="a1"> Initial angle to start drawing (0 is at 3 o'clock, then
	/// counterclockwise). </param>
	/// <param name="a2"> Ending angle. </param>
	/// <param name="x"> X-coordinate of center. </param>
	/// <param name="xr"> X-radius. </param>
	/// <param name="y"> Y-coordinate of center. </param>
	/// <param name="yr"> Y-radius. </param>
	public override void drawArc(double x, double y, double xr, double yr, double a1, double a2)
	{
		// For the time being, assume we are dealing with circles...

		_fp.print("newpath" + _nl + x + " " + y + " " + xr + " " + a1 + " " + a2 + " arc" + _nl);
		stroke();
	}

	/// <summary>
	/// Not implemented
	/// </summary>
	public override void drawCompoundText(IList<string> text, GRColor color, double x, double y, double angle, int flag)
	{
		string routine = "GRPSDrawingArea.drawCompoundText";
		Message.printWarning(1, routine, "GRPSDrawingArea.drawCompoundText not implemnted");
	}

	/// <summary>
	/// Draw a line. </summary>
	/// <param name="x"> array of x points. </param>
	/// <param name="y"> array of y points. </param>
	public override void drawLine(double[] x, double[] y)
	{
		moveTo(x[0], y[0]);
		lineTo(x[1], y[1]);
	}

	/// <summary>
	/// Draw a line </summary>
	/// <param name="x0"> the first x coordinate </param>
	/// <param name="y0"> the first y coordinate </param>
	/// <param name="x1"> the end x coordinate </param>
	/// <param name="y1"> the end y coordinate </param>
	public virtual void drawLine(double x0, double y0, double x1, double y1)
	{
		moveTo(x0, y0);
		lineTo(x1, y1);
	}

	/// <summary>
	/// Draws a polygon. </summary>
	/// <param name="x"> an array of x coordinates </param>
	/// <param name="y"> an array of y coordinates. </param>
	public virtual void drawPolygon(double[] x, double[] y)
	{
		int npts = x.Length;
		drawPolygon(npts, x, y);
	}

	/// <summary>
	/// Draws a polygon. </summary>
	/// <param name="npts"> the number of points in the polygon. </param>
	/// <param name="x"> an array of x coordinates </param>
	/// <param name="y"> an array of y coordinates. </param>
	public override void drawPolygon(int npts, double[] x, double[] y)
	{
		int i = 0;
		if (npts > 0)
		{
			moveTo(x[0], y[0]);
		}
		for (i = 1; i < npts; i++)
		{
			lineTo(x[i], y[i]);
		}
		lineTo(x[0], y[0]);
		if (i > 2)
		{
			lineTo(x[1], y[1]);
		}
		stroke();
	}

	/// <summary>
	/// Draws a polyline </summary>
	/// <param name="x"> an array of x coordinates </param>
	/// <param name="y"> an array of y coordinates. </param>
	public virtual void drawPolyline(double[] x, double[] y)
	{
		drawPolyline(x.Length, x, y);
	}

	/// <summary>
	/// Draws a polyline </summary>
	/// <param name="npts"> the number of points in the polyline </param>
	/// <param name="x"> an array of x coordinates </param>
	/// <param name="y"> an array of y coordinates. </param>
	public override void drawPolyline(int npts, double[] x, double[] y)
	{
		if (npts > 0)
		{
			moveTo(x[0], y[0]);
		}
		for (int i = 1; i < npts; i++)
		{
			lineTo(x[i], y[i]);
		}
		stroke();
	}

	/// <summary>
	/// Draws a rectangle in the current color. </summary>
	/// <param name="xll"> the lower-left x coordinate </param>
	/// <param name="yll"> the lower-left y coordinate </param>
	/// <param name="width"> the width of the rectangle </param>
	/// <param name="height"> the height of the rectangle </param>
	public override void drawRectangle(double xll, double yll, double width, double height)
	{
		moveTo(xll, yll);
		lineTo(xll + width, yll);
		lineTo(xll + width, yll + height);
		lineTo(xll, yll + height);
		lineTo(xll, yll);
		stroke();
	}

	/// <summary>
	/// Draw text. </summary>
	/// <param name="text"> the text string to draw. </param>
	/// <param name="x"> the x location at which to draw the string. </param>
	/// <param name="y"> the y location at which to draw the string. </param>
	/// <param name="a"> the angle of rotation of the text, counter-clockwise </param>
	/// <param name="flag"> one of the GRText.* values specifying how to draw the string. </param>
	public override void drawText(string text, double x, double y, double a, int flag)
	{
		drawText(text, x, y, a, flag, 0);
	}

	/// <summary>
	/// Draw text. </summary>
	/// <param name="text"> the text string to draw. </param>
	/// <param name="x"> the x location at which to draw the string. </param>
	/// <param name="y"> the y location at which to draw the string. </param>
	/// <param name="a"> the angle of rotation of the text, counter-clockwise </param>
	/// <param name="flag"> one of the GRText.* values specifying how to draw the string. </param>
	/// <param name="rotationDegrees"> degrees the text is rotated (UNUSED) </param>
	public override void drawText(string text, double x, double y, double a, int flag, double rotationDegrees)
	{
		int aflag;
		double px, py;

		if ((a < .001) && (a > -.001))
		{
			aflag = 0;
		}
		else
		{
			aflag = 1;
		}
		if (aflag != 0)
		{
			_fp.print("gsave " + x + " " + y + " translate " + a + " rotate ");
			px = 0.0;
			py = 0.0;
		}
		else
		{
			px = x;
			py = y;
		}
		if ((flag & GRText.CENTER_Y) != 0)
		{
			py -= _fontht / 2.0;
		}
		else if ((flag & GRText.TOP) != 0)
		{
			py -= _fontht;
		}
		if ((flag & GRText.CENTER_X) != 0)
		{
			_fp.print("(");
			drawText2(text);
			_fp.print(") " + px + " " + py + " CS" + _nl);
		}
		else if ((flag & GRText.RIGHT) != 0)
		{
			_fp.print("(");
			drawText2(text);
			_fp.print(") " + px + " " + py + " RS" + _nl);
		}
		else
		{
			_fp.print("("); // default
			drawText2(text);
			_fp.print(") " + px + " " + py + " LS" + _nl);
		}
		if (aflag != 0)
		{
			_fp.print(" grestore" + _nl);
		}
		stroke();
	}

	/// <summary>
	/// Draw text to file, replacing ( with \( and ) with \) </summary>
	/// <param name="text"> the text string to draw. </param>
	public virtual void drawText2(string text)
	{
		if (string.ReferenceEquals(text, null))
		{
			return;
		}
		int length = text.Length;
		StringBuilder buffer = new StringBuilder();
		char c;
		for (int i = 0; i < length; i++)
		{
			c = text[i];
			if ((c == ')') || (c == '(') || (c == '\\'))
			{
				buffer.Append('\\');
			}
			buffer.Append(c);
		}
		_fp.print(buffer.ToString());
	}

	/// <summary>
	/// Fill an arc using the current color, line, etc. </summary>
	/// <param name="x"> X-coordinate of center. </param>
	/// <param name="rx"> X-radius. </param>
	/// <param name="y"> Y-coordinate of center. </param>
	/// <param name="ry"> Y-radius. </param>
	/// <param name="a1"> Initial angle to start drawing (0 is at 3 o'clock, then
	/// counterclockwise). </param>
	/// <param name="a2"> Ending angle. </param>
	/// <param name="fillmode"> Fill mode for arc (see GR.FILL_CHORD or GR.FILL_PIE). </param>
	public override void fillArc(double x, double y, double rx, double ry, double a1, double a2, int fillmode)
	{
		// For the time being, assume we are dealing with circles...

		_fp.print("newpath" + _nl + x + " " + y + " " + rx + " " + a1 + " " + a2 + " arc closepath fill" + _nl);
		stroke();
	}

	/// <summary>
	/// FIlls a polygon with the current color. </summary>
	/// <param name="x"> an array of x coordinates </param>
	/// <param name="y"> an array of y coordinates </param>
	public virtual void fillPolygon(double[] x, double[] y)
	{
		int n = x.Length;
		fillPolygon(n, x, y);
	}

	/// <summary>
	/// Fill a polygon with the current color. </summary>
	/// <param name="n"> the number of coordinates </param>
	/// <param name="x"> X-coordinates of points in polygon. </param>
	/// <param name="y"> Y-coordinates of points in polygon. </param>
	public override void fillPolygon(int n, double[] x, double[] y)
	{
		int ny = y.Length;
		if (ny < n)
		{
			n = ny;
		}
		if (n < 2)
		{
			return;
		}
		stroke();
		_fp.print("newpath ");
		moveTo(x[0], y[0]);
		for (int i = 1; i < n; i++)
		{
			lineTo(x[i], y[i]);
		}
		lineTo(x[0], y[0]);
		_fp.print(" closepath fill " + _nl);
		stroke();
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~GRPSDrawingArea()
	{
		_fp = null;
		_psdev = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Flush the drawing area (and device).
	/// </summary>
	public override void flush()
	{
		stroke();
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public virtual GRLimits getBoundingBoxFromFile(string filename)
	{
		string routine = "GRPSDrawingArea.getBoundingBoxFromFile";
		Message.printWarning(1, routine, routine + " not implemented");
		return null;
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public override Shape getClip()
	{
		return null;
	}

	/// <summary>
	/// Get the device extents of data limits.  Simple returns <tt>limits</tt>. </summary>
	/// <param name="limits"> value returned </param>
	/// <param name="flag"> unused </param>
	/// <returns> the limits parameter. </returns>
	public override GRLimits getDataExtents(GRLimits limits, int flag)
	{
		// temporary...
		return limits;
	}

	/// <summary>
	/// Get the data values for device coordinates.  This is typically used when
	/// interpreting a mouse action and therefore for a PostScript file has little use. </summary>
	/// <returns> A GRPoint with the data point. </returns>
	/// <param name="devx"> Device x-coordinate. </param>
	/// <param name="devy"> Device y-coordinate. </param>
	/// <param name="flag"> GR.COORD_DEVICE if the coordinates are originating with the device
	/// (e.g., a mouse) or GR.COORD_PLOT if the coordinates are plotting coordinates
	/// (this flag affects how the y-axis is reversed on some devices). </param>
	public override GRPoint getDataXY(double devx, double devy, int flag)
	{
		double x = MathUtil.interpolate(devx, _plotx1, _plotx2, _datax1, _datax2);
		double y = MathUtil.interpolate(devy, _ploty1, _ploty2, _datay1, _datay2);
		return new GRPoint(x, y);
	}
	/// <summary>
	/// Initialize drawing area settings. </summary>
	/// <param name="dev"> GRDevice associated with the drawing area. </param>
	/// <param name="name"> A name for the drawing area. </param>
	/// <param name="aspect"> Aspect for the axes of the drawing area. </param>
	/// <param name="draw_limits"> Drawing limits (device coordinates to attach the lower-left
	/// and upper-right corner of the drawing area). </param>
	/// <param name="units"> Units of the limits (will be converted to device units). </param>
	/// <param name="flag"> Modifier for drawing limits.  If GRLimits.UNIT, then the limits are
	/// assumed to be percentages of the device (0.0 to 1.0) and the units are not
	/// used. </param>
	/// <param name="data_limits"> Data limits associated with the lower-left and upper-right
	/// corners of the drawing area. </param>
	private void initialize(GRPSDevice dev, string name, int aspect, GRLimits draw_limits, int units, int flag, GRLimits data_limits)
	{
		string routine = "GRDrawingArea.initialize(args)";
		Message.printWarning(1, routine, "Use PropList version");
	}

	/// <summary>
	/// Initialize drawing area settings. </summary>
	/// <param name="dev"> GRDevice associated with the drawing area. </param>
	/// <param name="props"> PropList with drawing area settings. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void initialize(GRPSDevice dev, RTi.Util.IO.PropList props) throws GRException
	private void initialize(GRPSDevice dev, PropList props)
	{
		string routine = "GRDrawingArea.initialize(PropList)";
		int dl = 10;

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Initializing");
		}

		// Initialize the basic data members...

		_status = GRUtil.STAT_OPEN;
		_axisx = GRAxis.LINEAR;
		_axisy = GRAxis.LINEAR;
		_color = GRColor.white;
		_dataset = false;
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
		_fontht = 8.0;
		_lastx = 0.0;
		_lasty = 0.0;
		_linearx1 = 0.0;
		_linearx2 = 0.0;
		_lineary1 = 0.0;
		_lineary2 = 0.0;
		_name = "";

		// Device data that we want reference to here...

		_psdev = (GRPSDevice)_dev;
		_fp = _psdev._fp;

		// Set passed-in values that are not set in the base GRPSDevice class...

	/* Need to make separate call or add props...
		setDrawingLimits ( draw_limits, units, flag );
		setDataLimits ( data_limits );
		_devyshift = dev.getLimits().getTopY ();
		if ( Message.isDebugOn ) {
			Message.printDebug ( 10, routine, "Device height is " +
			_devyshift );
		}
	*/
		// Set the newline...

		if (string.ReferenceEquals(_nl, null))
		{
			_nl = System.getProperty("line.separator");
		}

		// Set the shared font data...

		if (_psfonts == null)
		{
			_psfonts = new GRPSFontData[14];
			_psfonts[0] = new GRPSFontData(this, "courier", "Courier");
			_psfonts[1] = new GRPSFontData(this, "courier-bold", "Courier-Bold");
			_psfonts[2] = new GRPSFontData(this, "courier-boldoblique", "Courier-BoldOblique");
			_psfonts[3] = new GRPSFontData(this, "courier-oblique", "Courier-Oblique");
			_psfonts[4] = new GRPSFontData(this, "helvetica", "Helvetica");
			_psfonts[5] = new GRPSFontData(this, "helvetica-bold", "Helvetica-Bold");
			_psfonts[6] = new GRPSFontData(this, "helvetica-boldoblique", "Helvetica-BoldOblique");
			_psfonts[7] = new GRPSFontData(this, "helvetica-oblique", "Helvetica-Oblique");
			_psfonts[8] = new GRPSFontData(this, "symbol", "Symbol");
			_psfonts[9] = new GRPSFontData(this, "times", "Times-Roman");
			_psfonts[10] = new GRPSFontData(this, "times-bold", "Times-Bold");
			_psfonts[11] = new GRPSFontData(this, "times-bolditalic", "Times-BoldItalic");
			_psfonts[12] = new GRPSFontData(this, "times-italic", "Times-Italic");
			_psfonts[13] = new GRPSFontData(this, "times-roman", "Times-Roman");
		}
	}

	/// <summary>
	/// Draws a line to a point from the last-drawn or position point. </summary>
	/// <param name="x"> the x coordinate </param>
	/// <param name="y"> the y coordinate </param>
	public override void lineTo(double x, double y)
	{
		_fp.print(x + " " + y + " LT" + _nl);
		++_psdev._LineTo_count;
		if (_psdev._LineTo_count >= GRPSDevice._MAXLineTo_count)
		{
			stroke();
			moveTo(x, y);
		}
	}

	/// <summary>
	/// Draws a line to a point from the last-drawn or positioned point. </summary>
	/// <param name="point"> GRPoint defining where to draw to. </param>
	public override void lineTo(GRPoint point)
	{
		lineTo(point.getX(), point.getY());
	}

	/// <summary>
	/// Moves the pen to a point </summary>
	/// <param name="x"> the x coordinate to move to </param>
	/// <param name="y"> the y coordinate to move to </param>
	public override void moveTo(double x, double y)
	{
		_fp.print(x + " " + y + " MT" + _nl);
	}

	/// <summary>
	/// Moves the pen to a point </summary>
	/// <param name="point"> the GRPoint to move to </param>
	public override void moveTo(GRPoint point)
	{
		moveTo(point.getX(), point.getY());
	}

	/// <summary>
	/// Ends the page.
	/// </summary>
	public override void pageEnd()
	{
		_psdev.pageEnd();
	}

	/// <summary>
	/// Starts the page.
	/// </summary>
	public override void pageStart()
	{
		_psdev.pageStart();
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public virtual void print()
	{
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public override void setClip(GRLimits deviceLimits)
	{
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public override void setClip(Shape clipShape)
	{
	}

	/// <summary>
	/// Set the current color. </summary>
	/// <param name="color"> GRColor to use. </param>
	/// <seealso cref= GRColor </seealso>
	public override void setColor(GRColor color)
	{
		if (color == null)
		{
			string routine = "GRPSDrawingArea.setColor";
			Message.printWarning(2, routine, "Null color");
		}
		_color = color;
		setColor((double)color.getRed() / 255.0, (double)color.getGreen() / 255.0, (double)color.getBlue() / 255.0);
	}

	/// <summary>
	/// Set the current color. </summary>
	/// <param name="r"> Red component in range 0.0 to 1.0. </param>
	/// <param name="g"> Green component in range 0.0 to 1.0. </param>
	/// <param name="b"> Blue component in range 0.0 to 1.0. </param>
	public override void setColor(double r, double g, double b)
	{
		_color = new GRColor(r, g, b);
		_fp.print(r + " " + g + " " + b + " setrgbcolor" + _nl);
	}

	/// <summary>
	/// Set the current color. </summary>
	/// <param name="r"> Red component in range 0.0 to 1.0. </param>
	/// <param name="g"> Green component in range 0.0 to 1.0. </param>
	/// <param name="b"> Blue component in range 0.0 to 1.0. </param>
	public override void setColor(float r, float g, float b)
	{
		setColor((double)r, (double)g, (double)b);
	}

	/// <summary>
	/// Set the font for the drawing area. </summary>
	/// <param name="font"> Font name (e.g., "Helvetica"). </param>
	/// <param name="style"> Font style ("Plain", "Bold", or "Italic").  Currently this is
	/// ignored for PostScript. </param>
	/// <param name="fontht"> Font height in points. </param>
	public override void setFont(string font, string style, double fontht)
	{
		int nfonts = _psfonts.Length;
		for (int i = 0; i < nfonts; i++)
		{
			if (font.Equals(_psfonts[i].grname, StringComparison.OrdinalIgnoreCase))
			{
				_font = font;
				_fontht = fontht;
				_fp.print("/" + _psfonts[i].psname + " findfont " + StringUtil.formatString(fontht,"%.3f") + " scalefont setfont" + _nl);
			}
		}
	}

	/// <summary>
	/// Sets the line cap style, as defined in GRDrawingAreaUtil.CAP* </summary>
	/// <param name="linecap"> the linecap style. </param>
	public virtual void setLineCap(int linecap)
	{
		if (linecap == GRDrawingAreaUtil.CAP_BUTT)
		{
			_fp.print("0 setlinecap" + _nl);
		}
		else if (linecap == GRDrawingAreaUtil.CAP_ROUND)
		{
			_fp.print("1 setlinecap" + _nl);
		}
		else if (linecap == GRDrawingAreaUtil.CAP_PROJECT)
		{
			_fp.print("2 setlinecap" + _nl);
		}
	}

	/// <summary>
	/// Sets the line dash pattern. </summary>
	/// <param name="dash"> array defining the dash pattern </param>
	/// <param name="offset"> line offset. </param>
	public override void setLineDash(double[] dash, double offset)
	{
		if (dash == null)
		{
			// Set to solid line...
			_fp.print("[] 0 setdash" + _nl);
			return;
		}
		int ndash = dash.Length;
		if (ndash == 0)
		{
			// Set to solid line...
			_fp.print("[] 0 setdash" + _nl);
			return;
		}
		//if ( ndash < 1 ) {
			//return;
		//}
		_fp.print("[");
		for (int i = 0; i < ndash; i++)
		{
			_fp.print(dash[i] + " ");
		}
		_fp.print("] " + offset + " setdash" + _nl);
	}

	/// <summary>
	/// Sets the line join style. </summary>
	/// <param name="join"> the line join style, as definied in GRDrawingAreaUtil.JOIN* </param>
	public virtual void setLineJoin(int join)
	{
		if (join == GRDrawingAreaUtil.JOIN_MITER)
		{
			_fp.print("0 setlinejoin" + _nl);
		}
		else if (join == GRDrawingAreaUtil.JOIN_MITER)
		{
			_fp.print("1 setlinejoin" + _nl);
		}
		else if (join == GRDrawingAreaUtil.JOIN_MITER)
		{
			_fp.print("2 setlinejoin" + _nl);
		}
	}

	/// <summary>
	/// Sets the line width. </summary>
	/// <param name="linewidth"> the width of the line </param>
	public override void setLineWidth(double linewidth)
	{
		_fp.print("" + linewidth + " setlinewidth" + _nl);
		_linewidth = linewidth;
	}

	/// <summary>
	/// Stroke.
	/// </summary>
	public virtual void stroke()
	{
		_fp.print("ST" + _nl);
		_psdev._LineTo_count = 0;
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public virtual void grid(int nxg, double[] xg, int nyg, double[] yg, int flag)
	{
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public override double getYData(double ydev)
	{
		return -999.99;
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public override double getXData(double xdev)
	{
		return -999.99;
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public override int getUnits()
	{
		return -999;
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public override GRLimits getTextExtents(string text, int flag)
	{
		return null;
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public override void fillRectangle(double xll, double yll, double width, double height)
	{
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public override void fillRectangle(GRLimits limits)
	{
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public override void fillPolygon(int npts, double[] x, double[] y, int transparency)
	{
	}

	} // End class GRPSDrawingArea

}