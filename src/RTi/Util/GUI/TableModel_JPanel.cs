using System;

// TableModel_JPanel - this class is a generic JPanel to contain the JWorksheet that displays TableModel data

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


	using PropList = RTi.Util.IO.PropList;

	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// This class is a generic JPanel to contain the JWorksheet that displays
	/// TableModel data.  It primarily supports the TableModel_JFrame class, although
	/// it could be used independently.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class TableModel_JPanel extends javax.swing.JPanel
	public class TableModel_JPanel : JPanel
	{

	private JWorksheet_AbstractRowTableModel __tm = null; // Table model to display
	private JWorksheet_DefaultTableCellRenderer __cr = null; // Cell renderer for table model

	private TableModel_JFrame __parent = null; // Parent JFrame

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
	/// <param name="tm"> the table model to display in the panel. </param>
	/// <param name="cr"> the cell renderer to use for displays. </param>
	/// <exception cref="Exception"> if any error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TableModel_JPanel(TableModel_JFrame parent, JWorksheet_AbstractRowTableModel tm, JWorksheet_DefaultTableCellRenderer cr) throws Exception
	public TableModel_JPanel(TableModel_JFrame parent, JWorksheet_AbstractRowTableModel tm, JWorksheet_DefaultTableCellRenderer cr)
	{
		if (parent == null || tm == null || cr == null)
		{
			throw new System.NullReferenceException();
		}

		__parent = parent;
		__tm = tm;
		__cr = cr;

		__props = new PropList("");
		__props = new PropList("TableModel_JPanel.JWorksheet");
		__props.add("JWorksheet.ShowPopupMenu=true");
		__props.add("JWorksheet.SelectionMode=ExcelSelection");
		__props.add("JWorksheet.AllowCopy=true");

		setupGUI();
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="parent"> the JFrame in which this panel is displayed. </param>
	/// <param name="tm"> the table model to display in the panel. </param>
	/// <param name="cr"> the cell renderer to use for displays. </param>
	/// <param name="props"> the Properties to use to define the worksheet's characteristics. </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TableModel_JPanel(TableModel_JFrame parent, JWorksheet_AbstractRowTableModel tm, JWorksheet_DefaultTableCellRenderer cr, RTi.Util.IO.PropList props) throws Exception
	public TableModel_JPanel(TableModel_JFrame parent, JWorksheet_AbstractRowTableModel tm, JWorksheet_DefaultTableCellRenderer cr, PropList props)
	{
		if (parent == null || tm == null || cr == null)
		{
			throw new System.NullReferenceException();
		}

		__parent = parent;
		__tm = tm;
		__cr = cr;
		if (props == null)
		{
			__props = new PropList("");
		}
		else
		{
			__props = props;
		}

		setupGUI();
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~TableModel_JPanel()
	{
		__tm = null;
		__cr = null;
		__parent = null;
		__widths = null;
		__worksheet = null;
		__props = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
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
		string routine = "TableModel_JPanel.setupGUI";

		JScrollWorksheet jsw = null;
		try
		{
			jsw = new JScrollWorksheet(__cr, __tm, __props);
			__worksheet = jsw.getJWorksheet();
			__widths = __cr.getColumnWidths();
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
		if (__worksheet != null && __widths != null)
		{
			__worksheet.setColumnWidths(__widths);
		}
	}

	}

}