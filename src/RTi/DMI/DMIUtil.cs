using System;
using System.Collections.Generic;
using System.Text;

// DMIUtil - static methods for use with the DMI package

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
// DMIUtil.java - static methods for use with the DMI package
//
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2002-10-25	Steven A. Malers, RTi	Initial version - don't want to keep
//					adding to DMI when static utility
//					methods may not always be used and would
//					add to the size of DMI.  Start with the
//					getAvailableOdbcDsn() method.
// 2002-12-24	SAM, RTi		* Move general code from the DMI class
//					  to streamline the DMI class.
//					* Fill out the formatDateTime() class
//					  to actually do something, using CDSS
//					  HBData.formatSQLDate() as a model.
//					* Make the specific isMissingXXX()
//					  methods private to encourage only the
//					  isMissing() methods to be used.
// 2003-03-05	J. Thomas Sapienza, RTi	Moved formatting code for where strings
//					and order clauses in here from 
//					the old HydroBaseDMI.
// 2003-03-08	SAM, RTi		Add createHTMLDataDictionary() method to
//					automatically create a data dictionary
//					from a database connection.
// 2003-03-31	JTS, RTi		Fixed bug in formatDateTime that was
//					resulting in the wrong output for the
//					given precision.
// 2003-04-21	JTS, RTi		Added test HTML-generating Data 
//					dictionary code.
// 2003-04-22	JTS, RTi		Added the initial duplicateTable() code.
// 2003-04-23	JTS, RTi		* Added removeTable()
//					* Cleaned up duplicateTable().
//					* Added code to the Data Dictionary 
//					  generator to read SQL Server table
//					  and column comments.
// 2003-07-31	JTS, RTi		Data dictionary code now only limits
//					its initial query to finding type
//					TABLE database objects.
// 2003-09-02	JTS, RTi		* Cleaned out some old debugging code.
//					* Updated javadocs for recently-added
//					  methods.
// 2003-10-23	JTS, RTi		Added isMissing() methods for primitive
//					containers (Double, Integer, etc).
// 2003-11-12	JTS, RTi		* Corrected error in formatDateTime that
//					  was (for Access databases) putting in
//					  'day' instead of the actual day and
//					  'year' instead of the actual year.
//					* Corrected error in formatDateTime 
//					  that was screwing up Informix dates.
// 2004-01-02	SAM, RTi		* Add getWhereClausesFromInputFilter().
// 2004-01-21	JTS, RTi		* Corrected bug in formatWhere clause
//					  caused by passing in a *
// 2004-06-22	JTS, RTi		* Added getExtremeRecord().
//					* Added getMaxRecord().
//					* Added getMinRecord().
// 2004-10-25	SAM, RTi		Change getWhereClausesFromInputFilter()
//					to check the operator ignoring case.
//					Some tools now use the operator string
//					in a persistent way that may not match
//					the case.
// 2004-11-18	JTS, RTi		* Corrected error in how table anchor
//					  links were generated as they were not
//					  working with IE.
// 					* Foreign links are now pulled out of
//					  the table and added to the HTML
//					  data dictionary.
// 					* Converted so that ResultSets are now
//					  closed with DMI.closeResultSet().
//					* Port number no longer appears on
//					  data dictionary.
//					* Data dictionary now has a legend for
//					  table colors.
//					* Data dictionary now has links from
//					  the reference table definitions to
//					  the contents, and vice versa.
// 2004-11-19	JTS, RTi		DateTimes can now be formatted for
//					PostgreSQL databases.
// 2005-01-11	JTS, RTi		* Where fields are now trimmed when 
//					  pulled out of filter panels.
//					* Method to create where clauses from
//					  input filters now does so via 
//					  a separate call which operates on 
//					  a single filter and its operator.
// 2005-04-25	JTS, RTi		Added formatDateTime() that allows 
//					leaving off the escape characters from
//					the date string.  This was done
//					primarily for use with stored 
//					procedure dates.
// 2005-11-16	JTS, RTi		Added resultSetHasColumn().
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------
// EndHeader

// REVISIT (JTS - 2003-04-24)
// TO-DO:
// 1) Add an error section at the bottom of the data dictionary listing 
//    all the errors encountered (i.e., if in Access and unable to get the list
//    of foreign keys)
// 2) Get the stored procedures out of the database and list those
// 3) For reference tables, add something to split certain reference tables
//    into multiple lines (because they scroll off the page)
// 4) Should display table views

namespace RTi.DMI
{
	using InputFilter = RTi.Util.GUI.InputFilter;
	using InputFilter_JPanel = RTi.Util.GUI.InputFilter_JPanel;
	using IOUtil = RTi.Util.IO.IOUtil;
	using ProcessManager = RTi.Util.IO.ProcessManager;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using DateTime = RTi.Util.Time.DateTime;


	/// <summary>
	/// The DMIUtil class provides static methods to facilitate database interaction.
	/// </summary>
	public abstract class DMIUtil
	{

	///////////////////////////////////////////////////////////
	//  Missing data value fields
	//  - Use special values that are unlikely to occur for real data values
	///////////////////////////////////////////////////////////

	/// <summary>
	/// Constant that represents a missing date (DateTime).
	/// </summary>
	public const System.DateTime MISSING_DATE = null;

	/// <summary>
	/// Constant that represents a missing boolean value.
	/// Booleans must be handled internally as an object, not primitive, so that a null state can be 
	/// </summary>
	public const double? MISSING_BOOLEAN = null;

	/// <summary>
	/// Constant that represents a missing double value.
	/// </summary>
	public static readonly double MISSING_DOUBLE = Double.NaN;

	/// <summary>
	/// Constant that represents the low end of a missing double value, when performing
	/// comparisons where roundoff may have occurred.
	/// </summary>
	// TODO SAM 2015-08-06 Evaluate whether this can be permanently removed
	//public static final double MISSING_DOUBLE_FLOOR = -999.1;

	/// <summary>
	/// Constant that represents the high end of a missing double value, when performing
	/// comparisons where roundoff may have occurred.
	/// </summary>
	//TODO SAM 2015-08-06 Evaluate whether this can be permanently removed
	//public static final double MISSING_DOUBLE_CEILING = -998.9;

	/// <summary>
	/// Constant that represents a missing float value.
	/// </summary>
	public static readonly float MISSING_FLOAT = Float.NaN;

	/// <summary>
	/// Constant that represents a missing int value.
	/// </summary>
	public static readonly int MISSING_INT = int.MinValue;

	/// <summary>
	/// Constant that represents a missing long value.
	/// </summary>
	public static readonly long MISSING_LONG = long.MinValue;

	/// <summary>
	/// Constant that represents a missing string.
	/// </summary>
	public const string MISSING_STRING = "";

