﻿using System;

// ResultSetToDataTableFactory - class to process an SQL ResultSet into a DataTable

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

namespace RTi.Util.Table
{

	using DMI = RTi.DMI.DMI;
	using DMIUtil = RTi.DMI.DMIUtil;
	using Message = RTi.Util.Message.Message;
	using DateTime = RTi.Util.Time.DateTime;

	/// <summary>
	/// This class will process an SQL ResultSet into a DataTable.
	/// </summary>
	public class ResultSetToDataTableFactory
	{

	/// <summary>
	/// Constructor.
	/// </summary>
	public ResultSetToDataTableFactory()
	{

	}

	/// <summary>
	/// Create a DataTable from a ResultSet. </summary>
	/// <param name="dbengineType"> a value from DMI.DBENGINE_*, used for fine-grain handling
	/// of column data type mapping.  Specify as -1 to ignore. </param>
	/// <param name="rs"> the ResultSet from an SQL query </param>
	/// <param name="tableID"> the identifier to use for the table </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public DataTable createDataTable(int dbengineType, java.sql.ResultSet rs, String tableID) throws java.sql.SQLException
	public virtual DataTable createDataTable(int dbengineType, ResultSet rs, string tableID)
	{
		string routine = this.GetType().Name + ".createDataTable";
		DataTable table = new DataTable();
		table.setTableID(tableID);
		// Define the table columns from the ResultSet metadata
		ResultSetMetaData meta = rs.getMetaData();
		int columnCount = meta.getColumnCount();
		string[] columnNames = new string[columnCount];
		int columnType, precision, scale;
		int[] columnTypes = new int[columnCount];
		for (int i = 1; i <= columnCount; i++)
		{
			columnNames[i - 1] = meta.getColumnName(i);
			columnType = sqlToDMIColumnType(dbengineType, meta.getColumnType(i));
			columnTypes[i - 1] = columnType;
			precision = meta.getPrecision(i); // More like width
			scale = meta.getScale(i); // Digits after decimal for floating point
			//Message.printStatus(2,routine,"Column name=\"" + columnName + "\", sqlColumnType=" + meta.getColumnType(i) +
			//    ", tableColumnType=" + columnType +
			//    ", SQL precision (table width)=" + precision + ", SQL scale (table precision)=" + scale +
			//    ", displayWidth=" + meta.getColumnDisplaySize(i));
			// TODO SAM 2012-06-12
			// SQL Server behaves oddly in that the "scale" can be set to 0 (zero) but SQL Server Management Studio
			// still shows digits after the decimal point.  None of the properties on a column appear to be usable
			// to determine the number of digits to display.  To compensate, for now set the scale to 6 if
			// floating point and not specified
			if (dbengineType == DMI.DBENGINE_SQLSERVER)
			{
				if (((columnType == TableField.DATA_TYPE_DOUBLE) || (columnType == TableField.DATA_TYPE_FLOAT)) && (scale == 0))
				{
					scale = 6;
				}
			}
			else if (dbengineType == DMI.DBENGINE_ORACLE)
			{
				// If the column type is NUMBER rather than NUMBER(5,0), for example, scale may be -127 and precision 0
				if (((columnType == TableField.DATA_TYPE_DOUBLE) || (columnType == TableField.DATA_TYPE_FLOAT)) && (scale == -127))
				{
					scale = 6;
					precision = -1; // Allow any width
				}
			}
			Message.printStatus(2, routine, "Adding column \"" + columnNames[i - 1] + "\" SQLType=" + meta.getColumnType(i) + " columnType=" + columnType + " width (metadata precision)=" + precision + ", precision (metadata scale) = " + scale);
			if (Message.isDebugOn)
			{
				Message.printDebug(1, routine, "Adding column \"" + columnNames[i - 1] + "\" SQLType=" + meta.getColumnType(i) + " columnType=" + columnType + " width (precision)=" + precision + ", precision (scale) = " + scale);
			}
			if (precision > 100000)
			{
				// Varchar and others may return large value, which causes problems (have seen with PostgreSQL)
				// See:  https://dev.mysql.com/doc/connector-j/en/connector-j-reference-type-conversions.html
				// Not sure what the magic number is but use 100000
				precision = -1;
			}
			table.addField(new TableField(columnType,columnNames[i - 1],precision,scale), null);
		}
		// Transfer each record in the ResultSet to the table
		string s;
		double d;
		float f;
		int i;
		long l;
		bool b;
		System.DateTime date;
		Array a;
		object arrayObject;
		int[] baseType = new int[columnCount]; // Used with Array.getBaseType(), the original SQL type
		int[] baseType2 = new int [columnCount]; // The internal type, after conversion from SQL type
		TableRecord rec = null;
		bool isNull;
		int recordCount = 0; // Expected record count
		int recordCountAdded = 0; // Records actually added
		while (rs.next())
		{
			++recordCount;
			rec = new TableRecord(columnCount);
			for (int iCol = 1; iCol <= columnCount; iCol++)
			{
				int i0 = iCol - 1;
				try
				{
					// If null is encountered, it is added at the end.
					isNull = false;
					if (columnTypes[i0] == TableField.DATA_TYPE_DATE)
					{
						date = rs.getTimestamp(iCol);
						if (!rs.wasNull())
						{
							DateTime dt = new DateTime(date);
							rec.addFieldValue(dt);
						}
						else
						{
							isNull = true;
						}
					}
					else if (columnTypes[i0] == TableField.DATA_TYPE_DOUBLE)
					{
						d = rs.getDouble(iCol);
						if (!rs.wasNull())
						{
							rec.addFieldValue(new double?(d));
						}
						else
						{
							isNull = true;
						}
					}
					else if (columnTypes[i0] == TableField.DATA_TYPE_FLOAT)
					{
						f = rs.getFloat(iCol);
						if (!rs.wasNull())
						{
							rec.addFieldValue(new float?(f));
						}
						else
						{
							isNull = true;
						}
					}
					else if (columnTypes[i0] == TableField.DATA_TYPE_INT)
					{
						i = rs.getInt(iCol);
						if (!rs.wasNull())
						{
							rec.addFieldValue(new int?(i));
						}
						else
						{
							isNull = true;
						}
					}
					else if (columnTypes[i0] == TableField.DATA_TYPE_LONG)
					{
						l = rs.getLong(iCol);
						if (!rs.wasNull())
						{
							rec.addFieldValue(new long?(l));
						}
						else
						{
							isNull = true;
						}
					}
					else if (columnTypes[i0] == TableField.DATA_TYPE_BOOLEAN)
					{
						b = rs.getBoolean(iCol);
						if (!rs.wasNull())
						{
							rec.addFieldValue(new bool?(b));
						}
						else
						{
							isNull = true;
						}
					}
					else if (columnTypes[i0] == TableField.DATA_TYPE_STRING)
					{
						s = rs.getString(iCol);
						if (!rs.wasNull())
						{
							rec.addFieldValue(s.Trim());
						}
						else
						{
							isNull = true;
						}
					}
					else if (table.isColumnArray(columnTypes[i0]))
					{
						// Column contains an array of other data, generally primitives
						// Set the array as the object without additional processing unless some translation is needed
						// Although downstream code can handle int[] and Integer[], normalize to primitives here if possible
						// Check the array content type for error-handling (letting unknown types through tends to cause problems later)
						a = rs.getArray(iCol);
						if (!rs.wasNull())
						{
							baseType[i0] = a.getBaseType();
							if (columnTypes[i0] == TableField.DATA_TYPE_ARRAY)
							{
								// The column type does not yet have the base type so add...
								baseType2[i0] = sqlToDMIColumnType(dbengineType,baseType[i0]);
								if (Message.isDebugOn)
								{
									Message.printDebug(1, routine, "Column \"" + columnNames[i0] + "\" is array. SQLType=" + baseType[i0] + " columnType=" + baseType2[i0]);
								}
								columnTypes[i0] = TableField.DATA_TYPE_ARRAY + baseType2[i0];
								// Have to set the column type back in the table because it was not set before
								table.setTableField(i0, columnTypes[i0], columnNames[i0]);
							}
							// Now need to interpret the base type...
							if (baseType[i0] == Types.DATE)
							{
								// Know that the array will contain Date
								System.DateTime[] da = (System.DateTime [])(a.getArray());
								// Convert to DateTime objects
								DateTime[] dta = new DateTime[da.Length];
								for (int ic = 0; ic < da.Length; ic++)
								{
									dta[ic] = new DateTime(da[ic]);
								}
								rec.addFieldValue(dta);
							}
							else if (baseType[i0] == Types.DOUBLE)
							{
								// Translate to double[] for handling elsewhere
								arrayObject = a.getArray();
								if (arrayObject is double[])
								{
									double[] da = (double [])arrayObject;
									rec.addFieldValue(da);
								}
								else if (arrayObject is double?[])
								{
									double?[] Da = (double? [])arrayObject;
									double[] da = new double[Da.Length];
									for (int i2 = 0; i2 < Da.Length; i2++)
									{
										if (Da[i2] == null)
										{
											da[i2] = DMIUtil.MISSING_DOUBLE;
										}
										else
										{
											da[i2] = Da[i2].Value;
										}
									}
									rec.addFieldValue(da);
								}
								else
								{
									isNull = true;
								}
							}
							else if (baseType[i0] == Types.FLOAT)
							{
								rec.addFieldValue(a.getArray());
							}
							else if (baseType[i0] == Types.INTEGER)
							{
								// Translate to int [] for handling elsewhere
								arrayObject = a.getArray();
								if (arrayObject is int[])
								{
									int[] ia = (int [])arrayObject;
									rec.addFieldValue(ia);
								}
								else if (arrayObject is int?[])
								{
									int?[] Ia = (int? [])arrayObject;
									int[] ia = new int[Ia.Length];
									for (int i2 = 0; i2 < Ia.Length; i2++)
									{
										if (Ia[i2] == null)
										{
											ia[i2] = DMIUtil.MISSING_INT;
										}
										else
										{
											ia[i2] = Ia[i2].Value;
										}
									}
									rec.addFieldValue(ia);
								}
								else
								{
									isNull = true;
								}
							}
							else if (baseType[i0] == Types.BIGINT)
							{
								// This handles Long/long
								rec.addFieldValue(a.getArray());
							}
							else if ((baseType[i0] == Types.CHAR) || (baseType[i0] == Types.VARCHAR) || (baseType[i0] == Types.NVARCHAR))
							{
								string[] sa = (string [])(a.getArray());
								rec.addFieldValue(sa);
							}
							else
							{
								// Don't know the type
								// TODO SAM 2015-09-06 Need to confirm handling of the above baseType
								// - evaluate whether to throw an exception
								Message.printWarning(3,routine,"Don't know how to handle Java SQL array type " + baseType + " setting value to null.");
								isNull = true;
							}
						}
						else
						{
							isNull = true;
						}
					}
					else
					{
						// Default is string
						s = rs.getString(iCol);
						if (!rs.wasNull())
						{
							rec.addFieldValue(s.Trim());
						}
						else
						{
							isNull = true;
						}
					}
					if (isNull)
					{
						// Field was not yet added but should be added as null
						rec.addFieldValue(null);
					}
				}
				catch (Exception e)
				{
					// Leave as null but print a message to help figure out issue
					Message.printWarning(3,routine,"Error processing column[" + (iCol - 1) + "] \"" + columnNames[iCol - 1] + "\"");
					Message.printWarning(3,routine,e);
				}
			}
			try
			{
				table.addRecord(rec);
			}
			catch (Exception e)
			{
				// Should not happen
				Message.printWarning(3,routine,e);
			}
			++recordCountAdded;
		}
		Message.printStatus(2, routine, "Processed " + recordCount + " records from resultset into table rows.");
		// If some records were not processed, through an exception so that it does not seem like all is OK
		if (recordCountAdded != recordCount)
		{
			throw new SQLException("Number of records in resultset=" + recordCount + " but " + recordCountAdded + " were processed.  Check log.");
		}
		return table;
	}

