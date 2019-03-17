using System;
using System.Collections.Generic;

// GRLineStyleType - enumeration of line styles

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
	/// Line styles.  If a pattern is required, then other data will be needed to specify the pattern.
	/// </summary>
	public sealed class GRLineStyleType
	{
		/// <summary>
		/// A sequence of short lines separated by equal length spaces.
		/// </summary>
		public static readonly GRLineStyleType DASHED = new GRLineStyleType("DASHED", InnerEnum.DASHED, "Dashed");

		/// <summary>
		/// Solid line. 
		/// </summary>
		public static readonly GRLineStyleType SOLID = new GRLineStyleType("SOLID", InnerEnum.SOLID, "Solid");

		private static readonly IList<GRLineStyleType> valueList = new List<GRLineStyleType>();

		static GRLineStyleType()
		{
			valueList.Add(DASHED);
			valueList.Add(SOLID);
		}

		public enum InnerEnum
		{
			DASHED,
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
		private GRLineStyleType(string name, InnerEnum innerEnum, string displayName)
		{
			this.displayName = displayName;

			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

		/// <summary>
		/// Equals method to prevent common programming error of using the equals method instead of ==.
		/// </summary>
		public bool equals(string lineStyleType)
		{
			if (lineStyleType.Equals(this.displayName, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Return the display name for the line style type.  This is usually the same as the
		/// value but using appropriate mixed case. </summary>
		/// <returns> the display name. </returns>
		public override string ToString()
		{
			return displayName;
		}

		/// <summary>
		/// Return the enumeration value given a string name (case-independent). </summary>
		/// <returns> the enumeration value given a string name (case-independent), or null if not matched. </returns>
		public static GRLineStyleType valueOfIgnoreCase(string name)
		{
			if (string.ReferenceEquals(name, null))
			{
				return null;
			}
			// Currently supported values
			foreach (GRLineStyleType t in values())
			{
				if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					return t;
				}
			}
			return null;
		}

		public static IList<GRLineStyleType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static GRLineStyleType valueOf(string name)
		{
			foreach (GRLineStyleType enumInstance in GRLineStyleType.valueList)
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