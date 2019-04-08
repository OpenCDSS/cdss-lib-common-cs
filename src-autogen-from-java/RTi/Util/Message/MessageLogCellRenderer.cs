// MessageLogCellRenderer - the renderer for the worksheet that displays a log file.

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
// MessageLogCellRenderer - the renderer for the worksheet that displays
//	a log file.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2005-03-22	J. Thomas Sapienza, RTi	Initial version.
// 2005-04-26	JTS, RTi		Added finalize().
// 2005-05-23	JTS, RTi		Added getTableCellRendererComponent()
//					and modified it from the base class
//					version so that it only supports 
//					Strings and also doesn't trim Strings.
// ----------------------------------------------------------------------------

namespace RTi.Util.Message
{


	using StringUtil = RTi.Util.String.StringUtil;

	using JWorksheet = RTi.Util.GUI.JWorksheet;
	using JWorksheet_AbstractExcelCellRenderer = RTi.Util.GUI.JWorksheet_AbstractExcelCellRenderer;

	/// <summary>
	/// This class is the cell renderer for a worksheet that displays a log file.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class MessageLogCellRenderer extends RTi.Util.GUI.JWorksheet_AbstractExcelCellRenderer
	public class MessageLogCellRenderer : JWorksheet_AbstractExcelCellRenderer
	{

	/// <summary>
	/// Table model for which this class renders the cell.
	/// </summary>
	private MessageLogTableModel __tableModel;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="model"> the model for which this class will render cells. </param>
	public MessageLogCellRenderer(MessageLogTableModel model)
	{
		__tableModel = model;
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~MessageLogCellRenderer()
	{
		__tableModel = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the widths of the columns in the table. </summary>
	/// <returns> an integer array of the widths of the columns in the table. </returns>
	public override int[] getColumnWidths()
	{
		return __tableModel.getColumnWidths();
	}

	/// <summary>
	/// Returns the format for a given column. </summary>
	/// <param name="column"> the column for which to return the format (0-based). </param>
	/// <returns> the format (as used by StringUtil.formatString()) for a column. </returns>
	public override string getFormat(int column)
	{
		return __tableModel.getFormat(column);
	}

	/// <summary>
	/// Renders a value for a cell in a JTable.  This method is called automatically
	/// by the JTable when it is rendering its cells.  This overrides some code from
	/// DefaultTableCellRenderer. </summary>
	/// <param name="table"> the JTable (in this case, JWorksheet) in which the cell
	/// to be rendered will appear. </param>
	/// <param name="value"> the cell's value to be rendered. </param>
	/// <param name="isSelected"> whether the cell is selected or not. </param>
	/// <param name="hasFocus"> whether the cell has focus or not. </param>
	/// <param name="row"> the row in which the cell appears. </param>
	/// <param name="column"> the column in which the cell appears. </param>
	/// <returns> a properly-rendered cell that can be placed in the table. </returns>
	public override Component getTableCellRendererComponent(JTable table, object value, bool isSelected, bool hasFocus, int row, int column)
	{
		string str = "";
		 if (value != null)
		 {
			str = value.ToString();
		 }

		int abscolumn = ((JWorksheet)table).getAbsoluteColumn(column);

		string format = getFormat(abscolumn);

		int justification = SwingConstants.LEFT;
		str = StringUtil.formatString(value, format);

		// call DefaultTableCellRenderer's version of this method so that
		// all the cell highlighting is handled properly.
		base.getTableCellRendererComponent(table, str, isSelected, hasFocus, row, column);

		int tableAlignment = ((JWorksheet)table).getColumnAlignment(abscolumn);
		if (tableAlignment != JWorksheet.DEFAULT)
		{
			justification = tableAlignment;
		}

		setHorizontalAlignment(justification);
		setFont(((JWorksheet)table).getCellFont());

		return this;
	}

	}

}