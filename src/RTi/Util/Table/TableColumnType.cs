using System;
using System.Collections.Generic;

// TableColumnType - enumeration of data table column types.

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
	/// Enumeration of data table column types.
	/// These eventually will replace the numerical values in the TableField class.
	/// </summary>
	public sealed class TableColumnType
	{

	/// <summary>
	/// 4-byte integer, Java Integer.
	/// </summary>
	public static readonly TableColumnType INT = new TableColumnType("INT", InnerEnum.INT, TableField.DATA_TYPE_INT,"Integer");
	/// <summary>
	/// 2-byte integer, Java Short.
	/// </summary>
	public static readonly TableColumnType SHORT = new TableColumnType("SHORT", InnerEnum.SHORT, TableField.DATA_TYPE_SHORT,"Short");
	/// <summary>
	/// 8-byte double, Java Double.
	/// </summary>
	public static readonly TableColumnType DOUBLE = new TableColumnType("DOUBLE", InnerEnum.DOUBLE, TableField.DATA_TYPE_DOUBLE,"Double");
	/// <summary>
	/// 4-byte float, Java Float.
	/// </summary>
	public static readonly TableColumnType FLOAT = new TableColumnType("FLOAT", InnerEnum.FLOAT, TableField.DATA_TYPE_FLOAT,"Float");
	/// <summary>
	/// Java String.
	/// </summary>
	public static readonly TableColumnType STRING = new TableColumnType("STRING", InnerEnum.STRING, TableField.DATA_TYPE_STRING,"String");
	/// <summary>
	/// Java date and optionally time.
	/// </summary>
	public static readonly TableColumnType DATE = new TableColumnType("DATE", InnerEnum.DATE, TableField.DATA_TYPE_DATE,"Date");
	/// <summary>
	/// 8-byte integer, Java Long.
	/// </summary>
	public static readonly TableColumnType LONG = new TableColumnType("LONG", InnerEnum.LONG, TableField.DATA_TYPE_LONG,"Long");
	/// <summary>
	/// DateTime.
	/// </summary>
	public static readonly TableColumnType DateTime = new TableColumnType("DateTime", InnerEnum.DateTime, TableField.DATA_TYPE_DATETIME,"DateTime");

	private static readonly IList<TableColumnType> valueList = new List<TableColumnType>();

	static TableColumnType()
	{
		valueList.Add(INT);
		valueList.Add(SHORT);
		valueList.Add(DOUBLE);
		valueList.Add(FLOAT);
		valueList.Add(STRING);
		valueList.Add(DATE);
		valueList.Add(LONG);
		valueList.Add(DateTime);
	}

	public enum InnerEnum
	{
		INT,
		SHORT,
		DOUBLE,
		FLOAT,
		STRING,
		DATE,
		LONG,
		DateTime
	}

	public readonly InnerEnum innerEnumValue;
	private readonly string nameValue;
	private readonly int ordinalValue;
	private static int nextOrdinal = 0;

	/// <summary>
	/// The name that should be displayed when used in UIs and reports.
	/// </summary>
	private readonly string displayName;

	/// <summary>
	/// The internal code for the enumeration, matches the TableField definitions to bridge legacy code.
	/// </summary>
	private readonly int code;

	/// <summary>
	/// Construct an enumeration value from a string. </summary>
	/// <param name="displayName"> name that should be displayed in choices, etc. </param>
	private TableColumnType(string name, InnerEnum innerEnum, int code, string displayName)
	{
		this.code = code;
		this.displayName = displayName;

		nameValue = name;
		ordinalValue = nextOrdinal++;
		innerEnumValue = innerEnum;
	}

	/// <summary>
	/// Return the display name for the math operator.  This is usually the same as the
	/// value but using appropriate mixed case. </summary>
	/// <returns> the display name. </returns>
	public override string ToString()
	{
		return displayName;
	}

	/// <summary>
	/// Return the enumeration value given a string name (case-independent). </summary>
	/// <returns> the enumeration value given a string name (case-independent), or null if not matched. </returns>
	public static TableColumnType valueOfIgnoreCase(string name)
	{
		if (string.ReferenceEquals(name, null))
		{
			return null;
		}
		TableColumnType[] values = values();
		// Special case
		if (name.Equals("DateTime", StringComparison.OrdinalIgnoreCase))
		{
			return DATE;
		}
		// Currently supported values
		foreach (TableColumnType t in values)
		{
			if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				return t;
			}
		}
		return null;
	}

	/// <summary>
	/// Return the enumeration value given a code value. </summary>
	/// <returns> the enumeration value given a code value, or null if not matched. </returns>
	public static TableColumnType valueOf(int code)
	{
		TableColumnType[] values = values();
		// Currently supported values
		foreach (TableColumnType t in values)
		{
			if (code == t.code)
			{
				return t;
			}
		}
		return null;
	}


		public static IList<TableColumnType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}
	}

}