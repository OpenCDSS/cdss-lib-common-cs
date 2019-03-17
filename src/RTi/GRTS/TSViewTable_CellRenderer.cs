// TSViewTable_CellRenderer - this class is used to render cells for TS view tables

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

namespace RTi.GRTS
{


	using JWorksheet = RTi.Util.GUI.JWorksheet;
	using JWorksheet_AbstractExcelCellRenderer = RTi.Util.GUI.JWorksheet_AbstractExcelCellRenderer;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This class is used to render cells for TS view tables.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class TSViewTable_CellRenderer extends RTi.Util.GUI.JWorksheet_AbstractExcelCellRenderer
	public class TSViewTable_CellRenderer : JWorksheet_AbstractExcelCellRenderer
	{

	/// <summary>
	/// The table model for which to render cells.
	/// </summary>
	private TSViewTable_TableModel __tableModel;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="tableModel"> the table model for which to render cells </param>
	public TSViewTable_CellRenderer(TSViewTable_TableModel tableModel)
	{
		__tableModel = tableModel;
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~TSViewTable_CellRenderer()
	{
		__tableModel = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the format for a given column. </summary>
	/// <param name="column"> the column for which to return the format. </param>
	/// <returns> the format (as used by StringUtil.format) for a column. </returns>
	public override string getFormat(int column)
	{
		return __tableModel.getFormat(column);
	}

	/// <summary>
	/// Returns the widths of the columns in the table. </summary>
	/// <returns> an integer array of the widths of the columns in the table. </returns>
	public override int[] getColumnWidths()
	{
		return __tableModel.getColumnWidths();
	}

	/// <summary>
	/// Renders a value for a cell in a JTable.  This method is called automatically
	/// by the JTable when it is rendering its cells.  This overrides some code from DefaultTableCellRenderer.
	/// It handles the justification, which is important with numerical values. </summary>
	/// <param name="table"> the JTable (in this case, JWorksheet) in which the cell to be rendered will appear. </param>
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

		if (value is double?)
		{
			// Time series data value
			// TODO SAM 2010-07-15 If necessary add a method in the table model if the display
			// becomes more complicated with data flags, etc.
			// Currently column 0 is the date/time and columns 1+ are time series.
			if (__tableModel.getTS(column - 1).isDataMissing(((double?)value).Value))
			{
				str = "";
			}
			else
			{
				justification = SwingConstants.RIGHT;
				str = StringUtil.formatString(value, format);
			}
		}
		else if (value is string)
		{
			// Date/times are formatted as strings.
			justification = SwingConstants.LEFT;
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

	}

}