using System;
using System.Collections.Generic;

// DataStores_JPanel - UI JPanel to display list of DataStores

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
	using JScrollWorksheet = RTi.Util.GUI.JScrollWorksheet;
	using JWorksheet = RTi.Util.GUI.JWorksheet;

	using PropList = RTi.Util.IO.PropList;

	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// Panel to contain the JWorksheet that displays data stores data.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class DataStores_JPanel extends javax.swing.JPanel
	public class DataStores_JPanel : JPanel
	{

	/// <summary>
	/// The list of data stores to display in the worksheet.
	/// </summary>
	private IList<DataStore> __dataStoreList = null;

	/// <summary>
	/// The parent frame containing this panel.
	/// </summary>
	private DataStores_JFrame __parent = null;

	/// <summary>
	/// Column widths for the worksheet's fields.
	/// </summary>
	private int[] __widths;

	/// <summary>
	/// The worksheet to display the data.
	/// </summary>
	private JWorksheet __worksheet = null;

	/// <summary>
	/// Properties for how the worksheet should display.
	/// </summary>
	private PropList __props;

	/// <summary>
	/// Constructor.  This sets up the worksheet with a default set of properties:<br>
	/// <ul>
	/// <li>JWorksheet.ShowPopupMenu=true</li>
	/// <li>JWorksheet.SelectionMode=SingleRowSelection</li>
	/// <li>JWorksheet.AllowCopy=true</li>
	/// </ul>
	/// To display with other properties, use the other constructor. </summary>
	/// <param name="parent"> the JFrame in which this panel is displayed. </param>
	/// <param name="dataStoreList"> the list of data stores to display in the panel. </param>
	/// <exception cref="NullPointerException"> if any of the parameters are null. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public DataStores_JPanel(DataStores_JFrame parent, java.util.List<DataStore> dataStoreList) throws Exception
	public DataStores_JPanel(DataStores_JFrame parent, IList<DataStore> dataStoreList)
	{
		if (parent == null || dataStoreList == null)
		{
			throw new System.NullReferenceException();
		}

		__parent = parent;
		__dataStoreList = dataStoreList;

		__props = new PropList("DataTable_JPanel.JWorksheet");
		__props.add("JWorksheet.ShowPopupMenu=true");
		__props.add("JWorksheet.SelectionMode=ExcelSelection");
		__props.add("JWorksheet.AllowCopy=true");

		setupGUI();
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="parent"> the JFrame in which this panel is displayed. </param>
	/// <param name="table"> the table to display in this panel. </param>
	/// <param name="props"> the Properties to use to define the worksheet's characteristics. </param>
	/// <exception cref="NullPointerException"> if any of the parameters are null. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public DataStores_JPanel(DataStores_JFrame parent, java.util.List<DataStore> dataStoreList, RTi.Util.IO.PropList props) throws Exception
	public DataStores_JPanel(DataStores_JFrame parent, IList<DataStore> dataStoreList, PropList props)
	{
		if (parent == null || dataStoreList == null)
		{
			throw new System.NullReferenceException();
		}
		if (props == null)
		{
			props = new PropList("DataTable_JPanel.JWorksheet");
			props.add("JWorksheet.ShowPopupMenu=true");
			props.add("JWorksheet.SelectionMode=ExcelSelection");
			props.add("JWorksheet.AllowCopy=true");
		}

		__parent = parent;
		__dataStoreList = dataStoreList;
		__props = props;

		setupGUI();
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="parent"> the JFrame in which this panel is displayed. </param>
	/// <param name="filename"> the name of the file from which to read worksheet data. </param>
	/// <param name="props"> the Properties to use to define the worksheet's characteristics. </param>
	/// <exception cref="NullPointerException"> if any of the parameters are null. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public DataStores_JPanel(DataStores_JFrame parent, String filename, RTi.Util.IO.PropList props) throws Exception
	public DataStores_JPanel(DataStores_JFrame parent, string filename, PropList props)
	{
		if (parent == null || string.ReferenceEquals(filename, null) || props == null)
		{
			throw new System.NullReferenceException();
		}

		__parent = parent;
		__props = props;

		setupGUI();
	}

	/// <summary>
	/// Returns the number of columns in the worksheet. </summary>
	/// <returns> the number of columns in the worksheet. </returns>
	public virtual int getWorksheetColumnCount()
	{
		if (__worksheet == null)
		{
			return 0;
		}
		return __worksheet.getColumnCount();
	}

	/// <summary>
	/// Returns the number of rows in the worksheet. </summary>
	/// <returns> the number of rows in the worksheet. </returns>
	public virtual int getWorksheetRowCount()
	{
		if (__worksheet == null)
		{
			return 0;
		}
		return __worksheet.getRowCount();
	}

	/// <summary>
	/// Sets up the GUI.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void setupGUI() throws Exception
	private void setupGUI()
	{
		setLayout(new GridBagLayout());
		string routine = "DataTable_JPanel.setupGUI";

		JScrollWorksheet jsw = null;
		try
		{
			DataStores_TableModel tm = new DataStores_TableModel(__dataStoreList);
			DataStores_CellRenderer cr = new DataStores_CellRenderer(tm);

			jsw = new JScrollWorksheet(cr, tm, __props);
			__worksheet = jsw.getJWorksheet();
			__widths = cr.getColumnWidths();
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, e);
			jsw = new JScrollWorksheet(0, 0, __props);
			__worksheet = jsw.getJWorksheet();
		}
		__worksheet.setPreferredScrollableViewportSize(null);
		__worksheet.setHourglassJFrame(__parent);
		//__worksheet.addMouseListener(this);	
		//__worksheet.addKeyListener(this);

		JGUIUtil.addComponent(this, jsw, 0, 0, 1, 1, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.CENTER);
	}

	/// <summary>
	/// Sets the worksheet's column widths.  This should be called after the frame
	/// in which the panel is found has called setVisible(true).
	/// </summary>
	public virtual void setWorksheetColumnWidths()
	{
		if (__worksheet != null)
		{
			__worksheet.calculateColumnWidths();
		}
		if (__worksheet != null && __widths != null)
		{
			__worksheet.setColumnWidths(__widths);
		}
	}

	}

}