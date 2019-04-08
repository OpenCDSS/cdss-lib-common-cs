using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// PropList - use to hold a list of properties

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
// PropList - use to hold a list of properties
// ----------------------------------------------------------------------------
// Copyright:  See the COPYRIGHT file.
// ----------------------------------------------------------------------------
// History:
//
// Sep 1997?	Steven A. Malers,	Initial version.
//		Riverside Technology,
//		inc.
// 02 Feb 1998	SAM, RTi		Get all of the Prop* classes working
//					together.
// 24 Feb 1998	SAM, RTi		Add the javadoc comments.
// 02 May 1998	SAM, RTi		Add the getValid function.
// 13 Apr 1999	SAM, RTi		Add finalize.
// 17 May 1999	SAM, RTi		Add setUsingObject to avoid overload
//					conflict.
// 06 Nov 2000	CEN, RTi		Added read/writePersistent 
//  					implementation similar to C++ implem.
//					Included adding clear
// 14 Jan 2001	SAM, RTi		Overload set to take a Prop.
// 13 Feb 2001	SAM, RTi		Change readPersistent() and associated
//					methods return void as per C++.  Fix
//					bug where readPersistent() was not
//					handling whitespace correctly.  Add
//					javadoc to readPersistent().  Add
//					getValue(key,inst), getProp(key,inst),
//					findProp(key,inst) to store multiple
//					instances of properties with the same
//					key.
// 27 Apr 2001	SAM, RTi		Change all debug levels to 100.
// 10 May 2001	SAM, RTi		Testing to get working with embedded
//					variables like ${...}.  This involves
//					passing the persistent format to the
//					Prop.getValue() method so that it can
//					decide whether to expand the contents.
//					Move routine names into messages
//					themselves to limit overhead.  Set
//					unused variables to null to optimize
//					memory management.  Change initial
//					list size from 100 to 20.
// 14 May 2001	SAM, RTi		Change so that when parsing properties
//					the = is the only delimiter so that
//					quotes around arguments with spaces
//					are not needed.
// 2001-11-08	SAM, RTi		Synchronize with UNIX.  Changes from
//					2001-05-14... Add a boolean flag
//					_literal_quotes to keep the quotes in
//					the PropList.  This is useful where
//					commands are saved in PropLists.
//					Change so when reading a persistent
//					file, the file can be a regular file or
//					a URL.
// 2002-01-20	SAM, RTi		Fix one case where equals() was being
//					used instead of equalsIgnoreCase() when
//					finding property names.
// 2002-02-03	SAM, RTi		Add setHowSet() and getHowSet() to track
//					how a property is set.  Remove the
//					*_CONFIG static parameters because
//					similar values are found in Prop.  The
//					values were never used.  Change set
//					methods to be void instead of having a
//					return type.  The return value is never
//					used.  Fix bug where setValue() was
//					replacing the Prop in the PropList with
//					the given object (rather than the value
//					in the Prop) - not sure if this code
//					was ever getting called!  Change
//					readPersistent() and writePersistent()
//					to throw an IOException if there is an
//					error.  Add getPropsMatchingRegExp(),
//					which is used by TSProduct to help write
//					properties.
// 2002-07-01	SAM, RTi		Add elementAt() to get a property at
//					a position.
// 2002-12-24	SAM, RTi		Support /* */ comments in Java PropList.
// 2003-03-27	SAM, RTi		Fix bugs in setContents() and setValue()
//					methods where when a match was found
//					the code did not return, resulting in
//					a new duplicate property also being
//					appended.
// 2003-10-27	J. Thomas Sapienza, RTi	Added a very basic copy constructor.
//					In the future should implement
//					clone() and a copy constructor that
//					can handle Props that have data objects.
// 2003-11-11	JTS, RTi		* Added getPropCount().
//					* Added the methods with the replace
//					  parameter.
//					* Added unSetAll().
// 2004-02-03	SAM, RTi		* Add parse().
// 2004-07-15	JTS, RTi		Added sortList().
// 2004-11-29	JTS, RTi		Added getList().
// 2005-04-29	SAM, RTi		* Overload the parse method to take a
//					  "how set" value.
// 2005-06-09	JTS, RTi		Warnings in readPersistent() are now 
//					printed at level 2.
// 2005-12-06	JTS, RTi		Added validatePropNames().
// 2007-03-02	SAM, RTi		Update setUsingObject() to allow null.
// ---------------------------------------------------------------------------- 
// EndHeader

namespace RTi.Util.IO
{
    using Message = Message.Message;

