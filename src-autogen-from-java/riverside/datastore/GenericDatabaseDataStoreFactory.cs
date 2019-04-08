using System;

// GenericDatabaseDataStoreFactory - datastore factory for GenericDatabaseDataStore

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
	using GenericDMI = RTi.DMI.GenericDMI;
	using IOUtil = RTi.Util.IO.IOUtil;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// Factory to instantiate ODBCDataStore instances.
	/// @author sam
	/// </summary>
	public class GenericDatabaseDataStoreFactory : DataStoreFactory
	{

	/// <summary>
	/// Create an ODBCDataStore instance and open the encapsulated DMI using the specified properties.
	/// </summary>
	public virtual DataStore create(PropList props)
	{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		string routine = this.GetType().FullName + ".create";
		string name = props.getValue("Name");
		string description = props.getValue("Description");
		if (string.ReferenceEquals(description, null))
		{
			description = "";
		}
		string databaseEngine = IOUtil.expandPropertyForEnvironment("DatabaseEngine",props.getValue("DatabaseEngine"));
		string databaseServer = IOUtil.expandPropertyForEnvironment("DatabaseServer",props.getValue("DatabaseServer"));
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
		string databaseName = IOUtil.expandPropertyForEnvironment("DatabaseName",props.getValue("DatabaseName"));
		string odbcName = IOUtil.expandPropertyForEnvironment("OdbcName",props.getValue("OdbcName"));
		string systemLogin = IOUtil.expandPropertyForEnvironment("SystemLogin",props.getValue("SystemLogin"));
		string systemPassword = IOUtil.expandPropertyForEnvironment("SystemPassword",props.getValue("SystemPassword"));
		try
		{
			GenericDMI dmi = null;
			if ((!string.ReferenceEquals(odbcName, null)) && !odbcName.Equals(""))
			{
				// An ODBC connection is configured so use it
				dmi = new GenericDMI(databaseEngine, odbcName, systemLogin, systemPassword); // OK if null - use read-only guest
			}
			else
			{
				// Use the parts to create the connection
				dmi = new GenericDMI(databaseEngine, databaseServer, databaseName, port, systemLogin, systemPassword);
			}
			// Always create the datastore, which generally involves simple assignment
			GenericDatabaseDataStore ds = new GenericDatabaseDataStore(name, description, dmi);
			ds.setProperties(props);
			// Now try to open the database connection, which may generate an exception
			ds.setStatus(0);
			try
			{
				dmi.open();
			}
			catch (Exception e)
			{
				ds.setStatus(1);
				ds.setStatusMessage("" + e);
			}
			return ds;
		}
		catch (Exception e)
		{
			// TODO SAM 2010-09-02 Wrap the exception because need to move from default Exception
			Message.printWarning(3,routine,e);
			throw new Exception(e);
		}
	}

	}

}