using System;
using System.Collections.Generic;
using System.Text;

// StringUtil - string functions

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

// ----------------------------------------------------------------------------
// StringUtil - string functions
// ----------------------------------------------------------------------------
// Notes:	(1)	This class contains public static RTi string utility
//			functions.  They are essentially equivalent to the
//			HMData C library routines, except for overloading and
//			use of Java classes.
//		(2)	The debug messages in this code HAS been wrapped with
//			isDebugOn.
// ----------------------------------------------------------------------------
// History:
//
// 14 Mar 1998	Steven A. Malers, RTi	Add javadoc.
// 08 Apr 1998	SAM, RTi		Fix the Double toString conversion
//					problem.
// 09 Apr 1998	SAM, RTi		Add arrayToVector and VectorToArray.
// 14 Apr 1999	SAM, RTi		Add toString that takes a vector and
//					line separator to convert report
//					vectors into something that can display
//					in a TextArea.
// 23 Jun 1999	SAM, RTi		Add matchesRegExp.
// 28 Jun 1999	Catherine E.		Add indexOf to return index of string
//		Nutting-Lane, RTi	in string list.
// 01 Aug 1999	SAM, RTi		Add to unpad() the ability to unpad the
//					entire string.
// 26 Oct 1999	CEN, RTi		Check for "+" in fixedRead when reading
//					Integer.
// 03 Dec 1999	SAM, RTi		Add fix where formatString was crashing
//					when the number of format strings was
//					> the the number of data to process.
//					Optimize code a little by using literal
//					name for routine in formatString, set
//					intermediate vectors to null when done
//					in overloaed versions.
// 12 Oct 2000	SAM, RTi		Add \r to list of whitespace characters
//					in default for unpad().  Fix where
//					a one-character line was not getting
//					unpadded at the end.
// 13 Dec 2000	SAM, RTi		Add containsIgnoreCase().
// 02 Jan 2001	SAM, RTi		Add firstToken() and count().
// 15 Jan 2001	SAM, RTi		Add tokenCount() and change count() to
//					patternCount().  Add getToken() and
//					deprecate firstToken().
// 11 Apr 2001	SAM, RTi		Fix bug where breakStringList() was not
//					properly handling empty quoted strings.
// 01 Oct 2001	SAM, RTi		Overload formatString() to take Double
//					and Integer.
// 2001-12-05	SAM, RTi		Clean up sortStringList().
// 2001-12-20	SAM, RTi		Change so that fixedRead() does not
//					return the non-data items in the
//					return Vector.  This requires that all
//					code that use the method be updated
//					accordingly.  Also overload
//					so the format can be passed in using
//					arrays of specifiers (elimating the need
//					to repetitively parse a format string),
//					and, optionally, pass in the Vector to
//					fill so that it can be reused.
//					All of this should result in a marked
//					increase in performance, especially
//					in cases where the format used
//					repeatedly (e.g., model input files).
//					Minor cleanup in ato*() methods.
// 2002-01-08	SAM, RTi		Update sortStringList() to allow a
//					sorted index to be manipulated, similar
//					to the MathUtil sort methods.
// 2002-02-03	SAM, RTi		Overload matchesRegExp() to ignore case
//					and fix a bug where candidate strings
//					that were not at least as long as the
//					regular expression string caused an
//					exception.
// 2002-05-01	SAM, RTi		Add remove().
// 2002-06-19	SAM, RTi		Add lineWrap().
// 2002-09-03	J. Thomas Sapienza, RTi	Add replaceString().
// 2002-09-05	SAM, RTi		Fix bug in breakStringList() where
//					quoted strings were not being handle
//					correctly in all cases and rework code
//					to improve performance some.  Keep
//					around the old version as
//					breakStringListOld() to allow
//					comparisons when problems arise.
// 2002-10-03	SAM, RTi		Found that the new breakStringList() did
//					not behave the same as the old when
//					delimiters are at the beginning of the
//					string (old skipped).  Make the new
//					version behave the same as the old.
// 2002-10-14	Morgan Love, RTi	Added methods:
//					String byteToHex( byte b )
//						which Returns hex String 
//						representation of byte b.
//					String charToHex(char c)
//						which returns hex String 
//						representation of char c.
//					Both Methods originated in the NWSRFS
//					GUI StringUtil class: UnicodeFormatter.
//					These methods are similar to copyrighted
//					versions from Sun (OK for non-commercial
//					use).  However the code is very basic
//					and hopefully there is no issue with RTi
//					using - probably just the standard Sun
//					policy on released code.
//					Add maxSize() to return the longest
//					String in a Vector.
// 2003-03-06	SAM, RTi		Review replaceString() and change from
//					private to public.
// 2003-03-25	SAM, RTi		Change round() to use Long internally
//					because some very large numbers are
//					being encountered.
// 2003-06-04	SAM, RTi		Add endsWithIgnoreCase().
// 2003-06-11	JTS, RTi		Added startsWithIgnoreCase().
// 2003-06-12	JTS, RTi		Fixed bug in replaceString that resulted
//					in it not working if the string (s2)
//					that was replacing the other string 
//					(s1) contained s1 as a substring
// 2003-12-10	SAM, RTi		Add toVector() to take an Enumeration.
// 2003-12-17	SAM, RTi		Add removeDuplicates(Vector).
// 2004-03-03	JTS, RTi		Added wrap() and related helper methods
//					to wrap lines of text at given lengths.
// 2004-03-09	JTS, RTi		Added stringsAreEqual() for easy 
//					testing of even null strings.
// 2004-05-10	JTS, RTi		Added isLong().
// 2004-05-24	Scott Townsend, RTi	Added isASCII(). This is needed to
//					parse data coming from NWSRFS binary
//					database files.
// 2004-07-20	SAM, RTi		* Deprecate matchesRegExp() in favor of
//					  matchesIgnoreCase().
// 2005-08-18	SAM, RTi		Fix bug in removeDuplicates() where the
//					index was not being decremented on a
//					delete, resulting in not all duplicates
//					being removed.
// 2006-04-10	SAM, RTi		Overload addToStringList() to accept an
//					array of String.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------
// EndHeader

namespace RTi.Util.String
{
	using  = char?;
	using  = double?;
	using  = float?;
	using  = int?;
	using Math;


	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// This class provides static utility routines for manipulating strings.
	/// </summary>
	public sealed class StringUtil
	{

	// Global data...

	/// <summary>
	/// Indicates that strings should be sorted in ascending order.
	/// </summary>
	public const int SORT_ASCENDING = 1;

	/// <summary>
	/// Indicates that strings should be sorted in descending order.
	/// </summary>
	public const int SORT_DESCENDING = 2;

	/// <summary>
	/// Token types for parsing routines.
	/// </summary>
	public const int TYPE_CHARACTER = 1;
	public const int TYPE_DOUBLE = 2;
	public const int TYPE_FLOAT = 3;
	public const int TYPE_INTEGER = 4;
	public const int TYPE_STRING = 5;
	public const int TYPE_SPACE = 6;

	/// <summary>
	/// For use with breakStringList.  Skip blank fields (adjoining delimiters are merged).
	/// </summary>
	public const int DELIM_SKIP_BLANKS = 0x1;
	/// <summary>
	/// For use with breakStringList.  Allow tokens that are surrounded by quotes.  For example, this is
	/// used when a data field might contain the delimiting character.
	/// </summary>
	public const int DELIM_ALLOW_STRINGS = 0x2;
	/// <summary>
	/// For use with breakStringList.  When DELIM_ALLOW_STRINGS is set, include the quotes in the returned string.
	/// </summary>
	public const int DELIM_ALLOW_STRINGS_RETAIN_QUOTES = 0x4;

	/// <summary>
	/// For use with padding routines.  Pad/unpad back of string.
	/// </summary>
	public const int PAD_BACK = 0x1;
	/// <summary>
	/// For use with padding routines.  Pad/unpad front of string.
	/// </summary>
	public const int PAD_FRONT = 0x2;
	/// <summary>
	/// For use with padding routines.  Pad/unpad middle of string.  This is private
	/// because for middle unpadding we currently only allow the full PAD_FRONT_MIDDLE_BACK option.
	/// </summary>
	private const int PAD_MIDDLE = 0x4;
	/// <summary>
	/// For use with padding routines.  Pad/unpad front and back of string.
	/// </summary>
	public const int PAD_FRONT_BACK = PAD_FRONT | PAD_BACK;
	/// <summary>
	/// For use with padding routines.  Pad/unpad front, back, and middle of string.
	/// </summary>
	public const int PAD_FRONT_MIDDLE_BACK = PAD_FRONT | PAD_MIDDLE | PAD_BACK;

	/// <summary>
	/// Add a list of Strings to another list of Strings.  If the first list is
	/// null, the second list will be returned.  If the second list is null, the
	/// first list will be returned.  If both are null, null will be returned. </summary>
	/// <returns> Combined list. </returns>
	/// <param name="v"> list of Strings - will be modified if not null when passed in. </param>
	/// <param name="newv"> list of Strings to add. </param>
	public static IList<string> addListToStringList(IList<string> v, IList<string> newv)
	{
		if (newv == null)
		{
			return v;
		}
		IList<string> vmain = null;
		if (v == null)
		{
			// Create a list...
			vmain = new List<string>(50);
		}
		else
		{
			// Modify the old list
			vmain = v;
		}
		int length = newv.Count;
		for (int i = 0; i < length; i++)
		{
			vmain.Add(newv[i]);
		}
		return vmain;
	}

	/// <summary>
	/// Add a String to a list of String.  If the list is null, a new list
	/// will be returned, containing the string.  The String will always be added
	/// to the list, even if the String is null. </summary>
	/// <returns> list after String is added. </returns>
	/// <param name="v"> list of Strings. </param>
	/// <param name="string"> String to add to the list. </param>
	public static IList<string> addToStringList(IList<string> v, string @string)
	{
		IList<string> vmain = null;
		if (v == null)
		{
			// Create a list...
			vmain = new List<string>();
		}
		else
		{
			vmain = v;
		}
		vmain.Add(@string);
		return vmain;
	}

	/// <summary>
	/// Add an array of String to a list of String.  If the list is null, a new
	/// list will be returned, containing the strings.  The Strings will always be
	/// added to the list, even if they are null. </summary>
	/// <returns> list after String is added. </returns>
	/// <param name="v"> list of Strings. </param>
	/// <param name="strings"> Array of String to add to list. </param>
	public static IList<string> addToStringList(IList<string> v, string[] strings)
	{
		IList<string> vmain = null;
		if (v == null)
		{
			// Create a list...
			vmain = new List<string>(50);
		}
		else
		{
			vmain = v;
		}
		if (strings == null)
		{
			return vmain;
		}
		for (int i = 0; i < strings.Length; i++)
		{
			vmain.Add(strings[i]);
		}
		return vmain;
	}

	/// <summary>
	/// Convert a String to an int, similar to C language atoi() function. </summary>
	/// <param name="s"> String to convert. </param>
	/// <returns> An int as converted from the String or 0 if conversion fails. </returns>
	public static int atoi(string s)
	{
		if (string.ReferenceEquals(s, null))
		{
			return 0;
		}
		int value = 0;
		try
		{
			value = int.Parse(s.Trim());
		}
		catch (System.FormatException)
		{
			Message.printWarning(50, "StringUtil.atoi", "Unable to convert \"" + s + "\" to int.");
			value = 0;
		}
		return value;
	}

	/// <summary>
	/// Convert a String to a float, similar to C language atof() function. </summary>
	/// <param name="s"> String to convert. </param>
	/// <returns> A float as converted from the String, or 0.0 if there is a conversion error. </returns>
	public static float atof(string s)
	{
		if (string.ReferenceEquals(s, null))
		{
			return (float)0.0;
		}
		float value = (float)0.0;
		try
		{
			value = (Convert.ToSingle(s.Trim()));
		}
		catch (System.FormatException)
		{
			Message.printWarning(50, "StringUtil.atof", "Unable to convert \"" + s + "\" to float.");
			value = (float)0.0;
		}
		return value;
	}

	/// <summary>
	/// Convert a String to a double. </summary>
	/// <param name="s"> String to convert. </param>
	/// <returns> A double as converted from the String, or 0.0 if a conversion error. </returns>
	public static double atod(string s)
	{
		if (string.ReferenceEquals(s, null))
		{
			return 0.0;
		}
		double value = 0.0;
		try
		{
			value = (Convert.ToDouble(s.Trim()));
		}
		catch (System.FormatException)
		{
			Message.printWarning(50, "StringUtil.atod", "Unable to convert \"" + s + "\" to double.");
			value = 0.0;
		}
		return value;
	}

	/// <summary>
	/// Convert a String to a long. </summary>
	/// <param name="s"> String to convert. </param>
	/// <returns> A long as converted from the String, or 0 if an error. </returns>
	public static long atol(string s)
	{
		if (string.ReferenceEquals(s, null))
		{
			return 0;
		}
		long value = 0;
		try
		{
			value = (Convert.ToInt64(s.Trim()));
		}
		catch (System.FormatException)
		{
			Message.printWarning(50, "StringUtil.atol", "Unable to convert \"" + s + "\" to long.");
			value = 0;
		}
		return value;
	}

