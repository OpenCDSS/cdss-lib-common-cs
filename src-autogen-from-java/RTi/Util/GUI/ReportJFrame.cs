using System;
using System.Collections.Generic;
using System.Text;

// ReportJFrame - component to display a text report in a JTextArea object

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

//-----------------------------------------------------------------------------
// ReportJFrame - component to display a text report in a JTextArea object
//-----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file.
//-----------------------------------------------------------------------------
// History:
// 01 Apr 1998  DLG, RTi                Created initial class decription.
// 24 Feb 1999  CEN, RTi                No longer need paging using Java 1.2.
// 11 Jun 1999  Steven A. Malers, RTi	Clean up code, add finalize, and check
//					for problem displaying large report.
//					Rechecked in previous revisions to get
//					permissions back in place.
// 28 Nov 2000	SAM, RTi		Change "Export" button to "Save", which
//					is more consistent with standard GUI
//					notation.
// 07 Jan 2001	SAM, RTi		Change GUI to GUIUtil.
// 29 Mar 2001	SAM, RTi		Add optional Search button and related
//					features.
// 16 May 2001	SAM, RTi		Change search feature to be on by
//					default.
// 16 Jul 2001	SAM, RTi		If using Windows 95/98/ME, turn paging
//					on by default because the systems cannot
//					handle reports > 64KB.  Otherwise, turn
//					paging off always.  If Windows 95/98/ME,
//					change label on Search to "Search Page".
// 2002-04-05	SAM, RTi		Minor revision to use toVector() for
//					TextArea conversion.
// 2002-09-04	J. Thomas Sapienza, RTi	Dropped a 0 off of both assignments:
//					_page_length = 10000000000;
//					because the compiler was breaking.
// ============================================================================
// 2002-10-24	SAM, RTi		Copy the AWT JReportGUI to this class
//					and implement a Swing version.
//					Change to support general JTextComponent
//					and add ability to display a URL.
//					Remove paging code because Swing does
//					not require and it complicates the
//					logic.
// 2003-03-28	JTS, RTi		Added code for using a JTextArea as 
//					opposed to a JEditorPane if the proper
//					prop is passed in.  JTextAreas can 
//					very easily turn off word wrapping.
// 2003-05-27	JTS, RTi		Made class a window listener to get rid
//					of anonymous inner window adapter class.
// 2003-09-30	SAM, RTi		Enable the title bar and icon as set
//					by the main application.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//-----------------------------------------------------------------------------

namespace RTi.Util.GUI
{


	using URLHelp = RTi.Util.Help.URLHelp;
	using ExportJGUI = RTi.Util.IO.ExportJGUI;
	using IOUtil = RTi.Util.IO.IOUtil;
	using PrintJGUI = RTi.Util.IO.PrintJGUI;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// Display a report in a JTextArea.  See the constructor for more information.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class ReportJFrame extends javax.swing.JFrame implements java.awt.event.ActionListener, javax.swing.event.HyperlinkListener, java.awt.event.WindowListener
	public class ReportJFrame : JFrame, ActionListener, HyperlinkListener, WindowListener
	{

	private JTextField _status_JTextField; // status TextField
	private JTextArea _info_JTextArea; // Report TextArea
	private JEditorPane _info_JEditorPane; // Report TextArea
	private IList<string> _info_Vector; // Contains list of String to display in the _info_TextArea object

	private PropList _prop; // Controls display
	private string _help_key; // Help Keyword

	private SimpleJButton _close_JButton, _help_JButton, _print_JButton, _save_JButton, _search_JButton;

	private int _page_length; // lines to a page
	private int _print_size; // print point size
	private string _title = null; // Title for frame

	/// <summary>
	/// Determines the kind of text component that will be used for displaying results, either
	/// "JTextArea" (for simple black on white text) or "JEditorPane" (for marked-up navigable HTML).
	/// </summary>
	private string __textComponent = "JTextArea";

