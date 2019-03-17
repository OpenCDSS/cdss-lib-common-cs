using System;
using System.Collections.Generic;

// TSUtil_SortTimeSeries - sort time series.

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

namespace RTi.TS
{

	using MathUtil = RTi.Util.Math.MathUtil;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// Sort time series.
	/// </summary>
	public class TSUtil_SortTimeSeries
	{

	/// <summary>
	/// Time series to sort.
	/// </summary>
	private IList<TS> tslist = null;

	/// <summary>
	/// How to get TSID to sort ("TSID" or "AliasTSID").
	/// </summary>
	private string tsidFormat = null;

	/// <summary>
	/// Time series property to sort.
	/// </summary>
	private string property = null;

	/// <summary>
	/// Time series property format for sorting.
	/// </summary>
	private string propertyFormat = null;

	/// <summary>
	/// Time series sort order, -1 for descending and 1 for ascending.
	/// </summary>
	private int sortOrder = 1;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="tslist"> Time series to process. </param>
	/// <param name="tsidFormat"> how to get TSID, either TSID or AliasTSID. </param>
	/// <param name="property"> time series property to sort </param>
	/// <param name="propertyFormat"> time series property format for sorting, using C-style %s, etc. </param>
	/// <param name="sortOrder"> sort order, -1 descending or 1 ascending. </param>
	public TSUtil_SortTimeSeries(IList<TS> tslist, string tsidFormat, string property, string propertyFormat, int sortOrder)
	{
		this.tslist = tslist;
		this.tsidFormat = tsidFormat;
		this.property = property;
		this.propertyFormat = propertyFormat;
		this.sortOrder = sortOrder;
	}

