using System;
using System.Collections.Generic;

// DataTableJoinMethodType - enumeration of table join methods

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
	/// Enumeration of table join methods.
	/// </summary>
	public sealed class DataTableJoinMethodType
	{

	/*
	Always join table records.
	*/
	public static readonly DataTableJoinMethodType JOIN_ALWAYS = new DataTableJoinMethodType("JOIN_ALWAYS", InnerEnum.JOIN_ALWAYS, "JoinAlways");
	/*
	Only join if the join column values match in both tables.
	*/
	public static readonly DataTableJoinMethodType JOIN_IF_IN_BOTH = new DataTableJoinMethodType("JOIN_IF_IN_BOTH", InnerEnum.JOIN_IF_IN_BOTH, "JoinIfInBoth");

	private static readonly IList<DataTableJoinMethodType> valueList = new List<DataTableJoinMethodType>();

	static DataTableJoinMethodType()
	{
		valueList.Add(JOIN_ALWAYS);
		valueList.Add(JOIN_IF_IN_BOTH);
	}

	public enum InnerEnum
	{
		JOIN_ALWAYS,
		JOIN_IF_IN_BOTH
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
	private DataTableJoinMethodType(string name, InnerEnum innerEnum, string displayName)
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
	public static DataTableJoinMethodType valueOfIgnoreCase(string name)
	{
		if (string.ReferenceEquals(name, null))
		{
			return null;
		}
		DataTableJoinMethodType[] values = values();
		// Currently supported values
		foreach (DataTableJoinMethodType t in values)
		{
			if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				return t;
			}
		}
		return null;
	}


		public static IList<DataTableJoinMethodType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static DataTableJoinMethodType valueOf(string name)
		{
			foreach (DataTableJoinMethodType enumInstance in DataTableJoinMethodType.valueList)
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