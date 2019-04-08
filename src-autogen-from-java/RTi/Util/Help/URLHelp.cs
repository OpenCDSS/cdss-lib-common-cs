﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;

// URLHelp - On-line help class.

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
// URLHelp - On-line help class.
//------------------------------------------------------------------------------
// Copyright: See the COPYRIGHT file.
//------------------------------------------------------------------------------
// Notes:   This class contains on-line help static methods which may be
//          called from other classes as well as private methods to assist when
//          setting up the keys and URL addresses.
//------------------------------------------------------------------------------
// History:
//
// 01 Aug 1997	Darrell Gillmeister,	Created initial version.
//		RTi
// 14 Aug 1997	DLG, RTi		Modified constructors, setIndexURL,
//					readIndex, setBrowser.
// 15 Aug 1997	DLG, RTi		Modified readIndex.
// 18 Aug 1997	DLG, RTi		Working version created.
// 27 Aug 1997	DLG, RTi		Changed package to RTi.Util.Help.
// 03 Dec 1997	Matthew J. Rutherford, 	Changed stuff to make this whole class
//		RTi			static.
// 25 Jan 1998	Steven A. Malers, RTi	Upgrade to allow interaction with the
//					URLHelpGUI.  Alphabetize functions so
//					this class is easier to maintain.
// 28 Jan 1998	SAM, RTi		Update to use IO.getInputStream and
//					test whether a stand-alone application
//					can open a URL connection.  Use the
//					IO class for applet information.
// 02 Feb 1998	SAM, RTi		Use PropList to allow some flexibility
//					in the index file format.
// 17 Apr 1998	SAM, RTi		If a key is not found, print a nice
//					message at level 1 so we can use to
//					clean up the context-sensitive help.
// 03 Jul 1998	CGB, RTi		Added method showTopic which was
//					simply a split out of existing
//					processing code which launched the
//					browser with a specified URL.  The break
//					allows us to launch the browser for
//					addtional files other than the help
//					files.
// 14 Apr 1999	SAM, RTi		Change so the help index file is not
//					read when the index file is specified.
//					Only read when readIndex() is explicitly
//					called or... try to read when
//					showHelpForIndex() or showHelpForKey()
//					are called and the index has not been
//					read before (use a static to track).
//					If the help index file is reset, the
//					static is set back to indicating that
//					it has not been read.  The URLHelpGUI
//					now takes more care also.  Change
//					showTopic() to showURL().
// 2001-11-14	SAM, RTi		Change IO to IOUtil.  Review Javadoc.
//					Set variables to null if no longer used.
//					Add initialize() method to set the
//					default browser and index file using
//					run-time information.  Synchronize a few
//					methods to try to help with GUIs not
//					starting up right.
// 2001-12-11	SAM, RTi		Having some problems with the URLHelpGUI
//					throwing exceptions at startup.  It
//					looks like the thread used to get the
//					default browser may not be finishing in
//					time and the URLHelpGUI is using an
//					incompletely initialized URLHelp.
// 2002-07-10	SAM, RTi		Track down bug with process manager.
// 2002-10-11	SAM, RTi		Change ProcessManager to ProcessManager1
//					to maintain functionality - need to
//					update to take advantage of Java 1.4+.
// 2002-10-16	SAM, RTi		Change back to ProcessManager since in
//					its updated form it seems to work for
//					1.1.8 and 1.4.0.  Use a command array
//					when calling.  Also use ProcessManager
//					when spawning the browser.
// 2003-02-01	SAM, RTi		Add setHelpOverride() method to override
//					the help system with a message.  This is
//					being used to bypass the on-line help
//					system for software in favor of PDF
//					files until a better help system can be
//					implemented with Swing components.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//------------------------------------------------------------------------------
// EndHeader

namespace RTi.Util.Help
{

