﻿using System.Collections.Generic;
using System.IO;

// ExportJGUI - export to file utility

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
// ExportJGUI - export to file utility
//-----------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//-----------------------------------------------------------------------------
// Notes:	(1)	This GUI accepts a vector of strings and exports the
//			strings to the local file system.  If the file cannot
//			be written, a warning is printed and the file is not
//			written.
//-----------------------------------------------------------------------------
// History:
//
// 21 Nov 1997	Steven A. Malers, RTi	Implement this as a simple version of
//					the HBExportJGUI.
// 14 Mar 1998	SAM, RTi		Add Java documentation.
// 07 Dec 1999	SAM, RTi		Remove import * and add data member so
//					that the save directory is remembered.
//					This minimizes the need for user
//					navigation.
// 18 May 2001	SAM, RTi		Use the GUIUtil.lastFileDialogDirectory
//					information for picking the directory.
//-----------------------------------------------------------------------------
// 2003-06-05	J. Thomas Sapienza, RTi	Initial swing version.  Simply took the
//					old AWT ExportJGUI and converted it.
// 2005-11-16	JTS, RTi		Added JDialog support.
//-----------------------------------------------------------------------------

namespace RTi.Util.IO
{
	using GUIUtil = RTi.Util.GUI.GUIUtil;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;




	/// <summary>
	/// This class provides a generic way to export a vector of strings to a file.  The
	/// static export method is called, which creates a file selector for the export.
	/// At some point, this code will be able to export to a browser (when running as
	/// an applet), but Java security often prevents this from being done easily.
	/// At this time, an export to the local file system is always attempted, with
	/// security exceptions handled.
	/// </summary>
	public class ExportJGUI
	{

	//-----------------------------------------------------------
	//  Data Members
	//-----------------------------------------------------------            

	private static JFrame _parentJFrame; // Frame class making the call
							// to HBExportJGUI
	private static JDialog _parentJDialog;
	private static IList<string> _export; // List containing the formatted data to export

	/* support later... 
	These are now in IOUtil so take out of here when ready
	private static boolean          _isApplet;	// true if running an Applet,
							// false otherwise
	private static AppletContext    _appletContext; // current applet context
	private static URL              _documentBase;	// complete URL of the HTML
							// file that loaded the applet.
	*/

	//-----------------------------------------------------------------------------
	//  Notes:
	//  This function may be called from any class which extends JFrame. 
	//  The appropriate export functionality is invoked depending upon whether 
	//  an application or Applet calls this function.              
	//-----------------------------------------------------------------------------
	//  Variables:	I/O	Description
	//
	//          (I)parent - JFrame class from which this function is called.
	//          (I)export - Vector containing the information to export.
	//          (I)isApplet - true if running an Applet, false otherwise.
	//          (I)appletContext - current applet context.     
	//          (I)documentBase - complete URL of the HTML file that loaded the 
	//		applet.  
	//----------------------------------------------------------------------------

	/// <summary>
	/// Export a list of strings given the parent frame. </summary>
	/// <param name="parent"> JFrame that calls this routine. </param>
	/// <param name="export"> list of strings to export. </param>
	public static void export(JFrame parent, IList<string> export)
	{
		export(parent, export, "");
	}

	/// <summary>
	/// Export a list of strings given the parent frame and a help key for a help button. </summary>
	/// <param name="parent"> JFrame that calls this routine. </param>
	/// <param name="export"> list of strings to export. </param>
	/// <param name="helpkey"> Help key string for use with RTi.Util.Help.URLHelp (this is
	/// reserved for future use. </param>
	/// <seealso cref= RTi.Util.Help.URLHelp </seealso>
	public static void export(JFrame parent, IList<string> export, string helpkey)
	{
		string routine = "ExportJGUI.export";

		_parentJFrame = parent;
		_export = export;
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
			Message.printWarning(1, routine, "No text to export");
			return;
		}

