using System;
using System.Collections.Generic;

// GRAxisDirectionType - graph axis directions

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
	/// Graph axis directions.
	/// </summary>
	public sealed class GRAxisDirectionType
	{
		/// <summary>
		/// Axis has normal direction (e.g., Y-axis values on simple graph increase vertically).
		/// </summary>
		public static readonly GRAxisDirectionType NORMAL = new GRAxisDirectionType("NORMAL", InnerEnum.NORMAL, "Normal");

		/// <summary>
		/// Reversed axis. 
		/// </summary>
		public static readonly GRAxisDirectionType REVERSE = new GRAxisDirectionType("REVERSE", InnerEnum.REVERSE, "Reverse");

		private static readonly IList<GRAxisDirectionType> valueList = new List<GRAxisDirectionType>();

		static GRAxisDirectionType()
		{
			valueList.Add(NORMAL);
			valueList.Add(REVERSE);
		}

		public enum InnerEnum
		{
			NORMAL,
			REVERSE
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
		private GRAxisDirectionType(string name, InnerEnum innerEnum, string displayName)
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
		public static GRAxisDirectionType valueOfIgnoreCase(string name)
		{
			if (string.ReferenceEquals(name, null))
			{
				return null;
			}
			// Currently supported values
			foreach (GRAxisDirectionType t in values())
			{
				if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					return t;
				}
			}
			return null;
		}

		public static IList<GRAxisDirectionType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static GRAxisDirectionType valueOf(string name)
		{
			foreach (GRAxisDirectionType enumInstance in GRAxisDirectionType.valueList)
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