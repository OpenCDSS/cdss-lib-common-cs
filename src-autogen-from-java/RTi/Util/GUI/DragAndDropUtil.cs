using System;

// DragAndDropUtil - utility methods for use by classes that implement drag and drop capability

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
// DragAndDropUtil - Utility methods for use by classes that implement 
//	drag and drop capability.
//-----------------------------------------------------------------------------
// Copyright: See the COPYRIGHT file.
//-----------------------------------------------------------------------------
// History: 
// 2004-02-24	J. Thomas Sapienza, RTi	Initial version.
// 2004-03-04	JTS, RTi		Updated Javadocs in response to 
//					numerous changes.
// 2004-04-27	JTS, RTi		Revised after SAM's review.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//-----------------------------------------------------------------------------

namespace RTi.Util.GUI
{



	using IOUtil = RTi.Util.IO.IOUtil;

	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// Utility methods for use by classes that implement drag and drop capability.
	/// For information how to implement drag and drop in a Swing JComponent, see the
	/// documentation for the DragAndDrop interface.<para>
	/// <b>Note:</b><br>
	/// Because this class handles utility functions for both drag and drop listeners,
	/// there can be some confusion in the method names.  Java's API uses the same
	/// method names for both drag and drop listeners; only the parameters are named
	/// differently.  For this reason, in the methods that might be confusing the 
	/// Javadocs for each method are prefaced with the command that is occuring.  
	/// For instance:<br>
	/// <blockquote>
	/// Drop: called when ....
	/// </para>
	/// </blockquote><para>
	/// refers to a method that is used by drop listeners.
	/// </para>
	/// </summary>
	public class DragAndDropUtil
	{

	/// <summary>
	/// Class name.
	/// </summary>
	private const string __CLASS = "DragAndDropUtil";

	////////////////////////////////////////
	// These actions are chained from the actions in DnDConstants
	// because DnDConstants provides some other actions (in particular, for 
	// moving, linking and copying files) that JTS felt were unnecessary at
	// the current time for RTi developers.  Putting these links to the other
	// commands limits RTi developers to only using the following commands.

	/// <summary>
	/// Used for DragAndDrop components that don't respond to any DragAndDrop 
	/// actions.  For instance, if a component allows dragging but not dropping, 
	/// the drop action would be ACTION_NONE.  
	/// <para>Numeric value: 0.
	/// </para>
	/// </summary>
	public static readonly int ACTION_NONE = DnDConstants.ACTION_NONE;

	/// <summary>
	/// Used for DragAndDrop components that allow a copy of information.  
	/// Information that is copied should remain in the drag component and be 
	/// duplicated into the drop component.  
	/// <para>Numeric value: 1.
	/// </para>
	/// </summary>
	public static readonly int ACTION_COPY = DnDConstants.ACTION_COPY;

	/// <summary>
	/// Used for DragAndDrop components that move information from one to another.  
	/// Information that is moved will be copied into the drop component and 
	/// deleted from the drag component. 
	/// <para>Numeric value: 2.
	/// </para>
	/// </summary>
	public static readonly int ACTION_MOVE = DnDConstants.ACTION_MOVE;

	/// <summary>
	/// Used for DragAndDrop components that respond to either copy or move 
	/// actions.  This is mostly for use when working with outside drag sources, 
	/// such as from a Windows application, because it can't be certain whether 
	/// the information was sent with
	/// a COPY or a MOVE action.  Even then, they might not be properly used.<para>
	/// 
	/// For example, if text is dragged from Wordpad into a java application, the
	/// Wordpad drag sends a MOVE action.  The text that was dragged from Wordpad
	/// </para>
	/// remainds in Wordpad, however, so it is actually a COPY action.<para>
	/// 
	/// ACTION_COPY_OR_MOVE is a logical OR of the ACTION_COPY and ACTION_COPY 
	/// actions, provided for convenience.
	/// </para>
	/// <para>Numerical value: 3.
	/// </para>
	/// </summary>
	public static readonly int ACTION_COPY_OR_MOVE = DnDConstants.ACTION_COPY_OR_MOVE;

