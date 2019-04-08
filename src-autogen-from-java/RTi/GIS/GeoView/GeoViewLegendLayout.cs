using System.Collections.Generic;

// GeoViewLegendLayout - layout manager that controls how the legend is drawn on the map

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
// GeoViewLegendLayout - a layout manager that controls how the legend is
//	drawn on the map, also doing linking so the legend can be managed
// 	from the properties JFrame
//-----------------------------------------------------------------------------
// Copyright:  See the COPYRIGHT file.
//-----------------------------------------------------------------------------
// History:
// 2004-10-18	J. Thomas Sapienza, RTi	Initial version.
// 2005-04-27	JTS, RTi		Added finalize().
//-----------------------------------------------------------------------------

namespace RTi.GIS.GeoView
{

	using SimpleJTree_Node = RTi.Util.GUI.SimpleJTree_Node;

	/// <summary>
	/// A layout manager for the GeoViewLegend that controls how the legend is drawn
	/// on the map.  This class also links the legend to the properties JFrame for
	/// the GeoView so it can be changed there.
	/// TODO (JTS - 2006-05-23) How is this class used?
	/// </summary>
	public class GeoViewLegendLayout
	{

	/// <summary>
	/// Location constants that specify where the legend is placed on the GeoView.
	/// </summary>
	public const int NORTHWEST = 0, NORTHEAST = 2, SOUTHEAST = 4, SOUTHWEST = 6;

	/// <summary>
	/// The number of node layers added to the legend.
	/// </summary>
	private int count = 0;

	/// <summary>
	/// The position of the legend on the GeoView (one of NORTHWEST, NORTHEAST,
	/// SOUTHWEST, SOUTHEAST).
	/// </summary>
	private int __position = 0;

	/// <summary>
	/// The title of the legend.
	/// </summary>
	private string __title = null;

	/// <summary>
	/// The checkboxes that were added to the legend.  The items in this Vector 
	/// correspond to the items in the other Vectors at the same position.
	/// </summary>
	public IList<JCheckBox> __checkboxes = new List<JCheckBox>();

	/// <summary>
	/// The nodes that were added to the legend.  The items in this Vector 
	/// correspond to the items in the other Vectors at the same position.
	/// </summary>
	public IList<SimpleJTree_Node> __nodes = new List<SimpleJTree_Node>();

	/// <summary>
	/// The layers that were added to the legend.  The items in this Vector 
	/// correspond to the items in the other Vectors at the same position.
	/// </summary>
	public IList<GeoLayerView> __layers = new List<GeoLayerView>();

	/// <summary>
	/// A Vector of Booleans that specify whether the layer in the layers Vector
	/// at the same position is visible or not.  The items in this Vector 
	/// correspond to the items in the other Vectors at the same position.
	/// </summary>
	public IList<bool> __visibles = new List<bool>();

	/// <summary>
	/// Adds an item to the legend. </summary>
	/// <param name="node"> the node in the legend JTree that corresponds to the item being
	/// added to the legend. </param>
	/// <param name="layer"> the layer on the GeoView corresponding to the item being added
	/// to the legend. </param>
	/// <param name="checkbox"> the checkbox in the legend JTree that corresponds to the item
	/// being added. </param>
	/// <param name="visible"> whether the item is visible. </param>
	public virtual void addNodeLayerCheckBox(SimpleJTree_Node node, GeoLayerView layer, JCheckBox checkbox, bool visible)
	{
		__nodes.Add(node);
		__layers.Add(layer);
		__checkboxes.Add(checkbox);
		__visibles.Add(new bool?(visible));
		count++;
	}

	/// <summary>
	/// Clears everything from the legend.
	/// </summary>
	public virtual void empty()
	{
		__nodes = new List<SimpleJTree_Node>();
		__layers = new List<GeoLayerView>();
		__visibles = new List<bool>();
		__checkboxes = new List<JCheckBox>();
		count = 0;
	}

