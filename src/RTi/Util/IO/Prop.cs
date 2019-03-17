using System;

// Prop - use to hold an object's properties

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
// Prop - use to hold an object's properties
// ----------------------------------------------------------------------------
// Notes:	(1)	This is useful for program or other component
//			information.
//		(2)	PropList manages a list of these properties.
// ----------------------------------------------------------------------------
// History:
//
// Sep 1997?	Steven A. Malers,	Initial version.  Start dabbling to
//		Riverside Technology,	formalize update of legacy setDef/
//		inc.			getDef code.
// 02 Feb 1998	SAM, RTi		Get all the Prop* classes working
//					together and start to use in
//					production.
// 24 Feb 1998	SAM, RTi		Add the javadoc comments.
// 13 Apr 1999	SAM, RTi		Add finalize.
// 10 May 2001	SAM, RTi		Add ability to expand the property
//					contents as per makefile and RTi
//					property file.  Do so by overloading
//					getValue() to take a persistent format
//					flag to indicate that expansion should
//					be checked.  Also add refresh().
// 2002-02-03	SAM, RTi		Change long _flags integer _how_set and
//					clean up the SET_* values - use with
//					PropList's _how_set flag to streamline
//					tracking user input.  Change methods to
//					be of void type rather than return an
//					int (since the return type is of no
//					importance).
// 2004-02-19	J. Thomas Sapienza, RTi	Implements the Comparable interface so
//					that a PropList can be sorted.
// 2004-05-10	SAM, RTi		Add a "how set" option of
//					SET_AT_RUNTIME_FOR_USER.
// 2005-10-20	JTS, RTi		Added a "how set" option of 
//					SET_HIDDEN for Properties that are 
//					always behind-the-scenes, which should
//					never be saved, viewed, or known by
//					users.
// ----------------------------------------------------------------------------

namespace RTi.Util.IO
{
	/// <summary>
	/// This class provides a way to generically store property information and can
	/// be used similar to Java properties, environment variables, etc.  The main
	/// difference is that it allows the contents of a property to be an Object, which
	/// allows flexibility to use the property for anything in Java.
	/// <para>
	/// A property essentially consists of a string key and an associated object.
	/// The functions that deal with property "contents" return the literal object.
	/// The functions that deal with property "value" return a string representation of
	/// the value.  In many cases, the Object will actually be a String and therefore
	/// the contents and value will be the same (there is currently no constructor to
	/// take contents only - if it is added, then the value will be set to
	/// contents.toString() at construction).
	/// </para>
	/// <para>
	/// A property can also hold a literal string.  This will be the case when a configuration
	/// file is read and a literal comment or blank line is to be retained, to allow outputting
	/// the properties with very close to the original formatting.  In this case, the
	/// isLiteral() call will return true.
	/// </para>
	/// </summary>
	/// <seealso cref= PropList </seealso>
	/// <seealso cref= PropListManager </seealso>
	public class Prop : IComparable<Prop>
	{

	/// <summary>
	/// Indicates that it is unknown how the property was set (this is the default).
	/// </summary>
	public const int SET_UNKNOWN = 0;

	/// <summary>
	/// Indicates that the property was set from a file or database.  In this case,
	/// when a PropList is saved, the property should typically be saved.
	/// </summary>
	public const int SET_FROM_PERSISTENT = 1;

	/// <summary>
	/// Indicates that the property was set at run-time as a default value.  In this
	/// case, when a PropList is saved, the property often may be ignored because it
	/// will be set to the same default value the next time.
	/// </summary>
	public const int SET_AS_RUNTIME_DEFAULT = 2;

	/// <summary>
	/// Indicates that the property was set by the user at run-time.  In this case,
	/// when a PropList is saved, the property should likely be saved because the user
	/// has specified a value different from internal defaults.
	/// </summary>
	public const int SET_AT_RUNTIME_BY_USER = 3;

	/// <summary>
	/// Indicates that the property was automatically set for the user at run-time.  In
	/// this case, when a PropList is saved, the property should likely be saved because
	/// the user the property is considered important in defining something.  However,
	/// for all practical purposes, it is a run-time default and, in and of itself,
	/// should not force the user to save.
	/// </summary>
	public const int SET_AT_RUNTIME_FOR_USER = 4;

	/// <summary>
	/// Indicates that the property was set behind the scenes in a way that should be
	/// invisible to the user.  Users cannot edit hidden properties, will never see
	/// hidden properties, and should never be able to save hidden properties to a persistent source.
	/// </summary>
	public const int SET_HIDDEN = 5;

