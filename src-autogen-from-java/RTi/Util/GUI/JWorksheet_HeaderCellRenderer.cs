using System;
using System.Collections.Generic;
using System.IO;

// JWorksheet_HeaderCellRenderer - class to use as the cell renderer for the header of the JWorksheet

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
// JWorksheet_HeaderCellRenderer - Class to use as the cell renderer for
//	 the header of the JWorksheet, in order to set fonts.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2003-03-11	J. Thomas Sapienza, RTi	Initial version.
// 2003-03-20	JTS, RTi		Revised after SAM's review.
// 2003-07-01	JTS, RTi		Added code for doing multi-line headers.
// 2003-09-05	JTS, RTi		Multiline headers now use the proper
//					header font settings.
// 2003-10-09	JTS, RTi		Empty lines ('') were not being placed
//					into the lists for some reason, so they
//					are now spaced out to '      '.
// 2003-11-18	JTS, RTi		Added finalize().
// ----------------------------------------------------------------------------

namespace RTi.Util.GUI
{
	// TODO (JTS - 2004-01-20) needs renamed to JWorksheet_ColumnHeaderCellRenderer
	// need to be able to center text in the column header





	/// <summary>
	/// Class to use as cell renderer for the header of the JWorksheet, in order to
	/// be able to set the header fonts.
	/// </summary>
	public class JWorksheet_HeaderCellRenderer : DefaultTableCellRenderer
	{

	/// <summary>
	/// Whether the header can be rendered as a multiple-line header.
	/// </summary>
	private bool __multiline = false;

	/// <summary>
	/// The background color in which to display the header.
	/// </summary>
	private Color __color;

	/// <summary>
	/// The Font in which to display the header.
	/// </summary>
	private Font __headerFont;

	/// <summary>
	/// The size of the font in which to display the header.
	/// </summary>
	private int __fontSize;

	/// <summary>
	/// The style of the font in which to display the header.
	/// </summary>
	private int __fontStyle;

	/// <summary>
	/// The name of the font to display header text in.
	/// </summary>
	private string __fontName = null;

	/// <summary>
	/// The justification to apply to the header text.
	/// </summary>
	private int __justification = SwingConstants.CENTER;

	/// <summary>
	/// Constructor.  Builds a default renderer with Arial 12-point plain as the font,
	/// and justification of CENTER.
	/// </summary>
	public JWorksheet_HeaderCellRenderer()
	{
		__fontName = "Arial";
		__fontStyle = Font.PLAIN;
		__fontSize = 11;
		__justification = SwingConstants.CENTER;
		JTableHeader header = new JTableHeader();
		__color = (Color)(header.getClientProperty("TableHeader.background"));
		__headerFont = new Font(__fontName, __fontStyle, __fontSize);
	}

	/// <summary>
	/// Constructor.  Builds a renderer for the header with the given font and the 
	/// given header text justification (as defined in SwingConstants). </summary>
	/// <param name="fontName"> the name of the font for the header (e.g., "Courer") </param>
	/// <param name="fontStyle"> the style of the header's font (e.g., Font.PLAIN) </param>
	/// <param name="fontSize"> the size of the header font (e.g, 11) </param>
	/// <param name="justification"> the justification (CENTER, RIGHT, or LEFT) in which 
	/// to display the header text.  (e.g., SwingConstants.CENTER) </param>
	/// <param name="color"> the color for the header font (e.g., Color.LIGHT_GRAY) </param>
	public JWorksheet_HeaderCellRenderer(string fontName, int fontStyle, int fontSize, int justification, Color color)
	{
		__fontName = fontName;
		__fontStyle = fontStyle;
		__fontSize = fontSize;
		__justification = justification;
		if (color == null)
		{
			JTableHeader header = new JTableHeader();
			__color = (Color)(header.getClientProperty("TableHeader.background"));
		}
		else
		{
			__color = color;
		}
		__headerFont = new Font(__fontName, __fontStyle, __fontSize);
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~JWorksheet_HeaderCellRenderer()
	{
		__color = null;
		__headerFont = null;
		__fontName = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the font in which the header cells should be rendered. </summary>
	/// <returns> the font in which the header cells should be rendered. </returns>
	public virtual Font getFont()
	{
		//return new Font(__fontName, __fontStyle, __fontSize);
		return __headerFont;
	}

	/// <summary>
	/// Renders a value for a cell in a JTable.  This method is called automatically
	/// by the JTable when it is rendering its cells.  This overrides the method in
	/// DefaultTableCellRenderer. </summary>
	/// <param name="table"> the JTable (in this case, JWorksheet) in which the cell
	/// to be rendered will appear. </param>
	/// <param name="value"> the cell's value to be rendered. </param>
	/// <param name="isSelected"> whether the cell is selected or not. </param>
	/// <param name="hasFocus"> whether the cell has focus or not. </param>
	/// <param name="row"> the row in which the cell appears. </param>
	/// <param name="column"> the column in which the cell appears. </param>
	/// <returns> a properly-rendered cell that can be placed in the table. </returns>
	public virtual Component getTableCellRendererComponent(JTable table, object value, bool isSelected, bool hasFocus, int row, int column)
	{
		string str = "";
		 if (value != null)
		 {
			str = value.ToString();
		 }

		if (str.Length == 0)
		{
			str = " ";
		}

		// call DefaultTableCellRenderer's version of this method so that
		// all the cell highlighting is handled properly.
		if (!__multiline && table != null)
		{
			base.getTableCellRendererComponent(table, str, isSelected, hasFocus, row, column);
			setHorizontalAlignment(__justification);
			setFont(new Font(__fontName, __fontStyle, __fontSize));
			setBackground(__color);
			setBorder(new LineBorder(Color.darkGray));
			return this;
		}
		else
		{

			JList list = new JList();
			StreamReader br = new StreamReader(new StringReader(str));
			List<object> v = new List<object>();
			string line;
			try
			{
				line = br.ReadLine();
				while (!string.ReferenceEquals(line, null))
				{
					if (line.Equals(""))
					{
						line = "      ";
					}
					v.Add(line);
					line = br.ReadLine();
				}
			}
			catch (Exception)
			{
			}

			list.setFont(new Font(__fontName, __fontStyle, __fontSize));
			list.setOpaque(true);
			list.setForeground(UIManager.getColor("TableHeader.foreground"));
			list.setBackground(__color);
			list.setBorder(new LineBorder(Color.darkGray));

			list.setListData(v);

			return list;
		}
	}

	/// <summary>
	/// Sets the font in which the header cells should be rendered. </summary>
	/// <param name="fontName"> the name of the font to display the header in </param>
	/// <param name="fontStyle"> the style to display the header font in </param>
	/// <param name="fontSize"> the size to display the header font in </param>
	public virtual void setFont(string fontName, int fontStyle, int fontSize)
	{
		__fontName = fontName;
		__fontStyle = fontStyle;
		__fontSize = fontSize;
	}

	/// <summary>
	/// Sets whether the header should render the header as multiple lines, one
	/// above the other. </summary>
	/// <param name="multiline"> whether to render on more than one line. </param>
	public virtual void setMultiLine(bool multiline)
	{
		__multiline = multiline;
	}

	}

}