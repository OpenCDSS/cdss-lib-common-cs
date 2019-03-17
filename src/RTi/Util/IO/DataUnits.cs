using System;
using System.Collections.Generic;
using System.IO;

// DataUnits - class to provide capabilities for reading and storing data units and conversion between units

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

namespace RTi.Util.IO
{

	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// The DataUnits class provides capabilities for reading and storing 
	/// data units and conversion between units.  Units are maintained internally using a list of DataUnits.
	/// </summary>
	public class DataUnits
	{

	/// <summary>
	/// Indicates that the units system is unknown.
	/// </summary>
	public const int SYSTEM_UNKNOWN = 0;
	/// <summary>
	/// Indicates that the units are for the English System.
	/// </summary>
	public const int SYSTEM_ENGLISH = 1;
	/// <summary>
	/// Indicates that the units are for the International System.
	/// </summary>
	public const int SYSTEM_SI = 2;
	/// <summary>
	/// Indicates that the units are for both English and International System.
	/// </summary>
	public const int SYSTEM_ALL = 3;

	// Data members...

	/// <summary>
	/// The units abbreviation (e.g., "AF").
	/// </summary>
	private string __abbreviation;
	/// <summary>
	/// The long name (e.g., "ACRE-FOOT").
	/// </summary>
	private string __long_name;
	/// <summary>
	/// The dimension (e.g., "L3").
	/// </summary>
	private DataDimension __dimension;
	/// <summary>
	/// Indicates whether it the base unit in the dimension.
	/// </summary>
	private int __base_flag;
	/// <summary>
	/// The number of digits of precision after the decimal point on output.
	/// </summary>
	private int __output_precision;
	/// <summary>
	/// Units system (SYSTEM_SI, SYSTEM_ENGLISH, SYSTEM_ALL, or SYSTEM_UNKNOWN).
	/// </summary>
	private int __system;
	/// <summary>
	/// Multiplier for conversion (relative to base).
	/// </summary>
	private double __mult_factor;
	/// <summary>
	/// Add factor for conversion (relative to base).
	/// </summary>
	private double __add_factor;
	/// <summary>
	/// Behavior flag (e.g., whether to output in upper-case).
	/// </summary>
	private int __behavior_mask;
	/// <summary>
	/// Note indicating source of the data units.
	/// </summary>
	private string __source;

	/// <summary>
	/// List of internally-maintained available units, make sure to be non-null.
	/// </summary>
	private static IList<DataUnits> __units_Vector = new List<DataUnits>(20);

	/// <summary>
	/// Construct and set all data members to empty strings and zeros.
	/// </summary>
	public DataUnits()
	{
		initialize();
	}

	/// <summary>
	/// Construct using the individual data items.  The data source is set to an empty string. </summary>
	/// <param name="dimension"> Units dimension (see DataDimension). </param>
	/// <param name="base_flag"> 1 if the units are the base units for conversion purposes, for the dimension. </param>
	/// <param name="abbreviation"> Abbreviation for the units. </param>
	/// <param name="long_name"> Long name for the units. </param>
	/// <param name="output_precision"> The output precision for the units (the number of
	/// digits output after the decimal point). </param>
	/// <param name="mult_factor"> Multiplication factor used when converting to the base units for the dimension. </param>
	/// <param name="add_factor"> Addition factor used when converting to the base units for the dimension. </param>
	/// <seealso cref= DataDimension </seealso>
	public DataUnits(string dimension, int base_flag, string abbreviation, string long_name, int output_precision, double mult_factor, double add_factor) : this(dimension, base_flag, abbreviation, long_name, output_precision, mult_factor, add_factor, "")
	{
	}

	/// <summary>
	/// Construct using the individual data items. </summary>
	/// <param name="dimension"> Units dimension (see DataDimension). </param>
	/// <param name="base_flag"> 1 if the units are the base units for conversion purposes, for the dimension. </param>
	/// <param name="abbreviation"> Abbreviation for the units. </param>
	/// <param name="long_name"> Long name for the units. </param>
	/// <param name="output_precision"> The output precision for the units (the number of
	/// digits output after the decimal point). </param>
	/// <param name="mult_factor"> Multiplication factor used when converting to the base units for the dimension. </param>
	/// <param name="add_factor"> Addition factor used when converting to the base units for the dimension. </param>
	/// <param name="source"> note about the source of the data units, useful for troubleshooting conflicts or limitations
	/// in the data units definitions. </param>
	/// <seealso cref= DataDimension </seealso>
	public DataUnits(string dimension, int base_flag, string abbreviation, string long_name, int output_precision, double mult_factor, double add_factor, string source)
	{
		initialize();
		try
		{
			setDimension(dimension);
		}
		catch (Exception)
		{
			// Do nothing for now.
		}
		__base_flag = base_flag;
		setAbbreviation(abbreviation);
		setLongName(long_name);
		__output_precision = output_precision;
		__mult_factor = mult_factor;
		__add_factor = add_factor;
		setSource(source);
	}

	/// <summary>
	/// Copy constructor. </summary>
	/// <param name="units"> Instance of DataUnits to copy. </param>
	public DataUnits(DataUnits units)
	{
		initialize();
		setAbbreviation(units.__abbreviation);
		setLongName(units.__long_name);
		try
		{
			// Converts to integer, etc.
			setDimension(units.__dimension.getAbbreviation());
		}
		catch (Exception e)
		{
			string routine = "DataUnits";
			Message.printWarning(3, routine, "Error copying units.");
			Message.printWarning(3, routine, e);
			// Do nothing for now...
		}
		__base_flag = units.__base_flag;
		__output_precision = units.__output_precision;
		__system = units.__system;
		__mult_factor = units.__mult_factor;
		__add_factor = units.__add_factor;
		__behavior_mask = units.__behavior_mask;
	}