	/// <summary>
	/// Registers a DragAndDropListener for an DragAndDrop object. </summary>
	/// <param name="r"> the DragAndDrop object on which to register the listener. This can
	/// be either a component from which can are dragged or a component on which
	/// data are dropped. </param>
	/// <param name="d"> the listener to register. This is the component that will be 
	/// informed of events occuring throughout dragging and dropping. </param>
	public static void addDragAndDropListener(DragAndDrop r, DragAndDropListener d)
	{
		r.getDragAndDropControl().addDragAndDropListener(d);
	}

	/// <summary>
	/// Creates a drag source for a component that can be dragged.  The drag source
	/// handles recognizing and start drag events on a drop component.  The 
	/// DragSource object will be stored in the component's DragAndDropData object.  
	/// Most of the time, the parameters 'c' and 'dgl' will be the same object. </summary>
	/// <param name="c"> the Component from which data can be dragged.  It must implement 
	/// DragAndDrop. </param>
	/// <param name="action"> the action performed by the drag.  See DragAndDropUtil.ACTION_*. </param>
	/// <param name="dgl"> the DragGestureListener that will recognize when a drag begins.  
	/// Must implement DragAndDrop. </param>
	/// <returns> a DragSource object for storage in a DragAndDropData object. </returns>
	public static DragSource createDragSource(Component c, int action, DragGestureListener dgl)
	{
		DragSource dragSource = DragSource.getDefaultDragSource();
		dragSource.createDefaultDragGestureRecognizer(c,action, dgl);
		return dragSource;
	}

	/// <summary>
	/// Creates a drop target for a component onto which data can be dragged.  The
	/// drop target handles recognizing when a drop has occured on a drop compoenent, 
	/// and is stored in the component's DragAndDropData object.
	/// Most of the time, the parameters 'c' and 'dtl' will be the same object. </summary>
	/// <param name="c"> the Component onto which data can be dragged.  It must implement 
	/// DragAndDrop. </param>
	/// <param name="action"> the action that the drop recognizes.  See 
	/// DragAndDropUtil.ACTION_*. </param>
	/// <param name="dtl"> the DropTargetListener that recognizes when a drop occurs.  
	/// Must implement DragAndDrop. </param>
	/// <returns> a DropTarget object for storage in a DragAndDropData object. </returns>
	public static DropTarget createDropTarget(Component c, int action, DropTargetListener dtl)
	{
		DropTarget dropTarget = new DropTarget(c, action, dtl, true);
		return dropTarget;
	}