	/// <summary>
	/// Cleans up member variables. </summary>
	/// <exception cref="Throwable"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~GeoViewLegendLayout()
	{
		__checkboxes = null;
		__nodes = null;
		__layers = null;
		__visibles = null;
		__title = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the position of the given checkbox in the legend, or -1 if the 
	/// checkbox cannot be found. </summary>
	/// <returns> the position of the given checkbox in the legen, or -1 if the 
	/// checkbox cannot be found. </returns>
	public virtual int findCheckBox(JCheckBox checkbox)
	{
		JCheckBox tempCheckbox = null;
		for (int i = 0; i < count; i++)
		{
			tempCheckbox = __checkboxes[i];
			if (tempCheckbox == checkbox)
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// Returns the position of the given layer in the legend, or -1 if the 
	/// layer cannot be found. </summary>
	/// <returns> the position of the given layer in the legend, or -1 if the 
	/// layer cannot be found. </returns>
	public virtual int findLayer(GeoLayerView layer)
	{
		GeoLayerView tempLayer = null;
		for (int i = 0; i < count; i++)
		{
			tempLayer = __layers[i];
			if (tempLayer == layer)
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// Returns the position of the given node in the legend, or -1 if the 
	/// node cannot be found. </summary>
	/// <returns> the position of the given node in the legend, or -1 if the 
	/// node cannot be found. </returns>
	public virtual int findNode(SimpleJTree_Node node)
	{
		SimpleJTree_Node tempNode = null;
		for (int i = 0; i < count; i++)
		{
			tempNode = __nodes[i];
			if (tempNode == node)
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// Returns the count of items in this legend. </summary>
	/// <returns> the count of items in this legend. </returns>
	public virtual int getCount()
	{
		return count;
	}

	/// <summary>
	/// Returns the title of the legend. </summary>
	/// <returns> the title of the legend. </returns>
	public virtual string getTitle()
	{
		return __title;
	}

	/// <summary>
	/// Returns the position of the legend on the GeoView. </summary>
	/// <returns> the position of the legend on the GeoView. </returns>
	public virtual int getPosition()
	{
		return __position;
	}

	/// <summary>
	/// Returns whether the given layer is visible. </summary>
	/// <returns> whether the given layer is visible. </returns>
	public virtual bool isLayerVisible(GeoLayerView glv)
	{
		if (__layers.Count == 0)
		{
			return false;
		}

		int num = findLayer(glv);

		if (num == -1)
		{
			return false;
		}

		GeoLayerView layer = __layers[num];
		return layer.isVisible();
	}

	/// <summary>
	/// Returns whether the given layer's legend is visible. </summary>
	/// <param name="glv"> the layer to check. </param>
	/// <returns> whether the given layer's legend is visible. </returns>
	public virtual bool isLayerLegendVisible(GeoLayerView glv)
	{
		if (__layers.Count == 0)
		{
			return false;
		}

		int num = findLayer(glv);

		if (num == -1)
		{
			return false;
		}

		return __visibles[num];
	}

	/// <summary>
	/// Returns whether the given node's legend is visible. </summary>
	/// <param name="node"> the node to check. </param>
	/// <returns> whether the given node's legend is visible. </returns>
	public virtual bool isNodeLegendVisible(SimpleJTree_Node node)
	{
		if (__nodes.Count == 0)
		{
			return false;
		}

		int num = findNode(node);

		if (num == -1)
		{
			return false;
		}

		return __visibles[num];
	}

	/// <summary>
	/// Removes the given layer from the legend. </summary>
	/// <param name="layer"> the layer to remove. </param>
	public virtual void removeLayer(GeoLayerView layer)
	{
		int num = findLayer(layer);
		if (num == -1)
		{
			return;
		}
		__nodes.RemoveAt(num);
		__layers.RemoveAt(num);
		__visibles.RemoveAt(num);
		__checkboxes.RemoveAt(num);
		count--;
	}

	/// <summary>
	/// Removes the given node from the legend. </summary>
	/// <param name="node"> the node to remove. </param>
	public virtual void removeNode(SimpleJTree_Node node)
	{
		int num = findNode(node);
		if (num == -1)
		{
			return;
		}
		__nodes.RemoveAt(num);
		__layers.RemoveAt(num);
		__visibles.RemoveAt(num);
		__checkboxes.RemoveAt(num);
		count--;
	}

	/// <summary>
	/// Sets the checkbox at the given legend position. </summary>
	/// <param name="num"> the position at which to set the checkbox. </param>
	/// <param name="checkbox"> the checkbox to set. </param>
	public virtual void setCheckBox(int num, JCheckBox checkbox)
	{
		__checkboxes[num] = checkbox;
	}

	/// <summary>
	/// Sets the layer legend at the given position visible or not. </summary>
	/// <param name="num"> the position at which to set the layer visible. </param>
	/// <param name="visible"> whether to set the layer visible or invisible. </param>
	public virtual void setLayerLegendVisible(int num, bool visible)
	{
		__visibles[num] = new bool?(visible);
	}

	/// <summary>
	/// Sets the position of the legend. </summary>
	/// <param name="pos"> the position of the legend (NORTHWEST, NORTHEAST, SOUTHWEST,
	/// SOUTHEAST). </param>
	public virtual void setPosition(int pos)
	{
		__position = pos;
	}

	/// <summary>
	/// Sets the title of the legend. </summary>
	/// <param name="title"> the title to set. </param>
	public virtual void setTitle(string title)
	{
		__title = title;
	}

	}

}