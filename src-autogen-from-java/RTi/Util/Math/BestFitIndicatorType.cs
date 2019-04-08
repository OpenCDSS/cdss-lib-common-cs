using System;
using System.Collections.Generic;

// BestFitIndicatorType - best fit indicators that may be determined for an analysis

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
	/// Best fit indicators that may be determined, for example when performing a regression analysis.
	/// </summary>
	public sealed class BestFitIndicatorType
	{
		/// <summary>
		/// Nash-Sutcliffe model efficiency coefficient.
		/// </summary>
		public static readonly BestFitIndicatorType NASH_SUTCLIFFE = new BestFitIndicatorType("NASH_SUTCLIFFE", InnerEnum.NASH_SUTCLIFFE, "NashSutcliffeEfficiency");
		/// <summary>
		/// Correlation coefficient.
		/// </summary>
		public static readonly BestFitIndicatorType R = new BestFitIndicatorType("R", InnerEnum.R, "R");
		/// <summary>
		/// Standard Error of Prediction, defined as the square root
		/// of the sum of squared differences between the known dependent value, and the value
		/// determined from the equation used to estimate the data.
		/// </summary>
		public static readonly BestFitIndicatorType SEP = new BestFitIndicatorType("SEP", InnerEnum.SEP, "SEP");
		/// <summary>
		/// The following is used with TSTool's Mixed Station Analysis and indicates that monthly
		/// equations are used but the error is the total of all months.
		/// </summary>
		public static readonly BestFitIndicatorType SEP_TOTAL = new BestFitIndicatorType("SEP_TOTAL", InnerEnum.SEP_TOTAL, "SEPTotal");
		/// <summary>
		/// The following is used for filling non-mixed station, to allow for the same code without requiring sorting
		/// </summary>
		public static readonly BestFitIndicatorType NONE = new BestFitIndicatorType("NONE", InnerEnum.NONE, "None");

		private static readonly IList<BestFitIndicatorType> valueList = new List<BestFitIndicatorType>();

		static BestFitIndicatorType()
		{
			valueList.Add(NASH_SUTCLIFFE);
			valueList.Add(R);
			valueList.Add(SEP);
			valueList.Add(SEP_TOTAL);
			valueList.Add(NONE);
		}

		public enum InnerEnum
		{
			NASH_SUTCLIFFE,
			R,
			SEP,
			SEP_TOTAL,
			NONE
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
		/// Construct an enumeration value. </summary>
		/// <param name="displayName"> name that should be displayed in choices, etc. </param>
		private BestFitIndicatorType(string name, InnerEnum innerEnum, string displayName)
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
		public static BestFitIndicatorType valueOfIgnoreCase(string name)
		{
			BestFitIndicatorType[] values = values();
			foreach (BestFitIndicatorType t in values)
			{
				if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					return t;
				}
			}
			return null;
		}

		public static IList<BestFitIndicatorType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static BestFitIndicatorType valueOf(string name)
		{
			foreach (BestFitIndicatorType enumInstance in BestFitIndicatorType.valueList)
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