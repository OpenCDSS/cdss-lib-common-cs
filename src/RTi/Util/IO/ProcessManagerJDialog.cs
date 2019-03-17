﻿using System;
using System.Threading;

// ProcessManagerJDialog - class that wraps a JDialog around ProcessManager

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
// ProcessManagerJDialog - class that wraps a JDialog around ProcessManager
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 02 May 1998	Catherine E.		Created initial version of class.
//		Nutting-Lane, RTi
// 15 Oct 1998	Steven A. Malers, RTi	Changed so that the X button closes the
//					GUI and make the GUI come up wider.
// 30 Nov 1998	CEN, RTi		Added function to retrieve
//					ProcessManager _proc itself.
// 14 Apr 1999	CEN, RTi		Added code to compensate for bugs in
//					Java 1.2.  Documented this below in the
//					code for future removal.
// 07 Jan 2001	SAM, RTi		Change GUI to GUIUtil.  Change import *
//					to specific imports.
// 07 Aug 2001	SAM, RTi		When closing down and when the process
//					is finished, unset the output function
//					for Message so the GUI only shows the
//					output from the command.  Add
//					finalize().  Change to set color of
//					GUI to red if an error.  Change so sleep
//					is not a static variable.
// 16 Aug 2001	SAM, RTi		Update so when getting output from the
//					ProcessManager, null strings are
//					ignored.  This is necessary to support
//					NT temporary files.  Change checkQuery()
//					to checkProcess().  The former was a
//					hold-over from the initial threaded
//					query code for HydroBase.
// 23 Aug 2001	SAM, RTi		Add the cleanup() method to clean up
//					the process and event timer thread.
//					Without this the CPU can get hammered
//					by old process threads.  Change so
//					the command is shown in a TextField so
//					it does not mess up the display if very
//					long.  Change _proc to _process_manager
//					to make code clearer.
// 2002-08-05	SAM, RTi		Add close() method so that the GUI can
//					be automatically closed when a
//					successful run occurs.
// 2002-08-07	SAM, RTi		Fix so that timer thread is destroyed
//					before the process manager to avoid
//					the thread trying to use a null
//					process manager.
// 2002-10-11	SAM, RTi		Change name of class from
//					ProcessManagerGUI to
//					ProcessManagerJDialog.  Use a dialog
//					so it can be made modal
// 2002-10-20	SAM, RTi		Update the constructor to take a
//					ProcessManager instead of a raw command
//					because the ProcessManager has more
//					settings now and it would be difficult
//					to pass all variations through.
// 2002-10-23	SAM, RTi		* Put the output in a JScrollPane -
//					  scrolling was not working.
//					* Increase the default history size to
//					  500 and allow it to be changed with
//					  a property.
// 2004-08-25	J. Thomas Sapienza, RTi	Revised the GUI-building code so that
//					if a "HelpKey" parameter is not set in
//					the proplist, the Help button will not
//					appear on the GUI.
// 2005-04-26	JTS, RTi		Convert GUIUtil to JGUIUtil.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//------------------------------------------------------------------------------
//EndHeader

namespace RTi.Util.IO
{

	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using SimpleJButton = RTi.Util.GUI.SimpleJButton;
	using URLHelp = RTi.Util.Help.URLHelp;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using StopWatch = RTi.Util.Time.StopWatch;

	/// <summary>
	/// This class executes a command using the ProcessManager class.  The
	/// ProcessManager instance is managed as a thread.  Therefore, using the
	/// ProcessManagerJDialog will NOT pause the calling application until the process
	/// is complete.  This design may change in the future.
	/// The results are displayed in a list that gets updated as new messages are generated
	/// by the called program.
	/// The following is an example of how to utilize this class.
	/// <para>
	/// 
	/// <pre>
	/// ProcessManager pm = new ProcessManager ( String [] command );
	/// new ProcessManagerJDialog ( pm );
	/// </pre>
	/// </para>
	/// <para>
	/// 
	/// </para>
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class ProcessManagerJDialog extends javax.swing.JDialog implements java.awt.event.ActionListener, ProcessListener
	public class ProcessManagerJDialog : JDialog, ActionListener, ProcessListener
	{

