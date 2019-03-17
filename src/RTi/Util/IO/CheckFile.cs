using System;
using System.Collections.Generic;

// CheckFile - stores all information for a CheckFile

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
///****************************************************************************
/// File: CheckFile.java
/// Author: KAT
/// Date: 2007-03-15
/// Stores all information for a CheckFile.  A check file is an html file that
/// has information on data checks for various components.
/// *******************************************************************************
/// Revisions 
/// *******************************************************************************
/// 2007-03-15	Kurt Tometich	Initial version.
/// *****************************************************************************
/// </summary>
namespace RTi.Util.IO
{

	/// <summary>
	/// Stores all information for a CheckFile.  A check file is an HTML file that
	/// has information on data checks for various components.  This class does not
	/// perform data checks or validation, it is only used to format information
	/// into an HTML file.  Numerous Check File Data Models can be added to a
	/// CheckFile.  The CheckFile was designed for the State of Colorado to provide
	/// an easier way to navigate and filter on final product data checks.
	/// </summary>
	public class CheckFile
	{
		private string __check_file; // File for data check info
		private string __commands; // Command file name

		private IList<string> __header; // Header text for the check file.
										// This contains text about the
										// current program such as version
										// and other program specific
										// properties and system configurations
										// This is static and is not tied to the
										// data.  It is more of an overview of the
										// program and its state.  If empty then the
										// header section will be blank.

		private IList<string> __run_msgs; // List of runtime messages
										// Runtime messages are problems that
										// have occurred during processing checks
										// on the data.

		// Shared proplists used for html attributes.
		// Id's can be used to style or format
		// a specific HTML tag.  __title_prop is used
		// to set an id for HTML tags that are used
		// as titles to a section.  __td_prop is used
		// to set attributes for the td tag.
		private PropList __title_prop = new PropList("html_id");
		private PropList __td_prop = new PropList("html_td_tag");
		// stores invalid data for specific data checks	
		private IList<CheckFile_DataModel> __spec_data;
		// stores invalid data for general data checks
		private IList<CheckFile_DataModel> __gen_data;

	/// <summary>
	/// Constructor that initializes the check file. </summary>
	/// <param name="fname"> Name of the check file. </param>
	/// <param name="command_file"> Name of the command file. </param>
	public CheckFile(string command_file, string commands_asString)
	{
		if (isValidStr(command_file))
		{
			__check_file = command_file + ".html";
		}
		else
		{
			__check_file = "";
		}
		if (isValidStr(commands_asString))
		{
			__commands = commands_asString;
		}
		else
		{
			__commands = "";
		}

		// add the id value to be used to format HTML tags
		__title_prop.add("id=titles");
		// add the td attributes to the proplist
		__td_prop.add("valign=bottom");
		__run_msgs = new List<string>(); // Runtime error messages
		__header = new List<string>(); // Header for check file
		__gen_data = new List<CheckFile_DataModel>(); // general data checks
		__spec_data = new List<CheckFile_DataModel>(); // specific data checks
	}

	/// <summary>
	/// Adds data to the data vectors for the check file.  Does some simple checks
	/// to make sure data is available. </summary>
	/// <param name="data"> Model for the specific checked data. </param>
	/// <param name="table_headers"> List of column headers. </param>
	/// <param name="header"> Header for this data. </param>
	/// <param name="title"> Title for this data. </param>
	/// <param name="gen_data"> Model for the general checked data. </param>
	public virtual void addData(CheckFile_DataModel data, CheckFile_DataModel gen_data)
	{
		if (data != null)
		{
			__spec_data.Add(data);
			if (gen_data != null)
			{
				__gen_data.Add(gen_data);
			}
			// if no general data exists create a blank object for it
			else
			{
				__gen_data.Add(new CheckFile_DataModel(new List<object>(), new string[]{}, "", "", 0, 0));
			}
		}
	}