	/// <summary>
	/// Lookup the SQL column type to the DataTable type. </summary>
	/// <param name="sqlColumnType"> SQL column type from Types </param>
	/// <returns> DataTable column type from TableField </returns>
	private int sqlToDMIColumnType(int dbengineType, int sqlColumnType)
	{
		switch (sqlColumnType)
		{
			case Types.ARRAY:
				return TableField.DATA_TYPE_ARRAY;
			case Types.BIGINT:
				return TableField.DATA_TYPE_LONG;
			// BINARY not handled
			case Types.BIT:
				if (dbengineType == DMI.DBENGINE_POSTGRESQL)
				{
					// Database can have t or f as boolean
					return TableField.DATA_TYPE_BOOLEAN;
				}
				else
				{
					// 0 or 1
					return TableField.DATA_TYPE_INT;
				}
			// BLOB not handled
			case Types.BOOLEAN:
				return TableField.DATA_TYPE_BOOLEAN;
			case Types.CHAR:
				return TableField.DATA_TYPE_STRING;
			// CLOB not handled
			case Types.DATE:
				return TableField.DATA_TYPE_DATE;
			case Types.DECIMAL:
				return TableField.DATA_TYPE_DOUBLE;
			case Types.DOUBLE:
				return TableField.DATA_TYPE_DOUBLE;
			case Types.FLOAT:
				return TableField.DATA_TYPE_FLOAT;
			case Types.INTEGER:
				return TableField.DATA_TYPE_INT;
			case Types.LONGVARCHAR:
				return TableField.DATA_TYPE_STRING;
			case Types.NVARCHAR:
				return TableField.DATA_TYPE_STRING;
			case Types.NUMERIC:
				return TableField.DATA_TYPE_DOUBLE; // internally a BigDecimal - check the decimals to evaluate whether to use an integer
			case Types.REAL:
				return TableField.DATA_TYPE_DOUBLE;
			// REF not handled
			case Types.SMALLINT:
				return TableField.DATA_TYPE_INT;
			// STRUCT not handled
			case Types.TIME:
				return TableField.DATA_TYPE_DATE;
			case Types.TIMESTAMP:
				return TableField.DATA_TYPE_DATE;
			case Types.TINYINT:
				return TableField.DATA_TYPE_INT;
			case Types.VARCHAR:
				return TableField.DATA_TYPE_STRING;
			// VERBINARY not handled
			default:
				Message.printWarning(2,"sqlToDMIColumnType", "Unknown SQL type for conversion to table: " + sqlColumnType + ", using string.");
				return TableField.DATA_TYPE_STRING;
		}
	}

	}

}