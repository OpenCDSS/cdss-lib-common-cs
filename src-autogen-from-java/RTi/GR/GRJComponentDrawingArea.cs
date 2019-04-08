using System;
using System.Collections.Generic;

// GRJComponentDrawingArea - GR drawing area to draw in a JComponent

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
// GRJComponentDrawingArea - GR drawing area to draw in a JComponent
// ---------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
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
// ---------------------------------------------------------------------------
// 2002-01-07	SAM, RTi		Copy GRJavaDrawingArea and make
//					necessary changes to work with
//					JComponent devices rather than Canvas.
//					Change setFont() to take a style.
// ----------------------------------------------------------------------------
// 2003-05-01	J. Thomas Sapienza, RTi	Made changes to accomodate the massive
//					restructuring of GR.java.
// 2003-05-02	JTS, RTi		Added some methods because GRDrawingArea
//					was made abstract.
// 2004-03-19	JTS, RTi		Change to using Graphics2D.
// 2004-03-22	JTS, RTi		* Implemented dashed lines.
//					* Implemented line joins.
//					* Implemented line widths.
//					* Made the invalid constructor private
//					  so it can't be called.
//					* Implemented clear().
//					* Implemented line caps().
//					* Added drawOval() and fillOval(), which
//					  can draw circles much more cleanly 
//					  than the arc code.
// 2004-04-20	JTS, RTi		Added drawAnnotation().
// 2004-05-25	JTS, RTi		Added support for logarithmic scaling
//					in the scaleXData() and scaleYData()
//					methods.
// 2004-11-18	JTS, RTi		Implemented the fillPolygon() method
//					with transparency.
// 2005-04-20	JTS, RTi		* Added setClip().
//					* Added getClip().
// 2005-10-05	JTS, RTi		Added support for drawing Symbol
//					annotations.
// 2005-10-25	JTS, RTi		* Added support for Date input formats.
//					* Greatly expanded the documentation 
//					  in the drawAnnotation() method.
//					* Added getAnnotationPosition() to 
//					  eliminate redundant code.
// 2005-11-02	JTS, RTi		* Massive restructuring of the code
//					  following review by SAM.
//					* Added Y support for DateTime formats.
//					* Added RepeatDate and RepeatPercent
//					  formats as proof-of-concept, and to
//					  test the changes to make sure they 
//					  worked in both dimensions.
// 2005-11-28	JTS, RTi		* Corrected null pointer errors caused
//					  by not checking all prop values to
//					  see if they were null.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//					Fix setColor() overloads to actually persist setting.
// ----------------------------------------------------------------------------

namespace RTi.GR
{


	using Math;


	using PropList = RTi.Util.IO.PropList;

	using MathUtil = RTi.Util.Math.MathUtil;

	using Message = RTi.Util.Message.Message;

	using StringUtil = RTi.Util.String.StringUtil;

	using DateTime = RTi.Util.Time.DateTime;
	using DateTimeFormat = RTi.Util.Time.DateTimeFormat;

	/// <summary>
	/// Drawing area for a GRJComponentDevice.
	/// TODO (JTS - 2004-03-22) Possibly allow clipping and translation on an individual drawing area,
	/// rather than on the device as a whole.
	/// TODO (JTS - 2006-05-23) example of use (eg, ERDiagram_DrawingArea)
	/// </summary>
	public class GRJComponentDrawingArea : GRDrawingArea
	{

	/// <summary>
	/// Whether to disable scaling drawing amounts to data limits.
	/// </summary>
	private bool __scaleXData = true;
	private bool __scaleYData = true;

	/// <summary>
	/// Arrays that are used very often in drawing annotations. 
	/// Declared global for performance.  
	/// </summary>
	private double[] __xs = new double[2];
	private double[] __ys = new double[2];
	private double[] __tempXs = new double[2];
	private double[] __tempYs = new double[2];

	/// <summary>
	/// Font to use when drawing on this area.
	/// </summary>
	protected internal Font _fontObj = null;

	/// <summary>
	/// GRJComponentDevice used for drawing.
	/// </summary>
	protected internal GRJComponentDevice _jdev = null;

	/// <summary>
	/// Hashtable for caching DateTimeFormat objects, which can be expensive to create often.<para>
	/// <b>NOTE:</b> JTS tested the speed of creating a DateTimeFormat every time one
	/// is needed for drawing versus creating a single DateTimeFormat and caching it.
	/// Caching is over 500 times faster.  Creating a DateTimeFormat is an expensive
	/// operation and should be done rarely.  
	/// </para>
	/// </summary>
	private static Dictionary<string, DateTimeFormat> __dateFormatHashtable = new Dictionary<string, DateTimeFormat>();

	private readonly int __FORMAT_NONE = 0, __FORMAT_DATETIME = 1, __FORMAT_REPEAT_DATA = 2, __FORMAT_REPEAT_PERCENT = 3;

