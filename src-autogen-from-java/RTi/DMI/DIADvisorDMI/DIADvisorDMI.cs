using System;
using System.Collections.Generic;

// ----------------------------------------------------------------------------
// DIADvisorDMI.java - DMI to interact with DIADvisor database
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// Notes:
// (1) This class must override determineDatabaseVersion() and readGlobalData()
// in DMI.java
// ----------------------------------------------------------------------------
// History:
//
// 2003-02-04	Steven A. Malers, RTi	Initial version, based on
//					RiversideDB_DMI.
// 2003-03-30	SAM, RTi		* Add SysConfig to data objects.
//					* Add __operational_DMI and
//					  __archive_DMI to allow queries to the
//					  related database, when needed.
// 2003-06-16	SAM, RTi		* Update to use the new TS package
//					  (DateTime instead of TSDate, etc.).
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------
//EndHeader

namespace RTi.DMI.DIADvisorDMI
{



	using DayTS = RTi.TS.DayTS;
	using HourTS = RTi.TS.HourTS;
	using IrregularTS = RTi.TS.IrregularTS;
	using MinuteTS = RTi.TS.MinuteTS;
	using TS = RTi.TS.TS;
	using TSIdent = RTi.TS.TSIdent;

	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using DateTime = RTi.Util.Time.DateTime;
	using TimeInterval = RTi.Util.Time.TimeInterval;

	/// <summary>
	/// The DIADvisorDMI provides an interface to the DIADvisor database.
	/// 
	/// <b>SQL Method Naming Conventions</b><para>
	/// 
	/// The first word in the method name is one of the following:<br>
	/// <ol>
	/// <li>read</li>
	/// <li>write</li>
	/// <li>delete</li>
	/// <li>count</li>
	/// </ol>
	/// 
	/// The second part of the method name is the data object being operated on.
	/// If a list is returned, then "List" is included in the method name.
	/// Finally, if a select based on a where clause is used, the method includes the
	/// field for the Where.  Examples are:
	/// 
	/// <ol>
	/// <li>	readSensorDefList</li>
	/// <li>	readSensorDefForSensorID_num</li>
	/// </ol>
	/// 
	/// </para>
	/// <para>
	/// <b>Notes on versioning:</b><br>
	/// Version changes require changes throughout the code base.  The following
	/// example tells all the changes that need to be made when a new field is
	/// </para>
	/// added to an existing table:<para>
	/// <ul>
	/// <li>in buildSQL(), add the new field to all the select and write statement
	/// sections for the appropriate table.  Do not forget to wrap this new code
	/// with tests for the proper version (DMI.isDatabaseVersionAtLeast())</li>
	/// <li>if, for the table XXXX, a method exists like:<br>
	/// <code>private Vector toXXXXList</code><br>
	/// then add the field to the Vector-filling code in this method</li>
	/// <li>go to the RiversideDB_XXXX.java object that represents the table for
	/// which this field was added.  Add the data member for the field, 
	/// get/set statements, and then add the field (with brief information on the
	/// version in which it was added) to the toString()</li>
	/// <li>add the field, and the appropriate version-checking code, to the 
	/// writeXXXX() method</li>
	/// <li>update determineDatabaseVersion()</li>
	/// </ul>
	/// </para>
	/// </summary>
	public class DIADvisorDMI : DMI
	{

	/// <summary>
	/// DIADvisor version for 2.61 as of ????-??-??, including the
	/// following design elements:  NEED SOMETHING FROM DIAD
	/// </summary>
	public const long VERSION_026100_00000000 = 26120030330L;
	public const long VERSION_LATEST = VERSION_026100_00000000;

	// Alphabetize the following by table.  Make sure values to not overlap.

	/// <summary>
	/// Select DataChron, returning a Vector of DIADvisor_DataChron
	/// </summary>
	protected internal readonly int _S_DATACHRON = 50;

	/// <summary>
	/// Select GroupDef, returning a Vector of DIADvisor_GroupDef
	/// </summary>
	protected internal readonly int _S_GROUPDEF = 60;

	/// <summary>
	/// Select SensorDef, returning a Vector of DIADvisor_SensorDef
	/// </summary>
	protected internal readonly int _S_SENSOR_DEF = 100;

	/// <summary>
	/// Select SiteDef, returning a Vector of DIADvisor_SiteDef
	/// </summary>
	protected internal readonly int _S_SITE_DEF = 200;

	/// <summary>
	/// Select SysConfig, returning a single DIADvisor_SysConfig
	/// </summary>
	protected internal readonly int _S_SYS_CONFIG = 300;

	/// <summary>
	/// References to the operational and archive databases.  This is needed because
	/// DIADvisor has two databases, each with an ODBC connection.  All database
	/// interaction is assumed to occur with the operational datbase, with the archive
	/// DMI used only when reading time series from the archive database.
	/// </summary>
	internal DIADvisorDMI __dmi_operational = null;
	internal DIADvisorDMI __dmi_archive = null;

