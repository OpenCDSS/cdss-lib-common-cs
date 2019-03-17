using System;
using System.Collections.Generic;

// TSProductFormatType - format for TSProduct text representation, indicating formatting before display or writing to file

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

namespace RTi.GRTS
{
	/// <summary>
	/// Format for TSProduct text representation, indicating formatting before display or writing to file.
	/// </summary>
	public sealed class TSProductFormatType
	{
		/// <summary>
		/// Legacy properties list, similar to INI file.
		/// </summary>
		public static readonly TSProductFormatType PROPERTIES = new TSProductFormatType("PROPERTIES", InnerEnum.PROPERTIES, "Properties");
		/// <summary>
		/// JSON without line breaks or pretty formatting.
		/// </summary>
		public static readonly TSProductFormatType JSON_COMPACT = new TSProductFormatType("JSON_COMPACT", InnerEnum.JSON_COMPACT, "JSONCompact");
		/// <summary>
		/// JSON with line breaks and pretty formatting.
		/// </summary>
		public static readonly TSProductFormatType JSON_PRETTY = new TSProductFormatType("JSON_PRETTY", InnerEnum.JSON_PRETTY, "JSONPretty");

		private static readonly IList<TSProductFormatType> valueList = new List<TSProductFormatType>();

		static TSProductFormatType()
		{
			valueList.Add(PROPERTIES);
			valueList.Add(JSON_COMPACT);
			valueList.Add(JSON_PRETTY);
		}

		public enum InnerEnum
		{
			PROPERTIES,
			JSON_COMPACT,
			JSON_PRETTY
		}

		public readonly InnerEnum innerEnumValue;
		private readonly string nameValue;
		private readonly int ordinalValue;
		private static int nextOrdinal = 0;

		private readonly string displayName;

		/// <summary>
		/// Name that should be displayed in choices, etc. </summary>
		/// <param name="displayName"> </param>
		private TSProductFormatType(string name, InnerEnum innerEnum, string displayName)
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
		public static TSProductFormatType valueOfIgnoreCase(string name)
		{
			TSProductFormatType[] values = values();
			foreach (TSProductFormatType t in values)
			{
				if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					return t;
				}
			}
			return null;
		}

		public static IList<TSProductFormatType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static TSProductFormatType valueOf(string name)
		{
			foreach (TSProductFormatType enumInstance in TSProductFormatType.valueList)
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