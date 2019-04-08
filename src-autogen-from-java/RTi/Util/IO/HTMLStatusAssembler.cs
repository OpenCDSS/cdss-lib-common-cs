using System.Text;

// HTMLStatusAssembler - provides support for creating the HTML string for CommandStatus.

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
	/// Provides support for creating the HTML string for Command Status.
	/// <para>
	/// 
	/// @author dre
	/// </para>
	/// </summary>
	public class HTMLStatusAssembler
	{
	  private readonly StringBuilder TRAILER = new StringBuilder("</body></html>");
	  private StringBuilder buf = new StringBuilder("<html><body>");
	  // Add some spaces around the row count because the Java HTML viewer smashes together.
	  private readonly string TABLE_START = "<table border=1 width=650><tr bgcolor=\"CCCCFF\"><th align=left>"
			  + "&nbsp;&nbsp;&nbsp#&nbsp;&nbsp;&nbsp;</th>"
			  + "<th align=left>Phase</th><th align=left>Severity</th>"
			  + "<th align=left width=300>Problem</th><th>Recommendation</th></tr>";

	  private readonly string SUMMARY_TABLE_START = "<table border=1>"
			 + "<tr bgcolor=\"CCCCFF\"><th align=left>Phase</th><th align=left>Status/Max Severity</th></tr>";

	  private readonly string TABLE_END = "</table>";

	  /// <summary>
	  /// Creates a new HTML assembler for command status assembly.
	  /// </summary>
	  public HTMLStatusAssembler()
	  {
	  }

	  /// <summary>
	  /// Returns a HTML string ready for display in a browser.
	  /// </summary>
	  /// <returns> HTML string  </returns>
	  public virtual string getHTML()
	  {
		buf.Append(TRAILER);
		return buf.ToString();
	  }

	  /// <summary>
	  /// Adds entry for a phase in HTML.
	  /// </summary>
	  /// <param name="phase"> One of: INITIALIZATION,DISCOVERY,RUN </param>
	  /// <param name="severity"> One of : WARNING,ERROR </param>
	  /// <param name="color"> color associated with severity </param>
	  /// <param name="problem"> problem encountered </param>
	  /// <param name="recommendation"> recommended solution </param>
	  public virtual void addPhase(int count, string phase, string severity, string color, string problem, string recommendation)
	  {
		string bgcolor = "</td><td valign=top bgcolor=" + color + ">";

		buf.Append("<tr><td valign=top>" + count + "</td><td valign=top>" + phase + bgcolor + severity + "</td><td valign=top>" + HTMLUtil.text2html(problem,false) + "</td>" + "<td valign=top>" + HTMLUtil.text2html(recommendation,false) + "</td></tr>");
	  }

	  /// <summary>
	  /// Adds an entry for a command in HTML.
	  /// <para>
	  /// Note for each addCommand(), a endCommand() is required.
	  /// </para>
	  /// </summary>
	  /// <param name="commandString"> </param>
	  public virtual void addCommand(string commandString)
	  {
		buf.Append("<p><font bgcolor=white").Append("<strong>Command: " + commandString).Append("</strong></font>");
	  }

	  /// <summary>
	  /// Add HTML to start status table </summary>
	  /// <param name="nlog"> the number of log messages that will be shown (WARNING and more severe). </param>
	  public virtual void startCommandStatusTable(int nwarn, int nfail)
	  {
		buf.Append("<p><b>Command Status Details (" + nwarn + " warnings, " + nfail + " failures):");
		buf.Append(TABLE_START);
	  }

	  /// <summary>
	  /// Add HTML to terminate a command initiated with addCommand()
	  /// </summary>
	  public virtual void endCommand()
	  {
		buf.Append(TABLE_END);
	  }

	  public virtual void addNotACommandStatusProvider()
	  {
		buf.Append("<tr><td>Not a CommandStatusProvider</td></tr>");
	  }

	  /// <summary>
	  /// Add the command status summary table </summary>
	  /// <param name="commandStatus1"> </param>
	  /// <param name="commandStatus2"> </param>
	  /// <param name="commandStatus3"> </param>
	  public virtual void addCommandStatusSummary(CommandStatusType commandStatus1, CommandStatusType commandStatus2, CommandStatusType commandStatus3)
	  {

		string bgColor1 = "<td bgcolor=" + CommandStatusUtil.getStatusColor(commandStatus1) + ">";
		string bgColor2 = "<td bgcolor=" + CommandStatusUtil.getStatusColor(commandStatus2) + ">";
		string bgColor3 = "<td bgcolor=" + CommandStatusUtil.getStatusColor(commandStatus3) + ">";
		buf.Append("<p><b>Command Status Summary</b> (see below for details if problems exist):");

		buf.Append(SUMMARY_TABLE_START).Append("<tr><td>INITIALIZATION</td>").Append(bgColor1).Append(commandStatus1.ToString()).Append("</td></tr>").Append("<tr><td>DISCOVERY</td>").Append(bgColor2).Append(commandStatus2.ToString()).Append("</td></tr>").Append("<tr><td>RUN</td>").Append(bgColor3).Append(commandStatus3.ToString()).Append("</td></tr>").Append(TABLE_END);

	  }
	  public override string ToString()
	  {
		return buf.ToString();
	  }
	/// <summary>
	/// Adds a summary table indicating no issues found.
	/// 
	/// </summary>
	  public virtual void addNoProblems()
	  {
		string bgColor1 = "<td bgcolor=" + CommandStatusUtil.getStatusColor(CommandStatusType.SUCCESS) + ">";
		buf.Append(SUMMARY_TABLE_START).Append("<tr><td>INITIALIZATION").Append(bgColor1).Append(CommandStatusType.SUCCESS.ToString()).Append("</tr>").Append("<tr><td>DISCOVERY").Append(bgColor1).Append(CommandStatusType.SUCCESS.ToString()).Append("</tr>").Append("<tr><td>RUN").Append(bgColor1).Append(CommandStatusType.SUCCESS.ToString()).Append("</tr>").Append(TABLE_END);
	  }
	}

}