	private ProcessManager __process_manager; // manager to run process (see doc)
	private JList __output_JList; // List to display process output
	private int __historyMaxSize = 500; // # lines of process output to display
	private SimpleJButton __cancel_JButton;
	private SimpleJButton __close_JButton;
	private SimpleJButton __help_JButton;

	private JPanel __top_JPanel = null;
	private JPanel __south_JPanel = null;
	private JLabel __command_JLabel = null;
	private JTextField __command_JTextField = null;
	private JLabel __status0_JLabel = null;
						// Label before __status_JLabel - need
						// so color can be changed.
	private JLabel __status_JLabel; // process status: "Canceled", "Done" or "Active"

	private Thread __thread; // runs process

	/// <summary>
	/// Help key used with on-line help.
	/// </summary>
	private string __helpKey = null;

	/// <summary>
	/// Indicate whether all process output should be logged (default is only filtered output).
	/// </summary>
	private bool __logAllOutput = false;

	/// <summary>
	/// StopWatch to time execution.
	/// </summary>
	private StopWatch __stopWatch = new StopWatch();

	private DefaultListModel __output_ListModel; // This holds the Vector of strings from the process output.
	private int __exit_status = -1; // The exit status from the process.
	/// <summary>
	/// Filter to control how much output is displayed or ignored.
	/// </summary>
	private ProcessManagerOutputFilter __outputFilter = null;

	/// <summary>
	/// Create a ProcessManagerJDialog that executes the specified command.
	/// The process is created immediately upon instantiation of this class.
	/// The default maximum number of lines to be displayed in the output area is 500. </summary>
	/// <param name="parent"> Parent JFrame. </param>
	/// <param name="title"> Title for the dialog. </param>
	/// <param name="process_manager"> ProcessManager instance that contains information about
	/// the command to run. </param>
	public ProcessManagerJDialog(JFrame parent, string title, ProcessManager process_manager) : base(parent, title, true)
	{
		initialize(process_manager);
	}

	/// <summary>
	/// Create a ProcessManagerJDialog that executes the specified command.
	/// The process is created immediately upon instantiation of this class.
	/// The default maximum number of lines to be displayed in the output area is 100. </summary>
	/// <param name="parent"> Parent JFrame. </param>
	/// <param name="title"> Title for the dialog. </param>
	/// <param name="process_manager"> ProcessManager instance that contains information about
	/// the command to run. </param>
	/// <param name="props"> The property list to control the output appearance.  Currently,
	/// "HelpKey" can be set to a help system key.  "BufferSize" can be set to the
	/// number of lines to display in the output.  The default is 500.  Using a value
	/// of zero will not limit the size.  If "HelpKey" is not set in the proplist, the
	/// help button will not appear on the GUI. </param>
	public ProcessManagerJDialog(JFrame parent, string title, ProcessManager process_manager, PropList props) : this(parent, title, process_manager, null, props)
	{
	}

