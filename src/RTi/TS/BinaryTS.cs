using System;
using System.Text;

// BinaryTS - store time series in a binary file

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
// BinaryTS - store time series in a binary file
// ----------------------------------------------------------------------------
// History:
//
// 17 Jul 2000	Steven A. Malers, RTi	Initial version (format version
//					01.00.00).
// 12 Dec 2000	SAM, RTi		Change getNumTS() to size().
// 18 Dec 2000	SAM, RTi		Add readTimeSeries(), indexOf().
//					Add alias to the time series header.
//					Alphabetize some methods that were out
//					of order.
// 24 Feb 2001	SAM, RTi		Add flag to indexOf() to allow search
//					in either direction.  Change
//					getTSIdent() to getIdentifier() to be
//					consistent with other TS code.
// 31 Aug 2001	SAM, RTi		Tracking down error.  Get rid of
//					static data to save memory.  Fix header
//					size to be 560, not 80 (need to include
//					alias).  The effect was that the
//					last time series header to be written
//					clobbered the first few data values in
//					the first time series.
// 2001-11-06	SAM, RTi		Review javadoc.  Verify that variables
//					are set to null when no longer used.
// 2003-06-02	SAM, RTi		Upgrade to use generic classes.
//					* Change TSDate to DateTime.
//					* Change TS.INTERVAL* to TimeInterval.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------
// EndHeader

namespace RTi.TS
{

	using Message = RTi.Util.Message.Message;
	using DateTime = RTi.Util.Time.DateTime;
	using TimeInterval = RTi.Util.Time.TimeInterval;

	/// <summary>
	/// Provide an interface to a binary time series file.  This file is used to
	/// store large amounts of time series data for reshuffling on output.  For example,
	/// the output may print a month of data for each time series.  The binary file
	/// allows a large amount of data to be stored and accessed outside of memory.
	/// Currently, hourly, daily, and monthly data are supported.  All time series to
	/// be managed by the binary file must be of the same interval.  The data storage
	/// is currently as follows (characters are stored as 2-byte Unicode in the file):
	/// <pre>
	/// MainHeader                                              Size    Cumulative Size
	///        Version         char-40                         80      80
	///        IntervalBase    int                             4       84
	///        IntervalMult    int                             4       88
	///        Date1 (YYYY MM DD hh mm ss) 6 ints              24      112
	///        Date2 (YYYY MM DD hh mm ss) 6 ints              24      136
	/// TimeSeriesHeader (one record per time series)
	///        HasHeader (time series header is available) int 4       4
	///        HasData (data are available) int                4       8
	///        TS Header - data from time series header
	///                TSIdent char-80                         160     168
	///                Date1 (YYYY MM DD hh mm ss) 6 ints      24      192
	///                Date2 (YYYY MM DD hh mm ss) 6 ints      24      216
	///                Description char-80                     160     376
	///                Units char-12                           24      400
	///                Alias char-80                           160     560
	///        ...
	/// DataBlock (currently one block per month with size depending on interval)
	///        TS1 Month1
	///        TS2 Month1
	///        TS3 Month1
	///        ...
	///        TS1 Month2
	///        TS2 Month2
	///        TS3 Month2
	///        ...
	/// </pre>
	/// All the methods in this class that use time series index numbers use 0 for the
	/// first time series.
	/// </summary>
	public class BinaryTS
	{

	private readonly int PARAMETER_TS_DESCRIPTION = 1;
	private readonly int PARAMETER_TS_HAS_DATA = 2;
	private readonly int PARAMETER_TS_HAS_HEADER = 3;
	private readonly int PARAMETER_TS_IDENT = 4;
	private readonly int PARAMETER_TS_UNITS = 5;
	private readonly int PARAMETER_TS_ALIAS = 6;

	private readonly string _version = "01.01.00 (2000-12-18)";
							// File format version.

	// Data members...

	private RandomAccessFile _fp; // Pointer to random access
							// file.
	private long _header_byte; // Byte position of main header
							// (currently 0).
	private long _header_size; // Size of main header (bytes).

	private long _ts_header_byte; // Byte position of first TS
							// header (after the main
							// header).
	private long _ts_header_size; // Size of one ts header
							// (bytes).

