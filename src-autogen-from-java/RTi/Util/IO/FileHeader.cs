using System.Collections.Generic;

// FileHeader - use with IO.getFileHeader

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
// FileHeader - use with IO.getFileHeader
// ----------------------------------------------------------------------------
// History:
//
// Jun 1997	Steven A. Malers, RTi	Port from C.
// 14 Mar 1998	SAM, RTi		Add javadoc.
// 2005-04-26	J. Thomas Sapienza, RTi	Added finalize().
// ----------------------------------------------------------------------------

namespace RTi.Util.IO
{

	/// <summary>
	/// This class is used by the IO.getFileHeader method when processing input/output
	/// file headers.  The file header consists of a list of comment strings and
	/// integers indicating the first and last revisions in the header.
	/// </summary>
	public class FileHeader
	{

	private IList<string> _header;
	private int _header_first;
	private int _header_last;

	/// <summary>
	/// Default constructor.
	/// </summary>
	public FileHeader()
	{
		_header = new List<string>(10,5);
		_header_first = 0;
		_header_last = 0;
	}

	/// <summary>
	/// Add a string to the header. </summary>
	/// <param name="o"> String to add to header. </param>
	public virtual int addElement(string s)
	{
		_header.Add(s);
		return 0;
	}

	/// <summary>
	/// Return the string at index "i" (zero-referenced). </summary>
	/// <returns> The string at index "i". </returns>
	public virtual object elementAt(int i)
	{
		return _header[i];
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~FileHeader()
	{
		_header = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <returns> The first header revision (the smallest number indicating the oldest
	/// revision). </returns>
	public virtual int getHeaderFirst()
	{
		return _header_first;
	}

	/// <returns> The last header revision (the largest number indicating the most recent
	/// revision). </returns>
	public virtual int getHeaderLast()
	{
		return _header_last;
	}

	/// <summary>
	/// Set the first header revision number. </summary>
	/// <param name="header_first"> The first header revision number. </param>
	public virtual int setHeaderFirst(int header_first)
	{
		_header_first = header_first;
		return 0;
	}

	/// <summary>
	/// Set the last header revision number. </summary>
	/// <param name="header_last"> The last header revision number. </param>
	public virtual int setHeaderLast(int header_last)
	{
		_header_last = header_last;
		return 0;
	}

	/// <returns> The number of strings in the header. </returns>
	public virtual int size()
	{
		return _header.Count;
	}

	}

}