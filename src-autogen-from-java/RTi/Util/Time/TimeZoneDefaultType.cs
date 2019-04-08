using System;
using System.Collections.Generic;

// TimeZoneDefaultType - how to handle missing time zone, for example when DateTime.getDate() is called

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

namespace RTi.Util.Time
{

	/// <summary>
	/// How to handle missing time zone, for example when DateTime.getDate() is called.
	/// </summary>
	public sealed class TimeZoneDefaultType
	{
	/// <summary>
	/// Use local computer time zone.
	/// </summary>
	public static readonly TimeZoneDefaultType LOCAL = new TimeZoneDefaultType("LOCAL", InnerEnum.LOCAL, "Local");
	/// <summary>
	/// No default timezone allowed allowed.
	/// </summary>
	public static readonly TimeZoneDefaultType NONE = new TimeZoneDefaultType("NONE", InnerEnum.NONE, "None");
	/// <summary>
	/// GMT time zone as default.
	/// </summary>
	public static readonly TimeZoneDefaultType GMT = new TimeZoneDefaultType("GMT", InnerEnum.GMT, "GMT");

	private static readonly IList<TimeZoneDefaultType> valueList = new List<TimeZoneDefaultType>();

	static TimeZoneDefaultType()
	{
		valueList.Add(LOCAL);
		valueList.Add(NONE);
		valueList.Add(GMT);
	}

	public enum InnerEnum
	{
		LOCAL,
		NONE,
		GMT
	}

	public readonly InnerEnum innerEnumValue;
	private readonly string nameValue;
	private readonly int ordinalValue;
	private static int nextOrdinal = 0;

	/// <summary>
	/// The name that is used for choices and other technical code (terse).
	/// </summary>
	private readonly string displayName;

	/// <summary>
	/// Construct an enumeration value. </summary>
	/// <param name="displayName"> name that should be displayed in choices, etc. </param>
	private TimeZoneDefaultType(string name, InnerEnum innerEnum, string displayName)
	{
		this.displayName = displayName;

		nameValue = name;
		ordinalValue = nextOrdinal++;
		innerEnumValue = innerEnum;
	}

	/// <summary>
	/// Get the list of time zone default types. </summary>
	/// <returns> the list of time zone default types. </returns>
	public static IList<TimeZoneDefaultType> getTimeZoneDefaultChoices()
	{
		IList<TimeZoneDefaultType> choices = new List<TimeZoneDefaultType>();
		choices.Add(TimeZoneDefaultType.GMT);
		choices.Add(TimeZoneDefaultType.LOCAL);
		choices.Add(TimeZoneDefaultType.NONE);
		return choices;
	}

	/// <summary>
	/// Get the list of time zone default types. </summary>
	/// <returns> the list of time zone default types as strings. </returns>
	public static IList<string> getTimeZoneDefaultChoicesAsStrings(bool includeNote)
	{
		IList<TimeZoneDefaultType> choices = getTimeZoneDefaultChoices();
		IList<string> stringChoices = new List<string>();
		for (int i = 0; i < choices.Count; i++)
		{
			TimeZoneDefaultType choice = choices[i];
			string choiceString = "" + choice;
			stringChoices.Add(choiceString);
		}
		return stringChoices;
	}

	/// <summary>
	/// Return the short display name for the type.  This is the same as the value. </summary>
	/// <returns> the display name. </returns>
	public override string ToString()
	{
		return displayName;
	}

	/// <summary>
	/// Return the enumeration value given a string name (case-independent). </summary>
	/// <param name="name"> the display name for the time zone default. </param>
	/// <returns> the enumeration value given a string name (case-independent). </returns>
	/// <exception cref="IllegalArgumentException"> if the name does not match a valid time zone default type. </exception>
	public static TimeZoneDefaultType valueOfIgnoreCase(string name)
	{
		if (string.ReferenceEquals(name, null))
		{
			return null;
		}
		TimeZoneDefaultType[] values = values();
		foreach (TimeZoneDefaultType t in values)
		{
			if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				return t;
			}
		}
		throw new System.ArgumentException("The following does not match a valid time zone default type: \"" + name + "\"");
	}


		public static IList<TimeZoneDefaultType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static TimeZoneDefaultType valueOf(string name)
		{
			foreach (TimeZoneDefaultType enumInstance in TimeZoneDefaultType.valueList)
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