using System;
using System.Collections.Generic;

// StringFormatterType - string formatter types, to allow the input objects to be formatted with different format strings

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

namespace RTi.Util.String
{

	/// <summary>
	/// String formatter types, to allow the input objects to be formatted with different format strings.
	/// </summary>
	public sealed class StringFormatterType
	{
	/// <summary>
	/// C-style formats (see "sprintf").
	/// </summary>
	public static readonly StringFormatterType C = new StringFormatterType("C", InnerEnum.C, "C", "C/UNIX");

	private static readonly IList<StringFormatterType> valueList = new List<StringFormatterType>();

	static StringFormatterType()
	{
		valueList.Add(C);
	}

	public enum InnerEnum
	{
		C
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
	/// The name that is used for notes and explanations (more verbose).
	/// </summary>
	private readonly string displayNameVerbose;

	/// <summary>
	/// Construct an enumeration value. </summary>
	/// <param name="displayName"> name that should be displayed in choices, etc. </param>
	private StringFormatterType(string name, InnerEnum innerEnum, string displayName, string displayNameVerbose)
	{
		this.displayName = displayName;
		this.displayNameVerbose = displayNameVerbose;

		nameValue = name;
		ordinalValue = nextOrdinal++;
		innerEnumValue = innerEnum;
	}

	/// <summary>
	/// Get the list of date/time formatter types. </summary>
	/// <returns> the list of date/time formatter types. </returns>
	public static IList<StringFormatterType> getDateTimeFormatterChoices()
	{
		IList<StringFormatterType> choices = new List<StringFormatterType>();
		choices.Add(StringFormatterType.C);
		return choices;
	}

	/// <summary>
	/// Get the list of date/time formatter types. </summary>
	/// <returns> the list of date/time formatter types as strings. </returns>
	/// <param name="includeNote"> If true, the returned string will be of the form
	/// "Excel - Excel date/time formatting", using the sort and verbose display names.
	/// If false, the returned string will be of the form "Excel", using only the short display name. </param>
	public static IList<string> getDateTimeFormatterChoicesAsStrings(bool includeNote)
	{
		IList<StringFormatterType> choices = getDateTimeFormatterChoices();
		IList<string> stringChoices = new List<string>();
		for (int i = 0; i < choices.Count; i++)
		{
			StringFormatterType choice = choices[i];
			string choiceString = "" + choice;
			if (includeNote)
			{
				choiceString = choiceString + " - " + choice.toStringVerbose();
			}
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
	/// Return the verbose display name for the type. </summary>
	/// <returns> the verbose display name for the type. </returns>
	public string toStringVerbose()
	{
		return displayNameVerbose;
	}

	/// <summary>
	/// Return the enumeration value given a string name (case-independent). </summary>
	/// <param name="name"> the date/time format string to match, as either the short or verbose display name, or the
	/// concatenated version "displayName - displayNameVerbose". </param>
	/// <returns> the enumeration value given a string name (case-independent), or null if not matched. </returns>
	/// <exception cref="IllegalArgumentException"> if the name does not match a valid date/time formatter type. </exception>
	public static StringFormatterType valueOfIgnoreCase(string name)
	{
		if (string.ReferenceEquals(name, null))
		{
			return null;
		}
		StringFormatterType[] values = values();
		foreach (StringFormatterType t in values)
		{
			if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase) || name.Equals(t.toStringVerbose(), StringComparison.OrdinalIgnoreCase) || name.Equals(t.ToString() + " - " + t.toStringVerbose(), StringComparison.OrdinalIgnoreCase))
			{
				return t;
			}
		}
		throw new System.ArgumentException("The following does not match a valid string formatter type: \"" + name + "\"");
	}


		public static IList<StringFormatterType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static StringFormatterType valueOf(string name)
		{
			foreach (StringFormatterType enumInstance in StringFormatterType.valueList)
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