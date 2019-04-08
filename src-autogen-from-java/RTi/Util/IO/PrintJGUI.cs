using System;
using System.Collections.Generic;

// PrintJGUI - print utility

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

//-----------------------------------------------------------------------------
// PrintJGUI - print utility
//-----------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//-----------------------------------------------------------------------------
// Notes:	(1)	This GUI accepts a vector of strings and prints the
//			strings to the printer.  Some formatting options are
//			available.
//		(2)	The debug messages in this code HAS been wrapped with
//			isDebugOn.
//-----------------------------------------------------------------------------
// History:
//
// 02 Dec 1997	Steven A. Malers, RTi	Copy ExportGUI, merge the example from
//					the O'Reilly AWT book, and implement
//					simple printing.
// 14 Mar 1998	SAM, RTi		Add Javadoc.
// 06 Apr 1998	SAM, RTi		Overload print to take a font size.
//					Still use the fixed-width font.
// 29 Apr 1998	SAM, RTi		Overload print to take a PropList and
//					convert the arguments to properties.
//					Also add a LinesPerPage property
//					to allow flexibility.
// 15 May 1998  DLG, RTi		Fixed bug introduced by SAM. Was not
//					checking the font size as set using
//					a PropList object.
// 13 Apr 1999	SAM, RTi		Add finalize.
// 21 May 1999	SAM, RTi		Add buffer around page so that the
//					printing is not cut off by non-printable
//					area.
// 12 Oct 2000	SAM, RTi		Trim the end of lines that are printed -
//					newlines from a TextArea seem to cause
//					junk ? characters at the end of output
//					lines.  When printing from a TextArea,
//					use an 8 point font.
// 26 Apr 2001	SAM, RTi		Change so that if a one-element Vector
//					is being printed, split the one line
//					by newlines for printing.
//-----------------------------------------------------------------------------
// 2003-06-05	J. Thomas Sapienza, RTi	Implemented SWING versions of the
//					print code.  Original print code works
//					just fine with Swing components for
//					printing text, so simply converted the
//					old Frames to JFrames, TextAreas to
//					JTextAreas, etc.
// 2005-11-16	JTS, RTi		Added JDialog support.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//-----------------------------------------------------------------------------

namespace RTi.Util.IO
{



	using Message = RTi.Util.Message.Message;

	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This class provides a simple process for printing a vector of strings.  It is
	/// implemented similar to the ExportGUI class.  Printing is accomplished by calling
	/// a static print() method, which displays the standard print dialog for the
	/// platform.  In the future, the PrintJGUI class will provide a GUI to set output
	/// attributes, but Java provides limited print control capabilities as of version
	/// 1.1.  A 10-point Courier font is used by default for printing to preserve
	/// spacing and to
	/// allow a fairly wide output to be printed.  60-line output is assumed at this
	/// time since the 1.1 toolkit does not guarantee that requesting the page size
	/// will return a valid answer.
	/// <para>
	/// 
	/// <b>
	/// At this time, printing to the local machine is always attempted.  Although it
	/// is possible to expor to a browser and then print from the browser, this process
	/// is cumbersome and is not supported.
	/// </b>
	/// </para>
	/// </summary>
	public class PrintJGUI
	{

	private static int LEFT_BORDER = 36; // Left border in points
	private static int TOP_BORDER = 15; // Top border in points
	//private static int	BOTTOM_BORDER = 15;	// Bottom border in points

	private static JDialog _parentJDialog;
	private static IList<string> _export; // List containing the formatted data to print
	private static int _default_fontsize = 10;
							// Default font size.
	private static int _default_lines_per_page = 60;
							// Default lines per page.
	private static int _fontsize = _default_fontsize;
							// Font size.
	private static int _lines_per_page = _default_lines_per_page;
							// Lines per page.

	/* support later... 
	
	private static boolean          _isApplet;	// true if running an Applet,
							// false otherwise
	private static AppletContext    _appletContext; // current applet context
	private static URL              _documentBase;	// complete URL of the HTML
							// file that loaded the applet.
	*/

