using System;
using System.Collections.Generic;

// PropList_TableModel - table model for displaying prop list data.

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
// PropList_TableModel - table model for displaying prop list data.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2003-10-27	J. Thomas Sapienza, RTi	Initial version.
// 2004-11-29	JTS, RTi		* Added insertRowAt().
//					* Added addRow().
//					* Made the column reference numbers
//					  public for access in apps.
//					* Added deleteRow().
// 2005-04-26	JTS, RTi		Added finalize().
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.Util.IO
{

	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;
	using JWorksheet_TableModelListener = RTi.Util.GUI.JWorksheet_TableModelListener;

	/// <summary>
	/// This table model displays PropList data.  Currently it only handles PropLists
	/// that have String key/value pairs.
	/// The data model interactions are fully-handled in this class because PropList is not a simple
	/// list of data.
	/// <para>
	/// TODO (JTS - 2003-10-27) Add support for Object-storing props, or simply exclude them from being displayed.
	/// </para>
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class PropList_TableModel<T extends PropList> extends RTi.Util.GUI.JWorksheet_AbstractRowTableModel<T>
	public class PropList_TableModel<T> : JWorksheet_AbstractRowTableModel<T> where T : PropList
	{

	/// <summary>
	/// Number of columns in the table model.
	/// </summary>
	private readonly int __COLUMNS = 3;

	/// <summary>
	/// Reference to the column numbers.
	/// </summary>
	public readonly int COL_KEY = 1, COL_VAL = 2;

	/// <summary>
	/// Whether the table data is editable or not.
	/// </summary>
	private bool __keyEditable = true, __valEditable = true;

	/// <summary>
	/// The PropList for which data is displayed in the worksheet.
	/// </summary>
	private PropList __props;

	/// <summary>
	/// The column names.  They can be overridden by calling setKeyColumnName() and
	/// setValueColumnName(), but this must be done before the worksheet displaying the prop list is shown.
	/// </summary>
	private string __keyColName = "KEY", __valColName = "VALUE";

	/// <summary>
	/// Constructor. </summary>
	/// <param name="props"> the proplist that will be displayed in the table.  This proplist 
	/// will be duplicated for display so that changes can be accepted or rejected 
	/// by the user before being committed to the proplist read in from a file. </param>
	/// <param name="keyEditable"> whether the prop keys can be edited </param>
	/// <param name="valEditable"> whether the prop values can be edited </param>
	/// <exception cref="Exception"> if invalid data were passed in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public PropList_TableModel(T props, boolean keyEditable, boolean valEditable) throws Exception
	public PropList_TableModel(T props, bool keyEditable, bool valEditable)
	{
		if (props == null)
		{
			throw new Exception("Invalid proplist data passed to PropList_TableModel constructor.");
		}
		__props = new PropList(props);
		_rows = __props.size();

		__keyEditable = keyEditable;
		__valEditable = valEditable;
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="props"> the proplist that will be displayed in the table.  This proplist 
	/// will be duplicated for display so that changes can be accepted or rejected by 
	/// the user before being committed to the proplist read in from a file. </param>
	/// <param name="ignores"> a list of Strings representing keys that should not be 
	/// displayed in the table model.  Cannot be null. </param>
	/// <param name="keyEditable"> whether the prop keys can be edited </param>
	/// <param name="valEditable"> whether the prop values can be edited </param>
	/// <exception cref="Exception"> if invalid data were passed in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public PropList_TableModel(T props, java.util.List<String> ignores, boolean keyEditable, boolean valEditable) throws Exception
	public PropList_TableModel(T props, IList<string> ignores, bool keyEditable, bool valEditable)
	{
		if (props == null)
		{
			throw new Exception("Invalid proplist data passed to PropList_TableModel constructor.");
		}
		__props = new PropList(props);

		int size = ignores.Count;
		for (int i = 0; i < size; i++)
		{
			__props.unSet(ignores[i]);
		}

		_rows = __props.size();

		__keyEditable = keyEditable;
		__valEditable = valEditable;
	}

	/// <summary>
	/// Adds a row to the table; called by the worksheet when a call is made to 
	/// JWorksheet.addRow() or JWorksheet.insertRowAt(). </summary>
	/// <param name="prop"> the object (in this case, should only be a Prop) to insert because
	/// the data object is a full PropList. </param>
	/// <param name="row"> the row to insert the object at. </param>
	public virtual void addRow(Prop prop)
	{
		/*
		if (!(o instanceof Prop)) {
			Message.printWarning(2, "PropList_TableModel.addRow()",	
				"Only RTi.Util.IO.Prop objects can be added to a PropList table model.");
			return;
		}
		*/
		_rows++;
		__props.getList().Add(prop);
	}

	/// <summary>
	/// Deletes a row from the table; called by the worksheet when a call is made to JWorksheet.deleteRow(). </summary>
	/// <param name="row"> the number of the row to delete. </param>
	public override void deleteRow(int row)
	{
		if (_sortOrder != null)
		{
			row = _sortOrder[row];
		}

		_rows--;
		__props.getList().RemoveAt(row);
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~PropList_TableModel()
	{
		__props = null;
		__keyColName = null;
		__valColName = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the class of the data stored in a given column. </summary>
	/// <param name="columnIndex"> the column for which to return the data class. </param>
	public virtual Type getColumnClass(int columnIndex)
	{
		switch (columnIndex)
		{
			case COL_KEY:
				return typeof(string);
			case COL_VAL:
				return typeof(string);
		}
		return typeof(string);
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
			case COL_KEY:
				return __keyColName;
			case COL_VAL:
				return __valColName;
		}
		return " ";
	}

	/// <summary>
	/// Returns the text to be assigned to worksheet tooltips. </summary>
	/// <returns> a String array of tool tips. </returns>
	public override string[] getColumnToolTips()
	{
		string[] tips = new string[__COLUMNS];

		tips[COL_KEY] = "Property name.";
		tips[COL_VAL] = "Property value.";
		return tips;
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
			case 1:
				return "%-256s";
			case 2:
				return "%-256s";
		}
		return "%8s";
	}

	/// <summary>
	/// Returns the prop list of properties that are displayed in the worksheet. </summary>
	/// <returns> the prop list of properties that are displayed in the worksheet. </returns>
	public virtual PropList getPropList()
	{
		return __props;
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

		Prop p = __props.elementAt(row);
		switch (col)
		{
			case COL_KEY:
				return p.getKey();
			case COL_VAL:
				return p.getValue();
		}
		return " ";
	}

	/// <summary>
	/// Returns an array containing the widths (in number of characters) that the 
	/// fields in the table should be sized to. </summary>
	/// <returns> an integer array containing the widths for each field. </returns>
	public virtual int[] getColumnWidths()
	{
		int[] widths = new int[__COLUMNS];
		for (int i = 0; i < __COLUMNS; i++)
		{
			widths[i] = 0;
		}
		// TODO sam 2017-03-15 need to make the widths more intelligent
		widths[COL_KEY] = 20;
		widths[COL_VAL] = 80; // Make wide to handle long strings

		return widths;
	}

	/// <summary>
	/// Inserts a new row in the table; called by the worksheet when a call is made to JWorksheet.insertRowAt(). </summary>
	/// <param name="prop"> the object (in this case, should only be a Prop) to insert. </param>
	/// <param name="row"> the row to insert the object at. </param>
	public virtual void insertRowAt(Prop prop, int row)
	{
		if (_sortOrder != null)
		{
			row = _sortOrder[row];
		}

		/*
		if (!(prop instanceof Prop)) {
			Message.printWarning(2, "PropList_TableModel.insertRowAt()",	
				"Only RTi.Util.IO.Prop objects can be inserted to a PropList table model.");
			return;
		}
		*/
		_rows++;
		__props.getList().Insert(row, (Prop)prop);
	}

	/// <summary>
	/// Returns whether the cell at the given position is editable or not. </summary>
	/// <param name="rowIndex"> unused </param>
	/// <param name="columnIndex"> the index of the column to check for whether it is editable. </param>
	/// <returns> whether the cell at the given position is editable. </returns>
	public virtual bool isCellEditable(int rowIndex, int columnIndex)
	{
		if (columnIndex == COL_KEY)
		{
			if (!__keyEditable)
			{
				return false;
			}
			return true;
		}
		if (columnIndex == COL_VAL)
		{
			if (!__valEditable)
			{
				return false;
			}
			return true;
		}
		return true;
	}

	/// <summary>
	/// Overrides the default name of the key column ("KEY") -- THIS MUST BE DONE BEFORE
	/// THE WORKSHEET IS SHOWN IN THE GUI OR IT WILL NOT WORK. </summary>
	/// <param name="name"> the name to give to the column. </param>
	public virtual void setKeyColumnName(string name)
	{
		__keyColName = name;
	}

	/// <summary>
	/// Overrides the default name of the value column ("VALUE") -- THIS MUST BE DONE 
	/// BEFORE THE WORKSHEET IS SHOWN IN THE GUI OR IT WILL NOT WORK. </summary>
	/// <param name="name"> the name to give to the column. </param>
	public virtual void setValueColumnName(string name)
	{
		__valColName = name;
	}

	/// <summary>
	/// Sets the value at the specified position to the specified value. </summary>
	/// <param name="value"> the value to set the cell to. </param>
	/// <param name="row"> the row of the cell for which to set the value. </param>
	/// <param name="col"> the col of the cell for which to set the value. </param>
	public virtual void setValueAt(object value, int row, int col)
	{
		Prop p = (Prop)__props.elementAt(row);

		switch (col)
		{
			case COL_KEY:
				if (!(p.getKey().Equals((string)value)))
				{
					p.setKey((string)value);
					valueChanged(row, col);
				}
				break;
			case COL_VAL:
				if (!(p.getValue().Equals((string)value)))
				{
					p.setValue((string)value);
					valueChanged(row, col);
				}
				break;
		}

		base.setValueAt(value, row, col);
	}

	/// <summary>
	/// Called when one of the properties is edited. </summary>
	/// <param name="row"> the row of the property that was edited. </param>
	/// <param name="col"> the column of the property that was edited. </param>
	private void valueChanged(int row, int col)
	{
		int size = _listeners.size();
		JWorksheet_TableModelListener tml = null;
		for (int i = 0; i < size; i++)
		{
			tml = _listeners.get(i);
			tml.tableModelValueChanged(row, col, null);
		}
	}

	}

}