	using IOUtil = RTi.Util.IO.IOUtil;
	using ProcessManager = RTi.Util.IO.ProcessManager;
	using PropList = RTi.Util.IO.PropList;
	using PropListManager = RTi.Util.IO.PropListManager;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This class provides an interface to display on-line help using an index file.
	/// <para>
	/// 
	/// To implement the on-line help interface, first create a text index file and
	/// specify its name to the class using the <i>setIndexURL()</i> method.  This
	/// function can be called by the code that parses the program's command line
	/// options, properties (or parameters if an applet).  In this case, the index file
	/// is read when the first request for help is made (to increase startup
	/// performance).
	/// The <tt>-helpindex</tt> option is the standard
	/// for specifying the help index file.  For example, call the index file
	/// <i>FloodWatch_help_index.txt</i>.  If running
	/// stand-alone, the file can be a normal text file (use a file: URL protocol).  If
	/// running through a browser, specify a http protocol URL.  In either case,
	/// the index file should have
	/// a <i>.txt</i> extension.  This class uses the
	/// <i>RTi.Util.IO.IOUtil.isApplet()</i>
	/// method to
	/// determine whether the code is running as an applet so make sure to call
	/// <i>RTi.Util.IO.IOUtil.setApplet()</i> in the applet's <i>init()</i> function.
	/// </para>
	/// <para>
	/// Another alternative (using a more recent approach) to setting up the help
	/// system is to do something similar to
	/// the following (in this case the HBParse.getHome() call returns the top level
	/// install point for the software):
	/// <pre>
	/// // Initialize help to run-time values...
	/// Vector helphome = new Vector();
	/// helphome.addElement ( HBParse.getHome() + "/" +
	///				"doc/html/manuals/TSTool" );
	/// helphome.addElement ( "/Program Files/RTi/doc/html/manuals/TSTool" );
	/// helphome.addElement ( "C:/Program Files/RTi/doc/html/manuals/TSTool" );
	/// helphome.addElement ( "D:/Program Files/RTi/doc/html/manuals/TSTool" );
	/// helphome.addElement ( "http://cdss.state.co.us/manuals/TSTool" );
	/// URLHelp.initialize ( null, helphome, "tstool_help_index.txt" );
	/// helphome = null;
	/// // Now hook in the help system...
	/// URLHelpGUI _help_index_gui = new URLHelpGUI();
	/// _help_index_gui.attachMainMenu ( helpMenu );
	/// </pre>
	/// </para>
	/// <para>
	/// The format of the
	/// index file is as shown below.  If using the URLHelpGUI interface, it is
	/// important to list the topics in an appropriate order because they will be
	/// displayed in the order that they are read from the file.
	/// </para>
	/// <para>
	/// <b>To work dynamically, the DOCHOME property is now set internally in the
	/// software.  In the readIndex() method, when the DOCHOME property is encountered
	/// it is reset to the base directory name of the index file.</b>
	/// 
	/// <pre>
	/// # Comments start with #
	/// #
	/// # Key  URL  Topic (examples shown below)
	/// #
	/// # Variables can be defined similar to Makefiles
	/// #
	/// # On-line help index for FloodWatch
	/// 
	/// # Uncomment to use documentation on a web server...
	/// 
	/// SERVER=http://www
	/// 
	/// # Uncomment to use documentation on a local machine...
	/// 
	/// #SERVER=file:///I:/win95/local/java
	/// 
	/// DOCHOME=$(SERVER)/html
	/// 
	/// FloodWatch.MainGUI		"$(DOCHOME)/FloodWatch.html#Main"	"Main Window"
	/// FloodWatch.Options		"$(DOCHOME)/FloodWatch.html#Options"	"Options"
	/// FloodWatch.Enhance		"$(DOCHOME)/FloodWatch.html#Enhancements"	"Planned FloodWatch Enhancements"
	/// FloodWatch.Enhance.FP		"$(DOCHOME)/FloodWatch.html#EnhanceFP"	"  Selecting a Forecast Point"
	/// FloodWatch.Enhance.Graph	"$(DOCHOME)/FloodWatch.html#EnhanceGraph"	"  Graphing Time Series"
	/// </pre>
	/// 
	/// Keys should not contain spaces and should be as specific as possible to
	/// cross-reference with GUI components or topics.  URLs can be specified using
	/// variables.  Topics should be enclosed in double quotes to protect whitespace.
	/// Finally, code the event handlers for GUIs to call the <i>showHelpForKey()</i>
	/// function
	/// with keys that match the entries in the index file.  If running stand-alone,
	/// the browser set with <i>setBrowser()</i> will be called.  This function is
	/// normally called by the code that parses the command line arguments for the
	/// program (or the parameters if an applet).  The <tt>-browser</tt> option is
	/// the standard notation.  If running as an
	/// applet, the help will be shown in a new (blank) browser window.
	/// 
	/// </para>
	/// </summary>
	/// <seealso cref= URLHelpGUI </seealso>
	/// <seealso cref= URLHelpData </seealso>

