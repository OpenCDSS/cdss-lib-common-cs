using System;

// CommandProfile - this class provides profile information related to command execution.

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
	/// This class provides profile information related to command execution.  This information can be used
	/// to evaluate command processor performance and memory use.
	/// </summary>
	public class CommandProfile : ICloneable
	{

	/// <summary>
	/// Command start time in milliseconds (from Date).
	/// </summary>
	private long startTime = 0;

	/// <summary>
	/// Command end time in milliseconds (from Date).
	/// </summary>
	private long endTime = 0;

	/// <summary>
	/// Command start heap memory in bytes (from Date).
	/// </summary>
	private long startHeap = 0;

	/// <summary>
	/// Command end heap memory in bytes (from Date).
	/// </summary>
	private long endHeap = 0;

	/// <summary>
	/// Construct and initialize all profile values to zero.
	/// </summary>
	public CommandProfile()
	{
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="startTime"> starting milliseconds for command (specify 0 if unknown) </param>
	/// <param name="startHeap"> starting heap memory for command (specify 0 if unknown) </param>
	/// <param name="endTime"> ending milliseconds for command (specify 0 if unknown) </param>
	/// <param name="endHeap"> ending heap memory for command (specify 0 if unknown) </param>
	public CommandProfile(long startTime, long startHeap, long endTime, long endHeap)
	{
		this.startTime = startTime;
		this.startHeap = startHeap;
		this.endTime = endTime;
		this.endHeap = endHeap;
	}

	/// <summary>
	/// Clone the instance.  All command data are cloned.
	/// </summary>
	public virtual object clone()
	{
		try
		{
			CommandProfile profile = (CommandProfile)base.clone();
			return profile;
		}
		catch (CloneNotSupportedException)
		{
			// Should not happen because everything is cloneable.
			throw new InternalError();
		}
	}

	/// <summary>
	/// Return the heap memory at the end of the command execution, in bytes. </summary>
	/// <returns> the heap memory at the end of the command execution, in bytes </returns>
	public virtual long getEndHeap()
	{
		return this.endHeap;
	}

	/// <summary>
	/// Return the time in milliseconds (from 1970) at the end of the command execution. </summary>
	/// <returns> the time in milliseconds (from 1970 ) at the end of the command execution </returns>
	public virtual long getEndTime()
	{
		return this.endTime;
	}

	/// <summary>
	/// Return the run time in milliseconds (from 1970), computed as the end time minus the start time, or zero if
	/// the command has no end time. </summary>
	/// <returns> the run time in milliseconds (from 1970 ) </returns>
	public virtual long getRunTime()
	{
		if (this.endTime == 0)
		{
			return 0;
		}
		else
		{
			return (this.endTime - this.startTime);
		}
	}

	/// <summary>
	/// Return the heap memory at the start of the command execution, in bytes. </summary>
	/// <returns> the heap memory at the start of the command execution, in bytes </returns>
	public virtual long getStartHeap()
	{
		return this.startHeap;
	}

	/// <summary>
	/// Return the time in milliseconds (from 1970) at the start of the command execution. </summary>
	/// <returns> the time in milliseconds (from 1970 ) at the start of the command execution </returns>
	public virtual long getStartTime()
	{
		return this.startTime;
	}

	/// <summary>
	/// Set the heap memory in bytes at the end of the command execution. </summary>
	/// <param name="endheap"> the heap memory in bytes at the end of the command execution </param>
	public virtual void setEndHeap(long endHeap)
	{
		this.endHeap = endHeap;
	}

	/// <summary>
	/// Set the time in milliseconds (from 1970) at the end of the command execution. </summary>
	/// <param name="endTime"> the time in milliseconds (from 1970 ) at the end of the command execution </param>
	public virtual void setEndTime(long endTime)
	{
		this.endTime = endTime;
	}

	/// <summary>
	/// Set the heap memory in bytes at the end of the command execution. </summary>
	/// <param name="endheap"> the heap memory in bytes at the end of the command execution </param>
	public virtual void setStartHeap(long startHeap)
	{
		this.startHeap = startHeap;
	}

	/// <summary>
	/// Set the time in milliseconds (from 1970) at the start of the command execution. </summary>
	/// <param name="startTime"> the time in milliseconds (from 1970 ) at the start of the command execution </param>
	public virtual void setStartTime(long startTime)
	{
		this.startTime = startTime;
	}

	/// <summary>
	/// Return a string representation of the problem, suitable for display in logging, etc.
	/// </summary>
	public override string ToString()
	{
		return "Runtime " + this.startTime + "/" + this.endTime + "/" + (this.endTime - this.startTime) +
		" Heap " + this.startHeap + "/" + this.endHeap + "/" + (this.endHeap - this.startHeap);
	}

	}

}