	/// <summary>
	/// Constructor for a predefined ODBC DSN. </summary>
	/// <param name="database_engine"> The database engine to use (see the DMI constructor). </param>
	/// <param name="odbc_name"> The ODBC DSN that has been defined on the machine. </param>
	/// <param name="system_login"> If not null, this is used as the system login to make the
	/// connection.  If null, the default system login is used. </param>
	/// <param name="system_password"> If not null, this is used as the system password to make
	/// the connection.  If null, the default system password is used. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public DIADvisorDMI(String database_engine, String odbc_name, String system_login, String system_password) throws Exception
	public DIADvisorDMI(string database_engine, string odbc_name, string system_login, string system_password) : base(database_engine, odbc_name, system_login, system_password)
	{ // Use the default system login and password
		if (string.ReferenceEquals(system_login, null))
		{
			// Use the default...
			setSystemLogin("diad");
		}
		if (string.ReferenceEquals(system_password, null))
		{
			// Use the default...
			setSystemPassword("lena");
		}
		setEditable(true);
		setSecure(false);
	}

	/// <summary>
	/// Constructor for a database server and database name, to use an automatically
	/// created URL. </summary>
	/// <param name="database_engine"> The database engine to use (see the DMI constructor). </param>
	/// <param name="database_server"> The IP address or DSN-resolvable database server
	/// machine name. </param>
	/// <param name="database_name"> The database name on the server.  If null, default to
	/// "RiversideDB". </param>
	/// <param name="port"> Port number used by the database.  If negative, default to that for
	/// the database engine. </param>
	/// <param name="system_login"> If not null, this is used as the system login to make the
	/// connection.  If null, the default system login is used. </param>
	/// <param name="system_password"> If not null, this is used as the system password to make
	/// the connection.  If null, the default system password is used. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public DIADvisorDMI(String database_engine, String database_server, String database_name, int port, String system_login, String system_password) throws Exception
	public DIADvisorDMI(string database_engine, string database_server, string database_name, int port, string system_login, string system_password) : base(database_engine, database_server, database_name, port, system_login, system_password)
	{ // Use the default system login and password
		if (string.ReferenceEquals(system_login, null))
		{
			// Use the default...
			setSystemLogin("diad");
		}
		if (string.ReferenceEquals(system_password, null))
		{
			// Use the default...
			setSystemPassword("lena");
		}
		setEditable(true);
		setSecure(false);
	}

	/// <summary>
	/// Build an SQL string based on a requested SQL statement code.  This defines 
	/// the basic statement and allows overloaded methods to avoid redundant code.
	/// This method is used to eliminate redundant code where methods use the same
	/// basic statement but with different where clauses. </summary>
	/// <param name="statement"> Statement to set values in. </param>
	/// <param name="sqlNumber"> the number of the SQL statement to build.  Usually defined
	/// as a private constant as a mnemonic aid. </param>
	/// <returns> a string containing the full SQL. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void buildSQL(RTi.DMI.DMIStatement statement, int sqlNumber) throws Exception
	private void buildSQL(DMIStatement statement, int sqlNumber)
	{
		DMISelectStatement select;
		//DMIWriteStatement write;	// Probably don't need
		//DMIDeleteStatement del;
		string leftIdDelim = getFieldLeftEscape();
		string rightIdDelim = getFieldRightEscape();
		switch (sqlNumber)
		{
			case _S_DATACHRON:
				select = (DMISelectStatement)statement;
				select.addField("DataChron." + leftIdDelim + "Date/Time" + rightIdDelim);
				select.addField("DataChron." + leftIdDelim + "Sensor ID" + rightIdDelim);
				select.addField("DataChron.Count");
				select.addField("DataChron." + leftIdDelim + "Data Type" + rightIdDelim);
				select.addField("DataChron.Source");
				select.addField("DataChron." + leftIdDelim + "Data Value" + rightIdDelim);
				select.addField("DataChron." + leftIdDelim + "Data Value 2" + rightIdDelim);
				select.addField("DataChron.SeqNum");
				select.addField("DataChron.Comment");
				select.addTable("DataChron");
				break;
			case _S_GROUPDEF:
				select = (DMISelectStatement)statement;
				select.addField("GroupDef.Group");
				select.addField("GroupDef.Operation");
				select.addField("GroupDef.Units1");
				select.addField("GroupDef.Units2");
				select.addField("GroupDef.Display");
				select.addTable("GroupDef");
				break;
			case _S_SENSOR_DEF:
				select = (DMISelectStatement)statement;
				select.addField("SensorDef." + leftIdDelim + "Sensor ID" + rightIdDelim);
				select.addField("SensorDef." + leftIdDelim + "Site ID" + rightIdDelim);
				select.addField("SensorDef.Type");
				select.addField("SensorDef.Group");
				select.addField("SensorDef.Description");
				select.addField("SensorDef." + leftIdDelim + "Max Count" + rightIdDelim);
				select.addField("SensorDef." + leftIdDelim + "Min Count" + rightIdDelim);
				select.addField("SensorDef." + leftIdDelim + "Pos Delta" + rightIdDelim);
				select.addField("SensorDef." + leftIdDelim + "Neg Delta" + rightIdDelim);
				select.addField("SensorDef." + leftIdDelim + "Rating Type" + rightIdDelim);
				select.addField("SensorDef." + leftIdDelim + "Rating Interpolation" + rightIdDelim);
				select.addField("SensorDef." + leftIdDelim + "Rating Shift" + rightIdDelim);
				select.addField("SensorDef." + leftIdDelim + "Calibration Offset" + rightIdDelim);
				select.addField("SensorDef." + leftIdDelim + "Calibration Date" + rightIdDelim);
				select.addField("SensorDef.Slope");
				select.addField("SensorDef." + leftIdDelim + "Reference Level" + rightIdDelim);
				select.addField("SensorDef." + leftIdDelim + "Display Units" + rightIdDelim);
				select.addField("SensorDef.Decimal");
				select.addField("SensorDef." + leftIdDelim + "Display Units 2" + rightIdDelim);
				select.addField("SensorDef." + leftIdDelim + "Decimal 2" + rightIdDelim);
				select.addField("SensorDef." + leftIdDelim + "In Service" + rightIdDelim);
				select.addField("SensorDef.Suspect");
				select.addField("SensorDef.Alarms");
				select.addField("SensorDef.Notify");
				select.addField("SensorDef.Timeout");
				select.addField("SensorDef.Children");
				select.addField("SensorDef." + leftIdDelim + "Most Recent Time" + rightIdDelim);
				select.addField("SensorDef." + leftIdDelim + "Most Recent Data" + rightIdDelim);
				select.addField("SensorDef." + leftIdDelim + "Last Valid Time" + rightIdDelim);
				select.addField("SensorDef." + leftIdDelim + "Last Valid Data" + rightIdDelim);
				select.addField("SensorDef." + leftIdDelim + "Last Count" + rightIdDelim);
				select.addField("SensorDef.Equation");
				select.addField("SensorDef." + leftIdDelim + "Equation 2" + rightIdDelim);
				select.addTable("SensorDef");
				break;
			case _S_SITE_DEF:
				select = (DMISelectStatement)statement;
				select.addField("SiteDef.SiteName");
				select.addField("SiteDef." + leftIdDelim + "Site ID" + rightIdDelim);
				select.addField("SiteDef.Latitude");
				select.addField("SiteDef.Longitude");
				select.addField("SiteDef.XCoord");
				select.addField("SiteDef.YCoord");
				select.addField("SiteDef.PKey");
				select.addField("SiteDef." + leftIdDelim + "Repeater Group" + rightIdDelim);
				select.addField("SiteDef.Elevation");
				select.addField("SiteDef." + leftIdDelim + "Site Picture" + rightIdDelim);
				select.addField("SiteDef.Zone");
				select.addField("SiteDef.FIPS");
				select.addField("SiteDef.LastUpdate");
				select.addTable("SiteDef");
				break;
			case _S_SYS_CONFIG:
				select = (DMISelectStatement)statement;
				select.addField("SysConfig.Interval");
				select.addTable("SysConfig");
				break;
			default:
				Message.printWarning(2, "DIADvisorDMI.buildSQL", "Unknown statement code: " + sqlNumber);
				break;
		}
	}

