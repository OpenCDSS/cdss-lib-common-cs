using System;
using System.Collections.Generic;

// PropertyFileFormatType - format type for a property file,
// intended to be used as needed, but currently with no tight bundling to other code

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

namespace RTi.Util.IO
{
	/// <summary>
	/// Format type for a property file, intended to be used as needed, but currently with no tight
	/// bundling to other code.  For example, use the types for an applications configuration file.
	/// </summary>
	public sealed class PropertyFileFormatType
	{
	// TODO SAM 2012-07-27 Evaluate adding JSON, XML, and CSV
	/// <summary>
	/// Format of properties is PropertyName=Value, using double quotes for the value if necessary.
	/// </summary>
	public static readonly PropertyFileFormatType NAME_VALUE = new PropertyFileFormatType("NAME_VALUE", InnerEnum.NAME_VALUE, "NameValue");
	/// <summary>
	/// Format of properties is PropertyName=Type(Value), using double quotes for the value if necessary.
	/// Type is only used as appropriate to remove ambiguity of parsing to strings,
	/// for example DateTime("2010-01-15").
	/// </summary>
	public static readonly PropertyFileFormatType NAME_TYPE_VALUE = new PropertyFileFormatType("NAME_TYPE_VALUE", InnerEnum.NAME_TYPE_VALUE, "NameTypeValue");
	/// <summary>
	/// Format of properties is the same as NAME_TYPE_VALUE except that objects are formatted to be consistent
	/// with Python, which allows the property file to be used directly in Python to assign variables.
	/// </summary>
	public static readonly PropertyFileFormatType NAME_TYPE_VALUE_PYTHON = new PropertyFileFormatType("NAME_TYPE_VALUE_PYTHON", InnerEnum.NAME_TYPE_VALUE_PYTHON, "NameTypeValuePython");

	private static readonly IList<PropertyFileFormatType> valueList = new List<PropertyFileFormatType>();

	static PropertyFileFormatType()
	{
		valueList.Add(NAME_VALUE);
		valueList.Add(NAME_TYPE_VALUE);
		valueList.Add(NAME_TYPE_VALUE_PYTHON);
	}

	public enum InnerEnum
	{
		NAME_VALUE,
		NAME_TYPE_VALUE,
		NAME_TYPE_VALUE_PYTHON
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
	private PropertyFileFormatType(string name, InnerEnum innerEnum, string displayName)
	{
		this.displayName = displayName;

		nameValue = name;
		ordinalValue = nextOrdinal++;
		innerEnumValue = innerEnum;
	}

	/// <summary>
	/// Return the display name for the statistic.  This is usually the same as the
	/// value but using appropriate mixed case. </summary>
	/// <returns> the display name. </returns>
	public override string ToString()
	{
		return displayName;
	}

	/// <summary>
	/// Return the enumeration value given a string name (case-independent). </summary>
	/// <returns> the enumeration value given a string name (case-independent), or null if not matched. </returns>
	public static PropertyFileFormatType valueOfIgnoreCase(string name)
	{
		if (string.ReferenceEquals(name, null))
		{
			return null;
		}
		PropertyFileFormatType[] values = values();
		foreach (PropertyFileFormatType t in values)
		{
			if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				return t;
			}
		}
		return null;
	}


		public static IList<PropertyFileFormatType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static PropertyFileFormatType valueOf(string name)
		{
			foreach (PropertyFileFormatType enumInstance in PropertyFileFormatType.valueList)
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