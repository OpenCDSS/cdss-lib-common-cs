using System;
using System.Collections.Generic;

// NumberOfEquationsType - number of equations, for example when performing a regression analysis.

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
	/// Number of equations, for example when performing a regression analysis.
	/// </summary>
	public sealed class NumberOfEquationsType
	{
		/// <summary>
		/// One equation for entire sample.
		/// </summary>
		public static readonly NumberOfEquationsType ONE_EQUATION = new NumberOfEquationsType("ONE_EQUATION", InnerEnum.ONE_EQUATION, "OneEquation");
		/// <summary>
		/// Monthly equations.
		/// </summary>
		public static readonly NumberOfEquationsType MONTHLY_EQUATIONS = new NumberOfEquationsType("MONTHLY_EQUATIONS", InnerEnum.MONTHLY_EQUATIONS, "MonthlyEquations");

		private static readonly IList<NumberOfEquationsType> valueList = new List<NumberOfEquationsType>();

		static NumberOfEquationsType()
		{
			valueList.Add(ONE_EQUATION);
			valueList.Add(MONTHLY_EQUATIONS);
		}

		public enum InnerEnum
		{
			ONE_EQUATION,
			MONTHLY_EQUATIONS
		}

		public readonly InnerEnum innerEnumValue;
		private readonly string nameValue;
		private readonly int ordinalValue;
		private static int nextOrdinal = 0;

		private readonly string displayName;

		/// <summary>
		/// Name that should be displayed in choices, etc. </summary>
		/// <param name="displayName"> </param>
		private NumberOfEquationsType(string name, InnerEnum innerEnum, string displayName)
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
		public static NumberOfEquationsType valueOfIgnoreCase(string name)
		{
			NumberOfEquationsType[] values = values();
			foreach (NumberOfEquationsType t in values)
			{
				if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					return t;
				}
			}
			return null;
		}

		public static IList<NumberOfEquationsType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static NumberOfEquationsType valueOf(string name)
		{
			foreach (NumberOfEquationsType enumInstance in NumberOfEquationsType.valueList)
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