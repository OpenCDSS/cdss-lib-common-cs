using System;
using System.Collections.Generic;

// GRCanvasDrawingArea - GR drawing area for GRCanvasDevice

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

// ---------------------------------------------------------------------------
// GRCanvasDrawingArea - GR drawing area for GRCanvasDevice
// ---------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file
// ---------------------------------------------------------------------------
// History:
//
// 10 Aug 1997	Steven A. Malers, RTi	Initial Java version as port of C++/C
//					code.
// 18 Jun 1999	SAM, RTi		Complete working version now that
//					GIS classes are nearing completion.
// 08 Jul 1999	SAM, RTi		Start optimizing code by using
//					_global* data rather than allocating
//					new arrays all the time.
// 25 May 2000	SAM, RTi		Add getTextExtents.
// 17 Apr 2001	SAM, RTi		Fix bug in drawArc() and fillArc()
//					where Java methods were not being
//					passed the correct values.
// 2001-10-12	SAM, RTi		Change so when setting a color, the
//					Graphics object uses the reference to
//					the color that is passed in rather than
//					creating a new color.  Not creating a
//					new color each time can increase
//					performance.
// 2002-01-07	SAM, RTi		Rename GRJavaDrawingArea to
//					GRCanvasDrawingArea and make appropriate
//					changes.
//					Change setFont() to take a style.
// 2002-12-19	SAM, RTi		Overload fillPolygon() to accept an
//					alpha parameter for transparency.
// 2002-12-20	J. Thomas Sapienza, RTi	Changed KColor to be a multiple of 8
//					in order to get transparency to work.
// ----------------------------------------------------------------------------
// 2003-05-02	JTS, RTi		Added some methods because GRDrawingArea
//					was made abstract.
// 2003-05-07	JTS, RTi 		Made changes following review by SAM.
// 2005-04-20	JTS, RTi		* Added setClip().
//					* Added getClip().
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.GR
{


	using Math;

	using PropList = RTi.Util.IO.PropList;

	using MathUtil = RTi.Util.Math.MathUtil;

	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// Drawing area for GRCanvasDevice.
	/// </summary>
	public class GRCanvasDrawingArea : GRDrawingArea
	{

	/// <summary>
	/// Font to use for drawing on this area.
	/// </summary>
	protected internal Font _font_obj = null;
	/// <summary>
	/// the GRCanvasDevice used for drawing.
	/// </summary>
	protected internal GRCanvasDevice _jdev = null;

	/// <summary>
	/// Constructor.  Builds a GRCanvasDrawingArea with default settings.
	/// </summary>
	public GRCanvasDrawingArea() : base()
	{
		string routine = "GRCanvasDrawingArea()";

		if (Message.isDebugOn)
		{
			Message.printDebug(10, routine, "Constructing using no arguments");
		}
		Message.printWarning(2, routine, "Should not use void constructor");
		initialize(null);
	}

	/// <summary>
	/// Constructor.  Creates a drawing area with the specified settings. </summary>
	/// <param name="dev"> the device on which to create this drawing area. </param>
	/// <param name="name"> the name of this drawing area </param>
	/// <param name="aspect"> the aspect with which this drawing area should draw </param>
	/// <param name="draw_limits"> the drawing limits of the drawing area </param>
	/// <param name="flag"> Modifier for drawing limits.  If GRLimits.UNIT, then the limits are
	/// assumed to be percentages of the device (0.0 to 1.0) and the units are not
	/// used. </param>
	/// <param name="data_limits"> the limits of this drawing area's data. </param>
	public GRCanvasDrawingArea(GRCanvasDevice dev, string name, int aspect, GRLimits draw_limits, int units, int flag, GRLimits data_limits) : base(dev, name, aspect, draw_limits, units, flag, data_limits)
	{ // Set the generic information in the base class...

		if (Message.isDebugOn)
		{
			string routine = "GRCanvasDrawingArea(...)";
			Message.printDebug(10, routine, "Constructing using all arguments, name=\"" + name + "\"");
		}
		initialize(dev);
	}

	/// <summary>
	/// Constructor.  Creates a drawing area with default settings. </summary>
	/// <param name="dev"> the device on which to create this drawing area (UNUSED). </param>
	/// <param name="props"> proplist of settings (UNUSED) </param>
	public GRCanvasDrawingArea(GRCanvasDevice dev, PropList props)
	{
		// TODO JTS - 2003-05-08 called by GRDeviceUtil.getNewDrawingArea()
	}

	/// <summary>
	/// Clears the drawing area.
	/// </summary>
	public override void clear()
	{
		Message.printWarning(1, "?", "not implemented");
	}

	/// <summary>
	/// Clips the drawing area.
	/// </summary>
	public virtual void clip(int flag)
	{
		Message.printWarning(1, "?", "not implemented");
	}

	/// <summary>
	/// Not used for visual devices.
	/// </summary>
	public override void comment(string comment)
	{
	}

	/// <summary>
	/// Draw an arc using the current color, line, etc. </summary>
	/// <param name="x"> X-coordinate of center. </param>
	/// <param name="y"> Y-coordinate of center. </param>
	/// <param name="rx"> X-radius. </param>
	/// <param name="ry"> Y-radius. </param>
	/// <param name="a1"> Initial angle to start drawing (0 is at 3 o'clock, then
	/// counterclockwise). </param>
	/// <param name="a2"> Ending angle. </param>
	public override void drawArc(double x, double y, double rx, double ry, double a1, double a2)
	{ // Java draws using the rectangle...
		_jdev._graphics.drawArc((int)(x - rx), (int)(y - ry), (int)(rx * 2.0), (int)(ry * 2.0), (int)a1, (int)a2);
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public virtual void drawCompoundText(IList<string> text, Color color, double x, double y, double angle, int flag)
	{
		Message.printWarning(1, "?", "not implemented");
	}

	/// <summary>
	/// Draw an arc using the current color, line, etc. </summary>
	/// <param name="a1"> Initial angle to start drawing (0 is at 3 o'clock, then
	/// counterclockwise). </param>
	/// <param name="a2"> Ending angle. </param>
	/// <param name="x"> X-coordinate of center. </param>
	/// <param name="rx"> X-radius. </param>
	/// <param name="y"> Y-coordinate of center. </param>
	/// <param name="ry"> Y-radius. </param>
	public override void fillArc(double x, double y, double rx, double ry, double a1, double a2, int fillmode)
	{ // Java draws using the rectangle...
		_jdev._graphics.fillArc((int)(x - rx), (int)(y - ry), (int)(rx * 2.0), (int)(ry * 2.0), (int)a1, (int)a2);
	}

	/// <summary>
	/// Fill a polygon with a solid color. </summary>
	/// <param name="npts"> Number of points defining the polygon. </param>
	/// <param name="x"> X-coordinates of polygon points, in device units. </param>
	/// <param name="y"> Y-coordinates of polygon points, in device units. </param>
	public override void fillPolygon(int npts, double[] x, double[] y)
	{
		int[] ix = new int[npts];
		int[] iy = new int[npts];
		for (int i = 0; i < npts; i++)
		{
			ix[i] = (int)x[i];
			iy[i] = (int)y[i];
		}
		_jdev._graphics.fillPolygon(ix, iy, npts);
	}

	/// <summary>
	/// Fill a polygon with a transparent color. </summary>
	/// <param name="npts"> Number of points defining the polygon. </param>
	/// <param name="x"> X-coordinates of polygon points, in device units. </param>
	/// <param name="y"> Y-coordinates of polygon points, in device units. </param>
	/// <param name="transparency"> 0 indicates opaque and 255 indicates fully transparent.
	/// The value alpha is calculated as (255 - transparency). </param>
	public override void fillPolygon(int npts, double[] x, double[] y, int transparency)
	{
		if (npts < 1)
		{
			return;
		}
		int alpha = 255 - transparency;
		if (alpha == 255)
		{
			// Totally opaque...
			fillPolygon(npts, x, y);
			return;
		}

		int[] ix = new int[npts];
		int[] iy = new int[npts];
		// 1. Find the high and low x and y bounds of the polygon in 
		//    order to create an off-screen image that is sized correctly
		//    to fit the Polygon, and little else.
		int loX = ix[0];
		int hiX = ix[0];
		int loY = iy[0];
		int hiY = iy[0];
		ix[0] = (int)x[0];
		iy[0] = (int)y[0];
		for (int i = 1; i < npts; i++)
		{
			ix[i] = (int)x[i];
			iy[i] = (int)y[i];
			if (ix[i] < loX)
			{
				loX = ix[i];
			}
			if (ix[i] > hiX)
			{
				hiX = ix[i];
			}
			if (iy[i] < loY)
			{
				loY = iy[i];
			}
			if (iy[i] > hiY)
			{
				hiY = iy[i];
			}
		}
		int width = hiX - loX;
		int height = hiY - loY;
		if ((width == 0) || (height == 0))
		{
			// Basically a line - just draw normal to avoid division by
			// zero elsewhere...
			_jdev._graphics.fillPolygon(ix, iy, npts);
			return;
		}
		// Some degree of transparency...

		// 2. Create the off-screen image based on the extents calculated in
		//    step 1.

		// Create a Canvas object in order to use its "createImage" method to
		// create an off-screen bufferable Image.  In fact, any object that
		// extends "Component" could be used here.
		//Canvas tempCanvas = new Canvas();
		//Image buffer = tempCanvas.createImage(width, height);
		// This step - don't we need the image from the original  off-screen
		// buffer - otherwise, how do the pixels "show through" in the image
		// that is drawn in the last step?
		Image buffer = _jdev.createImage(width, height);

		// 3. Translate the x and y coordinates of the Polygon in order to 
		//    draw it inside the buffer Image object.
		// tx will contain the translated x points, and ty will contain
		// the translated y points
		// Because ix, iy are created locally, just reuse the coordinates...

		for (int i = 0; i < npts; i++)
		{
			ix[i] = ix[i] - loX;
			iy[i] = iy[i] - loY;
		}

		// 4. Create a Graphics object from the buffer Image
		Graphics bg = buffer.getGraphics();

		// 5. In this step, a color is chosen to be the transparent color.  
		//    When the buffer Image is finally drawn on-screen, none of the 
		//    pixels that have the transparent color will be drawn.  Their 
		//    alpha levels will all be 0.
		//  
		//    This can be somewhat confusing, so here is an explanation:
		//    Because Image objects can only contain rectangular areas, 	
		//    Polygons drawn within Images will often have extra space left
		//    over outside of the Polygon.  For instance:
		// 
		//    +--------+           This Image was size to contain a 
		//    | /\__   |           single Polygon.  If this image were 
		//    |/    \__|           drawn to the screen, all of the space 
		//    |\  /\__/|           around the Polygon but within the 
		//    | \/     |           boundaries of the Image would be 
		//    +--------+           drawn, as well.
		//
		//    In order to avoid that problem, the base background color 
		//    (the area outside the polygon in the image above) is set to
		//    a color (K) that appears nowhere else in the image.  Then, the
		//    alpha value for every pixel that is color K is set to 0.  When
		//    this Image is later drawn to the screen, none of the space 
		//    around the polygon will appear because all the pixels of that 
		//    out-lying background space have been set to K, and are therefore
		//    transparent.  Only the polygon will appear to have been drawn.
		// 
		//    Incidentally, this is how transparent GIFs work.  
		//
		//    Note: colors that come close to being the same color as K will
		//    not be turned to transparent.  Only those pixels that have the
		//    exact same R, G and B will have their alpha levels set to 0.

		// This chooses a very dark gray/green - which is unlikely to be used.

		////////////////////////////////////////////////////////
		//  IMPORTANT NOTE:
		//  While in theory any color can be chosen to be the KColor, in 
		//  practice it's not the case.  Inconsistencies in the execution
		//  of the code revealed that in some cases Java was "rounding" 
		//  the RGB color values of the drawn pixels to the nearest multiple 
		//  of 8.  Color(1, 2, 1) became Color(0, 0, 0).  Color(10, 10, 10)
		//  became Color(8, 8, 8).
		//
		//  For this reason, the transparent color is set below to be a very
		//  dark green -- nearly black, and all of its pixel color values are
		//  multiples of 8.  
		//
		//  Don't change this, as it may render the code unworkable in the
		//  the future.  
		//
		//  The reason for why Java does this isn't clear, but it's a fact
		//  of working with JDK 1.1.8.
		Color Kcolor = new Color(0, 8, 0);

		// Now fill the buffer Image with this color.
		bg.setColor(Kcolor);
		bg.fillRect(0, 0, width, height);

		// 6. Draw the Polygon into the buffer Image using the current color.
		bg.setColor(_color);
		bg.fillPolygon(ix, iy, npts);

		// 7. Using a PixelGrabber, get the pixels from the buffer Image 
		//    as a 1-D array of integers.  A PixelGrabber starts a separate
		//    thread and uses it to get all of the pixels out of the image.
		//
		//    Each int contains the color and alpha information for the pixel
		//    it represents.  
		//
		//    - Alpha values are stored in the first 8 bits
		//    - Red is stored in the second 8 bits
		//    - Green is stored in the third 8 bits
		//    - Blue is stored in the lowest 8 bits
		int[] pixels = new int[width * height];
		PixelGrabber grabber = new PixelGrabber(buffer.getSource(), 0, 0, width, height, pixels, 0, width);
		try
		{ // Grab the pixels and put them into the "pixels" array...
			grabber.grabPixels();
		}
		catch (Exception)
		{
			// An exception is thrown if the thread getting the pixels is
			// interrupted.  This should happen rarely, if ever.  
			//
			// Print error messages, but do not continue 
			// processing this image for drawing transparently.  
			//
			// The best thing to do is probably to simply paint
			// the polygon non-transparently.
			fillPolygon(npts, x, y);
		}

		// 8. Now the pixels have been pulled out of the Image, but they 
		//    all have alpha values that render them completely opaque.  
		//    Each pixel gets checked to see if it has the K color or not.
		//
		//    If a pixel has the K color, its alpha value gets set to 0 and
		//    it is completely transparent.  If not, their alpha value gets
		//    set to the 'alpha' variable passed into this method.
		int numPixels = width * height;

		// Calculate the integer value of the bitwise Or of the Kcolor's color
		// values.  This will be used to check for which pixels should be set
		// to transparent alphas in the final image.  These pixels will then
		// not be transferred.

		int K = (Kcolor.getRGB() & 0x00ffffff);
		int a = 0;
		for (int i = 0; i < numPixels; i++)
		{
			// If the non-alpha portion of the pixel is equal
			// to the K value, this pixel will be totally transparent and
			// will not be transferred to the final image.
			if ((pixels[i] & 0x00ffffff) == K)
			{
				a = 0;
			}
			else
			{ // Use the specified alpha for the pixel...
				a = alpha;
			}

			// Set the pixel to its own previous color, but with
			// the new alpha value.
			pixels[i] = (a << 24) | (pixels[i] & 0x00ffffff);
		}

		// 9. In order to turn an array of pixels back into an Image, a
		//    MemoryImageSource object is used.  Create that object, and
		//    then create an Image from it, in which the Polygon will be 
		//    drawn with its level of transparency, on top of a totally-
		//    transparent background.
		MemoryImageSource mis = new MemoryImageSource(width, height, pixels, 0, width);
		//Image misi = tempCanvas.createImage(mis);
		Image misi = _jdev.createImage(mis);

		// 10. Finally, draw the image on the screen, with no Image observer.
		//	Transparenent pixels in the image do not affect the existing
		//	pixels.
		_jdev._graphics.drawImage(misi, loX, loY, null);
	}

	/// <summary>
	/// Fills a rectangle in the current color. </summary>
	/// <param name="limits"> limits defining the rectangle to fill. </param>
	public override void fillRectangle(GRLimits limits)
	{
		string routine = "GRCanvasDrawingArea.fillRectangle(GRLimits)";

		if (limits == null)
		{
			return;
		}
		if (Message.isDebugOn)
		{
			Message.printDebug(10, routine, "Filling rectangle for limits " + limits.ToString());
		}
		fillRectangle(limits.getLeftX(), limits.getBottomY(), limits.getWidth(), limits.getHeight());
	}

	/// <summary>
	/// Fills a rectangle in the current color. </summary>
	/// <param name="xll"> the lower-left x position of the rectangle. </param>
	/// <param name="yll"> the lower-left y position of the rectangle. </param>
	/// <param name="width"> the width of the rectangle. </param>
	/// <param name="height"> the height of the rectangle. </param>
	public override void fillRectangle(double xll, double yll, double width, double height)
	{
		if (Message.isDebugOn)
		{
			string routine = "GRCanvasDrawingArea.fillRectangle(x,y,w,h)";
			Message.printDebug(10, routine, "Filling rectangle at " + xll + "," + yll + " w=" + width + " w=" + height);
		}
		_jdev._graphics.fillRect((int)xll, (int)yll, (int)width, (int)height);
	}

	/// <summary>
	/// Finalize before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~GRCanvasDrawingArea()
	{
		_font_obj = null;
		_jdev = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Flush graphics.  Does not seem to be necessary with Java.
	/// </summary>
	public override void flush()
	{
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public override Shape getClip()
	{
		return null;
	}

	/// <summary>
	/// Returns the data extents given a delta in DA units.
	/// This routine takes as input delta-x and delta-y values and calculates the
	/// corresponding data extents.  This is useful when it is known (guessed?) that
	/// output needs to be, say, 15 points high but it is not known what the
	/// corresponding data values are.  This can be used, for example, to draw a box
	/// around text
	/// The flags need to be implemented to allow the extents to be determined 
	/// exactly at the limits given, at the centroid of the drawing area, etc.  
	/// For now, calculate at the centroid so that projection issues do not cause
	/// problems. </summary>
	/// <param name="limits"> the limits for the drawing area. </param>
	/// <param name="flag"> indicates whether units should be returned in device or data 
	/// units.  Always defaults to GRUnits.DATA
	/// REVISIT (JTS - 2003-05-05)
	/// This parameter isn't even used.
	/// SAM:
	/// Revisit it later. </param>
	/// <returns> the data extents given a delta in DA units. </returns>
	public override GRLimits getDataExtents(GRLimits limits, int flag)
	{
		string routine = "GRCanvasDrawingArea.getDataExtents";

		// For now, default to getting the limits at the center of the drawing
		// area...

		if (Message.isDebugOn)
		{
			Message.printDebug(10, routine, "Getting DA extents for " + limits);
		}
		GRPoint xymin = getDataXY(((_plotx2 + _plotx1) / 2.0 - limits.getWidth() / 2.0), ((_ploty2 + _ploty1) / 2.0 - limits.getHeight() / 2.0), COORD_PLOT);
		GRPoint xymax = getDataXY(((_plotx2 + _plotx1) / 2.0 + limits.getWidth() / 2.0), ((_ploty2 + _ploty1) / 2.0 + limits.getHeight() / 2.0), COORD_PLOT);

		 GRLimits datalim = new GRLimits(xymin.getX(), xymin.getY(), xymax.getX(), xymax.getY());

		if (Message.isDebugOn)
		{
			Message.printDebug(10, routine, "Data limits are " + datalim);
		}
		return datalim;
	}

	/// <summary>
	/// Get the data units given device units. </summary>
	/// <param name="devx"> x coordinate in device units. </param>
	/// <param name="devy"> y coordinate in device units. </param>
	/// <param name="flag"> indicates whether coordinates are originating from the device
	/// or internally. </param>
	/// <returns> the data units given device units. </returns>
	public override GRPoint getDataXY(double devx, double devy, int flag)
	{
		string routine = "GRCanvasDrawingArea.getDataXY";

		// Interpolate to get the linearized X data value...

		double x = 0.0, y = 0.0;
		x = MathUtil.interpolate(devx, _plotx1, _plotx2, _linearx1, _linearx2);

		// Now we convert back to the data scale...

		if (_axisx == GRAxis.LOG)
		{
			x = Math.Pow(10.0, x);
		}
		else if (_axisx == GRAxis.STANDARD_NORMAL_PROBABILITY)
		{
			Message.printWarning(1, routine, "probability axis is not implemented");
			// later... *x = GRFuncStdNormCum ( *x );
		}

		// Interpolate to get the linearized Y data value...

		if (flag == COORD_DEVICE)
		{
			y = MathUtil.interpolate((_jdev._devy2 - devy), _ploty1, _ploty2, _lineary1, _lineary2);
		}
		else
		{
			y = MathUtil.interpolate(devy, _ploty1, _ploty2, _lineary1, _lineary2);
		}

		// Now we convert back to the data scale...

		if (_axisy == GRAxis.LOG)
		{
			y = Math.Pow(10.0, y);
		}
		else if (_axisy == GRAxis.STANDARD_NORMAL_PROBABILITY)
		{
			Message.printWarning(1, routine, "probability axis is not implemented");
			//y = GRFuncStdNormCum ( *y );
		}
		return new GRPoint(x, y);
	}

	/// <returns> size of string. </returns>
	/// <param name="text"> String to evaluate. </param>
	/// <param name="flag"> GRUnits.DATA or GRUnits.DEV, indicating units for returned size.
	/// Currently, device units (GRUnits.DEV) are always returned and the 
	/// GR.getTextExtents call scales to the device.
	/// REVISIT (JTS - 2003-05-05)
	/// This parameter isn't even used
	/// SAM:
	/// OK -- it's documented.  Revisit later. </param>
	public override GRLimits getTextExtents(string text, int flag)
	{
		// The font size is not scalable and is in device units.  Use the
		// Java API to get the size information...

		if (_jdev == null)
		{
			Message.printWarning(2, "GRCanvasDrawingArea.getTextExtents", "NULL _jdev");
			return null;
		}
		if (_jdev._graphics == null)
		{
			Message.printWarning(2, "GRCanvasDrawingArea.getTextExtents", "NULL _jdev._graphics");
			return null;
		}
		FontMetrics fm = _jdev._graphics.getFontMetrics();
		if (fm == null)
		{
			Message.printWarning(2, "GRCanvasDrawingArea.getTextExtents", "NULL FontMetrics");
			return null;
		}
		int width = fm.stringWidth(text);
		int height = fm.getAscent();

		return new GRLimits((double)width, (double)height);
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public override double getXData(double xdev)
	{
		Message.printWarning(1, "?", "not implemented");
		return 0.0;
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public override double getYData(double ydev)
	{
		Message.printWarning(1, "?", "not implemented");
		return 0.0;
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public virtual void grid(int nxg, double[] xg, int nyg, double[] yg, int flag)
	{
		Message.printWarning(1, "?", "not implemented");
	}

	/// <summary>
	/// Initialize the Java drawing area data.  Rely on the base class for the most
	/// part when it calls its initialize routine from its constructor. </summary>
	/// <param name="dev"> the GRCanvasDevice to use with this drawing area. </param>
	private void initialize(GRCanvasDevice dev)
	{
		string routine = "GRCanvasDrawingArea.initialize";
		int dl = 10;

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Initializing");
		}

		_font_obj = null;
		_jdev = dev;
	}

	/// <summary>
	/// Draw a line. </summary>
	/// <param name="x"> array of x points. </param>
	/// <param name="y"> array of y points. </param>
	public override void drawLine(double[] x, double[] y)
	{
		// Scale to device units...
		//double xs1 = scaleXData ( x[0] );
		//double ys1 = scaleYData ( y[0] );
		//double xs2 = scaleXData ( x[1] );
		//double ys2 = scaleYData ( y[1] );
		double xs1 = x[0];
		double ys1 = y[0];
		double xs2 = x[1];
		double ys2 = y[1];
		// Set the color...
		//graphics.setColor ( _color );
		// Draw the line...
		_jdev._graphics.drawLine((int)xs1, (int)ys1, (int)xs2, (int)ys2);
		_lastx = x[1];
		_lasty = y[1];
		_lastxp = xs2;
		_lastyp = ys2;
	}

	/// <summary>
	/// Draws a line to a point from the last-drawn or position point. </summary>
	/// <param name="x"> the x coordinate </param>
	/// <param name="y"> the y coordinate </param>
	public override void lineTo(double x, double y)
	{
		// Scale to device units...
		//double xs = scaleXData ( x );
		//double ys = scaleYData ( y );
		double xs = x;
		double ys = y;
		// Draw line to point...
		_jdev._graphics.drawLine((int)_lastxp, (int)(_lastyp), (int)xs, (int)ys);
		// Save coordintes...
		_lastx = x;
		_lasty = y;
		_lastxp = xs;
		_lastyp = ys;
	}

	/// <summary>
	/// Draws a line to a point from the last-drawn or positioned point. </summary>
	/// <param name="point"> GRPoint defining where to draw to. </param>
	public override void lineTo(GRPoint point)
	{
		lineTo(point.x, point.y);
	}

	/// <summary>
	/// Moves the pen to a point </summary>
	/// <param name="x"> the x coordinate to move to </param>
	/// <param name="y"> the y coordinate to move to </param>
	public override void moveTo(double x, double y)
	{
		// Scale to device units...
		//double xs = scaleXData ( x );
		//double ys = scaleYData ( y );
		double xs = x;
		double ys = y;
		// Save coordintes...
		_lastx = x;
		_lasty = y;
		_lastxp = xs;
		_lastyp = ys;
	}

	/// <summary>
	/// Moves the pen to a point </summary>
	/// <param name="point"> the GRPoint to move to </param>
	public override void moveTo(GRPoint point)
	{
		moveTo(point.getX(), point.getY());
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public override void pageEnd()
	{
		Message.printWarning(1, "?", "not implemented");
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public override void pageStart()
	{
		Message.printWarning(1, "?", "not implemented");
	}

	/// <summary>
	/// Draws a polygon. </summary>
	/// <param name="npts"> the number of points in the polygon. </param>
	/// <param name="x"> an array of x coordinates </param>
	/// <param name="y"> an array of y coordinates. </param>
	public override void drawPolygon(int npts, double[] x, double[] y)
	{
		// First draw a polyline for the given points...
		drawPolyline(npts, x, y);
		// Now connect the first and last points if necessary...
		if ((x[0] != y[npts - 1]) || (y[0] != y[npts - 1]))
		{
			lineTo(x[0], y[0]);
		}
	}

	/// <summary>
	/// Draws a polyline </summary>
	/// <param name="npts"> the number of points in the polyline </param>
	/// <param name="x"> an array of x coordinates </param>
	/// <param name="y"> an array of y coordinates. </param>
	public override void drawPolyline(int npts, double[] x, double[] y)
	{
		if (npts < 2)
		{
			return;
		}
		// Now loop through the line segments
		double xs1, xs2, ys1, ys2;
		// Scale the first point to device units...
		//xs1 = scaleXData ( x[0] );
		//ys1 = scaleYData ( y[0] );
		xs1 = x[0];
		ys1 = y[0];
		_lastxp = xs1;
		_lastyp = ys1;
		for (int i = 1; i < npts; i++)
		{
			// Scale to device units...
			//xs2 = scaleXData ( x[i] );
			//ys2 = scaleYData ( y[i] );
			xs2 = x[i];
			ys2 = y[i];
			// Draw line to point...
			_jdev._graphics.drawLine((int)_lastxp,(int)_lastyp, (int)xs2, (int)ys2);
			// Save last coordinates...
			_lastxp = xs2;
			_lastyp = ys2;
		}
		_lastx = x[npts - 1];
		_lasty = y[npts - 1];
	}

	/// <summary>
	/// Draws a rectangle in the current color. </summary>
	/// <param name="xll"> the lower-left x coordinate </param>
	/// <param name="yll"> the lower-left y coordinate </param>
	/// <param name="width"> the width of the rectangle </param>
	/// <param name="height"> the height of the rectangle </param>
	public override void drawRectangle(double xll, double yll, double width, double height)
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
	/// Draw text in the current color. </summary>
	/// <param name="text"> the text to draw </param>
	/// <param name="x"> the x position of the text </param>
	/// <param name="y"> the y position of the text </param>
	/// <param name="a"> unused </param>
	/// <param name="flag"> specifies the GRText.* position of the text relative to the point </param>
	public override void drawText(string text, double x, double y, double a, int flag)
	{
		drawText(text, x, y, a, flag, 0);
	}

	/// <summary>
	/// Draw text in the current color. </summary>
	/// <param name="text"> the text to draw </param>
	/// <param name="x"> the x position of the text </param>
	/// <param name="y"> the y position of the text </param>
	/// <param name="a"> unused </param>
	/// <param name="flag"> specifies the GRText.* position of the text relative to the point </param>
	/// <param name="rotationDegrees"> UNUSED </param>
	public override void drawText(string text, double x, double y, double a, int flag, double rotationDegrees)
	{
		// Make sure that we have a string...

		if (string.ReferenceEquals(text, null))
		{
			return;
		}

		// Scale the plotting point...

		//double xs = scaleXData ( x );
		//double ys = scaleYData ( y );
		double xs = x;
		double ys = y;

		// Figure out the size of the text...

		FontMetrics fm = _jdev._graphics.getFontMetrics();
		int width = fm.stringWidth(text);
		int height = fm.getAscent();

		int ix = (int)xs;
		int iy = (int)ys;

		if ((flag & GRText.CENTER_X) != 0)
		{
			ix = (int)xs - width / 2;
		}
		else if ((flag & GRText.RIGHT) != 0)
		{
			ix = (int)xs - width;
		}
		if ((flag & GRText.CENTER_Y) != 0)
		{
			iy += height / 2;
		}
		else if ((flag & GRText.TOP) != 0)
		{
			iy += height;
		}
		_jdev._graphics.drawString(text, ix, iy);
	}

	/// <summary>
	/// Scale x data value to device plotting coordinate. </summary>
	/// <param name="xdata"> x data value to scale </param>
	public override double scaleXData(double xdata)
	{
		return MathUtil.interpolate(xdata, _datax1, _datax2, _plotx1, _plotx2);
			// , GRAxis.X );
	}

	/// <summary>
	/// Scale y data vlaue to device plotting coordinate. </summary>
	/// <param name="ydata"> y data value to scale. </param>
	public override double scaleYData(double ydata)
	{
		return (_jdev._devy2 - MathUtil.interpolate(ydata, _datay1, _datay2, _ploty1, _ploty2));
			//, GRAxis.Y ));
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
	public override void setClip(Shape shape)
	{
	}

	/// <summary>
	/// Set the active color using a color. </summary>
	/// <param name="color"> Color to use. </param>
	public override void setColor(GRColor color)
	{
		_color = color;
		_jdev._graphics.setColor((Color)color);
		// Don't do the following to increase performance (SAM 2001-10-12)...
		//_jdev._graphics.setColor ( new Color ( _color.getRed(),
		//	_color.getGreen(), _color.getBlue() ) );
	}

	/// <summary>
	/// Set the active color using the RGB components. </summary>
	/// <param name="r"> Red component, 0.0 to 1.0. </param>
	/// <param name="g"> Green component, 0.0 to 1.0. </param>
	/// <param name="b"> Blue component, 0.0 to 1.0. </param>
	public override void setColor(float r, float g, float b)
	{
		GRColor color = new GRColor(r, g, b);
		_color = color;
		_jdev._graphics.setColor(_color);
	}

	//	int setCursor (		int, GRColor &, GRColor & );

	//	int setDrawMask (	int drawmask );

	/// <summary>
	/// Set the font for the drawing area. </summary>
	/// <param name="font"> Font name (e.g., "Helvetica"). </param>
	/// <param name="style"> Font style ("Plain", "Bold", or "Italic"). </param>
	/// <param name="height"> Font height in points. </param>
	public override void setFont(string font, string style, double height)
	{
		if (string.ReferenceEquals(font, null))
		{
			font = "Helvetica";
		}
		if (string.ReferenceEquals(style, null))
		{
			style = "Plain";
		}
		if (style.Equals("Plain", StringComparison.OrdinalIgnoreCase))
		{
			setFont(font, Font.PLAIN, (int)height);
		}
		else if (style.Equals("PlainItalic", StringComparison.OrdinalIgnoreCase))
		{
			setFont(font, Font.PLAIN | Font.ITALIC, (int)height);
		}
		else if (style.Equals("Bold", StringComparison.OrdinalIgnoreCase))
		{
			setFont(font, Font.BOLD, (int)height);
		}
		else if (style.Equals("BoldItalic", StringComparison.OrdinalIgnoreCase))
		{
			setFont(font, Font.BOLD | Font.ITALIC, (int)height);
		}
		else if (style.Equals("Italic", StringComparison.OrdinalIgnoreCase))
		{
			setFont(font, Font.ITALIC, (int)height);
		}
	}

	/// <summary>
	/// Set the font using a Java style call. </summary>
	/// <param name="name"> the name of the Font </param>
	/// <param name="style"> the style of the Font </param>
	/// <param name="size"> the size of the Font </param>
	public virtual void setFont(string name, int style, int size)
	{
		_font_obj = new Font(name, style, size);
		if (_jdev == null)
		{
			return;
		}
		if (_jdev._graphics == null)
		{
			return;
		}
		//Message.printStatus ( 1, "da.setFont", "Set font to " + name +
		//	" size " + size );
		_jdev._graphics.setFont(_font_obj);
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public override void setLineDash(double[] dash, double offset)
	{
		Message.printWarning(100, "?", "not implemented");
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public virtual void setLineDash(int ndash, double[] dash, double offset)
	{
		Message.printWarning(100, "?", "not implemented");
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public virtual void setLineJoin(int join)
	{
		Message.printWarning(100, "?", "not implemented");
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public override void setLineWidth(double linewidth)
	{
		Message.printWarning(100, "?", "not implemented");
		_linewidth = linewidth;
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
	public override int getUnits()
	{
		return 0;
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public override void drawCompoundText(IList<string> text, GRColor color, double x, double y, double angle, int flag)
	{
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public override void clip(bool clip)
	{
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public override void setColor(double R, double G, double B)
	{
	}


	}

}