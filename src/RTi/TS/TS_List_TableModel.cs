using System;

// TS_List_TableModel - table model for time series header information for TS instances

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

namespace RTi.TS
{

	using TS = RTi.TS.TS;
	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;

	/// <summary>
	/// This class is a table model for time series header information for TS
	/// instances.  By default the sheet will contain row and column numbers.
	/// The alias can be treated as a hidden column.
	/// This class may eventually be moved to the RTi.TS package.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class TS_List_TableModel extends RTi.Util.GUI.JWorksheet_AbstractRowTableModel
	public class TS_List_TableModel : JWorksheet_AbstractRowTableModel
	{

	/// <summary>
	/// Number of columns in the table model (with the alias).
	/// </summary>
	private int __COLUMNS = 13;

	/// <summary>
	/// Absolute column indices, for column lookups (includes the alias).
	/// </summary>
	public readonly int COL_ID = 0;
	public readonly int COL_ALIAS = 1;
	public readonly int COL_NAME = 2;
	public readonly int COL_DATA_SOURCE = 3;
	public readonly int COL_DATA_TYPE = 4;
	public readonly int COL_TIME_STEP = 5;
	public readonly int COL_SCENARIO = 6;
	public readonly int COL_SEQUENCE = 7;
	public readonly int COL_UNITS = 8;
	public readonly int COL_START = 9;
	public readonly int COL_END = 10;
	public readonly int COL_INPUT_TYPE = 11;
	public readonly int COL_INPUT_NAME = 12;

	/// <summary>
	/// Constructor.  This builds the model for displaying the given time series data. </summary>
	/// <param name="data"> the Vector of TS that will be displayed in the table (null is allowed). </param>
	/// <exception cref="Exception"> if an invalid results passed in. </exception>
	public TS_List_TableModel(System.Collections.IList data) : this(data, false)
	{
	}

	/// <summary>
	/// Constructor.  This builds the model for displaying the given time series data. </summary>
	/// <param name="data"> the Vector of TS that will be displayed in the table (null is allowed). </param>
	/// <param name="include_alias"> If true, an alias column will be included after the
	/// location column.  The JWorksheet.removeColumn ( COL_ALIAS ) method should be called. </param>
	public TS_List_TableModel(System.Collections.IList data, bool include_alias)
	{
		if (data == null)
		{
			_rows = 0;
		}
		else
		{
			_rows = data.Count;
		}
		_data = data;
	}

	/// <summary>
	/// From AbstractTableModel.  Returns the class of the data stored in a given
	/// column.  All values are treated as strings. </summary>
	/// <param name="columnIndex"> the column for which to return the data class. </param>
	public virtual Type getColumnClass(int columnIndex)
	{
		switch (columnIndex)
		{
			case COL_ID:
				return typeof(string);
			case COL_ALIAS:
				return typeof(string);
			case COL_NAME:
				return typeof(string);
			case COL_DATA_SOURCE:
				return typeof(string);
			case COL_DATA_TYPE:
				return typeof(string);
			case COL_TIME_STEP:
				return typeof(string);
			case COL_SCENARIO:
				return typeof(string);
			case COL_SEQUENCE:
				return typeof(string);
			case COL_UNITS:
				return typeof(string);
			case COL_START:
				return typeof(string);
			case COL_END:
				return typeof(string);
			case COL_INPUT_TYPE:
				return typeof(string);
			case COL_INPUT_NAME:
				return typeof(string);
			default:
				return typeof(string);
		}
	}

	/// <summary>
	/// From AbstractTableMode.  Returns the number of columns of data. </summary>
	/// <returns> the number of columns of data. </returns>
	public virtual int getColumnCount()
	{
		return __COLUMNS;
	}

	/// <summary>
	/// From AbstractTableMode.  Returns the name of the column at the given position. </summary>
	/// <returns> the name of the column at the given position. </returns>
	public virtual string getColumnName(int columnIndex)
	{
		switch (columnIndex)
		{
			case COL_ID:
				return "\nID";
			case COL_ALIAS:
				return "\nAlias";
			case COL_NAME:
				return "Name/\nDescription";
			case COL_DATA_SOURCE:
				return "Data\nSource";
			case COL_DATA_TYPE:
				return "Data\nType";
			case COL_TIME_STEP:
				return "Time\nStep";
			case COL_SCENARIO:
				return "\nScenario";
			case COL_SEQUENCE:
				return "Sequence\nID";
			case COL_UNITS:
				return "\nUnits";
			case COL_START:
				return "\nStart";
			case COL_END:
				return "\nEnd";
			case COL_INPUT_TYPE:
				return "Input\nType";
			case COL_INPUT_NAME:
				return "Input\nName";
			default:
				return "";
		}
	}

