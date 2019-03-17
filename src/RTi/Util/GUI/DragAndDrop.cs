﻿// DragAndDrop - interface for components that support drag and drop

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
// DragAndDrop - Interface for components that support drag and drop.
//-----------------------------------------------------------------------------
// Copyright: See the COPYRIGHT file.
//-----------------------------------------------------------------------------
// History: 
// 2004-02-24	J. Thomas Sapienza, RTi	Initial version.
// 2004-04-27	JTS, RTi		* Revised after SAM's review.
//					* Renamed from RTiDragAndDrop to
//					  DragAndDrop.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//-----------------------------------------------------------------------------

namespace RTi.Util.GUI
{


	/// <summary>
	/// This interface must be implemented by any component that is to support drag
	/// and drop.  Certain Java components support drag and drop by default, such
	/// as JTextField, JPasswordField.  See this page for more information:<br>
	/// <blockquote>
	/// <a href="http://java.sun.com/docs/books/tutorial/uiswing/misc/dnd.html">
	/// How to Use Drag and Drop Data Transfer</a></blockquote><para>
	/// 
	/// In general, though, dragging is supported on the following components:<br>
	/// <ul>
	/// <li>JColorChooser</li>
	/// <li>JEditorPane</li>
	/// <li>JFileChooser</li>
	/// <li>JFormattedTextField</li>
	/// <li>JList</li>
	/// <li>JTable</li>
	/// <li>JTextArea</li>
	/// <li>JTextField</li>
	/// <li>JTextPane</li>
	/// <li>JTree</li>
	/// </ul>
	/// </para>
	/// <para>
	/// and dropping is supported on the following components:<br>
	/// <ul>
	/// <li>JColorChooser</li>
	/// <li>JEditorPane</li>
	/// <li>JFormattedTextField</li>
	/// <li>JPasswordField</li>
	/// <li>JTextArea</li>
	/// <li>JTextField</li>
	/// <li>JTextPane</li>
	/// </ul>
	/// 
	/// </para>
	/// <b>Creating Drag-and-Droppable Components</b><para>
	/// Components that support being dragged and dropped must implement four
	/// interfaces:<br><ul>
	/// <li>DragGestureListener (Swing)</li>
	/// <li>DragSourceListener (Swing)</li>
	/// <li>DropTargetListener (Swing)</li>
	/// </para>
	/// <li>DragAndDrop (this interface)</li></ul><para>
	/// 
	/// These components must also have a member variable of type DragAndDropControl,
	/// which is used to store information about how drag and drop will be handled
	/// by the component.  This variable is necessary because Java interfaces are
	/// </para>
	/// not allowed to contain member variables..<para>
	/// 
	/// Most of the work done in drag and drop will be handled by the DragAndDropUtil 
	/// class.  The methods in this interface do not require anything in their method
	/// bodies, with the exception of getDataFlavors(), getDragAndDropControl(), 
	/// </para>
	/// getTransferable() and handleDropData().<para>
	/// 
	/// Components can be defined so that they support data being dragged out of them,
	/// data being dragged into them, or both.  Note that the text in editable text
	/// fields (such as JTextField) cannot be dragged out of the text fields.  
	/// </para>
	/// It is a known limitation of Java drag and drop.<para>
	/// 
	/// </para>
	/// <b>Constructor</b><para>
	/// 
	/// The constructor of the class must create an instance of a DragAndDropControl 
	/// member variable and populate it with the details of how the component 
	/// responds to drag and drop actions.  See the DragAndDropControl Javadocs for 
	/// </para>
	/// details.<para>
	/// 
	/// The constructor must also build a DragSource, a DropTarget, or both 
	/// (depending on how the component will work), and
	/// put them into the DragAndDropControl object.  The following code demonstrates a 
	/// </para>
	/// sample constructor:<para>
	/// <blockquote><pre>
	/// public GenericConstructor(boolean allowDrag, boolean allowDrop, int dragAction,
	/// int dropAction) {
	/// __data = new DragAndDropControl(allowDrag, allowDrop, dragAction, 
	///		dropAction);
	/// if (allowDrag) {
	///		__data.setDragSource(
	///			DragAndDropUtil.createDragSource(this, dragAction, 
	///			this));
	/// }
	/// if (allowDrop) {
	///		__data.setDropTarget(
	///			DragAndDropUtil.createDropTarget(this, dropAction, 
	///			this));
	/// }
	/// }
	/// </pre></blockquote>
	/// </para>
	/// <para>
	/// 
	/// </para>
	/// <b>Starting a Drag, Ending a Drop</b><para>
	/// 
	/// The mouse action that defines when a drag begins and when a drop ends will vary
	/// from system to system, but in general, a drag begins when the user presses the
	/// mouse button down on a component and holds it down, and then moves the mouse
	/// cursor away from the component.  A drop occurs when the user -- still holding
	/// down the mouse button -- moves the mouse cursor over another component and 
	/// </para>
	/// releases the button.<para>
	/// 
	/// </para>
	/// <b>Drag and Drop Action</b><para>
	/// 
	/// Drag and drop actions are defined in DragAndDropUtil.  Drag actions refer to 
	/// what the component should do after data are dragged out of it.  Drop actions 
	/// </para>
	/// refer to what the component should do when data are dragged into it.<para>
	/// 
	/// </para>
	/// <b>Responding to a Successful Drag</b><para>
	/// 
	/// When a drag is successful, the drag component will receive a call via this
	/// interface to dragSuccessful(int action).  The action tells the drag component
	/// the action that the component onto which data was dropped responded to.  The
	/// drag component should check that action and, using the table below, treat its
	/// data appropriately.  The drop component doesn't need to worry about what the
	/// action was.  If the drag was successful, the drop component need only copy
	/// the data into itself.  The drag component will worry about what should happen
	/// </para>
	/// to the original data.<para>
	/// <table border=1>
	/// <tr>
	/// <th>Drag Component Action</th><th>Drop Component Action</th><th>Result</th>
	/// </tr><tr>
	/// <td>ACTION_NONE</td><td>ACTION_NONE</td>
	/// <td>Nothing happens.</td>
	/// </tr><tr>
	/// <td>ACTION_NONE</td><td>ACTION_COPY</td>
	/// <td>Action mismatch, nothing happens.</td>
	/// </tr><tr>
	/// <td>ACTION_NONE</td><td>ACTION_MOVE</td>
	/// <td>Action mismatch, nothing happens.</td>
	/// </tr><tr>
	/// <td>ACTION_NONE</td><td>ACTION_COPY_OR_MOVE</td>
	/// <td>Action mismatch, nothing happens.</td>
	/// </tr><tr>
	/// <td>ACTION_COPY</td><td>ACTION_NONE</td>
	/// <td>Action mismatch, nothing happens.</td>
	/// </tr><tr>
	/// <td>ACTION_COPY</td><td>ACTION_COPY</td>
	/// <td>Data is copied from the drag component to the drop component.  
	/// Data remains in the drag component.</td>
	/// </tr><tr>
	/// <td>ACTION_COPY</td><td>ACTION_MOVE</td>
	/// <td>Action mismatch, nothing happens.</td>
	/// </tr><tr>
	/// <td>ACTION_COPY</td><td>ACTION_COPY_OR_MOVE</td>
	/// <td>Data is copied from the drag component to the drop component.  
	/// Data remains in the drag component.</td>
	/// </tr><tr>
	/// <td>ACTION_MOVE</td><td>ACTION_NONE</td>
	/// <td>Action mismatch, nothing happens.</td>
	/// </tr><tr>
	/// <td>ACTION_MOVE</td><td>ACTION_COPY</td>
	/// <td>Action mismatch, nothing happens.</td>
	/// </tr><tr>
	/// <td>ACTION_MOVE</td><td>ACTION_MOVE</td>
	/// <td>Data is moved from the drag component to the drop component.  
	/// The data are removed from the drag component.</td>
	/// </tr><tr>
	/// <td>ACTION_MOVE</td><td>ACTION_COPY_OR_MOVE</td>
	/// <td>Data is moved from the drag component to the drop component.  
	/// The data are removed from the drag component.</td>
	/// </tr><tr>
	/// <td>ACTION_COPY_OR_MOVE</td><td>ACTION_NONE</td>
	/// <td>Action mismatch, nothing happens.</td>
	/// </tr><tr>
	/// <td>ACTION_COPY_OR_MOVE</td><td>ACTION_COPY</td>
	/// <td>Data is copied from the drag component to the drop component.  
	/// Data remains in the drag component.</td>
	/// </tr><tr>
	/// <td>ACTION_COPY_OR_MOVE</td><td>ACTION_MOVE</td>
	/// <td>Data is moved from the drag component to the drop component.  
	/// The data are removed from the drag component.</td>
	/// </tr><tr>
	/// <td>ACTION_COPY_OR_MOVE</td><td>ACTION_COPY_OR_MOVE</td>
	/// <td>Data is placed into the drop component.  Data can either remain
	/// in the drag component or be removed, depending on how the developer
	/// wants to implement it.</td>
	/// </para>
	/// </tr><tr></table><para>
	/// 
	/// </para>
	/// <b>Implemented Listeners' Method Bodies</b><para>
	/// 
	/// Components that are to be drag-and-droppable must provide method bodies
	/// for all the interfaces in the Listeners listed above.  The following code 
	/// should be able to be used for all the components that implement these 
	/// listeners.  Copy it and paste it into the class source file.  Because of 
	/// the reliance on DragAndDropUtil and the callback methods defined in this 
	/// interface below, it is unlikely that anything more will need added to 
	/// </para>
	/// the methods listed in this section.<para>
	/// <blockquote><pre>
	/// ////////////////////////////////////////////////////////////////
	/// // Drag Gesture events
	/// public void dragGestureRecognized(DragGestureEvent dge) {
	/// DragAndDropUtil.dragStart(this, this, dge);
	/// }
	/// 
	/// ////////////////////////////////////////////////////////////////
	/// // Drag events
	/// public void dragDropEnd(DragSourceDropEvent dsde) {
	/// DragAndDropUtil.dragDropEnd(this, dsde);
	/// }
	/// public void dragEnter(DragSourceDragEvent dsde) {
	/// DragAndDropUtil.dragEnter(this, dsde);
	/// }
	/// public void dragExit(DragSourceEvent dse) {
	/// DragAndDropUtil.dragExit(this, dse);
	/// }
	/// public void dragOver(DragSourceDragEvent dsde) {
	/// DragAndDropUtil.dragOver(this, dsde);
	/// }
	/// public void dropActionChanged(DragSourceDragEvent dsde) {
	/// DragAndDropUtil.dropActionChanged(this, dsde);
	/// }
	/// 
	/// ////////////////////////////////////////////////////////////////
	/// // Drop events
	/// public void dropActionChanged(DropTargetDragEvent dtde) {
	/// DragAndDropUtil.dropActionChanged(this, dtde);
	/// }
	/// public void dragEnter(DropTargetDragEvent dtde) {
	/// DragAndDropUtil.dragEnter(this, dtde);
	/// }
	/// public void dragExit(DropTargetEvent dte) {
	/// DragAndDropUtil.dragExit(this, dte);
	/// }
	/// public void dragOver(DropTargetDragEvent dtde) {
	/// DragAndDropUtil.dragOver(this, dtde);
	/// }
	/// public void drop(DropTargetDropEvent dtde) {
	/// DragAndDropUtil.drop(this, dtde);
	/// }
	/// </para>
	/// </pre></blockquote><para>
	/// 
	/// At this point, the developer should write method bodies for:<br><ul>
	/// <li>getDataFlavors</li>
	/// <li>getDragAndDropControl</li>
	/// <li>getTransferable</li>
	/// </para>
	/// <li>handleDropData</li></ul><para>
	/// and they are done.  See below for what these methods should contain.
	/// </para>
	/// </summary>
	public interface DragAndDrop
	{

