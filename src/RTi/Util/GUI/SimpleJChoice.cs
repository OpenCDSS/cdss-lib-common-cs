// SimpleJChoice - Swing component that mimics the AWT Choice class

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

//-----------------------------------------------------------------------------
// SimpleJChoice - Swing component that mimics the AWT Choice class
//-----------------------------------------------------------------------------
// Copyright:  See the COPYRIGHT file.
//-----------------------------------------------------------------------------
// History:
//
// 2002-11-12	J. Thomas Sapienza, RTi	Initial version.
// 2002-11-13	JTS, RTi		Revised comments.
// 2003-06-16	Steven A. Malers, RTi	Fixed select() and remove() to compare
//					the contents of the string (not the
//					reference value).  Add getItemCount().
//-----------------------------------------------------------------------------

namespace RTi.Util.GUI
{

	/// <summary>
	/// SimpleJChoice copies the behavior of the AWT Choice object, using a 
	/// JComboBox as its underlying class.  It is designed to make things easier
	/// for developers moving between AWT Choices and Swing JComboBoxes.  
	/// The value for the SimpleJChoice's editable setting is always 'false' and
	/// cannot be changed.  If a JComboBox that can be edited is needed, see
	/// SimpleJComboBox.<para>
	/// Notes on use:
	/// <ol>
	/// <li>	This class does not support handling multiple items with the same
	/// String value very well.
	/// 
	/// Suppose a SimpleJChoice has two Strings "XYZ" in the 0th and 10th spots.
	/// These strings are not equivalent via '==', but are equivalent with 
	/// .equals(). 
	/// If the user selects the second one, getSelectedIndex() will return 0 
	/// </para>
	/// (for the first one), instead.<para>
	/// It's not intuitive behavior, and it may be a bug in the underlying
	/// JComboBox class, but that's how the class will perform.</li>
	/// <li>	The method remove(String) will remove only the first occurrence of the
	/// given String, if more than one exists in the list.</li>
	/// </ol>
	/// </para>
	/// </summary>
	/// @deprecated Should use SimpleJComboBox instead, as it has had far more 
	/// development.
	/// REVISIT (JTS - 2006-05-23)
	/// Remove this class! 
	public class SimpleJChoice : JComboBox
	{

	/// <summary>
	/// Constructor.  The Constructor initializes the SimpleJChoice's editable 
	/// setting to 'false', and that cannot be changed. </summary>
	/// @deprecated Use SimpleJComboBox. 
	public SimpleJChoice() : base()
	{
		setEditable(false);
	}

	/// <summary>
	/// Finalize method.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~SimpleJChoice()
	{
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Adds an item to the SimpleJChoice.  Mimics Choice's add(String) method. </summary>
	/// <param name="str"> the String to add to the SimpleJChoice. </param>
	public virtual void add(string str)
	{
		addItem(str);
	}

	/// <summary>
	/// Returns the String location at the given position in the list.  
	/// Mimics Choice's getItem(int) method. </summary>
	/// <param name="location"> the index in the SimpleJChoice of the String to return </param>
	/// <returns> the String at the given location. </returns>
	public virtual string getItem(int location)
	{
		return ((string)getItemAt(location));
	}

	/// <summary>
	/// Returns the number of items in the choice.
	/// Mimics Choice's getItemCount() method. </summary>
	/// <returns> the number of items in the choice. </returns>
	public virtual int getItemCount()
	{
		return getModel().getSize();
	}

	/// <summary>
	/// Returns the currently-selected String. </summary>
	/// <returns> the currently-selected String. </returns>
	public virtual string getSelectedString()
	{
		return ((string)base.getSelectedItem());
	}

	/// <summary>
	/// Inserts the given String in the SimpleJChoice, and puts it in at the given
	/// position. Mimics Choice's insert(String, int) method. </summary>
	/// <param name="str"> the String to be inserted in the SimpleJChoice. </param>
	/// <param name="location"> the index in the SimpleJChoice at which to insert the String. </param>
	public virtual void insert(string str, int location)
	{
		insertItemAt(str, location);
	}

	/// <summary>
	/// Removes the item in the list at the given position from the list.  Mimics 
	/// Choice's remove(int) method. </summary>
	/// <param name="location"> the index in the SimpleJChoice of the item to be removed. </param>
	public virtual void remove(int location)
	{
		removeItemAt(location);
	}

	/// <summary>
	/// Removes the first occurrence of the given String from the SimpleJChoice.  
	/// Mimics Choice's remove(String) method. </summary>
	/// <param name="str"> the String to remove from the list. </param>
	public virtual void remove(string str)
	{
		// Cannot simply call removeItem(str) because we need to compare
		// String contents, not memory locations for objects.  Therefore check
		// each string in the Choice and select the first one that matches.
		int size = getItemCount();
		string item;
		for (int i = 0; i < size; i++)
		{
			item = getItem(i);
			if (item.Equals(str))
			{
				removeItem(str);
				return;
			}
		}
	}

	/// <summary>
	/// Removes all items from the SimpleJChoice.  Mimics Choice's removeAll()
	/// method.
	/// </summary>
	public virtual void removeAll()
	{
		removeAllItems();
	}

	/// <summary>
	/// Makes the String at the given location the currently-selected SimpleJChoice
	/// item.  Mimics Choice's select(int) method. </summary>
	/// <param name="location"> the location in the SimpleJChoice list of the String to make
	/// the currently-selected String. </param>
	public virtual void select(int location)
	{
		setSelectedIndex(location);
	}

	/// <summary>
	/// Makes the given String the currently-selected SimpleJChoice item.  Mimics
	/// Choice's select(String) method.
	/// Note: The comparison of String str and the Strings in the List is 
	/// case-sensitive. </summary>
	/// <param name="str"> the String to make the currently-selected String. </param>
	public virtual void select(string str)
	{
		// Cannot simply call setSelectedItem(str) because we need to compare
		// String contents, not memory locations for objects.  Therefore check
		// each string in the Choice and select the first one that matches.
		int size = getItemCount();
		string item;
		for (int i = 0; i < size; i++)
		{
			item = getItem(i);
			if (item.Equals(str))
			{
				setSelectedItem(item);
				return;
			}
		}
	}

	/// <summary>
	/// Overrides JComboBox's setEditable() method.  setEditable is set to false
	/// in the SimpleJChoice constructor, and is not allowed to be changed.  This 
	/// setEditable() method is empty, and returns instantly upon being called.
	/// </summary>
	public virtual void setEditable(bool b)
	{
		// capture and ignore.  Editable is always false, as set 
		// in the constructor.
	}

	}

}