// Status - encapsulates a message and a level

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

/// <summary>
///***************************************************************************
/// Status.java		2007-03-22
/// ******************************************************************************  
/// Revisions
/// 2007-03-21	Ian Schneider, RTi		Initial version.
/// 2007-03-27	Kurt Tometich, RTi		Added some javadoc.
/// ****************************************************************************
/// </summary>
namespace RTi.Util.IO
{
	/// <summary>
	/// Status encapsulates a message and a level.  Can be used to log
	/// various levels of errors or warnings.  It usually reflects the Status of
	/// a data check or data validation rule, but can be used for other situations
	/// that require some type of Status.
	/// 
	/// </summary>
	public sealed class Status
	{

		public const int ERROR = 0;
		public const int WARNING = 1;
		public const int OK = 2;

		// A Status object with status of OK
		public static readonly Status OKAY = new Status("OK",OK);

		// Message to add to status is it fails
		private readonly string message;
		// Level to log the failure as
		private readonly int level;

		/// <summary>
		/// Initializes the Status object with its message and
		/// level. </summary>
		/// <param name="message"> The message to add if this test fails. </param>
		/// <param name="level"> The level to log the failure as. </param>
		private Status(string message, int level)
		{
			this.message = message;
			this.level = level;
		}

		/// <summary>
		/// Returns the current level. </summary>
		/// <returns> The level of error logging. </returns>
		public int getLevel()
		{
			return level;
		}

		/// <summary>
		/// Returns a new Status object with the given message and level.
		/// This should be used when there is a failure to return a status
		/// object that details the failure. </summary>
		/// <param name="message"> Message to add to the Status object (i.e. "Object
		/// must not be null").
		/// param level The level to the log the message as. </param>
		/// <returns> Status object. </returns>
		public static Status status(string message, int level)
		{
			return new Status(message,level);
		}

		/// <summary>
		/// Helper method that logs and error with the given message
		/// and returns the Status object. </summary>
		/// <param name="message"> The message to log for this error. </param>
		/// <returns> Status object. </returns>
		public static Status error(string message)
		{
			return status(message,ERROR);
		}

		/// <summary>
		/// Helper method that logs a warning with the given message
		/// and returns the Status object. </summary>
		/// <param name="message"> The message to log for this warning. </param>
		/// <returns> Status object. </returns>
		public static Status warning(string message)
		{
			return status(message,WARNING);
		}

		/// <summary>
		/// Overrides the toString method in Object and returns
		/// the message for this Status object. </summary>
		/// <returns> The message for this Status object. </returns>
		public override string ToString()
		{
			return message;
		}

	}

}