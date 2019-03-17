using System;
using System.Collections.Generic;
using System.IO;

// MessageLogJPanel - a GUI for displaying message log text in order to quickly summarize and move through the log file.

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

//------------------------------------------------------------------------------
// MessageLogJPanel - a GUI for displaying message log text 
// 	in order to quickly summarize and move through the log file.
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 2005-05-26	J. Thomas Sapienza, RTi	Initial version from code in 
//					MessageLogJDialog.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//------------------------------------------------------------------------------

namespace RTi.Util.Message
{





	using JFileChooserFactory = RTi.Util.GUI.JFileChooserFactory;
	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using JScrollWorksheet = RTi.Util.GUI.JScrollWorksheet;
	using JWorksheet = RTi.Util.GUI.JWorksheet;
	using ReportPrinter = RTi.Util.GUI.ReportPrinter;
	using ResponseJDialog = RTi.Util.GUI.ResponseJDialog;
	using SimpleJList = RTi.Util.GUI.SimpleJList;
	using SimpleFileFilter = RTi.Util.GUI.SimpleFileFilter;
	using SimpleJButton = RTi.Util.GUI.SimpleJButton;
	using SimpleJComboBox = RTi.Util.GUI.SimpleJComboBox;
	using SimpleJMenuItem = RTi.Util.GUI.SimpleJMenuItem;
	using TextResponseJDialog = RTi.Util.GUI.TextResponseJDialog;

	using PropList = RTi.Util.IO.PropList;

	using StringUtil = RTi.Util.String.StringUtil;

	using StopWatch = RTi.Util.Time.StopWatch;

