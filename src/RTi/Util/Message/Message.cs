using System;
using System.IO;
using System.Diagnostics;

using NLog;
using NLog.Config;
using NLog.Targets;

using RTi.Util.IO;

// Message - debug and status message class.

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
// RTi.Util.Message.Message - debug and status message class.
//------------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
//
// 26 Aug 1997  Matthew J. Rutherford,  Created initial version based
//              RTi                     on HBData C functions.
// 12 Nov 1997  Steven A. Malers, RTi   Add setDebug(boolean) to allow for
//                                      more control of debugging.
// 16 Mar 1998  SAM, RTi                Add javadoc.
// 10 Aug 1998  SAM, RTi                Update versions that take exceptions
//                                      to print to open log file.  There is
//                                      currently no way to get the stack trace
//                                      as strings.
// 06 Oct 1998  SAM, RTi                Clean up code in conjunction with C++
//                                      port.
// 23 Dec 1998  SAM, RTi                Overload setDebugLevel to take a string.
// 04 Feb 1999  SAM, RTi                Change so that instead of accepting
//                                      an Exception, printWarning accepts a
//                                      Throwable.
// 20 Feb 1999  SAM, RTi                Check for null Throwable, routine when
//                                      printing.
// 17 Sep 1999  SAM, RTi                Add closeLogFile().
// 15 Mar 2001  SAM, RTi                Add setProp() and
//                                      getMessageProp() to allow more
//                                      flexibility in message handling,
//                                      especially for Warning dialogs.  Change
//                                      IO to IOUtil.  Clean up javadoc and set
//                                      unused variables to null.
// 07 Aug 2001  SAM, RTi                Update so that the output functions can
//                                      be set to null to stop output
//                                      redirection.
// 2002-05-24   SAM, RTi                closeLogFile() was not static and was
//                                      not able to be called.  Make static so
//                                      it can be used in TSTool.
// 2003-04-09   SAM, RTi                Fix bug where initialize() was going
//                                      into an infinite recursion if the debug
//                                      flag is already turned on by external
//                                      code.
//------------------------------------------------------------------------------
// 2003-08-22   J. Thomas Sapienza, RTi Switched over to use the Swing
//                                      MessageJDialog and DiagnosticsJFrame.
// 2005-03-11   SAM, RTi                * Add a property in setPropValue() to
//                                        turn on levels for the messages.
//                                      * Do similar to above for message tags
//                                        and add __SHOW_MESSAGE_TAG to the
//                                        options for the behavior flag.
//                                      * Overload the printWarning() method
//                                        to take a tag for use in the log file
//                                        viewer.
// 2005-03-22   JTS, RTi                * Added addMessageLogListener().
//                                      * Added getMessageLogListeners().
//                                      * Added removeMessageLogListener().
// 2005-05-12   JTS, RTi                * Added restartLogFile().
//                                      * Added openNewLogFile().
// 2005-10-19   SAM, RTi                * Change so that the warning dialog is
//                                        not displayed if running in batch
//                                        mode.  In this case the GUI would not
//                                        be visible and the application waits
//                                        for a response on an invisible dialog.
// 2005-12-12   JTS, RTi                Changed restartLogFile() because in
//                                      certain conditions the code was not
//                                      opening the same log file after it was
//                                      closed.
//------------------------------------------------------------------------------
// EndHeader

namespace RTi.Util.Message
{
    /// <summary>
	/// This class provides useful static functions for printing debug, status, and
	/// warning messages (see the print* routines).  The class works both for terminal
	/// and GUI based applications.
	/// If a message level is zero, then no messages will be generated.  The higher the
	/// number, the more detailed the messages.  If a warning message is printed at
	/// level 1 and it is from a GUI application, a modal dialog will appear that
	/// forces the user to acknowledge the warning.
	/// To use the class, place code similar to the following in the main application:
	/// <para>
	/// <pre>
	/// Message.setDebugLevel ( Message.TERM_OUTPUT, 0 );      // Stdout
	/// Message.setWarningLevel ( Message.TERM_OUTPUT, 0 );
	/// Message.setDebugLevel ( Message.LOG_OUTPUT, 10 );      // Log file
	/// Message.setWarningLevel ( Message.LOG_OUTPUT, 10 );
	/// Message.setWarningLevel ( Message.STATUS_HISTORY_OUTPUT, 1 );
	/// </pre>
	/// </para>
	/// <para>
	/// 
	/// As a developer, decide on the appropriate message levels in code, with
	/// level 1 messages being visible to the user.  Libraries should generally never
	/// print level 1 warning messages.  Errors should be trapped and migrated back to
	/// the main application through exceptions.  Use the print* methods to embed
	/// message calls in code.  Because debug messages may occur often and because
	/// formatting messages is a performance hit, debug messages should be wrapped in the following code:
	/// </para>
	/// <para>
	/// <pre>
	/// if ( Message.isDebugOn ) {
	///  // Debug message...
	///  Message.printDebug ( ... );
	/// }
	/// </pre>
	/// 
	/// If using messages with a GUI, let the Message class know what the top-level
	/// component is (for use with the warning dialog, etc.)...
	/// </para>
	/// <para>
	/// 
	/// <pre>
	/// Message.setTopLevel ( this );  // Where this is usually a frame.
	/// </pre>
	/// </para>
	/// <para>
	/// 
	/// The following causes messages to be passed to a function, which can in turn
	/// process the messages (the DiagnosticsJFrame does this).  In the future,
	/// a listener approach may be taken.
	/// </para>
	/// <para>
	/// 
	/// <pre>
	///       Message.setOutputFunction ( Message.STATUS_HISTORY_OUTPUT, this,
	///               "printStatusMessages" );
	/// </pre>
	/// </para>
	/// </summary>
	public abstract class Message
    {