	/// <summary>
	/// Private so it can't be used.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") private GRJComponentDrawingArea()
	private GRJComponentDrawingArea() : base()
	{
		initialize(null);
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
	/// <param name="data_limits"> Data limits associated with the lower-left and upper-right
	/// corners of the drawing area. </param>
	public GRJComponentDrawingArea(GRJComponentDevice dev, string name, int aspect, GRLimits draw_limits, int units, int flag, GRLimits data_limits) : base(dev, name, aspect, draw_limits, units, flag, data_limits)
	{
		// Set the generic information in the base class...

		if (Message.isDebugOn)
		{
			string routine = "GRJComponentDrawingArea(...)";
			Message.printDebug(10, routine, "Constructing using all arguments, name=\"" + name + "\"");
		}
		initialize(dev);
	}

	/// <summary>
	/// Constructor.  Creates a drawing area with default settings. </summary>
	/// <param name="dev"> the device on which to create this drawing area. </param>
	/// <param name="props"> proplist of settings (UNUSED) </param>
	public GRJComponentDrawingArea(GRJComponentDevice dev, PropList props)
	{
		Message.printWarning(2, "GRJComponentDrawingArea", "This constructor " + "has not been completed -- it should not be used.  If " + "the calling code is using getNewDrawingArea, that method should be deprecated.");
		// TODO (JTS - 2003-05-08) called by GRDeviceUtil.getNewDrawingArea()
	}

	/// <summary>
	/// Calculates the font size that will fit within, but not over, the given number 
	/// of pixels in terms of the font's height. </summary>
	/// <returns> the font size (in points) of the font that will fit. </returns>
	public virtual int calculateFontSize(int pixels)
	{
		return calculateFontSize(_jdev._graphics, pixels);
	}

	/// <summary>
	/// Returns the font size that will fit within, but not over, the given number of
	/// pixels in terms of the font's height, using the given Graphics context. </summary>
	/// <returns> the font size (in points) of the font that will fit. </returns>
	public virtual int calculateFontSize(Graphics g, int pixels)
	{
		Font holdFont = getFont();
		string name = holdFont.getName();
		int style = holdFont.getStyle();

		int size = 0;
		FontMetrics fm;
		int height;

		for (size = 1; size < 100; size++)
		{
			setFont(name, style, size);
			fm = g.getFontMetrics();
			height = fm.getAscent();

			if (height > pixels)
			{
				if (size == 1)
				{
					return size;
				}
				else
				{
					return (size - 1);
				}
			}
		}
		return 100;
	}

	/// <summary>
	/// Clears the drawing area's canvas, filling it with white.
	/// </summary>
	public override void clear()
	{
		clear(GRColor.white);
	}

	/// <summary>
	/// Clears the drawing area's canvas, filling it with the specified color. </summary>
	/// <param name="color"> the color to fill the drawing area with. </param>
	public virtual void clear(GRColor color)
	{
		_jdev._graphics.setColor(color);
		_jdev._graphics.fillRect(0, 0, (int)(_drawx2 - _drawx1), (int)(_drawy2 - _drawy1));
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public override void clip(bool b)
	{
	}

	/// <summary>
	/// Not implemented.  Not used in visual devices.
	/// </summary>
	public override void comment(string comment)
	{
	}

	/// <summary>
	/// Draws an annotation as defined in the given PropList. </summary>
	/// <param name="p"> the PropList that defines a single annotation.
	/// TODO (JTS - 2006-05-23) Where are these properties defined? </param>
	public virtual void drawAnnotation(PropList p)
	{
		string routine = "GRJComponentDrawingArea.drawAnnotation";

		// Color property is common to all annotations. 
		string propVal = p.getValue("Color");
		GRColor color = null;
		if (string.ReferenceEquals(propVal, null))
		{
			color = GRColor.black;
		}
		else
		{
			color = GRColor.parseColor(propVal);
		}
		setColor(color);

		// OutlineColor property does not need to be defined.
		GRColor outlineColor = null;
		propVal = p.getValue("OutlineColor");
		if (string.ReferenceEquals(propVal, null) || propVal.Trim().Equals(""))
		{
			outlineColor = null;
		}
		else
		{
			outlineColor = GRColor.parseColor(propVal);
		}

		// XAxisSystem property is common to all annotations.
		propVal = p.getValue("XAxisSystem");
		bool xAxisPercent = false;
		if (string.ReferenceEquals(propVal, null))
		{
		}
		else if (propVal.Equals("Percent", StringComparison.OrdinalIgnoreCase))
		{
			xAxisPercent = true;
		}
		else if (propVal.Equals("Data", StringComparison.OrdinalIgnoreCase))
		{
			// default
		}
		else
		{
			Message.printWarning(2, "drawAnnotation", "Invalid XAxisSystem: " + propVal);
			return;
		}

		// YAxisSystem property is common to all annotations.
		propVal = p.getValue("YAxisSystem");
		bool yAxisPercent = false;
		if (string.ReferenceEquals(propVal, null))
		{
		}
		else if (propVal.Equals("Percent", StringComparison.OrdinalIgnoreCase))
		{
			yAxisPercent = true;
		}
		else if (propVal.Equals("Data", StringComparison.OrdinalIgnoreCase))
		{
			// default
		}
		else
		{
			Message.printWarning(2, "drawAnnotation", "Invalid YAxisSystem: " + propVal);
			return;
		}

		// Input Formats do not need to be defined.  If undefined, they are
		// set to null so that checking for whether they are defined or not
		// is easier later (just a "!= null").

		// The reason a boolean is not created to test whether an XFormat or
		// YFormat has been defined is because the XFormat and YFormat 
		// values are already being passed around with many other parameters,
		// and two more parameters with redundant information offer no
		// improvement.

		string xFormat = p.getValue("XFormat");
		if (string.ReferenceEquals(xFormat, null) || xFormat.Equals("") || xFormat.Equals("None", StringComparison.OrdinalIgnoreCase))
		{
			xFormat = null;
		}
		string yFormat = p.getValue("YFormat");
		if (string.ReferenceEquals(yFormat, null) || yFormat.Equals("") || yFormat.Equals("None", StringComparison.OrdinalIgnoreCase))
		{
			yFormat = null;
		}

		string shapeType = p.getValue("ShapeType");

		// These arrays are used for holding input format data in a neutral 
		// format, so that it can be passed to a helper method and processed.
		string[] formatXPts = new string[2];
		string[] formatYPts = new string[2];

		int xFormatType = __FORMAT_NONE;
		int yFormatType = __FORMAT_NONE;

		/////////////////////////////////////////////////////////////
		// The next section of code does operations on the "points" 
		// or "point" property, as appropriate for the shape type.
		// The code is here instead of below so that the common code
		// between "Symbol" and "Text" can be combined.
		//
		// The X and Y input formats are checked and if defined the X 
		// and/or Y values (as appropriate) are moved into the proper 
		// kind of variables.
		//
		// Additionally, this code does error checking on the input X and
		// Y values with their formats.  This ensures that no further 
		// processing is done if the input data or formats are incorrect.
		// Also, this means that in later methods it is guaranteed that the data are valid.

		// Notes on InputFormats:
		// - the format of the *InputFormat String is "INPUT TYPE,FORMAT".
		//   For example, "DateTime,mm-dd"
		// - However the input values are formatted in the Point or Points
		//   properties, they must not have commas. These properties are
		//   split apart based on commas, so any extras will foul up the process.
		// - Currently, only the "DateTime" format is supported.

		if (string.ReferenceEquals(shapeType, null))
		{
			// shape should always at least be an empty string, but
			// this will avoid any null pointer errors.
			return;
		}
		else if (shapeType.Equals("Line", StringComparison.OrdinalIgnoreCase) || shapeType.Equals("Rectangle", StringComparison.OrdinalIgnoreCase))
		{
			// Line and rectangle use a "Points" property in similar way
			propVal = p.getValue("Points");
			if (string.ReferenceEquals(propVal, null) || propVal.Equals(""))
			{
				Message.printWarning(2, "drawAnnotation", "Endpoints for a line annotation must be specified.");
				return;
			}

			IList<string> points = StringUtil.breakStringList(propVal, ",", 0);
			if (points.Count != 4)
			{
				Message.printWarning(2, "drawAnnotation", "Invalid points declaration:" + " " + propVal + ".  There must be 2 points (4 values) in the form 'X1,Y1,X2,Y2'.");
				return;
			}

			if ((string.ReferenceEquals(xFormat, null)) || StringUtil.startsWithIgnoreCase(xFormat, "RepeatData,") || StringUtil.startsWithIgnoreCase(xFormat, "RepeatPercent,"))
			{
				if (!string.ReferenceEquals(xFormat, null))
				{
					if (StringUtil.startsWithIgnoreCase(xFormat, "RepeatData,"))
					{
						xFormatType = __FORMAT_REPEAT_DATA;
						string s = xFormat.Substring(11, 1);
						if (s.Equals("-") || s.Equals("+"))
						{
						}
						else
						{
							Message.printWarning(3, "drawAnnotation", "Invalid format for RepeatData: " + xFormat);
							return;
						}
						s = xFormat.Substring(12);
						if (StringUtil.atod(s) == -1)
						{
							Message.printWarning(3, "drawAnnotation", "Invalid value for RepeatData: " + xFormat);
							return;
						}
					}
					else
					{
						   xFormatType = __FORMAT_REPEAT_PERCENT;
						string s = xFormat.Substring(14, 1);
						if (s.Equals("-") || s.Equals("+"))
						{
						}
						else
						{
							Message.printWarning(3, "drawAnnotation", "Invalid format for RepeatPercent: " + xFormat);
							return;
						}
						s = xFormat.Substring(15);
						if (StringUtil.atod(s) == -1)
						{
							Message.printWarning(3, "drawAnnotation", "Invalid value for RepeatPercent: " + xFormat);
							return;
						}
					}
				}

				// The simple case -- this is how annotations have 
				// always been defined, as a set of Data or Percent points.
				try
				{
					__xs[0] = (Convert.ToDouble((points[0])));
				}
				catch (Exception)
				{
					Message.printWarning(3, "drawAnnotation", "Invalid X1 value: " + points[0]);
					return;
				}
				try
				{
					__xs[1] = (Convert.ToDouble((points[2])));
				}
				catch (Exception)
				{
					Message.printWarning(2, "drawAnnotation", "Invalid X2 value: " + points[2]);
					return;
				}
			}
			else if (StringUtil.startsWithIgnoreCase(xFormat,"DateTime,"))
			{
				xFormatType = __FORMAT_DATETIME;
				// split the InputFormat string apart and pull out the date format.  
				int index = xFormat.IndexOf(",", StringComparison.Ordinal);
				string format = xFormat.Substring(index + 1);

				// see if a DateTimeFormat object has already been cached with the same date format.  

				// see performance note about caching at static declaration of Hashtable (top of class).

				DateTimeFormat dtf = (DateTimeFormat)__dateFormatHashtable[format];

				if (dtf == null)
				{
					// If not, cache this object.  The caching
					// process improves performance -- these objects take a lot to create.
	//				Message.printStatus(2, "", "No cached format found");
					dtf = new DateTimeFormat(format);
					__dateFormatHashtable[format] = dtf;
				}

				try
				{
					dtf.parse((string)points[0]);
				}
				catch (Exception e)
				{
					Message.printWarning(3, "drawAnnotation", "Invalid X1 value: " + points[0]);
					Message.printWarning(3, "drawAnnotation", e);
					return;
				}
				try
				{
					dtf.parse((string)points[2]);
				}
				catch (Exception e)
				{
					Message.printWarning(3, "drawAnnotation", "Invalid X2 value: " + points[2]);
					Message.printWarning(3, "drawAnnotation", e);
					return;
				}
				formatXPts[0] = (string)points[0];
				formatXPts[1] = (string)points[2];
			}

			if (string.ReferenceEquals(yFormat, null) || StringUtil.startsWithIgnoreCase(yFormat, "RepeatData,") || StringUtil.startsWithIgnoreCase(yFormat, "RepeatPercent,"))
			{
				if (!string.ReferenceEquals(yFormat, null))
				{
					if (StringUtil.startsWithIgnoreCase(yFormat, "RepeatData,"))
					{
						yFormatType = __FORMAT_REPEAT_DATA;
						string s = yFormat.Substring(11, 1);
						if (s.Equals("-") || s.Equals("+"))
						{
						}
						else
						{
							Message.printWarning(3, "drawAnnotation", "Invalid format for RepeatData: " + yFormat);
							return;
						}
						s = yFormat.Substring(12);
						if (StringUtil.atod(s) == -1)
						{
							Message.printWarning(3, "drawAnnotation", "Invalid value for RepeatData: " + yFormat);
							return;
						}
					}
					else
					{
						yFormatType = __FORMAT_REPEAT_PERCENT;
						string s = yFormat.Substring(14, 1);
						if (s.Equals("-") || s.Equals("+"))
						{
						}
						else
						{
							Message.printWarning(3, "drawAnnotation", "Invalid format for RepeatPercent: " + yFormat);
							return;
						}
						s = yFormat.Substring(15);
						if (StringUtil.atod(s) == -1)
						{
							Message.printWarning(3, "drawAnnotation", "Invalid value for RepeatPercent: " + yFormat);
							return;
						}
					}
				}

				// The simple case -- this is how annotations have 
				// always been defined, as a set of Data or Percent points.
				try
				{
					__ys[0] = (Convert.ToDouble(((string)points[1])));
				}
				catch (Exception)
				{
					Message.printWarning(3, "drawAnnotation", "Invalid Y1 value: " + points[1]);
					return;
				}
				try
				{
					__ys[1] = (Convert.ToDouble(((string)points[3])));
				}
				catch (Exception)
				{
					Message.printWarning(3, "drawAnnotation", "Invalid Y2 value: " + points[3]);
					return;
				}
			}
			else if (StringUtil.startsWithIgnoreCase(yFormat,"DateTime,"))
			{
				yFormatType = __FORMAT_DATETIME;
				// split the InputFormat string apart and pull out the date format.  
				int index = yFormat.IndexOf(",", StringComparison.Ordinal);
				string format = yFormat.Substring(index + 1);

				// see if a DateTimeFormat object has already been 
				// cached with the same date format.  

				// see performance note about caching at static declaration of Hashtable (top of class).

				DateTimeFormat dtf = (DateTimeFormat)__dateFormatHashtable[format];

				if (dtf == null)
				{
					// If not, cache this object.  The caching process improves performance -- these 
					// objects take a lot to create.
	//				Message.printStatus(2, "", "No cached format found");
					dtf = new DateTimeFormat(format);
					__dateFormatHashtable[format] = dtf;
				}

				try
				{
					dtf.parse((string)points[1]);
				}
				catch (Exception e)
				{
					Message.printWarning(3, "drawAnnotation", "Invalid Y1 value: " + points[1]);
					Message.printWarning(3, "drawAnnotation", e);
				}
				try
				{
					dtf.parse((string)points[3]);
				}
				catch (Exception e)
				{
					Message.printWarning(3, "drawAnnotation", "Invalid Y2 value: " + points[3]);
					Message.printWarning(3, "drawAnnotation", e);
				}
				formatYPts[0] = (string)points[1];
				formatYPts[1] = (string)points[3];
			}
		}
		else if (shapeType.Equals("Text", StringComparison.OrdinalIgnoreCase) || shapeType.Equals("Symbol", StringComparison.OrdinalIgnoreCase))
		{
			// Both these shapes have the "Point" property and both handle it the same.

			// Create a pretty-looking string for use in messages (i.e.,
			// ensures that capitalization is normalized).
			   if (shapeType.Equals("Text", StringComparison.OrdinalIgnoreCase))
			   {
				shapeType = "Text";
			   }
			else
			{
				shapeType = "Symbol";
			}

			propVal = p.getValue("Point");
			if (string.ReferenceEquals(propVal, null))
			{
				Message.printWarning(3, "drawAnnotation", shapeType + " point for a " + shapeType + " annotation must be specified.");
				return;
			}

			IList<string> point = StringUtil.breakStringList(propVal, ",", 0);
			if (point.Count != 2)
			{
				Message.printWarning(3, "drawAnnotation", "Invalid point declaration:" + " " + propVal + ".  There must be 2 points in the form 'X1,Y1'.");
				return;
			}

			if (string.ReferenceEquals(xFormat, null) || StringUtil.startsWithIgnoreCase(xFormat, "RepeatData,") || StringUtil.startsWithIgnoreCase(xFormat, "RepeatPercent,"))
			{
				if (!string.ReferenceEquals(xFormat, null))
				{
					if (StringUtil.startsWithIgnoreCase(xFormat,"RepeatData,"))
					{
						xFormatType = __FORMAT_REPEAT_DATA;
						string s = xFormat.Substring(11, 1);
						if (s.Equals("-") || s.Equals("+"))
						{
						}
						else
						{
							Message.printWarning(3, "drawAnnotation", "Invalid format for RepeatData: " + xFormat);
							return;
						}
						s = xFormat.Substring(12);
						if (StringUtil.atod(s) == -1)
						{
							Message.printWarning(3, "drawAnnotation", "Invalid value for RepeatData: " + xFormat);
							return;
						}
					}
					else
					{
						xFormatType = __FORMAT_REPEAT_PERCENT;
						string s = xFormat.Substring(14, 1);
						if (s.Equals("-") || s.Equals("+"))
						{
						}
						else
						{
							Message.printWarning(3, "drawAnnotation", "Invalid format for RepeatPercent: " + xFormat);
							return;
						}
						s = xFormat.Substring(15);
						if (StringUtil.atod(s) == -1)
						{
							Message.printWarning(3, "drawAnnotation", "Invalid value for RepeatPercent: " + xFormat);
							return;
						}
					}
				}

				// The simple case -- this is how annotations have 
				// always been defined, as a set of Data or Percent points.
				try
				{
					__xs[0] = (Convert.ToDouble(((string)point[0])));
				}
				catch (Exception)
				{
					Message.printWarning(3, "drawAnnotation", "Invalid X1 value: " + point[0]);
					return;
				}
			}
			else if (StringUtil.startsWithIgnoreCase(xFormat,"DateTime,"))
			{
				xFormatType = __FORMAT_DATETIME;
				// split the InputFormat string apart and pull out the date format.  
				int index = xFormat.IndexOf(",", StringComparison.Ordinal);
				string format = xFormat.Substring(index + 1);

				// see if a DateTimeFormat object has already been cached with the same date format.  

				// see performance note about caching at static declaration of Hashtable (top of class).

				DateTimeFormat dtf = (DateTimeFormat)__dateFormatHashtable[format];
				if (dtf == null)
				{
					// If not, cache this object.  The caching process improves performance -- these 
					// objects take a lot to create.
	//				Message.printStatus(2, "", "No cached format found");
					dtf = new DateTimeFormat(format);
					__dateFormatHashtable[format] = dtf;
				}
				try
				{
					dtf.parse((string)point[0]);
				}
				catch (Exception e)
				{
					Message.printWarning(2, "drawAnnotation", "Invalid X1 value: " + point[0]);
					Message.printWarning(3, routine, e);
				}
				formatXPts[0] = (string)point[0];
			}

			if (string.ReferenceEquals(yFormat, null) || StringUtil.startsWithIgnoreCase(yFormat, "RepeatData,") || StringUtil.startsWithIgnoreCase(yFormat, "RepeatPercent,"))
			{
				if (!string.ReferenceEquals(yFormat, null))
				{
					if (StringUtil.startsWithIgnoreCase(yFormat, "RepeatData,"))
					{
						yFormatType = __FORMAT_REPEAT_DATA;
						string s = yFormat.Substring(11, 1);
						if (s.Equals("-") || s.Equals("+"))
						{
						}
						else
						{
							Message.printWarning(3, "drawAnnotation", "Invalid format for RepeatData: " + yFormat);
							return;
						}
						s = yFormat.Substring(12);
						if (StringUtil.atod(s) == -1)
						{
							Message.printWarning(3, "drawAnnotation", "Invalid value for RepeatData: " + yFormat);
							return;
						}
					}
					else
					{
						yFormatType = __FORMAT_REPEAT_PERCENT;
						string s = yFormat.Substring(14, 1);
						if (s.Equals("-") || s.Equals("+"))
						{
						}
						else
						{
							Message.printWarning(3, "drawAnnotation", "Invalid format for RepeatPercent: " + yFormat);
							return;
						}
						s = yFormat.Substring(15);
						if (StringUtil.atod(s) == -1)
						{
							Message.printWarning(3, "drawAnnotation", "Invalid value for epeatPercent: " + yFormat);
							return;
						}
					}
				}

				// The simple case -- this is how annotations have 
				// always been defined, as a set of Data or Percent points.
				try
				{
					__ys[0] = (Convert.ToDouble(((string)point[1])));
				}
				catch (Exception)
				{
					Message.printWarning(3, "drawAnnotation", "Invalid Y1 value: " + point[1]);
					return;
				}
			}
			else if (StringUtil.startsWithIgnoreCase(yFormat,"DateTime,"))
			{
				yFormatType = __FORMAT_DATETIME;
				// split the InputFormat string apart and pull out the date format.  
					 int index = yFormat.IndexOf(",", StringComparison.Ordinal);
				string format = yFormat.Substring(index + 1);

				// see if a DateTimeFormat object has already been cached with the same date format.  

				// see performance note about caching at static declaration of Hashtable (top of class).

				DateTimeFormat dtf = (DateTimeFormat)__dateFormatHashtable[format];
				if (dtf == null)
				{
					// If not, cache this object.  The caching process improves performance -- these 
					// objects take a lot to create.
	//				Message.printStatus(2, "", "No cached format found");
					dtf = new DateTimeFormat(format);
					__dateFormatHashtable[format] = dtf;
				}
				try
				{
					dtf.parse((string)point[1]);
				}
				catch (Exception e)
				{
					Message.printWarning(2, "drawAnnotation", "Invalid Y1 value: " + point[1]);
					Message.printWarning(2, "drawAnnotation", e);
				}
				formatYPts[0] = (string)point[1];
			}
		}
		else
		{
			// Unknown shape encountered.
			Message.printWarning(2, "drawAnnotation", "Invalid shape type: " + shapeType);
			return;
		}

		// The next section gathers the remaining properties specific to
		// each shape type and sets up the code to actually draw the annotation.

		if (shapeType.Equals("Line", StringComparison.OrdinalIgnoreCase))
		{
			string lineStyle = p.getValue("LineStyle");
			if (string.ReferenceEquals(lineStyle, null))
			{
				// default
			}
			else if (lineStyle.Equals("None", StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			else if (lineStyle.Equals("Solid", StringComparison.OrdinalIgnoreCase))
			{
				// default
			}
			else
			{
				Message.printWarning(3, "drawAnnotation", "Invalid line style: " + lineStyle);
				return;
			}

			propVal = p.getValue("LineWidth");
			int width = 1;
			if (string.ReferenceEquals(propVal, null))
			{
				// default
			}
			else
			{
				width = StringUtil.atoi(propVal);
				if (width == -1)
				{
					Message.printWarning(3, "drawAnnotation", "Line width must be a positive " + "integer.  Invalid line width: " + propVal);
					return;
				}
			}

			if (xFormatType == __FORMAT_NONE && yFormatType == __FORMAT_NONE)
			{
				// No input format defined -- simple data/percent drawing.
				drawAnnotationLine(__xs, __ys, width, lineStyle, xAxisPercent, yAxisPercent);
			}
			else
			{
				// Date: 
				drawAnnotationLine(__xs, __ys, formatXPts, formatYPts, xFormat, yFormat, xFormatType, yFormatType, width, lineStyle, xAxisPercent, yAxisPercent);
			}
		}
		else if (shapeType.Equals("Rectangle", StringComparison.OrdinalIgnoreCase))
		{
			if (xFormatType == __FORMAT_NONE && yFormatType == __FORMAT_NONE)
			{
				// No input format defined -- simple data/percent drawing.
				drawAnnotationRectangle(__xs, __ys, xAxisPercent, yAxisPercent);
			}
			else
			{
				// Date: 
				drawAnnotationRectangle(__xs, __ys, formatXPts, formatYPts, xFormat, yFormat, xFormatType, yFormatType, xAxisPercent, yAxisPercent);
			}
		}
		else if (shapeType.Equals("Symbol", StringComparison.OrdinalIgnoreCase))
		{
			propVal = p.getValue("SymbolSize");
			int size = 0;
			if (string.ReferenceEquals(propVal, null))
			{
				// default
			}
			else
			{
				try
				{
					size = Integer.decode(propVal).intValue();
				}
				catch (Exception)
				{
					Message.printWarning(3, "drawAnnotation", "Invalid symbol size: '" + propVal + "'");
					return;
				}
			}

			if (size == 0)
			{
				// short-circuit, since nothing visible will be drawn
				return;
			}

			propVal = p.getValue("SymbolPosition");
			int symbolPosition = getAnnotationPosition(propVal);
			if (symbolPosition == -1)
			{
				Message.printWarning(2, "drawAnnotation", "Invalid position value: " + propVal);
				return;
			}

			propVal = p.getValue("DrawOutOfBounds");
			bool off = false;
			if (!string.ReferenceEquals(propVal, null) && propVal.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				off = true;
			}

			int symbol = 0;
			propVal = p.getValue("SymbolStyle");
			if (string.ReferenceEquals(propVal, null))
			{
				propVal = "None";
			}
			try
			{
				symbol = GRSymbol.toInteger(propVal);
			}
			catch (Exception)
			{
				Message.printWarning(2, "drawAnnotation", "Invalid symbol style: " + propVal);
				return;
			}

			if (xFormatType == __FORMAT_NONE && yFormatType == __FORMAT_NONE)
			{
				// no InputFormats on either axis
				drawAnnotationSymbol(__xs, __ys, symbol, size, GRUnits.DEVICE, outlineColor, symbolPosition, xAxisPercent, yAxisPercent, off);
			}
			else
			{
				drawAnnotationSymbol(__xs, __ys, formatXPts, formatYPts, xFormat, yFormat, xFormatType, yFormatType, symbol, size, GRUnits.DEVICE, outlineColor, symbolPosition, xAxisPercent, yAxisPercent, off);
			}
		}
		else if (shapeType.Equals("Text", StringComparison.OrdinalIgnoreCase))
		{
			propVal = p.getValue("FontSize");
			int fontSize = 10;
			if (string.ReferenceEquals(propVal, null))
			{
				// default
			}
			else
			{
				fontSize = StringUtil.atoi(propVal);
				if (fontSize == -1)
				{
					Message.printWarning(3, "drawAnnotation", "Invalid font size: " + propVal);
					return;
				}
			}

			if (fontSize == 0)
			{
				// short-circuit, since nothing visible will be drawn
				return;
			}

			string fontStyle = p.getValue("FontStyle");
			if (string.ReferenceEquals(fontStyle, null))
			{
				fontStyle = "Plain";
			}

			string fontName = p.getValue("FontName");
			if (string.ReferenceEquals(fontName, null))
			{
				Message.printWarning(3, "drawAnnotation", "Invalid font fontName: " + fontName);
				return;
			}

			setFont(fontName, fontStyle, (double)fontSize);

			string text = p.getValue("Text");
			if (string.ReferenceEquals(text, null))
			{
				// Nothing to draw -- short-circuit here.
				return;
			}

			propVal = p.getValue("TextPosition");
			int textPosition = getAnnotationPosition(propVal);
			if (textPosition == -1)
			{
				Message.printWarning(3, "drawAnnotation", "Invalid position value: " + propVal);
				return;
			}

			propVal = p.getValue("DrawOutOfBounds");
			bool off = false;
			if (!string.ReferenceEquals(propVal, null) && propVal.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				off = true;
			}

			if (xFormatType == __FORMAT_NONE && yFormatType == __FORMAT_NONE)
			{
				// No Input Formats defined.
				drawAnnotationText(__xs, __ys, text, textPosition, xAxisPercent, yAxisPercent, off);
			}
			else
			{
				drawAnnotationText(__xs, __ys, formatXPts, formatYPts, xFormat, yFormat, xFormatType, yFormatType, text, textPosition, xAxisPercent, yAxisPercent, off);
			}

		}
	}

	/// <summary>
	/// Annotation drawing helper code that handles Input Formats for line 
	/// annotations.  This code will translate the X and/or Y values appropriately and draw the line. </summary>
	/// <param name="xs"> the array of x values for this annotation.  If an XFormat is
	/// defined, this parameter is not used -- it will still be non-null, though. </param>
	/// <param name="ys"> the array of y values for this annotation.  If an YFormat is
	/// defined, this parameter is not used -- it will still be non-null, though. </param>
	/// <param name="formatXs"> the array of X format input values.  Null if unused. </param>
	/// <param name="formatYs"> the array of Y format input values.  Null if unused. </param>
	/// <param name="xFormat"> the format for the X axis.  Null if unused. </param>
	/// <param name="yFormat"> the format for the Y axis.  Null if unused. </param>
	/// <param name="xFormatType"> the kind of X axis format.  Can be null, in which case 
	/// there is no XFormat.  Currently only support "DateTime," format. </param>
	/// <param name="yFormatType"> the kind of Y axis format.  Can be null, in which case
	/// there is no YFormat.  Currently there are no supported YFormats. </param>
	/// <param name="lineWidth"> the width of the line </param>
	private void drawAnnotationLine(double[] xs, double[] ys, string[] formatXs, string[] formatYs, string xFormat, string yFormat, int xFormatType, int yFormatType, int lineWidth, string lineStyle, bool xAxisPercent, bool yAxisPercent)
	{
		if (!string.ReferenceEquals(xFormat, null))
		{
			int index = xFormat.IndexOf(",", StringComparison.Ordinal);
			xFormat = xFormat.Substring(index + 1);
		}
		if (!string.ReferenceEquals(yFormat, null))
		{
			int index = yFormat.IndexOf(",", StringComparison.Ordinal);
			yFormat = yFormat.Substring(index + 1);
		}

		double x1mod = determineModifier(true, xFormatType, xFormat, formatXs);
		double y1mod = determineModifier(false, yFormatType, yFormat, formatYs);

		double[] calcX1s = calculateFormatPoints(true, xFormatType, xFormat, xs, formatXs, 0, 2, -1);
		double[] calcX2s = calculateFormatPoints(true, xFormatType, xFormat, xs, formatXs, 1, 2, calcX1s.Length);
		double[] calcY1s = calculateFormatPoints(false, yFormatType, yFormat, ys, formatYs, 0, 2, -1);
		double[] calcY2s = calculateFormatPoints(false, yFormatType, yFormat, ys, formatYs, 1, 2, calcY1s.Length);

		xAxisPercent = determinePercent(xAxisPercent, xFormatType);
		yAxisPercent = determinePercent(yAxisPercent, yFormatType);

		for (int i = 0; i < calcX1s.Length; i++)
		{
			for (int j = 0; j < calcY1s.Length; j++)
			{
				__xs[0] = calcX1s[i] + x1mod;
				__xs[1] = calcX2s[i];
				__ys[0] = calcY1s[j] + y1mod;
				__ys[1] = calcY2s[j];
				drawAnnotationLine(__xs, __ys, lineWidth, lineStyle, xAxisPercent, yAxisPercent);
			}
		}
	}

	/// <summary>
	/// Annotation line drawing helper code. </summary>
	/// <param name="xs"> the x points of the line. </param>
	/// <param name="ys"> the y points of the line. </param>
	/// <param name="lineWidth"> the width of the line. </param>
	/// <param name="lineStyle"> the style of the line. </param>
	/// <param name="xAxisPercent"> whether the X axis is percent (true) or data (false). </param>
	/// <param name="yAxisPercent"> whether the Y axis is percent (true) or data (false). </param>
	private void drawAnnotationLine(double[] xs, double[] ys, int lineWidth, string lineStyle, bool xAxisPercent, bool yAxisPercent)
	{
		if (lineWidth != 1)
		{
			setLineWidth((double)lineWidth);
		}

		if (xAxisPercent)
		{
			__tempXs[0] = ((_datax2 - _datax1) * (xs[0] / 100.0)) + _datax1;
			__tempXs[1] = ((_datax2 - _datax1) * (xs[1] / 100.0)) + _datax1;
		}
		else
		{
			__tempXs[0] = xs[0];
			__tempXs[1] = xs[1];
		}

		if (yAxisPercent)
		{
			__tempYs[0] = ((_datay2 - _datay1) * (ys[0] / 100.0)) + _datay1;
			__tempYs[1] = ((_datay2 - _datay1) * (ys[1] / 100.0)) + _datay1;
		}
		else
		{
			__tempYs[0] = ys[0];
			__tempYs[1] = ys[1];
		}

		__tempXs[0] = scaleXData(__tempXs[0]);
		__tempXs[1] = scaleXData(__tempXs[1]);
		__tempYs[0] = scaleYData(__tempYs[0]);
		__tempYs[1] = scaleYData(__tempYs[1]);
		drawLine(__tempXs, __tempYs);

		if (lineWidth != 1)
		{
			setLineWidth(1.0);
		}
	}

	/// <summary>
	/// Annotation drawing helper code that handles input formats for rectangle annotations.
	/// This code will translate the X and/or Y values appropriately and draw the line. </summary>
	/// <param name="xs"> the array of x values for this annotation.  If an XFormat is
	/// defined, this parameter is not used -- it will still be non-null, though. </param>
	/// <param name="ys"> the array of y values for this annotation.  If an YFormat is
	/// defined, this parameter is not used -- it will still be non-null, though. </param>
	/// <param name="formatXs"> the array of X format input values.  Null if unused. </param>
	/// <param name="formatYs"> the array of Y format input values.  Null if unused. </param>
	/// <param name="xFormat"> the format for the X axis.  Null if unused. </param>
	/// <param name="yFormat"> the format for the Y axis.  Null if unused. </param>
	/// <param name="xFormatType"> the kind of X axis format.  Can be null, in which case 
	/// there is no XFormat.  Currently only support "DateTime," format. </param>
	/// <param name="yFormatType"> the kind of Y axis format.  Can be null, in which case
	/// there is no YFormat.  Currently there are no supported YFormats. </param>
	private void drawAnnotationRectangle(double[] xs, double[] ys, string[] formatXs, string[] formatYs, string xFormat, string yFormat, int xFormatType, int yFormatType, bool xAxisPercent, bool yAxisPercent)
	{
		if (!string.ReferenceEquals(xFormat, null))
		{
			int index = xFormat.IndexOf(",", StringComparison.Ordinal);
			xFormat = xFormat.Substring(index + 1);
		}
		if (!string.ReferenceEquals(yFormat, null))
		{
			int index = yFormat.IndexOf(",", StringComparison.Ordinal);
			yFormat = yFormat.Substring(index + 1);
		}

		double x1mod = determineModifier(true, xFormatType, xFormat, formatXs);
		double y1mod = determineModifier(false, yFormatType, yFormat, formatYs);

		double[] calcX1s = calculateFormatPoints(true, xFormatType, xFormat, xs, formatXs, 0, 2, -1);
		double[] calcX2s = calculateFormatPoints(true, xFormatType, xFormat, xs, formatXs, 1, 2, calcX1s.Length);
		double[] calcY1s = calculateFormatPoints(false, yFormatType, yFormat, ys, formatYs, 0, 2, -1);
		double[] calcY2s = calculateFormatPoints(false, yFormatType, yFormat, ys, formatYs, 1, 2, calcY1s.Length);

		xAxisPercent = determinePercent(xAxisPercent, xFormatType);
		yAxisPercent = determinePercent(yAxisPercent, yFormatType);

		for (int i = 0; i < calcX1s.Length; i++)
		{
			for (int j = 0; j < calcY1s.Length; j++)
			{
				__xs[0] = calcX1s[i] + x1mod;
				__xs[1] = calcX2s[i];
				__ys[0] = calcY1s[j] + y1mod;
				__ys[1] = calcY2s[j];
				drawAnnotationRectangle(__xs, __ys, xAxisPercent, yAxisPercent);
			}
		}
	}

	/// <summary>
	/// Annotation rectangle drawing helper code. </summary>
	/// <param name="xs"> the x points of the line. </param>
	/// <param name="ys"> the y points of the line. </param>
	/// <param name="xAxisPercent"> whether the X axis is percent (true) or data (false). </param>
	/// <param name="yAxisPercent"> whether the Y axis is percent (true) or data (false). </param>
	private void drawAnnotationRectangle(double[] xs, double[] ys, bool xAxisPercent, bool yAxisPercent)
	{
		if (xAxisPercent)
		{
			__tempXs[0] = ((_datax2 - _datax1) * (xs[0] / 100.0)) + _datax1;
			__tempXs[1] = ((_datax2 - _datax1) * (xs[1] / 100.0)) + _datax1;
		}
		else
		{
			__tempXs[0] = xs[0];
			__tempXs[1] = xs[1];
		}

		if (yAxisPercent)
		{
			__tempYs[0] = ((_datay2 - _datay1) * (ys[0] / 100.0)) + _datay1;
			__tempYs[1] = ((_datay2 - _datay1) * (ys[1] / 100.0)) + _datay1;
		}
		else
		{
			__tempYs[0] = ys[0];
			__tempYs[1] = ys[1];
		}

		__tempXs[0] = scaleXData(__tempXs[0]);
		__tempXs[1] = scaleXData(__tempXs[1]);
		__tempYs[0] = scaleYData(__tempYs[0]);
		__tempYs[1] = scaleYData(__tempYs[1]);
		double xll = __tempXs[0];
		if (__tempXs[1] < xll)
		{
			xll = __tempXs[1];
		}
		double yll = __tempYs[0];
		if (__tempYs[1] < yll)
		{
			yll = __tempYs[1];
		}
		double w = __tempXs[0] - __tempXs[1];
		if (w < 0)
		{
			w = w * -1.0;
		}
		double h = __tempYs[0] - __tempYs[1];
		if (h < 0)
		{
			h = h * -1.0;
		}
		// Currently annotations always fill but in the future will implement outline
		fillRectangle(xll, yll, w, h);
	}

	/// <summary>
	/// Annotation drawing helper code that handles Input Formats for symbol 
	/// annotations.  This code will translate the X and/or Y values appropriately and draw the symbol. </summary>
	/// <param name="xs"> the array of x values for this annotation.  If an XFormat is
	/// defined, this parameter is not used -- it will still be non-null, though. </param>
	/// <param name="ys"> the array of y values for this annotation.  If an YFormat is
	/// defined, this parameter is not used -- it will still be non-null, though. </param>
	/// <param name="formatXs"> the array of X format input values.  Null if unused. </param>
	/// <param name="formatYs"> the array of Y format input values.  Null if unused. </param>
	/// <param name="xFormat"> the format for the X axis.  Null if unused. </param>
	/// <param name="yFormat"> the format for the Y axis.  Null if unused. </param>
	/// <param name="xFormatType"> the kind of X axis format.  Can be null, in which case 
	/// there is no XFormat.  Currently only support "DateTime," format. </param>
	/// <param name="yFormatType"> the kind of Y axis format.  Can be null, in which case
	/// there is no YFormat.  Currently there are no supported YFormats. </param>
	/// <param name="symbol"> the symbol type to draw </param>
	/// <param name="size"> the size of the symbol </param>
	/// <param name="units"> the units in which the symbol will be drawn </param>
	/// <param name="pos"> the position of the symbol </param>
	/// <param name="off"> whether the symbol should still attempt to be drawn even if its 
	/// point lies outside the bounds of the drawing area.  If false, then no attempt
	/// to drawn the symbol will be made.  If true, the symbol will be drawn and
	/// there is a chance that part of the drawn area of the symbol will overlap 
	/// the drawing area and be visible. </param>
	private void drawAnnotationSymbol(double[] xs, double[] ys, string[] formatXs, string[] formatYs, string xFormat, string yFormat, int xFormatType, int yFormatType, int symbol, int size, int units, GRColor outlineColor, int pos, bool xAxisPercent, bool yAxisPercent, bool off)
	{
		if (!string.ReferenceEquals(xFormat, null))
		{
			int index = xFormat.IndexOf(",", StringComparison.Ordinal);
			xFormat = xFormat.Substring(index + 1);
		}
		if (!string.ReferenceEquals(yFormat, null))
		{
			int index = yFormat.IndexOf(",", StringComparison.Ordinal);
			yFormat = yFormat.Substring(index + 1);
		}

		double[] calcXs = calculateFormatPoints(true, xFormatType, xFormat, xs, formatXs, 0, 1, -1);
		double[] calcYs = calculateFormatPoints(false, yFormatType, yFormat, ys, formatYs, 0, 1, -1);

		xAxisPercent = determinePercent(xAxisPercent, xFormatType);
		yAxisPercent = determinePercent(yAxisPercent, yFormatType);

		for (int i = 0; i < calcXs.Length; i++)
		{
			for (int j = 0; j < calcYs.Length; j++)
			{
				__xs[0] = calcXs[i];
				__ys[0] = calcYs[j];

				drawAnnotationSymbol(__xs, __ys, symbol, size, units, outlineColor, pos, xAxisPercent, yAxisPercent, off);
			}
		}
	}

	/// <summary>
	/// Annotation symbol drawing helper code. </summary>
	/// <param name="xs"> the x points of the symbol. </param>
	/// <param name="ys"> the y points of the symbol. </param>
	/// <param name="symbol"> the symbol to draw </param>
	/// <param name="size"> the size of the symbol </param>
	/// <param name="units"> the units in which to draw the symbol </param>
	/// <param name="pos"> the position of the symbol </param>
	/// <param name="xAxisPercent"> whether the X axis is percent (true) or data (false). </param>
	/// <param name="yAxisPercent"> whether the Y axis is percent (true) or data (false). </param>
	/// <param name="off"> whether the symbol should still attempt to be drawn even if its 
	/// point lies outside the bounds of the drawing area.  If false, then no attempt
	/// to drawn the symbol will be made.  If true, the symbol will be drawn and
	/// there is a chance that part of the drawn area of the symbol will overlap 
	/// the drawing area and be visible. </param>
	private void drawAnnotationSymbol(double[] xs, double[] ys, int symbol, int size, int units, GRColor outlineColor, int pos, bool xAxisPercent, bool yAxisPercent, bool off)
	{
		double tempX = 0;
		double tempY = 0;
		if (xAxisPercent)
		{
			tempX = ((_datax2 - _datax1) * (__xs[0] / 100.0)) + _datax1;
		}
		else
		{
			tempX = __xs[0];
		}

		if (yAxisPercent)
		{
			tempY = ((_datay2 - _datay1) * (__ys[0] / 100.0)) + _datay1;
		}
		else
		{
			tempY = __ys[0];
		}

		bool yInDa = false; // Is text Y in drawing area?
		if (_datay2 >= _datay1)
		{
			// Normal axis orientation
			if ((tempY >= _datay1) && (tempY <= _datay2))
			{
				yInDa = true;
			}
		}
		else
		{
			// Reversed axis orientation
			if ((tempY >= _datay2) && (tempY <= _datay1))
			{
				yInDa = true;
			}
		}

		if (((tempX >= _datax1) && (tempX <= _datax2) && yInDa) || off)
		{
				GRDrawingAreaUtil.drawSymbol(this, symbol, tempX, tempY, size, size, 0, 0, null, GRUnits.DEVICE, pos, null, outlineColor);
		}
		else
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(2, "drawAnnotation", "Symbol annotation outside drawing area.");
			}
		}

	}

	/// <summary>
	/// Annotation drawing helper code that handles Input Formats for text 
	/// annotations.  This code will translate the X and/or Y values appropriately and draw the text. </summary>
	/// <param name="xs"> the array of x values for this annotation.  If an XFormat is
	/// defined, this parameter is not used -- it will still be non-null, though. </param>
	/// <param name="ys"> the array of y values for this annotation.  If an YFormat is
	/// defined, this parameter is not used -- it will still be non-null, though. </param>
	/// <param name="formatXs"> the array of X format input values.  Null if unused. </param>
	/// <param name="formatYs"> the array of Y format input values.  Null if unused. </param>
	/// <param name="xFormat"> the format for the X axis.  Null if unused. </param>
	/// <param name="yFormat"> the format for the Y axis.  Null if unused. </param>
	/// <param name="xFormatType"> the kind of X axis format.  Can be null, in which case 
	/// there is no XFormat.  Currently only support "DateTime," format. </param>
	/// <param name="yFormatType"> the kind of Y axis format.  Can be null, in which case
	/// there is no YFormat.  Currently there are no supported YFormats. </param>
	/// <param name="text"> the text to draw </param>
	/// <param name="pos"> the position of the text </param>
	/// <param name="off"> whether the text should still attempt to be drawn even if its 
	/// point lies outside the bounds of the drawing area.  If false, then no attempt
	/// to drawn the text will be made.  If true, the text will be drawn and
	/// there is a chance that part of the drawn area of the text will overlap 
	/// the drawing area and be visible. </param>
	private void drawAnnotationText(double[] xs, double[] ys, string[] formatXs, string[] formatYs, string xFormat, string yFormat, int xFormatType, int yFormatType, string text, int pos, bool xAxisPercent, bool yAxisPercent, bool off)
	{
		if (!string.ReferenceEquals(xFormat, null))
		{
			int index = xFormat.IndexOf(",", StringComparison.Ordinal);
			xFormat = xFormat.Substring(index + 1);
		}
		if (!string.ReferenceEquals(yFormat, null))
		{
			int index = yFormat.IndexOf(",", StringComparison.Ordinal);
			yFormat = yFormat.Substring(index + 1);
		}

		double[] calcXs = calculateFormatPoints(true, xFormatType, xFormat, xs, formatXs, 0, 1, -1);
		double[] calcYs = calculateFormatPoints(false, yFormatType, yFormat, ys, formatYs, 0, 1, -1);

		xAxisPercent = determinePercent(xAxisPercent, xFormatType);
		yAxisPercent = determinePercent(yAxisPercent, yFormatType);

		for (int i = 0; i < calcXs.Length; i++)
		{
			for (int j = 0; j < calcYs.Length; j++)
			{
				__xs[0] = calcXs[i];
				__ys[0] = calcYs[j];
				drawAnnotationText(__xs, __ys, text, pos, xAxisPercent, yAxisPercent, off);
			}
		}
	}

	/// <summary>
	/// Annotation text drawing helper code.  It is possible that the drawing area data y-coordinates
	/// are reversed, but the position will still be intuitive to the user (upper right is still that way
	/// on the physical screen). </summary>
	/// <param name="xs"> the x points of the text. </param>
	/// <param name="ys"> the y points of the text. </param>
	/// <param name="text"> the text to draw. </param>
	/// <param name="pos"> the position of the text. </param>
	/// <param name="xAxisPercent"> whether the X axis is percent (true) or data (false). </param>
	/// <param name="yAxisPercent"> whether the Y axis is percent (true) or data (false). </param>
	/// <param name="off"> whether the text should still attempt to be drawn even if its 
	/// point lies outside the bounds of the drawing area.  If false, then no attempt
	/// to drawn the text will be made.  If true, the text will be drawn and
	/// there is a chance that part of the drawn area of the text will overlap 
	/// the drawing area and be visible. </param>
	private void drawAnnotationText(double[] xs, double[] ys, string text, int pos, bool xAxisPercent, bool yAxisPercent, bool off)
	{
		double tempX = 0;
		double tempY = 0;

		if (xAxisPercent)
		{
			tempX = ((_datax2 - _datax1) * (__xs[0] / 100.0)) + _datax1;
		}
		else
		{
			tempX = __xs[0];
		}

		if (yAxisPercent)
		{
			tempY = ((_datay2 - _datay1) * (__ys[0] / 100.0)) + _datay1;
		}
		else
		{
			tempY = __ys[0];
		}

		bool yInDa = false; // Is text Y in drawing area?
		if (_datay2 >= _datay1)
		{
			// Normal axis orientation
			if ((tempY >= _datay1) && (tempY <= _datay2))
			{
				yInDa = true;
			}
		}
		else
		{
			// Reversed axis orientation
			if ((tempY >= _datay2) && (tempY <= _datay1))
			{
				yInDa = true;
			}
		}

		if ((tempX >= _datax1 && tempX <= _datax2 && yInDa) || off)
		{
			GRDrawingAreaUtil.drawText(this, text, tempX, tempY, 0, pos);
		}
		else
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(2, "drawAnnotation", "Text annotation data coordinate (" + tempX + "," + tempY + ") outside drawing area data limits (" + _datax1 + "," + _datay1 + ") (" + _datax2 + "," + _datay2 + ")");
			}
		}
	}