	/// <summary>
	/// The MessageLogJPanel class provides a graphical user interface for 
	/// viewing, summarizing, and navigating a log file.  For complete functionality, 
	/// a call to the following should be made in the application code:</p>
	/// <pre>
	/// Message.setPropValue("ShowMessageLevel=true");
	/// Message.setPropValue("ShowMessageTag=true");
	/// </pre>
	/// <para>The first call above results in message levels being shown in log file 
	/// messages, using square brackets.  The second call results in message tags 
	/// being shown in log file messages, using angle brackets.  The following 
	/// example illustrates the syntax of messages:</para>
	/// <pre>
	/// Warning[level]<tag>(routine):  Body of message...
	/// </pre>
	/// <para>The message levels are interpreted by MessageLogJPanel, resulting in 
	/// summary messages being listed in an area at the top of the viewer.  By 
	/// default, warning level 1 and 2 messages are listed in the summary, to 
	/// facilitate troubleshooting.  The summary messages are associated with the 
	/// messages in the full log display, providing a popup menu and simplifying 
	/// navigation of the full message log.</para>
	/// <para> The tags are also interpreted by MessageLogJPanel to allow 
	/// navigation in an application.  Messages with tags are printed using the 
	/// Message.print*(..., String Tag,...) methods.  If no tag is provided, then 
	/// messages will not include the <tag> information.  If the tag is provided, then 
	/// the MessageLogJPanel is able to notify MessageLogListener instances 
	/// when a tagged message is selected and the "Go to..." is chosen.</para>
	/// <para>
	/// An example of using the expanded Message class capability in an 
	/// application that performs clearly defined processing steps is as follows:
	/// </para>
	/// <ol>
	/// <li> Within each process, print important warning messages at level 2, 
	/// where the tag is the command/process count + "," + the warning count (e.g., 
	/// "1,1", "1,2").  Throw an exception if one or more important warnings are 
	/// generated while processing.</li>
	/// <li> For the controller, if an exception is caught, print a warning at level 1 
	/// indicating that a process/command resulted in a warning.  Use a tag that 
	/// indicates the commaand/process count (e.g., "ProcessCommands,1").</li>
	/// <li> Use the <code>Message.addMessageLogListener()</code> method 
	/// from the application code when initializing the application interface.  When a 
	/// user selects a message in the log file summary and then selects the popup 
	/// "Go to..." menu, the application method will receive the selected tag.
	/// The command/process count can then be processed and an appropriate 
	/// action taken (e.g., the command can be highlighted).</li> 
	/// </ol>
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class MessageLogJPanel extends javax.swing.JPanel implements java.awt.event.ActionListener, java.awt.event.MouseListener
	public class MessageLogJPanel : JPanel, ActionListener, MouseListener
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			__style = __STYLE_LOG_AND_SUMMARY;
		}


	/// <summary>
	/// The initial value selected from the second combo box, which determine the 
	/// initial range of warning levels shown in the summary list.  The "second combo 
	/// box" is the right-hand combo box at the top of the GUI.  It belongs to a pair
	/// of combo boxes which are used to filter the visible messages in the summary
	/// list.  Message levels are selected from these combo boxes and only messages
	/// with levels in the range specified by the combo boxes are visible in the list.
	/// </summary>
	private int __INITIAL_END_RANGE_VALUE = 2;

	/// <summary>
	/// The styles in which the GUI can be opened.  The first opens the GUI with 
	/// a summary of the warning level messages and the second opens and 
	/// only displays the log file.
	/// </summary>
	private readonly int __STYLE_LOG_AND_SUMMARY = 0, __STYLE_LOG = 1;

	/// <summary>
	/// Popup menu Strings.
	/// </summary>
	private readonly string __MENU_FIND_IN_WORKSHEET = "Find ...", __MENU_GO_TO_LOG_MESSAGE = "Go to log message", __MENU_GO_TO_ORIGINAL_COMMAND = "Go to original command";

	/// <summary>
	/// Button strings.
	/// </summary>
	private readonly string __BUTTON_CLOSE = "Close", __BUTTON_OPEN = "Open", __BUTTON_PRINT_LOG_FILE = "Print Log File", __BUTTON_PRINT_SUMMARY = "Print Summary", __BUTTON_REFRESH = "Refresh";

	/// <summary>
	/// The number of lines in the log file.  Used when moving around in the worksheet.
	/// </summary>
	private int __logFileLines = 0;

	/// <summary>
	/// The number of lines to display at once in the summary list.  Default is 8.
	/// </summary>
	private int __summaryListSize = 8;

	/// <summary>
	/// The style in which the GUI is to be displayed.  Possible options allow the
	/// display of the summary list and the log file text, or just the log file
	/// text.  See STYLE_*.
	/// </summary>
	private int __style;

	/// <summary>
	/// The parent JDialog on which this dialog is opened.  If this is not null, 
	/// __parentJFrame <i>will</i> be null.
	/// </summary>
	private MessageLogJDialog __parentJDialog = null;

	/// <summary>
	/// The parent JFrame on which this dialog is opened.  If this is not null, 
	/// __parentJDialog<i>will</i> be null.
	/// </summary>
	private MessageLogJFrame __parentJFrame = null;

	/// <summary>
	/// The GUI panel that holds the buttons on the GUI.
	/// </summary>
	private JPanel __buttonJPanel = null;

	/// <summary>
	/// The panel in which the summary list is displayed.  See setupSummaryJPanel()
	/// for information on why this is used.
	/// </summary>
	private JPanel __summaryJPanel;

	/// <summary>
	/// The popup menu that appears when a message is right-clicked on in the 
	/// summary list.
	/// </summary>
	private JPopupMenu __popup = null;

	/// <summary>
	/// The worksheet that will be used to display a log file.
	/// </summary>
	private JWorksheet __worksheet = null;

	/// <summary>
	/// The list in which the log file summary information is displayed.
	/// </summary>
	private SimpleJList __summaryList = null;

	/// <summary>
	/// Combo boxes for selecting the range of warning and status levels to filter the 
	/// summary list for.
	/// </summary>
	private SimpleJComboBox __levelNumBox1 = null, __levelNumBox2 = null;

	/// <summary>
	/// The popup menu item for going to a command in an application.
	/// </summary>
	private SimpleJMenuItem __goToCommandMenuItem;

	/// <summary>
	/// The file that was opened and from which log file information was read.  This is
	/// an absolute path to the file.
	/// </summary>
	private string __filePath = null;

	/// <summary>
	/// The last String that was searched for in the Message GUI.  If this is null,
	/// then no previous searches have been made.  This is used to automatically 
	/// populate the search dialog.
	/// </summary>
	private string __lastSearch = null;

	/// <summary>
	/// This list holds a subset of the values in the __posVector, and is regenerated
	/// every time a new range of values is selected from the combo boxes at the
	/// top of the GUI.  The subset of values will be the positions in the __posVector
	/// List of only those messages with warning levels between the selected combo box values.
	/// </summary>
	private IList<int> __filteredPosVector = null;

	/// <summary>
	/// This list holds a subset of the values in the __summaryData list, and is
	/// generated every time a new range of values is selected from the combo boxes
	/// at the top of the GUI.  The subset of values will be the messages that are
	/// currently visible in the summary list.
	/// </summary>
	private IList<string> __filteredSummaryListData = null;

	/// <summary>
	/// The MessageLogListeners that are registered to be notified from this
	/// class.  The listeners are redefined from Message every time a new log file is processed.
	/// </summary>
	private IList<MessageLogListener> __listeners = null;

	/// <summary>
	/// The list of values that will be displayed in the worksheet.  Each element
	/// is a single line of log file text.
	/// </summary>
	private IList<string> __logFileText = null;

	/// <summary>
	/// This list holds the lines that should be put into the summary list, in the
	/// order they were encountered in the log file.
	/// </summary>
	private IList<string> __messageVector = null;

	/// <summary>
	/// The list is tied to the __messageVector -- it holds the positions within the
	/// file at which the corresponding element in the __messageVector were found.  Thus
	/// if __messageVector.elementAt(10) is "Warning[2]: An error was found.", 
	/// __posVector.elementAt(10) is the line at which that warning was found.
	/// </summary>
	private IList<int> __posVector = null;

	/// <summary>
	/// The data that should go in the summary list if all warning levels should be visible.
	/// </summary>
	private IList<string> __summaryListData = null;

	/// <summary>
	/// Constructor.  Reads the log file information from the log file written to 
	/// by Message.  The log file is determined by a call to message.getLogFile(). </summary>
	/// <param name="props"> the properties to open the log file with.  Can be null.  If not
	/// null, the valid properties are:<para>
	/// <table>
	/// <tr>
	/// <th>Property Name</th> <th>Property Description</th> 
	/// <th>Default Value (if not defined)</th>
	/// </tr>
	/// 
	/// <tr>
	/// <td>InitialMaximumWarningLevel</td>
	/// <td>The initial high end of the range of warning levels to see in the summary
	/// list.  The initial low end of the range is "1".  All messages with levels 
	/// between 1 and InitialmMaximumWarningLevel (inclusive) will be shown when the
	/// GUI first opens.
	/// </td>
	/// <td>2</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>SummaryListVisibleRowCount</td>
	/// <td>The number of lines that the log file summary list should display
	/// at once.
	/// </td>
	/// <td>8</td>
	/// </tr>
	/// 
	/// </table> </param>
	/// <summary>
	/// Constructor. </summary>
	/// <param name="parent"> the parent JDialog in which this panel will be placed.  Cannot
	/// be null. </param>
	/// <param name="props"> the PropList that controls the log viewer properties. </param>
	/// <exception cref="Exception"> if the parent is null. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MessageLogJPanel(MessageLogJDialog parent, RTi.Util.IO.PropList props) throws Exception
	public MessageLogJPanel(MessageLogJDialog parent, PropList props)
	{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		if (parent == null)
		{
			throw new Exception("Cannot open a MessageLogJPanel in a " + "null dialog.");
		}
		__parentJDialog = parent;
		processPropList(props);
		setupJPanel();
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="parent"> the parent JFrame in which this panel will be placed. </param>
	/// <param name="props"> the PropList that controls the log viewer properties.  See the
	/// first constructor for valid properties. </param>
	/// <exception cref="Exception"> if the parent is null. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MessageLogJPanel(MessageLogJFrame parent, RTi.Util.IO.PropList props) throws Exception
	public MessageLogJPanel(MessageLogJFrame parent, PropList props)
	{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		if (parent == null)
		{
			throw new Exception("Cannot open a MessageLogJPanel in a " + "null dialog.");
		}
		__parentJFrame = parent;
		processPropList(props);
		setupJPanel();
	}

	/// <summary>
	/// Responds to action events. </summary>
	/// <param name="ActionEvent"> the ActionEvent that happened. </param>
	public virtual void actionPerformed(ActionEvent @event)
	{
		string command = @event.getActionCommand();
		if (@event.getSource() == __levelNumBox1 || @event.getSource() == __levelNumBox2)
		{
				filterSummaryList();
		}
		else if (command.Equals(__BUTTON_CLOSE))
		{
			if (__parentJFrame != null)
			{
				__parentJFrame.setVisible(false);
				__parentJFrame.dispose();
			}
			else
			{
				__parentJDialog.setVisible(false);
				__parentJDialog.dispose();
			}
		}
		else if (command.Equals(__BUTTON_OPEN))
		{
			openLogFile();
		}
		else if (command.Equals(__BUTTON_PRINT_LOG_FILE))
		{
			printLogFile();
		}
		else if (command.Equals(__BUTTON_PRINT_SUMMARY))
		{
			printSummary();
		}
		else if (command.Equals(__BUTTON_REFRESH))
		{
			refresh();
		}
		else if (command.Equals(__MENU_FIND_IN_WORKSHEET))
		{
			findInWorksheet();
		}
		else if (command.Equals(__MENU_GO_TO_LOG_MESSAGE))
		{
			moveToLogMessage();
		}
		else if (command.Equals(__MENU_GO_TO_ORIGINAL_COMMAND))
		{
			moveToOriginalCommand();
		}
	}

	/// <summary>
	/// Builds the combo boxes that hold the warning level range values.  This is a 
	/// helper method called by setupJPanel().
	/// </summary>
	private void buildLevelNumComboBoxes()
	{
		IList<string> v = new List<string>();
		for (int i = 1; i <= 100; i++)
		{
			v.Add("" + i);
		}
		__levelNumBox1 = new SimpleJComboBox(v);
		__levelNumBox2 = new SimpleJComboBox(v);
		__levelNumBox2.select("" + __INITIAL_END_RANGE_VALUE);
		__levelNumBox1.addActionListener(this);
		__levelNumBox2.addActionListener(this);
	}

	/// <summary>
	/// Filters and repopulates the summary list in response to a new selection 
	/// being made in one of the combo boxes.
	/// </summary>
	private void filterSummaryList()
	{
		if (__style != __STYLE_LOG_AND_SUMMARY)
		{
			return;
		}

		// Determine the range of values selected
		int index1 = __levelNumBox1.getSelectedIndex();
		int index2 = __levelNumBox2.getSelectedIndex();
		int size = __levelNumBox1.getItemCount();

		// If the range is the full range, then it's a trivial case -- set
		// the filtered position vector to contain all position values and 
		// fill the summary list with all warnings that were found.

		if (index1 == 0 && index2 == (size - 1))
		{
			__filteredPosVector = __posVector;
			__filteredSummaryListData = __summaryListData;
			setupSummaryJPanel(__summaryListData, false);
			return;
		}
		else if (index2 == 0 && index1 == (size - 1))
		{
			__filteredPosVector = __posVector;
			__filteredSummaryListData = __summaryListData;
			setupSummaryJPanel(__summaryListData, false);
			return;
		}

		// Otherwise, need to go through all the values that can go in the
		// summary list and remove those with warning levels outside the 
		// acceptable range.  This subset of values will be placed in the
		// summary list, and the subset of the values' positions will
		// be placed in the __filteredPosVector.

		int num1 = StringUtil.atoi(__levelNumBox1.getSelected());
		int num2 = StringUtil.atoi(__levelNumBox2.getSelected());

		if (num2 < num1 && num2 != num1)
		{
			int num3 = num2;
			num2 = num1;
			num1 = num3;
		}

		int num = 0;
		size = __summaryListData.Count;
		string s = null;
		__filteredPosVector = new List<int>();
		__filteredSummaryListData = new List<string>();
		for (int i = 0; i < size; i++)
		{
			s = __summaryListData[i];
			index1 = s.IndexOf("[", StringComparison.Ordinal);
			index2 = s.IndexOf("]", StringComparison.Ordinal);
			num = StringUtil.atoi(s.Substring(index1 + 1, index2 - (index1 + 1)));
			if (num >= num1 && num <= num2)
			{
				__filteredSummaryListData.Add(s);
				__filteredPosVector.Add(__posVector[i]);
			}
		}

		setupSummaryJPanel(__filteredSummaryListData, false);
	}

	/// <summary>
	/// Called when the user selects to find text in the message log.  Opens a 
	/// dialog for the text to search for and runs the search.
	/// </summary>
	private void findInWorksheet()
	{
		string text = null;
		if (string.ReferenceEquals(__lastSearch, null))
		{
			text = (new TextResponseJDialog(__parentJFrame, "Find Text in Log File", "Enter the text to search for in the log file.\n" + "The search is case-insensitive.", ResponseJDialog.OK | ResponseJDialog.CANCEL)).response();
		}
		else
		{
			text = (new TextResponseJDialog(__parentJFrame, "Find Text in Log File", "Enter the text to search for in the log file.\n" + "The search is case-insensitive.", __lastSearch, ResponseJDialog.OK | ResponseJDialog.CANCEL)).response();
		}

		if (string.ReferenceEquals(text, null))
		{
			return;
		}

		__lastSearch = text;

		int startingRow = __worksheet.getSelectedRow();
		if (startingRow == -1)
		{
			startingRow = 0;
		}

		int row = __worksheet.find(text, 1, startingRow, JWorksheet.FIND_WRAPAROUND | JWorksheet.FIND_CONTAINS | JWorksheet.FIND_CASE_INSENSITIVE);

		if (row == -1)
		{
			return;
		}

		__worksheet.scrollToRow(row);
		__worksheet.selectRow(row);
	}

	/// <summary>
	/// Returns the button JPanel.  Used by MessageLogJDialog. </summary>
	/// <returns> the button JPanel. </returns>
	public virtual JPanel getButtonJPanel()
	{
		return __buttonJPanel;
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void mouseClicked(MouseEvent @event)
	{
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
	/// Checks to see if a popup menu needs to be shown, and if so, shows it.  This 
	/// is when the popup check should occur for UNIX-based machines. </summary>
	/// <param name="event"> the MouseEvent that occurred. </param>
	public virtual void mousePressed(MouseEvent @event)
	{
		if (__popup.isPopupTrigger(@event))
		{
			showPopup(@event);
		}
	}

	/// <summary>
	/// Checks to see if a popup menu needs to be shown, and if so, shows it.  This 
	/// is when the popup check should occur for Windows machines. </summary>
	/// <param name="event"> the MouseEvent that occurred. </param>
	public virtual void mouseReleased(MouseEvent @event)
	{
		if (__popup.isPopupTrigger(@event))
		{
			showPopup(@event);
		}
	}

	/// <summary>
	/// Moves to a line in the log list with a tag that is currently selected in 
	/// the summary list, and make the line visible in the log list.
	/// </summary>
	private void moveToLogMessage()
	{
		string item = (string)__summaryList.getSelectedItem();
		if (string.ReferenceEquals(item, null))
		{
			return;
		}

		int index = item.IndexOf("[", StringComparison.Ordinal);
		int index2 = item.IndexOf("]", StringComparison.Ordinal);
		string levelNum = item.Substring(index + 1, index2 - (index + 1));

		if (string.ReferenceEquals(levelNum, null))
		{
			// This shouldn't happen, but it's best to check.
			return;
		}

		int? I = (int?)__filteredPosVector[__summaryList.getSelectedIndex()];
		__worksheet.selectRow(I.Value);
		__worksheet.scrollToRow(__logFileLines - 1);
		__worksheet.scrollToRow(I.Value);
	}

	/// <summary>
	/// Alerts the listeners to move to the original command that generated the tag
	/// on which the popup menu was opened.
	/// </summary>
	private void moveToOriginalCommand()
	{
		if (__listeners == null)
		{
			return;
		}

		string item = (string)__summaryList.getSelectedItem();
		if (string.ReferenceEquals(item, null))
		{
			return;
		}

		int openIndex = item.IndexOf("<", StringComparison.Ordinal);
		int closeIndex = item.IndexOf(">", StringComparison.Ordinal);
		item = item.Substring(openIndex + 1, closeIndex - (openIndex + 1));

		int size = __listeners.Count;
		for (int i = 0; i < size; i++)
		{
			((MessageLogListener)__listeners[i]).goToMessageTag(item);
		}
	}

	/// <summary>
	/// Called when the Open button is pressed, this opens a file chooser and allows the
	/// user to select which log file they would like to open and display in the GUI.
	/// </summary>
	private void openLogFile()
	{
		string routine = "MessageLogJPanel.openLogFile";

		JGUIUtil.setWaitCursor(this, true);
		string lastDirectorySelected = JGUIUtil.getLastFileDialogDirectory();

		JFileChooser fc = JFileChooserFactory.createJFileChooser(lastDirectorySelected);

		fc.setDialogTitle("Select Log File");
		SimpleFileFilter logFF = new SimpleFileFilter("log", "Log Files");
		fc.addChoosableFileFilter(logFF);
		fc.setAcceptAllFileFilterUsed(true);
		fc.setFileFilter(logFF);
		fc.setDialogType(JFileChooser.OPEN_DIALOG);

		int retVal = fc.showOpenDialog(this);
		if (retVal != JFileChooser.APPROVE_OPTION)
		{
			JGUIUtil.setWaitCursor(this, false);
			return;
		}

		string currDir = (fc.getCurrentDirectory()).ToString();

		if (!currDir.Equals(lastDirectorySelected, StringComparison.OrdinalIgnoreCase))
		{
			JGUIUtil.setLastFileDialogDirectory(currDir);
		}
		string path = fc.getSelectedFile().getPath();
		try
		{
			JGUIUtil.setWaitCursor(this, true);
			processLogFile(path);
			JGUIUtil.setWaitCursor(this, false);
		}
		catch (Exception e)
		{
			Message.printWarning(1, routine, "Unable to open file: " + path);
			Message.printWarning(2, routine, e);
		}
	}

	/// <summary>
	/// Prints the log file text.
	/// </summary>
	private void printLogFile()
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<String> logLines = (java.util.List<String>)__worksheet.getAllData();
		IList<string> logLines = (IList<string>)__worksheet.getAllData();
		ReportPrinter.printText(logLines, 66, 44, "" + __filePath, false, null); // do not use a pre-defined PageFormat for this print job
	}

	/// <summary>
	/// Prints the summary information text.
	/// </summary>
	private void printSummary()
	{
		ReportPrinter.printText(__filteredSummaryListData, 66, 44, "Message Log Summary", false, null); // do not use a pre-defined PageFormat for
	}

	/// <summary>
	/// Processes a log file in order to display its text and summary
	/// information in the GUI.  This method is called internally and can be called
	/// by external code to reprocess a log file.  This method will re-read the 
	/// MessageLogListeners that were set in Message.addMessageLogListener(). </summary>
	/// <param name="filename"> the name of the file to read and process. </param>
	/// <exception cref="Exception"> if the file cannot be found or there is an error reading
	/// from the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void processLogFile(String filename) throws Exception
	public virtual void processLogFile(string filename)
	{
		__filePath = filename;
		__logFileLines = 0;
		__listeners = Message.getMessageLogListeners();
		readLogFile();
		if (__style == __STYLE_LOG_AND_SUMMARY)
		{
			setupSummaryJPanel(__messageVector, true);
		}
		StopWatch sw = new StopWatch();
		sw.start();
		MessageLogTableModel model = new MessageLogTableModel(__logFileText);
		MessageLogCellRenderer cr = new MessageLogCellRenderer(model);
		__worksheet.setCellRenderer(cr);
		__worksheet.setShowHorizontalLines(false);
			// do not show black horizontal lines between each line in
			// the table
		__worksheet.setTableHeader(null);
		__worksheet.removeColumnHeader();
			// turn off the table header, which typically holds the name of the column.
		__worksheet.setModel(model);
		__worksheet.setColumnWidths(cr.getColumnWidths());
		sw.stop();

		string plural = "s";
		if (__logFileLines == 1)
		{
			plural = "";
		}
		setBorder(BorderFactory.createTitledBorder("Log File Contents - " + __filePath + " - " + __logFileLines + " line" + plural));
		filterSummaryList();
	}

	/// <summary>
	/// Processes a prop list and sets internal variables.  See the constructor for
	/// the list of recognized properties. </summary>
	/// <param name="props"> the PropList to process.  Can be null. </param>
	private void processPropList(PropList props)
	{
		if (props == null)
		{
			return;
		}

		string s = null;

		s = Message.getPropValue("ShowMessageTag");
		if (!string.ReferenceEquals(s, null))
		{
			if (s.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				__style = __STYLE_LOG_AND_SUMMARY;
			}
			else
			{
				__style = __STYLE_LOG;
			}
		}

		s = props.getValue("InitialMaximumWarningLevel");
		if (!string.ReferenceEquals(s, null))
		{
			__INITIAL_END_RANGE_VALUE = StringUtil.atoi(s);
		}

		s = props.getValue("SummaryListVisibleRowCount");
		if (!string.ReferenceEquals(s, null))
		{
			__summaryListSize = StringUtil.atoi(s);
		}
	}

	/// <summary>
	/// Takes a warning line from the log file and fills in the required information
	/// in the Vectors and Hashtables that record where warning lines occur within the
	/// log file. </summary>
	/// <param name="line"> the text of the line for which to determine tag information. </param>
	/// <param name="pos"> the line number in the log file at which the line occurred, base 0. </param>
	private void processWarningLine(string line, int pos)
	{
		int bracketIndex = line.IndexOf("]", StringComparison.Ordinal);
		string num = line.Substring(8, bracketIndex - 8);
			// The "8" takes care of the "Warning" at the beginning of
			// the line.
		int val = StringUtil.atoi(num);
		if (val < 1)
		{
			// Don't process any lines that don't have warning levels.
			return;
		}

		int openIndex = line.IndexOf("<", StringComparison.Ordinal);
		int closeIndex = line.IndexOf(">", StringComparison.Ordinal);

		if (val == 1)
		{
			// Always include in the summary any warnings printed at 
			// level 1 -- even if they don't have tags.
		}
		else if (openIndex <= 0 || closeIndex <= 0 || closeIndex < openIndex)
		{
			// If the angle brackets aren't found, or are incorrectly 
			// positioned, this is a line without a tag, and so it 
			// won't be added to the summary list.
		}

		__messageVector.Add(line);
		__posVector.Add(new int?(pos));
		// Initially, the filtered position vector must contain the same 
		// information as the unfiltered.
		if (StringUtil.atoi(num) <= (__INITIAL_END_RANGE_VALUE))
		{
			__filteredPosVector.Add(new int?(pos));
		}
	}

	/// <summary>
	/// Sets up the main hashtable that contains warning levels, tags and the positions
	/// of the tags within the log file.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void readLogFile() throws Exception
	private void readLogFile()
	{
		__messageVector = new List<string>();
		__filteredPosVector = new List<int>();
		__filteredSummaryListData = new List<string>();
		__posVector = new List<int>();
		StopWatch sw = new StopWatch();
		sw.clear();
		sw.start();
		StreamReader @in = new StreamReader(__filePath);
		string line = @in.ReadLine();

		__logFileText = new List<string>();

		bool doProcessing = false;
		if (__style == __STYLE_LOG_AND_SUMMARY)
		{
			doProcessing = true;
		}

		while (!string.ReferenceEquals(line, null))
		{
			__logFileLines++;
			__logFileText.Add(line);
			line = line.Trim();
			if (doProcessing && line.StartsWith("Warning", StringComparison.Ordinal))
			{
				if (!line.StartsWith("Warning[", StringComparison.Ordinal))
				{
					// ignore -- no warning level
				}
				else
				{
					processWarningLine(line, __logFileLines - 1);
				}
			}

			line = @in.ReadLine();
		}
		@in.Close();

		sw.stop();
	}

	/// <summary>
	/// Refresh the log file display by reloading the log file and positioning at the last row.
	/// </summary>
	private void refresh()
	{
		string routine = this.GetType().Name + ".refresh";
		// Reprocess the same file
		string path = __filePath;
		try
		{
			JGUIUtil.setWaitCursor(this, true);
			// Reread the log file
			processLogFile(path);
			// Position the cursor at the bottom
			__worksheet.scrollToLastRow();
			JGUIUtil.setWaitCursor(this, false);
		}
		catch (Exception e)
		{
			Message.printWarning(1, routine, "Unable to open file: " + path);
			Message.printWarning(2, routine, e);
		}
	}

	/// <summary>
	/// Sets up the button JPanel
	/// </summary>
	private void setupButtonJPanel()
	{
		__buttonJPanel = new JPanel();
		SimpleJButton printLogFileButton = new SimpleJButton(__BUTTON_PRINT_LOG_FILE, __BUTTON_PRINT_LOG_FILE, this);
		printLogFileButton.setToolTipText("Print the log file text.");
		SimpleJButton printSummaryButton = new SimpleJButton(__BUTTON_PRINT_SUMMARY, __BUTTON_PRINT_SUMMARY, this);
		printSummaryButton.setToolTipText("Print the summary list text.");
		SimpleJButton refreshButton = new SimpleJButton(__BUTTON_REFRESH, __BUTTON_REFRESH, this);
		refreshButton.setToolTipText("Reload the log file and position at the end.");
		SimpleJButton openButton = new SimpleJButton(__BUTTON_OPEN, __BUTTON_OPEN, this);
		openButton.setToolTipText("Open and display a new log file.");
		SimpleJButton closeButton = new SimpleJButton(__BUTTON_CLOSE, __BUTTON_CLOSE, this);
		closeButton.setToolTipText("Close the window.");

		__buttonJPanel.add(printLogFileButton);
		if (__style == __STYLE_LOG_AND_SUMMARY)
		{
			__buttonJPanel.add(printSummaryButton);
		}
		__buttonJPanel.add(refreshButton);
		__buttonJPanel.add(openButton);
		__buttonJPanel.add(closeButton);
	}

	/// <summary>
	/// Sets up the GUI.
	/// </summary>
	private void setupJPanel()
	{
		setupPopupMenus();

		if (__style == __STYLE_LOG_AND_SUMMARY)
		{
			setupSummaryJPanel(null, true);
		}

		setLayout(new GridBagLayout());
		setBorder(BorderFactory.createTitledBorder("Log File Contents - [no file]"));
		PropList p = new PropList("");
		p.add("JWorksheet.ShowRowHeader=false");
		p.add("JWorksheet.AllowCopy=true");
		p.add("JWorksheet.ShowPopupMenu=true");
		p.add("JWorksheet.SelectionMode=ExcelSelection");
		p.add("JWorksheet.OneClickRowSelection=true");
		p.add("JWorksheet.RowColumnPresent=false");
		p.add("JWorksheet.CellFontName=Courier");

		JScrollWorksheet jsw = new JScrollWorksheet(0, 0, p);
			__worksheet = jsw.getJWorksheet();

		JPopupMenu worksheetPopup = new JPopupMenu();
		SimpleJMenuItem menuItem = new SimpleJMenuItem(__MENU_FIND_IN_WORKSHEET, this);
		worksheetPopup.add(menuItem);
		worksheetPopup.addSeparator();
		__worksheet.setPopupMenu(worksheetPopup, true);

		if (__parentJFrame != null)
		{
			__worksheet.setHourglassJFrame(__parentJFrame);
		}
		else
		{
			__worksheet.setHourglassJDialog(__parentJDialog);
		}
		JGUIUtil.addComponent(this, jsw, 0, 1, 3, 3, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.CENTER);
		setupButtonJPanel();
	}

	/// <summary>
	/// Sets up the popup menus that will appear when right-clicking on messages in the
	/// summary list.
	/// </summary>
	private void setupPopupMenus()
	{
		__popup = new JPopupMenu();

		__goToCommandMenuItem = new SimpleJMenuItem(__MENU_GO_TO_ORIGINAL_COMMAND, this);
		__popup.add(__goToCommandMenuItem);
		__popup.addSeparator();
		SimpleJMenuItem mi = new SimpleJMenuItem(__MENU_GO_TO_LOG_MESSAGE,this);
		__popup.add(mi);
	}

	/// <summary>
	/// Sets up the panel that displays the summary list.  This method is necessary
	/// because JTS kept encountering abnormal behavior with trying to simply call
	/// 'setListData()' on the summary list.  The resulting list had the right number
	/// of elements, but the first element displayed incorrectly and caused GUI 
	/// problems when the frame was resized.  Hence, the following method.<para>
	/// This method will remove the panel currently displaying the summary list from
	/// the GUI, rebuild the summary list with the new data, put the panel back
	/// </para>
	/// into the GUI and force it to repaint itself.<para>
	/// This should be REVISIT ed when RTi moves to JDK 1.5.0 and above.  
	/// </para>
	/// </summary>
	/// <param name="data"> the data that should be displayed in the list.  If null, an empty
	/// list will be constructed. </param>
	/// <param name="initialSetup"> if true then this is the first time the summary is being 
	/// set up for a log file.   </param>
	private void setupSummaryJPanel(IList<string> data, bool initialSetup)
	{
		StopWatch sw = new StopWatch();
		sw.start();
		if (__summaryJPanel != null)
		{
			if (__parentJFrame != null)
			{
				__parentJFrame.remove(__summaryJPanel);
			}
			else
			{
				__parentJDialog.remove(__summaryJPanel);
			}
		}

		__summaryJPanel = new JPanel();
		__summaryJPanel.setLayout(new GridBagLayout());

		int y = 0;
		if (initialSetup)
		{
			buildLevelNumComboBoxes();
		}
		JGUIUtil.addComponent(__summaryJPanel, new JLabel("Show messages for levels: "), 0, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(__summaryJPanel, __levelNumBox1, 1, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(__summaryJPanel, new JLabel(" to: "), 2, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(__summaryJPanel, __levelNumBox2, 3, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		y++;

		if (data == null)
		{
			__summaryList = new SimpleJList();
		}
		else
		{
			__summaryList = new SimpleJList(new List<string>(data));
		}
		__summaryList.setFont(new java.awt.Font("Courier", java.awt.Font.PLAIN, 11));

		if (initialSetup)
		{
			__summaryListData = data;
			__filteredSummaryListData = data;
		}

		string plural = "s";
		int size = 0;
		if (data != null)
		{
			size = data.Count;
			if (size == 1)
			{
				plural = "";
			}
		}

		int count1 = 0;
		int count2 = 0;
		int index1 = 0;
		int index2 = 0;
		int num = 0;
		string s = null;
		for (int i = 0; i < size; i++)
		{
			s = (string)data[i];
			index1 = s.IndexOf("[", StringComparison.Ordinal);
			index2 = s.IndexOf("]", StringComparison.Ordinal);
			num = StringUtil.atoi(s.Substring(index1 + 1, index2 - (index1 + 1)));
			if (num == 1)
			{
				count1++;
			}
			else if (num == 2)
			{
				count2++;
			}
		}

		__summaryJPanel.setBorder(BorderFactory.createTitledBorder("Log File Summary - " + size + " message" + plural + " (" + count1 + " Warning[1], " + count2 + " Warning[2])"));

		__summaryList.setSelectionMode(ListSelectionModel.SINGLE_SELECTION);
		__summaryList.setVisibleRowCount(__summaryListSize);
		__summaryList.addMouseListener(this);
		JGUIUtil.addComponent(__summaryJPanel, new JScrollPane(__summaryList), 0, y, 5, 5, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.CENTER);

		if (__parentJFrame != null)
		{
			__parentJFrame.add(__summaryJPanel);
		}
		else
		{
			__parentJDialog.add(__summaryJPanel);
		}

		__summaryList.repaint();
		sw.stop();
	}

	/// <summary>
	/// Shows a popup menu if appropriate for the given event and the item
	/// currently selected in the summary list.
	/// </summary>
	private void showPopup(MouseEvent @event)
	{
		if (__summaryList.getItemCount() <= 0)
		{
			return;
		}
		string item = (string)__summaryList.getSelectedItem();
		if (string.ReferenceEquals(item, null))
		{
			return;
		}

		int closeIndex = item.IndexOf(">", StringComparison.Ordinal);
		bool foundTag = false;
		if (closeIndex > -1)
		{
			int openIndex = item.IndexOf("<", StringComparison.Ordinal);
			if (openIndex < closeIndex)
			{
	//			if (StringUtil.isInteger(num)) {
					item = item.Substring(0, closeIndex + 1);
					foundTag = true;
	//			}
			}
		}

		__goToCommandMenuItem.setEnabled(foundTag);
		__popup.show(@event.getComponent(), @event.getX(), @event.getY());
	}

	}

}