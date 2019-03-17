using System.Collections.Generic;

// JWorksheet_AbstractTableModel - base class for JWorksheet table models

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

namespace RTi.Util.GUI
{

	// TODO sam 2017-03-15 Need to make the data private and set the data via a constructor,
	// and allow a constructor with no data since that may be managed in a child class.

	/// <summary>
	/// This is the class from which all the classes that will be used as 
	/// TableModels in a JWorksheet should be used.  It implements a few core 
	/// data members that all those classes should have, including some sorting support. 
	/// <para>
	/// TODO (JTS - 2006-05-25) If I could do this over, I would combine this table model with 
	/// AbstractRowTableModel, in order to simplify things.  I don't see a very 
	/// good reason to require both of these, honestly.
	/// </para>
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public abstract class JWorksheet_AbstractTableModel<T> extends javax.swing.table.AbstractTableModel
	public abstract class JWorksheet_AbstractTableModel<T> : AbstractTableModel
	{

	/// <summary>
	/// Holds the sorted order of the records to be displayed.
	/// </summary>
	protected internal int[] _sortOrder = null;

	/// <summary>
	/// The number of rows in the results.
	/// </summary>
	protected internal int _rows = 0;

	/// <summary>
	/// The type of format in which the columns should be displayed.
	/// </summary>
	protected internal int _type = -1;

	/// <summary>
	/// The worksheet that this table model works with.  This is here so that it 
	/// can be set by derived classes.  This class provides a built-in List<T> data array
	/// that allows basic table model interaction, such as interacting with rows.
	/// If a List<T> is not appropriate for the data model, then manage the data
	/// in the child class and provide appropriate methods.
	/// TODO (JTS - 2004-11-30) remove?  subclasses may be using this ...
	/// TODO (JTS - 2005-03-30) no, leave in, and have it called automatically by the worksheet whenever a model
	/// is set in it.  add a setWorksheet() method.
	/// </summary>
	protected internal JWorksheet _worksheet;

	/// <summary>
	/// List of integer arrays denoting the cells whose default editability has
	/// been overridden.  The int[] arrays consist of:<br><pre>
	/// 0 - the row of the cell
	/// 1 - the column of the cell
	/// 2 - a 1 if the cell is editable, 0 if it is not
	/// </pre>
	/// </summary>
	protected internal IList<int[]> _cellEditOverride = new List<int[]>();

	/// <summary>
	/// The data that will be shown in the table.
	/// </summary>
	protected internal IList<T> _data = new List<T>();

	/// <summary>
	/// The list of table model listeners.
	/// </summary>
	protected internal IList<JWorksheet_TableModelListener> _listeners = new List<JWorksheet_TableModelListener>();

	/// <summary>
	/// Adds the object to the table model. </summary>
	/// <param name="o"> the object to add to the end of the results list. </param>
	public virtual void addRow(T o)
	{
		if (_data == null)
		{
			_data = new List<T>();
			_data.Add(o);
		}
		else
		{
			_data.Add(o);
		}
		_rows++;
	}

	/// <summary>
	/// Adds a table model listener to the list of table model listeners. </summary>
	/// <param name="listener"> the listener to add </param>
	public virtual void addTableModelListener(JWorksheet_TableModelListener listener)
	{
		_listeners.Add(listener);
	}

	/// <summary>
	/// Clears the data from the table model and empties the table.
	/// </summary>
	public virtual void clear()
	{
		_data = null;
		_rows = 0;
	}

	/// <summary>
	/// Removes a row's editability override and lets the normal cell editing rules take effect. </summary>
	/// <param name="row"> the row for which to remove the cell editability. </param>
	public virtual void clearOverrideCellEdit(int row)
	{
		int size = _cellEditOverride.Count;

		IList<int> removes = new List<int>();
		if (size > 0)
		{
			int[] temp;
			for (int i = 0; i < size; i++)
			{
				temp = (int[])_cellEditOverride[i];
				if (temp[0] == row)
				{
					removes.Add(new int?(i));
				}
			}
		}

		size = removes.Count;
		for (int i = (size - 1); i >= 0; i--)
		{
			_cellEditOverride.RemoveAt(row);
		}
	}

