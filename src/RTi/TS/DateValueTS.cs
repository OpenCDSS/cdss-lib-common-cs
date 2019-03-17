using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// DateValueTS - class to process date-value format time series

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
// DateValueTS - class to process date-value format time series
// ----------------------------------------------------------------------------
// History:
//
// 01 Jan 2000	Steven A. Malers, RTi	Copy and modify DateValueMinuteTS to
//					evaluate whether static I/O methods are
//					a good alternative.  Issues to consider
//					with this approach:
//					* For some time series, it may be
//					  appropriate to declare the time series
//					  in one place and populate data in
//					  another.  In this case, a pointer to
//					  a time series may need to be supplied
//					  to the read method.
//					* What if there are data specific to the
//					  time series format that need to be
//					  carried around between read/write (use
//					  a PropList in TS base class)?
//					* What if a connection to the TS file
//					  is to remain open so that an
//					  incremental read/write can occur
//					  (PropList again or add some specific
//					  data members to TS base class)?
//					Need to consider these before going to
//					this approach for everything, but at
//					the very least, this DateValueTS class
//					may minimize the amount of code
//					duplicated in other DateValue* classes.
// 21 Feb 2000	SAM, RTi		Address a number of feedback issues.
//					Add ability to handle irregular time
//					series when writing.  Figure out why
//					the scenario needs to be specified in
//					the file ID when writing.  Initialize
//					the data type and other information to
//					empty strings when readying.  Add
//					readTimeSeries that takes a TS* pointer.
// 18 Mar 2000	SAM, RTi		Updated based on purify of code.
// 17 Jun 2000	SAM, RTi		Update to use new utility classes.
// 28 Jul 2000	SAM, RTi		Overload readTimeSeries to make
//					units and read_data flag optional.
// 11 Aug 2000	SAM, RTi		Add method to write multiple time
//					series.  Previously, much of this code
//					was in the mergets utility program.
//					Use this method for all writing.  Add
//					javadoc throughout.  Remove
//					freeStringList and use delete to avoid
//					purify PLK messages.
// 20 Aug 2000	SAM, RTi		Add units conversion on output to get
//					rid of some warnings at run-time.
// 03 Oct 2000	SAM, RTi		Port C++
// 23 Oct 2000	SAM, RTi		Update so NumTS property is recognized
//					on a read, but only handle cases where
//					NumTS is 1.
// 07 Nov 2000	SAM, RTi		Add additional information to header,
//					including a header line.
//					Enable reading of multiple time series
//					with readTimeSeriesList().
// 23 Nov 2000	SAM, RTi		Change printSample() to getSample() and
//					return a Vector.
// 22 Feb 2001	SAM, RTi		Add call to setInputName() when reading.
//					Change IO to IOUtil.
// 01 Mar 2001	SAM, RTi		Add call to
//					IOUtil.getPathUsingWorkingDir() to
//					allow batch and GUI code to use the
//					same code.
// 11 Apr 2001	SAM, RTi		Update to support reading multiple
//					time series.
// 25 Apr 2001	SAM, RTi		Use "MissingVal" everywhere and not
//					"Missing".  Fix so that when a single
//					time series is read, the time series
//					identifier that is passed can be either
//					a filename or a time series identifier
//					where the filename is in the scenario.
//					Change so that the readTimeSeriesList()
//					methods that used to take a TS parameter
//					now take a String tsident_string
//					parameter.  Do this because when reading
//					a single time series from
//					Add isDateValueFile() method.  This can
//					be used to determine the file format
//					without relying on the source.
// 01 Jun 2001	SAM, RTi		Remove @ when writing data with times -
//					use a space.  The read code will now
//					properly handle.
// 14 Aug 2001	SAM, RTi		Track down problem where some DateValue
//					time series are not getting read
//					correctly - data_interval_base was not
//					being set for requested time series.
// 28 Aug 2001	SAM, RTi		Track down problem where reading a file
//					and then writing does not include the
//					data type - the Java TS class carries
//					the type independent of the TSIdent so
//					had to set from the TSIdent.  Also, if
//					the data type is missing, do not set
//					when reading.
// 2001-11-06	SAM, RTi		Review javadoc.  Verify that variables
//					are set to null when no longer used.
// 2001-11-20	SAM, RTi		If the start and end dates have a time
//					zone, set the time zone in the write
//					code to not have a time zone.  Otherwise
//					the output will be excessive and
//					redundant and will foul up the parsing
//					code.  The start and end dates with time
//					zones should be enough.  When writing
//					time series, if time is used, make
//					separate headers for the date and time
//					columns.
// 2002-01-14	SAM, RTi		Add readFromStringList().
// 2002-01-31	SAM, RTi		Overload readTimeSeries() to take a
//					requested TSID and a file name.  This
//					is consistent with new TSTool separate
//					handling of the TSID and storage path.
//					Start using the expanded identifier that
//					includes input type and input name.
// 2002-02-08	SAM, RTi		Call the new TSIdent.setInputType() and
//					TSIdent.setInputName() methods.
// 2002-04-16	SAM, RTi		Write the alias and handle reading the
//					alias.  This is needed to transfer
//					TSTool output persistently between runs.
//					Change the DateValue format version to
//					1.2.
// 2002-04-23	SAM, RTi		Fix bug where null time series could
//					not be written - should now result in
//					missing data and blanks in header.
// 2002-04-25	SAM, RTi		Change so when setting the input name
//					the passed in value is used (not the
//					full path).  This works better with
//					dynamic applications where the working
//					directory is set by the application.
//					Change all printWarning() calls to level
//					2.
// 2002-06-12	SAM, RTi		Fix bug where empty DataType header
//					information seems to be causing
//					problems.  In writeTimeSeries(),
//					surround DataType and Units with
//					strings.  Change the format version to
//					1.3.  Fix so that if the data type is
//					not set in the TS, use the TSIdent
//					data type on read and write.
// 2002-07-12	SAM, RTi		Fix problem where descriptions in
//					header were not being read correctly in
//					cases where the description contained
//					equals signs.  Also when reading only
//					the headers, quit reading if
//					"# Time series histories" has been
//					encountered.  Otherwise time series with
//					very long histories take a long time to
//					read for just the header.
// 2002-09-04	SAM, RTi		Update to support input/output of data
//					quality flags.  For now if a time series
//					has data flags, write them.  Backtrack
//					on the UsgsNwis format and only use the
//					data flags to set the data initially
//					but don't carry around (need a strategy
//					for how to handle data flags in
//					manipulation).  Change the version to
//					1.4.
// 2003-02-14	SAM, RTi		Allow NaN to be used as the missing data
//					value.
// 2003-06-02	SAM, RTi		Upgrade to use generic classes.
//					* Change TSDate to DateTime.
//					* Change TSUnits to DataUnits.
//					* Change TS.INTERVAL* to TimeInterval.
// 2003-07-24	SAM, RTi		* Change writeTimeSeries() to
//					  writeTimeSeriesList() when multiple
//					  time series are written.
// 2003-07-31	SAM, RTi		Change so when passing a requested time
//					series identifier to the read method,
//					the aliases in the file are checked
//					first.
// 2003-08-12	SAM, RTi		* Minor cleanup based on review of C++
//					  code (which was recently ported from
//					  this Java code): 1) Initialize some
//					  size variables to 0 in
//					  readTimeSeriesList(); 2) Remove check
//					  for zero year in writeTimeSeriesList()
//					  - average time series may use zero
//					  year.
//					* Update the readTimeSeries() method
//					  that takes a TSID and file name to
//					  handle an alias as the TSID.
//					* When reading the time series, do not
//					  allow file names to be in the
//					  scenario.
//					* Handle sequence number in
//					  readTimeSeriesList().
// 2003-08-19	SAM, RTi		* No longer handle time series
//					  identifiers that have the scenario as
//					  the scenario.  Instead, rely on the
//					  full input name.
//					* Track down a problem apparently
//					  introduced in the last update - time
//					  series files without alias were not
//					  being read.
// 2003-10-06	SAM, RTi		* After reading the header, check for
//					  critical information (e.g., TSID) and
//					  assign defaults.  This will help with
//					  file problems and will also allow
//					  files without the properties to be
//					  read.
//					* Add time series comments to the
//					  header.
// 2003-11-19	SAM, RTi		* Print the time series TSID and alias
//					  in comments and genesis.
//					* Throw an exception in
//					  writeTimeSeriesList() if an error
//					  occurs.
//					* For IrregularTS in
//					  writeTimeSeriesList(), add the units
//					  conversion in again.
// 2004-03-01	SAM, RTi		* Format TSID as quoted strings on
//					  output.
//					* Some command descriptions now have
//					  include = so manually parse out the
//					  header tokens.
// 2005-08-10	SAM, RTi		Fix bug where bad file was passing a
//					null file pointer to the read, resulting
//					in many warnings.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------
//EndHeader

namespace RTi.TS
{

	using DataUnits = RTi.Util.IO.DataUnits;
	using DataUnitsConversion = RTi.Util.IO.DataUnitsConversion;
	using GzipToolkit = RTi.Util.IO.GzipToolkit;
	using IOUtil = RTi.Util.IO.IOUtil;
	using Prop = RTi.Util.IO.Prop;
	using PropList = RTi.Util.IO.PropList;
	using ZipToolkit = RTi.Util.IO.ZipToolkit;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using DateTime = RTi.Util.Time.DateTime;
	using TimeInterval = RTi.Util.Time.TimeInterval;

	/// <summary>
	/// Class to read/write RTi DateValue time series files, using static methods.
	/// The file consists of header information as Property=Value strings and a data
	/// section that is column based.  Time series must have the same interval.
	/// Date/times for each record are parsed so records can be missing.
	/// </summary>
	public class DateValueTS
	{

	/// <summary>
	/// Latest file version.  Use the integer version for internal comparisons.
	/// </summary>
	private static string __VERSION_CURRENT = "1.6";
	private static int __VERSION_CURRENT_INT = 16000;

	/// <summary>
	/// Determine whether a file is a DateValue file.  This can be used rather than
	/// checking the source in a time series identifier.  If the file passes any of the
	/// following conditions, it is assumed to be a DateValue file:
	/// <ol>
	/// <li>	A line starts with "#DateValue".</li>
	/// <li>	A line starts with "# DateValue".</li>
	/// <li>	A line starts with "TSID" and includes "=".</li>
	/// </ol>
	/// IOUtil.getPathUsingWorkingDir() is called to expand the filename.
	/// </summary>
	public static bool isDateValueFile(string filename)
	{
		StreamReader @in = null;
		string full_fname = IOUtil.getPathUsingWorkingDir(filename);
		try
		{
			if (full_fname.ToUpper().EndsWith(".ZIP", StringComparison.Ordinal))
			{
				// Handle case where DateValue file is compressed (single file in .zip)
				ZipToolkit zt = new ZipToolkit();
				@in = zt.openBufferedReaderForSingleFile(full_fname,0);
			}
			else
			{
				@in = new StreamReader(IOUtil.getInputStream(full_fname));
			}
			bool is_datevalue = false;
			try
			{
				// Read lines and check for common strings that indicate a DateValue file.
				string @string = null;
				while (!string.ReferenceEquals((@string = @in.ReadLine()), null))
				{
					if (@string.StartsWith("# DateValue", StringComparison.Ordinal) || @string.StartsWith("#DateValue", StringComparison.Ordinal))
					{
						is_datevalue = true;
						break;
					}
					if (@string.regionMatches(true,0,"TSID",0,4) && (@string.IndexOf("=", StringComparison.Ordinal) >= 0))
					{
						is_datevalue = true;
						break;
					}
				}
			}
			finally
			{
				if (@in != null)
				{
					@in.Close();
				}
			}
			return is_datevalue;
		}
		catch (Exception)
		{
			return false;
		}
	}

	/// <summary>
	/// Parse a data string of the form "{dataflag1:"description",dataflag2:"description"}" </summary>
	/// <param name="value"> the DateValue property value to parse </param>
	private static IList<TSDataFlagMetadata> parseDataFlagDescriptions(string value)
	{
		IList<TSDataFlagMetadata> metaList = new List<TSDataFlagMetadata>();
		value = value.Trim().Replace("{","").Replace("}", "");
		IList<string> parts = StringUtil.breakStringList(value, ",", StringUtil.DELIM_ALLOW_STRINGS | StringUtil.DELIM_ALLOW_STRINGS_RETAIN_QUOTES);
		foreach (string part in parts)
		{
			// Now have flag:description
			IList<string> parts2 = StringUtil.breakStringList(part.Trim(), ":", StringUtil.DELIM_ALLOW_STRINGS | StringUtil.DELIM_ALLOW_STRINGS_RETAIN_QUOTES);
			if (parts2.Count == 2)
			{
				string propName = parts2[0].Trim();
				string propVal = parts2[1].Trim();
				if (propVal.StartsWith("\"", StringComparison.Ordinal) && propVal.EndsWith("\"", StringComparison.Ordinal))
				{
					// Have a quoted string
					metaList.Add(new TSDataFlagMetadata(propName,propVal.Substring(1, (propVal.Length - 1) - 1)));
				}
				else if (propVal.Equals("null", StringComparison.OrdinalIgnoreCase))
				{
					metaList.Add(new TSDataFlagMetadata(propName,""));
				}
			}
		}
		return metaList;
	}

