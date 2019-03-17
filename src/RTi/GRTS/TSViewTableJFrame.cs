using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// TSViewTableJFrame - provides a table view for a list of time series and is managed by the TSViewJFrame parent

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


	using DateValueTS = RTi.TS.DateValueTS;
	using TS = RTi.TS.TS;
	using TSLimits = RTi.TS.TSLimits;
	using TSUtil = RTi.TS.TSUtil;
	using JFileChooserFactory = RTi.Util.GUI.JFileChooserFactory;
	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using JWorksheet = RTi.Util.GUI.JWorksheet;
	using JWorksheet_AbstractTableModel = RTi.Util.GUI.JWorksheet_AbstractTableModel;
	using JWorksheet_Header = RTi.Util.GUI.JWorksheet_Header;
	using ResponseJDialog = RTi.Util.GUI.ResponseJDialog;
	using SimpleFileFilter = RTi.Util.GUI.SimpleFileFilter;
	using SimpleJButton = RTi.Util.GUI.SimpleJButton;
	using SimpleJComboBox = RTi.Util.GUI.SimpleJComboBox;
	using TableModel_JFrame = RTi.Util.GUI.TableModel_JFrame;
	using DataUnits = RTi.Util.IO.DataUnits;
	using IOUtil = RTi.Util.IO.IOUtil;
	using PropList = RTi.Util.IO.PropList;
	using URLHelp = RTi.Util.Help.URLHelp;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using DataTable = RTi.Util.Table.DataTable;
	using DataTable_CellRenderer = RTi.Util.Table.DataTable_CellRenderer;
	using DataTable_TableModel = RTi.Util.Table.DataTable_TableModel;
	using TableField = RTi.Util.Table.TableField;
	using TableRecord = RTi.Util.Table.TableRecord;
	using DateTime = RTi.Util.Time.DateTime;
	using StopWatch = RTi.Util.Time.StopWatch;
	using TimeInterval = RTi.Util.Time.TimeInterval;

	/// <summary>
	/// The TSViewTableJFrame provides a table view for a list of time series and is
	/// managed by the TSViewJFrame parent.  The GUI can display multiple time series 
	/// of different interval bases and multipliers.  All the TS of the same interval
	/// base and multiplier are displayed in the same Worksheet.  Different 
	/// interval multiplier TS with the same interval base are shown in worksheets 
	/// next to other worksheets with the same interval base.  Minute TS are the top 
	/// row, the next row are Hour TS, the next are Day TS, the next are Month TS and
	/// the bottom row are Year TS.<para>
	/// If there are no TS of a particular interval base (e.g., Day), the row for 
	/// that kind of TS is not displayed.  If more than one interval base is to be 
	/// displayed on the same form, a JCheckbox will appear for each interval base
	/// so that it can be turned on or off.  If only one interval base appears on 
	/// the form, no such Checkbox will appear.
	/// </para>
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class TSViewTableJFrame extends javax.swing.JFrame implements java.awt.event.ActionListener, java.awt.event.ItemListener, java.awt.event.MouseListener, java.beans.PropertyChangeListener, java.awt.event.WindowListener
	public class TSViewTableJFrame : JFrame, ActionListener, ItemListener, MouseListener, PropertyChangeListener, WindowListener
	{

	/// <summary>
	/// The number of characters in the table double values.
	/// </summary>
	private readonly int __OUTPUT_WIDTH = 10;

	/// <summary>
	/// JButton and JPopupMenu Strings.
	/// </summary>
	private readonly string __BUTTON_CLOSE = "Close", __BUTTON_GRAPH = "Graph", __BUTTON_HELP = "Help", __BUTTON_SAVE = "Save", __BUTTON_SUMMARY = "Summary", __MENU_CALCULATE_STATISTICS = "Calculate Statistics", __MENU_COPY = "Copy", __MENU_PASTE = "Paste";

	/// <summary>
	/// Boolean to keep track of whether the main JPanel has been removed from the 
	/// Center position of the content pane and placed in the North position, or not.
	/// The main panel will be moved to the north panel when the user has deselected
	/// all the checkboxes on the form and has no TS interval types visible.
	/// </summary>
	private bool __mainJPanelNorth = false;

	/// <summary>
	/// Whether worksheets should be built in displayed in the opposite of the normal
	/// order, from minute to year instead of year to minute.
	/// </summary>
	private bool __reverseNormalOrder = false;

	/// <summary>
	/// Boolean to keep track of whether the save button should always be enabled, 
	/// no matter what.  This is only true if there is a single worksheet displayed on the GUI.
	/// </summary>
	private bool __saveAlwaysEnabled = false;

	/// <summary>
	/// The original border set up for a JScrollPane, before the border is changed
	/// because the scrollpane's worksheet was selected.
	/// </summary>
	private Border __originalScrollPaneBorder = null;

	/// <summary>
	/// A counter that tracks how many panels and checkboxes have been added to the 
	/// main panel, so that the Y placement in the grid bag can be done correctly.
	/// </summary>
	private int __panelCount = 0;

	/// <summary>
	/// Checkboxes corresponding to each TS data interval for turning on and off the table displays.
	/// </summary>
	private JCheckBox __dayJCheckBox, __hourJCheckBox, __irregularMinuteJCheckBox, __irregularHourJCheckBox, __irregularDayJCheckBox, __irregularMonthJCheckBox, __irregularYearJCheckBox, __minuteJCheckBox, __monthJCheckBox, __yearJCheckBox;

	/// <summary>
	/// The main panel into which the form components are added.
	/// </summary>
	private JPanel __mainJPanel;

	/// <summary>
	/// JPanels for holding JCheckBoxes and JWorksheets for each TS data interval.
	/// </summary>
	private JPanel __dayJPanel, __hourJPanel, __irregularMinuteJPanel, __irregularHourJPanel, __irregularDayJPanel, __irregularMonthJPanel, __irregularYearJPanel, __minuteJPanel, __monthJPanel, __yearJPanel;

	/// <summary>
	/// JScrollPanes for each of the associated ts worksheets, in order to highlight
	/// the proper JScrollPane border after a worksheet is clicked on and selected.
	/// </summary>
	private JScrollPane[] __dayScrollPanes, __hourScrollPanes, __minuteScrollPanes, __monthScrollPanes, __yearScrollPanes, __irregularMinuteScrollPanes, __irregularHourScrollPanes, __irregularDayScrollPanes, __irregularMonthScrollPanes, __irregularYearScrollPanes;

	/// <summary>
	/// The scroll pane of the last worksheet that was clicked on and selected.
	/// </summary>
	private JScrollPane __lastSelectedScrollPane = null;

	/// <summary>
	/// The status bar message text field.
	/// </summary>
	private JTextField __messageJTextField;

	/// <summary>
	/// Arrays containing all the different JWorksheets (one for each interval type) for each different TS.
	/// </summary>
	private JWorksheet[] __dayWorksheets, __hourWorksheets, __minuteWorksheets, __monthWorksheets, __yearWorksheets, __irregularMinuteWorksheets, __irregularHourWorksheets, __irregularDayWorksheets, __irregularMonthWorksheets, __irregularYearWorksheets;

	/// <summary>
	/// The last worksheet that was clicked on and selected.
	/// </summary>
	private JWorksheet __lastSelectedWorksheet = null;

	/// <summary>
	/// Properties for the table display gui.
	/// </summary>
	private PropList __props;

	/// <summary>
	/// Buttons that appear at the bottom of the GUI, for doing special operations.
	/// </summary>
	private SimpleJButton __summaryJButton, __graphJButton, __closeJButton, __saveJButton;

	private SimpleJComboBox __flagJComboBox = null;

	/// <summary>
	/// TSViewJFrame parent that displays this gui.
	/// </summary>
	private TSViewJFrame __tsviewJFrame;

	/// <summary>
	/// Table models for each of the different worksheets (one for each interval type) for each different TS.
	/// </summary>
	private TSViewTable_TableModel[] __dayModels = new TSViewTable_TableModel[0], __hourModels = new TSViewTable_TableModel[0], __minuteModels = new TSViewTable_TableModel[0], __monthModels = new TSViewTable_TableModel[0], __yearModels = new TSViewTable_TableModel[0], __irregularMinuteModels = new TSViewTable_TableModel[0], __irregularHourModels = new TSViewTable_TableModel[0], __irregularDayModels = new TSViewTable_TableModel[0], __irregularMonthModels = new TSViewTable_TableModel[0], __irregularYearModels = new TSViewTable_TableModel[0];

	/// <summary>
	/// Lists of the mouse listeners that have been set up for all of the different
	/// kinds of worksheets.  These are used in order to find which worksheet was 
	/// clicked on after a mouse press on the JScrollPane associated with a worksheet.
	/// </summary>
	private IList<IList<MouseListener>> __dayMouseListeners, __hourMouseListeners, __minuteMouseListeners, __monthMouseListeners, __yearMouseListeners, __irregularMinuteMouseListeners, __irregularHourMouseListeners, __irregularDayMouseListeners, __irregularMonthMouseListeners, __irregularYearMouseListeners;

	/// <summary>
	/// List of Time Series to be displayed in the GUI.  __tslist is set from a 
	/// list passed in to this GUI at construction, and then the other lists are
	/// formed from the TS split out of __tslist.
	/// </summary>
	private IList<TS> __day;
	private IList<TS> __hour;
	private IList<TS> __minute;
	private IList<TS> __month;
	private IList<TS> __irregularMinute;
	private IList<TS> __irregularHour;
	private IList<TS> __irregularDay;
	private IList<TS> __irregularMonth;
	private IList<TS> __irregularYear;
	private IList<TS> __tslist;
	private IList<TS> __year;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="tsviewJFrame"> Parent TSViewJFrame. </param>
	/// <param name="tslist"> List of time series to view. </param>
	/// <param name="props"> Properties for display (currently same list passed in to TSViewJFrame). </param>
	public TSViewTableJFrame(TSViewJFrame tsviewJFrame, IList<TS> tslist, PropList props)
	{
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());

		__tsviewJFrame = tsviewJFrame;
		__tslist = tslist;
		__props = props;

		string propValue = __props.getValue("TSViewTitleString");

		if (string.ReferenceEquals(propValue, null))
		{
			if ((string.ReferenceEquals(JGUIUtil.getAppNameForWindows(), null)) || JGUIUtil.getAppNameForWindows().Equals(""))
			{
				setTitle("Time Series - Table");
			}
			else
			{
				setTitle(JGUIUtil.getAppNameForWindows() + " - Time Series - Table");
			}
		}
		else
		{
			if ((string.ReferenceEquals(JGUIUtil.getAppNameForWindows(), null)) || JGUIUtil.getAppNameForWindows().Equals(""))
			{
				setTitle(propValue + " - Table");
			}
			else
			{
				setTitle(JGUIUtil.getAppNameForWindows() + " - " + propValue + " - Table");
			}
		}

		propValue = __props.getValue("TSViewTableOrder");
		if (!string.ReferenceEquals(propValue, null))
		{
			if (propValue.Equals("FineToCoarse", StringComparison.OrdinalIgnoreCase))
			{
				__reverseNormalOrder = true;
			}
		}
		setupGUI(true);
	}

	/// <summary>
	/// Handle action events from ActionListener. </summary>
	/// <param name="event"> ActionEvent to handle. </param>
	public virtual void actionPerformed(ActionEvent @event)
	{
		string command = @event.getActionCommand();

		if (command.Equals(__BUTTON_CLOSE))
		{
			__tsviewJFrame.closeGUI(TSViewType.TABLE);
		}
		else if (command.Equals(__BUTTON_GRAPH))
		{
			__tsviewJFrame.openGUI(TSViewType.GRAPH);
		}
		else if (command.Equals(__BUTTON_HELP))
		{
			URLHelp.showHelpForKey("TSView.Table");
		}
		else if (command.Equals(__BUTTON_SUMMARY))
		{
			__tsviewJFrame.openGUI(TSViewType.SUMMARY);
		}
		else if (command.Equals(__BUTTON_SAVE))
		{
			saveClicked();
		}
		else if (command.Equals(__MENU_CALCULATE_STATISTICS))
		{
			// An event was generated in a JWorksheet requesting that statistics be calculated
			// -assume that the event originated from the last selected worksheet
			//  (not sure otherwise how to get the worksheet)
			try
			{
				calculateAndDisplayStatistics(__lastSelectedWorksheet);
			}
			catch (Exception e)
			{
				Message.printWarning(1,"","Error calculating statistics (" + e + ").");
				Message.printWarning(2, "", e);
			}
		}
		else if (command.Equals(__MENU_COPY))
		{
			// TODO sam 2017-04-01 why is this event handled here?
			// - a Copy popup menu is provided in the JWorksheet by default
			// - does this ever get called?
			// - maybe this was in place for much earlier code when Copy/Paste were buttons like the above
			// - comment out for now since worksheet will handle the event
			Message.printStatus(2, "", "Copy action event disabled in TSViewTableJFrame");
			//__lastSelectedWorksheet.copyToClipboard();
		}
		else if (command.Equals(__MENU_PASTE))
		{
			// TODO sam 2017-04-01 why is this event handled here?
			// - a Paste popup menu is provided in the JWorksheet by default
			// - does this ever get called?
			Message.printStatus(2, "", "Paste action event disabled in TSViewTableJFrame");
			//__lastSelectedWorksheet.pasteFromClipboard();
		}
	}

	/// <summary>
	/// Adds worksheets of the same interval base to their panel and also adds 
	/// the appropriate check box, as long as more than one kid of interval base will be displayed on the GUI. </summary>
	/// <param name="panel"> the main panel to which to add the panels (__mainJPanel). </param>
	/// <param name="subPanel"> the panel to which to add the checkbox and worksheets. </param>
	/// <param name="intervalDescription"> a text string that concisely names the kind of interval being dealt with </param>
	/// <param name="checkBox"> the check box to (possibly) add to the panel. </param>
	/// <param name="worksheets"> the worksheets to add to the panel. </param>
	/// <param name="scrollpanes"> the scrollpanes for each worksheet </param>
	/// <param name="mouseListeners"> a list of lists used to store mouse listeners for the
	/// worksheets' scrollpanes so that the last selected worksheet can be tracked. </param>
	private void addWorksheetsToPanel(JPanel panel, string intervalDescription, JPanel subPanel, JCheckBox checkBox, JWorksheet[] worksheets, JWorksheet[] headers, JScrollPane[] scrollPanes, IList<IList<MouseListener>> mouseListeners)
	{
		if (Message.isDebugOn)
		{
			Message.printDebug(1,"TSViewTableJFrame.addWorksheetsToPanel","panel=" + panel + " intervalDescription=" + intervalDescription + " subPanel=" + subPanel + " checkBox=" + checkBox + " worksheets=" + worksheets + " headers=" + headers + " scrollPanes=" + scrollPanes + " mouseListeners=" + mouseListeners);
		}
		if (worksheets == null || worksheets.Length == 0)
		{
			// There are no worksheets for the current interval type.
			checkBox.setSelected(false);
			return;
		}

		int numWorksheets = worksheets.Length;

		// Create the panel into which these worksheets will be placed, and give it a GridBagLayout.
		GridBagLayout gbl = new GridBagLayout();
		subPanel.setLayout(gbl);

		// For each worksheet, create its scrollpane and add the scrollpane
		// to the panel.  Also add mouse listeners to the scrollpane and its
		// scrollbars so that it can be determined after a mouse click which
		// worksheet (or worksheet's scrollpane components) was clicked on.
		// Put the registered mouse listeners into a list for this purpose.
		for (int i = 0; i < numWorksheets; i++)
		{
			scrollPanes[i] = new JScrollPane(worksheets[i]);

			if (IOUtil.isUNIXMachine())
			{
				// For the reason why this is done, see JWorksheet.adjustmentValueChanged().
				scrollPanes[i].getVerticalScrollBar().addAdjustmentListener(worksheets[i]);
				scrollPanes[i].getHorizontalScrollBar().addAdjustmentListener(worksheets[i]);
			}

			// Add lots of mouse listeners, so that (hopefully) anywhere
			// a user clicks it will count as a click on the worksheet.

			worksheets[i].addMouseListener(this);

			// The header worksheet will be used to control selection 
			// events on the other worksheets for which the header worksheet is the row header.
			headers[i].setRowSelectionModelPartner(worksheets[i].getRowSelectionModel());

			worksheets[i].addHeaderMouseListener(this);

			IList<MouseListener> worksheetMouseListeners = new List<MouseListener>();
			if (scrollPanes[i] is MouseListener)
			{
				// TODO sam 2017-04-01 The following will throw a ClassCastException if now wrapped in the "if"
				// Similar checks below for the scrollbars behave the same.
				// Maybe this is old code that is ineffective and can be removed.
				// The underlying UI components check for whether the object implements MouseListener
				// and apparently these objects do not... so no listener code would be called anyway.
				worksheetMouseListeners.Add((MouseListener)scrollPanes[i]);
			}

			scrollPanes[i].addMouseListener(worksheets[i]);
			if (scrollPanes[i].getVerticalScrollBar() is MouseListener)
			{
				worksheetMouseListeners.Add((MouseListener)scrollPanes[i].getVerticalScrollBar());
			}

			scrollPanes[i].getVerticalScrollBar().addMouseListener(worksheets[i]);
			if (scrollPanes[i].getHorizontalScrollBar() is MouseListener)
			{
				worksheetMouseListeners.Add((MouseListener)scrollPanes[i].getHorizontalScrollBar());
			}

			scrollPanes[i].getHorizontalScrollBar().addMouseListener(worksheets[i]);
			JGUIUtil.addComponent(subPanel, scrollPanes[i], i, 0, 1, 1, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.NORTHWEST);

			// The next line looks weird, but is done because somewhere
			// the pointer to the worksheet that the models have is getting
			// mis-pointed.  This makes sure the models know their worksheet.
			((TSViewTable_TableModel)worksheets[i].getModel()).setWorksheet(worksheets[i]);
			mouseListeners[i] = worksheetMouseListeners;
		}

		// If only one panel of worksheet data will appear on the GUI
		// (and each GUI panel contains the data for a specific Interval base)
		// then it's not necessary to include the checkbox for selecting and
		// deselecting specific interval bases.
		if (calculateNumberOfPanels() > 1)
		{
			JGUIUtil.addComponent(panel, checkBox, 0,__panelCount++, 1, 1, 1, 0, GridBagConstraints.NONE, GridBagConstraints.NORTHWEST);
		}

		subPanel.setBorder(BorderFactory.createTitledBorder(intervalDescription + " Interval"));
			  JGUIUtil.addComponent(panel, subPanel, 0, __panelCount++, 1, 1, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.NORTHWEST);
	}

	/// <summary>
	/// Builds the mouse listener list for the given worksheet array.  The list data is not populated. </summary>
	/// <param name="worksheets"> the array for which to build the mouse listener array </param>
	/// <returns> the mouse listener list. </returns>
	private IList<IList<MouseListener>> buildMouseListeners(JWorksheet[] worksheets)
	{
		if (worksheets == null)
		{
			return null;
		}

		int numWorksheets = worksheets.Length;
		IList<IList<MouseListener>> mouseListenerList = new List<IList<MouseListener>>(numWorksheets);
		// Fill in the listeners with null so there is at least a slot.
		// - this was the behavior in legacy code that used an array of List
		for (int i = 0; i < numWorksheets; i++)
		{
			mouseListenerList.Add(null); // Other code will set the list at this position
		}
		return mouseListenerList;
	}

	/// <summary>
	/// Copy the selected cells (or all none are selected) to a new worksheet,
	/// add a column for the statistic type, calculate the statistics,
	/// and display in a new worksheet.
	/// This method handles generic JWorksheets.
	/// If more specific behavior is needed (for example time series data with potentially
	/// inconsistent missing data values), then create a JWorksheet and set the properties
	/// <pre>
	/// JWorksheet.AllowCalculateStatistics=true
	/// JWorksheet.DelegateCalculateStatistics=true
	/// </pre>
	/// The latter property will tell this class to not process the "Calculate Statistics"
	/// action event and let another registered ActionListener handle.
	/// If the Calculate Statistics functionality is enabled and not delegated,
	/// then this method is called.
	/// This code is substantially copied from JWorksheet, which was substantially guided by JWorksheet_CopyPasteAdapter.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void calculateAndDisplayStatistics(RTi.Util.GUI.JWorksheet worksheet) throws Exception
	private void calculateAndDisplayStatistics(JWorksheet worksheet)
	{
		string routine = "calculateAndDisplayStatistics";
		try
		{
			int numSelectedCols = worksheet.getSelectedColumnCount();
			int numSelectedRows = worksheet.getSelectedRowCount();
			int[] selectedRows = worksheet.getSelectedRows();
			int[] selectedCols = worksheet.getSelectedColumns();
			int[] visibleCols = new int[selectedCols.Length];
			for (int icol = 0; icol < selectedCols.Length; icol++)
			{
				visibleCols[icol] = worksheet.getVisibleColumn(selectedCols[icol]);
			}

			// No data to process
			if (numSelectedCols == 0 || numSelectedRows == 0)
			{
				JGUIUtil.setWaitCursor(worksheet.getHourglassJFrame(), false);
				return;
			}

			/// <summary>
			/// TODO sam 2017-04-01 the following may or may not be helpful.
			///  - the statistics code implemented below processes the bounding block rather than specific selections
			/// if (numSelectedRows == 1 && numSelectedCols == 1) {
			/// // Trivial case -- this will always be a successful copy.  This case is just a placeholder.
			/// }
			/// else if (numSelectedRows == 1) {
			/// // The rows are valid; the only thing left to check is whether the columns are contiguous.
			/// if (!areCellsContiguous(numSelectedRows, selectedRows, numSelectedCols, visibleCols)) {
			///		showCopyErrorDialog("You must select a contiguous block of columns.");
			///		return;
			/// }
			/// }
			/// else if (numSelectedCols == 1) {
			/// // The cols are valid; the only thing left to check is whether the rows are contiguous.
			/// if (!areCellsContiguous(numSelectedRows, selectedRows, numSelectedCols, visibleCols)) {
			///		showCopyErrorDialog("You must select a contiguous block of rows.");
			///		return;
			/// }
			/// }
			/// else {
			/// // There are multiple rows selected and multiple columns selected.  Make sure both are contiguous.
			/// if (!areCellsContiguous(numSelectedRows, selectedRows, numSelectedCols, visibleCols)) {
			///		showCopyErrorDialog("You must select a contiguous block\nof rows and columns.");
			///		return;
			/// }			
			/// }
			/// </summary>

			int numColumns = worksheet.getColumnCount();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("rawtypes") Class[] classes = new Class[numColumns];
			Type[] classes = new Type[numColumns];
			bool[] canCalcStats = new bool[numColumns];
			// Arrays for statistics
			int[] count = new int[numColumns];
			// Allocate arrays for all columns, but only some will be used
			// Use highest precision types and then cast to lower if needed
			// For floating point results...
			// Time series will generally only have double values but leave other
			// cases in order to handle separate columns for flags, etc. that may be added to table.
			double[] min = new double[numColumns];
			double[] max = new double[numColumns];
			double[] sum = new double[numColumns];
			// For integer results...
			long[] imin = new long[numColumns];
			long[] imax = new long[numColumns];
			long[] isum = new long[numColumns];
			// Time series are needed to determine the missing data value
			TS[] ts = new TS[numColumns];
			int[] tsPrec = new int[numColumns];
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<RTi.TS.TS> tslist = (java.util.List<RTi.TS.TS>)((TSViewTable_TableModel)worksheet.getTableModel()).getData();
			IList<TS> tslist = (IList<TS>)((TSViewTable_TableModel)worksheet.getTableModel()).getData();
			string precisionProp = __props.getValue("OutputPrecision");
			for (int icol = 0; icol < numSelectedCols; icol++)
			{
				classes[icol] = worksheet.getColumnClass(worksheet.getAbsoluteColumn(selectedCols[icol]));
				canCalcStats[icol] = false;
				count[icol] = 0;
				sum[icol] = Double.NaN;
				min[icol] = Double.NaN;
				max[icol] = Double.NaN;
				isum[icol] = 0;
				imin[icol] = long.MaxValue;
				imax[icol] = long.MinValue;
				// There are no hidden columns but need to align the time series with selected columns
				// - First column is date/time
				// - Therefore time series 0 is actually in column 1
				int absCol = worksheet.getAbsoluteColumn(selectedCols[icol]);
				ts[icol] = null;
				if (absCol != 0)
				{
					// If 0 need to skip the date/time column
					ts[icol] = tslist[absCol - 1];
					if (ts[icol] != null)
					{
						// Calculate the output precision of the current TS's data
						tsPrec[icol] = 2;
						if (!string.ReferenceEquals(precisionProp, null))
						{
							tsPrec[icol] = StringUtil.atoi(precisionProp);
						}
						else
						{
							try
							{
								DataUnits units = DataUnits.lookupUnits(ts[icol].getDataUnits());
								tsPrec[icol] = units.getOutputPrecision();
							}
							catch (Exception)
							{
								// Use the default...
								tsPrec[icol] = 2;
							}
						}
					}
				}
			}

			// Initialize the list of table fields that contains a leftmost column "Statistic".
			IList<TableField> tableFieldList = new List<TableField>();
			tableFieldList.Add(new TableField(TableField.DATA_TYPE_STRING, "Statistic", -1, -1));
			// Add columns for the selected columns
			bool copyHeader = true;
			if (copyHeader)
			{
				int width = -1;
				// Determine the precision from the time series
				for (int icol = 0; icol < numSelectedCols; icol++)
				{
					if (classes[icol] == typeof(Double))
					{
						tableFieldList.Add(new TableField(TableField.DATA_TYPE_DOUBLE, worksheet.getColumnName(selectedCols[icol], true), width, tsPrec[icol]));
						canCalcStats[icol] = true;
					}
					else if (classes[icol] == typeof(Float))
					{
						tableFieldList.Add(new TableField(TableField.DATA_TYPE_FLOAT, worksheet.getColumnName(selectedCols[icol], true), width, tsPrec[icol]));
						canCalcStats[icol] = true;
					}
					else if (classes[icol] == typeof(Integer))
					{
						tableFieldList.Add(new TableField(TableField.DATA_TYPE_INT, worksheet.getColumnName(selectedCols[icol], true), width, -1));
						canCalcStats[icol] = true;
					}
					else if (classes[icol] == typeof(Long))
					{
						tableFieldList.Add(new TableField(TableField.DATA_TYPE_LONG, worksheet.getColumnName(selectedCols[icol], true), width, -1));
						canCalcStats[icol] = true;
					}
					else
					{
						// Add a string class
						tableFieldList.Add(new TableField(TableField.DATA_TYPE_STRING, worksheet.getColumnName(selectedCols[icol], true), width, -1));
						canCalcStats[icol] = false;
					}
				}
			}

			// Create the table
			DataTable table = new DataTable(tableFieldList);

			JWorksheet_AbstractTableModel tableModel = worksheet.getTableModel();
			// Transfer the data from the worksheet to the subset table
			object cellContents;
			for (int irow = 0; irow < numSelectedRows; irow++)
			{
				TableRecord rec = table.emptyRecord();
				rec.setFieldValue(0, ""); // Blanks for most rows until the statistics added at the end
				for (int icol = 0; icol < numSelectedCols; icol++)
				{
					cellContents = tableModel.getValueAt(selectedRows[irow],selectedCols[icol]);
					if (cellContents is double?)
					{
						double? d = (double?)cellContents;
						if ((ts[icol] != null) && ts[icol].isDataMissing(d.Value))
						{
							// Set to null to let the worksheet handle generically - NaN displays as NaN
							//rec.setFieldValue((icol + 1), Double.NaN);
							rec.setFieldValue((icol + 1), null);
						}
						else
						{
							rec.setFieldValue((icol + 1), cellContents);
						}
					}
					else
					{
						rec.setFieldValue((icol + 1), cellContents);
					}
					// Transfer values to the output cells and calculate the statistics
					if (cellContents != null)
					{
						// Column type allows calculating statistics so do some basic math
						if (canCalcStats[icol])
						{
							if ((classes[icol] == typeof(Double)))
							{
								double? d = (double?)cellContents;
								if (!d.isNaN() && (ts[icol] != null) && !ts[icol].isDataMissing(d.Value))
								{
									++count[icol];
									// Sum, used directly and for mean
									if (double.IsNaN(sum[icol]))
									{
										sum[icol] = d.Value;
									}
									else
									{
										sum[icol] += d.Value;
									}
									// Min statistic
									if (double.IsNaN(min[icol]))
									{
										min[icol] = d.Value;
									}
									else if (d.Value < min[icol])
									{
										min[icol] = d.Value;
									}
									// Max statistic
									if (double.IsNaN(max[icol]))
									{
										max[icol] = d.Value;
									}
									else if (d.Value > max[icol])
									{
										max[icol] = d.Value;
									}
								}
							}
							else if ((classes[icol] == typeof(Float)))
							{
								float? f = (float?)cellContents;
								if (!f.isNaN())
								{
									++count[icol];
									// Sum, used directly and for mean
									if (double.IsNaN(sum[icol]))
									{
										sum[icol] = f;
									}
									else
									{
										sum[icol] += f;
									}
									// Min statistic
									if (double.IsNaN(min[icol]))
									{
										min[icol] = f;
									}
									else if (f < min[icol])
									{
										min[icol] = f;
									}
									// Max statistic
									if (double.IsNaN(max[icol]))
									{
										max[icol] = f;
									}
									else if (f > max[icol])
									{
										max[icol] = f;
									}
								}
							}
							else if ((classes[icol] == typeof(Integer)))
							{
								int? i = (int?)cellContents;
								// No concept of NaN so previous null check is
								// main check for missing
								++count[icol];
								// Sum, used directly and for mean
								sum[icol] += i;
								// Min statistic
								if (imin[icol] == long.MaxValue)
								{
									imin[icol] = i;
								}
								else if (i < imin[icol])
								{
									imin[icol] = i;
								}
								// Max statistic
								if (imax[icol] == long.MinValue)
								{
									imax[icol] = i;
								}
								else if (i > imax[icol])
								{
									imax[icol] = i;
								}
							}
							else if ((classes[icol] == typeof(Long)))
							{
								long? i = (long?)cellContents;
								// No concept of NaN so previous null check is
								// main check for missing
								++count[icol];
								// Sum, used directly and for mean
								sum[icol] += i;
								// Min statistic
								if (imin[icol] == long.MaxValue)
								{
									imin[icol] = i.Value;
								}
								else if (i.Value < imin[icol])
								{
									imin[icol] = i.Value;
								}
								// Max statistic
								if (imax[icol] == long.MinValue)
								{
									imax[icol] = i.Value;
								}
								else if (i.Value > imax[icol])
								{
									imax[icol] = i.Value;
								}
							}
						}
					}
				}
				table.addRecord(rec);
			}
			// Add statistics at the bottom
			TableRecord rec = table.emptyRecord();
			rec.setFieldValue(0, "Count");
			for (int icol = 0; icol < numSelectedCols; icol++)
			{
				// TODO sam 2017-04-01 Worksheet should handle case even when object
				// is a different type than the column, but this is generally not done in tables
				if (canCalcStats[icol])
				{
					rec.setFieldValue((icol + 1), new int?(count[icol]));
				}
			}
			table.addRecord(rec);
			rec = table.emptyRecord();
			rec.setFieldValue(0, "Mean");
			for (int icol = 0; icol < numSelectedCols; icol++)
			{
				if (canCalcStats[icol])
				{
					if (classes[icol] == typeof(Double))
					{
						if ((count[icol] > 0) && !double.IsNaN(sum[icol]))
						{
							rec.setFieldValue((icol + 1), new double?(sum[icol]) / count[icol]);
						}
					}
					else if (classes[icol] == typeof(Float))
					{
						if ((count[icol] > 0) && !double.IsNaN(sum[icol]))
						{
							rec.setFieldValue((icol + 1), new float?(sum[icol]) / count[icol]);
						}
					}
					else if (classes[icol] == typeof(Long))
					{
						if (count[icol] > 0)
						{
							rec.setFieldValue((icol + 1), new long?(isum[icol]) / count[icol]);
						}
					}
					else if (classes[icol] == typeof(Integer))
					{
						if (count[icol] > 0)
						{
							rec.setFieldValue((icol + 1), new int?((int)isum[icol]) / count[icol]);
						}
					}
				}
			}
			table.addRecord(rec);
			rec = table.emptyRecord();
			rec.setFieldValue(0, "Min");
			for (int icol = 0; icol < numSelectedCols; icol++)
			{
				if (canCalcStats[icol])
				{
					if (classes[icol] == typeof(Double))
					{
						if (!double.IsNaN(min[icol]))
						{
							rec.setFieldValue((icol + 1), new double?(min[icol]));
						}
					}
					else if (classes[icol] == typeof(Float))
					{
						if (!double.IsNaN(min[icol]))
						{
							rec.setFieldValue((icol + 1), new float?(min[icol]));
						}
					}
					else if (classes[icol] == typeof(Long))
					{
						if (imin[icol] != long.MaxValue)
						{
							rec.setFieldValue((icol + 1), new long?(imin[icol]));
						}
					}
					else if (classes[icol] == typeof(Integer))
					{
						if (imin[icol] != long.MaxValue)
						{
							rec.setFieldValue((icol + 1), new int?((int)imin[icol]));
						}
					}
				}
			}
			table.addRecord(rec);
			rec = table.emptyRecord();
			rec.setFieldValue(0, "Max");
			for (int icol = 0; icol < numSelectedCols; icol++)
			{
				if (canCalcStats[icol])
				{
					if (classes[icol] == typeof(Double))
					{
						if (!double.IsNaN(max[icol]))
						{
							rec.setFieldValue((icol + 1), new double?(max[icol]));
						}
					}
					else if (classes[icol] == typeof(Float))
					{
						if (!double.IsNaN(max[icol]))
						{
							rec.setFieldValue((icol + 1), new float?(max[icol]));
						}
					}
					else if (classes[icol] == typeof(Long))
					{
						if (imax[icol] != long.MinValue)
						{
							rec.setFieldValue((icol + 1), new long?(imax[icol]));
						}
					}
					else if (classes[icol] == typeof(Integer))
					{
						if (imax[icol] != long.MinValue)
						{
							rec.setFieldValue((icol + 1), new long?(imax[icol]));
						}
					}
				}
			}
			table.addRecord(rec);
			rec = table.emptyRecord();
			rec.setFieldValue(0, "Sum");
			for (int icol = 0; icol < numSelectedCols; icol++)
			{
				if (canCalcStats[icol])
				{
					if (classes[icol] == typeof(Double))
					{
						if (!double.IsNaN(sum[icol]))
						{
							rec.setFieldValue((icol + 1), new double?(sum[icol]));
						}
					}
					else if (classes[icol] == typeof(Float))
					{
						if (!double.IsNaN(sum[icol]))
						{
							rec.setFieldValue((icol + 1), new float?(sum[icol]));
						}
					}
					else if (classes[icol] == typeof(Long))
					{
						rec.setFieldValue((icol + 1), new long?(isum[icol]));
					}
					else if (classes[icol] == typeof(Integer))
					{
						rec.setFieldValue((icol + 1), new int?((int)isum[icol]));
					}
				}
			}
			table.addRecord(rec);

			DataTable_TableModel dttm = new DataTable_TableModel(table);
			DataTable_CellRenderer scr = new DataTable_CellRenderer(dttm);
			PropList frameProps = new PropList("");
			frameProps.set("Title","Time Series Table Statistics");
			PropList worksheetProps = new PropList("");
			worksheetProps.add("JWorksheet.OneClickColumnSelection=true");
			worksheetProps.add("JWorksheet.RowColumnPresent=true");
			worksheetProps.add("JWorksheet.ShowPopupMenu=true");
			worksheetProps.add("JWorksheet.SelectionMode=ExcelSelection");
			worksheetProps.add("JWorksheet.AllowCopy=true");
			// The following will be default center on its parent and be shown in front
			TableModel_JFrame f = new TableModel_JFrame(dttm, scr, frameProps, worksheetProps);
			JGUIUtil.center(f,this);
			f.toFront(); // This does not seem to always work
			f.setAlwaysOnTop(true); // TODO sam 2017-04-01 don't like to do this but seems necessary
		}
		catch (Exception e)
		{
			(new ResponseJDialog(worksheet.getHourglassJFrame(), "Error", "Error calculating statistics.", ResponseJDialog.OK)).response();
			Message.printWarning(2, "", e);
		}
	}

	/// <summary>
	/// Calculates the number of panels with worksheets that will be added in total
	/// to the __mainJPanel depending on the number of base intervals with time series. </summary>
	/// <returns> the number of panels with worksheets added to __mainJPanel. </returns>
	private int calculateNumberOfPanels()
	{
		int size = 0;
		if (__minuteWorksheets != null && __minuteWorksheets.Length > 0)
		{
			size++;
		}
		if (__hourWorksheets != null && __hourWorksheets.Length > 0)
		{
			size++;
		}
		if (__dayWorksheets != null && __dayWorksheets.Length > 0)
		{
			size++;
		}
		if (__monthWorksheets != null && __monthWorksheets.Length > 0)
		{
			size++;
		}
		if (__yearWorksheets != null && __yearWorksheets.Length > 0)
		{
			size++;
		}
		if (__irregularMinute != null && __irregularMinuteWorksheets.Length > 0)
		{
			size++;
		}
		if (__irregularHour != null && __irregularHourWorksheets.Length > 0)
		{
			size++;
		}
		if (__irregularDay != null && __irregularDayWorksheets.Length > 0)
		{
			size++;
		}
		if (__irregularMonth != null && __irregularMonthWorksheets.Length > 0)
		{
			size++;
		}
		if (__irregularYear != null && __irregularYearWorksheets.Length > 0)
		{
			size++;
		}
		return size;
	}

	/// <summary>
	/// Calculates the number of worksheets in each panel and return an integer array
	/// that can tell exactly how many worksheets are in each panel. </summary>
	/// <returns> an integer array where:<br>
	/// [0] - the number of worksheets in the minute panel<br>
	/// [1] - the number of worksheets in the hour panel<br>
	/// [2] - the number of worksheets in the day panel<br>
	/// [3] - the number of worksheets in the month panel<br>
	/// [4] - the number of worksheets in the year panel<br>
	/// [5] - the number of worksheets in the irregular minute panel<br>
	/// [6] - the number of worksheets in the irregular hour panel<br>
	/// [7] - the number of worksheets in the irregular day panel<br>
	/// [8] - the number of worksheets in the irregular month panel<br>
	/// [9] - the number of worksheets in the irregular year panel<br>
	/// [10] - the total number of worksheets visible<br>
	/// [11] - the total number of panels with visible worksheets </returns>
	private int[] calculateVisibleWorksheetsByPanel()
	{
		int[] array = new int[12];
		// Regular interval
		if (__minuteWorksheets == null || !__minuteJCheckBox.isSelected())
		{
			array[0] = 0;
		}
		else
		{
			array[0] = __minuteWorksheets.Length;
		}
		if (__hourWorksheets == null || !__hourJCheckBox.isSelected())
		{
			array[1] = 0;
		}
		else
		{
			array[1] = __hourWorksheets.Length;
		}
		if (__dayWorksheets == null || !__dayJCheckBox.isSelected())
		{
			array[2] = 0;
		}
		else
		{
			array[2] = __dayWorksheets.Length;
		}
		if (__monthWorksheets == null || !__monthJCheckBox.isSelected())
		{
			array[3] = 0;
		}
		else
		{
			array[3] = __monthWorksheets.Length;
		}
		if (__yearWorksheets == null || !__yearJCheckBox.isSelected())
		{
			array[4] = 0;
		}
		else
		{
			array[4] = __yearWorksheets.Length;
		}
		// Irregular
		if (__irregularMinuteWorksheets == null || !__irregularMinuteJCheckBox.isSelected())
		{
			array[5] = 0;
		}
		else
		{
			array[5] = __irregularMinuteWorksheets.Length;
		}
		if (__irregularHourWorksheets == null || !__irregularHourJCheckBox.isSelected())
		{
			array[6] = 0;
		}
		else
		{
			array[6] = __irregularHourWorksheets.Length;
		}
		if (__irregularDayWorksheets == null || !__irregularDayJCheckBox.isSelected())
		{
			array[7] = 0;
		}
		else
		{
			array[7] = __irregularDayWorksheets.Length;
		}
		if (__irregularMonthWorksheets == null || !__irregularMonthJCheckBox.isSelected())
		{
			array[8] = 0;
		}
		else
		{
			array[8] = __irregularMonthWorksheets.Length;
		}
		if (__irregularYearWorksheets == null || !__irregularYearJCheckBox.isSelected())
		{
			array[9] = 0;
		}
		else
		{
			array[9] = __irregularYearWorksheets.Length;
		}

		array[10] = array[9] + array[8] + array[7] + array[6] + array[5] + array[4] + array[3] + array[2] + array[1] + array[0];

		array[11] = 0;
		if (array[0] > 0)
		{
			array[11]++;
		}
		if (array[1] > 0)
		{
			array[11]++;
		}
		if (array[2] > 0)
		{
			array[11]++;
		}
		if (array[3] > 0)
		{
			array[11]++;
		}
		if (array[4] > 0)
		{
			array[11]++;
		}
		if (array[5] > 0)
		{
			array[11]++;
		}
		if (array[6] > 0)
		{
			array[11]++;
		}
		if (array[7] > 0)
		{
			array[11]++;
		}
		if (array[8] > 0)
		{
			array[11]++;
		}
		if (array[9] > 0)
		{
			array[11]++;
		}

		return array;
	}

	/// <summary>
	/// Checks to see if only a single worksheet is displayed in the GUI.  If so, it
	/// is selected and the save button is set to always be enabled.
	/// </summary>
	private void checkForSingleWorksheet()
	{
		JWorksheet worksheet = null;

		int count = 0;

		if (__dayWorksheets != null)
		{
			for (int i = 0; i < __dayWorksheets.Length; i++)
			{
				count++;
				worksheet = __dayWorksheets[i];
			}
		}
		if (__minuteWorksheets != null)
		{
			for (int i = 0; i < __minuteWorksheets.Length; i++)
			{
				count++;
				worksheet = __minuteWorksheets[i];
			}
		}
		if (__hourWorksheets != null)
		{
			for (int i = 0; i < __hourWorksheets.Length; i++)
			{
				count++;
				worksheet = __hourWorksheets[i];
			}
		}
		if (__monthWorksheets != null)
		{
			for (int i = 0; i < __monthWorksheets.Length; i++)
			{
				count++;
				worksheet = __monthWorksheets[i];
			}
		}
		if (__yearWorksheets != null)
		{
			for (int i = 0; i < __yearWorksheets.Length; i++)
			{
				count++;
				worksheet = __yearWorksheets[i];
			}
		}
		if (__irregularMinuteWorksheets != null)
		{
			for (int i = 0; i < __irregularMinuteWorksheets.Length; i++)
			{
				count++;
				worksheet = __irregularMinuteWorksheets[i];
			}
		}
		if (__irregularHourWorksheets != null)
		{
			for (int i = 0; i < __irregularHourWorksheets.Length; i++)
			{
				count++;
				worksheet = __irregularHourWorksheets[i];
			}
		}
		if (__irregularDayWorksheets != null)
		{
			for (int i = 0; i < __irregularDayWorksheets.Length; i++)
			{
				count++;
				worksheet = __irregularDayWorksheets[i];
			}
		}
		if (__irregularMonthWorksheets != null)
		{
			for (int i = 0; i < __irregularMonthWorksheets.Length; i++)
			{
				count++;
				worksheet = __irregularMonthWorksheets[i];
			}
		}
		if (__irregularYearWorksheets != null)
		{
			for (int i = 0; i < __irregularYearWorksheets.Length; i++)
			{
				count++;
				worksheet = __irregularYearWorksheets[i];
			}
		}

		if (count != 1)
		{
			return;
		}

		selectWorksheet(worksheet, null);
		__saveAlwaysEnabled = true;
		__saveJButton.setEnabled(true);
	}

	/// <summary>
	/// Takes the Time Series from the __tslist list and puts each one into a list
	/// specific to its data interval (e.g., Day time series go into __day, 
	/// minute time series go into __minute, etc).  Different multipliers are all 
	/// lumped together, as long as they have the same data interval.  Irregular time series
	/// are split out by the interval of the starting date/time.
	/// </summary>
	private void createSeparateTimeSeries()
	{
		string routine = this.GetType().Name + ".createSeparateTimeSeries";
		int interval;
		__minute = new List<TS>();
		__hour = new List<TS>();
		__day = new List<TS>();
		__month = new List<TS>();
		__year = new List<TS>();
		__irregularMinute = new List<TS>();
		__irregularHour = new List<TS>();
		__irregularDay = new List<TS>();
		__irregularMonth = new List<TS>();
		__irregularYear = new List<TS>();

		foreach (TS ts in __tslist)
		{
			if (ts == null)
			{
				continue;
			}
			interval = ts.getDataIntervalBase();

			if (interval == TimeInterval.MINUTE)
			{
				__minute.Add(ts);
			}
			else if (interval == TimeInterval.HOUR)
			{
				__hour.Add(ts);
			}
			else if (interval == TimeInterval.DAY)
			{
				__day.Add(ts);
			}
			else if (interval == TimeInterval.MONTH)
			{
				__month.Add(ts);
			}
			else if (interval == TimeInterval.YEAR)
			{
				__year.Add(ts);
			}
			else if (interval == TimeInterval.IRREGULAR)
			{
				// Put in the appropriate list based on the date/time of the period start
				DateTime d = ts.getDate1();
				if (d == null)
				{
					d = ts.getDate1Original();
				}
				if (d != null)
				{
					int precision = d.getPrecision();
					if (precision == DateTime.PRECISION_SECOND)
					{
						// Include second precision here because most likely it is minute precision with seconds = 0
						__irregularMinute.Add(ts);
						Message.printStatus(2,routine,"Date/time precision is second, but treating as minute for table \"" + ts.getIdentifierString() + "\"");
					}
					else if (precision == DateTime.PRECISION_MINUTE)
					{
						__irregularMinute.Add(ts);
					}
					else if (precision == DateTime.PRECISION_HOUR)
					{
						__irregularHour.Add(ts);
					}
					else if (precision == DateTime.PRECISION_DAY)
					{
						__irregularDay.Add(ts);
					}
					else if (precision == DateTime.PRECISION_MONTH)
					{
						__irregularMonth.Add(ts);
					}
					else if (precision == DateTime.PRECISION_YEAR)
					{
						__irregularYear.Add(ts);
					}
					else
					{
						// Don't handle the precision
						Message.printWarning(3,routine,"Don't know how to handle time series interval in table for \"" + ts.getIdentifierString() + "\"");
					}
				}
			}
		}
	}

	/// <summary>
	/// Create the table models with the same interval base for all of the worksheets for the given list of time series.
	/// The time series will have the same base interval but if regular may have different interval multiplers.
	/// If irregular, the time series will have been grouped by the precision of the period date. </summary>
	/// <param name="tslist"> list of time series for which to create table models. </param>
	/// <returns> an array of TSViewTable_TableModel object, one for each worksheet
	/// that needs to be created, or a zero-element array if no worksheets need be created for the ts type. </returns>
	private TSViewTable_TableModel[] createTableModels(IList<TS> tslist)
	{
		string routine = "createTableModels";

		// If there is no data in the tslist list, there is no need to create the table models
		if ((tslist == null) || tslist.Count == 0)
		{
			return new TSViewTable_TableModel[0];
		}

		int numts = tslist.Count;

		// The following arrays are used to match up time series with 
		// the same interval multipliers.  The arrays are sized to the 
		// maximum size necessary and won't necessarily be filled completely.
		int[] mults = new int[numts];
		int[] matches = new int[numts];
		string[] tsFormatString = new string[numts];

		int count = 0;
		bool hit = false;

		// Get the first TS in the list and get the interval base.  All other
		// TS in the list must have the same interval base
		TS ts = tslist[0];
		int interval = ts.getDataIntervalBase();

		int dateFormat = DateTime.FORMAT_YYYY_MM_DD_HH_mm;
		// Get the proper date format
		if (interval == TimeInterval.HOUR)
		{
			dateFormat = DateTime.FORMAT_YYYY_MM_DD_HH;
		}
		else if (interval == TimeInterval.DAY)
		{
			dateFormat = DateTime.FORMAT_YYYY_MM_DD;
		}
		else if (interval == TimeInterval.MONTH)
		{
			dateFormat = DateTime.FORMAT_YYYY_MM;
		}
		else if (interval == TimeInterval.YEAR)
		{
			dateFormat = DateTime.FORMAT_YYYY;
		}
		else if (interval == TimeInterval.MINUTE)
		{
			dateFormat = DateTime.FORMAT_YYYY_MM_DD_HH_mm;
		}
		else if (interval == TimeInterval.IRREGULAR)
		{
			DateTime d = ts.getDate1();
			if (d == null)
			{
				d = ts.getDate1Original();
			}
			if (d != null)
			{
				int precision = d.getPrecision();
				if (precision == DateTime.PRECISION_MINUTE)
				{
					dateFormat = DateTime.FORMAT_YYYY_MM_DD_HH_mm;
				}
				else if (precision == DateTime.PRECISION_HOUR)
				{
					dateFormat = DateTime.FORMAT_YYYY_MM_DD_HH;
				}
				else if (precision == DateTime.PRECISION_DAY)
				{
					dateFormat = DateTime.FORMAT_YYYY_MM_DD;
				}
				else if (precision == DateTime.PRECISION_MONTH)
				{
					dateFormat = DateTime.FORMAT_YYYY_MM;
				}
				else if (precision == DateTime.PRECISION_YEAR)
				{
					dateFormat = DateTime.FORMAT_YYYY;
				}
			}
		}

		try
		{
		int tsPrecision = 2; // default.
		DataUnits units = null;
		string propValue = __props.getValue("OutputPrecision");
		int multi = 0;

		// Go through the time series and see how many of them have different intervals.
		// All of the TS with the same intervals need to be placed in the same worksheet.
		for (int i = 0; i < numts; i++)
		{
			ts = tslist[i];

			if (interval == TimeInterval.IRREGULAR)
			{
				count = 1; // Only one table model needed because only interval base (from TS dates for irregular) is of concern
			}
			else
			{
				// Regular interval time series
				// Get the interval multiplier for the current TS
				multi = ts.getDataIntervalMult();
				hit = false;

				// Look through the array of previously-found interval
				// multipliers (mults[]) and see if the multiplier has already been encountered.
				for (int j = 0; j < count; j++)
				{
					if (mults[j] == multi)
					{
						// If true;
						matches[i] = j;
						hit = true;
						break;
					}
				}

				// If the interval multiplier was not found, add it to the
				// list of found multipliers and increment the count of
				// different interval multipliers that have been found
				if (!hit)
				{
					mults[count] = multi;
					matches[i] = count;
					count++;
				}
			}

			// Calculate the output precision of the current TS's data
			tsPrecision = 2;
			if (!string.ReferenceEquals(propValue, null))
			{
				tsPrecision = StringUtil.atoi(propValue);
			}
			else
			{
				try
				{
					units = DataUnits.lookupUnits(ts.getDataUnits());
					tsPrecision = units.getOutputPrecision();
				}
				catch (Exception)
				{
					// Use the default...
					tsPrecision = 2;
				}
			}
			tsFormatString[i] = "%" + __OUTPUT_WIDTH + "." + tsPrecision + "f";

		}

		// Create an array of table models big enough to hold one table
		// model for every different interval multiplier
		TSViewTable_TableModel[] models = new TSViewTable_TableModel[count];

		bool useExtendedLegend = false;
		propValue = __props.getValue("Table.UseExtendedLegend");
		if ((!string.ReferenceEquals(propValue, null)) && (propValue.Equals("true", StringComparison.OrdinalIgnoreCase)))
		{
			useExtendedLegend = true;
		}

		TS tempTS = null;

		// Loop through all of the different interval multipliers
		for (int i = 0; i < count; i++)
		{
			IList<TS> tslistForIntervalMult = new List<TS>();
			if (interval == TimeInterval.IRREGULAR)
			{
				for (int j = 0; j < numts; j++)
				{
					tslistForIntervalMult.Add(tslist[j]);
				}
			}
			else
			{
				// Add all the time series with the same interval multiplier to the list
				for (int j = 0; j < numts; j++)
				{
					if (matches[j] == i)
					{
						tslistForIntervalMult.Add(tslist[j]);
					}
				}
			}

			// Get all the format precision strings for the TS that were found in the previous loop 
			string[] formats = new string[tslistForIntervalMult.Count];
			int formatCount = 0;
			for (int j = 0; j < numts; j++)
			{
				if (interval == TimeInterval.IRREGULAR)
				{
					formats[formatCount++] = tsFormatString[j];
				}
				else
				{
					if (matches[j] == i)
					{
						formats[formatCount++] = tsFormatString[j];
					}
				}
			}

			// Now get the starting date of data ...
			TSLimits limits = TSUtil.getPeriodFromTS(tslistForIntervalMult, TSUtil.MAX_POR);
			DateTime start = limits.getDate1();

			// ... and the interval multiplier ...
			if (interval == TimeInterval.IRREGULAR)
			{
				// In this case multi is the precision
				int datePrecision = ts.getDate1().getPrecision();
				models[i] = new TSViewTable_TableModel(tslistForIntervalMult, start, interval, datePrecision, dateFormat, formats, useExtendedLegend);
			}
			else
			{
				// Regular interval
				if (tslistForIntervalMult == null || tslistForIntervalMult.Count == 0)
				{
					// (in this case, use a representative TS)
					tempTS = tslist[i];
					multi = tempTS.getDataIntervalMult();
				}
				else
				{
					tempTS = tslistForIntervalMult[0];
					multi = tempTS.getDataIntervalMult();
				}
				// ... and create the table model to display all the time 
				// series with the same interval base and interval multiplier
				models[i] = new TSViewTable_TableModel(tslistForIntervalMult, start, interval, multi, dateFormat, formats, useExtendedLegend);
			}
		}

		return models;
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, "Error generating table models (" + e + ").");
			Message.printWarning(2, routine, e);
			return null;
		}
	}

	/// <summary>
	/// Creates worksheets for all of the table models that were previously-generated. </summary>
	/// <param name="models"> table models for each of the worksheets that need to be made. </param>
	/// <param name="p"> PropList defining JWorksheet characteristics.  See the JWorksheet constructors. </param>
	/// <returns> an array of JWorksheets, one for each model, or an empty array if no models. </returns>
	private JWorksheet[] createWorksheets(TSViewTable_TableModel[] models, PropList p)
	{
		if ((models == null) || (models.Length == 0))
		{
			return new JWorksheet[0];
		}
		int numWorksheets = models.Length;

		JWorksheet[] worksheets = new JWorksheet[numWorksheets];
		JWorksheet worksheet = null;
		TSViewTable_TableModel model = null;
		for (int i = 0; i < numWorksheets; i++)
		{
			model = models[i];
			TSViewTable_CellRenderer cr = new TSViewTable_CellRenderer(model);
			worksheet = new JWorksheet(cr, model, p);
			worksheet.setPreferredScrollableViewportSize(null);
			worksheet.setHourglassJFrame(this);
			worksheets[i] = worksheet;
			model.setWorksheet(worksheets[i]);
			// Add this class as an action listener on the worksheet so that
			// "Calculate Statistics" can be handled here.
			// Otherwise the generic handling won't be able to handle the time series missing value
			worksheet.addPopupMenuActionListener(this);
		}
		return worksheets;
	}

	/// <summary>
	/// Clean up before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~TSViewTableJFrame()
	{
		__dayJCheckBox = null;
		__hourJCheckBox = null;
		__minuteJCheckBox = null;
		__monthJCheckBox = null;
		__yearJCheckBox = null;
		__mainJPanel = null;
		__dayJPanel = null;
		__hourJPanel = null;
		__minuteJPanel = null;
		__monthJPanel = null;
		__yearJPanel = null;
		IOUtil.nullArray(__dayScrollPanes);
		IOUtil.nullArray(__hourScrollPanes);
		IOUtil.nullArray(__minuteScrollPanes);
		IOUtil.nullArray(__monthScrollPanes);
		IOUtil.nullArray(__yearScrollPanes);
		__lastSelectedScrollPane = null;
		__messageJTextField = null;
		IOUtil.nullArray(__dayWorksheets);
		IOUtil.nullArray(__hourWorksheets);
		IOUtil.nullArray(__minuteWorksheets);
		IOUtil.nullArray(__monthWorksheets);
		IOUtil.nullArray(__yearWorksheets);
		__lastSelectedWorksheet = null;
		__props = null;
		__summaryJButton = null;
		__graphJButton = null;
		__closeJButton = null;
		__saveJButton = null;
		__tsviewJFrame = null;
		IOUtil.nullArray(__dayModels);
		IOUtil.nullArray(__hourModels);
		IOUtil.nullArray(__minuteModels);
		IOUtil.nullArray(__monthModels);
		IOUtil.nullArray(__yearModels);
		__day = null;
		__hour = null;
		__minute = null;
		__month = null;
		__tslist = null;
		__year = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Figures out which worksheet or worksheet header or worksheet's JScrollPane area
	/// was clicked on, and marks that worksheet as the last-selected one. </summary>
	/// <param name="e"> the MouseEvent that means a worksheet area was clicked on. </param>
	private JWorksheet findClickedOnJWorksheet(MouseEvent e)
	{
		object source = e.getSource();
		if (source is JWorksheet)
		{
			return (JWorksheet)source;
		}
		else if (source is JWorksheet_Header)
		{
			return (JWorksheet)((JWorksheet_Header)source).getTable();
		}
		else
		{
			if (__dayWorksheets != null)
			{
				JWorksheet worksheet = searchListeners(__dayWorksheets, __dayMouseListeners, source);
				if (worksheet != null)
				{
					return worksheet;
				}
			}
			if (__hourWorksheets != null)
			{
				JWorksheet worksheet = searchListeners(__hourWorksheets, __hourMouseListeners, source);
				if (worksheet != null)
				{
					return worksheet;
				}
			}
			if (__minuteWorksheets != null)
			{
				JWorksheet worksheet = searchListeners(__minuteWorksheets, __minuteMouseListeners, source);
				if (worksheet != null)
				{
					return worksheet;
				}
			}
			if (__monthWorksheets != null)
			{
				JWorksheet worksheet = searchListeners(__monthWorksheets, __monthMouseListeners, source);
				if (worksheet != null)
				{
					return worksheet;
				}
			}
			if (__yearWorksheets != null)
			{
				JWorksheet worksheet = searchListeners(__yearWorksheets, __yearMouseListeners, source);
				if (worksheet != null)
				{
					return worksheet;
				}
			}
			if (__irregularMinuteWorksheets != null)
			{
				JWorksheet worksheet = searchListeners(__irregularMinuteWorksheets, __irregularMinuteMouseListeners, source);
				if (worksheet != null)
				{
					return worksheet;
				}
			}
			if (__irregularHourWorksheets != null)
			{
				JWorksheet worksheet = searchListeners(__irregularHourWorksheets, __irregularHourMouseListeners, source);
				if (worksheet != null)
				{
					return worksheet;
				}
			}
			if (__irregularDayWorksheets != null)
			{
				JWorksheet worksheet = searchListeners(__irregularDayWorksheets, __irregularDayMouseListeners, source);
				if (worksheet != null)
				{
					return worksheet;
				}
			}
			if (__irregularMonthWorksheets != null)
			{
				JWorksheet worksheet = searchListeners(__irregularMonthWorksheets, __irregularMonthMouseListeners, source);
				if (worksheet != null)
				{
					return worksheet;
				}
			}
			if (__irregularYearWorksheets != null)
			{
				JWorksheet worksheet = searchListeners(__irregularYearWorksheets, __irregularYearMouseListeners, source);
				if (worksheet != null)
				{
					return worksheet;
				}
			}
		}
		return null;
	}

	/// <summary>
	/// Find the JPanel associated with a specific worksheet. </summary>
	/// <param name="worksheet"> the worksheet for which to find the JPanel it is in. </param>
	/// <returns> the JPanel for a specific JWorksheet. </returns>
	public virtual JPanel findWorksheetsJPanel(JWorksheet worksheet)
	{
		if (worksheet == null)
		{
			return null;
		}
		if (__dayWorksheets != null)
		{
			for (int i = 0; i < __dayWorksheets.Length; i++)
			{
				if (__dayWorksheets[i] == worksheet)
				{
					return __dayJPanel;
				}
			}
		}
		if (__minuteWorksheets != null)
		{
			for (int i = 0; i < __minuteWorksheets.Length; i++)
			{
				if (__minuteWorksheets[i] == worksheet)
				{
					return __minuteJPanel;
				}
			}
		}
		if (__hourWorksheets != null)
		{
			for (int i = 0; i < __hourWorksheets.Length; i++)
			{
				if (__hourWorksheets[i] == worksheet)
				{
					return __hourJPanel;
				}
			}
		}
		if (__monthWorksheets != null)
		{
			for (int i = 0; i < __monthWorksheets.Length; i++)
			{
				if (__monthWorksheets[i] == worksheet)
				{
					return __monthJPanel;
				}
			}
		}
		if (__yearWorksheets != null)
		{
			for (int i = 0; i < __yearWorksheets.Length; i++)
			{
				if (__yearWorksheets[i] == worksheet)
				{
					return __yearJPanel;
				}
			}
		}
		if (__irregularMinuteWorksheets != null)
		{
			for (int i = 0; i < __irregularMinuteWorksheets.Length; i++)
			{
				if (__irregularMinuteWorksheets[i] == worksheet)
				{
					return __irregularMinuteJPanel;
				}
			}
		}
		if (__irregularHourWorksheets != null)
		{
			for (int i = 0; i < __irregularHourWorksheets.Length; i++)
			{
				if (__irregularHourWorksheets[i] == worksheet)
				{
					return __irregularHourJPanel;
				}
			}
		}
		if (__irregularDayWorksheets != null)
		{
			for (int i = 0; i < __irregularDayWorksheets.Length; i++)
			{
				if (__irregularDayWorksheets[i] == worksheet)
				{
					return __irregularDayJPanel;
				}
			}
		}
		if (__irregularMonthWorksheets != null)
		{
			for (int i = 0; i < __irregularMonthWorksheets.Length; i++)
			{
				if (__irregularMonthWorksheets[i] == worksheet)
				{
					return __irregularMonthJPanel;
				}
			}
		}
		if (__irregularYearWorksheets != null)
		{
			for (int i = 0; i < __irregularYearWorksheets.Length; i++)
			{
				if (__irregularYearWorksheets[i] == worksheet)
				{
					return __irregularYearJPanel;
				}
			}
		}
		return null;
	}

	/// <summary>
	/// Find the scrollpane associated with a specific worksheet. </summary>
	/// <param name="worksheet"> the worksheet for which to find the scrollpane. </param>
	/// <returns> the JScrollPane for a specific JWorksheet. </returns>
	public virtual JScrollPane findWorksheetsScrollPane(JWorksheet worksheet)
	{
		if (worksheet == null)
		{
			return null;
		}
		if (__dayWorksheets != null)
		{
			for (int i = 0; i < __dayWorksheets.Length; i++)
			{
				if (__dayWorksheets[i] == worksheet)
				{
					return __dayScrollPanes[i];
				}
			}
		}
		if (__minuteWorksheets != null)
		{
			for (int i = 0; i < __minuteWorksheets.Length; i++)
			{
				if (__minuteWorksheets[i] == worksheet)
				{
					return __minuteScrollPanes[i];
				}
			}
		}
		if (__hourWorksheets != null)
		{
			for (int i = 0; i < __hourWorksheets.Length; i++)
			{
				if (__hourWorksheets[i] == worksheet)
				{
					return __hourScrollPanes[i];
				}
			}
		}
		if (__monthWorksheets != null)
		{
			for (int i = 0; i < __monthWorksheets.Length; i++)
			{
				if (__monthWorksheets[i] == worksheet)
				{
					return __monthScrollPanes[i];
				}
			}
		}
		if (__yearWorksheets != null)
		{
			for (int i = 0; i < __yearWorksheets.Length; i++)
			{
				if (__yearWorksheets[i] == worksheet)
				{
					return __yearScrollPanes[i];
				}
			}
		}
		if (__irregularMinuteWorksheets != null)
		{
			for (int i = 0; i < __irregularMinuteWorksheets.Length; i++)
			{
				if (__irregularMinuteWorksheets[i] == worksheet)
				{
					return __irregularMinuteScrollPanes[i];
				}
			}
		}
		if (__irregularHourWorksheets != null)
		{
			for (int i = 0; i < __irregularHourWorksheets.Length; i++)
			{
				if (__irregularHourWorksheets[i] == worksheet)
				{
					return __irregularHourScrollPanes[i];
				}
			}
		}
		if (__irregularDayWorksheets != null)
		{
			for (int i = 0; i < __irregularDayWorksheets.Length; i++)
			{
				if (__irregularDayWorksheets[i] == worksheet)
				{
					return __irregularDayScrollPanes[i];
				}
			}
		}
		if (__irregularMonthWorksheets != null)
		{
			for (int i = 0; i < __irregularMonthWorksheets.Length; i++)
			{
				if (__irregularMonthWorksheets[i] == worksheet)
				{
					return __irregularMonthScrollPanes[i];
				}
			}
		}
		if (__irregularYearWorksheets != null)
		{
			for (int i = 0; i < __irregularYearWorksheets.Length; i++)
			{
				if (__irregularYearWorksheets[i] == worksheet)
				{
					return __irregularYearScrollPanes[i];
				}
			}
		}
		return null;
	}

	/// <summary>
	/// Handle events from the checkboxes, indicating which time series intervals should be shown. </summary>
	/// <param name="evt"> ItemEvent to handle. </param>
	public virtual void itemStateChanged(ItemEvent evt)
	{
		object source = evt.getSource();
		int state = evt.getStateChange();

		if (source == __flagJComboBox)
		{
			// Change the data flag visualization for all the table models
			TSDataFlagVisualizationType vizType = TSDataFlagVisualizationType.valueOfIgnoreCase(__flagJComboBox.getSelected());
			if (vizType != null)
			{
				for (int i = 0; i < __minuteModels.Length; i++)
				{
					__minuteModels[i].setDataFlagVisualizationType(vizType);
					__minuteModels[i].fireTableDataChanged();
				}
				for (int i = 0; i < __hourModels.Length; i++)
				{
					__hourModels[i].setDataFlagVisualizationType(vizType);
					__hourModels[i].fireTableDataChanged();
				}
				for (int i = 0; i < __dayModels.Length; i++)
				{
					__dayModels[i].setDataFlagVisualizationType(vizType);
					__dayModels[i].fireTableDataChanged();
				}
				for (int i = 0; i < __monthModels.Length; i++)
				{
					__monthModels[i].setDataFlagVisualizationType(vizType);
					__monthModels[i].fireTableDataChanged();
				}
				for (int i = 0; i < __yearModels.Length; i++)
				{
					__yearModels[i].setDataFlagVisualizationType(vizType);
					__yearModels[i].fireTableDataChanged();
				}
				for (int i = 0; i < __irregularMinuteModels.Length; i++)
				{
					__irregularMinuteModels[i].setDataFlagVisualizationType(vizType);
					__irregularMinuteModels[i].fireTableDataChanged();
				}
				for (int i = 0; i < __irregularHourModels.Length; i++)
				{
					__irregularHourModels[i].setDataFlagVisualizationType(vizType);
					__irregularHourModels[i].fireTableDataChanged();
				}
				for (int i = 0; i < __irregularDayModels.Length; i++)
				{
					__irregularDayModels[i].setDataFlagVisualizationType(vizType);
					__irregularDayModels[i].fireTableDataChanged();
				}
				for (int i = 0; i < __irregularMonthModels.Length; i++)
				{
					__irregularMonthModels[i].setDataFlagVisualizationType(vizType);
					__irregularMonthModels[i].fireTableDataChanged();
				}
				for (int i = 0; i < __irregularYearModels.Length; i++)
				{
					__irregularYearModels[i].setDataFlagVisualizationType(vizType);
					__irregularYearModels[i].fireTableDataChanged();
				}
			}
			return;
		}

		bool visible = false;
		if (state == ItemEvent.SELECTED)
		{
			visible = true;
		}
		else
		{
			visible = false;
		}

		if (source == __minuteJCheckBox)
		{
			__minuteJPanel.setVisible(visible);
		}
		else if (source == __hourJCheckBox)
		{
			__hourJPanel.setVisible(visible);
		}
		else if (source == __dayJCheckBox)
		{
			__dayJPanel.setVisible(visible);
		}
		else if (source == __monthJCheckBox)
		{
			__monthJPanel.setVisible(visible);
		}
		else if (source == __yearJCheckBox)
		{
			__yearJPanel.setVisible(visible);
		}
		else if (source == __irregularMinuteJCheckBox)
		{
			__irregularMinuteJPanel.setVisible(visible);
		}
		else if (source == __irregularHourJCheckBox)
		{
			__irregularHourJPanel.setVisible(visible);
		}
		else if (source == __irregularDayJCheckBox)
		{
			__irregularDayJPanel.setVisible(visible);
		}
		else if (source == __irregularMonthJCheckBox)
		{
			__irregularMonthJPanel.setVisible(visible);
		}
		else if (source == __irregularYearJCheckBox)
		{
			__irregularYearJPanel.setVisible(visible);
		}

		JPanel panel = findWorksheetsJPanel(__lastSelectedWorksheet);
		if (panel != null && !panel.isVisible())
		{
			if (__lastSelectedScrollPane != null)
			{
				// reset the border to its original state
				__lastSelectedScrollPane.setBorder(__originalScrollPaneBorder);
			}

			if (!__lastSelectedWorksheet.stopEditing())
			{
				__lastSelectedWorksheet.cancelEditing();
			}

			__lastSelectedWorksheet = null;
			__lastSelectedScrollPane = null;
			__originalScrollPaneBorder = null;
			__messageJTextField.setText("Currently-selected worksheet: (none)");
			if (!__saveAlwaysEnabled)
			{
				__saveJButton.setEnabled(false);
			}
		}

		int[] arr = calculateVisibleWorksheetsByPanel();

		if (arr[11] == 0)
		{
			__mainJPanelNorth = true;
			getContentPane().remove(__mainJPanel);
			getContentPane().add(__mainJPanel, "North");
		}
		else
		{
			if (__mainJPanelNorth)
			{
				getContentPane().remove(__mainJPanel);
				getContentPane().add(__mainJPanel, "Center");
				__mainJPanelNorth = false;
			}
		}

		if (arr[10] == 1)
		{
			if (__lastSelectedScrollPane != null)
			{
				// reset the border to its original state
				__lastSelectedScrollPane.setBorder((new JScrollPane()).getBorder());
			}

			setPanelsBorder(false);
		}
		else if (arr[11] > 1 && panel != null && panel.isVisible())
		{
			__lastSelectedScrollPane.setBorder(BorderFactory.createLineBorder(Color.blue, 2));
			setPanelsBorder(true);
		}
		else if (arr[11] > 1)
		{
			setPanelsBorder(true);
		}
	}

	/// <summary>
	/// Responds to mouse clicked events; does nothing. </summary>
	/// <param name="e"> the MouseEvent that happened. </param>
	public virtual void mouseClicked(MouseEvent e)
	{
	}

	/// <summary>
	/// Responds to mouse entered events; does nothing. </summary>
	/// <param name="e"> the MouseEvent that happened. </param>
	public virtual void mouseEntered(MouseEvent e)
	{
	}

	/// <summary>
	/// Responds to mouse exited events; does nothing. </summary>
	/// <param name="e"> the MouseEvent that happened. </param>
	public virtual void mouseExited(MouseEvent e)
	{
	}

	/// <summary>
	/// Responds to mouse pressed events, currently only selects the worksheet that was clicked on,
	/// which allows other behavior to be focused on the proper worksheet. </summary>
	/// <param name="e"> the MouseEvent that happened. </param>
	public virtual void mousePressed(MouseEvent e)
	{
		__saveJButton.setEnabled(true);

		if (__lastSelectedScrollPane != null)
		{
			// reset the border to its original state
			__lastSelectedScrollPane.setBorder(__originalScrollPaneBorder);
		}

		JWorksheet last = __lastSelectedWorksheet;
		// Find the worksheet that was clicked on ...

		selectWorksheet(findClickedOnJWorksheet(e), last);
	}

	/// <summary>
	/// Responds to mouse released events; checks to see which JWorksheet was clicked on
	/// and marks it as the most-recently selected JWorksheet. </summary>
	/// <param name="e"> the MouseEvent that happened. </param>
	public virtual void mouseReleased(MouseEvent e)
	{
		mousePressed(e);
	}

	/// <summary>
	/// Called when the user presses the "Save" button.  This gets the time series 
	/// stored in the most-recently selected JWorksheet and writes them to the 
	/// file selected by the user.  How to write the file is determined by the 
	/// extension of the file selected by the user.
	/// </summary>
	private void saveClicked()
	{
		string routine = "saveClicked";
		if (__lastSelectedWorksheet == null)
		{
			__messageJTextField.setText("No worksheets currently selected.  Select one and press 'Save' again.");
			return;
		}

		JGUIUtil.setWaitCursor(this, true);
		string last_directory = JGUIUtil.getLastFileDialogDirectory();
		JFileChooser fc = JFileChooserFactory.createJFileChooser(last_directory);
		fc.setAcceptAllFileFilterUsed(false);
		fc.setDialogTitle("Save Time Series File");
		SimpleFileFilter dff = new SimpleFileFilter("dv", "DateValue Time Series File");
		fc.addChoosableFileFilter(dff);
		SimpleFileFilter tff = new SimpleFileFilter("txt", "Tab-Delimited Text File");
		fc.addChoosableFileFilter(tff);
		SimpleFileFilter cff = new SimpleFileFilter("txt", "Comma-Delimited Text File");
		fc.addChoosableFileFilter(cff);
		fc.setFileFilter(tff);

		if (fc.showSaveDialog(this) != JFileChooser.APPROVE_OPTION)
		{
			JGUIUtil.setWaitCursor(this, false);
			return;
		}

		string directory = fc.getSelectedFile().getParent();
		if (!string.ReferenceEquals(directory, null))
		{
			JGUIUtil.setLastFileDialogDirectory(directory);
		}
		string filename = directory + File.separator + fc.getName(fc.getSelectedFile());

		IList<TS> tslist = ((TSViewTable_TableModel)__lastSelectedWorksheet.getModel()).getTSList();

		if (fc.getFileFilter() == dff)
		{
			try
			{
				DateValueTS.writeTimeSeriesList(tslist, filename);
			}
			catch (Exception e)
			{
				Message.printWarning(1, routine, "Error saving DateValue file \"" + filename + "\"");
				Message.printWarning(1, routine, e);
			}
		}
		else if (fc.getFileFilter() == tff)
		{
			writeTextFile(__lastSelectedWorksheet, '\t', filename);
		}
		else if (fc.getFileFilter() == cff)
		{
			writeTextFile(__lastSelectedWorksheet, ',', filename);
		}
		JGUIUtil.setWaitCursor(this, false);
	}

	/// <summary>
	/// Searches through the listener array associated with worksheets to locate
	/// the JWorksheet for which the JScrollPane area was clicked on. </summary>
	/// <param name="worksheets"> the worksheets associated with a certain data interval </param>
	/// <param name="mouseListeners"> a list of lists containing JScrollPane and JScrollPane
	/// scrollbars (as MouseListeners) used to scroll around the worksheets in the above array </param>
	/// <param name="source"> the object on which a MouseEvent was triggered. </param>
	/// <returns> the JWorksheet that was clicked on, or null if it could not be found </returns>
	private JWorksheet searchListeners(JWorksheet[] worksheets, IList<IList<MouseListener>> mouseListeners, object source)
	{
		if (mouseListeners == null || source == null)
		{
			return null;
		}

		int size = mouseListeners.Count;

		for (int i = 0; i < size; i++)
		{
			IList<MouseListener> v = mouseListeners[i];

			for (int j = 0; j < v.Count; j++)
			{
				if (v[j] == source)
				{
					return worksheets[i];
				}
			}
		}
		return null;
	}

	/// <summary>
	/// Sets a worksheet to be selected, outlining it in blue and also setting the
	/// text at the bottom of the GUI to reflect which worksheet is selected. </summary>
	/// <param name="selectWorksheet"> the worksheet to be selected. </param>
	/// <param name="lastWorksheet"> the worksheet that was previously selected. </param>
	private void selectWorksheet(JWorksheet selectWorksheet, JWorksheet lastWorksheet)
	{
		int[] arr = calculateVisibleWorksheetsByPanel();
		__lastSelectedWorksheet = selectWorksheet;

		if (lastWorksheet != null && lastWorksheet != selectWorksheet)
		{
			if (!lastWorksheet.stopEditing())
			{
				lastWorksheet.cancelEditing();
			}
		}
		// ... and find its scroll pane, too.
		__lastSelectedScrollPane = findWorksheetsScrollPane(selectWorksheet);

		// Back up the scroll pane's current border ...
		__originalScrollPaneBorder = __lastSelectedScrollPane.getBorder();
		// ... and change the scroll pane's border to represent that its worksheet is selected.
		if (arr[10] > 1)
		{
			__lastSelectedScrollPane.setBorder(BorderFactory.createLineBorder(Color.blue, 2));
		}

		TSViewTable_TableModel model = (TSViewTable_TableModel)selectWorksheet.getModel();
		string @base = null;
		switch (model.getIntervalBase())
		{
			case TimeInterval.MINUTE:
				@base = "Minute";
				break;
			case TimeInterval.HOUR:
				@base = "Hour";
				break;
			case TimeInterval.DAY:
				@base = "Day";
				break;
			case TimeInterval.MONTH:
				@base = "Month";
				break;
			case TimeInterval.YEAR:
				@base = "Year";
				break;
			case TimeInterval.IRREGULAR:
				@base = "Irregular";
				break;
			default:
				@base = "???";
				break;
		}

		string s = "";
		if (model.getIntervalBase() == TimeInterval.IRREGULAR)
		{
			// Returns all upper case so change to be consistent with displays
			string prec = TimeInterval.getName(model.getIrregularDateTimePrecision(),0);
			s = @base + " (" + prec + ")";
		}
		else
		{
			if (model.getIntervalMult() == 1)
			{
				s = @base;
			}
			else
			{
				s = "" + model.getIntervalMult() + @base;
			}
		}

		__messageJTextField.setText("Currently-selected worksheet interval: " + s);
	}

	/// <summary>
	/// Sets the column widths for the specified group of worksheets. </summary>
	/// <param name="worksheets"> the worksheets for which to set the column widths. </param>
	private void setColumnWidths(JWorksheet[] worksheets, int precision)
	{
		if (worksheets == null || worksheets.Length == 0)
		{
			return;
		}

		int[] skipCols = new int[] {0};
		int[] widths = null;

		int dateWidth = 16;

		// Get the proper date format
		if (precision == DateTime.PRECISION_HOUR)
		{
			dateWidth = 13;
		}
		else if (precision == DateTime.PRECISION_DAY)
		{
			dateWidth = 10;
		}
		else if (precision == DateTime.PRECISION_MONTH)
		{
			dateWidth = 7;
		}
		else if (precision == DateTime.PRECISION_YEAR)
		{
			dateWidth = 4;
		}
		else if (precision == DateTime.PRECISION_MINUTE)
		{
			dateWidth = 16;
		}

		for (int i = 0; i < worksheets.Length; i++)
		{
			TSViewTable_TableModel model = (TSViewTable_TableModel)worksheets[i].getModel();
			widths = model.getColumnWidths();
			widths[0] = dateWidth;
			worksheets[i].setColumnWidths(widths);
			worksheets[i].calculateColumnWidths(100, 1000, skipCols, getGraphics());
		}
	}

	/// <summary>
	/// Turns on or off the borders for all the panels at once. </summary>
	/// <param name="on"> if true, then the borders for all the panels will be displayed.  If false, they will not. </param>
	public virtual void setPanelsBorder(bool on)
	{
		if (on)
		{
			__minuteJPanel.setBorder(BorderFactory.createTitledBorder("Minute Interval"));
			__hourJPanel.setBorder(BorderFactory.createTitledBorder("Hour Interval"));
			__dayJPanel.setBorder(BorderFactory.createTitledBorder("Day Interval"));
			__monthJPanel.setBorder(BorderFactory.createTitledBorder("Month Interval"));
			__yearJPanel.setBorder(BorderFactory.createTitledBorder("Year Interval"));
			__irregularMinuteJPanel.setBorder(BorderFactory.createTitledBorder("Irregular Interval (Minute)"));
			__irregularMinuteJPanel.setBorder(BorderFactory.createTitledBorder("Irregular Interval (Hour)"));
			__irregularMinuteJPanel.setBorder(BorderFactory.createTitledBorder("Irregular Interval (Day)"));
			__irregularMinuteJPanel.setBorder(BorderFactory.createTitledBorder("Irregular Interval (Month)"));
			__irregularMinuteJPanel.setBorder(BorderFactory.createTitledBorder("Irregular Interval (Year)"));
		}
		else
		{
			__minuteJPanel.setBorder(null);
			__hourJPanel.setBorder(null);
			__dayJPanel.setBorder(null);
			__monthJPanel.setBorder(null);
			__yearJPanel.setBorder(null);
			__irregularMinuteJPanel.setBorder(null);
			__irregularHourJPanel.setBorder(null);
			__irregularDayJPanel.setBorder(null);
			__irregularMonthJPanel.setBorder(null);
			__irregularYearJPanel.setBorder(null);
		}
	}

	/// <summary>
	/// Sets up the GUI. </summary>
	/// <param name="mode"> Indicates whether the GUI should be visible at creation. </param>
	private void setupGUI(bool mode)
	{
		string routine = "TSViewTableJFrame.setupGUI";

		// Start a big try block to set up the GUI...
		try
		{

		// Add a listener to catch window manager events...
		addWindowListener(this);

		// Lay out the main window component by component.  We will start with
		// the menubar default components.  Then add each requested component
		// to the menu bar and the interface...
		GridBagLayout gbl = new GridBagLayout();

		// Add a panel to hold the tables...
		__mainJPanel = new JPanel();
		__mainJPanel.setLayout(gbl);
		getContentPane().add(__mainJPanel);

		// Create all the JCheckboxes
		__minuteJCheckBox = new JCheckBox("Minute Time Series", true);
		__minuteJCheckBox.addItemListener(this);
		__hourJCheckBox = new JCheckBox("Hour Time Series", true);
		__hourJCheckBox.addItemListener(this);
		__dayJCheckBox = new JCheckBox("Day Time Series", true);
		__dayJCheckBox.addItemListener(this);
		__monthJCheckBox = new JCheckBox("Month Time Series", true);
		__monthJCheckBox.addItemListener(this);
		__yearJCheckBox = new JCheckBox("Year Time Series", true);
		__yearJCheckBox.addItemListener(this);
		__irregularMinuteJCheckBox = new JCheckBox("Irregular Time Series (Minute)", true);
		__irregularMinuteJCheckBox.addItemListener(this);
		__irregularHourJCheckBox = new JCheckBox("Irregular Time Series (Hour)", true);
		__irregularHourJCheckBox.addItemListener(this);
		__irregularDayJCheckBox = new JCheckBox("Irregular Time Series (Day)", true);
		__irregularDayJCheckBox.addItemListener(this);
		__irregularMonthJCheckBox = new JCheckBox("Irregular Time Series (Month)", true);
		__irregularMonthJCheckBox.addItemListener(this);
		__irregularYearJCheckBox = new JCheckBox("Irregular Time Series (Year)", true);
		__irregularYearJCheckBox.addItemListener(this);

		// Create the PropList for the JWorksheets
		PropList p = new PropList("TSViewTableJFrame.JWorksheet");
		p.add("JWorksheet.OneClickColumnSelection=true");
		p.add("JWorksheet.RowColumnPresent=true");
		p.add("JWorksheet.ShowPopupMenu=true");
		p.add("JWorksheet.SelectionMode=ExcelSelection");
		p.add("JWorksheet.AllowCopy=true");
		p.add("JWorksheet.AllowPaste=true");
		p.add("JWorksheet.AllowCalculateStatistics=true");
		// Handling of "Calculate Statistics" action event will be delegated in JWorksheet to ActionPerformed here
		p.add("JWorksheet.DelegateCalculateStatistics=true");

		PropList p2 = new PropList("TSViewTableJFrame.JWorksheet");
		p2.add("JWorksheet.RowColumnPresent=true");
		p2.add("JWorksheet.Selectable=false");
		p2.add("JWorksheet.ShowPopupMenu=true");
		p2.add("JWorksheet.SelectionMode=ExcelSelection");

		// Separate the __tslist list into lists of like time series (same base interval and irregular 
		createSeparateTimeSeries();

		// Create all the table models and worksheets
		__dayModels = createTableModels(__day);
		__dayWorksheets = createWorksheets(__dayModels, p);
		JWorksheet[] dayHeaders = createWorksheets(__dayModels, p2);

		__minuteModels = createTableModels(__minute);
		__minuteWorksheets = createWorksheets(__minuteModels, p);
		JWorksheet[] minuteHeaders = createWorksheets(__minuteModels, p2);

		__hourModels = createTableModels(__hour);
		__hourWorksheets = createWorksheets(__hourModels, p);
		JWorksheet[] hourHeaders = createWorksheets(__hourModels, p2);

		__monthModels = createTableModels(__month);
		__monthWorksheets = createWorksheets(__monthModels, p);
		JWorksheet[] monthHeaders = createWorksheets(__monthModels, p2);

		__yearModels = createTableModels(__year);
		__yearWorksheets = createWorksheets(__yearModels, p);
		JWorksheet[] yearHeaders = createWorksheets(__yearModels, p2);

		__irregularMinuteModels = createTableModels(__irregularMinute);
		__irregularMinuteWorksheets = createWorksheets(__irregularMinuteModels, p);
		JWorksheet[] irregularMinuteHeaders = createWorksheets(__irregularMinuteModels, p2);

		__irregularHourModels = createTableModels(__irregularHour);
		__irregularHourWorksheets = createWorksheets(__irregularHourModels, p);
		JWorksheet[] irregularHourHeaders = createWorksheets(__irregularHourModels, p2);

		__irregularDayModels = createTableModels(__irregularDay);
		__irregularDayWorksheets = createWorksheets(__irregularDayModels, p);
		JWorksheet[] irregularDayHeaders = createWorksheets(__irregularDayModels, p2);

		__irregularMonthModels = createTableModels(__irregularMonth);
		__irregularMonthWorksheets = createWorksheets(__irregularMonthModels, p);
		JWorksheet[] irregularMonthHeaders = createWorksheets(__irregularMonthModels, p2);

		__irregularYearModels = createTableModels(__irregularYear);
		__irregularYearWorksheets = createWorksheets(__irregularYearModels, p);
		JWorksheet[] irregularYearHeaders = createWorksheets(__irregularYearModels, p2);

		// Create the panels for the interval bases
		__minuteJPanel = new JPanel();
		__hourJPanel = new JPanel();
		__dayJPanel = new JPanel();
		__monthJPanel = new JPanel();
		__yearJPanel = new JPanel();
		__irregularMinuteJPanel = new JPanel();
		__irregularHourJPanel = new JPanel();
		__irregularDayJPanel = new JPanel();
		__irregularMonthJPanel = new JPanel();
		__irregularYearJPanel = new JPanel();

		// Create the arrays of scroll panes
		if (__dayWorksheets.Length > 0)
		{
			__dayScrollPanes = new JScrollPane[__dayWorksheets.Length];
		}
		if (__minuteWorksheets.Length > 0)
		{
			__minuteScrollPanes = new JScrollPane[__minuteWorksheets.Length];
		}
		if (__hourWorksheets.Length > 0)
		{
			__hourScrollPanes = new JScrollPane[__hourWorksheets.Length];
		}
		if (__monthWorksheets.Length > 0)
		{
			__monthScrollPanes = new JScrollPane[__monthWorksheets.Length];
		}
		if (__yearWorksheets.Length > 0)
		{
			__yearScrollPanes = new JScrollPane[__yearWorksheets.Length];
		}
		if (__irregularMinuteWorksheets.Length > 0)
		{
			__irregularMinuteScrollPanes = new JScrollPane[__irregularMinuteWorksheets.Length];
		}
		if (__irregularHourWorksheets.Length > 0)
		{
			__irregularHourScrollPanes = new JScrollPane[__irregularHourWorksheets.Length];
		}
		if (__irregularDayWorksheets.Length > 0)
		{
			__irregularDayScrollPanes = new JScrollPane[__irregularDayWorksheets.Length];
		}
		if (__irregularMonthWorksheets.Length > 0)
		{
			__irregularMonthScrollPanes = new JScrollPane[__irregularMonthWorksheets.Length];
		}
		if (__irregularYearWorksheets.Length > 0)
		{
			__irregularYearScrollPanes = new JScrollPane[__irregularYearWorksheets.Length];
		}

		// Add the worksheets to the panels and add the panels to the main panel

		if (__reverseNormalOrder)
		{
			__minuteMouseListeners = buildMouseListeners(__minuteWorksheets);
			addWorksheetsToPanel(__mainJPanel, "Minute", __minuteJPanel, __minuteJCheckBox, __minuteWorksheets, minuteHeaders, __minuteScrollPanes, __minuteMouseListeners);

			__hourMouseListeners = buildMouseListeners(__hourWorksheets);
			addWorksheetsToPanel(__mainJPanel, "Hour", __hourJPanel, __hourJCheckBox, __hourWorksheets, hourHeaders, __hourScrollPanes, __hourMouseListeners);

			__dayMouseListeners = buildMouseListeners(__dayWorksheets);
			addWorksheetsToPanel(__mainJPanel, "Day", __dayJPanel, __dayJCheckBox, __dayWorksheets, dayHeaders, __dayScrollPanes, __dayMouseListeners);

			__monthMouseListeners = buildMouseListeners(__monthWorksheets);
			addWorksheetsToPanel(__mainJPanel, "Month", __monthJPanel, __monthJCheckBox, __monthWorksheets, monthHeaders, __monthScrollPanes, __monthMouseListeners);

			__yearMouseListeners = buildMouseListeners(__yearWorksheets);
			addWorksheetsToPanel(__mainJPanel, "Year", __yearJPanel, __yearJCheckBox, __yearWorksheets, yearHeaders, __yearScrollPanes, __yearMouseListeners);

			/*
			if (__irregularMinute != null && __irregularMinute.size() > 0) {
			    __irregularMinuteJPanel.setLayout(new GridBagLayout());
			    JGUIUtil.addComponent(__irregularMinuteJPanel,
					new JLabel("Table view for irregular data is not currently enabled.  Use the summary view."),
					0, 0, 1, 1, 1, 1, 
					GridBagConstraints.NONE, GridBagConstraints.WEST);
				  JGUIUtil.addComponent(__mainJPanel, __irregularMinuteJPanel,
					0, __panelCount++, 1, 1, 1, 1, 
					GridBagConstraints.BOTH, GridBagConstraints.NORTHWEST); 				
			}
			*/
		}
		else
		{
			__yearMouseListeners = buildMouseListeners(__yearWorksheets);
			addWorksheetsToPanel(__mainJPanel, "Year", __yearJPanel, __yearJCheckBox, __yearWorksheets, yearHeaders, __yearScrollPanes, __yearMouseListeners);

			__monthMouseListeners = buildMouseListeners(__monthWorksheets);
			addWorksheetsToPanel(__mainJPanel, "Month", __monthJPanel, __monthJCheckBox, __monthWorksheets, monthHeaders, __monthScrollPanes, __monthMouseListeners);

			__dayMouseListeners = buildMouseListeners(__dayWorksheets);
			addWorksheetsToPanel(__mainJPanel, "Day", __dayJPanel, __dayJCheckBox, __dayWorksheets, dayHeaders, __dayScrollPanes, __dayMouseListeners);

			__hourMouseListeners = buildMouseListeners(__hourWorksheets);
			addWorksheetsToPanel(__mainJPanel, "Hour", __hourJPanel, __hourJCheckBox, __hourWorksheets, hourHeaders, __hourScrollPanes, __hourMouseListeners);

			__minuteMouseListeners = buildMouseListeners(__minuteWorksheets);
			addWorksheetsToPanel(__mainJPanel, "Minute", __minuteJPanel, __minuteJCheckBox, __minuteWorksheets, minuteHeaders, __minuteScrollPanes, __minuteMouseListeners);

			/*
			if (__irregular != null && __irregular.size() > 0) {
			    __irregularJPanel.setLayout(new GridBagLayout());
			    JGUIUtil.addComponent(__irregularJPanel,
					new JLabel("Table view for irregular data is not currently enabled.  Use the summary view."),
					0, 0, 1, 1, 1, 1, 
					GridBagConstraints.NONE, GridBagConstraints.WEST);
				  JGUIUtil.addComponent(__mainJPanel, __irregularJPanel,
					0, __panelCount++, 1, 1, 1, 1, 
					GridBagConstraints.BOTH, GridBagConstraints.NORTHWEST);
			}
			*/
		}

		// Irregular is always last
		__irregularMinuteMouseListeners = buildMouseListeners(__irregularMinuteWorksheets);
		addWorksheetsToPanel(__mainJPanel, "Irregular (Minute)", __irregularMinuteJPanel, __irregularMinuteJCheckBox, __irregularMinuteWorksheets, irregularMinuteHeaders, __irregularMinuteScrollPanes, __irregularMinuteMouseListeners);

		__irregularHourMouseListeners = buildMouseListeners(__irregularHourWorksheets);
		addWorksheetsToPanel(__mainJPanel, "Irregular (Hour)", __irregularHourJPanel, __irregularHourJCheckBox, __irregularHourWorksheets, irregularHourHeaders, __irregularHourScrollPanes, __irregularHourMouseListeners);

		__irregularDayMouseListeners = buildMouseListeners(__irregularDayWorksheets);
		addWorksheetsToPanel(__mainJPanel, "Irregular (Day)", __irregularDayJPanel, __irregularDayJCheckBox, __irregularDayWorksheets, irregularDayHeaders, __irregularDayScrollPanes, __irregularDayMouseListeners);

		__irregularMonthMouseListeners = buildMouseListeners(__irregularMonthWorksheets);
		addWorksheetsToPanel(__mainJPanel, "Irregular (Month)", __irregularMonthJPanel, __irregularMonthJCheckBox, __irregularMonthWorksheets, irregularMonthHeaders, __irregularMonthScrollPanes, __irregularMonthMouseListeners);

		__irregularYearMouseListeners = buildMouseListeners(__irregularYearWorksheets);
		addWorksheetsToPanel(__mainJPanel, "Irregular (Year)", __irregularYearJPanel, __irregularYearJCheckBox, __irregularYearWorksheets, irregularYearHeaders, __irregularYearScrollPanes, __irregularYearMouseListeners);

		JPanel bottomJPanel = new JPanel();
		bottomJPanel.setLayout(gbl);
		__messageJTextField = new JTextField();
		__messageJTextField.setEditable(false);
		JGUIUtil.addComponent(bottomJPanel, __messageJTextField, 0, 1, 7, 1, 1.0, 0.0, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);

		// Put the buttons on the bottom of the window...
		JPanel button_JPanel = new JPanel();
		button_JPanel.setLayout(new FlowLayout(FlowLayout.CENTER));

		__flagJComboBox = new SimpleJComboBox(false);
		IList<string> flagChoices = new List<string>();
		flagChoices.Add("" + TSDataFlagVisualizationType.NOT_SHOWN);
		flagChoices.Add("" + TSDataFlagVisualizationType.SUPERSCRIPT);
		//flagChoices.add("" + TSDataFlagVisualizationType.SEPARATE_COLUMN);
		__flagJComboBox.setData(flagChoices);
		__flagJComboBox.addItemListener(this);
		__flagJComboBox.setToolTipText("Indicate how data flags should be visualized.");

		__graphJButton = new SimpleJButton(__BUTTON_GRAPH, this);
		string propValue = __props.getValue("EnableGraph");
		if ((!string.ReferenceEquals(propValue, null)) && propValue.Equals("false", StringComparison.OrdinalIgnoreCase))
		{
			__graphJButton.setEnabled(false);
		}

		__summaryJButton = new SimpleJButton(__BUTTON_SUMMARY, this);
		propValue = __props.getValue("EnableSummary");
		if ((!string.ReferenceEquals(propValue, null)) && propValue.Equals("false", StringComparison.OrdinalIgnoreCase))
		{
			__summaryJButton.setEnabled(false);
		}

		__closeJButton = new SimpleJButton(__BUTTON_CLOSE,this);

		__saveJButton = new SimpleJButton(__BUTTON_SAVE, this);
		__saveJButton.setEnabled(false);

		JLabel flagJLabel = new JLabel("Flags:");
		button_JPanel.add(flagJLabel);
		button_JPanel.add(__flagJComboBox);
		// Only enable flags display if at least one displayed time series has flags
		if (timeSeriesHaveFlags())
		{
			flagJLabel.setEnabled(true);
			__flagJComboBox.setEnabled(true);
			__flagJComboBox.select("" + TSDataFlagVisualizationType.NOT_SHOWN);
		}
		else
		{
			flagJLabel.setEnabled(false);
			__flagJComboBox.setEnabled(false);
		}
		button_JPanel.add(__graphJButton);
		button_JPanel.add(__summaryJButton);
		button_JPanel.add(__saveJButton);
	//	button_JPanel.add(__printJButton);
		button_JPanel.add(__closeJButton);
		// TODO - add later
		//button_JPanel.add(__helpJButton);

		JGUIUtil.addComponent(bottomJPanel, button_JPanel, 0, 0, 8, 1, 1, 1, GridBagConstraints.HORIZONTAL, GridBagConstraints.CENTER);

		getContentPane().add("South", bottomJPanel);

		__messageJTextField.setText("Currently-selected worksheet: (none)");

		pack();
		// TODO SAM 2012-04-16 Need to default size based on number of time series
		setSize(555,500);
		// Get the UI component to determine screen to display on - needed for multiple monitors
		object uiComponentO = __props.getContents("TSViewParentUIComponent");
		Component parentUIComponent = null;
		if ((uiComponentO != null) && (uiComponentO is Component))
		{
			parentUIComponent = (Component)uiComponentO;
		}
		// Center on the UI component rather than the graph, because the graph screen seems to get tied screen 0?
		JGUIUtil.center(this,parentUIComponent);
		setVisible(mode);

		} // end of try
		catch (Exception e)
		{
			Message.printWarning(2, routine, e);
		}

		// these calls are here because they require a valid graphics 
		// context to work properly (i.e., setVisible(true) must have already been called)
		setColumnWidths(__minuteWorksheets, DateTime.PRECISION_MINUTE);
		setColumnWidths(__hourWorksheets, DateTime.PRECISION_HOUR);
		setColumnWidths(__dayWorksheets, DateTime.PRECISION_DAY);
		setColumnWidths(__monthWorksheets, DateTime.PRECISION_MONTH);
		setColumnWidths(__yearWorksheets, DateTime.PRECISION_YEAR);
		setColumnWidths(__irregularMinuteWorksheets, DateTime.PRECISION_MINUTE);
		setColumnWidths(__irregularHourWorksheets, DateTime.PRECISION_HOUR);
		setColumnWidths(__irregularDayWorksheets, DateTime.PRECISION_DAY);
		setColumnWidths(__irregularMonthWorksheets, DateTime.PRECISION_MONTH);
		setColumnWidths(__irregularYearWorksheets, DateTime.PRECISION_YEAR);

		checkForSingleWorksheet();
	}

	/// <summary>
	/// Determine if any of the time series has flags.
	/// </summary>
	private bool timeSeriesHaveFlags()
	{
		foreach (TS ts in __tslist)
		{
			if (ts.hasDataFlags())
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Responds to window activated events; does nothing. </summary>
	/// <param name="evt"> the WindowEvent that happened. </param>
	public virtual void windowActivated(WindowEvent evt)
	{
	}

	/// <summary>
	/// Responds to window closed events; does nothing. </summary>
	/// <param name="evt"> the WindowEvent that happened. </param>
	public virtual void windowClosed(WindowEvent evt)
	{
	}

	/// <summary>
	/// Responds to window closing events; does nothing. </summary>
	/// <param name="evt"> the WindowEvent that happened. </param>
	public virtual void windowClosing(WindowEvent evt)
	{
		__tsviewJFrame.closeGUI(TSViewType.TABLE);
	}

	/// <summary>
	/// Responds to window deactivated events; does nothing. </summary>
	/// <param name="evt"> the WindowEvent that happened. </param>
	public virtual void windowDeactivated(WindowEvent evt)
	{
	}

	/// <summary>
	/// Responds to window deiconified events; does nothing. </summary>
	/// <param name="evt"> the WindowEvent that happened. </param>
	public virtual void windowDeiconified(WindowEvent evt)
	{
	}

	/// <summary>
	/// Responds to window iconified events; does nothing. </summary>
	/// <param name="evt"> the WindowEvent that happened. </param>
	public virtual void windowIconified(WindowEvent evt)
	{
	}

	/// <summary>
	/// Responds to window opened events; does nothing. </summary>
	/// <param name="evt"> the WindowEvent that happened. </param>
	public virtual void windowOpened(WindowEvent evt)
	{
	}

	/// <summary>
	/// Responds to window opening events; does nothing. </summary>
	/// <param name="evt"> the WindowEvent that happened. </param>
	public virtual void windowOpening(WindowEvent evt)
	{
	}

	/// <summary>
	/// Write all of a worksheet's data to a text file, separating field values
	/// by the given delimiter and separating rows with newlines. </summary>
	/// <param name="worksheet"> the worksheet to dump out the data of. </param>
	/// <param name="delimited"> the character to separate fields with. </param>
	/// <param name="filename"> the text file to write to. </param>
	private void writeTextFile(JWorksheet worksheet, char delimiter, string filename)
	{
		string routine = "writeTextFile";
		try
		{
			StopWatch sw = new StopWatch();
			sw.start();
			PrintWriter @out = new PrintWriter(new FileStream(filename, FileMode.Create, FileAccess.Write));

			__messageJTextField.setText("Saving file \"" + filename + "\"");
			JGUIUtil.forceRepaint(__messageJTextField);
			int rows = worksheet.getRowCount();
			int columns = worksheet.getColumnCount();

			worksheet.startNewConsecutiveRead();

			StringBuilder buff = null;
			int pct = 0;
			int lastPct = 0;
			for (int i = 0; i < rows; i++)
			{
				// Calculate the percentage complete, and if different
				// from the last percentage complete, update the status bar text field.
				pct = ((int)(((double)i / (double)rows) * 100));
				if (pct != lastPct)
				{
					lastPct = pct;
					__messageJTextField.setText("Saving file \"" + filename + "\" (" + pct + "% done)");
					JGUIUtil.forceRepaint(__messageJTextField);
				}

				buff = new StringBuilder();
				for (int j = 0; j < columns; j++)
				{
					buff.Append(worksheet.getConsecutiveValueAt(i, j));
					if (j != (columns - 1))
					{
						buff.Append(delimiter);
					}
				}
				@out.println(buff);
			}

			@out.flush();
			@out.close();
			sw.stop();
			double seconds = sw.getSeconds();
			string plural = "s";

			// Unlikely to happen, but it's just good GUI design to
			// not have something like "1 seconds" or "2 second(s)".
			if (seconds == 1.000)
			{
				plural = "";
			}
			__messageJTextField.setText("File saved (took " + sw.getSeconds() + " second" + plural + ")");
		}
		catch (Exception e)
		{
			__messageJTextField.setText("Error saving file \"" + filename + "\"");
			Message.printWarning(2, routine, "Error saving file \"" + filename + "\"");
			Message.printWarning(2, routine, e);
		}
	}

	/// <summary>
	/// Listens for property change events (from TSGraphEditor) &
	/// Notifies the model listeners (JTables/JWorksheet) that the table model has changed. 
	/// </summary>
	public virtual void propertyChange(PropertyChangeEvent e)
	{
		if (e.getPropertyName().Equals("TS_DATA_VALUE_CHANGE"))
		{
			// Expecting modified TS object
			TSViewTable_TableModel model = findModel(((TS)e.getNewValue()));
			model.fireTableDataChanged();
		}
	}
	/// <summary>
	/// Returns the first model encountered that contains the specified TS.
	/// </summary>
	/// <param name="ts"> </param>
	/// <returns> model containing specified TS, or null </returns>
	private TSViewTable_TableModel findModel(TS ts)
	{
		// TODO: Order search so that most likely models are searched first!

		TSViewTable_TableModel target = null;

		if ((target = findModel(__dayModels,ts)) != null)
		{
			return target;
		}
		else if ((target = findModel(__hourModels,ts)) != null)
		{
			return target;
		}
		else if ((target = findModel(__minuteModels,ts)) != null)
		{
			return target;
		}
		else if ((target = findModel(__monthModels,ts)) != null)
		{
			return target;
		}
		else if ((target = findModel(__yearModels,ts)) != null)
		{
			return target;
		}
		else if ((target = findModel(__irregularMinuteModels,ts)) != null)
		{
			return target;
		}
		else if ((target = findModel(__irregularHourModels,ts)) != null)
		{
			return target;
		}
		else if ((target = findModel(__irregularDayModels,ts)) != null)
		{
			return target;
		}
		else if ((target = findModel(__irregularMonthModels,ts)) != null)
		{
			return target;
		}
		else if ((target = findModel(__irregularYearModels,ts)) != null)
		{
			return target;
		}
		return target;
	}

	/// <summary>
	/// Returns the first model encountered that contains the specified TS. </summary>
	/// <param name="models"> </param>
	/// <param name="ts"> </param>
	/// <returns> model containing specified TS, or null </returns>
	private TSViewTable_TableModel findModel(TSViewTable_TableModel[] models, TS ts)
	{
		if (models != null)
		{
			int nModels = models.Length;
			for (int iModel = 0; iModel < nModels; iModel++)
			{
				TSViewTable_TableModel m = models[iModel];
				IList<TS> tslist = m.getTSList();
				int nVec = tslist.Count;
				for (int iVec = 0; iVec < nVec; iVec++)
				{
					if (tslist[iVec] == ts)
					{
						return m;
					}
				}
			}
		}
		return null;
	}

	}

}