﻿using System;
using System.Collections.Generic;

// InputFilterStringCriterionType - enumeration of string conditions that can be checked for in an input filter

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
	/// Enumeration of string conditions that can be checked for in an input filter.
	/// String conditions are usually performed by ignoring case.
	/// @author sam
	/// 
	/// </summary>
	public sealed class InputFilterStringCriterionType
	{

	/// <summary>
	/// String contains a substring.
	/// </summary>
	public static readonly InputFilterStringCriterionType CONTAINS = new InputFilterStringCriterionType("CONTAINS", InnerEnum.CONTAINS, "Contains");
	/// <summary>
	/// String ends with a substring.
	/// </summary>
	public static readonly InputFilterStringCriterionType ENDS_WITH = new InputFilterStringCriterionType("ENDS_WITH", InnerEnum.ENDS_WITH, "EndsWith");
	/// <summary>
	/// Full string matches.
	/// </summary>
	public static readonly InputFilterStringCriterionType MATCHES = new InputFilterStringCriterionType("MATCHES", InnerEnum.MATCHES, "Matches");
	/// <summary>
	/// String starts with a substring.
	/// </summary>
	public static readonly InputFilterStringCriterionType STARTS_WITH = new InputFilterStringCriterionType("STARTS_WITH", InnerEnum.STARTS_WITH, "StartsWith");

	private static readonly IList<InputFilterStringCriterionType> valueList = new List<InputFilterStringCriterionType>();

	static InputFilterStringCriterionType()
	{
		valueList.Add(CONTAINS);
		valueList.Add(ENDS_WITH);
		valueList.Add(MATCHES);
		valueList.Add(STARTS_WITH);
	}

	public enum InnerEnum
	{
		CONTAINS,
		ENDS_WITH,
		MATCHES,
		STARTS_WITH
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
	private InputFilterStringCriterionType(string name, InnerEnum innerEnum, string displayName)
	{
		this.displayName = displayName;

		nameValue = name;
		ordinalValue = nextOrdinal++;
		innerEnumValue = innerEnum;
	}

	/// <summary>
	/// Return the display name for the condition.  This is usually the same as the
	/// value but using appropriate mixed case. </summary>
	/// <returns> the display name. </returns>
	public override string ToString()
	{
		return displayName;
	}

	/// <summary>
	/// Return the enumeration value given a string name (case-independent). </summary>
	/// <returns> the enumeration value given a string name (case-independent), or null if not matched. </returns>
	public static InputFilterStringCriterionType valueOfIgnoreCase(string name)
	{
		// Legacy/alternate values
		if (name.Equals("Ends with", StringComparison.OrdinalIgnoreCase))
		{
			return ENDS_WITH;
		}
		else if (name.Equals("Starts with", StringComparison.OrdinalIgnoreCase))
		{
			return STARTS_WITH;
		}
		InputFilterStringCriterionType[] values = values();
		// Currently supported values
		foreach (InputFilterStringCriterionType t in values)
		{
			if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				return t;
			}
		}
		return null;
	}


		public static IList<InputFilterStringCriterionType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static InputFilterStringCriterionType valueOf(string name)
		{
			foreach (InputFilterStringCriterionType enumInstance in InputFilterStringCriterionType.valueList)
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