	/// <summary>
	/// This method is called for the component from which data are being dragged
	/// to indicate that a drag has just been started.
	/// </summary>
	void dragStarted();

	/// <summary>
	/// This method is called for the component from which data are being dragged
	/// to indicate that a drag ended and the data was successfully dragged. </summary>
	/// <param name="action"> the action that was attempted (ACTION_MOVE, ACTION_COPY, etc). </param>
	void dragSuccessful(int action);

	/// <summary>
	/// This method is called for the component from which data are being dragged to
	/// indicate that although a drag ended, it was not successful for one reason 
	/// or another.  No information is available about why the drag was not 
	/// successful, only that it failed. </summary>
	/// <param name="action"> the action that was attempted (ACTION_MOVE, ACTION_COPY, etc). </param>
	void dragUnsuccessful(int action);

	/// <summary>
	/// This method is called for a component on which data can be dropped 
	/// when data has been dragged over it and when the component can accept the data.  
	/// It can be used to change how the drop component looks in order to visually
	/// show the user that they can drag data there successfully, for instance, by 
	/// changing the border color to green.
	/// </summary>
	void dropAllowed();

	/// <summary>
	/// This method is called for a a component on which data can be dropped 
	/// when a drag has left its area.
	/// If the dropAllowed() or dropNotAllowed() methods were used to change how the 
	/// component looks in, this method can be used to set it back to looking normal.
	/// </summary>
	void dropExited();

