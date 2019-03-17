using System;

// CommandStatusType - this class provides an enumeration of possible command status values.  

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

	// TODO SAM 2016-02-24 Need to convert to enumeration
	/// <summary>
	/// This class provides an enumeration of possible command status values.  
	/// </summary>
	public class CommandStatusType
	{

		/// <summary>
		/// UNKNOWN indicates that the command could not be executed (no results). 
		/// </summary>
		public static CommandStatusType UNKNOWN = new CommandStatusType(-1, "UNKNOWN");

		/// <summary>
		/// When used with Command processing, INFO indicates information relevant to a command, perhaps
		/// to explain a warning that might come up later.
		/// </summary>
		public static CommandStatusType INFO = new CommandStatusType(-2, "INFO");

		/// <summary>
		/// When used with Command processing, SUCCESS indicates that results could be generated, with no warnings.
		/// </summary>
		public static CommandStatusType SUCCESS = new CommandStatusType(0, "SUCCESS");

		/// <summary>
		/// WARNING indicates that partial results were generated, but which may be in
		/// error due to initialization or runtime errors.
		/// </summary>
		public static CommandStatusType WARNING = new CommandStatusType(1, "WARNING");

		/// <summary>
		/// FAILURE indicates that the command could not be executed (no results). 
		/// </summary>
		public static CommandStatusType FAILURE = new CommandStatusType(2, "FAILURE");

		/// <summary>
		/// Used to set severity.
		/// @uml.property  name="__type"
		/// </summary>
		private int __type;
		/// <summary>
		/// Type name, e.g., "SUCCESS", "FAILURE".
		/// @uml.property  name="__typename"
		/// </summary>
		private string __typename;

		/// <summary>
		/// Construct the status type using the type/severity and name.  It is
		/// private because other code should use the predefined instances. </summary>
		/// <param name="type"> </param>
		/// <param name="typename"> </param>
		private CommandStatusType(int type, string typename)
		{
			__type = type;
			__typename = typename;
		}

		/// <summary>
		/// Determine if two types are equal.
		/// </summary>
		public virtual bool Equals(CommandStatusType type)
		{
			if (__type == type.getSeverity())
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Return the severity of the status (larger number means more severe problem).
		/// This is useful for ranking the severity of problems for output. </summary>
		/// <returns> the severity of the problem. </returns>
		public virtual int getSeverity()
		{
			return __type;
		}

		/// <summary>
		/// Return the severity of the status (larger number means more severe problem). </summary>
		/// <returns> the severity of the problem. </returns>
		/// @deprecated Use getSeverity(). 
		public virtual int getPriority()
		{
			return getSeverity();
		}

		/// <summary>
		/// Determine if a status severity is greater than the current status.
		/// For example, use this to check whether a command status type is greater than
		/// CommandStatusType.SUCCESS. </summary>
		/// <param name="type"> Command status severity. </param>
		/// <returns> true if the provided severity is greater than that of the instance. </returns>
		public virtual bool greaterThan(CommandStatusType type)
		{
			if (__type > type.getSeverity())
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Determine if a status severity is greater than or equal to the current status.
		/// For example, use this to check whether a command status type is greater than
		/// or equal to CommandStatusType.WARNING. </summary>
		/// <param name="type"> Command status severity. </param>
		/// <returns> true if the provided severity is greater than that of the instance. </returns>
		public virtual bool greaterThanOrEqualTo(CommandStatusType type)
		{
			if (__type >= type.getSeverity())
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Determine the maximum severity. </summary>
		/// <returns> the status that is the most severe from the two status. </returns>
		public static CommandStatusType maxSeverity(CommandStatusType status1, CommandStatusType status2)
		{
			int severity1 = status1.getSeverity();
			int severity2 = status2.getSeverity();
			if (severity1 > severity2)
			{
				return status1;
			}
			else
			{
				return status2;
			}
		}

		/// <summary>
		/// Parse the command status type and return an instance of the enumeration. </summary>
		/// <param name="cst"> CommandStatusType string to parse. </param>
		/// <returns> an instance of the enumeration that matches the string. </returns>
		/// <exception cref="InvalidParameterException"> if the requested string does not match a command status type. </exception>
		public static CommandStatusType parse(string cst)
		{
			if (cst.Equals(UNKNOWN.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				return UNKNOWN;
			}
			else if (cst.Equals(INFO.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				return INFO;
			}
			else if (cst.Equals(SUCCESS.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				return SUCCESS;
			}
			else if (cst.Equals(WARNING.ToString(), StringComparison.OrdinalIgnoreCase) || cst.Equals("warn", StringComparison.OrdinalIgnoreCase))
			{
				return WARNING;
			}
			else if (cst.Equals(FAILURE.ToString(), StringComparison.OrdinalIgnoreCase) || cst.Equals("fail", StringComparison.OrdinalIgnoreCase))
			{
				return FAILURE;
			}
			else
			{
				throw new InvalidParameterException("The command status type \"" + cst + "\" is not a recognized type.");
			}
		}

		/// <summary>
		/// Return a String representation of the command status, as follows:
		/// <pre>
		/// INFO - informational message
		/// UNKNOWN - status is unknown (not implemented or not initialized)
		/// SUCCESS - command was successful (no WARNING or FAILURE)
		/// WARNING - command completed but user should review possible problem
		/// FAILURE - command failed and results are very likely not complete or accurate
		/// </pre>
		/// </summary>
		public override string ToString()
		{
			return __typename;
		}
	}

}