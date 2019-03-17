using System.Text;

// TimeSeriesMeta - class to generically store a time series metadata record from a datastore

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
	using TSIdent = RTi.TS.TSIdent;

	/// <summary>
	/// Simple object to hold time series metadata for GenericDatabaseDataStore.
	/// </summary>
	public class TimeSeriesMeta
	{

	/// <summary>
	/// Internal identifier (primary key).
	/// </summary>
	private long id = -1;

	/// <summary>
	/// Location type.
	/// </summary>
	private string locationType;

	/// <summary>
	/// Location ID.
	/// </summary>
	private string locationID;

	/// <summary>
	/// Data source.
	/// </summary>
	private string dataSource;

	/// <summary>
	/// Data type.
	/// </summary>
	private string dataType;

	/// <summary>
	/// Interval.
	/// </summary>
	private string interval;

	/// <summary>
	/// Scenario.
	/// </summary>
	private string scenario;

	/// <summary>
	/// Descrition for time series (often the location name).
	/// </summary>
	private string description;

	/// <summary>
	/// Data units.
	/// </summary>
	private string units;

	/// <summary>
	/// Construct a metadata object. </summary>
	/// <param name="locationType"> the location type, as per the conventions of the datastore. </param>
	/// <param name="locationID"> the location identifier (e.g., station identifier). </param>
	/// <param name="dataSource"> the data source abbreviation (e.g., agency). </param>
	/// <param name="dataType"> the data type abbreviation (e.g., "Streamflow"). </param>
	/// <param name="interval"> the data interval, as per TimeInterval strings. </param>
	/// <param name="scenario"> the scenario for the time series. </param>
	/// <param name="description"> a short description of the time series, often the location name. </param>
	/// <param name="units"> the time series data units </param>
	/// <param name="id"> internal identifier, typically the primary key in a database. </param>
	public TimeSeriesMeta(string locationType, string locationID, string dataSource, string dataType, string interval, string scenario, string description, string units, long id)
	{
		this.locationType = (string.ReferenceEquals(locationType, null) ? "" : locationType);
		this.locationID = (string.ReferenceEquals(locationID, null) ? "" : locationID);
		this.dataSource = (string.ReferenceEquals(dataSource, null) ? "" : dataSource);
		this.dataType = (string.ReferenceEquals(dataType, null) ? "" : dataType);
		this.interval = (string.ReferenceEquals(interval, null) ? "" : interval);
		this.scenario = (string.ReferenceEquals(scenario, null) ? "" : scenario);
		this.description = (string.ReferenceEquals(description, null) ? "" : description);
		this.units = (string.ReferenceEquals(units, null) ? "" : units);
		this.id = id;
	}

	/// <summary>
	/// Return the data source.
	/// </summary>
	public virtual string getDataSource()
	{
		return this.dataSource;
	}

	/// <summary>
	/// Return the data type.
	/// </summary>
	public virtual string getDataType()
	{
		return this.dataType;
	}

	/// <summary>
	/// Return the description.
	/// </summary>
	public virtual string getDescription()
	{
		return this.description;
	}

	/// <summary>
	/// Return the identifier.
	/// </summary>
	public virtual long getId()
	{
		return this.id;
	}

	/// <summary>
	/// Return the interval.
	/// </summary>
	public virtual string getInterval()
	{
		return this.interval;
	}

	/// <summary>
	/// Return the location ID.
	/// </summary>
	public virtual string getLocationID()
	{
		return this.locationID;
	}

	/// <summary>
	/// Return the location ID.
	/// </summary>
	public virtual string getLocationType()
	{
		return this.locationType;
	}

	/// <summary>
	/// Return the scenario.
	/// </summary>
	public virtual string getScenario()
	{
		return this.scenario;
	}

	/// <summary>
	/// Return the time series identifier corresponding to the metadata. </summary>
	/// <param name="dataStore"> the data store that is being used to process data, or null to ignore (if null, the current
	/// API version is assumed). </param>
	/// <returns> the TSID string of the form ID.ACIS.MajorVariableNumber.Day[~DataStoreName] </returns>
	public virtual string getTSID(GenericDatabaseDataStore dataStore)
	{
		string dataStoreName = dataStore.getName();
		StringBuilder tsid = new StringBuilder();
		if (!getLocationType().Equals(""))
		{
			tsid.Append(getLocationType());
			tsid.Append(TSIdent.LOC_TYPE_SEPARATOR);
		}
		tsid.Append(getLocationID());
		tsid.Append(TSIdent.SEPARATOR);
		tsid.Append(getDataSource());
		tsid.Append(TSIdent.SEPARATOR);
		tsid.Append(getDataType());
		tsid.Append(TSIdent.SEPARATOR);
		tsid.Append(getInterval());
		if (!getScenario().Equals(""))
		{
			tsid.Append(TSIdent.SEPARATOR);
			tsid.Append(getScenario());
		}
		if ((!string.ReferenceEquals(dataStoreName, null)) && !dataStoreName.Equals(""))
		{
			tsid.Append("~" + dataStoreName);
		}
		return tsid.ToString();
	}

	/// <summary>
	/// Return the data type.
	/// </summary>
	public virtual string getUnits()
	{
		return this.units;
	}

	}

}