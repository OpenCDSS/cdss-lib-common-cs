using System.Collections.Generic;
using System.IO;

// DataTableHtmlWriter - write a table to a simple HTML file

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

namespace RTi.Util.Table
{

	using HTMLWriter = RTi.Util.IO.HTMLWriter;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// Write a table to a simple HTML file.
	/// </summary>
	public class DataTableHtmlWriter
	{

	/// <summary>
	/// The first table to be compared.
	/// </summary>
	private DataTable __table;

	/// <summary>
	/// Create the instance and check for initialization problems. </summary>
	/// <param name="table"> the data table to be written </param>
	public DataTableHtmlWriter(DataTable table)
	{
		// The tables being compared must not be null
		if (table == null)
		{
			throw new InvalidParameterException("The  table to write is null.");
		}
		else
		{
			setTable(table);
		}
	}

	/// <summary>
	/// Return the first table being compared.
	/// </summary>
	private DataTable getTable()
	{
		return __table;
	}

	/// <summary>
	/// Set the first table being compared. </summary>
	/// <param name="table1"> first table being compared. </param>
	private void setTable(DataTable table1)
	{
		__table = table1;
	}

	/// <summary>
	/// Formats a table to an HTML string.  This currently is used only by writeHtmlFile(). </summary>
	/// <param name="writeHeader"> If true, the field names will be read from the fields 
	/// and written as a one-line header of field names.
	/// If all headers are missing, then the header line will not be written. </param>
	/// <param name="comments"> a list of Strings to put at the top of the file as comments. </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private String toHtml(boolean writeHeader, java.util.List<String> comments, int [][] styleMaskArray, String [] styles, String customStyleText) throws Exception
	private string toHtml(bool writeHeader, IList<string> comments, int[][] styleMaskArray, string[] styles, string customStyleText)
	{
		string routine = "DataTableWriter.toHtml";
		DataTable table = getTable();
		// Create an HTML writer
		string htmlTitle = "";
		HTMLWriter html = new HTMLWriter(null, htmlTitle, false);
		// Start the file and write the head section
		html.htmlStart();
		if ((string.ReferenceEquals(htmlTitle, null)) || htmlTitle.Length == 0)
		{
			htmlTitle = "Time Series Summary";
		}
		writeHtmlHead(html, htmlTitle, customStyleText);
		// Start the body section
		html.bodyStart();
		// Put the standard header at the top of the file
		//IOUtil.printCreatorHeader ( out, "#", 80, 0 );
		// If any comments have been passed in, print them at the top of the file
		/* TODO SAM 2010-12-07 Add comments later - they are messing with formatting
		if ( comments != null ) {
		    html.commentStart();
		    html.write( "Comments:\n" );
		    for ( String comment: comments ) {
		        html.write( html.encodeHTML(comment) + "\n" );
		    }
		    html.commentEnd();
		}
		*/

		int nCols = table.getNumberOfFields();
		if (nCols == 0)
		{
			Message.printWarning(3, routine, "Table has 0 columns!  Nothing will be written.");
		}
		else
		{
			//StringBuffer line = new StringBuffer();

			//int nonBlank = 0; // Number of nonblank table headings
			html.tableStart();
			if (writeHeader)
			{
				// First determine if any headers are non blank
				/*
				for (int col = 0; col < cols; col++) {
				    if ( getFieldName(col).length() > 0 ) {
				        ++nonBlank;
				    }
				}
				if ( nonBlank > 0 ) {
				    out.println ( "# Column headings are first line below, followed by data lines.");
				    line.setLength(0);
				    for (int col = 0; col < (cols - 1); col++) {
				        line.append( "\"" + getFieldName(col) + "\"" + delimiter);
				    }
				    line.append( "\"" + getFieldName((cols - 1)) + "\"");
				    out.println(line);
				}
				*/
				html.tableRowStart();
				string[] tableHeaders = new string[nCols];
				for (int i = 0; i < nCols; i++)
				{
					tableHeaders[i] = table.getFieldName(i);
				}
				html.tableHeaders(tableHeaders);
				html.tableRowEnd();
			}

			int rows = table.getNumberOfRecords();
			string cellString;
			int tableFieldType;
			int precision;
			PropList maskProps = new PropList(""); // Reused below to stylize cells
			object cellObject;
			for (int row = 0; row < rows; row++)
			{
				html.tableRowStart();
				for (int col = 0; col < nCols; col++)
				{
					tableFieldType = table.getFieldDataType(col);
					precision = table.getFieldPrecision(col);
					cellObject = table.getFieldValue(row,col);
					// TODO SAM 2010-12-18 Why not get the format from the table?
					if (cellObject == null)
					{
						cellString = "";
					}
					else if (((tableFieldType == TableField.DATA_TYPE_FLOAT) || (tableFieldType == TableField.DATA_TYPE_DOUBLE)) && (precision > 0))
					{
						// Format according to the precision if floating point
						cellString = StringUtil.formatString(cellObject,"%." + precision + "f");
					}
					else
					{
						// Use default formatting.
						cellString = "" + cellObject;
					}
					if ((styleMaskArray != null) && (styleMaskArray[row][col] > 0))
					{
						//Message.printStatus(2, routine, "string=\"" + cellString + "\" mask=" + styleMaskArray[row][col] +
					   //     " style=\"" + styles[styleMaskArray[row][col]] );
						maskProps.set("class",styles[styleMaskArray[row][col]]);
						// Cell matches a mask so use the property
						string[] cellAsArray = new string[] {cellString};
						html.tableCells(cellAsArray,maskProps);
					}
					else
					{
						// No special style/formatting for cell
						html.tableCell(cellString);
					}
				}
				html.tableRowEnd();
			}
			html.tableEnd();
		}
		html.bodyEnd();
		html.htmlEnd();
		html.closeFile();
		return html.getHTML();
	}

