using System;

// JWorksheet_JWorksheet_ColSelectionModel - class to handle colomn selections in the JWorksheet

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
// JWorksheet_JWorksheet_ColSelectionModel.java - Class to handle col 
//	selections in the JWorksheet.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2002-12-XX	J. Thomas Sapienza, RTi	Initial version.
// 2003-03-04	JTS, RTi		Javadoc'd, revised.  
// 2003-03-20	JTS, RTi		Revised after SAM's review.
// 2003-05-22	JTS, RTi		Revised to allow selection of entire
//					row if the first column is selected.
// 2003-11-18	JTS, RTi		Added finalize().
// 2006-01-17	JTS, RTi		Added workaround for errors cause by 
//					changes in Java 1.5 in 
//					isSelectedIndex().
// ----------------------------------------------------------------------------

namespace RTi.Util.GUI
{


	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This class, in conjunction with the JWorksheet_RowSelectionModel, allows a JTable to have
	/// a selection model that is like that of Microsoft Excel.  This class shares
	/// data with the JWorksheet_RowSelectionModel that is in the same JTable, and 
	/// JWorksheet_RowSelectionModel shares its data with this one.  The two need 
	/// to interoperate very closely in order to get the desired effect.
	/// 
	/// JTables by default have two selection models:<ol>
	/// <li>The main one is a row selection model that keeps track of which rows 
	/// are selected.  With this one in place users can either choose single rows,
	/// multiple rows, or SINGLE individual cells.</li>
	/// <li>Each column also has a selection model in place that keeps track of which
	/// columns are selected.  If single cell selection is turned on, then only
	/// one column can be selected.  If multiple row or single row selection is 
	/// used, then all the columns in a row are selected when a row is selected.</li>
	/// </ol>
	/// 
	/// The row selection model tells the JTable which rows are selected, and the 
	/// column selection model tells which columns are selected.  
	/// The interesting thing about how these are implemented by default is that 
	/// there is no mechanism to say something like:
	/// "Column 1 is selected in row 1, 2 and 4, but columns 2 and 3 are selected in row 3"
	/// 
	/// If a column is selected in one row, it is selected in ALL rows.
	/// 
	/// The JWorksheet_ColSelectionModel and JWorksheet_RowSelectionModel overcome these limitations.
	/// </summary>
	public class JWorksheet_ColSelectionModel : DefaultListSelectionModel, ListSelectionListener
	{

	/// <summary>
	/// The JWorksheet_RowSelectionModel that is used in conjunction with this 
	/// JWorksheet_ColSelectionModel in a JTable.
	/// </summary>
	private JWorksheet_RowSelectionModel __rowsm = null;

	/// <summary>
	/// A local reference to the _buffer in the JWorksheet_RowSelectionModel.  
	/// </summary>
	private bool[] __buffer = null;
	/// <summary>
	/// A local reference to the _cellsSelected in the JWorksheet_RowSelectionModel.
	/// </summary>
	private bool[] __cellsSelected = null;
	/// <summary>
	/// Whether to re-read the _buffer and _cellsSelected from the JWorksheet_RowSelectionModel.
	/// </summary>
	protected internal bool _reset = true;

	/// <summary>
	/// The value of the JWorksheet_RowSelectionModel's _cols
	/// </summary>
	protected internal int _rCols = -1;
	/// <summary>
	/// The value of the JWorksheet_RowSelectionModel's _currRow.
	/// </summary>
	private int __rCurrRow = -1;

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
	/// Whether the zeroth column was clicked in or not.
	/// </summary>
	protected internal bool _zeroCol = false;

	/// <summary>
	/// Indicate whether Java 1.5+ is being used.
	/// </summary>
	internal static readonly bool is15 = isVersion15OrGreater();

	/// <summary>
	/// Constructor.  Initializes to no columns selected.
	/// </summary>
	public JWorksheet_ColSelectionModel()
	{
	}

	/// <summary>
	/// Determine whether Java 1.5+ is being used, necessary to handle the difference between
	/// the JTable selection behavior. </summary>
	/// <returns> true if the JRE is version 1.5+, false if not. </returns>
	private static bool isVersion15OrGreater()
	{
		string version = System.getProperty("java.vm.version");
		version = version.Substring(0,3);
		// System.out.println("returning to is15: " +(StringUtil.atof(version) >= 1.5));
		return (StringUtil.atof(version) >= 1.5);
	}

