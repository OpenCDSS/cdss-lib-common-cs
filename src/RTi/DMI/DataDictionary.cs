using System;
using System.Collections.Generic;

// DataDictionary - class to create a data dictionary for a database

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

namespace RTi.DMI
{

	using HTMLWriter = RTi.Util.IO.HTMLWriter;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using DateTime = RTi.Util.Time.DateTime;

	// TODO smalers 2018-09-12 with advances in the JDBC API, this code could be rewritten to be more readable.
	// For example, call the column metadata resultset getString("COLUMN_NAME") instead of dealing with resultset
	// integer positions.  There are some limitations, such as not being able to cleanly detect array columns
	// and dimensions of the array.
	/// <summary>
	/// This class creates a data dictionary.
	/// </summary>
	public class DataDictionary
	{

		// TODO smalers 2018-09-21 why is this using integer positions rather than column name lookup,
		// which would be more readable?
	/// <summary>
	/// Field numbers used in determining field values when generating data dictionaries.
	/// Equivalent is databaseMetaData.getColumns() and then getString("COLUMN_NAME"), etc.
	/// </summary>
	private readonly int __POS_NUM = 11, __POS_COLUMN_NAME = 0, __POS_IS_PRIMARY_KEY = 1, __POS_COLUMN_TYPE = 2, __POS_COLUMN_SIZE = 3, __POS_NUM_DIGITS = 4, __POS_NULLABLE = 5, __POS_REMARKS = 6, __POS_EXPORTED = 7, __POS_FOREIGN = 8, __POS_PRIMARY_TABLE = 9, __POS_PRIMARY_FIELD = 10;

	/// <summary>
	/// Constructor.
	/// </summary>
	public DataDictionary()
	{

	}

	/// <summary>
	/// Creates an HTML data dictionary.
	/// The data dictionary consists of three main sections:<ol>
	/// <li>The initial table list.  This shows a list of all the tables in the
	/// database and any accompanying remarks.  Each table name is a link to the 
	/// table detail in section 2, below:</li>
	/// <li>A detailed list of the columns and column types for every table.</li>
	/// <li>A list of all the reference tables passed in to this method and a dump of all their data.</li> </summary>
	/// <param name="dmi"> DMI instance for an opened database connection. </param>
	/// <param name="filename"> Complete name of the data dictionary HTML file to write.  If
	/// the filename does not end with ".html", that will be added to the end of the filename. </param>
	/// <param name="newline"> string to replace newlines with (e.g., "<br">). </param>
	/// <param name="surroundWithPre"> if true, output comments/remarks surrounded by <pre></pre>, for example
	/// to keep newlines as is. </param>
	/// <param name="encodeHtmlChars"> if true, encode HTML characters that have meaning, such as < so as to pass through to HTML. </param>
	/// <param name="referenceTables"> If not null, the contents of these tables will be listed
	/// in a section of the data dictionary to illustrate possible values for lookup fields. </param>
	/// <param name="excludeTables"> list of tables that should be
	/// excluded from the data dictionary.  The names of the tables in this list
	/// must match the actual table names exactly (cases and spaces).  May be null.  May contain wildcard *. </param>
	public virtual void createHTMLDataDictionary(DMI dmi, string filename, string newline, bool surroundWithPre, bool encodeHtmlChars, IList<string> referenceTables, IList<string> excludeTables)
	{
		string routine = this.GetType().Name + ".createHTMLDataDictionary";

		// Get the name of the data.  If the name is null, it's most likely
		// because the connection is going through ODBC, in which case the 
		// name of the ODBC source will be used.
		string dbName = dmi.getDatabaseName();
		if (string.ReferenceEquals(dbName, null))
		{
			dbName = dmi.getODBCName();
		}

		// do the following so no worries about making null checks
		if (referenceTables == null)
		{
			referenceTables = new List<string>();
		}

		if (!StringUtil.endsWithIgnoreCase(filename, ".html"))
		{
			filename = filename + ".html";
		}

		Message.printStatus(2, routine, "Creating HTMLWriter");
		HTMLWriter html = null;
		// try to open an HTMLWriter object.
		try
		{
			html = new HTMLWriter(filename, dbName + " Data Dictionary");
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, "Error opening HTMLWriter file - aborting data dictionary creation (" + e + ").");
			return;
		}

		// Write out the header information.  
		// This info tells when the Data Dictionary was 
		// created and if the database connection is through JDBC:
		// - the name of the database engine
		// - the name of the database server
		// - the name of the database
		// - the port on which the database is found
		//
		// If the database connection is through ODBC, the name of the ODBC
		// source is printed.
		Message.printStatus(2, routine, "Writing Data Dictionary header information");
		try
		{
			html.heading(1, dbName + " Data Dictionary");
			DateTime now = new DateTime(DateTime.DATE_CURRENT);
			html.addText("Generated at: " + now);
			html.paragraph();

			if (dmi.getJDBCODBC())
			{
				html.addText("Database engine: " + dmi.getDatabaseEngine());
				html.breakLine();
				html.addText("Database server: " + dmi.getDatabaseServer());
				html.breakLine();
				html.addText("Database name: " + dmi.getDatabaseName());
			}
			else
			{
				html.addText("ODBC DSN: " + dmi.getODBCName());
			}
			html.paragraph();
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, "Error writing dictionary header (" + e + ").");
			Message.printWarning(2, routine, e);
		}