	/*------------------------------------------------------------------------------
	** HMBreakStringList - get a list of strings from a string
	**------------------------------------------------------------------------------
	** Copyright:	See the COPYRIGHT file.
	**------------------------------------------------------------------------------
	** Notes:	(1)	The list is assumed to be of the form "val,val,val",
	**			where the commas indicate the delimiter character.
	**		(2)	Call "HMFreeStringList" when done with the list.
	**		(3)	The list always has one NULL element at the end so that
	**			we know how to free the memory.  However, "nlist" does
	**			not include this element.
	**		(4)	If the HMDELIM_ALLOW_STRINGS flag is set, then we
	**			strings to be treated as one token, even if they contain
	**			blanks.  The first quote character, either " or ' is
	**			used to contain the string.  The quote characters
	**			cannot be in the list of delimiting characters.
	**		(5)	It would be nice to allow return of all the tokens.
	**			Add the "flag" variable to allow for this enhancement
	**			in the future.
	**------------------------------------------------------------------------------
	** History:
	**
	** ?		Steven A. Malers, RTi	Created routine.
	** 06-08-95	SAM, RTi		Document all variables.
	** 08-21-95	SAM, RTi		Change so that delimiter list is a
	**					string so that more than one
	**					"whitespace" character can be used
	**					(e.g., spaces and tabs).  Also allow
	**					more than one whitespace character in
	**					sequence (skip them all).  Also add
	**					check to make sure that substring is not
	**					overrun.
	** 04 Oct 1995	SAM, RTi		Use HMAddToStringList to do bulk of
	**					work.
	** 02 Sep 1996	SAM, RTi		Break this routine out of HMUtil.c.  Do
	**					minor cleanup to make more stand-alone.
	** 07 Oct 1996	SAM, RTi		Add <string.h> to prototype functions.
	** 21 Jan 1997	SAM, RTi		Add flag to allow quoted strings to be
	**					separated out.
	** 17 Jun 1997	Matthew J. Rutherford, RTi
	**					Adjust string stuff so that a quote
	**					in the middle of a string is found.
	**------------------------------------------------------------------------------
	** Variable	I/O	Description
	**
	** delim	I	Character delimiter list.
	** flag		I	Flag to modify parsing.
	** i		L	Counter for characters in substring.
	** instring	L	Indicates if we are inside a quoted string.
	** list		L	List of broken out strings.
	** nlist	O	Number of strings in the final list.
	** nlist2	L	Used when adding strings to list.
	** pt		L	Pointer to original string.
	** pt2		L	Pointer to split out string.
	** quote	L	Character used for a quoted string.
	** routine	L	Name of this routine.
	** string	I	String of delimiter-separated items.
	** tempstr	L	String used when splitting out sub-strings.
	**------------------------------------------------------------------------------
	*/
	//TODO SAM 2010-09-21 Evaluate phasing out this method in favor of built-in parsing
	// features in Java (which are now more mature then when breakStringList() was originally written).
	/// <summary>
	/// Break a delimited string into a list of Strings.  The end of the string is
	/// considered as a delimiter so "xxxx,xxxx" returns two strings if the comma is a
	/// delimiter and "xxxxx" returns one string if the comma is the delimiter.  If a
	/// delimiter character is actually the last character, no empty/null field is returned at
	/// the end.  If multiple delimiters are at the front and skip blanks is specified,
	/// all the delimiters will be skipped.  Escaped single characters are passed through
	/// as is.  Therefore \" (two characters) will be two characters in the output.  Other
	/// code needs to interpret the two characters as the actual special character. </summary>
	/// <returns> A list of Strings, guaranteed to be non-null </returns>
	/// <param name="string"> The string to break. </param>
	/// <param name="delim"> A String containing characters to treat as delimiters.  Each
	/// character in the string is checked (the complete string is not used as a
	/// multi-character delimiter).  Cannot be null. </param>
	/// <param name="flag"> Bitmask indicating how to break the string.  Specify
	/// DELIM_SKIP_BLANKS to skip blank fields (delimiters that are next to each other
	/// are treated as one delimiter - delimiters at the front are ignored).  Specify
	/// DELIM_ALLOW_STRINGS to allow quoted strings (which may contain delimiters).
	/// Specify DELIM_ALLOW_STRINGS_RETAIN_QUOTES to retain the quotes in the return strings when
	/// DELIM_ALLOW_QUOTES is used.
	/// Specify 0 (zero) to do simple tokenizing where repeated delimiters are not
	/// merged and quoted strings are not handled as one token.  Note that when allowing
	/// quoted strings the string "xxxx"yy is returned as xxxxyy because no intervening delimiter is present. </param>
	public static IList<string> breakStringList(string @string, string delim, int flag)
	{
		string routine = "StringUtil.breakStringList";
		IList<string> list = new List<string>();

		if (string.ReferenceEquals(@string, null))
		{
			 return list;
		}
		if (@string.Length == 0)
		{
			 return list;
		}
		//if ( Message.isDebugOn ) {
		//	Message.printDebug ( 50, routine,
		//	Message.printStatus ( 1, routine,
		//	"SAMX Breaking \"" + string + "\" using \"" + delim + "\"" );
		//}
		int length_string = @string.Length;
		bool instring = false;
		bool retainQuotes = false;
		int istring = 0;
		char cstring;
		char quote = '\"';
		StringBuilder tempstr = new StringBuilder();
		bool allow_strings = false, skip_blanks = false;
		if ((flag & DELIM_ALLOW_STRINGS) != 0)
		{
			allow_strings = true;
		}
		if ((flag & DELIM_SKIP_BLANKS) != 0)
		{
			skip_blanks = true;
		}
		if (allow_strings && ((flag & DELIM_ALLOW_STRINGS_RETAIN_QUOTES) != 0))
		{
			retainQuotes = true;
		}
		// Loop through the characters in the string.  If in the main loop or
		// the inner "while" the end of the string is reached, the last
		// characters will be added to the last string that is broken out...
		bool at_start = true; // If only delimiters are at the front this will be true.
		for (istring = 0; istring < length_string;)
		{
			cstring = @string[istring];
			// Start the next string in the list.  Move characters to the
			// temp string until a delimiter is found.  If inside a string
			// then go until a closing delimiter is found.
			instring = false;
			tempstr.Length = 0; // Clear memory.
			while (istring < length_string)
			{
				// Process a sub-string between delimiters...
				cstring = @string[istring];
				// Check for escaped special characters...
				if ((cstring == '\\') && (istring < (length_string - 1)) && (@string[istring + 1] == '\"'))
				{
					// Add the backslash and the next character - currently only handle single characters
					tempstr.Append(cstring);
					// Now increment to the next character...
					++istring;
					cstring = @string[istring];
					tempstr.Append(cstring);
					++istring;
					continue;
				}
				//Message.printStatus ( 2, routine, "SAMX Processing character " + cstring );
				if (allow_strings)
				{
					// Allowing quoted strings so do check for the start and end of quotes...
					if (!instring && ((cstring == '"') || (cstring == '\'')))
					{
						// The start of a quoted string...
						instring = true;
						at_start = false;
						quote = cstring;
						if (retainQuotes)
						{
							tempstr.Append(cstring);
						}
						// Skip over the quote since we don't want to /store or process again...
						++istring;
						// cstring set at top of while...
						//Message.printStatus ( 1, routine, "SAMX start of quoted string " + cstring );
						continue;
					}
					// Check for the end of the quote...
					else if (instring && (cstring == quote))
					{
						// In a quoted string and have found the closing quote.  Need to skip over it.
						// However, could still be in the string and be escaped, so check for that
						// by looking for another string. Any internal escaped quotes will be a pair "" or ''
						// so look ahead one and if a pair, treat as characters to be retained.
						// This is usually only going to be encountered when reading CSV files, etc.
						if ((istring < (length_string - 1)) && (@string[istring + 1] == quote))
						{
							// Found a pair of the quote character so absorb both and keep looking for ending quote for the token
							tempstr.Append(cstring); // First quote retained because it is literal
							//Message.printStatus(2,routine,"found ending quote candidate at istring=" + istring + " adding as first in double quote");
							++istring;
							if (retainQuotes)
							{
								// Want to retain all the quotes
								tempstr.Append(cstring); // Second quote
								//Message.printStatus(2,routine,"Retaining 2nd quote of double quote at istring=" + istring );
							}
							++istring;
							// instring still true
							continue;
						}
						// Else... process as if not an escaped string but an end of quoted string
						if (retainQuotes)
						{
							tempstr.Append(cstring);
						}
						instring = false;
						//Message.printStatus ( 1, routine, "SAMX end of quoted string" + cstring );
						++istring;
						if (istring < length_string)
						{
							cstring = @string[istring];
							// If the current string is now another quote, just continue so it can be processed
							// again as the start of another string (but don't by default add the quote character)...
							if ((cstring == '\'') || (cstring == '"'))
							{
								if (retainQuotes)
								{
									tempstr.Append(cstring);
								}
								continue;
							}
						}
						else
						{
							// The quote was the last character in the original string.  Break out so the
							// last string can be added...
							break;
						}
						// If here, the closing quote has been skipped but don't want to break here
						// in case the final quote isn't the last character in the sub-string
						// (e.g, might be ""xxx).
					}
				}
				// Now check for a delimiter to break the string...
				if (delim.IndexOf(cstring) != -1)
				{
					// Have a delimiter character that could be in a string or not...
					if (!instring)
					{
						// Not in a string so OK to break...
						//Message.printStatus ( 1, routine, "SAMX have delimiter outside string" + cstring );
						break;
					}
				}
				else
				{
					// Else, treat as a character that needs to be part of the token and add below...
					at_start = false;
				}
				// It is OK to add the character...
				tempstr.Append(cstring);
				// Now increment to the next character...
				++istring;
				// Go to the top of the "while" and evaluate the current character that was just set.
				// cstring is set at top of while...
			}
			// Now have a sub-string and the last character read is a
			// delimiter character (or at the end of the original string).
			//
			// See if we are at the end of the string...
			if (instring)
			{
				if (Message.isDebugOn)
				{
					Message.printWarning(10, routine, "Quoted string \"" + tempstr + "\" is not closed");
				}
				// No further action is required...
			}
			// Check for and skip any additional delimiters that may be present in a sequence...
			else if (skip_blanks)
			{
				while ((istring < length_string) && (delim.IndexOf(cstring) != -1))
				{
					//Message.printStatus ( 1, routine, "SAMX skipping delimiter" + cstring );
					++istring;
					if (istring < length_string)
					{
						cstring = @string[istring];
					}
				}
				if (at_start)
				{
					// Just want to skip the initial delimiters without adding a string to the returned list...
					at_start = false;
					continue;
				}
				// After this the current character will be that which needs to be evaluated.  "cstring" is reset
				// at the top of the main "for" loop but it needs to be assigned here also because of the check
				// in the above while loop
			}
			else
			{
				// Not skipping multiple delimiters so advance over the character that triggered the break in
				// the main while loop...
				++istring;
				// cstring will be assigned in the main "for" loop
			}
			// Now add the string token to the list...
			list.Add(tempstr.ToString());
			//if ( Message.isDebugOn ) {
				//Message.printDebug ( 50, routine,
				//Message.printStatus ( 1, routine,
				//"SAMX Broke out list[" + (list.size() - 1) + "]=\"" + tempstr + "\"" );
			//}
		}
		return list;
	}

