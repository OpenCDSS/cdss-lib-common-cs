using System;
using System.Collections.Generic;

// RegressionType - regression types

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
	/// Regression types.
	/// </summary>
	public sealed class RegressionType
	{
		/// <summary>
		/// Ordinary Least Squares regression.
		/// </summary>
		public static readonly RegressionType OLS_REGRESSION = new RegressionType("OLS_REGRESSION", InnerEnum.OLS_REGRESSION, "OLSRegression");
		/// <summary>
		/// Maintenance of Variation 1.
		/// </summary>
		public static readonly RegressionType MOVE1 = new RegressionType("MOVE1", InnerEnum.MOVE1, "MOVE1");
		/// <summary>
		/// Maintenance of Variation 2.
		/// </summary>
		public static readonly RegressionType MOVE2 = new RegressionType("MOVE2", InnerEnum.MOVE2, "MOVE2");

		private static readonly IList<RegressionType> valueList = new List<RegressionType>();

		static RegressionType()
		{
			valueList.Add(OLS_REGRESSION);
			valueList.Add(MOVE1);
			valueList.Add(MOVE2);
		}

		public enum InnerEnum
		{
			OLS_REGRESSION,
			MOVE1,
			MOVE2
		}

		public readonly InnerEnum innerEnumValue;
		private readonly string nameValue;
		private readonly int ordinalValue;
		private static int nextOrdinal = 0;

		private readonly string displayName;

		/// <summary>
		/// Name that should be displayed in choices, etc. </summary>
		/// <param name="displayName"> </param>
		private RegressionType(string name, InnerEnum innerEnum, string displayName)
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
		public static RegressionType valueOfIgnoreCase(string name)
		{
			RegressionType[] values = values();
			foreach (RegressionType t in values)
			{
				if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					return t;
				}
			}
			return null;
		}

		public static IList<RegressionType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static RegressionType valueOf(string name)
		{
			foreach (RegressionType enumInstance in RegressionType.valueList)
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