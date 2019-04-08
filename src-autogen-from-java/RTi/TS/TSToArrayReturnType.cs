using System;
using System.Collections.Generic;

// TSToArrayReturnType - this enumeration defines the value to be returned when converting a time series to an array.

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
	/// This enumeration defines the value to be returned when converting a time series to an array.
	/// Typically the data value is returned; however, there are cases when the date/time is returned.
	/// </summary>
	public sealed class TSToArrayReturnType
	{
	/// <summary>
	/// Return the time series data value.
	/// </summary>
	public static readonly TSToArrayReturnType DATA_VALUE = new TSToArrayReturnType("DATA_VALUE", InnerEnum.DATA_VALUE, "DataValue");
	/// <summary>
	/// Return the date/time associated with data values.
	/// </summary>
	public static readonly TSToArrayReturnType DATE_TIME = new TSToArrayReturnType("DATE_TIME", InnerEnum.DATE_TIME, "DateTime");

	private static readonly IList<TSToArrayReturnType> valueList = new List<TSToArrayReturnType>();

	static TSToArrayReturnType()
	{
		valueList.Add(DATA_VALUE);
		valueList.Add(DATE_TIME);
	}

	public enum InnerEnum
	{
		DATA_VALUE,
		DATE_TIME
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
	private TSToArrayReturnType(string name, InnerEnum innerEnum, string displayName)
	{
		this.displayName = displayName;

		nameValue = name;
		ordinalValue = nextOrdinal++;
		innerEnumValue = innerEnum;
	}

	/// <summary>
	/// Return the display name for the type.  This is usually similar to the
	/// value but using appropriate mixed case. </summary>
	/// <returns> the display name. </returns>
	public override string ToString()
	{
		return displayName;
	}

	/// <summary>
	/// Return the enumeration value given a string name (case-independent). </summary>
	/// <returns> the enumeration value given a string name (case-independent), or null if not matched. </returns>
	public static TSToArrayReturnType valueOfIgnoreCase(string name)
	{
		if (string.ReferenceEquals(name, null))
		{
			return null;
		}
		TSToArrayReturnType[] values = values();
		foreach (TSToArrayReturnType t in values)
		{
			if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				return t;
			}
		}
		return null;
	}


		public static IList<TSToArrayReturnType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static TSToArrayReturnType valueOf(string name)
		{
			foreach (TSToArrayReturnType enumInstance in TSToArrayReturnType.valueList)
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