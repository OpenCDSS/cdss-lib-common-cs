﻿using System;
using System.Collections.Generic;

// GRColorTable - class to store list of colors for a classification

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
// GRColorTable - class to store list of colors for a classification
// ----------------------------------------------------------------------------
// History:
//
// 2001-09-17	Steven A. Malers, RTi	Initial version to support GIS feature
//					classification.
// 2001-09-21	SAM, RTi		Change to extend from Vector.
// 2001-10-10	SAM, RTi		Add toString(int) and getName().
// 2001-12-02	SAM, RTi		Overload constructor to take no
//					arguments.
// 2004-10-27	J. Thomas Sapienza, RTi	Implements Cloneable.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.GR
{

	/// <summary>
	/// Class to store a color table.  This can be used, for example, to organize
	/// colors for a legend for a map layer.  This class uses a Vector of GRColor and
	/// internal arrays to associate the colors with numbers and data values.
	/// TODO (JTS - 2006-05-23) Examples of use
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class GRColorTable extends java.util.Vector implements Cloneable
	public class GRColorTable : List<object>, ICloneable
	{

	/// <summary>
	/// Gray color table.
	/// </summary>
	public const int GRAY = 0;
	/// <summary>
	/// Blue to cyan color gradient.
	/// </summary>
	public const int BLUE_TO_CYAN = 1;
	/// <summary>
	/// Cyan to magenta color gradient.
	/// </summary>
	public const int BLUE_TO_MAGENTA = 2;
	/// <summary>
	/// Blue to red color gradient.
	/// </summary>
	public const int BLUE_TO_RED = 3;
	/// <summary>
	/// Cyan to yellow color gradient.
	/// </summary>
	public const int CYAN_TO_YELLOW = 4;
	/// <summary>
	/// Magenta to cyan color gradient.
	/// </summary>
	public const int MAGENTA_TO_CYAN = 5;
	/// <summary>
	/// Magenta to red color gradient.
	/// </summary>
	public const int MAGENTA_TO_RED = 6;
	/// <summary>
	/// Yellow to magenta color gradient.
	/// </summary>
	public const int YELLOW_TO_MAGENTA = 7;
	/// <summary>
	/// Yellow to red color gradient.
	/// </summary>
	public const int YELLOW_TO_RED = 8;

	/// <summary>
	/// Color table names.
	/// </summary>
	public static string[] COLOR_TABLE_NAMES = new string[] {"Gray", "BlueToCyan", "BlueToMagenta", "BlueToRed", "CyanToYellow", "MagentaToCyan", "MagentaToRed", "YellowToMagenta", "YellowToRed"};

	/// <summary>
	/// Name for the color table.  Use this, for example if the color table is
	/// not one of the standard color tables.
	/// </summary>
	protected internal string _name = "";

	/// <summary>
	/// Create a color table.
	/// </summary>
	public GRColorTable() : base(1)
	{
	}

	/// <summary>
	/// Create a color table. </summary>
	/// <param name="size"> The size hint allows for optimal initial sizing of the
	/// GRColor Vector.  If a likely size is not know, specify 0. </param>
	public GRColorTable(int size) : base(size)
	{
	}

	/// <summary>
	/// Add a color to the color table.  The data value associated with the value is
	/// set to zero. </summary>
	/// <param name="color"> Color to add to the color table. </param>
	public virtual void addColor(GRColor color)
	{
		this.Add(color);
	}

	/// <summary>
	/// Clones the Object. </summary>
	/// <returns> a Clone of the Object. </returns>
	public virtual object clone()
	{
		GRColorTable t = null;
		try
		{
			t = (GRColorTable)base.clone();
		}
		catch (Exception)
		{
			return null;
		}

		return t;
	}

	/// <summary>
	/// Create a color table using one of the standard GR named color tables.
	/// Null is returned if the color table name does not match a known name.
	/// See the overloaded method for more information. </summary>
	/// <param name="table_name"> Color table name (see COLOR_TABLE_NAME.*). </param>
	/// <param name="ncolors">	Number of colors to be in color table. </param>
	/// <param name="rflag"> Indicates whether colors should be reversed (true) or left
	/// in the initial order (0) (this feature makes it so only one versions of each
	/// standard table is defined). </param>
	/// <returns> a new GRColorTable for the table name, or null if unable to match the color table name. </returns>
	public static GRColorTable createColorTable(string table_name, int ncolors, bool rflag)
	{
		for (int i = 0; i < COLOR_TABLE_NAMES.Length; i++)
		{
			if (COLOR_TABLE_NAMES[i].Equals(table_name, StringComparison.OrdinalIgnoreCase))
			{
				return createColorTable(i, ncolors, rflag);
			}
		}
		return null;
	}

	/// <summary>
	/// Create a color table using one of the standard GR named color tables.
	/// This method forms colors using RGB values as floating point numbers in the range
	/// 0.0 to 1.0.  The floating point values are then converted to GRColor instances.
	/// For single color families (e.g., shades of blue), the table is centered on the
	/// color and is shaded on each side.  For multi-color families (e.g., several prime
	/// colors), each section of the table is centered on a prime color, with shades on each side.
	/// Colors returned for multi-color families are hard-coded for color requests less
	/// than the minimum for the table.  This ensures that roundoff error, etc., will
	/// not return bogus colors.  Do the hard-coding by filling in the end-colors first
	/// and then filling in the middle.  This ensures that a relatively nice gradation is used.
	/// Additionally, in some cases the relationship (iend - ibeg) is zero.  In these
	/// cases, skip the next blend operation (only assign the main color in the section
	/// and go on to the next.  This generally occurs when the number of colors
	/// requested is &gt;5 but still low enough that integer math can result in some
	/// stretches of the table being ignored. </summary>
	/// <param name="table_num"> Color table to be used (e.g., GRColorTable.GRAY). </param>
	/// <param name="ncolors">	Number of colors to be in color table. </param>
	/// <param name="rflag"> Indicates whether colors should be reversed (true) or left
	/// in the initial order (0) (this feature makes it so only one versions of each standard table is defined). </param>
	/// <returns> GRColorTable with number of requested colors or null if not able to create the color table. </returns>
	public static GRColorTable createColorTable(int table_num, int ncolors, bool rflag)
	{
		double drgb; // Color increment to be applied when varying shades.
		double[] r = null;
		double[] g = null;
		double[] b = null;
		int i, ibeg, iend;

		if (ncolors == 0)
		{
			return null;
		}
		r = new double[ncolors];
		g = new double[ncolors];
		b = new double[ncolors];
		if (table_num == MAGENTA_TO_CYAN)
		{
			// Magenta to blue to cyan...
			r[0] = 1.0;
			g[0] = 0.0; // magenta
			b[0] = 1.0;
			ibeg = 1;
			iend = ncolors / 2;
			if (iend > ibeg)
			{
				drgb = 1.0 / (iend - ibeg);
				for (i = 1; i < iend; i++)
				{
					r[i] = r[i - 1] - drgb;
					g[i] = 0.0;
					b[i] = 1.0;
				} // blue
			}
			ibeg = iend;
			iend = ncolors;
			if (iend > ibeg)
			{
				drgb = 1.0 / (iend - ibeg);
				for (i = ibeg; i < iend; i++)
				{
					b[i] = 1.0;
					r[i] = 0.0;
					g[i] = g[i - 1] + drgb;
				} // cyan
			}
		}
		else if (table_num == BLUE_TO_CYAN)
		{
			// blue to cyan
			r[0] = 0.0;
			g[0] = 0.0; // blue
			b[0] = 1.0;
			ibeg = 1;
			iend = ncolors;
			if (iend > ibeg)
			{
				drgb = 1.0 / (iend - ibeg);
				for (i = ibeg; i < iend; i++)
				{
					b[i] = 1.0;
					r[i] = 0.0;
					g[i] = g[i - 1] + drgb;
				} // cyan
			}
		}
		else if (table_num == CYAN_TO_YELLOW)
		{
			// Only green hues...
			r[0] = 0.0;
			g[0] = 1.0; // cyan
			b[0] = 1.0;
			ibeg = 1;
			iend = ncolors / 2;
			if (iend > ibeg)
			{
				drgb = 1.0 / (iend - ibeg);
				for (i = 1; i < iend; i++)
				{
					r[i] = 0.0;
					g[i] = 1.0;
					b[i] = b[i - 1] - drgb;
				} // green
			}
			ibeg = iend;
			iend = ncolors;
			if (iend > ibeg)
			{
				drgb = 1.0 / (iend - ibeg);
				for (i = ibeg; i < iend; i++)
				{
					r[i] = r[i - 1] + drgb;
					g[i] = 1.0;
					b[i] = 0.0;
				} // yellow
			}
		}
		else if (table_num == YELLOW_TO_RED)
		{
			// yellow to red...
			r[0] = 1.0;
			g[0] = 1.0; // yellow
			b[0] = 0.0;
			ibeg = 1;
			iend = ncolors;
			if (iend > ibeg)
			{
				drgb = 1.0 / (iend - ibeg);
				for (i = 1; i < iend; i++)
				{
					r[i] = 1.0;
					g[i] = g[i - 1] - drgb;
					b[i] = 0.0;
				} // red
			}
		}
		else if (table_num == YELLOW_TO_MAGENTA)
		{
			// Only red hues
			r[0] = 1.0;
			g[0] = 1.0; // yellow
			b[0] = 0.0;
			ibeg = 1;
			iend = ncolors / 2;
			if (iend > ibeg)
			{
				drgb = 1.0 / (iend - ibeg);
				for (i = 1; i < iend; i++)
				{
					r[i] = 1.0;
					g[i] = g[i - 1] - drgb;
					b[i] = 0.0;
				} // red
			}
			ibeg = iend;
			iend = ncolors;
			if (iend > ibeg)
			{
				drgb = 1.0 / (iend - ibeg);
				for (i = ibeg; i < iend; i++)
				{
					r[i] = 1.0;
					g[i] = 0.0;
					b[i] = b[i - 1] + drgb;
				} // magenta
			}
		}
		else if (table_num == BLUE_TO_RED)
		{
			// No magenta, white, or black
			//
			// Set the colors manually for requests for less than
			// five colors...
			if ((ncolors >= 1) && (ncolors <= 4))
			{
				// blue
				r[0] = 0.0;
				g[0] = 0.0;
				b[0] = 1.0;
			}
			if ((ncolors >= 2) && (ncolors <= 4))
			{
				// add red...
				r[ncolors - 1] = 1.0;
				g[ncolors - 1] = 0.0;
				b[ncolors - 1] = 0.0;
			}
			if (ncolors == 3)
			{
				// add green...
				r[2] = 0.0;
				g[2] = 1.0;
				b[2] = 0.0;
			}
			else if (ncolors == 4)
			{
				// add cyan, yellow
				r[2] = 0.0; // no green
				g[2] = 1.0;
				b[2] = 1.0;
				r[3] = 1.0;
				g[3] = 1.0;
				b[3] = 0.0;
			}
			if (ncolors >= 5)
			{
				// Interpolate between 5 known colors (the number of
				// colors to be used because some colors to not be
				// well represented...
				r[0] = 0.0;
				g[0] = 0.0;
				b[0] = 1.0; // First color always blue
				ibeg = 1;
				iend = ncolors / 4;
				if ((iend - ibeg) == 0)
				{
					// Not enough colors to put shades so just
					// stick in cyan...
					r[ibeg] = 0.0;
					g[ibeg] = 1.0;
					b[ibeg] = 1.0;
				} // cyan
				else
				{ // Do some shades...
					drgb = 1.0 / (iend - ibeg);
					for (i = ibeg; i < iend; i++)
					{
						r[i] = 0.0;
						g[i] = g[i - 1] + drgb;
						b[i] = 1.0;
					} // cyan
				}
				ibeg = iend;
				iend = ncolors * 2 / 4;
				if ((iend - ibeg) == 0)
				{
					// Not enough colors to put shades so just
					// stick in green...
					r[ibeg] = 0.0;
					g[ibeg] = 1.0;
					b[ibeg] = 0.0;
				} // green
				else
				{ // Do some shades...
					drgb = 1.0 / (iend - ibeg);
					for (i = ibeg; i < iend; i++)
					{
						r[i] = 0.0;
						g[i] = 1.0;
						b[i] = b[i - 1] - drgb;
					} // green
				}
				ibeg = iend;
				iend = ncolors * 3 / 4;
				if ((iend - ibeg) == 0)
				{
					// Not enough colors to put shades so just
					// stick in yellow...
					r[ibeg] = 1.0;
					g[ibeg] = 1.0;
					b[ibeg] = 0.0;
				} // yellow
				else
				{ // Do some shades...
					drgb = 1.0 / (iend - ibeg);
					for (i = ibeg; i < iend; i++)
					{
						r[i] = r[i - 1] + drgb;
						g[i] = 1.0;
						b[i] = 0.0;
					} // yellow
				}
				ibeg = iend;
				iend = ncolors;
				if ((iend - ibeg) == 0)
				{
					// Not enough colors to put shades so just
					// stick in red...
					r[ibeg] = 1.0;
					g[ibeg] = 1.0;
					b[ibeg] = 0.0;
				} // red
				else
				{ // Do some shades...
					drgb = 1.0 / (iend - ibeg);
					for (i = ibeg; i < iend; i++)
					{
						r[i] = 1.0;
						g[i] = g[i - 1] - drgb;
						b[i] = 0.0;
					} // red
				}
			}
		}
		else if (table_num == MAGENTA_TO_RED)
		{
			// Set the colors manually for requests for less than
			// five colors...
			if ((ncolors >= 1) && (ncolors <= 4))
			{ // magenta
				r[0] = 1.0;
				g[0] = 0.0;
				b[0] = 1.0;
			}
			if ((ncolors >= 2) && (ncolors <= 4))
			{ // red
				r[ncolors - 1] = 1.0;
				g[ncolors - 1] = 0.0;
				b[ncolors - 1] = 0.0;
			}
			if (ncolors == 3)
			{ // green
				r[2] = 0.0;
				g[2] = 1.0;
				b[2] = 0.0;
			}
			else if (ncolors == 4)
			{ // add cyan, yellow
				r[2] = 0.0; // no green
				g[2] = 1.0;
				b[2] = 1.0;
				r[3] = 1.0;
				g[3] = 1.0;
				b[3] = 0.0;
			}
			if (ncolors >= 5)
			{
				r[0] = 1.0;
				g[0] = 0.0;
				b[0] = 1.0; // First color always magenta
				ibeg = 1;
				iend = ncolors / 5;
				if ((iend - ibeg) == 0)
				{
					// Not enough colors to put shades so just
					// stick in blue...
					r[iend] = 0.0;
					g[iend] = 0.0;
					b[iend] = 1.0;
				} // blue
				else
				{ // Put in shades...
					drgb = 1.0 / (iend - ibeg);
					for (i = ibeg; i < iend; i++)
					{
						r[i] = r[i - 1] - drgb;
						g[i] = 0.0;
						b[i] = 1.0;
					}
				} // blue
				ibeg = iend;
				iend = ncolors * 2 / 5;
				if ((iend - ibeg) == 0)
				{
					// Not enough colors to put shades so just
					// stick in cyan...
					r[iend] = 0.0;
					g[iend] = 1.0;
					b[iend] = 1.0;
				} // cyan
				else
				{ // Put in shades...
					drgb = 1.0 / (iend - ibeg);
					for (i = ibeg; i < iend; i++)
					{
						r[i] = 0.0;
						g[i] = g[i - 1] + drgb;
						b[i] = 1.0;
					} // cyan
				}
				ibeg = iend;
				iend = ncolors * 3 / 5;
				if ((iend - ibeg) == 0)
				{
					// Not enough colors to put shades so just
					// stick in green...
					r[iend] = 0.0;
					g[iend] = 1.0;
					b[iend] = 0.0;
				} // green
				else
				{ // Put in shades...
					drgb = 1.0 / (iend - ibeg);
					for (i = ibeg; i < iend; i++)
					{
						r[i] = 0.0;
						g[i] = 1.0;
						b[i] = b[i - 1] - drgb;
					} // green
				}
				ibeg = iend;
				iend = ncolors * 4 / 5;
				if ((iend - ibeg) == 0)
				{
					// Not enough colors to put shades so just
					// stick in yellow...
					r[iend] = 1.0;
					g[iend] = 1.0;
					b[iend] = 0.0;
				} // yellow
				else
				{ // Put in shades...
					drgb = 1.0 / (iend - ibeg);
					for (i = ibeg; i < iend; i++)
					{
						r[i] = r[i - 1] + drgb;
						g[i] = 1.0;
						b[i] = 0.0;
					} // yellow
				}
				ibeg = iend;
				iend = ncolors;
				if ((iend - ibeg) == 0)
				{
					// Not enough colors to put shades so just
					// stick in red...
					r[iend] = 1.0;
					g[iend] = 0.0;
					b[iend] = 0.0;
				} // red
				else
				{ // Put in shades...
					drgb = 1.0 / (iend - ibeg);
					for (i = ibeg; i < iend; i++)
					{
						r[i] = 1.0;
						g[i] = g[i - 1] - drgb;
						b[i] = 0.0;
					} // red
				}
			}
		}
		else if (table_num == BLUE_TO_MAGENTA)
		{
			// Set the colors manually for requests for less than
			// five colors...
			if ((ncolors >= 1) && (ncolors <= 4))
			{ // blue
				r[0] = 0.0;
				g[0] = 0.0;
				b[0] = 1.0;
			}
			if ((ncolors >= 2) && (ncolors <= 4))
			{ // add magenta
				r[ncolors - 1] = 1.0;
				g[ncolors - 1] = 0.0;
				b[ncolors - 1] = 1.0;
			}
			if (ncolors == 3)
			{ // add green
				r[2] = 0.0;
				g[2] = 1.0;
				b[2] = 0.0;
			}
			else if (ncolors == 4)
			{ // add cyan, yellow
				r[2] = 0.0; // no green
				g[2] = 1.0;
				b[2] = 1.0;
				r[3] = 1.0;
				g[3] = 1.0;
				b[3] = 0.0;
			}
			if (ncolors >= 5)
			{
				r[0] = 0.0;
				g[0] = 0.0;
				b[0] = 1.0; // First color always blue
				ibeg = 1;
				iend = ncolors / 5;
				if ((iend - ibeg) == 0)
				{
					// Not enough colors to put shades so just
					// stick in cyan...
					r[iend] = 0.0;
					g[iend] = 1.0;
					b[iend] = 1.0;
				} // cyan
				else
				{ // Put in shades...
					drgb = 1.0 / (iend - ibeg);
					for (i = ibeg; i < iend; i++)
					{
						r[i] = 0.0;
						g[i] = g[i - 1] + drgb;
						b[i] = 1.0;
					}
				} // cyan
				ibeg = iend;
				iend = ncolors * 2 / 5;
				if ((iend - ibeg) == 0)
				{
					// Not enough colors to put shades so just
					// stick in green...
					r[iend] = 0.0;
					g[iend] = 1.0;
					b[iend] = 0.0;
				} // green
				else
				{ // Put in shades...
					drgb = 1.0 / (iend - ibeg);
					for (i = ibeg; i < iend; i++)
					{
						r[i] = 0.0;
						g[i] = 1.0;
						b[i] = b[i - 1] - drgb;
					}
				} // green
				ibeg = iend;
				iend = ncolors * 3 / 5;
				if ((iend - ibeg) == 0)
				{
					// Not enough colors to put shades so just
					// stick in yellow...
					r[iend] = 1.0;
					g[iend] = 1.0;
					b[iend] = 0.0;
				} // yellow
				else
				{ // Put in shades...
					drgb = 1.0 / (iend - ibeg);
					for (i = ibeg; i < iend; i++)
					{
						r[i] = r[i - 1] + drgb;
						g[i] = 1.0;
						b[i] = 0.0;
					}
				} // yellow
				ibeg = iend;
				iend = ncolors * 4 / 5;
				if ((iend - ibeg) == 0)
				{
					// Not enough colors to put shades so just
					// stick in red...
					r[iend] = 1.0;
					g[iend] = 0.0;
					b[iend] = 0.0;
				} // red
				else
				{ // Put in shades...
					drgb = 1.0 / (iend - ibeg);
					for (i = ibeg; i < iend; i++)
					{
						r[i] = 1.0;
						g[i] = g[i - 1] - drgb;
						b[i] = 0.0;
					}
				} // red
				ibeg = iend;
				iend = ncolors;
				if ((iend - ibeg) == 0)
				{
					// Not enough colors to put shades so just
					// stick in magenta...
					r[iend] = 1.0;
					g[iend] = 0.0;
					b[iend] = 1.0;
				} // magenta
				else
				{ // Put in shades...
					drgb = 1.0 / (iend - ibeg);
					for (i = ibeg; i < iend; i++)
					{
						r[i] = 1.0;
						g[i] = 0.0;
						b[i] = b[i - 1] + drgb;
					} // magenta
				}
			}
		}
		else if (table_num == GRAY)
		{
			// Gray hues...
			r[0] = 0.0;
			g[0] = 0.0;
			b[0] = 0.0;
			drgb = 1.0 / (ncolors - 1);
			for (i = 1; i < ncolors; i++)
			{
				r[i] = r[i - 1] + drgb;
				g[i] = g[i - 1] + drgb;
				b[i] = b[i - 1] + drgb;
			}
		}
	//	for ( i = 0; i < ncolors; i++ ) {
	//		//Message.printDebug ( dl, "", "Color[" + i + "]=(" +
	//		Message.printStatus ( 1, "", "Color[" + i + "]=(" +
	//		r[i] + "," + g[i] + "," + b[i] + ")" );
	//	}
		if (rflag)
		{
			// Reverse order of colors...
			double[] r2 = new double[ncolors];
			double[] g2 = new double[ncolors];
			double[] b2 = new double[ncolors];
			for (i = 0; i < ncolors; i++)
			{
				r2[i] = r[ncolors - i - 1];
				g2[i] = g[ncolors - i - 1];
				b2[i] = b[ncolors - i - 1];
			}
			for (i = 0; i < ncolors; i++)
			{
				r[i] = r2[i];
				g[i] = g2[i];
				b[i] = b2[i];
			}
			r2 = null;
			g2 = null;
			b2 = null;
		}
		// Now create new GRColor...
		GRColorTable table = new GRColorTable(ncolors);
		for (i = 0; i < ncolors; i++)
		{
			// Check roundoff...
			if (r[i] < 0.0)
			{
				r[i] = 0.0;
			}
			if (g[i] < 0.0)
			{
				g[i] = 0.0;
			}
			if (b[i] < 0.0)
			{
				b[i] = 0.0;
			}
			if (r[i] > 1.0)
			{
				r[i] = 1.0;
			}
			if (g[i] > 1.0)
			{
				g[i] = 1.0;
			}
			if (b[i] > 1.0)
			{
				b[i] = 1.0;
			}
			table.addColor(new GRColor(r[i], g[i], b[i]));
		}
		table.setName(COLOR_TABLE_NAMES[table_num]);
		return table;
	}

	/// <summary>
	/// Finalize before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~GRColorTable()
	{
		_name = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Get the color at an index.
	/// </summary>
	public virtual GRColor get(int index)
	{
		return (GRColor)this[index];
	}

	/// <summary>
	/// Return the color table name. </summary>
	/// <returns> the color table name. </returns>
	public virtual string getName()
	{
		return _name;
	}

	/// <summary>
	/// Set the color table name. </summary>
	/// <param name="name"> Color table name. </param>
	public virtual void setName(string name)
	{
		if (!string.ReferenceEquals(name, null))
		{
			_name = name;
		}
	}

	/// <summary>
	/// Convert the color table integer to its string name. </summary>
	/// <param name="color_table"> Internal color table number. </param>
	/// <returns> String name of color table or "Unknown" if not found. </returns>
	public static string ToString(int color_table)
	{
		if ((color_table >= GRAY) && (color_table <= YELLOW_TO_RED))
		{
			return COLOR_TABLE_NAMES[color_table];
		}
		return "Unknown";
	}

	}

}