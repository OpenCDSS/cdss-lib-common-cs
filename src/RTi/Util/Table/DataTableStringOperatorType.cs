using System;
using System.Collections.Generic;

// DataTableStringOperatorType - enumeration of simple string operators that can be performed on table cells

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
	/// Enumeration of simple string operators that can be performed on table cells.
	/// </summary>
	public sealed class DataTableStringOperatorType
	{

	/*
	Append string values.
	*/
	public static readonly DataTableStringOperatorType APPEND = new DataTableStringOperatorType("APPEND", InnerEnum.APPEND, "Append");
	/*
	Prepend string values.
	*/
	public static readonly DataTableStringOperatorType PREPEND = new DataTableStringOperatorType("PREPEND", InnerEnum.PREPEND, "Prepend");
	/*
	Replace string substring.
	*/
	public static readonly DataTableStringOperatorType REPLACE = new DataTableStringOperatorType("REPLACE", InnerEnum.REPLACE, "Replace");
	/*
	Remove string substring.
	*/
	public static readonly DataTableStringOperatorType REMOVE = new DataTableStringOperatorType("REMOVE", InnerEnum.REMOVE, "Remove");
	/*
	Return a token split from the string based on a delimiter.
	*/
	public static readonly DataTableStringOperatorType SPLIT = new DataTableStringOperatorType("SPLIT", InnerEnum.SPLIT, "Split");
	/*
	Return a substring.
	*/
	public static readonly DataTableStringOperatorType SUBSTRING = new DataTableStringOperatorType("SUBSTRING", InnerEnum.SUBSTRING, "Substring");
	/*
	Cast a string value to a boolean.
	*/
	// TODO SAM 2015-04-29 Need to enable Boolean
	//TO_BOOLEAN("ToBoolean"),
	/*
	Cast a string value to a date.
	*/
	public static readonly DataTableStringOperatorType TO_DATE = new DataTableStringOperatorType("TO_DATE", InnerEnum.TO_DATE, "ToDate");
	/*
	Cast a string value to a date/time.
	*/
	public static readonly DataTableStringOperatorType TO_DATE_TIME = new DataTableStringOperatorType("TO_DATE_TIME", InnerEnum.TO_DATE_TIME, "ToDateTime");
	/*
	Cast a string value to a double.
	*/
	public static readonly DataTableStringOperatorType TO_DOUBLE = new DataTableStringOperatorType("TO_DOUBLE", InnerEnum.TO_DOUBLE, "ToDouble");
	/*
	Cast a string value to an integer.
	*/
	public static readonly DataTableStringOperatorType TO_INTEGER = new DataTableStringOperatorType("TO_INTEGER", InnerEnum.TO_INTEGER, "ToInteger");

	private static readonly IList<DataTableStringOperatorType> valueList = new List<DataTableStringOperatorType>();

	static DataTableStringOperatorType()
	{
		valueList.Add(APPEND);
		valueList.Add(PREPEND);
		valueList.Add(REPLACE);
		valueList.Add(REMOVE);
		valueList.Add(SPLIT);
		valueList.Add(SUBSTRING);
		valueList.Add(TO_DATE);
		valueList.Add(TO_DATE_TIME);
		valueList.Add(TO_DOUBLE);
		valueList.Add(TO_INTEGER);
	}

	public enum InnerEnum
	{
		APPEND,
		PREPEND,
		REPLACE,
		REMOVE,
		SPLIT,
		SUBSTRING,
		TO_DATE,
		TO_DATE_TIME,
		TO_DOUBLE,
		TO_INTEGER
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
	/// Construct an enumeration value. </summary>
	/// <param name="displayName"> name that should be displayed in choices, etc. </param>
	private DataTableStringOperatorType(string name, InnerEnum innerEnum, string displayName)
	{
		this.displayName = displayName;

		nameValue = name;
		ordinalValue = nextOrdinal++;
		innerEnumValue = innerEnum;
	}

	/// <summary>
	/// Return the display name for the string operator.  This is usually the same as the
	/// value but using appropriate mixed case. </summary>
	/// <returns> the display name. </returns>
	public override string ToString()
	{
		return displayName;
	}

	/// <summary>
	/// Return the enumeration value given a string name (case-independent). </summary>
	/// <returns> the enumeration value given a string name (case-independent), or null if not matched. </returns>
	public static DataTableStringOperatorType valueOfIgnoreCase(string name)
	{
		if (string.ReferenceEquals(name, null))
		{
			return null;
		}
		DataTableStringOperatorType[] values = values();
		// Currently supported values
		foreach (DataTableStringOperatorType t in values)
		{
			if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				return t;
			}
		}
		return null;
	}


		public static IList<DataTableStringOperatorType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static DataTableStringOperatorType valueOf(string name)
		{
			foreach (DataTableStringOperatorType enumInstance in DataTableStringOperatorType.valueList)
			{
				if (enumInstance.nameValue == name)
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException(name);
		}
	}

}