using System;
using System.Collections.Generic;
using System.IO;

// SHEFType - SHEF data type class

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
// SHEFType - SHEF data type class
// ----------------------------------------------------------------------------
// Copyright:  See the COPYRIGHT file.
// ----------------------------------------------------------------------------
// History:
//
// 2003-10-31	Steven A. Malers, RTi	Initial version.  Copy DataType and
//					update the code.  The design is meant to
//					be compatible with RTi's RiverTrak and
//					the NWSRFS, in particular to support
//					SHEF output of time series.
// 2003-11-18	SAM, RTi		When reading the NWS SHEFPPDB file, save
//					the fully-expanded SHEF PE code as the
//					PE.  It is not clear whether this will
//					cause other issues.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------
// EndHeader

namespace RTi.Util.IO
{

	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using TimeScaleType = RTi.Util.Time.TimeScaleType;

	/// <summary>
	/// The SHEFType class provides capabilities for reading and storing 
	/// SHEF (Standard Hydrologic Exchange Format) data type information.  This
	/// information is cross-referenced using the SHEF physical element (PE) code in
	/// the DataType class.  In general, the data listed in DataType are written to
	/// SHEF (using, for example RTi.TS.ShefATS), using the default information in
	/// this SHEFType class, in order to simplify data exchange.
	/// SHEF Data types are maintained internally using a list of SHEFType (self-referencing).
	/// </summary>
	public class SHEFType
	{

	// Data members.
	// If the data units being written match the __units_engl and __units_si below,
	// then no units information needs to be written to the SHEF file, resulting
	// in shorter SHEF files.

	private string __SHEF_pe; // The SHEF physical element data type.
	private string __description; // The verbose description (e.g.,
						// ("Mean Areal Precipitation").
	private string __default_interval_base; // Default time interval base ("INST"
						// for instantaneous, "DAY", etc.)
						// REVISIT - make exactly compatible
						// with TimeInterval?
	private int __default_interval_mult; // Multiplier for
						// __default_interval_base
	private string __units_engl; // Default English units.
	private string __units_si; // Default SI units.
	private string __meas_time_scale; // Measurement time scale, from the
						// MeasTimeScale class.
						// See MeasTimeScale static data.

	// The following list can be modified  by the static functions...

	private static IList<SHEFType> __types_Vector = new List<SHEFType>();
						// Vector of internally-maintained available SHEF data types.

	/// <summary>
	/// Construct and set all data members to empty strings and zeros.
	/// </summary>
	public SHEFType()
	{
		initialize();
	}