		Message.printStatus(2, routine, "Getting list of tables and views");
		ResultSet rs = null;
		string[] tableTypes = new string[] {"TABLE", "VIEW"};
		if (dmi.getDatabaseEngineType() == DMI.DBENGINE_SQLSERVER)
		{
			// SQL Server does not seem to recognize the type array so get all and then filter below
			tableTypes = null;
		}
		DatabaseMetaData metadata = null;
		try
		{
			metadata = dmi.getConnection().getMetaData();
			rs = metadata.getTables(null, null, null, tableTypes);
			if (rs == null)
			{
				Message.printWarning(2, routine, "Error getting list of tables.  Aborting");
				return;
			}
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, "Error getting list of tables - aborting (" + e + ").");
			Message.printWarning(2, routine, e);
			return;
		}

		// Loop through the result set and pull out the list of all the table names and the table remarks.  
		Message.printStatus(2, routine, "Building table name and remark list");
		string temp;
		string temp2;
		IList<string> tableNames = new List<string>(); // Used for sorting the list of tables
		IList<JdbcTableMetadata> tableList0 = new List<JdbcTableMetadata>();
		bool doFullTableMetadata = true;
		bool idColumnSupported = true;
		bool idColumnSetterSupported = true;
		while (true)
		{
			try
			{
				if (!rs.next())
				{
					break;
				}
				JdbcTableMetadata table = new JdbcTableMetadata();
				// Table name...
				temp = rs.getString(3);
				if (!rs.wasNull())
				{
					tableNames.Add(temp.Trim());
					table.setName(temp.Trim());
					// Remarks...
					temp = rs.getString(5);
					if (!rs.wasNull())
					{
						table.setRemarks(temp.Trim());
					}
					else
					{
						// Add a multi-character blank string so when it's placed in the HTML table,
						// it will be turned into &nbsp; and will keep the table cell full.
						table.setRemarks("  ");
					}
					if (doFullTableMetadata)
					{
						temp = rs.getString(1);
						if (!rs.wasNull())
						{
							table.setCatalog(temp.Trim());
						}
						temp = rs.getString(2);
						if (!rs.wasNull())
						{
							table.setSchema(temp.Trim());
						}
						temp = rs.getString(4);
						if (!rs.wasNull())
						{
							table.setType(temp.Trim());
						}
						try
						{
							temp = rs.getString(9);
							if (!rs.wasNull())
							{
								table.setSelfRefColumn(temp.Trim());
							}
						}
						catch (Exception)
						{
							// Not all databases support
							idColumnSupported = false;
						}
						try
						{
							temp = rs.getString(10);
							if (!rs.wasNull())
							{
								table.setSelfRefColumnHowCreated(temp.Trim());
							}
						}
						catch (Exception)
						{
							// Not all databases support
							idColumnSetterSupported = false;
						}
					}
				}
				tableList0.Add(table);
			}
			catch (Exception e)
			{
				// continue getting the list of table names, but report the error.
				Message.printWarning(3, routine, "Error getting list of table names (" + e + ").");
				Message.printWarning(3, routine, e);
			}
		}
		try
		{
			DMI.closeResultSet(rs);
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, e);
		}

		// Sort the list of table names in ascending order and also the remarks, ignoring case.
		int[] sortOrder = new int[tableNames.Count];
		tableNames = StringUtil.sortStringList(tableNames, StringUtil.SORT_ASCENDING, sortOrder, true, true);
		IList<JdbcTableMetadata> tableList = new List<JdbcTableMetadata>(sortOrder.Length);
		for (int i = 0; i < sortOrder.Length; i++)
		{
			tableList.Add(tableList0[sortOrder[i]]);
		}

		Message.printStatus(2, routine, "Read " + tableNames.Count + " tables from database.");

		// Remove the list of system tables for each kind of database (all database types have certain system tables)
		bool isSQLServer = false;
		//String databaseEngine = dmi.getDatabaseEngine();
		int databaseEngineType = dmi.getDatabaseEngineType();
		Message.printStatus(2, routine, "Removing tables that should be skipped");
		string[] systemTablePatternsToRemove = DMIUtil.getSystemTablePatternsToRemove(databaseEngineType);
		for (int i = 0; i < systemTablePatternsToRemove.Length; i++)
		{
			StringUtil.removeMatching(tableNames,systemTablePatternsToRemove[i],true);
		}

		// Remove all the tables that were in the excludeTables parameter passed in to this method.
		if (excludeTables != null)
		{
			foreach (string excludeTable in excludeTables)
			{
				// Handle glob-style wildcards and protect other than *
				// Escape special characters that may occur in table names 
				string excludeTable2 = excludeTable.Replace(".", "\\.").Replace("*",".*").Replace("$", "\\$");
				for (int i = tableList.Count - 1; i >= 0; i--)
				{
					string table = tableList[i].getName();
					// Remove table name at end so loop works
					if (table.matches(excludeTable2))
					{
						if (Message.isDebugOn)
						{
							Message.printDebug(1,routine,"Removing table \"" + table + "\" from dictionary.");
						}
						tableList.RemoveAt(i);
					}
				}
			}
		}

		Message.printStatus(2, routine, "Printing table names and remarks");
		// Print out a table containing the names of all the tables 
		// that will be reported on as well as any table remarks for 
		// those tables.  Each table name will be a link to its detailed
		// column information later in the data dictionary.
		try
		{
			html.paragraph();
			html.heading(2, dbName + " Tables");

			//html.blockquoteStart();
			html.tableStart("border=2 cellspacing=0");
			html.tableRowStart("valign=top");
			html.tableRowStart("valign=top bgcolor=#CCCCCC");
			html.tableHeader("Table Name");
			html.tableHeader("Remarks");
			if (doFullTableMetadata)
			{
				html.tableHeader("Catalog");
				html.tableHeader("Schema");
				html.tableHeader("Type");
				if (idColumnSupported)
				{
					html.tableHeader("ID Column");
				}
				if (idColumnSetterSupported)
				{
					html.tableHeader("ID Column Setter");
				}
			}
			html.tableRowEnd();

			foreach (JdbcTableMetadata table in tableList)
			{
				string name = table.getName();
				html.tableRowStart("valign=top");
				html.tableCellStart();
				html.linkStart("#Table:" + name);
				html.addLinkText(name);
				html.linkEnd();
				html.tableCellEnd();
				temp = table.getRemarks();
				if (temp.Trim().Equals(""))
				{
					temp = "    ";
				}
				html.tableCell(temp);
				if (doFullTableMetadata)
				{
					html.tableCell(table.getCatalog());
					html.tableCell(table.getSchema());
					html.tableCell(table.getType());
					if (idColumnSupported)
					{
						html.tableCell(table.getSelfRefColumn());
					}
					if (idColumnSetterSupported)
					{
						html.tableCell(table.getSelfRefColumnHowCreated());
					}
				}
				html.tableRowEnd();
			}

			html.tableEnd();
			//html.blockquoteEnd();
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, "Error writing list of tables (" + e + ").");
			Message.printWarning(2, routine, e);
		}

		// Format the key for table formats
		try
		{
			html.paragraph();
			html.heading(2, "Table Color Legend");
			html.paragraph();
			//html.blockquoteStart();

			html.tableStart("border=2 cellspacing=0");

			html.tableRowStart("valign=top bgcolor=#CCCCCC");
			html.tableHeader("Table Section");
			html.tableHeader("Formatting Style");
			html.tableRowEnd();

			html.tableRowStart("valign=top");
			html.tableCell("Column Names");
			html.tableCellStart("valign=top bgcolor=#CCCCCC");
			html.boldStart();
			html.addText("Bold text, gray background");
			html.boldEnd();
			html.tableCellEnd();
			html.tableRowEnd();

			html.tableRowStart("valign=top");
			html.tableCell("Primary Key Fields");
			html.tableCellStart("valign=top bgcolor=yellow");
			html.boldStart();
			html.addText("Bold text, yellow background");
			html.boldEnd();
			html.tableCellEnd();
			html.tableRowEnd();

			html.tableRowStart("valign=top");
			html.tableCell("Foreign Key Fields");
			html.tableCellStart("valign=top bgcolor=orange");
			html.addText("Orange background with Foreign Key Link field");
			html.tableCellEnd();
			html.tableRowEnd();

			html.tableRowStart("valign=top");
			html.tableCell("Other Fields");
			html.tableCell("Normal text, white background");
			html.tableRowEnd();

			html.tableEnd();

			//html.blockquoteEnd();
		}
		catch (Exception e)
		{
			Message.printWarning(3, routine, "Error creating key for tables (" + e + ").");
			Message.printWarning(3, routine, e);
		}

		// Start the table detail section of the data dictionary.
		try
		{
			html.paragraph();
			html.heading(2, "Table Detail");
			html.paragraph();
			//html.blockquoteStart();
		}
		catch (Exception e)
		{
			Message.printWarning(3,routine,"Error creating table detail heading (" + e + ").");
			Message.printWarning(3, routine, e);
		}

		Message.printStatus(2, routine, "Writing table details for " + tableList.Count + " tables");

		string primaryKeyField = null;
		string primaryKeyTable = null;
		string tableName = null;
		int tableCount = 0;
		foreach (JdbcTableMetadata table in tableList)
		{
			++tableCount;
			try
			{
				tableName = table.getName();
				Message.printStatus(1, routine, "Processing table \"" + tableName + "\" (" + tableCount + " of " + tableList.Count + ")");
				html.anchor("Table:" + tableName);
				html.headingStart(3);
				html.addText(table.getName() + " - " + table.getRemarks());

				for (int j = 0; j < referenceTables.Count; j++)
				{
					if (tableName.Equals(referenceTables[j], StringComparison.OrdinalIgnoreCase))
					{
						   html.addText("  ");
						html.link("#ReferenceTable:" + tableName, "(View Contents)");
						j = referenceTables.Count + 1;
					}
				}

				html.headingEnd(3);
				//html.blockquoteStart();

				// Get a list of all the table columns that are in the Primary key.
				ResultSet primaryKeysRS = null;
				IList<string> primaryKeysV = null;
				int primaryKeysSize = 0;
				try
				{
					primaryKeysRS = metadata.getPrimaryKeys(null, null, tableName);
					primaryKeysV = new List<string>();
					while (primaryKeysRS.next())
					{
						primaryKeysV.Add(primaryKeysRS.getString(4));
					}
					primaryKeysSize = primaryKeysV.Count;
					DMI.closeResultSet(primaryKeysRS);
				}
				catch (Exception e)
				{
					// If an exception is thrown here, it is probably because the JDBC driver does not
					// support the "getPrimaryKeys" method.  
					// No problem, it will be treated as if there were no primary keys.
					Message.printWarning(2,routine,"Error getting primary keys for table \"" + tableName + "\" - not formatting primary keys (" + e + ").");
				}

				// Get a list of all the table columns that have foreign key references to other tables
				ResultSet foreignKeysRS = null;
				IList<string> foreignKeyPriTablesV = null;
				IList<string> foreignKeyPriFieldsV = null;
				IList<string> foreignKeyFieldsV = null;
				int foreignKeysSize = 0;
				try
				{
					foreignKeysRS = metadata.getImportedKeys(null, null, tableName);
					foreignKeyPriFieldsV = new List<string>();
					foreignKeyPriTablesV = new List<string>();
					foreignKeyFieldsV = new List<string>();
					while (foreignKeysRS.next())
					{
						foreignKeyPriTablesV.Add(foreignKeysRS.getString(3));
						foreignKeyPriFieldsV.Add(foreignKeysRS.getString(4));
						foreignKeyFieldsV.Add(foreignKeysRS.getString(8));
					}
					foreignKeysSize = foreignKeyFieldsV.Count;
					DMI.closeResultSet(foreignKeysRS);
				}
				catch (Exception e)
				{
					Message.printWarning(2,routine,"Error getting primary keys for table \"" + tableName + "\" - not formatting foreign keys (" + e + ").");
				}

				// Get a list of all the fields that are exported so that foreign keys can link to them
				ResultSet exportedKeysRS = null;
				IList<string> exportedKeysV = null;
				int exportedKeysSize = 0;
				try
				{
					exportedKeysRS = metadata.getExportedKeys(null, null, tableName);
					exportedKeysV = new List<string>();
					while (exportedKeysRS.next())
					{
						exportedKeysV.Add(exportedKeysRS.getString(4));
					}
					exportedKeysSize = exportedKeysV.Count;
					DMI.closeResultSet(exportedKeysRS);
				}
				catch (Exception e)
				{
					Message.printWarning(3, routine, "Error getting exported keys for table \"" + tableName + "\" uniquetempvar.");
					Message.printWarning(3, routine, e);
				}

				bool exportedKey = false;
				bool foreignKey = false;
				bool primaryKey = false;
				int foreignKeyPos = -1;
				IList<IList<string>> tableColumnsMetadataList = new List<IList<string>>();
				IList<string> columnNames = new List<string>();

				// Next, get the actual column data for the current table.
				rs = metadata.getColumns(null, null, tableName, null);
				if (rs == null)
				{
					Message.printWarning(3, routine, "Error getting columns for \"" + tableName + "\" table.");
					continue;
				}

				// Loop through each column and move all its important data into a list of lists.  This data will
				// be run through at least twice, and to do that
				// with a ResultSet would require several expensive opens and closes.

				string columnName = null;
				while (rs.next())
				{
					exportedKey = false;
					foreignKey = false;
					primaryKey = false;
					foreignKeyPos = -1;
					IList<string> columnMetadataList = new List<string>();
					for (int ic = 0; ic < __POS_NUM; ic++)
					{
						columnMetadataList.Add("");
					}
					if (Message.isDebugOn)
					{
						// TODO SAM 2014-03-09 The following seems to mess up subsequent requests for data
						// Subsequent requests give "no data" errors almost as if the first rs.getString() call
						// advances the record
						printColumnMetadata(rs);
					}
					// Get the 'column name' and store it in list position __POS_COLUMN_NAME
					columnName = rs.getString(4);
					if (string.ReferenceEquals(columnName, null))
					{
						columnName = " ";
					}
					else
					{
						columnName = columnName.Trim();
					}
					columnMetadataList[__POS_COLUMN_NAME] = columnName;
					columnNames.Add(columnName);

					// Get whether this is a primary key or not and store either "TRUE" (for it being a 
					// primary key) or "FALSE" in list position __POS_IS_PRIMARY_KEY
					for (int j = 0; j < primaryKeysSize; j++)
					{
						if (columnName.Equals(primaryKeysV[j].Trim()))
						{
							primaryKey = true;
							j = primaryKeysSize + 1;
						}
					}
					if (primaryKey)
					{
						columnMetadataList[__POS_IS_PRIMARY_KEY] = "TRUE";
					}
					else
					{
						columnMetadataList[__POS_IS_PRIMARY_KEY] = "FALSE";
					}

					// Get the 'column type' and store it in list position __POS_COLUMN_TYPE
					temp = rs.getString(6);
					if (string.ReferenceEquals(temp, null))
					{
						temp = "Unknown";
					}
					else
					{
						temp = temp.Trim();
					}
					columnMetadataList[__POS_COLUMN_TYPE] = temp;

					// Get the 'column size' and store it in list position __POS_COLUMN_SIZE
					temp = rs.getString(7);
					columnMetadataList[__POS_COLUMN_SIZE] = temp;

					// Get the 'column num digits' and store it in list position __POS_NUM_DIGITS
					temp = rs.getString(9);
					if (string.ReferenceEquals(temp, null))
					{
						columnMetadataList[__POS_NUM_DIGITS] = "0";
					}
					else
					{
						columnMetadataList[__POS_NUM_DIGITS] = temp;
					}

					// Get whether the column is nullable and store it in list position __POS_NULLABLE
					temp = rs.getString(18);
					if (string.ReferenceEquals(temp, null))
					{
						temp = "Unknown";
					}
					else
					{
						temp = temp.Trim();
					}
					columnMetadataList[__POS_NULLABLE] = temp;

					// Get the column remarks and store them in list position __POS_REMARKS
					if (isSQLServer)
					{
						temp = rs.getString(12);
						//columnData.set(__POS_REMARKS,getSQLServerColumnComment(dmi, tableName, columnName));
					}
					else
					{
						temp = rs.getString(12);
					}
					if (string.ReferenceEquals(temp, null))
					{
						temp = "   ";
					}
					else
					{
						temp = temp.Trim();
					}
					columnMetadataList[__POS_REMARKS] = temp;

					// Get whether the column is exported for foreign keys to connect to and store it
					// in Vector position __POS_EXPORTED as either "TRUE" or "FALSE"
					for (int j = 0; j < exportedKeysSize; j++)
					{
						if (columnName.Equals(exportedKeysV[j].Trim()))
						{
							exportedKey = true;
							j = exportedKeysSize + 1;
						}
					}
					if (exportedKey)
					{
						columnMetadataList[__POS_EXPORTED] = "TRUE";
					}
					else
					{
						columnMetadataList[__POS_EXPORTED] = "FALSE";
					}

					// Get whether the column is a foreign key field and store it in Vector position 
					// __POS_FOREIGN as either "TRUE" or "FALSE"

					// Additionally, set the table of the primary key to which the foreign key connects as
					// Vector position __POS_PRIMARY_TABLE.  
					// If not a foreign key, that position will be null

					// Set the field of the primary key to which the foreign key connects as Vector position
					// __POS_PRIMARY_FIELD.  If not a foreign key, that position will be null.

					for (int j = 0; j < foreignKeysSize; j++)
					{
						if (columnName.Equals(foreignKeyFieldsV[j].Trim()))
						{
							foreignKey = true;
							foreignKeyPos = j;
							j = foreignKeysSize + 1;
						}
					}
					if (foreignKey)
					{
						columnMetadataList[__POS_FOREIGN] = "TRUE";
						columnMetadataList[__POS_PRIMARY_TABLE] = foreignKeyPriTablesV[foreignKeyPos];
						columnMetadataList[__POS_PRIMARY_FIELD] = foreignKeyPriFieldsV[foreignKeyPos];
					}
					else
					{
						columnMetadataList[__POS_FOREIGN] = "FALSE";
						columnMetadataList[__POS_PRIMARY_TABLE] = null;
						columnMetadataList[__POS_PRIMARY_FIELD] = null;
					}

					tableColumnsMetadataList.Add(columnMetadataList);
				}

				try
				{
					DMI.closeResultSet(rs);
				}
				catch (Exception e)
				{
					Message.printWarning(2, routine, e);
				}

				// Create the table and the table header for displaying the table column information.
				html.tableStart("border=2 cellspacing=0");
				html.tableRowStart("valign=top bgcolor=#CCCCCC");
				html.tableHeader("Column Name");
				html.tableHeader("Remarks");
				html.tableHeader("Column Type");
				html.tableHeader("Allow Null");
				if (foreignKeysSize > 0)
				{
					html.tableHeader("Foreign Key Link");
				}
				html.tableRowEnd();

				// Next, an alphabetized list of the column names in the table will be compiled.  This will be used
				// to display columns in the right sorting order.
				int numColumns = columnNames.Count;
				int[] order = new int[numColumns];
				// FIXME SAM 2014-03-08 set the order to the original for now
				// Some code most have been lost here
				for (int j = 0; j < numColumns; j++)
				{
					order[j] = j;
				}
				System.Collections.IList[] sortedVectors = new System.Collections.IList[numColumns];
				for (int j = 0; j < numColumns; j++)
				{
					sortedVectors[j] = (IList<string>)tableColumnsMetadataList[order[j]];
				}

				// Now that the sorted order of the column names (and the lists of data) is known,
				// loop through the data lists looking for columns which are in 
				// the Primary key.  They will be displayed in bold face font with a yellow background.
				for (int j = 0; j < numColumns; j++)
				{
					IList<string> columnData = sortedVectors[j];
					temp = null;

					temp = columnData[__POS_IS_PRIMARY_KEY];

					if (temp.Equals("TRUE"))
					{
						html.tableRowStart("valign=top bgcolor=yellow");

						// display the column name
						temp = columnData[__POS_COLUMN_NAME];
						html.tableCellStart();
						html.boldStart();

						temp2 = columnData[__POS_EXPORTED];
						if (temp2.Equals("TRUE"))
						{
							html.anchor("Table:" + tableName + "." + temp);
						}

						html.addText(temp);
						html.boldEnd();
						html.tableCellEnd();

						// display the remarks
						temp = columnData[__POS_REMARKS];
						html.tableCellStart();
						html.boldStart();
						writeRemarks(html,temp,newline,surroundWithPre,encodeHtmlChars);
						html.boldEnd();
						html.tableCellEnd();

						// display the column type
						temp = columnData[__POS_COLUMN_TYPE];
						Message.printStatus(2, routine, "Table \"" + tableName + "\" column \"" + columnName + "\" type is " + temp);
						if (temp.Equals("real", StringComparison.OrdinalIgnoreCase))
						{
							temp = temp + "(" + columnData[__POS_COLUMN_SIZE] + ", " + columnData[__POS_NUM_DIGITS] + ")";
						}
						else if (temp.Equals("float", StringComparison.OrdinalIgnoreCase) || (temp.Equals("double", StringComparison.OrdinalIgnoreCase)) || (temp.Equals("smallint", StringComparison.OrdinalIgnoreCase)) || (temp.Equals("int", StringComparison.OrdinalIgnoreCase)) || (temp.Equals("integer", StringComparison.OrdinalIgnoreCase)) || (temp.Equals("counter", StringComparison.OrdinalIgnoreCase)) || (temp.Equals("datetime", StringComparison.OrdinalIgnoreCase)))
						{
						}
						else
						{
							temp = temp + "(" + columnData[__POS_COLUMN_SIZE] + ")";
						}
						html.tableCellStart();
						html.boldStart();
						html.addText(temp);
						html.boldEnd();
						html.tableCellEnd();

						// display whether it's nullable
						temp = columnData[__POS_NULLABLE];
						html.tableCellStart();
						html.boldStart();
						html.addText(temp);
						html.boldEnd();
						html.tableCellEnd();

						temp = columnData[__POS_FOREIGN];
						if (temp.Equals("TRUE"))
						{
							html.tableCellStart();
							primaryKeyTable = columnData[__POS_PRIMARY_TABLE];
							primaryKeyField = columnData[__POS_PRIMARY_FIELD];

							html.link("#Table:" + primaryKeyTable, primaryKeyTable);
							html.addLinkText(".");
							html.link("#Table:" + primaryKeyTable + "." + primaryKeyField, primaryKeyField);
							html.tableCellEnd();
						}
						else if (foreignKeysSize > 0)
						{
							html.tableCell("  ");
						}

						html.tableRowEnd();
					}
				}

				// Now do the same thing for the other fields, the non-primary key fields.  
				for (int j = 0; j < numColumns; j++)
				{
					IList<string> column = sortedVectors[j];
					string isPrimaryKey = column[__POS_IS_PRIMARY_KEY];

					if (isPrimaryKey.Equals("FALSE"))
					{
						string isForeignKey = column[__POS_FOREIGN];
						if (isForeignKey.Equals("TRUE"))
						{
							html.tableRowStart("valign=top bgcolor=orange");
						}
						else
						{
							html.tableRowStart("valign=top");
						}

						// display the column name
						string columnName2 = column[__POS_COLUMN_NAME];
						html.tableCellStart();
						html.addText(columnName2);
						html.tableCellEnd();

						// display the remarks
						string remarks = column[__POS_REMARKS];
						html.tableCellStart();
						writeRemarks(html,remarks,newline,surroundWithPre,encodeHtmlChars);
						html.tableCellEnd();

						// display the column type
						string columnType = column[__POS_COLUMN_TYPE];
						Message.printStatus(2, routine, "Table \"" + tableName + "\" column \"" + columnName + "\" type is " + columnType);
						if (columnType.Equals("real", StringComparison.OrdinalIgnoreCase))
						{
							columnType = columnType + "(" + column[__POS_COLUMN_SIZE] + ", " + column[__POS_NUM_DIGITS] + ")";
						}
						else if (columnType.Equals("float", StringComparison.OrdinalIgnoreCase) || (columnType.Equals("double", StringComparison.OrdinalIgnoreCase)) || (columnType.Equals("smallint", StringComparison.OrdinalIgnoreCase)) || (columnType.Equals("int", StringComparison.OrdinalIgnoreCase)) || (columnType.Equals("integer", StringComparison.OrdinalIgnoreCase)) || (columnType.Equals("counter", StringComparison.OrdinalIgnoreCase)) || (columnType.Equals("datetime", StringComparison.OrdinalIgnoreCase)))
						{
							// TODO smalers 2018-09-21 not sure why not just print the column size always as below
						}
						else
						{
							columnType = columnType + "(" + column[__POS_COLUMN_SIZE] + ")";
						}
						if (columnType.StartsWith("_", StringComparison.Ordinal))
						{
							// Used for arrays in PostgreSQL and maybe others
							columnType = columnType + "[...]"; // TODO smalers 2018-09-21 need to get actual dimension
						}
						html.tableCellStart();
						html.addText(columnType);
						html.tableCellEnd();

						// display whether it's nullable
						string isNullable = column[__POS_NULLABLE];
						html.tableCellStart();
						html.addText(isNullable);
						html.tableCellEnd();

						if (isForeignKey.Equals("TRUE"))
						{
							html.tableCellStart();
							primaryKeyTable = column[__POS_PRIMARY_TABLE];
							primaryKeyField = column[__POS_PRIMARY_FIELD];

							html.link("#Table:" + primaryKeyTable, primaryKeyTable);
							html.addLinkText(".");
							html.link("#Table:" + primaryKeyTable + "." + primaryKeyField, primaryKeyField);
							html.tableCellEnd();
						}
						else if (foreignKeysSize > 0)
						{
							html.tableCell("  ");
						}

						html.tableRowEnd();
					}
				}

				// Close the table, insert a paragraph break, and get ready to do it again for the next table.
				html.tableEnd();
				//html.blockquoteEnd();
				html.paragraph();
			}
			catch (Exception e)
			{
				Message.printWarning(2, routine, "Error printing column information for table \"" + tableName + "\" uniquetempvar.");
				Message.printWarning(2, routine, e);
			}
		}

		Message.printStatus(2, routine, "Listing stored procedures (not implemented yet)");
		// List stored procedures...
		// Not yet done
		// TODO (JTS - 2003-04-22) does this need to be done?

		// Now list the contents of the reference tables.  These tables are dumped out in their entirety. 

		if (referenceTables.Count > 0)
		{
			Message.printStatus(2, routine, "Printing contents of reference tables");
			try
			{
				html.paragraph();
				html.heading(2, "Reference Table Contents");
				html.paragraph();
				//html.blockquoteStart();
			}
			catch (Exception e)
			{
				Message.printWarning(3, routine, "Error printing header for reference table contents (" + e + ").");
				Message.printWarning(3, routine, e);
			}
		}

		string ldelim = dmi.getFieldLeftEscape();
		string rdelim = dmi.getFieldRightEscape();

		// Loop through each of the tables that was passed in to the method
		// in the referenceTables array and get a list of its column names
		// and then print out all of its data in one table.
		string refTableName;
		for (int i = 0; i < referenceTables.Count; i++)
		{
			refTableName = referenceTables[i];

			try
			{
				rs = metadata.getColumns(null, null, refTableName, null);
				if (rs == null)
				{
					Message.printWarning(2, routine, "Error getting columns for \"" + refTableName + "\" table.");
					continue;
				}
				html.anchor("ReferenceTable:" + refTableName);
				html.headingStart(3);
				html.addText(refTableName + "  ");
				html.link("#Table:" + refTableName, "(View Definition)");
				html.headingEnd(3);
				//html.blockquoteStart();

				IList<string> columnNames = new List<string>();
				while (rs.next())
				{
					columnNames.Add(rs.getString(4).Trim());
				}
				DMI.closeResultSet(rs);

				// create a SQL String that will query the appropriate table for all data in the found fields.
				// This is used because perhaps in the future it might be
				// desire to limit the fields from which data is displayed.
				string sql = "SELECT ";
				int j = 0;
				for (j = 0; j < columnNames.Count; j++)
				{
					if (j > 0)
					{
						sql += ", ";
					}
					sql += ldelim + columnNames[j] + rdelim;
				}
				sql += " FROM " + ldelim + refTableName + rdelim + " ORDER BY ";

				for (j = 0; j < columnNames.Count; j++)
				{
					if (j > 0)
					{
						sql += ", ";
					}
					sql += ldelim + columnNames[j] + rdelim;
				}

				// j will be greater than 0 if there were any columns in the list of columnNames for the table.
				// It will equal 0 if the table name could not be found or was null.
				if (j > 0)
				{
					rs = dmi.dmiSelect(sql);

					// Create the header for the reference table
					html.tableStart("border=2 cellspacing=0");
					html.tableRowStart("valign=top bgcolor=#CCCCCC");

					for (j = 0; j < columnNames.Count; j++)
					{
						html.tableHeader(columnNames[j]);
					}
					html.tableRowEnd();

					// Start dumping out all the data in the reference table.  The data is retrieved as
					// Strings, which seems to work fine.
					string tableCellValue;
					while (rs.next())
					{
						html.tableRowStart("valign=top");
						for (j = 0; j < columnNames.Count;j++)
						{
							tableCellValue = rs.getString(j + 1);
							if (string.ReferenceEquals(tableCellValue, null))
							{
								tableCellValue = "NULL";
							}
							html.tableCell(tableCellValue);
						}
						html.tableRowEnd();
					}
					html.tableEnd();
					DMI.closeResultSet(rs);
				}
				//html.blockquoteEnd();
				html.paragraph();
			}
			catch (Exception e)
			{
				Message.printWarning(2, routine, "Error dumping reference table data (" + e + ").");
				Message.printWarning(2, routine, e);
			}
		}

		Message.printStatus(2, routine, "Writing HTML file");
		// Finally, try to close and write out the HTML file.
		try
		{
			html.closeFile();
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, "Error closing the HTML file (" + e + ").");
			Message.printWarning(2, routine, e);
		}
		Message.printStatus(2, routine, "Done creating data dictionary");
	}

	/// <summary>
	/// Print the column metadata from a result set.
	/// </summary>
	private static void printColumnMetadata(ResultSet rs)
	{
		try
		{
			Message.printStatus(2,"","TABLE_CAT=" + rs.getString(1));
			Message.printStatus(2,"","TABLE_SCHEM=" + rs.getString(2));
			Message.printStatus(2,"","TABLE_NAME=" + rs.getString(3));
			Message.printStatus(2,"","COLUMN_NAME=" + rs.getString(4));
			Message.printStatus(2,"","DATA_TYPE=" + rs.getInt(5));
			Message.printStatus(2,"","TYPE_NAME=" + rs.getString(6));
			Message.printStatus(2,"","COLUMN_SIZE=" + rs.getInt(7));
			Message.printStatus(2,"","BUFFER_LENGTH=" + rs.getInt(8));
			Message.printStatus(2,"","DECIMAL_DIGITS=" + rs.getInt(9));
			Message.printStatus(2,"","NUM_PREC_RADIX=" + rs.getInt(10));
			Message.printStatus(2,"","NULLABLE=" + rs.getInt(11));
			Message.printStatus(2,"","REMARKS=" + rs.getString(12));
			Message.printStatus(2,"","COLUMN_DEF=" + rs.getString(13));
			Message.printStatus(2,"","SQL_DATA_TYPE=" + rs.getInt(14));
			Message.printStatus(2,"","SQL_DATETIME_SUB=" + rs.getInt(15));
			Message.printStatus(2,"","CHAR_OCTET_LENGTH=" + rs.getInt(16));
			Message.printStatus(2,"","ORDINAL_POSITION=" + rs.getInt(17));
			Message.printStatus(2,"","IS_NULLABLE=" + rs.getString(18));
			Message.printStatus(2,"","SCOPE_CATLOG=" + rs.getString(19));
			Message.printStatus(2,"","SCOPE_SCHEMA=" + rs.getString(20));
			Message.printStatus(2,"","SCOPE_TABLE=" + rs.getString(21));
			Message.printStatus(2,"","SOURCE_DATA_TYPE=" + rs.getString(22));
			Message.printStatus(2,"","IS_AUTOINCREMENT=" + rs.getString(23));
		}
		catch (Exception)
		{
			// Ignore, most likely something like Microsoft Access not supporting indices correctly
		}
	}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void writeRemarks(RTi.Util.IO.HTMLWriter html, String temp, String newline, boolean surroundWithPre, boolean encodeHtmlChars) throws Exception
	private void writeRemarks(HTMLWriter html, string temp, string newline, bool surroundWithPre, bool encodeHtmlChars)
	{
		string tempUpper = temp.ToUpper();
		if (tempUpper.StartsWith("<HTML>", StringComparison.Ordinal) && tempUpper.EndsWith("</HTML>", StringComparison.Ordinal))
		{
			// Pass the text through without additional formatting, but first remove the HTML tags.
			html.write(temp.Substring(6, (temp.Length - 12) - 6));
		}
		else if (surroundWithPre)
		{
			html.write("<pre>");
			html.write(temp);
			html.write("</pre>");
		}
		else if (encodeHtmlChars)
		{
			html.addText(temp);
		}
		else
		{
			// Just write the text
			if ((!string.ReferenceEquals(newline, null)) && newline.Length > 0)
			{
				temp = temp.Replace("\n",newline);
			}
			html.write(temp);
		}
	}

	}

}