using System;
using System.Collections.Generic;

// CommandStatus - this class provides a collecting point for status information for initializing and processing a command

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
	/// This class provides a collecting point for status information for initializing and
	/// processing a command.  It is returned when a command implements CommandStatusProvider.
	/// </summary>
	public class CommandStatus : ICloneable
	{

	/// <summary>
	/// Command status for initialization phase.
	/// @uml.property  name="__initialization_status"
	/// @uml.associationEnd  multiplicity="(1 1)"
	/// </summary>
	private CommandStatusType __initialization_status = CommandStatusType.UNKNOWN;

	/// <summary>
	/// Command status for discovery phase.
	/// @uml.property  name="__discovery_status"
	/// @uml.associationEnd  multiplicity="(1 1)"
	/// </summary>
	private CommandStatusType __discovery_status = CommandStatusType.UNKNOWN;

	/// <summary>
	/// Command status for run phase.
	/// @uml.property  name="__run_status"
	/// @uml.associationEnd  multiplicity="(1 1)"
	/// </summary>
	private CommandStatusType __run_status = CommandStatusType.UNKNOWN;

	/// <summary>
	/// A list of CommandLogRecord instances, indicating problems with initializing a command, guaranteed to be non-null.
	/// @uml.property  name="__initialization_log_Vector"
	/// @uml.associationEnd  multiplicity="(0 -1)" elementType="RTi.Util.IO.CommandLogRecord"
	/// </summary>
	private IList<CommandLogRecord> __initializationLogList = new List<CommandLogRecord>();

	/// <summary>
	/// A list of CommandLogRecord instances, indicating problems with running the discovery phase of a command, guaranteed to be non-null.
	/// @uml.property  name="__discovery_log_Vector"
	/// @uml.associationEnd  multiplicity="(0 -1)" elementType="RTi.Util.IO.CommandLogRecord"
	/// </summary>
	private IList<CommandLogRecord> __discoveryLogList = new List<CommandLogRecord>();

	/// <summary>
	/// A list of CommandLogRecord instances, indicating problems with running the command, guaranteed to be non-null.
	/// @uml.property  name="__run_log_Vector"
	/// @uml.associationEnd  multiplicity="(0 -1)" elementType="RTi.Util.IO.CommandLogRecord"
	/// </summary>
	private IList<CommandLogRecord> __runLogList = new List<CommandLogRecord>();

	/// <summary>
	/// Constructor that initializes the status for each phase to UNKNOWN.
	/// </summary>
	public CommandStatus()
	{
		// Default status is initialized above.
	}

	/// <summary>
	/// Add a CommandLogRecord for the command and reset the status for the phase
	/// to the most serious based on the log messages for the phase. </summary>
	/// <param name="phase"> Phase of running a command (see CommandPhaseType). </param>
	/// <param name="record"> CommandLogRecord indicating a problem running a command. </param>
	public virtual void addToLog(CommandPhaseType phase, CommandLogRecord record)
	{
		if (phase == CommandPhaseType.INITIALIZATION)
		{
			__initialization_status = CommandStatusType.maxSeverity(__initialization_status, record.getSeverity());
			__initializationLogList.Add(record);
		}
		else if (phase == CommandPhaseType.DISCOVERY)
		{
			__discovery_status = CommandStatusType.maxSeverity(__discovery_status, record.getSeverity());
			__discoveryLogList.Add(record);
		}
		else if (phase == CommandPhaseType.RUN)
		{
			__run_status = CommandStatusType.maxSeverity(__run_status, record.getSeverity());
			__runLogList.Add(record);
		}
	}

	/// <summary>
	/// Clear the CommandLogRecord for the command. </summary>
	/// <param name="phase"> Phase of running a command (see CommandPhaseType) or null to clear logs for all phases. </param>
	public virtual void clearLog(CommandPhaseType phase)
	{
		if ((phase == CommandPhaseType.INITIALIZATION) || (phase == null))
		{
			__initializationLogList.Clear();
			__initialization_status = CommandStatusType.UNKNOWN;
		}
		else if ((phase == CommandPhaseType.DISCOVERY) || (phase == null))
		{
			__discoveryLogList.Clear();
			__discovery_status = CommandStatusType.UNKNOWN;
		}
		else if ((phase == CommandPhaseType.RUN) || (phase == null))
		{
			__runLogList.Clear();
			__run_status = CommandStatusType.UNKNOWN;
		}
	}

	/// <summary>
	/// Clone the instance.  All command data are cloned, including the log records.
	/// </summary>
	public virtual object clone()
	{
		try
		{
			CommandStatus status = (CommandStatus)base.clone();
			// Copy the status information...
			status.__initialization_status = __initialization_status;
			status.__discovery_status = __discovery_status;
			status.__run_status = __run_status;
			// Clone the logs...
			status.__initializationLogList = new List<CommandLogRecord>();
			int size = __initializationLogList.Count;
			for (int i = 0; i < size; i++)
			{
				status.__initializationLogList.Add((CommandLogRecord)((CommandLogRecord)__initializationLogList[i]).clone());
			}
			status.__discoveryLogList = new List<CommandLogRecord>();
			size = __discoveryLogList.Count;
			for (int i = 0; i < size; i++)
			{
				status.__discoveryLogList.Add((CommandLogRecord)((CommandLogRecord)__discoveryLogList[i]).clone());
			}
			status.__runLogList = new List<CommandLogRecord>();
			size = __runLogList.Count;
			for (int i = 0; i < size; i++)
			{
				status.__runLogList.Add((CommandLogRecord)((CommandLogRecord)__runLogList[i]).clone());
			}
			return status;
		}
		catch (CloneNotSupportedException)
		{
			// Should not happen because everything is cloneable.
			throw new InternalError();
		}
	}

	/// <summary>
	/// Return the status for a phase of command processing.
	/// </summary>
	public virtual CommandStatusType getCommandStatus(CommandPhaseType phase)
	{
		if (phase == CommandPhaseType.INITIALIZATION)
		{
			return __initialization_status;
		}
		else if (phase == CommandPhaseType.DISCOVERY)
		{
			return __discovery_status;
		}
		else if (phase == CommandPhaseType.RUN)
		{
			return __run_status;
		}
		else
		{ // This should never happen.
			return CommandStatusType.UNKNOWN;
		}
	}

	/// <summary>
	/// Returns the command log for the specified phase, guaranteed to be non-null. </summary>
	/// <param name="phase"> - see CommandPhaseType. </param>
	/// <returns> command log as a list of CommandLogRecord </returns>
	public virtual IList<CommandLogRecord> getCommandLog(CommandPhaseType phase)
	{
		if (phase == CommandPhaseType.INITIALIZATION)
		{
			return __initializationLogList;
		}
		else if (phase == CommandPhaseType.DISCOVERY)
		{
			return __discoveryLogList;
		}
		else if (phase == CommandPhaseType.RUN)
		{
			return __runLogList;
		}
		else
		{
			// Return all records
			IList<CommandLogRecord> v = new List<CommandLogRecord>();
			((IList<CommandLogRecord>)v).AddRange(__initializationLogList);
			((IList<CommandLogRecord>)v).AddRange(__discoveryLogList);
			((IList<CommandLogRecord>)v).AddRange(__runLogList);
			return v;
		}
	}

	/// <summary>
	/// Returns the command log for the specified phases and status types, guaranteed to be non-null. </summary>
	/// <param name="phases"> array of CommandPhaseType to filter log records, or null to return all. </param>
	/// <param name="statuses"> array of CommandStatusType to filter log records, or null to return all. </param>
	/// <returns> command log as a list of CommandLogRecord </returns>
	public virtual IList<CommandLogRecord> getCommandLog(CommandPhaseType[] phases, CommandStatusType[] statuses)
	{
		if (phases == null)
		{
			phases = new CommandPhaseType[3];
			phases[0] = CommandPhaseType.INITIALIZATION;
			phases[1] = CommandPhaseType.DISCOVERY;
			phases[2] = CommandPhaseType.RUN;
		}
		if (statuses == null)
		{
			statuses = new CommandStatusType[4];
			statuses[0] = CommandStatusType.INFO;
			statuses[1] = CommandStatusType.SUCCESS;
			statuses[2] = CommandStatusType.FAILURE;
			statuses[3] = CommandStatusType.WARNING;
		}
		IList<CommandLogRecord> logList = new List<CommandLogRecord>();
		int j;
		IList<CommandLogRecord> logList2 = null;
		for (int i = 0; i < phases.Length; i++)
		{
			if (phases[i] == CommandPhaseType.INITIALIZATION)
			{
				logList2 = __initializationLogList;
			}
			else if (phases[i] == CommandPhaseType.DISCOVERY)
			{
				logList2 = __discoveryLogList;
			}
			else if (phases[i] == CommandPhaseType.RUN)
			{
				logList2 = __runLogList;
			}
			if (logList2 != null)
			{
				foreach (CommandLogRecord log in logList2)
				{
					for (j = 0; j < statuses.Length; j++)
					{
						if (log.getSeverity() == statuses[j])
						{
							logList.Add(log);
							break;
						}
					}
				}
			}
		}
		return logList;
	}

	/// <summary>
	/// Refresh the command status for a phase.  This should normally only be called when
	/// initializing a status or setting to success.  Otherwise, addToLog() should be
	/// used and the status determined from the CommandLogRecord status values. </summary>
	/// <param name="phase"> Command phase </param>
	/// <param name="severity_if_unknown"> The severity to set for the phase if it is currently
	/// unknown.  For example, specify as CommandStatusType.SUCCESS to override the
	/// initial CommandStatusType.UNKNOWN value. </param>
	public virtual void refreshPhaseSeverity(CommandPhaseType phase, CommandStatusType severity_if_unknown)
	{
		if (phase == CommandPhaseType.INITIALIZATION)
		{
			if (__initialization_status.Equals(CommandStatusType.UNKNOWN))
			{
				__initialization_status = severity_if_unknown;
			}
		}
		else if (phase == CommandPhaseType.DISCOVERY)
		{
			if (__discovery_status.Equals(CommandStatusType.UNKNOWN))
			{
				__discovery_status = severity_if_unknown;
			}
		}
		else if (phase == CommandPhaseType.RUN)
		{
			if (__run_status.Equals(CommandStatusType.UNKNOWN))
			{
				__run_status = severity_if_unknown;
			}
		}
	}

	/// <summary>
	/// Indicate whether the severity is greater than or equal to a provided severity. </summary>
	/// <param name="phase"> to check, or null to check against all phases. </param>
	/// <param name="severity"> A severity (e.g., CommandStatusType.WARNING) to check. </param>
	/// <returns> true if the maximum command status severity is >= the provided severity. </returns>
	public virtual bool severityGreaterThanOrEqualTo(CommandPhaseType phase, CommandStatusType severity)
	{
		if (phase == null)
		{
			// Check maximum of all phases.
			return CommandStatusUtil.getHighestSeverity(this).greaterThanOrEqualTo(severity);
		}
		else if (phase == CommandPhaseType.INITIALIZATION)
		{
			return __initialization_status.greaterThanOrEqualTo(severity);
		}
		else if (phase == CommandPhaseType.DISCOVERY)
		{
			return __discovery_status.greaterThanOrEqualTo(severity);
		}
		else if (phase == CommandPhaseType.RUN)
		{
			return __run_status.greaterThanOrEqualTo(severity);
		}
		else
		{ // This should never happen.
			return false;
		}
	}

	/// <summary>
	/// Convert the command status to a String, for simple viewing.
	/// </summary>
	public override string ToString()
	{
		return CommandStatusUtil.getCommandLogText(this);
	}

	}

}