	/// <summary>
	/// Deletes the specified row from the table model. </summary>
	/// <param name="row"> the row to delete. </param>
	public virtual void deleteRow(int row)
	{
		if (_data == null)
		{
			return;
		}
		if (row < 0 || row > _data.Count)
		{
			return;
		}
		_rows--;
		_data.RemoveAt(row);

		clearOverrideCellEdit(row);
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~JWorksheet_AbstractTableModel()
	{
		_sortOrder = null;
		_cellEditOverride = null;
		_data = null;
		_listeners = null;
		_worksheet = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Dummy version of the method to get column tool tips for a worksheet.  This one
	/// just returns null, meaning that no tool tips are to be set up.
	/// </summary>
	public virtual string[] getColumnToolTips()
	{
		return null;
	}

	/// <summary>
	/// Used for reading consecutive values from table models in which the values of 
	/// one row depend on the values of the rows before them (e.g., Time Series dates
	/// or running averages).  When a consecutive read is made, the table model is 
	/// guaranteed that the previous row that was operated on was either the current
	/// row (the column could be different) or the previous row.<para>
	/// </para>
	/// The default implementation of this method simply pipes through the call to getValueAt(row, column).<para>
	/// <b>Note:</b> if a class overrides this method, it should also override startNewConsecutiveRead().
	/// </para>
	/// </summary>
	/// <param name="row"> the row from which to read data </param>
	/// <param name="column"> the column from which to read data </param>
	/// <returns> the data read from the given row and column. </returns>
	public virtual object getConsecutiveValueAt(int row, int column)
	{
		return getValueAt(row, column);
	}

	/// <summary>
	/// Returns the data stored in the table model. </summary>
	/// <returns> the data stored in the table model. </returns>
	public virtual IList<T> getData()
	{
		return _data;
	}

	/// <summary>
	/// Returns the format in which the given column should be formatted. </summary>
	/// <param name="absoluteColumn"> the absolute column for which to return the format. </param>
	/// <returns> the format in which the given column should be formatted. </returns>
	public abstract string getFormat(int absoluteColumn);

	/// <summary>
	/// Inserts a row at the specified position. </summary>
	/// <param name="o"> the object that stores a row of data. </param>
	/// <param name="pos"> the position at which to insert the row. </param>
	public virtual void insertRowAt(T o, int pos)
	{
		if (_data == null)
		{
			_data = new List<T>();
			_data.Add(o);
		}
		else
		{
			_data.Insert(pos, o);
		}
		_rows++;
	}

	/// <summary>
	/// Return the sort order array.  If the data in the table have been sorted, this array
	/// is needed to access to original data in the proper order.
	/// </summary>
	public virtual int [] getSortOrder()
	{
		return _sortOrder;
	}

	/// <summary>
	/// Overrides the default cell editability of the specified cell and sets the 
	/// cell to be editable or not depending on the value of state. </summary>
	/// <param name="row"> row of the cell </param>
	/// <param name="column"> column of the cell </param>
	/// <param name="state"> whether the cell should be editable (true) or not (false) </param>
	public virtual void overrideCellEdit(int row, int column, bool state)
	{
		int size = _cellEditOverride.Count;

		if (size > 0)
		{
			int[] temp;
			for (int i = 0; i < size; i++)
			{
				temp = (int[])_cellEditOverride[i];
				if (temp[0] == row && temp[1] == column)
				{
					if (state)
					{
						temp[2] = 1;
					}
					else
					{
						temp[2] = 0;
					}
					return;
				}
			}
		}

		int[] cell = new int[3];
		cell[0] = row;
		cell[1] = column;
		if (state)
		{
			cell[2] = 1;
		}
		else
		{
			cell[2] = 0;
		}
		_cellEditOverride.Add(cell);
	}

	/// <summary>
	/// Removes a table model listener from the list of table model listeners. </summary>
	/// <param name="listener"> the table model listener to remove. </param>
	public virtual void removeTableModelListener(JWorksheet_TableModelListener listener)
	{
		_listeners.Remove(listener);
	}

	/// <summary>
	/// Sets new data into the table model (used if many rows change at once or all-new data is to be shown. </summary>
	/// <param name="data"> the list of data objects to be displayed in rows of the table. </param>
	public virtual void setNewData(IList<T> data)
	{
		_data = data;
		if (data == null)
		{
			_rows = 0;
		}
		else
		{
			_rows = _data.Count;
		}
	}

	/// <summary>
	/// Changes a row of data by replacing the old row with the new data. </summary>
	/// <param name="o"> the object that stores a row of data. </param>
	/// <param name="pos"> the position at which to set the data row. </param>
	public virtual void setRowData(T o, int pos)
	{
		if (_data == null)
		{
			_data = new List<T>();
			_data.Add(o);
		}
		else
		{
			_data[pos] = o;
		}
	}

	/// <summary>
	/// Sets the sorted order for the records to be displayed in.  This method is in
	/// this base class for all the table models used in a JWorksheet so that the 
	/// base JWorksheet can be sure that all of its models will have this method. </summary>
	/// <param name="sortOrder"> the sorted order in which records should be displayed.  
	/// The record that would normally go at position X will now be placed at position 'sortOrder[x]' </param>
	public virtual void setSortedOrder(int[] sortOrder)
	{
		_sortOrder = sortOrder;
	}

	internal bool __shouldDoGetConsecutiveValueAt = false;

	public virtual bool shouldDoGetConsecutiveValueAt()
	{
		return __shouldDoGetConsecutiveValueAt;
	}

	public virtual void shouldDoGetConsecutiveValueAt(bool should)
	{
		__shouldDoGetConsecutiveValueAt = should;
	}

	/// <summary>
	/// TODO SAM 2014-03-30 What does this do?
	/// </summary>
	public bool __shouldResetGetConsecutiveValueAt = false;

	public virtual bool shouldResetGetConsecutiveValueAt()
	{
		return __shouldResetGetConsecutiveValueAt;
	}

	public virtual void shouldResetGetConsecutiveValueAt(bool should)
	{
		__shouldResetGetConsecutiveValueAt = should;
	}

	/// <summary>
	/// Used to notify the table model of a new consecutive row read.  Consecutive reads
	/// might be used in places where the values of a row depend on the values of the
	/// previous rows (e.g., Time Series dates, running averages).  In a consecutive
	/// read, the table model is guaranteed that the row in the call to
	/// getValueAt() is either the same row that was read from last, or one greater
	/// than the row that was read from last time.
	/// </summary>
	public virtual void startNewConsecutiveRead()
	{
	}

	/// <summary>
	/// Can be called upon changing values in a table model.  Notifies all
	/// JWorksheet_TableModelListener listeners
	/// of the change.  Will not be called automatically by the table model. </summary>
	/// <param name="row"> the row (0+) of the changed value </param>
	/// <param name="col"> the col (0+) of the changed value </param>
	/// <param name="value"> the new value for the cell indicated by row and column. </param>
	protected internal virtual void valueChanged(int row, int col, object value)
	{
		for (int i = 0; i < _listeners.Count; i++)
		{
			_listeners[i].tableModelValueChanged(row, col, value);
		}
	}

	}

}