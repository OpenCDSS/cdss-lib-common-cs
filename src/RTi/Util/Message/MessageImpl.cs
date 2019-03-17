using System;
using System.Collections.Generic;
using System.IO;

// MessageImpl - implementation of logging using legacy Message

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
// RTi.Util.Message.MessageImpl - implementation of logging using legacy Message
//------------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
//
// 2007-02-27  Ian Schneider, RTi   Initial version, to allow legacy Message
//                                  class method to be called but also allow the
//                                  new MessageLoggingImpl class to be used,
//                                  resulting in Java logging to be used.
// 2007-05-11  Steven A. Malers,    Minor cleanup based on Eclipse feedback, and
//                                  add Javadoc for class.
//------------------------------------------------------------------------------

namespace RTi.Util.Message
{

	using IOUtil = RTi.Util.IO.IOUtil;
	using PropList = RTi.Util.IO.PropList;
	using StringUtil = RTi.Util.String.StringUtil;
	using DateTime = RTi.Util.Time.DateTime;

	/// <summary>
	/// This class is an implementation of the Message design for logging.  This
	/// implementation matches the legacy implementation.  Use the MessageLoggingImpl
	/// implementation to use the Java logging approach.
	/// </summary>
	internal class MessageImpl
	{

	/// <summary>
	/// Maximum number of output files to receive Message output.
	/// </summary>
	protected internal int MAX_FILES = 5;

	/// <summary>
	/// Show the message tags.  This is only used internally since properties are being
	/// phased in to set behavior. 
	/// </summary>
	protected internal int __SHOW_MESSAGE_TAG = 0x10;

	/// <summary>
	/// Indicates if the Message class has been initialized.
	/// </summary>
	protected internal bool _initialized = false;
	/// <summary>
	/// JFrame when a GUI application, to allow message dialogs to be on top.
	/// </summary>
	protected internal JFrame _top_level = null;
	/// <summary>
	/// Controls the appearance of messages.
	/// </summary>
	protected internal int _flag = 0;
	/// <summary>
	/// Debug levels for the different output streams.
	/// </summary>
	protected internal int[] _debug_level;
	/// <summary>
	/// Status levels for the different output streams.
	/// </summary>
	protected internal int[] _status_level;
	/// <summary>
	/// Warning levels for the different output streams.
	/// </summary>
	protected internal int[] _warning_level;
	/// <summary>
	/// Methods to receive output for the different streams.
	/// </summary>
	protected internal System.Reflection.MethodInfo[] _method;
	/// <summary>
	/// Indicates if any methods (functions) have been registered to receive output.
	/// </summary>
	protected internal bool _method_registered;
	/// <summary>
	/// Associated with _method.
	/// </summary>
	protected internal object[] _object;
	/// <summary>
	/// Newline character to use for output.
	/// </summary>
	protected internal string _newline;
	/// <summary>
	/// Prefix to use for messages.
	/// </summary>
	protected internal string _prefix;
	/// <summary>
	/// Suffix to use for messages.
	/// </summary>
	protected internal string _suffix;
	/// <summary>
	/// Output streams for file output.
	/// </summary>
	protected internal PrintWriter[] _out_stream;
	/// <summary>
	/// Name of log file (perhaps at some point track names of all output receivers?).
	/// </summary>
	protected internal string _logfile;
	/// <summary>
	/// Properties to control display of messages, especially warnings.
	/// </summary>
	protected internal PropList _props = new PropList("Message.props");
	/// <summary>
	/// Use to increase performance rather than hit proplist.
	/// </summary>
	protected internal bool _show_warning_dialog;

	protected internal IList<MessageLogListener> _messageLogListeners;

	/// <summary>
	/// Adds a listener to the list of listeners that can respond to actions from
	/// the MessageLogJFrame.  The listeners are updated by the MessageLogJFrame 
	/// every time its processLogFile() method is called and a log file is read and 
	/// displayed.  Any listeners added after a log file is read will not receive
	/// commands from the MessageLogJFrame until it reads the log file again. </summary>
	/// <param name="listener"> the listener to add. </param>
	protected internal virtual void addMessageLogListener(MessageLogListener listener)
	{
		if (!_initialized)
		{
			initialize();
		}
		_messageLogListeners.Add(listener);
	}

	/// <summary>
	/// Flush and close the log file associated with Message.LOG_OUTPUT, if it has been opened.
	/// </summary>
	protected internal virtual void closeLogFile()
	{
		if (_out_stream[Message.LOG_OUTPUT] != null)
		{
			_out_stream[Message.LOG_OUTPUT].flush();
			_out_stream[Message.LOG_OUTPUT].close();
			_out_stream[Message.LOG_OUTPUT] = null;
		}
	}