	/// <summary>
	/// This method is called for a component on which data can be dropped 
	/// when data has been dragged over it and when the component cannot 
	/// accept the data.  
	/// It can be used to change how the drop component looks in order to visually
	/// show the user that they cannot drag data there successfully, for instance, by 
	/// changing the border color to red.
	/// </summary>
	void dropNotAllowed();

	/// <summary>
	/// Called on the component on which data are being dropped when a drag was 
	/// successful and data was dropped onto that component.
	/// </summary>
	void dropSuccessful();

	/// <summary>
	/// Called on the component on which data are being dropped when a drag was not
	/// successful, and no data was transferred into that component.
	/// </summary>
	void dropUnsuccessful();

	/// <summary>
	/// Returns an array of all the data flavors supported by the transferable object.
	/// The method body will nearly always look like this (where XYZ is a data class
	/// that has extended Transferable):<para>
	/// <blockquote><pre>
	/// XYX xyz = new XYZ(...);
	/// return xyz.getTransferDataFlavors()
	/// </pre></blockquote>
	/// </para>
	/// </summary>
	/// <returns> an array of all the data flavors supported by the transferable object. </returns>
	DataFlavor[] getDataFlavors();

	/// <summary>
	/// Returns the DragAndDropControl associated with the DragAndDrop object.  It is 
	/// assumed by DragAndDropUtil to never be null. </summary>
	/// <returns> the DragAndDropControl associated with the DragAndDrop object. </returns>
	DragAndDropControl getDragAndDropControl();

