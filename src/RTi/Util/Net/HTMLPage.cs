using System;
using System.Collections.Generic;
using System.IO;

// HTMLPage - interface to parse and create a HTML page

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

//------------------------------------------------------------------------------
// HTMLPage - interface to parse and create a HTML page
//------------------------------------------------------------------------------
// Copyright: See the COPYRIGHT file.
//------------------------------------------------------------------------------
// Notes:	For the most part, this class is used to print HTML pages, e.g.,
//		for CGI interfaces.  Be very careful using printStatus because
//		it may print to the HTML page!
//------------------------------------------------------------------------------
// History:
//
// 05 Feb 1999	Steven A. Malers, RTi	Initial version.
// 08 Mar 1999	SAM, RTi		Add form, table capabilities.
//------------------------------------------------------------------------------

namespace RTi.Util.Net
{

	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// This class provides an interface to parse and print HTML pages, e.g., for
	/// CGI programs.  Currently the class does not automatically detect errors (e.g.,
	/// it will not automatically insert a head if you print a body before the head).
	/// You must insert the page
	/// segments programatically by calling the proper methods.  For example, call using
	/// the following sequence:
	/// <para>
	/// <pre>
	/// String outfile = new String ( "CGI.out" );
	/// HTMLPage htmlpage = new HTMLPage ( outfile );
	/// htmlpage.printContentType();	// If doing CGI.
	/// htmlpage.printHTML(true);	// Start HTML.
	/// htmlpage.printHead(true);	// Start Head.
	/// htmlpage.printTitle("CGI Web Interface");
	///					// Title.
	/// htmlpage.printHead(false);	// End Head.
	/// htmlpage.printBody(true);	// Start body.
	/// htmlpage.printPreformattedText(true);
	///					// Start pre-formatted text.
	/// // Print content here using htmlpage.print(), etc..
	/// htmlpage.printPreformattedText(false);
	///					// End pre-formatted text.
	/// htmlpage.printBody(false);	// End body.
	/// htmlpage.printHTML(false);	// End HTML page.
	/// htmlpage.close();		// Close HTML file.
	/// </pre>
	/// </para>
	/// </summary>
	public class HTMLPage
	{

	// Data Members...

	private PrintWriter _out = null; // PrintWriter for output.
	private string _htmlfile = null; // HTML file for output.
	//private static String _nl = System.getProperty ( "line.separator" );
	private static string _nl = "\n";
						// Use the UNIX variant for now.

	// Constructors...

	/// <summary>
	/// Construct an HTMLPage given a PrintWriter to write the page. </summary>
	/// <param name="out"> PrintWriter to write HTML to. </param>
	/// <exception cref="IOException"> if the PrintWriter is null. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public HTMLPage(java.io.PrintWriter out) throws java.io.IOException
	public HTMLPage(PrintWriter @out)
	{
		if (@out == null)
		{
			string message = "PrintWriter for HTMLPage is null.";
			string routine = "HTMLPage(PrintWriter)";
			Message.printWarning(2, routine, message);
			throw new IOException(message);
		}
		_out = @out;
	}

	/// <summary>
	/// Construct an HTMLPage given the name of a file to write. </summary>
	/// <param name="htmlfile"> File to write HTML to. </param>
	/// <exception cref="IOException"> if the file name is null or cannot be opened. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public HTMLPage(String htmlfile) throws java.io.IOException
	public HTMLPage(string htmlfile)
	{
		string message, routine = "HTMLPage(String)";

		if (string.ReferenceEquals(htmlfile, null))
		{
			message = "HTML file name is null.";
			Message.printWarning(2, routine, message);
			throw new IOException(message);

		}
		else if (htmlfile.Equals("stdout"))
		{
			// Request that standard output is used for output...
			try
			{
				if (Message.isDebugOn)
				{
					Message.printDebug(1, routine, "Writing HTML to standard output.");
				}
				_out = new PrintWriter(System.out);
				_htmlfile = htmlfile;
			}
			catch (Exception)
			{
				message = "Cannot create PrintWriter for System.out.";
				Message.printWarning(2, routine, message);
				throw new IOException(message);
			}
		}
		else
		{
			try
			{
				if (Message.isDebugOn)
				{
					Message.printDebug(1, routine, "Writing HTML to file \"" + htmlfile + "\".");
				}
				_out = new PrintWriter(new StreamWriter(htmlfile));
				_htmlfile = htmlfile;
			}
			catch (Exception)
			{
				message = "Cannot open HTML file \"" + htmlfile + "\".";
				Message.printWarning(2, routine, message);
				throw new IOException(message);
			}
		}
	}

	/// <summary>
	/// Flush and close the page.
	/// </summary>
	public virtual void close()
	{
		if (_out != null)
		{
			_out.flush();
			_out.close();
		}
	}

