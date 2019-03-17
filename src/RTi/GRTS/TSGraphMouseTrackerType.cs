using System;
using System.Collections.Generic;

// TSGraphMouseTrackerType - the mouse tracker mode, which controls the behavior of the tracker

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
	/// The mouse tracker mode, which controls the behavior of the tracker.
	/// </summary>
	public sealed class TSGraphMouseTrackerType
	{
		/// <summary>
		/// Track the single nearest point to the mouse.
		/// </summary>
		public static readonly TSGraphMouseTrackerType NEAREST = new TSGraphMouseTrackerType("NEAREST", InnerEnum.NEAREST, "Nearest");
		/// <summary>
		/// Track single nearest point to the mouse, for only selected time series.
		/// </summary>
		public static readonly TSGraphMouseTrackerType NEAREST_SELECTED = new TSGraphMouseTrackerType("NEAREST_SELECTED", InnerEnum.NEAREST_SELECTED, "NearestSelected");
		/// <summary>
		/// Track the nearest point(s) to the time for the mouse.
		/// </summary>
		public static readonly TSGraphMouseTrackerType NEAREST_TIME = new TSGraphMouseTrackerType("NEAREST_TIME", InnerEnum.NEAREST_TIME, "NearestTime");
		/// <summary>
		/// Track the nearest point(s) to the time for the mouse, for only selected time series.
		/// </summary>
		public static readonly TSGraphMouseTrackerType NEAREST_TIME_SELECTED = new TSGraphMouseTrackerType("NEAREST_TIME_SELECTED", InnerEnum.NEAREST_TIME_SELECTED, "NearestTimeSelected");
		/// <summary>
		/// Do not track the mouse.
		/// </summary>
		public static readonly TSGraphMouseTrackerType NONE = new TSGraphMouseTrackerType("NONE", InnerEnum.NONE, "None");

		private static readonly IList<TSGraphMouseTrackerType> valueList = new List<TSGraphMouseTrackerType>();

		static TSGraphMouseTrackerType()
		{
			valueList.Add(NEAREST);
			valueList.Add(NEAREST_SELECTED);
			valueList.Add(NEAREST_TIME);
			valueList.Add(NEAREST_TIME_SELECTED);
			valueList.Add(NONE);
		}

		public enum InnerEnum
		{
			NEAREST,
			NEAREST_SELECTED,
			NEAREST_TIME,
			NEAREST_TIME_SELECTED,
			NONE
		}

		public readonly InnerEnum innerEnumValue;
		private readonly string nameValue;
		private readonly int ordinalValue;
		private static int nextOrdinal = 0;

		private readonly string displayName;

		/// <summary>
		/// Name that should be displayed in choices, etc. </summary>
		/// <param name="displayName"> </param>
		private TSGraphMouseTrackerType(string name, InnerEnum innerEnum, string displayName)
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
		public static TSGraphMouseTrackerType valueOfIgnoreCase(string name)
		{
			if (string.ReferenceEquals(name, null))
			{
				return null;
			}
			TSGraphMouseTrackerType[] values = values();
			foreach (TSGraphMouseTrackerType t in values)
			{
				if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					return t;
				}
			}
			return null;
		}

		public static IList<TSGraphMouseTrackerType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static TSGraphMouseTrackerType valueOf(string name)
		{
			foreach (TSGraphMouseTrackerType enumInstance in TSGraphMouseTrackerType.valueList)
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