	/// <summary>
	/// Returns the Transferable object associated with this drag and drop component. </summary>
	/// <returns> the Transferable object associated with this drag and drop component. </returns>
	Transferable getTransferable();

	/// <summary>
	/// Called for a component when when data has been successfully droppedon it.  
	/// The object is the data that was transferred, and this method must fill 
	/// it into the object on which it was dropped. </summary>
	/// <param name="o"> the data that was dragged. </param>
	bool handleDropData(object o, Point p);

	/// <summary>
	/// Sets an alternate transferable object that should be transferred in a drag 
	/// instead of using the default object.  As an example of when this could be used:
	/// <para>An application may set an alternate transferable in a 
	/// DragAndDropSimpleJComboBox, so that when something is dragged from the combo 
	/// box, it is not the text displayed in the combo box that is dragged, but an
	/// object to which the text refers.  The combo box may have lists of time series
	/// identifiers, but when the drag occurs, the listener (notified by 
	/// dragAboutToStart from the DragAndDropListener class) can put the time series
	/// to be transferred in the DragAndDrop as the alternate transferable and then
	/// the time series, not the time series identifier, will be what is dragged and
	/// dropped.
	/// </para>
	/// </summary>
	/// <param name="t"> the alternate transferable to set. </param>
	void setAlternateTransferable(Transferable t);

	}

}