	/// <summary>
	/// Adds runtime messages (errors or warnings) to the runtime warning Vector. </summary>
	/// <param name="str"> String to add to the list. </param>
	public virtual void addRuntimeMessage(string str)
	{
		if (isValidStr(str))
		{
			__run_msgs.Add(str);
		}
	}

	/// <summary>
	/// Adds a string to the current header. </summary>
	/// <param name="str"> String to add to the header. </param>
	public virtual void addToHeader(string str)
	{
		if (isValidStr(str))
		{
			__header.Add(str);
		}
	}

	/// <summary>
	/// Adds an array of strings to the current header. </summary>
	/// <param name="strs"> List of Strings to add to the header. </param>
	public virtual void addToHeader(string[] strs)
	{
		for (int i = 0; i < strs.Length; i++)
		{
			if (!string.ReferenceEquals(strs[i], null))
			{
				addToHeader(strs[i]);
			}
		}
	}

	/// <summary>
	/// Close all open tags and finish writing the html to the file. </summary>
	/// <param name="html"> HTMLWriter object. </param>
	/// <exception cref="Exception">  </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void endHTML(HTMLWriter html) throws Exception
	private void endHTML(HTMLWriter html)
	{
		if (html != null)
		{
			html.bodyEnd();
			html.htmlEnd();
			html.closeFile();
		}
	}

	/// <summary>
	/// Writes all sections of the check file in HTML.  Requires that
	/// the data Vectors have been populated with the correct data and
	/// headers. </summary>
	/// <exception cref="Exception">  </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String finalizeCheckFile() throws Exception
	public virtual string finalizeCheckFile()
	{
		HTMLWriter html = new HTMLWriter(__check_file, "Check File", false);
		startHTML(html);
		writeTableOfContents(html);
		writeHeader(html);
		writeCommandFile(html);
		writeRuntimeMessages(html);
		writeDataChecks(html);
		endHTML(html);

		return __check_file;
	}

	/// <summary>
	/// Helper method to return the header string for the HTML file. </summary>
	/// <returns> Header text for the HTML file. </returns>
	private string getHeaderString()
	{
		string header = "";
		if (__header != null && __header.Count > 0)
		{
			for (int i = 0; i < __header.Count; i++)
			{
				string tmp = (string)__header[i];
				if (!tmp.EndsWith("\n", StringComparison.Ordinal) && !tmp.EndsWith("\r", StringComparison.Ordinal))
				{
					header += (tmp + "\n");
				}
				else
				{
					header += tmp;
				}
			}
		}
		return header;
	}

	/// <summary>
	/// Inserts the head tags and style information for RTi check files. </summary>
	/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void headForCheckFile(HTMLWriter html) throws Exception
	public virtual void headForCheckFile(HTMLWriter html)
	{
		html.headStart();
		writeCheckFileStyle(html);
		html.headEnd();
	}