	/// <summary>
	/// ReportJFrame constructor. </summary>
	/// <param name="info"> Contains list of String to display. </param>
	/// <param name="prop"> PropList object as described in the following table
	/// <table width=80% cellpadding=2 cellspacing=0 border=2>
	/// <tr>
	/// <td>Property</td>        <td>Description</td>     <td>Default</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>DisplayFont</td>
	/// <td>Font used within text area of ReportGUI.</td>
	/// <td>Courier</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>DisplaySize</td>
	/// <td>Font size used within text area of ReportGUI.</td>
	/// <td>11</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>DisplayStyle</td>
	/// <td>Font style used within text area of ReportGUI.</td>
	/// <td>Font.PLAIN</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>HelpKey</td>
	/// <td>Search key for help.</td>
	/// <td>Help button is disabled.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>PageLength</td>
	/// <td>No longer used, paging not necessary in Windows NT, NT200), etc or with
	/// Java1.2.  If a Windows 95/98/ME machine is detected, the page length is set
	/// to 100 regardless of what the property is.</td>
	/// <td>-</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>ParentUIComponent</td>
	/// <td>Component to use for parent, used to determine screen for centering the dialog.</td>
	/// <td>Screen 0</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>PrintSize</td>
	/// <td>Font size used for printing information.</td>
	/// <td>10</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>Search</td>
	/// <td>Indicates whether to enable the search button.</td>
	/// <td>True</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>Title</td>
	/// <td>Title placed at the top of the ReportGUI frame.</td>
	/// <td>No title (blank).</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>TotalHeight</td>
	/// <td>Height of ReportGUI, in pixels.</td>
	/// <td>550</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>TotalWidth</td>
	/// <td>Width of ReportGUI, in pixels.</td>
	/// <td>600</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>URL</td>
	/// <td>If specified, display the page using the URL, rather than the list of String.</td>
	/// <td>Use list of String.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>DisplayTextComponent</td>
	/// <td>If specified, determines the kind of Text Component to use for displaying
	/// the report data.  Possible options are "JTextArea" and "JEditorPane".  The
	/// difference is:<br>
	/// <ul>
	/// <li><b>JTextArea</b> - This text component turns off line wrapping, but cannot
	/// display HTML</li>
	/// <li><b>JEditorPane</b> - This text component cannot turn off line wrapping, 
	/// but it can display HTML</li>
	/// </ul>
	/// <td>JTextArea</td>
	/// </tr>
	/// 
	/// </table> </param>
	public ReportJFrame(IList<string> info, PropList prop)
	{
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		_info_Vector = info;
		_prop = prop;

		setGUI();
	}

	/// <summary>
	/// Responds to components that generate action events. </summary>
	/// <param name="evt"> ActionEvent object </param>
	public virtual void actionPerformed(ActionEvent evt)
	{
		object o = evt.getSource();
		if (o == _close_JButton)
		{
			o = null;
			close_clicked();
		}
		else if (o == _help_JButton)
		{
			o = null;
			URLHelp.showHelpForKey(_help_key);
		}
		else if (o == _print_JButton)
		{
			o = null;
			PrintJGUI.print(this, _info_Vector, null, _print_size);
		}
		else if (o == _save_JButton)
		{
			o = null;
			ExportJGUI.export(this, _info_Vector);
		}
		else if (o == _search_JButton)
		{
			o = null;
			if (string.ReferenceEquals(_title, null))
			{
				if (__textComponent.Equals("JTextArea"))
				{
					new SearchJDialog(this, _info_JTextArea, null);
				}
				else if (__textComponent.Equals("JEditorPane"))
				{
					new SearchJDialog(this,_info_JEditorPane,null);
				}
			}
			else
			{
				if (__textComponent.Equals("JTextArea"))
				{
					new SearchJDialog(this, _info_JTextArea, "Search " + _title);
				}
				else if (__textComponent.Equals("JEditorPane"))
				{
					new SearchJDialog(this, _info_JEditorPane, "Search " + _title);
				}
			}
		}
	}

	/// <summary>
	/// Responsible for closing the component.
	/// </summary>
	private void close_clicked()
	{
		setVisible(false);
		// If the soft close property is true, then just set hidden
		string prop_val = _prop.getValue("Close");
		if ((string.ReferenceEquals(prop_val, null)) || ((!string.ReferenceEquals(prop_val, null)) && !prop_val.Equals("soft", StringComparison.OrdinalIgnoreCase)))
		{
			  dispose();
		}
	}