	/// <summary>
	/// Override the method in DefaultListSelectionModel.  
	/// Adds a selection interval to the list of selected intervals.  It marks a new series of cells as selected. </summary>
	/// <param name="col0"> the first col of the selection interval </param>
	/// <param name="col1"> the last col of the selection interval. </param>
	public virtual void addSelectionInterval(int col0, int col1)
	{
		string routine = "JWorksheet_ColSelectionModel.addSelectionInterval";
		int dl = 10;
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "COL: addSelectionInterval(" + col0 + ", " + col1 + ")");
		}

		if (is15 && col0 != col1)
		{
			setLeadSelectionIndex(col1);
			return;
		}
		if (col0 == 0)
		{
			// System.out.println("   ZEROCOL = TRUE");
			_zeroCol = true;
		}
		if (is15)
		{
			__rowsm._currCol = col1;
		}
		else
		{
			__rowsm._currCol = col0;
		}
		if (col0 < _min)
		{
			_min = col0;
		}
		if (col1 < _min)
		{
			_min = col1;
		}
		if (col0 > _max)
		{
			_max = col0;
		}
		if (col1 > _max)
		{
			_max = col1;
		}
		_anchor = col0;
		_lead = col1;
		__rowsm._startCol = col0;
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~JWorksheet_ColSelectionModel()
	{
		__rowsm = null;
		__buffer = null;
		__cellsSelected = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the JWorksheet_RowSelectionModel being used. </summary>
	/// <returns> the JWorksheet_RowSelectionModel being used. </returns>
	public virtual JWorksheet_RowSelectionModel getJWorksheet_RowSelectionModel()
	{
		return __rowsm;
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
	/// Returns the max selection index. </summary>
	/// <returns> the max selection index. </returns>
	public virtual int getMaxSelectionIndex()
	{
		return _max;
	}

	/// <summary>
	/// Returns the min selection index. </summary>
	/// <returns> the min selection index. </returns>
	public virtual int getMinSelectionIndex()
	{
		return _min;
	}

	/// <summary>
	/// Overrides the method in DefaultListSelectionModel.  
	/// Returns whether the given col is selected or not. </summary>
	/// <param name="col"> the column to check whether it is selected or not. </param>
	/// <returns> true. </returns>
	public virtual bool isSelectedIndex(int col)

	{
	/*
	    String routine = "JWorksheet_ColSelectionModel.isSelectedIndex";
	    int dl = 10;
	    if ( Message.isDebugOn ) {
	    Message.printDebug ( dl, routine, "COL: isSelectedIndex(" + col + ")");
	    }*/
		__rCurrRow = __rowsm._currRow;

		// First check to see if the selected value has been drawn to the 
		// buffer (i.e., it is a new drag-selection) or if it is drawn 
		// to cellsSelected (i.e., the user dragged a new selected and released the mouse button).

		/*if ( Message.isDebugOn ) {
			Message.printDebug ( dl, routine, "  __rCurrRow: " + __rCurrRow);
			Message.printDebug ( dl, routine,"  _rCols: " + _rCols);
			Message.printDebug ( dl, routine,"  col: " + col);
		}*/
		if (__rowsm._drawnToBuffer)
		{
			if (_reset)
			{
				_reset = false;
				__buffer = __rowsm._buffer;
			}
			// The following was added because in 1.5 invalid array indices were being generated sometimes.
			int index = ((__rCurrRow * _rCols) + col);
			if ((__buffer.Length == 0) || index < 0 || index > __buffer.Length)
			{
				return false;
			}
			else if (__buffer[index])
			{
				return true;
			}
		}
		else
		{
			if (_reset)
			{
				_reset = false;
				__cellsSelected = __rowsm._cellsSelected;
			}
			if (__cellsSelected == null || __cellsSelected.Length == 0)
			{
				return false;
			}
			// The following was added because in 1.5 invalid array indices were being generated sometimes.		
			int index = ((__rCurrRow * _rCols) + col);
			if ((__cellsSelected.Length == 0) || index < 0 || index > __cellsSelected.Length)
			{
				return false;
			}
			else if (__cellsSelected[index])
			{
				return true;
			}
			/*if ( Message.isDebugOn ) {
				Message.printDebug ( dl, routine, "  __cellsSelected size: " + __cellsSelected.length);
			}*/
		}
		return false;
	}

	/// <summary>
	/// Overrides the method in DefaultListSelectionModel.  
	/// Removes an col interval from those already selected.  Does nothing. </summary>
	/// <param name="col0"> the first col. </param>
	/// <param name="col1"> the last col. </param>
	public virtual void removeIndexInterval(int col0, int col1)
	{
	}

	/// <summary>
	/// Overrides the method in DefaultListSelectionModel.  
	/// Removes a selection interval from those already selected. </summary>
	/// <param name="col0"> the first col to remove </param>
	/// <param name="col1"> the last col to remove. </param>
	public virtual void removeSelectionInterval(int col0, int col1)
	{
		string routine = "JWorksheet_ColSelectionModel.removeSelectionInterval";
		int dl = 10;
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "COL: removeSelectionInterval(" + col0 + ", " + col1 + ")");
		}
		if (is15)
		{
			__rowsm._currCol = col1;
		}
		else
		{
			__rowsm._currCol = col0;
		}
		if (col0 < _min)
		{
			_min = col0;
		}
		if (col1 < _min)
		{
			_min = col1;
		}
		if (col0 > _max)
		{
			_max = col0;
		}
		if (col1 > _max)
		{
			_max = col1;
		}
		_anchor = col0;
		_lead = col1;
		__rowsm._startCol = col0;
	}

