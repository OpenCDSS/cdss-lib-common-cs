using System;
using System.Collections.Generic;

// DataUnits_TableModel - table model for displaying data table data in a JWorksheet.

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
	/// Table model for displaying data table data in a JWorksheet.
	/// </summary>
	public class DataUnits_TableModel : JWorksheet_AbstractRowTableModel
	{

	/// <summary>
	/// The table displayed in the worksheet.
	/// </summary>
	private IList<DataUnits> __dataUnitsList = null;

	/// <summary>
	/// Number of columns in the table model (with the alias).
	/// </summary>
	private int __COLUMNS = 9;

	/// <summary>
	/// Absolute column indices, for column lookups.
	/// </summary>
	public readonly int COL_DIMENSION = 0;
	public readonly int COL_IS_BASE = 1;
	public readonly int COL_ABBREVIATION = 2;
	public readonly int COL_SYSTEM = 3;
	public readonly int COL_LONG_NAME = 4;
	public readonly int COL_PRECISION = 5;
	public readonly int COL_MULT = 6;
	public readonly int COL_ADD = 7;
	public readonly int COL_SOURCE = 8;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dataUnitsList"> the list of data units to show in a worksheet. </param>
	/// <exception cref="NullPointerException"> if the dataTable is null. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public DataUnits_TableModel(java.util.List<DataUnits> dataUnitsList) throws Exception
	public DataUnits_TableModel(IList<DataUnits> dataUnitsList)
	{
		if (dataUnitsList == null)
		{
			_rows = 0;
		}
		else
		{
			_rows = dataUnitsList.Count;
		}
		__dataUnitsList = dataUnitsList;
	}

	/// <summary>
	/// From AbstractTableModel.  Returns the class of the data stored in a given
	/// column.  All values are treated as strings. </summary>
	/// <param name="columnIndex"> the column for which to return the data class. </param>
	public virtual Type getColumnClass(int columnIndex)
	{
		switch (columnIndex)
		{
			case COL_DIMENSION:
				return typeof(string);
			case COL_IS_BASE:
				return typeof(string);
			case COL_ABBREVIATION:
				return typeof(string);
			case COL_SYSTEM:
				return typeof(string);
			case COL_LONG_NAME:
				return typeof(string);
			case COL_PRECISION:
				return typeof(string);
			case COL_MULT:
				return typeof(string);
			case COL_ADD:
				return typeof(string);
			case COL_SOURCE:
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
			case COL_DIMENSION:
				return "\n\nDimension";
			case COL_IS_BASE:
				return "Is Base\nUnit for\nDimension?";
			case COL_ABBREVIATION:
				return "\n\nAbbreviation";
			case COL_SYSTEM:
				return "\nUnits\nSystem";
			case COL_LONG_NAME:
				return "\nLong\nName";
			case COL_PRECISION:
				return "\n\nPrecision";
			case COL_MULT:
				return "Base\nMultiply\nFactor";
			case COL_ADD:
				return "Base\nAdd\nFactor";
			case COL_SOURCE:
				return "\n\nSource for Units";
			default:
				return "";
		}
	}

	/// <summary>
	/// Return tool tips for the columns.
	/// </summary>
	public override string[] getColumnToolTips()
	{
		string[] tooltips = new string[__COLUMNS];
		tooltips[COL_DIMENSION] = "Dimension abbreviation (e.g., L=Length)";
		tooltips[COL_IS_BASE] = "Is the base unit? Add and multiply factors are relative to base units.";
		tooltips[COL_ABBREVIATION] = "Units abbreviation, often displayed with data.";
		tooltips[COL_SYSTEM] = "Whether English, SI, or universal units.";
		tooltips[COL_LONG_NAME] = "Long name for units.";
		tooltips[COL_PRECISION] = "Default precision for units in output.";
		tooltips[COL_MULT] = "Multiple of base units.";
		tooltips[COL_ADD] = "Shift from base units, for temperatures.";
		tooltips[COL_SOURCE] = "Source of the data units definition.";
		return tooltips;
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

		DataUnits dataUnits = __dataUnitsList[row];
		if (dataUnits == null)
		{
			return "";
		}
		switch (col)
		{
			case COL_DIMENSION:
				return dataUnits.getDimension().getAbbreviation();
			case COL_IS_BASE:
				if (dataUnits.getBaseFlag() == 1)
				{
					return "Y";
				}
				else
				{
					return "N";
				}
			case COL_ABBREVIATION:
				return dataUnits.getAbbreviation();
			case COL_SYSTEM:
				return dataUnits.getSystemString();
			case COL_LONG_NAME:
				return dataUnits.getLongName();
			case COL_PRECISION:
				return "" + dataUnits.getOutputPrecision();
			case COL_MULT:
				return "" + dataUnits.getMultFactor();
			case COL_ADD:
				return "" + dataUnits.getAddFactor();
			case COL_SOURCE:
				return "" + dataUnits.getSource();
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
		widths[COL_DIMENSION] = 7;
		widths[COL_IS_BASE] = 8;
		widths[COL_ABBREVIATION] = 8;
		widths[COL_SYSTEM] = 4;
		widths[COL_LONG_NAME] = 20;
		widths[COL_PRECISION] = 6;
		widths[COL_MULT] = 8;
		widths[COL_ADD] = 8;
		widths[COL_SOURCE] = 40;
		return widths;
	}

	}

}