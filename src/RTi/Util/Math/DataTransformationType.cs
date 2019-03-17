using System;
using System.Collections.Generic;

// DataTransformationType - data transformations that may be applied to data before analysis

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

namespace RTi.Util.Math
{
	/// <summary>
	/// Data transformations that may be applied to data before analysis,
	/// for example before performing a regression analysis.
	/// </summary>
	public sealed class DataTransformationType
	{
		/// <summary>
		/// Data values are transformed by log10() prior to analysis.
		/// </summary>
		public static readonly DataTransformationType LOG = new DataTransformationType("LOG", InnerEnum.LOG, "Log");
		/// <summary>
		/// Data values are not transformed prior to analysis.
		/// </summary>
		public static readonly DataTransformationType NONE = new DataTransformationType("NONE", InnerEnum.NONE, "None");

		private static readonly IList<DataTransformationType> valueList = new List<DataTransformationType>();

		static DataTransformationType()
		{
			valueList.Add(LOG);
			valueList.Add(NONE);
		}

		public enum InnerEnum
		{
			LOG,
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
		private DataTransformationType(string name, InnerEnum innerEnum, string displayName)
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
		public static DataTransformationType valueOfIgnoreCase(string name)
		{
			DataTransformationType[] values = values();
			foreach (DataTransformationType t in values)
			{
				if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					return t;
				}
			}
			return null;
		}

		public static IList<DataTransformationType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static DataTransformationType valueOf(string name)
		{
			foreach (DataTransformationType enumInstance in DataTransformationType.valueList)
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