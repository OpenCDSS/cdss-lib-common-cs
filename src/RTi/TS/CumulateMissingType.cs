using System;
using System.Collections.Generic;

// CumulateMissingType - enumeration that stores values for how to handle missing values when cumulating data

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
	/// This enumeration stores values how to handle missing values when cumulating data
	/// (e.g., with TSUtil_CumulateTimeSeries).
	/// </summary>
	public sealed class CumulateMissingType
	{

	/// <summary>
	/// Trend in data is decreasing.
	/// </summary>
	public static readonly CumulateMissingType CARRY_FORWARD = new CumulateMissingType("CARRY_FORWARD", InnerEnum.CARRY_FORWARD, "CarryForwardIfMissing");

	/// <summary>
	/// Trend in data is increasing.
	/// </summary>
	public static readonly CumulateMissingType SET_MISSING = new CumulateMissingType("SET_MISSING", InnerEnum.SET_MISSING, "SetMissingIfMissing");

	private static readonly IList<CumulateMissingType> valueList = new List<CumulateMissingType>();

	static CumulateMissingType()
	{
		valueList.Add(CARRY_FORWARD);
		valueList.Add(SET_MISSING);
	}

	public enum InnerEnum
	{
		CARRY_FORWARD,
		SET_MISSING
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
	private CumulateMissingType(string name, InnerEnum innerEnum, string displayName)
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
	public static CumulateMissingType valueOfIgnoreCase(string name)
	{
		if (string.ReferenceEquals(name, null))
		{
			return null;
		}
		CumulateMissingType[] values = values();
		// Currently supported values
		foreach (CumulateMissingType t in values)
		{
			if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				return t;
			}
		}
		return null;
	}


		public static IList<CumulateMissingType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static CumulateMissingType valueOf(string name)
		{
			foreach (CumulateMissingType enumInstance in CumulateMissingType.valueList)
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