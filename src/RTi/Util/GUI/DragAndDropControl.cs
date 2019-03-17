using System.Collections.Generic;

// DragAndDropControl - class for holding data necessary for components that implement drag and drop capabilities.

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
// DragAndDropControl - class for holding data necessary for components that 
//	implement drag and drop capabilities.
//-----------------------------------------------------------------------------
// Copyright: See the COPYRIGHT file.
//-----------------------------------------------------------------------------
// History: 
// 2004-02-24	J. Thomas Sapienza, RTi	Initial version.
// 2004-03-04	JTS, RTi		Updated Javadocs in response to 
//					numerous changes.
// 2004-04-28	JTS, RTi		* Revised after SAM's review.
//					* Renamed from DragAndDropData to 
//					  DragAndDropControl.
//-----------------------------------------------------------------------------

namespace RTi.Util.GUI
{



	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// This class holds data necessary for components that implement DragAndDrop.
	/// The data are stored here mainly because Java interfaces are not 
	/// allowed to hold member variables.<para>
	/// 
	/// All classes that implement DragAndDrop need to have a private member 
	/// variable of this type, and they must all implement a method to return that 
	/// member variable.  It is through the data stored in this variable that the 
	/// DragAndDropUtil code knows how drags and drops should work on components.  
	/// Developers building drag-and-drop capable components should not need to do 
	/// much with the DragAndDropControl objects once they are instantiated.  
	/// </para>
	/// DragAndDropUtil will take care of working with this data.<para>
	/// 
	/// See the DragAndDropListener documentation for information on the order in
	/// which listener calls happen.
	/// </para>
	/// </summary>
	public class DragAndDropControl
	{

	/// <summary>
	/// The name of the class.
	/// </summary>
	private readonly string __CLASS = "DragAndDropControl";

	/// <summary>
	/// The action that is performed on a drag (see DragAndDropUtil.ACTION_*).  For 
	/// example, if this is ACTION_COPY, then when data are dragged from a component
	/// the system knows that the data should be copied from that component and not
	/// removed once the drag is complete.
	/// </summary>
	private int __dragAction = DragAndDropUtil.ACTION_NONE;

	/// <summary>
	/// The action that is performed on a drop (see DragAndDropUtil.ACTION_*).  For
	/// example, if this is ACTION_MOVE, then when data are dropped on a component
	/// the system knows that the data are to be removed from the origin component
	/// and placed into the destination component.
	/// </summary>
	private int __dropAction = DragAndDropUtil.ACTION_NONE;

	/// <summary>
	/// The drag source.  DragSource objects are Java objects that contain information
	/// about where drags can and cannot occur and relay that to the JVM running the GUI.
	/// </summary>
	private DragSource __dragSource = null;

	/// <summary>
	/// The drop target.  DropTarget objects are Java objects that contain information
	/// about where drops can and cannot occur and relay that to the JVM running the GUI.
	/// </summary>
	private DropTarget __dropTarget = null;

	/// <summary>
	/// The alternate transferable object that should be used instead of the default
	/// component transferable (if not null).   An example of when this could be used:
	/// <para>An application may set an alternate transferable in a 
	/// DragAndDropSimpleJComboBox, so that when something is dragged from the combo 
	/// box, it is not the text displayed in the combo box that is dragged, but an
	/// object to which the text refers.  The combo box may have lists of time series
	/// identifiers, but when the drag occurs, the listener (notified by 
	/// dragAboutToStart from the DragAndDropListener class) can put the time series
	/// to be transferred in the DragAndDrop as the alternate transferable and then
	/// the time series, not the time series identifier, will be what is dragged and dropped.
	/// 
	/// </para>
	/// </summary>
	private Transferable __alternateTransferable;

	/// <summary>
	/// The list of DragAndDropListeners registered for this drag and drop 
	/// component.  This is used so that other classes can be informed of events 
	/// occurring during the drag and drop process.
	/// </summary>
	private IList<DragAndDropListener> __listeners = null;

	/// <summary>
	/// Constructor.  Sets up whether drags and drops are possible as well as the 
	/// appropriate drag and drop actions. </summary>
	/// <param name="dragAction"> the action that drags perform (see DragAndDropUtil.ACTION_*) 
	/// when data is dragged from the component. </param>
	/// <param name="dropAction"> the action that drops perform (see DragAndDropUtil.ACTION_*) 
	/// when data is dropped on the component. </param>
	public DragAndDropControl(int dragAction, int dropAction)
	{
		__listeners = new List<DragAndDropListener>();
		setDragAction(dragAction);
		setDropAction(dropAction);
	}

