using System;
using System.Collections.Generic;
using System.Text;

// DateTimeFormat - this class aids in the parsing and formatting of DateTimes

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


	using IOUtil = RTi.Util.IO.IOUtil;

	using MathUtil = RTi.Util.Math.MathUtil;

	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// <para>
	/// This class aids in the parsing and formatting of DateTimes according to 
	/// String formats.  A major factor in the design of its interface methods was that
	/// it should be easy for developers to work with, as the following examples show.
	/// </para>
	/// <para>
	/// <b>Usage</b>
	/// </para>
	/// <para>
	/// To use this class, it should be instantiated with a DateTime format String.  
	/// This String will control how the class parses dates on input and formats dates for output.
	/// </para>
	/// <para>
	/// <b>DateTimeFormat Format Specifiers</b></para>
	/// <table>
	/// <tr>
	/// <td><b>Format Specifier</b></td>
	/// <td><b>Explanation</b></td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>yyyy</td>
	/// <td>Represents a 4-digit year.  No fewer than and no more than 4 digits will be accepted.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>yy</td>
	/// <td>Represents a 2-digit year.  No fewer than and no more than 4 digits will be accepted.<para>
	/// </para>
	/// 2-digit years are transformed into 4-digit years while parsing as follows:<para>
	/// <ul>
	/// <li>if the year is less than 30, 2000 will be added to it.   E.g., "17" would
	/// be set in the resulting DateTime as "2017".</li>
	/// <li>if the year is greater than or equal to 30, 1900 will be added to it. E.g.,
	/// "30" would be set in the resulting DateTime as "1930".</li>
	/// </para>
	/// </ul><para>
	/// This is consistent with the standard Microsoft Y2K 2-digit year handling.
	/// </tr>
	/// 
	/// <tr>
	/// <td>mm</td>
	/// <td>Represents a 2-digit month.  If the month is in the range 1-9, it must
	/// be padded with a single zero (e.g., "03").</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>m</td>
	/// <td>Represents a 1- or 2-digit month.  If the month is greater than 9, it will
	/// be represented by 2 digits.  If the month is in the range 1-9, it will be a single digit.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>dd</td>
	/// <td>Represents a 2-digit day.  If the day is in the range 1-9, it must
	/// be padded with a single zero (e.g., "03").</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>d</td>
	/// <td>Represents a 1- or 2-digit day.  If the day is greater than 9, it will
	/// be represented by 2 digits.  If the day is in the range 1-9, it will be a single digit.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>hh</td>
	/// <td>Represents a 2-digit hour.  If the hour is in the range 0-9, it must
	/// be padded with a single zero (e.g., "03").</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>h</td>
	/// <td>Represents a 1- or 2-digit hour.  If the hour is greater than 9, it will
	/// be represented by 2 digits.  If the hour is in the range 0-9, it will be a single digit.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>MM</td>
	/// <td>Represents a 2-digit minute.  If the minute is in the range 0-9, it must
	/// be padded with a single zero (e.g., "03").</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>M</td>
	/// <td>Represents a 1- or 2-digit minute.  If the minute is greater than 9, it will
	/// be represented by 2 digits.  If the minute is in the range 0-9, it will be a single digit.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>ss</td>
	/// <td>Represents a 2-digit second.  If the second is in the range 0-9, it must
	/// be padded with a single zero (e.g., "03").</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>s</td>
	/// <td>Represents a 1- or 2-digit second.  If the second is greater than 9, it will
	/// be represented by 2 digits.  If the second is in the range 0-9, it will be a single digit.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>ff</td>
	/// <td>Represents a 2-digit hundredth of second.  If the hundredth of second is 
	/// in the range 0-9, it must be padded with a single zero (e.g., "03").</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>f</td>
	/// <td>Represents a 1-digit hundredth of second.  If the hundredth of 
	/// second is greater than 9, only its tenth value (the left-most digit) will be 
	/// shown.  If the hundredth of second is in the range 0-9, it will be shown as 0.
	/// </td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>tz</td>
	/// <td>Represents a string time zone.  Currently on a single time zone type 
	/// </para>
	/// (all characters and numbers, no whitespace) is supported.<para>
	/// </para>
	/// <b>TODO (JTS - 2005-11-01)</b><para>Need to add more time zone support.
	/// </tr>
	/// </table>
	/// </para>
	/// <para>
	/// 
	/// </para>
	/// <b>Example of Creating a DateTimeFormat</b><para>
	/// <code><pre>
	/// // creating a DateTimeFormat for a given format is simple:
	/// DateTimeFormat format = new DateTimeFormat("yyyy-mm-dd hh:MM");
	/// </para>
	/// </pre></code><para>
	/// 
	/// </para>
	/// <b>Parsing Example</b><para>
	/// <code><pre>
	/// DateTimeFormat format = new DateTimeFormat("yyyy-mm-dd hh:MM:ss.ff");
	/// DateTime dt = null;
	/// try {
	///		dt = format.parse("2004-10-02 16:23:11.09");
	/// }
	/// catch (Exception e) {
	///		Message.printWarning(2, routine, ...);
	///		Message.printWarning(3, routine, e);
	/// }
	/// </para>
	/// </pre></code><para>
	/// 
	/// </para>
	/// <b>Formatting Example</b><para>
	/// <code><pre>
	/// DateTime dt = new DateTime(DateTime.DATE_CURRENT);
	/// DateTimeFormat format = new DateTimeFormat("yyyy-mm-dd hh:MM:ss.ff");
	/// String str = format.format(dt);
	/// // as of this writing, str == "2005-10-24 15:33:42.00";
	/// </para>
	/// </pre></code><para>
	/// 
	/// </para>
	/// <b>Iteration Example</b><para>
	/// The following piece of code creates a DateTime and DateTimeFormat and then 
	/// iterates through all the relative DateTimes in the same format, starting from the DateTime.
	/// <code><pre>
	/// DateTimeFormat format = new DateTime("mm/dd");
	/// DateTime dateTime = format.parse("01/03");
	/// 
	/// // a year must be set in the DateTime (see iterateRelativeDateTime()).
	/// dateTime.setYear(1999);
	/// 
	/// dateTime = format.iterateRelativeDateTime(dateTime);
	/// // dateTime = 2000-01-03
	/// dateTime = format.iterateRelativeDateTime(dateTime);
	/// // dateTime = 2001-01-03
	/// </pre></code>
	/// </para>
	/// </summary>
	public class DateTimeFormat
	{

	/// <summary>
	/// The number of date/time pieces that can be parsed and formatted by this class.<para>
	/// <b>Note:</b> "date/time piece" is a term used throughout this documentation, and
	/// it refers to a discrete part of a Date Time, such as a year, month, day, etc.
	/// This value is used internally in sizing internal arrays.
	/// </para>
	/// </summary>
	private const int __NUM_PIECES = 8;

	/// <summary>
	/// Date/time pieces that are recognized and handled by the DateTimeFormat class. 
	/// Used internally as the position of recognized date/time pieces within internal 
	/// arrays.  These are used instead of TimeInterval.* or DateTime.PRECISION_* 
	/// because they are neither intervals nor precisions, and because they are used
	/// as internal array element specifiers and the TimeInterval and DateTime values
	/// have different values.
	/// <para>
	/// For now, these are private.  In future, if a user needs access to them --
	/// for instance, if hasFormatSpecifier() should be opened to public access.  
	/// </para>
	/// </summary>
	private const int __YEAR = 0, __MONTH = 1, __DAY = 2, __HOUR = 3, __MINUTE = 4, __SECOND = 5, __HSECOND = 6, __TZ = 7;

	/// <summary>
	/// String arrays storing the various format specifiers recognized for date/time 
	/// pieces.  This is a two-dimensional array because a single date/time piece can 
	/// be represented by several different format specifiers.  For example: Years can be "yyyy" or "yy".<para>
	/// The first dimension is __NUM_PIECES long, and the second-dimension array at
	/// each element of the first-dimension corresponds to one of __YEAR, __MONTH,
	/// __DAY, etc.  For more information, see the static initialization block below.
	/// </para>
	/// </summary>
	private static string[][] __formatSpecifiers = null;

	/// <summary>
	/// String arrays storing the various regular expressions to use for recognizing 
	/// date/time pieces.  This is a two-dimensional array because a single date/time 
	/// piece can be represented by several different format specifiers.  
	/// For example: Years can be "(\d\d\d\d)" (for "yyyy") or "(\d\d)" (for "yy").<para>
	/// The first dimension is __NUM_PIECES long, and the second-dimension array at
	/// each element of the first-dimension corresponds to one of __YEAR, __MONTH,
	/// __DAY, etc.  For more information, see the static initialization block below.
	/// </para>
	/// </summary>
	private static string[][] __formatSpecifierRegEx = null;

	/// <summary>
	/// Boolean array that specifies for each date/time piece whether it is contained
	/// within the DateTimeFormat string this DateTimeFormat was instantiated with.<para>
	/// For instance, if the object was instantiated with the following format: 
	/// </para>
	/// "yyyy dd hh __TZ", the values in __datePieces would be as follows:<para>
	/// <code><pre>
	/// __datePieces[__YEAR] = true;
	/// __datePieces[__MONTH] = false;
	/// __datePieces[__DAY] = true;
	/// __datePieces[__HOUR] = true;
	/// __datePieces[__MINUTE] = false;
	/// __datePieces[__SECOND] = false;
	/// __datePieces[__HSECOND] = false;
	/// __datePieces[__TZ] = true;
	/// </pre></code>
	/// </para>
	/// </summary>
	private bool[] __datePieces = null;

	/// <summary>
	/// Whether the format in this DateTimeFormat is absolute or not.  An absolute
	/// format will specify a DateTime at a single point in time.  For example, 
	/// "2005", "2005-12", and "2005-12-25" are all absolute points in time.  "05",
	/// "2005 16:32", and "12/25 16:32" are relative points in time.  One way of 
	/// easily determining whether a format is absolute or not is to see whether the 
	/// date/time pieces it contain start at year and continue in an unbroken "chain" 
	/// to the lowest level of precision in the format (e.g., "Year-Month-
	/// </summary>
	private bool __isAbsolute = false;

	/// <summary>
	/// A copy of the format String, in character-array format.  This is used internally
	/// to track which format specifiers in the format String have been processed and which have not.<para>
	/// For instance, if a DateTimeFormat string is "yyyy-mm", then
	/// when this array is first instantiated it will contain "yyyy-mm".  Once 
	/// the "yyyy" format specifier has been handled internally, this array will 
	/// then contain "....-mm", where "." is actually the character specified by __used.
	/// This is so that it is known which format specifiers have been processed and which have not.
	/// </para>
	/// </summary>
	private char[] __formatCharacters = null;

	/// <summary>
	/// A character value used in the <code>findFormatSpecifierOccurrences()</code>
	/// method to represent a character that has already been processed into a regular 
	/// expression.  This character value corresponds to the ASCII EOT value, and thus 
	/// should never appear within a DateTimeFormat string.<para>
	/// For example, if the format String is "yyyy-mm-dd", once the "yyyy" format
	/// specifier has been processed internally, the __used value will be placed in
	/// the __formatCharacters array in its characters so that it is known not to 
	/// reprocess it.  The __formatCharacters array will then be "4444-mm-dd".
	/// </para>
	/// </summary>
	private readonly char __used = (char)4;

	/// <summary>
	/// Integer arrays that mark the location of each format specifier in the format 
	/// String.  The first two dimensions of this array correspond to the two 
	/// dimensions of the __formatSpecifiers and __formatSpecifierRegEx arrays (ie, 
	/// the first dimension corresponds to the date/time piece (e.g., __YEAR), and 
	/// the second dimension corresponds to particular format specifiers [e.g., "yyyy", "yy"]).<para>
	/// The third dimension corresponds to the location of the given date/time piece's 
	/// </para>
	/// format specifiers' locations within the string.  <para>
	/// </para>
	/// Thus, the following array values:<para>
	///   __formatSpecifierLocations[__HOUR][1][0] = 3;<br>
	/// </para>
	///   __formatSpecifierLocations[__HOUR][1][1] = 7;<para>
	/// specify that the second __HOUR format specifier ("h") is found in the 
	/// </para>
	/// DateTimeFormat string at character positions 3 and 7.<para>
	/// <b>NOTE:</b> This is a very complicated set of arrays, but it is only used
	/// to accumulate information about the format specifiers during initial set up.  
	/// Its information is soon moved into the __sortedFormatSpecifier* arrays to be 
	/// easier to work with, and its information is discarded prior to any parsing or formatting.
	/// </para>
	/// </summary>
	private int[][][] __formatSpecifierLocations = null;

	/// <summary>
	/// An array containing the length of every format specifier in the DateTimeFormat 
	/// string sorted in the same order as 
	/// <code>__sortedFormatSpecifierLocations</code>.  
	/// Thus traversal through the array will result in the length of each format 
	/// specifier in the DateTimeFormat string from left to right.<para>
	/// The location in the DateTimeFormat string of the format specifier with the length of 
	/// <code>__sortedFormatSpecifierLengths[X]</code> is available from 
	/// <code>__sortedFormatSpecifierLocations[X];</code>  See also 
	/// <code>int[] __sortedFormatSpecifierTypes</code> and 
	/// <code>String[] __sortedFormatSpecifierRegExs</code>.
	/// </para>
	/// <para>For more information on why these arrays are sorted, see createSortedArrays().
	/// </para>
	/// </summary>
	private int[] __sortedFormatSpecifierLengths = null;

	/// <summary>
	/// An array of the location of all format specifiers found in the DateTimeFormat 
	/// string, sorted into ascending order.  Thus, traversal through the array will 
	/// reveal the indices of each format specifier in the String in left-to-right order.  See also
	/// <code>int[] __sortedFormatSpecifierLengths</code>,
	/// <code>int[] __sortedFormatSpecifierTypes</code> and
	/// <code>String[] __sortedFormatSpecifierRegExs</code>.
	/// <para>For more information on why these arrays are sorted, see createSortedArrays().
	/// </para>
	/// </summary>
	private int[] __sortedFormatSpecifierLocations = null;

	/// <summary>
	/// An array containing the types (e.g., __YEAR) of all the format specifiers 
	/// found in the DateTimeFormat string, sorted in the same order as 
	/// <code>__sortedFormatSpecifierLocations</code>.  Thus, traversal through the 
	/// array will result in the type of each format specifier in the DateTimeFormat 
	/// string from left to right.<para>
	/// The location in the DateTimeFormat string of the format specifier type at 
	/// element X is available from <code>__sortedFormatSpecifierLocations[X]</code>;  
	/// See also <code>int[] __sortedFormatSpecifierLengths</code> and
	/// <code>String[] __sortedFormatSpecifierRegExs</code>.
	/// </para>
	/// <para>For more information on why these arrays are sorted, see createSortedArrays().
	/// </para>
	/// </summary>
	private int[] __sortedFormatSpecifierTypes = null;

	/// <summary>
	/// The default flag with which to construct new DateTimes when parsing.  This
	/// normally should not need to be changed, but if a slight performance increase 
	/// (around 10-15%) is desired, it can be set to DateTime.DATE_FAST;.  Setting
	/// to fast by default is not enabled because setting to fast disables some
	/// date checks (e.g., finding the day of the year and leap-year status), which
	/// may not be desirable in all situations.
	/// </summary>
	private int __parseDateConstructorFlag = DateTime.DATE_STRICT;

	/// <summary>
	/// The regular expression pattern that has been compiled from the DateTimeFormat 
	/// string.  This pattern can be used to match date/time strings that are in the same format.
	/// </summary>
	private Pattern __pattern = null;

	/// <summary>
	/// An array containing the regular expressions for parsing the format specifiers 
	/// in the DateTimeFormat string, sorted in the same order as 
	/// <code>__sortedFormatSpecifierLocations</code>.  Thus, traversal through the 
	/// array will result in the regular expression of each format specifier in the 
	/// DateTimeFormat string from left to right.<para>
	/// The location in the format String of the format specifier with the regular 
	/// expression at <code>__sortedFormatSpecifierRegExs[X]</code> is available from
	/// <code>__sortedFormatSpecifierLocations[X]</code>.  See also 
	/// <code>int[] __sortedFormatSpecifierLengths</code> and
	/// <code>int[] __sortedFormatSpecifierTypes</code>.
	/// </para>
	/// </summary>
	private string[] __sortedFormatSpecifierRegExs = null;

	/// <summary>
	/// An array containing the literal values that surround all the format specifiers 
	/// in the format String.  The array is set up such that the element at position X 
	/// contains the literal values between format specifier X and format specifier X-1.
	/// Element 0 contains the literal values that precede the first format 
	/// specifier, and the last element contains the literal values that 
	/// follow the final format specifier.<para>
	/// </para>
	/// For instance, in the following DateTimeFormat string:<para>
	/// </para>
	/// [yyyy/mm]<para>
	/// </para>
	/// FormatSpecifier 0 is "yyyy" and format specifier 1 is "mm".<para>
	/// </para>
	/// The elements of __formatSpecifierLiteralSurroundingValues in this case are:<para>
	/// [0] - "["<br>
	/// [1] - "/"<br>
	/// [2] = "]"
	/// </para>
	/// </summary>
	private string[] __formatSpecifierLiteralSurroundingValues = null;

	/// <summary>
	/// The DateTimeFormat string for which the parser and formatter were built.
	/// </summary>
	private string __format = null;

	/// <summary>
	/// The string that corresponds to the regular expression constructed to parse 
	/// the given DateTime format.
	/// </summary>
	private string __regularExpressionString = null;

	/// <summary>
	/// Initialization of static data.
	/// </summary>
	static DateTimeFormat()
	{
		__formatSpecifiers = new string[__NUM_PIECES][];
		__formatSpecifiers[__YEAR] = new string[2];
		__formatSpecifiers[__YEAR][0] = "yyyy";
		__formatSpecifiers[__YEAR][1] = "yy";
		__formatSpecifiers[__MONTH] = new string[2];
		__formatSpecifiers[__MONTH][0] = "mm";
		__formatSpecifiers[__MONTH][1] = "m";
		__formatSpecifiers[__DAY] = new string[2];
		__formatSpecifiers[__DAY][0] = "dd";
		__formatSpecifiers[__DAY][1] = "d";
		__formatSpecifiers[__HOUR] = new string[2];
		__formatSpecifiers[__HOUR][0] = "hh";
		__formatSpecifiers[__HOUR][1] = "h";
		__formatSpecifiers[__MINUTE] = new string[2];
		__formatSpecifiers[__MINUTE][0] = "MM";
		__formatSpecifiers[__MINUTE][1] = "M";
		__formatSpecifiers[__SECOND] = new string[2];
		__formatSpecifiers[__SECOND][0] = "ss";
		__formatSpecifiers[__SECOND][1] = "s";
		__formatSpecifiers[__HSECOND] = new string[2];
		__formatSpecifiers[__HSECOND][0] = "ff";
		__formatSpecifiers[__HSECOND][1] = "f";
		__formatSpecifiers[__TZ] = new string[1];
		__formatSpecifiers[__TZ][0] = "tz";

		__formatSpecifierRegEx = new string[__NUM_PIECES][];
		__formatSpecifierRegEx[__YEAR] = new string[2];
		__formatSpecifierRegEx[__YEAR][0] = "(\\d\\d\\d\\d)";
		__formatSpecifierRegEx[__YEAR][1] = "(\\d\\d)";
		__formatSpecifierRegEx[__MONTH] = new string[2];
		__formatSpecifierRegEx[__MONTH][0] = "(\\d\\d)";
		__formatSpecifierRegEx[__MONTH][1] = "(\\d\\d?)";
		__formatSpecifierRegEx[__DAY] = new string[2];
		__formatSpecifierRegEx[__DAY][0] = "(\\d\\d)";
		__formatSpecifierRegEx[__DAY][1] = "(\\d\\d?)";
		__formatSpecifierRegEx[__HOUR] = new string[2];
		__formatSpecifierRegEx[__HOUR][0] = "(\\d\\d)";
		__formatSpecifierRegEx[__HOUR][1] = "(\\d\\d?)";
		__formatSpecifierRegEx[__MINUTE] = new string[2];
		__formatSpecifierRegEx[__MINUTE][0] = "(\\d\\d)";
		__formatSpecifierRegEx[__MINUTE][1] = "(\\d\\d?)";
		__formatSpecifierRegEx[__SECOND] = new string[2];
		__formatSpecifierRegEx[__SECOND][0] = "(\\d\\d)";
		__formatSpecifierRegEx[__SECOND][1] = "(\\d\\d?)";
		__formatSpecifierRegEx[__HSECOND] = new string[2];
		__formatSpecifierRegEx[__HSECOND][0] = "(\\d\\d)";
		__formatSpecifierRegEx[__HSECOND][1] = "(\\d)";
		__formatSpecifierRegEx[__TZ] = new string[1];
		__formatSpecifierRegEx[__TZ][0] = "(\\w+)";
	}

	/// <summary>
	/// Constructor.  Creates an object that can parse date strings in the given 
	/// format and can format DateTimes to the given format.<para>
	/// No format is considered a bad format.  If no format specifiers are contained
	/// within the format, it will simply output only literal values when a date is
	/// formatted for output, and will recognize only strings that match the exact value
	/// of the format string for input.  
	/// </para>
	/// </summary>
	/// <param name="format"> the String to use for formatting input and output dates.  Can be null. </param>
	public DateTimeFormat(string format)
	{
		if (string.ReferenceEquals(format, null))
		{
			format = "";
		}
		__format = format;
		__formatCharacters = format.ToCharArray();
		createParser();
		createFormatter();
		cleanup();
		createDatePieces();
		__isAbsolute = isAbsolute(format, this);
	}

	/// <summary>
	/// Copy constructor. </summary>
	/// <param name="format"> the DateTimeFormat to copy. </param>
	public DateTimeFormat(DateTimeFormat format)
	{
		// The following is rather complicated.  Copies are made of all the 
		// internal data from the original format.  If this proves to be a 
		// maintenance hassle, it might be better just to do this, instead:
		/*
			public DateTimeFormat(DateTimeFormat format) {
				this(format.__format);
			}
		*/
		// That'll have more performance overhead, but it might be a better
		// tradeoff given the complexity of the following:

		if (format.__datePieces != null)
		{
			__datePieces = new bool[format.__datePieces.Length];
			for (int i = 0; i < format.__datePieces.Length; i++)
			{
				__datePieces[i] = format.__datePieces[i];
			}
		}

		__isAbsolute = format.__isAbsolute;

		if (format.__formatCharacters != null)
		{
			__formatCharacters = new char[format.__formatCharacters.Length];
			for (int i = 0; i < format.__formatCharacters.Length; i++)
			{
				__formatCharacters[i] = format.__formatCharacters[i];
			}
		}

		if (format.__sortedFormatSpecifierLengths != null)
		{
			__sortedFormatSpecifierLengths = new int[format.__sortedFormatSpecifierLengths.Length];
			for (int i = 0; i < format.__sortedFormatSpecifierLengths.Length;i++)
			{
				__sortedFormatSpecifierLengths[i] = format.__sortedFormatSpecifierLengths[i];
			}
		}

		if (format.__sortedFormatSpecifierLocations != null)
		{
			__sortedFormatSpecifierLocations = new int[format.__sortedFormatSpecifierLocations.Length];
			for (int i = 0; i < format.__sortedFormatSpecifierLocations.Length; i++)
			{
				__sortedFormatSpecifierLocations[i] = format.__sortedFormatSpecifierLocations[i];
			}
		}

		if (format.__sortedFormatSpecifierTypes != null)
		{
			__sortedFormatSpecifierTypes = new int[format.__sortedFormatSpecifierTypes.Length];
			for (int i = 0; i < format.__sortedFormatSpecifierTypes.Length; i++)
			{
				__sortedFormatSpecifierTypes[i] = format.__sortedFormatSpecifierTypes[i];
			}
		}

		__parseDateConstructorFlag = format.__parseDateConstructorFlag;

		if (format.__sortedFormatSpecifierRegExs != null)
		{
			__sortedFormatSpecifierRegExs = new string[format.__sortedFormatSpecifierRegExs.Length];
			for (int i = 0; i < format.__sortedFormatSpecifierRegExs.Length; i++)
			{
				__sortedFormatSpecifierRegExs[i] = format.__sortedFormatSpecifierRegExs[i];
			}
		}

		if (format.__formatSpecifierLiteralSurroundingValues != null)
		{
			__formatSpecifierLiteralSurroundingValues = new string[format.__formatSpecifierLiteralSurroundingValues.Length];
			for (int i = 0; i < format.__formatSpecifierLiteralSurroundingValues.Length; i++)
			{
				__formatSpecifierLiteralSurroundingValues[i] = format.__formatSpecifierLiteralSurroundingValues[i];
			}
		}

		__format = format.__format;

		__regularExpressionString = format.__regularExpressionString;
		__pattern = Pattern.compile(__regularExpressionString);
	}

	/// <summary>
	/// An internal cleanup method to free memory used when constructing the parsing and formatting data.
	/// </summary>
	private void cleanup()
	{
		for (int i = 0; i < __formatSpecifierLocations.Length; i++)
		{
			for (int j = 0; j < __formatSpecifierLocations[i].Length; j++)
			{
				__formatSpecifierLocations[i][j] = null;
			}
			__formatSpecifierLocations[i] = null;
		}
		__formatCharacters = null;
	}

	/// <summary>
	/// Creates the internal __datePieces array.  This array contains booleans that are 
	/// accessed internally to tell whether the format has a given date/time piece within it
	/// (e.g., if <code>__datePieces[__YEAR] == true</code> then the format contains
	/// a year formatting code; if false, it does not).
	/// </summary>
	private void createDatePieces()
	{
		__datePieces = new bool[__NUM_PIECES];

		// First, initialize the array to false for all elements
		for (int i = 0; i < __NUM_PIECES; i++)
		{
			__datePieces[i] = false;
		}

		// Run through the __sortedFormatSpecifierTypes
		for (int i = 0; i < __sortedFormatSpecifierTypes.Length; i++)
		{
			__datePieces[__sortedFormatSpecifierTypes[i]] = true;
		}
	}

	/// <summary>
	/// Creates the internal data structures necessary to format a DateTime object as
	/// a String with a DateTimeFormat string.
	/// </summary>
	private void createFormatter()
	{
		// __formatSpecifierLiteralSurroundingValues needs to be one element 
		// larger than the sorted arrays.  It contains the literal characters 
		// that precede a format specifier (but that are after the previous 
		// format specifier).  The final element's values do not precede a 
		// format specifier, but are the final literal characters in the
		// string, hence the +1 size.
		__formatSpecifierLiteralSurroundingValues = new string[__sortedFormatSpecifierLocations.Length + 1];

		if (__sortedFormatSpecifierLocations.Length == 0)
		{
			__formatSpecifierLiteralSurroundingValues[0] = "";
			return;
		}

		// the first format specifier's preceding characters are an exceptional 
		// case, since there is no prior format specifier.
		__formatSpecifierLiteralSurroundingValues[0] = __format.Substring(0, __sortedFormatSpecifierLocations[0]);

		for (int i = 0; i < __sortedFormatSpecifierLocations.Length; i++)
		{
			// the last format specifier's following characters are an 
			// exceptional case, since there is no following format specifier.
			if (i == (__sortedFormatSpecifierLocations.Length - 1))
			{
				// This is also the loop branch that will be entered
				// if there is only a single format specifier in the DateTimeFormat string.
				__formatSpecifierLiteralSurroundingValues[i + 1] = __format.Substring(__sortedFormatSpecifierLocations[i] + __sortedFormatSpecifierLengths[i]);
			}
			else
			{
				__formatSpecifierLiteralSurroundingValues[i + 1] = __format.Substring(__sortedFormatSpecifierLocations[i] + __sortedFormatSpecifierLengths[i], __sortedFormatSpecifierLocations[i + 1] - (__sortedFormatSpecifierLocations[i] + __sortedFormatSpecifierLengths[i]));
			}
		}
	}

	/// <summary>
	/// Creates arrays with the location, type, and length of the DateTimeFormat string 
	/// format specifiers, and of the regular expressions needed to parse those format 
	/// specifiers, in the order that the format specifiers appear in the 
	/// DateTimeFormat string, from left to right.<para>
	/// </para>
	/// For example, for the format String "yyyy-mm", the following arrays would be created:<para>
	/// __sortedFormatSpecifierLocations[0] = 0;
	/// __sortedFormatSpecifierTypes[0] = __YEAR;	
	/// __sortedFormatSpecifierLengths[0] = 4;		
	/// __sortedFormatSpecifierRegExs[0] = "\d\d\d\d"	
	/// </para>
	/// <para>
	/// __sortedFormatSpecifierLocations[1] = 5;
	/// __sortedFormatSpecifierTypes[1] = __MONTH;
	/// __sortedFormatSpecifierLengths[1] = 2;
	/// __sortedFormatSpecifierRegExs[1] = "\d\d";
	/// </para>
	/// <para>
	/// The reason these arrays need to be ordered is because both the regular 
	/// expression necessary for matching a date when parsing and the format information
	/// necessary when formatting a String for output are built from left-to-right. 
	/// Since they work by moving right through the String and either putting together
	/// regular expression pieces or putting together date pieces for output, it makes
	/// sense to compile the information in left-to-right order in the first place. 
	/// If this were not done, the __formatSpecifierLocations array would need to be
	/// iterated through every format specifier (e.g., __YEAR) and format specifier 
	/// type (e.g., "yyyy" and "yy") for every character in an input date or output
	/// date in order to determine whether a format specifier could be found at a 
	/// given character position.  This is much more efficient.
	/// </para>
	/// </summary>
	private void createSortedArrays()
	{
		// These lists are temporary, and are used to move information from
		// the __formatSpecifierLocations arrays into the separate ordered arrays.
		IList<int> lengths = new List<int>();
		IList<int> locs = new List<int>();
		IList<string> regExs = new List<string>();
		IList<int> types = new List<int>();

		// Move the data from the __formatSpecifierLocations arrays into the above lists.  

		int size = 0;
		for (int i = 0; i < __NUM_PIECES; i++)
		{
			size = __formatSpecifiers[i].Length;
			for (int j = 0; j < size; j++)
			{
				for (int k = 0; k < __formatSpecifierLocations[i][j].Length; k++)
				{
					locs.Add(new int?(__formatSpecifierLocations[i][j][k]));
					types.Add(new int?(i));
					lengths.Add(new int?(__formatSpecifiers[i][j].Length));
					regExs.Add(__formatSpecifierRegEx[i][j]);
				}
			}
		}

		// First, move the values from "locs" into an array, so that the
		// array can be sorted.  These values are the starting positions of
		// each format specifier within the DateTimeFormat string, and once 
		// formatted will be in left-to-right order moving through the string.

		size = locs.Count;
		__sortedFormatSpecifierLocations = new int[size];
		int[] order = new int[size];
		for (int i = 0; i < size; i++)
		{
			__sortedFormatSpecifierLocations[i] = ((int?)locs[i]).Value;
		}

		// Sort the array, but maintain a link to the prior sort order.

		MathUtil.sort(__sortedFormatSpecifierLocations, MathUtil.SORT_QUICK, MathUtil.SORT_ASCENDING, order, true);

		__sortedFormatSpecifierTypes = new int[size];
		__sortedFormatSpecifierLengths = new int[size];
		__sortedFormatSpecifierRegExs = new string[size];

		// Transfer data from the other Vectors into their respective arrays,
		// in the same order as the original values were moved from "locs" to
		// "__sortedFormatSpecifierLocations".  

		// That is, an element at position X in locs is now in position
		// Y in __sortedFormatSpecifierLocations.  Do the same thing, for 
		// Vectors "types", "lengths", and "regExs" with their respective arrays

		for (int i = 0; i < size; i++)
		{
			__sortedFormatSpecifierTypes[i] = ((int?)types[order[i]]).Value;
			__sortedFormatSpecifierLengths[i] = ((int?)lengths[order[i]]).Value;
			__sortedFormatSpecifierRegExs[i] = (string)regExs[order[i]];
		}
	}

	/// <summary>
	/// Executes a sequence of methods to create the information necessary to parse
	/// a date/time based on the specific DateTimeFormat string.
	/// </summary>
	private void createParser()
	{
		__formatSpecifierLocations = new int[__NUM_PIECES][][];
		for (int i = 0; i < __NUM_PIECES; i++)
		{
			__formatSpecifierLocations[i] = new int[__formatSpecifiers[i].Length][];
		}

		findFormatSpecifiers();
		createRegularExpression();
	}

	/// <summary>
	/// Based on the information processed from a DateTimeFormat string, creates the 
	/// regular expression necessary to parse a date string in the given format.
	/// </summary>
	private void createRegularExpression()
	{
		createSortedArrays();

		StringBuilder sb = new StringBuilder();

		char ch = (char)0;
		int length = __format.Length;
		int marked = 0;

		// Iterate through the length of the string ...

		for (int i = 0; i < length; i++)
		{
			// First, see if the given position (i) is one at which a
			// format specifier starts.  This method returns the index 
			// within the sorted arrays of the format specifier that starts at the given position.

			marked = isMarkedPosition(i);

			// If -1 was returned, no format specifier starts at the 
			// current position and the literal character can be moved into the regular expression.

			if (marked == -1)
			{
				// Java will handle any Unicode issues as long as the regular 'char' data type is used.
				ch = __format[i];

				// If the character is a backslash, then this is an 
				// escaped literal character (such as \m).  Characters 
				// that are used in format specifier strings must be escaped.

				if (ch == '\\')
				{
					// move to the next character position
					i++;
					if (i >= length)
					{
						continue;
					}
					ch = __format[i];
				}

				// Any character that is not a normal number or
				// letter should be escaped, because many of them have
				// secondary meaning in regular expressions.
				if (needsEscaped(ch))
				{
					sb.Append("\\");
				}
				sb.Append("" + ch);
			}
			else
			{
				// Copy the regular expression that corresponds to 
				// format specifier at the given position to the buffer
				sb.Append(__sortedFormatSpecifierRegExs[marked]);

				// Move ahead enough characters to move off the current
				// format specifier
				i += __sortedFormatSpecifierLengths[marked] - 1;
			}
		}

		// Build the regular expression ...
		__pattern = Pattern.compile(sb.ToString());
		__regularExpressionString = sb.ToString();
	}

	/// <summary>
	/// When a <code>parse()</code> call fails, calling this method on the same
	/// inputString string will result in an explanation of where the parsing failed.  
	/// The explanation will be returned as a String array, each element of which 
	/// contains different information about how the parse failed.  This is so that 
	/// applications can choose which information they wish to display.<para>
	/// <b>NOTE:</b> This is a <b>very</b> expensive method to call, and should not 
	/// be called in a loop for many parses().  At its worst case, performance-wise
	/// it is equivalent to calling parse() a number of times equal to the number of
	/// characters in all the format specifiers in the DateTimeFormat string.  At its 
	/// </para>
	/// best, it is equivalent to calling parse() twice.  <para>
	/// As such, care should be used when calling this method.  It should be used rarely.
	/// </para>
	/// </summary>
	/// <param name="inputString"> the DateTime string to try to parse.
	/// </para>
	/// </param>
	/// <returns> a String array containing the following information:<para>
	/// [0] = Parsing of "INPUT" failed. <br>
	/// [1] = INPUT<br>
	/// [2] = a carat pointing to the spot in INPUT where parsing failed, like what
	/// the java compiler shows upon failure.<br>
	/// [3] = The date/time format that was used to try parsing was: "FORMAT".<br>
	/// [4] = The original regular expression used to try parsing was: "REGEX".<br>
	/// [5] = The regular expression that caused parsing failure was: "BADREGEX".<para>
	/// </para>
	/// An example of one of these return arrays is:<para>
	/// [0] = "Parsing of "2005-06" failed."<br>
	/// [1] = "yy-mm"<br>
	/// [2] = "  ^"<br>
	/// [3] = "The date/time format that was used to try parsing was: "yy-mm"."<br>
	/// [4] = "The original regular expression used to try parsing was: "(\d\d)-(\d\d)".
	/// <br>
	/// [5] = "The regular expression that caused parsing failure was: "(\d\d\-).*".<p>
	/// While not all this info is necessary for users, in the event of a failure 
	/// </para>
	/// it can be printed to the log file for developers to debug.<para>
	/// <b>NOTE:</b> If inputString can be parsed properly by the parsing, the return value will be null. </returns>
	public virtual string[] explainParseFailureOld(string inputString)
	{
		bool done = false;
		bool escape = false;
		bool runParse = false;
		char[] chars = __regularExpressionString.ToString().ToCharArray();
		int length = __regularExpressionString.Length;
		int pos = 0;
		Matcher matcher = null;
		Pattern pattern = null;
		string etc = "";
		string lastGroup = "";
		string s = "";

		// This essentially tries to validate the string one character at a 
		// time until it finds where the validation fails.  It can then tell
		// which character in the input string caused the failure.  

		// Given a format String "mm-dd", the following is its regular expression for parsing:
		//	(\d\d)-(\d\d)

		// When this method is called, it will try to generate and run all the
		// following regular expressions, until one fails:
		//	(\d).*
		//	(\d\d).*
		//	(\d\d)-.*
		//	(\d\d)-(\d).*
		//	(\d\d)-(\d\d).*

		// As should be obvious, for long DateTimeFormat strings, this could 
		// turn out to be a lot of regular expressions.  There is almost 
		// certainly a better way to do this, but limited investigation 
		// revealed no leads on alternate methods, at least given Java 1.4.2's 
		// RegEx library.

		while (!done)
		{
			runParse = false;
			escape = false;

			// Java will handle any Unicode issues as long as
			// the regular 'char' data type is used.
			if (chars[pos] == '\\')
			{
				// an escaped character, add the next char
				s += "\\";
				escape = true;
				pos++;
			}

			if (pos >= length)
			{
				// at the very end of the String, so try to run the String as is.
				done = true;
				runParse = false;
			}
			else
			{
				if (needsEscaped(chars[pos]))
				{
					if (escape)
					{
						s += chars[pos];
						runParse = true;
					}
					else
					{
						runParse = false;
					}
				}
				else
				{
					s += chars[pos];
					runParse = true;
				}

				if (pos < (length - 1))
				{
					// check for repeat characters ("*","+","?")
					if (chars[pos + 1] == '*' || chars[pos + 1] == '+' || chars[pos + 1] == '?')
					{
						s += chars[pos + 1];
						pos++;
						runParse = true;
					}
				}
			}

			if (pos >= (length - 1))
			{
				// at the end of the String, so try to run the parse
				done = true;
			}

			if (runParse)
			{
				etc = "(" + s + ").*";
				pattern = Pattern.compile(etc);
				matcher = pattern.matcher(inputString);

				if (!matcher.matches())
				{
					string errorLocation = "";
					for (int i = 0; i < lastGroup.Length; i++)
					{
						errorLocation += " ";
					}
					errorLocation += "^";

					string[] errors = new string[5];
					errors[0] = "Parsing of \"" + inputString + "\" failed.";
					errors[1] = inputString;
					errors[2] = errorLocation;
					errors[3] = "The original regular expression used to try parsing was: \""
						+ __regularExpressionString + "\".";
					errors[4] = "The regular expression that caused parsing failure was: \"" + etc + "\".";
					return errors;
				}
				else
				{
					lastGroup = matcher.group(1);
				}
			}

			pos++;
		}

		return null;
	}

	/// <summary>
	/// Returns a 5-element array of Strings that explains why a String could not
	/// be parsed.  An example of the return array is:<para>
	/// [0] = "Parsing of "2006-05:13" failed."<br>
	/// [1] = "2006-05:13"<br>
	/// [2] = "      ^"<br>
	/// [3] = "The original regular expression used to try parsing was: "
	///      "(\d\d\d\d)-(\d\d)-(\d\d).";<br>
	/// [4] = "The regular expression that caused parsing failure was: "
	///      "(\d\d\d\d)-(\d\d)-."<br>
	/// </para>
	/// </summary>
	/// <param name="inputString"> the inputString to attempt parsing. </param>
	/// <returns> a 5-element String array explaining why the input string could not be parsed. </returns>
	public virtual string[] explainParseFailure(string inputString)
	{
		// This essentially tries to validate the string one character at a 
		// time until it finds where the validation fails.  It can then tell
		// which character in the input string caused the failure.  

		// Given a format String "mm-dd", the following is its regular expression for parsing:
		//	(\d\d)-(\d\d)

		// When this method is called the first time, it will generate and 
		// all the following regular expressions and cache them:
		//	(\d).*
		//	(\d\d).*
		//	(\d\d)-.*
		//	(\d\d)-(\d).*
		//	(\d\d)-(\d\d).*

		// After that (and immediately upon the second to Nth time this method
		// is called), each one is run upon the input string in turn in order
		// to find the point at which the parsing failed.

		// As should be obvious, for long DateTimeFormat strings, this could 
		// turn out to be a lot of regular expressions.  There is almost 
		// certainly a better way to do this, but limited investigation 
		// revealed no leads on alternate methods, at least given Java 1.4.2's 
		// RegEx library.

		if (__failurePatterns == null)
		{
			bool buildPattern = false;
				// Whether to build a Pattern from the current reg ex string
			bool done = false;
				// Whether the entire regex string has been converted to patterns or not.
			bool escape = false;
				// Whether the character needs escaped or not.
			char[] chars = __regularExpressionString.ToString().ToCharArray();
				// The character array to run through in generating the regular expressions
			int length = __regularExpressionString.Length;
			int pos = 0;
			Pattern pattern = null;
			string etc = "";
			string s = "";
			IList<Pattern> patterns = new List<Pattern>();
				// Holds the Pattern objects that are created
			IList<string> patternStrings = new List<string>();
				// Holds the regular expressions used to create the
				// Patterns in the patterns list.

			while (!done)
			{
				buildPattern = false;
				escape = false;

				// Java will handle any Unicode issues as long as the regular 'char' data type is used.
				if (chars[pos] == '\\')
				{
					// An escaped character, add the next char
					s += "\\";
					escape = true;
					pos++;
				}

				if (pos >= length)
				{
					// At the very end of the String, so build the Pattern.
					done = true;
					buildPattern = false;
				}
				else
				{
					if (needsEscaped(chars[pos]))
					{
						if (escape)
						{
							s += chars[pos];
							buildPattern = true;
						}
						else
						{
							buildPattern = false;
						}
					}
					else
					{
						s += chars[pos];
						buildPattern = true;
					}

					if (pos < (length - 1))
					{
						// Check for repeat characters ("*","+","?")
						if (chars[pos + 1] == '*' || chars[pos + 1] == '+' || chars[pos + 1] == '?')
						{
							s += chars[pos + 1];
							pos++;
							buildPattern = true;
						}
					}
				}

				if (pos >= (length - 1))
				{
					// At the end of the String, so build the pattern
					done = true;
					buildPattern = true;
				}

				if (buildPattern)
				{
					etc = "(" + s + ").*";
					pattern = Pattern.compile(etc);
					patterns.Add(pattern);
					patternStrings.Add(etc);
				}

				pos++;
			}

			int size = patterns.Count;

			__failurePatterns = new Pattern[size];
			__failurePatternStrings = new string[size];

			for (int i = 0; i < size; i++)
			{
				__failurePatterns[i] = (Pattern)patterns[i];
				__failurePatternStrings[i] = (string)patternStrings[i];
			}
		}

		Matcher matcher = null;
		string lastGroup = "";

		int size = __failurePatterns.Length;

		bool dieFast = false;

		for (int i = 0; i < size; i++)
		{
			if (__last > 0)
			{
				Matcher pass = __failurePatterns[__last - 1].matcher(inputString);
				Matcher fail = __failurePatterns[__last].matcher(inputString);
				if (pass.matches() && !fail.matches())
				{
					lastGroup = pass.group(1);
					dieFast = true;
					i = __last;
				}
				else
				{
					// not the same error, loop normally
					__last = -1;
				}
			}

			if (!dieFast)
			{
				matcher = __failurePatterns[i].matcher(inputString);
			}

			if (dieFast || !matcher.matches())
			{
				string errorLocation = "";
				for (int j = 0; j < lastGroup.Length; j++)
				{
					errorLocation += " ";
				}
				errorLocation += "^";

				string[] errors = new string[5];
				errors[0] = "Parsing of \"" + inputString + "\" failed.";
				errors[1] = inputString;
				errors[2] = errorLocation;
				errors[3] = "The original regular expression used to try parsing was: \""
					+ __regularExpressionString + "\".";
				errors[4] = "The regular expression that caused parsing failure was: \""
					+ __failurePatternStrings[i] + "\".";
				__last = i;
				return errors;
			}
			else
			{
				lastGroup = matcher.group(1);
			}
		}
		return null;
	}

	private int __last = -1;
	private Pattern[] __failurePatterns;
	private string[] __failurePatternStrings;

	/// <summary>
	/// Once a Regular Expression Matcher has successfully matched a DateTime string 
	/// to a regular expression, move the values that were found with the Matcher into a DateTime object.<para>
	/// This method is called by "parse()" once parse() has determined that a string
	/// being parsed fits the pattern of the Regular Expression Matcher.
	/// </para>
	/// </summary>
	/// <param name="matcher"> the Matcher that was used to match a String to a regular expression. </param>
	/// <param name="dt"> the DateTime object to fill with the matched values.  If null, 
	/// a new DateTime object will be instantiated. </param>
	private DateTime fillDateTime(Matcher matcher, DateTime dt)
	{
		// NOTE (JTS - 2005-10-24)
		// For future reference, should another developer try to improve performance here:
		// - tried using a DateTime instantiated here and returned with the 
		//   values in it.  Each time, the same DateTime would be used rather
		//   than creating one every time.  This offers a decent speed 
		//   increase (~10%), but the returned value has to be used in a copy 
		//   constructor in the calling code, anyway, thus eliminating the 
		//   performance gain.  It's possible this might be useful in some
		//   applications later on, so this should be kept in mind.
		//      In order to do this again, add a static DateTime object to
		//   this class and instantiate it once in the static{} block.  
		//   Call setToZero() on it in this method each time and set its values
		//   here and then return it instead of the one that is instantiated in this method.
		// - turning off STRICT processing, but not turning on FAST (eg, 
		//   passing in a flag value of 0).  Processing actually slowed down 
		//   slightly (~2%) due to how the flags are handled internally in the DateTime class.
		// This is not a REVISIT because there is nothing to REVISIT.  This is
		// a note to future developers who may think there are optimizations
		// that can be done here.

		bool hasTimeZone = false;
			// Whether the date being filled will need to have a time zone set or not
		int highestPrecision = -1;
			// The "highest" precision found while filling the DateTime.  
			// Finer-grained units of time have higher precision, so a 
			// Day has higher precision than Month or Year, but lower
			// precision than Hour, Minutes, etc.
		int val = -1;
			// The integer value of the value matched by the regular
			// expression Matcher.
		string matchedString = null;
			// The String matched by the regular expression Matcher.

		// The following is primarily used to pass in the DATE_FAST flag, which
		// results in a slight (10-15%) performance gain.

		if (dt == null)
		{
			if (__parseDateConstructorFlag != -1)
			{
				dt = new DateTime(__parseDateConstructorFlag);
			}
			else
			{
				dt = new DateTime();
			}
		}

		for (int i = 0; i < __sortedFormatSpecifierTypes.Length; i++)
		{
			if (__sortedFormatSpecifierTypes[i] > highestPrecision && __sortedFormatSpecifierTypes[i] != __TZ)
			{
				highestPrecision = __sortedFormatSpecifierTypes[i];
			}

			// Matcher.group(0) is equivalent to matcher.group(), which
			// returns the entire string that was matched against (i.e., the date string being parsed).

			matchedString = matcher.group(i + 1);

			switch (__sortedFormatSpecifierTypes[i])
			{
				case __YEAR:
					// Special handling is required for 2-digit 
					// years.  The handling method here is to 
					// translate any years less than 30 to mean
					// the 21st century, and any greater than or
					// equal to 30 be in the 20th century.  This is
					// consistent with the standard Microsoft Y2K default handling. 
					if (matchedString.Length == 2)
					{
						val = StringUtil.atoi(matchedString);
						if (val < 30)
						{
							val += 2000;
						}
						else
						{
							val += 1900;
						}
					}
					else
					{
						val = StringUtil.atoi(matchedString);
					}
					dt.setYear(val);
					break;
				case __MONTH:
					val = StringUtil.atoi(matchedString);
					dt.setMonth(val);
					break;
				case __DAY:
					val = StringUtil.atoi(matchedString);
					dt.setDay(val);
					break;
				case __HOUR:
					val = StringUtil.atoi(matchedString);
					dt.setHour(val);
					break;
				case __MINUTE:
					val = StringUtil.atoi(matchedString);
					dt.setMinute(val);
					break;
				case __SECOND:
					val = StringUtil.atoi(matchedString);
					dt.setSecond(val);
					break;
				case __HSECOND:
					// Special handling is required for 1-digit
					// hundredth-of-second values.  Because the
					// first value is the tenths of digits, if it
					// is the only part showing it must be
					// multiplied by 10 to have the proper magnitude.
					if (matchedString.Length == 2)
					{
						val = StringUtil.atoi(matchedString);
					}
					else
					{
						val = StringUtil.atoi(matchedString);
						val *= 10;
					}
					dt.setHSecond(val);
					break;
				case __TZ:
					hasTimeZone = true;
					dt.setTimeZone(matchedString);
					break;
				default:
					break;
			}
		}

		// Determine what the precision of the parsed DateTime should be set to.

		int precision = -1;

		switch (highestPrecision)
		{
			case __YEAR:
				precision = DateTime.PRECISION_YEAR;
				break;
			case __MONTH:
				precision = DateTime.PRECISION_MONTH;
				break;
			case __DAY:
				precision = DateTime.PRECISION_DAY;
				break;
			case __HOUR:
				precision = DateTime.PRECISION_HOUR;
				break;
			case __MINUTE:
				precision = DateTime.PRECISION_MINUTE;
				break;
			case __SECOND:
				precision = DateTime.PRECISION_SECOND;
				break;
			case __HSECOND:
				precision = DateTime.PRECISION_HSECOND;
				break;
			default:
				break;
		}

		if (precision != -1)
		{
			if (hasTimeZone)
			{
				dt.setPrecision(precision | DateTime.PRECISION_TIME_ZONE);
			}
			else
			{
				dt.setPrecision(precision);
			}
		}
		else if (hasTimeZone)
		{
			dt.setPrecision(DateTime.PRECISION_TIME_ZONE);
		}

		return dt;
	}

	/// <summary>
	/// Given a relative date/time and an absolute date/time, this fills the missing 
	/// values in the relative date/time with the values from the absolute date/time.<para>
	/// For example, if the relative date/time was "14:55" and the absolute date/time
	/// was "2006-05-23 12:30:44", the relative date/time would be filled with the
	/// year, month, day and seconds from the absolute date/time, resulting in
	/// a new relative date/time of "2006-05-23 14:55:44".
	/// </para>
	/// </summary>
	/// <param name="relDate"> the relative date/time to fill.  This date/time will be changed in this method. </param>
	/// <param name="absDate"> the absolute date/time to use for filling the relative date/time with. </param>
	public virtual void fillRelativeDateTime(DateTime relDate, DateTime absDate)
	{
		for (int i = 0; i < __datePieces.Length; i++)
		{
			if (!__datePieces[i])
			{
				// should never change
				switch (i)
				{
					case __YEAR:
						relDate.setYear(absDate.getYear());
						break;
					case __MONTH:
						relDate.setMonth(absDate.getMonth());
						break;
					case __DAY:
						relDate.setDay(absDate.getDay());
						break;
					case __HOUR:
						relDate.setHour(absDate.getHour());
						break;
					case __MINUTE:
						relDate.setMinute(absDate.getMinute());
						break;
					case __SECOND:
						relDate.setSecond(absDate.getSecond());
						break;
				}
			}
		}
	}

	/// <summary>
	/// Cleans up member variables. </summary>
	/// <exception cref="Throwable"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~DateTimeFormat()
	{
		// NOTE: 
		// Some data members are cleaned up in cleanup(), so it is unnecessary to finalize them here.

		__datePieces = null;
		__sortedFormatSpecifierLengths = null;
		__sortedFormatSpecifierLocations = null;
		__sortedFormatSpecifierTypes = null;
		__pattern = null;
		IOUtil.nullArray(__sortedFormatSpecifierRegExs);
		IOUtil.nullArray(__formatSpecifierLiteralSurroundingValues);
		__format = null;
		__regularExpressionString = null;

//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Iterates through all the date/time piece format specifiers, and tries to 
	/// find them within the format String.  This method, combined with
	/// findFormatSpecifierOccurrences(), fills the __formatSpecifiers array.
	/// </summary>
	private void findFormatSpecifiers()
	{
		int size = 0;
		for (int i = 0; i < __NUM_PIECES; i++)
		{
			size = __formatSpecifiers[i].Length;
			for (int j = 0; j < size; j++)
			{
				findFormatSpecifierOccurrences(i, j);
			}
		}
	}

	/// <summary>
	/// Finds occurrences of a specific format specifier type in the format String and 
	/// fills in __formatSpecifierLocations with information on the format specifier's 
	/// location within the format String. </summary>
	/// <param name="formatSpecifierType"> the type of format specifier being searched for 
	/// (e.g, __YEAR, __MINUTE). </param>
	/// <param name="formatSpecifierNum"> the number of the format specifier being searched 
	/// for.  This corresponds to the different types of format specifiers available 
	/// for each format specifier type (e.g, for __YEAR, format specifier #0 is "yyyy" 
	/// and format specifier #1 is "yy").  See <code>String[] __formatSpecifiers</code>. </param>
	private void findFormatSpecifierOccurrences(int formatSpecifierType, int formatSpecifierNum)
	{
		string formatSpecifier = __formatSpecifiers[formatSpecifierType][formatSpecifierNum];
			// The format specifier denoted by the parameters to this 
			// method, and which is being sought within the format string

		bool done = false;
			// Whether the method is done searching through the string
		int start = 0;
			// The starting point from which to search through the string for the formatSpecifier.
		int index = __format.IndexOf(formatSpecifier, start, StringComparison.Ordinal);
			// The index of where the formatSpecifier was found within the string
		int length = formatSpecifier.Length;
			// The length of the formatSpecifier
		IList<int> locs = new List<int>();
			// Holds the locations (in Integer format) of the points within
			// the date/time format String where the formatSpecifier can be found

		// Loop through the format String ...

		while (index != -1 && !done)
		{
			if (__formatCharacters[index] == __used)
			{
				// Skip it -- found by a higher-priority format 
				// specifier and marked as used.  "Higher-priority" 
				// means that the format specifier is searched for before others.  
				// For example, if a format is "Year: yyyy", then 
				// "yyyy" will first be matched by the higher-priority
				// format specifier of "yyyy" and processed.  If it
				// were processed by the lower-priority format specifier
				// ("yy"), it would be translated into two two-digit
				// year values, rather than a single four-digit year value.
				start++;
			}
			else if (isEscaped(formatSpecifier, index))
			{
				// Skip -- this is an escaped literal character, not 
				// one that should be in a format specifier.
				start++;
			}
			else
			{
				// Found a format specifier.  First, mark this format 
				// specifier as used so that later format specifier 
				// collections will not attempt to reuse it.
				for (int i = index; i < (index + length); i++)
				{
					__formatCharacters[i] = __used;
				}

				// The above is done so that "yyyy" will only be parsed
				// as a four-digit year, rather than as two two-digit years.

				// If the characters were not marked as __used, they
				// would be matched and re-matched by lower-priority format specifiers.  

				// For instance, in this format string: "yyyy-mm"
				// once "yyyy" is matched, this format becomes 
				// "xxxx-mm" (though instead of "x" it is an EOT character).  

				// If it weren't, then when the next kind of year 
				// format specifier was sought, it would find match
				// against the two "yy" strings in the above format.
				// That would be an error.

				// At the same time, it is legal to re-use a format
				// specifier more than once within a format, so that must be accounted for.

				// Add the current location as one where this format specifier can be found.
				locs.Add(new int?(index));

				// Increment start so that next time a format specifier 
				// is searched for within the string it will not look 
				// at any of the characters occupied by the format specifier.
				start = index + length;
			}

			if (start >= __format.Length)
			{
				done = true;
			}
			else
			{
				index = __format.IndexOf(formatSpecifier, start, StringComparison.Ordinal);
			}
		}

		// Translate the locs list into an array and put its values into
		// __formatSpecifierLocations.

		__formatSpecifierLocations[formatSpecifierType][formatSpecifierNum] = new int[locs.Count];
		int size = locs.Count;
		for (int i = 0; i < size; i++)
		{
			__formatSpecifierLocations[formatSpecifierType][formatSpecifierNum][i] = ((int?)locs[i]).Value;
		}
	}

	/// <summary>
	/// Formats a DateTime object according to the DateTimeFormat string this 
	/// DateTimeFormat object was constructed with. </summary>
	/// <param name="dt"> the DateTime object to format. </param>
	/// <returns> a String representation of the DateTime object. </returns>
	public virtual string format(DateTime dt)
	{
		if (__sortedFormatSpecifierLocations.Length == 0 || dt == null)
		{
			return "";
		}

		int val = -1;
		string format = null;
		string temp = null;
		string tz = null;
		StringBuilder sb = new StringBuilder();

		for (int i = 0; i < __sortedFormatSpecifierLocations.Length; i++)
		{
			val = -1;
			tz = null;
			format = null;

			// Add the literal characters that precede the 
			// current format specifier in the format String.

			sb.Append(__formatSpecifierLiteralSurroundingValues[i]);

			// Based on the type if the current format specifier, retrieve 
			// the appropriate value from the DateTime.

			switch (__sortedFormatSpecifierTypes[i])
			{
				case __YEAR:
					val = dt.getYear();
					break;
				case __MONTH:
					val = dt.getMonth();
					break;
				case __DAY:
					val = dt.getDay();
					break;
				case __HOUR:
					val = dt.getHour();
					break;
				case __MINUTE:
					val = dt.getMinute();
					break;
				case __SECOND:
					val = dt.getSecond();
					break;
				case __HSECOND:
					val = dt.getHSecond();
					break;
				case __TZ:
					tz = dt.getTimeZoneAbbreviation();
					break;
				default:
					break;
			}

			if (__sortedFormatSpecifierTypes[i] != __TZ)
			{
				if (__sortedFormatSpecifierTypes[i] == __YEAR)
				{
					// Years need to be handled specially.  4-digit years can be output as they are, but 
					// otherwise, just take the final two digits from the year and output it.
					if (__sortedFormatSpecifierLengths[i] != 4)
					{
						// Formats the output as a two-digit year.  First, the year has to be
						// modded to get only the tends and ones values from it.  
						val = val % 100;
						temp = StringUtil.formatString(val, "%02d");
						sb.Append(temp);
					}
					else
					{
						// Formats the date as a string at least 4 characters long (errors will
						// occur with years greater than 9999)
						format = "%0" + __sortedFormatSpecifierLengths[i] + "d";
						sb.Append(StringUtil.formatString(val, format));
					}
				}
				else if (__sortedFormatSpecifierTypes[i] == __HSECOND)
				{
					// Hundredths-of-seconds must be handled
					// specially.  If the formatting string is "ff"
					// then the full value is printed, but otherwise
					// the tens value of the number must be printed
					// and the ones value must be removed.
					temp = "" + StringUtil.formatString(val,"%02d");
					if (__sortedFormatSpecifierLengths[i] == 1)
					{
						temp = temp.Substring(0, 1);
					}
					sb.Append(temp);
				}
				else
				{
					// All other types are simple formatting.
					format = "%0" + __sortedFormatSpecifierLengths[i] + "d";
					sb.Append(StringUtil.formatString(val, format));
				}
			}
			else
			{
				// Time Zones can be handled separately, and simply.
				format = "%s";
				sb.Append(StringUtil.formatString(tz, format));
			}
		}

		// Finally, append the literal characters that appear at the end of the format String.

		sb.Append(__formatSpecifierLiteralSurroundingValues[__formatSpecifierLiteralSurroundingValues.Length - 1]);

		return sb.ToString();
	}

	/// <summary>
	/// Returns the String format used to create this DateTimeFormat. </summary>
	/// <returns> the String format used to create this DateTimeFormat. </returns>
	public virtual string getFormat()
	{
		return __format;
	}

	/// <summary>
	/// Given a range of precisions, returns formats that can be displayed as choices 
	/// for formatting.  The precisions passed to this method filter the choices to be shown.  <para>
	/// No formats containing values
	/// less precise than the first precision will be shown, and no formats containing
	/// values more precise than the second precision will be shown.  For instance,
	/// with a first precision of Year and a second precision of Day, the following
	/// would be among the choices returned: "yyyy-mm-dd", whereas with a first 
	/// parameter of Month and second parameter of day, it would never be because it
	/// contains year formatting information, and that is less precise than Month.
	/// </para>
	/// </summary>
	/// <param name="maxInterval"> the highest (least precise) interval for which to list 
	/// formats. This value should be less precise or equal to than the minInterval 
	/// value.  For instance, if this value is DateTime.PRECISION_MONTH, the other 
	/// parameter should not be DateTime.PRECISION_YEAR, but could be DateTime.PRECISION_DAY. </param>
	/// <param name="minInterval"> the lowest (most precise) interval for which to list 
	/// formats. This value should be more precise or equal to than the maxInterval 
	/// value.  For instance, if this value is DateTime.PRECISION_MONTH, the other 
	/// parameter should not be DateTime.PRECISION_MINUTE, but could be DateTime.PRECISION_YEAR. </param>
	/// <param name="showExample"> if true, then the formats shown in the choices will be
	/// followed by an example date formatted with the given format. </param>
	/// <returns> a list of choices suitable for use in a JComboBox. </returns>
	public static IList<string> getFormatChoices(int maxInterval, int minInterval, bool includeTZ, bool showExample)
	{
		IList<string> choices = new List<string>();
		if (maxInterval == DateTime.PRECISION_YEAR)
		{
			if (minInterval == DateTime.PRECISION_YEAR)
			{
				choices.Add("yyyy");
				choices.Add("yy");
			}
			else if (minInterval == DateTime.PRECISION_MONTH)
			{
				choices.Add("yyyy-mm");
				choices.Add("yyyy/mm");
				choices.Add("yy/mm");
				choices.Add("yy-mm");
				choices.Add("mm-yyyy");
				choices.Add("mm/yyyy");
				choices.Add("mm/yy");
				choices.Add("mm-yy");
			}
			else if (minInterval == DateTime.PRECISION_DAY)
			{
				choices.Add("yyyy-mm-dd");
				choices.Add("yyyy/mm/dd");
				choices.Add("yyyy-dd-mm");
				choices.Add("yyyy/dd/mm");
				choices.Add("mm-dd-yyyy");
				choices.Add("mm/dd/yyyy");
				choices.Add("dd-mm-yyyy");
				choices.Add("dd/mm/yyyy");
			}
			else if (minInterval == DateTime.PRECISION_HOUR)
			{
				choices.Add("yyyy-mm-dd hh");
				choices.Add("yyyy/mm/dd hh");
				choices.Add("yyyy-dd-mm hh");
				choices.Add("yyyy/dd/mm hh");
				choices.Add("mm-dd-yyyy hh");
				choices.Add("mm/dd/yyyy hh");
				choices.Add("dd-mm-yyyy hh");
				choices.Add("dd/mm/yyyy hh");
			}
			else if (minInterval == DateTime.PRECISION_MINUTE)
			{
				choices.Add("yyyy-mm-dd hh:MM");
				choices.Add("yyyy/mm/dd hh:MM");
				choices.Add("yyyy-dd-mm hh:MM");
				choices.Add("yyyy/dd/mm hh:MM");
				choices.Add("mm-dd-yyyy hh:MM");
				choices.Add("mm/dd/yyyy hh:MM");
				choices.Add("dd-mm-yyyy hh:MM");
				choices.Add("dd/mm/yyyy hh:MM");
			}
			else if (minInterval == DateTime.PRECISION_SECOND)
			{
				choices.Add("yyyy-mm-dd hh:MM:ss");
				choices.Add("yyyy/mm/dd hh:MM:ss");
				choices.Add("yyyy-dd-mm hh:MM:ss");
				choices.Add("yyyy/dd/mm hh:MM:ss");
				choices.Add("mm-dd-yyyy hh:MM:ss");
				choices.Add("mm/dd/yyyy hh:MM:ss");
				choices.Add("dd-mm-yyyy hh:MM:ss");
				choices.Add("dd/mm/yyyy hh:MM:ss");
			}
			else if (minInterval == DateTime.PRECISION_HSECOND)
			{
				choices.Add("yyyy-mm-dd hh:MM:ss.ff");
				choices.Add("yyyy/mm/dd hh:MM:ss.ff");
				choices.Add("yyyy-dd-mm hh:MM:ss.ff");
				choices.Add("yyyy/dd/mm hh:MM:ss.ff");
				choices.Add("mm-dd-yyyy hh:MM:ss.ff");
				choices.Add("mm/dd/yyyy hh:MM:ss.ff");
				choices.Add("dd-mm-yyyy hh:MM:ss.ff");
				choices.Add("dd/mm/yyyy hh:MM:ss.ff");
			}
		}
		else if (maxInterval >= DateTime.PRECISION_MONTH)
		{
			if (minInterval == DateTime.PRECISION_MONTH)
			{
				choices.Add("mm");
				choices.Add("m");
			}
			else if (minInterval == DateTime.PRECISION_DAY)
			{
				choices.Add("mm-dd");
				choices.Add("mm/dd");
			}
			else if (minInterval == DateTime.PRECISION_HOUR)
			{
				choices.Add("mm-dd hh");
				choices.Add("mm/dd hh");
			}
			else if (minInterval == DateTime.PRECISION_MINUTE)
			{
				choices.Add("mm-dd hh:MM");
				choices.Add("mm/dd hh:MM");
			}
			else if (minInterval == DateTime.PRECISION_SECOND)
			{
				choices.Add("mm-dd hh:MM:ss");
				choices.Add("mm/dd hh:MM:ss");
			}
			else if (minInterval == DateTime.PRECISION_HSECOND)
			{
				choices.Add("mm-dd hh:MM:ss.ff");
				choices.Add("mm/dd hh:MM:ss.ff");
			}
		}
		else if (maxInterval >= DateTime.PRECISION_DAY)
		{
			if (minInterval == DateTime.PRECISION_DAY)
			{
				choices.Add("dd");
			}
			else if (minInterval == DateTime.PRECISION_HOUR)
			{
				choices.Add("dd hh");
			}
			else if (minInterval == DateTime.PRECISION_MINUTE)
			{
				choices.Add("dd hh:MM");
			}
			else if (minInterval == DateTime.PRECISION_SECOND)
			{
				choices.Add("dd hh:MM:ss");
			}
			else if (minInterval == DateTime.PRECISION_HSECOND)
			{
				choices.Add("dd hh:MM:ss.ff");
			}
		}
		else if (maxInterval >= DateTime.PRECISION_HOUR)
		{
			if (minInterval == DateTime.PRECISION_HOUR)
			{
				choices.Add("hh");
			}
			else if (minInterval == DateTime.PRECISION_MINUTE)
			{
				choices.Add("hh:MM");
			}
			else if (minInterval == DateTime.PRECISION_SECOND)
			{
				choices.Add("hh:MM:ss");
			}
			else if (minInterval == DateTime.PRECISION_HSECOND)
			{
				choices.Add("hh:MM:ss.ff");
			}
		}
		else if (maxInterval >= DateTime.PRECISION_MINUTE)
		{
			if (minInterval == DateTime.PRECISION_MINUTE)
			{
				choices.Add("MM");
			}
			else if (minInterval == DateTime.PRECISION_SECOND)
			{
				choices.Add("MM:ss");
			}
			else if (minInterval == DateTime.PRECISION_HSECOND)
			{
				choices.Add("MM:ss.ff");
			}
		}
		else if (maxInterval >= DateTime.PRECISION_SECOND)
		{
			if (minInterval == DateTime.PRECISION_SECOND)
			{
				choices.Add("ss");
			}
			else if (minInterval == DateTime.PRECISION_HSECOND)
			{
				choices.Add("ss.ff");
			}
		}
		else if (maxInterval >= DateTime.PRECISION_HSECOND)
		{
			if (minInterval == DateTime.PRECISION_HSECOND)
			{
				choices.Add("ff");
			}
		}

		if (includeTZ)
		{
			IList<string> v = new List<string>();
			int size = choices.Count;
			string s = null;
			for (int i = 0; i < size; i++)
			{
				s = choices[i];
				s += " tz";
				v.Add(s);
			}
			choices = v;
		}

		if (showExample)
		{
			DateTime dt = new DateTime(DateTime.DATE_CURRENT);
			DateTimeFormat dtf = null;
			IList<string> v = new List<string>();
			int size = choices.Count;
			string s = null;
			for (int i = 0; i < size; i++)
			{
				s = choices[i];
				dtf = new DateTimeFormat(s);
				v.Add(s + " (" + dtf.format(dt) + ")");
			}
			choices = v;
		}

		return choices;
	}

	/// <summary>
	/// Returns the current date/time formatted in the specified format, suitable for display in a GUI. </summary>
	/// <param name="format"> the format for which to return a sample DateTime. </param>
	/// <returns> a date string formatted with the given format, or null if an error occurred formatting the date. </returns>
	public static string getSampleDateString(string format)
	{
		return getSampleDateString(null, format);
	}

	/// <summary>
	/// Returns the specified date formatted in the given format, suitable for display in a GUI. </summary>
	/// <param name="dt"> the DateTime to format in the given format. </param>
	/// <param name="format"> the format for which to return a sample DateTime. </param>
	/// <returns> a date string formatted with the given format, or null if an error occurred formatting the date. </returns>
	public static string getSampleDateString(DateTime dt, string format)
	{
		if (dt == null)
		{
			dt = new DateTime(DateTime.DATE_CURRENT);
		}
		DateTimeFormat dtf = new DateTimeFormat(format);
		return dtf.format(dt);
	}

	/// <summary>
	/// Returns a copy of the __sortedFormatSpecifierTypes array for use in isAbsolute(). </summary>
	/// <returns> a copy of the __sortedFormatSpecifierTypes array for use in isAbsolute(). </returns>
	private int[] getSortedFormatSpecifierTypes()
	{
		if (__sortedFormatSpecifierTypes == null)
		{
			return null;
		}
		else
		{
			return (int[])(__sortedFormatSpecifierTypes.Clone());
		}
	}

	/// <summary>
	/// Returns whether the format includes the given date/time piece.  This can be 
	/// used to determine easily whether a format will display years, for instance, by:<para>
	/// <code><pre>
	/// DateTimeFormat dtf = ...
	/// ...
	/// if (dtf.hasFormatSpecifier(DateTimeFormat.__YEAR)) {
	///		...
	/// }
	/// </pre></code>
	/// </para>
	/// </summary>
	/// <param name="datePiece"> one of __YEAR, __MONTH, __DAY, etc. </param>
	/// <returns> whether the format includes the given date/time piece. </returns>
	/* FIXME SAM 2007-05-09 Evaluate whether needed
	private boolean hasFormatSpecifier(int datePiece) {
		if (datePiece < 0 || datePiece > __NUM_PIECES) {
			return false;
		}
		return __datePieces[datePiece];
	}
	*/

	/// <summary>
	/// Returns whether the format in this Object is absolute or not.  See isAbsolute(String format). </summary>
	/// <returns> if the date format is absolute, false if not. </returns>
	public virtual bool isAbsolute()
	{
		return __isAbsolute;
	}

	/// <summary>
	/// Checks to see if a DateTimeFormat string represents an absolute date or not.
	/// A date is considered absolute if it contains Year, or Year and Month, or Year
	/// and Month and Day, etc., on through Year to Hundredth-of-second.  If the
	/// format refers to a specific point in time, then it is absolute.  If not, it is relative.<para>
	/// </para>
	/// For example:<para>
	/// "yyyy-mm" specifies to a specific year and month.  Though it is not any more 
	/// </para>
	/// precise than that, it is still a specific point in time and is absolute.<para>
	/// "yyyy-dd" specifies a specific year and specific day, but the month is not 
	/// </para>
	/// specified and thus this format could refer to any month within the given year. It is not absolute.<para>
	/// "mm-dd" specifies a specific month and day, but not a specific year and thus
	/// could apply to any year.  It is not absolute.
	/// </para>
	/// </summary>
	/// <param name="format"> the DateTime format String to check. </param>
	/// <returns> true if the format represents an absolute date, false if not. </returns>
	public static bool isAbsolute(string format)
	{
		return isAbsolute(format, new DateTimeFormat(format));
	}

	/// <summary>
	/// Checks to see if a DateTimeFormat string represents an absolute date or not.
	/// This method is different from the public static version in that it takes an 
	/// instance of a DateTimeFormat -- this is done to avoid infinite recursive when
	/// determining if a new instance of a DateTimeFormat is absolute, in its 
	/// constructor.  See the DateTimeFormat constructor to see how this is called. </summary>
	/// <param name="format"> the DateTime format String to check. </param>
	/// <param name="dtf"> the DateTimeFormat created with the given format. </param>
	/// <returns> true if the format represents an absolute date, false if not. </returns>
	private static bool isAbsolute(string format, DateTimeFormat dtf)
	{
		int[] sortedTypes = dtf.getSortedFormatSpecifierTypes();

		bool[] types = new bool[__NUM_PIECES];
		int i = 0;
		for (i = 0; i < __NUM_PIECES; i++)
		{
			types[i] = false;
		}

		for (i = 0; i < sortedTypes.Length; i++)
		{
			types[sortedTypes[i]] = true;
		}

		// find out many consecutive trues there are starting at zero
		i = 0;
		while (i < __NUM_PIECES && types[i])
		{
			i++;
		}

		if (i == 0)
		{
			return false;
		}

		while (i < __NUM_PIECES && !types[i])
		{
			i++;
		}

		if (i < (__NUM_PIECES - 1))
		{
			return false;
		}

		return true;
	}

	/// <summary>
	/// Checks to see whether the format specifier at a given position is preceded by an
	/// escape character and thus will never be in a date format specifier.<para>
	/// </para>
	/// </summary>
	/// <param name="formatSpecifier"> the format specifier to check. </param>
	/// <param name="index"> the index at which the format specifier was found. </param>
	/// <returns> true if the first character of the string was escaped with a backslash
	/// and is thus not in a format specifier after all.  Or false, if the first 
	/// character was not escaped and thus this is an actual date format specifier. </returns>
	private bool isEscaped(string formatSpecifier, int index)
	{
		if (index == 0)
		{
			// Impossible to have a "\" in the -1st position, so this is a valid formatSpecifier.
			return false;
		}

		if (index == 1)
		{
			// Only have to check back one character (not possible
			// to have another "\" at the -1st position)
			if (__format.IndexOf("\\" + formatSpecifier, StringComparison.Ordinal) == 0)
			{
				return true;
			}
			return false;
		}
		else
		{
			// Look back through the string to make sure that if this 
			// format specifier is escaped, that its escape value isn't 
			// actually escaped itself.  That is, this:
			// 	\\mm  
			//	(an escaped "\" followed by a two-digit month format specifier)
			// is NOT the same as:
			// 	\mm
			//	(an escaped "m" followed by a one-digit month format specifier)
			int count = 0;
			int back = index - 1;
			while (back >= 0 && __format[back] == '\\')
			{
				count++;
				back--;
			}
			if (count % 2 != 0)
			{
				return true;
			}
			return false;
		}
	}

	/// <summary>
	/// Checks to see if any format specifier was marked as occurring at the given index in the format String. </summary>
	/// <param name="index"> the index at which to see if any format specifier is found. </param>
	/// <returns> the index of the format specifier (within the __sortedFormatSpecifier* 
	/// arrays) found at the specified index, or -1 if none occur at the given position. </returns>
	private int isMarkedPosition(int index)
	{
		for (int i = 0; i < __sortedFormatSpecifierLocations.Length; i++)
		{
			if (__sortedFormatSpecifierLocations[i] == index)
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// Increments to the next relative date time from the given date time.  None of the
	/// fields that are specified in the format for this DateTimeFormat will be changed,
	/// but the others will increment and iterate properly.  The fields that are not
	/// specified in the format will iterate from whatever their current values in the
	/// DateTime object are.  Starting values for these fields must be specified 
	/// in the calling code, or they will default to 0s and 1s.<para>
	/// A different format must be used to show the iterated values, as they are not
	/// </para>
	/// present in the DateTimeFormat object that is used to iterate.<para>
	/// Relative DateTimeFormats offer the ability to specify a date that is not tied
	/// to a specific point in time.  "July 4th" could be 2005-07-04, 1996-07-04,
	/// 1723-07-04, etc.  Because relative dates do not specify a single point in time,
	/// they can be used to specify a date that repeats in time.  The date/time 
	/// pieces that are present in the format (in the above, Month and Day) remain 
	/// constant, while the other date/time pieces can vary.  This method offers an 
	/// </para>
	/// easy way to repeat a relative date/time across time.<para>
	/// This method only does a single increment of the relative DateTime, and so 
	/// </para>
	/// iteration must be handled in the calling code.<para>
	/// For example, this is used in annotation drawing code on a TSGraph to specify
	/// a range of dates for an alert level.  An alert level line is specified by 
	/// two dates in the format: "mm/dd".  The left-most year on the graph is used as
	/// the starting year and the dates are iterated (with this method) through all the
	/// </para>
	/// years drawn on the graph.<para>
	/// This concept is confusing, and it is best illustrated by examples. 
	/// The following show the format of the
	/// DateTimeFormat, the original value of the DateTime, and its values for 5
	/// iterations.  All the iterated values are shown with the following format:
	/// </para>
	/// yyyy-mm-dd hh:MM<para>
	/// </para>
	/// <b>Example 1</b><para>
	/// </para>
	/// "mm hh:MM"  (2005-10-29 11:30)<para>
	/// This example can be used to iterate through all the days in October for every 
	/// </para>
	/// year, when the time is 11:30 AM.<para>
	/// - 2005-10-30 11:30<br>
	/// - 2005-10-31 11:30<br>
	/// - 2006-10-01 11:30<br>
	/// - 2006-10-02 11:30<br>
	/// </para>
	/// - 2006-10-03 11:30<para>
	/// </para>
	/// <b>Example 2</b><para>
	/// </para>
	/// "mm-dd MM"  (2005-03-18 20:00)<para>
	/// </para>
	/// This example will iterate through the start of all the hours in March 18, for every year.<para>
	/// - 2005-03-18 21:00<br>
	/// - 2005-03-18 22:00<br>
	/// - 2005-03-18 23:00<br>
	/// - 2006-03-18 00:00<br>
	/// </para>
	/// - 2007-03-18 01:00<para>
	/// </para>
	/// <b>Example 3</b><para>
	/// </para>
	/// "hh:MM:ss" (2005-12-28 12:45:15)<para>
	/// </para>
	/// This example will iterate through every day of every year, at 12:45:15.<para>
	/// - 2005-12-29 12:45:15<br>
	/// - 2005-12-30 12:45:15<br>
	/// - 2005-12-31 12:45:15<br>
	/// - 2006-01-01 12:45:15<br>
	/// </para>
	/// - 2006-01-02 12:45:15<para>
	/// <b>Note:</b> The DateTime object passed into this method is changed internally,
	/// so if its original values should be maintained, a copy should be made prior to
	/// calling this method.
	/// </para>
	/// </summary>
	/// <param name="dt"> the DateTime to iterate.  This DateTime object is changed within the
	/// method and returned, so if its original values should be retained a copy should be made. </param>
	/// <returns> the next relative DateTime forward in time from the given date.
	/// TODO (JTS - 2005-11-01) Perhaps transition this into nextRelativeDateTime / previousRelativeDateTime
	/// in the future when iteration in both directions is necessary. </returns>
	public virtual DateTime iterateRelativeDateTime(DateTime dt)
	{
		int[] vals = new int[7];
		vals[0] = dt.getYear();
		vals[1] = dt.getMonth();
		vals[2] = dt.getDay();
		vals[3] = dt.getHour();
		vals[4] = dt.getMinute();
		vals[5] = dt.getSecond();
		vals[6] = dt.getHSecond();

		bool start = false;
		bool previousLoop = false;
		bool done = false;

		for (int i = (__NUM_PIECES - 2); i >= 0; i--)
		{
			if (__datePieces[i])
			{
				previousLoop = false;
			}

			if (start && __datePieces[i])
			{
				// should never change
				switch (i)
				{
					case __YEAR:
						dt.setYear(vals[__YEAR]);
						break;
					case __MONTH:
						dt.setMonth(vals[__MONTH]);
						break;
					case __DAY:
						dt.setDay(vals[__DAY]);
						break;
					case __HOUR:
						dt.setHour(vals[__HOUR]);
						break;
					case __MINUTE:
						dt.setMinute(vals[__MINUTE]);
						break;
					case __SECOND:
						dt.setSecond(vals[__SECOND]);
						break;
				}
			}
			else if (start && !__datePieces[i] && !done)
			{
				switch (i)
				{
					case __YEAR:
						if (!previousLoop)
						{
							dt.addYear(1);
						}
						break;
					case __MONTH:
						if (!previousLoop)
						{
							dt.addMonth(1);
						}
						previousLoop = false;
						if (dt.getMonth() == 1)
						{
							previousLoop = true;
						}
						else
						{
							done = true;
						}
						break;
					case __DAY:
						if (!previousLoop)
						{
							dt.addDay(1);
						}
						previousLoop = false;
						if (dt.getDay() == 1)
						{
							previousLoop = true;
						}
						else
						{
							done = true;
						}
						break;
					case __HOUR:
						if (!previousLoop)
						{
							dt.addHour(1);
						}
						previousLoop = false;
						if (dt.getHour() == 0)
						{
							previousLoop = true;
						}
						else
						{
							done = true;
						}
						break;
					case __MINUTE:
						if (!previousLoop)
						{
							dt.addMinute(1);
						}
						previousLoop = false;
						if (dt.getMinute() == 0)
						{
							previousLoop = true;
						}
						else
						{
							done = true;
						}
						break;
					case __SECOND:
						if (!previousLoop)
						{
							dt.addSecond(1);
						}
						previousLoop = false;
						if (dt.getSecond() == 0)
						{
							previousLoop = true;
						}
						else
						{
							done = true;
						}
						break;
				}
			}
			else if (!start && __datePieces[i])
			{
				// first one found
				start = true;
			}
		}

		return dt;
	}

	/// <summary>
	/// Checks a character to see whether it needs to be escaped prior to being placed
	/// within a regular expression as a character literal. </summary>
	/// <param name="ch"> the character to check. </param>
	/// <returns> true if the character needs escaped, false if not. </returns>
	private bool needsEscaped(char ch)
	{
		if (ch >= (char)48 && ch <= (char)57)
		{
			// ASCII number values
			return false;
		}
		else if (ch >= (char)65 && ch <= (char)90)
		{
			// ASCII upper case
			return false;
		}
		else if (ch >= (char)97 && ch <= (char)122)
		{
			// ASCII lower case		
			return false;
		}
		else if (ch == ' ' || ch == ':')
		{
			return false;
		}
		return true;
	}

	/// <summary>
	/// Parses the given input date and returns a DateTime containing its values.  If
	/// the input string could not be parsed, null will be returned.  See 
	/// <code>explainParseFailure()</code> if further information is necessary as to why a parse failed. </summary>
	/// <param name="inputString"> the input string containing a date to parse. </param>
	/// <returns> a DateTime with the given input String's values. </returns>
	/// <exception cref="Exception"> if the inputString could not be parsed. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public DateTime parse(String inputString) throws Exception
	public virtual DateTime parse(string inputString)
	{
		return parse(inputString, null);
	}

	/// <summary>
	/// Parses the given input date and returns a DateTime containing its values.  If
	/// the input string could not be parsed, null will be returned.  See 
	/// <code>explainParseFailure()</code> if further information is necessary as to why a parse failed. </summary>
	/// <param name="inputString"> the input string containing a date to parse. </param>
	/// <param name="dt"> the DateTime object to fill with the values in the given 
	/// inputString. </param>
	/// <returns> a DateTime with the given input String's values.  If null, a new one will be created. </returns>
	/// <exception cref="Exception"> if the inputString could not be parsed. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public DateTime parse(String inputString, DateTime dt) throws Exception
	public virtual DateTime parse(string inputString, DateTime dt)
	{
		Matcher matcher = __pattern.matcher(inputString);

		if (!matcher.matches())
		{
			throw new Exception("Could not parse the input string \"" + inputString + "\".");
		}
		else
		{
			return fillDateTime(matcher, dt);
		}
	}

	/// <summary>
	/// Sets the flag with which the DateTime constructor will be called when a 
	/// DateTime is parsed from an input String.  By default, new DateTime objects are
	/// created with the default constructor, which specifies a flag of 
	/// DateTime.DATE_STRICT.  Passing in a value of <code>DateTime.DATE_FAST</code> 
	/// will result in a slight performance gain when parsing DateTimes.  Setting to 
	/// fast by default is not enabled because setting to fast disables some date 
	/// checks (e.g., finding the day of the year and leap-year status), which may not 
	/// be desirable in all situations. </summary>
	/// <param name="flag"> the flag to call DateTime constructors with in parse(). </param>
	public virtual void setParseDateTimeConstructorFlag(int flag)
	{
		__parseDateConstructorFlag = flag;
	}

	/// <summary>
	/// Returns a String representation of this object. </summary>
	/// <returns> a String representation of this object. </returns>
	public override string ToString()
	{
		return ToString(false);
	}

	/// <summary>
	/// Returns a String representation of this object. </summary>
	/// <param name="verbose"> if true, much extra information on the internal data members
	/// is printed, as an aid to debugging parsing. </param>
	/// <returns> a String representation of this object. </returns>
	public virtual string ToString(bool verbose)
	{
		if (!verbose)
		{
			return "DateTimeFormat\n"
				+ "Format: '" + __format + "'\n"
				+ "Regular Expression: '"
				+ __regularExpressionString + "'\n";
		}
		else
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("DateTimeFormat \n" + "  Format: '" + __format + "'\n" + "  Regular Expression: '" + __regularExpressionString + "'\n" + "DateTime Constructor Flag: " + __parseDateConstructorFlag + "\n\n" + "Sorted FormatSpecifier Data:\n");

			if (__sortedFormatSpecifierLengths.Length == 1)
			{
				sb.Append("   (1 FormatSpecifier)");
			}
			else
			{
				sb.Append("   (" + __sortedFormatSpecifierLengths.Length + " FormatSpecifiers)\n");
			}

			for (int i = 0; i < __sortedFormatSpecifierLengths.Length; i++)
			{
				sb.Append("   [" + i + "] Loc: " + __sortedFormatSpecifierLocations[i] + "   Len: " + __sortedFormatSpecifierLengths[i] + "   Type: " + ToString(__sortedFormatSpecifierTypes[i]) + "  RegEx: '" + __sortedFormatSpecifierRegExs[i] + "'\n");
			}

			sb.Append("\nLiteral Surrounding Values:\n");
			for (int i = 0; i < __formatSpecifierLiteralSurroundingValues.Length; i++)
			{
				sb.Append("   [" + i + "]: '" + __formatSpecifierLiteralSurroundingValues[i] + "'\n");
			}

			sb.Append("\nDate Pieces:\n");
			for (int i = 0; i < __datePieces.Length; i++)
			{
				sb.Append("   [" + ToString(i) + "]: " + __datePieces[i] + "\n");
			}

			return sb.ToString();
		}
	}

	/// <summary>
	/// Converts an integer representation of a date/time piece (__YEAR, __MONTH, __DAY, etc) into a String. </summary>
	/// <returns> a String representation of a date/time piece. </returns>
	public virtual string ToString(int datePiece)
	{
		switch (datePiece)
		{
			case __YEAR:
				return "Year";
			case __MONTH:
				return "Month";
			case __DAY:
				return "Day";
			case __HOUR:
				return "Hour";
			case __MINUTE:
				return "Minute";
			case __SECOND:
				return "Second";
			case __HSECOND:
				return "HSecond";
			case __TZ:
				return "TZ";
			default:
				return "Invalid value: " + datePiece;
		}
	}

	}

}