using System.Collections.Generic;
using System.Text;
using System.IO;

// IOUtil - this class provides static functions for file input/output

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

namespace RTi.Util.IO
{




	using Math;
	using System;




	using SimpleFileFilter = RTi.Util.GUI.SimpleFileFilter;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using TimeUtil = RTi.Util.Time.TimeUtil;

	/// <summary>
	/// This class provides static functions for file input/output and also provides
	/// global functionality that may be useful in any program.  The class provides
	/// useful functionality in addition to the Java System, IO, and security classes.
	/// A PropListManager is used to manage a global, un-named PropList in conjunction with other PropLists.
	/// To make the best use of this class, initialize from the main() or init() functions, as follows:
	/// <para>
	/// 
	/// <pre>
	/// // Called if an applet...
	/// public static final String PROGNAME = "myprog";
	/// public static final String PROGVER = "1.2 (12 Mar 1998)";
	/// 
	/// public void init ()
	/// {	IOUtil.setApplet ( this );
	///    IOUtil.setProgramData ( PROGNAME, PROGVER, null );
	/// }
	/// 
	/// // Called if stand-alone...
	/// public static void main ( String argv )
	/// {	// The default is not an applet.
	///    IOUtil.setProgramData ( PROGNAME, PROGVER, argv );
	/// }
	/// </pre>
	/// </para>
	/// </summary>
	public abstract class IOUtil
	{

	// Global data...

	// TODO SAM 2009-05-06 Evaluate whether needed
	/// <summary>
	/// Flags use to indicate the vendor
	/// </summary>
	public const int SUN = 1;
	public const int MICROSOFT = 2;
	public const int UNKNOWN = 3;

	/// <summary>
	/// String to use to indicate a file header revision line. </summary>
	/// <seealso cref= #getFileHeader </seealso>
	protected internal const string HEADER_REVISION_STRING = "HeaderRevision";
	/// <summary>
	/// String used to indicate comments in files (unless otherwise indicated).
	/// </summary>
	protected internal const string UNIVERSAL_COMMENT_STRING = "#";

	/// <summary>
	/// Command-line arguments, guaranteed to be non-null but may be empty.
	/// </summary>
	private static string[] _argv = new string[0];
	/// <summary>
	/// Applet, null if not an applet.
	/// </summary>
	private static Applet _applet = null;
	/// <summary>
	/// Applet context.  Call setAppletContext() from init() of an application that uses this class.
	/// </summary>
	private static AppletContext _applet_context = null;
	/// <summary>
	/// Document base for the applet.
	/// </summary>
	private static URL _document_base = null;
	/// <summary>
	/// Program command file.
	/// TODO SAM (2009-05-06) Evaluate phasing out since command file is managed with processor, not program.
	/// </summary>
	private static string _command_file = "";
	/// <summary>
	/// Program command list.
	/// TODO SAM (2009-05-06) Evaluate phasing out since command file is managed with processor, not program.
	/// </summary>
	private static IList<string> _command_list = null;
	/// <summary>
	/// Host (computer) running the program.
	/// </summary>
	private static string _host = "";
	/// <summary>
	/// Program name, as it should appear in title bars, Help About, etc.
	/// </summary>
	private static string _progname = "";
	/// <summary>
	/// Program version, typically "XX.XX.XX" or "XX.XX.XX beta".
	/// </summary>
	private static string _progver = "";
	/// <summary>
	/// Program user.
	/// </summary>
	private static string _user = "";
	/// <summary>
	/// Indicates whether a test run (not used much anymore) - can be used for experimental features that are buried
	/// in the code base.
	/// </summary>
	private static bool _testing = false;
	/// <summary>
	/// Program working directory, which is virtual and used to create absolute paths to files.  This is needed
	/// because the application cannot change the current working directory due to security checks.
	/// </summary>
	private static string _working_dir = "";
	/// <summary>
	/// Indicates whether global data are initialized.
	/// </summary>
	private static bool _initialized = false;
	/// <summary>
	/// Indicate whether the program is running as an applet.
	/// </summary>
	private static bool _is_applet = false;
	/// <summary>
	/// Indicates whether the program is running in batch (non-interactive) or interactive GUI/shell.
	/// </summary>
	private static bool _is_batch = false;
	/// <summary>
	/// A property list manager that can be used globally in the application.
	/// </summary>
	private static PropListManager _prop_list_manager = null;
	/// <summary>
	/// TODO SAM 2009-05-06 Seems to be redundant with _is_applet.
	/// </summary>
	private static bool __runningApplet = false;
	/// <summary>
	/// Home directory for the application, typically the installation location (e.g., C:\Program Files\Company\AppName).
	/// </summary>
	private static string __homeDir = null;

	/// <summary>
	/// Add a PropList to the list managed by the IOUtil PropListManager. </summary>
	/// <param name="proplist"> PropList to add to the list managed by the PropListManager. </param>
	/// <param name="replace_if_match"> If the name of the PropList matches one that is already
	/// in the list, replace it (true), or add the new list additionally (false). </param>
	public static void addPropList(PropList proplist, bool replace_if_match)
	{
		if (!_initialized)
		{
			initialize();
		}
		_prop_list_manager.addList(proplist, replace_if_match);
	}

	/// <summary>
	/// Adjust an existing path.  This can be used, for example to navigate up an absolute path by a relative change.
	/// The resulting path is returned.  Rules for adjustment are as follows:
	/// <ol>
	/// <li>	If the adjustment is an absolute path, the returned path is the same as the adjustment.
	/// </li>
	/// <li>	If the adjustment is a relative path (e.g., "..", "../something",
	/// "something", "something/something2"), the initial path is adjusted by
	/// removing redundant path information if possible.
	/// </li>
	/// <li>	If the adjustment is a relative path that cannot be applied, an exception is thrown.
	/// </li>
	/// <li>	The returned path will not have the file separator unless the path is the root directory.
	/// </li>
	/// </ol>
	/// No check for path existence is made. </summary>
	/// <returns> the original path adjusted by the adjustment, with no path separator at the end. </returns>
	/// <param name="initialPath"> Original path to adjust. </param>
	/// <param name="adjustment"> Adjustment to the path to apply (e.g., "..", or a file/folder name). </param>
	/// <exception cref="Exception"> if the path cannot be adjusted. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static String adjustPath(String initialPath, String adjustment) throws Exception
	public static string adjustPath(string initialPath, string adjustment)
	{
		File a = new File(adjustment);
		// If the adjustment is blank, return the initial path.  Do not trim
		// the adjustment because it is possible that a filename has only spaces.
		if ((string.ReferenceEquals(adjustment, null)) || (adjustment.Length == 0))
		{
			return initialPath;
		}
		if (a.isAbsolute())
		{
			// Adjustment is absolute so make the adjustment...
			return adjustment;
		}
		// The adjustment is relative.  First make sure the initial path ends in a file separator...
		StringBuilder buffer = new StringBuilder(initialPath);
		char filesep = File.separator.charAt(0);
		if (initialPath[initialPath.Length - 1] != filesep)
		{
			buffer.Append(filesep);
		}
		// Loop through the adjustment.  For every ".." that is encountered, remove one directory from "buffer"...
		int length = adjustment.Length;
		string upOne = "..";
		for (int i = 0; i < length; i++)
		{
			if (adjustment.IndexOf(upOne,i, StringComparison.Ordinal) == i)
			{
				// The next part of the string has "..".  Move up one level in the initial string.
				// The buffer will have a separator at the end so need to skip over it at the start...
				for (int j = buffer.Length - 2; j >= 0; j--)
				{
					if (buffer[j] == filesep)
					{
						// Found the previous separator...
						buffer.Length = j + 1;
						break;
					}
				}
				// Increment in the adjustment...
				i += 2; // Loop increment will go past the separator
			}
			else if (adjustment.IndexOf("..",i, StringComparison.Ordinal) == i)
			{
				// Need to go up one directory
			}
			else if (adjustment[i] == '.')
			{
				// If the next character is a separator (or at the end of the string), ignore this part of the
				// path (since it references the current directory...
				if (i == (length - 1))
				{
					// Done processing...
					break;
				}
				else if (adjustment[i + 1] == filesep)
				{
					// Skip...
					++i;
					continue;
				}
				else
				{
					// A normal "." for a file extension so add it...
					buffer.Append('.');
				}
			}
			else
			{
				// Add the characters to the adjusted path...
				buffer.Append(adjustment[i]);
			}
		}
		// Remove the trailing separator, but only if not the root directory..
		if ((buffer[buffer.Length - 1] == filesep) && !buffer.Equals("" + filesep))
		{
			// Remove the trailing file separator...
			buffer.Length = buffer.Length - 1;
		}
		return buffer.ToString();
	}

	/// <summary>
	/// Tries to manually load a class into memory in order to see if it is available.  
	/// Normally, class-loading is done by the Java Virtual Machine when a class is 
	/// first used in code, but this method can be used to load a class at any time, 
	/// and more importantly, to check whether a class is available to the virtual machine.<para>
	/// Classes may be unavailable because of a difference in program versions, or 
	/// because they were intentionally left out of a jar file in order to limit
	/// </para>
	/// the functionality of an application.<para>
	/// The virtual machine will look through the entire class path when it tries to load the given class.
	/// </para>
	/// </para>
	/// </summary>
	/// <param name="className"> the fully-qualified class name (including package) of the class to try loading.  Examples:<para>
	/// - RTi.Util.GUI.JWorksheet<para>
	/// </para>
	/// - java.util.Vector<para>
	/// - DWR.DMI.HydroBaseDMI.HydroBase_StructureView
	/// </para>
	/// </param>
	/// <returns> true if the class could be loaded, false if not. </returns>
	public static bool classCanBeLoaded(string className)
	{
		try
		{
			Type.GetType(className);
		}
		catch (ClassNotFoundException)
		{
			return false;
		}
		return true;
	}

	/// <summary>
	/// Copies a file from one file to another. </summary>
	/// <param name="source"> the source file to copy. </param>
	/// <param name="dest"> the destination file to copy the first file to. </param>
	/// <exception cref="IOException"> if there is an error copying the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void copyFile(java.io.File source, java.io.File dest) throws java.io.IOException
	public static void copyFile(File source, File dest)
	{
	/*
		FileChannel in = new FileInputStream(source).getChannel();
		FileChannel out = new FileOutputStream(dest).getChannel();
	
		long size = in.size();
		MappedByteBuffer buf = in.map(FileChannel.MapMode.READ_ONLY, 0, size);
		out.write(buf);
	
		in.close();
		out.close();
	*/
		// FIXME JTS (2004-10-11) the above is supposed to be faster, but MT was getting some 
		// bizarre errors that seem related to the use of Channels.  Will
		// try the following, but I believe it's going to be slowed.
		FileStream fis = new FileStream(source, FileMode.Open, FileAccess.Read);
		FileStream fos = new FileStream(dest, FileMode.Create, FileAccess.Write);
		sbyte[] buf = new sbyte[1024];
		int i = 0;
		while ((i = fis.Read(buf, 0, buf.Length)) != -1)
		{
			fos.Write(buf, 0, i);
		}
		fis.Close();
		fos.Close();
	}

	/// <summary>
	/// Copies the specified String to the system clipboard. </summary>
	/// <param name="string"> the String to copy to the clipboard. </param>
	public static void copyToClipboard(string @string)
	{
		StringSelection stsel = new StringSelection(@string);
		Clipboard clipboard = Toolkit.getDefaultToolkit().getSystemClipboard();
		clipboard.setContents(stsel, stsel);
	}

	/// <summary>
	/// Enforce a file extension.  For example, if a file chooser is used but the file
	/// extension is not added by the chooser.  For example, if the file name is
	/// "file" and the extension is "zzz", then the returned value will be "file.zzz".
	/// If the file is "file.zzz", the returned value will be the same (no change).
	/// There is currently no sophistication to handle input file names with multiple
	/// extensions that are different from the requested extension. </summary>
	/// <param name="filename"> The file name on which to enforce the extension. </param>
	/// <param name="extension"> The file extension to enforce, without the leading ".". </param>
	public static string enforceFileExtension(string filename, string extension)
	{
		if (StringUtil.endsWithIgnoreCase(filename, "." + extension))
		{
			return filename;
		}
		else
		{
			return filename + "." + extension;
		}
	}