	/// <summary>
	/// Returns The hexadecimal String representation of a byte.  For example, passing
	/// in a byte with value 63 would result in a String "3f".  Note that there is no
	/// leading "0x" in the returned String.  Note also that values less than 0 or 
	/// greater than 255 may cause bad results, and are not supported by this method. </summary>
	/// <param name="b"> Byte to convert to a hexadecimal String. </param>
	/// <returns> String that represents the Hexadecimal value of the byte. </returns>
	public static string byteToHex(sbyte b)
	{
		char[] hexDigit = new char[] {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f'};
		char[] array = new char[] {hexDigit[(b >> 4) & 0x0f], hexDigit[b & 0x0f]};

		return new string(array);
	}

	/// <summary>
	/// Center a string by padding with spaces. </summary>
	/// <returns> The centered string. </returns>
	/// <param name="orig"> The original string. </param>
	/// <param name="width"> Width of the centered string. </param>
	public static string centerString(string orig, int width)
	{
		if (orig.Length >= width)
		{
			return orig;
		}
		int border = (width - orig.Length) / 2;
		StringBuilder centered = new StringBuilder(orig);
		for (int i = 0; i < border; i++)
		{
			centered.Insert(0,' ');
		}
		return centered.ToString();
	}

	/// <summary>
	/// Returns The hexadecimal String representation of char c.  For example, passing
	/// in a char with a value 'c' would result in a String "0063".  Note that there is
	/// no leading "0x" in the returned String.  Because the Java char type is two bytes
	/// in order to store Unicode characters, the returned string is four characters
	/// (two per byte). </summary>
	/// <param name="c"> char to convert to hexadecimal String. </param>
	/// <returns> String that represents the Hexadecimal value of the char. </returns>
	public static string charToHex(char c)
	{
		sbyte hi = (sbyte)((int)((uint)c >> 8));
		sbyte lo = unchecked((sbyte)(c & 0xff));
		return byteToHex(hi) + byteToHex(lo);
	}

	/// <summary>
	/// Indicate whether the string contains any of the specified characters.  This
	/// can be used to check for restricted characters in input. </summary>
	/// <param name="s"> String to check. </param>
	/// <param name="chars"> Characters to check for in string. </param>
	/// <param name="ignore_case"> Specify to true if case should be ignored. </param>
	/// <returns> true if the checked string contains any of the specified characters. </returns>
	public static bool containsAny(string s, string chars, bool ignore_case)
	{
		if ((string.ReferenceEquals(s, null)) || (string.ReferenceEquals(chars, null)))
		{
			return false;
		}
		// Convert the case here once rather than in indexOfIgnoreCase()
		if (ignore_case)
		{
			s = s.ToUpper();
			chars = chars.ToUpper();
		}
		int size = chars.Length;
		for (int i = 0; i < size; i++)
		{
			if (s.IndexOf(chars[i]) >= 0)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Convert a string containing a number sequence like "1,4-5,13" to zero offset like "0,3-4,12".
	/// This method is used to convert user-based parameters that have values 1+ with internal code values 0+, which
	/// is useful when high-level (i.e., user-specified) parameters need to be converted to the zero offset form
	/// used by low-level code. </summary>
	/// <param name="sequenceString"> a string of positions like "1,4-5,13", where each index is 1+. </param>
	/// <returns> the string of positions like "0,3-4,12", where each index is 0+. </returns>
	public static string convertNumberSequenceToZeroOffset(string sequenceString)
	{
		if ((string.ReferenceEquals(sequenceString, null)) || (sequenceString.Length == 0))
		{
			return sequenceString;
		}
		StringBuilder b = new StringBuilder();
		IList<string> v = StringUtil.breakStringList(sequenceString, ", ", StringUtil.DELIM_SKIP_BLANKS);
		int vsize = 0;
		if (v != null)
		{
			vsize = v.Count;
		}
		for (int i = 0; i < vsize; i++)
		{
			string vi = v[i];
			if (i != 0)
			{
				b.Append(",");
			}
			if (StringUtil.isInteger(vi))
			{
				int index = int.Parse(vi);
				b.Append("" + (index - 1));
			}
			else
			{
				int pos = vi.IndexOf("-", StringComparison.Ordinal);
				if (pos >= 0)
				{
					// Specifying a range of values...
					int first_in_range = -1;
					int last_in_range = -1;
					if (pos == 0)
					{
						// First index is 1 (will be decremented below)...
						first_in_range = 1;
					}
					else
					{
						// Get first to skip...
						first_in_range = int.Parse(vi.Substring(0,pos).Trim());
					}
					last_in_range = int.Parse(vi.Substring(pos + 1).Trim());
					b.Append("" + (first_in_range - 1) + "-" + (last_in_range - 1));
				}
			}
		}
		return b.ToString();
	}

	/// <summary>
	/// Determine whether one strings ends with the specified substring, ignoring case. </summary>
	/// <param name="s"> String to evaluate. </param>
	/// <param name="pattern"> End-string to compare. </param>
	/// <returns> true if the String "s" ends with "pattern", ignoring case.  If the
	/// pattern string is null or empty, false is returned. </returns>
	public static bool endsWithIgnoreCase(string s, string pattern)
	{
		if ((string.ReferenceEquals(s, null)) || (string.ReferenceEquals(pattern, null)))
		{
			return false;
		}
		int plen = pattern.Length;
		int slen = s.Length;
		if ((plen == 0) || (slen < plen))
		{
			return false;
		}
		string sub = s.Substring(slen - plen);
		return sub.regionMatches(true,0,pattern,0,plen);
	}

	/// <summary>
	/// Parse a fixed-format string (e.g., a FORTRAN data file) using a simplified
	/// notation.  <b>This routine needs to be updated to accept C-style formatting
	/// commands.  Requesting more fields than there are data results in default (zero
	/// or blank) data being returned.</b>
	/// This method can be used to read integers and floating point numbers from a
	/// string containing fixed-format information. </summary>
	/// <returns> A List of objects that are read from the string according to the
	/// specified format described below.  Integers are returned as Integers, doubles
	/// as Doubles, etc.  Blank "x" fields are not returned (therefore the list of returned
	/// objects has a size of all non-x formats). </returns>
	/// <param name="string"> String to parse. </param>
	/// <param name="format"> Format to use for parsing, as shown in the following table.
	/// An example is: "i5f10x3a10", or in general
	/// "v#", where "v" indicates a variable type and "#" indicates the TOTAL width
	/// of the variable field in the string.
	/// NO WHITESPACE OR DELIMITERS IN THE FORMAT!
	/// <para>
	/// 
	/// <table width=100% cellpadding=10 cellspacing=0 border=2>
	/// <tr>
	/// <td><b>Data Type</b></td>	<td><b>Format</b></td>	<td><b>Example</b></td>
	/// </tr
	/// 
	/// <tr>
	/// <td><b>integer, Integer</b></td>
	/// <td>i</td>
	/// <td>i5</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>float, Float</b></td>
	/// <td>f</td>
	/// <td>f10</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>double, Double</b></td>
	/// <td>d</td>
	/// <td>d10</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>Spaces (not returned)</b></td>
	/// <td>x</td>
	/// <td>x20</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>char</b></td>
	/// <td>c</td>
	/// <td>c</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>String</b></td>
	/// <td>s, a</td>
	/// <td>s10, a10</td>
	/// </tr>
	/// </table> </param>
	public static IList<object> fixedRead(string @string, string format)
	{ // Determine the format types and widths...
		// THIS CODE INLINED FROM THE METHOD BELOW.  MODIFY THE OTHER METHOD AND THEN MAKE THIS CODE AGREE....

		// First loop through the format string and count the number of valid format specifier characters...
		int format_length = 0;
		if (!string.ReferenceEquals(format, null))
		{
			format_length = format.Length;
		}
		int field_count = 0;
		char cformat;
		for (int i = 0; i < format_length; i++)
		{
			cformat = format[i];
			if ((cformat == 'a') || (cformat == 'A') || (cformat == 'c') || (cformat == 'C') || (cformat == 'd') || (cformat == 'D') || (cformat == 'f') || (cformat == 'F') || (cformat == 'i') || (cformat == 'I') || (cformat == 's') || (cformat == 'S') || (cformat == 'x') || (cformat == 'X'))
			{
				++field_count;
			}
		}
		// Now set the array sizes for formats...
		int[] field_types = new int[field_count];
		int[] field_widths = new int[field_count];
		field_count = 0; // Reset for detailed loop...
		StringBuilder width_string = new StringBuilder();
		for (int iformat = 0; iformat < format_length; iformat++)
		{
			width_string.Length = 0;
			// Get a format character...
			cformat = format[iformat];
			//System.out.println ( "Format character is: " + cformat );
			if ((cformat == 'c') || (cformat == 'C'))
			{
				field_types[field_count] = TYPE_CHARACTER;
				field_widths[field_count] = 1;
				continue;
			}
			else if ((cformat == 'd') || (cformat == 'D'))
			{
				field_types[field_count] = TYPE_DOUBLE;
			}
			else if ((cformat == 'f') || (cformat == 'F'))
			{
				field_types[field_count] = TYPE_FLOAT;
			}
			else if ((cformat == 'i') || (cformat == 'I'))
			{
				field_types[field_count] = TYPE_INTEGER;
			}
			else if ((cformat == 'a') || (cformat == 'A') || (cformat == 's') || (cformat == 'S'))
			{
				field_types[field_count] = TYPE_STRING;
			}
			else if ((cformat == 'x') || (cformat == 'X'))
			{
				field_types[field_count] = TYPE_SPACE;
			}
			else
			{
				// Problem!!!
				continue;
			}
			// Determine the field width...
			++iformat;
			while (iformat < format_length)
			{
				cformat = format[iformat];
				if (!char.IsDigit(cformat))
				{
					// Went into the next field...
					--iformat;
					break;
				}
				width_string.Append(cformat);
				++iformat;
			}
			field_widths[field_count] = atoi(width_string.ToString());
			++field_count;
		}
		width_string = null;
		// ...END OF INLINED CODE
		// Now do the read...	
		IList<object> v = fixedRead(@string, field_types, field_widths, null);
		return v;
	}

	/// <summary>
	/// Parse a fixed string. </summary>
	/// <returns> A list of objects that are read from the string according to the
	/// specified format.  Integers are returned as Integers, doubles as Doubles, etc.
	/// Blank TYPE_SPACE fields are not returned. </returns>
	/// <param name="string"> String to parse. </param>
	/// <param name="format"> Format of string (see overloaded method for explanation). </param>
	/// <param name="results"> If specified and not null, the list will be used to save the
	/// results.  This allows a single list to be reused in repetitive reads.
	/// The list is cleared before reading. </param>
	public static IList<object> fixedRead(string @string, string format, IList<object> results)
	{ // First loop through the format string and count the number of valid format specifier characters...
		int format_length = 0;
		if (!string.ReferenceEquals(format, null))
		{
			format_length = format.Length;
		}
		int field_count = 0;
		char cformat;
		for (int i = 0; i < format_length; i++)
		{
			cformat = @string[i];
			if ((cformat == 'a') || (cformat == 'A') || (cformat == 'c') || (cformat == 'C') || (cformat == 'd') || (cformat == 'D') || (cformat == 'f') || (cformat == 'F') || (cformat == 'i') || (cformat == 'I') || (cformat == 's') || (cformat == 'S') || (cformat == 'x') || (cformat == 'X'))
			{
				++field_count;
			}
		}
		// Now set the array sizes for formats...
		int[] field_types = new int[field_count];
		int[] field_widths = new int[field_count];
		field_count = 0; // Reset for detailed loop...
		StringBuilder width_string = new StringBuilder();
		for (int iformat = 0; iformat < format_length; iformat++)
		{
			width_string.Length = 0;
			// Get a format character...
			cformat = format[iformat];
			//System.out.println ( "Format character is: " + cformat );
			if ((cformat == 'c') || (cformat == 'C'))
			{
				field_types[field_count] = TYPE_CHARACTER;
				field_widths[field_count] = 1;
				continue;
			}
			else if ((cformat == 'd') || (cformat == 'D'))
			{
				field_types[field_count] = TYPE_DOUBLE;
			}
			else if ((cformat == 'f') || (cformat == 'F'))
			{
				field_types[field_count] = TYPE_FLOAT;
			}
			else if ((cformat == 'i') || (cformat == 'I'))
			{
				field_types[field_count] = TYPE_INTEGER;
			}
			else if ((cformat == 'a') || (cformat == 'A') || (cformat == 's') || (cformat == 'S'))
			{
				field_types[field_count] = TYPE_STRING;
			}
			else if ((cformat == 'x') || (cformat == 'X'))
			{
				field_types[field_count] = TYPE_SPACE;
			}
			else
			{
				// Problem!!!
				continue;
			}
			// Determine the field width...
			++iformat;
			while (iformat < format_length)
			{
				cformat = format[iformat];
				if (!char.IsDigit(cformat))
				{
					// Went into the next field...
					--iformat;
					break;
				}
				width_string.Append(cformat);
				++iformat;
			}
			field_widths[field_count] = atoi(width_string.ToString());
			++field_count;
		}
		width_string = null;
		IList<object> v = fixedRead(@string, field_types, field_widths, results);
		return v;
	}

	/// <summary>
	/// Parse a fixed-format string (e.g., a FORTRAN data file).
	/// Requesting more fields than there are data results in default (zero
	/// or blank) data being returned.</b>
	/// This method can be used to read integers and floating point numbers from a
	/// string containing fixed-format information. </summary>
	/// <returns> A List of objects that are read from the string according to the
	/// specified format.  Integers are returned as Integers, doubles as Doubles, etc.
	/// Blank TYPE_SPACE fields are not returned. </returns>
	/// <param name="string"> String to parse. </param>
	/// <param name="field_types"> Field types to use for parsing </param>
	/// <param name="field_widths"> Array of fields widths. </param>
	/// <param name="results"> If specified and not null, the list will be used to save the
	/// results.  This allows a single list to be reused in repetitive reads.
	/// The list is cleared before reading. </param>
	public static IList<object> fixedRead(string @string, int[] field_types, int[] field_widths, IList<object> results)
	{
		int dtype = 0, isize, j, nread = 0; // Number of values read from file.
		bool eflag = false; // Indicates that the end of the line has been
					// reached before all of the format has been
					// evaluated.

		int size = field_types.Length;
		int string_length = @string.Length;
		IList<object> tokens = null;
		if (results != null)
		{
			tokens = results;
			tokens.Clear();
		}
		else
		{
			tokens = new List<object>(size);
		}

		StringBuilder var = new StringBuilder();
		int istring = 0; // Position in string to parse.
		for (int i = 0; i < size; i++)
		{
			dtype = field_types[i];
			// Read the variable...
			var.Length = 0;
			if (eflag)
			{
				// End of the line has been reached before the processing has finished...
			}
			else
			{
				//System.out.println ( "Variable size=" + size);
				isize = field_widths[i];
				for (j = 0; j < isize; j++, istring++)
				{
					if (istring >= string_length)
					{
						// End of the string.  Process the rest of the variables so that they are
						// given a value of zero...
						eflag = true;
						break;
					}
					else
					{
						var.Append(@string[istring]);
					}
				}
			}
			// 1. Convert the variable that was read as a character
			//    string to the proper representation.  Apparently
			//    most atomic objects can be instantiated from a
			//    String but not a StringBuffer.
			// 2. Add to the list.
			//Message.printStatus ( 2, "", "String to convert to object is \"" + var + "\"" );
			if (dtype == StringUtil.TYPE_CHARACTER)
			{
				tokens.Add(new char?(var[0]));
			}
			else if (dtype == StringUtil.TYPE_DOUBLE)
			{
				string sdouble = var.ToString().Trim();
				if (sdouble.Length == 0)
				{
					tokens.Add(Convert.ToDouble("0.0"));
				}
				else
				{
					tokens.Add(Convert.ToDouble(sdouble));
				}
			}
			else if (dtype == StringUtil.TYPE_FLOAT)
			{
				string sfloat = var.ToString().Trim();
				if (sfloat.Length == 0)
				{
					tokens.Add(Convert.ToSingle("0.0"));
				}
				else
				{
					tokens.Add(Convert.ToSingle(sfloat));
				}
			}
			else if (dtype == StringUtil.TYPE_INTEGER)
			{
				string sinteger = var.ToString().Trim();
				if (sinteger.Length == 0)
				{
					tokens.Add(Convert.ToInt32("0"));
				}
				else
				{
					// check for "+"
					if (sinteger.StartsWith("+", StringComparison.Ordinal))
					{
						sinteger = sinteger.Substring(1);
					}
					tokens.Add(Convert.ToInt32(sinteger));
				}
			}
			else if (dtype == StringUtil.TYPE_STRING)
			{
				tokens.Add(var.ToString());
			}
			++nread;
		}
		return tokens;
	}

	// TODO SAM 2014-03-30 Refactor the fixedRead methods to call the following method
	/// <summary>
	/// Parse the format string for fixedRead into lists that can be used for other fixedRead commands.
	/// The field types and widths WILL INCLUDE "x" formats because fixedRead() needs that information. </summary>
	/// <param name="format"> the fixedRead method format (e.g., "d10f8i3x3s15") </param>
	/// <param name="fieldTypes"> a non-null List<Integer> that will be set to the field types for each format part. </param>
	/// <param name="fieldWidths"> a non-null List<Integer> that will be set to the field widths for each format part. </param>
	public static void fixedReadParseFormat(string format, IList<int> fieldTypes, IList<int> fieldWidths)
	{
		// Now set the array sizes for formats...
		StringBuilder width_string = new StringBuilder();
		char cformat;
		for (int iformat = 0; iformat < format.Length; iformat++)
		{
			width_string.Length = 0;
			// Get a format character...
			cformat = format[iformat];
			//System.out.println ( "Format character is: " + cformat );
			if ((cformat == 'c') || (cformat == 'C'))
			{
				fieldTypes.Add(TYPE_CHARACTER);
				fieldWidths.Add(1);
				continue;
			}
			else if ((cformat == 'd') || (cformat == 'D'))
			{
				fieldTypes.Add(TYPE_DOUBLE);
			}
			else if ((cformat == 'f') || (cformat == 'F'))
			{
				fieldTypes.Add(TYPE_FLOAT);
			}
			else if ((cformat == 'i') || (cformat == 'I'))
			{
				fieldTypes.Add(TYPE_INTEGER);
			}
			else if ((cformat == 'a') || (cformat == 'A') || (cformat == 's') || (cformat == 'S'))
			{
				fieldTypes.Add(TYPE_STRING);
			}
			else if ((cformat == 'x') || (cformat == 'X'))
			{
				fieldTypes.Add(TYPE_SPACE);
			}
			else
			{
				// Problem!!!
				continue;
			}
			// Determine the field width...
			++iformat;
			while (iformat < format.Length)
			{
				cformat = format[iformat];
				if (!char.IsDigit(cformat))
				{
					// Went into the next field...
					--iformat;
					break;
				}
				width_string.Append(cformat);
				++iformat;
			}
			fieldWidths.Add(atoi(width_string.ToString()));
		}
	}

	/// <summary>
	/// Format two arrays of number parallel to each other.
	/// This is a convenience method useful for logging. </summary>
	/// <param name="label1"> label for the first array </param>
	public static string formatArrays(string label1, double[] array1, string label2, double[] array2, string delim, string lineDelim)
	{
		StringBuilder b = new StringBuilder();
		int len = array1.Length;
		if (array2.Length > array1.Length)
		{
			len = array2.Length;
		}
		if ((!string.ReferenceEquals(label1, null)) && !label1.Equals(""))
		{
			b.Append(label1);
		}
		if (!string.ReferenceEquals(delim, null))
		{
			b.Append(delim);
		}
		if ((!string.ReferenceEquals(label2, null)) && !label2.Equals(""))
		{
			b.Append(label2);
		}
		if ((b.Length > 0) && (!string.ReferenceEquals(lineDelim, null)))
		{
			b.Append(lineDelim);
		}
		int lineCount = 0;
		int im1;
		for (int i = 0; i < len; i++)
		{
			im1 = i - 1;
			if (lineCount > 0)
			{
				if (!string.ReferenceEquals(lineDelim, null))
				{
					b.Append(lineDelim);
				}
			}
			if (im1 <= array1.Length)
			{
				b.Append("" + array1[i]);
			}
			if (!string.ReferenceEquals(delim, null))
			{
				b.Append(delim);
			}
			if (im1 <= array2.Length)
			{
				b.Append("" + array2[i]);
			}
			++lineCount;
		}
		return b.ToString();
	}

	/// <summary>
	/// Format an array as a sequence of numbers separated by a delimiter.  A blank string is returned if
	/// the array is null or empty. </summary>
	/// <returns> a string containing the formatted sequence of integers. </returns>
	/// <param name="array"> array of numbers to format </param>
	/// <param name="delim"> delimiter to user between numbers (no extra spaces will be added) </param>
	public static string formatNumberSequence(int[] array, string delim)
	{
		StringBuilder b = new StringBuilder();
		if (array != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if ((i > 0) && (!string.ReferenceEquals(delim, null)))
				{
					b.Append(delim);
				}
				b.Append("" + array[i]);
			}
		}
		return b.ToString();
	}

	// Format a string like the C sprintf
	//
	// Notes:	(1)	We accept any of the formats:
	//
	//			%%		- literal percent
	//			%c		- single character
	//			%s %8.8s %-8s	- String
	//			%d %4d		- Integer
	//			%8.2f %#8.2f	- Float
	//			%8.2F %#8.2F	- Double
	//
	/// <summary>
	/// Format a string like the C sprintf function. </summary>
	/// <returns> The formatted string. </returns>
	/// <param name="v"> The list of objects to format.  Floating point numbers must be Double, etc. because
	/// the toString function is called for each object (actually, a number can be
	/// passed in as a String since toString will work in that case too). </param>
	/// <param name="format"> The format to use for formatting, containing normal characters
	/// and the following formatting strings:
	/// <para>
	/// <table width=100% cellpadding=10 cellspacing=0 border=2>
	/// <tr>
	/// <td><b>Data Type</b></td>	<td><b>Format</b></td>	<td><b>Example</b></td>
	/// </tr
	/// 
	/// <tr>
	/// <td><b>Literal %</b></td>
	/// <td>%%</td>
	/// <td>%%5</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>Single character</b></td>
	/// <td>%c</td>
	/// <td>%c</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>String</b></td>
	/// <td>%s</td>
	/// <td>%s, %-20.20s</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>Integer</b></td>
	/// <td>%d</td>
	/// <td>%4d, %04d</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>Float, Double</b></td>
	/// <td>%f, %F</td>
	/// <td>%8.2f, %#8.2f</td>
	/// </tr>
	/// 
	/// </table>
	/// </para>
	/// <para>
	/// 
	/// The format can be preceded by a - (e.g., %-8.2f, %-s) to left-justify the
	/// formatted string.  The default is to left-justify strings and right-justify
	/// numbers.  Numeric formats, if preceded by a 0 will result in the format being
	/// padded by zeros (e.g., %04d will pad an integer with zeros up to 4 digits).
	/// To force strings to be a certain width use a format like %20.20s.  To force
	/// floating point numbers to always use a decimal point use the #.
	/// Additional capabilities may be added later. </param>
	public static string formatString<T1>(IList<T1> v, string format) where T1 : object
	{
		StringBuilder buffer = new StringBuilder();
		int dl = 75;

		if (v == null)
		{
			return buffer.ToString();
		}
		if (string.ReferenceEquals(format, null))
		{
			return buffer.ToString();
		}

		// Now loop through the format and as format specifiers are encountered
		// put them in the formatted string...

		int diff;
		int i;
		int iend;
		int iformat;
		int iprecision;
		int iwidth;
		int j = 0;
		int length_format = format.Length;
		int length_temp;
		int offset = 0; // offset in string or array
		int precision = 0; // precision as integer
		int sign;
		int width = 0;
		int vindex = 0;
		char cformat;
		char cvalue;
		char[] sprecision = new char[20]; // should be enough
		char[] swidth = new char[20];
		bool dot_found, first, left_shift, pound_format, zero_format;
		int vsizem1 = v.Count - 1;

		for (iformat = 0; iformat < length_format; iformat++)
		{
			cformat = format[iformat];
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, "StringUtil.formatString", "Format character :\"" + cformat + "\", vindex = " + vindex);
			}
			if (cformat == '%')
			{
				// The start of a format field.  Get the rest so that we can process.  First advance one...
				dot_found = false;
				left_shift = false;
				pound_format = false;
				zero_format = false;
				iprecision = 0;
				iwidth = 0;
				++iformat;
				if (iformat >= length_format)
				{
					// End of format...
					break;
				}
				// On the character after the %
				first = true;
				for (; iformat < length_format; iformat++)
				{
					cformat = format[iformat];
					if (Message.isDebugOn)
					{
						Message.printDebug(dl, "StringUtil.formatString", "Format character :\"" + cformat + "\" vindex =" + vindex);
					}
					if (first)
					{
						// First character after the %...
						// Need to update so that some of the following can be combined.
						if (cformat == '%')
						{
							// Literal percent...
							buffer.Append('%');
							first = false;
							break;
						}
						else if (cformat == 'c')
						{
							// Append a Character from the list...
							buffer.Append(v[vindex].ToString());
							if (Message.isDebugOn)
							{
								Message.printDebug(dl, "StringUtil.formatString", "Processed list[" + vindex + "], a char");
							}
							++vindex;
							first = false;
							break;
						}
						else if (cformat == '-')
						{
							// Left shift...
							left_shift = true;
							continue;
						}
						else if (cformat == '#')
						{
							// Special format...
							pound_format = true;
							continue;
						}
						else if (cformat == '0')
						{
							// Leading zeros...
							zero_format = true;
							continue;
						}
						else
						{
							// Not a recognized formatting character so we will just go
							// to the next checks outside this loop...
							first = false;
						}
					}
					// Else retrieving characters until an ending "s", "i", "d", or "f" is encountered.
					if (char.IsDigit(cformat) || (cformat == '.'))
					{
						if (cformat == '.')
						{
							dot_found = true;
							continue;
						}
						if (dot_found)
						{
							// part of the precision...
							sprecision[iprecision] = cformat;
							++iprecision;
						}
						else
						{
							// part of the width...
							swidth[iwidth] = cformat;
							++iwidth;
						}
						continue;
					}
					if ((cformat != 'd') && (cformat != 'f') && (cformat != 'F') && (cformat != 's'))
					{
						Message.printWarning(3, "StringUtil.formatString", "Invalid format string character (" + cformat + ") in format (" + format + ").");
						break;
					}
					// If here, have a valid format string and need to process...

					// First get the width and precision on the format...

					// Get the desired output width and precision (already initialize to zeros above)...

					if (iwidth > 0)
					{
						width = int.Parse((new string(swidth)).Substring(0,iwidth));
					}

					if (iprecision > 0)
					{
						precision = int.Parse((new string(sprecision)).Substring(0,iprecision));
					}

					// Check to see if the number of formats is greater than the input list.  If so, this
					// is likely a programming error so print a warning so the developer can fix.

					if (vindex > vsizem1)
					{
						Message.printWarning(3, "StringUtil.formatString", "The number of format strings \"" + format + "\" is > the number of data.  Check code.");
						return buffer.ToString();
					}

					// Now format for the different data types...

					if (cformat == 'd')
					{
						// Integer.  If NULL or an empty string, just add a blank string of the desired width...
						if (v[vindex] == default(T1))
						{
							if (Message.isDebugOn)
							{
								Message.printDebug(dl, "StringUtil.formatString", "NULL integer");
							}
							// NULL string.  Set it to be spaces for the width requested.
							for (i = 0; i < width; i++)
							{
								buffer.Append(' ');
							}
							++vindex;
							break;
						}
						StringBuilder temp = new StringBuilder(v[vindex].ToString());
						if (temp.Length == 0)
						{
							if (Message.isDebugOn)
							{
								Message.printDebug(dl, "StringUtil.formatString", "Zero length string for integer");
							}
							// Empty string.  Set it to be spaces for the width requested.
							for (i = 0; i < width; i++)
							{
								buffer.Append(' ');
							}
							++vindex;
							break;
						}
						if (Message.isDebugOn)
						{
							Message.printDebug(dl, "StringUtil.formatString", "Processing list[" + vindex + "], an integer \"" + temp + "\"");
						}
						++vindex;
						cvalue = temp[0];
						if (cvalue == '-')
						{
							sign = 1;
						}
						else
						{
							sign = 0;
						}
						// String will be left-justified so we need to see if we need to shift
						// right.  Allow overflow.  "temp" already has the sign in it.
						length_temp = temp.Length;
						diff = width - length_temp;
						if (diff > 0)
						{
							if (left_shift)
							{
								if (zero_format)
								{
									// Need to add zeros in the front...
									if (sign == 1)
									{
										offset = 1;
									}
									else
									{
										offset = 0;
									}
									for (j = 0; j < diff; j++)
									{
										temp.Insert(offset, '0');
									}
								}
								else
								{
									// Add spaces at the end...
									for (j = 0; j < diff; j++)
									{
										temp.Insert(length_temp,' ');
									}
								}
							}
							else
							{
								// Add spaces at the beginning...
								if (sign == 1)
								{
									offset = 1;
								}
								else
								{
									offset = 0;
								}
								if (zero_format)
								{
									// Add zeros...
									for (j = 0; j < diff; j++)
									{
										temp.Insert(offset,'0');
									}
								}
								else
								{
									for (j = 0; j < diff; j++)
									{
										temp.Insert(0, ' ');
									}
								}
							}
						}
						buffer.Append(temp);
					}
					else if ((cformat == 'f') || (cformat == 'F'))
					{
						// Float.  First, get the whole number as a string...
						// If NULL, just add a blank string of the desired width...
						if (v[vindex] == default(T1))
						{
							if (Message.isDebugOn)
							{
								Message.printDebug(dl, "StringUtil.formatString", "NULL float");
							}
							// NULL string.  Set it to be spaces for the width requested.
							for (i = 0; i < width; i++)
							{
								buffer.Append(' ');
							}
							++vindex;
							break;
						}
						StringBuilder temp = new StringBuilder();
						string whole_number_string;
						string remainder_string;
						string number_as_string = "";
						int point_pos;
						if (cformat == 'f')
						{
							number_as_string = v[vindex].ToString();
						}
						else if (cformat == 'F')
						{
							number_as_string = v[vindex].ToString();
						}
						if (number_as_string.Length == 0)
						{
							if (Message.isDebugOn)
							{
								Message.printDebug(dl, "StringUtil.formatString", "Zero length string for float");
							}
							// Empty string.  Set it to be spaces for the width requested.
							for (i = 0; i < width; i++)
							{
								buffer.Append(' ');
							}
							++vindex;
							break;
						}
						else if (number_as_string.Equals("NaN"))
						{
							// Pad with spaces and justify according to the formatting.
							if (left_shift)
							{
								buffer.Append("NaN");
								for (i = 0; i < (width - 3); i++)
								{
									buffer.Append(' ');
								}
							}
							else
							{
								for (i = 0; i < (width - 3); i++)
								{
									buffer.Append(' ');
								}
								buffer.Append("NaN");
							}
							++vindex;
							break;
						}
						// Need to check here as to whether the number is less than 10^-3 or greater
						// than 10^7, in which case the string comes back in exponential notation
						// and fouls up the rest of the process...
						int E_pos = number_as_string.IndexOf('E');
						if (E_pos >= 0)
						{
							// Scientific notation.  Get the parts to the number and then
							// put back together.  According to the documentation, the
							// format is -X.YYYE-ZZZ where the first sign is optional, the first digit (X)
							// is mandatory (and non-zero), the YYYY are variable length, the sign after the E is
							// mandatory, and the exponent is variable length.  The sign after the E appears to be optional.
							if (Message.isDebugOn)
							{
								Message.printDebug(dl, "StringUtil.formatString", "Detected scientific notation for Double: " + number_as_string);
							}
							StringBuilder expanded_string = new StringBuilder();
							int sign_offset = 0;
							if (number_as_string[0] == '-')
							{
								expanded_string.Append("-");
								sign_offset = 1;
							}
							// Position of dot in float...
							int dot_pos = number_as_string.IndexOf('.');
							// Sign of the exponent...
							char E_sign = number_as_string[E_pos + 1];
							// Exponent as an integer...
							int exponent = 0;
							if ((E_sign == '-') || (E_sign == '+'))
							{
								exponent = atoi(number_as_string.Substring(E_pos + 2));
							}
							else
							{
								// No sign on exponent.
								exponent = atoi(number_as_string.Substring(E_pos + 1));
							}
							// Left side of number...
							string left = number_as_string.Substring(sign_offset, dot_pos - sign_offset);
							// Right side of number...
							string right = number_as_string.Substring((dot_pos + 1), E_pos - (dot_pos + 1));
							// Add to the buffer on the left side of the number...
							if (E_sign == '-')
							{
								// Add zeros on the left...
								int dot_shift = exponent - 1;
								expanded_string.Append(".");
								for (int ishift = 0; ishift < dot_shift; ishift++)
								{
									expanded_string.Append("0");
								}
								expanded_string.Append(left);
								expanded_string.Append(right);
							}
							else
							{
								// Shift the decimal to the right...
								expanded_string.Append(left);
								// Now transfer as many digits as available.
								int len_right = right.Length;
								for (int ishift = 0; ishift < exponent; ishift++)
								{
									if (ishift <= (len_right - 1))
									{
										expanded_string.Append(right[ishift]);
									}
									else
									{
										expanded_string.Append("0");
									}
								}
								expanded_string.Append(".");
								// If we did not shift through all the original right-side digits, add them now...
								if (exponent < len_right)
								{
									expanded_string.Append(right.Substring(exponent));
								}
							}
							// Now reset the string...
							number_as_string = expanded_string.ToString();
							if (Message.isDebugOn)
							{
								Message.printDebug(dl, "StringUtil.formatString", "Expanded number: \"" + number_as_string + "\"");
							}
						}
						if (Message.isDebugOn)
						{
							Message.printDebug(dl, "StringUtil.formatString", "Processing list[" + vindex + "], a float or double \"" + number_as_string + "\"");
						}
						++vindex;
						// Figure out if negative...
						if (number_as_string[0] == '-')
						{
							sign = 1;
						}
						else
						{
							sign = 0;
						}
						// Find the position of the decimal point...
						point_pos = number_as_string.IndexOf('.');
						if (point_pos == -1)
						{
							// No decimal point.
							whole_number_string = number_as_string;
							remainder_string = "";
						}
						else
						{
							// has decimal point
							whole_number_string = number_as_string.Substring(0,point_pos);
							remainder_string = number_as_string.Substring(point_pos + 1);
						}
						// Round the number so that the number of precision digits exactly matches what we want...
						if (precision < remainder_string.Length)
						{
							number_as_string = StringUtil.round(number_as_string, precision);
							// We may need to recompute the parts of the string.  Just do it for now...
							// Figure out if negative...
							if (number_as_string[0] == '-')
							{
								sign = 1;
							}
							else
							{
								sign = 0;
							}
							// Find the position of the decimal point...
							point_pos = number_as_string.IndexOf('.');
							if (point_pos == -1)
							{
								// No decimal point.
								whole_number_string = number_as_string;
								remainder_string = "";
							}
							else
							{
								// has decimal point
								whole_number_string = number_as_string.Substring(0,point_pos);
								remainder_string = number_as_string.Substring(point_pos + 1);
							}
						}
						// Now start at the back of the string and start adding parts...
						if (precision > 0)
						{
							int iprec;
							// First fill with zeros for the precision amount...
							for (iprec = 0; iprec < precision; iprec++)
							{
								temp.Insert(0, '0');
							}
							// Now overwrite with the actual numbers...
							iend = remainder_string.Length;
							if (iend > precision)
							{
								iend = precision;
							}
							for (iprec = 0; iprec < iend; iprec++)
							{
								temp[iprec] = remainder_string[iprec];
							}
							// Round off the last one if there is truncation.  Deal with this later...
							if (precision < remainder_string.Length)
							{
								// TODO - old comment: working on doing the round above...
							}
							// Now add the decimal point...
							temp.Insert(0, '.');
						}
						else if ((precision == 0) && pound_format)
						{
							// Always add a decimal point...
							temp.Insert(0, '.');
						}
						// Now add the whole number.  If it overflows, that is OK.  If it is
						// less than the width we will deal with it in the next step.
						temp.Insert(0, whole_number_string);
						// If the number that we have now is less than the desired width, we need
						// to add spaces.  Depending on the sign in the format, we add them at the left or right.
						if (temp.Length < width)
						{
							int ishift;
							iend = width - temp.Length;
							if (left_shift)
							{
								// Add at the end...
								for (ishift = 0; ishift < iend; ishift++)
								{
									temp.Insert(temp.Length, ' ');
								}
							}
							else
							{
								// Add at the end..
								for (ishift = 0; ishift < iend; ishift++)
								{
									if (zero_format)
									{
										// Format was similar to "%05.1f"
										temp.Insert(0, '0');
									}
									else
									{
										temp.Insert(0, ' ');
									}
								}
							}
						}

						// Append to our main string...
						buffer.Append(temp);
					}
					else if (cformat == 's')
					{
						// First set the string the requested size, which is the precision.  If the
						// precision is zero, do the whole thing.  String will be left-justified so we
						// need to see if we need to shift right.  Allow overflow...
						// If NULL, just add a blank string of the desired width...
						if (v[vindex] == default(T1))
						{
							if (Message.isDebugOn)
							{
								Message.printDebug(dl, "StringUtil.formatString", "NULL string");
							}
							// NULL string.  Set it to be spaces for the width requested.
							for (i = 0; i < precision; i++)
							{
								buffer.Append(' ');
							}
							++vindex;
							break;
						}
						StringBuilder temp = new StringBuilder(v[vindex].ToString());
						if (temp.Length == 0)
						{
							if (Message.isDebugOn)
							{
								Message.printDebug(dl, "StringUtil.formatString", "Zero length string");
							}
							// Empty string.  Set it to be spaces for the width requested.
							for (i = 0; i < width; i++)
							{
								buffer.Append(' ');
							}
							++vindex;
							break;
						}
						if (Message.isDebugOn)
						{
							Message.printDebug(dl, "StringUtil.formatString", "Processing list[" + vindex + "], a string \"" + temp + "\"");
						}
						++vindex;
						if (iprecision > 0)
						{
							// Now figure out whether we need to right-justify...
							diff = precision - temp.Length;
							if (!left_shift)
							{
								// Right justify...
								if (diff > 0)
								{
									for (j = 0; j < diff; j++)
									{
										temp.Insert(0, ' ');
									}
								}
							}
							else
							{
								// Left justify.  Set the buffer to the precision...
								temp.Length = precision;
								// Now fill the end with spaces instead of NULLs...
								for (j = (precision - diff); j < precision; j++)
								{
									temp[j] = ' ';
								}
							}
							// If our string length is longer than the string, append a substring...
							if (temp.Length > precision)
							{
								buffer.Append(temp.ToString().Substring(0,precision));
							}
							else
							{
								// Do the whole string...
								buffer.Append(temp.ToString());
							}
						}
						else
						{
							// Write the whole string...
							if (temp != null)
							{
								buffer.Append(temp);
							}
						}
					}
					// End of a format string.  Break out and look for the next one...
					break;
				}
			}
			else
			{
				// A normal character so just add to the buffer...
				buffer.Append(cformat);
			}
		}

		return buffer.ToString();
	}

