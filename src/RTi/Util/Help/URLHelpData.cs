// URLHelpData - a single data item for on-line help class

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
// URLHelpData - a single data item for on-line help class
//------------------------------------------------------------------------------
// Copyright: See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
//
// 28 Jan 1998	Steven A. Malers,	Created initial version.
//		RTi
// 2001-11-14	SAM, RTi		Clean up javadoc.  Add finalize().
//					Set variables to null when no longer
//					used.
//------------------------------------------------------------------------------

namespace RTi.Util.Help
{

	/// <summary>
	/// This class stores one help index item, for use by URLHelp.  The item consists
	/// of a key, a topic description, and a URL.  The
	/// URLHelp.readIndex() function describes the index file format for help items. </summary>
	/// <seealso cref= URLHelp </seealso>
	public class URLHelpData
	{

	/// <summary>
	/// Key used to look up help index information.
	/// </summary>
	protected internal string _key;

	/// <summary>
	/// String to display in a help index (descriptive phrase).
	/// </summary>
	protected internal string _topic;

	/// <summary>
	/// URL corresponding to the key and topic.
	/// </summary>
	protected internal string _URL;

	/// <summary>
	/// Construct and set data to empty strings.
	/// </summary>
	public URLHelpData()
	{
		initialize();
	}

	/// <summary>
	/// Construct using the data members. </summary>
	/// <param name="key"> The key for the help index item. </param>
	/// <param name="topic"> The topic for the help index item. </param>
	/// <param name="URL"> The URL for the help index item. </param>
	public URLHelpData(string key, string topic, string URL)
	{
		initialize();
		setKey(key);
		setTopic(topic);
		setURL(URL);
	}

	/// <summary>
	/// Clean up for garbage collection. </summary>
	/// <exception cref="Throwable"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~URLHelpData()
	{
		_key = null;
		_topic = null;
		_URL = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the key for the help index item. </summary>
	/// <returns> The key for the help index item. </returns>
	public virtual string getKey()
	{
		return _key;
	}

	/// <summary>
	/// Return the topic for the help index item. </summary>
	/// <returns> The topic for the help index item. </returns>
	public virtual string getTopic()
	{
		return _topic;
	}

	/// <summary>
	/// Return the URL for the help index item. </summary>
	/// <returns> The URL for the help index item. </returns>
	public virtual string getURL()
	{
		return _URL;
	}

	/// <summary>
	/// Initialize the data.
	/// </summary>
	private void initialize()
	{
		_key = "";
		_topic = "";
		_URL = "";
	}

	/// <summary>
	/// Set the string key used to look up help. </summary>
	/// <param name="key"> The key to use for the help index item. </param>
	public virtual void setKey(string key)
	{
		if (!string.ReferenceEquals(key, null))
		{
			_key = key;
		}
	}

	/// <summary>
	/// Set the topic used for the help data item. </summary>
	/// <param name="topic"> The topic to use for the help index item. </param>
	public virtual void setTopic(string topic)
	{
		if (!string.ReferenceEquals(topic, null))
		{
			_topic = topic;
		}
	}

	/// <summary>
	/// Set the URL to use for the help data item. </summary>
	/// <param name="URL"> The URL to use for the help index item. </param>
	public virtual void setURL(string URL)
	{
		if (!string.ReferenceEquals(URL, null))
		{
			_URL = URL;
		}
	}

	/// <summary>
	/// Convert to a string representation. </summary>
	/// <returns> The string representation of the help index item. </returns>
	public override string ToString()
	{
		return "key:\"" + _key + "\" URL:\"" + _URL +
			"\" topic:\"" + _topic + "\"";
	}

	} // End URLHelpData

}