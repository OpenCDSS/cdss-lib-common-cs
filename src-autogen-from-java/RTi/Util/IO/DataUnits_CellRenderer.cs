// DataUnits_CellRenderer - this class is a cell renderer for cells in DataUnits JWorksheets.

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

namespace RTi.Util.IO
{


	using JWorksheet = RTi.Util.GUI.JWorksheet;
	using JWorksheet_AbstractExcelCellRenderer = RTi.Util.GUI.JWorksheet_AbstractExcelCellRenderer;

	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This class is a cell renderer for cells in DataUnits JWorksheets.
	/// </summary>
	public class DataUnits_CellRenderer : JWorksheet_AbstractExcelCellRenderer
	{

	/// <summary>
	/// The table model for which this class will render cells.
	/// </summary>
	private DataUnits_TableModel __tableModel;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="tableModel"> the tableModel for which to render cells. </param>
	public DataUnits_CellRenderer(DataUnits_TableModel tableModel)
	{
		__tableModel = tableModel;
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~DataUnits_CellRenderer()
	{
		__tableModel = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
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

		int abscolumn = ((JWorksheet)table).getAbsoluteColumn(column);

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

		int tableAlignment = ((JWorksheet)table).getColumnAlignment(abscolumn);
		if (tableAlignment != JWorksheet.DEFAULT)
		{
			justification = tableAlignment;
		}

		setHorizontalAlignment(justification);
		setFont(((JWorksheet)table).getCellFont());

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