	/// <summary>
	/// Determine the database version by examining the table structure for the
	/// database.  The following versions are known for DIADvisor:
	/// <ul>
	/// <li>	02610000000000 - Current version used for testing new features.</li>
	/// </ul>
	/// </summary>
	public override void determineDatabaseVersion()
	{ // Assume this...
		setDatabaseVersion(VERSION_026100_00000000);
		Message.printStatus(1, "DIADvisorDMI.determineDatabaseVersion", "DIADvisor version determined to be at least " + getDatabaseVersion());
	}

	/// <summary>
	/// Clean up for garbage collection. </summary>
	/// <exception cref="Throwable"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~DIADvisorDMI()
	{
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Read records from the DataChron table.
	/// This method is called from readTimeSeries(). </summary>
	/// <returns> a vector of objects of type DIADvisor_DataChron. </returns>
	/// <param name="sensorid"> Sensor ID for data records. </param>
	/// <param name="req_date1"> Requested first date/time (can be null). </param>
	/// <param name="req_date2"> Requested last date/time (can be null). </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.util.List readDataChronListForSensorIDAndPeriod(String sensorid, RTi.Util.Time.DateTime req_date1, RTi.Util.Time.DateTime req_date2) throws Exception
	public virtual System.Collections.IList readDataChronListForSensorIDAndPeriod(string sensorid, DateTime req_date1, DateTime req_date2)
	{
		DMISelectStatement q = new DMISelectStatement(this);
		// Build the SQL here because it depends on the table...
		q = new DMISelectStatement(this);
		buildSQL(q, _S_DATACHRON);
		// Primary key is the sensor ID...
		q.addWhereClause("DataChron." + getFieldLeftEscape() + "Sensor ID" + getFieldRightEscape() + "=" + sensorid);
		// If req_date1 is specified, use it...
		if (req_date1 != null)
		{
			q.addWhereClause("DataChron.StartTime >= " + DMIUtil.formatDateTime(this, req_date1));
		}
		// If req_date2 is specified, use it...
		if (req_date2 != null)
		{
			q.addWhereClause("DataChron.StartTime <= " + DMIUtil.formatDateTime(this, req_date2));
		}
		// Also add wheres for the start and end dates if they are specified...
		ResultSet rs = dmiSelect(q);
		System.Collections.IList v = toDataChronList(rs);
		rs.close();
		return v;
	}

	/// <summary>
	/// Read global data that should be kept in memory to increase performance.
	/// This is called from the DMI.open() base class method.
	/// throws SQLException if there is an error reading the data.
	/// throws Exception thrown in readTablesList()
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void readGlobalData() throws java.sql.SQLException, Exception
	public override void readGlobalData()
	{
		// Read the Tables table.
		// Nothing here yet.
	}

	/// <summary>
	/// Read the GroupDef records. </summary>
	/// <returns> a vector of objects of type DIADvisor_GroupDef. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.util.List readGroupDefList() throws Exception
	public virtual System.Collections.IList readGroupDefList()
	{
		DMISelectStatement q = new DMISelectStatement(this);
		buildSQL(q, _S_GROUPDEF);
		ResultSet rs = dmiSelect(q);
		System.Collections.IList v = toGroupDefList(rs);
		rs.close();
		return v;
	}

	/// <summary>
	/// Read records from the regular interval time series tables:  hour, day, interval.
	/// This method is called from readTimeSeries(). </summary>
	/// <returns> a vector of objects of type DIADvisor_RegularTSRecord. </returns>
	/// <param name="ts_table"> Table from which to read the time series records. </param>
	/// <param name="sensorid"> Sensor ID for data records. </param>
	/// <param name="req_date1"> Requested first date/time (can be null). </param>
	/// <param name="req_date2"> Requested last date/time (can be null). </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private java.util.List readRegularTSRecordList(String ts_table, String sensorid, RTi.Util.Time.DateTime req_date1, RTi.Util.Time.DateTime req_date2) throws Exception
	private System.Collections.IList readRegularTSRecordList(string ts_table, string sensorid, DateTime req_date1, DateTime req_date2)
	{
		DMISelectStatement q = new DMISelectStatement(this);
		// Build the SQL here because it depends on the table...
		q = new DMISelectStatement(this);
		q.addTable(ts_table);
		q.addField(ts_table + ".StartTime");
		q.addField(ts_table + "." + getFieldLeftEscape() + "Sensor ID" + getFieldRightEscape());
		q.addField(ts_table + ".Value");
		q.addField(ts_table + ".Count");
		// Primary key is the sensor ID...
		q.addWhereClause(ts_table + "." + getFieldLeftEscape() + "Sensor ID" + getFieldRightEscape() + "=" + sensorid);
		// If req_date1 is specified, use it...
		if (req_date1 != null)
		{
			q.addWhereClause(ts_table + ".StartTime >= " + DMIUtil.formatDateTime(this, req_date1));
		}
		// If req_date2 is specified, use it...
		if (req_date2 != null)
		{
			q.addWhereClause(ts_table + ".StartTime <= " + DMIUtil.formatDateTime(this, req_date2));
		}
		// Also add wheres for the start and end dates if they are specified...
		ResultSet rs = dmiSelect(q);
		System.Collections.IList v = toRegularTSRecordList(rs);
		rs.close();
		return v;
	}

	/// <summary>
	/// Read SensorDef records for distinct group. </summary>
	/// <returns> a vector of objects of type DIADvisor_SensorDef. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.util.List readSensorDefListForDistinctGroup() throws Exception
	public virtual System.Collections.IList readSensorDefListForDistinctGroup()
	{
		DMISelectStatement q = new DMISelectStatement(this);
		// Select from SensorDef
		q.addField("SensorDef.Group");
		q.addTable("SensorDef");
		q.selectDistinct(true);
		q.addOrderByClause("SensorDef.Group");
		ResultSet rs = dmiSelect(q);
		// Transfer here instead of the toSensorDefList() method...
		System.Collections.IList v = new List<object>();
		int index = 1;
		string s;
		DIADvisor_SensorDef data = null;
		while (rs.next())
		{
			data = new DIADvisor_SensorDef();
			s = rs.getString(index);
			if (!rs.wasNull())
			{
				data.setGroup(s.Trim());
			}
			v.Add(data);
		}
		rs.close();
		return v;
	}

	/// <summary>
	/// Read a SensorDef record for the given sensor id. </summary>
	/// <param name="SensorId"> Sensor id for sensor that is to be queried. </param>
	/// <returns> a single DIADvisor_SensorDef. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public DIADvisor_SensorDef readSensorDefForSensorID(int SensorId) throws Exception
	public virtual DIADvisor_SensorDef readSensorDefForSensorID(int SensorId)
	{
		DMISelectStatement q = new DMISelectStatement(this);
		buildSQL(q, _S_SENSOR_DEF);
		q.addWhereClause("SensorDef." + getFieldLeftEscape() + "Sensor ID" + getFieldRightEscape() + "=" + SensorId);
		ResultSet rs = dmiSelect(q);
		System.Collections.IList v = toSensorDefList(rs);
		rs.close();
		if ((v == null) || (v.Count < 1))
		{
			return null;
		}
		else
		{
			return (DIADvisor_SensorDef)v[0];
		}
	}

	/// <summary>
	/// Read the SensorDef records. </summary>
	/// <returns> a vector of objects of type DIADvisor_SensorDef. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.util.List readSensorDefList() throws Exception
	public virtual System.Collections.IList readSensorDefList()
	{
		DMISelectStatement q = new DMISelectStatement(this);
		buildSQL(q, _S_SENSOR_DEF);
		ResultSet rs = dmiSelect(q);
		System.Collections.IList v = toSensorDefList(rs);
		rs.close();
		return v;
	}

	/// <summary>
	/// Read the SiteDef records. </summary>
	/// <returns> a vector of objects of type DIADvisor_SiteDef. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.util.List readSiteDefList() throws Exception
	public virtual System.Collections.IList readSiteDefList()
	{
		DMISelectStatement q = new DMISelectStatement(this);
		buildSQL(q, _S_SITE_DEF);
		//q.addWhereClause ( "Area.MeasLoc_num=" + MeasLoc_num ); 
		ResultSet rs = dmiSelect(q);
		System.Collections.IList v = toSiteDefList(rs);
		rs.close();
		return v;
	}

	/// <summary>
	/// Read the SysConfig record. </summary>
	/// <returns> the DIADvisor_SysConfig corresponding to the single record in the
	/// SysConfig table. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public DIADvisor_SysConfig readSysConfig() throws Exception
	public virtual DIADvisor_SysConfig readSysConfig()
	{
		DMISelectStatement q = new DMISelectStatement(this);
		buildSQL(q, _S_SYS_CONFIG);
		ResultSet rs = dmiSelect(q);
		System.Collections.IList v = toSysConfigList(rs);
		rs.close();
		if ((v == null) || (v.Count == 0))
		{
			return null;
		}
		else
		{
			return (DIADvisor_SysConfig)v[0];
		}
	}

	/// <summary>
	/// Read a time series matching a time series identifier. </summary>
	/// <returns> a time series or null. </returns>
	/// <param name="tsident_string"> TSIdent string indentifying the time series. </param>
	/// <param name="req_date1"> Optional date to specify the start of the query (specify 
	/// null to read the entire time series). </param>
	/// <param name="req_date2"> Optional date to specify the end of the query (specify 
	/// null to read the entire time series). </param>
	/// <param name="req_units"> requested data units (specify null or blank string to 
	/// return units from the database). </param>
	/// <param name="read_data"> Indicates whether data should be read (specify false to 
	/// only read header information). </param>
	/// <exception cref="if"> there is an error reading the time series. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public RTi.TS.TS readTimeSeries(String tsident_string, RTi.Util.Time.DateTime req_date1, RTi.Util.Time.DateTime req_date2, String req_units, boolean read_data) throws Exception
	public virtual TS readTimeSeries(string tsident_string, DateTime req_date1, DateTime req_date2, string req_units, bool read_data)
	{
		string routine = "DIADvisorDMI.readTimeSeries";
		// Create a TSIdent to get at the individual parts...
		TSIdent tsident = new TSIdent(tsident_string);
		string interval = tsident.getInterval();
		TS ts = null; // Time series being read
		string table; // Table from which to read data
		bool is_regular = true; // Indicates whether a regular or
						// irregular time series interval.
		bool is_datavalue1 = true; // Indicates whether DataValue1 or
						// DataValue2 is used - to increase
						// performance.
		try
		{
		if (StringUtil.indexOfIgnoreCase(tsident.getInterval(), "DataValue2",0) >= 0)
		{
			is_datavalue1 = false;
		}
		if (tsident.getInterval().regionMatches(true,0,"irreg",0,5))
		{
			// Read from Datachron...
			ts = new IrregularTS();
			table = "DataChron";
			is_regular = false;
		}
		else
		{ // Read from Day, Hour, or Interval...
			if (interval.Equals("Hour", StringComparison.OrdinalIgnoreCase))
			{
				ts = new HourTS();
				ts.setDataInterval(TimeInterval.HOUR, 1);
				table = "Hour";
			}
			else if (interval.Equals("Day", StringComparison.OrdinalIgnoreCase))
			{
				ts = new DayTS();
				ts.setDataInterval(TimeInterval.DAY, 1);
				table = "Day";
			}
			else
			{ // Assume minutes...
				ts = new MinuteTS();
				ts.setDataInterval(TimeInterval.MINUTE, tsident.getIntervalMult());
				table = "Interval";
			}
		}
		ts.setIdentifier(tsident);

		// Read the sensor for more information...

		DIADvisor_SensorDef sensordef = __dmi_operational.readSensorDefForSensorID(StringUtil.atoi(tsident.getLocation()));
		if (sensordef == null)
		{
			Message.printWarning(2, routine, "Sensor " + tsident.getLocation() + " not found in DIADvisor for TS \"" + tsident_string + "\"");
		}

		ts.setDescription(sensordef.getDescription());
		if (StringUtil.indexOfIgnoreCase(tsident.getType(),"DataValue2",0) >= 0)
		{
			ts.setDataUnits(sensordef.getDisplayUnits2());
			ts.setDataUnitsOriginal(sensordef.getDisplayUnits2());
		}
		else
		{
			ts.setDataUnits(sensordef.getDisplayUnits());
			ts.setDataUnitsOriginal(sensordef.getDisplayUnits());
		}
		if (req_date1 != null)
		{
			ts.setDate1(req_date1);
		}
		if (req_date2 != null)
		{
			ts.setDate2(req_date2);
		}
		// SAMX - problem here - in order to read the header and get the dates,
		// we really need to get the dates from somewhere.
		if (!read_data)
		{
			return ts;
		}
		// Read the data...
		System.Collections.IList data_records = null;
		if (is_regular)
		{
			if (tsident.getScenario().Equals(""))
			{
				data_records = __dmi_operational.readRegularTSRecordList(table, tsident.getLocation(), req_date1, req_date2);
			}
			else
			{
				data_records = __dmi_archive.readRegularTSRecordList(table, tsident.getLocation(), req_date1, req_date2);
			}
		}
		else
		{
			if (tsident.getScenario().Equals(""))
			{
				data_records = __dmi_operational.readDataChronListForSensorIDAndPeriod(tsident.getLocation(), req_date1, req_date2);
			}
			else
			{
				data_records = __dmi_archive.readDataChronListForSensorIDAndPeriod(tsident.getLocation(), req_date1, req_date2);
			}
		}
		DIADvisor_RegularTSRecord reg_data = null;
		DIADvisor_DataChron irreg_data = null;
		int size = 0;
		if (data_records != null)
		{
			size = data_records.Count;
		}
		if (size == 0)
		{
			// Return null because there are no data to set dates.
			// This prevents problems in graphing and other code
			// where dates are required.
			return null;
		}

		if ((req_date1 != null) && (req_date2 != null))
		{
			// Allocate the memory regardless of whether there was
			// data.  If no data have been found then missing data
			// will be initialized...
			ts.setDate1(req_date1);
			ts.setDate1Original(req_date1);
			ts.setDate2(req_date2);
			ts.setDate2Original(req_date2);
			ts.allocateDataSpace();
		}
		else if (size > 0)
		{
			// Set the date from the records...
			if (is_regular)
			{
				reg_data = (DIADvisor_RegularTSRecord) data_records[0];
				ts.setDate1(new DateTime(reg_data._StartTime));
				ts.setDate1Original(new DateTime(reg_data._StartTime));

				reg_data = (DIADvisor_RegularTSRecord) data_records[size - 1];
				ts.setDate2(new DateTime(reg_data._StartTime));
				ts.setDate2Original(new DateTime(reg_data._StartTime));
				ts.allocateDataSpace();
			}
			else
			{
				irreg_data = (DIADvisor_DataChron) data_records[0];
				ts.setDate1(new DateTime(irreg_data._DateTime));
				ts.setDate1Original(new DateTime(irreg_data._DateTime));

				irreg_data = (DIADvisor_DataChron) data_records[size - 1];
				ts.setDate2(new DateTime(irreg_data._DateTime));
				ts.setDate2Original(new DateTime(irreg_data._DateTime));
				ts.allocateDataSpace();
			}
		}

		// The date needs to be the correct precision so assign from the TS
		// start date (the precision is adjusted when dates are set)...

		DateTime date = new DateTime(ts.getDate1());

		double val = 0.0;
		string flag = "";
		if (tsident.getInterval().regionMatches(true,0,"irreg",0,5))
		{
			// Loop through and assign the data...
			for (int i = 0; i < size; i++)
			{
				// Set the date rather than declaring a new instance
				// to increase performance...
				if (is_regular)
				{
					reg_data = (DIADvisor_RegularTSRecord) data_records[i];
					date.setDate(reg_data._StartTime);
					// Adjust so the time is the interval-ending
					// time (DIADvisor uses the interval start,
					// which is not consistent with other RTi
					// tools...
					// Also take the value from the one possible
					// value (for DataValue1)...
					val = reg_data._Value;
				}
				else
				{ // Since DataChron is irregular, the time is
					// used as is.   Get the value depending on
					// whether DataValue1 or DataValue2 are used...
					irreg_data = (DIADvisor_DataChron) data_records[i];
					date.setDate(irreg_data._DateTime);
					flag = irreg_data._DataType;
					if (is_datavalue1)
					{
						val = irreg_data._DataValue;
					}
					else
					{
						val = irreg_data._DataValue2;
					}
				}
				if (is_regular)
				{
					ts.setDataValue(date, val);
				}
				else
				{ // Also set the data flag...
					ts.setDataValue(date, val, flag, 0);
				}
			}
		}
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, e);
			Message.printWarning(2, routine, "Last op SQL: " + __dmi_operational.getLastSQLString());
			Message.printWarning(2, routine, "Last archive SQL: " + __dmi_operational.getLastSQLString());
		}
		return ts;
	}

