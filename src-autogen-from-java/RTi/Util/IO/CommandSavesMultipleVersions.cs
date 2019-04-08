// CommandSavesMultipleVersions - implementation of this interface indicates that a command can save multiple versions

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
	/// <summary>
	/// Implementation of this interface indicates that a command can save multiple versions.  This is used
	/// in transitionary code, for example to handle the older TSTool "TS Alias = " notation,
	/// which is being phased out in favor of simple parameter=value notation.
	/// </summary>
	public interface CommandSavesMultipleVersions
	{

	/// <summary>
	/// Return the string representation of the command, considering the major software version. </summary>
	/// <param name="parameters"> the command parameters </param>
	/// <param name="majorVersion"> the major version of the application/processor (e.g., version 10 for TSTool
	/// no longer uses "TS Alias = " notation. </param>
	string ToString(PropList parameters, int majorVersion);

	}

}