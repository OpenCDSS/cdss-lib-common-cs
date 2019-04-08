using System;

// SimpleJTree_Node - class that makes up each node of a SimpleJTree

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
// SimpleJTree_Node - The class that makes up each node of a SimpleJTree,
// able to store either normal JTree text or a Component, and with other
// utility functions that make it easie to use than normal nodes.
//-----------------------------------------------------------------------------
// Copyright: See the COPYRIGHT file.
//-----------------------------------------------------------------------------
// History: 
//
// 2003-04-30	J. Thomas Sapienza, RTI	Initial version
// 2003-05-01	JTS, RTi		* Initial version complete
//					* Javadoc'd.
// 2003-05-13	JTS, RTi		* Added code so that nodes can now
//					  store data.
// 2003-05-27	JTS, RTi		Added equals().
// 2005-04-26	JTS, RTi		Added finalize().
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//-----------------------------------------------------------------------------

namespace RTi.Util.GUI
{


	/// <summary>
	/// This is a specialized node used by SimpleJTree's that overrides 
	/// DefaultMutableTreeNode and can be used inside of JTrees.  SimpleJTree_Nodes
	/// are used instead of DefaultMutableTreeNodes because these nodes keep 
	/// track of additional data that comes in very handy, such as:<ul>
	/// <li>The Name associated with the node</li>
	/// <li>Whether the node contains a component or text<li>
	/// </ul>
	/// <para>
	/// The most useful data is the name, because if names are used then nodes can
	/// be programatically referred to in the tree by a pre-determined name.  
	/// Instead of finding a node by manually recursing through a tree or by
	/// keeping a reference to a specific node, the node can be referred to be name, instead.
	/// </para>
	/// <para>
	/// <b>Note:</b> This class is ready-to-go for insertion in a SimpleJTree, and 
	/// applications can just use this class, but at the same time this class
	/// is easily extensible for more specialized purposes.  The developer should
	/// choose which way to go.  For an example of a specialized version, see
	/// RTi.GIS.GeoView.GeoViewLegendJTree_Node.
	/// </para>
	/// </summary>
	public class SimpleJTree_Node : DefaultMutableTreeNode, ICloneable
	{

	/// <summary>
	/// Whether the node contains a component (true) or just text (true).
	/// </summary>
	private bool __containsComponent = false;

	/// <summary>
	/// Whether the node has been marked for future deletion or not.
	/// </summary>
	private bool __delete = false;

	/// <summary>
	/// Whether the node should be visible or not.
	/// </summary>
	private bool __visible = true;

	/// <summary>
	/// The Icon associated with the text in the node.
	/// </summary>
	private Icon __icon = null;

	/// <summary>
	/// The node's name.
	/// </summary>
	private string __name = null;

	/// <summary>
	/// The text held in the node.
	/// </summary>
	private string __text = null;

	/// <summary>
	/// Optional data of any type that can be held in the node.
	/// </summary>
	private object __data = null;

	/// <summary>
	/// Constructor.  Creates a node with the given text and a name the same as the text </summary>
	/// <param name="text"> the text to display in the node </param>
	public SimpleJTree_Node(string text) : base(text)
	{
		__containsComponent = false;
		__text = text;
		initialize(text);
	}

	/// <summary>
	/// Constructor.  Creates a node that holds the given object and has the given name. </summary>
	/// <param name="userObject"> the Component to store in this node. </param>
	/// <param name="name"> the name of this node. </param>
	public SimpleJTree_Node(Component userObject, string name) : base(userObject)
	{
		__containsComponent = true;
		initialize(name);
	}

	/// <summary>
	/// Constructor.  Creates a node that holds the given text and has the given name. </summary>
	/// <param name="text"> the text to hold in this node. </param>
	/// <param name="name"> the name of this node. </param>
	public SimpleJTree_Node(string text, string name) : base(text)
	{
		__text = text;
		__containsComponent = false;
		initialize(name);
	}

	/// <summary>
	/// Constructor.  Creates a node that holds the given text and uses the given Icon 
	/// and has the given name. </summary>
	/// <param name="text"> the text to hold in this node. </param>
	/// <param name="icon"> the icon to display in this node. </param>
	/// <param name="name"> the name of this node. </param>
	public SimpleJTree_Node(string text, Icon icon, string name) : base(text)
	{
		__text = text;
		__icon = icon;
		__containsComponent = false;
		initialize(name);
	}

	/// <summary>
	/// Clones this node and returns a copy with the identical settings.  If the 
	/// node being cloned contains a Component, that Component is <b>NOT</b> cloned,
	/// too.  The cloned node will contain a reference to the same Component held in
	/// the original node.  The data stored in the cloned object is not a clone
	/// of the original data: a reference is made to the same data object as in the original. </summary>
	/// <returns> a cloned copy of this node. </returns>
	public virtual object clone()
	{
		SimpleJTree_Node cloned = null;
		if (__containsComponent)
		{
			cloned = new SimpleJTree_Node((Component)getUserObject(), __name);
		}
		else
		{
			cloned = new SimpleJTree_Node(__text, __icon, __name);
		}
		cloned.setData(__data);
		cloned.markForDeletion(__delete);
		cloned.markVisible(__visible);

		return cloned;
	}

	/// <summary>
	/// Returns true if this node contains a component, otherwise false. </summary>
	/// <returns> true if this node contains a component, otherwise false. </returns>
	public virtual bool containsComponent()
	{
		return __containsComponent;
	}