	/// <summary>
	/// Set the DIADvisorDMI corresponding to the archive database. </summary>
	/// <param name="dmi"> DIADvisorDMI corresponding to the DIADvisor archive database. </param>
	public virtual void setArchiveDMI(DIADvisorDMI dmi)
	{
		__dmi_archive = dmi;
	}

	/// <summary>
	/// Set the DIADvisorDMI corresponding to the operational database. </summary>
	/// <param name="dmi"> DIADvisorDMI corresponding to the DIADvisor operational database. </param>
	public virtual void setOperationalDMI(DIADvisorDMI dmi)
	{
		__dmi_operational = dmi;
	}

	/// <summary>
	/// Convert a ResultSet to a Vector of DIADvisor_DataChron. </summary>
	/// <param name="rs"> ResultSet from a DataChron table query.
	/// query. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private java.util.List toDataChronList(java.sql.ResultSet rs) throws Exception
	private System.Collections.IList toDataChronList(ResultSet rs)
	{
		System.Collections.IList v = new List<object>();
		int index = 1;
		double d;
		int i;
		string s;
		System.DateTime date;
		DIADvisor_DataChron data = null;
		while (rs.next())
		{
			data = new DIADvisor_DataChron();
			index = 1;
			date = rs.getTimestamp(index++);
			if (rs.wasNull())
			{
				// Absolutely need a date!
				continue;
			}
			data.setDateTime(date);
			i = rs.getInt(index++);
			if (!rs.wasNull())
			{
				data.setSensorID(i);
			}
			i = rs.getInt(index++);
			if (!rs.wasNull())
			{
				data.setCount(i);
			}
			s = rs.getString(index++);
			if (!rs.wasNull())
			{
				data.setDataType(s.Trim());
			}
			s = rs.getString(index++);
			if (!rs.wasNull())
			{
				data.setSource(s.Trim());
			}
			d = rs.getDouble(index++);
			if (!rs.wasNull())
			{
				data.setDataValue(d);
			}
			d = rs.getDouble(index++);
			if (!rs.wasNull())
			{
				data.setDataValue2(d);
			}
			i = rs.getInt(index++);
			if (!rs.wasNull())
			{
				data.setSeqNum(i);
			}
			s = rs.getString(index++);
			if (!rs.wasNull())
			{
				data.setComment(s.Trim());
			}
			v.Add(data);
		}
		return v;
	}

