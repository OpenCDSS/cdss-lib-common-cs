using System;
using System.Collections.Generic;

// DMI - base class for all database DMI operations

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
// DMI.java - base class for all database DMI operations
// ----------------------------------------------------------------------------
// To do:
//
// * Can the last* data and methods be boiled down to just "statement" (any
//   need for the query data/method)?
// * Need to figure out cases where a check for "Access_ODBC" is done.
// * should the standard be to use a dmiExecute() method that detects the type
//   of DMIStatement to process?
// * In constructors and setDatabaseEngine() - can some duplicate code be 
//   combined without confusing the logic?  Should setDatabaseEngine() also set
//   the port number?
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// Notes:
// (1) This class is abstract and cannot be directly instantiated
// (2) Derived classes must override the 
//     public abstract String buildSQL(int sqlNumber, Vector values);
//     function
// ----------------------------------------------------------------------------
// History:
// 2002-05-10	J. Thomas Sapienza, RTi	Initial version around this time.
// 2002-05-21	JTS, RTi		Changed private members to begin with 
//					two underscores, representing the new 
//					naming convention in use.  Began 
//					changelog.  Added the MISSING_* 
//					variables and the isMissing* methods.
// 2002-05-22	JTS, RTi		executeQuery and dmiRunQuery return
//                         		ResultSets now, not Vectors.  Changed 
//					STRING_NA to MISSING_STRING and set 
//					it to "".
// 2002-05-23	JTS, RTi		Superficial coding changes (spaces 
//					removed, several constants renumbered, 
//					parentheses removed from returns, 
//					methods renamed)
// 2002-05-28	JTS, RTi		Added formatDate() method.
// 2002-05-29	JTS, RTi		Removed everything related to the 
//					__debug member variable.  Replaced all 
//					System.out.println's with calls to
//					the Message class.
// 2002-05-20	JTS, RTi		'UserName' in variables and methods has 
//					become 'Login'
// 2002-06-25	Steven A. Malers, RTi	Update to handle DMIStatement to
//					store SQL statement information.
//					Add databaseHasTable(),
//					databaseTableHasColumn(),
//					determineDatabaseVersion(), and
//					isDatabaseVersionAtLeast() to support
//					versioning.
//					Remove @author and @version Javadoc
//					tags.
//					Add readGlobalData().
//					Change isConnected() to isOpen() to be
//					consistent with previous DMI work.
// 2002-07-11	J. Thomas Sapienza, RTi	Change things to use the DMIStatement
//					classes, as opposed to operating solely
//					based on SQL strings
// 2002-07-12	JTS, RTi		Changed all the DMI functions that
//					were named executeXXXX to the form
//					"dmiXXXX" to avoid confusion with 
//					the executeXXXX functions contained in
//					java.sql.Statement
//					dmiWrite changed to use an integer flag
//					to determine what its operation should
//					be
//					Removed several functions rendered 
//					unnecessary by the change to dmiWrite,
//					including dmiUpdate, dmiInsert, and
//					a couple overloaded dmiWrite functions
// 2002-07-16	JTS, RTi		Changed all the Message.printDebugs
//					that were still printing the old names
//					of various functions
// 2002-07-17	JTS, RTi		Corrected several bugs that hadn't
//					yet been uncovered in dmiCount, 
//					including one that formed count queries
//					incorrectly when an ORDER BY clause was
//					present in the original SQL, and 
//					another that caused windows memory	
//					reference faults.  Removed 
//					MISSING_INTEGER since the class already
//					has MISSING_INT
//					Added support for SQL Server 2000
//					jdbc connections.  Removed the old
//					(and by now, commented-out) buildSQL
//					code.
//					Massive rework of commenting that had
//					grown badly out-of-date
// 2002-07-18	JTS, RTi		Added code to get a list of tables in
//					the current database.  Database can 
//					now be set in read-only mode (disables
//					any calls to dmiWrite or dmiDelete.
//					Database can
//					generate information about the fields
//					in a table (or tables) and populate
//					a DMITableModel to show the data in 
//					a JTable.  "SQL_Server_2000" changed to
//					"SQLServer2000".  Sql Server 2000 no
//					longer has the port number hard-coded.
// 2002-07-22	JTS, RTi		Change in dmiGetFieldInfo to avoid
//					returning null.  Instead, an empty
//					DMITableModel is returned.  Cleaned 
//					up the printDebug statements and added
//					the use of the dl and routine variables.
// 2002-07-23	JTS, RTi		Method calls that require the database
//					to be connected are first checked to
//					see if the connection is open.  If it
//					isn't, a SQLException is thrown.
// 2002-07-24	JTS, RTi		Added some methods that will need to 
//					be removed later, but which are useful
//					for development for Morgan and me right
//					now (soft, loud, createWeirdFiles)
// 2002-08-06	JTS, RTi		Added the databaseHasStoredProcedure
//					procedures
// 2002-08-13	JTS, RTi		Changed database type "Access" to
//					"Access_ODBC" (to more accurately 
//					represent what it is)
// 2002-08-14	JTS, RTi		Made a few constants static that
//					weren't.  Alphabetized, organized
//					member variables and methods.
//					Recurrent bug when executing a count
//					statement of the general form 
//					SELECT COUNT (*);
//					finally eliminated with lots of string
//					work.  BTW, that's a regular expression.
//					More checks in various methods for
//					__connected. Code cleaned up.
//					Removed some ancient methods that have
//					not been used for the last three months
//					and had been set to deprecated for 
//					almost that long.
// 2002-08-15	JTS, RTi		More Housekeeping.  Removed the "field
//					type constants.  
// 2002-08-21	JTS, RTi		Better handling of Statement and 
//					ResultSet objects now; they get 
//					close()d.  Introduced a finalize()
//					method.
// 2002-08-22	JTS, RTi		Statements aren't closed anymore -- 
//					found out that they lead to erratic
//					and unpredictable exceptions if they
//					finish closing before they're done 
//					populating the result set
// 2002-10-07	JTS, RTi		Added support for SQL Server 7, and 
//					cleaned up some bad string comparisons
//					on the way out.
// 2002-10-14	SAM, RTi		* Change the login and password data and
//					  members to use the notation "System"
//					  for low-level system connections and
//					  "User" for user connections.  The
//					  system information is used for the URL
//					  to make the connection and the user
//					  information can be used for table and
//					  record level restrictions.
//					* Default __secure to false - need to
//					  change to true if the user login is
//					  somehow verified as secure.
//					* Change getDatabaseProperties to return
//					  a Vector of String - not other object
//					  types.
// 2002-12-24	SAM, RTi		* Clarify how code handles different
//					  database connections.  Now the
//					  _auto_odbc boolean is used to
//					  indicate whether the ODBC URL is
//					  formed internally (using
//					  __database_name) or whether the ODBC
//					  name is specified (using __odbc_name).
//					* Replace the
//					  Access_ODBC type with Access database
//					  type coupled with __auto_odbc = false
//					  and __odbc_name.
//					* Change __databaseType to
//					  _database_engine to more closely agree
//					  with C++ and other conventions.
//					  Change the related methods
//					  appropriately.
//					* Change __ip to __database_server to
//					  more closely agree with other
//					  conventions.  Change the related
//					  methods appropriately.
//					* Remove the __blab data member and
//					  related code since it is redundant
//					  with Message debugging and the DMI
//					  classs has tested out in production.
//					* Remove references to TSDate since it
//					  is not used - in the future use
//					  DateTime if necessary.
//					* Move general support code to DMIUtil
//					  to streamline this DMI code.
//					* Use equalsIgnoreCase() for database
//					  engine, etc. - no reason to require
//					  exact match because it leads to errors
//					  in configuration.
//					* Remove checkStringForNull() - the
//					  wasNull() method should be used when
//					  processing result sets, giving better
//					  overall performance.
//					* Remove createWeirdFiles() - apparently
//					  used in early development.
//					* Update the constructors to work either
//					  with straight ODBC or an internally
//					  created JDBC/ODBC URL.
//					* Define protected final int values for
//					  database types to increase performance
//					  (string comparisons for the database
//					  type is slower).  This also allows
//					  more checks for valid database
//					  engines.
// 2003-01-06	SAM, RTi		* JTS moved DMITableModel code out to
//					  other file(s).  Will work on later to
//					  make a general component.
//					* condenseSpaces(), condenseString(),
//					  removeSpaces(), split(),
//					  vectorToStringArray()
//					  have been removed since they supported
//					  an error trap that will be very
//					  infrequent.  Need to include in
//					  StringUtil as time allows.
//					  Remove dmiCountCheck() whose sole
//					  purpose was to check for SELECT
//					  COUNT (*); - we need to guard against
//					  this crash in other ways.
//					* Update getDatabaseProperties() to be
//					  more verbose and stand-alone.
//					* Move sqlError() to
//					  DMIUtil.printSQLException().
//					* Change the connection method from
//					  __auto_odbc to __jdbc_odbc.  If true
//					  then a specific driver is used.  If
//					  false the the general ODBC driver is
//					  used.  The other option for C++
//					  applications is "File".
// 2003-03-05	JTS, RTi		Added the left and right delims
//					that are set for each database type.
// 2003-03-07	JTS, RTI		Added setCapitalized() method so that
//					capitalization of all SQL strings can
//					be handled at the most basic level.
// 2003-03-08	SAM, RTi		Fix bug where when using an ODBC name
//					constructor (not JDBC-ODBC), the ODBC
//					name was not actually getting set in the
//					constructor.
// 2003-04-07	JTS, RTi		Added getDatabaseMetaData().
// 2003-04-22	JTS, RTi		Added getJDBCODBC().
// 2003-06-16	JTS, RTi		Added escape().
// 2003-08-26	JTS, RTi		Added __id and __name fields.
// 2004-04-26	JTS, RTi		Added new open() methods that take a
//					boolean parameter for printing debug
//					information to status level 1 instead.
//					This is useful for applications like
//					RTAssistant where the dmi connection
//					happens when the GUI starts up and 
//					where the user doesn't have a chance
//					to change the Debug level settings
//					prior to the call to open().
// 2004-07-13	JTS, RTi		Added closeResultSet() methods.
// 2004-07-20	JTS, RTi		* Added __statementsVector to hold
//					  the statements created during a 
//					  transaction.
//					* Statements created during delete
//					  and write operations are closed now.
// 2004-11-16	Scott Townsend, RTi	Added PostgreSQL support.
// 2005-03-07	JTS, RTi		Added a new closeResultSet() method
//					that takes a Statement as a parameter.
//					This is used with connections that use
//					stored procedures.
// 2005-04-07	JTS, RTi		Added setInputName() and getInputName().
// 2005-10-12	JTS, RTi		Shared constructor code moved to a 
//					single initialize() method in order to
//					accomodate changes in HydroBase.
// 2005-12-01	JTS, RTi		Previous fix broke Access ODBC 
//					connections.  Fixed.
// ----------------------------------------------------------------------------
// EndHeader

namespace RTi.DMI
{
	using Message = RTi.Util.Message.Message;

	using StringUtil = RTi.Util.String.StringUtil;



	/// <summary>
	/// The DMI class serves as a base class to allow interaction with ODBC/JDBC compliant database servers.
	/// Derived classes should define specific code to implement a connection with the database.
	/// In particular, a derived class should:
	/// <ol>
	/// <li>	Call the appropriate constructor in this base class when constructing an instance.</li>
	/// <li>	Implement a determineDatabaseVersion() method, which is called from
	/// the open() method in this base DMI class.  The
	/// determineDatabaseVersion() method should call the setDatabaseVersion()
	/// method of this DMI base class.</li>
	/// <li>	Implement a readGlobalData() method to read global data that will be
	/// kept in memory in the derived DMI class (e.g., for commonly used data like units).
	/// <li>	Implement a getDatabaseProperties() method that can be used by an
	/// application (e.g., to show database properties to a user.</li>
	/// <li>	Use the DMI*Statement objects to create SQL statements.  Execute the statements using dmi*() methods.</li>
	/// <li>	Implement constructors that follow the guidelines described below.
	/// </ol>
	/// To create a connection to a database, a DMI instance should be created using one
	/// of the two constructors.  The constructors are provided for the two main ways
	/// to connect to a database:
	/// <ol>
	/// <li>	Using an ODBC DSN that is defined on the machine.  In this case only
	/// the ODBC name is required from the derived class.</li>
	/// <li>	Using a database server name and database name on the server.
	/// <b>This method is preferred with Java because it allows individual
	/// manufacture JDBC drivers to be used.</b>  In this case the constructor
	/// in the derived class should pass the system login and password used to
	/// make the connection, as well as other information that may be required
	/// for the specific database.</li>
	/// </ol>
	/// The open() method in this base class will make a database connection using only
	/// one of the above methods, depending on whether the ODBC name or the database
	/// name and server have been defined.
	/// 
	/// A new DMI should typically be constructed only after an existing DMI is
	/// destroyed.  Application code should include code similar to the following:
	/// 
	/// <pre>
	/// someDMI dmi = null;
	/// ...
	/// // Doing a database login/connection...
	/// if ( dmi != null ) {
	/// try {	dmi.close();
	///		dmi = null;
	/// }
	/// catch ( Exception e ) {
	///		// Usually can ignore.
	/// }
	/// }
	/// 
	/// try {	dmi = new someDMI ( ... );
	/// // If necessary...
	/// dmi.setXXX();
	/// dmi.open();
	/// }
	/// catch ( Exception e ) {
	/// // Error message/dialog...
	/// }
	/// </pre>
	/// </summary>
	public abstract class DMI
	{

