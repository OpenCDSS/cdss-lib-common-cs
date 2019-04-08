using System;
using System.Collections.Generic;
using System.Text;

// GenericDatabaseDataStore - generic datastore for a database connection

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

	using AbstractDatabaseDataStore = RTi.DMI.AbstractDatabaseDataStore;
	using DMI = RTi.DMI.DMI;
	using DMISelectStatement = RTi.DMI.DMISelectStatement;
	using DMIUtil = RTi.DMI.DMIUtil;
	using GenericDMI = RTi.DMI.GenericDMI;
	using TS = RTi.TS.TS;
	using TSIdent = RTi.TS.TSIdent;
	using TSUtil = RTi.TS.TSUtil;
	using InputFilter = RTi.Util.GUI.InputFilter;
	using InputFilter_JPanel = RTi.Util.GUI.InputFilter_JPanel;
	using IOUtil = RTi.Util.IO.IOUtil;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;
	using DateTime = RTi.Util.Time.DateTime;
	using StopWatch = RTi.Util.Time.StopWatch;

	/// <summary>
	/// Data store for Generic database, to allow table/view queries.
	/// This class maintains the database connection information in a general way.
	/// @author sam
	/// </summary>
	public class GenericDatabaseDataStore : AbstractDatabaseDataStore
	{

	/// <summary>
	/// Datastore configuration properties that map the database time series metadata table/view to the datastore.
	/// </summary>
	public const string TS_META_TABLE_PROP = "TimeSeriesMetadataTable";
	public const string TS_META_TABLE_LOCTYPE_COLUMN_PROP = "TimeSeriesMetadataTable_LocationTypeColumn";
	public const string TS_META_TABLE_LOCATIONID_COLUMN_PROP = "TimeSeriesMetadataTable_LocationIdColumn";
	public const string TS_META_TABLE_DATASOURCE_COLUMN_PROP = "TimeSeriesMetadataTable_DataSourceColumn";
	public const string TS_META_TABLE_DATATYPE_COLUMN_PROP = "TimeSeriesMetadataTable_DataTypeColumn";
	public const string TS_META_TABLE_DATAINTERVAL_COLUMN_PROP = "TimeSeriesMetadataTable_DataIntervalColumn";
	public const string TS_META_TABLE_SCENARIO_COLUMN_PROP = "TimeSeriesMetadataTable_ScenarioColumn";
	public const string TS_META_TABLE_DESCRIPTION_COLUMN_PROP = "TimeSeriesMetadataTable_DescriptionColumn";
	public const string TS_META_TABLE_UNITS_COLUMN_PROP = "TimeSeriesMetadataTable_DataUnitsColumn";
	public const string TS_META_TABLE_ID_COLUMN_PROP = "TimeSeriesMetadataTable_MetadataIdColumn";

	/*
	Property that indicates filter, will be followed by ".1", ".2", etc.
	*/
	public const string TS_META_TABLE_FILTER_PROP = "TimeSeriesMetadataTable_MetadataFilter";

	public const string TS_DATA_TABLE_PROP = "TimeSeriesDataTable";
	public const string TS_DATA_TABLE_METAID_COLUMN_PROP = "TimeSeriesDataTable_MetadataIdColumn";
	public const string TS_DATA_TABLE_DATETIME_COLUMN_PROP = "TimeSeriesDataTable_DateTimeColumn";
	public const string TS_DATA_TABLE_VALUE_COLUMN_PROP = "TimeSeriesDataTable_ValueColumn";
	public const string TS_DATA_TABLE_FLAG_COLUMN_PROP = "TimeSeriesDataTable_FlagColumn";

	/// <summary>
	/// Hashtable that stores list of data types for different time series metadata inputs.
	/// The key is a string consisting of locType, locID, dataSource, interval, scenario as passed to getTimeSeriesMetaDataTypeList.
	/// </summary>
	private Dictionary<string, IList<string>> timeSeriesMetaHash = new Dictionary<string, IList<string>>();

	/// <summary>
	/// Database metadata, stored here to speed up database interactions.
	/// </summary>
	private DatabaseMetaData databaseMetadata = null;

	/// <summary>
	/// Construct a data store given a DMI instance, which is assumed to be open. </summary>
	/// <param name="name"> identifier for the data store </param>
	/// <param name="description"> name for the data store </param>
	/// <param name="dmi"> DMI instance to use for the data store. </param>
	public GenericDatabaseDataStore(string name, string description, DMI dmi)
	{
		setName(name);
		setDescription(description);
		setDMI(dmi);
		// Rely on other authentication to prevent writing.
		// TODO SAM 2013-02-26 Perhaps use a database configuration file property to control
		dmi.setEditable(true);
	}

	/// <summary>
	/// Factory method to construct a data store connection from a properties file. </summary>
	/// <param name="filename"> name of file containing property strings </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static GenericDatabaseDataStore createFromFile(String filename) throws java.io.IOException, Exception
	public static GenericDatabaseDataStore createFromFile(string filename)
	{
		// Read the properties from the file
		PropList props = new PropList("");
		props.setPersistentName(filename);
		props.readPersistent(false);
		// Set a property for the configuration filename because it is used later
		props.set("DataStoreConfigFile=" + filename);
		string name = IOUtil.expandPropertyForEnvironment("Name",props.getValue("Name"));
		string description = IOUtil.expandPropertyForEnvironment("Description",props.getValue("Description"));
		string databaseEngine = IOUtil.expandPropertyForEnvironment("DatabaseEngine",props.getValue("DatabaseEngine"));
		string databaseServer = IOUtil.expandPropertyForEnvironment("DatabaseServer",props.getValue("DatabaseServer"));
		string databaseName = IOUtil.expandPropertyForEnvironment("DatabaseName",props.getValue("DatabaseName"));
		string databasePort = IOUtil.expandPropertyForEnvironment("DatabasePort",props.getValue("DatabasePort"));
		int port = -1;
		if ((!string.ReferenceEquals(databasePort, null)) && !databasePort.Equals(""))
		{
			try
			{
				port = int.Parse(databasePort);
			}
			catch (System.FormatException)
			{
				port = -1;
			}
		}
		string odbcName = props.getValue("OdbcName");
		string systemLogin = IOUtil.expandPropertyForEnvironment("SystemLogin",props.getValue("SystemLogin"));
		string systemPassword = IOUtil.expandPropertyForEnvironment("SystemPassword",props.getValue("SystemPassword"));

		// Get the properties and create an instance
		GenericDMI dmi = null;
		if ((!string.ReferenceEquals(odbcName, null)) && !odbcName.Equals(""))
		{
			// An ODBC connection is configured so use it
			dmi = new GenericDMI(databaseEngine, odbcName, systemLogin, systemPassword); // OK if null - use read-only guest
		}
		else
		{
			// Use the parts and create the connection string on the fly
			dmi = new GenericDMI(databaseEngine, databaseServer, databaseName, port, systemLogin, systemPassword);
		}
		dmi.open();
		GenericDatabaseDataStore ds = new GenericDatabaseDataStore(name, description, dmi);
		// Save all the properties generically for use later.  This defines tables for time series meta and data mapping.
		ds.setProperties(props);
		return ds;
	}

	/// <summary>
	/// Return the SQL column type.
	/// </summary>
	private int getColumnType(DatabaseMetaData metadata, string tableName, string columnName)
	{
		string routine = "GenericDatabaseDataStore.getColumnType", message;
		ResultSet rs;
		try
		{
			rs = metadata.getColumns(null, null, tableName, columnName);
			if (rs == null)
			{
				message = "Error getting columns for \"" + tableName + "\" table.";
				Message.printWarning(2, routine, message);
				throw new Exception(message);
			}
		}
		catch (Exception)
		{
			message = "Error getting database information for table \"" + tableName + "\".";
			Message.printWarning(2, routine, message);
			throw new Exception(message);
		}
		// Now check for the column by looping through result set...

		string s;
		int i;
		int colType = -1;
		try
		{
			while (rs.next())
			{
				// The column name is field 4, data type field 5...
				s = rs.getString(4);
				if (!rs.wasNull())
				{
					s = s.Trim();
					if (s.Equals(columnName, StringComparison.OrdinalIgnoreCase))
					{
						i = rs.getInt(5);
						if (!rs.wasNull())
						{
							colType = i;
						}
						break;
					}
				}
			}
		}
		catch (SQLException)
		{
		}
		finally
		{
			DMI.closeResultSet(rs);
		}
		return colType;
	}

	/// <summary>
	/// Return the database metadata associated with the database.  If metadata have not been retrieved, retrieve and save.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private java.sql.DatabaseMetaData getDatabaseMetaData() throws java.sql.SQLException
	private DatabaseMetaData getDatabaseMetaData()
	{
		if (this.databaseMetadata == null)
		{
			// Metadata have not previously been retrieved so get now
			this.databaseMetadata = getDMI().getConnection().getMetaData();
		}
		return this.databaseMetadata;
	}

	/// <summary>
	/// Get a datastore property for a specific table.  This requires that property be coded as:
	/// <pre>
	/// PropName = "Table1:PropValue,Table2:PropValue"
	/// </pre>
	/// </summary>
	private string getPropertyForTable(string propName, string table)
	{
		// Get the full property
		string propVal = getProperty(propName);
		if (string.ReferenceEquals(propVal, null) || (propVal.IndexOf(":", StringComparison.Ordinal) < 0))
		{
			// Return as is
			return propVal;
		}
		// Parse, multiple items in list separated by ,
		string[] parts = propVal.Split(",", true);
		string[] parts2;
		for (int i = 0; i < parts.Length; i++)
		{
			parts2 = parts[i].Split(":", true);
			if (parts2[0].Trim().Equals(table, StringComparison.OrdinalIgnoreCase))
			{
				return parts2[1].Trim();
			}
		}
		return null;
	}

	/// <summary>
	/// Get data type strings for the datastore, if time series support is configured.
	/// This method checks for a cached result and if found, returns it.  Otherwise, it calls the read method of similar name and caches
	/// the result. </summary>
	/// <param name="includeNotes"> if true, include notes in the data type strings, like "DataType - Note"
	/// (currently does nothing) </param>
	/// <param name="locType"> location type to use as filter (ignored if blank or null) </param>
	/// <param name="locID"> location ID to use as filter (ignored if blank or null) </param>
	/// <param name="dataSource"> data source to use as filter (ignored if blank or null) </param>
	/// <param name="interval"> interval to use as filter (ignored if blank or null) </param>
	/// <param name="scenario"> scenario to use as filter (ignored if blank or null) </param>
	/// <returns> the list of data type strings by making a unique query of the  </returns>
	public virtual IList<string> getTimeSeriesMetaDataTypeList(bool includeNotes, string locType, string locID, string dataSource, string interval, string scenario)
	{
		// Only cache certain combinations as there is a trade-off between memory and performance
		// TODO SAM 2013-08-28 Evaluate what gets cached and what not.
		// For now only cache all nulls since that is the main choice
		// TODO SAM 2013-08-28 Remove logging messages if code works OK
		//Message.printStatus(2, "", "Getting data types for " + locType + "." + locID + "." + dataSource + "." + interval + "." + scenario);
		IList<string> dataTypeList = null;
		if ((string.ReferenceEquals(locType, null)) && (string.ReferenceEquals(locID, null)) && (string.ReferenceEquals(dataSource, null)) && (string.ReferenceEquals(interval, null)) && (string.ReferenceEquals(scenario, null)))
		{
			//Message.printStatus(2, "", "Looking up data from hashtable");
			// Use the hashtable
			string key = locType + "." + locID + "." + dataSource + "." + interval + "." + scenario;
			object o = this.timeSeriesMetaHash[key];
			if (o == null)
			{
				// Not in the hashtable so read the data and set in the hastable
				//Message.printStatus(2, "", "Nothing in hashtable...reading");
				dataTypeList = readTimeSeriesMetaDataTypeList(includeNotes, locType, locID, dataSource, interval, scenario);
				//Message.printStatus(2, "", "Read " + dataTypeList.size() + " data types...saving to hashtable");
				this.timeSeriesMetaHash[key] = dataTypeList;
			}
			else
			{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<String> dataTypeList0 = (java.util.List<String>)o;
				IList<string> dataTypeList0 = (IList<string>)o;
				dataTypeList = dataTypeList0;
				//Message.printStatus(2, "", "Got " + dataTypeList.size() + " data types from hashtable");
			}
			return dataTypeList;
		}
		else
		{
			dataTypeList = readTimeSeriesMetaDataTypeList(includeNotes, locType, locID, dataSource, interval, scenario);
			//Message.printStatus(2, "", "Got " + dataTypeList.size() + " data types from read (but not saving to hashtable)");
			return dataTypeList;
		}
	}

	/// <summary>
	/// Create a list of where clauses give an InputFilter_JPanel.  The InputFilter
	/// instances that are managed by the InputFilter_JPanel must have been defined with
	/// the database table and field names in the internal (non-label) data. </summary>
	/// <returns> a list of where clauses, each of which can be added to a DMI statement. </returns>
	/// <param name="dmi"> The DMI instance being used, which may be checked for specific formatting. </param>
	/// <param name="panel"> The InputFilter_JPanel instance to be converted.  If null, an empty list will be returned. </param>
	private IList<string> getWhereClausesFromInputFilter(DMI dmi, InputFilter_JPanel panel)
	{
		// Loop through each filter group.  There will be one where clause per filter group.

		if (panel == null)
		{
			return new List<string>();
		}

		int nfg = panel.getNumFilterGroups();
		InputFilter filter;
		IList<string> whereClauses = new List<string>();
		string whereClause = ""; // A where clause that is being formed.
		for (int ifg = 0; ifg < nfg; ifg++)
		{
			filter = panel.getInputFilter(ifg);
			whereClause = DMIUtil.getWhereClauseFromInputFilter(dmi, filter,panel.getOperator(ifg), true);
			if (!string.ReferenceEquals(whereClause, null))
			{
				whereClauses.Add(whereClause);
			}
		}
		return whereClauses;
	}

	/// <summary>
	/// Create a where string given an InputFilter_JPanel.  The InputFilter
	/// instances that are managed by the InputFilter_JPanel must have been defined with
	/// the database table and field names in the internal (non-label) data. </summary>
	/// <returns> a list of where clauses as a string, each of which can be added to a DMI statement. </returns>
	/// <param name="dmi"> The DMI instance being used, which may be checked for specific formatting. </param>
	/// <param name="panel"> The InputFilter_JPanel instance to be converted.  If null, an empty list will be returned. </param>
	/// <param name="tableAndColumnName"> the name of the table for which to get where clauses in format TableName.ColumnName. </param>
	/// <param name="useAnd"> if true, then "and" is used instead of "where" in the where strings.  The former can be used
	/// with "join on" SQL syntax. </param>
	/// <param name="addNewline"> if true, add a newline if the string is non-blank - this simply helps with formatting of
	/// the big SQL, so that logging has reasonable line breaks </param>
	private string getWhereClauseStringFromInputFilter(DMI dmi, InputFilter_JPanel panel, string tableAndColumnName, bool addNewline)
	{
		IList<string> whereClauses = getWhereClausesFromInputFilter(dmi, panel);
		StringBuilder whereString = new StringBuilder();
		foreach (string whereClause in whereClauses)
		{
			//Message.printStatus(2,"","Comparing where clause \"" + whereClause + "\" to \"" + tableAndColumnName + "\"");
			if (whereClause.ToUpper().IndexOf(tableAndColumnName.ToUpper(), StringComparison.Ordinal) < 0)
			{
				// Not for the requested table so don't include the where clause
				//Message.printStatus(2, "", "Did not match");
				continue;
			}
			//Message.printStatus(2, "", "Matched");
			if ((whereString.Length > 0))
			{
				// Need to concatenate
				whereString.Append(" and ");
			}
			whereString.Append("(" + whereClause + ")");
		}
		if (addNewline && (whereString.Length > 0))
		{
			whereString.Append("\n");
		}
		return whereString.ToString();
	}

	/// <summary>
	/// Indicate whether properties have been defined to allow querying time series from the datastore.
	/// Only the minimal properties are checked. </summary>
	/// <param name="checkDatabase"> check to see whether tables and columns mentioned in the configuration actually
	/// exist in the database </param>
	/// <returns> true if the datastore has properties defined to support reading time series </returns>
	public virtual bool hasTimeSeriesInterface(bool checkDatabase)
	{
		string routine = "GenericDatabaseDataStore.hasTimeSeriesInterface";
		DatabaseMetaData meta = null;
		if (checkDatabase)
		{
			try
			{
				meta = getDatabaseMetaData();
			}
			catch (Exception)
			{
				return false;
			}
		}
		string table, column;
		try
		{
			// Must have metadata table
			table = getProperty(TS_META_TABLE_PROP);
			if (string.ReferenceEquals(table, null))
			{
				Message.printStatus(2,routine,"Datastore \"" + getName() + "\" does not have configuration property \"" + TS_META_TABLE_PROP + "\".");
				return false;
			}
			else if (checkDatabase && !DMIUtil.databaseHasTable(meta, table))
			{
				 Message.printStatus(2,routine,"Datastore \"" + getName() + "\" does not have table/view \"" + table + "\".");
				 return false;
			}
			// Must have location, data type, and interval columns
			column = getProperty(TS_META_TABLE_LOCATIONID_COLUMN_PROP);
			if (string.ReferenceEquals(column, null))
			{
				Message.printStatus(2,routine,"Datastore \"" + getName() + "\" does not have configuration property \"" + TS_META_TABLE_LOCATIONID_COLUMN_PROP + "\".");
				return false;
			}
			else if (checkDatabase && !DMIUtil.databaseTableHasColumn(meta, table, column))
			{
				Message.printStatus(2,routine,"Datastore \"" + getName() + "\" table/view \"" + table + "\" does not have column \"" + column + "\".");
				return false;
			}
			column = getProperty(TS_META_TABLE_DATASOURCE_COLUMN_PROP);
			if (string.ReferenceEquals(column, null))
			{
				Message.printStatus(2,routine,"Datastore \"" + getName() + "\" does not have configuration property \"" + TS_META_TABLE_DATASOURCE_COLUMN_PROP + "\".");
				return false;
			}
			else if (checkDatabase && !DMIUtil.databaseTableHasColumn(meta, table, column))
			{
				Message.printStatus(2,routine,"Datastore \"" + getName() + "\" table/view \"" + table + "\" does not have column \"" + column + "\".");
				return false;
			}
			column = getProperty(TS_META_TABLE_DATAINTERVAL_COLUMN_PROP);
			if (string.ReferenceEquals(column, null))
			{
				Message.printStatus(2,routine,"Datastore \"" + getName() + "\" does not have configuration property \"" + TS_META_TABLE_DATAINTERVAL_COLUMN_PROP + "\".");
				return false;
			}
			else if (checkDatabase && !DMIUtil.databaseTableHasColumn(meta, table, column))
			{
				Message.printStatus(2,routine,"Datastore \"" + getName() + "\" table/view \"" + table + "\" does not have column \"" + column + "\".");
				return false;
			}
		}
		catch (Exception)
		{
			// Could not get database information
			return false;
		}
		return true;
	}

	/// <summary>
	/// Read a time series from the datastore.
	/// </summary>
	public virtual TS readTimeSeries(string tsidentString, DateTime inputStart, DateTime inputEnd, bool readData)
	{
		string routine = "GenericDatabaseDataStore.readTimeSeries", message;
		TS ts = null;
		TSIdent tsident = null;
		try
		{
			tsident = TSIdent.parseIdentifier(tsidentString);
		}
		catch (Exception e)
		{
			message = "Time series identifier \"" + tsidentString + "\" is invalid (" + e + ")";
			Message.printWarning(3,routine,message);
			throw new Exception(message);
		}
		// Get the time series metadata record
		StopWatch metaTimer = new StopWatch();
		metaTimer.start();
		TimeSeriesMeta tsMeta = readTimeSeriesMeta(tsident.getLocationType(), tsident.getLocation(), tsident.getSource(), tsident.getType(), tsident.getInterval(), tsident.getScenario());
		metaTimer.stop();
		if (tsMeta == null)
		{
			return null;
		}
		// Create the time series
		double missing = Double.NaN;
		try
		{
			ts = TSUtil.newTimeSeries(tsident + "~" + getName(), true);
			ts.setIdentifier(tsident);
			ts.setDataUnits(tsMeta.getUnits());
			ts.setDataUnitsOriginal(tsMeta.getUnits());
			ts.setDescription(tsMeta.getDescription());
			ts.setMissing(missing);
		}
		catch (Exception e)
		{
			Message.printWarning(3,routine,"Error creating time series (" + e + ").");
			return null;
		}
		if (!readData)
		{
			return ts;
		}
		// Read the time series data
		DMI dmi = getDMI();
		StopWatch dataTimer = new StopWatch();
		dataTimer.start();
		DMISelectStatement ss = new DMISelectStatement(dmi);
		string dataTable = getProperty(GenericDatabaseDataStore.TS_DATA_TABLE_PROP);
		//Message.printStatus(2, routine, "Data table = \"" + dataTable + "\"");
		if (!string.ReferenceEquals(dataTable, null))
		{
			// Table name may contain formatting like %I, etc.
			dataTable = ts.formatLegend(dataTable);
		}
		string dtColumn = getPropertyForTable(GenericDatabaseDataStore.TS_DATA_TABLE_DATETIME_COLUMN_PROP, dataTable);
		bool dateTimeInt = false; // true=integer year, false=timestamp
		int dtColumnType = -1;
		try
		{
			dtColumnType = getColumnType(getDatabaseMetaData(), dataTable, dtColumn);
			if ((dtColumnType == Types.TIMESTAMP) || (dtColumnType == Types.DATE))
			{
				dateTimeInt = false;
			}
			else if ((dtColumnType == Types.BIGINT) || (dtColumnType == Types.INTEGER) || (dtColumnType == Types.SMALLINT))
			{
				dateTimeInt = true;
			}
			else
			{
				message = "SQL column type " + dtColumnType + " for \"" + dtColumn +
					"\" is not supported - don't understand date/time.";
				Message.printWarning(3, routine, message);
				throw new Exception(message);
			}
		}
		catch (SQLException e)
		{
			message = "Cannot determine column type for \"" + dtColumn + "\" don't understand date/time (" + e + ").";
			Message.printWarning(3, routine, message);
			throw new Exception(message);
		}
		string valColumn = getProperty(GenericDatabaseDataStore.TS_DATA_TABLE_VALUE_COLUMN_PROP);
		string flagColumn = getProperty(GenericDatabaseDataStore.TS_DATA_TABLE_FLAG_COLUMN_PROP);
		string idColumn = getProperty(GenericDatabaseDataStore.TS_DATA_TABLE_METAID_COLUMN_PROP);
		ss.addTable(dataTable);
		ss.addField(dtColumn);
		ss.addField(valColumn);
		if (!string.ReferenceEquals(flagColumn, null))
		{
			ss.addField(flagColumn);
		}
		ss.addOrderByClause(dtColumn);
		try
		{
			ss.addWhereClause(idColumn + " = " + tsMeta.getId());
		}
		catch (Exception e)
		{
			Message.printWarning(3, routine, "Error setting TimeSeriesMeta ID for query (" + e + ").");
		}
		if (inputStart != null)
		{
			try
			{
				ss.addWhereClause(dtColumn + " >= " + DMIUtil.formatDateTime(dmi, inputStart));
			}
			catch (Exception e)
			{
				Message.printWarning(3, routine, "Error setting input start for query (" + e + ").");
			}
		}
		if (inputEnd != null)
		{
			try
			{
				ss.addWhereClause(dtColumn + " <= " + DMIUtil.formatDateTime(dmi, inputEnd));
			}
			catch (Exception e)
			{
				Message.printWarning(3, routine, "Error setting input end for query (" + e + ").");
			}
		}
		string sqlString = ss.ToString();
		Message.printStatus(2,routine,"Select statement = " + sqlString);
		ResultSet rs = null;
		double d, value;
		System.DateTime dt;
		string s, flag = "";
		DateTime dateTime;
		int i;
		int index;
		IList<TimeSeriesData> tsdataList = new List<TimeSeriesData>();
		StopWatch selectTimer = new StopWatch();
		try
		{
			selectTimer.start();
			rs = dmi.dmiSelect(ss);
			selectTimer.stop();
			while (rs.next())
			{
				index = 1;
				if (dateTimeInt)
				{
					i = rs.getInt(index++);
					if (rs.wasNull())
					{
						continue;
					}
					else
					{
						dateTime = new DateTime(DateTime.PRECISION_YEAR);
						dateTime.setYear(i);
					}
				}
				else
				{
					dt = rs.getTimestamp(index++);
					if (rs.wasNull())
					{
						continue;
					}
					else
					{
						dateTime = new DateTime(dt);
					}
				}
				d = rs.getDouble(index++);
				if (rs.wasNull())
				{
					value = missing;
				}
				else
				{
					value = d;
				}
				if (!string.ReferenceEquals(flagColumn, null))
				{
					s = rs.getString(index);
					if (rs.wasNull())
					{
						flag = s;
					}
					else
					{
						flag = "";
					}
				}
				tsdataList.Add(new TimeSeriesData(dateTime,value,flag));
			}
		}
		catch (Exception e)
		{
			Message.printWarning(3, routine, "Error reading time series data from database with statement \"" + sqlString + "\" uniquetempvar.");
		}
		finally
		{
			DMI.closeResultSet(rs);
		}
		dataTimer.stop();
		// Process the data records into the time series
		StopWatch setTimer = new StopWatch();
		setTimer.start();
		if (inputStart != null)
		{
			ts.setDate1(inputStart);
			ts.setDate1Original(inputStart);
		}
		if (inputEnd != null)
		{
			ts.setDate2(inputEnd);
			ts.setDate1Original(inputEnd);
		}
		if (tsdataList.Count > 0)
		{
			ts.setDate1(tsdataList[0].getDateTime());
			ts.setDate1Original(tsdataList[0].getDateTime());
			ts.setDate2(tsdataList[tsdataList.Count - 1].getDateTime());
			ts.setDate2Original(tsdataList[tsdataList.Count - 1].getDateTime());
			ts.allocateDataSpace();
			foreach (TimeSeriesData tsdata in tsdataList)
			{
				if (string.ReferenceEquals(flagColumn, null))
				{
					ts.setDataValue(tsdata.getDateTime(), tsdata.getValue());
				}
				else
				{
					ts.setDataValue(tsdata.getDateTime(), tsdata.getValue(), tsdata.getFlag(), -1);
				}
			}
		}
		setTimer.stop();
		Message.printStatus(2,routine,"Read " + tsdataList.Count + " values for \"" + ts.getIdentifierString() + "\" metaID=" + tsMeta.getId() + " metatime=" + metaTimer.getMilliseconds() + "ms, selecttime=" + selectTimer.getMilliseconds() + "ms, datatime=" + dataTimer.getMilliseconds() + "ms, settime=" + setTimer.getMilliseconds() + "ms");
		return ts;
	}

	/// <summary>
	/// Read time series metadata for one time series. </summary>
	/// <returns> the time series metadata object, or null if not exactly 1 metadata records match. </returns>
	public virtual TimeSeriesMeta readTimeSeriesMeta(string locType, string locID, string dataSource, string dataType, string interval, string scenario)
	{
		string routine = "GenericDatabaseDataStore.readTimeSeriesMeta";
		DMI dmi = getDMI();
		// Create a statement to read the specific metadata record
		DMISelectStatement ss = new DMISelectStatement(dmi);
		string metaTable = getProperty(GenericDatabaseDataStore.TS_META_TABLE_PROP);
		string idColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_ID_COLUMN_PROP);
		string ltColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_LOCTYPE_COLUMN_PROP);
		string locIdColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_LOCATIONID_COLUMN_PROP);
		string sourceColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_DATASOURCE_COLUMN_PROP);
		string dtColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_DATATYPE_COLUMN_PROP);
		string intervalColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_DATAINTERVAL_COLUMN_PROP);
		string scenarioColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_SCENARIO_COLUMN_PROP);
		string descColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_DESCRIPTION_COLUMN_PROP);
		string unitsColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_UNITS_COLUMN_PROP);
		ss.addTable(metaTable);
		if (!string.ReferenceEquals(idColumn, null))
		{
			ss.addField(idColumn);
		}
		if (!string.ReferenceEquals(ltColumn, null))
		{
			ss.addField(ltColumn);
		}
		if (!string.ReferenceEquals(locIdColumn, null))
		{
			ss.addField(locIdColumn);
		}
		if (!string.ReferenceEquals(sourceColumn, null))
		{
			ss.addField(sourceColumn);
		}
		if (!string.ReferenceEquals(dtColumn, null))
		{
			ss.addField(dtColumn);
		}
		if (!string.ReferenceEquals(intervalColumn, null))
		{
			ss.addField(intervalColumn);
		}
		if (!string.ReferenceEquals(scenarioColumn, null))
		{
			ss.addField(scenarioColumn);
		}
		if (!string.ReferenceEquals(descColumn, null))
		{
			ss.addField(descColumn);
		}
		if (!string.ReferenceEquals(unitsColumn, null))
		{
			ss.addField(unitsColumn);
		}
		readTimeSeriesMetaAddWhere(ss,metaTable,ltColumn,locType);
		readTimeSeriesMetaAddWhere(ss,metaTable,locIdColumn,locID);
		readTimeSeriesMetaAddWhere(ss,metaTable,sourceColumn,dataSource);
		readTimeSeriesMetaAddWhere(ss,metaTable,dtColumn,dataType);
		readTimeSeriesMetaAddWhere(ss,metaTable,intervalColumn,interval);
		readTimeSeriesMetaAddWhere(ss,metaTable,scenarioColumn,scenario);
		string sqlString = ss.ToString();
		//Message.printStatus(2,routine,"Select statement = " + sqlString );
		ResultSet rs = null;
		long l, id = -1;
		string s, desc = "", units = "";
		int count = 0, i;
		try
		{
			rs = dmi.dmiSelect(ss);
			while (rs.next())
			{
				// Since the calling arguments include everything of interest, really only need the ID, units, and description from the query,
				// but jump through the arguments as of above
				// TODO SAM 2013-08-29 should request by column name
				++count;
				i = 0;
				if (!string.ReferenceEquals(idColumn, null))
				{
					l = rs.getLong(++i);
					if (!rs.wasNull())
					{
						id = l;
					}
				}
				if (!string.ReferenceEquals(ltColumn, null))
				{
					++i;
				}
				if (!string.ReferenceEquals(locIdColumn, null))
				{
					++i;
				}
				if (!string.ReferenceEquals(sourceColumn, null))
				{
					++i;
				}
				if (!string.ReferenceEquals(dtColumn, null))
				{
					++i;
				}
				if (!string.ReferenceEquals(intervalColumn, null))
				{
					++i;
				}
				if (!string.ReferenceEquals(scenarioColumn, null))
				{
					++i;
				}
				if (!string.ReferenceEquals(descColumn, null))
				{
					s = rs.getString(++i);
					if (!rs.wasNull())
					{
						desc = s;
					}
				}
				if (!string.ReferenceEquals(unitsColumn, null))
				{
					s = rs.getString(++i);
					if (!rs.wasNull())
					{
						units = s;
					}
				}
			}
		}
		catch (Exception e)
		{
			Message.printWarning(3, routine, "Error reading time series metadata from database with statement \"" + sqlString + "\" uniquetempvar.");
		}
		finally
		{
			DMI.closeResultSet(rs);
		}
		if (count != 1)
		{
			Message.printWarning(3, routine, "Expecting 1 time series meta object for \"" + sqlString + "\" but have " + count);
			return null;
		}
		if (id < 0)
		{
			return null;
		}
		else
		{
			return new TimeSeriesMeta(locType, locID, dataSource, dataType, interval, scenario, desc, units, id);
		}
	}

	/// <summary>
	/// Read location type strings for the data store, if time series support is configured.
	/// Not a lot of error checking is done because the data store should have been checked out by this point </summary>
	/// <param name="locID"> location ID to use as filter (ignored if blank or null) </param>
	/// <param name="locType"> location type to use as filter (ignored if blank or null) </param>
	/// <param name="dataType"> data type to use as filter (ignored if blank or null) </param>
	/// <param name="interval"> interval to use as filter (ignored if blank or null) </param>
	/// <param name="scenario"> scenario to use as filter (ignored if blank or null) </param>
	/// <returns> the list of interval strings by making a unique query of the  </returns>
	public virtual IList<string> readTimeSeriesMetaDataSourceList(string locType, string locID, string dataType, string interval, string scenario)
	{
		string routine = "GenericDatabaseDataStore.readDataSourceStrings";
		DMI dmi = getDMI();
		IList<string> dataSources = new List<string>();
		// Create a statement to read distinct data types from the time series metadata table
		DMISelectStatement ss = new DMISelectStatement(dmi);
		string metaTable = getProperty(GenericDatabaseDataStore.TS_META_TABLE_PROP);
		string ltColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_LOCTYPE_COLUMN_PROP);
		string idColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_LOCATIONID_COLUMN_PROP);
		string sourceColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_DATASOURCE_COLUMN_PROP);
		string dtColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_DATATYPE_COLUMN_PROP);
		string intervalColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_DATAINTERVAL_COLUMN_PROP);
		string scenarioColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_SCENARIO_COLUMN_PROP);
		ss.addTable(metaTable);
		ss.addField(sourceColumn);
		readTimeSeriesMetaAddWhere(ss,metaTable,ltColumn,locType);
		readTimeSeriesMetaAddWhere(ss,metaTable,idColumn,locID);
		// LocationType is what is being read so don't filter
		readTimeSeriesMetaAddWhere(ss,metaTable,dtColumn,dataType);
		readTimeSeriesMetaAddWhere(ss,metaTable,intervalColumn,interval);
		readTimeSeriesMetaAddWhere(ss,metaTable,scenarioColumn,scenario);
		ss.selectDistinct(true);
		ss.addOrderByClause(sourceColumn);
		string sqlString = ss.ToString();
		Message.printStatus(2,routine,"Running:" + sqlString);
		ResultSet rs = null;
		string s;
		try
		{
			rs = dmi.dmiSelect(ss);
			while (rs.next())
			{
				s = rs.getString(1);
				if (!rs.wasNull())
				{
					dataSources.Add(s);
				}
			}
		}
		catch (Exception e)
		{
			Message.printWarning(3, routine, "Error reading time series metadata from database with statement \"" + sqlString + "\" uniquetempvar.");
		}
		finally
		{
			DMI.closeResultSet(rs);
		}
		return dataSources;
	}

	/// <summary>
	/// Read data type strings for the data store, if time series support is configured.
	/// Not a lot of error checking is done because the data store should have been checked out by this point </summary>
	/// <param name="includeNotes"> if true, include notes in the data type strings, like "DataType - Note"
	/// (currently does nothing) </param>
	/// <param name="locType"> location type to use as filter (ignored if blank or null) </param>
	/// <param name="locID"> location ID to use as filter (ignored if blank or null) </param>
	/// <param name="dataSource"> data source to use as filter (ignored if blank or null) </param>
	/// <param name="interval"> interval to use as filter (ignored if blank or null) </param>
	/// <param name="scenario"> scenario to use as filter (ignored if blank or null) </param>
	/// <returns> the list of data type strings by making a unique query of the  </returns>
	public virtual IList<string> readTimeSeriesMetaDataTypeList(bool includeNotes, string locType, string locID, string dataSource, string interval, string scenario)
	{
		string routine = "GenericDatabaseDataStore.readTimeSeriesMetaDataTypeList";
		DMI dmi = getDMI();
		IList<string> dataTypes = new List<string>();
		// Create a statement to read distinct data types from the time series metadata table
		DMISelectStatement ss = new DMISelectStatement(dmi);
		string metaTable = getProperty(GenericDatabaseDataStore.TS_META_TABLE_PROP);
		string ltColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_LOCTYPE_COLUMN_PROP);
		string idColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_LOCATIONID_COLUMN_PROP);
		string sourceColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_DATASOURCE_COLUMN_PROP);
		string dtColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_DATATYPE_COLUMN_PROP);
		string intervalColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_DATAINTERVAL_COLUMN_PROP);
		string scenarioColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_SCENARIO_COLUMN_PROP);
		ss.addTable(metaTable);
		ss.addField(dtColumn);
		ss.selectDistinct(true);
		readTimeSeriesMetaAddWhere(ss,metaTable,ltColumn,locType);
		readTimeSeriesMetaAddWhere(ss,metaTable,idColumn,locID);
		readTimeSeriesMetaAddWhere(ss,metaTable,sourceColumn,dataSource);
		// Data type is what is being read so don't filter
		readTimeSeriesMetaAddWhere(ss,metaTable,intervalColumn,interval);
		readTimeSeriesMetaAddWhere(ss,metaTable,scenarioColumn,scenario);
		ss.addOrderByClause(dtColumn);
		string sqlString = ss.ToString();
		ResultSet rs = null;
		string s;
		try
		{
			rs = dmi.dmiSelect(ss);
			while (rs.next())
			{
				s = rs.getString(1);
				if (!rs.wasNull())
				{
					dataTypes.Add(s);
				}
			}
		}
		catch (Exception e)
		{
			Message.printWarning(3, routine, "Error reading time series metadata from database with statement \"" + sqlString + "\" uniquetempvar.");
		}
		finally
		{
			DMI.closeResultSet(rs);
		}
		return dataTypes;
	}

	/// <summary>
	/// Read interval strings for the data store, if time series support is configured.
	/// Not a lot of error checking is done because the data store should have been checked out by this point </summary>
	/// <param name="locType"> location type to use as filter (ignored if blank or null) </param>
	/// <param name="locID"> location ID to use as filter (ignored if blank or null) </param>
	/// <param name="dataSource"> data source to use as filter (ignored if blank or null) </param>
	/// <param name="dataType"> data type to use as filter (ignored if blank or null) </param>
	/// <param name="scenario"> scenario to use as filter (ignored if blank or null) </param>
	/// <returns> the list of distinct location ID strings from time series metadata </returns>
	public virtual IList<string> readTimeSeriesMetaIntervalList(string locType, string locID, string dataSource, string dataType, string scenario)
	{
		string routine = "GenericDatabaseDataStore.readIntervalStrings";
		DMI dmi = getDMI();
		IList<string> intervals = new List<string>();
		// Create a statement to read distinct data types from the time series metadata table
		DMISelectStatement ss = new DMISelectStatement(dmi);
		string metaTable = getProperty(GenericDatabaseDataStore.TS_META_TABLE_PROP);
		string ltColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_LOCTYPE_COLUMN_PROP);
		string idColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_LOCATIONID_COLUMN_PROP);
		string sourceColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_DATASOURCE_COLUMN_PROP);
		string dtColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_DATATYPE_COLUMN_PROP);
		string intervalColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_DATAINTERVAL_COLUMN_PROP);
		string scenarioColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_SCENARIO_COLUMN_PROP);
		ss.addTable(metaTable);
		ss.addField(intervalColumn);
		readTimeSeriesMetaAddWhere(ss,metaTable,ltColumn,locType);
		readTimeSeriesMetaAddWhere(ss,metaTable,idColumn,locID);
		readTimeSeriesMetaAddWhere(ss,metaTable,sourceColumn,dataSource);
		readTimeSeriesMetaAddWhere(ss,metaTable,dtColumn,dataType);
		// Interval is what is being read so don't filter
		readTimeSeriesMetaAddWhere(ss,metaTable,scenarioColumn,scenario);
		ss.selectDistinct(true);
		ss.addOrderByClause(intervalColumn);
		string sqlString = ss.ToString();
		Message.printStatus(2,routine,"Running:" + sqlString);
		ResultSet rs = null;
		string s;
		try
		{
			rs = dmi.dmiSelect(ss);
			while (rs.next())
			{
				s = rs.getString(1);
				if (!rs.wasNull())
				{
					intervals.Add(s);
				}
			}
		}
		catch (Exception e)
		{
			Message.printWarning(3, routine, "Error reading time series metadata from database with statement \"" + sqlString + "\" uniquetempvar.");
		}
		finally
		{
			DMI.closeResultSet(rs);
		}
		return intervals;
	}

	/// <summary>
	/// Read a list of TimeSeriesMeta for the specified criteria. </summary>
	/// <param name="dataType"> data type to use as filter (ignored if blank or null) </param>
	/// <param name="interval"> interval to use as filter (ignored if blank or null) </param>
	/// <param name="filterPanel"> panel that contains input filters where filter criteria for query are specified </param>
	/// <returns> list of TimeSeriesMeta matching the query criteria </returns>
	public virtual IList<TimeSeriesMeta> readTimeSeriesMetaList(string dataType, string interval, GenericDatabaseDataStore_TimeSeries_InputFilter_JPanel filterPanel)
	{
		IList<TimeSeriesMeta> metaList = new List<TimeSeriesMeta>();
		string routine = "GenericDatabaseDataStore.readTimeSeriesMetaList";
		DMI dmi = getDMI();
		// Create a statement to read the specific metadata record
		DMISelectStatement ss = new DMISelectStatement(dmi);
		string metaTable = getProperty(GenericDatabaseDataStore.TS_META_TABLE_PROP);
		string idColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_ID_COLUMN_PROP);
		string ltColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_LOCTYPE_COLUMN_PROP);
		string locIdColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_LOCATIONID_COLUMN_PROP);
		string sourceColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_DATASOURCE_COLUMN_PROP);
		string dtColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_DATATYPE_COLUMN_PROP);
		string intervalColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_DATAINTERVAL_COLUMN_PROP);
		string scenarioColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_SCENARIO_COLUMN_PROP);
		string descColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_DESCRIPTION_COLUMN_PROP);
		string unitsColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_UNITS_COLUMN_PROP);
		ss.addTable(metaTable);
		if (!string.ReferenceEquals(idColumn, null))
		{
			ss.addField(idColumn);
		}
		if (!string.ReferenceEquals(ltColumn, null))
		{
			ss.addField(ltColumn);
		}
		if (!string.ReferenceEquals(locIdColumn, null))
		{
			ss.addField(locIdColumn);
		}
		if (!string.ReferenceEquals(sourceColumn, null))
		{
			ss.addField(sourceColumn);
		}
		if (!string.ReferenceEquals(dtColumn, null))
		{
			ss.addField(dtColumn);
		}
		if (!string.ReferenceEquals(intervalColumn, null))
		{
			ss.addField(intervalColumn);
		}
		if (!string.ReferenceEquals(scenarioColumn, null))
		{
			ss.addField(scenarioColumn);
		}
		if (!string.ReferenceEquals(descColumn, null))
		{
			ss.addField(descColumn);
		}
		if (!string.ReferenceEquals(unitsColumn, null))
		{
			ss.addField(unitsColumn);
		}
		string locType = null;
		string locID = null;
		string dataSource = null;
		string scenario = null;
		readTimeSeriesMetaAddWhere(ss,metaTable,dtColumn,dataType);
		readTimeSeriesMetaAddWhere(ss,metaTable,intervalColumn,interval);
		IList<string> whereClauses = new List<string>();
		if (!string.ReferenceEquals(ltColumn, null))
		{
			whereClauses.Add(getWhereClauseStringFromInputFilter(dmi, filterPanel, metaTable + "." + ltColumn, true));
		}
		if (!string.ReferenceEquals(locIdColumn, null))
		{
			whereClauses.Add(getWhereClauseStringFromInputFilter(dmi, filterPanel, metaTable + "." + locIdColumn, true));
		}
		if (!string.ReferenceEquals(sourceColumn, null))
		{
			whereClauses.Add(getWhereClauseStringFromInputFilter(dmi, filterPanel, metaTable + "." + sourceColumn, true));
		}
		if (!string.ReferenceEquals(scenarioColumn, null))
		{
			whereClauses.Add(getWhereClauseStringFromInputFilter(dmi, filterPanel, metaTable + "." + scenarioColumn, true));
		}
		// Add user-specified filters, must be in the provided standard columns
		int i = 0;
		while (true)
		{
			++i;
			string propName = TS_META_TABLE_FILTER_PROP + "." + i;
			string propVal = getProperty(propName);
			if (string.ReferenceEquals(propVal, null))
			{
				// Done with filter properties
				break;
			}
			string[] parts = propVal.Split(",", true);
			if (parts.Length == 5)
			{
				string column = parts[1].Trim();
				if (!string.ReferenceEquals(column, null))
				{
					whereClauses.Add(getWhereClauseStringFromInputFilter(dmi, filterPanel, metaTable + "." + column, true));
				}
			}
		}
		try
		{
			ss.addWhereClauses(whereClauses);
		}
		catch (Exception e)
		{
			Message.printWarning(3, routine, "Error adding where clauses (" + e + ").");
		}
		string sqlString = ss.ToString();
		Message.printStatus(2, routine, "Running:  " + sqlString);
		ResultSet rs = null;
		long l, id = -1;
		string s, desc = "", units = "";
		int index;
		try
		{
			rs = dmi.dmiSelect(ss);
			while (rs.next())
			{
				index = 1;
				if (!string.ReferenceEquals(idColumn, null))
				{
					l = rs.getLong(index++);
					if (!rs.wasNull())
					{
						id = l;
					}
				}
				if (!string.ReferenceEquals(ltColumn, null))
				{
					s = rs.getString(index++);
					if (rs.wasNull())
					{
						locType = "";
					}
					else
					{
						locType = s;
					}
				}
				if (!string.ReferenceEquals(locIdColumn, null))
				{
					s = rs.getString(index++);
					if (rs.wasNull())
					{
						locID = "";
					}
					else
					{
						locID = s;
					}
				}
				if (!string.ReferenceEquals(sourceColumn, null))
				{
					s = rs.getString(index++);
					if (rs.wasNull())
					{
						dataSource = "";
					}
					else
					{
						dataSource = s;
					}
				}
				if (!string.ReferenceEquals(dtColumn, null))
				{
					s = rs.getString(index++);
					if (rs.wasNull())
					{
						dataType = "";
					}
					else
					{
						dataType = s;
					}
				}
				if (!string.ReferenceEquals(intervalColumn, null))
				{
					s = rs.getString(index++);
					if (rs.wasNull())
					{
						interval = "";
					}
					else
					{
						interval = s;
					}
				}
				if (!string.ReferenceEquals(scenarioColumn, null))
				{
					s = rs.getString(index++);
					if (rs.wasNull())
					{
						scenario = "";
					}
					else
					{
						scenario = s;
					}
				}
				if (!string.ReferenceEquals(descColumn, null))
				{
					s = rs.getString(index++);
					if (rs.wasNull())
					{
						desc = "";
					}
					else
					{
						desc = s;
					}
				}
				if (!string.ReferenceEquals(unitsColumn, null))
				{
					s = rs.getString(index++);
					if (rs.wasNull())
					{
						units = "";
					}
					else
					{
						units = s;
					}
				}
				metaList.Add(new TimeSeriesMeta(locType, locID, dataSource, dataType, interval, scenario, desc, units, id));
			}
		}
		catch (Exception e)
		{
			Message.printWarning(3, routine, "Error reading time series metadata from database with statement \"" + sqlString + "\" uniquetempvar.");
		}
		finally
		{
			DMI.closeResultSet(rs);
		}
		return metaList;
	}

	/// <summary>
	/// Read location ID strings for the data store, if time series features are configured.
	/// Not a lot of error checking is done because the data store should have been checked out by this point </summary>
	/// <param name="locType"> location type to use as filter (ignored if blank or null) </param>
	/// <param name="dataSource"> data source to use as filter (ignored if blank or null) </param>
	/// <param name="dataType"> data type to use as filter (ignored if blank or null) </param>
	/// <param name="interval"> interval to use as filter (ignored if blank or null) </param>
	/// <param name="scenario"> scenario to use as filter (ignored if blank or null) </param>
	/// <returns> the list of distinct location ID strings from time series metadata </returns>
	public virtual IList<string> readTimeSeriesMetaLocationIDList(string locType, string dataSource, string dataType, string interval, string scenario)
	{
		string routine = "GenericDatabaseDataStore.readLocationIDStrings";
		DMI dmi = getDMI();
		IList<string> locIDs = new List<string>();
		// Create a statement to read distinct data types from the time series metadata table
		DMISelectStatement ss = new DMISelectStatement(dmi);
		string metaTable = getProperty(GenericDatabaseDataStore.TS_META_TABLE_PROP);
		string ltColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_LOCTYPE_COLUMN_PROP);
		string idColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_LOCATIONID_COLUMN_PROP);
		string sourceColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_DATASOURCE_COLUMN_PROP);
		string dtColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_DATATYPE_COLUMN_PROP);
		string intervalColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_DATAINTERVAL_COLUMN_PROP);
		string scenarioColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_SCENARIO_COLUMN_PROP);
		ss.addTable(metaTable);
		ss.addField(idColumn);
		readTimeSeriesMetaAddWhere(ss,metaTable,ltColumn,locType);
		// LocationID is what is being read so don't filter
		readTimeSeriesMetaAddWhere(ss,metaTable,sourceColumn,dataSource);
		readTimeSeriesMetaAddWhere(ss,metaTable,dtColumn,dataType);
		readTimeSeriesMetaAddWhere(ss,metaTable,intervalColumn,interval);
		readTimeSeriesMetaAddWhere(ss,metaTable,scenarioColumn,scenario);
		ss.selectDistinct(true);
		ss.addOrderByClause(idColumn);
		string sqlString = ss.ToString();
		Message.printStatus(2,routine,"Running:" + sqlString);
		ResultSet rs = null;
		string s;
		try
		{
			rs = dmi.dmiSelect(ss);
			while (rs.next())
			{
				s = rs.getString(1);
				if (!rs.wasNull())
				{
					locIDs.Add(s);
				}
			}
		}
		catch (Exception e)
		{
			Message.printWarning(3, routine, "Error reading time series metadata from database with statement \"" + sqlString + "\" uniquetempvar.");
		}
		finally
		{
			DMI.closeResultSet(rs);
		}
		return locIDs;
	}

	/// <summary>
	/// Read location type strings for the data store, if time series features are configured.
	/// Not a lot of error checking is done because the data store should have been checked out by this point </summary>
	/// <param name="locID"> location ID to use as filter (ignored if blank or null) </param>
	/// <param name="dataSource"> data source to use as filter (ignored if blank or null) </param>
	/// <param name="dataType"> data type to use as filter (ignored if blank or null) </param>
	/// <param name="interval"> interval to use as filter (ignored if blank or null) </param>
	/// <param name="scenario"> scenario to use as filter (ignored if blank or null) </param>
	/// <returns> the list of interval strings by making a unique query of the  </returns>
	public virtual IList<string> readTimeSeriesMetaLocationTypeList(string locID, string dataSource, string dataType, string interval, string scenario)
	{
		string routine = "GenericDatabaseDataStore.readLocationTypeStrings";
		DMI dmi = getDMI();
		IList<string> locTypes = new List<string>();
		// Create a statement to read distinct data types from the time series metadata table
		DMISelectStatement ss = new DMISelectStatement(dmi);
		string metaTable = getProperty(GenericDatabaseDataStore.TS_META_TABLE_PROP);
		string ltColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_LOCTYPE_COLUMN_PROP);
		string idColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_LOCATIONID_COLUMN_PROP);
		string sourceColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_DATASOURCE_COLUMN_PROP);
		string dtColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_DATATYPE_COLUMN_PROP);
		string intervalColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_DATAINTERVAL_COLUMN_PROP);
		string scenarioColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_SCENARIO_COLUMN_PROP);
		ss.addTable(metaTable);
		ss.addField(ltColumn);
		// LocationType is what is being read so don't filter
		readTimeSeriesMetaAddWhere(ss,metaTable,idColumn,locID);
		readTimeSeriesMetaAddWhere(ss,metaTable,sourceColumn,dataSource);
		readTimeSeriesMetaAddWhere(ss,metaTable,dtColumn,dataType);
		readTimeSeriesMetaAddWhere(ss,metaTable,intervalColumn,interval);
		readTimeSeriesMetaAddWhere(ss,metaTable,scenarioColumn,scenario);
		ss.selectDistinct(true);
		ss.addOrderByClause(ltColumn);
		string sqlString = ss.ToString();
		Message.printStatus(2,routine,"Running:" + sqlString);
		ResultSet rs = null;
		string s;
		try
		{
			rs = dmi.dmiSelect(ss);
			while (rs.next())
			{
				s = rs.getString(1);
				if (!rs.wasNull())
				{
					locTypes.Add(s);
				}
			}
		}
		catch (Exception e)
		{
			Message.printWarning(3, routine, "Error reading time series metadata from database with statement \"" + sqlString + "\" uniquetempvar.");
		}
		finally
		{
			DMI.closeResultSet(rs);
		}
		return locTypes;
	}

	/// <summary>
	/// Utility method to add a where clause to the metadata select statement. </summary>
	/// <param name="ss"> select statement to execute </param>
	/// <param name="table"> table to query </param>
	/// <param name="column"> for where clause </param>
	/// <param name="value"> value to use in where clause </param>
	private void readTimeSeriesMetaAddWhere(DMISelectStatement ss, string table, string column, string value)
	{
		if ((!string.ReferenceEquals(value, null)) && !value.Equals("") && !value.Equals("*"))
		{
			try
			{
				ss.addWhereClause(table + "." + column + " = '" + value + "'");
			}
			catch (Exception)
			{
				// Should not happen
			}
		}
	}

	/// <summary>
	/// Read location type strings for the data store, if time series features are configured.
	/// Not a lot of error checking is done because the data store should have been checked out by this point </summary>
	/// <param name="locType"> location type to use as filter (ignored if blank or null) </param>
	/// <param name="locID"> location ID to use as filter (ignored if blank or null) </param>
	/// <param name="dataSource"> data source to use as filter (ignored if blank or null) </param>
	/// <param name="dataType"> data type to use as filter (ignored if blank or null) </param>
	/// <param name="interval"> interval to use as filter (ignored if blank or null) </param>
	/// <returns> the list of interval strings by making a unique query of the  </returns>
	public virtual IList<string> readTimeSeriesMetaScenarioList(string locType, string locID, string dataSource, string dataType, string interval)
	{
		string routine = "GenericDatabaseDataStore.readDataSourceStrings";
		DMI dmi = getDMI();
		IList<string> scenarios = new List<string>();
		// Create a statement to read distinct data types from the time series metadata table
		DMISelectStatement ss = new DMISelectStatement(dmi);
		string metaTable = getProperty(GenericDatabaseDataStore.TS_META_TABLE_PROP);
		string ltColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_LOCTYPE_COLUMN_PROP);
		string idColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_LOCATIONID_COLUMN_PROP);
		string sourceColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_DATASOURCE_COLUMN_PROP);
		string dtColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_DATATYPE_COLUMN_PROP);
		string intervalColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_DATAINTERVAL_COLUMN_PROP);
		string scenarioColumn = getProperty(GenericDatabaseDataStore.TS_META_TABLE_SCENARIO_COLUMN_PROP);
		ss.addTable(metaTable);
		ss.addField(scenarioColumn);
		readTimeSeriesMetaAddWhere(ss,metaTable,ltColumn,locType);
		readTimeSeriesMetaAddWhere(ss,metaTable,idColumn,locID);
		readTimeSeriesMetaAddWhere(ss,metaTable,sourceColumn,dataSource);
		readTimeSeriesMetaAddWhere(ss,metaTable,dtColumn,dataType);
		readTimeSeriesMetaAddWhere(ss,metaTable,intervalColumn,interval);
		// Scenario is what is being read so don't filter
		ss.selectDistinct(true);
		ss.addOrderByClause(scenarioColumn);
		string sqlString = ss.ToString();
		Message.printStatus(2,routine,"Running:" + sqlString);
		ResultSet rs = null;
		string s;
		try
		{
			rs = dmi.dmiSelect(ss);
			while (rs.next())
			{
				s = rs.getString(1);
				if (!rs.wasNull())
				{
					scenarios.Add(s);
				}
			}
		}
		catch (Exception e)
		{
			Message.printWarning(3, routine, "Error reading time series metadata from database with statement \"" + sqlString + "\" uniquetempvar.");
		}
		finally
		{
			DMI.closeResultSet(rs);
		}
		return scenarios;
	}

	}

}