	/// <summary>
	/// Convert a ResultSet to a Vector of DIADvisor_GroupDef. </summary>
	/// <param name="rs"> ResultSet from a GroupDef table query. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private java.util.List toGroupDefList(java.sql.ResultSet rs) throws Exception
	private System.Collections.IList toGroupDefList(ResultSet rs)
	{
		System.Collections.IList v = new List<object>();
		int index = 1;
		string s;
		int i;
		DIADvisor_GroupDef data = null;
		while (rs.next())
		{
			data = new DIADvisor_GroupDef();
			index = 1;
			s = rs.getString(index++);
			if (!rs.wasNull())
			{
				data.setGroup(s.Trim());
			}
			s = rs.getString(index++);
			if (!rs.wasNull())
			{
				data.setOperation(s.Trim());
			}
			s = rs.getString(index++);
			if (!rs.wasNull())
			{
				data.setUnits1(s.Trim());
			}
			s = rs.getString(index++);
			if (!rs.wasNull())
			{
				data.setUnits2(s.Trim());
			}
			i = rs.getInt(index++);
			if (!rs.wasNull())
			{
				data.setDisplay(i);
			}
			v.Add(data);
		}
		return v;
	}

	/// <summary>
	/// Convert a ResultSet to a Vector of DIADvisor_RegularTSRecord. </summary>
	/// <param name="rs"> ResultSet from a regular time series table (Hour, Day, Interval)
	/// query. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private java.util.List toRegularTSRecordList(java.sql.ResultSet rs) throws Exception
	private System.Collections.IList toRegularTSRecordList(ResultSet rs)
	{
		System.Collections.IList v = new List<object>();
		int index = 1;
		double d;
		int i;
		System.DateTime date;
		DIADvisor_RegularTSRecord data = null;
		while (rs.next())
		{
			data = new DIADvisor_RegularTSRecord();
			index = 1;
			date = rs.getTimestamp(index++);
			if (rs.wasNull())
			{
				// Absolutely need a date!
				continue;
			}
			data.setStartTime(date);
			i = rs.getInt(index++);
			if (!rs.wasNull())
			{
				data.setSensorID(i);
			}
			d = rs.getDouble(index++);
			if (!rs.wasNull())
			{
				data.setValue(d);
			}
			i = rs.getInt(index++);
			if (!rs.wasNull())
			{
				data.setCount(i);
			}
			v.Add(data);
		}
		return v;
	}

