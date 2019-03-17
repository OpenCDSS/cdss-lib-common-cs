// MessageUtil - Utility code for the Message package

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

//------------------------------------------------------------------------------
// RTi.Util.Message.MessageUtil - Utility code for the Message package
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 2005-04-29	Steven A. Malers, RTi	Initialize code with the
//					formatMessageTag() method.
//------------------------------------------------------------------------------
// EndHeader

namespace RTi.Util.Message
{
	/// <summary>
	/// This class provides useful static methods to support the Message package.
	/// </summary>
	public abstract class MessageUtil
	{

	/// <summary>
	/// Format a standard message tag string, for use with command-oriented programs.
	/// The format of the string will be:
	/// <pre>
	/// root,count
	/// </pre> </summary>
	/// <param name="tag_root"> A root string to include in the tag. </param>
	/// <param name="count"> A count to modify the root (1+), for example, indicating the
	/// count of warnings within a command. </param>
	/// <returns> a formatted message tag, or an empty string if root is null. </returns>
	public static string formatMessageTag(string root, int count)
	{
		if ((string.ReferenceEquals(root, null)) || (root.Length == 0))
		{
			return "";
		}
		else
		{
			return root + "," + count;
		}
	}

	}

}