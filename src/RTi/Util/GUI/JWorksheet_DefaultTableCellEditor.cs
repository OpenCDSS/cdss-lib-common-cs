using System;

// JWorksheet_DefaultTableCellEditor - class that overrides the default worksheet cell editor

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
// JWorksheet_DefaultTableCellEditor - Class that overrides the default 
//	worksheet cell editor.  
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2003-10-13	J. Thomas Sapienza, RTi	Initial version.
// 2003-10-22	JTS, RTi		* Added getCellEditorValue().
//					* Added stopCellEditing().
//					* Adapted code from the source code
//					  for DefaultCellEditor so this class
//					  works as desired.
// 2003-11-18	JTS, RTi		* Added code to set the edit cell in
//					  the worksheet.
//					* Overrode cancelCellEditing().
//					* Added finalize().
// 2004-02-02	JTS, RTi		Changed the default value that is shown
//					when editing to a blank ("") so that
//					the editor functions more like Excel.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.Util.GUI
{



	using DMIUtil = RTi.DMI.DMIUtil;

	using IOUtil = RTi.Util.IO.IOUtil;

	/// <summary>
	/// This class replaces the normal cell editor class in the JWorksheet.  It mimics
	/// all the behavior of the original cell editor, but it also gives the ability to
	/// do some nice handling of MISSING values as well as highlighting the cell that
	/// is currently being edited.
	/// </summary>
	public class JWorksheet_DefaultTableCellEditor : DefaultCellEditor, TableCellEditor
	{

	/// <summary>
	/// Array of values used to allow editing of multiple types of classes in one
	/// editor.  From the original java code.
	/// </summary>
	private Type[] __argTypes = new Type[]{typeof(string)};

	/// <summary>
	/// Object used to do some complicated handling of different data types in the same
	/// editor object.  From the original java code.
	/// </summary>
	private System.Reflection.ConstructorInfo __constructor;

	/// <summary>
	/// The worksheet in which this editor was initialized.
	/// </summary>
	private JWorksheet __worksheet;

	/// <summary>
	/// Used to store the data object that will be returned when editing is successful.
	/// From the original java code.
	/// </summary>
	private object __editorValue;

	/// <summary>
	/// Constructor.
	/// </summary>
	public JWorksheet_DefaultTableCellEditor() : base(new JTextField())
	{
	}

	/// <summary>
	/// Cancels the cell editing going on in the cell editor.  Overrides method from
	/// DefaultCellEditor and just forwards the call on to the super class.
	/// </summary>
	public virtual void cancelCellEditing()
	{
		__worksheet.setEditCell(-1, -1);
		base.cancelCellEditing();
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~JWorksheet_DefaultTableCellEditor()
	{
		IOUtil.nullArray(__argTypes);
		__constructor = null;
		__worksheet = null;
		__editorValue = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
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
	/// <param name="column"> the visible column of the cell being edited </param>
	/// <returns> the component for editing </returns>
	public virtual Component getTableCellEditorComponent(JTable table, object value, bool isSelected, int row, int column)
	{
		int absColumn = ((JWorksheet)table).getAbsoluteColumn(column);

		// The following code was taken from DefaultCellEditor.java ...

		__editorValue = null;
		Type columnClass = table.getColumnClass(absColumn);
		// Since our obligation is to produce a value which is
		// assignable for the required type it is OK to use the
		// String constructor for columns which are declared
		// to contain Objects. A String is an Object.
		if (columnClass == typeof(object))
		{
			columnClass = typeof(string);
		}
		try
		{
			__constructor = columnClass.GetConstructor(__argTypes);
		}
		catch (Exception e)
		{
			Console.WriteLine(e.ToString());
			Console.Write(e.StackTrace);
			return null;
		}

		__worksheet = (JWorksheet)table;
		__worksheet.setEditCell(row, column);

		// The preceding code was taken from DefaultCellEditor.java ...

		bool setNumericAlignment = false;

		if (columnClass == typeof(Double))
		{
			// When editing fields with missing double values, don't put
			// -999.0 into the editor initially -- instead, fill the editor
			// with a blank String.  The cell renderer renders cells with
			// missing values as empty cells, so having a number pop up
			// if the cell is edited could be confusing.
			if (DMIUtil.isMissing(((double?)value).Value))
			{
				value = "";
			}
			// also, mark this cell as one for which to put the edited
			// value on the right side of the cell (like in Excel).
			setNumericAlignment = true;
		}
		else if (columnClass == typeof(Integer))
		{
			// When editing fields with missing integer values, don't put
			// -999 into the editor initially -- instead, fill the editor
			// with a blank String.  The cell renderer renders cells with
			// missing values as empty cells, so having a number pop up
			// if the cell is edited could be confusing.	
			if (DMIUtil.isMissing(((int?)value).Value))
			{
				value = "";
			}
			// also, mark this cell as one for which to put the edited
			// value on the right side of the cell (like in Excel).		
			setNumericAlignment = true;
		}

	//	value = "";

		// create the standard editing component for this cell
		JComponent jc = (JComponent)base.getTableCellEditorComponent(table, value, isSelected, row, column);
		// set it to have the standard border
	//	jc.setBorder(new LineBorder(Color.black));

		// if the alignment of the text in the editor needs adjusted because
		// a number is being edited, do so.
		if (setNumericAlignment)
		{
			((JTextField)jc).setHorizontalAlignment(JTextField.RIGHT);
		}

	//	((JTextField)jc).getCaret().setSelectionVisible(true);
	//	((JTextField)jc).selectAll();

		// set the border to something slightly more visible
		jc.setBorder(BorderFactory.createLineBorder(Color.blue, 2));
		return (Component)jc;
	}

	/// <summary>
	/// Returns the editor value that was saved after editing was successfully stopped.
	/// This entire method was borrowed from code in JTable.java. </summary>
	/// <returns> the editor value that was saved after editing was successfully stopped. </returns>
	public virtual object getCellEditorValue()
	{
		return __editorValue;
	}

	/// <summary>
	/// Tries to stop the editing that is going on in the cell.  If editing cannot be
	/// stopped (because an invalid value [e.g., 'U7' entered into an Integer field]
	/// is entered), return false and highlight the editor component with a red 
	/// outline.  Otherwise, close the cell editor and the value will be entered into
	/// the table model via the setValueAt() method.  This entire method was borrowed
	/// from code in JTable.java. </summary>
	/// <returns> true if editing was stopped successfully, otherwise false. </returns>
	public virtual bool stopCellEditing()
	{
		string s = (string)base.getCellEditorValue();
		// Here we are dealing with the case where a user
		// has deleted the string value in a cell, possibly
		// after a failed validation. Return null, so that
		// they have the option to replace the value with
		// null or use escape to restore the original.
		// For Strings, return "" for backward compatibility.
		if ("".Equals(s))
		{
			if (__constructor.getDeclaringClass() == typeof(string))
			{
				__editorValue = s;
			}
			bool result = base.stopCellEditing();
			if (result)
			{
				__worksheet.setEditCell(-1, -1);
			}
		}

		try
		{
			__editorValue = __constructor.newInstance(new object[]{s});
		}
		catch (Exception)
		{
			((JComponent)getComponent()).setBorder(new LineBorder(Color.red));
			return false;
		}

		__worksheet.setEditCell(-1, -1);
		return base.stopCellEditing();
	}

	}

}