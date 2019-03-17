using System;
using System.Collections.Generic;

// TSViewType - time series view window types, used to manage windows

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
	/// Time series view window types, used to manage windows.
	/// </summary>
	public sealed class TSViewType
	{
		/// <summary>
		/// Graph view.
		/// </summary>
		public static readonly TSViewType GRAPH = new TSViewType("GRAPH", InnerEnum.GRAPH, "Graph");
		/// <summary>
		/// Properties view, currently only used with graph but may properties may be used with other views.
		/// </summary>
		public static readonly TSViewType PROPERTIES = new TSViewType("PROPERTIES", InnerEnum.PROPERTIES, "Properties");
		/// <summary>
		/// Properties view (not visible), used to programatically make quick changes to properties.
		/// </summary>
		public static readonly TSViewType PROPERTIES_HIDDEN = new TSViewType("PROPERTIES_HIDDEN", InnerEnum.PROPERTIES_HIDDEN, "PropertiesHidden");
		/// <summary>
		/// Summary view.
		/// </summary>
		public static readonly TSViewType SUMMARY = new TSViewType("SUMMARY", InnerEnum.SUMMARY, "Summary");
		/// <summary>
		/// Table view.
		/// </summary>
		public static readonly TSViewType TABLE = new TSViewType("TABLE", InnerEnum.TABLE, "Table");

		private static readonly IList<TSViewType> valueList = new List<TSViewType>();

		static TSViewType()
		{
			valueList.Add(GRAPH);
			valueList.Add(PROPERTIES);
			valueList.Add(PROPERTIES_HIDDEN);
			valueList.Add(SUMMARY);
			valueList.Add(TABLE);
		}

		public enum InnerEnum
		{
			GRAPH,
			PROPERTIES,
			PROPERTIES_HIDDEN,
			SUMMARY,
			TABLE
		}

		public readonly InnerEnum innerEnumValue;
		private readonly string nameValue;
		private readonly int ordinalValue;
		private static int nextOrdinal = 0;

		private readonly string displayName;

		/// <summary>
		/// Name that should be displayed in choices, etc. </summary>
		/// <param name="displayName"> </param>
		private TSViewType(string name, InnerEnum innerEnum, string displayName)
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
		public static TSViewType valueOfIgnoreCase(string name)
		{
			TSViewType[] values = values();
			foreach (TSViewType t in values)
			{
				if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					return t;
				}
			}
			return null;
		}

		public static IList<TSViewType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static TSViewType valueOf(string name)
		{
			foreach (TSViewType enumInstance in TSViewType.valueList)
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