	/// <summary>
	/// Flush the output buffers.  It does not appear that this method has any effect on some systems. </summary>
	/// <param name="flag"> Currently unused. </param>
	protected internal virtual void flushOutputFiles(int flag)
	{
		if (!_initialized)
		{
			initialize();
		}
		for (int i = 0; i < MAX_FILES; i++)
		{
			if (_out_stream[i] != null)
			{
				_out_stream[i].flush();
			}
		}
	}

	/// <summary>
	/// Return the debug level for an output stream. </summary>
	/// <returns> The debug level for an output stream number (specified by a *_OUTPUT value). </returns>
	/// <param name="i"> The output stream number. </param>
	protected internal virtual int getDebugLevel(int i)
	{
		if (!_initialized)
		{
			initialize();
		}
		return _debug_level[i];
	}

	/// <summary>
	/// Return the name of the log file. </summary>
	/// <returns> The name of the log file. </returns>
	protected internal virtual string getLogFile()
	{
		return _logfile;
	}

	/// <summary>
	/// Returns the list of listeners that are set to respond to actions from the
	/// MessageLogJFrame.  The returned Vector will never be null. </summary>
	/// <returns> the list of listeners for the MessageLogJFrame. </returns>
	protected internal virtual IList<MessageLogListener> getMessageLogListeners()
	{
		if (!_initialized)
		{
			initialize();
		}

		return _messageLogListeners;
	}

	/// <summary>
	/// Return the value of a Message property.  See setPropValue() for a description of valid properties. </summary>
	/// <returns> the property value or null if not defined. </returns>
	protected internal virtual string getPropValue(string key)
	{
		if (!_initialized)
		{
			initialize();
		}
		return _props.getValue(key);
	}

	/// <summary>
	/// Return the status level for an output stream. </summary>
	/// <returns> The status level for an output stream number (specified by a *_OUTPUT value). </returns>
	/// <param name="i"> The output stream number. </param>
	protected internal virtual int getStatusLevel(int i)
	{
		if (!_initialized)
		{
			initialize();
		}
		return _status_level[i];
	}

	/// <summary>
	/// Return the warning level for an output stream. </summary>
	/// <returns> The warning level for an output stream number (specified by a *_OUTPUT value). </returns>
	/// <param name="i"> The output stream number. </param>
	protected internal virtual int getWarningLevel(int i)
	{
		if (!_initialized)
		{
			initialize();
		}
		return _warning_level[i];
	}

	/// <summary>
	/// Initialize global data.
	/// </summary>
	protected internal virtual void initialize()
	{
		if (_initialized)
		{
			return;
		}
		// For now we want to show the routine and flush the output.  For
		// performance, we will likely turn flushing off at some point...
		_debug_level = new int[MAX_FILES];
		_flag = Message.GUI_FOR_WARNING_1 | Message.SHOW_ROUTINE | Message.FLUSH_OUTPUT;
		_logfile = "";
		_method = new System.Reflection.MethodInfo[MAX_FILES];
		_method_registered = false;
		_newline = System.getProperty("line.separator");
		_object = new object[MAX_FILES];
		_out_stream = new PrintWriter[MAX_FILES];
		_prefix = "";
		_show_warning_dialog = true;
		_status_level = new int[MAX_FILES];
		_suffix = "";
		_warning_level = new int[MAX_FILES];
		_messageLogListeners = new List<MessageLogListener>();

		initStreams();

		_initialized = true;

		// Default values for properties (set after flag is set to avoid
		// recursion - because if debug is turned on the following may call
		// printDebug() which will call this initialize() method if _initialized
		// is false).  Setting these values here assures that null properties
		// will not happen in later code.

		_props.set("ShowWarningDialog=true");
		_props.set("WarningDialogOKNoMoreButton=false");
		_props.set("WarningDialogOKNoMoreButtonLabel=OK - Do Not Show More Warnings");
		_props.set("ShowMessageLevel=false");
		_props.set("ShowMessageTag=false");
	}

	protected internal virtual void initStreams()
	{
		for (int i = 0; i < MAX_FILES; i++)
		{
			_out_stream[i] = null;
			_method[i] = null;
			_object[i] = null;
			_debug_level[i] = 0;
			_status_level[i] = 1; // Default is some status
			_warning_level[i] = 1; // Default is some warning
		}

		// Set the output stream for terminal so that we can debug initial
		// output.  Don't call setOutputFile or we will recurse...

		_out_stream[Message.TERM_OUTPUT] = new PrintWriter(System.out, true);
	}

