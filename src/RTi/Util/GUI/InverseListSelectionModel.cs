// InverseListSelectionModel - class to allow inverse list selections (all selected first)

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
// InverseListSelectionModel - class to allow inverse list selections, where
// 	all values are selected by default and the user selects the items to
// 	deselect.
//---------------------------------------------------------------------------
// Copyright:  See the COPYRIGHT file.
//---------------------------------------------------------------------------
// History:
// 2003-03-26	J. Thomas Sapienza, RTi	Initial version.
// 2005-04-26	JTS, RTi		Added finalize().
//---------------------------------------------------------------------------

namespace RTi.Util.GUI
{

	/// <summary>
	/// This class provides a list selection model in which the default
	/// mode is for all the rows to be selected and cells are deselected when they
	/// are clicked on.  This is the inverse of the way the list usually works.  <para>
	/// Developers do not need to interact directly with this class, instead, they 
	/// should use a SimpleJList and call setInverseListSelection(true) on that, like
	/// </para>
	/// the following:<para>
	/// <code>
	///        SimpleJList divisionJList = new SimpleJList(names);
	/// divisionJList.setInverseListSelection(true);
	/// </code>
	/// </para>
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class InverseListSelectionModel extends javax.swing.DefaultListSelectionModel
	public class InverseListSelectionModel : DefaultListSelectionModel
	{

	/// <summary>
	/// An array which remembers which rows should (true) and shouldn't be (false)
	/// selected.
	/// </summary>
	private bool[] __selected;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="size"> the number of elements in the JList that should be monitored 
	/// for selection or not. </param>
	public InverseListSelectionModel(int size)
	{
		initialize(size);
	}

	/// <summary>
	/// Used when a new item is added to the list to preserve the old selections. </summary>
	/// <param name="index"> the location of the new item. </param>
	public virtual void add(int index)
	{
		int length = __selected.Length;
		bool[] temp = new bool[length];
		for (int i = 0; i < length; i++)
		{
			temp[i] = __selected[i];
		}
		__selected = new bool[length + 1];
		if (index == (length + 1))
		{
			__selected[index - 1] = false;
			return;
		}
		int i = 0;
		for (i = 0; i < index; i++)
		{
			__selected[i] = temp[i];
		}
		__selected[i] = false;
		for (i = (index + 1); i < length; i++)
		{
			__selected[i] = temp[i];
		}
	}

	/// <summary>
	/// Adds an interval of values to the selected list. 
	/// Overrides method in DefaultListSelectionModel. </summary>
	/// <param name="row0"> the first row to be selected. </param>
	/// <param name="row1"> the last row to be selected. </param>
	public virtual void addSelectionInterval(int row0, int row1)
	{
		__selected[row0] = true;
		base.addSelectionInterval(row0, row0);
	}

	/// <summary>
	/// Clears all selected rows.  Overrides method in DefaultListSelectionModel.
	/// </summary>
	public virtual void clearSelection()
	{
		for (int i = 0; i < __selected.Length; i++)
		{
			__selected[i] = false;
		}
		base.removeSelectionInterval(0, (__selected.Length - 1));
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~InverseListSelectionModel()
	{
		__selected = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Gets the highest-selected row.  Overrides method in DefaultListSelectionModel. </summary>
	/// <returns> the highest selected row, or -1 if no rows are selected. </returns>
	public virtual int getMaxSelectionIndex()
	{
		for (int i = (__selected.Length - 1); i >= 0; i--)
		{
			if (__selected[i])
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// Gets the lowest-selected row.  Overrides method in DefaultListSelectionModel. </summary>
	/// <returns> the lowest-selected row, or -1 if no rows are selected. </returns>
	public virtual int getMinSelectionIndex()
	{
		for (int i = 0; i < __selected.Length; i++)
		{
			if (__selected[i])
			{
				return i;
			}
		}
		return -1;
	}


	/// <summary>
	/// Initializes the __selected array and also initializes the entire list to be
	/// selected. </summary>
	/// <param name="size"> the size of the JList to be monitored for selection. </param>
	private void initialize(int size)
	{
		__selected = new bool[size];
		for (int i = 0; i < size; i++)
		{
			__selected[i] = true;
		}
		base.setSelectionInterval(0, size - 1);
	}

	/// <summary>
	/// Returns true if the given row is selected.  Overrides method in 
	/// DefaultListSelectionModel. </summary>
	/// <param name="row"> the row to check if it is selected. </param>
	/// <returns> true if the row is selected, false otherwise. </returns>
	public virtual bool isSelected(int row)
	{
		if (__selected[row] == true)
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Returns true if the list has no rows selected.  
	/// Overrides method in DefaultListSelectionModel. </summary>
	/// <returns> true if the list has no rows selected, false otherwise. </returns>
	public virtual bool isSelectionEmpty()
	{
		for (int i = 0; i < __selected.Length; i++)
		{
			if (__selected[i])
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Removes the row from the list of selected rows and shifts all the higher rows'
	/// selection values down in the array. </summary>
	/// <param name="index"> the location of the new item. </param>
	public virtual void remove(int index)
	{
		int length = __selected.Length;
		bool[] temp = new bool[length];
		for (int i = 0; i < length; i++)
		{
			temp[i] = __selected[i];
		}
		__selected = new bool[length - 1];
		for (int i = 0; i < index; i++)
		{
			__selected[i] = temp[i];
		}
		for (int i = (index + 1); i < length; i++)
		{
			__selected[i] = temp[i];
		}
	}

	/// <summary>
	/// Removes an interval of values from the selected list.
	/// Overrides method in DefaultListSelectionModel; </summary>
	/// <param name="row0"> the first row to be selected. </param>
	/// <param name="row1"> the last row to be selected. </param>
	public virtual void removeSelectionInterval(int row0, int row1)
	{
		if (__selected[row0] == true)
		{
			__selected[row0] = false;
			base.removeSelectionInterval(row0, row0);
		}
		else
		{
			__selected[row0] = true;
			base.addSelectionInterval(row0, row0);
		}
	}

	/// <summary>
	/// Selects all the rows in the list.
	/// </summary>
	public virtual void selectAll()
	{
		int length = __selected.Length;
		for (int i = 0; i < length; i++)
		{
			__selected[i] = true;
			base.addSelectionInterval(i, i);
		}
	}

	/// <summary>
	/// Sets a selection of rows as selected.
	/// Overrides method in DefaultListSelectionModel; </summary>
	/// <param name="row0"> the first row to be selected. </param>
	/// <param name="row1"> the last row to be selected. </param>
	public virtual void setSelectionInterval(int row0, int row1)
	{
		if (__selected[row0] == true)
		{
			__selected[row0] = false;
			base.removeSelectionInterval(row0, row0);
		}
		else
		{
			__selected[row0] = true;
			base.addSelectionInterval(row0, row0);
		}
	}

	/// <summary>
	/// Updates the list selection model to monitor a JList of a different size. </summary>
	/// <param name="size"> the size of the JList to be monitored for selection. </param>
	public virtual void update(int size)
	{
		initialize(size);
	}

	}

}