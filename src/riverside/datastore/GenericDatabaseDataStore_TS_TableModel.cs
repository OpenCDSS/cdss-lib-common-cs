using System;

// GenericDatabaseDataStore_TS_TableModel - table model to display time series data in UI

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

	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;

	/// <summary>
	/// This class is a table model for time series metadata information.
	/// By default the sheet will contain row and column numbers.
	/// </summary>
	public class GenericDatabaseDataStore_TS_TableModel : JWorksheet_AbstractRowTableModel
	{

	/// <summary>
	/// Number of columns in the table model (with the alias).
	/// </summary>
	private int __COLUMNS = 11;

	/// <summary>
	/// Absolute column indices, for column lookups.
	/// </summary>
	public readonly int COL_LOC_TYPE = 0;
	public readonly int COL_ID = 1;
	//public final int COL_ALIAS = 1;
	public readonly int COL_DESC = 2;
	public readonly int COL_DATA_SOURCE = 3;
	public readonly int COL_DATA_TYPE = 4;
	public readonly int COL_TIME_STEP = 5;
	public readonly int COL_SCENARIO = 6;
	//public final int COL_SEQUENCE = 7;
	public readonly int COL_UNITS = 7;
	public readonly int COL_START = 8;
	public readonly int COL_END = 9;
	public readonly int COL_INPUT_TYPE = 10;
	//public final int COL_INPUT_NAME	= 12;

	/// <summary>
	/// Datastore that is providing the data.
	/// </summary>
	private GenericDatabaseDataStore __dataStore = null;

	/// <summary>
	/// Constructor.  This builds the model for displaying the given time series data. </summary>
	/// <param name="data"> the list of TimeSeriesMeta that will be displayed in the table (null is allowed).
	/// location column.  The JWorksheet.removeColumn ( COL_ALIAS ) method should be called. </param>
	/// <exception cref="Exception"> if an invalid results passed in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public GenericDatabaseDataStore_TS_TableModel(java.util.List data, GenericDatabaseDataStore dataStore) throws Exception
	public GenericDatabaseDataStore_TS_TableModel(System.Collections.IList data, GenericDatabaseDataStore dataStore)
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
		__dataStore = dataStore;
	}

	/// <summary>
	/// From AbstractTableModel.  Returns the class of the data stored in a given
	/// column.  All values are treated as strings. </summary>
	/// <param name="columnIndex"> the column for which to return the data class. </param>
	public virtual Type getColumnClass(int columnIndex)
	{
		switch (columnIndex)
		{
			case COL_LOC_TYPE:
				return typeof(string);
			case COL_ID:
				return typeof(string);
			case COL_DESC:
				return typeof(string);
			case COL_DATA_SOURCE:
				return typeof(string);
			case COL_DATA_TYPE:
				return typeof(string);
			case COL_TIME_STEP:
				return typeof(string);
			case COL_SCENARIO:
				return typeof(string);
			case COL_UNITS:
				return typeof(string);
			case COL_START:
				return typeof(string);
			case COL_END:
				return typeof(string);
			case COL_INPUT_TYPE:
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
			case COL_LOC_TYPE:
				return "Location\nType";
			case COL_ID:
				return "\nID";
			case COL_DESC:
				return "Name/\nDescription";
			case COL_DATA_SOURCE:
				return "Data\nSource";
			case COL_DATA_TYPE:
				return "Data\nType";
			case COL_TIME_STEP:
				return "Time\nStep";
			case COL_SCENARIO:
				return "\nScenario";
			case COL_UNITS:
				return "\nUnits";
			case COL_START:
				return "\nStart";
			case COL_END:
				return "\nEnd";
			case COL_INPUT_TYPE:
				return "Input\nType";
			default:
				return "";
		}
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
	/// <returns> the data that should be placed in the JTable at the given row and column. </returns>
	public virtual object getValueAt(int row, int col)
	{ // make sure the row numbers are never sorted ...

		if (_sortOrder != null)
		{
			row = _sortOrder[row];
		}

		TimeSeriesMeta meta = (TimeSeriesMeta)_data.get(row);
		if (meta == null)
		{
			return "";
		}
		switch (col)
		{
			case COL_LOC_TYPE:
				return meta.getLocationType();
			case COL_ID:
				return meta.getLocationID();
			case COL_DESC:
				return meta.getDescription();
			case COL_DATA_SOURCE:
				return meta.getDataSource();
			case COL_DATA_TYPE:
				return meta.getDataType();
			case COL_TIME_STEP:
				return meta.getInterval();
			case COL_SCENARIO:
				return meta.getScenario();
			case COL_UNITS:
				return meta.getUnits();
			case COL_START:
				return "";
			case COL_END:
				return "";
			case COL_INPUT_TYPE:
					return __dataStore.getName();
			default:
				return "";
		}
	}

	/// <summary>
	/// Returns an array containing the column widths (in number of characters). </summary>
	/// <returns> an integer array containing the widths for each field. </returns>
	public virtual int[] getColumnWidths()
	{
		int[] widths = new int[__COLUMNS];
		widths[COL_LOC_TYPE] = 12;
		widths[COL_ID] = 12;
		widths[COL_DESC] = 20;
		widths[COL_DATA_SOURCE] = 10;
		widths[COL_DATA_TYPE] = 8;
		widths[COL_TIME_STEP] = 8;
		widths[COL_SCENARIO] = 8;
		widths[COL_UNITS] = 8;
		widths[COL_START] = 10;
		widths[COL_END] = 10;
		widths[COL_INPUT_TYPE] = 12;
		return widths;
	}

	}

}