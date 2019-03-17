using System.Collections.Generic;

// SimpleJList - class to allow an easily-changeable JList

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
// SimpleJList - class to allow an easily-changeable JList.
//---------------------------------------------------------------------------
// Copyright:  See the COPYRIGHT file.
//---------------------------------------------------------------------------
// History:
//
// 2002-09-18	J. Thomas Sapienza, RTi	Initial version.
// 2002-09-25	JTS, RTi		Javadoc'd
// 2002-12-17	JTS, RTi		Added 'getSelectedItem()'
// 2003-03-25	JTS, RTi		Added the Array and Vector constructors.
// 2003-03-26	JTS, RTi		Added code to support an optional 
//					InverseListSelectionModel.
// 2004-05-04	JTS, RTi		Added the setListData() methods.
// 2004-05-06	JTS, RTi		Now uses the MutableJList_SelectionModel
// 2005-04-08	JTS, RTi		Renamed from MutableJList.
// 2005-04-26	JTS, RTi		Added finalize().
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//---------------------------------------------------------------------------

namespace RTi.Util.GUI
{



	/// <summary>
	/// Class wraps around JList and provides functionality for dynamically 
	/// changing the contents of JList.<para>
	/// </para>
	/// Example of use:<para>
	/// <code>
	/// SimpleJList sourceList = new SimpleJList();
	/// sourceList.addMouseListener(this);
	/// sourceList.add(RT);
	/// sourceList.add(WIS);
	/// sourceList.addListSelectionListener(this);
	/// </code>
	/// </para>
	/// </summary>
	public class SimpleJList : JList, ListSelectionListener
	{

	/// <summary>
	/// Whether to automatically update the list or not.
	/// </summary>
	private bool __autoUpdate = true;

	/// <summary>
	/// The List model that the list will use.
	/// </summary>
	private DefaultListModel __dlm;

	/// <summary>
	/// The InverseListSelectionModel that can possibly be used.
	/// </summary>
	private InverseListSelectionModel __ilsm = null;

	/// <summary>
	/// The list selection model used by default.
	/// </summary>
	private SimpleJList_SelectionModel __mjlsm = null;

