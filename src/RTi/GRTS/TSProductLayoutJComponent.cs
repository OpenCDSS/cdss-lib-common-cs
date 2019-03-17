using System;
using System.Collections.Generic;

// TSProductLayoutJComponent - handles drawing the TSProduct layout preview in the upper-left-hand corner of the TSProductJFrame

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

namespace RTi.GRTS
{






	using GRAspect = RTi.GR.GRAspect;
	using GRColor = RTi.GR.GRColor;
	using GRDrawingAreaUtil = RTi.GR.GRDrawingAreaUtil;
	using GRJComponentDevice = RTi.GR.GRJComponentDevice;
	using GRJComponentDrawingArea = RTi.GR.GRJComponentDrawingArea;
	using GRLimits = RTi.GR.GRLimits;
	using GRText = RTi.GR.GRText;
	using GRUnits = RTi.GR.GRUnits;

	using DayTS = RTi.TS.DayTS;
	using HourTS = RTi.TS.HourTS;
	using IrregularTS = RTi.TS.IrregularTS;
	using MinuteTS = RTi.TS.MinuteTS;
	using MonthTS = RTi.TS.MonthTS;
	using TS = RTi.TS.TS;
	using TSIdent = RTi.TS.TSIdent;
	using YearTS = RTi.TS.YearTS;

	using DragAndDropControl = RTi.Util.GUI.DragAndDropControl;
	using DragAndDropTransferPrimitive = RTi.Util.GUI.DragAndDropTransferPrimitive;
	using DragAndDropUtil = RTi.Util.GUI.DragAndDropUtil;
	using ResponseJDialog = RTi.Util.GUI.ResponseJDialog;
	using DragAndDrop = RTi.Util.GUI.DragAndDrop;
	using JComboBoxResponseJDialog = RTi.Util.GUI.JComboBoxResponseJDialog;

	using IOUtil = RTi.Util.IO.IOUtil;