	/// <summary>
	/// Expand a configuration string property value using environment and Java runtime environment variables.
	/// If the string is prefixed with "Env:" then the string will be replaced with the environment variable
	/// of the matching name.  If the string is prefixed with "SysProp:" then the string will be replaced with
	/// the JRE runtime system property of the same name.  Comparisons are case-sensitive and if a match
	/// is not found the original string will be returned. </summary>
	/// <param name="propName"> name of the property, used for messaging </param>
	/// <param name="propValue"> the string property value to expand </param>
	/// <returns> expanded property value </returns>
	public static string expandPropertyForEnvironment(string propName, string propValue)
	{
		if (string.ReferenceEquals(propValue, null))
		{
			return null;
		}
		int pos = StringUtil.indexOfIgnoreCase(propValue,"Env:",0);
		if ((pos == 0) && (propValue.Length > 4))
		{
			string env = Environment.GetEnvironmentVariable(propValue.Substring(4));
			if (!string.ReferenceEquals(env, null))
			{
				return env;
			}
			else
			{
				return propValue;
			}
		}
		pos = StringUtil.indexOfIgnoreCase(propValue,"SysProp:",0);
		if ((pos == 0) && (propValue.Length > 8))
		{
			string sys = System.getProperty(propValue.Substring(8));
			if (!string.ReferenceEquals(sys, null))
			{
				return sys;
			}
			else
			{
				return propValue;
			}
		}
		if (propValue.Equals("Prompt", StringComparison.OrdinalIgnoreCase))
		{
			// Prompt for the value
			Console.Write("Enter value for \"" + propName + "\": ");
			StreamReader @in = new StreamReader(System.in);
			try
			{
				propValue = @in.ReadLine().Trim();
			}
			catch (IOException)
			{
				propValue = "";
			}
		}
		// No special case so return the original value
		return propValue;
	}

	/// <summary>
	/// Determine if a file/directory exists. </summary>
	/// <returns> true if the file/directory exists, false if not. </returns>
	/// <param name="filename"> String path to the file/directory to check. </param>
	public static bool fileExists(string filename)
	{
		if (string.ReferenceEquals(filename, null))
		{
			return false;
		}
		File file = new File(filename);
		bool exists = file.exists();
		file = null;
		return exists;
	}

	/// <summary>
	/// Determine if a file/directory is readable. </summary>
	/// <returns> true if the file/directory is readable, false if not. </returns>
	/// <param name="filename"> String path to the file/directory to check. </param>
	public static bool fileReadable(string filename)
	{
		if (string.ReferenceEquals(filename, null))
		{
			return false;
		}
		Stream st = null;
		try
		{
			st = IOUtil.getInputStream(filename);
		}
		catch (Exception)
		{
			st = null;
			return false;
		}
		if (st != null)
		{
			try
			{
				st.Close();
			}
			catch (Exception)
			{
			}
			st = null;
			return true;
		}
		return false;
		// This only works with files.  The above works with URLs...
		//File file = new File(filename);
		//boolean canread = file.canRead();
		//file = null;
		//return canread;
	}

	/// <summary>
	/// Read in a file and store it in a string list (list of String). </summary>
	/// <param name="filename">	File to read and convert to string list. </param>
	/// <returns> the file as a string list. </returns>
	/// <exception cref="IOException"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<String> fileToStringList(String filename) throws java.io.IOException
	public static IList<string> fileToStringList(string filename)
	{
		IList<string> list = null;
		string message, routine = "IOUtil.fileToStringList", tempstr;

		if (string.ReferenceEquals(filename, null))
		{
			message = "Filename is NULL";
			Message.printWarning(10, routine, message);
			throw new IOException(message);
		}
		if (filename.Length == 0)
		{
			message = "Filename is empty";
			Message.printWarning(10, routine, message);
			throw new IOException(message);
		}

		// Open the file...

		if (Message.isDebugOn)
		{
			Message.printDebug(30, routine, "Breaking file \"" + filename + "\" into string list");
		}
		StreamReader fp = null;
		try
		{
			fp = new StreamReader(IOUtil.getInputStream(filename));
		}
		catch (Exception e)
		{
			message = "Unable to read file \"" + filename + "\" (" + e + ").";
			Message.printWarning(3, routine, message);
			throw new IOException(message);
		}

		list = new List<string>(50);
		while (true)
		{
			tempstr = fp.ReadLine();
			if (string.ReferenceEquals(tempstr, null))
			{
				break;
			}
			tempstr = StringUtil.removeNewline(tempstr);
			list.Add(tempstr);
		}
		fp.Close();
		fp = null;
		tempstr = null;
		message = null;
		routine = null;
		return list;
	}

	/// <summary>
	/// Determine if a file is writeable.  False is returned if the file does not exist. </summary>
	/// <returns> true if the file is writeable, false if not.  The file must exist. </returns>
	/// <param name="filename"> String path to the file to check. </param>
	public static bool fileWriteable(string filename)
	{
		if (string.ReferenceEquals(filename, null))
		{
			return false;
		}
		File file = new File(filename);
		bool canwrite = file.canWrite();
		file = null;
		return canwrite;
	}

	/// <summary>
	/// Format a header for a file, useful to understand the file creation.  The header looks like the following:
	/// <para>
	/// <pre>
	/// # File generated by
	/// # program:   demandts 2.7 (25 Jun 1995)
	/// # user:      sam
	/// # date:      Mon Jun 26 14:49:18 MDT 1995
	/// # host:      white
	/// # directory: /crdss/dmiutils/demandts/data
	/// # command:   ../src/demandts -d1 -w1,10 -demands -istatemod 
	/// #            /crdss/statemod/data/white/white.ddh -icu 
	/// #            /crdss/statemod/data/white/white.ddc -sstatemod 
	/// #            /crdss/statemod/data/white/white.dds -eff12 
	/// </pre>
	/// </para>
	/// <para>
	/// </para>
	/// </summary>
	/// <param name="commentLinePrefix"> The string to use for the start of comment lines (e.g., "#").  Use blank if the
	/// prefix character will be added by calling code. </param>
	/// <param name="maxWidth"> The maximum length of a line of output (if whitespace is
	/// embedded in the header information, lines will be broken appropriately to fit
	/// within the specified length. </param>
	/// <param name="isXML"> Indicates whether the comments are being formatted for an XML file.
	/// XML files must be handled specifically because some characters that may be printed
	/// to the header may not be handled by the XML parser.  The opening and closing
	/// XML tags must be added before and after calling this method. </param>
	/// <returns> the list of formatted header strings, guaranteed to be non-null </returns>
	public static IList<string> formatCreatorHeader(string commentLinePrefix, int maxWidth, bool isXml)
	{
		int commentLen, i, leftBorder = 12, len;

		if (!_initialized)
		{
			// Need to initialize the class static data
			initialize();
		}

		string now = TimeUtil.getSystemTimeString("");

		// Make sure that a valid comment string is used...

		if (string.ReferenceEquals(commentLinePrefix, null))
		{
			commentLinePrefix = "";
		}
		string commentLinePrefix2 = "";
		if (!commentLinePrefix.Equals(""))
		{
			// Add a space to the end of the prefix so that comments are not smashed right up against
			// the line prefix - this helps with readability
			commentLinePrefix2 = commentLinePrefix + " ";
		}
		commentLen = commentLinePrefix2.Length;

		// Format the comment string for the command line printout...

		StringBuilder commentSpace0 = new StringBuilder(commentLinePrefix2);
		for (i = 0; i < leftBorder; i++)
		{
			commentSpace0.Append(" ");
		}
		string commentSpace = commentSpace0.ToString();
		commentSpace0 = null;

		IList<string> comments = new List<string>();
		comments.Add(commentLinePrefix2 + "File generated by...");
		comments.Add(commentLinePrefix2 + "program:      " + _progname + " " + _progver);
		comments.Add(commentLinePrefix2 + "user:         " + _user);
		comments.Add(commentLinePrefix2 + "date:         " + now);
		comments.Add(commentLinePrefix2 + "host:         " + _host);
		comments.Add(commentLinePrefix2 + "directory:    " + _working_dir);
		comments.Add(commentLinePrefix2 + "command line: " + _progname);
		int column0 = commentLen + leftBorder + _progname.Length + 1;
		int column = column0; // Column position, starting at 1
		StringBuilder b = new StringBuilder(commentLinePrefix2);
		if (_argv != null)
		{
			for (i = 0; i < _argv.Length; i++)
			{
				len = _argv[i].Length;
				// Need 1 to account for blank between arguments...
				if ((column + 1 + len) > maxWidth)
				{
					// Put the argument on a new line...
					comments.Add(b.ToString());
					b.Length = 0;
					b.Append(commentLinePrefix2);
					b.Append(commentSpace + _argv[i]);
					column = column0 + len;
				}
				else
				{
					// Put the argument on the same line...
					b.Append(" " + _argv[i]);
					column += (len + 1);
				}
			}
		}
		comments.Add(b.ToString());
		if (_command_list != null)
		{
			// Print the command list contents...
			if (isXml)
			{
				comments.Add(commentLinePrefix2);
			}
			else
			{
				comments.Add(commentLinePrefix2 + "-----------------------------------------------------------------------");
			}
			if (fileReadable(_command_file))
			{
				comments.Add(commentLinePrefix2 + "Last command file: \"" + _command_file + "\"");
			}
			comments.Add(commentLinePrefix2);
			comments.Add(commentLinePrefix2 + "Commands used to generate output:");
			comments.Add(commentLinePrefix2);
			int size = _command_list.Count;
			for (i = 0; i < size; i++)
			{
				comments.Add(commentLinePrefix2 + _command_list[i]);
			}
		}
		else if (fileReadable(_command_file))
		{
			// Print the command file contents...
			if (isXml)
			{
				comments.Add(commentLinePrefix2);
			}
			else
			{
				comments.Add(commentLinePrefix2 + "-----------------------------------------------------------------------");
			}
			comments.Add(commentLinePrefix2 + "Command file \"" + _command_file + "\":");
			comments.Add(commentLinePrefix2);
			bool error = false;
			StreamReader cfp = null;
			StreamReader file = null;
			try
			{
				file = new StreamReader(_command_file);
				cfp = new StreamReader(file);
			}
			catch (Exception)
			{
				error = true;
			}
			if (!error)
			{
				string @string;
				while (true)
				{
					try
					{
						@string = cfp.ReadLine();
						if (string.ReferenceEquals(@string, null))
						{
							break;
						}
					}
					catch (Exception)
					{
						// End of file.
						break;
					}
					comments.Add(commentLinePrefix2 + " " + @string);
				}
			}
		}
		return comments;
	}

	/// <summary>
	/// Get the Applet. </summary>
	/// <returns> The Applet instance set with setApplet(). </returns>
	/// <seealso cref= #setApplet </seealso>
	public static Applet getApplet()
	{
		return _applet;
	}

	/// <summary>
	/// Get the AppletContext. </summary>
	/// <returns> The AppletContext instance set with setAppletContext. </returns>
	/// <seealso cref= #setAppletContext </seealso>
	public static AppletContext getAppletContext()
	{
		return _applet_context;
	}

	/// <summary>
	/// Returns the application home directory.  This is the directory from which log files,
	/// configuration files, documentation etc, can be located.  Normally it is the installation home
	/// (e.g., C:\Program Files\RTi\TSTool-Version). </summary>
	/// <returns> the application home directory. </returns>
	public static string getApplicationHomeDir()
	{
		return __homeDir;
	}

	/// <summary>
	/// Get the document base. </summary>
	/// <returns> The DocumentBase instance set when setApplet() is called. </returns>
	/// <seealso cref= #setApplet </seealso>
	public static URL getDocumentBase()
	{
		return _document_base;
	}

	/// <summary>
	/// Return the drive letter for a path. </summary>
	/// <returns> the drive letter for a path (e.g., "C:") or return an empty string if
	/// no drive is found it the start of the path. </returns>
	/// @deprecated come back to this to resolve UNC issues.  TODO JTS - 2006-02-16 
	public static string getDrive(string path)
	{
		if (isUNIXMachine())
		{
			return "";
		}
		// Assume windows...
		if ((path.Length >= 2) && (((path[0] >= 'a') && (path[0] <= 'z')) || ((path[0] >= 'A') && (path[0] <= 'Z'))) && (path[1] == ':'))
		{
			return path.Substring(0,2);
		}
		else
		{
			return "";
		}
	}