	// Simple variations on formatString for single objects...
	// TODO SAM 2010-06-15 Need to figure out how to not create lists on each call, but needs to be thread safe

	/// <summary>
	/// Format a double as a string. </summary>
	/// <returns> Formatted string. </returns>
	/// <param name="d"> A double to format. </param>
	/// <param name="format"> Format to use. </param>
	public static string formatString(double d, string format)
	{
		IList<double> v = new List<double>(1, 1);
		v.Add(new double?(d));
		return formatString(v, format);
	}

	/// <summary>
	/// Format a Double as a string. </summary>
	/// <returns> Formatted string. </returns>
	/// <param name="d"> A Double to format. </param>
	/// <param name="format"> Format to use. </param>
	public static string formatString(double? d, string format)
	{
		IList<double> v = new List<double>(1, 1);
		v.Add(d);
		return formatString(v, format);
	}

	/// <summary>
	/// Format a float as a string. </summary>
	/// <returns> Formatted string. </returns>
	/// <param name="f"> A float to format. </param>
	/// <param name="format"> Format to use. </param>
	public static string formatString(float f, string format)
	{
		IList<float> v = new List<float>(1, 1);
		v.Add(new float?(f));
		return formatString(v, format);
	}

	/// <summary>
	/// Format an int as a string. </summary>
	/// <returns> Formatted string. </returns>
	/// <param name="i"> An int to format. </param>
	/// <param name="format"> Format to use. </param>
	public static string formatString(int i, string format)
	{
		IList<int> v = new List<int>(1, 1);
		v.Add(new int?(i));
		return formatString(v, format);
	}