	/// <summary>
	/// Open the log file.
	/// Because no log file is specified, the name of the log file will default to
	/// the program name and the extension ".log" (set with IOUtil.setProgramName). </summary>
	/// <seealso cref= RTi.Util.IO.IOUtil#setProgramName </seealso>
	/// <returns> The PrintWriter corresponding to the log file. </returns>
	/// <exception cref="IOException"> if there is an error opening the log file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected java.io.PrintWriter openLogFile() throws java.io.IOException
	protected internal virtual PrintWriter openLogFile()
	{
		string message = null, routine = "Message.openLogFile";

		if (!_initialized)
		{
			initialize();
		}
		string program_name = IOUtil.getProgramName();
		if (string.ReferenceEquals(program_name, null))
		{
			// No program name set...
			message = "No program name set.  Cannot use default log file name.";
			Message.printWarning(2, routine, message);
			throw new IOException(message);
		}
		if (program_name.Length == 0)
		{
			// No program name set...
			message = "Program name is zero-length.  Cannot use default log file name.";
			Message.printWarning(2, routine, message);
			throw new IOException(message);
		}
		string logfile = program_name + ".log";
		// The following will throw an IOException if there is an error...
		return openLogFile(logfile);
	}

	/// <summary>
	/// Open the log file using the specified name. </summary>
	/// <returns> The PrintWriter corresponding to the log file. </returns>
	/// <exception cref="IOException"> if there is an error opening the log file. </exception>
	/// <param name="logfile"> Name of log file to open. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected java.io.PrintWriter openLogFile(String logfile) throws java.io.IOException
	protected internal virtual PrintWriter openLogFile(string logfile)
	{
		if (!_initialized)
		{
			initialize();
		}
		string routine = "Message.openLogFile";
		string message = null;

		if (string.ReferenceEquals(logfile, null))
		{
			message = "Null log file.  Not opening log file.";
			Message.printWarning(2, routine, message);
			throw new IOException(message);
		}
		if (logfile.Length <= 0)
		{
			message = "Zero-length log file.  Not opening log file.";
			Message.printWarning(2, routine, message);
			throw new IOException(message);
		}
		PrintWriter ofp = null;
		try
		{
			ofp = new PrintWriter(new StreamWriter(logfile));
			Message.setOutputFile(Message.LOG_OUTPUT, ofp);
		}
		catch (IOException e)
		{
			message = "Unable to open log file \"" + logfile + "\"";
			Message.printWarning(2, routine, message);
			if (Message.isDebugOn)
			{
				// Need to get the stack trace because without the log
				// it will be difficult to troubleshoot other issues.
				e.printStackTrace(_out_stream[Message.TERM_OUTPUT]);
			}
			throw new IOException(message);
		}

		// If we get here, we opened the log file...

		Message.printStatus(1, routine, "Opened log file \"" + logfile + "\".  Previous messages not in file.");

		setLogFile(logfile);
		Message.setOutputFile(Message.LOG_OUTPUT, ofp);

		// Write the log file information...

		ofp.println("#");
		ofp.println("# " + logfile + " - " + IOUtil.getProgramName() + " log file");
		ofp.println("#");
		IOUtil.printCreatorHeader(ofp, "#", 80, 0);
		return ofp;
	}

	/// <summary>
	/// Opens a new file at the specified location. </summary>
	/// <param name="path"> the full path to the new log file to open. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void openNewLogFile(String path) throws Exception
	protected internal virtual void openNewLogFile(string path)
	{
		Message.setLogFile(path);
		Message.openLogFile(path);
	}

	/// <summary>
	/// Print a debug message to the registered output receivers. </summary>
	/// <param name="level"> Debug level for the message. </param>
	/// <param name="routine"> Name of the routine printing the message. </param>
	/// <param name="message"> Debug message. </param>
	protected internal virtual void printDebug(int level, string routine, string message)
	{
		if (!_initialized)
		{
			initialize();
		}
		string dlstring = null;
		if ((_flag & Message.SHOW_MESSAGE_LEVEL) != 0)
		{
			dlstring = "[" + level + "]";
		}
		else
		{
			dlstring = "";
		}
		string routine_string = null;
		if (!string.ReferenceEquals(routine, null))
		{
			if ((_flag & Message.SHOW_ROUTINE) != 0)
			{
				if (routine.Length == 0)
				{
					routine_string = "";
				}
				else
				{
					routine_string = "(" + routine + ")";
				}
			}
			else
			{
				routine_string = "";
			}
		}
		else
		{
			routine_string = "";
		}

		object[] arg_list = null;

		// Format the final string...

		string message2 = _prefix + "Debug" + dlstring + routine_string + ": " + message + _suffix;

		// Only do the following if methods are registered...
		if (_method_registered)
		{
			arg_list = new object[3];
			arg_list[0] = new int?(level);
			arg_list[1] = routine;
			arg_list[2] = message2;
		}

		for (int i = 0; i < MAX_FILES; i++)
		{
			if (_out_stream[i] != null && level <= _debug_level[i])
			{
				_out_stream[i].print(message2 + _newline);
				if ((_flag & Message.FLUSH_OUTPUT) != 0)
				{
					_out_stream[i].flush();
				}
			}

			if ((_method[i] != null) && (_object[i] != null) && (level <= _debug_level[i]))
			{
				try
				{
					_method[i].invoke(_object[i], arg_list);
				}
				catch (Exception e)
				{
					printDebug(level, routine, e);
				}
			}
		}
		dlstring = null;
		routine_string = null;
		message2 = null;
		if (_method_registered)
		{
			arg_list[0] = null;
			arg_list[1] = null;
			arg_list[2] = null;
			arg_list = null;
		}
	}

