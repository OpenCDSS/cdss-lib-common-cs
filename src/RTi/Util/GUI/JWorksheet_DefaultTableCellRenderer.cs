﻿// JWorksheet_DefaultTableCellRenderer - base class to use as a cell renderer for the JWorksheet

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
// JWorksheet_DefaultTableCellRenderer - Base class to use as a cell renderer
//	for the JWorksheet, if no other renderers are to be used.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2002-12-XX	J. Thomas Sapienza, RTi	Initial version.
// 2003-03-04	JTS, RTi		Javadoc'd, revised.  
// 2003-03-20	JTS, RTi		* Revised after SAM's review.
//					* getWidths renamed to getColumnWidths
//					* Extends JWorksheet_AbstractTable ...
//					  CellRenderer now.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.Util.GUI
{

	/// <summary>
	/// Base class from which other classes that are to be used as cell renderers in 
	/// the JWorksheet should be built, as it provides a getWidths() routine (which 
	/// is used by JWorksheet) if the other classes do not.  If a JTable is not
	/// assigned a specific cell renderer, this one will do the job.<para>
	/// </para>
	/// REVISIT (JTS - 2006-05-25)<para>
	/// If I could do this over again, I would have combined AbstractTableCellRenderer,
	/// DefaultTableCellRenderer and AbstractExcelCellRenderer into a single cell 
	/// renderer.  The reasoning for having the separation came about from the 
	/// </para>
	/// way the JWorksheet was designed originally.<para>
	/// AbstractTableCellRenderer was supposed to be The Base Class for all other 
	/// </para>
	/// renderers, providing the basic outline of what they would do.<para>
	/// DefaultTableCellRenderer was supposed to be used for worksheets that didn't
	/// </para>
	/// require any special cell formatting.<para>
	/// AbstractExcelCellRenderer was supposed to be the base class for cell renderers
	/// </para>
	/// that would do formatting of cell contents.<para>
	/// </para>
	/// In theory.<para>
	/// In practice, ALL cell renderers are doing cell formatting, so the 
	/// AbstractTableCellRenderer and DefaultTableCellRenderer are unnecessary overhead.
	/// </para>
	/// <para>
	/// </para>
	/// <b>Also</b><para>
	/// I really don't see much of a good reason to even REQUIRE cell renderers for
	/// most classes.  There are a lot of cell renderers out there that are almost 100%
	/// the same class.  At this point there's little chance of going back and 
	/// eliminating them, but if I could I would.  Use a default cell renderer for all
	/// those classes and eliminate a lot of maintenance woes.
	/// </para>
	/// </summary>
	public class JWorksheet_DefaultTableCellRenderer : JWorksheet_AbstractTableCellRenderer
	{

	/// <summary>
	/// Returns the table cell renderer used to render a cell in a table. </summary>
	/// <param name="table"> the JWorksheet in which the cell is being renderer. </param>
	/// <param name="value"> the value to put in the cell. </param>
	/// <param name="isSelected"> whether the cell is selected or not. </param>
	/// <param name="hasFocus"> whether the cell currently has focus or not. </param>
	/// <param name="row"> the row in which the cell can be found. </param>
	/// <param name="column"> the column in which the cell can be found. </param>
	/// <returns> a component that can be displayed in a table cell, which contains 
	/// the value of the cell being rendered. </returns>
	public virtual Component getTableCellRendererComponent(JTable table, object value, bool isSelected, bool hasFocus, int row, int column)
	{
		return base.getTableCellRendererComponent(table, value, isSelected, hasFocus, row, column);
	}

	}

}