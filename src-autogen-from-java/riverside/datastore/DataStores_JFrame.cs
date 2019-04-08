using System.Collections.Generic;

// DataStores_JFrame - UI class to display a list of DataStores

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

namespace riverside.datastore
{


	using JGUIUtil = RTi.Util.GUI.JGUIUtil;

	/// <summary>
	/// This class is a frame that displays data store information.  Currently data store information cannot be edited.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class DataStores_JFrame extends javax.swing.JFrame
	public class DataStores_JFrame : JFrame
	{

	/// <summary>
	/// The data table that was passed in.
	/// </summary>
	private IList<DataStore> __dataStoreList = null;

	/// <summary>
	/// The panel containing the worksheet that will be displayed in the frame.
	/// </summary>
	private DataStores_JPanel __dataTablePanel = null;

	/// <summary>
	/// Message bar text fields.
	/// </summary>
	private JTextField __messageJTextField, __statusJTextField;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="title"> the title to put on the frame. </param>
	/// <param name="dataStoreList"> the list of data stores to display in the worksheet. </param>
	/// <exception cref="Exception"> if table is null. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public DataStores_JFrame(String title, java.awt.Component parent, java.util.List<DataStore> dataStoreList) throws Exception
	public DataStores_JFrame(string title, Component parent, IList<DataStore> dataStoreList)
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
		__dataStoreList = dataStoreList;

		setupGUI(parent);
	}

	/// <summary>
	/// Sets the status bar's message and status text fields. </summary>
	/// <param name="message"> the value to put into the message text field. </param>
	/// <param name="status"> the value to put into the status text field. </param>
	public virtual void setMessageStatus(string message, string status)
	{
		if (!string.ReferenceEquals(message, null))
		{
			__messageJTextField.setText(message);
		}
		if (!string.ReferenceEquals(status, null))
		{
			__statusJTextField.setText(status);
		}
	}

	/// <summary>
	/// Sets up the GUI.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void setupGUI(java.awt.Component parent) throws Exception
	private void setupGUI(Component parent)
	{
		__dataTablePanel = new DataStores_JPanel(this, __dataStoreList);

		getContentPane().add("Center", __dataTablePanel);

		JPanel statusBar = new JPanel();
		statusBar.setLayout(new GridBagLayout());

		__messageJTextField = new JTextField(20);
		__messageJTextField.setEditable(false);
		__statusJTextField = new JTextField(10);
		__statusJTextField.setEditable(false);

		JGUIUtil.addComponent(statusBar, __messageJTextField, 0, 0, 1, 1, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.WEST);
		JGUIUtil.addComponent(statusBar, __statusJTextField, 1, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		getContentPane().add("South", statusBar);

		setSize(800, 400);
		JGUIUtil.center(this,parent);

		int count = __dataTablePanel.getWorksheetRowCount();
		string plural = "s";
		if (count == 1)
		{
			plural = "";
		}
		int count_col = __dataTablePanel.getWorksheetColumnCount();
		string plural_col = "s";
		if (count_col == 1)
		{
			plural_col = "";
		}

		setMessageStatus("Displaying " + count + " row" + plural + ", " + count_col + " column" + plural_col + ".", "Ready");

		setVisible(true);

		__dataTablePanel.setWorksheetColumnWidths();
	}

	}

}