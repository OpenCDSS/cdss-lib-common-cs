// DragAndDropDefaultListCellRenderer - a cell renderer for JComboBox lists, for drag and drop

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
// DragAndDropDefaultListCellRenderer - a cell renderer for JComboBox lists,
// 	allowing data to be dragged onto a combo box.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2004-03-01	J. Thomas Sapienza, RTi	Initial version.
// 2004-04-27	JTS, RTi		Revised after SAM's review.
// 2005-04-26	JTS, RTi		Added finalize().
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.Util.GUI
{


	/// <summary>
	/// This class is a cell renderer that allows the dragging of data onto a 
	/// SimpleJComboBox.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class DragAndDropDefaultListCellRenderer extends javax.swing.DefaultListCellRenderer
	public class DragAndDropDefaultListCellRenderer : DefaultListCellRenderer
	{

	/// <summary>
	/// The combo box for which this class will render data.
	/// </summary>
	private DragAndDropSimpleJComboBox __comboBox = null;

	/// <summary>
	/// Constructs a default renderer object for an item
	/// in a list.
	/// </summary>
	public DragAndDropDefaultListCellRenderer(DragAndDropSimpleJComboBox comboBox) : base()
	{
		__comboBox = comboBox;
		setOpaque(true);
		setBorder(new EmptyBorder(1, 1, 1, 1));
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~DragAndDropDefaultListCellRenderer()
	{
		__comboBox = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	public virtual Component getListCellRendererComponent(JList list, object value, int index, bool isSelected, bool cellHasFocus)
	{
		// set the item that is to be dragged or dropped in the combo box
		if (isSelected)
		{
			__comboBox.setLastSelectedItem("" + value);
		}

		return base.getListCellRendererComponent(list, value, index, isSelected, cellHasFocus);
	}

	}

}