// DragAndDropSimpleJList - class that supports drag and drop operations on a JList

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
// DragAndDropSimpleJList - class that supports drag and drop operations 
// 	on a JList.
//---------------------------------------------------------------------------
// Copyright:  See the COPYRIGHT file.
//---------------------------------------------------------------------------
// History:
//
// 2003-05-06	J. Thomas Sapienza, RTi	Initial version.
// 2005-04-08	JTS, RTi		Renamed from DragAndDropMutableJList.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//---------------------------------------------------------------------------

namespace RTi.Util.GUI
{




	public class DragAndDropSimpleJList : SimpleJList, DragGestureListener, DragSourceListener, DropTargetListener, DragAndDrop, ListSelectionListener
	{

	/// <summary>
	/// The DragAndDropControl object that holds information about how this object 
	/// works with DragAndDrop.
	/// </summary>
	private DragAndDropControl __data = null;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dragAction"> the action to be taken when something is dragged from the list </param>
	/// <param name="dropAction"> the action to be taken when something is dropped on the list </param>
	public DragAndDropSimpleJList(int dragAction, int dropAction) : base()
	{
		initialize(dragAction, dropAction);
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="array"> an array of objects that will be used to populate the list. </param>
	/// <param name="dragAction"> the action to be taken when something is dragged from the list </param>
	/// <param name="dropAction"> the action to be taken when something is dropped on the list </param>
	public DragAndDropSimpleJList(object[] array, int dragAction, int dropAction) : base(array)
	{
		initialize(dragAction, dropAction);
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="vector"> a list of objects that will be used to populate the list. </param>
	/// <param name="dragAction"> the action to be taken when something is dragged from the list </param>
	/// <param name="dropAction"> the action to be taken when something is dropped on the list </param>
	public DragAndDropSimpleJList(System.Collections.IList vector, int dragAction, int dropAction) : base(vector)
	{
		initialize(dragAction, dropAction);
	}

	/// <summary>
	/// Returns the data flavors in which the combo box can transfer data. </summary>
	/// <returns> the data flavors in which the combo box can transfer data. </returns>
	public virtual DataFlavor[] getDataFlavors()
	{
		if (__data.getAlternateTransferable() != null)
		{
			return __data.getAlternateTransferable().getTransferDataFlavors();
		}
		else
		{
			return DragAndDropTransferPrimitive.getTransferDataFlavors(DragAndDropTransferPrimitive.TYPE_STRING);
		}
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~DragAndDropSimpleJList()
	{
		__data = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
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
			return new DragAndDropTransferPrimitive((string)getSelectedItem());
		}
	}

	/// <summary>
	/// Handles data that has been dropped on this combo box. </summary>
	/// <param name="o"> the data that has been dropped. </param>
	public virtual bool handleDropData(object o, Point p)
	{
		// REVISIT (JTS - 2004-05-06)
		// come back and actually add support for dropping things on the list,
		// once we start doing that.  No time right now.
		if (o == null)
		{
			return false;
		}
		if (o is DragAndDropTransferPrimitive)
		{

		}
		else
		{
			// should be String
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

		// only allow dragging if the combo box is not editable
		if (__data.allowsDrag())
		{
			__data.setDragSource(DragAndDropUtil.createDragSource(this, dragAction, this));
		}
		if (__data.allowsDrop())
		{
			__data.setDropTarget(DragAndDropUtil.createDropTarget(this, dropAction, this));
		}
		setSelectionModel(new SimpleJList_SelectionModel(getItemCount()));
		((SimpleJList_SelectionModel)getSelectionModel()).setSupportsDragAndDrop(true);
		getSelectionModel().addListSelectionListener(this);
		addMouseListener((SimpleJList_SelectionModel)getSelectionModel());
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
	/// Does nothing (DragAndDrop interface method).
	/// </summary>
	public virtual void dragSuccessful(int action)
	{
		int result = DragAndDropUtil.determineAction(__data.getDragAction(), action);
		if (result == DragAndDropUtil.ACTION_MOVE)
		{
			remove(getSelectedIndex());
		}
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
	/// Sets the alternate transferable to use in the event of a drag. </summary>
	/// <param name="t"> the alternate transferable to use. </param>
	public virtual void setAlternateTransferable(Transferable t)
	{
		__data.setAlternateTransferable(t);
	}

	/// <summary>
	/// Recognizes a drag only if a click is made on a cell that is already selected
	/// and a drag is started.  Otherwise, allow normal worksheet cell-selection via
	/// dragging.
	/// </summary>
	public virtual void dragGestureRecognized(DragGestureEvent dge)
	{
		Point p = dge.getDragOrigin();
		int row = locationToIndex(p);

		if (isSelectedIndex(row))
		{
			if (((SimpleJList_SelectionModel)getSelectionModel()).dragWasTriggered())
			{
				DragAndDropUtil.dragStart(this, this, dge);
			}
		}
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

	/// <summary>
	/// Repaints the list in response to list changes. From ListSelectionListener. </summary>
	/// <param name="event"> the ListSelectionEvent that happened. </param>
	public override void valueChanged(ListSelectionEvent @event)
	{
		repaint();
	}

	}

}