	private long _ts_data_size; // Size of one block (month) of
							// data for one time series.
	private double _missing; // Missing data value.
	private DateTime _date1; // Start of period.
	private DateTime _date2; // End of period.
	private int _interval_base; // Time series interval base.
	private int _interval_mult; // Time series interval
							// multiplier.
	private string _tsfile; // Binary time series file.
	private int _nts; // Number of time series in
							// file.
	private int _amon1; // Absolute months of period.

	/// <summary>
	/// Construct and initialize a binary time series file. </summary>
	/// <param name="tsfile"> Name of binary file to write. </param>
	/// <param name="nts"> Number of time series to be in file. </param>
	/// <param name="interval_base"> Time series interval base (see TimeInterval.*).  Currently
	/// only monthly, daily, and hourly data are supported. </param>
	/// <param name="interval_mult"> Time series interval multiplier. </param>
	/// <param name="date1"> First date in period to be stored in binary file. </param>
	/// <param name="date2"> Last date in period to be stored in binary file. </param>
	/// <param name="mode"> "r" for reading or "rw" for read/write. </param>
	/// <param name="write_header"> Indicates if header should be written.  Use true when the
	/// file is being created. </param>
	/// <exception cref="IOException"> if unabled to create the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public BinaryTS(String tsfile, int nts, int interval_base, int interval_mult, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2, String mode, boolean write_header) throws java.io.IOException
	public BinaryTS(string tsfile, int nts, int interval_base, int interval_mult, DateTime date1, DateTime date2, string mode, bool write_header)
	{
		initialize(tsfile, nts, interval_base, interval_mult, date1, date2, mode, write_header);
	}

	/// <summary>
	/// Close the BinaryTS file. </summary>
	/// <exception cref="IOException"> if there is an error closing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void close() throws java.io.IOException
	public virtual void close()
	{
		_fp.close();
	}

	/// <summary>
	/// Calculate the file position for any data value.  This DOES NOT position the
	/// file pointer! </summary>
	/// <param name="parameter"> Parameter to find (see PARAMETER_*). </param>
	/// <param name="its"> Time series index. </param>
	/// <returns> byte position in file for requested parameter. </returns>
	internal virtual long calculatePosition(int parameter, int its)
	{
		long pos = -1;
		// Order in header is:
		// Data		Bytes	Cumulative
		// HAS_HEADER	4	4
		// HAS_DATA	4	8
		// IDENT	160	168
		// DATE1	24	192
		// DATE2	24	216
		// DESCRIPTION	160	376
		// UNITS	24	400
		// ALIAS	80	560
		if (parameter == PARAMETER_TS_ALIAS)
		{
			// Skip over HAS_HEADER, HAS_DATA, IDENT, dates, DESCRIPTION,
			// units...
			pos = _ts_header_byte + its * _ts_header_size + 400;
			if (Message.isDebugOn)
			{
				Message.printDebug(1, "BinaryTS.calculatePosition", "Alias position [" + its + "] is " + pos);
			}
		}
		else if (parameter == PARAMETER_TS_DESCRIPTION)
		{
			// Skip over HAS_HEADER, HAS_DATA, IDENT, and dates...
			pos = _ts_header_byte + its * _ts_header_size + 216;
			if (Message.isDebugOn)
			{
				Message.printDebug(1, "BinaryTS.calculatePosition", "Description position [" + its + "] is " + pos);
			}
		}
		else if (parameter == PARAMETER_TS_HAS_DATA)
		{
			// Skip over HAS_HEADER...
			pos = _ts_header_byte + its * _ts_header_size + 4;
			if (Message.isDebugOn)
			{
				Message.printDebug(1, "BinaryTS.calculatePosition", "Has-data position [" + its + "] is " + pos);
			}
		}
		else if (parameter == PARAMETER_TS_HAS_HEADER)
		{
			pos = _ts_header_byte + its * _ts_header_size;
			if (Message.isDebugOn)
			{
				Message.printDebug(1, "BinaryTS.calculatePosition", "Has-header position [" + its + "] is " + pos);
			}
		}
		else if (parameter == PARAMETER_TS_IDENT)
		{
			// Skip over HAS_HEADER and HAS_DATA...
			pos = _ts_header_byte + its * _ts_header_size + 8;
			if (Message.isDebugOn)
			{
				Message.printDebug(1, "BinaryTS.calculatePosition", "Ident position [" + its + "] is " + pos);
			}
		}
		else if (parameter == PARAMETER_TS_UNITS)
		{
			// Skip over HAS_HEADER, HAS_DATA, IDENT, dates, and
			// DESCRIPTION...
			pos = _ts_header_byte + its * _ts_header_size + 376;
			if (Message.isDebugOn)
			{
				Message.printDebug(1, "BinaryTS.calculatePosition", "Units position [" + its + "] is " + pos);
			}
		}
		return pos;
	}