	/// <summary>
	/// Returns the text to be assigned to worksheet tooltips. </summary>
	/// <returns> a String array of tool tips. </returns>
	public override string[] getColumnToolTips()
	{
		string[] tips = new string[__COLUMNS];

		tips[COL_ID] = "<html>The location identifier for the time series (e.g., station ID).</html>";
		tips[COL_ALIAS] = "The alias the time series (optional).";
		tips[COL_NAME] = "The descriptive name for the time series.";
		tips[COL_DATA_SOURCE] = "The data source for the time series.";
		tips[COL_DATA_TYPE] = "The data type for the time series.";
		tips[COL_TIME_STEP] = "The data interval (time step) for the time series.";
		tips[COL_SCENARIO] = "The data scenario (optional).";
		tips[COL_SEQUENCE] = "For time series traces, typically the historical year for the trace.";
		tips[COL_UNITS] = "The data units for the time series.";
		tips[COL_START] = "The period start for available data.";
		tips[COL_END] = "The period end for available data.";
		tips[COL_INPUT_TYPE] = "The input type (e.g., database/file format).";
		tips[COL_INPUT_NAME] = "The specific database/file input name.";

		return tips;
	}

	/// <summary>
	/// Returns an array containing the column widths (in number of characters). </summary>
	/// <returns> an integer array containing the widths for each field. </returns>
	public virtual int[] getColumnWidths()
	{
		int[] widths = new int[__COLUMNS];
		widths[COL_ID] = 12;
		widths[COL_ALIAS] = 12;
		widths[COL_NAME] = 20;
		widths[COL_DATA_SOURCE] = 10;
		widths[COL_DATA_TYPE] = 8;
		widths[COL_TIME_STEP] = 8;
		widths[COL_SCENARIO] = 8;
		widths[COL_SEQUENCE] = 8;
		widths[COL_UNITS] = 8;
		widths[COL_START] = 10;
		widths[COL_END] = 10;
		widths[COL_INPUT_TYPE] = 12;
		widths[COL_INPUT_NAME] = 20;
		return widths;
	}

	/// <summary>
	/// Returns the format to display the specified column. </summary>
	/// <param name="column"> column for which to return the format. </param>
	/// <returns> the format (as used by StringUtil.formatString()). </returns>
	public override string getFormat(int column)
	{
		switch (column)
		{
			default:
				return "%s";
		}
	}

	/// <summary>
	/// From AbstractTableMode.  Returns the number of rows of data in the table.
	/// </summary>
	public virtual int getRowCount()
	{
		return _rows;
	}

	/// <summary>
	/// From AbstractTableMode.  Returns the data that should be placed in the JTable at the given row and column. </summary>
	/// <param name="row"> the row for which to return data. </param>
	/// <param name="col"> the absolute column for which to return data. </param>
	/// <returns> the data that should be placed in the JTable at the given row and col. </returns>
	public virtual object getValueAt(int row, int col)
	{ // make sure the row numbers are never sorted ...

		if (_sortOrder != null)
		{
			row = _sortOrder[row];
		}

		TS ts = (TS)_data.get(row);
		switch (col)
		{
			case COL_ID:
				return ts.getIdentifier().getLocation();
			case COL_ALIAS:
				return ts.getAlias();
			case COL_NAME:
				return ts.getDescription();
			case COL_DATA_SOURCE:
				return ts.getIdentifier().getSource();
			case COL_DATA_TYPE:
				return ts.getDataType();
			case COL_TIME_STEP:
				return ts.getIdentifier().getInterval();
			case COL_SCENARIO:
				return ts.getIdentifier().getScenario();
			case COL_SEQUENCE:
				return ts.getSequenceID();
			case COL_UNITS:
				return ts.getDataUnits();
			case COL_START:
				return ts.getDate1();
			case COL_END:
				return ts.getDate2();
			case COL_INPUT_TYPE:
				return ts.getIdentifier().getInputType();
			case COL_INPUT_NAME:
				return ts.getIdentifier().getInputName();
			default:
				return "";
		}
	}

	}

}