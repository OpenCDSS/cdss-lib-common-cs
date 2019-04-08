// TableModel_JFrame - generic Frame for displaying a table model in JWorksheet format

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
// TableModel_JFrame - generic Frame for displaying a table model in JWorksheet
//	format.
// ----------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
// ----------------------------------------------------------------------------
// History:
//
// 2004-10-27	Steven A. Malers, RTi	Copy DataTable_JFrame and modify.
// ----------------------------------------------------------------------------

namespace RTi.Util.GUI
{


	using PropList = RTi.Util.IO.PropList;

	/// <summary>
	/// This class manages and displays data given a TableModel and CellRenderer by
	/// using a TableModel_JPanel.  The JFrame is suitable for general displays where
	/// a table model and renderer are available.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class TableModel_JFrame extends javax.swing.JFrame
	public class TableModel_JFrame : JFrame
	{

	private JWorksheet_AbstractRowTableModel __tm = null; // Table model to display
	private JWorksheet_DefaultTableCellRenderer __cr = null; // Cell renderer for the table model.
	private TableModel_JPanel __tm_JPanel = null; // The panel to hold the worksheet.
	private PropList __worksheet_props = null; // Properties to control the worksheet.

	private JTextField __message_JTextField, __status_JTextField;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="tm"> The table model to display. </param>
	/// <param name="cr"> The cell renderer for the table model. </param>
	/// <param name="frame_props"> Properties to control the frame.
	/// Currently only "Title" can be set. </param>
	/// <param name="worksheet_props"> Properties to control the worksheet.  Pass null for defaults. </param>
	/// <exception cref="Exception"> if table is null. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TableModel_JFrame(JWorksheet_AbstractRowTableModel tm, JWorksheet_DefaultTableCellRenderer cr, RTi.Util.IO.PropList frame_props, RTi.Util.IO.PropList worksheet_props) throws Exception
	public TableModel_JFrame(JWorksheet_AbstractRowTableModel tm, JWorksheet_DefaultTableCellRenderer cr, PropList frame_props, PropList worksheet_props)
	{
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		if (frame_props == null)
		{
			frame_props = new PropList("");
		}
		string title = frame_props.getValue("Title");
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
		__tm = tm;
		__cr = cr;
		// Can be null...
		__worksheet_props = worksheet_props;

		setupGUI();
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~TableModel_JFrame()
	{
		__tm = null;
		__cr = null;
		__tm_JPanel = null;
		__message_JTextField = null;
		__status_JTextField = null;
		__worksheet_props = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Sets the status bar's message and status text fields. </summary>
	/// <param name="message"> the value to put into the message text field. </param>
	/// <param name="status"> the value to put into the status text field. </param>
	public virtual void setMessageStatus(string message, string status)
	{
		if (!string.ReferenceEquals(message, null))
		{
			__message_JTextField.setText(message);
		}
		if (!string.ReferenceEquals(status, null))
		{
			__status_JTextField.setText(status);
		}
	}

	/// <summary>
	/// Sets up the GUI.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void setupGUI() throws Exception
	private void setupGUI()
	{
		if (__worksheet_props != null)
		{
			__tm_JPanel = new TableModel_JPanel(this, __tm, __cr, __worksheet_props);
		}
		else
		{
			// Use defaults...
			__tm_JPanel = new TableModel_JPanel(this, __tm, __cr);
		}

		getContentPane().add("Center", __tm_JPanel);

		JPanel statusBar = new JPanel();
		statusBar.setLayout(new GridBagLayout());

		__message_JTextField = new JTextField(20);
		__message_JTextField.setEditable(false);
		__status_JTextField = new JTextField(10);
		__status_JTextField.setEditable(false);

		JGUIUtil.addComponent(statusBar, __message_JTextField, 0, 0, 1, 1, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.WEST);
		JGUIUtil.addComponent(statusBar, __status_JTextField, 1, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		getContentPane().add("South", statusBar);

		setSize(600, 400);
		JGUIUtil.center(this);

		int count = __tm_JPanel.getWorksheetRowCount();
		string plural = "s";
		if (count == 1)
		{
			plural = "";
		}

		setMessageStatus("Displaying " + count + " record" + plural + ".", "Ready");

		setVisible(true);
		toFront(); // Needed because sometimes gets hidden

		__tm_JPanel.setWorksheetColumnWidths();
	}

	}

}