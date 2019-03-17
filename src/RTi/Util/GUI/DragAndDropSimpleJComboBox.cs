﻿// DragAndDropSimpleJComboBox - a SimpleJComboBox that supports drag and drop capability

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
// DragAndDropSimpleJComboBox - Implements a SimpleJComboBox that supports 
//	drag and drop capability.
//-----------------------------------------------------------------------------
// Copyright: See the COPYRIGHT file.
//-----------------------------------------------------------------------------
// History: 
// 2004-02-24	J. Thomas Sapienza, RTi	Initial version.
// 2004-04-27	JTS, RTi		Revised after SAM's review.
// 2004-06-01	JTS, RTi		In order to support dragging from a
//					combo box in Linux, changed the way
//					drags are handled so that they start
//					from within the popup list that shows
//					all the possible combo box values.
// 2004-09-20	JTS, RTi		Started using a specific 
//					ListCellRenderer so that a user can
//					drag from the list that appears when
//					the combo box is clicked on.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//-----------------------------------------------------------------------------

namespace RTi.Util.GUI
{





	/// <summary>
	/// This class implements a SimpleJComboBox that supports dragging and dropping
	/// text.  Currently only supports drags and drops of DragAndDropTransferPrimitive data.  
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class DragAndDropSimpleJComboBox extends SimpleJComboBox implements java.awt.dnd.DragGestureListener, java.awt.dnd.DragSourceListener, java.awt.dnd.DropTargetListener, DragAndDrop
	public class DragAndDropSimpleJComboBox : SimpleJComboBox, DragGestureListener, DragSourceListener, DropTargetListener, DragAndDrop
	{

	/// <summary>
	/// The DragAndDropControl object that holds information about how this object works with DragAndDrop.
	/// </summary>
	private DragAndDropControl __data = null;

	/// <summary>
	/// The item from the combo box that was dragged.
	/// </summary>
	private int __selectedIndex = -1;

	/// <summary>
	/// The text of the item that was last-selected from the list that appears when the
	/// combo box is clicked on.  This is so that items from the middle of the list can be dragged.
	/// </summary>
	private string __lastSelectedItem = null;

	/// <summary>
	/// Creates a SimpleJComboBox that supports drag and drop. </summary>
	/// <param name="editable"> whether the text in this object is editable.  SimpleJComboBoxes
	/// with editable text cannot support drag operations. </param>
	/// <param name="dragAction"> the action to take when dragging data.  
	/// See DragAndDropUtil.ACTION_*.  Drags can only be performed if the combo box is NON editable. </param>
	/// <param name="dropAction"> the action to take when dropping data.  
	/// See DragAndDropUtil.ACTION_*.  Drops can only be performed if the combo box is editable. </param>
	public DragAndDropSimpleJComboBox(bool editable, int dragAction, int dropAction) : base(editable)
	{
		initialize(dragAction, dropAction);
	}

	/// <summary>
	/// Creates a SimpleJComboBox that supports drag and drop. </summary>
	/// <param name="v"> a Vector of values to initialize the combo box with. </param>
	/// <param name="dragAction"> the action to take when dragging data.  
	/// See DragAndDropUtil.ACTION_*.  Drags can only be performed if the combo box is NON editable. </param>
	/// <param name="dropAction"> the action to take when dropping data.  
	/// See DragAndDropUtil.ACTION_*.  Drops can only be performed if the combo box is editable. </param>
	public DragAndDropSimpleJComboBox(System.Collections.IList v, int dragAction, int dropAction) : base(v)
	{
		initialize(dragAction, dropAction);
	}

