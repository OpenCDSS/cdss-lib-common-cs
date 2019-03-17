using System;

// DataTable_JPanel - panel for displaying a worksheet containing data table data.

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
// DataTable_JPanel - panel for displaying a worksheet containing data table
//	data.
// ----------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
// ----------------------------------------------------------------------------
// History:
//
// 2003-08-21	J. Thomas Sapienza, RTi	Initial version.
// 2004-01-22	JTS, RTi		Revised to use JScrollWorksheet
//					for displaying row headers.
// 2004-07-29	JTS, RTi		* In-memory DataTables can now be passed
//					  in, instead of just files containing
//					  data tables.
//					* Fixed bug where some of the 
//					  constructors were not calling
//					  setupGUI().
// 2004-10-13	JTS, RTi		When a the name of the file containing
//					a datatable is passed in, if the file
//					cannot be read properly a message
//					is printed where normally the worksheet
//					would appear.
// 2004-10-22	JTS, RTi		Corrected a bug where tables in 
//					memory (eg, not read from a file)
//					were not being displayed properly.
// 2004-10-28	JTS, RTi		When column widths are being set, if
//					there are no widths in the table model
//					column widths will now be estimated
//					using the header column names.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.Util.Table
{


	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using JScrollWorksheet = RTi.Util.GUI.JScrollWorksheet;
	using JWorksheet = RTi.Util.GUI.JWorksheet;

	using PropList = RTi.Util.IO.PropList;

	using Message = RTi.Util.Message.Message;

	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// Panel to contain the JWorksheet that displays DataTable data.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class DataTable_JPanel extends javax.swing.JPanel
	public class DataTable_JPanel : JPanel
	{

	/// <summary>
	/// The table of data to display in the worksheet.
	/// </summary>
	private DataTable __table = null;

	/// <summary>
	/// The parent frame containing this panel.
	/// </summary>
	private JFrame __parent = null;

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
	/// The name of the file from which to read data for the worksheet.
	/// </summary>
	private string __filename = null;

	/// <summary>
	/// Constructor.  This sets up the worksheet with a default set of properties:<br>
	/// <ul>
	/// <li>JWorksheet.ShowPopupMenu=true</li>
	/// <li>JWorksheet.SelectionMode=SingleRowSelection</li>
	/// <li>JWorksheet.AllowCopy=true</li>
	/// </ul>
	/// To display with other properties, use the other constructor. </summary>
	/// <param name="parent"> the JFrame in which this panel is displayed. </param>
	/// <param name="table"> the table to display in the panel. </param>
	/// <exception cref="NullPointerException"> if any of the parameters are null. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public DataTable_JPanel(javax.swing.JFrame parent, DataTable table) throws Exception
	public DataTable_JPanel(JFrame parent, DataTable table)
	{
		if (parent == null || table == null)
		{
			throw new System.NullReferenceException();
		}

		__parent = parent;
		__filename = null;
		__table = table;

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
//ORIGINAL LINE: public DataTable_JPanel(DataTable_JFrame parent, DataTable table, RTi.Util.IO.PropList props) throws Exception
	public DataTable_JPanel(DataTable_JFrame parent, DataTable table, PropList props)
	{
		if (parent == null || table == null || props == null)
		{
			throw new System.NullReferenceException();
		}

		__parent = parent;
		__filename = null;
		__table = table;
		__props = props;

		setupGUI();
	}

	/// <summary>
	/// Constructor.  This sets up the worksheet with a default set of properties:<br>
	/// <ul>
	/// <li>JWorksheet.ShowPopupMenu=true</li>
	/// <li>JWorksheet.SelectionMode=SingleRowSelection</li>
	/// <li>JWorksheet.AllowCopy=true</li>
	/// </ul>
	/// To display with other properties, use the other constructor. </summary>
	/// <param name="parent"> the JFrame in which this panel is displayed. </param>
	/// <param name="filename"> the name of the file from which to read worksheet data. </param>
	/// <exception cref="NullPointerException"> if any of the parameters are null. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public DataTable_JPanel(DataTable_JFrame parent, String filename) throws Exception
	public DataTable_JPanel(DataTable_JFrame parent, string filename)
	{
		if (parent == null || string.ReferenceEquals(filename, null))
		{
			throw new System.NullReferenceException();
		}

		__parent = parent;
		__filename = filename;

		__props = new PropList("DataTable_JPanel.JWorksheet");
		__props.add("JWorksheet.ShowPopupMenu=true");
		__props.add("JWorksheet.SelectionMode=ExcelSelection");
		__props.add("JWorksheet.AllowCopy=true");

		setupGUI();
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="parent"> the JFrame in which this panel is displayed. </param>
	/// <param name="filename"> the name of the file from which to read worksheet data. </param>
	/// <param name="props"> the Properties to use to define the worksheet's characteristics. </param>
	/// <exception cref="NullPointerException"> if any of the parameters are null. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public DataTable_JPanel(DataTable_JFrame parent, String filename, RTi.Util.IO.PropList props) throws Exception
	public DataTable_JPanel(DataTable_JFrame parent, string filename, PropList props)
	{
		if (parent == null || string.ReferenceEquals(filename, null) || props == null)
		{
			throw new System.NullReferenceException();
		}

		__parent = parent;
		__filename = filename;
		__props = props;

		setupGUI();
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~DataTable_JPanel()
	{
		if (__table != null)
		{
			if (__table is DbaseDataTable)
			{
				((DbaseDataTable)__table).close();
			}
		}
		__table = null;
		__parent = null;
		__widths = null;
		__worksheet = null;
		__props = null;
		__filename = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
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

		bool fileReadOK = false;

		if (string.ReferenceEquals(__filename, null))
		{
			// assume that a table was passed in
			fileReadOK = true;
		}
		else
		{
			if (StringUtil.endsWithIgnoreCase(__filename, "dbf"))
			{
				try
				{
					DbaseDataTable dbaseTable = new DbaseDataTable(__filename, false, true);
					__table = dbaseTable;
					fileReadOK = true;
				}
				catch (Exception e)
				{
					Message.printWarning(2, routine, "Error reading '" + __filename + "'");
					Message.printWarning(2, routine, e);
					fileReadOK = false;
				}
			}
			else
			{
				Message.printStatus(2, routine, "Data table in filename '" + __filename + "' is not supported for reading.");
				throw new Exception("Data table type in filename '" + __filename + "' is not supported for reading.");
			}
		}

		if (!fileReadOK)
		{
			JGUIUtil.addComponent(this, new JLabel("Attributes are not available for this layer."), 0, 0, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.NORTHWEST);
			return;
		}

		JScrollWorksheet jsw = null;
		try
		{
			DataTable_TableModel tm = new DataTable_TableModel(__table);
			DataTable_CellRenderer cr = new DataTable_CellRenderer(tm);

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
			// There are cases where the column widths are very large
			// May need to put the check here to guard against because UI may freeze
			// For now changed ResultSetToDataTableFactory code to handle better at front end
			/*
			for ( int i = 0; i < __widths.length; i++ ) {
				if ( __widths[i] > 5000 ) {
					Message.printStatus(2, "", "Column width [" + i + "] \"" + __table.getFieldName(i) + "\" = " + __widths[i] + " resetting to 5000");
					__widths[i] = 5000;
				}
			}
			*/
			__worksheet.setColumnWidths(__widths);
		}
	}

	}

}