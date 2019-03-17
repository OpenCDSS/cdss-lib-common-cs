// DragAndDropJLabel - JLabel from which data can be dragged and onto which data can be dropped.

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
// DragAndDropJLabel - Class that implements a JLabel from which data can be
// 	dragged and onto which data can be dropped.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2004-03-01	J. Thomas Sapienza, RTi	Initial version.
// 2004-04-27	JTS, RTi		Revised after SAM's review.
// ----------------------------------------------------------------------------

namespace RTi.Util.GUI
{



	/// <summary>
	/// This class implements a JLabel that supports dragging String data from and 
	/// dropping String data onto.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class DragAndDropJLabel extends javax.swing.JLabel implements java.awt.dnd.DragGestureListener, java.awt.dnd.DragSourceListener, java.awt.dnd.DropTargetListener, DragAndDrop
	public class DragAndDropJLabel : JLabel, DragGestureListener, DragSourceListener, DropTargetListener, DragAndDrop
	{

	/// <summary>
	/// Reference to the DragAndDropControl that controls how drag and drop is performed
	/// on this object.
	/// </summary>
	private DragAndDropControl __data = null;

	/// <summary>
	/// Constructor.  Creates a JLabel with the specified dragging and dropping actions. </summary>
	/// <param name="text"> the text to appear on the JLabel. </param>
	/// <param name="dragAction"> the action to take upon dragging.  If 
	/// DragAndDropUtil.ACTION_NONE, dragging is not supported. </param>
	/// <param name="dropAction"> the action to take upon dropping.  If
	/// DragAndDropUtil.ACTION_NONE, dropping is not supported. </param>
	public DragAndDropJLabel(string text, int dragAction, int dropAction) : base(text)
	{
		initialize(dragAction, dropAction);
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~DragAndDropJLabel()
	{
		__data = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Initializes the DragAndDropControl according to the specified actions. </summary>
	/// <param name="dragAction"> the action to take upon dragging.  If 
	/// DragAndDropUtil.ACTION_NONE, dragging is not supported. </param>
	/// <param name="dropAction"> the action to take upon dropping.  If
	/// DragAndDropUtil.ACTION_NONE, dropping is not supported. </param>
	private void initialize(int dragAction, int dropAction)
	{
		__data = new DragAndDropControl(dragAction, dropAction);
		if (__data.allowsDrag())
		{
			__data.setDragSource(DragAndDropUtil.createDragSource(this, dragAction, this));
		}
		if (__data.allowsDrop())
		{
			__data.setDropTarget(DragAndDropUtil.createDropTarget(this, dropAction, this));
		}
	}

	/// <summary>
	/// Returns the data flavors that this object can use to transfer data. </summary>
	/// <returns> the data flavors that this object can use to transfer data. </returns>
	public virtual DataFlavor[] getDataFlavors()
	{
		return DragAndDropTransferPrimitive.getTransferDataFlavors(DragAndDropTransferPrimitive.TYPE_STRING);
	}

	/// <summary>
	/// Returns the data that controls how drag and drop are performed. </summary>
	/// <returns> the data that controls how drag and drop are performed. </returns>
	public virtual DragAndDropControl getDragAndDropControl()
	{
		return __data;
	}

	/// <summary>
	/// Handles data that were dropped on this object.  If the data were of a 
	/// supported  type, the text of the label is changed to the dropped text. </summary>
	/// <param name="o"> the data dropped on the label. </param>
	/// <param name="p"> the Point at which data was dropped. </param>
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
	/// Returns the transferable object that can be moved around in a drag operation. </summary>
	/// <returns> the transferable object that can be moved around in a drag operation. </returns>
	public virtual Transferable getTransferable()
	{
		return new DragAndDropTransferPrimitive(getText());
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
	/// Does nothing (DragAndDrop interface method).
	/// </summary>
	public virtual void setAlternateTransferable(Transferable t)
	{
	}

	///////////////////////////////////////////////////////////////////////
	// DragGesture method
	///////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Calls DragAndDropUtil.dragStart (DragGesture event).
	/// </summary>
	public virtual void dragGestureRecognized(DragGestureEvent dge)
	{
		DragAndDropUtil.dragStart(this, this, dge);
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