	/// <summary>
	/// Close and delete the BinaryTS file.  The calling code should set the instance
	/// to null. </summary>
	/// <exception cref="IOException"> if there is an error closing the file. </exception>
	/// <exception cref="SecurityException"> if there is an error deleting the file because of
	/// a security problem. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void delete() throws SecurityException, java.io.IOException
	public virtual void delete()
	{
		close();
		File fp = new File(_tsfile);
		fp.delete();
		fp = null;
	}

	/// <summary>
	/// Finalize before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~BinaryTS()
	{
		_date1 = null;
		_date2 = null;
		_fp.close();
		_fp = null;
		_tsfile = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the alias for the requested time series. </summary>
	/// <param name="its"> The time series index. </param>
	/// <exception cref="IOException"> if there is an error reading the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String getAlias(int its) throws java.io.IOException
	public virtual string getAlias(int its)
	{
		long pos = calculatePosition(PARAMETER_TS_ALIAS, its);
		_fp.seek(pos);
		return readString(pos, 80);
	}

	/// <summary>
	/// Return the interval base. </summary>
	/// <returns> the data interval base. </returns>
	public virtual int getDataIntervalBase()
	{
		return _interval_base;
	}

	/// <summary>
	/// Return the interval multiplier. </summary>
	/// <returns> the data interval multiplier. </returns>
	public virtual int getDataIntervalMult()
	{
		return _interval_mult;
	}

	/// <summary>
	/// Get the position for a data value to read.  This DOES NOT position the file
	/// pointer. </summary>
	/// <param name="its"> Time series to process. </param>
	/// <param name="date"> Date to process. </param>
	/// <returns> the data position or -1 if outside the data range. </returns>
	public virtual long getDataPosition(int its, DateTime date)
	{
		int amon = date.getAbsoluteMonth();
		// Initialize byte position in file to the start of the data...
		long pos = _header_byte + _header_size + _nts * _ts_header_size;
		// Skip over data from all previous months, all stations.
		pos += (amon - _amon1) * _nts * _ts_data_size;
		// Add data for previous stations in current month...
		pos += its * _ts_data_size;
		// Now add based on the interval...
		if (_interval_base == TimeInterval.MONTH)
		{
			// Already at the right position
			;
		}
		else if (_interval_base == TimeInterval.DAY)
		{
			pos += (date.getDay() - 1) * 8;
		}
		else if (_interval_base == TimeInterval.HOUR)
		{
			pos += (((date.getDay() - 1) * 24 + date.getHour()) / _interval_mult) * 8;
		}
		return pos; // Double precision
	}

	/// <summary>
	/// Return the data units for the requested time series. </summary>
	/// <param name="its"> The time series index. </param>
	/// <exception cref="IOException"> if there is an error reading the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String getDataUnits(int its) throws java.io.IOException
	public virtual string getDataUnits(int its)
	{
		long pos = calculatePosition(PARAMETER_TS_UNITS, its);
		_fp.seek(pos);
		return readString(pos, 12);
	}

	/// <summary>
	/// Returns a data value for a date. </summary>
	/// <param name="its"> Time series to read from. </param>
	/// <param name="date"> Date to get data for. </param>
	/// <returns> data value for a date or the missing data value. </returns>
	/// <exception cref="IOException"> If there is an error reading the data value. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double getDataValue(int its, RTi.Util.Time.DateTime date) throws java.io.IOException
	public virtual double getDataValue(int its, DateTime date)
	{
		// Check the date coming in...

		if (date == null)
		{
			return _missing;
		}

		if ((date.lessThan(_date1)) || (date.greaterThan(_date2)))
		{
			return _missing;
		}

		// Calculate the data position.  This should be safe to call since we
		// checked dates above.

		long pos = getDataPosition(its, date);
		_fp.seek(pos);
		double value = _fp.readDouble();
		//Message.printStatus ( 1, "BinaryTS.setDataValue",
		//"Got " + value + " for its[" + its + "] on " + date );
		return value;
	}

	/// <summary>
	/// Return the first date in the period. </summary>
	/// <returns> the first date in the period. </returns>
	public virtual DateTime getDate1()
	{
		return _date1;
	}

	/// <summary>
	/// Return the last date in the period. </summary>
	/// <returns> the last date in the period. </returns>
	public virtual DateTime getDate2()
	{
		return _date2;
	}

	/// <summary>
	/// Return the description for the requested time series. </summary>
	/// <param name="its"> The time series index. </param>
	/// <exception cref="IOException"> if there is an error reading the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String getDescription(int its) throws java.io.IOException
	public virtual string getDescription(int its)
	{
		long pos = calculatePosition(PARAMETER_TS_DESCRIPTION, its);
		_fp.seek(pos);
		return readString(pos, 80);
	}

	/// <summary>
	/// Return the TSIdent for the requested time series. </summary>
	/// <param name="its"> The time series index. </param>
	/// <exception cref="Exception"> if there is an error reading the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TSIdent getIdentifier(int its) throws Exception
	public virtual TSIdent getIdentifier(int its)
	{
		long pos = calculatePosition(PARAMETER_TS_IDENT, its);
		_fp.seek(pos);
		return new TSIdent(readString(pos, 80));
	}

	/// <summary>
	/// Return the TSIdent for the requested time series. </summary>
	/// <param name="its"> The time series index. </param>
	/// <exception cref="Exception"> if there is an error reading the file. </exception>
	/// @deprecated Use getIdentifier. 
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TSIdent getTSIdent(int its) throws Exception
	public virtual TSIdent getTSIdent(int its)
	{
		return getIdentifier(its);
	}

	/// <summary>
	/// Determine whether data are available for a time series. </summary>
	/// <param name="its"> Time series of interest. </param>
	public virtual bool hasData(int its)
	{ // Read the flag for the header...
		long pos = calculatePosition(PARAMETER_TS_HAS_DATA, its);
		try
		{
			_fp.seek(pos);
			int i = _fp.readInt();
			if (i == 0)
			{
				return false;
			}
			else
			{
				return true;
			}
		}
		catch (Exception)
		{
			return false;
		}
	}

	/// <summary>
	/// Determine whether the header is available for a time series. </summary>
	/// <param name="its"> Time series of interest. </param>
	public virtual bool hasHeader(int its)
	{ // Read the flag for the header...
		long pos = calculatePosition(PARAMETER_TS_HAS_HEADER, its);
		try
		{
			_fp.seek(pos);
			int i = _fp.readInt();
			if (i == 0)
			{
				return false;
			}
			else
			{
				return true;
			}
		}
		catch (Exception)
		{
			return false;
		}
	}

	/// <summary>
	/// Find a time series in the BinaryTS file.  The indicated field is searched and a
	/// case-insensitive query is made. </summary>
	/// <param name="id"> String identifier to match. </param>
	/// <param name="field"> Field to match (currently can only be "Alias").
	/// This may be changed to defined field names for time series data values. </param>
	/// <param name="direction"> If &lt;= 0, the direction of the search will be forward.  
	/// If &lt; 0, the direction of search will be backwards. </param>
	/// <returns> the Vector position of the match or -1 if no match or the field is not
	/// recognized. </returns>
	public virtual int indexOf(string id, string field, int direction)
	{
		try
		{
			int ifield = 1;
			if (field.Equals("Alias", StringComparison.OrdinalIgnoreCase))
			{
				ifield = 1;
			}
			if ((ifield != 1) || (ifield != 2))
			{
				return -1;
			}
			if (direction >= 0)
			{
				for (int i = 0; i < _nts; i++)
				{
					if (ifield == 1)
					{
						if (id.Equals(getAlias(i), StringComparison.OrdinalIgnoreCase))
						{
							return i;
						}
					}
				}
			}
			else
			{
				for (int i = (_nts - 1); i >= 0; i--)
				{
					if (ifield == 1)
					{
						if (id.Equals(getAlias(i), StringComparison.OrdinalIgnoreCase))
						{
							return i;
						}
					}
				}
			}
			return -1;
		}
		catch (Exception)
		{
			return -1;
		}
	}

	/// <summary>
	/// Initialize the data. </summary>
	/// <param name="tsfile"> Name of binary file to write. </param>
	/// <param name="nts"> Number of time series to be in file. </param>
	/// <param name="interval_base"> Time series interval base (see TimeInterval.*).  Currently
	/// only monthly, daily, and hourly data are supported. </param>
	/// <param name="interval_mult"> Time series interval multiplier. </param>
	/// <param name="date1"> First date in period to be stored in binary file. </param>
	/// <param name="date2"> Last date in period to be stored in binary file. </param>
	/// <param name="mode"> "r" for reading or "rw" for read/write. </param>
	/// <param name="write_header"> Indicates if header should be written. </param>
	/// <exception cref="IOException"> If the binary TS file cannot be created. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void initialize(String tsfile, int nts, int interval_base, int interval_mult, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2, String mode, boolean write_header) throws java.io.IOException
	private void initialize(string tsfile, int nts, int interval_base, int interval_mult, DateTime date1, DateTime date2, string mode, bool write_header)
	{
		_tsfile = tsfile;
		_nts = nts;
		_interval_base = interval_base;
		_interval_mult = interval_mult;
		_date1 = new DateTime(date1);
		_date2 = new DateTime(date2);
		_missing = -999.0;

		// Open the binary file...

		_fp = new RandomAccessFile(tsfile, mode);

		// Calculate sizes, positions...

		_amon1 = _date1.getAbsoluteMonth();

		_header_byte = 0;
		_header_size = 136;

		_ts_header_byte = _header_byte + _header_size;
		//_ts_header_size = 400;
		_ts_header_size = 560;

		if (_interval_base == TimeInterval.MONTH)
		{
			_ts_data_size = 1 * 8;
		}
		else if (_interval_base == TimeInterval.DAY)
		{
			_ts_data_size = 31 * 8;
		}
		else if (_interval_base == TimeInterval.HOUR)
		{
			_ts_data_size = (31 * 24 / _interval_mult) * 8;
		}

		if (Message.isDebugOn)
		{
			Message.printStatus(1, "BinaryTS.intialize", "Binary file has size: main header: " + _header_size + " total ts header: " + _ts_header_size * _nts + " total data size: " + _ts_data_size * _nts * (_date2.getAbsoluteMonth() - _date1.getAbsoluteMonth() + 1));
		}

		// Initialize the time series flags...

		long pos;
		for (int i = 0; i < _nts; i++)
		{
			pos = calculatePosition(PARAMETER_TS_HAS_HEADER, i);
			_fp.seek(pos);
			_fp.writeInt(0);
			pos = calculatePosition(PARAMETER_TS_HAS_DATA, i);
			_fp.seek(pos);
			_fp.writeInt(0);
		}

		// Write the header if requested...

		if (write_header)
		{
			writeHeader();
		}

		// Write one value at the end to make sure the file can get created...

		//pos = getDataPosition ( (_nts - 1), _date2 );
		//Message.printStatus ( 1, routine,
		//"Writing last value at byte " + pos + " to size file." );
		setDataValue((_nts - 1), _date2, _missing);
	}

	/// <summary>
	/// Determine if a data value is missing.
	/// </summary>
	public virtual bool isDataMissing(int its, double value)
	{
		if (value == _missing)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Read a String from the file at the current position.  The first null character
	/// causes the read to stop but the file position is set as if the full string was
	/// read. </summary>
	/// <param name="pos"> Starting byte (currently only used to set the position after the
	/// read). </param>
	/// <param name="size"> Number of characters to read. </param>
	/// <exception cref="IOException"> if there is an error reading the data. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private String readString(long pos, int maxsize) throws java.io.IOException
	private string readString(long pos, int maxsize)
	{
		StringBuilder buffer = new StringBuilder();
		char c;
		// Position the pointer...
		_fp.seek(pos);
		for (int i = 0; i < maxsize; i++)
		{
			c = _fp.readChar();
			if (c == '\0')
			{
				// End of string...
				break;
			}
			buffer.Append(c);
		}
		// Need to position at the end of the string, in case not all was
		// read.
		_fp.seek(pos + maxsize * 2);
		return buffer.ToString();
	}

	/// <summary>
	/// Read a time series from the binary file.  A new instance of the time series is
	/// returned and should be set to null when not needed.  The file is assumed to
	/// already be opened.  This is used, for example, when reading and writing from a
	/// BinaryTS file during processing. </summary>
	/// <param name="its"> Time series index. </param>
	/// <exception cref="Exception"> if the interval for the time series does not match that
	/// for the file or if a write error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TS readTimeSeries(int its) throws Exception
	public virtual TS readTimeSeries(int its)
	{ // Don't use the raw read methods for some data because strings with
		// nulls are better handled with this class' methods.
		long pos = calculatePosition(PARAMETER_TS_HAS_HEADER, its);
		_fp.seek(pos);
		int has_header = _fp.readInt();
		if (has_header == 0)
		{
			return null;
		}
		_fp.readInt(); // Indicates if time series has data
		// Read the identifier and create a time series...
		string tsident_string = getDescription(its);
		TSIdent tsident = new TSIdent(tsident_string);
		TimeInterval interval = TimeInterval.parseInterval(tsident.getInterval());
		TS ts = TSUtil.newTimeSeries(tsident_string, true);
		ts.setIdentifier(tsident);
		// Set the dates...
		DateTime date1 = new DateTime();
		DateTime date2 = new DateTime();
		if (interval.getBase() == TimeInterval.YEAR)
		{
			date1.setPrecision(DateTime.PRECISION_YEAR);
			date2.setPrecision(DateTime.PRECISION_YEAR);
		}
		else if (interval.getBase() == TimeInterval.MONTH)
		{
			date1.setPrecision(DateTime.PRECISION_MONTH);
			date2.setPrecision(DateTime.PRECISION_MONTH);
		}
		else if (interval.getBase() == TimeInterval.DAY)
		{
			date1.setPrecision(DateTime.PRECISION_DAY);
			date2.setPrecision(DateTime.PRECISION_DAY);
		}
		else if (interval.getBase() == TimeInterval.HOUR)
		{
			date1.setPrecision(DateTime.PRECISION_HOUR);
			date2.setPrecision(DateTime.PRECISION_HOUR);
		}
		else if (interval.getBase() == TimeInterval.MINUTE)
		{
			date1.setPrecision(DateTime.PRECISION_MINUTE);
			date2.setPrecision(DateTime.PRECISION_MINUTE);
		}
		date1.setYear(_fp.readInt());
		date1.setMonth(_fp.readInt());
		date1.setDay(_fp.readInt());
		date1.setHour(_fp.readInt());
		date1.setMinute(_fp.readInt());
		date1.setSecond(_fp.readInt());
		date2.setYear(_fp.readInt());
		date2.setMonth(_fp.readInt());
		date2.setDay(_fp.readInt());
		date2.setHour(_fp.readInt());
		date2.setMinute(_fp.readInt());
		date2.setSecond(_fp.readInt());
		ts.setDate1(date1);
		ts.setDate2(date2);
		ts.setDescription(getDescription(its));
		ts.setDataUnits(getDataUnits(its));
		ts.setAlias(getAlias(its));
		// Read the data...
		DateTime date = new DateTime(date1);
		for (; date.lessThanOrEqualTo(date2); date.addInterval(_interval_base, _interval_mult))
		{
			ts.setDataValue(date, getDataValue(its,date));
		}
		// Clean up...
		tsident_string = null;
		tsident = null;
		interval = null;
		date1 = null;
		date2 = null;
		date = null;
		return ts;
	}

	/// <summary>
	/// Set the data value for the date. </summary>
	/// <param name="its"> Time series to process. </param>
	/// <param name="date"> Date of interest. </param>
	/// <param name="value"> Data value corresponding to date. </param>
	/// <exception cref="IOException"> if there is an error setting the data value. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setDataValue(int its, RTi.Util.Time.DateTime date, double value) throws java.io.IOException
	public virtual void setDataValue(int its, DateTime date, double value)
	{ // Do not define routine here to increase peformance.

		if (date == null)
		{
			return;
		}

		if ((date.lessThan(_date1)) || (date.greaterThan(_date2)))
		{
			return;
		}

		// Get the data position.  This should be safe because we checked dates
		// above...

		long pos = getDataPosition(its, date);
		_fp.seek(pos);
		//Message.printStatus ( 1, "BinaryTS.setDataValue",
		//"Setting " + value + " for its[" + its + "] on " + date );
		_fp.writeDouble(value);
	}

	/// <summary>
	/// Return the number of time series in the file. </summary>
	/// <returns> the number of time series in the file. </returns>
	public virtual int size()
	{
		return _nts;
	}

	/// <summary>
	/// Write a String to the file at the current position.  The file is padded with
	/// null characters. </summary>
	/// <exception cref="IOException"> if there is an error writing the data. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void writeString(String string, int maxsize) throws java.io.IOException
	private void writeString(string @string, int maxsize)
	{
		int len = 0;
		if (!string.ReferenceEquals(@string, null))
		{
			len = @string.Length;
		}
		int i = 0;
		for (i = 0; (i < len) && (i < maxsize); i++)
		{
			_fp.writeChar(@string[i]);
		}
		if (len < maxsize)
		{
			for (i = len; i < maxsize; i++)
			{
				_fp.writeChar('\0');
			}
		}
	}

	/// <summary>
	/// Write the BinaryTS file header. </summary>
	/// <exception cref="IOException"> if there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeHeader() throws java.io.IOException
	public virtual void writeHeader()
	{
		_fp.seek(_header_byte);
		writeString(_version, 40);
		_fp.writeInt(_interval_base);
		_fp.writeInt(_interval_mult);
		_fp.writeInt(_date1.getYear());
		_fp.writeInt(_date1.getMonth());
		_fp.writeInt(_date1.getDay());
		_fp.writeInt(_date1.getHour());
		_fp.writeInt(_date1.getMinute());
		_fp.writeInt(_date1.getSecond());
		_fp.writeInt(_date2.getYear());
		_fp.writeInt(_date2.getMonth());
		_fp.writeInt(_date2.getDay());
		_fp.writeInt(_date2.getHour());
		_fp.writeInt(_date2.getMinute());
		_fp.writeInt(_date2.getSecond());
	}

	/// <summary>
	/// Write a time series to the binary file.  The time series must have the same
	/// interval as that specified when constructing the BinaryTS and only the period
	/// for the BinaryTS will be written (a time series with a longer period will be
	/// truncated).  Null time series will result in space being
	/// wasted in the file.  Use the hasTSHeader() and hasTSData() methods to see if
	/// time series header and data are available. </summary>
	/// <param name="ts"> Time series to write. </param>
	/// <param name="its"> Time series index. </param>
	/// <exception cref="IOException"> if the interval for the time series does not match that
	/// for the file or if a write error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeTimeSeries(TS ts, int its) throws java.io.IOException
	public virtual void writeTimeSeries(TS ts, int its)
	{
		if (ts == null)
		{
			return;
		}
		// Write the header...
		long pos = calculatePosition(PARAMETER_TS_HAS_HEADER, its);
		_fp.seek(pos);
		_fp.writeInt(1);
		_fp.writeInt(0); // Will be updated later...
		// Add what was just written and skip HAS_DATA...
		writeString(ts.getIdentifierString(), 80);
		DateTime date = ts.getDate1();
		_fp.writeInt(date.getYear());
		_fp.writeInt(date.getMonth());
		_fp.writeInt(date.getDay());
		_fp.writeInt(date.getHour());
		_fp.writeInt(date.getMinute());
		_fp.writeInt(date.getSecond());
		date = ts.getDate2();
		_fp.writeInt(date.getYear());
		_fp.writeInt(date.getMonth());
		_fp.writeInt(date.getDay());
		_fp.writeInt(date.getHour());
		_fp.writeInt(date.getMinute());
		_fp.writeInt(date.getSecond());
		writeString(ts.getDescription(), 80);
		//Message.printStatus ( 1, "BinaryTS.writeTimeSeries",
		//"Write data units: \"" + ts.getDataUnits() + "\" at " +
		//_fp.getFilePointer() );
		writeString(ts.getDataUnits(), 12);
		writeString(ts.getAlias(), 80);
		// Try to read to debug...
		//pos = calculatePosition ( PARAMETER_TS_UNITS, its );
		//_fp.seek ( pos );
		//Message.printStatus ( 1, "BinaryTS.writeTimeSeries",
		//"Read data units: \"" + readString ( pos, 12 ) + "\" at " + pos );
		// Write the data...
		if (Message.isDebugOn && (its == (_nts - 1)))
		{
			Message.printStatus(1, "", "_ts_header_byte=" + _ts_header_byte + " _ts_header_size=" + _ts_header_size + " _header_byte=" + _header_byte + " _header_size=" + _header_size + " pos after header = " + _fp.getFilePointer());
		}
		date = new DateTime(_date1);
		for (; date.lessThanOrEqualTo(_date2); date.addInterval(_interval_base, _interval_mult))
		{
			setDataValue(its, date, ts.getDataValue(date));
		}
		pos = calculatePosition(PARAMETER_TS_HAS_DATA, its);
		_fp.writeInt(1);
		date = null;
	}

	} // End BinaryTS class

}