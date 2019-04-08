using System;
using System.Collections;
using System.Collections.Generic;

// TSPropertiesJFrame - displays properties for a time series

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

namespace RTi.GRTS
{


	using MonthTS = RTi.TS.MonthTS;
	using MonthTSLimits = RTi.TS.MonthTSLimits;
	using TS = RTi.TS.TS;
	using TSUtil = RTi.TS.TSUtil;
	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using SimpleJButton = RTi.Util.GUI.SimpleJButton;
	using PrintJGUI = RTi.Util.IO.PrintJGUI;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using DataTable = RTi.Util.Table.DataTable;
	using DataTable_JPanel = RTi.Util.Table.DataTable_JPanel;
	using TableField = RTi.Util.Table.TableField;
	using TableRecord = RTi.Util.Table.TableRecord;
	using TimeInterval = RTi.Util.Time.TimeInterval;

	/// <summary>
	/// The TSPropertiesJFrame displays properties for a time series, including
	/// information from the TSIdent and also basic statistics from TSLimits.  The
	/// properties are typically shown from a parent JFrame window.
	/// </summary>
	public class TSPropertiesJFrame : JFrame, ActionListener, ChangeListener, WindowListener
	{

	/// <summary>
	/// Time series to display.
	/// </summary>
	private TS __ts;
	/// <summary>
	/// Properties to control output.
	/// </summary>
	private PropList __props;
	/// <summary>
	/// Print button to be enabled only with the History tab.
	/// </summary>
	private SimpleJButton __print_JButton;
	/// <summary>
	/// Tabbed pane to manage panels with properties.
	/// </summary>
	private JTabbedPane __props_JTabbedPane;
	/// <summary>
	/// JTextArea for history tab.
	/// </summary>
	private JTextArea __history_JTextArea;
	/// <summary>
	/// JTextArea for comments tab.
	/// </summary>
	private JTextArea __comments_JTextArea;
	/// <summary>
	/// Panel for time series history.
	/// </summary>
	private JPanel __history_JPanel;
	/// <summary>
	/// Panel for time series comments.
	/// </summary>
	private JPanel __comments_JPanel;

	/// <summary>
	/// Construct a TSPropertiesJFrame. </summary>
	/// <param name="gui"> Parent JFrame.  Currently this is ignored and can be set to null. </param>
	/// <param name="ts"> Time series for which to display properties. </param>
	/// <exception cref="Exception"> if there is an error displaying properties. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TSPropertiesJFrame(javax.swing.JFrame gui, RTi.TS.TS ts, RTi.Util.IO.PropList props) throws Exception
	public TSPropertiesJFrame(JFrame gui, TS ts, PropList props) : base("Time Series Properties")
	{
		__ts = ts;
		if (props == null)
		{
			props = new PropList("");
		}
		__props = props;
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		openGUI(true);
	}

	/// <summary>
	/// Handle action events (button press, etc.) </summary>
	/// <param name="e"> ActionEvent to handle. </param>
	public virtual void actionPerformed(ActionEvent e)
	{
		string command = e.getActionCommand();
		if (command.Equals("Close"))
		{
			JGUIUtil.close(this);
		}
		else if (command.Equals("Print"))
		{
			try
			{
				//PrintJGUI.print ( this, JGUIUtil.toVector(__history_JTextArea), null, 8 );
				if (__props_JTabbedPane.getSelectedComponent() == __comments_JPanel)
				{
					PrintJGUI.printJTextAreaObject(this, null, __comments_JTextArea);
				}
				else if (__props_JTabbedPane.getSelectedComponent() == __history_JPanel)
				{
					PrintJGUI.printJTextAreaObject(this, null, __history_JTextArea);
				}
			}
			catch (Exception ex)
			{
				Message.printWarning(1, "TSPropertiesJFrame.actionPerformed", "Error printing (" + ex + ").");
				Message.printWarning(2, "TSPropertiesJFrame.actionPerformed", ex);
			}
		}
	}