	/// <summary>
	/// Create a ProcessManagerJDialog that executes the specified command and allows filtering of output.
	/// The process is created immediately upon instantiation of this class.
	/// The default maximum number of lines to be displayed in the output area is 100. </summary>
	/// <param name="parent"> Parent JFrame. </param>
	/// <param name="title"> Title for the dialog. </param>
	/// <param name="process_manager"> ProcessManager instance that contains information about
	/// the command to run. </param>
	/// <param name="props"> The property list to control the output appearance.  Currently,
	/// "HelpKey" can be set to a help system key. If "HelpKey" is not set in the proplist, the
	/// help button will not appear on the GUI.  "BufferSize" can be set to the
	/// number of lines to display in the output.  The default is 500.  Using a value
	/// of zero will not limit the size.  Setting "LogOutput=False" will log the process output
	/// (the default is true). </param>
	public ProcessManagerJDialog(JFrame parent, string title, ProcessManager process_manager, ProcessManagerOutputFilter outputFilter, PropList props) : base(parent, title, true)
	{
		__outputFilter = outputFilter;
		// Transfer properties to local data.
		if (props == null)
		{
			props = new PropList("ProcessManagerJDialog");
		}
		string propValue = props.getValue("HelpKey");
		if (!string.ReferenceEquals(propValue, null))
		{
			setHelpKey(propValue);
		}
		propValue = props.getValue("BufferSize");
		if ((!string.ReferenceEquals(propValue, null)) && StringUtil.isInteger(propValue))
		{
			setHistoryMaxSize(StringUtil.atoi(propValue));
		}
		propValue = props.getValue("LogAllOutput");
		if ((!string.ReferenceEquals(propValue, null)) && propValue.Equals("True", StringComparison.OrdinalIgnoreCase))
		{
			setLogAllOutput(true);
		}
		initialize(process_manager);
	}

	/// <summary>
	/// Handle action events. </summary>
	/// <param name="e"> ActionEvent to handle. </param>
	public virtual void actionPerformed(ActionEvent e)
	{
		if (e.getSource() == __help_JButton)
		{
			if (!string.ReferenceEquals(getHelpKey(), null))
			{
				URLHelp.showHelpForKey(getHelpKey());
			}
		}
		else if (e.getSource() == __close_JButton)
		{
			close();
		}
		else if (e.getSource() == __cancel_JButton)
		{
			__process_manager.cancel();
			__cancel_JButton.setEnabled(false);
			__status_JLabel.setText("Cancelled");
			JGUIUtil.setWaitCursor(this, false);
			// This does not call close because the user may want to look at the output.
		}
	}

	/// <summary>
	/// Clean up the thread by destroying the process and the thread that is monitoring
	/// the thread's process.  This method should only be called by close().
	/// </summary>
	private void cleanup()
	{ // Destroy the process manager...
		__process_manager = null;
		// Also clean up the thread that runs the _process_manager...
		__thread = null;
	}

	/// <summary>
	/// Close the process manager GUI, stopping the process.  This method is normally
	/// called only after a process is finished (e.g., to automatically close the
	/// GUI if an error did not occur.
	/// </summary>
	public virtual void close()
	{
		setVisible(false);
		if (!__process_manager.isProcessFinished())
		{
			// Cancel to make sure the process shuts down...
			__process_manager.cancel();
		}
		// Close down the process, if necessary...
		cleanup();
		dispose();
	}

