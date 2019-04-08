using System;
using System.Collections.Generic;

// MessageJDialog - modal dialog that is displayed with warning level one

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
// MessageJDialog - modal dialog that is displayed with warning level one
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 02 Sep 1997	Matthew J. Rutherford,	Created function.
//		RTi
// 13 Oct 1997	MJR, RTi		Put in code to handle messages that
//					have new-line characters in the middle.
// 12 Dec 1997	Steven A. Malers, RTi	Change to 1.1 event handling.  Hopefully
//					this fixes some CRDSS problems.  Also
//					use the RTi GUI utility class.
// 16 Mar 1998	SAM, RTi		Add javadoc.
// 13 Apr 1999	SAM, RTi		Add finalize.
// 07 Feb 2001	SAM, RTi		Try to allow enter on dialog button to
//					serve as OK - does not seem to work!
//					Previously the handler was
//					being set for the dialog itself, which
//					does not generate key events.  Fix to
//					tie to the button itself.  Other minor
//					cleanup.  Change GUI to GUIUtil.  Need
//					to change so that if the dialog string
//					is > 100 characters, break at 80 for
//					displays.  Add okClicked().
// 15 Mar 2001	SAM, RTi		In conjunction with new Message class
//					data, check for whether to use
//					"OK - Don't Show Warning JDialog" and
//					"Cancel" buttons.
// 2002-05-24	SAM, RTi		Add ability to display a Cancel button
//					and add MessageJDialogListener feature
//					so the "Cancel" button can be detected
//					in high-level code.  Remove AWTEvent
//					code since listeners are being used.
// 2002-10-11	SAM, RTi		Change ProcessManager to
//					ProcessManager1.
// 2002-10-17	AML, RTi		Change ProcessManager1 to
//					ProcessManager.
//------------------------------------------------------------------------------
// 2003-08-22	J. Thomas Sapienza, RTi	Initial Swing version.
// 2003-08-25	JTS, RTi		Corrected infinite loop happening on
//					dialog close.
// 2003-09-30	SAM, RTi		Use the title from main app if set.
// 2003-12-10	SAM, RTi		* Change the properties from
//					  "..JDialog.." back to "..Dialog.."
//					  since the intent is more general than
//					  AWT or Swing.
//					* Remove a few lines of commented code -
//					  no need to retain old font selection
//					  code.
// 2004-01-14	SAM, AML, RTi		* Display the content as a scroll pane
//					  if the number of lines exceeds a
//					  limit.
// 2004-02-04	JTS, RTi		Corrected bug in displaying multi-line
//					warnings caused by incorrect set-up of
//					GridLayout.
// 2004-03-03	JTS, RTi		Added code to wrap lines at 100 
//					characters when they are put into the
//					MessageJDialogs.
// 2005-04-05	JTS, RTi		"View Log File" now uses the new 
//					log file viewer, instead of opening
//					the log file with notepad.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//------------------------------------------------------------------------------
// EndHeader

namespace RTi.Util.Message
{




	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using SimpleJButton = RTi.Util.GUI.SimpleJButton;

	using PropList = RTi.Util.IO.PropList;

	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This class provides a modal dialog for messages.  It is normally only used
	/// from within the Message class to display warning messages for level 1 warnings
	/// (currently the warning prefix is hard-coded here).
	/// The dialog looks similar to the following:
	/// <p align="center">
	/// <img src="MessageJDialog.gif">
	/// </p>
	/// See also the Message.setPropValue() method to control the handling of warning
	/// messages. </summary>
	/// <seealso cref= Message </seealso>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class MessageJDialog extends javax.swing.JDialog implements java.awt.event.ActionListener, java.awt.event.KeyListener, java.awt.event.WindowListener
	public class MessageJDialog : JDialog, ActionListener, KeyListener, WindowListener
	{

	private string _ok_no_more_button_label = null; // JLabel for button indicating
							// whether more warnings should
							// be allowed.

	private static MessageJDialogListener[] _listeners = null;
							// Listeners that want to know
							// the MessageJDialog buttons
							// that are pressed.

	/// <summary>
	/// The parent JFrame on which the dialog was opened.
	/// </summary>
	private JFrame __parent = null;

