using System;
using System.Collections.Generic;

// MessageLogTableModel - this class is the table model for displaying a log file in a worksheet.

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
// MessageLogTableModel - this class is the table model for displaying a log 
//	file in a worksheet.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2005-03-22	J. Thomas Sapienza, RTi	Initial version.
// ----------------------------------------------------------------------------

namespace RTi.Util.Message
{

	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;

	/// <summary>
	/// This class is a table model for displaying log file data within a worksheet.  
	/// The table contains a single column, each row of which is a line in a log file.
	/// The worksheet text is all monospaced, and there is no header on the right side
	/// for listing line numbers.
	/// </summary>
	public class MessageLogTableModel : JWorksheet_AbstractRowTableModel
	{

	/// <summary>
	/// Number of columns in the table model.
	/// </summary>
	private const int __COLUMNS = 2;

	/// <summary>
	/// Reference to the column.
	/// </summary>
	public const int COL_MESSAGE = 1;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="messages"> the messages that will be displayed in the table.  Each element
	/// in the Vector is a line in the log file, and must be a String. </param>
	public MessageLogTableModel(System.Collections.IList messages)
	{
		if (messages == null)
		{
			messages = new List<object>();
		}
		_rows = messages.Count;
		_data = messages;
	}

	/// <summary>
	/// Returns the class of the data stored in a given column. </summary>
	/// <param name="columnIndex"> the column for which to return the data class. </param>
	public virtual Type getColumnClass(int columnIndex)
	{
		switch (columnIndex)
		{
			case COL_MESSAGE:
				return typeof(string);
			default:
				return typeof(string);
		}
	}

	/// <summary>
	/// Returns the number of columns of data. </summary>
	/// <returns> the number of columns of data. </returns>
	public virtual int getColumnCount()
	{
		return __COLUMNS;
	}

	/// <summary>
	/// Returns the name of the column at the given position. </summary>
	/// <returns> the name of the column at the given position. </returns>
	public virtual string getColumnName(int columnIndex)
	{
		switch (columnIndex)
		{
			case COL_MESSAGE:
				return "MESSAGE";
			default:
				return " ";
		}
	}

	/// <summary>
	/// Returns an array containing the widths that the fields in the table should 
	/// be sized to.  The widths roughly correspond to the number of characters wide
	/// the column should be. </summary>
	/// <returns> an integer array containing the widths for each column. </returns>
	public virtual int[] getColumnWidths()
	{
		int[] widths = new int[__COLUMNS];
		for (int i = 0; i < __COLUMNS; i++)
		{
			widths[i] = 0;
		}
		widths[COL_MESSAGE] = 256;
		return widths;
	}

	/// <summary>
	/// Returns the format that the specified column should be displayed in when
	/// the table is being displayed in the given table format. </summary>
	/// <param name="column"> column for which to return the format. </param>
	/// <returns> the format (as used by StringUtil.formatString()) in which to display 
	/// the column. </returns>
	public override string getFormat(int column)
	{
		switch (column)
		{
			case COL_MESSAGE:
				return "%-256s";
			default:
				return "%-8s";
		}
	}

	/// <summary>
	/// Returns the number of rows of data in the table.
	/// </summary>
	public virtual int getRowCount()
	{
		return _rows;
	}

	/// <summary>
	/// Returns the data that should be placed in the JTable at the given row and column </summary>
	/// <param name="row"> the row for which to return data. </param>
	/// <param name="col"> the column for which to return data. </param>
	/// <returns> the data that should be placed in the JTable at the given row and col. </returns>
	public virtual object getValueAt(int row, int col)
	{
		return _data.get(row);
	}

	}

}