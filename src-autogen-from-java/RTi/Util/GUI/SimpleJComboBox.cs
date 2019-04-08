using System;
using System.Collections.Generic;

// SimpleJComboBox - a simplified interface to a JComboBox, optimized for use with Strings

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

//---------------------------------------------------------------------------
// SimpleJComboBox - a simplified interface to a JComboBox, optimized for
// 	use with Strings.
//---------------------------------------------------------------------------
// Copyright:  See the COPYRIGHT file.
//---------------------------------------------------------------------------
// History:
//
// 2002-10-01	J. Thomas Sapienza, RTi	Initial version.
// 2002-10-10	JTS, RTi		Javadoc'd
// 2002-11-12	JTS, RTi		Changed init() to initialize().  Added
//					some more javadocs, revised some code,
//					and added some methods to bring into
//					line with methods in java.awt.Choice.
// 2003-05-21	JTS, RTi		Added setSelectedPrefixItem for use
//					with some CWRAT code.
// 2003-08-27	JTS, RTi		Added constructor that takes only a 
//					boolean.
// 2003-09-03	JTS, RTi		Renamed 'remove(int)' to 'removeAt(int)'
//					because the old method was conflicting
//					with a method that did a completely
//					different thing in the Container
//					class from which JComboBox is 
//					extended.
// 2003-09-17	JTS, RTi		Added setData() to replace all the
//					values in the combo box at once.
// 2003-09-18	JTS, RTI		setData() now clones the data vector
//					before using it.
// 2003-10-08	JTS, RTi		has() deprecated for contains().
// 2003-12-10	SAM, RTi		Change so that add() does not select
//					the item.
// 2003-12-12	JTS, RTi		* Javadoc'd a few methods that had 
//					  not been doc'd yet.
//					* Changed getSelected() so that if the
//					  Combo Box is editable, the text that
//					  the user typed in is returned, rather
//					  than the currently-selected value.
// 2004-02-24	JTS, RTi		Added isEditable().
// 2004-06-02	JTS, RTi		Added selectIgnoreCase().
// 2004-08-03	JTS, RTi		Added indexOf().
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//---------------------------------------------------------------------------

namespace RTi.Util.GUI
{



	/// <summary>
	/// A simplified interface to many methods in the JComboBox.
	/// Part of what this does is treat the JComboBox more as 
	/// a JTextField, as JTextField's methods for retrieving and
	/// setting text are more intuitive, but it also emulates a 
	/// little of the functionality of the old Choice classes to
	/// make porting a little more painless.<para>
	/// In addition, it assumes only strings.  Normal JComboBoxes
	/// can have any type of object in them, which makes for 
	/// some annoying cast problems.  SimpleJComboBox only ever
	/// </para>
	/// assumes that strings will be placed in the Combo Box.<para>
	/// </para>
	/// <b>Editable Combo Boxes</b><para>
	/// The combox box class supports editable combo boxes -- ones in which users
	/// can not only select a value, but can also type in a new value if they don't
	/// </para>
	/// find any that meet their needs.<para>
	/// This slightly complicates things for the developer, but not to a great extent.
	/// Here is a list of all the methods that may have non-intuitive responses 
	/// </para>
	/// depending on whether a combo box is editable or not.<para>
	/// <ul>
	/// <li><b>getFieldText()</b> - In a non-editable combobox, this method is not
	/// useful and will just return an empty string ("").  In an editable combobox,
	/// this method will return whatever value the user has entered.</li>
	/// <li><b>getItem()</b> - This returns the value in the list of combo box values
	/// that is stored at a specified position.  Because the list of values is stored
	/// internally, this can never be used to return a user-entered value, <b>unless</b>
	/// the value is inserted in the list after being typed in the combo box field.</li>
	/// <li><b>getItemAt()</b> - This returns the value in the list of combo box values
	/// that is stored at a specified position.  Because the list of values is stored
	/// internally, this can never be used to return a user-entered value, <b>unless</b>
	/// the value is inserted in the list after being typed in the combo box field.</li>
	/// <li><b>getSelected()</b> - This returns the currently-selected item.  If the
	/// combo box is editable and the user has not typed in anything, the 
	/// currently-selected value is returned.  If the user has typed anything in, 
	/// that is returned.  In a non-editable combo box, it just returns the 
	/// currently- selected item.</li>
	/// <li><b>getSelectedItem()</b> - Returns the currently-selected value.  This is
	/// the method used by the super class and should not be used.  Instead, use
	/// getSelected().  If used with any combo box, editable or not, it will only 
	/// return the currently-selected value, never anything the user has typed in.</li>
	/// <li><b>getStringAt()</b> - This returns the value in the list of combo box 
	/// values that is stored at a specified position.  Because the list of values is 
	/// stored internally, this can never be used to return a user-entered value, 
	/// <b>unless</b> the value is inserted in the list after being typed in the combo 
	/// box field.</li>
	/// <li><b>getSelectedIndex()</b> - In a non-editable combo box, this will return
	/// the index of the currently-selected value.  In an editable combox box, it will
	/// do the same, unless the user has typed in a value.  In that case, it will return -1.</li>
	/// </ul>
	/// </para>
	/// </summary>
	public class SimpleJComboBox : JComboBox
	{

