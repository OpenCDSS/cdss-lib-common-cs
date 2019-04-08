using System;
using System.Collections.Generic;
using System.IO;

// DataType - data type class

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
// DataType - data type class
// ----------------------------------------------------------------------------
// Copyright:  See the COPYRIGHT file.
// ----------------------------------------------------------------------------
// History:
//
// 2003-10-31	Steven A. Malers, RTi	Initial version.  Copy DataUnits and
//					update the code.  The design is meant to
//					be compatible with RTi's RiverTrak and
//					the NWSRFS, in particular to support
//					SHEF output of time series.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------
// EndHeader

namespace RTi.Util.IO
{

	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// The DataType class provides capabilities for reading and storing 
	/// data type information.  Data types are used to look up the dimension, default
	/// units, and SHEF (Standard Hydrologic Exchange Format) information.  Data types
	/// are maintained internally using a Vector of DataType (self-referencing).  Data types
	/// are typically read from a persistent source at application startup.
	/// </summary>
	public class DataType
	{

	/// <summary>
	/// Indicates that the units system is unknown.
	/// </summary>
	public const int SYSTEM_UNKNOWN = 0;

	// Data members...

	/// <summary>
	/// The data type abbreviation (e.g., "MAP").  This is typically based on persistent data formats.
	/// </summary>
	private string __abbreviation;

	/// <summary>
	/// The verbose description (e.g., ("Mean Areal Precipitation").
	/// </summary>
	private string __description;

	// TODO SAM 2007-12-11 Need a class for the dimension.
	/// <summary>
	/// The dimension (e.g., "L3"). </summary>
	/// <seealso cref= DataDimension </seealso>
	private DataDimension __dimension;

	/// <summary>
	/// Default English units (e.g., when creating a new time series in a
	/// system configured for English units. "DEGF" for NWSRFS temperature).
	/// </summary>
	private string __default_engl_units;

	/// <summary>
	/// Default maximum value for English units data - for initializing range checks.
	/// </summary>
	private double __default_engl_max;

	/// <summary>
	/// Default minimum value for English units data - for initializing range checks.
	/// </summary>
	private double __default_engl_min;

	/// <summary>
	/// Default SI units (e.g., when creating a new time series in a system configured for SI units.
	/// "DEGC" for NWSRFS temperature).
	/// </summary>
	private string __default_si_units;

	/// <summary>
	/// Default maximum value for SI units data - for initializing range checks.
	/// </summary>
	private double __default_si_max;

	/// <summary>
	/// Default minimum value for SI units data - for initializing range checks.
	/// </summary>
	private double __default_si_min;

	/// <summary>
	/// Measurement location type See MeasLocType static data.
	/// </summary>
	private string __meas_loc_type;

	/// <summary>
	/// Measurement time scale See MeasTimeScale static data.
	/// </summary>
	private string __meas_time_scale;

	/// <summary>
	/// Either "OBS" for observed or "SIM" for simulated.  This is a hold-over from
	/// NWSRFS conventions, which limit some data types' use to only observed or simulated data.
	/// </summary>
	private string __record_type;

	// TODO SAM 2007-12-11 Evaluate a more generic use of the lookup so the SHEF code
	// does not need to be stored in this class.
	/// <summary>
	/// SHEF (Standard Hydrologic Exchange Format) physical element data type.  The value is
	/// populated when SHEF data are read. </summary>
	/// <seealso cref= SHEFType </seealso>
	private string __SHEF_pe;

	/// <summary>
	/// List of internally-maintained DataType instances.  The list can be modified by the static read methods.
	/// </summary>
	private static IList<DataType> __types_Vector = new List<DataType>();

