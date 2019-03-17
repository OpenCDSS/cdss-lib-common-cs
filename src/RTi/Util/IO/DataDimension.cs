using System;
using System.Collections.Generic;

// DataDimension - data dimension class

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
// DataDimension - data dimension class
// ----------------------------------------------------------------------------
// History:
//
// 13 Jan 1998	Steven A. Malers, RTi	Initial version.
// 13 Apr 1999	SAM, RTi		Add finalize.
// 2001-11-06	SAM, RTi		Review javadoc.  Verify that variables
//					are set to null when no longer used.
//					Change some methods to have void return
//					type.
// 2003-05-23	SAM, RTi		* Remove the internal integer dimension.
//					  Dimension data is now being read from
//					  databases like RiversideDB and the
//					  hard-coded values are difficult to
//					  keep consistent.  Put alist of
//					  "standard" dimensions in the class
//					  header documentation, as a reference.
//					* Remove the default initialization
//					  method - the data should always be
//					  initialized externally from a database
//					  or file.
//					* Change private data to use __ in
//					  front, consistent with other RTi code.
//					* Deprecate lookup() in favor of
//					  lookupDimension().
// 2003-12-04	SAM, RTi		* Add getDimensionData().
// ----------------------------------------------------------------------------

namespace RTi.Util.IO
{

	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// The DataDimension class stores data dimension data and provides methods to
	/// interpret and use such data.  Data dimensions (e.g., "L" for length,
	/// "L/T" for discharge) are primarily used when determining units conversions or
	/// labels for output.  Standard dimensions that have been used in RTi software
	/// include:
	/// <pre>
	/// DIRECTION (e.g., degrees).
	/// CONSTANT
	/// ENERGY
	/// ENERGY_PER_AREA
	/// POWER
	/// LENGTH
	/// SPEED
	/// AREA
	/// VOLUME
	/// DISCHARGE
	/// PRESSURE
	/// TEMPERATURE
	/// TIME
	/// <pre>
	/// </summary>
	public class DataDimension
	{

	// Private static data members for object house-keeping...

	private static IList<DataDimension> __dimensionList = new List<DataDimension>(20);

	// Data members...

	private string __abbreviation; // Abbreviation for dimension.  This is
						// used in data units files to group
						// units by dimension.  Example: "L"
	private string __long_name; // Long name for dimension (e.g., "LENGTH).

	/// <summary>
	/// Construct using primitive data. </summary>
	/// <param name="abbreviation"> the abbreviation to use </param>
	/// <param name="long_name"> the long_name to use </param>
	public DataDimension(string abbreviation, string long_name)
	{
		setAbbreviation(abbreviation);
		setLongName(long_name);
	}

	/// <summary>
	/// Copy constructor. </summary>
	/// <param name="dim"> DataDimension to copy. </param>
	public DataDimension(DataDimension dim) : this(dim.getAbbreviation(), dim.getLongName())
	{
	}

	/// <summary>
	/// Add a DataDimension to the internal list of dimensions.  After adding, the
	/// dimensions can be used throughout the application. </summary>
	/// <param name="dim"> Instance of DataDimension to add to the list. </param>
	public static void addDimension(DataDimension dim)
	{ // First see if the dimension is already in the list...

		int size = __dimensionList.Count;
		DataDimension pt = null;
		for (int i = 0; i < size; i++)
		{
			// Get the dimension for the loop index...
			pt = (DataDimension)__dimensionList[i];
			// Now compare...
			if (dim.getAbbreviation().Equals(pt.getAbbreviation(), StringComparison.OrdinalIgnoreCase))
			{
				// The requested dimension matches something that is
				// already in the list.  Reset the list...
				__dimensionList[i] = new DataDimension(dim);
				return;
			}
		}
		// Need to add the units to the list...
		__dimensionList.Add(new DataDimension(dim));
		pt = null;
	}

	/// <summary>
	/// Finalize before garbage collection. </summary>
	/// <exception cref="Throwable"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~DataDimension()
	{
		__abbreviation = null;
		__long_name = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the dimension abbreviation. </summary>
	/// <returns> The dimension abbreviation. </returns>
	public virtual string getAbbreviation()
	{
		return __abbreviation;
	}

	/// <summary>
	/// Return a list of DataDimension containing the static shared dimension data. </summary>
	/// <returns> a list of DataDimension containing the static shared dimension data. </returns>
	public static IList<DataDimension> getDimensionData()
	{
		return __dimensionList;
	}

	/// <summary>
	/// Return the dimension long name. </summary>
	/// <returns> The dimension long name. </returns>
	public virtual string getLongName()
	{
		return __long_name;
	}

	/// <summary>
	/// Lookup a DataDimension given the dimension string abbreviation. </summary>
	/// <returns> DataDimension given the dimension string abbreviation. </returns>
	/// <param name="dimension_string"> Dimension abbreviation string. </param>
	/// <exception cref="Exception"> If the data dimension cannot be determined from the string. </exception>
	/// @deprecated Use lookupDimension 
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static DataDimension lookup(String dimension_string) throws Exception
	public static DataDimension lookup(string dimension_string)
	{
		return lookupDimension(dimension_string);
	}

	/// <summary>
	/// Lookup a DataDimension given the dimension string abbreviation. </summary>
	/// <returns> DataDimension given the dimension string abbreviation. </returns>
	/// <param name="dimension_string"> Dimension abbreviation string. </param>
	/// <exception cref="Exception"> If the data dimension cannot be determined from the string. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static DataDimension lookupDimension(String dimension_string) throws Exception
	public static DataDimension lookupDimension(string dimension_string)
	{
		if (string.ReferenceEquals(dimension_string, null))
		{
			throw new Exception("Null dimension string");
		}
		if (dimension_string.Length <= 0)
		{
			throw new Exception("Empty dimension string");
		}

		int size = __dimensionList.Count;
		DataDimension dim = null;
		for (int i = 0; i < size; i++)
		{
			dim = (DataDimension)__dimensionList[i];
			if (dimension_string.Equals(dim.getAbbreviation(), StringComparison.OrdinalIgnoreCase))
			{
				// Have a match...
				return dim;
			}
		}
		// Unable to find...
		string message = "Unable to look up dimension \"" + dimension_string + "\"";
		Message.printWarning(2, "DataDimension.lookupDimension", message);
		throw new Exception(message);
	}

	/// <summary>
	/// Set the dimension abbreviation. </summary>
	/// <param name="abbreviation"> The dimension abbreviation. </param>
	public virtual void setAbbreviation(string abbreviation)
	{
		if (string.ReferenceEquals(abbreviation, null))
		{
			return;
		}
		__abbreviation = abbreviation;
	}

	/// <summary>
	/// Set the dimension long name. </summary>
	/// <param name="long_name"> The dimension long name. </param>
	public virtual void setLongName(string long_name)
	{
		if (string.ReferenceEquals(long_name, null))
		{
			return;
		}
		__long_name = long_name;
	}

	/// <summary>
	/// Return a string representation of the DataDimension. </summary>
	/// <returns> A string representation of the DataDimension. </returns>
	public override string ToString()
	{
		return "Dimension:  \"" + __abbreviation + "\", \"" + __long_name + "\"";
	}

	}

}