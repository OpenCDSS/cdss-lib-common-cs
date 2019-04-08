using System;
using System.Collections.Generic;

// GeoViewLegendJTree_Node - convenience class to use when putting legend data into a SimpleJTree

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
// GeoViewLegendJTree_Node - Convenience class to use when putting ESRI data
// into a SimpleJTree.
//-----------------------------------------------------------------------------
// Copyright: See the COPYRIGHT file.
//-----------------------------------------------------------------------------
// History: 
//
// 2003-05-12	J. Thomas Sapienza, RTI	Initial version
// 2003-05-13	JTS, RTi		Added support for popup menus
// 2003-05-14	JTS, RTi		* Text field selection works
//					* Lots of javadoc'ing.
// 2003-05-21	JTS, RTi		Layers were not being selected when the
//					nodes were selected; fixed that.
// 2003-05-22	JTS, RTi		* Added isSelected/setSelected/
//					  isVisible/setVisible.
//					* Added getLayerView
// 2004-09-16	JTS, RTi		Changed the call of isPopupTrigger to
//					use the one in the JPopupMenu.
// 2004-10-14	JTS, RTi		Added getFieldText().
// 2005-04-27	JTS, RTi		Added finalize().
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//-----------------------------------------------------------------------------

namespace RTi.GIS.GeoView
{



	//import javax.swing.JEditorPane;

	using JGUIUtil = RTi.Util.GUI.JGUIUtil;

	using SimpleJTree_CellRenderer = RTi.Util.GUI.SimpleJTree_CellRenderer;
	using SimpleJTree_Node = RTi.Util.GUI.SimpleJTree_Node;
	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// This class is a convenience class for displaying CheckBox and label information
	/// in a Tree similar to how ESRI handles its Table of Contents tree sections.
	/// These nodes contain two components, a JCheckBox (with no text) and a separate JLabel.  
	/// </summary>
	public class GeoViewLegendJTree_Node : SimpleJTree_Node, FocusListener, MouseListener, ItemListener, ItemSelectable
	{

	/// <summary>
	/// Whether this node has been selected (i.e., the label has been clicked on) or not
	/// </summary>
	private bool __selected = false;

	/// <summary>
	/// The Color in which the background of the non-selected node text should be drawn.
	/// </summary>
	private Color bg = null;
	/// <summary>
	/// The Color in which the foreground of the non-selected node text should be drawn.
	/// </summary>
	private Color fg = null;

	/// <summary>
	/// Reference to the tree in which this component appears.
	/// </summary>
	private GeoViewLegendJTree __tree;

	/// <summary>
	/// Reference to the unlabelled checkbox that appears in this component.
	/// </summary>
	private JCheckBox __check = null;

	/// <summary>
	/// The popup menu associated with this node.  
	/// </summary>
	private JPopupMenu __popup = null;

	/// <summary>
	/// Label that appears in this component.  Originally used a JTextField to automatically handle some of
	/// the selection rendering.  However, JTextField did not cleanly handle HTML labels so switch to a JLabel.
	/// </summary>
	//private JEditorPane __field = null;
	//private JButton __field = null;
	//private JLabel __field = null;
	private JTextField __field = null;