	/// <summary>
	/// Checks if the given string is null or has a length of zero. </summary>
	/// <param name="str"> String to check. </param>
	/// <returns> If string is valid or not. </returns>
	private bool isValidStr(string str)
	{
		if (!string.ReferenceEquals(str, null) && str.Length > 0)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Writes the start tags for the HTML check file. </summary>
	/// <param name="html"> HTMLWriter object. </param>
	/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void startHTML(HTMLWriter html) throws Exception
	private void startHTML(HTMLWriter html)
	{
		if (html != null)
		{
			html.htmlStart();
			headForCheckFile(html);
			html.bodyStart();
		}
	}

	/// <summary>
	/// An overridden method for toString().  Returns the name of
	/// the check file. </summary>
	/// <returns> Name of the check file. </returns>
	public override string ToString()
	{
		return __check_file;
	}

	/// <summary>
	/// Inserts the style attributes for a check file. </summary>
	/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeCheckFileStyle(HTMLWriter html) throws Exception
	public virtual void writeCheckFileStyle(HTMLWriter html)
	{
		html.write("<style>\n" + "#titles { font-weight:bold; color:#303044 }\n" + "table { background-color:black; text-align:left }\n" + "th {background-color:#333366; text-align:center;" + " vertical-align:bottom; color:white }\n" + "td {background-color:white; text-align:center;" + " vertical-align:bottom; }\n" + "body { text-align:left; font-size:12; }\n" + "pre { font-size:12; }\n" + "p { font-size:12; }\n" + "</style>\n");
	}

	/// <summary>
	/// Writes the command file in HTML using the <pre> tag.  This will
	/// force the output to match the command files text exactly. </summary>
	/// <param name="html"> HTMLWriter object. </param>
	/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void writeCommandFile(HTMLWriter html) throws Exception
	private void writeCommandFile(HTMLWriter html)
	{
		if (html != null)
		{
			// proplist provides an anchor link for this section used
			// from the table of contents
			PropList command_file_prop = new PropList("Command File");
			command_file_prop.add("name=command_file");
			html.paragraphStart(__title_prop);
			html.link(command_file_prop, "", "Command File");
			html.paragraphEnd();
			html.pre(__commands);
			html.horizontalRule();
		}
	}

	/// <summary>
	/// Writes the HTML for the specific data checks. </summary>
	/// <param name="html"> HTMLWriter object. </param>
	/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void writeDataChecks(HTMLWriter html) throws Exception
	private void writeDataChecks(HTMLWriter html)
	{
		if (html != null)
		{
			// proplist provides an anchor link for this section used
			// from the table of contents
			PropList tableStart = new PropList("Table");
			tableStart.add("border=\"1\"");
			tableStart.add("bordercolor=black");
			tableStart.add("cellspacing=1");
			tableStart.add("cellpadding=1");
			// write out all component data checks
			for (int i = 0; i < __spec_data.Count; i++)
			{
				writeGenericDataChecks(html, tableStart, (CheckFile_DataModel)__gen_data[i], i);
				html.breakLine();
				writeOtherData(html, tableStart, (CheckFile_DataModel)__spec_data[i], i);
			}
		}
	}

	/// <summary>
	/// Writes the HTML for the general data checks. </summary>
	/// <param name="html"> HTMLWriter object. </param>
	/// <param name="tableStart"> - List of properties for the HTML table. </param>
	/// <param name="int"> index Current index of the data list. </param>
	/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void writeGenericDataChecks(HTMLWriter html, PropList tableStart, CheckFile_DataModel gen_data_model, int index) throws Exception
	private void writeGenericDataChecks(HTMLWriter html, PropList tableStart, CheckFile_DataModel gen_data_model, int index)
	{
		if (html != null)
		{
			// grab the data from the model
			System.Collections.IList gen_data = new List<object>();
			gen_data = gen_data_model.getData();
			// proplist provides an anchor link for this section used
			// from the table of contents
			PropList gen_prop = new PropList("Gen");
			gen_prop.add("name=generic" + index);
			// start the generic data section
			html.paragraphStart(__title_prop);
			html.link(gen_prop, "", gen_data_model.getTitle());
			if (gen_data.Count > 0)
			{
				// table start
				html.tableStart(tableStart);
				html.tableRowStart();
				html.tableHeaders(gen_data_model.getTableHeader());
				html.tableRowEnd();
				// loop through the actual data and add table cells
				for (int j = 0; j < gen_data.Count; j++)
				{
					string[] tds = ((string [])gen_data[j]);
					if (tds != null && tds.Length > 0)
					{
						html.tableRowStart();
						html.tableCells(tds, __td_prop);
						html.tableRowEnd();
					}
				}
				html.tableEnd();
			}
		}
	}

	/// <summary>
	/// Helper method to write the header section of the check file. </summary>
	/// <param name="html"> HTMLWriter object. </param>
	/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void writeHeader(HTMLWriter html) throws Exception
	private void writeHeader(HTMLWriter html)
	{
		if (html != null)
		{
			// proplist provides an anchor link for this section used
			// from the table of contents
			PropList header_prop = new PropList("header");
			header_prop.add("name=header");
			html.paragraphStart(__title_prop);
			html.link(header_prop, "", "Header");
			html.paragraphEnd();
			html.pre(getHeaderString());
			html.horizontalRule();
		}
	}

	/// <summary>
	/// Writes the specific data checks to the check file. </summary>
	/// <param name="html"> HTMLWriter object. </param>
	/// <param name="tableStart"> List of HTML table properties. </param>
	/// <param name="index"> The current index of the data Vector. </param>
	/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void writeOtherData(HTMLWriter html, PropList tableStart, CheckFile_DataModel data_model, int index) throws Exception
	private void writeOtherData(HTMLWriter html, PropList tableStart, CheckFile_DataModel data_model, int index)
	{
		if (html != null)
		{
			// Get the data from the model
			System.Collections.IList data = data_model.getData();
			// proplist provides an anchor link for this section used
			// from the table of contents
			PropList data_prop = new PropList("Data " + index);
			data_prop.add("name=data" + index);
			// write the more component specific data
			html.paragraphStart(__title_prop);
			html.link(data_prop, "", data_model.getTitle());
			html.pre(data_model.getInfo());
			if (data.Count > 0)
			{
				// table start
				html.tableStart(tableStart);
				html.tableRowStart();
				html.tableHeaders(data_model.getTableHeader());
				html.tableRowEnd();
				// loop through the data
				for (int j = 0; j < data.Count; j++)
				{
					string[] tds = ((string [])data[j]);
					if (tds != null && tds.Length > 0)
					{
						html.tableRowStart();
						html.tableCells(tds, __td_prop);
						html.tableRowEnd();
					}
				}
				html.tableEnd();
			}
		}
	}

	/// <summary>
	/// Writes all runtime messages to the runtime message section
	/// in the check file. </summary>
	/// <param name="html"> HTMLWriter object. </param>
	/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void writeRuntimeMessages(HTMLWriter html) throws Exception
	private void writeRuntimeMessages(HTMLWriter html)
	{
		if (html != null)
		{
			// proplist provides an anchor link for this section used
			// from the table of contents
			PropList runmsg_prop = new PropList("Run Messages");
			runmsg_prop.add("name=run_msgs");
			html.paragraphStart(__title_prop);
			html.link(runmsg_prop, "", "Run Messages");
			html.paragraphEnd();
			string message = "";
			if (__run_msgs.Count == 0)
			{
				__run_msgs.Add("No warnings or errors encountered");
			}
			// loop through all the run messages and print them out
			for (int i = 0; i < __run_msgs.Count; i++)
			{
				string msg = (string)__run_msgs[i];
				if (isValidStr(msg))
				{
					if (!msg.EndsWith("\n", StringComparison.Ordinal) && !msg.EndsWith("\r", StringComparison.Ordinal))
					{
						message += (msg + "\n");
					}
					else
					{
						message += msg;
					}
				}
			}
			html.pre(message);
			html.horizontalRule();
		}
	}

	/// <summary>
	/// Writes the HTML table of contents for the check file. </summary>
	/// <param name="html"> HTMLWriter object. </param>
	/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void writeTableOfContents(HTMLWriter html) throws Exception
	private void writeTableOfContents(HTMLWriter html)
	{
		if (html != null)
		{
			// properties for the table of contents HTML table
			string tcontents = "Table Of Contents";
			PropList tableStart = new PropList("Table");
			tableStart.add("border=\"1\"");
			tableStart.add("bordercolor=black");
			tableStart.add("cellspacing=1");
			tableStart.add("cellpadding=1");
			string[] data_table_header = new string[] {"Component", "Type of Check", "# Problems", "# Total Checks"};
			//html follows ...
			html.headerStart(4, __title_prop); // <h3> tag
			html.addText(tcontents);
			html.headerEnd(4);
			html.link("#header", "Header");
			html.breakLine();
			html.link("#command_file", "Command File");
			html.breakLine();
			html.link("#run_msgs", "Runtime Messages (" + __run_msgs.Count + ")");
			html.breakLine();
			// Table of contents data records (there may be many of these)
			// this is written as a table of components and there
			// general and specific data checks
			html.tableStart(tableStart);
			html.tableRowStart();
			html.tableHeaders(data_table_header);
			html.tableRowEnd();
			// Write out the data and links to data checks
			// as a table with links to missing and specific data checks
			for (int i = 0; i < __spec_data.Count; i++)
			{
				// get the data models
				CheckFile_DataModel dm = (CheckFile_DataModel)__spec_data[i];
				CheckFile_DataModel dm_gen = (CheckFile_DataModel)__gen_data[i];
				// get the data needed for the TOC from the data models
				//String data_size = new Integer( 
				//		dm_gen.getDataSize() ).toString();
				string data_size = (new int?(dm_gen.getTotalNumberProblems())).ToString();
				string total_size = (new int?(dm_gen.getTotalChecked())).ToString();
				string[] toc_values = new string[] {dm.getTitle(), "Zero or Missing", data_size, total_size};
				// write the first data section (row)
				// this section has the general data check info and links
				writeTocDataSection(html, toc_values, i);
				data_size = (new int?(dm.getTotalNumberProblems())).ToString();
				total_size = (new int?(dm.getTotalChecked())).ToString();
				string[] toc_values2 = new string[] {dm.getTitle(), data_size, total_size};
				// write the second data section (row)
				// this section has the specific data check info and links
				writeTocDataSection(html, toc_values2, i);
			}
			html.tableEnd();
			html.horizontalRule();
		}
	}

	/// <summary>
	/// Writes the data portion of the table of contents.  This section contains
	/// a table with links to data checks. </summary>
	/// <param name="html"> HTMLWriter object. </param>
	/// <param name="toc"> Table of contents list data. </param>
	/// <param name="index"> Current index of the iteration of the data Vectors. </param>
	/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void writeTocDataSection(HTMLWriter html, String[] toc, int index) throws Exception
	private void writeTocDataSection(HTMLWriter html, string[] toc, int index)
	{
		if (html != null && toc != null)
		{
			// HTML anchors used to link table of contents to data sections
			string base_anchor = "#data" + index;
			string generic_anchor = "#generic" + index;
			// proplist for <td rowspan=2>
			// need this for formatting the data table
			PropList tdStart = new PropList("Table");
			tdStart.add("valign=bottom");
			tdStart.add("rowspan=2");
			// If it has 4 elements, this is the first row of data
			// because it contains the type of data or component name.
			// This row must be written differently since the column
			// has to span two rows.
			if (toc.Length == 4)
			{
				html.tableRowStart();
				for (int i = 0; i < toc.Length; i++)
				{
					switch (i)
					{
						case 0:
							html.tableCellStart(tdStart);
							html.addText(toc[i]);
							html.tableCellEnd();
						break;
						case 1:
							html.tableCellStart(__td_prop);
							html.link(generic_anchor, toc[i]);
							html.tableCellEnd();
						break;
						case 2:
							html.tableCellStart(__td_prop);
							html.link(generic_anchor, toc[i]);
							html.tableCellEnd();
						break;
						case 3:
							html.tableCellStart(__td_prop);
							html.addText(toc[i]);
							html.tableCellEnd();
						break;
						default:
							;
						break;
					}
				}
				html.tableRowEnd();
			}
			// If it has 3 elements then this is the second row
			// for this data element.  This row is formatted differently 
			// because it doesn't have a component name.
			else if (toc.Length == 3)
			{
				html.tableRowStart();
				for (int i = 0; i < toc.Length; i++)
				{
					switch (i)
					{
						case 0:
							html.tableCellStart(__td_prop);
							html.link(base_anchor, toc[i]);
							html.tableCellEnd();
						break;
						case 1:
							html.tableCellStart(__td_prop);
							html.link(base_anchor, toc[i]);
							html.tableCellEnd();
						break;
						case 2:
							html.tableCellStart(__td_prop);
							html.addText(toc[i]);
							html.tableCellEnd();
						break;
						default:
							;
						break;
					}
				}
				html.tableRowEnd();
			}
		}
	}

	}

}