	/// <summary>
	/// Determines the action that should be taken given a combination of actions
	/// from a drag and a drop component.  When setting up a draggable or a droppable
	/// component, an action is specified that tells Java's drag and drop how to 
	/// behave.  This method takes an action from the component from which data was
	/// dragged and the action from the component on which data is to be dropped and
	/// sees if the actions can work together and perform a task. <para>
	/// If either action is ACTION_NONE, then ACTION_NONE will be returned and no
	/// </para>
	/// drag and drop action can be performed.<para>
	/// If one action is ACTION_COPY, then ACTION_COPY is returned if the other
	/// action is ACTION_COPY or ACTION_COPY_OR_MOVE.  Otherwise, ACTION_NONE will
	/// </para>
	/// be returned.<para>
	/// If one action is ACTION_MOVE, then ACTION_MOVE is returned if the other
	/// action is ACTION_MOVE or ACTION_COPY_OR_MOVE.  Otherwise, ACTION_NONE will
	/// </para>
	/// be returned.<para>
	/// If one action is ACTION_COPY_OR_MOVE, then ACTION_COPY will be returned if
	/// the other action is ACTION_COPY.  ACTION_MOVE will be returned if the other
	/// action is ACTION_MOVE. ACTION_COPY will be returned if the other action is
	/// ACTION_COPY_OR_MOVE.
	/// </para>
	/// </summary>
	/// <param name="dragAction"> the action performed by the drag component. </param>
	/// <param name="dropAction"> the action recognized by the drop component. </param>
	/// <returns> the action that should be performed, given the actions of both 
	/// components. See DragAndDrop's documentation for "Responding to a 
	/// Successful Drag" for an explanation. </returns>
	public static int determineAction(int dragAction, int dropAction)
	{
		if (dragAction == ACTION_NONE || dropAction == ACTION_NONE)
		{
			return ACTION_NONE;
		}

		if (dragAction == ACTION_COPY)
		{
			if (dropAction == ACTION_COPY)
			{
				return ACTION_COPY;
			}
			else if (dropAction == ACTION_MOVE)
			{
				return ACTION_NONE;
			}
			else if (dropAction == ACTION_COPY_OR_MOVE)
			{
				return ACTION_COPY;
			}
		}
		else if (dragAction == ACTION_MOVE)
		{
			if (dropAction == ACTION_COPY)
			{
				return ACTION_NONE;
			}
			else if (dropAction == ACTION_MOVE)
			{
				return ACTION_MOVE;
			}
			else if (dropAction == ACTION_COPY_OR_MOVE)
			{
				return ACTION_MOVE;
			}
		}
		else if (dragAction == ACTION_COPY_OR_MOVE)
		{
			if (dropAction == ACTION_COPY)
			{
				return ACTION_COPY;
			}
			else if (dropAction == ACTION_MOVE)
			{
				return ACTION_MOVE;
			}
			else if (dropAction == ACTION_COPY_OR_MOVE)
			{
				return ACTION_COPY;
			}
		}
		return ACTION_NONE;
	}

	/// <summary>
	/// Drag: called when a drag has been terminated because the mouse button was 
	/// released.  It determines whether or not the drag was successful and calls
	/// dragUnsuccessful() or dragSuccessful() on the DragAndDrop parameter as 
	/// necessary.  In addition, all the DragAndDropListeners for the drag object 
	/// are notified via dragSuccessful() or dragUnsuccessful(). </summary>
	/// <param name="d"> the DragAndDrop object that instantiated the drag. </param>
	/// <param name="dsde"> the DragSourceDropEvent created when the drag ended. </param>
	public static void dragDropEnd(DragAndDrop d, DragSourceDropEvent dsde)
	{
		if ((!d.getDragAndDropControl().allowsDrag()) || (dsde.getDropSuccess() == false))
		{
			d.dragUnsuccessful(dsde.getDropAction());
			d.getDragAndDropControl().notifyListenersDragUnsuccessful(dsde.getDropAction());
		}
		else
		{
			d.dragSuccessful(dsde.getDropAction());
			d.getDragAndDropControl().notifyListenersDragSuccessful(dsde.getDropAction());
		}
	}

	/// <summary>
	/// Drag: called when a drag goes into a component.
	/// Sets the mouse cursor over the component to represent whether anything can
	/// be dropped in that component. </summary>
	/// <param name="d"> the DragAndDrop object from which data was dragged. </param>
	/// <param name="dsde"> the DragSourceDragEvent created when the drag entered the 
	/// droppable component. </param>
	public static void dragEnter(DragAndDrop d, DragSourceDragEvent dsde)
	{
		if (!d.getDragAndDropControl().allowsDrag())
		{
			DragSourceContext context = dsde.getDragSourceContext();
			context.setCursor(DragSource.DefaultCopyNoDrop);
		}
		else
		{
			setDragOverFeedback(d, dsde);
		}
	}

