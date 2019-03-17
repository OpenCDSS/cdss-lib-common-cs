using System;
using System.Collections.Generic;

// GRColor - class to store GRColors and color methods

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
// GRColor - class to store GRColors and color methods.
// ---------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file
// ---------------------------------------------------------------------------
// History:
//
// ?		Steven A. Malers, RTi	Initial version.
// 2000-10-15	SAM, RTi		Add COLOR_NAMES[], parseColor(),
//					toIngeter(), and toString().
// 2001-06-28	SAM, RTi		Change parseColor() to return a GRColor.
//					Try using 0, 0, -1 for None color for
//					transparency - does not work.  To
//					support, transparency, add
//					isTransparent().  Allow parseColor() to
//					parse strings with floating point or
//					integer RGB values.
// 2003-05-07	J. Thomas Sapienza, RTi	Made changes following review by SAM.
// 2004-10-27	JTS, RTi		Implements Cloneable.
// ---------------------------------------------------------------------------

namespace RTi.GR
{

	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// Class to store a color.  This class extends Color and adds features like using
	/// named colors to simplify use with GUIs.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class GRColor extends java.awt.Color implements Cloneable
	public class GRColor : Color, ICloneable
	{

	/// <summary>
	/// Names of colors.
	/// </summary>
	public static readonly string[] COLOR_NAMES = new string[] {"None", "Black", "Blue", "Cyan", "DarkGray", "Gray", "Green", "LightGray", "Magenta", "Orange", "Pink", "Red", "White", "Yellow"};

	// The following works.  Casting does not because it is forward referencing.
	/// <summary>
	/// All of the static GR colors are available.
	/// </summary>

	public static readonly GRColor black = new GRColor(0.0, 0.0, 0.0);
	public static readonly GRColor blue = new GRColor(0.0, 0.0, 1.0);
	public static readonly GRColor cyan = new GRColor(0.0, 1.0, 1.0);
	public static readonly GRColor darkGray = new GRColor(.184, .31, .31);
	public static readonly GRColor gray = new GRColor(.753, .753, .753);
	public static readonly GRColor green = new GRColor(0.0, 1.0, 0.0);
	public static readonly GRColor lightGray = new GRColor(.659, .659, .659);
	public static readonly GRColor magenta = new GRColor(1.0, 0.0, 1.0);
	public static readonly GRColor orange = new GRColor(.8, .196, .196);
	public static readonly GRColor pink = new GRColor(.737, .561, .561);
	public static readonly GRColor red = new GRColor(1.0, 0.0, 0.0);
	public static readonly GRColor white = new GRColor(1.0, 1.0, 1.0);
	public static readonly GRColor yellow = new GRColor(1.0, 1.0, 0.0);

	public static readonly GRColor gray10 = new GRColor(0.1, 0.1, 0.1);
	public static readonly GRColor gray20 = new GRColor(0.2, 0.2, 0.2);
	public static readonly GRColor gray30 = new GRColor(0.3, 0.3, 0.3);
	public static readonly GRColor gray40 = new GRColor(0.4, 0.4, 0.4);
	public static readonly GRColor gray50 = new GRColor(0.5, 0.5, 0.5);
	public static readonly GRColor gray60 = new GRColor(0.6, 0.6, 0.6);
	public static readonly GRColor gray70 = new GRColor(0.7, 0.7, 0.7);
	public static readonly GRColor gray80 = new GRColor(0.8, 0.8, 0.8);
	public static readonly GRColor gray90 = new GRColor(0.9, 0.9, 0.9);

	public static readonly GRColor grey10 = new GRColor(0.1, 0.1, 0.1);
	public static readonly GRColor grey20 = new GRColor(0.2, 0.2, 0.2);
	public static readonly GRColor grey30 = new GRColor(0.3, 0.3, 0.3);
	public static readonly GRColor grey40 = new GRColor(0.4, 0.4, 0.4);
	public static readonly GRColor grey50 = new GRColor(0.5, 0.5, 0.5);
	public static readonly GRColor grey60 = new GRColor(0.6, 0.6, 0.6);
	public static readonly GRColor grey70 = new GRColor(0.7, 0.7, 0.7);
	public static readonly GRColor grey80 = new GRColor(0.8, 0.8, 0.8);
	public static readonly GRColor grey90 = new GRColor(0.9, 0.9, 0.9);