	/// <summary>
	/// Clean up instance before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~HTMLPage()
	{
		// Only close the HTML file if it was originally opened with the name.
		// Otherwise, the code that created the instance may have other plans
		// for the PrintWriter.

		if (!string.ReferenceEquals(_htmlfile, null))
		{
			if (_out != null)
			{
				close();
			}
			_htmlfile = null;
		}
		_out = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Print text to the HTML page. </summary>
	/// <param name="string"> String to print. </param>
	public virtual void print(string @string)
	{
		if (!string.ReferenceEquals(@string, null))
		{
			_out.print(@string + _nl);
		}
	}

	/// <summary>
	/// Print text to the HTML page. </summary>
	/// <param name="strings"> list of Strings to print. </param>
	public virtual void print(IList<string> strings)
	{
		if (strings != null)
		{
			int size = strings.Count;
			string @string = null;
			for (int i = 0; i < size; i++)
			{
				@string = strings[i];
				if (!string.ReferenceEquals(@string, null))
				{
					_out.print(@string + _nl);
				}
			}
		}
	}

	/// <summary>
	/// Print a comment the HTML page. </summary>
	/// <param name="string"> String to print. </param>
	public virtual void printComment(string @string)
	{
		_out.print("<!-- ");
		if (!string.ReferenceEquals(@string, null))
		{
			_out.print(@string);
		}
		_out.print(" -->" + _nl);
	}

	/// <summary>
	/// Print a comment to the HTML page. </summary>
	/// <param name="strings"> list of String to print. </param>
	public virtual void printComment(IList<string> strings)
	{
		_out.print("<!--" + _nl);
		print(strings);
		_out.print("-->" + _nl);
	}

	/// <summary>
	/// Print the MIME text "Content-type:  text/html" to the HTML page.
	/// </summary>
	public virtual void printContentType()
	{
		_out.print("Content-type: text/html" + _nl + _nl);
	}

	/// <summary>
	/// Print a BODY tag. </summary>
	/// <param name="start"> If true, start the body.  If false, end the body. </param>
	public virtual void printBody(bool start)
	{
		if (start)
		{
			_out.print("<BODY>" + _nl);
		}
		else
		{
			_out.print("</BODY>" + _nl);
		}
	}

	/// <summary>
	/// Print a FORM tag. </summary>
	/// <param name="start"> If true, start form.  If false, end form. </param>
	public virtual void printForm(bool start)
	{
		printForm(start, "");
	}

	/// <summary>
	/// Print a FORM tag. </summary>
	/// <param name="start"> If true, start form.  If false, end form. </param>
	/// <param name="modifiers"> Modifiers to included in the FORM tag (e.g., "action=XX").
	/// You need to understand HTML syntax because the string is not checked for
	/// errors. </param>
	public virtual void printForm(bool start, string modifiers)
	{
		if (start)
		{
			if (!string.ReferenceEquals(modifiers, null))
			{
				if (!modifiers.Equals(""))
				{
					_out.print("<FORM " + modifiers + ">" + _nl);
				}
			}
			else
			{
				_out.print("<FORM>" + _nl);
			}
		}
		else
		{
			_out.print("</FORM>" + _nl);
		}
	}

	/// <summary>
	/// Print a HEAD tag. </summary>
	/// <param name="start"> If true, start the header.  If false, end the header. </param>
	public virtual void printHead(bool start)
	{
		if (start)
		{
			_out.print("<HEAD>" + _nl);
		}
		else
		{
			_out.print("</HEAD>" + _nl);
		}
	}

	/// <summary>
	/// Print an HTML tag. </summary>
	/// <param name="start"> If true, start the HTML page.  If false, end the HTML page. </param>
	public virtual void printHTML(bool start)
	{
		if (start)
		{
			_out.print("<HTML>" + _nl);
		}
		else
		{
			_out.print("</HTML>" + _nl);
		}
	}

	/// <summary>
	/// Print a PRE tag. </summary>
	/// <param name="start"> If true, start preformatted text.  If false, end preformatted
	/// text. </param>
	public virtual void printPreformattedText(bool start)
	{
		if (start)
		{
			_out.print("<PRE>" + _nl);
		}
		else
		{
			_out.print("</PRE>" + _nl);
		}
	}

	/// <summary>
	/// Print pre-formatted text. </summary>
	/// <param name="string"> String to print. </param>
	/// <param name="surround"> If true, surround with PRE HTML tags. </param>
	public virtual void printPreformattedText(string @string, bool surround)
	{
		if (surround)
		{
			_out.print("<PRE>" + _nl);
		}
		if (!string.ReferenceEquals(@string, null))
		{
			_out.print(@string + _nl);
		}
		if (surround)
		{
			_out.print("</PRE>" + _nl);
		}
	}

	/// <summary>
	/// Print pre-formatted text. </summary>
	/// <param name="strings"> Vector of strings to print. </param>
	/// <param name="surround"> If true, surround with PRE HTML tags. </param>
	public virtual void printPreformattedText(IList<string> strings, bool surround)
	{
		if (surround)
		{
			_out.print("<PRE>" + _nl);
		}
		print(strings);
		if (surround)
		{
			_out.print("</PRE>" + _nl);
		}
	}

	/// <summary>
	/// Print a SELECT tag. </summary>
	/// <param name="start"> If true, start select.  If false, end select. </param>
	public virtual void printSelect(bool start)
	{
		printSelect(start, "");
	}

	/// <summary>
	/// Print a SELECT tag. </summary>
	/// <param name="start"> If true, start select.  If false, end select. </param>
	/// <param name="modifiers"> Modifiers to included in the SELECT tag (e.g., "width=XX").
	/// You need to understand HTML syntax because the string is not checked for
	/// errors. </param>
	public virtual void printSelect(bool start, string modifiers)
	{
		if (start)
		{
			if (!string.ReferenceEquals(modifiers, null))
			{
				if (!modifiers.Equals(""))
				{
					_out.print("<SELECT " + modifiers + ">" + _nl);
				}
			}
			else
			{
				_out.print("<SELECT>" + _nl);
			}
		}
		else
		{
			_out.print("</SELECT>" + _nl);
		}
	}

	/// <summary>
	/// Print a TABLE tag. </summary>
	/// <param name="start"> If true, start table.  If false, end table. </param>
	public virtual void printTable(bool start)
	{
		printTable(start, "");
	}

	/// <summary>
	/// Print a TABLE tag. </summary>
	/// <param name="start"> If true, start table.  If false, end table. </param>
	/// <param name="modifiers"> Modifiers to included in the TABLE tag (e.g., "width=XX").
	/// You need to understand HTML syntax because the string is not checked for
	/// errors. </param>
	public virtual void printTable(bool start, string modifiers)
	{
		if (start)
		{
			if (!string.ReferenceEquals(modifiers, null))
			{
				if (!modifiers.Equals(""))
				{
					_out.print("<TABLE " + modifiers + ">" + _nl);
				}
			}
			else
			{
				_out.print("<TABLE>" + _nl);
			}
		}
		else
		{
			_out.print("</TABLE>" + _nl);
		}
	}

	/// <summary>
	/// Print a TD tag. </summary>
	/// <param name="start"> If true, start table cell.  If false, end table cell. </param>
	public virtual void printTableCell(bool start)
	{
		printTableCell(start, "");
	}

	/// <summary>
	/// Print a TD tag. </summary>
	/// <param name="start"> If true, start table cell.  If false, end table cell. </param>
	/// <param name="modifiers"> Modifiers to included in the TD tag (e.g., "width=XX").
	/// You need to understand HTML syntax because the string is not checked for
	/// errors. </param>
	public virtual void printTableCell(bool start, string modifiers)
	{
		if (start)
		{
			if (!string.ReferenceEquals(modifiers, null))
			{
				if (!modifiers.Equals(""))
				{
					_out.print("<TD " + modifiers + ">" + _nl);
				}
			}
			else
			{
				_out.print("<TD>" + _nl);
			}
		}
		else
		{
			_out.print("</TD>" + _nl);
		}
	}

	/// <summary>
	/// Print a TR tag. </summary>
	/// <param name="start"> If true, start table row.  If false, end table row. </param>
	public virtual void printTableRow(bool start)
	{
		printTableRow(start, "");
	}

	/// <summary>
	/// Print a TR tag. </summary>
	/// <param name="start"> If true, start table row.  If false, end table row. </param>
	/// <param name="modifiers"> Modifiers to included in the TR tag (e.g., "width=XX").
	/// You need to understand HTML syntax because the string is not checked for
	/// errors. </param>
	public virtual void printTableRow(bool start, string modifiers)
	{
		if (start)
		{
			if (!string.ReferenceEquals(modifiers, null))
			{
				if (!modifiers.Equals(""))
				{
					_out.print("<TR " + modifiers + ">" + _nl);
				}
			}
			else
			{
				_out.print("<TR>" + _nl);
			}
		}
		else
		{
			_out.print("</TR>" + _nl);
		}
	}

	/// <summary>
	/// Print a title to the HTML page.  If the title is null, an empty title is used.
	/// </summary>
	public virtual void printTitle(string title)
	{
		_out.print("<TITLE>");
		if (!string.ReferenceEquals(title, null))
		{
			_out.print(title);
		}
		_out.print("</TITLE>" + _nl);
	}

	} // End of HTMLPage class

}