	/// <summary>
	/// Format an Integer as a string. </summary>
	/// <returns> Formatted string. </returns>
	/// <param name="i"> An Integer to format. </param>
	/// <param name="format"> Format to use. </param>
	public static string formatString(int? i, string format)
	{
		IList<int> v = new List<int>(1, 1);
		v.Add(i);
		return formatString(v, format);
	}

	/// <summary>
	/// Format a long as a string. </summary>
	/// <returns> Formatted string. </returns>
	/// <param name="l"> A long to format. </param>
	/// <param name="format"> Format to use. </param>
	public static string formatString(long l, string format)
	{
		IList<long> v = new List<long>(1, 1);
		v.Add(new long?(l));
		return formatString(v, format);
	}

	/// <summary>
	/// Format an object as a string. </summary>
	/// <returns> Formatted string. </returns>
	/// <param name="o"> An object to format. </param>
	/// <param name="format"> Format to use. </param>
	public static string formatString(object o, string format)
	{
		IList<object> v = new List<object>(1);
		v.Add(o);
		return formatString(v, format);
	}

	/// <summary>
	/// Format a string for output to a CSV file.  The following actions are taken:
	/// <ol>
	/// <li> If the string contains a comma or "alwaysQuote" is true, the string is surrounded by quotes.</li>
	/// <li> If the string contains double quotes, each double quote is replaced with two double quotes, as per Excel conventions.
	/// </ol> </summary>
	/// <param name="s"> string to process </param>
	/// <param name="treatAsString"> if true, always return a result enclosed in double quotes, regardless of contents. </param>
	/// <returns> the string formatted for inclusion as an item in an Excel CSV file. </returns>
	public static string formatStringForCsv(string s, bool alwaysQuote)
	{
		StringBuilder b = new StringBuilder();
		if (alwaysQuote || (s.IndexOf(",", StringComparison.Ordinal) >= 0))
		{
			b.Append("\"");
		}
		int length = s.Length;
		char c;
		for (int i = 0; i < length; i++)
		{
			c = s[i];
			if (c == '"')
			{
				// Detected a quote so output double quotes.
				b.Append("\"\"");
			}
			else
			{
				// Just append the character
				b.Append(c);
			}
		}
		if (alwaysQuote || (s.IndexOf(",", StringComparison.Ordinal) >= 0))
		{
			b.Append("\"");
		}
		return b.ToString();
	}

	/// <summary>
	/// Return an array of valid format specifiers for the formatString() method, in
	/// the format "%X - Description" where X is the format specifier.  The specifiers correspond to the C sprintf
	/// formatting routine. </summary>
	/// <returns> an array of format specifiers. </returns>
	/// <param name="includeDescription"> If false, only the %X specifiers are returned.  if
	/// True, the description is also returned. </param>
	/// <param name="forOutput"> if true, return specifiers for formatting; if false only include formatters for parsing </param>
	public static string[] getStringFormatSpecifiers(bool includeDescription, bool forOutput)
	{
		string[] formats = new string[12];
		formats[0] = "%% - literal percent";
		formats[1] = "%c - single character";
		formats[2] = "%d - integer";
		formats[3] = "%4d - integer, 4-digit width";
		formats[4] = "%-d - integer, left justified";
		formats[5] = "%-4d - integer, 4-digit width, left justified";
		formats[6] = "%f - floating point";
		formats[7] = "%8.2f - floating point, 8 digits wide, 2 decimls";
		formats[8] = "%-8.2f - floating point, 8 digits wide, 2 decimls, left justified";
		formats[9] = "%-f - floating point, left justified";
		formats[10] = "%s - string";
		formats[11] = "%20.20s - string padded to width";
		return formats;
	}

	/// <summary>
	/// Return a token in a string or null if no token.  This method calls
	/// breakStringList() and returns the requested token or null if out of range. </summary>
	/// <param name="string"> The string to break. </param>
	/// <param name="delim"> A String containing characters to treat as delimiters. </param>
	/// <param name="flag"> Bitmask indicating how to break the string.  Specify
	/// DELIM_SKIP_BLANKS to skip blank fields and
	/// DELIM_ALLOW_STRINGS to allow quoted strings (which may contain delimiters). </param>
	/// <param name="token"> Token to return (starting with 0). </param>
	/// <returns> the requested token or null. </returns>
	public static string getToken(string @string, string delim, int flag, int token)
	{
		if (token < 0)
		{
			return null;
		}
		IList<string> v = breakStringList(@string, delim, flag);
		if (v == null)
		{
			return null;
		}
		if (v.Count < (token + 1))
		{
			return null;
		}
		return v[token];
	}

	// TODO SAM 2009-06-01 Evaluate whether to deprecate, etc given that it should not be ignore case
	// based on the name of the method.
	/// <summary>
	/// Return index of string in string list.  If string is not in string list,
	/// -1 is returned.  <b>A case-insensitive compare is used.</b> </summary>
	/// <returns> Index of string in stringlist (or -1). </returns>
	/// <param name="stringlist"> List of strings to search. </param>
	/// <param name="searchString"> String to return index of. </param>
	public static int indexOf(IList<string> stringlist, string searchString)
	{
		if (stringlist == null || string.ReferenceEquals(searchString, null))
		{
			return -1;
		}
		int num_strings = stringlist.Count;
		string currentString;
		for (int i = 0; i < num_strings; i++)
		{
			currentString = stringlist[i];
			if (string.ReferenceEquals(currentString, null))
			{
				// skip
			}
			else if (currentString.Equals(searchString, StringComparison.OrdinalIgnoreCase))
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// Return index of string in string list.  If string is not in string list,
	/// -1 is returned.  A case-insensitive compare is used. </summary>
	/// <returns> Index of string in stringlist (or -1). </returns>
	/// <param name="stringlist"> List of strings to search. </param>
	/// <param name="searchString"> String to return index of. </param>
	public static int indexOfIgnoreCase(IList<string> stringlist, string searchString)
	{
		if (stringlist == null || string.ReferenceEquals(searchString, null))
		{
			return -1;
		}

		int num_strings = stringlist.Count;
		string currentString;
		for (int i = 0; i < num_strings; i++)
		{
			currentString = stringlist[i];
			if (string.ReferenceEquals(currentString, null))
			{
				// skip
			}
			else if (currentString.Equals(searchString, StringComparison.OrdinalIgnoreCase))
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// Determine whether a string exists in another string, ignoring case. </summary>
	/// <param name="full"> Full string to check. </param>
	/// <param name="substring"> Substring to find in "full". </param>
	/// <param name="fromIndex"> The index where the search should begin. </param>
	/// <returns> position of substring or -1 if not found. </returns>
	public static int indexOfIgnoreCase(string full, string substring, int fromIndex)
	{ // Convert both strings to uppercase and then do the comparison.
		string full_up = full.ToUpper();
		string substring_up = substring.ToUpper();
		int pos = full_up.IndexOf(substring_up, fromIndex, StringComparison.Ordinal);
		return pos;
	}

	/// <summary>
	/// Determine whether a string is an ASCII string. </summary>
	/// <returns> true if the string is an ASCII string. </returns>
	/// <param name="s"> String to check. </param>
	public static bool isASCII(string s)
	{
		int sLength = s.Length;
		char[] c = new char[sLength];

		// Get character array
		try
		{
			s.CopyTo(0, c, 0, sLength - 0);
		}
		catch (StringIndexOutOfBoundsException)
		{
			return false;
		}

		// Loop through character array checking to make sure it is ASCII
		for (int i = 0;i < sLength;i++)
		{
			if ((!char.IsLetterOrDigit(c[i]) && !char.IsWhiteSpace(c[i]) && c[i] != '.' && c[i] != '-' && c[i] != '_') || (charToHex(c[i])).CompareTo("007F") > 0)
			{
				return false;
			}
		}

		// Return true if it makes it to here
		return true;
	}

	/// <summary>
	/// Determine whether a string can be converted to a boolean. </summary>
	/// <returns> true if the string can be converted to a boolean ("true" or "false"), false otherwise. </returns>
	/// <param name="s"> String to convert. </param>
	public static bool isBoolean(string s)
	{
		if (string.ReferenceEquals(s, null))
		{
			return false;
		}
		if (s.Equals("true", StringComparison.OrdinalIgnoreCase) || s.Equals("false", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Determine whether a string is a double precision value. </summary>
	/// <returns> true if the string can be converted to a double. </returns>
	/// <param name="s"> String to convert. </param>
	public static bool isDouble(string s)
	{
		if (string.ReferenceEquals(s, null))
		{
			return false;
		}
		try
		{
			(Convert.ToDouble(s.Trim()));
			return true;
		}
		catch (System.FormatException)
		{
			return false;
		}
	}

	/// <summary>
	/// Determine whether a string can be converted to an integer. </summary>
	/// <returns> true if the string can be converted to a integer. </returns>
	/// <param name="s"> String to convert. </param>
	public static bool isInteger(string s)
	{
		if (string.ReferenceEquals(s, null))
		{
			return false;
		}
		try
		{
			int.Parse(s.Trim());
			return true;
		}
		catch (System.FormatException)
		{
			return false;
		}
	}

	/// <summary>
	/// Determine whether a string can be converted to a long. </summary>
	/// <returns> true if the string can be converted to a long. </returns>
	/// <param name="s"> String to convert. </param>
	public static bool isLong(string s)
	{
		if (string.ReferenceEquals(s, null))
		{
			return false;
		}
		try
		{
			(Convert.ToInt64(s));
			return true;
		}
		catch (System.FormatException)
		{
			return false;
		}
	}

	/// <summary>
	/// Wrap text by breaking a string into lines that are less than or equal to a desired length. </summary>
	/// <returns> the text as a new string delimited by the line break characters. </returns>
	/// <param name="string"> String to wrap. </param>
	/// <param name="maxlength"> Maximum length of the string. </param>
	/// <param name="linebreak"> Character(s) to insert at the end of a line (e.g., "\n").
	/// If not specified, "\n" is used. </param>
	public static string lineWrap(string @string, int maxlength, string linebreak)
	{
		if ((string.ReferenceEquals(linebreak, null)) || linebreak.Equals(""))
		{
			linebreak = "\n";
		}
		if (string.ReferenceEquals(@string, null))
		{
			return linebreak;
		}
		if (@string.Length <= maxlength)
		{
			return @string + linebreak;
		}
		// For now just do breakStringList() using white space.  However, need
		// to consider commas, etc.  One idea would be to loop through the
		// string and start at the next maxlength point.  Then go back to find
		// a delimiter.  If none is found, insert one somewhere or move forward.
		//
		// Also need to consider Tom's code.
		IList<string> v = breakStringList(@string, " \t\n", 0);
		int size = 0;
		if (v != null)
		{
			size = v.Count;
		}
		StringBuilder main_buffer = new StringBuilder();
		StringBuilder sub_buffer = new StringBuilder();
		string token = null;
		for (int i = 0; i < size; i++)
		{
			token = v[i];
			if ((sub_buffer.Length + 1 + token.Length) > maxlength)
			{
				// Add the sub_buffer to the buffer...
				main_buffer.Append(sub_buffer.ToString() + linebreak);
				sub_buffer.Length = 0;
				sub_buffer.Append(token);
			}
			else
			{
				// Add the token to the sub_buffer...
				sub_buffer.Append(" " + token);
			}
		}
		if (sub_buffer.Length > 0)
		{
			main_buffer.Append(sub_buffer.ToString() + linebreak);
		}
		return main_buffer.ToString();
	}

	/// <summary>
	/// Convert a string containing literal representations of characters (e.g., "\t") to
	/// the internal equivalent (e.g., tab character).  This should be used with care, for example
	/// to convert a visual delimiter string into the internal equivalent.  The string combinations
	/// that are recognized are: "\t" (tab) and "\s" (space). </summary>
	/// <param name="s"> string to convert </param>
	/// <returns> the converted string, or null if the input string is null </returns>
	public static string literalToInternal(string s)
	{
		if (!string.ReferenceEquals(s, null))
		{
			s = s.Replace("\\t", "\t");
			s = s.Replace("\\s", " ");
		}
		return s;
	}

	/// <summary>
	/// Determine the maximum size of the String in a list. </summary>
	/// <param name="v"> list of objects to check the size.  The toString() method is called
	/// to get a String representation of the object for the check. </param>
	/// <returns> the maximum size or -1 if it cannot be determined. </returns>
	public static int maxSize(IList<string> v)
	{
		int size = 0;
		int maxsize = -1;
		int len = 0;
		if (v != null)
		{
			len = v.Count;
		}
		object o;
		for (int i = 0; i < len; i++)
		{
			o = v[i];
			if (o == null)
			{
				continue;
			}
			size = o.ToString().Length;
			if (size > maxsize)
			{
				maxsize = size;
			}
		}
		return maxsize;
	}

	/// <summary>
	/// Determine if strings match, while ignoring uppercase/lowercase.  The input
	/// strings are converted to uppercase before the comparison is made. </summary>
	/// <returns> true if the string matches the regular expression, ignoring case. </returns>
	/// <param name="s"> String to check. </param>
	/// <param name="regex"> Regular expression used as input to String.matches(). </param>
	public static bool matchesIgnoreCase(string s, string regex)
	{
		return s.ToUpper().matches(regex.ToUpper());
	}

	/// <summary>
	/// ** Notes:	(1)	This routine compares a candidate string with a string
	/// **			that contains regular expression wildcard characters and
	/// **			returns 1 if the candidate string matches.  The
	/// **			following wild cards are currently recognized:
	/// **
	/// **				.	Match one character.
	/// **				*	Match zero or more characters.
	/// **				[...]	Match any one the characters enclosed
	/// **					in the brackets.  This can be a list
	/// **					of characters or a character range
	/// **					separated by a -.
	/// **		(2)	This routine is designed to be called by a higher level
	/// **			routine to check actual filenames versus wildcard patterns.
	/// **		(3)	The following combination is known not to work:
	/// **
	/// **				xxx*.[abc]
	/// **
	/// **			and will be fixed as time allows.
	/// </pre> </summary>
	/// <param name="ignore_case"> if true, case will be ignored when comparing strings.  If
	/// false, strings will be compared literally. </param>
	/// <param name="candidate_string"> String to evaluate. </param>
	/// <param name="regexp_string"> Regular expression string to match. </param>
	/// @deprecated Use the standard String.matches() method or StringUtil.matchesIgnoreCase(). 
	public static bool matchesRegExp(bool ignore_case, string candidate_string, string regexp_string)
	{
		string okchars = "", routine = "StringUtil.mtchesRegExp";
		int dl = 50, nokchars = 0;
		bool asterisk = false, jumptotest = false;

		if (string.ReferenceEquals(candidate_string, null))
		{
			return false;
		}
		if (string.ReferenceEquals(regexp_string, null))
		{
			return false;
		}
		int candidate_len = candidate_string.Length;
		int regexp_len = regexp_string.Length;

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Comparing \"" + candidate_string + "\" to \"" + regexp_string + "\"");
		}

		// Put in this quick check because the code does not seem to be
		// working but need to get something delivered for regular expressions that end in *...

		if (regexp_string.EndsWith("*", StringComparison.Ordinal) && (StringUtil.patternCount(regexp_string,"*") == 1))
		{
			// The regular expression is xxx* so do a quick check...
			int endpos = regexp_string.IndexOf("*", StringComparison.Ordinal);
			if (endpos == 0)
			{
				return true;
			}
			if (candidate_string.Length < endpos)
			{
				// Candidate string is not long enough to compare
				// needs to be as long as the regular expression without the *)...
				return false;
			}
			if (ignore_case)
			{
				if (regexp_string.Substring(0,endpos).Equals(candidate_string.Substring(0,endpos), StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			else
			{
				if (regexp_string.Substring(0,endpos).Equals(candidate_string.Substring(0,endpos)))
				{
					return true;
				}
			}
		}

		// ican = position in candiate_string
		// ireg = position in regexp_string
		// ccan = character in candidate_string
		// creg = character in regexp_string
		int ican = 0, ireg = 0;
		char ccan, creg;
		while (true)
		{
			// Start new segment in the regular expression...
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "Start new segment section");
			}
			if (!jumptotest)
			{
				asterisk = false;
				for (ireg = 0; ireg < regexp_len; ireg++)
				{
					creg = regexp_string[ireg];
					if (creg != '*')
					{
						break;
					}
					// Else equals '*'..
					asterisk = true;
				}
			}

			// Now test for a match...

			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine,"Start test section");
			}

			jumptotest = false;
			// i is position in regexp_string, j in candidate_string.
			while (true)
			{
				creg = regexp_string[ireg];
				for (ican = 0; (ireg < regexp_len) && (creg != '*'); ireg++, ican++)
				{
					creg = regexp_string[ireg];
					if (ican >= candidate_len)
					{
						// No match...
						return false;
					}
					ccan = candidate_string[ican];
					if (Message.isDebugOn)
					{
						Message.printDebug(dl, routine, "regexp_string[" + ireg + "]=" + creg + "candidate_string[" + ican + "]=" + ccan);
					}
					if (creg != ccan)
					{
						if (creg == '.')
						{
							// Single character match...
							if (Message.isDebugOn)
							{
								Message.printDebug(dl, routine, "Character . - go to next character");
							}
							continue;
						}
						else if (creg == '[')
						{
							// Start of character range.
							// First need to get OK characters...
							if (Message.isDebugOn)
							{
								Message.printDebug(dl, routine, "[ - check range character");
							}
							++ireg;
							while (true)
							{
								if (ireg >= regexp_len)
								{
									return false;
								}
								creg = regexp_string[ireg];
								if (creg != ']')
								{
									break;
								}
								else if (creg == '-')
								{
									// Need to find the next character and then go until that matches...
									++ireg;
									if (ireg >= regexp_len)
									{
										return false;
									}
									creg = regexp_string[ireg];
									if ((nokchars > 0) && (creg < okchars[nokchars - 1]))
									{
										return false;
									}
									if (Message.isDebugOn)
									{
										Message.printDebug(dl, routine, "Using range " + okchars[nokchars - 1] + " to " + creg);
									}
									while (true)
									{
										okchars += okchars[nokchars - 1] + 1;
										++nokchars;
										if (Message.isDebugOn)
										{
											Message.printDebug(dl, routine, "Added " + okchars[nokchars - 1] + " from [-] list");
										}
										if (okchars[nokchars - 1] == creg)
										{
											// Last character in range...
											break;
										}
									}
								}
								else
								{
									// Just add the character...
									okchars += creg;
									++nokchars;
									if (Message.isDebugOn)
									{
										Message.printDebug(dl, routine, "Added " + okchars[nokchars - 1] + " from [abc] list");
									}
									++ireg;
								}
							}
							// Now check the character...
							if (okchars.IndexOf(ccan) >= 0)
							{
								// Matches OK...
								continue;
							}
							else
							{
								// No match...
								return false;
							}
						}
						else if (!asterisk)
						{
							// ?
							if (Message.isDebugOn)
							{
								Message.printDebug(dl, routine, "Not asterisk.");
							}
							return false;
						}
						// increment candidate...
						++ican;
						// Reevaluate the loop again...
						if (Message.isDebugOn)
						{
							Message.printDebug(dl, routine, "Jumping to test");
						}
						jumptotest = true;
						break;
					}
					else
					{
						if (Message.isDebugOn)
						{
							Message.printDebug(dl, routine, "Chars are equal.  Increment...");
						}
					}
				}
				if (jumptotest || (ireg >= regexp_len) || (creg == '*'))
				{
					break;
				}
			}

			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "Outside for loop");
			}

			if (!jumptotest)
			{
				if (creg == '*')
				{
					//if ( Message.isDebugOn ) {
					//	Message.printDebug ( dl, routine, "Have an * - increment by " + i + " and restart segment" );
					//}
					// Don't need?
					//pt_candidate	+= j;
					//pt_regexp	+= i;
					continue;
				}

				if (ican >= candidate_len)
				{
					// End of string...
					if (Message.isDebugOn)
					{
						Message.printDebug(dl, routine, "End of string.");
					}
					return true;
				}
				else if ((ireg > 0) && (regexp_string[ireg - 1] == '*'))
				{
					// Rest of string is wildcard...
					if (Message.isDebugOn)
					{
						Message.printDebug(dl, routine, "Rest of string *.");
					}
					return true;
				}
				else if (!asterisk)
				{
					if (Message.isDebugOn)
					{
						Message.printDebug(dl, routine, "Not asterisk.");
					}
					return false;
				}
				// Don't need?
				//++pt_candidate;
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, routine, "Jumping to test");
				}
				jumptotest = true;
			}
		}
	}