	/// <summary>
	/// Refers to the very last position in the combo box, for use with setSelectionFailureFallback.
	/// </summary>
	public int LAST = -999;

	/// <summary>
	/// Whether the text field for this combo box is editable or not.
	/// </summary>
	private bool __editable = false;

	/// <summary>
	/// The position at which fall back text (see setSelectionFailureFallback) will be inserted in the combo box.
	/// </summary>
	private int __fallbackPos = -1;

	/// <summary>
	/// The fallback text to be inserted in the combo box (see setSelectionFailureFallback).
	/// </summary>
	private string __fallbackString = "";

	/// <summary>
	/// Constructor
	/// </summary>
	public SimpleJComboBox() : base()
	{
		initialize(-1, false);
	}

	/// <summary>
	/// Creates a JComboBox that takes it's items from an existing ComboBoxModel.
	/// </summary>
	public SimpleJComboBox(ComboBoxModel aModel) : base(aModel)
	{
			initialize(-1, false);
	}

	/// <summary>
	/// Constructor.
	/// <para><b>Note:</b> if using an editable combo box, the method 
	/// <tt>getSelected()</tt> should be used instead of <tt>getSelectedItem()</tt>.
	/// </para>
	/// </summary>
	/// <param name="editable"> if true, then the values in the combo box can be edited. </param>
	public SimpleJComboBox(bool editable) : base()
	{
		initialize(-1, editable);
	}

	/// <summary>
	/// Constructor.  Also populates the SimpleJComboBox with the contents of the list passed in.
	/// The default width of the SimpleJComboBox will be the width of the widest String in the list.
	/// <para><b>Note:</b> if using an editable combo box, the method 
	/// <tt>getSelected()</tt> should be used instead of <tt>getSelectedItem()</tt>.
	/// </para>
	/// </summary>
	/// <param name="v"> a list of Strings to be placed in the SimpleJComboBox. </param>
	public SimpleJComboBox(System.Collections.IList v) : base(new List<object>(v))
	{
		initialize(-1, false);
	}

	/// <summary>
	/// Constructor.  Also populates the SimpleJComboBox with the contents of the list passed in.
	/// The default width of the SimpleJComboBox will be the width of the widest String in the list.
	/// <para><b>Note:</b> if using an editable combo box, the method 
	/// <tt>getSelected()</tt> should be used instead of <tt>getSelectedItem()</tt>.
	/// </para>
	/// </summary>
	/// <param name="v"> a list of Strings to be placed in the SimpleJComboBox. </param>
	/// <param name="editable"> if true, then the values in the combo box can be edited. </param>
	public SimpleJComboBox(System.Collections.IList v, bool editable) : base(new List<object>(v))
	{
		initialize(-1, editable);
	}

	/// <summary>
	/// Constructor.  Sets the default width of the SimpleJComboBox and also sets 
	/// whether the SimpleJComboBox is editable or not.
	/// <para><b>Note:</b> if using an editable combo box, the method 
	/// <tt>getSelected()</tt> should be used instead of <tt>getSelectedItem()</tt>.
	/// </para>
	/// </summary>
	/// <param name="defaultSize"> the default field width of the SimpleJComboBox. </param>
	/// <param name="editable"> whether the SimpleJComboBox should be editable (true) or not. </param>
	public SimpleJComboBox(int defaultSize, bool editable) : base()
	{
		initialize(defaultSize, editable);
	}

