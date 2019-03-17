using System;

// CommandLog_TableModel - table model that displays a list of CommandLogRecord in a worksheet

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

	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;

	/// <summary>
	/// This table model displays a list of CommandLogRecord in a worksheet.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class CommandLog_TableModel extends RTi.Util.GUI.JWorksheet_AbstractRowTableModel
	public class CommandLog_TableModel : JWorksheet_AbstractRowTableModel
	{

	/// <summary>
	/// Number of columns in the table model.
	/// </summary>
	private readonly int __COLUMNS = 5;

	/// <summary>
	/// References to columns.
	/// </summary>
	public const int COL_SEVERITY = 0, COL_TYPE = 1, COL_COMMAND = 2, COL_PROBLEM = 3, COL_RECOMMENDATION = 4;
		// TODO SAM 2009-02-27 Add reference to data object that caused problem
		// COL_DATA;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <exception cref="Exception"> if an invalid data was passed in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public CommandLog_TableModel(java.util.List data) throws Exception
	public CommandLog_TableModel(System.Collections.IList data)
	{
		if (data == null)
		{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			throw new Exception("Invalid data list passed to " + this.GetType().FullName + " constructor.");
		}
		_rows = data.Count;
		_data = data;
	}

	/// <summary>
	/// Returns the class of the data stored in a given column. </summary>
	/// <param name="columnIndex"> the column for which to return the data class. </param>
	public virtual Type getColumnClass(int columnIndex)
	{
		switch (columnIndex)
		{
			case COL_SEVERITY:
				return typeof(string); //CommandStatusType.class;
			case COL_TYPE:
				return typeof(string); // from class name
			case COL_COMMAND:
				return typeof(string); //CommandStatusProvider.class;
			case COL_PROBLEM:
				return typeof(string);
			case COL_RECOMMENDATION:
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
	/// <param name="columnIndex"> the position of the column for which to return the name. </param>
	/// <returns> the name of the column at the given position. </returns>
	public virtual string getColumnName(int columnIndex)
	{
		switch (columnIndex)
		{
			case COL_SEVERITY:
				return "Severity";
			case COL_TYPE:
				return "Type";
			case COL_COMMAND:
				return "Command";
			case COL_PROBLEM:
				return "Problem";
			case COL_RECOMMENDATION:
				return "Recommendation";
			default:
				return " ";
		}
	}

	/// <summary>
	/// Returns an array containing the column widths (in number of characters). </summary>
	/// <returns> an integer array containing the widths for each field. </returns>
	public override string[] getColumnToolTips()
	{
		string[] tips = new string[__COLUMNS];
		tips[COL_SEVERITY] = "Severity of problem, WARNING and FAILURE typically need to be resolved.";
		tips[COL_TYPE] = "Type of problem.";
		tips[COL_COMMAND] = "Command associated with problem.";
		tips[COL_PROBLEM] = "Problem description.";
		tips[COL_RECOMMENDATION] = "Recommendation for resolving problem.";
		return tips;
	}

	/// <summary>
	/// Returns an array containing the widths (in number of characters) that the 
	/// fields in the table should be sized to. </summary>
	/// <returns> an integer array containing the widths for each field. </returns>
	public virtual int[] getColumnWidths()
	{
		int[] widths = new int[__COLUMNS];
		widths[COL_SEVERITY] = 6;
		widths[COL_TYPE] = 15;
		widths[COL_COMMAND] = 23;
		widths[COL_PROBLEM] = 23;
		widths[COL_RECOMMENDATION] = 23;
		return widths;
	}

	/// <summary>
	/// Returns the format that the specified column should be displayed in when
	/// the table is being displayed in the given table format. </summary>
	/// <param name="column"> column for which to return the format. </param>
	/// <returns> the format (as used by StringUtil.formatString() in which to display the column. </returns>
	public override string getFormat(int column)
	{
		switch (column)
		{
			case COL_SEVERITY:
				return "%-8s";
			case COL_TYPE:
				return "%-s";
			case COL_COMMAND:
				return "%-40s";
			case COL_PROBLEM:
				return "%-40s";
			case COL_RECOMMENDATION:
				return "%-40s";
			default:
				return "%-8s";
		}
	}

	/// <summary>
	/// Returns the number of rows of data in the table. </summary>
	/// <returns> the number of rows of data in the table. </returns>
	public virtual int getRowCount()
	{
		return _rows;
	}

	/// <summary>
	/// Returns the data that should be placed in the JTable at the given row and column. </summary>
	/// <param name="row"> the row for which to return data. </param>
	/// <param name="col"> the column for which to return data. </param>
	/// <returns> the data that should be placed in the JTable at the given row and col. </returns>
	public virtual object getValueAt(int row, int col)
	{
		if (_sortOrder != null)
		{
			row = _sortOrder[row];
		}

		CommandLogRecord log = (CommandLogRecord)_data.get(row);

		switch (col)
		{
			case COL_SEVERITY:
				return log.getSeverity().ToString();
			case COL_TYPE:
				return CommandStatusUtil.getCommandLogRecordDisplayName(log);
			case COL_COMMAND:
				CommandStatusProvider csp = log.getCommandStatusProvider();
				if (csp == null)
				{
					return "";
				}
				else
				{
					return csp.ToString();
				}
			case COL_PROBLEM:
				return log.getProblem();
			case COL_RECOMMENDATION:
				return log.getRecommendation();
			default:
				return "";
		}
	}

	/// <summary>
	/// Returns whether the cell at the given position is editable or not.  In this
	/// table model all columns are not editable. </summary>
	/// <param name="rowIndex"> unused </param>
	/// <param name="columnIndex"> the index of the column to check for whether it is editable. </param>
	/// <returns> whether the cell at the given position is editable. </returns>
	public virtual bool isCellEditable(int rowIndex, int columnIndex)
	{
		return false;
	}

	/// <summary>
	/// Sets the value at the specified position to the specified value. </summary>
	/// <param name="value"> the value to set the cell to. </param>
	/// <param name="row"> the row of the cell for which to set the value. </param>
	/// <param name="col"> the col of the cell for which to set the value. </param>
	public virtual void setValueAt(object value, int row, int col)
	{
		if (_sortOrder != null)
		{
			row = _sortOrder[row];
		}

		CommandLogRecord log = (CommandLogRecord)_data.get(row);

		switch (col)
		{
			case COL_SEVERITY:
				// Not editable and currently no way to set...log.setSeverity((CommandStatusType)value);
				break;
			case COL_TYPE:
				// Not editable and currently no way to set
				break;
			case COL_COMMAND:
				// Not editable and currently no way to set...log.setCommand((Command)value);
				break;
			case COL_PROBLEM:
				log.setProblem((string)value);
				break;
			case COL_RECOMMENDATION:
				log.setRecommendation((string)value);
				break;
		}

		base.setValueAt(value, row, col);
	}

	}

}