using System;
using System.Collections.Generic;

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


	/// <summary>
	/// The following are used when setting message output locations.  Output to the
	/// terminal (UNIX or DOS shell).  This prints to System.out.
	/// </summary>
	public const int TERM_OUTPUT = 0;

	/// <summary>
	/// Output to the log file.
	/// </summary>
	public const int LOG_OUTPUT = 1;

	/// <summary>
	/// Output messages to the DiagnosticsJFrame as a scrolling history. </summary>
	/// <seealso cref= DiagnosticsJFrame </seealso>
	public const int STATUS_HISTORY_OUTPUT = 2;

	/// <summary>
	/// Output to a one-line status bar at the bottom of the application screen.
	/// The routine to print the message needs to be set using setOutputFunction.
	/// REVISIT JAVADOC: see RTi.Util.Message.Message.setOutputFunction
	/// </summary>
	public const int STATUS_BAR_OUTPUT = 3;

	/// <summary>
	/// Output to a scrolling status area within ProcessManagerDialog or
	/// ProcessManagerJDialog - this may not be needed after 2002-10-16 revisions to the ProcessManager. </summary>
	/// <seealso cref= RTi.Util.IO.ProcessManagerDialog </seealso>
	/// <seealso cref= RTi.Util.IO.ProcessManagerJDialog </seealso>
	public const int PROCESS_MANAGER_GUI = 4;

	/// <summary>
	/// A list of output names corresponding to the *_OUTPUT values.
	/// </summary>
	public static readonly string[] OUTPUT_NAMES = new string[] {"Terminal", "Log file", "Status history", "Status bar", "Process manager"};

	/// <summary>
	/// Indicates whether debug is on.  This is public so that it can be quickly
	/// accessed.  To increase performance, all debug messages should be wrapped with
	/// <para>
	/// <pre>
	/// if ( Message.isDebugOn ) {
	///  // Do the debug message...
	///  Message.printDebug ( 1, "myroutine", "the message" );
	/// }
	/// </pre>
	/// The default to no debugging being done.  Set to true in command-line parsing
	/// code if setDebug is called.
	/// </para>
	/// </summary>
	public static bool isDebugOn = false;

	/// <summary>
	/// The following are flags for printing messages, set by various methods in this
	/// class.  The following setting will print the message level in debug and warning
	/// messages (the default is to not show the level).  The Diagnostics GUI will at
	/// some point edit all of these settings.
	/// 
	/// Show the message level in output (normally the level is not displayed in messages).
	/// </summary>
	public const int SHOW_MESSAGE_LEVEL = 0x1;

	/// <summary>
	/// Show the dialog for warning messages at level 1 (this is the default).
	/// THIS IS BEING PHASED OUT IN FAVOR OF THE PROPERTIES.
	/// </summary>
	public const int GUI_FOR_WARNING_1 = 0x2;

	/// <summary>
	/// Show the routine name (the default is to print the routine name for debug
	/// messages and for warning messages greater than level 1).
	/// </summary>
	public const int SHOW_ROUTINE = 0x4;

	/// <summary>
	/// Flush the output buffer after each message is written.  The default is to let
	/// the system flush the output buffer.
	/// </summary>
	public const int FLUSH_OUTPUT = 0x8;

	private static readonly MessageImpl impl = getImpl();

	private static MessageImpl getImpl()
	{
		string prop = System.getProperty("RTi.Util.MessageImpl");
		MessageImpl impl = null;
		Exception ex = null;
		if (!string.ReferenceEquals(prop, null))
		{
			try
			{
				impl = (MessageImpl)System.Activator.CreateInstance(Type.GetType(prop));
			}
			catch (Exception e)
			{
				ex = e;
			}
		}
		if (impl == null)
		{
			impl = new MessageImpl();
		}
		if (ex != null)
		{
			impl.printWarning(0,"getImpl",ex);
		}
		return impl;
	}

	/// <summary>
	/// Adds a listener to the list of listeners that can respond to actions from
	/// the MessageLogJFrame.  The listeners are updated by the MessageLogJFrame 
	/// every time its processLogFile() method is called and a log file is read and 
	/// displayed.  Any listeners added after a log file is read will not receive
	/// commands from the MessageLogJFrame until it reads the log file again. </summary>
	/// <param name="listener"> the listener to add. </param>
	public static void addMessageLogListener(MessageLogListener listener)
	{
		impl.addMessageLogListener(listener);
	}

	/// <summary>
	/// Flush and close the log file associated with Message.LOG_OUTPUT, if it has been opened.
	/// </summary>
	public static void closeLogFile()
	{
		impl.closeLogFile();
	}

	/// <summary>
	/// Flush the output buffers.  It does not appear that this method has any effect on some systems. </summary>
	/// <param name="flag"> Currently unused. </param>
	public static void flushOutputFiles(int flag)
	{
		impl.flushOutputFiles(flag);
	}

	/// <summary>
	/// Return the debug level for an output stream. </summary>
	/// <returns> The debug level for an output stream number (specified by a *_OUTPUT value). </returns>
	/// <param name="i"> The output stream number. </param>
	public static int getDebugLevel(int i)
	{
		return impl.getDebugLevel(i);
	}

	/// <summary>
	/// Return the name of the log file. </summary>
	/// <returns> The name of the log file. </returns>
	public static string getLogFile()
	{
		return impl.getLogFile();
	}

	/// <summary>
	/// Returns the list of listeners that are set to respond to actions from the
	/// MessageLogJFrame.  The returned list will never be null. </summary>
	/// <returns> the list of listeners for the MessageLogJFrame. </returns>
	public static IList<MessageLogListener> getMessageLogListeners()
	{
		return impl.getMessageLogListeners();
	}

	/// <summary>
	/// Return the value of a Message property.  See setPropValue() for a description of valid properties. </summary>
	/// <returns> the property value or null if not defined. </returns>
	public static string getPropValue(string key)
	{
		return impl.getPropValue(key);
	}

	/// <summary>
	/// Return the status level for an output stream. </summary>
	/// <returns> The status level for an output stream number (specified by a *_OUTPUT value). </returns>
	/// <param name="i"> The output stream number. </param>
	public static int getStatusLevel(int i)
	{
		return impl.getStatusLevel(i);
	}

	/// <summary>
	/// Return the warning level for an output stream. </summary>
	/// <returns> The warning level for an output stream number (specified by a *_OUTPUT value). </returns>
	/// <param name="i"> The output stream number. </param>
	public static int getWarningLevel(int i)
	{
		return impl.getWarningLevel(i);
	}

	/// <summary>
	/// Open the log file.
	/// Because no log file is specified, the name of the log file will default to
	/// the program name and the extension ".log" (set with IOUtil.setProgramName). </summary>
	/// <seealso cref= RTi.Util.IO.IOUtil#setProgramName </seealso>
	/// <returns> The PrintWriter corresponding to the log file. </returns>
	/// <exception cref="IOException"> if there is an error opening the log file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.io.PrintWriter openLogFile() throws java.io.IOException
	public static PrintWriter openLogFile()
	{
		return impl.openLogFile();
	}

	/// <summary>
	/// Open the log file using the specified name. </summary>
	/// <returns> The PrintWriter corresponding to the log file. </returns>
	/// <exception cref="IOException"> if there is an error opening the log file. </exception>
	/// <param name="logfile"> Name of log file to open. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.io.PrintWriter openLogFile(String logfile) throws java.io.IOException
	public static PrintWriter openLogFile(string logfile)
	{
		return impl.openLogFile(logfile);
	}

	/// <summary>
	/// Opens a new file at the specified location. </summary>
	/// <param name="path"> the full path to the new log file to open. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void openNewLogFile(String path) throws Exception
	public static void openNewLogFile(string path)
	{
		impl.openNewLogFile(path);
	}

	/// <summary>
	/// Print a debug message to the registered output receivers. </summary>
	/// <param name="level"> Debug level for the message. </param>
	/// <param name="routine"> Name of the routine printing the message. </param>
	/// <param name="message"> Debug message. </param>
	public static void printDebug(int level, string routine, string message)
	{
		impl.printDebug(level,routine,message);
	}

	/// <summary>
	/// Print a stack trace as if debug message.  Currently, this will print to the
	/// log file if it is not null. </summary>
	/// <param name="level"> Debug level for the message. </param>
	/// <param name="routine"> Name of the routine printing the message. </param>
	/// <param name="e"> Throwable (e.g., Error, Exception) for which to print a stack trace. </param>
	public static void printDebug(int level, string routine, Exception e)
	{
		impl.printDebug(level,routine,e);
	}

	/// <summary>
	/// Print information about the registered message levels using status messages,
	/// using the current message settings.
	/// </summary>
	public static void printMessageLevels()
	{
		impl.printMessageLevels();
	}

	/// <summary>
	/// Print information about the registered message levels using the current message settings. </summary>
	/// <param name="flag"> If true, print using status messages.  If false, print to the
	/// system standard output. </param>
	public static void printMessageLevels(bool flag)
	{
		impl.printMessageLevels(flag);
	}

	/// <summary>
	/// Print a status message to the registered output receivers. </summary>
	/// <param name="level"> Status level for the message. </param>
	/// <param name="routine"> Name of the routine printing the message. </param>
	/// <param name="message"> Status message. </param>
	public static void printStatus(int level, string routine, string message)
	{
		impl.printStatus(level,routine,message);
	}

	/// <summary>
	/// This routine calls printWarning but allows the developer to specify a
	/// different _top_level frame from the preset top level frame.  This is useful
	/// when you are printing a warning of level 1 and want to specify which window
	/// that WarningDialog should be associated with, without changing the top
	/// level window that should be typically used.
	/// </summary>
	public static void printWarning(int level, string routine, string message, JFrame top_level)
	{
		if (top_level == null)
		{
			impl.printWarning(level,routine,message);
		}
		else
		{
			impl.printWarning(level,routine,message,top_level);
		}
	}

	/// <summary>
	/// Print a warning message to the registered output receivers.  The overloaded
	/// version is called without the tag. </summary>
	/// <param name="level"> Warning level for the message. </param>
	/// <param name="routine"> Name of the routine printing the message. </param>
	/// <param name="message"> Warning message. </param>
	public static void printWarning(int level, string routine, string message)
	{
		printWarning(level, null, routine, message);
	}

	/// <summary>
	/// Print a warning message to the registered output receivers. </summary>
	/// <param name="level"> Warning level for the message.  The level will be printed with
	/// the message if the ShowMessageLevel property is "true". </param>
	/// <param name="tag"> A tag to be printed with the message.  The tag will be printed with
	/// the message if the ShowMessageTag property is "true". </param>
	/// <param name="routine"> Name of the routine printing the message.  If blank the routine will not be printed. </param>
	/// <param name="message"> Warning message. </param>
	public static void printWarning(int level, string tag, string routine, string message)
	{
		impl.printWarning(level,tag,routine,message);
	}

	/// <summary>
	/// Print a stack trace as if a warning message.  Output will only be to the log file, if open. </summary>
	/// <param name="level"> Warning level for the message. </param>
	/// <param name="routine"> Name of the routine printing the message. </param>
	/// <param name="e"> Throwable (e.g. Error, Exception) for which to print a stack trace. </param>
	public static void printWarning(int level, string routine, Exception e)
	{
		impl.printWarning(level,routine,e);
	}

	/// <summary>
	/// Removes a listener from the Vector of listeners that are set to listen to 
	/// actions from the MessageLogJFrame. </summary>
	/// <param name="listener"> the listener to remove from the Vector of MessageLogListeners. </param>
	public static void removeMessageLogListener(MessageLogListener listener)
	{
		impl.removeMessageLogListener(listener);
	}

	/// <summary>
	/// Closes and opens the log file, so old data will be overwritten next time something is logged.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void restartLogFile() throws Exception
	public static void restartLogFile()
	{
		impl.closeLogFile();
		impl.openLogFile(getLogFile());
	}

	/// <summary>
	/// Set the output behavior flags (e.g., SHOW_MESSAGE_LEVEL) as a bit mask.
	/// Currently this sets the given bit (but does not unset the others).
	/// <b>The handling of these flags needs more work (the defaults are usually OK).</b>
	/// See setProp() for additional properties used to control message behavior. </summary>
	/// <param name="flag"> Bit mask for flag to set. </param>
	public static void setBehaviorFlag(int flag)
	{
		impl.setBehaviorFlag(flag);
	}

	/// <summary>
	/// Set the flag indicating whether debug is on or off.  This is NOT called by the
	/// setDebugLevel method if the debug level is greater than zero for any output
	/// receiver.  If debug is turned off, then debug messages that check this flag will run much faster. </summary>
	/// <param name="flag"> true if debug should be turned on, false if not. </param>
	public static void setDebug(bool flag)
	{
		impl.setDebug(flag);
	}

	/// <summary>
	/// Set the debug level for an output receiver. If the level is > 0, do not set
	/// debugging to on.  The debug levels are independent of whether debug is actually on or off. </summary>
	/// <param name="i"> Output receiver number (the *_OUTPUT values). </param>
	/// <param name="level"> Debug level for the output receiver.
	/// TODO JAVADOC: see RTi.Util.Message.Message.setDebug </param>
	public static void setDebugLevel(int i, int level)
	{
		impl.setDebugLevel(i,level);
	}

	/// <summary>
	/// Set the debug level by parsing the levels from a string like "#,#", "#", ",#",
	/// etc.  The TERM_OUTPUT and LOG_OUTPUT levels are set. </summary>
	/// <param name="debug_level"> Debug level as a string. </param>
	public static void setDebugLevel(string debug_level)
	{
		impl.setDebugLevel(debug_level);
	}

	/// <summary>
	/// Set the log file name for the log file. </summary>
	/// <param name="logfile"> Name of log file. </param>
	public static void setLogFile(string logfile)
	{
		impl.setLogFile(logfile);
	}

	/// <summary>
	/// Set the output file for an output receiver. </summary>
	/// <param name="i"> Output receiver (the *_OUTPUT values). </param>
	/// <param name="output_stream"> Output PrintStream for the output receiver (usually a log
	/// file).  The default is no log file. </param>
	public static void setOutputFile(int i, PrintStream output_stream)
	{
		impl.setOutputFile(i,output_stream);
	}

	/// <summary>
	/// Set the output file for an output receiver. </summary>
	/// <param name="i"> Output receiver (the *_OUTPUT values). </param>
	/// <param name="output_stream"> Output PrintWriter for the output receiver (usually a log
	/// file).  The default is no log file. </param>
	public static void setOutputFile(int i, PrintWriter output_stream)
	{
		impl.setOutputFile(i,output_stream);
	}

	/// <summary>
	/// Set the function to call for an output receiver. </summary>
	/// <param name="i"> Output receiver (the *_OUTPUT values). </param>
	/// <param name="obj"> Object containing the function to call. </param>
	/// <param name="function_name"> Name of function to call to receive message.  The function
	/// must take an integer (message level), a String (the routine name), and a
	/// second String (the message). </param>
	public static void setOutputFunction(int i, object obj, string function_name)
	{
		impl.setOutputFunction(i,obj,function_name);
	}

	/// <summary>
	/// Set the prefix to use in front of all messages (the default is nothing).
	/// This can be used, for example, to format messages for HTML. </summary>
	/// <param name="prefix"> Prefix string to use in front of all messages. </param>
	public static void setPrefix(string prefix)
	{
		impl.setPrefix(prefix);
	}

	/// <summary>
	/// Set a property used to control message behavior.  These properties currently are
	/// used mainly to control the warning dialog behavior.
	/// The properties should be set by high-level code
	/// to control how the application behaves.  For example, if there is potential for
	/// many warnings to be generated, the warning dialog can be configured to be turned
	/// off during stretches of processing and then turned on again during stretches
	/// where user input is required.  Use the getPropValue() method to determine a
	/// property value (e.g., to determine if the use has indicated that further warning
	/// messages should not be shown.  The following properties are recognized (not all
	/// are normally set by application code - some are typically only set internally
	/// and are then queried by application code - see comments):
	/// 
	/// <table width=100% cellpadding=10 cellspacing=0 border=2>
	/// <tr>
	/// <td><b>Property</b></td>	<td><b>Description</b></td>
	/// <td><b>Default</b></td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>ShowMessageLevel</td>
	/// <td>Indicate whether message levels should be shown for messages.  The default
	/// is to NOT show message levels.  If true, the normal messages will be modified
	/// to include the level in [brackets], as follows (the optional Tag and Routine are
	/// shown for illustration):
	/// <pre>
	/// Warning[Level]<Tag>(Routine)...
	/// </pre>
	/// </td>
	/// <td>false - do not show message levels in output.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>ShowMessageTag</td>
	/// <td>Indicate whether message tags should be shown for messages.  The default
	/// is to NOT show message tags.  Message tags are used in overloaded methods that
	/// pass the tag to the message.  For example, this can be used to help link the
	/// messages to some scope/content in an application.  If true, the normal messages
	/// will be modified to include the tag in <brackets>, as follows (the optional Tag
	/// and Routine are shown for illustration):
	/// <pre>
	/// Warning[Level]<Tag>(Routine)...
	/// </pre>
	/// </td>
	/// <td>false - do not show message tags in output.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>ShowWarningDialog</td>
	/// <td>Set to false to turn off the Warning dialog.  Use selectively to
	/// turn off warnings in cases where multiple warnings may occur.  This property
	/// is normally set to true internally by the MessageJDialog when the user indicates
	/// via the dialog that no more warnings should be shown (see the
	/// "WarningDialogOKNoMoreButton" property).</td>
	/// <td>true - always show warning dialogs (at level 1).</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>WarningDialogOKNoMoreButton</td>
	/// <td>Set to true to turn cause the warning dialog to display a button like
	/// "OK - Do Not Show More Warnings".  Pressing this button will cause the
	/// "ShowWarningDialog" property to be set to false.  See also the
	/// "WarningDialogOKNoMoreButtonLabel" property.
	/// </td>
	/// <td>false</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>WarningDialogOKNoMoreButtonLabel</td>
	/// <td>Set the label for the button associated with the
	/// "WarningDialogOKNoMoreButton" property.
	/// </td>
	/// <td>When button is active: "OK - Do Not Show More Warnings".</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>WarningDialogViewLogButton</td>
	/// <td>Indicate with true or false whether a button should be enabled to view the log file.
	/// </td>
	/// <td>false</td>
	/// </tr>
	/// </table>
	/// </summary>
	public static void setPropValue(string prop)
	{
		impl.setPropValue(prop);
	}

	/// <summary>
	/// Set the status level for the application.  Only messages with levels less than
	/// or equal to the set level will be printed. </summary>
	/// <param name="i"> Output receiver (the *_OUTPUT values). </param>
	/// <param name="level"> Status message level for the application. </param>
	public static void setStatusLevel(int i, int level)
	{
		impl.setStatusLevel(i,level);
	}

	/// <summary>
	/// Set the suffix to use at the end of all messages (the default is nothing).
	/// This can be used, for example, to format messages for HTML. </summary>
	/// <param name="suffix"> Suffix string to use behind all messages. </param>
	public static void setSuffix(string suffix)
	{
		impl.setSuffix(suffix);
	}

	/// <summary>
	/// Set the top-level frame for the application that uses messages.  This allows
	/// the modal warning dialog to be created. </summary>
	/// <param name="f"> Top-level JFrame for application. </param>
	public static void setTopLevel(JFrame f)
	{
		impl.setTopLevel(f);
	}

	/// <summary>
	/// Set the warning level for the application.  Only messages with levels less than
	/// or equal to the set level will be printed. </summary>
	/// <param name="i"> Output receiver (the *_OUTPUT values). </param>
	/// <param name="level"> Warning message level for the application. </param>
	public static void setWarningLevel(int i, int level)
	{
		impl.setWarningLevel(i,level);
	}

	}

}