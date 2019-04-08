using System.Collections.Generic;

// ResponseJDialog - provides an area for a message body and buttons for response

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

namespace RTi.Util.GUI
{




	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// ResponseJDialog provides an area for a message body and expects a user response
	/// given a choice of buttons.  Flags can be passed to the constructor that
	/// determine what sort of dialog will be visible.  The flags supported are:
	/// YES_NO_CANCEL, YES_NO, OK.
	/// A ResponseJDialog should be instantiated as follows:
	/// <pre>
	/// int x = new ResponseJDialog( JFrame parent, 
	///		String label, int flag ).response()
	/// </pre>
	/// where processing is halted until a response occurs (the dialog is modal).
	/// The user response is returned through the response() method.
	/// </summary>
	public class ResponseJDialog : JDialog, ActionListener, KeyListener, WindowListener
	{

	private string __BUTTON_CANCEL = "Cancel", __BUTTON_NO = "No", __BUTTON_OK = "OK", __BUTTON_YES = "Yes";

	private SimpleJButton __yes_JButton, __no_JButton, __cancel_JButton, __ok_JButton; // Ok Button
	private int __mode, __response; // user selected response as
						// identified by the return status
						// public final statics below

	/// <summary>
	/// ResponseJDialog modes, which can be ORed together.
	/// </summary>
	public const int YES = 0x1, NO = 0x2, OK = 0x4, CANCEL = 0x8;

	/// <summary>
	/// ResponseJDialog constructor. </summary>
	/// <param name="parent"> JFrame class instantiating this class. </param>
	/// <param name="label"> Label to display in the GUI.  Newlines result in line breaks in the dialog. </param>
	public ResponseJDialog(JFrame parent, string label) : base(parent, true)
	{ // Call the full version with no title and ok Button
		initialize(null, label, OK, GridBagConstraints.WEST);
	}

	/// <summary>
	/// ResponseJDialog constructor. </summary>
	/// <param name="parent"> JDialog class instantiating this class. </param>
	/// <param name="label"> Label to display in the GUI.  Newlines result in line breaks in the dialog. </param>
	public ResponseJDialog(JDialog parent, string label) : base(parent, true)
	{ // Call the full version with no title and ok Button
		initialize(null, label, OK, GridBagConstraints.WEST);
	}

	/// <summary>
	/// ResponseJDialog constructor. </summary>
	/// <param name="parent"> JFrame class instantiating this class. </param>
	/// <param name="label"> Label to display in the GUI.  Newlines result in line breaks in the dialog. </param>
	/// <param name="modal"> whether the dialog is modal or not. </param>
	public ResponseJDialog(JFrame parent, string label, bool modal) : base(parent, modal)
	{ // Call the full version with no title and ok Button
		initialize(null, label, OK, GridBagConstraints.WEST);
	}

	/// <summary>
	/// ResponseJDialog constructor for use by the HelpAboutJDialog. </summary>
	/// <param name="parent"> JDialog class instantiating this class. </param>
	/// <param name="modal"> whether it will be a modal dialog or not. </param>
	protected internal ResponseJDialog(JFrame parent, bool modal) : base(parent, modal)
	{
	}

	/// <summary>
	/// ResponseJDialog constructor. </summary>
	/// <param name="parent"> JFrame class instantiating this class. </param>
	/// <param name="label"> Label to display in the dialog.  Newlines result in line breaks in the dialog. </param>
	/// <param name="mode"> mode in which this UI is to be used (i.e., YES|NO|CANCEL)
	/// process different types of yes responses from the calling form. </param>
	public ResponseJDialog(JFrame parent, string label, int mode) : base(parent, true)
	{ // Call the full version with no title...
		initialize(null, label, mode, GridBagConstraints.WEST);
	}

	/// <summary>
	/// ResponseJDialog constructor. </summary>
	/// <param name="parent"> JFrame class instantiating this class. </param>
	/// <param name="label"> Label to display in the dialog.  Newlines result in line breaks in the dialog. </param>
	/// <param name="mode"> mode in which this UI is to be used (i.e., YES|NO|CANCEL)
	/// process different types of yes responses from the calling form. </param>
	/// <param name="modal"> whether the dialog is modal or not. </param>
	public ResponseJDialog(JFrame parent, string label, int mode, bool modal) : base(parent, modal)
	{ // Call the full version with no title...
		initialize(null, label, mode, GridBagConstraints.WEST);
	}