	/// <summary>
	/// Finalize before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~PrintJGUI()
	{
		_export = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Print to the local printer given the calling Frame and a vector of String's to
	/// print.  The default 10-point font is used. </summary>
	/// <param name="parent"> JFrame from which printing occurs. </param>
	/// <param name="export"> list of String's to print. </param>
	public static void print(JFrame parent, IList<string> export)
	{
		print(parent, export, null);
	}

	/// <summary>
	/// Print to the local printer given the calling Frame, a vector of String's to
	/// print, and the font size. </summary>
	/// <param name="parent"> JFrame from which printing occurs. </param>
	/// <param name="export"> list of String's to print. </param>
	/// <param name="fontsize"> Font size to use (points). </param>
	public static void print(JFrame parent, IList<string> export, int fontsize)
	{
		PropList props = new PropList("PrintJGUI");
		props.set("FontSize", "" + fontsize);
		print(parent, export, props);
	}

	/// <summary>
	/// Print to the local printer given the calling Frame, a vector of String's to
	/// print, and a help key to be used with RTi.Util.Help.URLHelp (however, help is
	/// not supported at this time because the standard print dialog is used). </summary>
	/// <param name="parent"> JFrame from which printing occurs. </param>
	/// <param name="export"> list of String's to print. </param>
	/// <param name="helpkey"> Help key to use with URLHelp. </param>
	/// <param name="fontsize"> Font size for output (font is still fixed-width Courier). </param>
	/// <seealso cref= RTi.Util.Help.URLHelp </seealso>
	public static void print(JFrame parent, IList<string> export, string helpkey, int fontsize)
	{
		PropList props = new PropList("PrintJGUI");
		props.set("HelpKey", helpkey);
		props.set("FontSize", "" + fontsize);
		print(parent, export, props);
	}

	/// <summary>
	/// Print to the local printer given the calling JFrame, a vector of String's to
	/// print, and a PropList containing modifiers.  
	/// The help key to be used with RTi.Util.Help.URLHelp (however, help is
	/// not supported at this time because the standard print dialog is used). </summary>
	/// <param name="parent"> JFrame from which printing occurs. </param>
	/// <param name="export"> list of String's to print. </param>
	/// <param name="proplist"> PropList of properties to modify output.  Valid properties are
	/// as shown below:
	/// <para>
	/// 
	/// <table width=100% cellpadding=2 cellspacing=0 border=2>
	/// <tr>
	/// <td><b>Property</b></td>   <td><b>Description</b></td>   <td><b>Default</b></td>
	/// </tr
	/// 
	/// <tr>
	/// <td><b>Font  </b></td>
	/// <td>The font to use for printing.  <b>This is currently ignored (always set
	/// to the default.</b></td>
	/// <td>Courier (Plain)</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>FontSize</b></td>
	/// <td>The font size for printing (points).</td>
	/// <td>10</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>LinesPerPage</b></td>
	/// <td>The number of lines per page.
	/// <b>If this is specified, the font height will be determined by dividing
	/// the page height by the number of lines.</b></td>
	/// <td>60</td>
	/// </tr>
	/// </table>
	/// </para>
	/// <para>
	/// 
	/// </para>
	/// </param>
	/// <seealso cref= RTi.Util.Help.URLHelp </seealso>
	public static void print(JFrame parent, IList<string> export, PropList proplist)
	{
		string routine = "PrintJGUI.export";

		_export = export;
		// Do some checks.  It is possible that the Vector has one string that itself needs to be parsed...
		if ((export != null) && (export.Count == 1))
		{
			_export = StringUtil.breakStringList((string)export[0], "\n", 0);
		}
	/* support later.
		_isApplet 	= isApplet;               
		_appletContext 	= appletContext;     
		_documentBase 	= documentBase;  
	*/

		//
		// determine if data exist in the Vector, if not, issue a warning.
		//
		if (_export == null || _export.Count == 0)
		{
			Message.printWarning(1, routine, "No text to print");
			return;
		}

		// If the PropList is null, initialize one so we don't have to
		// constantly check for nulls...

		PropList props = null;
		if (proplist == null)
		{
			props = new PropList("PrintJGUI");
		}
		else
		{
			props = proplist;
		}

		// Now get the properties for use here...

		string prop_value = null;
		prop_value = props.getValue("LinesPerPage");
		if (!string.ReferenceEquals(prop_value, null))
		{
			_lines_per_page = StringUtil.atoi(prop_value);
			if (_lines_per_page == 0)
			{
				_lines_per_page = _default_lines_per_page;
			}
		}

		prop_value = props.getValue("FontSize");
		if (!string.ReferenceEquals(prop_value, null))
		{
			_fontsize = StringUtil.atoi(prop_value);
		}

		// We always try to print to the local machine.  If we are trusted, we
		// do it.  Otherwise, we try to write to the server if an applet.
		try
		{
			printToLocalPrinter(parent);
		}
		catch (IOException)
		{
			//
			// Try export to browser if running from an Applet.
			//

	/* Later we might allow output to a browser...
	
			if ( isApplet ) {
				if ( Message.isDebugOn ) {
					Message.printDebug( 10, routine,
					"Going to exportToBrowser()" );
				}
				try {	exportToBrowser();
				}
				catch ( IOException e2 ) {
					Message.printWarning ( 1, routine,
					"Cannot export to web server");
				}
			}
	*/

		}
	}

	//-----------------------------------------------------------------------------
	//  Notes:
	//  (1)This function accepts a  TextArea. 
	//  (2)calls getListContents( MultiList list ) to get a formatted export Vector
	//  (3)calls export( Frame parent, Vector export )
	//-----------------------------------------------------------------------------
	//  Variables:	I/O	Description
	//  (I)parent - parent Frame object from which this call was made.
	//  (I)satus_TextField - status TextField to display statis to. May be null
	//  (I)textArea - TextArea object
	//-----------------------------------------------------------------------------                        
	/// <summary>
	/// Print to the local printer given the calling JFrame, a JTextArea to
	/// print, and a status JTextField. </summary>
	/// <param name="parent"> JFrame from which printing occurs. </param>
	/// <param name="status_TextField"> JTextField to receive status messages. </param>
	/// <param name="textArea"> JTextArea to print. </param>
	public static void printJTextAreaObject(JFrame parent, JTextField status_TextField, JTextArea textArea)
	{
		string statusString; // contains status information
		string routine = "PrintJGUI.printTextAreaObject()";

		// display status
		statusString = "Exporting query results...";
		parent.setCursor(new Cursor(Cursor.WAIT_CURSOR));
		if (status_TextField != null)
		{
			status_TextField.setText(statusString);
		}
		Message.printStatus(1, routine, statusString);

		/* This does not work because getText returns a newline-delimited
		** string (one big string) and we need carriage returns on the PC!
		** SAM submitted a bug report (feature request) to JavaSoft on
		** 12 Nov 1997
	
		// get the formatted export Vector for the list object
		Vector export_Vector = new Vector( 10, 5);
			
		export_Vector.addElement( textArea.getText() );
		**
		** So use the following instead...
		*/

		IList<string> exportList = StringUtil.breakStringList(textArea.getText(), "\n", 0);

		print(parent, exportList, 8); // Use small font
		exportList = null;

		// display status
		statusString = "Finished printing.";
		parent.setCursor(new Cursor(Cursor.DEFAULT_CURSOR));
		if (status_TextField != null)
		{
			status_TextField.setText(statusString);
		}
		Message.printStatus(1, routine, statusString);
	}

	//-----------------------------------------------------------------------------
	// Notes:	(1)	This function prints the _export Vector to a local
	//			printer if running an application.
	//		(2)	The following comments apply to the code used in this
	//			function.  The code has been modified from the original:
	//
	//				This example is from the book _Java AWT
	//				Reference_ by John Zukowski.   Written by John
	//				Zukowski.  Copyright (c) 1997 O'Reilly &
	//				Associates.  You may study, use, modify, and
	//				distribute this example for any purpose.
	//				This example is provided WITHOUT WARRANTY
	//				either expressed or
	//		(3)	At this time, it seems to be difficult to really know
	//			how big the page is that we are printing to.  The
	//			page height that we use here may result in extra white
	//			space on each page.
	//-----------------------------------------------------------------------------
	//  Variables:	I/O	Description
	//--------------------------------------------------------------------------
	/// <summary>
	/// A utility routine used internally in this class.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void printToLocalPrinter(javax.swing.JFrame parent) throws java.io.IOException
	private static void printToLocalPrinter(JFrame parent)
	{
		string routine = "PrintJGUI.printToLocalPrinter";
		int dl = 20;

		// First check to see if we are allowed to print...

		if (!SecurityCheck.canPrint())
		{
			Message.printWarning(1, routine, "This application does not have sufficient permissions\n" + "to print to the local machine.  Please configure as a\n" + "trusted application using your browser security settings.\n");
			throw (new IOException("Security violation"));
		}

		// Get a print job...

		PrintJob pjob = parent.getToolkit().getPrintJob(parent, "Printing...", (Properties)null);
		if (pjob == null)
		{
			/* For now, ignore because the user can cancel...
			Message.printWarning ( 1, routine, "Unable to start print job" );
			throw ( new IOException ( "Security violation" ) );
			*/
			return; // Just return as if we did it.
		}
			Graphics pg = pjob.getGraphics();
			if (pg == null)
			{
			Message.printWarning(1, routine, "Unable to get graphics handle for print job");
			throw (new IOException("Security violation"));
			}

		// Print strings to graphics via printjob.  Does not deal with word wrap or tabs

		int pageNum = 1;
		int linesForThisPage = 0;
		//int linesForThisJob = 0;

		string nextLine;

		// Get the printable page height...

		int pageHeight = pjob.getPageDimension().height;
		int printable_pageHeight = pageHeight - TOP_BORDER; // Maximum coordinate (NOT PRINTABLE HEIGHT)
		int pageWidth = pjob.getPageDimension().width;

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Page dimensions are: width=" + pageWidth + " height=" + pageHeight);
		}

		// Have to set the font to get any output.  For now use a fixed-width
		// font.  If the number of lines were specified, scale the font
		// accordingly.  Otherwise, use the requested font size.

	/* For now always go with the font size.
		if ( _lines_per_page_specified ) {
			_fontsize = pageHeight/_lines_per_page;
		}
	*/
		// Else, font size has been specified or is defaulted...

		//Font helv = new Font("Helvetica", Font.PLAIN, 12);
		Font font = new Font("Courier", Font.PLAIN, _fontsize);

		pg.setFont(font);
		FontMetrics fm = pg.getFontMetrics(font);
		int fontHeight = fm.getHeight();
		int fontDescent = fm.getDescent();
		int curHeight = TOP_BORDER;

		// Print the lines.  Remember that y=0 at the top of the page.

		int size = _export.Count;
		try
		{
			for (int i = 0; i < size; i++)
			{
				// Don't do a trim() here because it may shift the line if there are leading spaces...
				nextLine = StringUtil.removeNewline((string)_export[i]);
				if ((curHeight + fontHeight) > printable_pageHeight)
				{
					// New Page
					if (Message.isDebugOn)
					{
						Message.printDebug(30, routine, "" + linesForThisPage + " lines printed for page " + pageNum);
					}
					pageNum++;
					linesForThisPage = 0;
					pg.dispose();
					pg = pjob.getGraphics();
						if (pg == null)
						{
						Message.printWarning(1, routine, "Unable to get graphics handle for print job");
						throw (new IOException("Security violation"));
						}
					pg.setFont(font);
					curHeight = TOP_BORDER;
				}
				curHeight += fontHeight;
				// Printing does not seem to like empty strings...
				if ((!string.ReferenceEquals(nextLine, null)) && (nextLine.Length > 0))
				{
					pg.drawString(nextLine, LEFT_BORDER, (curHeight - fontDescent));
				}
				linesForThisPage++;
				//linesForThisJob++;
			}
		}
		catch (Exception t)
		{
			Console.WriteLine(t.ToString());
			Console.Write(t.StackTrace);
		}

		// Now get rid of the graphics and end the job...

		pg.dispose();
		pjob.end();
	}