	/// <summary>
	/// Indicates whether the color is transparent or not.
	/// </summary>
	private bool _is_transparent = false;

	/// <summary>
	/// Constructor.  Builds a GRColor with the given red, green and blue values. </summary>
	/// <param name="r"> the red value in the range (0.0 - 1.0) </param>
	/// <param name="g"> the green value in the range (0.0 - 1.0) </param>
	/// <param name="b"> the blue value in the range (0.0 - 1.0) </param>
	public GRColor(double r, double g, double b) : base((float)r, (float)g, (float)b)
	{
	}

	/// <summary>
	/// Constructor.  Builds a GRColor with the given color.
	/// <para>
	/// From the Color.java javadocs:
	/// </para>
	/// <para>
	/// Creates an opaque sRGB color with the specified combined RGB value consisting of the red 
	/// component in bits 16-23, the green component in bits 8-15, and the blue 
	/// component in bits 0-7. The actual color used in rendering depends on 
	/// finding the best match given the color space available for a particular 
	/// output device. Alpha is defaulted to 255.
	/// </para>
	/// </summary>
	/// <param name="rgb"> the color to set this GRColor to. </param>
	public GRColor(int rgb) : base(rgb)
	{
	}

	/// <summary>
	/// Constructor.  Builds a GRColor with the given red, green and blue values. </summary>
	/// <param name="r"> the red value in the range (0 - 255) </param>
	/// <param name="g"> the green value in the range (0 - 255) </param>
	/// <param name="b"> the blue value in the range (0 - 255) </param>
	public GRColor(int r, int g, int b) : base(r, g, b)
	{
	}

	/// <summary>
	/// Constructor.  Builds a GRColor with the given red, green and blue values. </summary>
	/// <param name="r"> the red value in the range (0 - 255) </param>
	/// <param name="g"> the green value in the range (0 - 255) </param>
	/// <param name="b"> the blue value in the range (0 - 255) </param>
	/// <param name="a"> the opacity in the range (0-255) </param>
	public GRColor(int r, int g, int b, int a) : base(r, g, b, a)
	{
	}

	/// <summary>
	/// Constructor.  Builds a GRColor with the given red, green and blue values. </summary>
	/// <param name="r"> the red value in the range (0.0 - 1.0) </param>
	/// <param name="g"> the green value in the range (0.0 - 1.0) </param>
	/// <param name="b"> the blue value in the range (0.0 - 1.0) </param>
	public GRColor(float r, float g, float b) : base(((r < 0) ? (float)0.0 : (r > 1.0) ? (float)1.0 : r), ((g < 0) ? (float)0.0 : (g > 1.0) ? (float)1.0 : g), ((b < 0) ? (float)0.0 : (b > 1.0) ? (float)1.0 : b))
	{
	// NOTE!!
	// This code is UGLY, but it's that way for a reason.  The original intent 
	// was to have the constructor check to make sure that the color levels are
	// in the bounds of 0.0 to 1.0, but since the call to "super" has be the
	// first statement in a constructor, this was the only way to do it.

	// Each parameter of the call to super (float, float, float) is a decision
	// tree that first checks:
	// is the color (r, g, or b) less than 0.0?
	//     if so --> pass in 0.0 as the parameter otherwise ...
	//     is the color greater than 1.0?
	//         if so --> pass in 1.0 as the parameter otherwise ...
	//         pass in the color itself.  
	}

	/// <summary>
	/// Clones the Object. </summary>
	/// <returns> a clone of the object. </returns>
	public virtual object clone()
	{
		try
		{
			return (GRColor)base.clone();
		}
		catch (Exception)
		{
			return null;
		}
	}

