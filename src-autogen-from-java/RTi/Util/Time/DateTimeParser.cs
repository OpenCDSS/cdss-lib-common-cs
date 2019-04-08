using System;

// DateTimeParser - parser for a date/time string.

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

namespace RTi.Util.Time
{
	// TODO SAM 2012-04-11 This code needs to be reviewed in conjunction with DateTimeFormat.  Ideally, the
	// DateTimeFormat class should just contain instances of DateTimeFormatterType (an enumeration) and the
	// format string.  Then a DateTimeFormatter class could use DateTimeFormat to do formatting and this
	// DateTimeParse class could be used for parsing. For now, include the DateTimeFormatter type as a data member
	// in this class
	/// <summary>
	/// Parser for a date/time string.
	/// </summary>
	public class DateTimeParser
	{

	/// <summary>
	/// The formatter that is used with parsing.  This indicates the expected format of the date/times.
	/// </summary>
	internal DateTimeFormatterType __formatterType = null;

	/// <summary>
	/// The format string used with the formatter to translate date/time strings to DateTime objects.
	/// </summary>
	private string __formatString = null;

	/// <summary>
	/// Current year - this is used when a 2-digit year format is parsed.
	/// </summary>
	private int __currentYear2 = 0;

	/// <summary>
	/// Current century - this is used when a 2-digit year format is parsed.
	/// </summary>
	private int __currentCentury = 0;

	/// <summary>
	/// Construct a parser with a format string to use for parsing.
	/// The format type is determined from the first part of the string.
	/// </summary>
	public DateTimeParser(string formatString)
	{
		if (string.ReferenceEquals(formatString, null))
		{
			throw new System.ArgumentException("Formatter string is null.");
		}
		int pos = formatString.IndexOf(":", StringComparison.Ordinal);
		if (pos > 0)
		{
			// String might indicate the format type
			DateTimeFormatterType formatterType = DateTimeFormatterType.valueOfIgnoreCase(formatString.Substring(0,pos));
			if (formatterType == null)
			{
				// Use default
				init(DateTimeFormatterType.C, formatString);
			}
			else
			{
				init(formatterType, formatString.Substring(pos + 1));
			}
		}
		else
		{
			init(DateTimeFormatterType.C, formatString);
		}
	}

	/// <summary>
	/// Construct a parser with a formatter type and format string to use for parsing.
	/// </summary>
	public DateTimeParser(DateTimeFormatterType formatterType, string formatString)
	{
		init(formatterType, formatString);
	}

	/// <summary>
	/// Initialize the instance.
	/// </summary>
	private void init(DateTimeFormatterType formatterType, string formatString)
	{
		if (formatterType != DateTimeFormatterType.C)
		{
			throw new System.ArgumentException("Date/time formatter type " + formatterType + " is not supported.");
		}
		if (string.ReferenceEquals(formatString, null))
		{
			throw new System.ArgumentException("Formatter string is null.");
		}
		setDateTimeFormatterType(formatterType);
		setDateTimeFormatString(formatString);
		// Determine the current year.
		DateTime now = new DateTime(DateTime.DATE_CURRENT);
		setCurrentCentury((now.getYear() / 100) * 100); // For example, roundoff 2012 to 2000
		setCurrentYear2(now.getYear() - getCurrentCentury());
	}

	/// <summary>
	/// Parse a string into a date/time object.  If the provided DateTime instance is not null, then it will
	/// be filled with the result; if null, a new DateTime instance will be created.
	/// The precision of the result is set to the finest date/time part that is set. </summary>
	/// <param name="dt"> DateTime instance to reuse, or null to create and return a new DateTime instance </param>
	/// <param name="dtString"> date/time string to parse, using the formatter specified in the constructor </param>
	/// <exception cref="IllegalArgumentException"> if the input cannot be parsed </exception>
	public virtual DateTime parse(DateTime dt, string dtString)
	{
		if (dt == null)
		{
			// Create a new date/time
			dt = new DateTime();
		}
		else
		{
			// Reinitialize
			dt.setToZero();
		}
		// Process the format and extract corresponding information from the string
		DateTimeFormatterType formatterType = getFormatterType();
		if (formatterType == DateTimeFormatterType.C)
		{
			parseC(dt, dtString);
		}
		return dt;
	}

