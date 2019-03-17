using System;
using System.Collections.Generic;

// DataTableFunctionType - enumeration that defines data table function types

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

namespace RTi.Util.Table
{
	/// <summary>
	/// This enumeration defines data table function types, which are functions that
	/// are used to assign data to table cells.
	/// </summary>
	public sealed class DataTableFunctionType
	{
		/// <summary>
		/// The table row (1+).
		/// </summary>
		public static readonly DataTableFunctionType ROW = new DataTableFunctionType("ROW", InnerEnum.ROW, "Row");
		/// <summary>
		/// The table row (0+).
		/// </summary>
		public static readonly DataTableFunctionType ROW0 = new DataTableFunctionType("ROW0", InnerEnum.ROW0, "Row0");

		private static readonly IList<DataTableFunctionType> valueList = new List<DataTableFunctionType>();

		static DataTableFunctionType()
		{
			valueList.Add(ROW);
			valueList.Add(ROW0);
		}

		public enum InnerEnum
		{
			ROW,
			ROW0
		}

		public readonly InnerEnum innerEnumValue;
		private readonly string nameValue;
		private readonly int ordinalValue;
		private static int nextOrdinal = 0;

		/// <summary>
		/// The name that should be displayed in UIs and reports.
		/// </summary>
		private readonly string displayName;

		/// <summary>
		/// Construct a time series statistic enumeration value.
		/// </summary>
		/// <param name="displayName">
		///            name that should be displayed in choices, etc. </param>
		private DataTableFunctionType(string name, InnerEnum innerEnum, string displayName)
		{
			this.displayName = displayName;

			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

		/// <summary>
		/// Return the display name for the type. This is usually similar to the
		/// value but using appropriate mixed case.
		/// </summary>
		/// <returns> the display name. </returns>
		public override string ToString()
		{
			return displayName;
		}

		/// <summary>
		/// Return the enumeration value given a string name (case-independent).
		/// </summary>
		/// <returns> the enumeration value given a string name (case-independent), or
		///         null if not matched. </returns>
		public static DataTableFunctionType valueOfIgnoreCase(string name)
		{
			if (string.ReferenceEquals(name, null))
			{
				return null;
			}
			DataTableFunctionType[] values = values();
			foreach (DataTableFunctionType t in values)
			{
				if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					return t;
				}
			}
			return null;
		}


		public static IList<DataTableFunctionType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static DataTableFunctionType valueOf(string name)
		{
			foreach (DataTableFunctionType enumInstance in DataTableFunctionType.valueList)
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