	/// <summary>
	/// Construct and set all data members to empty strings and zeros.
	/// </summary>
	public DataType()
	{
		initialize();
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
	/// <seealso cref= DataDimension </seealso>
	/* TODO
	public DataUnits ( String dimension, int base_flag, String abbreviation,
				String long_name, int output_precision, double mult_factor, double add_factor )
	{	initialize ();
		try {
		setDimension ( dimension );
		}
		catch ( Exception e ) {
			// Do nothing for now.
		}
		__base_flag = base_flag;
		setAbbreviation ( abbreviation );
		setLongName ( long_name );
		__output_precision = output_precision;
		__mult_factor = mult_factor;
		__add_factor = add_factor;
	}
	*/

	/// <summary>
	/// Copy constructor. </summary>
	/// <param name="type"> Instance of DataType to copy. </param>
	public DataType(DataType type)
	{
		initialize();
		__abbreviation = type.__abbreviation;
		__description = type.__description;
		try
		{
			// Converts to integer, etc.
			setDimension(type.__dimension.getAbbreviation());
		}
		catch (Exception)
		{
			// Do nothing for now...
		}
		__default_engl_max = type.__default_engl_max;
		__default_engl_min = type.__default_engl_min;
		__default_engl_units = type.__default_engl_units;
		__default_si_max = type.__default_si_max;
		__default_si_min = type.__default_si_min;
		__default_si_units = type.__default_si_units;
		__meas_loc_type = type.__meas_loc_type;
		__meas_time_scale = type.__meas_time_scale;
		__record_type = type.__record_type;
		__SHEF_pe = type.__SHEF_pe;
	}

	// FIXME SAM 2008-05-03 Need to have the option to sort
	/// <summary>
	/// Add a data type to the internal list of data types.  After adding, the data type
	/// can be used throughout an application. </summary>
	/// <param name="type"> Instance of DataType to add to the list. </param>
	public static void addDataType(DataType type)
	{ // First see if the type is already in the list...

		int size = __types_Vector.Count;
		DataType pt = null;
		for (int i = 0; i < size; i++)
		{
			// Get the type for the loop index...
			pt = __types_Vector[i];
			// Now compare...
			if (type.getAbbreviation().Equals(pt.getAbbreviation(), StringComparison.OrdinalIgnoreCase))
			{
				// The requested units match something that is already in the list.  Reset the list...
				__types_Vector[i] = type;
				return;
			}
		}
		// Need to add the units to the list...
		__types_Vector.Add(type);
	}

	/// <summary>
	/// Finalize before garbage collection. </summary>
	/// <exception cref="Throwable"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~DataType()
	{
		__abbreviation = null;
		__description = null;
		__default_engl_units = null;
		__default_si_units = null;
		__dimension = null;
		__meas_loc_type = null;
		__meas_time_scale = null;
		__record_type = null;
		__SHEF_pe = null;
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
	/// Return the list of data types data. </summary>
	/// <returns> the list of data types (useful for debugging and GUI displays).
	/// Perhaps later overload to request by dimension, system, etc. </returns>
	public static IList<DataType> getDataTypesData()
	{
		return __types_Vector;
	}

	/// <summary>
	/// Return the default maximum value when used with English units. </summary>
	/// <returns> the default maximum value when used with English units. </returns>
	public virtual double getDefaultEnglishMax()
	{
		return __default_engl_max;
	}

	/// <summary>
	/// Return the default minimum value when used with English units. </summary>
	/// <returns> the default minimum value when used with English units. </returns>
	public virtual double getDefaultEnglishMin()
	{
		return __default_engl_min;
	}

	/// <summary>
	/// Return the default units string when used with English units. </summary>
	/// <returns> the default units string when used with English units. </returns>
	public virtual string getDefaultEnglishUnits()
	{
		return __default_engl_units;
	}

	/// <summary>
	/// Return the default maximum value when used with SI units. </summary>
	/// <returns> the default maximum value when used with SI units. </returns>
	public virtual double getDefaultSIMax()
	{
		return __default_si_max;
	}

	/// <summary>
	/// Return the default minimum value when used with SI units. </summary>
	/// <returns> the default minimum value when used with SI units. </returns>
	public virtual double getDefaultSIMin()
	{
		return __default_si_min;
	}

	/// <summary>
	/// Return the default units string when used with SI units. </summary>
	/// <returns> the default units string when used with SI units. </returns>
	public virtual string getDefaultSIUnits()
	{
		return __default_si_units;
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
	/// Return the description for the units. </summary>
	/// <returns> The description for the units. </returns>
	public virtual string getDescription()
	{
		return __description;
	}

	/// <summary>
	/// Return the measurement location type. </summary>
	/// <returns> the measurement location type. </returns>
	public virtual string getMeasLocType()
	{
		return __meas_loc_type;
	}

	/// <summary>
	/// Return the measurement time scale. </summary>
	/// <returns> the measurement time scale. </returns>
	public virtual string getMeasTimeScale()
	{
		return __meas_time_scale;
	}

	/// <summary>
	/// Return the record type ("OBS" or "SIM"). </summary>
	/// <returns> the record type ("OBS" or "SIM"). </returns>
	public virtual string getRecordType()
	{
		return __record_type;
	}

	/// <summary>
	/// Return the SHEF physical element data type. </summary>
	/// <returns> the SHEF physical element data type. </returns>
	public virtual string getSHEFpe()
	{
		return __SHEF_pe;
	}

	/// <summary>
	/// Return all the DataUnits objects that have the Dimension abbreviation
	/// equal to the parameter passed in. </summary>
	/// <param name="system"> Requested units system.  Pass null or "" to get all systems,
	/// "ENGL" for English, or "SI" for SI units. </param>
	/// <param name="dimension"> the dimension abbreviation to return units for. </param>
	/// <returns> a list of all the DataUnits objects that match the dimension or
	/// an empty list if none exist. </returns>
	/// @deprecated use lookupUnitsForDimension 
	/* TODO
	public static List<DataUnit> getUnitsForDimension ( String system, String dimension )
	{	return lookupUnitsForDimension ( system, dimension );
	}
	*/

	/// <summary>
	/// Initialize data members.
	/// </summary>
	private void initialize()
	{
		__abbreviation = "";
		__default_engl_units = "";
		__default_si_units = "";
		__description = "";

		// _dimension is initialized in its class

		__description = "";

		__default_engl_max = 9999999.0;
		__default_engl_min = 0.0;
		__default_engl_units = "";

		__default_si_max = 9999999.0;
		__default_si_min = 0.0;
		__default_si_units = "";

		__meas_loc_type = "";
		__meas_time_scale = "";
		__record_type = "";
		__SHEF_pe = "";
	}

	/// <summary>
	/// Return the matching global DataType instance, given the data type abbreviation.  A copy is not made. </summary>
	/// <returns> A DataType instance, given the data type abbreviation. </returns>
	/// <param name="type_string"> The data type abbreviation to look up. </param>
	/// <exception cref="Exception"> If there is a problem looking up the data type abbreviation. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static DataType lookupDataType(String type_string) throws Exception
	public static DataType lookupDataType(string type_string)
	{
		string routine = "DataType.lookupDataType";

		// First see if the data type is already in the list...

		int size = __types_Vector.Count;
		DataType pt = null;
		for (int i = 0; i < size; i++)
		{
			pt = __types_Vector[i];
			if (Message.isDebugOn)
			{
				Message.printDebug(20, routine, "Comparing " + type_string + " and " + pt.getAbbreviation());
			}
			if (type_string.Equals(pt.getAbbreviation(), StringComparison.OrdinalIgnoreCase))
			{
				// The requested data type match something that is in the list.  Return the matching DataType...
				return (pt);
			}
		}
		// Throw an exception...
		throw new Exception("\"" + type_string + "\" data type not found");
	}

	/// <summary>
	/// Return all the DataUnits objects that have the Dimension abbreviation equal to the paramter passed in. </summary>
	/// <param name="system"> Requested units system.  Pass null or "" to get all systems,
	/// "ENGL" for English, or "SI" for SI units. </param>
	/// <param name="dimension"> the dimension abbreviation to return units for. </param>
	/// <returns> a Vector of all the DataUnits objects that match the dimension or an empty Vector if none exist. </returns>
	/* TODO
	public static List<DataUnit> lookupUnitsForDimension ( String system, String dimension )
	{	String	routine = "DataUnits.lookupUnitsForDimension";
	
		List<DataUnit> v = new Vector();
	
		// First see if the units are already in the list...
	
		int size = __units_Vector.size();
		DataUnits pt = null;
		DataDimension dud;
		String dudDim;
	
		for ( int i = 0; i < size; i++ ) {
			pt = __units_Vector.elementAt(i);
			if ( (system != null) && !system.equals("") &&
				!pt.getSystemString().equals("") &&
				!pt.getSystemString().equalsIgnoreCase(system) ) {
				// The system does not equal the requested value so
				// ignore the units object (system of "" is OK for
				// ENGL and SI)...
				continue;
			}
			dud = (DataDimension)pt.getDimension();
			dudDim = dud.getAbbreviation();
			if ( dimension.equalsIgnoreCase(dudDim) ) {
				v.addElement(pt);
			}
		}
	
		return v;
	}
	*/

	/// <summary>
	/// Read a file that is in NWS DATATYPE format.  See the fully loaded method for
	/// more information.  This version calls the other version with define_dimensions as true. </summary>
	/// <param name="dfile"> Units file to read (can be a URL). </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void readNWSDataTypeFile(String dfile) throws java.io.IOException
	public static void readNWSDataTypeFile(string dfile)
	{
		readNWSDataTypeFile(dfile, true);
	}

	/// <summary>
	/// Read a file that is in NWS DATATYPE format.  The NWS DATAUNIT file should be
	/// read before calling this method, using DataUnits.readNWSUnitsFile().
	/// This routine depends on on the values in the DATATYPE file originally supplied
	/// by the NWS.  The file is normally named
	/// /awips/hydroapps/lx/rfc/nwsrfs/sys_files/DATAUNIT and has the format:
	/// <para>
	/// <pre>
	///   04/19/2000   DATATYPE
	/// 
	/// AESC SNOW COVER          AREAL EXTENT        OBSERVED
	/// AESC DLES A    INST BOTH  1 YES
	/// AESC FCST PCTD YES  PP    0
	/// AESC CALB
	/// 
	/// AIAI ANTECEDENT INDEX  .  .
	/// AIAI DLES A/P  INST BOTH  1 YES
	/// AIAI FCST REAL NO   FC    0
	/// AIAI CALB
	/// 
	/// AEIS ANTECEDENT EVAPORATION INDEX  .  .
	/// AEIS L    A/P  INST BOTH  1 YES
	/// AEIS FCST MM   NO   FC    0
	/// AEIS CALB
	/// ...
	/// ZRX  ZERO RAIN FRACTION                      INSTANTANEOUS
	/// ZRX  DLES A    INST BOTH  1
	/// ZRX  FCST PCTD NO   PP    0
	/// ZRX  CALB
	/// 
	/// END DATATYPE
	/// </pre>
	/// In this format, multiple lines are used for each data type, with the data type
	/// abbreviation being included at the start of each line.
	/// The first line includes data type, and description, possibly for 3 levels of
	/// output (when read here, the end of the line is set to the description, removing extra whitespace).
	/// The second line includes data type, dimension, whether an areal ("A"),
	/// point ("P") or both ("A/P") measurement, the time scale ("ACCM", "MEAN", or
	/// "INST", whether used in the forecast and calibration system ("CALB", "FCST",
	/// or "BOTH", ignored), number of data values per time step (ignored), and whether
	/// the data can be stored in DATACARD format ("YES" or "NO", ignored).
	/// If the data type is used in the forecast system, a line will be included for
	/// forecast system information, including data type,
	/// the word "FCST", the data units in which the forecast system does computations
	/// (used as the default units for this data type), whether missing data are allowed
	/// ("YES" or "NO", ignored), the code for the component that can write the data
	/// type ("PP" for preprocessor or "FC" for forecast, ignored), and the number
	/// additional pieces of information (ignored).
	/// If the data type is used in the calibration system, a line will be included for
	/// calibration system information, including data type, the word "CALB".
	/// </para>
	/// </summary>
	/// <param name="dfile"> Data type file to read (can be a URL). </param>
	/// <param name="define_dimensions"> If true, then DataDimension.addDimension() is called
	/// for each dimension referenced in the data units, with the name and abbreviation
	/// being the same.  This is required in many cases because defining a data type
	/// instance checks the dimension against defined dimensions. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void readNWSDataTypeFile(String dfile, boolean define_dimensions) throws java.io.IOException
	public static void readNWSDataTypeFile(string dfile, bool define_dimensions)
	{
		string routine = "DataUnits.readNWSDataTypeFile", @string;
		StreamReader fp = null;

		try
		{ // Main try...
		// Open the file (allow the data type file to be a normal file or a URL
		// so web applications can also be supported)...
		try
		{
			fp = new StreamReader(IOUtil.getInputStream(dfile));
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, e);
			throw new IOException("Error opening data type file \"" + dfile + "\"");
		}
		finally
		{
			if (fp != null)
			{
				fp.Close();
			}
		}
		int linecount = 0;
		DataType type = null;
		string type_prev = ""; // Previous type, to keep track of how many
					// lines are read for the same type.
		int type_count = 0; // Count of how many times the type has been
					// read, to keep track of multiple lines of input.
		IList<object> tokens = new List<object>(7);
					// Tokens from data lines - share the Vector between multiple reads.
		// Format to read the first data line per data type...
		int[] format_1 = new int[] {StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING};
		int[] format_1w = new int[] {4, 1, 20, 20, 20};
		// Format to read the 2nd data line per data type...
		int[] format_2 = new int[] {StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_INTEGER, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING};
		int[] format_2w = new int[] {4, 1, 4, 1, 3, 2, 4, 1, 4, 1, 2, 1, 4};
		// Format to read the third data line per data type...
		int[] format_3 = new int[] {StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING};
		int[] format_3w = new int[] {4, 1, 4, 1, 4, 1, 4};
		string abbreviation = "?"; // Data type abbreviation.
		string description = null; // Description for data type.
		string dimension = null; // Data dimension.
		string default_units = null; // Default units.
		string tmp = null; // Temporary string.
		DataUnits units = null; // Used to assign default units.
		string meas_loc_type = null; // Measurement location type
		string meas_time_scale = null; // Measurement time scale.
		bool in_fcst = false; // Indicates whether the data type is
		bool read_fcst = false; // used in the forecast system, and whether a data line has been read.
		bool in_calb = false; // Indicates whether the data type is
		bool read_calb = false; // used in the calibration system, and whether a data line has been read.
		string where_used = null; // Indicates whether the data type is
						// used in the calibration and/or forecast system.
		while (true)
		{
			// Read a line...
			type_prev = abbreviation; // Save the previous data type that was read.
			@string = fp.ReadLine();
			++linecount;
			if (string.ReferenceEquals(@string, null))
			{
				// End of file...
				break;
			}
			try
			{ // If exceptions are caught, ignore the data..
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
			// A line with data.  First process to verify continuation of previous lines of data...
			if (@string.StartsWith(type_prev + " ", StringComparison.Ordinal))
			{
				// Continuation...
				++type_count;
			}
			else
			{
				// Reset...
				type_count = 1;
				in_fcst = read_fcst = in_calb = read_calb = false;
			}
			// Now process the specific line...
			if (type_count == 1)
			{
				StringUtil.fixedRead(@string, format_1, format_1w, tokens);
				abbreviation = ((string)tokens[0]).Trim();
				description = ((string)tokens[1]).Trim();
				tmp = ((string)tokens[2]).Trim();
				// The description is comprised of the Level 1, 2, 3
				// descriptions.  Sometimes these have a "  ." at the end so remove...
				if ((tmp.Length > 0) && !tmp.Equals("."))
				{
					description += " " + tmp;
				}
				tmp = ((string)tokens[3]).Trim();
				if ((tmp.Length > 0) && !tmp.Equals("."))
				{
					description += " " + tmp;
				}
				while ((description.Length > 1) && description.EndsWith(" .", StringComparison.Ordinal))
				{
					description = description.Substring(0, description.Length - 2).Trim();
				}
			}
			else if (type_count == 2)
			{
				StringUtil.fixedRead(@string, format_2, format_2w, tokens);
				dimension = ((string)tokens[1]).Trim();
				meas_loc_type = ((string)tokens[2]).Trim();
				meas_time_scale = ((string)tokens[3]).Trim();
				where_used = ((string)tokens[4]).Trim();
				if (where_used.Equals("CALB", StringComparison.OrdinalIgnoreCase))
				{
					in_calb = true;
				}
				else if (where_used.Equals("FCST", StringComparison.OrdinalIgnoreCase))
				{
					in_fcst = true;
				}
				else if (where_used.Equals("BOTH", StringComparison.OrdinalIgnoreCase))
				{
					in_calb = true;
					in_fcst = true;
				}
				// Can be NONE for RTi data.
			}
			else if (@string.Substring(5).StartsWith("FCST", StringComparison.Ordinal))
			{
				StringUtil.fixedRead(@string, format_3, format_3w, tokens);
				default_units = ((string)tokens[2]).Trim();
				read_fcst = true;
			}
			else if (@string.Substring(5).StartsWith("CALB", StringComparison.Ordinal))
			{
				// No additional data are processed.
				read_calb = true;
			}
			// Add a new line if:
			// * the data type is for FCST only and the FCST line has been read
			// * the data type is for CALB only and the CALB line has been read
			// * the data type is for FCST and CALB and both lines have been read
			// * the data type is neither FCST or CALB and two lines have been read
			if ((!in_fcst && in_calb && read_calb) || (in_fcst && read_fcst && !in_calb) || (in_fcst && read_fcst && in_calb && read_calb) || (!in_fcst && !in_calb && (type_count == 2)))
			{
				// Have complete data for the type so add as a new data type...
				type = new DataType();
				type.setAbbreviation(abbreviation);
				type.setDescription(description);
				if (define_dimensions)
				{
					// Define the dimension in the DataDimension global data so that it can be referenced
					// below.  It is OK to define more than once because DataDimension will keep only one
					// unique definition.
					DataDimension.addDimension(new DataDimension(dimension,dimension));
				}
				type.setDimension(dimension);
				// Determine if the data units are SI or English and then set as the default.
				try
				{
					units = DataUnits.lookupUnits(default_units);
					if (units != null)
					{
						if ((units.getSystem() == DataUnits.SYSTEM_ENGLISH) || (units.getSystem() == DataUnits.SYSTEM_ALL))
						{
							type.setDefaultEnglishUnits(default_units);
						}
						if ((units.getSystem() == DataUnits.SYSTEM_SI) || (units.getSystem() == DataUnits.SYSTEM_ALL))
						{
							type.setDefaultSIUnits(default_units);
						}
					}
				}
				catch (Exception)
				{
					// Ignore for now - default units just will not be set.
				}
				if ((meas_loc_type.IndexOf("A", StringComparison.Ordinal) >= 0) && (meas_loc_type.IndexOf("P", StringComparison.Ordinal) >= 0))
				{
					type.setMeasLocType(MeasLocType.AREA_OR_POINT);
				}
				else if (meas_loc_type.IndexOf("A", StringComparison.Ordinal) >= 0)
				{
					type.setMeasLocType(MeasLocType.AREA);
				}
				else if (meas_loc_type.IndexOf("P", StringComparison.Ordinal) >= 0)
				{
					type.setMeasLocType(MeasLocType.POINT);
				}
				// NWS types exactly match RTi internal code...
				type.setMeasTimeScale(meas_time_scale);
				addDataType(type);
			}
			}
			catch (Exception e)
			{
				Message.printWarning(2, routine, "Error reading data type at line " + linecount + " of file \"" + dfile + "\"");
				Message.printWarning(2, routine, e);
			}
		}
		fp.Close();
		// Add FMAP as a data type.  This is not in the DATAUNIT file because it is considered the
		// future part of MAP.  However, it has special meaning in the database files so treat as a
		// special case.
		// FIXME SAM  2008-05-03 Need constructor for DataType that takes all parameters and make DataType immutable
		DataType fmap = new DataType();
		fmap.setAbbreviation("FMAP");
		fmap.setDescription("Future Mean Areal Precipitation");
		fmap.setDefaultEnglishUnits("DEGF");
		fmap.setDefaultSIUnits("DEGC");
		fmap.setDimension("TEMP");
		addDataType(fmap);
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, e);
			// Global catch...
			throw new IOException("Error reading data type file \"" + dfile + "\"");
		}
	}

	/// <summary>
	/// Read a file that is in RTi format.
	/// This routine depends on on the values in an RTi DATAUNIT file.  The
	/// format for this file is as follows:
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
	/* TODO - need to define format - currently rely on NWS file on NWS systems,
	RiversideDB for RiverTrak systems and do not read on other systems.
	
	public static void readUnitsFile ( String dfile, boolean define_dimensions )
	throws IOException
	{	String	message, routine = "DataUnits.readUnitsFile";
		Vector	units_file = null;
	
		try {	// Main try...
	
		// Read the file into a list...
	
		try {	units_file = IOUtil.fileToStringList ( dfile );
		}
		catch ( Exception e ) {
			message = "Unable to read units file \"" + dfile + "\"";
			Message.printWarning ( 2, routine, message );
			throw new IOException ( message );
		}
		if ( units_file == null ) {
			message = "Empty contents for units file \"" + dfile + "\"";
			Message.printWarning ( 2, routine, message );
			throw new IOException ( message );
		}
		int nstrings = units_file.size();
		if ( nstrings == 0 ) {
			message = "Empty contents for units file \"" + dfile + "\"";
			Message.printWarning ( 2, routine, message );
			throw new IOException ( message );
		}
	
		// For each line, if not a comment, break apart and add units to the
		// global list...
	
		int		ntokens;
		DataUnits	units;
		String		string, token;
		Vector		tokens = null;
		char		first;
		for ( int i = 0; i < nstrings; i++ ) {
			try {
			string = (String)units_file.elementAt(i);
			if ( string == null ) {
				continue;
			}
			if ( string.length() == 0 ) {
				continue;
			}
			first = string.charAt(0);
			if (	(first == '#') ||
				(first == '\n') ||
				(first == '\r') ) {
				continue;
			}
			// Break the line...
			tokens = StringUtil.breakStringList (
				string, "|", 0 );
			if ( tokens == null ) {
				// A corrupt line...
				continue;
			}
			if ( tokens.size() < 7 ) {
				// A corrupt line...
				continue;
			}
			// Else add the units...
			units = new DataUnits ();
			if ( define_dimensions ) {
				// Define the dimension in the DataDimension global
				// data so that it can be referenced below.  It is OK
				// to define more than once because DataDimension will
				// keep only one unique definition.
				DataDimension.addDimension (
					new DataDimension(
						((String)tokens.elementAt(0)).trim(),
						((String)tokens.elementAt(0)).trim()));
			}
			units.setDimension ( ((String)tokens.elementAt(0)).trim() );
			token = (String)tokens.elementAt(1);
			if ( token.equalsIgnoreCase("BASE") ) {
				// Base units for the dimension...
				units.setBaseFlag ( 1 );
			}
			else {	units.setBaseFlag ( 0 );
			}
			units.setAbbreviation ( ((String)tokens.elementAt(2)).trim() );
			units.setSystem ( ((String)tokens.elementAt(3)).trim() );
			units.setLongName ( ((String)tokens.elementAt(4)).trim() );
			units.setOutputPrecision ( StringUtil.atoi(
				((String)tokens.elementAt(5)).trim()) );
			units.setMultFactor ( StringUtil.atod(
				((String)tokens.elementAt(6)).trim()) );
			units.setAddFactor ( StringUtil.atod(
				((String)tokens.elementAt(7)).trim()) );
	
			// Add the units to the list...
	
			addUnits ( units );
			}
			catch ( Exception e ) {
				Message.printWarning ( 2, routine,
				"Error reading units at line " + (i + 1) +
				" of file \"" + dfile + "\"" );
			}
		}
	
		// Check the units for consistency...
	
		checkUnitsData();
		units = null;
		string = null;
		token = null;
		tokens = null;
		}
		catch ( Exception e ) {
			Message.printWarning ( 2, routine, e );
			// Global catch...
			throw new IOException ( "Error reading units file \"" + dfile
			+ "\"" );
		}
		message = null;
		routine = null;
		units_file = null;
	}
	*/

	/// <summary>
	/// Set the abbreviation string for the data type. </summary>
	/// <param name="abbreviation"> Data type abbreviation (e.g., "MAP" for mean areal
	/// precipitation). </param>
	public virtual void setAbbreviation(string abbreviation)
	{
		if (string.ReferenceEquals(abbreviation, null))
		{
			return;
		}
		__abbreviation = abbreviation;
	}

	/// <summary>
	/// Set the default units for English units. </summary>
	/// <param name="default_engl_units"> Default English units. </param>
	public virtual void setDefaultEnglishUnits(string default_engl_units)
	{
		if (string.ReferenceEquals(default_engl_units, null))
		{
			return;
		}
		__default_engl_units = default_engl_units;
	}


	/// <summary>
	/// Set the default maximum for English units. </summary>
	/// <param name="default_engl_units"> Default English units. </param>
	public virtual void setDefaultEnglishMax(double default_engl_max)
	{
		__default_engl_max = default_engl_max;
	}

	/// <summary>
	/// Set the default minimum for English units. </summary>
	/// <param name="default_engl_units"> Default English units. </param>
	public virtual void setDefaultEnglishMin(double default_engl_min)
	{
		__default_engl_min = default_engl_min;
	}

	/// <summary>
	/// Set the default units for SI units. </summary>
	/// <param name="default_si_units"> Default SI units. </param>
	public virtual void setDefaultSIUnits(string default_si_units)
	{
		if (string.ReferenceEquals(default_si_units, null))
		{
			return;
		}
		__default_si_units = default_si_units;
	}

	/// <summary>
	/// Set the default maximum for SI units. </summary>
	/// <param name="default_engl_units"> Default English units. </param>
	public virtual void setDefaultSIMax(double default_si_max)
	{
		__default_si_max = default_si_max;
	}

	/// <summary>
	/// Set the default minimum for SI units. </summary>
	/// <param name="default_engl_units"> Default English units. </param>
	public virtual void setDefaultSIMin(double default_si_max)
	{
		__default_si_min = default_si_max;
	}

	/// <summary>
	/// Set the description for the data type (e.g., "MEAN AREAL PRECIPITATION"). </summary>
	/// <param name="description"> Long name for the data type. </param>
	public virtual void setDescription(string description)
	{
		if (string.ReferenceEquals(description, null))
		{
			return;
		}
		__description = description;
	}

	/// <summary>
	/// Set the dimension for the data type. </summary>
	/// <param name="dimension_string"> Dimension string (e.g., "L3/T"). </param>
	/// <exception cref="Exception"> If the dimension string to be used is not recognized. </exception>
	/// <seealso cref= DataDimension </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setDimension(String dimension_string) throws Exception
	public virtual void setDimension(string dimension_string)
	{
		string routine = "DataType.setDimension(String)";

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
			Message.printWarning(2, routine, message);
			throw new Exception(message);
		}

		// Now set the dimension...

		__dimension = dim;
	}

