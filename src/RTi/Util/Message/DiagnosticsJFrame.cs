using System;
using System.Threading;

// DiagnosticsJFrame - generic diagnostic preferences window.

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
// DiagnosticsJFrame - generic diagnostic preferences window.
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 07 Oct 1997	Catherine E.		Created initial version of class
//		Nutting-Lane, RTi	for StateModGUI.
// 14 Nov 1997	CEN, RTi		Moved to message class for global use
// 16 Mar 1998	Steven A. Malers, RTi	Add javadoc.
// 14 Apr 1998	SAM, RTi		Change the menu to Diagnostics...
// 27 Apr 1998	SAM, RTi		Add option for a help button.
//					Simplify some of the button code by
//					using SimpleButton.
// 12 Oct 1998	SAM, RTi		Add View Log File button but do not
//					enable.
// 07 Dec 1998	SAM, RTi		Enable View Log File.  Do some other
//					clean-up.
// 15 Mar 2001	SAM, RTi		Change IO to IOUtil and GUI to JGUIUtil.
//					Clean up imports, javadoc and memory
//					management.  Check for platform when
//					editing the log file.
// 2001-11-27	SAM, RTi		Overload attachMainMenu() to allow a
//					CheckboxMenu to be used.
// 2001-12-03	SAM, RTi		Begin conversion to Swing.  Do enough to
//					get it to hook into JMenuBar but need to
//					complete the port to Swing.
// 2002-09-11	SAM, RTi		Synchronize with DiagnosticsGUI and the
//					version used on Unix.  This
//					DiagnosticsJFrame should be used with
//					Swing from this time forward.
//					STILL NEED TO FULLY CONVERT TO A JFrame.
// 2002-10-11	SAM, RTi		Change ProcessManager to
//					ProcessManager1.
// 2002-10-16	SAM, RTi		Change back to ProcessManager since the
//					improved version seems to work under
//					1.1.8 and 1.4.0!  Use a thread to run
//					the editor.
// 2002-11-03	SAM, RTi		Don't set the background - let the look
//					and feel default handle.  In order to
//					do this, had to do the full conversion
//					to Swing.
// 2003-09-30	SAM, RTi		Use the icon and title from the
//					application if available.
// 2004-01-31	SAM, RTi		Fix bug where "Close" was applying the
//					settings.
// 2005-03-14	J. Thomas Sapienza, RTi	* Added code for displaying the
//					  new MessageLogJFrame.
//					* The old View Log File button is now
//					  labelled "Launch Log File Viewer."
//					* The help button was removed.
//					* Put tool tips on all the buttons.
//					* Added the Restart and New buttons.
// 2005-03-15	JTS, RTi		* Added a column for setting debug
//					  levels for messages sent to the
//					  console.
// 2005-03-24	JTS, RTi		When the user presses "Restart Log File"
//					they are now prompted to make sure that
//					is what they want to do.
// 2005-08-03	SAM, RTi		* Check for a blank string for the
//					  application name when setting the
//					  title.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//------------------------------------------------------------------------------
// EndHeader

namespace RTi.Util.Message
{


	using JFileChooserFactory = RTi.Util.GUI.JFileChooserFactory;
	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using ResponseJDialog = RTi.Util.GUI.ResponseJDialog;
	using SimpleFileFilter = RTi.Util.GUI.SimpleFileFilter;
	using SimpleJButton = RTi.Util.GUI.SimpleJButton;
	using SimpleJMenuItem = RTi.Util.GUI.SimpleJMenuItem;
	using IOUtil = RTi.Util.IO.IOUtil;
	using ProcessManager = RTi.Util.IO.ProcessManager;
	using PropList = RTi.Util.IO.PropList;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This class provides a simple GUI for setting various diagnostic information for
	/// a Java application.  Quite often, this GUI should only be enabled when
	/// diagnostics need to be provided.
	/// An example of how to implement the GUI is as follows (note that this uses
	/// resources at program initialization but it does allow for very simple use of
	/// the component):
	/// <para>
	/// 
	/// <pre>
	/// DiagnosticsJFrame diagnostics_gui = new DiagnosticsJFrame();
	/// diagnostics_gui.attachMainMenu( _view_menu );
	/// </pre>
	/// </para>
	/// <para>
	/// 
	/// The resulting GUI looks as follows:
	/// </para>
	/// <para>
	/// 
	/// <center>
	/// </para>
	/// <img src="DiagnosticsGUI.gif"><para>
	/// </center>
	/// </para>
	/// <para>
	/// 
	/// The GUI provides access to various methods of the Message class.  It is assumed
	/// that the log file has been set.
	/// <b>In the future, greater control of the log file will be added.</b>
	/// </para>
	/// </summary>
	/// <seealso cref= Message </seealso>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class DiagnosticsJFrame extends javax.swing.JFrame implements java.awt.event.ActionListener, java.awt.event.ItemListener, java.awt.event.WindowListener
	public class DiagnosticsJFrame : JFrame, ActionListener, ItemListener, WindowListener
	{

