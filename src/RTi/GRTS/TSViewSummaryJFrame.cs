using System;
using System.Collections.Generic;
using System.Text;

// TSViewSummaryJFrame - Summary (text) view of one or more time series

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
// TSViewSummaryJFrame - Summary (text) view of one or more time series
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// Notes:	(1)	This class displays the time series using the property
//			list.  Mixed time steps are allowed.
//------------------------------------------------------------------------------
// History:
// 
// 05 Dec 1998	Steven A. Malers,	Initial version.  Copy OpTableDislayGUI
//		Riverside Technology,	and modify as necessary.
//		inc.
// 12 Oct 2000	SAM, RTi		Enable Print and Save As buttons.
// 19 Feb 2001	SAM, RTi		Change GUI to GUIUtil.
// 04 May 2001	SAM, RTi		Add TSViewTitleString property to set
//					the title of the view window.
//					Enable the search button.
// 17 May 2001	SAM, RTi		Change Save As button to Save As choice
//					to save .txt and DateValue file.  Add
//					finalize().
// 07 Sep 2001	SAM, RTi		Set TextArea buffer to null after set
//					to help garbage collection.
// 2001-11-05	SAM, RTi		Update javadoc.  Make sure variables are
//					set to null when done.
// 2001-12-11	SAM, RTi		Change help key to "TSView.Summary".
// 2002-01-17	SAM, RTi		Change name from TSViewSummaryGUI to
//					TSViewSummaryFrame to allow support for
//					Swing.
// ==================================
// 2002-11-11	SAM, RTi		Copy AWT version and update to Swing.
// 2003-06-04	SAM, RTi		* Final update to Swing based on GR and
//					  TS changes.
//					* Add a JScrollPane around the
//					  JTextArea.
//					* Change the Save As choice to a button.
// 2003-08-21	SAM, RTi		* Change DateValueTS.writeTimeSeries()
//					  to DateValueTS.writeTimeSeriesList().
// 2003-09-30	SAM, RTi		* Use icon/title from the main
//					  application if available.
// 2004-01-04	SAM, RTi		* Fix bug where saving the file was not
//					  using the full path.
//					* Comment the Help button for now -
//					  is not typically enabled.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//------------------------------------------------------------------------------
// EndHeader

namespace RTi.GRTS
{


	using DateValueTS = RTi.TS.DateValueTS;
	using TS = RTi.TS.TS;
	using TSUtil = RTi.TS.TSUtil;
	using JFileChooserFactory = RTi.Util.GUI.JFileChooserFactory;
	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using SimpleFileFilter = RTi.Util.GUI.SimpleFileFilter;
	using SimpleJButton = RTi.Util.GUI.SimpleJButton;
	using SearchJDialog = RTi.Util.GUI.SearchJDialog;
	using IOUtil = RTi.Util.IO.IOUtil;
	using PrintJGUI = RTi.Util.IO.PrintJGUI;
	using PropList = RTi.Util.IO.PropList;
	using URLHelp = RTi.Util.Help.URLHelp;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// The TSViewSummaryJFrame provides a text report summary for a list of time
	/// series.  The report is dependent on the time series interval and data type.  The
	/// TSViewSummaryJFrame is managed by the TSViewJFrame parent.
	/// </summary>
	public class TSViewSummaryJFrame : JFrame, ActionListener, WindowListener
	{

	// Private data...

	/// <summary>
	/// TSViewJFrame parent, which manages all the views collectively.
	/// </summary>
	private TSViewJFrame __tsview_JFrame;
	/// <summary>
	/// List of time series to graph.
	/// </summary>
	private IList<TS> __tslist;
	/// <summary>
	/// Property list.
	/// </summary>
	private PropList __props;

	private SimpleJButton __graph_JButton = null;
	private SimpleJButton __table_JButton = null;
	private SimpleJButton __save_JButton = null;
	private SimpleJButton __close_JButton = null;
	private SimpleJButton __help_JButton = null;
	private SimpleJButton __print_JButton = null;
	private SimpleJButton __search_JButton = null;

	private JTextArea __summary_JTextArea = null;

	private string __summary_font_name = "Courier"; // Default for now
	private int __summary_font_style = Font.PLAIN;
	private int __summary_font_size = 11;

