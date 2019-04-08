using System;

// ProblemGutter - used for displaying line numbers and markers next to a JList in a JScrollPane.

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


	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// ProblemGutter is used for displaying line numbers and markers next to a JList in a JScrollPane.
	/// <para>
	/// When the mouse hovers over a marker, text associated with the marker is displayed. 
	/// </para>
	/// <para>
	/// The text is obtained from JList model items implementing the <code>CommandStatusProvider</code> interface.
	/// </para>
	/// <para>
	/// The margin showing the line numbers & markers may be hidden/shown by
	/// clicking in the gutter. (When collapsed, the gutter is only a few pixels wide.
	/// 
	/// </para>
	/// </summary>

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class ProblemGutter extends javax.swing.JComponent implements java.awt.event.AdjustmentListener
	public class ProblemGutter : JComponent, AdjustmentListener
	{
	  /// <summary>
	  /// GutterRowIterator encapsulates layout logic for the <code>ProblemGutter</code>. 
	  /// </summary>
	  internal class GutterRowIterator
	  {
		  private readonly ProblemGutter outerInstance;

		internal double barHeight;

		internal double y;

		internal GutterRowIterator(ProblemGutter outerInstance)
		{
			this.outerInstance = outerInstance;
			int height = getHeight();
			int rowHeight = (int) outerInstance._jList.getCellBounds(0, 0).getHeight();
			int errorHeight = outerInstance._jList.getModel().getSize() * rowHeight;
			// FIXME SAM 2007-08-16 Need to handle the following more gracefully.
			// _jList may have a cell height of zero, so initialize to non-zero.
			if (errorHeight == 0)
			{
				errorHeight = 1;
			}
			barHeight = rowHeight * Math.Min(height / (double) errorHeight, 1.0);
		}

		/// <summary>
		/// Returns next row position.  
		/// </summary>
		internal virtual void next()
		{
		  y += barHeight;
		}
	  } // eof class GutterRowIterator

	  private static string PKG = "RTi/Util/IO";

	  private static ImageIcon errorIcon = null;

	  private static ImageIcon warningIcon = null;
	  private static ImageIcon unknownIcon = null;
	  private int _iconOffset;
	  /// <summary>
	  /// JList component </summary>
	  internal JList _jList;
	  /// <summary>
	  /// Scroll pane containing the JList & ProblemGutter </summary>
	  internal JScrollPane _jScrollPane;
	  /// <summary>
	  /// Width of bar </summary>
	  private const int BAR_WIDTH = 4;
	  /// <summary>
	  /// Dimension of ProblemGutter component </summary>
	  private Dimension _dimn = new Dimension();
	  /// <summary>
	  /// Flag for whether ProblemGutter component expanded </summary>
	  private bool _isComponentExpanded = true;

	  /// <summary>
	  /// Use to make decisions about what marker to show.
	  /// For example if the last phase is RUN, then don't need to choose the marker because of a discovery issue.
	  /// </summary>
	  private CommandPhaseType lastCommandPhaseType = null;

	  /// <summary>
	  /// Creates a JComponent displaying line numbers and problem markers.
	  /// </summary>
	  /// <param name="jList"> <code>JList component</code> </param>
	  /// <param name="scroller"> <code> JScrollPane component </code> </param>
	  public ProblemGutter(JList jList, JScrollPane scroller)
	  {
			string routine = this.GetType().Name + ".ProblemGutter";
		  _jList = jList;
		  _jScrollPane = scroller;

		  if (errorIcon == null)
		  {
			  try
			  {
				  //TODO: dre refactor
				  errorIcon = JGUIUtil.loadIconImage(PKG + "/error.gif");
				  if (errorIcon == null)
				  {
					  URL url = this.GetType().getResource("error.gif");
					  errorIcon = new ImageIcon(url);
					  if (errorIcon.getImageLoadStatus() != MediaTracker.COMPLETE)
					  {
						  Message.printWarning(2, routine, "Unable to load icon using getResource()");
					  }
				  }
			  }
			  catch (Exception)
			  {
				  Message.printWarning(2, routine, "Unable to load icon using \"" + PKG + "/error.gif");
				  //Message.printWarning ( 2, "", "Unable to load icon " + "error.gif");

			  }
		  }
		  if (warningIcon == null)
		  {
			  try
			  {
				 warningIcon = JGUIUtil.loadIconImage(PKG + "/warning.gif");
				  //URL url = this .getClass().getResource("warning.gif" );
				  //warningIcon = new ImageIcon(url);
			  }
			  catch (Exception)
			  {
				  Message.printWarning(2, routine, "Unable to load icon using \"" + PKG + "/warning.gif");
				//Message.printWarning ( 2, "", "Unable to load icon " + "warning.gif");

			  }
		  }
		  if (unknownIcon == null)
		  {
			  try
			  {
				  unknownIcon = JGUIUtil.loadIconImage(PKG + "/unknown.gif");
				  //URL url = this .getClass().getResource("unknown.gif" );
				  //unknownIcon = new ImageIcon(url);
			  }
			  catch (Exception)
			  {
				  Message.printWarning(2, routine, "Unable to load icon using \"" + PKG + "/unknown.gif");
				//Message.printWarning ( 2, "", "Unable to load icon " + "unknown.gif");

			  }
		  }

		  // Add listener to hide/show problemGutter
		  addMouseListener(new MouseAdapterAnonymousInnerClass(this));

		  addMouseMotionListener(new MouseMotionAdapterAnonymousInnerClass(this));
		  scroller.getVerticalScrollBar().addAdjustmentListener(this);

		  Font f = jList.getFont();
		  Font italicFont = f.deriveFont(Font.ITALIC);
		  setFont(italicFont);
		  setBorder(BorderFactory.createRaisedBevelBorder());
	  }

		private class MouseAdapterAnonymousInnerClass : java.awt.@event.MouseAdapter
		{
			private readonly ProblemGutter outerInstance;

			public MouseAdapterAnonymousInnerClass(ProblemGutter outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void mouseClicked(MouseEvent e)
			{
			  if (!outerInstance.inBounds(e.getX(), e.getY()))
			  {
				  return;
			  }
			  if (outerInstance._isComponentExpanded)
			  {
				  outerInstance.hideBar();
			  }
			  else
			  {
				  outerInstance.showBar();
			  }
			}
		}

		private class MouseMotionAdapterAnonymousInnerClass : MouseMotionAdapter
		{
			private readonly ProblemGutter outerInstance;

			public MouseMotionAdapterAnonymousInnerClass(ProblemGutter outerInstance)
			{
				this.outerInstance = outerInstance;
			}

				/// <summary>
				/// Detects when the mouse is over a marker and displays the
				/// marker text.
				/// </summary>
			public void mouseMoved(MouseEvent e)
			{
				// Protect against empty list
				if (outerInstance._jList.getModel().getSize() < 1)
				{
					return;
				}
				int index = outerInstance.findError(e.getPoint());
				ListModel dataModel = outerInstance._jList.getModel();
				if (index > -1 && index < dataModel.getSize())
				{
					CommandStatusProvider csp = (CommandStatusProvider) dataModel.getElementAt(index);
					int highestSeverity = CommandStatusProviderUtil.getHighestSeverity(csp);
					if (highestSeverity != CommandStatusType.UNKNOWN.getSeverity())
					{
						// Change pointer to indicate on marker
						((JComponent)e.getComponent()).setCursor(Cursor.getPredefinedCursor(Cursor.HAND_CURSOR));
						outerInstance.showMarkerText(e);
					}
					else
					{
						((JComponent)e.getComponent()).setCursor(Cursor.getPredefinedCursor(Cursor.DEFAULT_CURSOR));
						((JComponent)e.getComponent()).setToolTipText(null);
					}
				}
				else
				{
					((JComponent)e.getComponent()).setToolTipText(null);
					((JComponent)e.getComponent()).setCursor(Cursor.getPredefinedCursor(Cursor.DEFAULT_CURSOR));
				}

			}
		}

	  public virtual void adjustmentValueChanged(AdjustmentEvent ae)
	  {
		_jScrollPane.validate();
	  }

	  /// <summary>
	  /// Returns the index 
	  /// </summary>
	  private int findError(Point p)
	  {
		GutterRowIterator it = new GutterRowIterator(this);
		int idx = 0;

		while (it.y < getHeight())
		{
			if (p.y >= it.y && p.y < it.y + it.barHeight)
			{
				return idx;
			}
			it.next();
			idx++;
		}
		return -1;
	  }

	  /// <summary>
	  /// Returns the ImageIcon for the marker.
	  /// 
	  /// The highest severity for the item is used to determine which
	  /// marker to use. </summary>
	  /// <param name="index"> JList model item index
	  /// @return </param>
	  private ImageIcon getMarker(int index)
	  {
		  if (index > _jList.getModel().getSize() - 1)
		  {
			return null;
		  }
		object o = _jList.getModel().getElementAt(index);
		CommandStatusProvider csp;
		ImageIcon markerIcon = null;

		//TODO: dre: refactor to CommandStatusProviderUtil
		// but then CommandStatusProviderUtil would need to
		// deal with icons
		if (o is CommandStatusProvider)
		{
			csp = (CommandStatusProvider)o;
			CommandStatus cs = csp.getCommandStatus();
			markerIcon = null;
			if (cs != null)
			{
				CommandStatusType severity = CommandStatusType.UNKNOWN;
				// Get the highest severity considering the command phase
				// TODO sam 2017-04-13 need to get this worked out
				bool legacyLogic = true;
				if (legacyLogic)
				{
					// Severity is the max on the command.
					// - can be an issue if discovery severity is higher than the run severity,
					//   for example dynamic data not found at discovery but created later
					// - in this case the run severity (SUCCESS) should be used.
					severity = CommandStatusUtil.getHighestSeverity(csp);
				}
				else
				{
					if (this.lastCommandPhaseType == CommandPhaseType.INITIALIZATION)
					{
						// Show all issues
						severity = CommandStatusUtil.getHighestSeverity(csp);
					}
					else if (this.lastCommandPhaseType == CommandPhaseType.DISCOVERY)
					{
						// Show all issues
						severity = CommandStatusUtil.getHighestSeverity(csp);
					}
					else if (this.lastCommandPhaseType == CommandPhaseType.RUN)
					{
						// Just ran and have not done more editing so show only run issues
						CommandPhaseType[] commandPhaseTypes = new CommandPhaseType[] {CommandPhaseType.RUN};
						severity = CommandStatusUtil.getHighestSeverity(csp, commandPhaseTypes);
					}
				}

				// Determine icon for marker
			   if (severity.Equals(CommandStatusType.WARNING))
			   {
				   //TODO: dre refactor so icons are "provided"
				  markerIcon = warningIcon;
			   }
			   else if (severity.Equals(CommandStatusType.FAILURE))
			   {
				  markerIcon = errorIcon;
			   }
			   // TODO SAM 2007-08-16 Remove when all commands have
			   // been updatd - don't want to deceive users into thinking
			   // that error handling is updated everywhere
			   else if (severity.Equals(CommandStatusType.UNKNOWN))
			   {
				 markerIcon = unknownIcon;
			   }
			   // No marker for SUCCESS - only show problems
			}
		}
		else
		{
			markerIcon = unknownIcon;
		}
		return markerIcon;
	  }

	  /// <summary>
	  /// Returns text associated with marker formatted as HTML.
	  /// </summary>
	  /// <param name="index"> index of item in JList </param>
	  /// <returns> text associated with mark formatted as HTML </returns>
	  private string getMarkerText(int index)
	  {
		string markerText = "No Status Available";
		  if (_jList.getModel().getSize() == 0)
		  {
			  return null;
		  }

		object o = _jList.getModel().getElementAt(index);
		if (o is CommandStatusProvider)
		{
			CommandStatusProvider csp = (CommandStatusProvider)o;
			markerText = CommandStatusUtil.getCommandLogHTML(csp);
		}

		return markerText;
	  }

	  /// <summary>
	  /// Returns component width.
	  /// <para>
	  /// As the number of digits in the line number
	  /// increases so will the required width.
	  /// </para>
	  /// </summary>
	  /// <returns> width in pixels </returns>
		private int getMyWidth()
		{
		  FontMetrics fm = _jList.getFontMetrics(_jList.getFont());

		  int lineNumberWidth = fm.stringWidth(getVisibleEndLine() + "");
		  _iconOffset = BAR_WIDTH + 4 + lineNumberWidth;
		  //int wWidth= warningIcon.getIconWidth();
		  return _isComponentExpanded ? lineNumberWidth + warningIcon.getIconWidth() + 4 + BAR_WIDTH : BAR_WIDTH;
		}

		/// <summary>
		/// Returns preferred size of ProblemGutter.
		/// </summary>
		/// <returns> preferred size </returns>
		public virtual Dimension getPreferredSize()
		{
		  _dimn.width = getMyWidth();
		  _dimn.height = _jList.getHeight();
		  return _dimn;
		}

	  /// <summary>
	  /// Returns index of last visible item.
	  /// </summary>
	  /// <returns> last visible item index </returns>
	  private int getVisibleEndLine()
	  {
		int lastLine = _jList.getLastVisibleIndex();
		return lastLine;
	  }

	  /// <summary>
	  /// Returns index of first visible item. </summary>
	  /// <returns> first visible item index
	  /// 
	  /// private int getVisibleStartLine()
	  /// {
	  ///  int firstLine = _jList.getFirstVisibleIndex();
	  ///  return firstLine;
	  /// } </returns>

	  /// <summary>
	  /// Collapses ProblemGutter to minimum width.
	  /// </summary>
	  private void hideBar()
	  {
		  // TODO SAM 2008-10-01 For now disable the hiding because it causes usability problems.
		  bool barIsHideable = false;
		  if (barIsHideable)
		  {
			  _isComponentExpanded = false;
			  _jScrollPane.setRowHeaderView(this);
		  }
	  }

	  private bool inBounds(int x, int y)
	  {
		//    if (showing)
		//      return x > (d.width - BAR);
		return true;
	  }

	  public virtual void paint(Graphics g)
	  {
		// draw the border one pixel bigger in height so bottom left bevel
		// can look like it doesn't turn.
		// we will paint over the top and bottom center portions of the border
		// in paintNumbers
		getBorder().paintBorder(this, g, 0, 0, _dimn.width, _dimn.height + 1);

		//  Insets insets = getBorder().getBorderInsets(this);
		//  g.drawRect(0, 0, getWidth()- insets.right-1, getHeight() - insets.bottom+1);
		if (_isComponentExpanded)
		{
			paintNumbers(g);
			paintMarkers(g);
		}
	  }

	  /// <summary>
	  /// Paints Markers.
	  /// </summary>
	  /// <param name="g"> </param>
	  private void paintMarkers(Graphics g)
	  {
		if (_jList.getModel().getSize() < 1)
		{
			  return;
		}
		ImageIcon markerIcon;
		Rectangle r = g.getClipBounds();
		Insets insets = getBorder().getBorderInsets(this);
		// adjust the clip
		// trim the width by border insets
		r.width -= insets.right + insets.left;
		// slide the clip over by the left insets
		r.x += insets.left;
		// we never trimmed the top or bottom.
		// this will paint over the border.
		// ((Graphics2D) g).fill(r);

		int cellHeight = 10; // Provide a default
		if (_jList.getModel().getSize() > 0)
		{
			cellHeight = _jList.getCellBounds(0,0).height;
		}

		int y = 0; // cellHeight;

		g.setColor(UIManager.getColor("Label.foreground"));

		// for (int i = (int) Math.floor(y / h) + 1; i <= max + 1; i++)
	   // int firstLine = _jList.getFirstVisibleIndex();
	   // int lastLine = _jList.getLastVisibleIndex();
		int lastLine = _jList.getModel().getSize();

		for (int i = 0; i < lastLine; i++)
		{

			markerIcon = getMarker(i);
			if (markerIcon != null)
			{
				//g.drawString(i + "", insets.left, y + ascent);
				markerIcon.paintIcon(this, g, _iconOffset,y);
			}
			// y += h;
			y += cellHeight;
		}
	  } // eof paintMarkers

	  /// <summary>
	  /// Paints line numbers. </summary>
	  /// <param name="g"> </param>
		private void paintNumbers(Graphics g)
		{
		  g.setColor(UIManager.getColor("InternalFrame.activeTitleBackground"));
		  //    g.setColor(UIManager.getColor("control"));
		  Rectangle r = g.getClipBounds();
		  Font f = _jList.getFont();
		  Font italicFont = f.deriveFont(Font.ITALIC);
		  g.setFont(italicFont);
		  FontMetrics fm = g.getFontMetrics(f);
		  Insets insets = getBorder().getBorderInsets(this);

		  // adjust the clip
		  // trim the width by border insets
		  r.width -= insets.right + insets.left;
		  // slide the clip over by the left insets
		  r.x += insets.left;
		  // we never trimmed the top or bottom.
		  // this will paint over the border.
		 // ((Graphics2D) g).fill(r);

		  int cellHeight = 20; // default to something non-zero
		  if (_jList.getModel().getSize() > 0)
		  {
			  cellHeight = _jList.getCellBounds(0,0).height;
		  }
		  int ascent = fm.getAscent();

		  int h = cellHeight;
		  int y = (int)(r.getY() / h) * h;
		  int max = (int)(r.getY() + r.getHeight()) / h;

		  g.setColor(UIManager.getColor("Label.foreground"));

		  for (int i = (int) Math.Floor(y / h) + 1; i <= max + 1; i++)
		  {
			  int xx = _iconOffset - fm.stringWidth("" + (i));
			  g.drawString(i + "", xx, y + ascent);
			  y += cellHeight;
		  }
		}

		/// <summary>
		/// Set the last command phase that for the component.
		/// </summary>
		public virtual void setLastCommandPhase(CommandPhaseType lastCommandPhaseType)
		{
			this.lastCommandPhaseType = lastCommandPhaseType;
		}

		/// <summary>
		/// Convenience method for expanding the Problem Gutter.
		/// </summary>
	  private void showBar()
	  {
		  if (!_isComponentExpanded)
		  {
			  _isComponentExpanded = true;
			  _jScrollPane.setRowHeaderView(this);
		  }
	  }

	  /// <summary>
	  /// Displays text associated with marker using a ToolTip.
	  /// </summary>
	  /// <param name="e"> mouse event  </param>
	  private void showMarkerText(MouseEvent e)
	  {
		string markerText = null;

		int index = findError(e.getPoint());
		if (index > -1 && index < _jList.getModel().getSize())
		{
			markerText = getMarkerText(index);
			//  showProblem( "Problem",markerText);
		}
	   // System.out.println("MarkerClass: "+e.getSource().getClass().getName());
		((JComponent)e.getComponent()).setToolTipText(markerText);
	  }
	}

}