	/// <summary>
	/// ResponseJDialog constructor. </summary>
	/// <param name="parent"> JDialog class instantiating this class. </param>
	/// <param name="label"> Label to display in the dialog.  Newlines result in line breaks in the dialog. </param>
	/// <param name="mode"> mode in which this UI is to be used (i.e., YES|NO|CANCEL)
	/// process different types of yes responses from the calling form. </param>
	public ResponseJDialog(JDialog parent, string label, int mode) : base(parent, true)
	{ // Call the full version with no title...
		initialize(null, label, mode, GridBagConstraints.WEST);
	}

	/// <summary>
	/// ResponseJDialog constructor. </summary>
	/// <param name="parent"> JFrame class instantiating this class. </param>
	/// <param name="title"> Dialog title. </param>
	/// <param name="label"> Label to display in the GUI.  Newlines result in line breaks in the dialog. </param>
	/// <param name="mode"> mode in which this UI is to be used (i.e., YES|NO|CANCEL)
	/// process different types of yes responses from the calling form. </param>
	public ResponseJDialog(JFrame parent, string title, string label, int mode) : base(parent, true)
	{
		initialize(title, label, mode, GridBagConstraints.WEST);
	}

	/// <summary>
	/// ResponseJDialog constructor. </summary>
	/// <param name="parent"> JFrame class instantiating this class. </param>
	/// <param name="title"> Dialog title. </param>
	/// <param name="label"> Label to display in the GUI.  Newlines result in line breaks in the dialog. </param>
	/// <param name="mode"> mode in which this UI is to be used (i.e., YES|NO|CANCEL)
	/// process different types of yes responses from the calling form. </param>
	/// <param name="modal"> whether the dialog is modal or not. </param>
	public ResponseJDialog(JFrame parent, string title, string label, int mode, bool modal) : base(parent, modal)
	{
		initialize(title, label, mode, GridBagConstraints.WEST);
	}

	/// <summary>
	/// ResponseJDialog constructor. </summary>
	/// <param name="parent"> JDialog class instantiating this class. </param>
	/// <param name="title"> Dialog title. </param>
	/// <param name="label"> Label to display in the GUI.  Newlines result in line breaks in
	/// the dialog. </param>
	/// <param name="mode"> mode in which this UI is to be used (i.e., YES|NO|CANCEL)
	/// process different types of yes responses from the calling form. </param>
	public ResponseJDialog(JDialog parent, string title, string label, int mode) : base(parent, true)
	{
		initialize(title, label, mode, GridBagConstraints.WEST);
	}

	/// <summary>
	/// ResponseJDialog constructor. </summary>
	/// <param name="parent"> Frame class instantiating this class. </param>
	/// <param name="title"> Dialog title. </param>
	/// <param name="label"> Label to display in the GUI.  Newlines result in line breaks in the dialog. </param>
	/// <param name="mode"> mode in which this UI is to be used (i.e., YES|NO|CANCEL)
	/// process different types of yes responses from the calling form. </param>
	/// <param name="alignment"> Specify GridBagConstraints.CENTER to center the text lines. </param>
	public ResponseJDialog(JFrame parent, string title, string label, int mode, int alignment) : base(parent, true)
	{
		initialize(title, label, mode, alignment);
	}

	/// <summary>
	/// ResponseJDialog constructor. </summary>
	/// <param name="parent"> Frame class instantiating this class. </param>
	/// <param name="title"> Dialog title. </param>
	/// <param name="label"> Label to display in the GUI.  Newlines result in line breaks in the dialog. </param>
	/// <param name="mode"> mode in which this UI is to be used (i.e., YES|NO|CANCEL)
	/// process different types of yes responses from the calling form. </param>
	/// <param name="alignment"> Specify GridBagConstraints.CENTER to center the text lines. </param>
	/// <param name="modal"> whether the dialog is modal or not. </param>
	public ResponseJDialog(JFrame parent, string title, string label, int mode, int alignment, bool modal) : base(parent, modal)
	{
		initialize(title, label, mode, alignment);
	}

