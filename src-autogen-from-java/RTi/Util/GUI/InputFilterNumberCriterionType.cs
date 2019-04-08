using System;
using System.Collections.Generic;

// InputFilterNumberCriterionType - enumeration of number conditions that can be checked for in an input filter

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

namespace RTi.Util.GUI
{
	/// <summary>
	/// Enumeration of number conditions that can be checked for in an input filter.
	/// @author sam
	/// 
	/// </summary>
	public sealed class InputFilterNumberCriterionType
	{

	/// <summary>
	/// Number is exactly equal to a value.
	/// </summary>
	public static readonly InputFilterNumberCriterionType EQUALS = new InputFilterNumberCriterionType("EQUALS", InnerEnum.EQUALS, "=");
	/// <summary>
	/// Number is greater than a value.
	/// </summary>
	public static readonly InputFilterNumberCriterionType GREATER_THAN = new InputFilterNumberCriterionType("GREATER_THAN", InnerEnum.GREATER_THAN, ">");
	/// <summary>
	/// Number is less than or equal to a value.
	/// </summary>
	public static readonly InputFilterNumberCriterionType GREATER_THAN_OR_EQUAL_TO = new InputFilterNumberCriterionType("GREATER_THAN_OR_EQUAL_TO", InnerEnum.GREATER_THAN_OR_EQUAL_TO, ">=");
	/// <summary>
	/// Number is less than a value.
	/// </summary>
	public static readonly InputFilterNumberCriterionType LESS_THAN = new InputFilterNumberCriterionType("LESS_THAN", InnerEnum.LESS_THAN, "<");
	/// <summary>
	/// Number is less than or equal to a value.
	/// </summary>
	public static readonly InputFilterNumberCriterionType LESS_THAN_OR_EQUAL_TO = new InputFilterNumberCriterionType("LESS_THAN_OR_EQUAL_TO", InnerEnum.LESS_THAN_OR_EQUAL_TO, "<=");

	private static readonly IList<InputFilterNumberCriterionType> valueList = new List<InputFilterNumberCriterionType>();

	static InputFilterNumberCriterionType()
	{
		valueList.Add(EQUALS);
		valueList.Add(GREATER_THAN);
		valueList.Add(GREATER_THAN_OR_EQUAL_TO);
		valueList.Add(LESS_THAN);
		valueList.Add(LESS_THAN_OR_EQUAL_TO);
	}

	public enum InnerEnum
	{
		EQUALS,
		GREATER_THAN,
		GREATER_THAN_OR_EQUAL_TO,
		LESS_THAN,
		LESS_THAN_OR_EQUAL_TO
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
	/// Construct a time series statistic enumeration value. </summary>
	/// <param name="displayName"> name that should be displayed in choices, etc. </param>
	private InputFilterNumberCriterionType(string name, InnerEnum innerEnum, string displayName)
	{
		this.displayName = displayName;

		nameValue = name;
		ordinalValue = nextOrdinal++;
		innerEnumValue = innerEnum;
	}

	/// <summary>
	/// Return the display name for the statistic.  This is usually the same as the
	/// value but using appropriate mixed case. </summary>
	/// <returns> the display name. </returns>
	public override string ToString()
	{
		return displayName;
	}

	/// <summary>
	/// Return the enumeration value given a string name (case-independent). </summary>
	/// <returns> the enumeration value given a string name (case-independent), or null if not matched. </returns>
	public static InputFilterNumberCriterionType valueOfIgnoreCase(string name)
	{
		// Legacy/alternate values
		if (name.Equals("Equals", StringComparison.OrdinalIgnoreCase))
		{
			return EQUALS;
		}
		else if (name.Equals("Greater than", StringComparison.OrdinalIgnoreCase))
		{
			return GREATER_THAN;
		}
		else if (name.Equals("Less than", StringComparison.OrdinalIgnoreCase))
		{
			return LESS_THAN;
		}
		InputFilterNumberCriterionType[] values = values();
		// Currently supported values
		foreach (InputFilterNumberCriterionType t in values)
		{
			if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				return t;
			}
		}
		return null;
	}


		public static IList<InputFilterNumberCriterionType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static InputFilterNumberCriterionType valueOf(string name)
		{
			foreach (InputFilterNumberCriterionType enumInstance in InputFilterNumberCriterionType.valueList)
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