	/// <summary>
	/// Drop: called when a drag goes into a component that can be dropped on.
	/// Checks to see if the component allows drags and drops and calls dropNotAllowed()
	/// or dropAllowed() on the 'd' parameter as necessary.  In addition, the
	/// drop component's DragAndDropListeners are notified via dropAllowed()
	/// or dropNotAllowed(). </summary>
	/// <param name="d"> the DragAndDrop object that data is being dragged onto. </param>
	/// <param name="dtde"> the DropTargetDragEvent created when the mouse entered the 
	/// component. </param>
	public static void dragEnter(DragAndDrop d, DropTargetDragEvent dtde)
	{
		if ((!d.getDragAndDropControl().allowsDrop()) || (!DragAndDropUtil.isDragOK(d, dtde)))
		{
			d.dropNotAllowed();
			d.getDragAndDropControl().notifyListenersDropNotAllowed();
			dtde.rejectDrag();
		}
		else
		{
			d.dropAllowed();
			d.getDragAndDropControl().notifyListenersDropAllowed();
			dtde.acceptDrag(dtde.getDropAction());
		}
	}

	/// <summary>
	/// Drag: called when a drag exits a component that can be dropped on.  Sets 
	/// the cursor to indicate that no drop is allowed. </summary>
	/// <param name="d"> the DragAndDrop object from which data was dragged. </param>
	/// <param name="dse"> the DragSourceEvent created when the component was exited. </param>
	public static void dragExit(DragAndDrop d, DragSourceEvent dse)
	{
		if (!d.getDragAndDropControl().allowsDrag())
		{
			DragSourceContext context = dse.getDragSourceContext();
			context.setCursor(DragSource.DefaultCopyNoDrop);
			return;
		}

		DragSourceContext context = dse.getDragSourceContext();
		int dragAction = d.getDragAndDropControl().getDragAction();

		if (dragAction == ACTION_COPY || dragAction == ACTION_COPY_OR_MOVE)
		{
			context.setCursor(DragSource.DefaultCopyNoDrop);
		}
		else if (dragAction == ACTION_MOVE)
		{
			context.setCursor(DragSource.DefaultMoveNoDrop);
		}
		else
		{
			context.setCursor(DragSource.DefaultCopyNoDrop);
		}
		context.setCursor(null);
	}

	/// <summary>
	/// Drop: called when a drag exits a component that can be dropped on.  Calls
	/// dropExited() on the 'd' parameter, if the 'd' object allows drops.  In 
	/// addition, the drop component's DragAndDropListeners are notified via
	/// dropExited(). </summary>
	/// <param name="d"> the DragAndDrop object that was exited. </param>
	/// <param name="dtde"> the DropTargetEvent created when the component was exited. </param>
	public static void dragExit(DragAndDrop d, DropTargetEvent dtde)
	{
		if (!d.getDragAndDropControl().allowsDrop())
		{
			return;
		}
		d.dropExited();
		d.getDragAndDropControl().notifyListenersDropExited();
	}

	/// <summary>
	/// Drag: called when a drag is over a component that can be dragged on. </summary>
	/// <param name="d"> the DragAndDrop object from which data was dragged. </param>
	/// <param name="dsde"> the DragSourceDragEvent created when the component was dragged over. </param>
	public static void dragOver(DragAndDrop d, DragSourceDragEvent dsde)
	{
		if (!d.getDragAndDropControl().allowsDrag())
		{
			return;
		}
	//	This next line isn't probably necessary, so it is commented out.
	//	It is left in here as an example of what can be done in here.
	//	setDragOverFeedback(d, dsde);
	// 	REVISIT (JTS - 2004-02-26)
	// 	should this method provide any feedback to the 'd' parameter?
	}