	/// <summary>
	/// Construct using the individual data items. </summary>
	/// <param name="dimension"> Units dimension (see DataDimension). </param>
	/// <param name="base_flag"> 1 if the units are the base units for conversion purposes,
	/// for the dimension. </param>
	/// <param name="abbreviation"> Abbreviation for the units. </param>
	/// <param name="long_name"> Long name for the units. </param>
	/// <param name="output_precision"> The output precision for the units (the number of
	/// digits output after the decimal point). </param>
	/// <param name="mult_factor"> Multiplication factor used when converting to the base units
	/// for the dimension. </param>
	/// <param name="add_factor"> Addition factor used when converting to the base units
	/// for the dimension. </param>
	/// <seealso cref= DataDimension </seealso>
	/* REVISIT
	public DataUnits (	String dimension, int base_flag, String abbreviation,
				String long_name, int output_precision,
				double mult_factor, double add_factor )
	{	initialize ();
		try {	setDimension ( dimension );
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
	/// <param name="type"> Instance of SHEFType to copy. </param>
	public SHEFType(SHEFType type)
	{
		initialize();
		__SHEF_pe = type.__SHEF_pe;
		__description = type.__description;
		__default_interval_base = type.__default_interval_base;
		__default_interval_mult = type.__default_interval_mult;
		__units_engl = type.__units_engl;
		__units_si = type.__units_si;
		__meas_time_scale = type.__meas_time_scale;
	}

	/// <summary>
	/// Add a SHEF data type to the internal list of data types.  After adding, the data
	/// type can be used throughout an application. </summary>
	/// <param name="type"> Instance of SHEFType to add to the list. </param>
	public static void addSHEFType(SHEFType type)
	{ // First see if the type is already in the list...

		int size = __types_Vector.Count;
		SHEFType pt = null;
		for (int i = 0; i < size; i++)
		{
			// Get the type for the loop index...
			pt = __types_Vector[i];
			// Now compare...
			if (type.getSHEFpe().Equals(pt.getSHEFpe(), StringComparison.OrdinalIgnoreCase))
			{
				// The requested units match something that is
				// already in the list.  Reset the list...
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
	~SHEFType()
	{
		__description = null;
		__default_interval_base = null;
		__units_engl = null;
		__units_si = null;
		__meas_time_scale = null;
		__SHEF_pe = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the list of SHEF data types data. </summary>
	/// <returns> the list of SHEF data types (useful for debugging and GUI displays).
	/// Perhaps later overload to request by dimension, system, etc. </returns>
	public static IList<SHEFType> getSHEFTypesData()
	{
		return __types_Vector;
	}

	/// <summary>
	/// Return the description for the units. </summary>
	/// <returns> The description for the units. </returns>
	public virtual string getDescription()
	{
		return __description;
	}

	/// <summary>
	/// Return the English units. </summary>
	/// <returns> the SI units. </returns>
	public virtual string getEnglishUnits()
	{
		return __units_engl;
	}

	/// <summary>
	/// Return the default data interval base. </summary>
	/// <returns> the default data interval base. </returns>
	public virtual string getDefaultIntervalBase()
	{
		return __default_interval_base;
	}

	/// <summary>
	/// Return the default data interval multiplier. </summary>
	/// <returns> the default data interval multiplier. </returns>
	public virtual int getDefaultIntervalMult()
	{
		return __default_interval_mult;
	}

	/// <summary>
	/// Return the measurement time scale. </summary>
	/// <returns> the measurement time scale. </returns>
	public virtual string getMeasTimeScale()
	{
		return __meas_time_scale;
	}

	/// <summary>
	/// Return the SHEF physical element data type. </summary>
	/// <returns> the SHEF physical element data type. </returns>
	public virtual string getSHEFpe()
	{
		return __SHEF_pe;
	}

	/// <summary>
	/// Return the SI units. </summary>
	/// <returns> the SI units. </returns>
	public virtual string getSIUnits()
	{
		return __units_si;
	}

	/// <summary>
	/// Initialize data members.
	/// </summary>
	private void initialize()
	{
		__SHEF_pe = "";
		__description = "";
		__default_interval_base = "";
		__default_interval_mult = 1;
		__units_engl = "";
		__units_si = "";
		__meas_time_scale = "";
	}

	/// <summary>
	/// Return the matching SHEFType instance, given the SHEF PE.  A copy is not made. </summary>
	/// <returns> A SHEFType instance, given the SHEF PE. </returns>
	/// <param name="type_string"> The SHEF PE data type abbreviation to look up. </param>
	/// <exception cref="Exception"> If there is a problem looking up the data type
	/// abbreviation. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static SHEFType lookupSHEFType(String type_string) throws Exception
	public static SHEFType lookupSHEFType(string type_string)
	{
		string routine = "SHEFType.lookupSHEFType";

		// First see if the data type is already in the list...

		int size = __types_Vector.Count;
		SHEFType pt = null;
		for (int i = 0; i < size; i++)
		{
			pt = (SHEFType)__types_Vector[i];
			if (Message.isDebugOn)
			{
				Message.printDebug(20, routine, "Comparing " + type_string + " and " + pt.getSHEFpe());
			}
			if (type_string.Equals(pt.getSHEFpe(), StringComparison.OrdinalIgnoreCase))
			{
				// The requested data type match something that is
				// in the list.  Return the matching SHEFType...
				return pt;
			}
		}
		// Throw an exception...
		throw new Exception("\"" + type_string + "\" SHEF PE data type not found");
	}

	/// <summary>
	/// Read a file that is in NWS SHEFPPDB format, which contains translation
	/// information needed to convert SHEF codes to the NWS Preprocessor Database
	/// (PPDB) codes.  The NWS DATATYPE file should be
	/// read before calling this method, using DataType.readNWSDataTypeFile().  The
	/// contents of SHEFPPDB are used to populate SHEFType objects maintained as a
	/// list in this class and are also to set the SHEF PE value in the DataType objects
	/// that are defined, where a match is found.  This allows the data types to be
	/// used to look up the SHEF PE, which can then be used to look up the specific
	/// information for the SHEF output.
	/// This routine depends on on the values in the SHEFPPDB file orignally supplied
	/// by the NWS.  The file is normally named
	/// /awips/hydroapps/lx/rfc/nwsrfs/sys_files/SHEFPPDB and has the format:
	/// <para>
	/// <pre>
	/// $  12/18/2001  SHEFPPDB
	/// 
	/// $  INFORMATION TO CONVERT SHEF PE CODES TO OFS PPDB CODES
	/// 
	/// $ PPDB DAILY DATA TYPES:
	/// PP01 PPHRZZZ  PP 1001 0060 6 R Z
	/// PP03 PPTRZZZ  PP 1003      3 R Z
	/// PP06 PPQRZZZ  PP 1006      4 R Z
	/// PP24 PPDRZZZ  PP 2001 1024 2 R Z
	/// PP24 PPPRZZZ  PP 5004      2 R Z
	/// RC24 XCDPZZZ  XC 2001 1024 2 P Z
	/// RI24 RIDRZZZ  RI 2001 1024 2 R Z
	/// RP24 RPDRZZZ  RP 2001 1024 2 R Z
	/// TA01 TAIRZZZ  TA  0        6 R Z
	/// TA03 TAIRZZZ  TA  0        3 R Z
	/// TA06 TAIRZZZ  TA  0        4 R Z
	/// TA24 TADRZZZ  TA 2001 1024 2 R Z
	/// TD24 TDDRZZZ  TD 2001 1024 2 R Z
	/// TFMN TAIFZNZ  TA  0        1 F N
	/// TFMX TAIFZXZ  TA  0        1 F X
	/// TN24 TAIRMNZ  TA  0        7 R N
	/// TN24 TAIRZNZ  TA  0        2 R N
	/// TN24 TAIRZPZ  TA  0        2 R P
	/// TX24 TAIRZXZ  TA  0        8 R X
	/// US24 ULDRZZZ  UL 2001 1024 2 R Z
	/// US24 USDRZZZ  US 2001 1024 2 R Z
	/// 
	/// $ PPDB RRS DATA TYPES:
	/// AESC SAIRZZZ  SA  0        0 R Z
	/// DQIN QDIFZZZ  QD  0        0 F Z
	/// DQIN QDIRZZZ  QD  0        0 R Z
	/// DQME QD FZZZ  QD   -1      0 F Z
	/// DQME QD RZZZ  QD   -1      0 R Z
	/// FBEL HFIRZZZ  HF  0        0 R Z
	/// FGDP GDIRZZZ  GD  0        0 R Z
	/// GATE NGIRZZZ  NG  0        0 R Z
	/// ICET ITIRZZZ  IT  0        0 R Z
	/// LAKH HKIRZZZ  HK  0        0 R Z
	/// LELV HLIRZZZ  HL  0        0 R Z
	/// PELV HPIRZZZ  HP  0        0 R Z
	/// QIN  QRIRZZZ  QR  0        0 R Z
	/// QME  QR RZZZ  QR   -1      0 R Z
	/// RQGM QG RZZZ  QG   -1      0 R Z
	/// RQIM QI RZZZ  QI   -1      0 R Z
	/// RQIN QIIRZZZ  QI  0        0 R Z
	/// RQME QT FZZZ  QT   -1      0 F Z
	/// RQME QT RZZZ  QT   -1      0 R Z
	/// RQOT QTIFZZZ  QT  0        0 F Z
	/// RQOT QTIRZZZ  QT  0        0 R Z
	/// RQSW QSIRZZZ  QS  0        0 R Z
	/// RSTO LSIRZZZ  LS  0        0 R Z
	/// SNOG SDIRZZZ  SD  0        0 R Z
	/// SNWE SWIRZZZ  SW  0        0 R Z
	/// STG  HGIRZZZ  HG  0        0 R Z
	/// TWEL HTIRZZZ  HT  0        0 R Z
	/// TWSW HWIRZZZ  HW  0        0 R Z
	/// ZELV HZIFZZZ  HZ  0        0 F Z
	/// ZELV HZIRZZZ  HZ  0        0 R Z
	/// 
	/// END
	/// </pre>
	/// The records are described as follows, by character position:
	/// <ul>
	/// <li>	1-4 (characters) is the PPDB data type (stored in DataType as the
	/// abbreviation).</li>
	/// <li>	6-12 (characters) is the expanded SHEF code which will be found in
	/// SHEF data (this is stored in DataType as the SHEF PE code).</li>
	/// <li>	15-16 (characters) is the SHEF PE code.  Currently ignored in favor of
	/// the previous item.</li>
	/// <li>	18-21 (integer) is the first duration code, which is the integer
	/// representation of the duration used in the SHEF file.  The code
	/// specifies the units of time and the number of units as follows:
	/// <pre>
	/// 0XXX minutes
	/// 1XXX hours
	/// 2XXX days
	/// 5004 time period beginning at 7 AM local time prior to the observation
	/// time ending at the observation time.
	/// 
	/// where XXX is the number of units (i.e., 6 hours is 1006, 1 day is 2001,
	/// instantaneous is 0)
	/// </pre>
	/// A duration code of -1 is used to specify that any duration from 1 to 24
	/// hours is acceptable (durations are rounded to the nearest hour;
	/// durations of less than 30 minutes are treated as an instantaneous
	/// observation).
	/// </li>
	/// <li>	23-26 (integer) is the second duration code (zero or blank if none).
	/// For example, 2001 (1 day) could have a second duration code of
	/// 1024 (24 hour).
	/// </li>
	/// <li>	28 (integer) is the observation hour code, which specifies the allowable
	/// ending hour or range of ending hours of the observation.  The codes are
	/// as follows (HZ indicates the beginning and ending hour of the hydrologic
	/// day, usually 12Z).
	/// <pre>
	/// Code Definition
	/// 0    any hour
	/// 1    HZ
	/// 2    HZ +- 2
	/// 3    HZ +- 3*n (n=0,7)
	/// 4    HZ +- 6*n (n=0,3)
	/// 5    HZ + 12
	/// 6    HZ + n
	/// 7    HZ -8/HZ + 2
	/// 8    HZ -8/HZ + 2
	/// </pre>
	/// </li>
	/// <li>	30 (character) Type code.</li>
	/// <li>	32 (character) Extreme code.</li>
	/// </ul>
	/// </para>
	/// </summary>
	/// <param name="dfile"> NWS SHEFPPDB file to read (can be a URL). </param>
	/// <param name="update_datatypes"> If true, then the DataType data will be checked for
	/// a matching data type and if a match is found, its SHEF PE data will be updated
	/// to match the values read from the SHEFPPDB file. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void readNWSSHEFPPDBFile(String dfile, boolean update_datatypes) throws java.io.IOException
	public static void readNWSSHEFPPDBFile(string dfile, bool update_datatypes)
	{
		string routine = "SHEFType.readNWSSHEFPPDBFile", @string;
		StreamReader fp = null;

		try
		{ // Main try...
		// Open the file (allow the file to be a normal file or a URL
		// so web applications can also be supported)...
		try
		{
			fp = new StreamReader(IOUtil.getInputStream(dfile));
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, e);
			throw new IOException("Error opening SHEFPPDB file \"" + dfile + "\"");
		}
		int linecount = 0;
		SHEFType type = null;
		IList<object> tokens = new List<object>(8);
					// Tokens from data lines - share the list
					// between multiple reads.
		// Format to read the first data line per data type...
		int[] format = new int[] {StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING};
		int[] format_w = new int[] {4, 1, 7, 2, 2, 1, 4, 1, 4, 1, 1, 1, 1, 1, 1};
		string datatype = null; // Data type abbreviation.
		string SHEF_pe_long = null;
					// SHEF pe code - fully expanded.
		string duration = null; // Duration code
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
			{ // If exceptions are caught, ignore the data..
			@string = @string.Trim();
			if (@string.Length == 0)
			{
				// Skip blank lines...
				continue;
			}
			if (@string[0] == '$')
			{
				// A comment line...
				continue;
			}
			if (@string.regionMatches(true,0,"END",0,3))
			{
				// End of file...
				break;
			}
			// Add as a new SHEF type...
			type = new SHEFType();
			StringUtil.fixedRead(@string, format, format_w, tokens);
			datatype = ((string)tokens[0]).Trim();
			// Expanded SHEF code, parts of which are used to help assign
			// data...
			SHEF_pe_long = ((string)tokens[1]).Trim();
			// TODO.
			// Don't assign default units to SHEFType - take from the time
			// series at output - for parsing input, these will need to be
			// defined.
			// Determine the duration and time scale from the duration code
			// as follows:
			//
			// -1 indicates mean value ("MEAN") - default to 24 hour
			// 0 indicates instantaneous value ("INST")
			// >0 indicates accumulated value ("ACCM") - REVISIT - this does
			//	not quite work with temperatures - this is just a
			//	default and the DataType entries should take precedence
			// 
			duration = ((string)tokens[3]).Trim();
			if (duration.Equals("-1"))
			{
				type.setDefaultIntervalBase("DAY");
				type.setDefaultIntervalMult(1);
				type.setMeasTimeScale("" + TimeScaleType.MEAN);
			}
			else if (duration.Equals("0"))
			{
				type.setMeasTimeScale("" + TimeScaleType.INST);
			}
			else
			{
				// duration > 0
				type.setMeasTimeScale("" + TimeScaleType.ACCM);
				// Also evaluate the full SHEF, which has more detailed
				// information about the interval...
				if (SHEF_pe_long[2] == 'H')
				{
					type.setDefaultIntervalBase("HOUR");
					type.setDefaultIntervalMult(1);
				}
				else if (SHEF_pe_long[2] == 'T')
				{
					type.setDefaultIntervalBase("HOUR");
					type.setDefaultIntervalMult(3);
				}
				else if (SHEF_pe_long[2] == 'Q')
				{
					type.setDefaultIntervalBase("HOUR");
					type.setDefaultIntervalMult(6);
				}
				else if (SHEF_pe_long[2] == 'D')
				{
					type.setDefaultIntervalBase("DAY");
					type.setDefaultIntervalMult(1);
				}
			}
			type.setSHEFpe(SHEF_pe_long);
			addSHEFType(type);
			if (update_datatypes)
			{
				// Try to find a matching data type and set its SHEF_pe
				try
				{ //Message.printStatus ( 1, "",
					//"SAMX - looking for \"" + datatype + "\"" );
					DataType dt = DataType.lookupDataType(datatype);
					//Message.printStatus ( 1, "",
					//"SAMX - found \"" + datatype +
					//"\". Set PE to \"" + SHEF_pe + "\"" );
					dt.setSHEFpe(SHEF_pe_long);
				}
				catch (Exception)
				{
					// Ignore.
					//Message.printStatus ( 1, "",
					//"SAMX - did not find \"" + datatype + "\"" );
				}
			}
			}
			catch (Exception e)
			{
				Message.printWarning(2, routine, "Error reading SHEFPPDB file at line " + linecount + " of file \"" + dfile + "\"");
				Message.printWarning(2, routine, e);
			}
		}
		fp.Close();
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, e);
			// Global catch...
			throw new IOException("Error reading data type file \"" + dfile + "\"");
		}
		fp = null;
	}

	/// <summary>
	/// Set the default data interval base (see TimeInterva.*). </summary>
	/// <param name="default_interval_base"> The default data interval base. </param>
	public virtual void setDefaultIntervalBase(string default_interval_base)
	{
		__default_interval_base = default_interval_base;
	}

	/// <summary>
	/// Set the default data interval multiplier. </summary>
	/// <param name="default_interval_mult"> The default data interval multiplier. </param>
	public virtual void setDefaultIntervalMult(int default_interval_mult)
	{
		__default_interval_mult = default_interval_mult;
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
	/// Set the SHEF physical element code (currently the fully expanded code). </summary>
	/// <param name="SHEF_pe"> ShEF physical element code. </param>
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
		return __SHEF_pe + "|" + __description + "|" + __default_interval_base + "|" + __default_interval_mult + "|" + __units_engl + "|" + __units_si + "|" + __meas_time_scale + "|";
	}

	} // End SHEFType

}