	/// <summary>
	/// Constructor.
	/// </summary>
	public SimpleJList() : base()
	{
		__dlm = new DefaultListModel();
		initialize();
		setModel(__dlm);
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="array"> an array of objects that will be used to populate the list. </param>
	public SimpleJList(object[] array) : base()
	{
		__dlm = new DefaultListModel();
		for (int i = 0; i < array.Length; i++)
		{
			__dlm.addElement(array[i]);
		}
		initialize();
		setModel(__dlm);
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="vector"> a list of objects that will be used to populate the list. </param>
	public SimpleJList(System.Collections.IList vector) : base()
	{
		__dlm = new DefaultListModel();
		int size = vector.Count;
		for (int i = 0; i < size; i++)
		{
			__dlm.addElement(vector[i]);
		}
		initialize();
		setModel(__dlm);
	}

	/// <summary>
	/// Adds an object to the JList in the last position. </summary>
	/// <param name="o"> the object to add to the list. </param>
	public virtual void add(object o)
	{
		__dlm.addElement(o);
		if (__ilsm != null)
		{
			__ilsm.add(__dlm.size());
		}
		else
		{
			__mjlsm.add(__dlm.size());
		}
		update();
	}

	/// <summary>
	/// Adds an object to the JList in the given position. </summary>
	/// <param name="o"> the object to add to the list </param>
	/// <param name="pos"> the position in the list at which to put the object. </param>
	public virtual void add(object o, int pos)
	{
		__dlm.add(pos, o);
		if (__ilsm != null)
		{
			__ilsm.add(pos);
		}
		else
		{
			__mjlsm.add(pos);
		}
		update();
	}

	/// <summary>
	/// Deselects the specified row.  Does not work with InverseListSelectionModels. </summary>
	/// <param name="row"> the row to deselect. </param>
	public virtual void deselectRow(int row)
	{
		if (__ilsm != null)
		{
			return;
		}
		((SimpleJList_SelectionModel)getSelectionModel()).deselectRow(row);
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~SimpleJList()
	{
		__dlm = null;
		__mjlsm = null;
		__ilsm = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns whether automatic updating of the list is turned on </summary>
	/// <returns> __autoUpdate </returns>
	/// <seealso cref= #setAutoUpdate </seealso>
	public virtual bool getAutoupdate()
	{
		return __autoUpdate;
	}

	/// <summary>
	/// Returns the Object at the given position. </summary>
	/// <param name="pos"> the position at which to return the Object. </param>
	/// <returns> the Object at the given position. </returns>
	public virtual object getItem(int pos)
	{
		return __dlm.get(pos);
	}

	/// <summary>
	/// Returns the number of items in the list. </summary>
	/// <returns> the number of items in the list. </returns>
	public virtual int getItemCount()
	{
		return __dlm.size();
	}

	/// <summary>
	/// Returns all the items in the list. </summary>
	/// <returns> all the items in the list. </returns>
	public virtual System.Collections.IList getItems()
	{
		System.Collections.IList v = new List<object>(__dlm.size());
		for (int i = 0; i < __dlm.size(); i++)
		{
			v.Add(__dlm.get(i));
		}
		return v;
	}

	/// <summary>
	/// Returns the number of items in the list. </summary>
	/// <returns> the number of items in the list. </returns>
	public virtual int getListSize()
	{
		return __dlm.size();
	}

	/// <summary>
	/// Returns only the first selected item in the list. </summary>
	/// <returns> only the first selected item in the list. </returns>
	public virtual object getSelectedItem()
	{
		int[] indices = getSelectedIndices();

		return __dlm.get(indices[0]);
	}

	/// <summary>
	/// Returns only the selected items in the list.  The returned Vector is guaranteed to be non-null. </summary>
	/// <returns> only the selected items in the list. </returns>
	public virtual System.Collections.IList getSelectedItems()
	{
		System.Collections.IList v = new List<object>(getSelectedSize());
		int[] indices = getSelectedIndices();
		for (int i = 0; i < indices.Length; i++)
		{
			v.Add(__dlm.get(indices[i]));
		}
		return v;
	}

	/// <summary>
	/// Returns the number of rows selected in the list. </summary>
	/// <returns> the number of rows selected in the list. </returns>
	public virtual int getSelectedSize()
	{
		int[] indices = getSelectedIndices();
		return indices.Length;
	}

	/// <summary>
	/// Returns the index of the given object in the list. </summary>
	/// <param name="o"> the Object for which to search in the list. </param>
	/// <returns> the index of the given object in the list. </returns>
	public virtual int indexOf(object o)
	{
		return __dlm.indexOf(o);
	}

	/// <summary>
	/// Returns the first index of the given object in the list, starting from a
	/// certain point. </summary>
	/// <param name="o"> the Object for which to search in the list. </param>
	/// <param name="pos"> the position from which to start searching. </param>
	/// <returns> the index of the object in the list </returns>
	public virtual int indexOf(object o, int pos)
	{
		return __dlm.indexOf(o, pos);
	}

	/// <summary>
	/// Initializes internal settings.
	/// </summary>
	private void initialize()
	{
		__mjlsm = new SimpleJList_SelectionModel(getItemCount());
		setSelectionModel(__mjlsm);
		__mjlsm.setSupportsDragAndDrop(true);
		__mjlsm.addListSelectionListener(this);
		addMouseListener(__mjlsm);
	}

	/// <summary>
	/// Removes the object at the given position. </summary>
	/// <param name="pos"> the position of the Object to be removed. </param>
	public virtual void remove(int pos)
	{
		__dlm.remove(pos);
		if (__ilsm != null)
		{
			__ilsm.remove(pos);
		}
		else
		{
			__mjlsm.remove(pos);
		}
		update();
	}

	/// <summary>
	/// Removes a given Object from the list </summary>
	/// <param name="o"> the Object to be removed. </param>
	public virtual void remove(object o)
	{
		int index = indexOf(o);
		__dlm.removeElement(o);
		if (__ilsm != null)
		{
			__ilsm.remove(index);
		}
		else
		{
			__mjlsm.remove(index);
		}
		update();
	}

	/// <summary>
	/// Removes all objects form the list
	/// </summary>
	public virtual void removeAll()
	{
		__dlm.removeAllElements();
		if (__ilsm != null)
		{
			__ilsm.update(0);
		}
		else
		{
			__mjlsm.update(0);
		}
		update();
	}

	/// <summary>
	/// Selects a given row, deselecting all other rows. </summary>
	/// <param name="i"> the row to select </param>
	public virtual void select(int i)
	{
		setSelected(i);
	}

	/// <summary>
	/// Selects a given row, deselecting all other rows. </summary>
	/// <param name="i"> the row to select </param>
	public virtual void selectRow(int i)
	{
		setSelected(i);
	}

	/// <summary>
	/// Selects all the rows in the list.
	/// </summary>
	public virtual void selectAll()
	{
		if (__ilsm != null)
		{
			__ilsm.selectAll();
		}
		else
		{
			setSelectionInterval(0, __dlm.size() - 1);
		}
	}

	/// <summary>
	/// Sets the object at the given position. </summary>
	/// <param name="pos"> the position at which to set the object. </param>
	/// <param name="o"> the object to set in the position, overwriting the old object. </param>
	public virtual void set(int pos, object o)
	{
		__dlm.set(pos, o);
		update();
	}

	/// <summary>
	/// Sets whether the list should be automatically updated any time it is changed
	/// or not. </summary>
	/// <param name="update"> if true, the list will be auto updated every time it changes. </param>
	public virtual void setAutoUpdate(bool update)
	{
		__autoUpdate = update;
	}

	/// <summary>
	/// Sets up the SimpleJList as having an inverse list selection model (or not). </summary>
	/// <param name="inverse"> if true, the list selection model is set to be an 
	/// InverseListSelectionModel.  If false, the normal DefaultListSelectionModel is
	/// used. </param>
	public virtual void setInverseListSelection(bool inverse)
	{
		if (inverse == true)
		{
			__ilsm = new InverseListSelectionModel(__dlm.size());
			setSelectionModel(__ilsm);
			removeMouseListener(__mjlsm);
		}
		else
		{
			__ilsm = null;
			initialize();
		}
	}

	/// <summary>
	/// Sets the data in the list from an array of Objects.  Any data already in the
	/// list will be lost.  Overloads the method in JList. </summary>
	/// <param name="array"> the array of Objects to populate the list with. </param>
	public virtual void setListData(object[] array)
	{
		__dlm = new DefaultListModel();
		for (int i = 0; i < array.Length; i++)
		{
			__dlm.addElement(array[i]);
		}

		setModel(__dlm);
	}

	/// <summary>
	/// Sets the data in the list from a Vector of Objects.  Any data already in the 
	/// list will be lost.  Overloads the method in JList. </summary>
	/// <param name="vector"> the Vector of Objects to populate the list with. </param>
	public virtual void setListData(System.Collections.IList vector)
	{
		__dlm = new DefaultListModel();
		int size = vector.Count;
		for (int i = 0; i < size; i++)
		{
			__dlm.addElement(vector[i]);
		}
		setModel(__dlm);
	}

	/// <summary>
	/// Sets the model to use with this SimpleJList. </summary>
	/// <param name="dlm"> a non-null DefaultListModel to use. </param>
	public virtual void setModel(DefaultListModel dlm)
	{
		base.setModel(dlm);
		if (__ilsm != null)
		{
			__ilsm.update(dlm.size());
		}
		else
		{
			__mjlsm.update(dlm.size());
		}
	}

	/// <summary>
	/// Selects a given row, deselecting all other rows. </summary>
	/// <param name="i"> the row to select </param>
	public virtual void setSelected(int i)
	{
		setSelectedIndex(i);
	}

	/// <summary>
	/// If autoupdate is true, update the list with the current list model
	/// </summary>
	public virtual void update()
	{
		if (__autoUpdate == true)
		{
			setModel(__dlm);
		}
	}

	/// <summary>
	/// If autoupdate is true, update the list with the current list model. </summary>
	/// <param name="update"> autoupdate will be set to this value, so instead of calling
	/// setAutoupdate(true) and then update(), both commands can be combined into
	/// one by calling update(true); </param>
	public virtual void update(bool update)
	{
		__autoUpdate = update;
		if (__autoUpdate == true)
		{
			setModel(__dlm);
		}
	}

	/// <summary>
	/// Repaints the list in response to list changes. From ListSelectionListener. </summary>
	/// <param name="event"> the ListSelectionEvent that happened. </param>
	public virtual void valueChanged(ListSelectionEvent @event)
	{
		repaint();
	}

	}

}