        private static bool _configured = false;

        /**
        Indicates whether debug is on.  This is public so that it can be quickly
        accessed.  To increase performance, all debug messages should be wrapped with
        <p>
        <pre>
        if ( Message.isDebugOn ) {
          // Do the debug message...
          Message.printDebug ( 1, "myroutine", "the message" );
        }
        </pre>
        The default to no debugging being done.Set to true in command-line parsing
        code if setDebug is called.
        */
        public static bool isDebugOn = false;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static void closeLogFile()
        {
            LogManager.Flush();
            LogManager.Shutdown();
        }

        public static void clearFileBeforeWriting(string LogFile)
        {
            File.WriteAllText(LogFile, string.Empty);
        }

        public static void configureLogger(string LogFile)
        {
            var config = new LoggingConfiguration();

            FileTarget target = logFileTarget(LogFile);

            config.AddTarget(target);

            var rule = new LoggingRule("*", LogLevel.Debug, target);

            config.LoggingRules.Add(rule);

            LogManager.Configuration = config;

            _configured = true;
        }

        public static FileTarget logFileTarget(string LogFile)
        {
            try
            {
                FileTarget target = new FileTarget
                {
                    FileName = LogFile,
                    Name = "LogFile"
                };
                return target;
            }
            catch (System.NullReferenceException e)
            {
                return null;
            }
        }

        public static void openLogFile(string LogFile)
        {
            string message = "";
            string routine = "Message.OpenLogFile";
            if (LogFile == null)
            {
                message = "Null log file.  Not opening log file.";
                printWarning(2, routine, message);
                //throw new IOException(message);
            }
            if (LogFile.Length <= 0)
            {
                message = "Zero-length log file.  Not opening log file.";
                printWarning(2, routine, message);
                //throw new IOException(message);
            }
            clearFileBeforeWriting(LogFile);
            configureLogger(LogFile);

            // Add header to logfile
            TextWriter writeFile = new StreamWriter(LogFile);
            writeFile.WriteLine("#");
            writeFile.WriteLine("# " + LogFile + " - " + IOUtil.getProgramName() + " log file");
            writeFile.WriteLine("#");
            writeFile.Close();
        }

        public static void printDebug(int level, string routine, string warn)
        {
            string debugMessage = "";
            debugMessage += "Debug[" + level.ToString() + "]";
            if (!string.IsNullOrEmpty(routine))
            {
                debugMessage += "(" + routine + ")";
            }
            debugMessage += ": ";
            debugMessage += warn;
            if (!_configured)
            {
                Console.WriteLine(debugMessage);
            }
            else
            {
                if (level < 11)
                {
                    logger.Debug(debugMessage);
                }
                else
                {
                    logger.Trace(debugMessage);
                }
            }
        }

        public static void printStatus(int level, string routine, string warn)
        {
            string statusMessage = "";
            statusMessage += "Status[" + level.ToString() + "]";
            if (!string.IsNullOrEmpty(routine))
            {
                statusMessage += "(" + routine + ")";
            }
            statusMessage += ": ";
            statusMessage += warn;
            if (!_configured)
            {
                Console.WriteLine(statusMessage);
            }
            else
            {
                logger.Info(statusMessage);
            }
        }

        public static void printWarning(int level, string routine, string warn)
        {
            string warningMessage = "";
            warningMessage += "Warning[" + level.ToString() + "]";
            if (!string.IsNullOrEmpty(routine))
            {
                warningMessage += "(" + routine + ")";
            }
            warningMessage += ": ";
            warningMessage += warn;
            if (!_configured)
            {
                Console.WriteLine(warningMessage);
            }
            else
            {
                logger.Debug(warningMessage);
            }
        }

        public static void printWarning(int level, string routine, Exception e)
        {
            string warningMessage = "";
            warningMessage += "Warning[" + level.ToString() + "]";
            if (!string.IsNullOrEmpty(routine))
            {
                warningMessage += "(" + routine + ")";
            }
            warningMessage += ": ";
            warningMessage += e.ToString();
            if (!_configured)
            {
                Console.WriteLine(warningMessage);
            }
            else
            {
                logger.Debug(warningMessage);
            }
        }
    }
}
