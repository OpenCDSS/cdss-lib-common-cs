using System;
using System.Collections.Generic;

// YearType - Year types, which indicate the span of months that define a year

namespace RTi.Util.Time
{

	/// <summary>
	/// Year types, which indicate the span of months that define a year.  For example "Water Year" is often
	/// used in the USA to indicate annual water volumes, based on seasons.  This enumeration should be used to
	/// indicate common full-year definitions.  Year types that include only a season or part of the year could
	/// specify this enumeration for full years and would otherwise need to define the year in some other way.
	/// By convention, non-calendar year types that do not contain "Year" in the name start in the previous
	/// calendar year and end in the current calendar year.  As new year types are added they should conform to the
	/// standard of starting with "Year" if the start matches the calendar year, and ending with "Year" if the end
	/// matches the calendar year.
	/// </summary>
	public sealed class YearType
	{
	/// <summary>
	/// Standard calendar year.
	/// </summary>
	public static readonly YearType CALENDAR = new YearType("CALENDAR", InnerEnum.CALENDAR, "Calendar", 0, 1, 0, 12);
	/// <summary>
	/// November (of previous year) to October (of current) year.
	/// </summary>
	public static readonly YearType NOV_TO_OCT = new YearType("NOV_TO_OCT", InnerEnum.NOV_TO_OCT, "NovToOct", -1, 11, 0, 10);
	/// <summary>
	/// Water year (October to September).
	/// </summary>
	public static readonly YearType WATER = new YearType("WATER", InnerEnum.WATER, "Water", -1, 10, 0, 9);
	/// <summary>
	/// May to April, with year agreeing with start.
	/// </summary>
	public static readonly YearType YEAR_MAY_TO_APR = new YearType("YEAR_MAY_TO_APR", InnerEnum.YEAR_MAY_TO_APR, "YearMayToApr", 0, 5, 1, 4);

	private static readonly IList<YearType> valueList = new List<YearType>();

	static YearType()
	{
		valueList.Add(CALENDAR);
		valueList.Add(NOV_TO_OCT);
		valueList.Add(WATER);
		valueList.Add(YEAR_MAY_TO_APR);
	}

	public enum InnerEnum
	{
		CALENDAR,
		NOV_TO_OCT,
		WATER,
		YEAR_MAY_TO_APR
	}

	public readonly InnerEnum innerEnumValue;
	private readonly string nameValue;
	private readonly int ordinalValue;
	private static int nextOrdinal = 0;

	/// <summary>
	/// The name that is used for choices and other technical code (terse).
	/// </summary>
	private readonly string __displayName;

	/// <summary>
	/// The calendar year offset in which the year starts.
	/// For example, -1 indicates that the year starts in the previous calendar year.
	/// </summary>
	private readonly int __startYearOffset;

	/// <summary>
	/// The calendar year offset in which the year ends.
	/// For example, 0 indicates that the year ends in the previous calendar year.
	/// </summary>
	private readonly int __endYearOffset;

	/// <summary>
	/// The calendar month (1-12) when the year starts.  For example, 10 indicates that the year starts in October.
	/// </summary>
	private readonly int __startMonth;

	/// <summary>
	/// The calendar month (1-12) when the year ends.  For example, 9 indicates that the year ends in September.
	/// </summary>
	private readonly int __endMonth;

	/// <summary>
	/// Construct an enumeration value. </summary>
	/// <param name="displayName"> name that should be displayed in choices, etc. </param>
	/// <param name="startYearOffset"> the offset to the calendar year for the start of the year.  For example, does the
	/// output year start in the same year as the calendar year (0), previous calendar year (-1), or next calendar year (1)? </param>
	/// <param name="startMonth"> the first calendar month (1-12) for the year type. </param>
	/// <param name="endYearOffset"> the offset to the calendar year for the end of the year.  For example, does the
	/// output year end in the same year as the calendar year (0), previous calendar year (-1), or next calendar year (1)? </param>
	/// <param name="endMonth"> the last calendar month (1-12) for the year type. </param>
	private YearType(string name, InnerEnum innerEnum, string displayName, int startYearOffset, int startMonth, int endYearOffset, int endMonth)
	{
		this.__displayName = displayName;
		this.__startYearOffset = startYearOffset;
		this.__startMonth = startMonth;
		this.__endYearOffset = endYearOffset;
		this.__endMonth = endMonth;

		nameValue = name;
		ordinalValue = nextOrdinal++;
		innerEnumValue = innerEnum;
	}

