using System;

// CommandLogRecord - this class provides a single record of logging as managed by the CommandStatus class

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
	// TODO SAM 2007-06-25 Can a link to full logging be implemented somehow to allow
	// drill-down to the log file (with an appropriate filter/navigation)?
	/// <summary>
	/// This class provides a single record of logging as managed by the CommandStatus class.
	/// It is meant only to track a problem and recommend a solution.  Consequently,
	/// the status severity should normally be WARNING or FAILURE, with no other
	/// log records tracked for status purposes.
	/// </summary>
	public class CommandLogRecord : ICloneable
	{

	/// <summary>
	/// CommandStatusProvider that generates this log (currently null unless extracted by other code from
	/// the log list.
	/// </summary>
	private CommandStatusProvider __commandStatusProvider = null;

	/// <summary>
	/// Log type/severity level.
	/// </summary>
	private CommandStatusType __severity = null;

	/// <summary>
	/// Type of problem that has been identified.
	/// This is used, for example, for a log record report to show categories of problems.
	/// </summary>
	private string __type = null;

	/// <summary>
	/// Problem that has been identified.
	/// @uml.property  name="__problem"
	/// </summary>
	private string __problem = null;

	/// <summary>
	/// Recommended solution.
	/// @uml.property  name="__recommendation"
	/// </summary>
	private string __recommendation = null;

	/// <summary>
	/// Constructor for a command log record. </summary>
	/// <param name="severity"> Severity for the log record, from CommandStatusType. </param>
	/// <param name="problem"> A String describing the problem. </param>
	/// <param name="recommendation"> A String recommending a solution. </param>
	public CommandLogRecord(CommandStatusType severity, string problem, string recommendation) : this(severity, "", problem, recommendation)
	{
	}

	/// <summary>
	/// Constructor for a command log record. </summary>
	/// <param name="severity"> Severity for the log record, from CommandStatusType. </param>
	/// <param name="type"> the log record type. </param>
	/// <param name="problem"> A String describing the problem. </param>
	/// <param name="recommendation"> A String recommending a solution. </param>
	public CommandLogRecord(CommandStatusType severity, string type, string problem, string recommendation)
	{
		__severity = severity;
		__type = type;
		__problem = problem;
		__recommendation = recommendation;
	}

	/// <summary>
	/// Details that can be used to troubleshoot and link to other information.
	/// TODO SAM 2007-06-25 Need to flush out the details.  For example, this could
	/// be a list of the parameter/value pairs.  It is more difficult to define the
	/// properties when a run-time error with dynamic data.
	/// 
	/// private PropList __details_PropList = null;
	/// </summary>
	/// <summary>
	/// Return the status for a phase of processing.
	/// 
	/// public CommandLogRecord ( String problem, String recommendation, PropList details )
	/// {
	/// setProblem ( problem );
	/// setRecommendation ( recommendation );
	/// setDetails ( details );
	/// }
	/// </summary>

	/// <summary>
	/// Clone the instance.  All command data are cloned.
	/// </summary>
	public virtual object clone()
	{
		try
		{
			CommandLogRecord record = (CommandLogRecord)base.clone();
			// The problem and recommendation are automatically copied.
			// Copy the severity...
			record.__severity = __severity;
			return record;
		}
		catch (CloneNotSupportedException)
		{
			// Should not happen because everything is cloneable.
			throw new InternalError();
		}
	}

	/// <summary>
	/// Get the log record command status provider. </summary>
	/// <returns> the log record command status provider. </returns>
	public virtual CommandStatusProvider getCommandStatusProvider()
	{
	  return __commandStatusProvider;
	}

	/// <summary>
	/// Get the log record problem.
	/// </summary>
	/// <returns> the problem string </returns>
	public virtual string getProblem()
	{
	  return __problem;
	}

	/// <summary>
	/// Get the log record recommendation.
	/// </summary>
	/// <returns> recommendation string </returns>
	public virtual string getRecommendation()
	{
	  return __recommendation;
	}

	/// <summary>
	/// Get the log record type.
	/// </summary>
	/// <returns> type string </returns>
	public virtual string getType()
	{
	  return __type;
	}

	/// <summary>
	/// Get the severity associated with a log record.
	/// </summary>
	public virtual CommandStatusType getSeverity()
	{
		return __severity;
	}

	/// <summary>
	/// Set the command status provider for this record (e.g., the Command that generated the record).
	/// Currently this is not in the constructor and is typically set with CommandStatusUtil.getLogRecordList().
	/// Make it protected to handle internally in this package for now.
	/// </summary>
	protected internal virtual void setCommandStatusProvider(CommandStatusProvider csp)
	{
		__commandStatusProvider = csp;
	}

	/// <summary>
	/// Set the details describing the problem.
	/// 
	/// public void setDetails ( PropList details )
	/// {	__details_PropList = details;
	/// }
	/// </summary>

	/// <summary>
	/// Set the description of the problem that was identified.
	/// </summary>
	public virtual void setProblem(string problem)
	{
		__problem = problem;
	}

	/// <summary>
	/// Set the recommendation to resolve the problem.
	/// </summary>
	public virtual void setRecommendation(string recommendation)
	{
		__recommendation = recommendation;
	}

	/// <summary>
	/// Return a string representation of the problem, suitable for display in a popup, etc.
	/// </summary>
	public override string ToString()
	{
		return "Severity:  " + __severity + "\n" +
		"Type:  " + __type + "\n" +
		"Problem:  " + __problem + "\n" +
		"Recommendation:  " + __recommendation + "\n"; // +
		//"Details:\n" + "uncomment"; //XXX dre:uncomment
		//__details_PropList;
	}

	}

}