	/// <summary>
	/// ResponseJDialog constructor. </summary>
	/// <param name="parent"> JDialog class instantiating this class. </param>
	/// <param name="title"> Dialog title. </param>
	/// <param name="label"> Label to display in the GUI.  Newlines result in line breaks in the dialog. </param>
	/// <param name="mode"> mode in which this UI is to be used (i.e., YES|NO|CANCEL)
	/// process different types of yes responses from the calling form. </param>
	/// <param name="alignment"> Specify GridBagConstraints.CENTER to center the text lines. </param>
	public ResponseJDialog(JDialog parent, string title, string label, int mode, int alignment) : base(parent, true)
	{
		initialize(title, label, mode, alignment);
	}

	/// <summary>
	/// Responds to ActionEvents. </summary>
	/// <param name="event"> ActionEvent object </param>
	public virtual void actionPerformed(ActionEvent @event)
	{
		string s = @event.getActionCommand();
		if (s.Equals(__BUTTON_YES))
		{
			__response = YES;
		}
		else if (s.Equals(__BUTTON_NO))
		{
			__response = NO;
		}
		else if (s.Equals(__BUTTON_CANCEL))
		{
			__response = CANCEL;
		}
		else if (s.Equals(__BUTTON_OK))
		{
			__response = OK;
		}
		response();
	}

