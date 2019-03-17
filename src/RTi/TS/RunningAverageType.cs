using System;
using System.Collections.Generic;

// RunningAverageType - enumeration for running average types, used to determine the sample for the analysis.

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
	/// This enumeration defines running average types, used to determine the sample for the analysis.
	/// The enumeration can be used with more than average statistic.
	/// </summary>
	public sealed class RunningAverageType
	{
		/// <summary>
		/// Average values from all the available years.
		/// </summary>
		public static readonly RunningAverageType ALL_YEARS = new RunningAverageType("ALL_YEARS", InnerEnum.ALL_YEARS, "AllYears");
		/// <summary>
		/// Average values from both sides of the time step, inclusive of the center value.
		/// </summary>
		public static readonly RunningAverageType CENTERED = new RunningAverageType("CENTERED", InnerEnum.CENTERED, "Centered");
		/// <summary>
		/// Custom bracket using custom offsets.  For example, at an interval, the sample may be determined by a future bracket.
		/// </summary>
		public static readonly RunningAverageType CUSTOM = new RunningAverageType("CUSTOM", InnerEnum.CUSTOM, "Custom");
		/// <summary>
		/// Average values from the same time step for each of the previous N -1 years and the current year.
		/// </summary>
		public static readonly RunningAverageType NYEAR = new RunningAverageType("NYEAR", InnerEnum.NYEAR, "NYear");
		/// <summary>
		/// Average values from the same time step for all of the previous years and the current year (similar to
		/// N-Year but for all years.
		/// </summary>
		public static readonly RunningAverageType N_ALL_YEAR = new RunningAverageType("N_ALL_YEAR", InnerEnum.N_ALL_YEAR, "NAllYear");
		/// <summary>
		/// Average values prior to the current time step, not inclusive of the current point.
		/// </summary>
		public static readonly RunningAverageType PREVIOUS = new RunningAverageType("PREVIOUS", InnerEnum.PREVIOUS, "Previous");
		/// <summary>
		/// Average values prior to the current time step, inclusive of the current point.
		/// </summary>
		public static readonly RunningAverageType PREVIOUS_INCLUSIVE = new RunningAverageType("PREVIOUS_INCLUSIVE", InnerEnum.PREVIOUS_INCLUSIVE, "PreviousInclusive");
		/// <summary>
		/// Average values after to the current time step, not inclusive of the current point.
		/// </summary>
		public static readonly RunningAverageType FUTURE = new RunningAverageType("FUTURE", InnerEnum.FUTURE, "Future");
		/// <summary>
		/// Create a running average by averaging values after to the current time step, inclusive of the current point.
		/// </summary>
		public static readonly RunningAverageType FUTURE_INCLUSIVE = new RunningAverageType("FUTURE_INCLUSIVE", InnerEnum.FUTURE_INCLUSIVE, "FutureInclusive");

		private static readonly IList<RunningAverageType> valueList = new List<RunningAverageType>();

		static RunningAverageType()
		{
			valueList.Add(ALL_YEARS);
			valueList.Add(CENTERED);
			valueList.Add(CUSTOM);
			valueList.Add(NYEAR);
			valueList.Add(N_ALL_YEAR);
			valueList.Add(PREVIOUS);
			valueList.Add(PREVIOUS_INCLUSIVE);
			valueList.Add(FUTURE);
			valueList.Add(FUTURE_INCLUSIVE);
		}

		public enum InnerEnum
		{
			ALL_YEARS,
			CENTERED,
			CUSTOM,
			NYEAR,
			N_ALL_YEAR,
			PREVIOUS,
			PREVIOUS_INCLUSIVE,
			FUTURE,
			FUTURE_INCLUSIVE
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
		private RunningAverageType(string name, InnerEnum innerEnum, string displayName)
		{
			this.displayName = displayName;

			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

	/// <summary>
	/// Return the display name for the running average type.  This is usually similar to the
	/// value but using appropriate mixed case. </summary>
	/// <returns> the display name. </returns>
	public override string ToString()
	{
		return displayName;
	}

	/// <summary>
	/// Return the enumeration value given a string name (case-independent). </summary>
	/// <returns> the enumeration value given a string name (case-independent), or null if not matched. </returns>
	public static RunningAverageType valueOfIgnoreCase(string name)
	{
		if (string.ReferenceEquals(name, null))
		{
			return null;
		}
		// Legacy
		if (name.Equals("N-Year", StringComparison.OrdinalIgnoreCase))
		{
			// Replaced with newer "NYear"
			return NYEAR;
		}
		RunningAverageType[] values = values();
		foreach (RunningAverageType t in values)
		{
			if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				return t;
			}
		}
		return null;
	}


		public static IList<RunningAverageType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static RunningAverageType valueOf(string name)
		{
			foreach (RunningAverageType enumInstance in RunningAverageType.valueList)
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