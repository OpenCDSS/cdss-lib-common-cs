using System;
using System.Collections.Generic;

// DataTableFilter - filter to determine whether table rows that are being processed match filter criteria

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

namespace RTi.Util.Table
{

	using Message = RTi.Util.Message.Message;
	using StringDictionary = RTi.Util.String.StringDictionary;

	/// <summary>
	/// This class provides a filter to determine whether table rows that are being processed match filter criteria.
	/// </summary>
	public class DataTableFilter
	{

	/// <summary>
	/// Table to be filtered.
	/// </summary>
	private DataTable table = null;

	/// <summary>
	/// Table column numbers for include filter columns.
	/// </summary>
	private int[] columnIncludeFiltersNumbers = new int[0];

	/// <summary>
	/// Glob (*) patterns to match include filter columns.
	/// </summary>
	private string[] columnIncludeFiltersGlobs = null;

	/// <summary>
	/// Table column numbers for exclude filter columns.
	/// </summary>
	private int[] columnExcludeFiltersNumbers = new int[0];

	/// <summary>
	/// Glob (*) patterns to match exclude filter columns.
	/// </summary>
	private string[] columnExcludeFiltersGlobs = null;

	/// <summary>
	/// Constructor for StringDictionaries.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public DataTableFilter(DataTable table, RTi.Util.String.StringDictionary columnIncludeFilters, RTi.Util.String.StringDictionary columnExcludeFilters) throws InvalidTableColumnException
	public DataTableFilter(DataTable table, StringDictionary columnIncludeFilters, StringDictionary columnExcludeFilters)
	{
		this.table = table;
		// Get include filter columns and glob-style regular expressions
		if (columnIncludeFilters != null)
		{
			LinkedHashMap<string, string> map = columnIncludeFilters.getLinkedHashMap();
			this.columnIncludeFiltersNumbers = new int[map.size()];
			this.columnIncludeFiltersGlobs = new string[map.size()];
			int ikey = -1;
			string key = null;
			foreach (KeyValuePair<string, string> entry in map.entrySet())
			{
				++ikey;
				this.columnIncludeFiltersNumbers[ikey] = -1;
				try
				{
					key = entry.Key;
					this.columnIncludeFiltersNumbers[ikey] = table.getFieldIndex(key);
					this.columnIncludeFiltersGlobs[ikey] = map.get(key);
					// Turn default globbing notation into internal Java regex notation
					this.columnIncludeFiltersGlobs[ikey] = this.columnIncludeFiltersGlobs[ikey].Replace("*", ".*").ToUpper();
				}
				catch (Exception)
				{
					throw new InvalidTableColumnException("ColumnIncludeFilters column \"" + key + "\" not found in table \"" + table.getTableID() + "\"");
				}
			}
		}
		// Get exclude filter columns and glob-style regular expressions
		if (columnExcludeFilters != null)
		{
			LinkedHashMap<string, string> map = columnExcludeFilters.getLinkedHashMap();
			this.columnExcludeFiltersNumbers = new int[map.size()];
			this.columnExcludeFiltersGlobs = new string[map.size()];
			int ikey = -1;
			string key = null;
			foreach (KeyValuePair<string, string> entry in map.entrySet())
			{
				++ikey;
				this.columnExcludeFiltersNumbers[ikey] = -1;
				try
				{
					key = entry.Key;
					this.columnExcludeFiltersNumbers[ikey] = table.getFieldIndex(key);
					this.columnExcludeFiltersGlobs[ikey] = map.get(key);
					// Turn default globbing notation into internal Java regex notation
					this.columnExcludeFiltersGlobs[ikey] = this.columnExcludeFiltersGlobs[ikey].Replace("*", ".*").ToUpper();
					Message.printStatus(2,"","Exclude filter column \"" + key + "\" [" + this.columnExcludeFiltersNumbers[ikey] + "] glob \"" + this.columnExcludeFiltersGlobs[ikey] + "\"");
				}
				catch (Exception)
				{
					throw new InvalidTableColumnException("ColumnExcludeFilters column \"" + key + "\" not found in table \"" + table.getTableID() + "\"");
				}
			}
		}
	}

	/// <summary>
	/// Determine whether a row should be included in processing because it matches the include and exclude filters. </summary>
	/// <param name="irow"> row index (0+) to check for inclusion </param>
	/// <param name="throwExceptions"> if true, throw exceptions when table data cannot be checked for some reason </param>
	public virtual bool includeRow(int irow, bool throwExceptions)
	{
		DataTable table = this.table;

		bool filterMatches = true; // Default is match
		object o;
		string s;
		if (this.columnIncludeFiltersNumbers.Length > 0)
		{
			// Filters can be done on any columns so loop through to see if row matches
			for (int icol = 0; icol < this.columnIncludeFiltersNumbers.Length; icol++)
			{
				if (this.columnIncludeFiltersNumbers[icol] < 0)
				{
					filterMatches = false;
					break;
				}
				try
				{
					o = table.getFieldValue(irow, this.columnIncludeFiltersNumbers[icol]);
					if (o == null)
					{
						filterMatches = false;
						break; // Don't include nulls when checking values
					}
					s = ("" + o).ToUpper();
					if (!s.matches(this.columnIncludeFiltersGlobs[icol]))
					{
						// A filter did not match so don't copy the record
						filterMatches = false;
						break;
					}
				}
				catch (Exception e)
				{
					if (throwExceptions)
					{
						throw new Exception("Error getting table data for [" + irow + "][" + this.columnIncludeFiltersNumbers[icol] + "] uniquetempvar.");
					}
				}
			}
			if (!filterMatches)
			{
				// Skip the record.
				return false;
			}
		}
		if (this.columnExcludeFiltersNumbers.Length > 0)
		{
			int matchesCount = 0;
			// Filters can be done on any columns so loop through to see if row matches
			for (int icol = 0; icol < this.columnExcludeFiltersNumbers.Length; icol++)
			{
				if (this.columnExcludeFiltersNumbers[icol] < 0)
				{
					// Can't do filter so don't try
					break;
				}
				try
				{
					o = table.getFieldValue(irow, this.columnExcludeFiltersNumbers[icol]);
					//Message.printStatus(2,"","Got cell object " + o );
					if (o == null)
					{
						if (this.columnExcludeFiltersGlobs[icol].Length == 0)
						{
							// Trying to match blank cells
							++matchesCount;
						}
						else
						{ // Don't include nulls when checking values
							break;
						}
					}
					s = ("" + o).ToUpper();
					//Message.printStatus(2,"","Comparing table value \"" + s + "\" with exclude filter \"" + columnExcludeFiltersGlobs[icol] + "\"");
					if (s.matches(this.columnExcludeFiltersGlobs[icol]))
					{
						// A filter matched so don't copy the record
						//Message.printStatus(2,"","Exclude filter matches");
						++matchesCount;
					}
				}
				catch (Exception e)
				{
					if (throwExceptions)
					{
						throw new Exception("Error getting table data for [" + irow + "][" + this.columnExcludeFiltersNumbers[icol] + "] uniquetempvar.");
					}
				}
			}
			//Message.printStatus(2,"","matchesCount=" + matchesCount + " excludeFiltersLength=" +  columnExcludeFiltersNumbers.length );
			if (matchesCount == this.columnExcludeFiltersNumbers.Length)
			{
				// Skip the record since all exclude filters were matched
				//Message.printStatus(2,"","Skipping since all exclude filters matched");
				return false;
			}
		}
		return filterMatches;
	}

	}

}