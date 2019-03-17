// RegExpFilenameFilter - general purpose filename filter that supports regular expressions

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
// RegExpFilenameFilter - general purpose filename filter that supports
//			regular expressions
// ----------------------------------------------------------------------------
// Copyright RTi:  See the COPYRIGHT file.
// ----------------------------------------------------------------------------
// History:
//
// 23 Jun 1999	Steven A. Malers,	Initial version.  Finally got tired of
//		Riverside Technology,	lake of API.
//		inc.
// ----------------------------------------------------------------------------

namespace RTi.Util.IO
{

	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This class implements FilenameFilter for use with FileDialog.
	/// It supports UNIX-style wildcards in filenames, as follows:
	/// <para>
	/// <pre>
	/// .     Match one character.
	///     Match zero or more characters.
	/// [...] Match any one of the characters enclosed in the brackets.  This can be
	///      a list of characters or a character range separated by a -.
	/// </pre>
	/// </para>
	/// <para>
	/// To use with FileDialog, contruct this filter with an appropriate filename
	/// regular expression (usually *.ext, where ext is a desired file extension).
	/// Then specify the filter to the FileDialog.
	/// </para>
	/// </summary>
	public class RegExpFilenameFilter : FilenameFilter
	{

	private string _regexp = "";

	/// <summary>
	/// Basic constructor.  The main functionality is in the accept() method. </summary>
	/// <param name="regexp"> a regular expression. </param>
	public RegExpFilenameFilter(string regexp)
	{
		_regexp = regexp;
	}

	/// <returns> true if the name matches the regular expression passed in during construction. </returns>
	/// <param name="dir"> Directory of file being evaluated. </param>
	/// <param name="name"> File being evaluated (without leading directory path). </param>
	public virtual bool accept(File dir, string name)
	{
		return StringUtil.matchesRegExp(name, _regexp);
	}

	/// <summary>
	/// Finalize before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~RegExpFilenameFilter()
	{
		_regexp = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	} // End RegExpFilenameFilter class

}