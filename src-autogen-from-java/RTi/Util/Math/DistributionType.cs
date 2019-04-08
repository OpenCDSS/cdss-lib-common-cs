using System;
using System.Collections.Generic;

// DistributionType - distribution types, can be used with statistics such as plotting position.

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
	/// Distribution types, can be used with statistics such as plotting position.
	/// The list should not be used generically but should be selected from as appropriate.
	/// </summary>
	public sealed class DistributionType
	{
		/// <summary>
		///   Emperical.
		/// </summary>
		public static readonly DistributionType EMPERICAL = new DistributionType("EMPERICAL", InnerEnum.EMPERICAL, "Emperical");
		/// <summary>
		/// Gringorten.
		/// TODO SAM 2016-06-17 This is actually not a distribution - need to transition RunningStatisticTimeSeries to Emperical with Gringorten plotting position
		/// </summary>
		public static readonly DistributionType GRINGORTEN = new DistributionType("GRINGORTEN", InnerEnum.GRINGORTEN, "Gringorten");
		/// <summary>
		/// Log-Normal.
		/// </summary>
		public static readonly DistributionType LOG_NORMAL = new DistributionType("LOG_NORMAL", InnerEnum.LOG_NORMAL, "LogNormal");
		/// <summary>
		/// Log Pearson Type 3.
		/// </summary>
		public static readonly DistributionType LOG_PEARSON_TYPE3 = new DistributionType("LOG_PEARSON_TYPE3", InnerEnum.LOG_PEARSON_TYPE3, "LogPearsonType3");
		/// <summary>
		/// Normal.
		/// </summary>
		public static readonly DistributionType NORMAL = new DistributionType("NORMAL", InnerEnum.NORMAL, "Normal");
		/// <summary>
		/// Sample data (for simple statistics, no population distribution).
		/// </summary>
		public static readonly DistributionType SAMPLE = new DistributionType("SAMPLE", InnerEnum.SAMPLE, "Sample");
		/// <summary>
		/// Wakeby.
		/// </summary>
		public static readonly DistributionType WAKEBY = new DistributionType("WAKEBY", InnerEnum.WAKEBY, "Wakeby");
		/// <summary>
		/// Weibull.
		/// </summary>
		public static readonly DistributionType WEIBULL = new DistributionType("WEIBULL", InnerEnum.WEIBULL, "Weibull");

		private static readonly IList<DistributionType> valueList = new List<DistributionType>();

		static DistributionType()
		{
			valueList.Add(EMPERICAL);
			valueList.Add(GRINGORTEN);
			valueList.Add(LOG_NORMAL);
			valueList.Add(LOG_PEARSON_TYPE3);
			valueList.Add(NORMAL);
			valueList.Add(SAMPLE);
			valueList.Add(WAKEBY);
			valueList.Add(WEIBULL);
		}

		public enum InnerEnum
		{
			EMPERICAL,
			GRINGORTEN,
			LOG_NORMAL,
			LOG_PEARSON_TYPE3,
			NORMAL,
			SAMPLE,
			WAKEBY,
			WEIBULL
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
		private DistributionType(string name, InnerEnum innerEnum, string displayName)
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
		public static DistributionType valueOfIgnoreCase(string name)
		{
			if (string.ReferenceEquals(name, null))
			{
				return null;
			}
			// Convert legacy to new
			if (name.Equals("SampleData", StringComparison.OrdinalIgnoreCase))
			{
				name = "" + SAMPLE;
			}
			DistributionType[] values = values();
			foreach (DistributionType t in values)
			{
				if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					return t;
				}
			}
			return null;
		}

		public static IList<DistributionType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static DistributionType valueOf(string name)
		{
			foreach (DistributionType enumInstance in DistributionType.valueList)
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