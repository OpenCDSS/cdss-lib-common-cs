using System.Collections.Generic;

// ERDiagram_JFrame - abstract JFrame class for the specific instances of JFrames for ER diagram

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
// ERDiagram_JFrame - the abstract JFrame class off of which all the specific
//	database instances of JFrames are built.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2003-08-27	J. Thomas Sapienza, RTi	Initial changelog.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.DMI
{


	using DMI = RTi.DMI.DMI;
	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using SimpleJComboBox = RTi.Util.GUI.SimpleJComboBox;
	using DataTable = RTi.Util.Table.DataTable;

	/// <summary>
	/// This class is a JFrame inside of which is displayed the ER Diagram for a database.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class ERDiagram_JFrame extends javax.swing.JFrame
	public class ERDiagram_JFrame : JFrame
	{

	/// <summary>
	/// The panel containing the ER diagram.
	/// </summary>
	private ERDiagram_JPanel __panel;

	/// <summary>
	/// The message field that appears at the bottom of the JFrame.
	/// </summary>
	private JTextField __messageField;

	/// <summary>
	/// The status field that appears at the bottom of the JFrame.
	/// </summary>
	private JTextField __statusField;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dmi"> an open and connected dmi that is hooked into the database for which
	/// the ER Diagram will be built. </param>
	/// <param name="tablesTableName"> the name of the table in the database that contains 
	/// a list of all the tables. </param>
	/// <param name="tableNameField"> the name of the field within the above table that contains
	/// the names of the tables. </param>
	/// <param name="erdXField"> the name of the field within the above table that contains the
	/// X position of the tables in the ER Diagram. </param>
	/// <param name="erdYField"> the name of the field within the above table that contains the
	/// Y position of the tables in the ER Diagram. </param>
	/// <param name="pageFormat"> the pageFormat with which the page will be printed. </param>
	public ERDiagram_JFrame(DMI dmi, string tablesTableName, string tableNameField, string erdXField, string erdYField, PageFormat pageFormat) : this(dmi, tablesTableName, tableNameField, erdXField, erdYField, null, pageFormat, false)
	{
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dmi"> an open and connected dmi that is hooked into the database for which
	/// the ER Diagram will be built. </param>
	/// <param name="tablesTableName"> the name of the table in the database that contains 
	/// a list of all the tables. </param>
	/// <param name="tableNameField"> the name of the field within the above table that contains
	/// the names of the tables. </param>
	/// <param name="erdXField"> the name of the field within the above table that contains the
	/// X position of the tables in the ER Diagram. </param>
	/// <param name="erdYField"> the name of the field within the above table that contains the
	/// Y position of the tables in the ER Diagram. </param>
	/// <param name="referenceTables"> a Vector of tables that will be marked as reference
	/// tables. </param>
	/// <param name="pageFormat"> the pageFormat with which the page will be printed. </param>
	public ERDiagram_JFrame(DMI dmi, string tablesTableName, string tableNameField, string erdXField, string erdYField, IList<string> referenceTables, PageFormat pageFormat) : this(dmi, tablesTableName, tableNameField, erdXField, erdYField, referenceTables, pageFormat, false)
	{
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dmi"> an open and connected dmi that is hooked into the database for which
	/// the ER Diagram will be built. </param>
	/// <param name="tablesTableName"> the name of the table in the database that contains 
	/// a list of all the tables. </param>
	/// <param name="tableNameField"> the name of the field within the above table that contains
	/// the names of the tables. </param>
	/// <param name="erdXField"> the name of the field within the above table that contains the
	/// X position of the tables in the ER Diagram. </param>
	/// <param name="erdYField"> the name of the field within the above table that contains the
	/// Y position of the tables in the ER Diagram. </param>
	/// <param name="referenceTables"> a Vector of tables that will be marked as reference
	/// tables. </param>
	/// <param name="pageFormat"> the pageFormat with which the page will be printed. </param>
	public ERDiagram_JFrame(DMI dmi, string tablesTableName, string tableNameField, string erdXField, string erdYField, PageFormat pageFormat, bool debug) : this(dmi, tablesTableName, tableNameField, erdXField, erdYField, null, pageFormat, debug)
	{
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dmi"> an open and connected dmi that is hooked into the database for which
	/// the ER Diagram will be built. </param>
	/// <param name="tablesTableName"> the name of the table in the database that contains 
	/// a list of all the tables. </param>
	/// <param name="tableNameField"> the name of the field within the above table that contains
	/// the names of the tables. </param>
	/// <param name="erdXField"> the name of the field within the above table that contains the
	/// X position of the tables in the ER Diagram. </param>
	/// <param name="erdYField"> the name of the field within the above table that contains the
	/// Y position of the tables in the ER Diagram. </param>
	/// <param name="pageFormat"> the pageFormat with which the page will be printed. </param>
	/// <param name="debug"> whether to turn on debugging options in the popup menu.   </param>
	public ERDiagram_JFrame(DMI dmi, string tablesTableName, string tableNameField, string erdXField, string erdYField, IList<string> referenceTables, PageFormat pageFormat, bool debug) : base(dmi.getDatabaseName())
	{

		__panel = new ERDiagram_JPanel(this, dmi, tablesTableName, tableNameField, erdXField, erdYField, referenceTables, pageFormat, debug);

		setupGUI();

	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dmi"> an open and connected dmi that is hooked into the database for which
	/// the ER Diagram will be built. </param>
	/// <param name="tablesTableName"> the name of the table in the database that contains 
	/// a list of all the tables. </param>
	/// <param name="tableNameField"> the name of the field within the above table that contains
	/// the names of the tables. </param>
	/// <param name="erdXField"> the name of the field within the above table that contains the
	/// X position of the tables in the ER Diagram. </param>
	/// <param name="erdYField"> the name of the field within the above table that contains the
	/// Y position of the tables in the ER Diagram. </param>
	/// <param name="pageFormat"> the pageFormat with which the page will be printed. </param>
	/// <param name="debug"> whether to turn on debugging options in the popup menu.   </param>
	public ERDiagram_JFrame(DMI dmi, DataTable tablesTable, string tableNameField, string erdXField, string erdYField, IList<string> referenceTables, PageFormat pageFormat, bool debug) : base(dmi.getDatabaseName())
	{
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());

		__panel = new ERDiagram_JPanel(this, dmi, tablesTable, tableNameField, erdXField, erdYField, referenceTables, pageFormat, debug);

		setupGUI();
	}

	/// <summary>
	/// Sets up the GUI.
	/// </summary>
	private void setupGUI()
	{
		addWindowListener(__panel);
		getContentPane().add(__panel);

		JPanel bottom = new JPanel();
		bottom.setLayout(new GridBagLayout());
		__statusField = new JTextField(10);
		__messageField = new JTextField(10);
		__statusField.setEditable(false);
		__messageField.setEditable(false);

		__panel.setMessageFields(__messageField, __statusField);

		SimpleJComboBox c = new SimpleJComboBox(true);
		c.add("10%");
		c.add("50%");
		c.add("100%");
		c.add("200%");
		c.select("100%");
		c.addActionListener(__panel.getDevice());
		__panel.getDevice().setScaleComboBox(c);
		getContentPane().add("North", c);

		JGUIUtil.addComponent(bottom, __messageField, 0, 0, 1, 1, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.WEST);
		JGUIUtil.addComponent(bottom, __statusField, 1, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		getContentPane().add("South", bottom);

		__messageField.setText("");
		__statusField.setText("READY");

		setSize(800, 600);
		setVisible(true);
		__panel.setInitialViewportPosition();
		//show();
	}

	// TODO SAM 2015-05-10 Calling this cases a StackOverflowException - call directly above
	/// <summary>
	/// Overrides the default JFrame setVisible(true) -- calls setInitialViewportPosition on the 
	/// ERDiagram_JPanel to position the ER Diagram within its scroll pane.
	/// </summary>
	//public void show() {
	//	setVisible(true);
	//	__panel.setInitialViewportPosition();
	//}

	}

}