	/// <summary>
	/// Sort the time series as per the constructor parameters.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.util.List<TS> sortTimeSeries() throws Exception
	public virtual IList<TS> sortTimeSeries()
	{
		IList<TS> tslist = this.tslist;

		if ((tslist == null) || (tslist.Count == 0))
		{
			return tslist;
		}
		int order = StringUtil.SORT_ASCENDING;
		if (this.sortOrder < 0)
		{
			order = StringUtil.SORT_DESCENDING;
		}
		// Since TS does not implement Comparable, sort the TSIdent strings...
		int size = tslist.Count;
		if ((!string.ReferenceEquals(this.property, null)) && !this.property.Equals(""))
		{
			// Sort using a specific time series property
			// First determine whether the type of property is consistent
			// If so, sort using the native format so number sort OK.
			// If not, convert to strings to sort
			// Nulls are treated as small values
			TS ts = null;
			int intCount = 0;
			int doubleCount = 0;
			int stringCount = 0;
			int unknownCount = 0;
			int nullCount = 0;
			object propVal;
			for (int i = 0; i < size; i++)
			{
				ts = tslist[i];
				if (ts == null)
				{
					++nullCount;
				}
				else
				{
					propVal = ts.getProperty(this.property);
					if (propVal == null)
					{
						++nullCount;
					}
					else if ((propVal is float?) || (propVal is double?))
					{
						++doubleCount;
					}
					else if ((propVal is int?) || (propVal is long?))
					{
						++intCount;
					}
					else if (propVal is string)
					{
						++stringCount;
					}
					else
					{
						++unknownCount;
					}
				}
			}
			if ((doubleCount + nullCount) == size)
			{
				// Sorting floating point numbers
				double[] doubles = new double[size];
				// Sort doubles
				object o;
				for (int i = 0; i < size; i++)
				{
					ts = tslist[i];
					if (ts == null)
					{
						// Set to smallest double value
						doubles[i] = double.Epsilon;
					}
					else
					{
						o = ts.getProperty(this.property);
						if (o == null)
						{
							doubles[i] = double.Epsilon;
						}
						else
						{
							// Check on counts should have determined all Doubles or nulls so cast should be safe
							doubles[i] = (double?)o.Value;
						}
					}
				}
				int[] sortOrder = new int[size];
				// Get the sorted order...
				MathUtil.sort(doubles, MathUtil.SORT_QUICK, order, sortOrder, true); // Use sort array
				IList<TS> tslistSorted = new List<TS>(size);
				for (int i = 0; i < size; i++)
				{
					tslistSorted.Add(tslist[sortOrder[i]]);
				}
				return tslistSorted;
			}
			else if ((intCount + nullCount) == size)
			{
				// Sorting integer numbers
				long[] integers = new long[size];
				// Sort integers
				for (int i = 0; i < size; i++)
				{
					ts = tslist[i];
					if (ts == null)
					{
						// Set to smallest integer value
						integers[i] = int.MinValue;
					}
					else
					{
						object o = ts.getProperty(this.property);
						if (o == null)
						{
							integers[i] = int.MinValue;
						}
						else
						{
							// Check on counts should have determined all Integers or nulls so cast should be safe
							integers[i] = (int?)o;
						}
					}
				}
				int[] sortOrder = new int[size];
				// Get the sorted order...
				MathUtil.sort(integers, MathUtil.SORT_QUICK, order, sortOrder, true); // Use sort array
				IList<TS> tslistSorted = new List<TS>(size);
				for (int i = 0; i < size; i++)
				{
					tslistSorted.Add(tslist[sortOrder[i]]);
				}
				return tslistSorted;
			}
			else
			{
				// Sorting strings
				IList<string> strings = new List<string>(size);
				// Sort by formatting a property string
				for (int i = 0; i < size; i++)
				{
					ts = tslist[i];
					if (ts == null)
					{
						strings.Add("");
					}
					else
					{
						object o = ts.getProperty(this.property);
						if (o == null)
						{
							strings.Add("");
						}
						else
						{
							// TODO SAM 2014-05-12 This may have problems with floating point numbers not formatting nicely (e.g., scientific notation)
							strings.Add("" + o);
						}
					}
				}
				int[] sortOrder = new int[size];
				// Get the sorted order...
				StringUtil.sortStringList(strings, order, sortOrder, true, true); // Ignore case.
				IList<TS> tslistSorted = new List<TS>(size);
				for (int i = 0; i < size; i++)
				{
					tslistSorted.Add(tslist[sortOrder[i]]);
				}
				return tslistSorted;
			}
		}
		else if ((!string.ReferenceEquals(this.propertyFormat, null)) && !this.propertyFormat.Equals(""))
		{
			IList<string> strings = new List<string>(size);
			// Sort by formatting a property string
			TS ts = null;
			for (int i = 0; i < size; i++)
			{
				ts = tslist[i];
				if (ts == null)
				{
					strings.Add("");
				}
				else
				{
					strings.Add(ts.formatLegend(this.propertyFormat));
				}
			}
			int[] sortOrder = new int[size];
			// Get the sorted order...
			StringUtil.sortStringList(strings, order, sortOrder, true, true); // Ignore case.
			// Now sort the time series...
			IList<TS> tslistSorted = new List<TS>(size);
			for (int i = 0; i < size; i++)
			{
				tslistSorted.Add(tslist[sortOrder[i]]);
			}
			return tslistSorted;
		}
		else
		{
			// Default is to sort by the Alias and/or TSID
			bool tryAliasFirst = false;
			if ((!string.ReferenceEquals(this.tsidFormat, null)) && this.tsidFormat.Equals("AliasTSID", StringComparison.OrdinalIgnoreCase))
			{
				tryAliasFirst = true;
			}
			TSIdent tsid;
			IList<string> strings = new List<string>(size);
			TS ts = null;
			for (int i = 0; i < size; i++)
			{
				ts = tslist[i];
				if (tryAliasFirst)
				{
					// Use the alias if non-null and non-blank
					string alias = ts.getAlias();
					if ((!string.ReferenceEquals(alias, null)) && !alias.Equals(""))
					{
						strings.Add(alias);
						continue;
					}
				}
				if (ts == null)
				{
					strings.Add("");
					continue;
				}
				tsid = ts.getIdentifier();
				if (tsid == null)
				{
					strings.Add("");
					continue;
				}
				// Use the full identifier...
				strings.Add(tsid.ToString(true));
			}
			int[] sortOrder = new int[size];
			// Get the sorted order...
			StringUtil.sortStringList(strings, order, sortOrder, true, true); // Ignore case.
			// Now sort the time series...
			IList<TS> tslistSorted = new List<TS>(size);
			for (int i = 0; i < size; i++)
			{
				tslistSorted.Add(tslist[sortOrder[i]]);
			}
			return tslistSorted;
		}
	}

	}

}