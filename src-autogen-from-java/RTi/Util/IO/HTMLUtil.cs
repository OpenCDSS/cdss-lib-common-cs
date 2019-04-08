using System;
using System.Text;

// HTMLUtil - this class convert a text in an HTML text format with symbolic code (&xxxx;)

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
	//package com.jcorporate.expresso.core.misc;

	/*
	 * HtmlUtil.java
	 *
	 * Copyright 1999, 2002, 2002 Yves Henri AMAIZO.
	 *                            amy_amaizo@compuserve.com
	 */

	/// <summary>
	/// This class convert a text in an HTML text format with symbolic code (&xxxx;),
	/// it also convert a given HTML text format which contain symbolic code to text.
	/// 
	/// @version        $Revision: 3 $  $Date: 2006-03-01 03:17:08 -0800 (Wed, 01 Mar 2006) $
	/// @author        Yves Henri AMAIZO
	/// </summary>
	public class HTMLUtil
	{
		/// <summary>
		/// Method text2html: Convert a text to an HTML format.
		/// </summary>
		/// <param name="text">:     The original text string </param>
		/// <param name="includeWrapper"> if true, include &lt;html&gt; wrapper tags around the HTML.  If false, convert the string
		/// encoding to HTML to deal with special characters, but do not wrap the string with the tags. </param>
		/// <returns>          The converted HTML text including symbolic codes string </returns>
		public static string text2html(string text, bool includeWrapper)
		{
			if (string.ReferenceEquals(text, null))
			{
				return text;
			}

			StringBuilder t = new StringBuilder(text.Length + 10); // 10 is just a test value, could be anything, should affect performance

			if (includeWrapper)
			{
				t.Append("<html>");
			}

			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				// Check for non ISO8859-1 characters
				int pos = (int)c;
				//Message.printStatus(2, "", "Position for " + c + " is " + pos );
				if (pos < symbolicCode.Length)
				{
					// Character is within the lookup table
					string sc = symbolicCode[pos];
					//Message.printStatus(2, "", "Translated character is " + sc );
					if ("".Equals(sc))
					{
						// Character does not need to be converted so just append
						t = t.Append(c);
					}
					else
					{
						// Character was converted to encoded representation
						t = t.Append(sc);
					}
				}
				else
				{
					// Not in the lookup table so just append
					t = t.Append(c);
				}
			}
			if (includeWrapper)
			{
				t.Append("</html>");
			}
			return t.ToString();
		}

		/// <summary>
		/// Method html2text: Convert an HTML text format to a normal text format.
		/// </summary>
		/// <param name="text">:     The original HTML text string </param>
		/// <returns>          The converted text without symbolic codes string </returns>
		public static string html2text(string text)
		{
			if (string.ReferenceEquals(text, null))
			{
				return text;
			}
			StringBuilder t = new StringBuilder(text.Length);
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				if (c == '&')
				{
					string code = c.ToString();
					do
					{
						if (++i >= text.Length)
						{
							break;
						}
						if (text[i] == '&')
						{
							i--;
							break;
						}
						code += text[i];
					} while (text[i] != ';');
					int index = Arrays.binarySearch(sortedSymbolicCode, new NumericSymbolicCode(code, 0));
					// Does the extracting code correspond to something ?
					if (index >= 0)
					{
						t = t.Append((char) sortedSymbolicCode[index].getNumericCode());
					}
					else
					{
						t = t.Append(code);
					}
				}
				else
				{
					t = t.Append(c);
				}
			}
			return t.ToString();
		}

		/// <summary>
		/// Array of symbolic code order by numeric code ! <br>
		/// The symbolic codes and their position correspond to the ISO 8859-1 set
		/// of char. The empty definitions mean that there is no symbolic codes for
		/// that character or this symbolic code is not used.
		/// </summary>
		private static readonly string[] symbolicCode = new string[] {"", "", "", "", "", "", "", "", "", "", "<br>", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "&#25;", "", "", "", "", "", "", "", "", "&quot;", "", "", "", "&amp;", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "&lt;", "", "&gt;", "", "&#64;", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "&#96;", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "&#128;", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "&#145;", "&#146;", "&#147;", "&#148;", "", "", "", "", "", "", "", "", "", "", "", "&nbsp;", "&iexcl;", "&cent;", "&pound;", "&curren;", "&yen;", "&brvbar;", "&sect;", "&uml;", "&copy;", "&ordf;", "&laquo;", "&not;", "&shy;", "&reg;", "&macr;", "&deg;", "&plusmn;", "&sup2;", "&sup3;", "&acute;", "&micro;", "&para", "&middot;", "&cedil;", "&supl;", "&ordm;", "&raquo;", "&frac14;", "&frac12;", "&frac34;", "&iquest;", "&Agrave;", "&Aacute;", "&Acirc;", "&Atilde;", "&Auml;", "&Aring;", "&AElig;", "&Ccedil;", "&Egrave;", "&Eacute;", "&Ecirc;", "&Euml;", "&Igrave;", "&Iacute;", "&Icirc;", "&Iuml;", "&ETH;", "&Ntilde;", "&Ograve;", "&Oacute;", "&Ocirc;", "&Otilde;", "&Ouml;", "&times;", "&Oslash;", "&Ugrave;", "&Uacute;", "&Ucirc;", "&Uuml;", "&Yacute;", "&THORN;", "&szlig;", "&agrave;", "&aacute;", "&acirc;", "&atilde;", "&auml;", "&aring;", "&aelig;", "&ccedil;", "&egrave;", "&eacute;", "&ecirc;", "&euml;", "&igrave;", "&iacute;", "&icirc;", "&iuml;", "&eth;", "&ntilde;", "&ograve", "&oacute;", "&ocirc;", "&otilde", "&ouml;", "&divide;", "&oslash;", "&ugrave;", "&uacute;", "&ucirc;", "&uuml;", "&yacute;", "&thorn;", "&yuml;"};

		/// <summary>
		/// Array of symbolic code order symbolic code !<br>
		/// This array is the reciprocal from the 'symbolicCode' array.
		/// </summary>
		private static NumericSymbolicCode[] sortedSymbolicCode = new NumericSymbolicCode[symbolicCode.Length];

		/// <summary>
		/// This class is the structure used for the 'sortedSymbolicCode' array.
		/// Each symbolic code string (sorted by alphabetical order) have its numerical
		/// corresponding code.<br>
		/// This class also implements the 'Comparable' interface to ease the sorting
		/// process in the initialisation bloc.
		/// </summary>
		private sealed class NumericSymbolicCode : IComparable<NumericSymbolicCode>
		{

			public NumericSymbolicCode(string symbolicCode, int numericCode)
			{
				this.symbolicCode = symbolicCode;
				this.numericCode = numericCode;
			}

			//public String getSymbolicCode() {
			//    return symbolicCode;
			//}

			public int getNumericCode()
			{
				return numericCode;
			}

			public int CompareTo(NumericSymbolicCode nsc)
			{
				return symbolicCode.CompareTo(nsc.symbolicCode);
			}

			internal string symbolicCode;
			internal int numericCode;
		}

		/// <summary>
		/// Initialization and sorting of the 'sortedSymbolicCode'
		/// </summary>
		static HTMLUtil()
		{
			for (int i = 0; i < symbolicCode.Length; i++)
			{
				sortedSymbolicCode[i] = new NumericSymbolicCode(symbolicCode[i], i);
			}
			Arrays.sort(sortedSymbolicCode);
		}
	}

}