	/// <summary>
	/// Print to the local printer given the calling Frame and a vector of String's to
	/// print.  The default 10-point font is used. </summary>
	/// <param name="parent"> JDialog from which printing occurs. </param>
	/// <param name="export"> list of String to print. </param>
	public static void print(JDialog parent, IList<string> export)
	{
		print(parent, export, null);
	}

	/// <summary>
	/// Print to the local printer given the calling Frame, a vector of String's to print, and the font size. </summary>
	/// <param name="parent"> JDialog from which printing occurs. </param>
	/// <param name="export"> list of String to print. </param>
	/// <param name="fontsize"> Font size to use (points). </param>
	public static void print(JDialog parent, IList<string> export, int fontsize)
	{
		PropList props = new PropList("PrintJGUI");
		props.set("FontSize", "" + fontsize);
		print(parent, export, props);
	}

	/// <summary>
	/// Print to the local printer given the calling Frame, a vector of String's to
	/// print, and a help key to be used with RTi.Util.Help.URLHelp (however, help is
	/// not supported at this time because the standard print dialog is used). </summary>
	/// <param name="parent"> JDialog from which printing occurs. </param>
	/// <param name="export"> list of String to print. </param>
	/// <param name="helpkey"> Help key to use with URLHelp. </param>
	/// <param name="fontsize"> Font size for output (font is still fixed-width Courier). </param>
	/// <seealso cref= RTi.Util.Help.URLHelp </seealso>
	public static void print(JDialog parent, IList<string> export, string helpkey, int fontsize)
	{
		PropList props = new PropList("PrintJGUI");
		props.set("HelpKey", helpkey);
		props.set("FontSize", "" + fontsize);
		print(parent, export, props);
	}