    /// <summary>
	/// This class manages a list of Prop instances, effectively creating a properties
	/// list that can be used to store properties for an object, etc.  This class
	/// contains a list of Prop instances and methods to interface with the data (set
	/// properties, look up properties, etc.).  Property lists are typically used to
	/// store and pass variable length, variable content data, as opposed to fixed
	/// parameters.  Often, only a PropList needs to be used (and Prop, PropListManager)
	/// can be avoided.  Note that the standard Java Hashtable can also be used for
	/// properties but does not have some of the features of PropList.
	/// <para>
	/// 
	/// Often, a PropList will contain only simple string properties.  However, it is
	/// possible to store any Object in a PropList, keyed by a name.  Internally, each
	/// property has a String key, a String value, an Object contents, and an integer
	/// flag indicating how the property was set (from file, by user, etc).  For simple
	/// strings, the value and contents are the same.  For other Objects, the contents
	/// evaluates to toString(); however, applications will often use the contents
	/// directly by casting after retrieving from the PropList.
	/// </para>
	/// <para>
	/// 
	/// An additional feature of PropList is the use of variables in strings.  If a
	/// PropList is created using a persistent format of FORMAT_NWSRFS or
	/// FORMAT_MAKEFILE, variables are encoded using $(varname).  If FORMAT_PROPERTIES,
	/// the notation is ${varname}.  For example, two properties may be defined as:
	/// </para>
	/// <para>
	/// <pre>
	/// prop1 = "Hello World"
	/// title = "My name is ${prop1}
	/// </pre>
	/// 
	/// The default persistent format does not support this behavior, but using the
	/// formats described above automatically supports this behavior.  Additionally,
	/// the IOUtil property methods (setProp(), getProp*()) are used to check for
	/// properties.  Therefore, you can use IOUtil to define properties one place in an
	/// application and use the properties in a different part of the application.
	/// </para>
	/// <para>
	/// 
	/// Each of the special formats will also make an external call to a program to fill
	/// in information if the following syntax is used:
	/// </para>
	/// <para>
	/// <pre>
	/// prop1 = "Current time: `date`"
	/// </pre>
	/// 
	/// The back-quotes cause a system call using ProcessManager and the output is
	/// placed in the resulting variable.
	/// </para>
	/// <para>
	/// If the properties are edited at run-time (e.g., by a graphical user interface),
	/// it is common to use the "how set" flag to control how the properties file is
	/// written.  For example:
	/// <pre>
	/// PropList props = new PropList ( "" );
	/// props.setPersistentName ( "somefile" );
	/// // The following uses a "how set" value of Prop.SET_FROM_PERSISTENT.
	/// props.readPersistent ( "somefile" );
	/// // Next, the application may check the file properties and assign some internal
	/// // defaults to have a full set of properties...
	/// props.setHowSet ( Prop.SET_AS_RUNTIME_DEFAULT );
	/// // When a user interface is displayed...
	/// props.setHowSet ( Prop.SET_AT_RUNTIME_BY_USER );
	/// // ...User interaction...
	/// props.setHowSet ( Prop.SET_UNKNOWN );
	/// // Then there is usually custom code to write a specific PropList to a file.
	/// // Only properties that were originally read or have been modified by the user
	/// // may be written (internal defaults often make the property list verbose, but
	/// // may be still desirable).
	/// </pre>
	/// </para>
	/// </summary>
	/// <seealso cref= Prop </seealso>
	/// <seealso cref= PropListManager </seealso>
	/// <seealso cref= ProcessManager </seealso>

    public class PropList
    {

        /// <summary>
        /// Indicates that the configuration file format is unknown.
        /// </summary>
        public const int FORMAT_UNKNOWN = 0;

        /// <summary>
        /// Indicates that the configuration file format is that used by Makefiles.
        /// </summary>
        public const int FORMAT_MAKEFILE = 1;

        /// <summary>
        /// Indicates that the configuration file format is that used by NWSRFS
        /// configuration files (apps_defaults).  A : is used instead of the = for assignment.
        /// </summary>
        public const int FORMAT_NWSRFS = 2;

        /// <summary>
        /// Indicates that configuration information is being stored in a custom database.
        /// </summary>
        public const int FORMAT_CUSTOM_DB = 3;

        /// <summary>
        /// Indicates that configuration information is being stored in standard RTi properties file.
        /// </summary>
        public const int FORMAT_PROPERTIES = 4;

