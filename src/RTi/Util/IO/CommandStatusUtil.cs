using System.Collections.Generic;
using System.Text;

// CommandStatusUtil - provides convenience methods for working with CommandStatus.

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

	//import RTi.Util.Message.Message;

	/// <summary>
	/// Provides convenience methods for working with CommandStatus.
	/// 
	/// @author dre
	/// </summary>
	public class CommandStatusUtil
	{
	/// <summary>
	/// Adds a command to the HTML accumulating in the HTMLStatusAssembler. </summary>
	/// <param name="command"> </param>
	/// <param name="assembler"> </param>
	/// <returns> return 0 if no warnings or failures found, otherwise 1 </returns>
	private static int addCommandHTML(Command command, HTMLStatusAssembler assembler)
	{
		CommandStatus cs;
		int problemsFound = 0;

		if (command is CommandStatusProvider)
		{
			cs = ((CommandStatusProvider)command).getCommandStatus();
			if (isProblematic(cs))
			{
				problemsFound = 1;
			}
			// add Command
			assembler.addCommand(command.ToString());

			// add Command Summary Table
			addCommandSummary(cs, assembler);

			addCommandDetailTable(assembler, cs);
		}
		else
		{
			addNotACommandStatusProvider(assembler);
		}

		assembler.endCommand();

		return problemsFound;
	}

	/// <summary>
	/// Adds Detail table. </summary>
	/// <param name="assembler"> </param>
	/// <param name="cs"> </param>
	private static void addCommandDetailTable(HTMLStatusAssembler assembler, CommandStatus cs)
	{
		int nwarn = getSeverityCount(cs, CommandStatusType.WARNING);
		int nfail = getSeverityCount(cs, CommandStatusType.FAILURE);
		assembler.startCommandStatusTable(nwarn, nfail);

		int startCount = 1;
		if (cs.getCommandStatus(CommandPhaseType.INITIALIZATION) == CommandStatusType.WARNING || cs.getCommandStatus(CommandPhaseType.INITIALIZATION) == CommandStatusType.FAILURE)
		{
			int nprinted = addPhaseHTML(cs, assembler, CommandPhaseType.INITIALIZATION, startCount);
			if (nprinted > 0)
			{
				startCount += (nprinted - 1);
			}
		}
		if (cs.getCommandStatus(CommandPhaseType.DISCOVERY) == CommandStatusType.WARNING || cs.getCommandStatus(CommandPhaseType.DISCOVERY) == CommandStatusType.FAILURE)
		{
			int nprinted = addPhaseHTML(cs, assembler, CommandPhaseType.DISCOVERY, startCount);
			if (nprinted > 0)
			{
				startCount += (nprinted - 1);
			}
		}
		if (cs.getCommandStatus(CommandPhaseType.RUN) == CommandStatusType.SUCCESS || cs.getCommandStatus(CommandPhaseType.RUN) == CommandStatusType.WARNING || cs.getCommandStatus(CommandPhaseType.RUN) == CommandStatusType.FAILURE)
		{
			addPhaseHTML(cs, assembler, CommandPhaseType.RUN, startCount);
		}
	}

	/// <summary>
	/// Adds command status summary table. </summary>
	/// <param name="cs"> </param>
	/// <param name="assembler"> </param>
	private static void addCommandSummary(CommandStatus cs, HTMLStatusAssembler assembler)
	{
		assembler.addCommandStatusSummary(cs.getCommandStatus(CommandPhaseType.INITIALIZATION), cs.getCommandStatus(CommandPhaseType.DISCOVERY), cs.getCommandStatus(CommandPhaseType.RUN));
	}

	/// <summary>
	/// Adds text indicating no issues found. </summary>
	/// <param name="assembler"> </param>
	private static void addNotACommandStatusProvider(HTMLStatusAssembler assembler)
	{
		assembler.addNotACommandStatusProvider();
	}

	/// <summary>
	/// Adds html for a command status phase. </summary>
	/// <param name="cs"> CommandStatus instance to print </param>
	/// <param name="assembler"> object that processes the output </param>
	/// <param name="commandPhaseType"> the command phase type to print in output </param>
	/// <param name="startCount"> the starting count to display in output, will be incremented during output </param>
	/// <returns> the last count printed </returns>
	private static int addPhaseHTML(CommandStatus cs, HTMLStatusAssembler assembler, CommandPhaseType commandPhaseType, int startCount)
	{
		IList<CommandLogRecord> logRecList = cs.getCommandLog(commandPhaseType);
		int count = startCount;
		foreach (CommandLogRecord logRec in logRecList)
		{
			CommandStatusType severity = logRec.getSeverity();
			assembler.addPhase(count++, commandPhaseType.ToString(), severity.ToString(), getStatusColor(severity), logRec.getProblem(), logRec.getRecommendation());
		}
		return logRecList.Count;
	}

	/// <summary>
	/// Returns the command status log records ready for display as HTML. </summary>
	/// <param name="csp"> command status provider </param>
	/// <returns> concatenated log records as text </returns>
	//  public static String getCommandLogHTML(CommandStatus status)
	//    {
	//	  if ( status == null ) {
	//		  return "<HTML>Status is not available.</HTML>";
	//	  }
	//	  else {
	//		  //FIXME SAM 2007-08-15 Need to figure out where the following lives
	//		  //return HTMLUtil.text2html(status.getCommandLogText());
	//		  return "<HTML><pre>" + getCommandLogText(status) + "</pre></HTML>";
	//	  }
	//    }
	//    
	/// <summary>
	/// Returns the command log records ready for display as HTML. </summary>
	/// <param name="csp"> command status provider </param>
	/// <returns> concatenated log records as HTML </returns>
	//  public static String getCommandLogHTML(CommandStatusProvider csp)
	//    {
	//      return "<html><font bgcolor=red> Stati </font></html>";
	////	  if ( csp == null ) {
	////	    return "DeanDoIt";
	////	//	  return getCommandLogTextHTML ( (CommandStatus)null );
	////	  }
	////	  else {
	////		  return getCommandLogText(csp.getCommandStatus());
	////	  }
	//      
	//    }

	/// <summary>
	/// Append log records from a list of commands to a status.  For example, this is used
	/// when running a list of commands with a "runner" command like RunCommands to get a full list of logs.
	/// The command associated with the individual logs is set to the original command so that
	/// the "runner" is not associated with the log. </summary>
	/// <param name="status"> a CommandStatus instance to which log records should be appended. </param>
	/// <param name="commandList"> a list of CommandStatusProviders (such as Command instances) that
	/// have log records to be appended to the first parameter. </param>
	public static void appendLogRecords(CommandStatus status, IList<CommandStatusProvider> commandList)
	{
		  if (status == null)
		  {
			  return;
		  }
		  if (commandList == null)
		  {
			  return;
		  }
		  // Loop through the commands
		  int size = commandList.Count;
		  CommandStatusProvider csp;
		  for (int i = 0; i < size; i++)
		  {
			  // Transfer the command log records to the status...
			  csp = commandList[i];
			  CommandStatus status2 = csp.getCommandStatus();
			  // Append command log records for each run mode...
			  IList<CommandLogRecord> logs = status2.getCommandLog(CommandPhaseType.INITIALIZATION);
			  CommandLogRecord logRecord;
			  for (int il = 0; il < logs.Count; il++)
			  {
				  logRecord = logs[il];
				  logRecord.setCommandStatusProvider(csp);
				  status.addToLog(CommandPhaseType.INITIALIZATION, logRecord);
			  }
			  logs = status2.getCommandLog(CommandPhaseType.DISCOVERY);
			  for (int il = 0; il < logs.Count; il++)
			  {
				  logRecord = logs[il];
				  logRecord.setCommandStatusProvider(csp);
				  status.addToLog(CommandPhaseType.DISCOVERY, logRecord);
			  }
			  logs = status2.getCommandLog(CommandPhaseType.RUN);
			  for (int il = 0; il < logs.Count; il++)
			  {
				  logRecord = logs[il];
				  logRecord.setCommandStatusProvider(csp);
				  status.addToLog(CommandPhaseType.RUN, logRecord);
			  }
		  }
	}

	/// <param name="csp"> CommandStatusProvider (i.e., a command for which to get the command log). </param>
	/// <returns> the command log as HTML. </returns>
	public static string getCommandLogHTML(CommandStatusProvider csp)
	{
		string toolTip = getHTMLCommandStatus(csp.getCommandStatus());
		return toolTip;
	}

	/// <summary>
	/// Get the display name (problem type) for a CommandLogRecord class.  This is used, for example when displaying the
	/// full list of problems.  If the command log record has a non-blank type, it will be used.  Otherwise the default
	/// is "CommandRun".  If the CommandLogRecord class name indicates an extended class, then the class name is used. </summary>
	/// <param name="log"> CommandLogRecord instance. </param>
	public static string getCommandLogRecordDisplayName(CommandLogRecord log)
	{
		// Have to check the class name because instanceof will return true for anything derived from
		// CommandLogRecord
		string className = log.GetType().Name;
		if (className.Equals("CommandLogRecord"))
		{
			// Using base class for log record class - general command run-time error but indicate whether
			// the problem was generated in initialization, discover, or run
			// TODO SAM 2009-03-06 Need to figure out whether phases are reflected in the string
			string type = log.getType();
			if ((!string.ReferenceEquals(type, null)) && !type.Equals(""))
			{
				return type;
			}
			else
			{
				// Generic type.
				return "CommandRun";
			}
		}
		else
		{
			// The class name must be specific and is used for output.
			return className;
		}
	}

	  /// <summary>
	  /// Returns the command log records ready for display as text, suitable
	  /// for general output.  Rudimentary formatting is done to make the output readable,
	  /// but see the HTML output. </summary>
	  /// <param name="csp"> command status provider
	  /// </param>
	  /// <returns> concatenated log records as text </returns>
	  public static string getCommandLogText(CommandStatus cs)
	  {
		  if (cs == null)
		  {
			  return "Unable to determine command status.";
		  }

		  string nl = System.getProperty("line.separator");

		  string thick_line = "=================================================================";
		  string thin_line = "-----------------------------------------------------------------";
		  string dash_line = ".................................................................";
		  StringBuilder b = new StringBuilder();
		  b.Append(thick_line);
		  // TODO SAM 2007-09-06 Need to figure out how to list command string
		  //b.append( nl + "Command:  " + "would be nice to have here.")
		  //b.append( nl + thick_line );
		  b.Append(nl + "Initialization status: " + cs.getCommandStatus(CommandPhaseType.INITIALIZATION));
		  IList<CommandLogRecord> v = cs.getCommandLog(CommandPhaseType.INITIALIZATION);
		  if (v.Count > 0)
		  {
			  b.Append(nl + thin_line);
			  b.Append(nl + "Initialization log:");
			  int size = v.Count;
			  for (int i = 0; i < size; i++)
			  {
					  if (i > 0)
					  {
						  b.Append(nl + dash_line);
					  }
					b.Append(nl + v[i]);
			  }
		  }
		  b.Append(nl + thick_line);
		  b.Append(nl + "Discovery status: " + cs.getCommandStatus(CommandPhaseType.DISCOVERY));
		  v = cs.getCommandLog(CommandPhaseType.DISCOVERY);
		  if (v.Count > 0)
		  {
			  b.Append(nl + thin_line);
			  b.Append(nl + "Discovery log:");
			  int size = v.Count;
			  for (int i = 0; i < size; i++)
			  {
						if (i > 0)
						{
							b.Append(nl + dash_line);
						}
					b.Append(nl + v[i]);
			  }
		  }
		  b.Append(nl + thick_line);
		  b.Append(nl + "Run status: " + cs.getCommandStatus(CommandPhaseType.RUN));
		  v = cs.getCommandLog(CommandPhaseType.RUN);
		  if (v.Count > 0)
		  {
			  b.Append(nl + thin_line);
			  b.Append(nl + "Run log:");
			  int size = v.Count;
			  for (int i = 0; i < size; i++)
			  {
					  if (i > 0)
					  {
					  b.Append(nl + dash_line);
					  }
					b.Append(nl + v[i]);
			  }
		  }

		  return b.ToString();

	  }

	  /// <summary>
	  /// Returns the command log records ready for display as text </summary>
	  /// <param name="csp"> command status provider
	  /// </param>
	  /// <returns> concatenated log records as text </returns>
	//  public static String getCommandLogText(CommandStatusProvider csp)
	//  {
	//	  if ( csp == null ) {
	//		  return getCommandLogText ( (CommandStatus)null );
	//	  }
	//	  else {
	//		  return getCommandLogText ( csp.getCommandStatus() );
	//	  }
	//  }

	  /// <summary>
	  /// Returns the highest status severity of all phases, to indicate the most
	  /// severe problem with a command. </summary>
	  /// <param name="cs"> command status </param>
	  /// <seealso cref= CommandStatusType </seealso>
	  /// <returns> The highest severity status from a command. </returns>
	  /// <seealso cref= CommandStatusType </seealso>
	  public static CommandStatusType getHighestSeverity(CommandStatus cs)
	  {
		  CommandStatusType status = CommandStatusType.UNKNOWN;
		  if (cs == null)
		  {
			  return status; // Default UNKNOWN
		  }

			  CommandStatusType phaseStatus = cs.getCommandStatus(CommandPhaseType.INITIALIZATION);
			  if (phaseStatus.getSeverity() > status.getSeverity())
			  {
				  status = phaseStatus;
			  }
			  phaseStatus = cs.getCommandStatus(CommandPhaseType.DISCOVERY);
			  if (phaseStatus.getSeverity() > status.getSeverity())
			  {
				  status = phaseStatus;
			  }
			  phaseStatus = cs.getCommandStatus(CommandPhaseType.RUN);
			  // TODO sam 2017-04-13 This can be problematic if the discovery mode had a warning or failure
			  // and run mode was success.  This may occur due to dynamic files being created, etc.
			  // The overall status in this case should be success.
			  // Need to evaluate how this method gets called and what intelligence is used.
			  if (phaseStatus.getSeverity() > status.getSeverity())
			  {
				  status = phaseStatus;
			  }

		  return status;
	  }

	  /// <summary>
	  /// Returns the highest status severity of the specified phases, to indicate the most severe problem with a command. </summary>
	  /// <param name="cs"> command status </param>
	  /// <param name="commandPhaseTypes"> the command phases types to consider when evaluating the highest severity status.
	  /// If null or empty consider all. </param>
	  /// <seealso cref= CommandStatusType </seealso>
	  /// <returns> The highest severity status from a command, considering the requested phases. </returns>
	  /// <seealso cref= CommandStatusType </seealso>
	  public static CommandStatusType getHighestSeverity(CommandStatus cs, CommandPhaseType[] commandPhaseTypes)
	  {
		  CommandStatusType status = CommandStatusType.UNKNOWN;
		  if (cs == null)
		  {
			  return status; // Default UNKNOWN
		  }

		  bool includeInit = false;
		  bool includeDiscovery = false;
		  bool includeRun = false;

		  if ((commandPhaseTypes == null) || (commandPhaseTypes.Length == 0))
		  {
			  includeInit = true;
			  includeDiscovery = true;
			  includeRun = true;
		  }
		  else
		  {
			  for (int i = 0; i < commandPhaseTypes.Length; i++)
			  {
				  if (commandPhaseTypes[i] == CommandPhaseType.INITIALIZATION)
				  {
					  includeInit = true;
				  }
				  else if (commandPhaseTypes[i] == CommandPhaseType.DISCOVERY)
				  {
					  includeDiscovery = true;
				  }
				  else if (commandPhaseTypes[i] == CommandPhaseType.RUN)
				  {
					  includeRun = true;
				  }
			  }
		  }
			  CommandStatusType phaseStatus = null;
			  if (includeInit)
			  {
				  phaseStatus = cs.getCommandStatus(CommandPhaseType.INITIALIZATION);
				  if (phaseStatus.getSeverity() > status.getSeverity())
				  {
					  status = phaseStatus;
				  }
			  }
			  if (includeDiscovery)
			  {
				  phaseStatus = cs.getCommandStatus(CommandPhaseType.DISCOVERY);
				  if (phaseStatus.getSeverity() > status.getSeverity())
				  {
					  status = phaseStatus;
				  }
			  }
			  if (includeRun)
			  {
				  phaseStatus = cs.getCommandStatus(CommandPhaseType.RUN);
				  if (phaseStatus.getSeverity() > status.getSeverity())
				  {
					  status = phaseStatus;
				  }
			  }

		  return status;
	  }

	   /// <summary>
	   /// Returns the highest status severity of all phases, to indicate the most
	   /// severe problem with a command.
	   /// </summary>
	   /// <param name="csp"> command status provider </param>
	   /// <seealso cref= CommandStatusType </seealso>
	   /// <returns> The highest severity status from a command. </returns>
	   /// <seealso cref= CommandStatusType </seealso>
	  public static CommandStatusType getHighestSeverity(CommandStatusProvider csp)
	  {
		  if (csp == null)
		  {
			return CommandStatusType.UNKNOWN;
		  }

		  return getHighestSeverity(csp.getCommandStatus());
	  }

	  /// <summary>
	  /// Returns the highest status severity for the indicated phases.
	  /// </summary>
	  /// <param name="csp"> command status provider </param>
	  /// <seealso cref= CommandStatusType </seealso>
	  /// <returns> The highest severity status from a command. </returns>
	  /// <seealso cref= CommandStatusType </seealso>
	  public static CommandStatusType getHighestSeverity(CommandStatusProvider csp, CommandPhaseType[] commandPhaseTypes)
	  {
		  if (csp == null)
		  {
			return CommandStatusType.UNKNOWN;
		  }

		  return getHighestSeverity(csp.getCommandStatus(), commandPhaseTypes);
	  }

	  /// <summary>
	  /// Returns the Command Status for the specified commands as HTML.
	  /// </summary>
	  /// <param name="commands"> </param>
	  /// <returns> HTML status report </returns>
	  public static string getHTMLStatusReport(IList<Command> commands)
	  {
		HTMLStatusAssembler assembler = new HTMLStatusAssembler();
		int count = 0;

		foreach (Command command in commands)
		{
			count = count + addCommandHTML(command, assembler);
		}
		/* TODO SAM 2013-12-07 The above will now add output for all successful phases so no need for below
		if (count == 0)
		  {
		    assembler.addNoProblems();
		  }
		  */

		return assembler.getHTML();
	  }

	  /// <summary>
	  /// Returns status for a single command in HTML.
	  /// <para>
	  /// Useful for providing tooltip
	  /// </para>
	  /// </summary>
	  /// <param name="css">
	  /// @return </param>
	public static string getHTMLCommandStatus(CommandStatus css)
	{
	  HTMLStatusAssembler assembler = new HTMLStatusAssembler();
	  addCommandDetailTable(assembler, css);
	  return assembler.getHTML();
	}

	/// <summary>
	/// Given a list of Command, return a list of all the log records.  This is useful for
	/// providing the full list for reporting.  The getLogRecordList() method is called with the commands that
	/// implement CommandStatusProvider. </summary>
	/// <param name="commandList"> List of CommandStatusProvider (e.g., list of commands from a command processor). </param>
	/// <param name="commandPhase"> command phase to return or null to return all (currently null does not return anything). </param>
	/// <returns> the list of log records for the given command phase </returns>
	public static IList<CommandLogRecord> getLogRecordListFromCommands(IList<Command> commandList, CommandPhaseType commandPhase)
	{
		IList<CommandStatusProvider> commandStatusProviderList = new List<CommandStatusProvider>();
		foreach (Command command in commandList)
		{
			if (command is CommandStatusProvider)
			{
				commandStatusProviderList.Add((CommandStatusProvider)command);
			}
		}
		CommandPhaseType[] phases = new CommandPhaseType[1];
		phases[0] = commandPhase;
		return getLogRecordList(commandStatusProviderList, phases, null);
	}

	/// <summary>
	/// Given a list of CommandStatusProvider, return a list of all the log records.  This is useful for
	/// providing the full list for display in a UI or output file. </summary>
	/// <param name="commandStatusProviderList"> List of CommandStatusProvider (e.g., list of commands from a command processor). </param>
	/// <param name="commandPhases"> array of command phases to return or null to return all. </param>
	/// <param name="commandStatuses"> array of command statuses to return or null to return all. </param>
	/// <returns> the list of log records for the given command phase </returns>
	public static IList<CommandLogRecord> getLogRecordList(IList<CommandStatusProvider> commandStatusProviderList, CommandPhaseType[] commandPhases, CommandStatusType[] commandStatuses)
	{ //String routine = "CommandStatusUtil.getLogRecordList";
		IList<CommandLogRecord> logRecordList = new List<CommandLogRecord>(); // Returned list of log records
		if (commandStatusProviderList == null)
		{
			// Return empty list
			return logRecordList;
		}
		CommandStatus status = null; // Status for the command
		IList<CommandLogRecord> statusLogRecordList = null; // List of status log records for the command
		foreach (CommandStatusProvider csp in commandStatusProviderList)
		{
			// Get the information from a single command status provider
			//Message.printStatus(2, routine, "Getting log records for " + csp );
			status = csp.getCommandStatus();
			statusLogRecordList = status.getCommandLog(commandPhases,commandStatuses);
			//Message.printStatus(2, routine, "Command " + csp + " has " + statusLogRecordList.size() + " log records." );
			foreach (CommandLogRecord logRecord in statusLogRecordList)
			{
				// Append to full list of log records
				// Also set the command instance in the log record here since it was not
				// included in the original design.
				// Special case is that RunCommands() will set the command status provider when it rolls up status messages
				// so don't reset the CommandStatusProvider here in that case.
				if (logRecord.getCommandStatusProvider() == null)
				{
					logRecord.setCommandStatusProvider(csp);
				}
				logRecordList.Add(logRecord);
			}
		}
		return logRecordList;
	}

	/// <summary>
	/// Determine the number of log records for the requested severity. </summary>
	/// <param name="status"> a CommandStatus that has log records to be appended to the first parameter. </param>
	/// <param name="severity"> the requested severity for a count. </param>
	/// <returns> the number of log records for the requested severity. </returns>
	public static int getSeverityCount(CommandStatus status, CommandStatusType severity)
	{
		int severityCount = 0;
		for (int iphase = 0; iphase <= 2; iphase++)
		{
			IList<CommandLogRecord> logs = null;
			if (iphase == 0)
			{
				logs = status.getCommandLog(CommandPhaseType.INITIALIZATION);
			}
			else if (iphase == 1)
			{
				logs = status.getCommandLog(CommandPhaseType.DISCOVERY);
			}
			else if (iphase == 2)
			{
				logs = status.getCommandLog(CommandPhaseType.RUN);
			}
			foreach (CommandLogRecord log in logs)
			{
				if (log.getSeverity() == severity)
				{
					++severityCount;
				}
			}
		}
		return severityCount;
	}

	/// <summary>
	/// Determine the number of log records (or commands) for the requested severity. </summary>
	/// <param name="command"> a command that has log records to be appended to the first parameter.  It must implement
	/// CommandStatusProvider. </param>
	/// <param name="severity"> the requested severity for a count. </param>
	/// <returns> the number of log records for the requested severity. </returns>
	/// <param name="countCommands"> if false, return the total count of log records for a severity, and if true return the total
	/// number of commands that have at least one log record with the indicated severity (in this case 0 or 1). </param>
	public static int getSeverityCount(Command command, CommandStatusType severity, bool countCommands)
	{
		IList<Command> list = new List<Command>(1);
		list.Add(command);
		return getSeverityCount(list, severity, countCommands);
	}

	/// <summary>
	/// Determine the number of log records (or commands) for the requested severity. </summary>
	/// <param name="commandList"> a list of CommandStatusProviders (such as Command instances) to be examined. </param>
	/// <param name="severity"> the requested severity for a count. </param>
	/// <returns> the number of log records for the requested severity. </returns>
	/// <param name="countCommands"> if false, return the total count of log records for a severity, and if true return the total
	/// number of commands that have at least one log record with the indicated severity. </param>
	public static int getSeverityCount(IList<Command> commandList, CommandStatusType severity, bool countCommands)
	{
		  if (commandList == null)
		  {
			  return 0;
		  }
		  // Loop through the commands
		  int size = commandList.Count;
		  CommandStatusProvider csp;
		  Command command;
		  int severityCount = 0;
		  int severityCountCommandCount = 0;
		  for (int i = 0; i < size; i++)
		  {
			  // Transfer the command log records to the status...
			  command = commandList[i];
			  if (command is CommandStatusProvider)
			  {
				  csp = (CommandStatusProvider)command;
			  }
			  else
			  {
				  continue;
			  }
			  CommandStatus status = csp.getCommandStatus();
			  // Get the logs for the initialization...
			  int severityCount0 = getSeverityCount(status, severity);
			  severityCount += severityCount0;
			  if (severityCount0 > 0)
			  {
				  // Found a command that had status of the requested severity
				  ++severityCountCommandCount;
			  }
		  }
		  if (countCommands)
		  {
			  return severityCountCommandCount;
		  }
		  else
		  {
			  return severityCount;
		  }
	}

	/// <summary>
	/// Returns a color associated with specified status type, for background color.
	/// </summary>
	/// <param name="type"> command status type </param>
	/// <returns> color associated with type </returns>
	public static string getStatusColor(CommandStatusType type)
	{
		if (type == CommandStatusType.SUCCESS)
		{
			return "green";
		}
		else if (type == CommandStatusType.WARNING)
		{
			return "yellow";
		}
		else if (type == CommandStatusType.FAILURE)
		{
			return "red";
		}
		else
		{
			return "white";
		}
	}

	  /// <summary>
	  /// Returns whether command status has warnings/failures. </summary>
	  /// <param name="cs"> </param>
	  /// <returns> True if command has warnings/failures </returns>
	  private static bool isProblematic(CommandStatus cs)
	  {
		bool ret = false;
		if (getHighestSeverity(cs).greaterThan(CommandStatusType.SUCCESS))
		{
			ret = true;
		}
		return ret;
	  }
	}

}