// AnnotatedCommandJList - provides a JList with line numbers and visual problem markers

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
	/// Provides a JList with line numbers and visual problem markers.
	/// 
	/// @author dre
	/// </summary>
	public class AnnotatedCommandJList : JPanel
	{
	  private const long serialVersionUID = 1L;
	  /// <summary>
	  /// GUI component displaying problem overview of entire list </summary>
	  private OverviewGutter _overviewGutter;
	  private ProblemGutter _problemGutter;
	  /// <summary>
	  /// Extended JList </summary>
	  private JList _jList;
	  /// <summary>
	  /// JScrollPane containing JList </summary>
	  private JScrollPane _jScrollPane;

	  private ListModel _dataModel;

	  /// <summary>
	  /// The last command phase type that applies.
	  /// This is used so that if there are discovery issues they are only shown during discovery, and not after running.
	  /// Dynamic command files often have discovery issues that are cleared up after running.
	  /// </summary>
	  private CommandPhaseType lastCommandPhaseType = null;

	  /// <summary>
	  /// Creates a component for viewing a list with line numbers &
	  /// markers.
	  /// </summary>
	  public AnnotatedCommandJList()
	  {
		  initialize();
	  }
	  /// <summary>
	  /// Creates a component for viewing a list with line numbers &
	  /// markers.
	  /// <para>
	  /// The compound component consists of a JScrollPane containing
	  /// a JList, ProblemGutter & OverviewGutter.
	  /// </para>
	  /// <para>
	  /// Markers can only be displayed for data model items implementing the
	  /// <code>CommandStatusProvider</code> interface.
	  /// 
	  /// </para>
	  /// </summary>
	  /// <param name="dataModel"> - data model to be displayed </param>
	  public AnnotatedCommandJList(ListModel dataModel)
	  {
		  _dataModel = dataModel;
		  initialize();
		  _jList.setModel(dataModel);

		  // Listen for data model changes
		  //dataModel.addListDataListener(new MyListDataListener());
	  }

	  /// <summary>
	  /// Initialize and connect components.
	  /// </summary>
	  private void initialize()
	  {
		setLayout(new BorderLayout());
		// Override JList's getToolTipText method to return tooltip
		_jList = new JList();
	 //Uncomment the semicolon after JList() above & uncomment the following
	 //block to see JList tooltips    
	//    {
	//      static final long serialVersionUID = 1L;
	//
	//      /**
	//       * Returns text to be displayed in tool tip.
	//       * <p>
	//       * Called by ToolTipMgr to get tool tip text
	//       */
	//       public String getToolTipText(MouseEvent event)
	//       {
	//   	     if (getModel().getSize() != 0)
	//    	   {
	//         // Get item index
	//         int index = locationToIndex(event.getPoint());
	//
	//         // Get item
	//
	//         Object item = getModel().getElementAt(index);
	//          return CommandStatusProviderUtil.getCommandLogHTML(item);
	//         }
	//         else
	//         {
	//        	 return new String("");
	//         }
	//       }
	//    };

		// Enable horizontal scrolling
		// FontMetrics metrics = getFontMetrics(_jList.getFont());
		// System.out.println(_jList.getFont().toString());
		// Set a fixed cell height to avoid calling the ListCellRenderer
		// getPreferredSize() method
	   // _jList.setFixedCellHeight(metrics.getHeight());
	   //dre _jList.setCellRenderer(new HorzScrollListCellRenderer());
		_jList.setPrototypeCellValue("gjqqyAZ");
		_jList.setFixedCellWidth(-1);
		_jScrollPane = new JScrollPane(_jList);
		add(_jScrollPane, BorderLayout.CENTER);

		_jScrollPane.getViewport().addChangeListener(new ChangeListenerAnonymousInnerClass(this));

		// Provide line numbers and markers
		_problemGutter = new ProblemGutter(_jList, _jScrollPane);
		_jScrollPane.setRowHeaderView(_problemGutter);

		// Get the size of the vertical scrollbar arrow, and the "lowerright" componoent in the JScroll pane
		// in order to compute the vertical size of wasted space.  This space is passed to the overview gutter
		// constructor and is used to offset computations of the problem indicators.
		//Message.printStatus_jScrollPane.get

		// Provide an overview of markers
		_overviewGutter = new OverviewGutter(_dataModel, _jList);
		add(_overviewGutter, BorderLayout.EAST);
	  } // eof initialize()

	  private class ChangeListenerAnonymousInnerClass : ChangeListener
	  {
		  private readonly AnnotatedCommandJList outerInstance;

		  public ChangeListenerAnonymousInnerClass(AnnotatedCommandJList outerInstance)
		  {
			  this.outerInstance = outerInstance;
		  }

		  public void stateChanged(ChangeEvent e)
		  {
		  e.getSource();
		  outerInstance._jList.repaint();
		  }
	  }

	  /// <summary>
	  /// Returns the JList component.
	  /// 
	  /// @return
	  /// </summary>
	 public virtual JList getJList()
	 {
		 return _jList;
	 }

	/// <summary>
	/// Sets the font for the JList
	/// </summary>
	/// <param name="font"> font to be used </param>
	 public virtual void setFont(Font font)
	 {
	   if (_jList != null)
	   {
		   _jList.setFont(font);
	   }
	 }

	 /// <summary>
	 /// Set the last command phase that for the component.
	 /// </summary>
	 public virtual void setLastCommandPhase(CommandPhaseType lastCommandPhaseType)
	 {
		 this.lastCommandPhaseType = lastCommandPhaseType;
		 this._problemGutter.setLastCommandPhase(lastCommandPhaseType);
	 }
	} // AnnotatedList

	/// <summary>
	/// Provides a ListCellRenderer for JList, enabling a functional 
	/// JScrollPane horizontal scroll bar.
	/// 
	/// By default Swing will show JList items with an ellipsis if they are
	/// too long to display in the available space. Therefore JScrollpane's
	/// horizontal scroll bar is not activated. Or, if it is displayed it does
	/// not display the knob.
	/// 
	/// This behaviour can be changed to make the horizontal scroll bar
	/// functional by providing the JList a ListCellRenderer which avoids
	/// using the ellipsis i.e.  SwingUtilities.layoutCompoundLabel(..)
	/// (And the private layoutCompoundLabelImpl method that does the
	/// actual implementation).
	/// <para><p>
	/// Usage:
	/// <pre>
	/// JList jList = new JList();
	/// jList.setCellRenderer(new HorzScrollListCellRenderer());
	/// // Following statement needed to cause scrollbar to show 
	/// jList.setMinimumSize(300,100);
	/// </pre>
	/// 
	/// <b>setMinimumSize() is crucial to scroll bar showing, though I haven't
	/// researched why. I suspect it somehow triggers code in
	/// JScrollPane.
	/// </para>
	/// </summary>

	internal class HorzScrollListCellRenderer : JPanel, ListCellRenderer
	{
	  private const long serialVersionUID = 1L;
	  private object _currentValue;
	  private JList _currentList;
	  protected internal static Border _noFocusBorder;
	  private bool _isSelected;
	  /// <summary>
	  /// Creates a HorzListCellRenderer for JList that avoids the use of
	  /// ellipsis when the rendering width is insufficient. This is necessary
	  /// if a horizontal scroll bar is to be employed for displaying the text.
	  /// </summary>
	  public HorzScrollListCellRenderer()
	  {
	  }

	  public virtual Component getListCellRendererComponent(JList list, object value, int index, bool isSelected, bool cellHasFocus)
	  {
		_currentValue = value;
		_currentList = list;
		_isSelected = isSelected;
		_noFocusBorder = new EmptyBorder(1, 1, 1, 1);
		setBorder((cellHasFocus) ? UIManager.getBorder("List.focusCellHighlightBorder") : _noFocusBorder);
		return this;
	  }

	  public virtual void paintComponent(Graphics g)
	  {
		// draw text
		//int stringLen = g.getFontMetrics().stringWidth(_currentValue.toString());
		//int ht = _currentList.getFixedCellHeight();

		// g.setColor(currentList.getBackground());
		g.setColor(_isSelected?_currentList.getSelectionBackground(): _currentList.getBackground());
		g.fillRect(0,0,getWidth(),getHeight());

		getBorder().paintBorder(this, g, 0, 0, getWidth(), getHeight());

		g.setColor(_currentList.getForeground());
		g.setColor(_isSelected?_currentList.getSelectionForeground(): _currentList.getForeground());
		//g.drawString(currentValue.toString(),0,ht/2);
	//    g.drawString(currentValue.toString(), 0, 
	//            g.getFontMetrics().getLeading()
	//            +g.getFontMetrics().getAscent());
		FontMetrics fm = _currentList.getFontMetrics(_currentList.getFont());
		g.drawString(_currentValue.ToString(), 0, fm.getLeading() + fm.getAscent());
	  }

	  public virtual Dimension getPreferredSize()
	  {
		//TODO: Investigate why only first JList item used in determining width
		//BasicListUI.updateLayoutState call getPreferredSize()
		// look at logic around setFixed cellheight/width
		Graphics2D g = (Graphics2D)getGraphics();
		if (g != null)
		{
			//int stringLen = g.getFontMetrics().stringWidth(_currentValue.toString());
			int stringLen = _currentList.getFontMetrics(_currentList.getFont()).stringWidth(_currentValue.ToString());

			int ht = _currentList.getFixedCellHeight();
			return new Dimension(stringLen, ht);
		}
		else
		{
		  return new Dimension(150, 10);
		}
	  }

	}

}