	/// <summary>
	/// Returns true if the nodes are equal.  This is equivalent to String's equals() 
	/// method, which is different from doing String comparisons with the == operator.
	/// <para>
	/// If this node (not the passed-in node) contains a component then only the names
	/// of the nodes are checked to see if they match.
	/// </para>
	/// <para>
	/// If this node (not the passed-in node) does not contain a component, then the 
	/// name of the node, the node's icon, and the nodes text are all checked to see if they are equal.
	/// </para>
	/// </summary>
	/// <param name="node"> the node to compare against. </param>
	/// <returns> true if the nodes are equal. </returns>
	public virtual bool Equals(SimpleJTree_Node node)
	{
		if (__containsComponent)
		{
			if (!node.containsComponent())
			{
				return false;
			}
			if (!(__name.Equals(node.getName())))
			{
				return false;
			}
		}
		else
		{
			if (node.containsComponent())
			{
				return false;
			}
			if (!(__name.Equals(node.getName())))
			{
				return false;
			}
			if (__icon == null)
			{
				if (node.getIcon() != null)
				{
					return false;
				}
			}
			else
			{
				if (__icon != node.getIcon())
				{
					return false;
				}
			}
			if (string.ReferenceEquals(__text, null))
			{
				if (!string.ReferenceEquals(node.getText(), null))
				{
					return false;
				}
			}
			else
			{
				if ((!__text.Equals(node.getText())))
				{
					return false;
				}
			}
		}

		return true;
	}

	/// <summary>
	/// Returns the data stored in this node. </summary>
	/// <returns> the data stored in this node. </returns>
	public virtual object getData()
	{
		return __data;
	}

	/// <summary>
	/// Returns the Icon stored in this node. </summary>
	/// <returns> the Icon stored in this node. </returns>
	public virtual Icon getIcon()
	{
		return __icon;
	}

	/// <summary>
	/// Return the name of this node. </summary>
	/// <returns> the name of this node. </returns>
	public virtual string getName()
	{
		return __name;
	}

	/// <summary>
	/// Returns the value stored in the superclass of this node.  This is used by 
	/// the SimpleJTree_CellRenderer to determine the value of the node <i>after</i> editing has finished. </summary>
	/// <returns> the value stored in the superclass of this node. </returns>
	public virtual string getSuperString()
	{
		return base.ToString();
	}

	// TODO SAM 2015-12-13 Why is this here?  Doesn't the base class have already for node labels
	/// <summary>
	/// Return the text of this node (which is what is stored in the node if a Component is not stored in it). </summary>
	/// <returns> the text of this node. </returns>
	public virtual string getText()
	{
		return __text;
	}

	/// <summary>
	/// Initialization method common to all constructors. </summary>
	/// <param name="name"> the name of the node.  If name is null, it will be created from
	/// the <tt>toString()</tt> of this node, but that is not recommended as the
	/// node will then be hard to refer to by name. </param>
	private void initialize(string name)
	{
		if (string.ReferenceEquals(name, null))
		{
			__name = ToString();
		}
		else
		{
			__name = name;
		}
	}

	/// <summary>
	/// Returns whether this node is visible or not. </summary>
	/// <returns> whether this node is visible or not. </returns>
	public virtual bool isVisible()
	{
		return __visible;
	}

	/// <summary>
	/// Sets whether this node should be deleted in the future or not. </summary>
	/// <param name="markDelete"> whether this node should be deleted in the future (true) or not (false). </param>
	public virtual void markForDeletion(bool markDelete)
	{
		__delete = markDelete;
	}

	/// <summary>
	/// Sets whether this node should be made visible in the future or not. </summary>
	/// <param name="visible"> whether this node should be made visible in the future (true) or not (false). </param>
	public virtual void markVisible(bool visible)
	{
		__visible = visible;
	}

	/// <summary>
	/// Sets the component to store in this node. </summary>
	/// <param name="component"> the Component to store in this node. </param>
	public virtual void setComponent(Component component)
	{
		setUserObject(component);
	}

	/// <summary>
	/// Sets whether this node contains a component or not. </summary>
	/// <param name="containsComponent"> whether this node contains a component (true) or not (false). </param>
	public virtual void setComponent(bool containsComponent)
	{
		__containsComponent = containsComponent;
	}

	/// <summary>
	/// Sets the data stored in this node. </summary>
	/// <param name="data"> the data to store in this node. </param>
	public virtual void setData(object data)
	{
		__data = data;
	}

	/// <summary>
	/// Sets the Icon used if this displays text. </summary>
	/// <param name="icon"> the icon to show if this displays text.  If null, no icon will be shown. </param>
	public virtual void setIcon(Icon icon)
	{
		__icon = icon;
	}

	/// <summary>
	/// Sets the name of this node. </summary>
	/// <param name="name"> the value to set the node name to </param>
	public virtual void setName(string name)
	{
		__name = name;
	}

	/// <summary>
	/// Sets the text stored in this node. </summary>
	/// <param name="text"> the text to store in this node. </param>
	public virtual void setText(string text)
	{
		__text = text;
	}

	/// <summary>
	/// Returns whether this node should be deleted sometime in the future. </summary>
	/// <returns> whether this node should be deleted sometime in the future (true) or not (false). </returns>
	public virtual bool shouldBeDeleted()
	{
		return __delete;
	}

	/// <summary>
	/// Returns a String representation of the node -- with information from the super
	/// object too, if available.  The returned string will be:<br>
	/// SimpleJTree_Node Name: [Name as returned by getName()]<br>
	///  [result of super.toString()]<br> </summary>
	/// <returns> a String representation of the node. </returns>
	public override string ToString()
	{
		return "SimpleJTree_Node Name: " + __name + " " + base.ToString();
	}

	}

}