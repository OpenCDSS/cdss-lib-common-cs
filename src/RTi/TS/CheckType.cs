using System;
using System.Collections.Generic;

// CheckType - data check types, typically used in analysis code that checks time series data or statistic values against some criteria

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

namespace RTi.TS
{
	/// <summary>
	/// Data check types, typically used in analysis code that checks time series data or statistic
	/// values against some criteria.  Not all of these checks may be appropriate for code that uses the enumeration and
	/// therefore a subset should be created as appropriate (e.g., repeating values apply to time series data but
	/// probably not statistics computed from the data).
	/// </summary>
	public sealed class CheckType
	{
	/// <summary>
	/// Absolute change from one value to the next > (in data units).
	/// </summary>
	public static readonly CheckType ABS_CHANGE_GREATER_THAN = new CheckType("ABS_CHANGE_GREATER_THAN", InnerEnum.ABS_CHANGE_GREATER_THAN, "AbsChange>");
	/// <summary>
	/// Absolute change from one value to the next > (percent).
	/// </summary>
	public static readonly CheckType ABS_CHANGE_PERCENT_GREATER_THAN = new CheckType("ABS_CHANGE_PERCENT_GREATER_THAN", InnerEnum.ABS_CHANGE_PERCENT_GREATER_THAN, "AbsChangePercent>");
	/// <summary>
	/// Change from one value to the next > (in data units).
	/// </summary>
	public static readonly CheckType CHANGE_GREATER_THAN = new CheckType("CHANGE_GREATER_THAN", InnerEnum.CHANGE_GREATER_THAN, "Change>");
	/// <summary>
	/// Change from one value to the next < (in data units).
	/// </summary>
	public static readonly CheckType CHANGE_LESS_THAN = new CheckType("CHANGE_LESS_THAN", InnerEnum.CHANGE_LESS_THAN, "Change<");
	/// <summary>
	/// Change in range of values (in data units).
	/// </summary>
	public static readonly CheckType IN_RANGE = new CheckType("IN_RANGE", InnerEnum.IN_RANGE, "InRange");
	/// <summary>
	/// Change out of range of values (in data units).
	/// </summary>
	public static readonly CheckType OUT_OF_RANGE = new CheckType("OUT_OF_RANGE", InnerEnum.OUT_OF_RANGE, "OutOfRange");
	/// <summary>
	/// Is value missing.
	/// </summary>
	public static readonly CheckType MISSING = new CheckType("MISSING", InnerEnum.MISSING, "Missing");
	/// <summary>
	/// Does value repeat.
	/// </summary>
	public static readonly CheckType REPEAT = new CheckType("REPEAT", InnerEnum.REPEAT, "Repeat");
	/// <summary>
	/// Is value less than.
	/// </summary>
	public static readonly CheckType LESS_THAN = new CheckType("LESS_THAN", InnerEnum.LESS_THAN, "<");
	/// <summary>
	/// Is value less than or equal to.
	/// </summary>
	public static readonly CheckType LESS_THAN_OR_EQUAL_TO = new CheckType("LESS_THAN_OR_EQUAL_TO", InnerEnum.LESS_THAN_OR_EQUAL_TO, "<=");
	/// <summary>
	/// Is value greater than.
	/// </summary>
	public static readonly CheckType GREATER_THAN = new CheckType("GREATER_THAN", InnerEnum.GREATER_THAN, ">");
	/// <summary>
	/// Is value greater than or equal to.
	/// </summary>
	public static readonly CheckType GREATER_THAN_OR_EQUAL_TO = new CheckType("GREATER_THAN_OR_EQUAL_TO", InnerEnum.GREATER_THAN_OR_EQUAL_TO, ">=");
	/// <summary>
	/// Is value equal to.
	/// </summary>
	public static readonly CheckType EQUAL_TO = new CheckType("EQUAL_TO", InnerEnum.EQUAL_TO, "==");

	private static readonly IList<CheckType> valueList = new List<CheckType>();

	static CheckType()
	{
		valueList.Add(ABS_CHANGE_GREATER_THAN);
		valueList.Add(ABS_CHANGE_PERCENT_GREATER_THAN);
		valueList.Add(CHANGE_GREATER_THAN);
		valueList.Add(CHANGE_LESS_THAN);
		valueList.Add(IN_RANGE);
		valueList.Add(OUT_OF_RANGE);
		valueList.Add(MISSING);
		valueList.Add(REPEAT);
		valueList.Add(LESS_THAN);
		valueList.Add(LESS_THAN_OR_EQUAL_TO);
		valueList.Add(GREATER_THAN);
		valueList.Add(GREATER_THAN_OR_EQUAL_TO);
		valueList.Add(EQUAL_TO);
	}

	public enum InnerEnum
	{
		ABS_CHANGE_GREATER_THAN,
		ABS_CHANGE_PERCENT_GREATER_THAN,
		CHANGE_GREATER_THAN,
		CHANGE_LESS_THAN,
		IN_RANGE,
		OUT_OF_RANGE,
		MISSING,
		REPEAT,
		LESS_THAN,
		LESS_THAN_OR_EQUAL_TO,
		GREATER_THAN,
		GREATER_THAN_OR_EQUAL_TO,
		EQUAL_TO
	}

	public readonly InnerEnum innerEnumValue;
	private readonly string nameValue;
	private readonly int ordinalValue;
	private static int nextOrdinal = 0;

	/// <summary>
	/// The name that should be displayed when the best fit type is used in UIs and reports.
	/// </summary>
	private readonly string displayName;

	/// <summary>
	/// Construct a time series statistic enumeration value. </summary>
	/// <param name="displayName"> name that should be displayed in choices, etc. </param>
	private CheckType(string name, InnerEnum innerEnum, string displayName)
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
	public static CheckType valueOfIgnoreCase(string name)
	{
		CheckType[] values = values();
		foreach (CheckType t in values)
		{
			if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				return t;
			}
		}
		return null;
	}


		public static IList<CheckType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static CheckType valueOf(string name)
		{
			foreach (CheckType enumInstance in CheckType.valueList)
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