	/// <summary>
	/// Determine the file extension. </summary>
	/// <returns> the part of a file name after the last "." character, or null if no ".". </returns>
	public static string getFileExtension(string file)
	{
		IList<string> v = StringUtil.breakStringList(file, ".", 0);
		if ((v == null) || (v.Count == 0))
		{
			return null;
		}
		return v[v.Count - 1];
	}

	// NEED TO CLEAN UP JAVADOC AND OPTIMIZE FOR GC...
	/* ----------------------------------------------------------------------------
	** HMGetFileHeader -	get the header for a file, assuming that the header is
	**			indicated by comment or other special characters
	** ----------------------------------------------------------------------------
	** Copyright:	See the COPYRIGHT file.
	** ----------------------------------------------------------------------------
	** Notes:	(1)	This routine opens and closes the file and removes the
	**			newlines from the strings.
	**		(2)	"comments" contains a list of strings that, if at the
	**			beginning of a line, indicate that the line is a
	**			comment.  For example { "#", "REM", "$" }.  The
	**			"ignore_comments" string list indicates comments that
	**			do match "comments" but are comments that are to be
	**			ignored.  For example { "#>", "#ignore" }.
	**		(3)	It is assumed that a special comment can be saved that
	**			has the format:
	**
	**				#HeaderRevision 1
	**
	**			where the number indicates the revision on the file
	**			header.  The smallest such number is returned in
	**			"header_first" and the largest in "header_last".
	** ----------------------------------------------------------------------------
	** History:
	**
	** 14 Feb 96	Steven A. Malers, RTi	Created routine.
	** 04 Mar 96	SAM, RTi		Change so that the comment strings are
	**					a string list and add the ignore list.
	**					Also add the header revisions.
	** 05 Sep 96	SAM, RTi		Split out of HMUtil.c file.
	** 07 Oct 96	SAM, RTi		Add <string.h> to prototype functions.
	** ----------------------------------------------------------------------------
	** Variable	I/O	Description
	**
	** comments	I	Strings at start of line that indicate a header.
	** dl		L	Debug level for this routine.
	** filename	I	Name of file to pull lines from.
	** flags	I	Flags.  Currently unused but may be used in the future
	**			to ignore some of the special processing.
	** fp		L	Pointer to open file.
	** header_first	O	First header revision encountered in comments (smallest
	**			number).
	** header_last	O	Last header revision encountered in comments (largest
	**			number).
	** header_revision L	Header revision read from file.
	** i		L	Loop counter for strings.
	** ierror	O	Error code.
	** ignore_comments I	Strings at start of line that indicte a header but which
	**			should be ignored.
	** iscomment	L	Is the current line a comment?
	** isignore	L	Is the current line a comment that is to be ignored?
	** len		L	Length of string.
	** list		O	String list for header lines.
	** nlines	O	Number of lines in header.
	** revlen	L	Length of the header revision string.
	** revpt	L	Pointer to a start of potential header revision comment.
	** routine	L	Name of this routine.
	** string	L	Line read from file.
	** ----------------------------------------------------------------------------
	*/

	/// @deprecated Use version that operates on lists. 
	public static FileHeader getFileHeader(string filename, string[] commentIndicators, string[] ignoredCommentIndicators, int flags)
	{
		IList<string> commentIndicatorList = null;
		if (commentIndicators != null)
		{
			commentIndicatorList = StringUtil.toList(commentIndicators);
		}
		IList<string> ignoredCommentIndicatorList = null;
		if (ignoredCommentIndicators != null)
		{
			ignoredCommentIndicatorList = StringUtil.toList(ignoredCommentIndicators);
		}
		return getFileHeader(filename, commentIndicatorList, ignoredCommentIndicatorList, flags);
	}

	/// <returns> The FileHeader associated with a file.  This information is used by processFileHeaders
	/// when tracking revisions to files.  Comment strings must start at the beginning of the line. </returns>
	/// <param name="fileName"> The name of the file to process. </param>
	/// <param name="commentIndicators"> A list of strings indicating valid comment indicators.  For example: {"#", "*"}. </param>
	/// <param name="ignoredCommentIndicators"> A list of strings indicating valid comments which
	/// should be ignored and which take precedence over "comments".  For example, "#>"
	/// might be used for comments that are written to a file each time it is revised
	/// but those comments are to be ignored each time the header is read. </param>
	/// <param name="flags"> Currently unused. </param>
	/// <seealso cref= #processFileHeaders </seealso>
	public static FileHeader getFileHeader(string fileName, IList<string> commentIndicators, IList<string> ignoredCommentIndicators, int flags)
	{
		string routine = "IOUtil.getFileHeader", @string;
		int dl = 30, header_first = -1, header_last = -1, header_revision, i, len, revlen;
		bool iscomment, isignore;

		// Need to handle error.

		revlen = HEADER_REVISION_STRING.Length;
		if (string.ReferenceEquals(fileName, null))
		{
			Message.printWarning(10, routine, "NULL file name pointer");
			return null;
		}
		if (fileName.Length == 0)
		{
			Message.printWarning(10, routine, "Empty file name");
			return null;
		}
		if (!fileReadable(fileName))
		{
			Message.printWarning(10, routine, "File \"" + fileName + "\" is not readable");
			return null;
		}
		if (commentIndicators == null)
		{
			Message.printWarning(10, routine, "Empty comment strings list");
			return null;
		}

		// Open the file...

		StreamReader fp = null;
		try
		{
			fp = new StreamReader(fileName);
		}
		catch (Exception)
		{
			Message.printWarning(10, routine, "Error opening file \"" + fileName + "\" for reading.");
			return null;
		}

		// Now read lines until we get to the end of the file or hit a
		// non-header line (OK to skip "ignore_comments")...

		FileHeader header = new FileHeader();

		int length_comments = commentIndicators.Count;
		int linecount = 0;
		while (true)
		{
			++linecount;
			try
			{
				@string = fp.ReadLine();
				if (string.ReferenceEquals(@string, null))
				{
					break;
				}
			}
			catch (Exception)
			{
				// End of file.
				break;
			}
			// First, find out if the line is a comment.  It is if the
			// first part of the string exactly matches any of the comment strings.
			iscomment = false;
			bool revision_start = false;
			int comment_length = 0;
			for (i = 0; i < length_comments; i++)
			{
				// Find the length of the comment string...
				comment_length = ((string)commentIndicators[i]).Length;
				if (comment_length < 1)
				{
					continue;
				}
				// Allow characters too so do a regionMatches...
				revision_start = @string.regionMatches(true,0,(string)commentIndicators[i],0, ((string)commentIndicators[i]).Length);
				if (revision_start)
				{
					// Found a match...
					iscomment = true;
					if (Message.isDebugOn)
					{
						Message.printDebug(50, routine, "Found comment at line " + linecount);
					}
					break;
				}
			}
			// If we do not have a comment, then there is no need to continue in this loop because
			// we are out of the comments section in the file...
			if (!iscomment)
			{
				break;
			}
			// Find out if this is a header revision comment, and, if so, compare to the current values saved...
			string revision_string;
			if ((comment_length + revlen) <= @string.Length)
			{
				// There might be a header string
				revision_string = @string.Substring(comment_length, (revlen));
			}
			else
			{
				revision_string = @string.Substring(comment_length);
			}
			if (revision_string.Equals(HEADER_REVISION_STRING))
			{
				// This is a header revision line so read the revision number from the string...
				revision_string = @string.Substring(comment_length + revlen + 1);
				header_revision = StringUtil.atoi(revision_string);
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, routine, "Found header revision " + header_revision + "from \"" + @string + "\"");
				}
				header_first = Math.Min(header_first, header_revision);
				header_last = Math.Max(header_last, header_revision);
			}
			// Now determine whether this is a comment that can be ignored.
			// If so, we just do not add it to the list...
			isignore = false;
			if (ignoredCommentIndicators != null)
			{
				for (i = 0; i < ignoredCommentIndicators.Count; i++)
				{
					// Find the length of the comment string...
					len = ((string)ignoredCommentIndicators[i]).Length;
					string ignore_substring;
					if (len <= @string.Length)
					{
						ignore_substring = @string.Substring(0,len);
					}
					else
					{
						ignore_substring = @string.Substring(0);
					}
					if (ignore_substring.Equals((string)ignoredCommentIndicators[i]))
					{
						// Found a match...
						isignore = true;
						if (Message.isDebugOn)
						{
							Message.printDebug(dl, routine, "Ignoring: \"" + @string + "\"");
						}
						break;
					}
				}
			}
			// If the comment is to be ignored, read another line...
			if (isignore)
			{
				// Don't want to read any further.  First ignored comment indicates the entire header has been read
				break;
			}
			// If we have gotten to here, add the line to the list...
			@string = StringUtil.removeNewline(@string);
			header.addElement(@string);
			//FIXME SAM 2008-12-11 need to trap error
			//if ( list == (char **)NULL ) {
			//	HMPrintWarning ( 10, routine, "Error adding to string list" );
			//}
		}

