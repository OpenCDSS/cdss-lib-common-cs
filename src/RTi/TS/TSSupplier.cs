using System.Collections.Generic;

// TSSupplier - provide basic I/O interface to supply time series

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
// TSSupplier - provide basic I/O interface to supply time series
// ----------------------------------------------------------------------------
// History:
//
// 2001-11-07	Steven A. Malers, RTi	Implemented code.
// 2002-04-25	SAM, RTi		Remove some methods - try to make
//					the interface as simple as possible.
//					Add getTSSupplierName().
// 2003-06-02	SAM, RTi		Upgrade to use generic classes.
//					* Change TSDate to DateTime.
// ----------------------------------------------------------------------------
// EndHeader

namespace RTi.TS
{

	using DateTime = RTi.Util.Time.DateTime;

	/// <summary>
	/// The TSSupplier interface should be implemented by classes that are supplying
	/// time series information to higher level code.  For example, a class can be
	/// written to read and write time series from a custom database.  The higher-level
	/// class should include a method addTSSupplier() that makes the interface class
	/// known to the higher level code.  This interface should be used in conjunction
	/// with the TSReceiver and TSIdentSupplier interfaces.
	/// </summary>
	public abstract interface TSSupplier
	{
	/// <summary>
	/// Return the name of the TSSupplier.  This is used for messages.
	/// </summary>
	string getTSSupplierName();

	/// <summary>
	/// Read a time series given a time series identifier string.  The string may be
	/// a file name if the time series are stored in files, or may be a true identifier
	/// string if the time series is stored in a database.  The specified period is
	/// read.  The data are converted to the requested units. </summary>
	/// <param name="tsident_string"> Time series identifier or file name to read. </param>
	/// <param name="date1"> First date to query.  If specified as null the entire period will
	/// be read. </param>
	/// <param name="date2"> Last date to query.  If specified as null the entire period will
	/// be read. </param>
	/// <param name="req_units"> Requested units to return data.  If specified as null or an
	/// empty string the units will not be converted. </param>
	/// <param name="read_data"> if true, the data will be read.  If false, only the time series
	/// header will be read. </param>
	/// <returns> Time series of appropriate type (e.g., MonthTS, HourTS). </returns>
	/// <exception cref="Exception"> if an error occurs during the read. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract TS readTimeSeries(String tsident_string, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2, String req_units, boolean read_data) throws Exception;
	TS readTimeSeries(string tsident_string, DateTime date1, DateTime date2, string req_units, bool read_data);

	/// <summary>
	/// Read a time series given an existing time series and a file name.
	/// The specified period is read.
	/// The data are converted to the requested units. </summary>
	/// <param name="req_ts"> Requested time series to fill.  If null, return a new time series.
	/// If not null, all data are reset, except for the identifier, which is assumed
	/// to have been set in the calling code.  This can be used to query a single
	/// time series from a file that contains multiple time series. </param>
	/// <param name="fname"> File name to read. </param>
	/// <param name="date1"> First date to query.  If specified as null the entire period will
	/// be read. </param>
	/// <param name="date2"> Last date to query.  If specified as null the entire period will
	/// be read. </param>
	/// <param name="req_units"> Requested units to return data.  If specified as null or an
	/// empty string the units will not be converted. </param>
	/// <param name="read_data"> if true, the data will be read.  If false, only the time series
	/// header will be read. </param>
	/// <returns> Time series of appropriate type (e.g., MonthTS, HourTS). </returns>
	/// <exception cref="Exception"> if an error occurs during the read. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract TS readTimeSeries(TS req_ts, String fname, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2, String req_units, boolean read_data) throws Exception;
	TS readTimeSeries(TS req_ts, string fname, DateTime date1, DateTime date2, string req_units, bool read_data);

	/// <summary>
	/// Read a time series list from a file (this is typically used used where a time
	/// series file can contain one or more time series).
	/// The specified period is
	/// read.  The data are converted to the requested units. </summary>
	/// <param name="fname"> File to read. </param>
	/// <param name="date1"> First date to query.  If specified as null the entire period will
	/// be read. </param>
	/// <param name="date2"> Last date to query.  If specified as null the entire period will
	/// be read. </param>
	/// <param name="req_units"> Requested units to return data.  If specified as null or an
	/// empty string the units will not be converted. </param>
	/// <param name="read_data"> if true, the data will be read.  If false, only the time series
	/// header will be read. </param>
	/// <returns> List of time series of appropriate type (e.g., MonthTS, HourTS). </returns>
	/// <exception cref="Exception"> if an error occurs during the read. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract java.util.List<TS> readTimeSeriesList(String fname, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2, String req_units, boolean read_data) throws Exception;
	IList<TS> readTimeSeriesList(string fname, DateTime date1, DateTime date2, string req_units, bool read_data);

	/// <summary>
	/// Read a time series list from a file or database using the time series identifier
	/// information as a query pattern.
	/// The specified period is
	/// read.  The data are converted to the requested units. </summary>
	/// <param name="tsident"> A TSIdent instance that indicates which time series to query.
	/// If the identifier parts are empty, they will be ignored in the selection.  If
	/// set to "*", then any time series identifier matching the field will be selected.
	/// If set to a literal string, the identifier field must match exactly to be
	/// selected. </param>
	/// <param name="fname"> File to read. </param>
	/// <param name="date1"> First date to query.  If specified as null the entire period will
	/// be read. </param>
	/// <param name="date2"> Last date to query.  If specified as null the entire period will
	/// be read. </param>
	/// <param name="req_units"> Requested units to return data.  If specified as null or an
	/// empty string the units will not be converted. </param>
	/// <param name="read_data"> if true, the data will be read.  If false, only the time series
	/// header will be read. </param>
	/// <returns> List of time series of appropriate type (e.g., MonthTS, HourTS). </returns>
	/// <exception cref="Exception"> if an error occurs during the read. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract java.util.List<TS> readTimeSeriesList(TSIdent tsident, String fname, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2, String req_units, boolean read_data) throws Exception;
	IList<TS> readTimeSeriesList(TSIdent tsident, string fname, DateTime date1, DateTime date2, string req_units, bool read_data);

	}

}