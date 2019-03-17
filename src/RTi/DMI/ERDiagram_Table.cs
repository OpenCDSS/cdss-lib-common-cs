using System;

// ERDiagram_Table - class representing a table object in an ER Diagram

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
// ERDiagram_Table - class representing a table object in an ER Diagram
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2003-07-28	J. Thomas Sapienza, RTi	Initial version.
// 2003-07-29	JTS, RTi		* Completed first version of drawing
//					  code.
//					* Added toString().
//					* Added getFieldY().
//					* Javadoc'd.
// 2003-07-31	JTS, RTi		Added contains().
// 2003-08-27	JTS, RTi		Added __textVisible setting.
// 2003-12-11	JTS, RTi		Added __titleVisible setting.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.DMI
{

	using GRColor = RTi.GR.GRColor;
	using GRDrawingAreaUtil = RTi.GR.GRDrawingAreaUtil;
	using GRLimits = RTi.GR.GRLimits;
	using GRText = RTi.GR.GRText;
	using GRUnits = RTi.GR.GRUnits;
	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// This class is the object representing a table in an ER Diagram.  It stores 
	/// graphical information about the table and can draw itself on a drawing area.
	/// </summary>
	public class ERDiagram_Table : DMIDataObject
	{

	/// <summary>
	/// Whether the bounds have been calculated for the current settings.  If false,
	/// the bounds (the height and width necessary for drawing the table) will be
	/// recalculated at the next time the table is drawn.
	/// </summary>
	private bool __boundsCalculated = false;

	/// <summary>
	/// Whether to draw the framework of the table (true), or the actual table.
	/// Drawing the table as a framework is much faster, because only the outline lines are drawn. 
	/// </summary>
	private bool __drawFramework = false;

	/// <summary>
	/// Whether the table has been marked or not.
	/// </summary>
	private bool __marked = false;

	/// <summary>
	/// Whether the table's text is visible or not.
	/// </summary>
	private bool __textVisible = true;

	/// <summary>
	/// Whether the table's title is visible or not.
	/// </summary>
	private bool __titleVisible = true;

	/// <summary>
	/// Whether the table is visible or not.  If it is not visible, it will not be drawn.
	/// </summary>
	private bool __visible = true;

	/// <summary>
	/// The thickness (in pixels) of the border around the table fields.
	/// </summary>
	private double __borderThickness;

	/// <summary>
	/// The radius (in pixels) of the curve at the corner of the table's 
	/// rectangular-ish shape.  If set to 0, the table will be stored in a true rectangle.
	/// </summary>
	private double __cornerRadius = 0;

	/// <summary>
	/// The space (in pixels) on either side of the line separating key fields from 
	/// non-key fields.  It is calculated to be 1/2 of the average height of the text 
	/// in the table, in calculateBounds().
	/// </summary>
	private double __dividerFieldHeight;

	/// <summary>
	/// The thickness (in pixels) of the line separating key fields from non-key fields.
	/// </summary>
	private double __dividerLineThickness;

	/// <summary>
	/// The distance (in pixels) that the dropshadow is off-set from the table.  If 
	/// set to 0, there will be no drop shadow.  With a positive value, the drop shadow
	/// is displaced the specified number of pixels to the right of and below the table.
	/// If the number is negative, the drop shadow will appear to the left and above the table.
	/// </summary>
	private double __dropShadowOffset;

	/// <summary>
	/// The spacing (in pixels) between the sides of the border and the longest text string in the table.
	/// </summary>
	private double __fieldNameHorizontalSpacing;

	/// <summary>
	/// The spacing (in pixels) between the top border and the first key field, and
	/// the spacing between the last field and the bottom border.
	/// </summary>
	private double __fieldNameVerticalSpacing;

	/// <summary>
	/// The height of the table.  Calculated by calculateBounds().
	/// </summary>
	private double __height;

	/// <summary>
	/// The width of the table.  Calculated by calculateBounds().
	/// </summary>
	private double __width;

	/// <summary>
	/// The lower-left X position of the table.
	/// </summary>
	private double __x;

	/// <summary>
	/// The lower-left Y position of the table.
	/// </summary>
	private double __y;

	/// <summary>
	/// The color in which the area behind the table text is drawn.
	/// </summary>
	private GRColor __backgroundColor;

	/// <summary>
	/// The color in which the drop shadow is drawn.
	/// </summary>
	private GRColor __dropShadowColor;

	/// <summary>
	/// The old background color in which the table was drawn.  Used when the table
	/// is highlighted during a search, in order to restore the original table color when the table is deselected.
	/// </summary>
	private GRColor __oldBackgroundColor;

	/// <summary>
	/// The color in which the table borders and line separating key fields from non-key fields are drawn.
	/// </summary>
	private GRColor __outlineColor;

	/// <summary>
	/// The name of the table.
	/// </summary>
	private string __name;

	/// <summary>
	/// The table key fields.  Can be null.
	/// </summary>
	private string[] __keyFields;

	/// <summary>
	/// The table non-key fields.  Can be null.
	/// </summary>
	private string[] __nonKeyFields;

	/// <summary>
	/// Constructor.  Creates a table with no name.
	/// </summary>
	public ERDiagram_Table() : this((string)null)
	{
	}

	/// <summary>
	/// Constructor.  Creates a table with the specified name. </summary>
	/// <param name="name"> the name of the table to create. </param>
	public ERDiagram_Table(string name)
	{
		setName(name);

		// initialize some default settings.
		__width = 0;
		__height = 0;
		__x = -10000;
		__y = -10000;
		//__visible = false;
		__visible = true;
		__textVisible = true;
		__titleVisible = true;
		__drawFramework = false;
		__boundsCalculated = false;
		__dropShadowOffset = 5;
		__dividerLineThickness = 1;
		__borderThickness = 1;
		__fieldNameVerticalSpacing = 3;
		__fieldNameHorizontalSpacing = 3;
		__cornerRadius = 5;
		__dividerFieldHeight = 10;
		__outlineColor = GRColor.black;
		__dropShadowColor = GRColor.gray;
		__backgroundColor = new GRColor(248, 251, 191);
		__oldBackgroundColor = new GRColor(248, 251, 191);
	}

	/// <summary>
	/// Copy constructor. </summary>
	/// <param name="table"> the ERDiagram_Table object to copy. </param>
	public ERDiagram_Table(ERDiagram_Table table)
	{
		setName(table.getName());

		__width = table.getWidth();
		__height = table.getHeight();
		__textVisible = table.isTextVisible();
		__titleVisible = table.isTitleVisible();
		__x = table.getX();
		__y = table.getY();
		__visible = table.isVisible();
		__drawFramework = table.isFrameworkDrawn();
		__boundsCalculated = false;
		__dropShadowOffset = table.getDropShadowOffset();
		__dividerLineThickness = table.getDividerLineThickness();
		__fieldNameVerticalSpacing = table.getFieldNameVerticalSpacing();
		__fieldNameHorizontalSpacing = table.getFieldNameHorizontalSpacing();
		__cornerRadius = table.getCornerRadius();
		__dividerFieldHeight = table.getDividerFieldHeight();
		__outlineColor = table.getOutlineColor();
		__dropShadowColor = table.getDropShadowColor();
		__backgroundColor = table.getBackgroundColor();
		__oldBackgroundColor = __backgroundColor;
	}

	/// <summary>
	/// Returns whether the bounds are calculated, or whether calculateBounds() needs to be called again. </summary>
	/// <returns> whether the bounds are calculated. </returns>
	public virtual bool areBoundsCalculated()
	{
		return __boundsCalculated;
	}

	/// <summary>
	/// Based on table settings, such as border thickness and the number and length 
	/// of field names, this method recalculates some internal settings necessary to
	/// draw the table properly.  If a call is made to most set*() methods, then the
	/// setBoundsCalculated() flag will be set so that the bounds are reset next time draw() is called. </summary>
	/// <param name="da"> the DrawingArea used to calculate some GRLimits (such as when getting text extents) </param>
	public virtual void calculateBounds(ERDiagram_DrawingArea da)
	{
		string routine = this.GetType().Name + ".calculateBounds";
		setBoundsCalculated(true);

		double height = (__borderThickness * 2);

		double maxTextWidth = 0;

		GRLimits limits = null;

		int length = 0;

		double averageTextHeight = 0;
		int count = 0;

		GRDrawingAreaUtil.setFont(da, "Arial", 10);
		if (__keyFields != null)
		{
			length = __keyFields.Length;
		}
		for (int i = 0; i < length; i++)
		{
			count++;
			limits = GRDrawingAreaUtil.getTextExtents(da, __keyFields[i], GRUnits.DEVICE);

			if (limits.getWidth() > maxTextWidth)
			{
				maxTextWidth = limits.getWidth();
			}

			averageTextHeight += limits.getHeight();

			if (i < (length - 1))
			{
				height += getFieldNameVerticalSpacing() + limits.getHeight();
			}
			else
			{
				height += limits.getHeight();
			}
		}

		if (__nonKeyFields != null)
		{
			length = __nonKeyFields.Length;
		}
		for (int i = 0; i < length; i++)
		{
			count++;
			limits = GRDrawingAreaUtil.getTextExtents(da, __nonKeyFields[i], GRUnits.DEVICE);

			if (limits.getWidth() > maxTextWidth)
			{
				maxTextWidth = limits.getWidth();
			}

			averageTextHeight += limits.getHeight();

			if (i < (length - 1))
			{
				height += getFieldNameVerticalSpacing() + limits.getHeight();
			}
			else
			{
				height += limits.getHeight();
			}
		}

		averageTextHeight /= count;
		averageTextHeight /= 2;
		if (averageTextHeight < 5)
		{
			averageTextHeight = 5;
		}
		__dividerFieldHeight = averageTextHeight;

		height += (__dividerFieldHeight * 2) + __dividerLineThickness;

		height += getFieldNameVerticalSpacing() + (__cornerRadius * 2);

		__height = height;

		maxTextWidth += (__fieldNameHorizontalSpacing * 2) + (__borderThickness * 2);

		__width = maxTextWidth + (2 * __cornerRadius);
		if (__cornerRadius == 0)
		{
			__width += 5;
		}

		if ((__cornerRadius * 2) > __width || (__cornerRadius * 2) > __height)
		{
			__cornerRadius = 0;
		}

		//System.out.println("name: " + __name + " __x: " + __x + "  __y : " 
		//	+ __y + "  __width: " + __width + "  __height: " + __height);
		if (((__x + __width) < 0) && ((__y + __height) < 0))
		{
			Message.printStatus(2, routine, "Setting table to visible=false name =" + __name + " __x: " + __x + " __y : " + __y + "  __width: " + __width + "  __height: " + __height);
			__visible = false;
		}
	}

	/// <summary>
	/// Checks to see if the table contains the given X and Y point in its boundaries.
	/// REVISIT: not taking into account the title of the table yet. </summary>
	/// <param name="x"> the x value to check </param>
	/// <param name="y"> the y value to check </param>
	/// <returns> true if the point is contained in the table.  False if not. </returns>
	public virtual bool contains(double x, double y)
	{
		if ((x >= __x) && (x <= (__x + __width)))
		{
			if ((y >= __y) && (y <= (__y + __height)))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Draws the table to the specified drawing area. </summary>
	/// <param name="da"> the drawing are to which to draw the table. </param>
	public virtual void draw(ERDiagram_DrawingArea da)
	{
		string routine = "draw";
		if (!areBoundsCalculated())
		{
			calculateBounds(da);
		}

		double[] xs = new double[2];
		double[] ys = new double[2];

		Message.printStatus(2,routine,"Drawing table \"" + getName() + "\" at " + __x + ", " + __y);

		int length = 0;
		string text = null;
		GRLimits limits = null;
		double ypos = 0;
		double xpos = 0;

		xs[0] = __x + __cornerRadius;
		xs[1] = __x + __width - __cornerRadius;

		ys[0] = __y + __cornerRadius;
		ys[1] = __y + __height - __cornerRadius;

		if (!__drawFramework)
		{
			if (__dropShadowOffset > 0)
			{
				xs[0] += __dropShadowOffset;
				xs[1] += __dropShadowOffset;

				ys[0] -= __dropShadowOffset;
				ys[1] -= __dropShadowOffset;
				GRDrawingAreaUtil.setColor(da, __dropShadowColor);

				GRDrawingAreaUtil.fillRectangle(da, __x + __dropShadowOffset, ys[0], __width, ys[1] - ys[0]);
				GRDrawingAreaUtil.fillRectangle(da, xs[0], __y - __dropShadowOffset, xs[1] - xs[0], __height);

				GRDrawingAreaUtil.fillArc(da, xs[0], ys[0], __cornerRadius, __cornerRadius, 180, 90, 0);
				GRDrawingAreaUtil.fillArc(da, xs[1], ys[1], __cornerRadius, __cornerRadius, 0, 90, 0);
				GRDrawingAreaUtil.fillArc(da, xs[0], ys[1], __cornerRadius, __cornerRadius, 180, -90, 0);
				GRDrawingAreaUtil.fillArc(da, xs[1], ys[0], __cornerRadius, __cornerRadius, 0, -90, 0);

				xs[0] -= __dropShadowOffset;
				xs[1] -= __dropShadowOffset;

				ys[0] += __dropShadowOffset;
				ys[1] += __dropShadowOffset;
			}

			GRDrawingAreaUtil.setColor(da, __backgroundColor);
		}
		else
		{
			GRDrawingAreaUtil.setColor(da, GRColor.lightGray);
			GRDrawingAreaUtil.drawRectangle(da, __x, ys[0], __width, ys[1] - ys[0]);
			GRDrawingAreaUtil.drawRectangle(da, xs[0], __y, xs[1] - xs[0], __height);
			GRDrawingAreaUtil.drawArc(da, xs[0], ys[0], __cornerRadius, __cornerRadius, 180, 90);
			GRDrawingAreaUtil.drawArc(da, xs[1], ys[1], __cornerRadius, __cornerRadius, 0, 90);
			GRDrawingAreaUtil.drawArc(da, xs[0], ys[1], __cornerRadius, __cornerRadius, 180, -90);
			GRDrawingAreaUtil.drawArc(da, xs[1], ys[0], __cornerRadius, __cornerRadius, 0, -90);
		}

		if (!__drawFramework)
		{
			GRDrawingAreaUtil.fillRectangle(da, __x, ys[0], __width, ys[1] - ys[0]);
			GRDrawingAreaUtil.fillRectangle(da, xs[0], __y, xs[1] - xs[0], __height);
			GRDrawingAreaUtil.fillArc(da, xs[0], ys[0], __cornerRadius, __cornerRadius, 180, 90, 0);
			GRDrawingAreaUtil.fillArc(da, xs[1], ys[1], __cornerRadius, __cornerRadius, 0, 90, 0);
			GRDrawingAreaUtil.fillArc(da, xs[0], ys[1], __cornerRadius, __cornerRadius, 180, -90, 0);
			GRDrawingAreaUtil.fillArc(da, xs[1], ys[0], __cornerRadius, __cornerRadius, 0, -90, 0);

			GRDrawingAreaUtil.setColor(da, __outlineColor);

			GRDrawingAreaUtil.drawLine(da, xs[0], __y, xs[1], __y);
			GRDrawingAreaUtil.drawLine(da, xs[0], __y + __height, xs[1], __y + __height);

			GRDrawingAreaUtil.drawLine(da, __x, ys[0], __x, ys[1]);
			GRDrawingAreaUtil.drawLine(da, __x + __width, ys[0], __x + __width, ys[1]);

			GRDrawingAreaUtil.drawArc(da, xs[0], ys[0], __cornerRadius, __cornerRadius, 180, 90);
			GRDrawingAreaUtil.drawArc(da, xs[1], ys[1], __cornerRadius, __cornerRadius, 0, 90);
			GRDrawingAreaUtil.drawArc(da, xs[0], ys[1], __cornerRadius, __cornerRadius, 180, -90);
			GRDrawingAreaUtil.drawArc(da, xs[1], ys[0], __cornerRadius, __cornerRadius, 0, -90);

			ypos = __y + __height - __cornerRadius;
			if (__cornerRadius == 0)
			{
				xpos = __x + 5;
			}
			else
			{
				xpos = __x + __cornerRadius;
			}
		}

		string loc = " (" + __x + ", " + __y + ")";
		loc = "";
		GRDrawingAreaUtil.setFont(da, "Arial", 10);

		if (__titleVisible)
		{
			GRDrawingAreaUtil.drawText(da, __name + loc, __x, __y + __height + 3, 0, GRText.BOTTOM);
		}

		if (!__drawFramework)
		{
			GRDrawingAreaUtil.setFont(da, "Arial", 10);
			if (__keyFields != null)
			{
				length = __keyFields.Length;
			}

			for (int i = 0; i < length; i++)
			{
				text = __keyFields[i];
				limits = GRDrawingAreaUtil.getTextExtents(da, text, GRUnits.DEVICE);

				if (__textVisible)
				{
				GRDrawingAreaUtil.drawText(da, text, xpos, ypos, 0, GRText.TOP);
				}

				if (i < (length - 1))
				{
					ypos -= limits.getHeight() + __fieldNameVerticalSpacing;
				}
				else
				{
					ypos -= limits.getHeight();
				}
			}
			ypos -= __dividerFieldHeight;
			GRDrawingAreaUtil.drawLine(da, __x, ypos, __x + __width, ypos);
			ypos -= __dividerLineThickness;
			ypos -= __dividerFieldHeight;

			if (__nonKeyFields != null)
			{
				length = __nonKeyFields.Length;
			}

			for (int i = 0; i < length; i++)
			{
				text = __nonKeyFields[i];
				limits = GRDrawingAreaUtil.getTextExtents(da, text, GRUnits.DEVICE);

				if (__textVisible)
				{
				GRDrawingAreaUtil.drawText(da, text, xpos, ypos, 0, GRText.TOP);
				}

				if (i < (length - 1))
				{
					ypos -= limits.getHeight() + __fieldNameVerticalSpacing;
				}
			}
		}
	}

	/// <summary>
	/// Returns the table's background color. </summary>
	/// <returns> the table's background color. </returns>
	public virtual GRColor getBackgroundColor()
	{
		return __backgroundColor;
	}

	/// <summary>
	/// Returns the thickness of the border (in pixels). </summary>
	/// <returns> the thickness of the border (in pixels). </returns>
	public virtual double getBorderThickness()
	{
		return __borderThickness;
	}

	/// <summary>
	/// Returns the exact center point of the table. </summary>
	/// <returns> the exact center point of the table. </returns>
	public virtual Point getCenterPoint()
	{
		int x = (int)__x + (int)(__width / 2);
		int y = (int)__y + (int)(__height / 2);

		return new Point(x, y);
	}

	/// <summary>
	/// Returns the radius (in pixels) of the corner curve. </summary>
	/// <returns> the radius (in pixels) of the corner curve. </returns>
	public virtual double getCornerRadius()
	{
		return __cornerRadius;
	}

	/// <summary>
	/// Returns the divider's field height. </summary>
	/// <returns> the divider's field height. </returns>
	public virtual double getDividerFieldHeight()
	{
		return __dividerFieldHeight;
	}

	/// <summary>
	/// Returns the thickness (in pixels) of the line separating the key and non-key
	/// fields. </summary>
	/// <returns> the thickness (in pixels) of the line separating the key and non-key
	/// fields. </returns>
	public virtual double getDividerLineThickness()
	{
		return __dividerLineThickness;
	}

	/// <summary>
	/// Returns the drop shadow color. </summary>
	/// <returns> the drop shadow color. </returns>
	public virtual GRColor getDropShadowColor()
	{
		return __dropShadowColor;
	}

	/// <summary>
	/// Returns the offset (in pixels) of the drop shadow. </summary>
	/// <returns> the offset (in pixels) of the drop shadow. </returns>
	public virtual double getDropShadowOffset()
	{
		return __dropShadowOffset;
	}

	/// <summary>
	/// Returns the spacing (in pixels) between the left and right borders and the 
	/// longest string in the table. </summary>
	/// <returns> the spacing (in pixels) between the left and right borders and the 
	/// longest string in the table. </returns>
	public virtual double getFieldNameHorizontalSpacing()
	{
		return __fieldNameHorizontalSpacing;
	}

	/// <summary>
	/// Returns the spacing (in pixels) between the top border and the first field
	/// and the bottom border and the last field. </summary>
	/// <returns> the spacing (in pixels) between the top border and the first field
	/// and the bottom border and the last field. </returns>
	public virtual double getFieldNameVerticalSpacing()
	{
		return __fieldNameVerticalSpacing;
	}

	/// <summary>
	/// Get the Y position of the field with the specified name (case-insensitive)
	/// in the table.  This is used for calculating the position of connection
	/// lines between tables.  This method can return the value of Y at the Top, 
	/// bottom, or middle of the text, depending on the value of <pre>flag</pre>. </summary>
	/// <param name="da"> the drawing are on which the table is drawn. </param>
	/// <param name="fieldName"> the name of the field for which to return the Y position of
	/// the text. </param>
	/// <param name="flag"> the flag specifying which Y value (the top, middle, or bottom)
	/// should be returned.  If GRText.TOP is specified, the top of the String will 
	/// be returned as the Y location.  If GRText.BOTTOM is specified, the bottom 
	/// of the String will be returned as the Y location.  If GRText.CENTER_Y is
	/// specified, the center of the String will be returned as the Y location.  
	/// If anything else is specified, the top of the String will be returned. </param>
	public virtual double getFieldY(ERDiagram_DrawingArea da, string fieldName, int flag)
	{
		if (!__boundsCalculated)
		{
			calculateBounds(da);
		}
		int length = 0;
		if (__keyFields != null)
		{
			length = __keyFields.Length;
		}

		GRDrawingAreaUtil.setFont(da, "Arial", 10);
		double ypos = 0;
		ypos = __y + __height - __cornerRadius;
		string text = null;
		GRLimits limits = null;
		int colon;
		for (int i = 0; i < length; i++)
		{
			text = __keyFields[i];
			colon = text.IndexOf(':');
			if (colon > -1)
			{
				text = text.Substring(0, colon);
			}
			limits = GRDrawingAreaUtil.getTextExtents(da, text, GRUnits.DEVICE);
			if (text.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
			{
				if (flag == GRText.TOP)
				{
					return ypos;
				}
				else if (flag == GRText.BOTTOM)
				{
					return (ypos - limits.getHeight());
				}
				else if (flag == GRText.CENTER_Y)
				{
					return (double)((int)(ypos - (limits.getHeight() / 2)));
				}
				else
				{
					return ypos;
				}
			}

			if (i < (length - 1))
			{
				ypos -= limits.getHeight() + __fieldNameVerticalSpacing;
			}
			else
			{
				ypos -= limits.getHeight();
			}
		}
		ypos -= __dividerFieldHeight;
		GRDrawingAreaUtil.drawLine(da, __x, ypos, __x + __width, ypos);
		ypos -= __dividerLineThickness;
		ypos -= __dividerFieldHeight;

		if (__nonKeyFields != null)
		{
			length = __nonKeyFields.Length;
		}

		for (int i = 0; i < length; i++)
		{
			text = __nonKeyFields[i];
			colon = text.IndexOf(':');
			if (colon > -1)
			{
				text = text.Substring(0, colon);
			}
			limits = GRDrawingAreaUtil.getTextExtents(da, text, GRUnits.DEVICE);

			if (text.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
			{
				if (flag == GRText.TOP)
				{
					return ypos;
				}
				else if (flag == GRText.BOTTOM)
				{
					return (ypos - limits.getHeight());
				}
				else if (flag == GRText.CENTER_Y)
				{
					return (double)((int)(ypos - (limits.getHeight() / 2)));
				}
				else
				{
					return ypos;
				}
			}

			if (i < (length - 1))
			{
				ypos -= limits.getHeight() + __fieldNameVerticalSpacing;
			}
		}

		return -1;
	}

	/// <summary>
	/// Returns the height (in pixels) of the curved rectangle containing the table 
	/// data. </summary>
	/// <returns> the height (in pixels) of the curved rectangle containing the table
	/// data. </returns>
	public virtual double getHeight()
	{
		return __height;
	}

	/// <summary>
	/// Returns the name of the key field at the given position in the key fields
	/// array. </summary>
	/// <returns> the name of the key field at the given position in the key fields
	/// array. </returns>
	public virtual string getKeyField(int field)
	{
		if (__keyFields == null)
		{
			return null;
		}
		return __keyFields[field];
	}

	/// <summary>
	/// Returns the number of key fields in the table. </summary>
	/// <returns> the number of key fields in the table. </returns>
	public virtual int getKeyFieldCount()
	{
		if (__keyFields == null)
		{
			return 0;
		}
		return __keyFields.Length;
	}

	/// <summary>
	/// Returns the limits of the table. </summary>
	/// <returns> the limits of the table. </returns>
	public virtual GRLimits getLimits()
	{
		return new GRLimits(__x, __y, __x + __width, __y + __height);
	}

	/// <summary>
	/// Returns the name of the table. </summary>
	/// <returns> the name of the table. </returns>
	public virtual string getName()
	{
		return __name;
	}

	/// <summary>
	/// Returns the name of the non-key field at the given position in the non-key
	/// fields array. </summary>
	/// <returns> the name of the non-key field at the given position in the non-key
	/// fields array. </returns>
	public virtual string getNonKeyField(int field)
	{
		if (__nonKeyFields == null)
		{
			return null;
		}
		return __nonKeyFields[field];
	}

	/// <summary>
	/// Returns the number of key fields in the table. </summary>
	/// <returns> the number of key fields in the table. </returns>
	public virtual int getNonKeyFieldCount()
	{
		if (__nonKeyFields == null)
		{
			return 0;
		}
		return __nonKeyFields.Length;
	}

	/// <summary>
	/// Returns the table's outline color. </summary>
	/// <returns> the table's outline color. </returns>
	public virtual GRColor getOutlineColor()
	{
		return __outlineColor;
	}

	/// <summary>
	/// Returns the width (in pixels) of the curved rectangle containing the table data. </summary>
	/// <returns> the width (in pixels) of the curved rectangle containing the table data. </returns>
	public virtual double getWidth()
	{
		return __width;
	}

	/// <summary>
	/// Returns the X position of the table. </summary>
	/// <returns> the X position of the table. </returns>
	public virtual double getX()
	{
		return __x;
	}

	/// <summary>
	/// Returns the Y position of the table. </summary>
	/// <returns> the Y position of the table. </returns>
	public virtual double getY()
	{
		return __y;
	}

	/// <summary>
	/// Returns whether the table's framework is drawn or not. </summary>
	/// <returns> whether the table's framework is drawn or not. </returns>
	public virtual bool isFrameworkDrawn()
	{
		return __drawFramework;
	}

	/// <summary>
	/// Returns whether the table has been marked or not. </summary>
	/// <returns> whether the table has been marked or not. </returns>
	public virtual bool isMarked()
	{
		return __marked;
	}

	/// <summary>
	/// Returns whether the table's text is visible (true) or not (false). </summary>
	/// <returns> whether the table's text is visible or not. </returns>
	public virtual bool isTextVisible()
	{
		return __textVisible;
	}

	/// <summary>
	/// Returns whether the table's title is visible (true) or not (false). </summary>
	/// <returns> whether the table's title is visible or not. </returns>
	public virtual bool isTitleVisible()
	{
		return __titleVisible;
	}

	/// <summary>
	/// Returns whether the table is visible or not. </summary>
	/// <returns> whether the table is visible or not. </returns>
	public virtual bool isVisible()
	{
		return __visible;
	}

	/// <summary>
	/// Checks to see whether any portion of the table can found within the given
	/// GRLimits.  If so, returns true. </summary>
	/// <param name="limits"> the limits for which to check an intersection with the table. </param>
	/// <returns> true if any portion of the table can be found within the specified
	/// limits. </returns>
	public virtual bool isWithinLimits(GRLimits limits, double scale)
	{
		return limits.contains(__x * scale, __y * scale, (__x + __width) * scale, (__y + __height) * scale, false);
	}

	/// <summary>
	/// Marks the table selected or not.  When the table is selected, it is currently
	/// drawn in a hard-coded color (red).  If the table is marked not selected, its 
	/// old background color will be restored. </summary>
	/// <param name="selected"> whether the table is selected or not. </param>
	public virtual void markSelected(bool selected)
	{
		if (selected)
		{
			__oldBackgroundColor = __backgroundColor;
			__backgroundColor = GRColor.red;
			__marked = true;
		}
		else
		{
			__backgroundColor = __oldBackgroundColor;
			__marked = false;
		}
	}

	/// <summary>
	/// Sets the thickness of the border around the table (in pixels).  
	/// If the value of the border thickness is changed, 
	/// calculateBounds() will be called before the table is next drawn. </summary>
	/// <param name="thickness"> the thickness to set. </param>
	public virtual void setBorderThickness(double thickness)
	{
		if (thickness != __borderThickness)
		{
			__boundsCalculated = false;
			__borderThickness = thickness;
		}
	}

	/// <summary>
	/// Sets whether the bounds have been calculated, or whether they need to be 
	/// calculated again. </summary>
	/// <param name="boundsCalculated"> true if the bounds have been calculated, or false if
	/// they should be reculated before the table is drawn next time. </param>
	private void setBoundsCalculated(bool boundsCalculated)
	{
		__boundsCalculated = boundsCalculated;
	}

	/// <summary>
	/// Sets the radius of the curve at the corner of the table rectangle.  
	/// If set to 0, the table will be stored in a true rectangle.
	/// If the radius size is changed, calculateBounds() will be called before the 
	/// table is next drawn. </summary>
	/// <param name="radius"> the radius to set. </param>
	public virtual void setCornerRadius(double radius)
	{
		if (__cornerRadius != radius)
		{
			__boundsCalculated = false;
			__cornerRadius = radius;
		}
	}

	/// <summary>
	/// Sets the thickness (in pixels) of the line separating the key fields from the 
	/// non-key fields.  If the thickness of the divider lines is changed, 
	/// calculateBounds() will be called before the table is next drawn. </summary>
	/// <param name="thickness"> the thickness to be set. </param>
	public virtual void setDividerLineThickness(double thickness)
	{
		if (__dividerLineThickness != thickness)
		{
			__boundsCalculated = false;
			__dividerLineThickness = thickness;
		}
	}

	/// <summary>
	/// Sets whether the table's framework (outline lines) should be drawn or not.
	/// When the framework is drawn, no field information is visible, but it is useful
	/// for debugging. </summary>
	/// <param name="framework"> whether the framework should be drawn or not. </param>
	public virtual void setDrawFramework(bool framework)
	{
		__drawFramework = framework;
	}

	/// <summary>
	/// Sets the offset of the table dropshadow (in pixels). 
	/// If set to 0, there will be no drop 
	/// shadow.  With a positive value, the drop shadow is displaced the specified 
	/// number of pixels to the right of and below the table.  If the number is 
	/// negative, the drop shadow will appear to the left and above the table.
	/// If the drop shadow offset is changed, calculateBounds() will be called 
	/// before the table is next drawn. </summary>
	/// <param name="offset"> the offset to set. </param>
	public virtual void setDropShadowOffset(double offset)
	{
		if (__dropShadowOffset != offset)
		{
			__boundsCalculated = false;
			__dropShadowOffset = offset;
		}
	}

	/// <summary>
	/// Sets the spacing (in pixels) between the right and left borders and the 
	/// longest text string in the table.
	/// If the spacing is changed, calculateBounds() will be called before the table is
	/// next drawn. </summary>
	/// <param name="spacing"> the spacing to set. </param>
	public virtual void setFieldNameHorizontalSpacing(double spacing)
	{
		if (__fieldNameHorizontalSpacing != spacing)
		{
			__boundsCalculated = false;
			__fieldNameHorizontalSpacing = spacing;
		}
	}

	/// <summary>
	/// Sets the spacing (in pixels) between the top border and the first text field 
	/// and the bottom border and the last non-key field.
	/// If the spacing is changed, calculateBounds() will be called before the table is
	/// next drawn. </summary>
	/// <param name="spacing"> the spacing to set. </param>
	public virtual void setFieldNameVerticalSpacing(double spacing)
	{
		if (__fieldNameVerticalSpacing != spacing)
		{
			__boundsCalculated = false;
			__fieldNameVerticalSpacing = spacing;
		}
	}

	/// <summary>
	/// Sets the key fields.  If keyFields is null, no keyfields will be put in the 
	/// table.
	/// calculateBounds() will be called before the table is next drawn. </summary>
	/// <param name="keyFields"> the keyFields to set.  Can be null. </param>
	public virtual void setKeyFields(string[] keyFields)
	{
		__boundsCalculated = false;
		__keyFields = keyFields;
	}

	/// <summary>
	/// Sets the name of the table.
	/// If the name of the table changed, calculateBounds() will be called 
	/// before the table is next drawn. </summary>
	/// <param name="name"> the name of the table. </param>
	public virtual void setName(string name)
	{
		if (string.ReferenceEquals(name, null))
		{
			__boundsCalculated = false;
			__name = "";
			return;
		}
		if (!name.Equals(__name))
		{
			__boundsCalculated = false;
			__name = name;
		}
	}

	/// <summary>
	/// Sets the non-key fields.  If nonKeyFields is null, no non-key fields will be
	/// put in the table.
	/// table.
	/// calculateBounds() will be called before the table is next drawn. </summary>
	/// <param name="nonKeyFields"> the non-key fields to set.  Can be null. </param>
	public virtual void setNonKeyFields(string[] nonKeyFields)
	{
		__boundsCalculated = false;
		__nonKeyFields = nonKeyFields;
	}

	/// <summary>
	/// Sets whether the table's text will be visible or not. </summary>
	/// <param name="visible"> whether the table's text will be visible (true) or not (false). </param>
	public virtual void setTextVisible(bool visible)
	{
		__textVisible = visible;
	}

	/// <summary>
	/// Sets whether the table's title will be visible or not. </summary>
	/// <param name="visible"> whether the table's title will be visible (true) or not (false). </param>
	public virtual void setTitleVisible(bool visible)
	{
		__titleVisible = visible;
	}

	/// <summary>
	/// Sets whether the table is visible or not.
	/// If the table's visibility changes, calculateBounds() will be called before the table is next drawn. </summary>
	/// <param name="visible"> whether the table should be visible or not. </param>
	public virtual void setVisible(bool visible)
	{
		if (__visible != visible)
		{
			__boundsCalculated = false;
			__visible = visible;
		}
	}

	/// <summary>
	/// Sets the X position of the table.  If the value of X changes, calculateBounds() will be called before the table is next drawn. </summary>
	/// <param name="x"> the X position of the table. </param>
	public virtual void setX(double x)
	{
		if (__x != x)
		{
			__boundsCalculated = false;
			__x = x;
		}
	}

	/// <summary>
	/// Sets the Y position of the table.  If the value of Y changes, calculateBounds() will be called before the table is next drawn. </summary>
	/// <param name="y"> the Y position of the table. </param>
	public virtual void setY(double y)
	{
		if (__y != y)
		{
			__boundsCalculated = false;
			__y = y;
		}
	}

	/// <summary>
	/// Returns a String representing the table. </summary>
	/// <returns> a String representing the table. </returns>
	public override string ToString()
	{
		string str = "Table: '" + __name + "'\n"
			+ "  Position: " + __x + ", " + __y + "\n"
			+ "  Width:    " + __width + "\n"
			+ "  Height:   " + __height + "\n"
			+ "  Visible: " + __visible + "\n"
			+ "  Horizontal Field Spacing: " + __fieldNameHorizontalSpacing + "\n"
			+ "  Vertical Field Spacing:   " + __fieldNameVerticalSpacing + "\n"
			+ "  Divider Line Thickness:   " + __dividerLineThickness + "\n"
			+ "  Divider Field Height:     " + __dividerFieldHeight + "\n"
			+ "  Border Thickness:         " + __borderThickness + "\n"
			+ "  Corner Radius:            " + __cornerRadius + "\n"
			+ "  Drop Shadow Offset:       " + __dropShadowOffset + "\n"
			+ "  Outline Color:            " + __outlineColor + "\n"
			+ "  Background Color:         " + __backgroundColor + "\n"
			+ "  Drop Shadow Color:        " + __dropShadowColor + "\n"
			+ "  Key Fields: \n";
		if (__keyFields == null)
		{
			str += "    (None)\n";
		}
		else
		{
			for (int i = 0; i < __keyFields.Length; i++)
			{
				str += "    [" + i + "]: '" + __keyFields[i] + "'\n";
			}
		}
		str += "  Non-Key Fields:\n";
		if (__nonKeyFields == null)
		{
			str += "    (None)\n";
		}
		else
		{
			for (int i = 0; i < __nonKeyFields.Length; i++)
			{
				str += "    [" + i + "]: '" + __nonKeyFields[i] + "'\n";
			}
		}
		str += "\n";
		return str;
	}

	}

}