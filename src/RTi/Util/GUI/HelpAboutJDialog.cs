using System.Collections.Generic;

// HelpAboutJDialog - provides a Help About JDialog with general features

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


	using IOUtil = RTi.Util.IO.IOUtil;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This class provides a simple "Help About" JDialog.
	/// The events for the JDialog are handled according to the ResponseJDialog base class.
	/// Construct the JDialog using code as in the following example:
	/// <para>
	/// <pre>
	/// // Tie to a GUI's action listener and create in the
	/// // actionPerformed routine...
	/// else if (action.equals("Help.About")) {
	///		// Start a modal JDialog...
	///		String help_string = new String (
	///		IO.getProgramName() + " (TM) - Internet Edition\n" +
	///		"Version " + IO.getProgramVersion() + "\n" +
	///		"Copyright 1997...\n");
	/// 
	///		HelpAboutJDialog help_about_gui = new HelpAboutJDialog (this,
	///			"About " + IO.getProgramName(), help_string );
	/// }
	/// </pre>
	/// </para>
	/// <para>
	/// The text for the help JDialog can contain newline characters (\n) to cause
	/// line breaks.  Additional formatting options may be added.
	/// </para>
	/// <para>
	/// </para>
	/// </summary>
	/// <seealso cref= ResponseDialog
	/// </p> </seealso>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class HelpAboutJDialog extends ResponseJDialog implements java.awt.event.ActionListener, java.awt.event.KeyListener, java.awt.event.WindowListener
	public class HelpAboutJDialog : ResponseJDialog, ActionListener, KeyListener, WindowListener
	{

	/// <summary>
	/// Button labels.
	/// </summary>
	private string __BUTTON_SYSTEM_DETAILS = "Show Software/System Details", __BUTTON_OK = "OK";

	/// <summary>
	/// GUI buttons.
	/// </summary>
	private SimpleJButton __jarJButton = null, __okJButton = null;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="parent"> The parent JFrame. </param>
	/// <param name="title"> Title for the JDialog. </param>
	/// <param name="text"> Text for the JDialog.  Use newline characters to break lines. </param>
	/// <param name="showSystemDetails"> if true, show a button that allows the user to display system details, useful
	/// for troubleshooting; if false, the button is only available when debug logging is turned on </param>
	public HelpAboutJDialog(JFrame parent, string title, string text, bool showSystemDetails) : base(parent, false) // Not modal because may need to remain visible during troubleshooting in other windows
	{
		initialize(title, text, showSystemDetails);
	}

	/// <summary>
	/// Responds to ActionEvents. </summary>
	/// <param name="event"> ActionEvent object </param>
	public override void actionPerformed(ActionEvent @event)
	{
		string s = @event.getActionCommand();
		if (s.Equals(__BUTTON_OK))
		{
			setVisible(false);
			dispose();
		}
		else if (s.Equals(__BUTTON_SYSTEM_DETAILS))
		{
			IList<string> v1 = IOUtil.getSystemProperties();
			IList<string> v2 = IOUtil.getJarFilesManifests();

			IList<string> v3 = new List<string>();
			for (int i = 0; i < v1.Count; i++)
			{
				v3.Add(v1[i]);
			}
			for (int i = 0; i < v2.Count; i++)
			{
				v3.Add(v2[i]);
			}
			PropList props = new PropList("HelpAboutJDialog");
			props.set("Title=System Information");
			new ReportJDialog(this, v3, props, true);
		}
	}

	/// <summary>
	/// Instantiate the dialog components </summary>
	/// <param name="parent"> JFrame class instantiating this class. </param>
	/// <param name="title"> Dialog title </param>
	/// <param name="label"> Label to display in the GUI. </param>
	/// <param name="showSystemDetails"> if true, show a button that allows the user to display system details, useful
	/// for troubleshooting; if false, the button is only available when debug logging is turned on </param>
	private void initialize(string title, string label, bool showSystemDetails)
	{
		addWindowListener(this);

		// Split the text based on the new-line delimiter - internally use \n, not the platform's separator!
		IList<string> vec = StringUtil.breakStringList(label, "\n", 0);
		int size = vec.Count;

		// Main panel
		JPanel main_JPanel = new JPanel();

		if (vec != null)
		{
			Insets insets = new Insets(1, 5, 1, 5);
			// New approach where alignment can be CENTER (because
			// used by the HelpAboutDialog)...
			   main_JPanel.setLayout(new GridBagLayout());
			if (size > 20)
			{
				//add message String to a JList that is within a JScrollPane
				JList<string> list = null;
				 list = new JList<string>(new List<string>(vec));
				//list.setBackground(Color.LIGHT_GRAY);
				JScrollPane pane = new JScrollPane(list);
				Dimension d = new Dimension(500, 500);
				pane.setPreferredSize(d);
				pane.setMinimumSize(d);
				//pane.setMaximumSize(d);

				//add JScrollPane to JPanel
				   JGUIUtil.addComponent(main_JPanel, pane, 0,0,1,1,1,1,insets, GridBagConstraints.BOTH, GridBagConstraints.CENTER);
			}
			else
			{
				// Add each string as a JLabel...
				for (int i = 0; i < size; i++)
				{
					   JGUIUtil.addComponent(main_JPanel, new JLabel((string)vec[i]), 0, i, 1, 1, 0, 0, insets, GridBagConstraints.NONE, GridBagConstraints.CENTER);
				}
			}
		}

		getContentPane().add("Center", main_JPanel);

		// Now add the buttons...

		// South Panel
		JPanel south_JPanel = new JPanel();
		south_JPanel.setLayout(new BorderLayout());
		getContentPane().add("South", south_JPanel);

		// South Panel: North
		JPanel southNorth_JPanel = new JPanel();
		southNorth_JPanel.setLayout(new FlowLayout(FlowLayout.CENTER));
		south_JPanel.add("North", southNorth_JPanel);

		   __okJButton = new SimpleJButton(__BUTTON_OK, this);
		__okJButton.addKeyListener(this);
		southNorth_JPanel.add(__okJButton);

		if (showSystemDetails || Message.isDebugOn)
		{
			__jarJButton = new SimpleJButton(__BUTTON_SYSTEM_DETAILS, this);
			southNorth_JPanel.add(__jarJButton);
		}

		if (!string.ReferenceEquals(title, null))
		{
			setTitle(title);
		}

		setResizable(true);
		pack();
		JGUIUtil.center(this);
		base.setVisible(true);
		addKeyListener(this);
	}

	/// <summary>
	/// Responds to key pressed events.  Pressing 'Enter' will activate the OK button. </summary>
	/// <param name="e"> the KeyEvent that happened. </param>
	public override void keyPressed(KeyEvent e)
	{
		int code = e.getKeyCode();

		if (code == KeyEvent.VK_ENTER)
		{
			actionPerformed(new ActionEvent(this, 0, __BUTTON_OK));
		}
	}

	/// <summary>
	/// Responds to key released events; does nothing. </summary>
	/// <param name="e"> the KeyEvent that happened. </param>
	public override void keyReleased(KeyEvent e)
	{
	}

	/// <summary>
	/// Responds to key typed events; does nothing. </summary>
	/// <param name="e"> the KeyEvent that happened. </param>
	public override void keyTyped(KeyEvent e)
	{
	}

	public override void windowClosing(WindowEvent @event)
	{
	}
	public override void windowActivated(WindowEvent evt)
	{
	}
	public override void windowClosed(WindowEvent evt)
	{
	}
	public override void windowDeactivated(WindowEvent evt)
	{
	}
	public override void windowDeiconified(WindowEvent evt)
	{
	}
	public override void windowIconified(WindowEvent evt)
	{
	}
	public override void windowOpened(WindowEvent evt)
	{
	}

	}
}