	/// <summary>
	/// Print a stack trace as if debug message.  Currently, this will print to the
	/// log file if it is not null. </summary>
	/// <param name="level"> Debug level for the message. </param>
	/// <param name="routine"> Name of the routine printing the message. </param>
	/// <param name="e"> Throwable (e.g., Error, Exception) for which to print a stack trace. </param>
	protected internal virtual void printDebug(int level, string routine, Exception e)
	{
		if (!_initialized)
		{
			initialize();
		}
		printDebug(level, routine, "Exception stack trace follows (see log file)...");
		if (e != null)
		{
			printDebug(level, routine, e.Message);
			if (_out_stream[Message.LOG_OUTPUT] != null)
			{
				e.printStackTrace(_out_stream[Message.LOG_OUTPUT]);
			}
		}
		else
		{
			printDebug(level, routine, "Null Throwable.");
		}
		printDebug(level, routine, "... end of exception stack trace.");
	}

	/// <summary>
	/// Print information about the registered message levels using status messages,
	/// using the current message settings.
	/// </summary>
	protected internal virtual void printMessageLevels()
	{
		if (!_initialized)
		{
			initialize();
		}
		printMessageLevels(true);
	}

	/// <summary>
	/// Print information about the registered message levels using the current message settings. </summary>
	/// <param name="flag"> If true, print using status messages.  If false, print to the system standard output. </param>
	protected internal virtual void printMessageLevels(bool flag)
	{
		int i;
		string @string = null;

		if (!_initialized)
		{
			initialize();
		}
		if (flag)
		{
			// Print using status messages...
			printStatus(1, "", "");
			printStatus(1, "", "Is debug turned on:  " + Message.isDebugOn);
			printStatus(1, "", "");
			printStatus(1, "", "-------------------------------------------------");
			printStatus(1, "", "File  Debug   Status  Warning  File      Function");
			printStatus(1, "", "#     Level   Level   Level    Attached  Attached");
			printStatus(1, "", "-------------------------------------------------");
			for (i = 0; i < MAX_FILES; i++)
			{
				@string = StringUtil.formatString(i,"%-3d") + "   " + StringUtil.formatString(_debug_level[i], "%-5d") + "   " + StringUtil.formatString(_status_level[i], "%-5d") + "   " + StringUtil.formatString(_warning_level[i], "%-5d") + "    ";
				// Now indicate whether files are attached for the different message types.
				// Since all the types go to the same file, just output one.
				if (_out_stream[i] != null)
				{
					@string = @string + "  Y       ";
				}
				else
				{
					@string = @string + "          ";
				}
				// Now indicate whether functions are attached for the different message types...
				if (_method[i] != null)
				{
					@string = @string + "  Y  ";
				}
				else
				{
					@string = @string + "     ";
				}
				printStatus(1, "", @string);
			}
		}
		else
		{
			// Print to the system...
			PrintWriter fp = new PrintWriter(System.out, true);

			fp.println();
			fp.println("-------------------------------------------------");
			fp.println("File  Debug   Status  Warning  File      Function");
			fp.println("#     Level   Level   Level    Attached  Attached");
			fp.println("-------------------------------------------------");
			for (i = 0; i < MAX_FILES; i++)
			{
				@string = StringUtil.formatString(i,"%-3d") + "   " + StringUtil.formatString(_debug_level[i], "%-5d") + "   " + StringUtil.formatString(_status_level[i], "%-5d") + "   " + StringUtil.formatString(_warning_level[i], "%-5d") + "    ";
				// Now indicate whether files are attached for the different message types.
				// Since all the types go to the same file, just output one.
				if (_out_stream[i] != null)
				{
					@string = @string + "  Y  ";
				}
				else
				{
					@string = @string + "     ";
				}
				@string = @string + "     ";
				// Now indicate whether functions are attached for the different message types...
				if (_method[i] != null)
				{
					@string = @string + "  Y  ";
				}
				else
				{
					@string = @string + "     ";
				}
				fp.println(@string);
			}
			fp.close();
			fp.flush();
			fp = null;
		}
		@string = null;
	}