	/// <summary>
	/// Set the measurement location type. </summary>
	/// <param name="meas_loc_type"> Measurement location type. </param>
	public virtual void setMeasLocType(string meas_loc_type)
	{
		if (string.ReferenceEquals(meas_loc_type, null))
		{
			return;
		}
		__meas_loc_type = meas_loc_type;
	}

	/// <summary>
	/// Set the measurement time scale </summary>
	/// <param name="meas_time_scale"> Measurement time scale </param>
	public virtual void setMeasTimeScale(string meas_time_scale)
	{
		if (string.ReferenceEquals(meas_time_scale, null))
		{
			return;
		}
		__meas_time_scale = meas_time_scale;
	}

	/// <summary>
	/// Set the SHEF physical element code for this data type. </summary>
	/// <param name="SHEF_pe"> SHEF physical element code for this data type. </param>
	public virtual void setSHEFpe(string SHEF_pe)
	{
		if (string.ReferenceEquals(SHEF_pe, null))
		{
			return;
		}
		__SHEF_pe = SHEF_pe;
	}

	/// <summary>
	/// Return A string representation of the units (verbose). </summary>
	/// <returns> A string representation of the units (verbose). </returns>
	public override string ToString()
	{
		return __abbreviation + "|" + __description + "|" + __dimension.getAbbreviation() + "|" + __default_engl_units + "|" + __default_engl_max + "|" + __default_engl_min + "|" + __default_si_units + "|" + __default_si_max + "|" + __default_si_min + "|" + __meas_loc_type + "|" + __meas_time_scale + "|" + __record_type + "|" + __SHEF_pe + "|";
	}

	}

}