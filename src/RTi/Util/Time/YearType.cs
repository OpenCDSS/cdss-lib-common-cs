using System;
using System.Collections.Generic;

// YearType - Year types, which indicate the span of months that define a year

namespace RTi.Util.Time
{
    using Message = Message.Message;

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
        /// Return the enumeration value given a string name (case-independent). </summary>
        /// <param name="name"> the year type string to match. </param>
        /// <returns> the enumeration value given a string name (case-independent), or null if not matched. </returns>
        /// <exception cref="IllegalArgumentException"> if the name does not match a valid year type. </exception>
        public static YearType valueOfIgnoreCase(string name)
        {
            string routine = "YearType.valueOfIgnoreCase";
            if (string.ReferenceEquals(name, null))
            {
                return null;
            }
            IList<YearType> values = valueList;
            foreach (YearType t in values)
            {
                if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return t;
                }
            }
            return null;
        }
    }
}
