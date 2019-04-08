using System;
using System.Collections.Generic;
using System.IO;

// ShefATS - class for I/O of '.A' type SHEF (Standard Hydrological Exchange Format) time series

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
// ShefATS - class for I/O of '.A' type SHEF (Standard Hydrological Exchange
//				Format) time series
// ----------------------------------------------------------------------------
// History:
//
// 2001-06-13	Michael Thiemann, RTi	Initial version (C++).
// 2003-07-22	Steven A. Malers, RTi	Port to Java.  Include fewer methods
//					than the C++ and utilize a PropList
//					to pass parameters, since there are
//					so many options.
// 2003-11-03	SAM, RTi		* Finish initial port, for testing with
//					  the Idaho Power system.
//					* Add more Javadoc.
//					* When writing time series, check the
//					  TS start date time zone.
// 2003-11-19	SAM, RTi		* Add comments to the top of output.
//					* Remove overloaded methods that were
//					  commented out.
// 2003-11-24	SAM, RTi		* Duration was being written twice when
//					  specified as input.
// ----------------------------------------------------------------------------
// EndHeader

namespace RTi.TS
{

	using DataType = RTi.Util.IO.DataType;
	using DataUnits = RTi.Util.IO.DataUnits;
	using DataUnitsConversion = RTi.Util.IO.DataUnitsConversion;
	using IOUtil = RTi.Util.IO.IOUtil;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using DateTime = RTi.Util.Time.DateTime;
	using TimeInterval = RTi.Util.Time.TimeInterval;
	using TZ = RTi.Util.Time.TZ;
	using TimeScaleType = RTi.Util.Time.TimeScaleType;

	/// <summary>
	/// This ShefATS class provides static methods for reading and writing .A format
	/// SHEF (Standard Hydrological Exchange Format) time series.  Currently, only write methods are available.
	/// </summary>
	public class ShefATS
	{

	/// <summary>
	/// Use the in-memory DataType data to lookup the SHEF PE codes for a list of time
	/// series, using the time series data types.  These values can be used when calling
	/// writeTimeSeriesList().  A blank PE code will be assigned if
	/// it is not found, which will cause the time series to not be written.
	/// </summary>
	public static IList<string> getPEForTimeSeries(IList<TS> tslist)
	{
		string routine = "ShefATS.getPEForTimeSeries";
		IList<string> v = new List<string>();
		int size = 0;
		if (tslist != null)
		{
			size = tslist.Count;
		}
		TS ts;
		DataType datatype;
		for (int i = 0; i < size; i++)
		{
			ts = tslist[i];
			if (ts == null)
			{
				v.Add("");
				continue;
			}
			try
			{
				datatype = DataType.lookupDataType(ts.getDataType());
				v.Add(datatype.getSHEFpe());
			}
			catch (Exception)
			{
				Message.printWarning(3, routine, "Unable to look up data type for \"" + ts.getIdentifierString() + "\" uniquetempvar.  Setting SHEF PE to blank.");
				v.Add("");
				continue;
			}
		}
		return v;
	}

	/// <summary>
	/// Get a SHEF time zone from another time zone string.  Recognized SHEF time zones
	/// are taken from Table 8 of the SHEF handbook. </summary>
	/// <returns> a recognized SHEF time zone matching the characteristics of the specified time zone. </returns>
	/// <param name="ts_tz"> Time zone abbreviation recognized by the RTi.Util.Time.TZ class. </param>
	/// <exception cref="Exception"> if the specified time zone cannot be converted to a recognized SHEF time zone. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static String getSHEFTimeZone(String ts_tz) throws Exception
	public static string getSHEFTimeZone(string ts_tz)
	{ // Important time zones known to be SHEF time zones...
		string[] shef_tz = new string[] {"N", "NS", "A", "AD", "AS", "E", "ED", "ES", "C", "CD", "CS", "J", "M", "MD", "MS", "P", "PD", "PS", "Y", "YD", "YS", "H", "HS", "L", "LD", "LS", "B", "BD", "BS", "Z"};
		// Loop through the time zones.  If the abbreviation exactly matches a SHEF time zone, assume that it is OK.
		for (int i = 0; i < shef_tz.Length; i++)
		{
			if (ts_tz.Equals(shef_tz[i], StringComparison.OrdinalIgnoreCase))
			{
				return shef_tz[i]; // Return the SHEF value in case there is an upper/lower case issue.
			}
		}
		// Get time zones with the same characteristics...
		IList<TZ> matching_tz = TZ.getMatchingDefinedTZ(ts_tz, false);
		int size = 0;
		if (matching_tz != null)
		{
			size = matching_tz.Count;
		}
		// Loop through the matches and compare with the SHEF time zones that
		// are know.  If a match occurs, then return the matching SHEF time zone...
		string tz_abbrev;
		for (int j = 0; j < size; j++)
		{
			tz_abbrev = matching_tz[j].getAbbreviation();
			for (int i = 0; i < shef_tz.Length; i++)
			{
				if (tz_abbrev.Equals(shef_tz[i], StringComparison.OrdinalIgnoreCase))
				{
					return shef_tz[i];
				}
			}
		}
		throw new Exception("Unable to find SHEF time zone for \"" + ts_tz + "\"");
		//return null;	// Compiler complains if no return.
	}