	private JTextField __consoleDebugLevel_JTextField;
	private JTextField __consoleWarningLevel_JTextField;
	private JTextField __consoleStatusLevel_JTextField;
	private JTextField __guiDebugLevel_JTextField;
	private JTextField __guiWarningLevel_JTextField;
	private JTextField __guiStatusLevel_JTextField;
	private JTextField __logDebugLevel_JTextField;
	private JTextField __logWarningLevel_JTextField;
	private JTextField __logStatusLevel_JTextField;

	private JLabel __logfileName_JLabel;

	private JCheckBox __debug_JCheckBox;
	private JCheckBox __message_JCheckBox;
	private JCheckBoxMenuItem __menu_JCheckBoxMenuItem = null;
	private JList __message_JList;
	private DefaultListModel __message_JListModel;

	private int __list_max;

	/// <summary>
	/// Parent component is used to position this component.
	/// </summary>
	private Component __parent = null;

	/// <summary>
	/// Constructor the GUI but do not make visible. </summary>
	/// <param name="parent"> UI parent component, used to center dialog </param>
	public DiagnosticsJFrame(Component parent) : base("Diagnostics")
	{
		__list_max = 100;
		__parent = parent;
		openGUI(0);
	}

	/// <summary>
	/// Constructor the GUI but do not make visible.
	/// </summary>
	public DiagnosticsJFrame() : base("Diagnostics")
	{
		__list_max = 100;
		openGUI(0);
	}

	/// <summary>
	/// Construct and specify the help key (the GUI will not be visible at construction). </summary>
	/// <param name="help_key"> Help key for the URLHelp class. </param>
	/// <seealso cref= RTi.Util.Help.URLHelp
	/// @deprecated. </seealso>
	public DiagnosticsJFrame(string help_key) : base("Diagnostics")
	{
		__list_max = 100;
		openGUI(0);
	}

