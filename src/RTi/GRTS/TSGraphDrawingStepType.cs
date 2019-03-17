using System;
using System.Collections.Generic;

// TSGraphDrawingStepType - the steps that occur during drawing, to help code like annotations

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
	/// The steps that occur during drawing, to help code like annotations.
	/// </summary>
	public sealed class TSGraphDrawingStepType
	{
		/// <summary>
		/// Before drawing anything related to the back axes, which is underlying border (appropriate for Rectangle annotations that should be drawn under axes).
		/// </summary>
		public static readonly TSGraphDrawingStepType BEFORE_BACK_AXES = new TSGraphDrawingStepType("BEFORE_BACK_AXES", InnerEnum.BEFORE_BACK_AXES, "BeforeBackAxes");
		/// <summary>
		/// After drawing anything related to the back axes.
		/// </summary>
		public static readonly TSGraphDrawingStepType AFTER_BACK_AXES = new TSGraphDrawingStepType("AFTER_BACK_AXES", InnerEnum.AFTER_BACK_AXES, "AfterBackAxes");
		/// <summary>
		/// Before drawing any data (time series).
		/// </summary>
		public static readonly TSGraphDrawingStepType BEFORE_DATA = new TSGraphDrawingStepType("BEFORE_DATA", InnerEnum.BEFORE_DATA, "BeforeData");
		/// <summary>
		/// After drawing data (time series).
		/// </summary>
		public static readonly TSGraphDrawingStepType AFTER_DATA = new TSGraphDrawingStepType("AFTER_DATA", InnerEnum.AFTER_DATA, "AfterData");

		private static readonly IList<TSGraphDrawingStepType> valueList = new List<TSGraphDrawingStepType>();

		static TSGraphDrawingStepType()
		{
			valueList.Add(BEFORE_BACK_AXES);
			valueList.Add(AFTER_BACK_AXES);
			valueList.Add(BEFORE_DATA);
			valueList.Add(AFTER_DATA);
		}

		public enum InnerEnum
		{
			BEFORE_BACK_AXES,
			AFTER_BACK_AXES,
			BEFORE_DATA,
			AFTER_DATA
		}

		public readonly InnerEnum innerEnumValue;
		private readonly string nameValue;
		private readonly int ordinalValue;
		private static int nextOrdinal = 0;

		private readonly string displayName;

		/// <summary>
		/// Name that should be displayed in choices, etc. </summary>
		/// <param name="displayName"> </param>
		private TSGraphDrawingStepType(string name, InnerEnum innerEnum, string displayName)
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
		public static TSGraphDrawingStepType valueOfIgnoreCase(string name)
		{
			if (string.ReferenceEquals(name, null))
			{
				return null;
			}
			TSGraphDrawingStepType[] values = values();
			foreach (TSGraphDrawingStepType t in values)
			{
				if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					return t;
				}
			}
			return null;
		}

		public static IList<TSGraphDrawingStepType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static TSGraphDrawingStepType valueOf(string name)
		{
			foreach (TSGraphDrawingStepType enumInstance in TSGraphDrawingStepType.valueList)
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