	/// <summary>
	/// Clean up for garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~ProcessManagerJDialog()
	{
		__process_manager = null;
		__output_JList = null;
		__output_ListModel = null;
		__cancel_JButton = null;
		__close_JButton = null;
		__help_JButton = null;
		__top_JPanel = null;
		__south_JPanel = null;
		__command_JLabel = null;
		__command_JTextField = null;
		__status0_JLabel = null;
		__status_JLabel = null;
		__thread = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the exit status of the process.  This value is initialized to -1.
	/// Because the dialog is modal, the dialog will not exit its control until
	/// "closed" is pressed.  However, during the close process the original
	/// ProcessManager may be destroyed.  Therefore, the exit status that is returned
	/// here is the value returned by the process manager when the
	/// ProcessListener.processStatus() method is called in this dialog. </summary>
	/// <returns> the exit status of the process that is managed by this dialog. </returns>
	public virtual int getExitStatus()
	{
		return __exit_status;
	}

	/// <summary>
	/// Return the help key associated with this component.
	/// </summary>
	private string getHelpKey()
	{
		return __helpKey;
	}

	/// <summary>
	/// Return the maximum size of the output list.
	/// </summary>
	private int getHistoryMaxSize()
	{
		return __historyMaxSize;
	}

	/// <summary>
	/// Return whether output should be logged.
	/// </summary>
	private bool getLogAllOutput()
	{
		return __logAllOutput;
	}

	/// <summary>
	/// Return the ProcessManager associated with this dialog. </summary>
	/// <returns> the ProcessManager associated with this dialog. </returns>
	public virtual ProcessManager getProcessManager()
	{
		return __process_manager;
	}

	/// <summary>
	/// Initialize the GUI and run the command. </summary>
	/// <param name="process_manager"> ProcessManager instance that contains information about
	/// the command to run. </param>
	private void initialize(ProcessManager process_manager)
	{
		string rtn = "ProcessManagerJDialog.initialize";

		// initialize members
		__process_manager = process_manager;
		__status_JLabel = new JLabel("Active (there may be a delay in displaying program output)");
		__output_ListModel = new DefaultListModel(); // Basically a Vector
		__output_JList = new JList(__output_ListModel);
		__output_JList.setVisibleRowCount(10);
		// The following will return the DefaultListModel which is OK to use...
		__help_JButton = new SimpleJButton("Help", "Help", this);
		__close_JButton = new SimpleJButton("Close", "Close", this);
		__cancel_JButton = new SimpleJButton("Cancel", "Cancel", this);

		GridBagLayout gb = new GridBagLayout();

		__top_JPanel = new JPanel(gb);
		__command_JLabel = new JLabel("Command:  ");
		JGUIUtil.addComponent(__top_JPanel, __command_JLabel, 0, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		__command_JTextField = new JTextField(__process_manager.getCommand(), 50);
		__command_JTextField.setEditable(false);
		JGUIUtil.addComponent(__top_JPanel, __command_JTextField, 1, 0, 1, 1, 1, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		__status0_JLabel = new JLabel("Status:  ");
		JGUIUtil.addComponent(__top_JPanel, __status0_JLabel, 0, 1, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(__top_JPanel, __status_JLabel, 1, 1, 1, 1, 1, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);

		JGUIUtil.addComponent(__top_JPanel, new JScrollPane(__output_JList), 0, 2, 2, 10, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.NORTH);
		getContentPane().add("Center", __top_JPanel);

		__south_JPanel = new JPanel(new FlowLayout(FlowLayout.CENTER));
		__south_JPanel.add(__cancel_JButton);
		__south_JPanel.add(__close_JButton);

		if (!string.ReferenceEquals(getHelpKey(), null))
		{
			__south_JPanel.add(__help_JButton);
		}
		getContentPane().add("South", __south_JPanel);

		pack();
		setSize(500, 300);
		JGUIUtil.center(this);

		// start process
		JGUIUtil.setWaitCursor(this, true);
		Message.printStatus(1, rtn, "Creating ProcessManager " + __process_manager.getCommand());
		__process_manager.addProcessListener(this); // initializes a ProcessManager
		Message.printStatus(1, rtn, "Creating thread from process.");
		__thread = new Thread(__process_manager); // create a new thread from process
		Message.printStatus(1, rtn, "Running thread.");
		__stopWatch.start();
		__thread.Start(); // executes the run() member of the
						// ProcessManager class (overloads 
						// the run member of the Thread class)

		// If the JDialog is modal, the thread must be started before the
		// following is called, otherwise the dialog interface thread goes into
		// its loop and does not get to the code that starts the thread!

		setVisible(true);
	}

	/// <summary>
	/// Handle error from the process. </summary>
	/// <param name="error"> Line of standard error from the process. </param>
	public virtual void processError(string error)
	{
		processOutput(error);
	}

	/// <summary>
	/// Handle output from the process. </summary>
	/// <param name="output"> Line of output from the process. </param>
	public virtual void processOutput(string output)
	{
		// Use the output filter if specified to limit the amount of output to display.
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String filteredoutput;
		string filteredoutput;
		if (__outputFilter != null)
		{
			filteredoutput = __outputFilter.filterOutput(output);
		}
		else
		{
			filteredoutput = output;
		}
		if (string.ReferenceEquals(filteredoutput, null))
		{
			return;
		}
		if (getLogAllOutput())
		{
			// Want to log all output
			Message.printStatus(2, "processOutput", "Output from process is: \"" + output + "\"");
		}
		else
		{
			// Only log the filtered output.
			Message.printStatus(2, "processOutput", "Filtered output from process is: \"" + filteredoutput + "\"");
		}
		ThreadStart r = () =>
		{
		int historyMaxSize = getHistoryMaxSize();
		if (historyMaxSize > 0)
		{
			// To avoid "blinking", remove the listeners and then add before adding items.
			// This should keep the list focus on the end of the list.
			ListDataListener[] listeners = __output_ListModel.getListDataListeners();
			for (int i = listeners.Length - 1; i >= 0 ; i--)
			{
				__output_ListModel.removeListDataListener(listeners[i]);
			}
			// Delete the first item(s) to get to the requested size minus one.
			while (__output_ListModel.size() >= historyMaxSize)
			{
				__output_ListModel.removeElementAt(0);
			}
			// Now add the listeners again.
			for (int i = 0; i < listeners.Length; i++)
			{
				__output_ListModel.addListDataListener(listeners[i]);
			}
		}
		// Now add the new output line.
		__output_ListModel.addElement(filteredoutput);
		// Force the last row to be visible.  If the total output size larger
		// than the buffer, the above check condition should allow the following
		// to add up to the buffer.
		__output_JList.ensureIndexIsVisible(__output_ListModel.size() - 1);
		};
		SwingUtilities.invokeLater(r);
	}

	/// <summary>
	/// Handle a change in status for the process.
	/// </summary>
	public virtual void processStatus(int status, string message)
	{
		__exit_status = status;
		__stopWatch.stop();
		if (status == 0)
		{
			// Successful completion of process...
			__status_JLabel.setText("Complete - success (run time=" + __stopWatch.getSeconds() + " seconds)");
			JGUIUtil.setWaitCursor(this, false);
		}
		else
		{
			// Unsuccessful completion of process...
			__cancel_JButton.setEnabled(false);
			if (message.Equals(""))
			{
				__status_JLabel.setText("Complete - error: " + status + " (run time=" + __stopWatch.getSeconds() + " seconds)");
			}
			else
			{
				__status_JLabel.setText("Complete - error:" + status + " (" + message + ") (run time=" + __stopWatch.getSeconds() + " seconds)");
			}
			// Color the interface to make it obvious that there was an
			// error...
			setBackground(Color.red);
			setForeground(Color.red);
			__top_JPanel.setBackground(Color.red);
			__south_JPanel.setBackground(Color.red);
			__command_JLabel.setBackground(Color.red);
			__command_JTextField.setBackground(Color.red);
			// Hard to read when it is red...
			//_statusHistory.setBackground(Color.red);
			__status0_JLabel.setBackground(Color.red);
			__status_JLabel.setBackground(Color.red);
			JGUIUtil.setWaitCursor(this, false);
			repaint();
		}
		// Either way, disable the Cancel button because not needed...
		__cancel_JButton.setEnabled(false);
	}

	/// <summary>
	/// Handle WindowEvent events. </summary>
	/// <param name="e"> WindowEvent to handle. </param>
	protected internal virtual void processWindowEvent(WindowEvent e)
	{
		if (e.getID() == WindowEvent.WINDOW_CLOSING)
		{
			base.processWindowEvent(e);
			close();
		}
		else
		{
			base.processWindowEvent(e);
		}
	}

	/// <summary>
	/// Set the help key used with on-line help for this component.
	/// </summary>
	private void setHelpKey(string helpKey)
	{
		__helpKey = helpKey;
	}

	/// <summary>
	/// Set the maximum number of lines to display, or 0 to display all.
	/// </summary>
	private void setHistoryMaxSize(int bufferSize)
	{
		__historyMaxSize = bufferSize;
	}

	/// <summary>
	/// Set whether the process output should be logged.
	/// </summary>
	private void setLogAllOutput(bool logAllOutput)
	{
		__logAllOutput = logAllOutput;
	}

	} // End ProcessManagerJDialog

}