	/// <summary>
	/// Handle ActionEvent events. </summary>
	/// <param name="e"> ActionEvent to handle. </param>
	public virtual void actionPerformed(ActionEvent e)
	{
		string rtn = "DiagnosticsGUI.actionPerformed";
		string command = e.getActionCommand();

		if (command.Equals("Apply"))
		{
			applySettings();
		}
		else if (command.Equals("Close"))
		{
			applySettings();
			setVisible(false);
			if (__menu_JCheckBoxMenuItem != null)
			{
				__menu_JCheckBoxMenuItem.setState(false);
			}
		}
		else if (command.Equals("DiagnosticsGUI"))
		{
			setVisible(true);
			__debug_JCheckBox.setSelected(Message.isDebugOn);
			if (__menu_JCheckBoxMenuItem != null)
			{
				__menu_JCheckBoxMenuItem.setState(true);
			}
		}
		else if (command.Equals("ViewLogFile"))
		{
			string logfile = Message.getLogFile();
			if (logfile.Equals(""))
			{
				return;
			}

			try
			{
				PropList p = new PropList("");
				p.set("NumberSummaryListLines=10");
				p.set("NumberLogLines=25");
				p.set("ShowSummaryList=true");
				MessageLogJFrame logJFrame = null;
				if ((__parent != null) && (__parent is JFrame))
				{
					logJFrame = new MessageLogJFrame((JFrame)__parent, p);
				}
				else
				{
					logJFrame = new MessageLogJFrame(this, p);
				}
				logJFrame.processLogFile(Message.getLogFile());
				logJFrame.setVisible(true);
			}
			catch (Exception ex)
			{
				Message.printWarning(1, rtn, "Unable to view log file \"" + Message.getLogFile() + "\"");
				Message.printWarning(2, rtn, ex);
			}
		}
		else if (command.Equals("Flush"))
		{
			Message.flushOutputFiles(0);
			Message.printStatus(1, rtn, "Log file updated.");
		}
		else if (command.Equals("LaunchLogFileViewer"))
		{
			string[] command_array = new string[2];
			// Try using external viewer.  Otherwise, display a warning.
			command_array[0] = getExternalFileViewerProgram();
			try
			{
				Message.flushOutputFiles(0);
				command_array[1] = Message.getLogFile();
				ProcessManager p = new ProcessManager(command_array);
				// This will run as a thread until the process is shut down...
				Thread t = new Thread(p);
				t.Start();
			}
			catch (Exception)
			{
				Message.printWarning(1, rtn, "Unable to view log file \"" + Message.getLogFile() + "\" using " + command_array[0]);
			}
			Message.flushOutputFiles(0);
		}
		else if (command.Equals("restart"))
		{
			int x = (new ResponseJDialog(this, "Start a new log file?", "Are you sure you want to restart the log file?  All " + "contents currently in the log file will be lost.", ResponseJDialog.YES | ResponseJDialog.NO)).response();
			if (x == ResponseJDialog.NO)
			{
				return;
			}
			try
			{
				Message.restartLogFile();
			}
			catch (Exception ex)
			{
				Message.printWarning(1, rtn, "Unable to re-open log file.");
				Message.printWarning(2, rtn, ex);
			}
		}
		else if (command.Equals("new"))
		{
			Message.closeLogFile();
			JGUIUtil.setWaitCursor(this, true);
			string lastDirectorySelected = JGUIUtil.getLastFileDialogDirectory();

			JFileChooser fc = JFileChooserFactory.createJFileChooser(lastDirectorySelected);

			fc.setDialogTitle("Select Log File");
			SimpleFileFilter htmlFF = new SimpleFileFilter("log", "Log Files");
			fc.addChoosableFileFilter(htmlFF);
			fc.setAcceptAllFileFilterUsed(true);
			fc.setFileFilter(htmlFF);
			fc.setDialogType(JFileChooser.OPEN_DIALOG);

			JGUIUtil.setWaitCursor(this, false);
			int retVal = fc.showOpenDialog(this);
			if (retVal != JFileChooser.APPROVE_OPTION)
			{
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
				Message.openNewLogFile(path);
			}
			catch (Exception ex)
			{
				Message.printWarning(1, rtn, "Unable to open new log file: " + path);
				Message.printWarning(2, rtn, ex);
			}
		}
	}

	/// <summary>
	/// Apply the settings in the GUI.  This is called when "Apply" or "Close" are pressed.
	/// </summary>
	private void applySettings()
	{
		string rtn = "DiagnosticsGUI.applySettings";
		Message.setDebugLevel(Message.TERM_OUTPUT, StringUtil.atoi(__consoleDebugLevel_JTextField.getText()));
		Message.setWarningLevel(Message.TERM_OUTPUT, StringUtil.atoi(__consoleWarningLevel_JTextField.getText()));
		Message.setStatusLevel(Message.TERM_OUTPUT, StringUtil.atoi(__consoleStatusLevel_JTextField.getText()));
		Message.setDebugLevel(Message.STATUS_HISTORY_OUTPUT, StringUtil.atoi(__guiDebugLevel_JTextField.getText()));
		Message.setWarningLevel(Message.STATUS_HISTORY_OUTPUT, StringUtil.atoi(__guiWarningLevel_JTextField.getText()));
		Message.setStatusLevel(Message.STATUS_HISTORY_OUTPUT, StringUtil.atoi(__guiStatusLevel_JTextField.getText()));
		Message.setDebugLevel(Message.LOG_OUTPUT, StringUtil.atoi(__logDebugLevel_JTextField.getText()));
		Message.setWarningLevel(Message.LOG_OUTPUT, StringUtil.atoi(__logWarningLevel_JTextField.getText()));
		Message.setStatusLevel(Message.LOG_OUTPUT, StringUtil.atoi(__logStatusLevel_JTextField.getText()));

		Message.isDebugOn = __debug_JCheckBox.isSelected();

		if (Message.isDebugOn)
		{
			Message.printStatus(1, rtn, "Setting debug to " + Message.getDebugLevel(Message.STATUS_HISTORY_OUTPUT) + " (STATUS_HISTORY_OUTPUT) " + Message.getDebugLevel(Message.LOG_OUTPUT) + " (LOG_OUTPUT)");
		}
		else
		{
			Message.printStatus(1, rtn, "Debug has been turned off");
		}
	}