	/// <summary>
	/// Creates a SimpleJComboBox that supports drag and drop. </summary>
	/// <param name="v"> a Vector of values to initialize the combo box with. </param>
	/// <param name="editable"> whether the text in this object is editable.  SimpleJComboBoxes
	/// with editable text cannot support drag operations. </param>
	/// <param name="dragAction"> the action to take when dragging data.  
	/// See DragAndDropUtil.ACTION_*.  Drags can only be performed if the combo box is NON editable. </param>
	/// <param name="dropAction"> the action to take when dropping data.  
	/// See DragAndDropUtil.ACTION_*.  Drops can only be performed if the combo box is editable. </param>
	public DragAndDropSimpleJComboBox(System.Collections.IList v, bool editable, int dragAction, int dropAction) : base(v, editable)
	{
		initialize(dragAction, dropAction);
	}

	/// <summary>
	/// Creates a SimpleJComboBox that supports drag and drop. </summary>
	/// <param name="size"> the default width of the drop down combo box area. </param>
	/// <param name="editable"> whether the text in this object is editable.  SimpleJComboBoxes
	/// with editable text cannot support drag operations. </param>
	/// <param name="dragAction"> the action to take when dragging data.  
	/// See DragAndDropUtil.ACTION_*.  Drags can only be performed if the combo box
	/// is NON editable. </param>
	/// <param name="dropAction"> the action to take when dropping data.  
	/// See DragAndDropUtil.ACTION_*.  Drops can only be performed if the combo box
	/// is editable. </param>
	public DragAndDropSimpleJComboBox(int size, bool editable, int dragAction, int dropAction) : base(size, editable)
	{
		initialize(dragAction, dropAction);
	}

