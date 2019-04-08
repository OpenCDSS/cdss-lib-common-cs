using System;
using System.Collections.Generic;

// SortOrderType - sort order, for example when ranking or determining plotting position

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

namespace RTi.Util.Math
{
	/// <summary>
	/// Sort order, for example when ranking or determining plotting position.
	/// </summary>
	public sealed class SortOrderType
	{
		/// <summary>
		/// Low to high (first position in rank is smallest value).
		/// </summary>
		public static readonly SortOrderType LOW_TO_HIGH = new SortOrderType("LOW_TO_HIGH", InnerEnum.LOW_TO_HIGH, "LowToHigh");
		/// <summary>
		/// High to low (first position in rank is highest value).
		/// </summary>
		public static readonly SortOrderType HIGH_TO_LOW = new SortOrderType("HIGH_TO_LOW", InnerEnum.HIGH_TO_LOW, "HighToLow");

		private static readonly IList<SortOrderType> valueList = new List<SortOrderType>();

		static SortOrderType()
		{
			valueList.Add(LOW_TO_HIGH);
			valueList.Add(HIGH_TO_LOW);
		}

		public enum InnerEnum
		{
			LOW_TO_HIGH,
			HIGH_TO_LOW
		}

		public readonly InnerEnum innerEnumValue;
		private readonly string nameValue;
		private readonly int ordinalValue;
		private static int nextOrdinal = 0;

		private readonly string displayName;

		/// <summary>
		/// Name that should be displayed in choices, etc. </summary>
		/// <param name="displayName"> </param>
		private SortOrderType(string name, InnerEnum innerEnum, string displayName)
		{
			this.displayName = displayName;

			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

		/// <summary>
		/// Return the display name. </summary>
		/// <returns> the display name. </returns>
		public override string ToString()
		{
			return displayName;
		}

		/// <summary>
		/// Return the enumeration value given a string name (case-independent). </summary>
		/// <returns> the enumeration value given a string name (case-independent), or null if not matched. </returns>
		public static SortOrderType valueOfIgnoreCase(string name)
		{
			if (string.ReferenceEquals(name, null))
			{
				return null;
			}
			SortOrderType[] values = values();
			foreach (SortOrderType t in values)
			{
				if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					return t;
				}
			}
			return null;
		}

		public static IList<SortOrderType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static SortOrderType valueOf(string name)
		{
			foreach (SortOrderType enumInstance in SortOrderType.valueList)
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