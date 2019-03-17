// DragAndDropJWorksheet - JWorksheet from which data can be dragged and onto which data can be dropped

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
// DragAndDropJWorksheet - Class that implements a JWorksheet from which data 
//	can be dragged and onto which data can be dropped.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2004-03-01	J. Thomas Sapienza, RTi	Initial version.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.Util.GUI
{



	using IOUtil = RTi.Util.IO.IOUtil;
	using PropList = RTi.Util.IO.PropList;

	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// This class implements a JWorksheet that supports dragging data out of and 
	/// dropping data into.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class DragAndDropJWorksheet extends JWorksheet implements java.awt.dnd.DragGestureListener, java.awt.dnd.DragSourceListener, java.awt.dnd.DropTargetListener, DragAndDrop
	public class DragAndDropJWorksheet : JWorksheet, DragGestureListener, DragSourceListener, DropTargetListener, DragAndDrop
	{

	/// <summary>
	/// The DragAndDropControl object that holds information about how this object 
	/// works with DragAndDrop.
	/// </summary>
	private DragAndDropControl __data = null;

	/// <summary>
	/// Constructor.  Creates a JWorksheet with the specified cell renderer and table 
	/// model.  For information about the support properties that can be passed in 
	/// in the proplist, see the JWorksheet javadocs. </summary>
	/// <param name="cellRenderer"> the cell renderer to use. </param>
	/// <param name="tableModel"> the table model to use. </param>
	/// <param name="props"> the properties that define JWorksheet behavior. </param>
	public DragAndDropJWorksheet(JWorksheet_DefaultTableCellRenderer cellRenderer, JWorksheet_AbstractTableModel tableModel, PropList props) : base(cellRenderer, tableModel, props)
	{
		initialize(DragAndDropUtil.ACTION_COPY, DragAndDropUtil.ACTION_NONE);
	}

	/// <summary>
	/// Constructor.  Creates a JWorksheet with the specified number of rows and 
	/// columns. For information about the support properties that can be passed in 
	/// in the proplist, see the JWorksheet javadocs. </summary>
	/// <param name="rows"> the number of rows in the empty worksheet. </param>
	/// <param name="cols"> the number of columns in the empty worksheet. </param>
	/// <param name="props"> the properties that define JWorksheet behavior. </param>
	public DragAndDropJWorksheet(int rows, int cols, PropList props) : base(rows, cols, props)
	{
		initialize(DragAndDropUtil.ACTION_COPY, DragAndDropUtil.ACTION_NONE);
	}

	/// <summary>
	/// Returns the data flavors in which the worksheet can transfer data. </summary>
	/// <returns> the data flavors in which the worksheet can transfer data. </returns>
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
	~DragAndDropJWorksheet()
	{
		__data = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the data structure with info about the worksheet's DragAndDrop rules. </summary>
	/// <returns> the data structure with info about the worksheet's DragAndDrop rules. </returns>
	public virtual DragAndDropControl getDragAndDropControl()
	{
		return __data;
	}

	/// <summary>
	/// Returns the transferable object that can be dragged from this worksheet. </summary>
	/// <returns> the transferable object that can be dragged from this worksheet. </returns>
	public virtual Transferable getTransferable()
	{
		if (IOUtil.testing())
		{
			Message.printStatus(1, "", "getTransferable: " + __data.getAlternateTransferable());
		}
		if (__data.getAlternateTransferable() != null)
		{
			return __data.getAlternateTransferable();
		}
		else
		{
			return new DragAndDropTransferPrimitive("No alternate transferable set");
		}
	}

	/// <summary>
	/// Handles data that has been dropped on this worksheet. </summary>
	/// <param name="o"> the data that has been dropped. </param>
	public virtual bool handleDropData(object o, Point p)
	{
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

		// only allow dragging if the worksheet is not editable
		if (__data.allowsDrag())
		{
			__data.setDragSource(DragAndDropUtil.createDragSource(this, dragAction, this));
		}
		if (__data.allowsDrop())
		{
			__data.setDropTarget(DragAndDropUtil.createDropTarget(this, dropAction, this));
		}
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
		int col = columnAtPoint(p);
		int row = rowAtPoint(p);

		if (isCellSelected(row, col))
		{
			if (((JWorksheet_RowSelectionModel)getSelectionModel()).dragWasTriggered())
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

	}

}