	/// <summary>
	/// Write a single time series to a SHEF .A format file. </summary>
	/// <param name="ts"> Single time series to write. </param>
	/// <param name="fname"> Name of file to write.  The IOUtil.getPathUsingWorkingDir() method is applied to the filename. </param>
	/// <param name="append"> indicate whether to append to the file. </param>
	/// <param name="date1"> First date to write (if NULL write the entire time series). </param>
	/// <param name="date2"> Last date to write (if NULL write the entire time series). </param>
	/// <param name="units"> Units to write.  If different than the current units the units
	/// will be converted on output.  Units are defined in Appendix A of the SHEF documentation. </param>
	/// <param name="write_data"> Indicates whether data should be written (as opposed to only writing the header). </param>
	/// <param name="PE"> physical element corresponding to the time series data type,
	/// according to Table 9 of the SHEF documentation.  This is required. </param>
	/// <param name="duration"> Duration of the data, as defined by tables 3 and 11 of the SHEF documentation. </param>
	/// <param name="alt_id"> Aternate identifier, different from the time series identifier
	/// location, to be used in SHEF output. </param>
	/// <param name="props"> See the overloaded method for a description. </param>
	/// <exception cref="Exception"> if there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeTimeSeries(TS ts, String fname, boolean append, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2, String units, boolean write_data, String PE, String alt_id, String timeZone, String observationTime, String creationDate, String duration, int hourMax, int precision) throws Exception
	public static void writeTimeSeries(TS ts, string fname, bool append, DateTime date1, DateTime date2, string units, bool write_data, string PE, string alt_id, string timeZone, string observationTime, string creationDate, string duration, int hourMax, int precision)
	{ // Call the fully-loaded method...
		IList<TS> v = new List<TS>(1);
		v.Add(ts);
		IList<string> PEList = null, UnitsList = null, DurationList = null, AltIDList = null;
		if ((!string.ReferenceEquals(PE, null)) && !PE.Equals(""))
		{
			PEList = new List<string>();
			PEList.Add(PE);
		}
		if ((!string.ReferenceEquals(units, null)) && !units.Equals(""))
		{
			UnitsList = new List<string> (1);
			UnitsList.Add(units);
		}
		if ((!string.ReferenceEquals(alt_id, null)) && !alt_id.Equals(""))
		{
			AltIDList = new List<string> (1);
			AltIDList.Add(alt_id);
		}
		if ((!string.ReferenceEquals(duration, null)) && !duration.Equals(""))
		{
			DurationList = new List<string> (1);
			DurationList.Add(duration);
		}

		writeTimeSeriesList(v, fname, append, date1, date2, UnitsList, PEList, DurationList, AltIDList, timeZone, observationTime, creationDate, duration, hourMax, precision);
	}

