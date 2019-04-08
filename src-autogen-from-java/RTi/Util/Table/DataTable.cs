using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// DataTable - class to hold tabular data from a database

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
// DataTable - class to hold tabular data from a database
// ----------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
// ----------------------------------------------------------------------------
// History:
//
// 23 Jun 1999	Catherine E.
//		Nutting-Lane, RTi	Initial version
// 2001-09-17	Steven A. Malers, RTi	Change the name of the class from Table
//					to DataTable to avoid conflict with the
//					existing C++ class.  Review code.
//					Remove unneeded messages to increase
//					performance.  Add get methods for fields
//					for use when writing table.  Make data
//					protected to allow extension to derived
//					classes (e.g., DbaseDataTable).
// 2001-10-04	SAM, RTi		Add getFormatFormat(int) to allow
//					operation on a single field.
// 2001-10-10	SAM, RTi		Add getFieldNames().
// 2001-10-12	SAM, RTi		By default, trim character fields.  Add
//					the trimStrings() method to allow an
//					override.  This data member should be
//					checked by the specific read code in
//					derived classes.  Also change the
//					format for strings to %- etc. because
//					strings are normally left justified.
// 2001-12-06	SAM, RTi		Change so that getNumberOfRecords()
//					returns the value of _num_records and
//					not _table_records.size().  The latter
//					produces errors when records are read
//					on the fly.  Classes that allow on-the-
//					fly reads will need to set the number of
//					records.
// 2002-07-27	SAM, RTi		Trim the column names when reading the
//					header.
// 2003-12-16	J. Thomas Sapienza, RTi	* Added code for writing a table out to
//					  a delimited file.
//					* Added code for dumping a table to 
//					  Status level 1 (for debugging).
//					* Added code to trim spaces from values
//					  read in from a table.
// 2003-12-18	JTS, RTi		Added deleteRecord().
// 2004-02-25	JTS, RTi		Added parseFile().
// 2004-03-11	JTS, RTi		Added isDirty().
// 2004-03-15	JTS, RTi		Commented out the DELIM_SKIP_BLANKS 
//					from the delimited file read, so to 
//					allow fields with no data.
// 2004-04-04	SAM, RTi		Fix bug where the first non-comment line
//					was being ignored.
// 2004-08-03	JTS, RTi		Added setFieldValue().
// 2004-08-05	JTS, RTi		Added version of parseDelimitedFile()
//					that takes a parameter specifying the 
//					max number of lines to read from the 
//					file.
// 2004-10-21	JTS, RTi		Added hasField().
// 2004-10-26	JTS, RTi		Added deleteField().
// 2005-01-03	JTS, RTi		* Added setFieldWidth()
//					* When a table is read in with
//					  parseDelimitedFile(), String columns
//					  are now checked for the longest string
//					  and the width of that column is set
//					  so that the entire string will
//					  be displayed.
// 2005-01-27	JTS, RTi		Corrected null pointer bug in 
//					parseDelimitedFile().
// 2005-11-16	SAM, RTi		Add MergeDelimiters and TrimInput
//					properties to parseDelimitedFile().
// 2006-03-02	SAM, RTi		Change so that when on the fly reading
//					is occurring, getTableRecord() returns
//					null.
// 2006-03-13	JTS, RTi		Correct bug so that parsed data tables
//					have _have_data set to true.
// 2006-06-21	SAM, RTi		Change so that when writing a delimited
//					file the contents are quoted if the data
//					contain the delimiter.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------
// EndHeader

namespace RTi.Util.Table
{

	using DMIUtil = RTi.DMI.DMIUtil;
	using IOUtil = RTi.Util.IO.IOUtil;
	using PropList = RTi.Util.IO.PropList;
	using MathUtil = RTi.Util.Math.MathUtil;
	using Message = RTi.Util.Message.Message;
	using StringDictionary = RTi.Util.String.StringDictionary;
	using StringUtil = RTi.Util.String.StringUtil;
	using DateTime = RTi.Util.Time.DateTime;

	// TODO SAM 2010-12-16 Evaluate using a different package for in-memory tables, such as
	// from H2 or other embedded database.
	/// <summary>
	/// This class contains records of data as a table, using a list of TableRecord
	/// instances.  The format of the table is defined using the TableField class.
	/// Tables can be used to store record-based data.  This class was originally
	/// implemented to store Dbase files associated with ESRI shapefile GIS data.
	/// Consequently, although a data table theoretically can store a variety of
	/// data types (see TableField), in practice only String and double types are used
	/// for some applications.
	/// Full handling of other data types will be added in the future.
	/// An example of a DataTable instantiation is:
	/// <para>
	/// 
	/// <pre>
	/// try {
	/// /// First, create define the table by assembling a list of TableField objects...
	/// List<TableField> myTableFields = new ArrayList<TableField>(3);
	/// myTableFields.add ( new TableField ( TableField.DATA_TYPE_STRING, "id_label_6", 12 ) );
	/// myTableFields.add ( new TableField ( TableField.DATA_TYPE_INT, "Basin", 12 ) );
	/// myTableFields.add ( new TableField ( TableField.DATA_TYPE_STRING, "aka", 12 ) );
	/// 
	/// // Now define table with one simple call...
	/// DataTable myTable = new DataTable ( myTableFields );
	/// 
	/// // Now define a record to be included in the table...
	/// TableRecord contents = new TableRecord (3);
	/// contents.addFieldValue ( "123456" );
	/// contents.addFieldValue ( new Integer (6));
	/// contents.addFieldValue ( "Station ID" );
	/// 
	/// myTable.addRecord ( contents );
	/// 
	/// // Get the 2nd field from the first record (fields and records are zero-index based)...
	/// system.out.println ( myTable.getFieldValue ( 0, 1 ));
	/// 
	/// } catch (Exception e ) {
	/// // process exception
	/// }
	/// </pre>
	/// 
	/// </para>
	/// </summary>
	/// <seealso cref= RTi.Util.Table.TableField </seealso>
	/// <seealso cref= RTi.Util.Table.TableRecord </seealso>
	public class DataTable
	{
	/// <summary>
	/// The identifier for the table.
	/// </summary>
	private string __table_id = "";

	/// <summary>
	/// List of TableField that define the table columns.
	/// </summary>
	protected internal IList<TableField> _table_fields;

	/// <summary>
	/// List of TableRecord, that contains the table data.
	/// </summary>
	protected internal IList<TableRecord> _table_records;

	/// <summary>
	/// List of comments for the table.  For example, an analysis that creates a table of results may
	/// need explanatory comments corresponding to column headings.  The comments can be output when the
	/// table is written to a file.
	/// </summary>
	private IList<string> __comments = new List<string>();

	/// <summary>
	/// Number of records in the table (kept for case where records are not in memory).
	/// </summary>
	protected internal int _num_records = 0;

	/// <summary>
	/// Indicates if data records have been read into memory.  This can be reset by derived classes that
	/// instead keep open a binary database file (e.g., dBase) and override the read/write methods.
	/// </summary>
	protected internal bool _haveDataInMemory = true;

	/// <summary>
	/// Indicates whether string data should be trimmed on retrieval.  In general, this
	/// should be true because older databases like Dbase pad data with spaces but seldom
	/// are spaces actually actual data values.
	/// </summary>
	protected internal bool _trim_strings = true;

	/// <summary>
	/// Indicates whether addRecord() has been called.  If so, assume that the data records
	/// are in memory for calls to getNumberOfRecords(). Otherwise, just return the _num_records value.
	/// </summary>
	protected internal bool _add_record_called = false;

	/// <summary>
	/// Construct a new table.  Use setTableFields() at a later time to define the table.
	/// </summary>
	public DataTable()
	{ // Estimate that 100 is a good increment for the data list...
		initialize(new List<TableField>(), 10, 100);
	}

	/// <summary>
	/// Construct a new table.  The list of TableRecord will increment in size by 100. </summary>
	/// <param name="tableFieldsList"> a list of TableField objects defining table contents. </param>
	public DataTable(IList<TableField> tableFieldsList)
	{ // Guess that 100 is a good increment for the data list...
		initialize(tableFieldsList, 10, 100);
	}

	/// <summary>
	/// Construct a new table. </summary>
	/// <param name="tableFieldsList"> a list of TableField objects defining table contents. </param>
	/// <param name="listSize"> Initial list size for the list holding records.  This
	/// can be used to optimize performance. </param>
	/// <param name="listIncrement"> Increment for the list holding records.  This
	/// can be used to optimize performance. </param>
	public DataTable(IList<TableField> tableFieldsList, int listSize, int listIncrement)
	{
		initialize(tableFieldsList, listSize, listIncrement);
	}

	/// <summary>
	/// Add a String to the comments associated with the time series (e.g., station remarks). </summary>
	/// <param name="comment"> Comment string to add. </param>
	public virtual void addToComments(string comment)
	{
		if (!string.ReferenceEquals(comment, null))
		{
			__comments.Add(comment);
		}
	}

	/// <summary>
	/// Add a list of String to the comments associated with the time series (e.g., station remarks). </summary>
	/// <param name="comments"> Comments strings to add. </param>
	public virtual void addToComments(IList<string> comments)
	{
		if (comments == null)
		{
			return;
		}
		foreach (string comment in comments)
		{
			if (!string.ReferenceEquals(comment, null))
			{
				__comments.Add(comment);
			}
		}
	}

	/// <summary>
	/// Adds a record to end of the list of TableRecords maintained in the DataTable.
	/// Use insertRecord() to insert within the existing records. </summary>
	/// <param name="newRecord"> new record to be added. </param>
	/// <exception cref="Exception"> when the number of fields in new_record is not equal to the
	/// number of fields in the current TableField declaration. </exception>
	/// <returns> the new record (allows command chaining) </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TableRecord addRecord(TableRecord newRecord) throws Exception
	public virtual TableRecord addRecord(TableRecord newRecord)
	{
		int num_table_fields = _table_fields.Count;
		int num_new_record_fields = newRecord.getNumberOfFields();
		_add_record_called = true;
		if (num_new_record_fields == num_table_fields)
		{
			_table_records.Add(newRecord);
			return newRecord;
		}
		else
		{
			throw new Exception("Number of fields in the new record (" + num_new_record_fields + ") does not match current description of the table fields (" + num_table_fields + ").");
		}
	}

	/// <summary>
	/// Add a field to the right-most end of table and each entry in the existing TableRecords.
	/// The added fields are initialized with blank strings or NaN, as appropriate. </summary>
	/// <param name="tableField"> information about field to add. </param>
	/// <param name="initValue"> the initial value to set for all the existing rows in the table (can be null). </param>
	/// <returns> the field index (0+). </returns>
	public virtual int addField(TableField tableField, object initValue)
	{
		return addField(-1, tableField, initValue);
	}

	/// <summary>
	/// Add a field to the table and each entry in TableRecord.  The field is added at the specified insert position.
	/// The added fields are initialized with blank strings or NaN, as appropriate. </summary>
	/// <param name="insertPos"> the column (0+) at which to add the column (-1 or >= the number of existing columns to insert at the end). </param>
	/// <param name="tableField"> information about field to add. </param>
	/// <param name="initValue"> the initial value to set for all the existing rows in the table (can be null). </param>
	/// <returns> the field index (0+). </returns>
	public virtual int addField(int insertPos, TableField tableField, object initValue)
	{
		return addField(insertPos, tableField, initValue, null);
	}

	/// <summary>
	/// Add a field to the table and each entry in TableRecord.  The field is added at the specified insert position.
	/// The added fields are initialized with blank strings or NaN, as appropriate. </summary>
	/// <param name="insertPos"> the column (0+) at which to add the column (-1 or >= the number of existing columns to insert at the end). </param>
	/// <param name="tableField"> information about field to add. </param>
	/// <param name="initValue"> the initial value to set for all the existing rows in the table (can be null). </param>
	/// <param name="initFunction"> the initial function used to set initial values for all the existing rows in the table (can be null). </param>
	/// <returns> the field index (0+). </returns>
	public virtual int addField(int insertPos, TableField tableField, object initValue, DataTableFunctionType initFunction)
	{
		bool addAtEnd = false;
		if ((insertPos < 0) || (insertPos >= _table_fields.Count))
		{
			// Add at the end
			_table_fields.Add(tableField);
			addAtEnd = true;
		}
		else
		{
			// Insert at the specified column location
			_table_fields.Insert(insertPos,tableField);
		}
		// Add value to each record in the table to be consistent with the field data
		int num = _table_records.Count;
		TableRecord tableRecord;
		for (int i = 0; i < num; i++)
		{
			tableRecord = _table_records[i];
			// Calculate the initial value if a function
			// Add element and set to specified initial value
			// These are ordered in the most likely types to optimize
			// TODO SAM 2014-05-04 Why are these broken out separately?
			int dataType = tableField.getDataType();
			if (dataType == TableField.DATA_TYPE_STRING)
			{
				if (initFunction != null)
				{
					if (initFunction == DataTableFunctionType.ROW)
					{
						initValue = "" + (i + 1);
					}
					else if (initFunction == DataTableFunctionType.ROW0)
					{
						initValue = "" + i;
					}
				}
				if (addAtEnd)
				{
					tableRecord.addFieldValue(initValue);
				}
				else
				{
					tableRecord.addFieldValue(insertPos, initValue);
				}
			}
			else if (dataType == TableField.DATA_TYPE_INT)
			{
				if (initFunction != null)
				{
					if (initFunction == DataTableFunctionType.ROW)
					{
						initValue = new int?(i + 1);
					}
					else if (initFunction == DataTableFunctionType.ROW0)
					{
						initValue = new int?(i);
					}
				}
				if (addAtEnd)
				{
					tableRecord.addFieldValue(initValue);
				}
				else
				{
					tableRecord.addFieldValue(insertPos, initValue);
				}
			}
			else if (dataType == TableField.DATA_TYPE_DOUBLE)
			{
				if (initFunction != null)
				{
					if (initFunction == DataTableFunctionType.ROW)
					{
						initValue = new double?(i + 1);
					}
					else if (initFunction == DataTableFunctionType.ROW0)
					{
						initValue = new double?(i);
					}
				}
				if (addAtEnd)
				{
					tableRecord.addFieldValue(initValue);
				}
				else
				{
					tableRecord.addFieldValue(insertPos, initValue);
				}
			}
			else if (dataType == TableField.DATA_TYPE_SHORT)
			{
				if (initFunction != null)
				{
					if (initFunction == DataTableFunctionType.ROW)
					{
						initValue = new short?((short)(i + 1));
					}
					else if (initFunction == DataTableFunctionType.ROW0)
					{
						initValue = new short?((short)(i));
					}
				}
				if (addAtEnd)
				{
					tableRecord.addFieldValue(initValue);
				}
				else
				{
					tableRecord.addFieldValue(insertPos, initValue);
				}
			}
			else if (dataType == TableField.DATA_TYPE_FLOAT)
			{
				if (initFunction != null)
				{
					if (initFunction == DataTableFunctionType.ROW)
					{
						initValue = new float?(i + 1);
					}
					else if (initFunction == DataTableFunctionType.ROW0)
					{
						initValue = new float?(i);
					}
				}
				if (addAtEnd)
				{
					tableRecord.addFieldValue(initValue);
				}
				else
				{
					tableRecord.addFieldValue(insertPos, initValue);
				}
			}
			else if (dataType == TableField.DATA_TYPE_LONG)
			{
				if (initFunction != null)
				{
					if (initFunction == DataTableFunctionType.ROW)
					{
						initValue = new long?(i + 1);
					}
					else if (initFunction == DataTableFunctionType.ROW0)
					{
						initValue = new long?(i);
					}
				}
				if (addAtEnd)
				{
					tableRecord.addFieldValue(initValue);
				}
				else
				{
					tableRecord.addFieldValue(insertPos, initValue);
				}
			}
			else if (dataType == TableField.DATA_TYPE_DATE)
			{
				// Function not relevant
				if (addAtEnd)
				{
					tableRecord.addFieldValue(initValue);
				}
				else
				{
					tableRecord.addFieldValue(insertPos, initValue);
				}
			}
			else if (dataType == TableField.DATA_TYPE_DATETIME)
			{
				// Function not relevant
				if (addAtEnd)
				{
					tableRecord.addFieldValue(initValue);
				}
				else
				{
					tableRecord.addFieldValue(insertPos, initValue);
				}
			}
		}
		if (addAtEnd)
		{
			return getNumberOfFields() - 1; // Zero offset
		}
		else
		{
			return insertPos;
		}
	}

	/// <summary>
	/// Append one table to another. </summary>
	/// <param name="table"> original table </param>
	/// <param name="newTableID"> identifier for new table </param>
	/// <param name="reqIncludeColumns"> requested columns to include or null to include all </param>
	/// <param name="distinctColumns"> requested columns to check for distinct combinations (currently only one column
	/// is allowed), will override reqIncludeColumns, specify null to not check for distinct values </param>
	/// <param name="columnMap"> map to rename original columns to new name </param>
	/// <param name="columnFilters"> map for columns that will apply a filter </param>
	/// <returns> the number of rows appended </returns>
	public virtual int appendTable(DataTable table, DataTable appendTable, string[] reqIncludeColumns, Dictionary<string, string> columnMap, Dictionary<string, string> columnFilters)
	{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		string routine = this.GetType().FullName + ".appendTable";
		// List of columns that will be appended
		string[] columnNamesToAppend = null;
		string[] columnNames = table.getFieldNames();
		if ((reqIncludeColumns != null) && (reqIncludeColumns.Length > 0))
		{
			// Append only the requested names
			columnNamesToAppend = reqIncludeColumns;
		}
		else
		{
			// Append all
			columnNamesToAppend = table.getFieldNames();
		}
		// Column numbers in the append table to match the original table.  Any values set to -1 will result in null.
		int[] columnNumbersInAppendTable = new int[table.getNumberOfFields()];
		string[] appendTableColumnNamesOriginal = appendTable.getFieldNames();
		string[] appendTableColumnNames = appendTable.getFieldNames();
		// Replace the append table names using the column map
		object o;
		for (int icol = 0; icol < appendTableColumnNames.Length; icol++)
		{
			if (columnMap != null)
			{
				o = columnMap[appendTableColumnNames[icol]];
				if (o != null)
				{
					// Reset the append column name with the new name, which should match a column name in the first table
					appendTableColumnNames[icol] = (string)o;
				}
			}
		}
		// Loop through the columns in the original table and match the column numbers in the append table
		bool appendColumnFound = false;
		for (int icol = 0; icol < columnNumbersInAppendTable.Length; icol++)
		{
			columnNumbersInAppendTable[icol] = -1; // No match between first and append table
			// Check each of the column names in the original table to match whether appending from the append table
			// The append table column names will have been mapped to the first table above
			for (int i = 0; i < appendTableColumnNames.Length; i++)
			{
				// First check to see if the column name should be appended
				appendColumnFound = false;
				for (int j = 0; j < columnNamesToAppend.Length; j++)
				{
					if (columnNamesToAppend[j].Equals(appendTableColumnNamesOriginal[i], StringComparison.OrdinalIgnoreCase))
					{
						appendColumnFound = true;
						break;
					}
				}
				if (!appendColumnFound)
				{
					// Skip the table column - don't append
					continue;
				}
				if (columnNames[icol].Equals(appendTableColumnNames[i], StringComparison.OrdinalIgnoreCase))
				{
					columnNumbersInAppendTable[icol] = i;
					break;
				}
			}
		}
		int[] tableColumnTypes = table.getFieldDataTypes(); // Original table column types
		int[] appendTableColumnTypes = appendTable.getFieldDataTypes(); // Append column types, lined up with original table
		int errorCount = 0;
		StringBuilder errorMessage = new StringBuilder();
		// Get filter columns and glob-style regular expressions
		int[] columnNumbersToFilter = new int[columnFilters.Count];
		string[] columnFilterGlobs = new string[columnFilters.Count];
		IEnumerator<string> keys = columnFilters.Keys.GetEnumerator();
		int ikey = -1;
		string key = null;
		while (keys.MoveNext())
		{
			++ikey;
			columnNumbersToFilter[ikey] = -1;
			try
			{
				key = keys.Current;
				columnNumbersToFilter[ikey] = appendTable.getFieldIndex(key);
				columnFilterGlobs[ikey] = columnFilters[key];
				// Turn default globbing notation into internal Java regex notation
				columnFilterGlobs[ikey] = columnFilterGlobs[ikey].Replace("*", ".*").ToUpper();
			}
			catch (Exception)
			{
				++errorCount;
				if (errorMessage.Length > 0)
				{
					errorMessage.Append(" ");
				}
				errorMessage.Append("Filter column \"" + key + "\" not found in table \"" + appendTable.getTableID() + "\".");
			}
		}
		// Loop through all the data records and append records to the table
		int icol;
		int irowAppended = 0;
		bool somethingAppended = false;
		bool filterMatches;
		string s;
		TableRecord rec;
		for (int irow = 0; irow < appendTable.getNumberOfRecords(); irow++)
		{
			somethingAppended = false;
			filterMatches = true;
			if (columnNumbersToFilter.Length > 0)
			{
				// Filters can be done on any columns so loop through to see if row matches before doing append
				for (icol = 0; icol < columnNumbersToFilter.Length; icol++)
				{
					if (columnNumbersToFilter[icol] < 0)
					{
						filterMatches = false;
						break;
					}
					try
					{
						o = appendTable.getFieldValue(irow, columnNumbersToFilter[icol]);
						if (o == null)
						{
							filterMatches = false;
							break; // Don't include nulls when checking values
						}
						s = ("" + o).ToUpper();
						if (!s.matches(columnFilterGlobs[icol]))
						{
							// A filter did not match so don't copy the record
							filterMatches = false;
							break;
						}
					}
					catch (Exception e)
					{
						errorMessage.Append("Error getting append table data [" + irow + "][" + columnNumbersToFilter[icol] + "].");
						Message.printWarning(3, routine, "Error getting append table data for [" + irow + "][" + columnNumbersToFilter[icol] + "] uniquetempvar.");
					}
				}
				if (!filterMatches)
				{
					// Skip the record.
					continue;
				}
			}
			// Loop through columns in the original table and set values from the append table
			// Create a record and add...
			rec = new TableRecord();
			for (icol = 0; icol < columnNumbersInAppendTable.Length; icol++)
			{
				try
				{
					if (columnNumbersInAppendTable[icol] < 0)
					{
						// Column in first table was not matched in the append table so set to null
						rec.addFieldValue(null);
					}
					else
					{
						// Set the value in the original table, if the type matches
						if (tableColumnTypes[icol] == appendTableColumnTypes[columnNumbersInAppendTable[icol]])
						{
							rec.addFieldValue(appendTable.getFieldValue(irow, columnNumbersInAppendTable[icol]));
						}
						else
						{
							rec.addFieldValue(null);
						}
					}
					somethingAppended = true;
				}
				catch (Exception e)
				{
					// Should not happen
					errorMessage.Append("Error appending [" + irow + "][" + columnNumbersInAppendTable[icol] + "].");
					Message.printWarning(3, routine, "Error setting appending [" + irow + "][" + columnNumbersInAppendTable[icol] + "] uniquetempvar.");
					++errorCount;
				}
			}
			if (somethingAppended)
			{
				// Set the record in the original table
				try
				{
					table.addRecord(rec);
					++irowAppended;
				}
				catch (Exception)
				{
					errorMessage.Append("Error appending row [" + irow + "].");
				}
			}
		}
		if (errorCount > 0)
		{
			throw new Exception("There were + " + errorCount + " errors appending data to the table: " + appendTable.getTableID());
		}
		return irowAppended;
	}

