using System;
using System.Collections.Generic;

// LookupMethodType - lookup method types when using a lookup table

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

namespace RTi.Util.Table
{
	/// <summary>
	/// Lookup method types when using a lookup table.
	/// </summary>
	public sealed class LookupMethodType
	{
		/// <summary>
		/// Interpolate between known values.
		/// </summary>
		public static readonly LookupMethodType INTERPOLATE = new LookupMethodType("INTERPOLATE", InnerEnum.INTERPOLATE, "Interpolate");
		/// <summary>
		/// Use the next value in the table.
		/// </summary>
		public static readonly LookupMethodType NEXT_VALUE = new LookupMethodType("NEXT_VALUE", InnerEnum.NEXT_VALUE, "NextValue");
		/// <summary>
		/// Use the previous value in the table.
		/// </summary>
		public static readonly LookupMethodType PREVIOUS_VALUE = new LookupMethodType("PREVIOUS_VALUE", InnerEnum.PREVIOUS_VALUE, "PreviousValue");

		private static readonly IList<LookupMethodType> valueList = new List<LookupMethodType>();

		static LookupMethodType()
		{
			valueList.Add(INTERPOLATE);
			valueList.Add(NEXT_VALUE);
			valueList.Add(PREVIOUS_VALUE);
		}

		public enum InnerEnum
		{
			INTERPOLATE,
			NEXT_VALUE,
			PREVIOUS_VALUE
		}

		public readonly InnerEnum innerEnumValue;
		private readonly string nameValue;
		private readonly int ordinalValue;
		private static int nextOrdinal = 0;

		private readonly string displayName;

		/// <summary>
		/// Name that should be displayed in choices, etc. </summary>
		/// <param name="displayName"> </param>
		private LookupMethodType(string name, InnerEnum innerEnum, string displayName)
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
		public static LookupMethodType valueOfIgnoreCase(string name)
		{
			LookupMethodType[] values = values();
			foreach (LookupMethodType t in values)
			{
				if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					return t;
				}
			}
			return null;
		}

		public static IList<LookupMethodType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static LookupMethodType valueOf(string name)
		{
			foreach (LookupMethodType enumInstance in LookupMethodType.valueList)
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