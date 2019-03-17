using System;
using System.Collections.Generic;

// GRArrowStyleType - enumeration for arrow types

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

namespace RTi.GR
{
	/// <summary>
	/// Arrow styles.  If a size is required, then other data will be needed to specified in addition to the style.
	/// </summary>
	public sealed class GRArrowStyleType
	{
		/// <summary>
		/// No arrow.
		/// </summary>
		public static readonly GRArrowStyleType NONE = new GRArrowStyleType("NONE", InnerEnum.NONE, "None");

		/// <summary>
		/// Solid arrowhead. 
		/// </summary>
		public static readonly GRArrowStyleType SOLID = new GRArrowStyleType("SOLID", InnerEnum.SOLID, "Solid");

		private static readonly IList<GRArrowStyleType> valueList = new List<GRArrowStyleType>();

		static GRArrowStyleType()
		{
			valueList.Add(NONE);
			valueList.Add(SOLID);
		}

		public enum InnerEnum
		{
			NONE,
			SOLID
		}

		public readonly InnerEnum innerEnumValue;
		private readonly string nameValue;
		private readonly int ordinalValue;
		private static int nextOrdinal = 0;

		/// <summary>
		/// The string name that should be displayed.
		/// </summary>
		private readonly string displayName;

		/// <summary>
		/// Construct a time series list type enumeration value. </summary>
		/// <param name="displayName"> name that should be displayed in choices, etc. </param>
		private GRArrowStyleType(string name, InnerEnum innerEnum, string displayName)
		{
			this.displayName = displayName;

			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

		/// <summary>
		/// Equals method to prevent common programming error of using the equals method instead of ==.
		/// </summary>
		public bool equals(string arrowStyleType)
		{
			if (arrowStyleType.Equals(this.displayName, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Return the display name for the arrow style type.  This is usually the same as the
		/// value but using appropriate mixed case. </summary>
		/// <returns> the display name. </returns>
		public override string ToString()
		{
			return displayName;
		}

		/// <summary>
		/// Return the enumeration value given a string name (case-independent). </summary>
		/// <returns> the enumeration value given a string name (case-independent), or null if not matched. </returns>
		public static GRArrowStyleType valueOfIgnoreCase(string name)
		{
			if (string.ReferenceEquals(name, null))
			{
				return null;
			}
			// Currently supported values
			foreach (GRArrowStyleType t in values())
			{
				if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					return t;
				}
			}
			return null;
		}

		public static IList<GRArrowStyleType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static GRArrowStyleType valueOf(string name)
		{
			foreach (GRArrowStyleType enumInstance in GRArrowStyleType.valueList)
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