	/// <summary>
	/// Parse the date/time string using the C formatter type.
	/// </summary>
	private void parseC(DateTime dt, string dtString)
	{
		int smallestPrecision = DateTime.PRECISION_YEAR;
		string formatString = getFormatString();
		int lenFormat = formatString.Length;
		int icharFormat = 0; // Position in format string
		int icharString = 0; // Position in string being parsed
		char c; // Character in format string
		string s; // String carved out of string for parsing
		// Not sure what order information will be set so turn off checking
		dt.setPrecision(DateTime.DATE_FAST, true);
		while (icharFormat < lenFormat)
		{
			c = formatString[icharFormat];
			if (c == '%')
			{
				// Have a format character to process
				++icharFormat;
				if (icharFormat >= lenFormat)
				{
					// Past end of format string
					break;
				}
				c = formatString[icharFormat];
				++icharFormat; // For next loop
				if (c == 'a')
				{
					// Abbreviated weekday name.
					// Not handled
				}
				else if (c == 'A')
				{
					// Full weekday name.
					// Not handled
				}
				else if (c == 'b')
				{
					// Abbreviated month name - 3 characters
					s = dtString.Substring(icharString, 3);
					dt.setMonth(TimeUtil.monthFromAbbrev(s));
					icharString += 3;
					smallestPrecision = Math.Min(smallestPrecision, DateTime.PRECISION_MONTH);
				}
				else if (c == 'B')
				{
					// Long month name.
					// Not handled
				}
				else if (c == 'c')
				{
					// Not handled
				}
				else if (c == 'd')
				{
					// Day of month - 2 characters
					s = dtString.Substring(icharString, 2);
					dt.setDay(int.Parse(s));
					icharString += 2;
					smallestPrecision = Math.Min(smallestPrecision, DateTime.PRECISION_DAY);
				}
				else if (c == 'H')
				{
					// Hour of day - 2 characters
					s = dtString.Substring(icharString, 2);
					dt.setHour(int.Parse(s));
					icharString += 2;
					smallestPrecision = Math.Min(smallestPrecision, DateTime.PRECISION_HOUR);
				}
				else if (c == 'I')
				{
					// Hour of day 1-12
					// Not supported since need the AM/PM also
				}
				else if (c == 'j')
				{
					// Day of year (assumes that year is already set)...
					// Not supported
				}
				else if (c == 'm')
				{
					// Month of year...
					s = dtString.Substring(icharString, 2);
					dt.setMonth(int.Parse(s));
					icharString += 2;
					smallestPrecision = Math.Min(smallestPrecision, DateTime.PRECISION_MONTH);
				}
				else if (c == 'M')
				{
					// Minute of hour - 2 digits
					s = dtString.Substring(icharString, 2);
					dt.setMinute(int.Parse(s));
					icharString += 2;
					smallestPrecision = Math.Min(smallestPrecision, DateTime.PRECISION_MINUTE);
				}
				else if (c == 'p')
				{
					// AM or PM...
					// Not supported
				}
				else if (c == 'S')
				{
					// Seconds of minute - 2 digits
					s = dtString.Substring(icharString, 2);
					dt.setSecond(int.Parse(s));
					icharString += 2;
					smallestPrecision = Math.Min(smallestPrecision, DateTime.PRECISION_SECOND);
				}
				else if ((c == 'U') || (c == 'W'))
				{
					// Week of year...
					// Not supported
				}
				else if (c == 'x')
				{
					// Not supported
				}
				else if (c == 'X')
				{
				 // Not supported
				}
				else if (c == 'y')
				{
					// Two digit year...
					s = dtString.Substring(icharString, 2);
					int y2 = int.Parse(s);
					// Initialize 4-digit year to current century
					int y4 = getCurrentCentury() + y2;
					if (y2 > getCurrentYear2())
					{
						// Assume date was actually in the last century
						y4 -= 100;
					}
					dt.setYear(y4);
					icharString += 2;
					smallestPrecision = Math.Min(smallestPrecision, DateTime.PRECISION_YEAR);
				}
				else if (c == 'Y')
				{
					// 4-digit year
					s = dtString.Substring(icharString, 4);
					dt.setYear(int.Parse(s));
					icharString += 4;
					smallestPrecision = Math.Min(smallestPrecision, DateTime.PRECISION_YEAR);
				}
				else if (c == 'Z')
				{
					// Time zone not supported - don't know length
				}
			}
			else
			{
				// Other characters in format are treated as placeholders - skip the character in format and string
				++icharFormat;
				++icharString;
			}
		}
		// Reset to strict so that further use of the date/time will enforce valid date/times
		dt.setPrecision(smallestPrecision);
		dt.setPrecision(DateTime.DATE_STRICT, true);
	}

	/// <summary>
	/// Return the current century as 4-digit year.
	/// </summary>
	private int getCurrentCentury()
	{
		return __currentCentury;
	}

	/// <summary>
	/// Return the current 2-digit year.
	/// </summary>
	private int getCurrentYear2()
	{
		return __currentYear2;
	}

	/// <summary>
	/// Return the format string.
	/// </summary>
	public virtual string getFormatString()
	{
		return __formatString;
	}

	/// <summary>
	/// Return the formatter type.
	/// </summary>
	public virtual DateTimeFormatterType getFormatterType()
	{
		return __formatterType;
	}

	/// <summary>
	/// Set the current century as 4-digit year.
	/// </summary>
	private void setCurrentCentury(int currentCentury)
	{
		__currentCentury = currentCentury;
	}

	/// <summary>
	/// Set the current 2-digit year.
	/// </summary>
	private void setCurrentYear2(int currentYear2)
	{
		__currentYear2 = currentYear2;
	}

	/// <summary>
	/// Set the date/time format string.
	/// </summary>
	private void setDateTimeFormatString(string formatString)
	{
		__formatString = formatString;
	}

	/// <summary>
	/// Set the date/time formatter type.
	/// </summary>
	private void setDateTimeFormatterType(DateTimeFormatterType formatterType)
	{
		__formatterType = formatterType;
	}

	}

}