	/// <summary>
	/// Attach the DiagnosticsGUI menus to the given menu.  The menu will be labelled "Diagnostics Preferences...". </summary>
	/// <param name="menu"> Menu to attach to. </param>
	public virtual void attachMainMenu(JMenu menu)
	{
		menu.add(new SimpleJMenuItem("Diagnostics...", "DiagnosticsGUI", this));
		menu.add(new SimpleJMenuItem("Diagnostics - View Log File ...", "ViewLogFile", this));
	}

	/// <summary>
	/// Attach the DiagnosticsGUI menus to the given menu.  The menu will be labelled "Diagnostics Preferences...". </summary>
	/// <param name="menu"> Menu to attach to. </param>
	/// <param name="use_checkbox"> If true, use a CheckboxMenuItem.  If false, use a normal MenuItem. </param>
	public virtual void attachMainMenu(JMenu menu, bool use_checkbox)
	{
		if (use_checkbox)
		{
			__menu_JCheckBoxMenuItem = new JCheckBoxMenuItem("Diagnostics", isVisible());
			__menu_JCheckBoxMenuItem.addItemListener(this);

			menu.add(__menu_JCheckBoxMenuItem);
			menu.add(new SimpleJMenuItem("Diagnostics - View Log File ...", "ViewLogFile", this));
		}
		else
		{
			attachMainMenu(menu);
		}
	}

	/// <summary>
	/// Return the name of the external program to use for viewing log files.  The program
	/// should be in the path.  Currently this is not configurable. </summary>
	/// <returns> the name of the external program to use to view the log file. </returns>
	private string getExternalFileViewerProgram()
	{
		if (IOUtil.isUNIXMachine())
		{
			return "nedit";
		}
		else
		{
			return "notepad";
		}
	}

	/// <summary>
	/// Handle ItemEvent events. </summary>
	/// <param name="e"> ItemEvent to handle. </param>
	public virtual void itemStateChanged(ItemEvent e)
	{
		if (e.getSource() == __message_JCheckBox)
		{
			if (__message_JCheckBox.isSelected())
			{
				__message_JList.setVisible(true);
			}
			else
			{
				__message_JList.setVisible(false);
			}
			pack();
		}
		else if (e.getSource() == __menu_JCheckBoxMenuItem)
		{
			// Use the CheckboxMenuItem
			setVisible(__menu_JCheckBoxMenuItem.getState());
			__debug_JCheckBox.setSelected(Message.isDebugOn);
		}
	}