	/// <summary>
	/// Print a status message to the registered output receivers. </summary>
	/// <param name="level"> Status level for the message. </param>
	/// <param name="routine"> Name of the routine printing the message. </param>
	/// <param name="message"> Status message. </param>
	protected internal virtual void printStatus(int level, string routine, string message)
	{
		if (!_initialized)
		{
			initialize();
		}

		string slstring = null;
		if ((_flag & Message.SHOW_MESSAGE_LEVEL) != 0)
		{
			slstring = "[" + level + "]";
		}
		else
		{
			slstring = "";
		}
		string routine_string = "";
		if (!string.ReferenceEquals(routine, null))
		{
			// We only show the routine if we have requested it and the
			// status level is greater than 1.
			if ((_flag & Message.SHOW_ROUTINE) != 0)
			{
				if (level > 1)
				{
					if (routine.Length == 0)
					{
						routine_string = "";
					}
					else
					{
						routine_string = "(" + routine + ")";
					}
				}
			}
		}

		object[] arg_list = null;

		// Format the final string...

		string message2 = _prefix + "Status" + slstring + routine_string + ": " + message + _suffix;

		if (_method_registered)
		{
			arg_list = new object[3];
			arg_list[0] = new int?(level);
			arg_list[1] = "Status: ";
			arg_list[2] = message2;
		}

		DateTime now = null;
		if (IOUtil.testing())
		{
			now = new DateTime(DateTime.DATE_CURRENT);
			now.setPrecision(DateTime.PRECISION_SECOND);
		}

		for (int i = 0; i < MAX_FILES; i++)
		{
			if ((_out_stream[i] != null) && (level <= _status_level[i]))
			{
				_out_stream[i].print(message2 + _newline);
				_out_stream[i].flush();
			}
			if ((_method[i] != null && _object[i] != null) && (level <= _status_level[i]))
			{
				try
				{
					_method[i].invoke(_object[i], arg_list);
				}
				catch (Exception)
				{
					//System.out.println( 
					//"Exception (Message.printStatus): " + e );
				}
			}
		}
		slstring = null;
		routine_string = null;
		message2 = null;
		if (_method_registered)
		{
			arg_list[0] = null;
			arg_list[1] = null;
			arg_list[2] = null;
			arg_list = null;
		}
	}

	/// <summary>
	/// This method calls printWarning but allows the developer to specify a
	/// different _top_level frame from the preset top level frame.  This is useful
	/// when you are printing a warning of level 1 and want to specify which window
	/// that WarningDialog should be associated with, without changing the top
	/// level window that should be typically used.
	/// </summary>
	protected internal virtual void printWarning(int level, string routine, string message, JFrame top_level)
	{ // Save current global top level JFrame...
		JFrame permanent_top_level = _top_level;

		if (top_level != null)
		{
			_top_level = top_level;
		}
		printWarning(level, routine, message);

		// Reset back to original...
		_top_level = permanent_top_level;
	}

	/// <summary>
	/// Print a warning message to the registered output receivers.  The overloaded
	/// version is called without the tag. </summary>
	/// <param name="level"> Warning level for the message. </param>
	/// <param name="routine"> Name of the routine printing the message. </param>
	/// <param name="message"> Warning message. </param>
	protected internal virtual void printWarning(int level, string routine, string message)
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
	protected internal virtual void printWarning(int level, string tag, string routine, string message)
	{
		if (!_initialized)
		{
			initialize();
		}

		string wlstring = null;
		if ((_flag & Message.SHOW_MESSAGE_LEVEL) != 0)
		{
			wlstring = "[" + level + "]";
		}
		else
		{
			wlstring = "";
		}

		string tagstring = null;
		if (((_flag & __SHOW_MESSAGE_TAG) != 0) && (!string.ReferenceEquals(tag, null)) && !tag.Equals(""))
		{
			tagstring = "<" + tag + ">";
		}
		else
		{
			tagstring = "";
		}

		string routine_string = null;
		if (!string.ReferenceEquals(routine, null))
		{
			if (((_flag & Message.SHOW_ROUTINE) != 0) && (level > 1))
			{
				if (routine.Length == 0)
				{
					routine_string = "";
				}
				else
				{
					routine_string = "(" + routine + ")";
				}
			}
			else
			{
				routine_string = "";
			}
		}
		else
		{
			routine_string = "";
		}

		// Format the final string...

		// TODO SAM 2005-03-11 Not sure of warning level and tag should
		// always be shown or should only be in the log file.  Need to evaluate
		// with StateDMI and TSTool, which are probably the only applications
		// that will initially use the extra information.
		string message2 = _prefix + "Warning" + wlstring + tagstring + routine_string + ": " + message + _suffix;

		object[] arg_list = null;

		if (_method_registered)
		{
			arg_list = new object[3];
			arg_list[0] = new int?(level);
			arg_list[1] = routine;
			arg_list[2] = message2;
		}

		for (int i = 0; i < MAX_FILES; i++)
		{
			if (_out_stream[i] != null && level <= _warning_level[i])
			{
				_out_stream[i].print(message2 + _newline);
				if ((_flag & Message.FLUSH_OUTPUT) != 0)
				{
					_out_stream[i].flush();
				}
			}
			if (_method[i] != null && _object[i] != null && level <= _warning_level[i])
			{
				try
				{
					_method[i].invoke(_object[i], arg_list);
				}
				catch (Exception e)
				{
					printWarning(level,routine,e);
				}
			}
		}

		// Now pop up the MessageJDialog if necessary.

		if (_show_warning_dialog)
		{
			if ((level == 1) && ((_flag & Message.GUI_FOR_WARNING_1) != 0) && (_top_level != null) && !IOUtil.isBatch())
			{ // Batch = no GUI
				new MessageJDialog(_top_level, message2);
			}
		}
		wlstring = null;
		tagstring = null;
		routine_string = null;
		message2 = null;
		if (_method_registered)
		{
			arg_list[0] = null;
			arg_list[1] = null;
			arg_list[2] = null;
			arg_list = null;
		}
	}