	/// <summary>
	/// Print to the local printer given the calling JDialog, a vector of String's to
	/// print, and a PropList containing modifiers.  
	/// The help key to be used with RTi.Util.Help.URLHelp (however, help is
	/// not supported at this time because the standard print dialog is used). </summary>
	/// <param name="parent"> JDialog from which printing occurs. </param>
	/// <param name="export"> list of String's to print. </param>
	/// <param name="proplist"> PropList of properties to modify output.  Valid properties are
	/// as shown below:
	/// <para>
	/// 
	/// <table width=100% cellpadding=2 cellspacing=0 border=2>
	/// <tr>
	/// <td><b>Property</b></td>   <td><b>Description</b></td>   <td><b>Default</b></td>
	/// </tr
	/// 
	/// <tr>
	/// <td><b>Font  </b></td>
	/// <td>The font to use for printing.  <b>This is currently ignored (always set
	/// to the default.</b></td>
	/// <td>Courier (Plain)</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>FontSize</b></td>
	/// <td>The font size for printing (points).</td>
	/// <td>10</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>LinesPerPage</b></td>
	/// <td>The number of lines per page.
	/// <b>If this is specified, the font height will be determined by dividing
	/// the page height by the number of lines.</b></td>
	/// <td>60</td>
	/// </tr>
	/// </table>
	/// </para>
	/// <para>
	/// 
	/// </para>
	/// </param>
	/// <seealso cref= RTi.Util.Help.URLHelp </seealso>
	public static void print(JDialog parent, IList<string> export, PropList proplist)
	{
		string routine = "PrintJGUI.export";

		_parentJDialog = parent;
		_export = export;
		// Do some checks.  It is possible that the Vector has one string that
		// itself needs to be parsed...
		if ((export != null) && (export.Count == 1))
		{
			_export = StringUtil.breakStringList((string)export[0], "\n", 0);
		}
	/* support later.
		_isApplet 	= isApplet;               
		_appletContext 	= appletContext;     
		_documentBase 	= documentBase;  
	*/

		//
		// determine if data exist in the Vector, if not, issue a warning.
		//
		if (_export == null || _export.Count == 0)
		{
			Message.printWarning(1, routine, "No text to print");
			return;
		}

		// If the PropList is null, initialize one so we don't have to
		// constantly check for nulls...

		PropList props = null;
		if (proplist == null)
		{
			props = new PropList("PrintJGUI");
		}
		else
		{
			props = proplist;
		}

		// Now get the properties for use here...

		string prop_value = null;
		prop_value = props.getValue("LinesPerPage");
		if (!string.ReferenceEquals(prop_value, null))
		{
			_lines_per_page = StringUtil.atoi(prop_value);
			if (_lines_per_page == 0)
			{
				_lines_per_page = _default_lines_per_page;
			}
		}

		prop_value = props.getValue("FontSize");
		if (!string.ReferenceEquals(prop_value, null))
		{
			_fontsize = StringUtil.atoi(prop_value);
		}

		// We always try to print to the local machine.  If we are trusted, we
		// do it.  Otherwise, we try to write to the server if an applet.
		try
		{
			printToLocalPrinter(parent);
		}
		catch (IOException)
		{
			//
			// Try export to browser if running from an Applet.
			//

	/* Later we might allow output to a browser...
	
			if ( isApplet ) {
				if ( Message.isDebugOn ) {
					Message.printDebug( 10, routine,
					"Going to exportToBrowser()" );
				}
				try {	exportToBrowser();
				}
				catch ( IOException e2 ) {
					Message.printWarning ( 1, routine,
					"Cannot export to web server");
				}
			}
	*/

		}
	}