		// We always try to write to the local disk.  If we are trusted, we
		// do it.  Otherwise, we try to write to the server if an applet.
		try
		{
			exportToLocalDrive();
		}
		catch (IOException)
		{
			//
			// Try export to browser if running from an Applet.
			//
			if (IOUtil.isApplet())
			{
			Message.printWarning(1, routine, "This application does not have sufficient permissions\n" + "to write to the local machine.  Please configure as a\n" + "trusted application using your browser security settings.\n");
			}

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
	//  (1)This function accepts a  JTextArea. 
	//  (2)calls getListContents( MultiList list ) to get a formatted export Vector
	//  (3)calls export( JFrame parent, Vector export )
	//-----------------------------------------------------------------------------
	//  Variables:	I/O	Description
	//  (I)parent - parent JFrame object from which this call was made.
	//  (I)status_JTextField - status JTextField to display statis to. May be null
	//  (I)textArea - JTextArea object
	//-----------------------------------------------------------------------------

	/// <summary>
	/// Export a JTextArea of strings given the parent frame a JTextField to receive
	/// status messages. </summary>
	/// <param name="parent"> JFrame that calls this routine. </param>
	/// <param name="status_JTextField"> JTextField to receive messages as the JTextArea is
	/// exported. </param>
	/// <param name="textArea"> JTextArea to export. </param>
	public static void exportJTextAreaObject(JFrame parent, JTextField status_JTextField, JTextArea textArea)
	{
		string statusString; // contains status information
		string routine = "ExportJGUI.exportJTextAreaObject()";

		// display status
		statusString = "Exporting query results...";
		parent.setCursor(new Cursor(Cursor.WAIT_CURSOR));
		if (status_JTextField != null)
		{
			status_JTextField.setText(statusString);
		}
		Message.printStatus(1, routine, statusString);

		/* This does not work because getText returns a newline-delimited
		** string (one big string) and we need carriage returns on the PC!
		** SAM submitted a bug report (feature request) to JavaSoft on
		** 12 Nov 1997
	
		// get the formatted export Vector for the list object
		Vector export_Vector = new Vector( 10, 5);
			
		export_Vector.addElement( new String( textArea.getText() ) );
		**
		** So use the following instead...
		*/

		IList<string> export_Vector = StringUtil.breakStringList(textArea.getText(), "\n", 0);

		// export to file/browser page     
		export(parent, export_Vector);

		// display status
		statusString = "Finished exporting.";
		parent.setCursor(new Cursor(Cursor.DEFAULT_CURSOR));
		if (status_JTextField != null)
		{
			status_JTextField.setText(statusString);
		}
		Message.printStatus(1, routine, statusString);

		return;
	}

	//-----------------------------------------------------------------------------
	//  Notes:
	//  This function exports the _export Vector to a LOCAL FILE if running an 
	//  application.
	//-----------------------------------------------------------------------------
	//  Variables:	I/O	Description
	//-----------------------------------------------------------------------------

	/// <summary>
	/// Export to a file on the local drive.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void exportToLocalDrive() throws java.io.IOException
	private static void exportToLocalDrive()
	{
		FileDialog fd; // FileDialog Object
		PrintWriter oStream; // DataOutputStream Object
		string routine = "ExportJGUI.exportToLocalDrive";

		// Instantiate a file dialog object with export.txt as 
		// the default filename. 

		fd = new FileDialog(_parentJFrame, "Export", FileDialog.SAVE);
		string last_directory_selected = GUIUtil.getLastFileDialogDirectory();
		if (!string.ReferenceEquals(last_directory_selected, null))
		{
			fd.setDirectory(last_directory_selected);
		}
		fd.setFile("export.txt");
		fd.setVisible(true);

		// Determine the name of the export file as specified from 
		// the FileDialog object        

		string fileName = fd.getDirectory() + fd.getFile();

		// return if no file name is selected

		if (fd.getFile() == null || fd.getFile().Equals(""))
		{
			return;
		}
		if (fd.getDirectory() != null)
		{
			GUIUtil.setLastFileDialogDirectory(fd.getDirectory());
		}

		if (!string.ReferenceEquals(fileName, null))
		{

			// First see if we can write the file given the security
			// settings...

			if (!SecurityCheck.canWriteFile(fileName))
			{
				Message.printWarning(1, routine, "Cannot save \"" + fileName + "\".");
				throw new IOException("Security check failed - unable to write \"" + fileName + "\"");
			}

			// We are allowed to write the file so try to do it...

			_parentJFrame.setCursor(new Cursor(Cursor.WAIT_CURSOR));
			//
			// Create a new FileOutputStream wrapped with a DataOutputStream
			// for writing to a file.
			try
			{
				oStream = new PrintWriter(new StreamWriter(fileName));
				//
				// Write each element of the _export Vector to a file.
				//
				// For some reason, when just using println in an
				// applet, the cr-nl pair is not output like it should
				// be on Windows95.  Java Bug???
				string linesep = System.getProperty("line.separator");
				/* For debugging...
				Vector v;
				v = StringUtil.showControl(linesep);
				oStream.print ( "Separator:" + linesep );
				for ( int j = 0; j < v.size(); j++ ) {
					oStream.print ((String)v.elementAt(j) +
					linesep );
				}
				*/
				for (int i = 0; i < _export.Count; i++)
				{
					oStream.print(_export[i].ToString() + linesep);
					/* For debugging...
					v = StringUtil.showControl( _export.elementAt(i).toString() );
					for ( int j = 0; j < v.size(); j++ ) {
						oStream.print ((String)v.elementAt(j) + linesep );
					}
					*/
				}
				//
				// close the PrintStream Object
				//
				oStream.flush();
				oStream.close();
			}
			catch (IOException)
			{
				Message.printWarning(1, routine, "Trouble opening or writing to file \"" + fileName + "\".");
				_parentJFrame.setCursor(new Cursor(Cursor.DEFAULT_CURSOR));
				throw new IOException("Trouble opening or writing to file \"" + fileName + "\".");
			}

			_parentJFrame.setCursor(new Cursor(Cursor.DEFAULT_CURSOR));
		}
		Message.printStatus(1, routine, "Successfully exported data to \"" + fileName + "\".");

		return;
	}

	/// <summary>
	/// Export a vector of strings given the parent frame. </summary>
	/// <param name="parent"> JDialog that calls this routine. </param>
	/// <param name="export"> Vector of strings to export. </param>
	public static void export(JDialog parent, IList<string> export)
	{
		export(parent, export, "");
	}

	/// <summary>
	/// Export a vector of strings given the parent frame and a help key for a help button. </summary>
	/// <param name="parent"> JDialog that calls this routine. </param>
	/// <param name="export"> list of strings to export. </param>
	/// <param name="helpkey"> Help key string for use with RTi.Util.Help.URLHelp (this is
	/// reserved for future use. </param>
	/// <seealso cref= RTi.Util.Help.URLHelp </seealso>
	public static void export(JDialog parent, IList<string> export, string helpkey)
	{
		string routine = "ExportJGUI.export";

		_parentJDialog = parent;
		_export = export;
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
			Message.printWarning(1, routine, "No text to export");
			return;
		}

		// We always try to write to the local disk.  If we are trusted, we
		// do it.  Otherwise, we try to write to the server if an applet.
		try
		{
			dialogExportToLocalDrive();
		}
		catch (IOException)
		{
			//
			// Try export to browser if running from an Applet.
			//
			if (IOUtil.isApplet())
			{
			Message.printWarning(1, routine, "This application does not have sufficient permissions\n" + "to write to the local machine.  Please configure as a\n" + "trusted application using your browser security settings.\n");
			}

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
	//  (1)This function accepts a  JTextArea. 
	//  (2)calls getListContents( MultiList list ) to get a formatted export Vector
	//  (3)calls export( JDialog parent, Vector export )
	//-----------------------------------------------------------------------------
	//  Variables:	I/O	Description
	//  (I)parent - parent JDialog object from which this call was made.
	//  (I)status_JTextField - status JTextField to display statis to. May be null
	//  (I)textArea - JTextArea object
	//-----------------------------------------------------------------------------

	/// <summary>
	/// Export a JTextArea of strings given the parent frame a JTextField to receive
	/// status messages. </summary>
	/// <param name="parent"> JDialog that calls this routine. </param>
	/// <param name="status_JTextField"> JTextField to receive messages as the JTextArea is
	/// exported. </param>
	/// <param name="textArea"> JTextArea to export. </param>
	public static void exportJTextAreaObject(JDialog parent, JTextField status_JTextField, JTextArea textArea)
	{
		string statusString; // contains status information
		string routine = "ExportJGUI.exportJTextAreaObject()";

		// display status
		statusString = "Exporting query results...";
		parent.setCursor(new Cursor(Cursor.WAIT_CURSOR));
		if (status_JTextField != null)
		{
			status_JTextField.setText(statusString);
		}
		Message.printStatus(1, routine, statusString);

		/* This does not work because getText returns a newline-delimited
		** string (one big string) and we need carriage returns on the PC!
		** SAM submitted a bug report (feature request) to JavaSoft on
		** 12 Nov 1997
	
		// get the formatted export Vector for the list object
		Vector export_Vector = new Vector( 10, 5);
			
		export_Vector.addElement( new String( textArea.getText() ) );
		**
		** So use the following instead...
		*/

		IList<string> exportList = StringUtil.breakStringList(textArea.getText(), "\n", 0);

		// export to file/browser page     
		export(parent, exportList);

		// display status
		statusString = "Finished exporting.";
		parent.setCursor(new Cursor(Cursor.DEFAULT_CURSOR));
		if (status_JTextField != null)
		{
			status_JTextField.setText(statusString);
		}
		Message.printStatus(1, routine, statusString);

		return;
	}

	//-----------------------------------------------------------------------------
	//  Notes:
	//  This function exports the _export Vector to a LOCAL FILE if running an 
	//  application.
	//-----------------------------------------------------------------------------
	//  Variables:	I/O	Description
	//-----------------------------------------------------------------------------

	/// <summary>
	/// Export to a file on the local drive.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void dialogExportToLocalDrive() throws java.io.IOException
	private static void dialogExportToLocalDrive()
	{
		FileDialog fd; // FileDialog Object
		PrintWriter oStream; // DataOutputStream Object
		string routine = "ExportJGUI.dialogExportToLocalDrive";

		// Instantiate a file dialog object with export.txt as 
		// the default filename. 

		JFrame frame = new JFrame();
		java.awt.Dimension d = _parentJDialog.getSize();
		frame.setSize(d.width, d.height);
		frame.setVisible(true);
		RTi.Util.GUI.JGUIUtil.center(frame);
		frame.setVisible(false);
		fd = new FileDialog(frame, "Export", FileDialog.SAVE);
		frame.dispose();
		string last_directory_selected = GUIUtil.getLastFileDialogDirectory();
		if (!string.ReferenceEquals(last_directory_selected, null))
		{
			fd.setDirectory(last_directory_selected);
		}
		fd.setFile("export.txt");
		fd.setVisible(true);

		// Determine the name of the export file as specified from 
		// the FileDialog object        

		string fileName = fd.getDirectory() + fd.getFile();

		// return if no file name is selected

		if (fd.getFile() == null || fd.getFile().Equals(""))
		{
			return;
		}
		if (fd.getDirectory() != null)
		{
			GUIUtil.setLastFileDialogDirectory(fd.getDirectory());
		}

		if (!string.ReferenceEquals(fileName, null))
		{

			// First see if we can write the file given the security
			// settings...

			if (!SecurityCheck.canWriteFile(fileName))
			{
				Message.printWarning(1, routine, "Cannot save \"" + fileName + "\".");
				throw new IOException("Security check failed - unable to write \"" + fileName + "\"");
			}

			// We are allowed to write the file so try to do it...

			_parentJDialog.setCursor(new Cursor(Cursor.WAIT_CURSOR));
			//
			// Create a new FileOutputStream wrapped with a DataOutputStream
			// for writing to a file.
			try
			{
				oStream = new PrintWriter(new StreamWriter(fileName));
				//
				// Write each element of the _export Vector to a file.
				//
				// For some reason, when just using println in an
				// applet, the cr-nl pair is not output like it should
				// be on Windows95.  Java Bug???
				string linesep = System.getProperty("line.separator");
				/* For debugging...
				Vector v;
				v = StringUtil.showControl(linesep);
				oStream.print ( "Separator:" + linesep );
				for ( int j = 0; j < v.size(); j++ ) {
					oStream.print ((String)v.elementAt(j) +
					linesep );
				}
				*/
				for (int i = 0; i < _export.Count; i++)
				{
					oStream.print(_export[i].ToString() + linesep);
					/* For debugging...
					v = StringUtil.showControl(	_export.elementAt(i).toString() );
					for ( int j = 0; j < v.size(); j++ ) {
						oStream.print ((String)v.elementAt(j) + linesep );
					}
					*/
				}
				//
				// close the PrintStream Object
				//
				oStream.flush();
				oStream.close();
			}
			catch (IOException)
			{
				Message.printWarning(1, routine, "Trouble opening or writing to file \"" + fileName + "\".");
				_parentJDialog.setCursor(new Cursor(Cursor.DEFAULT_CURSOR));
				throw new IOException("Trouble opening or writing to file \"" + fileName + "\".");
			}

			_parentJDialog.setCursor(new Cursor(Cursor.DEFAULT_CURSOR));
		}
		Message.printStatus(1, routine, "Successfully exported data to \"" + fileName + "\".");

		return;
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
		return;
	}
	*/

	/* Enable later.  This is specific to a Symantec multilist...
	//-----------------------------------------------------------------------------
	//  Notes:
	//  (1)This function accepts a  MultiList object. 
	//  (2)calls getListContents( MultiList list ) to get a formatted export Vector
	//  (3)calls export( JFrame parent, Vector export )
	//-----------------------------------------------------------------------------
	//  Variables:	I/O	Description
	//  (I)parent - parent JFrame object from which this call was made.
	//  (I)satus_JTextField - status JTextField to display statis to. May be null
	//  (I)list - MultiList object
	//-----------------------------------------------------------------------------                        
	public static void exportListObject (	JFrame parent,
						JTextField status_JTextField,
						MultiList list )
	{	String 	statusString;		// contains status information
	        int 	numRows;		// number of list rows  	
				
		numRows = list.getNumberOfRows();
	
		// display status
		statusString = "Exporting query results...";		   
		parent.setCursor( new Cursor(Cursor.WAIT_CURSOR) );
		if ( status_JTextField != null ) {
			status_JTextField.setText( statusString );
		}
		Message.printStatus( 1, "HBExportJGUI.exportListObject()", statusString );        
	      
		// get the formatted export Vector for the list object
		Vector export_Vector = new Vector( numRows, 5);
		getListContents( list, export_Vector );
	
		// export to file/browser page     
		export( parent, export_Vector );
	
		// display status
		statusString = "Finished exporting.";		   
		parent.setCursor( new Cursor(Cursor.DEFAULT_CURSOR) );
		if ( status_JTextField != null ) {
			status_JTextField.setText( statusString );
		}
		Message.printStatus( 1, "HBExportJGUI.exportListObject()", statusString );        
	 
		return;
	}
	
	/* Maybe support this later - this is specific to a Symantec multilst.
	
	//-----------------------------------------------------------------------------
	//  Notes:
	//  (1)This function accepts two MultiList objects. 
	//  (2)calls getListContents( MultiList list ) to get a formatted export Vector
	//  (3)calls export( JFrame parent, Vector export )
	//-----------------------------------------------------------------------------
	//  Variables:	I/O	Description
	//  (I)parent - parent JFrame object from which this call was made.
	//  (I)satus_JTextField - status JTextField to display statis to. May be null
	//  (I)first_list - MultiList object
	//  (I)second_list - MultiList object
	//-----------------------------------------------------------------------------                        
	public static void exportListObject (	JFrame parent,
						JTextField status_JTextField,
						MultiList first_list,
						MultiList second_list )
	{	String 	statusString;		// contains status information
	        int 	totRows;		// total number of rows
				
		totRows = first_list.getNumberOfRows() + second_list.getNumberOfRows();
	
		// display status
		statusString = "Exporting query results...";		   
		parent.setCursor( new Cursor(Cursor.WAIT_CURSOR) );
		if ( status_JTextField != null ) {
			status_JTextField.setText( statusString );
		}
		Message.printStatus( 1, "HBExportJGUI.exportListObject()", statusString );        
	      
		// get the formatted export Vector for each  list object
		Vector export_Vector = new Vector( totRows, 5);
		getListContents( first_list, export_Vector );
		getListContents( second_list, export_Vector );
	
		// export to file/browser page     
		export( parent, export_Vector );
	
		// display status
		statusString = "Finished exporting.";		   
		parent.setCursor( new Cursor(Cursor.DEFAULT_CURSOR) );
		if ( status_JTextField != null ) {
			status_JTextField.setText( statusString );
		}
		Message.printStatus( 1, "HBExportJGUI.exportListObject()", statusString );        
	 
		return;
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
	       	export_Vector.addElement(new String( formatString + "\n" ));         	
	
	        // add list items to Vector
		// NOTE: list ARE deliminated when returned from the list object via ';'
	        for (curRow=0; curRow<numRows; curRow++) {
	
			formatString = "";
			// add an extra ';' to the end of the selectedRowContents String so that
			// the breakStringList will be abl eto detect all the columns
			row_Vector = StringUtil.breakStringList( listItems[curRow] + ";", ";", 0);		
			size = row_Vector.size();
			for ( curCol=0 ; curCol < size; curCol++ ) {
				formatString = formatString 
					  	+ row_Vector.elementAt( curCol ).toString().trim()
						+ delim;
			}
	        	export_Vector.addElement(new String( formatString + "\n" ));         
	        }           
		return;
	}
	*/

	} // end ExportJGUI class definition

}