// WarningCount - simple count of warnings, intended to be used with utility code that may generate warnings.

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
	/// Simple count of warnings, intended to be used with utility code that may generate warnings.
	/// Could use a MutableInteger class if that were available.
	/// @author sam
	/// 
	/// </summary>
	public class WarningCount : object
	{

	/// <summary>
	/// The number of warnings that are detected.
	/// </summary>
	private int __warningCount = 0;

	/// <summary>
	/// Constructor.
	/// </summary>
	public WarningCount() : base()
	{
	}

	/// <summary>
	/// Return the warning count.
	/// </summary>
	public virtual int getCount()
	{
		return __warningCount;
	}

	/// <summary>
	/// Increment the warning count by 1.
	/// </summary>
	public virtual int incrementCount()
	{
		++__warningCount;
		return __warningCount;
	}

	/// <summary>
	/// Increment the warning count. </summary>
	/// <param name="count"> the increment to the previous count </param>
	/// <returns> the count after incrementing </returns>
	public virtual int incrementCount(int count)
	{
		__warningCount += count;
		return __warningCount;
	}

	}

}