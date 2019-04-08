using System.Collections.Generic;

// DMIStoredProcedureData - class for storing stored procedure

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
// DMIStoredProcedureData - Class for storing information on the particulars
//	of a stored procedure.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2004-06-10	J. Thomas Sapienza, RTi	Initial version.
// 2005-06-02	JTS, RTi		Moved the stored procedure callable
//					statement setup from DMIStatement into
//					here so that stored procedure 
//					connections can be cached.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.DMI
{

	/// <summary>
	/// A class that stores information on how in particular a stored procedure will
	/// execute.  The following is an example of how to create and use this object, from HydroBaseDMI:<para>
	/// <code>
	/// // Look up the definition of the stored procedure (stored in a
	/// // DMIStoredProcedureData object) in the hashtable.  This allows
	/// // repeated calls to the same stored procedure to re-used stored
	/// // procedure meta data without requerying the database.
	/// 
	/// DMIStoredProcedureData data = (DMIStoredProcedureData)__storedProcedureHashtable.get(name);
	/// 
	/// if (data != null) {
	///		// If a data object was found, set up the data in the statement below and return true
	/// }
	/// else if (data == null && DMIUtil.databaseHasStoredProcedure(this, name)) {
	///		// If no data object was found, but the stored procedure is
	///		// defined in the database then build the data object for the
	///		// stored procedure and then store it in the hashtable.
	///		data = new DMIStoredProcedureData(this, name);
	///		__storedProcedureHashtable.put(name, data);
	/// }
	/// else {
	///		// If no data object was found and the stored procedure is not
	///		// defined in the database, then use the original DMI code.
	///		// Return false to buildSQL() so it knows to continue executing.
	///		if (IOUtil.testing()) {
	///			Message.printStatus(2, "HydroBaseDMI.canSetUpStoredProcedure",
	///				"No stored procedure defined in database for SQL#: " + sqlNumber);
	///		}
	/// 
	///		if (Message.isDebugOn) {
	///			Message.printDebug(30, "HydroBaseDMI.canSetUpStoredProcedure",
	///				"No stored procedure defined in database for SQL#: " + sqlNumber);
	///		}		
	///		return false;
	/// }
	/// 
	/// if (Message.isDebugOn) {
	///		Message.printDebug(30, "HydroBaseDMI.canSetUpStoredProcedure",
	///			"Stored procedure '" + name + "' found and will be used.");
	/// }	
	/// 
	/// if (IOUtil.testing() && debug) {
	///		DMIUtil.dumpProcedureInfo(this, name);
	/// }
	/// 
	/// // Set the data object in the statement.  Doing so will set up the
	/// // statement as a stored procedure statement.
	/// 
	/// statement.setStoredProcedureData(data);
	/// </code>
	/// </para>
	/// </summary>
	public class DMIStoredProcedureData
	{

	/// <summary>
	/// Whether the parameter can be null or not.
	/// </summary>
	private bool[] __parameterNullable = null;

	/// <summary>
	/// Whether there is a return type.
	/// </summary>
	private bool __hasReturnValue = false;

	/// <summary>
	/// The object that will actually execute the stored procedure in the low level java code.
	/// </summary>
	private CallableStatement __callableStatement = null;

	/// <summary>
	/// The dmi used that this stored procedure will connect to the DB with.
	/// </summary>
	private DMI __dmi = null;

	/// <summary>
	/// The types of all the parameters in int type.
	/// </summary>
	private int[] __parameterTypes = null;

	/// <summary>
	/// The number of parameters.
	/// </summary>
	private int __numParameters = 0;

	/// <summary>
	/// The type of the return value.
	/// </summary>
	private int __returnType = -1;

	/// <summary>
	/// The names of all the parameters.
	/// </summary>
	private string[] __parameterNames = null;

	/// <summary>
	/// The types of all the parameters in readable String format.
	/// </summary>
	private string[] __parameterTypeStrings = null;

	/// <summary>
	/// The name of the stored procedure.
	/// </summary>
	private string __procedureName = null;

	/// <summary>
	/// The name of the return parameter.
	/// </summary>
	private string __returnName = null;

	/// <summary>
	/// The type of the return value.
	/// </summary>
	private string __returnTypeString = null;

	/// <summary>
	/// Constructor.  Private so it cannot be called.
	/// </summary>
	// FIXME SAM 2008-04-15 Evaluate if this is by design or not needed.
	//private DMIStoredProcedureData() {}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dmi"> the dmi to use for getting stored procedure information. </param>
	/// <param name="procedureName"> the name of the procedure for which to query the 
	/// database for information. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public DMIStoredProcedureData(DMI dmi, String procedureName) throws Exception
	public DMIStoredProcedureData(DMI dmi, string procedureName)
	{
		__procedureName = procedureName;
		ResultSet rs = dmi.getConnection().getMetaData().getProcedureColumns(dmi.getDatabaseName(), null, procedureName, null);
		__dmi = dmi;
		IList<IList<object>> v = DMIUtil.processResultSet(rs);
		//DMIUtil.printResults(v);
		rs.close();

		fillProcedureData(v);

		buildCallableStatement();
	}

	/// <summary>
	/// Creates the callable statement that will be executed whenever this stored procedure is run.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void buildCallableStatement() throws Exception
	private void buildCallableStatement()
	{
		__callableStatement = __dmi.getConnection().prepareCall(createStoredProcedureCallString());

		if (hasReturnValue())
		{
			__callableStatement.registerOutParameter(1, getReturnType());
		}
	}

	/// <summary>
	/// Creates the string that will be passed to the DMI's connection in order to build the stored procedure. </summary>
	/// <returns> the "{call .... }" string. </returns>
	private string createStoredProcedureCallString()
	{
		string callString = "{";

		if (hasReturnValue())
		{
			callString += "? = ";
		}

		callString += "call " + getProcedureName() + " (";

		int numParameters = getNumParameters();

		for (int i = 0; i < numParameters; i++)
		{
			if (i > 0)
			{
				callString += ", ";
			}
			callString += "?";
		}

		callString += ") }";

		return callString;
	}

	/// <summary>
	/// Pulls out information from the list of procedure data and populates the member variables.
	/// </summary>
	private void fillProcedureData(IList<IList<object>> v)
	{
		__hasReturnValue = hasReturnValue(v);

		int size = v.Count;
		__numParameters = size;
		if (__hasReturnValue)
		{
			__numParameters--;
		}

		__parameterNullable = new bool[__numParameters];
		__parameterTypes = new int[__numParameters];
		__parameterNames = new string[__numParameters];
		__parameterTypeStrings = new string[__numParameters];

		int? I = null;
		int count = 0;
		IList<object> row = null;
		for (int i = 0; i < size; i++)
		{
			row = v[i];

			I = (int?)row[4];
			if (I.Value == DatabaseMetaData.procedureColumnReturn)
			{
				// this is a return value row
				__returnName = (string)row[3];
				__returnType = ((int?)row[5]).Value;
				__returnTypeString = (string)row[6];
			}
			else
			{
				__parameterNames[count] = (string)row[3];
				__parameterTypes[count] = ((int?)row[5]).Value;
				__parameterTypeStrings[count] = (string)row[6];
				if (((int?)row[11]).Value == DatabaseMetaData.procedureNullable)
				{
					__parameterNullable[count] = true;
				}
				else
				{
					__parameterNullable[count] = false;
				}
				count++;
			}
		}
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~DMIStoredProcedureData()
	{
		__parameterNames = null;
		__parameterNullable = null;
		__parameterTypes = null;
		__parameterTypeStrings = null;
		__procedureName = null;
		__returnName = null;
		__returnTypeString = null;
		__dmi = null;
		__callableStatement = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the callable statement that was created to run this stored procedure. </summary>
	/// <returns> the callable statement that was created to run this stored procedure. </returns>
	public virtual CallableStatement getCallableStatement()
	{
		return __callableStatement;
	}

	/// <summary>
	/// Returns the number of parameters in the procedure. </summary>
	/// <returns> the number of parameters in the procedure. </returns>
	public virtual int getNumParameters()
	{
		return __numParameters;
	}

	/// <summary>
	/// Returns the name of the parameter. </summary>
	/// <returns> the name of the parameter. </returns>
	public virtual string getParameterName(int parameterNum)
	{
		return __parameterNames[parameterNum];
	}

	/// <summary>
	/// Returns the integer type (comparable to java.sql.Types) of the parameter. </summary>
	/// <returns> the integer type of the parameter. </returns>
	public virtual int getParameterType(int parameterNum)
	{
		return __parameterTypes[parameterNum];
	}

	/// <summary>
	/// Returns the type string of the parameter. </summary>
	/// <returns> the type string of the parameter. </returns>
	public virtual string getParameterTypeString(int parameterNum)
	{
		return __parameterTypeStrings[parameterNum];
	}

	/// <summary>
	/// Returns the procedure name. </summary>
	/// <returns> the procedure name. </returns>
	public virtual string getProcedureName()
	{
		return __procedureName;
	}

	/// <summary>
	/// Returns the name of the return value, or null if there is no return value. </summary>
	/// <returns> the name of the return value, or null if there is no return value. </returns>
	public virtual string getReturnName()
	{
		return __returnName;
	}

	/// <summary>
	/// Returns the type of the return value, or -1 if there is no return value. </summary>
	/// <returns> the type of the return value, or -1 if there is no return value. </returns>
	public virtual int getReturnType()
	{
		return __returnType;
	}

	/// <summary>
	/// Returns the type string of the return value or null if there is no return value. </summary>
	/// <returns> the type string of the return value or null if there is no return value. </returns>
	public virtual string getReturnTypeString()
	{
		return __returnTypeString;
	}

	/// <summary>
	/// Returns whether the parameter has a return value. </summary>
	/// <returns> whether the parameter has a return value. </returns>
	public virtual bool hasReturnValue()
	{
		return __hasReturnValue;
	}

	/// <summary>
	/// Checks procedure data to see if there are any return values. </summary>
	/// <returns> true if any return values are present, false otherwise. </returns>
	private bool hasReturnValue(IList<IList<object>> v)
	{
		int size = v.Count;

		int? I = null;
		IList<object> row = null;
		for (int i = 0; i < size; i++)
		{
			row = v[i];

			I = (int?)row[4];
			if (I.Value == DatabaseMetaData.procedureColumnReturn)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Returns whether a parameter can be null or not. </summary>
	/// <returns> whether a parameter can be null or not. </returns>
	public virtual bool isParameterNullable(int parameterNum)
	{
		return __parameterNullable[parameterNum];
	}

	/// <summary>
	/// Returns a String description of the stored procedure. </summary>
	/// <returns> a String description of the stored procedure. </returns>
	public override string ToString()
	{
		string desc = "Stored Procedure:   '" + __procedureName + "'\n";
		desc += "  Has return value?  " + __hasReturnValue + "\n";
		if (__hasReturnValue)
		{
			desc += "  Return parameter: '" + __returnName + "'\n";
			desc += "  Return type:      '" + __returnTypeString + "'\n";
		}
		desc += "  Num parameters:    " + __numParameters + "\n";
		for (int i = 0; i < __numParameters; i++)
		{
			desc += "  [" + i + "] Name:         '" + __parameterNames[i] + "'\n";
			desc += "      Type:          " + __parameterTypeStrings[i] + " (" + __parameterTypes[i] + ")\n";
			desc += "      Nullable?      " + __parameterNullable[i] + "\n";
		}
		return desc;
	}

	}

}