using System;
using System.Collections.Generic;

// TSUtil_ChangeInterval_OutputFillMethodType - To be used with the TSUtil_ChangeInterval class,
// used when converting from INST to MEAN time series going from larger to smaller time interval.

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
	/// To be used with the TSUtil_ChangeInterval class.  Used when converting from INST to MEAN time
	/// series going from larger to smaller time interval.
	/// </summary>
	public sealed class TSUtil_ChangeInterval_OutputFillMethodType
	{
	/// <summary>
	/// Required intervening values are interpolated from end-points of longer interval.
	/// </summary>
	public static readonly TSUtil_ChangeInterval_OutputFillMethodType INTERPOLATE = new TSUtil_ChangeInterval_OutputFillMethodType("INTERPOLATE", InnerEnum.INTERPOLATE, "Interpolate");
	/// <summary>
	/// Required intervening values are repeated from previous values.
	/// </summary>
	public static readonly TSUtil_ChangeInterval_OutputFillMethodType REPEAT = new TSUtil_ChangeInterval_OutputFillMethodType("REPEAT", InnerEnum.REPEAT, "Repeat");

	private static readonly IList<TSUtil_ChangeInterval_OutputFillMethodType> valueList = new List<TSUtil_ChangeInterval_OutputFillMethodType>();

	static TSUtil_ChangeInterval_OutputFillMethodType()
	{
		valueList.Add(INTERPOLATE);
		valueList.Add(REPEAT);
	}

	public enum InnerEnum
	{
		INTERPOLATE,
		REPEAT
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
	/// Construct an output fill method enumeration value. </summary>
	/// <param name="displayName"> name that should be displayed in choices, etc. </param>
	private TSUtil_ChangeInterval_OutputFillMethodType(string name, InnerEnum innerEnum, string displayName)
	{
		this.displayName = displayName;

		nameValue = name;
		ordinalValue = nextOrdinal++;
		innerEnumValue = innerEnum;
	}

	/// <summary>
	/// Get the list of time scales. </summary>
	/// <returns> the list of time scales. </returns>
	public static IList<TSUtil_ChangeInterval_OutputFillMethodType> getOutputFillMethodChoices()
	{
		IList<TSUtil_ChangeInterval_OutputFillMethodType> choices = new List<TSUtil_ChangeInterval_OutputFillMethodType>();
		choices.Add(TSUtil_ChangeInterval_OutputFillMethodType.INTERPOLATE);
		choices.Add(TSUtil_ChangeInterval_OutputFillMethodType.REPEAT);
		return choices;
	}

	/// <summary>
	/// Get the list of output fill methods. </summary>
	/// <returns> the list of output fill methods as strings. </returns>
	public static IList<string> getOutputFillMethodChoicesAsStrings()
	{
		IList<TSUtil_ChangeInterval_OutputFillMethodType> choices = getOutputFillMethodChoices();
		IList<string> stringChoices = new List<string>();
		for (int i = 0; i < choices.Count; i++)
		{
			stringChoices.Add("" + choices[i]);
		}
		return stringChoices;
	}

	/// <summary>
	/// Return the short display name for the output fill method.  This is the same as the value. </summary>
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
	public static TSUtil_ChangeInterval_OutputFillMethodType valueOfIgnoreCase(string name)
	{
		TSUtil_ChangeInterval_OutputFillMethodType[] values = values();
		foreach (TSUtil_ChangeInterval_OutputFillMethodType t in values)
		{
			if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				return t;
			}
		}
		throw new System.ArgumentException("The following does not match a output fill method: \"" + name + "\"");
	}


		public static IList<TSUtil_ChangeInterval_OutputFillMethodType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static TSUtil_ChangeInterval_OutputFillMethodType valueOf(string name)
		{
			foreach (TSUtil_ChangeInterval_OutputFillMethodType enumInstance in TSUtil_ChangeInterval_OutputFillMethodType.valueList)
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