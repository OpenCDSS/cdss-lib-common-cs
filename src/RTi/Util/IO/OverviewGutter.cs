using System.Collections.Generic;

// OverviewGutter - provides an overview of Problems associated with the

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

namespace RTi.Util.IO
{


	/// <summary>
	/// Provides an overview of Problems associated with the
	/// MessageModel. 
	/// <para>
	/// Problem markers are drawn relative to the position where they occur
	/// in the model.
	/// </para>
	/// <para>
	/// When a gutter marker is clicked, the associated
	/// index in the list will be scrolled to and selected.
	/// </para>
	/// </summary>
	public class OverviewGutter : JComponent
	{
	  private const long serialVersionUID = 1L;

	  /// <summary>
	  /// Height of marker </summary>
	  private const int MARKER_HEIGHT = 4;
	  /// <summary>
	  /// Margin between Component edge & marker </summary>
	  private const int LEFT_MARGIN = 2;
	  /// <summary>
	  /// Component's preferred width </summary>
	  private const int PREFERRED_WIDTH = 12;
	  /// <summary>
	  /// Data model associated with OverviewGutter </summary>
	  private readonly ListModel _dataModel;
	  /// <summary>
	  /// Height of gutter used for marker positioning (it is MARKER_HEIGHT shorter </summary>
	  internal int _gutterHeight;
	  /// <summary>
	  /// Maintains the list of markers </summary>
	  private IList<MarkerInstance> _markerInstances = new List<MarkerInstance>();
	  /// <summary>
	  /// JList associated with OverviewGutter </summary>
	  private readonly JList _list;

	  /// <summary>
	  /// Creates an overview gutter for the specified data model & JList.
	  /// <para>
	  /// Problems associated with data model items are indicated visually by
	  /// colored markers drawn in the gutter.
	  /// 
	  /// </para>
	  /// </summary>
	  /// <param name="dataModel"> Associated data model </param>
	  /// <param name="list">  Associated JList </param>
	  public OverviewGutter(ListModel dataModel, JList list)
	  {
		  _dataModel = dataModel;
		  _list = list;

		  setName("overviewGutter");

		  // Detect mouse clicks on markers & ensure associated JList item is visible
		  addMouseListener(new MouseAdapterAnonymousInnerClass(this));

		  // Display marker text when pointer is over a marker
		  addMouseMotionListener(new MouseMotionAdapterAnonymousInnerClass(this));

		   // really only the width matters
		  setPreferredSize(new Dimension(PREFERRED_WIDTH, 16));
		  setToolTipText(null);
	  }

		private class MouseAdapterAnonymousInnerClass : MouseAdapter
		{
			private readonly OverviewGutter outerInstance;

			public MouseAdapterAnonymousInnerClass(OverviewGutter outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void mouseClicked(MouseEvent me)
			{
				int index = outerInstance.findMarkerInstance(me.getPoint());
				if (index >= 0)
				{
					outerInstance._list.ensureIndexIsVisible(index);
					outerInstance._list.setSelectedIndex(index);
				}
			}

			public void mouseExited(MouseEvent e)
			{
			  setToolTipText(null);
			}
		}

		private class MouseMotionAdapterAnonymousInnerClass : MouseMotionAdapter
		{
			private readonly OverviewGutter outerInstance;

			public MouseMotionAdapterAnonymousInnerClass(OverviewGutter outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void mouseMoved(MouseEvent e)
			{
			  string markerText = null;

			  int index = outerInstance.findMarkerInstance(e.getPoint());

			  if (index > -1 && index < outerInstance._dataModel.getSize())
			  {
				  CommandStatusProvider csp = (CommandStatusProvider) outerInstance._dataModel.getElementAt(index);
				  int highestSeverity = CommandStatusProviderUtil.getHighestSeverity(csp);
				  if (highestSeverity != CommandStatusType.UNKNOWN.getSeverity())
				  {

					  // Change pointer to indicate on marker
					  ((JComponent)e.getComponent()).setCursor(Cursor.getPredefinedCursor(Cursor.HAND_CURSOR));
					  markerText = outerInstance.getMarkerText(index);
				  }
			  }
			  else
			  {
				  ((JComponent)e.getComponent()).setCursor(Cursor.getPredefinedCursor(Cursor.DEFAULT_CURSOR));
			  }
			  ((JComponent)e.getComponent()).setToolTipText(markerText);
			}
		}

