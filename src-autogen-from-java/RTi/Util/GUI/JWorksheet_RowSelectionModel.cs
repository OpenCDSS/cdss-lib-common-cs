using System;
using System.Collections.Generic;

// JWorksheet_RowSelectionModel - class to handle row selections in the JWorksheet

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
// JWorksheet_RowSelectionModel.java - Class to handle row selections in the 
//	JWorksheet.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2002-12-XX	J. Thomas Sapienza, RTi	Initial version.
// 2003-03-04	JTS, RTi		Javadoc'd, revised.  
// 2003-03-13	JTS, RTi		Added code to determine which rows
//					are selected.
// 2003-04-14	JTS, RTi		Added getSelectedColumn() and 
//					getSelectedColumns()
// 2003-05-22	JTS, RTi		Revised to allow selection of an entire
//					row if the first column is selected.
// 2003-09-23	JTS, RTi		* Renamed selectRow to 
//					  selectAllCellsInRow().
//					* Renamed selectCurrentRow to
//					  selectAllCellsInCurrentRow().
//					* Added selectRow() and made it public.
// 					* Added clearSelection().
// 2003-10-15	JTS, RTi		Added selectAllRows().
// 2003-10-24	JTS, RTi		Added selectColumn().
// 2003-10-27	JTS, RTi		__oneClickRowSelection is now by 
//					default 'false'.
// 2003-11-18	JTS, RTi		* Added finalize().
//					* Added selectCell().
// 2004-01-22	JTS, RTi		Added selectable code so that worksheets
//					can be made unselectable.
// 2004-06-07	JTS, RTi		Corrected bug in selectCell() that
//					was causing nothing to be selected.
// 2006-01-17	JTS, RTi		Added workaround for errors caused by
//					by changes in Java 1.5 to 
//					addSelectionInterval().
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.Util.GUI
{


	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// This class, in conjunction with the JWorksheet_ColSelectionModel, allows a JTable to have
	/// a selection model that is like that of Microsoft Excel.  This class shares
	/// data with the JWorksheet_ColSelectionModel that is in the same JTable, and 
	/// JWorksheet_ColSelectionModel shares its data with this one.  The two need 
	/// to interoperate very closely in order to get the desired effect.<para>
	/// 
	/// JTables by default have two selection models:<ol>
	/// <li>The main one is a row selection model that keeps track of which rows 
	/// are selected.  With this one in place users can either choose single rows,
	/// multiple rows, or SINGLE individual cells.</li>
	/// <li>Each column also has a selection model in place that keeps track of which
	/// columns are selected.  If single cell selection is turned on, then only
	/// one column can be selected.  If multiple row or single row selection is 
	/// used, then all the columns in a row are selected when a row is selected.</li>
	/// </para>
	/// </ol><para>
	/// 
	/// The row selection model tells the JTable which rows are selected, and the 
	/// column selection model tells which columns are selected.  
	/// The interesting thing about how these are implemented by default is that 
	/// there is no mechanism to say something like:
	/// </para>
	/// "Column 1 is selected in row 1, 2 and 4, but columns 2 and 3 are selected in row 3"<para>
	/// 
	/// </para>
	/// If a column is selected in one row, it is selected in ALL rows.<para>
	/// 
	/// The JWorksheet_ColSelectionModel and JWorksheet_RowSelectionModel overcome these limitations.
	/// 
	/// Under 1.5, some selection method calls changed. 
	/// 1.4 actions and methods:
	/// click - setSelectionInterval
	/// ctrl-click - addSelectionInterval
	/// shift-click - setLeadSelectionIndex
	/// click-drag - setLeadSelectionIndex
	/// 
	/// 1.5 actions and methods
	/// click - setSelectionInterval
	/// ctrl-click - addSelectionInterval
	/// shift-click - setSelectionInterval
	/// click-drag - setSelectionInterval
	/// 
	/// The partial fix is to reroute the method calls to the 1.4 behavior.  Additionally, current row/column needed to be set
	/// in some instances such as selecting or deselection cells.
	/// </para>
	/// </summary>
	public class JWorksheet_RowSelectionModel : DefaultListSelectionModel
	{

	/// <summary>
	/// The JWorksheet_ColSelectionModel that is used in conjunction with this JWorksheet_RowSelectionModel in a JWorksheet.
	/// </summary>
	private JWorksheet_ColSelectionModel __colsm = null;

	/// <summary>
	/// A 1-D representation of the 2-D array of cells.  This is a sort of double
	/// buffer on top of the cellsSelected array of highlighted cells so that 
	/// cells can be selected and drawn as highlighted, and then the selection 
	/// cancelled (e.g., by dragging the mouse back and de-highlighting them) and
	/// only the originally-selected cells remain highlighted.
	/// </summary>
	protected internal bool[] _buffer = null;

	/// <summary>
	/// A 1-D representation of the 2-D array of cells.  This specifies which ones 
	/// are highlighted (have a value of true) so the JTable renderer can highlight
	/// them.  This represents the entire size of the table (row * height), but each
	/// cell is held in a single bit.
	/// </summary>
	protected internal bool[] _cellsSelected = null;

	/// <summary>
	/// Whether a potential drag was started on the row selection model.  
	/// This happens when a cell is clicked on, and then clicked on again while it was already selected.
	/// </summary>
	public bool __possibleDrag = false;

	/// <summary>
	/// Whether the last cell drawing operation was done to the double buffer
	/// (_buffer) or to the main "canvas" (_cellsSelected).  If drawnToBuffer is true,
	/// the cell representation from _buffer will be used for the JTable renderer
	/// to determine which cells are highlighted.
	/// </summary>
	protected internal bool _drawnToBuffer = false;

	/// <summary>
	/// Whether all the cells in a row will be selected if the 0th column is clicked on.  
	/// </summary>
	private bool __oneClickRowSelection = false;

	/// <summary>
	/// Whether any data can be selected in the worksheet.
	/// </summary>
	private bool __selectable = true;

	/// <summary>
	/// Number of columns in the JTable.
	/// </summary>
	protected internal int _cols = -1;

	/// <summary>
	/// The last-selected column.
	/// </summary>
	protected internal int _currCol = -1;

	/// <summary>
	/// The last-selected row.
	/// </summary>
	protected internal int _currRow = -1;

	/// <summary>
	/// Number of rows in the JTable.
	/// </summary>
	protected internal int _rows = -1;

	/// <summary>
	/// The total size (rows * cols) of the JTable.
	/// </summary>
	protected internal int _size = -1;

	/// <summary>
	/// The Column at which drawing started for a dragged selection.
	/// </summary>
	protected internal int _startCol = -1;

	/// <summary>
	/// The Row at which drawing started for a dragged selection.
	/// </summary>
	protected internal int _startRow = -1;

	/// <summary>
	/// The anchor position from which all values are initially selected and dragged.
	/// </summary>
	protected internal int _anchor = -1;

	/// <summary>
	/// The most recently-selected position.
	/// </summary>
	protected internal int _lead = -1;

	/// <summary>
	/// The lowest row number selected.
	/// </summary>
	protected internal int _min = int.MaxValue;

	/// <summary>
	/// The highest row number selected.
	/// </summary>
	protected internal int _max = -1;

	/// <summary>
	/// Sets the "partner" row selection model for use when two worksheets work 
	/// together to do the same selection style.  See the TSViewTable code in GRTS for an example of how it works.
	/// </summary>
	private JWorksheet_RowSelectionModel __partner = null;

	/// <summary>
	/// Indicate whether the Java Runtime Environment is 1.5 or greater.  This is used because
	/// some JTable code originally developed with Java 1.4 does not work with 1.5+ and special checks are needed.
	/// </summary>
	private static readonly bool is15 = JWorksheet_ColSelectionModel.is15;

	/// <summary>
	/// Constructor.  Sets up the buffers and other internal variables. </summary>
	/// <param name="tRows"> the number of rows in the JTable. </param>
	/// <param name="tCols"> the number of columns in the JTable. </param>
	public JWorksheet_RowSelectionModel(int tRows, int tCols)
	{
		_rows = tRows;
		_cols = tCols;
		_size = _rows * _cols;

		_cellsSelected = new bool [_size];
		_buffer = new bool [_size];
		zeroArray(_cellsSelected);
		zeroArray(_buffer);
	}

	/// <summary>
	/// Overrides method in DefaultListSelectionModel.  
	/// Adds a selection interval to the list of selected intervals. </summary>
	/// <param name="row0"> the first row of the selection interval </param>
	/// <param name="row1"> the last row of the selection interval. </param>
	public virtual void addSelectionInterval(int row0, int row1)
	{
		string routine = "JWorksheet_RowSelectionModel.addSelectionInterval";
		int dl = 10;

		if (is15)
		{
			_currRow = row1;
			if (row0 != row1)
			{
				setLeadSelectionIndex(row1);
				return;
			}
		}
		if (__oneClickRowSelection && __partner != null)
		{
			if (__partner.allCellsInRowSelected(row0))
			{
				__partner.forceDeselectRow(row0);
			}
			else
			{
				__partner.forceSelectAllCellsInRow(row0);
			}
		}

		if (!__selectable)
		{
			return;
		}
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "ROW: addSelectionInterval(" + row0 + ", " + row1 + ")");
		}
		if (row0 < _min)
		{
			_min = row0;
		}
		if (row1 < _min)
		{
			_min = row1;
		}
		if (row0 > _max)
		{
			_max = row0;
		}
		if (row1 > _max)
		{
			_max = row1;
		}

		_anchor = row0;
		_lead = row1;
		_drawnToBuffer = true;
		_startRow = row0;
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine,"  _anchor: " + _anchor);
			Message.printDebug(dl, routine,"  _lead: " + _lead);
			Message.printDebug(dl, routine,"  _startRow: " + _startRow);
			Message.printDebug(dl, routine,"  _currRow: " + _currRow);
			Message.printDebug(dl, routine,"  _cols: " + _cols);
			Message.printDebug(dl, routine,"  _startCol: " + _startCol);
		}
		int index = ((_currRow * _cols) + _startCol);
		if (index < 0 || index > _buffer.Length)
		{
			// ignore -- this is an error encountered when running with Java 1.5.
		}
		else
		{
			_buffer[index] = true;
		}

		Array.Copy(_buffer, 0, _cellsSelected, 0, _size);
		if (_currCol == 0)
		{
			selectAllCellsInCurrentRow();
		}
		notifyAllListeners(_startRow, _startRow);

		__colsm._reset = true;
	}

	/// <summary>
	/// Checks to see if all the cells in a row are selected. </summary>
	/// <param name="row"> the row to check. </param>
	/// <returns> true if all cells are selected, false if not. </returns>
	public virtual bool allCellsInRowSelected(int row)
	{
		for (int i = 0; i < _cols; i++)
		{
			if (_buffer[(row * _cols) + i] == false)
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Clears all selected cells and notifies their listeners that they were deselected.
	/// </summary>
	public virtual void clearSelection()
	{
		zeroArray(_buffer);
		Array.Copy(_buffer, 0, _cellsSelected, 0, _size);
		notifyAllListeners(0, _rows);
	}

	/// <summary>
	/// Deselects all the cells in the current row.
	/// </summary>
	public virtual void deselectCurrentRow()
	{
		deselectRow(_currRow);
	}

	/// <summary>
	/// Deselects all the cells in the specified row. </summary>
	/// <param name="row"> the row to deselect. </param>
	public virtual void deselectRow(int row)
	{
		if (!__oneClickRowSelection)
		{
			return;
		}
		for (int i = 0; i < _cols; i++)
		{
			_buffer[(row * _cols) + i] = false;
		}
		if (__partner != null)
		{
			__partner.forceDeselectRow(row);
		}
		notifyAllListeners(row, row);
	}

	/// <summary>
	/// Returns whether a drag (for drag and drop) was started on a cell in the selection model. </summary>
	/// <returns> whether a drag was started. </returns>
	public virtual bool dragWasTriggered()
	{
		return __possibleDrag;
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~JWorksheet_RowSelectionModel()
	{
		__colsm = null;
		_buffer = null;
		_cellsSelected = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Forces all the cells in the specified row to be deselected, no matter what the
	/// other internal settings of the row selection model are. </summary>
	/// <param name="row"> the row to deselect. </param>
	protected internal virtual void forceDeselectRow(int row)
	{
		for (int i = 0; i < _cols; i++)
		{
			_buffer[(row * _cols) + i] = false;
		}
		Array.Copy(_buffer, 0, _cellsSelected, 0, _size);
		notifyAllListeners(row, row);
	}

	/// <summary>
	/// Forces all the cells in the specified row to be selected. </summary>
	/// <param name="row"> the row to select. </param>
	private void forceSelectAllCellsInRow(int row)
	{
		for (int i = 0; i < _cols; i++)
		{
			_buffer[(row * _cols) + i] = true;
		}
		Array.Copy(_buffer, 0, _cellsSelected, 0, _size);
		notifyAllListeners(row, row);
	}

	/// <summary>
	/// Returns the JWorksheet_ColSelectionModel being used. </summary>
	/// <returns> the JWorksheet_ColSelectionModel being used. </returns>
	public virtual JWorksheet_ColSelectionModel getJWorksheet_ColSelectionModel()
	{
		return __colsm;
	}

	/// <summary>
	/// Returns whether all the cells in a row should be selected when the 0th column is clicked on. </summary>
	/// <returns> whether to do one click row selection or not </returns>
	public virtual bool getOneClickRowSelection()
	{
		return __oneClickRowSelection;
	}

	/// <summary>
	/// Returns the partner row selection model.  See the GRTS code for an example of how it works. </summary>
	/// <returns> the partner row selection model. </returns>
	public virtual JWorksheet_RowSelectionModel getPartner()
	{
		return __partner;
	}

	/// <summary>
	/// Returns an integer of the first column that is selected or -1 if none are selected. </summary>
	/// <returns> an integer of the first column that is selected or -1 if none are selected. </returns>
	public virtual int getSelectedColumn()
	{
		for (int i = 0; i < _cols; i++)
		{
			for (int j = 0; j < _rows; j++)
			{
				if (_buffer[(j * _cols) + i] == true)
				{
					return i;
				}
			}
		}

		return -1;
	}

	/// <summary>
	/// Returns an integer array of the columns that have had some of their cells selected. </summary>
	/// <returns> an integer array of the columns that have had some of their cells selected. </returns>
	public virtual int[] getSelectedColumns()
	{
		System.Collections.IList v = new List<object>();

		for (int i = 0; i < _cols; i++)
		{
			for (int j = 0; j < _rows; j++)
			{
				if (_buffer[(j * _cols) + i] == true)
				{
					v.Add(new int?(i));
					j = _rows + 1;
				}
			}
		}

		int[] arr = new int[v.Count];
	//	Message.printStatus(1, "", "" + v.size() + " columns selected");
		for (int i = 0; i < arr.Length; i++)
		{
			arr[i] = ((int?)v[i]).Value;
	//		Message.printStatus(1, "", "column: " + arr[i]);
		}
		return arr;
	}

	/// <summary>
	/// Returns an integer of the first row that is selected or -1 if none are selected. </summary>
	/// <returns> an integer of the first row that is selected or -1 if none are selected. </returns>
	public virtual int getSelectedRow()
	{
		for (int i = 0; i < _rows; i++)
		{
			for (int j = 0; j < _cols; j++)
			{
				if (_buffer[(i * _cols) + j] == true)
				{
					return i;
				}
			}
		}

		return -1;
	}

	/// <summary>
	/// Returns an integer array of the rows that have had some of their cells selected. </summary>
	/// <returns> an integer array of the rows that have had some of their cells selected. </returns>
	public virtual int[] getSelectedRows()
	{
		System.Collections.IList v = new List<object>();

		for (int i = 0; i < _rows; i++)
		{
			for (int j = 0; j < _cols; j++)
			{
				if (_buffer[(i * _cols) + j] == true)
				{
					v.Add(new int?(i));
					j = _cols + 1;
				}
			}
		}

		int[] arr = new int[v.Count];
		for (int i = 0; i < arr.Length; i++)
		{
			arr[i] = ((int?)v[i]).Value;
		}
		return arr;
	}

	/// <summary>
	/// Returns the anchor selection index. </summary>
	/// <returns> the anchor selection index. </returns>
	public virtual int getAnchorSelectionIndex()
	{
		return _anchor;
	}

	/// <summary>
	/// Returns the lead selection index. </summary>
	/// <returns> the lead selection index. </returns>
	public virtual int getLeadSelectionIndex()
	{
		return _lead;
	}

	/// <summary>
	/// Returns the maximum selection index. </summary>
	/// <returns> the maximum selection index. </returns>
	public virtual int getMaxSelectionIndex()
	{
		return _max;
	}

	/// <summary>
	/// Returns the minimum selection index. </summary>
	/// <returns> the minimum selection index. </returns>
	public virtual int getMinSelectionIndex()
	{
		return _min;
	}

	/// <summary>
	/// Overrides method in DefaultListSelectionModel.  Returns whether the given row is selected or not.  Always returns true. </summary>
	/// <returns> true. </returns>
	public virtual bool isSelectedIndex(int row)

	{
	/*String routine = "JWorksheet_RowSelectionModel.isSelectedIndex";
	    int dl = 10;
	    if ( Message.isDebugOn ) {
	    Message.printDebug ( dl, routine, "ROW: isSelectedIndex(" + row + ")");
	    }*/
		_currRow = row;
		return true;
	}

	/// <summary>
	/// Notifies all listeners that something has changed in the selection model. </summary>
	/// <param name="startRow"> the first row at which a change has occurred. </param>
	/// <param name="endRow"> the last row at which a change has occurred. </param>
	private void notifyAllListeners(int startRow, int endRow)
	{
		string routine = "JWorksheet_RowSelectionModel.notifyAllListeners";
		int dl = 10;
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, " startRow: " + startRow + "  endRow: " + endRow);
		}
		ListSelectionListener[] listeners = getListSelectionListeners();
		ListSelectionEvent e = null;

		for (int i = 0; i < listeners.Length; i++)
		{
			if (e == null)
			{
				e = new ListSelectionEvent(this, startRow - 30, endRow + 30, true);
			}
			((ListSelectionListener)listeners[i]).valueChanged(e);
		}
	}

	/// <summary>
	/// Overrides method in DefaultListSelectionModel.  Removes an row interval from those already selected.  Does nothing. </summary>
	/// <param name="row0"> the first row. </param>
	/// <param name="row1"> the last row. </param>
	public virtual void removeIndexInterval(int row0, int row1)
	{
		throw new Exception("Developer thinks method not called");
	}

	/// <summary>
	/// Overrides method in DefaultListSelectionModel.  Removes a selection interval from those already selected. </summary>
	/// <param name="row0"> the first row to remove </param>
	/// <param name="row1"> the last row to remove. </param>
	public virtual void removeSelectionInterval(int row0, int row1)
	{
		string routine = "JWorksheet_RowSelectionModel.removeSelectionInterval";
		int dl = 10;
		if (__oneClickRowSelection && __partner != null)
		{
			__partner.forceDeselectRow(row0);
		}

		if (!__selectable)
		{
			return;
		}
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "ROW: removeSelectionInterval(" + row0 + ", " + row1 + ")");
		}
		if (row0 < _min)
		{
			_min = row0;
		}
		if (row1 < _min)
		{
			_min = row1;
		}
		if (row0 > _max)
		{
			_max = row0;
		}
		if (row1 > _max)
		{
			_max = row1;
		}
		if (is15)
		{
			_currRow = row1;
		}
		_anchor = row0;
		_lead = row1;

		// used to avoid weird calls like:
		//   removeSelectionInterval(2147483647, -1)
		// that get called when setting up a table
		if (row0 > 100000000 || row0 < 0 || row1 > 100000000 || row1 < 0)
		{
			return;
		}

		__colsm._reset = true;
		_drawnToBuffer = true;
		_startRow = row0;
		_buffer[((_currRow * _cols) + _startCol)] = false;
		if (_currCol == 0)
		{
			deselectCurrentRow();
		}
		Array.Copy(_buffer, 0, _cellsSelected, 0, _size);
		notifyAllListeners(_startRow, _startRow);
	}

	/// <summary>
	/// Selects all the cells the current row.
	/// </summary>
	private void selectAllCellsInCurrentRow()
	{
		selectAllCellsInRow(_currRow);
	}

	/// <summary>
	/// Selects all the cells in the specified row. </summary>
	/// <param name="row"> the row to select. </param>
	private void selectAllCellsInRow(int row)
	{
		if (!__oneClickRowSelection)
		{
			return;
		}
		for (int i = 0; i < _cols; i++)
		{
			_buffer[(row * _cols) + i] = true;
		}
		if (__partner != null)
		{
			__partner.forceSelectAllCellsInRow(row);
		}
		notifyAllListeners(row, row);
	}

	/// <summary>
	/// Selects an individual cell. </summary>
	/// <param name="row"> the row of the cell. </param>
	/// <param name="visibleColumn"> the <b>visible</b> column of the cell. </param>
	public virtual void selectCell(int row, int visibleColumn)
	{
		__colsm._reset = true;
		_drawnToBuffer = true;
		Array.Copy(_cellsSelected, 0, _buffer, 0, _size);

		int currRow = -1;
		bool found = false;
		int j = 0;
		for (; j < _buffer.Length && !found; j++)
		{
			// the very first time in this loop, this will increment the currRow # to 0
			if ((j % _cols) == 0)
			{
				currRow++;
			}

			if ((j % _cols) == visibleColumn)
			{
				if (currRow == row)
				{
					_buffer[j] = true;
					found = true;
				}
			}
			else
			{
				_buffer[j] = false;
			}
		}

		for (; j < _buffer.Length; j++)
		{
			_buffer[j] = false;
		}

		notifyAllListeners(0, _rows);
	}

	/// <summary>
	/// Forces a row to be selected and sets up the anchor points for selecting more cells based on the 0th column
	/// of the specified row.  This sets up variables as if setSelectionInterval were called. </summary>
	/// <param name="row"> the row to select. </param>
	public virtual void selectRow(int row)
	{
		int dl = 10;
		string routine = "selectRow";
		if (Message.isDebugOn)
		{
			Message.printDebug(dl,routine,"ROW: selectRow(" + row + ")");
		}
		__colsm._reset = true;
		_drawnToBuffer = true;
		Array.Copy(_cellsSelected, 0, _buffer, 0, _size);
		_startRow = row;
		_currRow = row;
		_startCol = 0;
		_currCol = 0;

		int sstartCol = 0;
		int endCol = _cols;

		for (int j = sstartCol; j < endCol; j++)
		{
			_buffer[(row * _cols) + j] = true;
		}
		notifyAllListeners(row, row);
	}

	public virtual void selectRowWithoutDeselecting(int row)
	{
		__colsm._reset = true;
		_drawnToBuffer = true;
	//	System.arraycopy(_cellsSelected, 0, _buffer, 0, _size);
		_startRow = row;
		_currRow = row;
		_startCol = 0;
		_currCol = 0;

		int sstartCol = 0;
		int endCol = _cols;

		for (int j = sstartCol; j < endCol; j++)
		{
			_buffer[(row * _cols) + j] = true;
		}
		notifyAllListeners(row, row);
	}

	/// <summary>
	/// Selects all the rows in the table model
	/// </summary>
	public virtual void selectAllRows()
	{
		__colsm._reset = true;
		_drawnToBuffer = true;
		Array.Copy(_cellsSelected, 0, _buffer, 0, _size);

		for (int j = 0; j < _buffer.Length; j++)
		{
			_buffer[j] = true;
		}
		notifyAllListeners(0, _rows);
	}

	/// <summary>
	/// Selects a column and highlights all its cells. </summary>
	/// <param name="visibleColumn"> the <b>visible</b> column to select. </param>
	public virtual void selectColumn(int visibleColumn)
	{
		__colsm._reset = true;
		_drawnToBuffer = true;
		Array.Copy(_cellsSelected, 0, _buffer, 0, _size);

		for (int j = 0; j < _buffer.Length; j++)
		{
			if ((j % _cols) == visibleColumn)
			{
				_buffer[j] = true;
			}
			else
			{
				_buffer[j] = false;
			}
		}
		notifyAllListeners(0, _rows);
	}

	/// <summary>
	/// Overrides method in DefaultListSelectionModel.  Sets the anchor's selection row.  Currently does nothing. </summary>
	/// <param name="anchorIndex"> the row of the anchor position. </param>
	public virtual void setAnchorSelectionIndex(int anchorIndex)
	{
		// called when ctrl-shift-click
		int dl = 10; // Debug level, to help track down Java 1.4 to 1.5 changes in behavior
		string routine = "JWorksheet_RowSelectModel.setAnchorSelectionIndex";

		if (Message.isDebugOn)
		{
			Message.printDebug(dl,routine,"ROW: setAnchorSelectionIndex(" + anchorIndex + ")");
		}
	}

	/// <summary>
	/// Overrides method in DefaultListSelectionModel.  Sets the lead selection row. </summary>
	/// <param name="leadIndex"> the lead row. </param>
	public virtual void setLeadSelectionIndex(int leadIndex)
	{
		int dl = 10; // Debug level, to help track down Java 1.4 to 1.5 changes in behavior
		string routine = "JWorksheet_RowSelectModel.setLeadSelectionIndex";
		//System.out.println("ROW.setLeadSelectionIndex " + leadIndex);
		debug();
		if (__oneClickRowSelection && __partner != null)
		{
			return;
		}
		if (!__selectable)
		{
			// Worksheet is not selectable in any form so return.
			return;
		}
		if (Message.isDebugOn)
		{
			Message.printDebug(dl,routine,"ROW: setLeadSelectionIndex(" + leadIndex + ")");
		}
		__colsm._reset = true;
		_drawnToBuffer = true;
		Array.Copy(_cellsSelected, 0, _buffer, 0, _size);
		int sstartRow = 0;
		int endRow = 0;

		// Order the rows to check to be increasing, despite the drag direction.
		if (_startRow < _currRow)
		{
			sstartRow = _startRow;
			endRow = _currRow;
		}
		else
		{
			sstartRow = _currRow;
			endRow = _startRow;
		}

		int sstartCol = 0;
		int endCol = 0;
		// Order the columns to check to be increasing, despite the drag direction.
		if (_startCol < _currCol)
		{
			sstartCol = _startCol;
			endCol = _currCol;
		}
		else
		{
			sstartCol = _currCol;
			endCol = _startCol;
		}

		bool selectRows = false;
		if (endCol == 0 && __colsm._zeroCol && __oneClickRowSelection)
		{
			selectRows = true;
		}
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Setting selected, StartCol=" + sstartCol + " EndCol=" + endCol + " StartRow=" + sstartRow + " EndRow=" + endRow);
		}
		// Loop through selected columns (may or may not have selected rows).
		if (sstartCol >= 0 && endCol >= 0)
		{
			// Loop through selected rows in column.
			for (int i = sstartRow; i <= endRow; i++)
			{
				if (selectRows)
				{
					if (Message.isDebugOn)
					{
						Message.printDebug(dl,routine, "selectRows=true, selecting all rows in column " + i);
					}
					selectAllCellsInRow(i);
				}
				else
				{
					if (Message.isDebugOn)
					{
						Message.printDebug(dl,routine, "In row: " + i + " Selecting columns StartCol=" + sstartCol + " EndCol=" + endCol + " #ColsTotal=" + _cols);
					}
					for (int j = sstartCol; j <= endCol; j++)
					{
						_buffer[(i * _cols) + j] = true;
					}
				}
			}
		}
		notifyAllListeners(sstartRow, endRow);
	}

	/// <summary>
	/// Sets whether all the cells in a row should be selected when the 0th column is clicked on. </summary>
	/// <param name="oneClick"> whether to turn on one click row selectio or not. </param>
	public virtual void setOneClickRowSelection(bool oneClick)
	{
		__oneClickRowSelection = oneClick;
	}

	/// <summary>
	/// Sets the row selection model's partner selection model (from another worksheet). </summary>
	/// <param name="partner"> the partner row selection model.  If null, partner row selection is turned off. </param>
	public virtual void setPartner(JWorksheet_RowSelectionModel partner)
	{
		__partner = partner;
	}

	/// <summary>
	/// Sets whether any cells in the worksheet can be selected. </summary>
	/// <param name="selectable"> whether any cells can be selected.  If false, all calls to
	/// cell selection routines return immediately. </param>
	public virtual void setSelectable(bool selectable)
	{
		__selectable = selectable;
	}

	/// <summary>
	/// Overrides method in DefaultListSelectionModel.  Sets the selection interval. </summary>
	/// <param name="row0"> the first selection interval. </param>
	/// <param name="row1"> the last selection interval. </param>
	public virtual void setSelectionInterval(int row0, int row1)
	{
		string routine = "JWorksheet_RowSelectionModel.setSelectionInterval";
		int dl = 10;
		//System.out.println("ROW.setSelectionInterval " + row0+","+row1);

		if (is15)
		{
			_currRow = row1;
			if (row0 != row1)
			{
				setLeadSelectionIndex(row1);
				return;
			}
		}
		if (__oneClickRowSelection && __partner != null)
		{
			__partner.clearSelection();
			__partner.forceSelectAllCellsInRow(row0);
		}

		if (!__selectable)
		{
			return;
		}

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "ROW: setSelectionInterval(" + row0 + ", " + row1 + ")");
		}
		_max = -1;
		_min = int.MaxValue;
		if (row0 < _min)
		{
			_min = row0;
		}
		if (row1 < _min)
		{
			_min = row1;
		}
		if (row0 > _max)
		{
			_max = row0;
		}
		if (row1 > _max)
		{
			_max = row1;
		}

		_anchor = row0;
		_lead = row1;
		__colsm._reset = true;

		if (_buffer[((_currRow * _cols) + _startCol)] == true)
		{
			__possibleDrag = true;
		}
		else
		{
			__possibleDrag = false;
		}

		if (_drawnToBuffer == true)
		{
			zeroArray(_cellsSelected);
		}
		else
		{
			_drawnToBuffer = true;
		}
		zeroArray(_buffer);
		_startRow = _currRow;

		if (__colsm._zeroCol && __oneClickRowSelection)
		{
			selectAllCellsInRow(row0);
		}

		// This can happen if a call is made to selectAllCellsInRow() in the worksheet
		if (_currRow == -1 && _startCol == -1)
		{
			_currRow = row0;
			_startCol = 0;
			selectAllCellsInRow(row0);
		}
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "_buffer.length: " + _buffer.Length);
			Message.printDebug(dl, routine,"  _currRow: " + _currRow);
			Message.printDebug(dl, routine,"  _cols: " + _cols);
			Message.printDebug(dl, routine,"  _startCol: " + _startCol);
		}
		_buffer[((_currRow * _cols) + _startCol)] = true;
		notifyAllListeners(_startRow, _startRow);
	}

	/// <summary>
	/// Sets the JWorksheet_ColSelectionModel to use. </summary>
	/// <param name="csm"> the JWorksheet_ColSelectionModel to use. </param>
	public virtual void setColSelectionModel(JWorksheet_ColSelectionModel csm)
	{
		__colsm = csm;
		__colsm._rCols = _cols;
		addListSelectionListener(__colsm);
	}

	/// <summary>
	/// Sets a boolean array to all false. </summary>
	/// <param name="array"> the array to "zero" out. </param>
	private void zeroArray(bool[] array)
	{
		for (int i = 0; i < _size; i++)
		{
			array[i] = false;
		}
	}

	public virtual void moveLeadSelectionIndex(int leadIndex)
	{
		throw new Exception("Developer thinks method not called");
	}

	private void debug()
	{
	//    System.out.println(_anchor);
	//    System.out.println(_lead);
	//    System.out.println(_startRow);
	//    System.out.println(_startCol);
	//    System.out.println(_min);
	//    System.out.println(_max);
	//    System.out.println(_currRow);
	}

	}

}