	/// <summary>
	/// Creates a SimpleJComboBox that supports drag and drop. </summary>
	/// <param name="v"> a Vector of values to initialize the combo box with. </param>
	/// <param name="size"> the default width of the drop down combo box area. </param>
	/// <param name="editable"> whether the text in this object is editable.  SimpleJComboBoxes
	/// with editable text cannot support drag operations. </param>
	/// <param name="dragAction"> the action to take when dragging data.  
	/// See DragAndDropUtil.ACTION_*.  Drags can only be performed if the combo box is NON editable. </param>
	/// <param name="dropAction"> the action to take when dropping data.  
	/// See DragAndDropUtil.ACTION_*.  Drops can only be performed if the combo box is editable. </param>
	public DragAndDropSimpleJComboBox(System.Collections.IList v, int size, bool editable, int dragAction, int dropAction) : base(v, size, editable)
	{
		initialize(dragAction, dropAction);
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~DragAndDropSimpleJComboBox()
	{
		__data = null;
		__lastSelectedItem = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the data flavors in which the combo box can transfer data. </summary>
	/// <returns> the data flavors in which the combo box can transfer data. </returns>
	public virtual DataFlavor[] getDataFlavors()
	{
		return DragAndDropTransferPrimitive.getTransferDataFlavors(DragAndDropTransferPrimitive.TYPE_STRING);
	}

	/// <summary>
	/// Returns the data structure with info about the combo box's DragAndDrop rules. </summary>
	/// <returns> the data structure with info about the combo box's DragAndDrop rules. </returns>
	public virtual DragAndDropControl getDragAndDropControl()
	{
		return __data;
	}

	/// <summary>
	/// Returns the transferable object that can be dragged from this combo box. </summary>
	/// <returns> the transferable object that can be dragged from this combo box. </returns>
	public virtual Transferable getTransferable()
	{
		if (__data.getAlternateTransferable() != null)
		{
			return __data.getAlternateTransferable();
		}
		else
		{
			return new DragAndDropTransferPrimitive(__lastSelectedItem);
		}
	}

	/// <summary>
	/// Handles data that has been dropped on this combo box. </summary>
	/// <param name="o"> the data that has been dropped. </param>
	public virtual bool handleDropData(object o, Point p)
	{
		if (o == null)
		{
			return false;
		}
		if (o is DragAndDropTransferPrimitive)
		{
			setText("" + ((string)((DragAndDropTransferPrimitive)o).getData()));
		}
		else
		{
			// this class is set up (see getDataFlavors()) to recognize
			// STRING type transfer data flavors as defined in the 
			// Primitive.  All Primitives support transferring their data
			// in its default format (Boolean, Integer, String, etc.) but
			// also support a final default 'text' flavor, which supports
			// transferring most other kinds of data from other
			// applications.  Nearly everything can at least transfer data
			// as text.  This catches that.
			setText(o.ToString());
		}
		return true;
	}

	/// <summary>
	/// Initializes the drag and drop aspects of this class. </summary>
	/// <param name="dragAction"> the action to take on dragging. </param>
	/// <param name="dropAction"> the action to take on dropping. </param>
	private void initialize(int dragAction, int dropAction)
	{
		// create the data object (for use by DragAndDropUtil)
		__data = new DragAndDropControl(dragAction, dropAction);

		Accessible a = getUI().getAccessibleChild(this, 0);
		JList list = null;
		if (a != null && a is ComboPopup)
		{
			// get the popup list
			list = ((ComboPopup)a).getList();
		}

		setDragAction(dragAction);

		// only allow dragging if the combo box is not editable
		if (__data.allowsDrag() && !isEditable() && list != null)
		{
			__data.setDragSource(DragAndDropUtil.createDragSource(list, dragAction, this));
		}
		// only allow drops if the combobox is editable
		if (__data.allowsDrop() && isEditable())
		{
			// note:
			// it was essential that the drop target be the editor 
			// for the combo box and not the combo box itself.  Otherwise,
			// there was some weird behavior.
			if (isEditable())
			{
				__data.setDropTarget(DragAndDropUtil.createDropTarget((JTextComponent)getEditor().getEditorComponent(), dropAction, this));
			}
		}
		else
		{
			// turn off the default drag and drop support in the text 
			// component.
			__data.setDropAction(DragAndDropUtil.ACTION_NONE);
			((JTextComponent)getEditor().getEditorComponent()).setTransferHandler(null);
		}

		DragAndDropDefaultListCellRenderer lcr = new DragAndDropDefaultListCellRenderer(this);
		setRenderer(lcr);
	}

	/// <summary>
	/// Sets whether dragging is enabled, and makes sure it is not enabled if the
	/// combo box is editable. </summary>
	/// <param name="action"> the action to take on dragging </param>
	public virtual void setDragAction(int action)
	{
		if (isEditable())
		{
			__data.setDragAction(DragAndDropUtil.ACTION_NONE);
		}
		else
		{
			__data.setDragAction(action);
		}
	}

	/// <summary>
	/// Sets the item that was last-selected from the list that appears when the 
	/// combo box is clicked on.  This is so that items in the middle of the list
	/// can be dragged. </summary>
	/// <param name="lastSelectedItem"> the text of the last-selected item. </param>
	protected internal virtual void setLastSelectedItem(string lastSelectedItem)
	{
		__lastSelectedItem = lastSelectedItem;
	}

	///////////////////////////////////////////////////////////////////////
	// DragAndDrop interface methods
	///////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Does nothing (DragAndDrop interface method).
	/// </summary>
	public virtual void dragStarted()
	{
	}

	/// <summary>
	/// Called when a drag has been performed successfully.  Checks to see if the 
	/// action performed was a move, and if so, removes the item that was dragged
	/// from the combo box. </summary>
	/// <param name="action"> the action performed by the drop component when the drop occurred. </param>
	public virtual void dragSuccessful(int action)
	{
		int result = DragAndDropUtil.determineAction(__data.getDragAction(), action);
		if (result == DragAndDropUtil.ACTION_MOVE && __selectedIndex > -1)
		{
			removeAt(__selectedIndex);
		}
		__selectedIndex = -1;
	}

	/// <summary>
	/// Does nothing (DragAndDrop interface method).
	/// </summary>
	public virtual void dragUnsuccessful(int action)
	{
	}

	/// <summary>
	/// Does nothing (DragAndDrop interface method).
	/// </summary>
	public virtual void dropExited()
	{
	}

	/// <summary>
	/// Does nothing (DragAndDrop interface method).
	/// </summary>
	public virtual void dropAllowed()
	{
	}

	/// <summary>
	/// Does nothing (DragAndDrop interface method).
	/// </summary>
	public virtual void dropNotAllowed()
	{
	}

	/// <summary>
	/// Does nothing (DragAndDrop interface method).
	/// </summary>
	public virtual void dropSuccessful()
	{
	}

	/// <summary>
	/// Does nothing (DragAndDrop interface method).
	/// </summary>
	public virtual void dropUnsuccessful()
	{
	}

	/// <summary>
	/// Sets the alternate transferable, in case data other than the label in the
	/// combo box should be transferred upon a drag. </summary>
	/// <param name="t"> the alternate Transferable to use. </param>
	public virtual void setAlternateTransferable(Transferable t)
	{
		__data.setAlternateTransferable(t);
	}


	////////////////////////////////////////////////////////////////
	// Drag Gesture events
	/// <summary>
	/// Calls DragAndDropUtil.dragStart (DragGesture event) and also keeps track of
	/// the item that was selected in case it has to be removed from the combo box
	/// upon a successful drag with a move action.
	/// </summary>
	public virtual void dragGestureRecognized(DragGestureEvent dge)
	{
		DragAndDropUtil.dragStart(this, this, dge);
		__selectedIndex = getSelectedIndex();
	}

	///////////////////////////////////////////////////////////////////////
	// Drag methods
	///////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Calls DragAndDropUtil.dragDropEnd (Drag event).
	/// </summary>
	public virtual void dragDropEnd(DragSourceDropEvent dsde)
	{
		DragAndDropUtil.dragDropEnd(this, dsde);
	}

	/// <summary>
	/// Calls DragAndDropUtil.dragEnter (Drag event).
	/// </summary>
	public virtual void dragEnter(DragSourceDragEvent dsde)
	{
		DragAndDropUtil.dragEnter(this, dsde);
	}

	/// <summary>
	/// Calls DragAndDropUtil.dragExit(Drag event).
	/// </summary>
	public virtual void dragExit(DragSourceEvent dse)
	{
		DragAndDropUtil.dragExit(this, dse);
	}

	/// <summary>
	/// Calls DragAndDropUtil.dragOver (Drag event).
	/// </summary>
	public virtual void dragOver(DragSourceDragEvent dsde)
	{
		DragAndDropUtil.dragOver(this, dsde);
	}

	/// <summary>
	/// Calls DragAndDropUtil.dropActionChanged (Drag event).
	/// </summary>
	public virtual void dropActionChanged(DragSourceDragEvent dsde)
	{
		DragAndDropUtil.dropActionChanged(this, dsde);
	}

	/// <summary>
	/// Calls DragAndDropUtil.dropActionChanged (Drop event).
	/// </summary>
	public virtual void dropActionChanged(DropTargetDragEvent dtde)
	{
		DragAndDropUtil.dropActionChanged(this, dtde);
	}

	///////////////////////////////////////////////////////////////////////
	// Drop methods
	///////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Calls DragAndDropUtil.dragEnter (Drop event).
	/// </summary>
	public virtual void dragEnter(DropTargetDragEvent dtde)
	{
		DragAndDropUtil.dragEnter(this, dtde);
	}

	/// <summary>
	/// Calls DragAndDropUtil.dragExit (Drop event).
	/// </summary>
	public virtual void dragExit(DropTargetEvent dte)
	{
		DragAndDropUtil.dragExit(this, dte);
	}

	/// <summary>
	/// Calls DragAndDropUtil.dragOver (Drop event).
	/// </summary>
	public virtual void dragOver(DropTargetDragEvent dtde)
	{
		DragAndDropUtil.dragOver(this, dtde);
	}

	/// <summary>
	/// Calls DragAndDropUtil.drop (Drop event).
	/// </summary>
	public virtual void drop(DropTargetDropEvent dtde)
	{
		DragAndDropUtil.drop(this, dtde);
	}

	}

}