	/// <summary>
	/// Drop: called when a drag is over a component that can be dropped on.  Calls
	/// d.dropNotAllowed() if the component doesn't allow drops or if the drop is 
	/// invalid.  Calls d.dropAllowed() if the drop is permitted.  In addition, 
	/// the drop component's DragAndDropListeners are notified via the appropriate
	/// dropAllowed() or dropNotAllowed() call. </summary>
	/// <param name="d"> the DragAndDrop object that is being dragged over. </param>
	/// <param name="dtde"> the DropTargetDragEvent created when the component is dragged over. </param>
	public static void dragOver(DragAndDrop d, DropTargetDragEvent dtde)
	{
		if ((!d.getDragAndDropControl().allowsDrop()) || (!DragAndDropUtil.isDragOK(d, dtde)))
		{
			d.dropNotAllowed();
			d.getDragAndDropControl().notifyListenersDropNotAllowed();
			dtde.rejectDrag();
		}
		else
		{
			d.dropAllowed();
			d.getDragAndDropControl().notifyListenersDropAllowed();
			dtde.acceptDrag(dtde.getDropAction());
		}
	}

	/// <summary>
	/// DragGestureEvent: called when a drag should be started.  Gets the data to be
	/// dragged and starts the drag operation.  The drag component's 
	/// DragAndDropListeners are notified via dragAboutToStart() prior to the 
	/// Transferable data being copied into the drag event.  Once the data have been
	/// copied into the drag and are under the mouse cursor, d.dragStart() is called
	/// and the drag component's DragAndDropListeners are notified via dragStart(). </summary>
	/// <param name="d"> the DragAndDrop from which the drag is occuring. </param>
	/// <param name="dsl"> the DragSourceListener that recognized the drag. </param>
	/// <param name="dge"> the DragGestureEvent that was recognized. </param>
	public static void dragStart(DragAndDrop d, DragSourceListener dsl, DragGestureEvent dge)
	{
		if (!d.getDragAndDropControl().allowsDrag())
		{
			return;
		}

		if ((dge.getDragAction() & d.getDragAndDropControl().getDragAction()) == 0)
		{
			return;
		}

		// give the listeners a chance to veto the drag ...
		if (!d.getDragAndDropControl().notifyListenersDragAboutToStart())
		{
			return;
		}

		Transferable transferable = d.getTransferable();

		try
		{
			// REVISIT (JTS - 2004-02-24)
			// if you want to drag images, this code won't work
			dge.startDrag(DragSource.DefaultCopyNoDrop, transferable, dsl);
			d.dragStarted();
			d.getDragAndDropControl().notifyListenersDragStarted();
		}
		catch (InvalidDnDOperationException idoe)
		{
			Message.printWarning(2, __CLASS + ".dragStart", "Invalid DragAndDrop operation.");
			Message.printWarning(2, __CLASS + ".dragStart", idoe);
			Console.WriteLine(idoe.ToString());
			Console.Write(idoe.StackTrace);
		}
	}

