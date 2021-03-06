﻿// MessageLogJDialog - a GUI for displaying message log text

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
// MessageLogJDialog - a GUI for displaying message log text 
// 	in order to quickly summarize and move through the log file.
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 2005-03-10	J. Thomas Sapienza, RTi	Initial version.
// 2005-03-17	JTS, RTi		Incorporated a new way of handling
//					tags.
// 2005-03-22	JTS, RTi		Modified to use a worksheet as an 
//					alternate to the JTextArea.
// 2005-03-23	JTS, RTi		* Added the number of lines to the
//					  information printed by the summary 
//					  list panel.
// 					* Summary information and the log file
//					  can now be printed.
//					* The summary is turned off if tags
//					  are turned off in Message.
//					* Worksheet now uses Courier font.
// 2005-03-25	JTS, RTi		* The popup menu to go to a command is
//					  now only enabled if a command with a
//					  tag is selected.
//					* Warnings at level 1 are now included
//					  in the summary list.
// 2005-04-05	JTS, RTi		Converted from a JFrame to a JDialog.
// 2005-05-03	JTS, RTi		* Changed MutableJList to SimpleJList.
//					* The messages at the top were not
//					  being properly filtered according to 
//					  message level when a file was opened.
//					  This has been corrected.
// 2005-05-26	JTS, RTi		Removed all message log code and
//					moved it to MessageLogJPanel.  This
//					class is now just a shell class for
//					displaying that panel.
//------------------------------------------------------------------------------

namespace RTi.Util.Message
{


	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using PropList = RTi.Util.IO.PropList;

	/// <summary>
	/// The MessageLogJDialog class provides a graphical user interface for 
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
	/// <para>The message levels are interpreted by MessageLogJDialog, resulting in 
	/// summary messages being listed in an area at the top of the viewer.  By 
	/// default, warning level 1 and 2 messages are listed in the summary, to 
	/// facilitate troubleshooting.  The summary messages are associated with the 
	/// messages in the full log display, providing a popup menu and simplifying 
	/// navigation of the full message log.</para>
	/// <para> The tags are also interpreted by MessageLogJDialog to allow 
	/// navigation in an application.  Messages with tags are printed using the 
	/// Message.print*(..., String Tag,...) methods.  If no tag is provided, then 
	/// messages will not include the <tag> information.  If the tag is provided, then 
	/// the MessageLogJDialog is able to notify MessageLogListener instances 
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
//ORIGINAL LINE: @SuppressWarnings("serial") public class MessageLogJDialog extends javax.swing.JDialog implements java.awt.event.WindowListener
	public class MessageLogJDialog : JDialog, WindowListener
	{

	/// <summary>
	/// The panel that does all the work of displaying the log file.
	/// </summary>
	private MessageLogJPanel __messageLogJPanel = null;

	/// <summary>
	/// The parent is used to center this dialog when set to visible.
	/// </summary>
	private Component __parent = null;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="parent"> the parent JDialog on which this dialog should be opened.
	/// Cannot be null. </param>
	/// <param name="modal"> if true, this dialog will be opened as a modal dialog. </param>
	/// <param name="props"> the PropList that controls the dialog's properties.  See the
	/// first constructor for valid properties. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MessageLogJDialog(javax.swing.JDialog parent, boolean modal, RTi.Util.IO.PropList props) throws Exception
	public MessageLogJDialog(JDialog parent, bool modal, PropList props) : base(parent, modal)
	{
		setupGUI(props);
		__parent = parent;
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="parent"> the parent JFrame on which this dialog should be opened.
	/// Cannot be null. </param>
	/// <param name="modal"> if true, this dialog will be opened as a modal dialog. </param>
	/// <param name="props"> the PropList that controls the dialog's properties.  See the
	/// first constructor for valid properties. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MessageLogJDialog(javax.swing.JFrame parent, boolean modal, RTi.Util.IO.PropList props) throws Exception
	public MessageLogJDialog(JFrame parent, bool modal, PropList props) : base(parent, modal)
	{
		setupGUI(props);
		__parent = parent;
	}

	/// <summary>
	/// Used by the MessageLogJPanel to add its summary panel to the dialog. </summary>
	/// <param name="summaryJPanel"> the summary panel to add. </param>
	protected internal virtual void add(JPanel summaryJPanel)
	{
		getContentPane().add("North", summaryJPanel);
		invalidate();
		validate();
		repaint();
		summaryJPanel.repaint();
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~MessageLogJDialog()
	{
		__messageLogJPanel = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
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
		__messageLogJPanel.processLogFile(filename);
	}

	/// <summary>
	/// Used by the MessageLogJDialog to remove its summary panel from the dialog. </summary>
	/// <param name="summaryJPanel"> the summary panel to be removed. </param>
	protected internal virtual void remove(JPanel summaryJPanel)
	{
		getContentPane().remove(summaryJPanel);
	}

	/// <summary>
	/// Sets up the GUI.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void setupGUI(RTi.Util.IO.PropList props) throws Exception
	private void setupGUI(PropList props)
	{
		addWindowListener(this);

		__messageLogJPanel = new MessageLogJPanel(this, props);
		getContentPane().add("Center", __messageLogJPanel);
		getContentPane().add("South", __messageLogJPanel.getButtonJPanel());

		if (string.ReferenceEquals(JGUIUtil.getAppNameForWindows(), null))
		{
			setTitle("Message Log");
		}
		else
		{
			setTitle(JGUIUtil.getAppNameForWindows() + " - Message Log");
		}

	//	JGUIUtil.setIcon(this, JGUIUtil.getIconImage());

		pack();
		setSize(getWidth() + 200, getHeight());
			// pack() packs in everything pretty well, but the overall
			// size is just a little small for the worksheet to display
			// nicely.
		JGUIUtil.center(this,__parent);
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowActivated(WindowEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowClosed(WindowEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowClosing(WindowEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowDeactivated(WindowEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowDeiconified(WindowEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowIconified(WindowEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowOpened(WindowEvent @event)
	{
	}

	}

}