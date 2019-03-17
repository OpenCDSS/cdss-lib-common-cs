using System;
using System.Collections.Generic;

// DataTableMathOperatorType - enumeration of simple math operators that can be performed on table cells

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
	/// Enumeration of simple math operators that can be performed on table cells.
	/// @author sam
	/// 
	/// </summary>
	public sealed class DataTableMathOperatorType
	{

	/// <summary>
	/// Add values.
	/// </summary>
	public static readonly DataTableMathOperatorType ADD = new DataTableMathOperatorType("ADD", InnerEnum.ADD, "+");
	/// <summary>
	/// Assign values.
	/// </summary>
	public static readonly DataTableMathOperatorType ASSIGN = new DataTableMathOperatorType("ASSIGN", InnerEnum.ASSIGN, "=");
	/// <summary>
	/// Divide values.
	/// </summary>
	public static readonly DataTableMathOperatorType DIVIDE = new DataTableMathOperatorType("DIVIDE", InnerEnum.DIVIDE, "/");
	/// <summary>
	/// Multiply values.
	/// </summary>
	public static readonly DataTableMathOperatorType MULTIPLY = new DataTableMathOperatorType("MULTIPLY", InnerEnum.MULTIPLY, "*");
	/// <summary>
	/// Subtract values.
	/// </summary>
	public static readonly DataTableMathOperatorType SUBTRACT = new DataTableMathOperatorType("SUBTRACT", InnerEnum.SUBTRACT, "-");
	/// <summary>
	/// TODO SAM 2013-08-26 Need to enable
	/// Convert to double.
	/// </summary>
	//TO_DOUBLE ( "ToDouble" ),
	/// <summary>
	/// Convert to integer.
	/// </summary>
	public static readonly DataTableMathOperatorType TO_INTEGER = new DataTableMathOperatorType("TO_INTEGER", InnerEnum.TO_INTEGER, "ToInteger");

	private static readonly IList<DataTableMathOperatorType> valueList = new List<DataTableMathOperatorType>();

	static DataTableMathOperatorType()
	{
		valueList.Add(ADD);
		valueList.Add(ASSIGN);
		valueList.Add(DIVIDE);
		valueList.Add(MULTIPLY);
		valueList.Add(SUBTRACT);
		valueList.Add(TO_INTEGER);
	}

	public enum InnerEnum
	{
		ADD,
		ASSIGN,
		DIVIDE,
		MULTIPLY,
		SUBTRACT,
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
	/// Construct an enumeration value from a string. </summary>
	/// <param name="displayName"> name that should be displayed in choices, etc. </param>
	private DataTableMathOperatorType(string name, InnerEnum innerEnum, string displayName)
	{
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
	public static DataTableMathOperatorType valueOfIgnoreCase(string name)
	{
		DataTableMathOperatorType[] values = values();
		// Currently supported values
		foreach (DataTableMathOperatorType t in values)
		{
			if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				return t;
			}
		}
		return null;
	}


		public static IList<DataTableMathOperatorType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static DataTableMathOperatorType valueOf(string name)
		{
			foreach (DataTableMathOperatorType enumInstance in DataTableMathOperatorType.valueList)
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