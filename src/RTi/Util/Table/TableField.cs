using System;
using System.Collections.Generic;

// TableField - this class defines the fields (columns) in a table

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

	/// <summary>
	/// This class defines the fields (columns) in a table.  A DataTable is created
	/// by specifying a list of TableField objects to pass into the DataTable
	/// constructor.  Note that the field types have been implemented in a generic
	/// sense; however, for historical reasons, the table design somewhat mimics Dbase data tables.
	/// In Dbase files, it is somewhat ambiguous to know
	/// when a numeric field is a floating point or integer.  It can be assumed that
	/// a precision of zero for a numeric field indicates an integer.  However, at this
	/// time, the DATA_TYPE_DOUBLE and DATA_TYPE_STRING types are used nearly exclusively. </summary>
	/// <seealso cref= RTi.Util.Table.Table </seealso>
	/// <seealso cref= RTi.Util.Table.TableRecord </seealso>
	public class TableField
	{

	/// <summary>
	/// 4-byte integer.
	/// </summary>
	public const int DATA_TYPE_INT = 0;

	/// <summary>
	/// 2-byte integer.
	/// </summary>
	public const int DATA_TYPE_SHORT = 1;

	/// <summary>
	/// 8-byte double.
	/// </summary>
	public const int DATA_TYPE_DOUBLE = 2;

	/// <summary>
	/// 4-byte float.
	/// </summary>
	public const int DATA_TYPE_FLOAT = 3;

	/// <summary>
	/// 1-byte characters as string.
	/// </summary>
	public const int DATA_TYPE_STRING = 4;

	/// <summary>
	/// Date and time, stored internally as Java Date object (use when simple data are
	/// being manipulated).
	/// </summary>
	public const int DATA_TYPE_DATE = 5;

	/// <summary>
	/// 8-byte integer (long).
	/// </summary>
	public const int DATA_TYPE_LONG = 6;

	/// <summary>
	/// Date and time, stored internally as DateTime object (advantage is can use precision and
	/// other data to control object).
	/// </summary>
	public const int DATA_TYPE_DATETIME = 7;

	/// <summary>
	/// Boolean.
	/// </summary>
	public const int DATA_TYPE_BOOLEAN = 8;

	/// <summary>
	/// Used to indicate an array type of array.  See DATA_TYPE_ARRAY for more information.
	/// </summary>
	public const int DATA_TYPE_ARRAY_BASE = 1000;

	/// <summary>
	/// Array of other types, for compatibility with java.sql.Types.ARRAY.
	/// Subtract DATA_TYPE_ARRAY_BASE to get the type of objects in the array.
	/// </summary>
	public const int DATA_TYPE_ARRAY = DATA_TYPE_ARRAY_BASE;

	/// <summary>
	/// Data type (DATA_TYPE_*) for the field (column).
	/// If a simple data type the value corresponds to the data type.
	/// If a complex data type such as ARRAY, the data type within the complex type requires some math,
	/// which is handled by methods like isColumnArray().
	/// </summary>
	private int _data_type;

	/// <summary>
	/// Field name (also used for a column heading).
	/// </summary>
	private string _name;

	/// <summary>
	/// Field description, can be used for column tool tip when displaying table.
	/// </summary>
	private string __description = "";

	/// <summary>
	/// Units for the column, for example "mm".
	/// </summary>
	private string _units = "";

	// TODO SAM 2011-04-27 Why is this needed - is it related to dBase constraint?
	/// <summary>
	/// Field width (e.g., maximum characters for strings or number width in characters).
	/// </summary>
	private int _width;

	/// <summary>
	/// Precision applied to numbers (e.g., 3 in 11.3 numbers).
	/// </summary>
	private int _precision;

	/// <summary>
	/// Construct a new table of default type String.
	/// The precision defaults to 10 characters, precision 0.
	/// </summary>
	public TableField()
	{
		initialize(DATA_TYPE_STRING, "", 10, 0);
	}

	/// <summary>
	/// Construct a new table field for the specified type.
	/// The precision defaults to 10 characters, precision 0. </summary>
	/// <param name="type"> Type of data associated with a particular column within
	/// a DataTable.  Use TableField.DATA_TYPE_* </param>
	public TableField(int type)
	{
		initialize(type, "", 10, 0);
	}

	/// <summary>
	/// Copy constructor. </summary>
	/// <param name="field"> table field to copy </param>
	public TableField(TableField field)
	{
		initialize(field.getDataType(), field.getName(), field.getWidth(), field.getPrecision());
	}

	// TODO SAM 2011-12-25 Evaluate defaulting width to -1 for strings.
	/// <summary>
	/// Construct a new table field for the specified type and name.
	/// The width defaults to 10 characters, precision 0. </summary>
	/// <param name="type"> Type of data associated with a particular column within
	/// a DataTable.  Use TableField.DATA_TYPE_* </param>
	/// <param name="name"> Field name. </param>
	public TableField(int type, string name)
	{
		initialize(type, name, 10, 0);
	}

	/// <summary>
	/// Construct a new table field for the specified type and name.
	/// The precision defaults to zero (precision is only applicable to floating point data). </summary>
	/// <param name="type"> Type of data associated with a particular column within
	/// a DataTable.  Use TableField.DATA_TYPE_* </param>
	/// <param name="name"> Field name. </param>
	/// <param name="width"> Field width in characters (-1 is allowed for variable-length strings). </param>
	public TableField(int type, string name, int width)
	{
		initialize(type, name, width, 0);
	}

	/// <summary>
	/// Construct a new table field for the specified type and name. </summary>
	/// <param name="type"> Type of data associated with a particular column within
	/// a DataTable.  Use TableField.DATA_TYPE_* </param>
	/// <param name="name"> Field name. </param>
	/// <param name="width"> Field width in characters (-1 is allowed for variable-length strings). </param>
	/// <param name="precision"> Field precision in characters.  Used only for floating point data. </param>
	public TableField(int type, string name, int width, int precision)
	{
		initialize(type, name, width, precision);
	}

	/// <summary>
	/// Get type of data represented in this field. </summary>
	/// <returns> data type (DATA_TYPE_*) </returns>
	public virtual int getDataType()
	{
		return _data_type;
	}

	/// <summary>
	/// TODO SAM 2009-07-22 Need to use an enum type class for the types but need to refactor code.
	/// Get type of data represented in this field, as a String. </summary>
	/// <returns> data type as string (e.g., DATA_TYPE_INT = "integer") or null if unknown. </returns>
	public static string getDataTypeAsString(int dataType)
	{
		if (dataType == DATA_TYPE_DATE)
		{
			// Internally represented as a Java Date
			return "date";
		}
		else if (dataType == DATA_TYPE_DATETIME)
		{
			// Internally treated as DateTime object.
			return "datetime";
		}
		else if (dataType == DATA_TYPE_DOUBLE)
		{
			return "double";
		}
		else if (dataType == DATA_TYPE_FLOAT)
		{
			return "float";
		}
		else if (dataType == DATA_TYPE_INT)
		{
			return "integer";
		}
		else if (dataType == DATA_TYPE_LONG)
		{
			return "long";
		}
		else if (dataType == DATA_TYPE_SHORT)
		{
			return "short";
		}
		else if (dataType == DATA_TYPE_STRING)
		{
			return "string";
		}
		else
		{
			return null;
		}
	}

	/// <summary>
	/// TODO SAM 2009-04-22 Need to use enum construct for data types.
	/// Get the list of available data types, useful for displaying choices to users. </summary>
	/// <returns> a list of data type strings, suitable for choices for users. </returns>
	/// <param name="includeNote"> if true, include a note describing the data type using form "dataType - note". </param>
	public static IList<string> getDataTypeChoices(bool includeNote)
	{
		IList<string> dataTypeList = new List<string>();
		if (includeNote)
		{
			dataTypeList.Add("boolean - boolean (true/false)");
			dataTypeList.Add("datetime - date and time");
			dataTypeList.Add("double - double precision number");
			dataTypeList.Add("float - single precision number");
			dataTypeList.Add("integer - integer");
			dataTypeList.Add("long - long integer");
			dataTypeList.Add("short - short integer");
			dataTypeList.Add("string");
		}
		else
		{
			dataTypeList.Add("boolean");
			dataTypeList.Add("datetime");
			dataTypeList.Add("double");
			dataTypeList.Add("float");
			dataTypeList.Add("integer");
			dataTypeList.Add("long");
			dataTypeList.Add("short");
			dataTypeList.Add("string");
		}
		return dataTypeList;
	}

	/// <summary>
	/// Get field description. </summary>
	/// <returns> field description </returns>
	public virtual string getDescription()
	{
		return __description;
	}

	/// <summary>
	/// Get field name. </summary>
	/// <returns> field name. </returns>
	public virtual string getName()
	{
		return _name;
	}

	/// <summary>
	/// Get the field precision. </summary>
	/// <returns> field precision (digits after .). </returns>
	public virtual int getPrecision()
	{
		return _precision;
	}

	/// <summary>
	/// Get the units for the column. </summary>
	/// <returns> column units </returns>
	public virtual string getUnits()
	{
		return _units;
	}

	/// <summary>
	/// Get the field width. </summary>
	/// <returns> field width (overall character width). </returns>
	public virtual int getWidth()
	{
		return _width;
	}

	/// <summary>
	/// Initialize the instance. </summary>
	/// <param name="type"> Data type for field. </param>
	/// <param name="name"> Field name. </param>
	/// <param name="width"> field width for output (-1 is allowed for variable-length strings). </param>
	/// <param name="precision"> digits after decimal for numbers. </param>
	private void initialize(int type, string name, int width, int precision)
	{
		_width = width;
		_precision = precision;
		_data_type = type;
		_name = name;
	}

	/// <summary>
	/// TODO SAM 2009-07-22 Need to use an enum type class for the types and refactor code.
	/// Lookup the type of data represented in this field as an internal integer given the string type representation. </summary>
	/// <returns> data type as internal integer representation (e.g., DATA_TYPE_INT = "integer") or -1 if unknown. </returns>
	public static int lookupDataType(string dataType)
	{
		if (dataType.Equals("date", StringComparison.OrdinalIgnoreCase))
		{
			return DATA_TYPE_DATETIME;
		}
		else if (dataType.Equals("datetime", StringComparison.OrdinalIgnoreCase))
		{
			return DATA_TYPE_DATETIME;
		}
		else if (dataType.Equals("double", StringComparison.OrdinalIgnoreCase))
		{
			return DATA_TYPE_DOUBLE;
		}
		else if (dataType.Equals("float", StringComparison.OrdinalIgnoreCase))
		{
			return DATA_TYPE_FLOAT;
		}
		else if (dataType.Equals("int", StringComparison.OrdinalIgnoreCase) || dataType.Equals("integer", StringComparison.OrdinalIgnoreCase))
		{
			return DATA_TYPE_INT;
		}
		else if (dataType.Equals("long", StringComparison.OrdinalIgnoreCase))
		{
			return DATA_TYPE_LONG;
		}
		else if (dataType.Equals("short", StringComparison.OrdinalIgnoreCase))
		{
			return DATA_TYPE_SHORT;
		}
		else if (dataType.Equals("string", StringComparison.OrdinalIgnoreCase))
		{
			return DATA_TYPE_STRING;
		}
		else
		{
			return -1;
		}
	}

	/// <summary>
	/// Set the data type. </summary>
	/// <param name="data_type"> data type using DATA_TYPE_*. </param>
	public virtual void setDataType(int data_type)
	{
		_data_type = data_type;
	}

	/// <summary>
	/// Set the field description. </summary>
	/// <param name="description"> field description </param>
	public virtual void setDescription(string description)
	{
		if (!string.ReferenceEquals(description, null))
		{
			__description = description;
		}
	}

	/// <summary>
	/// Set the field name. </summary>
	/// <param name="name"> field name. </param>
	public virtual void setName(string name)
	{
		if (!string.ReferenceEquals(name, null))
		{
			_name = name;
		}
	}

	/// <summary>
	/// Set the field precision. </summary>
	/// <param name="precision"> field precision (characters). </param>
	public virtual void setPrecision(int precision)
	{
		_precision = precision;
	}

	/// <summary>
	/// Set the units for the column. </summary>
	/// <param name="units"> column units. </param>
	public virtual void setUnits(string units)
	{
		if (!string.ReferenceEquals(units, null))
		{
			_units = units;
		}
	}

	/// <summary>
	/// Set the field width. </summary>
	/// <param name="width"> precision field width (characters). </param>
	public virtual void setWidth(int width)
	{
		_width = width;
	}

	}

}