	/// <summary>
	/// Create a copy of the table. </summary>
	/// <param name="table"> original table </param>
	/// <param name="newTableID"> identifier for new table </param>
	/// <param name="reqIncludeColumns"> requested columns to include or null to include all, must specify the distinct column if only
	/// the distinct column is to be copied (this is a change from behavior prior to TSTool 10.26.00 where distinctColumns would
	/// override the reqIncludeColumns and default of all columns) </param>
	/// <param name="distinctColumns"> requested columns to check for distinct combinations, multiple columns are allowed,
	/// specify null to not check for distinct values </param>
	/// <param name="columnMap"> map to rename original columns to new name </param>
	/// <param name="columnFilters"> map for columns that will apply a filter to match column values to include </param>
	/// <param name="columnExcludeFilters"> dictionary for columns that will apply a filter to match column values to exclude </param>
	/// <returns> copy of original table </returns>
	public virtual DataTable createCopy(DataTable table, string newTableID, string[] reqIncludeColumns, string[] distinctColumns, Dictionary<string, string> columnMap, Dictionary<string, string> columnFilters, StringDictionary columnExcludeFilters)
	{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		string routine = this.GetType().FullName + ".createCopy";
		// List of columns that will be copied
		string[] columnNamesToCopy = null;
		// TODO SAM 2013-11-25 Remove code if the functionality works
		//if ( (distinctColumns != null) && (distinctColumns.length > 0) ) {
		//    // Distinct overrides requested column names
		//    reqIncludeColumns = distinctColumns;
		//}
		if ((reqIncludeColumns != null) && (reqIncludeColumns.Length > 0))
		{
			// Copy only the requested names
			columnNamesToCopy = reqIncludeColumns;
		}
		else
		{
			// Copy all
			columnNamesToCopy = table.getFieldNames();
		}
		/* TODO SAM 2013-11-26 Remove this once tested - distinct columns are NOT required to be in output
		if ( (distinctColumns != null) && (distinctColumns.length > 0) ) {
		    // Add the distinct columns to the requested columns if not already included
		    boolean [] found = new boolean[distinctColumns.length];
		    int foundCount = 0;
		    for ( int id = 0; id < distinctColumns.length; id++ ) {
		        found[id] = false;
		        for ( int ir = 0; ir < reqIncludeColumns.length; ir++ ) {
		            if ( reqIncludeColumns[ir].equalsIgnoreCase(distinctColumns[id]) ) {
		                ++foundCount;
		                found[id] = true;
		                break;
		            }
		        }
		    }
		    if ( foundCount != distinctColumns.length ) { // At least one of the distinct columns was not found
		        String [] tmp = new String[reqIncludeColumns.length + (distinctColumns.length - foundCount)];
		        System.arraycopy(reqIncludeColumns, 0, tmp, 0, reqIncludeColumns.length);
		        int addCount = 0;
		        for ( int id = 0; id < distinctColumns.length; id++ ) {
		            if ( !found[id] ) {
		                tmp[tmp.length + addCount] = distinctColumns[id];
		                ++addCount; // Do after assignment above
		            }
		        }
		        reqIncludeColumns = tmp;
		    }
		}
		*/
		// Figure out which columns numbers should be copied.  Initialize an array with -1 and then set to
		// actual table columns if matching
		int errorCount = 0;
		StringBuilder errorMessage = new StringBuilder();
		int[] columnNumbersToCopy = new int[columnNamesToCopy.Length];
		for (int icol = 0; icol < columnNamesToCopy.Length; icol++)
		{
			try
			{
				columnNumbersToCopy[icol] = table.getFieldIndex(columnNamesToCopy[icol]);
			}
			catch (Exception)
			{
				columnNumbersToCopy[icol] = -1; // Requested column not matched
				++errorCount;
				if (errorMessage.Length > 0)
				{
					errorMessage.Append(" ");
				}
				errorMessage.Append("Requested column \"" + columnNamesToCopy[icol] + "\" not found in existing table.");
			}
		}
		// Get (include) filter columns and glob-style regular expressions
		if (columnFilters == null)
		{
			columnFilters = new Dictionary<string, string>();
		}
		int[] columnNumbersToFilter = new int[columnFilters.Count];
		string[] columnFilterGlobs = new string[columnFilters.Count];
		IEnumerator<string> keys = columnFilters.Keys.GetEnumerator();
		int ikey = -1;
		string key = null;
		while (keys.MoveNext())
		{
			++ikey;
			columnNumbersToFilter[ikey] = -1;
			try
			{
				key = keys.Current;
				columnNumbersToFilter[ikey] = table.getFieldIndex(key);
				columnFilterGlobs[ikey] = columnFilters[key];
				// Turn default globbing notation into internal Java regex notation
				columnFilterGlobs[ikey] = columnFilterGlobs[ikey].Replace("*", ".*").ToUpper();
			}
			catch (Exception)
			{
				++errorCount;
				if (errorMessage.Length > 0)
				{
					errorMessage.Append(" ");
				}
				errorMessage.Append("ColumnFilters \"" + key + "\" not found in existing table.");
			}
		}
		// Get exclude filter columns and glob-style regular expressions
		int[] columnExcludeFiltersNumbers = new int[0];
		string[] columnExcludeFiltersGlobs = null;
		if (columnExcludeFilters != null)
		{
			LinkedHashMap<string, string> map = columnExcludeFilters.getLinkedHashMap();
			columnExcludeFiltersNumbers = new int[map.size()];
			columnExcludeFiltersGlobs = new string[map.size()];
			ikey = -1;
			foreach (KeyValuePair<string, string> entry in map.entrySet())
			{
				++ikey;
				columnExcludeFiltersNumbers[ikey] = -1;
				try
				{
					key = entry.Key;
					columnExcludeFiltersNumbers[ikey] = table.getFieldIndex(key);
					columnExcludeFiltersGlobs[ikey] = map.get(key);
					// Turn default globbing notation into internal Java regex notation
					columnExcludeFiltersGlobs[ikey] = columnExcludeFiltersGlobs[ikey].Replace("*", ".*").ToUpper();
				}
				catch (Exception)
				{
					++errorCount;
					if (errorMessage.Length > 0)
					{
						errorMessage.Append(" ");
					}
					errorMessage.Append("ColumnExcludeFilters column \"" + key + "\" not found in existing table.");
				}
			}
		}
		int[] distinctColumnNumbers = null;
		if ((distinctColumns != null) && (distinctColumns.Length > 0))
		{
			distinctColumnNumbers = new int[distinctColumns.Length];
			for (int id = 0; id < distinctColumns.Length; id++)
			{
				distinctColumnNumbers[id] = -1;
				try
				{
					distinctColumnNumbers[id] = table.getFieldIndex(distinctColumns[id]);
				}
				catch (Exception)
				{
					distinctColumnNumbers[id] = -1; // Distinct column not matched
					++errorCount;
					if (errorMessage.Length > 0)
					{
						errorMessage.Append(" ");
					}
					errorMessage.Append("Distinct column \"" + distinctColumns[id] + "\" not found in existing table.");
				}
			}
		}
		// Create a new data table with the requested column names
		DataTable newTable = new DataTable();
		newTable.setTableID(newTableID);
		// Get the column information from the original table
		object newColumnNameO = null; // Used to map column names
		TableField newTableField; // New table field
		IList<object []> distinctList = new List<object []>(); // Unique combinations of requested distinct column values
		// Create requested columns in the output table
		for (int icol = 0; icol < columnNumbersToCopy.Length; icol++)
		{
			if (columnNumbersToCopy[icol] == -1)
			{
				// Did not find the column in the table so add a String column for null values
				newTableField = new TableField(TableField.DATA_TYPE_STRING, columnNamesToCopy[icol], -1, -1);
			}
			else
			{
				// Copy the data from the original table
				// First make a copy of the existing table field
				newTableField = new TableField(table.getTableField(columnNumbersToCopy[icol]));
			}
			if (columnMap != null)
			{
				newColumnNameO = columnMap[newTableField.getName()];
				if (newColumnNameO != null)
				{
					// Reset the column name with the new name
					newTableField.setName((string)newColumnNameO);
				}
			}
			newTable.addField(newTableField, null);
		}
		// Now loop through all the data records and copy to the output table
		int icol;
		int irowCopied = 0;
		bool somethingCopied = false;
		bool filterMatches, distinctMatches;
		object o = null;
		object[] oDistinctCheck = null;
		if ((distinctColumnNumbers != null) && (distinctColumnNumbers.Length > 0))
		{
			oDistinctCheck = new object[distinctColumnNumbers.Length];
		}
		string s;
		int distinctMatchesCount = 0; // The number of distinct column value that match the current row
		for (int irow = 0; irow < table.getNumberOfRecords(); irow++)
		{
			somethingCopied = false;
			filterMatches = true;
			if (columnNumbersToFilter.Length > 0)
			{
				// Filters can be done on any columns so loop through to see if row matches before doing copy
				for (icol = 0; icol < columnNumbersToFilter.Length; icol++)
				{
					if (columnNumbersToFilter[icol] < 0)
					{
						filterMatches = false;
						break;
					}
					try
					{
						o = table.getFieldValue(irow, columnNumbersToFilter[icol]);
						if (o == null)
						{
							filterMatches = false;
							break; // Don't include nulls when checking values
						}
						s = ("" + o).ToUpper();
						if (!s.matches(columnFilterGlobs[icol]))
						{
							// A filter did not match so don't copy the record
							filterMatches = false;
							break;
						}
					}
					catch (Exception e)
					{
						errorMessage.Append("Error getting table data for filter check [" + irow + "][" + columnNumbersToFilter[icol] + "].");
						Message.printWarning(3, routine, "Error getting table data for [" + irow + "][" + columnNumbersToFilter[icol] + "] uniquetempvar.");
						++errorCount;
					}
				}
				if (!filterMatches)
				{
					// Skip the record.
					continue;
				}
			}
			// If here need to check the exclude filters on the row
			if (columnExcludeFiltersNumbers.Length > 0)
			{
				int matchesCount = 0;
				// Filters can be done on any columns so loop through to see if row matches before doing copy
				for (icol = 0; icol < columnExcludeFiltersNumbers.Length; icol++)
				{
					if (columnExcludeFiltersNumbers[icol] < 0)
					{
						// Can't do filter so don't try
						break;
					}
					try
					{
						o = table.getFieldValue(irow, columnExcludeFiltersNumbers[icol]);
						if (o == null)
						{
							if (columnExcludeFiltersGlobs[icol].Length == 0)
							{
								// Trying to match blank cells
								++matchesCount;
							}
							else
							{ // Don't include nulls when checking values
								break;
							}
						}
						s = ("" + o).ToUpper();
						if (s.matches(columnExcludeFiltersGlobs[icol]))
						{
							// A filter matched so don't copy the record
							++matchesCount;
						}
					}
					catch (Exception e)
					{
						errorMessage.Append("Error getting table data for filter check [" + irow + "][" + columnExcludeFiltersNumbers[icol] + "].");
						Message.printWarning(3, routine, "Error getting table data for [" + irow + "][" + columnExcludeFiltersNumbers[icol] + "] uniquetempvar.");
						++errorCount;
					}
				}
				if (matchesCount == columnExcludeFiltersNumbers.Length)
				{
					// Skip the record since all filters were matched
					continue;
				}
			}
			// If here then the row is OK to include
			if ((distinctColumnNumbers != null) && (distinctColumnNumbers.Length > 0))
			{
				// Distinct columns can be done on any columns so loop through to see if row matches before doing copy
				// First retrieve the objects and store in an array because a distinct combinations of 1+ values is checked
				distinctMatches = false;
				for (icol = 0; icol < distinctColumnNumbers.Length; icol++)
				{
					if (distinctColumnNumbers[icol] < 0)
					{
						break;
					}
					try
					{
						// This array is reused but will be copied below if needed to save
						oDistinctCheck[icol] = table.getFieldValue(irow, distinctColumnNumbers[icol]);
					}
					catch (Exception e)
					{
						errorMessage.Append("Error getting table data checking distinct for [" + irow + "][" + distinctColumnNumbers[icol] + "].");
						Message.printWarning(3, routine, "Error getting table data for [" + irow + "][" + distinctColumnNumbers[icol] + "] uniquetempvar.");
						++errorCount;
					}
				}
				// Now actually check the values
				foreach (object [] odArray in distinctList)
				{
					distinctMatchesCount = 0;
					for (icol = 0; icol < distinctColumnNumbers.Length; icol++)
					{
						if ((oDistinctCheck[icol] == null) || ((oDistinctCheck[icol] is string) && ((string)oDistinctCheck[icol]).Trim().Length == 0))
						{
							// TODO SAM 2013-11-25 Don't include nulls and blank strings in distinct values
							// Might need to change this in the future if those values have relevance
							continue;
						}
						if (odArray[icol].Equals(oDistinctCheck[icol]))
						{
							++distinctMatchesCount;
						}
					}
					if (distinctMatchesCount == distinctColumnNumbers.Length)
					{
						// The columns of interest matched a distinct combination so skip adding the record.
						distinctMatches = true;
						break;
					}
				}
				if (distinctMatches)
				{
					// The columns of interest matched a distinct combination so skip adding the record.
					continue;
				}
				else
				{
					// Create a copy of the temporary object to save and use below.
					object[] oDistinctCheckCopy = new object[distinctColumnNumbers.Length];
					Array.Copy(oDistinctCheck, 0, oDistinctCheckCopy, 0, distinctColumnNumbers.Length);
					distinctList.Add(oDistinctCheckCopy); // Have another combination of distinct values to check for other table rows
					// The row will be added below
				}
			}
			// If here then the row can be added.
			for (icol = 0; icol < columnNumbersToCopy.Length; icol++)
			{
				try
				{
					if (columnNumbersToCopy[icol] < 0)
					{
						// Value in new table is null
						newTable.setFieldValue(irowCopied, icol, null, true);
					}
					else
					{
						// Value in new table is copied from original
						// TODO SAM 2013-08-06 Need to evaluate - following is OK for immutable objects but what about DateTime, etc?
						newTable.setFieldValue(irowCopied, icol, table.getFieldValue(irow, columnNumbersToCopy[icol]), true);
					}
					somethingCopied = true;
				}
				catch (Exception e)
				{
					// Should not happen
					errorMessage.Append("Error setting new table data copying [" + irow + "][" + columnNumbersToCopy[icol] + "].");
					Message.printWarning(3, routine, "Error setting new table data for [" + irow + "][" + columnNumbersToCopy[icol] + "] uniquetempvar.");
					++errorCount;
				}
			}
			if (somethingCopied)
			{
				++irowCopied;
			}
		}
		if (errorCount > 0)
		{
			throw new Exception("There were + " + errorCount + " errors transferring data to new table: " + errorMessage);
		}
		return newTable;
	}

	/// <summary>
	/// Deletes a field and all the field's data from the table. </summary>
	/// <param name="fieldNum"> the number of the field to delete. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void deleteField(int fieldNum) throws Exception
	public virtual void deleteField(int fieldNum)
	{
		if (fieldNum < 0 || fieldNum > (_table_fields.Count - 1))
		{
			throw new Exception("Field number " + fieldNum + " out of bounds.");
		}
		_table_fields.RemoveAt(fieldNum);

		int size = _table_records.Count;
		TableRecord record = null;
		for (int i = 0; i < size; i++)
		{
			record = _table_records[i];
			record.deleteField(fieldNum);
		}
	}

	/// <summary>
	/// Deletes a record from the table. </summary>
	/// <param name="recordNum"> the number of the record to delete. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void deleteRecord(int recordNum) throws Exception
	public virtual void deleteRecord(int recordNum)
	{
		if (recordNum < 0 || recordNum > (_table_records.Count - 1))
		{
			throw new Exception("Record number " + recordNum + " out of bounds.");
		}

		_table_records.RemoveAt(recordNum);
	}

	/// <summary>
	/// Dumps a table to Status level 1. </summary>
	/// <param name="delimiter"> the delimiter to use. </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void dumpTable(String delimiter) throws Exception
	public virtual void dumpTable(string delimiter)
	{
		string routine = "DataTable.dumpTable";
		int cols = getNumberOfFields();
		int rows = getNumberOfRecords();
		string rowPlural = "s";
		if (rows == 1)
		{
			rowPlural = "";
		}
		string colPlural = "s";
		if (cols == 1)
		{
			colPlural = "";
		}
		Message.printStatus(1, "", "Table has " + rows + " row" + rowPlural + " and " + cols + " column" + colPlural + ".");

		if (cols == 0)
		{
			Message.printWarning(2, routine, "Table has 0 columns!  Nothing will be written.");
			return;
		}

		string line = "";
		for (int col = 0; col < (cols - 1); col++)
		{
			line += getFieldName(col) + delimiter;
		}
		line += getFieldName((cols - 1));
		Message.printStatus(1, "", line);

		for (int row = 0; row < rows; row++)
		{
			line = "";
			for (int col = 0; col < (cols - 1); col++)
			{
				line += "" + getFieldValue(row, col) + delimiter;
			}
			line += getFieldValue(row, (cols - 1));

			Message.printStatus(2, "", line);
		}
	}

	/// <summary>
	/// Copies a DataTable. </summary>
	/// <param name="originalTable"> the table to be copied. </param>
	/// <param name="cloneData"> if true, the data in the table will be cloned.  If false, both
	/// tables will have pointers to the same data. </param>
	/// <returns> the new copy of the table. </returns>
	public static DataTable duplicateDataTable(DataTable originalTable, bool cloneData)
	{
		string routine = "DataTable.duplicateDataTable";

		DataTable newTable = null;
		int numFields = originalTable.getNumberOfFields();

		TableField field = null;
		TableField newField = null;
		IList<TableField> tableFields = new List<TableField>();
		for (int i = 0; i < numFields; i++)
		{
			field = originalTable.getTableField(i);
			newField = new TableField(field.getDataType(), field.getName(), field.getWidth(), field.getPrecision());
			tableFields.Add(newField);
		}
		newTable = new DataTable(tableFields);
		if (!cloneData)
		{
			return newTable;
		}
		newTable._haveDataInMemory = true;

		int numRecords = originalTable.getNumberOfRecords();
		int type = -1;
		TableRecord newRecord = null;
		for (int i = 0; i < numRecords; i++)
		{
			try
			{
				newRecord = new TableRecord(numFields);
				for (int j = 0; j < numFields; j++)
				{
					type = newTable.getFieldDataType(j);
					if (type == TableField.DATA_TYPE_INT)
					{
						newRecord.addFieldValue(new int?(((int?)originalTable.getFieldValue(i, j)).Value));
					}
					else if (type == TableField.DATA_TYPE_SHORT)
					{
						newRecord.addFieldValue(new short?(((short?)originalTable.getFieldValue(i, j)).Value));
					}
					else if (type == TableField.DATA_TYPE_DOUBLE)
					{
						newRecord.addFieldValue(new double?(((double?)originalTable.getFieldValue(i, j)).Value));
					}
					else if (type == TableField.DATA_TYPE_FLOAT)
					{
						newRecord.addFieldValue(new float?(((float?)originalTable.getFieldValue(i, j)).Value));
					}
					else if (type == TableField.DATA_TYPE_STRING)
					{
						newRecord.addFieldValue((string)originalTable.getFieldValue(i, j));
					}
					else if (type == TableField.DATA_TYPE_DATE)
					{
						newRecord.addFieldValue(((System.DateTime)originalTable.getFieldValue(i, j)).clone());
					}
					else if (type == TableField.DATA_TYPE_DATETIME)
					{
						newRecord.addFieldValue(((DateTime)originalTable.getFieldValue(i, j)).clone());
					}
					else if (type == TableField.DATA_TYPE_LONG)
					{
						newRecord.addFieldValue(new long?(((long?)originalTable.getFieldValue(i, j)).Value));
					}
				}
				newTable.addRecord(newRecord);
			}
			catch (Exception e)
			{
				Message.printWarning(2, routine, "Error adding record " + i + " to table.");
				Message.printWarning(2, routine, e);
			}
		}
		return newTable;
	}

	/// <summary>
	/// Return a new TableRecord that is compatible with this table, where all values are null.  This is useful
	/// for inserting new table records where only specific column value is known, in which case the record can be
	/// modified with TableRecord.setFieldValue(). </summary>
	/// <returns> a new record with null objects in each value. </returns>
	public virtual TableRecord emptyRecord()
	{
		TableRecord newRecord = new TableRecord();
		int nCol = getNumberOfFields();
		for (int i = 0; i < nCol; i++)
		{
			newRecord.addFieldValue(null);
		}
		return newRecord;
	}