	//-----------------------------------------------------------------------------
	//  Notes:
	//  (1)This function accepts a  TextArea. 
	//  (2)calls getListContents( MultiList list ) to get a formatted export Vector
	//  (3)calls export( Frame parent, Vector export )
	//-----------------------------------------------------------------------------
	//  Variables:	I/O	Description
	//  (I)parent - parent Frame object from which this call was made.
	//  (I)satus_TextField - status TextField to display statis to. May be null
	//  (I)textArea - TextArea object
	//-----------------------------------------------------------------------------                        
	/// <summary>
	/// Print to the local printer given the calling JDialog, a JTextArea to
	/// print, and a status JTextField. </summary>
	/// <param name="parent"> JDialog from which printing occurs. </param>
	/// <param name="status_TextField"> JTextField to receive status messages. </param>
	/// <param name="textArea"> JTextArea to print. </param>
	public static void printJTextAreaObject(JDialog parent, JTextField status_TextField, JTextArea textArea)
	{
		string statusString; // contains status information
		string routine = "PrintJGUI.printTextAreaObject()";

		// display status
		statusString = "Exporting query results...";
		parent.setCursor(new Cursor(Cursor.WAIT_CURSOR));
		if (status_TextField != null)
		{
			status_TextField.setText(statusString);
		}
		Message.printStatus(1, routine, statusString);

		/* This does not work because getText returns a newline-delimited
		** string (one big string) and we need carriage returns on the PC!
		** SAM submitted a bug report (feature request) to JavaSoft on
		** 12 Nov 1997
	
		// get the formatted export Vector for the list object
		Vector export_Vector = new Vector( 10, 5);
			
		export_Vector.addElement( textArea.getText() );
		**
		** So use the following instead...
		*/

		IList<string> exportList = StringUtil.breakStringList(textArea.getText(), "\n", 0);

		print(parent, exportList, 8); // Use small font
		exportList = null;

		// display status
		statusString = "Finished printing.";
		parent.setCursor(new Cursor(Cursor.DEFAULT_CURSOR));
		if (status_TextField != null)
		{
			status_TextField.setText(statusString);
		}
		Message.printStatus(1, routine, statusString);
	}