	public class URLHelp
	{

	/// <summary>
	/// Indicates whether the index file has been successfuly read.  This is used
	/// to determine whether help can be displayed.
	/// </summary>
	protected internal static bool _isHelpAvailable = false;

	/// <summary>
	/// Indicates whether for the current index an attempt to read the index has been
	/// made.  This is used to optimize the start-up and eliminate retries to read
	/// the index from a remote URL.
	/// </summary>
	protected internal static bool _hasIndexReadBeenTried = false;

	/// <summary>
	/// A list of String to use with setHelpOverride() instead of trying to display a help topic.
	/// </summary>
	private static IList<string> __help_override_List = null;

	/// <summary>
	/// A list of URLHelpData containing help index data.
	/// </summary>
	protected internal static IList<URLHelpData> _data = null;

	/// <summary>
	/// The path to the browser executable to run when running stand-alone (not as an
	/// applet).  Will be an empty string if not set.
	/// </summary>
	private static string _browser = "";

	/// <summary>
	/// The URL to the help index file.  Will be an empty string if not set.
	/// </summary>
	private static string _index_URL = "";

	/// <summary>
	/// Indicate whether new initialization is being used.
	/// </summary>
	private static bool _new_initialization = false;

	/// <summary>
	/// Show help for a key word. </summary>
	/// @deprecated Replaced by showHelpForKey(), showHelpForIndex(), showHelpForTopic() 
	/// <param name="key"> Key corresponding to the topic to display. </param>
	public static void displayHelp(string key)
	{
		showHelpForKey(key);
	}

	/// <summary>
	/// Return the integer position of the key in the index. </summary>
	/// <returns> The integer position of the key in the index, starting at zero, or
	/// -1 if not found. </returns>
	/// <param name="key"> The key for the help index topic of interest. </param>
	private static int findIndexForKey(string key)
	{
		if (!isHelpAvailable())
		{
			return -1;
		}

		// Loop through the _data Vector and find the requested key...
		int size = _data.Count;
		URLHelpData idata;
		for (int i = 0; i < size; i++)
		{
			idata = (URLHelpData)_data[i];
			// If the present key element, i, matches the requested
			// key, return the corresponding index...
			if (key.Equals(idata.getKey(), StringComparison.OrdinalIgnoreCase))
			{
				idata = null;
				return i;
			}
		}
		Message.printWarning(1, "URLHelp.findIndexForKey", "Requested help key \"" + key + "\" not found\n" + "in the help index file: \"" + _index_URL + "\".\n" + "The documentation and software are inconsistent.");
		idata = null;
		return -1;
	}

	/// <summary>
	/// Return the integer position of the topic in the index. </summary>
	/// <returns> The integer position of the topic in the index, starting at zero, or
	/// -1 if not found. </returns>
	/// <param name="topic"> The topic of interest. </param>
	private static int findIndexForTopic(string topic)
	{
		if (!isHelpAvailable())
		{
			return -1;
		}

		// Loop through the _data Vector and find the requested topic...
		int size = _data.Count;
		URLHelpData idata;
		for (int i = 0; i < size; i++)
		{
			idata = (URLHelpData)_data[i];
			// If the present Topic element, i, matches the requested
			// key, return the corresponding index...
			if (topic.Equals(idata.getTopic(), StringComparison.OrdinalIgnoreCase))
			{
				idata = null;
				return i;
			}
		}
		Message.printWarning(3, "URLHelp.findIndexForTopic", "Requested help topic \"" + topic + "\" not found.");
		idata = null;
		return -1;
	}

	/// <summary>
	/// The path to the browser to execute when displaying help. </summary>
	/// <returns> The path to the browser to execute when displaying help stand-alone
	/// (not an applet). </returns>
	public static string getBrowser()
	{
		lock (typeof(URLHelp))
		{
			return _browser;
		}
	}

	/// <summary>
	/// Return the URLHelpData data list.  Used by URLHelpGUI for displays. </summary>
	/// <returns> The URLHelpData data list. </returns>
	protected internal static IList<URLHelpData> getData()
	{
		lock (typeof(URLHelp))
		{
			return _data;
		}
	}