	/// <summary>
	/// Return the last month (1-12) in the year. </summary>
	/// <returns> the last month in the year. </returns>
	public int getEndMonth()
	{
		return __endMonth;
	}

	/// <summary>
	/// Return the end year offset. </summary>
	/// <returns> the end year offset. </returns>
	public int getEndYearOffset()
	{
		return __endYearOffset;
	}

	/// <summary>
	/// Return the year start (calendar notation for the current year and year type).
	/// For example, if Water year (Oct 1 of previous calendar year to Sep 30 of ending calendar year),
	/// return Oct 1 of the previous year.
	/// @return
	/// </summary>
	public DateTime getStartDateTimeForCurrentYear()
	{
		DateTime now = new DateTime(DateTime.DATE_CURRENT);
		now.setDay(1);
		now.setHour(0);
		now.setMinute(0);
		now.setSecond(0);
		if (getStartYearOffset() < 0)
		{
			if (now.getMonth() < getStartMonth())
			{
				// The start date/time needs to be adjusted (generally to previous year)
				now.addYear(getStartYearOffset());
			}
		}
		now.setMonth(getStartMonth());
		return now;
	}

	/// <summary>
	/// Return the first month (1-12) in the year. </summary>
	/// <returns> the first month in the year. </returns>
	public int getStartMonth()
	{
		return __startMonth;
	}

	/// <summary>
	/// Return the start year offset.
	/// For example, -1 indicates that the year starts in the previous calendar year. </summary>
	/// <returns> the start year offset. </returns>
	public int getStartYearOffset()
	{
		return __startYearOffset;
	}

	/// <summary>
	/// Get the list of year types. </summary>
	/// <returns> the list of year types. </returns>
	public static IList<YearType> getYearTypeChoices()
	{
		IList<YearType> choices = new List<YearType>();
		choices.Add(YearType.CALENDAR);
		choices.Add(YearType.NOV_TO_OCT);
		choices.Add(YearType.WATER);
		return choices;
	}

	/// <summary>
	/// Get the list of year types. </summary>
	/// <returns> the list of year types as strings. </returns>
	public static IList<string> getYearTypeChoicesAsStrings()
	{
		IList<YearType> choices = getYearTypeChoices();
		IList<string> stringChoices = new List<string>();
		for (int i = 0; i < choices.Count; i++)
		{
			YearType choice = choices[i];
			string choiceString = "" + choice;
			stringChoices.Add(choiceString);
		}
		return stringChoices;
	}

	/// <summary>
	/// Return the short display name for the statistic.  This is the same as the value. </summary>
	/// <returns> the display name. </returns>
	public override string ToString()
	{
		return __displayName;
	}

	/// <summary>
	/// Return the enumeration value given a string name (case-independent). </summary>
	/// <param name="name"> the year type string to match. </param>
	/// <returns> the enumeration value given a string name (case-independent), or null if not matched. </returns>
	/// <exception cref="IllegalArgumentException"> if the name does not match a valid year type. </exception>
	public static YearType valueOfIgnoreCase(string name)
	{
		if (string.ReferenceEquals(name, null))
		{
			return null;
		}
		YearType[] values = values();
		foreach (YearType t in values)
		{
			if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				return t;
			}
		}
		return null;
	}

	/// <summary>
	/// Indicate if the year for the year type matches the calendar year for the start.
	/// This will be the case if the start offset is zero.
	/// </summary>
	public bool yearMatchesStart()
	{
		if (getStartYearOffset() == 0)
		{
			return true;
		}
		else
		{
			return false;
		}
	}


		public static IList<YearType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static YearType valueOf(string name)
		{
			foreach (YearType enumInstance in YearType.valueList)
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