	//-----------------------------------------------------------------------------
	// Notes:	(1)	This function prints the _export Vector to a local
	//			printer if running an application.
	//		(2)	The following comments apply to the code used in this
	//			function.  The code has been modified from the original:
	//
	//				This example is from the book _Java AWT
	//				Reference_ by John Zukowski.   Written by John
	//				Zukowski.  Copyright (c) 1997 O'Reilly &
	//				Associates.  You may study, use, modify, and
	//				distribute this example for any purpose.
	//				This example is provided WITHOUT WARRANTY
	//				either expressed or
	//		(3)	At this time, it seems to be difficult to really know
	//			how big the page is that we are printing to.  The
	//			page height that we use here may result in extra white
	//			space on each page.
	//-----------------------------------------------------------------------------
	//  Variables:	I/O	Description
	//--------------------------------------------------------------------------
	/// <summary>
	/// A utility routine used internally in this class.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void printToLocalPrinter(javax.swing.JDialog parent) throws java.io.IOException
	private static void printToLocalPrinter(JDialog parent)
	{
		string routine = "PrintJGUI.printToLocalPrinter";
		int dl = 20;

		// First check to see if we are allowed to print...

		if (!SecurityCheck.canPrint())
		{
			Message.printWarning(1, routine, "This application does not have sufficient permissions\n" + "to print to the local machine.  Please configure as a\n" + "trusted application using your browser security settings.\n");
			throw (new IOException("Security violation"));
		}

		// Get a print job...

		JFrame frame = new JFrame();
		java.awt.Dimension d = _parentJDialog.getSize();
		frame.setSize(d.width, d.height);
		frame.setVisible(true);
		RTi.Util.GUI.JGUIUtil.center(frame);
		frame.setVisible(false);
		string program_name = IOUtil.getProgramName();
		string jobname = "";
		if ((string.ReferenceEquals(program_name, null)) || program_name.Equals(""))
		{
			jobname = IOUtil.getProgramUser() + " print job";
		}
		else
		{
			jobname = IOUtil.getProgramUser() + " " + program_name + " print job.";
		}
		PrintJob pjob = parent.getToolkit().getPrintJob(frame, jobname, (Properties)null);
		frame.dispose();
		if (pjob == null)
		{
			/* For now, ignore because the user can cancel...
			Message.printWarning ( 1, routine, "Unable to start print job" );
			throw ( new IOException ( "Security violation" ) );
			*/
			return; // Just return as if we did it.
		}
			Graphics pg = pjob.getGraphics();
			if (pg == null)
			{
			Message.printWarning(1, routine, "Unable to get graphics handle for print job");
			throw (new IOException("Security violation"));
			}

		// Print strings to graphics via printjob.  Does not deal with word wrap or tabs

		int pageNum = 1;
		int linesForThisPage = 0;
		//int linesForThisJob = 0;

		string nextLine;

		// Get the printable page height...

		int pageHeight = pjob.getPageDimension().height;
		int printable_pageHeight = pageHeight - TOP_BORDER; // Maximum coordinate (NOT PRINTABLE HEIGHT)
		int pageWidth = pjob.getPageDimension().width;

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Page dimensions are: width=" + pageWidth + " height=" + pageHeight);
		}

		// Have to set the font to get any output.  For now use a fixed-width
		// font.  If the number of lines were specified, scale the font
		// accordingly.  Otherwise, use the requested font size.

	/* For now always go with the font size.
		if ( _lines_per_page_specified ) {
			_fontsize = pageHeight/_lines_per_page;
		}
	*/
		// Else, font size has been specified or is defaulted...

		//Font helv = new Font("Helvetica", Font.PLAIN, 12);
		Font font = new Font("Courier", Font.PLAIN, _fontsize);

		pg.setFont(font);
		FontMetrics fm = pg.getFontMetrics(font);
		int fontHeight = fm.getHeight();
		int fontDescent = fm.getDescent();
		int curHeight = TOP_BORDER;

		// Print the lines.  Remember that y=0 at the top of the page.

		int size = _export.Count;
		try
		{
			for (int i = 0; i < size; i++)
			{
				// Don't do a trim() here because it may shift the line if there are leading spaces...
				nextLine = StringUtil.removeNewline((string)_export[i]);
				if ((curHeight + fontHeight) > printable_pageHeight)
				{
					// New Page
					if (Message.isDebugOn)
					{
						Message.printDebug(30, routine, "" + linesForThisPage + " lines printed for page " + pageNum);
					}
					pageNum++;
					linesForThisPage = 0;
					pg.dispose();
					pg = pjob.getGraphics();
						if (pg == null)
						{
						Message.printWarning(1, routine, "Unable to get graphics handle for print job");
						throw (new IOException("Security violation"));
						}
					pg.setFont(font);
					curHeight = TOP_BORDER;
				}
				curHeight += fontHeight;
				// Printing does not seem to like empty strings...
				if ((!string.ReferenceEquals(nextLine, null)) && (nextLine.Length > 0))
				{
					pg.drawString(nextLine, LEFT_BORDER,(curHeight - fontDescent));
				}
				linesForThisPage++;
				//linesForThisJob++;
			}
		}
		catch (Exception t)
		{
			Console.WriteLine(t.ToString());
			Console.Write(t.StackTrace);
		}