	/// <summary>
	/// Clean up for garbage collection. </summary>
	/// <exception cref="Throwable"> if an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~ResponseJDialog()
	{
		__yes_JButton = null;
		__no_JButton = null;
		__cancel_JButton = null;
		__ok_JButton = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Instantiate the dialog components </summary>
	/// <param name="parent"> JFrame class instantiating this class. </param>
	/// <param name="title"> Dialog title </param>
	/// <param name="label"> Label to display in the GUI. </param>
	/// <param name="mode"> mode in which this UI is to be used (i.e., YES|NO|CANCEL)
	/// process different types of yes responses from the calling form. </param>
	/// <param name="alignment"> Specify GridBagConstraints.CENTER to center the text lines. </param>
	private void initialize(string title, string label, int mode, int alignment)
	{
		__mode = mode;
		addWindowListener(this);

		// Split the text based on the new-line delimiter (we use \n, not the
		// platform's separator!
		IList<string> vec = StringUtil.breakStringList(label, "\n", 0);
		int size = vec.Count;
		// North Panel
		JPanel north_JPanel = new JPanel();
		if (alignment == GridBagConstraints.CENTER)
		{
			Insets insets = new Insets(1, 5, 1, 5);
			// New approach where alignment can be CENTER (because used by the HelpAboutDialog)...
			north_JPanel.setLayout(new GridBagLayout());
			if (size > 20)
			{
				//add message String to a JList that is within a JScrollPane
				JList list = null;
				if (vec is List<object>)
				{
					list = new JList((List<object>)vec);
				}
				else
				{
					list = new JList(new List<object>(vec));
				}
				list.setBackground(Color.LIGHT_GRAY);
				JScrollPane pane = new JScrollPane(list);
				Dimension d = new Dimension(400, 200);
				pane.setPreferredSize(d);
				pane.setMinimumSize(d);
				pane.setMaximumSize(d);

				//add JScrollPane to JPanel
				JGUIUtil.addComponent(north_JPanel, pane, 0,0,1,1,0,0,insets, GridBagConstraints.NONE, alignment);
			}
			else
			{
				// Add each string as a JLabel...
				for (int i = 0; i < size; i++)
				{
					JGUIUtil.addComponent(north_JPanel, new JLabel((string)vec[i]), 0,i,1,1,0,0,insets, GridBagConstraints.NONE, alignment);
				}
			}
		}
		else
		{
			// This is the layout that was used previously.  If the
			// above works out OK with spacing, etc., might use GridBagLayout always.
			//north_JPanel.setLayout(new GridLayout ( vec.size(), 1));
			if (size > 20)
			{
				north_JPanel.setLayout(new GridLayout(1, 1));
				//add message String to a JList that is within a JScrollPane
				JList list = null;
				if (vec is List<object>)
				{
					list = new JList((List<object>)vec);
				}
				else
				{
					list = new JList(new List<object>(vec));
				}
				list.setBackground(Color.LIGHT_GRAY);
				JScrollPane pane = new JScrollPane(list);
				Dimension d = new Dimension(600, 200);
				pane.setPreferredSize(d);
				pane.setMinimumSize(d);
				pane.setMaximumSize(d);

				//add JScrollPane to JPanel
				north_JPanel.add(pane);
			}
			else
			{
				north_JPanel.setLayout(new GridLayout(vec.Count, 1));
				// Add each string...
				for (int i = 0; i < size; i++)
				{
					north_JPanel.add(new JLabel("    " + vec[i] + "     "));
				}
			}
		}

		getContentPane().add("North", north_JPanel);

		// Now add the buttons...

		// South Panel
		JPanel south_JPanel = new JPanel();
		south_JPanel.setLayout(new BorderLayout());
		getContentPane().add("South", south_JPanel);

		// South Panel: North
		JPanel southNorth_JPanel = new JPanel();
		southNorth_JPanel.setLayout(new FlowLayout(FlowLayout.CENTER));
		south_JPanel.add("North", southNorth_JPanel);

		if ((__mode & YES) != 0)
		{
			// Add a Yes button...
			   __yes_JButton = new SimpleJButton(__BUTTON_YES, this);
			__yes_JButton.addKeyListener(this);
			southNorth_JPanel.add(__yes_JButton);
		}

		if ((__mode & NO) != 0)
		{
			__no_JButton = new SimpleJButton(__BUTTON_NO, this);
			__no_JButton.addKeyListener(this);
			southNorth_JPanel.add(__no_JButton);
		}

		if ((__mode & OK) != 0)
		{
			__ok_JButton = new SimpleJButton(__BUTTON_OK, this);
			__ok_JButton.addKeyListener(this);
			southNorth_JPanel.add(__ok_JButton);
		}

		if ((__mode & CANCEL) != 0)
		{
			__cancel_JButton = new SimpleJButton(__BUTTON_CANCEL, this);
			__cancel_JButton.addKeyListener(this);
			southNorth_JPanel.add(__cancel_JButton);
		}

		if (!string.ReferenceEquals(title, null))
		{
			setTitle(title);
		}
		// Dialogs do no need to be resizable...
		setResizable(false);
		pack();
		JGUIUtil.center(this);
		base.setVisible(true);
		addKeyListener(this);
	}

	/// <summary>
	/// Responds to key pressed events.  If the dialog has been initialized to have a
	/// 'No' button, pressing 'N' will activate that button.  If the dialog has been
	/// initialized to have a 'Yes' button, pressing 'Y' will activate that button.
	/// If the dialog has a 'Cancel' button, pressing 'Escape' will activate that 
	/// button.  If the dialog has an 'OK' button, pressing 'Enter' will activate that button. </summary>
	/// <param name="e"> the KeyEvent that happened. </param>
	public virtual void keyPressed(KeyEvent e)
	{
		int code = e.getKeyCode();

		if (code == KeyEvent.VK_N)
		{
			if ((__mode & NO) == NO)
			{
				actionPerformed(new ActionEvent(this, 0, __BUTTON_NO));
			}
		}
		if (code == KeyEvent.VK_Y)
		{
			if ((__mode & YES) == YES)
			{
				actionPerformed(new ActionEvent(this, 0, __BUTTON_YES));
			}
		}
		if (code == KeyEvent.VK_ESCAPE)
		{
			if ((__mode & CANCEL) == CANCEL)
			{
				actionPerformed(new ActionEvent(this, 0, __BUTTON_CANCEL));
			}
		}
		if (code == KeyEvent.VK_ENTER)
		{
			if ((__mode & OK) == OK)
			{
				actionPerformed(new ActionEvent(this, 0, __BUTTON_OK));
			}
		}
	}

	/// <summary>
	/// Responds to key released events; does nothing. </summary>
	/// <param name="e"> the KeyEvent that happened. </param>
	public virtual void keyReleased(KeyEvent e)
	{
	}

	/// <summary>
	/// Responds to key typed events; does nothing. </summary>
	/// <param name="e"> the KeyEvent that happened. </param>
	public virtual void keyTyped(KeyEvent e)
	{
	}

	/// <summary>
	/// Return the user response and dispose the dialog. </summary>
	/// <returns> the Dialog response (e.g., OK, CANCEL, YES, or NO) </returns>
	public virtual int response()
	{
		setVisible(false);
		dispose();
		return __response;
	}

	/// <summary>
	/// Respond to WindowEvents. </summary>
	/// <param name="event"> WindowEvent object. </param>
	public virtual void windowClosing(WindowEvent @event)
	{
		__response = CANCEL;
		response();
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