	/// <summary>
	/// Check to see if a String matches a regular expression, considering case explicitly. </summary>
	/// <param name="candidate_string"> String to evaluate. </param>
	/// <param name="regexp_string"> Regular expression string to match. </param>
	/// <returns> true if the candidate string matches the regular expression. </returns>
	/// @deprecated Use the standard String.matches() method or StringUtil.matchesIgnoreCase(). 
	public static bool matchesRegExp(string candidate_string, string regexp_string)
	{
		return matchesRegExp(false, candidate_string, regexp_string);
	}

	/// <summary>
	/// Parse a string like "1, 2, 3" or "1,2,3" or "1-3,4,6-10" into an array containing the numbers.
	/// Single values result in a range where the start and end value are the same. </summary>
	/// <param name="seq"> string to parse </param>
	/// <param name="delim"> delimiter characters </param>
	/// <param name="parseFlag"> see breakStringList() flag </param>
	/// <param name="offset"> number to add to each value, useful when converting from user-specified values to
	/// internal zero-index values - specify zero if no offset is required </param>
	/// <returns> an array of integers parsed from the string. </returns>
	public static int [][] parseIntegerRangeSequence(string seq, string delim, int parseFlag, int offset)
	{
		if (string.ReferenceEquals(seq, null))
		{
			return new int[0][];
		}
		IList<string> tokens = breakStringList(seq, delim, parseFlag);
		int size = 0;
		if (tokens != null)
		{
			size = tokens.Count;
		}
		if (size == 0)
		{
			return new int[0][];
		}
		else
		{
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: int[][] ranges = new int[size][2];
			int[][] ranges = RectangularArrays.RectangularIntArray(size, 2);
			for (int i = 0; i < size; i++)
			{
				string token = tokens[i];
				if (token.IndexOf("-", StringComparison.Ordinal) > 0)
				{
					// Range.  Split out the start and end of the range
					IList<string> tokens2 = breakStringList(token, "-", 0);
					ranges[i][0] = int.Parse(tokens2[0].Trim()) + offset;
					ranges[i][1] = int.Parse(tokens2[1].Trim()) + offset;
				}
				else
				{
					// Single number
					ranges[i][0] = int.Parse(token.Trim()) + offset;
					ranges[i][1] = ranges[i][0];
				}
			}
			return ranges;
		}
	}

	/// <summary>
	/// Parse a string like "1, 2, 3" or "1,2,3" into an array containing the numbers. </summary>
	/// <param name="seq"> string to parse </param>
	/// <param name="delim"> delimiter characters </param>
	/// <param name="parseFlag"> see breakStringList() flag </param>
	/// <returns> an array of integers parsed from the string. </returns>
	public static int [] parseIntegerSequenceArray(string seq, string delim, int parseFlag)
	{
		if (string.ReferenceEquals(seq, null))
		{
			return new int[0];
		}
		IList<string> tokens = breakStringList(seq, delim, parseFlag);
		int size = 0;
		if (tokens != null)
		{
			size = tokens.Count;
		}
		if (size == 0)
		{
			return new int[0];
		}
		else
		{
			int[] iseq = new int[size];
			for (int i = 0; i < size; i++)
			{
				iseq[i] = int.Parse(tokens[i].Trim());
			}
			return iseq;
		}
	}

	/// <summary>
	/// Parse a string like "1:3" or "1:" or "1:-2" into an array containing the positional numbers, where the
	/// parts are start, stop, and optionally step.
	/// Single values result in a range where the start and end value are the same.
	/// Note that this does NOT impose zero-offset indexing (like Python) - the range is 1+ to nVals, which
	/// is more conducive to applications with users who are not programmers. </summary>
	/// <param name="seq"> string to parse </param>
	/// <param name="delim"> delimiter character(s) </param>
	/// <param name="parseFlag"> see breakStringList() flag </param>
	/// <param name="count"> the number of values in the list corresponding to the slice, needed if the slice uses
	/// ending notation like -2. </param>
	/// <returns> an array of integers parsed from the string. </returns>
	public static int [] parseIntegerSlice(string seq, string delim, int parseFlag, int count)
	{
		if (string.ReferenceEquals(seq, null))
		{
			return new int[0];
		}
		IList<string> tokens = breakStringList(seq, delim, parseFlag);
		if (seq.EndsWith(delim, StringComparison.Ordinal))
		{
			// breakStringList won't return a token at the end
			tokens.Add("");
		}
		int size = 0;
		if (tokens != null)
		{
			size = tokens.Count;
		}
		if (size == 0)
		{
			return new int[0];
		}
		else if (size == 1)
		{
			// Single value
			int[] vals = new int[1];
			vals[0] = int.Parse(tokens[0]);
			return vals;
		}
		else
		{
			// Start value...
			int start = 1; // Default
			string token = tokens[0];
			if (!token.Equals(""))
			{
				start = int.Parse(token);
			}
			// End value...
			int end = count; // Default
			token = tokens[1];
			if (token.Equals(""))
			{
				// End has not been specified so loop to the count
				end = count;
			}
			else
			{
				// End has been specified...
				end = int.Parse(token);
				if (end < 0)
				{
					// Negative number so relative to the count
					end = end + count;
				}
			}
			// Determine the step
			int step = 1;
			if (size == 3)
			{
				// Have a step
				step = int.Parse(tokens[2]);
			}
			// Determine the number of values...
			// FIXME SAM 2010-12-17 Need more checks on invalid integers to avoid negative nVals
			//Message.printStatus(2,"", "Seq=\""+seq+"\" delim=\""+delim+"\" flag=" + parseFlag + " count="+count +
			//      " start=" + start + " end=" + end + " step=" + step );
			int nVals = (end - start) / step + 1;
			// Now iterate and generate the sequence
			int[] vals = new int[nVals];
			int i = 0;
			for (int ival = start; ival <= end; ival += step)
			{
				vals[i++] = ival;
			}
			return vals;
		}
	}

	/// <summary>
	/// Count the number of unique (non-overlapping) instances of a pattern in a string. </summary>
	/// <param name="s"> String to search. </param>
	/// <param name="pattern"> Pattern to search for.  Currently this can only be a one-character string. </param>
	/// <returns> The count of the unique instances. </returns>
	public static int patternCount(string s, string pattern)
	{
		int count = 0;
		if ((string.ReferenceEquals(s, null)) || (string.ReferenceEquals(pattern, null)) || (pattern.Length < 1))
		{
			return count;
		}
		int size = s.Length;
		char c = pattern[0];
		for (int i = 0; i < size; i++)
		{
			if (s[i] == c)
			{
				++count;
			}
		}
		return count;
	}

	/// <summary>
	/// Return "s" if the count is > 1 or an empty string if 1.  This is used to form strings that may or may not refer
	/// to a plural. </summary>
	/// <param name="count"> the number of objects being evaluted to determine if plural. </param>
	/// <returns> "s" if count is > 1, and "" otherwise. </returns>
	public static string pluralS(int count)
	{
		if (count > 1)
		{
			return "s";
		}
		else
		{
			return "";
		}
	}

	/// <returns> String up to but not including the delimiter character. </returns>
	/// <param name="string0"> String to read from. </param>
	/// <param name="delim"> Delimiter character to read to. </param>
	public static string readToDelim(string string0, char delim)
	{
		int i = 0;
		char c;
		StringBuilder @string = new StringBuilder();

		if (string.ReferenceEquals(string0, null))
		{
			return @string.ToString();
		}
		do
		{
			c = string0[i];
			if (c == delim)
			{
				return @string.ToString();
			}
			else
			{
				@string.Append(c);
			}
			i++;
		} while (c != '\0');
		return @string.ToString();
	}

	/// <summary>
	/// Remove a character from a string. </summary>
	/// <returns> String that has the character removed. </returns>
	/// <param name="s"> String to remove character from. </param>
	/// <param name="r"> String to remove. </param>
	public static string remove(string s, string r)
	{
		if ((string.ReferenceEquals(s, null)) || (string.ReferenceEquals(r, null)))
		{
			return s;
		}
		StringBuilder buffer = new StringBuilder();
		int size = s.Length;
		int r_length = r.Length;
		for (int i = 0; i < size; i++)
		{
			if (s.IndexOf(r,i, StringComparison.Ordinal) == i)
			{
				// Skip next few characters...
				i += (r_length - 1);
			}
			else
			{
				buffer.Append(s[i]);
			}
		}
		return buffer.ToString();
	}

	/// <summary>
	/// Remove matching strings from a list. </summary>
	/// <param name="strings"> list of strings to process </param>
	/// <param name="regex"> Java regular expression as per String.match() - if null then null strings will be matched </param>
	/// <param name="ignoreCase"> if true then the strings and regular expression will be compared as uppercase </param>
	public static void removeMatching(IList<string> strings, string regex, bool ignoreCase)
	{
		if (strings == null)
		{
			return;
		}
		string s;
		string regexUpper = regex.ToUpper();
		bool matches = false;
		for (int i = 0; i < strings.Count; i++)
		{
			matches = false;
			s = strings[i];
			if (string.ReferenceEquals(s, null))
			{
				// Special case for removing nulls
				if (string.ReferenceEquals(regex, null))
				{
					matches = true;
				}
			}
			else
			{
				if (ignoreCase)
				{
					matches = s.ToUpper().matches(regexUpper);
				}
				else
				{
					matches = s.matches(regex);
				}
				if (matches)
				{
					strings.Remove(i);
					--i;
				}
			}
		}
	}

