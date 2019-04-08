// TimeSeriesData - class to store a single time series record in a DataStore

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

namespace riverside.datastore
{
	using DMIDataObject = RTi.DMI.DMIDataObject;
	using DMIUtil = RTi.DMI.DMIUtil;
	using DateTime = RTi.Util.Time.DateTime;

	/// <summary>
	/// Store single value record data from time series table layouts.  Some data fields are optional
	/// depending on the table layout.  This class is meant to be used internally in the RiversideDB_DMI package,
	/// for example during transfer of database records to time series objects.
	/// </summary>
	public class TimeSeriesData : DMIDataObject
	{

	/// <summary>
	/// Internal key to link time series record to time series metadata.
	/// </summary>
	//private long metaId = DMIUtil.MISSING_LONG;

	/// <summary>
	/// DateTime corresponding to the data value.
	/// </summary>
	private DateTime dateTime = null;

	/// <summary>
	/// Data value.
	/// </summary>
	private double value = DMIUtil.MISSING_DOUBLE;

	/// <summary>
	/// Optional data quality flag.
	/// </summary>
	private string flag = DMIUtil.MISSING_STRING;

	/// <summary>
	/// Optional duration of value in seconds.
	/// </summary>
	//private int duration = DMIUtil.MISSING_INT;

	/// <summary>
	/// Optional DateTime corresponding to insert date/time for the value.  Tables that use this
	/// column must be sorted first by _Date_Time and then _Creation_Time.
	/// </summary>
	//private DateTime creationTime = null;

	/// <summary>
	/// Constructor.  
	/// </summary>
	public TimeSeriesData(DateTime dateTime, double value, string flag) : base()
	{
		this.dateTime = dateTime;
		this.value = value;
		this.flag = flag;
	}

	/// <summary>
	/// Returns the DateTime for the value </summary>
	/// <returns> the DateTime for the value </returns>
	public virtual DateTime getDateTime()
	{
		return this.dateTime;
	}

	/// <summary>
	/// Returns the flag for the value </summary>
	/// <returns> the flag for the value </returns>
	public virtual string getFlag()
	{
		return this.flag;
	}


	/// <summary>
	/// Returns the data value </summary>
	/// <returns> the data value </returns>
	public virtual double getValue()
	{
		return this.value;
	}

	}

}