using System.Collections.Generic;

// DataTable_JFrame - frame for displaying a data table in JWorksheet format

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
// DataTable_JFrame - Frame for displaying a data table in JWorksheet
//	format.
// ----------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
// ----------------------------------------------------------------------------
// History:
//
// 2003-08-21	J. Thomas Sapienza, RTi	Initial version.
// 2004-01-08	Steven A. Malers, RTi	Set the icon and title from JGUIUtil.
// 2004-07-29	JTS, RTi		In-memory DataTables can now be passed
//					in, instead of just files containing
//					data tables.
// 2005-04-26	JTS, RTi		Added all data members to finalize().
// ----------------------------------------------------------------------------

namespace RTi.Util.Table
{


	using JGUIUtil = RTi.Util.GUI.JGUIUtil;

	/// <summary>
	/// This class is the frame in which a tabbed pane displays multiple DataTable data, each in a JWorksheet is displayed.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class DataTableList_JFrame extends javax.swing.JFrame implements javax.swing.event.ChangeListener
	public class DataTableList_JFrame : JFrame, ChangeListener
	{

	/// <summary>
	/// The data tables that were passed in.
	/// </summary>
	private IList<DataTable> __tableList = null;

	/// <summary>
	/// Tab labels for each table.
	/// </summary>
	private string[] __tabLabels = null;

	/// <summary>
	/// The panel containing the worksheet that will be displayed in the frame.
	/// </summary>
	private IList<DataTable_JPanel> __dataTablePanelList = new List<DataTable_JPanel>();

	/// <summary>
	/// Message bar text fields.
	/// </summary>
	private JTextField __messageJTextField;
	private JTextField __statusJTextField;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="title"> the title to put on the frame. </param>
	/// <exception cref="Exception"> if table is null. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public DataTableList_JFrame(String title, String [] tabLabels, java.util.List<DataTable> tableList) throws Exception
	public DataTableList_JFrame(string title, string[] tabLabels, IList<DataTable> tableList)
	{
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		if (string.ReferenceEquals(title, null))
		{
			if ((string.ReferenceEquals(JGUIUtil.getAppNameForWindows(), null)) || JGUIUtil.getAppNameForWindows().Equals(""))
			{
				setTitle("Table");
			}
			else
			{
				setTitle(JGUIUtil.getAppNameForWindows() + " - Table");
			}
		}
		else
		{
			if ((string.ReferenceEquals(JGUIUtil.getAppNameForWindows(), null)) || JGUIUtil.getAppNameForWindows().Equals(""))
			{
				setTitle(title);
			}
			else
			{
				setTitle(JGUIUtil.getAppNameForWindows() + " - " + title);
			}
		}
		__tableList = tableList;
		__tabLabels = tabLabels;

		setupGUI();
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="title"> the title to put on the frame. </param>
	/// <param name="filename"> the name of the file to be read and displayed in the worksheet. </param>
	/// <exception cref="Exception"> if filename is null. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public DataTableList_JFrame(String title, String filename) throws Exception
	public DataTableList_JFrame(string title, string filename)
	{
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		if (string.ReferenceEquals(title, null))
		{
			if ((string.ReferenceEquals(JGUIUtil.getAppNameForWindows(), null)) || JGUIUtil.getAppNameForWindows().Equals(""))
			{
				setTitle("Table");
			}
			else
			{
				setTitle(JGUIUtil.getAppNameForWindows() + " - Table");
			}
		}
		else
		{
			if ((string.ReferenceEquals(JGUIUtil.getAppNameForWindows(), null)) || JGUIUtil.getAppNameForWindows().Equals(""))
			{
				setTitle(title);
			}
			else
			{
				setTitle(JGUIUtil.getAppNameForWindows() + " - " + title);
			}
		}

		setupGUI();
	}

	/// <summary>
	/// Sets the status bar's message and status text fields. </summary>
	/// <param name="message"> the value to put into the message text field. </param>
	/// <param name="status"> the value to put into the status text field. </param>
	public virtual void setMessageStatus(string message, string status)
	{
		if ((!string.ReferenceEquals(message, null)) && (__messageJTextField != null))
		{
			__messageJTextField.setText(message);
		}
		if ((!string.ReferenceEquals(status, null)) && (__statusJTextField != null))
		{
			__statusJTextField.setText(status);
		}
	}

	/// <summary>
	/// Sets up the GUI.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void setupGUI() throws Exception
	private void setupGUI()
	{
		JPanel mainPanel = new JPanel();
		mainPanel.setLayout(new GridBagLayout());
		getContentPane().add(mainPanel);
		// Add a tabbed pane and within that tabs for each data table
		JTabbedPane mainTabbedPane = new JTabbedPane();
		mainTabbedPane.addChangeListener(this);
		JGUIUtil.addComponent(mainPanel, mainTabbedPane, 0, 0, 1, 1, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.CENTER);
		//mainPanel.add(mainTabbedPane);
		int iTab = -1;
		foreach (DataTable table in __tableList)
		{
			++iTab;
			DataTable_JPanel panel = new DataTable_JPanel(this, table);
			__dataTablePanelList.Add(panel);
			// TODO SAM 2016-02-27 Would be nice here to set tool tips on columns to help understand content
			// - could pass into constructor
			mainTabbedPane.addTab(__tabLabels[iTab],panel);
		}

		JPanel statusBar = new JPanel();
		statusBar.setLayout(new GridBagLayout());

		__messageJTextField = new JTextField(20);
		__messageJTextField.setEditable(false);
		__statusJTextField = new JTextField(10);
		__statusJTextField.setEditable(false);

		JGUIUtil.addComponent(statusBar, __messageJTextField, 0, 0, 1, 1, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.WEST);
		JGUIUtil.addComponent(statusBar, __statusJTextField, 1, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		getContentPane().add("South", statusBar);

		setSize(600, 400);
		JGUIUtil.center(this);

		tabClicked(__dataTablePanelList[0]);
		setVisible(true);

		foreach (DataTable_JPanel p in __dataTablePanelList)
		{
			p.setWorksheetColumnWidths();
		}
	}

	/// <summary>
	/// Event handler for tab selection.
	/// </summary>
	public virtual void stateChanged(ChangeEvent e)
	{
		JTabbedPane sourceTabbedPane = (JTabbedPane)e.getSource();
		int index = sourceTabbedPane.getSelectedIndex();
		tabClicked(__dataTablePanelList[index]);
	}

	/// <summary>
	/// Call when a tab is clicked.
	/// </summary>
	private void tabClicked(DataTable_JPanel panel)
	{
		int countRows = panel.getWorksheetRowCount();
		int countCols = panel.getWorksheetColumnCount();
		setMessageStatus("Displaying " + countRows + " rows, " + countCols + " columns.", "Ready");
	}

	}

}