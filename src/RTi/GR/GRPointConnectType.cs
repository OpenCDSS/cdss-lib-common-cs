using System;
using System.Collections.Generic;

// GRPointConnectType - enumeration of lines to connect points

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
	/// Ways to connect points when drawing, needed for example to represent instantaneous values, averages, etc.
	/// Handling of missing values with gaps is expected to occur as appropriate but is not indicated by this type.
	/// </summary>
	public sealed class GRPointConnectType
	{
		/// <summary>
		/// Connect points.
		/// </summary>
		public static readonly GRPointConnectType CONNECT = new GRPointConnectType("CONNECT", InnerEnum.CONNECT, "Connect");

		/// <summary>
		/// Step-function with line drawn backward from the point (point's y-coordinate is end of step). 
		/// </summary>
		public static readonly GRPointConnectType STEP_BACKWARD = new GRPointConnectType("STEP_BACKWARD", InnerEnum.STEP_BACKWARD, "StepBackward");

		/// <summary>
		/// Step-function with line drawn forward from the point (point's y-coordinate is start of step). 
		/// </summary>
		public static readonly GRPointConnectType STEP_FORWARD = new GRPointConnectType("STEP_FORWARD", InnerEnum.STEP_FORWARD, "StepForward");

		private static readonly IList<GRPointConnectType> valueList = new List<GRPointConnectType>();

		static GRPointConnectType()
		{
			valueList.Add(CONNECT);
			valueList.Add(STEP_BACKWARD);
			valueList.Add(STEP_FORWARD);
		}

		public enum InnerEnum
		{
			CONNECT,
			STEP_BACKWARD,
			STEP_FORWARD
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
		/// Construct a point connect type enumeration value. </summary>
		/// <param name="displayName"> name that should be displayed in choices, etc. </param>
		private GRPointConnectType(string name, InnerEnum innerEnum, string displayName)
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
		public static GRPointConnectType valueOfIgnoreCase(string name)
		{
			if (string.ReferenceEquals(name, null))
			{
				return null;
			}
			// Currently supported values
			foreach (GRPointConnectType t in values())
			{
				if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					return t;
				}
			}
			return null;
		}

		public static IList<GRPointConnectType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static GRPointConnectType valueOf(string name)
		{
			foreach (GRPointConnectType enumInstance in GRPointConnectType.valueList)
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