using System;
using System.Collections.Generic;

// URLHelpJFrame - GUI for the URLHelp class

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
// URLHelpJFrame - GUI for the URLHelp class
// ----------------------------------------------------------------------------
// Copyright: see the COPYRIGHT file.
// ----------------------------------------------------------------------------
// History:
//
// 24 Jan 1998	Steven A. Malers, RTi	First version.
// 14 Apr 1999	SAM, RTi		Update to include browse buttons to
//					pick the web browser, etc.  Change so
//					the initial refresh reads the index
//					file since the read is now triggered
//					by viewing in URLHelp.
// 07 Jun 1999	SAM, RTi		Fix problem where if running
//					stand-alone, we don't want the help to
//					read the index file until it is
//					actually needed.  For the GUI, update
//					so the index list is not shown until
//					the GUI is made visible.
// 2001-11-14	SAM, RTi		Update javadoc.  Change GUI to JGUIUtil.
//					Add finalize().  Verify that variables
//					are set to null when no longer used.
//					Move the browser and index selection to
//					the bottom since they should be set
//					correctly at run-time now.  Change so
//					that index is re-read only if the URL
//					has changed.  Allow the title to be set.
//					Change so that when resizing vertically,
//					the list can grow but other components
//					cannot.  Remove menu items (no need for
//					this Dialog to have a menu).  Add
//					JPopupMenu to list to allow display and
//					search of help.
// ----------------------------------------------------------------------------
// 2003-05-19	J. Thomas Sapienza, RTi	Initial Swing version from AWT code.
// 2002-11-29	SAM, RTi		Set the title and icon consistent with
//					other components.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.Util.Help
{





	using JFileChooserFactory = RTi.Util.GUI.JFileChooserFactory;
	using FindInJListJDialog = RTi.Util.GUI.FindInJListJDialog;
	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using SimpleFileFilter = RTi.Util.GUI.SimpleFileFilter;
	using SimpleJButton = RTi.Util.GUI.SimpleJButton;
	using SimpleJMenuItem = RTi.Util.GUI.SimpleJMenuItem;

	/// <summary>
	/// This class implements a graphical user interface (GUI) for the URLHelp
	/// class.  The GUI should be instantiated with code similar to the following:
	/// <para>
	/// 
	/// <pre>
	///        private URLHelpJFrame _help__index_gui = null;
	/// 
	///        // Add the Help Index GUI using the standard dialog...
	/// 
	///        _help__index_gui = new URLHelpJFrame ();
	///        _help__index_gui.attachMainJMenu ( _help_menu );
	/// </pre>
	/// </para>
	/// <para>
	/// 
	/// The class code will attach its own menus to the specified menu and will
	/// set up its own event handlers.  The GUI interface appears as follows:
	/// </para>
	/// <para>
	/// 
	/// <center>
	/// </para>
	/// <img src="URLHelpJFrame.gif"><para>
	/// </center>
	/// 
	/// The browser path is that specified by URLHelp.getBrowser() and the index is
	/// that specified by URLHelp.getIndexURL() (RTi in the past used
	/// the <tt>-browser Browser</tt> and <tt>-helpindex URL</tt> command-line
	/// arguments or applet parameters which are interpreted in the main program,
	/// resulting in calls to URLHelp.setBrowser() and URLHelp.setIndexURL().  Newer
	/// code uses the URLHelp.initialize() method to set up the help system.)
	/// The help topics are those read from
	/// the index file by the URLHelp.readIndex() function.
	/// A help topic can be selected and when
	/// "Get Help for Selected Topic" is pressed, the URL will be displayed in a
	/// stand-alone browser (if running as a stand-alone application) or into a blank
	/// browser page if running as an applet.
	/// </para>
	/// <para>
	/// 
	/// It is envisioned that the selection of the browser and index will be enhanced
	/// and perhaps a "Details" button will display the key and URL as well as the
	/// topic to help documentation writers.
	/// 
	/// </para>
	/// </summary>
	/// <seealso cref= URLHelp </seealso>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class URLHelpJFrame extends javax.swing.JFrame implements java.awt.event.ActionListener, java.awt.event.KeyListener, java.awt.event.MouseListener, javax.swing.event.ListSelectionListener
	public class URLHelpJFrame : JFrame, ActionListener, KeyListener, MouseListener, ListSelectionListener
	{

	private readonly string __NO_TOPICS = "No topics selected";

	private readonly string __SELECT_BROWSER = "URLHelp.SelectBrowser";
	private readonly string __CLOSE = "URLHelp.Close";
	private readonly string __URLHELP = "URLHelp";
	private readonly string __HELP = "URLHelp.GetHelp";
	private readonly string __SELECT_INDEX = "URLHelp.SelectIndex";
	private readonly string __SEARCH = "URLHelp.Search for...";

	private JTextField __browserJTextField;
	private JTextField __indexJTextField;

	private SimpleJButton __getHelpButton;
	private SimpleJButton __browser_selectButton;
	private SimpleJButton __index_selectButton;

	private JList<string> __topicJList;

	private bool __dataRefreshedOnce = false;

	private JPopupMenu __helpJPopupMenu;

	/// <summary>
	/// Construct with the specified mode. </summary>
	/// <param name="mode"> 1 if the GUI should be visible at construction, 0 if hidden. </param>
	/// <param name="title"> Title of window (default is "Help Index"). </param>
	public URLHelpJFrame(int mode, string title) : base("Help Index")
	{
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		if (string.ReferenceEquals(title, null))
		{
			if ((string.ReferenceEquals(JGUIUtil.getAppNameForWindows(), null)) || JGUIUtil.getAppNameForWindows().Equals(""))
			{
				setTitle("Help Index");
			}
			else
			{
				setTitle(JGUIUtil.getAppNameForWindows() + " - Help Index");
			}
		}
		else
		{
			if ((string.ReferenceEquals(JGUIUtil.getAppNameForWindows(), null)) || JGUIUtil.getAppNameForWindows().Equals(""))
			{
				setTitle(title + " - Help Index");
			}
			else
			{
				setTitle(JGUIUtil.getAppNameForWindows() + " - " + title + " - Help Index");
			}
		}
		openGUI(mode);
	}

	/// <summary>
	/// Construct with the default mode (do not make visible at construction).
	/// </summary>
	public URLHelpJFrame() : this(0, null)
	{
	}

	/// <summary>
	/// Construct with the specified mode. </summary>
	/// <param name="mode"> 1 if the GUI should be visible at construction, 0 if hidden. </param>
	public URLHelpJFrame(int mode) : this(mode, null)
	{
	}

	/// <summary>
	/// Handle action events. </summary>
	/// <param name="event"> Action event. </param>
	public virtual void actionPerformed(ActionEvent @event)
	{ // Check the names of the events.  These are tied to menu names.
		string command = @event.getActionCommand();
		if (command.Equals(__URLHELP))
		{
			// The main menu choice... Make the GUI visible...
			if (!__dataRefreshedOnce)
			{
				refresh(true);
			}
			setVisible(true);
		}
		else if (command.Equals(__CLOSE))
		{
			// Make the GUI hidden...
			setVisible(false);
		}
		else if (command.Equals(__HELP))
		{
			// Get help for the selected topic...
			int index = __topicJList.getSelectedIndex();
			if (index >= 0)
			{
				URLHelp.showHelpForIndex(index);
			}
		}
		else if (command.Equals(__SELECT_BROWSER))
		{
			string lastDirectory = JGUIUtil.getLastFileDialogDirectory();
			JFileChooser fc = JFileChooserFactory.createJFileChooser(lastDirectory);
			fc.setDialogTitle("Select Web Browser");
			SimpleFileFilter jff = new SimpleFileFilter("exe", "Executable Files");
			fc.addChoosableFileFilter(jff);
			fc.setFileFilter(jff);
			fc.showOpenDialog(this);

			File file = fc.getSelectedFile();
			if (file == null || file.getName() == null || file.getName().Equals(""))
			{
				return;
			}

			string fileName = file.getParent() + "\\" + file.getName();

			URLHelp.setBrowser(fileName);
			__browserJTextField.setText(fileName);
		}
		else if (command.Equals(__SELECT_INDEX))
		{
			// Select and set the index...
			string lastDirectory = JGUIUtil.getLastFileDialogDirectory();
			JFileChooser fc = JFileChooserFactory.createJFileChooser(lastDirectory);
			fc.setDialogTitle("Select Help Index File");
			SimpleFileFilter jff = new SimpleFileFilter("html", "HTML Files");
			fc.addChoosableFileFilter(jff);
			fc.setFileFilter(jff);
			fc.showOpenDialog(this);

			File file = fc.getSelectedFile();
			if (file == null || file.getName() == null || file.getName().Equals(""))
			{
				return;
			}

			string fileName = file.getParent() + "\\" + file.getName();
			JGUIUtil.setLastFileDialogDirectory(file.getParent());

			__indexJTextField.setText(fileName);
		}
		else if (command.Equals(__SEARCH))
		{
			new FindInJListJDialog(this, __topicJList, "Find Help Topic");
		}
		command = null;
	}

	/// <summary>
	/// Attach the GUI menus to the specified menu. </summary>
	/// <param name="menu"> The menu to attach to. </param>
	public virtual void attachMainJMenu(JMenu menu)
	{ // The command used will be what triggers the GUI to become visible
		// when the menu is selected in the main app!
		menu.add(new SimpleJMenuItem("Help Index...",__URLHELP,this));
	}

	/// <summary>
	/// Close the GUI.  At this time it just hides the GUI because the GUI handles
	/// events for itself. </summary>
	/// <param name="status"> Unused at this time.  Specify zero. </param>
	public virtual void closeGUI(int status)
	{
		setVisible(false);
	}

	/// <summary>
	/// Clean up for garbage collection. </summary>
	/// <exception cref="Throwable"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~URLHelpJFrame()
	{
		__browserJTextField = null;
		__indexJTextField = null;

		__getHelpButton = null;
		__browser_selectButton = null;
		__index_selectButton = null;

		__topicJList = null;
		__helpJPopupMenu = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Handle key press events. </summary>
	/// <param name="event"> The key press event. </param>
	public virtual void keyPressed(KeyEvent @event)
	{
	}

	/// <summary>
	/// Handle key release events. </summary>
	/// <param name="event"> The key release event. </param>
	public virtual void keyReleased(KeyEvent @event)
	{
	}

	/// <summary>
	/// Handle key type events. </summary>
	/// <param name="event"> The key type event. </param>
	public virtual void keyTyped(KeyEvent @event)
	{
	}

	/// <summary>
	/// Handle mouse clicked event.
	/// </summary>
	public virtual void mouseClicked(MouseEvent @event)
	{
	}

	/// <summary>
	/// Handle mouse entered event.
	/// </summary>
	public virtual void mouseEntered(MouseEvent @event)
	{
	}

	/// <summary>
	/// Handle mouse exited event.
	/// </summary>
	public virtual void mouseExited(MouseEvent @event)
	{
	}

	/// <summary>
	/// Handle mouse pressed event.
	/// </summary>
	public virtual void mousePressed(MouseEvent @event)
	{
		int mods = @event.getModifiers();
		Component c = @event.getComponent();
		if (c.Equals(__topicJList) && (__topicJList.getModel().getSize() > 0) && ((mods & MouseEvent.BUTTON3_MASK) != 0))
		{ //&&
			//event.isPopupTrigger() ) {
			__helpJPopupMenu.show(@event.getComponent(), @event.getX(), @event.getY());
		}
		c = null;
	}

	/// <summary>
	/// Handle mouse released event.
	/// </summary>
	public virtual void mouseReleased(MouseEvent @event)
	{
	}
	/// <summary>
	/// Open the GUI. </summary>
	/// <param name="mode"> if 1, make the GUI visible; if 0, hide the GUI. </param>
	private void openGUI(int mode)
	{ // objects to be used in the GUI Layout
		int b = 3;
		Insets NLBR = new Insets(0,b,b,b);
		Insets TLNR = new Insets(b,b,0,b);
		Insets TNNR = new Insets(b,0,0,b);
		Insets TLNN = new Insets(b,b,0,0);

		// Make sure that we have a valid URLHelp to hold the data...
		// Use a main panel with grid bag layout so the list can expand...

		GridBagLayout gbl = new GridBagLayout();

		// Add a list of the available index information at the top of the
		// dialog...

		JPanel topics_JPanel = new JPanel();
		topics_JPanel.setLayout(gbl);

		int y = 0;
		JGUIUtil.addComponent(topics_JPanel, new JLabel("Help Topics:"), 0, y, 8, 1, 1, 0, TLNR, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);

		IList<URLHelpData> data = URLHelp.getData();
		// We create the list no matter what here so there is a list to work
		// with later.  The "refresh" is used to reset the list...
		if ((data == null) || (mode != JGUIUtil.GUI_VISIBLE))
		{
			// There is no help index available.  Add a list with one
			// item that has "No topics available"...
			List<string> v = new List<string>(1);
			v.Add(__NO_TOPICS);
			__topicJList = new JList<string>();
			__topicJList.setListData(v);
			JGUIUtil.addComponent(topics_JPanel, new JScrollPane(__topicJList), 0, ++y, 8, 1, 1, 1, NLBR, GridBagConstraints.BOTH, GridBagConstraints.WEST);
		}
		else
		{ // Add a list that has all the help topics shown...
			// Wait until refresh to fill!
			List<string> v = new List<string>();
			v.Add(__NO_TOPICS);
			__topicJList = new JList<string>();
			JGUIUtil.addComponent(topics_JPanel, new JScrollPane(__topicJList), 0, ++y, 8, 1, 1, 1, NLBR, GridBagConstraints.BOTH, GridBagConstraints.WEST);
		}
		__topicJList.setSize(300, 200);
		__topicJList.addListSelectionListener(this);
		__topicJList.addMouseListener(this);

		__helpJPopupMenu = new JPopupMenu("");
		__helpJPopupMenu.add(new SimpleJMenuItem("Search for...", __SEARCH, this));
		__helpJPopupMenu.add(new SimpleJMenuItem("Show Help", __HELP, this));
		getContentPane().add(__helpJPopupMenu);

		// Add a panel that displays index file and browser and allows user to
		// select...

		JGUIUtil.addComponent(topics_JPanel, new JLabel("Browser:"), 0, ++y, 1, 1, 0, 0, TLNN, GridBagConstraints.NONE, GridBagConstraints.WEST);
		__browserJTextField = new JTextField(URLHelp.getBrowser());
		// User must browse...
		__browserJTextField.setEnabled(false);
		JGUIUtil.addComponent(topics_JPanel, __browserJTextField, 1, y, 6, 1, 1, 0, TNNR, GridBagConstraints.HORIZONTAL, GridBagConstraints.EAST);
		__browser_selectButton = new SimpleJButton("Browse...", __SELECT_BROWSER,this);
		__browser_selectButton.setToolTipText("Select the web browser program to run.");
		JGUIUtil.addComponent(topics_JPanel, __browser_selectButton, 7, y, 1, 1, 0, 0, TNNR, GridBagConstraints.NONE, GridBagConstraints.EAST);

		JGUIUtil.addComponent(topics_JPanel, new JLabel("Help Index:"), 0, ++y, 1, 1, 0, 0, TLNN, GridBagConstraints.NONE, GridBagConstraints.WEST);
		__indexJTextField = new JTextField(URLHelp.getIndexURL());
		// User must browse...
		__indexJTextField.setEnabled(false);
		//__indexJTextField.addKeyListener ( this );
		JGUIUtil.addComponent(topics_JPanel, __indexJTextField, 1, y, 6, 1, 1, 0, TNNR, GridBagConstraints.HORIZONTAL, GridBagConstraints.EAST);
		__index_selectButton = new SimpleJButton("Browse...", __SELECT_INDEX,this);
		__index_selectButton.setToolTipText("Select the help index file for documentation.");
		JGUIUtil.addComponent(topics_JPanel, __index_selectButton, 7, y, 1, 1, 0, 0, TNNR, GridBagConstraints.NONE, GridBagConstraints.EAST);

		// Now add the buttons at the bottom...

		// Only center panels can resize!!!!
		getContentPane().add("Center", topics_JPanel);

		JPanel button_JPanel = new JPanel();
		getContentPane().add("South", button_JPanel);
		button_JPanel.setLayout(new FlowLayout(FlowLayout.CENTER));

		__getHelpButton = new SimpleJButton("Show Help", __HELP,this);
		__getHelpButton.setToolTipText("Display the selected help topic using the web browser.");
		__getHelpButton.setEnabled(false);
		button_JPanel.add(__getHelpButton);

		button_JPanel.add(new SimpleJButton("Close",__CLOSE,this));

		// Now clean up...

		if ((mode & JGUIUtil.GUI_VISIBLE) != 0)
		{
			// We want to see the GUI at creation...
			setVisible(true);
		}
		else
		{ // We don't want to see the GUI at creation...
			setVisible(false);
		}

		if (mode == JGUIUtil.GUI_VISIBLE)
		{
			// Refresh the list now...
			refresh(true);
		}
		setSize(300,300);
		pack();
		JGUIUtil.center(this);

		// Clean up...

		NLBR = null;
		TLNR = null;
		TNNR = null;
		TLNN = null;
		topics_JPanel = null;
		button_JPanel = null;
		gbl = null;
		data = null;
	}

	/// <summary>
	/// Handle window events. </summary>
	/// <param name="event"> The window event. </param>
	public virtual void processWindowEvent(WindowEvent @event)
	{
		if (@event.getID() == WindowEvent.WINDOW_CLOSING)
		{
			setVisible(false);
		}
	}

	/// <summary>
	/// This function takes the data fields and refreshes the results by running
	/// the security checks again. </summary>
	/// <param name="flag"> true if the index should be re-read. </param>
	private void refresh(bool flag)
	{ // Indicate that the data have been refreshed at least once...

		__dataRefreshedOnce = true;

		// Re-read the index file and reset the list.  ALL OF THIS NEEDS BETTER
		// ERROR HANDLING!!!...

		string index_file = __indexJTextField.getText();
		int len = 0;
		if (!string.ReferenceEquals(index_file, null))
		{
			len = index_file.Length;
		}
		if (len > 0)
		{
			// First set the index and reread the data...
			if (flag)
			{
				// Only reset if something has changed...
				if (!URLHelp.getIndexURL().Equals(index_file, StringComparison.OrdinalIgnoreCase))
				{
					URLHelp.setIndexURL(index_file);
					URLHelp.readIndex();
				}
			}
			// Now reset the list.  By this point, there will be something
			// in the list so we just clear all and add again...
			IList<URLHelpData> data = URLHelp.getData();
			if (data != null)
			{
				__topicJList.removeAll();
				__getHelpButton.setEnabled(false);
				if (data.Count < 1)
				{
					List<string> v = new List<string>();
					v.Add(__NO_TOPICS);
					__topicJList.setListData(v);
				}
				else
				{
					URLHelpData idata = null;
					List<string> v = new List<string>();
					for (int i = 0; i < data.Count; i++)
					{
						idata = data[i];
						// At some point we may want to allow
						// display of things other than the
						// topic, sort the topics, etc.
						//__topicJList.add ( idata.getTopic() );
						v.Add(idata.getTopic());
					}
					__topicJList.setListData(v);
					idata = null;
				}
			}
			data = null;
		}
		// Reset the browser...
		URLHelp.setBrowser(__browserJTextField.getText().Trim());
		index_file = null;
	}

	public virtual void valueChanged(ListSelectionEvent e)
	{
		// All we care is that the help item is selected and we enable the
		// help button...

		string @string = (string)__topicJList.getSelectedValue();
		if (string.ReferenceEquals(@string, null))
		{
			// IE 4 was catching a null here?
			__getHelpButton.setEnabled(false);
		}
		string browser = URLHelp.getBrowser();
		if (string.ReferenceEquals(browser, null))
		{
			__getHelpButton.setEnabled(false);
		}
		if (browser.Length <= 0)
		{
			__getHelpButton.setEnabled(false);
		}
		if (!@string.Equals(__NO_TOPICS))
		{
			__getHelpButton.setEnabled(true);
		}
	}

	}

}