	/// <summary>
	/// Indicates whether property is read from a persistent source, set internally as a
	/// run-time default, or is set at runtime by the user.
	/// </summary>
	private int __howSet;
	/// <summary>
	/// Integer key for faster lookups.
	/// </summary>
	private int __intKey;
	/// <summary>
	/// Indicate whether the property is a literal string.
	/// By default the property is a normal property.
	/// </summary>
	private bool __isLiteral = false;
	/// <summary>
	/// String to look up property.
	/// </summary>
	private string __key;
	/// <summary>
	/// Contents of property (anything derived from Object).  This may be a string or another
	/// object.  If a string, it contains the value before expanding wildcards, etc.
	/// </summary>
	private object __contents;
	/// <summary>
	/// Value of the object as a string.  In most cases, the object will be a string.  The
	/// value is the fully-expanded string (wildcards and other variables are expanded).  If not
	/// a string, this may contain the toString() representation.
	/// </summary>
	private string __value;

	/// <summary>
	/// Construct a property having no key and no object (not very useful!).
	/// </summary>
	public Prop()
	{
		initialize(SET_UNKNOWN, 0, "", null, null);
	}

	/// <summary>
	/// Construct using a string key and a string. </summary>
	/// <param name="key"> String to use as key to look up property. </param>
	/// <param name="contents"> The contents of the property (in this case the same as the value. </param>
	public Prop(string key, string contents)
	{ // Contents and value are the same.
		initialize(SET_UNKNOWN, 0, key, contents, contents);
	}

	/// <summary>
	/// Construct using a string key, and both contents and string value. </summary>
	/// <param name="key"> String to use as key to look up property. </param>
	/// <param name="contents"> The contents of the property (in this case the same as the </param>
	/// <param name="value"> The value of the property as a string. </param>
	public Prop(string key, object contents, string value)
	{ // Contents and string are different...
		initialize(SET_UNKNOWN, 0, key, contents, value);
	}

	/// <summary>
	/// Construct using a string key, and both contents and string value. </summary>
	/// <param name="key"> String to use as key to look up property. </param>
	/// <param name="contents"> The contents of the property (in this case the same as the </param>
	/// <param name="value"> The value of the property as a string. </param>
	/// <param name="howSet"> Indicates how the property is being set. </param>
	public Prop(string key, object contents, string value, int howSet)
	{ // Contents and string are different...
		initialize(howSet, 0, key, contents, value);
	}

	/// <summary>
	/// Construct using a string key, an integer key, and string contents. </summary>
	/// <param name="key"> String to use as key to look up property. </param>
	/// <param name="intkey"> Integer to use to look up the property (integer keys can be used
	/// in place of strings for lookups). </param>
	/// <param name="contents"> The contents of the property (in this case the same as the </param>
	public Prop(string key, int intkey, string contents)
	{
		initialize(SET_UNKNOWN, intkey, key, contents, contents);
	}

	/// <summary>
	/// Construct using a string key, an integer key, and both contents and value. </summary>
	/// <param name="key"> String to use as key to look up property. </param>
	/// <param name="intKey"> Integer to use to look up the property (integer keys can be used in place of strings for lookups). </param>
	/// <param name="contents"> The contents of the property. </param>
	/// <param name="value"> The string value of the property. </param>
	public Prop(string key, int intKey, object contents, string value)
	{
		initialize(SET_UNKNOWN, intKey, key, contents, value);
	}

	/// <summary>
	/// Construct using a string key, an integer key, and both contents and value. </summary>
	/// <param name="key"> String to use as key to look up property. </param>
	/// <param name="intKey"> Integer to use to look up the property (integer keys can be used in place of strings for lookups). </param>
	/// <param name="contents"> The contents of the property. </param>
	/// <param name="value"> The string value of the property. </param>
	/// <param name="howSet"> Indicates how the property is being set. </param>
	public Prop(string key, int intKey, object contents, string value, int howSet)
	{
		initialize(howSet, intKey, key, contents, value);
	}

	/// <summary>
	/// Construct using a string key, an integer key, string contents, and specify modifier flags. </summary>
	/// <param name="key"> String to use as key to look up property. </param>
	/// <param name="intKey"> Integer to use to look up the property (integer keys can be used in place of strings for lookups). </param>
	/// <param name="contents"> The contents of the property (in this case the same as the value. </param>
	/// <param name="howSet"> Indicates how the property is being set (see SET_*). </param>
	public Prop(string key, int intKey, string contents, int howSet)
	{
		initialize(howSet, intKey, key, contents, contents);
	}

	/// <summary>
	/// Used to compare this Prop to another Prop in order to sort them.  Inherited from Comparable interface. </summary>
	/// <param name="o"> the Prop to compare against. </param>
	/// <returns> 0 if the Props' keys and values are the same, or -1 if this Prop sorts
	/// earlier than the other Prop, or 1 if this Prop sorts higher than the other Prop. </returns>
	public virtual int CompareTo(Prop p)
	{
		int result = 0;

		result = __key.CompareTo(p.getKey());
		if (result != 0)
		{
			return result;
		}

		result = __value.CompareTo(p.getValue());
		return result;
	}

