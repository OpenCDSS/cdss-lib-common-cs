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

	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

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

	/// <summary>
	/// Add a property by parsing out a property string like "X=Y". </summary>
	/// <param name="prop_string"> A property string. </param>
	public virtual void add(string prop_string)
	{ // Just call the set routine...
		set(prop_string);
	}

	/// <summary>
	/// Append a property to the list using a string key. </summary>
	/// <param name="key"> String key for the property. </param>
	/// <param name="contents"> Contents for the property. </param>
	private void append(string key, object contents, bool isLiteral)
	{
		Prop prop = new Prop(key, contents, contents.ToString(), __howSet);
		prop.setIsLiteral(isLiteral);
		if (Message.isDebugOn)
		{
			Message.printDebug(100, "PropList.append", "Setting property \"" + key + "\" to: \"" + contents.ToString() + "\"");
		}
		__list.Add(prop);
	}

	/// <summary>
	/// Append a property to the list using a string key, the contents, and value. </summary>
	/// <param name="key"> String key for the property. </param>
	/// <param name="contents"> Contents for the property. </param>
	/// <param name="value"> String value for the property. </param>
	private void append(string key, object contents, string value)
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
	/// Finalize before garbage collection. </summary>
	/// <exception cref="Throwable"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~PropList()
	{
		__listName = null;
		__list = null;
		__persistentName = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
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
	/// Find a property in the list. </summary>
	/// <returns> The index position of the property corresponding to the string key, or -1 if not found. </returns>
	/// <param name="key"> The string key used to look up the property. </param>
	/// <param name="inst"> Instance number of property (0+). If inst is one, then
	/// the second property with that name is returned, etc. </param>
	public virtual int findProp(string key, int inst)
	{
		int size = size();
		for (int i = 0; i < size; i++)
		{
			if (propAt(i).getKey().Equals(key, StringComparison.OrdinalIgnoreCase))
			{
				if ((inst--) != 0)
				{
					 continue;
				}
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// Find a property in the list. </summary>
	/// <returns> The index position of the property corresponding to the integer key, or -1 if not found. </returns>
	/// <param name="intKey"> The integer key used to look up the property. </param>
	public virtual int findProp(int intKey)
	{
		int prop_intKey, size = __list.Count;
		Prop prop_i;
		for (int i = 0; i < size; i++)
		{
			prop_i = __list[i];
			prop_intKey = prop_i.getIntKey();
			if (intKey == prop_intKey)
			{
				// Have a match.  Return the position...
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
	/// Returns the list of Props. </summary>
	/// <returns> the list of Props. </returns>
	public virtual IList<Prop> getList()
	{
		return __list;
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
	/// Return the name of the property list. </summary>
	/// <returns> The name of the property list. </returns>
	public virtual string getPropListName()
	{
		return __listName;
	}

	/// <summary>
	/// Returns a count of the number of the properties in the list that have the specified key value. </summary>
	/// <param name="key"> the key for which to find matching properties.  Can not be null. </param>
	/// <returns> the number of properties in the list with the same key. </returns>
	public virtual int getPropCount(string key)
	{
		int count = 0;
		int size = size();
		for (int i = 0; i < size; i++)
		{
			if (propAt(i).getKey().Equals(key, StringComparison.OrdinalIgnoreCase))
			{
				count++;
			}
		}
		return count;
	}

	// TODO SAM 2005-04-29 StringUtil.matchesRegExp() has been deprecated.
	// Need to figure out how to update the following code.

	/// <summary>
	/// Return a list of Prop that have a key that matches a regular expression.
	/// This is useful when writing a PropList to a file in a well-defined order. </summary>
	/// <param name="regExp"> Regular expression recognized by StringUtil.matchesRegExp(). </param>
	/// <returns> a list of Prop, or null if no matching properties are found. </returns>
	public virtual IList<Prop> getPropsMatchingRegExp(string regExp)
	{
		if ((__list == null) || (string.ReferenceEquals(regExp, null)))
		{
			return null;
		}
		int size = __list.Count;
		IList<Prop> props = new List<Prop>();
		Prop prop;
		for (int i = 0; i < size; i++)
		{
			prop = __list[i];
			// Do a case-independent comparison...
			if (StringUtil.matchesRegExp(true, prop.getKey(), regExp))
			{
				props.Add(prop);
			}
		}
		if (props.Count == 0)
		{
			props = null;
		}
		return props;
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
	/// Return the property corresponding to the string key or null if not found. </summary>
	/// <returns> The property corresponding to the string key, or <CODE>null</CODE> if not found. </returns>
	/// <param name="key"> The string key used to look up the property. </param>
	/// <param name="inst"> Instance number of property (0+). If inst is one, then
	/// the second property with that name is returned, etc. </param>
	public virtual Prop getProp(string key, int inst)
	{
		int index = findProp(key, inst);
		if (index == -1)
		{
			return null;
		}
		return propAt(index);
	}

	/// <summary>
	/// Search the list using the integer key.  This should not be confused with
	/// elementAt(), which returns the property at a position in the list. </summary>
	/// <returns> The property corresponding to the string key. </returns>
	/// <param name="intKey"> The integer key used to look up the property. </param>
	public virtual Prop getProp(int intKey)
	{
		int pos = findProp(intKey);
		if (pos >= 0)
		{
			return __list[pos];
		}
		return null;
	}

	/// <summary>
	/// Return a valid non-null PropList. </summary>
	/// <returns> A valid PropList.  If the PropList that is passed in is null, a new
	/// PropList will be created given the supplied name.  This is useful for routines
	/// that need a valid PropList for local processing (rather than checking for a
	/// null list that may have been passed as input). </returns>
	/// <param name="props"> PropList to check.  Will be returned if not null. </param>
	/// <param name="newName"> Name of new PropList if "props" is null. </param>
	public static PropList getValidPropList(PropList props, string newName)
	{
		if (props != null)
		{
			// List is not null, so return...
			return props;
		}
		else
		{
			// List is null, so create a new and return...
			if (string.ReferenceEquals(newName, null))
			{
				return new PropList("PropList.getValidPropList");
			}
			return new PropList(newName);
		}
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
				Message.printDebug(100,"PropList.getValue", "Found value of \"" + key + "\" to be \"" + value + "\"");
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
	/// Return the string value of the property given the instance number (allows
	/// storage of duplicate keys), or null if not found. </summary>
	/// <returns> The string value of the property corresponding to the string key, or <CODE>null</CODE> if not found. </returns>
	/// <param name="key"> The string key used to look up the property. </param>
	/// <param name="inst"> Instance number of property (0+). If inst is one, then
	/// the second property with that name is returned, etc. </param>
	public virtual string getValue(string key, int inst)
	{
		Prop p = getProp(key, inst);
		if (p == null)
		{
			return null;
		}
		string value = p.getValue(this);
		p = null;
		return value;
	}

	/// <summary>
	/// Return the string value of the property corresponding to the integer key, or null if not a valid integer key. </summary>
	/// <returns> The string value of the property corresponding to the integer key. </returns>
	/// <param name="intKey"> The integer key used to look up the property. </param>
	public virtual string getValue(int intKey)
	{
		int pos = findProp(intKey);
		if (pos >= 0)
		{
			// Have a match.  Get the value...
			return (__list[pos]).getValue(this);
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
	/// Create a PropList by parsing a string of the form prop="val",prop="val".
	/// The "how set" value for each property is set to Prop.SET_UNKNOWN. </summary>
	/// <param name="string"> String to parse. </param>
	/// <param name="listName"> the name to assign to the new PropList. </param>
	/// <param name="delim"> the delimiter to use when parsing the proplist ("," in the above
	/// example).  Quoted strings are assumed to be allowed. </param>
	/// <returns> a PropList with the expanded properties.  A non-null value is
	/// guaranteed; however, the list may contain zero items. </returns>
	public static PropList parse(string @string, string listName, string delim)
	{
		return parse(Prop.SET_UNKNOWN, @string, listName, delim);
	}

	/// <summary>
	/// Create a PropList by parsing a string of the form: prop="val",prop="val".  There
	/// may be spaces embedded between tokens. </summary>
	/// <param name="howSet"> Indicate the "how set" value that should be set for all
	/// properties that are parsed (see Prop.SET_*). </param>
	/// <param name="string"> String to parse. </param>
	/// <param name="listName"> the name to assign to the new PropList. </param>
	/// <param name="delim"> the delimiter to use when parsing the PropList ("," in the above
	/// example).  Quoted strings are assumed to be allowed. </param>
	/// <returns> a PropList with the expanded properties.  A non-null value is
	/// guaranteed; however, the list may contain zero items. </returns>
	/// <seealso cref= Prop </seealso>
	public static PropList parse(int howSet, string @string, string listName, string delim)
	{
		PropList props = new PropList(listName);
		props.setHowSet(howSet);
		if (string.ReferenceEquals(@string, null))
		{
			return props;
		}
		// Allowing quoted strings is necessary because a comma or = could be in a string
		IList<string> tokens = StringUtil.breakStringList(@string, delim, StringUtil.DELIM_ALLOW_STRINGS);

		int size = 0;
		if (tokens != null)
		{
			size = tokens.Count;
		}
		for (int i = 0; i < size; i++)
		{
			//Message.printStatus ( 2, "PropList.parse", "Parsing parameter string \"" + tokens.elementAt(i));
			// The above call to breakStringList() may have stripped quotes that would protected "=" in the
			// properties.  Therefore just find the first "=" and take the left and right sides.
			string token = tokens[i];
			int pos = token.IndexOf('=');
			if (pos > 0)
			{
				// Don't want property names to have spaces
				string prop = token.Substring(0,pos).Trim();
				// TODO SAM 2008-11-18 Evaluate how to handle whitespace
				string value = "";
				if (token.Length > (pos + 1))
				{
					// Right side is NOT empty
					value = token.Substring((pos + 1), token.Length - (pos + 1));
				}
				//Message.printStatus ( 2, "PropList.parse", "Setting property \"" + prop + "\"=\"" + value + "\"" );
				props.set(prop, value);
			}
		}
		return props;
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
			@in = new StreamReader(IOUtil.getInputStream(__persistentName));
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
					lineSave = line.Substring(0,line.Length - 1);
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
							append("Literal" + literalCount, line, true);
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
						append("Literal" + literalCount, line, true);
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
						append("Literal" + literalCount, line, true);
					}
					continue;
				}
				if (inComment)
				{
					// Did not detect an end to the comment above so skip the line...
					if (includeLiterals)
					{
						++literalCount;
						append("Literal" + literalCount, line, true);
					}
					continue;
				}
				if (line.Length == 0)
				{
					if (includeLiterals)
					{
						++literalCount;
						append("Literal" + literalCount, line, true);
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
						append("Literal" + literalCount, line, true);
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
						append("Literal" + literalCount, line, true);
					}
					continue;
				}
				v = new List<string>(2);
				v.Add(line.Substring(0,pos));
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
						append(name, value, false);
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
	public virtual void set(string propString)
	{
		Prop prop = PropListManager.parsePropString(this,propString);
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
			append(prop.getKey(), prop.getContents(), prop.getValue(this));
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
	/// Set the property given a string key and contents.  If the contents do not have
	/// a clean string value, use set ( new Prop(String,Object,String) ) instead.
	/// If the property key exists, reset the property to the new information. </summary>
	/// <param name="key"> The property string key. </param>
	/// <param name="contents"> The contents of the property as an Object.  The value is
	/// determined by calling the object's toString() method.  If contents are null, then
	/// the String value is also set to null. </param>
	public virtual void setUsingObject(string key, object contents)
	{ // Ignore null keys...

		if (string.ReferenceEquals(key, null))
		{
			return;
		}

		// Find if this is already a property in this list...

		int index = findProp(key);
		string value = null;
		if (contents != null)
		{
			contents.ToString();
		}
		if (index < 0)
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
		value = null;
	}

	/// <summary>
	/// Set the value of a property after finding in the list (warning:  if the
	/// key is not found, a new property will be added).
	/// <b>Warning:  If using the PropList in a general way, set values using the
	/// set() method (setValue is a used internally and for advanced code - additional
	/// checks are being added to make setValue more universal).</b> </summary>
	/// <param name="key"> String key for property. </param>
	/// <param name="value"> String value for property. </param>
	public virtual void setValue(string key, string value)
	{
		int pos = findProp(key);
		if (pos >= 0)
		{
			// Have a match.  Reset the value in the corresponding Prop...
			Prop prop = __list[pos];
			prop.setValue(value);
			return;
		}
		// If we get to here we did not find a match and need to add a new item to the list...
		append(key, value, false);
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
	/// Does a simple sort of the items in the PropList, using java.util.Collections().
	/// </summary>
	public virtual void sortList()
	{
		__list.Sort();
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

	/// <summary>
	/// Unset a value (remove from the list).
	/// Remove the property from the property list. </summary>
	/// <param name="pos"> index (0+) for property to remove. </param>
	public virtual void unset(int pos)
	{
		if (pos >= 0)
		{
			__list.RemoveAt(pos);
		}
	}

	/// <summary>
	/// Unset a value (remove from the list).
	/// Remove the property from the property list. </summary>
	/// <param name="key"> String key for property to remove. </param>
	public virtual void unSet(string key)
	{
		int pos = findProp(key);
		if (pos >= 0)
		{
			__list.RemoveAt(pos);
		}
	}

	/// <summary>
	/// Unsets all properties with matching keys from the list. </summary>
	/// <param name="key"> String key for the properties to remove. </param>
	public virtual void unSetAll(string key)
	{
		int count = getPropCount(key);
		for (int i = 0; i < count; i++)
		{
			unSet(key);
		}
	}

	/*
	Checks all the property names in the PropList to make sure only valid and 
	deprecated ones are in the list, and returns a list with warning messages
	about deprecated and invalid properties.  Invalid properties ARE NOT removed.
	See the overloaded method for more information.
	*/
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.util.List<String> validatePropNames(java.util.List<String> validProps, java.util.List<String> deprecatedProps, java.util.List<String> deprecatedNotes, String target) throws Exception
	public virtual IList<string> validatePropNames(IList<string> validProps, IList<string> deprecatedProps, IList<string> deprecatedNotes, string target)
	{
		return validatePropNames(validProps, deprecatedProps, deprecatedNotes, target, false);
	}

	/// <summary>
	/// Checks all the property names in the PropList to make sure only valid and 
	/// deprecated ones are in the list, and returns a list with warning messages
	/// about deprecated and invalid properties. </summary>
	/// <param name="validProps"> a list of property names that are possible valid values.
	/// Not all the values in this list need to be present in the PropList, but all
	/// the property names in the PropList must be in this list to be considered valid.
	/// <para>If any properties are found in the PropList that are not valid and not 
	/// </para>
	/// deprecated, the returned list will include a line in the format:<para>
	/// </para>
	/// <pre>	[name] is not a valid [target].</pre><para>
	/// Where [name] is the name of the property and [target] is the target (see the
	/// </para>
	/// <b>target</b> parameter for more information on this).<para>
	/// If this parameter is null it will be considered the same as a zero-size
	/// list.  In both cases, all properties in the PropList will be flagged as invalid properties.
	/// </para>
	/// </param>
	/// <param name="deprecatedProps"> an optional list of the names of properties that are
	/// deprecated (i.e., in a transitional state between property names).  If any 
	/// property names in this list are found in the PropList, the returned list
	/// will contain a line in the format: <para>
	/// </para>
	/// <pre>	[name] is no longer recognized as a valid [target].  [note]</pre><para>
	/// Where [name] is the name of the property, [target] is the target (see the 
	/// <b>target</b> parameter in this method), and [note] is an optional note 
	/// </para>
	/// explaining the deprecation (see the next parameter).<para>
	/// </para>
	/// This list can be null if there are no deprecated properties.<para>
	/// <b>Note:</b> The property names in this list must not include any of the
	/// values in the deprecatedProps list, or else the property names will be 
	/// considered valid -- and <b>not</b> checked for whether they are deprecated or not.
	/// </para>
	/// </param>
	/// <param name="deprecatedNotes"> an optional list that accompanies the deprecatedProps
	/// list and offers further information about the deprecation.<para>
	/// If deprecatedProps is included, this list is optional.  However, if
	/// </para>
	/// this list is non-null, the deprecatedProps list must be non-null as well.<para>
	/// The elements in this list are related to the elements in the deprecatedProps
	/// list on a 1:1 basis.  That is, the data at element N in each list are 
	/// </para>
	/// the name of a deprecated property and a note explaining the deprecation.<para>
	/// The note will be added to the deprecation warning lines as shown above in the
	/// documentation for deprecatedProps.  For best formatting, the first character
	/// of the note should be capitalized and it should include a period at the end of the note.
	/// </para>
	/// </param>
	/// <param name="target"> an option String describing in more detail the kind of property
	/// stored in the PropList.  The default value is "property."  As shown in the 
	/// documentation above, target is used to offer further information about an invalid or deprecated prop.<para>
	/// For example, if a PropList stores worksheet properties, this value could be set
	/// </para>
	/// to "JWorksheet property" so that the error messages would be:<para>
	/// <pre>	PropertyName is no longer recognized as a valid JWorksheet property.
	/// </para>
	/// PropertyName is not a valid JWorksheet property.</pre><para>
	/// </para>
	/// </param>
	/// <param name="removeInvalid"> indicates whether invalid properties should be removed from the list. </param>
	/// <returns> null or a list containing Strings, each of which is a warning about 
	/// an invalid or deprecated property in the PropList.  The order of the returned
	/// list is that invalid properties are returned first, and deprecated properties
	/// returned second.  If null is returned, no invalid or deprecated properties were found in the PropList. </returns>
	/// <exception cref="Exception"> if an error occurs.  Specifically, if deprecatedProps and
	/// deprecatedNotes are non-null and the size of the lists is different, an 
	/// Exception will be thrown warning of the error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.util.List<String> validatePropNames(java.util.List<String> validProps, java.util.List<String> deprecatedProps, java.util.List<String> deprecatedNotes, String target, boolean removeInvalid) throws Exception
	public virtual IList<string> validatePropNames(IList<string> validProps, IList<string> deprecatedProps, IList<string> deprecatedNotes, string target, bool removeInvalid)
	{
		// Get the sizes of the lists that will be iterated through, handling null lists gracefully.

		int validPropsSize = 0;
		if (validProps != null)
		{
			validPropsSize = validProps.Count;
		}

		int deprecatedPropsSize = 0;
		if (deprecatedProps != null)
		{
			deprecatedPropsSize = deprecatedProps.Count;
		}

		// The size of the deprecatedNotes list is computed in order to check
		// that its size is the same as the deprecatedProps size.

		bool hasNotes = false;
		int deprecatedNotesSize = 0;
		if (deprecatedNotes != null)
		{
			deprecatedNotesSize = deprecatedNotes.Count;
			hasNotes = true;
		}

		if (hasNotes && deprecatedPropsSize != deprecatedNotesSize)
		{
			throw new Exception("The number of deprecatedProps (" + deprecatedPropsSize + ") is not the same as the " + "number of deprecatedNotes (" + deprecatedNotesSize + ")");
		}

		// Default the target value.

		if (string.ReferenceEquals(target, null))
		{
			target = "property";
		}

		bool valid = false;
		Prop p = null;
		string key = null;
		string msg = null;
		string val = null;
		IList<string> warnings = new List<string>();

		// Iterate through all the properties in the PropList and check for
		// whether they are valid, invalid, or deprecated.

		IList<string> invalids = new List<string>();
		IList<string> deprecateds = new List<string>();

		string removeInvalidString = "";
		if (removeInvalid)
		{
			removeInvalidString = "  Removing invalid property.";
		}

		for (int i = 0; i < size(); i++)
		{ // Check size dynamically in case props are removed below
			p = propAt(i);
			key = p.getKey();

			valid = false;

			// First make sure that the property is in the valid property name list.
			// Properties will only be checked for whether they are deprecated if they are not valid.

			for (int j = 0; j < validPropsSize; j++)
			{
				val = validProps[j];
				if (val.Equals(key, StringComparison.OrdinalIgnoreCase))
				{
					valid = true;
					break;
				}
			}

			if (!valid)
			{
				// Only check to see if the property is in the deprecated list if it was not already found in 
				// the valid properties list.

				for (int j = 0; j < deprecatedPropsSize; j++)
				{
					val = deprecatedProps[j];
					if (val.Equals(key, StringComparison.OrdinalIgnoreCase))
					{
						msg = "\"" + key + "\" is no longer recognized as a valid " + target + "." + removeInvalidString;
						if (hasNotes)
						{
							msg += "  " + deprecatedNotes[j];
						}
						deprecateds.Add(msg);

						// Flag valid as true because otherwise this Property will also be reported
						// as an invalid property below, and that is not technically true.  Nor
						// is it technically true that the property is valid, strictly-speaking,
						// but this avoids double error messages for the same property.

						valid = true;
						break;
					}
				}
			}

			// Add the error message for invalid properties.  

			if (!valid)
			{
				invalids.Add("\"" + key + "\" is not a valid " + target + "." + removeInvalidString);
			}

			if (!valid && removeInvalid)
			{
				this.unset(i);
			}
		}

		for (int i = 0; i < invalids.Count; i++)
		{
			warnings.Add(invalids[i]);
		}

		for (int i = 0; i < deprecateds.Count; i++)
		{
			warnings.Add(deprecateds[i]);
		}

		if (warnings.Count == 0)
		{
			return null;
		}
		else
		{
			return warnings;
		}
	}

	/// <summary>
	/// Write the property list to a persistent medium (e.g., configuration file).
	/// The original persistent name is used for the filename.
	/// If the properties contain literals, then blocks will be determined from the literals and properties
	/// with starting values that match the literal will be stripped of the leading literal.  For example,
	/// App.Property = X will be written:
	/// <pre>
	/// [Block]
	/// Property = X
	/// </pre> </summary>
	/// <exception cref="IOException"> if there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writePersistent() throws java.io.IOException
	public virtual void writePersistent()
	{
		PrintWriter @out;
		try
		{
			@out = new PrintWriter(new FileStream(__persistentName, FileMode.Create, FileAccess.Write));
		}
		catch (Exception)
		{
			string message = "Unable to open \"" + __persistentName + "\" to write PropList.";
			Message.printWarning(3, "PropList.writePersistent", message);
			throw new IOException(message);
		}

		Prop p = null;
		string key = null, value = null;
		bool gotSpace;
		char c;
		int size = size();
		int length = 0;
		string blockName = null;
		for (int i = 0; i < size; i++)
		{
			p = propAt(i);
			value = p.getValue(this);
			if (p.getIsLiteral())
			{
				// TODO SAM 2008-11-21 Decide if there needs to be a parameter to control whether these
				// are written.  Presumably if they are read from a file they will typically be rewritten to the file.
				// A literal string.  Just print it.
				@out.println(value.ToString());
				// See if inside a block
				if (value.StartsWith("[", StringComparison.Ordinal))
				{
					// Have a [Block] in the properties so keep track of it
					value = value.Trim();
					blockName = value.Trim().Substring(1, (value.Length - 1) - 1);
				}
			}
			else
			{
				// A normal property.  Format it as Property = Value
				key = p.getKey();
				if ((!string.ReferenceEquals(blockName, null)) && key.StartsWith(blockName + ".", StringComparison.Ordinal))
				{
					// A block name has been detected.  Strip from the key so that the block name is not redundant.
					key = key.Substring(blockName.Length + 1); // +1 is for "."
				}
				// Do any special character encoding
				gotSpace = false;
				length = value.Length;
				for (int j = 0; j < length; j++)
				{
					c = value[j];
					if (c == ' ')
					{
						gotSpace = true;
					}
				}

				if (gotSpace)
				{
					@out.println(key + " = \"" + value.ToString() + "\"");
				}
				else
				{
					@out.println(key + " = " + value.ToString());
				}
			}
		}
		@out.close();
	}

	}

}