	/// <summary>
	/// Print a stack trace as if a warning message.  Output will only be to the log file, if open. </summary>
	/// <param name="level"> Warning level for the message. </param>
	/// <param name="routine"> Name of the routine printing the message. </param>
	/// <param name="e"> Throwable (e.g. Error, Exception) for which to print a stack trace. </param>
	protected internal virtual void printWarning(int level, string routine, Exception e)
	{
		if (!_initialized)
		{
			initialize();
		}
		Console.WriteLine(e.ToString());
		Console.Write(e.StackTrace);

		if (IOUtil.isRunningApplet())
		{
			Console.WriteLine("Exception stack trace follows...");
			Console.WriteLine(e.ToString());
			Console.Write(e.StackTrace);
			Console.WriteLine("... end of exception stack trace.");
		}
		else
		{
			printWarning(level, routine, "Exception stack trace follows (see log file)...");
			if (e != null)
			{
				printWarning(level, routine, e.Message);
				if (_out_stream[Message.LOG_OUTPUT] != null)
				{
					e.printStackTrace(_out_stream[Message.LOG_OUTPUT]);
				}
			}
			printWarning(level, routine, "... end of exception stack trace.");
		}
	}

	/// <summary>
	/// Removes a listener from the Vector of listeners that are set to listen to 
	/// actions from the MessageLogJFrame. </summary>
	/// <param name="listener"> the listener to remove from the Vector of MessageLogListeners. </param>
	protected internal virtual void removeMessageLogListener(MessageLogListener listener)
	{
		if (!_initialized)
		{
			initialize();
		}

		int size = _messageLogListeners.Count;
		MessageLogListener temp = null;
		for (int i = (size - 1); i >= 0; i--)
		{
			temp = (MessageLogListener)_messageLogListeners[i];

			if (temp == listener)
			{
				_messageLogListeners.RemoveAt(i);
			}
		}
	}

	/// <summary>
	/// Closes and opens the log file, so old data will be overwritten next time something is logged.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void restartLogFile() throws Exception
	protected internal virtual void restartLogFile()
	{
		Message.closeLogFile();
		Message.openLogFile(getLogFile());
	}

	/// <summary>
	/// Set the output behavior flags (e.g., SHOW_MESSAGE_LEVEL) as a bit mask.
	/// Currently this sets the given bit (but does not unset the others).
	/// <b>The handling of these flags needs more work (the defaults are usually OK).</b>
	/// See setProp() for additional properties used to control message behavior. </summary>
	/// <param name="flag"> Bit mask for flag to set. </param>
	protected internal virtual void setBehaviorFlag(int flag)
	{
		if (!_initialized)
		{
			initialize();
		}
		int current = _flag;
		_flag = current | flag;
	}

	/// <summary>
	/// Set the flag indicating whether debug is on or off.  This is called by the
	/// setDebugLevel method if the debug level is greater than zero for any output
	/// receiver.  If debug is turned off, then debug messages that check this flag will run much faster. </summary>
	/// <param name="flag"> true if debug should be turned on, false if not. </param>
	protected internal virtual void setDebug(bool flag)
	{
		if (!_initialized)
		{
			initialize();
		}
		Message.isDebugOn = flag;
	}