	/// <summary>
	/// Finalize before garbage collection. </summary>
	/// <exception cref="Throwable"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~Prop()
	{
		__key = null;
		__contents = null;
		__value = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the contents (Object) for the property. </summary>
	/// <returns> The contents (Object) for the property (note: the original is returned, not a copy). </returns>
	public virtual object getContents()
	{
		return __contents;
	}

	/// <summary>
	/// Return the way that the property was set (see SET_*). </summary>
	/// <returns> the way that the property was set. </returns>
	public virtual int getHowSet()
	{
		return __howSet;
	}

	/// <summary>
	/// Return the integer key for the property. </summary>
	/// <returns> The integer key for the property. </returns>
	public virtual int getIntKey()
	{
		return __intKey;
	}

	/// <summary>
	/// Return whether the property is a literal string.  If true the string contents can be
	/// output as is to represent the persistent format of the data, without the key.  For example, the
	/// property might be Literal1 = "# Some comment in the file.". </summary>
	/// <returns> true if the property is a literal string, false if not. </returns>
	public virtual bool getIsLiteral()
	{
		return __isLiteral;
	}

	/// <summary>
	/// Return the string key for the property. </summary>
	/// <returns> The string key for the property. </returns>
	public virtual string getKey()
	{
		return __key;
	}

	/// <summary>
	/// Return the string value for the property. </summary>
	/// <returns> The string value for the property. </returns>
	public virtual string getValue()
	{
		return __value;
	}

	/// <summary>
	/// Return the string value for the property expanding the contents if necessary. </summary>
	/// <param name="props"> PropList to search. </param>
	/// <returns> The string value for the property. </returns>
	public virtual string getValue(PropList props)
	{ // This will expand contents if necessary...
		refresh(props);
		return __value;
	}

	/// <summary>
	/// Initialize member data.
	/// </summary>
	private void initialize(int howSet, int intKey, string key, object contents, string value)
	{
		__howSet = howSet;
		__intKey = intKey;
		if (string.ReferenceEquals(key, null))
		{
			__key = "";
		}
		else
		{
			__key = key;
		}
		__contents = contents;
		if (string.ReferenceEquals(value, null))
		{
			__value = "";
		}
		else
		{
			__value = value;
		}
	}

	/// <summary>
	/// Refresh the contents by resetting the value by expanding the contents. </summary>
	/// <param name="props"> PropList to search. </param>
	/// <returns> The string value for the property. </returns>
	public virtual void refresh(PropList props)
	{
		int persistent_format = props.getPersistentFormat();
		 if ((persistent_format == PropList.FORMAT_MAKEFILE) || (persistent_format == PropList.FORMAT_NWSRFS) || (persistent_format == PropList.FORMAT_PROPERTIES))
		 {
			// Try to expand the contents...
			if (__contents is string)
			{
				__value = PropListManager.resolveContentsValue(props,(string)__contents);
			}
		 }
	}

	/// <summary>
	/// Set the contents for a property. </summary>
	/// <param name="contents"> The contents of a property as an Object. </param>
	public virtual void setContents(object contents)
	{ // Use a reference here (do we need a copy?)...

		if (contents != null)
		{
			__contents = contents;
		}
	}

	/// <summary>
	/// Set how the property is being set (see SET_*).
	/// Set how the property is being set.
	/// </summary>
	public virtual void setHowSet(int how_set)
	{
		__howSet = how_set;
	}

	/// <summary>
	/// Set the integer key for the property.  This is usually maintained by PropList. </summary>
	/// <param name="intkey"> Integer key for the property. </param>
	/// <seealso cref= PropList </seealso>
	public virtual void setIntKey(int intkey)
	{
		__intKey = intkey;
	}

	/// <summary>
	/// Indicate whether the property is a literal string. </summary>
	/// <param name="isLiteral"> true if the property is a literal string, false if a normal property. </param>
	public virtual void setIsLiteral(bool isLiteral)
	{
		__isLiteral = isLiteral;
	}

	/// <summary>
	/// Set the string key for the property. </summary>
	/// <param name="key"> String key for the property. </param>
	public virtual void setKey(string key)
	{
		if (!string.ReferenceEquals(key, null))
		{
			__key = key;
		}
	}

	/// <summary>
	/// Set the string value for the property. </summary>
	/// <param name="value"> The string value for the property. </param>
	public virtual void setValue(string value)
	{
		if (!string.ReferenceEquals(value, null))
		{
			__value = value;
		}
	}

	/// <summary>
	/// Return a string representation of the property (a verbose output). </summary>
	/// <returns> a string representation of the property. </returns>
	public override string ToString()
	{
		if (getIsLiteral())
		{
			return __value;
		}
		else
		{
			return "Key=\"" + __key + "\" (" + __intKey + "), value = \"" + __value + "\" _how_set= " + __howSet;
		}
	}

	}

}