	/// <summary>
	/// Constructor.  Populates the SimpleJComboBox with the contents of the list of Strings passed in,
	/// sets the default field width, and whether the combo box is editable.
	/// <para><b>Note:</b> if using an editable combo box, the method 
	/// <tt>getSelected()</tt> should be used instead of <tt>getSelectedItem()</tt>.
	/// </para>
	/// </summary>
	/// <param name="v"> a list of Strings to be placed in the SimpleJComboBox. </param>
	/// <param name="defaultSize"> the default field width of the SimpleJComboBox. </param>
	/// <param name="editable"> whether the SimpleJComboBox should be editable (true) or not. </param>
	public SimpleJComboBox(System.Collections.IList v, int defaultSize, bool editable) : base(new List<object>(v))
	{
		initialize(defaultSize, editable);
	}

	/// <summary>
	/// Finalize method.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~SimpleJComboBox()
	{
		__fallbackString = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Adds a String to the end of the SimpleJComboBox. </summary>
	/// <param name="s"> the String to add to the SimpleJComboBox. </param>
	public virtual void add(string s)
	{
		addItem(s);
	}

	/// <summary>
	/// Adds a string to a list of strings in a SimpleJComboBox and adds it in alphabetical order. </summary>
	/// <param name="s"> the string to add alphabetically to the combo box </param>
	public virtual void addAlpha(string s)
	{
		int size = getItemCount();
		for (int i = 0; i < size; i++)
		{
			int comp = s.CompareTo((string)getItemAt(i));
			if (comp < 0)
			{
				addAt(s, i);
				return;
			}
		}
		add(s);
	}

	/// <summary>
	/// Adds a string to a list of strings in a SimpleJComboBox and adds it in alphabetical order. </summary>
	/// <param name="s"> the string to add alphabetically to the combo box </param>
	/// <param name="skip"> the number of initial rows to skip before doing the alphabetical comparison. </param>
	public virtual void addAlpha(string s, int skip)
	{
		int size = getItemCount();
		for (int i = skip; i < size; i++)
		{
			int comp = s.CompareTo((string)getItemAt(i));
			if (comp < 0)
			{
				addAt(s, i);
				return;
			}
		}
		add(s);
	}

	/// <summary>
	/// Adds a set of Listeners to the SimpleJComboBox.  The ActionListener is 
	/// used to tell when the SimpleJComboBox selection changes, and the KeyListener
	/// is placed on the SimpleJComboBox's text field to tell whenever a key is pressed in the SimpleJComboBox. </summary>
	/// <param name="a"> an ActionListener. </param>
	/// <param name="k"> a KeyListner. </param>
	public virtual void addActionAndKeyListeners(ActionListener a, KeyListener k)
	{
		addActionListener(a);
		addTextFieldKeyListener(k);
	}

	/// <summary>
	/// Adds a String at a certain point in the SimpleJComboBox. </summary>
	/// <param name="s"> the String to add to the SimpleJComboBox. </param>
	/// <param name="loc"> the location at which the String should be inserted. </param>
	public virtual void addAt(string s, int loc)
	{
		insertItemAt(s, loc);
	}

	/// <summary>
	/// Adds a KeyListener to the SimpleJComboBox's text field. </summary>
	/// <param name="k"> a KeyListener. </param>
	public virtual void addTextFieldKeyListener(KeyListener k)
	{
		((JTextComponent)getEditor().getEditorComponent()).addKeyListener(k);
	}

	/// <summary>
	/// Searches through the SimpleJComboBox to see if it contains a given String. </summary>
	/// <param name="s"> the String for which to search the SimpleJComboBox. </param>
	/// <returns> true if the String is contained already by the SimpleJComboBox, 
	/// or false if it is not.  False is returned if s is null. </returns>
	public virtual bool contains(string s)
	{
		if (string.ReferenceEquals(s, null))
		{
			return false;
		}
		int size = getItemCount();
		for (int i = 0; i < size; i++)
		{
			if (s.Equals((string)getItemAt(i)))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Returns the text currently displayed by the Simple JComboBox -- more
	/// specifically, the text that has been entered by the user into the editable
	/// text field of the combo box. </summary>
	/// <returns> a String containing the text of the text field of the SimpleJComboBox. </returns>
	public virtual string getFieldText()
	{
		return ((JTextComponent)getEditor().getEditorComponent()).getText();
	}

	/// <summary>
	/// Returns the String at the given index.  Mimics Choice's getItem(int) method.
	/// If any edits have been made to the combo box (for instance, if it is editable 
	/// and the user has entered a new value), those edits will not be represented in this call.<para>
	/// If the location is out of bounds, null is returned.
	/// </para>
	/// </summary>
	/// <param name="location"> the index in the SimpleJComboBox of the item to return. </param>
	/// <returns> the String at the given index. </returns>
	public virtual string getItem(int location)
	{
		return (string)getItemAt(location);
	}

	/// <summary>
	/// Returns the list item at the specified index.  If index is out of range (less
	/// than zero or greater than or equal to size), it will return null.  If any
	/// edits have been made to the combo box (for instance, if it is editable and
	/// the user has entered a new value), those edits will not be represented in this call.<para>
	/// If the location is out of bounds, null is returned.
	/// </para>
	/// </summary>
	/// <param name="index"> an integer indicating the list position, where the first item starts at zero. </param>
	/// <returns> the Object at that list position, or null if out of range. </returns>
	public virtual object getItemAt(int index)
	{
		return base.getItemAt(index);
	}

	/// <summary>
	/// Searches through the SimpleJComboBox to see if it contains a given 
	/// String and then returns the position in the SimpleJComboBox of the String. </summary>
	/// <param name="s"> the String for which to search in the SimpleJComboBox. </param>
	/// <returns> the numeric location of the String in the SimpleJComboBox 
	/// (base 0), or -1 if the String was not found. </returns>
	public virtual int getPosition(string s)
	{
		int size = getItemCount();
		for (int i = 0; i < size; i++)
		{
			if (s.Equals((string)getItemAt(i)))
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// Returns the currently-selected SimpleJComboBox option's text.  If editable, return getFieldText().
	/// Otherwise, return getSelectedItem(). </summary>
	/// <returns> a String containing the text of the currently-selected SimpleJComboBox value. </returns>
	public virtual string getSelected()
	{
		if (__editable)
		{
			return getFieldText();
		}
		else
		{
			return (string)getSelectedItem();
		}
	}

	/// <summary>
	/// Returns the current selected item.<para>
	/// <B>Don't use.  Use getSelected() instead, or strange behavior may be 
	/// </para>
	/// encountered with editable combo boxes.</b><para>
	/// If the combo box is editable, then this value may not have been added to the 
	/// combo box with <tt>addItem</tt>, <tt>insertItemAt</tt>, or the data constructors.
	/// </para>
	/// </summary>
	/// <returns> the current selected Object. </returns>
	public virtual object getSelectedItem()
	{
		return base.getSelectedItem();
	}

	/// <summary>
	/// Returns the value stored at the specified position. </summary>
	/// <param name="i"> the position from which to return the String. </param>
	/// <returns> the value stored at the specified position. </returns>
	public virtual string getStringAt(int i)
	{
		return (string)(base.getItemAt(i));
	}

	/// <summary>
	/// Returns the combo box's text editor.  This is what the user types values into
	/// in an editable combo box.  In a non-editable combo box, this returns null. </summary>
	/// <returns> the combo box's text editor, or null if the combo box is uneditable. </returns>
	public virtual JTextComponent getJTextComponent()
	{
		return ((JTextComponent)(getEditor().getEditorComponent()));
	}

	/// <summary>
	/// Searches through the SimpleJComboBox to see if it contains a given String and 
	/// returns the index of the string in the box. </summary>
	/// <param name="s"> the String for which to search the SimpleJComboBox. </param>
	/// <returns> -1 if the string is not found, or the index of the first match. </returns>
	public virtual int indexOf(string s)
	{
		int size = getItemCount();
		for (int i = 0; i < size; i++)
		{
			if (s.Equals((string)getItemAt(i)))
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// Initializes the SimpleJComboBox with a defaultSize and editable value.  If 
	/// defaultSize is set to -1, the defaultSize will be calculated from the width
	/// of the widest String in the SimpleJComboBox. </summary>
	/// <param name="defaultSize"> the width to make the SimpleJComboBox.  If defaultSize 
	/// is set to -1, the width will be calculated from the width of the widest String in the SimpleJComboBox. </param>
	/// <param name="editable"> whether the SimpleJComboBox should be editable (true) or not. </param>
	private void initialize(int defaultSize, bool editable)
	{
		if (defaultSize > -1)
		{
			string s = "";
			for (int i = 0; i < defaultSize; i++)
			{
				s += "W";
			}
			setPrototypeDisplayValue(s);
		}

		setEditable(editable);
		__editable = editable;
	}

	/// <summary>
	/// Inserts a String at the given position into the SimpleJComboBox.  Mimics Choice's insert(String, int) method. </summary>
	/// <param name="str"> the String to be inserted. </param>
	/// <param name="location"> the index at which to insert the String. </param>
	public virtual void insert(string str, int location)
	{
		insertItemAt(str, location);
	}

	/// <summary>
	/// Inserts an item into the item list at a given index.  This method works only
	/// if the <tt>JComboBox</tt> uses a mutable data model.  This method will not
	/// work if the Object to be inserted is not a String. </summary>
	/// <param name="anObject"> the Object to add to the list. </param>
	/// <param name="index"> an integer specifying the position at which to add the item. </param>
	public virtual void insertItemAt(object anObject, int index)
	{
		if (anObject == null || anObject is string)
		{
			base.insertItemAt(anObject, index);
		}
	}

	/// <summary>
	/// Returns whether the text is editable or not. </summary>
	/// <returns> whether the text is editable or not. </returns>
	public virtual bool isEditable()
	{
		return __editable;
	}

	/// <summary>
	/// Removes the String at the given Index from the SimpleJComboBox.  Mimics Choice's remove(int) method. </summary>
	/// <param name="location"> the index in the SimpleJComboBox of the String to be removed.
	/// If location is greater than the number of elements in the combo box, or less
	/// than 0, nothing will be done. </param>
	public virtual void removeAt(int location)
	{
		if (location >= getItemCount() || location < 0)
		{
			return;
		}
		else
		{
			removeItemAt(location);
		}
	}

	/// <summary>
	/// Removes the first occurrence of a String from the SimpleJComboBox.  
	/// Mimics Choice's remove(String) method. </summary>
	/// <param name="s"> the String to remove from the SimpleJComboBox. </param>
	/// <returns> true if the String was found and removed, or false if the String
	/// was not found in the SimpleJComboBox. </returns>
	public virtual bool remove(string s)
	{
		if (contains(s))
		{
			removeItem(s);
			return true;
		}
		return false;
	}

	/// <summary>
	/// Removes all items from the SimpleJComboBox.  Mimics Choice's removeAll() method.
	/// </summary>
	public virtual void removeAll()
	{
		removeAllItems();
	}

	/// <summary>
	/// Removes the KeyListener from the SimpleJComboBox's text field. </summary>
	/// <param name="k"> the KeyListener to remove. </param>
	public virtual void removeTextFieldKeyListener(KeyListener k)
	{
		((JTextComponent)getEditor().getEditorComponent()).removeKeyListener(k);
	}

	/// <summary>
	/// Sets the String at the given index as the currently-selected String.  Mimics Choice's select(int) method. </summary>
	/// <param name="location"> the index in the SimpleJComboBox of the String to be the
	/// currently-selected String. </param>
	public virtual void select(int location)
	{
		setSelectedIndex(location);
	}

	/// <summary>
	/// Sets the given String as the currently-selected String.  Mimics Choice's select(String) method. </summary>
	/// <param name="str"> the String to set as the currently-selected String. </param>
	public virtual void select(string str)
	{
		setSelectedItem(str);
	}

	/// <summary>
	/// Selects the given String (if it exists in the combo box), ignoring case.
	/// If the string does not exist in the combo box, no change will be made to the current selection. </summary>
	/// <param name="str"> the String to select. </param>
	public virtual void selectIgnoreCase(string str)
	{
		int size = getItemCount();
		string s = null;
		for (int i = 0; i < size; i++)
		{
			s = getStringAt(i);
			if (s.Equals(str, StringComparison.OrdinalIgnoreCase))
			{
				select(i);
				return;
			}
		}
	}

	/// <summary>
	/// Sets the data stored in the combo box all at once. </summary>
	/// <param name="v"> a list of Strings, each of which will be an item in the combo box. </param>
	public virtual void setData(System.Collections.IList v)
	{
		setModel(new DefaultComboBoxModel(new List<object>(v)));
		repaint();
	}

	/// <summary>
	/// Sets whether the textfield of this combobox is editable or not.
	/// <para><b>Note:</b> if using an editable combo box, the method 
	/// <tt>getSelected()</tt> should be used instead of <tt>getSelectedItem()</tt>.
	/// </para>
	/// </summary>
	/// <param name="editable"> whether the textfield is editable or not. </param>
	public virtual void setEditable(bool editable)
	{
		__editable = editable;
		base.setEditable(editable);
	}

	/// <summary>
	/// Sets the selected item in the combo box display area to the object in the
	/// argument.  If anObject is in the list, the display area shows anObject
	/// selected.  <b>anObject must be a String or nothing will happen</b>.<para>
	/// If anObject is <i>not</i> in the list and the combo box is uneditable, it
	/// </para>
	/// will not change the current selection.  For editable combo boxes, the selection will change to anObject.<para>
	/// If this constitutes a change in the selected item, ItemListeners added to the 
	/// combo box will be notified with one or two ItemEvents.  If there is a current
	/// selected item, an ItemEvent will be fired and the state change will be 
	/// ItemEvent.DESELECTED.  If anObject is in the list and is not currently 
	/// </para>
	/// selected then an ItemEvent will be fired and the StateChange will be ItemEvent.SELECTED.<para>
	/// ActionListeners added to the combo box will be notified with an ActionEvent when this method is called.
	/// </para>
	/// </summary>
	/// <param name="anObject"> the list object to select; use null to clear the selection. </param>
	public virtual void setSelectedItem(object anObject)
	{
		if (anObject == null || anObject is string)
		{
			base.setSelectedItem(anObject);
		}
	}

	/// <summary>
	/// Sets the currently selected item to be the first value in the list that 
	/// starts with the characters in prefix (case-sensitive). </summary>
	/// <param name="prefix"> the prefix to match. </param>
	/// <returns> true if a matching item was found; false if not. </returns>
	public virtual bool setSelectedPrefixItem(string prefix)
	{
		int size = getItemCount();
		string s = null;
		for (int i = 0; i < size; i++)
		{
			s = (string)getItemAt(i);
			if (s.StartsWith(prefix, StringComparison.Ordinal))
			{
				setSelectedIndex(i);
				return true;
			}
		}
		if (__fallbackPos != -1 && !prefix.Trim().Equals(""))
		{
			if (__fallbackPos == LAST)
			{
				if (string.ReferenceEquals(__fallbackString, null))
				{
					select(getItemCount() - 1);
				}
				else
				{
					int index = __fallbackString.IndexOf("~", StringComparison.Ordinal);
					string value = null;
					if (index > -1)
					{
						value = __fallbackString.Substring(0, index);
						value += prefix;
						value += __fallbackString.Substring(index + 1);
					}
					else
					{
						value = __fallbackString;
					}
					addAt(value, getItemCount());
					select(getItemCount() - 1);
				}
			}
			else
			{
				if (string.ReferenceEquals(__fallbackString, null))
				{
					select(__fallbackPos);
				}
				else
				{
					int index = __fallbackString.IndexOf("~", StringComparison.Ordinal);
					string value = null;
					if (index > -1)
					{
						value = __fallbackString.Substring(0, index);
						value += prefix;
						value += __fallbackString.Substring(index + 1);
					}
					else
					{
						value = __fallbackString;
					}
					if (__fallbackPos == 0)
					{
						insertItemAt(value, __fallbackPos);
					}
					else
					{
						addAt(value, __fallbackPos);
					}
					select(__fallbackPos);
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Sets the item that should be inserted into the list if no items could be
	/// matched by selecting a specific prefix.  If there is a "~" in the fallback
	/// String, then whatever the value that was to be matched was will be inserted
	/// in the string at the position of the first tilde.<para>
	/// </para>
	/// For instance if the following were done:<para>
	/// <pre>	comboBox.setSelectionFailureFallback("Value (~) not found", 0);</pre>
	/// And a call was made to <pre>setSelectedPrefixItem("Station 1");</pre><br>
	/// If no Strings were found that match the prefix "Station 1", the following
	/// </para>
	/// String would be inserted at the very beginning of the Combo Box:<para>
	/// <pre>	"Value (Station 1) not found"</pre>
	/// </para>
	/// </summary>
	/// <param name="text"> the fallback String to insert in the combo box if the selected
	/// prefix item could not be found.  If null, the fallback String process will be disabled. </param>
	/// <param name="i"> the position in the combo box at which to insert the fallback item.
	/// If -1, the fallback String process will be disabled. </param>
	public virtual void setSelectionFailureFallback(string text, int i)
	{
		if (string.ReferenceEquals(text, null) || i == -1)
		{
			__fallbackString = null;
			__fallbackPos = -1;
		}
		else
		{
			__fallbackString = text;
			__fallbackPos = i;
		}
	}

	/// <summary>
	/// Sets the current text of the SimpleJComboBox to the given String.  
	/// If the String is already in the SimpleJComboBox, then that element of the 
	/// SimpleJComboBox is made the currently-selected element.  If the String is 
	/// not found, the String will be added and then made the currently-selected element. </summary>
	/// <param name="s"> the String to set the currently-selected element to. </param>
	public virtual void setText(string s)
	{
		if (!contains(s))
		{
			add(s);
		}
		setSelectedItem(s);
	}

	}

}