		// Now get rid of the graphics and end the job...

		pg.dispose();
			pjob.end();
	}

	/* Make available later...
	
	//-----------------------------------------------------------------------------
	//  Notes:
	//  This function exports the _export Vector to a BROWSER page if running an 
	//  Applet.
	//-----------------------------------------------------------------------------
	//  Variables:	I/O	Description
	//-----------------------------------------------------------------------------                        
	private static void exportToBrowser() throws IOException
	{
		Socket	s = null;
		String	fileName="", response, routine="HBExport.exportToBrowser", 
			//server="arkansas.riverside.com";
			server=HBSource.getDBHost();
	
		try {
			s = new Socket( server, 5150 );
		}catch ( IOException e ){
			Message.printWarning( 1, routine,
			"Unable to connect to server: " + server );
			return;
		}
	
		if ( Message.isDebugOn ) {
			Message.printDebug( 2, routine,
			"Successfully established connection with: " + server );
		}
	
		try {
			PrintStream out;
			DataInputStream din;
			out = new PrintStream(s.getOutputStream());
			din = new DataInputStream(s.getInputStream());
	
			//
			// Send the Password across.
			//
			out.print("ex&or!\n");
			//
			// Wait for the response.
			//
			response = din.readLine();
	
			if( !response.equals( "Password OK" ) ){
				Message.printWarning( 1, routine,
			"Invalid response \"" + response + "\" retured." );
				throw( 
				new IOException( "Password not accepted." ) );
			}
			//
			// Build the file name out of the program name, 
			// user name, and a time stamp.
			//
			String date = "{0,date,yyyy.MM.dd.HH.mm.ss}";
			MessageFormat mf = new MessageFormat( date );
			Object[] o = new Object [ 1 ];
			o[0]	= new Date();
	
			fileName = 	
				HBGUIApp.getProgramName() + "." +
				HBGUIApp.getLogin() + "." +
				mf.format(o).toString() + ".txt";
	
			if ( Message.isDebugOn ) {
				Message.printDebug( 10, routine,
				"Sending across file name \"" + fileName + "\"." );
			}
			//
			// Send the file name.
			//
			out.print(  fileName + "\n" );
			//
			// Wait for the response
			//
			response = din.readLine();
	
			if( !response.equals( "Filename OK" ) ){
				Message.printStatus(1, routine,
			"Invalid response \"" + response + "\" retured." );
				throw( 
				new IOException( "Filename not accepted." ) );
			}
			if ( Message.isDebugOn ) {
				Message.printDebug( 10, routine,
				"Printing contents of Vector to file." );
			}
	
			for( int i=0; i<_export.size(); i++ ){
				out.print( _export.elementAt(i).toString() );
			}
	
		}catch( IOException e ){
		}finally {
			try {
				s.close();
			}
			catch ( IOException ie ){
			}
		}
	
		//
		// Send new page to the parent window.
		//
		try {
			String string = "http://" + server + "/tmp/" + fileName;
			URL url = new URL( string );
			if ( Message.isDebugOn ) {
				Message.printDebug( 10, routine,
				"Attempting to show document: " + string );
			}
	
			_appletContext.showDocument( url, "_blank" );
		}
		catch (MalformedURLException Excep) {
			Message.printWarning( 1, routine,
			"Problem showing exported page to browser!" );
			Message.printWarning( 1, routine, Excep.toString() );
			return;
		}
	}
	*/