	/// <summary>
	/// Create a data table that contains time series properties. </summary>
	/// <param name="ts"> time series from which to generate a property table. </param>
	/// <returns> a property table </returns>
	private DataTable createPropertyTable(TS ts)
	{
		Dictionary<string, object> properties = ts.getProperties();
		List<string> keyList = new List<string>(properties.Keys);
		// Don't sort because order of properties often has some meaning.  Users can sort displayed table.
		//Collections.sort(keyList);
		// Get the length of the name and values to set the table width.
		// TODO SAM 2011-04-25 Sure would be nice to not have to do this
		int nameLength = 25;
		int valueLength = 25;
		foreach (string key in keyList)
		{
			nameLength = Math.Max(nameLength, key.Length);
			object value = properties[key];
			if (value == null)
			{
				value = "";
			}
			valueLength = Math.Max(valueLength, ("" + value).Length);
		}
		IList<TableField> tableFields = new List<TableField>();
		nameLength = -1;
		valueLength = -1;
		tableFields.Add(new TableField(TableField.DATA_TYPE_STRING,"Property Name",nameLength));
		tableFields.Add(new TableField(TableField.DATA_TYPE_STRING,"Property Value",valueLength));
		DataTable table = new DataTable(tableFields);
		if (properties == null)
		{
			properties = new Hashtable();
		}
		TableRecord rec;
		foreach (string key in keyList)
		{
			rec = new TableRecord();
			rec.addFieldValue(key);
			object value = properties[key];
			if (value == null)
			{
				value = "";
			}
			else if (value is double?)
			{
				double? d = (double?)value;
				if (d.isNaN())
				{
					value = "";
				}
			}
			else if (value is float?)
			{
				float? f = (float?)value;
				if (f.isNaN())
				{
					value = "";
				}
			}
			// TODO SAM 2010-10-08 Should objects be used?
			rec.addFieldValue("" + value); // To force string, no matter the value
			try
			{
				table.addRecord(rec);
			}
			catch (Exception)
			{
				// Should not happen
			}
		}
		return table;
	}

