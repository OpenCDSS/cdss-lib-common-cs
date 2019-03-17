using System;
using System.Collections.Generic;

// TrendType - enumeration to store values for a trend, meaning whether data values increase over time, decrease, or are variable

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
	/// This enumeration stores values for a trend, meaning whether data values increase over time, decrease, or
	/// are variable.  For example, the trend for accumulated precipitation data is that values increase over time.
	/// </summary>
	public sealed class TrendType
	{
		/// <summary>
		/// Trend in data is decreasing.
		/// </summary>
		public static readonly TrendType DECREASING = new TrendType("DECREASING", InnerEnum.DECREASING, "Decreasing");
		/// <summary>
		/// Trend in data is increasing.
		/// </summary>
		public static readonly TrendType INCREASING = new TrendType("INCREASING", InnerEnum.INCREASING, "Increasing");
		/// <summary>
		/// Trend in values is variable (some increasing and decreasing).
		/// </summary>
		public static readonly TrendType VARIABLE = new TrendType("VARIABLE", InnerEnum.VARIABLE, "Variable");

		private static readonly IList<TrendType> valueList = new List<TrendType>();

		static TrendType()
		{
			valueList.Add(DECREASING);
			valueList.Add(INCREASING);
			valueList.Add(VARIABLE);
		}

		public enum InnerEnum
		{
			DECREASING,
			INCREASING,
			VARIABLE
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
		/// Construct an enumeration value. </summary>
		/// <param name="displayName"> name that should be displayed in choices, etc. </param>
		private TrendType(string name, InnerEnum innerEnum, string displayName)
		{
			this.displayName = displayName;

			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

	/// <summary>
	/// Return the display name for the enumeration.  This is usually the same as the
	/// value but using appropriate mixed case. </summary>
	/// <returns> the display name. </returns>
	public override string ToString()
	{
		return displayName;
	}

	/// <summary>
	/// Return the enumeration value given a string name (case-independent). </summary>
	/// <returns> the enumeration value given a string name (case-independent), or null if not matched. </returns>
	public static TrendType valueOfIgnoreCase(string name)
	{
		TrendType[] values = values();
		// Currently supported values
		foreach (TrendType t in values)
		{
			if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				return t;
			}
		}
		return null;
	}


		public static IList<TrendType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static TrendType valueOf(string name)
		{
			foreach (TrendType enumInstance in TrendType.valueList)
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