	/// <summary>
	/// Finalize before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~GRColor()
	{
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Indicate whether color is transparent (no color). </summary>
	/// <returns> true if transparent. </returns>
	public virtual bool isTransparent()
	{
		return _is_transparent;
	}

	/// <summary>
	/// Set whether color is transparent (no color). </summary>
	/// <param name="is_transparent"> True if color is transparent. </param>
	/// <returns> true if transparent, after set. </returns>
	public virtual bool isTransparent(bool is_transparent)
	{
		_is_transparent = is_transparent;
		return _is_transparent;
	}

	/// <summary>
	/// Define a new color determined by parsing the string.  This version is used
	/// because the base class already has a getColor() method.  Constructing from a
	/// String is not supported because the Color base class cannot be easily
	/// initialized.  Valid color strings include the following:
	/// <ul>
	/// <li>	Color name (e.g., "Black", or any other recognized color name included in COLOR_NAMES).</li>
	/// <li>	Floating point RGB in the range 0 to 1 (e.g., "0.0,1.0,1.0").  The
	/// numbers must be floating point numbers (a period in the string will
	/// indicate that all are floating point numbers).</li>
	/// <li>	Integer RGB in the range 0 to 255 (e.g., 0,255,255).</li>
	/// <li>	Integer color where leftmost bits are ignored and others are in order RGB (e.g., 255 is blue).</li>
	/// <li>	Hexadecimal similar to previous version (e.g., 0x000000ff).</li>
	/// </ul> </summary>
	/// <param name="color"> Name of color (see COLOR_NAMES). </param>
	/// <returns> a new Color instance, or black if the name cannot be matched or an error occurs. </returns>
	public static GRColor parseColor(string color)
	{
		if (color.IndexOf('.') >= 0)
		{
			// Assume 0.0-1.0 RGB values separated by commas
			IList<string> v = StringUtil.breakStringList(color,",",StringUtil.DELIM_SKIP_BLANKS);
			if ((v == null) || (v.Count != 3))
			{
				v = null;
				return new GRColor(0);
			}
			GRColor grc = new GRColor(StringUtil.atof(v[0]), StringUtil.atof(v[1]), StringUtil.atof(v[2]));
			return grc;
		}
		else if (color.IndexOf(',') >= 0)
		{
			// Assume 0-255 RGB values separated by commas
			IList<string> v = StringUtil.breakStringList(color,",",StringUtil.DELIM_SKIP_BLANKS);
			if ((v == null) || (v.Count != 3))
			{
				return new GRColor(0);
			}
			GRColor grc = new GRColor(StringUtil.atoi(v[0]), StringUtil.atoi(v[1]), StringUtil.atoi(v[2]));
			return grc;
		}
		else if (color.Equals("black", StringComparison.OrdinalIgnoreCase))
		{
			return new GRColor(0, 0, 0);
		}
		else if (color.Equals("blue", StringComparison.OrdinalIgnoreCase))
		{
			return new GRColor(0, 0, 255);
		}
		else if (color.Equals("cyan", StringComparison.OrdinalIgnoreCase))
		{
			return new GRColor(0, 255, 255);
		}
		else if (color.Equals("darkgray", StringComparison.OrdinalIgnoreCase) || color.Equals("darkgrey", StringComparison.OrdinalIgnoreCase))
		{
			return new GRColor(84, 84, 84);
		}
		else if (color.Equals("gray", StringComparison.OrdinalIgnoreCase) || color.Equals("grey", StringComparison.OrdinalIgnoreCase))
		{
			return new GRColor(192, 192, 192);
		}
		else if (color.Equals("green", StringComparison.OrdinalIgnoreCase))
		{
			return new GRColor(0, 255, 0);
		}
		else if (color.Equals("lightgray", StringComparison.OrdinalIgnoreCase) || color.Equals("lightgrey", StringComparison.OrdinalIgnoreCase))
		{
			return new GRColor(168, 168, 168);
		}
		else if (color.Equals("magenta", StringComparison.OrdinalIgnoreCase))
		{
			return new GRColor(255, 0, 255);
		}
		else if (color.Equals("none", StringComparison.OrdinalIgnoreCase))
		{
			GRColor grc = new GRColor(0, 0, 0);
			grc.isTransparent(true);
			return grc;
		}
		else if (color.Equals("orange", StringComparison.OrdinalIgnoreCase))
		{
			return new GRColor(255, 165, 0);
		}
		else if (color.Equals("pink", StringComparison.OrdinalIgnoreCase))
		{
			return new GRColor(188, 143, 143);
		}
		else if (color.Equals("red", StringComparison.OrdinalIgnoreCase))
		{
			return new GRColor(255, 0, 0);
		}
		else if (color.Equals("white", StringComparison.OrdinalIgnoreCase))
		{
			return new GRColor(255, 255, 255);
		}
		else if (color.Equals("yellow", StringComparison.OrdinalIgnoreCase))
		{
			return new GRColor(255, 255, 0);
		}
		try
		{
			// Try base class method to decode hex into an integer...
			Color c = decode(color);
			GRColor grc = new GRColor(c.getRed(), c.getGreen(), c.getBlue());
			return grc;
		}
		catch (Exception)
		{
			//Message.printWarning ( 1, "", e );
			; // just return black
		}
		return new GRColor(0);
	}

	/// <summary>
	/// Return the RGB integer value for a named color (00RRGGBB).  If the color
	/// cannot be matched, the integer version of the color is returned (e.g., if the
	/// String is "0x000000ff", then 255 will be returned.  If no conversion can be
	/// made, then zero (black) is returned. </summary>
	/// <param name="color"> Name of color (see COLOR_NAMES). </param>
	/// <returns> integer value corresponding to the color or -1 if not found as a named color. </returns>
	public static int toInteger(string color)
	{
		if (color.Equals("black", StringComparison.OrdinalIgnoreCase))
		{
			return 0x00000000;
		}
		else if (color.Equals("blue", StringComparison.OrdinalIgnoreCase))
		{
			return 0x000000ff;
		}
		else if (color.Equals("cyan", StringComparison.OrdinalIgnoreCase))
		{
			return 0x0000ffff;
		}
		else if (color.Equals("darkgray", StringComparison.OrdinalIgnoreCase) || color.Equals("darkgrey", StringComparison.OrdinalIgnoreCase))
		{
			return 0x003f3f3f;
		}
		else if (color.Equals("gray", StringComparison.OrdinalIgnoreCase) || color.Equals("grey", StringComparison.OrdinalIgnoreCase))
		{
			return 0x007f7f7f;
		}
		else if (color.Equals("green", StringComparison.OrdinalIgnoreCase))
		{
			return 0x0000ff00;
		}
		else if (color.Equals("lightgray", StringComparison.OrdinalIgnoreCase) || color.Equals("lightgrey", StringComparison.OrdinalIgnoreCase))
		{
			return 0x00bebebe;
		}
		else if (color.Equals("magenta", StringComparison.OrdinalIgnoreCase))
		{
			return 0x00ff00ff;
		}
		else if (color.Equals("none", StringComparison.OrdinalIgnoreCase))
		{
			return -1;
		}
		else if (color.Equals("orange", StringComparison.OrdinalIgnoreCase))
		{
			return 0x00ff7f00;
		}
		else if (color.Equals("pink", StringComparison.OrdinalIgnoreCase))
		{
			return 0x00ff7f7f;
		}
		else if (color.Equals("red", StringComparison.OrdinalIgnoreCase))
		{
			return 0x00ff0000;
		}
		else if (color.Equals("white", StringComparison.OrdinalIgnoreCase))
		{
			return 0x00ffffff;
		}
		else if (color.Equals("yellow", StringComparison.OrdinalIgnoreCase))
		{
			return 0x00ffff00;
		}
		return StringUtil.atoi(color);
	}

	/// <summary>
	/// Return the named String value matching an integer RGB color.  If a named color
	/// cannot be determined, then the integer value is returned as a string. </summary>
	/// <param name="color"> Color as integer. </param>
	/// <returns> String value corresponding to the color or null if not found as a named color. </returns>
	public static string ToString(int color)
	{
		if (color == 0x00000000)
		{
			return "Black";
		}
		else if (color == 0x000000ff)
		{
			return "Blue";
		}
		else if (color == 0x0000ffff)
		{
			return "Cyan";
		}
		else if (color == 0x003f3f3f)
		{
			return "DarkGray";
		}
		else if (color == 0x007f7f7f)
		{
			return "Gray";
		}
		else if (color == 0x0000ff00)
		{
			return "Green";
		}
		else if (color == 0x00bebebe)
		{
			return "LightGray";
		}
		else if (color == 0x00ff00ff)
		{
			return "Magenta";
		}
		else if (color == -1)
		{
			return "None";
		}
		else if (color == 0x00ffa500)
		{
			return "Orange";
		}
		else if (color == 0x00ff7f7f)
		{
			return "Pink";
		}
		else if (color == 0x00ff0000)
		{
			return "Red";
		}
		else if (color == 0x00ffffff)
		{
			return "White";
		}
		else if (color == 0x00ffff00)
		{
			return "Yellow";
		}
		return "" + color;
	}

	/// <summary>
	/// Return string value. </summary>
	/// <returns> String representation of color. </returns>
	public override string ToString()
	{
		return ToString(getRGB());
	}

	}

}