	/// <summary>
	/// Return the URL for the help index file. </summary>
	/// <returns> The URL for the help index file. </returns>
	public static string getIndexURL()
	{
		lock (typeof(URLHelp))
		{
			return _index_URL;
		}
	}

	/// <summary>
	/// Initialize the URLHelp instance.  This method should be called before any
	/// specific set methods.  The following tasks are performed:
	/// <ol>
	/// <li>	If the browser parameter is not set, the browser executable is set to
	/// the default browser on the computer (PC only; UNIX not enabled).  This
	/// requires that the shellcom executable be available.  If the
	/// browser parameter is set, it is used as is.</li>
	/// <li>	The index file is located using the combination of index_paths and
	/// index_file.
	/// </ol> </summary>
	/// <param name="browser"> Path to browser to use.  If null, use the default browser from
	/// the system. </param>
	/// <param name="index_paths"> Leading path URLs to prepend to the index_file to search for
	/// an existing index file.  If not specified, use the index_file without a leading
	/// path.  The paths should not include the directory separator. </param>
	/// <param name="index_file"> Index file to append to the index_paths, to locate the index
	/// file. </param>
	public static void initialize(string browser, IList<string> index_paths, string index_file)
	{ // First set the browser...
		lock (typeof(URLHelp))
		{
			if (!string.ReferenceEquals(browser, null))
			{
				setBrowser(browser);
			}
			else
			{ // Run a process to get the default browser.  Rely on the
				// timeout rather than a STOP code so that the shellcon
				// program does not need to print a stop code.
				if (!IOUtil.isUNIXMachine())
				{
					try
					{
						string[] command_array = new string[2];
						command_array[0] = "shellcon";
						command_array[1] = "-defaultbrowser";
						ProcessManager pm = new ProcessManager(command_array);
						pm.saveOutput(true);
						pm.run();
						IList<string> output = pm.getOutputList();
						pm = null;
						if ((output != null) && (output.Count > 0))
						{
							setBrowser(output[0]);
						}
						output = null;
					}
					catch (Exception)
					{
						// Won't work if running as an Applet!
					}
				}
			}
			// Now set the help index URL.  The first one that can be successfully
			// read is assumed to be the correct one.
			if ((index_paths == null) || (index_paths.Count == 0))
			{
				// Just try the index file as is...
				setIndexURL(index_file);
				readIndex(true);
			}
			else
			{ // Loop through and combine the leading path with the file...
				int size = index_paths.Count;
				for (int i = 0; i < size; i++)
				{
					setIndexURL(index_paths[i] + "/" + index_file);
					readIndex(true);
					if (_isHelpAvailable)
					{
						// The index was read successfully so break...
						break;
					}
				}
			}
			// This helps read_index do the right thing if called later...
			_new_initialization = true;
		}
	}

	/// <summary>
	/// Determine whether the help index was successfully read. </summary>
	/// <returns> true if the help index was successfully read, false if not.
	/// This is reset each time a new index file is specified. </returns>
	private static bool isHelpAvailable()
	{
		return _isHelpAvailable;
	}

	/// <summary>
	/// Read the help index file and store the results in the data list.
	/// An internal check is done to see if the read should be consistent with a call
	/// to initialize().
	/// </summary>
	protected internal static void readIndex()
	{
		lock (typeof(URLHelp))
		{
			readIndex(_new_initialization);
		}
	}

	/// <summary>
	/// Read the help index file and store the results in the data vector.
	/// This version is typically called by the initialize() method. </summary>
	/// <param name="dynamic_home"> If true, then the DOCHOME location will be set to the path
	/// of the index file, this allowing configuration at run-time. </param>
	protected internal static void readIndex(bool dynamic_home)
	{
		lock (typeof(URLHelp))
		{
			string routine = "URLHelp.readIndex";
			int dl = 10;
			PropList proplist = new PropList("URLHelp");
        
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "Trying to read help index file \"" + _index_URL + "\"");
			}
        
			// Input Stream to read the _index_URL file
			Stream inStream;
        
			// Initially, assume that help is not available.  If the read
			// succeeds, set to true...
        
			setIsHelpAvailable(false);
        
			// We are trying to read here, so always indicate so...
        
			_hasIndexReadBeenTried = true;
        
			// Instantiate the _data object...
			_data = new List<URLHelpData>(20,10);
        
			// Try to get the input stream to the index.  The index may be a URL
			// or a filename...
        
