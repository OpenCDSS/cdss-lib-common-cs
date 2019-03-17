using System;

// DataStores_CellRenderer - cell renderer to display list of DataStores

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

namespace riverside.datastore
{


	using JWorksheet = RTi.Util.GUI.JWorksheet;
	using JWorksheet_AbstractExcelCellRenderer = RTi.Util.GUI.JWorksheet_AbstractExcelCellRenderer;
	using JWorksheet_CellAttributes = RTi.Util.GUI.JWorksheet_CellAttributes;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This class is a cell renderer for cells in DataStores JWorksheets.
	/// </summary>
	public class DataStores_CellRenderer : JWorksheet_AbstractExcelCellRenderer
	{

	/// <summary>
	/// The table model for which this class will render cells.
	/// </summary>
	private DataStores_TableModel __tableModel;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="tableModel"> the tableModel for which to render cells. </param>
	public DataStores_CellRenderer(DataStores_TableModel tableModel)
	{
		__tableModel = tableModel;
	}

	/// <summary>
	/// Renders a cell for the worksheet. </summary>
	/// <param name="table"> the JWorksheet for which a cell will be renderer. </param>
	/// <param name="value"> the value in the cell. </param>
	/// <param name="isSelected"> whether the cell is selected or not. </param>
	/// <param name="hasFocus"> whether the cell has the input focus or not. </param>
	/// <param name="row"> the row in the worksheet where the cell is located. </param>
	/// <param name="column"> the column in the worksheet where the cell is located. </param>
	/// <returns> the rendered cell. </returns>
	public override Component getTableCellRendererComponent(JTable table, object value, bool isSelected, bool hasFocus, int row, int column)
	{
		string str = "";
		 if (value != null)
		 {
			str = value.ToString();
		 }

		 JWorksheet ws = (JWorksheet)table;
		int abscolumn = ws.getAbsoluteColumn(column);

		string format = getFormat(abscolumn);

		int justification = SwingConstants.LEFT;

		if (value is int?)
		{
			justification = SwingConstants.RIGHT;
			str = StringUtil.formatString(value, format);
		}
		else if (value is double?)
		{
			justification = SwingConstants.RIGHT;
			str = StringUtil.formatString(value, format);
		}
		else if (value is System.DateTime)
		{
			justification = SwingConstants.LEFT;
			// FYI: str has been set above with str = value.toString()
		}
		else if (value is string)
		{
			justification = SwingConstants.LEFT;
			str = StringUtil.formatString(value, format);
		}
		else if (value is float?)
		{
			justification = SwingConstants.RIGHT;
			str = StringUtil.formatString(value, format);
		}
		else
		{
			justification = SwingConstants.LEFT;
		}

		str = str.Trim();

		// call DefaultTableCellRenderer's version of this method so that
		// all the cell highlighting is handled properly.
		base.getTableCellRendererComponent(table, str, isSelected, hasFocus, row, column);

		int tableAlignment = ws.getColumnAlignment(abscolumn);
		if (tableAlignment != JWorksheet.DEFAULT)
		{
			justification = tableAlignment;
		}

		setHorizontalAlignment(justification);
		setFont(ws.getCellFont());

		if (column == __tableModel.COL_NAME)
		{
			// Set the foreground to yellow if the column is the datastore name and is a duplicate
			// This is brute force but there are not that many datastores so OK
			// Yellow is easier to read as background.
			bool duplicate = false;
			for (int irow = 0; irow < __tableModel.getRowCount(); irow++)
			{
				if (irow == row)
				{
					continue;
				}
				if (str.Equals("" + __tableModel.getValueAt(irow, column), StringComparison.OrdinalIgnoreCase))
				{
					duplicate = true;
					break;
				}
			}
			if (duplicate)
			{
				JWorksheet_CellAttributes ca = ws.getCellAttributes(row, column);
				if (ca == null)
				{
					ca = new JWorksheet_CellAttributes();
				}
				ca.backgroundColor = Color.yellow;
				ws.setCellAttributes(row, column, ca);
			}
		}
		else if (column == __tableModel.COL_STATUS)
		{
			if (str.ToUpper().IndexOf("ERROR", StringComparison.Ordinal) >= 0)
			{
				// Some type of error so highlight
				JWorksheet_CellAttributes ca = ws.getCellAttributes(row, column);
				if (ca == null)
				{
					ca = new JWorksheet_CellAttributes();
				}
				ca.backgroundColor = Color.yellow;
				ws.setCellAttributes(row, column, ca);
			}
		}
		else if (column == __tableModel.COL_STATUS_MESSAGE)
		{
			if (str.Length > 0)
			{
				// Some type of error so highlight
				JWorksheet_CellAttributes ca = ws.getCellAttributes(row, column);
				if (ca == null)
				{
					ca = new JWorksheet_CellAttributes();
				}
				ca.backgroundColor = Color.yellow;
				ws.setCellAttributes(row, column, ca);
			}
		}

		return this;
	}

	/// <summary>
	/// Returns the data format for the given column. </summary>
	/// <param name="column"> the column for which to return the data format. </param>
	/// <returns> the data format for the given column. </returns>
	public override string getFormat(int column)
	{
		return __tableModel.getFormat(column);
	}

	/// <summary>
	/// Returns the widths the columns should be set to. </summary>
	/// <returns> the widths the columns should be set to. </returns>
	public override int[] getColumnWidths()
	{
		return __tableModel.getColumnWidths();
	}

	}

}