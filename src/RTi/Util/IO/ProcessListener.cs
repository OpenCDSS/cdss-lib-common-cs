// ProcessListener - listener for process events

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
// ProcessListener - listener for process events
// ----------------------------------------------------------------------------
// History:
//
// 2002-10-16	Steven A. Malers, RTi	Implemented code.
// ----------------------------------------------------------------------------

namespace RTi.Util.IO
{
	/// <summary>
	/// This ProcessListener interface can be implemented by classes that need to
	/// listen for output from an external process, as controlled by the ProcessManager.
	/// This interface is used by the ProcessManagerJDialog to retrieve process output
	/// as it is generated. 
	/// </summary>
	public abstract interface ProcessListener
	{
	/// <summary>
	/// ProcessManager will call this method if a line from standard output is read. </summary>
	/// <param name="output"> A line from the process' standard output. </param>
	void processOutput(string output);

	/// <summary>
	/// ProcessManager will call this method if a line from standard error is read. </summary>
	/// <param name="error"> A line from the process' standard error. </param>
	void processError(string error);

	/// <summary>
	/// ProcessManager will call this method when the status of the process changes. </summary>
	/// <param name="code"> If zero, then a normal exit has occurred.  If not zero, assume that
	/// the process has terminated with an error.  At some point, need to consider how
	/// to handle pause, interrupt, etc. </param>
	/// <param name="message"> A string message that can be displayed so that calling code
	/// does not need to interpret the numeric code. </param>
	void processStatus(int code, string message);

	}

}