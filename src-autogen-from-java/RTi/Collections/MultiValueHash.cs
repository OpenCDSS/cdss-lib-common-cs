using System;
using System.Collections;
using System.Collections.Generic;

// MultiValueHash - class to handle multi-value hash

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

namespace RTi.Collections
{

	/// <summary>
	/// Provides a hashmap containing multiple values for a key.
	/// <para>
	/// For instance, Bob may have two phone numbers, 2143 & 1824.
	/// <br>
	/// The key would be Bob & the values 2143 & 1834
	/// 
	/// </para>
	/// </summary>
	/// <seealso cref= Map </seealso>
	 public class MultiValueHash
	 {
	  private System.Collections.IDictionary map = new Hashtable();

	  /// <summary>
	  /// Associates the specified value with the specified key in this map. </summary>
	  /// <param name="key"> key with which the specified value is to be associated </param>
	  /// <param name="value"> value to be associated with the specified key </param>
	  public virtual void put(object key, object value)
	  {
		System.Collections.IList l = (System.Collections.IList) map[key];
		 if (l == null)
		 {
		  map[key] = l = new List<object>();
		 }
		l.Add(value);
	  }

	/// <summary>
	/// Returns the value to which this map maps the specified key.
	/// <para>
	/// A return value of null is returned if the map contains no mapping for
	/// this key; it's also possible tat the map explicitely maps the key to
	/// null.
	/// 
	/// </para>
	/// </summary>
	/// <param name="key"> key whose associated value is to be returned. </param>
	/// <returns> the value to which this map maps the specified key, or null
	///  if the map contains no mapping for this key. </returns>
	  public virtual object get(object key)
	  {
		System.Collections.IList l = (System.Collections.IList) map[key];
		return l;
	  }

	/// <summary>
	/// Returns a set view of the keys contained in this map.
	/// <para>
	/// The set is backed by the map, so changes to the map are reflected in 
	/// the set, and vice-versa. 
	/// 
	/// </para>
	/// </summary>
	/// <returns> a set view of the keys contained in this map </returns>
	  public virtual ISet<object> keySet()
	  {
		return map.Keys;
	  }

		public static void Main(string[] args)
		{
		MultiValueHash mvh = new MultiValueHash();
		mvh.put("Bob","2143");
		mvh.put("Bob","1834");

		mvh.put("Sally","1234");
		mvh.put("Sally","5678");
		mvh.put("Sally","1010");

		ISet<object> set = mvh.Keys;
		Console.WriteLine("# keys: " + set.Count);
		string key;

		System.Collections.IEnumerator it = set.GetEnumerator();
		while (it.MoveNext())
		{
				 key = (string)it.Current;
				 System.Collections.IList l = (System.Collections.IList) mvh.get(key);

				  Console.WriteLine(" " + key + ": " + l);
		}

		System.Collections.IList l = (System.Collections.IList) mvh.get("Bob");

	   if (l != null)
	   {
			Console.WriteLine(l.Count + ": " + l);
	   }
		}

	 } // eof class MultiValueHash


}