        /// <summary>
        /// Name of this PropList.
        /// </summary>
        private string __listName;
        /// <summary>
        /// List of Prop.
        /// </summary>
        private IList<Prop> __list;
        /// <summary>
        /// File to save in.
        /// </summary>
        private string __persistentName;
        /// <summary>
        /// Format of file to read.
        /// </summary>
        private int __persistentFormat;
        /// <summary>
        /// Last line read from the property file.
        /// </summary>
        private int __lastLineNumberRead;
        /// <summary>
        /// Indicates if quotes should be treated literally when setting Prop values.
        /// </summary>
        private bool __literalQuotes = true;
        /// <summary>
        /// The "how set" value to use when properties are being set.
        /// </summary>
        private int __howSet = Prop.SET_UNKNOWN;

        /// <summary>
        /// Copy constructor for a PropList.  Currently only works with PropLists that store String key/value pairs.<para>
        /// TODO (JTS - 2003-10-27) Later, add in support for cloning of props that have data objects in the value section.
        /// </para>
        /// </summary>
        /// <param name="props"> the PropList to duplicate. </param>
        public PropList(PropList props)
        {
            // in the order of the data members above ...

            setPropListName(props.getPropListName());

            __list = new List<Prop>();
            // duplicate all the props
            int size = props.size();
            Prop prop = null;
            for (int i = 0; i < size; i++)
            {
                prop = props.propAt(i);
                set(new Prop(prop.getKey(), prop.getValue()), false);
            }

            setPersistentName(props.getPersistentName());
            setPersistentFormat(props.getPersistentFormat());
            // __lastLineNumberRead is ignored (that var probably doesn't need to be private)
            setLiteralQuotes(props.getLiteralQuotes());
            setHowSet(props.getHowSet());
        }

        /// <summary>
        /// Construct given the name of the list (the list name should be unique if
        /// multiple lists are being used in a PropListManager.  The persistent format defaults to FORMAT_UNKNOWN. </summary>
        /// <param name="listName"> The name of the property list. </param>
        /// <seealso cref= PropListManager </seealso>
        public PropList(string listName)
        {
            initialize(listName, "", FORMAT_UNKNOWN);
        }

        /// <summary>
        /// Construct using a list name and a configuration file format type. </summary>
        /// <param name="listName"> The name of the property list. </param>
        /// <param name="persistentFormat"> The format of the list when written to a configuration file (see FORMAT_*). </param>
        public PropList(string listName, int persistentFormat)
        {
            initialize(listName, "", persistentFormat);
        }

        /// <summary>
        /// Construct using a list name, a configuration file name,
        /// and a configuration file format type.  The file is not actually read (call readPersistent() to do so). </summary>
        /// <param name="listName"> The name of the property list. </param>
        /// <param name="persistentName"> The name of the configuration file. </param>
        /// <param name="persistentFormat"> The format of the list when written to a configuration file (see FORMAT_*). </param>
        public PropList(string listName, string persistentName, int persistentFormat)
        {
            initialize(listName, persistentName, persistentFormat);
        }

        /**
        Append a property to the list using a string key.
        @param key String key for the property.
        @param contents Contents for the property.
        */
        private void append(string key, Object contents, bool isLiteral)
        {
            Prop prop = new Prop(key, contents, contents.ToString(), __howSet);
            prop.SetIsLiteral(isLiteral);
            if (Message.isDebugOn)
            {
                Message.printDebug(100, "PropList.append", "Setting property \"" + key + "\" to: \"" + contents.ToString() + "\"");
            }
            __list.Add(prop);
        }

        /**
        Append a property to the list using a string key, the contents, and value.
        @param key String key for the property.
        @param contents Contents for the property.
        @param value String value for the property.
        */
        private void append(string key, Object contents, string value)
        {
            Prop prop = new Prop(key, contents, value, __howSet);
            if (Message.isDebugOn)
            {
                Message.printDebug(100, "PropList.append", "Setting property \"" + key + "\" to: \"" + value + "\"");
            }
            __list.Add(prop);
        }

        /// <summary>
        /// Remove all items from the PropList.
        /// </summary>
        public virtual void clear()
        {
            __list.Clear();
        }

        /// <summary>
        /// Return the Prop instance at the requested position.  Use size() to determine the size of the PropList. </summary>
        /// <param name="pos"> the position of the property to return (0+). </param>
        /// <returns> the Prop at the specified position. </returns>
        public virtual Prop elementAt(int pos)
        {
            return __list[pos];
        }

