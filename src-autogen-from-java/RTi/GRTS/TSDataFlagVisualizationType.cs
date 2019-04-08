using System;
using System.Collections.Generic;

// TSDataFlagVisualizationType - indicate how data flags should be visualized in table displays

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
	/// Indicate how data flags should be visualized in table displays
	/// </summary>
	public sealed class TSDataFlagVisualizationType
	{
		/// <summary>
		/// Data flags are not shown.
		/// </summary>
		public static readonly TSDataFlagVisualizationType NOT_SHOWN = new TSDataFlagVisualizationType("NOT_SHOWN", InnerEnum.NOT_SHOWN, "Not shown");
		/// <summary>
		/// Data flags are shown in separate column of table.
		/// </summary>
		public static readonly TSDataFlagVisualizationType SEPARATE_COLUMN = new TSDataFlagVisualizationType("SEPARATE_COLUMN", InnerEnum.SEPARATE_COLUMN, "Separate column");
		/// <summary>
		/// Data flags are shown as superscript on data values.
		/// </summary>
		public static readonly TSDataFlagVisualizationType SUPERSCRIPT = new TSDataFlagVisualizationType("SUPERSCRIPT", InnerEnum.SUPERSCRIPT, "Superscript");

		private static readonly IList<TSDataFlagVisualizationType> valueList = new List<TSDataFlagVisualizationType>();

		static TSDataFlagVisualizationType()
		{
			valueList.Add(NOT_SHOWN);
			valueList.Add(SEPARATE_COLUMN);
			valueList.Add(SUPERSCRIPT);
		}

		public enum InnerEnum
		{
			NOT_SHOWN,
			SEPARATE_COLUMN,
			SUPERSCRIPT
		}

		public readonly InnerEnum innerEnumValue;
		private readonly string nameValue;
		private readonly int ordinalValue;
		private static int nextOrdinal = 0;

		private readonly string displayName;

		/// <summary>
		/// Name that should be displayed in choices, etc. </summary>
		/// <param name="displayName"> </param>
		private TSDataFlagVisualizationType(string name, InnerEnum innerEnum, string displayName)
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
		public static TSDataFlagVisualizationType valueOfIgnoreCase(string name)
		{
			if (string.ReferenceEquals(name, null))
			{
				return null;
			}
			TSDataFlagVisualizationType[] values = values();
			foreach (TSDataFlagVisualizationType t in values)
			{
				if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					return t;
				}
			}
			return null;
		}

		public static IList<TSDataFlagVisualizationType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static TSDataFlagVisualizationType valueOf(string name)
		{
			foreach (TSDataFlagVisualizationType enumInstance in TSDataFlagVisualizationType.valueList)
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