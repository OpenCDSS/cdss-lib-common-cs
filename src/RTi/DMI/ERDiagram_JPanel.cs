using System;
using System.Collections.Generic;

// ERDiagram_JPanel - JPanel that can be added to frames that displays an ER Diagram

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
// ERDiagram_JPanel - A jpanel that can be added to frames that displays an
//	ER Diagram for a database connection.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2003-07-28	J. Thomas Sapienza, RTI	Initial version.
// 2003-08-11	JTS, RTi		Javadoc'd.
// 2003-08-27	JTS, RTi		Javadocs brought up to data again.
// 2004-01-19	Steven A. Malers, RTi	Change deprecated setDirty(boolean) call
//					to setDirty(boolean).
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.DMI
{



	using GRLimits = RTi.GR.GRLimits;
	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using ResponseJDialog = RTi.Util.GUI.ResponseJDialog;
	using Message = RTi.Util.Message.Message;
	using DataTable = RTi.Util.Table.DataTable;
	using TableRecord = RTi.Util.Table.TableRecord;

	/// <summary>
	/// This class is a JPanel that displays an ER Diagram for a database.  It can be added to any JFrame.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class ERDiagram_JPanel extends javax.swing.JPanel implements java.awt.event.WindowListener
	public class ERDiagram_JPanel : JPanel, WindowListener
	{

	/// <summary>
	/// Whether debugging options have been turned on for the ERDiagram program.  These
	/// options are indicated by additional menu items in the popup menu.
	/// </summary>
	private bool __debug;

	/// <summary>
	/// The DMI connection that will be used to query meta information about the database and its tables.
	/// </summary>
	private DMI __dmi;

	/// <summary>
	/// The scaling factor to affect drawing.
	/// </summary>
	private double __scale = 0;

	/// <summary>
	/// The number of pixels to draw vertically.
	/// </summary>
	private double __vPixels;

	/// <summary>
	/// The device on which the drawing will take place.
	/// </summary>
	private ERDiagram_Device __device;

	/// <summary>
	/// The drawing area that the device will use for drawing the ER diagram.
	/// </summary>
	private ERDiagram_DrawingArea __drawingArea;

	/// <summary>
	/// The ERDiagram_JFrame in which this panel is located.
	/// </summary>
	private ERDiagram_JFrame __parent;

	/// <summary>
	/// Array of the table relationships that will be shown on the ER diagram.
	/// </summary>
	private ERDiagram_Relationship[] __rels;

	/// <summary>
	/// Array of the tables that will be shown on the ER Diagram.
	/// </summary>
	private ERDiagram_Table[] __tables;

	/// <summary>
	/// The number of elements in the __referenceTable list.
	/// </summary>
	private int __referenceTableCount;

	/// <summary>
	/// The scrollpane used to scroll around the panel.
	/// </summary>
	private JScrollPane __jsp;

	/// <summary>
	/// A reference to the text field in the parent JFrame that is on the left side of the status bar at the bottom.
	/// </summary>
	private JTextField __messageField;

	/// <summary>
	/// A reference to the status text field in the parent JFrame that is on the right
	/// side of the status bar at the bottom.
	/// </summary>
	private JTextField __statusField;

	/// <summary>
	/// The format for the printed output of the ER diagram generation.
	/// </summary>
	private PageFormat __pageFormat;

	/// <summary>
	/// The name of the field in the table in the database that specifies where the tables' X positions are.
	/// </summary>
	private string __erdXField;

	/// <summary>
	/// The name of the field in the table in the database that specifies where the tables' Y positions are.
	/// </summary>
	private string __erdYField;

	/// <summary>
	/// The name of the table in the database that contains a list of the tables and
	/// their positions in the ER Diagram (if DataTable not provided).
	/// </summary>
	private string __tablesTableName;

	/// <summary>
	/// The data table containing table layout information (if database table name not provided).
	/// </summary>
	private DataTable __tablesTable = null;

	/// <summary>
	/// The name of the field in the table in the database that specifies the names of the tables to put on the ER Diagram.
	/// </summary>
	private string __tableNameField = null;

	/// <summary>
	/// A list of the names of the reference tables in the ER diagram.  Will never be null.
	/// </summary>
	private IList<string> __referenceTables;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dmi"> an open and connected dmi that is hooked into the database for which the ER Diagram will be built. </param>
	/// <param name="tablesTableName"> the name of the table in the database that contains a list of all the tables. </param>
	/// <param name="tableNameField"> the name of the field within the above table that contains the names of the tables. </param>
	/// <param name="erdXField"> the name of the field within the above table that contains the X position of the tables in the ER Diagram. </param>
	/// <param name="erdYField"> the name of the field within the above table that contains the Y position of the tables in the ER Diagram. </param>
	/// <param name="pageFormat"> the pageFormat with which the page will be printed. </param>
	/// <param name="debug"> whether to turn on debugging options in the popup menu. </param>
	public ERDiagram_JPanel(ERDiagram_JFrame parent, DMI dmi, DataTable tablesTable, string tableNameField, string erdXField, string erdYField, IList<string> referenceTables, PageFormat pageFormat, bool debug)
	{
		__dmi = dmi;
		__parent = parent;
		__debug = debug;

		// assumed for now
		__scale = .5;

		__tablesTable = tablesTable;
		__tableNameField = tableNameField;
		__erdXField = erdXField;
		__erdYField = erdYField;
		if (referenceTables == null)
		{
			__referenceTables = new List<string>();
		}
		else
		{
			__referenceTables = referenceTables;
		}
		__referenceTableCount = __referenceTables.Count;

		__pageFormat = pageFormat;

		if (__debug)
		{
			Console.WriteLine("width: " + pageFormat.getImageableWidth());
			Console.WriteLine("height:" + pageFormat.getImageableHeight());
		}

		int widthPixels = (int)(pageFormat.getWidth() / __scale);
		int heightPixels = (int)(pageFormat.getHeight() / __scale);

		setupGUI(widthPixels, heightPixels, __scale);
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
	/// <param name="debug"> whether to turn on debugging options in the popup menu. </param>
	public ERDiagram_JPanel(ERDiagram_JFrame parent, DMI dmi, string tablesTableName, string tableNameField, string erdXField, string erdYField, IList<string> referenceTables, PageFormat pageFormat, bool debug)
	{
		__dmi = dmi;
		__parent = parent;
		__debug = debug;

		// assumed for now
		__scale = .5;

		__tablesTableName = tablesTableName;
		__tableNameField = tableNameField;
		__erdXField = erdXField;
		__erdYField = erdYField;
		if (referenceTables == null)
		{
			__referenceTables = new List<string>();
		}
		else
		{
			__referenceTables = referenceTables;
		}
		__referenceTableCount = __referenceTables.Count;

		__pageFormat = pageFormat;

		if (__debug)
		{
			Console.WriteLine("width: " + pageFormat.getImageableWidth());
			Console.WriteLine("height:" + pageFormat.getImageableHeight());
		}

		int widthPixels = (int)(pageFormat.getWidth() / __scale);
		int heightPixels = (int)(pageFormat.getHeight() / __scale);

		setupGUI(widthPixels, heightPixels, __scale);
	}

	/// <summary>
	/// Checks to see whether any of the tables' positions have been changed. </summary>
	/// <returns> true if any table has been moved, otherwise return false. </returns>
	private bool areTablesDirty()
	{
		for (int i = 0; i < __tables.Length; i++)
		{
			if (__tables[i].isDirty())
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Called after windowClosing and windowClosed events; checks to see if the
	/// positions of any of the tables have changed, and if so, prompts whether the
	/// changes should be saved or not.
	/// </summary>
	protected internal virtual void closeWindow()
	{
		if (areTablesDirty())
		{
			ResponseJDialog dialog = new ResponseJDialog(getParentJFrame(), "Save changes?", "Changes have been made to the table " + "positions.  Save changes?", ResponseJDialog.YES | ResponseJDialog.NO);
			if (dialog.response() == ResponseJDialog.YES)
			{
				writeTables();
			}
		}
	}

	/// <summary>
	/// Converts the list of relationships to remove all relationships that point
	/// to reference tables (see the __referenceTables list), so that reference tables
	/// appear to hover independent of all other tables. </summary>
	/// <param name="rels"> the list of relationships to convert. </param>
	/// <returns> the converted relationships list. </returns>
	private IList<ERDiagram_Relationship> convertReferenceRelationships(IList<ERDiagram_Relationship> rels)
	{
		IList<ERDiagram_Relationship> newRels = new List<ERDiagram_Relationship>();
		ERDiagram_Relationship rel;
		for (int i = 0; i < rels.Count; i++)
		{
			rel = rels[i];
			if (isReferenceTable(rel.getStartTable()) && isReferenceTable(rel.getEndTable()))
			{
	//Message.printStatus(2, "", "Link from " + rel.getStartTable() + " to " 
	//	+ rel.getEndTable() + ": both are refs, both removed.");
				rel.setReference(true);
				newRels.Add(rel);
				rel.setNonReferenceTable(rel.getEndTable());
			}
			else if (isReferenceTable(rel.getStartTable()))
			{
	//Message.printStatus(2, "", "Link from " + rel.getStartTable() + " to " 
	//	+ rel.getEndTable() + ": removing " + rel.getStartTable());
				rel.setReference(true);
				newRels.Add(rel);
				rel.setNonReferenceTable(rel.getEndTable());
			}
			else if (isReferenceTable(rel.getEndTable()))
			{
	//Message.printStatus(2, "", "Link from " + rel.getStartTable() + " to " 
	//	+ rel.getEndTable() + ": removing " + rel.getEndTable());
				rel.setReference(true);
				newRels.Add(rel);
				rel.setNonReferenceTable(rel.getStartTable());
			}
			else
			{
				rel.setReference(false);
				newRels.Add(rel);
			}
		}

		return newRels;
	}

	/// <summary>
	/// Returns the device that is controlling all the drawing. </summary>
	/// <returns> the device that is controlling all the drawing. </returns>
	public virtual ERDiagram_Device getDevice()
	{
		return __device;
	}

	/// <summary>
	/// Returns the DMI being used to connect to the database. </summary>
	/// <returns> the DMI bring used to connect to the database. </returns>
	public virtual DMI getDMI()
	{
		return __dmi;
	}

	/// <summary>
	/// Returns the page format that was set up for this page. </summary>
	/// <returns> the PageFormat that was set up for this page. </returns>
	public virtual PageFormat getPageFormat()
	{
		return __pageFormat;
	}

	/// <summary>
	/// Returns the ERDiagram_JFrame in which this panel is located. </summary>
	/// <returns> the ERDiagram_JFrame in which this panel is located. </returns>
	protected internal virtual ERDiagram_JFrame getParentJFrame()
	{
		return __parent;
	}

	/// <summary>
	/// Returns the table from the __tables array that has the given name 
	/// (using a case-sensitive comparison). </summary>
	/// <param name="name"> the name of the table to return. </param>
	/// <returns> the table from __tables array with the given name, or null if none
	/// match. </returns>
	private ERDiagram_Table getTableByName(string name)
	{
		for (int i = 0; i < __tables.Length; i++)
		{
			if (__tables[i].getName().Equals(name))
			{
				return __tables[i];
			}
		}
		return null;
	}

	/// <summary>
	/// Returns whether debugging was turned on. </summary>
	/// <returns> whether debugging was turned on. </returns>
	protected internal virtual bool isDebug()
	{
		return __debug;
	}

	/// <summary>
	/// Returns whether the table with the given name is a reference table or not. </summary>
	/// <param name="name"> the name of the table to check (case sensitive). </param>
	/// <returns> true if the table is a reference table, false if not. </returns>
	private bool isReferenceTable(string name)
	{
		string s = null;
		for (int i = 0; i < __referenceTableCount; i++)
		{
			s = (string)__referenceTables[i];
			if (s.Equals(name))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Returns true if the table with the specified name is in the __tables array. </summary>
	/// <param name="tableName"> the name of the table to look for. </param>
	/// <returns> true if the table is in the __tables array, false otherwise. </returns>
	private bool isTableInTablesArray(string tableName)
	{
		for (int i = 0; i < __tables.Length; i++)
		{
			if (tableName.Equals(__tables[i].getName()))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Takes the list of relationship objects and removes all relationships for 
	/// which either the start table or the end table is not visible. </summary>
	/// <returns> the relationships list, minus all relationships connecting to 
	/// non-visible tables. </returns>
	private IList<ERDiagram_Relationship> pruneInvisibleRelationships(IList<ERDiagram_Relationship> rels)
	{
		IList<ERDiagram_Relationship> newRels = new List<ERDiagram_Relationship>();
		ERDiagram_Relationship rel;

		for (int i = 0; i < rels.Count; i++)
		{
			rel = rels[i];
			if (isTableInTablesArray(rel.getStartTable()))
			{
				if (isTableInTablesArray(rel.getEndTable()))
				{
					if (getTableByName(rel.getStartTable()).isVisible() && getTableByName(rel.getEndTable()).isVisible())
					{
							rel.setVisible(true);
					}
					newRels.Add(rel);
				}
			}
		}

		return newRels;
	}

	/// <summary>
	/// Calls the DMIUtil method to read in the list of relationships from the 
	/// database and populate the array of ERDiagram_Relationship objects.
	/// </summary>
	protected internal virtual ERDiagram_Relationship[] readRelationships()
	{
		setMessageStatus("Creating list of table relationships", "WAIT");
		IList<ERDiagram_Relationship> rels = DMIUtil.createERDiagramRelationships(__dmi, null);

		rels = pruneInvisibleRelationships(rels);
		rels = convertReferenceRelationships(rels);
		__rels = new ERDiagram_Relationship[rels.Count];
		for (int i = 0; i < rels.Count; i++)
		{
			__rels[i] = rels[i];
		}

		setMessageStatus("Done creating list", "READY");
		return __rels;
	}

	/// <summary>
	/// Calls the DMIUtil method to read in the tables from the database and populate
	/// the array of ERDiagram_Table object.
	/// </summary>
	protected internal virtual ERDiagram_Table[] readTables()
	{
		string routine = this.GetType().Name + ".readTables";
		setMessageStatus("Creating list of database tables", "WAIT");
		IList<ERDiagram_Table> tables = new List<ERDiagram_Table>(0);
		// First read the table metadata from the database.  If the name of the table with database tables
		// is not specified, don't pass that to the following - coordinates for drawing won't be initialized.
		tables = DMIUtil.createERDiagramTables(__dmi, __tablesTableName, __tableNameField, __erdXField, __erdYField, null);
		// Next if a data table has been provided, get the X and Y coordinates from that table
		if (__tablesTable != null)
		{
			int nameCol = -1;
			try
			{
				nameCol = __tablesTable.getFieldIndex(__tableNameField);
			}
			catch (Exception e)
			{
				Message.printWarning(3, routine, e);
				nameCol = -1;
			}
			int xCol = -1;
			try
			{
				xCol = __tablesTable.getFieldIndex(__erdXField);
			}
			catch (Exception e)
			{
				Message.printWarning(3, routine, e);
				xCol = -1;
			}
			int yCol = -1;
			try
			{
				yCol = __tablesTable.getFieldIndex(__erdYField);
			}
			catch (Exception e)
			{
				Message.printWarning(3, routine, e);
				yCol = -1;
			}
			double x, y;
			 object o = null;
			foreach (ERDiagram_Table table in tables)
			{
				// TODO SAM 2015-05-09 Implement robust error handling
				// Find a matching table name
				TableRecord rec = null;
				try
				{
					rec = __tablesTable.getRecord(nameCol, table.getName());
				}
				catch (Exception e)
				{
					Message.printWarning(3,routine,e);
					Message.printStatus(2,routine,"Could not match table name \"" + table.getName() + "\" in table list.");
					continue;
				}
				// Try to get the X and Y coordinates from the table
				try
				{
					o = rec.getFieldValue(xCol);
				}
				catch (Exception e)
				{
					Message.printWarning(3, routine, e);
					x = 0.0;
				}
				if (o == null)
				{
					x = 0.0;
				}
				else
				{
					x = (double?)o.Value;
				}
				try
				{
					o = rec.getFieldValue(yCol);
				}
				catch (Exception e)
				{
					Message.printWarning(3, routine, e);
					y = 0.0;
				}
				if (o == null)
				{
					y = 0.0;
				}
				else
				{
					y = (double?)o.Value;
				}
				table.setX(x);
				table.setY(y);
			}
		}
		Message.printStatus(2,"","Initialized " + tables.Count + " tables for diagram.");

		__tables = new ERDiagram_Table[tables.Count];
		for (int i = 0; i < tables.Count; i++)
		{
			__tables[i] = (ERDiagram_Table)tables[i];
		}
		setMessageStatus("Done creating list", "READY");
		return __tables;
	}

	/// <summary>
	/// Sets the initial position of the scrollpane's viewport to be the lower-left
	/// hand corner.
	/// </summary>
	protected internal virtual void setInitialViewportPosition()
	{
		Point pt = new Point();
		pt.x = 0;
		pt.y = (int)(__vPixels - (__jsp.getViewport().getExtentSize().getHeight()));
		__jsp.getViewport().setViewPosition(pt);
	}

	/// <summary>
	/// Sets the message in the status bar of the parent JFrame.  This can be a 
	/// short sentence as the message field is the longer one.  This can also be null,
	/// in which case the message text field will be cleared. </summary>
	/// <param name="text"> the text to put in the message field. </param>
	public virtual void setMessage(string text)
	{
		if (__messageField != null)
		{
			if (!string.ReferenceEquals(text, null))
			{
				__messageField.setText(text);
			}
			else
			{
				__messageField.setText("");
			}
			JGUIUtil.forceRepaint(__messageField);
		}

	}

	/// <summary>
	/// Sets up the text fields that are in the status bar of the parent JFrame.
	/// Either can be null. </summary>
	/// <param name="messageField"> the longer text field. </param>
	/// <param name="statusField"> the shorted text field. </param>
	protected internal virtual void setMessageFields(JTextField messageField, JTextField statusField)
	{
		__messageField = messageField;
		__statusField = statusField;
	}

	/// <summary>
	/// Sets up a message (which can be long) and a status message (which should 
	/// generally be one word) in the status bar of the parent JFrame. </summary>
	/// <param name="message"> the message to set in the status bar (can be null, in which 
	/// case the message text field will be cleared). </param>
	/// <param name="status"> the status to set in the status bar (can be null, in which 
	/// case the status text field will be cleared). </param>
	public virtual void setMessageStatus(string message, string status)
	{
		setMessage(message);
		setStatus(status);
	}

	/// <summary>
	/// Sets the status message in the status bar of the parent JFrame.  This should
	/// generally just be one word.  This can also be null, in which the case the
	/// status text field will be cleared. </summary>
	/// <param name="status"> the status to set in the status text field. </param>
	public virtual void setStatus(string status)
	{
		if (__statusField != null)
		{
			if (!string.ReferenceEquals(status, null))
			{
				__statusField.setText(status);
			}
			else
			{
				__statusField.setText("");
			}
			JGUIUtil.forceRepaint(__statusField);
		}
	}

	/// <summary>
	/// Sets up the GUI. </summary>
	/// <param name="hPixels"> the number of horizontal pixels in the drawing area </param>
	/// <param name="vPixels"> the number of vertical pixels in the drawing area </param>
	/// <param name="scale"> the scaling factor to adjust ppi by </param>
	private void setupGUI(int hPixels, int vPixels, double scale)
	{
		JGUIUtil.setSystemLookAndFeel(true);

		readTables();
		readRelationships();

		__device = new ERDiagram_Device(__tables, __rels, this, scale);
		__device.setPreferredSize(new Dimension(hPixels, vPixels));
		GRLimits drawingLimits = new GRLimits(0.0, 0.0, hPixels, vPixels);
		__drawingArea = new ERDiagram_DrawingArea(__device, drawingLimits);
		__device.setDrawingArea(__drawingArea);

		setLayout(new GridBagLayout());
		__jsp = new JScrollPane(__device);
		__vPixels = vPixels;
		JGUIUtil.addComponent(this, __jsp, 0, 0, 1, 1, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.SOUTHWEST);
	}

	/// <summary>
	/// Responds to window activated events; does nothing. </summary>
	/// <param name="ev"> the WindowEvent that happened. </param>
	public virtual void windowActivated(WindowEvent ev)
	{
	}

	/// <summary>
	/// Responds to window closed events; calls closeWindow(). </summary>
	/// <param name="ev"> the WindowEvent that happened. </param>
	public virtual void windowClosed(WindowEvent ev)
	{
		closeWindow();
	}

	/// <summary>
	/// Responds to window closing events; calls closeWindow(). </summary>
	/// <param name="ev"> the WindowEvent that happened. </param>
	public virtual void windowClosing(WindowEvent ev)
	{
		closeWindow();
	}

	/// <summary>
	/// Responds to window deactivated events; does nothing. </summary>
	/// <param name="ev"> the WindowEvent that happened. </param>
	public virtual void windowDeactivated(WindowEvent ev)
	{
	}

	/// <summary>
	/// Responds to window deiconified events; does nothing. </summary>
	/// <param name="ev"> the WindowEvent that happened. </param>
	public virtual void windowDeiconified(WindowEvent ev)
	{
	}

	/// <summary>
	/// Responds to window iconified events; does nothing. </summary>
	/// <param name="ev"> the WindowEvent that happened. </param>
	public virtual void windowIconified(WindowEvent ev)
	{
	}

	/// <summary>
	/// Responds to window opened events; does nothing. </summary>
	/// <param name="ev"> the WindowEvent that happened. </param>
	public virtual void windowOpened(WindowEvent ev)
	{
	}

	/// <summary>
	/// Generates the SQL to write table positions back to the database.  Only tables
	/// that have moved (they are dirty) have their positions saved.
	/// </summary>
	protected internal virtual void writeTables()
	{
		setMessageStatus("Writing table changes to database", "WAIT");
		ERDiagram_Table table = null;
		string sql = null;
		for (int i = 0; i < __tables.Length; i++)
		{
			table = __tables[i];
			if (table.isDirty())
			{
				sql = "UPDATE " + __tablesTableName + " SET "
					+ __erdXField + " = " + table.getX() + ", " + __erdYField + "= " + table.getY() + " WHERE " + __tableNameField + " = '"
					+ table.getName() + "'";
				try
				{
					__dmi.dmiWrite(sql);
				}
				catch (Exception e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
				__tables[i].setDirty(false);
			}
		}
		setMessageStatus("Done writing table changes", "READY");
	}

	}

}