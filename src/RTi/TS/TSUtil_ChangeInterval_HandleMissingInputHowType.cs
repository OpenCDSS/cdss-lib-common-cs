using System;
using System.Collections.Generic;

// TSUtil_ChangeInterval_HandleMissingInputHowType - To be used with the TSUtil_ChangeInterval class,
// indicating how to handle missing values in input.

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
	/// To be used with the TSUtil_ChangeInterval class, indicating how to handle missing values in input.
	/// </summary>
	public sealed class TSUtil_ChangeInterval_HandleMissingInputHowType
	{
	/// <summary>
	/// Keep the missing values in input.
	/// </summary>
	public static readonly TSUtil_ChangeInterval_HandleMissingInputHowType KEEP_MISSING = new TSUtil_ChangeInterval_HandleMissingInputHowType("KEEP_MISSING", InnerEnum.KEEP_MISSING, "KeepMissing");
	/// <summary>
	/// Repeat non-missing values.
	/// </summary>
	public static readonly TSUtil_ChangeInterval_HandleMissingInputHowType REPEAT = new TSUtil_ChangeInterval_HandleMissingInputHowType("REPEAT", InnerEnum.REPEAT, "Repeat");
	/// <summary>
	/// Set missing values to zero.
	/// </summary>
	public static readonly TSUtil_ChangeInterval_HandleMissingInputHowType SET_TO_ZERO = new TSUtil_ChangeInterval_HandleMissingInputHowType("SET_TO_ZERO", InnerEnum.SET_TO_ZERO, "SetToZero");

	private static readonly IList<TSUtil_ChangeInterval_HandleMissingInputHowType> valueList = new List<TSUtil_ChangeInterval_HandleMissingInputHowType>();

	static TSUtil_ChangeInterval_HandleMissingInputHowType()
	{
		valueList.Add(KEEP_MISSING);
		valueList.Add(REPEAT);
		valueList.Add(SET_TO_ZERO);
	}

	public enum InnerEnum
	{
		KEEP_MISSING,
		REPEAT,
		SET_TO_ZERO
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
	private TSUtil_ChangeInterval_HandleMissingInputHowType(string name, InnerEnum innerEnum, string displayName)
	{
		this.displayName = displayName;

		nameValue = name;
		ordinalValue = nextOrdinal++;
		innerEnumValue = innerEnum;
	}

	/// <summary>
	/// Get the list of enumerations. </summary>
	/// <returns> the list of enumerations. </returns>
	public static IList<TSUtil_ChangeInterval_HandleMissingInputHowType> getHandleMissingInputHowChoices()
	{
		IList<TSUtil_ChangeInterval_HandleMissingInputHowType> choices = new List<TSUtil_ChangeInterval_HandleMissingInputHowType>();
		choices.Add(TSUtil_ChangeInterval_HandleMissingInputHowType.KEEP_MISSING);
		choices.Add(TSUtil_ChangeInterval_HandleMissingInputHowType.REPEAT);
		choices.Add(TSUtil_ChangeInterval_HandleMissingInputHowType.SET_TO_ZERO);
		return choices;
	}

	/// <summary>
	/// Get the list of enumerations as strings. </summary>
	/// <returns> the list of enumerations strings. </returns>
	public static IList<string> getHandleMissingInputHowChoicesAsStrings()
	{
		IList<TSUtil_ChangeInterval_HandleMissingInputHowType> choices = getHandleMissingInputHowChoices();
		IList<string> stringChoices = new List<string>();
		for (int i = 0; i < choices.Count; i++)
		{
			stringChoices.Add("" + choices[i]);
		}
		return stringChoices;
	}

	/// <summary>
	/// Return the short display name for the statistic.  This is the same as the value. </summary>
	/// <returns> the display name. </returns>
	public override string ToString()
	{
		return displayName;
	}

	/// <summary>
	/// Return the enumeration value given a string name (case-independent). </summary>
	/// <param name="name"> the time scale string to match. </param>
	/// <returns> the enumeration value given a string name (case-independent). </returns>
	/// <exception cref="IllegalArgumentException"> if the name does not match a valid time scale. </exception>
	public static TSUtil_ChangeInterval_HandleMissingInputHowType valueOfIgnoreCase(string name)
	{
		TSUtil_ChangeInterval_HandleMissingInputHowType[] values = values();
		foreach (TSUtil_ChangeInterval_HandleMissingInputHowType t in values)
		{
			if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				return t;
			}
		}
		throw new System.ArgumentException("The following does not match a recognized HandleMissingInputHow value: \"" + name + "\"");
	}


		public static IList<TSUtil_ChangeInterval_HandleMissingInputHowType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static TSUtil_ChangeInterval_HandleMissingInputHowType valueOf(string name)
		{
			foreach (TSUtil_ChangeInterval_HandleMissingInputHowType enumInstance in TSUtil_ChangeInterval_HandleMissingInputHowType.valueList)
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