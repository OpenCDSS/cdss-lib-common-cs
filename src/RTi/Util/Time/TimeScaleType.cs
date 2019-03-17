using System;
using System.Collections.Generic;

// TimeScaleType - time scale types, which are important when making time-based data observations

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
	/// Time scale types, which are important when making time-based data observations.
	/// </summary>
	public sealed class TimeScaleType
	{
	/// <summary>
	/// Data are accumulated over the time interval prior to the recorded date/time.
	/// </summary>
	public static readonly TimeScaleType ACCM = new TimeScaleType("ACCM", InnerEnum.ACCM, "ACCM", "Accumulated");
	/// <summary>
	/// Instantaneous data value is recorded at the date/time.
	/// </summary>
	public static readonly TimeScaleType INST = new TimeScaleType("INST", InnerEnum.INST, "INST", "Instantaneous");
	/// <summary>
	/// Data are averaged over the time interval prior to the recorded date/time.
	/// </summary>
	public static readonly TimeScaleType MEAN = new TimeScaleType("MEAN", InnerEnum.MEAN, "MEAN", "Mean");

	private static readonly IList<TimeScaleType> valueList = new List<TimeScaleType>();

	static TimeScaleType()
	{
		valueList.Add(ACCM);
		valueList.Add(INST);
		valueList.Add(MEAN);
	}

	public enum InnerEnum
	{
		ACCM,
		INST,
		MEAN
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
	private TimeScaleType(string name, InnerEnum innerEnum, string displayName, string displayNameVerbose)
	{
		this.displayName = displayName;
		this.displayNameVerbose = displayNameVerbose;

		nameValue = name;
		ordinalValue = nextOrdinal++;
		innerEnumValue = innerEnum;
	}

	/// <summary>
	/// Get the list of time scales. </summary>
	/// <returns> the list of time scales. </returns>
	public static IList<TimeScaleType> getTimeScaleChoices()
	{
		IList<TimeScaleType> choices = new List<TimeScaleType>();
		choices.Add(TimeScaleType.ACCM);
		choices.Add(TimeScaleType.INST);
		choices.Add(TimeScaleType.MEAN);
		return choices;
	}

	/// <summary>
	/// Get the list of time scales. </summary>
	/// <returns> the list of time scales as strings. </returns>
	/// <param name="includeNote"> If true, the returned string will be of the form
	/// "ACCM - Accumulated", using the sort and verbose display names.
	/// If false, the returned string will be of the form "ACCM", using only the short display name. </param>
	public static IList<string> getTimeScaleChoicesAsStrings(bool includeNote)
	{
		IList<TimeScaleType> choices = getTimeScaleChoices();
		IList<string> stringChoices = new List<string>();
		for (int i = 0; i < choices.Count; i++)
		{
			TimeScaleType choice = choices[i];
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
	/// Return the short display name for the statistic.  This is the same as the value. </summary>
	/// <returns> the display name. </returns>
	public override string ToString()
	{
		return displayName;
	}

	/// <summary>
	/// Return the verbose display name for the statistic. </summary>
	/// <returns> the verbose display name (e.g., "Accumulated" instead of the shorter "ACCM"). </returns>
	public string toStringVerbose()
	{
		return displayNameVerbose;
	}

	/// <summary>
	/// Return the enumeration value given a string name (case-independent). </summary>
	/// <param name="name"> the time scale string to match, as either the short or verbose display name, or the
	/// concatenated version "displayName - displayNameVerbose". </param>
	/// <returns> the enumeration value given a string name (case-independent), or null if not matched. </returns>
	/// <exception cref="IllegalArgumentException"> if the name does not match a valid time scale. </exception>
	public static TimeScaleType valueOfIgnoreCase(string name)
	{
		TimeScaleType[] values = values();
		foreach (TimeScaleType t in values)
		{
			if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase) || name.Equals(t.toStringVerbose(), StringComparison.OrdinalIgnoreCase) || name.Equals(t.ToString() + " - " + t.toStringVerbose(), StringComparison.OrdinalIgnoreCase))
			{
				return t;
			}
		}
		throw new System.ArgumentException("The following does not match a valid time scale: \"" + name + "\"");
	}


		public static IList<TimeScaleType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static TimeScaleType valueOf(string name)
		{
			foreach (TimeScaleType enumInstance in TimeScaleType.valueList)
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