	/// <summary>
	/// Write a list of time series to a SHEF A format file. </summary>
	/// <param name="tslist"> list of time series to write. </param>
	/// <param name="out"> PrintWriter to write to. </param>
	/// <param name="append"> indicate whether to append (if true, write and abbreviated header). </param>
	/// <param name="date1"> First date to write (if null write the entire time series). </param>
	/// <param name="date2"> Last date to write (if null write the entire time series). </param>
	/// <param name="unitsList"> List of units other than the default, if not an empty list, then one per time series. </param>
	/// <param name="PE"> Vector of PE Physical element codes (see SHEF Handbook), one per time series. </param>
	/// <param name="DurList"> list of duration codes (see SHEF Handbook), one per time series. </param>
	/// <param name="AltID"> list of alternate identifiers to output (default is to use the TS location). </param>
	/// <param name="timeZone"> the time zone abbreviation, which be applied for all SHEF messages.
	/// The time zone should match a value from according to Table 8 of the SHEF
	/// documentation.  If null or blank, the time zone from the time series start date
	/// will be used.  If that is blank, "Z" will be used.  If a non-null time zone is
	/// determined from the parameter or time series, an attempt will be made to convert
	/// the time zone to standard SHEF time zones (e.g., "MST" becomes "MS").  Note
	/// that the time series time zone, if known internally is not shifted (data remain
	/// in the same hour as originally read). </param>
	/// <param name="observationTime"> the observation time part of the SHEF message, which will be applied to all records
	/// that are output.  Specifying this string may be needed in cases where the
	/// default cannot be determined correctly (e.g., to specify an observation time of of "DH1200"
	/// for forecasted daily maximum and minimum temperatures).  By default the observation time will be determined
	/// from the time series.  For example, a 24Hour or Day interval would result in "DH2400".  If a number is specified,
	/// the "DH" or other prefix will automatically be added. </param>
	/// <param name="creationDate"> the creation date part of the SHEF message, which will be applied to all
	/// records that are output (e.g., "DCYYMMDDHHmm").  If null or blank no creation date is included in
	/// the output output.  If a number, "DC" will automatically be added. </param>
	/// <param name="duration"> the duration part of the SHEF message, which will be applied to all records
	/// that are output.  Specifying this string may be needed in cases where the
	/// default cannot be determined correctly (e.g., to specify a duration of "DVH06"
	/// to specify six hour duration.  The default is to determine the duration from the data interval of the time series.  For
	/// example, a 24Hour interval would result in "DVH24". </param>
	/// <param name="hourMax"> the maximum integer hour in the day.  Specify 24 (for 1-24 clock) or any other value for 0-23 clock. </param>
	/// <param name="precision"> the precision for output, 0+ (default is to use the units to determine precision, or 2 if unable). </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeTimeSeriesList(java.util.List<TS> tslist, java.io.PrintWriter out, boolean append, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2, java.util.List<String> unitsList, java.util.List<String> PE, java.util.List<String> DurList, java.util.List<String> AltID, String timeZone, String observationTime, String creationDate, String duration, int hourMax, int precision) throws Exception
	public static void writeTimeSeriesList(IList<TS> tslist, PrintWriter @out, bool append, DateTime date1, DateTime date2, IList<string> unitsList, IList<string> PE, IList<string> DurList, IList<string> AltID, string timeZone, string observationTime, string creationDate, string duration, int hourMax, int precision)
	{
		string message, routine = "ShefATS.writeTimeSeriesList";
		DateTime ts_start = null, ts_end = null;

		// Check for a null time series list...

		if (tslist == null)
		{
			Message.printWarning(3, routine, "Null time series list.  Not writing SHEFA file.");
			return;
		}

		int size = tslist.Count;
		if (size == 0)
		{
			Message.printWarning(3, routine, "No time series in list.  Not writing.");
			return;
		}

		// Check properties - set strings to null if blank to simplify logic below.

		// Convert the time zone to the value that SHEF will use

		string SHEFTimeZone = "";
		if ((!string.ReferenceEquals(timeZone, null)) && (timeZone.Trim().Length > 0))
		{
			// Use the specified time zone for all time series.
			SHEFTimeZone = timeZone;
		}
		else
		{
			// Default is Zulu...
			SHEFTimeZone = "Z";
		}

		// Observation time can be not specified, an integer or D...

		bool observationTimeLiteral = false; // Does the observation time have characters - hence us as is
		if ((!string.ReferenceEquals(observationTime, null)) && (observationTime.Trim().Length == 0))
		{
			// Set to null to simplify logic below
			observationTime = null;
		}
		if (!string.ReferenceEquals(observationTime, null))
		{
			if (!StringUtil.isInteger(observationTime))
			{
				// Observation time is being specified as a literal and will be included as is
				// (otherwise the DH etc will be added as appropriate)
				observationTimeLiteral = true;
			}
		}

		// Creation date can be not specified, an integer or DC...

		string creationDateOutput = ""; // No creation date
		if ((!string.ReferenceEquals(creationDate, null)) && (creationDate.Trim().Length == 0))
		{
			// Set to null to simplify logic below
			creationDate = null;
		}
		if (!string.ReferenceEquals(creationDate, null))
		{
			if (!StringUtil.isInteger(creationDate))
			{
				// Creation date is being specified as a literal and will be included as is
				creationDateOutput = "/" + creationDate;
			}
			else
			{
				// Add the leading "DC"
				creationDateOutput = "/DC" + creationDate;
			}
		}

		// Duration can be not specified, or as a literal...

		if ((!string.ReferenceEquals(duration, null)) && (duration.Trim().Length == 0))
		{
			// Set to null to simplify logic below
			duration = null;
		}

		// Use the requested output period if not null...

		if (date1 != null)
		{
			ts_start = new DateTime(date1);
		}
		if (date2 != null)
		{
			ts_end = new DateTime(date2);
		}

		// Check if alternative IDs are defined
		bool alternativeIDDefined = false;
		if ((AltID != null) && (AltID.Count == size))
		{
			alternativeIDDefined = true;
		}

		// Check if durations are defined
		bool durationDefined = false;
		if ((DurList != null) && (DurList.Count == size))
		{
			durationDefined = true;
		}

		TS ts = null;

		bool newUnitsDefined = false;
		if ((unitsList != null) && (unitsList.Count == size))
		{
			newUnitsDefined = true;
		}

		double value = 0.0;
		double mult, add;
		int system, newSystem; // Units system
		bool scale = false;
		string durationCode = "";
		DateTime t;

		// Output some header comments...

		if (append)
		{
			// Add a short note.
			@out.println(":");
			@out.println(": Appended records...");
			@out.println(":");
		}
		else
		{
			// Output the full header
			@out.println(": SHEF A file");
			IOUtil.printCreatorHeader(@out, ":", 80, 0);
		}
		// Always print the list of time series...
		@out.println(":");
		@out.println(": Time series and requested output are as follows.");
		@out.println(": Blanks indicate that values will be determined automatically.");
		@out.println(":");
		string units, pe, dur, altid;
		@out.println(":Count Location        Units PE      Duration AltID");
		@out.println(":");
		for (int i = 0; i < size; i++)
		{
			ts = tslist[i];
			if (ts == null)
			{
				continue;
			}
			units = "";
			pe = "";
			dur = "";
			altid = "";
			if ((unitsList != null) && (unitsList.Count == size))
			{
				units = unitsList[i];
			}
			if ((PE != null) && (PE.Count == size))
			{
				pe = PE[i];
			}
			if ((DurList != null) && (DurList.Count == size))
			{
				dur = DurList[i];
			}
			if ((AltID != null) && (AltID.Count == size))
			{
				altid = AltID[i];
			}
			@out.println(": " + StringUtil.formatString((i + 1),"%4d") + " " + StringUtil.formatString(ts.getLocation(),"%-15.15s") + " " + StringUtil.formatString(units,"%-5.5s") + " " + StringUtil.formatString(pe,"%-7.7s") + " " + StringUtil.formatString(dur,"%-8.8s") + " " + StringUtil.formatString(altid,"%s"));
		}
		// List in order of SHEF format left to right
		@out.println(":");
		@out.println(": Override properties for output are as follows:");
		@out.println(":");
		if (string.ReferenceEquals(timeZone, null))
		{
			@out.println(": TimeZone = default (Z).");
		}
		else
		{
			@out.println(": TimeZone = " + timeZone);
		}
		if (string.ReferenceEquals(observationTime, null))
		{
			@out.println(": ObservationTime = default (determined from data)");
		}
		else
		{
			@out.println(": ObservationTime = " + observationTime);
		}
		if (string.ReferenceEquals(creationDate, null))
		{
			@out.println(": CreationDate = default (not used)");
		}
		else
		{
			@out.println(": CreationDate = " + creationDate);
		}
		if (string.ReferenceEquals(duration, null))
		{
			@out.println(": Duration = default (determined from data interval)");
		}
		else
		{
			@out.println(": Duration = " + duration);
		}
		if (hourMax == 24)
		{
			@out.println(": HourMax = 24.  Hours are 1-24");
		}
		else
		{
			@out.println(": HourMax = default (23).  Hours are 0-23");
		}
		if (precision < 0)
		{
			@out.println(": Precision = default (from units, or 2).");
		}
		else
		{
			@out.println(": Precision = " + precision);
		}
		@out.println(":");

		DataUnits tsUnits;
		DataUnitsConversion conversion;
		string SHEFID, SHEFMessage;
		string dateString, qualityFlag;
		string PhysicalElement, scaleType;
		string SHEFSystem, unitsFormat;
		int tsMult, tsBase;

		TSIterator tsi;

		// Loop through the time series...

		string SHEFTimeZone_ts = SHEFTimeZone; // SHEF time zone to be used for a time series.
		string timeZone_ts = null; // Time zone from a time series start date/time.
		//String creation_date=creation_date_prop;// Used to indicate when the data record was created.
		string observationTimeOutput = null; // The observation time that is actually output
		string observationTimePrefix = "DH"; // Default for hourly data
		for (int i = 0; i < size; i++)
		{
			ts = (TS)tslist[i];
			if (ts == null)
			{
				continue;
			}

			if (date1 == null)
			{
				// Use the start from the time series...
				ts_start = ts.getDate1();
				if (ts_start == null)
				{
					continue;
				}
			}
			if (date2 == null)
			{
				// Use the end from the time series...
				ts_end = ts.getDate2();
				if (ts_end == null)
				{
					continue;
				}
			}

			// Check the time zone to see if it should be taken from the time series...

			SHEFTimeZone_ts = SHEFTimeZone; // Default determined above
			if (string.ReferenceEquals(timeZone, null))
			{
				// No time zone was specified as an input parameter so try to get it from the time series...
				timeZone_ts = ts.getDate1().getTimeZoneAbbreviation();
				if (timeZone_ts.Length != 0)
				{
					// Time series has a time zone so use it...
					try
					{
						SHEFTimeZone_ts = getSHEFTimeZone(timeZone_ts);
					}
					catch (Exception e)
					{
						// Unable to get a valid time zone...
						message = "Time zone from time series (\"" + timeZone_ts + "\") not recognized (" + e +
							").  Skipping output.";
						@out.println(": " + message);
						Message.printWarning(3, routine, message);
						continue;
					}
				}
			}

			tsi = ts.iterator(ts_start, ts_end);
			if (tsi == null)
			{
				continue;
			}

			// get the identifier

			SHEFID = ts.getLocation();
			if (alternativeIDDefined)
			{
				if (AltID[i].Length > 0)
				{
					SHEFID = AltID[i];
				}
			}

			// get the PE code
			 PhysicalElement = PE[i];
			if (PhysicalElement.Equals(""))
			{
				message = "No PE code specified for \"" + ts.getIdentifierString() + "\"... skipping SHEF .A write.";
				@out.println(": " + message);
				Message.printWarning(3, routine, message);
				continue;
			}

			// get multiplier and base
			tsMult = ts.getDataIntervalMult();
			tsBase = ts.getDataIntervalBase();

			// Get the original TS units and format for output
			try
			{
				tsUnits = DataUnits.lookupUnits(ts.getDataUnits());
				system = tsUnits.getSystem();
				// The output format does not specify a width
				if (precision >= 0)
				{
					// Use the user-specified precision
					unitsFormat = "%." + precision + "f";
				}
				else
				{
					// Determine the precision from units and defaults
					unitsFormat = DataUnits.getOutputFormatString(ts.getDataUnits(), 0, 0);
				}
			}
			catch (Exception e)
			{
				message = "Error getting units for " + ts.getIdentifierString() + ": " + ts.getDataUnits() + " (" + e +
					"). Will skip.";
				@out.println(": " + message);
				Message.printWarning(3, routine, message);
				Message.printWarning(3, routine, e);
				continue;
			}

			// Set the scale if the TS is accumulated or a mean.  If so, SHEF must have a duration
			scale = false;
			if (durationDefined)
			{
				scaleType = DurList[i];

				if (scaleType.Equals("" + TimeScaleType.ACCM, StringComparison.OrdinalIgnoreCase) || scaleType.Equals("" + TimeScaleType.MEAN, StringComparison.OrdinalIgnoreCase))
				{
					scale = true;
					// a 'V' after the PE indicates that the duration of the data is defined elsewhere in the message
					PhysicalElement += "V";
				}
			}

			durationCode = "";
			//Set the duration code if the TS is regular
			if (scale && (tsBase != TimeInterval.IRREGULAR))
			{
				durationCode = "/DV";
				string multString = StringUtil.formatString(tsMult, "%02d");
				if (tsBase == TimeInterval.SECOND)
				{
					durationCode = durationCode + "S" + multString;
				}
				else if (tsBase == TimeInterval.MINUTE)
				{
					durationCode = durationCode + "N" + multString;
				}
				else if (tsBase == TimeInterval.HOUR)
				{
					durationCode = durationCode + "H" + multString;
				}
				else if (tsBase == TimeInterval.DAY)
				{
					durationCode = durationCode + "D" + multString;
				}
				else if (tsBase == TimeInterval.WEEK)
				{
					multString = StringUtil.formatString(tsMult * 7, "%02d");
					durationCode = durationCode + "D" + multString;
				}
				else if (tsBase == TimeInterval.MONTH)
				{
					durationCode = durationCode + "M" + multString;
				}
				else if (tsBase == TimeInterval.YEAR)
				{
					durationCode = durationCode + "Y" + multString;
				}
			}

			// get conversion if units are defined
			mult = 1.0;
			add = 0.0;
			if (newUnitsDefined)
			{
				units = (string)unitsList[i];

				if ((!string.ReferenceEquals(units, null)) && (units.Length != 0) && !units.Equals(ts.getDataUnits(), StringComparison.OrdinalIgnoreCase))
				{
					try
					{
						DataUnits newUnits = DataUnits.lookupUnits(units);
						newSystem = newUnits.getSystem();
						conversion = DataUnits.getConversion(ts.getDataUnits(), units);
						mult = conversion.getMultFactor();
						add = conversion.getAddFactor();
						system = newSystem;
						unitsFormat = DataUnits.getOutputFormatString(units, 0, 0);
					}
					catch (Exception e)
					{
						message = "Unable to convert units to \"" + units + "\" (" + e +
							") leaving units as \"" + ts.getDataUnits() + "\"";
						@out.println(": " + message + " :");
						Message.printWarning(3, routine, message);
					}
				}
			}

			// get the units system
			if (system == DataUnits.SYSTEM_SI)
			{
				SHEFSystem = "/DUS";
			}
			else if ((system == DataUnits.SYSTEM_ENGLISH) || (system == DataUnits.SYSTEM_ALL))
			{
				// Units system does not matter.  Default to English...
				SHEFSystem = "/DUE";
			}
			else
			{
				message = "Cannot find valid units system.  SHEF will default to ENGLISH.";
				Message.printWarning(3, routine, message);

				SHEFSystem = "/DUE";

				@out.println(": " + message + " :");
			}

			int durationIrreg = 0;
			int mod;

			// TODO - why is some of the following code in the loop.  Can't it be determined once outside the loop?

			while (tsi.next() != null)
			{
				value = tsi.getDataValue();
				if (!ts.isDataMissing(value))
				{
					value *= mult + add;
					// TODO - is the constructor necessary?
					t = new DateTime(tsi.getDate());

					if (hourMax == 24)
					{
						if (t.getHour() == 0)
						{
							// Want hour 0 of day to be hour 24 of the previous day
							t.setHour(24);
							t.addDay(-1);
						}
					}

					dateString = t.ToString(DateTime.FORMAT_YYYYMMDDHHmm);
					qualityFlag = "";

					if (tsBase == TimeInterval.IRREGULAR)
					{
						if (scale)
						{
							// The duration is in seconds.
							durationIrreg = ((IrregularTSIterator)tsi).getDuration();

							// TODO - this code is kind of ugly - maybe it can be done cleaner
							if (durationIrreg > 0)
							{
								mod = durationIrreg / 60;
								if (mod > 0)
								{
									// Duration in minutes
									durationIrreg = mod;
									mod = durationIrreg / 60;
									durationCode = "/DVN";
									if (mod > 0)
									{
										//duration in hours
										durationIrreg = mod;
										mod = durationIrreg / 24;
										durationCode = "/DVH";
										if (mod > 0)
										{ //duration in days
											durationIrreg = mod;
											mod = durationIrreg / 30;
											durationCode = "/DVD";
											if (mod > 0)
											{ //duration in month
												durationIrreg = mod;
												mod = durationIrreg / 12;
												durationCode = "/DVM";
												if (mod > 0)
												{ //duration in years
													durationIrreg = mod;
													durationCode = "/DVY";
												}
											}
										}
									}
								}
								else
								{
									durationCode = "/DVS";
								}
							}

							durationCode = durationCode + StringUtil.formatString(duration, "%02d");
						}

						qualityFlag = tsi.getDataFlag();
						if ((!string.ReferenceEquals(qualityFlag, null)) && (qualityFlag.Length > 0))
						{
							qualityFlag = "/DQ" + qualityFlag;
						}
					}

					// Override with the creation date if specified by the user...

					if (string.ReferenceEquals(observationTime, null))
					{
						// Get the observation time from the data...
						observationTimeOutput = " " + observationTimePrefix + dateString.Substring(8);
					}
					else
					{
						// Use what was specified by the calling code
						if (observationTimeLiteral)
						{
							// Use what has been defined
							observationTimeOutput = " " + observationTime;
						}
						else
						{
							// Prefix with the standard 
							observationTimeOutput = " " + observationTimePrefix + observationTime;
						}
					}

					// Override the duration code determined above with the value specified by the user...

					if (!string.ReferenceEquals(duration, null))
					{
						durationCode = "/" + duration;
					}

					SHEFMessage = ".A " + SHEFID + " " + dateString.Substring(0, 8) + " " + SHEFTimeZone_ts + observationTimeOutput + creationDateOutput + SHEFSystem + qualityFlag + durationCode + // blank if not used
						"/" + PhysicalElement + // Always used
						" " + StringUtil.formatString(value, unitsFormat); // Value always specified
					@out.println(SHEFMessage);
				}
			}
		}
	}

