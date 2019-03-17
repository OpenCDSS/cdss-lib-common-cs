using System;

// XMRGViewer - viewer for XMRG data

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

// ----------------------------------------------------------------------------
// XMRGViewer - the main controlling class for running the XMRGProcessor
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2004-09-08	J. Thomas Sapienza, RTi	Initial version.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.GIS.GeoView
{

	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// This class is the main controlling class for running the XMRGProcessor.  It 
	/// sets up logging and processes command-line arguments.<para>
	/// The XMRGProcessor is a program to automatically download XMRG files from a
	/// server, convert them to the local endian type, clip them to a desired region,
	/// and then generate shapefiles from them.
	/// REVISIT (JTS - 2006-05-22)
	/// This class may be able to be removed, since most of the important functionality
	/// now is in a separate panel class.
	/// </para>
	/// </summary>
	public class XMRGViewer
	{

	/// <summary>
	/// The name of the program.
	/// </summary>
	public const string PROGRAM_NAME = "XMRGProcessor";

	/// <summary>
	/// The program version.
	/// </summary>
	public const string PROGRAM_VERSION = "0.1.0 2004-10-14";

	/// <summary>
	/// The home directory in which the log file should be generated.
	/// </summary>
	//private static String __home = "/opt/RTi/NWSRFS/logs";
	private static string __home = "./";

	private static string __xmrgFilename = null;

	/// <summary>
	/// Initializes the message system.
	/// </summary>
	private static void initializeMessage()
	{
		string routine = "XMRGViewer.initializeMessage";

		//set up message levels 
		Message.setDebugLevel(Message.TERM_OUTPUT, 50);
		Message.setDebugLevel(Message.LOG_OUTPUT, 50);
		Message.setStatusLevel(Message.TERM_OUTPUT, 1);
		Message.setStatusLevel(Message.LOG_OUTPUT, 2);
		Message.setWarningLevel(Message.TERM_OUTPUT, 1);
		Message.setWarningLevel(Message.LOG_OUTPUT, 2);

		Message.isDebugOn = false;

		string fs = File.separator;

		try
		{
			string logFileName = __home + fs + "XMRGProcessor.log";
			Message.openLogFile(logFileName);
		}
		catch (Exception e)
		{
			Message.printWarning(1, routine, "Couldn't open log file: " + __home + fs + "XMRGProcessor.log");
			Message.printWarning(2, routine, e);
			Environment.Exit(1);
		}
	}

	/// <summary>
	/// Start main application. </summary>
	/// <param name="args"> Command line arguments. </param>
	public static void Main(string[] args)
	{
		string routine = "XMRGViewer.main";

		//set up message class
		initializeMessage();

		Message.printStatus(10, routine, "Parsing command line arguments.");
		try
		{
			parseArgs(args);
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, e);
			Message.printWarning(2, routine, "Error parsing arguments passed into " + PROGRAM_NAME + "Exiting...");
			quitProgram(5);
		}

		try
		{
			new XMRGViewerJFrame(__xmrgFilename, false);
		}
		catch (Exception e)
		{
			Message.printWarning(1, routine, e);
		}
	}


	/// <summary>
	/// Parses command line arguments. </summary>
	/// <param name="args"> the arguments from the command line. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void parseArgs(String[] args) throws Exception
	public static void parseArgs(string[] args)
	{
		string routine = "XMRGViewer.parseArgs";

		int length = args.Length;

		for (int i = 0; i < length; i++)
		{
			if (args[i].Equals("-f"))
			{
				if (i == (length - 1))
				{
					Message.printWarning(1, routine, "No value set for parameter '-f'");
					throw new Exception("No value set for " + "parameter '-f'");
				}
				i++;
				__xmrgFilename = args[i];
			}
			else
			{
				Message.printWarning(1, routine, "Unknown parameter: '" + args[i] + "'");
			}
		}
	}

	/// <summary>
	/// Clean up and exit application. </summary>
	/// <param name="status"> Program exit status. </param>
	private static void quitProgram(int status)
	{
		string routine = "XMRGViewer.quitProgram";
		Message.printStatus(1, routine, "Exiting with status: " + status + ".");
		Environment.Exit(status);
	}

	}

}