	using Message = RTi.Util.Message.Message;

	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This class handles drawing the TSProduct layout preview in the upper-left-hand
	/// corner of the TSProductJFrame.  It also handles all the mouse interaction
	/// between the user and the layout preview.  An instance is normally only created
	/// when instantiating a TSProductJFrame.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class TSProductLayoutJComponent extends RTi.GR.GRJComponentDevice implements java.awt.event.ActionListener, java.awt.event.MouseListener, java.awt.dnd.DragGestureListener, java.awt.dnd.DragSourceListener, java.awt.dnd.DropTargetListener, RTi.Util.GUI.DragAndDrop
	public class TSProductLayoutJComponent : GRJComponentDevice, ActionListener, MouseListener, DragGestureListener, DragSourceListener, DropTargetListener, DragAndDrop
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			__GRAPHS_HEIGHT = __WIDTH - (__X_OFFSET * 2);
			__GRAPHS_WIDTH = __GRAPHS_HEIGHT;
		}


	/// <summary>
	/// Class name.
	/// </summary>
	private readonly string __CLASS = "TSProductLayoutJComponent";

	/// <summary>
	/// Constant values for the drawing area.
	/// </summary>
	private readonly int __HEIGHT = 111, __MAX_FONT_SIZE = 70, __MIN_FONT_SIZE = 6, __WIDTH = 100; // total width

	/// <summary>
	/// More constant values.
	/// </summary>
	private int __BOTTOM_Y = 21, __X_OFFSET = 10, __GRAPHS_HEIGHT, __GRAPHS_WIDTH; // because it's always a square

	/// <summary>
	/// The font in which text should be drawn on the preview.
	/// </summary>
	private readonly string __FONT = "Arial";

	/// <summary>
	/// GUI labels.
	/// </summary>
	private readonly string __MENU_SHOW_ALL_PROPERTIES = "Show all properties", __MENU_SHOW_ANNOTATION_PROPERTIES = "Show all annotation properties", __MENU_SHOW_DATA_PROPERTIES = "Show all data properties", __POPUP_ADD_GRAPH_ABOVE = "Add Graph Above Selected", __POPUP_ADD_GRAPH_BELOW = "Add Graph Below Selected", __POPUP_ADD_GRAPH_END = "Add Graph at Bottom", __POPUP_REMOVE_GRAPH = "Remove Graph", __POPUP_MOVE_GRAPH_UP = "Move Graph Up", __POPUP_MOVE_GRAPH_DOWN = "Move Graph Down";

	/// <summary>
	/// Whether the current paint() call is the first time ever.  Used for setting up some initial settings.
	/// </summary>
	private bool __firstPaint = true;

	/// <summary>
	/// Data that explain how drag and drop is to be handled for this component.
	/// </summary>
	private DragAndDropControl __dndData;

	/// <summary>
	/// The drawing area on which the preview will be drawn.
	/// </summary>
	private GRJComponentDrawingArea __da = null;

	/// <summary>
	/// Menu items for the popup menu.
	/// </summary>
	private JMenuItem __addAboveJMenuItem, __addBelowJMenuItem, __moveUpJMenuItem, __moveDownJMenuItem, __removeJMenuItem;

	/// <summary>
	/// The menu that appears when the layout is right-clicked on.
	/// </summary>
	private JPopupMenu __popup;

	/// <summary>
	/// The drawing area bounds.
	/// </summary>
	private Rectangle _bounds = null;

	/// <summary>
	/// The product for which the graph previews are being drawn.
	/// </summary>
	private TSProduct __product = null;

	/// <summary>
	/// The frame on which this preview is located.
	/// </summary>
	private TSProductJFrame __parent = null;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="parent"> the parent TSProductJFrame on which this appears. </param>
	/// <param name="product"> the product being represented. </param>
	public TSProductLayoutJComponent(TSProductJFrame parent, TSProduct product) : base("TSProductLayoutJComponent")
	{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}

		__parent = parent;
		__product = product;
		// create a new DragAndDropControl object that specifies how drag and
		// drop will behave in this component.  In this case, the data object
		// is being set up to not allow dragging (ACTION_NONE) and to accept
		// copy or move drops (ACTION_COPY_OR_MOVE).
		__dndData = new DragAndDropControl(DragAndDropUtil.ACTION_NONE, DragAndDropUtil.ACTION_COPY_OR_MOVE);

		// this should always be true, but in the future it may be that 
		// dropping data is disabled for some instances of the component
		if (__dndData.allowsDrop())
		{
			// sets this component as a drop target for drag and drop 
			// operations, and specifies that it should allow drops from 
			// either copy or move actions
			__dndData.setDropTarget(DragAndDropUtil.createDropTarget(this, DragAndDropUtil.ACTION_COPY_OR_MOVE, this));

		}

		GRLimits limits = new GRLimits(0, 0, __WIDTH, __HEIGHT);
		__da = new GRJComponentDrawingArea(this, __CLASS + ".DA", GRAspect.FILL, limits, GRUnits.DEVICE, GRLimits.DEVICE, limits);
		setLimits(limits);

		buildPopup();

		repaint();
	}

	/// <summary>
	/// Responds to action events. </summary>
	/// <param name="event"> the ActionEvent that happened. </param>
	public virtual void actionPerformed(ActionEvent @event)
	{
		string command = @event.getActionCommand();

		// Commit any changes to the product prior to doing anything
		__parent.updateTSProduct();

		if (command.Equals(__POPUP_ADD_GRAPH_ABOVE) || command.Equals(__POPUP_ADD_GRAPH_BELOW) || command.Equals(__POPUP_ADD_GRAPH_END))
		{
			int selectedGraph = __parent.getSelectedGraph();

			if (command.Equals(__POPUP_ADD_GRAPH_ABOVE))
			{
			}
			else if (command.Equals(__POPUP_ADD_GRAPH_BELOW))
			{
				selectedGraph++;
			}
			else
			{
				// __POPUP_ADD_GRAPH_END
				selectedGraph = __parent.getGraphList().Count;
			}
			if (!__parent.areGraphsDefined())
			{
				selectedGraph = -1;
			}

			__parent.addGraph(selectedGraph);
		}
		else if (command.Equals(__POPUP_REMOVE_GRAPH))
		{
			removeGraphClicked();
		}
		else if (command.Equals(__POPUP_MOVE_GRAPH_UP))
		{
			moveGraphUpClicked();
		}
		else if (command.Equals(__POPUP_MOVE_GRAPH_DOWN))
		{
			moveGraphDownClicked();
		}
		else if (command.Equals(__MENU_SHOW_ALL_PROPERTIES))
		{
			__product.showProps(2);
			return;
		}
		else if (command.Equals(__MENU_SHOW_ANNOTATION_PROPERTIES))
		{
			__product.showPropsStartingWith(2, "Annotation");
		}
		else if (command.Equals(__MENU_SHOW_DATA_PROPERTIES))
		{
			__product.showPropsStartingWith(2, "Data");
			return;
		}

		// Do this any time graphs are changed ...
		__parent.getTSViewJFrame().getViewGraphJFrame().getMainJComponent().reinitializeGraphs(__product);
		if (__parent.getTSViewJFrame().getViewGraphJFrame().getReferenceGraph() != null)
		{
			__parent.getTSViewJFrame().getViewGraphJFrame().getReferenceGraph().reinitializeGraphs(__product);
		}

		repaint();
		__parent.checkGUIState();
	}

	/// <summary>
	/// Builds the list of graphs that are 'down' from the selected graph, for use in the 'move graph' dialog box. </summary>
	/// <param name="selectedGraph"> the graph to be moved down. </param>
	/// <returns> a list of the graphs that are 'down' from the selected graph. </returns>
	private IList<string> buildDownList(int selectedGraph)
	{
		IList<string> v = new List<string>();
		IList<string> graphs = __parent.getGraphList();
		string s = null;
		int count = 1;
		string plural = "";
		for (int i = selectedGraph + 1; i < graphs.Count; i++)
		{
			s = StringUtil.getToken(graphs[i], "-", 0, 0);
			s = s.Trim();
			v.Add("" + count + " step" + plural + ", below graph #" + (i + 1) + " (\"" + s + "\")");
			if (count == 1)
			{
				plural = "s";
			}
			count++;
		}
		return v;
	}

	/// <summary>
	/// Builds the popup menu.
	/// </summary>
	private void buildPopup()
	{
		__popup = new JPopupMenu();
		__addAboveJMenuItem = new JMenuItem(__POPUP_ADD_GRAPH_ABOVE);
		__addAboveJMenuItem.addActionListener(this);
		__popup.add(__addAboveJMenuItem);
		__addBelowJMenuItem = new JMenuItem(__POPUP_ADD_GRAPH_BELOW);
		__addBelowJMenuItem.addActionListener(this);
		__popup.add(__addBelowJMenuItem);
		JMenuItem mi = new JMenuItem(__POPUP_ADD_GRAPH_END);
		mi.addActionListener(this);
		__popup.add(mi);
		__removeJMenuItem = new JMenuItem(__POPUP_REMOVE_GRAPH);
		__removeJMenuItem.addActionListener(this);
		__popup.add(__removeJMenuItem);
		__popup.addSeparator();
		__moveUpJMenuItem = new JMenuItem(__POPUP_MOVE_GRAPH_UP);
		__moveUpJMenuItem.addActionListener(this);
		__popup.add(__moveUpJMenuItem);
		__moveDownJMenuItem = new JMenuItem(__POPUP_MOVE_GRAPH_DOWN);
		__moveDownJMenuItem.addActionListener(this);
		__popup.add(__moveDownJMenuItem);
		if (IOUtil.testing())
		{
			__popup.addSeparator();
			mi = new JMenuItem(__MENU_SHOW_ALL_PROPERTIES);
			mi.addActionListener(this);
			__popup.add(mi);
			mi = new JMenuItem(__MENU_SHOW_DATA_PROPERTIES);
			mi.addActionListener(this);
			__popup.add(mi);
			mi = new JMenuItem(__MENU_SHOW_ANNOTATION_PROPERTIES);
			mi.addActionListener(this);
			__popup.add(mi);
		}
	}

	/// <summary>
	/// Builds the list of graphs that are 'up' from the selected graph, for use 
	/// in the 'move graph' dialog box. </summary>
	/// <param name="selectedGraph"> the graph to be moved up. </param>
	/// <returns> a list of the graphs that are 'up' from the selected graph. </returns>
	private IList<string> buildUpList(int selectedGraph)
	{
		IList<string> v = new List<string>();
		IList<string> graphs = __parent.getGraphList();
		string s = null;
		int count = 1;
		string plural = "";
		for (int i = selectedGraph - 1; i >= 0; i--)
		{
			s = StringUtil.getToken(graphs[i], "-", 0, 0);
			s = s.Trim();
			v.Add("" + count + " step" + plural + ", above graph #" + (i + 1) + " ('" + s + "')");
			if (count == 1)
			{
				plural = "s";
			}
			count++;
		}
		return v;
	}

	/// <summary>
	/// Calculates the proper font size to be drawn inside a graph preview. </summary>
	/// <param name="graphInsideHeight"> the height inside the graph in which the text can
	/// be drawn.  Typically it is 4 less than graph height, to account for the 
	/// 1 pixel border on top and bottom and then 1 pixel of space between the 
	/// border and the text.
	/// // REVISIT (JTS - 2004-04-27)
	/// // probably move to a utility function in GR </param>
	/// <param name="fontSize"> the desired fontSize in points. </param>
	/// <param name="text"> text to size -- usually the graph number. </param>
	/// <returns> the font size to draw in so that text fits best, in points.  Returned
	/// as an integer because java fonts only accept integer-based font point sizes. </returns>
	private int calculateFontSize(int graphInsideHeight, int fontSize, string text)
	{
		bool done = false;
		int height = -1;
		int smaller = -1;
		int bigger = -1;

		while (!done)
		{
			if (fontSize >= __MAX_FONT_SIZE)
			{
				return __MAX_FONT_SIZE;
			}
			if (fontSize <= __MIN_FONT_SIZE)
			{
				return __MIN_FONT_SIZE;
			}

			height = calculateTextHeight(fontSize, text);
			if (height >= __MAX_FONT_SIZE)
			{
				return __MAX_FONT_SIZE;
			}
			if (height <= __MIN_FONT_SIZE)
			{
				return __MIN_FONT_SIZE;
			}

			if (height < graphInsideHeight)
			{
				bigger = calculateTextHeight(fontSize + 1, text);
				if (bigger > graphInsideHeight)
				{
					return fontSize;
				}
				else if (bigger == graphInsideHeight)
				{
					return fontSize + 1;
				}
				else
				{
					fontSize++;
				}
			}
			else if (height == graphInsideHeight)
			{
				return height;
			}
			else
			{
				// height > graphInsideHeight
				smaller = calculateTextHeight(fontSize - 1, text);

				if (smaller > graphInsideHeight)
				{
					fontSize--;
				}
				else if (smaller == graphInsideHeight)
				{
					return fontSize - 1;
				}
				else
				{
					return fontSize - 1;
				}
			}
		}
		return -1;
	}

	/// <summary>
	/// Calculates the height of the given text in the given font size in pixels. </summary>
	/// <param name="fontSize"> the size of the font in points. </param>
	/// <param name="text"> the text to draw. </param>
	/// <returns> the height of the given text in the given font size in pixels. </returns>
	private int calculateTextHeight(int fontSize, string text)
	{
		GRDrawingAreaUtil.setFont(__da, __FONT, fontSize);
		GRLimits limits = GRDrawingAreaUtil.getTextExtents(__da, text, GRUnits.DEVICE);
		return (int)limits.getHeight();
	}

	////////////////////////////////////////////////////////////////
	// DragAndDrop methods
	/// <summary>
	/// Called when a drag is about to start.
	/// </summary>
	public virtual bool dragAboutToStart()
	{
		return true;
	}

	/// <summary>
	/// Called when a drag is started.
	/// </summary>
	public virtual void dragStarted()
	{
	}

	/// <summary>
	/// Called when a drag is successful.
	/// </summary>
	public virtual void dragSuccessful(int action)
	{
	}

	/// <summary>
	/// Called when a drag is unsuccessful.
	/// </summary>
	public virtual void dragUnsuccessful(int action)
	{
	}

	/// <summary>
	/// Called when data are over this component and can be dropped.
	/// </summary>
	public virtual void dropAllowed()
	{
	}

	/// <summary>
	/// Called when data are dragged outside of this component's area.
	/// </summary>
	public virtual void dropExited()
	{
	}

	/// <summary>
	/// Called when data are over this component and cannot be dropped.
	/// </summary>
	public virtual void dropNotAllowed()
	{
	}

	/// <summary>
	/// Called when a drop was completed successfully.
	/// </summary>
	public virtual void dropSuccessful()
	{
	}

	/// <summary>
	/// Called when a drop was not completed successfully.
	/// </summary>
	public virtual void dropUnsuccessful()
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void setAlternateTransferable(Transferable t)
	{
	}

	////////////////////////////////////////////////////////////////
	// Drag Gesture events
	/// <summary>
	/// Called when a drag gesture was recognized and a drag can start.
	/// </summary>
	public virtual void dragGestureRecognized(DragGestureEvent dge)
	{
	}

	////////////////////////////////////////////////////////////////
	// Drag events
	/// <summary>
	/// Called when the drag ends.
	/// </summary>
	public virtual void dragDropEnd(DragSourceDropEvent dsde)
	{
	}

	/// <summary>
	/// Called when a drag entered a component's area.
	/// </summary>
	public virtual void dragEnter(DragSourceDragEvent dsde)
	{
		// REVISIT (JTS - 2004-04-28)
		// change the cursor depending on whether over a graph or not
	}

	/// <summary>
	/// Called when a drag exits a component's area.
	/// </summary>
	public virtual void dragExit(DragSourceEvent dse)
	{
	}

	/// <summary>
	/// Called when dragged data is over a component.
	/// </summary>
	public virtual void dragOver(DragSourceDragEvent dsde)
	{
	}

	/// <summary>
	/// Called if the drop action changes for the drop component.
	/// </summary>
	public virtual void dropActionChanged(DragSourceDragEvent dsde)
	{
	}

	////////////////////////////////////////////////////////////////
	// Drop events
	/// <summary>
	/// Called when the action for a drop changes.
	/// </summary>
	public virtual void dropActionChanged(DropTargetDragEvent dtde)
	{
		DragAndDropUtil.dropActionChanged(this, dtde);
	}

	/// <summary>
	/// Called when a drag enters this component's area.
	/// </summary>
	public virtual void dragEnter(DropTargetDragEvent dtde)
	{
		DragAndDropUtil.dragEnter(this, dtde);
	}

	/// <summary>
	/// Called when a drag exits this components's area.
	/// </summary>
	public virtual void dragExit(DropTargetEvent dte)
	{
		DragAndDropUtil.dragExit(this, dte);
	}

	/// <summary>
	/// Called when a drag is over this component.
	/// </summary>
	public virtual void dragOver(DropTargetDragEvent dtde)
	{
		DragAndDropUtil.dragOver(this, dtde);
	}

	/// <summary>
	/// Called when a drop occurs.
	/// </summary>
	public virtual void drop(DropTargetDropEvent dtde)
	{
		DragAndDropUtil.drop(this, dtde);
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~TSProductLayoutJComponent()
	{
		__dndData = null;
		__da = null;
		__addAboveJMenuItem = null;
		__addBelowJMenuItem = null;
		__moveUpJMenuItem = null;
		__moveDownJMenuItem = null;
		__removeJMenuItem = null;
		__popup = null;
		_bounds = null;
		__product = null;
		__parent = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Determines the graph that was clicked on. </summary>
	/// <returns> the number of the graph that was clicked on, or -1 if no graph was
	/// clicked on. </returns>
	private int findClickedGraph(int x, int y)
	{
		int numGraphs = __product.getNumSubProducts();

		if (numGraphs == 0)
		{
			return -1;
		}

		// change y to accomodate RTi's inverted Y style as compared to the Y
		// value received by a mouse click (in which the origin is at the 
		// upper-left).
		y = invertY(y);

		if (x < __X_OFFSET || x > (__X_OFFSET + __GRAPHS_WIDTH))
		{
			// outside the bounds of any graph
			return -1;
		}
		if (y < __BOTTOM_Y || y > (__BOTTOM_Y + __GRAPHS_HEIGHT))
		{
			// above or below any graph
			return -1;
		}

		int bottomY = __BOTTOM_Y;
		int graphHeight = (int)(__GRAPHS_HEIGHT / numGraphs);
		int topY = bottomY + graphHeight;
		for (int i = 0; i < numGraphs; i++)
		{
			if (i > 0)
			{
				bottomY += graphHeight;
				topY += graphHeight;
			}
			if (y < topY && y > bottomY)
			{
				return (numGraphs - i - 1);
			}
		}
		return -1;
	}

	/// <summary>
	/// Gets the number of positions that a graph should be moved as selected from 
	/// the move graph dialog box. </summary>
	/// <param name="s"> the String to get theamount of change from. </param>
	/// <returns> the number of steps of change. </returns>
	private int getChangeAmount(string s)
	{
		// given a string like "1 step", "2 steps", etc ...
		int index = s.IndexOf("step", StringComparison.Ordinal);
		s = s.Substring(0, index);
		s = s.Trim();
		try
		{
			int? I = Convert.ToInt32(s);
			return I.Value;
		}
		catch (Exception)
		{
			return -1;
		}
	}

	/// <summary>
	/// Returns the valid data flavors for data that can be dropped on this
	/// TSProductLayoutJComponent.
	/// Recognizes most Time Series and also string and text flavors.  If a String or
	/// text was dropped on this, it assumes it to be a local transfer of a time 
	/// series from one graph to another and will try to do so. </summary>
	/// <returns> the valid data flavors for data that can be dropped on this.  For more
	/// information on data flavors, check each time series.  </returns>
	public virtual DataFlavor[] getDataFlavors()
	{
		DataFlavor[] flavors = new DataFlavor[9];
		flavors[0] = MinuteTS.minuteTSFlavor;
		flavors[1] = HourTS.hourTSFlavor;
		flavors[2] = DayTS.dayTSFlavor;
		flavors[3] = MonthTS.monthTSFlavor;
		flavors[4] = YearTS.yearTSFlavor;
		flavors[5] = IrregularTS.irregularTSFlavor;

		// the following allow a TSID to be dropped
		flavors[6] = TSIdent.tsIdentFlavor;
		flavors[7] = DragAndDropTransferPrimitive.stringFlavor;
		flavors[8] = DragAndDropTransferPrimitive.textFlavor;
		return flavors;
	}

	/// <summary>
	/// Returns this component's DragAndDropControl object. </summary>
	/// <returns> thsi component's DragAndDropControl object. </returns>
	public virtual DragAndDropControl getDragAndDropControl()
	{
		return __dndData;
	}

	/// <summary>
	/// Returns null.  This component does not support dragging.
	/// </summary>
	public virtual Transferable getTransferable()
	{
		return null;
	}

	/// <summary>
	/// Handles data dropped on this component. </summary>
	/// <param name="o"> the data that was dropped. </param>
	/// <param name="p"> the Point at which data was dropped. </param>
	/// <returns> true if the data was handled successfully, false if not. </returns>
	public virtual bool handleDropData(object o, Point p)
	{
		if (!__parent.areGraphsDefined())
		{
			return false;
		}

		string id = null;
		IList<TS> v = null;

		if (o is TS)
		{
			if (o is YearTS)
			{
				if (IOUtil.testing())
				{
					Message.printStatus(1, "", "Dropping Year time series");
				}
			}
			else if (o is MonthTS)
			{
				if (IOUtil.testing())
				{
					Message.printStatus(1, "", "Dropping Month time series");
				}
			}
			else if (o is DayTS)
			{
				if (IOUtil.testing())
				{
					Message.printStatus(1, "", "Dropping Day time series");
				}
			}
			else if (o is HourTS)
			{
				if (IOUtil.testing())
				{
					Message.printStatus(1, "", "Dropping Hour time series");
				}
			}
			else if (o is MinuteTS)
			{
				if (IOUtil.testing())
				{
					Message.printStatus(1,"", "Dropping Minute time series");
				}
			}
			else if (o is IrregularTS)
			{
				if (IOUtil.testing())
				{
					Message.printStatus(1, "", "Dropping Irregular time series");
				}
			}
			else
			{
				if (IOUtil.testing())
				{
					Message.printStatus(1, "", "Unknown time series: " + o);
				}
			}

			TS ts = (TS)o;
			id = ts.getIdentifier().ToString();
			int x = p.x;
			int graph = findClickedGraph(x, p.y);

			__parent.addData(graph, id);
			v = __product.getTSList();
			v.Add(ts);
			__product.setTSList(v);
			__parent.getTSViewJFrame().getViewGraphJFrame().getMainJComponent().reinitializeGraphs(__product);
			if (__parent.getTSViewJFrame().getViewGraphJFrame().getReferenceGraph() != null)
			{
				__parent.getTSViewJFrame().getViewGraphJFrame().getReferenceGraph().reinitializeGraphs(__product);
			}
			return true;
		}
		else if (o is DragAndDropTransferPrimitive)
		{
			DragAndDropTransferPrimitive d = (DragAndDropTransferPrimitive)o;
			string s = (string)d.getData();

			int x = p.x;
			int graph = findClickedGraph(x, p.y);

			int selectedGraph = __parent.getSelectedGraph();

			int index = s.IndexOf("-", StringComparison.Ordinal);
			string tsString = s.Substring(0, index).Trim();
			int tsNum = StringUtil.atoi(tsString) - 1;

			if (graph != -1 && graph != selectedGraph)
			{
				__parent.moveSelectedData(selectedGraph, graph, tsNum);
				return true;
			}
			else
			{
				return false;
			}
		}
		else
		{
			Message.printStatus(1, "", "Unknown drop: " + o + " (" + o.GetType() + ")");
			return false;
		}
	}

	// REVISIT (JTS - 2004-04-27)
	// move into some GR util?
	/// <summary>
	/// Takes a Y value returned from the Java-based coordinate system and converts
	/// it to RTi's coordinate system. </summary>
	/// <returns> the RTi-based Y coordinate. </returns>
	public virtual int invertY(int y)
	{
		return (int)(_devy2 - y);
	}

	/// <summary>
	/// Shows a popup menu if the layout was right-clicked on in a graph area. </summary>
	/// <param name="even"> the MouseEvent that happened. </param>
	private void showPopup(MouseEvent @event)
	{
		if (@event.getButton() != MouseEvent.BUTTON1)
		{
			int selectedGraph = __parent.getSelectedGraph();
			int numGraphs = __product.getNumSubProducts();
			if (numGraphs == 0)
			{
				__moveUpJMenuItem.setEnabled(false);
				__moveDownJMenuItem.setEnabled(false);
				__removeJMenuItem.setEnabled(false);
				__addAboveJMenuItem.setEnabled(false);
				__addBelowJMenuItem.setEnabled(false);
			}
			else
			{
				__removeJMenuItem.setEnabled(true);
				__addAboveJMenuItem.setEnabled(true);
				__addBelowJMenuItem.setEnabled(true);
				if (selectedGraph == 0)
				{
					__moveUpJMenuItem.setEnabled(false);
				}
				else
				{
					__moveUpJMenuItem.setEnabled(true);
				}
				if (selectedGraph == (numGraphs - 1))
				{
					__moveDownJMenuItem.setEnabled(false);
				}
				else
				{
					__moveDownJMenuItem.setEnabled(true);
				}
			}
			__popup.show(@event.getComponent(), @event.getX(), @event.getY());
		}
	}

	/// <summary>
	/// Responds to mouse clicked events.  Selects a graph if one is clicked on. </summary>
	/// <param name="event"> the MouseEvent that happened. </param>
	public virtual void mouseClicked(MouseEvent @event)
	{
		if (@event.getButton() != MouseEvent.BUTTON1)
		{
			return;
		}
		int x = @event.getX();
		int y = @event.getY();

		int graphNum = findClickedGraph(x, y);

		if (graphNum == -1)
		{
			return;
		}

		__parent.setSelectedGraph(graphNum);
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void mouseEntered(MouseEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void mouseExited(MouseEvent @event)
	{
	}

	/// <summary>
	/// Possibly shows a popup menu if the right mouse button was pressed. </summary>
	/// <param name="event"> the MouseEvent that happened. </param>
	public virtual void mousePressed(MouseEvent @event)
	{
		showPopup(@event);
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void mouseReleased(MouseEvent @event)
	{
	}

	/// <summary>
	/// Responds when the "Move Graph Down" menu item is clicked by opening a dialog
	/// to determine how far to move the graph.
	/// </summary>
	private void moveGraphDownClicked()
	{
		int selectedGraph = __parent.getSelectedGraph();
		IList<string> v = buildDownList(selectedGraph);
		if (v.Count == 1)
		{
			__product.swapSubProducts(selectedGraph, selectedGraph + 1);
			__parent.redisplayProperties();
			__parent.setSelectedGraph(selectedGraph + 1);
		}
		else
		{
			string s = (new JComboBoxResponseJDialog(__parent, "Move Graph Down", "Move graph down: ", v, ResponseJDialog.OK | ResponseJDialog.CANCEL)).response();
			if (string.ReferenceEquals(s, null))
			{
				return;
			}
			int change = getChangeAmount(s);
			if (change == -1)
			{
				return;
			}
			for (int i = 0; i < change; i++)
			{
				__product.swapSubProducts(selectedGraph, selectedGraph + 1);
				selectedGraph++;
	//			Message.printStatus(1, "", "Swapped graphs "
	//				+ selectedGraph + " and " + (selectedGraph + 1)
	//				+ ".  Selected graph now: " + selectedGraph);
			}
			__parent.redisplayProperties();
			__parent.setSelectedGraph(selectedGraph);
		}
	}

	/// <summary>
	/// Responds when the "Move Graph Up" menu item is clicked by opening a dialog to
	/// determine how far to move the graph.
	/// </summary>
	private void moveGraphUpClicked()
	{
		int selectedGraph = __parent.getSelectedGraph();
		IList<string> v = buildUpList(selectedGraph);
		if (v.Count == 1)
		{
			__product.swapSubProducts(selectedGraph, selectedGraph - 1);
			__parent.redisplayProperties();
			__parent.setSelectedGraph(selectedGraph - 1);
		}
		else
		{
			string s = (new JComboBoxResponseJDialog(__parent, "Move Graph Up", "Move graph up: ", v, ResponseJDialog.OK | ResponseJDialog.CANCEL)).response();
			if (string.ReferenceEquals(s, null))
			{
				return;
			}
			int change = getChangeAmount(s);
			if (change == -1)
			{
				return;
			}
			for (int i = 0; i < change; i++)
			{
				__product.swapSubProducts(selectedGraph, selectedGraph - 1);
				selectedGraph--;
	//			Message.printStatus(1, "", "Swapped graphs "
	//				+ selectedGraph + " and " + (selectedGraph - 1)
	//				+ ".  Selected graph now: " + selectedGraph);
			}
			__parent.redisplayProperties();
			__parent.setSelectedGraph(selectedGraph);
		}
	}

	/// <summary>
	/// Responds when the remove graph menu item is selected by opening a dialog to
	/// make sure the graph should really be removed.
	/// </summary>
	private void removeGraphClicked()
	{
		if ((new ResponseJDialog(__parent, "Remove Graph", "Are you sure " + "you want to remove this graph?", ResponseJDialog.YES | ResponseJDialog.NO)).response() == ResponseJDialog.NO)
		{
			return;
		}
		int selectedGraph = __parent.getSelectedGraph();
		int origPos = selectedGraph + 1;
		IList<string> graphs = __parent.getGraphList();
		int count = graphs.Count;
		int shifts = count - (selectedGraph + 1);
		for (int i = 0; i < shifts; i++)
		{
			__product.swapSubProducts(selectedGraph, selectedGraph + 1);
			selectedGraph++;
		}

		__product.removeSubProduct(selectedGraph);

		count--;
		if (count == 0)
		{
			__parent.setSelectedSubProductAndData(-1, 0);
			selectedGraph = -1;
		}
		else if (count == 1)
		{
			__parent.setSelectedSubProductAndData(0, 0);
			selectedGraph = 0;
		}
		else
		{
			if (origPos > count)
			{
				__parent.setSelectedSubProductAndData(origPos - 2, 0);
				selectedGraph = (origPos - 2);
			}
			else
			{
				__parent.setSelectedSubProductAndData(origPos - 1, 0);
				selectedGraph = (origPos - 1);
			}
		}
	//	__product.dumpProps();
		__parent.redisplayProperties();
		if (selectedGraph > -1)
		{
			__parent.setSelectedGraph(selectedGraph);
		}
	}

	/// <summary>
	/// Paints the display, showing a rectangle for every graph in the TSProduct with
	/// the number of the graph in the center of the graph.  The currently-selected
	/// graph is shaded grey. </summary>
	/// <param name="g"> the Graphics context on which to paint. </param>
	public virtual void paint(Graphics g)
	{
		setGraphics(g);
		_bounds = getBounds();

		if (__firstPaint)
		{
			// If double buffering, create a new image...
			if ((_buffer == null) || _double_buffering)
			{
				setupDoubleBuffer(0, 0, _bounds.width, _bounds.height);
			}
			__firstPaint = false;
		}

		_graphics.setColor(Color.white);
		_bounds = getBounds();
		_graphics.fillRect(0, 0, _bounds.width, _bounds.height);

		// separate main graphs from reference graph
		GRDrawingAreaUtil.setColor(__da, GRColor.black);
		GRDrawingAreaUtil.drawLine(__da, 0, 10, __WIDTH, 10);

		int numGraphs = __product.getNumSubProducts();

		if (numGraphs == 0)
		{
			showDoubleBuffer(g);
			return;
		}

		int graphHeight = (int)(__GRAPHS_HEIGHT / numGraphs);
		int y = __BOTTOM_Y;

		GRDrawingAreaUtil.setColor(__da, GRColor.black);

		string num = "";
		int fontSize = 0;
		int textPos = ((int)(__WIDTH / 2));
		int selectedGraph = __parent.getSelectedGraph();

		// draw the rectangle for each graph with the number of the graph
		// in the center
		for (int i = numGraphs - 1; i >= 0; i--)
		{
			num = "" + (i + 1);
			fontSize = calculateFontSize(graphHeight - 4, graphHeight - 4, num);
			GRDrawingAreaUtil.setFont(__da, __FONT, fontSize);

			if (i == selectedGraph)
			{
				GRDrawingAreaUtil.setColor(__da, GRColor.grey50);
				GRDrawingAreaUtil.fillRectangle(__da, __X_OFFSET, y, __GRAPHS_WIDTH, graphHeight);
				GRDrawingAreaUtil.setColor(__da, GRColor.black);
			}

			GRDrawingAreaUtil.drawRectangle(__da, __X_OFFSET, y, __GRAPHS_WIDTH, graphHeight);
			GRDrawingAreaUtil.drawText(__da, "" + (i + 1), textPos, (y + graphHeight), 0, GRText.CENTER_X | GRText.TOP);
			y += graphHeight;
		}

		showDoubleBuffer(g);
	}

	}

}