	/// <summary>
	/// Convert a ResultSet to a Vector of DIADvisor_SensorDef. </summary>
	/// <param name="rs"> ResultSet from a SensorDef table query. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private java.util.List toSensorDefList(java.sql.ResultSet rs) throws Exception
	private System.Collections.IList toSensorDefList(ResultSet rs)
	{
		System.Collections.IList v = new List<object>();
		int index = 1;
		string s;
		int i;
		bool b;
		double d;
		System.DateTime date;
		DIADvisor_SensorDef data = null;
		while (rs.next())
		{
			data = new DIADvisor_SensorDef();
			index = 1;
			i = rs.getInt(index++);
			if (!rs.wasNull())
			{
				data.setSensorID(i);
			}
			i = rs.getInt(index++);
			if (!rs.wasNull())
			{
				data.setSiteID(i);
			}
			s = rs.getString(index++);
			if (!rs.wasNull())
			{
				data.setType(s.Trim());
			}
			s = rs.getString(index++);
			if (!rs.wasNull())
			{
				data.setGroup(s.Trim());
			}
			s = rs.getString(index++);
			if (!rs.wasNull())
			{
				data.setDescription(s.Trim());
			}
			i = rs.getInt(index++);
			if (!rs.wasNull())
			{
				data.setMaxCount(i);
			}
			i = rs.getInt(index++);
			if (!rs.wasNull())
			{
				data.setMinCount(i);
			}
			i = rs.getInt(index++);
			if (!rs.wasNull())
			{
				data.setPosDelta(i);
			}
			i = rs.getInt(index++);
			if (!rs.wasNull())
			{
				data.setNegDelta(i);
			}
			s = rs.getString(index++);
			if (!rs.wasNull())
			{
				data.setRatingType(s.Trim());
			}
			s = rs.getString(index++);
			if (!rs.wasNull())
			{
				data.setRatingInterpolation(s.Trim());
			}
			d = rs.getDouble(index++);
			if (!rs.wasNull())
			{
				data.setRatingShift(d);
			}
			i = rs.getInt(index++);
			if (!rs.wasNull())
			{
				data.setCalibrationOffset(i);
			}
			date = rs.getTimestamp(index++);
			if (!rs.wasNull())
			{
				data.setCalibrationDate(date);
			}
			d = rs.getDouble(index++);
			if (!rs.wasNull())
			{
				data.setSlope(d);
			}
			d = rs.getDouble(index++);
			if (!rs.wasNull())
			{
				data.setReferenceLevel(d);
			}
			s = rs.getString(index++);
			if (!rs.wasNull())
			{
				data.setDisplayUnits(s.Trim());
			}
			i = rs.getInt(index++);
			if (!rs.wasNull())
			{
				data.setDecimal(i);
			}
			s = rs.getString(index++);
			if (!rs.wasNull())
			{
				data.setDisplayUnits2(s.Trim());
			}
			i = rs.getInt(index++);
			if (!rs.wasNull())
			{
				data.setDecimal2(i);
			}
			b = rs.getBoolean(index++);
			if (!rs.wasNull())
			{
				data.setInService(b);
			}
			b = rs.getBoolean(index++);
			if (!rs.wasNull())
			{
				data.setSuspect(b);
			}
			b = rs.getBoolean(index++);
			if (!rs.wasNull())
			{
				data.setAlarms(b);
			}
			b = rs.getBoolean(index++);
			if (!rs.wasNull())
			{
				data.setNotify(b);
			}
			i = rs.getInt(index++);
			if (!rs.wasNull())
			{
				data.setTimeout(i);
			}
			b = rs.getBoolean(index++);
			if (!rs.wasNull())
			{
				data.setChildren(b);
			}
			date = rs.getTimestamp(index++);
			if (!rs.wasNull())
			{
				data.setMostRecentTime(date);
			}
			d = rs.getDouble(index++);
			if (!rs.wasNull())
			{
				data.setMostRecentData(d);
			}
			date = rs.getTimestamp(index++);
			if (!rs.wasNull())
			{
				data.setLastValidTime(date);
			}
			d = rs.getDouble(index++);
			if (!rs.wasNull())
			{
				data.setLastValidData(d);
			}
			i = rs.getInt(index++);
			if (!rs.wasNull())
			{
				data.setLastCount(i);
			}
			s = rs.getString(index++);
			if (!rs.wasNull())
			{
				data.setEquation(s.Trim());
			}
			s = rs.getString(index++);
			if (!rs.wasNull())
			{
				data.setEquation2(s.Trim());
			}
			v.Add(data);
		}
		return v;
	}