	/// <summary>
	/// Clean up before garbage collection. </summary>
	/// <exception cref="Throwable"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~TSPropertiesJFrame()
	{
		__ts = null;
		__props_JTabbedPane = null;
		__comments_JTextArea = null;
		__history_JTextArea = null;
		__print_JButton = null;
		__history_JPanel = null;
		__comments_JPanel = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Open the properties GUI. </summary>
	/// <param name="mode"> Indicates whether the GUI is visible at creation. </param>
	private void openGUI(bool mode)
	{
		string routine = "TSViewPropertiesJFrame.openGUI";

		// Start a big try block to set up the GUI...
		try
		{

		// Add a listener to catch window manager events...

		addWindowListener(this);
		GridBagLayout gbl = new GridBagLayout();
		Insets insetsTLBR = new Insets(2, 2, 2, 2); // space around text area

		// Font for reports (fixed width)...

		Font report_Font = new Font("Courier", Font.PLAIN, 11);

		// Add a panel to hold the main components...

		JPanel display_JPanel = new JPanel();
		display_JPanel.setLayout(gbl);
		getContentPane().add(display_JPanel);

		__props_JTabbedPane = new JTabbedPane();
		__props_JTabbedPane.addChangeListener(this);
		JGUIUtil.addComponent(display_JPanel, __props_JTabbedPane, 0, 0, 10, 1, 1.0, 1.0, insetsTLBR, GridBagConstraints.BOTH, GridBagConstraints.CENTER);

		//
		// General Tab...
		//

		JPanel general_JPanel = new JPanel();
		general_JPanel.setLayout(gbl);
		__props_JTabbedPane.addTab("General", null, general_JPanel, "General (built-in) properties");

		int y = 0;
		JGUIUtil.addComponent(general_JPanel, new JLabel("Identifier:"), 0, y, 1, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JTextField identifier_JTextField = new JTextField(__ts.getIdentifierString(), 50);
		identifier_JTextField.setEditable(false);
		JGUIUtil.addComponent(general_JPanel, identifier_JTextField, 1, y, 6, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		identifier_JTextField = null;

		JGUIUtil.addComponent(general_JPanel, new JLabel("Identifier (with input):"), 0, ++y, 1, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.EAST);
		// Limit the length of this field...
		JTextField input_JTextField = new JTextField(__ts.getIdentifier().ToString(true), 50);
		input_JTextField.setEditable(false);
		JGUIUtil.addComponent(general_JPanel, input_JTextField, 1, y, 6, 1, 1.0, 0.0, insetsTLBR, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);

		JGUIUtil.addComponent(general_JPanel, new JLabel("Alias:"), 0, ++y, 1, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JTextField alias_JTextField = new JTextField(__ts.getAlias(), 50);
		alias_JTextField.setEditable(false);
		JGUIUtil.addComponent(general_JPanel, alias_JTextField, 1, y, 2, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		alias_JTextField = null;

		JGUIUtil.addComponent(general_JPanel, new JLabel("Sequence (ensemble trace) ID:"), 0, ++y, 1, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JTextField seqnum_JTextField = new JTextField("" + __ts.getSequenceID(), 5);
		seqnum_JTextField.setEditable(false);
		JGUIUtil.addComponent(general_JPanel, seqnum_JTextField, 1, y, 2, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		alias_JTextField = null;

		JGUIUtil.addComponent(general_JPanel, new JLabel("Description:"), 0, ++y, 1, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.EAST);
		// Set a maximum size so this does not get outrageously big...
		JTextField description_JTextField = new JTextField(__ts.getDescription(),50);
		description_JTextField.setEditable(false);
		JGUIUtil.addComponent(general_JPanel, description_JTextField, 1, y, 6, 1, 1.0, 0.0, insetsTLBR, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		description_JTextField = null;

		JGUIUtil.addComponent(general_JPanel, new JLabel("Units (Current):"), 0, ++y, 1, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JTextField units_JTextField = new JTextField(__ts.getDataUnits(), 10);
		units_JTextField.setEditable(false);
		JGUIUtil.addComponent(general_JPanel, units_JTextField, 1, y, 1, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		units_JTextField = null;

		JGUIUtil.addComponent(general_JPanel, new JLabel("Units (Original):"), 0, ++y, 1, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JTextField unitsorig_JTextField = new JTextField(__ts.getDataUnitsOriginal(), 10);
		unitsorig_JTextField.setEditable(false);
		JGUIUtil.addComponent(general_JPanel, unitsorig_JTextField, 1, y, 1, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		unitsorig_JTextField = null;

		JCheckBox isselected_JCheckBox = new JCheckBox("Is Selected", __ts.isSelected());
		isselected_JCheckBox.setEnabled(false);
		JGUIUtil.addComponent(general_JPanel, isselected_JCheckBox, 1, ++y, 1, 1, 1.0, 0.0, insetsTLBR, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		isselected_JCheckBox = null;

		JCheckBox iseditable_JCheckBox = new JCheckBox("Is Editable", __ts.isEditable());
		iseditable_JCheckBox.setEnabled(false);
		JGUIUtil.addComponent(general_JPanel, iseditable_JCheckBox, 1, ++y, 1, 1, 1.0, 0.0, insetsTLBR, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		iseditable_JCheckBox = null;

		JCheckBox isdirty_JCheckBox = new JCheckBox("Is Dirty (data edited without recomputing limits)", __ts.isDirty());
		isdirty_JCheckBox.setEnabled(false);
		JGUIUtil.addComponent(general_JPanel, isdirty_JCheckBox, 1, ++y, 1, 1, 1.0, 0.0, insetsTLBR, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		isdirty_JCheckBox = null;

		// Properties tab...

		JPanel properties_JPanel = new JPanel();
		properties_JPanel.setLayout(gbl);
		__props_JTabbedPane.addTab("Properties", null, properties_JPanel, "Dynamic properties");
		JGUIUtil.addComponent(properties_JPanel, new JScrollPane(new DataTable_JPanel(this, createPropertyTable(__ts))), 0, y, 6, 1, 1.0, 1.0, insetsTLBR, GridBagConstraints.BOTH, GridBagConstraints.CENTER);

		// Comments Tab...

		__comments_JPanel = new JPanel();
		__comments_JPanel.setLayout(gbl);
		__props_JTabbedPane.addTab("Comments", null, __comments_JPanel, "Comments");

		y = 0;
		__comments_JTextArea = new JTextArea(StringUtil.ToString(__ts.getComments(), System.getProperty("line.separator")),5,80);
		__comments_JTextArea.setFont(report_Font);
		__comments_JTextArea.setEditable(false);
		JGUIUtil.addComponent(__comments_JPanel, new JScrollPane(__comments_JTextArea), 0, y, 6, 1, 1.0, 1.0, insetsTLBR, GridBagConstraints.BOTH, GridBagConstraints.CENTER);

		//
		// Period Tab...
		//

		JPanel period_JPanel = new JPanel();
		period_JPanel.setLayout(gbl);
		__props_JTabbedPane.addTab("Period", null, period_JPanel, "Period");

		y = 0;
		JGUIUtil.addComponent(period_JPanel, new JLabel("Current (reflects manipulation):"), 0, y, 1, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JTextField period_JTextField = new JTextField(" " + __ts.getDate1() + " to " + __ts.getDate2(), 30);
		period_JTextField.setEditable(false);
		JGUIUtil.addComponent(period_JPanel, period_JTextField, 1, y, 2, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		period_JTextField = null;

		JGUIUtil.addComponent(period_JPanel, new JLabel("Original (from input):"), 0, ++y, 1, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JTextField origperiod_JTextField = new JTextField(" " + __ts.getDate1Original() + " to " + __ts.getDate2Original(), 30);
		origperiod_JTextField.setEditable(false);
		JGUIUtil.addComponent(period_JPanel, origperiod_JTextField, 1, y, 2, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		origperiod_JTextField = null;

		JGUIUtil.addComponent(period_JPanel, new JLabel("Total Points:"), 0, ++y, 1, 1, 0, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JTextField points_JTextField = new JTextField(" " + __ts.getDataSize());
		points_JTextField.setEditable(false);
		JGUIUtil.addComponent(period_JPanel, points_JTextField, 1, y, 1, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		points_JTextField = null;

		//
		// Limits Tab...
		//

		JPanel limits_JPanel = new JPanel();
		limits_JPanel.setLayout(gbl);
		__props_JTabbedPane.addTab("Limits", null, limits_JPanel, "Limits");

		y = 0;
		JGUIUtil.addComponent(limits_JPanel, new JLabel("Current (reflects manipulation):"), 0, y, 6, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JTextArea limits_JTextArea = null;
		if (__ts.getDataIntervalBase() == TimeInterval.MONTH)
		{
			try
			{
				limits_JTextArea = new JTextArea((new MonthTSLimits((MonthTS)__ts)).ToString(),12,80);
			}
			catch (Exception)
			{
				limits_JTextArea = new JTextArea("No Limits Available",5,80);
			}
		}
		else
		{
			try
			{
				limits_JTextArea = new JTextArea((TSUtil.getDataLimits(__ts, __ts.getDate1(), __ts.getDate2())).ToString(),15,80);
			}
			catch (Exception)
			{
				limits_JTextArea = new JTextArea("No Limits Available",5,80);
			}
		}
		limits_JTextArea.setEditable(false);
		limits_JTextArea.setFont(report_Font);
		JGUIUtil.addComponent(limits_JPanel, new JScrollPane(limits_JTextArea), 0, ++y, 6, 1, 1.0, 1.0, insetsTLBR, GridBagConstraints.BOTH, GridBagConstraints.CENTER);
		limits_JTextArea = null;

		++y;
		JGUIUtil.addComponent(limits_JPanel, new JLabel("Original (from input):"), 0, ++y, 6, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JTextArea origlim_JTextArea = null;
		if (__ts.getDataLimitsOriginal() == null)
		{
			origlim_JTextArea = new JTextArea("No Limits Available");
		}
		else
		{
			origlim_JTextArea = new JTextArea(__ts.getDataLimitsOriginal().ToString(),10,80);
			origlim_JTextArea.setFont(report_Font);
			origlim_JTextArea.setEditable(false);
		}
		origlim_JTextArea.setEditable(false);
		JGUIUtil.addComponent(limits_JPanel, new JScrollPane(origlim_JTextArea), 0, ++y, 6, 1, 1.0, 1.0, insetsTLBR, GridBagConstraints.BOTH, GridBagConstraints.CENTER);

		//
		// History Tab...
		//

		__history_JPanel = new JPanel();
		__history_JPanel.setLayout(gbl);
		__props_JTabbedPane.addTab("History", null, __history_JPanel,"History");
		y = 0;
		__history_JTextArea = new JTextArea(StringUtil.ToString(__ts.getGenesis(),System.getProperty("line.separator")),5,80);
		__history_JTextArea.setFont(report_Font);
		__history_JTextArea.setEditable(false);
		JGUIUtil.addComponent(__history_JPanel, new JScrollPane(__history_JTextArea), 0, y, 7, 1, 1.0, 1.0, insetsTLBR, GridBagConstraints.BOTH, GridBagConstraints.CENTER);

		JGUIUtil.addComponent(__history_JPanel, new JLabel("Read From:"), 0, ++y, 1, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JTextField inputname_JTextField = new JTextField(__ts.getInputName());
		inputname_JTextField.setEditable(false);
		JGUIUtil.addComponent(__history_JPanel, inputname_JTextField, 1, y, 6, 1, 1.0, 0.0, insetsTLBR, GridBagConstraints.HORIZONTAL, GridBagConstraints.CENTER);
		inputname_JTextField = null;

		//
		// Data Flags Tab...
		//

		JPanel dataflags_JPanel = new JPanel();
		dataflags_JPanel.setLayout(gbl);
		__props_JTabbedPane.addTab("Data Flags", dataflags_JPanel);

		y = 0;
		JGUIUtil.addComponent(dataflags_JPanel, new JLabel("Missing Data Value:"), 0, y, 1, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JTextField missing_JTextField = new JTextField(StringUtil.formatString(__ts.getMissing(),"%.4f"), 15);
		missing_JTextField.setEditable(false);
		JGUIUtil.addComponent(dataflags_JPanel, missing_JTextField, 1, y, 1, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		missing_JTextField = null;

		JCheckBox hasdataflags_JCheckBox = new JCheckBox("Has Data Flags", __ts.hasDataFlags());
		hasdataflags_JCheckBox.setEnabled(false);
		JGUIUtil.addComponent(dataflags_JPanel, hasdataflags_JCheckBox, 0, ++y, 2, 1, 1.0, 0.0, insetsTLBR, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		hasdataflags_JCheckBox = null;

		// Put the buttons on the bottom of the window...

		JPanel button_JPanel = new JPanel();
		button_JPanel.setLayout(new FlowLayout(FlowLayout.CENTER));

		button_JPanel.add(new SimpleJButton("Close", "Close",this));
		__print_JButton = new SimpleJButton("Print", "Print", this);
		__print_JButton.setEnabled(false);
		button_JPanel.add(__print_JButton);

		getContentPane().add("South", button_JPanel);
		button_JPanel = null;

		if ((string.ReferenceEquals(JGUIUtil.getAppNameForWindows(), null)) || JGUIUtil.getAppNameForWindows().Equals(""))
		{
			setTitle(__ts.getIdentifier().ToString() + " - Properties");
		}
		else
		{
			setTitle(JGUIUtil.getAppNameForWindows() + " - " + __ts.getIdentifier().ToString() + " - Properties");
		}

		pack();
		// Get the UI component to determine screen to display on - needed for multiple monitors
		object uiComponentO = __props.getContents("TSViewParentUIComponent");
		Component parentUIComponent = null;
		if ((uiComponentO != null) && (uiComponentO is Component))
		{
			parentUIComponent = (Component)uiComponentO;
		}
		JGUIUtil.center(this, parentUIComponent);
		setResizable(false);
		setVisible(mode);
		} // end of try
		catch (Exception e)
		{
			Message.printWarning(2, routine, e);
		}
	}

	/// <summary>
	/// React to tab selections.  Currently all that is done is the Print button is enabled or disabled. </summary>
	/// <param name="e"> the ChangeEvent that happened. </param>
	public virtual void stateChanged(ChangeEvent e)
	{ // Check for null because events are sometimes generated at startup...
		if ((__props_JTabbedPane.getSelectedComponent() == __history_JPanel) || (__props_JTabbedPane.getSelectedComponent() == __comments_JPanel))
		{
			JGUIUtil.setEnabled(__print_JButton, true);
		}
		else
		{
			JGUIUtil.setEnabled(__print_JButton, false);
		}
	}

	// WindowListener functions...

	public virtual void windowActivated(WindowEvent evt)
	{
	}

	public virtual void windowClosed(WindowEvent evt)
	{
	}

	/// <summary>
	/// Close the GUI.
	/// </summary>
	public virtual void windowClosing(WindowEvent @event)
	{
		JGUIUtil.close(this);
	}

	public virtual void windowDeactivated(WindowEvent evt)
	{
	}

	public virtual void windowDeiconified(WindowEvent evt)
	{
	}

	public virtual void windowOpened(WindowEvent evt)
	{
	}

	public virtual void windowIconified(WindowEvent evt)
	{
	}

	}

}