	/// <summary>
	/// Drop: called when data is dropped onto an DragAndDrop component.  If for
	/// any reason the drop is invalid, d.dropUnsuccessful() is called and 
	/// the drop component's DragAndDropListeners are notified via dropUnsuccessful().  
	/// If the drop is valid, d.handleDropData(...) is called.  
	/// If the data were handled properly, d.dropSuccessful() is called and the drop 
	/// component's DragAndDropListeners are notified via dropSuccessful(). 
	/// Otherwise d.dropUnsuccessful() is called. </summary>
	/// <param name="d"> the DragAndDrop onto which the drop is occurring. </param>
	/// <param name="dtde"> the DropTargetDropEvent created when the drop occurred. </param>
	public static void drop(DragAndDrop d, DropTargetDropEvent dtde)
	{
		string routine = __CLASS + ".getDropObject";
		if (!d.getDragAndDropControl().allowsDrop())
		{
			return;
		}

		DataFlavor[] flavors = d.getDataFlavors();
		int pos = findRequestedDragFlavor(d, dtde);
		if (pos == -1)
		{
			Message.printWarning(2, routine, "No matching data flavor " + "found.");
			dtde.rejectDrop();
			d.dropUnsuccessful();
			d.getDragAndDropControl().notifyListenersDropUnsuccessful();
			return;
		}
		DataFlavor chosen = flavors[pos];
		if (IOUtil.testing())
		{
			Message.printStatus(2, routine, "Using data flavor '" + chosen.getMimeType());
		}

		int sourceActions = dtde.getSourceActions();
		if ((sourceActions & d.getDragAndDropControl().getDropAction()) == 0)
		{
			Message.printWarning(2, routine, "No action match found for " + sourceActions);
			dtde.rejectDrop();
			d.dropUnsuccessful();
			d.getDragAndDropControl().notifyListenersDropUnsuccessful();
			return;
		}

		object data = null;
		try
		{
			dtde.acceptDrop(d.getDragAndDropControl().getDropAction());
			data = dtde.getTransferable().getTransferData(chosen);
		}
		catch (Exception t)
		{
			Message.printWarning(2, routine, "Couldn't get transfer data: " + t.Message);
		//	t.printStackTrace();
			Message.printWarning(2, routine, t);
			dtde.dropComplete(false);
			d.dropUnsuccessful();
			d.getDragAndDropControl().notifyListenersDropUnsuccessful();
			return;
		}

		bool result = false;
		try
		{
			result = d.handleDropData(data, dtde.getLocation());
		}
		catch (Exception e)
		{
			Console.WriteLine(e.ToString());
			Console.Write(e.StackTrace);
			result = false;
		}

		if (!result)
		{
			dtde.dropComplete(false);
			d.dropUnsuccessful();
			d.getDragAndDropControl().notifyListenersDropUnsuccessful();
		}
		else
		{
			dtde.dropComplete(true);
			d.dropSuccessful();
			d.getDragAndDropControl().notifyListenersDropSuccessful();
		}
	}

	/// <summary>
	/// Drag: called when the action for a drop has been changed by the user pressing
	/// control, shift, or control-shift.  Sets the cursor appropriately to represent
	/// the new action. </summary>
	/// <param name="d"> the DragAndDrop object from which data was dragged. </param>
	/// <param name="dsde"> the DragSourceDragEvent created when the drop action changed. </param>
	public static void dropActionChanged(DragAndDrop d, DragSourceDragEvent dsde)
	{
		if (!d.getDragAndDropControl().allowsDrag())
		{
			return;
		}
		setDragOverFeedback(d, dsde);
	}

	/// <summary>
	/// Drop: called when the action for a drop has been changed by the user pressing
	/// control, shift, or control-shift. </summary>
	/// <param name="d"> the DragAndDrop object on which data is being dragged. </param>
	/// <param name="dtde"> the DropTargetDragEvent created when the drop action changed. </param>
	public static void dropActionChanged(DragAndDrop d, DropTargetDragEvent dtde)
	{
		if ((!d.getDragAndDropControl().allowsDrop()) || (!DragAndDropUtil.isDragOK(d, dtde)))
		{
			dtde.rejectDrag();
		}
		else
		{
			dtde.acceptDrag(dtde.getDropAction());
		}
	}

