using System;

// GRText - store text attributes

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
// GRText - store text attributes
// ----------------------------------------------------------------------------
// History:
//
// 2001-10-11	Steven A. Malers, RTi	Add getLabelPositions() and toString().
// 2001-10-15	SAM, RTi		Change to getTextPositions() and add
//					parseTextPosition().
// 2002-02-07	SAM, RTi		Change text positions "LeftCenter" to
//					"Left" and "RightCenter" to "Right" to
//					be consistent with C++.
// ----------------------------------------------------------------------------

namespace RTi.GR
{
	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// Class to store text attributes.  Note that internally text positions are
	/// specified using justification.  For example, LEFT means that the text is
	/// left-justified but will be to the right of a symbol.  For user interfaces, the
	/// positions are identified using relative positions (not justification).
	/// Therefore, for the above example, the position would be specified as
	/// Right or one of the other "Right" positions.
	/// </summary>
	public class GRText
	{

	/// <summary>
	/// Flags that indicate position of text.  These flags are used as a mask with text drawing methods.
	/// The left edge of the text will be at the point used in drawing routines.
	/// </summary>
	public const int LEFT = 0x1;
	/// <summary>
	/// The horizontal center of the text will be at the point used in drawing routines.
	/// </summary>
	public const int CENTER_X = 0x2;
	/// <summary>
	/// The right edge of the text will be at the point used in drawing routines.
	/// </summary>
	public const int RIGHT = 0x4;
	/// <summary>
	/// The bottom edge of the text will be at the point used in drawing routines.
	/// </summary>
	public const int BOTTOM = 0x8;
	/// <summary>
	/// The top edge of the text will be at the point used in drawing routines.
	/// </summary>
	public const int TOP = 0x10;
	/// <summary>
	/// The vertical middle center of the text will be at the point used in drawing routines.
	/// </summary>
	public const int CENTER_Y = 0x20;

	/// <summary>
	/// For axis labels - shift ends so that they are not centered.
	/// </summary>
	public const int SHIFT_ENDS = 0x40;
	/// <summary>
	/// Labels are coming in with the bottom or right one last.
	/// </summary>
	public const int REVERSE_LABELS = 0x80;
	/// <summary>
	/// Put in for now - needs to be completed.
	/// </summary>
	public const int SHIFT_ENDS_INVERTED = 0x100;

	/// <summary>
	/// Return available text positions.  The text positions are suitable for
	/// positioning relative to a point, as follows:
	/// <pre>
	/// UpperLeft |  Above | UpperRight
	/// --------------------------------
	///     Left | Center | Right
	/// --------------------------------
	/// LowerLeft | Below  | LowerRight
	/// </pre> </summary>
	/// <returns> a String array containing possible text positions (e.g., "UpperRight").
	/// These strings can be used for properties for maps, time series plots, etc. </returns>
	public static string[] getTextPositions()
	{
		string[] positions = new string[9];
		positions[0] = "AboveCenter";
		positions[1] = "BelowCenter";
		positions[2] = "Center";
		positions[3] = "Left";
		positions[4] = "LowerLeft";
		positions[5] = "LowerRight";
		positions[6] = "Right";
		positions[7] = "UpperLeft";
		positions[8] = "UpperRight";
		return positions;
	}

	/// <summary>
	/// Parse a text position and return the integer equivalent. </summary>
	/// <param name="position"> Position for text, corresponding to a value returned from
	/// getTextPositions (e.g., "UpperRight"). </param>
	/// <returns> the integer equivalent of a text position. </returns>
	/// <exception cref="Exception"> if the position is not recognized.  In this case the
	/// calling code should probably use a reasonable default like LEFT|CENTER_Y. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int parseTextPosition(String position) throws Exception
	public static int parseTextPosition(string position)
	{
		if (position.Equals("AboveCenter", StringComparison.OrdinalIgnoreCase))
		{
			return BOTTOM | CENTER_X;
		}
		else if (position.Equals("BelowCenter", StringComparison.OrdinalIgnoreCase))
		{
			return TOP | CENTER_X;
		}
		else if (position.Equals("Center", StringComparison.OrdinalIgnoreCase))
		{
			return CENTER_Y | CENTER_X;
		}
		else if (position.Equals("Left", StringComparison.OrdinalIgnoreCase))
		{
			return RIGHT | CENTER_Y;
		}
		else if (position.Equals("LeftCenter", StringComparison.OrdinalIgnoreCase))
		{
			Message.printWarning(2, "", "Label position LeftCenter is obsolete - change to Left");
			return RIGHT | CENTER_Y;
		}
		else if (position.Equals("LowerLeft", StringComparison.OrdinalIgnoreCase))
		{
			return RIGHT | TOP;
		}
		else if (position.Equals("LowerRight", StringComparison.OrdinalIgnoreCase))
		{
			return LEFT | TOP;
		}
		else if (position.Equals("Right", StringComparison.OrdinalIgnoreCase))
		{
			return LEFT | CENTER_Y;
		}
		else if (position.Equals("RightCenter", StringComparison.OrdinalIgnoreCase))
		{
			Message.printWarning(2, "", "Label position RightCenter is obsolete - change to Right");
			return LEFT | CENTER_Y;
		}
		else if (position.Equals("UpperLeft", StringComparison.OrdinalIgnoreCase))
		{
			return RIGHT | BOTTOM;
		}
		else if (position.Equals("UpperRight", StringComparison.OrdinalIgnoreCase))
		{
			return LEFT | BOTTOM;
		}
		else
		{
			throw new Exception("Unknown text position \"" + position + "\"");
		}
	}

	/// <summary>
	/// Return String corresponding to position information. </summary>
	/// <param name="position"> Combination of position bit mask values. </param>
	/// <returns> String corresponding to position information. </returns>
	public static string ToString(int position)
	{
		if (((position & CENTER_X) != 0) && ((position & BOTTOM) != 0))
		{
			return "AboveCenter";
		}
		else if (((position & CENTER_X) != 0) && ((position & TOP) != 0))
		{
			return "BelowCenter";
		}
		else if (((position & CENTER_X) != 0) && ((position & CENTER_Y) != 0))
		{
			return "Center";
		}
		else if (((position & RIGHT) != 0) && ((position & CENTER_Y) != 0))
		{
			return "Left";
		}
		else if (((position & TOP) != 0) && ((position & RIGHT) != 0))
		{
			return "LowerLeft";
		}
		else if (((position & TOP) != 0) && ((position & LEFT) != 0))
		{
			return "LowerRight";
		}
		else if (((position & LEFT) != 0) && ((position & CENTER_Y) != 0))
		{
			return "Right";
		}
		else if (((position & BOTTOM) != 0) && ((position & LEFT) != 0))
		{
			return "UpperRight";
		}
		else if (((position & BOTTOM) != 0) && ((position & RIGHT) != 0))
		{
			return "UpperLeft";
		}
		else
		{
			return "Center";
		}
	}

	}

}