	// TODO SAM 2015-05-18 This is brute force - need to make more elegant
	/// <summary>
	/// Parse a properties string of the form "{stringprop:"propval",intprop:123,doubleprop=123.456}"
	/// </summary>
	private static PropList parseTimeSeriesProperties(string value)
	{
		PropList props = new PropList("");
		value = value.Trim().Replace("{","").Replace("}", "");
		IList<string> parts = StringUtil.breakStringList(value, ",", StringUtil.DELIM_ALLOW_STRINGS | StringUtil.DELIM_ALLOW_STRINGS_RETAIN_QUOTES);
		foreach (string part in parts)
		{
			// Now have Name:value
			IList<string> parts2 = StringUtil.breakStringList(part.Trim(), ":", StringUtil.DELIM_ALLOW_STRINGS | StringUtil.DELIM_ALLOW_STRINGS_RETAIN_QUOTES);
			if (parts2.Count == 2)
			{
				string propName = parts2[0].Trim();
				string propVal = parts2[1].Trim();
				if (propVal.StartsWith("\"", StringComparison.Ordinal) && propVal.EndsWith("\"", StringComparison.Ordinal))
				{
					// Have a quoted string
					props.setUsingObject(propName,propVal.Substring(1, (propVal.Length - 1) - 1));
				}
				else if (propVal.Equals("null", StringComparison.OrdinalIgnoreCase))
				{
					props.setUsingObject(propName,null);
				}
				else if (StringUtil.isInteger(propVal))
				{
					props.setUsingObject(propName, int.Parse(propVal));
				}
				else if (StringUtil.isDouble(propVal))
				{
					props.setUsingObject(propName, double.Parse(propVal));
				}
			}
		}
		return props;
	}

	/// <summary>
	/// Read at time series from a List of String.  Currently this is accomplished by
	/// writing the contents to a temporary file and then reading using one of the
	/// standard methods.  A more efficient method may be added in the future but this
	/// approach works OK for smaller files. </summary>
	/// <param name="strings"> List of String containing data in DateValue file format. </param>
	/// <param name="tsident_string"> Time series identifier as string (used for initial
	/// settings - reset by file contents). </param>
	/// <param name="req_date1"> Requested starting date to initialize period (or null to read the entire period). </param>
	/// <param name="req_date2"> Requested ending date to initialize period (or null to read the entire period). </param>
	/// <param name="req_units"> Units to convert to (currently ignored). </param>
	/// <param name="read_data"> Indicates whether data should be read (false=no, true=yes). </param>
	/// <exception cref="Exception"> if there is an error reading the time series. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static TS readFromStringList(java.util.List<String> strings, String tsident_string, RTi.Util.Time.DateTime req_date1, RTi.Util.Time.DateTime req_date2, String req_units, boolean read_data) throws Exception
	public static TS readFromStringList(IList<string> strings, string tsident_string, DateTime req_date1, DateTime req_date2, string req_units, bool read_data)
	{ // Write the strings to a temporary file...
		string temp = IOUtil.tempFileName();
		PrintWriter pw = new PrintWriter(new FileStream(temp, FileMode.Create, FileAccess.Write));
		int size = 0;
		if (strings != null)
		{
			size = strings.Count;
		}
		for (int i = 0; i < size; i++)
		{
			pw.println(strings[i]);
		}
		pw.close();
		// Create a DateValueTS from the temporary file...
		TS ts = readTimeSeries(temp, req_date1, req_date2, req_units, read_data);
		// Remove the temporary file...
		File tempf = new File(temp);
		tempf.delete();
		// Return...
		return ts;
	}

	/// <summary>
	/// Read a time series from a DateValue format file. </summary>
	/// <returns> time series if successful, or null if not. </returns>
	/// <param name="tsident_string"> One of the following:  1) the time series identifier to
	/// read (where the scenario is the file name) or 2) the name of a file to read
	/// (in which case it is assumed that only one time series exists in the
	/// file - otherwise use the readTimeSeriesList() method). </param>
	/// <exception cref="TSException"> if there is an error reading the time series. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static TS readTimeSeries(String tsident_string) throws Exception
	public static TS readTimeSeries(string tsident_string)
	{
		return readTimeSeries(tsident_string, null, null, null, true);
	}

	/// <summary>
	/// Read a time series from a DateValue format file.  The entire file is read using the units from the file. </summary>
	/// <returns> 0 if successful, 1 if not. </returns>
	/// <param name="in"> Reference to open BufferedReader. </param>
	/// <exception cref="TSException"> if there is an error reading the time series. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static TS readTimeSeries(java.io.BufferedReader in) throws Exception
	public static TS readTimeSeries(StreamReader @in)
	{
		return readTimeSeries(@in, null, null, null, true);
	}

	/// <summary>
	/// Read a time series from a DateValue format file. </summary>
	/// <returns> a time series if successful, null if not. </returns>
	/// <param name="in"> Reference to open BufferedReader. </param>
	/// <param name="req_date1"> Requested starting date to initialize period (or null to read the entire period). </param>
	/// <param name="req_date2"> Requested ending date to initialize period (or null to read the entire period). </param>
	/// <param name="req_units"> Units to convert to (currently ignored). </param>
	/// <param name="read_data"> Indicates whether data should be read (false=no, true=yes). </param>
	/// <exception cref="Exception"> if there is an error reading the time series. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static TS readTimeSeries(java.io.BufferedReader in, RTi.Util.Time.DateTime req_date1, RTi.Util.Time.DateTime req_date2, String req_units, boolean read_data) throws Exception
	public static TS readTimeSeries(StreamReader @in, DateTime req_date1, DateTime req_date2, string req_units, bool read_data)
	{ // Call the generic method...
		return readTimeSeries((TS)null, @in, req_date1, req_date2, req_units, read_data);
	}

	/// <summary>
	/// Read a time series from a DateValue format file. </summary>
	/// <returns> a time series if successful, null if not.  The units are taken from the file and all data are read
	/// (not just the header). </returns>
	/// <param name="tsident_string"> One of the following:  1) the time series identifier to
	/// read (where the scenario is the file name) or 2) the name of a file to read
	/// (in which case it is assumed that only one time series exists in the
	/// file - otherwise use the readTimeSeriesList() method). </param>
	/// <param name="date1"> Starting date to initialize period (or null to read entire time series). </param>
	/// <param name="date2"> Ending date to initialize period (or null to read entire time
	/// series). </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static TS readTimeSeries(String tsident_string, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2) throws Exception
	public static TS readTimeSeries(string tsident_string, DateTime date1, DateTime date2)
	{
		return readTimeSeries(tsident_string, date1, date2, "", true);
	}

	/// <summary>
	/// Read a time series from a DateValue format file.
	/// The IOUtil.getPathUsingWorkingDir() method is applied to the filename. </summary>
	/// <returns> a time series if successful, null if not. </returns>
	/// <param name="tsident_string"> The full identifier for the time series to
	/// read with the file name in the ~DateValue~InputName part of the identifier or
	/// 2) the name of a file to read (in which case it is assumed that only one time
	/// series exists in the file - otherwise use the readTimeSeriesList() method). </param>
	/// <param name="date1"> Starting date to initialize period (null to read the entire time series). </param>
	/// <param name="date2"> Ending date to initialize period (null to read the entire time series). </param>
	/// <param name="units"> Units to convert to. </param>
	/// <param name="read_data"> Indicates whether data should be read (false=no, true=yes). </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static TS readTimeSeries(String tsident_string, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2, String units, boolean read_data) throws Exception
	public static TS readTimeSeries(string tsident_string, DateTime date1, DateTime date2, string units, bool read_data)
	{
		TS ts = null;
		bool is_file = true; // Is tsident_string a file?  Assume and check below

		string input_name = tsident_string;
		string full_fname = IOUtil.getPathUsingWorkingDir(tsident_string);
		Message.printStatus(2, "", "Reading \"" + tsident_string + "\"");
		if (!IOUtil.fileReadable(full_fname) && (tsident_string.IndexOf("~", StringComparison.Ordinal) > 0))
		{
			// The string is a TSID string (implicit read command) with the file name in the
			// input type.
			is_file = false;
			// Try the input name to get the file...
			TSIdent tsident = new TSIdent(tsident_string);
			full_fname = IOUtil.getPathUsingWorkingDir(tsident.getInputName());
			input_name = full_fname;
		}
		// By here the file name is set.
		if (!IOUtil.fileExists(full_fname))
		{
			Message.printWarning(2, "DateValueTS.readTimeSeries", "File does not exist: \"" + full_fname + "\"");
		}
		if (!IOUtil.fileReadable(full_fname))
		{
			Message.printWarning(2, "DateValueTS.readTimeSeries", "File is not readable: \"" + full_fname + "\"");
		}
		StreamReader @in = null;
		if (full_fname.ToUpper().EndsWith(".ZIP", StringComparison.Ordinal))
		{
			// Handle case where DateValue file is compressed (single file in .zip)
			ZipToolkit zt = new ZipToolkit();
			@in = zt.openBufferedReaderForSingleFile(full_fname,0);
		}
		else
		{
			// The following will throw an exception that is appropriate (like no file found).
			@in = new StreamReader(IOUtil.getInputStream(full_fname));
		}
		try
		{
			// Call the fully-loaded method...
			if (is_file)
			{
				// Expect that the time series file has one time series and should read it...
				ts = readTimeSeries((TS)null, @in, date1, date2, units, read_data);
			}
			else
			{
				// Pass the file pointer and an empty time series, which
				// will be used to locate the time series in the file.
				ts = TSUtil.newTimeSeries(tsident_string, true);
				if (ts == null)
				{
					string message = "Unable to create time series for \"" + tsident_string + "\"";
					Message.printWarning(2, "DateValueTS.readTimeSeries(String,...)", message);
					throw new Exception(message);
				}
				ts.setIdentifier(tsident_string);
				ts.getIdentifier().setInputType("DateValue");
				readTimeSeriesList(ts, @in, date1, date2, units, read_data);
			}
			ts.setInputName(full_fname);
			ts.addToGenesis("Read time series from \"" + full_fname + "\"");
			ts.getIdentifier().setInputName(input_name);

		}
		finally
		{
			if (@in != null)
			{
				@in.Close();
			}
		}
		return ts;
	}

	/// <summary>
	/// Read a time series from a DateValue format file.  The TSID string is specified
	/// in addition to the path to the file.  It is expected that a TSID in the file
	/// matches the TSID (and the path to the file, if included in the TSID would not
	/// properly allow the TSID to be specified).  This method can be used with newer
	/// code where the I/O path is separate from the TSID that is used to identify the time series.
	/// The IOUtil.getPathUsingWorkingDir() method is applied to the filename. </summary>
	/// <returns> a time series if successful, null if not. </returns>
	/// <param name="tsident_string"> The full identifier for the time series to
	/// read.  This string can also be the alias for the time series in the file. </param>
	/// <param name="filename"> The name of a file to read
	/// (in which case the tsident_string must match one of the TSID strings in the file). </param>
	/// <param name="date1"> Starting date to initialize period (null to read the entire time series). </param>
	/// <param name="date2"> Ending date to initialize period (null to read the entire time series). </param>
	/// <param name="units"> Units to convert to. </param>
	/// <param name="read_data"> Indicates whether data should be read (false=no, true=yes). </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static TS readTimeSeries(String tsident_string, String filename, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2, String units, boolean read_data) throws Exception
	public static TS readTimeSeries(string tsident_string, string filename, DateTime date1, DateTime date2, string units, bool read_data)
	{
		TS ts = null;

		string input_name = filename;
		string full_fname = IOUtil.getPathUsingWorkingDir(filename);
		if (!IOUtil.fileExists(full_fname))
		{
			Message.printWarning(2, "DateValueTS.readTimeSeries", "File does not exist: \"" + filename + "\"");
		}
		if (!IOUtil.fileReadable(full_fname))
		{
			Message.printWarning(2, "DateValueTS.readTimeSeries", "File is not readable: \"" + filename + "\"");
		}
		StreamReader @in = null;
		if (full_fname.ToUpper().EndsWith(".ZIP", StringComparison.Ordinal))
		{
			// Handle case where DateValue file is compressed (single file in .zip)
			ZipToolkit zt = new ZipToolkit();
			@in = zt.openBufferedReaderForSingleFile(full_fname,0);
		}
		else
		{
			@in = new StreamReader(IOUtil.getInputStream(full_fname));
		}
		// Pass the file pointer and an empty time series, which
		// will be used to locate the time series in the file.
		// The following is somewhat ugly because if we are using an alias we
		// cannot get the time series from newTimeSeries() because it does not
		// have an interval.  In this case, assume daily data.  This requires
		// special treatment in the readTimeSeriesList() method in order to
		// reset the time series to what is actually found in the file.
		// TODO - clean this up, perhaps by moving the time series creation
		// into the readTimeSeriesList() method rather than doing it here.
		try
		{
			if (tsident_string.IndexOf(".", StringComparison.Ordinal) >= 0)
			{
				// Normal time series identifier...
				ts = TSUtil.newTimeSeries(tsident_string, true);
			}
			else
			{
				// Assume an alias...
				ts = new DayTS();
			}
			if (ts == null)
			{
				Message.printWarning(2, "DateValueTS.readTimeSeries(String,...)","Unable to create time series for \"" + tsident_string + "\"");
				return ts;
			}
			if (tsident_string.IndexOf(".", StringComparison.Ordinal) >= 0)
			{
				ts.setIdentifier(tsident_string);
			}
			else
			{
				ts.setAlias(tsident_string);
			}
			IList<TS> v = readTimeSeriesList(ts, @in, date1, date2, units, read_data);
			if (tsident_string.IndexOf(".", StringComparison.Ordinal) < 0)
			{
				// The time series was specified with an alias so it needs
				// to be replaced with what was read.  The alias will have been
				// assigned in the readTimeSeriesList() method.
				ts = v[0];
			}
			ts.getIdentifier().setInputType("DateValue");
			ts.setInputName(full_fname);
			ts.addToGenesis("Read time series from \"" + full_fname + "\"");
			ts.getIdentifier().setInputName(input_name);
		}
		finally
		{
			if (@in != null)
			{
				@in.Close();
			}
		}
		return ts;
	}

	/// <summary>
	/// Read a time series from a DateValue format file.  The data units are taken from
	/// the file and all data are read (not just the header). </summary>
	/// <returns> a time series if successful, null if not. </returns>
	/// <param name="req_ts"> time series to fill.  If null,
	/// return a new time series.  If non-null, all data are reset, except for the
	/// identifier, which is assumed to have been set in the calling code. </param>
	/// <param name="fname"> Name of file to read. </param>
	/// <param name="date1"> Starting date to initialize period to. </param>
	/// <param name="date2"> Ending date to initialize period to. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static TS readTimeSeries(TS req_ts, String fname, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2) throws Exception
	public static TS readTimeSeries(TS req_ts, string fname, DateTime date1, DateTime date2)
	{
		return readTimeSeries(req_ts, fname, date1, date2, "", true);
	}