	/// <summary>
	/// Construct the dialog with the specified message. </summary>
	/// <param name="parent"> JFrame from which the dialog is created. </param>
	/// <param name="message"> Message to display. </param>
	public MessageJDialog(JFrame parent, string message) : base(parent, "Warning!", true)
	{
		__parent = parent;
		if ((string.ReferenceEquals(JGUIUtil.getAppNameForWindows(), null)) || JGUIUtil.getAppNameForWindows().Equals(""))
		{
			setTitle("Warning!");
		}
		else
		{
			setTitle(JGUIUtil.getAppNameForWindows() + " - Warning!");
		}

		addWindowListener(this);

		setResizable(false);

		message = StringUtil.wrap(message, 100);

		IList<string> vec = StringUtil.breakStringList(message, "\n", StringUtil.DELIM_SKIP_BLANKS);
		JPanel pan = new JPanel();
		int size = vec.Count;

		string prop_value = Message.getPropValue("WarningDialogScrollCutoff");
		if ((!string.ReferenceEquals(prop_value, null)) && (prop_value.Equals("true", StringComparison.OrdinalIgnoreCase)) || (size > 20))
		{
			pan.setLayout(new GridLayout(1, 1));
			//use a JList within a JScrollPane to display text
			//instead of just making JLabels 
			JList list = new JList(new List<object>(vec));
			list.setBackground(Color.LIGHT_GRAY);
			JScrollPane pane = new JScrollPane(list);
			Dimension d = new Dimension(600, 200);
			pane.setPreferredSize(d);
			pane.setMinimumSize(d);
			pane.setMaximumSize(d);
			pan.add(pane);
		}
		else
		{
			pan.setLayout(new GridLayout(size, 1));
			for (int i = 0; i < size; i++)
			{
				pan.add(new JLabel("     " + vec[i] + "     "));
			}
		}
		getContentPane().add("Center", pan);

		// Add buttons...

		JPanel bp = new JPanel();
		bp.setLayout(new FlowLayout());

		SimpleJButton ok_Button = new SimpleJButton("OK", this);
		bp.add(ok_Button);

		// Custom buttons based on Message properties...

		prop_value = null;
		prop_value = Message.getPropValue("WarningDialogOKNoMoreButton");
		if ((!string.ReferenceEquals(prop_value, null)) && (prop_value.Equals("true", StringComparison.OrdinalIgnoreCase)))
		{
			_ok_no_more_button_label = Message.getPropValue("WarningDialogOKNoMoreButtonLabel");
			SimpleJButton okno_Button = new SimpleJButton(_ok_no_more_button_label, this);
			bp.add(okno_Button);
		}
		prop_value = Message.getPropValue("WarningDialogViewLogButton");
		if ((!string.ReferenceEquals(prop_value, null)) && (prop_value.Equals("true", StringComparison.OrdinalIgnoreCase)))
		{
			// Only add the button if there is a log file name specified...
			if (Message.getLogFile().Length > 0)
			{
				SimpleJButton view_log_Button = new SimpleJButton("View Log", this);
				bp.add(view_log_Button);
			}
		}
		prop_value = Message.getPropValue("WarningDialogCancelButton");
		if ((!string.ReferenceEquals(prop_value, null)) && (prop_value.Equals("true", StringComparison.OrdinalIgnoreCase)))
		{
			SimpleJButton cancel_Button = new SimpleJButton("Cancel", this);
			bp.add(cancel_Button);
		}

		bp.addKeyListener(this);
		getContentPane().add("South", bp);
		pack();
			JGUIUtil.center(this);
		setVisible(true);
	}

	/// <summary>
	/// Handle action events. </summary>
	/// <param name="e"> ActionEvent to handle. </param>
	public virtual void actionPerformed(ActionEvent e)
	{ // Check the names of the events.  These are tied to button names.
		string command = e.getActionCommand();
		if (command.Equals("Cancel"))
		{
			cancelClicked();
		}
		else if (command.Equals("OK"))
		{
			okClicked();
		}
		else if (command.Equals(_ok_no_more_button_label))
		{
			Message.setPropValue("ShowWarningDialog=false");
			okClicked();
		}
		else if (command.Equals("View Log"))
		{
			viewLogClicked();
		}
	}

	/// <summary>
	/// Add a MissageJDialogListener to receive MessageJDialog events.  
	/// Multiple listeners
	/// can be registered.  MessageJDialog button actions will result in registered
	/// listeners being called. </summary>
	/// <param name="listener"> MessageJDialogListener to add. </param>
	public static void addMessageJDialogListener(MessageJDialogListener listener)
	{ // Use arrays to make a little simpler than Vectors to use later...
		if (listener != null)
		{
			// Resize the listener array...
			if (_listeners == null)
			{
				_listeners = new MessageJDialogListener[1];
				_listeners[0] = listener;
			}
			else
			{ // Need to resize and transfer the list...
				int size = _listeners.Length;
				MessageJDialogListener[] newlisteners = new MessageJDialogListener[size + 1];
				for (int i = 0; i < size; i++)
				{
					newlisteners[i] = _listeners[i];
				}
				_listeners = newlisteners;
				_listeners[size] = listener;
				newlisteners = null;
			}
		}
	}

	/// <summary>
	/// Handle event to close the dialog.
	/// </summary>
	private void cancelClicked()
	{
		setVisible(false);
		// If any MessageJDialogListeners are registered, call the
		// messageJDialogAction() method.
		if (_listeners != null)
		{
			for (int ilist = 0; ilist < _listeners.Length; ilist++)
			{
				_listeners[ilist].messageJDialogAction("Cancel");
			}
		}
		dispose();
	}

	/// <summary>
	/// Finalize before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~MessageJDialog()
	{
		_ok_no_more_button_label = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Handle key events. </summary>
	/// <param name="e"> KeyEvent to handle. </param>
	public virtual void keyPressed(KeyEvent e)
	{ // Handle a return as if OK is pressed...
		if (e.getKeyCode() == KeyEvent.VK_ENTER)
		{
		Message.printStatus(1, "", "return over OK");
			okClicked();
		}
	}

	public virtual void keyReleased(KeyEvent e)
	{
	}

	public virtual void keyTyped(KeyEvent e)
	{
		// Just worry about what is pressed.
	}

	/// <summary>
	/// Handle event to close the dialog.
	/// </summary>
	private void okClicked()
	{
		setVisible(false);
		dispose();
	}

	public virtual void windowActivated(WindowEvent e)
	{
	}
	public virtual void windowClosed(WindowEvent e)
	{
	}
	public virtual void windowClosing(WindowEvent e)
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

	/// <summary>
	/// Display the log file.
	/// </summary>
	private void viewLogClicked()
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
			MessageLogJDialog logDialog = new MessageLogJDialog(__parent, true, p);
			logDialog.processLogFile(Message.getLogFile());
			logDialog.setVisible(true);
		}
		catch (Exception ex)
		{
			string routine = "MessageJDialog.viewLogClicked";
			Message.printWarning(1, routine, "Unable to view log file \"" + Message.getLogFile() + "\"");
			Message.printWarning(2, routine, ex);
		}
	}

	} // end MessageJDialog

}