	/// <summary>
	/// Add the contents of the formatted Vector to the JTextArea object starting from
	/// the specified line number and ending with the specified line number.
	/// </summary>
	private void displayContents()
	{
		_status_JTextField.setText("Displaying Report...");
		setGUICursor(Cursor.WAIT_CURSOR);

		string prop_value = _prop.getValue("URL");
		if (__textComponent.Equals("JEditorPane"))
		{
		   if (!string.ReferenceEquals(prop_value, null))
		   {
				// Try to set text in the HTML viewer using the URL.
				try
				{
					_info_JEditorPane.setPage(prop_value);
					// Force the position to be at the top...
					_info_JEditorPane.setCaretPosition(0);
					_info_JEditorPane.addHyperlinkListener(this);
					_status_JTextField.setText("Ready");
				}
				catch (Exception)
				{
					_status_JTextField.setText("Unable to display \"" + prop_value + "\"");
				}
		   }
			else
			{
				_status_JTextField.setText("Unable to display - no URL provided");
			}
		}
		else
		{
			// Trying to view using the text area...
			bool status_set = false; // Indicate whether the status message has been set - to get most appropriate message
			if ((_info_Vector == null) && (!string.ReferenceEquals(prop_value, null)))
			{
				// Read the text into a Vector...
				if (!IOUtil.fileExists(prop_value))
				{
					_status_JTextField.setText("Unable to display - file does not exist:  " + prop_value);
					status_set = true;
				}
				else
				{
					try
					{
						_info_Vector = IOUtil.fileToStringList(prop_value);
					}
					catch (IOException)
					{
						_info_Vector = null;
						_status_JTextField.setText("Unable to display - no URL provided");
						status_set = true;
					}
				}
			}

			if (_info_Vector != null)
			{

				StringBuilder contents = new StringBuilder();
				string newLine = System.getProperty("line.separator");
				int from = 0;
				int to = _info_Vector.Count;
				int size = _info_Vector.Count;

				// Set the JTextArea
				if (Message.isDebugOn)
				{
					string routine = "ReportJFrame.displayContents";
					Message.printDebug(1, routine, "Text report is " + size + " lines.");
				}
				for (int i = from; i < to; i++)
				{
					contents.Append((string)_info_Vector[i] + newLine);
				}
				if (__textComponent.Equals("JTextArea"))
				{
					_info_JTextArea.setText(contents.ToString());
					_info_JTextArea.setCaretPosition(0);
				}
				else if (__textComponent.Equals("JEditorPane"))
				{
					_info_JEditorPane.setContentType("text/html");
					_info_JEditorPane.setText(contents.ToString());
					// Force the position to be at the top...
					_info_JEditorPane.setCaretPosition(0);
				}
				_status_JTextField.setText("Ready");
			}

			else if (!status_set)
			{
				_status_JTextField.setText("No text to display");
			}
		}

		setGUICursor(Cursor.DEFAULT_CURSOR);
	}