			try
			{ // Get an input stream...
				inStream = IOUtil.getInputStream(_index_URL);
        
				// exit if inStream is not available	
				if (inStream == null)
				{
					Message.printWarning(2, routine, "Unable to get input stream for \"" + _index_URL + "\".");
					return;
				}
			}
			catch (Exception)
			{
				Message.printWarning(2, routine, "Unable to get input stream for index file \"" + _index_URL + "\".");
				return;
			}
        
			// Now read each line and process as necessary...
        
			StreamReader input = null;
			try
			{
				string line = null;
				string url = null;
				string upline = null;
				IList<string> strings = null;
				int size;
				URLHelpData data = null;
				input = new StreamReader(inStream);
				while (true)
				{
					// Read a line...
					line = input.ReadLine();
					if (string.ReferenceEquals(line, null))
					{
						// End of file...
						break;
					}
					if (Message.isDebugOn)
					{
						Message.printDebug(dl, routine, "Line is: \"" + line + "\"");
					}
					// For some reason, if the URL does not exist, we
					// may still get a connection and the first line will
					// be something like:
					// <HEAD><TITLE>404 Not Found</TITLE></HEAD>
					// Check and see if we find a 404 error and if so
					// treat the entire read as a failure.
					upline = line.ToUpper();
					if (upline.LastIndexOf("404 NOT FOUND", StringComparison.Ordinal) >= 0)
					{
						Message.printWarning(2, routine, "File not found (404 error).");
						return;
					}
					// If the first character is a #, it is a comment line..
					if (line.StartsWith("#", StringComparison.Ordinal))
					{
						continue;
					}
					// Now break into tokens, accepting quoted strings...
					strings = StringUtil.breakStringList(line, " \t", StringUtil.DELIM_ALLOW_STRINGS | StringUtil.DELIM_SKIP_BLANKS);
					size = strings.Count;
					if (Message.isDebugOn)
					{
						for (int i = 0; i < size; i++)
						{
							Message.printDebug(dl, routine, "Token[" + i + "] = \"" + strings[i] + "\"");
						}
					}
					// If the first token has an = sign or the second token
					// starts with =, then we have a property
					// variable and need to save in the property list...
					if (((size > 0) && (upline.LastIndexOf("=", StringComparison.Ordinal) >= 0)) || ((size > 1) && line.StartsWith("=", StringComparison.Ordinal)))
					{
						if (dynamic_home && upline.StartsWith("DOCHOME", StringComparison.Ordinal))
						{
							// Reset the DOCHOME value to the
							// leading path to the index file (which
							// must be an absolute path).  This
							// works with files and URLs that may
							// have forward and back slashes...
							int pos1 = _index_URL.LastIndexOf('/');
							int pos2 = _index_URL.LastIndexOf('\\');
							if (pos1 > pos2)
							{
								pos2 = pos1;
							}
							proplist.set("DOCHOME=" + _index_URL.Substring(0,pos2));
						}
						else
						{ // Add as is...
							proplist.add(line);
						}
						continue;
					}
					// Now try to add the index entry to the list in
					// memory.
					if (size > 1)
					{
						// Have the key and URL so add a new data...
						data = new URLHelpData();
						data.setKey(strings[0]);
						// Expand the URL using the property list...
						url = PropListManager.resolveContentsValue(proplist, strings[1]);
						if (string.ReferenceEquals(url, null))
						{
							continue;
						}
						if (url.Length < 1)
						{
							continue;
						}
						data.setURL(url);
						if (size < 3)
						{
							// Don't have a description, so use the key for the description...
							data.setTopic(strings[0]);
						}
						else
						{
							// Have a description, so use it...
							data.setTopic(strings[2]);
						}
						// Now save the data in the vector...
						_data.Add(data);
					}
				}
			}
			catch (Exception e)
			{
				Message.printWarning(2, routine, "Error reading index file");
				Message.printWarning(2, routine, e);
				return;
			}
			finally
			{
				if (input != null)
				{
					try
					{
						input.Close();
					}
					catch (IOException)
					{
						// Should not happen
					}
				}
			}
        
			// If the index was read correctly then all is fine so help is
			// available...
        
			if (Message.isDebugOn)
			{
				Message.printDebug(2, routine, "Read " + _data.Count + " help topics");
			}
        
