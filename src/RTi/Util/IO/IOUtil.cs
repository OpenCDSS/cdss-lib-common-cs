using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
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
    using Message = Message.Message;
    using StringUtil = String.StringUtil;

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
    /// 
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
        //private static Applet _applet = null;
        /// <summary>
        /// Applet context.  Call setAppletContext() from init() of an application that uses this class.
        /// </summary>
        //private static AppletContext _applet_context = null;
        /// <summary>
        /// Document base for the applet.
        /// </summary>
        //private static URL _document_base = null;
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
        /// Determine if a file/directory exists. </summary>
        /// <returns> true if the file/directory exists, false if not. </returns>
        /// <param name="filename"> String path to the file/directory to check. </param>
        public static bool fileExists(string filename)
        {
            if (string.ReferenceEquals(filename, null))
            {
                return false;
            }
            FileInfo file = new FileInfo(filename);
            bool exists = file.Exists;
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
            StreamReader st = null;
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

            DateTime now = DateTime.Now;

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
            comments.Add(commentLinePrefix2 + "date:         " + now.ToString());
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
                try
                {
                    cfp = new StreamReader(_command_file);
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
                    //revision_start = @string.regionMatches(true, 0, (string)commentIndicators[i], 0, ((string)commentIndicators[i]).Length);
                    //TODO @jurentie 04/05/2019 - Check to see if this is working....
                    revision_start = (@string.IndexOf((string)commentIndicators[i], 0, ((string)commentIndicators[i]).Length) == 0);
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
                    header_revision = int.Parse(revision_string);
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
                            ignore_substring = @string.Substring(0, len);
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
        /// Open an input stream given a URL or regular file name. </summary>
        /// <returns> An InputStream given a URL or file name.  If the string starts with
        /// "http:", "ftp:", or "file:", a URL is created and the associated stream is
        /// returned.  Otherwise, a file is opened and the associated stream is returned. </returns>
        /// <param name="url_string"> </param>
        /// <exception cref="IOException"> if the input stream cannot be initialized. </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public static java.io.InputStream getInputStream(String url_string) throws java.io.IOException
        public static StreamReader getInputStream(string url_string)
        {
            string routine = "IOUtil.getInputStream";
            StreamReader stream;
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

            if (string.Compare(url_string, 0, "http:", 0, 5, StringComparison.CurrentCultureIgnoreCase) == 0 ||
                string.Compare(url_string, 0, "file:", 0, 5, StringComparison.CurrentCultureIgnoreCase) == 0 ||
                string.Compare(url_string, 0, "ftp:", 0, 4, StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                try
                {
                    stream = new StreamReader(WebRequest.Create(url_string).GetResponse().GetResponseStream());
                    return stream;
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
                    return (new StreamReader(fileStream));
                }
                catch (Exception)
                {
                    Message.printWarning(10, routine, noIndex);
                    throw new IOException(noIndex);
                }
            }
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
                        fullPath = (Path.GetFullPath(_working_dir + "/" + path).ToString());
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
                        fullPath = (Path.GetFullPath(_working_dir + "/" + path).ToString());
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
            catch (Exception e)
            {
                // Probably a security exception...
                return null;
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
                    //_host = InetAddress.getLocalHost().getHostName();
                    _progname = "program name unknown";
                    _progver = "version unknown";
                    //_user = System.getProperty("user.name");
                    //_working_dir = System.getProperty("user.dir");
                    //__homeDir = System.getProperty("user.dir");
                }
            }
            catch (Exception e)
            {
                // Don't do anything.  Just print a warning...
                Message.printWarning(3, routine, "Caught an exception initializing IOUtil (" + e + ").  Continuing.");
            }

            // Initialize the applet context...

            //_applet_context = null;

            // Initialize the property list manager to contain an unnamed list...

            _prop_list_manager = new PropListManager();
            _prop_list_manager.addList(new PropList("", PropList.FORMAT_PROPERTIES), true);

            // Set the flag to know that the class has been initialized...

            _initialized = true;
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
            return Path.IsPathRooted(path);
        }

        /**
        Determine if a UNIX machine.  The following seem to be standard, although there are variations depending
        on hardware and OS version:
        <pre>
        Operating System      os.arch    os.name          os.version
        Mac OS X              x86_64     "Mac OS X"       10.6.4
        Windows 95            x86        "Windows 95"     4.0
        Windows 98            x86        "Windows 95"     4.1
        NT                    x86        "Windows NT"     4.0
        Windows 2000          x86        "Windows NT"     5.0
        Linux                 i386       "Linux"
        HP-UX                 PA-RISC
        </pre>
        @return true if a UNIX platform, including os.name of Linux, false if not (presumably Windows).
        */
        //TODO @jurenite 03/25/2019 - Not sure if the function work properly...
        public static bool isUNIXMachine()
        {
            int platform = (int)Environment.OSVersion.Platform;
            if ((platform == 4) || (platform == 6) || (platform == 128))
            {
                return true;
            }
            return false;
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
        public static int printCreatorHeader(TextWriter ofp, string comment0, int maxwidth, int flag)
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
        public static int printCreatorHeader(TextWriter ofp, string commentLinePrefix, int maxwidth, int flag, PropList props)
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
                ofp.WriteLine(c);
            }
            ofp.Flush();
            return 0;
        }

        /// @deprecated Use the overloaded version that uses List parameters. 
        /// <param name="oldFile"> </param>
        /// <param name="newFile"> </param>
        /// <param name="newComments"> </param>
        /// <param name="commentIndicators"> </param>
        /// <param name="ignoredCommentIndicators"> </param>
        /// <param name="flags"> </param>
        /// <returns> PrintWriter to allow additional writing to the file. </returns>
        public static TextWriter processFileHeaders(string oldFile, string newFile, string[] newComments, string[] commentIndicators, string[] ignoredCommentIndicators, int flags)
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
        public static TextWriter processFileHeaders(string oldFile, string newFile, IList<string> newComments, IList<string> commentIndicators, IList<string> ignoredCommentIndicators, int flags)
        {
            Debug.WriteLine("hello world!");
            string comment;
            string routine = "IOUtil.processFileHeaders";
            FileHeader oldheader;
            TextWriter ofp = null;
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
                ofp = new StreamWriter(newFile);
                if (newFile.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
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
                ofp.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                ofp.WriteLine("<!--");
            }
            ofp.WriteLine(comment + HEADER_REVISION_STRING + " " + header_revision);
            ofp.WriteLine(comment);

            // Now print the standard header...

            PropList props = new PropList("header");
            if (is_xml)
            {
                props.set("IsXML=true");
            }
            Debug.WriteLine("here5");
            printCreatorHeader(ofp, comment, 80, 0, props);

            // Now print essential comments for this revision.  These strings do not have the comment prefix....

            if (newComments != null)
            {
                if (newComments.Count > 0)
                {
                    if (is_xml)
                    {
                        ofp.WriteLine(comment);
                    }
                    else
                    {
                        ofp.WriteLine(comment + "----");
                    }
                    for (i = 0; i < newComments.Count; i++)
                    {
                        ofp.WriteLine(comment + " " + newComments[i]);
                    }
                }
            }

            if (is_xml)
            {
                ofp.WriteLine(comment);
            }
            else
            {
                ofp.WriteLine(comment + "------------------------------------------------");
            }

            // Now print the old header.  It already has the comment character.

            if (oldheader != null)
            {
                if (oldheader.size() > 0)
                {
                    for (i = 0; i < oldheader.size(); i++)
                    {
                        ofp.WriteLine(oldheader.elementAt(i));
                    }
                }
            }

            if (is_xml)
            {
                ofp.WriteLine("-->");
            }
            return ofp;
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
                if (working_dir.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                {
                    working_dir = working_dir.Substring(0, (working_dir.Length - 1));
                }

                // for windows-based machines:
                if (Path.DirectorySeparatorChar.ToString().Equals("\\"))
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
        /// Convert a path and an absolute directory to an absolute path. </summary>
        /// <param name="dir"> Directory to prepend to path. </param>
        /// <param name="path"> Path to append to dir to create an absolute path.  If absolute, it
        /// will be returned.  If relative, it will be appended to dir.  If the path
        /// includes "..", the directory will be truncated before appending the non-".." part of the path. </param>
        public static string toAbsolutePath(string dir, string path)
        {
            FileInfo f = new FileInfo(path);
            if (Path.IsPathRooted(path))
            {
                return path;
            }
            // Loop through the "path".  For each occurrence of "..", knock a directory off the end of the "dir"...

            // Always trim any trailing directory separators off the directory paths
            while (dir.Length > 1 && dir.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                dir = dir.Substring(0, dir.Length - 1);
            }
            while (path.Length > 1 && path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                path = path.Substring(0, path.Length - 1);
            }

            int path_length = path.Length;
            string sep = Path.DirectorySeparatorChar.ToString();
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
                        dir = dir.Substring(0, pos);
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

            return dir + Path.DirectorySeparatorChar.ToString() + path;
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
    }
}
