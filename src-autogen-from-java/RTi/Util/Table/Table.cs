using System;
using System.Collections.Generic;
using System.IO;

// DataTable - coordinating class for the Table utility

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
// DataTable - coordinating class for the Table utility
// ----------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
// ----------------------------------------------------------------------------
// History:
//
// 23 Jun 1999	Catherine E.
//		Nutting-Lane, RTi	Initial version
// 2001-09-17	Steven A. Malers, RTi	Change the name of the class from Table
//					to DataTable to avoid conflict with the
//					C++ Table class.  Review code but don't
//					do much cleanup since the new DataTable
//					class should now be getting used.  Do
//					change to not use deprecated methods in
//					other table related classes.
// 2005-04-26	J. Thomas Sapienza, RTi	Added finalize().
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.Util.Table
{


	using Message = RTi.Util.Message.Message;

	using StringUtil = RTi.Util.String.StringUtil;


	/// <summary>
	/// This class manages the table group functionality. 
	/// Tables can be used to store record-based data.
	/// An example of its is as follows:
	/// <para>
	/// 
	/// <pre>
	/// try {
	/// // first, create table definition by assembling vector of 
	/// // TableField objects
	/// Vector myTableFields = new Vector(3);
	/// TableField firstTableField = new TableField ( 
	///		TableField.DATA_TYPE_STRING, "id_label_6" );
	/// TableField secondTableField = new TableField ( 
	///		TableField.DATA_TYPE_INT, "labelLength" );
	/// TableField thirdTableField = new TableField ( 
	///		TableField.DATA_TYPE_STRING, "aka" );
	/// myTableFields.addElement ( firstTableField );
	/// myTableFields.addElement ( secondTableField );
	/// myTableFields.addElement ( thirdTableField );
	/// 
	/// // now define table with one simple call
	/// Table myTable = new Table ( myTableFields );
	/// 
	/// TableRecord contents = new TableRecord (3);
	/// contents.addFieldValue ( "123456" );
	/// contents.addFieldValue ( new Integer (6));
	/// contents.addFieldValue ( "RTi station" );
	/// 
	/// myTable.addRecord ( contents );
	/// 
	/// system.out.println ( myTable.getFieldValue ( 0, 2 ));
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
	/// @deprecated Use DataTable. 
	public class Table
	{

	private System.Collections.IList _table_fields; // vector of data types - DATA_TYPE_*
	private System.Collections.IList _table_records; // vector of records

	/// <summary>
	/// Construct a new table. </summary>
	/// <param name="tableFieldsVector"> a vector of TableField objects defining table contents </param>
	public Table(System.Collections.IList tableFieldsVector)
	{
		initialize(tableFieldsVector);
	}

	private void initialize(System.Collections.IList tableFieldsVector)
	{
		_table_fields = tableFieldsVector;
		_table_records = new List<object>(10, 10);
	}

	/// <summary>
	/// Adds a record to the vector of TableRecords. </summary>
	/// <param name="new_record"> new record to be added </param>
	/// <exception cref="when"> the number of entries in new_record is not equal to the number of entries in the current TableField declaration </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void addRecord(TableRecord new_record) throws Exception
	public virtual void addRecord(TableRecord new_record)
	{
		int num_table_fields = _table_fields.Count;
		int num_new_record_fields = new_record.getNumberOfFields();
		if (num_new_record_fields == num_table_fields)
		{
			_table_records.Add(new_record);
		}
		else
		{
			throw new Exception("Number of records in the new record (" + num_new_record_fields + ") does not match current " + "description of the table fields.");
		}
	}

	/// <summary>
	/// Add a field to the TableField and each entry in TableRecord </summary>
	/// <param name="tableField"> type of data the new field will contain </param>
	public virtual void addField(TableField tableField)
	{
		_table_fields.Add(tableField);

		// add field to each record
		int num = _table_records.Count;
		TableRecord tableRecord;
		for (int i = 0; i < num; i++)
		{
			tableRecord = (TableRecord)_table_records[i];

			// add element and set default to 0 or ""
			// these are ordered in the most likely types to optimize
			int data_type = tableField.getDataType();
			if (data_type == TableField.DATA_TYPE_STRING)
			{
				tableRecord.addFieldValue("");
			}
			else if (data_type == TableField.DATA_TYPE_INT)
			{
				tableRecord.addFieldValue(new int?(0));
			}
			else if (data_type == TableField.DATA_TYPE_DOUBLE)
			{
				tableRecord.addFieldValue(new double?(0));
			}
			else if (data_type == TableField.DATA_TYPE_SHORT)
			{
				tableRecord.addFieldValue(Convert.ToInt16("0"));
			}
			else if (data_type == TableField.DATA_TYPE_FLOAT)
			{
				tableRecord.addFieldValue(new float?(0));
			}
		}
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~Table()
	{
		_table_fields = null;
		_table_records = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns a field index. </summary>
	/// <returns> Index of table entry associated with the given heading. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int getFieldIndex(String heading) throws Exception
	public virtual int getFieldIndex(string heading)
	{
		int num = _table_fields.Count;
		for (int i = 0; i < num; i++)
		{
			if (((TableField)_table_fields[i]).getName().Equals(heading, StringComparison.OrdinalIgnoreCase))
			{
				return i;
			}
		}

		// if this line is reached, the given heading was never found
		throw new Exception("Unable to find field with heading \"" + heading + "\"");
	}

	/// <summary>
	/// Returns the number of fields in the table. </summary>
	/// <returns> number of fields this table is current representing </returns>
	public virtual int getNumberOfFields()
	{
		return _table_fields.Count;
	}

	/// <summary>
	/// Returns the number of records within this table. </summary>
	/// <returns> number of records within this table </returns>
	public virtual int getNumberOfRecords()
	{
		return _table_records.Count;
	}

	/// <summary>
	/// Returns the TableFeld object for the specified zero-based index. </summary>
	/// <returns> TableField object for the specified zero-based index. </returns>
	public virtual TableField getTableField(int index)
	{
		return ((TableField)_table_fields[index]);
	}

	/// <summary>
	/// Returns a field value. </summary>
	/// <param name="record_index"> zero-based index of record </param>
	/// <param name="heading"> title of desired heading </param>
	/// <returns> field value for the specified heading of the specified record index
	/// Returned object must be properly cast. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object getFieldValue(int record_index, String heading) throws Exception
	public virtual object getFieldValue(int record_index, string heading)
	{
		return getFieldValue(record_index, getFieldIndex(heading));
	}

	/// <summary>
	/// Returns a field value. </summary>
	/// <param name="record_index"> zero-based index of record </param>
	/// <param name="field_index"> zero_based index of desired field </param>
	/// <returns> field value for the specified index of the specified record index
	/// Returned object must be properly cast. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object getFieldValue(int record_index, int field_index) throws Exception
	public virtual object getFieldValue(int record_index, int field_index)
	{
		string rtn = "getFieldValue";
		int num_recs = _table_records.Count;
		int num_fields = _table_fields.Count;

		if (num_recs <= record_index)
		{
			throw new Exception("Requested record index " + record_index + " is not available (only " + num_recs + " have been established.");
		}

		if (num_fields <= field_index)
		{
			throw new Exception("Requested field index " + field_index + " is not available (only " + num_fields + " have been established.");
		}

		/* break this up ...
		return (((TableRecord)_table_records.elementAt(record_index)).
			getFieldValue(field_index));
		*/
		Message.printStatus(10, rtn, "Getting table record " + record_index + " from " + num_recs + " available records.");
		TableRecord tableRecord = (TableRecord)_table_records[record_index];
		Message.printStatus(10, rtn, "Getting table record field.");
		return tableRecord.getFieldValue(field_index);
	}

	/// <summary>
	/// Returns the header title for an index. </summary>
	/// <returns> heading title for specified zero-based index. </returns>
	public virtual string getHeadingForIndex(int index)
	{
		return ((TableField)_table_fields[index]).getName();
	}

	/// <summary>
	/// Returns the table record at a specified index. </summary>
	/// <returns> TableRecord at specified record_index </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TableRecord getRecord(int record_index) throws Exception
	public virtual TableRecord getRecord(int record_index)
	{
		 if (_table_records.Count <= record_index)
		 {
			 throw new Exception("Unable to return TableRecord at index " + record_index);
		 }
		 return ((TableRecord)_table_records[record_index]);
	}

	/// <summary>
	/// Returns all the table records. </summary>
	/// <returns> vector of TableRecord </returns>
	public virtual System.Collections.IList getTableRecords()
	{
		return _table_records;
	}

	/// <summary>
	/// Set table header for the specified zero-based index. </summary>
	/// <param name="index"> index of field to set </param>
	/// <param name="header"> header of the field </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setTableFieldHeader(int index, String header) throws Exception
	public virtual void setTableFieldHeader(int index, string header)
	{
		if (_table_fields.Count <= index)
		{
			throw new Exception("Index " + index + " is not valid.");
		}
		TableField tableField = (TableField)_table_fields[index];
		tableField.setName(header);
	}

	/// <summary>
	/// Set field data type for the specified zero-based index. </summary>
	/// <param name="index"> index of field to set </param>
	/// <param name="data_type"> data type; use FieldType.DATA_TYPE_* </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setTableFieldType(int index, int data_type) throws Exception
	public virtual void setTableFieldType(int index, int data_type)
	{
		if (_table_fields.Count <= index)
		{
			throw new Exception("Index " + index + " is not valid.");
		}
		TableField tableField = (TableField)_table_fields[index];
		tableField.setDataType(data_type);
	}

	/// <summary>
	/// Set field data type and header for the specified zero-based index. </summary>
	/// <param name="index"> index of field to set </param>
	/// <param name="data_type"> data type; use FieldType.DATA_TYPE_* </param>
	/// <param name="header"> header of the field </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setTableField(int index, int data_type, String header) throws Exception
	public virtual void setTableField(int index, int data_type, string header)
	{
		if (_table_fields.Count <= index)
		{
			throw new Exception("Index " + index + " is not valid.");
		}
		TableField tableField = (TableField)_table_fields[index];
		tableField.setDataType(data_type);
		tableField.setName(header);
	}

	/// <summary>
	/// Given a clear definition of what data to expect, reads and stores data in table </summary>
	/// <returns> new table containing data </returns>
	/// <param name="filename"> name of file containing delimited data </param>
	/// <param name="delimiter"> string representing delimiter in data file </param>
	/// <param name="tableFields"> vector of TableField objects defining data expectations </param>
	/// <param name="num_lines_header"> number of lines in header (typically 1) </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static Table parseDelimitedFile(String filename, String delimiter, java.util.List tableFields, int num_lines_header) throws Exception
	public static Table parseDelimitedFile(string filename, string delimiter, System.Collections.IList tableFields, int num_lines_header)
	{
		string rtn = "Table.parseDelimitedFile";
		string iline;
		bool processed_header = false;
		System.Collections.IList columns;
		int num_fields = 0, type, num_lines_header_read = 0;
		Table table;

		StreamReader @in = new StreamReader(filename);

		table = new Table(tableFields);
		if (num_lines_header == 0)
		{
			processed_header = true;
		}

		while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
		{

			// check if read comment or empty line
			if (iline.StartsWith("#", StringComparison.Ordinal) || iline.Trim().Length == 0)
			{
				continue;
			}

			columns = StringUtil.breakStringList(iline, delimiter, StringUtil.DELIM_SKIP_BLANKS);

			// line is part of header ... 
			if (!processed_header)
			{
				num_fields = columns.Count;
				if (num_fields < tableFields.Count)
				{
					throw new IOException("table fields specifications do not " + "match data found in file.");
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
					type = ((TableField)tableFields[i]).getDataType();
					if (type == TableField.DATA_TYPE_STRING)
					{
						contents.addFieldValue((string)columns[i]);
						/*
						currentString = 
							(String)columns.elementAt(i);
						// strip any double quotes
						modifiedString = currentString.replace
							('\"', ' ' );
						contents.addField ( modifiedString );
						*/
					}
					else if (type == TableField.DATA_TYPE_DOUBLE)
					{
						contents.addFieldValue(Convert.ToDouble((string)columns[i]));
					}
					else if (type == TableField.DATA_TYPE_INT)
					{
						contents.addFieldValue(Convert.ToInt32((string)columns[i]));
					}
					else if (type == TableField.DATA_TYPE_SHORT)
					{
						contents.addFieldValue(Convert.ToInt16((string)columns[i]));
					}
					else if (type == TableField.DATA_TYPE_FLOAT)
					{
						contents.addFieldValue(Convert.ToSingle((string)columns[i]));
					}
				}
				table.addRecord(contents);
				}
				catch (Exception e)
				{
					Message.printWarning(2, rtn, e);
				}
			}
		}
		return table;
	}

	/// <summary>
	/// Reads header of delimited file and return vector of TableField objects </summary>
	/// <returns> vector of TableField objects (only header titles will be set) </returns>
	/// <param name="filename"> name of file containing delimited data </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List parseDelimitedFileHeader(String filename) throws Exception
	public static System.Collections.IList parseDelimitedFileHeader(string filename)
	{
		return parseDelimitedFileHeader(filename, ",");
	}

	/// <summary>
	/// Reads header of delimited file and return vector of TableField objects.  The
	/// heading titles will be correctly returned.  The data type, however, will be set
	/// to TableField.DATA_TYPE_STRING.  This should be changed if not appropriate. </summary>
	/// <returns> vector of TableField objects (heading titles will be correctly set but data type will be string) </returns>
	/// <param name="filename"> name of file containing delimited data </param>
	/// <param name="delimiter"> string representing delimiter in data file  </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List parseDelimitedFileHeader(String filename, String delimiter) throws Exception
	public static System.Collections.IList parseDelimitedFileHeader(string filename, string delimiter)
	{
		string iline;
		System.Collections.IList columns, tableFields = null;
		int num_fields = 0;
		TableField newTableField = null;

		StreamReader @in = new StreamReader(filename);

		while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
		{

			// check if read comment or empty line
			if (iline.StartsWith("#", StringComparison.Ordinal) || iline.Trim().Length == 0)
			{
				continue;
			}

			columns = StringUtil.breakStringList(iline, delimiter, StringUtil.DELIM_SKIP_BLANKS);

			num_fields = columns.Count;
			tableFields = new List<object>(num_fields, 1);
			for (int i = 0; i < num_fields; i++)
			{
				newTableField = new TableField();
				newTableField.setName((string)columns[i]);
				newTableField.setDataType(TableField.DATA_TYPE_STRING);
				tableFields.Add(newTableField);
			}
			return tableFields;
		}
		return tableFields;
	}

	} // End of Table class

}