	/// <summary>
	/// Remove matching strings from a list. </summary>
	/// <returns> the number of strings removed </returns>
	/// <param name="strings"> main list of strings to process </param>
	/// <param name="strings2"> list of strings to remove from main list </param>
	/// <param name="ignoreCase"> if true then the strings will be compared as uppercase </param>
	public static int removeMatching(IList<string> strings, IList<string> strings2, bool ignoreCase)
	{
		if ((strings == null) || (strings2 == null))
		{
			return 0;
		}
		int size = strings.Count;
		string s;
		bool match;
		int matchCount = 0;
		for (int i = 0; i < size; i++)
		{
			s = strings[i];
			if (string.ReferenceEquals(s, null))
			{
				continue;
			}
			match = false;
			foreach (string s2 in strings2)
			{
				if (string.ReferenceEquals(s2, null))
				{
					continue;
				}
				if (ignoreCase)
				{
					if (s.Equals(s2, StringComparison.OrdinalIgnoreCase))
					{
						match = true;
						break;
					}
				}
				else
				{
					if (s.Equals(s2))
					{
						match = true;
						break;
					}
				}
			}
			if (match)
			{
				// Remove from list
				strings.Remove(i);
				--i;
			}
		}
		return matchCount;
	}

	/// <summary>
	/// Remove the duplicates from a list of String.  The input list is modified so
	/// make a copy before calling this method if necessary. </summary>
	/// <param name="strings"> list of String to evaluate. </param>
	/// <param name="ignore_case"> If true, case is ignored in making string comparisons. </param>
	/// <param name="sorted"> If true, the input list is assumed to be sorted - this
	/// increases processing speed.  TRUE IS CURRENTLY THE ONLY VALUE THAT IS SUPPORTED. </param>
	/// <returns> the list with duplicate values rememoved. </returns>
	public static IList<string> removeDuplicates(IList<string> strings, bool ignore_case, bool sorted)
	{
		if (sorted)
		{
			// Loop through and compare each string with the previous string
			// in the list, removing the current string if a duplicate.
			int size = 0;
			if (strings != null)
			{
				size = strings.Count;
			}
			string @string, string0 = null;
			if (size > 0)
			{
				string0 = strings[0];
			}
			for (int i = 1; i < size; i++)
			{
				@string = strings[i];
				if (ignore_case)
				{
					if (@string.Equals(string0, StringComparison.OrdinalIgnoreCase))
					{
						strings.RemoveAt(i);
						--i;
						--size;
					}
				}
				else if (@string.Equals(string0))
				{
					strings.Remove(i);
					--i;
					--size;
				}
				string0 = @string;
			}
		}
		return strings;
	}

	/// <summary>
	/// Remove the newline character(s) from a string.
	/// The newline pattern for UNIX or PC machines is recognized, as appropriate. </summary>
	/// <returns> String that has the newline character removed. </returns>
	/// <param name="string"> String to remove newline read from. </param>
	public static string removeNewline(string @string)
	{
		char c, c2 = '\n';
		int k;

		if (string.ReferenceEquals(@string, null))
		{
			return @string;
		}
		int string_length = @string.Length;
		if (string_length == 0)
		{
			return @string;
		}
		for (int i = 0; i < string_length; i++)
		{
			c = @string[i];
			if ((c == '\n') || (c == '\r'))
			{
				// Regardless of platform a newline is always initiated by a \n character
				// See if the character after newline(s) is the end of the string...
				for (k = (i + 1); k < string_length; k++)
				{
					c2 = @string[k];
					if ((c2 != '\n') && (c2 != '\r'))
					{
						break;
					}
				}
				if ((c2 == '\n') || (c2 == '\r'))
				{
					// Nothing after the newline(s).  Return the string up to that point.  substring will
					// return up to i - 1!
					string newstring = @string.Substring(0, i);
					return newstring;
				}
				else
				{
					// Something after the newline(s)...
					 //*pt = ' ';
					Message.printWarning(3, "StringUtil.removeNewline", "embedded newlines not handled yet");
					// FIXME SAM 2009-01-19 Need to use a StringBuilder or something to better handle
					// embedded newlines.
					/*
					try {
						throw new Exception(
							"embedded newlines not"
							+ "handled yet");
					}
					catch (Exception e) {
						Message.printWarning(2, "", e);
					}
					*/
				}
			}
		}
		// If we get to here there were no newlines so just return...
		return @string;
	}

	/// <summary>
	/// The newline pattern for UNIX or PC machines is recognized, as appropriate. </summary>
	/// <returns> String that has the newline character removed. </returns>
	/// <param name="string"> String to remove newline read from. </param>
	public static string removeNewline(StringBuilder @string)
	{
		return removeNewline(@string.ToString());
	}

	/// <summary>
	/// Reorder the string list given the order array (for example created by the sortStringList() method). </summary>
	/// <param name="strings"> the string list to sort </param>
	/// <param name="sortOrder"> an array indicating the order that the strings should be in (e.g., if sortOrder[0] = 15, then
	/// string [15] should be used in position 0. </param>
	/// <param name="createNewList"> if true, create and return a new list; if false, reorder the provided list in place </param>
	public static IList<string> reorderStringList(IList<string> strings, int[] sortOrder, bool createNewList)
	{
		List<string> strings2 = new List<string>(strings.Count);
		for (int i = 0; i < strings.Count; i++)
		{
			strings2.Add(strings[sortOrder[i]]);
		}
		if (createNewList)
		{
			return strings2;
		}
		else
		{
			for (int i = 0; i < strings.Count; i++)
			{
				strings[i] = strings2[i];
			}
			return strings;
		}
	}

	/// <summary>
	/// Replaces every instance of a given substring in one string with
	/// another substring.  The replacement is not recursive.  This method can be used
	/// instead of the newer String.replace*() methods when using older versions of Java. </summary>
	/// <param name="strOrig"> the String in which the string replacement should occur. </param>
	/// <param name="s1"> the String to be replaced. </param>
	/// <param name="s2"> the String to replace s1. </param>
	/// <returns> str with every instance of s1 replaced with s2. </returns>
	public static string replaceString(string strOrig, string s1, string s2)
	{
		if (string.ReferenceEquals(strOrig, null))
		{
			return strOrig;
		}
		if (strOrig.Length == 0)
		{
			return strOrig;
		}

		string str = strOrig;
		int i = str.IndexOf(s1, StringComparison.Ordinal);
		int s1_len = s1.Length; // length of string to replace
		int s2_len = s2.Length; // Length of string replace with
		int len = str.Length;
		string before = null;
		string after = null;
		int start = 0;
		while (i >= 0)
		{
			// If in here, then we need to do a replacement...
			// String before the match...
			before = str.Substring(0, i);
			if (i == (len - 1))
			{
				// At the end of the string...
				str = before + s2;
				i = -1;
			}
			else
			{
				after = str.Substring(i + s1_len);
				str = before + s2 + after;
				start = before.Length + s2_len;
				i = str.IndexOf(s1, start, StringComparison.Ordinal);
			}
		}
		return str;
	}

	/// <summary>
	/// Given a string representation of a floating point number, round to the
	/// desired precision.  Currently this operates on a string (and not a double)
	/// because the method is called from the formatString() method that operates on strings. </summary>
	/// <returns> String representation of the rounded floating point number. </returns>
	/// <param name="string"> String containing a floating point number. </param>
	/// <param name="precision"> Number of digits after the decimal point to round the number. </param>
	public static string round(string @string, int precision)
	{
		string new_string;

		// First break the string into its integer and remainder parts...
		int dot_pos = @string.IndexOf('.');
		if (dot_pos < 0)
		{
			// No decimal.
			return @string;
		}
		// If we get to here there is a decimal.  Figure out the size of the integer and the remainder...
		int integer_length = dot_pos;
		int remainder_length = @string.Length - integer_length - 1;
		if (remainder_length == precision)
		{
			// Then our precision matches the remainder length and we can return the original string...
			return @string;
		}
		else if (remainder_length < precision)
		{
			// If the remainder length is less than the precision, then we
			// can just add zeros on the end of the original string until we get to the precision length...
		}
		// If we get to here we need to do the more complicated roundoff 
		// stuff.  First check if the precision is zero.  If so, round off the main number and return...
		if (precision == 0)
		{
			long ltemp = (long)Math.Round((Convert.ToDouble(@string)), MidpointRounding.AwayFromZero);
			return ((new long?(ltemp)).ToString());
		}
		// If we get to here, we have more than a zero precision and need to
		// jump through some hoops.  First, create a new string that has the remainder...
		StringBuilder remainder_string = new StringBuilder(@string.Substring(dot_pos + 1));
		// Next insert a decimal point after the precision digits.
		remainder_string.Insert(precision,'.');
		// Now convert the string to a Double...
		double? dtemp = Convert.ToDouble(remainder_string.ToString());
		// Now round...
		long ltemp = (long)Math.Round(dtemp.Value, MidpointRounding.AwayFromZero);
		// Now convert back to a string...
		string rounded_remainder = (new long?(ltemp)).ToString();
		 string integer_string = @string.Substring(0,integer_length);
		if (rounded_remainder.Length < precision)
		{
			// The number we were working with had leading zeros and we
			// lost that during the round.  Insert zeros again...
			StringBuilder buf = new StringBuilder(rounded_remainder);
			int number_to_add = precision - rounded_remainder.Length;
			for (int i = 0; i < number_to_add; i++)
			{
				buf.Insert(0,'0');
			}
			new_string = integer_string + "." + buf.ToString();
			return new_string;
		}
		else if (rounded_remainder.Length > precision)
		{
			// We have, during rounding, had to carry over into the next
			// larger ten's spot (for example, 99.6 has been rounded to
			// 100.  Therefore, we need to use all but the first digit of
			// the rounded remainder and we need to increment our original number (or decrement if negative!).
			char first_char = @string[0];
			long new_long = (Convert.ToInt64(integer_string));
			if (first_char == '-')
			{
				// Negative...
				--new_long;
			}
			else
			{
				// Positive...
				++new_long;
			}
			new_string = new_long + "." + rounded_remainder.Substring(1);
			return new_string;
		}
		// Now put together the string again...
		new_string = integer_string + "." + ltemp;

	/*
		if ( Message.isDebugOn ) {
			Message.printDebug ( 20, routine, "Original: " + string +
			" new: " + new_string );
		}
	*/
		return new_string;
	}

	// showControl - do a verbose output of a string and show control characters
	//
	// Notes:	(1)	This is mainly used for debugging Java bugs and
	//			understanding the language better.
	/// <summary>
	/// This is mainly used for Java debugging and testing. </summary>
	/// <returns> A list of strings, each of which is the expanded character for a character in the original string. </returns>
	/// <param name="string"> String to print control characters for. </param>
	public static IList<string> showControl(string @string)
	{
		IList<string> v = new List<string>();

		int length = @string.Length;
		char c;
		string control;
		for (int i = 0; i < length; i++)
		{
			c = @string[i];
			if (char.IsControl(c))
			{
				// Control character...
				if (c == '\r')
				{
					control = "CR";
				}
				else if (c == '\n')
				{
					control = "NL";
				}
				else
				{
					control = "Ctrl-unknown(" + c + ")";
				}
				v.Add("Letter [" + i + "]: " + control);
			}
			else if (char.IsLetterOrDigit(c))
			{
				// Print it...
				v.Add("Letter [" + i + "]: " + c);
			}
			else
			{
				// Don't handle...
				v.Add("Letter [" + i + "]: unknown(" + c + ")");
			}
		}
		return v;
	}

	/// <summary>
	/// Sort a list of strings into ascending order, considering case. </summary>
	/// <returns> The sorted list (a new list is returned). </returns>
	/// <param name="list"> The original list of String. </param>
	public static IList<string> sortStringList(IList<string> list)
	{
		return sortStringList(list, SORT_ASCENDING, null, false, false);
	}

	/// <summary>
	/// Sort a list of strings. </summary>
	/// <returns> The sorted list (a new list is always returned, even if empty). </returns>
	/// <param name="list"> The original list of String. </param>
	/// <param name="order"> Order to sort (SORT_ASCENDING or SORT_DESCENDING). </param>
	/// <param name="sort_order"> Original locations of data after sort (array needs to be
	/// allocated before calling routine).  For example, first sort String data and then
	/// sort associated data by using new_other_data[i] = old_other_data[sort_order[i]];
	/// Can be null if sflag is false. </param>
	/// <param name="sflag"> Indicates whether "sort_order" is to be filled. </param>
	/// <param name="ignore_case"> If true, then case is ignored when comparing the strings. </param>
	public static IList<string> sortStringList(IList<string> list, int order, int[] sort_order, bool sflag, bool ignore_case)
	{
		int i, ismallest;
		int[] itmp = null;
		string routine = "StringUtil.sortStringList", smallest = "";

		if ((list == null) || (list.Count == 0))
		{
			Message.printWarning(50, routine, "NULL string list");
			// Always return a new list
			return new List<string>();
		}
		int size = list.Count;

		IList<string> list_tosort = list;
		if (ignore_case)
		{
			// Create a new list that is all upper case...
			list_tosort = new List<string>(size);
			string @string = null;
			for (int j = 0; j < size; j++)
			{
				@string = list[j];
				if (string.ReferenceEquals(@string, null))
				{
					list_tosort.Add(@string);
				}
				else
				{
					list_tosort.Add(@string.ToUpper());
				}
			}
		}
		IList<string> newlist = new List<string>(size);

		// Allocate memory for the temporary int array used to keep track of the sort order...

		itmp = new int [size];

		for (i = 0; i < size; i++)
		{
			itmp[i] = 0; // indicates not in new list yet
		}

		// OK, now do the sort.  Just do a buble sort and go through the entire
		// list twice.  Note that "issmallest" is used even if finding the largest for descending sort.

		int count = 0;
		while (true)
		{
			ismallest = -1;
			for (i = 0; i < size; i++)
			{
				if (itmp[i] != 0)
				{
					// Already in the new list...
					continue;
				}
				// Save the "smallest" string (null is considered smallest).  If this is the first
				// string encountered this iteration, initialize with the first string...
				// TODO SAM 2013-09-15 How to handle nulls?
				if ((ismallest == -1) || ((order == SORT_ASCENDING) && (list_tosort[i].CompareTo(smallest) < 0)) || ((order == SORT_DESCENDING) && (list_tosort[i].CompareTo(smallest) > 0)))
				{
					smallest = list_tosort[i];
					ismallest = i;
				}
			}
			if (ismallest == -1)
			{
				// We have exhausted the search so break out...
				break;
			}
			// Put in the original item (which will have the original case)...
			newlist.Add(list[ismallest]);
			if (sflag)
			{
				sort_order[count++] = ismallest;
			}

			itmp[ismallest] = 1;
		}
		return newlist;
	}

	/// <summary>
	/// Checks to see if one String starts with another, ignoring case. </summary>
	/// <param name="s"> the String to check if it begins with the other </param>
	/// <param name="pattern"> the String that is being checked if it is the start of the other string. </param>
	/// <returns> true if the second String is the starting String in the first. </returns>
	public static bool startsWithIgnoreCase(string s, string pattern)
	{
		if ((string.ReferenceEquals(s, null)) || (string.ReferenceEquals(pattern, null)))
		{
			return false;
		}
		int plen = pattern.Length;
		int slen = s.Length;
		if ((plen == 0) || (slen < plen))
		{
			return false;
		}
		string sub = s.Substring(0, plen);
		return sub.regionMatches(true,0,pattern,0,plen);
	}

