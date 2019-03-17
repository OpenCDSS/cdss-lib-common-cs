using System;
using System.Collections.Generic;

// OutOfRangeLookupMethodType - lookup method types when using a lookup table, for out of range values.

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
	/// Lookup method types when using a lookup table, for out of range values.
	/// </summary>
	public sealed class OutOfRangeLookupMethodType
	{
		/// <summary>
		/// Interpolate between known values.
		/// </summary>
		public static readonly OutOfRangeLookupMethodType EXTRAPOLATE = new OutOfRangeLookupMethodType("EXTRAPOLATE", InnerEnum.EXTRAPOLATE, "Extrapolate");
		/// <summary>
		/// Set the resulting value missing.
		/// </summary>
		public static readonly OutOfRangeLookupMethodType SET_MISSING = new OutOfRangeLookupMethodType("SET_MISSING", InnerEnum.SET_MISSING, "SetMissing");
		/// <summary>
		/// Use the end value in the table.
		/// </summary>
		public static readonly OutOfRangeLookupMethodType USE_END_VALUE = new OutOfRangeLookupMethodType("USE_END_VALUE", InnerEnum.USE_END_VALUE, "UseEndValue");

		private static readonly IList<OutOfRangeLookupMethodType> valueList = new List<OutOfRangeLookupMethodType>();

		static OutOfRangeLookupMethodType()
		{
			valueList.Add(EXTRAPOLATE);
			valueList.Add(SET_MISSING);
			valueList.Add(USE_END_VALUE);
		}

		public enum InnerEnum
		{
			EXTRAPOLATE,
			SET_MISSING,
			USE_END_VALUE
		}

		public readonly InnerEnum innerEnumValue;
		private readonly string nameValue;
		private readonly int ordinalValue;
		private static int nextOrdinal = 0;

		private readonly string displayName;

		/// <summary>
		/// Name that should be displayed in choices, etc. </summary>
		/// <param name="displayName"> </param>
		private OutOfRangeLookupMethodType(string name, InnerEnum innerEnum, string displayName)
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
		public static OutOfRangeLookupMethodType valueOfIgnoreCase(string name)
		{
			OutOfRangeLookupMethodType[] values = values();
			foreach (OutOfRangeLookupMethodType t in values)
			{
				if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					return t;
				}
			}
			return null;
		}

		public static IList<OutOfRangeLookupMethodType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static OutOfRangeLookupMethodType valueOf(string name)
		{
			foreach (OutOfRangeLookupMethodType enumInstance in OutOfRangeLookupMethodType.valueList)
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