	/// <summary>
	/// Construct a TSViewSummaryJFrame. </summary>
	/// <param name="tsview_gui"> Parent TSViewJFrame. </param>
	/// <param name="tslist"> List of time series to display. </param>
	/// <param name="props"> Properties for display (currently same list passed in to TSViewFrame). </param>
	/// <exception cref="if"> there is an error displaying the time series. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TSViewSummaryJFrame(TSViewJFrame tsview_gui, java.util.List<RTi.TS.TS> tslist, RTi.Util.IO.PropList props) throws Exception
	public TSViewSummaryJFrame(TSViewJFrame tsview_gui, IList<TS> tslist, PropList props) : base("Time Series - Summary View")
	{
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		initialize(tsview_gui, tslist, props);
	}

	/// <summary>
	/// Handle action events (button press, etc.) </summary>
	/// <param name="e"> ActionEvent to handle. </param>
	public virtual void actionPerformed(ActionEvent e)
	{
		object o = e.getSource();
		if (o == __close_JButton)
		{
			// Close the GUI via the parent...
			__tsview_JFrame.closeGUI(TSViewType.SUMMARY);
		}
		else if (o == __help_JButton)
		{
			// Show help...
			URLHelp.showHelpForKey("TSView.Summary");
		}
		else if (o == __graph_JButton)
		{
			// Display a graph...
			__tsview_JFrame.openGUI(TSViewType.GRAPH);
		}
		else if (o == __print_JButton)
		{
			// Print the summary...
			PrintJGUI.printJTextAreaObject(this, null, __summary_JTextArea);
		}
		else if (o == __save_JButton)
		{
			// Save the summary report or the data in the report...
			save();
		}
		else if (o == __search_JButton)
		{
			// Search the text area...
			new SearchJDialog(this, __summary_JTextArea, "Search " + getTitle());
		}
		else if (o == __table_JButton)
		{
			// Display a table...
			__tsview_JFrame.openGUI(TSViewType.TABLE);
		}
	}