	/// <summary>
	/// The listeners that are registered to listen for this objects item state changed events.
	/// </summary>
	private System.Collections.IList __listeners = null;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="text"> the Text to appear next to the JCheckBox in this component. </param>
	/// <param name="name"> the name of this node. </param>
	/// <param name="tree"> the tree in which this component appears </param>
	public GeoViewLegendJTree_Node(string text, string name, GeoViewLegendJTree tree) : base(new JPanel(), name)
	{
		initialize(text, name, tree, null);
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="text"> the Text to appear next to the JCheckbox in this component. </param>
	/// <param name="name"> the name of this node. </param>
	/// <param name="tree"> the tree in which this component appears </param>
	/// <param name="popupMenu"> the popupMenu that this node should display. </param>
	public GeoViewLegendJTree_Node(string text, string name, GeoViewLegendJTree tree, JPopupMenu popupMenu) : base(new JPanel(), name)
	{
		initialize(text, name, tree, popupMenu);
	}

	/// <summary>
	/// Registers an item listener for this component. </summary>
	/// <param name="listener"> the listener to add to the list of listeners. </param>
	public virtual void addItemListener(ItemListener listener)
	{
		__listeners.Add(listener);
	}

	/// <summary>
	/// Deselects all the labels in all the other nodes in the tre.
	/// </summary>
	private void deselectAllOthers()
	{
		deselectAllOthers(__tree.getRoot());
	}

	/// <summary>
	/// Utility method used by deselectAllOthers() </summary>
	/// <param name="node"> the node from which to recurse the tree. </param>
	private void deselectAllOthers(SimpleJTree_Node node)
	{
		if (node is GeoViewLegendJTree_Node)
		{
			if (node != this)
			{
				((GeoViewLegendJTree_Node)node).deselectField();
			}
		}
		if (node.getChildCount() >= 0)
		{
			for (System.Collections.IEnumerator e = node.children(); e.MoveNext();)
			{
				SimpleJTree_Node n = (SimpleJTree_Node)e.Current;
				deselectAllOthers(n);
			}
		}
	}

	/// <summary>
	/// Deselects the text field in this node.
	/// </summary>
	public virtual void deselectField()
	{
		__field.setBackground(bg);
		__field.setForeground(fg);
		__field.repaint();
		__selected = false;
		GeoLayerView layerView = (GeoLayerView)getData();
		if (layerView != null)
		{
			layerView.isSelected(false);
		}
	}

	/// <summary>
	/// Returns the text stored in this node. </summary>
	/// <returns> the text stored in this node. </returns>
	public virtual string getFieldText()
	{
		if (__field == null)
		{
			return null;
		}
		return __field.getText().Trim();
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~GeoViewLegendJTree_Node()
	{
		bg = null;
		fg = null;
		__tree = null;
		__check = null;
		__popup = null;
		__field = null;
		__listeners = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Indicate when focus is gained on the component.
	/// </summary>
	public virtual void focusGained(FocusEvent e)
	{
		Message.printStatus(2,"","Legend item focused gained for label component " + __field);
	}

	/// <summary>
	/// Indicate when focus is lost on the component.
	/// </summary>
	public virtual void focusLost(FocusEvent e)
	{
		Message.printStatus(2,"","Legend item focused gained for label component " + __field);
	}

	/// <summary>
	/// Returns the layer view stored in this node. </summary>
	/// <returns> the layer view stored in this node. </returns>
	public virtual GeoLayerView getLayerView()
	{
		return (GeoLayerView)getData();
	}

	/// <summary>
	/// Gets the selected objects (from extending ItemSelectable; not used). </summary>
	/// <returns> null. </returns>
	public virtual object[] getSelectedObjects()
	{
		return null;
	}

	/// <summary>
	/// Initializes the settings in the GeoViewLegendJTree_Node. </summary>
	/// <param name="text"> the Text to appear next to the JCheckBox in this component. </param>
	/// <param name="name"> the name of this node </param>
	/// <param name="tree"> the SimpleJTree that contains this component </param>
	/// <param name="listener"> the ItemListener to register for this component </param>
	/// <param name="popupMenu"> the popupMenu that this node should display.  If null, no popup will be displayed. </param>
	private void initialize(string text, string name, GeoViewLegendJTree tree, JPopupMenu popup)
	{
		JPanel panel = new JPanel();
		panel.setLayout(new GridBagLayout());
		__check = new JCheckBox();
		__check.setBackground(UIManager.getColor("Tree.textBackground"));
		__field = new JTextField();
		//__field = new JEditorPane();
		__tree = tree;

		// Because of the way these two components (the checkbox and the
		// label) are drawn, sometimes the first letter of the JLabel is
		// slightly (like, 2 pixels) overlapped by the CheckBox.  Adding
		// a single space at the front of the label text seems to avoid this.

		if (text.StartsWith("<", StringComparison.Ordinal))
		{
			// Assume HTML so just set it
			__field.setText(text);
			// TODO SAM 2010-12-15 Uncomment this if using a JEditPane with HTML
			//__field.setContentType("mime/html");
		}
		else
		{
			// Add extra space
			__field.setText(" " + text);
			__field.setFont((new SimpleJTree_CellRenderer()).getFont());
		}

		__field.addMouseListener(this);
		// JTextField and JEditorPane...
		__field.setEditable(false);
		// JButton and JLabel...
		//__field.addFocusListener(this);
		//__field.setFocusable(true);
		// Don't want any decorative border
		__field.setBorder(null);
		__field.setBackground(UIManager.getColor("Tree.textBackground"));
		JGUIUtil.addComponent(panel, __check, 0, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(panel, __field, 1, 0, 2, 1, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.WEST);
		setComponent(panel);

		__check.addItemListener(this);
		__listeners = new List<object>();
		addItemListener(tree);

		__popup = popup;

		// Store the default label drawing colors
		bg = __field.getBackground();
		fg = __field.getForeground();
	}

	/// <summary>
	/// Returns whether the check box is selected or not. </summary>
	/// <returns> whether the check box is selected or not. </returns>
	public virtual bool isCheckBoxSelected()
	{
		return __check.isSelected();
	}

	/// <summary>
	/// Returns whether the layer associated with this node is visible or not. </summary>
	/// <returns> whether the layer associated with this node is visible or not. </returns>
	public override bool isVisible()
	{
		return isCheckBoxSelected();
	}

	/// <summary>
	/// Returns whether the text field is selected or not. </summary>
	/// <returns> whether the text field is selected or not. </returns>
	public virtual bool isTextSelected()
	{
		return __selected;
	}

	/// <summary>
	/// Returns whether the layer associated with this node is selected or not. </summary>
	/// <returns> whether the layer associated with this node is selected or not. </returns>
	public virtual bool isSelected()
	{
		return isTextSelected();
	}

	/// <summary>
	/// Sets whether the layer associated with this node is selected or not. </summary>
	/// <param name="sel"> whether the layer is selected or not. </param>
	public virtual void setSelected(bool sel)
	{
		if (sel)
		{
			selectField();
		}
		else
		{

			deselectField();
		}
	}

	/// <summary>
	/// Sets whether the layer associated with this node is visible or not. </summary>
	/// <param name="vis"> whether the layer is visible or not. </param>
	public virtual void setVisible(bool vis)
	{
		if (vis)
		{
			__check.setSelected(true);
		}
		else
		{
			__check.setSelected(false);
		}
	}

	/// <summary>
	/// The internal item state changed event that occurs when the JCheckBox is clicked.
	/// Internally, this class is its own listener for the JCheckBox's item state
	/// changed event.  It catches the event and then RE-posts it so that the 
	/// GeoViewLegendJTree that catches the new event can see which specific node issued the event. </summary>
	/// <param name="e"> the ItemEvent that happened. </param>
	public virtual void itemStateChanged(ItemEvent e)
	{
		ItemEvent newEvt = new ItemEvent(this, 0, null, e.getStateChange());
		for (int i = 0; i < __listeners.Count; i++)
		{
			ItemListener l = (ItemListener)__listeners[i];
			l.itemStateChanged(newEvt);
		}
	}

	/// <summary>
	/// Checks to see if the mouse event would trigger display of the popup menu.
	/// The popup menu does not display if it is null. </summary>
	/// <param name="e"> the MouseEvent that happened. </param>
	private void maybeShowPopup(MouseEvent e)
	{
		if (__popup != null && __popup.isPopupTrigger(e))
		{
			__popup.show(e.getComponent(), e.getX(), e.getY());
		}
	}

	/// <summary>
	/// Responds to mouse clicked events; does nothing. </summary>
	/// <param name="event"> the MouseEvent that happened. </param>
	public virtual void mouseClicked(MouseEvent @event)
	{
	}

	/// <summary>
	/// Responds to mouse dragged events; does nothing. </summary>
	/// <param name="event"> the MouseEvent that happened. </param>
	public virtual void mouseDragged(MouseEvent @event)
	{
	}

	/// <summary>
	/// Responds to mouse entered events; does nothing. </summary>
	/// <param name="event"> the MouseEvent that happened. </param>
	public virtual void mouseEntered(MouseEvent @event)
	{
	}

	/// <summary>
	/// Responds to mouse exited events; does nothing. </summary>
	/// <param name="event"> the MouseEvent that happened. </param>
	public virtual void mouseExited(MouseEvent @event)
	{
	}

	/// <summary>
	/// Responds to mouse moved events; does nothing. </summary>
	/// <param name="event"> the MouseEvent that happened. </param>
	public virtual void mouseMoved(MouseEvent @event)
	{
	}

	/// <summary>
	/// Responds to mouse pressed events. </summary>
	/// <param name="event"> the MouseEvent that happened. </param>
	public virtual void mousePressed(MouseEvent @event)
	{
		if (@event.getButton() == 1)
		{
			if (!@event.isControlDown())
			{
				deselectAllOthers();
				selectField();
			}
			else
			{
				if (__selected)
				{
					deselectField();
				}
				else
				{
					selectField();
				}
			}
			__tree.repaint();
		}
		// A node was either selected or deselected -- repaint the buttons
		// in the geoviewjpanel as appropriate
		__tree.updateGeoViewJPanelButtons();
	}

	/// <summary>
	/// Responds to mouse released events. </summary>
	/// <param name="event"> the MouseEvent that happened. </param>
	public virtual void mouseReleased(MouseEvent @event)
	{
		maybeShowPopup(@event);
	}

	/// <summary>
	/// Removes an item listener from the list of listeners. </summary>
	/// <param name="listener"> the listener to remove. </param>
	public virtual void removeItemListener(ItemListener listener)
	{
		for (int i = 0; i < __listeners.Count; i++)
		{
			if ((ItemListener)__listeners[i] == listener)
			{
				__listeners.RemoveAt(i);
			}
		}
	}

	/// <summary>
	/// Select's this node's text field.
	/// </summary>
	public virtual void selectField()
	{
		__selected = true;
		JTextField tf = new JTextField(); // use this to get selection colors to mimic a JTextField
		//__field.setBackground(__field.getSelectionColor());
		//__field.setForeground(__field.getSelectedTextColor());
		__field.setBackground(tf.getSelectionColor());
		__field.setForeground(tf.getSelectedTextColor());
		__field.repaint();
		GeoLayerView layerView = (GeoLayerView)getData();
		if (layerView != null)
		{
			layerView.isSelected(true);
		}
	}

	/// <summary>
	/// Sets the selected state of the JCheckBox. </summary>
	/// <param name="selected"> the state to set the JCheckBox to </param>
	public virtual void setCheckBoxSelected(bool selected)
	{
		__check.setSelected(selected);
	}

	}

}