	public const string CLASS = "DMI";

	// Integer constants for database engine types, to formalize the engines that
	// are supported and to streamline internal processing.
	// TODO SAM 2015-02-16 Need to convert to an enumeration

	/// <summary>
	/// Database engine corresponding to "Access" database engine (Jet).
	/// </summary>
	public const int DBENGINE_ACCESS = 10;

	/// <summary>
	/// Database engine corresponding to "Derby" database engine (Oracle implementation of Apache Derby).
	/// </summary>
	public const int DBENGINE_DERBY = 15;

	/// <summary>
	/// Database engine corresponding to "Informix" database engine (no distinction about version?).
	/// </summary>
	public const int DBENGINE_INFORMIX = 20;

	/// <summary>
	/// Database engine corresponding to "MySQL" database engine (no distinction about version?).
	/// </summary>
	public const int DBENGINE_MYSQL = 30;

	/// <summary>
	/// Database engine corresponding to "Oracle" database engine (no distinction about version?).
	/// </summary>
	public const int DBENGINE_ORACLE = 40;

	/// <summary>
	/// Database engine corresponding to "SQL Server" database engine (2000, 2005, or 2008).
	/// Previously had definitions for the following but the current 2008 JDBC driver is advertised to be
	/// backward compatible so only use the one value now:
	/// <pre>
	/// protected final int _DBENGINE_SQLSERVER7 = 50;
	/// protected final int _DBENGINE_SQLSERVER2000 = 60;
	/// protected final int _DBENGINE_SQLSERVER_2005 = 61;
	/// </pre>
	/// </summary>
	public const int DBENGINE_SQLSERVER = 62;

	/// <summary>
	/// Database engine corresponding to "PostgreSQL" database engine (no distinction about version?).
	/// </summary>
	public const int DBENGINE_POSTGRESQL = 70;

	/// <summary>
	/// Database engine corresponding to "H2" database engine
	/// </summary>
	public const int DBENGINE_H2 = 80;

	/// <summary>
	/// Database engine corresponding to "Excel" database engine (no distinction about version?).
	/// </summary>
	public const int DBENGINE_EXCEL = 90;

	/// <summary>
	/// Database engine corresponding to ODBC DSN database connection but engine type is not
	/// specifically known to this code.  Useful for generic connections.
	/// </summary>
	public const int DBENGINE_ODBC = 100;

	///////////////////////////////////////////////////////////
	//  Commit / Rollback constants
	///////////////////////////////////////////////////////////

	/// <summary>
	/// Constant for referring to ROLLBACK operations
	/// </summary>
	public const int ROLLBACK = 0;

	/// <summary>
	/// Constant for referring to COMMIT operations
	/// </summary>
	public const int COMMIT = 1;

	///////////////////////////////////////////////////////////
	//  SQL Type constants
	///////////////////////////////////////////////////////////
	/// <summary>
	/// Constant used to tell that the last query type was NONE
	/// </summary>
	public const int NONE = 0;

	/// <summary>
	/// Constant used to tell that the last query type was COUNT
	/// </summary>
	public const int COUNT = 1;

	/// <summary>
	/// Constant used to tell that the last query type was DELETE
	/// </summary>
	public const int DELETE = 2;

	/// <summary>
	/// Constant used to tell that the last query type was SELECT
	/// </summary>
	public const int SELECT = 3;

	/// <summary>
	/// Constant used to tell that the last query type was a WRITE
	/// </summary>
	public const int WRITE = 4;

	/// <summary>
	/// Constant used in dmiWrite() to specify to first do an INSERT, and if that fails, try to do an UPDATE
	/// </summary>
	public const int INSERT_UPDATE = 6;

	/// <summary>
	/// Constant used in dmiWrite() to specify to first do an UPDATE, and if that fails, try to do an INSERT
	/// </summary>
	public const int UPDATE_INSERT = 7;

	/// <summary>
	/// Constant used in dmiWrite() to specify to first delete the record before inserting a new one
	/// </summary>
	public const int DELETE_INSERT = 8;

	/// <summary>
	/// Constant used in dmiWrite() to specify to do an UPDATE
	/// </summary>
	public const int UPDATE = 9;

	/// <summary>
	/// Constant used in dmiWrite() to specify to do an INSERT
	/// </summary>
	public const int INSERT = 10;

	///////////////////////////////////////////////////////////
	//  Member variables
	///////////////////////////////////////////////////////////	

	/// <summary>
	/// Indicate whether database changes are automatically committed once they are done.
	/// </summary>
	private bool __autoCommit;

	/// <summary>
	/// Indicate whether the SQL should be capitalized before being sent to the database.
	/// </summary>
	private bool __capitalize;

	/// <summary>
	/// If true, then when an SQL statement is executed and an error occurs, the
	/// text of the SQL statement will be printed to the log file at Status level 2.
	/// This should only be used for debugging and testing, and should not be enabled in production code.
	/// </summary>
	private bool __dumpSQLOnError = false;

	/// <summary>
	/// If true, then when an SQL statement is executed, the text of the SQL statement 
	/// will be printed to the log file at Status level 2. This should only be used 
	/// for debugging and testing, and should not be enabled in production code.
	/// </summary>
	private bool __dumpSQLOnExecution = false;

	/// <summary>
	/// Indicate whether an JDBC ODBC connection is automatically set up (true) or
	/// whether the ODBC is defined on the machine (false).
	/// </summary>
	private bool __jdbc_odbc;

	/// <summary>
	/// Whether the program is currently connected to the database or not
	/// </summary>
	private bool __connected;

	/// <summary>
	/// Whether operations are currently being done in a transaction.
	/// </summary>
	private bool __inTransaction = false;

	/// <summary>
	/// Whether for the open() methods to print information to debug as normal (false)
	/// or to force the output to go to status level 1 (true).
	/// This is used with troubleshooting.  Usually this should be false.
	/// </summary>
	private bool __printStatus = false;

	/// <summary>
	/// Connection that is used for the database interaction
	/// </summary>
	private Connection __connection;

	/// <summary>
	/// Name of the database in the server if __jdbc_odbc is true.
	/// </summary>
	private string __database_name;

	/// <summary>
	/// ODBC Data Source Name if __jdbc_odbc is false.
	/// </summary>
	private string __odbc_name;

	/// <summary>
	/// Additional connection properties, which will be added at the end of the connection URL with a leading semi-colon.
	/// These are typically passed in for datastores that require special properties.
	/// Set with setAdditionalConnectionProperties() before calling open().
	/// </summary>
	private string __additionalConnectionProperties = "";

	/// <summary>
	/// Database engine to connect to, as a string, useful for debugging.
	/// <para>
	/// Valid types are:<br>
	/// <ul>
	/// <li>Access</ul>
	/// <li>Excel</ul>
	/// <li>Informix</ul>
	/// <li>MySQL</ul>
	/// <li>Oracle</ul>
	/// <li>PostgreSQL</ul>
	/// <li>SQLServer</ul>
	/// </ul>
	/// </para>
	/// </summary>
	private string __database_engine_String;

	/// <summary>
	/// The left-side escape string to wrap fields so that they are not mistaken for reserved words.
	/// This leads to more verbose SQL but is conservative.  For example, for SQL Server, "[" is used.
	/// </summary>
	private string __fieldLeftEscape = "";
	/// <summary>
	/// The right-side escape string to wrap fields so that they are not mistaken for reserved words.
	/// This leads to more verbose SQL but is conservative.  For example, for SQL Server, "]" is used.
	/// </summary>
	private string __fieldRightEscape = "";
	/// <summary>
	/// The delimiter for strings.
	/// </summary>
	private string __stringDelim = "";

	/// <summary>
	/// Database engine as integer, to improve performance.  The protected value is
	/// used directly in DMIStatement and other classes in this package to improve performance.
	/// </summary>
	protected internal int _database_engine;

	/// <summary>
	/// Database version.  This should be set by calling determineDatabaseVersion(),
	/// which is an abstract method to be defined in derived DMI classes.  The 
	/// standard for the version number is XXXXXXDDDDDDDD where XXXXXXX is 
	/// typically a database version (e.g., RiverTrak 020601) and DDDDDDDD is 
	/// usually the build date (e.g., 20020625).
	/// </summary>
	private long __database_version;

	/// <summary>
	/// True if any writes have been done to the database (uncommitted)
	/// </summary>
	private bool __dirty = false;

	/// <summary>
	/// Whether the DMI should treat the database as being in read-only mode (false,
	/// all calls to DMI.delete() or DMI.write() are ignored) or not
	/// </summary>
	private bool __editable;

	/// <summary>
	/// Host name or IP address of the database server (or "Local" if the local host is used).
	/// </summary>
	private string __database_server;

	/// <summary>
	/// The last SELECT query SQL string that was executed.
	/// </summary>
	private DMISelectStatement __lastQuery;

	// TODO SAM 2007-05-08 Need to evaluate if used in troubleshooting, etc.
	/// <summary>
	/// The last SELECT query (in string form) that was executed.
	/// JTS 16/04/02 -- This is in here so that panels that use the DMI without
	/// the use of statement objects (i.e., the SQL Analyzer-type program) can 
	/// re-query after a write or delete.
	/// </summary>
	//private String __lastQueryString;

	/// <summary>
	/// The last SQL statement that was executed
	/// </summary>
	private DMIStatement __lastSQL;

	/// <summary>
	/// The type of the last SQL executed
	/// </summary>
	private int __lastSQLType;

	/// <summary>
	/// The login timeout to use when establishing the database connection.
	/// </summary>
	private int __loginTimeout = -1;

	/// <summary>
	/// Port number of the database to connect, used by client-server databases.
	/// </summary>
	private int __port;

	/// <summary>
	/// Sets whether in toString and getDatabaseProperties to print out 
	/// information that may be secure (such as login name and password), or not.
	/// </summary>
	private bool __secure = false;

	/// <summary>
	/// ID string to identify the DMI connection.
	/// </summary>
	private string __id;

	/// <summary>
	/// Name string to identify the DMI connection.
	/// </summary>
	private string __inputName = "";

	/// <summary>
	/// Name string to identify the DMI connection.
	/// TODO (JTS - 2005-04-07) probably rendered obsolete by the new InputName stuff.  Deprecated -- anything
	/// uses and I won't remove.  If nothing has popped up in 4 months, eliminate.
	/// </summary>
	private string __name;

	/// <summary>
	/// Login name to connect to the database (often used in database connection URL).
	/// </summary>
	private string __system_login;

	/// <summary>
	/// Password to connect to the database (often used in the database connection URL).
	/// </summary>
	private string __system_password;

	/// <summary>
	/// User login name to restrict access to the database.  This is typically more
	/// restrictive than the system login but may be the same.
	/// </summary>
	private string __user_login;

	/// <summary>
	/// User password to restrict access to the database.  This is typically more
	/// restrictive than the system password but may be the same.
	/// </summary>
	private string __user_password;

	/// <summary>
	/// List for holding all the statements that were created during a transaction,
	/// so they can be closed when the transaction is committed or rolled back.
	/// </summary>
	private IList<Statement> __statementsVector;

	/// <summary>
	/// An empty constructor.  If this constructor is used, initialize() must be called
	/// with the proper values to initialize the DMI settings.
	/// </summary>
	public DMI()
	{
	}

	/// <summary>
	/// Constructor for this DMI base class for cases when a connection to an ODBC DSN
	/// defined on the machine.  The generic JDBC/ODBC driver is used.  To use a
	/// manufacturer's driver, use the overloaded constructor that takes as a parameter the database name. </summary>
	/// <param name="database_engine"> Database engine type (see setDatabaseEngine()). </param>
	/// <param name="ODBC_name"> ODBC DSN to use.  The ODBC DSN must be defined on the machine. </param>
	/// <param name="system_login"> System login to use for the database connection.  Specify
	/// as null to not use.  Generally the login will be defined in a derived class and is specific to a database. </param>
	/// <param name="system_password"> System login to use for the database connection.  Specify
	/// as null to not use.  Generally the login will be defined in a derived class and is specific to a database. </param>
	/// <exception cref="Exception"> thrown if an unknown databaseEngine is specified. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public DMI(String database_engine, String ODBC_name, String system_login, String system_password) throws Exception
	public DMI(string database_engine, string ODBC_name, string system_login, string system_password)
	{
		initialize(database_engine, null, null, 0, system_login, system_password, ODBC_name, false);
	}

	/// <summary>
	/// Constructor for this DMI base class for cases when a connection to a server
	/// database will be defined automatically, not using a predefined ODBC DSN.
	/// The specified JDBC/ODBC driver for the database engine is used.  To use
	/// the generic JDBC/ODBC driver with an ODBC DSN, use the overloaded constructor
	/// that takes as a parameter the ODBC name. </summary>
	/// <param name="database_engine"> Database engine type (see setDatabaseEngine()). </param>
	/// <param name="database_server"> Database server name (IP address or DNS-resolvable name). </param>
	/// <param name="database_name"> Database name on the server. </param>
	/// <param name="port"> Port to use for the database communications.  Specify as a negative
	/// number to use the default.  Generally the default value determined from the database engine is correct. </param>
	/// <param name="system_login"> System login to use for the database connection.  Specify
	/// as null to not use.  Generally the login will be defined in a derived class and is specific to a database. </param>
	/// <param name="system_password"> System login to use for the database connection.  Specify
	/// as null to not use.  Generally the login will be defined in a derived class and is specific to a database. </param>
	/// <exception cref="Exception"> thrown if an unknown databaseEngine is specified. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public DMI(String database_engine, String database_server, String database_name, int port, String system_login, String system_password) throws Exception
	public DMI(string database_engine, string database_server, string database_name, int port, string system_login, string system_password)
	{
		initialize(database_engine, database_server, database_name, port, system_login, system_password, null, true);
	}

