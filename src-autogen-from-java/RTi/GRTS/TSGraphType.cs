using System;
using System.Collections.Generic;

// TSGraphType - time series graph types to be used for graph properties

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
	/// Time series graph types to be used for graph properties.
	/// This can be used for a graph space, or for individual time series in a graph space.
	/// Other properties influence the actual appearance (e.g., log/linear axis in conjunction with line graph).
	/// </summary>
	public sealed class TSGraphType
	{
		public static readonly TSGraphType UNKNOWN = new TSGraphType("UNKNOWN", InnerEnum.UNKNOWN, "Unknown");
		/// <summary>
		/// Used with right-axis graph type to indicate no right-axis graph.
		/// </summary>
		public static readonly TSGraphType NONE = new TSGraphType("NONE", InnerEnum.NONE, "None");
		public static readonly TSGraphType AREA = new TSGraphType("AREA", InnerEnum.AREA, "Area");
		public static readonly TSGraphType AREA_STACKED = new TSGraphType("AREA_STACKED", InnerEnum.AREA_STACKED, "AreaStacked");
		public static readonly TSGraphType BAR = new TSGraphType("BAR", InnerEnum.BAR, "Bar");
		public static readonly TSGraphType DOUBLE_MASS = new TSGraphType("DOUBLE_MASS", InnerEnum.DOUBLE_MASS, "Double-Mass");
		public static readonly TSGraphType DURATION = new TSGraphType("DURATION", InnerEnum.DURATION, "Duration");
		public static readonly TSGraphType LINE = new TSGraphType("LINE", InnerEnum.LINE, "Line");
		public static readonly TSGraphType PERIOD = new TSGraphType("PERIOD", InnerEnum.PERIOD, "PeriodOfRecord");
		public static readonly TSGraphType POINT = new TSGraphType("POINT", InnerEnum.POINT, "Point");
		public static readonly TSGraphType PREDICTED_VALUE = new TSGraphType("PREDICTED_VALUE", InnerEnum.PREDICTED_VALUE, "PredictedValue");
		public static readonly TSGraphType PREDICTED_VALUE_RESIDUAL = new TSGraphType("PREDICTED_VALUE_RESIDUAL", InnerEnum.PREDICTED_VALUE_RESIDUAL, "PredictedValueResidual");
		public static readonly TSGraphType RASTER = new TSGraphType("RASTER", InnerEnum.RASTER, "Raster");
		public static readonly TSGraphType XY_SCATTER = new TSGraphType("XY_SCATTER", InnerEnum.XY_SCATTER, "XY-Scatter");

		private static readonly IList<TSGraphType> valueList = new List<TSGraphType>();

		static TSGraphType()
		{
			valueList.Add(UNKNOWN);
			valueList.Add(NONE);
			valueList.Add(AREA);
			valueList.Add(AREA_STACKED);
			valueList.Add(BAR);
			valueList.Add(DOUBLE_MASS);
			valueList.Add(DURATION);
			valueList.Add(LINE);
			valueList.Add(PERIOD);
			valueList.Add(POINT);
			valueList.Add(PREDICTED_VALUE);
			valueList.Add(PREDICTED_VALUE_RESIDUAL);
			valueList.Add(RASTER);
			valueList.Add(XY_SCATTER);
		}

		public enum InnerEnum
		{
			UNKNOWN,
			NONE,
			AREA,
			AREA_STACKED,
			BAR,
			DOUBLE_MASS,
			DURATION,
			LINE,
			PERIOD,
			POINT,
			PREDICTED_VALUE,
			PREDICTED_VALUE_RESIDUAL,
			RASTER,
			XY_SCATTER
		}

		public readonly InnerEnum innerEnumValue;
		private readonly string nameValue;
		private readonly int ordinalValue;
		private static int nextOrdinal = 0;

		private readonly string displayName;

		/// <summary>
		/// Name that should be displayed in choices, etc. </summary>
		/// <param name="displayName"> </param>
		private TSGraphType(string name, InnerEnum innerEnum, string displayName)
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
		public static TSGraphType valueOfIgnoreCase(string name)
		{
			if (string.ReferenceEquals(name, null))
			{
				return null;
			}
			TSGraphType[] values = values();
			foreach (TSGraphType t in values)
			{
				if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					return t;
				}
			}
			return null;
		}

		public static IList<TSGraphType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static TSGraphType valueOf(string name)
		{
			foreach (TSGraphType enumInstance in TSGraphType.valueList)
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