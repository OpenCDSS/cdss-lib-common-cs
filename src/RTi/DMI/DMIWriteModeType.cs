using System;
using System.Collections.Generic;

// DMIWriteModeType - enumeration for ways to write to a database

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

namespace RTi.DMI
{
	/// <summary>
	/// This enumeration stores values for a modes of writing to a DMI (e.g., database).  Different modes may be
	/// appropriate for performance reasons or because of business processes that expect certain behavior in
	/// business processes.
	/// </summary>
	public sealed class DMIWriteModeType
	{

	/// <summary>
	/// Delete the items first and then insert.
	/// </summary>
	public static readonly DMIWriteModeType DELETE_INSERT = new DMIWriteModeType("DELETE_INSERT", InnerEnum.DELETE_INSERT, DMI.DELETE_INSERT,"DeleteInsert");

	/// <summary>
	/// Insert only.
	/// </summary>
	public static readonly DMIWriteModeType INSERT = new DMIWriteModeType("INSERT", InnerEnum.INSERT, DMI.INSERT,"Insert");

	/// <summary>
	/// Try inserting first and if an exception occurs (data already exist), update the existing values.
	/// </summary>
	public static readonly DMIWriteModeType INSERT_UPDATE = new DMIWriteModeType("INSERT_UPDATE", InnerEnum.INSERT_UPDATE, DMI.INSERT_UPDATE,"InsertUpdate");

	/// <summary>
	/// Update only.
	/// </summary>
	public static readonly DMIWriteModeType UPDATE = new DMIWriteModeType("UPDATE", InnerEnum.UPDATE, DMI.UPDATE,"Update");

	/// <summary>
	/// Try updating the values first and if that fails (previous values do not exist) insert the values.
	/// </summary>
	public static readonly DMIWriteModeType UPDATE_INSERT = new DMIWriteModeType("UPDATE_INSERT", InnerEnum.UPDATE_INSERT, DMI.UPDATE_INSERT,"UpdateInsert");

	private static readonly IList<DMIWriteModeType> valueList = new List<DMIWriteModeType>();

	static DMIWriteModeType()
	{
		valueList.Add(DELETE_INSERT);
		valueList.Add(INSERT);
		valueList.Add(INSERT_UPDATE);
		valueList.Add(UPDATE);
		valueList.Add(UPDATE_INSERT);
	}

	public enum InnerEnum
	{
		DELETE_INSERT,
		INSERT,
		INSERT_UPDATE,
		UPDATE,
		UPDATE_INSERT
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
	/// The internal code for the enumeration, matches the matches the DMI write mode
	/// definitions to bridge legacy code..
	/// </summary>
	private readonly int code;

	/// <summary>
	/// Construct an enumeration value. </summary>
	/// <param name="code"> the internal numeric code for the enumeration </param>
	/// <param name="displayName"> name that should be displayed in choices, etc. </param>
	private DMIWriteModeType(string name, InnerEnum innerEnum, int code, string displayName)
	{
		this.code = code;
		this.displayName = displayName;

		nameValue = name;
		ordinalValue = nextOrdinal++;
		innerEnumValue = innerEnum;
	}

	/// <summary>
	/// Return the internal code used with the enumeration, which is the same as DMI definitions, to allow
	/// transition of code to the enumeration.
	/// </summary>
	public int getCode()
	{
		return this.code;
	}

	/// <summary>
	/// Return the display name for the enumeration string.  This is usually the same as the
	/// value but using appropriate mixed case. </summary>
	/// <returns> the display name. </returns>
	public override string ToString()
	{
		return displayName;
	}

	/// <summary>
	/// Return the enumeration value given a string name (case-independent). </summary>
	/// <returns> the enumeration value given a string name (case-independent), or null if not matched. </returns>
	public static DMIWriteModeType valueOfIgnoreCase(string name)
	{
		if (string.ReferenceEquals(name, null))
		{
			return null;
		}
		DMIWriteModeType[] values = values();
		// Currently supported values
		foreach (DMIWriteModeType t in values)
		{
			if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				return t;
			}
		}
		return null;
	}


		public static IList<DMIWriteModeType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static DMIWriteModeType valueOf(string name)
		{
			foreach (DMIWriteModeType enumInstance in DMIWriteModeType.valueList)
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