	/// <summary>
	/// Convert a ResultSet to a Vector of DIADvisor_SiteDef. </summary>
	/// <param name="rs"> ResultSet from a SiteDef table query. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private java.util.List toSiteDefList(java.sql.ResultSet rs) throws Exception
	private System.Collections.IList toSiteDefList(ResultSet rs)
	{
		System.Collections.IList v = new List<object>();
		int index = 1;
		string s;
		long l;
		double d;
		System.DateTime date;
		DIADvisor_SiteDef data = null;
		while (rs.next())
		{
			data = new DIADvisor_SiteDef();
			index = 1;
			s = rs.getString(index++);
			if (!rs.wasNull())
			{
				data.setSiteName(s.Trim());
			}
			d = rs.getDouble(index++);
			if (!rs.wasNull())
			{
				data.setLatitude(d);
			}
			d = rs.getDouble(index++);
			if (!rs.wasNull())
			{
				data.setLongitude(d);
			}
			d = rs.getDouble(index++);
			if (!rs.wasNull())
			{
				data.setXCoord(d);
			}
			d = rs.getDouble(index++);
			if (!rs.wasNull())
			{
				data.setYCoord(d);
			}
			l = rs.getLong(index++);
			if (!rs.wasNull())
			{
				data.setPKey(l);
			}
			s = rs.getString(index++);
			if (!rs.wasNull())
			{
				data.setRepeaterGroup(s.Trim());
			}
			d = rs.getDouble(index++);
			if (!rs.wasNull())
			{
				data.setElevation(d);
			}
			s = rs.getString(index++);
			if (!rs.wasNull())
			{
				data.setSitePicture(s.Trim());
			}
			s = rs.getString(index++);
			if (!rs.wasNull())
			{
				data.setZone(s.Trim());
			}
			s = rs.getString(index++);
			if (!rs.wasNull())
			{
				data.setFIPS(s.Trim());
			}
			date = rs.getTimestamp(index++);
			if (!rs.wasNull())
			{
				data.setLastUpdate(date);
			}
			v.Add(data);
		}
		return v;
	}

	/// <summary>
	/// Convert a ResultSet to a Vector of DIADvisor_SysConfig. </summary>
	/// <param name="rs"> ResultSet from a SysConfig table query. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private java.util.List toSysConfigList(java.sql.ResultSet rs) throws Exception
	private System.Collections.IList toSysConfigList(ResultSet rs)
	{
		System.Collections.IList v = new List<object>();
		int index = 1;
		int i;
		DIADvisor_SysConfig data = null;
		while (rs.next())
		{
			data = new DIADvisor_SysConfig();
			index = 1;
			i = rs.getInt(index++);
			if (!rs.wasNull())
			{
				data.setInterval(i);
			}
			v.Add(data);
		}
		return v;
	}

	} // End DIADvisorDMI

}