	/// <summary>
	/// Used internally when parsing a delimited file to determine whether a field name is already present in a 
	/// table's fields, so as to avoid duplication. </summary>
	/// <param name="tableFields"> a list of the tableFields created so far for a table. </param>
	/// <param name="name"> the name of the field to check. </param>
	/// <returns> true if the field name already is present in the table fields, false if not. </returns>
	private static bool findPreviousFieldNameOccurances(IList<TableField> tableFields, string name)
	{
		int size = tableFields.Count;
		TableField field = null;
		string fieldName = null;
		for (int i = 0; i < size; i++)
		{
			field = tableFields[i];
			fieldName = field.getName();
			if (name.Equals(fieldName))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Format the contents of an array column into a string.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String formatArrayColumn(int row, int col) throws Exception
	public virtual string formatArrayColumn(int row, int col)
	{
		// Get the internal data type
		int columnType = getFieldDataTypes()[col];
		int dataType = columnType - TableField.DATA_TYPE_ARRAY_BASE;
		// For the purposes of rendering in the table, treat array as formatted string [ val1, val2, ... ]
		// Where the formatting of the values is for the raw value
		StringBuilder b = new StringBuilder("[");
		object oa = null;
		switch (dataType)
		{
			case TableField.DATA_TYPE_DATETIME:
				DateTime[] dta = (DateTime [])getFieldValue(row,col);
				for (int i = 0; i < dta.Length; i++)
				{
					if (i > 0)
					{
						b.Append(",");
					}
					if (dta[i] != null)
					{
						b.Append("\"" + dta[i] + "\"");
					}
				}
				break;
			case TableField.DATA_TYPE_DOUBLE:
				oa = getFieldValue(row,col);
				double[] da = new double[0];
				if (oa == null)
				{
					return null;
				}
				else if (oa is double[])
				{
					da = (double [])oa;
				}
				else if (oa is double?[])
				{
					double?[] Da = (double? [])oa;
					da = new double[Da.Length];
					for (int i = 0; i < da.Length; i++)
					{
						if (DMIUtil.isMissing(Da[i]))
						{
							da[i] = DMIUtil.MISSING_DOUBLE;
						}
						else
						{
							da[i] = Da[i].Value;
						}
					}
				}
				else
				{
					throw new Exception("Don't know how to handle double array - is not double[] or Double[]");
				}
				for (int i = 0; i < da.Length; i++)
				{
					if (i > 0)
					{
						b.Append(",");
					}
					if (!DMIUtil.isMissing(da[i]))
					{
						// Need to get the TableField format because the overall column will be string
						// TODO SAM 2015-09-06
						//b.append(StringUtil.formatString(da[i],__fieldFormats[col]));
						b.Append(StringUtil.formatString(da[i],"%.6f"));
					}
				}
				break;
			case TableField.DATA_TYPE_FLOAT:
				oa = getFieldValue(row,col);
				float[] fa = new float[0];
				if (oa == null)
				{
					return null;
				}
				else if (oa is float[])
				{
					fa = (float [])oa;
				}
				else if (oa is float?[])
				{
					float?[] Fa = (float? [])oa;
					fa = new float[Fa.Length];
					for (int i = 0; i < fa.Length; i++)
					{
						if (DMIUtil.isMissing(Fa[i]))
						{
							fa[i] = DMIUtil.MISSING_FLOAT;
						}
						else
						{
							fa[i] = Fa[i].Value;
						}
					}
				}
				else
				{
					throw new Exception("Don't know how to handle float array - is not float[] or Float[]");
				}
				for (int i = 0; i < fa.Length; i++)
				{
					if (i > 0)
					{
						b.Append(",");
					}
					if (!DMIUtil.isMissing(fa[i]))
					{
						// Need to get the TableField format because the overall column will be string
						// TODO SAM 2015-09-06
						//b.append(StringUtil.formatString(da[i],__fieldFormats[col]));
						b.Append(StringUtil.formatString(fa[i],"%.6f"));
					}
				}
				break;
			case TableField.DATA_TYPE_INT:
				oa = getFieldValue(row,col);
				int[] ia = new int[0];
				if (oa == null)
				{
					return null;
				}
				else if (oa is int[])
				{
					ia = (int [])oa;
				}
				else if (oa is int?[])
				{
					int?[] Ia = (int? [])oa;
					ia = new int[Ia.Length];
					for (int i = 0; i < ia.Length; i++)
					{
						if (DMIUtil.isMissing(Ia[i]))
						{
							ia[i] = DMIUtil.MISSING_INT;
						}
						else
						{
							ia[i] = Ia[i].Value;
						}
					}
				}
				else
				{
					throw new Exception("Don't know how to handle integer array - is not int[] or Integer[]");
				}
				for (int i = 0; i < ia.Length; i++)
				{
					if (i > 0)
					{
						b.Append(",");
					}
					if (!DMIUtil.isMissing(ia[i]))
					{
						// Need to get the TableField format because the overall column will be string
						// TODO SAM 2015-09-06
						//b.append(StringUtil.formatString(da[i],__fieldFormats[col]));
						b.Append(ia[i]);
					}
				}
				break;
			case TableField.DATA_TYPE_LONG:
				oa = getFieldValue(row,col);
				long[] la = new long[0];
				if (oa == null)
				{
					return null;
				}
				else if (oa is long[])
				{
					la = (long [])oa;
				}
				else if (oa is long?[])
				{
					long?[] La = (long? [])oa;
					la = new long[La.Length];
					for (int i = 0; i < la.Length; i++)
					{
						if (DMIUtil.isMissing(La[i]))
						{
							la[i] = DMIUtil.MISSING_LONG;
						}
						else
						{
							la[i] = La[i].Value;
						}
					}
				}
				else
				{
					throw new Exception("Don't know how to handle long array - is not long[] or Long[]");
				}
				for (int i = 0; i < la.Length; i++)
				{
					if (i > 0)
					{
						b.Append(",");
					}
					if (!DMIUtil.isMissing(la[i]))
					{
						// Need to get the TableField format because the overall column will be string
						// TODO SAM 2015-09-06
						//b.append(StringUtil.formatString(da[i],__fieldFormats[col]));
						b.Append(la[i]);
					}
				}
				break;
			case TableField.DATA_TYPE_BOOLEAN:
				oa = getFieldValue(row,col);
				bool?[] Ba = new bool?[0]; // Use Boolean object array because boolean can't indicate null value
				if (oa == null)
				{
					return null;
				}
				else if (oa is bool?[])
				{
					Ba = (bool? [])oa;
				}
				else if (oa is bool[])
				{
					bool[] ba = (bool [])oa;
					Ba = new bool?[ba.Length];
					for (int i = 0; i < ba.Length; i++)
					{
						// No need to check for missing
						Ba[i] = ba[i];
					}
				}
				else
				{
					throw new Exception("Don't know how to handle boolean array - is not boolean[] or Boolean[]");
				}
				for (int i = 0; i < Ba.Length; i++)
				{
					if (i > 0)
					{
						b.Append(",");
					}
					if (!DMIUtil.isMissing(Ba[i]))
					{
						// Need to get the TableField format because the overall column will be string
						// TODO SAM 2015-09-06
						//b.append(StringUtil.formatString(da[i],__fieldFormats[col]));
						b.Append(Ba[i]);
					}
				}
				break;
			case TableField.DATA_TYPE_STRING:
				string[] sa = (string [])getFieldValue(row,col);
				if (sa == null)
				{
					return null;
				}
				for (int i = 0; i < sa.Length; i++)
				{
					if (i > 0)
					{
						b.Append(",");
					}
					if (!string.ReferenceEquals(sa[i], null))
					{
						// Need to get the TableField format because the overall column will be string
						// TODO SAM 2015-09-06
						//b.append(StringUtil.formatString(da[i],__fieldFormats[col]));
						// TODO SAM 2015-11-05 Need to decide if strings in array should be quoted
						b.Append(sa[i]);
					}
				}
				break;
			default:
				// Don't know the type so don't know how to format the array. Just leave blank
				break;
		}
		b.Append("]");
		return b.ToString();
	}

	/// <summary>
	/// Return the time series comments. </summary>
	/// <returns> The comments list. </returns>
	public virtual IList<string> getComments()
	{
		return __comments;
	}

	/// <summary>
	/// Return the field data type, given an index. </summary>
	/// <returns> Data type for specified zero-based index. </returns>
	/// <param name="index"> field index (0+). </param>
	public virtual int getFieldDataType(int index)
	{
		if (_table_fields.Count <= index)
		{
			throw new System.IndexOutOfRangeException("Table field index " + index + " is not valid.");
		}
		return (_table_fields[index]).getDataType();
	}

	/// <summary>
	/// Return the field data types for all of the fields.  This is useful because
	/// code that processes all the fields can request the information once and then re-use. </summary>
	/// <returns> Data types for all fields, in an integer array or null if no fields. </returns>
	public virtual int[] getFieldDataTypes()
	{
		int size = getNumberOfFields();
		if (size == 0)
		{
			return null;
		}
		int[] types = new int[size];
		for (int i = 0; i < size; i++)
		{
			types[i] = getFieldDataType(i);
		}
		return types;
	}

	/// <summary>
	/// Get C-style format specifier that can be used to format field values for
	/// output.  This format can be used with StringUtil.formatString().  All fields
	/// formats are set to the full width and precision defined for the field.  Strings
	/// are left-justified and numbers are right justified. </summary>
	/// <returns> a String format specifier. </returns>
	/// <param name="index"> Field index (zero-based). </param>
	public virtual string getFieldFormat(int index)
	{
		int fieldType = getFieldDataType(index);
		int fieldWidth = getFieldWidth(index);
		if (fieldType == TableField.DATA_TYPE_STRING)
		{
			// Output left-justified and padded...
			if (fieldWidth < 0)
			{
				// Variable width strings
				return "%-s";
			}
			else
			{
				return "%-" + fieldWidth + "." + getFieldWidth(index) + "s";
			}
		}
		else
		{
			if ((fieldType == TableField.DATA_TYPE_FLOAT) || (fieldType == TableField.DATA_TYPE_DOUBLE))
			{
				int precision = getFieldPrecision(index);
				if (fieldWidth < 0)
				{
					if (precision < 0)
					{
						// No width precision specified - rely on data object representation
						return "%f";
					}
					else
					{
						return "%." + precision + "f";
					}
				}
				else
				{
					return "%" + fieldWidth + "." + precision + "f";
				}
			}
			else
			{
				return "%" + fieldWidth + "d";
			}
		}
	}

	/// <summary>
	/// Get C-style format specifiers that can be used to format field values for
	/// output.  These formats can be used with StringUtil.formatString(). </summary>
	/// <returns> a new String array with the format specifiers. </returns>
	public virtual string[] getFieldFormats()
	{
		int nfields = getNumberOfFields();
		string[] format_spec = new string[nfields];
		for (int i = 0; i < nfields; i++)
		{
			format_spec[i] = getFieldFormat(i);
		}
		return format_spec;
	}

	/// <summary>
	/// Return the field index associated with the given field name. </summary>
	/// <returns> Index of table entry associated with the given field name. </returns>
	/// <param name="field_name"> Field name to look up. </param>
	/// <exception cref="Exception"> if the field name is not found. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int getFieldIndex(String field_name) throws Exception
	public virtual int getFieldIndex(string field_name)
	{
		int num = _table_fields.Count;
		for (int i = 0; i < num; i++)
		{
			if ((_table_fields[i]).getName().Equals(field_name, StringComparison.OrdinalIgnoreCase))
			{
				return i;
			}
		}

		// if this line is reached, the given field was never found
		throw new Exception("Unable to find table field with name \"" + field_name + "\" in table \"" + getTableID() + "\"");
	}

	/// <summary>
	/// Return the field indices associated with the given field names.  This method simply
	/// calls getFieldIndex() for each requested name. </summary>
	/// <returns> array of indices associated with the given field names. </returns>
	/// <param name="fieldNames"> Field names to look up. </param>
	/// <exception cref="Exception"> if any field name is not found. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int [] getFieldIndices(String [] fieldNames) throws Exception
	public virtual int [] getFieldIndices(string[] fieldNames)
	{
		int[] fieldIndices = new int[fieldNames.Length];
		for (int i = 0; i < fieldNames.Length; i++)
		{
			fieldIndices[i] = getFieldIndex(fieldNames[i]);
		}
		return fieldIndices;
	}

	/// <summary>
	/// Return the field name, given an index. </summary>
	/// <returns> Field name for specified zero-based index. </returns>
	/// <param name="index"> field index. </param>
	public virtual string getFieldName(int index)
	{
		return (_table_fields[index]).getName();
	}

	/// <summary>
	/// Return the field names for all fields. </summary>
	/// <returns> a String array with the field names. </returns>
	public virtual string[] getFieldNames()
	{
		int nfields = getNumberOfFields();
		string[] field_names = new string[nfields];
		for (int i = 0; i < nfields; i++)
		{
			field_names[i] = getFieldName(i);
		}
		return field_names;
	}

	/// <summary>
	/// Return the field precision, given an index. </summary>
	/// <returns> Field precision for specified zero-based index. </returns>
	/// <param name="index"> field index. </param>
	public virtual int getFieldPrecision(int index)
	{
		return (_table_fields[index]).getPrecision();
	}

	/// <summary>
	/// Return the field value for the requested record and field name.
	/// The overloaded method that takes integers should be called for optimal
	/// performance (so the field name lookup is avoided). </summary>
	/// <param name="record_index"> zero-based index of record </param>
	/// <param name="field_name"> Field name of field to read. </param>
	/// <returns> field value for the specified field name of the specified record index
	/// The returned object must be properly cast. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object getFieldValue(long record_index, String field_name) throws Exception
	public virtual object getFieldValue(long record_index, string field_name)
	{
		return getFieldValue(record_index, getFieldIndex(field_name));
	}

	/// <summary>
	/// Return the field value for the requested record and field index.  <b>Note that
	/// this method can be overruled to implement on-the-fly data reads.  For example,
	/// the DbaseDataTable class overrules this method to allow data to be read from the
	/// binary Dbase file, as needed, at run-time, rather than reading from memory.  In
	/// this case, the haveData() method can be used to indicate if data should be
	/// taken from memory (using this method) or read from file (using a derived class method).</b> </summary>
	/// <param name="record_index"> zero-based index of record </param>
	/// <param name="field_index"> zero_based index of desired field </param>
	/// <returns> field value for the specified index of the specified record index
	/// The returned object must be properly cast. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object getFieldValue(long record_index, int field_index) throws Exception
	public virtual object getFieldValue(long record_index, int field_index)
	{
		int num_recs = _table_records.Count;
		int num_fields = _table_fields.Count;

		if (num_recs <= record_index)
		{
			throw new Exception("Requested record index " + record_index + " is not available (only " + num_recs + " are available).");
		}

		if (num_fields <= field_index)
		{
			throw new Exception("Requested field index " + field_index + " is not available (only " + num_fields + " have been established.");
		}

		TableRecord tableRecord = _table_records[(int)record_index];
		object o = tableRecord.getFieldValue(field_index);
		tableRecord = null;
		return o;
	}

	/// <summary>
	/// Return the field values for all rows in the table for the requested field/column. </summary>
	/// <returns> the field values for all rows in the table for the requested field/column </returns>
	/// <param name="fieldName"> name of field for which to return values for all rows </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.util.List<Object> getFieldValues(String fieldName) throws Exception
	public virtual IList<object> getFieldValues(string fieldName)
	{
		IList<object> values = new List<object>();
		int columnNum = getFieldIndex(fieldName);
		int size = getNumberOfRecords();
		for (int i = 0; i < size; i++)
		{
			values.Add(getFieldValue(i,columnNum));
		}
		return values;
	}

	/// <summary>
	/// Return the field width, given an index. </summary>
	/// <returns> Field width for specified zero-based index. </returns>
	/// <param name="index"> field index. </param>
	public virtual int getFieldWidth(int index)
	{
		return (_table_fields[index]).getWidth();
	}

	/// <summary>
	/// Return the number of fields in the table. </summary>
	/// <returns> number of fields in the table. </returns>
	public virtual int getNumberOfFields()
	{
		return _table_fields.Count;
	}

	// TODO SAM 2010-09-22 Evaluate whether the records list size should be returned if records in memory?
	/// <summary>
	/// Return the number of records in the table.  <b>This value should be set by
	/// code that manipulates the data table.  If the table records list has been
	/// manipulated with a call to addRecord(), the size of the list will be returned.
	/// Otherwise, the setNumberOfRecords() methods should be called appropriately and
	/// its the value that is set will be returned.  This latter case
	/// will be in effect if tables are being read on-the-fly.</b> </summary>
	/// <returns> number of records in the table. </returns>
	public virtual int getNumberOfRecords()
	{
		if (_add_record_called)
		{
			return _table_records.Count;
		}
		else
		{
			return _num_records;
		}
	}

	/// <summary>
	/// Return the TableRecord at a record index. </summary>
	/// <param name="record_index"> Record index (zero-based). </param>
	/// <returns> TableRecord at specified record_index </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TableRecord getRecord(int record_index) throws Exception
	public virtual TableRecord getRecord(int record_index)
	{
		if (!_haveDataInMemory)
		{
			// Most likely a derived class is not handling on the fly
			// reading of data and needs more development.  Return null
			// because the limitation is likely handled elsewhere.
			return null;
		}
		if (_table_records.Count <= record_index)
		{
			throw new Exception("Unable to return TableRecord at index [" + record_index + "].  Max value allowed is " + (_table_records.Count - 1) + ".");
		}
		return (_table_records[record_index]);
	}

	/// <summary>
	/// Return the TableRecord for the given column and column value.  If multiple records are matched the first record is returned. </summary>
	/// <param name="columnNum"> column number, 0+ </param>
	/// <param name="columnValue"> column value to match in the records.  The first matching record is returned.
	/// The type of the object will be checked before doing the comparison. </param>
	/// <returns> TableRecord matching the specified column value or null if no record is matched. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TableRecord getRecord(int columnNum, Object columnValue) throws Exception
	public virtual TableRecord getRecord(int columnNum, object columnValue)
	{
		int[] columnNums = new int[1];
		columnNums[0] = columnNum;
		IList<object> columnValues = new List<object>();
		columnValues.Add(columnValue);
		IList<TableRecord> records = getRecords(columnNums, columnValues);
		if (records.Count == 0)
		{
			return null;
		}
		else
		{
			return records[0];
		}
	}

	/// <summary>
	/// Return the TableRecord for the given column and column value.  If multiple records are matched
	/// the first record is returned. </summary>
	/// <param name="columnName"> name of column (field), case-insensitive. </param>
	/// <param name="columnValue"> column value to match in the records.  The first matching record is returned.
	/// The type of the object will be checked before doing the comparison. </param>
	/// <returns> TableRecord matching the specified column value or null if no record is matched. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TableRecord getRecord(String columnName, Object columnValue) throws Exception
	public virtual TableRecord getRecord(string columnName, object columnValue)
	{
		IList<string> columnNames = new List<string>();
		columnNames.Add(columnName);
		IList<object> columnValues = new List<object>();
		columnValues.Add(columnValue);
		IList<TableRecord> records = getRecords(columnNames, columnValues);
		if (records.Count == 0)
		{
			return null;
		}
		else
		{
			return records[0];
		}
	}

	/// <summary>
	/// Return a list of TableRecord matching the given columns and column values. </summary>
	/// <param name="columnNames"> list of column (field) names, case-insensitive. </param>
	/// <param name="columnValue"> list of column values to match in the records.
	/// The type of the object will be checked before doing the comparison. </param>
	/// <returns> TableRecord matching the specified column value, guaranteed to be non-null but may be zero length. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.util.List<TableRecord> getRecords(java.util.List<String> columnNames, java.util.List<? extends Object> columnValues) throws Exception
	public virtual IList<TableRecord> getRecords<T1>(IList<string> columnNames, IList<T1> columnValues) where T1 : object
	{
		// Figure out the column numbers that will be checked
		int iColumn = -1;
		int[] columnNumbers = new int[columnNames.Count];
		IList<TableRecord> recList = new List<TableRecord>();
		foreach (string columnName in columnNames)
		{
			++iColumn;
			// If -1 is returned then a column name does not exist and no matches are possible
			columnNumbers[iColumn] = getFieldIndex(columnName);
			if (columnNumbers[iColumn] < 0)
			{
				return recList;
			}
		}
		return getRecords(columnNumbers, columnValues);
	}

	/// <summary>
	/// Return a list of TableRecord matching the given columns and column values. </summary>
	/// <param name="columnNumbers"> list of column (field) numbers, 0+.  Any values < 0 will result in an empty list being returned. </param>
	/// <param name="columnValue"> list of column values to match in the records.
	/// The type of the object will be checked before doing the comparison. </param>
	/// <returns> TableRecord matching the specified column value, guaranteed to be non-null but may be zero length. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.util.List<TableRecord> getRecords(int [] columnNumbers, java.util.List<? extends Object> columnValues) throws Exception
	public virtual IList<TableRecord> getRecords<T1>(int[] columnNumbers, IList<T1> columnValues) where T1 : object
	{
		if (!_haveDataInMemory)
		{
			// Most likely a derived class is not handling on the fly
			// reading of data and needs more development.  Return null
			// because the limitation is likely handled elsewhere.
			// TODO SAM 2013-07-02 Why not return an empty list here?
			return null;
		}
		IList<TableRecord> recList = new List<TableRecord>();
		// Make sure column numbers are valid.
		for (int iColumn = 0; iColumn < columnNumbers.Length; iColumn++)
		{
			if (columnNumbers[iColumn] < 0)
			{
				return recList;
			}
		}
		// Now search the the records and then the columns in the record
		object columnContents;
		int iColumn = -1;
		foreach (TableRecord rec in _table_records)
		{ // Loop through all table records
			int matchCount = 0; // How many column values match
			iColumn = -1;
			foreach (object columnValue in columnValues)
			{
				++iColumn;
				columnContents = rec.getFieldValue(columnNumbers[iColumn]);
				if (columnContents == null)
				{
					// Only match if both are match
					if (columnValue == null)
					{
						++matchCount;
					}
				}
				else if (getFieldDataType(columnNumbers[iColumn]) == TableField.DATA_TYPE_STRING)
				{
					// Do case insensitive comparison
					if (((string)columnValue).Equals("" + columnContents, StringComparison.OrdinalIgnoreCase))
					{
						++matchCount;
					}
				}
				else
				{
					// Not a string so just use the equals() method to compare.
					if (columnValue.Equals(columnContents))
					{
						++matchCount;
					}
				}
			}
			if (matchCount == columnValues.Count)
			{
				// Have matched the requested number of column values so add record to the match list
				recList.Add(rec);
			}
		}
		return recList;
	}

	/// <summary>
	/// Return the table identifier. </summary>
	/// <returns> the table identifier. </returns>
	public virtual string getTableID()
	{
		return __table_id;
	}

	/// <summary>
	/// Return the list of TableRecords. </summary>
	/// <returns> list of TableRecord. </returns>
	public virtual IList<TableRecord> getTableRecords()
	{
		return _table_records;
	}

	/// <summary>
	/// Return the TableField object for the requested column. </summary>
	/// <param name="index"> Table field index (zero-based). </param>
	/// <returns> TableField object for the specified zero-based index. </returns>
	public virtual TableField getTableField(int index)
	{
		return (_table_fields[index]);
	}

	/// <summary>
	/// Get the data type for the field. </summary>
	/// <returns> the data type for the field (see TableField.DATA_TYPE_*). </returns>
	/// <param name="index"> index of field (zero-based). </param>
	/// <exception cref="If"> the index is out of range. </exception>
	/// @deprecated use getFieldDataType 
	public virtual int getTableFieldType(int index)
	{
		if (_table_fields.Count <= index)
		{
			throw new System.IndexOutOfRangeException("Index " + index + " is not valid.");
		}
		return _table_fields[index].getDataType();
	}

	/// <summary>
	/// Return the unique field values for the requested field index.  This is used,
	/// for example, when displaying unique values on a map display.  The calling code
	/// will need to cast the returned objects appropriately.  The performance of this
	/// operation will degrade if a large number of unique values are present.  This
	/// should not normally be the case if the end-user is intelligent about their
	/// choice of the field that is being analyzed. </summary>
	/// <param name="field_index"> zero_based index of desired field </param>
	/// <returns> Simple array (e.g., double[]) of unique data values from the field.
	/// Depending on the field data type, a double[], int[], short[], or String[] will be returned. </returns>
	/// <exception cref="if"> the field index is not in the allowed range. </exception>
	/* TODO SAM Implement this later.
	public Object getUniqueFieldValues ( int field_index )
	throws Exception
	{	int num_recs = _table_records.size();
		int num_fields = _table_fields.size();
	
		if ( num_fields <= field_index ) {
			throw new Exception ( "Requested field index " + field_index +
			" is not available (only " + num_fields +
			" are available)." );
		}
	
		// Use a temporary list to get the unique values...
		Vector u = new Vector ( 100, 100 );
	
		// Determine the field type...
		int field_type = getTableFieldType ( field_index );
		//String rtn = "getFieldValue";
		//Message.printStatus ( 10, rtn, "Getting table record " +
		//	record_index + " from " + num_recs + " available records." );
		TableRecord tableRecord = null;
		Object o = null;
		for ( int i = 0; i < num_recs; i++ ) {
			tableRecord = (TableRecord)_table_records.elementAt(i);
			o = tableRecord.getFieldValue(field_index);
			// Now search through the list of known unique values...
			usize = u.size();
			for ( j = 0; j < usize; j++ ) {
			}
		}
		// Now return the values in an array of the appropriate type...
	}
	*/

	/// <summary>
	/// Checks to see if the table has a field with the given name. </summary>
	/// <param name="fieldName"> the name of the field to check for (case-sensitive). </param>
	/// <returns> true if the table has the field, false otherwise. </returns>
	public virtual bool hasField(string fieldName)
	{
		string[] fieldNames = getFieldNames();
		for (int i = 0; i < fieldNames.Length; i++)
		{
			if (fieldNames[i].Equals(fieldName))
			{
				return true;
			}
		}
		return false;
	}


	/// <summary>
	/// Indicate whether the table has data in memory.  This will be true if any table records
	/// have been added during a read or write operation.  This method is meant to be called by derived classes
	/// that allow records to be accessed on the fly rather than from memory (e.g., dBase tables).
	/// </summary>
	public virtual bool haveDataInMemory()
	{
		return _haveDataInMemory;
	}

	/// <summary>
	/// Initialize the data. </summary>
	/// <param name="tableFieldsList"> list of TableField used to define the DataTable. </param>
	/// <param name="listSize"> Initial list size for the list holding records. </param>
	/// <param name="sizeIncrement"> Increment for the list holding records. </param>
	private void initialize(IList<TableField> tableFieldsList, int listSize, int sizeIncrement)
	{
		_table_fields = tableFieldsList;
		_table_records = new List<TableRecord> (10);
	}

	/// <summary>
	/// Insert a table record into the table.  If inserting at the start or middle, the provided table record will be inserted and
	/// all other records will be shifted.  If inserting after the existing records, empty records will be added up to the requested
	/// insert position. </summary>
	/// <param name="row"> row position (0+) to insert the record </param>
	/// <param name="record"> table record to insert </param>
	/// <param name="doCheck"> indicate whether the record should be checked against the table for consistency; false inserts with no check
	/// (currently this parameter is not enabled).  Use emptyRecord() to create a record that matches the table design. </param>
	/// <exception cref="Exception"> if there is an error inserting the record </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void insertRecord(int row, TableRecord record, boolean doCheck) throws Exception
	public virtual void insertRecord(int row, TableRecord record, bool doCheck)
	{
		// TODO SAM 2014-02-01 enable doCheck
		int nRows = getNumberOfRecords();
		if (row < nRows)
		{
			// Inserting in the existing table
			_table_records.Insert(row, record);
		}
		else
		{
			// Appending - add blank rows up until the last one
			for (int i = nRows; i < row; i++)
			{
				addRecord(emptyRecord());
			}
			// Now add the final record
			addRecord(record);
		}
	}

	/// <summary>
	/// Determine whether the column data type is an array. </summary>
	/// <returns> true if the column data type is an array (data type is DATA_TYPE_ARRAY_BASE plus primitive type). </returns>
	public virtual bool isColumnArray(int columnType)
	{
		if (((columnType / 100) * 100) == TableField.DATA_TYPE_ARRAY_BASE)
		{
			// Data type is 10nn so it is an array.
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Indicate whether a table's column is empty (all null or blank strings).
	/// This is useful when setting column widths narrow for unused data, or deleting unused columns. </summary>
	/// <param name="columnNum"> column number 0+ to check </param>
	/// <returns> true if the column is empty, false if contains at least one record with a value. </returns>
	public virtual bool isColumnEmpty(int columnNum)
	{
		TableRecord rec = null;
		int recCount = getNumberOfRecords();
		int emptyCount = 0;
		string s;
		object o = null;
		int columnType = getFieldDataType(columnNum);
		for (int i = 0; i < recCount; i++)
		{
			rec = _table_records[i];
			try
			{
				o = rec.getFieldValue(columnNum);
			}
			catch (Exception)
			{
				// Count as empty
				++emptyCount;
				continue;
			}
			if (o == null)
			{
				++emptyCount;
			}
			else if (columnType == TableField.DATA_TYPE_STRING)
			{
				s = (string)o;
				if (s.Trim().Length == 0)
				{
					++emptyCount;
				}
			}
		}
		if (emptyCount == recCount)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Returns whether any of the table records are dirty or not. </summary>
	/// <returns> whether any of the table records are dirty or not. </returns>
	public virtual bool isDirty()
	{
		TableRecord record = null;
		int recordCount = getNumberOfRecords();

		for (int i = 0; i < recordCount; i++)
		{
			record = _table_records[i];
			if (record.isDirty())
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Join one table to another by matching column column values. </summary>
	/// <param name="table"> original table </param>
	/// <param name="tableToJoin"> table being joined </param>
	/// <param name="joinColumnsMap"> map indicating which columns need to be matched in the tables, for the join
	/// (this must be populated, even if the join column name is the same in both tables) </param>
	/// <param name="reqIncludeColumns"> requested columns to include from the second table or null to include all
	/// (the join tables will be automatically included because they exist in the first table) </param>
	/// <param name="columnMap"> map to rename original columns to new name </param>
	/// <param name="columnFilters"> map for columns that will apply a filter to limit rows that are processed </param>
	/// <param name="joinMethod"> the method used to join the tables </param>
	/// <param name="handleMultipleMatchesHow"> indicate how multiple join matches should be handled (currently only
	/// NUMBER_COLUMNS and USE_LAST_MATCH [default] are supported) </param>
	/// <param name="problems"> list of problems that will be filled during processing </param>
	/// <returns> the number of rows appended </returns>
	public virtual int joinTable(DataTable table, DataTable tableToJoin, Dictionary<string, string> joinColumnsMap, string[] reqIncludeColumns, Dictionary<string, string> columnMap, Dictionary<string, string> columnFilters, DataTableJoinMethodType joinMethod, HandleMultipleJoinMatchesHowType handleMultipleMatchesHow, IList<string> problems)
	{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		string routine = this.GetType().FullName + ".joinTable", message;

		// List of columns that will be copied to the first table
		string[] columnNamesToCopy = null;
		if ((reqIncludeColumns != null) && (reqIncludeColumns.Length > 0))
		{
			// Append only the requested names
			columnNamesToCopy = reqIncludeColumns;
			for (int icol = 0; icol < reqIncludeColumns.Length; icol++)
			{
				Message.printStatus(2,routine,"Will copy table2 column \"" + reqIncludeColumns[icol] + "\"");
			}
		}
		else
		{
			// Append all
			Message.printStatus(2,routine,"Copy all columns in table2 to table1.");
			columnNamesToCopy = tableToJoin.getFieldNames();
		}
		// Make sure that the columns to copy do not include the join columns, which should already by in the tables.
		// Just set to blank so they can be ignored in following logic
		for (int icol = 0; icol < columnNamesToCopy.Length; icol++)
		{
			IEnumerator<string> keys = joinColumnsMap.Keys.GetEnumerator();
			while (keys.MoveNext())
			{
				string key = keys.Current;
				if (columnNamesToCopy[icol].Equals(key, StringComparison.OrdinalIgnoreCase))
				{
					Message.printStatus(2,routine,"Table 2 column to copy \"" + columnNamesToCopy[icol] + "\" is same as join column.  Will not copy from table2.");
					columnNamesToCopy[icol] = "";
				}
			}
		}
		// Column numbers in the copy table to match the original table.  Any values set to -1 will result in null in output.
		// numberDuplicates will add columns on the fly as they are needed, at end of table
		string[] table1CopyColumnNames = new string[columnNamesToCopy.Length];
		int[] table1CopyColumnNumbers = new int[columnNamesToCopy.Length];
		int[] table1CopyColumnTypes = new int[columnNamesToCopy.Length];
		string[] table2CopyColumnNames = new string[columnNamesToCopy.Length];
		int[] table2CopyColumnNumbers = new int[columnNamesToCopy.Length];
		int[] table2CopyColumnTypes = new int[columnNamesToCopy.Length];
		IList<int> matchCountList = new List<int>();
		if (handleMultipleMatchesHow == HandleMultipleJoinMatchesHowType.NUMBER_COLUMNS)
		{
			// Create a list to count how many matches have occurred so that duplicates can add new numbered columns
			// Will need to be careful if "InsertBeforeColumn" functionality is added
			matchCountList = new List<int>(table.getNumberOfRecords());
		}
		// Replace the copy table names using the column map
		object o;
		for (int icol = 0; icol < columnNamesToCopy.Length; icol++)
		{
			table1CopyColumnNames[icol] = columnNamesToCopy[icol]; // Default to same as requested
			table2CopyColumnNames[icol] = columnNamesToCopy[icol]; // Default
			if (table2CopyColumnNames[icol].Equals(""))
			{
				// Column was removed from copy above (typically because it is the join column)
				table2CopyColumnNumbers[icol] = -1;
			}
			else
			{
				try
				{
					table2CopyColumnNumbers[icol] = tableToJoin.getFieldIndex(table2CopyColumnNames[icol]);
					table2CopyColumnTypes[icol] = tableToJoin.getFieldDataType(table2CopyColumnNumbers[icol]);
				}
				catch (Exception)
				{
					message = "Cannot determine table2 copy column number for \"" + table2CopyColumnNames[icol] + "\".";
					problems.Add(message);
					Message.printWarning(3,routine,message);
				}
			}
			if (columnMap != null)
			{
				// Initialize the table2 column to join from the requested columns, with matching name in both tables.
				// Rename in output (table1)
				o = columnMap[table2CopyColumnNames[icol]];
				if (o != null)
				{
					// Reset the copy column name with the new name, which will match a column name in the first table
					// (or will be created in the new table if necessary)
					// This column may not yet exist in the joined table so get column number and type below after column is added
					table1CopyColumnNames[icol] = (string)o;
				}
			}
			Message.printStatus(2,routine,"Will copy table2 column \"" + table2CopyColumnNames[icol] + "\" to table1 column \"" + table1CopyColumnNames[icol] + "\"");
		}

		// Create columns in the output table for the "include columns" (including new column names from the column map)
		// Use column types that match the copy table's column types
		// Figure out the column numbers in both tables for the include
		for (int icol = 0; icol < table1CopyColumnNames.Length; icol++)
		{
			table1CopyColumnNumbers[icol] = -1;
			if (table1CopyColumnNames[icol].Length == 0)
			{
				// Name was removed above because it duplicates the join column, so don't add
				continue;
			}
			try
			{
				table1CopyColumnNumbers[icol] = table.getFieldIndex(table1CopyColumnNames[icol]);
			}
			catch (Exception)
			{
				 // OK - handle non-existent column below.
			}
			if (table1CopyColumnNumbers[icol] >= 0)
			{
				// Already exists so skip because don't want table2 values to overwrite table1 values
				message = "Include column \"" + table1CopyColumnNames[icol] +
					"\" already exists in original table.  Not adding new column.";
				Message.printStatus(2,routine,message);
				// TODO SAM 2014-04-15 Actually, do want join to overwrite - allows subset of table to be processed
				//table1CopyColumnNumbers[icol] = -1;
				table1CopyColumnTypes[icol] = table.getFieldDataType(table1CopyColumnNumbers[icol]);
			}
			else
			{
				// Does not exist in first table so create column with the same properties as the original
				// Use the original column name to find the property
				try
				{
					Message.printStatus(2,routine,"Creating table1 column \"" + table1CopyColumnNames[icol] + "\" type=" + TableColumnType.valueOf(tableToJoin.getFieldDataType(table2CopyColumnNumbers[icol])) + " width=" + tableToJoin.getFieldWidth(table2CopyColumnNumbers[icol]) + " precision=" + tableToJoin.getFieldPrecision(table2CopyColumnNumbers[icol]));
					table1CopyColumnNumbers[icol] = table.addField(new TableField(tableToJoin.getFieldDataType(table2CopyColumnNumbers[icol]), table1CopyColumnNames[icol],tableToJoin.getFieldWidth(table2CopyColumnNumbers[icol]), tableToJoin.getFieldPrecision(table2CopyColumnNumbers[icol])), null);
					table1CopyColumnTypes[icol] = table.getFieldDataType(table1CopyColumnNumbers[icol]);
				}
				catch (Exception e)
				{
					message = "Error adding new column \"" + table1CopyColumnNames[icol] + "\" to joined table (" + e + ").";
					problems.Add(message);
					Message.printWarning(3,routine,message);
				}
			}
		}

		// Determine the column numbers in the first and second tables for the join columns
		// Do this AFTER the above checks on output columns because columns may be inserted and change the column order
		/* TODO SAM 2015-02-03 Does not seem to be needed
		if ( reqIncludeColumns == null ) {
		    reqIncludeColumns = new String[0];
		}
		*/
		string[] table1JoinColumnNames = new string[joinColumnsMap.Count];
		int[] table1JoinColumnNumbers = new int[joinColumnsMap.Count];
		int[] table1JoinColumnTypes = new int[joinColumnsMap.Count];
		string[] table2JoinColumnNames = new string[joinColumnsMap.Count];
		int[] table2JoinColumnNumbers = new int[joinColumnsMap.Count];
		int[] table2JoinColumnTypes = new int[joinColumnsMap.Count];
		IEnumerator<string> keys = joinColumnsMap.Keys.GetEnumerator();
		string key;
		int ikey = -1;
		while (keys.MoveNext())
		{
			++ikey;
			table1JoinColumnNames[ikey] = "";
			table1JoinColumnNumbers[ikey] = -1;
			table2JoinColumnNames[ikey] = "";
			table2JoinColumnNumbers[ikey] = -1;
			key = keys.Current;
			Message.printStatus(2, routine, "Determining join columns for table1 join column \"" + key + "\"");
			try
			{
				table1JoinColumnNames[ikey] = key;
				table1JoinColumnNumbers[ikey] = table.getFieldIndex(table1JoinColumnNames[ikey]);
				table1JoinColumnTypes[ikey] = table.getFieldDataType(table1JoinColumnNumbers[ikey]);
				Message.printStatus(2,routine,"Table1 join column \"" + table1JoinColumnNames[ikey] + "\" has table1 column number=" + table1JoinColumnNumbers[ikey]);
				try
				{
					// Look up the column to use in table2 by using a key from table1
					table2JoinColumnNames[ikey] = joinColumnsMap[table1JoinColumnNames[ikey]];
					table2JoinColumnNumbers[ikey] = tableToJoin.getFieldIndex(table2JoinColumnNames[ikey]);
					table2JoinColumnTypes[ikey] = tableToJoin.getFieldDataType(table2JoinColumnNumbers[ikey]);
					Message.printStatus(2,routine,"Table2 join column \"" + table2JoinColumnNames[ikey] + "\" has table2 column number=" + table2JoinColumnNumbers[ikey]);
				}
				catch (Exception)
				{
					message = "Table2 join column \"" + table2JoinColumnNames[ikey] + "\" not found in table2 \"" + tableToJoin.getTableID() + "\".";
					problems.Add(message);
					Message.printWarning(3,routine,message);
				}
			}
			catch (Exception)
			{
				message = "Join column \"" + table1JoinColumnNames[ikey] + "\" not found in table1 \"" + table.getTableID() + "\".";
				problems.Add(message);
				Message.printWarning(3,routine,message);
			}
		}

		// Get filter columns and glob-style regular expressions
		int[] columnNumbersToFilter = new int[columnFilters.Count];
		string[] columnFilterGlobs = new string[columnFilters.Count];
		keys = columnFilters.Keys.GetEnumerator();
		ikey = -1;
		key = null;
		while (keys.MoveNext())
		{
			++ikey;
			columnNumbersToFilter[ikey] = -1;
			try
			{
				key = (string)keys.Current;
				columnNumbersToFilter[ikey] = tableToJoin.getFieldIndex(key);
				columnFilterGlobs[ikey] = columnFilters[key];
				// Turn default globbing notation into internal Java regex notation
				columnFilterGlobs[ikey] = columnFilterGlobs[ikey].Replace("*", ".*").ToUpper();
			}
			catch (Exception)
			{
				message = "Filter column \"" + key + "\" not found in table \"" + tableToJoin.getTableID() + "\".";
				problems.Add(message);
				Message.printWarning(3,routine,message);
			}
		}
		// Loop through all of the records in the table being joined and check the filters.
		// Do this up front because the records are checked multiple times during the join
		bool[] joinTableRecordMatchesFilter = new bool[tableToJoin.getNumberOfRecords()];
		int icol;
		int nrowsJoined = 0;
		string s;
		for (int irow = 0; irow < tableToJoin.getNumberOfRecords(); irow++)
		{
			joinTableRecordMatchesFilter[irow] = true;
			if (columnNumbersToFilter.Length > 0)
			{
				// Filters can be done on any columns so loop through to see if row matches before doing copy
				for (icol = 0; icol < columnNumbersToFilter.Length; icol++)
				{
					if (columnNumbersToFilter[icol] < 0)
					{
						joinTableRecordMatchesFilter[irow] = false;
						break;
					}
					try
					{
						o = tableToJoin.getFieldValue(irow, columnNumbersToFilter[icol]);
						if (o == null)
						{
							joinTableRecordMatchesFilter[irow] = false;
							break; // Don't include nulls when checking values
						}
						// Do filter on strings only using uppercase
						s = ("" + o).ToUpper();
						if (!s.matches(columnFilterGlobs[icol]))
						{
							// A filter did not match so don't copy the record
							joinTableRecordMatchesFilter[irow] = false;
							break;
						}
					}
					catch (Exception e)
					{
						message = "Error getting copy table data for [" + irow + "][" + columnNumbersToFilter[icol] + "] (" + e + ").";
						problems.Add(message);
						Message.printWarning(3, routine, message);
					}
				}
			}
		}
		// Loop through all the data records in the original table (the original records, NOT any that have been appended due
		// to the join), loop through records in the join table, and join records to the table original as appropriate (this may
		// result in a modification of the same records, or appending new records at the bottom of the table).
		// Keep track of which rows do not match and add at the end.  Otherwise, duplicate rows are added.
		bool[] joinTableRecordMatchesTable1 = new bool[tableToJoin.getNumberOfRecords()];
		int tableNumRows = table.getNumberOfRecords();
		bool joinColumnsMatch = false; // Indicates whether two tables' join column values match
		object table1Value, table2Value;
		string stringTable1Value, stringTable2Value;
		TableRecord recToModify = null;
		// Loop through all rows in the first table
		for (int irow = 0; irow < tableNumRows; irow++)
		{
			if (handleMultipleMatchesHow == HandleMultipleJoinMatchesHowType.NUMBER_COLUMNS)
			{
				// Initialize the number of matches for this row
				matchCountList.Add(new int?(0));
			}
			// Loop through all rows in the second table
			for (int irowJoin = 0; irowJoin < tableToJoin.getNumberOfRecords(); irowJoin++)
			{
				if (!joinTableRecordMatchesFilter[irowJoin])
				{
					// Join row did not match filter so no need to process it
					continue;
				}
				else
				{
					// Join table record matched filter so evaluate if the join column values match in the two tables.
					// If there is a match, the join will be done in-line with an existing record.
					// If not, the join will only occur if the join method is JOIN_ALWAYS and in this case the join column values
					// and all append values will be added to the main table in a new row.
					joinColumnsMatch = true; // Set to false in checks below
					table1Value = null;
					table2Value = null;
					for (icol = 0; icol < table1JoinColumnNumbers.Length; icol++)
					{
						if ((table1JoinColumnNumbers[icol] < 0) || (table2JoinColumnNumbers[icol] < 0))
						{
							// Something did not check out above so ignore to avoid more errors.
							continue;
						}
						try
						{
							table1Value = table.getFieldValue(irow, table1JoinColumnNumbers[icol]);
						}
						catch (Exception e)
						{
							message = "Error getting table1 value to check join (" + e + ").";
							problems.Add(message);
							Message.printWarning(3, routine, message);
						}
						try
						{
							table2Value = tableToJoin.getFieldValue(irowJoin, table2JoinColumnNumbers[icol]);
						}
						catch (Exception e)
						{
							message = "Error getting table2 value to check join (" + e + ").";
							problems.Add(message);
							Message.printWarning(3, routine, message);
						}
						// For now if either is null do not add the record
						if ((table1Value == null) || (table2Value == null))
						{
							joinColumnsMatch = false;
							break;
						}
						else if (table1JoinColumnTypes[icol] == TableField.DATA_TYPE_STRING)
						{
							stringTable1Value = (string)table1Value;
							stringTable2Value = (string)table2Value;
							if (!stringTable1Value.Equals(stringTable2Value, StringComparison.OrdinalIgnoreCase))
							{
								joinColumnsMatch = false;
								break;
							}
						}
						// All other data types use equals
						else if (!table1Value.Equals(table2Value))
						{
							joinColumnsMatch = false;
							break;
						}
					}
					//Message.printStatus(2,routine,"Join value1=\"" + table1Value + "\" value2=\""+ table2Value + "\" match=" + joinColumnsMatch);
					if (joinColumnsMatch)
					{
						//Message.printStatus(2,routine,"Setting in existing row.");
						joinTableRecordMatchesTable1[irowJoin] = true;
						try
						{
							recToModify = table.getRecord(irow); // Modify existing row in table
						}
						catch (Exception e)
						{
							message = "Error getting existing joined record to modify (" + e + ").";
							problems.Add(message);
							Message.printWarning(3, routine, message);
						}
						// Loop through the columns to copy and set the values from
						// the second table into the first table (which previously had columns added)
						for (icol = 0; icol < table2CopyColumnNumbers.Length; icol++)
						{
							try
							{
								if (table1CopyColumnNumbers[icol] < 0)
								{
									// There was an issue with the column to add so skip
									//Message.printStatus(2,routine,"Don't have column number for table1 column \"" +
									//     table1CopyColumnNames[icol] + "\"");
									continue;
								}
								else if (table2CopyColumnNumbers[icol] < 0)
								{
									// There was an issue with the column to add so skip
									//Message.printStatus(2,routine,"Don't have column number for table2 column \"" +
									//     table2CopyColumnNames[icol] + "\"");
									continue;
								}
								else
								{
									// Set the value in the original table, if the type matches
									// TODO SAM 2013-08-19 Check that the column types match
									if (table1CopyColumnTypes[icol] == table2CopyColumnTypes[icol])
									{
										if (handleMultipleMatchesHow == HandleMultipleJoinMatchesHowType.NUMBER_COLUMNS)
										{
											// Increment the match counter
											if (icol == 0)
											{
												matchCountList[irow] = new int?(matchCountList[irow] + 1);
												Message.printStatus(2, routine, "Incremented match counter [" + irow + "] to " + matchCountList[irow] + " for column \"" + table2CopyColumnNames[icol] + "\"");
											}
											if (matchCountList[irow] == 1)
											{
												// This is the first match so do simple set on requested output columns
												// Set the column values in the joined table
												recToModify.setFieldValue(table1CopyColumnNumbers[icol], tableToJoin.getFieldValue(irowJoin, table2CopyColumnNumbers[icol]));
											}
											else
											{
												// Else, need to add output columns that have number appended
												// For now look up the column
												// TODO SAM 2015-03-04 add column to the existing column number array to increase performance
												int icol1 = -1;
												string duplicateColumn = table1CopyColumnNames[icol] + "_" + matchCountList[irow];
												Message.printStatus(2,routine,"Duplicate match.  Will output to column \"" + duplicateColumn + "\"");
												try
												{
													Message.printStatus(2,routine,"See if column \"" + duplicateColumn + "\" exists.");
													icol1 = table.getFieldIndex(duplicateColumn);
													Message.printStatus(2,routine,"It does, will write to table 1 column [" + icol1 + "].");
												}
												catch (Exception)
												{
													// Add the column if it has not been added by a previous duplicate.
													// First get the column used for the first match, which will not have a trailing number
													Message.printStatus(2,routine,"It does not, need to add new column to table1.");
													Message.printStatus(2,routine,"Getting table2 column to copy properties [" + table2CopyColumnNumbers[icol] + "]");
													TableField tf = tableToJoin.getTableField(table2CopyColumnNumbers[icol]);
													// Keep everything the same except change the column name
													Message.printStatus(2,routine,"Adding table1 column \"" + duplicateColumn + "\" with properties from \"" + tf.getName() + "\"");
													icol1 = table.addField(new TableField(tf.getDataType(), duplicateColumn, tf.getWidth(), tf.getPrecision()), null);
													Message.printStatus(2,routine,"Added table1 column \"" + duplicateColumn + "\" [" + icol1 + "]");
												}
												// Set in new column number, using table2 column number to copy
												Message.printStatus(2,routine,"Setting table1 col \"" + duplicateColumn + "\" [" + icol1 + "] from table1 [" + irowJoin + "][" + icol + "] value " + tableToJoin.getFieldValue(irowJoin, table2CopyColumnNumbers[icol]));
												recToModify.setFieldValue(icol1,tableToJoin.getFieldValue(irowJoin, table2CopyColumnNumbers[icol]));
											}
										}
										else
										{
											// Set the column values in the joined table
											recToModify.setFieldValue(table1CopyColumnNumbers[icol], tableToJoin.getFieldValue(irowJoin, table2CopyColumnNumbers[icol]));
										}
										++nrowsJoined;
									}
									else
									{
										Message.printStatus(2,routine,"Column types are different, cannot set value from table2 to table1.");
									}
								}
							}
							catch (Exception e)
							{
								// Should not happen
								message = "Error setting [" + irow + "][" + table1CopyColumnNumbers[icol] + "] (" + e + ").";
								problems.Add(message);
								Message.printWarning(3, routine, message);
							}
						}
					}
				}
			}
		}
		// Now add any rows that were not matched - add at the end so as to not upset the original sequence and lists used above
		// TODO SAM 2015-03-05 Need to enable for NUMBER_COLUMNS
		if (joinMethod == DataTableJoinMethodType.JOIN_ALWAYS)
		{
			if (handleMultipleMatchesHow == HandleMultipleJoinMatchesHowType.NUMBER_COLUMNS)
			{
				problems.Add("Requested NumberColumns for multiple join matches but not suported " + "with JoinMethod=JoinAlways - multiple matches will be in extra rows.");
			}
			for (int irowJoin = 0; irowJoin < tableToJoin.getNumberOfRecords(); irowJoin++)
			{
				if (joinTableRecordMatchesTable1[irowJoin])
				{
					// Row was matched above so no need to add again.
					continue;
				}
				// Add a row to the table, containing only the join column values from the second table
				// and nulls for all the other values
				try
				{
					recToModify = table.addRecord(table.emptyRecord());
				}
				catch (Exception e)
				{
					message = "Error adding new record to modify (" + e + ").";
					problems.Add(message);
					Message.printWarning(3, routine, message);
				}
				// A new record was added.  Also include the join column values using the table1 names
				// TODO SAM 2013-08-19 Evaluate whether table2 names should be used (or option to use)
				for (icol = 0; icol < table2JoinColumnNumbers.Length; icol++)
				{
					try
					{
						if (table2JoinColumnNumbers[icol] < 0)
						{
							// There was an issue with the column to add so skip
							continue;
						}
						else
						{
							// Set the value in the original table, if the type matches
							// TODO SAM 2013-08-19 Check that the column types match
							if (table1JoinColumnTypes[icol] == table2JoinColumnTypes[icol])
							{
								recToModify.setFieldValue(table1JoinColumnNumbers[icol], tableToJoin.getFieldValue(irowJoin, table2JoinColumnNumbers[icol]));
								++nrowsJoined;
							}
						}
					}
					catch (Exception e)
					{
						// Should not happen
						message = "Error setting row value for column [" + table1JoinColumnNumbers[icol] + "] (" + e + ").";
						problems.Add(message);
						Message.printWarning(3, routine, message);
						Message.printWarning(3, routine, e);
					}
				}
				// Loop through the columns to include and set the values from
				// the second table into the first table (which previously had columns added)
				for (icol = 0; icol < table2CopyColumnNumbers.Length; icol++)
				{
					try
					{
						if (table1CopyColumnNumbers[icol] < 0)
						{
							// There was an issue with the column to add so skip
							Message.printStatus(2,routine,"Don't have column number for table1 column \"" + table1CopyColumnNames[icol]);
							continue;
						}
						else if (table2CopyColumnNumbers[icol] < 0)
						{
							// There was an issue with the column to add so skip
							Message.printStatus(2,routine,"Don't have column number for table2 column \"" + table2CopyColumnNames[icol] + "\"");
							continue;
						}
						else
						{
							// Set the value in the original table, if the type matches
							// TODO SAM 2013-08-19 Check that the column types match
							if (table1CopyColumnTypes[icol] == table2CopyColumnTypes[icol])
							{
								recToModify.setFieldValue(table1CopyColumnNumbers[icol], tableToJoin.getFieldValue(irowJoin, table2CopyColumnNumbers[icol]));
								++nrowsJoined;
							}
							else
							{
								Message.printStatus(2,routine,"Column types are different, cannot set value from table2 to table1.");
							}
						}
					}
					catch (Exception e)
					{
						// Should not happen
						message = "Error adding new row, column [" + table1CopyColumnNumbers[icol] + "] (" + e + ").";
						problems.Add(message);
						Message.printWarning(3, routine, message);
					}
				}
			}
		}
		if (problems.Count > 0)
		{
			throw new Exception("There were " + problems.Count + " errors joining table \"" + tableToJoin.getTableID() + "\" to \"" + table.getTableID() + "\"");
		}
		return nrowsJoined;
	}

	/// <summary>
	/// Given a definition of what data to expect, read a simple delimited file and
	/// store the data in a table.  Comment lines start with # and are not considered part of the header. </summary>
	/// <returns> new DataTable containing data. </returns>
	/// <param name="filename"> name of file containing delimited data. </param>
	/// <param name="delimiter"> string representing delimiter in data file (typically a comma). </param>
	/// <param name="tableFields"> list of TableField objects defining data expectations. </param>
	/// <param name="num_lines_header"> number of lines in header (typically 1).  The header
	/// lines are read and ignored. </param>
	/// <exception cref="Exception"> if there is an error parsing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static DataTable parseDelimitedFile(String filename, String delimiter, java.util.List<TableField> tableFields, int num_lines_header) throws Exception
	public static DataTable parseDelimitedFile(string filename, string delimiter, IList<TableField> tableFields, int num_lines_header)
	{
		return parseDelimitedFile(filename, delimiter, tableFields, num_lines_header, false);
	}

	/// <summary>
	/// Given a definition of what data to expect, read a simple delimited file and
	/// store the data in a table.  Comment lines start with # and are not considered part of the header. </summary>
	/// <returns> new DataTable containing data. </returns>
	/// <param name="filename"> name of file containing delimited data. </param>
	/// <param name="delimiter"> string representing delimiter in data file (typically a comma). </param>
	/// <param name="tableFields"> list of TableField objects defining data expectations. </param>
	/// <param name="num_lines_header"> number of lines in header (typically 1).  The header
	/// lines are read and ignored. </param>
	/// <param name="trim_spaces"> if true, then when a column value is read between delimiters,
	/// it will be .trim()'d before being parsed into a number or String. </param>
	/// <exception cref="Exception"> if there is an error parsing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static DataTable parseDelimitedFile(String filename, String delimiter, java.util.List<TableField> tableFields, int num_lines_header, boolean trim_spaces) throws Exception
	public static DataTable parseDelimitedFile(string filename, string delimiter, IList<TableField> tableFields, int num_lines_header, bool trim_spaces)
	{
		return parseDelimitedFile(filename, delimiter, tableFields, num_lines_header, trim_spaces, -1);
	}

	/// <summary>
	/// Given a definition of what data to expect, read a simple delimited file and
	/// store the data in a table.  Comment lines start with # and are not considered part of the header.
	/// This method may not be maintained in the future.
	/// The parseFile() method is more flexible. </summary>
	/// <returns> new DataTable containing data. </returns>
	/// <param name="filename"> name of file containing delimited data. </param>
	/// <param name="delimiter"> string representing delimiter in data file (typically a comma). </param>
	/// <param name="tableFields"> list of TableField objects defining data expectations. </param>
	/// <param name="num_lines_header"> number of lines in header (typically 1).  The header
	/// lines are read and ignored. </param>
	/// <param name="trim_spaces"> if true, then when a column value is read between delimiters,
	/// it will be .trim()'d before being parsed into a number or String. </param>
	/// <param name="maxLines"> the maximum number of lines to read from the file.  If less than
	/// or equal to 0, all lines will be read. </param>
	/// <exception cref="Exception"> if there is an error parsing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static DataTable parseDelimitedFile(String filename, String delimiter, java.util.List<TableField> tableFields, int num_lines_header, boolean trim_spaces, int maxLines) throws Exception
	public static DataTable parseDelimitedFile(string filename, string delimiter, IList<TableField> tableFields, int num_lines_header, bool trim_spaces, int maxLines)
	{
		string iline;
		bool processed_header = false;
		IList<string> columns;
		int num_fields = 0, num_lines_header_read = 0;
		int lineCount = 0;
		DataTable table;

		StreamReader @in = new StreamReader(filename);

		table = new DataTable(tableFields);
		table._haveDataInMemory = true;
		int[] field_types = table.getFieldDataTypes();
		if (num_lines_header == 0)
		{
			processed_header = true;
			num_fields = field_types.Length;
		}

		string col = null;

		// Create an array to use for determining the maximum size of all the
		// fields that are Strings.  This will be used to set the width of
		// the data values for those fields so that the width of the field is
		// equal to the width of the longest string.  This is mostly important
		// for when the table is to be placed within a DataTable_TableModel, 
		// so that the String field data are not truncated.
		int numFields = tableFields.Count;
		int[] stringLengths = new int[numFields];
		for (int i = 0; i < numFields; i++)
		{
			stringLengths[i] = 0;
		}
		int length = 0;

		while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
		{
			// check if read comment or empty line
			if (iline.StartsWith("#", StringComparison.Ordinal) || iline.Trim().Length == 0)
			{
				continue;
			}

			// TODO SAM if a column contains only quoted strings, but each string is a number, then there is no
			// way to treat the column as strings.  This may be problematic if the string is zero-padded.
			columns = StringUtil.breakStringList(iline, delimiter, StringUtil.DELIM_ALLOW_STRINGS);

			// line is part of header ... 
			if (!processed_header)
			{
				num_fields = columns.Count;
				if (num_fields < tableFields.Count)
				{
					@in.Close();
					throw new IOException("Table fields specifications do not match data found in file.");
				}

				num_lines_header_read++;
				if (num_lines_header_read == num_lines_header)
				{
					processed_header = true;
				}
			}
			else
			{
				// line contains data - store in table as record
				TableRecord contents = new TableRecord(num_fields);
				try
				{
					for (int i = 0; i < num_fields; i++)
					{
						col = columns[i];
						if (trim_spaces)
						{
							col = col.Trim();
						}
						if (field_types[i] == TableField.DATA_TYPE_STRING)
						{
							contents.addFieldValue(col);
							length = col.Length;
							if (length > stringLengths[i])
							{
								stringLengths[i] = length;
							}
						}
						else if (field_types[i] == TableField.DATA_TYPE_DOUBLE)
						{
							contents.addFieldValue(Convert.ToDouble(col));
						}
						else if (field_types[i] == TableField.DATA_TYPE_INT)
						{
							contents.addFieldValue(Convert.ToInt32(col));
						}
						else if (field_types[i] == TableField.DATA_TYPE_SHORT)
						{
							contents.addFieldValue(Convert.ToInt16(col));
						}
						else if (field_types[i] == TableField.DATA_TYPE_FLOAT)
						{
							contents.addFieldValue(Convert.ToSingle(col));
						}
						else if (field_types[i] == TableField.DATA_TYPE_LONG)
						{
							contents.addFieldValue(Convert.ToInt64(col));
						}
					}
					table.addRecord(contents);
					contents = null;
				}
				catch (Exception e)
				{
					if (IOUtil.testing())
					{
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
					}
					Message.printWarning(2, "DataTable.parseDelimitedFile", e);
				}
			}
			lineCount++;
			if (maxLines > 0 && lineCount >= maxLines)
			{
				@in.Close();

				// Set the widths of the string fields to the length
				// of the longest strings within those fields
				for (int i = 0; i < num_fields; i++)
				{
					col = columns[i];
					if (field_types[i] == TableField.DATA_TYPE_STRING)
					{
						table.setFieldWidth(i, stringLengths[i]);
					}
				}

				return table;
			}
		}
		@in.Close();
		return table;
	}

	/// <summary>
	/// Reads the header of a comma-delimited file and return list of TableField objects. </summary>
	/// <returns> list of TableField objects (only field names will be set). </returns>
	/// <param name="filename"> name of file containing delimited data. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<TableField> parseDelimitedFileHeader(String filename) throws Exception
	public static IList<TableField> parseDelimitedFileHeader(string filename)
	{
		return parseDelimitedFileHeader(filename, ",");
	}

	/// <summary>
	/// Reads the header of a delimited file and return list of TableField objects.
	/// The field names will be correctly returned.  The data type, however, will be set
	/// to TableField.DATA_TYPE_STRING.  This should be changed if not appropriate. </summary>
	/// <returns> list of TableField objects (field names will be correctly set but data type will be string). </returns>
	/// <param name="filename"> name of file containing delimited data. </param>
	/// <param name="delimiter"> string representing delimiter in data file. </param>
	/// <exception cref="Exception"> if there is an error reading the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<TableField> parseDelimitedFileHeader(String filename, String delimiter) throws Exception
	public static IList<TableField> parseDelimitedFileHeader(string filename, string delimiter)
	{
		string iline;
		IList<string> columns;
		IList<TableField> tableFields = null;
		int num_fields = 0;
		TableField newTableField = null;

		StreamReader @in = new StreamReader(filename);

		try
		{
			while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
			{

				// check if read comment or empty line
				if (iline.StartsWith("#", StringComparison.Ordinal) || iline.Trim().Length == 0)
				{
					continue;
				}

				columns = StringUtil.breakStringList(iline, delimiter, 0);
		//			StringUtil.DELIM_SKIP_BLANKS );

				num_fields = columns.Count;
				tableFields = new List<TableField> (num_fields);
				for (int i = 0; i < num_fields; i++)
				{
					newTableField = new TableField();
					newTableField.setName(columns[i].Trim());
					newTableField.setDataType(TableField.DATA_TYPE_STRING);
					tableFields.Add(newTableField);
				}
				break;
			}
		}
		finally
		{
			if (@in != null)
			{
				@in.Close();
			}
		}
		return tableFields;
	}

	// TODO SAM 2012-01-09 Need to handle doubled and tripled quotes as per:
	// http://en.wikipedia.org/wiki/Comma-separated_values
	// For now assume no embedded quotes in quoted strings
	/// <summary>
	/// Parses a file and returns the DataTable for the file.  Currently only does
	/// delimited files, and the data type for a column must be consistent.
	/// The lines in delimited files do not need to all have the same
	/// number of columns: the number of columns in the returned DataTable will be 
	/// the same as the line in the file with the most delimited columns, all others
	/// will be padded with empty value columns on the right of the table. </summary>
	/// <param name="filename"> the name of the file from which to read the table data.
	/// </para>
	/// </param>
	/// <param name="props"> a PropList with settings for how the file should be read and handled.<para>
	/// Properties and their effects:<br>
	/// <table width=100% cellpadding=10 cellspacing=0 border=2>
	/// <tr>
	/// <td><b>Property</b></td>    <td><b>Description</b></td> <td><b>Default</b></td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>ColumnDataTypes</b></td>
	/// <td>The data types for the column, either "Auto" (determine from column contents),
	/// "AllStrings" (all are strings, fastest processing and the default from historical behavior),
	/// or a list of data types (to be implemented in the future).  SEE ALSO DateTimeColumns.</td>
	/// <td>AllStrings.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>CommentLineIndicator</b></td>
	/// <td>The characters with which comment lines begin.
	/// Lines starting with this character are skipped (TrimInput is applied after checking for comments).</td>
	/// <td>No default.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>DateTimeColumns</b></td>
	/// <td>Specify comma-separated column names that should be treated as DateTime columns.
	/// The column names must agree with those determined from the table headings.</td>
	/// <td>Determine column types from data - date/times are not determined.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>Delimiter</b></td>
	/// <td>The character (s) that should be used to delimit fields in the file.  Fields are broken
	/// using the following StringUtil.breakStringList() call (the flag can be modified by MergeDelimiters):<br>
	/// <blockquote>
	///    v = StringUtil.breakStringList(line, delimiters, 0);
	/// </blockquote><br></td>
	/// <td>Comma (,).</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>FixedFormat</b></td>
	/// <td>"True" or "False".  Currently ignored.</td>
	/// <td></td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>HeaderLines (previously HeaderRows)</b></td>
	/// <td>The lines containing the header information, specified as single number or a range (e.g., 2-3).
	/// Multiple lines will be separated with a newline when displayed, or Auto to automatically treat the
	/// first non-comment row as a header if the value is double-quoted.</td>
	/// <td>Auto</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>MergeDelimiters</b></td>
	/// <td>"True" or "False".  If true, then adjoining delimiter characters are treated as one by using
	/// StringUtil.breakStringList(line,delimiters,StringUtil.DELIM_SKIP_BLANKS.</td>
	/// <td>False (do not merge blank columns).</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>SkipLines (previously SkipRows)</b></td>
	/// <td>Lines from the original file to skip (each value 0+), as list of comma-separated individual row or
	/// ranges like 3-6.  Skipped lines are generally information that cannot be parsed.  The lines are skipped after
	/// the initial read and are not available for further processing.</td>
	/// <td>Don't skip any lines.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>DoubleColumns</b></td>
	/// <td>Specify comma-separated column names that should be treated as double precision columns.
	/// The column names must agree with those determined from the table headings.</td>
	/// <td>Determine column types from data - date/times are not determined.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>IntegerColumns</b></td>
	/// <td>Specify comma-separated column names that should be treated as integer columns.
	/// The column names must agree with those determined from the table headings.</td>
	/// <td>Determine column types from data - date/times are not determined.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>TextColumns</b></td>
	/// <td>Specify comma-separated column names that should be treated as text columns.
	/// The column names must agree with those determined from the table headings.</td>
	/// <td>Determine column types from data - date/times are not determined.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>Top</b></td>
	/// <td>Specify an integer that is the top N rows to be processed.</td>
	/// <td>Determine column types from data - date/times are not determined.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>TrimInput</b></td>
	/// <td>"True" or "False".  Indicates input strings should be trimmed before parsing.</td>
	/// <td>False</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>TrimStrings</b></td>
	/// <td>"True" or "False".  Indicates whether strings should
	/// be trimmed before being placed in the data table (after parsing).</td>
	/// <td>False</td>
	/// </tr>
	/// 
	/// </table> </param>
	/// <returns> the DataTable that was created. </returns>
	/// <exception cref="Exception"> if an error occurs </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static DataTable parseFile(String filename, RTi.Util.IO.PropList props) throws Exception
	public static DataTable parseFile(string filename, PropList props)
	{
		string routine = "DataTable.parseFile";
		if (props == null)
		{
			props = new PropList(""); // To simplify code below
		}
		// TODO SAM 2005-11-16 why is FixedFormat included?  Future feature?
		/*String propVal = props.getValue("FixedFormat");
		if (propVal != null) {
			if (propVal.equalsIgnoreCase("false")) {
				fixed = false;
			}
		}
		*/

		// FIXME SAM 2008-01-27 Using other than the default of strings does not seem to work
		// The JWorksheet does not display correctly.
		bool ColumnDataTypes_Auto_boolean = false; // To improve performance below
		// TODO SAM 2008-04-15 Evaluate whether the following should be used
		//String ColumnDataTypes = "AllStrings";  // Default for historical reasons
		string propVal = props.getValue("ColumnDataTypes");
		if ((!string.ReferenceEquals(propVal, null)) && (propVal.Equals("Auto", StringComparison.OrdinalIgnoreCase)))
		{
			//ColumnDataTypes = "Auto";
			ColumnDataTypes_Auto_boolean = true;
		}

		string Delimiter = "";
		propVal = props.getValue("Delimiter");
		if (!string.ReferenceEquals(propVal, null))
		{
			Delimiter = propVal;
		}
		else
		{
			Delimiter = ",";
		}

		propVal = props.getValue("HeaderLines");
		if (string.ReferenceEquals(propVal, null))
		{
			// Use older form...
			propVal = props.getValue("HeaderRows");
			if (!string.ReferenceEquals(propVal, null))
			{
				Message.printWarning(3, routine, "Need to convert HeaderRows parameter to HeaderLines in software.");
			}
		}
		IList<int> HeaderLineList = new List<int>();
		int HeaderLinesList_maxval = -1; // Used to optimize code below
		bool HeaderLines_Auto_boolean = false; // Are header rows to be determined automatically?
		if ((string.ReferenceEquals(propVal, null)) || (propVal.Length == 0))
		{
			// Default...
			HeaderLines_Auto_boolean = true;
		}
		else
		{
			// Interpret the property.
			Message.printStatus(2, routine, "HeaderLines=\"" + propVal + "\"");
			if (propVal.Equals("Auto", StringComparison.OrdinalIgnoreCase))
			{
				HeaderLines_Auto_boolean = true;
			}
			else
			{
				// Determine the list of rows to skip.
				IList<string> v = StringUtil.breakStringList(propVal, ", ", StringUtil.DELIM_SKIP_BLANKS);
				int vsize = 0;
				if (v != null)
				{
					vsize = v.Count;
				}
				// FIXME SAM 2008-01-27 Figure out how to deal with multi-row headings.  For now only handle first
				if (vsize > 1)
				{
					Message.printWarning(3, routine, "Only know how to handle single-row headings.  Ignoring other heading rows.");
					vsize = 1;
				}
				for (int i = 0; i < vsize; i++)
				{
					string vi = v[i];
					if (StringUtil.isInteger(vi))
					{
						int row = int.Parse(vi);
						Message.printStatus(2, routine, "Header row is [" + row + "]");
						HeaderLineList.Add(new int?(row));
						HeaderLinesList_maxval = Math.Max(HeaderLinesList_maxval, row);
					}
					else
					{
						int pos = vi.IndexOf("-", StringComparison.Ordinal);
						if (pos >= 0)
						{
							// Specifying a range of values...
							int first_to_skip = -1;
							int last_to_skip = -1;
							if (pos == 0)
							{
								// First index is 0...
								first_to_skip = 0;
							}
							else
							{
								// Get first to skip...
								first_to_skip = int.Parse(vi.Substring(0,pos).Trim());
							}
							last_to_skip = int.Parse(vi.Substring(pos + 1).Trim());
							for (int @is = first_to_skip; @is <= last_to_skip; @is++)
							{
								HeaderLineList.Add(new int?(@is));
								HeaderLinesList_maxval = Math.Max(HeaderLinesList_maxval, @is);
							}
						}
					}
				}
			}
		}
		// Use to speed up code below.
		int HeaderLinesList_size = HeaderLineList.Count;

		string[] dateTimeColumns = null;
		propVal = props.getValue("DateTimeColumns");
		if ((!string.ReferenceEquals(propVal, null)) && propVal.Length > 0)
		{
			dateTimeColumns = propVal.Split(",", true);
			for (int i = 0; i < dateTimeColumns.Length; i++)
			{
				dateTimeColumns[i] = dateTimeColumns[i].Trim();
			}
		}

		string[] doubleColumns = null;
		propVal = props.getValue("DoubleColumns");
		if ((!string.ReferenceEquals(propVal, null)) && propVal.Length > 0)
		{
			doubleColumns = propVal.Split(",", true);
			for (int i = 0; i < doubleColumns.Length; i++)
			{
				doubleColumns[i] = doubleColumns[i].Trim();
			}
		}

		string[] integerColumns = null;
		propVal = props.getValue("IntegerColumns");
		if ((!string.ReferenceEquals(propVal, null)) && propVal.Length > 0)
		{
			integerColumns = propVal.Split(",", true);
			for (int i = 0; i < integerColumns.Length; i++)
			{
				integerColumns[i] = integerColumns[i].Trim();
			}
		}

		string[] textColumns = null;
		propVal = props.getValue("TextColumns");
		if ((!string.ReferenceEquals(propVal, null)) && propVal.Length > 0)
		{
			textColumns = propVal.Split(",", true);
			for (int i = 0; i < textColumns.Length; i++)
			{
				textColumns[i] = textColumns[i].Trim();
			}
		}

		int top = -1;
		int topm1 = -1; // Used for 0-index comparison
		propVal = props.getValue("Top");
		if ((!string.ReferenceEquals(propVal, null)) && propVal.Length > 0)
		{
			try
			{
				top = int.Parse(propVal);
				topm1 = top - 1;
			}
			catch (System.FormatException)
			{
				// Just process all
			}
		}

		int parseFlagHeader = StringUtil.DELIM_ALLOW_STRINGS;
		// Retain the quotes in data records makes sure that quoted numbers come across as intended as literal strings. 
		// This is important when numbers are zero padded, such as for station identifiers.
		// The problem is that it will result in embedded escaped quotes "" in the output
		int parseFlag = StringUtil.DELIM_ALLOW_STRINGS | StringUtil.DELIM_ALLOW_STRINGS_RETAIN_QUOTES;
		propVal = props.getValue("MergeDelimiters");
		if (!string.ReferenceEquals(propVal, null))
		{
			parseFlag |= StringUtil.DELIM_SKIP_BLANKS;
			parseFlagHeader |= StringUtil.DELIM_SKIP_BLANKS;
		}

		string CommentLineIndicator = null;
		propVal = props.getValue("CommentLineIndicator");
		if (!string.ReferenceEquals(propVal, null))
		{
			CommentLineIndicator = propVal;
		}

		propVal = props.getValue("SkipLines");
		if (string.ReferenceEquals(propVal, null))
		{
			// Try the older form...
			propVal = props.getValue("SkipRows");
			if (!string.ReferenceEquals(propVal, null))
			{
				Message.printWarning(3, routine, "Need to convert SkipRows parameter to SkipLines in software.");
			}
		}
		IList<int> skipLinesList = new List<int>();
		int skipLinesList_maxval = - 1;
		if ((!string.ReferenceEquals(propVal, null)) && (propVal.Length > 0))
		{
			// Determine the list of rows to skip.
			IList<string> v = StringUtil.breakStringList(propVal, ", ", StringUtil.DELIM_SKIP_BLANKS);
			int vsize = 0;
			if (v != null)
			{
				vsize = v.Count;
			}
			for (int i = 0; i < vsize; i++)
			{
				string vi = v[i];
				if (StringUtil.isInteger(vi))
				{
					int row = int.Parse(vi);
					skipLinesList.Add(new int?(row));
					skipLinesList_maxval = Math.Max(skipLinesList_maxval, row);
				}
				else
				{
					int pos = vi.IndexOf("-", StringComparison.Ordinal);
					if (pos >= 0)
					{
						// Specifying a range of values...
						int first_to_skip = -1;
						int last_to_skip = -1;
						if (pos == 0)
						{
							// First index is 0...
							first_to_skip = 0;
						}
						else
						{
							// Get first to skip...
							first_to_skip = int.Parse(vi.Substring(0,pos).Trim());
						}
						last_to_skip = int.Parse(vi.Substring(pos + 1).Trim());
						for (int @is = first_to_skip; @is <= last_to_skip; @is++)
						{
							skipLinesList.Add(new int?(@is));
							skipLinesList_maxval = Math.Max(skipLinesList_maxval, @is);
						}
					}
				}
			}
		}
		// Use to speed up code below.
		int skipLinesList_size = skipLinesList.Count;

		propVal = props.getValue("TrimInput");
		bool TrimInput_Boolean = false; // Default
		if ((!string.ReferenceEquals(propVal, null)) && propVal.Equals("true", StringComparison.OrdinalIgnoreCase))
		{
			TrimInput_Boolean = true;
		}

		bool TrimStrings_boolean = false;
		propVal = props.getValue("TrimStrings");
		if ((!string.ReferenceEquals(propVal, null)) && propVal.Equals("true", StringComparison.OrdinalIgnoreCase))
		{
			TrimStrings_boolean = true;
		}

		IList<IList<string>> data_record_tokens = new List<IList<string>>();
		IList<string> v = null;
		int maxColumns = 0;
		int numColumnsParsed = 0;

		StreamReader @in = new StreamReader(filename);
		string line;

		// TODO JTS 2006-06-05
		// Found a bug in DataTable.  If you attempt to call
		// parseFile() on a file of size 0 (no lines, no characters)
		// it will throw an exception.  This should be checked out in the future.

		// Read until the end of the file...

		int linecount = 0; // linecount = 1 for first line in file, for user perspective.
		int dataLineCount = 0;
		int linecount0; // linecount0 = linecount - 1 (zero index), for code perspective.
		bool headers_found = false; // Indicates whether the headers have been found
		IList<TableField> tableFields = null; // Table fields as controlled by header or examination of data records
		int numFields = -1; // Number of table fields.
		TableField tableField = null; // Table field added below
		while (true)
		{
			line = @in.ReadLine();
			if (string.ReferenceEquals(line, null))
			{
				// End of file...
				break;
			}
			++linecount;
			linecount0 = linecount - 1;

			if (Message.isDebugOn)
			{
				Message.printDebug(10, routine, "Line [" + linecount0 + "]: " + line);
			}

			// Skip any comments anywhere in the file.
			if ((!string.ReferenceEquals(CommentLineIndicator, null)) && line.StartsWith(CommentLineIndicator, StringComparison.Ordinal))
			{
				continue;
			}

			// Also skip the requested lines to skip linecount is 1+ while lines to skip are 0+

			if (linecount0 <= skipLinesList_maxval)
			{
				// Need to check it...
				if (parseFile_LineMatchesLineFromList(linecount0,skipLinesList, skipLinesList_size))
				{
					// Skip the line as requested
					continue;
				}
			}

			// "line" now contains the latest non-comment line so evaluate whether
			// the line contains the column names.

			if (!headers_found && (HeaderLines_Auto_boolean || ((HeaderLineList != null) && linecount0 <= HeaderLinesList_maxval)))
			{
				if (HeaderLines_Auto_boolean)
				{
					// If a quote is detected, then this line is assumed to contain the name of the fields.
					if (line.StartsWith("\"", StringComparison.Ordinal))
					{
						tableFields = parseFile_ParseHeaderLine(line, linecount0, TrimInput_Boolean, Delimiter, parseFlagHeader);
						numFields = tableFields.Count;
						// Read another line of data to be used below
						headers_found = true;
						continue;
					}
				}
				else if (HeaderLineList != null)
				{
					// Calling code has specified the header rows.  Check to see if this is a row.
					if (parseFile_LineMatchesLineFromList(linecount0,HeaderLineList, HeaderLinesList_size))
					{
						// This row has been specified as a header row so process it.
						tableFields = parseFile_ParseHeaderLine(line, linecount0, TrimInput_Boolean, Delimiter, parseFlagHeader);
						numFields = tableFields.Count;

						//FIXME SAM 2008-01-27 Figure out how to deal with multi-row headings
						// What is the column name?
						// If the maximum header row has been processed, indicate that headers have been found.
						//if ( linecount0 == HeaderLines_Vector_maxval ) {
							headers_found = true;
						//}
						// Now read another line of data to be used below.
						continue;
					}
				}
			}

			if (linecount0 <= HeaderLinesList_maxval)
			{
				// Currently only allow one header row so need to ignore other rows that are found
				// (don't want them considered as data).
				if (parseFile_LineMatchesLineFromList(linecount0,HeaderLineList, HeaderLinesList_size))
				{
					continue;
				}
			}

			// Now evaluate the data lines.  Parse into tokens to allow evaluation of the number of columns below.

			++dataLineCount;
			// If "Top" was specified as a parameter, skip lines after top
			if ((top >= 0) && (dataLineCount > top))
			{
				break;
			}

			if (TrimInput_Boolean)
			{
				v = StringUtil.breakStringList(line.Trim(), Delimiter, parseFlag);
			}
			else
			{
				v = StringUtil.breakStringList(line, Delimiter, parseFlag);
			}
			numColumnsParsed = v.Count;
			if (numColumnsParsed > maxColumns)
			{
				maxColumns = numColumnsParsed;
			}
			// Save the tokens from the data rows - this will NOT include comments, headers, or lines to be excluded.
			data_record_tokens.Add(v);
		}
		// Close the file...
		@in.Close();

		// Make sure that the table fields are in place for the maximum number of columns.

		if (tableFields == null)
		{
			tableFields = new List<TableField>();
			for (int i = 0; i < maxColumns; i++)
			{
				// Default field definition builds String fields
				tableFields.Add(new TableField());
			}
		}
		else
		{
			// Add enough fields to account for the maximum number of columns in the table.  
			string temp = null;
			for (int i = numFields; i < maxColumns; i++)
			{
				tableField = new TableField();
				temp = "Field_" + (i + 1);
				while (findPreviousFieldNameOccurances(tableFields,temp))
				{
					temp = temp + "_2";
				}
				tableField.setName(temp);
				tableField.setDataType(TableField.DATA_TYPE_STRING);
				tableFields.Add(tableField);
			}
		}

		// Loop through the data and determine what type of data are in each column.
		// Do this in any case because the length of the string columns and precision for floating point
		// columns need to be determined.

		numFields = tableFields.Count;
		int numRecords = data_record_tokens.Count; // Number of data records
		int[] count_int = new int[maxColumns];
		int[] count_double = new int[maxColumns];
		int[] count_string = new int[maxColumns];
		int[] count_blank = new int[maxColumns];
		int[] lenmax_string = new int[maxColumns];
		int[] precision = new int[maxColumns];
		for (int icol = 0; icol < maxColumns; icol++)
		{
			count_int[icol] = 0;
			count_double[icol] = 0;
			count_string[icol] = 0;
			count_blank[icol] = 0;
			lenmax_string[icol] = 0;
			precision[icol] = 0;
		}
		// Loop through all rows of data that were read
		int vsize;
		string cell;
		string cell_trimmed; // Must have when checking for types.
		int periodPos; // Position of period in floating point numbers
		bool isTypeFound = false;
		for (int irow = 0; irow < numRecords; irow++)
		{
			// If "Top" was specified as a parameter, skip lines after top
			if ((top >= 0) && (irow > topm1))
			{
				break;
			}
			v = data_record_tokens[irow];
			vsize = v.Count;
			// Loop through all columns in the row.
			for (int icol = 0; icol < vsize; icol++)
			{
				cell = v[icol];
				cell_trimmed = cell.Trim();
				isTypeFound = false;
				if (cell_trimmed.Length == 0)
				{
					// Blank cell - can be any type and should not impact result
					++count_blank[icol];
					isTypeFound = true;
				}
				if (StringUtil.isInteger(cell_trimmed))
				{
					++count_int[icol];
					// Length needed in case handled as string data
					lenmax_string[icol] = Math.Max(lenmax_string[icol], cell_trimmed.Length);
					isTypeFound = true;
				}
				// TODO SAM 2012-05-31 Evaluate whether this needs a more robust solution
				// Sometimes long integers won't parse in the above but do get parsed as doubles below.  This can
				// lead to treatment as a floating point number.  Instead, the column likely should be treated as
				// strings.  An example is very long identifiers like "394359105411900".  For now the work-around
				// is to add quotes in the original data to make sure the column is treated like a string.
				// Could add a long but this cascades through a lot of code since the long type is not yet supported
				// in DataTable
				if (StringUtil.isDouble(cell_trimmed))
				{
					++count_double[icol];
					isTypeFound = true;
					// Length needed in case handled as string data
					lenmax_string[icol] = Math.Max(lenmax_string[icol], cell_trimmed.Length);
					// Precision to help with visualization, such as table views
					periodPos = cell_trimmed.IndexOf(".", StringComparison.Ordinal);
					if (periodPos >= 0)
					{
						precision[icol] = Math.Max(precision[icol], (cell_trimmed.Length - periodPos - 1));
					}
				}
				// TODO SAM 2008-01-27 Need to handle date/time?
				if (!isTypeFound)
				{
					// Assume string, but strip off the quotes if necessary
					++count_string[icol];
					if (TrimStrings_boolean)
					{
						lenmax_string[icol] = Math.Max(lenmax_string[icol], cell_trimmed.Length);
					}
					else
					{
						lenmax_string[icol] = Math.Max(lenmax_string[icol], cell.Length);
					}
				}
			}
		}

		// TODO SAM 2016-08-25 Could optimize so that if all column types are specified, don't need to scan data for type

		// Loop through the table fields and based on the examination of data above,
		// set the table field type and if a string, max width.

		int[] tableFieldType = new int[tableFields.Count];
		bool isString = false;
		bool isDateTime = false;
		bool isInteger = false;
		bool isDouble = false;
		if (ColumnDataTypes_Auto_boolean)
		{
			for (int icol = 0; icol < maxColumns; icol++)
			{
				isDateTime = false;
				tableField = (TableField)tableFields[icol];
				if (dateTimeColumns != null)
				{
					for (int i = 0; i < dateTimeColumns.Length; i++)
					{
						if (dateTimeColumns[i].Equals(tableField.getName(), StringComparison.OrdinalIgnoreCase))
						{
							isDateTime = true;
						}
					}
				}
				isDouble = false;
				if (doubleColumns != null)
				{
					for (int i = 0; i < doubleColumns.Length; i++)
					{
						if (doubleColumns[i].Equals(tableField.getName(), StringComparison.OrdinalIgnoreCase))
						{
							isDouble = true;
						}
					}
				}
				isInteger = false;
				if (integerColumns != null)
				{
					for (int i = 0; i < integerColumns.Length; i++)
					{
						if (integerColumns[i].Equals(tableField.getName(), StringComparison.OrdinalIgnoreCase))
						{
							isInteger = true;
						}
					}
				}
				isString = false;
				if (textColumns != null)
				{
					for (int i = 0; i < textColumns.Length; i++)
					{
						if (textColumns[i].Equals(tableField.getName(), StringComparison.OrdinalIgnoreCase))
						{
							isString = true;
						}
					}
				}
				// Set column type based on calling code specified type and then discovery from data
				if (isDateTime)
				{
					tableField.setDataType(TableField.DATA_TYPE_DATETIME);
					tableFieldType[icol] = TableField.DATA_TYPE_DATETIME;
					Message.printStatus(2, routine, "Column [" + icol + "] type is date/time as determined from specified column type.");
				}
				else if (isDouble)
				{
					tableField.setDataType(TableField.DATA_TYPE_DOUBLE);
					tableFieldType[icol] = TableField.DATA_TYPE_DOUBLE;
					Message.printStatus(2, routine, "Column [" + icol + "] type is double as determined from specified column type.");
					tableField.setWidth(lenmax_string[icol]);
					tableField.setPrecision(precision[icol]);
					// Default the following
					//tableField.setWidth (-1);
					//tableField.setPrecision ( 6 );
				}
				else if (isInteger)
				{
					tableField.setDataType(TableField.DATA_TYPE_INT);
					tableFieldType[icol] = TableField.DATA_TYPE_INT;
					Message.printStatus(2, routine, "Column [" + icol + "] type is integer as determined from specified column type.");
				}
				else if (isString)
				{
					tableField.setDataType(TableField.DATA_TYPE_STRING);
					tableFieldType[icol] = TableField.DATA_TYPE_STRING;
					if (lenmax_string[icol] <= 0)
					{
						// Likely that the entire column of numbers is empty so set the width to the field name
						// width if available)
						tableField.setWidth(tableFields[icol].getName().Length);
					}
					else
					{
						tableField.setWidth(lenmax_string[icol]);
					}
					Message.printStatus(2, routine, "Column [" + icol + "] type is string as determined from specified column type.");
				}
				else if ((count_int[icol] > 0) && (count_string[icol] == 0) && ((count_double[icol] == 0) || (count_int[icol] == count_double[icol])))
				{
					// All data are integers so assume column type is integer
					// Note that integers also meet the criteria of double, hence the extra check above
					// TODO SAM 2013-02-17 Need to handle DATA_TYPE_LONG
					tableField.setDataType(TableField.DATA_TYPE_INT);
					tableFieldType[icol] = TableField.DATA_TYPE_INT;
					tableField.setWidth(lenmax_string[icol]);
					Message.printStatus(2, routine, "Column [" + icol + "] type is integer as determined from examining data (" + count_int[icol] + " integers, " + count_double[icol] + " doubles, " + count_string[icol] + " strings, " + count_blank[icol] + " blanks).");
				}
				else if ((count_double[icol] > 0) && (count_string[icol] == 0))
				{
					// All data are double (integers will also count as double) so assume column type is double
					tableField.setDataType(TableField.DATA_TYPE_DOUBLE);
					tableFieldType[icol] = TableField.DATA_TYPE_DOUBLE;
					tableField.setWidth(lenmax_string[icol]);
					tableField.setPrecision(precision[icol]);
					Message.printStatus(2, routine, "Column [" + icol + "] type is double as determined from examining data (" + count_int[icol] + " integers, " + count_double[icol] + " doubles, " + count_string[icol] + " strings, " + count_blank[icol] + " blanks, width=" + lenmax_string[icol] + ", precision=" + precision[icol] + ".");
				}
				else
				{
					// Based on what is known, can only treat column as containing strings.
					tableField.setDataType(TableField.DATA_TYPE_STRING);
					tableFieldType[icol] = TableField.DATA_TYPE_STRING;
					if (lenmax_string[icol] <= 0)
					{
						// Likely that the entire column of numbers is empty so set the width to the field name
						// width if available)
						tableField.setWidth(tableFields[icol].getName().Length);
					}
					else
					{
						tableField.setWidth(lenmax_string[icol]);
					}
					Message.printStatus(2, routine, "Column [" + icol + "] type is string as determined from examining data (" + count_int[icol] + " integers, " + count_double[icol] + " doubles, " + count_string[icol] + " strings), " + count_blank[icol] + " blanks.");
				   // Message.printStatus ( 2, routine, "length max=" + lenmax_string[icol] );
				}
			}
		}
		else
		{
			// All are strings (from above but reset just in case)...
			for (int icol = 0; icol < maxColumns; icol++)
			{
				tableField = (TableField)tableFields[icol];
				tableField.setDataType(TableField.DATA_TYPE_STRING);
				tableFieldType[icol] = TableField.DATA_TYPE_STRING;
				tableField.setWidth(lenmax_string[icol]);
				Message.printStatus(2, routine,"Column [" + icol + "] type is " + tableField.getDataType() + " all strings assumed, width =" + tableField.getWidth());
			}
		}
		// The data fields may have less columns than the headers and if so set the field type of the
		// unknown columns to string
		for (int icol = maxColumns; icol < tableFields.Count; icol++)
		{
			tableFieldType[icol] = TableField.DATA_TYPE_STRING;
		}

		// Create the table from the field information.

		DataTable table = new DataTable(tableFields);
		table._haveDataInMemory = true;
		TableRecord tablerec = null;

		// Now transfer the data records to table records.

		int cols = 0;
		int errorCount = 0;
		for (int irow = 0; irow < numRecords; irow++)
		{
			// If "Top" was specified as a parameter, skip lines after top
			if ((top >= 0) && (irow > topm1))
			{
				break;
			}
			v = data_record_tokens[irow];

			tablerec = new TableRecord(maxColumns);
			cols = v.Count;
			for (int icol = 0; icol < cols; icol++)
			{
				if (TrimStrings_boolean)
				{
					cell = v[icol].Trim();
				}
				else
				{
					cell = v[icol];
				}
				if (ColumnDataTypes_Auto_boolean)
				{
					// Set the data as an object of the column type.
					if (tableFieldType[icol] == TableField.DATA_TYPE_INT)
					{
						cell = cell.Trim();
						if (parseFile_CellContentsNull(cell))
						{
							tablerec.addFieldValue(null);
						}
						else
						{
							tablerec.addFieldValue(Convert.ToInt32(cell));
						}
					}
					else if (tableFieldType[icol] == TableField.DATA_TYPE_DATETIME)
					{
						cell = cell.Trim();
						if (parseFile_CellContentsNull(cell))
						{
							tablerec.addFieldValue(null);
						}
						else
						{
							try
							{
								tablerec.addFieldValue(DateTime.parse(cell.Replace("\"", "")));
							}
							catch (Exception)
							{
								tablerec.addFieldValue(null);
							}
						}
					}
					else if (tableFieldType[icol] == TableField.DATA_TYPE_DOUBLE)
					{
						cell = cell.Trim();
						if (parseFile_CellContentsNull(cell))
						{
							tablerec.addFieldValue(null);
						}
						else
						{
							tablerec.addFieldValue(Convert.ToDouble(cell));
						}
					}
					else if (tableFieldType[icol] == TableField.DATA_TYPE_STRING)
					{
						// Know that it is a string.
						// Could contain embedded "" that need to be replaced with single "
						tablerec.addFieldValue(parseFile_ProcessString(cell));
					}
					else
					{
						// Add as string
						tablerec.addFieldValue(parseFile_ProcessString(cell));
					}
				}
				else
				{
					// Set as the string value.
					tablerec.addFieldValue(parseFile_ProcessString(cell));
				}
			}

			// If the specific record does not have enough columns, pad the columns at the end with blanks,
			// using blank strings or NaN for number fields.  This depends on whether headings were read.
			// Sometimes the header row has more columns than data rows, in particular because breakStringList()
			// will drop an empty field at the end.

			for (int icol = cols; icol < table.getNumberOfFields(); icol++)
			{
				if (ColumnDataTypes_Auto_boolean)
				{
					// Add values based on the column type.
					if (tableFieldType[icol] == TableField.DATA_TYPE_STRING)
					{
						tablerec.addFieldValue("");
					}
					else
					{
						tablerec.addFieldValue(null);
					}
				}
				else
				{
					// Add a blank string.
					tablerec.addFieldValue("");
				}
			}

			try
			{
				table.addRecord(tablerec);
			}
			catch (Exception e)
			{
				Message.printWarning(3, routine, "Error adding row to table at included data row [" + irow + "] uniquetempvar.");
				++errorCount;
			}
		}
		if (errorCount > 0)
		{
			// There were errors processing the data
			string message = "There were " + errorCount + " errors processing the data.";
			Message.printWarning(3, routine, message);
			throw new Exception(message);
		}

		return table;
	}

	/// <summary>
	/// Determine whether a cell's string contents are null.
	/// This will be the case if the cell is empty, "null" (upper or lower case).
	/// Call this when processing non-text cells that need to store a value (double, integer, boolean, date/time, etc.).
	/// </summary>
	private static bool parseFile_CellContentsNull(string cell)
	{
		if ((string.ReferenceEquals(cell, null)) || cell.Length == 0 || cell.ToUpper().Equals("NULL"))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Determine whether a line from the file matches the list of rows that are of interest. </summary>
	/// <param name="linecount0"> </param>
	/// <param name="rows_List"> list of Integer objects that are row numbers (0+) of interest. </param>
	/// <param name="rows_List_size"> Size of rows_List - used to speed up performance. </param>
	/// <returns> true if the line matches an item in the list. </returns>
	private static bool parseFile_LineMatchesLineFromList(int linecount0, IList<int> rows_List, int rows_List_size)
	{
		int? int_object;
		if (rows_List != null)
		{
			rows_List_size = rows_List.Count;
		}
		for (int @is = 0; @is < rows_List_size; @is++)
		{
			int_object = rows_List[@is];
			if (linecount0 == int_object.Value)
			{
				// Skip the line as requested
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Parse a line that is known to be a header line to initialize the table fields.
	/// All fields are set to type String, although this will be reset when data records are processed. </summary>
	/// <param name="line"> Line to parse. </param>
	/// <param name="linecount0"> Line number (0+). </param>
	/// <param name="TrimInput_Boolean"> Indicates whether input rows should be trimmed before parsing. </param>
	/// <param name="Delimiter"> The delimiter characters for parsing the line into tokens. </param>
	/// <param name="parse_flag"> the flag to be passed to StringUtil.breakStringList() when parsing the line. </param>
	/// <returns> A list of TableField describing the table columns. </returns>
	private static IList<TableField> parseFile_ParseHeaderLine(string line, int linecount0, bool TrimInput_Boolean, string Delimiter, int parse_flag)
	{
		string routine = "DataTable.parseFile_ParseHeaderLine";
		Message.printStatus(2, routine, "Adding column headers from line [" + linecount0 + "]: " + line);
		IList<string> columns = null;
		if (TrimInput_Boolean)
		{
			columns = StringUtil.breakStringList(line.Trim(), Delimiter, parse_flag);
		}
		else
		{
			columns = StringUtil.breakStringList(line, Delimiter, parse_flag);
		}

		int numFields = columns.Count;
		IList<TableField> tableFields = new List<TableField>();
		TableField tableField = null;
		string temp = null;
		for (int i = 0; i < numFields; i++)
		{
			temp = columns[i].Trim();
			while (findPreviousFieldNameOccurances(tableFields, temp))
			{
				temp = temp + "_2";
			}
			tableField = new TableField();
			tableField.setName(temp);
			// All table fields by default are treated as strings.
			tableField.setDataType(TableField.DATA_TYPE_STRING);
			tableFields.Add(tableField);
		}
		return tableFields;
	}

	/// <summary>
	/// Process a string table field value before setting as data in the table.
	/// </summary>
	private static string parseFile_ProcessString(string cell)
	{
		if ((string.ReferenceEquals(cell, null)) || (cell.Length == 0))
		{
			return cell;
		}
		char c1 = cell[0];
		int len = cell.Length;
		char c2 = cell[len - 1];
		if ((c1 == '"') || (c1 == '\''))
		{
			// Have a quoted string.  Remove the quotes from each end (but not the middle)
			// Embedded quotes will typically be represented as double quote "" or '' so replace
			if ((c2 == c1) && (len > 1))
			{
				return cell.Substring(1, (len - 1) - 1).Replace("\"\"", "\"");
				// Add single quotes later if necessary...seem to mainly deal with double quotes
			}
			else
			{
				// Truncated field or error in input?  Unlikely case
				return cell.Substring(1);
			}
		}
		else
		{
			return cell;
		}
	}

	/// <summary>
	/// Set the comments string list. </summary>
	/// <param name="comments"> Comments to set. </param>
	public virtual void setComments(IList<string> comments)
	{
		if (comments != null)
		{
			__comments = comments;
		}
	}

	/// <summary>
	/// Sets the value of a specific field. </summary>
	/// <param name="row"> the row (0+) in which to set the value. </param>
	/// <param name="col"> the column (0+) in which to set the value. </param>
	/// <param name="value"> the value to set. </param>
	/// <exception cref="Exception"> if the field value cannot be set, including if the row does not exist. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setFieldValue(int row, int col, Object value) throws Exception
	public virtual void setFieldValue(int row, int col, object value)
	{
		setFieldValue(row, col, value, false);
	}

	/// <summary>
	/// Sets the value of a specific field. </summary>
	/// <param name="row"> the row (0+) in which to set the value . </param>
	/// <param name="col"> the column (0+) in which to set the value. </param>
	/// <param name="value"> the value to set. </param>
	/// <param name="createIfNecessary"> if true and the requested row is not in the existing rows, create
	/// intervening rows, initialize to missing (null objects), and then set the data. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setFieldValue(int row, int col, Object value, boolean createIfNecessary) throws Exception
	public virtual void setFieldValue(int row, int col, object value, bool createIfNecessary)
	{
		int nRows = getNumberOfRecords();
		if ((row > (nRows - 1)) && createIfNecessary)
		{
			// Create empty rows
			for (int i = nRows; i <= row; i++)
			{
				addRecord(emptyRecord());
			}
		}
		// Now set the value (will throw ArrayIndexOutOfBoundsException if row is out of range)...
		TableRecord record = _table_records[row];
		record.setFieldValue(col, value);
	}

	/// <summary>
	/// Sets the width of the field. </summary>
	/// <param name="col"> the column for which to set the width. </param>
	/// <param name="width"> the width to set. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setFieldWidth(int col, int width) throws Exception
	public virtual void setFieldWidth(int col, int width)
	{
		TableField field = _table_fields[col];
		field.setWidth(width);
	}

	/// <summary>
	/// Set the table identifier. </summary>
	/// <param name="table_id"> Identifier for the table </param>
	public virtual void setTableID(string table_id)
	{
		__table_id = table_id;
	}

	/// <summary>
	/// Set the number of records in the table.  This method should typically only be
	/// called when data are read on-the-fly (and are not stored in memory in the table records). </summary>
	/// <param name="num_records"> Number of records in the table. </param>
	public virtual void setNumberOfRecords(int num_records)
	{
		_num_records = num_records;
	}

	/// <summary>
	/// Set field data type and header for the specified zero-based index. </summary>
	/// <param name="index"> index of field to set </param>
	/// <param name="data_type"> data type; use TableField.DATA_TYPE_* </param>
	/// <param name="name"> name of the field. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setTableField(int index, int data_type, String name) throws Exception
	public virtual void setTableField(int index, int data_type, string name)
	{
		if (_table_fields.Count <= index)
		{
			throw new Exception("Index " + index + " is not valid.");
		}
		TableField tableField = _table_fields[index];
		tableField.setDataType(data_type);
		tableField.setName(name);
	}

	/// <summary>
	/// Set the table fields to define the table. </summary>
	/// <param name="tableFieldsList"> a list of TableField objects defining table contents. </param>
	public virtual void setTableFields(IList<TableField> tableFieldsList)
	{
		_table_fields = tableFieldsList;
	}

	/// <summary>
	/// Set table field name. </summary>
	/// <param name="index"> index of field to set (zero-based). </param>
	/// <param name="name"> Field name. </param>
	/// <exception cref="If"> the index is out of range. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setTableFieldName(int index, String name) throws Exception
	public virtual void setTableFieldName(int index, string name)
	{
		if (_table_fields.Count <= index)
		{
			throw new Exception("Index " + index + " is not valid.");
		}
		TableField tableField = _table_fields[index];
		tableField.setName(name);
	}

	/// <summary>
	/// Set field data type for the specified zero-based index. </summary>
	/// <param name="index"> index of field to set </param>
	/// <param name="data_type"> data type; use TableField.DATA_TYPE_* </param>
	/// <exception cref="If"> the index is out of range. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setTableFieldType(int index, int data_type) throws Exception
	public virtual void setTableFieldType(int index, int data_type)
	{
		if (_table_fields.Count <= index)
		{
			throw new Exception("Index " + index + " is not valid.");
		}
		TableField tableField = _table_fields[index];
		tableField.setDataType(data_type);
	}

	/// <summary>
	/// Set values in the list of table records and setting values for specific columns in each record.
	/// The records might exist in a table or may not yet have been added to the table.
	/// The table records are modified directly, rather than trying to find the row in the table to modify. </summary>
	/// <param name="tableRecords"> list of TableRecord to set values in </param>
	/// <param name="columnValues"> map for columns values that will be set, where rows to be modified will be the result of the filters;
	/// values are strings and need to be converged before setting, based on column type </param>
	/// <param name="getter"> a DataTableValueStringProvider implementation, which is called prior to setting values if not null,
	/// used to provide ability to dynamically format the values being set in the table </param>
	/// <param name="createColumns"> indicates whether new columns should be created if necessary
	/// (currently ignored due to need to synchronize the table records and full table) </param>
	public virtual void setTableRecordValues(IList<TableRecord> tableRecords, Dictionary<string, string> columnValues, DataTableValueStringProvider getter, bool createColumns)
	{
		string routine = this.GetType().Name + ".setTableRecordValues";
		if (tableRecords == null)
		{
			return;
		}
		// List of columns that will be set, taken from keys in the column values
		int errorCount = 0;
		StringBuilder errorMessage = new StringBuilder();
		// Get the column numbers and values to to set
		string[] columnNamesToSet = new string[columnValues.Count];
		string[] columnValuesToSet = new string[columnValues.Count];
		int[] columnNumbersToSet = new int[columnValues.Count];
		int[] columnTypesToSet = new int[columnValues.Count];
		int ikey = -1;
		foreach (KeyValuePair<string, string> pairs in columnValues.SetOfKeyValuePairs())
		{
			columnNumbersToSet[++ikey] = -1;
			try
			{
				columnNamesToSet[ikey] = pairs.Key;
				columnValuesToSet[ikey] = pairs.Value;
				columnNumbersToSet[ikey] = getFieldIndex(columnNamesToSet[ikey]);
				columnTypesToSet[ikey] = getFieldDataType(columnNumbersToSet[ikey]);
				//Message.printStatus(2,routine,"Setting column \"" + columnNamesToSet[ikey] + " " + columnNumbersToSet[ikey] + "\"");
			}
			catch (Exception)
			{
				// OK, will add the column below
			}
		}
		// If necessary, add columns to the table and records.  For now, always treat as strings
		// TODO SAM 2013-08-06 Evaluate how to handle other data types in set
		//TableField newTableField;
		// Create requested columns in the output table
		for (int icol = 0; icol < columnNumbersToSet.Length; icol++)
		{
			if ((columnNumbersToSet[icol] < 0) && createColumns)
			{
				errorMessage.Append("  createColumns=true is not yet supported.");
				/*
				// Did not find the column in the table so add a String column for null values
				newTableField = new TableField(TableField.DATA_TYPE_STRING, columnNamesToSet[icol], -1, -1);
				// Add to the full table
				columnNumbersToSet[icol] = addField(newTableField, null );
				columnTypesToSet[icol] = getFieldDataType(columnNumbersToSet[icol]);
				*/
			}
		}
		// Now loop through all the provided data records and set values
		int icol;
		foreach (TableRecord rec in tableRecords)
		{
			string columnValueToSet = null; // A single value to set, may contain formatting such as ${Property} when used with TSTool
			for (icol = 0; icol < columnNumbersToSet.Length; icol++)
			{
				try
				{
					// OK if setting to null value, but hopefully should not happen
					// TODO SAM 2013-08-06 Handle all column types
					//Message.printStatus(2,routine,"Setting ColNum=" + columnNumbersToSet[icol] + " RowNum=" + irow + " value=" +
					//    columnValues.get(columnNamesToSet[icol]));
					if (columnNumbersToSet[icol] >= 0)
					{
						columnValueToSet = columnValuesToSet[icol];
						if (getter != null)
						{
							// columnValueToSet will initially have formatting information like ${Property}
							columnValueToSet = getter.getTableCellValueAsString(columnValueToSet);
						}
						if (columnTypesToSet[icol] == TableField.DATA_TYPE_INT)
						{
							// TODO SAM 2013-08-26 Should parse the values once rather than each time set to improve error handling and performance
							rec.setFieldValue(columnNumbersToSet[icol], int.Parse(columnValueToSet));
						}
						else if (columnTypesToSet[icol] == TableField.DATA_TYPE_DOUBLE)
						{
							// TODO SAM 2013-08-26 Should parse the values once rather than each time set to improve error handling and performance
							rec.setFieldValue(columnNumbersToSet[icol], double.Parse(columnValueToSet));
						}
						else if (columnTypesToSet[icol] == TableField.DATA_TYPE_STRING)
						{
							rec.setFieldValue(columnNumbersToSet[icol], columnValueToSet);
						}
						else
						{
							errorMessage.Append("Do not know how to set data type (" + TableColumnType.valueOf(columnTypesToSet[icol]) + ") for column \"" + columnNamesToSet[icol] + "].");
							++errorCount;
						}
					}
				}
				catch (Exception e)
				{
					// Should not happen
					errorMessage.Append("Error setting table record [" + columnNumbersToSet[icol] + "] uniquetempvar.");
					Message.printWarning(3, routine, "Error setting new table data for [" + columnNumbersToSet[icol] + "] uniquetempvar.");
					Message.printWarning(3, routine, e);
					++errorCount;
				}
			}
		}
		if (errorCount > 0)
		{
			throw new Exception("There were + " + errorCount + " errors setting table values: " + errorMessage);
		}
	}

	/// <summary>
	/// Set values in the table by first matching rows using column filters (default is match all) and
	/// then setting values for specific columns.
	/// This method is called by overloaded versions that specify either column filters or lists of records. </summary>
	/// <param name="columnFilters"> map to filter rows to set values in </param>
	/// <param name="columnValues"> map for columns values that will be set, where rows to be modified will be the result of the filters;
	/// values are strings and need to be converged before setting, based on column type </param>
	/// <param name="getter"> a DataTableValueStringProvider implementation, which is called prior to setting values if not null,
	/// used to provide ability to dynamically format the values being set in the table </param>
	/// <param name="createColumns"> indicates whether new columns should be created if necessary </param>
	public virtual void setTableValues(Dictionary<string, string> columnFilters, Dictionary<string, string> columnValues, DataTableValueStringProvider getter, bool createColumns)
	{
		string routine = this.GetType().Name + ".setTableValues";
		// List of columns that will be set, taken from keys in the column values
		int errorCount = 0;
		StringBuilder errorMessage = new StringBuilder();
		// Get filter columns and glob-style regular expressions
		int[] columnNumbersToFilter = new int[columnFilters.Count];
		string[] columnFilterGlobs = new string[columnFilters.Count];
		IEnumerator<string> keys = columnFilters.Keys.GetEnumerator();
		int ikey = -1;
		string key = null;
		while (keys.MoveNext())
		{
			++ikey;
			columnNumbersToFilter[ikey] = -1;
			try
			{
				key = keys.Current;
				columnNumbersToFilter[ikey] = getFieldIndex(key);
				columnFilterGlobs[ikey] = columnFilters[key];
				// Turn default globbing notation into internal Java regex notation
				columnFilterGlobs[ikey] = columnFilterGlobs[ikey].Replace("*", ".*").ToUpper();
			}
			catch (Exception)
			{
				++errorCount;
				if (errorMessage.Length > 0)
				{
					errorMessage.Append(" ");
				}
				errorMessage.Append("Filter column \"" + key + "\" not found in table.");
			}
		}
		// Get the column numbers and values to to set
		string[] columnNamesToSet = new string[columnValues.Count];
		string[] columnValuesToSet = new string[columnValues.Count];
		int[] columnNumbersToSet = new int[columnValues.Count];
		int[] columnTypesToSet = new int[columnValues.Count];
		ikey = -1;
		foreach (KeyValuePair<string, string> pairs in columnValues.SetOfKeyValuePairs())
		{
			columnNumbersToSet[++ikey] = -1;
			try
			{
				columnNamesToSet[ikey] = (string)pairs.Key;
				columnValuesToSet[ikey] = pairs.Value;
				columnNumbersToSet[ikey] = getFieldIndex(columnNamesToSet[ikey]);
				columnTypesToSet[ikey] = getFieldDataType(columnNumbersToSet[ikey]);
				//Message.printStatus(2,routine,"Setting column \"" + columnNamesToSet[ikey] + " " + columnNumbersToSet[ikey] + "\"");
			}
			catch (Exception)
			{
				// OK, will add the column below
			}
		}
		// If necessary, add columns to the table.  For now, always treat as strings
		// TODO SAM 2013-08-06 Evaluate how to handle other data types in set
		TableField newTableField;
		// Create requested columns in the output table
		for (int icol = 0; icol < columnNumbersToSet.Length; icol++)
		{
			if ((columnNumbersToSet[icol] < 0) && createColumns)
			{
				// Did not find the column in the table so add a String column for null values
				newTableField = new TableField(TableField.DATA_TYPE_STRING, columnNamesToSet[icol], -1, -1);
				columnNumbersToSet[icol] = addField(newTableField, null);
				columnTypesToSet[icol] = getFieldDataType(columnNumbersToSet[icol]);
			}
		}
		// Now loop through all the data records and set values if rows are matched
		int icol;
		bool filterMatches;
		object o = null;
		string s;
		for (int irow = 0; irow < getNumberOfRecords(); irow++)
		{
			filterMatches = true;
			if (columnNumbersToFilter.Length > 0)
			{
				// Filters can be done on any columns so loop through to see if row matches before doing set
				for (icol = 0; icol < columnNumbersToFilter.Length; icol++)
				{
					if (columnNumbersToFilter[icol] < 0)
					{
						filterMatches = false;
						break;
					}
					try
					{
						o = getFieldValue(irow, columnNumbersToFilter[icol]);
						if (o == null)
						{
							filterMatches = false;
							break; // Don't include nulls when checking values
						}
						s = ("" + o).ToUpper();
						if (!s.matches(columnFilterGlobs[icol]))
						{
							// A filter did not match so don't process the record
							filterMatches = false;
							break;
						}
					}
					catch (Exception e)
					{
						errorMessage.Append("Error getting table data for [" + irow + "][" + columnNumbersToFilter[icol] + "].");
						Message.printWarning(3, routine, "Error getting table data for [" + irow + "][" + columnNumbersToFilter[icol] + "] uniquetempvar.");
						++errorCount;
					}
				}
				//Message.printStatus(2,routine,"" + irow + " matches=" + filterMatches );
				if (!filterMatches)
				{
					// Skip the record.
					continue;
				}
			}
			string columnValueToSet = null; // A single value to set, may contain formatting such as ${Property} when used with TSTool
			for (icol = 0; icol < columnNumbersToSet.Length; icol++)
			{
				try
				{
					// OK if setting to null value, but hopefully should not happen
					// TODO SAM 2013-08-06 Handle all column types
					//Message.printStatus(2,routine,"Setting ColNum=" + columnNumbersToSet[icol] + " RowNum=" + irow + " value=" +
					//    columnValues.get(columnNamesToSet[icol]));
					if (columnNumbersToSet[icol] >= 0)
					{
						columnValueToSet = columnValuesToSet[icol];
						if (getter != null)
						{
							// columnValueToSet will initially have formatting information like ${Property}
							columnValueToSet = getter.getTableCellValueAsString(columnValueToSet);
						}
						if (columnTypesToSet[icol] == TableField.DATA_TYPE_INT)
						{
							// TODO SAM 2013-08-26 Should parse the values once rather than each time set to improve error handling and performance
							setFieldValue(irow, columnNumbersToSet[icol], int.Parse(columnValueToSet), true);
						}
						else if (columnTypesToSet[icol] == TableField.DATA_TYPE_DOUBLE)
						{
							// TODO SAM 2013-08-26 Should parse the values once rather than each time set to improve error handling and performance
							setFieldValue(irow, columnNumbersToSet[icol], double.Parse(columnValueToSet), true);
						}
						else if (columnTypesToSet[icol] == TableField.DATA_TYPE_STRING)
						{
							setFieldValue(irow, columnNumbersToSet[icol], columnValueToSet, true);
						}
						else
						{
							errorMessage.Append("Do not know how to set data type (" + TableColumnType.valueOf(columnTypesToSet[icol]) + ") for column \"" + columnNamesToSet[icol] + "].");
							++errorCount;
						}
					}
				}
				catch (Exception e)
				{
					// Should not happen
					errorMessage.Append("Error setting table data [" + irow + "][" + columnNumbersToSet[icol] + "] uniquetempvar.");
					Message.printWarning(3, routine, "Error setting new table data for [" + irow + "][" + columnNumbersToSet[icol] + "] uniquetempvar.");
					Message.printWarning(3, routine, e);
					++errorCount;
				}
			}
		}
		if (errorCount > 0)
		{
			throw new Exception("There were + " + errorCount + " errors setting table values: " + errorMessage);
		}
	}

	/// <summary>
	/// Sort the table rows by sorting a column's values. </summary>
	/// <param name="sortColumns"> the name of the columns to be sorted, allowed to be integer, double, string, or DateTime type. </param>
	/// <param name="sortOrder"> order to sort (specify as 0+ to sort ascending and < 0 to sort descending) </param>
	/// <returns> the sort order array indicating the position in the original data
	/// (useful if a parallel sort of data needs to occur) </returns>
	public virtual int [] sortTable(string[] sortColumns, int[] sortOrder)
	{ //String routine = getClass().getSimpleName() + ".sortTable";
		int[] sortColumnsNum = new int[sortColumns.Length];
		IList<string> errors = new List<string>();
		for (int i = 0; i < sortColumns.Length; i++)
		{
			sortColumnsNum[i] = -1;
			try
			{
				sortColumnsNum[i] = getFieldIndex(sortColumns[i]);
			}
			catch (Exception)
			{
				errors.Add(sortColumns[i]);
			}
		}
		if (errors.Count > 0)
		{
			StringBuilder b = new StringBuilder("The following column(s) to sort were not found in table \"" + getTableID() + "\": ");
			for (int i = 0; i < errors.Count; i++)
			{
				if (i > 0)
				{
					b.Append(",");
				}
				b.Append("\"" + errors[i] + "\"");
			}
			throw new Exception(b.ToString());
		}
		int nrecords = getNumberOfRecords();
		int sortFlag = StringUtil.SORT_ASCENDING;
		if (sortOrder[0] < 0)
		{
			sortFlag = StringUtil.SORT_DESCENDING;
		}
		int[] sortedOrderArray = new int[nrecords]; // Overall sort order different from original
		// First sort by the first column.
		int iSort = 0;
		if (getFieldDataType(sortColumnsNum[iSort]) == TableField.DATA_TYPE_STRING)
		{
			string value;
			IList<string> values = new List<string>(nrecords);
			foreach (TableRecord rec in getTableRecords())
			{
				try
				{
					value = rec.getFieldValueString(sortColumnsNum[iSort]);
					if (string.ReferenceEquals(value, null))
					{
						value = "";
					}
					else
					{
						values.Add(value);
					}
				}
				catch (Exception e)
				{
					// Should not happen but if it does it is probably bad
					throw new Exception(e);
				}
			}
			StringUtil.sortStringList(values, sortFlag, sortedOrderArray, true, true);
		}
		else if (getFieldDataType(sortColumnsNum[iSort]) == TableField.DATA_TYPE_DATETIME)
		{
			object value;
			double[] values = new double[nrecords];
			int irec = -1;
			foreach (TableRecord rec in getTableRecords())
			{
				++irec;
				try
				{
					value = rec.getFieldValue(sortColumnsNum[iSort]);
					if (value == null)
					{
						value = -double.MaxValue;
					}
					values[irec] = ((DateTime)value).toDouble();
				}
				catch (Exception e)
				{
					// Should not happen but if it does it is probably bad
					throw new Exception(e);
				}
			}
			MathUtil.sort(values, MathUtil.SORT_QUICK, sortFlag, sortedOrderArray, true);
		}
		else if ((getFieldDataType(sortColumnsNum[iSort]) == TableField.DATA_TYPE_DOUBLE) || (getFieldDataType(sortColumnsNum[iSort]) == TableField.DATA_TYPE_FLOAT))
		{
			object o;
			double value;
			double[] values = new double[nrecords];
			int irec = -1;
			foreach (TableRecord rec in getTableRecords())
			{
				++irec;
				try
				{
					o = rec.getFieldValue(sortColumnsNum[iSort]);
					if (o == null)
					{
						value = -double.MaxValue;
					}
					else
					{
						if (o is double?)
						{
							value = (double?)o.Value;
						}
						else
						{
							value = (float?)o;
						}
					}
					values[irec] = value;
				}
				catch (Exception e)
				{
					// Should not happen but if it does it is probably bad
					throw new Exception(e);
				}
			}
			MathUtil.sort(values, MathUtil.SORT_QUICK, sortFlag, sortedOrderArray, true);
		}
		else if (getFieldDataType(sortColumnsNum[iSort]) == TableField.DATA_TYPE_INT)
		{
			int? value;
			int[] values = new int[nrecords];
			int irec = -1;
			foreach (TableRecord rec in getTableRecords())
			{
				++irec;
				try
				{
					value = (int?)rec.getFieldValue(sortColumnsNum[iSort]);
					if (value == null)
					{
						value = -int.MaxValue;
					}
					values[irec] = value.Value;
				}
				catch (Exception e)
				{
					// Should not happen but if it does it is probably bad
					throw new Exception(e);
				}
			}
			MathUtil.sort(values, MathUtil.SORT_QUICK, sortFlag, sortedOrderArray, true);
		}
		else
		{
			throw new Exception("Sorting table only implemented for string, integer, double, float and DateTime columns.");
		}
		// Shuffle the table's row list according to sortOrder.  Because other objects may have references to
		// the tables record list, can't create a new list.  Therefore, copy the old list to a backup and then use
		// that to sort into an updated original list.
		IList<TableRecord> backup = new List<TableRecord>(nrecords);
		IList<TableRecord> records = this.getTableRecords();
		foreach (TableRecord rec in records)
		{
			backup.Add(rec);
		}
		// Now set from the backup to the original list
		for (int irec = 0; irec < nrecords; irec++)
		{
			records[irec] = backup[sortedOrderArray[irec]];
		}
		// Now sort by columns [1]+ (zero index).  Only sort the last column being iterated.
		// The previous columns are used to find blocks of rows to sort.  In other words, if 3 columns are sorted
		// then columns [0-1] must match and then that block of rows is sorted based on column [2].
		int iSort2;
		int lastRec = getNumberOfRecords() - 1;
		for (iSort = 1; iSort < sortColumnsNum.Length; iSort++)
		{
			object[] sortValuesPrev = null;
			int irec = -1;
			bool needToSort = false;
			object o2;
			int blockStartRow = 0, blockEndRow = 0, sortColumnMatchCount = 0;
			// Iterate through the table.
			foreach (TableRecord rec in getTableRecords())
			{
				++irec;
				//Message.printStatus(2,routine,"Processing record " + irec );
				// Check the current row's sort columns against the previous row
				if (sortValuesPrev == null)
				{
					// Initialize this row with values to be compared with the next row
					sortValuesPrev = new object[iSort];
					for (iSort2 = 0; iSort2 < iSort; iSort2++)
					{
						try
						{
							sortValuesPrev[iSort2] = rec.getFieldValue(sortColumnsNum[iSort2]);
						}
						catch (Exception e)
						{
							throw new Exception(e);
						}
					}
					blockStartRow = irec;
					blockEndRow = irec;
					//Message.printStatus(2,routine,"Initializing " + irec + " for first comparison." );
					continue;
				}
				else
				{
					// Compare this row's values with the previous block of similar values
					sortColumnMatchCount = 0;
					for (iSort2 = 0; iSort2 < iSort; iSort2++)
					{
						try
						{
							o2 = rec.getFieldValue(sortColumnsNum[iSort2]);
						}
						catch (Exception e)
						{
							throw new Exception(e);
						}
						if (((o2 == null) && (sortValuesPrev[iSort2] == null)) || (o2 != null) && o2.Equals(sortValuesPrev[iSort2]))
						{
							//Message.printStatus(2, routine, "Previous value["+iSort2+"]: " + sortValuesPrev[iSort2] + " current value=" + o2 );
							++sortColumnMatchCount;
						}
						else
						{
							// The current row did not match so save the current row as the previous and break to indicate that the block needs sorted
							//Message.printStatus(2,routine,"Record " + irec + " compare values did not match previous row." );
							break;
						}
					}
					// If all the values matched, can process another row before sorting, but check to see if at end of table below
					if (sortColumnMatchCount == iSort)
					{
						//Message.printStatus(2,routine,"Record " + irec + " sort columns match previous." );
						needToSort = false;
						blockEndRow = irec; // Advance the end of the block
					}
					else
					{
						// Current row's sort column values did not match so need to sort the block
						//Message.printStatus(2,routine,"Record " + irec + " sort columns do not match previous.  Resetting \"previous\" values to this record." );
						needToSort = true;
						// Save the current row to compare with the next row
						for (int iSort3 = 0; iSort3 < iSort; iSort3++)
						{
							try
							{
								sortValuesPrev[iSort3] = rec.getFieldValue(sortColumnsNum[iSort3]);
							}
							catch (Exception e)
							{
								throw new Exception(e);
							}
						}
					}
					if ((irec == lastRec) && (blockStartRow != blockEndRow))
					{
						// Need to sort if in the last row unless the block was only one row
						needToSort = true;
						//Message.printStatus(2, routine, "Need to sort end of table from " + blockStartRow + " to " + blockEndRow );
					}
					if (needToSort)
					{
						// Need to sort the block of rows using the "rightmost" sort column
						//Message.printStatus(2, routine, "Need to sort block of rows from " + blockStartRow + " to " + blockEndRow );
						try
						{
							//Message.printStatus(2, routine, "Sorting rows from " + blockStartRow + " to " + blockEndRow + " based on column " + sortColumnNum[iSort] );
							sortTableSubset(blockStartRow,blockEndRow,sortColumnsNum[iSort],sortOrder[iSort],sortedOrderArray);
						}
						catch (Exception e)
						{
							throw new Exception(e);
						}
						// Now that the block has been started, reset for the next block
						// blockStartRow should = irec since rec was different and triggered the sort of the previous block
						blockStartRow = blockEndRow + 1;
						blockEndRow = blockStartRow;
					}
					//Message.printStatus(2, routine, "At end of loop irec=" + irec + " blockStartRow=" + blockStartRow + " blockEndRow=" + blockEndRow );
				}
			}
		}
		return sortedOrderArray;
	}

	/// <summary>
	/// Sort a subset of a table.  This is called internally by other methods. </summary>
	/// <param name="blockStartRow"> starting row (0 index) to sort </param>
	/// <param name="blockEndRow"> ending row (0 index) to sort </param>
	/// <param name="iCol"> column number to sort </param>
	/// <param name="sortOrder"> sort order </param>
	/// <param name="sortedOrderArray"> the sort order array indicating the position in the original data
	/// (useful if a parallel sort of data needs to occur) </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sortTableSubset(int blockStartRow, int blockEndRow, int iCol, int sortOrder, int [] sortedOrderArray) throws Exception
	private void sortTableSubset(int blockStartRow, int blockEndRow, int iCol, int sortOrder, int[] sortedOrderArray)
	{
		if (blockStartRow == blockEndRow)
		{
			// Only one row to sort
			return;
		}
		int nrecords = blockEndRow - blockStartRow + 1; // Number of records in the block to sort
		int[] sortOrderArray2 = new int[nrecords]; // Overall sort order different from original
		// First sort by the first column.
		int sortFlag = StringUtil.SORT_ASCENDING;
		if (sortOrder < 0)
		{
			sortFlag = StringUtil.SORT_DESCENDING;
		}
		if (getFieldDataType(iCol) == TableField.DATA_TYPE_STRING)
		{
			string value;
			IList<string> values = new List<string>(nrecords);
			TableRecord rec;
			for (int irec = blockStartRow; irec <= blockEndRow; irec++)
			{
				rec = getRecord(irec);
				try
				{
					value = rec.getFieldValueString(iCol);
					if (string.ReferenceEquals(value, null))
					{
						value = "";
					}
					else
					{
						values.Add(value);
					}
				}
				catch (Exception e)
				{
					// Should not happen but if it does it is probably bad
					throw new Exception(e);
				}
			}
			StringUtil.sortStringList(values, sortFlag, sortOrderArray2, true, true);
		}
		else if (getFieldDataType(iCol) == TableField.DATA_TYPE_DATETIME)
		{
			object value;
			double[] values = new double[nrecords];
			TableRecord rec;
			for (int irec = blockStartRow, pos = 0; irec <= blockEndRow; irec++, pos++)
			{
				rec = getRecord(irec);
				try
				{
					value = rec.getFieldValue(iCol);
					if (value == null)
					{
						value = -double.MaxValue;
					}
					values[pos] = ((DateTime)value).toDouble();
				}
				catch (Exception e)
				{
					// Should not happen but if it does it is probably bad
					throw new Exception(e);
				}
			}
			MathUtil.sort(values, MathUtil.SORT_QUICK, sortFlag, sortOrderArray2, true);
		}
		else if ((getFieldDataType(iCol) == TableField.DATA_TYPE_DOUBLE) || (getFieldDataType(iCol) == TableField.DATA_TYPE_FLOAT))
		{
			object o;
			double value;
			double[] values = new double[nrecords];
			TableRecord rec;
			for (int irec = blockStartRow, pos = 0; irec <= blockEndRow; irec++, pos++)
			{
				rec = getRecord(irec);
				try
				{
					o = rec.getFieldValue(iCol);
					if (o == null)
					{
						value = -double.MaxValue;
					}
					else
					{
						if (o is double?)
						{
							value = (double?)o.Value;
						}
						else
						{
							value = (float?)o;
						}
					}
					values[pos] = value;
				}
				catch (Exception e)
				{
					// Should not happen but if it does it is probably bad
					throw new Exception(e);
				}
			}
			MathUtil.sort(values, MathUtil.SORT_QUICK, sortFlag, sortOrderArray2, true);
		}
		else if (getFieldDataType(iCol) == TableField.DATA_TYPE_INT)
		{
			int? value;
			int[] values = new int[nrecords];
			TableRecord rec;
			for (int irec = blockStartRow, pos = 0; irec <= blockEndRow; irec++, pos++)
			{
				rec = getRecord(irec);
				try
				{
					value = (int?)rec.getFieldValue(iCol);
					if (value == null)
					{
						value = -int.MaxValue;
					}
					values[pos] = value.Value;
				}
				catch (Exception e)
				{
					// Should not happen but if it does it is probably bad
					throw new Exception(e);
				}
			}
			MathUtil.sort(values, MathUtil.SORT_QUICK, sortFlag, sortOrderArray2, true);
		}
		else
		{
			throw new Exception("Sorting table only implemented for string, integer, double, float and DateTime columns.");
		}
		// Shuffle the table's row list according to sortOrder.  Because other objects may have references to
		// the tables record list, can't create a new list.  Therefore, copy the old list to a backup and then use
		// that to sort into an updated original list.
		IList<TableRecord> backup = new List<TableRecord>(nrecords);
		IList<TableRecord> records = this.getTableRecords();
		TableRecord rec;
		for (int irec = blockStartRow; irec <= blockEndRow; irec++)
		{
			rec = getRecord(irec);
			backup.Add(rec);
		}
		// Now set from the backup to the original list
		for (int irec = blockStartRow; irec <= blockEndRow; irec++)
		{
			records[irec] = backup[sortOrderArray2[irec - blockStartRow]];
			sortedOrderArray[irec] = sortOrderArray2[irec - blockStartRow];
		}
	}

	/// <summary>
	/// Set whether strings should be trimmed at read. </summary>
	/// <param name="trim_strings"> If true, strings will be trimmed at read. </param>
	/// <returns> Boolean value indicating whether strings should be trimmed, after reset. </returns>
	public virtual bool trimStrings(bool trim_strings)
	{
		_trim_strings = trim_strings;
		return _trim_strings;
	}

	/// <summary>
	/// Indicate whether strings should be trimmed at read. </summary>
	/// <returns> Boolean value indicating whether strings should be trimmed. </returns>
	public virtual bool trimStrings()
	{
		return _trim_strings;
	}

	// TODO SAM 2006-06-21
	// Need to check for delimiter in header and make this code consistent with
	// the RTi.Util.GUI.JWorksheet file saving code, or refactor to use the same code.
	/// <summary>
	/// Writes a table to a delimited file.  If the data items contain the delimiter,
	/// they will be written surrounded by double quotes. </summary>
	/// <param name="filename"> the file to write </param>
	/// <param name="delimiter"> the delimiter between columns </param>
	/// <param name="writeColumnNames"> If true, the field names will be read from the fields 
	/// and written as a one-line header of field names.  The headers are double-quoted.
	/// If all headers are missing, then the header line will not be written. </param>
	/// <param name="comments"> a list of Strings to put at the top of the file as comments, </param>
	/// <param name="commentLinePrefix"> prefix string for comment lines specify if incoming comment strings have not already been
	/// prefixed. </param>
	/// <param name="alwaysQuoteStrings"> if true, then always surround strings with double quotes; if false strings will only
	/// be quoted when they include the delimiter </param>
	/// <param name="newlineReplacement"> if not null, replace newlines in string table values with the replacement string
	/// (which can be an empty string).  This is needed to ensure that the delimited file does not include unexpected
	/// newlines in mid-row.  Checks are done for \r\n, then \n, then \r to catch all combinations.  This can be a
	/// performance hit and mask data issues so the default is to NOT replace newlines. </param>
	/// <param name="NaNValue"> value to replace NaN in output (a value of null will result in NaN being written). </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeDelimitedFile(String filename, String delimiter, boolean writeColumnNames, java.util.List<String> comments, String commentLinePrefix, boolean alwaysQuoteStrings, String newlineReplacement, String NaNValue) throws Exception
	public virtual void writeDelimitedFile(string filename, string delimiter, bool writeColumnNames, IList<string> comments, string commentLinePrefix, bool alwaysQuoteStrings, string newlineReplacement, string NaNValue)
	{
		string routine = this.GetType().Name + ".writeDelimitedFile";

		if (string.ReferenceEquals(filename, null))
		{
			Message.printWarning(1, routine, "Cannot write to file '" + filename + "'");
			throw new Exception("Cannot write to file '" + filename + "'");
		}
		if (comments == null)
		{
			comments = new List<string>(); // To simplify logic below
		}
		string commentLinePrefix2 = commentLinePrefix;
		if (!commentLinePrefix.Equals(""))
		{
			commentLinePrefix2 = commentLinePrefix + " "; // Add space for readability
		}
		if (string.ReferenceEquals(NaNValue, null))
		{
			NaNValue = "NaN";
		}

		PrintWriter @out = new PrintWriter(new StreamWriter(filename));
		int row = 0, col = 0;
		try
		{
			// If any comments have been passed in, print them at the top of the file
			if (comments != null && comments.Count > 0)
			{
				int size = comments.Count;
				for (int i = 0; i < size; i++)
				{
					@out.println(commentLinePrefix2 + comments[i]);
				}
			}

			int cols = getNumberOfFields();
			if (cols == 0)
			{
				Message.printWarning(3, routine, "Table has 0 columns!  Nothing will be written.");
				return;
			}

			StringBuilder line = new StringBuilder();

			int nonBlank = 0; // Number of non-blank table headings
			if (writeColumnNames)
			{
				// First determine if any headers are non blank
				for (col = 0; col < cols; col++)
				{
					if (getFieldName(col).Length > 0)
					{
						++nonBlank;
					}
				}
				if (nonBlank > 0)
				{
					line.Length = 0;
					for (col = 0; col < (cols - 1); col++)
					{
						line.Append("\"" + getFieldName(col) + "\"" + delimiter);
					}
					line.Append("\"" + getFieldName((cols - 1)) + "\"");
					@out.println(line);
				}
			}

			int rows = getNumberOfRecords();
			string cell;
			int tableFieldType;
			int precision;
			object fieldValue;
			double? fieldValueDouble;
			float? fieldValueFloat;
			bool doQuoteCell = false; // Whether a single cell should have surrounding quotes
			for (row = 0; row < rows; row++)
			{
				line.Length = 0;
				for (col = 0; col < cols; col++)
				{
					if (col > 0)
					{
						line.Append(delimiter);
					}
					tableFieldType = getFieldDataType(col);
					precision = getFieldPrecision(col);
					fieldValue = getFieldValue(row,col);
					if (fieldValue == null)
					{
						cell = "";
					}
					else if (tableFieldType == TableField.DATA_TYPE_FLOAT)
					{
						fieldValueFloat = (float?)fieldValue;
						if (fieldValueFloat.isNaN())
						{
							cell = NaNValue;
						}
						else if (precision > 0)
						{
							// Format according to the precision if floating point
							cell = StringUtil.formatString(fieldValue,"%." + precision + "f");
						}
						else
						{
							// Use default formatting.
							cell = "" + fieldValue;
						}
					}
					else if (tableFieldType == TableField.DATA_TYPE_DOUBLE)
					{
						fieldValueDouble = (double?)fieldValue;
						if (fieldValueDouble.isNaN())
						{
							cell = NaNValue;
						}
						else if (precision > 0)
						{
							// Format according to the precision if floating point
							cell = StringUtil.formatString(fieldValue,"%." + precision + "f");
						}
						else
						{
							// Use default formatting.
							cell = "" + fieldValue;
						}
					}
					else
					{
						// Use default formatting.
						cell = "" + fieldValue;
					}
					// Figure out if the cell needs to be quoted
					// Surround the values with double quotes if:
					// 1) the field contains the delimiter
					// 2) alwaysQuoteStrings=true
					// 3) the field contains a double quote (additionally replace " with "")
					doQuoteCell = false;
					if (tableFieldType == TableField.DATA_TYPE_STRING)
					{
						if (cell.IndexOf("\"", StringComparison.Ordinal) > -1)
						{
							// Cell includes a double quote so quote the whole thing
							doQuoteCell = true;
						}
						else if (alwaysQuoteStrings)
						{
							doQuoteCell = true;
						}
					}
					if ((cell.IndexOf(delimiter, StringComparison.Ordinal) > -1))
					{
						// Always have to protect delimiter character in the cell string
						doQuoteCell = true;
					}
					if (doQuoteCell)
					{
						// First replace all single \" instances with double
						cell = cell.Replace("\"", "\"\"");
						// Then add quotes around the whole thing
						cell = "\"" + cell + "\"";
					}
					if ((tableFieldType == TableField.DATA_TYPE_STRING) && (!string.ReferenceEquals(newlineReplacement, null)))
					{
						// Replace newline strings with the specified string
						cell = cell.Replace("\r\n", newlineReplacement); // Windows/Mac use 2-characters
						cell = cell.Replace("\n", newlineReplacement); // *NIX
						cell = cell.Replace("\r", newlineReplacement); // to be sure
					}
					line.Append(cell);
				}
				@out.println(line);
			}
		}
		catch (Exception e)
		{
			// Log and rethrow
			Message.printWarning(3, routine, "Unexpected error writing delimited file row [" + row + "][" + col + "] uniquetempvar.");
			Message.printWarning(3, routine, e);
			throw (e);
		}
		finally
		{
			@out.flush();
			@out.close();
		}
	}
	}

}