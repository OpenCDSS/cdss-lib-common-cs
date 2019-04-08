using System;
using System.Collections.Generic;

// FileWriteModeType - mode for writing files, intended to be used as needed, but currently with no tight bundling to other code.

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
	/// Mode for writing files, intended to be used as needed, but currently with no tight
	/// bundling to other code.
	/// </summary>
	public sealed class FileWriteModeType
	{
	/// <summary>
	/// Append to the end of the file.
	/// </summary>
	public static readonly FileWriteModeType APPEND = new FileWriteModeType("APPEND", InnerEnum.APPEND, "Append");
	/// <summary>
	/// Overwrite the file with the new content.
	/// </summary>
	public static readonly FileWriteModeType OVERWRITE = new FileWriteModeType("OVERWRITE", InnerEnum.OVERWRITE, "Overwrite");
	/// <summary>
	/// Update the file contents by updating overlapping data and appending the rest.
	/// </summary>
	public static readonly FileWriteModeType UPDATE = new FileWriteModeType("UPDATE", InnerEnum.UPDATE, "Update");

	private static readonly IList<FileWriteModeType> valueList = new List<FileWriteModeType>();

	static FileWriteModeType()
	{
		valueList.Add(APPEND);
		valueList.Add(OVERWRITE);
		valueList.Add(UPDATE);
	}

	public enum InnerEnum
	{
		APPEND,
		OVERWRITE,
		UPDATE
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
	private FileWriteModeType(string name, InnerEnum innerEnum, string displayName)
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
	public static FileWriteModeType valueOfIgnoreCase(string name)
	{
		if (string.ReferenceEquals(name, null))
		{
			return null;
		}
		FileWriteModeType[] values = values();
		foreach (FileWriteModeType t in values)
		{
			if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				return t;
			}
		}
		return null;
	}


		public static IList<FileWriteModeType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static FileWriteModeType valueOf(string name)
		{
			foreach (FileWriteModeType enumInstance in FileWriteModeType.valueList)
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