	/* Enable later.  This is specific to a Symantec multilist...
	//-----------------------------------------------------------------------------
	//  Notes:
	//  (1)This function accepts a  MultiList object. 
	//  (2)calls getListContents( MultiList list ) to get a formatted export Vector
	//  (3)calls export( Frame parent, Vector export )
	//-----------------------------------------------------------------------------
	//  Variables:	I/O	Description
	//  (I)parent - parent Frame object from which this call was made.
	//  (I)satus_TextField - status TextField to display statis to. May be null
	//  (I)list - MultiList object
	//-----------------------------------------------------------------------------                        
	public static void exportListObject (	Frame parent,
						TextField status_TextField,
						MultiList list )
	{	String 	statusString;		// contains status information
	        int 	numRows;		// number of list rows  	
				
		numRows = list.getNumberOfRows();
	
		// display status
		statusString = "Exporting query results...";		   
		parent.setCursor( new Cursor(Cursor.WAIT_CURSOR) );
		if ( status_TextField != null ) {
			status_TextField.setText( statusString );
		}
		Message.printStatus( 1, "HBExportGUI.exportListObject()", statusString );        
	      
		// get the formatted export Vector for the list object
		Vector export_Vector = new Vector( numRows, 5);
		getListContents( list, export_Vector );
	
		// export to file/browser page     
		export( parent, export_Vector );
	
		// display status
		statusString = "Finished exporting.";		   
		parent.setCursor( new Cursor(Cursor.DEFAULT_CURSOR) );
		if ( status_TextField != null ) {
			status_TextField.setText( statusString );
		}
		Message.printStatus( 1, "HBExportGUI.exportListObject()", statusString );        
	}
	
	/* Maybe support this later - this is specific to a Symantec multilst.
	
	//-----------------------------------------------------------------------------
	//  Notes:
	//  (1)This function accepts two MultiList objects. 
	//  (2)calls getListContents( MultiList list ) to get a formatted export Vector
	//  (3)calls export( Frame parent, Vector export )
	//-----------------------------------------------------------------------------
	//  Variables:	I/O	Description
	//  (I)parent - parent Frame object from which this call was made.
	//  (I)satus_TextField - status TextField to display statis to. May be null
	//  (I)first_list - MultiList object
	//  (I)second_list - MultiList object
	//-----------------------------------------------------------------------------                        
	public static void exportListObject (	Frame parent,
						TextField status_TextField,
						MultiList first_list,
						MultiList second_list )
	{	String 	statusString;		// contains status information
	        int 	totRows;		// total number of rows
				
		totRows = first_list.getNumberOfRows() + second_list.getNumberOfRows();
	
		// display status
		statusString = "Exporting query results...";		   
		parent.setCursor( new Cursor(Cursor.WAIT_CURSOR) );
		if ( status_TextField != null ) {
			status_TextField.setText( statusString );
		}
		Message.printStatus( 1, "HBExportGUI.exportListObject()", statusString );        
	      
		// get the formatted export Vector for each  list object
		Vector export_Vector = new Vector( totRows, 5);
		getListContents( first_list, export_Vector );
		getListContents( second_list, export_Vector );
	
		// export to file/browser page     
		export( parent, export_Vector );
	
		// display status
		statusString = "Finished exporting.";		   
		parent.setCursor( new Cursor(Cursor.DEFAULT_CURSOR) );
		if ( status_TextField != null ) {
			status_TextField.setText( statusString );
		}
		Message.printStatus( 1, "HBExportGUI.exportListObject()", statusString );        
	}
	*/

	/* maybe support this later
	//-----------------------------------------------------------------------------
	//  Notes:
	//  (1)This function accept a MultiList object and formats a Vector containing
	//  headings and list contents for exporting. 
	//  (2)columns are deliminated via the user preference deliminator.
	//-----------------------------------------------------------------------------
	//  Variables:	I/O	Description
	//  (I)list - MultiList object
	//  (O)returns the headings and contents of the MultiList object as formatted
	//  for exporting. Columns are deliminated via the user preferenece export deliminator		
	//-----------------------------------------------------------------------------                        
	private static void getListContents( MultiList list, Vector export_Vector ) {
	        String[] 	headings,		// list headings
				listItems;              // list items. Columns are 
							// deliminated via ';'
		String 		formatString,		// formatted export String
				delim;			// export deliminator
	        int 		numRows,		// number of list rows  	
				curRow,			// row counter
				curCol,			// column counter
				size;			// Vector and String[] sizes
		Vector 		row_Vector;		// contains columns for the cuRow as elements
	
		// get the export deliminator
		delim = HBGUIApp.getValue("RunTime.ExportDelimiter").trim();
		if ( delim.equals("[TAB]") ) {
			delim = "\t";
		}
	
		// add headings to Vector
		// NOTE: headings are NOT deliminated when returned from the list object
		// get the list headings, list items, and number of rows
		headings = list.getHeadings();
	        listItems = list.getListItems();
		numRows = list.getNumberOfRows();
		size = headings.length;
		formatString = "";
		for ( curCol=0; curCol < size; curCol++ ) {
			formatString = formatString 
					+ headings[curCol].trim() 
					+ delim;
		}
	       	export_Vector.addElement( formatString + "\n" );         	
	
	        // add list items to Vector
		// NOTE: list ARE deliminated when returned from the list object via ';'
	        for (curRow=0; curRow<numRows; curRow++) {
	
			formatString = "";
			// add an extra ';' to the end of the selectedRowContents
			// String so that the breakStringList will be able to detect
			// all the columns
			row_Vector = StringUtil.breakStringList(
				listItems[curRow] + ";", ";", 0);		
			size = row_Vector.size();
			for ( curCol=0 ; curCol < size; curCol++ ) {
				formatString = formatString 
					  	+ row_Vector.elementAt( curCol ).toString().trim()
						+ delim;
			}
	        	export_Vector.addElement( formatString + "\n" );         
	        }           
	}
	*/

	} // end PrintJGUI class definition

}