	/// <summary>
	/// Checks to see if two strings (of which either or both may be null) are equal. </summary>
	/// <param name="s1"> the first String to check. </param>
	/// <param name="s2"> the second String to check. </param>
	/// <returns> true if the Strings are equal (null == null), false if not. </returns>
	public static bool stringsAreEqual(string s1, string s2)
	{
		if (string.ReferenceEquals(s1, null) && string.ReferenceEquals(s2, null))
		{
			return true;
		}
		if (string.ReferenceEquals(s1, null) || string.ReferenceEquals(s2, null))
		{
			return false;
		}
		if (s1.Trim().Equals(s2.Trim()))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Convert a list of strings to an array of strings. </summary>
	/// <returns> An array containing the strings. </returns>
	/// <param name="v"> list of strings to convert. </param>
	public static string[] toArray(IList<string> v)
	{
		if (v == null)
		{
			return null;
		}
		int vector_size = v.Count;
		string[] array = new string [vector_size];
		for (int i = 0; i < vector_size; i++)
		{
			array[i] = v[i];
		}
		return array;
	}

	/// <summary>
	/// Return the count of the tokens in a string or null if no token.  This method
	/// calls breakStringList() and returns the resulting count. </summary>
	/// <param name="string"> The string to break. </param>
	/// <param name="delim"> A String containing characters to treat as delimiters. </param>
	/// <param name="flag"> Bitmask indicating how to break the string.  Specify
	/// DELIM_SKIP_BLANKS to skip blank fields and
	/// DELIM_ALLOW_STRINGS to allow quoted strings (which may contain delimiters). </param>
	/// <returns> the first token or null. </returns>
	public static int tokenCount(string @string, string delim, int flag)
	{
		IList<string> v = breakStringList(@string, delim, flag);
		if (v == null)
		{
			return 0;
		}
		int size = v.Count;
		v = null;
		return size;
	}

	/// <summary>
	/// Convert an array of strings to a List of strings. </summary>
	/// <returns> A List containing the strings. </returns>
	/// <param name="array"> Array of strings to convert. </param>
	public static IList<string> toList(string[] array)
	{
		if (array == null)
		{
			return null;
		}
		int array_size = array.Length;
		IList<string> v = new List<string>(array_size, 50);
		for (int i = 0; i < array_size; i++)
		{
			v.Add(array[i]);
		}
		return v;
	}

	/// <summary>
	/// Convert an enumeration of strings to a list of strings. </summary>
	/// <returns> A list containing the strings. </returns>
	/// <param name="e"> Enumeration of strings to convert. </param>
	public static IList<string> toList(IEnumerator<string> e)
	{
		if (e == null)
		{
			return null;
		}
		IList<string> v = new List<string>(50);
		while (e.MoveNext())
		{
			v.Add(e.Current);
		}
		return v;
	}

	/// <summary>
	/// Convert a list of strings into one long string that is delimited by the
	/// given string (usually the system line separator).  Null strings are treated
	/// as empty strings.  This is useful for converting lists to something that a TextArea can display. </summary>
	/// <param name="delimiter"> delimiter to include between each string, or null to not use a delimiter. </param>
	/// <returns> the combined string, or null if the original list is null. </returns>
	public static string ToString(IList<string> strings, string delimiter)
	{
		if (strings == null)
		{
			return null;
		}
		StringBuilder buffer = new StringBuilder();
		int size = strings.Count;
		for (int i = 0; i < size; i++)
		{
			if ((i > 0) && (!string.ReferenceEquals(delimiter, null)))
			{
				buffer.Append(delimiter);
			}
			buffer.Append(strings[i]);
		}
		return buffer.ToString();
	}

	/// <summary>
	/// Remove characters from string. </summary>
	/// <returns> A string that has been unpadded (whitespace removed from front, back and/or middle). </returns>
	/// <param name="string"> String to unpad. </param>
	/// <param name="white0"> Whitespace characters to remove. </param>
	/// <param name="flag"> Bitmask indicating how to unpad.  Can be
	/// PAD_FRONT, PAD_BACK, PAD_MIDDLE, PAD_FRONT_BACK, or PAD_FRONT_MIDDLE_BACK. </param>
	public static string unpad(string @string, string white0, int flag)
	{
		int length_string, length_white;
		string default_white = " \t\n\r", white;
		StringBuilder buffer;

		// Check for NULL prointers...

		if (string.ReferenceEquals(@string, null))
		{
			return @string;
		}

		// Set default whitespace characters if not specified...

		if (string.ReferenceEquals(white0, null))
		{
			white = default_white;
			length_white = white.Length;
		}
		else
		{
			length_white = white0.Length;
			if (length_white == 0)
			{
				white = default_white;
				length_white = white.Length;
			}
			else
			{
				white = white0;
				length_white = white.Length;
			}
		}

		length_string = @string.Length;
		if (length_string == 0)
		{
			return @string;
		}

		int istring;
		char cstring = '\0';

		// Unpad the whole string...

		if ((flag == StringUtil.PAD_FRONT_MIDDLE_BACK) && (length_string > 0))
		{
			buffer = new StringBuilder();
			for (istring = 0; istring < length_string; istring++)
			{
				cstring = @string[istring];
				if (white.IndexOf(cstring) != -1)
				{
					// Don't transfer the character...
					continue;
				}
				buffer.Append(cstring);
			}
			return buffer.ToString();
		}

		buffer = new StringBuilder(@string);

		// Do the back first so that we do not shift the string yet...

		if (((flag & StringUtil.PAD_BACK) != 0) && (length_string > 0))
		{
			// Remove whitespace from back...
			istring = length_string - 1;
			if (istring >= 0)
			{
				cstring = @string[istring];
				while ((istring >= 0) && (white.IndexOf(cstring) != -1))
				{
					// Shorten by one character as we backtrack...
					--length_string;
					if (length_string < 0)
					{
						length_string = 0;
					}
					buffer.Length = length_string;
					--istring;
					if (istring >= 0)
					{
						cstring = @string[istring];
					}
				}
			}
			//Message.printDebug ( dl, routine, "Result after \"%s\" off back: \"%s\".", white, string );
		}

		// Now do the front...

		int skip_count = 0;
		if (((flag & StringUtil.PAD_FRONT) != 0) && (length_string > 0))
		{
			// Remove whitespace from front...
			istring = 0;
			cstring = @string[istring];
			while ((istring < length_string) && (white.IndexOf(cstring) != -1))
			{
				// Skipping leading whitespace...
				++skip_count;
				++istring;
				if (istring < length_string)
				{
					cstring = @string[istring];
				}
			}
			if (skip_count > 0)
			{
				// We need to shift the string...
				return (buffer.ToString().Substring(skip_count));
			}
			//buffer.append ( string.substring(istring) );
			//strcpy ( string, pt );
			//Message.printDebug ( dl, routine, "Result after \"%s\" off front: \"%s\".", white, string );
		}

		// Else, return the string from the front and back operations...
		return buffer.ToString();
	}

	/// <summary>
	/// This is essentially equivalent to String.trim(). </summary>
	/// <returns> A string that has had spaces removed from the front and back. </returns>
	/// <param name="string"> The string to unpad. </param>
	public static string unpad(string @string)
	{
		return (unpad(@string, " ", PAD_FRONT_BACK));
	}

	/// <summary>
	/// This is the same as the String version, but allows a StringBuffer as input.
	/// </summary>
	public static string unpad(StringBuilder @string, string white0, int flag)
	{
		return unpad(@string.ToString(), white0, flag);
	}

	/// <summary>
	/// Wraps text to fit on a line of a certain length.  Text is wrapped at newlines,
	/// commas, periods, exclamation marks, question marks, open and close parentheses,
	/// open and close braces, open and close curly brackets, colons, semicolons,
	/// spaces, tabs, backslashes and forward slashes. </summary>
	/// <param name="s"> the text to wrap to fit on a certain-length line. </param>
	/// <param name="lineLength"> the maximum length of a line of text.  Must be at least 2. </param>
	/// <returns> a wrapped that will fit on lines of the given length. </returns>
	public static string wrap(string s, int lineLength)
	{
		IList<string> v = StringUtil.breakStringList(s, "\n", 0);
		StringBuilder sb = new StringBuilder("");

		for (int i = 0; i < v.Count; i++)
		{
			sb.Append(wrapHelper(v[i], lineLength));
		}
		return sb.ToString();
	}

	/// <summary>
	/// Wraps text to fit on a line of a certain length.  Text is wrapped at newlines,
	/// commas, periods, exclamation marks, question marks, open and close parentheses,
	/// open and close braces, open and close curly brackets, colons, semicolons,
	/// spaces, tabs, backslashes and forward slashes. </summary>
	/// <param name="s"> the text to wrap to fit on a certain-length line. </param>
	/// <param name="lineLength"> the maximum length of a line of text.  Must be at least 2. </param>
	/// <returns> a wrapped that will fit on lines of the given length. </returns>
	public static string wrapHelper(string s, int lineLength)
	{
		if (lineLength < 2)
		{
			return "";
		}

		// the most-recently-located index of a point in the text at which a wrap can occur
		int next = -1;
		// the previously-located index of a point in the text at which a wrap could occur
		int last = -1;
		string trim = null;
		StringBuilder sb = new StringBuilder();

		// first check for the trivial case -- a String that's shorter than the maximum allowed line length
		if (s.Length <= lineLength)
		{
			sb.Append(s + "\n");
			return sb.ToString();
		}

		while (true)
		{
			last = next;

			// find the next point from which a line wrap can possibly occur.
			next = wrapFindFirstWrappableIndex(s, next + 1);

			// if the next point for a valid wordwrap is beyond the maximum allowable line length ...
			if (next > lineLength)
			{
				// ... and no previous word wrap point was found ...
				if (last == -1)
				{
					// ... split the text up the length of the line.
					// Put in a hyphen, and then carry over the 
					// rest of the text after the hyphen to
					// be checked for wordwraps in the next
					// iteration.
					trim = s.Substring(0, lineLength - 1);
					trim += "-";
					sb.Append(trim + "\n");
					s = s.Substring(lineLength - 1);
					s = s.Trim();
					last = -1;
					next = -1;
				}
				// ... and a previous wrap point was found.
				else
				{
					// ... split the text at the point of the 
					// previous word wrap point and then let the 
					// rest of the string be carried over to be 
					// checked in the next iteration.
					trim = s.Substring(0, last);
					sb.Append(trim + "\n");
					s = s.Substring(last);
					s = s.Trim();
					last = -1;
					next = -1;
				}
			}
			// if the next wrap point if exactly on the barrier between
			// maximum line length and invalid line length ...
			else if (next == lineLength)
			{
				// ... a perfect fit was found, so take all the text 
				// up the next wrap point and put it on one line,
				// then carry over the rest of the text to be checked
				// in a later iteration.
				trim = s.Substring(0, next);
				sb.Append(trim + "\n");
				s = s.Substring(next);
				s = s.Trim();
				last = -1;
				next = -1;
			}
			// if no valid wrap points can be found in the rest of the
			// text, but a valid wrap point was found just prior ...
			else if (next == -1 && last > -1)
			{
				// ... means this is possibly the last line.  See
				// if what is left will fit on one line ...
				if (s.Length <= lineLength)
				{
					sb.Append(s + "\n");
					return sb.ToString();
				}

				// ... but if not, take the text up to the last 
				// wrap point and put it on a line and then take 
				// the rest of the text and prepare it to be handled 
				// in the next iteration.

				trim = s.Substring(0, last);
				sb.Append(trim + "\n");
				s = s.Substring(last);
				s = s.Trim();
				last = -1;
				next = -1;
			}
			// if no valid wrap points can be found through the end
			// of the text, and none were found previously ...
			else if (next == -1 && last == -1)
			{
				// ... then the end of the text is getting close.

				// if the text is still longer than the maximum 
				// allowable line length ...
				if (s.Length > lineLength)
				{
					// ... then loop through and separate it
					// into hyphenatable chunks and put each 
					// chunk on a line.  
					while (true)
					{
						trim = s.Substring(0, lineLength - 1);
						trim += "-";
						sb.Append(trim + "\n");
						s = s.Substring(lineLength - 1);
						s = s.Trim();
						if (s.Length <= lineLength)
						{
							sb.Append(s + "\n");
							return sb.ToString();
						}
					}
				}
				// if the remaining text can all fit on one line,
				// put it on one line and return it.
				else
				{
					sb.Append(s + "\n");
					return sb.ToString();
				}
			}
		}
	}

	/// <summary>
	/// A helper function used by wrap() to locate a the point at which a line of text can be wrapped. </summary>
	/// <param name="s"> the text to check. </param>
	/// <param name="from"> the point from which to check the text. </param>
	/// <returns> the location of the next immediate wrap point, or -1 if none can be found. </returns>
	private static int wrapFindFirstWrappableIndex(string s, int from)
	{
		// There are two batches of characters to be checked and each batch must be handled differently.  

		// In the first case are characters that denote that the line can be wrapped immediately AFTERWARDS
		int index1 = wrapFindFirstWrappableIndexHelper(s, ".", from, -1);
		index1 = wrapFindFirstWrappableIndexHelper(s, ",", from, index1);
		index1 = wrapFindFirstWrappableIndexHelper(s, "!", from, index1);
		index1 = wrapFindFirstWrappableIndexHelper(s, "?", from, index1);
		index1 = wrapFindFirstWrappableIndexHelper(s, ";", from, index1);
		index1 = wrapFindFirstWrappableIndexHelper(s, ":", from, index1);
		index1 = wrapFindFirstWrappableIndexHelper(s, ")", from, index1);
		index1 = wrapFindFirstWrappableIndexHelper(s, "}", from, index1);
		index1 = wrapFindFirstWrappableIndexHelper(s, "]", from, index1);
		index1 = wrapFindFirstWrappableIndexHelper(s, "/", from, index1);
		index1 = wrapFindFirstWrappableIndexHelper(s, "\\", from, index1);

		// In the second case are characters that denote that the line must be wrapped BEFORE.
		int index2 = wrapFindFirstWrappableIndexHelper(s, "(", from, -1);
		index2 = wrapFindFirstWrappableIndexHelper(s, "{", from, index2);
		index2 = wrapFindFirstWrappableIndexHelper(s, "[", from, index2);
		index2 = wrapFindFirstWrappableIndexHelper(s, " ", from, index2);
		index2 = wrapFindFirstWrappableIndexHelper(s, "\t", from, index2);

		/*
		So given a line like this:
			The solution (X) is easy!  Okay.
		
		If the line needs to be wrapped at the open parentheses, the wrap
		is performed before, so that the wrap would look like:
			The solution
			(X) is easy!  Okay.
		and not:
			The solution (
			X) is easy!  Okay.
		
		Likewise, if the wrap must be performed at the exclamation point,
		the wrap should look like:
			The solution (X) is easy!
			Okay.
		and not:
			The solution (X) is easy
			!  Okay.
		*/

		int index = -1;

		// if no valid wrap point could be found from the second batch ...
		if (index2 == -1)
		{
			// .. but the first batch had a valid point ...
			if (index1 > -1)
			{
				// ... increment the wrap position, in accord with
				// how the first batch wrap positions are set
				index = index1 + 1;
			}
			else
			{
				// (redundant, just to show what will actually be
				// returned from the method below)
				index = -1;
			}
		}
		// ... if a valid wrap point was found in the second batch ...
		else
		{
			// ... and no valid wrap point was found in the first ...
			if (index1 == -1)
			{
				// .. the wrap point is the second batch's point.
				index = index2;
			}
			// ... AND the first batch had a valid wrap point ...
			else
			{
				// ... choose whichever is lesser.  The two values
				// will NEVER be the same.

				if (index2 < index1)
				{
					index = index2;
				}
				else
				{
					index = index1 + 1;
				}
			}
		}

		return index;
	}

	/// <summary>
	/// Helper method for wrapFindFirstWrappableIndex to locate the first index of
	/// a character in a line from a specified point and compare it to a 
	/// previously-determined index of another character in the line. </summary>
	/// <param name="s"> the text to check. </param>
	/// <param name="ch"> the character to find the index of. </param>
	/// <param name="from"> the point from which to search for the index. </param>
	/// <param name="index"> the location found for a wrappable point in the same line of text
	/// from the same from point, but for a different character. </param>
	/// <returns> the earliest wrap point based on the wrap point that is found and the
	/// previously-found wrap point (index), or -1 if none can be found. </returns>
	private static int wrapFindFirstWrappableIndexHelper(string s, string ch, int from, int index)
	{
		// Find the first position of the specified character from the specified start point.
		int i = s.IndexOf(ch, from, StringComparison.Ordinal);

		// If no position was found, and no position had been found previously ...
		if (i == -1 && index == -1)
		{
			return -1;
		}

		// If a position was found this time, but none had been found previously ...
		if (i > -1 && index == -1)
		{
			return i;
		}
		// If no position was found this time, but one had been found previously ...
		else if (i == -1 && index > -1)
		{
			return index;
		}
		// If a position was found this time AND one had been found previously ...
		else
		{
			// return whichever is smaller.
			if (index < i)
			{
				return index;
			}
			else
			{
				return i;
			}
		}
	}

	}

}