	/// <summary>
	/// Construct with the flag. </summary>
	/// <param name="mode"> Indicates how the GUI should be constructed.  Currently, the mode
	/// can be 1 to indicated that the GUI should be visible on creation, and zero to
	/// indicate that it should be hidden on creation. </param>
	public virtual void openGUI(int mode)
	{
		string rtn = "openGUI";

		try
		{
			JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		if ((string.ReferenceEquals(JGUIUtil.getAppNameForWindows(), null)) || (JGUIUtil.getAppNameForWindows().Length == 0))
		{
			setTitle("Diagnostics");
		}
		else
		{
			setTitle(JGUIUtil.getAppNameForWindows() + " - Diagnostics");
		}

		// Set the visibility up front so that if not visible it does not
		// flash when drawing.  Reset at the end to the final value...

		setVisible(false);

		__consoleDebugLevel_JTextField = new JTextField(4);
		__consoleWarningLevel_JTextField = new JTextField(4);
		__consoleStatusLevel_JTextField = new JTextField(4);
		__guiDebugLevel_JTextField = new JTextField(4);
		__guiWarningLevel_JTextField = new JTextField(4);
		__guiStatusLevel_JTextField = new JTextField(4);
		__logDebugLevel_JTextField = new JTextField(4);
		__logWarningLevel_JTextField = new JTextField(4);
		__logStatusLevel_JTextField = new JTextField(4);
		__debug_JCheckBox = new JCheckBox("Allow debug", Message.isDebugOn);
		__message_JCheckBox = new JCheckBox("Show messages", true);
		__message_JListModel = new DefaultListModel();
		__message_JList = new JList(__message_JListModel);
		JScrollPane message_JScrollPane = new JScrollPane(__message_JList);

		JPanel p1 = new JPanel();
		GridLayout gl1 = new GridLayout(4, 4, 4, 6);
		p1.setLayout(gl1);
		gl1 = null;

		p1.add(new JLabel("Message type"));
		p1.add(new JLabel("Console output"));
		p1.add(new JLabel("Status bar"));
		p1.add(new JLabel("Log file"));
		p1.add(new JLabel(""));
		p1.add(new JLabel("Status"));
		p1.add(__consoleStatusLevel_JTextField);
		p1.add(__guiStatusLevel_JTextField);
		p1.add(__logStatusLevel_JTextField);
		p1.add(new JLabel(""));
		p1.add(new JLabel("Warning"));
		p1.add(__consoleWarningLevel_JTextField);
		p1.add(__guiWarningLevel_JTextField);
		p1.add(__logWarningLevel_JTextField);
		p1.add(new JLabel(""));
		p1.add(new JLabel("Debug"));
		p1.add(__consoleDebugLevel_JTextField);
		p1.add(__guiDebugLevel_JTextField);
		p1.add(__logDebugLevel_JTextField);
		p1.add(__debug_JCheckBox);

		JPanel p2 = new JPanel();
		SimpleJButton applyButton = new SimpleJButton("Apply",this);
		applyButton.setToolTipText("Apply changes to the logging levels.");
		p2.add(applyButton);
		SimpleJButton launch_button = new SimpleJButton("Launch Log File Viewer", "LaunchLogFileViewer", this);
		launch_button.setToolTipText("Use " + getExternalFileViewerProgram() + " to view the log file.");
		// Disable if the log file is not known to the Message class or does not exist...
		string logfile = Message.getLogFile();
		if (string.ReferenceEquals(logfile, null))
		{
			launch_button.setEnabled(false);
		}
		if (!IOUtil.fileReadable(logfile))
		{
			launch_button.setEnabled(false);
		}
		logfile = null;

		SimpleJButton viewButton = new SimpleJButton("View Log File", "ViewLogFile", this);
		viewButton.setToolTipText("Open the log file viewer/navigator.");
		p2.add(viewButton);
		p2.add(launch_button);
		launch_button = null;
		SimpleJButton closeButton = new SimpleJButton("Close",this);
		closeButton.setToolTipText("Close the window, losing any changes made without pressing 'Apply'.");
		p2.add(closeButton);

		int y = 0;
		JPanel main_JPanel = new JPanel();
		main_JPanel.setLayout(new GridBagLayout());
		getContentPane().add("Center", main_JPanel);
		JGUIUtil.addComponent(main_JPanel, new JLabel("More detailed messages are printed as the message level increases."), 0, y, 1, 1, 1, 0, 20, 1, 0, 1, GridBagConstraints.NONE, GridBagConstraints.CENTER);
		JGUIUtil.addComponent(main_JPanel, new JLabel("(0 results in none of the messages being printed)"), 0, ++y, 1, 1, 1, 0, 0, 1, 0, 1, GridBagConstraints.NONE, GridBagConstraints.CENTER);
		JGUIUtil.addComponent(main_JPanel, __logfileName_JLabel = new JLabel(""), 0, ++y, 1, 1, 1, 0, 0, 1, 0, 1, GridBagConstraints.NONE, GridBagConstraints.CENTER);
		JGUIUtil.addComponent(main_JPanel, p1, 0, ++y, 1, 1, 0, 0, 4, 1, 14, 1, GridBagConstraints.NONE, GridBagConstraints.CENTER);
		p1 = null;

		JGUIUtil.addComponent(main_JPanel, new JLabel("Message history:"), 0, ++y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(main_JPanel, __message_JCheckBox, 0, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		__message_JCheckBox.addItemListener(this);
		JGUIUtil.addComponent(main_JPanel, message_JScrollPane, 0, ++y, 1, 1, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.CENTER);
		SimpleJButton flushButton = new SimpleJButton("Flush Log File","Flush",this);
		flushButton.setToolTipText("Force the log file buffer to flush, in " + "order to guarantee that all messages have been logged to the file.");

		SimpleJButton restartButton = new SimpleJButton("Restart Log File", "restart", this);
		restartButton.setToolTipText("Re-open the log file, " + "overwriting all text that currently is in the log file.");
		SimpleJButton newButton = new SimpleJButton("New Log File", "new", this);
		newButton.setToolTipText("Open a new log file for writing.");

		JPanel buttons = new JPanel();
		buttons.add(flushButton);
		buttons.add(restartButton);
		buttons.add(newButton);

		JGUIUtil.addComponent(main_JPanel, buttons, 0, ++y, 1, 1, 1, 0, GridBagConstraints.NONE, GridBagConstraints.CENTER);

		JGUIUtil.addComponent(main_JPanel, p2, 0, ++y, 1, 1, 0, 0, 10, 1, 0, 1, GridBagConstraints.NONE, GridBagConstraints.CENTER);
		p2 = null;

		Message.setOutputFunction(Message.STATUS_HISTORY_OUTPUT, this, "printStatusMessages");

		// Refresh the contents of the graphical components...
		refreshContents();

		// Listen for window events (close, etc.)...

		addWindowListener(this);

		pack();
		JGUIUtil.center(this,__parent);

		if ((mode & JGUIUtil.GUI_VISIBLE) != 0)
		{
			setVisible(true);
		}
		else
		{
			setVisible(false);
		}

		}
		catch (Exception e)
		{
			Console.WriteLine(e.ToString());
			Console.Write(e.StackTrace);
			Message.printWarning(2, rtn, e);
		}
		rtn = null;
	}

	/// <summary>
	/// Routine that is registered with the Message.printStatus routine and which
	/// results in messages being printed to the history. </summary>
	/// <param name="level"> Message level. </param>
	/// <param name="rtn"> Routine that is printing the message. </param>
	/// <param name="message"> Message to be printed. </param>
	public virtual void printStatusMessages(int level, string rtn, string message)
	{
		while (__message_JListModel.size() > __list_max)
		{
			__message_JListModel.removeElementAt(0);
		}
		__message_JListModel.addElement(message);
		int index = __message_JListModel.size() - 1;
		__message_JList.setSelectedIndex(index);
		__message_JList.ensureIndexIsVisible(index);
	}

	/// <summary>
	/// Refresh the contents of the display.  This is called by setVisible() to ensure that
	/// the display is consistent with memory.
	/// </summary>
	private void refreshContents()
	{
		string logfile = Message.getLogFile();
		if ((string.ReferenceEquals(logfile, null)) || logfile.Equals(""))
		{
			__logfileName_JLabel.setText("No log file has been opened.  Verify the software configuration.");
		}
		else
		{
			__logfileName_JLabel.setText("Most recent log file = \"" + logfile + "\"");
		}

		__consoleDebugLevel_JTextField.setText(Message.getDebugLevel(Message.TERM_OUTPUT).ToString());
		__consoleWarningLevel_JTextField.setText(Message.getWarningLevel(Message.TERM_OUTPUT).ToString());
		__consoleStatusLevel_JTextField.setText(Message.getStatusLevel(Message.TERM_OUTPUT).ToString());
		__guiDebugLevel_JTextField.setText(Message.getDebugLevel(Message.STATUS_HISTORY_OUTPUT).ToString());
		__guiWarningLevel_JTextField.setText(Message.getWarningLevel(Message.STATUS_HISTORY_OUTPUT).ToString());
		__guiStatusLevel_JTextField.setText(Message.getStatusLevel(Message.STATUS_HISTORY_OUTPUT).ToString());
		__logDebugLevel_JTextField.setText(Message.getDebugLevel(Message.LOG_OUTPUT).ToString());
		__logWarningLevel_JTextField.setText(Message.getWarningLevel(Message.LOG_OUTPUT).ToString());
		__logStatusLevel_JTextField.setText(Message.getStatusLevel(Message.LOG_OUTPUT).ToString());
	}

	/// <summary>
	/// Set the window visible, refreshing the contents from settings that may have been
	/// applied programatically elsewhere. </summary>
	/// <param name="isVisible"> If true, set the window to visible and refresh the contents.
	/// If false, set the window to invisible. </param>
	public virtual void setVisible(bool isVisible)
	{
		if (isVisible)
		{
			// Refresh the contents
			refreshContents();
		}
		// Always center on the main interface since people will think it is popping up new each time
		if (isVisible)
		{
			JGUIUtil.center(this,__parent);
		}
		base.setVisible(isVisible);
	}

	public virtual void windowActivated(WindowEvent e)
	{
	}

	/// <summary>
	/// If the window is closing, set to visible but do not dispose.
	/// </summary>
	public virtual void windowClosing(WindowEvent e)
	{
		setVisible(false);
		if (__menu_JCheckBoxMenuItem != null)
		{
			__menu_JCheckBoxMenuItem.setState(false);
		}
	}

	public virtual void windowClosed(WindowEvent e)
	{
	}

	public virtual void windowDeactivated(WindowEvent e)
	{
	}

	public virtual void windowDeiconified(WindowEvent e)
	{
	}

	public virtual void windowIconified(WindowEvent e)
	{
	}

	public virtual void windowOpened(WindowEvent e)
	{
	}

	} // End DiagnosticsJFrame class

}