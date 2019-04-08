using System;
using System.Collections.Generic;

// TSFunctionType - enumeration that defines time series function types, which are functions that are used to assign data to time series

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

namespace RTi.TS
{
	/// <summary>
	/// This enumeration defines time series function types, which are functions that are used to assign data
	/// to time series.  Consequently, x = f(y) indicates y as the independent date/time and x being the generated time series value.
	/// </summary>
	public sealed class TSFunctionType
	{
		/// <summary>
		/// Assign the year to the whole number part of the value.
		/// </summary>
		public static readonly TSFunctionType DATE_YYYY = new TSFunctionType("DATE_YYYY", InnerEnum.DATE_YYYY, "DateYYYY");
		/// <summary>
		/// Assign the year and month to the whole number part of the value.
		/// </summary>
		public static readonly TSFunctionType DATE_YYYYMM = new TSFunctionType("DATE_YYYYMM", InnerEnum.DATE_YYYYMM, "DateYYYYMM");
		/// <summary>
		/// Assign the year, month, and day to the whole number part of the value.
		/// </summary>
		public static readonly TSFunctionType DATE_YYYYMMDD = new TSFunctionType("DATE_YYYYMMDD", InnerEnum.DATE_YYYYMMDD, "DateYYYYMMDD");
		/// <summary>
		/// Assign the date and time (to hour) to the whole number part of the value and the hour as the
		/// fraction (hour 1 = .01, hour 24 = .24).
		/// </summary>
		public static readonly TSFunctionType DATETIME_YYYYMMDD_HH = new TSFunctionType("DATETIME_YYYYMMDD_HH", InnerEnum.DATETIME_YYYYMMDD_HH, "DateTimeYYYYMMDD_hh");
		/// <summary>
		/// Assign the date to the whole number part of the value and the hour and minute as the
		/// fraction (hour 1, minute 1 = .0101, hour 24, 59 = .2459).
		/// </summary>
		public static readonly TSFunctionType DATETIME_YYYYMMDD_HHMM = new TSFunctionType("DATETIME_YYYYMMDD_HHMM", InnerEnum.DATETIME_YYYYMMDD_HHMM, "DateTimeYYYYMMDD_hhmm");
		/// <summary>
		/// Assign a random number in the range 0.0 to 1.0.
		/// </summary>
		public static readonly TSFunctionType RANDOM_0_1 = new TSFunctionType("RANDOM_0_1", InnerEnum.RANDOM_0_1, "Random_0_1");
		/// <summary>
		/// Assign a random number in the range 0.0 to 1000.0.
		/// </summary>
		public static readonly TSFunctionType RANDOM_0_1000 = new TSFunctionType("RANDOM_0_1000", InnerEnum.RANDOM_0_1000, "Random_0_1000");

		private static readonly IList<TSFunctionType> valueList = new List<TSFunctionType>();

		static TSFunctionType()
		{
			valueList.Add(DATE_YYYY);
			valueList.Add(DATE_YYYYMM);
			valueList.Add(DATE_YYYYMMDD);
			valueList.Add(DATETIME_YYYYMMDD_HH);
			valueList.Add(DATETIME_YYYYMMDD_HHMM);
			valueList.Add(RANDOM_0_1);
			valueList.Add(RANDOM_0_1000);
		}

		public enum InnerEnum
		{
			DATE_YYYY,
			DATE_YYYYMM,
			DATE_YYYYMMDD,
			DATETIME_YYYYMMDD_HH,
			DATETIME_YYYYMMDD_HHMM,
			RANDOM_0_1,
			RANDOM_0_1000
		}

		public readonly InnerEnum innerEnumValue;
		private readonly string nameValue;
		private readonly int ordinalValue;
		private static int nextOrdinal = 0;

		/// <summary>
		/// The name that should be displayed when the best fit type is used in UIs and reports.
		/// </summary>
		private readonly string displayName;

		/// <summary>
		/// Construct a time series statistic enumeration value. </summary>
		/// <param name="displayName"> name that should be displayed in choices, etc. </param>
		private TSFunctionType(string name, InnerEnum innerEnum, string displayName)
		{
			this.displayName = displayName;

			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

	/// <summary>
	/// Return the display name for the type.  This is usually similar to the
	/// value but using appropriate mixed case. </summary>
	/// <returns> the display name. </returns>
	public override string ToString()
	{
		return displayName;
	}

	/// <summary>
	/// Return the enumeration value given a string name (case-independent). </summary>
	/// <returns> the enumeration value given a string name (case-independent), or null if not matched. </returns>
	public static TSFunctionType valueOfIgnoreCase(string name)
	{
		if (string.ReferenceEquals(name, null))
		{
			return null;
		}
		TSFunctionType[] values = values();
		foreach (TSFunctionType t in values)
		{
			if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				return t;
			}
		}
		return null;
	}


		public static IList<TSFunctionType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static TSFunctionType valueOf(string name)
		{
			foreach (TSFunctionType enumInstance in TSFunctionType.valueList)
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