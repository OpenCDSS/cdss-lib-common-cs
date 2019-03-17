using System;

// DataTable_TableModel - table model for displaying data table data in a JWorksheet

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

namespace RTi.Util.Table
{

	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;
	using IOUtil = RTi.Util.IO.IOUtil;
	using Message = RTi.Util.Message.Message;
	using DateTime = RTi.Util.Time.DateTime;

	/// <summary>
	/// Table model for displaying data table data in a JWorksheet.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class DataTable_TableModel extends RTi.Util.GUI.JWorksheet_AbstractRowTableModel
	public class DataTable_TableModel : JWorksheet_AbstractRowTableModel
	{

	/// <summary>
	/// The classes of the fields, stored in an array for quicker access.
	/// </summary>
	private Type[] __fieldClasses;

	/// <summary>
	/// The field types as per the table field types.
	/// </summary>
	private int[] __fieldTypes;

	/// <summary>
	/// The table displayed in the worksheet.
	/// </summary>
	private DataTable __dataTable;

	/// <summary>
	/// The number of columns in the table model.
	/// </summary>
	private int __columns = 0;

	/// <summary>
	/// The formats of the table fields, stored in an array for quicker access.
	/// </summary>
	private string[] __fieldFormats;

	/// <summary>
	/// The names of the table fields, stored in the array for quicker access.
	/// </summary>
	private string[] __fieldNames;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dataTable"> the table to show in a worksheet. </param>
	/// <exception cref="NullPointerException"> if the dataTable is null. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public DataTable_TableModel(DataTable dataTable) throws Exception
	public DataTable_TableModel(DataTable dataTable)
	{
		if (dataTable == null)
		{
			throw new System.NullReferenceException();
		}

		__dataTable = dataTable;
		_rows = __dataTable.getNumberOfRecords();
		__columns = __dataTable.getNumberOfFields();

		__fieldNames = __dataTable.getFieldNames();
		__fieldFormats = __dataTable.getFieldFormats();
		__fieldTypes = __dataTable.getFieldDataTypes();
		__fieldClasses = determineClasses(__fieldTypes);
	}

	/// <summary>
	/// Determines the kind of classes stored in each table field. </summary>
	/// <param name="dataTypes"> the data types array from the data table. </param>
	/// <returns> an array of the Class of each field. </returns>
	private Type[] determineClasses(int[] dataTypes)
	{
		Type[] classes = new Type[dataTypes.Length];

		for (int i = 0; i < dataTypes.Length; i++)
		{
			if (__dataTable.isColumnArray(dataTypes[i]))
			{
				classes[i] = typeof(string);
			}
			else
			{
				switch (dataTypes[i])
				{
					case TableField.DATA_TYPE_ARRAY:
						// For the purposes of rendering in the table, treat array as formatted string "[ , , , ]"
						classes[i] = typeof(string);
						break;
					case TableField.DATA_TYPE_BOOLEAN:
						classes[i] = typeof(Boolean);
						break;
					case TableField.DATA_TYPE_INT:
						classes[i] = typeof(Integer);
						break;
					case TableField.DATA_TYPE_SHORT:
						classes[i] = typeof(Short);
						break;
					case TableField.DATA_TYPE_DOUBLE:
						classes[i] = typeof(Double);
						break;
					case TableField.DATA_TYPE_FLOAT:
						classes[i] = typeof(Float);
						break;
					case TableField.DATA_TYPE_STRING:
						classes[i] = typeof(string);
						break;
					case TableField.DATA_TYPE_DATE:
						classes[i] = typeof(System.DateTime);
						break;
					case TableField.DATA_TYPE_DATETIME:
						classes[i] = typeof(DateTime);
						break;
					case TableField.DATA_TYPE_LONG:
						classes[i] = typeof(Long);
						break;
					default:
						throw new Exception("TableField data type " + dataTypes[i] + " is not supported in DataTable table model.");
				}
			}
		}
		return classes;
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~DataTable_TableModel()
	{
		IOUtil.nullArray(__fieldClasses);
		__dataTable = null;
		IOUtil.nullArray(__fieldFormats);
		IOUtil.nullArray(__fieldNames);
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the class of the data stored in a given column. </summary>
	/// <param name="columnIndex"> the column for which to return the data class. </param>
	public virtual Type getColumnClass(int columnIndex)
	{
		return __fieldClasses[columnIndex];
	}

	/// <summary>
	/// Returns the number of columns of data. </summary>
	/// <returns> the number of columns of data. </returns>
	public virtual int getColumnCount()
	{
		return __columns;
	}

	/// <summary>
	/// Returns the name of the column at the given position. </summary>
	/// <param name="columnIndex"> the position of the column for which to return the name. </param>
	/// <returns> the name of the column at the given position. </returns>
	public virtual string getColumnName(int columnIndex)
	{
		string prefix = "";
		if (_worksheet != null)
		{
			prefix = _worksheet.getColumnPrefix(columnIndex);
		}
		return prefix + __fieldNames[columnIndex];
	}

	/// <summary>
	/// Returns an array containing the column tool tips. </summary>
	/// <returns> a String array containing the tool tips for each field (the field descriptions are used). </returns>
	public override string[] getColumnToolTips()
	{
		string[] tips = new string[__columns];
		for (int i = 0; i < __columns; i++)
		{
			tips[i] = __dataTable.getTableField(i).getDescription();
		}
		return tips;
	}

	/// <summary>
	/// Returns an array containing the widths (in number of characters) that the 
	/// fields in the table should be sized to. </summary>
	/// <returns> an integer array containing the widths for each field. </returns>
	public virtual int[] getColumnWidths()
	{
		int[] widths = new int[__columns];
		for (int i = 0; i < __columns; i++)
		{
			widths[i] = __dataTable.getFieldWidth(i);
			if (widths[i] < 0)
			{
				widths[i] = 15; // Default
			}
		}
		return widths;
	}

	/// <summary>
	/// Returns the format to be applied to data values in the column, for display in the table.
	/// If the column contains an array, the format applies to the individual values in the array. </summary>
	/// <param name="column"> column for which to return the format. </param>
	/// <returns> the format (as used by StringUtil.formatString() in which to display the column. </returns>
	public override string getFormat(int column)
	{
		switch (__fieldTypes[column])
		{
			case TableField.DATA_TYPE_ARRAY:
				// For the purposes of rendering in the table, treat array as formatted string
				return "%s";
			default:
				return __fieldFormats[column];
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

		try
		{
			if (__dataTable.isColumnArray(__fieldTypes[col]))
			{
				// Column is an array of primitive types
				return __dataTable.formatArrayColumn(row,col);
			}
			else
			{
				return __dataTable.getFieldValue(row, col);
			}
		}
		catch (Exception e)
		{
			Message.printWarning(3, "getValueAt", "Error processing column \"" + getColumnName(col) + "\"");
			Message.printWarning(3, "getValueAt", e);
			return "";
		}
	}

	/// <summary>
	/// Returns whether the cell at the given position is editable or not.  In this
	/// table model all columns above #2 are editable. </summary>
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
		base.setValueAt(value, row, col);
	}

	}

}