	/// <summary>
	/// Initialization routine that sets up the internal DMI settings. </summary>
	/// <param name="database_engine"> the type of database engine to which the DMI is 
	/// connecting.  See the __database_engine_string private javadocs. </param>
	/// <param name="database_server"> the name or IP of the server to which the DMI will connect. </param>
	/// <param name="database_name"> the name of the database on the server that the DMI will connect to. </param>
	/// <param name="port"> the port on the database_server where the database listens for connections. </param>
	/// <param name="system_login"> the login value to use to connect to the database server or ODBC connection. </param>
	/// <param name="ssytem_password"> the login password to use to connect to the database server or ODBC connection. </param>
	/// <param name="ODBC_name"> the name of the ODBC data source to connect to. </param>
	/// <param name="jdbc_odbc"> if true then the connection is a IP or server-name -based
	/// connection.  If false, it is an ODBC connection.
	/// TODO (JTS - 2006-05-22) This boolean seems backwards!!! </param>
	/// <exception cref="Exception"> if the database_engine that is passed in is not recognized. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void initialize(String database_engine, String database_server, String database_name, int port, String system_login, String system_password, String ODBC_name, boolean jdbc_odbc) throws Exception
	public virtual void initialize(string database_engine, string database_server, string database_name, int port, string system_login, string system_password, string ODBC_name, bool jdbc_odbc)
	{
		string routine = "DMI.DMI";
		int dl = 25;

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "DMI created for database engine: " + database_engine);
		}
		Message.printStatus(2, routine, "Initializing DMI for database engine: " + database_engine);

		// initialize member variables
		__database_engine_String = database_engine;

		__jdbc_odbc = jdbc_odbc;
		__database_name = database_name;
		__odbc_name = ODBC_name;
		__database_server = database_server;
		__system_login = system_login;
		__system_password = system_password;
		__user_login = null;
		__user_password = null;
		__lastSQL = null;
		__lastQuery = null;
		__connection = null;

		__database_version = 0;
		__port = port;

		__dirty = false;
		__editable = false;
		__secure = false;
		__autoCommit = true;
		__connected = false;

		__lastSQLType = NONE;

		// Check the database engine type and set appropriate defaults...
		if ((!string.ReferenceEquals(__database_engine_String, null)) && __database_engine_String.Equals("Access", StringComparison.OrdinalIgnoreCase))
		{
			__fieldLeftEscape = "[";
			__fieldRightEscape = "]";
			__stringDelim = "'";
			__database_server = "Local";
			_database_engine = DBENGINE_ACCESS;
		}
		else if ((!string.ReferenceEquals(__database_engine_String, null)) && __database_engine_String.Equals("Derby", StringComparison.OrdinalIgnoreCase))
		{
			__fieldLeftEscape = "";
			__fieldRightEscape = "";
			__stringDelim = "'";
			if (__database_server.Equals("memory", StringComparison.OrdinalIgnoreCase))
			{
				__database_server = "localhost";
			}
			_database_engine = DBENGINE_DERBY;
		}
		else if ((!string.ReferenceEquals(__database_engine_String, null)) && __database_engine_String.Equals("Excel", StringComparison.OrdinalIgnoreCase))
		{
			// TODO SAM 2012-11-09 Need to confirm how Excel queries behave, for now use Access settings
			__fieldLeftEscape = "[";
			__fieldRightEscape = "]";
			__stringDelim = "'";
			__database_server = "Local";
			_database_engine = DBENGINE_EXCEL;
		}
		else if ((!string.ReferenceEquals(__database_engine_String, null)) && __database_engine_String.Equals("Informix", StringComparison.OrdinalIgnoreCase))
		{
			__fieldLeftEscape = "\"";
			__fieldRightEscape = "\"";
			__stringDelim = "'";
			_database_engine = DBENGINE_INFORMIX;
			if (port <= 0)
			{
				setDefaultPort();
			}
		}
		else if ((!string.ReferenceEquals(__database_engine_String, null)) && __database_engine_String.Equals("MySQL", StringComparison.OrdinalIgnoreCase))
		{
			__fieldLeftEscape = "";
			__fieldRightEscape = "";
			__stringDelim = "'";
			_database_engine = DBENGINE_MYSQL;
			if (port <= 0)
			{
				setDefaultPort();
			}
		}
		else if ((!string.ReferenceEquals(__database_engine_String, null)) && __database_engine_String.Equals("Oracle", StringComparison.OrdinalIgnoreCase))
		{
			__fieldLeftEscape = "\"";
			__fieldRightEscape = "\"";
			__stringDelim = "'";
			_database_engine = DBENGINE_ORACLE;
			if (port <= 0)
			{
				setDefaultPort();
			}
		}
		else if ((!string.ReferenceEquals(__database_engine_String, null)) && __database_engine_String.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
		{
			// The following caused issues
			//__fieldLeftEscape = "\"";
			//__fieldRightEscape = "\"";
			__fieldLeftEscape = "";
			__fieldRightEscape = "";
			__stringDelim = "'";
			_database_engine = DBENGINE_POSTGRESQL;
			if (port <= 0)
			{
				setDefaultPort();
			}
		}
		else if ((!string.ReferenceEquals(__database_engine_String, null)) && (StringUtil.startsWithIgnoreCase(__database_engine_String,"SQL_Server") || StringUtil.startsWithIgnoreCase(__database_engine_String,"SQLServer")))
		{ // Current config file
			__fieldLeftEscape = "[";
			__fieldRightEscape = "]";
			__stringDelim = "'";
			_database_engine = DBENGINE_SQLSERVER;
			if (port <= 0)
			{
				setDefaultPort();
			}
			else
			{
				// If use has provided a port AND a named instance, e.g. localhost\SQLEXPRESS,
				// warn them about it. -IWS
				if (__database_server.IndexOf('\\') >= 0)
				{
					Message.printWarning(3, "initialize", "SQLServer connection should either " + "provide a named instance OR a port, but not both.");
				}
			}
		}
		else if ((!string.ReferenceEquals(__database_engine_String, null)) && __database_engine_String.Equals("H2", StringComparison.OrdinalIgnoreCase))
		{
			__fieldLeftEscape = "";
			__fieldRightEscape = "";
			__stringDelim = "'";
			_database_engine = DBENGINE_H2;
		}
		else
		{
			if ((!string.ReferenceEquals(__odbc_name, null)) && !__odbc_name.Equals(""))
			{
				// Using a generic ODBC DSN connection so assume some basic defaults
				__fieldLeftEscape = "";
				__fieldRightEscape = "";
				__stringDelim = "'";
				_database_engine = DBENGINE_ODBC;
			}
			else
			{
				// Using a specific driver but don't know which one, which is problematic for internals
				throw new Exception("Trying to use unknown database engine: " + __database_engine_String + " in DMI()");
			}
		}

		__statementsVector = new List<Statement>();
	}

	/// <summary>
	/// Close the database connection.  If the database is not connected yet, don't do anything. </summary>
	/// <exception cref="SQLException"> thrown if the java.sql code has 
	/// any problems doing a Connection.close() </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void close() throws java.sql.SQLException
	public virtual void close()
	{
		// let the JDBC handle the close
		if (__connected)
		{
			__connection.close();
			__connected = false;
		}
	}

	/// <summary>
	/// Closes a result set and frees the resources associated with it. </summary>
	/// <param name="rs"> the ResultSet to close. </param>
	public static void closeResultSet(ResultSet rs)
	{
		try
		{
			if (rs != null)
			{
				Statement s = rs.getStatement();
				rs.close();
				if (s != null)
				{
					s.close();
					s = null;
				}
				rs = null;
			}
		}
		catch (SQLException)
		{
			// Swallow the exception since this is a utility method that is called to clean-up.
		}
	}


	/// <summary>
	/// Closes a result set from a stored procedure and frees the resources associated with it. </summary>
	/// <param name="rs"> the ResultSet to close. </param>
	/// <param name="select"> the select statement that was set up to execute the stored procedure. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void closeResultSet(java.sql.ResultSet rs, DMIStatement select) throws java.sql.SQLException
	public static void closeResultSet(ResultSet rs, DMIStatement select)
	{
		if (rs != null)
		{
			Statement s = rs.getStatement();
			rs.close();
			if (s != null)
			{
				s.close();
				s = null;
			}
			rs = null;
		}

		if (select.getCallableStatement() != null)
		{
			select.getCallableStatement().close();
		}
	}

	/// <summary>
	/// Closes an statements that were opened during a transaction.  Called 
	/// automatically by commit() and rollback().
	/// </summary>
	private void closeStatements()
	{
		string routine = "DMI.closeStatements()";
		int size = __statementsVector.Count;
		Statement s = null;
		for (int i = 0; i < size; i++)
		{
			s = __statementsVector[i];
			try
			{
				s.close();
			}
			catch (Exception e)
			{
				Message.printWarning(3, routine, "Error closing statement:");
				Message.printWarning(3, routine, e);
			}
		}
	}

	/// <summary>
	/// Commits any database operations that have been made since the beginning of the current transaction. </summary>
	/// <exception cref="SQLException"> thrown if the java.sql code has any problems 
	/// doing a Connection.commit() or in setAutoCommit(), or if the database
	/// was not connected when the call was made </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void commit() throws java.sql.SQLException
	public virtual void commit()
	{
		// TODO (JTS - 2006-05-22)This code has not been tested or used in nearly 4 years.  Do not rely
		// on this method without testing it first!
		if (!__connected)
		{
			throw new SQLException("Database not connected, cannot call DMI.commit()");
		}

		__inTransaction = false;

		// Connection.commit() should only be used when autoCommit is turned off, so check that
		if (__autoCommit == false)
		{
			__connection.commit();
		}

		// Since commit() marks the end of a transaction, turn 
		// autoCommit back on and mark the database as not dirty (clean)
		setAutoCommit(true);
		__dirty = false;

		closeStatements();
	}

	/// <summary>
	/// Indicate whether the database is connected. </summary>
	/// <returns> true if the database connection is made, false otherwise. </returns>
	public virtual bool connected()
	{
		return __connected;
	}

	/// <summary>
	/// Determine the version of the database.  This method should be defined in a
	/// derived class and be called when a database connection is made (e.g., in the
	/// derived class open() method).  The database version should be set to 0 (zero) if it cannot be determined.
	/// </summary>
	public abstract void determineDatabaseVersion();