	/// <summary>
	/// Write a list of time series to the specified SHEF A file. </summary>
	/// <param name="tslist"> list of time series to write. </param>
	/// <param name="fname"> Name of file to write. </param>
	/// <param name="append"> indicates whether to append records to the file. </param>
	/// <param name="date1"> First date to write (if null write the entire time series). </param>
	/// <param name="date2"> Last date to write (if null write the entire time series). </param>
	/// <param name="units"> list of units to write, one per time series.  If different than
	/// the current units the units will be converted on output.  Specify null to use the time series units. </param>
	/// <param name="list"> of PE Physical element codes, one per time series (see SHEF Handbook).  This information must be supplied. </param>
	/// <param name="props"> See the overloaded method for a description. </param>
	/// <exception cref="IOException"> if there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeTimeSeriesList(java.util.List<TS> tslist, String fname, boolean append, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2, java.util.List<String> units, java.util.List<String> PEList, java.util.List<String> DurList, java.util.List<String> AltIDList, String timeZone, String observationTime, String creationDate, String duration, int hourMax, int precision) throws Exception
	public static void writeTimeSeriesList(IList<TS> tslist, string fname, bool append, DateTime date1, DateTime date2, IList<string> units, IList<string> PEList, IList<string> DurList, IList<string> AltIDList, string timeZone, string observationTime, string creationDate, string duration, int hourMax, int precision)
	{
		string routine = "ShefATS.writeTimeSeriesList";

		string full_fname = IOUtil.getPathUsingWorkingDir(fname);
		try
		{
			PrintWriter fout = new PrintWriter(new FileStream(full_fname, append));

			writeTimeSeriesList(tslist, fout, append, date1, date2, units, PEList, DurList, AltIDList, timeZone, observationTime, creationDate, duration, hourMax, precision);

			fout.flush();
			fout.close();
		}
		catch (Exception e)
		{
			string message = "Error writing \"" + full_fname + "\" for writing.";
			Message.printWarning(3, routine, message);
			Message.printWarning(3, routine, e);
			throw new Exception(message);
		}
	}

	}

}