		try
		{
			fp.Close();
		}
		catch (Exception)
		{
		}
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "\"" + fileName + "\":  " + header.size() + " lines in header");
		}
		header.setHeaderFirst(header_first);
		header.setHeaderLast(header_last);
		return header;
	}

	/// <summary>
	/// Get a list of files from a path list. </summary>
	/// <returns> a list of paths to a file given a prefix path and a file name.
	/// The files do not need to exist.  Return null if there is a problem with input. </returns>
	/// <param name="paths"> Paths to prefix the file with. </param>
	/// <param name="file"> Name of file to append to paths. </param>
	public static IList<string> getFilesFromPathList(IList<string> paths, string file)
	{
		string fullfile, routine = "IOUtil.getFilesFromPathList";
		IList<string> newlist = null;
		int i, npaths;

		// Check for NULL list, and file...

		if (paths == null)
		{
			Message.printWarning(10, routine, "NULL path list");
			return null;
		}
		if (string.ReferenceEquals(file, null))
		{
			Message.printWarning(10, routine, "NULL file name");
			return newlist;
		}

		npaths = paths.Count;
		newlist = new List<string>(10);
		string dirsep = System.getProperty("file.separator");
		for (i = 0; i < npaths; i++)
		{
			// Add each string to the list...
			fullfile = paths[i] + dirsep + file;
			newlist.Add(fullfile);
		}
		return newlist;
	}

	/// <summary>
	/// Return a list of files matching a pattern.
	/// Currently the folder must exist (no wildcard) and the file part of the path can contain wildcards using
	/// globbing notation (e.g., *.txt). </summary>
	/// <param name="folder"> folder to search for files </param>
	/// <param name="extension"> to match or use * to match all files </param>
	/// <param name="caseIndependent"> if true check extension case-independent (default is case-dependent)
	/// TODO SAM 2012-07-22 This method should be replaced with java.nio.file.PathMatcher when updated to Java 1.7. </param>
	public static IList<File> getFilesMatchingPattern(string folder, string extension, bool caseIndependent)
	{
		File f = new File(folder);
		// Get the list of all files in the folder (do this because want to do case-independent)...
		SimpleFileFilter filter = new SimpleFileFilter("*", "all");
		File[] files = f.listFiles(filter);
		IList<File> matchedFiles = new List<File>();
		if ((files == null) || (files.Length == 0))
		{
			return matchedFiles;
		}
		else
		{
			foreach (File f2 in files)
			{
				if ((string.ReferenceEquals(extension, null)) || extension.Length == 0 || extension.Equals("*"))
				{
					matchedFiles.Add(f2);
				}
				else
				{
					if (caseIndependent)
					{
						if (IOUtil.getFileExtension(f2.getName()).Equals(extension, StringComparison.OrdinalIgnoreCase))
						{
							matchedFiles.Add(f2);
						}
					}
					else
					{
						if (IOUtil.getFileExtension(f2.getName()).Equals(extension))
						{
							matchedFiles.Add(f2);
						}
					}
				}
			}
			return matchedFiles;
		}
	}

	/// <summary>
	/// Open an input stream given a URL or regular file name. </summary>
	/// <returns> An InputStream given a URL or file name.  If the string starts with
	/// "http:", "ftp:", or "file:", a URL is created and the associated stream is
	/// returned.  Otherwise, a file is opened and the associated stream is returned. </returns>
	/// <param name="url_string"> </param>
	/// <exception cref="IOException"> if the input stream cannot be initialized. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.io.InputStream getInputStream(String url_string) throws java.io.IOException
	public static Stream getInputStream(string url_string)
	{
		string routine = "IOUtil.getInputStream";
			URL url;
			FileStream fileStream;
			string noIndex = "Cannot open file at " + url_string + ".";

		// Make sure that the string is not empty...

		if (string.ReferenceEquals(url_string, null))
		{
			throw new IOException("URL is null");
		}
		if (url_string.Length < 1)
		{
			throw new IOException("URL is empty");
		}

		if (url_string.regionMatches(true, 0, "http:", 0, 5) || url_string.regionMatches(true, 0, "file:", 0, 5) || url_string.regionMatches(true, 0, "ftp:", 0, 4))
		{
			try
			{
				url = new URL(url_string);
				return (url.openStream());
			}
			catch (Exception)
			{
				Message.printWarning(10, routine, noIndex);
				throw new IOException(noIndex);
			}
		}
		else
		{
			try
			{
				fileStream = new FileStream(url_string, FileMode.Open, FileAccess.Read);
				return (fileStream);
			}
			catch (Exception)
			{
				Message.printWarning(10, routine, noIndex);
				throw new IOException(noIndex);
			}
		}
	}

	/// <summary>
	/// Returns the contents of the manifests in all the Jar files in the classpath. 
	/// The contents are returned are a formatted list of Strings.  The name of
	/// each Jar file is printed, followed by a list of the lines in its manifest.
	/// If there are further Jar files, a space is added and the pattern is repeated.<para>
	/// The order of the Jar files in the list is the same as the order the Jar files
	/// </para>
	/// appear in the CLASSPATH.<para>
	/// The order of the manifest data is not necessarily the same as how they appear 
	/// in the manifest file.  The Java classes provided for accessing Manifest data
	/// do not return the data in any given order.  For this reason, the manifest data
	/// are sorted alphabetically in the list.
	/// </para>
	/// </summary>
	/// <returns> the contents of the manifests of the Jar files in a Vector of Strings. </returns>
	public static IList<string> getJarFilesManifests()
	{
		string routine = "IOUtil.getJarFilesManifests";

		// Get the Classpath and split it into a String array.  The order
		// of the elements in the array is the same as the order in which
		// things are included in the classpath.
		string[] jars = System.getProperty("java.class.path").Split(System.getProperty("path.separator"));

		Attributes a = null;
		int j = -1;
		int size = -1;
		JarFile jar = null;
		Manifest mf = null;
		object[] o = null;
		ISet<object> set = null;
		string tab = "    ";
		IList<string> sort = null;
		IList<string> v = new List<string>();

		for (int i = 0; i < jars.Length; i++)
		{
			if (!StringUtil.endsWithIgnoreCase(jars[i], ".jar"))
			{
				// directories, etc, can be specified in a class path
				// but avoid those for just the jar files in the class path.
				continue;
			}

			v.Add(jars[i]);

			try
			{
				jar = new JarFile(jars[i]);
				mf = jar.getManifest();
				a = mf.getMainAttributes();
				set = a.Keys;
				o = set.ToArray();
				sort = new List<string>();
				for (j = 0; j < o.Length; j++)
				{
					sort.Add(tab + ((Attributes.Name)(o[j])) + " = " + a.getValue((Attributes.Name)(o[j])));
				}

				// the order in which the data in the manifest file 
				// are returned is not guaranteed to be in the same
				// order as they are in the manifest file.  Thus, the
				// data are sorted to present a consistent return pattern.
				sort.Sort();
				size = sort.Count;
				for (j = 0; j < size; j++)
				{
					v.Add(sort[j]);
				}
			}
			catch (Exception e)
			{
				Message.printWarning(2, routine, "An error occurred while reading the manifest for: '" + jars[i] + "'.");
				Message.printWarning(3, routine, e);
				v.Add(tab + "An error occurred while reading the manifest.");
			}
			finally
			{
				try
				{
					jar.close();
				}
				catch (IOException)
				{
					// Should not happen
				}
			}

			v.Add("");
		}

		return v;
	}

	/// <summary>
	/// Return the Java Runtime Environment architecture bits. </summary>
	/// <returns> the JRE bits, 32 or 64 </returns>
	public static int getJreArchBits()
	{
		string arch = System.getProperty("sun.arch.data.model");
		if (string.ReferenceEquals(arch, null))
		{
			return -1;
		}
		int bits = int.Parse(arch);
		return bits;
	}

	/// <summary>
	/// Return the operating system architecture bits.  This is only enabled on Windows. </summary>
	/// <returns> the architecture bits, 32 or 64. </returns>
	public static int getOSArchBits()
	{
		if (!isUNIXMachine())
		{
			string arch = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
			string wow64Arch = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432");

			int realArch = 32;
			if (((!string.ReferenceEquals(arch, null)) && arch.EndsWith("64", StringComparison.Ordinal)) || ((!string.ReferenceEquals(wow64Arch, null)) && wow64Arch.EndsWith("64", StringComparison.Ordinal)) || System.getProperty("os.arch").contains("64") && !System.getProperty("os.arch").Equals("IA64N"))
			{
				//IA64N, despite its name, is not actually 64 bit
				//see http://h30499.www3.hp.com/t5/System-Administration/Java-SDK-What-are-IA64N-and-IA64W/td-p/4863858
				realArch = 64;
			}
			return realArch;
		}
		return 32;
	}

	/// <summary>
	/// Return a path considering the working directory set by
	/// setProgramWorkingDir().  The following rules are used:
	/// <ul>
	/// <li>	If the path is null or empty, return the path.</li>
	/// <li>	If the path is an absolute path (starts with / or \ or has : as the
	/// second character; or starts with http:, ftp:, file:), it is returned as is.</li>
	/// <li>	If the path is a relative path and the working directory is ".", the path is returned.</li>
	/// <li>	If the path is a relative path and the working directory is not ".",
	/// the path is appended to the current working directory (separated with
	/// / or \ as appropriate) and returned.</li>
	/// </ul> </summary>
	/// <param name="path"> Path to use. </param>
	/// <returns> a path considering the working directory. </returns>
	public static string getPathUsingWorkingDir(string path)
	{
		string routine = "IOUtil.getPathUsingWorkingDir";
		if ((string.ReferenceEquals(path, null)) || (path.Length == 0))
		{
			return path;
		}
		// Check for URL...
		if (path.StartsWith("http:", StringComparison.Ordinal) || path.StartsWith("ftp:", StringComparison.Ordinal) || path.StartsWith("file:", StringComparison.Ordinal))
		{
			return path;
		}
		// Check for absolute path...
		if (isUNIXMachine())
		{
			if (path[0] == '/')
			{
				return path;
			}
			if (_working_dir.Equals("") || _working_dir.Equals("."))
			{
				return path;
			}
			else
			{
				string fullPath = path;
				try
				{
					fullPath = ((new File(_working_dir + "/" + path)).getCanonicalPath().ToString());
				}
				catch (IOException e)
				{
					Message.printWarning(3, routine, e);
					// FIXME SAM 2009-05-05 Evaluate whether to do the following - used for startup issues before logging?
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
				return fullPath;
				//return ( _working_dir + "/" + path );
			}
		}
		else
		{
			if (path.StartsWith("\\\\", StringComparison.Ordinal))
			{
				// UNC path
				return path;
			}
			if ((path[0] == '\\') || ((path.Length >= 2) && (path[1] == ':')))
			{
				return path;
			}
			if (_working_dir.Equals("") || _working_dir.Equals("."))
			{
				return path;
			}
			else
			{
				string fullPath = path;
					try
					{
						fullPath = ((new File(_working_dir + "\\" + path)).getCanonicalPath().ToString());
					}
				catch (IOException e)
				{
					Message.printWarning(3, routine, e);
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
				return fullPath;
				//return ( _working_dir + "\\" + path );
			}
		}
	}

	/// <summary>
	/// Return the program arguments. </summary>
	/// <returns> The program arguments set with setProgramArguments. </returns>
	/// <seealso cref= #setProgramArguments </seealso>
	public static string[] getProgramArguments()
	{
		if (!_initialized)
		{
			initialize();
		}
		return _argv;
	}

	/// <summary>
	/// Return the program command file. </summary>
	/// <returns> The command file used with the program, as set by setProgramCommandFile. </returns>
	/// <seealso cref= #setProgramCommandFile </seealso>
	public static string getProgramCommandFile()
	{
		if (!_initialized)
		{
			initialize();
		}
		return _command_file;
	}

	/// <summary>
	/// Return the program command list.  Typically either a command file or list is
	/// used and the list takes precedence. </summary>
	/// <returns> The command list used with the program, as set by setProgramCommandList. </returns>
	/// <seealso cref= #setProgramCommandList </seealso>
	public static IList<string> getProgramCommandList()
	{
		if (!_initialized)
		{
			initialize();
		}
		return _command_list;
	}

	/// <summary>
	/// Return name of host machine. </summary>
	/// <returns> The host that is running the program, as set by setProgramHost. </returns>
	/// <seealso cref= #setProgramHost </seealso>
	public static string getProgramHost()
	{
		if (!_initialized)
		{
			initialize();
		}
		return _host;
	}

	/// <summary>
	/// Return the program name. </summary>
	/// <returns> The program name as set by setProgramName. </returns>
	/// <seealso cref= #setProgramName </seealso>
	public static string getProgramName()
	{
		if (!_initialized)
		{
			initialize();
		}
		return _progname;
	}

	/// <summary>
	/// Return the program user. </summary>
	/// <returns> The program user as set by setProgramUser. </returns>
	/// <seealso cref= #setProgramUser </seealso>
	public static string getProgramUser()
	{
		if (!_initialized)
		{
			initialize();
		}
		return _user;
	}

	/// <summary>
	/// Return the program version. </summary>
	/// <returns> The program version as set by setProgramVersion. </returns>
	/// <seealso cref= #setProgramVersion </seealso>
	public static string getProgramVersion()
	{
		if (!_initialized)
		{
			initialize();
		}
		return _progver;
	}

	/// <summary>
	/// Return the working directory. </summary>
	/// <returns> The program working directory as set by setProgramWorkingDir. </returns>
	/// <seealso cref= #setProgramWorkingDir </seealso>
	public static string getProgramWorkingDir()
	{
		if (!_initialized)
		{
			initialize();
		}
		return _working_dir;
	}

	/// <summary>
	/// Return global PropList property. </summary>
	/// <returns> The property in the global property list manager corresponding to the given key.
	/// <b>This routine is being reworked to be consistent with the Prop* classes.</b> </returns>
	/// <param name="key"> String key to look up a property. </param>
	/// <seealso cref= Prop </seealso>
	/// <seealso cref= PropList </seealso>
	/// <seealso cref= PropListManager </seealso>
	public static Prop getProp(string key)
	{
		if (!_initialized)
		{
			initialize();
		}
		return _prop_list_manager.getProp(key);
	}

	/// <summary>
	/// Return property value as an Object. </summary>
	/// <returns> The value of a property in the global property list manager corresponding to the given key. </returns>
	/// <param name="key"> String key to look up a property. </param>
	/// <seealso cref= Prop </seealso>
	/// <seealso cref= PropList </seealso>
	/// <seealso cref= PropListManager </seealso>
	public static object getPropContents(string key)
	{
		if (!_initialized)
		{
			initialize();
		}
		Prop prop = getProp(key);
		if (string.ReferenceEquals(key, null))
		{
			return null;
		}
		return prop.getContents();
	}

	/// <summary>
	/// Return global PropListManager. </summary>
	/// <returns> the instance of the global property list manager. </returns>
	/// <seealso cref= Prop </seealso>
	/// <seealso cref= PropList </seealso>
	/// <seealso cref= PropListManager </seealso>
	public static PropListManager getPropListManager()
	{
		if (!_initialized)
		{
			initialize();
		}
		return _prop_list_manager;
	}

	/// <summary>
	/// Return property value as a String. </summary>
	/// <returns> The value of a property in the global property list manager corresponding to the given key. </returns>
	/// <param name="key"> String key to look up a property. </param>
	/// <seealso cref= Prop </seealso>
	/// <seealso cref= PropList </seealso>
	/// <seealso cref= PropListManager </seealso>
	public static string getPropValue(string key)
	{
		if (!_initialized)
		{
			initialize();
		}
		try
		{
			Prop prop = getProp(key);
			if (prop == null)
			{
				return null;
			}
			return prop.getValue();
		}
		catch (Exception)
		{
			// Probably a security exception...
			return null;
		}
	}

	/// <summary>
	/// Returns a list of Strings containing information about the system on which
	/// the Java application is currently running. </summary>
	/// <returns> a list of Strings. </returns>
	public static IList<string> getSystemProperties()
	{
		string tab = "    ";

		IList<string> v = new List<string>();

		v.Add("System Properties Defined for Application: ");
		v.Add(tab + " Program Name: " + _progname + " " + _progver);
		v.Add(tab + " User Name: " + _user);
		string now = TimeUtil.getSystemTimeString("");
		v.Add(tab + " Date: " + now);
		v.Add(tab + " Host: " + _host);
		v.Add(tab + " Working Directory: " + _working_dir);
		string command = tab + " Command: " + _progname + " ";

		int totalLength = command.Length;

		int length = 0;

		if (_argv != null)
		{
			for (int i = 0; i < _argv.Length; i++)
			{
				length = _argv[i].Length + 1;

				if (totalLength + length >= 80)
				{
					// it would be too big for the line, so add the current line and put the next
					// argument on what will be the next line
					v.Add(command);
					command = tab + tab + _argv[i];
				}
				else
				{
					command += _argv[i] + " ";
				}
				totalLength = command.Length;
			}
		}
		v.Add(command);
		v.Add("");

		v.Add("Operating System Information");
		v.Add(tab + "Name: " + System.getProperty("os.name"));
		v.Add(tab + "Version: " + System.getProperty("os.version"));
		v.Add(tab + "System Architecture: " + System.getProperty("os.arch"));
		v.Add("");

		v.Add("Java Virtual Machine Memory Information: ");
		Runtime r = Runtime.getRuntime();
		v.Add(tab + "Maximum memory (see Java -Xmx): " + r.maxMemory() + " bytes, " + r.maxMemory() / 1024 + " kb, " + r.maxMemory() / 1048576 + " mb");
		v.Add(tab + "Total memory (will be increased to maximum as needed): " + r.totalMemory() + " bytes, " + r.totalMemory() / 1024 + " kb, " + r.totalMemory() / 1048576 + " mb");
		long used = r.totalMemory() - r.freeMemory();
		v.Add(tab + "Used memory: " + used + " bytes, " + used / 1024 + " kb, " + used / 1048576 + " mb");
		v.Add(tab + "Free memory: " + r.freeMemory() + " bytes, " + r.freeMemory() / 1024 + " kb, " + r.freeMemory() / 1048576 + " mb");
		v.Add("");

		v.Add("Java Virtual Machine Properties (System.getProperties()): ");
		Properties properties = System.getProperties();
		ISet<string> names = properties.stringPropertyNames();
		List<string> nameList = new List<string>(names);
		nameList.Sort();
		foreach (string name in nameList)
		{
			if (name.Equals("line.separator"))
			{
				// Special case because printing actual character will be invisible
				string nl = System.getProperty(name);
				nl = nl.Replace("\r", "\\r");
				nl = nl.Replace("\n", "\\n");
				v.Add(tab + " " + name + " = \"" + nl + "\"");
			}
			else
			{
				v.Add(tab + " " + name + " = \"" + System.getProperty(name) + "\"");
			}
		}
		v.Add("");

		v.Add("Java Information");
		v.Add(tab + "Vendor: " + System.getProperty("java.vendor"));
		v.Add(tab + "Version: " + System.getProperty("java.version"));
		v.Add(tab + "Home: " + System.getProperty("java.home"));

		string sep = System.getProperty("path.separator");

		string[] jars = System.getProperty("java.class.path").Split(sep);

		if (jars.Length == 0)
		{
			return v;
		}

		string cp = tab + "Classpath: " + jars[0];
		totalLength = cp.Length;

		for (int i = 1; i < jars.Length; i++)
		{
			length = jars[i].Length;

			if (totalLength + length >= 80)
			{
				v.Add(cp + sep);
				cp = tab + tab + jars[i];
			}
			else
			{
				cp += sep + jars[i];
			}
			totalLength = cp.Length;
		}
		v.Add(cp);

		v.Add("");

		return v;
	}

	/// <summary>
	/// Determines JVM through which the application/applet is currently running </summary>
	/// <returns> the vendor as SUN, MICROSOFT, etc... </returns>
	public static int getVendor()
	{
		string s = System.getProperty("java.vendor.url");
		if (s.Equals("http://www.sun.com", StringComparison.OrdinalIgnoreCase))
		{
			return SUN;
		}
		else if (s.Equals("http://www.microsoft.com", StringComparison.OrdinalIgnoreCase))
		{
			return MICROSOFT;
		}
		else
		{
			return UNKNOWN;
		}
	}

	/// <summary>
	/// Initialize the global data.  setApplet() should be called first in an applet
	/// to allow some of the if statements below to be executed properly.
	/// </summary>
	private static void initialize()
	{
		string routine = "IOUtil.initialize";
		int dl = 1;

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Initializing IOUtil...");
		}
		try
		{
			// Put this in just in case we have security problems...
			if (_is_applet)
			{
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, routine, "An applet!");
				}
				_command_file = "";
				_command_list = null;
				// TODO (JTS - 2005-06-06) should do some testing to see what the effects are
				// of doing a host set like in the non-applet code below.  Possibilities I foresee:
				// 1) applets lack the permission to get the hostname
				// 2) applets return the name of the computer the user is physically working on.
				// 3) applets return the name of the server on which the applet code actually resides.
				// I have no way of knowing right now which one would
				// be the case, and moreover, no time to test this.
				_host = "web server/client/URL unknown";
				_progname = "program name unknown";
				_progver = "version unknown";
				_user = "user unknown (applet)";
				_working_dir = "dir unknown (applet)";
				__homeDir = "dir unknown (applet)";
			}
			else
			{
				// A stand-alone application...
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, routine, "Not an applet!");
				}
				_command_file = "";
				_command_list = null;
				_host = InetAddress.getLocalHost().getHostName();
				_progname = "program name unknown";
				_progver = "version unknown";
				_user = System.getProperty("user.name");
				_working_dir = System.getProperty("user.dir");
				__homeDir = System.getProperty("user.dir");
			}
		}
		catch (Exception e)
		{
			// Don't do anything.  Just print a warning...
			Message.printWarning(3, routine, "Caught an exception initializing IOUtil (" + e + ").  Continuing.");
		}

		// Initialize the applet context...

		_applet_context = null;

		// Initialize the property list manager to contain an unnamed list...

		_prop_list_manager = new PropListManager();
		_prop_list_manager.addList(new PropList("", PropList.FORMAT_PROPERTIES), true);

		// Set the flag to know that the class has been initialized...

		_initialized = true;
	}

	/// <summary>
	/// Return true if the program is an applet (must have set with setApplet). </summary>
	/// <seealso cref= #setApplet </seealso>
	public static bool isApplet()
	{
		return _is_applet;
	}

	/// <summary>
	/// Set whether the program is an Applet (often called by other routines).
	/// DO NOT CALL INITIALIZE BEFORE SETTING.  THIS
	/// FUNCTION IS EXPECTED TO BE CALLED FIRST THING IN THE init() FUNCTION OF
	/// AN APPLET CLASS.  THEN, initialize() WILL KNOW TO TREAT AS AN APPLET!
	/// initialize() is called automatically from this method. </summary>
	/// <param name="is_applet"> true or false, indicatign whether an Applet. </param>
	/// @deprecated Use setApplet() 
	/// <seealso cref= #setApplet </seealso>
	public static void isApplet(bool is_applet)
	{
		int dl = 1;

		_is_applet = is_applet;
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, "IOUtil.isApplet", "set _is_applet to " + _is_applet);
		}
		// Force the reinitialization.  Problems may have occurred when IO
		// first loaded if it did not know that we are running an Applet so reinitialize now.
		initialize();
	}

	/// <summary>
	/// Return true if the program is a batch program. </summary>
	/// <returns> true if the program is a batch program. </returns>
	public static bool isBatch()
	{
		return _is_batch;
	}

	/// <summary>
	/// Set whether the program is running in batch mode (the default if not set is false). </summary>
	/// <param name="is_batch"> Indicates whether the program is batch mode. </param>
	public static void isBatch(bool is_batch)
	{
		_is_batch = is_batch;
		if (Message.isDebugOn)
		{
			Message.printDebug(1, "IOUtil.isBatch", "Batch mode is " + _is_batch);
		}
	}

	/// <summary>
	/// Determine whether the machine is big or little endian.  This method should be
	/// used when dealing with binary files written using native operating system
	/// applications (e.g., native C, C++, and FORTRAN compilers).  The Java Virtual
	/// Machine is big endian so any binary files written with Java are transparently
	/// big endian.  If little endian files need to be read, use the
	/// EndianDataInputStream and other classes in this package.
	/// Currently the determinatoin is made by looking at the operating system.  The following are assumed:
	/// <pre>
	/// Linux	          LittleEndian
	/// All other UNIX    BigEndian
	/// All others        LittleEndian
	/// </pre> </summary>
	/// <returns> true if the machine is big endian. </returns>
	public static bool isBigEndianMachine()
	{
		string name = System.getProperty("os.name");
		if (name.Equals("Linux", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		else if (isUNIXMachine())
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Determine whether a path is an absolute path.
	/// The standard Java File.isAbsolute() only returns true on Windows if the leading
	/// path contains a drive like C:\xxxx or if the path begins with two backslashes.
	/// A single backslash will return false.  This method will return true for a single
	/// backslash at the front of a string on Windows.
	/// </summary>
	public static bool isAbsolute(string path)
	{
		if (!isUNIXMachine() && path.StartsWith("\\", StringComparison.Ordinal))
		{
			// UNC will match this, as well as normal paths
			return true;
		}
		// Use the standard method...
		File f = new File(path);
		return f.isAbsolute();
	}

	/// <summary>
	/// Checks to see if the given port is open.  Open ports can be used.  Ports that
	/// are not open are already in use by some other process. </summary>
	/// <param name="port"> the port number to check. </param>
	/// <returns> whether the port is open or not. </returns>
	public static bool isPortOpen(int port)
	{
		try
		{
			(new ServerSocket(port)).close();
			(new ServerSocket(port)).close();
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	/// <summary>
	/// Returns whether the program is running as an applet (must be set with setRunningApplet).
	/// </summary>
	public static bool isRunningApplet()
	{
		return __runningApplet;
	}

	/// <summary>
	/// Determine if a UNIX machine.  The following seem to be standard, although there are variations depending
	/// on hardware and OS version:
	/// <pre>
	/// Operating System      os.arch    os.name          os.version
	/// Mac OS X              x86_64     "Mac OS X"       10.6.4
	/// Windows 95            x86        "Windows 95"     4.0
	/// Windows 98            x86        "Windows 95"     4.1
	/// NT                    x86        "Windows NT"     4.0
	/// Windows 2000          x86        "Windows NT"     5.0
	/// Linux                 i386       "Linux"
	/// HP-UX                 PA-RISC
	/// </pre> </summary>
	/// <returns> true if a UNIX platform, including os.name of Linux, false if not (presumably Windows). </returns>
	public static bool isUNIXMachine()
	{
		string arch = System.getProperty("os.arch").ToUpper();
		string name = System.getProperty("os.name").ToUpper();
		if (arch.Equals("UNIX") || arch.Equals("PA-RISC") || name.Equals("LINUX") || name.StartsWith("MAC OS X", StringComparison.Ordinal))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Sets an array and all its elements to null (for garbage collection).  Care
	/// should be taken to ensure that this method is NOT used with static data, as
	/// the finalize method of any instance of an Object will clear the static data
	/// for all instances of the Object. </summary>
	/// <param name="array"> the array to null.  Can already be null. </param>
	public static void nullArray(object[] array)
	{
		if (array == null)
		{
			return;
		}
		int size = array.Length;
		for (int i = 0; i < size; i++)
		{
			array[i] = null;
		}
	}

	/// <summary>
	/// Open the resource identified by the URL using the appropriate application for the operating system and user
	/// environment.  On Windows, determine the default application using the file extension (e.g., "html" will result in
	/// a web browser).  On UNIX/Linux, a web browser is always used. </summary>
	/// <param name="url"> URL to open. </param>
	/// @deprecated use the java.awt.Desktop class 
	public static void openURL(string url)
	{
		try
		{
			Desktop desktop = Desktop.getDesktop();
			desktop.browse(new URI(url));
		}
		catch (Exception e)
		{
			Message.printWarning(2, "IOUtil.openURL", "Could not open application to view to URL \"" + url + "\" uniquetempvar.");
		}
	}

	/// <summary>
	/// Print a standard header to a file.  See the overloaded method for more
	/// information.  It is assumed that the file is not an XML file. </summary>
	/// <param name="ofp"> PrintWriter that is being written to. </param>
	/// <param name="comment0"> The String to use for comments. </param>
	/// <param name="maxwidth"> The maximum length of a line of output (if whitespace is
	/// embedded in the header information, lines will be broken appropriately to fit within the specified length. </param>
	/// <param name="flag"> Currently unused. </param>
	/// <returns> 0 if successful, 1 if not. </returns>
	public static int printCreatorHeader(PrintWriter ofp, string comment0, int maxwidth, int flag)
	{
		return printCreatorHeader(ofp, comment0, maxwidth, flag, null);
	}

	/// <summary>
	/// Print a header to a file.  The header looks like the following:
	/// <para>
	/// <pre>
	/// # File generated by
	/// # program:   demandts 2.7 (25 Jun 1995)
	/// # user:      sam
	/// # date:      Mon Jun 26 14:49:18 MDT 1995
	/// # host:      white
	/// # directory: /crdss/dmiutils/demandts/data
	/// # command:   ../src/demandts -d1 -w1,10 -demands -istatemod 
	/// #            /crdss/statemod/data/white/white.ddh -icu 
	/// #            /crdss/statemod/data/white/white.ddc -sstatemod 
	/// #            /crdss/statemod/data/white/white.dds -eff12 
	/// </pre>
	/// </para>
	/// <para>
	/// </para>
	/// </summary>
	/// <param name="ofp"> PrintWriter that is being written to. </param>
	/// <param name="commentLinePrefix"> The string to use for the start of comment lines (e.g., "#"). </param>
	/// <param name="maxwidth"> The maximum length of a line of output (if whitespace is
	/// embedded in the header information, lines will be broken appropriately to fit
	/// within the specified length. </param>
	/// <param name="flag"> Currently unused. </param>
	/// <param name="props"> Properties used to format the header.  Currently the only
	/// property that is recognized is "IsXML", which can be "true" or "false".  XML
	/// files must be handled specifically because some characters that may be printed
	/// to the header may not be handled by the XML parser.  The opening and closing
	/// XML tags must be added before and after calling this method. </param>
	/// <returns> 0 if successful, 1 if not. </returns>
	public static int printCreatorHeader(PrintWriter ofp, string commentLinePrefix, int maxwidth, int flag, PropList props)
	{
		string routine = "IOUtil.printCreatorHeader";
		bool isXml = false;
		// Figure out properties...
		if (props != null)
		{
			string prop_value = props.getValue("IsXML");
			if ((!string.ReferenceEquals(prop_value, null)) && prop_value.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				isXml = true;
				// If XML, do not print multiple dashes together in the comments below.
			}
		}

		if (ofp == null)
		{
			Message.printWarning(2, routine, "Output file pointer is NULL");
			return 1;
		}

		if (!_initialized)
		{
			initialize();
		}

		// Get the formatted header comments...

		IList<string> comments = formatCreatorHeader(commentLinePrefix, maxwidth, isXml);

		foreach (string c in comments)
		{
			ofp.println(c);
		}
		ofp.flush();
		return 0;
	}

	/// <summary>
	/// Print a list of strings to a file.  The file is created, opened, and closed. </summary>
	/// <param name="file"> name of file to write. </param>
	/// <param name="strings"> list of strings to write. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void printStringList(String file, java.util.List<String> strings) throws java.io.IOException
	public static void printStringList(string file, IList<string> strings)
	{
		string message, routine = "IOUtil.printStringList";
		PrintWriter ofp;

		// Open the file...

		try
		{
			ofp = new PrintWriter(new FileStream(file, FileMode.Create, FileAccess.Write));
		}
		catch (Exception)
		{
			message = "Unable to open output file \"" + file + "\"";
			Message.printWarning(2, routine, message);
			throw new IOException(message);
		}
		try
		{
			printStringList(ofp, strings);
		}
		finally
		{
			  // Flush and close the file...
			ofp.flush();
			ofp.close();
		}
	}

	/// <summary>
	/// Print a list of strings to an opened file. </summary>
	/// <param name="ofp"> PrintWrite to write to. </param>
	/// <param name="strings"> list of strings to write. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void printStringList(java.io.PrintWriter ofp, java.util.List<String> strings) throws java.io.IOException
	public static void printStringList(PrintWriter ofp, IList<string> strings)
	{
		if (strings == null)
		{
			return;
		}
		int size = strings.Count;
		for (int i = 0; i < size; i++)
		{
			ofp.println(strings[i]);
		}
	}

	/// @deprecated Use the overloaded version that uses List parameters. 
	/// <param name="oldFile"> </param>
	/// <param name="newFile"> </param>
	/// <param name="newComments"> </param>
	/// <param name="commentIndicators"> </param>
	/// <param name="ignoredCommentIndicators"> </param>
	/// <param name="flags"> </param>
	/// <returns> PrintWriter to allow additional writing to the file. </returns>
	public static PrintWriter processFileHeaders(string oldFile, string newFile, string[] newComments, string[] commentIndicators, string[] ignoredCommentIndicators, int flags)
	{
		IList<string> newCommentList = null;
		IList<string> commentIndicatorList = null;
		IList<string> ignoreCommentIndicatorList = null;
		if (newComments != null)
		{
			newCommentList = StringUtil.toList(newComments);
		}
		if (commentIndicators != null)
		{
			commentIndicatorList = StringUtil.toList(commentIndicators);
		}
		if (ignoredCommentIndicators != null)
		{
			ignoreCommentIndicatorList = StringUtil.toList(ignoredCommentIndicators);
		}
		return processFileHeaders(oldFile, newFile, newCommentList, commentIndicatorList, ignoreCommentIndicatorList, flags);
	}

	/// <summary>
	/// This method should be used to process the header of a file that is going through
	/// revisions over time.  It can be used short of full revision control on the file.
	/// The old file header will be copied to the new file using special comments
	/// (assume # is comment):
	/// <para>
	/// 
	/// <pre>
	/// #HeaderRevision 1
	/// </pre>
	/// </para>
	/// <para>
	/// 
	/// Where the number indicates the revision for the header.  The initial header will be number 0.
	/// </para>
	/// </summary>
	/// <returns> PrintWriter for the file (it will be opened and processed so that the
	/// new file header consists of the old header with new comments at the top).  The
	/// file can then be written to.  Return null if the new file cannot be opened. </returns>
	/// <param name="oldFile"> An existing file whose header is to be updated. </param>
	/// <param name="newFile"> The name of the new file that is to contain the updated header
	/// (and will be pointed to by the returned PrintWriter (it can be the same as
	/// "oldfile").  If the name of the file ends in XML then the file is assumed to
	/// be an XML file and the header is wrapped in <!-- --> (this may change to actual XML tags in the future). </param>
	/// <param name="newComments"> list of strings to be added as comments in the new revision (often null). </param>
	/// <param name="commentIndicators"> list of strings that indicate comment lines that should be retained in the next revision. </param>
	/// <param name="ignoredCommentIndicators"> list of strings that indicate comment lines that
	/// can be ignored in the next revision (e.g., lines that describe the file format that only need to appear once). </param>
	/// <param name="flags"> Currently unused. </param>
	public static PrintWriter processFileHeaders(string oldFile, string newFile, IList<string> newComments, IList<string> commentIndicators, IList<string> ignoredCommentIndicators, int flags)
	{
		string comment;
		string routine = "IOUtil.processFileHeaders";
		FileHeader oldheader;
		PrintWriter ofp = null;
		int dl = 50, i, header_last = -1, header_revision, wl = 20;
		bool is_xml = false;

		// Get the old file header...

		if (string.ReferenceEquals(oldFile, null))
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "NULL old file - no old header");
			}
			oldheader = null;
		}
		else if (oldFile.Length == 0)
		{
			Message.printWarning(dl, routine, "Empty old file - no old header");
			oldheader = null;
		}
		else
		{
			// Try to get the header...
			oldheader = getFileHeader(oldFile, commentIndicators, ignoredCommentIndicators, 0);
			if (oldheader != null)
			{
				header_last = oldheader.getHeaderLast();
			}
		}

		// Open the new output file...

		try
		{
			ofp = new PrintWriter(new FileStream(newFile, FileMode.Create, FileAccess.Write));
			if (StringUtil.endsWithIgnoreCase(newFile,".xml"))
			{
				is_xml = true;
			}
		}
		catch (Exception e)
		{
			Message.printWarning(3, routine, e);
			Message.printWarning(wl, routine, "Unable to open output file \"" + newFile + "\"");
			return null;
		}

		// Print the new file header.  If a comment string is not specified, use the default...

		if ((commentIndicators == null) || (commentIndicators.Count == 0))
		{
			comment = UNIVERSAL_COMMENT_STRING;
		}
		else
		{
			comment = (string)commentIndicators[0];
		}
		header_revision = header_last + 1;
		if (is_xml)
		{
			ofp.println("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
			ofp.println("<!--");
		}
		ofp.println(comment + HEADER_REVISION_STRING + " " + header_revision);
		ofp.println(comment);

		// Now print the standard header...

		PropList props = new PropList("header");
		if (is_xml)
		{
			props.set("IsXML=true");
		}
		printCreatorHeader(ofp, comment, 80, 0, props);

		// Now print essential comments for this revision.  These strings do not have the comment prefix...

		if (newComments != null)
		{
			if (newComments.Count > 0)
			{
				if (is_xml)
				{
					ofp.println(comment);
				}
				else
				{
					ofp.println(comment + "----");
				}
				for (i = 0; i < newComments.Count; i++)
				{
					ofp.println(comment + " " + newComments[i]);
				}
			}
		}

		if (is_xml)
		{
			ofp.println(comment);
		}
		else
		{
			ofp.println(comment + "------------------------------------------------");
		}

		// Now print the old header.  It already has the comment character.

		if (oldheader != null)
		{
			if (oldheader.size() > 0)
			{
				for (i = 0; i < oldheader.size(); i++)
				{
					ofp.println(oldheader.elementAt(i));
				}
			}
		}

		if (is_xml)
		{
			ofp.println("-->");
		}
		return ofp;
	}

	// TODO SAM 2012-06-28 Perhaps return an object that has the response code and output strings - refactor later if needed.
	/// <summary>
	/// Read a response given a URL string.  If the response code is >= 400 the result is read from the error stream.
	/// Otherwise, the response is read from the input stream. </summary>
	/// <returns> the string read from a URL. </returns>
	/// <param name="urlString"> the URL to read from. </param>
	/// <param name="errorCheck"> if specified, the  </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static String readFromURL(String urlString) throws java.net.MalformedURLException, java.io.IOException
	public static string readFromURL(string urlString)
	{
		URL url = new URL(urlString);
		// Open the input stream...
		HttpURLConnection urlConnection = (HttpURLConnection)url.openConnection();
		Stream @in = null;
		if (urlConnection.getResponseCode() >= 400)
		{
			@in = urlConnection.getErrorStream();
		}
		else
		{
			@in = urlConnection.getInputStream();
		}
		StreamReader inp = new StreamReader(@in);
		StreamReader reader = new StreamReader(inp);
		char[] buffer = new char[8192];
		int len1 = 0;
		StringBuilder b = new StringBuilder();
		while ((len1 = reader.Read(buffer, 0, buffer.Length)) != -1)
		{
			b.Append(buffer,0,len1);
		}
		@in.Close();
		urlConnection.disconnect();
		return b.ToString();
	}

	/// <summary>
	/// Replaces old file extension with new one.  If the file has no
	/// extension then it adds the extension specified. </summary>
	/// <param name="file"> File name to change extension on. </param>
	/// <param name="extension"> New file extension. </param>
	/// <returns> file_new New file name with replaced extension. </returns>
	/// <exception cref="IOException">  </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static String replaceFileExtension(String file, String extension) throws java.io.IOException
	public static string replaceFileExtension(string file, string extension)
	{
		// First make sure the file is an absolute value this makes it easier to check and replace
		File tmp = new File(file);
		if (!tmp.isAbsolute())
		{
			file = tmp.getCanonicalPath().ToString();
		}
		tmp = null;

		// Add a period to the beginning of the extension if one doesn't exist already
		if (!(extension.StartsWith(".", StringComparison.Ordinal)))
		{
			extension = "." + extension;
		}
		IList<string> v = StringUtil.breakStringList(file, ".", 0);
		if ((v == null) || (v.Count == 0))
		{
			// didn't have an extension so add one and return it
			if (!string.ReferenceEquals(file, null) && file.Length > 0)
			{
				return file += extension;
			}
		}
		string file_new = "";
		for (int i = 0; i < v.Count - 1; i++)
		{
			file_new += (string)v[i];
		}
		// add the new extension
		file_new += extension;
		return file_new;
	}

	/// <summary>
	/// Set the applet for a program.  This is generally called from the init() method
	/// of an application.  This method then saves the AppletContext and DocumentBase
	/// for later use.  After calling with a non-null Applet, isApplet() will return true. </summary>
	/// <param name="applet"> The Applet for the application. </param>
	/// <seealso cref= #isApplet </seealso>
	/// <seealso cref= #getApplet </seealso>
	/// <seealso cref= #getAppletContext </seealso>
	/// <seealso cref= #getDocumentBase </seealso>
	public static void setApplet(Applet applet)
	{
		if (applet != null)
		{
			_applet = applet;
			_applet_context = applet.getAppletContext();
			_document_base = applet.getDocumentBase();
			_is_applet = true;
		}
		// Do this after setting the applet so that the initialization can check...
		if (!_initialized)
		{
			initialize();
		}
	}

	/// <summary>
	/// Set the AppletContext.  This is generally only called from low-level code
	/// (normally just need to call setApplet()). </summary>
	/// <param name="applet_context"> The AppletContext for the current applet. </param>
	/// <seealso cref= #setApplet </seealso>
	/// <seealso cref= #getAppletContext </seealso>
	public static void setAppletContext(AppletContext applet_context)
	{
		_applet_context = applet_context;
	}

	/// <summary>
	/// Sets the application home directory.  This is a base directory that should 
	/// only be set once during an application run.  It is the base from which log
	/// files, system files, etc, can be located.  For instance, for CDSS TSTool
	/// the application home is set to C:\CDSS\TSTool-Version.  Other directories under this include "system" and "logs". </summary>
	/// <param name="homeDir"> the home directory to set. </param>
	public static void setApplicationHomeDir(string homeDir)
	{
		if (!_initialized)
		{
			initialize();
		}

		if (!string.ReferenceEquals(homeDir, null))
		{
			homeDir = homeDir.Trim();
			// Remove the trailing directory separator
			if (homeDir.EndsWith(File.separator, StringComparison.Ordinal))
			{
				homeDir = homeDir.Substring(0, (homeDir.Length - 1));
			}

			// for windows-based machines:
			if (File.separator.Equals("\\"))
			{
				// on dos
				if (homeDir.StartsWith("\\\\", StringComparison.Ordinal))
				{
					// UNC path -- leave as is
				}
				else if (homeDir[1] != ':')
				{
					// homeDir does not start with a drive letter.  Get the drive letter of the current
					// working dir and use it instead.  Since working dir is initialized to the java 
					// working dir when IOUtil is first used, _working_dir will always have a drive letter
					// for windows machines.
					char drive = _working_dir[0];
					homeDir = drive + ":" + homeDir;
				}
			}
			__homeDir = homeDir;
		}
	}

	/// <summary>
	/// Set the program arguments.  This is generally only called from low-level code
	/// (normally just need to call setProgramData()).  A copy is saved. </summary>
	/// <param name="argv"> Program arguments. </param>
	/// <seealso cref= #setProgramData </seealso>
	public static void setProgramArguments(string[] argv)
	{
		if (!_initialized)
		{
			initialize();
		}

		if (argv == null)
		{
			// No arguments - initialize to avoid null pointer exceptions...
			_argv = new string[0];
			return;
		}

		// Create a copy of the command-line arguments
		int length = argv.Length;
		if (length > 0)
		{
			_argv = new string[length];
			for (int i = 0; i < length; i++)
			{
				_argv[i] = argv[i];
			}
		}
	}

	/// <summary>
	/// Set the program command file.  This is generally only called from low-level
	/// command parsing code (e.g. RTi.HydroSolutions.DSSApp.parseCommands()). </summary>
	/// <param name="command_file"> Command file to use with the program. </param>
	/// <seealso cref= #getProgramCommandFile </seealso>
	/// @deprecated utilize command processors to get comments suitable for files 
	public static void setProgramCommandFile(string command_file)
	{
		if (!_initialized)
		{
			initialize();
		}
		if (!string.ReferenceEquals(command_file, null))
		{
			_command_file = command_file;
		}
	}

	/// <summary>
	/// Set the program main data, which can be used later for GUI labels, etc.  This
	/// is generally called from the main() or init() function of an application (or from application base classes). </summary>
	/// <param name="progname"> The program name. </param>
	/// <param name="progver"> The program version. </param>
	/// <param name="argv"> The program command-line arguments (ignored if an Applet). </param>
	/// <seealso cref= #getProgramName </seealso>
	/// <seealso cref= #getProgramVersion </seealso>
	/// <seealso cref= #getProgramArguments </seealso>
	public static void setProgramData(string progname, string progver, string[] argv)
	{
		if (!_initialized)
		{
			initialize();
		}
		setProgramName(progname);
		setProgramVersion(progver);
		setProgramArguments(argv);
	}

	/// <summary>
	/// Set the host name on which the program is running. </summary>
	/// <param name="host"> The host (machine) name. </param>
	/// <seealso cref= #getProgramHost </seealso>
	public static void setProgramHost(string host)
	{
		if (!_initialized)
		{
			initialize();
		}
		if (!string.ReferenceEquals(host, null))
		{
			_host = host;
		}
	}

	/// <summary>
	/// Set the program name. </summary>
	/// <param name="progname"> The program name. </param>
	/// <seealso cref= #getProgramName </seealso>
	public static void setProgramName(string progname)
	{
		if (!_initialized)
		{
			initialize();
		}
		if (!string.ReferenceEquals(progname, null))
		{
			_progname = progname;
		}
	}

	/// <summary>
	/// Set the program user.  This is usually called from within IO by checking system properties. </summary>
	/// <param name="user"> The user name. </param>
	/// <seealso cref= #getProgramUser </seealso>
	public static void setProgramUser(string user)
	{
		if (!_initialized)
		{
			initialize();
		}
		if (!string.ReferenceEquals(user, null))
		{
			_user = user;
		}
	}

	/// <summary>
	/// Set the program version. </summary>
	/// <param name="progver"> The program version. </param>
	/// <seealso cref= #getProgramVersion </seealso>
	public static void setProgramVersion(string progver)
	{
		if (!_initialized)
		{
			initialize();
		}
		if (!string.ReferenceEquals(progver, null))
		{
			_progver = progver;
		}
	}

	/// <summary>
	/// Set the program working directory.  It does not cause a directory change.
	/// This method may be called, for example, when a GUI program applies an artificial
	/// directory change.  Java does not allow a change in the working directory but
	/// by setting here the application is indicating that relative paths should be
	/// relative to this directory.  The value of the working directory should be an
	/// absolute path if from a GUI to ensure that the correct absolute path to files
	/// can be determined.  The default working directory is the directory in which the
	/// application started.  This is often reset soon by an application to indicate a
	/// "home" directory where work occurs. </summary>
	/// <param name="working_dir"> The program working directory.  The trailing directory
	/// delimiter will be removed if specified.  Currently, working_dir must be an
	/// absolute path (e.g., as taken from a file chooser).  If not, the given directory
	/// is prepended with the previous drive letter if a Windows machine.  In the
	/// future, a relative path (e.g., "..\xxxx") may be allowed, in which case, the
	/// previous working directory will be adjusted. </param>
	/// <seealso cref= #getProgramWorkingDir </seealso>
	public static void setProgramWorkingDir(string working_dir)
	{
		if (!_initialized)
		{
			initialize();
		}
		if (!string.ReferenceEquals(working_dir, null))
		{
			working_dir = working_dir.Trim();
			if (working_dir.EndsWith(File.separator, StringComparison.Ordinal))
			{
				working_dir = working_dir.Substring(0,(working_dir.Length - 1));
			}

			// for windows-based machines:
			if (File.separator.Equals("\\"))
			{
				// on dos
				if (working_dir.StartsWith("\\\\", StringComparison.Ordinal))
				{
					// UNC drive -- leave as is
				}
				else if (working_dir[1] != ':')
				{
					// working_dir does not start with a drive letter.  Get the drive letter of the current
					// working dir and use it instead.  Since working dir is initialized to the java 
					// working dir when IOUtil is first used, _working_dir will always have a drive letter
					// for windows machines.
					char drive = _working_dir[0];
					working_dir = drive + ":" + working_dir;
				}
			}
			_working_dir = working_dir;
		}
	}

	/// <summary>
	/// Set a property in the global PropListManager.  This sets the value in the un-named PropList.
	/// </summary>
	public static void setProp(string key, object prop)
	{
		if (!_initialized)
		{
			initialize();
		}
		// Set in the first list...
		_prop_list_manager.setValue("", key, prop);
	}

	/// <summary>
	/// Sets whether the program is running as an applet.
	/// </summary>
	public static void setRunningApplet(bool applet)
	{
		if (!_initialized)
		{
			initialize();
		}
		__runningApplet = applet;
	}

	/// <summary>
	/// Determine a unique temporary file name, using the system clock.  On UNIX, temporary files are created
	/// in /tmp.  On PCs, temporary files are created in C:/TEMP.  The file may theoretically be grabbed by another
	/// application but this is unlikely.
	/// If using Java 1.2x, can use the File.createTempFile() method instead. </summary>
	/// <returns> Full path to an unused temporary file. </returns>
	public static string tempFileName()
	{
		return tempFileName(null, null);
	}

	/// <summary>
	/// Determine a unique temporary file name, using the system clock milliseconds.
	/// The system property java.io.tmpdir is used to determine the folder.
	/// The file may theoretically be grabbed by another
	/// application but this is unlikely since the filename is based on the current time </summary>
	/// <param name="prefix"> prefix to filename, in addition to temporary pattern, or null for no prefix. </param>
	/// <param name="extension"> extension to filename (without leading .), or null for no extension. </param>
	/// <returns> Full path to an unused temporary file. </returns>
	public static string tempFileName(string prefix, string extension)
	{ // Get the prefix...
		string dir = null;
		if (string.ReferenceEquals(prefix, null))
		{
			prefix = "";
		}
		if (string.ReferenceEquals(extension, null))
		{
			extension = "";
		}
		else if (!extension.StartsWith(".", StringComparison.Ordinal))
		{
			extension = "." + extension;
		}
		string[] tmpdirs = null;
		if (isUNIXMachine())
		{
			tmpdirs = new string[3];
			tmpdirs[0] = System.getProperty("java.io.tmpdir");
			tmpdirs[1] = "/tmp";
			tmpdirs[2] = "/var/tmp";
		}
		else
		{
			tmpdirs = new string[3];
			tmpdirs[0] = System.getProperty("java.io.tmpdir");
			tmpdirs[1] = "C:\\tmp";
			tmpdirs[2] = "C:\\temp";
		}
		for (int i = 0; i < tmpdirs.Length; i++)
		{
			string dir2 = tmpdirs[i];
			File f = new File(dir2);
			if (f.exists() && f.isDirectory())
			{
				// Found a folder that will work
				dir = dir2;
				break;
			}
		}
		if (string.ReferenceEquals(dir, null))
		{
			// Should hopefully never happen
			throw new Exception("Cannot determine temporary file location.");
		}
		// Use the date as a seed and make sure the file does not exist...
		string filename = null;
		while (true)
		{
			System.DateTime d = System.DateTime.Now;
			if (dir.EndsWith(File.separator, StringComparison.Ordinal))
			{
				filename = dir + prefix + d.Ticks + extension;
			}
			else
			{
				filename = dir + File.separator + prefix + d.Ticks + extension;
			}
			if (!fileExists(filename))
			{
				break;
			}
		}
		File finalName = new File(filename);
		try
		{
			// Do this to unmangle Windows paths
			return finalName.getCanonicalPath();
		}
		catch (IOException)
		{
			// Just go with mangled name.
			return filename;
		}
	}

	/// <summary>
	/// Set whether the application is being run in test mode.  The testing()
	/// method can be called to check the value.  An appropriate way to use this
	/// functionality is to check for a -test command line argument.  If present, call
	/// IOUtil.testing(true).  Later, check the value with IOUtil.testing().  This is
	/// useful for adding GUI features or expanded debugging only for certain parts of the code that are being tested. </summary>
	/// <param name="is_testing"> true if the application is being run in test mode (default initial value is false). </param>
	/// <returns> the value of the testing flag, after being set. </returns>
	public static bool testing(bool is_testing)
	{
		_testing = is_testing;
		return _testing;
	}

	/// <summary>
	/// Determine whether the application is being run in test mode.  See overloaded method for more information. </summary>
	/// <returns> the value of the testing flag, after being set. </returns>
	public static bool testing()
	{
		return _testing;
	}

	/// <summary>
	/// Convert a path and an absolute directory to an absolute path. </summary>
	/// <param name="dir"> Directory to prepend to path. </param>
	/// <param name="path"> Path to append to dir to create an absolute path.  If absolute, it
	/// will be returned.  If relative, it will be appended to dir.  If the path
	/// includes "..", the directory will be truncated before appending the non-".." part of the path. </param>
	public static string toAbsolutePath(string dir, string path)
	{
		File f = new File(path);
		if (f.isAbsolute())
		{
			return path;
		}
		// Loop through the "path".  For each occurrence of "..", knock a directory off the end of the "dir"...

		// Always trim any trailing directory separators off the directory paths
		while (dir.Length > 1 && dir.EndsWith(File.separator, StringComparison.Ordinal))
		{
			dir = dir.Substring(0, dir.Length - 1);
		}
		while (path.Length > 1 && path.EndsWith(File.separator, StringComparison.Ordinal))
		{
			path = path.Substring(0, path.Length - 1);
		}

		int path_length = path.Length;
		string sep = File.separator;
		int pos;
		for (int i = 0; i < path_length; i++)
		{
			if (path.StartsWith("./", StringComparison.Ordinal) || path.StartsWith(".\\", StringComparison.Ordinal))
			{
				// No need for this in the result...
				// Adjust the path and evaluate again...
				path = path.Substring(2);
				i = -1;
				path_length -= 2;
			}
			if (path.StartsWith("../", StringComparison.Ordinal) || path.StartsWith("..\\", StringComparison.Ordinal))
			{
				// Remove a directory from each path...
				pos = dir.LastIndexOf(sep, StringComparison.Ordinal);
				if (pos >= 0)
				{
					// This will remove the separator...
					dir = dir.Substring(0,pos);
				}
				// Adjust the path and evaluate again...
				path = path.Substring(3);
				i = -1;
				path_length -= 3;
			}
			else if (path.Equals(".."))
			{
				// remove a directory from each path
				pos = dir.LastIndexOf(sep, StringComparison.Ordinal);
				if (pos >= 0)
				{
					dir = dir.Substring(0, pos);
				}
				// adjust the path and evaluate again
				path = path.Substring(2);
				i = -1;
				path_length -= 2;
			}
		}

		return dir + File.separator + path;
	}

	/// <summary>
	/// Convert a path "path" and an absolute directory "dir" to a relative path.
	/// If "dir" is at the start of "path" it is removed.  If it is not present, an
	/// exception is thrown.  For example, a "dir" of \a\b\c\d\e and a "path" of
	/// \a\b\c\x\y will result in ..\..\x\y.<para>
	/// The strings passed in to this method should not end with a file separator 
	/// (either "\" or "/", depending on the system).  If they have a file separator,
	/// the separator will be trimmed off the end.
	/// <br>
	/// There are four conditions for which to check:<ul>
	/// <li>The directories are exactly the same ("\a\b\c" and "\a\b\c")</li>
	/// <li>The second directory is farther down the same branch that the first
	/// directory is on ("\a\b\c\d\e" and "\a\b\c").<li>
	/// <li>The second directory requires a backtracking up the branch on which the
	/// first directory is on ("\a\b\c\d" and "\a\b\c\e" or "\a\b\c\d" and "\g")</li>
	/// <li>For DOS: the directories are on different drives.</li>
	/// <br>
	/// This method will do error checking to make sure the directories passed in
	/// to it are not null or empty, but apart from that does no error-checking to 
	/// validate proper directory naming structure.  This method will fail with
	/// improper directory names (e.g., "C:\\c:\\\\\\\\test\\\\").
	/// </para>
	/// </summary>
	/// <param name="rootDir"> the root directory from which to build a relative directory. </param>
	/// <param name="relDir"> the directory for which to create the relative directory path from the rootDir. </param>
	/// <returns> the relative path created from the two directory structures.  This
	/// path will NOT have a trailing directory separator (\ or /).  If both the
	/// rootDir and relDir are the same, for instance, the value "." will be returned.
	/// Plus the directory separator, this becomes ".\" or "./". </returns>
	/// <exception cref="Exception"> if the conversion cannot occur.  Most likely will occur
	/// in DOS when the two directories are on different drives.  Will also be thrown
	/// if null or empty strings are passed in as directories. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static String toRelativePath(String rootDir, String relDir) throws Exception
	public static string toRelativePath(string rootDir, string relDir)
	{
		// do some simple error checking
		if (string.ReferenceEquals(rootDir, null) || rootDir.Trim().Equals(""))
		{
			throw new Exception("Bad rootDir (" + rootDir + ") passed in to IOUtil.toRelativePath()");
		}
		if (string.ReferenceEquals(relDir, null) || relDir.Trim().Equals(""))
		{
			throw new Exception("Bad relDir (" + relDir + ") passed in to IOUtil.toRelativePath()");
		}

		string sep = File.separator;

		bool unix = true;

		if (sep.Equals("\\"))
		{
			unix = false;
			// this is running on DOS.  Check to see if the drive letters
			// are the same for each directory -- if they aren't, the
			// second directory can't be converted to a relative directory.
			char drive1 = rootDir.ToLower()[0];
			char drive2 = relDir.ToLower()[0];

			if (drive1 != drive2)
			{
				throw new Exception("Cannot adjust \"" + relDir + " to relative using directory \"" + rootDir + "\"");
			}
		}

		// Always trim any trailing directory separators off the directory paths
		while (rootDir.Length > 1 && rootDir.EndsWith(File.separator, StringComparison.Ordinal))
		{
			rootDir = rootDir.Substring(0, rootDir.Length - 1);
		}
		while (relDir.Length > 1 && relDir.EndsWith(File.separator, StringComparison.Ordinal))
		{
			relDir = relDir.Substring(0, relDir.Length - 1);
		}

		// Check to see if the two paths are the same
		if ((unix && rootDir.Equals(relDir)) || (!unix && rootDir.Equals(relDir, StringComparison.OrdinalIgnoreCase)))
		{
			return ".";
		}

		// Check to see if the relDir dir is farther up the same branch that the rootDir is on.

		if ((unix && relDir.StartsWith(rootDir, StringComparison.Ordinal)) || (!unix && StringUtil.startsWithIgnoreCase(relDir, rootDir)))
		{

			// At this point, it is known that relDir is longer than rootDir
			string c = "" + relDir[rootDir.Length];

			if (c.Equals(File.separator))
			{
				string higher = relDir.Substring(rootDir.Length);
				if (higher.StartsWith(sep, StringComparison.Ordinal))
				{
					higher = higher.Substring(1);
				}
				return higher;
			}
		}

		// if none of the above were triggered, then the second directory
		// is higher up the first directory's directory branch

		// get the final directory separator from the first directory, and
		// then start working backwards in the string to find where the
		// second directory and the first directory share directory information
		int start = rootDir.LastIndexOf(sep, StringComparison.Ordinal);
		int x = 0;
		for (int i = start; i >= 0; i--)
		{
			string s = rootDir[i].ToString();

			if (!s.Equals(sep))
			{
				// do nothing this iteration
			}
			else if ((unix && relDir.regionMatches(false, 0, rootDir + sep, 0, i + 1)) || (!unix && relDir.regionMatches(true,0,rootDir + sep, 0, i + 1)))
			{
				// A common "header" in the directory name has been found.  Count the number of separators in each
				// directory to determine how much separation lies between the two
				int dir1seps = StringUtil.patternCount(rootDir.Substring(0, i), sep);
				int dir2seps = StringUtil.patternCount(rootDir, sep);
				x = i + 1;
				if (x > relDir.Length)
				{
					x = relDir.Length;
				}
				string uncommon = relDir.Substring(x, relDir.Length - x);
				int steps = dir2seps - dir1seps;
				if (steps == 1)
				{
					if (uncommon.Trim().Equals(""))
					{
						return "..";
					}
					else
					{
						return ".." + sep + uncommon;
					}
				}
				else
				{
					if (uncommon.Trim().Equals(""))
					{
						uncommon = "..";
					}
					else
					{
						uncommon = ".." + sep + uncommon;
					}
					for (int j = 1; j < steps; j++)
					{
						uncommon = ".." + sep + uncommon;
					}
				}
				return uncommon;
			}
		}

		return relDir;
	}

	/// <summary>
	/// Verify that a path is appropriate for the operating system.
	/// This is a simple method that does the following:
	/// <ol>
	/// <li>    If on UNIX/LINUX, replace all "\" characters with "/".  WARNING - as implemented,
	///        this will convert UNC paths to forward slashes.</li>
	/// <li>    If on Windows, do nothing.  Java automatically handles "/" in paths.</li>
	/// </ol> </summary>
	/// <returns> A path to the file that uses separators appropriate for the operating system. </returns>
	public static string verifyPathForOS(string path)
	{
		return verifyPathForOS(path, false);
	}

	/// <summary>
	/// Verify that a path is appropriate for the operating system.
	/// This is a simple method that does the following:
	/// <ol>
	/// <li>    If on UNIX/LINUX, replace all "\" characters with "/".  WARNING - as implemented,
	///        this will convert UNC paths to forward slashes.</li>
	/// <li>    If on Windows, do nothing (unless force=true).  Java automatically handles "/" in paths.</li>
	/// </ol> </summary>
	/// <param name="force"> always do the conversion (on Windows this will always convert // to \ - this should probably be
	/// the default behavior but make it an option since this has not always been the behavior of this method (see overload). </param>
	/// <returns> A path to the file that uses separators appropriate for the operating system. </returns>
	public static string verifyPathForOS(string path, bool force)
	{
		if (string.ReferenceEquals(path, null))
		{
			return path;
		}
		if (isUNIXMachine())
		{
			return (path.Replace('\\', '/'));
		}
		else
		{
			if (force)
			{
				// Even on windows force it although it does not seem to be necessary in most cases
				return (path.Replace('/', '\\'));
			}
			else
			{
				// Just return... paths on Windows can have / or \ and still work
				return path;
			}
		}
	}

	/// <summary>
	/// Write a file. </summary>
	/// <param name="filename"> Name of file to write. </param>
	/// <param name="contents"> Contents to write to file.  It is assumed that the contents
	/// contains line break characters. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeFile(String filename, String contents) throws java.io.IOException
	public static void writeFile(string filename, string contents)
	{
		string message, routine = "IOUtil.writeFile";

		StreamWriter fp = null;
		try
		{
			fp = new StreamWriter(filename);
			fp.Write(contents);
			fp.Close();
		}
		catch (Exception)
		{
			message = "Unable to open file \"" + filename + "\"";
			Message.printWarning(2, routine, message);
			throw new IOException(message);
		}
	}

	}

}