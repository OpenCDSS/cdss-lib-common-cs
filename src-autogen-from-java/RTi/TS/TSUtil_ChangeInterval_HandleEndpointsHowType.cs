using System;
using System.Collections.Generic;

// TSUtil_ChangeInterval_HandleEndpointsHowType - To be used with the TSUtil_ChangeInterval class,
// indicating how to handle end-points converting from small to large interval, for interval smaller than daily.

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
	/// To be used with the TSUtil_ChangeInterval class, indicating how to handle end-points converting from
	/// small to large interval, for interval smaller than daily.
	/// </summary>
	public sealed class TSUtil_ChangeInterval_HandleEndpointsHowType
	{
	/// <summary>
	/// Average both end-points.
	/// </summary>
	public static readonly TSUtil_ChangeInterval_HandleEndpointsHowType AVERAGE_ENDPOINTS = new TSUtil_ChangeInterval_HandleEndpointsHowType("AVERAGE_ENDPOINTS", InnerEnum.AVERAGE_ENDPOINTS, "AverageEndpoints");
	/// <summary>
	/// Only include the first end-point.
	/// </summary>
	public static readonly TSUtil_ChangeInterval_HandleEndpointsHowType INCLUDE_FIRST_ONLY = new TSUtil_ChangeInterval_HandleEndpointsHowType("INCLUDE_FIRST_ONLY", InnerEnum.INCLUDE_FIRST_ONLY, "IncludeFirstOnly");

	private static readonly IList<TSUtil_ChangeInterval_HandleEndpointsHowType> valueList = new List<TSUtil_ChangeInterval_HandleEndpointsHowType>();

	static TSUtil_ChangeInterval_HandleEndpointsHowType()
	{
		valueList.Add(AVERAGE_ENDPOINTS);
		valueList.Add(INCLUDE_FIRST_ONLY);
	}

	public enum InnerEnum
	{
		AVERAGE_ENDPOINTS,
		INCLUDE_FIRST_ONLY
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
	/// Construct a time series statistic enumeration value. </summary>
	/// <param name="displayName"> name that should be displayed in choices, etc. </param>
	private TSUtil_ChangeInterval_HandleEndpointsHowType(string name, InnerEnum innerEnum, string displayName)
	{
		this.displayName = displayName;

		nameValue = name;
		ordinalValue = nextOrdinal++;
		innerEnumValue = innerEnum;
	}

	/// <summary>
	/// Get the list of enumerations. </summary>
	/// <returns> the list of enumerations. </returns>
	public static IList<TSUtil_ChangeInterval_HandleEndpointsHowType> getHandleEndpointsHowChoices()
	{
		IList<TSUtil_ChangeInterval_HandleEndpointsHowType> choices = new List<TSUtil_ChangeInterval_HandleEndpointsHowType>();
		choices.Add(TSUtil_ChangeInterval_HandleEndpointsHowType.AVERAGE_ENDPOINTS);
		choices.Add(TSUtil_ChangeInterval_HandleEndpointsHowType.INCLUDE_FIRST_ONLY);
		return choices;
	}

	/// <summary>
	/// Get the list of enumerations as strings. </summary>
	/// <returns> the list of enumerations strings. </returns>
	public static IList<string> getHandleEndpointsHowChoicesAsStrings()
	{
		IList<TSUtil_ChangeInterval_HandleEndpointsHowType> choices = getHandleEndpointsHowChoices();
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
	public static TSUtil_ChangeInterval_HandleEndpointsHowType valueOfIgnoreCase(string name)
	{
		TSUtil_ChangeInterval_HandleEndpointsHowType[] values = values();
		foreach (TSUtil_ChangeInterval_HandleEndpointsHowType t in values)
		{
			if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				return t;
			}
		}
		throw new System.ArgumentException("The following does not match a recognized HandleEndpointsHow value: \"" + name + "\"");
	}


		public static IList<TSUtil_ChangeInterval_HandleEndpointsHowType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static TSUtil_ChangeInterval_HandleEndpointsHowType valueOf(string name)
		{
			foreach (TSUtil_ChangeInterval_HandleEndpointsHowType enumInstance in TSUtil_ChangeInterval_HandleEndpointsHowType.valueList)
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