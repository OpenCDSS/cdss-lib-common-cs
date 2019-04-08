using System;
using System.Collections.Generic;

// JWorksheet_JComboBoxCellEditor - class that implements a specialized cell editor
// for columns that need to insert data with either JComboBoxes or text fields or both,
// in different cells in the same column

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
// JWorksheet_JComboBoxCellEditor - Class that implements a specialized
//	cell editor for columns that need to insert data with either 
//	JComboBoxes or text fields or both, in different cells in the same
//	column
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2003-06-30	J. Thomas Sapienza, RTi	Initial version.
// 2003-07-08	JTS, RTi		Changed to use SimpleJComboBox.
// 2003-10-27	JTS, RTi		Added getJComboBoxModel().
// 2003-11-18	JTS, RTi		Added finalize().
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.Util.GUI
{






	/// <summary>
	/// This class implements a specialized cell editor for columns that need to 
	/// allow editing of data via JComboBoxes, JTextFields, or both.  <para>
	/// This class is a fairly complex amalgamation of methods from many different
	/// table-related classes, and some of the code is adapted from the original 
	/// </para>
	/// java code.<para>
	/// Some background:<br>
	/// The JTable does not normally allow different editors to be used on 
	/// separate cells with in a column.  Cell editors and renderers are assigned 
	/// on a column-by-column by basis.  The need was expressed for RTi to be able
	/// to mix and match editors within a single column.  This class fulfills that
	/// </para>
	/// need.<para>
	/// Using this class, JComboBoxes (which could eventually be moved to
	/// SimpleJComboBoxes so that the values are selectable <i>and</i> editable)
	/// can be used to select values for entry into a JTable field, or JTextfields
	/// can be used for typing in values for data entry, in exactly the fashion in
	/// </para>
	/// which the JTable's editing normally works.<para>
	/// Because the cell editing is normally handled column-by-column, this class
	/// has do some data handling (e.g., it has to know the number of rows
	/// currently in the table) that seems extraneous, but this is in order to
	/// </para>
	/// simulate a different editors on different rows.<para>
	/// Because different cells can use different data models for their JComboBoxes,
	/// this class also does some semi-intelligent caching of data models.  For 
	/// example:<br>
	/// A table has <b>10</b> rows and the even rows have one data model and the
	/// odd rows have another.  The even rows' combo boxes show a list of color names,
	/// </para>
	/// and the odd rows show a list of states. <para>
	/// Instead of storing 10 different data models, one for each cell, when a cell
	/// is assigned a data model, this class will check to see if any other classes
	/// are using the same data model.  If so, they will both share the same 
	/// data model (and reduce the memory footprint of this class).
	/// </para>
	/// </summary>
	public class JWorksheet_JComboBoxCellEditor : AbstractCellEditor, JWorksheet_Listener, ActionListener, TableCellEditor
	{

	/// <summary>
	/// Whether the SimpleJComboBox for the column should be editable.
	/// REVISIT (JTS - 2003-07-08)
	/// Probably expand this so in the future individual cells can have editable
	/// combo boxes or not.
	/// </summary>
	private bool __editable = false;

	/// <summary>
	/// If this is set to true, then when a new row is added to the table (and this
	/// is notified), the next row's data model for this column will be the same 
	/// as the data model for the row immediately above it.
	/// </summary>
	private bool __previousRowCopy = false;

	/// <summary>
	/// Borrowed from JTable's implementation of some code.  Used to generically 
	/// refer to constructors for data that could appear in this column.
	/// </summary>
	private System.Reflection.ConstructorInfo __constructor;

	/// <summary>
	/// The number of rows that are being managed by this editor.  If a table has
	/// more rows than this class knows about, the rows with row numbers &gr; 
	/// __size will be handled with normal JTable editing text fields.
	/// </summary>
	private int __size = 0;

	/// <summary>
	/// The last SimpleJComboBox that was created by a call to 
	/// getTableCellEditorComponent().
	/// </summary>
	private SimpleJComboBox __lastJCB = null;
	/// <summary>
	/// The last JTextField that was created by a call to getTableCellEditorComponent().
	/// </summary>
	private JTextField __lastJTF = null;

	/// <summary>
	/// Borrowed from JTable code.  Used to refer back to the data value being edited.
	/// </summary>
	private object __value;

	/// <summary>
	/// This list contains all the different data models being used in the table column.
	/// </summary>
	private System.Collections.IList __models = null;
	/// <summary>
	/// This list maps row numbers to the data model number that they are using.
	/// More than row can use the same data model.  There should be one entry
	/// in this Vector for every row in the table.
	/// </summary>
	private System.Collections.IList __rowToModel = null;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="worksheet"> the worksheet in which this class will be used. </param>
	/// <param name="rows"> the number of the rows in the table when this cell editor
	/// was applied. </param>
	/// <param name="editable"> whether the SimpleJComboBox for this column should be editable. </param>
	public JWorksheet_JComboBoxCellEditor(JWorksheet worksheet, int rows, bool editable)
	{
		worksheetSetRowCount(rows);
		addCellEditorListener(worksheet);
		__editable = editable;
	}

	/// <summary>
	/// Responds to action events on the JComponents used for editing.  Calls
	/// stopCellEditing(). </summary>
	/// <param name="e"> the ActionEvent that happened. </param>
	public virtual void actionPerformed(ActionEvent e)
	{
		stopCellEditing();
	}


	/// <summary>
	/// Adds a cell editor listener; from AbstractCellEditor. </summary>
	/// <param name="l"> the cell editor to add. </param>
	public virtual void addCellEditorListener(CellEditorListener l)
	{
		base.addCellEditorListener(l);
	}

	/// <summary>
	/// Stops any editing; from AbstractCellEditor.  Calls fireEditingCanceled().
	/// </summary>
	public virtual void cancelCellEditing()
	{
		fireEditingCanceled();
	}

	/// <summary>
	/// Counts the number of rows that are referring to the model stored in the 
	/// __models Vector at the given position.  This is used to determine when a 
	/// data model can be removed from the __models Vector (i.e., when there are 
	/// no references to it in the __rowToModel Vector). </summary>
	/// <param name="modelNum"> the modelNum for which to see how many rows use it as their
	/// data model </param>
	/// <returns> the number of rows that use the specified model as their model. </returns>
	private int countModelUse(int modelNum)
	{
		int size = __rowToModel.Count;

		int count = 0;
		int j = -1;
		for (int i = 0; i < size; i++)
		{
			j = ((int?)__rowToModel[i]).Value;
			if (modelNum == j)
			{
				count++;
			}
		}
		return count;
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~JWorksheet_JComboBoxCellEditor()
	{
		__constructor = null;
		__lastJCB = null;
		__lastJTF = null;
		__value = null;
		__models = null;
		__rowToModel = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Looks through the models already set in this class for various and checks to
	/// see if the specified row matches any of them.  If the specified one matches,
	/// the model number (the element of the model in __models) will be return.  
	/// Otherwise, -1 is returned. </summary>
	/// <param name="v"> a Vector of values (SimpleJComboBox data model) to see if is already 
	/// present in the __models Vector. </param>
	/// <returns> -1 if the model cannot be found, or the model number of the model in
	/// the __models Vector if it matched. </returns>
	private int findModelInModels(System.Collections.IList v)
	{
		int size = __models.Count;

		for (int i = 0; i < size; i++)
		{
//JAVA TO C# CONVERTER WARNING: LINQ 'SequenceEqual' is not always identical to Java AbstractList 'equals':
//ORIGINAL LINE: if (v.equals((java.util.List)__models.get(i)))
			if (v.SequenceEqual((System.Collections.IList)__models[i]))
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// Gets the value of the JComponent cell editor as an Object; from CellEditor. </summary>
	/// <returns> the value of the JComponent cell editor. </returns>
	public virtual object getCellEditorValue()
	{
		if (__lastJCB != null)
		{
			return __lastJCB.getSelectedItem();
		}
		else if (__lastJTF != null)
		{
			return __value;
		}
		return null;
	}

	/// <summary>
	/// Returns the value of the JComponent cell editor as a String.  For JComboBoxes,
	/// this method is the same as getCellEditorValue().  For JTextFields, however,
	/// instead of returning __value, the actual text in the textfield is returned.
	/// </summary>
	public virtual object getCellEditorValueString()
	{
		if (__lastJCB != null)
		{
			return __lastJCB.getSelectedItem();
		}
		else if (__lastJTF != null)
		{
			return __lastJTF.getText();
		}
		return null;
	}

	/// <summary>
	/// Returns the combox box data model stored at the specific row. </summary>
	/// <param name="row"> the row to return the data model for. </param>
	/// <returns> null if the row doesn't use a combo box, or the Vector of values stored
	/// in the combo box if it does. </returns>
	public virtual System.Collections.IList getJComboBoxModel(int row)
	{
		int? I = (int?)__rowToModel[row];
		if (I.Value == -1)
		{
			return null;
		}
		return (System.Collections.IList)(__models[I.Value]);
	}

	/// <summary>
	/// Returns whether a new row added to the JWorksheet should use the same
	/// data model for the SimpleJComboBox as the one immediately preceding it. </summary>
	/// <returns> true if the new row should use the data model of the row above 
	/// it. </returns>
	public virtual bool getPreviousRowCopy()
	{
		return __previousRowCopy;
	}

	/// <summary>
	/// Returns a component that is used for editing the data in the cell; from 
	/// TableCellEditor.<para>
	/// Its original javadocs:<br>
	/// Sets an initial <code>value</code> for the editor. This will cause the editor 
	/// to <code>stopEditing</code> and lose any partially edited value if the editor 
	/// </para>
	/// is editing when this method is called.<para>
	/// Returns the component that should be added to the client's 
	/// <code>Component</code> hierarchy. Once installed in the client's hierarchy 
	/// this component will then be able to draw and receive user input.
	/// </para>
	/// </summary>
	/// <param name="table"> the JTable that is asking the editor to edit; can be null </param>
	/// <param name="value"> the value of the cell to be edited; it is up to the specific 
	/// editor to interpret and draw the value. For example, if value is the string 
	/// "true", it could be rendered as a string or it could be rendered as a check 
	/// box that is checked. null is a valid value </param>
	/// <param name="isSelected"> true if the cell is to be rendered with highlighting </param>
	/// <param name="row"> the row of the cell being edited </param>
	/// <param name="column"> the column of the cell being edited </param>
	/// <returns> the component for editing </returns>
	public virtual Component getTableCellEditorComponent(JTable table, object value, bool isSelected, int row, int column)
	{
		// get the number in the data model vector of the data model used
		// by this row.  If the number is -1, then the row will be edited
		// with a JTextField.
		int modelNum = -1;
		if (row < __size)
		{
			modelNum = ((int?)__rowToModel[row]).Value;
		}

		// set the current value of the editor component to the value passed-in.
		setValue(value);

		// the following code was borrowed from the java JTable's code
		// for validating values upon entry into the table
		try
		{
			Type type = table.getColumnClass(column);
			// Since our obligation is to produce a value which is
			// assignable for the required type it is OK to use the
			// String constructor for columns which are declared
			// to contain Objects. A String is an Object.
			if (type == typeof(object))
			{
				type = typeof(string);
			}
			Type[] argTypes = new Type[] {typeof(string)};
			__constructor = type.GetConstructor(argTypes);
		}
		catch (Exception)
		{
			return null;
		}
		// end of code borrowed straight from JTable.java
		////////////////////////////////

		// Do different things depending on whether a JTextField or a 
		// SimpleJComboBox will be returned.
		if (modelNum != -1 && modelNum < __models.Count)
		{
			System.Collections.IList v = (System.Collections.IList)__models[modelNum];
			SimpleJComboBox jcb = new SimpleJComboBox(v, __editable);
			jcb.addActionListener(this);
			jcb.setSelectedItem(value);
			__lastJTF = null;
			__lastJCB = jcb;
			return jcb;
		}
		else
		{
			JTextField jtf = new JTextField();
			jtf.addActionListener(this);
			if (value == null)
			{
				jtf.setText("");
			}
			else
			{
				jtf.setText(value.ToString());
			}
			__lastJCB = null;
			__lastJTF = jtf;
			jtf.setBorder(null);
			return jtf;
		}
	}

	/// <summary>
	/// Returns whether the cell is editable; from AbstractCellEditor.  Returns true,
	/// unless it was called in response to a single mouse click. </summary>
	/// <param name="event"> the event that caused this method to be called </param>
	/// <returns> whether the cell is editable. </returns>
	public virtual bool isCellEditable(EventObject @event)
	{
		if (@event is MouseEvent)
		{
			return ((MouseEvent)@event).getClickCount() >= 1;
		}
		return true;
	}

	/// <summary>
	/// Removes a cell editor listener; from AbstractCellListener. </summary>
	/// <param name="l"> the cell editor listener to add. </param>
	public virtual void removeCellEditorListener(CellEditorListener l)
	{
		base.removeCellEditorListener(l);
	}

	/// <summary>
	/// Sets a data model to be be used for a specific row in the JWorksheet. </summary>
	/// <param name="row"> the row to set the SimpleJComboBox data model for. </param>
	/// <param name="v"> a Vector of values (Doules, Integers, Strings, Dates) that will be
	/// used to populate the values in the SimpleJComboBox to use at the given row. </param>
	public virtual void setJComboBoxModel(int row, System.Collections.IList v)
	{
		// check to see if the row already has a model assigned to it, and
		// if so, remove it from __models if it is the only instance.
		int modelNum = ((int?)__rowToModel[row]).Value;
		if (modelNum > -1)
		{
			int matches = countModelUse(modelNum);
			if (matches == 1)
			{
				__models.RemoveAt(modelNum);
			}
		}

		int i = findModelInModels(v);
		if (i == -1)
		{
			__models.Add(v);
			__rowToModel[row] = new int?(__models.Count - 1);
		}
		else
		{
			__rowToModel[row] = new int?(i);
		}
	}

	/// <summary>
	/// Sets whether a new row added to the JWorksheet should use the same
	/// data model for the SimpleJComboBox as the one immediately preceding it. </summary>
	/// <param name="copy"> true if the new row should use the data model of the row above 
	/// it. </param>
	public virtual void setPreviousRowCopy(bool copy)
	{
		__previousRowCopy = copy;
	}

	/// <summary>
	/// Sets the editor JComponent's initial value. </summary>
	/// <param name="value"> the value to set the JComponent to intially. </param>
	private void setValue(object value)
	{
		if (__lastJCB != null)
		{
			__lastJCB.setSelectedItem(value);
		}
		else if (__lastJTF != null)
		{
			if (value == null)
			{
				__lastJTF.setText("");
			}
			else
			{
				__lastJTF.setText(value.ToString());
			}
		}
		__value = value;
	}

	/// <summary>
	/// Returns true, unless the event that was passed in was a mouse drag event;
	/// from AbstractCellEditor. </summary>
	/// <param name="event"> the event from which it should be determined whether to select
	/// the cell or not. </param>
	/// <returns> whether the cell should be selected </returns>
	public virtual bool shouldSelectCell(EventObject @event)
	{
		if (@event is MouseEvent)
		{
			MouseEvent e = (MouseEvent)@event;
			if (e.getID() != MouseEvent.MOUSE_DRAGGED)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Returns whether cell editing can start; from AbstractCellEditor.  Always returns
	/// true. </summary>
	/// <returns> true </returns>
	public virtual bool startCellEditing()
	{
		return true;
	}

	/// <summary>
	/// Returns whether or not the cell should stop editing; from AbstractCellEditor.
	/// Unless there is an error while parsing the value in a JTextField, it mostly
	/// just calls fireEditingStopped() and returns true. </summary>
	/// <returns> whether cell editing should stop. </returns>
	public virtual bool stopCellEditing()
	{
		if (__lastJCB != null)
		{
			if (__lastJCB.isEditable())
			{
				__lastJCB.actionPerformed(new ActionEvent(this, 0, ""));
			}
		}

		else if (__lastJTF != null)
		{
			string s = (string)getCellEditorValueString();
			if (s.Equals(""))
			{
				if (__constructor.getDeclaringClass() == typeof(string))
				{
					__value = s;
				}
			}

			try
			{
				__value = __constructor.newInstance(new object[]{s});
			}
			catch (Exception)
			{
				((JComponent)__lastJTF).setBorder(new LineBorder(Color.red));
				return false;
			}
		}

		fireEditingStopped();
		return true;
	}

	/// <summary>
	/// Responds to a row being added to the JTable; from JWorksheet_Listener.  By the
	/// time this is called, the JWorksheet has already added the row and taken care
	/// of its own internal bookkeeping. </summary>
	/// <param name="row"> the number of the row that was added. </param>
	public virtual void worksheetRowAdded(int row)
	{
		__size++;
		if (__previousRowCopy && (row != 0))
		{
			int? prevRowModel = (int?)__rowToModel[row - 1];
			__rowToModel.Insert(row, prevRowModel);
		}
		else
		{
			__rowToModel.Insert(row, new int?(-1));
		}
	}

	/// <summary>
	/// Responds to a row being deleted from the JTable; from JWorksheet_Listener.  By
	/// the time this is called, the JWorksheet has already deleted the row and taken
	/// care of its own internal bookkeeping. </summary>
	/// <param name="row"> the number of the row that was deleted. </param>
	public virtual void worksheetRowDeleted(int row)
	{
		__size--;

		if (row >= __rowToModel.Count)
		{
			return;
		}

		int modelNum = ((int?)__rowToModel[row]).Value;

		if (modelNum > -1)
		{
			int matches = countModelUse(modelNum);
			if (matches <= 1 && modelNum != -1 && modelNum < __models.Count)
			{
				__models.RemoveAt(modelNum);
			}
			else if (modelNum >= __models.Count)
			{
				// REVISIT (JTS - 2003-12-01)
				// for some reason, the number of models in use
				// is getting out of whack (particularly in the
				// statemod graphing tool worksheet).  Not a fatal
				// error, but it shouldn't be happening!!  Track 
				// it down sometime.
			}
		}

		__rowToModel.RemoveAt(row);
	}

	/// <summary>
	/// Sets the count of rows for which this class is managing editor components;
	/// from JWorksheet_Listener. </summary>
	/// <param name="rows"> the number of rows to manage. </param>
	public virtual void worksheetSetRowCount(int rows)
	{
		__size = rows;
		__rowToModel = new List<object>();
		__models = new List<object>();
		for (int i = 0; i < __size; i++)
		{
			__rowToModel.Add(new int?(-1));
		}
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void worksheetSelectAllRows(int time)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void worksheetDeselectAllRows(int time)
	{
	}

	}

}