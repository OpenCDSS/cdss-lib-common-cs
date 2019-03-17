// GenericDMI extends DMI - DMI object for generic database connection

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
// GenericDMIJava - base class for a Generic DMI
// This is basically just a very bare-bones DMI for doing database operations.
// The DMI class is abstract and so cannot be instantiated by itself.  If
// there is a need to simply connect to a database and pass in SQL strings to
// run queries, the GenericDMI can be instantiated and used to do that.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2002-07-16	J. Thomas Sapienza, RTi	Initial Version
// 2003-01-03	Steven A. Malers, RTi	Make changes consistent with the updated
//					DMI base class.  Looks like this class
//					needs a lot of work anyhow?
// 2003-05-07	JTS, RTi		Finall did some javadoc'ing.
// ----------------------------------------------------------------------------

namespace RTi.DMI
{
	/// <summary>
	/// Bare-bones class for doing simple and quick DMI work.  Mostly it just implements
	/// the abstract methods in the base DMI class, but it has no database-specific
	/// code to get in the way of doing simple JDBC work.<para>
	/// This class is useful for DMI debugging purposes.  It isn't
	/// tied to any particular database and so can be used for checking connections,
	/// executing queries (with dmiSelect) and writes (with dmiWrite).  
	/// Here is an example of use:
	/// <pre>
	/// GenericDMI dmi = null;
	/// dmi = new GenericDMI("SQLServer", "localhost", "RiversideDB", 1433, "sa", "sa");
	/// dmi.open();
	/// ResultSet rs = dmi.dmiSelect("select * from geoloc");
	/// if (rs.next()) {
	///		System.out.println("'''" + rs.getString(8) + "'''");
	/// }
	/// </pre>
	/// </para>
	/// </summary>
	public class GenericDMI : DMI
	{

	/// <summary>
	/// Builds a DMI connection to the named ODBC connection. </summary>
	/// <param name="database_engine"> the kind of database that is running </param>
	/// <param name="ODBC_name"> the name of the ODBC data source to connect to. </param>
	/// <param name="system_login"> the login to use to log into the ODBC data source </param>
	/// <param name="system_password"> the password to use to log into the ODBC data source </param>
	/// <exception cref="Exception"> if an error occurs </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public GenericDMI(String database_engine, String ODBC_name, String system_login, String system_password) throws Exception
	public GenericDMI(string database_engine, string ODBC_name, string system_login, string system_password) : base(database_engine, ODBC_name, system_login, system_password)
	{
	}

	/// <summary>
	/// Builds a DMI connection to the named database. </summary>
	/// <param name="database_engine"> the kind of database that is running </param>
	/// <param name="database_server"> the machine on which the database is running </param>
	/// <param name="database_name"> the name of the database to which to connect </param>
	/// <param name="port"> the port on which the database is running </param>
	/// <param name="system_login"> the login to use to log into the ODBC data source </param>
	/// <param name="system_password"> the password to use to log into the ODBC data source </param>
	/// <exception cref="Exception"> if an error occurs </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public GenericDMI(String database_engine, String database_server, String database_name, int port, String system_login, String system_password) throws Exception
	public GenericDMI(string database_engine, string database_server, string database_name, int port, string system_login, string system_password) : base(database_engine, database_server, database_name, port, system_login, system_password)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public override void determineDatabaseVersion()
	{
		setDatabaseVersion(0);
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public override void readGlobalData()
	{
	}

	}

}