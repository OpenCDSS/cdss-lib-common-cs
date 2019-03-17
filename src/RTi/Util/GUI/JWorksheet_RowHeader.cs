// JWorksheet_RowHeader - class to create a worksheet header that tracks row numbers

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
// JWorksheet_RowHeader - class to create a worksheet header that tracks 
//	row numbers.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2004-01-20	J. Thomas Sapienza, RTi	Initial version.
// ----------------------------------------------------------------------------

namespace RTi.Util.GUI
{




	/// <summary>
	/// This class is a header for worksheets that goes on the left side of the 
	/// worksheet and tracks the row number for each row.
	/// </summary>
	public class JWorksheet_RowHeader : JLabel, ListCellRenderer
	{

	/// <summary>
	/// Constructor. </summary>
	/// <param name="worksheet"> the worksheet in which the header will appear. </param>
	/// <param name="font"> the Font to use for text in the header. </param>
	/// <param name="backgroundColor"> the color to set the scrolled area background to. </param>
	public JWorksheet_RowHeader(JWorksheet worksheet, Font font, Color backgroundColor)
	{
		JTableHeader header = worksheet.getTableHeader();
		setOpaque(true);
		setBorder(UIManager.getBorder("TableHeader.cellBorder"));
		setBorder(new LineBorder(Color.darkGray));
		setHorizontalAlignment(CENTER);
		setForeground(header.getForeground());
		setBackground(header.getBackground());
		setFont(font);
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="worksheet"> the worksheet in which the header appears. </param>
	public JWorksheet_RowHeader(JWorksheet worksheet)
	{
		JTableHeader header = worksheet.getTableHeader();
		setOpaque(true);
		setBorder(UIManager.getBorder("TableHeader.cellBorder"));
		setBorder(new LineBorder(Color.darkGray));
		setHorizontalAlignment(CENTER);
		setForeground(header.getForeground());
		setBackground(header.getBackground());
		setFont(new Font("Arial", 12, Font.BOLD));
	}

	/// <summary>
	/// Returns the rendered value for a given row. </summary>
	/// <param name="list"> ignored. </param>
	/// <param name="value"> the value to put in the given row's header. </param>
	/// <param name="index"> the row number for which to render the header cell. </param>
	/// <param name="isSelected"> ignored. </param>
	/// <param name="cellhasFocus"> ignored. </param>
	public virtual Component getListCellRendererComponent(JList list, object value, int index, bool isSelected, bool cellHasFocus)
	{
		setText((value == null) ? "" : value.ToString());
		return this;
	}

	}

}