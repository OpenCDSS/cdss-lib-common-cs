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
    using Message = Message.Message;

    /// <summary>
	/// This class provides static utility routines for manipulating strings.
	/// </summary>
    public class StringUtil
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
                field_widths[field_count] = int.Parse(width_string.ToString());
                ++field_count;
            }
            width_string = null;
            // ...END OF INLINED CODE
            // Now do the read...	
            IList<object> v = fixedRead(@string, field_types, field_widths, null);
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
        public static string formatString<T1>(IList<T1> v, string format) // where T1 : object
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
                            width = int.Parse((new string(swidth)).Substring(0, iwidth));
                        }

                        if (iprecision > 0)
                        {
                            precision = int.Parse((new string(sprecision)).Substring(0, iprecision));
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
                            if (v[vindex] == null)
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
                                            temp.Insert(length_temp, ' ');
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
                                            temp.Insert(offset, '0');
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
                            if (v[vindex] == null)
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
                                    exponent = int.Parse(number_as_string.Substring(E_pos + 2));
                                }
                                else
                                {
                                    // No sign on exponent.
                                    exponent = int.Parse(number_as_string.Substring(E_pos + 1));
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
                                whole_number_string = number_as_string.Substring(0, point_pos);
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
                                    whole_number_string = number_as_string.Substring(0, point_pos);
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
                            if (v[vindex] == null)
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
                                    buffer.Append(temp.ToString().Substring(0, precision));
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
            IList<double> v = new List<double>();
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
            IList<float> v = new List<float>();
            v.Add(f);
            return formatString(v, format);
        }

        /// <summary>
        /// Format an Integer as a string. </summary>
        /// <returns> Formatted string. </returns>
        /// <param name="i"> An Integer to format. </param>
        /// <param name="format"> Format to use. </param>
        public static string formatString(int i, string format)
        {
            IList<int> v = new List<int>();
            v.Add(i);
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
                Convert.ToDouble(s.Trim());
                return true;
            }
            catch (System.FormatException)
            {
                return false;
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
        /// Given a string representation of a floating point number, round to the
        /// desired precision.  Currently this operates on a string (and not a double)
        /// because the method is called from the formatString() method that operates on strings. </summary>
        /// <returns> String representation of the rounded floating point number. </returns>
        /// <param name="string"> String containing a floating point number. </param>
        /// <param name="precision"> Number of digits after the decimal point to round the number. </param>
        public static string round(string @string, int precision)
        {
            string new_string;
            long ltemp = 0;

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
                ltemp = (long)Math.Round((Convert.ToDouble(@string)), MidpointRounding.AwayFromZero);
                return ((new long?(ltemp)).ToString());
            }
            // If we get to here, we have more than a zero precision and need to
            // jump through some hoops.  First, create a new string that has the remainder...
            StringBuilder remainder_string = new StringBuilder(@string.Substring(dot_pos + 1));
            // Next insert a decimal point after the precision digits.
            remainder_string.Insert(precision, '.');
            // Now convert the string to a Double...
            double? dtemp = Convert.ToDouble(remainder_string.ToString());
            // Now round...
            ltemp = (long)Math.Round(dtemp.Value, MidpointRounding.AwayFromZero);
            // Now convert back to a string...
            string rounded_remainder = (new long?(ltemp)).ToString();
            string integer_string = @string.Substring(0, integer_length);
            if (rounded_remainder.Length < precision)
            {
                // The number we were working with had leading zeros and we
                // lost that during the round.  Insert zeros again...
                StringBuilder buf = new StringBuilder(rounded_remainder);
                int number_to_add = precision - rounded_remainder.Length;
                for (int i = 0; i < number_to_add; i++)
                {
                    buf.Insert(0, '0');
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
            IList<string> v = new List<string>();
            for (int i = 0; i < array_size; i++)
            {
                v.Add(array[i]);
            }
            return v;
        }
    }
}