	/// <summary>
	/// Read a time series from a DateValue format file.
	/// The IOUtil.getPathUsingWorkingDir() method is applied to the filename. </summary>
	/// <returns> a time series if successful, null if not. </returns>
	/// <param name="req_ts"> time series to fill.  If null,
	/// return a new time series.  All data are reset, except for the identifier, which
	/// is assumed to have been set in the calling code. </param>
	/// <param name="fname"> Name of file to read. </param>
	/// <param name="date1"> Starting date to initialize period to. </param>
	/// <param name="date2"> Ending date to initialize period to. </param>
	/// <param name="units"> Units to convert to. </param>
	/// <param name="read_data"> Indicates whether data should be read (false=no, true=yes). </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static TS readTimeSeries(TS req_ts, String fname, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2, String units, boolean read_data) throws Exception
	public static TS readTimeSeries(TS req_ts, string fname, DateTime date1, DateTime date2, string units, bool read_data)
	{
		TS ts = null;

		string input_name = fname;
		string full_fname = IOUtil.getPathUsingWorkingDir(fname);
		if (!IOUtil.fileExists(full_fname))
		{
			Message.printWarning(2, "DateValueTS.readTimeSeries", "File does not exist: \"" + fname + "\"");
		}
		if (!IOUtil.fileReadable(full_fname))
		{
			Message.printWarning(2, "DateValueTS.readTimeSeries", "File is not readable: \"" + fname + "\"");
		}
		StreamReader @in = null;
		if (full_fname.ToUpper().EndsWith(".ZIP", StringComparison.Ordinal))
		{
			// Handle case where DateValue file is compressed (single file in .zip)
			ZipToolkit zt = new ZipToolkit();
			@in = zt.openBufferedReaderForSingleFile(full_fname,0);
		}
		else
		{
			@in = new StreamReader(IOUtil.getInputStream(full_fname));
		}
		try
		{
			ts = readTimeSeries(req_ts, @in, date1, date2, units, read_data);
			ts.setInputName(full_fname);
			ts.addToGenesis("Read time series from \"" + full_fname + "\"");
			ts.getIdentifier().setInputName(input_name);
		}
		finally
		{
			if (@in != null)
			{
				@in.Close();
			}
		}
		return ts;
	}

	/// <summary>
	/// Read a time series from a DateValue format file. </summary>
	/// <returns> a time series if successful, null if not. </returns>
	/// <param name="req_ts"> time series to fill.  If null,return a new time series.
	/// All data are reset, except for the identifier, which is assumed to have been set in the calling code. </param>
	/// <param name="in"> Reference to open input stream. </param>
	/// <param name="req_date1"> Requested starting date to initialize period (or null to read the entire time series). </param>
	/// <param name="req_date2"> Requested ending date to initialize period (or null to read the entire time series). </param>
	/// <param name="req_units"> Units to convert to (currently ignored). </param>
	/// <param name="read_data"> Indicates whether data should be read. </param>
	/// <exception cref="Exception"> if there is an error reading the time series. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static TS readTimeSeries(TS req_ts, java.io.BufferedReader in, RTi.Util.Time.DateTime req_date1, RTi.Util.Time.DateTime req_date2, String req_units, boolean read_data) throws Exception
	public static TS readTimeSeries(TS req_ts, StreamReader @in, DateTime req_date1, DateTime req_date2, string req_units, bool read_data)
	{
		IList<TS> tslist = readTimeSeriesList(req_ts, @in, req_date1, req_date2, req_units, read_data);
		if ((tslist == null) || (tslist.Count != 1))
		{
			return null;
		}
		else
		{
			TS ts = tslist[0];
			return ts;
		}
	}

	/// <summary>
	/// Read all the time series from a DateValue format file.
	/// The IOUtil.getPathUsingWorkingDir() method is applied to the filename. </summary>
	/// <returns> a list of time series if successful, null if not. </returns>
	/// <param name="fname"> Name of file to read. </param>
	/// <param name="date1"> Starting date to initialize period (null to read the entire time series). </param>
	/// <param name="date2"> Ending date to initialize period (null to read the entire time series). </param>
	/// <param name="units"> Units to convert to. </param>
	/// <param name="read_data"> Indicates whether data should be read. </param>
	/// <exception cref="FileNotFoundException"> if the file is not found. </exception>
	/// <exception cref="IOException"> if there is an error reading the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<TS> readTimeSeriesList(String fname, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2, String units, boolean read_data) throws Exception, java.io.IOException, java.io.FileNotFoundException
	public static IList<TS> readTimeSeriesList(string fname, DateTime date1, DateTime date2, string units, bool read_data)
	{
		IList<TS> tslist = null;
		string input_name = fname;
		string full_fname = IOUtil.getPathUsingWorkingDir(fname);
		if (!IOUtil.fileExists(full_fname))
		{
			Message.printWarning(2, "DateValueTS.readTimeSeries", "File does not exist: \"" + fname + "\"");
		}
		if (!IOUtil.fileReadable(full_fname))
		{
			Message.printWarning(2, "DateValueTS.readTimeSeries", "File is not readable: \"" + fname + "\"");
		}
		StreamReader @in = null;
		try
		{
			if (full_fname.ToUpper().EndsWith(".ZIP", StringComparison.Ordinal))
			{
				// Handle case where DateValue file is compressed (single file in .zip)
				ZipToolkit zt = new ZipToolkit();
				@in = zt.openBufferedReaderForSingleFile(full_fname,0);
			}
			else if (full_fname.ToUpper().EndsWith(".GZ", StringComparison.Ordinal))
			{
				// Handle case where DateValue file is compressed (single file in .gz)
				GzipToolkit zt = new GzipToolkit();
				@in = zt.openBufferedReaderForSingleFile(full_fname,0);
			}
			else
			{
				@in = new StreamReader(IOUtil.getInputStream(full_fname));
			}
			tslist = readTimeSeriesList(null, @in, date1, date2, units, read_data);
			TS ts;
			int nts = 0;
			if (tslist != null)
			{
				nts = tslist.Count;
			}
			for (int i = 0; i < nts; i++)
			{
				ts = tslist[i];
				if (ts != null)
				{
					ts.setInputName(full_fname);
					ts.addToGenesis("Read time series from \"" + full_fname + "\"");
					ts.getIdentifier().setInputName(input_name);
				}
			}
		}
		finally
		{
			if (@in != null)
			{
				@in.Close();
			}
		}
		return tslist;
	}

	// TODO SAM 2008-05-09 Evaluate types of exceptions that are thrown
	/// <summary>
	/// Read a time series from a DateValue format file. </summary>
	/// <returns> a List of time series if successful, null if not.  The calling code
	/// is responsible for freeing the memory for the time series. </returns>
	/// <param name="req_ts"> time series to fill.  If null, return all new time series in the list.
	/// All data are reset, except for the identifier, which is assumed to have been set in the calling code. </param>
	/// <param name="in"> Reference to open input stream. </param>
	/// <param name="req_date1"> Requested starting date to initialize period (or null to read the entire time series). </param>
	/// <param name="req_date2"> Requested ending date to initialize period (or null to read the entire time series). </param>
	/// <param name="units"> Units to convert to (currently ignored). </param>
	/// <param name="read_data"> Indicates whether data should be read. </param>
	/// <exception cref="Exception"> if there is an error reading the time series. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static java.util.List<TS> readTimeSeriesList(TS req_ts, java.io.BufferedReader in, RTi.Util.Time.DateTime req_date1, RTi.Util.Time.DateTime req_date2, String req_units, boolean read_data) throws Exception
	private static IList<TS> readTimeSeriesList(TS req_ts, StreamReader @in, DateTime req_date1, DateTime req_date2, string req_units, bool read_data)
	{
		string date_str, message = null, @string = "", value, variable;
		string routine = "DateValueTS.readTimeSeriesList";
		int dl = 10, dl2 = 30, numts = 1;
		DateTime date1 = new DateTime(), date2 = new DateTime();
		// Do not allow consecutive delimiters in header or data values.  For example:
		// 1,,2 will return
		// 2 values for version 1.3 and 3 values for version 1.4 (middle value is missing).
		int delimParseFlag = 0;

		// Always read the header.  Optional is whether the data are read...

		int line_count = 0;
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Processing header...");
		}
		string delimiter_default = " ";
		string alias = "", dataflag = "", datatype = "", delimiter = delimiter_default, description = "", identifier = "", missing = "", seqnum = "", units = "";
		IList<string> alias_v = null;
		IList<string> dataflag_v = null;
		bool[] ts_has_data_flag = null;
		int[] ts_data_flag_length = null;
		IList<string> datatype_v = null;
		IList<string> description_v = null;
		IList<string> identifier_v = null;
		IList<string> missing_v = null;
		IList<PropList> propertiesList = null;
		IList<IList<TSDataFlagMetadata>> dataFlagMetadataList = null;
		IList<string> seqnum_v = null;
		IList<string> units_v = null;
		bool include_count = false;
		bool include_total_time = false;
		int size = 0;
		int equal_pos = 0; // Position of first '=' in line.
		int warning_count = 0;
		try
		{
		while (!string.ReferenceEquals((@string = @in.ReadLine()), null))
		{
			++line_count;
			// Trim the line to better deal with blank lines...
			@string = @string.Trim();
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine,"Processing: \"" + @string + "\"");
			}
			if (!read_data && @string.regionMatches(true,0,"# Time series histories",0,23))
			{
				if (Message.isDebugOn)
				{
					Message.printDebug(10, routine, "Detected end of header.");
				}
				break;
			}
			if ((@string.Equals("")))
			{
				// Skip comments and blank lines for now...
				continue;
			}
			else if (@string[0] == '#')
			{
				string version = "# DateValueTS";
				if (@string.regionMatches(0,version,0,version.Length))
				{
					// Have the file version so use to indicate how file is processed.
					// This property should be used at the top because it impacts how other data are parsed.
					double version_double = StringUtil.atod(StringUtil.getToken(@string," ",StringUtil.DELIM_SKIP_BLANKS, 2));
					if ((version_double > 0.0) && (version_double < 1.4))
					{
						// Older settings...
						delimParseFlag = StringUtil.DELIM_SKIP_BLANKS;
					}
					else
					{
						// Default and new settings.
						delimParseFlag = 0;
					}
				}
				continue;
			}

			if ((equal_pos = @string.IndexOf('=')) == -1)
			{
				// Assume this not a header definition variable and that we are done with the header...
				if (Message.isDebugOn)
				{
					Message.printDebug(10, routine, "Detected end of header.");
				}
				break;
			}

			// Else, process the header string...

			// Don't parse out quoted strings here.  If any tokens use
			// quoted strings, they need to be processed below.  Because
			// some property values now contain the =, parse out manually..

			if (equal_pos == 0)
			{
				if (Message.isDebugOn)
				{
					Message.printDebug(10, routine, "Bad property for \"" + @string + "\".");
					++warning_count;
				}
				continue;
			}

			// Now the first token is the left side and the second token is the right side...

			variable = @string.Substring(0,equal_pos).Trim();
			if (equal_pos == (@string.Length - 1))
			{
				value = "";
			}
			else
			{
				// Trim the value so no whitespace on either end.
				value = @string.Substring(equal_pos + 1).Trim();
			}