	/// <summary>
	/// Clean up for garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~TSViewSummaryJFrame()
	{
		__tsview_JFrame = null;
		__tslist = null;
		__props = null;

		__graph_JButton = null;
		__table_JButton = null;
		__save_JButton = null;
		__close_JButton = null;
		__help_JButton = null;
		__print_JButton = null;
		__search_JButton = null;
		__summary_JTextArea = null;
		__summary_font_name = null;

//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Initialize the data and GUI. </summary>
	/// <param name="tsview_gui"> Parent TSViewJFrame. </param>
	/// <param name="tslist"> List of time series to display. </param>
	/// <param name="props"> Properties for display (currently same list passed in to TSViewJFrame). </param>
	private void initialize(TSViewJFrame tsview_gui, IList<TS> tslist, PropList props)
	{
		__tsview_JFrame = tsview_gui;
		__tslist = tslist;
		__props = props;
		string prop_value = __props.getValue("TSViewTitleString");
		if (string.ReferenceEquals(prop_value, null))
		{
			if ((string.ReferenceEquals(JGUIUtil.getAppNameForWindows(), null)) || JGUIUtil.getAppNameForWindows().Equals(""))
			{
				setTitle("Time Series - Summary");
			}
			else
			{
				setTitle(JGUIUtil.getAppNameForWindows() + " - Time Series - Summary");
			}
		}
		else
		{
			if ((string.ReferenceEquals(JGUIUtil.getAppNameForWindows(), null)) || JGUIUtil.getAppNameForWindows().Equals(""))
			{
				setTitle(prop_value + " - Summary");
			}
			else
			{
				setTitle(JGUIUtil.getAppNameForWindows() + " - " + prop_value + " - Summary");
			}
		}
		openGUI(true);
	}

	/// <summary>
	/// Open the GUI and display the time series summary. </summary>
	/// <param name="mode"> Indicates whether the GUI should be visible at creation. </param>
	private void openGUI(bool mode)
	{
		string routine = this.GetType().Name + ".openGUI";
		// Start a big try block to set up the GUI...
		try
		{

		// Add a listener to catch window manager events...

		addWindowListener(this);

		// Lay out the main window component by component.  We will start with
		// the menubar default components.  Then add each requested component
		// to the menu bar and the interface...

		GridBagLayout gbl = new GridBagLayout();

		Insets insetsTLBR = new Insets(7, 7, 7, 7); // space around text area

		// Add a panel to hold the text area...

		JPanel display_JPanel = new JPanel();
		display_JPanel.setLayout(gbl);
		getContentPane().add(display_JPanel);

		// Get the formatted string for the time series...

		StringBuilder buffer = new StringBuilder();
		string nl = System.getProperty("line.separator");
		try
		{
			IList<string> summary_strings = TSUtil.formatOutput(__tslist,__props);
			if (summary_strings != null)
			{
				int size = summary_strings.Count;
				for (int i = 0; i < size; i++)
				{
					buffer.Append(summary_strings[i] + nl);
				}
			}
		}
		catch (Exception e)
		{
			buffer.Append("Error creating time series summary.");
			Message.printWarning(3, routine, e);
		}
		nl = null;

		__summary_JTextArea = new JTextArea();
		__summary_JTextArea.setBackground(Color.white);
		__summary_JTextArea.setEditable(false);
		__summary_JTextArea.setFont(new Font(__summary_font_name, __summary_font_style, __summary_font_size));
		JScrollPane summary_JScrollPane = new JScrollPane(__summary_JTextArea);
		JGUIUtil.addComponent(display_JPanel, summary_JScrollPane, 0, 0, 1, 1, 1, 1, insetsTLBR, GridBagConstraints.BOTH, GridBagConstraints.WEST);

		// Put the buttons on the bottom of the window...

		JPanel button_JPanel = new JPanel();
		button_JPanel.setLayout(new FlowLayout(FlowLayout.CENTER));

		__graph_JButton = new SimpleJButton("Graph", "TSViewSummaryJFrame.Graph", this);
		string prop_value = __props.getValue("EnableGraph");
		if ((!string.ReferenceEquals(prop_value, null)) && prop_value.Equals("false", StringComparison.OrdinalIgnoreCase))
		{
			__graph_JButton.setEnabled(false);
		}
		button_JPanel.add(__graph_JButton);

		__table_JButton = new SimpleJButton("Table", "TSViewSummaryJFrame.Table", this);
		prop_value = __props.getValue("EnableTable");
		if ((!string.ReferenceEquals(prop_value, null)) && prop_value.Equals("false", StringComparison.OrdinalIgnoreCase))
		{
			__table_JButton.setEnabled(false);
		}
		button_JPanel.add(__table_JButton);

		__search_JButton = new SimpleJButton("Search", "TSViewSummaryJFrame.Search", this);
		button_JPanel.add(__search_JButton);

		__help_JButton = new SimpleJButton("Help", "TSViewSummaryJFrame.Help",this);
		// REVISIT - enable later when better on-line help system is enabled.
		//button_JPanel.add ( __help_JButton );

		__print_JButton = new SimpleJButton("Print", "TSViewSummaryJFrame.Print", this);
		button_JPanel.add(__print_JButton);

		__save_JButton = new SimpleJButton("Save", "TSViewSummaryJFrame.Save", this);
		button_JPanel.add(__save_JButton);

		__close_JButton = new SimpleJButton("Close", "TSViewSummaryJFrame.Close",this);
		button_JPanel.add(__close_JButton);

		getContentPane().add("South", button_JPanel);

		// Get properties specific to the view
		prop_value = __props.getValue("Summary.TotalWidth");
		if (string.ReferenceEquals(prop_value, null))
		{
			prop_value = __props.getValue("TotalWidth");
		}
		pack(); // Before setting size
		int total_width = 0, total_height = 0;
		if (!string.ReferenceEquals(prop_value, null))
		{
			total_width = StringUtil.atoi(prop_value);
		}
		prop_value = __props.getValue("Summary.TotalHeight");
		if (string.ReferenceEquals(prop_value, null))
		{
			prop_value = __props.getValue("TotalHeight");
		}
		if (!string.ReferenceEquals(prop_value, null))
		{
			total_height = StringUtil.atoi(prop_value);
		}
		if ((total_width <= 0) || (total_height <= 0))
		{
			// No property so make a guess...
			setSize(800, 600);
		}
		else
		{
			setSize(total_width, total_height);
		}
		// Get the UI component to determine screen to display on - needed for multiple monitors
		object uiComponentO = __props.getContents("TSViewParentUIComponent");
		Component parentUIComponent = null;
		if ((uiComponentO != null) && (uiComponentO is Component))
		{
			parentUIComponent = (Component)uiComponentO;
		}
		// Center on the UI component rather than the graph, because the graph screen seems to get tied screen 0?
		JGUIUtil.center(this, parentUIComponent);
		// Seems to work best here to get the window size right.
		__summary_JTextArea.setText(buffer.ToString());
		// Set the cursor position to the top
		__summary_JTextArea.setCaretPosition(0);
		setVisible(mode);
		} // end of try
		catch (Exception e)
		{
			Message.printWarning(3, routine, e);
		}
	}

	/// <summary>
	/// Save the graph in standard formats.
	/// </summary>
	private void save()
	{
		string routine = "TSViewSummaryFrame.save";
		string last_directory = JGUIUtil.getLastFileDialogDirectory();
		JFileChooser fc = JFileChooserFactory.createJFileChooser(last_directory);
		fc.setDialogTitle("Save Summary");
		fc.setAcceptAllFileFilterUsed(false);
		SimpleFileFilter dv_sff = new SimpleFileFilter("dv", "DateValue Time Series File");
		fc.addChoosableFileFilter(dv_sff);
		SimpleFileFilter dvtxt_sff = new SimpleFileFilter("txt", "DateValue Time Series File");
		fc.addChoosableFileFilter(dvtxt_sff);
		SimpleFileFilter txt_sff = new SimpleFileFilter("txt", "Text Report");
		fc.addChoosableFileFilter(txt_sff);
		if (fc.showSaveDialog(this) != JFileChooser.APPROVE_OPTION)
		{
			// Canceled...
			return;
		}
		// Else figure out the file format and location and then do the save...
		last_directory = fc.getSelectedFile().getParent();
		string path = fc.getSelectedFile().getPath();
		JGUIUtil.setLastFileDialogDirectory(last_directory);
		if ((fc.getFileFilter() == dv_sff) || (fc.getFileFilter() == dvtxt_sff))
		{
			if (fc.getFileFilter() == dv_sff)
			{
				path = IOUtil.enforceFileExtension(path, "dv");
			}
			else
			{
				path = IOUtil.enforceFileExtension(path, "txt");
			}
			if (!TSUtil.intervalsMatch(__tslist))
			{
				Message.printWarning(1, routine, "Unable to write " + "DateValue time series of different intervals.");
				return;
			}
			try
			{
				__tsview_JFrame.setWaitCursor(true);
				DateValueTS.writeTimeSeriesList(__tslist, path);
				__tsview_JFrame.setWaitCursor(false);
			}
			catch (Exception e)
			{
				Message.printWarning(1, routine, "Error saving DateValue file \"" + path + "\"");
				Message.printWarning(3, routine, e);
				__tsview_JFrame.setWaitCursor(false);
			}
		}
		else if (fc.getFileFilter() == txt_sff)
		{
			path = IOUtil.enforceFileExtension(path, "txt");
			try
			{
				__tsview_JFrame.setWaitCursor(true);
				JGUIUtil.writeFile(__summary_JTextArea, path);
				__tsview_JFrame.setWaitCursor(false);
			}
			catch (Exception e)
			{
				Message.printWarning(1, routine, "Error saving report file \"" + path + "\"");
				Message.printWarning(3, routine, e);
				__tsview_JFrame.setWaitCursor(false);
			}
		}
	}

	/// <summary>
	/// Respond to window closing event. </summary>
	/// <param name="event"> WindowEvent object. </param>
	public virtual void windowClosing(WindowEvent @event)
	{
		__tsview_JFrame.closeGUI(TSViewType.SUMMARY);
		return;
	}

	public virtual void processWindowEvent(WindowEvent e)
	{
		if (e.getID() == WindowEvent.WINDOW_CLOSING)
		{
			base.processWindowEvent(e);
			__tsview_JFrame.closeGUI(TSViewType.SUMMARY);
		}
		else
		{
			base.processWindowEvent(e);
		}
	}

	// WindowListener functions...

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
	public virtual void windowOpened(WindowEvent evt)
	{
		;
	}
	public virtual void windowIconified(WindowEvent evt)
	{
		;
	}

	}

}