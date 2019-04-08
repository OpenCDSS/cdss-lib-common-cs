using System;
using System.Collections.Generic;
using System.IO;

// RiverWareTS - read and write RiverWare time series files, including individual time series and RDF files that can contain ensembles.

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

	using IOUtil = RTi.Util.IO.IOUtil;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using DateTime = RTi.Util.Time.DateTime;
	using TimeUtil = RTi.Util.Time.TimeUtil;
	using TimeInterval = RTi.Util.Time.TimeInterval;

	// TODO SAM 2013-09-21 Evaluate whether to refactor so static methods are not used (less memory footprint)
	/// <summary>
	/// The RiverWareTS class reads and writes RiverWare time series files, including individual time series and RDF
	/// files that can contain ensembles.
	/// </summary>
	public class RiverWareTS
	{

	/// <summary>
	/// Private method to create a time series given the proper heading information </summary>
	/// <param name="req_ts"> If non-null, an existing time series is expected to be passed in. </param>
	/// <param name="filename"> name of file being read, assumed to start with ObjectName.SlotName,
	/// which are used to determine the location ID and data type </param>
	/// <param name="timestep"> RiverWare timestep as string (essentially the same as TS except
	/// there is a space between the multiplier and base). </param>
	/// <param name="units"> Units of the data. </param>
	/// <param name="date1"> Requested start date. </param>
	/// <param name="date2"> Requested end date. </param>
	/// <param name="date1_file"> Start date in the file. </param>
	/// <param name="date2_file"> End date in the file. </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static TS createTimeSeries(TS req_ts, String filename, String timestep, String units, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2, RTi.Util.Time.DateTime date1_file, RTi.Util.Time.DateTime date2_file) throws Exception
	private static TS createTimeSeries(TS req_ts, string filename, string timestep, string units, DateTime date1, DateTime date2, DateTime date1_file, DateTime date2_file)
	{
		string routine = "RiverWareTS.createTimeSeries";

		// Declare the time series of the proper type based on the interval.
		// Use a TSIdent to parse out the interval information...

		string timestep2 = StringUtil.unpad(timestep, " ", StringUtil.PAD_FRONT_MIDDLE_BACK);
		string location = "";
		string datatype = "";
		IList<string> tokens = StringUtil.breakStringList(filename, ".", 0);
		if ((tokens != null) && (tokens.Count >= 2))
		{
			location = tokens[0].Trim();
			// Only want the relative part...
			File f = new File(location);
			location = f.getName();
			datatype = tokens[1].Trim();
		}
		TSIdent ident = new TSIdent(location, "", datatype, timestep2, "");

		TS ts = null;

		// Set the time series pointer to either the requested time series
		// or a newly-created time series.
		if (req_ts != null)
		{
			ts = req_ts;
			// Identifier is assumed to have been set previously.
		}
		else
		{
			ts = TSUtil.newTimeSeries(ident.ToString(), true);
		}
		if (ts == null)
		{
			Message.printWarning(3, routine, "Unable to create new time series for \"" + ident + "\"");
			return (TS)null;
		}

		// Only set the identifier if a new time series.  Otherwise assume the
		// the existing identifier is to be used (e.g., from a file name).
		if (req_ts == null)
		{
			ts.setIdentifier(ident.ToString());
		}

		// Set the parameters from the input and override with the parameters...

		string description = "";
		if (req_ts == null)
		{
			ts.setLocation(location);
			description = location + ", " + datatype;
		}
		ts.setDataUnits(units);
		ts.setDataType(datatype);
		ts.setDescription(description);
		ts.setDataUnitsOriginal(units);
		// Original dates are what is in the file...
		ts.setDate1Original(date1_file);
		ts.setDate2Original(date2_file);
		ts.setDate1(new DateTime(date1));
		ts.setDate2(new DateTime(date2));

		// Set missing to NaN since this is what RiverWare uses...

		ts.setMissing(Double.NaN);

		if (Message.isDebugOn)
		{
			Message.printDebug(10, routine, "Period to read is " + date1 + " to " + date2);
			Message.printDebug(10, routine, "Read TS header");
		}
		return ts;
	}

	/// <summary>
	/// Determine whether a file is a RiverWare time series file.  This can be used rather than
	/// checking the source in a time series identifier. </summary>
	/// <param name="filename"> name of file to check.  IOUtil.getPathUsingWorkingDir() is called to expand the filename. </param>
	/// <param name="checkForRdf"> if true, then if the file passes any of the
	/// following conditions, it is assumed to be a RiverWare single time series file:
	/// <ol>
	/// <li>	A line starts with "START_DATE:".</li>
	/// </ol>
	/// If checkForRdf is false, true is returned if END_PACKAGE_PREAMBLE is found in the file. </param>
	public static bool isRiverWareFile(string filename, bool checkForRdf)
	{
		StreamReader @in = null;
		string filenameFull = IOUtil.getPathUsingWorkingDir(filename);
		try
		{
			@in = new StreamReader(IOUtil.getInputStream(filenameFull));
			// Read lines and check for common strings that indicate a RiverWare file.
			// Search for a maximum number of non-comment lines
			int countNotComment = 0;
			string @string = null;
			bool isRiverWare = false;
			while (!string.ReferenceEquals((@string = @in.ReadLine()), null))
			{
				if (!checkForRdf && @string.regionMatches(true,0, "START_DATE:",0,11))
				{
					isRiverWare = true;
					break;
				}
				else if (checkForRdf && @string.regionMatches(true,0, "END_PACKAGE_PREAMBLE",0,20))
				{
					isRiverWare = true;
					break;
				}
				else if ((@string.Length > 0) && (@string[0] != '#'))
				{
					// Not a comment
					++countNotComment;
					if (countNotComment > 100)
					{
						// Had enough lines to check;
						break;
					}
				}
			}
			return isRiverWare;
		}
		catch (Exception)
		{
			return false;
		}
		finally
		{
			if (@in != null)
			{
				try
				{
					@in.Close();
				}
				catch (Exception)
				{
					// Absorb - should not happen
				}
			}
		}
	}

	/// <summary>
	/// Parse a RiverWare date/time string of the form YYYY-MM-DD HH:MM, allowing 1 or 2-digit parts.  Year must be 4 digits.
	/// This is the date format found in RDF files. </summary>
	/// <param name="dt"> date/time string </param>
	/// <param name="precision"> if > 0, the DateTime precision to set for the returned DateTime.  For example, if year interval, then issues of
	/// 24-hour RiverWare time can be ignored.  If </param>
	/// <returns> parsed date/time or null </returns>
	private static DateTime parseRiverWareDateTime(string dt, int precision)
	{
		if (string.ReferenceEquals(dt, null))
		{
			return null;
		}
		dt = dt.Trim();
		// First split by space
		string[] dtParts = dt.Split(" ", true);
		// Split the date
		string[] dParts = dtParts[0].Split("-", true);
		// Split the date
		string[] tParts = dtParts[1].Split(":", true);
		DateTime d = new DateTime(precision);
		if (dParts.Length > 0)
		{
			d.setYear(int.Parse(dParts[0]));
		}
		if (precision == DateTime.PRECISION_YEAR)
		{
			return d;
		}
		if (dParts.Length > 1)
		{
			d.setMonth(int.Parse(dParts[1]));
		}
		if (precision == DateTime.PRECISION_MONTH)
		{
			return d;
		}
		if (dParts.Length > 2)
		{
			d.setDay(int.Parse(dParts[2]));
		}
		if (precision == DateTime.PRECISION_DAY)
		{
			return d;
		}
		if (tParts.Length > 0)
		{
			int hour = int.Parse(tParts[0]);
			if (hour == 24)
			{
				// RiverWare data files have hour 24, which is really hour 0 of the next day
				d.addDay(1);
				d.setHour(0);
			}
			else
			{
				d.setHour(hour);
			}
		}
		if (precision == DateTime.PRECISION_HOUR)
		{
			return d;
		}
		if (tParts.Length > 1)
		{
			d.setMinute(int.Parse(tParts[1]));
		}
		return d;
	}

	/// <summary>
	/// Read the time series from a RiverWare file. </summary>
	/// <param name="filename"> Name of file to read. </param>
	/// <returns> TS for data in the file or null if there is an error reading the time series. </returns>
	public static TS readTimeSeries(string filename)
	{
		return readTimeSeries(filename, null, null, null, true);
	}

	/// <summary>
	/// Read a time series from a RiverWare format file.
	/// The resulting time series will have an identifier like STATIONID.RiverWare.Streamflow.1Day.
	/// IOUtil.getPathUsingWorkingDir() is called to expand the filename. </summary>
	/// <returns> a pointer to a newly-allocated time series if successful, or null if not. </returns>
	/// <param name="filename"> Name of file to read, assumed to start with ObjectName.SlotName </param>
	/// <param name="date1"> Starting date to initialize period (null to read the entire time series). </param>
	/// <param name="date2"> Ending date to initialize period (null to read the entire time series). </param>
	/// <param name="units"> Units to convert to. </param>
	/// <param name="read_data"> Indicates whether data should be read (false=no, true=yes). </param>
	public static TS readTimeSeries(string filename, DateTime date1, DateTime date2, string units, bool read_data)
	{
		TS ts = null;

		string full_fname = IOUtil.getPathUsingWorkingDir(filename);
		StreamReader @in = null;
		try
		{
			@in = new StreamReader(IOUtil.getInputStream(full_fname));
			// Don't have a requested time series but need the filename
			// to infer location and data type...
			ts = readTimeSeries((TS)null, @in, full_fname, date1, date2, units, read_data);
			ts.setInputName(full_fname);
			ts.getIdentifier().setInputType("RiverWare");
			ts.getIdentifier().setInputName(full_fname);
			ts.addToGenesis("Read data from \"" + full_fname + "\" for period " + ts.getDate1() + " to " + ts.getDate2());
		}
		catch (Exception)
		{
			Message.printWarning(3, "RiverWareTS.readTimeSeries(String,...)", "Unable to open file \"" + full_fname + "\"");
		}
		finally
		{
			if (@in != null)
			{
				try
				{
					@in.Close();
				}
				catch (Exception)
				{
					// Absorb - should not happen
				}
			}
		}
		return ts;
	}

	/// <summary>
	/// Read a time series from a RiverWare format file.  The TSID string is specified
	/// in addition to the path to the file.  It is expected that a TSID in the file
	/// matches the TSID (and the path to the file, if included in the TSID would not
	/// properly allow the TSID to be specified).  This method can be used with newer
	/// code where the I/O path is separate from the TSID that is used to identify the time series.
	/// The IOUtil.getPathUsingWorkingDir() method is applied to the filename. </summary>
	/// <returns> a pointer to a newly-allocated time series if successful, or null if not. </returns>
	/// <param name="tsident_string"> The full identifier for the time series to
	/// read (where the scenario is NOT the file name). </param>
	/// <param name="filename"> The name of a file to read
	/// (in which case the tsident_string must match one of the TSID strings in the file). </param>
	/// <param name="date1"> Starting date to initialize period (NULL to read the entire time series). </param>
	/// <param name="date2"> Ending date to initialize period (NULL to read the entire time series). </param>
	/// <param name="units"> Units to convert to. </param>
	/// <param name="read_data"> Indicates whether data should be read (false=no, true=yes). </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static TS readTimeSeries(String tsident_string, String filename, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2, String units, boolean read_data) throws Exception
	public static TS readTimeSeries(string tsident_string, string filename, DateTime date1, DateTime date2, string units, bool read_data)
	{
		TS ts = null;

		string full_fname = IOUtil.getPathUsingWorkingDir(filename);
		if (!IOUtil.fileReadable(full_fname))
		{
			Message.printWarning(3, "RiverWareTS.readTimeSeries", "Unable to determine file for \"" + filename + "\"");
			return ts;
		}
		StreamReader @in = null;
		try
		{
			@in = new StreamReader(IOUtil.getInputStream(full_fname));
		}
		catch (Exception)
		{
			Message.printWarning(3, "RiverWareTS.readTimeSeries(String,...)", "Unable to open file \"" + full_fname + "\"");
			return ts;
		}
		try
		{
			// Call the fully-loaded method...
			// Pass the file pointer and an empty time series, which
			// will be used to locate the time series in the file.
			ts = TSUtil.newTimeSeries(tsident_string, true);
			if (ts == null)
			{
				Message.printWarning(3, "RiverWareTS.readTimeSeries(String,...)", "Unable to create time series for \"" + tsident_string + "\"");
				return ts;
			}
			ts.setIdentifier(tsident_string);
			readTimeSeries(ts, @in, full_fname, date1, date2, units, read_data);
			ts.setInputName(full_fname);
			ts.getIdentifier().setInputType("RiverWare");
			ts.getIdentifier().setInputName(filename);
			ts.addToGenesis("Read data from \"" + full_fname + "\" for period " + ts.getDate1() + " to " + ts.getDate2());
		}
		catch (Exception e)
		{
			// Just rethrow
			throw e;
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
	/// Read a time series from a RiverWare format file. </summary>
	/// <returns> a pointer to time series if successful, null if not.  The calling code
	/// is responsible for freeing the memory for the time series. </returns>
	/// <param name="req_ts"> Pointer to time series to fill.  If null,
	/// return a new time series.  All data are reset, except for the identifier, which
	/// is assumed to have been set in the calling code. </param>
	/// <param name="in"> Reference to open input stream. </param>
	/// <param name="filename"> Name of file that is being read (assumed to start with ObjectName.SlotName,
	/// which are used for the location and data type). </param>
	/// <param name="req_date1"> Requested starting date to initialize period (or null to read the entire time series). </param>
	/// <param name="req_date2"> Requested ending date to initialize period (or null to read the entire time series). </param>
	/// <param name="req_units"> Units to convert to (currently ignored). </param>
	/// <param name="read_data"> Indicates whether data should be read (false=no, true=yes). </param>
	/// <exception cref="Exception"> if there is an error reading the time series. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static TS readTimeSeries(TS req_ts, java.io.BufferedReader in, String filename, RTi.Util.Time.DateTime req_date1, RTi.Util.Time.DateTime req_date2, String req_units, boolean read_data) throws Exception
	public static TS readTimeSeries(TS req_ts, StreamReader @in, string filename, DateTime req_date1, DateTime req_date2, string req_units, bool read_data)
	{
		string routine = "RiverWareTS.readTimeSeries";
		string @string = null, timestep_string = "", end_date_string = "", start_date_string = "", scale_string = "";
		int dl = 10;
		DateTime date1_file = null, date2_file = null;

		// Always read the header.  Optional is whether the data are read...

		int line_count = 0;

		string units = "";
		string token0, token1;
		DateTime date1 = null, date2 = null;
		TS ts = null;
		try
		{
			while (true)
			{
				@string = @in.ReadLine();
				if (string.ReferenceEquals(@string, null))
				{
					break;
				}
				++line_count;
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, routine, "Processing: \"" + @string + "\"");
				}
				@string = @string.Trim();
				if ((@string.Length == 0) || (@string[0] == '#'))
				{
					// Skip comments and blank lines...
					continue;
				}
				int pos = @string.IndexOf(":", StringComparison.Ordinal);
				if (pos < 0)
				{
					// No more header information so break out and read data...
					break;
				}

				// Break the tokens using ':' as the delimiter.  However, because dates typically
				// use 24:00, have to take more care to get the correct token.

				token0 = @string.Substring(0,pos).Trim();
				token1 = @string.Substring(pos + 1).Trim();
				if (token0.Equals("start_date", StringComparison.OrdinalIgnoreCase))
				{
					start_date_string = token1.Trim();
				}
				else if (token0.Equals("end_date", StringComparison.OrdinalIgnoreCase))
				{
					end_date_string = token1.Trim();
				}
				else if (token0.Equals("timestep", StringComparison.OrdinalIgnoreCase))
				{
					timestep_string = token1.Trim();
				}
				else if (token0.Equals("units", StringComparison.OrdinalIgnoreCase))
				{
					units = token1.Trim();
				}
				else if (token0.Equals("scale", StringComparison.OrdinalIgnoreCase))
				{
					scale_string = token1.Trim();
				}
				// Don't care about "set_scale" or "set_units" because they are
				// only significant to RiverWare (not used by TS package).
			}
		}
		catch (Exception e)
		{
			Message.printWarning(3, routine, "Error processing line " + line_count + ": \"" + @string + "\"");
			Message.printWarning(3, routine, e);
		}

		// Process the dates.  RiverWare files always have 24:00 in the dates, even if the interval
		// is >= daily.  This causes problems with the general DateTime.parse() method in that the
		// dates may roll over to the following month.  Therefore, strip the 24:00 off the date strings before parsing.

		if ((StringUtil.indexOfIgnoreCase(timestep_string, "Day", 0) >= 0) || (StringUtil.indexOfIgnoreCase(timestep_string, "Month", 0) >= 0) || (StringUtil.indexOfIgnoreCase(timestep_string, "Year", 0) >= 0) || (StringUtil.indexOfIgnoreCase(timestep_string, "Annual", 0) >= 0))
		{
			// Remove the trailing 24:00 from start and end because it cases a problem parsing (rolls over to next month).
			int pos = start_date_string.IndexOf("24:00", StringComparison.Ordinal);
			if (pos > 0)
			{
				start_date_string = start_date_string.Substring(0,pos).Trim();
			}
			pos = end_date_string.IndexOf("24:00", StringComparison.Ordinal);
			if (pos > 0)
			{
				end_date_string = end_date_string.Substring(0,pos).Trim();
			}
		}
		date1_file = DateTime.parse(start_date_string);
		date2_file = DateTime.parse(end_date_string);
		int datePrecision = date1_file.getPrecision();
		// Further set the precision based on the data interval
		if ((StringUtil.indexOfIgnoreCase(timestep_string, "Day", 0) >= 0))
		{
			datePrecision = DateTime.PRECISION_DAY;
		}
		else if ((StringUtil.indexOfIgnoreCase(timestep_string, "Month", 0) >= 0))
		{
			datePrecision = DateTime.PRECISION_MONTH;
		}
		else if ((StringUtil.indexOfIgnoreCase(timestep_string, "Year", 0) >= 0) || (StringUtil.indexOfIgnoreCase(timestep_string, "Annual", 0) >= 0))
		{
			datePrecision = DateTime.PRECISION_YEAR;
		}
		date1_file.setPrecision(datePrecision);
		date2_file.setPrecision(datePrecision);

		// Create an in-memory time series and set header information...

		if (req_date1 != null)
		{
			date1 = new DateTime(req_date1,datePrecision);
		}
		else
		{
			date1 = date1_file;
		}
		if (req_date2 != null)
		{
			date2 = new DateTime(req_date2,datePrecision);
		}
		else
		{
			date2 = new DateTime(date2_file);
		}
		ts = createTimeSeries(req_ts, filename, timestep_string, units, date1, date2, date1_file, date2_file);
		if (!read_data)
		{
			return ts;
		}

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Reading data...");
		}

		// Allocate the memory for the data array...

		if (ts.allocateDataSpace() == 1)
		{
			Message.printWarning(3, routine, "Error allocating data space...");
			// Clean up memory...
			throw new Exception("Error allocating time series memory.");
		}

		int data_interval_base = ts.getDataIntervalBase();
		int data_interval_mult = ts.getDataIntervalMult();

		// The latest string that was read for the header should be the first
		// data line so make sure not to skip it...

		try
		{
			bool first_data = true;
			// Dates are not specified in the file so iterate with date...
			DateTime date = new DateTime(date1_file);
			double scale = 1.0;
			if (StringUtil.isDouble(scale_string))
			{
				scale = Convert.ToDouble(scale_string);
			}
			for (; date.lessThanOrEqualTo(date2_file); date.addInterval(data_interval_base, data_interval_mult))
			{
				if (first_data)
				{
					first_data = false;
				}
				else
				{
					@string = @in.ReadLine();
					++line_count;
					if (string.ReferenceEquals(@string, null))
					{
						break;
					}
				}
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, routine, "Processing: \"" + @string + "\"");
				}
				@string = @string.Trim();
				if ((@string.Length == 0) || (@string[0] == '#'))
				{
					// Skip comments and blank lines...
					continue;
				}

				// String will contain a data value or "NaN".  If "NaN", just
				// skip because the time series is initialized to missing data.

				if (date.lessThan(date1))
				{
					// No need to do...
					continue;
				}
				else if (date.greaterThan(date2))
				{
					if (Message.isDebugOn)
					{
						Message.printDebug(dl, routine, "Finished reading data at: " + date.ToString());
					}
					// Will return below...
					break;
				}
				// Else set the data value...
				if (!@string.Equals("NaN", StringComparison.OrdinalIgnoreCase))
				{
					if (Message.isDebugOn)
					{
						Message.printDebug(dl, routine, "Line " + line_count + " setting value " + @string + "*scale at " + date);
					}
					ts.setDataValue(date, scale * Convert.ToDouble(@string));
				}
				else if (Message.isDebugOn)
				{
					Message.printDebug(dl, routine, "Line " + line_count + " setting value " + @string + " at " + date);
				}
			}
		}
		catch (Exception e)
		{
			Message.printWarning(3, routine, "Error processing line " + line_count + ": \"" + @string + "\"");
			Message.printWarning(3, routine, e);
		}
		return ts;
	}

	/// <summary>
	/// Read multiple time series from a RiverWare RDF format file. </summary>
	/// <returns> a pointer to a newly-allocated time series if successful, or null if not. </returns>
	/// <param name="filename"> Name of file to read. </param>
	/// <param name="readStart"> Starting date to initialize period (null to read the entire time series). </param>
	/// <param name="readEnd"> Ending date to initialize period (null to read the entire time series). </param>
	/// <param name="units"> Units to convert to. </param>
	/// <param name="readData"> Indicates whether data should be read (false=no, true=yes). </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<TS> readTimeSeriesListFromRdf(String filename, RTi.Util.Time.DateTime readStart, RTi.Util.Time.DateTime readEnd, String units, boolean readData) throws Exception
	public static IList<TS> readTimeSeriesListFromRdf(string filename, DateTime readStart, DateTime readEnd, string units, bool readData)
	{
		string routine = "RiverWareTS.readTimeSeriesFromList", message;

		StreamReader @in = null;
		IList<TS> tslist = new List<TS>();
		try
		{
			@in = new StreamReader(IOUtil.getInputStream(filename));
			tslist = readTimeSeriesListFromRdf(filename, @in, readStart, readEnd, units, readData);
		}
		catch (Exception e)
		{
			message = "Error reading file \"" + filename + "\" ( " + e + ").";
			Message.printWarning(3, routine, message);
			Message.printWarning(3, routine, e);
			throw (e);
		}
		finally
		{
			if (@in != null)
			{
				try
				{
					@in.Close();
				}
				catch (Exception)
				{
					// Absorb - should not happen
				}
			}
		}
		return tslist;
	}

	/// <summary>
	/// Read multiple time series from a RiverWare RDF format file. </summary>
	/// <returns> a pointer to a newly-allocated time series if successful, or null if not. </returns>
	/// <param name="filename"> Name of file to read. </param>
	/// <param name="readStart"> Starting date to initialize period (null to read the entire time series). </param>
	/// <param name="readEnd"> Ending date to initialize period (null to read the entire time series). </param>
	/// <param name="units"> Units to convert to. </param>
	/// <param name="readData"> Indicates whether data should be read (false=no, true=yes). </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<TS> readTimeSeriesListFromRdf(String filename, java.io.BufferedReader in, RTi.Util.Time.DateTime readStart, RTi.Util.Time.DateTime readEnd, String units, boolean readData) throws java.io.IOException
	public static IList<TS> readTimeSeriesListFromRdf(string filename, StreamReader @in, DateTime readStart, DateTime readEnd, string units, bool readData)
	{
		string routine = "RiverWareTS.readTimeSeriesListFromRdf";
		TS ts = null;
		List<TS> tslist = new List<TS>();
		string s, su, s2 = null;
		int lineCount = 0; // Line being read (so increment before reading)
		// Package properties
		string packageName = "";
		string packageOwner = "";
		string packageDescription = "";
		string packageCreateDate = ""; // Use string because use 24 hour clock
		DateTime packageCreateDate_DateTime = null;
		int packageNumberOfRuns = 0;
		// Run properties
		string runStart = "";
		DateTime runStart_DateTime = null;
		string runEnd = "";
		DateTime runEnd_DateTime = null;
		string runTimeStepUnit = "";
		TimeInterval runTimeStep_TimeInterval = null;
		int intervalBase = 0;
		int intervalMult = 0;
		int runUnitQuantity = -1;
		int runTimeSteps = -1;
		string runSlotSet = "";
		string runRuleSet = "";
		int runConsecutive = -1;
		int runIdxSequential = -1;
		// Slot properties
		string slotObjectType = "";
		string slotObjectName = "";
		string slotSlotName = "";
		string slotUnits = "";
		double slotScale = 1.0;
		int slotRows = -1;
		int slotCols = -1;
		double value = 0.0;
		bool slotIsTable = false;
		// Loop over lines in file and parse - for now do it all linear rather than in separate methods because of data sharing
		int colonPos;
		while (true)
		{
			++lineCount;
			s = @in.ReadLine();
			if (string.ReferenceEquals(s, null))
			{
				break;
			}
			// Parse package preamble strings, in order of documentation
			s = s.Trim();
			su = s.ToUpper();
			colonPos = s.IndexOf(":", StringComparison.Ordinal);
			if (su.StartsWith("#", StringComparison.Ordinal))
			{
				// comment
				continue;
			}
			else if (su.StartsWith("NAME:", StringComparison.Ordinal))
			{
			packageName = (s.Length >= (colonPos + 1) ? s.Substring(colonPos + 1).Trim() : "");
			}
			else if (su.StartsWith("OWNER:", StringComparison.Ordinal))
			{
			packageOwner = (s.Length >= (colonPos + 1) ? s.Substring(colonPos + 1).Trim() : "");
			}
			else if (su.StartsWith("DESCRIPTION:", StringComparison.Ordinal))
			{
			packageDescription = (s.Length >= (colonPos + 1) ? s.Substring(colonPos + 1).Trim() : "");
			}
			else if (su.StartsWith("CREATE_DATE:", StringComparison.Ordinal))
			{
			packageCreateDate = (s.Length >= (colonPos + 1) ? s.Substring(colonPos + 1).Trim() : "");
				try
				{
					// TODO SAM 2013-09-21 Looks like the date string can have single digits rather than zero padded
					// Need to add format to parse
				packageCreateDate_DateTime = parseRiverWareDateTime(packageCreateDate,DateTime.PRECISION_MINUTE);
				}
				catch (Exception)
				{
				packageCreateDate_DateTime = null;
				}
			}
			else if (su.StartsWith("NUMBER_OF_RUNS:", StringComparison.Ordinal))
			{
				s2 = (s.Length >= (colonPos + 1) ? s.Substring(colonPos + 1) : "");
				try
				{
				packageNumberOfRuns = int.Parse(s2);
				}
				catch (System.FormatException)
				{
					throw new IOException("number_of_runs (" + s2 + ") is not an integer.");
				}
			}
			else if (s.Equals("END_PACKAGE_PREAMBLE", StringComparison.OrdinalIgnoreCase))
			{
				// Break and continue reading run data below
				Message.printStatus(2, routine, "Detected END_PACKAGE_PREAMBLE at line " + lineCount);
				break;
			}
		}
		// Have read the package preamble ending with END_PACKAGE_PREAMBLE, now start on the runs
		for (int irun = 0; irun < packageNumberOfRuns; irun++)
		{
			Message.printStatus(2, routine, "Reading run [" + irun + "]");
			while (true)
			{
				++lineCount;
				s = @in.ReadLine();
				if (string.ReferenceEquals(s, null))
				{
					break;
				}
				// Parse run preamble strings, in order of documentation
				s = s.Trim();
				su = s.ToUpper();
				colonPos = s.IndexOf(":", StringComparison.Ordinal);
				if (su.StartsWith("START:", StringComparison.Ordinal))
				{
					runStart = (s.Length >= (colonPos + 1) ? s.Substring(colonPos + 1).Trim() : "");
				}
				else if (su.StartsWith("END:", StringComparison.Ordinal))
				{
					runEnd = (s.Length >= (colonPos + 1) ? s.Substring(colonPos + 1).Trim() : "");
				}
				else if (su.StartsWith("TIME_STEP_UNIT:", StringComparison.Ordinal))
				{
					runTimeStepUnit = (s.Length >= (colonPos + 1) ? s.Substring(colonPos + 1).Trim() : "");
				}
				else if (su.StartsWith("UNIT_QUANTITY:", StringComparison.Ordinal))
				{
					s2 = (s.Length >= (colonPos + 1) ? s.Substring(colonPos + 1).Trim() : "");
					try
					{
						runUnitQuantity = int.Parse(s2);
					}
					catch (System.FormatException)
					{
						throw new IOException("unit_quantity (" + s2 + ") is not an integer.");
					}
					if (runUnitQuantity == 1)
					{
						// Handles OK except for "week"
						try
						{
							runTimeStep_TimeInterval = TimeInterval.parseInterval(runTimeStepUnit);
						}
						catch (Exception)
						{
							throw new IOException("time_step_unit (" + runTimeStepUnit + ") is not recognized.");
						}
					}
					else
					{
						try
						{
							runTimeStep_TimeInterval = TimeInterval.parseInterval("" + runUnitQuantity + runTimeStepUnit);
						}
						catch (Exception)
						{
							throw new IOException("time_step_unit (" + runTimeStepUnit + ") and unit_quantity (" + runUnitQuantity + ") are not recognized.");
						}
					}
					intervalBase = runTimeStep_TimeInterval.getBase();
					intervalMult = runTimeStep_TimeInterval.getMultiplier();
					// Now know the interval so can get the start and end to the proper precision
					runStart_DateTime = parseRiverWareDateTime(runStart,runTimeStep_TimeInterval.getBase());
					runEnd_DateTime = parseRiverWareDateTime(runEnd,runTimeStep_TimeInterval.getBase());
				}
				else if (su.StartsWith("TIME_STEPS:", StringComparison.Ordinal))
				{
					s2 = (s.Length >= (colonPos + 1) ? s.Substring(colonPos + 1).Trim() : "");
					try
					{
						runTimeSteps = int.Parse(s2);
					}
					catch (System.FormatException)
					{
						throw new IOException("time_steps (" + s2 + ") is not an integer.");
					}
				}
				else if (su.StartsWith("SLOT_SET:", StringComparison.Ordinal))
				{
					runSlotSet = (s.Length >= (colonPos + 1) ? s.Substring(colonPos + 1).Trim() : "");
				}
				else if (su.StartsWith("RULE_SET:", StringComparison.Ordinal))
				{
					runRuleSet = (s.Length >= (colonPos + 1) ? s.Substring(colonPos + 1).Trim() : "");
				}
				else if (su.StartsWith("CONSECUTIVE:", StringComparison.Ordinal))
				{
					s2 = (s.Length >= (colonPos + 1) ? s.Substring(colonPos + 1).Trim() : "");
					try
					{
						runConsecutive = int.Parse(s2);
					}
					catch (System.FormatException)
					{
						throw new IOException("consecutive (" + s2 + ") is not an integer.");
					}
					if (runConsecutive == 1)
					{
						throw new IOException("Only consecutive=0 is currently supported.");
					}
				}
				else if (su.StartsWith("IDX_SEQUENTIAL:", StringComparison.Ordinal))
				{
					s2 = (s.Length >= (colonPos + 1) ? s.Substring(colonPos + 1).Trim() : "");
					try
					{
						runIdxSequential = int.Parse(s2);
					}
					catch (System.FormatException)
					{
						throw new IOException("idx_sequential (" + s2 + ") is not an integer.");
					}
				}
				else if (s.Equals("END_RUN_PREAMBLE", StringComparison.OrdinalIgnoreCase))
				{
					Message.printStatus(2, routine, "Detected END_RUN_PREAMBLE at line " + lineCount);
					break;
				}
			}
			// If here the run preamble has been read
			// Next read the dates for the run if time series or row numbers if a table
			// TODO SAM 2013-09-21 is there any need to keep these and use later?
			// If regular data the run start and end should match
			for (int idate = 0; idate < runTimeSteps; idate++)
			{
				++lineCount;
				s = @in.ReadLine();
				if ((idate == 0) && (s.IndexOf("-", StringComparison.Ordinal) > 0))
				{
					// Assume this is a time series with a date string
					// Make sure date matches the run start
					DateTime d1 = parseRiverWareDateTime(s.Trim(),runTimeStep_TimeInterval.getBase());
					if (!d1.Equals(runStart_DateTime))
					{
						throw new IOException("At line " + lineCount + " date/time does not match start date/time.");
					}
				}
				else if ((idate == (runTimeSteps - 1)) && (s.IndexOf("-", StringComparison.Ordinal) > 0))
				{
					// Assume this is a time series with a date string
					// Make sure date matches the run end
					DateTime d2 = parseRiverWareDateTime(s.Trim(),runTimeStep_TimeInterval.getBase());
					if (!d2.Equals(runEnd_DateTime))
					{
						throw new IOException("At line " + lineCount + " date/time does not match end date/time.");
					}
				}
			}
			Message.printStatus(2, routine, "Read last run date/time (or column row number) at line " + lineCount);
			// Read the slot data.  There is not property to indicate how many slots so have to loop until END_RUN indicates
			// that all slots are read for the run.
			bool readingSlotsForRun = true;
			while (readingSlotsForRun)
			{
				// These two while statements could probably be joined but as shown clearly indicate separate blocks of data
				Message.printStatus(2, routine, "Start reading slot data at line " + (lineCount + 1));
				while (true)
				{
					++lineCount;
					s = @in.ReadLine();
					if (string.ReferenceEquals(s, null))
					{
						// Premature end of file, break out of slot loop and let other code continue error handling
						readingSlotsForRun = false;
						break;
					}
					s = s.Trim();
					su = s.ToUpper();
					if (su.Equals("END_RUN"))
					{
						// Done processing slots for run - break out of while and go back to for loop for run
						Message.printStatus(2, routine, "Detected END_RUN at line " + lineCount);
						readingSlotsForRun = false;
						break;
					}
					colonPos = s.IndexOf(":", StringComparison.Ordinal);
					// Parse slot preamble strings, in order of documentation
					if (su.StartsWith("OBJECT_TYPE:", StringComparison.Ordinal))
					{
						slotObjectType = (s.Length >= (colonPos + 1) ? s.Substring(colonPos + 1).Trim() : "");
						Message.printStatus(2, routine, "Detected OBJECT_TYPE at line " + lineCount);
						// TODO SAM 2013-09-21 Maybe this indicates whether a time series or table?
					}
					else if (su.StartsWith("OBJECT_NAME:", StringComparison.Ordinal))
					{
						slotObjectName = (s.Length >= (colonPos + 1) ? s.Substring(colonPos + 1).Trim() : "");
						Message.printStatus(2, routine, "Detected OBJECT_NAME at line " + lineCount);
					}
					else if (su.StartsWith("SLOT_NAME:", StringComparison.Ordinal))
					{
						slotSlotName = (s.Length >= (colonPos + 1) ? s.Substring(colonPos + 1).Trim() : "");
						Message.printStatus(2, routine, "Detected SLOT_NAME at line " + lineCount);
					}
					else if (su.StartsWith("ROWS:", StringComparison.Ordinal))
					{
						// Indicates a table rather than time series
						slotIsTable = true;
						try
						{
							s2 = (s.Length >= (colonPos + 1) ? s.Substring(colonPos + 1).Trim() : "");
							slotRows = int.Parse(s2);
						}
						catch (System.FormatException)
						{
							throw new IOException("At line " + lineCount + " \"rows\" (" + s2 + ") is not an integer.");
						}
						Message.printStatus(2, routine, "Detected ROWS at line " + lineCount + " data object is table.  Will read but ignore.");
					}
					else if (su.StartsWith("COLS:", StringComparison.Ordinal))
					{
						// Used with table
						try
						{
							s2 = (s.Length >= (colonPos + 1) ? s.Substring(colonPos + 1).Trim() : "");
							slotCols = int.Parse(s2);
						}
						catch (System.FormatException)
						{
							throw new IOException("At line " + lineCount + " \"cols\" (" + s2 + ") is not an integer.");
						}
					}
					else if (su.Equals("END_SLOT_PREAMBLE"))
					{
						Message.printStatus(2, routine, "Detected END_SLOT_PREAMBLE at line " + lineCount);
						break;
					}
				}
				if (!readingSlotsForRun)
				{
					// Need to break out of this level also because END_RUN was detected
					break;
				}
				// If here the slot preamble has been read
				// Read the data for the slot
				if (slotIsTable)
				{
					// Reading a table
					// TODO SAM 2013-09-21 Need to actually handle table - for now just handle time series
					Message.printStatus(2, routine, "Start reading table at line " + lineCount);
					for (int irow = 0; irow < slotRows; irow++)
					{
						++lineCount;
						s = @in.ReadLine();
					}
					for (int icol = 0; icol < slotCols; icol++)
					{
						++lineCount;
						s = @in.ReadLine();
						colonPos = s.IndexOf(":", StringComparison.Ordinal);
						slotUnits = (s.Length >= (colonPos + 1) ? s.Substring(colonPos + 1).Trim() : "");
						++lineCount;
						s = @in.ReadLine();
						colonPos = s.IndexOf(":", StringComparison.Ordinal);
						s2 = (s.Length >= (colonPos + 1) ? s.Substring(colonPos + 1).Trim() : "");
						try
						{
							s2 = (s.Length >= (colonPos + 1) ? s.Substring(colonPos + 1).Trim() : "");
							slotScale = double.Parse(s2);
						}
						catch (System.FormatException)
						{
							throw new IOException("At line " + lineCount + " \"slot_scale\" (" + s2 + ") is not a number.");
						}
						for (int irow = 0; irow < slotRows; irow++)
						{
							++lineCount;
							s = @in.ReadLine();
						}
					}
				}
				else
				{
					// Reading a time series, one value per dates that were read in the run preamble
					Message.printStatus(2, routine, "Slot is time series.  Starting to read at line " + (lineCount + 1));
					++lineCount;
					s = @in.ReadLine();
					colonPos = s.IndexOf(":", StringComparison.Ordinal);
					slotUnits = (s.Length >= (colonPos + 1) ? s.Substring(colonPos + 1).Trim() : "");
					++lineCount;
					s = @in.ReadLine();
					colonPos = s.IndexOf(":", StringComparison.Ordinal);
					s2 = (s.Length >= (colonPos + 1) ? s.Substring(colonPos + 1).Trim() : "");
					try
					{
						slotScale = double.Parse(s2);
					}
					catch (System.FormatException)
					{
						throw new IOException("At line " + lineCount + " \"slot_scale\" (" + s2 + ") is not a number.");
					}
					// Create the time series
					string tsid = slotObjectName + ".RiverWare." + slotSlotName + "." + runTimeStep_TimeInterval;
					DateTime fileStart = new DateTime(runStart_DateTime);
					DateTime fileEnd = new DateTime(runEnd_DateTime);
					if (((runConsecutive == 0) && (packageNumberOfRuns > 1)) || (runIdxSequential == 1))
					{
						// runConsecutive=0 means that a single run was done over the period start to end (?)
						// runIdxSequential means that overlapping runs were made, with resequenced historical input
						// The run dates are already overlapping.  Set the sequence number to the year of the date for the run.
						// TODO SAM 2013-09-21 Are the index sequential years truly sequential or can they be mixed?
						//int sequenceNum = fileStart.getYear() + irun;
						// TODO SAM what is the unique identifier for the sequence number?  Year of historical trace, some other
						// metadata?
						int sequenceNum = irun + 1;
						tsid = tsid + TSIdent.SEQUENCE_NUMBER_LEFT + sequenceNum + TSIdent.SEQUENCE_NUMBER_RIGHT;
					}
					Message.printStatus(2, routine, "Creating time series \"" + tsid + "\"");
					try
					{
						ts = TSUtil.newTimeSeries(tsid, true);
					}
					catch (Exception e)
					{
						throw new IOException("Error creating time series using TSID \"" + tsid + "\" uniquetempvar.");
					}
					try
					{
						ts.setIdentifier(tsid);
					}
					catch (Exception e)
					{
						throw new IOException("Error setting time seriies identifier \"" + tsid + "\" uniquetempvar.");
					}
					if (readStart != null)
					{
						ts.setDate1(readStart);
					}
					else
					{
						ts.setDate1(fileStart);
					}
					if (readEnd != null)
					{
						ts.setDate2(readEnd);
					}
					else
					{
						ts.setDate2(fileEnd);
					}
					ts.setDate1Original(fileStart);
					ts.setDate2Original(fileEnd);
					ts.setDataUnits(slotUnits);
					ts.setDataUnitsOriginal(slotUnits);
					// Set all the properties (some of these also will be used for the ensemble if ensembles are read
					ts.setProperty("PackageName", packageName);
					ts.setProperty("PackageOwner", packageOwner);
					ts.setProperty("PackageDescription", packageDescription);
					ts.setProperty("PackageCreateDate", packageCreateDate_DateTime);
					ts.setProperty("PackageNumberOfRuns", new int?(packageNumberOfRuns));
					ts.setProperty("RunConsecutive", new int?(runConsecutive));
					ts.setProperty("RunIdxSequential", new int?(runIdxSequential));
					ts.setProperty("RunSlotSet", runSlotSet);
					ts.setProperty("RunRuleSet", runRuleSet);
					ts.setProperty("SlotObjectType", slotObjectType);
					ts.setProperty("SlotObjectName", slotObjectName);
					ts.setProperty("SlotSlotName", slotSlotName);
					ts.getIdentifier().setInputType("RiverWare");
					ts.getIdentifier().setInputName(filename);
					if (readData)
					{
						ts.allocateDataSpace();
					}
					tslist.Add(ts);
					// Use the file date to read through data but time series will only have data within period
					DateTime date = new DateTime(fileStart);
					for (int istep = 0; istep < runTimeSteps; istep++)
					{
						++lineCount;
						s = @in.ReadLine().Trim();
						if (readData)
						{
							if (s.Equals("NaN", StringComparison.OrdinalIgnoreCase))
							{
								// Missing value.  Don't need to set anything
							}
							else
							{
								// Parse the value
								value = double.Parse(s);
								//Message.printStatus(2,routine,"Setting " + date + " " + value );
								ts.setDataValue(date, slotScale * value);
							}
						}
						date.addInterval(intervalBase,intervalMult);
					}
					Message.printStatus(2, routine, "Read last slot time series value at line " + lineCount);
				}
				// Read and check for expected end of data
				++lineCount;
				s = @in.ReadLine();
				if (!s.ToUpper().Equals("END_COLUMN"))
				{
					throw new IOException("At line " + lineCount + " expecting END_COLUMN, have: " + s);
				}
				Message.printStatus(2, routine, "Detected END_COLUMN at line " + lineCount);
				++lineCount;
				s = @in.ReadLine();
				if (!s.ToUpper().Equals("END_SLOT"))
				{
					throw new IOException("At line " + lineCount + " expecting END_SLOT, have: " + s);
				}
				Message.printStatus(2, routine, "Detected END_SLOT at line " + lineCount);
				// At top of loop will look for END_RUN and if that is not found, another slot for the run will be read
			} // Loop on slots in run
		} // Loop on runs
		Message.printStatus(2, routine, "Processed " + lineCount + " lines");
		return tslist;
	}

	/// <summary>
	/// Write a RiverWare time series to the open PrintWriter. </summary>
	/// <param name="ts"> Time series to write. </param>
	/// <param name="fp"> PrintWrite to write to. </param>
	/// <exception cref="IOException"> if there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void writeTimeSeries(TS ts, java.io.PrintWriter fp) throws java.io.IOException
	private static void writeTimeSeries(TS ts, PrintWriter fp)
	{
		writeTimeSeries(ts, fp, (DateTime)null, (DateTime)null, (PropList)null, true);
	}

	/// <summary>
	/// Write a time series to a RiverWare format file.  The entire period is written. </summary>
	/// <param name="ts"> Single time series to write. </param>
	/// <param name="fname"> Name of file to write. </param>
	/// <exception cref="IOException"> if there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeTimeSeries(TS ts, String fname) throws java.io.IOException
	public static void writeTimeSeries(TS ts, string fname)
	{
		PrintWriter @out = null;

		string full_fname = IOUtil.getPathUsingWorkingDir(fname);
		try
		{
			@out = new PrintWriter(new StreamWriter(full_fname));
		}
		catch (Exception)
		{
			string message = "Error opening \"" + full_fname + "\" for writing.";
			Message.printWarning(3, "RiverWareTS.writePersistent(TS,String)", message);
			throw new IOException(message);
		}
		try
		{
			writeTimeSeries(ts, @out);
		}
		catch (IOException e)
		{
			// Rethrow
			throw e;
		}
		finally
		{
			@out.close();
		}
	}

	/// <summary>
	/// Write a time series to a RiverWare format file using a default format based on the time series units. </summary>
	/// <param name="ts"> Vector of pointers to time series to write. </param>
	/// <param name="fname"> Name of file to write. </param>
	/// <param name="req_date1"> First date to write (if NULL write the entire time series). </param>
	/// <param name="req_date2"> Last date to write (if NULL write the entire time series). </param>
	/// <param name="req_units"> Units to write.  If different than the current units the units
	/// will be converted on output. </param>
	/// <param name="scale"> Scale to divide values by for output.  Ignored if <= 0.0. </param>
	/// <param name="set_units"> RiverWare "set_units" parameter.  If empty or null will not be written. </param>
	/// <param name="set_scale"> RiverWare "set_scale" parameter.  If zero or negative will not be written. </param>
	/// <param name="write_data"> Indicates whether data should be written (as opposed to only
	/// writing the header) (<b>currently not used</b>). </param>
	/// <exception cref="IOException"> if there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeTimeSeries(TS ts, String fname, RTi.Util.Time.DateTime req_date1, RTi.Util.Time.DateTime req_date2, String req_units, double scale, String set_units, double set_scale, boolean write_data) throws java.io.IOException
	public static void writeTimeSeries(TS ts, string fname, DateTime req_date1, DateTime req_date2, string req_units, double scale, string set_units, double set_scale, bool write_data)
	{
		PropList props = new PropList("RiverWare");
		if (!string.ReferenceEquals(req_units, null))
		{
			props.set("Units", req_units);
		}
		if (scale >= 0.0)
		{
			props.set("Scale", "" + scale);
		}
		if (!string.ReferenceEquals(set_units, null))
		{
			props.set("SetUnits", set_units);
		}
		if (set_scale >= 0.0)
		{
			props.set("SetScale", "" + set_scale);
		}
		writeTimeSeries(ts, fname, req_date1, req_date2, props, write_data);
	}

	/// <summary>
	/// Write a time series to a RiverWare format file using a default format based on the time series units. </summary>
	/// <param name="ts"> Vector of pointers to time series to write. </param>
	/// <param name="fname"> Name of file to write. </param>
	/// <param name="req_date1"> First date to write (if NULL write the entire time series). </param>
	/// <param name="req_date2"> Last date to write (if NULL write the entire time series). </param>
	/// <param name="props"> Properties to control the write.  The following properties are
	/// supported:
	/// <table width=100% cellpadding=10 cellspacing=0 border=2>
	/// <tr>
	/// <td><b>Property</b></td>	<td><b>Description</b></td>	<td><b>Default</b></td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>Scale</b></td>
	/// <td>The factor to divide values by for output.
	/// </td>
	/// <td>Do not divide (scale of 1).</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>SetUnits</b></td>
	/// <td>The RiverWare "set_units" parameter.  If empty or null will not be written.
	/// </td>
	/// <td></td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>SetScale</b></td>
	/// <td>The RiverWare "set_scale" parameter.  If empty or null will not be written.
	/// </td>
	/// <td></td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>Units</b></td>
	/// <td>The Units to write to the RiverWare header.  The data values are NOT
	/// converted.
	/// </td>
	/// <td></td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>Precision</b></td>
	/// <td>The precision (number of digits after decimal) to use when writing data
	/// values.
	/// </td>
	/// <td></td>
	/// </tr>
	/// 
	/// </table> </param>
	/// <param name="write_data"> Indicates whether data should be written (as opposed to only
	/// writing the header) (<b>currently not used</b>). </param>
	/// <exception cref="IOException"> if there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeTimeSeries(TS ts, String fname, RTi.Util.Time.DateTime req_date1, RTi.Util.Time.DateTime req_date2, RTi.Util.IO.PropList props, boolean write_data) throws java.io.IOException
	public static void writeTimeSeries(TS ts, string fname, DateTime req_date1, DateTime req_date2, PropList props, bool write_data)
	{
		PrintWriter @out = null;

		string full_fname = IOUtil.getPathUsingWorkingDir(fname);
		try
		{
			@out = new PrintWriter(new StreamWriter(full_fname));
		}
		catch (Exception)
		{
			string message = "Error opening \"" + full_fname + "\" for writing.";
			Message.printWarning(3,"RiverWareTS.writeTimeSeries",message);
			throw new IOException(message);
		}
		writeTimeSeries(ts, @out, req_date1, req_date2, props, write_data);
		@out.close();
	}

	/// <summary>
	/// Write a time series to a RiverWare format file using a default format based on the data units. </summary>
	/// <param name="ts"> Time series to write. </param>
	/// <param name="fp"> PrintWriter to write to. </param>
	/// <param name="req_date1"> First date to write (if NULL write the entire time series). </param>
	/// <param name="req_date2"> Last date to write (if NULL write the entire time series). </param>
	/// <param name="req_units"> Units to write.  If different than the current units the units
	/// will be converted on output.  This method does not support set_units. </param>
	/// <param name="scale"> Scale to divide values by for output. </param>
	/// <param name="set_units"> RiverWare "set_units" parameter.  If empty or null will not be written. </param>
	/// <param name="set_scale"> RiverWare "set_scale" parameter.  If zero or negative will not be written. </param>
	/// <param name="write_data"> Indicates whether data should be written (if false only the
	/// header is written). </param>
	/// <param name="props"> Properties to control output, as follows:
	/// <table width=100% cellpadding=10 cellspacing=0 border=2>
	/// <tr>
	/// <td><b>Property</b></td> <td><b>Description</b></td> <td><b>Default</b></td>
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
	/// <td><b>WriteHeaderComments</b></td>
	/// <td><b>Whether to write header comments.</b>
	/// <td>Write header comments.</td>
	/// </tr>
	/// </table> </param>
	/// <exception cref="IOException"> if there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void writeTimeSeries(TS ts, java.io.PrintWriter fp, RTi.Util.Time.DateTime req_date1, RTi.Util.Time.DateTime req_date2, RTi.Util.IO.PropList props, boolean write_data) throws java.io.IOException
	private static void writeTimeSeries(TS ts, PrintWriter fp, DateTime req_date1, DateTime req_date2, PropList props, bool write_data)
	{
		string message, routine = "RiverWareTS.writeTimeSeries";

		if (ts == null)
		{
			message = "Time series is NULL, cannot continue.";
			Message.printWarning(3, routine, message);
			throw new IOException(message);
		}

		if (fp == null)
		{
			message = "Output stream is NULL, cannot continue.";
			Message.printWarning(3, routine, message);
			throw new IOException(message);
		}

		// Get the interval information to facilitate use below...

		int data_interval_base = ts.getDataIntervalBase();
		int data_interval_mult = ts.getDataIntervalMult();

		// Get the dates for output...

		DateTime date1 = null;
		if (req_date1 == null)
		{
			date1 = new DateTime(ts.getDate1());
		}
		else
		{
			date1 = new DateTime(req_date1);
			// Make sure the precision is that of the data...
			date1.setPrecision(data_interval_base);
		}
		DateTime date2 = null;
		if (req_date2 == null)
		{
			date2 = new DateTime(ts.getDate2());
		}
		else
		{
			date2 = new DateTime(req_date2);
			// Make sure the precision is that of the data...
			date2.setPrecision(data_interval_base);
		}

		if (props == null)
		{
			props = new PropList("RiverWare");
		}
		bool writeHeaderComments = true; // Default is to write header comments
		string propVal = props.getValue("WriteHeaderComments");
		if ((!string.ReferenceEquals(propVal, null)) && propVal.Equals("False", StringComparison.OrdinalIgnoreCase))
		{
			writeHeaderComments = false;
		}

		// Write header...

		if (writeHeaderComments)
		{
			fp.println("#");
			IOUtil.printCreatorHeader(fp, "#", 80, 0);
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
						fp.println("# " + comments[iComment]);
					}
				}
			}
			fp.println("#");
			fp.println("# RiverWare Time Series File");
			fp.println("#");
		}
		if (data_interval_base == TimeInterval.HOUR)
		{
			// Adjust the internal 0-23 clock to the 0-24 RiverWare clock...
			if (date1.getHour() == 0)
			{
				// Set to hour 24 of the previous day...
				DateTime d = new DateTime(date1);
				d.addDay(-1);
				fp.println("start_date: " + d.ToString(DateTime.FORMAT_YYYY_MM_DD) + " 24:00");
			}
			else
			{
				// OK to write the date/time as is with hour and minute...
				fp.println("start_date: " + date1.ToString(DateTime.FORMAT_YYYY_MM_DD_HH_mm));
			}
			if (date2.getHour() == 0)
			{
				// Set to hour 24 of the previous day...
				DateTime d = new DateTime(date2);
				d.addDay(-1);
				fp.println("end_date: " + d.ToString(DateTime.FORMAT_YYYY_MM_DD) + " 24:00");
			}
			else
			{
				// OK to write the date/time as is with hour and minute...
				fp.println("end_date: " + date2.ToString(DateTime.FORMAT_YYYY_MM_DD_HH_mm));
			}
		}
		else if (data_interval_base == TimeInterval.DAY)
		{
			// Use the in-memory day but always add 24:00 at the end...
			fp.println("start_date: " + date1.ToString(DateTime.FORMAT_YYYY_MM_DD) + " 24:00");
			fp.println("end_date: " + date2.ToString(DateTime.FORMAT_YYYY_MM_DD) + " 24:00");
		}
		else if (data_interval_base == TimeInterval.MONTH)
		{
			// Use the in-memory day but always add 24:00 at the end...
			// Set the day to the number of days in the month.  The day
			// will be ignored below during iteration because the precision
			// was set above to month.
			date1.setDay(TimeUtil.numDaysInMonth(date1));
			fp.println("start_date: " + date1.ToString(DateTime.FORMAT_YYYY_MM_DD) + " 24:00");
			date2.setDay(TimeUtil.numDaysInMonth(date2));
			fp.println("end_date: " + date2.ToString(DateTime.FORMAT_YYYY_MM_DD) + " 24:00");
		}
		else if (data_interval_base == TimeInterval.YEAR)
		{
			// Use the in-memory day but always add 24:00 at the end...
			// Set the month and day to the end of the year.  The month and
			// day will be ignored below during iteration because the
			// precision was set above to month.
			date1.setMonth(12);
			date1.setDay(31);
			fp.println("start_date: " + date1.ToString(DateTime.FORMAT_YYYY_MM_DD) + " 24:00");
			date2.setMonth(12);
			date2.setDay(31);
			fp.println("end_date: " + date2.ToString(DateTime.FORMAT_YYYY_MM_DD) + " 24:00");
		}
		else
		{
			// Interval is not supported...
			throw new IOException("Interval for \"" + ts.getIdentifier() + "\" is not supported for RiverWare.");
		}
		// Print the interval, with multiplier, if provided...
		try
		{
			TimeInterval interval = TimeInterval.parseInterval(ts.getIdentifier().getInterval());
			if (interval.getMultiplierString().Equals(""))
			{
				fp.println("timestep: 1 " + interval.getBaseString());
			}
			else
			{
				fp.println("timestep: " + interval.getMultiplierString() + " " + interval.getBaseString());
			}
		}
		catch (Exception)
		{
			; // Ignore for now
		}
		string Units = props.getValue("Units");
		if ((!string.ReferenceEquals(Units, null)) && (Units.Length > 0))
		{
			fp.println("units: " + Units);
		}
		else
		{
			fp.println("units: " + ts.getDataUnits());
		}
		string Scale = props.getValue("Scale");
		double Scale_double = 1.0;
		if ((string.ReferenceEquals(Scale, null)) || (Scale.Length == 0))
		{
			Scale = "1"; // Default
		}
		else if (StringUtil.isDouble(Scale))
		{
			Scale_double = Convert.ToDouble(Scale);
		}
		fp.println("scale: " + Scale);
		string SetUnits = props.getValue("SetUnits");
		if ((!string.ReferenceEquals(SetUnits, null)) && !SetUnits.Equals(""))
		{
			fp.println("set_units: " + SetUnits);
		}
		string SetScale = props.getValue("SetScale");
		if ((!string.ReferenceEquals(SetScale, null)) && StringUtil.isDouble(SetScale))
		{
			fp.println("set_scale: " + SetScale);
		}
		string Precision = props.getValue("Precision");
		if (string.ReferenceEquals(Precision, null))
		{
			Precision = "4"; // Default
		}
		string format = "%." + Precision + "f";

		DateTime date = new DateTime(date1);
		double value;
		for (; date.lessThanOrEqualTo(date2); date.addInterval(data_interval_base,data_interval_mult))
		{
			value = ts.getDataValue(date);
			if (ts.isDataMissing(value))
			{
				fp.println("NaN");
			}
			else
			{
				fp.println(StringUtil.formatString(value / Scale_double, format));
			}
		}
	}

	}

}