	/// <summary>
	/// Checks a string for the presence of single quotes ('). </summary>
	/// <param name="s"> String to check </param>
	/// <returns> true if a single quote is detected, false otherwise. </returns>
	public static bool checkSingleQuotes(string s)
	{
		int index = s.IndexOf("'", StringComparison.Ordinal);
		if (index > -1)
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Creates a data dictionary. </summary>
	/// <param name="dmi"> DMI instance for an opened database connection. </param>
	/// <param name="filename"> Complete name of the data dictionary HTML file (a file extension
	/// is not added) to write. </param>
	/// <param name="referenceTables"> If not null, the contents of these tables will be listed
	/// in a section of the data dictionary to illustrate possible values for lookup
	/// fields.  Need to work on this to know what field to list first.
	/// TODO (JTS - 2004-11-19) this method is VERY out of date, compared to the HTML data dictionary.  The
	/// two methods should either be reconciled, or this one should be removed. </param>
	public static void createDataDictionary(DMI dmi, string filename, string[] referenceTables)
	{
		string routine = "DMIUtil.createDataDictionary";
		// Convert the messages to HTML code after getting the logic to work.
		// First get a list of tables and print out their information...

		Message.printStatus(2, routine, "Tables");
		ResultSet rs = null;
		DatabaseMetaData metadata = null;
		try
		{
			metadata = dmi.getConnection().getMetaData();
			rs = metadata.getTables(null, null, null, null);
			if (rs == null)
			{
				Message.printWarning(2, routine, "Error getting list of tables.");
			}
		}
		catch (Exception)
		{
			Message.printWarning(2, routine, "Error getting list of tables.");
			rs = null;
		}

		// Now get all the table names by looping through result set...

		bool more = false;
		try
		{
			more = rs.next();
		}
		catch (Exception)
		{
			more = false;
		}

		string s;
		IList<string> table_names = new List<string>();
		IList<string> table_remarks = new List<string>();
		while (more)
		{

			try
			{
				// Table name...
				s = rs.getString(3);
				if (!rs.wasNull())
				{
					table_names.Add(s.Trim());
					// Remarks...
					s = rs.getString(5);
					if (!rs.wasNull())
					{
						table_remarks.Add(s.Trim());
						// Remarks...
					}
					else
					{
						table_remarks.Add("");
					}
				}
				// Get the next item in the list...
				more = rs.next();
			}
			catch (Exception e)
			{
				// Ignore for now...
				Message.printWarning(2, routine, e);
			}
		}
		try
		{
			DMI.closeResultSet(rs);
		}
		catch (Exception)
		{
		}

		// Sort (need to add)...

		// Output tables...

		int size = table_names.Count;
		for (int i = 0; i < size; i++)
		{
			Message.printStatus(2, routine, table_names[i] + "," + table_remarks[i]);
		}

		// Next list the table contents...

		string table_name;
		string column_name;
		string column_type;
		int column_size;
		int column_num_digits;
		string column_nullable;
		for (int i = 0; i < size; i++)
		{
			Message.printStatus(2, routine, "Table details");

			table_name = (string)table_names[i];
			try
			{
				rs = metadata.getColumns(null, null, table_name, null);
				if (rs == null)
				{
					Message.printWarning(2, routine, "Error getting columns for \"" + table_name + "\" table.");
					DMI.closeResultSet(rs);
					continue;
				}

				// Print the column information...

				more = rs.next();

				while (more)
				{
					// The column name is field 4...
					column_name = rs.getString(4);
					if (!rs.wasNull())
					{
						column_name = column_name.Trim();
					}
					else
					{
						column_name = "";
					}
					column_type = rs.getString(6);
					if (!rs.wasNull())
					{
						column_type = column_type.Trim();
					}
					else
					{
						column_type = "Unknown";
					}
					column_size = rs.getInt(7);
					column_num_digits = rs.getInt(9);
					column_nullable = rs.getString(18);
					if (!rs.wasNull())
					{
						column_nullable = column_nullable.Trim();
					}
					else
					{
						column_nullable = "Unknown";
					}
					Message.printStatus(2, routine, "\"" + table_name + "\": \"" + column_name + "\" " + column_type + " " + column_size + " " + column_num_digits + " " + column_nullable);

					more = rs.next();
				}
			}
			catch (Exception)
			{
				Message.printWarning(2, routine, "Error getting columns for \"" + table_name + "\" table.");
			}
			try
			{
				DMI.closeResultSet(rs);
			}
			catch (Exception)
			{
			}
		}

		// List stored procedures...
		Message.printStatus(2, routine, "Stored procedures");

		// Next list the contents of reference tables...
		Message.printStatus(2, routine, "Reference tables");
	}

	/// <summary>
	/// Creates a list of ERDiagram_Relationship objects to be used in an ER Diagram. </summary>
	/// <param name="dmi"> an open and connected dmi object.  Must not be null. </param>
	/// <param name="notIncluded"> a list of the names of the tables for which to not make
	/// relationships.  May be null. </param>
	/// <returns> a list of ERDiagram_Relationship objects for use in an ER Diagram.  
	/// null is returned if there was an error creating the objects or reading from the database. </returns>
	public static IList<ERDiagram_Relationship> createERDiagramRelationships(DMI dmi, IList<string> notIncluded)
	{
		IList<string> tableNames = getDatabaseTableNames(dmi, null, null, true, notIncluded);

		if (tableNames == null)
		{
			return null;
		}

		try
		{
			DatabaseMetaData metadata = dmi.getConnection().getMetaData();
			ResultSet rs = null;

			string startTable = null;
			string endTable = null;
			string startField = null;
			string endField = null;

			int size = tableNames.Count;
			IList<ERDiagram_Relationship> rels = new List<ERDiagram_Relationship>();

			for (int i = 0; i < size; i++)
			{
				rs = metadata.getExportedKeys(null, null, tableNames[i]);

				while (rs.next())
				{
					startTable = rs.getString(3);
					startField = rs.getString(4);
					endTable = rs.getString(7);
					endField = rs.getString(8);

					ERDiagram_Relationship rel = new ERDiagram_Relationship(startTable, startField, endTable, endField);
					rels.Add(rel);
				}
				DMI.closeResultSet(rs);
			}
			return rels;
		}
		catch (Exception e)
		{
			Console.WriteLine(e.ToString());
			Console.Write(e.StackTrace);
			return null;
		}
	}

	/// <summary>
	/// Creates a list of ERDiagram_Tables for use in an ERDiagram. </summary>
	/// <param name="dmi"> an open and connected DMI object.  Must not be null. </param>
	/// <param name="tablesTableName"> the name of the table in the database that contains the
	/// list of table names and ER Diagram information.  If null the coordinates for the ER Diagram will not be set. </param>
	/// <param name="tableField"> the name of the column in the tables table that contains the names of the tables. </param>
	/// <param name="erdXField"> the name of the column in the tables table that contains the X positions of the ERDiagram Tables. </param>
	/// <param name="erdYField"> the name of the column in the tables table that contains the Y positions of the ERDIagram Tables. </param>
	/// <param name="notIncluded"> a list of the names of the tables to not include in the ERDiagram.  May be null. </param>
	/// <returns> a list of ERDiagram_Table objects that can be used to build an 
	/// ER Diagram.  null is returned if there was an error creating the tables or reading from the database. </returns>
	public static IList<ERDiagram_Table> createERDiagramTables(DMI dmi, string tablesTableName, string tableField, string erdXField, string erdYField, IList<string> notIncluded)
	{
		string routine = "DMIUtil.createERDiagramTables";
		string temp;
		DatabaseMetaData metadata = null;
		ResultSet rs = null;

		IList<string> tableNames = getDatabaseTableNames(dmi, null, null, true, notIncluded);

		if (tableNames == null)
		{
			return null;
		}

		int size = tableNames.Count;
		string tableName = null;
		Message.printStatus(2, routine, "Determining table details for ER Diagram");

		IList<ERDiagram_Table> tables = new List<ERDiagram_Table>();
		ERDiagram_Table table = null;

		try
		{
			metadata = dmi.getConnection().getMetaData();
		}
		catch (Exception e)
		{
			Message.printWarning(3,routine,e);
			return null;
		}

		for (int i = 0; i < size; i++)
		{
			tableName = tableNames[i];
			table = new ERDiagram_Table(tableName);

			try
			{
				// First get a list of all the table columns that are in the Primary key.
				ResultSet primaryKeyRS = null;
				IList<string> primaryKeyList = null;
				int primaryKeyListSize = 0;
				try
				{
					primaryKeyRS = metadata.getPrimaryKeys(null, null, tableName);
					primaryKeyList = new List<string>();
					while (primaryKeyRS.next())
					{
						primaryKeyList.Add(primaryKeyRS.getString(4));
					}
					primaryKeyListSize = primaryKeyList.Count;
					DMI.closeResultSet(primaryKeyRS);
				}
				catch (Exception)
				{
					// If an exception is thrown here, it is probably because the JDBC driver does not
					// support the "getPrimaryKeys" method.  
					// No problem, it will be treated as if there were no primary keys.
				}
				Message.printStatus(2,routine,"Table \"" + tableName + "\" has " + primaryKeyListSize + " primary keys");

				bool key = false;
				IList<IList<string>> columns = new List<IList<string>>();
				IList<string> columnNames = new List<string>();

				// Next, get the actual column data for the current table.
				rs = metadata.getColumns(null, null, tableName, null);
				if (rs == null)
				{
					Message.printWarning(2, routine, "Error getting columns for \"" + tableName + "\" table.");
					DMI.closeResultSet(rs);
					continue;
				}

				// Loop through each column and move all its important data into a list of list.  This data will
				// be run through at least twice, and to do that with a ResultSet would require several expensive opens and closes.
				string columnName = null;
				while (rs.next())
				{
					key = false;
					IList<string> columnNameList = new List<string>();

					// Get the 'column name' and store it in list position 0
					columnName = rs.getString(4);
					if (string.ReferenceEquals(columnName, null))
					{
						columnName = " ";
					}
					else
					{
						columnName = columnName.Trim();
					}
					columnNameList.Add(columnName);
					columnNames.Add(columnName);

					// Get whether this is a primary key or not and store either "TRUE" (for it being a 
					// primary key) or "FALSE" in list position 1
					for (int j = 0; j < primaryKeyListSize; j++)
					{
						if (columnName.Equals(primaryKeyList[j].Trim()))
						{
							key = true;
						}
					}

					if (key)
					{
						columnNameList.Add("TRUE");
					}
					else
					{
						columnNameList.Add("FALSE");
					}

					// Get the 'column type' and store it in list position 2
					temp = rs.getString(6);
					if (string.ReferenceEquals(temp, null))
					{
						temp = "Unknown";
					}
					else
					{
						temp = temp.Trim();
					}
					columnNameList.Add(temp);

					// Get the 'column size' and store it in list position 3
					temp = rs.getString(7);
					columnNameList.Add(temp);

					// Get the 'column num digits' and store it in list position 4
					temp = rs.getString(9);
					columnNameList.Add(temp);

					// Get whether the column is nullable and store it in list position 5
					temp = rs.getString(18);
					if (string.ReferenceEquals(temp, null))
					{
						temp = "Unknown";
					}
					else
					{
						temp = temp.Trim();
					}
					columnNameList.Add(temp);

					columns.Add(columnNameList);
				}

				// Next, an alphabetized list of the column names in the table will be compiled.
				// This will be used to display columns in the right sorting order.
				int numColumns = columnNames.Count;
				int[] order = new int[numColumns];
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<String>[] sortedLists = new java.util.ArrayList[numColumns];
				IList<string>[] sortedLists = new List<object>[numColumns];
				for (int j = 0; j < numColumns; j++)
				{
					sortedLists[j] = columns[order[j]];
				}

				string[] keyFields = new string[primaryKeyListSize];
				// Now that the sorted order of the column names (and the lists of data) is known, loop through
				// the data lists looking for columns that are in the Primary key.  They will be displayed in bold
				// face font with a yellow background.
				string field;
				string[] nonKeyFields = new string[(numColumns - primaryKeyListSize)];
				int count = 0;
				for (int j = 0; j < numColumns; j++)
				{
					IList<string> column = sortedLists[j];
					temp = null;

					temp = column[1];

					if (temp.Equals("TRUE"))
					{
						// display the column name
						temp = column[0];
						field = temp + ": ";

						// display the column type
						temp = column[2];
						if (temp.Equals("real", StringComparison.OrdinalIgnoreCase))
						{
							temp = temp + "(" + column[3] + ", " + column[4];
						}
						else if (temp.Equals("float", StringComparison.OrdinalIgnoreCase) || (temp.Equals("double", StringComparison.OrdinalIgnoreCase)) || (temp.Equals("smallint", StringComparison.OrdinalIgnoreCase)) || (temp.Equals("int", StringComparison.OrdinalIgnoreCase)) || (temp.Equals("integer", StringComparison.OrdinalIgnoreCase)) || (temp.Equals("counter", StringComparison.OrdinalIgnoreCase)) || (temp.Equals("datetime", StringComparison.OrdinalIgnoreCase)))
						{
						}
						else
						{
							temp = temp + "(" + column[3] + ")";
						}
						field += temp;
						keyFields[count++] = field;
					}
				}

				// Now do the same thing for the other fields, the non-primary key fields.  
				count = 0;
				for (int j = 0; j < numColumns; j++)
				{
					IList<string> column = sortedLists[j];
					temp = null;

					temp = column[1];

					if (temp.Equals("FALSE"))
					{
						// display the column name
						temp = column[0];
						field = temp + ": ";

						// display the column type
						temp = column[2];
						if (temp.Equals("real", StringComparison.OrdinalIgnoreCase))
						{
							temp = temp + "(" + column[3] + ", " + column[4];
						}
						else if (temp.Equals("float", StringComparison.OrdinalIgnoreCase) || (temp.Equals("double", StringComparison.OrdinalIgnoreCase)) || (temp.Equals("smallint", StringComparison.OrdinalIgnoreCase)) || (temp.Equals("int", StringComparison.OrdinalIgnoreCase)) || (temp.Equals("integer", StringComparison.OrdinalIgnoreCase)) || (temp.Equals("counter", StringComparison.OrdinalIgnoreCase)) || (temp.Equals("datetime", StringComparison.OrdinalIgnoreCase)))
						{
						}
						else
						{
							temp = temp + "(" + column[3] + ")";
						}
						field += temp;
						nonKeyFields[count++] = field;
					}
				}
				table.setKeyFields(keyFields);
				table.setNonKeyFields(nonKeyFields);
				table.setVisible(true);
				if ((!string.ReferenceEquals(tablesTableName, null)) && tablesTableName.Length > 0)
				{
					setTableXY(dmi, table, tablesTableName, tableField, erdXField, erdYField);
				}
				tables.Add(table);
			}
			catch (Exception e)
			{
				Message.printWarning(2, routine, "Error determining column information for table: " + tableName);
				Message.printWarning(2, routine, e);
			}

			try
			{
				DMI.closeResultSet(rs);
			}
			catch (Exception e)
			{
				Message.printWarning(2, routine, e);
			}
		}
		return tables;
	}

	/// <summary>
	/// Given a result set, prints the name and type of each column to log file status level 2. </summary>
	/// <param name="rs"> the ResultSet to dump information for. </param>
	/// <exception cref="SQLException"> if there is an error dumping information. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void dumpResultSetTypes(java.sql.ResultSet rs) throws java.sql.SQLException
	public static void dumpResultSetTypes(ResultSet rs)
	{

		// set up the types vector 
		ResultSetMetaData rsmd = rs.getMetaData();
		int columnCount = rsmd.getColumnCount();
		int colType = 0;
		string type = null;
		string colName = null;
		for (int i = 0; i < columnCount; i++)
		{
			colType = rsmd.getColumnType(i + 1);
			colName = rsmd.getColumnName(i + 1);
			switch (colType)
			{
				case java.sql.Types.BIGINT:
					type = "bigint";
					break;
				case java.sql.Types.BIT:
					type = "bit";
					break;
				case java.sql.Types.CHAR:
					type = "char";
					break;
				case java.sql.Types.DATE:
					type = "date";
					break;
				case java.sql.Types.DECIMAL:
					type = "decimal";
					break;
				case java.sql.Types.DOUBLE:
					type = "double";
					break;
				case java.sql.Types.FLOAT:
					type = "float";
					break;
				case java.sql.Types.INTEGER:
					type = "integer";
					break;
				case java.sql.Types.LONGVARBINARY:
					type = "longvarbinary";
					break;
				case java.sql.Types.LONGVARCHAR:
					type = "longvarchar";
					break;
				case java.sql.Types.NULL:
					type = "NULL";
					break;
				case java.sql.Types.NUMERIC:
					type = "numeric";
					break;
				case java.sql.Types.OTHER:
					type = "other";
					break;
				case java.sql.Types.REAL:
					type = "real";
					break;
				case java.sql.Types.SMALLINT:
					type = "smallint";
					break;
				case java.sql.Types.TIME:
					type = "time";
					break;
				case java.sql.Types.TIMESTAMP:
					type = "timestamp";
					break;
				case java.sql.Types.TINYINT:
					type = "tinyint";
					break;
				case java.sql.Types.VARBINARY:
					type = "varbinary";
					break;
				case java.sql.Types.VARCHAR:
					type = "varchar";
					break;
			}

			Message.printStatus(2, "", "Column " + (i + 1) + " \"" + colName + "\" " + ": " + type);
		}
	}

	/// <summary>
	/// Duplicates a table, its columns and primary key, and possibly the data (see the copyData parameter). </summary>
	/// <param name="dmi"> an open DMI connection. </param>
	/// <param name="origTableName"> the name of the table to duplicate </param>
	/// <param name="newTableName"> the name of the table to create </param>
	/// <param name="copyData"> if set to true, the data from the original table will also
	/// be copied into the new table.  <br>
	/// <b> Note:</b> If copyData is set to true, a 
	/// "SELECT * INTO newTable FROM origTable" query is run.  If set to false, the
	/// column information is queried from the original table and a CREATE TABLE
	/// SQL command is built. </param>
	/// <exception cref="Exception"> if an error occurs </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void duplicateTable(DMI dmi, String origTableName, String newTableName, boolean copyData) throws Exception
	public static void duplicateTable(DMI dmi, string origTableName, string newTableName, bool copyData)
	{
		string routine = "DMIUtil.duplicateTable";
		StringBuilder SQL = new StringBuilder();

		// Make sure not trying to create a table name that already 
		// exists in the database.  This check is done because this 
		// might not necessarily throw an Exception from the database.
		//
		// Example with SQL Server 2000:
		//
		// If the database owner has created a table called "Scenario",
		// it will be in the database catalog under the full name of
		// "dbo.Scenario".  If the user "guest" creates tries to duplicate
		// "Scenario", the new table will be placed in the database catalog as "guest.Scenario".
		//
		// Queries that don't reference the full table name, like:
		// "SELECT * FROM SCENARIO"
		// can still be executed, but it's unclear which table will actually be read.
		//
		// So it's better just to prevent duplicate table names completely.
		if (databaseHasTable(dmi, newTableName))
		{
			throw new Exception("Table '" + newTableName + "' already " + "exists in the database.");
		}

		if (copyData)
		{
			SQL.Append("SELECT * INTO " + newTableName + " FROM " + origTableName);
			// Turn off capitalization before executing the query so that
			// table names are made case sensitive to how the name was passed in to the method.
			bool caps = dmi.getCapitalize();
			if (caps)
			{
				dmi.setCapitalize(false);
			}
			dmi.dmiExecute(SQL.ToString());
			if (caps)
			{
				dmi.setCapitalize(true);
			}
			return;
		}

		ResultSet rs = null;
		DatabaseMetaData metadata = null;
		metadata = dmi.getConnection().getMetaData();

		// get the column data for the original table
		rs = metadata.getColumns(null, null, origTableName, null);
		if (rs == null)
		{
			throw new Exception("Error getting columns for \"" + origTableName + "\" table.");
		}

		bool more = rs.next();
		if (more == false)
		{
			throw new Exception("Unable to retrieve column information for table '" + origTableName + "'");
		}

		// get a list of all the table columns that are in the Primary key.
		ResultSet primaryKeysRS = null;
		IList<string> primaryKeysV = null;
		int primaryKeysSize = 0;
		primaryKeysRS = metadata.getPrimaryKeys(null, null, origTableName);
		primaryKeysV = new List<string>();
		while (primaryKeysRS.next())
		{
			primaryKeysV.Add(primaryKeysRS.getString(4));
		}
		primaryKeysSize = primaryKeysV.Count;
		DMI.closeResultSet(primaryKeysRS);

		bool key = false;
		string temp = null;
		IList<IList<string>> columns = new List<IList<string>>();
		// Loop through each column and move all its important data into a list of list.  This data will
		// be run through at least twice, and to do that with a ResultSet would require several expensive
		// opens and closes.
		while (more)
		{
			key = false;
			IList<string> column = new List<string>();

			// Get the 'column name' and store it in Vector position 0
			temp = rs.getString(4);
			if (string.ReferenceEquals(temp, null))
			{
				temp = " ";
			}
			else
			{
				temp = temp.Trim();
			}
			column.Add(temp);

			// Get whether this is a primary key or not and store either "TRUE" (for it being a 
			// primary key) or "FALSE" in list position 1
			for (int j = 0; j < primaryKeysSize; j++)
			{
				if (temp.Trim().Equals(primaryKeysV[j].Trim()))
				{
					key = true;
				}
			}
			if (key)
			{
				column.Add("TRUE");
			}
			else
			{
				column.Add("FALSE");
			}

			// Get the 'column type' and store it in list position 2
			temp = rs.getString(6);
			if (string.ReferenceEquals(temp, null))
			{
			temp = "Unknown";
			}
			else
			{
				temp = temp.Trim();
			}
			column.Add(temp);

			// Get the 'column size' and store it in list position 3
			temp = rs.getString(7);
			column.Add(temp);

			// Get the 'column num digits' and store it in list position 4
			temp = rs.getString(9);
			column.Add(temp);

			// Get whether the column is nullable and store it in list position 5
			temp = rs.getString(18);
			if (string.ReferenceEquals(temp, null))
			{
				temp = "Unknown";
			}
			else
			{
				temp = temp.Trim();
			}
			column.Add(temp);

			// Get the column remarks and store them in list position 6
			temp = rs.getString(12);
			if (string.ReferenceEquals(temp, null))
			{
				temp = "   ";
			}
			else
			{
				temp = temp.Trim();
			}
			column.Add(temp);

			columns.Add(column);
			more = rs.next();
		}

		DMI.closeResultSet(rs);

		// Start forming the sql string.
		SQL.Append("CREATE TABLE " + newTableName + "(\n");

		int numFields = columns.Count;
		string comma = null;
		for (int i = 0; i < numFields; i++)
		{
			comma = ",\n";
			if (i == (numFields - 1) && primaryKeysSize == 0)
			{
				comma = "\n)";
			}
			IList<string> column = columns[i];
			string name = column[0];
			string type = column[2];
			if (type.Equals("VARCHAR", StringComparison.OrdinalIgnoreCase))
			{
				type = type + " (" + column[3] + ")";
			}
			string isNull = column[5];
			if (isNull.Equals("Unknown", StringComparison.OrdinalIgnoreCase) || isNull.Equals("No", StringComparison.OrdinalIgnoreCase))
			{
					isNull = "NOT NULL";
			}
			else
			{
				isNull = "";
			}

			SQL.Append("   " + name + "\t" + type + "\t" + isNull + comma);
		}
		if (primaryKeysSize > 0)
		{
			SQL.Append("PRIMARY KEY (");
			for (int i = 0; i < primaryKeysSize; i++)
			{
				if (i > 0)
				{
					SQL.Append(", ");
				}
				SQL.Append(primaryKeysV[i]);
			}
			SQL.Append("))");

		}
		Message.printDebug(25, routine, "SQL: '" + SQL.ToString() + "'");

		// Turn off capitalization before executing the query so that
		// table names are made case sensitive to how the name was passed in to the method.
		bool caps = dmi.getCapitalize();
		if (caps)
		{
			dmi.setCapitalize(false);
		}
		dmi.dmiExecute(SQL.ToString());
		if (caps)
		{
			dmi.setCapitalize(true);
		}
	}

	/// <summary>
	/// Determine whether a database has a given stored procedure.  This can be used
	/// to determine a database version or as a basic check for a stored procedure before executing it. </summary>
	/// <returns> true if the procedure is in the database, false if not. </returns>
	/// <param name="dmi"> DMI instance for an opened database connection. </param>
	/// <param name="procedureName"> the name of the procedure to test for. </param>
	/// <exception cref="Exception"> if an error occurs </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static boolean databaseHasStoredProcedure(DMI dmi, String procedureName) throws Exception, java.sql.SQLException
	public static bool databaseHasStoredProcedure(DMI dmi, string procedureName)
	{
		if (!dmi.connected())
		{
			throw new SQLException("Database not connected, cannot call DMIUtil.databaseHasStoredProcedure()");
		}

		return databaseHasStoredProcedure(dmi, dmi.getConnection().getMetaData(), procedureName);
	}

	/// <summary>
	/// Determine whether a database has a given stored procedure.  This can be used
	/// to determine a database version or as a basic check for a stored procedure before executing it. </summary>
	/// <returns> true if the procedure is in the database, false if not </returns>
	/// <param name="dmi"> DMI instance for a database. </param>
	/// <param name="metaData"> meta data to search for the procedure name </param>
	/// <param name="procedureName"> the name of the procedure </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static boolean databaseHasStoredProcedure(DMI dmi, java.sql.DatabaseMetaData metaData, String procedureName) throws Exception, java.sql.SQLException
	public static bool databaseHasStoredProcedure(DMI dmi, DatabaseMetaData metaData, string procedureName)
	{
		if (!dmi.connected())
		{
			throw new SQLException("Database not connected, cannot call DMIUtil.databaseHasStoredProcedure()");
		}

		string message;
		string routine = "DMIUtil.databaseHasStoredProcedure";
		int dl = 25;

		string dbName = dmi.getDatabaseName();
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Checking if database \"" + dbName + "\" has stored procedure \"" + procedureName + "\"");
		}
		ResultSet rs = null;
		try
		{
			rs = metaData.getProcedures(dbName, null, null);
		}
		catch (Exception e)
		{
			message = "Exception getting list of stored procedures from database \"" + dbName + "\" (" + e + ")";
			Message.printWarning(3, routine, message);
			Message.printWarning(3, routine, e);
			throw new Exception(message);
		}
		if (rs == null)
		{
			message = "Null result set getting procedure names";
			Message.printWarning(3, routine, message);
			throw new Exception(message);
		}