	/// <summary>
	/// Finalize before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~ReportJFrame()
	{
		_status_JTextField = null;
		_info_JEditorPane = null;
		_info_JTextArea = null;
		_info_Vector = null;

		_prop = null;
		_help_key = null;
		_title = null;

		_save_JButton = null;
		_search_JButton = null;
		_help_JButton = null;
		_print_JButton = null;
		_close_JButton = null;

//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Handle hyperlink events, if a URL is being displayed.
	/// </summary>
	public virtual void hyperlinkUpdate(HyperlinkEvent e)
	{
		if (e.getEventType() != HyperlinkEvent.EventType.ACTIVATED)
		{
			return;
		}
		if (!__textComponent.Equals("JEditorPane"))
		{
			return;
		}
		try
		{
			_info_JEditorPane.setPage(e.getURL());
			// Force the position to be at the top...
			_info_JEditorPane.setCaretPosition(0);
			_status_JTextField.setText("Ready");
		}
		catch (Exception)
		{
			_status_JTextField.setText("Unable to display \"" + e.getURL() + "\"");
		}
	}

	/// <summary>
	/// Instantiates and arranges the GUI components.
	/// </summary>
	private void setGUI()
	{
		int height, width, displayStyle, displaySize;
		string displayFont, propValue;

		/// <summary>
		/// This anonymous inner class extends WindowAdapter and overrides
		/// the no-ops window closing event.
		/// </summary>
		addWindowListener(this);
		// Objects used throughout the GUI layout..

		Insets insetsTLBR = new Insets(7,7,7,7);
		GridBagLayout gbl = new GridBagLayout();

		// If the property list is null, allocate one here so we
		// don't have to constantly check for null...
		if (_prop == null)
		{
			_prop = new PropList("Default");
		}

		// No check needed on these as null is the default value
		_help_key = _prop.getValue("HelpKey");
		_title = _prop.getValue("Title");

		// Check the non-null values so a default is applied if the property 'key' does not exist

		// Determine the width
		propValue = _prop.getValue("TotalWidth");
		if (string.ReferenceEquals(propValue, null))
		{
			width = 600;
		}
		else
		{
			width = StringUtil.atoi(propValue);
		}

		// Determine the height
		propValue = _prop.getValue("TotalHeight");
		if (string.ReferenceEquals(propValue, null))
		{
			height = 550;
		}
		else
		{
			height = StringUtil.atoi(propValue);
		}

		// Determine the Font type
		propValue = _prop.getValue("DisplayFont");
		if (string.ReferenceEquals(propValue, null))
		{
			displayFont = "Courier";
		}
		else
		{
			displayFont = propValue;
		}

		// Determine the Font style
		propValue = _prop.getValue("DisplayStyle");
		if (string.ReferenceEquals(propValue, null))
		{
			displayStyle = Font.PLAIN;
		}
		else
		{
			displayStyle = StringUtil.atoi(propValue);
		}

		// Determine the Font size
		propValue = _prop.getValue("DisplaySize");
		if (string.ReferenceEquals(propValue, null))
		{
			displaySize = 11;
		}
		else
		{
			displaySize = StringUtil.atoi(propValue);
		}

		// Determine the print size in number of lines
		propValue = _prop.getValue("PrintSize");
		if (string.ReferenceEquals(propValue, null))
		{
			_print_size = 10;
		}
		else
		{
			_print_size = StringUtil.atoi(propValue);
		}

		propValue = _prop.getValue("DisplayTextComponent");
		if (!string.ReferenceEquals(propValue, null))
		{
			if (propValue.Equals("JTextArea", StringComparison.OrdinalIgnoreCase))
			{
				__textComponent = "JTextArea";
			}
			else if (propValue.Equals("JEditorPane", StringComparison.OrdinalIgnoreCase))
			{
				__textComponent = "JEditorPane";
			}
			else
			{
				// default to JTextArea if any other value is provided
				__textComponent = "JTextArea";
			}
			if (Message.isDebugOn)
			{
				Message.printDebug(2, "ReportJFrame.setGUI", "Setting text display area to be of type:\"" + __textComponent + "\"");
			}
		}

		// Determine the page length in number of lines. NOTE: This was
		// implemented to manage limitations of displayable memory in the
		// the java.awt.TextArea in Windows 95.  If a Windows 95 variant, then
		// if the value page length is not set, set it to 100.  If not a
		// Windows 95 variant (e.g., NT), then set the value to a large number.
		propValue = _prop.getValue("PageLength");
		string os_name = System.getProperty("os.name");
		if (string.ReferenceEquals(propValue, null))
		{
			//_page_length = 100;
			// TODO SAM (2001-06-08) - Evaluate need.
			// Make this large so that paging is off
			// by default.  However, if a Windows 95/98/ME machine, set to
			// 100 because these machines cannot handle large reports...
			if (os_name.Equals("Windows 95", StringComparison.OrdinalIgnoreCase))
			{
				_page_length = 100;
			}
			else
			{
				_page_length = 1000000000;
			}
		}
		else
		{
			_page_length = StringUtil.atoi(propValue);
			if (!os_name.Equals("Windows 95", StringComparison.OrdinalIgnoreCase))
			{
				// Set to a large number to disable the page length for NT machines...
				_page_length = 1000000000;
			}
			else
			{
				// Limit to reasonable value...
				if (_page_length > 200)
				{
					_page_length = 100;
				}
			}
		}
		os_name = null;

		// Center Panel
		JPanel center_JPanel = new JPanel();
		center_JPanel.setLayout(gbl);
		getContentPane().add("Center", center_JPanel);

		if (__textComponent.Equals("JTextArea"))
		{
			_info_JTextArea = new JTextArea();
			_info_JTextArea.setEditable(false);
			_info_JTextArea.setLineWrap(false);
			_info_JTextArea.setFont(new Font(displayFont, displayStyle, displaySize));
			JScrollPane info_JScrollPane = new JScrollPane(_info_JTextArea);
			JGUIUtil.addComponent(center_JPanel, info_JScrollPane, 0, 0, 1, 1, 1, 1, insetsTLBR, GridBagConstraints.BOTH, GridBagConstraints.WEST);
		}
		else if (__textComponent.Equals("JEditorPane"))
		{
			_info_JEditorPane = new JEditorPane();
			_info_JEditorPane.setFont(new Font(displayFont, displayStyle, displaySize));
			_info_JEditorPane.setEditable(false);
			JScrollPane info_JScrollPane = new JScrollPane(_info_JEditorPane);
			JGUIUtil.addComponent(center_JPanel, info_JScrollPane, 0, 0, 1, 1, 1, 1, insetsTLBR, GridBagConstraints.BOTH, GridBagConstraints.WEST);
		}

		// Bottom Panel
		JPanel bottom_JPanel = new JPanel();
		bottom_JPanel.setLayout(new BorderLayout());
		getContentPane().add("South", bottom_JPanel);

		// Bottom: Center Panel
		JPanel bottomC_JPanel = new JPanel();
		bottomC_JPanel.setLayout(new FlowLayout(FlowLayout.CENTER));
		bottom_JPanel.add("Center", bottomC_JPanel);

		propValue = _prop.getValue("Search");
		if ((string.ReferenceEquals(propValue, null)) || propValue.Equals("true", StringComparison.OrdinalIgnoreCase))
		{
			_search_JButton = new SimpleJButton("Search", this);
			bottomC_JPanel.add(_search_JButton);
		}
		_print_JButton = new SimpleJButton("Print", this);
		bottomC_JPanel.add(_print_JButton);
		_save_JButton = new SimpleJButton("Save", this);
		bottomC_JPanel.add(_save_JButton);
		_close_JButton = new SimpleJButton("Close", this);
		bottomC_JPanel.add(_close_JButton);

		if (!string.ReferenceEquals(_help_key, null))
		{
			_help_JButton = new SimpleJButton("Help", this);
		}

		// Bottom: South Panel
		JPanel bottomS_JPanel = new JPanel();
		bottomS_JPanel.setLayout(gbl);
		bottom_JPanel.add("South", bottomS_JPanel);

		_status_JTextField = new JTextField();
		_status_JTextField.setEditable(false);
		JGUIUtil.addComponent(bottomS_JPanel, _status_JTextField, 0, 0, 1, 1, 1, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);

		// Frame settings
		if (!string.ReferenceEquals(_title, null))
		{
			if ((string.ReferenceEquals(JGUIUtil.getAppNameForWindows(), null)) || JGUIUtil.getAppNameForWindows().Equals(""))
			{
				setTitle(_title);
			}
			else
			{
				setTitle(JGUIUtil.getAppNameForWindows() + " - " + _title);
			}
		}
		pack();
		setSize(width, height);
		// Get the UI component to determine screen to display on - needed for multiple monitors
		object uiComponentO = _prop.getContents("ParentUIComponent");
		Component parentUIComponent = null;
		if ((uiComponentO != null) && (uiComponentO is Component))
		{
			parentUIComponent = (Component)uiComponentO;
		}
		JGUIUtil.center(this, parentUIComponent);

		displayContents();

		setVisible(true);
	}

	/// <summary>
	/// Sets the Cursor for all the GUI components </summary>
	/// <param name="flag"> Cursor type (e.g, Cursor.WAIT_CURSOR etc..) </param>
	private void setGUICursor(int flag)
	{
		setCursor(new Cursor(flag));
		if (__textComponent.Equals("JTextArea"))
		{
			_info_JTextArea.setCursor(new Cursor(flag));
		}
		else if (__textComponent.Equals("JEditorPane"))
		{
			_info_JEditorPane.setCursor(new Cursor(flag));
		}
		_status_JTextField.setCursor(new Cursor(flag));
	}

	public virtual void windowActivated(WindowEvent e)
	{
	}
	public virtual void windowClosed(WindowEvent e)
	{
	}
	public virtual void windowClosing(WindowEvent e)
	{
		close_clicked();
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

	}

}