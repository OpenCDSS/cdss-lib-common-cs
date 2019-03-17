// CommandListListener - interface for a listener for basic changes to Command lists

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
	/// This interface provides a listener for basic changes to Command lists.  It
	/// can be used, for example, to allow domain-specific classes to notify UI
	/// classes when commands have been added, removed, or changed, to allow appropriate
	/// display changes to occur.
	/// </summary>
	public interface CommandListListener
	{

	/// <summary>
	/// Indicate when one or more commands have been added. </summary>
	/// <param name="index0"> The index (0+) of the first command that is added. </param>
	/// <param name="index1"> The index (0+) of the last command that is added. </param>
	void commandAdded(int index0, int index1);

	/// <summary>
	/// Indicate when one or more commands have changed, for example in definition
	/// or status. </summary>
	/// <param name="index0"> The index (0+) of the first command that is changed. </param>
	/// <param name="index1"> The index (0+) of the last command that is changed. </param>
	void commandChanged(int index0, int index1);

	/// <summary>
	/// Indicate when one or more commands have been removed. </summary>
	/// <param name="index0"> The index (0+) of the first command that is removed. </param>
	/// <param name="index1"> The index (0+) of the last command that is removed. </param>
	void commandRemoved(int index0, int index1);

	}

}