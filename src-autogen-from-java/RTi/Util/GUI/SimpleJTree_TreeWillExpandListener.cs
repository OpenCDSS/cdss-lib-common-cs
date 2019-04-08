// SimpleJTree_TreeWillExpandListener - class that allows control over whether JTrees can expand or collapse

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
// SimpleJTree_TreeWillExpandListener - Class that allows control over
// whether JTrees can expand or collapse.
//-----------------------------------------------------------------------------
// Copyright: See the COPYRIGHT file.
//-----------------------------------------------------------------------------
// History: 
//
// 2003-05-01	J. Thomas Sapienza, RTi	* Initial version
//					* Javadoc'd.
// 2004-07-06	JTS, RTi		* Added setTree().
//					* Changed expansion code to notify 
//					  listeners of the node that is 
//					  expanding.
// 2005-04-26	JTS, RTi		Added finalize().
//-----------------------------------------------------------------------------

namespace RTi.Util.GUI
{


	/// <summary>
	/// This class is used when it is not desired that the SimpleJTree be able
	/// to be collapsed.  It overrides the default behavior in the SimpleJTree's
	/// oroginal TreeWillExpandListener such that collapsing and expansion of the
	/// tree can be disabled.<para>
	/// The default behavior is that expansion and collapsing are both allowed.
	/// </para>
	/// </summary>
	public class SimpleJTree_TreeWillExpandListener : TreeWillExpandListener
	{

	/// <summary>
	/// Determines whether collapsing is allowed in the tree or not.
	/// </summary>
	private bool __collapseAllowed = true;

	/// <summary>
	/// Determines whether expanding is allowed in the tree or not.
	/// </summary>
	private bool __expandAllowed = true;

	/// <summary>
	/// The SimpleJTree this listener is operating on.
	/// </summary>
	private SimpleJTree __tree = null;

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~SimpleJTree_TreeWillExpandListener()
	{
		__tree = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns true if the tree can be collapsed, false otherwise. </summary>
	/// <returns> true if the tree can be collapsed, false otherwise. </returns>
	public virtual bool isCollapseAllowed()
	{
		return __collapseAllowed;
	}

	/// <summary>
	/// Returns true if the tree can be expanded, false otherwise. </summary>
	/// <returns> true if the tree can be expanded, false otherwise. </returns>
	public virtual bool isExpandAllowed()
	{
		return __expandAllowed;
	}

	/// <summary>
	/// Sets whether the tree can be collapsed or not. </summary>
	/// <param name="collapseAllowed"> if true, the tree can be collapsed.  If false, it 
	/// can not. </param>
	public virtual void setCollapseAllowed(bool collapseAllowed)
	{
		__collapseAllowed = collapseAllowed;
	}

	/// <summary>
	/// Sets whether the tree can be expanded or not. </summary>
	/// <param name="expandAllowed"> if true, the tree can be expanded.  If false, it can not. </param>
	public virtual void setExpandAllowed(bool expandAllowed)
	{
		__expandAllowed = expandAllowed;
	}

	/// <summary>
	/// Sets the tree this listener is operating on. </summary>
	/// <param name="tree"> the SimpleJTree this node is listening for. </param>
	public virtual void setTree(SimpleJTree tree)
	{
		__tree = tree;
	}

	/// <summary>
	/// Invoked whenever a node in the tree is about to be collapsed (overridden 
	/// from TreeWillExpandListener).  Throws an exception if the tree cannot be
	/// collapsed (this exception doesn't ever show up to the user; it is used by
	/// the JTree to determine if it can collapse or not). </summary>
	/// <param name="event"> event that happened. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void treeWillCollapse(javax.swing.event.TreeExpansionEvent event) throws javax.swing.tree.ExpandVetoException
	public virtual void treeWillCollapse(TreeExpansionEvent @event)
	{
		if (!__collapseAllowed)
		{
			throw new ExpandVetoException(@event, "Cannot collapse this tree");
		}
	}

	/// <summary>
	/// Invoked whenever a node in the tree is about to be expanded (overridden 
	/// from TreeWillExpandListener).  Throws an exception if the tree cannot be
	/// collapsed (this exception doesn't ever show up to the user; it is used by
	/// the JTree to determine if it can expand or not). </summary>
	/// <param name="event"> event that happened. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void treeWillExpand(javax.swing.event.TreeExpansionEvent event) throws javax.swing.tree.ExpandVetoException
	public virtual void treeWillExpand(TreeExpansionEvent @event)
	{
		if (!__expandAllowed)
		{
			throw new ExpandVetoException(@event, "Cannot expand this tree");
		}
		System.Collections.IList v = __tree.getListeners();
		if (v == null)
		{
			return;
		}

		SimpleJTree_Node node = (SimpleJTree_Node)((@event.getPath()).getLastPathComponent());

		int size = v.Count;
		for (int i = 0; i < size; i++)
		{
			((SimpleJTree_Listener)v[i]).nodeExpanding(node);
		}
	}

	}

}