	  private int findMarkerInstance(Point point)
	  {
		int index = -1;

		if (_markerInstances.Count > 0)
		{
			MarkerInstance markerInstance;

			// Determine markerInstance containing point
			for (int i = 0; i < _markerInstances.Count; i++)
			{
			  markerInstance = (MarkerInstance)_markerInstances[i];
			  if (markerInstance.getRect().contains(point))
			  {
				  index = markerInstance.getIndex();
			  }
			}
		}
		return index;
	  }

	  /// <summary>
	  /// Returns the gutter pixel position (0+) corresponding to a JList index.
	  /// This may be an estimate if the list is long compared to the height of the
	  /// gutter, resulting in round off in integer math.
	  /// </summary>
	  /// <param name="i"> JList data model index </param>
	  /// <returns> Pixel position in gutter corresponding to JList data model position,
	  /// assuming that JList index is centered on corresponding gutter position. </returns>
	  private int findGutterPosition(int index)
	  {
		  if ((_gutterHeight <= 0) || (_dataModel.getSize() == 0))
		  {
			  return -1;
		  }
		  // Order is important to prevent integer math roundoff to zero.
		  int y = (_gutterHeight * index) / _dataModel.getSize();
		  //Message.printStatus ( 1,"findGutterPosition","index=" + index + " height="+ getHeight() + " size="+_dataModel.getSize() + " y="+y);
		  return y;
	  }

	  /// <summary>
	  /// Paints the component on the screen
	  /// <para>
	  /// Markers are drawn in the overview gutter relative to their position 
	  /// in the list. A marker for the first item should be at the top, while
	  /// a marker for the last item of the list appears at the bottom of the
	  /// gutter.
	  /// </para>
	  /// <para>
	  /// Markers are drawn from y downward to y + MARKER_HEIGHT, therefore
	  /// for positioning the height 
	  /// </para>
	  /// </summary>
	  protected internal virtual void paintComponent(Graphics g)
	  {
		  base.paintComponent(g);
		  _markerInstances.Clear();

		  if (_list.getModel().getSize() == 0)
		  {
			  return;
		  }

		   _gutterHeight = getHeight() - MARKER_HEIGHT;


		  // paint background to show gutter area
		  // g.setColor(Color.gray);
		  // g.fillRect(0,0,getWidth(),getHeight());

		  // Find out which markers to draw
		  findMarkers();
		  // Draw markers
		  drawMarkers(g);
	  }

	  private void drawMarkers(Graphics g)
	  {
		Color color;
		/*
		 * Loop thru markers & draw
		 */

		if (_markerInstances.Count > 0)
		{
			MarkerInstance markerInstance;

			for (int i = 0; i < _markerInstances.Count; i++)
			{
				markerInstance = (MarkerInstance)_markerInstances[i];
				int severity = markerInstance.getSeverity();
				// TODO:dre refactor colors to CommandStatusProviderUtil?
				if (severity == CommandStatusType.WARNING.getSeverity())
				{
					  color = Color.yellow;
				}
				else if (severity == CommandStatusType.FAILURE.getSeverity())
				{
					  color = Color.red;
				}
				else
				{
					color = null;
				}
				Rectangle rect = markerInstance.getRect();

				// Paint a filled colored rectangle with black outline.  Center
				// vertically on the y coordinate.
				g.setColor(color);
				g.fillRect(rect.x, rect.y, rect.width, MARKER_HEIGHT);
				g.setColor(Color.BLACK);
				g.drawRect(rect.x, rect.y, rect.width, MARKER_HEIGHT);
			}
		}
	  }

	  private void findMarkers()
	  {
		int markerWidth = getWidth() - 2 * LEFT_MARGIN;
		/*
		 * Loop thru data model items detecting problems,
		 * creating a new MarkerInstance for each problem.
		 */
		for (int i = 0; i < _dataModel.getSize(); i++)
		{
			// Get the severity associated with an item in the list model.
			int severity = getMarker(i); // CommandStatus highest severity

			if (severity == CommandStatusType.WARNING.getSeverity() || severity == CommandStatusType.FAILURE.getSeverity())
			{

				// Find position in gutter corresponding to item
				int y = findGutterPosition(i);
				Rectangle rect = new Rectangle(LEFT_MARGIN, y, markerWidth, MARKER_HEIGHT);
				MarkerInstance markerInstance = new MarkerInstance(this, i, severity, rect);
				_markerInstances.Add(markerInstance);
			}
		}
	  }