	//TODO SAM 2010-12-07 Evaluate passing in styles - for now just pick some defaults
	/// <summary>
	/// Writes a table to an HTML file. </summary>
	/// <param name="filename"> the file to write to. </param>
	/// <param name="writeHeader"> If true, the field names will be read from the fields 
	/// and written as a one-line header of field names.
	/// If all headers are missing, then the header line will not be written. </param>
	/// <param name="comments"> a list of Strings to put at the top of the file as comments. </param>
	/// <param name="styleMaskArray"> an array with integer values for each cell in the table.  These values will be used to look
	/// up styles from the "styles" array.  For example a styleMask value of "1" would match the style in styles[1], which
	/// would be the style name used in "customStyleText" text passed in (e.g, ".bad { background-color:yellow; }\n".
	/// This capability is being evaluated for effectiveness. </param>
	/// <param name="styles"> an array of styles associated with mask values as described above </param>
	/// <param name="customStyleText"> is text to be inserted in the "<style>" section of the HTML.  It should consist of lines
	/// as illustrated above. </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeHtmlFile(String filename, boolean writeHeader, java.util.List<String> comments, int [][] styleMaskArray, String [] styles, String customStyleText) throws java.io.IOException, Exception
	public virtual void writeHtmlFile(string filename, bool writeHeader, IList<string> comments, int[][] styleMaskArray, string[] styles, string customStyleText)
	{
		string routine = "DataTableWriter.writeHtmlFile";

		if (string.ReferenceEquals(filename, null))
		{
			Message.printWarning(2, routine, "Cannot write to null file.");
			throw new InvalidParameterException("Cannot write to null file.");
		}

		PrintWriter @out = null;
		try
		{
			@out = new PrintWriter(new StreamWriter(filename));
			@out.print(toHtml(writeHeader, comments, styleMaskArray, styles, customStyleText));
		}
		finally
		{
			if (@out != null)
			{
				@out.close();
			}
		}
	}

	/// <summary>
	/// Writes the start tags for the HTML check file. </summary>
	/// <param name="html"> HTMLWriter object. </param>
	/// <param name="title"> title for the document. </param>
	/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void writeHtmlHead(RTi.Util.IO.HTMLWriter html, String title, String customStyleText) throws Exception
	private void writeHtmlHead(HTMLWriter html, string title, string customStyleText)
	{
		if (html != null)
		{
			html.headStart();
			html.title(title);
			writeHtmlStyles(html, customStyleText);
			html.headEnd();
		}
	}

	/// <summary>
	/// Inserts the style attributes for a time series summary.
	/// This was copied from the TSHtmlFormatter since tables are used with time series also. </summary>
	/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void writeHtmlStyles(RTi.Util.IO.HTMLWriter html, String customStyleText) throws Exception
	private void writeHtmlStyles(HTMLWriter html, string customStyleText)
	{
		html.write("<style>\n" + "@media screen {\n" + "#titles { font-weight:bold; color:#303044 }\n" + "table { background-color:black; text-align:left; border:1; bordercolor:black; cellspacing:1; cellpadding:1 }\n" + "th { background-color:#333366; text-align:center; vertical-align:bottom; color:white }\n" + "tr { valign:bottom; halign:right }\n" + "td { background-color:white; text-align:right; vertical-align:bottom; font-style:normal; " + "font-family:courier; font-size:.75em }\n" + "body { text-align:left; font-size:12pt; }\n" + "pre { font-size:12pt; margin: 0px }\n" + "p { font-size:12pt; }\n" + "/* The following controls formatting of data values in tables */\n" + ".flagcell { background-color:lightgray; }\n" + ".missing { background-color:yellow; }\n" + ".flag { vertical-align: super; }\n" + ".flagnote { font-style:normal; font-family:courier; font-size:.75em; }\n");
		if ((!string.ReferenceEquals(customStyleText, null)) && !customStyleText.Equals(""))
		{
			html.write(customStyleText);
		}
		html.write("}\n" + "@media print {\n" + "#titles { font-weight:bold; color:#303044 }\n" + "table { border-collapse: collapse; background-color:white; text-align:left; border:1pt solid #000000; cellspacing:2pt; cellpadding:2pt }\n" + "th { background-color:white; text-align:center; vertical-align:bottom; color:black }\n" + "tr { valign:bottom; halign:right;  }\n" + "td { background-color:white; border: 1pt solid #000000; text-align:right; vertical-align:bottom; font-style:normal; " + "font-family:courier; font-size:11pt; padding: 2pt; }\n" + "body { text-align:left; font-size:11pt; }\n" + "pre { font-size:11pt; margin: 0px }\n" + "p { font-size:11pt; }\n" + "/* The following controls formatting of data values in tables */\n" + ".flagcell { background-color:lightgray; }\n" + ".missing { background-color:yellow; }\n" + ".flag { vertical-align: super; }\n" + ".flagnote { font-style:normal; font-family:courier; font-size:11pt; }\n");
		if ((!string.ReferenceEquals(customStyleText, null)) && !customStyleText.Equals(""))
		{
			html.write(customStyleText);
		}
		html.write("}\n" + "}\n" + "</style>\n");
	}

	}

}