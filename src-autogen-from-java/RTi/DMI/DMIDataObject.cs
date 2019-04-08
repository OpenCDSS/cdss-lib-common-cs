// DMIDataObject - base class for data objects

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

//-----------------------------------------------------------------------------
// DMIDataObject - base class for data objects
//-----------------------------------------------------------------------------
// History:
//
// 2002-06-25	Steven A. Malers, RTi	Initial version.
// 2004-01-19	SAM, RTi		Change setDirty(boolean) to
//					setDirty(boolean) to be more consistent
//					with other Java code.
// 2004-01-26	J. Thomas Sapienza, RTi	* Added _newRecord.
//					* Added _object.
// 2004-01-30	JTS, RTi		* Removed _newRecord.
//					* Renamed _object to _original.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//-----------------------------------------------------------------------------

namespace RTi.DMI
{
	/// <summary>
	/// The DMIDataObject class is the base class for objects managed with DMI classes.
	/// Currently the object only contains data/methods to handle the "dirty" flag and original copy of the data.
	/// </summary>
	public class DMIDataObject : object
	{

	/// <summary>
	/// Flag to indicate whether the data object has been modified.
	/// </summary>
	private bool _dirty;

	/// <summary>
	/// A separate object that can be stored by any data object.  One use is to keep
	/// a clone of the original version of a table record so that after the record is
	/// modified and needs to be rewritten, it can be compared to the original.
	/// </summary>
	private object _original = null;

	/// <summary>
	/// Constructor. 
	/// </summary>
	public DMIDataObject()
	{
		_dirty = false;
		_original = null;
	}

	/// <summary>
	/// Returns the object stored in this object. </summary>
	/// <returns> the object stored in this object. </returns>
	public virtual object getOriginal()
	{
		return _original;
	}

	/// <summary>
	/// Indicate whether the object is dirty (has been modified). </summary>
	/// <returns> true if the object is dirty (has been modified). </returns>
	public virtual bool isDirty()
	{
		return _dirty;
	}

	/// <summary>
	/// Set whether the object is dirty (has been modified).
	/// This method is mean to be called after the initial database read, indicating
	/// a change by an application. </summary>
	/// <param name="dirty"> true if the object has been modified after the read. </param>
	public virtual void setDirty(bool dirty)
	{
		_dirty = dirty;
	}

	/// <summary>
	/// Sets the original version of this record.
	/// </summary>
	public virtual void setOriginal(object original)
	{
		_original = original;
	}

	}

}