			// Deal with the tokens...
			if (variable.Equals("Alias", StringComparison.OrdinalIgnoreCase))
			{
				// Have the alias...
				alias = value;
				alias_v = StringUtil.breakStringList(value, delimiter, delimParseFlag | StringUtil.DELIM_ALLOW_STRINGS);
				size = 0;
				if (alias_v != null)
				{
					size = alias_v.Count;
				}
				if (size != numts)
				{
					Message.printWarning(3, routine, "Number of Alias values using delimiter \"" + delimiter + "\" (" + size + ") is != NumTS (" + numts + ").  Read errors may occur.");
					++warning_count;
					for (int ia = size; ia < numts; ia++)
					{
						alias_v.Add("");
					}
				}
			}
			else if (variable.ToUpper().StartsWith("DATAFLAGDESCRIPTIONS_", StringComparison.Ordinal))
			{
				// Found a properties string of the form DataFlagDescriptions_NN = { ... }
				if (dataFlagMetadataList == null)
				{
					// Create a list of data flag metadata for each time series
					dataFlagMetadataList = new List<IList<TSDataFlagMetadata>>(numts);
					for (int i = 0; i < numts; i++)
					{
						dataFlagMetadataList.Add(new List<TSDataFlagMetadata>());
					}
				}
				// Now parse out the properties for this time series and set in the list
				int pos1 = variable.IndexOf("_", StringComparison.Ordinal);
				if (pos1 > 0)
				{
					int iprop = int.Parse(variable.Substring(pos1 + 1).Trim());
					dataFlagMetadataList[(iprop - 1)] = parseDataFlagDescriptions(value);
				}
			}
			else if (variable.Equals("DataFlags", StringComparison.OrdinalIgnoreCase))
			{
				// Have the data flags indicator which may or may not be surrounded by quotes...
				dataflag = value;
				dataflag_v = StringUtil.breakStringList(dataflag, delimiter,delimParseFlag | StringUtil.DELIM_ALLOW_STRINGS);
				size = 0;
				if (dataflag_v != null)
				{
					size = dataflag_v.Count;
				}
				if (size != numts)
				{
					Message.printWarning(2, routine, "Number of DataFlag values using delimiter \"" + delimiter + "\" (" + size + ") is != NumTS (" + numts + "). Assuming no data flags.  Read errors may occur.");
					++warning_count;
					for (int ia = size; ia < numts; ia++)
					{
						dataflag_v.Add("false");
					}
				}
				// Now further process the data flag indicators.  Need a boolean for each time series to indicate whether
				// data flags are used and need a width for the data flags
				ts_has_data_flag = new bool[numts];
				ts_data_flag_length = new int[numts];
				for (int ia = 0; ia < numts; ia++)
				{
					dataflag = dataflag_v[ia].Trim();
					IList<string> v = StringUtil.breakStringList(dataflag,",",0);
					size = 0;
					if (v != null)
					{
						size = v.Count;
					}
					if (size == 0)
					{
						// Assume no data flag...
						ts_has_data_flag[ia] = false;
						continue;
					}
					// If the first value is "true", assume that the data flag is used...
					if (v[0].Trim().Equals("true", StringComparison.OrdinalIgnoreCase))
					{
						ts_has_data_flag[ia] = true;
					}
					else
					{
						ts_has_data_flag[ia] = false;
					}
					// Now set the length...
					ts_data_flag_length[ia] = 2; // Default
					if (size > 1)
					{
						ts_data_flag_length[ia] = StringUtil.atoi(((string)v[1]).Trim());
					}
				}
			}
			else if (variable.Equals("DataType", StringComparison.OrdinalIgnoreCase))
			{
				// Have the data type...
				datatype = value;
				datatype_v = StringUtil.breakStringList(datatype, delimiter, delimParseFlag | StringUtil.DELIM_ALLOW_STRINGS);
				size = 0;
				if (datatype_v != null)
				{
					size = datatype_v.Count;
				}
				if (size != numts)
				{
					Message.printWarning(2, routine, "Number of DataType values using delimiter \"" + delimiter + "\" (" + size + ") is != NumTS (" + numts + "). Assuming blank.  Read errors may occur.");
					++warning_count;
					for (int ia = size; ia < numts; ia++)
					{
						datatype_v.Add("");
					}
				}
			}
			else if (variable.Equals("Delimiter", StringComparison.OrdinalIgnoreCase))
			{
				// Have the delimiter.  This value is probably quoted so remove quotes.
				string delimiter_previous = delimiter;
				delimiter = StringUtil.remove(value, "\"");
				delimiter = StringUtil.remove(delimiter, "\'");
				if (value.Length == 0)
				{
					delimiter = delimiter_default;
				}
				Message.printStatus(2, routine, "Delimiter is \"" + delimiter + "\" for remaining properties and data columns (previously was \"" + delimiter_previous + "\").");
			}
			else if (variable.Equals("Description", StringComparison.OrdinalIgnoreCase))
			{
				// Have the description.  The description may contain "=" so get the second token manually...
				description = value;
				description_v = StringUtil.breakStringList(description, delimiter, delimParseFlag | StringUtil.DELIM_ALLOW_STRINGS);
				size = 0;
				if (description_v != null)
				{
					size = description_v.Count;
				}
				if (size != numts)
				{
					Message.printWarning(2, routine, "Number of Description values using delimiter \"" + delimiter + "\" (" + size + ") is != NumTS (" + numts + ").  Assuming blank.  Read errors may occur.");
					++warning_count;
					for (int ia = size; ia < numts; ia++)
					{
						description_v.Add("");
					}
				}
			}
			else if (variable.Equals("End", StringComparison.OrdinalIgnoreCase))
			{
				// Have the ending date.  This may be reset below by the requested end date..
				date2 = DateTime.parse(value);
			}
			else if (variable.Equals("IncludeCount", StringComparison.OrdinalIgnoreCase))
			{
				// Will have data column for the count...
				if (value.Equals("true", StringComparison.OrdinalIgnoreCase))
				{
					include_count = true;
				}
				else
				{
					include_count = false;
				}
			}
			else if (variable.Equals("IncludeTotalTime", StringComparison.OrdinalIgnoreCase))
			{
				// Will have data column for the total time...
				if (value.Equals("true", StringComparison.OrdinalIgnoreCase))
				{
					include_total_time = true;
				}
				else
				{
					include_total_time = false;
				}
			}
			else if (variable.Equals("MissingVal", StringComparison.OrdinalIgnoreCase))
			{
				// Have the missing data value...
				missing = value;
				missing_v = StringUtil.breakStringList(missing, delimiter, delimParseFlag | StringUtil.DELIM_ALLOW_STRINGS);
				size = 0;
				if (missing_v != null)
				{
					size = missing_v.Count;
				}
				if (size != numts)
				{
					Message.printWarning(2, routine, "Number of Missing values using delimiter \"" + delimiter + "\" (" + size + ") is != NumTS (" + numts + ").  Assuming -999.  Read errors may occur.");
					++warning_count;
					for (int ia = size; ia < numts; ia++)
					{
						missing_v.Add("");
					}
				}
			}
			else if (variable.Equals("NumTS", StringComparison.OrdinalIgnoreCase))
			{
				// Have the number of time series...
				numts = StringUtil.atoi(value);
			}
			else if (variable.ToUpper().StartsWith("PROPERTIES_", StringComparison.Ordinal))
			{
				// Found a properties string of the form Properties_NN = { ... }
				if (propertiesList == null)
				{
					// Create a PropList for each time series
					propertiesList = new List<PropList>(numts);
					for (int i = 0; i < numts; i++)
					{
						propertiesList.Add(new PropList(""));
					}
				}
				// Now parse out the properties for this time series and set in the list
				int pos1 = variable.IndexOf("_", StringComparison.Ordinal);
				if (pos1 > 0)
				{
					int iprop = int.Parse(variable.Substring(pos1 + 1).Trim());
					propertiesList[(iprop - 1)] = parseTimeSeriesProperties(value);
				}
			}
			else if (variable.Equals("SequenceNum", StringComparison.OrdinalIgnoreCase) || variable.Equals("SequenceID", StringComparison.OrdinalIgnoreCase))
			{
				// Have sequence numbers...
				seqnum = value;
				seqnum_v = StringUtil.breakStringList(seqnum, delimiter, delimParseFlag | StringUtil.DELIM_ALLOW_STRINGS);
				size = 0;
				if (seqnum_v != null)
				{
					size = seqnum_v.Count;
					for (int i = 0; i < size; i++)
					{
						// Replace old -1 missing with blank string
						if (seqnum_v[i].Equals("-1"))
						{
							seqnum_v[i] = "";
						}
					}
				}
				if (size != numts)
				{
					Message.printWarning(2, routine, "Number of SequenceID (or SequenceNum) values using delimiter \"" + delimiter + "\" (" + size + ") is != NumTS (" + numts + ").  Assuming -1.  Read errors may occur.");
					++warning_count;
					for (int ia = size; ia < numts; ia++)
					{
						seqnum_v.Add("");
					}
				}
			}
			else if (variable.Equals("Start", StringComparison.OrdinalIgnoreCase))
			{
				// Have the starting date.  This may be reset below by the requested start date....
				date1 = DateTime.parse(value);
			}
			else if (variable.Equals("TSID", StringComparison.OrdinalIgnoreCase))
			{
				// Have the TSIdent...
				identifier = value;
				identifier_v = StringUtil.breakStringList(identifier, delimiter, delimParseFlag | StringUtil.DELIM_ALLOW_STRINGS);
				size = 0;
				if (identifier_v != null)
				{
					 size = identifier_v.Count;
				}
				if (size != numts)
				{
					Message.printWarning(2, routine, "Number of TSID values using delimiter \"" + delimiter + "\" (" + size + ") is != NumTS (" + numts + "). Assuming blank.  Read errors may occur.");
					++warning_count;
					for (int ia = size; ia < numts; ia++)
					{
						identifier_v.Add("");
					}
				}
			}
			else if (variable.Equals("Units", StringComparison.OrdinalIgnoreCase))
			{
				// Have the data units...
				units = value;
				units_v = StringUtil.breakStringList(units, delimiter, delimParseFlag | StringUtil.DELIM_ALLOW_STRINGS);
				if (units_v != null)
				{
					size = units_v.Count;
				}
				if (size != numts)
				{
					Message.printWarning(2, routine, "Number of Units values using delimiter \"" + delimiter + "\" (" + size + ") is != NumTS (" + numts + "). Assuming blank. Read errors may occur.");
					++warning_count;
					for (int ia = size; ia < numts; ia++)
					{
						units_v.Add("");
					}
				}
			}
			else
			{
				Message.printWarning(3, routine, "Property \"" + variable + "\" is not currently recognized.");
			}
		}
		}
		catch (Exception e)
		{
			message = "Unexpected error processing line " + line_count + ": \"" + @string + "\"";
			Message.printWarning(3, routine, message);
			Message.printWarning(3, routine, e);
			throw new Exception(message);
		}
		if (warning_count > 0)
		{
			// Print a warning and throw an exception about the header not being properly
			message = "" + warning_count + " errors existing in file header.  Not reading data.";
			Message.printWarning(3, routine, message);
			// FIXME SAM 2008-04-14 Throw a more specific exception
			throw new Exception(message);
		}
		// Reset for below.
		warning_count = 0;

		// Make sure the data flag boolean array is allocated.  This simplifies the logic below...

		if (ts_has_data_flag == null)
		{
			ts_has_data_flag = new bool[numts];
			for (int i = 0; i < numts; i++)
			{
				ts_has_data_flag[i] = false;
			}
		}

		// Check required data lists and assign defaults if necessary...

		if (identifier_v == null)
		{
			identifier_v = new List<string>(numts);
			// TODO SAM 2008-04-14 Evaluate tightening this constraint - throw exception?
			Message.printWarning(2, routine, "TSID property in file is missing.  Assigning default TS1, TS2, ...");
			for (int i = 0; i < numts; i++)
			{
				identifier_v.Add("TS" + (i + 1));
			}
		}

		// Declare the time series of the proper type based on the interval.
		// Use a TSIdent to parse out the interval information...

		TSIdent ident = null;
		int data_interval_base = 0;

		int req_ts_i = -1; // Which time series corresponds to the requested time series.
		int req_ts_column = -1; // Which column of data corresponds to the requested time series.
		int req_ts_column2 = -1; // Which column of data corresponds to the
					// requested time series, after adjustment for possible additional time column in date.
		TS ts = null;
		IList<TS> tslist = null;
		TS[] ts_array = null; // Use this to speed processing so we don't have to search through tslist all the time
		// Set the time series to either the requested time series
		// or a newly-created time series.  If a requested time series is
		// given but only its alias is available, create a new time series
		// using the matching TSID, which will contain the interval, etc.
		if (req_ts != null)
		{
			req_ts_i = -1; // Index of found time series...
			// If there is only one time series in the file, assume it should be used, regardless...
			if (numts == 1)
			{
				//Message.printStatus ( 1, "", "Using only TS because only one TS in file." );
				req_ts_i = 0;
			}
			if (req_ts_i < 0)
			{
				// Need to keep searching.  Loop through all the time series identifiers and compare exactly.
				// That way if only the scenarios are different we will find the correct time series.
				for (int i = 0; i < numts; i++)
				{
					// Check the alias for a match.  This takes precedence over the identifier.
					if (alias_v != null)
					{
						alias = alias_v[i].Trim();
						if (!alias.Equals("") && req_ts.getAlias().Equals(alias, StringComparison.OrdinalIgnoreCase))
						{
							// Found a matching time series...
							req_ts_i = i;
							//Message.printStatus ( 1, "", "Found matching TS "+req_ts_i+ " based on alias." );
							break;
						}
					}
					// Now check the identifier...
					identifier = identifier_v[i].Trim();
					if (req_ts.getIdentifierString().Equals(identifier, StringComparison.OrdinalIgnoreCase))
					{
						// Found a matching time series...
						req_ts_i = i;
						//Message.printStatus ( 1, "", "SAMX Found matching TS " + req_ts_i + " based on full TSID." );
						break;
					}
				}
			}
			if (req_ts_i < 0)
			{
				// Did not find the requested time series...
				message = "Did not find the requested time series \"" + req_ts.getIdentifierString() + "\" Alias \"" + req_ts.getAlias() + "\"";
				Message.printWarning(2, routine, message);
				throw new Exception(message);
			}
			// If here a requested time series was found.  However, if the requested TSID used the
			// alias only, need to create a time series of the correct type using the header information...
			if (req_ts.getLocation().Equals("") && !req_ts.getAlias().Equals(""))
			{
				// The requested time series is only identified by the alias and needs to be recreated for the full
				// identifier.  This case is configured in the calling public readTimeSeries() method.
				identifier = identifier_v[req_ts_i].Trim();
				//Message.printStatus ( 1, routine,"SAMX creating new req_ts for \"" +
				//identifier + "\" alias \"" + req_ts.getAlias() +"\"");
				ts = TSUtil.newTimeSeries(identifier, true);
				ts.setIdentifier(identifier);
				// Reset the requested time series to the new one because req_ts is checked below...
				ts.setAlias(req_ts.getAlias());
				req_ts = ts;
			}
			else
			{
				// A full TSID was passed in for the requested time series and there is no need to reassign the requested
				// time series...
				//Message.printStatus ( 1, routine, "SAMX using existing ts for \"" +
				//identifier + "\" alias \"" + req_ts.getAlias() +"\"");
				ts = req_ts;
				// Identifier is assumed to have been set previously.
			}
			// Remaining logic is the same...
			tslist = new List<TS>(1);
			tslist.Add(ts);
			ts_array = new TS[1];
			ts_array[0] = ts;
			if (Message.isDebugOn)
			{
				Message.printDebug(1, routine, "Adding requested time series to list.");
			}
			ident = new TSIdent(ts.getIdentifier());
			data_interval_base = ident.getIntervalBase();
			// Offset other information because of extra columns...
			// Make sure to set the interval for use below...
			identifier = identifier_v[req_ts_i].Trim();
			ident = new TSIdent(identifier);
			// Set the data type in the TS header using the information in the identifier.
			// It may be overwritten below if the DataType property is specified...
			ts.setDataType(ident.getType());
			// Reset the column to account for the date...
			req_ts_column = req_ts_i + 1; // 1 is date.
			if (include_count)
			{
				++req_ts_column;
			}
			if (include_total_time)
			{
				++req_ts_column;
			}
			if (dataflag_v != null)
			{
				// At least one of the time series in the file uses data flags so adjust the column for
				// time series that may be before the requested time series...
				for (int ia = 0; ia < req_ts_i; ia++)
				{
					if (ts_has_data_flag[ia])
					{
						++req_ts_column;
					}
				}
			}
			if (Message.isDebugOn)
			{
				Message.printDebug(1, routine, "Time series \"" + req_ts.getIdentifierString() + "\" will be read from data column " + req_ts_column + " (date column = 0)");
			}
		}
		else
		{
			// Allocate here as many time series as indicated by numts...
			tslist = new List<TS>(numts);
			ts_array = new TS[numts];
			if (Message.isDebugOn)
			{
				Message.printDebug(1, routine, "Allocated space for " + numts + " time series in list.");
			}
			for (int i = 0; i < numts; i++)
			{
				identifier = identifier_v[i].Trim();
				ident = new TSIdent(identifier);
				// Need this to check whether time may be specified on data line...
				data_interval_base = ident.getIntervalBase();
				ts = TSUtil.newTimeSeries(identifier, true);
				if (ts == null)
				{
					Message.printWarning(2, routine, "Unable to create new time series for \"" + identifier + "\"");
					return null;
				}
				// Only set the identifier if a new time series.
				// Otherwise assume the the existing identifier is to be used (e.g., from a file name).
				ts.setIdentifier(identifier);
				ts.getIdentifier().setInputType("DateValue");
				tslist.Add(ts);
				ts_array[i] = ts;
				if (Message.isDebugOn)
				{
					Message.printDebug(1, routine, "Created memory for \"" + ts.getIdentifierString() + "\"");
				}
			}
		}

		// Set the parameters from the input variables and override with the
		// parameters in the file if necessary...

		if (req_date1 != null)
		{
			date1 = req_date1;
		}
		if (req_date2 != null)
		{
			date2 = req_date2;
		}
		if ((date1 != null) && (date2 != null) && date1.greaterThan(date2))
		{
			Message.printWarning(2, routine, "Date2 (" + date2 + ") is > Date1 (" + date1 + ").  Errors are likely.");
			++warning_count;
		}
		try
		{
			for (int i = 0; i < numts; i++)
			{
				if (req_ts != null)
				{
					if (req_ts_i != i)
					{
						// A time series was requested but does not match so continue...
						continue;
					}
					else
					{ // Found the matching requested time series...
						ts = ts_array[0];
					}
				}
				else
				{ // Reading a list...
					ts = ts_array[i];
				}
				if (Message.isDebugOn)
				{
					Message.printDebug(1, routine, "Setting properties for \"" + ts.getIdentifierString() + "\"");
				}
				if (alias_v != null)
				{
					alias = alias_v[i].Trim();
					if (!alias.Equals(""))
					{
						ts.setAlias(alias);
					}
				}
				if (datatype_v != null)
				{
					datatype = datatype_v[i].Trim();
					if (!datatype.Equals(""))
					{
						ts.setDataType(datatype);
					}
				}
				if (units_v != null)
				{
					units = units_v[i].Trim();
					ts.setDataUnits(units);
					ts.setDataUnitsOriginal(units);
				}
				ts.setDate1(date1);
				ts.setDate1Original(date1);
				ts.setDate2(date2);
				ts.setDate2Original(date2);
				if (description_v != null)
				{
					description = description_v[i].Trim();
					ts.setDescription(description);
				}
				if (missing_v != null)
				{
					missing = missing_v[i].Trim();
					if (missing.Equals("NaN", StringComparison.OrdinalIgnoreCase))
					{
						ts.setMissing(Double.NaN);
					}
					else if (StringUtil.isDouble(missing))
					{
						ts.setMissing(StringUtil.atod(missing));
					}
				}
				if (seqnum_v != null)
				{
					seqnum = seqnum_v[i].Trim();
					ts.setSequenceID(seqnum);
				}
				if (ts_has_data_flag[i])
				{
					// Data flags are being used.
					ts.hasDataFlags(true, true);
				}
				if (propertiesList != null)
				{
					// Transfer the properties
					PropList props = propertiesList[i];
					foreach (Prop prop in props.getList())
					{
						ts.setProperty(prop.getKey(), prop.getContents());
					}
				}
				if (dataFlagMetadataList != null)
				{
					// Transfer the data flag descriptions
					IList<TSDataFlagMetadata> metaList = dataFlagMetadataList[i];
					foreach (TSDataFlagMetadata meta in metaList)
					{
						ts.addDataFlagMetadata(meta);
					}
				}
			}
		}
		catch (Exception e)
		{
			message = "Unexpected error initializing time series.";
			Message.printWarning(3, routine, message);
			Message.printWarning(3, routine, e);
			++warning_count;
		}
		if (warning_count > 0)
		{
			message = "" + warning_count + " errors occurred initializing time series.  Not reading data.";
			Message.printWarning(3, routine, message);
			// FIXME SAM 2008-04-14 Evaluate throwing more specific exception.
			throw new Exception(message);
		}
		if (Message.isDebugOn)
		{
			Message.printDebug(10, routine, "Read TS header");
		}
		warning_count = 0; // Reset for reading data section below.

		// Check the header information.  If the data type has not been
		// specified but is included in the time series identifier, set in the data type...

		size = 0;
		if (tslist != null)
		{
			size = tslist.Count;
		}
		for (int i = 0; i < size; i++)
		{
			if (ts.getDataType().Trim().Equals(""))
			{
				ts.setDataType(ts.getIdentifier().getType());
			}
		}

		if (!read_data)
		{
			return tslist;
		}

		// Allocate the memory for the data array.  This needs to be done
		// whether a requested time series or list is being done...

		if (req_ts != null)
		{
			ts = ts_array[0];
			if (ts.allocateDataSpace() != 0)
			{
				message = "Error allocating data space for time series.";
				Message.printWarning(3, routine, message);
				throw new Exception(message);
			}
		}
		else
		{
			for (int i = 0; i < numts; i++)
			{
				ts = ts_array[i];
				if (ts.allocateDataSpace() != 0)
				{
					message = "Error allocating data space for time series.";
					Message.printWarning(3, routine, message);
					throw new Exception(message);
				}
			}
		}

		// Now read the data.  Need to monitor if this is a real hog and optimize if so...
		warning_count = 0;

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Reading data...");
		}

		DateTime date;
		IList<string> strings;
		double dvalue; // Double data value
		string svalue; // String data value
		bool first = true;
		int nstrings = 0;
		bool use_time = false;
		if ((data_interval_base == TimeInterval.HOUR) || (data_interval_base == TimeInterval.MINUTE) || (data_interval_base == TimeInterval.IRREGULAR))
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "Expect time to be given with dates - may be separate column.");
			}
			use_time = true;
		}
		// Compute the number of expected columns...
		int num_expected_columns = numts + 1; // Number of expected columns
							// given the number of time
							// series, extra columns at the
							// front, data flag columns, and
							// a column for times
							// Column 1 is the date
		int num_extra_columns = 0; // Number of extra columns at
							// the front of the data
							// (record count and total
							// time).
		if (include_count)
		{ // Record count
			++num_expected_columns;
			++num_extra_columns;
		}
		if (include_total_time)
		{ // Total time...
			++num_expected_columns;
			++num_extra_columns;
		}
		// Adjust the number of expected columns if data flags are included...
		int its = 0, i = 0;
		for (its = 0; its < numts; its++)
		{
			if (ts_has_data_flag[its])
			{
				++num_expected_columns;
			}
		}
		int first_data_column = 0;
		int num_expected_columns_p1 = num_expected_columns + 1;
		// Read lines until the end of the file...
		while (true)
		{
			try
			{
			if (first)
			{
				// Have read in the line above so process it in the
				// following code.  The line will either start with
				// "Date" or a date (e.g., MM/DD/YYYY), or will be
				// invalid.  Note that for some programs, the date and
				// all other columns actually have a suffix.  This may
				// be phased out at some time but is the reason why the
				// first characters are checked...
				first = false;
				if (@string.regionMatches(true,0,"date",0,4))
				{
					// Can ignore because it is the header line for columns...
					continue;
				}
			}
			else
			{
				// Need to read a line...
				@string = @in.ReadLine();
				++line_count;
				if (string.ReferenceEquals(@string, null))
				{
					if (Message.isDebugOn)
					{
						Message.printDebug(dl, routine, "Detected end of file.");
					}
					break;
				}
			}
			// Remove whitespace at front and back...
			@string = @string.Trim();
			if (Message.isDebugOn)
			{
				Message.printDebug(dl2, routine, "Processing: \"" + @string + "\"");
			}
			if ((@string.Length == 0) || ((@string.Length > 0) && (@string[0] == '#')))
			{
				// Skip comments and blank lines for now...
				continue;
			}
			if (!char.IsDigit(@string[0]))
			{
				// Not a data line...
				Message.printWarning(2, routine, "Error in data format for line " + line_count + ". Expecting number at start: \"" + @string + "\"");
				++warning_count;
				continue;
			}
			// Now parse the string...
			// If hour, or minute data, expect data line to be YYYY-MM-DD HH:MM Value
			// If there is a space between date and time, assume that the first two need to be concatenated.
			@string = @string.Trim();
			if (dataflag_v == null)
			{
				// No data flags so parse without handling quoted strings.  This will in general be faster...
				strings = StringUtil.breakStringList(@string, delimiter, delimParseFlag);
			}
			else
			{
				// Expect to have data flags so parse WITH handling quoted strings.  This will generally be slower...
				strings = StringUtil.breakStringList(@string, delimiter, delimParseFlag | StringUtil.DELIM_ALLOW_STRINGS);
			}
			nstrings = 0;
			if (strings != null)
			{
				nstrings = strings.Count;
			}
			if (nstrings == num_expected_columns)
			{
				// Assume that there is NO space between date and time or that time field is not used...
				date_str = ((string)strings[0]).Trim();
				// Date + extra columns...
				first_data_column = 1 + num_extra_columns;
				req_ts_column2 = req_ts_column;
			}
			else if (use_time && (nstrings == num_expected_columns_p1))
			{
				// Assume that there IS a space between the date and
				// time.  Concatenate together so that the DateTime.parse will work.
				date_str = ((string)strings[0]).Trim() + " " + ((string)strings[1]).Trim();
				// Date + time + extra column...
				first_data_column = 2 + num_extra_columns;
				// Adjusted requested time series column...
				req_ts_column2 = req_ts_column + 1;
			}
			else
			{
				Message.printWarning(2, routine, "Error in data format for line " + line_count + ". Have " + nstrings + " fields using delimiter \"" + delimiter + "\" but expecting " + num_expected_columns + ": \"" + @string);
				++warning_count;
				//Message.printStatus ( 1, routine, "use_time=" + use_time + " num_expected_columns_p1=" +
				//num_expected_columns_p1 );
				// Ignore the line...
				strings = null;
				continue;
			}
			// Allow all common date formats, even if not the right precision...
			date = DateTime.parse(date_str);
			// The input line date may not have the proper resolution, so
			// set to the precision of the time series defined in the header.
			if (data_interval_base == TimeInterval.MINUTE)
			{
				date.setPrecision(DateTime.PRECISION_MINUTE);
			}
			else if (data_interval_base == TimeInterval.HOUR)
			{
				date.setPrecision(DateTime.PRECISION_HOUR);
			}
			else if (data_interval_base == TimeInterval.DAY)
			{
				date.setPrecision(DateTime.PRECISION_DAY);
			}
			else if (data_interval_base == TimeInterval.MONTH)
			{
				date.setPrecision(DateTime.PRECISION_MONTH);
			}
			else if (data_interval_base == TimeInterval.YEAR)
			{
				date.setPrecision(DateTime.PRECISION_YEAR);
			}
			if (date.lessThan(date1))
			{
				// No data of interest yet...
				strings = null;
				if (Message.isDebugOn)
				{
					Message.printDebug(1, routine, "Ignoring data - before start date");
				}
				continue;
			}
			else if (date.greaterThan(date2))
			{
				// No need to keep reading...
				strings = null;
				if (Message.isDebugOn)
				{
					Message.printDebug(1, routine, "Stop reading data - after start date");
				}
				break;
			}

			// Else, save the data for each column...

			if (req_ts != null)
			{
				// Just have to process one column...
				svalue = ((string)strings[req_ts_column2]).Trim();
				// This introduces a performance hit - maybe need to add a boolean array for each time series
				// to be able to check whether NaN is the missing - then can avoid the check.
				// For now just check the string.
				if (svalue.Equals("NaN") || (string.ReferenceEquals(svalue, null)) || (svalue.Length == 0))
				{
					// Treat the data value as missing.
					dvalue = ts_array[0].getMissing();
				}
				else
				{
					// A numerical missing value like -999 will just get assigned.
					dvalue = StringUtil.atod(svalue);
				}
				if (ts_has_data_flag[req_ts_i])
				{
					// Has a data flag...
					dataflag = ((string)strings[req_ts_column2 + 1]).Trim();
					ts_array[0].setDataValue(date, dvalue, dataflag, 1);
					if (Message.isDebugOn)
					{
						Message.printDebug(dl2, routine, "For date " + date.ToString() + ", value=" + dvalue + ", flag=\"" + dataflag + "\"");
					}
				}
				else
				{ // No data flag...
					ts_array[0].setDataValue(date, dvalue);
					if (Message.isDebugOn)
					{
						Message.printDebug(dl2, routine, "For date " + date.ToString() + ", value=" + dvalue);
					}
				}
			}
			else
			{
				// Loop through all the columns...
				for (i = first_data_column, its = 0; i < nstrings; i++, its++)
				{
					// Set the data value in the requested time series.  If a requested time series is
					// being used, the array will only contain one time series, which is the requested time
					// series (SAMX 2002-09-05 so why the code above???)...
					//
					// This introduces a performance hit - maybe need to add a boolean array for each time
					// series to be able to check whether NaN is the missing - then can avoid the check.  For
					// now just check the string.
					svalue = ((string)strings[i]).Trim();
					if (svalue.Equals("NaN"))
					{
						dvalue = ts_array[its].getMissing();
					}
					else
					{
						dvalue = StringUtil.atod(svalue);
					}
					if (ts_has_data_flag[its])
					{
						dataflag = ((string) strings[++i]).Trim();
						ts_array[its].setDataValue(date, dvalue, dataflag, 1);
						if (Message.isDebugOn)
						{
							Message.printDebug(dl2, routine, "For date " + date.ToString() + ", value=" + dvalue + ", flag=\"" + dataflag + "\"");
						}
					}
					else
					{
						// No data flag...
						ts_array[its].setDataValue(date, dvalue);
						if (Message.isDebugOn)
						{
							Message.printDebug(dl2, routine, "For date " + date.ToString() + ", value=" + dvalue);
						}
					}
				}
			}

			// Clean up memory...

			strings = null;
			}
			catch (Exception e)
			{
				Message.printWarning(2, routine, "Unexpected error processing line " + line_count + ": \"" + @string + "\"");
				Message.printWarning(3, routine, e);
				++warning_count;
			}
		}

		if (warning_count > 0)
		{
			message = "" + warning_count + " errors were detected reading data in file.";
			Message.printWarning(2, routine, message);
			// FIXME SAM 2008-04-14 Evaluate throwing a more specific exception
			throw new Exception(message);
		}

		//if ( Message::isDebugOn ) {
		//	long address = (long)ts;
		//	Message::printDebug ( 1, routine,
		//	ts->getIdentifierString() + " Read data for " +
		//	ts->getDate1().toString() + " Address " +
		//	String::valueOf(address) + " to " + ts->getDate2().toString());
		//}
		if (req_ts != null)
		{
			req_ts.addToGenesis("Read DateValue time series from " + ts.getDate1() + " to " + ts.getDate2());
		}
		else
		{
			for (i = 0; i < numts; i++)
			{
				ts_array[i].addToGenesis("Read DateValue time series from " + ts.getDate1() + " to " + ts.getDate2());
			}
		}
		return tslist;
	}

	/// <summary>
	/// Write a time series to a DateValue format file. </summary>
	/// <param name="ts"> single time series to write. </param>
	/// <param name="out"> PrintWriter to write to. </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeTimeSeries(TS ts, java.io.PrintWriter out) throws Exception
	public static void writeTimeSeries(TS ts, PrintWriter @out)
	{ // Call the fully-loaded method...
		IList<TS> v = new List<TS>(1);
		v.Add(ts);
		writeTimeSeriesList(v, @out, (DateTime)null, (DateTime)null, null, true);
	}

	/// <summary>
	/// Write a list of time series to a DateValue format file. </summary>
	/// <param name="out"> PrintWrite to write to. </param>
	/// <param name="date1"> First date to write (if null write the entire time series). </param>
	/// <param name="date2"> Last date to write (if null write the entire time series). </param>
	/// <param name="units"> Units to write.  If different than the current units the units will be converted on output. </param>
	/// <param name="writeData"> Indicates whether data should be written (as opposed to only writing the header). </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeTimeSeries(TS ts, java.io.PrintWriter out, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2, String units, boolean writeData) throws Exception
	public static void writeTimeSeries(TS ts, PrintWriter @out, DateTime date1, DateTime date2, string units, bool writeData)
	{
		IList<TS> v = new List<TS>(1);
		v.Add(ts);
		writeTimeSeriesList(v, @out, date1, date2, units, writeData);
	}

	/// <summary>
	/// Write a time series to the specified file.
	/// The IOUtil.getPathUsingWorkingDir() method is applied to the filename. </summary>
	/// <param name="ts"> Time series to write. </param>
	/// <param name="fname"> Name of file to write. </param>
	/// <param name="date1"> First date to write (if NULL write the entire time series). </param>
	/// <param name="date2"> Last date to write (if NULL write the entire time series). </param>
	/// <param name="units"> Units to write.  If different than the current units the units will be converted on output. </param>
	/// <param name="writeData"> Indicates whether data should be written (as opposed to only writing the header). </param>
	/// <exception cref="Exception"> if there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeTimeSeries(TS ts, String fname, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2, String units, boolean writeData) throws Exception
	public static void writeTimeSeries(TS ts, string fname, DateTime date1, DateTime date2, string units, bool writeData)
	{
		string routine = "DateValueTS.writeTimeSeries";

		string full_fname = IOUtil.getPathUsingWorkingDir(fname);
		try
		{
			FileStream fos = new FileStream(full_fname, FileMode.Create, FileAccess.Write);
			PrintWriter fout = new PrintWriter(fos);

			try
			{
				writeTimeSeries(ts, fout, date1, date2, units, writeData);
			}
			finally
			{
				fout.close();
			}
		}
		catch (Exception)
		{
			string message = "Error opening \"" + full_fname + "\" for writing.";
			Message.printWarning(2, routine, message);
			throw new Exception(message);
		}
	}

	/// <summary>
	/// Write a single time series to the specified file.  The entire period is written. </summary>
	/// <param name="ts"> Time series to write. </param>
	/// <param name="fname"> Name of file to write. </param>
	/// <exception cref="Exception"> if there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeTimeSeries(TS ts, String fname) throws Exception
	public static void writeTimeSeries(TS ts, string fname)
	{
		writeTimeSeries(ts, fname, null, null, null, true);
	}

	/// <summary>
	/// Write the data flag descriptions for a time series. </summary>
	/// <param name="out"> PrintWriter to use for writing </param>
	/// <param name="ts"> time series for which to write data flag descriptions </param>
	/// <param name="its"> counter for time series (0+) to number the descriptions </param>
	private static void writeTimeSeriesDataFlagDescriptions(PrintWriter @out, TS ts, int its)
	{
		IList<TSDataFlagMetadata> metaList = ts.getDataFlagMetadataList();
		if (metaList.Count > 0)
		{
			StringBuilder b = new StringBuilder("DataFlagDescriptions_" + (its + 1) + " = {");
			TSDataFlagMetadata meta;
			for (int iMeta = 0; iMeta < metaList.Count; iMeta++)
			{
				meta = metaList[iMeta];
				if (iMeta > 0)
				{
					b.Append(",");
				}
				// TODO SAM 2015-05-23 What to do if the data flag contains whitespace or characters that cause problems?  Quote?
				// What to do if the description contains double quotes?  Unlikely but possible.  For now remove.
				string desc = meta.getDescription().Replace("\"","");
				b.Append(meta.getDataFlag() + ":\"" + desc + "\"");
			}
			b.Append("}");
			@out.println(b.ToString());
		}
	}

	/// <summary>
	/// Write a list of time series to a DateValue format file.
	/// Currently there is no way to indicate that the count or total time should be printed. </summary>
	/// <param name="tslist"> list of time series to write. </param>
	/// <param name="out"> PrintWrite to write to. </param>
	/// <param name="date1"> First date to write (if NULL write the entire time series). </param>
	/// <param name="date2"> Last date to write (if NULL write the entire time series). </param>
	/// <param name="units"> Units to write.  If different than the current units the units will be converted on output. </param>
	/// <param name="writeData"> Indicates whether data should be written (as opposed to only writing the header). </param>
	/// <exception cref="Exception"> if there is an error writing the file (I/O error or invalid data). </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeTimeSeriesList(java.util.List<TS> tslist, java.io.PrintWriter out, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2, String units, boolean writeData) throws Exception
	public static void writeTimeSeriesList(IList<TS> tslist, PrintWriter @out, DateTime date1, DateTime date2, string units, bool writeData)
	{
		writeTimeSeriesList(tslist, @out, date1, date2, units, writeData, null);
	}

	/// <summary>
	/// Write a list of time series to a DateValue format file.
	/// Currently there is no way to indicate that the count or total time should be printed. </summary>
	/// <param name="tslist"> list of time series to write. </param>
	/// <param name="out"> PrintWrite to write to. </param>
	/// <param name="date1"> First date to write (if null write the entire time series). </param>
	/// <param name="date2"> Last date to write (if null write the entire time series). </param>
	/// <param name="units"> Units to write.  If different than the current units the units will be converted on output. </param>
	/// <param name="writeData"> Indicates whether data should be written (as opposed to only writing the header). </param>
	/// <param name="props"> Properties to control output (see overloaded method for description). </param>
	/// <exception cref="Exception"> if there is an error writing the file (I/O error or invalid data). </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeTimeSeriesList(java.util.List<TS> tslist, java.io.PrintWriter out, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2, String units, boolean writeData, RTi.Util.IO.PropList props) throws Exception
	public static void writeTimeSeriesList(IList<TS> tslist, PrintWriter @out, DateTime date1, DateTime date2, string units, bool writeData, PropList props)
	{
		string message, routine = "DateValueTS.writeTimeSeriesList";
		DateTime outputStart = null, outputEnd = null, t = new DateTime(DateTime.DATE_FAST);

		// Check for a null time series list...

		if (tslist == null)
		{
			message = "Null time series list.  Output will be empty.";
			Message.printWarning(2, routine, message);
		}

		int size = 0;
		if (tslist != null)
		{
			size = tslist.Count;
		}
		if (size == 0)
		{
			message = "No time series in list.  Output will be empty.";
			Message.printWarning(2, routine, message);
		}

		// Make sure that a non-null properties list is available
		if (props == null)
		{
			props = new PropList("DateValueTS");
		}

		// TODO SAM 2012-04-04 Eventually need to support intervals like IrregularMinute
		// Interval used with irregular time series.
		// This is needed because depending on how the irregular time series was initialized, its dates may
		// not properly indicate the precision.  For example, a DateTime precision (and interval) of IRREGULAR
		// may be used in cases where the data were read from a data format where the precision could not be
		// determined.
		TimeInterval irregularInterval = null;
		object obj = props.getContents("IrregularInterval");
		if (obj != null)
		{
			irregularInterval = (TimeInterval)obj;
		}
		string version = props.getValue("Version");
		bool version14 = false; // Currently the only version that is supported other than current version
		int versionInt = __VERSION_CURRENT_INT;
		if (!string.ReferenceEquals(version, null))
		{
			if (version.Equals("1.4"))
			{
				version14 = true;
			}
			versionInt = int.Parse(version.Trim().Replace(".","")) * 1000;
		}

		// Set the parameters for output..

		TSLimits limits = new TSLimits();
		if (size > 0)
		{
			if ((date1 == null) || (date2 == null))
			{
				// Get the limits...
				try
				{
					limits = TSUtil.getPeriodFromTS(tslist, TSUtil.MAX_POR);
				}
				catch (Exception)
				{
				}
			}
			if (date1 == null)
			{
				// Use the maximum period in the time series list...
				outputStart = new DateTime(limits.getDate1());
			}
			else
			{
				outputStart = new DateTime(date1);
			}

			if (date2 == null)
			{
				// Use the time series value...
				outputEnd = new DateTime(limits.getDate2());
			}
			else
			{
				outputEnd = new DateTime(date2);
			}
		}

		// Loop through the time series and make sure they have the same interval...

		 int dataIntervalBase = TimeInterval.UNKNOWN;
		int dataIntervalMult = 0;
		int iDataIntervalBase = 0;
		int iDataIntervalMult = 0;

		TS ts = null;
		// Set up conversion factors for units (apparently some compilers don't
		// like allocating one slot so always make it more and ignore the extra value)...
		double[] mult = new double[size + 1];
		double[] add = new double[size + 1];
		DataUnitsConversion conversion;
		int nonNullCount = 0;
		for (int i = 0; i < size; i++)
		{
			mult[i] = 1.0;
			add[i] = 0.0;
			ts = tslist[i];
			if (ts != null)
			{
				++nonNullCount;
				iDataIntervalBase = ts.getDataIntervalBase();
				iDataIntervalMult = ts.getDataIntervalMult();
				if (nonNullCount == 1)
				{
					// First non-null time series so initialize interval
					dataIntervalBase = iDataIntervalBase;
					dataIntervalMult = iDataIntervalMult;
				}
				else if ((dataIntervalBase != iDataIntervalBase) || (dataIntervalMult != iDataIntervalMult))
				{
					message = "Time series do not have the same interval.  Can't write";
					Message.printWarning(2, routine, message);
					throw new UnequalTimeIntervalException(message);
				}
			}
			// Get the conversion factors to use for output.  Don't call
			// TSUtil.convertUnits because we don't want to alter the time series itself...
			if ((ts != null) && (!string.ReferenceEquals(units, null)) && (units.Length != 0) && !units.Equals(ts.getDataUnits(), StringComparison.OrdinalIgnoreCase))
			{
				try
				{
					conversion = DataUnits.getConversion(ts.getDataUnits(), units);
					mult[i] = conversion.getMultFactor();
					add[i] = conversion.getAddFactor();
				}
				catch (Exception)
				{
					Message.printWarning(2, routine, "Unable to convert units to \"" + units + "\" leaving units as \"" + ts.getDataUnits() + "\"");
				}
			}
		}

		// Write header.  See the printSample() method for an example string...

		string delim = " ";
		string propval = props.getValue("Delimiter");
		if (!string.ReferenceEquals(propval, null))
		{
			delim = propval;
		}
		int precision = 4;
		propval = props.getValue("Precision");
		if ((!string.ReferenceEquals(propval, null)) && StringUtil.isInteger(propval))
		{
			precision = int.Parse(propval);
		}
		// Override of missing value in the time series
		string missingValueString = props.getValue("MissingValue");
		if ((!string.ReferenceEquals(missingValueString, null)) && !StringUtil.isDouble(missingValueString))
		{
			Message.printWarning(3, routine, "Specified missing value \"" + missingValueString + "\" is not a number - ignoring.");
			missingValueString = null;
		}
		// Indicate which properties should be written
		object includePropertiesProp = props.getContents("IncludeProperties");
		string[] includeProperties = null;
		if (includePropertiesProp != null)
		{
			includeProperties = (string [])includePropertiesProp;
			for (int i = 0; i < includeProperties.Length; i++)
			{
				includeProperties[i] = includeProperties[i].Replace("*", ".*"); // Change glob notation to Java regular expression
			}
		}
		// Indicate whether data flag descriptions should be written
		string writeDataFlagDescriptions0 = props.getValue("WriteDataFlagDescriptions");
		bool writeDataFlagDescriptions = false; // default
		if ((!string.ReferenceEquals(writeDataFlagDescriptions0, null)) && writeDataFlagDescriptions0.Equals("true", StringComparison.OrdinalIgnoreCase))
		{
			writeDataFlagDescriptions = true;
		}
		string outputFormat = "%." + precision + "f";
		string nodataString = "?";
		StringBuilder aliasBuffer = new StringBuilder();
		bool hasSeqnum = false;
		StringBuilder seqnumBuffer = new StringBuilder();
		StringBuilder dataflagBuffer = new StringBuilder();
		StringBuilder columnsBuffer = new StringBuilder();
		StringBuilder datatypeBuffer = new StringBuilder();
		StringBuilder descriptionBuffer = new StringBuilder();
		StringBuilder missingvalBuffer = new StringBuilder();
		StringBuilder tsidBuffer = new StringBuilder();
		StringBuilder unitsBuffer = new StringBuilder();

		if (dataIntervalBase == TimeInterval.IRREGULAR)
		{
			// Check the start date/time to see if the data precision includes time.  If no start is
			// defined, assume no time - will not matter since no data
			string header = "Date" + delim;
			if (size == 1)
			{
				ts = tslist[0];
				if (ts != null)
				{
					DateTime start = ts.getDate1();
					if (start != null)
					{
						int p = start.getPrecision();
						if ((p == DateTime.PRECISION_HOUR) || (p == DateTime.PRECISION_MINUTE) | (p == DateTime.PRECISION_SECOND))
						{
							header += "Time" + delim;
						}
					}
				}
			}
			columnsBuffer.Append(header);
		}
		else if ((dataIntervalBase == TimeInterval.MINUTE) || (dataIntervalBase == TimeInterval.HOUR))
		{
			columnsBuffer.Append("Date" + delim + "Time" + delim);
		}
		else
		{
			columnsBuffer.Append("Date" + delim);
		}
		bool hasDataFlags = false; // Only include data flags in output if
						// at least one time series actually has the flag.
		for (int i = 0; i < size; i++)
		{
			ts = tslist[i];
			if (i != 0)
			{
				// Append the delimiter...
				aliasBuffer.Append(delim);
				seqnumBuffer.Append(delim);
				columnsBuffer.Append(delim);
				dataflagBuffer.Append(delim);
				datatypeBuffer.Append(delim);
				descriptionBuffer.Append(delim);
				missingvalBuffer.Append(delim);
				tsidBuffer.Append(delim);
				unitsBuffer.Append(delim);
			}
			// Now add the data...
			if (ts == null)
			{
				aliasBuffer.Append("\"" + nodataString + "\"");
				seqnumBuffer.Append("\"" + nodataString + "\"");
				columnsBuffer.Append(nodataString);
				dataflagBuffer.Append("\"" + nodataString + "\"");
				datatypeBuffer.Append(nodataString);
				descriptionBuffer.Append("\"" + nodataString + "\"");
				missingvalBuffer.Append(nodataString);
				tsidBuffer.Append("\"" + nodataString + "\"");
				unitsBuffer.Append(nodataString);
			}
			else
			{
				string alias = ts.getAlias();
				aliasBuffer.Append("\"" + alias + "\"");
				if ((!string.ReferenceEquals(ts.getSequenceID(), null)) && !ts.getSequenceID().Equals(""))
				{
					// At least one time series has the sequence number so it will be output below.
					hasSeqnum = true;
				}
				if ((string.ReferenceEquals(ts.getSequenceID(), null)) || ts.getSequenceID().Equals(""))
				{
					if (version14)
					{
						seqnumBuffer.Append("-1");
					}
					else
					{
						seqnumBuffer.Append("\"\"");
					}
				}
				else
				{
					if (version14)
					{
						// Used integer sequence numbers, so output -1 if not an integer
						try
						{
							int.Parse(ts.getSequenceID());
							seqnumBuffer.Append(ts.getSequenceID());
						}
						catch (System.FormatException)
						{
							seqnumBuffer.Append("-1");
						}
					}
					else
					{
						seqnumBuffer.Append("\"" + ts.getSequenceID() + "\"");
					}
				}
				if (!ts.getDataUnits().Trim().Equals(""))
				{
					// Has units so display in column heading...
					if (alias.Length > 0)
					{
						// Use the alias.
						columnsBuffer.Append("\"" + alias + ", " + ts.getDataUnits() + "\"");
					}
					else
					{
						 columnsBuffer.Append("\"" + ts.getIdentifier().ToString() + ", " + ts.getDataUnits() + "\"");
					}
				}
				else
				{
					if (alias.Length > 0)
					{
						columnsBuffer.Append("\"" + alias + "\"");
					}
					else
					{
						columnsBuffer.Append("\"" + ts.getIdentifier().ToString() + "\"");
					}
				}
				if (ts.hasDataFlags())
				{
					hasDataFlags = true;
					dataflagBuffer.Append("true");
					columnsBuffer.Append(delim);
					columnsBuffer.Append("DataFlag");
				}
				else
				{
					dataflagBuffer.Append("false");
				}
				if (ts.getDataType().Trim().Equals(""))
				{
					datatypeBuffer.Append("\"" + ts.getIdentifier().getType() + "\"");
				}
				else
				{
					datatypeBuffer.Append("\"" + ts.getDataType() + "\"");
				}
				descriptionBuffer.Append("\"" + ts.getDescription() + "\"");
				// If the missing value is NaN, just print NaN.  Otherwise the %.Nf results in NaN.000...
				// The following is a trick to check for NaN...
				if (!string.ReferenceEquals(missingValueString, null))
				{
					// Property has specified the missing value to use
					missingvalBuffer.Append(missingValueString);
				}
				else
				{
					if (ts.getMissing() != ts.getMissing())
					{
						missingvalBuffer.Append("NaN");
					}
					else
					{
						// Assume that missing is indicated by a number...
						missingvalBuffer.Append(StringUtil.formatString(ts.getMissing(),outputFormat));
					}
				}
				tsidBuffer.Append("\"" + ts.getIdentifier().ToString() + "\"");
				unitsBuffer.Append("\"" + ts.getDataUnits() + "\"");
			}
		}

		// Print the standard header...

		if (version14)
		{
			@out.println("# DateValueTS 1.4 file");
		}
		else
		{
			@out.println("# DateValueTS " + __VERSION_CURRENT + " file");
		}
		IOUtil.printCreatorHeader(@out, "#", 80, 0);
		object o = props.getContents("OutputComments");
		if (o != null)
		{
			// Write additional comments that were passed in
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<String> comments = (java.util.List<String>)o;
			IList<string> comments = (IList<string>)o;
			int commentSize = comments.Count;
			if (commentSize > 0)
			{
				for (int iComment = 0; iComment < commentSize; iComment++)
				{
					@out.println("# " + comments[iComment]);
				}
			}
		}
		@out.println("#");
		@out.println("Delimiter   = \"" + delim + "\"");
		@out.println("NumTS       = " + size);
		@out.println("TSID        = " + tsidBuffer.ToString());
		@out.println("Alias       = " + aliasBuffer.ToString());
		if (hasSeqnum)
		{
			if (version14)
			{
				// Format 1.4, where sequence number is an integer, with -1 indicating no sequence number
				@out.println("SequenceNum = " + seqnumBuffer.ToString());
			}
			else
			{
				// Format as of 1.5, where sequence ID is a string
				@out.println("SequenceID  = " + seqnumBuffer.ToString());
			}
		}
		@out.println("Description = " + descriptionBuffer.ToString());
		@out.println("DataType    = " + datatypeBuffer.ToString());
		@out.println("Units       = " + unitsBuffer.ToString());
		@out.println("MissingVal  = " + missingvalBuffer.ToString());
		if (hasDataFlags)
		{
			// At least one of the time series in the list has data flags
			// so output the data flags information for all the time series...
			@out.println("DataFlags   = " + dataflagBuffer.ToString());
		}
		if (versionInt >= 16000)
		{
			// Writing time series properties and data flag descriptions was added to version 1.6
			if (includeProperties != null)
			{
				for (int its = 0; its < tslist.Count; its++)
				{
					ts = tslist[its];
					if (ts == null)
					{
						continue;
					}
					else
					{
						// Output the properties
						writeTimeSeriesProperties(@out,ts,its,includeProperties);
					}
				}
			}
			if (writeDataFlagDescriptions)
			{
				for (int its = 0; its < tslist.Count; its++)
				{
					ts = tslist[its];
					if (ts == null)
					{
						continue;
					}
					else
					{
						writeTimeSeriesDataFlagDescriptions(@out, ts, its);
					}
				}
			}
		}
		if (size == 0)
		{
			@out.println("# Unable to determine data start and end - no time series.");
		}
		else
		{
			@out.println("Start       = " + outputStart.ToString());
			@out.println("End         = " + outputEnd.ToString());
		}

		// Print the comments/genesis information...

		@out.println("#");
		@out.println("# Time series comments/histories:");
		@out.println("#");

		string printGenesis = "true";
		IList<string> genesis = null;
		IList<string> comments = null;
		int j = 0, jsize = 0;
		for (int i = 0; i < size; i++)
		{
			ts = tslist[i];
			if (ts == null)
			{
				@out.println("#");
				@out.println("# Time series " + (i + 1) + " is null");
				@out.println("#");
				continue;
			}
			comments = ts.getComments();
			genesis = ts.getGenesis();
			if (((comments == null) || (comments.Count == 0)) && ((genesis == null) || (genesis.Count == 0)))
			{
				@out.println("#");
				@out.println("# Time series " + (i + 1) + " (TSID=" + ts.getIdentifier().ToString() + " Alias=" + ts.getAlias() + ") has no comments or history");
				@out.println("#");
				continue;
			}
			if ((comments != null) && (comments.Count > 0))
			{
				@out.println("#");
				@out.println("# Comments for time series " + (i + 1) + " (TSID=" + ts.getIdentifier().ToString() + " Alias=" + ts.getAlias() + "):");
				@out.println("#");
				jsize = comments.Count;
				for (j = 0; j < jsize; j++)
				{
					@out.println("#   " + comments[j]);
				}
			}
			if ((genesis != null) && (genesis.Count > 0) && printGenesis.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				@out.println("#");
				@out.println("# Creation history for time series " + (i + 1) + " (TSID=" + ts.getIdentifier().ToString() + " Alias=" + ts.getAlias() + "):");
				@out.println("#");
				jsize = genesis.Count;
				for (j = 0; j < jsize; j++)
				{
					@out.println("#   " + genesis[j]);
				}
			}
		}
		@out.println("#");
		@out.println("#EndHeader");

		if (!writeData)
		{
			// Don't want to write the data...
			return;
		}

		// Header line indicating data columns....

		@out.println(columnsBuffer.ToString());

		double value = 0.0;
		string string_value;

		// Need to add iterator at some point - could use this to test performance...
		StringBuilder buffer = new StringBuilder();
		TSData datapoint = new TSData(); // Data point associated with a date - used to get flags.
		string dataflag; // Data flag associated with a data point.
		if (dataIntervalBase == TimeInterval.IRREGULAR)
		{
			// Irregular interval... loop through all of the values...
			// This assumes that _date1 and _date2 have been set.
			if (size == 1)
			{
				// Legacy logic that works with one irregular time series (should be fast)
				IrregularTS its = null;
				IList<TSData> alldata = null;
				if (ts != null)
				{
					its = (IrregularTS)ts;
					alldata = its.getData();
				}
				if (alldata == null)
				{
					return;
				}
				int dataSize = alldata.Count;
				TSData tsdata = null;
				DateTime date;
				buffer = new StringBuilder();
				for (int i = 0; i < dataSize; i++)
				{
					buffer.Length = 0;
					tsdata = alldata[i];
					if (tsdata == null)
					{
						break;
					}
					date = tsdata.getDate();
					if (date.lessThan(outputStart))
					{
						continue;
					}
					else if (date.greaterThan(outputEnd))
					{
						break;
					}
					// Else print the record...
					value = tsdata.getDataValue();
					if (ts.isDataMissing(value))
					{
						 if (!string.ReferenceEquals(missingValueString, null))
						 {
							// Property has specified the missing value to use
							string_value = missingValueString;
						 }
						 else
						 {
							if (double.IsNaN(value))
							{
								string_value = "NaN";
							}
							else
							{
								string_value = StringUtil.formatString(tsdata.getDataValue(), outputFormat);
							}
						 }
					}
					else
					{
						// Convert the units...
						string_value = StringUtil.formatString((tsdata.getDataValue() * mult[0] + add[0]), outputFormat);
					}
					// Use the precision of the dates in the data - ISO formats will be used by default...
					buffer.Append(date.ToString());
					buffer.Append(delim);
					buffer.Append(string_value);
					// Now print the data flag...
					if (ts.hasDataFlags())
					{
						dataflag = tsdata.getDataFlag();
						// Always enclose the data flag in quotes because it may contain white space...
						buffer.Append(delim);
						buffer.Append("\"");
						buffer.Append(dataflag);
						buffer.Append("\"");
					}
					@out.println(buffer.ToString());
				}
			}
			else
			{
				// More than one irregular time series.  They at least have to have the same date/time precision
				// for the period.  Otherwise it will be difficult to navigate the data.  For now develop this code
				// separately but if the logic works out, it should be able to replace the above block of code.
				int irrPrecision = -1;
				int tsPrecision;
				if (irregularInterval != null)
				{
					// Use the precision that was specified
					irrPrecision = irregularInterval.getBase();
				}
				else
				{
					for (int its = 0; its < size; its++)
					{
						if (tslist[its] == null)
						{
							continue;
						}
						ts = (IrregularTS)tslist[its];
						if (ts.getDate1() == null)
						{
							continue;
						}
						tsPrecision = ts.getDate1().getPrecision();
						if (tsPrecision == TimeInterval.IRREGULAR)
						{
							// Treat as minute
							tsPrecision = DateTime.PRECISION_MINUTE;
						}
						if (irrPrecision == -1)
						{
							// Just assign
							irrPrecision = tsPrecision;
						}
						else if (irrPrecision != tsPrecision)
						{
							// This will be a problem in processing the data
							message = "Irregular time series do not have the same date/time precision.  Can't write";
							Message.printWarning(2, routine, message);
							throw new UnequalTimeIntervalException(message);
						}
					}
				}
				if (irrPrecision == -1)
				{
					// Apparently no non-null time series with data
					message = "Irregular time series do not have data to determine date/time precision (all empty?).  Can't write";
					Message.printWarning(2, routine, message);
					throw new System.ArgumentException(message);
				}
				// Was able to determine the precision of data so can continue
				// The logic works as follows:
				// 0) Advance the iterator for each time series to initialize
				// 1) Find the earliest date/time in the iterator current position
				// 2) Output the values at the earliest date/time order
				//    - actual value if time series has a value at the date/time
				//    - values not at the same date/time result in blanks for the other time series
				// 3) For any values printed, advance that time series' iterator
				// 4) Go to step 1
				// Create iterators for each time series
				IList<TSIterator> tsIteratorList = new List<TSIterator>(size);
				for (int its = 0; its < size; its++)
				{
					if (tslist[its] == null)
					{
						tsIteratorList.Add(null); // Keep same order as time series
					}
					ts = (IrregularTS)tslist[its];
					try
					{
						tsIteratorList.Add(ts.iterator(outputStart,outputEnd));
					}
					catch (Exception)
					{
						tsIteratorList.Add(null); // Keep same order as time series
					}
				}
				int its;
				TSIterator itsIterator;
				DateTime dtEarliest;
				// Use the following to extract data from each time series
				// A call to the iterator next() method will return null when no more data, which is
				// the safest way to process the data
				TSData[] tsdata = new TSData[size];
				DateTime dt;
				int loopCount = 0;
				DateTime dtEarliestOutput = new DateTime(irrPrecision); // Use for output to ensure precision
				while (true)
				{
					// Using the current date/time, output the earliest value for all time series that have the value and
					// increment the iterator for each value that is output.
					dtEarliest = null;
					++loopCount;
					if (loopCount == 1)
					{
						// Need to call next() one time on all time series to initialize all iterators to the first
						// data point in the time series.  Otherwise, next() is only called below
						// when an actual time series value is output.
						for (its = 0; its < size; its++)
						{
							itsIterator = tsIteratorList[its];
							if (itsIterator != null)
							{
								tsdata[its] = itsIterator.next();
							}
						}
					}
					// Figure out the earliest date/time to output from the current iterator data
					for (its = 0; its < size; its++)
					{
						if (tsdata[its] == null)
						{
							continue;
						}
						dt = tsdata[its].getDate();
						if (dt != null)
						{
							if (dtEarliest == null)
							{
								dtEarliest = dt;
							}
							else if (dt.lessThan(dtEarliest))
							{
								dtEarliest = dt;
							}
						}
					}
					if (dtEarliest == null)
					{
						// Done printing data records
						break;
					}
					// Make sure the date/time for output is set the proper precision for equals() calls and formatting
					dtEarliestOutput.setDate(dtEarliest);
					dtEarliestOutput.setPrecision(irrPrecision);
					// First print the date/time
					buffer.Length = 0;
					buffer.Append("" + dtEarliestOutput);
					for (its = 0; its < size; its++)
					{
						dt = null;
						if (tsdata[its] != null)
						{
							dt = tsdata[its].getDate();
						}
						if ((dt != null) && dtEarliestOutput.Equals(dt))
						{
							// Output the value and if requested the flag (this copied from below for regular time series)
							if (ts != null)
							{
								value = tsdata[its].getDataValue();
							}
							if ((ts == null) || ts.isDataMissing(value))
							{
								if (!string.ReferenceEquals(missingValueString, null))
								{
									// Property has specified the missing value to use
									string_value = missingValueString;
								}
								else
								{
									if (double.IsNaN(value))
									{
										string_value = "NaN";
									}
									else
									{
										// Format the missing value number
										string_value = StringUtil.formatString(value, outputFormat);
									}
								}
							}
							else
							{
								// Format the data value
								string_value = StringUtil.formatString((value * mult[its] + add[its]),outputFormat);
							}
							if (its == 0)
							{
								buffer.Append(string_value);
							}
							else
							{
								buffer.Append(delim + string_value);
							}
							// Now print the data flag...
							if (ts.hasDataFlags())
							{
								dataflag = tsdata[its].getDataFlag();
								// Always enclose the data flag in quotes because it may contain white space...
								buffer.Append(delim + "\"" + dataflag + "\"");
							}
							// Get the next value since this time series was able to output
							// Advancing past data will result in null for calls to request the date, etc.
							itsIterator = tsIteratorList[its];
							tsdata[its] = itsIterator.next();
						}
						else
						{
							// Output blanks (quoted blanks for illustration)
							buffer.Append(delim + "");
							if (ts.hasDataFlags())
							{
								buffer.Append(delim + "\"" + "" + "\"");
							}
							// Keep the iterator at the same spot so that the value will be tested for order
						}
					}
					// Output the line
					@out.println(buffer.ToString());
				}
			}
		}
		else if ((dataIntervalBase != TimeInterval.IRREGULAR) && (outputStart != null) && (outputEnd != null))
		{
			// Regular interval and have period to output...
			t = new DateTime(outputStart);
			// Make sure no time zone is set to minimize output...
			t.setTimeZone("");
			int its;
			for (; t.lessThanOrEqualTo(outputEnd); t.addInterval(dataIntervalBase, dataIntervalMult))
			{
				buffer.Length = 0;
				//buffer.append( t.toString().replace(' ','@') + delim);
				buffer.Append(t.ToString() + delim);
				for (its = 0; its < size; its++)
				{
					ts = tslist[its];
					// Need to work on formatting number to a better precision.  For now just get to
					// output without major loss in precision...
					if (ts != null)
					{
						value = ts.getDataValue(t);
					}
					if ((ts == null) || ts.isDataMissing(value))
					{
						if (!string.ReferenceEquals(missingValueString, null))
						{
							// Property has specified the missing value to use
							string_value = missingValueString;
						}
						else
						{
							if (double.IsNaN(value))
							{
								string_value = "NaN";
							}
							else
							{
								// Format the missing value number
								string_value = StringUtil.formatString(value, outputFormat);
							}
						}
					}
					else
					{
						// Format the data value
						string_value = StringUtil.formatString((value * mult[its] + add[its]),outputFormat);
					}
					if (its == 0)
					{
						buffer.Append(string_value);
					}
					else
					{
						buffer.Append(delim + string_value);
					}
					// Now print the data flag...
					if (ts.hasDataFlags())
					{
						datapoint = ts.getDataPoint(t, datapoint);
						dataflag = datapoint.getDataFlag();
						// Always enclose the data flag in quotes because it may contain white space...
						buffer.Append(delim + "\"" + dataflag + "\"");
					}
				}
				@out.println(buffer.ToString());
			}
		}
	}

	/// <summary>
	/// Write multiple time series to a single file.  This is useful when the time
	/// series are to be read into a spreadsheet.  The standard DateValue time series
	/// header is used, but each data item is separated by a | delimiter.  The time
	/// series must have the same interval and the overall output period will be that
	/// of the maximum bounds of the time series.  The following additional data
	/// properties are included in the header:
	/// <pre>
	/// # Indicate the number of time series in the file.
	/// NumTS = #
	/// </pre> </summary>
	/// <param name="tslist"> list of time series. </param>
	/// <param name="fname"> file name to write. </param>
	/// <exception cref="Exception"> if there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeTimeSeriesList(java.util.List<TS> tslist, String fname) throws Exception
	public static void writeTimeSeriesList(IList<TS> tslist, string fname)
	{
		writeTimeSeriesList(tslist, fname, (DateTime)null, (DateTime)null, null, true);
	}

	/// <summary>
	/// Write a list of time series to the specified file. </summary>
	/// <param name="tslist"> list of time series to write. </param>
	/// <param name="fname"> Name of file to write.
	/// The IOUtil.getPathUsingWorkingDir() method is applied to the filename. </param>
	/// <param name="date1"> First date to write (if NULL write the entire time series). </param>
	/// <param name="date2"> Last date to write (if NULL write the entire time series). </param>
	/// <param name="units"> Units to write.  If different than the current units the units will be converted on output. </param>
	/// <param name="write_data"> Indicates whether data should be written (as opposed to only writing the header). </param>
	/// <exception cref="Exception"> if there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeTimeSeriesList(java.util.List<TS> tslist, String fname, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2, String units, boolean write_data) throws Exception
	public static void writeTimeSeriesList(IList<TS> tslist, string fname, DateTime date1, DateTime date2, string units, bool write_data)
	{
		writeTimeSeriesList(tslist, fname, date1, date2, units, write_data, null);
	}

	/// <summary>
	/// Write a list of time series to the specified file. </summary>
	/// <param name="tslist"> list of time series to write. </param>
	/// <param name="fname"> Name of file to write.
	/// The IOUtil.getPathUsingWorkingDir() method is applied to the filename. </param>
	/// <param name="date1"> First date to write (if NULL write the entire time series). </param>
	/// <param name="date2"> Last date to write (if NULL write the entire time series). </param>
	/// <param name="units"> Units to write.  If different than the current units the units will be converted on output. </param>
	/// <param name="writeData"> Indicates whether data should be written (as opposed to only writing the header). </param>
	/// <param name="props"> Properties to control output, as follows:
	/// <table width=100% cellpadding=10 cellspacing=0 border=2>
	/// <tr>
	/// <td><b>Property</b></td> <td><b>Description</b></td> <td><b>Default</b></td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>Delimiter</b></td>
	/// <td><b>The delimiter to use in output.</b>
	/// <td>Space</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>IncludeProperties</b></td>
	/// <td><b>An array of strings indicating properties to write, with * to use glob-style pattern matching.</b>
	/// <td>Do not write any properties.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>IrregularInterval</b></td>
	/// <td><b>The TimeInterval that indicates the interval for date/times when outputting more than one irregular time series</b>
	/// <td>Determine from time series list (precision of starting date/time).</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>MissingValue</b></td>
	/// <td><b>The missing value to be output for numerical values.</b>
	/// <td>4</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>OutputComments</b></td>
	/// <td><b>Additional comments to be output in the header, as a list of String.  The comment
	/// lines are not added to in any way.</b>
	/// <td>No additional comments.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>Precision</b></td>
	/// <td><b>The precision (number of digits after the decimal) to use for numerical values.</b>
	/// <td>4</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>Version</b></td>
	/// <td><b>The DateValue file format version.</b>
	/// <td>Current version.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>WriteDataFlagDescriptions</b></td>
	/// <td><b>A string "true" or "false" indicating whether data flag descriptions should be written.</b>
	/// <td>false (to adhere to legacy behavior)</td>
	/// </tr>
	/// 
	/// </table> </param>
	/// <exception cref="Exception"> if there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeTimeSeriesList(java.util.List<TS> tslist, String fname, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2, String units, boolean writeData, RTi.Util.IO.PropList props) throws Exception
	public static void writeTimeSeriesList(IList<TS> tslist, string fname, DateTime date1, DateTime date2, string units, bool writeData, PropList props)
	{
		string routine = "DateValueTS.writeTimeSeriesList";

		string full_fname = IOUtil.getPathUsingWorkingDir(fname);
		try
		{
			FileStream fos = new FileStream(full_fname, FileMode.Create, FileAccess.Write);
			PrintWriter fout = new PrintWriter(fos);

			try
			{
				writeTimeSeriesList(tslist, fout, date1, date2, units, writeData, props);
			}
			finally
			{
				fout.close();
			}
		}
		catch (UnequalTimeIntervalException e)
		{
			// Just rethrow because message will be specific
			throw e;
		}
		catch (Exception e)
		{
			string message = "Error writing \"" + full_fname + "\" (" + e + ").";
			Message.printWarning(2, routine, message);
			Message.printWarning(3, routine, e);
			throw new Exception(message);
		}
	}

	/// <summary>
	/// Write the properties for a time series.
	/// </summary>
	private static void writeTimeSeriesProperties(PrintWriter @out, TS ts, int its, string[] includeProperties)
	{
		// Get the list of matching properties
		// TODO SAM 2015-05-18 Add support for wildcards - for now must match exactly
		object o;
		StringBuilder b = new StringBuilder("Properties_" + (its + 1) + " = {");
		// Get all the properties.  Then extract the properties that match the IncludeProperties list
		Dictionary<string, object> props = ts.getProperties();
		IList<string> matchedProps = new List<string>();
		for (int iprop = 0; iprop < includeProperties.Length; iprop++)
		{
			foreach (string key in props.Keys)
			{
				if (key.matches(includeProperties[iprop]))
				{
					// Make sure the property is not already in the list to include
					bool match = false;
					foreach (string p in matchedProps)
					{
						if (p.Equals(key))
						{
							match = true;
							break;
						}
					}
					if (!match)
					{
						matchedProps.Add(key);
					}
				}
			}
		}
		// Loop through the full list of properties and get those that match the pattern
		int iprop = -1;
		foreach (string p in matchedProps)
		{
			++iprop;
			o = ts.getProperty(p);
			if (iprop > 0)
			{
				b.Append(",");
			}
			b.Append(p + ":");
			if (o == null)
			{
				b.Append("null");
			}
			else if (o is double?)
			{
				// Don't want default of exponential notation so always format
				b.Append("" + StringUtil.formatString((double?)o,"%.6f"));
			}
			else if (o is float?)
			{
				// Don't want default of exponential notation so always format
				b.Append("" + StringUtil.formatString((float?)o,"%.6f"));
			}
			else if (o is int?)
			{
				b.Append("" + o);
			}
			else if (o is long?)
			{
				b.Append("" + o);
			}
			else if (o is string)
			{
				b.Append("\"" + o + "\"");
			}
			else
			{
				// TODO SAM 2015-05-18 this may cause problems if it contains newlines
				b.Append("\"" + o + "\"");
			}
		}
		b.Append("}");
		@out.println(b.ToString());
	}

	}

}