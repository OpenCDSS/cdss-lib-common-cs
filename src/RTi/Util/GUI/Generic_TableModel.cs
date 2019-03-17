using System;

// Generic_TableModel - table model for a generic worksheet

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
// Generic_TableModel - Table Model for a generic worksheet.  Manages 
// a Vector of GenericWorksheetData objects.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2003-12-10	J. Thomas Sapienza, RTi	Initial version.
// 2005-03-23	JTS, RTi		Removed reference to the 0th (row
//					count) column.
// 2005-04-26	JTS, RTi		Added finalize().
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.Util.GUI
{

	using DMIUtil = RTi.DMI.DMIUtil;

	using IOUtil = RTi.Util.IO.IOUtil;

	/// <summary>
	/// This class is a table model for displaying GenericWorksheetData objects in
	/// a worksheet.  Unlike hard-coded table models (which are useful for more 
	/// complicated worksheets), all the settings for this table model, such as 
	/// column names, column formats, etc, can be set at run time.<para>
	/// </para>
	/// <b>How to Use a Generic Table Model</b><para>
	/// There are several ways to go about creating a worksheet that displays data 
	/// from GenericWorksheetObjects.  This first method can be used for when 
	/// </para>
	/// there is no actual data ready with which to populate the table model:<para>
	/// <pre>
	/// // create a table model that will have 3 data fields
	/// Generic_TableModel model = new Generic_TableModel(3);
	/// 
	/// // at this point, the class information for each field must be set.
	/// // There are two ways to go about this.  
	/// // First (if no GenericWorksheetData objects representative of
	/// // what will be stored in the worksheet are available):
	/// if (haveNoRepresentativeObjects) {
	///		model.setColumnClass(0, String.class);
	///		model.setColumnClass(1, Integer.class);
	///		model.setColumnClass(2, Double.class);
	/// }
	/// // but if an object representative of the objects that will be
	/// // stored in the worksheet is available (and has NO null values
	/// // stored in it), the following can be done:
	/// else {
	///		model.determineColumnClasses(representativeGenericData);
	/// }
	/// 
	/// // set up the column information (name, format, etc)
	/// model.setColumnInformation(0, "String column", "%4.4s", 10, true);
	/// model.setColumnInformation(1, "Integer column", "%8d", 8, false);
	/// model.setColumnInformation(2, "Double column", "%10.2f", 12, true);
	/// 
	/// // create the cell renderer
	/// Generic_CellRenderer renderer = new Generic_CellRenderer(model);
	/// 
	/// // create the worksheet -- from this point on out, it's the same
	/// // as working with any other worksheet.
	/// __worksheet = new JWorksheet(renderer, model, propList);
	/// </pre>
	/// </para>
	/// <para>
	/// If, however, there is a Vector of data objects to be placed in the worksheet,
	/// and the first GenericWorksheetData object in the Vector has NO null values,
	/// </para>
	/// the following can be done:<para>
	/// <pre>
	/// // create the table model.  This will automatically determine the 
	/// // column classes from the first element in the Vector.
	/// Generic_TableModel model = new Generic_TableModel(vector);
	/// 
	/// // set up the column information (name, format, etc)
	/// model.setColumnInformation(0, "String column", "%4.4s", 10, true);
	/// model.setColumnInformation(1, "Integer column", "%8d", 8, false);
	/// model.setColumnInformation(2, "Double column", "%10.2f", 12, true);
	/// 
	/// // create the cell renderer
	/// Generic_CellRenderer renderer = new Generic_CellRenderer(model);
	/// 
	/// // create the worksheet -- from this point on out, it's the same
	/// // as working with any other worksheet.
	/// __worksheet = new JWorksheet(renderer, model, propList);
	/// </pre>
	/// </para>
	/// <para>
	/// </para>
	/// <b>Adding a new Row of Data to a Generic Table</b><para>
	/// Adding a new row of data is a little trickier than normal, simply because
	/// the order and classes of the fields in the new GenericWorksheetData object
	/// </para>
	/// must match exactly all the others in the worksheet.<para>
	/// </para>
	/// The first way to do this is to manually create a new (and empty) data object:<para>
	/// <pre>
	/// GenericWorksheetData d = new GenericWorksheetData(3);
	/// d.setValueAt(0, "test");
	/// d.setValueAt(1, new Integer(111));
	/// d.setValueAt(2, new Double(999));
	/// __worksheet.addRow(d);
	/// </pre>
	/// </para>
	/// <para>
	/// The alternate way is to use a method in the GenericWorksheetData class to
	/// </para>
	/// have it build an empty data object:<para>
	/// <pre>
	/// // get the first element in the worksheet in order to build a new one
	/// GenericWorksheetData d =(GenericWorksheetData)__worksheet.getRowData(0);
	/// 
	/// GenericWorksheetData newRow = d.getEmptyGenericWorksheetData();
	/// __worksheet.addRow(newRow);
	/// </pre>
	/// </para>
	/// </summary>
	public class Generic_TableModel : JWorksheet_AbstractRowTableModel
	{

	/// <summary>
	/// Array of whether each column is editable or not.
	/// </summary>
	private bool[] __editable = null;

	/// <summary>
	/// Whether any tool tips have been set yet.
	/// </summary>
	private bool __tipsSet = false;

	/// <summary>
	/// Array storing the kind of data class for each column.
	/// </summary>
	private Type[] __classes = null;

	/// <summary>
	/// Array of the column widths for each column.
	/// </summary>
	private int[] __widths = null;

	/// <summary>
	/// Number of columns in the table model.
	/// </summary>
	private int __COLUMNS = 0;

	/// <summary>
	/// Array of the formatting information for use by StringUtil.format() for each
	/// column.
	/// </summary>
	private string[] __formats = null;

	/// <summary>
	/// Array of the names of each column.
	/// </summary>
	private string[] __names = null;

	/// <summary>
	/// Array of the tool tips for each column.
	/// </summary>
	private string[] __toolTips = null;

	/// <summary>
	/// Constructor.  
	/// Sets up a table model for the given number of columns.  The
	/// Class for each column <b>must</b> be set with setColumnClass() prior to
	/// putting this model in a worksheet, or a call should be made to
	/// determineColumnClasses(GenericWorksheetData), with a GenericWorksheetData
	/// object that has no null values. </summary>
	/// <param name="columns"> the number of columns in the table model. </param>
	public Generic_TableModel(int columns)
	{
		__COLUMNS = columns;
		_rows = 0;

		initialize();
	}

	/// <summary>
	/// Constructor.  
	/// Sets up a table model and determines the number of columns from the first 
	/// value in the data Vector.  If the data Vector is empty or null, 
	/// an exception is thrown because the number of columns cannot be determined.<para>
	/// The class type for each field is determined from the Classes of the data in 
	/// the first element of the Vector, so the first element of the data Vector cannot have any null values.  
	/// </para>
	/// </summary>
	/// <param name="data"> the Vector of GenericWorksheetData values to start the worksheet
	/// with.  Must have a size &gt;0 and be non-null.  The first element cannot have
	/// any null values. </param>
	/// <exception cref="Exception"> if a null or 0-size data Vector is passed in, or if the
	/// first element of the Vector has any null values. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Generic_TableModel(java.util.List data) throws Exception
	public Generic_TableModel(System.Collections.IList data)
	{
		if (data == null || data.Count == 0)
		{
			throw new Exception("Cannot determine number of columns.");
		}
		else
		{
			GenericWorksheetData d = (GenericWorksheetData)data[0];
			__COLUMNS = d.getColumnCount();
			_data = data;
			_rows = data.Count;

			initialize();

			determineColumnClasses();
		}
	}

	/// <summary>
	/// Determines the kinds of classes for each column of data from the first element
	/// in the internal data Vector.
	/// </summary>
	public virtual void determineColumnClasses()
	{
		if (_data.size() == 0)
		{
			return;
		}

		determineColumnClasses((GenericWorksheetData)_data.get(0));
	}

	/// <summary>
	/// Determines the kinds of classes for each column of data from the specified
	/// GenericWorksheetData object.  It cannot contain any null values. </summary>
	/// <param name="data"> the GenericWorksheetData object from which to determine the column classes. </param>
	public virtual void determineColumnClasses(GenericWorksheetData data)
	{
		for (int i = 0; i < __COLUMNS; i++)
		{
			__classes[i] = (data.getValueAt(i)).GetType();
		}
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~Generic_TableModel()
	{
		__editable = null;
		IOUtil.nullArray(__classes);
		__widths = null;
		IOUtil.nullArray(__formats);
		IOUtil.nullArray(__names);
		IOUtil.nullArray(__toolTips);
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the class of the data stored in a given column. </summary>
	/// <param name="column"> the column for which to return the data class. </param>
	public virtual Type getColumnClass(int column)
	{
		return __classes[column];
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
	public virtual string getColumnName(int column)
	{
		return __names[column];
	}

	/// <summary>
	/// Returns the tool tips for the columns, or null if none have been set. </summary>
	/// <returns> the tool tips for the columns, or null if none have been set. </returns>
	public override string[] getColumnToolTips()
	{
		if (!__tipsSet)
		{
			return null;
		}
		for (int i = 0; i < __COLUMNS; i++)
		{
			if (!string.ReferenceEquals(__toolTips[i], null))
			{
				return __toolTips;
			}
		}
		return null;
	}

	/// <summary>
	/// Returns an array containing the widths (in number of characters) that the 
	/// fields in the table should be sized to. </summary>
	/// <returns> an integer array containing the widths for each field. </returns>
	public virtual int[] getColumnWidths()
	{
		return __widths;
	}

	/// <summary>
	/// Returns the format that the specified column should be displayed in when
	/// the table is being displayed in the given table format. </summary>
	/// <param name="column"> column for which to return the format. </param>
	/// <returns> the format (as used by StringUtil.formatString() in which to display the column. </returns>
	public override string getFormat(int column)
	{
		return __formats[column];
	}

	/// <summary>
	/// Returns the number of rows of data in the table.
	/// </summary>
	public virtual int getRowCount()
	{
		return _rows;
	}

	/// <summary>
	/// Returns the data that should be placed in the worksheet at the given row and column. </summary>
	/// <param name="row"> the row for which to return data. </param>
	/// <param name="col"> the column for which to return data. </param>
	/// <returns> the data that should be placed in the worksheet at the given row and column. </returns>
	public virtual object getValueAt(int row, int col)
	{
		if (_sortOrder != null)
		{
			row = _sortOrder[row];
		}

		GenericWorksheetData d = (GenericWorksheetData)_data.get(row);
		return d.getValueAt(col);
	}

	/// <summary>
	/// Initializes the data arrays.  
	/// </summary>
	private void initialize()
	{
		__classes = new Type[__COLUMNS];
		__names = new string[__COLUMNS];

		for (int i = 0; i < __COLUMNS; i++)
		{
			__names[i] = "COLUMN " + i;
		}

		__formats = new string[__COLUMNS];

		for (int i = 0; i < __COLUMNS; i++)
		{
			__formats[i] = "";
		}

		__toolTips = new string[__COLUMNS];
		__widths = new int[__COLUMNS];

		for (int i = 0; i < __COLUMNS; i++)
		{
			__widths[i] = 8;
		}

		__editable = new bool[__COLUMNS];
		for (int i = 0; i < __COLUMNS; i++)
		{
			__editable[i] = false;
		}
	}

	/// <summary>
	/// Returns whether the specified cell is editable or not. </summary>
	/// <returns> whether the specified cell is editable or not. </returns>
	public virtual bool isCellEditable(int row, int col)
	{
		return __editable[col];
	}

	/// <summary>
	/// Sets the column class for the specified column. </summary>
	/// <param name="column"> the column for which to set the class.  Cannot change the class
	/// of column 0. </param>
	/// <param name="c"> the Class to set for the column. </param>
	public virtual void setColumnClass(int column, Type c)
	{
		__classes[column] = c;
	}

	/// <summary>
	/// Sets whether the specified column is editable or not. </summary>
	/// <param name="column"> the column to set editable.  Cannot change the editability of 
	/// column 0. </param>
	/// <param name="editable"> whether the column is editable (true) or not. </param>
	public virtual void setColumnEditable(int column, bool editable)
	{
		__editable[column] = editable;
	}

	/// <summary>
	/// Sets the format (as used by StringUtil.format()) for the specified column. </summary>
	/// <param name="column"> the column to set the editability of.  Cannot change the format
	/// of column 0. </param>
	/// <param name="format"> the format to set for the column. </param>
	public virtual void setColumnFormat(int column, string format)
	{
		__formats[column] = format;
	}

	/// <summary>
	/// Sets the column name, format, and width at once. </summary>
	/// <param name="column"> the column to set the values for. </param>
	/// <param name="name"> the name to give the column. </param>
	/// <param name="format"> the format to use in the column. </param>
	/// <param name="width"> the width to set the column to. </param>
	public virtual void setColumnInformation(int column, string name, string format, int width)
	{
		setColumnName(column, name);
		setColumnFormat(column, format);
		setColumnWidth(column, width);
	}

	/// <summary>
	/// Sets the column name, format, width, and tool tip at once. </summary>
	/// <param name="column"> the column to set the values for. </param>
	/// <param name="name"> the name to give the column. </param>
	/// <param name="format"> the format to use in the column. </param>
	/// <param name="width"> the width to set the column to. </param>
	/// <param name="toolTip"> the toolTip to set on the column. </param>
	public virtual void setColumnInformation(int column, string name, string format, int width, string toolTip)
	{
		setColumnName(column, name);
		setColumnFormat(column, format);
		setColumnWidth(column, width);
		setColumnToolTip(column, toolTip);
	}

	/// <summary>
	/// Sets the column name, format, width and editability at once. </summary>
	/// <param name="column"> the column to set the values for. </param>
	/// <param name="name"> the name to give the column. </param>
	/// <param name="format"> the format to use in the column. </param>
	/// <param name="width"> the width to set the column to. </param>
	/// <param name="editable"> whether the column is editable or not. </param>
	public virtual void setColumnInformation(int column, string name, string format, int width, bool editable)
	{
		setColumnName(column, name);
		setColumnFormat(column, format);
		setColumnWidth(column, width);
		setColumnEditable(column, editable);
	}

	/// <summary>
	/// Sets the column name, format, width, tool tip and editability at once. </summary>
	/// <param name="column"> the column to set the values for. </param>
	/// <param name="name"> the name to give the column. </param>
	/// <param name="format"> the format to use in the column. </param>
	/// <param name="width"> the width to set the column to. </param>
	/// <param name="toolTip"> the toolTip to set on the column. </param>
	/// <param name="editable"> whether the column is editable or not. </param>
	public virtual void setColumnInformation(int column, string name, string format, int width, string toolTip, bool editable)
	{
		setColumnName(column, name);
		setColumnFormat(column, format);
		setColumnWidth(column, width);
		setColumnToolTip(column, toolTip);
		setColumnEditable(column, editable);
	}

	/// <summary>
	/// Sets the name of the specified column. </summary>
	/// <param name="column"> the column to set the name for. </param>
	/// <param name="name"> the name to set for the column. </param>
	public virtual void setColumnName(int column, string name)
	{
		__names[column] = name;
	}

	/// <summary>
	/// Sets the tool tip on the specified column. </summary>
	/// <param name="column"> the column to set the tool tip for. </param>
	/// <param name="tip"> the tool tip to set on the column. </param>
	public virtual void setColumnToolTip(int column, string tip)
	{
		__toolTips[column] = tip;
		__tipsSet = true;
	}

	/// <summary>
	/// Sets the width of the specified column. </summary>
	/// <param name="column"> the column the set the width for. </param>
	/// <param name="width"> the width to set on the column. </param>
	public virtual void setColumnWidth(int column, int width)
	{
		if (width < 0)
		{
			return;
		}
		__widths[column] = width;
	}

	/// <summary>
	/// Sets the value in the table model data at the specified position. </summary>
	/// <param name="value"> the value to set. </param>
	/// <param name="row"> the row of data in which to set the value. </param>
	/// <param name="col"> the column of data in which to set the value. </param>
	public virtual void setValueAt(object value, int row, int col)
	{
		if (_sortOrder != null)
		{
			row = _sortOrder[row];
		}

		GenericWorksheetData d = (GenericWorksheetData)_data.get(row);

		if (value == null)
		{
			Type c = __classes[col];
			if (c == typeof(string))
			{
				value = DMIUtil.MISSING_STRING;
			}
			else if (c == typeof(Double))
			{
				value = new double?(DMIUtil.MISSING_DOUBLE);
			}
			else if (c == typeof(Integer))
			{
				value = new int?(DMIUtil.MISSING_INT);
			}
			else if (c == typeof(System.DateTime))
			{
				value = DMIUtil.MISSING_DATE;
			}
		}

		d.setValueAt(col, value);

		base.setValueAt(value, row, col);
	}

	}

}