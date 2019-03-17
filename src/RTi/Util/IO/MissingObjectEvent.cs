using System;

// MissingObjectEvent - this event describes missing object errors, for example when an object is requested but cannot be read.

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
	/// This event describes missing object errors, for example when an object is requested but cannot
	/// be read.
	/// @author sam
	/// 
	/// </summary>
	public class MissingObjectEvent : CommandProcessorEvent
	{

	/// <summary>
	/// Identifier of object that was missing.
	/// </summary>
	internal string __id = "";

	/// <summary>
	/// Class of object that was missing.
	/// </summary>
	internal Type __missingObjectClass = null;

	/// <summary>
	/// Human readable name for class of object that was missing.
	/// </summary>
	internal string __missingObjectClassName = "";

	/// <summary>
	/// Object that was the source/domain of the problem.
	/// </summary>
	internal object __resource = null;

	/// <summary>
	/// Construct an instance indicating that an object could not be read/created. </summary>
	/// <param name="missingObjectID"> unique identifier for the object. </param>
	/// <param name="resource"> the object that is the "domain" of the problem. </param>
	/// <param name="missingObjectClass"> the class that for the missing object. </param>
	/// <param name="missingObjectClassName"> the human-readable name of the class (e.g., "Time Series" as
	/// opposed to "RTi.TS.TS". </param>
	public MissingObjectEvent(string missingObjectID, Type missingObjectClass, string missingObjectClassName, object resource)
	{
		__id = missingObjectID;
		__resource = resource;
		__missingObjectClass = missingObjectClass;
		__missingObjectClassName = missingObjectClassName;
	}

	public virtual Type getMissingObjectClass()
	{
		return __missingObjectClass;
	}

	public virtual string getMissingObjectClassName()
	{
		return __missingObjectClassName;
	}

	public virtual string getMissingObjectID()
	{
		return __id;
	}

	public virtual object getResource()
	{
		return __resource;
	}

	}

}