	/// <summary>
	/// Registers a DragAndDropListener on the component to which this data belongs.
	/// If the listener is already registered, it will not be registered again. </summary>
	/// <param name="d"> the DragAndDropListener to register. </param>
	public virtual void addDragAndDropListener(DragAndDropListener d)
	{
		DragAndDropListener listener;
		for (int i = 0; i < __listeners.Count; i++)
		{
			listener = (DragAndDropListener)__listeners[i];
			if (listener == d)
			{
				return;
			}
		}
		__listeners.Add(d);
	}

	/// <summary>
	/// Returns whether drags are supported.  Checks to make sure that the drag action
	/// is not ACTION_NONE. </summary>
	/// <returns> whether drags are supported. </returns>
	public virtual bool allowsDrag()
	{
		// any other drag action (ACTION_MOVE, ACTION_COPY, ACTION_COPY_OR_MOVE)
		// means that data can be dragged from the component.
		if (__dragAction != DragAndDropUtil.ACTION_NONE)
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Returns whether drops are supported.  Checks to make sure that the drop action
	/// is not ACTION_NONE. </summary>
	/// <returns> whether drops are supported. </returns>
	public virtual bool allowsDrop()
	{
		// any other drag action (ACTION_MOVE, ACTION_COPY, ACTION_COPY_OR_MOVE)
		// means that data can dropped on the component.
		if (__dropAction != DragAndDropUtil.ACTION_NONE)
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~DragAndDropControl()
	{
		__dragSource = null;
		__dropTarget = null;
		__alternateTransferable = null;
		__listeners = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the alternate transferable of the component to which this data 
	/// belongs.  For more information on alternate transferables, see the javadocs
	/// for setAlternateTransferable(). </summary>
	/// <returns> the alternate transferable of the component to which thsi data 
	/// belongs. </returns>
	public virtual Transferable getAlternateTransferable()
	{
		return __alternateTransferable;
	}

	/// <summary>
	/// Returns the drag action. </summary>
	/// <returns> the drag action. </returns>
	public virtual int getDragAction()
	{
		return __dragAction;
	}

	/// <summary>
	/// Returns the drop action. </summary>
	/// <returns> the drop action. </returns>
	public virtual int getDropAction()
	{
		return __dropAction;
	}

	/// <summary>
	/// Returns the drag source. </summary>
	/// <returns> the drag source. </returns>
	public virtual DragSource getDragSource()
	{
		return __dragSource;
	}

	/// <summary>
	/// Returns the drop target. </summary>
	/// <returns> the drop target. </returns>
	public virtual DropTarget getDropTarget()
	{
		return __dropTarget;
	}

	/// <summary>
	/// Notifies DragAndDropListeners registered on the drag component to which 
	/// this data belongs that data is about to be copied into the drag for 
	/// transfer to another component.  At this point, registered listeners can 
	/// veto the drag or can make some last-minute changes to the data before 
	/// it is put into the drag. </summary>
	/// <returns> true if all the listeners will allow the drag to begin, false if any
	/// veto the drag. </returns>
	public virtual bool notifyListenersDragAboutToStart()
	{
		bool successful = true;
		for (int i = 0; i < __listeners.Count; i++)
		{
			if (!((DragAndDropListener)__listeners[i]).dragAboutToStart())
			{
				successful = false;
			}
		}
		return successful;
	}

	/// <summary>
	/// Notifies DragAndDropListeners registered on the drag component to which
	/// this data belongs that the drag has officially started and data is 
	/// currently under the mouse cursor and being dragged to another component.
	/// </summary>
	public virtual void notifyListenersDragStarted()
	{
		for (int i = 0; i < __listeners.Count; i++)
		{
			((DragAndDropListener)__listeners[i]).dragStarted();
		}
	}

	/// <summary>
	/// Notifies DragAndDropListeners registered on the drag component to which this
	/// data belongs that data dragged out of this component was successfully dropped elsewhere. </summary>
	/// <param name="action"> the action performed by the component that received the dropped data. </param>
	public virtual void notifyListenersDragSuccessful(int action)
	{
		for (int i = 0; i < __listeners.Count; i++)
		{
			((DragAndDropListener)__listeners[i]).dragSuccessful(action);
		}
	}

	/// <summary>
	/// Notifies DragAndDropListeners registered on the drag component to which this
	/// data belongs that data dragged out of this component was unsuccessfully dropped elsewhere. </summary>
	/// <param name="action"> the action attempted by the component that tried to receive the dropped data. </param>
	public virtual void notifyListenersDragUnsuccessful(int action)
	{
		for (int i = 0; i < __listeners.Count; i++)
		{
			((DragAndDropListener)__listeners[i]).dragUnsuccessful(action);
		}
	}

	/// <summary>
	/// Notifies DragAndDropListeners registered on the drop component to which this
	/// data belongs that a drop is allowed for the area that the mouse cursor is
	/// over.  Components can use this notification to change GUI attributes such
	/// as Colors or information in status bars.
	/// </summary>
	public virtual void notifyListenersDropAllowed()
	{
		for (int i = 0; i < __listeners.Count; i++)
		{
			((DragAndDropListener)__listeners[i]).dropAllowed();
		}
	}

	/// <summary>
	/// Notifies DragAndDropListeners registered on the drop component to which 
	/// this data belongs that a drop area has been exited.  Components can use this
	/// notification to change back GUI attributes that may have been set when the
	/// mouse cursor moved into a drop area.
	/// </summary>
	public virtual void notifyListenersDropExited()
	{
		for (int i = 0; i < __listeners.Count; i++)
		{
			((DragAndDropListener)__listeners[i]).dropExited();
		}
	}

	/// <summary>
	/// Notifies DragAndDropListeners registered on the drop component to which this
	/// data belongs that a drop is not allowed for the area that the mouse cursor
	/// is over.  Components can use this notification to change GUI attributes such
	/// as Colors, or to put information in status bars.
	/// </summary>
	public virtual void notifyListenersDropNotAllowed()
	{
		for (int i = 0; i < __listeners.Count; i++)
		{
			((DragAndDropListener)__listeners[i]).dropNotAllowed();
		}
	}

	/// <summary>
	/// Notifies DragAndDropListeners registered on the drop component to which this
	/// data belongs that a data drop was performed successfully.
	/// </summary>
	public virtual void notifyListenersDropSuccessful()
	{
		for (int i = 0; i < __listeners.Count; i++)
		{
			((DragAndDropListener)__listeners[i]).dropSuccessful();
		}
	}

	/// <summary>
	/// Notifies DragAndDropListeners registered on the drop component to which this
	/// data belongs that a data drop was performed but it was not successful.
	/// </summary>
	public virtual void notifyListenersDropUnsuccessful()
	{
		for (int i = 0; i < __listeners.Count; i++)
		{
			((DragAndDropListener)__listeners[i]).dropUnsuccessful();
		}
	}

	/// <summary>
	/// Unregisters a DragAndDropListener on the component to which this data belongs. </summary>
	/// <param name="d"> the DragAndDropListener to unregister. </param>
	public virtual void removeDragAndDropListener(DragAndDropListener d)
	{
		DragAndDropListener listener;
		for (int i = 0; i < __listeners.Count; i++)
		{
			listener = (DragAndDropListener)__listeners[i];
			if (listener == d)
			{
				__listeners.Remove(listener);
				return;
			}
		}
	}

	/// <summary>
	/// Sets an alternate transferable object that a DragAndDrop-enabled GUI object
	/// can put into a drag.  Transferable objects are the data that are 
	/// transferred between components in a drag (see 
	/// RTi.Util.GUI.DragAndDropTransferPrimitive).  <para>
	/// 
	/// </para>
	/// Alternate transferables are useful for situations such as the following:<para>
	/// 
	/// A DragAndDropSimpleJComboBox holds the names of time series.  A time series
	/// can be dragged from the combo box to another component, and when that happens
	/// the actual time series data, not the time series data name, should be moved
	/// </para>
	/// to the other component.  <para>
	/// 
	/// Since DragAndDropSimpleJComboBoxes store strings and transfer their data
	/// using DragAndDropPrimitives, a drag and drop from this class would transfer
	/// a String by default.  But by listening to the 'dragAboutToStart()' event
	/// (in the DragAndDrop interface) and using this method, the GUI can set up
	/// the TS to be the Transferable object when the drag starts.  Once the drag 
	/// is complete -- either successful or not -- the alternate transferable will
	/// </para>
	/// be set to null automatically.<para>
	/// 
	/// </para>
	/// Here is the setAlternateTransferable() pseudo-code for the above example:<para>
	/// 
	/// <blockquote><pre>
	/// public boolean dragAboutToStart() {
	///		// get the time series id stored in the combo box
	///		String tsid = __tsidComboBox.getSelected();
	///		// try to get the time series data
	///		TS ts = getTimeSeriesForID(tsid);
	///		// if no data was returned, the drag cannot happen ...
	///		if (ts == null) {
	///			// ... so veto the drag by returning false.
	///			return false;
	///		}
	///		else {
	///			// ... otherwise, set the time series as the 
	///			// alternate transferable (otherwise the combo box
	///			// is set up to automatically transfer whatever
	///			// string is currently selected -- in this case
	///			// the tsid)
	///			__tsidComboBox.setAlternateTransferable(ts);
	///			// ... and accept the transfer
	///			return true;
	///		}
	/// }
	/// </pre></blockquote>
	/// </para>
	/// </summary>
	public virtual void setAlternateTransferable(Transferable t)
	{
		__alternateTransferable = t;
	}

	/// <summary>
	/// Sets the action to be performed when data is dragged from this object.  See
	/// DragAndDropUtil.ACTION_*. </summary>
	/// <param name="dragAction"> drag action to set. </param>
	public virtual void setDragAction(int dragAction)
	{
		string routine = __CLASS + ".setAction";
		if (dragAction != DragAndDropUtil.ACTION_COPY && dragAction != DragAndDropUtil.ACTION_MOVE && dragAction != DragAndDropUtil.ACTION_COPY_OR_MOVE && dragAction != DragAndDropUtil.ACTION_NONE)
		{
				Message.printWarning(2, routine, "Invalid dragAction: " + dragAction + ".  Defaulting to " + "DragAndDropUtil.ACTION_NONE.");
			__dragAction = DragAndDropUtil.ACTION_NONE;
		}
		else
		{
			__dragAction = dragAction;
		}
	}

	/// <summary>
	/// Sets the action to be performed when data is dropped on this object.  See 
	/// DragAndDropUtil.ACTION_*. </summary>
	/// <param name="dropAction"> drop action to set. </param>
	public virtual void setDropAction(int dropAction)
	{
		string routine = __CLASS + ".setAction";
		if (dropAction != DragAndDropUtil.ACTION_COPY && dropAction != DragAndDropUtil.ACTION_MOVE && dropAction != DragAndDropUtil.ACTION_COPY_OR_MOVE && dropAction != DragAndDropUtil.ACTION_NONE)
		{
				Message.printWarning(2, routine, "Invalid dropAction: " + dropAction + ".  Defaulting to " + "DragAndDropUtil.ACTION_NONE.");
			__dropAction = DragAndDropUtil.ACTION_NONE;
		}
		else
		{
			__dropAction = dropAction;
		}
	}

	/// <summary>
	/// Sets the drag source. </summary>
	/// <param name="ds"> drag source to set. </param>
	public virtual void setDragSource(DragSource ds)
	{
		__dragSource = ds;
	}

	/// <summary>
	/// Sets the drop target. </summary>
	/// <param name="dt"> drop target to set. </param>
	public virtual void setDropTarget(DropTarget dt)
	{
		__dropTarget = dt;
	}

	/// <summary>
	/// Returns a String representation of this object, suitable for debugging. </summary>
	/// <returns> a String representation of this object, suitable for debugging. </returns>
	public override string ToString()
	{
		string atc = null;
		if (__alternateTransferable == null)
		{
			atc = "null";
		}
		else
		{
			atc = "" + __alternateTransferable.GetType();
		}
		return "AllowsDrag: " + allowsDrag() + "\n" +
			"AllowsDrop: " + allowsDrop() + "\n" +
			"DragAction: " + __dragAction + "\n" +
			"DropAction: " + __dropAction + "\n" +
			"DragSource: " + __dragSource + "\n" +
			"DropTarget: " + __dropTarget + "\n" +
			"AlternateTransferable Class: " + atc;
	}

	}

}