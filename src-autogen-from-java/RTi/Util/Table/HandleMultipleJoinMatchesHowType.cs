using System;
using System.Collections.Generic;

// HandleMultipleJoinMatchesHowType - enumeration indicates how to handle multiple table join matches

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
	/// This enumeration indicates how to handle multiple table join matches (e.g., with the JoinTable command).
	/// </summary>
	public sealed class HandleMultipleJoinMatchesHowType
	{

	/// <summary>
	/// Add rows for each match.
	/// </summary>
	//ADD_ROWS("AddRows"),

	/// <summary>
	/// Number columns to be copied.  For example, 2nd match results in a new column with "_2" in column name.
	/// </summary>
	public static readonly HandleMultipleJoinMatchesHowType NUMBER_COLUMNS = new HandleMultipleJoinMatchesHowType("NUMBER_COLUMNS", InnerEnum.NUMBER_COLUMNS, "NumberColumns");

	/// <summary>
	/// Use the last match.
	/// </summary>
	public static readonly HandleMultipleJoinMatchesHowType USE_LAST_MATCH = new HandleMultipleJoinMatchesHowType("USE_LAST_MATCH", InnerEnum.USE_LAST_MATCH, "UseLastMatch"); //,

	private static readonly IList<HandleMultipleJoinMatchesHowType> valueList = new List<HandleMultipleJoinMatchesHowType>();

	static HandleMultipleJoinMatchesHowType()
	{
		valueList.Add(NUMBER_COLUMNS);
		valueList.Add(USE_LAST_MATCH);
	}

	public enum InnerEnum
	{
		NUMBER_COLUMNS,
		USE_LAST_MATCH
	}

	public readonly InnerEnum innerEnumValue;
	private readonly string nameValue;
	private readonly int ordinalValue;
	private static int nextOrdinal = 0;

	/// <summary>
	/// Use the last non-missing value.
	/// </summary>
	//USE_FIRST_MATCH("UseFirstMatch");

	/// <summary>
	/// The name that should be displayed when the type is used in UIs and reports.
	/// </summary>
	private readonly string displayName;

	/// <summary>
	/// Construct with the display name. </summary>
	/// <param name="displayName"> name that should be displayed in choices, etc. </param>
	private HandleMultipleJoinMatchesHowType(string name, InnerEnum innerEnum, string displayName)
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
	public static HandleMultipleJoinMatchesHowType valueOfIgnoreCase(string name)
	{
		if (string.ReferenceEquals(name, null))
		{
			return null;
		}
		HandleMultipleJoinMatchesHowType[] values = values();
		// Currently supported values
		foreach (HandleMultipleJoinMatchesHowType t in values)
		{
			if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				return t;
			}
		}
		return null;
	}


		public static IList<HandleMultipleJoinMatchesHowType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static HandleMultipleJoinMatchesHowType valueOf(string name)
		{
			foreach (HandleMultipleJoinMatchesHowType enumInstance in HandleMultipleJoinMatchesHowType.valueList)
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