	/// <summary>
	/// Set the debug level for an output receiver. If the level is > 0, do not set
	/// debugging to on.  The debug levels are independent of whether debug is actually on or off. </summary>
	/// <param name="i"> Output receiver number (the *_OUTPUT values). </param>
	/// <param name="level"> Debug level for the output receiver.
	/// TODO JAVADOC: see RTi.Util.Message.Message.setDebug </param>
	protected internal virtual void setDebugLevel(int i, int level)
	{
		if (!_initialized)
		{
			initialize();
		}
		string routine = "setDebugLevel";
		if (i >= MAX_FILES)
		{
			printWarning(1, routine, "Attempting to set level " + i + ". Only " + MAX_FILES + " are available.");
			return;
		}
		_debug_level[i] = level;
		printStatus(1, routine, "Set debug level for " + Message.OUTPUT_NAMES[i] + " to " + level);
		routine = null;
	}

	/// <summary>
	/// Set the debug level by parsing the levels from a string like "#,#", "#", ",#",
	/// etc.  The TERM_OUTPUT and LOG_OUTPUT levels are set. </summary>
	/// <param name="debug_level"> Debug level as a string. </param>
	protected internal virtual void setDebugLevel(string debug_level)
	{
		if (string.ReferenceEquals(debug_level, null))
		{
			return;
		}
		if (debug_level.Length == 0)
		{
			return;
		}

		// Split the argument...

		IList<string> list = null;
		int nlist = 0;
		if (debug_level.Length > 0)
		{
			list = StringUtil.breakStringList(debug_level, ",", 0);
			nlist = list.Count;
		}
		else
		{
			nlist = 0;
		}

		int level0, level1;
		if (nlist == 1)
		{
			//
			// #
			//
			level0 = StringUtil.atoi(list[0].ToString());
			level1 = level0;
			setDebugLevel(Message.TERM_OUTPUT, level0);
			setDebugLevel(Message.LOG_OUTPUT, level1);
		}
		else
		{
			//
			// ,# or #,#
			//
			string tmp = list[0].ToString();

			if (tmp.Length > 0)
			{
				level0 = StringUtil.atoi(tmp);
				setDebugLevel(Message.TERM_OUTPUT, level0);
			}
			tmp = list[1].ToString();

			if (tmp.Length > 0)
			{
				level1 = StringUtil.atoi(tmp);
				setDebugLevel(Message.LOG_OUTPUT, level1);
			}
			tmp = null;
		}
		list = null;
	}

	/// <summary>
	/// Set the log file name for the log file. </summary>
	/// <param name="logfile"> Name of log file. </param>
	protected internal virtual void setLogFile(string logfile)
	{
		if (!string.ReferenceEquals(logfile, null))
		{
			_logfile = logfile;
		}
	}

	/// <summary>
	/// Set the output file for an output receiver. </summary>
	/// <param name="i"> Output receiver (the *_OUTPUT values). </param>
	/// <param name="output_stream"> Output PrintStream for the output receiver (usually a log
	/// file).  The default is no log file. </param>
	protected internal virtual void setOutputFile(int i, PrintStream output_stream)
	{
		setOutputFile(i, new PrintWriter(output_stream, true));
	}

	/// <summary>
	/// Set the output file for an output receiver. </summary>
	/// <param name="i"> Output receiver (the *_OUTPUT values). </param>
	/// <param name="output_stream"> Output PrintWriter for the output receiver (usually a log
	/// file).  The default is no log file. </param>
	protected internal virtual void setOutputFile(int i, PrintWriter output_stream)
	{
		if (!_initialized)
		{
			initialize();
		}
		if (i >= MAX_FILES)
		{
			printWarning(1, "Message.setOutputFile", "Attempting to set file " + i + ". Only " + MAX_FILES + " are available.");
			return;
		}
		_out_stream[i] = output_stream;
	}

	/// <summary>
	/// Set the function to call for an output receiver. </summary>
	/// <param name="i"> Output receiver (the *_OUTPUT values). </param>
	/// <param name="obj"> Object containing the function to call. </param>
	/// <param name="function_name"> Name of function to call to receive message.  The function
	/// must take an integer (message level), a String (the routine name), and a second String (the message). </param>
	protected internal virtual void setOutputFunction(int i, object obj, string function_name)
	{
		if (!_initialized)
		{
			initialize();
		}
		string routine = "Message.setOutputFunction";
		if (i >= MAX_FILES)
		{
			printWarning(1, routine, "Attempting to set function " + i + ". Only " + MAX_FILES + " are available.");
			return;
		}

		if (obj == null)
		{
			// Just set it (to turn off stream) and return...
			_object[i] = null;
			_method[i] = null;
			// If all objects and methods are null, set functions to off...
			bool nonnull_found = false;
			for (int j = 0; j < MAX_FILES; j++)
			{
				if ((_object[j] != null) || (_method[j] != null))
				{
					nonnull_found = true;
					break;
				}
			}
			if (!nonnull_found)
			{
				_method_registered = false;
			}
			return;
		}

		Type[] args = new Type[3];

		args[0] = typeof(int);
		args[1] = typeof(string);
		args[2] = typeof(string);

		System.Reflection.MethodInfo method;
		try
		{
			method = obj.GetType().GetMethod(function_name, args);
		}
		catch (NoSuchMethodException)
		{
			Message.printWarning(2, routine, "Error getting \"Method\" for " + function_name + "( int, String, String )");
			return;
		}
		_object[i] = obj;
		_method[i] = method;
		_method_registered = true;
		routine = null;
	}