	/// <summary>
	/// Add a set of units to the internal list of units.  After adding, the units can
	/// be used throughout the application. </summary>
	/// <param name="units"> Instance of DataUnits to add to the list. </param>
	public static void addUnits(DataUnits units)
	{ // First see if the units are already in the list...

		int size = __units_Vector.Count;
		DataUnits pt = null;
		for (int i = 0; i < size; i++)
		{
			// Get the units for the loop index...
			pt = __units_Vector[i];
			// Now compare...
			if (units.getAbbreviation().Equals(pt.getAbbreviation(), StringComparison.OrdinalIgnoreCase))
			{
				// The requested units match something that is already in the list.  Reset the list...
				__units_Vector[i] = units;
				return;
			}
		}
		// Need to add the units to the list...
		__units_Vector.Add(units);
	}

	/// <summary>
	/// Determine whether a list of units strings are compatible.
	/// The units are allowed to be different as long as they are within the same
	/// dimension (e.g., each is a length).
	/// If it is necessary to guarantee that the units are exactly the same, call the
	/// version of this method that takes the boolean flag. </summary>
	/// <param name="units_strings"> list of units strings. </param>
	public static bool areUnitsStringsCompatible(IList<string> units_strings)
	{
		return areUnitsStringsCompatible(units_strings, false);
	}

	/// <summary>
	/// Determine whether a two units strings are compatible.
	/// The units are allowed to be different as long as they are within the same dimension (e.g., each is a length). </summary>
	/// <param name="units_string1"> First units strings. </param>
	/// <param name="units_string2"> Second units strings. </param>
	/// <param name="require_same"> Flag indicating whether the units must exactly match (no
	/// conversion necessary).  If true, the units must be the same.  If false, the
	/// units must only be in the same dimension (e.g., "CFS" and "GPM" would be compatible). </param>
	public static bool areUnitsStringsCompatible(string units_string1, string units_string2, bool require_same)
	{
		IList<string> units_strings = new List<string>(2);
		units_strings.Add(units_string1);
		units_strings.Add(units_string2);
		bool result = areUnitsStringsCompatible(units_strings, require_same);
		return result;
	}