        /// <summary>
        /// Find a property in the list. </summary>
        /// <returns> The index position of the property corresponding to the string key, or -1 if not found. </returns>
        /// <param name="key"> The string key used to look up the property. </param>
        public virtual int findProp(string key)
        {
            int size = __list.Count;
            Prop prop_i;
            string propKey;
            for (int i = 0; i < size; i++)
            {
                prop_i = __list[i];
                propKey = (string)prop_i.getKey();
                if (key.Equals(propKey, StringComparison.OrdinalIgnoreCase))
                {
                    // Have a match.  Return the position...
                    if (Message.isDebugOn)
                    {
                        Message.printDebug(100, "PropList.findProp", "Found property \"" + key + "\" at index " + i);
                    }
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Return property contents. </summary>
        /// <returns> The contents of the property as an Object, or null if not found. </returns>
        /// <param name="key"> The string key used to look up the property. </param>
        public virtual object getContents(string key)
        {
            int pos = findProp(key);
            if (pos >= 0)
            {
                // We have a match.  Get the contents...
                return (__list[pos]).getContents();
            }
            if (Message.isDebugOn)
            {
                Message.printDebug(100, "PropList.getContents", "Did not find property \"" + key + "\"");
            }
            return null;
        }

        /// <summary>
        /// Return the "how set" value that is in effect when setting properties. </summary>
        /// <returns> the "how set" value that is in effect when setting properties. </returns>
        public virtual int getHowSet()
        {
            return __howSet;
        }

        /// <summary>
        /// Indicate whether quotes in the contents should be handled literally. </summary>
        /// <returns> true if quotes are handled literally, false if they should be discarded
        /// when contents are converted to the string value. </returns>
        public virtual bool getLiteralQuotes()
        {
            return __literalQuotes;
        }

        /// <summary>
        /// Return the name of the property list file. </summary>
        /// <returns> The name of the property list file. </returns>
        public virtual string getPersistentName()
        {
            return __persistentName;
        }

        /// <summary>
        /// Return the format of the property list file. </summary>
        /// <returns> The format of the property list file. </returns>
        public virtual int getPersistentFormat()
        {
            return __persistentFormat;
        }

        /// <summary>
        /// Search the list using the string key. </summary>
        /// <returns> The property corresponding to the string key, or null if not found. </returns>
        /// <param name="key"> The string key used to look up the property. </param>
        public virtual Prop getProp(string key)
        {
            int pos = findProp(key);
            if (pos >= 0)
            {
                Prop prop = __list[pos];
                prop.refresh(this);
                return prop;
            }
            return null;
        }

        /// <summary>
        /// Return the name of the property list. </summary>
        /// <returns> The name of the property list. </returns>
        public virtual string getPropListName()
        {
            return __listName;
        }

        /// <summary>
        /// The string value of the property corresponding to the string key, or null if not found. </summary>
        /// <returns> The string value of the property corresponding to the string key. </returns>
        /// <param name="key"> The string key used to look up the property. </param>
        public virtual string getValue(string key)
        {
            int pos = findProp(key);
            if (pos >= 0)
            {
                // We have a match.  Get the value...
                string value = (__list[pos]).getValue(this);
                if (Message.isDebugOn)
                {
                    Message.printDebug(100, "PropList.getValue", "Found value of \"" + key + "\" to be \"" + value + "\"");
                }
                return value;
            }
            if (Message.isDebugOn)
            {
                Message.printDebug(100, "PropList.getValue", "Did not find property \"" + key + "\"");
            }
            return null;
        }

        /// <summary>
        /// Initialize the object. </summary>
        /// <param name="listName"> Name for the PropList. </param>
        /// <param name="persistentName"> Persistent name for the PropList (used only when reading from a file). </param>
        /// <param name="persistentFormat"> Format for properties file. </param>
        private void initialize(string listName, string persistentName, int persistentFormat)
        {
            if (string.ReferenceEquals(listName, null))
            {
                __listName = "";
            }
            else
            {
                __listName = listName;
            }
            if (string.ReferenceEquals(persistentName, null))
            {
                __persistentName = "";
            }
            else
            {
                __persistentName = persistentName;
            }
            __persistentFormat = persistentFormat;
            __list = new List<Prop>();
            __lastLineNumberRead = 0;
        }

        /// <summary>
        /// Return the prop at a position (zero-index), or null if the index is out of range. </summary>
        /// <returns> The property for the specified index position (referenced to zero).
        /// Return null if the index is invalid. </returns>
        /// <param name="i"> The index position used to look up the property. </param>
        public virtual Prop propAt(int i)
        {
            if ((i < 0) || (i > (__list.Count - 1)))
            {
                return null;
            }
            return __list[i];
        }

        /// <summary>
        /// Read a property list from a persistent source, appending new properties to current list.
        /// The properties will be appended to the current list and non-property literals like comments will be ignored. </summary>
        /// <exception cref="IOException"> if there is an error reading the file. </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void readPersistent() throws java.io.IOException
        public virtual void readPersistent()
        {
            readPersistent(true);
        }

        /// <summary>
        /// Read a property list from a persistent source, appending new properties to current list.
        /// Non-property literals like comments will be ignored. </summary>
        /// <param name="append"> If true, properties from the file will be appended to the current list. </param>
        /// <exception cref="IOException"> if there is an error reading the file. </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void readPersistent(boolean append) throws java.io.IOException
        public virtual void readPersistent(bool append)
        {
            readPersistent(true, false);
        }

        /// <summary>
        /// Read a property list from a persistent source.  The "how_set" flag for each
        /// property is set to Prop.SET_FROM_PERSISTENT.  The file can have the format:
        /// <pre>
        /// # COMMENT
        /// # Simple setting:
        /// variable = value
        /// 
        /// \/\*
        /// Multi-line
        /// comment - start and end of comments must be in first characters of line.
        /// \*\/
        /// 
        /// # Use section headings:
        /// [MyProps]
        /// prop1 = value
        /// 
        /// # For the above retrieve with MyProps.prop1
        /// 
        /// # Lines can have continuation if \ is at the end:
        /// variable = xxxx \
        /// yyy
        /// 
        /// # Properties with whitespace can be enclosed in " " for clarity (or not):
        /// variable = "string with spaces"
        /// 
        /// # Variables ${var} will be expanded at query time to compare to
        /// # IOUtil.getPropValue() and also other defined properties:
        /// variable = ${var}
        /// 
        /// # Text defined inside 'hard quotes' will not be expanded and will be literal:
        /// variable = 'hello ${world}'
        /// variable = 'hello `date`'
        /// 
        /// # Text defined inside "soft quotes" will be expanded:
        /// variable = "hello ${world}"
        /// variable = "hello `date`"
        /// 
        /// # Duplicate variables will be read in.  However, to lookup, use getPropValue()
        /// </pre> </summary>
        /// <param name="append"> Append to current property list (true) or clear out current list (false). </param>
        /// <param name="includeLiterals"> if true, comments and other non-property lines will be included as literals
        /// in the property list using key "Literal1", "Literal2", etc.  This is useful if reading a property file,
        /// updating its values, and then writing out again, trying to retain the original comments. </param>
        /// <exception cref="IOException"> if there is an error reading the file. </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void readPersistent(boolean append, boolean includeLiterals) throws java.io.IOException
        public virtual void readPersistent(bool append, bool includeLiterals)
        {
            string routine = "PropList.readPersistent";

            string line;
            string prefix = "";
            int idx;
            bool continuation = false;
            string lineSave = null;
            string name, value;
            IList<string> v = null;
            bool inComment = false;
            int literalCount = 0;

            if (!append)
            {
                clear();
            }

            StreamReader @in = null;
            try
            {
                @in = IOUtil.getInputStream(__persistentName);
            }
            catch (Exception)
            {
                string message = "Unable to open \"" + __persistentName + "\" to read properties.";
                Message.printWarning(2, routine, message);
                throw new IOException(message);
            }

            int howSetPrev = __howSet;
            __howSet = Prop.SET_FROM_PERSISTENT;
            try
            {
                __lastLineNumberRead = 0;
                int length = 0;
                while (!string.ReferenceEquals((line = @in.ReadLine()), null))
                {
                    line = line.Trim();
                    ++__lastLineNumberRead;
                    if (continuation)
                    {
                        // Take this line and add it to the previous.  Add a space to separate tokens.
                        // Should not normally be a comment.
                        line = lineSave + " " + line;
                    }
                    // Handle line continuation with \ at end...
                    if (line.EndsWith("\\", StringComparison.Ordinal))
                    {
                        continuation = true;
                        // Add a space at the end because when continuing lines the next line likely has separation tokens.
                        lineSave = line.Substring(0, line.Length - 1);
                        continue;
                    }
                    continuation = false;
                    lineSave = null;
                    if (line.Length > 0)
                    {
                        if (line[0] == '#')
                        {
                            // Comment line
                            if (includeLiterals)
                            {
                                ++literalCount;
                                this.append("Literal" + literalCount, line, true);
                            }
                            continue;
                        }
                        else if (line.StartsWith("<#", StringComparison.Ordinal) || line.StartsWith("</#", StringComparison.Ordinal))
                        {
                            // Freemarker template syntax
                            // TODO sam 2017-04-08 evaluate whether to handle here by default
                            // or pass in parameters to ignore line patterns
                            continue;
                        }
                    }
                    if ((idx = line.IndexOf('#')) != -1)
                    {
                        // Remove # comments from the ends of lines
                        line = line.Substring(0, idx);
                    }
                    if (inComment && line.StartsWith("*/", StringComparison.Ordinal))
                    {
                        inComment = false;
                        // For now the end of comment must be at the start of the line so ignore the rest of the line...
                        if (includeLiterals)
                        {
                            ++literalCount;
                            this.append("Literal" + literalCount, line, true);
                        }
                        continue;
                    }
                    if (!inComment && line.StartsWith("/*", StringComparison.Ordinal))
                    {
                        inComment = true;
                        // For now the end of comment must be at the start of the line so ignore the rest of the line...
                        if (includeLiterals)
                        {
                            ++literalCount;
                            this.append("Literal" + literalCount, line, true);
                        }
                        continue;
                    }
                    if (inComment)
                    {
                        // Did not detect an end to the comment above so skip the line...
                        if (includeLiterals)
                        {
                            ++literalCount;
                            this.append("Literal" + literalCount, line, true);
                        }
                        continue;
                    }
                    if (line.Length == 0)
                    {
                        if (includeLiterals)
                        {
                            ++literalCount;
                            this.append("Literal" + literalCount, line, true);
                        }
                        continue;
                    }
                    if (line[0] == '[')
                    {
                        // Block indicator - contents of [] will be prepended to property names
                        if (line.IndexOf("${", StringComparison.Ordinal) >= 0)
                        {
                            // Likely a Freemarker template syntax so skip
                            // TODO sam 2017-04-08 may need to be more specific about this
                            continue;
                        }
                        if (line.IndexOf(']') == -1)
                        {
                            Message.printWarning(2, routine, "Missing ] on line " + __lastLineNumberRead + " of " + __persistentName);
                            continue;
                        }
                        prefix = line.Substring(1, line.IndexOf(']') - 1) + ".";
                        if (includeLiterals)
                        {
                            ++literalCount;
                            this.append("Literal" + literalCount, line, true);
                        }
                        continue;
                    }

                    int pos = line.IndexOf('=');
                    if (pos < 0)
                    {
                        Message.printWarning(2, routine, "Missing equal sign on line " + __lastLineNumberRead + " of " + __persistentName + " (" + line + ")");
                        if (includeLiterals)
                        {
                            ++literalCount;
                            this.append("Literal" + literalCount, line, true);
                        }
                        continue;
                    }
                    v = new List<string>(2);
                    v.Add(line.Substring(0, pos));
                    v.Add(line.Substring(pos + 1));

                    if (v.Count == 2)
                    {
                        name = prefix + v[0].Trim();
                        value = v[1].Trim();
                        length = value.Length;
                        if ((length > 1) && ((value[0] == '"') || (value[0] == '\'') && (value[0] == value[length - 1])))
                        {
                            // Get rid of bounding quotes because they are not needed...
                            value = value.Substring(1, (length - 1) - 1);
                        }
                        // Now set in the PropList...
                        if (name.Length > 0)
                        {
                            this.append(name, value, false);
                        }
                    }
                    else
                    {
                        Message.printWarning(2, routine, "Missing or too many equal signs on line " + __lastLineNumberRead + " of " + __persistentName + " (" + line + ")");
                    }
                }

                @in.Close();
            }
            catch (Exception)
            {
                string message = "Exception caught while reading line " + __lastLineNumberRead + " of " + __persistentName + ".";
                Message.printWarning(3, routine, message);
                throw new IOException(message);
            }
            finally
            {
                if (@in != null)
                {
                    @in.Close();
                }
            }
            // Clean up...
            __howSet = howSetPrev;
        }

        /// <summary>
        /// Set the property given a string like "prop=propcontents" where "propcontents"
        /// can be a string containing wild-cards.  This feature is used with configuration files.
        /// If the property key exists, reset the property to the new information. </summary>
        /// <seealso cref= PropListManager </seealso>
        /// <param name="propString"> A property string like prop=contents. </param>
        //public virtual void set(string propString)
        //{
        //    Prop prop = PropListManager.parsePropString(this, propString);
        //    if (prop == null)
        //    {
        //        return;
        //    }
        //    set(prop.getKey(), (string)prop.getContents(), prop.getValue(this));
        //}

        /// <summary>
        /// Set the property given a string key and string value. 
        /// If the property key exists, reset the property to the new information. </summary>
        /// <param name="key"> The property string key. </param>
        /// <param name="value"> The string value of the property (will also be used for the contents). </param>
        //public virtual void set(string key, string value)
        //{
        //    set(key, value, true);
        //}

        /// <summary>
        /// Set the property given a string key and string value.  If the key already exists
        /// it will be replaced if the replace parameter is true.  Otherwise, a duplicate property will be added to the list. </summary>
        /// <param name="key"> The property string key. </param>
        /// <param name="value"> The string value of the property (will also be used for the contents. </param>
        /// <param name="replace"> if true, if the key already exists in the PropList, its value
        /// will be replaced.  If false, a duplicate key will be added. </param>
        //public virtual void set(string key, string value, bool replace)
        //{
        //    int index = findProp(key);

        //    if (index < 0 || !replace)
        //    {
        //        // Not currently in the list so add it...
        //        Append(key, (object)value, value);
        //    }
        //    else
        //    {
        //        // Already in the list so change it...
        //        Prop prop = __list[index];
        //        prop.setKey(key);
        //        prop.setContents(value);
        //        prop.setValue(value);
        //        prop.setHowSet(__howSet);
        //    }
        //}

        /// <summary>
        /// Set the property given a string key and contents. 
        /// If the property key exists, reset the property to the new information. </summary>
        /// <param name="key"> The property string key. </param>
        /// <param name="contents"> The contents of the property. </param>
        /// <param name="value"> The string value of the property </param>
        //public virtual void set(string key, string contents, string value)
        //{
        //    set(key, contents, value, true);
        //}

        /// <summary>
        /// Set the property given a string key and contents. If the key already exists
        /// it will be replaced if the replace parameter is true.  Otherwise, a duplicate property will be added to the list. </summary>
        /// <param name="key"> The property string key. </param>
        /// <param name="contents"> The contents of the property. </param>
        /// <param name="value"> The string value of the property </param>
        /// <param name="replace"> if true, if the key already exists in the PropList, its value
        /// will be replaced.  If false, a duplicate key will be added. </param>
        //public virtual void set(string key, string contents, string value, bool replace)
        //{ // Find if this is already a property in this list...

        //    int index = findProp(key);
        //    if (index < 0 || !replace)
        //    {
        //        // Not currently in the list so add it...
        //        append(key, contents, value);
        //    }
        //    else
        //    {
        //        // Already in the list so change it...
        //        Prop prop = __list[index];
        //        prop.setKey(key);
        //        prop.setContents(contents);
        //        prop.setValue(value);
        //        prop.setHowSet(__howSet);
        //    }
        //}

        /// <summary>
        /// Set the property given a string like "prop=propcontents" where "propcontents"
        /// can be a string containing wild-cards.  This feature is used with configuration files.
        /// If the property key exists, reset the property to the new information. </summary>
        /// <seealso cref= PropListManager </seealso>
        /// <param name="propString"> A property string like prop=contents. </param>
        public virtual void set(string propString)
        {
            Prop prop = PropListManager.parsePropString(this, propString);
            if (prop == null)
            {
                return;
            }
            set(prop.getKey(), (string)prop.getContents(), prop.getValue(this));
        }

        /// <summary>
        /// Set the property given a string key and string value. 
        /// If the property key exists, reset the property to the new information. </summary>
        /// <param name="key"> The property string key. </param>
        /// <param name="value"> The string value of the property (will also be used for the contents). </param>
        public virtual void set(string key, string value)
        {
            set(key, value, true);
        }

        /// <summary>
        /// Set the property given a string key and contents. 
        /// If the property key exists, reset the property to the new information. </summary>
        /// <param name="key"> The property string key. </param>
        /// <param name="contents"> The contents of the property. </param>
        /// <param name="value"> The string value of the property </param>
        public virtual void set(string key, string contents, string value)
        {
            set(key, contents, value, true);
        }

        /// <summary>
        /// Set the property given a string key and string value.  If the key already exists
        /// it will be replaced if the replace parameter is true.  Otherwise, a duplicate property will be added to the list. </summary>
        /// <param name="key"> The property string key. </param>
        /// <param name="value"> The string value of the property (will also be used for the contents. </param>
        /// <param name="replace"> if true, if the key already exists in the PropList, its value
        /// will be replaced.  If false, a duplicate key will be added. </param>
        public virtual void set(string key, string value, bool replace)
        {
            int index = findProp(key);

            if (index < 0 || !replace)
            {
                // Not currently in the list so add it...
                append(key, (object)value, value);
            }
            else
            {
                // Already in the list so change it...
                Prop prop = __list[index];
                prop.setKey(key);
                prop.setContents(value);
                prop.setValue(value);
                prop.setHowSet(__howSet);
            }
        }

        /// <summary>
        /// Set the property given a string key and contents. If the key already exists
        /// it will be replaced if the replace parameter is true.  Otherwise, a duplicate property will be added to the list. </summary>
        /// <param name="key"> The property string key. </param>
        /// <param name="contents"> The contents of the property. </param>
        /// <param name="value"> The string value of the property </param>
        /// <param name="replace"> if true, if the key already exists in the PropList, its value
        /// will be replaced.  If false, a duplicate key will be added. </param>
        public virtual void set(string key, string contents, string value, bool replace)
        { // Find if this is already a property in this list...

            int index = findProp(key);
            if (index < 0 || !replace)
            {
                // Not currently in the list so add it...
                append(key, contents, value);
            }
            else
            {
                // Already in the list so change it...
                Prop prop = __list[index];
                prop.setKey(key);
                prop.setContents(contents);
                prop.setValue(value);
                prop.setHowSet(__howSet);
            }
        }

        /// <summary>
        /// Set the property given a Prop.  If the property key exists, reset the property to the new information. </summary>
        /// <param name="prop"> The contents of the property. </param>
        public virtual void set(Prop prop)
        {
            set(prop, true);
        }

        /// <summary>
        /// Set the property given a Prop. If the key already exists
        /// it will be replaced if the replace parameter is true.  Otherwise, a duplicate property will be added to the list.
        /// If the property key exists, reset the property to the new information. </summary>
        /// <param name="prop"> The contents of the property. </param>
        public virtual void set(Prop prop, bool replace)
        { // Find if this is already a property in this list...
            if (prop == null)
            {
                return;
            }
            int index = findProp(prop.getKey());
            if (index < 0 || !replace)
            {
                // Not currently in the list so add it...
                this.append(prop.getKey(), prop.getContents(), prop.getValue(this));
            }
            else
            {
                // Already in the list so change it...
                prop.setHowSet(__howSet);
                __list[index] = prop;
            }
        }

        /// <summary>
        /// Set the "how set" flag, which indicates how properties are being set. </summary>
        /// <param name="how_set"> For properties that are being set, indicates how they are being set (see Prop.SET_*). </param>
        public virtual void setHowSet(int how_set)
        {
            __howSet = how_set;
        }

        /// <summary>
        /// Set the flag for how to handle quotes in the PropList when expanding the
        /// contents.  The default is to keep the literal quotes.  This parameter is
        /// checked by the PropListManager.resolveContentsValue() method. </summary>
        /// <param name="literal_quotes"> If set to true, quotes in the contents will be passed
        /// literally to the value.  If set to false, the quotes will be discarded when
        /// contents are converted to a string value. </param>
        public virtual void setLiteralQuotes(bool literal_quotes)
        {
            __literalQuotes = literal_quotes;
        }

        /// <summary>
        /// Set the property list name. </summary>
        /// <param name="list_name"> The property list name. </param>
        public virtual int setPropListName(string list_name)
        {
            if (!string.ReferenceEquals(list_name, null))
            {
                __listName = list_name;
            }
            return 0;
        }

        /// <summary>
        /// Set the configuration file name. </summary>
        /// <param name="persistent_name"> The configuration file name. </param>
        public virtual void setPersistentName(string persistent_name)
        {
            if (!string.ReferenceEquals(persistent_name, null))
            {
                __persistentName = persistent_name;
            }
        }

        /// <summary>
        /// Set the configuration file format. </summary>
        /// <param name="persistent_format"> The configuration file format. </param>
        public virtual void setPersistentFormat(int persistent_format)
        {
            __persistentFormat = persistent_format;
        }

        /// <summary>
        /// Return the size of the property list. </summary>
        /// <returns> The size of the property list. </returns>
        public virtual int size()
        {
            if (__list == null)
            {
                return 0;
            }
            else
            {
                return __list.Count;
            }
        }

        /// <summary>
        /// Return a string representation of the PropList (verbose). </summary>
        /// <returns> A string representation of the PropList (verbose). </returns>
        public override string ToString()
        {
            return ToString(",");
        }

        /// <summary>
        /// Return a string representation of the PropList (verbose).
        /// The properties are formatted as follows:  prop="value",prop="value" </summary>
        /// <returns> A string representation of the PropList (verbose). </returns>
        /// <param name="delim"> Delimiter to use between properties (comma in the above example). </param>
        public virtual string ToString(string delim)
        {
            StringBuilder b = new StringBuilder();
            int size = __list.Count;
            Prop prop;
            for (int i = 0; i < size; i++)
            {
                prop = __list[i];
                if (i > 0)
                {
                    b.Append(delim);
                }
                if (prop == null)
                {
                    b.Append("null");
                }
                else
                {
                    b.Append(prop.getKey() + "=\"" + prop.getValue() + "\"");
                }
            }
            return b.ToString();
        }

    }
}