	/// <summary>
	/// Overrides the method in DefaultListSelectionModel.  
	/// Sets the anchor's selection col.  Currently does nothing. </summary>
	/// <param name="anchorCol"> the col of the anchor position, the initial point clicked
	/// when the user is dragging the mouse. </param>
	public virtual void setAnchorSelectionIndex(int anchorCol)
	{
		string routine = "JWorksheet_ColSelectionModel.setAnchorSelectionIndex";
		int dl = 10;
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "COL: setAnchorSelectionIndex(" + anchorCol + ")");
		}
	}

	/// <summary>
	/// Overrides the method in DefaultListSelectionModel.  
	/// Sets the lead selection col. </summary>
	/// <param name="leadCol"> the lead col. </param>
	public virtual void setLeadSelectionIndex(int leadCol)
	{
		string routine = "JWorksheet_ColSelectionModel.setLeadSelectionIndex";
		int dl = 10;
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "COL: setLeadSelectionIndex(" + leadCol + ")");
		}
		__rowsm._currCol = leadCol;
	}

	/// <summary>
	/// Sets the JWorksheet_RowSelectionModel to use. </summary>
	/// <param name="rsm"> the JWorksheet_RowSelectionModel to use. </param>
	public virtual void setRowSelectionModel(JWorksheet_RowSelectionModel rsm)
	{
		__rowsm = rsm;
	}

	/// <summary>
	/// From DefaultListSelectionModel.  Sets the selection interval. </summary>
	/// <param name="col0"> the first selection interval. </param>
	/// <param name="col1"> the last selection interval. </param>
	public virtual void setSelectionInterval(int col0, int col1)
	{
		string routine = "JWorksheet_ColSelectionModel.setSelectionInterval";
		int dl = 10;
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine,"COL: setSelectionInterval(" + col0 + ", " + col1 + ")");
		}

		if (is15 && col0 != col1)
		{
			setLeadSelectionIndex(col1);
			return;
		}
		_zeroCol = false;
		if (col0 == 0)
		{
			_zeroCol = true;
			//System.out.println("   ZEROCOL = TRUE");
		}
		if (is15)
		{
			__rowsm._currCol = col1;
		}
		else
		{
			__rowsm._currCol = col0;
		}
		_max = -1;
		_min = int.MaxValue;
		if (col0 < _min)
		{
			_min = col0;
		}
		if (col1 < _min)
		{
			_min = col1;
		}
		if (col0 > _max)
		{
			_max = col0;
		}
		if (col1 > _max)
		{
			_max = col1;
		}
		_anchor = col0;
		_lead = col1;
		__rowsm._startCol = col0;
	}

	/// <summary>
	/// From ListSelectionListener.  This is notified if any changes have been made to the selection model
	/// (by the JWorksheet_RowSelectionModel) and the JTable rendered needs to check which ones need highlighted again. </summary>
	/// <param name="e"> the ListSelectionEvent that happened. </param>
	public virtual void valueChanged(ListSelectionEvent e)
	{
		if (!(e.getValueIsAdjusting()))
		{
			Array.Copy(__rowsm._buffer, 0, __rowsm._cellsSelected, 0, __rowsm._size);
		}
	}

	}

}