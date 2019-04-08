using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

// StringDictionary - dictionary string that handles the format "Key1:value1, Key2:value2, ..."

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

namespace RTi.Util.String
{

	/// <summary>
	/// Dictionary string that handles the format "Key1:value1, Key2:value2, ..."
	/// </summary>
	public class StringDictionary
	{

	/// <summary>
	/// LinkedHashMap for the dictionary, which maintains the original insert order.
	/// </summary>
	private LinkedHashMap<string, string> dict = new LinkedHashMap<string, string>();

	/// <summary>
	/// The key/value separator.
	/// </summary>
	private string keyValueSep = ":";

	/// <summary>
	/// The item separator.
	/// </summary>
	private string itemSep = ",";

	/// <summary>
	/// Construct the parser by specifying the input string, the key/value separator, and the dictionary item separator.
	/// </summary>
	public StringDictionary(string s, string keyValueSep, string itemSep)
	{
		if ((!string.ReferenceEquals(s, null)) && (s.Length > 0) && (s.IndexOf(keyValueSep, StringComparison.Ordinal) > 0))
		{
			// First break map pairs
			IList<string> pairs = StringUtil.breakStringList(s, itemSep, 0);
			// Now break pairs and put in LinkedHashMap
			foreach (string pair in pairs)
			{
				string[] parts = pair.Split(keyValueSep, true);
				if (parts.Length == 1)
				{
					this.dict.put(parts[0].Trim(), "");
				}
				else if (parts.Length > 1)
				{
					this.dict.put(parts[0].Trim(), parts[1].Trim());
				}
			}
		}
	}

	/// <summary>
	/// Get a value from the dictionary. </summary>
	/// <param name="key"> string key to look up. </param>
	public virtual string get(string key)
	{
		return this.dict.get(key);
	}

	/// <summary>
	/// Get the dictionary as a TreeMap.
	/// </summary>
	public virtual LinkedHashMap<string, string> getLinkedHashMap()
	{
		return this.dict;
	}

	/// <summary>
	/// Return the string representation of the dictionary in form "key1:value1,key2:value2".
	/// </summary>
	public override string ToString()
	{
		ISet<object> set = this.dict.entrySet();
		System.Collections.IEnumerator i = set.GetEnumerator();
		StringBuilder b = new StringBuilder();
		DictionaryEntry item;
		while (i.MoveNext())
		{
			item = (DictionaryEntry)i.Current;
			if (b.Length > 0)
			{
				b.Append(itemSep);
			}
			b.Append(item.Key + keyValueSep + item.Value);
		}
		return b.ToString();
	}

	}

}