	/// <summary>
	/// Determine whether a list of units strings are compatible. </summary>
	/// <param name="units_strings"> list of units strings. </param>
	/// <param name="require_same"> Flag indicating whether the units must exactly match (no
	/// conversion necessary).  If true, the units must be the same, either in
	/// spelling or have the a conversion factor of unity.  If false, the
	/// units must only be in the same dimension (e.g., "CFS" and "GPM" would be compatible). </param>
	public static bool areUnitsStringsCompatible(IList<string> units_strings, bool require_same)
	{
		if (units_strings == null)
		{
			// No units.  Decide later whether to throw an exception.
			return true;
		}
		int size = units_strings.Count;
		if (size < 2)
		{
			// No need to compare...
			return true;
		}
		string units1 = units_strings[0];
		if (string.ReferenceEquals(units1, null))
		{
			return true;
		}
		string units2;
		// Allow nulls because it is assumed that later they will result in an ignored conversion...
		DataUnitsConversion conversion = null;
		for (int i = 1; i < size; i++)
		{
			units2 = units_strings[i];
			if (string.ReferenceEquals(units2, null))
			{
				continue;
			}
			// Get the conversions and return false if a conversion cannot be obtained...
			try
			{
				conversion = getConversion(units1, units2);
				if (require_same)
				{
					// If the factors are not unity, return false.
					// This will allow AF and ACFT to compare exactly...
					if ((conversion.getAddFactor() != 0.0) || (conversion.getMultFactor() != 1.0))
					{
						return false;
					}
				}
			}
			catch (Exception)
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// This routine checks the internal list of units data for integrity.  This
	/// consists of making sure that for units of a dimension, there is
	/// base unit only.  THIS ROUTINE IS CURRENTLY A PLACEHOLDER.
	/// @TODO SAM 2009-03-25 THE FUNCTIONALITY NEEDS TO BE ADDED.
	/// </summary>
	private static void checkUnitsData()
	{ // First see if the units are already in the list...

		//Message.printWarning ( 3, routine, "No functionality here yet!" );
	}

	/// <summary>
	/// Finalize before garbage collection. </summary>
	/// <exception cref="Throwable"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~DataUnits()
	{
		__abbreviation = null;
		__long_name = null;
		__dimension = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the units abbreviation string. </summary>
	/// <returns> The units abbreviation string. </returns>
	public virtual string getAbbreviation()
	{
		return __abbreviation;
	}

	/// <summary>
	/// Return The addition factor when converting to the base units. </summary>
	/// <returns> The addition factor when converting to the base units. </returns>
	public virtual double getAddFactor()
	{
		return __add_factor;
	}

	/// <summary>
	/// Return One (1) if the units are the base units for a dimension, zero otherwise. </summary>
	/// <returns> One (1) if the units are the base units for a dimension, zero otherwise. </returns>
	public virtual int getBaseFlag()
	{
		return __base_flag;
	}

	/// <summary>
	/// Return "BASE" if the unit is the base unit for conversions, and "OTHR" if not. </summary>
	/// <returns> "BASE" if the unit is the base unit for conversions, and "OTHR" if not. </returns>
	public virtual string getBaseString()
	{
		if (__base_flag == 1)
		{
			return "BASE";
		}
		else
		{
			return "OTHR";
		}
	}

	/// <summary>
	/// Get the conversion from units string to another. </summary>
	/// <returns> A DataUnitsConversion instance with the conversion information from one set of units to another. </returns>
	/// <param name="u1_string"> Original units. </param>
	/// <param name="u2_string"> The units after conversion. </param>
	/// <exception cref="Exception"> If the conversion cannot be found. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static DataUnitsConversion getConversion(String u1_string, String u2_string) throws Exception
	public static DataUnitsConversion getConversion(string u1_string, string u2_string)
	{ // Call the routine that takes the auxiliary information.  This is not
		// fully implemented at this time but provides a migration path from the legacy code...
		return getConversion(u1_string, u2_string, 0.0, "");
	}

	/// <summary>
	/// Get the conversion from units string to another. </summary>
	/// <returns> A DataUnitsConversion instance with the conversion information from one set of units to another. </returns>
	/// <param name="u1_string"> Original units. </param>
	/// <param name="u2_string"> The units after conversion. </param>
	/// <param name="aux"> An auxiliary piece of information when converting between units of different dimension. </param>
	/// <param name="aunits"> The units of "aux". </param>
	/// <exception cref="Exception"> If the conversion cannot be found. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static DataUnitsConversion getConversion(String u1_string, String u2_string, double aux, String aunits) throws Exception
	public static DataUnitsConversion getConversion(string u1_string, string u2_string, double aux, string aunits)
	{
		int dl = 20;
		string routine = "DataUnits.getConversion", u1_dim, u2_dim;

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Trying to get conversion from \"" + u1_string + "\" to \"" + u2_string + "\"");
		}

		// Make sure that the units strings are not NULL...

		if (((string.ReferenceEquals(u1_string, null)) || (u1_string.Equals(""))) && ((string.ReferenceEquals(u2_string, null)) || (u2_string.Equals(""))))
		{
			// Both units are unspecified so return a unit conversion...
			DataUnitsConversion c = new DataUnitsConversion();
			c.setMultFactor(1.0);
			c.setAddFactor(0.0);
			return c;
		}

		string message = "";
		if (string.ReferenceEquals(u1_string, null))
		{
			message = "Source units string is NULL";
			Message.printWarning(3, routine, message);
			throw new Exception(message);
		}
		if (string.ReferenceEquals(u2_string, null))
		{
			message = "Secondary units string is NULL";
			Message.printWarning(3, routine, message);
			throw new Exception(message);
		}

		// Set the conversion units...

		DataUnitsConversion c = new DataUnitsConversion();
		c.setOriginalUnits(u1_string);
		c.setNewUnits(u2_string);

		// First thing we do is see if the units are the same.  If so, we are done...

		if (u1_string.Trim().Equals(u2_string.Trim(), StringComparison.OrdinalIgnoreCase))
		{
			c.setMultFactor(1.0);
			c.setAddFactor(0.0);
			return c;
		}

		if (u1_string.Length == 0)
		{
			message = "Source units string is empty";
			Message.printWarning(3, routine, message);
			throw new Exception(message);
		}
		if (u2_string.Length == 0)
		{
			message = "Secondary units string is empty";
			Message.printWarning(3, routine, message);
			throw new Exception(message);
		}

		// First get the units data...

		DataUnits u1, u2;
		try
		{
			u1 = lookupUnits(u1_string);
		}
		catch (Exception)
		{
			message = "Unable to get units type for \"" + u1_string + "\"";
			Message.printWarning(3, routine, message);
			throw new Exception(message);

		}
		try
		{
			u2 = lookupUnits(u2_string);
		}
		catch (Exception)
		{
			message = "Unable to get units type for \"" + u2_string + "\"";
			Message.printWarning(3, routine, message);
			throw new Exception(message);
		}

		// Get the dimension for the units of interest...

		u1_dim = u1.getDimension().getAbbreviation();
		u2_dim = u2.getDimension().getAbbreviation();

		if (u1_dim.Equals(u2_dim, StringComparison.OrdinalIgnoreCase))
		{
			// Same dimension...
			c.setMultFactor(u1.getMultFactor() / u2.getMultFactor());
			// For the add factor assume that a value over .001 indicates
			// that an add factor should be considered.  This should only
			// be the case for temperatures and all other dimensions should have a factor of 0.0.
			if ((Math.Abs(u1.getAddFactor()) > .001) || (Math.Abs(u2.getAddFactor()) > .001))
			{
				// The addition factor needs to take into account the
				// different scales for the measurement range...
				c.setAddFactor(-1.0 * u2.getAddFactor() / u2.getMultFactor() + u1.getAddFactor() / u2.getMultFactor());
			}
			else
			{
				c.setAddFactor(0.0);
			}
			Message.printStatus(1, "", "Add factor is " + c.getAddFactor());
		}
		else
		{
			message = "Dimensions are different for " + u1_string + " and " + u2_string;
			Message.printWarning(3, routine, message);
			throw new Exception(message);
		}

		// Else, units groups are of different types - need to do more than
		// one step.  These are currently special cases and do not handle a
		// generic conversion (dimensional analysis like Unicalc)!

	/*  Not yet enabled in java...
		else if	(((u1_dim.getDimension() == DataDimension.VOLUME) &&
			(u2_dim.getDimension() == DataDimension.LENGTH)) ||
			((u1_dim.getDimension() == DataDimension.DISCHARGE)&&
			(u2_dim.getDimension() == DataDimension.LENGTH))) {
			// 1) Convert volume to M3, 2) convert area to M2, 3) divide
			// volume by area, 4) convert depth to correct units...
			//
			// If dealing with discharge, ignore time (for now)...
			DataUnitsConversion c2;
			if ( u1_dim.getDimension() == DataDimension.VOLUME ) {
				try {	c2 = getConversion ( u1_string, "M3" );
				}
				catch ( Exception e ) {
					throw new Exception (
					"can't get M3 conversion" );
				}
				mfac = c2.getMultFactor();
				afac = c2.getAddFactor();
				c.setMultFactor ( c2.getMultFactor() );
			}
			else if ( u1_dim.getDimension() == DataDimension.DISCHARGE ) {
				try {	c2 = getConversion ( u1_string, "CMS" );
				}
				catch ( Exception e ) {
					throw new Exception (
					"can't get M3 conversion" );
				}
				mfac = c2.getMultFactor();
				afac = c2.getAddFactor();
				c.setMultFactor ( c2.getMultFactor() );
			}
			try {	c2 = getConversion ( aunits, "M2" );
			}
			catch ( Exception e ) {
				throw new Exception ( "can't get M2 conversion" );
			}
			double add, mult = c.getMultFactor();
			mfac = c2.getMultFactor();
			afac = c2.getAddFactor();
			area	= aux;
			area	*= mfac;
			mult	/= area;
			c.setMultFactor ( mult );
	
			try {	c2 = getConversion ( "M", u2_string );
			}
			catch ( Exception e ) {
				throw new Exception ( "can't get M conversion" );
			}
			mfac = c2.getMultFactor();
			mult	*= mfac;	
			add	= 0.0;
			c.setMultFactor ( mult );
		}
	*/
		return c;
	}

	/// <summary>
	/// Return a DataDimension instance for the units. </summary>
	/// <returns> A DataDimension instance for the units. </returns>
	/// <seealso cref= DataDimension </seealso>
	public virtual DataDimension getDimension()
	{
		return __dimension;
	}

	/// <summary>
	/// Return the long name for the units. </summary>
	/// <returns> The long name for the units. </returns>
	public virtual string getLongName()
	{
		return __long_name;
	}

	/// <summary>
	/// Return the multiplication factor used to convert to the base units. </summary>
	/// <returns> The multiplication factor used to convert to the base units. </returns>
	public virtual double getMultFactor()
	{
		return __mult_factor;
	}

	/// <summary>
	/// Determine the format for output based on the units and precision.  A default precision of 2 is used. </summary>
	/// <returns> the printing format for data of a units type. </returns>
	/// <param name="units_string"> Units of data. </param>
	/// <param name="width"> Width of output (if zero, no width will be used in the format). </param>
	public static DataFormat getOutputFormat(string units_string, int width)
	{
		return getOutputFormat(units_string, width, 2);
	}

	/// <summary>
	/// Determine the format for output based on the units and precision. </summary>
	/// <returns> the printing format for data of a units type. </returns>
	/// <param name="units_string"> Units of data. </param>
	/// <param name="width"> Width of output (if zero, no width will be used in the format). </param>
	/// <param name="default_precision"> Default precision if precision cannot be determined
	/// from the units.  If not specified, 2 will be used. </param>
	public static DataFormat getOutputFormat(string units_string, int width, int default_precision)
	{
		string routine = "DataUnits.getOutputFormat";

		// Initialize the DataFormat for return...

		DataFormat format = new DataFormat();
		format.setWidth(width);
		format.setPrecision(default_precision);

		// Check for valid units request...

		if ((string.ReferenceEquals(units_string, null)) || (units_string.Length == 0))
		{
			// No units are specified...
			Message.printWarning(3, routine, "No units abbreviation specified.  Using precision " + default_precision);
			return format;
		}

		// Get the units...

		try
		{
			DataUnits units = lookupUnits(units_string);
			format.setPrecision(units.getOutputPrecision());
		}
		catch (Exception)
		{
			Message.printWarning(3, "DataUnits.getOutputFormat", "Unable to find data for units \"" + units_string + "\".  Using format \"" + format.ToString() + "\"");
		}
		return format;
	}

	/// <summary>
	/// Get the output format string for data given the units, width and precision. </summary>
	/// <returns> the output format string in C-style format (e.g., %10.2f). </returns>
	/// <param name="units"> Units of data. </param>
	/// <param name="width"> Width of output (if zero, no width will be used in the format). </param>
	/// <param name="default_precision"> Default precision if precision cannot be determined
	/// from the units.  If not specified, 2 will be used. </param>
	public static string getOutputFormatString(string units, int width, int default_precision)
	{
		return getOutputFormat(units,width,default_precision).ToString();
	}

	/// <summary>
	/// Return the output precision for the units. </summary>
	/// <returns> The output precision for the units (the number of digits after the decimal point). </returns>
	public virtual int getOutputPrecision()
	{
		return __output_precision;
	}

	/// <summary>
	/// Return The source of the data units. </summary>
	/// <returns> The source of the data units (narrative). </returns>
	public virtual string getSource()
	{
		return __source;
	}

	/// <summary>
	/// Return The units system. </summary>
	/// <returns> The units system.  See SYSTEM*. </returns>
	public virtual int getSystem()
	{
		return __system;
	}

	/// <summary>
	/// Return the units system as a string. </summary>
	/// <returns> The units system as a string ("SI", "ENGL", "" ). See SYSTEM*. </returns>
	public virtual string getSystemString()
	{
		if (__system == SYSTEM_SI)
		{
			return "SI";
		}
		else if (__system == SYSTEM_ENGLISH)
		{
			return "ENGL";
		}
		else if (__system == SYSTEM_ALL)
		{
			return "ALL";
		}
		else
		{
			return "";
		}
	}

	/// <summary>
	/// Return the list of units data. </summary>
	/// <returns> the list of units data (useful for debugging and GUI displays).
	/// Perhaps later overload to request by dimension, system, etc. </returns>
	public static IList<DataUnits> getUnitsData()
	{
		return __units_Vector;
	}

	/// <summary>
	/// Initialize data members.
	/// </summary>
	private void initialize()
	{
		setAbbreviation("");
		setLongName("");

		// _dimension is initialized in its class

		__base_flag = 0;
		__output_precision = 2;
		__system = SYSTEM_UNKNOWN;
		__mult_factor = 0.0; // This will cause obvious errors to show up if units are not defined correctly.
		__add_factor = 0.0;
		__behavior_mask = 0;
		__source = "";
	}

	/// <summary>
	/// Return a DataUnits instance, given the units abbreviation.  A copy is NOT made. </summary>
	/// <returns> A DataUnits instance, given the units abbreviation. </returns>
	/// <param name="units_string"> The units abbreviation to look up. </param>
	/// <exception cref="Exception"> If there is a problem looking up the units abbreviation. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static DataUnits lookupUnits(String units_string) throws Exception
	public static DataUnits lookupUnits(string units_string)
	{
		string routine = "DataUnits.lookupUnits";

		// First see if the units are already in the list...

		int size = __units_Vector.Count;
		DataUnits pt = null;
		for (int i = 0; i < size; i++)
		{
			pt = __units_Vector[i];
			if (Message.isDebugOn)
			{
				Message.printDebug(20, routine, "Comparing \"" + units_string + "\" and \"" + pt.getAbbreviation() + "\"");
			}
			if (units_string.Equals(pt.getAbbreviation(), StringComparison.OrdinalIgnoreCase))
			{
				// The requested units match something that is in the list.  Return the matching DataUnits...
				return pt;
			}
		}
		// Throw an exception...
		throw new Exception("\"" + units_string + "\" units not found");
	}

	/// <summary>
	/// Return all the DataUnits objects that have the Dimension abbreviation equal to the parameter passed in. </summary>
	/// <param name="system"> Requested units system.  Pass null or "" to get all systems,
	/// "ENGL" for English, or "SI" for SI units. </param>
	/// <param name="dimension"> the dimension abbreviation to return units for. </param>
	/// <returns> a list of all the DataUnits objects that match the dimension or an empty list if none exist. </returns>
	public static IList<DataUnits> lookupUnitsForDimension(string system, string dimension)
	{
		IList<DataUnits> v = new List<DataUnits>();

		// First see if the units are already in the list...

		int size = __units_Vector.Count;
		DataUnits pt = null;
		DataDimension dud;
		string dudDim;

		for (int i = 0; i < size; i++)
		{
			pt = __units_Vector[i];
			if ((!string.ReferenceEquals(system, null)) && !system.Equals("") && !pt.getSystemString().Equals("") && !pt.getSystemString().Equals(system, StringComparison.OrdinalIgnoreCase))
			{
				// The system does not equal the requested value so
				// ignore the units object (system of "" is OK for ENGL and SI)...
				continue;
			}
			dud = pt.getDimension();
			dudDim = dud.getAbbreviation();
			if (dimension.Equals(dudDim, StringComparison.OrdinalIgnoreCase))
			{
				v.Add(pt);
			}
		}

		return v;
	}

	/// <summary>
	/// Read a file that is in NWS DATAUNIT format.  See the fully loaded method for
	/// more information.  This version calls the other version with define_dimensions as true. </summary>
	/// <param name="dfile"> Units file to read (can be a URL). </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void readNWSUnitsFile(String dfile) throws java.io.IOException
	public static void readNWSUnitsFile(string dfile)
	{
		readNWSUnitsFile(dfile, true);
	}

	/// <summary>
	/// Read a file that is in NWS DATAUNIT format.
	/// This routine depends on on the values in the DATAUNIT file orignally supplied
	/// by the NWS.  Because the units system cannot be determined from this file,
	/// the units system is hard-coded.  This may lead to some errors if the contents
	/// of the units file changes.  The typical format for this file are as follows:
	/// <para>
	/// <pre>
	///   11/8/90   'HYD.RFS.SYSTEM(DATAUNIT)'
	/// 
	/// LENGTH
	/// L    BASE MM    MILLIMETER                          1 1.        .
	/// L    OTHR CM    CENTIMETER                          2 10.       .
	/// L    OTHR M     METER                               2 1000.     .
	/// L    OTHR KM    KILOMETER                           1 1000000.  .
	/// L    OTHR IN    INCH                                2 25.4      .
	/// L    OTHR FT    FOOT                                2 304.8     .
	/// L    OTHR MI    MILE (STATUTE)                      1 1609344.  .
	/// L    OTHR NM    MILE (NAUTICAL)                     1 1853248.  .
	/// TEMPERATURE
	/// TEMP BASE DEGC  DEGREE CENTIGRADE                   1 1.        0.000
	/// TEMP OTHR DEGK  DEGREE KELVIN                       1 1.        -273.
	/// TEMP OTHR DEGF  DEGREE FAHRENHEIT                   1 .555556   -17.8
	/// TEMP OTHR DEGR  DEGREE RANKINE                      1 .555556   -273.
	/// END DATAUNIT
	/// </pre>
	/// </para>
	/// </summary>
	/// <param name="dfile"> Units file to read (can be a URL). </param>
	/// <param name="define_dimensions"> If true, then DataDimension.addDimension() is called
	/// for each dimension referenced in the data units, with the name and abbreviation
	/// being the same.  This is required in many cases because defining a data unit
	/// instance checks the dimension against defined dimensions. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void readNWSUnitsFile(String dfile, boolean define_dimensions) throws java.io.IOException
	public static void readNWSUnitsFile(string dfile, bool define_dimensions)
	{
		double add_factor = 0.0, mult_factor = 1.0;
		string abbreviation, base_string, dimension, long_name, routine = "DataUnits.readNWSUnitsFile", @string;
		int output_precision = 2;
		StreamReader fp = null;
		string[] engl_units = new string[] {"IN", "FT", "MI", "NM", "FT/S", "FT/M", "MI/H", "MI/D", "KNOT", "IN2", "FT2", "MI2", "NM2", "ACRE", "CFSD", "FT3", "IN3", "GAL", "ACFT", "CFS", "AF/D", "MGD", "GPM", "INHG", "DEGF"};
		string[] si_units = new string[] {"MM", "CM", "M", "KM", "M/S", "CM/S", "KM/H", "KM/D", "M2", "MM2", "CM2", "KM2", "HECT", "M3", "CC", "LITR", "CMSD", "MCM", "CHM", "CMS", "CC/S", "CM/H", "MMHG", "DEGC"};

		try
		{
			// Main try...
			// Open the file (allow the units file to be a normal file or a URL so
			// web applications can also be supported)...
			try
			{
				fp = new StreamReader(IOUtil.getInputStream(dfile));
			}
			catch (Exception e)
			{
				Message.printWarning(3, routine, e);
				throw new IOException("Error opening units file \"" + dfile + "\" uniquetempvar.");
			}
			int linecount = 0;
			DataUnits units = null;
			bool system_found = false; // Indicates whether the system for the units has been found.
			while (true)
			{
				// Read a line...
				@string = fp.ReadLine();
				++linecount;
				if (string.ReferenceEquals(@string, null))
				{
					// End of file...
					break;
				}
				try
				{
					// If exceptions are caught, ignore the data..
					@string = @string.Trim();
					if (@string.Length == 0)
					{
						// Skip blank lines...
						continue;
					}
					if (@string[0] == '*')
					{
						// A comment line...
						if (@string.regionMatches(true,0,"* END",0,5))
						{
							// End of file...
							break;
						}
						// Else ignore...
						continue;
					}
					// A line with conversion factors...
					dimension = @string.Substring(0,4).Trim();
					base_string = @string.Substring(5, 4).Trim();
					abbreviation = @string.Substring(10, 4).Trim();
					long_name = @string.Substring(16, 36).Trim();
					// This is sometimes blank.  If so, default to 3...
					if (@string.Substring(52, 1).Trim().Equals(""))
					{
						output_precision = 3;
					}
					else
					{
						output_precision = int.Parse(@string.Substring(52, 1).Trim());
					}
					mult_factor = double.Parse(@string.Substring(54, 10).Trim());
					if (dimension.Equals("TEMP", StringComparison.OrdinalIgnoreCase))
					{
						//if ( string.length() >= 71 ) {
							//add_factor = StringUtil.atod(string.substring(64,71).trim() );
						//}
						//else {	
							add_factor = double.Parse(@string.Substring(64).Trim());
						//}
					}
					else
					{
						add_factor = 0.0;
					}
					// Now add as a new set of units (for now, we add everything and don't just add the ones that are
					// commonly used, as in the legacy HMData code)...
					units = new DataUnits();
					if (define_dimensions)
					{
						// Define the dimension in the DataDimension global
						// data so that it can be referenced below.  It is OK
						// to define more than once because DataDimension will keep only one unique definition.
						DataDimension.addDimension(new DataDimension(dimension,dimension));
					}
					units.setDimension(dimension);
					if (base_string.Equals("BASE", StringComparison.OrdinalIgnoreCase))
					{
						units.setBaseFlag(1);
					}
					else
					{
						units.setBaseFlag(0);
					}
					units.setAbbreviation(abbreviation);
					units.setLongName(long_name);
					units.setOutputPrecision(output_precision);
					units.setMultFactor(mult_factor);
					units.setAddFactor(add_factor);
					// Determine the system from hard-coded units...
					system_found = false;
					units.setSystem(SYSTEM_ALL); // default
					for (int iu = 0; iu < engl_units.Length; iu++)
					{
						if (abbreviation.Equals(engl_units[iu], StringComparison.OrdinalIgnoreCase))
						{
							units.setSystem(SYSTEM_ENGLISH);
							system_found = true;
							break;
						}
					}
					if (!system_found)
					{
						for (int iu = 0; iu < si_units.Length; iu++)
						{
							if (abbreviation.Equals(si_units[iu], StringComparison.OrdinalIgnoreCase))
							{
								units.setSystem(SYSTEM_SI);
								break;
							}
						}
					}
					// Set how the units are defined
					units.setSource("Read from NWSRFS units file \"" + dfile + "\"");
					addUnits(units);
				}
				catch (Exception e)
				{
					Message.printWarning(3, routine, "Error reading units at line " + linecount + " of file \"" + dfile + "\" - ignoring line (" + e + ").");
					Message.printWarning(3, routine, e);
				}
			}
		}
		catch (Exception e)
		{
			Message.printWarning(3, routine, e);
			// Global catch...
			throw new IOException("Error reading units file \"" + dfile + "\" uniquetempvar.");
		}
		finally
		{
			if (fp != null)
			{
				fp.Close();
			}
			checkUnitsData();
		}
	}

	/// <summary>
	/// Read a file that is in RTi format.  See the fully loaded method for more information.
	/// This version calls the other version with define_dimensions as true. </summary>
	/// <param name="dfile"> Units file to read (can be a URL). </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void readUnitsFile(String dfile) throws java.io.IOException
	public static void readUnitsFile(string dfile)
	{
		readUnitsFile(dfile, true);
	}

	/// <summary>
	/// Read a file that is in RTi format.
	/// This routine depends on on the values in an RTi DATAUNIT file.  The format for this file is as follows:
	/// <para>
	/// <pre>
	/// # Dimension|BASE or OTHR|Abbreviation|System|Long name|Precision|MultFac|AddFac|
	/// # TEMPERATURE
	/// TEMP|BASE|DEGC|SI|DEGREE CENTIGRADE|1|1.|0.0|
	/// TEMP|OTHR|DEGK|ENG|DEGREE KELVIN|1|1.|-273.|
	/// TEMP|OTHR|DEGF||DEGREE FAHRENHEIT|1|.555556|-17.8|
	/// TEMP|OTHR|DEGR||DEGREE RANKINE|1|.555556|-273.|
	/// # TIME
	/// TIME|BASE|SEC||SECOND|2|1.|0.0|
	/// TIME|OTHR|MIN||MINUTE|2|60.|0.0|
	/// TIME|OTHR|HR||HOUR|2|3600.|0.0|
	/// TIME|OTHR|DAY||DAY|2|86400.|0.0|
	/// </pre>
	/// </para>
	/// </summary>
	/// <param name="dfile"> Name of units file (can be a URL). </param>
	/// <param name="define_dimensions"> If true, then DataDimension.addDimension() is called
	/// for each dimension referenced in the data units, with the name and abbreviation
	/// being the same.  This is required in many cases because defining a data unit
	/// instance checks the dimension against defined dimensions. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void readUnitsFile(String dfile, boolean define_dimensions) throws java.io.IOException
	public static void readUnitsFile(string dfile, bool define_dimensions)
	{
		string message, routine = "DataUnits.readUnitsFile";
		IList<string> units_file = null;

		try
		{
			// Main try...

			// Read the file into a list...
			// FIXME SAM 2009-03-25 Error handling needs to be improved here - remove nested exceptions
			try
			{
				units_file = IOUtil.fileToStringList(dfile);
			}
			catch (Exception)
			{
				message = "Unable to read units file \"" + dfile + "\"";
				Message.printWarning(3, routine, message);
				throw new IOException(message);
			}
			if (units_file == null)
			{
				message = "Empty contents for units file \"" + dfile + "\"";
				Message.printWarning(3, routine, message);
				throw new IOException(message);
			}
			int nstrings = units_file.Count;
			if (nstrings == 0)
			{
				message = "Empty contents for units file \"" + dfile + "\"";
				Message.printWarning(3, routine, message);
				throw new IOException(message);
			}

			// For each line, if not a comment, break apart and add units to the global list...

			DataUnits units;
			string @string, token;
			IList<string> tokens = null;
			char first;
			for (int i = 0; i < nstrings; i++)
			{
				try
				{
					@string = units_file[i];
					if (string.ReferenceEquals(@string, null))
					{
						continue;
					}
					if (@string.Length == 0)
					{
						continue;
					}
					first = @string[0];
					if ((first == '#') || (first == '\n') || (first == '\r'))
					{
						continue;
					}
					// Break the line...
					tokens = StringUtil.breakStringList(@string, "|", 0);
					if (tokens == null)
					{
						// A corrupt line...
						continue;
					}
					if (tokens.Count < 7)
					{
						// A corrupt line...
						continue;
					}
					// Else add the units...
					units = new DataUnits();
					if (define_dimensions)
					{
						// Define the dimension in the DataDimension global data so that it can be referenced below.
						// It is OK to define more than once because DataDimension will
						// keep only one unique definition.
						DataDimension.addDimension(new DataDimension(tokens[0].Trim(), tokens[0].Trim()));
					}
					units.setDimension(tokens[0].Trim());
					token = tokens[1];
					if (token.Equals("BASE", StringComparison.OrdinalIgnoreCase))
					{
						// Base units for the dimension...
						units.setBaseFlag(1);
					}
					else
					{
						units.setBaseFlag(0);
					}
					units.setAbbreviation(tokens[2].Trim());
					units.setSystem(tokens[3].Trim());
					units.setLongName(tokens[4].Trim());
					string precision = tokens[5].Trim();
					if (StringUtil.isInteger(precision))
					{
						units.setOutputPrecision(int.Parse(precision));
					}
					units.setMultFactor(double.Parse(tokens[6].Trim()));
					string add = tokens[7].Trim();
					if (StringUtil.isDouble(add))
					{
						units.setAddFactor(double.Parse(add));
					}
					// Set how the units are defined
					units.setSource("Read from units file \"" + dfile + "\"");
					// Add the units to the list...
					addUnits(units);
				}
				catch (Exception e)
				{
					Message.printWarning(3, routine, "Error reading units at line " + (i + 1) + " of file \"" + dfile + "\" - ignoring line (" + e + ").");
				}
			}

			// Check the units for consistency...

			checkUnitsData();
		}
		catch (Exception e)
		{
			Message.printWarning(3, routine, e);
			// Global catch...
			throw new IOException("Error reading units file \"" + dfile + "\" uniquetempvar.");
		}
	}

	/// <summary>
	/// Set the abbreviation string for the units. </summary>
	/// <param name="abbreviation"> Units abbreviation (e.g., "CFS"). </param>
	public virtual void setAbbreviation(string abbreviation)
	{
		if (string.ReferenceEquals(abbreviation, null))
		{
			return;
		}
		__abbreviation = abbreviation;
	}

	/// <summary>
	/// Set the addition factor when converting to the base units for the dimension. </summary>
	/// <param name="add_factor"> Add factor to convert to the base units. </param>
	public virtual void setAddFactor(double add_factor)
	{
		__add_factor = add_factor;
	}

	/// <summary>
	/// Indicate whether the units are base units (should only have one base for a dimension. </summary>
	/// <param name="base_flag"> Indicates if the units are base units. </param>
	public virtual void setBaseFlag(int base_flag)
	{
		__base_flag = base_flag;
	}

	/// <summary>
	/// Set the behavior flag for the units (used for converting to strings).  This is not used at this time. </summary>
	/// <param name="behavior_mask"> Indicates how units should be displayed.  </param>
	public virtual void setBehaviorMask(int behavior_mask)
	{
		__behavior_mask = behavior_mask;
	}

	/// <summary>
	/// Set the dimension for the units. </summary>
	/// <param name="dimension_string"> Dimension string (e.g., "L3/T"). </param>
	/// <exception cref="Exception"> If the dimension string to be used is not recognized. </exception>
	/// <seealso cref= DataDimension </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setDimension(String dimension_string) throws Exception
	public virtual void setDimension(string dimension_string)
	{
		string routine = "DataUnits.setDimension(String)";

		// Return if null...

		if (string.ReferenceEquals(dimension_string, null))
		{
			return;
		}

		// First look up the dimension to make sure that it is valid...

		DataDimension dim;
		try
		{
			dim = DataDimension.lookupDimension(dimension_string);
		}
		catch (Exception)
		{
			// Problem finding dimension.  Don't set...
			string message;
			message = "Can't find dimension \"" + dimension_string + "\".  Not setting.";
			Message.printWarning(3, routine, message);
			throw new Exception(message);
		}

		// Now set the dimension...

		__dimension = dim;
	}

	/// <summary>
	/// Set the long name for the units (e.g., "cubic feet per second"). </summary>
	/// <param name="long_name"> Long name for the units. </param>
	public virtual void setLongName(string long_name)
	{
		if (string.ReferenceEquals(long_name, null))
		{
			return;
		}
		__long_name = long_name;
	}

	/// <summary>
	/// Set the multiplication factor used when converting to the base units. </summary>
	/// <param name="mult_factor"> Multiplication factor used when converting to the base units. </param>
	public virtual void setMultFactor(double mult_factor)
	{
		__mult_factor = mult_factor;
	}

	/// <summary>
	/// Set the number of digits after the decimal to be used for output data of these units. </summary>
	/// <param name="output_precision"> Number of digits after the decimal to be used for output
	/// for data of these units. </param>
	public virtual void setOutputPrecision(int output_precision)
	{
		__output_precision = output_precision;
	}

	/// <summary>
	/// Set the source of the data units. </summary>
	/// <param name="source"> source of the data units (narrative). </param>
	public virtual void setSource(string source)
	{
		if (string.ReferenceEquals(source, null))
		{
			return;
		}
		__source = source;
	}

	/// <summary>
	/// Set the system of units. </summary>
	/// <param name="system"> System of units (see SYSTEM_*). </param>
	public virtual void setSystem(int system)
	{
		__system = system;
	}

	/// <summary>
	/// Set the system of units. </summary>
	/// <param name="system"> System of units.  Recognized strings are "SI", "ENG", or nothing.
	/// If the system cannot be determined, SYSTEM_UNKNOWN is assumed. </param>
	public virtual void setSystem(string system)
	{
		if (string.ReferenceEquals(system, null))
		{
			return;
		}
		if (system.regionMatches(true,0,"SI",0,2))
		{
			__system = SYSTEM_SI;
		}
		else if (system.regionMatches(true,0,"ENG",0,3))
		{
			__system = SYSTEM_ENGLISH;
		}
		else if (system.regionMatches(true,0,"ALL",0,4))
		{
			__system = SYSTEM_ALL;
		}
		else
		{
			__system = SYSTEM_UNKNOWN;
		}
	}

	/// <summary>
	/// Return A string representation of the units (verbose). </summary>
	/// <returns> A string representation of the units (verbose). </returns>
	public override string ToString()
	{
		return __dimension.getAbbreviation() + "|" + getBaseString() + "|" + __abbreviation + "|" + getSystemString() + "|" + __long_name + "|" + __output_precision + "|" + __mult_factor + "|" + __add_factor + "|";
	}

	}

}