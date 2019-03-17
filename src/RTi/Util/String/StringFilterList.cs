using System.Collections.Generic;

// StringFilterList - list of string filter data to be evaluated by include/exclude checks

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
	/// List of string filter data to be evaluated by include/exclude checks.
	/// @author sam
	/// 
	/// </summary>
	public class StringFilterList
	{

		/// <summary>
		/// List of keys, for example these can be column names or properties.
		/// </summary>
		private IList<string> keys = new List<string>();

		/// <summary>
		/// List of filter patterns to match, no constraint on whether globbing or other regex.
		/// </summary>
		private IList<string> patterns = new List<string>();

		/// <summary>
		/// Constructor.
		/// </summary>
		public StringFilterList()
		{
		}

		/// <summary>
		/// Add a filter. </summary>
		/// <param name="key"> key for filter </param>
		/// <param name="pattern"> filter pattern </param>
		public virtual void add(string key, string pattern)
		{
			keys.Add(key);
			patterns.Add(pattern);
		}

		/// <summary>
		/// Return the key at the position. </summary>
		/// <param name="pos"> filter position 0+. </param>
		public virtual string getKey(int pos)
		{
			return keys[pos];
		}

		/// <summary>
		/// Return the filter pattern at the position. </summary>
		/// <param name="pos"> filter position 0+. </param>
		public virtual string getPattern(int pos)
		{
			return patterns[pos];
		}

		/// <summary>
		/// Return the size of the filter list.
		/// </summary>
		public virtual int size()
		{
			return keys.Count;
		}
	}

}