	/// <summary>
	/// Execute a count query to find the number of records specified by the query.
	/// This may be used in conjunction with a select statement to find
	/// out how many records will be pulled back (for doing a progress bar or the
	/// like), so the input SQL string can be a properly-formatted <b><code>SELECT </b></code>
	/// statement, or a normal <b><code>SELECT COUNT</b></code> statement.<para>
	/// If a normal SELECT statement is passed in, 
	/// this method will chop off any <b><codE>ORDER BY</b></code> clauses and
	/// will also remove the fields that are being selected and replace them
	/// </para>
	/// with <b><code>COUNT(*)</b></code>.<para>
	/// For instance, the SQL:<br>
	/// <code>
	/// </para>
	/// SELECT field1, field2 FROM tableName where field3 = 123 ORDER BY field4;<para>
	/// </code> will converted to:<br>
	/// <code> 
	/// </para>
	/// SELECT COUNT(*) FROM tableName where field3 = 123;<para>
	/// </codE> prior to being run.
	/// </para>
	/// <para>
	/// <b>Known issues</b><br>
	/// A problem has been encountered with an ODBC connection to a Microsoft Access
	/// 97 database in which malformed COUNT() statements have returned incorrect
	/// </para>
	/// results.  <para>
	/// An SQL statement of SELECT COUNT(*) ASDKJASD (where ASDKJASD could actually
	/// be pretty much any word or combination of letters) would return a result that
	/// said that 1 record was returned.  It is unclear why this SQL statement was
	/// not returning an error from the database that the SQL was malformed.
	/// </para>
	/// </summary>
	/// <param name="sql"> a Select statement for which the number of records that will
	/// be affected is the result wanted </param>
	/// <returns> an integer telling how many records were counted </returns>
	/// <exception cref="SQLException"> thrown if there are problems with a 
	/// Connection.createStatement or Statement.executeQuery(), or if the database was not connected </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int dmiCount(String sql) throws java.sql.SQLException
	public virtual int dmiCount(string sql)
	{
		// TODO (JTS - 2006-05-22) This code has not been tested or used in nearly 4 years.  Do not rely
		// on this method without testing it first!
		if (!__connected)
		{
			throw new SQLException("Database not connected.  Cannot make call to DMI.dmiCount()");
		}

		string routine = "DMI.dmiCount";
		int dl = 25;
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "SQL to count (pre): '" + sql + "'");
		}

		sql = sql.Trim();
		sql = sql.ToUpper();

		// If the sql statement is already formatted as a SELECT COUNT statement, do nothing with it.
		if (!sql.StartsWith("SELECT COUNT", StringComparison.Ordinal))
		{
			// Otherwise, the sql is formatted as a SELECT statement so the 'SELECT [fields]' part is removed ...
			if (sql.IndexOf("FROM", StringComparison.Ordinal) >= 0)
			{
				sql = sql.Substring(sql.IndexOf("FROM", StringComparison.Ordinal));
				// And then the ORDER BY clauses are removed as well, leaving behind only
				// "FROM [table] WHERE (wheres)"
				if (sql.IndexOf("ORDER BY", StringComparison.Ordinal) >= 0)
				{
					sql = sql.Substring(0, sql.IndexOf("ORDER BY", StringComparison.Ordinal));
				}
			}

			// Graft on the count statement to the beginning and it is functional sql again, or should be
			sql = "SELECT COUNT(*) " + sql;
		}

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "SQL to count (post): '" + sql + "'");
		}

		Statement s = __connection.createStatement();
		ResultSet rs = s.executeQuery(sql);
		rs.next();
		int count = rs.getInt(1);

		closeResultSet(rs);
		if (__inTransaction)
		{
			__statementsVector.Add(s);
		}
		else
		{
			s.close();
		}

		return count;
	}

	/// <summary>
	/// Executes a count query from a DMISelectStatement object.  This creates the 
	/// SQL string from the DMISelectStatement object and then passes the resulting
	/// string to the dmiCount(String) function. </summary>
	/// <param name="s"> a DMISelectStatement object to run a count by </param>
	/// <returns> an integer telling how many records were counted </returns>
	/// <exception cref="SQLException"> thrown if there are problems with a 
	/// Connection.createStatement or Statement.executeQuery() </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int dmiCount(DMISelectStatement s) throws java.sql.SQLException
	public virtual int dmiCount(DMISelectStatement s)
	{
		// TODO (JTS - 2006-05-22) This code has not been tested or used in nearly 4 years.  Do not rely
		// on this method without testing it first!
		if (!__connected)
		{
			throw new SQLException("Database not connected, cannot call DMI.dmiCount()");
		}

		if (s.isStoredProcedure())
		{
			ResultSet rs = s.executeStoredProcedure();
			rs.next();
			return rs.getInt(1);
		}

		// saves the DMISelectStatement as the last statement executed
		setLastStatement(s);

		return dmiCount(s.ToString());
	}

	/// <summary>
	/// Executes a database delete from a DMIDeleteStatement object.  This method
	/// creates the SQL string from the DMIDeleteStatement object and then passes
	/// the resulting string to the dmiDelete(String) function.<para>
	/// The code in dmiDelete is surrounded by a check of the private boolean member 
	/// variable __editable (see isEditable() and setEditable(boolean)).  If
	/// __editable is set to true, none of the code is executed and an exception is thrown.
	/// </para>
	/// </summary>
	/// <param name="s"> a DMIStatement object containing a delete statement (should be a DMIDeleteStatement object) </param>
	/// <returns> the number of rows deleted </returns>
	/// <exception cref="SQLException"> thrown if there are problems with a 
	/// Connection.createStatement or Statement.executeQuery().  Also thrown if
	/// the database is in read-only mode </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int dmiDelete(DMIDeleteStatement s) throws java.sql.SQLException
	public virtual int dmiDelete(DMIDeleteStatement s)
	{
		if (!__connected)
		{
			throw new SQLException("Database not connected, cannot call DMI.dmiDelete()");
		}

		if (!__editable)
		{
			throw new SQLException("Database in read-only mode, cannot execute a dmiDelete");
		}

		if (s.isStoredProcedure())
		{
			s.executeStoredProcedure();
			return s.getIntReturnValue();
		}
		else
		{
			// Save the DMIDeleteStatement as the last statement executed
			setLastStatement(s);
			return dmiDelete(s.ToString());
		}
	}

	/// <summary>
	/// Executes a database delete.  This method runs the SQL DELETE statement in its String parameter.<para>
	/// The code in dmiDelete is surrounded by a check of the private boolean member 
	/// variable __editable (see isEditable() and setEditable(boolean)).  If
	/// __editable is set to true, none of the code is executed and an exception is thrown. 
	/// </para>
	/// </summary>
	/// <param name="sql"> the SQL statement that contains the <b><code>DELETE</b></code> command </param>
	/// <returns> the number of rows deleted </returns>
	/// <exception cref="SQLException"> thrown if there are problems with a 
	/// Connection.createStatement or Statement.executeQuery().  Also thrown if
	/// the database is in read-only mode, or if it is not connected </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int dmiDelete(String sql) throws java.sql.SQLException
	public virtual int dmiDelete(string sql)
	{
		if (!__connected)
		{
			throw new SQLException("Database not connected. Cannot make call to DMI.dmiDelete()");
		}

		if (__dumpSQLOnExecution)
		{
			Message.printStatus(2, "DMI.dmiDelete", sql);
		}

		string routine = "DMI.dmiDelete";
		int dl = 25;
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "SQL: '" + sql + "'");
		}

		if (!__editable)
		{
			throw new SQLException("Database in read-only mode, cannot execute a dmiDelete.");
		}

		Statement s = __connection.createStatement();
		if (__capitalize)
		{
			sql = sql.ToUpper();
		}

		int result = 0;
		try
		{
			result = s.executeUpdate(sql);
		}
		catch (SQLException ex)
		{
			if (__dumpSQLOnError)
			{
				Message.printStatus(2, "DMI.dmiDelete", sql);
			}
			throw ex;
		}

		if (__inTransaction)
		{
			__statementsVector.Add(s);
		}
		else
		{
			s.close();
		}

		// Used for knowing when to do a startTransaction(ROLLBACK) versus a startTransaction(COMMIT).
		// Since a delete statement causes a database change (and if the code has 
		// gotten this far the delete was successful and didn't throw an exception), the database can 
		// now be considered changed and the __dirty flag should be set
		testAndSetDirty();
		return result;
	}

	/// <summary>
	/// Executes a String of SQL code.  Used for doing create and drop table commands. </summary>
	/// <param name="sql"> the command to execute </param>
	/// <returns> the number of lines affected </returns>
	/// <exception cref="Exception"> if an error occurs </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int dmiExecute(String sql) throws java.sql.SQLException
	public virtual int dmiExecute(string sql)
	{
		if (!__connected)
		{
			throw new SQLException("Database not connected.  Cannot make call to DMI.dmiExecute");
		}

		if (!__editable)
		{
			throw new SQLException("Database is in read-only mode, cannot make call to DMI.dmiExecute");
		}

		string routine = "DMI.dmiExecute";
		int dl = 25;

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "SQL: '" + sql + "'");
		}

		Statement s = __connection.createStatement();
		if (__capitalize)
		{
			sql = sql.ToUpper();
		}

		int result = s.executeUpdate(sql);

		if (__inTransaction)
		{
			__statementsVector.Add(s);
		}
		else
		{
			s.close();
		}

		return result;
	}

	/// <summary>
	/// Re-executes the previous SQL statement executed by the database.  
	/// Every time one of the dmiCount, dmiDelete, dmiWrite or dmiSelect 
	/// methods that take a DMIDeleteStatement, DMIWriteStatement or 
	/// DMISelectStatement as their parameter is called is called, 
	/// the DMI stores the DMI*Statement.  This method re-runs the DMI*Statement
	/// executed just previously by the database.
	/// <para>
	/// If the database is accessed via one of the dmiCount, dmiDelete, or
	/// </para>
	/// dmiWrite statements that takes a String argument then no previous-sql information is saved.  <para>
	/// If the database is accessed via a dmiSelect statement that takes a String
	/// argument, the SELECT statement is saved, but cannot be used by this method.
	/// It saved for use in the dmiRunLastSelectForTableModel function.
	/// </para>
	/// <b>Note:</b><para>
	/// If using stored procedures, this method WILL NOT WORK.  It only works with
	/// DMI connections that pass String-based SQL statements (either as Strings or DMIStatement objects).
	/// </para>
	/// </summary>
	/// <returns> an object, the type of which depends on which kind of query was
	/// being executed.  The problem here is that SELECT queries return 
	/// Vector objects while all other SQL statements return integers.
	/// Any method which calls this function should first call <b><code>
	/// getLastSQLType() </b></code> so that it knows what sort of object will
	/// be returned, and then cast the return type appropriately.<para>
	/// <code>
	/// int type = rdmi.getLastSQLType();<br>
	/// if (type == DMI.SELECT) {<br>
	/// &nbsp;&nbsp;&nbsp;Vector results = (Vector) dmiRunSQL();<br>
	/// } else { // for all other types<br>
	/// &nbsp;&nbsp;&nbsp;Integer i = (Integer) dmiRunSQL();<br>
	/// }<br>
	/// </para>
	/// </returns>
	/// <exception cref="SQLException"> thrown if there are problems with a 
	/// Connection.createStatement, Statement.executeQuery(),
	/// Statement.executeDelete() or Statement.executeUpdate() </exception>
	/// <exception cref="Exception"> thrown by dmiWrite(DMIWriteStatement) when a DELETE_INSERT is attempted </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object dmiRunLastSQL() throws java.sql.SQLException, Exception
	public virtual object dmiRunLastSQL()
	{
		switch (__lastSQLType)
		{
			case NONE:
				return null;
			case SELECT:
				return dmiSelect((DMISelectStatement)__lastSQL);
			case WRITE:
				return new int?(dmiWrite((DMIWriteStatement)__lastSQL, UPDATE_INSERT));
			case DELETE:
				return new int?(dmiDelete((DMIDeleteStatement)__lastSQL));
			case COUNT:
				return new int?(dmiCount((DMISelectStatement)__lastSQL));
		}
		return null;
	}

	/// <summary>
	/// Re-executes the last SELECT statement executed.  Used for re-querying
	/// a table after a DELETE or INSERT statement is executed.  
	/// <code>dmiRunSQL</code> can't be used at that point as the last
	/// SQL executed will have been the DELETE or INSERT statement </summary>
	/// <returns> a ResultSet of the values returned from the query </returns>
	/// <exception cref="SQLException"> thrown if there are problems with a 
	/// Connection.createStatement or Statement.executeQuery() </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.sql.ResultSet dmiRunSelect() throws java.sql.SQLException
	public virtual ResultSet dmiRunSelect()
	{
		if (!__connected)
		{
			throw new SQLException("Database not connected.  Cannot make call to DMI.dmiRunSelect()");
		}
		return dmiSelect((DMISelectStatement)__lastQuery);
	}

	// TODO SAM 2012-09-07 Need to set the last query string
	/// <summary>
	/// Runs an SQL string that contains a <b><code>SELECT</b></code> statement 
	/// and returns a resultSet of the records returned. </summary>
	/// <param name="sql"> an SQL statement that contains the <b><code>SELECT</b></code> statement to be executed </param>
	/// <returns> the resultset pulled back from the operation </returns>
	/// <exception cref="SQLException"> thrown if there are problems with a 
	/// Connection.createStatement or Statement.executeQuery(), or if the database is not connected. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.sql.ResultSet dmiSelect(String sql) throws java.sql.SQLException
	public virtual ResultSet dmiSelect(string sql)
	{
		string routine = "DMI.dmiSelect";
		if (!__connected)
		{
			throw new SQLException("Database not connected.  Cannot make call to DMI.dmiSelect()");
		}

		if (__dumpSQLOnExecution)
		{
			Message.printStatus(2, routine, sql);
		}

		int dl = 25;
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "SQL: '" + sql + "'");
		}

		Statement s = __connection.createStatement();
		ResultSet rs = null;
		if (__capitalize)
		{
			sql = sql.ToUpper();
		}

		// FIXME SAM 2008-04-15 Evaluate if needed
		//__lastQueryString = sql;

		try
		{
			rs = s.executeQuery(sql);
		}
		catch (SQLException ex)
		{
			if (__dumpSQLOnError)
			{
				Message.printStatus(2, routine, sql);
			}
			throw ex;
		}
		// The statement will automatically be closed so don't do here

		return rs;
	}

	/// <summary>
	/// Execute an SQL select statement from a DMISelectStatement object.  The SQL
	/// statement is built from the DMISelectStatement object and the resulting 
	/// string passed to dmiSelect(String). </summary>
	/// <param name="select"> an DMISelectStatement instance specifying the query. </param>
	/// <returns> the ResultSet from the select. </returns>
	/// <exception cref="SQLException"> thrown if there are problems with a 
	/// Connection.createStatement or Statement.executeQuery() </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.sql.ResultSet dmiSelect(DMISelectStatement select) throws java.sql.SQLException
	public virtual ResultSet dmiSelect(DMISelectStatement select)
	{
		if (!__connected)
		{
			throw new SQLException("Database not connected.  Cannot make call to DMI.dmiSelect()");
		}

		if (select.isStoredProcedure())
		{
			return select.executeStoredProcedure();
		}
		else
		{
			// sets the DMISelectStatement as the last statement executed
			setLastStatement(select);
			return dmiSelect(select.ToString());
		}
	}

	/// <summary>
	/// Runs an SQL statement that contains a <b><code>INSERT</b></code> or <b><code> UPDATE</b></code>.<para>
	/// The code in dmiWrite is surrounded by a check of the private boolean member 
	/// variable __editable (set isEditable() and setEditable(boolean).  If
	/// __editable is set to true, none of the code is executed and an exception is thrown.  
	/// </para>
	/// </summary>
	/// <param name="sql"> an SQL command that contains the insert or update command to be run </param>
	/// <returns> an integer of the rowcount from the insert or update </returns>
	/// <exception cref="SQLException"> thrown if there are problems with a 
	/// Connection.createStatement or Statement.executeUpdate().  Also thrown if
	/// the database is in read-only mode, or if the database is not connected </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int dmiWrite(String sql) throws java.sql.SQLException
	public virtual int dmiWrite(string sql)
	{
		if (!__connected)
		{
			throw new SQLException("Database not connected.  Cannot make call to DMI.dmiWrite");
		}

		if (!__editable)
		{
			throw new SQLException("Database is in read-only mode, cannot make call to DMI.dmiWrite");
		}

		if (__dumpSQLOnExecution)
		{
			Message.printStatus(2, "DMI.dmiWrite", sql);
		}

		string routine = "DMI.dmiWrite";
		int dl = 25;

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "SQL: '" + sql + "'");
		}

		Statement s = __connection.createStatement();
		if (__capitalize)
		{
			sql = sql.ToUpper();
		}

		int result = 0;
		try
		{
			result = s.executeUpdate(sql);
		}
		catch (SQLException ex)
		{
			if (__dumpSQLOnError)
			{
				Message.printStatus(2, "DMI.dmiWrite", sql);
			}
			throw ex;
		}

		if (__inTransaction)
		{
			__statementsVector.Add(s);
		}
		else
		{
			s.close();
		}

		return result;
	}

	/// <summary>
	/// Executes an insert or update statement from a DMIWriteStatement object.
	/// The SQL string stored in the DMIWriteStatement object is generated and the
	/// resulting string passed into a dmiWrite(String) function that does the work.<para>
	/// The code in dmiWrite is surrounded by a check of the private boolean member 
	/// variable __editable (set isEditable() and setEditable(boolean).  If
	/// __editable is set to true, none of the code is executed and an exception is thrown.  
	/// </para>
	/// </summary>
	/// <param name="s"> a DMIWriteStatement object to be executed </param>
	/// <param name="writeFlag"> not used with stored procedures; for SQL can be INSERT_UPDATE, UPDATE_INSERT, UPDATE,
	/// INSERT, DELETE_INSERT to indicate order of operations (can impact performance). </param>
	/// <returns> an integer of the rowcount from the insert or update </returns>
	/// <exception cref="SQLException"> thrown if there are problems with a 
	/// Connection.createStatement or Statement.executeQuery().  Also thrown if the
	/// database is in read-only mode, or if the database is not connected </exception>
	/// <exception cref="Exception"> thrown if a DELETE_INSERT statement is run, as this statement type is not supported yet.  </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int dmiWrite(DMIWriteStatement s, int writeFlag) throws java.sql.SQLException, Exception
	public virtual int dmiWrite(DMIWriteStatement s, int writeFlag)
	{
		if (!__connected)
		{
			throw new SQLException("Database not connected.  Cannot make call to DMI.dmiWriteStatement()");
		}
		if (!__editable)
		{
			throw new SQLException("Database is in read-only mode");
		}

		if (s.isStoredProcedure())
		{
			s.executeStoredProcedure();
			return s.getIntReturnValue();
		}

		// Set the DMIWriteStatement as the last statement executed
		setLastStatement(s);

		Statement stmt = __connection.createStatement();

		switch (writeFlag)
		{
			case INSERT_UPDATE:
			// first try to insert the statement in the table.  

			////////////////////////////////////////////////////////////////
			// NOTE FOR DEVELOPERS:
			////////////////////////////////////////////////////////////////
			// This database write method works by first trying to insert a 
			// a record, and if that record already existed in the database,
			// an update is performed.  
			//
			// The tricky part is that each database sends different error
			// codes to tell when a duplicate record is found on an insert.
			// To add a new database type to this section, a duplicate record
			// insert must attempted on the new database type and the SQLState
			// and ErrorCode from the resulting SQLException must be recorded
			// and then used below (where, for instance, ODBC has S1000 and 0)
			///////////////////////////////////////////////////////////////	
			try
			{
				if (__dumpSQLOnExecution)
				{
					Message.printStatus(2, "DMI.dmiWrite", "Trying to execute INSERT: " + s.toInsertString());
				}
				stmt.executeUpdate(s.toInsertString());
				// Used for knowing when to do a startTransaction(ROLLBACK) versus a startTransaction(COMMIT).
				// Since a delete statement causes a database change (and if the code has gotten this far
				// the delete was successful and didn't throw an exception), the database can now be
				// considered changed and the __dirty flag should be set		
				testAndSetDirty();
			}
			catch (SQLException e)
			{
				if (_database_engine == DBENGINE_ACCESS)
				{
					if (e.getSQLState() == "S1000" && e.getErrorCode() == 0)
					{
						// The Insert failed because a record with the existing key data already exists.
						// That record will be updated, instead.
						try
						{
							if (__dumpSQLOnExecution)
							{
								Message.printStatus(2, "DMI.dmiWrite", s.toUpdateString());
							}
							stmt.executeUpdate(s.toUpdateString());
						}
						catch (Exception ex)
						{
							if (__dumpSQLOnError)
							{
								Message.printStatus(2, "DMI.dmiWrite", s.toUpdateString());
							}
							throw ex;
						}

						// Used for knowing when to do a startTransaction(ROLLBACK) versus a 
						// startTransaction(COMMIT).  Since a delete statement causes a database change (and if 
						// the code has gotten this far the delete was successful and didn't throw an exception), 
						// the database can now be considered changed and the __dirty flag should be set
						testAndSetDirty();
					}
					else
					{
						stmt.close();
						throw e;
					}
				}
				// TODO SAM 2009-05-14 Evaluate whether this code is needed/fragile/etc.
				else if ((_database_engine == DBENGINE_SQLSERVER))
				{
					if (e.getSQLState() == "23000" && e.getErrorCode() == 2627)
					{
						// The Insert failed because a record with the existing key data already exists.
						// That record will be updated, instead.
						try
						{
							if (__dumpSQLOnExecution)
							{
								Message.printStatus(2, "DMI.dmiWrite", s.toUpdateString());
							}
							stmt.executeUpdate(s.toUpdateString());
						}
						catch (Exception ex)
						{
							if (__dumpSQLOnError)
							{
								Message.printStatus(2, "DMI.dmiWrite", s.toUpdateString());
							}
							throw ex;
						}

						// Used for knowing when to do a startTransaction(ROLLBACK) versus a 
						// startTransaction(COMMIT).  Since a delete statement causes a database change (and if 
						// the code has gotten this far the delete was successful and didn't throw an exception), 
						// the database can now be considered changed and the __dirty flag should be set
						testAndSetDirty();
					}
					else
					{
						stmt.close();
						throw (e);
					}
				}
				else
				{
					throw new Exception("INSERT_UPDATE may have encountered an " + "existing record, but since there is no information " + "on the INSERT error codes for the database type " + "with which you are working (" + __database_engine_String + ") " + "there is no certainty about this.  DMI needs to be enhanced to provide feedback.");
				}
			}
			break;
			case UPDATE_INSERT:
				int result = 0;
				try
				{
					if (__dumpSQLOnExecution)
					{
						Message.printStatus(2, "DMI.dmiWrite", s.toUpdateString(true));
					}
					result = stmt.executeUpdate(s.toUpdateString(true));
				}
				catch (Exception e)
				{
					if (__dumpSQLOnError)
					{
						Message.printStatus(2, "DMI.dmiWrite", s.toUpdateString());
					}
					throw e;
				}

				if (result == 0)
				{
					// The update failed, so try an insert.
					if (__dumpSQLOnExecution)
					{
						Message.printStatus(2, "DMI.dmiWrite", s.toInsertString());
					}
					try
					{
						result = stmt.executeUpdate(s.toInsertString());
					}
					catch (Exception e)
					{
						if (__dumpSQLOnError)
						{
							Message.printStatus(2, "DMI.dmiWrite", s.toInsertString());
						}
						throw e;
					}
				}
				// Used for knowing when to do a startTransaction(ROLLBACK) versus a startTransaction(COMMIT).
				// Since a delete statement causes a database change (and if the code has gotten this far
				// the delete was successful and didn't throw an exception), the database can now be
				// considered changed and the __dirty flag should be set.		
				testAndSetDirty();
				break;
			case UPDATE:
				try
				{
					if (__dumpSQLOnExecution)
					{
						Message.printStatus(2, "DMI.dmiWrite", s.toUpdateString());
					}
					stmt.executeUpdate(s.toUpdateString());
				}
				catch (Exception e)
				{
					if (__dumpSQLOnError)
					{
						Message.printStatus(2, "DMI.dmiWrite", s.toUpdateString());
					}
					throw e;
				}

				// Used for knowing when to do a startTransaction(ROLLBACK) versus a startTransaction(COMMIT).
				// Since a delete statement causes a database change (and if the code has gotten this far
				// the delete was successful and didn't throw an exception), the database can now be
				// considered changed and the __dirty flag should be set.
				testAndSetDirty();
				break;
			case INSERT:
				try
				{
					if (__dumpSQLOnExecution)
					{
						Message.printStatus(2, "DMI.dmiWrite", s.toInsertString());
					}
					stmt.executeUpdate(s.toInsertString());
				}
				catch (Exception e)
				{
					if (__dumpSQLOnError)
					{
						Message.printStatus(2, "DMI.dmiWrite", s.toInsertString());
					}
					throw e;
				}
				// Used for knowing when to do a startTransaction(ROLLBACK) versus a startTransaction(COMMIT).
				// Since a delete statement causes a database change (and if the code has gotten this far
				// the delete was successful and didn't throw an exception), the database can now be
				// considered changed and the __dirty flag should be set.	
				testAndSetDirty();
				break;
			case DELETE_INSERT:
				Message.printWarning(25, "DMI.dmiWrite", "DELETE_INSERT not implemented yet");
				throw new Exception("DELETE_INSERT not implemented");
			default:
				throw new Exception("Unspecified WRITE type in DMI.dmiWrite:" + writeFlag);
		}

		if (__inTransaction)
		{
			__statementsVector.Add(stmt);
		}
		else
		{
			stmt.close();
		}

		// TODO Remove the integer return values, or not?
		// The argument can be made for keeping and removing the return values.
		//
		// Against:
		// They are unnecessary.  When working with write statements, no return
		// is anticipated and therefore is unlikely to be used.
		//
		// For:
		// It's just a little more debug information.  The return value from a
		// update or insert is how many records were updated or inserted.  
		// While that number should be known for insert statements, perhaps
		// there could be a time when the number of updated records would be useful information to have.
		return 0;
	}

	/// <summary>
	/// Applies escape sequences to a string based on the kind of database being used, as follows:
	/// <li>
	/// <ol> SQL Server engine - translate all instances of ' (apostrophe) to '' (two apostrophes).</li>
	/// </li>
	/// This method is intended to apply to simple strings (e.g., query parameters) and does not deal with
	/// escaping full SQL strings (e.g., to add [] around parameter names that may be reserved words). </summary>
	/// <param name="str"> the string to check for characters that need escaped. </param>
	/// <returns> a string with the appropriate escape sequences inserted. </returns>
	public virtual string escape(string str)
	{
		string workStr = str;
		if (_database_engine == DBENGINE_SQLSERVER)
		{
			if (workStr.IndexOf('\'') > -1)
			{
				workStr = StringUtil.replaceString(str, "'", "''");
			}
		}

		// any other escaping can be done here, too ...

		return workStr;
	}

	/// <summary>
	/// Clean up memory for garbage collection. </summary>
	/// <exception cref="Throwable"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~DMI()
	{
		__database_engine_String = null;
		__database_name = null;
		__odbc_name = null;
		__database_server = null;
		__system_login = null;
		__system_password = null;
		__user_login = null;
		__user_password = null;
		__lastSQL = null;
		__lastQuery = null;
		__connection = null;

//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns all the kinds of databases a DMI can connect to. </summary>
	/// <returns> a list of all the kinds of databases a DMI can connect to. </returns>
	protected internal static IList<string> getAllDatabaseTypes()
	{
		IList<string> v = new List<string>(9);
		v.Add("Access");
		v.Add("Derby");
		v.Add("Excel");
		v.Add("H2");
		v.Add("Informix");
		v.Add("MySQL");
		v.Add("Oracle");
		v.Add("PostgreSQL");
		v.Add("SQLServer");
		return v;
	}

	/// <summary>
	/// Returns the current value of the autoCommit setting </summary>
	/// <returns> the value of the autoCommit setting </returns>
	public virtual bool getAutoCommit()
	{
		return __autoCommit;
	}

	/// <summary>
	/// Returns whether the SQL is being capitalized or not. </summary>
	/// <returns> whether the SQL is being capitalized or not. </returns>
	public virtual bool getCapitalize()
	{
		return __capitalize;
	}

	/// <summary>
	/// Returns the connection being used to interact with the database </summary>
	/// <returns> the connection being used to interact with the database </returns>
	public virtual Connection getConnection()
	{
		return __connection;
	}

	/// <summary>
	/// Returns the meta data associated with the currently-opened connection, or null
	/// if there is no open connection. </summary>
	/// <returns> the meta data associated with the currently-opened connection, or null
	/// if there is no open connection. </returns>
	public virtual DatabaseMetaData getDatabaseMetaData()
	{
		if (__connected)
		{
			try
			{
				return __connection.getMetaData();
			}
			catch (SQLException)
			{
				return null;
			}
		}
		return null;
	}

	/// <summary>
	/// Return the name of the database.  This is used when the database name on the
	/// server has been set with setDatabaseName() for cases when the connection URL is
	/// completed automatically and a predefined ODBC DSN is not used. </summary>
	/// <returns> the name of the database. </returns>
	public virtual string getDatabaseName()
	{
		return __database_name;
	}

	/// <summary>
	/// Return a list of Strings containing properties from the DMI object.
	/// DMI classes that extend this class should define this method for use in general
	/// database properties displays.  Currently there is no standard envisioned for the
	/// database information, only that it provide the user with useful information
	/// about the database connection.  The contents returned here depend on whether the
	/// connection is secure (if not, login and password information are not printed). </summary>
	/// <returns> a list containing database properties as Strings. </returns>
	/// <param name="level"> A numerical value that can be used to control the amount
	/// of output, to be defined by specific DMI instances.  A general guideline is to
	/// use 3 for full output, including database version, history of changes, server
	/// information (e.g., for use in a properties dialog); 2 for a concise output
	/// including server name (e.g., for use in the header of an output file; 1 for
	/// very concise output (e.g., the database name and version, for use in a product
	/// footer).  This arguments defaults to 3 if this base class method is called. </param>
	public virtual IList<string> getDatabaseProperties(int level)
	{
		IList<string> v = new List<string>();
		if (__jdbc_odbc)
		{ // Need the server name, database name, etc...
			v.Add("Database engine: " + __database_engine_String);
			v.Add("Database server: " + __database_server);
			v.Add("Database name: " + __database_name);
			v.Add("Database port: " + __port);
		}
		else
		{
			v.Add("ODBC DSN: " + __odbc_name);
		}
		// Always have this...
		v.Add("Database version: " + __database_version);

		// Secure information...

		if (__secure)
		{
			v.Add("System login: " + __system_login);
			v.Add("System password: " + __system_password);
			v.Add("User login: " + __user_login);
			v.Add("User password: " + __user_password);
		}

		return v;
	}

	/// <summary>
	/// Return the database engine type.  One of:<br>
	/// <ul>
	/// <li>Access</li>
	/// <li>H2</li>
	/// <li>Informix</li>
	/// <li>MySQL</li>
	/// <li>Oracle</li>
	/// <li>PostgreSQL</li>
	/// <li>SQLServer</li>
	/// </ul> </summary>
	/// <returns> the database engine type as a string. </returns>
	public virtual string getDatabaseEngine()
	{
		return __database_engine_String;
	}

	/// <summary>
	/// TODO SAM 2009-05-20 Evaluate using an object to hold the type and string representation.
	/// Return the database engine type as an integer. </summary>
	/// <returns> the database engine type as a integer. </returns>
	public virtual int getDatabaseEngineType()
	{
		return _database_engine;
	}

	/// <summary>
	/// Return the database server name (IP address or DNS-resolvable machine name).
	/// The server name is only required if setDataseName() is also called.
	/// Otherwise, the connection is assumed to use a predefined ODBC DSN,
	/// which can be retrieved using getODBCName(). </summary>
	/// <returns> the database server name (IP address or DNS-resolvable machine name). </returns>
	public virtual string getDatabaseServer()
	{
		return __database_server;
	}

	/// <summary>
	/// Return the database version number. </summary>
	/// <returns> the database version number. </returns>
	public virtual long getDatabaseVersion()
	{
		return __database_version;
	}

	/// <summary>
	/// Return the status of the dirty flag </summary>
	/// <returns> the status of the dirty flag </returns>
	public virtual bool getDirty()
	{
		return __dirty;
	}

	/// <summary>
	/// Returns the ID string that identifies the connection. </summary>
	/// <returns> the ID string that identifies the connection. </returns>
	public virtual string getID()
	{
		return __id;
	}

	/// <summary>
	/// Returns the name of the connection. </summary>
	/// <returns> the name of the connection. </returns>
	public virtual string getInputName()
	{
		return __inputName;
	}

	/// <summary>
	/// Indicate whether this is a jdbc_odbc connection or not. </summary>
	/// <returns> true if the database connection uses JDBC/ODBC (meaning that the
	/// connection is created with a database server and database name.  Return false if
	/// the connection is made by specifying an ODBC DSN. </returns>
	public virtual bool getJDBCODBC()
	{
		return __jdbc_odbc;
	}

	/// <summary>
	/// Returns the last query string that was executed </summary>
	/// <returns> the last query string that was executed </returns>
	public virtual string getLastQueryString()
	{
		if (__lastQuery == null)
		{
			return "";
		}
		else
		{
			return __lastQuery.ToString();
		}
	}

	/// <summary>
	/// Returns the last SQL string that was executed.  Returns "" if the DMI 
	/// connection is using stored procedures. </summary>
	/// <returns> the last SQL string that was executed. </returns>
	public virtual string getLastSQLString()
	{
		switch (__lastSQLType)
		{
			case NONE:
				return "";
			case WRITE:
				DMIWriteStatement w = (DMIWriteStatement)__lastSQL;
				return w.toUpdateString() + " / " + w.toInsertString();
			case SELECT:
			case COUNT:
				DMISelectStatement s = (DMISelectStatement)__lastSQL;
				return s.ToString();
			case DELETE:
				DMIDeleteStatement d = (DMIDeleteStatement)__lastSQL;
				return d.ToString();
		}
		return "";
	}

	/// <summary>
	/// Returns the type of the last SQL string that was executed </summary>
	/// <returns> the type of the last SQL string that was executed </returns>
	public virtual int getLastSQLType()
	{
		return __lastSQLType;
	}

	/// <summary>
	/// Returns the field left escape string </summary>
	/// <returns> the field left escape string </returns>
	public virtual string getFieldLeftEscape()
	{
		return __fieldLeftEscape;
	}

	/// <summary>
	/// Returns the field right escape string </summary>
	/// <returns> the field right escape string </returns>
	public virtual string getFieldRightEscape()
	{
		return __fieldRightEscape;
	}

	/// <summary>
	/// TODO SAM 2009-05-20 Maybe name should be allowed as the longer display name, as opposed to the shorter ID.
	/// Returns the name of the connection. </summary>
	/// <returns> the name of the connection. </returns>
	/// @deprecated (2005-04-07) 
	public virtual string getName()
	{
		return __name;
	}

	/// <summary>
	/// Return the ODBC Data Source Name (DSN).
	/// This is used with a predefined ODBC DSN on the machine. </summary>
	/// <returns> the ODBC DSN for the database connection. </returns>
	public virtual string getODBCName()
	{
		return __odbc_name;
	}

	/// <summary>
	/// Returns the port of the database connection. </summary>
	/// <returns> the port of the database connection. </returns>
	public virtual int getPort()
	{
		return __port;
	}

	/// <summary>
	/// Returns the setting of the secure variable </summary>
	/// <returns> the setting of the secure variable </returns>
	public virtual bool getSecure()
	{
		return __secure;
	}

	/// <summary>
	/// Returns the database types a DMI can connect to that are done via direct server connection (no predefined
	/// ODBC connection is needed). </summary>
	/// <returns> a list of the database types a DMI can connect to that are done via direct server connection. </returns>
	protected internal static IList<string> getServerDatabaseTypes()
	{
		IList<string> v = new List<string>();
		// Do not include Access since this requires that an ODBC connection be defined.
		v.Add("H2");
		v.Add("Informix");
		v.Add("MySQL");
		v.Add("Oracle");
		v.Add("PostgreSQL");
		v.Add("SQLServer");
		return v;
	}

	/// <summary>
	/// Returns the string delimiter </summary>
	/// <returns> the string delimiter </returns>
	public virtual string getStringIdDelim()
	{
		return __stringDelim;
	}

	/// <summary>
	/// Return the system login name for the database connection. </summary>
	/// <returns> the login name for the database connection. </returns>
	public virtual string getSystemLogin()
	{
		return __system_login;
	}

	/// <summary>
	/// Return the password for the database connection. </summary>
	/// <returns> the password for the database connection. </returns>
	public virtual string getSystemPassword()
	{
		return __system_password;
	}

	/// <summary>
	/// Return the user login name. </summary>
	/// <returns> the user login name. </returns>
	public virtual string getUserLogin()
	{
		return __user_login;
	}

	/// <summary>
	/// Return the user password. </summary>
	/// <returns> the user password. </returns>
	public virtual string getUserPassword()
	{
		return __user_password;
	}

	/// <summary>
	/// Indicate whether the database version is at least the indicated version. </summary>
	/// <returns> true if the database version is at least that indicated. </returns>
	public virtual bool isDatabaseVersionAtLeast(long version)
	{
		// The database versions are just numbers so can check arithmetically...
		if (__database_version >= version)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Indicate whether or not the database is set to read-only mode.  If isEditable()
	/// returns true, then no calls to dmiWrite or dmiDelete will be executed. </summary>
	/// <returns> true if the database is in read-only mode, false if not </returns>
	public virtual bool isEditable()
	{
		return __editable;
	}

	/// <summary>
	/// Indicate whether the DMI is connected to a database. </summary>
	/// <returns> true if the database connection has been established. </returns>
	public virtual bool isOpen()
	{
		return __connected;
	}

	/// <summary>
	/// Opens the connection to the DMI with information that was previously set, 
	/// and sets printStatus to the given value while open() (the other version of the
	/// method, not this one) is being called, in order to print more debugging
	/// information.  Once this method is done, __printStatus will be set to false.<para>
	/// __printStatus is primarily used in printStatusOrDebug(), used internally for
	/// debugging DMI connection open()s.
	/// </para>
	/// </summary>
	/// <param name="printStatus"> whether to print information to status while in open(). </param>
	/// <exception cref="SQLException"> if the DMI is already connected. </exception>
	/// <exception cref="Exception"> if some other error happens while connecting to the database. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void open(boolean printStatus) throws Exception, java.sql.SQLException
	public virtual void open(bool printStatus)
	{
		__printStatus = printStatus;
		try
		{
			open();
		}
		catch (SQLException se)
		{
			__printStatus = false;
			throw se;
		}
		catch (Exception e)
		{
			__printStatus = false;
			throw e;
		}
		__printStatus = false;
	}

	/// <summary>
	/// Open a connection to the database with information that was previously specified. </summary>
	/// <exception cref="SQLException"> if the DMI is already connected. </exception>
	/// <exception cref="Exception"> if some other error happens while connecting to the database. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void open() throws Exception, java.sql.SQLException
	public virtual void open()
	{
		string routine = CLASS + ".open";

		printStatusOrDebug(10, routine, "DMI.open() -----------------------");
		printStatusOrDebug(10, routine, "Checking for existing connection ...");

		if (__connected)
		{
			printStatusOrDebug(10, routine, "Already connected!  Throwing exception.");
			throw new SQLException("Must close the first DMI connection before opening a new one.");
		}

		printStatusOrDebug(10, routine, "         ... no connection found.");

		open(__system_login, __system_password);

		// Determine the database version (the derived class method will be called if defined)...
		printStatusOrDebug(10, routine, "Determining database version ... (abstract method, outside of DMI class)");
		determineDatabaseVersion();

		printStatusOrDebug(10, routine, "Reading global data ... (abstract method, outside of DMI class)");
		readGlobalData();
		__lastQuery = null;
		__lastSQL = null;
		__lastSQLType = NONE;
		printStatusOrDebug(10, routine, "----------------------- DMI.open()");
	}

	/// <summary>
	/// Open a connection to the database with the specified system login and password.
	/// All other connection parameters must have been set previously. </summary>
	/// <param name="system_login"> System login. </param>
	/// <param name="system_password"> System password. </param>
	/// <exception cref="SQLException"> thrown by DriverManger.getConnection() or Connection.setAutoCommit() </exception>
	/// <exception cref="Exception"> thrown when attempting to connecting to a database
	/// for which no JDBC information is known. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void open(String system_login, String system_password) throws java.sql.SQLException, Exception
	public virtual void open(string system_login, string system_password)
	{
		string routine = CLASS + ".open(String, String)";
		printStatusOrDebug(10, routine, "Checking for existing connection ...");
		if (__connected)
		{
			printStatusOrDebug(10, routine, "Already connected!  Throwing exception.");
			throw new SQLException("Must close the first DMI connection before opening a new one.");
		}

		printStatusOrDebug(10, routine, "         ... no connection found.");

		string connUrl = "";
		int dl = 10;

		if (__secure && (Message.isDebugOn || __printStatus))
		{
			printStatusOrDebug(dl, routine, "SystemLogin: " + system_login + "," + "SystemPassword: " + system_password + ")");
		}

		// Set up the database-specific connection strings...

		if (__jdbc_odbc)
		{
			printStatusOrDebug(dl, routine, "Using JDBC ODBC connection.");
			// The URL is formed using several pieces of information...
			if (_database_engine == DBENGINE_ACCESS)
			{
				printStatusOrDebug(dl, routine, "Database engine is type 'DBENGINE_ACCESS'");
				// Always require an ODBC DSN (although this may be
				// a problem with some config files and software where
				// an MDB file is allowed for the database name).  This
				// case is therefore the same as if __jdbc_odbc = false.
				Type.GetType("sun.jdbc.odbc.JdbcOdbcDriver");
				connUrl = "jdbc:odbc:" + __database_name;
				Message.printStatus(2, routine, "Opening ODBC connection for Microsoft Access JDBC/ODBC and \"" + connUrl + "\"");
			}
			if (_database_engine == DBENGINE_DERBY)
			{
				printStatusOrDebug(dl, routine, "Database engine is type 'DBENGINE_DERBY'");
				// If the server name is "memory" then the in-memory URL is used
				// TODO SAM 2014-04-22 Figure out how to handle better
				// TODO SAM 2014-04-22 Figure out how to open vs create
				System.setProperty("derby.system.home", "C:\\derby");
				// Load the database driver class into memory...
				Type.GetType("org.apache.derby.jdbc.EmbeddedDriver");
				if (__database_server.Equals("memory", StringComparison.OrdinalIgnoreCase))
				{
					connUrl = "jdbc:derby:memory:db;create=true";
				}
				else
				{
					throw new SQLException("For Derby currenty only support in-memory databases.");
				}
				Message.printStatus(2, routine, "Opening ODBC connection for Derby JDBC/ODBC and \"" + connUrl + "\"");
			}
			else if (_database_engine == DBENGINE_INFORMIX)
			{
				printStatusOrDebug(dl, routine, "Database engine is type 'DBENGINE_INFORMIX'");
				// Use the free driver that comes from Informix...
				// If Informix is ever enabled, also need to add a
				// property to the configuration which is the "online
				// server", like "ol_hydrobase".  For now just dummy in...
				string server = "ol_hydrobase";
				// Load the database driver class into memory...
				Type.GetType("com.informix.jdbc.IfxDriver");
				connUrl = "jdbc:informix-sqli://"
					+ __database_server + ":"
					+ __port + "/" + __database_name + ":INFORMIXSERVER=" + server;
				// Login and password specified below.
				// +";user=" + login + ";password=" + password;
				Message.printStatus(2, routine, "Opening ODBC connection for Informix using \"" + connUrl + "\"");
			}
			else if (_database_engine == DBENGINE_MYSQL)
			{
				printStatusOrDebug(dl, routine, "Database engine is type 'DBENGINE_MYSQL'");
				// Use the public domain driver that comes with MySQL...
				connUrl = "jdbc:mysql://"
					+ __database_server + ":"
					+ __port + "/" + __database_name;
				Message.printStatus(2, routine, "Opening ODBC connection for MySQL using \"" + connUrl + "\"");
			}
			else if (_database_engine == DBENGINE_POSTGRESQL)
			{
				printStatusOrDebug(dl, routine, "Database engine is type 'DBENGINE_POSTGRESQL'");
				connUrl = "jdbc:postgresql://"
					+ __database_server + ":"
					+ __port + "/" + __database_name;
				Message.printStatus(2, routine, "Opening ODBC connection for PostgreSQL using \"" + connUrl + "\"");
			}
			// All the SQL Server connections are now concentrated into one code block as using the SQL Server
			// 2008 JDBC (jdbc4) driver.  Comments are included below in case troubleshooting needs to occur.
			else if (_database_engine == DBENGINE_SQLSERVER)
			{
				// http://msdn.microsoft.com/en-us/library/ms378428%28SQL.90%29.aspx
				connUrl = "jdbc:sqlserver://" + __database_server;
				// if connecting to a named instance, DO NOT USE PORT!
				// NOTE : it is generally recommended to use the port for speed
				// -IWS
				if (__database_server.IndexOf('\\') < 0)
				{
					// Database instance is NOT specified, for example the server name would be something like:
					//     "localhost\CDSS" (and database name would be "HydroBase_CO_YYYYMMDD")
					// Consequently, use the port number.
					connUrl += ":" + __port;
				}
				connUrl += ";databaseName=" + __database_name;
				Message.printStatus(2, routine, "Opening ODBC connection for SQLServer using \"" + connUrl + "\"");
			}
			/*
			else if (_database_engine == _DBENGINE_SQLSERVER2000 ) {
				printStatusOrDebug(dl, routine, "Database engine is type 'DBENGINE_SQLSERVER2000'");
				// Use the driver distributed by Microsoft...
				Class.forName( "com.microsoft.jdbc.sqlserver.SQLServerDriver");
				connUrl = "jdbc:microsoft:sqlserver://"
					+ __database_server + ":"
					+ __port + ";DatabaseName=" + __database_name;
				Message.printStatus(2, routine,
					"Opening ODBC connection for SQLServer2000 using \"" + connUrl + "\"");
			} 
			else if (_database_engine == _DBENGINE_SQLSERVER_2005 ) {
				printStatusOrDebug(dl, routine, "Database engine is type 'DBENGINE_SQLSERVER 2005'");
				// Use the driver distributed by Microsoft...
				// note the slight differences in class name...
				// other than that, they behave the same?
				Class.forName( "com.microsoft.sqlserver.jdbc.SQLServerDriver");
				connUrl = "jdbc:sqlserver://"
					+ __database_server + ":"
					+ __port + ";DatabaseName=" + __database_name;
				Message.printStatus(2, routine, "Opening ODBC connection for SQLServer using \"" + connUrl + "\"");
			} 
			else if (_database_engine == _DBENGINE_SQLSERVER7 ) {
				printStatusOrDebug(dl, routine, "Database engine is type 'DBENGINE_SQLSERVER7'");
				// This is the older UNA2000 driver...
	
				// NOTE:
				// Una2000 is the driver purchased by the state for
				// connecting to SQL Server 7 databases.  This driver
				// is not something RTi is free to distribute to 
				// customers.  Other customers may need a different
				// driver coded in here or -- better yet -- they should
				// switch to MS SQL Server 2000, which is freely available as SQL Server Express
				// (and older MSDE).  
	
				Class.forName( "com.inet.tds.TdsDriver");
				connUrl = "jdbc:inetdae7:" 
					+ __database_server + ":"
					+ __port + "?database=" + __database_name;
				// This will turn off support for < SQL Server 7...
				// "?sql7=true";
				Message.printStatus(2, routine,
					"Opening ODBC connection for SQLServer7 using \"" + connUrl + "\"" );
			}
			*/
			else if (_database_engine == DBENGINE_H2)
			{
				printStatusOrDebug(dl, routine, "Database engine is type 'DBENGINE_H2'");
				// Load the database driver class into memory...
				Type.GetType("org.h2.Driver");
				java.io.File f = new java.io.File(__database_server);
				connUrl = "jdbc:h2:file:" + f.getAbsolutePath() + ";IFEXISTS=TRUE";
				Message.printStatus(2, routine, "Opening JDBC connection for H2 using \"" + connUrl + "\"");
			}
			else if (_database_engine == DBENGINE_ORACLE)
			{
				printStatusOrDebug(dl, routine, "Database engine is type 'ORACLE'");
				// Load the database driver class into memory...
				Type.GetType("oracle.jdbc.driver.OracleDriver");
				connUrl = "jdbc:oracle:thin:@" + __database_server + ":" + __port + ":" + __database_name;
				Message.printStatus(2, routine, "Opening ODBC connection for Oracle using \"" + connUrl + "\"");
			}
			else
			{
				printStatusOrDebug(dl, routine, "Unknown database engine, throwing exception.");
				throw new Exception("Don't know what JDBC driver to use for database type " + __database_engine_String);
			}
		}
		else
		{
			// The URL uses a standard JDBC ODBC connections, regardless of
			// the database engine (this should not normally be used for
			// Java applications - or need to figure out how to use the
			// ODBC protocol URL knowing only the ODBC DSN for each engine).
			printStatusOrDebug(dl, routine, "Using default Java connection method.");
			Type.GetType("sun.jdbc.odbc.JdbcOdbcDriver");
			connUrl = "jdbc:odbc:" + __odbc_name;
			if ((!string.ReferenceEquals(__additionalConnectionProperties, null)) && (__additionalConnectionProperties.Length > 0))
			{
				connUrl = connUrl + ";" + __additionalConnectionProperties;
			}
			Message.printStatus(2, routine, "Opening ODBC connection using JDBC/ODBC and \"" + connUrl + "\"");
		}
		if (__secure)
		{
			printStatusOrDebug(dl, routine, "Calling getConnection(" + connUrl + ", " + system_login + ", " + system_password + ") via the Java DriverManager.");
		}
		else
		{
			printStatusOrDebug(dl, routine, "Calling getConnection(" + connUrl + ", " + system_login + ", " + "password-not-shown) via the Java DriverManager.");
		}
		// Get the login timeout and reset to requested if specified
		int loginTimeout = DriverManager.getLoginTimeout();
		if (__loginTimeout >= 0)
		{
			DriverManager.setLoginTimeout(__loginTimeout);
		}
		__connection = DriverManager.getConnection(connUrl, system_login, system_password);
		if (__loginTimeout >= 0)
		{
			// Now set back to the original timeout
			DriverManager.setLoginTimeout(loginTimeout);
		}

		/* TODO SAM 2013-10-07 This seems to be old so commenting out
		if (_database_engine == DBENGINE_ORACLE && __database_name != null ) {
			__connection.createStatement().execute("alter session set current_schema = " + __database_name );
		}
		*/
		printStatusOrDebug(dl, routine, "Setting autoCommit to: " + __autoCommit);
		__connection.setAutoCommit(__autoCommit);

		printStatusOrDebug(dl, routine, "Connected!");
		__connected = true;
	}

	/// <summary>
	/// A helper method called by the open() methods that determines whether output
	/// needs to go to debug as normal, or should be forced to status level 2.
	/// See open(boolean), which sets __printStatus. If __printStatus is true, debug will go to status level 2. </summary>
	/// <param name="dl"> the level at which the message should be printed (for debug messages). </param>
	/// <param name="routine"> the routine that is printing the message. </param>
	/// <param name="message"> the message to be printed. </param>
	private void printStatusOrDebug(int dl, string routine, string message)
	{
		if (__printStatus)
		{
			Message.printStatus(2, routine, message);
		}
		else
		{
			Message.printDebug(dl, routine, message);
		}
	}

	/// <summary>
	/// Read global data from the database, that can be used throughout the DMI 
	/// session.  This is called from the open() method.
	/// TODO SAM This is being evaluated.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract void readGlobalData() throws java.sql.SQLException, Exception;
	public abstract void readGlobalData();

	/// <summary>
	/// Attempts to perform a rollback to cancel database changes since the start of the last transaction. </summary>
	/// <exception cref="SQLException"> thrown by Connection.rollback() or 
	/// Connection.setAutoCommit, or when the database is not connected </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void rollback() throws java.sql.SQLException
	public virtual void rollback()
	{
		// TODO (JTS - 2006-05-22) This code has not been tested or used in nearly 4 years.  Do not rely
		// on this method without testing it first!
		if (!__connected)
		{
			throw new SQLException("Database not connected.  Cannot make call to DMI.rollback()");
		}

		string routine = "DMI.rollback";
		int dl = 25;

		__inTransaction = false;
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "[method called]");
		}

		__connection.rollback();

		// since a rollback signifies the end of a transaction, set the autoCommit setting back to being on
		setAutoCommit(true);
		__dirty = false;
		closeStatements();
	}

	/// <summary>
	/// Set additional connection URL properties. </summary>
	/// <param name="additionalConnectionProperties"> a string of form "prop1=value1;prop2=value2" </param>
	public virtual void setAdditionalConnectionProperties(string additionalConnectionProperties)
	{
		this.__additionalConnectionProperties = additionalConnectionProperties;
	}

	/// <summary>
	/// Sets the autoCommit setting for the database. </summary>
	/// <param name="autoCommitSetting"> the autoCommit setting. </param>
	/// <exception cref="SQLException"> thrown by Connection.setAutoCommit(), or if the database is not connected </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setAutoCommit(boolean autoCommitSetting) throws java.sql.SQLException
	public virtual void setAutoCommit(bool autoCommitSetting)
	{
		// TODO (JTS - 2006-05-22) This code has not been tested or used in nearly 4 years.  Do not rely
		// on this method without testing it first!
		// there's the likelihood that the setting of autoCommit will have
		// to be handled differently for certain database types.  This isn't the case right now.
		if (!__connected)
		{
			throw new SQLException("Database not connected.  Cannot make call to DMI.setAutoCommit()");
		}

		string routine = "DMI.setAutoCommit";
		int dl = 25;

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "autoCommit: " + autoCommitSetting);
		}

		__autoCommit = autoCommitSetting;

		if (__database_engine_String.Equals("SOMETHING", StringComparison.OrdinalIgnoreCase))
		{
			// TODO handle this differently -- i.e., does MS Access blow up when trying to setAutoCommit?
		}
		else
		{
			__connection.setAutoCommit(autoCommitSetting);
		}
	}

	/// <summary>
	/// Sets whether SQL should be capitalized before being passed to the database. </summary>
	/// <param name="capitalize"> whether SQL should be capitalized. </param>
	public virtual void setCapitalize(bool capitalize)
	{
		__capitalize = capitalize;
	}

	/// <summary>
	/// Sets the connection used to interact with the database.  in this way, the
	/// connection established with one DMI can be passed to a different one so 
	/// that the second doesn't have to go through the connection process again. </summary>
	/// <param name="c"> the connection to use for communicating with the database </param>
	public virtual void setConnection(Connection c)
	{
		__connection = c;
		if (c != null)
		{
			__connected = true;
		}
	}

	/// <summary>
	/// Set the database connection login timeout, which should be set prior to calling open().
	/// A call to DriverManager.setLoginTimeout() will occur prior to getting the connection and then the timeout will be set back to
	/// the previous value.  This ensures that the value is not interpreted globally. </summary>
	/// <param name="loginTimeout"> connection login timeout in seconds. </param>
	public virtual void setLoginTimeout(int loginTimeout)
	{
		__loginTimeout = loginTimeout;
	}

	/// <summary>
	/// Set the type of the database engine to which the DMI will connect.  Valid types are:<br>
	/// <ul>
	/// <li> Access</ul>
	/// <li> H2</ul>
	/// <li> Informix</ul>
	/// <li> MySQL</ul>
	/// <li> Oracle</ul>
	/// <li> PostgreSQL</ul>
	/// <li> SQLServer</ul>
	/// </ul> </summary>
	/// <param name="database_engine"> the type of database engine. </param>
	/// <exception cref="Exception"> if the database engine is not recognized. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setDatabaseEngine(String database_engine) throws Exception
	public virtual void setDatabaseEngine(string database_engine)
	{
		string routine = "DMI.setDatabaseEngine";
		int dl = 25;

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "database_engine: " + database_engine);
		}
		__database_engine_String = database_engine;
		if (__database_engine_String.Equals("Access", StringComparison.OrdinalIgnoreCase))
		{
			__database_server = "Local";
			_database_engine = DBENGINE_ACCESS;
		}
		else if (__database_engine_String.Equals("Derby", StringComparison.OrdinalIgnoreCase))
		{
			__database_server = "myhost";
			_database_engine = DBENGINE_DERBY;
		}
		else if (__database_engine_String.Equals("Informix", StringComparison.OrdinalIgnoreCase))
		{
			_database_engine = DBENGINE_INFORMIX;
		}
		else if (__database_engine_String.Equals("MySQL", StringComparison.OrdinalIgnoreCase))
		{
			_database_engine = DBENGINE_MYSQL;
		}
		else if (__database_engine_String.Equals("Oracle", StringComparison.OrdinalIgnoreCase))
		{
			_database_engine = DBENGINE_ORACLE;
		}
		else if (__database_engine_String.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
		{
			_database_engine = DBENGINE_POSTGRESQL;
		}
		else if (StringUtil.startsWithIgnoreCase(__database_engine_String,"SQL_Server") || StringUtil.startsWithIgnoreCase(__database_engine_String,"SQLServer"))
		{
			_database_engine = DBENGINE_SQLSERVER;
		}
		else if (__database_engine_String.Equals("H2", StringComparison.OrdinalIgnoreCase))
		{
			_database_engine = DBENGINE_H2;
		}
		else
		{
			throw new Exception("Trying to use unknown database engine: " + __database_engine_String + " in DMI()");
		}
	}

	/// <summary>
	/// Set the name of the database, which is used when a connection URL is
	/// automatically defined and a predefined ODBC DSN is not used.
	/// If an ODBC DSN is used, then the database connection uses the ODBC Data Source
	/// Name (DSN) set with setODBCName(). </summary>
	/// <param name="database_name"> the name of the database </param>
	public virtual void setDatabaseName(string database_name)
	{
		if (Message.isDebugOn)
		{
			Message.printDebug(25, "DMI.setDatabaseName", "database_name: " + database_name);
		}
		__database_name = database_name;
		__jdbc_odbc = true;
	}

	/// <summary>
	/// Set the database version.  This should be called from a derived DMI class
	/// determineDatabaseVersion() method. </summary>
	/// <param name="version"> Database version.  The standard
	/// for the version number is XXXXXXDDDDDDDD where XXXXXXX is typically a 
	/// database version (e.g., RiverTrak 020601) and DDDDDDDD is usually the build date (e.g., 20020625). </param>
	public virtual void setDatabaseVersion(long version)
	{
		__database_version = version;
	}

	/// <summary>
	/// Sets the value of __editable.  If __editable is true, no calls to dmiWrite or dmiDelete will be executed. </summary>
	/// <param name="editable"> value to set __editable to </param>
	public virtual void setEditable(bool editable)
	{
		__editable = editable;
	}

	/// <summary>
	/// Set the IP address or DNS-resolvable machine name of the database server.
	/// Note that this can be set to "Local" if a local database is used (e.g., for Access). </summary>
	/// <param name="database_server"> the IP address or machine name of the database host. </param>
	public virtual void setDatabaseServer(string database_server)
	{
		int dl = 25;

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, "DMI.setDatabaseServer", "Database server: \"" + database_server + "\"");
		}
		__database_server = database_server;
	}

	/// <summary>
	/// Sets whether to print out the SQL string that caused an exception to be
	/// thrown after a database command.  If true, the string will be printed at
	/// Message.printStatus(2).  Defaults to false.  For use in debugging. </summary>
	/// <param name="dumpSQL"> whether to dump the SQL after there is an error. </param>
	public virtual void setDumpSQLOnError(bool dumpSQL)
	{
		__dumpSQLOnError = dumpSQL;
	}

	/// <summary>
	/// Sets whether to print out the SQL string that caused an exception to be
	/// thrown after a database command.  If true, the string will be printed
	/// at Message.printStatus(2).  Defaults to false.  For use in debugging. </summary>
	/// <param name="dumpSQL"> whether to dump the SQL after an error occurs. </param>
	public virtual void dumpSQLOnError(bool dumpSQL)
	{
		setDumpSQLOnError(dumpSQL);
	}

	/// <summary>
	/// Sets whether to print out the SQL string that will be executed prior to any
	/// UPDATEs, INSERTS, SELECTs or DELETEs.  If true, the string will be printed at
	/// Message.printStatus(2).  Defaults to false.  For use in debugging. </summary>
	/// <param name="dumpSQL"> whether to dump the SQL prior to running it. </param>
	public virtual void setDumpSQLOnExecution(bool dumpSQL)
	{
		__dumpSQLOnExecution = dumpSQL;
	}

	/// <summary>
	/// Sets whether to print out the SQL string that will be executed prior to any
	/// UPDATEs, INSERTS, SELECTs or DELETEs.  If true, the string will be printed at
	/// Message.printStatus(2).  Defaults to false.  For use in debugging. </summary>
	/// <param name="dumpSQL"> whether to dump the SQL prior to running it. </param>
	public virtual void dumpSQLOnExecution(bool dumpSQL)
	{
		setDumpSQLOnExecution(dumpSQL);
	}

	/// <summary>
	/// Sets the ID string that identifies the connection. </summary>
	/// <param name="id"> the id string that identifies the connection. </param>
	public virtual void setID(string id)
	{
		__id = id;
	}

	/// <summary>
	/// Sets the name of the connection. </summary>
	/// <param name="inputName"> the name of the connection. </param>
	public virtual void setInputName(string inputName)
	{
		__inputName = inputName;
	}

	/// <summary>
	/// Saves the last delete statement executed. </summary>
	/// <param name="s"> a DMIDeleteStatement to be saved </param>
	private void setLastStatement(DMIDeleteStatement s)
	{
		__lastSQL = (DMIStatement) s;
		__lastSQLType = DELETE;
	}

	/// <summary>
	/// Saves the last select statement executed. </summary>
	/// <param name="s"> a DMISelectStatement to be saved </param>
	private void setLastStatement(DMISelectStatement s)
	{
		__lastSQL = (DMIStatement) s;
		__lastSQLType = SELECT;
		__lastQuery = s;
	}

	/// <summary>
	/// Saves the last write statement executed </summary>
	/// <param name="s"> a DMIWriteStatement to be saved </param>
	private void setLastStatement(DMIWriteStatement s)
	{
		__lastSQL = (DMIStatement) s;
		__lastSQLType = WRITE;
	}

	/// <summary>
	/// TODO SAM 2009-05-20 Evaluate whether the name should be allowed as a longer name compared to the short ID.
	/// Sets the name of the connection. </summary>
	/// <param name="name"> the name of the connection. </param>
	/// @deprecated (2005-04-07) 
	public virtual void setName(string name)
	{
		__name = name;
	}

	/// <summary>
	/// Set the ODBC DSN for the database, which is used when a predefined ODBC DSN is
	/// used to make the connection.  If an ODBC DSN is not used, then the database
	/// connection uses the database server set with setDatabaseServer() and database
	/// name set with setDatabaseName(). </summary>
	/// <param name="odbc_name"> the ODBC DSN for the database (must be defined on the machine). </param>
	public virtual void setODBCName(string odbc_name)
	{
		if (Message.isDebugOn)
		{
			Message.printDebug(25, "DMI.setODBCName", "odbc_name: " + odbc_name);
		}
		__odbc_name = odbc_name;
		__jdbc_odbc = false;
	}

	/// <summary>
	/// Sets the port of the database to connect to </summary>
	/// <param name="port"> the value to set the port to </param>
	public virtual void setPort(int port)
	{
		string routine = "DMI.setPort";
		int dl = 25;

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Port: " + port);
		}
		__port = port;
	}

	/// <summary>
	/// Sets the secure flag </summary>
	/// <param name="secure"> the value to set the secure flag to </param>
	public virtual void setSecure(bool secure)
	{
		string routine = "DMI.setSecure";
		int dl = 25;

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Secure: " + secure);
		}
		__secure = secure;
	}

	/// <summary>
	/// Set the system login name for the database connection. </summary>
	/// <param name="login"> the system login name. </param>
	public virtual void setSystemLogin(string login)
	{
		string routine = "DMI.setSystemLogin";
		int dl = 25;

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Login: '" + login + "'");
		}
		__system_login = login;
	}

	/// <summary>
	/// Set the system password for the database connection. </summary>
	/// <param name="pw"> the system password for the database connection. </param>
	public virtual void setSystemPassword(string pw)
	{
		string routine = "DMI.setSystemPassword";
		int dl = 25;

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Password: '" + pw + "'");
		}
		__system_password = pw;
	}

	/// <summary>
	/// Set the user login name for database use.  This information can be used for record-based permissions. </summary>
	/// <param name="login"> the user login name. </param>
	public virtual void setUserLogin(string login)
	{
		__user_login = login;
	}

	/// <summary>
	/// Set the user password for database use.  This information can be used for record-based permissions. </summary>
	/// <param name="password"> the user password. </param>
	public virtual void setUserPassword(string password)
	{
		__user_password = password;
	}

	/// <summary>
	/// Starts a transaction, first rolling back any changes that may have been 
	/// made to the database since the start of the last transaction. </summary>
	/// <exception cref="SQLException"> thrown in startTransaction() </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startTransaction() throws java.sql.SQLException
	public virtual void startTransaction()
	{
		if (!__connected)
		{
			throw new SQLException("Database not connected.  Cannot make call to DMI.startTransaction()");
		}

		__dirty = false;
		startTransaction(ROLLBACK);

		// TODO (JTS - 2004-07-20) nothing in here to actually START a transaction!  Other transactions
		// are either committed or rolled back, but where is one started?

		__inTransaction = true;
	}

	/// <summary>
	/// Starts a transaction, and either rolls back the current database changes
	/// or commits them, depending on the argument to this method. </summary>
	/// <param name="action"> specifies what to do when the transaction is started.  
	/// values that can be passed in are <code>ROLLBACK</code> or <code>COMMIT
	/// </code>, and they tell what to do before the transaction is started. </param>
	/// <exception cref="SQLException"> thrown in setAutoCommit(), commit() or rollback(),
	/// or when the database is not connected </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startTransaction(int action) throws java.sql.SQLException
	public virtual void startTransaction(int action)
	{
		if (!__connected)
		{
			throw new SQLException("Database not connected.  Cannot make call to DMI.startTransaction()");
		}

		string routine = "DMI.startTransaction";
		int dl = 25;

		// autoCommit and transaction do not work together.  autocommit
		// must be turned off for transactions to function, and so do that
		setAutoCommit(false);

		if (action == COMMIT)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "COMMIT");
			}
			commit();
		}
		else if (action == ROLLBACK)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "ROLLBACK");
			}
			rollback();
		}
	}

	/// <summary>
	/// Sets the dirty flag if autocommit is set to off.  otherwise, does nothing
	/// </summary>
	private void testAndSetDirty()
	{
		if (__autoCommit == false)
		{
			__dirty = true;
		}
	}

	/// <summary>
	/// Returns a string of useful information about the DMI </summary>
	/// <returns> a string of useful information about the DMI </returns>
	public override string ToString()
	{
		string dmiDesc = "";

		if (!__secure)
		{
			dmiDesc = "DMI Information:"
				+ "\n   System Login: " + __system_login + "\n   System Password: " + __system_password + "\n   User Login: " + __user_login + "\n   User Password: " + __user_password + "\n   Connecting to database '" + __database_name + "' of type '" + __database_engine_String + "'"
				+ "\n   at '" + __database_server + ":" + __port + "'"
				+ "\n   Current status is: ";

				if (__connected == true)
				{
					dmiDesc += "CONNECTED";
				}
				else
				{
					dmiDesc += "NOT CONNECTED";
				}

			dmiDesc += "\n   autoCommit is currently: ";

				if (__autoCommit == true)
				{
					dmiDesc += "ON";
				}
				else
				{
					dmiDesc += "OFF";
				}

			dmiDesc += "\n   The previously executed SQL statement was:"
				+ "\n   [" + getLastSQLString() + "]";
		}
		else
		{
			dmiDesc = "DMI Information:"
				+ "\n   Connecting to database '" + __database_name + "' of type '" + __database_engine_String + "'"
				+ "\n   Current status is: ";

				if (__connected == true)
				{
					dmiDesc += "CONNECTED";
				}
				else
				{
					dmiDesc += "NOT CONNECTED";
				}
		}

		return dmiDesc;
	}

	/// <summary>
	/// Sets the DMI to use the default port for the database engine that the DMI
	/// was set up to connect to.  Only works currently for true client-server type databases.
	/// </summary>
	public virtual void setDefaultPort()
	{
		if (_database_engine == DBENGINE_ACCESS)
		{
		}
		else if (_database_engine == DBENGINE_H2)
		{
		}
		else if (_database_engine == DBENGINE_INFORMIX)
		{
			__port = 1526;
		}
		else if (_database_engine == DBENGINE_MYSQL)
		{
			__port = 3306;
		}
		else if (_database_engine == DBENGINE_ORACLE)
		{
			__port = 1521;
		}
		else if (_database_engine == DBENGINE_POSTGRESQL)
		{
			__port = 5432;
		}
		else if (_database_engine == DBENGINE_SQLSERVER)
		{
			__port = 1433;
		}
		else
		{
			//
		}
	}

	}

}