	/// <summary>
	/// Finds the numeric position of the first of the DragAndDrop's DragFlavors 
	/// that the DropTargetDropEvent supports, in the DragAndDrop's DataFlavor 
	/// array.  In the low-level java code, checks to see if a List contains() an 
	/// instance of one of the drag flavors. </summary>
	/// <param name="d"> the DragAndDrop into which data will be dropped. </param>
	/// <param name="dtde"> the DropTargetDropEvent of the data that is trying to be dropped. </param>
	/// <returns> the numeric position of the first supported DataFlavor, or -1 if 
	/// none of the DataFlavors match or are supported. </returns>
	private static int findRequestedDragFlavor(DragAndDrop d, DropTargetDropEvent dtde)
	{
		DataFlavor[] flavors = d.getDataFlavors();
		for (int i = 0; i < flavors.Length; i++)
		{
			if (dtde.isDataFlavorSupported(flavors[i]))
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// Checks to see if the drag event supports the data flavors in the 
	/// DragAndDrop object. </summary>
	/// <param name="d"> the DragAndDrop object into which data is being dragged. </param>
	/// <param name="dtde"> the DropTargetDragEvent with information about the drag. </param>
	/// <returns> true if the drag events supports the DragAndDrop's data flavors.  
	/// False if not. </returns>
	public static bool isDragFlavorSupported(DragAndDrop d, DropTargetDragEvent dtde)
	{
		DataFlavor[] flavors = d.getDataFlavors();
		for (int i = 0; i < flavors.Length; i++)
		{
			if (dtde.isDataFlavorSupported(flavors[i]))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Checks to see if a drag action is OK or not.  The drag action is OK if
	/// the DataFlavor of the drag is supported and the drop action of the 
	/// drag is supported, too. </summary>
	/// <param name="d"> the DragAndDrop object into which something is being dragged. </param>
	/// <param name="dtde"> the DropTargetDragEvent of the attempted drag. </param>
	/// <returns> true if the drag is OK, false if not. </returns>
	private static bool isDragOK(DragAndDrop d, DropTargetDragEvent dtde)
	{
		string routine = __CLASS + ".isDragOK";

		if (!isDragFlavorSupported(d, dtde))
		{
			if (IOUtil.testing())
			{
				Message.printWarning(2, routine, "Drag Flavor is not " + "supported.");
			}
			return false;
		}

		int da = dtde.getDropAction();
		if ((da & d.getDragAndDropControl().getDropAction()) == 0)
		{
			if (IOUtil.testing())
			{
				Message.printWarning(2, routine, "No acceptable matching drag " + "action compared to: " + da);
				Message.printWarning(2, routine, "   drop action is: " + d.getDragAndDropControl().getDropAction());
			}
			return false;
		}
		return true;
	}

	/// <summary>
	/// Unregisters a DragAndDropListener for an DragAndDrop object. </summary>
	/// <param name="r"> the DragAndDrop object on which to unregister the listener.  This can
	/// be either a component from which can are dragged or a component on which
	/// data are dropped. </param>
	/// <param name="d"> the listener to unregister. </param>
	public static void removeDragAndDropListener(DragAndDrop r, DragAndDropListener d)
	{
		r.getDragAndDropControl().removeDragAndDropListener(d);
	}

	/// <summary>
	/// Sets the cursor feedback for when data is being dragged over a component. </summary>
	/// <param name="d"> the DragAndDrop object from which data is being dragged. </param>
	/// <param name="dsde"> the DragSourceDragEvent created when data is over a component. </param>
	public static void setDragOverFeedback(DragAndDrop d, DragSourceDragEvent dsde)
	{
		if (!d.getDragAndDropControl().allowsDrag())
		{
			return;
		}

		DragSourceContext context = dsde.getDragSourceContext();
		int dropAction = dsde.getDropAction();
		int dragAction = d.getDragAndDropControl().getDragAction();

		// if the drop action is valid, then set the cursor to represent that
		if ((dropAction & dragAction) != 0)
		{
			if (dropAction == ACTION_COPY || dropAction == ACTION_COPY_OR_MOVE)
			{
				context.setCursor(DragSource.DefaultCopyDrop);
			}
			else if (dropAction == ACTION_MOVE)
			{
				context.setCursor(DragSource.DefaultMoveDrop);
			}
			else
			{
				context.setCursor(DragSource.DefaultCopyDrop);
			}
		}
		// otherwise, put a "can't do that" cursor
		else
		{
			if (dragAction == ACTION_COPY || dragAction == ACTION_COPY_OR_MOVE)
			{
				context.setCursor(DragSource.DefaultCopyNoDrop);
			}
			else if (dragAction == ACTION_MOVE)
			{
				context.setCursor(DragSource.DefaultMoveNoDrop);
			}
			else
			{
				context.setCursor(DragSource.DefaultCopyNoDrop);
			}
		}
	}

	}

}