	/// <summary>
	/// Set the prefix to use in front of all messages (the default is nothing).
	/// This can be used, for example, to format messages for HTML. </summary>
	/// <param name="prefix"> Prefix string to use in front of all messages. </param>
	protected internal virtual void setPrefix(string prefix)
	{
		if (!_initialized)
		{
			initialize();
		}
		if (!string.ReferenceEquals(prefix, null))
		{
			_prefix = prefix;
		}
	}

	/// <summary>
	/// Set a property used to control message behavior.  These properties are currently
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
	/// <td>Indicate with true or false whether a button should be enabled to view the
	/// log file.
	/// </td>
	/// <td>false</td>
	/// </tr>
	/// </table>
	/// </summary>
	protected internal virtual void setPropValue(string prop)
	{
		if (!_initialized)
		{
			initialize();
		}
		_props.set(prop); // Set the property no matter what it is.
		// Check some properties and set other variables to optimize performance...
		if (_props.getValue("ShowWarningDialog").Equals("true", StringComparison.OrdinalIgnoreCase))
		{
			_show_warning_dialog = true;
		}
		else
		{
			_show_warning_dialog = false;
		}
		if (_props.getValue("ShowMessageLevel").Equals("true", StringComparison.OrdinalIgnoreCase))
		{
			// Turn on the level...
			_flag |= Message.SHOW_MESSAGE_LEVEL;
		}
		else
		{
			// Turn off the level...
			_flag ^= Message.SHOW_MESSAGE_LEVEL;
		}
		if (_props.getValue("ShowMessageTag").Equals("true", StringComparison.OrdinalIgnoreCase))
		{
			// Turn on the level...
			_flag |= __SHOW_MESSAGE_TAG;
		}
		else
		{
			// Turn off the level...
			_flag ^= __SHOW_MESSAGE_TAG;
		}
	}

	/// <summary>
	/// Set the status level for the application.  Only messages with levels less than
	/// or equal to the set level will be printed. </summary>
	/// <param name="i"> Output receiver (the *_OUTPUT values). </param>
	/// <param name="level"> Status message level for the application. </param>
	protected internal virtual void setStatusLevel(int i, int level)
	{
		if (!_initialized)
		{
			initialize();
		}
		string routine = "setStatusLevel";
		if (i >= MAX_FILES)
		{
			printWarning(1, routine, "Attempting to set level " + i + ". Only " + MAX_FILES + " are available.");
			return;
		}
		_status_level[i] = level;
		printStatus(1, routine, "Set status level for " + Message.OUTPUT_NAMES[i] + " to " + level);
		routine = null;
	}

	/// <summary>
	/// Set the suffix to use at the end of all messages (the default is nothing).
	/// This can be used, for example, to format messages for HTML. </summary>
	/// <param name="suffix"> Suffix string to use behind all messages. </param>
	protected internal virtual void setSuffix(string suffix)
	{
		if (!_initialized)
		{
			initialize();
		}
		if (!string.ReferenceEquals(suffix, null))
		{
			_suffix = suffix;
		}
	}

	/// <summary>
	/// Set the top-level frame for the application that uses messages.  This allows
	/// the modal warning dialog to be created. </summary>
	/// <param name="f"> Top-level JFrame for application. </param>
	protected internal virtual void setTopLevel(JFrame f)
	{
		if (!_initialized)
		{
			initialize();
		}
		_top_level = f;
	}

	/// <summary>
	/// Set the warning level for the application.  Only messages with levels less than
	/// or equal to the set level will be printed. </summary>
	/// <param name="i"> Output receiver (the *_OUTPUT values). </param>
	/// <param name="level"> Warning message level for the application. </param>
	protected internal virtual void setWarningLevel(int i, int level)
	{
		if (!_initialized)
		{
			initialize();
		}
		string routine = "setWarningLevel";
		if (i >= MAX_FILES)
		{
			printWarning(1, routine, "Attempting to set level " + i + ". Only " + MAX_FILES + " are available.");
			return;
		}
		_warning_level[i] = level;
		printStatus(1, routine, "Set warning level for " + Message.OUTPUT_NAMES[i] + " to " + level);
		routine = null;
	}

	}

}