		while (rs.next())
		{
			string proc = rs.getString("PROCEDURE_NAME");
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "Procedure name to check = \"" + proc + "\"");
			}
			// The SQL Server driver may return the procedure name with ";" and a number at the end.  If so
			// strip it off for further processing.
			int pos = proc.IndexOf(";", StringComparison.Ordinal);
			if (pos > 0)
			{
				proc = proc.Substring(0,pos);
			}
			if (proc.Equals(procedureName, StringComparison.OrdinalIgnoreCase))
			{
				// Using the following will close the related statement, which causes a problem on the 2nd call
				// to this method.
				//DMI.closeResultSet(rs);
				rs.close();
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, routine, "Database \"" + dbName + "\" DOES have stored procedure \"" + procedureName + "\"");
				}
				return true;
			}
		}
		// Using the following will close the related statement, which causes a problem on the 2nd call
		// to this method.
		//DMI.closeResultSet(rs);
		rs.close();
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Database \"" + dbName + "\" DOES NOT have stored procedure \"" + procedureName + "\"");
		}
		return false;
	}

	/// <summary>
	/// Determine whether a database has a table. </summary>
	/// <returns> true if the specified table is in the database, false if not. </returns>
	/// <param name="dmi"> DMI instance for a database connection. </param>
	/// <param name="tableName"> Name of table. </param>
	/// <exception cref="Exception"> if there is an error getting database information. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static boolean databaseHasTable(DMI dmi, String tableName) throws Exception, java.sql.SQLException
	public static bool databaseHasTable(DMI dmi, string tableName)
	{
		if (!dmi.connected())
		{
			throw new SQLException("Database not connected, cannot call DMIUtil.databaseHasTable()");
		}

		return databaseHasTable(dmi.getConnection().getMetaData(), tableName);
	}

	/// <summary>
	/// Determine whether a database has a table. </summary>
	/// <returns> true if the specified table is in the database, false if not. </returns>
	/// <param name="metadata"> DatabaseMetaData for connection. </param>
	/// <param name="tableName"> Name of table. </param>
	/// <exception cref="if"> there is an error getting database information. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static boolean databaseHasTable(java.sql.DatabaseMetaData metadata, String tableName) throws Exception
	public static bool databaseHasTable(DatabaseMetaData metadata, string tableName)
	{
		string message, routine = "DMI.databaseHasTable";
		ResultSet rs = null;
		int dl = 5;

		// The following can be used to get a full list of columns...
		try
		{
			rs = metadata.getTables(null, null, null, null);
			if (rs == null)
			{
				message = "Error getting list of tables to find table \"" + tableName + "\".";
				Message.printWarning(2, routine, message);
				throw new Exception(message);
			}
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "Database returned non-null table list.");
			}
		}
		catch (Exception)
		{
			message = "Error getting list of tables to find table \"" + tableName + "\".";
			Message.printWarning(2, routine, message);
			throw new Exception(message);
		}

		// Now check for the table by looping through result set...

		bool more = rs.next();

		string s;
		while (more)
		{
			// The table name is field 3...

			//if ( Message.isDebugOn ) {
			//	Message.printDebug ( dl, routine, "Checking table " + count );
			//}
			s = rs.getString(3);
			if (!rs.wasNull())
			{
				s = s.Trim();
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, routine, "Database has table \"" + s + "\"");
				}
				if (s.Equals(tableName, StringComparison.OrdinalIgnoreCase))
				{
					rs.close();
					return true;
				}
			}
			else
			{
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, routine, "Database has null table.");
				}
			}

			// Get the next item in the list...

			more = rs.next();
		}
		DMI.closeResultSet(rs);
		return false;
	}

	/// <summary>
	/// Determine whether a table in the database has a column. </summary>
	/// <returns> true if the specified table includes the specified column, false if the column is not in the table. </returns>
	/// <param name="dmi"> DMI instance for a database connection. </param>
	/// <param name="tableName"> Name of table. </param>
	/// <param name="columnName"> Name of column to check. </param>
	/// <exception cref="if"> there is an error getting database information. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static boolean databaseTableHasColumn(DMI dmi, String tableName, String columnName) throws java.sql.SQLException, Exception
	public static bool databaseTableHasColumn(DMI dmi, string tableName, string columnName)
	{
		if (!dmi.connected())
		{
			throw new SQLException("Database not connected, cannot call DMIUtil.databaseTableHasColumn()");
		}
		bool columnExists = databaseTableHasColumn(dmi.getConnection().getMetaData(), tableName, columnName);
		if (!columnExists)
		{
			columnExists = databaseTableHasColumn(dmi.getConnection().getMetaData(), tableName.ToUpper(), columnName.ToUpper());
		}
		return columnExists;
	}

	/// <summary>
	/// Determine whether a table in the database has a column. </summary>
	/// <returns> true if the specified table includes the specified column, false if the column is not in the table. </returns>
	/// <param name="metadata"> DatabaseMetaData for connection. </param>
	/// <param name="tableName"> Name of table. </param>
	/// <param name="columnName"> Name of column to check. </param>
	/// <exception cref="if"> there is an error getting database information. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static boolean databaseTableHasColumn(java.sql.DatabaseMetaData metadata, String tableName, String columnName) throws Exception, java.sql.SQLException
	public static bool databaseTableHasColumn(DatabaseMetaData metadata, string tableName, string columnName)
	{
		string message, routine = "DMI.databaseTableHasColumn";
		ResultSet rs = null;
		int dl = 5;

		// The following can be used to get a full list of columns...
		//try {	rs = metadata.getColumns ( null, null, table_name, null );}
		try
		{
			rs = metadata.getColumns(null, null, tableName, columnName);
			if (rs == null)
			{
				message = "Error getting columns for \"" + tableName + "\" table.";
				Message.printWarning(2, routine, message);
				throw new Exception(message);
			}
		}
		catch (Exception)
		{
			message = "Error getting database information for table \"" + tableName + "\".";
			Message.printWarning(2, routine, message);
			throw new Exception(message);
		}

		// Now check for the columns by looping through result set...

		bool more = rs.next();

		string s;
		//Vector	column_names = new Vector ( 5, 5 );
		while (more)
		{
			// The column name is field 4...

			s = rs.getString(4);
			if (!rs.wasNull())
			{
				s = s.Trim();
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, routine, "Table \"" + tableName + "\" has column \"" + s + "\"");
				}
				if (s.Equals(columnName, StringComparison.OrdinalIgnoreCase))
				{
					rs.close();
					return true;
				}
				//column_names.add ( s );
			}
			else
			{
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, routine, "Table \"" + tableName + "\" has null column");
				}
			}

			// Get the next item in the list...

			more = rs.next();
		}

		DMI.closeResultSet(rs);
		return false;
	}

	// TODO SAM 2012-05-06 Need to evaluate whether functions should be processed more but complex SQL should
	// probably just be constructed without the helper methods
	/// <summary>
	/// Escape an SQL field (column, table, or table with database and/or schema).
	/// This is necessary to ensure that fields that are reserved names do not get interpreted as such.
	/// For example, in SQL Server, [brackets] can be used around fields.
	/// If the input string contains a "(", then it is assumed that the field is specified using a function
	/// and no action is taken.
	/// If the input string contains a ".", then it is assumed that the database, schema, and/or table name precedes the
	/// field name and the string is split so that only the field name is escaped. </summary>
	/// <param name="dmi"> the DMI being used </param>
	/// <param name="field"> the field being escaped, can be "Column", "Table.Column", "Schema.Table.Column" or "Database.Schema.Table.Column"
	/// (or omit the column). </param>
	/// <returns> the escaped field </returns>
	public static string escapeField(DMI dmi, string field)
	{
		if ((field.IndexOf('(') >= 0) || (!"".Equals(dmi.getFieldLeftEscape())) && (field.IndexOf(dmi.getFieldLeftEscape(), StringComparison.Ordinal) >= 0))
		{
			// Function or already escaped so probably too complicated to figure out here
			return field;
		}
		else
		{
			// Escape the fields
			string[] parts = field.Split("\\.", true); // This is a regex; have to escape the '.'
			StringBuilder b = new StringBuilder();
			string le, re;
			foreach (string part in parts)
			{
				if (part.StartsWith(dmi.getFieldLeftEscape(), StringComparison.Ordinal))
				{
					// Already escaped so no need to do so again
					le = "";
					re = "";
				}
				else
				{
					le = dmi.getFieldLeftEscape();
					re = dmi.getFieldRightEscape();
				}
				if (b.Length > 0)
				{
					b.Append(".");
				}
				b.Append(le);
				b.Append(part);
				b.Append(re);
			}
			return b.ToString();
		}
	}

	/// <summary>
	/// Format a date/time string based on the database engine so that it can be used in an SQL statement. </summary>
	/// <param name="dmi"> DMI instance form which to format date. </param>
	/// <param name="datetime"> a DateTime object containing a date.  The precision of this
	/// DateTime object controls the formatting of the string. </param>
	/// <returns> a String representation of the DateTime, in the proper
	/// form for use with the specified database engine. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static String formatDateTime(DMI dmi, RTi.Util.Time.DateTime datetime) throws Exception
	public static string formatDateTime(DMI dmi, DateTime datetime)
	{
		return formatDateTime(dmi, datetime, true, -1);
	}

	/// <summary>
	/// Format a date/time string based on the database engine so that it can be used in an SQL statement. </summary>
	/// <param name="dmi"> DMI instance form which to format date. </param>
	/// <param name="datetime"> a DateTime object containing a date.  The precision of this
	/// DateTime object controls the formatting of the string. </param>
	/// <param name="escapeChar"> if true, the date will be wrapped with an escape character
	/// appropriate for the database engine. </param>
	/// <returns> a String representation of the DateTime, in the proper
	/// form for use with the specified database engine. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static String formatDateTime(DMI dmi, RTi.Util.Time.DateTime datetime, boolean escapeChar) throws Exception
	public static string formatDateTime(DMI dmi, DateTime datetime, bool escapeChar)
	{
		return formatDateTime(dmi, datetime, escapeChar, -1);
	}

	/// <summary>
	/// Format a date/time string based on the database engine so that it can be used in an SQL statement. </summary>
	/// <param name="dmi"> DMI instance form which to format date. </param>
	/// <param name="datetime"> a DateTime object containing a date.  The precision of this
	/// DateTime object controls the formatting of the string. </param>
	/// <param name="escapeChar"> if true, the date will be wrapped with an escape character
	/// appropriate for the database engine </param>
	/// <param name="DateTime.PRECISION_">* indicating the format of the output - if specified as -1, use the precision
	/// from the date/time </param>
	/// <returns> a String representation of the DateTime, in the proper
	/// form for use with the specified database engine. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static String formatDateTime(DMI dmi, RTi.Util.Time.DateTime datetime, boolean escapeChar, int precision) throws Exception
	public static string formatDateTime(DMI dmi, DateTime datetime, bool escapeChar, int precision)
	{
		string month = StringUtil.formatString(datetime.getMonth(),"%02d");
		string day = StringUtil.formatString(datetime.getDay(),"%02d");
		string year = StringUtil.formatString(datetime.getYear(),"%04d");
		string hour = null;
		string minute = null;
		string second = null;
		string databaseEngine = dmi.getDatabaseEngine();
		int databaseEngineType = dmi.getDatabaseEngineType();
		StringBuilder formatted = new StringBuilder();

		if (precision == -1)
		{
			// Default is to use precision from date/time
			precision = datetime.getPrecision();
		}
		if (precision <= DateTime.PRECISION_HOUR)
		{
			hour = StringUtil.formatString(datetime.getHour(),"%02d");
		}
		if (precision <= DateTime.PRECISION_MINUTE)
		{
			minute = StringUtil.formatString(datetime.getMinute(),"%02d");
		}
		if (precision <= DateTime.PRECISION_SECOND)
		{
			second = StringUtil.formatString(datetime.getSecond(),"%02d");
		}

		// There are just enough differences between database engines to make
		// reusing code difficult.  Just handle separately for each engine.

		if (databaseEngineType == DMI.DBENGINE_ACCESS)
		{
			// TODO How to handle month or year precision?
			if (escapeChar)
			{
				formatted.Append("#");
			}
			formatted.Append("" + month);
			formatted.Append("-");
			formatted.Append("" + day);
			formatted.Append("-");
			formatted.Append("" + year);
			if (precision <= DateTime.PRECISION_HOUR)
			{
				formatted.Append(" " + hour);
			}
			if (precision <= DateTime.PRECISION_MINUTE)
			{
				formatted.Append(":" + minute);
			}
			if (precision <= DateTime.PRECISION_SECOND)
			{
				formatted.Append(":" + second);
			}
			if (escapeChar)
			{
				formatted.Append("#");
			}
			return formatted.ToString();
		}
		else if (databaseEngineType == DMI.DBENGINE_INFORMIX)
		{
			// TODO Need to check the INFORMIX documentation for all the variations on this...
			if (escapeChar)
			{
				formatted.Append("DATETIME (");
			}
			formatted.Append("" + year);
			if (precision <= DateTime.PRECISION_MONTH)
			{
				formatted.Append("-" + month);
			}
			if (precision <= DateTime.PRECISION_DAY)
			{
				formatted.Append("-" + day);
			}
			if (precision <= DateTime.PRECISION_HOUR)
			{
				formatted.Append(" " + hour);
			}
			if (precision <= DateTime.PRECISION_MINUTE)
			{
				formatted.Append(":" + minute);
			}
			if (precision <= DateTime.PRECISION_SECOND)
			{
				formatted.Append(":" + second);
			}
			if (escapeChar)
			{
				formatted.Append(")");
			}
			return formatted.ToString();
		}
		//else if ( databaseEngineType == DMI.DBENGINE_ORACLE ) {
		//}
		else if (databaseEngineType == DMI.DBENGINE_SQLSERVER)
		{
			if (escapeChar)
			{
				formatted.Append("'");
			}
			formatted.Append("" + year);
			if (precision <= DateTime.PRECISION_MONTH)
			{
				formatted.Append("-" + month);
			}
			if (precision <= DateTime.PRECISION_DAY)
			{
				formatted.Append("-" + day);
			}
			if (precision <= DateTime.PRECISION_HOUR)
			{
				formatted.Append(" " + hour);
			}
			if (precision <= DateTime.PRECISION_MINUTE)
			{
				formatted.Append(":" + minute);
			}
			if (precision <= DateTime.PRECISION_SECOND)
			{
				formatted.Append(":" + second);
			}
			if (escapeChar)
			{
				formatted.Append("'");
			}
			return formatted.ToString();
		}
		else if ((databaseEngineType == DMI.DBENGINE_MYSQL) || (databaseEngineType == DMI.DBENGINE_POSTGRESQL))
		{
			// PostgreSQL datetimes must have at least year-month-day
			if (escapeChar)
			{
				formatted.Append("'");
			}
			formatted.Append("" + year);
			formatted.Append("-" + month);
			formatted.Append("-" + day);
			if (precision <= DateTime.PRECISION_HOUR)
			{
				formatted.Append(" " + hour);
			}
			if (precision <= DateTime.PRECISION_MINUTE)
			{
				formatted.Append(":" + minute);
			}
			if (precision <= DateTime.PRECISION_SECOND)
			{
				formatted.Append(":" + second);
			}
			if (escapeChar)
			{
				formatted.Append("'");
			}
			return formatted.ToString();
		}
		else
		{
			throw new Exception("Unsupported database type (" + databaseEngine + ") in formatDateTime()");
		}
	}

	/// <summary>
	/// Formats a where clause given the field side (in the whereString) and the 
	/// is side (in the isString), and the kind of value that should be stored in the isString(type). </summary>
	/// <param name="whereString"> field name (checks are not performed on this) </param>
	/// <param name="isString"> user-specified search criteria (checks are performed on this) </param>
	/// <param name="type"> the expected isString type (e.g., STRING, INT, ... etc) </param>
	/// <returns> a formatted where String if checks are satisfied, or null if an
	/// exception is thrown or 'NONE' if the format is not to be added as a where clause. </returns>
	/// <exception cref="Exception"> if an error occurs farther down the stack </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static String formatWhere(String whereString, String isString, int type) throws Exception
	public static string formatWhere(string whereString, string isString, int type)
	{
		// initialize variables
		string formatString = null;

		// trim where and is Strings
		whereString = whereString.Trim();
		isString = isString.Trim();

		// check first for "is null" and "is not null" as the isString
		if (isString.Equals("is null", StringComparison.OrdinalIgnoreCase) || isString.Equals("is not null", StringComparison.OrdinalIgnoreCase))
		{
			return whereString + " " + isString;
		}

		// check the isString and format an Integer type where clause
		if (type == StringUtil.TYPE_INTEGER || type == StringUtil.TYPE_DOUBLE || type == StringUtil.TYPE_FLOAT)
		{
			formatString = formatWhereNumber(whereString, isString, type);
		}
		else if (type == StringUtil.TYPE_STRING)
		{
			formatString = formatWhereString(whereString, isString);
		}

		return formatString;
	}

	/// <summary>
	/// Determines if the numString is the expected type. </summary>
	/// <param name="type"> the type that numString should be </param>
	/// <param name="numString"> the string to check </param>
	/// <returns> formatte where string  </returns>
	private static string formatWhereCheckNumber(int type, string numString)
	{
		string formatString = null;

		// Determine if the numString is of the expected type. IF not, throw an Exception.
		if (type == StringUtil.TYPE_INTEGER)
		{
			int? isInteger = Convert.ToInt32(numString);
			formatString = isInteger.ToString();
		}
		else if (type == StringUtil.TYPE_DOUBLE || type == StringUtil.TYPE_FLOAT)
		{
			double? isDouble = Convert.ToDouble(numString);
			formatString = isDouble.ToString();
		}
		return formatString;
	}

	/// <summary>
	/// Formats the integer, double and float type where clauses. </summary>
	/// <param name="whereString"> field name (checks are not performed on this) </param>
	/// <param name="isString"> user-specified search criteria (checks are performed on this) </param>
	/// <param name="type"> one of INT, DOUBLE or FLOAT (static values defined in this class) </param>
	/// <returns> a formatted where string if the checks are satisfied, null if an 
	/// exception is thrown, or 'NONE' if the format is not to be added as a where clause.
	/// TODO (JTS 2003-03-04) Maybe rework so instead of returning null, returns an exception? </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static String formatWhereNumber(String whereString, String isString, int type) throws Exception
	private static string formatWhereNumber(string whereString, string isString, int type)
	{
		if (string.ReferenceEquals(isString, null))
		{
			return "NONE";
		}

		// first check if this field is being queried for everything ('*' is
		// treated, for RTi's purposes, as a valid numeric query).  If so,
		// just return "NONE" so that this isn't added as a where clause.
		if (isString.Trim().Equals("*"))
		{
			return "NONE";
		}

		string function = "DMIUtil.formatWhereNumber";
		string formatString = null;
		string firstOperator = null;
		string secondOperator = null;
		string isNumber = null;
		string numberString = "";
		string message = "";
		string betweenForNumber = "";
		string exampleMessage = "";

		if (type == StringUtil.TYPE_INTEGER)
		{
			message = " is not a valid search criteria. Examples are:";
			exampleMessage = "\nBETWEEN 500 AND 550"
					+ "\n= 550"
					+ "\n<= 550"
					+ "\nis null"
					+ "\nis not null"
					+ "\n* (returns all records)";
			message = message + exampleMessage;
			betweenForNumber = "BETWEEN 12 AND 56";
		}
		else if (type == StringUtil.TYPE_DOUBLE || type == StringUtil.TYPE_FLOAT)
		{
			message = " is not a valid search criteria. Examples are";
			exampleMessage = "\nBETWEEN 12.34 AND 56.78"
					+ "\n= 1234.56"
					+ "\n<= 1345.56"
					+ "\nis null"
					+ "\nis not null"
					+ "\n* (returns all records)";
			message = message + exampleMessage;
			betweenForNumber = "BETWEEN 12.34 AND 56.78";
		}

		// isString cannot be empty. The user must supply a search criteria.
		if (isString.Length == 0)
		{
			Message.printWarning(2, function, "You must select an \"Is\"" + " query criteria. Examples are:" + exampleMessage);
			return null;
		}

		// check for BETWEEN searches
		if (isString.StartsWith("BETWEEN", StringComparison.Ordinal))
		{
			IList<string> v = StringUtil.breakStringList(isString, " ", 0);

			if (((string)v[0]).Equals("BETWEEN", StringComparison.OrdinalIgnoreCase))
			{
				// If the query is using BETWEEN searches, then it MUST
				// adhere to the following format:
				// elementAt(0) = "BETWEEN"
				// elementAt(1) = Integer or Double
				// elementAt(2) = "AND"
				// elementAt(3) = Integer or Double
				// any other BETWEEN format is NOT accepted
				// try will catch Vector out of bounds
				try
				{
					// try will catch number format for the first value
					try
					{
						formatWhereCheckNumber(type,(string)v[1]);
					}
					catch (Exception)
					{
						Message.printWarning(2, function, (string)v[1] + message);
						return null;
					}

					// determine if AND is present
					try
					{
						string andString = (string)v[2];
						if (!andString.Equals("AND", StringComparison.OrdinalIgnoreCase))
						{
							Message.printWarning(2,function, "Missing 'AND' in the search criteria." + "\nBETWEEN searches must be specified as follows:\n" + betweenForNumber);
							return null;
						}
					}
					catch (System.IndexOutOfRangeException)
					{
						Message.printWarning(2, function, "Missing 'AND' in the search criteria.\nBETWEEN searches " + "must be specified as follows:\n" + betweenForNumber);
						return null;
					}

					// try will catch number format for the second value
					try
					{
						formatWhereCheckNumber(type, (string)v[3]);
					}
					catch (Exception)
					{
						Message.printWarning(2, function, (string)v[3] + message);
						return null;
					}

					// Make sure that no more than 4 elements are present in the list
					if (v.Count > 4)
					{
						Message.printWarning(2, function, "Too many terms specified in the search criteria." + "\nBETWEEN searches must be specified as follows:" + "\n" + betweenForNumber);
					}

					formatString = " " + (string)v[0] + " " + (string)v[1] + " " + (string)v[2] + " " + (string)v[3];

					return whereString + formatString;
				}
				catch (System.IndexOutOfRangeException)
				{
					Message.printWarning(2, function, "BETWEEN searches must be specified as follows:" + "\n" + betweenForNumber);
					return null;
				}
			}
		}

		// if the isString can successfully be decoded into a number
		// then no operators where provided. Default to placing an '=' 
		// in front of the number.
		try
		{
			isNumber = formatWhereCheckNumber(type, isString);
			formatString = " = " + isNumber.ToString();
		}

		// could not successfully decode the isString into the 
		// expected number type. check to see if an operator(s)or 
		// other characters prevented this.
		catch (Exception)
		{
			// this loop will build a numberString without the 
			// operators so that we can decode the remaining 
			// characters to ensure that a valid number type exist. 
			// The operator(s)are concatenated with the numberString 
			// if all the checks are passed.
			int isLength = isString.Length;
			for (int currIndex = 0; currIndex < isLength; currIndex++)
			{
				string currChar = isString[currIndex].ToString().Trim();

				// Check the first character which MUST be an operator. If not, throw an exception.
				if (currIndex == 0)
				{
					// if the first character is not '=', 
					// '<', or '>' then throw an exception
					if (!(currChar.Equals("=")) && !(currChar.Equals("<")) && !(currChar.Equals(">")))
					{
						try
						{
							formatWhereCheckNumber(type, isString);
						}
						catch (Exception)
						{
							Message.printWarning(2, function, isString + message);
							return null;
						}
					}
					else
					{
						firstOperator = currChar;
					}
				}
				// Check the second character which CAN be an operator if the first operator was '<' or '>'
				else if (currIndex == 1)
				{
					// determine if a second operator exist
					if (firstOperator.Equals("<") || firstOperator.Equals(">"))
					{
						if (currChar.Equals("="))
						{
							secondOperator = currChar;
						}
						// build the number String
						else
						{
							numberString = currChar;
						}
					}
					// build the number String
					else
					{
						numberString = currChar;
					}
				}
				// build the number String
				else
				{
					numberString += currChar;
				}
			}

			// decode the numberString to determine if it is the expected number type
			try
			{
				isNumber = formatWhereCheckNumber(type, numberString);
				// numberString was successfully decoded.  Build the concatenated formatString
				if (!string.ReferenceEquals(secondOperator, null))
				{
					formatString = " " + firstOperator + secondOperator + " " + isNumber.ToString();
				}
				else
				{
					formatString = " " + firstOperator + " " + isNumber.ToString();
				}
			}
			// Could not successfully decode the numberString.
			// issue a warning.
			catch (Exception)
			{
				Message.printWarning(2, function, isString + message);
				return null;
			}
		}

		return whereString + formatString;
	}

	/// <summary>
	/// Formats the String type where clauses. </summary>
	/// <param name="whereString"> field name (checks are not done on this) </param>
	/// <param name="isString"> user-specified search criteria (checks are performed on this) </param>
	/// <returns> formatted where String if checks are satisfied, null otherwise.
	/// TODO (JTS 2003-03-04)  Maybe throw an exception if the checks aren't satisfied? </returns>
	private static string formatWhereString(string whereString, string isString)
	{
		// initialize variables
		string function = "DMIUtil.formatWhereString()";
		string formatString = null;
		string likeString = null;
		bool foundLike = false;
		string remainingString = null;

		string exampleMessage = "\nLike COLORADO"
			+ "\n= COLORADO"
			+ "\nis null"
			+ "\nis not null"
			+ "\n*(returns all records)"
			+ "\n\nNote:  Strings are converted to uppercase "
			+ "for queries.";

		// IsString cannot be empty. The user must supply a search criteria.
		if (isString.Length == 0)
		{
			Message.printWarning(2, function, "You must select an \"Is\"" + " query criteria. Examples are:" + exampleMessage);
			return null;
		}

		// Replace wild cards using * with the database's wildcard character.
		string replacedString = isString;
		try
		{
			// char c = DMI._wildcard.charAt(0);
			replacedString = replacedString.Replace('*', '%');
		}
		catch (Exception e)
		{
			// do nothing, _wildcard's length was < 1
			Console.WriteLine(e.ToString());
			Console.Write(e.StackTrace);
		}

		// check for single quotes. if present issue warning and return null.
		int isLength = isString.Length;
		if (checkSingleQuotes(isString))
		{
			Message.printWarning(2, function, "Single quotes are not permitted in queries. ");
			return null;
		}

		// If the isString is a '*' or '%' then return a NONE so that this will
		// not be added as a where clause. Issue a warning that ALL records will
		// be returned.
			//if (isString.equals("*") || isString.equals(DMI._wildcard)) {
	//        if (isString.equals("*") || isString.equals("%")) {
	//		return "NONE";
	//	}

		// Determine if the replacedString begins with an '=' or 'Like' if not,
		// we will supply the 'Like' syntax. recall that the replacedString
		// has been trimmed so it should begin with one of the following.
		isLength = isString.Length;
		if (replacedString.Substring(0, 1).Equals("=", StringComparison.OrdinalIgnoreCase))
		{
			likeString = "=";
			remainingString = replacedString.Substring(1).Trim();
			foundLike = true;
		}
		else if (isLength > 3)
		{
			if (replacedString.Substring(0, 4).Equals("like", StringComparison.OrdinalIgnoreCase))
			{
				likeString = "LIKE";
				remainingString = replacedString.Substring(4).Trim();
				foundLike = true;
			}
		}
		// user did not supply 'Like' or '=' therefore default to 'Like'
		if (!(foundLike))
		{
			likeString = "LIKE";
			remainingString = replacedString.Substring(0).Trim();
		}

		// Check to ensure that the remaining String begins and ends with '
		// add the missing ' at the beginning of remainingString
		if (!(remainingString.StartsWith("'", StringComparison.Ordinal)))
		{
			remainingString = "'" + remainingString;
		}
		// add the missing ' at the end of remainingString
		if (!(remainingString.EndsWith("'", StringComparison.Ordinal)))
		{
			remainingString = remainingString + "'";
		}

		// Loop over the remainingString to provide internal checks within the
		// quotes
		isLength = remainingString.Length;

		formatString = " " + likeString + " " + remainingString;
		return whereString + formatString;
	}

	/// <summary>
	/// Constructs a concatenated String with AND inserted at the appropriate locations. </summary>
	/// <param name="and"> Vector of Strings in which to construct an AND clause </param>
	/// <returns> returns a String with AND inserted at the appropriate locations, null if "and" is null </returns>
	public static string getAndClause(IList<string> and)
	{
			if (and == null)
			{
					return null;
			}

			int size = and.Count;
			string andString = "";

			for (int i = 0; i < size; i++)
			{
				if ((i != (size - 1)) && (size != 1))
				{
					andString += and[i] + " AND ";
				}
				else
				{
					andString += and[i];
				}
			}
			return "(" + andString + ")";
	}

	/// <summary>
	/// Returns all the kinds of databases a DMI can connect to. </summary>
	/// <returns> a list of all the kinds of databases a DMI can connect to. </returns>
	public static IList<string> getAllDatabaseTypes()
	{
		return DMI.getAllDatabaseTypes();
	}

	/// <summary>
	/// Return the list of columns for a table. </summary>
	/// <returns> the list of columns for a table. </returns>
	/// <param name="dmi"> DMI for connection. </param>
	/// <param name="tableName"> Name of table. </param>
	/// <exception cref="if"> there is an error getting database information. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<String> getTableColumns(DMI dmi, String tableName) throws Exception, java.sql.SQLException
	public static IList<string> getTableColumns(DMI dmi, string tableName)
	{
		return getTableColumns(dmi.getDatabaseMetaData(), tableName);
	}

	/// <summary>
	/// Return the list of column names for a table. </summary>
	/// <returns> the list of columns for a table. </returns>
	/// <param name="metadata"> DatabaseMetaData for connection. </param>
	/// <param name="tableName"> Name of table. </param>
	/// <exception cref="if"> there is an error getting database information. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<String> getTableColumns(java.sql.DatabaseMetaData metadata, String tableName) throws Exception, java.sql.SQLException
	public static IList<string> getTableColumns(DatabaseMetaData metadata, string tableName)
	{
		string message, routine = "DMI.getTableColumns";
		ResultSet rs = null;

		// The following can be used to get a full list of columns...
		try
		{
			rs = metadata.getColumns(null, null, tableName, null);
			if (rs == null)
			{
				message = "Error getting columns for \"" + tableName + "\" table.";
				Message.printWarning(2, routine, message);
				throw new Exception(message);
			}
		}
		catch (Exception)
		{
			message = "Error getting database information for table \"" + tableName + "\".";
			Message.printWarning(2, routine, message);
			throw new Exception(message);
		}

		string columnName;
		IList<string> columnNames = new List<string>();
		while (rs.next())
		{
			try
			{
				// Column name...
				columnName = rs.getString(4);
				if (!rs.wasNull())
				{
					columnNames.Add(columnName.Trim());
				}
			}
			catch (Exception e)
			{
				// continue getting the list of table names, but report the error.
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
		return columnNames;
	}

	/// <summary>
	/// Return the foreign table and column name corresponding to table and column that have a foreign key defined. </summary>
	/// <returns> the foreign table and column name corresponding to table and column that have a foreign key defined. </returns>
	/// <param name="dmi"> DMI for connection. </param>
	/// <param name="tableName"> Name of table. </param>
	/// <exception cref="if"> there is an error getting database information. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static String [] getTableForeignKeyTableAndColumn(DMI dmi, String tableName, String columnName) throws Exception, java.sql.SQLException
	public static string [] getTableForeignKeyTableAndColumn(DMI dmi, string tableName, string columnName)
	{
		return getTableForeignKeyTableAndColumn(dmi.getDatabaseMetaData(), tableName, columnName);
	}

	/// <summary>
	/// Return the list of columns that are a primary key for a table. </summary>
	/// <returns> the list of columns that are a primary key for a table. </returns>
	/// <param name="metadata"> DatabaseMetaData for connection </param>
	/// <param name="tableName"> name of table that has a foreign key to another table </param>
	/// <param name="columnName"> name of column in tableName that is the foreign key to another table </param>
	/// <exception cref="if"> there is an error getting database information. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static String [] getTableForeignKeyTableAndColumn(java.sql.DatabaseMetaData metadata, String tableName, String columnName) throws Exception, java.sql.SQLException
	public static string [] getTableForeignKeyTableAndColumn(DatabaseMetaData metadata, string tableName, string columnName)
	{
		string message, routine = "DMIUtil.getTableForeignKeyTableAndColumn";
		ResultSet rs = null;

		try
		{
			rs = metadata.getImportedKeys(null, null, tableName);
			//rs = metadata.getExportedKeys( null, null, tableName);
			if (rs == null)
			{
				message = "Error getting foreign keys for \"" + tableName + "\" table.";
				Message.printWarning(2, routine, message);
				throw new Exception(message);
			}
		}
		catch (Exception)
		{
			message = "Error getting foreign keys for table \"" + tableName + "\".";
			Message.printWarning(2, routine, message);
			throw new Exception(message);
		}

		string mainColumn = null;
		//String mainTable = null;
		string foreignTable = null;
		string foreignColumn = null;
		string[] data = null;
		while (rs.next())
		{
			try
			{
				// Foreign table...
				foreignTable = rs.getString(3);
				foreignColumn = rs.getString(4);
				// The table initiating the request (the ones that has keys pointing to the foreign table)...
				//mainTable = rs.getString(7);
				mainColumn = rs.getString(8);
				if (columnName.Equals(mainColumn, StringComparison.OrdinalIgnoreCase))
				{
					// Found the matching requested column
					data = new string[2];
					data[0] = foreignTable;
					data[1] = foreignColumn;
					break;
				}
				//Message.printStatus(2, routine, "For requested table \"" + mainTable + "\" requested column \"" + mainColumn +
				//    "\", foreignTable=\"" + foreignTable + "\" foreignColumn=\"" + foreignColumn + "\"");
			}
			catch (Exception e)
			{
				// continue getting the list of table names, but report the error.
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
		return data;
	}

	/// <summary>
	/// Return the table column names that are for primary keys. </summary>
	/// <returns> the table column names that are for primary keys. </returns>
	/// <param name="dmi"> DMI for connection. </param>
	/// <param name="tableName"> Name of table. </param>
	/// <exception cref="if"> there is an error getting database information. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<String> getTablePrimaryKeyColumns(DMI dmi, String tableName) throws Exception, java.sql.SQLException
	public static IList<string> getTablePrimaryKeyColumns(DMI dmi, string tableName)
	{
		return getTablePrimaryKeyColumns(dmi.getDatabaseMetaData(), tableName);
	}

	/// <summary>
	/// Return the list of columns that are a primary key for a table. </summary>
	/// <returns> the list of columns that are a primary key for a table. </returns>
	/// <param name="metadata"> DatabaseMetaData for connection. </param>
	/// <param name="tableName"> Name of table. </param>
	/// <exception cref="if"> there is an error getting database information. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<String> getTablePrimaryKeyColumns(java.sql.DatabaseMetaData metadata, String tableName) throws Exception, java.sql.SQLException
	public static IList<string> getTablePrimaryKeyColumns(DatabaseMetaData metadata, string tableName)
	{
		string message, routine = "DMI.getTablePrimaryKeyColumns";
		ResultSet rs = null;

		try
		{
			rs = metadata.getPrimaryKeys(null, null, tableName);
			if (rs == null)
			{
				message = "Error getting primary keys for \"" + tableName + "\" table.";
				Message.printWarning(2, routine, message);
				throw new Exception(message);
			}
		}
		catch (Exception)
		{
			message = "Error getting primary keys for table \"" + tableName + "\".";
			Message.printWarning(2, routine, message);
			throw new Exception(message);
		}

		string columnName;
		IList<string> columnNames = new List<string>();
		while (rs.next())
		{
			try
			{
				// Column name...
				columnName = rs.getString(4);
				if (!rs.wasNull())
				{
					columnNames.Add(columnName.Trim());
				}
			}
			catch (Exception e)
			{
				// continue getting the list of table names, but report the error.
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
		return columnNames;
	}

	/// <summary>
	/// Returns the catalog of database names in the connection, excluding known system databases.
	/// For example, if the JDBC connection is with a SQL Server "master" database, this will return the databases
	/// under that instance. </summary>
	/// <param name="dmi"> an open, connected and not-null DMI connection to a database. </param>
	/// <param name="removeSystemDatabases"> if true, remove the known system databases (this may take some work to keep up to date). </param>
	/// <param name="notIncluded"> a list of all the database names that should not be included
	/// in the final list of database names. </param>
	/// <returns> the list of databases names in the dmi's database instance.  null
	/// is returned if there was an error reading from the database. </returns>
	public static IList<string> getDatabaseCatalogNames(DMI dmi, bool removeSystemDatabases, IList<string> notIncluded)
	{
		string routine = "getDatabaseCatalogNames";
		// Get the name of the data.  If the name is null, it's most likely
		// because the connection is going through ODBC, in which case the 
		// name of the ODBC source will be used.
		string dbName = dmi.getDatabaseName();
		if (string.ReferenceEquals(dbName, null))
		{
			dbName = dmi.getODBCName();
		}

		Message.printStatus(2, routine, "Getting catalog of databases");
		ResultSet rs = null;
		DatabaseMetaData metadata = null;
		try
		{
			metadata = dmi.getConnection().getMetaData();
			rs = metadata.getCatalogs();
			if (rs == null)
			{
				Message.printWarning(2, routine, "Error getting catalog of databases.");
				return null;
			}
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, "Error getting catalog of databases.");
			Message.printWarning(2, routine, e);
			return null;
		}

		string catName;
		IList<string> dbNames = new List<string>();
		try
		{
			while (rs.next())
			{
				try
				{
					// Table name...
					catName = rs.getString(1);
					if (rs.wasNull())
					{
						catName = null;
					}
					if (!string.ReferenceEquals(catName, null))
					{
						dbNames.Add(catName.Trim());
					}
				}
				catch (Exception e)
				{
					// continue getting the list of table names, but report the error.
					Message.printWarning(2, routine, e);
				}
			}
		}
		catch (Exception)
		{
			// TODO SAM 2012-01-31 probably should not catch this but has been done historically
		}
		try
		{
			DMI.closeResultSet(rs);
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, e);
		}

		// Sort the list of database names in ascending order, ignoring case.
		dbNames = StringUtil.sortStringList(dbNames, StringUtil.SORT_ASCENDING, null, false, true);

		// Additionally remove all the databases that were in the notIncluded parameter passed in to this method.
		if (notIncluded != null)
		{
			Message.printStatus(2, routine, "Removing requested databases from database list");
			foreach (string s in notIncluded)
			{
				StringUtil.removeMatching(dbNames,s,true);
			}
		}
		return dbNames;
	}

	/// <summary>
	/// Returns the list of procedure names in the database, excluding known system procedures. </summary>
	/// <param name="dmi"> an open, connected and not-null DMI connection to a database. </param>
	/// <param name="removeSystemProcedures"> if true, remove the known system procedures
	/// (this may take some work to keep up to date). </param>
	/// <param name="notIncluded"> a list of all the procedure names that should not be included
	/// in the final list of procedure names. </param>
	/// <returns> the list of procedure names in the dmi's database.  null
	/// is returned if there was an error reading from the database. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<String> getDatabaseProcedureNames(DMI dmi, boolean removeSystemProcedures, java.util.List<String> notIncluded) throws java.sql.SQLException
	public static IList<string> getDatabaseProcedureNames(DMI dmi, bool removeSystemProcedures, IList<string> notIncluded)
	{
		string routine = "getDatabaseProcedureNames";
		int dl = 25;
		// Get the name of the data.  If the name is null, it's most likely
		// because the connection is going through ODBC, in which case the 
		// name of the ODBC source will be used.
		string dbName = dmi.getDatabaseName();
		if (string.ReferenceEquals(dbName, null))
		{
			dbName = dmi.getODBCName();
		}

		IList<string> procNames = new List<string>();
		Message.printStatus(2, routine, "Getting list of procedures");
		ResultSet rs = null;
		DatabaseMetaData metadata = null;
		int databaseEngineType = dmi.getDatabaseEngineType();
		try
		{
			metadata = dmi.getConnection().getMetaData();
			Message.printStatus(2,routine,"Database " + dbName + " supports catalogs in procedure calls:" + metadata.supportsCatalogsInProcedureCalls());
			if (!metadata.supportsStoredProcedures())
			{
				// Return empty list
				Message.printStatus(2, routine, "Database " + dbName + " driver does not support procedures");
				return procNames;
			}
			if (databaseEngineType == DMI.DBENGINE_SQLSERVER)
			{
				// TODO SAM 2016-03-25 Seems like this does not work the same but cannot get it to work
				rs = metadata.getProcedures(dbName, null, null);
			}
			else
			{
				rs = metadata.getProcedures(dbName, null, null);
			}
			if (rs == null)
			{
				Message.printWarning(2, routine, "Null result set getting procedure names.");
				return null;
			}
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, "Error getting list of procedures.  Aborting. uniquetempvar.");
			Message.printWarning(2, routine, e);
			return null;
		}

		while (rs.next())
		{
			string proc = rs.getString("PROCEDURE_NAME");
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "Procedure name to check = \"" + proc + "\"");
			}
			// The SQL Server driver may return the procedure name with ";" and a number at the end.  If so
			// strip it off for further processing.
			int pos = proc.IndexOf(";", StringComparison.Ordinal);
			if (pos > 0)
			{
				proc = proc.Substring(0,pos);
			}
			procNames.Add(proc);
		}
		// Strip out system stored procedures that a user should not run.
		// TODO SAM 2013-04-10 Need to figure out a more elegant way to do this.
		string[] systemProcPatternsToRemove = new string[0];
		if (databaseEngineType == DMI.DBENGINE_SQLSERVER)
		{
			string[] systemProcPatternsToRemove0 = new string[] {"dm.*", "dt.*", "fn.*", "sp.*", "xp.*"};
			systemProcPatternsToRemove = systemProcPatternsToRemove0;
		}
		if (removeSystemProcedures)
		{
			Message.printStatus(2, routine, "Removing system procedures from procedure list");
			for (int i = 0; i < systemProcPatternsToRemove.Length; i++)
			{
				StringUtil.removeMatching(procNames,systemProcPatternsToRemove[i],true);
			}
		}
		// Additionally remove all the tables that were in the notIncluded parameter passed in to this method.
		if (notIncluded != null)
		{
			Message.printStatus(2, routine, "Removing requested procedures from procedure list");
			foreach (string s in notIncluded)
			{
				StringUtil.removeMatching(procNames,s,true);
			}
		}
		// Using the following will close the related statement, which causes a problem on the 2nd call
		// to this method.
		//DMI.closeResultSet(rs);
		rs.close();
		return procNames;
	}

	/// <summary>
	/// Returns the catalog of schema names for the connection, excluding known system schemas.
	/// For example, if the JDBC connection is with a SQL Server "master" database, this will return the databases
	/// under that instance. </summary>
	/// <param name="dmi"> an open, connected and not-null DMI connection to a database. </param>
	/// <param name="catalog"> the catalog </param>
	/// <param name="removeSystemSchemas"> if true, remove the known system databases (this may take some work to keep up to date). </param>
	/// <param name="notIncluded"> a list of all the database names that should not be included
	/// in the final list of database names. </param>
	/// <returns> the list of databases names in the dmi's database instance.  null
	/// is returned if there was an error reading from the database. </returns>
	public static IList<string> getDatabaseSchemaNames(DMI dmi, string catalog, bool removeSystemSchemas, IList<string> notIncluded)
	{
		string routine = "getDatabaseSchemaNames";
		// Get the name of the data.  If the name is null, it's most likely
		// because the connection is going through ODBC, in which case the 
		// name of the ODBC source will be used.
		string dbName = dmi.getDatabaseName();
		if (string.ReferenceEquals(dbName, null))
		{
			dbName = dmi.getODBCName();
		}

		Message.printStatus(2, routine, "Getting schemas");
		ResultSet rs = null;
		DatabaseMetaData metadata = null;
		try
		{
			metadata = dmi.getConnection().getMetaData();
			// TODO SAM 2013-07-22 SQL Server does not implement completely so just brute force below
			//if ( dmi.getDatabaseEngineType() == DMI.DBENGINE_SQLSERVER ) {
			//    // Have to make the call without the catalog...
				rs = metadata.getSchemas();
			//}
			//else {
			//    rs = metadata.getSchemas(catalog,null);
			//}
			if (rs == null)
			{
				Message.printWarning(3, routine, "Error getting schemas.");
				return null;
			}
		}
		catch (Exception e)
		{
			Message.printWarning(3, routine, "Error getting schemas.");
			Message.printWarning(3, routine, e);
			return null;
		}

		string schema, cat;
		IList<string> schemas = new List<string>();
		try
		{
			while (rs.next())
			{
				try
				{
					// Schema name...
					schema = rs.getString(1);
					if (!rs.wasNull())
					{
						if ((string.ReferenceEquals(catalog, null)) || (dmi.getDatabaseEngineType() == DMI.DBENGINE_SQLSERVER))
						{
							schemas.Add(schema.Trim());
						}
						else
						{
							// Check the catalog before adding
							// TODO SAM 2013-07-22 SQL Server does not seem to return catalog
							cat = rs.getString(2);
							Message.printStatus(2,routine,"schema=" + schema + " cat=" + cat);
							if (!rs.wasNull())
							{
								// Have catalog
								if (catalog.Equals(cat, StringComparison.OrdinalIgnoreCase))
								{
									schemas.Add(schema.Trim());
								}
							}
						}
					}
				}
				catch (Exception e)
				{
					// continue getting the list of schemas, but report the error.
					Message.printWarning(3, routine, "Error getting schema information (" + e + ").");
					Message.printWarning(3, routine, e);
				}
			}
		}
		catch (Exception)
		{
			// TODO SAM 2012-01-31 probably should not catch this but has been done historically
		}
		try
		{
			DMI.closeResultSet(rs);
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, e);
		}

		// Sort the list of schemas in ascending order, ignoring case.
		schemas = StringUtil.sortStringList(schemas, StringUtil.SORT_ASCENDING, null, false, true);

		// Additionally remove all the schemas that were in the notIncluded parameter passed in to this method.
		if (notIncluded != null)
		{
			Message.printStatus(2, routine, "Removing requested schemas from schema list");
			foreach (string s in notIncluded)
			{
				StringUtil.removeMatching(schemas,s,true);
			}
		}
		return schemas;
	}

	/// <summary>
	/// Returns the list of table names in the database, excluding known system tables. </summary>
	/// <param name="dmi"> an open, connected and not-null DMI connection to a database. </param>
	/// <param name="catalog"> if not null, a database name under a main database (such as database under SQL Server master
	/// database that corresponds to the connection). </param>
	/// <param name="schema"> if not null, the schema for a database (such as used by Oracle) </param>
	/// <param name="removeSystemTables"> if true, remove the known system tables (this may take some work to keep up to date). </param>
	/// <param name="notIncluded"> a list of all the table names that should not be included
	/// in the final list of table names. </param>
	/// <returns> the list of table names in the dmi's database.  null
	/// is returned if there was an error reading from the database. </returns>
	public static IList<string> getDatabaseTableNames(DMI dmi, string catalog, string schema, bool removeSystemTables, IList<string> notIncluded)
	{
		string routine = "getDatabaseTableNames";
		// Get the name of the data.  If the name is null, it's most likely
		// because the connection is going through ODBC, in which case the 
		// name of the ODBC source will be used.
		string dbName = dmi.getDatabaseName();
		if (string.ReferenceEquals(dbName, null))
		{
			dbName = dmi.getODBCName();
		}

		Message.printStatus(2, routine, "Getting list of tables");
		ResultSet rs = null;
		DatabaseMetaData metadata = null;
		int databaseEngineType = dmi.getDatabaseEngineType();
		try
		{
			metadata = dmi.getConnection().getMetaData();
			string[] typeArray = new string[] {"TABLE", "VIEW"};
			if (databaseEngineType == DMI.DBENGINE_SQLSERVER)
			{
				// SQL Server does not seem to recognize the type array so get all and then filter below
				typeArray = null;
			}
			rs = metadata.getTables(catalog, schema, null, typeArray);
			if (rs == null)
			{
				Message.printWarning(2, routine, "Error getting list of tables.  Aborting");
				return null;
			}
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, "Error getting list of tables.  Aborting.");
			Message.printWarning(2, routine, e);
			return null;
		}

		// Loop through the result set and pull out the list of
		// all the table names and the table remarks.  
		Message.printStatus(2, routine, "Building table name list");
		string tableName;
		string tableType;
		IList<string> tableNames = new List<string>();
		try
		{
			while (rs.next())
			{
				try
				{
					// Table name...
					tableName = rs.getString(3);
					if (rs.wasNull())
					{
						tableName = null;
					}
					else
					{
						if (databaseEngineType == DMI.DBENGINE_ORACLE)
						{
							if (tableName.StartsWith("/", StringComparison.Ordinal))
							{
								// See large number of tables with names like "/f1892dbb_LogicalBasicNetwork", type is "SYNONYM"
								continue;
							}
						}
					}
					// Table type...
					tableType = rs.getString(4);
					if (rs.wasNull())
					{
						tableType = null;
					}
					if (Message.isDebugOn)
					{
						Message.printDebug(10, routine, "Table \"" + tableName + "\" type is \"" + tableType + "\"");
					}
					if (removeSystemTables)
					{
						// TODO SAM 2012-01-31 Should perhaps be able to check for system tables here but SQL
						// Server seems to call everything "TABLE" or "VIEW"
						if (!tableType.Equals("TABLE", StringComparison.OrdinalIgnoreCase) && !tableType.Equals("VIEW", StringComparison.OrdinalIgnoreCase))
						{
							tableName = null;
						}
					}
					if (!string.ReferenceEquals(tableName, null))
					{
						tableNames.Add(tableName.Trim());
					}
				}
				catch (Exception e)
				{
					// continue getting the list of table names, but report the error.
					Message.printWarning(2, routine, e);
				}
			}
		}
		catch (Exception)
		{
			// TODO SAM 2012-01-31 probably should not catch this but has been done historically
		}
		try
		{
			DMI.closeResultSet(rs);
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, e);
		}

		// Sort the list of table names in ascending order, ignoring case.
		tableNames = StringUtil.sortStringList(tableNames, StringUtil.SORT_ASCENDING, null, false, true);

		// Remove the list of system tables for each kind of database 
		// (all database types have certain system tables)
		// TODO SAM 2012-01-31 Should be able to do from metadata but SQL Server does not indicate system tables.
		string[] systemTablePatternsToRemove = getSystemTablePatternsToRemove(dmi.getDatabaseEngineType());

		if (removeSystemTables)
		{
			Message.printStatus(2, routine, "Removing system tables from table list");
			for (int i = 0; i < systemTablePatternsToRemove.Length; i++)
			{
				StringUtil.removeMatching(tableNames,systemTablePatternsToRemove[i],true);
			}
		}

		// Additionally remove all the tables that were in the notIncluded parameter passed in to this method.
		if (notIncluded != null)
		{
			Message.printStatus(2, routine, "Removing requested tables from table list");
			foreach (string s in notIncluded)
			{
				StringUtil.removeMatching(tableNames,s,true);
			}
		}
		return tableNames;
	}

	/// <summary>
	/// Return a list of String containing defined ODBC Data Source Names.
	/// This method is only applicable on Windows operating systems.  The windows
	/// registry for "HKEY_CURRENT_USER: Software\ODBC\ODBC.INI\ODBC Data Sources" is
	/// read using the external shellcon.exe program.  This program must therefore be in the path. </summary>
	/// <returns> a list of String containing defined ODBC Data Source Names.  The
	/// list may be empty. </returns>
	/// <param name="strip_general"> If true, strip general ODBC DSNs from the list (e.g., "Excel Files"). </param>
	public static IList<string> getDefinedOdbcDsn(bool strip_general)
	{
		IList<string> output = null;
		if (!IOUtil.isUNIXMachine())
		{
			try
			{
				string[] command_array = new string[2];
				command_array[0] = "shellcon";
				command_array[1] = "-dsn";
				ProcessManager pm = new ProcessManager(command_array);
				pm.saveOutput(true);
				pm.run();
				output = pm.getOutputList();
				//Message.printStatus ( 2, routine,
				//"Exit status from shellcon for ODBC is: " +
				//pm.getExitStatus() );
				// Finish the process...
				pm = null;
			}
			catch (Exception e)
			{
				// Won't work if running as an Applet!
				Message.printWarning(2, "DMIUtil.getDefinedOdbcDsn",e);
				output = null;
			}
		}

		IList<string> available_OdbcDsn = new List<string>();
		if ((output != null) && (output.Count > 0))
		{
			output = StringUtil.sortStringList(output, StringUtil.SORT_ASCENDING, null, false, true);
			int size = output.Count;
			string odbc = "";
			for (int i = 0; i < size; i++)
			{
				odbc = ((string)output[i]).Trim();
				if (strip_general && (odbc.regionMatches(true,0,"dBASE Files",0,11) || odbc.regionMatches(true,0,"Excel Files",0,11) || odbc.regionMatches(true,0,"FoxPro Files",0,12) || odbc.regionMatches(true,0,"MS Access Database",0,18) || odbc.regionMatches(true,0,"MQIS",0,4) || odbc.regionMatches(true,0,"Visual FoxPro",0,13)))
				{
					continue;
				}
				available_OdbcDsn.Add(odbc);
			}
		}
		return available_OdbcDsn;
	}

	/// <summary>
	/// This function determines the extreme integer value of the specified field 
	/// from the requested table via using max(field) or min(field). </summary>
	/// <param name="dmi"> the DMI to use. </param>
	/// <param name="table"> table name. </param>
	/// <param name="field"> table field to determine the max value of. </param>
	/// <param name="flag"> "MAX" or "MIN" depending upon which extreme is desired. </param>
	/// <returns> returns a int of the extreme record or DMIUtil.MISSING_INT if an error occurred. </returns>
	private static int getExtremeRecord(DMI dmi, string table, string field, string flag)
	{
		try
		{
			string query = "select " + flag + "(" + field.Trim() + ") from " + table.Trim();
			ResultSet rs = dmi.dmiSelect(query);

			int extreme = DMIUtil.MISSING_INT;
			if (rs.next())
			{
				extreme = rs.getInt(1);
				if (rs.wasNull())
				{
					extreme = DMIUtil.MISSING_INT;
				}
			}
			DMI.closeResultSet(rs);
			return extreme;
		}
		catch (Exception e)
		{
			string routine = "DMIUtil.getExtremeRecord";
			Message.printWarning(2, routine, "Error finding extreme.");
			Message.printWarning(2, routine, e);
			return DMIUtil.MISSING_INT;
		}
	}

	/// <summary>
	/// This function determines the max value of the specified integer field from the 
	/// requested table via using max(field) and the the specified DMI connection. </summary>
	/// <param name="dmi"> the DMI to use. </param>
	/// <param name="table"> table name </param>
	/// <param name="field"> table field to determine the max value of </param>
	/// <returns> returns max record or DMIUtil.MISSING_INT if an error occurred. </returns>
	public static int getMaxRecord(DMI dmi, string table, string field)
	{
		return DMIUtil.getExtremeRecord(dmi, table, field, "MAX");
	}

	/// <summary>
	/// This function determines the minimum value of the specified integer field from the 
	/// requested table via using min(field) and the the specified DMI connection. </summary>
	/// <param name="dmi"> the DMI to use. </param>
	/// <param name="table"> table name </param>
	/// <param name="field"> table field to determine the min value of </param>
	/// <returns> returns min record or DMIUtil.MISSING_INT if an error occurred. </returns>
	public static int getMinRecord(DMI dmi, string table, string field)
	{
		return DMIUtil.getExtremeRecord(dmi, table, field, "MIN");
	}

	/// <summary>
	/// Constructs a concatenated String with OR inserted at the appropriate locations. </summary>
	/// <param name="or"> list of Strings in which to construct an OR clause </param>
	/// <returns> returns a String with OR inserted at the appropriate locations, null if "or" is null </returns>
	public static string getOrClause(IList<string> or)
	{
			if (or == null)
			{
				return null;
			}

			int size = or.Count;
			string orString = "";

			for (int i = 0; i < size; i++)
			{
				if ((i != (size - 1)) && (size != 1))
				{
					orString += or[i] + " OR ";
				}
				else
				{
					orString += or[i];
				}
			}
			return "(" + orString + ")";
	}

	/// <summary>
	/// Returns the database types a DMI can connect to that are done via direct server connection. </summary>
	/// <returns> a list of the database types a DMI can connect to that are done via direct server connection. </returns>
	public static IList<string> getServerDatabaseTypes()
	{
		return DMI.getServerDatabaseTypes();
	}

	/// <summary>
	/// Returns the comment for a table in a SQL Server database. </summary>
	/// <param name="dmi"> a dmi object that is open, not-null, and connected to a SQL Server database. </param>
	/// <param name="tableName"> the name of the table for which to return the comment.  Must not be null. </param>
	/// <returns> the comment for a table in the SQL Server database. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static String getSQLServerTableComment(DMI dmi, String tableName) throws Exception
	public static string getSQLServerTableComment(DMI dmi, string tableName)
	{
		string SQL = "SELECT * FROM ::fn_listextendedproperty('MS_Description'"
			+ ",'user','dbo', 'table','" + tableName + "',null,null)";
		ResultSet rs = dmi.dmiSelect(SQL);

		bool more = rs.next();

		string comment = "   ";
		if (more)
		{
			comment = rs.getString(4);
		}
		DMI.closeResultSet(rs);
		return comment;
	}

	/// <summary>
	/// Returns the comment for a column in a SQL Server database. </summary>
	/// <param name="dmi"> a dmi object that is open, not-null, and connected to a SQL Server database. </param>
	/// <param name="tableName"> the name of the table containing the column.  Must not be null. </param>
	/// <param name="columnName"> the name of the column for which to return the comment.  Must not be null. </param>
	/// <returns> the comment for a column in a SQL Server database. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static String getSQLServerColumnComment(DMI dmi, String tableName, String columnName) throws Exception
	public static string getSQLServerColumnComment(DMI dmi, string tableName, string columnName)
	{
		bool doMeta = false;
		string comment = "   ";
		if (doMeta)
		{

		}
		else
		{
			string SQL = "SELECT * FROM ::fn_listextendedproperty('MS_Description'"
				+ ",'user','dbo', 'table','" + tableName + "','column'" + ",'" + columnName + "')";
			ResultSet rs = dmi.dmiSelect(SQL);

			bool more = rs.next();

			if (more)
			{
				comment = rs.getString(4);
			}
			DMI.closeResultSet(rs);
		}
		return comment;
	}

	/// <summary>
	/// Return a list of system table name patterns to remove.  These are internal tables that should not be visible to users.
	/// </summary>
	public static string [] getSystemTablePatternsToRemove(int databaseEngineType)
	{
		string[] systemTablePatternsToRemove = new string[0];
		if (databaseEngineType == DMI.DBENGINE_ACCESS)
		{
			string[] systemTablePatternsToRemove0 = new string[] {"MSysAccessObjects", "MSysACEs", "MSysObjects", "MSysQueries", "MSysRelationships", "Paste Errors"};
			systemTablePatternsToRemove = systemTablePatternsToRemove0;
		}
		else if (databaseEngineType == DMI.DBENGINE_SQLSERVER)
		{
			string[] systemTablePatternsToRemove0 = new string[] {"syscolumns", "syscomments", "sysdepends", "sysfilegroups", "sysfiles", "sysfiles1", "sysforeignkeys", "sysfulltextcatalogs", "sysfulltextnotify", "sysindexes", "sysindexkeys", "sysmembers", "sysobjects", "syspermissions", "sysproperties", "sysprotects", "sysreferences", "systypes", "sysusers", "sysconstraints", "syssegments", "dtproperties", "all_columns", "all_objects", "all_parameters", "all_sql_modules", "all_views", "allocation_keys", "allocation_units", "assemblies", "assembly_.*", "asymmetric_keys", "availability_.*", "backup_devices", "certificates", "check_constraints", "column_.*", "columns", "computed_columns", "configurations", "constraint_.*", "conversation_.*", "credentials", "crypt_properties", "cryptographic_.*", "databases", "database_.*", "data_spaces", "default_constraints", "destination_data.*", "dm_.*", "domains", "domain_constraints", "change_tracking.*", "endpoints", "endpoint_webmethods", "event_notification.*", "events", "extended_.*", "filegroups", "filetable.*", "foreign_key.*", "fulltext_.*", "function_.*", "http_endpoints", "identity_columns", "index_columns", "indexes", "internal_tables", "key_.*", "linked_logins", "login_token", "master_.*", "messages", "message_.*", "module_.*", "numbered_.*", "objects", "openkeys", "parameter_.*", "parameters", "partition_.*", "partitions", "plan_guides", "procedures.*", "referential_.*", "remote_.*", "registered_search.*", "resource_.*", "routes", "routine_columns", "routines", "sequences", "schemas", "schemata", "securable_classes", "selective_xml.*", "server_.*", "servers", "service_.*", "services", "soap_.*", "spatial_.*", "spt_.*", "sql_.*", "stats", "stats_columns", "symmetric_keys", "synonyms", "system_.*", "sys.*", "tables", "table_constraints", "table_privileges", "table_types", "tcp_endpoints", "trace_.*", "traces", "transmission_queue", "triggers", "trigger_.*", "types", "type_assembly_usages", "user_token", "version", "via_endpoints", "views", "view_column_usage", "view_table_usage", "xml_indexes", "xml_schema.*"};
			systemTablePatternsToRemove = systemTablePatternsToRemove0;
		}
		else
		{
			// TODO SAM 2012-01-31 Unsure what tables are specific to other database types, this needs to be checked.
		}
		return systemTablePatternsToRemove;
	}

	/// <summary>
	/// Create a list of where clauses give an InputFilter_JPanel.  The InputFilter
	/// instances that are managed by the InputFilter_JPanel must have been defined with
	/// the database table and field names in the internal (non-label) data. </summary>
	/// <returns> a list of where clauses, each of which can be added to a DMI statement. </returns>
	/// <param name="dmi"> The DMI instance being used, which may be checked for specific formatting. </param>
	/// <param name="panel"> The InputFilter_JPanel instance to be converted. </param>
	public static IList<string> getWhereClausesFromInputFilter(DMI dmi, InputFilter_JPanel panel)
	{ // Loop through each filter group.  There will be one where clause per filter group.
		int nfg = panel.getNumFilterGroups();
		InputFilter filter;
		IList<string> where_clauses = new List<string>();
		string where_clause = ""; // A where clause that is being formed.
		for (int ifg = 0; ifg < nfg; ifg++)
		{
			filter = panel.getInputFilter(ifg);
			where_clause = getWhereClauseFromInputFilter(dmi, filter, panel.getOperator(ifg));
			if (!string.ReferenceEquals(where_clause, null))
			{
				where_clauses.Add(where_clause);
			}
		}
		return where_clauses;
	}

	/// <summary>
	/// Create a single where clause given an InputFilter.  The InputFilter
	/// instances that are managed by the InputFilter_JPanel must have been defined with
	/// the database table and field names in the internal (non-label) data. </summary>
	/// <returns> a list of where clauses, each of which can be added to a DMI statement. </returns>
	/// <param name="dmi"> The DMI instance being used, which may be checked for specific formatting. </param>
	/// <param name="operator"> the operator to use in creating the where clause </param>
	/// <param name="panel"> The InputFilter_JPanel instance to be converted. </param>
	public static string getWhereClauseFromInputFilter(DMI dmi, InputFilter filter, string @operator)
	{
		return getWhereClauseFromInputFilter(dmi, filter, @operator, false);
	}

	/// <summary>
	/// Create a single where clause given an InputFilter.  The InputFilter
	/// instances that are managed by the InputFilter_JPanel must have been defined with
	/// the database table and field names in the internal (non-label) data. </summary>
	/// <returns> a list of where clauses, each of which can be added to a DMI statement. </returns>
	/// <param name="dmi"> The DMI instance being used, which may be checked for specific formatting </param>
	/// <param name="panel"> The InputFilter_JPanel instance to be converted </param>
	/// <param name="operator"> the operator to use in creating the where clause </param>
	/// <param name="upperCase"> if true, then the where clause for strings will be converted to upper case using
	/// the SQL upper() function - this is necessary for databases where a global case-insensitive
	/// option is not available </param>
	public static string getWhereClauseFromInputFilter(DMI dmi, InputFilter filter, string @operator, bool upperCase)
	{
		string routine = "getWhereClauseFromInputFilter";
		// Get the selected filter for the filter group...
		if (filter.getWhereLabel().Trim().Equals(""))
		{
			// Blank indicates that the filter should be ignored...
			return null;
		}
		// Get the input type...
		int input_type = filter.getInputType();
		if (filter.getChoiceTokenType() > 0)
		{
			input_type = filter.getChoiceTokenType();
		}
		// Get the internal where...
		string whereSubject = filter.getWhereInternal();
		if ((string.ReferenceEquals(whereSubject, null)) || whereSubject.Equals(""))
		{
			return null;
		}
		// Get the user input...
		string input = filter.getInputInternal().Trim();
		if (upperCase)
		{
			input = input.ToUpper();
		}
		Message.printStatus(2,routine,"Internal input is \"" + input + "\"");
		// Now format the where clause...

		string where_clause = null;

		if (@operator.Equals(InputFilter.INPUT_BETWEEN, StringComparison.OrdinalIgnoreCase))
		{
			// TODO - need to enable in InputFilter_JPanel.
		}
		else if (@operator.Equals(InputFilter.INPUT_CONTAINS, StringComparison.OrdinalIgnoreCase))
		{
			// Only applies to strings...
			if (upperCase)
			{
				where_clause = "upper(" + whereSubject + ") like '%" + input + "%'";
			}
			else
			{
				where_clause = whereSubject + " like '%" + input + "%'";
			}
		}
		else if (@operator.Equals(InputFilter.INPUT_ENDS_WITH, StringComparison.OrdinalIgnoreCase))
		{
			// Only applies to strings...
			if (upperCase)
			{
				where_clause = "upper(" + whereSubject + ") like '%" + input + "'";
			}
			else
			{
				where_clause = whereSubject + " like '%" + input + "'";
			}
		}
		else if (@operator.Equals(InputFilter.INPUT_EQUALS, StringComparison.OrdinalIgnoreCase))
		{
			if (input_type == StringUtil.TYPE_STRING)
			{
				if (upperCase)
				{
					where_clause = "upper(" + whereSubject + ")='" + input + "'";
				}
				else
				{
					where_clause = whereSubject + "='" + input + "'";
				}
			}
			else
			{
				// Number...
				where_clause = whereSubject + "=" + input;
			}
		}
		else if (@operator.Equals(InputFilter.INPUT_GREATER_THAN, StringComparison.OrdinalIgnoreCase))
		{
			// Only applies to numbers (?)...
			where_clause = whereSubject + ">" + input;
		}
		else if (@operator.Equals(InputFilter.INPUT_GREATER_THAN_OR_EQUAL_TO, StringComparison.OrdinalIgnoreCase))
		{
			// Only applies to numbers (?)...
			where_clause = whereSubject + ">=" + input;
		}
		else if (@operator.Equals(InputFilter.INPUT_IS_EMPTY, StringComparison.OrdinalIgnoreCase))
		{
			where_clause = whereSubject + "='' or where is null";
		}
		else if (@operator.Equals(InputFilter.INPUT_LESS_THAN, StringComparison.OrdinalIgnoreCase))
		{
			// Only applies to numbers (?)...
			where_clause = whereSubject + "<" + input;
		}
		else if (@operator.Equals(InputFilter.INPUT_LESS_THAN_OR_EQUAL_TO, StringComparison.OrdinalIgnoreCase))
		{
			// Only applies to numbers (?)...
			where_clause = whereSubject + "<=" + input;
		}
		else if (@operator.Equals(InputFilter.INPUT_MATCHES, StringComparison.OrdinalIgnoreCase))
		{
			// Only applies to strings
			if (upperCase)
			{
				where_clause = "upper(" + whereSubject + ")='" + input + "'";
			}
			else
			{
				where_clause = whereSubject + "='" + input + "'";
			}
		}
		else if (@operator.Equals(InputFilter.INPUT_ONE_OF, StringComparison.OrdinalIgnoreCase))
		{
			// TODO - need to enable in InputFilter_JPanel
		}
		else if (@operator.Equals(InputFilter.INPUT_STARTS_WITH, StringComparison.OrdinalIgnoreCase))
		{
			// Only applies to strings...
			if (upperCase)
			{
				where_clause = "upper(" + whereSubject + ") like '" + input + "%'";
			}
			else
			{
				where_clause = whereSubject + " like '" + input + "%'";
			}
		}
		else
		{
			// Unrecognized where...
			string message = "Unrecognized operator \"" + @operator + "\"";
			Message.printWarning(2, routine, message);
			throw new InvalidParameterException(message);
		}
		// TODO - need to handle is null, negative (not), when enabled in InputFilter_JPanel.
		// TODO - need a clean way to enforce upper case input but
		// also perhaps allow a property in the filter to override
		// because a database may have mixed case in only a few tables...
		//if ( dmi.uppercaseStringsPreferred() ) {
			//where_clause = where_clause.toUpperCase();
		//}
		return where_clause;
	}

	/// <summary>
	/// Determines whether a boolean value is missing. </summary>
	/// <param name="value"> the boolean to be checked </param>
	/// <returns> true always because the value will be true or false (see overloaded version for Boolean type) </returns>
	public static bool isMissing(bool value)
	{
		return false;
	}

	/// <summary>
	/// Determines whether a Boolean value is missing. </summary>
	/// <param name="value"> the Boolean to be checked </param>
	/// <returns> true always because the value will be true or false </returns>
	public static bool isMissing(bool? value)
	{
		if (value == null)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Determines whether a date value is missing. </summary>
	/// <param name="value"> the date to be checked </param>
	/// <returns> true if the date is missing, false if not </returns>
	public static bool isMissing(System.DateTime value)
	{
		return isMissingDate(value);
	}

	/// <summary>
	/// Determines whether a double value is missing. </summary>
	/// <param name="value"> the double to be checked </param>
	/// <returns> true if the double is missing, false if not </returns>
	public static bool isMissing(double value)
	{
		return isMissingDouble(value);
	}

	/// <summary>
	/// Determines whether a Double value is missing. </summary>
	/// <param name="value"> the Double to be checked. </param>
	/// <returns> true if the Double is missing, false if not. </returns>
	public static bool isMissing(double? value)
	{
		return isMissingDouble(value.Value);
	}

	/// <summary>
	/// Determines whether a float value is missing. </summary>
	/// <param name="value"> the float to be checked </param>
	/// <returns> true if the float is missing, false if not </returns>
	public static bool isMissing(float value)
	{
		return isMissingDouble(value);
	}

	/// <summary>
	/// Determines whether a Float value is missing. </summary>
	/// <param name="value"> the Float to be checked. </param>
	/// <returns> true if the Float is missing, false if not. </returns>
	public static bool isMissing(float? value)
	{
		return isMissingDouble(value.Value);
	}

	/// <summary>
	/// Determines whether an int value is missing. </summary>
	/// <param name="value"> the int to be checked </param>
	/// <returns> true if the int is missing, false if not </returns>
	public static bool isMissing(int value)
	{
		return isMissingInt(value);
	}

	/// <summary>
	/// Determines whether an Integer value is missing. </summary>
	/// <param name="value"> the Integer to be checked. </param>
	/// <returns> true if the Integer is missing, false if not. </returns>
	public static bool isMissing(int? value)
	{
		return isMissingInt(value.Value);
	}

	/// <summary>
	/// Determines whether a long value is missing. </summary>
	/// <param name="value"> the long to be checked </param>
	/// <returns> true if the long is missing, false if not </returns>
	public static bool isMissing(long value)
	{
		return isMissingLong(value);
	}

	/// <summary>
	/// Determines whether a Long value is missing. </summary>
	/// <param name="value"> the Long to be checked. </param>
	/// <returns> true if the Long is missing, false if not. </returns>
	public static bool isMissing(long? value)
	{
		return isMissingLong(value.Value);
	}

	/// <summary>
	/// Determines whether a String value is missing. </summary>
	/// <param name="value"> the String to be checked </param>
	/// <returns> true if the String is missing, false if not </returns>
	public static bool isMissing(string value)
	{
		if ((string.ReferenceEquals(value, null)) || (value.Length == 0))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Determines whether a Date is missing. </summary>
	/// <param name="value"> the Date to be checked </param>
	/// <returns> true if the Date is missing, false if not </returns>
	private static bool isMissingDate(System.DateTime value)
	{
		if (value == MISSING_DATE)
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Determines whether a double is missing. </summary>
	/// <param name="value"> the double to be checked </param>
	/// <returns> true if the double is missing, false if not </returns>
	private static bool isMissingDouble(double value)
	{
		if (double.IsNaN(value))
		{
			return true;
		}
		// TODO SAM 2015-08-06 Remove the following if NaN works for missing
		//if ((value > MISSING_DOUBLE_FLOOR) && (value < MISSING_DOUBLE_CEILING)) {
		//	return true;
		//}
		return false;
	}

	/// <summary>
	/// Determines whether an int is missing. </summary>
	/// <param name="value"> the int to be checked </param>
	/// <returns> true if the int is missing, false if not </returns>
	private static bool isMissingInt(int value)
	{
		if (value == MISSING_INT)
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Determines whether a long is missing. </summary>
	/// <param name="value"> the long to be checked </param>
	/// <returns> true if the long is missing, false if not </returns>
	private static bool isMissingLong(long value)
	{
		if (value == MISSING_LONG)
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Uses Message.printDebug(1, ...) to print out the results stored in a 
	/// list of lists (which has been returned from a call to processResultSet)
	/// </summary>
	public static void printResults(IList<IList<object>> v)
	{
		printResults(v, "  ");
	}

	/// <summary>
	/// Uses Message.printDebug(1, ...) to print out the results stored in a 
	/// list of lists (which has been returned from a call to processResultSet)
	/// </summary>
	public static void printResults(IList<IList<object>> v, string delim)
	{
		int size = v.Count;
		for (int i = 0; i < size; i++)
		{
			IList<object> vv = v[i];
			int vsize = vv.Count;
			Message.printDebug(1, "", "  -> ");
			for (int j = 0; j < vsize; j++)
			{
				Message.printDebug(1, "", "" + vv[j] + delim);
			}
			Message.printDebug(1, "", "\n");
		}
	}

	/// <summary>
	/// Print the detailed information for an SQLException, using Message.printDebug().
	/// The SQLException message, state, and error code are printed for each level of the stack trace. </summary>
	/// <param name="dl"> Debug level to print at. </param>
	/// <param name="routine"> Name of the routine that should be included in the message (can be null). </param>
	/// <param name="e"> the exception that was thrown </param>
	public static void printSQLException(int dl, string routine, SQLException e)
	{
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "SQL Exception:");
			while (e != null)
			{
				Message.printDebug(1, "", "  Message:   " + e.Message);
				Message.printDebug(1, "", "  SQLState:  " + e.getSQLState());
				Message.printDebug(1, "", "  ErrorCode: " + e.getErrorCode());
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				e = e.getNextException();
				Message.printDebug(dl, "", "");
			}
		}
	}

	/// <summary>
	/// Takes a result set returned from a <b><code>SELECT</b></code> statement
	/// and transforms it into a list so that it can be returned and operated on more easily.<para>
	/// strings are entered into the resulting list as strings, but numeric 
	/// </para>
	/// values are entered in as the Java Wrapper objects that correspond to the kind of primitive they are. <para>
	/// e.g., <code>int</code>s will be entered into the vector as <code>Integer</code>
	/// s.  <code>float</code>s will be entered into the vector as <code>Float
	/// </para>
	/// </code>s, and so on.<para>
	/// <b>Note:</b> Not all of the SQL data types are accounted for yet in this
	/// method.  For instance, <code>java.sql.Types.DISTINCT</code> has no code
	/// in place to transform it into something we can work with in a vector. 
	/// This may cause some odd errors, but there's little that can be done right 
	/// now.  Fortunately, such occurrences should be very rare.
	/// </para>
	/// </summary>
	/// <param name="rs"> the resultSet whose values will be entered into vector format </param>
	/// <returns> a list containing all the values from the resultset </returns>
	/// <exception cref="SQLException"> thrown by ResultSet.getMetaData(), 
	/// ResultSetMetaData.getColumnCount(), or any of the ResultSet.get[DataType]() methods </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<java.util.List<Object>> processResultSet(java.sql.ResultSet rs) throws java.sql.SQLException
	public static IList<IList<object>> processResultSet(ResultSet rs)
	{
		string routine = "DMI.processResultSet";
		int dl = 25;

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "[method called]");
		}

		// Used for storing the type of each column in the resultSet
		IList<int> types = new List<int>();

		// The list which will be built containing the rows from the resultSet
		IList<IList<object>> results = new List<IList<object>>();

		// Set up the types list 
		ResultSetMetaData rsmd = rs.getMetaData();
		int columnCount = rsmd.getColumnCount();
		for (int i = 0; i < columnCount; i++)
		{
			types.Add(new int?(rsmd.getColumnType(i + 1)));
		}

		while (rs.next())
		{
			IList<object> row = new List<object>(0);
			for (int i = 0; i < columnCount; i++)
			{
				int? I = types[i];
				int val = I.Value;

				switch (val)
				{
					case java.sql.Types.BIGINT:
						row.Add(new int?(rs.getInt(i + 1)));
						break;
					case java.sql.Types.BIT:
						row.Add("java.sql.Types.BIT");
						break;
					case java.sql.Types.CHAR:
						row.Add(rs.getString(i + 1));
						break;
					case java.sql.Types.DATE:
						row.Add(rs.getDate(i + 1));
						break;
					case java.sql.Types.DECIMAL:
						row.Add(new double?(rs.getDouble(i + 1)));
						break;
					case java.sql.Types.DOUBLE:
						row.Add(new double?(rs.getDouble(i + 1)));
						break;
					case java.sql.Types.FLOAT:
						row.Add(new float?(rs.getFloat(i + 1)));
						break;
					case java.sql.Types.INTEGER:
						row.Add(new int?(rs.getInt(i + 1)));
						break;
					case java.sql.Types.LONGVARBINARY:
						row.Add(rs.getBinaryStream(i + 1));
						break;
					case java.sql.Types.LONGVARCHAR:
						row.Add(rs.getString(i + 1));
						break;
					case java.sql.Types.NULL:
						row.Add("java.sql.Types.NULL");
						break;
					case java.sql.Types.NUMERIC:
						row.Add("java.sql.Types.NUMERIC");
						break;
					case java.sql.Types.NVARCHAR:
						row.Add(rs.getString(i + 1));
						break;
					case java.sql.Types.OTHER:
						row.Add(rs.getObject(i + 1));
						break;
					case java.sql.Types.REAL:
						row.Add(new float?(rs.getFloat(i + 1)));
						break;
					case java.sql.Types.SMALLINT:
						row.Add(new int?(rs.getInt(i + 1)));
						break;
					case java.sql.Types.TIME:
						row.Add(rs.getTime(i + 1));
						break;
					case java.sql.Types.TIMESTAMP:
						row.Add(rs.getTimestamp(i + 1));
						break;
					case java.sql.Types.TINYINT:
						row.Add(new int?(rs.getInt(i + 1)));
						break;
					case java.sql.Types.VARBINARY:
						row.Add(rs.getBinaryStream(i + 1));
						break;
					case java.sql.Types.VARCHAR:
						row.Add(rs.getString(i + 1));
						break;
					default:
						Message.printWarning(3, routine, "Unhandled database data type " + val);
						break;
				} // end switch
			} // end for on columns
			results.Add(row);
		} // end while on records
		return results;
	}

	/// <summary>
	/// Queries a resultset's meta data for the names of the columns returned into the result set. </summary>
	/// <param name="rs"> the ResultSet for which the column names are desired </param>
	/// <returns> a vector containing the names of the columns in order from 1 to X </returns>
	/// <exception cref="SQLException"> thrown by ResultSet.getMetaData, 
	/// ResultSetMetaData.getColumnCount or ResultSetMetaData.getColumnName </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<String> processResultSetColumnNames(java.sql.ResultSet rs) throws java.sql.SQLException
	public static IList<string> processResultSetColumnNames(ResultSet rs)
	{
		string routine = "DMI.processResultSetColumnNames";
		int dl = 25;

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "[method called]");
		}
		IList<string> names = new List<string>();

		ResultSetMetaData rsmd = rs.getMetaData();
		int count = rsmd.getColumnCount();

		///////////////////////////////////////////////////////////////////
		// Important Developer Note
		///////////////////////////////////////////////////////////////////	
		// when querying the names of columns from ResultSetMetaData, columns 
		// are numbered from 1 to X, not from 0 to X. 
		// 
		// Furthermore, calling rsmd.getColumnName(0) did not throw an 
		// exception, nor did it crash the program.  The code just hung.  
		// Something to watch out for.

		for (int i = 1; i <= count; i++)
		{
			names.Add(rsmd.getColumnName(i));
		}

		return names;
	}

	/// <summary>
	/// Remove comments from an SQL statement.  This is needed because some databases such as Microsoft Access
	/// do not allow comments.  Currently only C-style slash-dot and dot-slash comments are removed. </summary>
	/// <param name="sql"> SQL statement to process. </param>
	public static string removeCommentsFromSql(string sql)
	{
		StringBuilder s = new StringBuilder(sql);
		// Search for /* */ comments until none are left
		int pos1 = 0;
		int pos2 = 0;
		int lenRemoved;
		while (true)
		{
			pos1 = s.ToString().IndexOf("/*",pos2);
			pos2 = s.ToString().IndexOf("*/",pos2);
			if ((pos1 < 0) || (pos2 < 0))
			{
				break;
			}
			s.Remove(pos1, (pos2 + 2) - pos1);
			lenRemoved = pos2 - pos1 + 2;
			pos2 = pos2 - lenRemoved;
			if (pos2 > (s.Length - 1))
			{
				break;
			}
		}
		// Search for -- to newline until none are left
		pos1 = 0;
		while (true)
		{
			pos1 = s.ToString().IndexOf("--",pos1);
			pos2 = s.ToString().IndexOf("\n",pos2);
			if (pos2 < 0)
			{
				// Go to the end of the string (-1 because incremented below)
				pos2 = s.Length - 1;
			}
			if ((pos1 < 0) || (pos2 < 0))
			{
				break;
			}
			// The following should work even if \r\n is used for new lines
			s.Remove(pos1, (pos2 + 1) - pos1);
			lenRemoved = pos2 - pos1 + 1;
			pos2 = pos2 - lenRemoved;
			if (pos2 > (s.Length - 1))
			{
				break;
			}
		}
		return s.ToString();
	}

	/// <summary>
	/// Removes a table from a database if the user has the permission to. </summary>
	/// <param name="dmi"> an open DMI object connected to the database in which to work. </param>
	/// <param name="tableName"> the name of the table to remove. </param>
	/// <exception cref="Exception"> if an error occurs </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void removeTable(DMI dmi, String tableName) throws Exception
	public static void removeTable(DMI dmi, string tableName)
	{
		string SQL = null;

		string databaseEngine = dmi.getDatabaseEngine();
		int databaseEngineType = dmi.getDatabaseEngineType();

		if (databaseEngineType == DMI.DBENGINE_SQLSERVER)
		{
			SQL = "DROP TABLE " + tableName;
		}
		else
		{
			throw new Exception("Unsupported database type: " + databaseEngine + " in 'removeTable'");
		}

		dmi.dmiExecute(SQL);
	}

	/// <summary>
	/// Checks to see whether a result set has a column with the given name.  The check is case-sensitive. </summary>
	/// <param name="resultSet"> the result set to check. </param>
	/// <param name="columnName"> the name of the column to search for in the result set.  
	/// The column name is checked with case sensitivity. </param>
	/// <returns> true if the result set has a column with the given name, false if not. </returns>
	/// <exception cref="Exception"> if an error occurs checking for the name. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static boolean resultSetHasColumn(java.sql.ResultSet resultSet, String columnName) throws Exception
	public static bool resultSetHasColumn(ResultSet resultSet, string columnName)
	{
		ResultSetMetaData rsmd = resultSet.getMetaData();
		int num = rsmd.getColumnCount();
		for (int i = 1; i <= num; i++)
		{
			if (rsmd.getColumnName(i).Equals(columnName))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Sets the X and Y position of an ERDiagram_Table object from data in the dmi's database. </summary>
	/// <param name="dmi"> an open, connected and not-null dmi object. </param>
	/// <param name="table"> the table object to fill with data from the database.  Must not be null. </param>
	/// <param name="tablesTableName"> the name of the table in the database that contains the
	/// list of table names and ER Diagram information.  Must not be null. </param>
	/// <param name="tableField"> the name of the column in the tables table that contains the
	/// names of the tables.  Must not be null. </param>
	/// <param name="erdXField"> the name of the column in the tables table that contains the
	/// X positions of the ERDiagram Tables.  Must not be null. </param>
	/// <param name="erdYField"> the name of the column in the tables table that contains the
	/// Y positions of the ERDIagram Tables.  Must not be null. </param>
	private static void setTableXY(DMI dmi, ERDiagram_Table table, string tablesTableName, string tableField, string erdXField, string erdYField)
	{
		string sql = "SELECT " + erdXField + ", " + erdYField + " FROM "
			+ tablesTableName + " WHERE " + tableField + " = '" + table.getName() + "'";
		try
		{
			ResultSet rs = dmi.dmiSelect(sql);

			bool more = rs.next();

			if (more)
			{
				double x = rs.getDouble(1);
				if (!rs.wasNull())
				{
					table.setX(x);
				}
				double y = rs.getDouble(2);
				if (!rs.wasNull())
				{
					table.setY(y);
				}
			}
			DMI.closeResultSet(rs);
		}
		catch (Exception e)
		{
			Console.WriteLine(e.ToString());
			Console.Write(e.StackTrace);
		}
	}

	// TODO (JTS - 2003-04-22) Proof of concept of getting user privileges out of a table
	public static void testPrivileges(DMI dmi, string tableName)
	{
		try
		{
			DatabaseMetaData metadata = null;
			metadata = dmi.getConnection().getMetaData();

			ResultSet rs = metadata.getTablePrivileges(null, null, tableName);

			bool more = rs.next();
			/*
			System.out.println("Privileges for table: " + tableName);
			System.out.println("Grantor		Grantee		" + "Privilege");
			*/
			while (more)
			{
				/*
				System.out.println(rs.getString(4) + "\t" + rs.getString(5) + "\t" + rs.getString(6));
				*/
				more = rs.next();
			}
		}
		catch (Exception e)
		{
			Console.WriteLine(e.ToString());
			Console.Write(e.StackTrace);
		}
	}

	// TODO (JTS - 2003-04-22) Proof of concept of getting key information out of a table
	public static void testKeys(DatabaseMetaData metadata, string tableName)
	{
		Message.printStatus(2, "", "" + tableName + " foreign key info:");
		ResultSet keysRS = null;
		IList<string> keysV = null;
		int keysSize = 0;
		try
		{
			keysRS = metadata.getImportedKeys(null, null, tableName);
			keysV = new List<string>();
			while (keysRS.next())
			{
				keysV.Add(testKeysString(keysRS));
			}
			keysSize = keysV.Count;
		}
		catch (Exception)
		{
		}
		for (int i = 0; i < keysSize; i++)
		{
			Message.printStatus(2, "", "  [" + i + "] (imp): " + keysV[i]);
		}

		try
		{
			keysRS = metadata.getExportedKeys(null, null, tableName);
			keysV = new List<string>();
			while (keysRS.next())
			{
				keysV.Add(testKeysString(keysRS));
			}
			keysSize = keysV.Count;
		}
		catch (Exception)
		{
		}
		for (int i = 0; i < keysSize; i++)
		{
			Message.printStatus(2, "", "  [" + i + "] (exp): " + keysV[i]);
		}
	}

	// TODO (JTS - 2003-04-22) Proof of concept of getting key information out of a table
	public static string testKeysString(ResultSet rs)
	{
		string s = null;
		try
		{
			s = "[" + rs.getString(3) + "." + rs.getString(4) + "] -> ["
				+ rs.getString(7) + "." + rs.getString(8) + "]";
		}
		catch (Exception e)
		{
			Console.WriteLine(e.ToString());
			Console.Write(e.StackTrace);
			s = "[null]";
		}
		return s;
	}

	/// <summary>
	/// Convert a ResultSet to a list of strings.  The ResultSet is expected to only contain a string data element.
	/// This is useful for converting a distinct string query to the list of strings. </summary>
	/// <param name="rs"> ResultSet from a table query. </param>
	/// <returns> a list of strings for the first and only column in the result set, guaranteed to be non-null,
	/// but may be empty. </returns>
	/// <exception cref="SQLException"> if an error occurs </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<String> toStringList(java.sql.ResultSet rs) throws java.sql.SQLException
	public static IList<string> toStringList(ResultSet rs)
	{
		IList<string> v = new List<string>();
		int index = 1;
		string s;
		while (rs.next())
		{
			index = 1;
			s = rs.getString(index++);
			if (!rs.wasNull())
			{
				v.Add(s.Trim());
			}
		}
		return v;
	}

	public static void dumpProcedureInfo(DMI dmi, string procedure)
	{
	Message.printStatus(2, "", "Dumping procedure info for: " + procedure);
	Message.printStatus(2, "", "----------------------------------------------");
		try
		{
			DatabaseMetaData metadata = null;
			metadata = dmi.getConnection().getMetaData();
			ResultSet rs = metadata.getProcedureColumns(dmi.getDatabaseName(), null, procedure, null);
			printResults(processResultSet(rs));
			rs.close();
		}
		catch (Exception e)
		{
			Console.WriteLine(e.ToString());
			Console.Write(e.StackTrace);
		}
	}

	}

}