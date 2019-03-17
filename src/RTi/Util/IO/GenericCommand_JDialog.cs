﻿// GenericCommand_JDialog - simple JTextArea editor for command

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

// ----------------------------------------------------------------------------
// GenericCommand_JDialog - simple JTextArea editor for command
// ----------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
// ----------------------------------------------------------------------------
// History: 
//
// 2005-04-29	Steven A. Malers, RTi	Initial version (copy and modify old
//					GenericCommand_JDialog).
// 2005-05-09	SAM, RTi		Rename Command_JDialog class to
//					GenericCommand_JDialog.
// 2005-05-19	SAM, RTi		Move from TSTool package.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.Util.IO
{

	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using SimpleJButton = RTi.Util.GUI.SimpleJButton;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class GenericCommand_JDialog extends javax.swing.JDialog implements java.awt.event.ActionListener, java.awt.event.KeyListener, java.awt.event.WindowListener
	public class GenericCommand_JDialog : JDialog, ActionListener, KeyListener, WindowListener
	{
	private JTextArea __command_JTextArea = null;
							// Command as JTextArea
	private bool __error_wait = false; // Is there an error that we
							// are waiting to be cleared up
							// or Cancel?
	private bool __first_time = true;
	private Command __command = null; // Command to edit.
	private bool __ok = false; // Indicates whether the user
							// has pressed OK to close the
							// dialog.

	/// <summary>
	/// GenericCommand_JDialog constructor. </summary>
	/// <param name="parent"> JFrame class instantiating this class. </param>
	/// <param name="command"> Command to edit. </param>
	public GenericCommand_JDialog(JFrame parent, Command command) : base(parent, true)
	{
		initialize(parent, command);
	}

	/// <summary>
	/// Respond to ActionEvents. </summary>
	/// <param name="event"> ActionEvent object </param>
	public virtual void actionPerformed(ActionEvent @event)
	{
		string s = @event.getActionCommand();

		if (s.Equals("Cancel"))
		{
			response(false);
		}
		else if (s.Equals("OK"))
		{
			checkInput();
			refresh();
			if (!__error_wait)
			{
				response(true);
			}
		}
	}

	/// <summary>
	/// Check the input.  Since the user can edit the command as they like, no checks
	/// are currently performed.
	/// </summary>
	private void checkInput()
	{
	}

	/// <summary>
	/// Commit the edits to the command.  In this case the command should be reparsed
	/// to check its low-level values.
	/// </summary>
	private void commitEdits()
	{
		string command_string = __command_JTextArea.getText();
		// Edit directly since protected data
		// REVISIT SAM 2005-05-09 Is an interface method needed?
		__command.setCommandString(command_string);
		// REVISIT SAM 2005-05-09 Need to somehow get parameters using
		// standard format?
	}

	/// <summary>
	/// Instantiates the GUI components. </summary>
	/// <param name="parent"> JFrame class instantiating this class. </param>
	/// <param name="command"> Command to edit. </param>
	private void initialize(JFrame parent, Command command)
	{
		__command = command;

		addWindowListener(this);

			Insets insetsTLBR = new Insets(2,2,2,2);

		// Main panel...

		JPanel main_JPanel = new JPanel();
		main_JPanel.setLayout(new GridBagLayout());
		getContentPane().add("North", main_JPanel);
		int y = 0;

			JGUIUtil.addComponent(main_JPanel, new JLabel("Edit the command without error checks."), 0, y, 7, 1, 0, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
			JGUIUtil.addComponent(main_JPanel, new JLabel("Use the command editor dialogs to verify " + "command syntax."), 0, ++y, 7, 1, 0, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);

			JGUIUtil.addComponent(main_JPanel, new JLabel("Command:"), 0, ++y, 1, 1, 0, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.EAST);
		__command_JTextArea = new JTextArea(8, 60);
		__command_JTextArea.setLineWrap(true);
		__command_JTextArea.setWrapStyleWord(true);
		__command_JTextArea.addKeyListener(this);
		JGUIUtil.addComponent(main_JPanel, new JScrollPane(__command_JTextArea), 1, y, 6, 1, 1, 0, insetsTLBR, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);

		// Refresh the contents...
		refresh();

		// South Panel: North
		JPanel button_JPanel = new JPanel();
		button_JPanel.setLayout(new FlowLayout(FlowLayout.CENTER));
			JGUIUtil.addComponent(main_JPanel, button_JPanel, 0, ++y, 8, 1, 1, 0, insetsTLBR, GridBagConstraints.HORIZONTAL, GridBagConstraints.CENTER);

		button_JPanel.add(new SimpleJButton("Cancel", "Cancel", this));
		button_JPanel.add(new SimpleJButton("OK", "OK", this));

		if (command.getCommandName().Length > 0)
		{
			setTitle("Edit " + command.getCommandName() + " command");
		}
		else
		{
			setTitle("Edit command");
		}
		// Dialogs do not need to be resizable...
		setResizable(true);
			pack();
			JGUIUtil.center(this);
			base.setVisible(true);
	}

	/// <summary>
	/// Respond to KeyEvents.
	/// </summary>
	public virtual void keyPressed(KeyEvent @event)
	{
		int code = @event.getKeyCode();

		if (code == KeyEvent.VK_ENTER)
		{
			refresh();
			if (!__error_wait)
			{
				response(true);
			}
		}
	}

	public virtual void keyReleased(KeyEvent @event)
	{
	}

	public virtual void keyTyped(KeyEvent @event)
	{
		;
	}

	/// <summary>
	/// Indicate if the user pressed OK (cancel otherwise).
	/// </summary>
	public virtual bool ok()
	{
		return __ok;
	}

	/// <summary>
	/// Refresh the command from the other text field contents.
	/// </summary>
	private void refresh()
	{
		__error_wait = false;
		if (__first_time)
		{
			// Populate the component from the intial command...
			__first_time = false;
			// Parse the incoming string and fill the fields...
			__command_JTextArea.setText(__command.ToString());
		}
	}

	/// <summary>
	/// React to the user response. </summary>
	/// <param name="ok"> if false, then the edit is cancelled.  If true, the edit is committed
	/// and the dialog is closed. </param>
	public virtual void response(bool ok)
	{
		__ok = ok;
		if (ok)
		{
			// Commit the changes...
			commitEdits();
			if (__error_wait)
			{
				// Not ready to close out!
				return;
			}
		}
		// Now close out...
		setVisible(false);
		dispose();
	}

	/// <summary>
	/// Responds to WindowEvents. </summary>
	/// <param name="event"> WindowEvent object </param>
	public virtual void windowClosing(WindowEvent @event)
	{
		response(false);
	}

	public virtual void windowActivated(WindowEvent evt)
	{
		;
	}
	public virtual void windowClosed(WindowEvent evt)
	{
		;
	}
	public virtual void windowDeactivated(WindowEvent evt)
	{
		;
	}
	public virtual void windowDeiconified(WindowEvent evt)
	{
		;
	}
	public virtual void windowIconified(WindowEvent evt)
	{
		;
	}
	public virtual void windowOpened(WindowEvent evt)
	{
		;
	}

	}

}