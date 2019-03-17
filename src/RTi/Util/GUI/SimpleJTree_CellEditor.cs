// SimpleJTree_CellEditor - class to control editing of Components and regular JTree text values in a SimpleJTree

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
// SimpleJTree_CellEditor - Class to control editing of both Components
// and regular JTree text values in a SimpleJTree
//-----------------------------------------------------------------------------
// Copyright: See the COPYRIGHT file.
//-----------------------------------------------------------------------------
// History: 
//
// 2003-04-30	J. Thomas Sapienza, RTI	Initial version
// 2003-05-01	JTS, RTi		* Initial version complete
//					* Javadoc'd.
// 2005-04-26	JTS, RTi		Added finalize().
//-----------------------------------------------------------------------------

namespace RTi.Util.GUI
{




	/// <summary>
	/// This is the cell editor used by the SimpleJTree to allow Components to be
	/// shown and clicked on but at the same time to allow the standard JTree 
	/// functionality whereby text can be shown and edited.
	/// </summary>
	public class SimpleJTree_CellEditor : DefaultTreeCellEditor
	{

	/// <summary>
	/// Whether the text stored in the tree is editable or not.
	/// </summary>
	private bool __editable = true;

	/// <summary>
	/// A reference to the JTree which is using this editor.
	/// </summary>
	private JTree __tree;

	/// <summary>
	/// Constructor.
	/// Builds a cell editor for the given tree and the given renderer. </summary>
	/// <param name="tree"> the Tree that this will be a cell editor for. </param>
	/// <param name="renderer"> the cell renderer that will use this cell editor. </param>
	public SimpleJTree_CellEditor(JTree tree, SimpleJTree_CellRenderer renderer) : base(tree, renderer, null)
	{
		__tree = tree;
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~SimpleJTree_CellEditor()
	{
		__tree = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the value currently being edited (overridden from 
	/// DefaultTreeCellEditor). </summary>
	/// <returns> the value currently being edited. </returns>
	public virtual object getCellEditorValue()
	{
		return base.getCellEditorValue();
	}

	/// <summary>
	/// Configures the editor.  Passed onto the <tt>realEditor</tt>.  (Overridden
	/// from DefaultTreeCellEditor).  This determines whether the node that
	/// was clicked on contains a Component or not.  If it has a Component, then
	/// the Component is rendered as usual and should work normally. If the
	/// node has text (default JTree functionality), then the default 
	/// cell editor functionality is performed.<para>
	/// <b>Note:</b> All the components that might need to be rendered properly 
	/// need to be listed in here.  Those that are currently supported are:<ul>
	/// <li>JButton</li>
	/// <li>JCheckBox</li>
	/// <li>JLabel</li>
	/// <li>JPanel</li>
	/// </ul>
	/// Any others that do not match the above are turned into Strings and placed in
	/// a JLabel.
	/// </para>
	/// </summary>
	/// <param name="tree"> the JTree that is asking the editor to edit; this parameter
	/// can be null. </param>
	/// <param name="value"> the value of the cell to be edited. </param>
	/// <param name="selected"> true if the cell is to be rendererd with selection
	/// highlighting. </param>
	/// <param name="expanded"> true if the node is expanded. </param>
	/// <param name="leaf"> true if the node is a leaf node. </param>
	/// <param name="row"> the row index of the node being edited. </param>
	/// <returns> the Component for editing. </returns>
	public virtual Component getTreeCellEditorComponent(JTree tree, object value, bool selected, bool expanded, bool leaf, int row)
	{
		SimpleJTree_Node temp = (SimpleJTree_Node) value;

		if (temp.containsComponent())
		{
			object o = temp.getUserObject();
			if (o is JCheckBox)
			{
				JCheckBox temp2 = (JCheckBox)o;
				return temp2;
			}
			else if (o is JLabel)
			{
				JLabel temp2 = (JLabel)o;
				return temp2;
			}
			else if (o is JButton)
			{
				JButton temp2 = (JButton)o;
				return temp2;
			}
			else if (o is JPanel)
			{
				JPanel temp2 = (JPanel)o;
				return temp2;
			}
			else if (o is JComponent)
			{
				JComponent temp2 = (JComponent)o;
				return temp2;
			}
			else
			{
				return new JLabel(o.ToString());
			}
		}
		else
		{
			Component c = base.getTreeCellEditorComponent(tree, temp.getText(), selected, expanded, leaf, row);
			return c;
		}
	}

	/// <summary>
	/// If the <tt>realEditor</tt> returns true to this message, 
	/// <tt>prepareForEditiing</tt> is messaged and true is returned (overridden
	/// from DefaultTreeCellEditor).  This method figure out what in the tree is
	/// being clicked on and if the object being clicked on is a Component then
	/// the click is passed through to the component (so it functions normally).  
	/// Otherwise, if the tree has been set to allow text editing it might open
	/// up an editor on a text field. </summary>
	/// <param name="evt"> the event the editor should use to consider whether to begin
	/// editing or not. </param>
	/// <returns> true if editing can start. </returns>
	public virtual bool isCellEditable(EventObject evt)
	{
		if (evt is MouseEvent)
		{
			MouseEvent mevt = (MouseEvent) evt;

			// Figure out which node in the tree was clicked on by
			// 1 - getting the X, Y coordinate of the mouse click
			// 2 - seeing which tree row that corresponds to
			// 3 - finding out the tree path to that row
			// 4 - finding which node is stored there
			// (a lot of work for just that)
			int row = __tree.getRowForLocation(mevt.getX(), mevt.getY());
			TreePath path = __tree.getPathForRow(row);

			if (path == null)
			{
				return false;
			}

			SimpleJTree_Node node = (SimpleJTree_Node)path.getLastPathComponent();

			// If the node has a component, always return true because
			// that allows the component to function properly.
			if (node.containsComponent())
			{
				return true;
			}
			// If there have been two clicks and the node doesn't have
			// a component in it (and right now nodes are assumed to 
			// either store text or a component), then editing can 
			// start 
			else if (mevt.getClickCount() == 2)
			{
				if (__editable)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		return false;
	}

	/// <summary>
	/// Sets whether the cell editor should allow text fields (the default
	/// JTree functionality) to be edited or not. </summary>
	/// <param name="editable"> if true, then the text fields can be edited. </param>
	public virtual void setEditable(bool editable)
	{
		__editable = editable;
	}

	/// <summary>
	/// If the <tt>realEditor</tt> will allow editing to stop, the <tt>realEditor</tt>
	/// is removed and true is returned, otherwise false is returned (overridden
	/// from DefaultTreeCellEditor). </summary>
	/// <returns> true if editing was stopped; false otherwise. </returns>
	public virtual bool stopCellEditing()
	{
		return true;
	}

	}

}