			setIsHelpAvailable(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(2, routine, "Done reading help index \"" + _index_URL + "\"");
			}
        
			if (Message.isDebugOn)
			{
				URLHelpData data = null;
				for (int i = 0; i < _data.Count; i++)
				{
					data = (URLHelpData)_data[i];
					Message.printDebug(dl, routine, data.ToString());
				}
				data = null;
			}
        
			proplist = null;
			try
			{
				inStream.Close();
			}
			catch (Exception)
			{
			}
			inStream = null;
		}
	}

	/// <summary>
	/// Set the path to the browser to execute for on-line help when running
	/// stand-alone.  This can be an executable if the executable is in the path.
	/// Otherwise, it needs to be the full path. </summary>
	/// <param name="browser"> The browser to use when running stand-alone. </param>
	public static void setBrowser(string browser)
	{
		lock (typeof(URLHelp))
		{
			if (!string.ReferenceEquals(browser, null))
			{
				_browser = browser;
				//if ( Message.isDebugOn ) {
					//Message.printDebug( 10, "URLHelp.setBrowser()", 
					Message.printStatus(1, "URLHelp.setBrowser()", "Setting browser: \"" + _browser + "\"");
				//}
			}
		}
	}

	/// <summary>
	/// Indicate whether help topics have been read.
	/// Set to true if the help index file is read successfully, false if not.
	/// Called by <i>readIndex()</i>. </summary>
	/// <param name="state"> Indicates whether the on-line help topics have been read. </param>
	private static void setIsHelpAvailable(bool state)
	{
		_isHelpAvailable = state;
	}

	/// <summary>
	/// Set a list of String to be displayed instead of trying to display a help
	/// topic.  For example, call this with a general message directing users to the PDF documentation. </summary>
	/// <param name="override"> A Vector of String to be displayed in a dialog as an override of the normal help system. </param>
	public static void setHelpOverride(IList<string> @override)
	{
		__help_override_List = @override;
	}

	/// <summary>
	/// Set the URL that points to the index file.  This function sets the index file
	/// path for key and URL addresses for an application.
	/// The index is not read until the first help request is made. </summary>
	/// <param name="indexURL"> The URL for the help index file (local or URL). </param>
	public static void setIndexURL(string indexURL)
	{
		lock (typeof(URLHelp))
		{
			if (!string.ReferenceEquals(indexURL, null))
			{
				_index_URL = indexURL;
				// Reset the static data...
				_isHelpAvailable = false;
				_hasIndexReadBeenTried = false;
			}
		}
	}

	/// <summary>
	/// Display the on-line help for the given index in the data vector (0 is first
	/// entry).  If running stand-alone, a new browser will be spawned, which requires
	/// that the browser path is set.  If an applet, a clone of the original browser
	/// window will be started and will display the page.  This routine should be called
	/// by all other show* functions like showHelpForKey, showHelpForTopic, etc. </summary>
	/// <param name="index"> Index number for help topic. </param>
	public static void showHelpForIndex(int index)
	{
		string routine = "URLHelp.showHelpForIndex";
		int dl = 10;

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Displaying help for index " + index);
		}

		// Check to see if we need to try to read the index file...

		if (!_hasIndexReadBeenTried)
		{
			// Have not tried to read the index yet so do so...
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "Have not tried reading index yet, so do so...");
			}
			readIndex(_new_initialization);
		}

			if (!isHelpAvailable())
			{
			// Help is not available.
					Message.printWarning(1, routine, "On-line help is not available (no index was found).");
					return;
			}

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Help is available so try to display...");
		}

		if (index < 0)
		{
					Message.printWarning(2, routine, "Requested index location is < 0.");
					return;
		}

		URLHelpData data = _data[index];
			string URLAddress = data.getURL();

		// Return if the requested key is not found...

		if (string.ReferenceEquals(URLAddress, null))
		{
			Message.printWarning(2, routine, "URL for index [" + index + "] not found.");
			return;
		}

		// launch the browser with the specified URL

		showURL(URLAddress);
	}

	/// <summary>
	/// Display the on-line help for the given key. </summary>
	/// <param name="key"> Key for help topic. </param>
	public static void showHelpForKey(string key)
	{
		if (__help_override_List != null)
		{
			// The help override is in place so use it...  For now just
			// Use a standard warning dialog...
			StringBuilder b = new StringBuilder();
			int size = __help_override_List.Count;
			for (int i = 0; i < size; i++)
			{
				b.Append((string)__help_override_List[i]);
				b.Append("\n");
			}
			Message.printWarning(1, "", b.ToString());
			return;
		}
		if (!_hasIndexReadBeenTried)
		{
			// Have not tried to read the index yet so do so...
			if (Message.isDebugOn)
			{
				Message.printDebug(10, "URLHelp.showHelpForKey", "Have not tried reading index yet, so do so...");
			}
			readIndex(_new_initialization);
		}
		showHelpForIndex(findIndexForKey(key));
	}

	/// <summary>
	/// Display the on-line help for the given topic. </summary>
	/// <param name="topic"> Help topic to display. </param>
	public static void showHelpForTopic(string topic)
	{
		if (!_hasIndexReadBeenTried)
		{
			// Have not tried to read the index yet so do so...
			if (Message.isDebugOn)
			{
				Message.printDebug(10, "URLHelp.showHelpForTopic", "Have not tried reading index yet, so do so...");
			}
			readIndex(_new_initialization);
		}
		showHelpForIndex(findIndexForTopic(topic));
	}

	/// <summary>
	/// Display the requested URLAdrress in the selected browser.
	/// If running stand-alone, a new browser will be spawned, which requires that the
	/// browser path is set.  If an applet, a clone of the original browser
	/// window will be started and will display the page. </summary>
	/// <param name="URLAddress"> Specify the full URL address to the file. </param>
	/// @deprecated Use showURL (since that is a more specific name for the method). 
	public static void showTopic(string URLAddress)
	{
		showURL(URLAddress);
	}

	/// <summary>
	/// Display the requested URLAdrress in the selected browser.
	/// If running stand-alone, a new browser will be spawned, which requires that the
	/// browser path is set.  If an applet, a clone of the original browser
	/// window will be started and will display the page. </summary>
	/// <param name="URLAddress"> Specify the full URL address to the file. </param>
	public static void showURL(string URLAddress)
	{
		string routine = "URLHelp.showURL";

		// Send on-line help to the parent window if running an applet...

		if (IOUtil.isApplet())
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(10, routine, "Attempting to show help page \"" + URLAddress + "\" to a new browser.");
			}
			try
			{
				URL url = new URL(IOUtil.getDocumentBase(),URLAddress);
				AppletContext ac = IOUtil.getAppletContext();
				// Display the URL in a new window.  Allthough the
				// documentation says that the document can be displayed
				// in the originating window, it seems that if this is
				// done, then the applet loses contact with the browser.
				if (ac != null)
				{
					ac.showDocument(url, "_blank");
				}
				url = null;
				ac = null;
			}
			catch (Exception)
			{
				Message.printWarning(2, routine, "Malformed URL: \"" + URLAddress + "\"");
			}
		}
		else
		{ // Open on-line help using the _browser path, when running
			// stand alone.  This will start a new browser for each on-line
			// document requested.  Default to the Internet Explorer path
			// if no browser is specified.
			string command; // entire path (i.e., browser executable +
						// help file)	

			if (Message.isDebugOn)
			{
				Message.printDebug(10, routine, "Displaying help using browser: \"" + _browser + "\"");
			}

			// Construct the full system command using the _browser path
			// and the URLAddress.

			if (_browser.Length <= 0)
			{
				Message.printWarning(1, routine, "The browser has " + "not been specified.  Cannot display help.");
				return;
			}
			string[] command_array = new string[2];
			command_array[0] = _browser;
			command_array[1] = URLAddress;
			command = _browser + " " + URLAddress;

			Message.printStatus(1, routine, "Launching browser with command: \"" + command + "\"");

			// Since using a thread, there might be some way to communicate
			// with the thread later...

			try
			{
				ProcessManager pm = new ProcessManager(command_array);
				Thread thread = new Thread(pm);
				// This will cause the browser to run until it is
				// exited, at which time the thread will close down...
				thread.Start();
			}
			catch (Exception e)
			{
				Message.printWarning(1, routine, "Error executing: \"" + command + "\"");
				Message.printWarning(2, routine, e);
				return;
			}
			if (Message.isDebugOn)
			{
				Message.printDebug(2, routine, "Successfuly spawned browser.");
			}
			command = null;
		}
	}

	} // End of URLHelp

}