	/// <summary>
	/// Draw an arc using the current color, line, etc. </summary>
	/// <param name="x"> X-coordinate of center. </param>
	/// <param name="rx"> X-radius. </param>
	/// <param name="y"> Y-coordinate of center. </param>
	/// <param name="ry"> Y-radius. </param>
	/// <param name="a1"> Initial angle to start drawing (0 is at 3 o'clock, then counter-clockwise). </param>
	/// <param name="a2"> Ending angle. </param>
	public override void drawArc(double x, double y, double rx, double ry, double a1, double a2)
	{
		// Java draws using the rectangle...
		_jdev._graphics.drawArc((int)(x - rx), (int)(y - ry), (int)(rx * 2.0), (int)(ry * 2.0), (int)a1, (int)a2);
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
	public virtual void drawCompoundText(IList<string> text, Color color, double x, double y, double angle, int flag)
	{
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
	/// Draws an oval in the current color. </summary>
	/// <param name="centerX"> the X position of the center of the oval </param>
	/// <param name="centerY"> the Y position of the center of the oval </param>
	/// <param name="xRadius"> the X radius of the oval </param>
	/// <param name="yRadius"> the Y radius of the oval </param>
	public virtual void drawOval(double centerX, double centerY, double xRadius, double yRadius)
	{
		_jdev._graphics.drawOval((int)(centerX - xRadius), (int)(centerY - yRadius), (int)(xRadius * 2), (int)(yRadius * 2));
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
	/// Draw text. </summary>
	/// <param name="text"> the text string to draw. </param>
	/// <param name="x"> the x location at which to draw the string. </param>
	/// <param name="y"> the y location at which to draw the string. </param>
	/// <param name="a"> the alpha value of the string. </param>
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
	/// <param name="a"> the alpha value (transparency) of the string (currently not used). </param>
	/// <param name="flag"> one of the GRText.* values specifying how to draw the string. </param>
	/// <param name="rotationDegrees"> number of degrees to rotates the text clock-wise from 
	/// due East.  Standard Java rotation transforms rotate clock-wise with positive numbers. </param>
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

		Font font = null;
		FontMetrics fm = null;

		font = _jdev._graphics.getFont();
		fm = _jdev._graphics.getFontMetrics();

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
		if ((rotationDegrees > -.001) && (rotationDegrees < .001))
		{
			// For close to zero, draw horizontally
			_jdev._graphics.drawString(text, ix, iy);
		}
		else
		{
			// Draw rotated text using a small image - otherwise transform might rotate too large an area
			int larger = 0;
			if (height > width)
			{
				larger = height;
			}
			else
			{
				larger = width;
			}
			if ((rotationDegrees % 90) < .001)
			{
				// See:  http://stackoverflow.com/questions/10083913/how-to-rotate-text-with-graphics2d-in-java
				// The following works when 90-degree multiples but need a generic way to deal with centering, justifying.
				// The problem is the drawing routine works from the lower-left corner of the text but the
				// positioning flag requires an offset.
				Graphics2D g2d = _jdev._graphics;
				double rotationRad = Math.toRadians(rotationDegrees);
				// Translate coordinate system to rotated space
				// (xs,ys) work as is if left-bottom edge is at point
				double xt = xs; // Translate values
				double yt = ys;
				if ((int)(rotationDegrees + .001) == 90)
				{
					// Drawing down with bottom of text to left
					if ((flag & GRText.CENTER_Y) != 0)
					{
						xt -= height / 2;
					}
					else if ((flag & GRText.TOP) != 0)
					{
						xt -= height;
					}
					if ((flag & GRText.CENTER_X) != 0)
					{
						yt -= width / 2;
					}
					else if ((flag & GRText.RIGHT) != 0)
					{
						yt -= width;
					}
				}
				else if (((int)(rotationDegrees + .01) == 270) || ((int)(rotationDegrees - .01) == -90))
				{
					// Drawing up with bottom of text to right
					if ((flag & GRText.CENTER_Y) != 0)
					{
						xt += height / 2;
					}
					else if ((flag & GRText.TOP) != 0)
					{
						xt += height;
					}
					if ((flag & GRText.CENTER_X) != 0)
					{
						yt += width / 2;
					}
					else if ((flag & GRText.RIGHT) != 0)
					{
						yt += width;
					}
				}
				g2d.translate(xt,yt);
				g2d.rotate(rotationRad);
				g2d.drawString(text, 0, 0);
				// Translate back to original coordinate system
				g2d.rotate(-rotationRad);
				g2d.translate(-xt,-yt);
			}
			else
			{
				// This works but does not seem to get the positioning quite right
				BufferedImage temp = new BufferedImage(larger * 2, larger * 2, BufferedImage.TYPE_4BYTE_ABGR);
				Graphics2D tempG = temp.createGraphics();
				tempG.setFont(font);

				AffineTransform origXform = tempG.getTransform();
				AffineTransform newXform = (AffineTransform)(origXform.clone());
				newXform.rotate(Math.toRadians(rotationDegrees), larger,larger);

				tempG.setTransform(newXform);
				tempG.setColor(_jdev._graphics.getColor());
				//Message.printStatus(2,"","Drawing text \"" + text + "\" into image of size " + larger );
				tempG.drawString(text, larger, larger);
				//Message.printStatus(2,"","Drawing image into device at " + ((int)x - larger) + "," + ((int)y - larger) );
				_jdev._graphics.drawImage(temp, ((int)x - larger), ((int)y - larger), null);
				tempG.dispose();
			}
		}
	}

	/// <summary>
	/// Draw an arc using the current color, line, etc. </summary>
	/// <param name="x"> X-coordinate of center. </param>
	/// <param name="rx"> X-radius. </param>
	/// <param name="y"> Y-coordinate of center. </param>
	/// <param name="ry"> Y-radius. </param>
	/// <param name="a1"> Initial angle to start drawing (0 is at 3 o'clock, then counterclockwise). </param>
	/// <param name="a2"> Ending angle. </param>
	public override void fillArc(double x, double y, double rx, double ry, double a1, double a2, int fillmode)
	{
		// Java draws using the rectangle...
		_jdev._graphics.fillArc((int)(x - rx), (int)(y - ry), (int)(rx * 2.0), (int)(ry * 2.0), (int)a1, (int)a2);
	}

	/// <summary>
	/// Fills an oval with the current color. </summary>
	/// <param name="centerX"> the X position of the center of the oval </param>
	/// <param name="centerY"> the Y position of the center of the oval </param>
	/// <param name="xRadius"> the X radius of the oval </param>
	/// <param name="yRadius"> the Y radius of the oval </param>
	public virtual void fillOval(double centerX, double centerY, double xRadius, double yRadius)
	{
		_jdev._graphics.fillOval((int)(centerX - xRadius), (int)(centerY - yRadius), (int)(xRadius * 2), (int)(yRadius * 2));
	}

	/// <summary>
	/// Fill a polygon. </summary>
	/// <param name="npts"> the number of points in the polygon. </param>
	/// <param name="x"> an array of x coordinates </param>
	/// <param name="y"> an array of y coordinates </param>
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
	/// Fills a polygon. </summary>
	/// <param name="npts"> the number of points in the polygon. </param>
	/// <param name="x"> an array of x coordinates </param>
	/// <param name="y"> an array of y coordinates </param>
	/// <param name="transparency"> the amount of transparency in the polygon from 0 (totally
	/// opaque) to 255 (totally transparent). </param>
	public override void fillPolygon(int npts, double[] x, double[] y, int transparency)
	{
		if (npts < 1)
		{
			return;
		}

		// RTi does 0 as Opaque.  Everyone else (including Sun) does 0 as
		// transparent and 255 as opaque.
		double alpha = 255 - transparency;

		if (alpha > 255)
		{
			alpha = 255;
		}
		else if (alpha < 0)
		{
			alpha = 0;
		}

		if (alpha == 255)
		{
			// Totally opaque...
			fillPolygon(npts, x, y);
			return;
		}

		alpha = alpha / 255.0;

		float floatAlpha = (float)alpha;

		int[] ix = new int[npts];
		int[] iy = new int[npts];
		for (int i = 0; i < npts; i++)
		{
			ix[i] = (int)x[i];
			iy[i] = (int)y[i];
		}

		_jdev._graphics.setComposite(makeComposite(floatAlpha));

		_jdev._graphics.fillPolygon(ix, iy, npts);
	}

	/// <summary>
	/// Fill a rectangle in the current color. </summary>
	/// <param name="limits"> limits defining the rectangle. </param>
	public override void fillRectangle(GRLimits limits)
	{
		if (limits == null)
		{
			return;
		}

		if (Message.isDebugOn)
		{
			Message.printDebug(10, "GRJComponentDrawingArea.fillRectangle(GRLimits)", "Filling rectangle for limits " + limits.ToString());
		}
		fillRectangle(limits.getLeftX(), limits.getBottomY(), limits.getWidth(), limits.getHeight());
	}

	/// <summary>
	/// Fill a rectangle in the current color. </summary>
	/// <param name="xll"> the lower-left x coordinate </param>
	/// <param name="yll"> the lower-right y coordinate </param>
	/// <param name="width"> the width of the rectangle </param>
	/// <param name="height"> the height of the rectangle </param>
	public override void fillRectangle(double xll, double yll, double width, double height)
	{
		if (Message.isDebugOn)
		{
			Message.printDebug(10, "GRJComponentDrawingArea.fillRectangle(x,y,w,h)", "Filling rectangle at " + xll + "," + yll + " w=" + width + " w=" + height);
		}
		_jdev._graphics.fillRect((int)xll, (int)yll, (int)width, (int)height);
	}

	/// <summary>
	/// Finalize before garbage collection. </summary>
	/// <exception cref="Throwable"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~GRJComponentDrawingArea()
	{
		_fontObj = null;
		_jdev = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Flush graphics (does nothing).  Does not seem to be necessary with Java.
	/// </summary>
	public override void flush()
	{
	}

	/// <summary>
	/// Given a position for an annotation, returns the integer value that should be
	/// passed to drawing routines. </summary>
	/// <param name="position"> the position of the annotation (eg, "UpperRight"). </param>
	/// <returns> the integer value of the position, or -1 if the position is invalid. </returns>
	private int getAnnotationPosition(string position)
	{
		int pos = -1;
		if (string.ReferenceEquals(position, null))
		{
			// default to center
			pos = GRText.CENTER_X | GRText.CENTER_Y;
		}
		else if (position.Equals("UpperRight", StringComparison.OrdinalIgnoreCase))
		{
			pos = GRText.LEFT | GRText.BOTTOM;
		}
		else if (position.Equals("Right", StringComparison.OrdinalIgnoreCase))
		{
			pos = GRText.LEFT | GRText.CENTER_Y;
		}
		else if (position.Equals("LowerRight", StringComparison.OrdinalIgnoreCase))
		{
			pos = GRText.LEFT | GRText.TOP;
		}
		else if (position.Equals("Below", StringComparison.OrdinalIgnoreCase) || position.Equals("BelowCenter", StringComparison.OrdinalIgnoreCase))
		{
			pos = GRText.TOP | GRText.CENTER_X;
		}
		else if (position.Equals("LowerLeft", StringComparison.OrdinalIgnoreCase))
		{
			pos = GRText.RIGHT | GRText.TOP;
		}
		else if (position.Equals("Left", StringComparison.OrdinalIgnoreCase))
		{
			pos = GRText.RIGHT | GRText.CENTER_Y;
		}
		else if (position.Equals("UpperLeft", StringComparison.OrdinalIgnoreCase))
		{
			pos = GRText.RIGHT | GRText.BOTTOM;
		}
		else if (position.Equals("Above", StringComparison.OrdinalIgnoreCase) || position.Equals("AboveCenter", StringComparison.OrdinalIgnoreCase))
		{
			pos = GRText.BOTTOM | GRText.CENTER_X;
		}
		else if (position.Equals("Center", StringComparison.OrdinalIgnoreCase))
		{
			pos = GRText.CENTER_X | GRText.CENTER_Y;
		}

		return pos;
	}

	/// <summary>
	/// Returns the clipping shape on the current graphics context. </summary>
	/// <returns> the clipping shape on the current graphics context. </returns>
	public override Shape getClip()
	{
		return _jdev._graphics.getClip();
	}

	/// <summary>
	/// Returns the data extents given a delta in DA units.
	/// This routine takes as input delta-x and delta-y values and calculates the
	/// corresponding data extents.  This is useful when it is known (guessed?) that
	/// output needs to be, say, 15 points high but it is not known what the
	/// corresponding data values are.  This can be used, for example, to draw a box
	/// around text (better to allow PostScript o figure out the box size but that is a project for another day).
	/// TODO (JTS - 2003-05-05) Should that be implemented? SAM: sure, later
	/// The flags need to be implemented to allow the extents to be determined 
	/// exactly at the limits given, at the centroid of the drawing area, etc.  
	/// For now, calculate at the centroid so that projection issues do not cause problems. </summary>
	/// <param name="limits"> the limits for the drawing area. </param>
	/// <param name="flag"> indicates whether units should be returned in device or data units.
	/// REVISIT (JTS - 2003-05-05) This parameter isn't even used. </param>
	/// <returns> the data extents given a delta in DA units. </returns>
	public override GRLimits getDataExtents(GRLimits limits, int flag)
	{
		string routine = "GRJComponentDrawingArea.getDataExtents";

		// For now, default to getting the limits at the center of the drawing area...

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
	/// <param name="flag"> indicates whether coordinates are originating from the device or internally. </param>
	/// <returns> the data units given device units. </returns>
	public override GRPoint getDataXY(double devx, double devy, int flag)
	{
		string routine = "GRJComponentDrawingArea.getDataXY";

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

		// Now convert back to the data scale...
		if (_axisy == GRAxis.LOG)
		{
			y = Math.Pow(10.0, y);
		}
		else if (_axisy == GRAxis.STANDARD_NORMAL_PROBABILITY)
		{
			Message.printWarning(3, routine, "probability axis is not implemented");
			//y = GRFuncStdNormCum ( *y );
		}
		return new GRPoint(x, y);
	}

	/// <summary>
	/// Returns the font currently being used. </summary>
	/// <returns> the font currently being used. </returns>
	public virtual Font getFont()
	{
		return _jdev._graphics.getFont();
	}

	/// <returns> size of string. </returns>
	/// <param name="text"> String to evaluate. </param>
	/// <param name="flag"> GRUnits.DATA or GRUnits.DEV, indicating units for returned size.
	/// Currently, device units are always returned and the GR.getTextExtents call scales to the device. </param>
	public override GRLimits getTextExtents(string text, int flag)
	{
		// The font size is not scalable and is in device units.  Use the
		// Java API to get the size information...
		if (_jdev == null)
		{
			Message.printWarning(3, "GRJComponentDrawingArea.getTextExtents", "NULL _jdev");
			return null;
		}

		if (_jdev._graphics == null)
		{
			Message.printWarning(3, "GRJComponentDrawingArea.getTextExtents", "NULL " + _jdev._graphics);
			return null;
		}
		FontMetrics fm = _jdev._graphics.getFontMetrics();
		if (fm == null)
		{
			Message.printWarning(3, "GRJComponentDrawingArea.getTextExtents", "NULL FontMetrics");
			return null;
		}
		int width = fm.stringWidth(text);
		int height = fm.getAscent();

		return new GRLimits((double)width, (double)height);
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
	public override double getXData(double xdev)
	{
		Message.printWarning(3, "GRJComponentDrawingArea", "not implemented");
		return 0.0;
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public override double getYData(double ydev)
	{
		Message.printWarning(3, "GRJComponentDrawingArea", "not implemented");
		return 0.0;
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public virtual void grid(int nxg, double[] xg, int nyg, double[] yg, int flag)
	{
		Message.printWarning(3, "GRJComponentDrawingArea", "not implemented");
	}

	/// <summary>
	/// Initialize the Java drawing area data.  Rely on the base class for the most
	/// part when it calls its initialize routine from its constructor. </summary>
	/// <param name="dev"> GRJComponentDevice associated with this GRJComponentArea </param>
	private void initialize(GRJComponentDevice dev)
	{
		string routine = "GRJComponentDrawingArea.initialize";
		int dl = 10;

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Initializing");
		}

		_fontObj = null;
		_jdev = dev;
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
	/// Used to make transparent polygons. </summary>
	/// <param name="alpha"> the transparency level. </param>
	/// <returns> an AlphaComposite to use for transparency. </returns>
	private AlphaComposite makeComposite(float alpha)
	{
		int type = AlphaComposite.SRC_OVER;
		return (AlphaComposite.getInstance(type, alpha));
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
		// Save coordinates...
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
		Message.printWarning(3, "GRJComponentDrawingArea", "not implemented");
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public override void pageStart()
	{
		Message.printWarning(3, "GRJComponentDrawingArea", "not implemented");
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public virtual void print()
	{
	}

	/// <summary>
	/// Scale x data value to device plotting coordinate. </summary>
	/// <param name="xdata"> x data value to scale </param>
	public override double scaleXData(double xdata)
	{
		if (__scaleXData)
		{
			return base.scaleXData(xdata);
	//		return MathUtil.interpolate(xdata, _datax1, _datax2, _plotx1, _plotx2);
		}
		else
		{
			return xdata;
		}
	}

	/// <summary>
	/// Scale y data vlaue to device plotting coordinate. </summary>
	/// <param name="ydata"> y data value to scale. </param>
	public override double scaleYData(double ydata)
	{
		if (__scaleYData)
		{
			return (_jdev._devy2 - base.scaleYData(ydata));
	//		return(_jdev._devy2 - MathUtil.interpolate( ydata, _datay1, _datay2, _ploty1, _ploty2));
		}
		else
		{
			return ydata;
		}
	}

	/// <summary>
	/// Sets the clip (in data units) for the area outside of which nothing should
	/// be drawn.  If the dataUnits are null, any clip will be removed. </summary>
	/// <param name="dataLimits"> the limits of the area outside of which nothing will be drawn. </param>
	public override void setClip(GRLimits dataLimits)
	{
		if (dataLimits == null)
		{
			_jdev._graphics.setClip(null);
		}
		else
		{
			int lx = (int)scaleXData(dataLimits.getLeftX());
			int rx = (int)scaleXData(dataLimits.getRightX());
			int by = (int)scaleYData(dataLimits.getBottomY());
			int ty = (int)scaleYData(dataLimits.getTopY());

			Rectangle r = new Rectangle(lx, ty, rx - lx, by - ty);

			_jdev._graphics.setClip(r);
		}
	}

	/// <summary>
	/// Sets the clip shape for the area outside of which nothing should
	/// be drawn.  If the shape is null, any clip will be removed. </summary>
	/// <param name="clipShape"> the shape to clip to. </param>
	public override void setClip(Shape clipShape)
	{
		_jdev._graphics.setClip(clipShape);
	}

	/// <summary>
	/// Set the active color using a color. </summary>
	/// <param name="color"> Color to use. </param>
	public override void setColor(GRColor color)
	{
		_color = color;
		_jdev._graphics.setColor((Color)color);
	}

	/// <summary>
	/// Set the active color using the RGB components. </summary>
	/// <param name="r"> Red component, 0.0 to 1.0. </param>
	/// <param name="g"> Green component, 0.0 to 1.0. </param>
	/// <param name="b"> Blue component, 0.0 to 1.0. </param>
	public override void setColor(float r, float g, float b)
	{
		_color = new GRColor(r, g, b);
		Graphics2D gr = _jdev._graphics;
		gr.setColor(_color);
	}

	/// <summary>
	/// Set the active color using the RGB components. </summary>
	/// <param name="r"> Red component, 0.0 to 1.0. </param>
	/// <param name="g"> Green component, 0.0 to 1.0. </param>
	/// <param name="b"> Blue component, 0.0 to 1.0. </param>
	public override void setColor(double R, double G, double B)
	{
		_color = new GRColor((float)R, (float)G, (float)B);
		Graphics2D gr = _jdev._graphics;
		gr.setColor(_color);
	}

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
		else if (style.Equals("Bold", StringComparison.OrdinalIgnoreCase))
		{
			setFont(font, Font.BOLD, (int)height);
		}
		else if (style.Equals("Italic", StringComparison.OrdinalIgnoreCase))
		{
			setFont(font, Font.ITALIC, (int)height);
		}
		else if (style.Equals("PlainItalic", StringComparison.OrdinalIgnoreCase))
		{
			setFont(font, Font.ITALIC, (int)height);
		}
		else if (style.Equals("PlainBold", StringComparison.OrdinalIgnoreCase))
		{
			setFont(font, Font.BOLD, (int)height);
		}
		else if (style.Equals("BoldItalic", StringComparison.OrdinalIgnoreCase))
		{
			setFont(font, Font.BOLD | Font.ITALIC, (int)height);
		}
	}

	/// <summary>
	/// Set the font using a Java style call. </summary>
	/// <param name="name"> the name of the font. </param>
	/// <param name="style"> the style of the font. </param>
	/// <param name="size"> the size of the font. </param>
	public virtual void setFont(string name, int style, int size)
	{
		_fontObj = new Font(name, style, size);
		if (_jdev == null)
		{
			return;
		}
		if (_jdev._graphics == null)
		{
			return;
		}
		_jdev._graphics.setFont(_fontObj);
	}

	/// <summary>
	/// Set the line dash for line-drawing commands for GRJComponentDrawingAreas. </summary>
	/// <param name="dash"> a float array specifying the dash pattern.  If null, line dashes will be turned off. </param>
	/// <param name="offset"> the initial dash offset. </param>
	public override void setLineDash(double[] Dash, double Offset)
	{
		if (Dash != null)
		{
			float[] dash = new float[Dash.Length];
			for (int i = 0; i < Dash.Length; i++)
			{
				dash[i] = (float)Dash[i];
			}
			float offset = (float)Offset;
			setFloatLineDash(dash, offset);
		}
		else
		{
			setFloatLineDash((float[])null, (float)Offset);
		}
	}

	/// <summary>
	/// Set the line dash for line-drawing commands for GRJComponentDrawingAreas. </summary>
	/// <param name="dash"> a float array specifying the dash pattern.  If null, line dashes will be turned off. </param>
	/// <param name="offset"> the initial dash offset. </param>
	public virtual void setFloatLineDash(float[] dash, float offset)
	{
		Stroke s = _jdev._graphics.getStroke();
		if (dash != null)
		{
			for (int i = 0; i < dash.Length; i++)
			{
				if (dash[i] == 0)
				{
					dash[i] = 1;
				}
			}
		}
		BasicStroke newStroke = null;
		if (s is BasicStroke)
		{
			BasicStroke b = (BasicStroke)s;
			newStroke = new BasicStroke(b.getLineWidth(), b.getEndCap(), b.getLineJoin(), b.getMiterLimit(), dash, offset);
		}
		else
		{
			BasicStroke b = new BasicStroke();
			newStroke = new BasicStroke(b.getLineWidth(), b.getEndCap(), b.getLineJoin(), b.getMiterLimit(), dash, offset);
		}
		_jdev._graphics.setStroke(newStroke);
	}



	/// <summary>
	/// Sets how lines are joined together. </summary>
	/// <param name="join"> the line join style (one of java.awt.BasicStroke.JOIN_BEVEL,
	/// BasicStroke.JOIN_MITER, or BasicStroke.JOIN_ROUND). </param>
	public virtual void setLineJoin(int join)
	{
		Stroke s = _jdev._graphics.getStroke();
		BasicStroke newStroke = null;
		if (s is BasicStroke)
		{
			BasicStroke b = (BasicStroke)s;
			newStroke = new BasicStroke(b.getLineWidth(), b.getEndCap(), join, b.getMiterLimit(), b.getDashArray(), b.getDashPhase());
		}
		else
		{
			BasicStroke b = new BasicStroke();
			newStroke = new BasicStroke(b.getLineWidth(), b.getEndCap(), join, b.getMiterLimit(), b.getDashArray(), b.getDashPhase());
		}
		_jdev._graphics.setStroke(newStroke);
	}

	/// <summary>
	/// Sets the line width. </summary>
	/// <param name="lineWidth"> the width that lines should be drawn at. </param>
	public override void setLineWidth(double lineWidth)
	{
		Stroke s = _jdev._graphics.getStroke();
		BasicStroke newStroke = null;
		if (s is BasicStroke)
		{
			BasicStroke b = (BasicStroke)s;
			newStroke = new BasicStroke((float)lineWidth, b.getEndCap(), b.getLineJoin(), b.getMiterLimit(), b.getDashArray(), b.getDashPhase());
		}
		else
		{
			BasicStroke b = new BasicStroke();
			newStroke = new BasicStroke((float)lineWidth, b.getEndCap(), b.getLineJoin(), b.getMiterLimit(), b.getDashArray(), b.getDashPhase());
		}
		_jdev._graphics.setStroke(newStroke);
	}

	/// <summary>
	/// Set the line cap to be used for GRJComponentDrawingAreas. </summary>
	/// <param name="da"> the drawing area on which to set the line join style. </param>
	/// <param name="cap"> the cap style to use (one of java.awt.BasicStroke.CAP_BUTT,
	/// BasicStroke.CAP_ROUND, BasicStroke.CAP_SQUARE). </param>
	public virtual void setLineCap(int cap)
	{
		Stroke s = _jdev._graphics.getStroke();
		BasicStroke newStroke = null;
		if (s is BasicStroke)
		{
			BasicStroke b = (BasicStroke)s;
			newStroke = new BasicStroke(b.getLineWidth(), cap, b.getLineJoin(), b.getMiterLimit(), b.getDashArray(), b.getDashPhase());
		}
		else
		{
			BasicStroke b = new BasicStroke();
			newStroke = new BasicStroke(b.getLineWidth(), cap, b.getLineJoin(), b.getMiterLimit(), b.getDashArray(), b.getDashPhase());
		}
		_jdev._graphics.setStroke(newStroke);
	}

	/// <summary>
	/// Sets whether drawing should be scaled to the data extents (true) or not (false). </summary>
	/// <param name="whether"> to scale drawing to the data extents (true) or not (false). </param>
	public virtual void setScaleData(bool set)
	{
		__scaleXData = set;
		__scaleYData = set;
	}

	private double[] calculateFormatPoints(bool isXAxis, int formatType, string format, double[] points, string[] formatPoints, int pointNum, int totalPoints, int minNumPoints)
	{
		double[] calcPoints = null;
		if (formatType == __FORMAT_DATETIME)
		{
			int index = format.IndexOf(",", StringComparison.Ordinal);
			format = format.Substring(index + 1);

			// see performance note about caching at static
			// declaration of Hashtable (top of class).

			// should not be null here, unless some catastrophic error occurred.
			DateTimeFormat dtf = __dateFormatHashtable[format];
			int num = 0;
			bool isAbsolute = dtf.isAbsolute();
			if (isAbsolute)
			{
				// easy case
				try
				{
					calcPoints = new double[1];
					calcPoints[0] = dtf.parse(formatPoints[pointNum]).toDouble();
				}
				catch (Exception)
				{
					// dates have already been checked.
				}
			}
			else
			{
				// roughly the number of years spanning left-to-right
				if (isXAxis)
				{
					num = (int)_datax2 - (int)_datax1;
				}
				else
				{
					num = (int)_datay2 - (int)_datay1;
				}
				// add some padding
				num += 4;

				DateTime dt1 = null;
				try
				{
					dt1 = dtf.parse(formatPoints[pointNum]);
				}
				catch (Exception)
				{
					// dates have already been checked.
				}

				if (isXAxis)
				{
					dt1.setYear(((int)_datax1) - 2);
				}
				else
				{
					dt1.setYear(((int)_datay1) - 2);
				}

				bool done = false;
				IList<double> v = new List<double>();
				int count = 0;
				while (!done)
				{
					v.Add(new double?(dt1.toDouble()));
					count++;
					dt1 = dtf.iterateRelativeDateTime(dt1);
					if (minNumPoints > -1)
					{
						if (count < minNumPoints)
						{
							// not ready yet
						}
						else
						{
							done = true;
						}
					}
					else
					{
						// use normal count
						if (isXAxis && dt1.getYear() > (int)_datax2 + 2)
						{
							done = true;
						}
						else if (!isXAxis && dt1.getYear() > (int)_datay2 + 2)
						{
							done = true;
						}
					}
				}

				int size = v.Count;
				calcPoints = new double[v.Count];
				for (int i = 0; i < size; i++)
				{
					calcPoints[i] = v[i];
				}
			}
		}
		else if (formatType == __FORMAT_REPEAT_DATA)
		{
			int index = format.IndexOf(",", StringComparison.Ordinal);
			format = format.Substring(index + 1);
			bool pos = true;
			int count = 0;
			if (format.StartsWith("-", StringComparison.Ordinal))
			{
				pos = false;
			}
			format = format.Substring(1);
			double mod = StringUtil.atod(format);

			bool done = false;
			IList<double> v = new List<double>();
			double temp = points[pointNum];
			if (pos)
			{
				double highBound = 0;
				if (isXAxis)
				{
					highBound = _datax2;
				}
				else
				{
					highBound = _datay2;
				}
				while (!done)
				{
					v.Add(new double?(temp));
					count++;
					temp += mod;

					if (minNumPoints > -1)
					{
						if (count < minNumPoints)
						{
							// not ready
						}
						else
						{
							done = true;
						}
					}
					else
					{
						if (temp > highBound)
						{
							done = true;
						}
					}
				}
			}
			else
			{
				double lowBound = 0;
				if (isXAxis)
				{
					lowBound = _datax1;
				}
				else
				{
					lowBound = _datay1;
				}
				while (!done)
				{
					v.Add(new double?(temp));
					count++;
					temp -= mod;

					if (minNumPoints > -1)
					{
						if (count < minNumPoints)
						{
							// not ready
						}
						else
						{
							done = true;
						}
					}
					else
					{
						if (temp < lowBound)
						{
							done = true;
						}
					}
				}
			}

			int size = v.Count;
			calcPoints = new double[size];
			for (int i = 0; i < size; i++)
			{
				calcPoints[i] = v[i];
			}
		}
		else
		{
			if (points.Length == totalPoints)
			{
				calcPoints = points;
			}
			else
			{
				calcPoints = new double[totalPoints];
				for (int i = 0; i < totalPoints; i++)
				{
					calcPoints[i] = points[i];
				}
			}
		}
		return calcPoints;
	}

	private double determineModifier(bool isXAxis, int formatType, string format, string[] formatPoints)
	{
		if (formatType != __FORMAT_DATETIME)
		{
			return 0;
		}
		else
		{
			if (formatPoints.Length == 1)
			{
				// not a line
				return 0;
			}
			// see performance note about caching at static
			// declaration of Hashtable (top of class).

			// should not be null here, unless some catastrophic error occurred.
			DateTimeFormat dtf = __dateFormatHashtable[format];
			DateTime dt1 = null;
			DateTime dt2 = null;
			try
			{
				dt1 = dtf.parse(formatPoints[0]);
			}
			catch (Exception)
			{
				// dates have already been checked.
			}

			try
			{
				dt2 = dtf.parse(formatPoints[1]);
			}
			catch (Exception)
			{
				// dates have already been checked.
			}

			if (dt1.Equals(dt2))
			{
				// wraps around all year long
				return -1;
			}
			else if (dt1.greaterThan(dt2))
			{
				return -1;
			}
			else
			{
				return 0;
			}
		}
	}

	private bool determinePercent(bool percent, int formatType)
	{
		if (formatType == __FORMAT_NONE)
		{
			return percent;
		}
		else if (formatType == __FORMAT_DATETIME)
		{
			return false;
		}
		else if (formatType == __FORMAT_REPEAT_DATA)
		{
			return false;
		}
		else if (formatType == __FORMAT_REPEAT_PERCENT)
		{
			return true;
		}
		else
		{
			return percent;
		}
	}

	}

}