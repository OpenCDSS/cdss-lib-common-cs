﻿using System;
using System.Collections.Generic;

// GeoViewPanel - panel class to manage GeoView legend

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
// Panel class to manage GeoView legend
// ----------------------------------------------------------------------------
// History:
//
// 2001-10-08	Steven A. Malers, RTi	Add invalidate so that the entire legend
//					can be invalidated, causing the
//					individual panels to redraw.
// 2001-10-12	SAM, RTi		Handle events to enable/disable
//					properties button.
// 2001-10-17	SAM, RTi		Set unused variables to null to help
//					garbage collection.
// 2001-11-27	SAM, RTi		Fix bug where removeAllLayerViews()
//					was not having a visible effect
//					(__legendVector was not getting
//					cleared).  Also fix bug where the
//					layer view legends were being removed
//					from this main panel, and not the panel
//					in the scroll pane!
// 2001-12-04	SAM, RTi		Update to use Swing.
// ----------------------------------------------------------------------------
// 2003-05-06	J. Thomas Sapienza, RTi	Brought in line with the non-Swing
//					version of the code
// 2003-05-09	JTS, RTi		Changed layout of the panel that holds
//					the buttons to be a GridBagLayout so
//					the button sizing works properly.
// 2003-05-13	JTS, RTi		* Private variables now meet RTi's
//					  standard naming convention.
//					* Started work implementing the legend
//					  with a SimpleJTree.
//					* Added a mouse listener so that the 
//					  tree can have a popup menu.
//					* Removed checkState();
// ----------------------------------------------------------------------------
// 2003-05-14	JTS, RTi		* Initial version from 
//					  GeoViewLegendJPanel
//					* Lots of javadoc'ing.
//					* Uncommented and filled in all methods.
// 2003-05-15	JTS, RTi		Can now remove more than one layer at
//					at a time.
// 2003-05-21	JTS, RTi		Added a popup to the project nodes to
//					open a new project.
// 2003-08-21	JTS, RTi		Layers are now added to the legend tree
//					in the correct order.
// 2003-08-22	JTS, RTi		Popup menu for layer nodes now has
//					an entry for viewing the attributes 
//					table.
// 2004-07-29	JTS, RTi		* Fixed problem with drawing the node
//					  information for scaled symbols.
//					* Attribute tables can now be displayed
//					  for layers that had their tables
//					  generated in-memory.
//					* Nodes rows are now sized individually
//					  because of setRowHeight(0);
// 2004-08-03	JTS, RTi		Added a popup menu that is used for
//					animated layers.
// 2004-09-16	JTS, RTi		Changed the call of isPopupTrigger to
//					use the one in the JPopupMenu.
// 2004-10-14	JTS, RTi		* Added isLayerVisible().
//					* Corrected bug in getLayerNames() that
//					  was resulting in nothing being
//					  returned.
// 2004-10-22	JTS, RTi		When viewing an attribute table now,
//					the GUI defers to showing the table
//					that resides in memory, rather than
//					trying to re-read the original from
//					a file.
// 2004-10-27	JTS, RTi		Added setLayerVisible().
// 2004-11-11	JTS, RTi		In legends with class breaks, the
//					class break text background is now
//					set to match the color of the tree.
// 2005-04-27	JTS, RTi		Added all member variables to finalize()
// 2006-03-06	JTS, RTi		Changed text for showing the animation
//					control from "Show Animation Control
//					GUI".
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.GIS.GeoView
{




	using GRLegend = RTi.GR.GRLegend;
	using GRScaledClassificationSymbol = RTi.GR.GRScaledClassificationSymbol;
	using GRSymbol = RTi.GR.GRSymbol;

	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using ResponseJDialog = RTi.Util.GUI.ResponseJDialog;

	using SimpleJTree = RTi.Util.GUI.SimpleJTree;
	using SimpleJTree_Node = RTi.Util.GUI.SimpleJTree_Node;

	using Message = RTi.Util.Message.Message;

	using StringUtil = RTi.Util.String.StringUtil;

	using DataTable = RTi.Util.Table.DataTable;
	using DataTable_JFrame = RTi.Util.Table.DataTable_JFrame;

	/// <summary>
	/// The GeoViewLegendJTree is a Tree that displays -- in ESRI-like fashion -- the layers available in a GeoView.  
	/// TODO (JTS - 2006-05-23) How to see this tree?  Is it automatic?
	/// </summary>
	public class GeoViewLegendJTree : SimpleJTree, ActionListener, ItemListener, MouseListener
	{

	/// <summary>
	/// Menu item labels.
	/// </summary>
	private readonly string __MENU_VIEW_ATTRIBUTES = "View Attribute Table", __MENU_SHOW_ANIMATION_CONTROL_GUI = "Show Animation Control";

	/// <summary>
	/// The GeoViewJPanel in which this tree appears.
	/// </summary>
	private GeoViewJPanel __theGeoViewJPanel = null;

	/// <summary>
	/// The root-most and visible node in the tree.  All the layer nodes are children
	/// of this node.  When this node is null, the tree is empty.  
	/// </summary>
	private GeoViewLegendJTree_Node __projectNode = null;

	/// <summary>
	/// The popup menu to be assigned to the project nodes.  It is displayed when a
	/// project node is right-clicked on in the tree.
	/// </summary>
	private JPopupMenu __projectNodePopup = null;

	/// <summary>
	/// The popup menu to be assigned to all the layer nodes except those that 
	/// are animated.  It is displayed when any non-animation layer node is right-clicked on in the tree.
	/// </summary>
	private JPopupMenu __layerPopup = null;

	/// <summary>
	/// The popup menu to be displayed when an animated layer is right-clicked on in the tree.
	/// </summary>
	private JPopupMenu __animatedPopup = null;

	/// <summary>
	/// The popup menu to be displayed when the tree is right-clicked on.
	/// </summary>
	private JPopupMenu __treePopup = null;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="gvp"> GeoViewJPanel to contain and manage the legend panel. </param>
	public GeoViewLegendJTree(GeoViewJPanel gvp) : base()
	{
		// initialize all the SimpleJTree settings.

		__theGeoViewJPanel = gvp;

		// These calls set up the SimpleJTree to display in a fashion similar to ESRI's Table of Contents
		setClosedIcon(null);
		setOpenIcon(null);
		setLeafIcon(null);
		setLineStyle(SimpleJTree.LINE_NONE);

		// Create the popup menu for the tree nodes.  This popup menu
		// has a "Properties" and a "Remove" menu item
		__layerPopup = new JPopupMenu();
		JMenuItem mi = new JMenuItem(__theGeoViewJPanel.REMOVE);
		mi.addActionListener(this);
		__layerPopup.add(mi);
		__layerPopup.addSeparator();
		mi = new JMenuItem(__theGeoViewJPanel.PROPERTIES);
		mi.addActionListener(this);
		__layerPopup.add(mi);
		mi = new JMenuItem(__MENU_VIEW_ATTRIBUTES);
		mi.addActionListener(this);
		__layerPopup.add(mi);

		// Create the popup for nodes of views that are animated.
		__animatedPopup = new JPopupMenu();
		mi = new JMenuItem(__theGeoViewJPanel.REMOVE);
		mi.addActionListener(this);
		__animatedPopup.add(mi);
		__animatedPopup.addSeparator();
		mi = new JMenuItem(__theGeoViewJPanel.PROPERTIES);
		mi.addActionListener(this);
		__animatedPopup.add(mi);
		mi = new JMenuItem(__MENU_VIEW_ATTRIBUTES);
		mi.addActionListener(this);
		__animatedPopup.add(mi);
		mi = new JMenuItem(__MENU_SHOW_ANIMATION_CONTROL_GUI);
		mi.addActionListener(this);
		__animatedPopup.add(mi);

		// Create the popup menu for the tree.  This has an "Add" menu item
		__treePopup = new JPopupMenu();
		mi = new JMenuItem(__theGeoViewJPanel.ADD);
		mi.addActionListener(this);
		__treePopup.add(mi);
		__treePopup = null;

		// Create the popup menu for project nodes.  This has an "Open Project" menu item
		__projectNodePopup = new JPopupMenu();
		mi = new JMenuItem(__theGeoViewJPanel.OPEN_GVP);
		mi.addActionListener(__theGeoViewJPanel);
		__projectNodePopup.add(mi);
		__projectNodePopup.addSeparator();

		mi = new JMenuItem(__theGeoViewJPanel.ADD);
		mi.addActionListener(this);
		__projectNodePopup.add(mi);
		mi = new JMenuItem(__theGeoViewJPanel.ADD_SUMMARY_LAYER_TO_GEOVIEW);
		mi.addActionListener(__theGeoViewJPanel);
		__projectNodePopup.add(mi);
		mi = new JMenuItem(__theGeoViewJPanel.SET_ATTRIBUTE_KEY);
		mi.addActionListener(__theGeoViewJPanel);
		__projectNodePopup.add(mi);
		__projectNodePopup.addSeparator();

		mi = new JMenuItem(__theGeoViewJPanel.SELECT_GEOVIEW_ITEM);
		mi.addActionListener(__theGeoViewJPanel);
		__projectNodePopup.add(mi);
		mi = new JMenuItem(__theGeoViewJPanel.GEOVIEW_ZOOM);
		mi.addActionListener(__theGeoViewJPanel);
		__projectNodePopup.add(mi);
		mi = new JMenuItem(__theGeoViewJPanel.GEOVIEW_ZOOM_OUT);
		mi.addActionListener(__theGeoViewJPanel);
		__projectNodePopup.add(mi);
		__projectNodePopup.addSeparator();

		mi = new JMenuItem(__theGeoViewJPanel.PRINT_GEOVIEW);
		mi.addActionListener(__theGeoViewJPanel);
		__projectNodePopup.add(mi);
		mi = new JMenuItem(__theGeoViewJPanel.SAVE_AS_IMAGE_MENU);
		mi.addActionListener(__theGeoViewJPanel);
		__projectNodePopup.add(mi);
		mi = new JMenuItem(__theGeoViewJPanel.SAVE_AS_SHAPEFILE_MENU);
		mi.addActionListener(__theGeoViewJPanel);
		__projectNodePopup.add(mi);

		__projectNodePopup.addSeparator();
		mi = new JMenuItem(__theGeoViewJPanel.GEOVIEW_PROPERTIES);
		mi.addActionListener(__theGeoViewJPanel);
		__projectNodePopup.add(mi);

		__treePopup = __projectNodePopup;

		addMouseListener(this);

		// Allow each row's height to be computed individually
		setRowHeight(0);
	}

	/// <summary>
	/// Handle action events from the popup menus. </summary>
	/// <param name="evt"> ActionEvent from popup menus </param>
	public virtual void actionPerformed(ActionEvent evt)
	{
		string command = evt.getActionCommand();

		if (command.Equals(__theGeoViewJPanel.ADD_LAYER_TO_GEOVIEW) || command.Equals(__theGeoViewJPanel.ADD))
		{
			__theGeoViewJPanel.addLayerView();
		}
		else if (command.Equals(__theGeoViewJPanel.REMOVE))
		{
			System.Collections.IList v = getSelectedLayerViews(false);
			if (v.Count == 0)
			{
				// this *shouldn't* happen, but ...
				return;
			}

			string plural = "";
			if (v.Count > 1)
			{
				plural = "s";
			}

			int response = (new ResponseJDialog(__theGeoViewJPanel.getParentJFrame(), "Remove layer" + plural + "?", "Are you sure you want to remove the selected layer" + plural + " from the view?", ResponseJDialog.YES | ResponseJDialog.NO)).response();
			if (response == ResponseJDialog.NO)
			{
				return;
			}

			for (int i = 0; i < v.Count; i++)
			{
				__theGeoViewJPanel.removeLayerView((GeoLayerView)v[i], true);
			}
		}
		else if (command.Equals(__theGeoViewJPanel.PROPERTIES))
		{
			GeoViewLegendJTree_Node node = (GeoViewLegendJTree_Node)getSelectedNode();
			GeoLayerView layerView = (GeoLayerView)node.getData();
			__theGeoViewJPanel.showLayerViewProperties(layerView);
		}
		else if (command.Equals(__MENU_SHOW_ANIMATION_CONTROL_GUI))
		{
			GeoViewLegendJTree_Node node = (GeoViewLegendJTree_Node)getSelectedNode();
			GeoLayerView layerView = (GeoLayerView)node.getData();
			layerView.getAnimationControlJFrame().setVisible(true);
		}
		else if (command.Equals(__MENU_VIEW_ATTRIBUTES))
		{
			GeoViewLegendJTree_Node node = (GeoViewLegendJTree_Node)getSelectedNode();
			GeoLayerView layerView = (GeoLayerView)node.getData();
			GRLegend legend = layerView.getLegend();
			string layerName = legend.getText();
			string filename = layerView.getLayer().getFileName();
			if (!string.ReferenceEquals(filename, null) && layerView.getLayer().getAttributeTable() == null)
			{
				int index = filename.LastIndexOf(".", StringComparison.Ordinal);
				if (index == -1)
				{
					Message.printStatus(1,"GeoViewPropertiesJFrame.actionPerformed", "Error trying to parse layer file name (" + filename + ") into the associated .dbf file");
					return;
				}
				string @base = filename.Substring(0, index + 1);
				string dbfFilename = @base + "dbf";

				try
				{
					new DataTable_JFrame("Attributes of " + layerName, dbfFilename);
				}
				catch (Exception e)
				{
					Message.printWarning(1, "GeoViewLegendJTree.actionPerformed", "Error opening DataTable_JFrame (" + e + ").");
					Message.printWarning(2, "GeoViewLegendJTree.actionPerformed",e);
				}
			}
			else
			{
				GeoLayer layer = layerView.getLayer();
				DataTable table = layer.getAttributeTable();
				try
				{
					new DataTable_JFrame("Attributes of " + layerName, table);
				}
				catch (Exception e)
				{
					Message.printWarning(1, "GeoViewLegendJTree.actionPerformed", "Error opening DataTable_JFrame");
					Message.printWarning(2, "GeoViewLegendJTree.actionPerformed",e);
				}
			}
		}
	}

	/// <summary>
	/// Add an item to the legend.  Make sure the GeoLayerView has already been added
	/// to the main GeoView.  Because the number of layer views to add may not initially
	/// be known, the legend panels are removed at each call and are then re-added so
	/// the highest numbered layer view is at the top of the legend panel. </summary>
	/// <param name="layerView"> GeoLayerView for which to draw the legend panel. </param>
	/// <param name="count"> of layers added (1 is first, displayed on bottom). </param>
	public virtual void addLayerView(GeoLayerView layerView, int count)
	{
		string routine = "addLayerView";
		string layerName = layerView.getLegend().getText();

		GeoViewLegendJTree_Node node = null;
		string layerLabel = " " + layerName; // Space helps with presentation
		if (!layerView.getLayer().isSourceAvailable())
		{
			// The data for the layer is not available so add a red ! to the front of the label.
			// TODO SAM 2010-12-15 HTML and JEditPane does weird things when the strings need to wrap
			//layerLabel = "<html><p style=\"color:red\">! " + layerName + "</p></html>";
			layerLabel = "** NO DATA ** " + layerName;
		}
		if (layerView.isAnimated())
		{
			node = new GeoViewLegendJTree_Node(layerLabel, layerName, this, __animatedPopup);
		}
		else
		{
			node = new GeoViewLegendJTree_Node(layerLabel, layerName, this, __layerPopup);
		}

		node.setCheckBoxSelected(layerView.isVisible());

		try
		{
			if (__projectNode == null)
			{
				__projectNode = new GeoViewLegendJTree_Node("GeoView Layers", "Project Node Name", this, __projectNodePopup);
				__projectNode.setCheckBoxSelected(true);
				addNode(__projectNode);
			}

			node.setData(layerView);

			addNode(node, __projectNode, 0);
			// Now draw the symbol(s)...
			// Get the number of symbols for the layer view.  First need to
			// determine the layer view number, which is stored in the "Number"
			// property for the layerview.  Currently this is supported only for
			// CLASSIFICATION_SINGLE and CLASSIFICATION_SCALED_SYMBOL.

			int nsymbol = layerView.getLegend().size();
			GRSymbol symbol = null;
			GeoLayerViewLegendJComponent[] layerCanvas = null;
			JLabel[] layerClassJLabel = null;
			int y = 0;
			Insets insets_none = new Insets(1, 1, 1, 1);
			for (int isym = 0; isym < nsymbol; isym++)
			{
				symbol = layerView.getLegend().getSymbol(isym);
				if (symbol.getClassificationType() == GRSymbol.CLASSIFICATION_SINGLE || symbol.getClassificationType() == GRSymbol.CLASSIFICATION_SCALED_TEACUP_SYMBOL)
				{
					if (isym == 0)
					{
						// For now assume that symbol types will not be mixed for a layer...
						layerCanvas = new GeoLayerViewLegendJComponent[nsymbol];
					}
					layerCanvas[isym] = new GeoLayerViewLegendJComponent(layerView, isym, 0);
					SimpleJTree_Node symNode = new SimpleJTree_Node((Component)layerCanvas[isym], "" + layerName + " symbol #" + isym);

					// This data is used when drawing the legend on 
					// the map later, so it's important that it be set.
					symNode.setData(layerCanvas[isym]);
					addNode(symNode, node);
				}
				else if (symbol.getClassificationType() == GRSymbol.CLASSIFICATION_SCALED_SYMBOL)
				{
					// This is currently enabled only for vertical signed
					// bars where the bar is centered vertically on the
					// point, positive values are drawn with the main
					// foreground color and negative values are drawn with
					// the secondary foreground color.
					JPanel panel = new JPanel();
					panel.setBackground(Color.white);
					panel.setLayout(new GridBagLayout());
					if (isym == 0)
					{
						// For now assume that symbol types will not be mixed for a layer...
						layerCanvas = new GeoLayerViewLegendJComponent[nsymbol];
						layerClassJLabel = new JLabel[nsymbol];
					}

					layerCanvas[isym] = new GeoLayerViewLegendJComponent(layerView, isym, 0);
					JGUIUtil.addComponent(panel, layerCanvas[isym], 1, ++y, 1, 1, 1, 1, insets_none, GridBagConstraints.NONE, GridBagConstraints.SOUTH);
					if (!symbol.getClassificationField().Equals(""))
					{
						// Get the maximum value for the symbol, which is used to scale the symbol...
						// TODO SAM 2009-07-02 - need to streamline this - store with symbol at creation?
						DataTable attribute_table = layerView.getLayer().getAttributeTable();
						int classification_field = -1;
						string cf = symbol.getClassificationField();
						if (attribute_table != null)
						{
							try
							{
								classification_field = attribute_table.getFieldIndex(cf);
							}
							catch (Exception)
							{
								// Just won't label below.
								classification_field = -1;
							}
						}

						if ((classification_field >= 0))
						{
							double symbol_max = ((GRScaledClassificationSymbol)symbol).getClassificationDataDisplayMax();
							// Do this to keep legend a reasonable width...
							if (cf.Length > 20)
							{
								cf = cf.Substring(0,20) + "...";
							}
							layerClassJLabel[isym] = new JLabel(cf + ", Max = " + StringUtil.formatString(symbol_max,"%.3f"));
						}
						else
						{
							if (cf.Length > 20)
							{
								cf = cf.Substring(0,20) + "...";
							}
							layerClassJLabel[isym] = new JLabel(cf);
						}
					}
					else
					{
						// Add a label with the field and maximum value...
						layerClassJLabel[isym] = new JLabel("");
					}
					JGUIUtil.addComponent(panel, layerClassJLabel[isym], 2, y, 1, 1, 1, 0, insets_none, GridBagConstraints.HORIZONTAL, GridBagConstraints.NORTH);
					SimpleJTree_Node symNode = new SimpleJTree_Node((Component)panel, "" + layerName + " symbol #" + isym);

					// The text that is drawn on the legend to label
					// the height of the bar.  Essential this is set!
					// This information cannot easily be pulled from the
					// tree, so it is just set here so that the legend 
					// will be labelled with "Max = XXX.XX", or such.
					layerCanvas[isym].setLegendText(layerClassJLabel[isym].getText());

					// This data is used when drawing the legend on 
					// the map later, so it's important that it be set
					symNode.setData(layerCanvas[isym]);
					addNode(symNode, node);
				}
				else
				{
					// Multiple legend items need to be drawn...
					int numclass = symbol.getNumberOfClassifications();
					layerCanvas = new GeoLayerViewLegendJComponent[numclass];
					layerClassJLabel = new JLabel[numclass];
					for (int i = 0; i < numclass; i++)
					{
						JPanel panel = new JPanel();
						panel.setLayout(new GridBagLayout());
						panel.setBackground(getBackground());
						layerCanvas[i] = new GeoLayerViewLegendJComponent(layerView, isym, i);
						JGUIUtil.addComponent(panel, layerCanvas[i], 1, ++y, 1, 1, 0, 0, insets_none, GridBagConstraints.NONE, GridBagConstraints.SOUTH);
						// Add a label for the classification...
						layerClassJLabel[i] = new JLabel(symbol.getClassificationLabel(i));
						layerClassJLabel[i].setBackground(getBackground());
						JGUIUtil.addComponent(panel, layerClassJLabel[i], 2, y, 1, 1, 1, 0, insets_none, GridBagConstraints.HORIZONTAL, GridBagConstraints.SOUTH);
						SimpleJTree_Node symNode = new SimpleJTree_Node((Component)panel, "" + layerName + " symbol #" + isym);
						addNode(symNode, node);
					}
				}
			}
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, "Error adding layer to display");
			Message.printWarning(2, routine, e);
		}
	}

	/// <summary>
	/// Empties the tree of all nodes and sets the project node to null.
	/// </summary>
	public virtual void emptyTree()
	{
		try
		{
			if (__projectNode != null)
			{
				removeNode("Project Node Name");
				__projectNode = null;
			}
		}
		catch (Exception e)
		{
			Message.printWarning(1, "GeoViewLegendJTree.emptyTree", "Error emptying tree.");
			Message.printWarning(2, "GeoViewLegendJTree.emptyTree", e);
		}
	}

	/// <summary>
	/// Clean up for garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~GeoViewLegendJTree()
	{
		__projectNode = null;
		__layerPopup = null;
		__treePopup = null;
		__theGeoViewJPanel = null;
		__projectNodePopup = null;
		__animatedPopup = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns a list of all the nodes that contain layer information (i.e., 
	/// are GeoViewLegendJTree_Nodes), but not the project node. </summary>
	/// <returns> a list of the nodes with layer information. </returns>
	public virtual System.Collections.IList getAllLayerNodes()
	{
		System.Collections.IList v = new List<object>();

		if (__projectNode != null)
		{
			if (__projectNode.getChildCount() >= 0)
			{
				for (System.Collections.IEnumerator e = __projectNode.children(); e.MoveNext();)
				{
					SimpleJTree_Node n = (SimpleJTree_Node)e.Current;
					getAllLayerNodes(n, v);
				}
			}
		}
		return v;
	}

	/// <summary>
	/// Utility method used by getAllLayerNodes() </summary>
	/// <param name="node"> the node to check to see if to add to the Vector </param>
	/// <param name="v"> the Vector to which to add the nodes. </param>
	private void getAllLayerNodes(SimpleJTree_Node node, System.Collections.IList v)
	{
		if (node is GeoViewLegendJTree_Node)
		{
			v.Add(node);
		}
		if (node.getChildCount() >= 0)
		{
			for (System.Collections.IEnumerator e = node.children(); e.MoveNext();)
			{
				SimpleJTree_Node n = (SimpleJTree_Node)e.Current;
				getAllLayerNodes(n, v);
			}
		}
	}

	/// <summary>
	/// Returns a lsit of all the nodes that contain layer information (i.e., 
	/// are GeoViewLegendJTree_Nodes), but not the project node. </summary>
	/// <returns> a list of the nodes with layer information. </returns>
	public virtual System.Collections.IList getAllNodes()
	{
		System.Collections.IList v = new List<object>();

		if (__projectNode == null || __projectNode.getChildCount() <= 0)
		{
			return v;
		}

		SimpleJTree_Node node = null;

		for (System.Collections.IEnumerator e = __projectNode.children(); e.MoveNext();)
		{
			node = (SimpleJTree_Node)e.Current;
			v.Add(node);
			for (System.Collections.IEnumerator e2 = node.children(); e2.MoveNext();)
			{
				v.Add(e2.Current);
			}
		}
		return v;
	}

	/// <summary>
	/// Return the GeoViewPanel associated with the legend panel. </summary>
	/// <returns> the GeoViewPanel associated with the legend panel. </returns>
	public virtual GeoViewJPanel getGeoViewJPanel()
	{
		return __theGeoViewJPanel;
	}

	/// <summary>
	/// Returns the names of all the layers in the JTree. </summary>
	/// <param name="visibleOnly"> whether to only return the names of the visible layers. </param>
	/// <returns> a list (guaranteed to be non-null) with the names of the specified kind of layers. </returns>
	public virtual System.Collections.IList[] getLayersNamesAndNodes(bool visibleOnly)
	{
		System.Collections.IList allNodes = getAllLayerNodes();
		System.Collections.IList layers = new List<object>();
		System.Collections.IList names = new List<object>();
		System.Collections.IList matchNodes = new List<object>();
		for (int i = 0; i < allNodes.Count; i++)
		{
			GeoViewLegendJTree_Node node = (GeoViewLegendJTree_Node)allNodes[i];
			if (((visibleOnly && node.isCheckBoxSelected()) || (!visibleOnly)))
			{
				   layers.Add(node.getData());
				names.Add(node.getFieldText());
				matchNodes.Add(node);
			}
		}
		System.Collections.IList[] vectors = new System.Collections.IList[] {layers, names, matchNodes};
		return vectors;
	}

	/// <summary>
	/// Returns the names of all the layers in the JTree. </summary>
	/// <param name="visibleOnly"> whether to only return the names of the visible layers. </param>
	/// <returns> a list (guaranteed to be non-null) with the names of the specified kind of layers. </returns>
	public virtual System.Collections.IList getLayerNames(bool visibleOnly)
	{
		System.Collections.IList allNodes = getAllLayerNodes();
		System.Collections.IList names = new List<object>();
		for (int i = 0; i < allNodes.Count; i++)
		{
			GeoViewLegendJTree_Node node = (GeoViewLegendJTree_Node)allNodes[i];
			if (((visibleOnly && node.isCheckBoxSelected()) || (!visibleOnly)))
			{
				names.Add(node.getFieldText());
			}
		}
		return names;
	}

	/// <summary>
	/// Return the number of items in the legend. </summary>
	/// <returns> the number of items in the legend. </returns>
	public virtual int getNumLegend()
	{
		if (__projectNode != null)
		{
			return getChildCount(__projectNode);
		}
		return 0;
	}

	/// <summary>
	/// Return the list of application layer views that are selected in the legend. </summary>
	/// <returns> a list of application layer types (Strings) that are selected in the
	/// legend.  The list can be of zero size.
	/// Currently the list is not sorted and duplicates and blanks may be present. </returns>
	/// <param name="visibleOnly"> If true only return app layer types that are visible and
	/// selected.  If false, return selected app layer types, whether visible or not. </param>
	public virtual IList<string> getSelectedAppLayerTypes(bool visibleOnly)
	{
		System.Collections.IList allNodes = getAllLayerNodes();
		IList<string> appLayerTypes = new List<string>();
		for (int i = 0; i < allNodes.Count; i++)
		{
			GeoViewLegendJTree_Node node = (GeoViewLegendJTree_Node)allNodes[i];
			if (((visibleOnly && node.isCheckBoxSelected()) || (!visibleOnly)) && node.isTextSelected())
			{
				GeoLayerView layerView = (GeoLayerView)node.getData();
				appLayerTypes.Add(layerView.getLayer().getAppLayerType());
			}
		}
		return appLayerTypes;
	}

	/// <summary>
	/// Return the list of layer views that are selected in the legend. </summary>
	/// <returns> a Vector of layer views that are selected in the legend.  The list can be of zero size. </returns>
	/// <param name="visibleOnly"> If true only return layers that are visible and selected.
	/// If false, return selected layers, whether visible or not. </param>
	public virtual IList<GeoLayerView> getSelectedLayerViews(bool visibleOnly)
	{
		System.Collections.IList allNodes = getAllLayerNodes();
		IList<GeoLayerView> appLayerTypes = new List<GeoLayerView>();
		for (int i = 0; i < allNodes.Count; i++)
		{
			GeoViewLegendJTree_Node node = (GeoViewLegendJTree_Node)allNodes[i];
			if (((visibleOnly && node.isCheckBoxSelected()) || (!visibleOnly)) && node.isTextSelected())
			{
				GeoLayerView layerView = (GeoLayerView)node.getData();
				appLayerTypes.Add(layerView);
			}
		}
		return appLayerTypes;
	}

	/// <summary>
	/// Checks to see if the specified layer is visible. </summary>
	/// <param name="num"> the number of the layer to check. </param>
	/// <returns> true if the layer is visible, false if not. </returns>
	public virtual bool isLayerVisible(int num)
	{
		System.Collections.IList allNodes = getAllLayerNodes();
		GeoViewLegendJTree_Node node = (GeoViewLegendJTree_Node)allNodes[num];
		if (node.isCheckBoxSelected())
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Checks to see if the layer with the given name is visible. </summary>
	/// <param name="name"> the name of the layer to check for visibility. </param>
	/// <returns> true if the layer is visible, false if not. </returns>
	public virtual bool isLayerVisible(string name)
	{
		System.Collections.IList allNodes = getAllLayerNodes();
		for (int i = 0; i < allNodes.Count; i++)
		{
			GeoViewLegendJTree_Node node = (GeoViewLegendJTree_Node)allNodes[i];
			if (node.getFieldText().Equals(name) && node.isCheckBoxSelected())
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Respond to item state changed events.  These happen when the checkbox for
	/// a layer is selected or deselected. </summary>
	/// <param name="event"> the ItemEvent that happened. </param>
	public virtual void itemStateChanged(ItemEvent @event)
	{
		bool select = false;
		if (@event.getStateChange() == ItemEvent.SELECTED)
		{
			select = true;
		}

		// Mark the node as selected, and also select (or deselect) all
		// its immediate children nodes.
		SimpleJTree_Node node = (SimpleJTree_Node)@event.getItemSelectable();
		if (node is GeoViewLegendJTree_Node)
		{
			GeoLayerView layerView = (GeoLayerView)node.getData();
			if (layerView != null)
			{
				layerView.isVisible(select);
			}
		}
		System.Collections.IEnumerator e = getChildren(node);
		while (e.MoveNext())
		{
			SimpleJTree_Node n = (SimpleJTree_Node)e.Current;

			if (n is GeoViewLegendJTree_Node)
			{
				((GeoViewLegendJTree_Node)n).setCheckBoxSelected(select);
				GeoLayerView layerView = (GeoLayerView)n.getData();
				layerView.isVisible(select);
			}
		}

		// redraw the map according to the new settings
		bool waitState = JGUIUtil.getWaitCursor();
		JGUIUtil.setWaitCursor(__theGeoViewJPanel.getParentJFrame(), true);
		__theGeoViewJPanel.setWaitCursorAfterRepaint(waitState);
		repaint();
		__theGeoViewJPanel.refreshAfterSelection();
	}

	/// <summary>
	/// Determines whether the popup menu should be shown based on data in the MouseEvent. </summary>
	/// <param name="e"> the MouseEvent that happened. </param>
	private void maybeShowPopup(MouseEvent e)
	{
		if (__treePopup != null && __treePopup.isPopupTrigger(e))
		{
			__treePopup.show(e.getComponent(), e.getX(), e.getY());
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
	/// Responds to mouse pressed events and sees if the popup should be shown. </summary>
	/// <param name="event"> the MouseEvent that happened. </param>
	public virtual void mousePressed(MouseEvent @event)
	{
		maybeShowPopup(@event);
	}

	/// <summary>
	/// Responds to mouse released events and sees if the popup should be shown. </summary>
	/// <param name="event"> the MouseEvent that happened. </param>
	public virtual void mouseReleased(MouseEvent @event)
	{
		maybeShowPopup(@event);
	}

	/// <summary>
	/// Remove all layer views from the legend.  This should be called when clearing
	/// the GeoViewJPanel of all data.  The GeoViewLegendJTree will have no memory of its previous contents.
	/// </summary>
	public virtual void removeAllLayerViews()
	{
		emptyTree();
	}

	/// <summary>
	/// Remove an item from the legend. </summary>
	/// <param name="layerView_to_remove"> GeoLayerView to remove from the legend panel. </param>
	public virtual void removeLayerView(GeoLayerView layerView_to_remove)
	{
		SimpleJTree_Node node = findNodeWithData(layerView_to_remove);
		if (node == null)
		{
			return;
		}

		try
		{
			markVisibleNodes();
			removeNode(node);
			resetVisibleNodes();
		}
		catch (Exception e)
		{
			Message.printWarning(1, "GeoViewLegendJTree.removeLayerView", "Error removing layer view.");
			Message.printWarning(2, "GeoViewLegendJTree.removeLayerView",e);
		}
	}

	/// <summary>
	/// Sets whether a layer should be visible or not.  If the layer is turned off,
	/// it will be deselected in the tree and not drawn on the map. </summary>
	/// <param name="layerView"> the layer to affect. </param>
	/// <param name="visible"> whether to make the layer visible or not. </param>
	public virtual void setLayerVisible(GeoLayerView layerView, bool visible)
	{
		System.Collections.IList allNodes = getAllLayerNodes();
		GeoLayerView layer = null;
		GeoViewLegendJTree_Node node = null;
		for (int i = 0; i < allNodes.Count; i++)
		{
			node = (GeoViewLegendJTree_Node)allNodes[i];
			layer = (GeoLayerView)node.getData();
			if (layer == layerView)
			{
				node.setVisible(visible);
			}
		}
	}

	/// <summary>
	/// Sets the text of the project node (the top-most visible node in the tree).
	/// If a project node doesn't exist (is null), one will be added. </summary>
	/// <param name="name"> name to set the project node to. </param>
	public virtual void setProjectNodeText(string name)
	{
		try
		{
			if (__projectNode == null)
			{
				__projectNode = new GeoViewLegendJTree_Node(name, "Project Node Name", this, __projectNodePopup);
				__projectNode.setCheckBoxSelected(true);
				addNode(__projectNode);
			}
			else
			{
				__projectNode = new GeoViewLegendJTree_Node(name, "Project Node Name", this, __projectNodePopup);
				__projectNode.setCheckBoxSelected(true);
				replaceNode("Project Node Name", __projectNode);
			}
		}
		catch (Exception e)
		{
			Message.printWarning(1, "GeoViewLegendJTree.setProjectNodeText", "Error setting project node text.");
			Message.printWarning(2, "GeoViewLegendJTree.setProjectNodeText", e);
		}
	}

	/// <summary>
	/// Updates the geo view JPanel's button state depending on whether any of the layers are selected or not.
	/// </summary>
	public virtual void updateGeoViewJPanelButtons()
	{
		// Count how many children of the projectNode are selected
		// and if more than 0, tell the GeoViewJPanel to update how the buttons are enabled or not
		System.Collections.IEnumerator e = getChildren(__projectNode);
		while (e.MoveNext())
		{
			SimpleJTree_Node n = (SimpleJTree_Node)e.Current;

			if (n is GeoViewLegendJTree_Node)
			{
				if (((GeoViewLegendJTree_Node)n).isSelected())
				{
					__theGeoViewJPanel.setButtonsEnabledByLayersSelected(true);
					return;
				}
			}
		}
		__theGeoViewJPanel.setButtonsEnabledByLayersSelected(false);
	}

	}

}