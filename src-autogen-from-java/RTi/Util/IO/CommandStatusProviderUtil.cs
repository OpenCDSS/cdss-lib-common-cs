using System.Collections.Generic;

// CommandStatusProviderUtil - provides convenience methods for working with CommandStatus.

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
	/// Provides convenience methods for working with Command Status.
	/// 
	/// @author dre
	/// </summary>
	public class CommandStatusProviderUtil
	{
	  /// <summary>
	  /// Returns the command log records ready for display as html.
	  /// </summary>
	  /// <param name="csp"> command status provider </param>
	  /// <returns> concatenated log records as text
	  ///  </returns>
	  public static string getCommandLogHTML(object o)
	  {
		if (o is CommandStatusProvider)
		{
			CommandStatusProvider csp = (CommandStatusProvider)o;
			string toolTip = CommandStatusProviderUtil.getCommandLogHTML(csp);
			return toolTip;
		}
		else
		{
		  return null;
		}
	  }

	  /// <summary>
	  /// Returns the command log records ready for display as html.
	  /// </summary>
	  /// <param name="csp"> command status provider </param>
	  /// <returns> concatenated log records as text </returns>
	  public static string getCommandLogHTML(CommandStatusProvider csp)
	  {
		return HTMLUtil.text2html(getCommandLogText(csp),true);
	  }


	  /// <summary>
	  /// Returns the command log records ready for display as text </summary>
	  /// <param name="csp"> command status provider
	  /// </param>
	  /// <returns> concatenated log records as text </returns>
	  public static string getCommandLogText(CommandStatusProvider csp)
	  {
		string markerText = "";

		if (csp != null)
		{
			CommandStatus cs = csp.getCommandStatus();

			if (cs != null)
			{
				IList<CommandLogRecord> v = cs.getCommandLog(CommandPhaseType.INITIALIZATION);
				int size = v.Count;
				for (int i = 0; i < size; i++)
				{
					markerText = markerText + v[i].ToString();
				}
				v = cs.getCommandLog(CommandPhaseType.DISCOVERY);
				size = v.Count;
				for (int i = 0; i < size; i++)
				{
					markerText = markerText + v[i].ToString();
				}
				v = cs.getCommandLog(CommandPhaseType.RUN);
				size = v.Count;
				for (int i = 0; i < size; i++)
				{
					markerText = markerText + v[i].ToString();
				}
			}

		}
		return markerText;

	  }

	  /// <summary>
	  /// Returns the highest status severity of all provided command status providers,
	  /// to indicate the most severe problem with a list of commands. </summary>
	  /// <param name="csp_list"> command status provider list. </param>
	  /// <seealso cref= CommandStatusType </seealso>
	  /// <returns> The highest severity status from any command status provider in the list. </returns>
	  public static CommandStatusType getHighestSeverity(IList<CommandStatusProvider> csp_list)
	  {
			if (csp_list == null)
			{
			return CommandStatusType.UNKNOWN;
			}

			// Loop through the list...

			int size = csp_list.Count;
			CommandStatusType max = CommandStatusType.UNKNOWN;
			CommandStatusProvider csp;
			for (int i = 0; i < size; i++)
			{
				csp = csp_list[i];
				max = CommandStatusType.maxSeverity(max, CommandStatusUtil.getHighestSeverity(csp));
			}
		  return max;

	  }

	  /// <summary>
	  /// Returns the highest status severity of all phases.
	  /// </summary> </param>
	  /// <param name="csp"> command status provider <seealso cref= CommandStatusType </seealso>
	  /// <returns> highest severity 
	  /// <ol> 
	  ///  <li> -1 - UNKNOWN
	  ///  <li> 0 - SUCCESS</li>
	  ///  <li> 1 - WARNING</li>
	  ///  <li> 2 - FAILURE</li>
	  ///  </ol> </returns>
	  /// <seealso cref= CommandStatusType </seealso>
	  public static int getHighestSeverity(CommandStatusProvider csp)
	  {
		CommandStatus cs = csp.getCommandStatus();
		int status = CommandStatusType.UNKNOWN.getSeverity();

		if (cs != null)
		{
			int phaseStatus = cs.getCommandStatus(CommandPhaseType.INITIALIZATION).getSeverity();
			if (phaseStatus > status)
			{
				status = phaseStatus;
			}
			phaseStatus = cs.getCommandStatus(CommandPhaseType.DISCOVERY).getSeverity();
			if (phaseStatus > status)
			{
				status = phaseStatus;
			}
			phaseStatus = cs.getCommandStatus(CommandPhaseType.RUN).getSeverity();
			if (phaseStatus > status)
			{
				status = phaseStatus;
			}
		}
		return status;
	  } // eof getHighestSeverity()
	}


}