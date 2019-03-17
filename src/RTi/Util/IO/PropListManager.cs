using System;
using System.Collections.Generic;
using System.Text;

// PropListManager - manage a list of property lists

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
// PropListManager - manage a list of property lists
// ----------------------------------------------------------------------------
// Copyright: see the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// Sep 1997	Steven A. Malers	Original version.
//		Riverside Technology,
//		inc.
// 02 Feb 1998	SAM, RTi		Update so all Prop* classes work
//					together.
// 24 Feb 1998	SAM, RTi		Add javadoc comments.  Clean up some
//					code where values were being handled
//					as Object rather than String.
// 27 Apr 2001	SAM, RTi		Change all debug levels to 100.
// 10 May 2001	SAM, RTi		Add finalize().  Optimize code for
//					unused variables, loops.
// 2001-11-08	SAM, RTi		Synchronize with UNIX code... Add
//					handling of literal quotes.
// 2002-10-11	SAM, RTi		Change ProcessManager to
//					ProcessManager1.
// 2002-10-16	SAM, RTi		Revert to ProcessManager - seems to work
//					OK with cleaned up ProcessManager!
// 2003-09-26	SAM, RTi		Fix problem where parsePropString() was
//					not handling "X=" - now it sets the
//					property to a blank string.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.Util.IO
{

	using StringUtil = RTi.Util.String.StringUtil;
	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// This class manages a list of PropList objects.  It is generally only used for
	/// applications where several property lists need to be evaluated to determine the
	/// value of properties.  For example, an application may support a user
	/// configuration file, a system configuration file, and run-time user settings.
	/// Each source of properties can be stored in a separate PropList and can be
	/// managed by PropListManager.  This class has several functions that can handle
	/// recursive checks of PropLists and can expand configuration contents.
	/// </summary>
	/// <seealso cref= Prop </seealso>
	/// <seealso cref= PropList </seealso>
	public class PropListManager
	{

	internal IList<PropList> _proplists; // List of PropList objects

	/// <summary>
	/// Default constructor.
	/// </summary>
	public PropListManager()
	{
		initialize();
	}

	/// <summary>
	/// Add an existing PropList to the the list managed by this class. </summary>
	/// <param name="proplist"> The PropList to add. </param>
	/// <param name="replace_if_match"> If the name of the PropList matches one that is already
	/// in the list, replace it (true), or add the new list additionally (false). </param>
	public virtual void addList(PropList proplist, bool replace_if_match)
	{
		if (proplist == null)
		{
			return;
		}
		if (!replace_if_match)
		{
			// Always add...	
			_proplists.Add(proplist);
		}
		else
		{
			// Loop through and check names...
			int size = _proplists.Count;
			PropList proplist_pt = null;
			for (int i = 0; i < size; i++)
			{
				proplist_pt = (PropList)_proplists[i];
				if (proplist_pt == null)
				{
					continue;
				}
				if (proplist_pt.getPropListName().Equals(proplist.getPropListName(), StringComparison.OrdinalIgnoreCase))
				{
					_proplists[i] = proplist;
					return;
				}
			}
			_proplists.Add(proplist);
		}
	}

	/// <summary>
	/// Create and add a PropList to the the list managed by this class. </summary>
	/// <param name="listname"> The name of the list to add. </param>
	/// <param name="listformat"> The format of the property list to add. </param>
	public virtual int addList(string listname, int listformat)
	{
		if (string.ReferenceEquals(listname, null))
		{
			return 1;
		}
		// Allocate a new list...
		PropList list = new PropList(listname, listformat);
		// Now add it to the list...
		_proplists.Add(list);
		return 0;
	}

	/// <summary>
	/// Clean up memory for garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~PropListManager()
	{
		_proplists = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the property associated with the key. </summary>
	/// <returns> The property associated with the string key. </returns>
	/// <param name="key"> The string key to look up. </param>
	public virtual Prop getProp(string key)
	{ // For each list, search until we find the value...
		int size = _proplists.Count;
		PropList proplist;
		Prop found;
		for (int i = 0; i < size; i++)
		{
			proplist = (PropList)_proplists[i];
			if (proplist == null)
			{
				continue;
			}
			found = proplist.getProp(key);
			if (found != null)
			{
				return found;
			}
		}
		return null;
	}

	/// <summary>
	/// Return the list of PropLists managed by this PropListManager.
	/// </summary>
	public virtual IList<PropList> getPropLists()
	{
		return _proplists;
	}

	/// <summary>
	/// Return the string value of the property. </summary>
	/// <returns> The string value of the property (if configuration information, the
	/// value is fully-expanded. </returns>
	/// <param name="key"> The String key to look up. </param>
	public virtual string getValue(string key)
	{ // For each list, search until we find the value...
		int size = _proplists.Count;
		PropList proplist;
		string found;
		for (int i = 0; i < size; i++)
		{
			proplist = _proplists[i];
			if (proplist == null)
			{
				continue;
			}
			found = proplist.getValue(key);
			if (!string.ReferenceEquals(found, null))
			{
				return found;
			}
		}
		return null;
	}

	// Initialize the object...

	private int initialize()
	{
		_proplists = new List<PropList>();
		return 0;
	}

	/// <summary>
	/// Parse a property string like "Variable=Value" where the value may be an
	/// expression to be expanded.  This function is in this class (rather than
	/// PropList) because it relies on the list of PropLists to expand the value of the property string. </summary>
	/// <returns> An instance of Prop resulting from the property string. </returns>
	/// <param name="prop_string"> The property string to parse. </param>
	internal static Prop parsePropString(string prop_string)
	{
		Prop prop = null;

		if (string.ReferenceEquals(prop_string, null))
		{
			return prop;
		}
		if (prop_string.Length < 1)
		{
			return prop;
		}

		IList<string> tokens = StringUtil.breakStringList(prop_string, "=\n", StringUtil.DELIM_SKIP_BLANKS | StringUtil.DELIM_ALLOW_STRINGS);
		if (tokens == null)
		{
			return prop;
		}
		int size = tokens.Count;
		if (size < 2)
		{
			// For some reason the above is not returning 2 tokens if the
			// property is like "X=" (set to blank).  Handle here...
			if (prop_string.EndsWith("=", StringComparison.Ordinal))
			{
				tokens = new List<string>(2);
				tokens.Add(prop_string.Substring(0,(prop_string.Length - 1)));
				tokens.Add("");
			}
			else
			{
				Message.printWarning(2, "PropListManager.parsePropString", "Need at least two tokens to set a property (\"" + prop_string + "\")");
				return prop;
			}
		}

		// The variable is the first token, the contents is the remaining...

		string variable = ((string)tokens[0]).Trim();
		string contents = "";
		for (int i = 1; i < size; i++)
		{
			contents = contents + ((string)tokens[i]).Trim();
		}
		prop = new Prop(variable, (object)contents, "");
		return prop;
	}

	/// <summary>
	/// Parse a property string like "Variable=Value" where the value may be an
	/// expression to be expanded.  Use the single property list that is provided. </summary>
	/// <returns> An instance of Prop resulting from the property string. </returns>
	/// <param name="proplist"> The property list to search for properties. </param>
	/// <param name="prop_string"> The property string to parse. </param>
	internal static Prop parsePropString(PropList proplist, string prop_string)
	{
		Prop prop = null;

		// Parse the string...

		prop = parsePropString(prop_string);

		if (prop == null)
		{
			return null;
		}

		// Now evaluate the contents to get a value...

		string value = null;
		if (proplist != null)
		{
			value = resolveContentsValue(proplist, (string)prop.getContents());
		}
		else
		{
			value = (string)prop.getContents();
		}

		// Now fill in the Prop and return...

		prop.setValue(value);
		return prop;
	}

	/// <summary>
	/// Given a PropListManager and a string representation of the property contents,
	/// expand the contents to the literal property value. </summary>
	/// <returns> The property value as a string. </returns>
	/// <param name="proplist_manager"> The PropListManager to search for properties. </param>
	/// <param name="contents"> the string contents to be expanded. </param>
	public static string resolveContentsValue(PropListManager proplist_manager, string contents)
	{
		return resolveContentsValue(proplist_manager.getPropLists(), contents);
	}

	/// <summary>
	/// Given a property list and a string representation of the property contents,
	/// expand the contents to the literal property value. </summary>
	/// <returns> The property value as a string. </returns>
	/// <param name="proplist"> The proplist to search for properties. </param>
	/// <param name="contents"> The string contents to be expanded. </param>
	public static string resolveContentsValue(PropList proplist, string contents)
	{ // Use a vector with one item to look up the information...

		IList<PropList> v = new List<PropList>();
		v.Add(proplist);

		string results = resolveContentsValue(v, contents);
		return results;
	}

	// ----------------------------------------------------------------------------
	// PropListManager.resolveContentsValue - resolve the value of a variable's
	//					contents
	// ----------------------------------------------------------------------------
	// Copyright:	See the COPYRIGHT file.
	// ----------------------------------------------------------------------------
	// Notes:	(1)	This routine expects a string that contains the contents
	//			of a property variable, e.g., the right string
	//			from the following property definition:
	//
	//				myvariable = value
	//
	//		(2)	This code originally came from HMResolveConfigVariable.
	//			However, in this code the property lists have already
	//			been read into memory and can be searched without
	//			much I/O processing.
	//			This routine does not hard-code the name of the
	//			configuration file - it uses the files that are passed
	//			in and assumes that they are in the correct order to be
	//			searched (highest precedence first).
	//		(3)	This routine resolves the value for a variable by
	//			looking for its definition in the vector of PropLists
	//			that are passed in, returning the first one found.
	//		(4)	If "variable" cannot be resolved, "value" is set to an
	//			empty string.
	//		(5)	Need to get the comment value filled.  THIS DOES NOT
	//			CURRENTLY WORK.
	//		(6)	Values can be nested once:  $($(inside)_outside).
	//		(7)	Variables can be arrays.  For example:
	//
	//				myvariable[0] = value
	//				myvariable[1] = value
	//
	//			Then, if called with "myvariable[0]", the first instance
	//			is retrieved, etc.  This is useful where we want to
	//			repeat the same information in a configuration file
	//			but still use this routine to parse.
	//		(8)	Lines can be continued by making sure that \ is the
	//			last character on a line.
	// ----------------------------------------------------------------------------
	// History:
	//
	// 10 Feb 96	Steven A. Malers, RTi	Add "comment" parameter to argument
	//					list.
	// 03 Sep 96	SAM, RTi		Split code out of the HMUtil.c file
	//					and make more stand-alone.
	// 07 Oct 96	SAM, RTi		Include <string.h> and <ctype.h> to
	//					prototype functions.  Remove unused
	//					variables.
	// 18 Mar 1997	SAM, RTi		Check information set by HMSetDef first
	//					before going to environment and files.
	// 17 Apr 1997	SAM, RTi		Add ability to nest definitions, e.g.,
	//					$($(inside)_outside)
	// 19 May 1997	SAM, RTi		Add ability to handle arrays of
	//					variables.
	// 28 Jul 1997	SAM, RTi		Add ability to handle \-terminated line
	//					continuation.
	// 03 Feb 1998	SAM, RTi		Previous history is for
	//					HMResolveConfigVariable.  On this date
	//					port the code to Java.  Will probably
	//					need some cleanup over time.
	// ----------------------------------------------------------------------------
	// Variable	I/O	Description
	//
	// cchar	L	Character indicating start of a comment (blank lines
	//			are also treated as comments).
	// comment	L	Comment string associated with the variable.
	// datapt	L	Pointer to HMDataDefine global data.
	// dchar	L	Delimiter character used to separate variable and
	//			value (not counting white space).
	// dfp		L	Pointer to file.
	// file_type	I	Type of config file.
	// found_count	L	Indicates how many occurances of the variable have been
	//			found (used for array variables).
	// i		L	Position holder within record of configuration file.
	// iend		L	Limit for "i".
	// ilist	L	Loop counter for PropLists.
	// quote1	L	Quote character for strings with whitespace.
	// quote2	L	2nd valid quote character.
	// rfr_close	L	String to close reference to defined variable.
	// rfr_close_len L	Length of "rfr_close".
	// rfr_open	L	String to open reference to defined variable.
	// rfr_open_len	L	Length of "rfr_open".
	// rfr_val	L	Value of a variable that is referred back to.
	// rfr_val_nest	L	Nested reference variable value.
	// rfr_var	L	Variable that is referred back to.
	// ref_var_nest	L	Nested reference variable.
	// token	L	Token variable to look up, after breaking out array
	//			index.
	// token0	I	Token variable to look up, before breaking out array
	//			index.
	// value	O	Falue of "variable".
	// variable	O	Variable that is being looked up.
	// ----------------------------------------------------------------------------

	/// <summary>
	/// Given a vector of property lists and a string representation of the property
	/// contents, expand the contents to the literal property value. </summary>
	/// <returns> The property value as a string. </returns>
	/// <param name="proplists"> The list of property lists to search for properties. </param>
	/// <param name="contents"> The string contents to be expanded. </param>
	public static string resolveContentsValue(IList<PropList> proplists, string contents)
	{
		char cchar, hard_quote, soft_quote, syscall_quote;
		string rfr_close, rfr_open, rfr_val, rfr_val_nest, routine = "PropListManager.resolveContentsValue";
		StringBuilder rfr_var = new StringBuilder(), rfr_var_nest = new StringBuilder();
		int dl = 100, i, iend, ilist, rfr_open_len;
		bool in_soft_quote = false;

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Trying to find value for contents \"" + contents + "\"");
		}

		// Initialize variables...

		StringBuilder value = new StringBuilder();

		// Because of a technical issue, we need to assume here that we are inside soft quotes...

		in_soft_quote = true;

		// When we call this routine, we are passed the contents of a
		// property string and we are trying to expand that string here.  To
		// do so, we may need to recursively call code to search the property
		// lists again.  At this point, we don't know the variable name or
		// care whether it is an array or a single variable.

		if (proplists == null)
		{
			Message.printWarning(2, routine, "PropList vector is NULL");
			return null;
		}

		// First check the IOUtil properties...

		string ioval = IOUtil.getPropValue(contents);
		if (!string.ReferenceEquals(ioval, null))
		{
			return ioval;
		}

		// Use the old "file" notation for now until the code is working, then clean up...
		int size = proplists.Count;
		PropList proplist = null;
		bool literal_quotes = true;
		for (ilist = 0; ilist < size; ++ilist)
		{
			// Get the PropList based on the vector position...
			proplist = (PropList)proplists[ilist];
			if (proplist == null)
			{
				continue;
			}
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "Checking list \"" + proplist.ToString() + "\"");
			}

			// Set some parsing information based on the prop list...

			literal_quotes = proplist.getLiteralQuotes();

			if (proplist.getPersistentFormat() == PropList.FORMAT_NWSRFS)
			{
				rfr_close = ")";
				rfr_open = "$(";
				cchar = '#';
				hard_quote = '\'';
				soft_quote = '\"';
				syscall_quote = '`';
			}
			else if (proplist.getPersistentFormat() == PropList.FORMAT_PROPERTIES)
			{
				// Assume PropList.FORMAT_PROPERTIES
				rfr_close = "}";
				rfr_open = "${";
				cchar = '#';
				hard_quote = '\'';
				soft_quote = '\"';
				syscall_quote = '`';
			}
			else
			{
				// Assume PropList.FORMAT_MAKEFILE
				rfr_close = ")";
				rfr_open = "$(";
				cchar = '#';
				hard_quote = '\'';
				soft_quote = '\"';
				syscall_quote = '`';
			}
			rfr_open_len = rfr_open.Length;

			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "Nested definitions are surrounded by " + rfr_open + rfr_close);
			}

			// Determine the associated value.  This is done by evaluating
			// string tokens and concatenating them to the "value".
			//
			i = 0;
			iend = contents.Length - 1;
			char c;
			while (i <= iend)
			{
				// Determine contents of resource until:
				//
				// 1. Comment is found.
				// 2. End of line is reached.
				c = contents[i];
				if ((c == cchar) && !in_soft_quote)
				{
					break;
				}
				// Hard quote.  Read until the closing quote...
				if (!literal_quotes && (c == hard_quote))
				{
					// Skip quote and then add characters to resource...
					if (Message.isDebugOn)
					{
						Message.printDebug(dl, routine, "Detected start of hard quote: " + c);
					}
					i++;
					if (i > iend)
					{
						break;
					}
					c = contents[i];
	/* Need to find an isprint equivalent to test here... */
					while (c != hard_quote)
					{
						value.Append(c);
						i++;
						if (i > iend)
						{
							break;
						}
						c = contents[i];
					}
					// Skip over trailing quote...
					i++;
					if (i > iend)
					{
						break;
					}
					c = contents[i];
					continue;
				}
				if (c == syscall_quote)
				{
					// Skip quote and then form and execute a system call...
					if (Message.isDebugOn)
					{
						Message.printDebug(dl, routine, "Detected start of system call: " + c);
					}
					i++;
					if (i > iend)
					{
						break;
					}
					c = contents[i];
					StringBuilder syscall = new StringBuilder();
					while (c != syscall_quote)
					{
						syscall.Append(c);
						i++;
						if (i > iend)
						{
							break;
						}
						c = contents[i];
					}
					// Now make the system call and append...
					if (Message.isDebugOn)
					{
						Message.printDebug(dl, routine, "Making system call \"" + syscall + "\"");
					}
					IList<string> sysout = null;
					try
					{
						// Run using the full command since we don't know for sure how to tokenize,
						// but the version that takes a command array is safer...
						ProcessManager pm = new ProcessManager(syscall.ToString());
						pm.saveOutput(true);
						pm.run();
						sysout = pm.getOutputList();
						if (pm.getExitStatus() != 0)
						{
							// Return null so calling code does not assume all is well
							Message.printWarning(2, routine, "Error running \"" + syscall.ToString());
							pm = null;
							return null;
						}
						pm = null;
					}
					catch (Exception)
					{
						// Unable to run so return null so calling code does not assume all is well
						Message.printWarning(2, routine, "Error running \"" + syscall.ToString());
						return null;
					}
					// Assume one line of output is all that should be passed...
					string syscall_results = "";
					if ((sysout != null) && (sysout.Count > 0))
					{
						syscall_results = (string)sysout[0];
					}
					value.Append(syscall_results);
					sysout = null;
					syscall_results = null;
					// Now positioned on matching syscall_quote
					// Skip over trailing quote...
					i++;
					if (i > iend)
					{
						break;
					}
					c = contents[i];
					continue;
				}
				// Soft quote...
				else if (!literal_quotes && (c == soft_quote))
				{
					// This mainly just tells the rest of the code
					// to allow spaces, etc., in the string.  It
					// really has little effect other than limiting
					// the processing of comments, etc.
					// Skip over quote...
					i++;
					if (i > iend)
					{
						break;
					}
					c = contents[i];
					in_soft_quote = true;
					continue;
				}
				// Now look for any embedded references
				// to other variables...
				if ((contents.Length - i) >= rfr_open_len)
				{
					if (contents.Substring(i, (rfr_open_len)).Equals(rfr_open))
					{
					if (Message.isDebugOn)
					{
						Message.printDebug(dl, routine, "Detected variable reference");
					}
					i += rfr_open_len;
					if (i > iend)
					{
						break;
					}
					c = contents[i];
					// Now, if the next characters are the open characters we
					// need to recurse on a nested definition...
					if (contents.Substring(i, rfr_open_len).Equals(rfr_open))
					{
						if (Message.isDebugOn)
						{
							Message.printDebug(dl, routine, "Detected nesting");
						}
						i += rfr_open_len;
						if (i > iend)
						{
							break;
						}
						c = contents[i];
						while ((i <= iend) && (c != rfr_close[0]))
						{
							rfr_var_nest.Append(c);
							i++;
							if (i > iend)
							{
								break;
							}
							c = contents[i];
						}
						i++; //skip closing character
						if (i > iend)
						{
							break;
						}
						c = contents[i];
						// Now we call the routine that looks
						// up a value given the variable name.
						rfr_val_nest = resolvePropValue(proplists, rfr_var_nest.ToString());
						if (string.ReferenceEquals(rfr_val_nest, null))
						{
							// Just use a blank string...
							rfr_val_nest = "";
						}
						if (Message.isDebugOn)
						{
							Message.printDebug(dl, routine, "Nested value for \"" + rfr_var_nest + "\" is \"" + rfr_val_nest + "\"");
						}
						// Now concatenate this to the variable that we really want to look up...
						rfr_var.Append(rfr_val_nest);
					}
					// Now continue forming the recursive reference...
					while ((i <= iend) && (c != rfr_close[0]))
					{
						rfr_var.Append(c);
						i++;
						if (i > iend)
						{
							break;
						}
						c = contents[i];
					}
					i++; //skip closing character
					if (i > iend)
					{
						break;
					}
					c = contents[i];
					if (Message.isDebugOn)
					{
						Message.printDebug(dl, routine, "Recursive call to resolve \"" + rfr_var + "\"");
					}
					rfr_val = resolvePropValue(proplists, rfr_var.ToString());
					if (string.ReferenceEquals(rfr_val, null))
					{
						// Set to a blank string...
						rfr_val = "";
					}
					if (Message.isDebugOn)
					{
						Message.printDebug(dl, routine, "Recursive value for \"" + rfr_var + "\" - \"" + rfr_val + "\"");
					}
					value.Append(rfr_val);
					continue;
					}
				}
				// All other characters...
				// Add character to resource...
				value.Append(c);
				i++;
				if (i > iend)
				{
					break;
				}
				c = contents[i];
			}
			if (value.Length > 0)
			{
				// We have found a value...
				//
				// Need to cut off trailing white space...
				string value_string = value.ToString().Trim();
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, routine, "\"" + contents + "\": contents \" = \"" + value_string + "\"");
				}
				return value_string;
			}
		}
		if (value.Length < 1)
		{
			// Unable to expand the value...
			return value.ToString();
		}
		else
		{
			// Need to cut off trailing white space...
			string value_string = value.ToString().Trim();
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "\"" + proplists.ToString() + "\": contents \"" + contents + "\" = \"" + value_string + "\"");
			}
			return value_string;
		}
	}

	/// <summary>
	/// Return the string value for a property, given the property key, using the
	/// single specified property list. </summary>
	/// <returns> The string property value. </returns>
	/// <param name="list"> The single property list to check. </param>
	/// <param name="key"> The string key to look up> </param>
	public static string resolvePropValue(PropList list, string key)
	{ // Create a vector and call the routine that accepts the vector...

		IList<PropList> v = new List<PropList>();
		v.Add(list);
		string result = resolvePropValue(v, key);
		return result;
	}

	// ----------------------------------------------------------------------------
	// PropListManager.resolvePropValue -	given a vector of PropLists and a key
	//					string, return the value
	// ----------------------------------------------------------------------------
	// Notes:	(1)	The key string can be a variable name (e.g., "MYVAR" )
	//			or it can be an array (e.g., "MYVAR[0]").  This routine
	//			loops through the property lists to find the key and
	//			then resolves its contents.
	// ----------------------------------------------------------------------------
	// History:
	//
	// 03 Feb 1998	Steven A. Malers	Initial version.  The original
	//					HMResolveConfigVariable routine has
	//					been split into several routines.  This
	//					code now searches properties in
	//					memory rather than reading from files
	//					and consequently it is faster and
	//					easier to manage the lookups.
	// ----------------------------------------------------------------------------
	/// <summary>
	/// Return the string value for a property, given the property key, using the
	/// specified vector of property lists. </summary>
	/// <returns> The string property value. </returns>
	/// <param name="list"> The list of property lists to check. </param>
	/// <param name="key"> The string key to look up> </param>
	public static string resolvePropValue(IList<PropList> list, string key)
	{
		int array_index = 0, dl = 100;

		// Make sure that we have non-null data...

		if (list == null)
		{
			return null;
		}

		if (string.ReferenceEquals(key, null))
		{
			return null;
		}

		// First check the IOUtil properties...

		string ioval = IOUtil.getPropValue(key);
		if (!string.ReferenceEquals(ioval, null))
		{
			return ioval;
		}

		// See if we are looking up a normal variable or an array item...

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, "PropListManager.resolvePropValue", "Looking up \"" + key + "\" in vector of PropList");
		}

		IList<string> strings = StringUtil.breakStringList(key, "[]", StringUtil.DELIM_SKIP_BLANKS);
		int nstrings = strings.Count;
		if (nstrings >= 2)
		{
			// We have an array.  Break out the variable name and the
			// array position (zero-referenced)...
			array_index = StringUtil.atoi((string)strings[1]);
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, "PropListManager.resolvePropValue", "Array index is " + array_index);
			}
		}
		strings = null;

		// Loop through the vector searching each PropList in order...

		int found_count = 0, vsize = list.Count;
		PropList proplist = null;
		Prop prop = null;
		for (int i = 0; i < vsize; i++)
		{
			// First get the list...
			proplist = (PropList)list[i];
			if (proplist == null)
			{
				continue;
			}
			// Now loop through the items in the list...
			int psize = proplist.size();
			for (int j = 0; j < psize; j++)
			{
				prop = (Prop)proplist.propAt(j);
				if (prop.getKey().Equals(key, StringComparison.OrdinalIgnoreCase))
				{
					// We have a match.
					++found_count;
					if ((found_count - 1) == array_index)
					{
						// This is the one that we want to return...
						string result = prop.getValue();
						return result;
					}
				}
			}
		}
		return null;
	}

	/// <summary>
	/// Given a string like X=Y, resolve the value of the property.  Not implemented.
	/// </summary>
	public static object resolvePropString(PropList list, string @string)
	{
		return null;
	}

	/// <summary>
	/// Given a string like X=Y, resolve the value of the property.  Not implemented.
	/// </summary>
	public static object resolvePropVariable(PropList list, string @string)
	{
		return null;
	}

	// Set routines...

	// Need to figure out which list is used for the default?  Should use the one
	// in memory.  For now, require that the list name be specified when setting.
	// Probably can try to reset in a list if found but still need to determine the
	// list if adding.
	/*
	public int setValue ( String key, Object value )
	{
		if ( value != null ) {
			_value = value;
		}
		return 0;
	}
	*/

	/// <summary>
	/// Set the value of a property by searching the property lists </summary>
	/// <param name="listname"> The property list name to set the value in. </param>
	/// <param name="key"> The string key of the property to set. </param>
	/// <param name="contents"> The string value to set for the property. </param>
	public virtual int setValue(string listname, string key, object contents)
	{
		if (contents == null)
		{
			return 1;
		}
		// For each list, search until we find the value and then reset it...
		int size = _proplists.Count;
		PropList proplist;
		for (int i = 0; i < size; i++)
		{
			proplist = (PropList)_proplists[i];
			if (proplist == null)
			{
				continue;
			}
			if (proplist.getPropListName().Equals(listname))
			{
				// We have our list.  Set the value by calling that list's routine...
				proplist.setUsingObject(key, contents);
				return 0;
			}
		}
		// Cannot find list...
		return 1;
	}

	}

}