	 /// <summary>
	 /// Returns text to be displayed in tool tip.
	 /// <para>
	 /// Called by ToolTipMgr to get tool tip text.
	 /// 
	 /// </para>
	 /// </summary>
	 /// <param name="event"> Mouse event causing invocation </param>
	 /// <returns> Text to be displayed in tooltip or null indicating no tooltip </returns>
	  public virtual string getToolTipText(MouseEvent @event)
	  {
		string str = null;
		if (_dataModel.getSize() != 0)
		{

		//    int index = findListIndex(event.getPoint());
			int index = findMarkerInstance(@event.getPoint());
			if (index > -1)
			{
				str = getMarkerText(index);
	//            try
	//              {
	//                CommandStatusProvider csp = (CommandStatusProvider) _dataModel
	//                        .getElementAt(index);
	//                str = CommandStatusProviderUtil.getCommandLogHTML(csp);
	//              }
	//            catch (ClassCastException e)
	//              {
	//                Message.printWarning ( 2, "",
	//                        "Item #:"
	//                                + index
	//                                + " does not implement ComandStatusProvider interface"
	//                                + "\n  item.toString(): "
	//                                + _dataModel.getElementAt(index)
	//                                        .toString() + "\n\n" + e);
	//                str = new String("NotACommandStatusProvider");
	//              }
			}
		}

		return str;
	  }

	  /// <summary>
	  /// Returns the severity the marker.
	  /// 
	  /// The highest severity for the item is used to determine which
	  /// marker to use. </summary>
	  /// <param name="index"> JList model item index </param>
	  /// <returns> severity index </returns>
	  private int getMarker(int index)
	  {
		  int markerIndex = 0;
		  CommandStatusProvider csp;

		  object o = _dataModel.getElementAt(index);

		  if (o is CommandStatusProvider)
		  {
			  csp = (CommandStatusProvider)o;
			  markerIndex = CommandStatusProviderUtil.getHighestSeverity(csp);
		  }
		  return markerIndex;
	  }

	  /// <summary>
	  /// Returns the text (as HTML) associated with the specified data model
	  ///  index.
	  /// </summary>
	  /// <param name="index"> Index in data model
	  /// @return </param>
	  private string getMarkerText(int index)
	  {
		CommandStatusProvider csp;
		string markerText = "";
		object o = _dataModel.getElementAt(index);

		if (o is CommandStatusProvider)
		{
			csp = (CommandStatusProvider)o;
			markerText = CommandStatusUtil.getCommandLogHTML(csp);
		}
		return markerText;
	  }
	  /// <summary>
	  /// Maintains the attributes of a marker.
	  /// 
	  /// @author dre
	  /// </summary>
	  private class MarkerInstance
	  {
		  private readonly OverviewGutter outerInstance;

		/// <summary>
		/// marker severity </summary>
		internal int _severity;
		/// <summary>
		/// location & size of marker in gutter </summary>
		internal Rectangle _rect;
		/// <summary>
		/// Index of associated item in model </summary>
		internal int _modelIndex;

		/// <summary>
		/// Creates an instance of a marker.
		/// </summary>
		/// <param name="modelIndex"> index of item in model </param>
		/// <param name="severity"> </param>
		/// <param name="rect"> rectangle enclosing marker </param>
		public MarkerInstance(OverviewGutter outerInstance, int modelIndex, int severity, Rectangle rect)
		{
			this.outerInstance = outerInstance;
		 _modelIndex = modelIndex;
		 _severity = severity;
		 _rect = rect;
		}

		/// <summary>
		/// Returns the model index associated with the marker </summary>
		/// <returns> Index of item in data model </returns>
		public virtual int getIndex()
		{
		  return _modelIndex;
		}

		/// <summary>
		/// Returns the rectangle associated with the marker </summary>
		/// <returns> rectance enclosing marker </returns>
		public virtual Rectangle getRect()
		{
		  return _rect;
		}

		/// <summary>
		/// Returns the marker's severity </summary>
		/// <returns> Return the severity associated with the marker </returns>
		public virtual int getSeverity()
		{
		  return _severity;
		}
	  }

	}


}