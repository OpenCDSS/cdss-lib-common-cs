using System;

// JWorksheet_CellAttributes - class for storing cell attributes for JWorksheet cells

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
// JWorksheet_CellAttributes - Class for storing cell attributes for JWorksheet
//	cells.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2003-07-03	J. Thomas Sapienza, RTi	Initial version.
// 2003-10-07	JTS, RTi		Added the editable flag.
// 2003-10-20	JTS, RTi		Added toString().
// 2003-10-21	JTS, RTi		* Implemented Cloneable.
//					* Added copy constructor.
// 2003-11-18	JTS, RTi		Added finalize().
// ----------------------------------------------------------------------------

namespace RTi.Util.GUI
{

	/// <summary>
	/// This class stores attributes for formatting JWorksheet cells.  All member 
	/// variables are public for speed's sake (this class could be reference many times
	/// when rendering a JWorksheet).<para>
	/// Variables that are not set will not affect a cell's rendering.  To disable
	/// an attribute in a renderered cell, set it to <pre>null</pre> if it is an 
	/// object or <pre>-1</pre> if it is an <pre>int</pre>.
	/// </para>
	/// </summary>
	public class JWorksheet_CellAttributes : ICloneable
	{

	/// <summary>
	/// Cell background color.
	/// </summary>
	public Color backgroundColor = null;

	/// <summary>
	/// Cell background color when selected.
	/// </summary>
	public Color backgroundSelectedColor = null;

	/// <summary>
	/// Cell border color.
	/// </summary>
	public Color borderColor = null;

	/// <summary>
	/// Whether the cell is editable or not.
	/// </summary>
	public bool editable = true;

	/// <summary>
	/// Whether the cell attributes are enabled (true) or not.  By default the 
	/// attributes are enabled, though they can be turned off for a cell by getting
	/// the attributes for a certain cell and setting enabled to false;
	/// </summary>
	public bool enabled = true;

	/// <summary>
	/// Cell foreground color.
	/// </summary>
	public Color foregroundColor = null;

	/// <summary>
	/// Cell foreground color when selected.
	/// </summary>
	public Color foregroundSelectedColor = null;

	/// <summary>
	/// Font in which to render the cell.
	/// </summary>
	public Font font = null;

	/// <summary>
	/// The name of the font to display in.  
	/// If the font is not set from the <pre>font</pre> member variable this is 
	/// the name of the font to use.
	/// </summary>
	public string fontName = null;

	/// <summary>
	/// The size of the font to display in.
	/// If the font is not set from the <pre>font</pre> member variable this is 
	/// the size of the font to use.
	/// </summary>
	public int fontSize = -1;

	/// <summary>
	/// The style of the font to display in.
	/// If the font is not set from the <pre>font</pre> member variable this is 
	/// the style of the font to use (Font.PLAIN, Font.BOLD, etc.).
	/// </summary>
	public int fontStyle = -1;

	/// <summary>
	/// The horizontal alignment of text in the cell, from SwingConstants.
	/// </summary>
	public int horizontalAlignment = -1;

	/// <summary>
	/// The vertical alignment of text in the cell, from SwingConstants.
	/// </summary>
	public int verticalAlignment = -1;

	/// <summary>
	/// Constructor.
	/// </summary>
	public JWorksheet_CellAttributes()
	{
	}

	/// <summary>
	/// Copy constructor. </summary>
	/// <param name="ca"> the cell attributes to copy into this one. </param>
	public JWorksheet_CellAttributes(JWorksheet_CellAttributes ca)
	{
		backgroundColor = ca.backgroundColor;
		backgroundSelectedColor = ca.backgroundSelectedColor;
		foregroundColor = ca.foregroundColor;
		foregroundSelectedColor = ca.foregroundSelectedColor;
		borderColor = ca.borderColor;
		font = ca.font;
		fontName = ca.fontName;
		fontSize = ca.fontSize;
		fontStyle = ca.fontStyle;
		horizontalAlignment = ca.horizontalAlignment;
		verticalAlignment = ca.verticalAlignment;
		enabled = ca.enabled;
		editable = ca.editable;
	}

	/// <summary>
	/// Clones the cell attributes.
	/// </summary>
	public virtual object clone()
	{
		try
		{
			base.clone();
		}
		catch (CloneNotSupportedException)
		{
		}
		return this;
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~JWorksheet_CellAttributes()
	{
		backgroundColor = null;
		backgroundSelectedColor = null;
		borderColor = null;
		foregroundColor = null;
		foregroundSelectedColor = null;
		font = null;
		fontName = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns a String representation of these attributes. </summary>
	/// <returns> a String representation of these attributes. </returns>
	public override string ToString()
	{
		return "backgroundColor:         " + backgroundColor + "\n" +
			"backgroundSelectedColor: " + backgroundSelectedColor + "\n" +
			"foregroundColor:         " + foregroundColor + "\n" +
			"foregroundSelectedColor: " + foregroundSelectedColor + "\n" +
			"borderColor:             " + borderColor + "\n" +
			"font:                    " + font + "\n" +
			"fontName:                " + fontName + "\n" +
			"fontSize:                " + fontSize + "\n" +
			"fontStyle:               " + fontStyle + "\n" +
			"horizontalAlignment:     " + horizontalAlignment + "\n" +
			"verticalAlignment:       " + verticalAlignment + "\n" +
			"enabled:                 " + enabled + "\n" +
			"editable:                " + editable + "\n";
	}

	}

}