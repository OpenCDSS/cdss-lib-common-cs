using System;
using System.Collections.Generic;

// DMIStatement - base class for statements

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

namespace RTi.DMI
{

	using DateTime = RTi.Util.Time.DateTime;
	using TimeZoneDefaultType = RTi.Util.Time.TimeZoneDefaultType;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// The DMIStatement class stores basic information about SQL statements.  It serves
	/// as the base class for more specific statements.
	/// </summary>
	public class DMIStatement
	{

	/// <summary>
	/// Flags to note what kind of JOIN (if any) the statement is.
	/// </summary>
	protected internal readonly int _JOIN_INNER = 0, _JOIN_LEFT = 1, _JOIN_RIGHT = 2;

	/// <summary>
	/// Whether the statement is executing via a stored procedure.
	/// </summary>
	protected internal bool _isSP = false;

	/// <summary>
	/// The object that will actually execute the stored procedure in the low level java code.
	/// </summary>
	protected internal CallableStatement __storedProcedureCallableStatement = null;

	/// <summary>
	/// DMI that will execute the query.  It is used to determine the database type for formatting statements.
	/// </summary>
	protected internal DMI _dmi;

	/// <summary>
	/// The object that holds information about the stored procedure, if one is
	/// being used.  If this query will not use a stored procedure, this is null.
	/// </summary>
	protected internal DMIStoredProcedureData _spData = null;

	/// <summary>
	/// A counter for stored procedures that tracks the parameter being set via
	/// a setValue() call.  This is necessary in order to be able to use the existing
	/// write.*() methods in DMIs and have them work with both stored procedures and 
	/// the old style dmi statements.  As addValue() statements are called, the
	/// counter is incremented so the stored procedure knows the position into which
	/// the value placed by the next addValue() statement should go.
	/// </summary>
	private int __paramNum;

	/// <summary>
	/// Array of the parameters set in a stored procedure, for use in printing the
	/// stored procedure out in executable format when a query is run.
	/// </summary>
	private string[] __spParameters = null;

	/// <summary>
	/// List for fields that are auto-numbers
	/// </summary>
	protected internal IList<string> _autonumber_Vector;

	/// <summary>
	/// List for fields used in the statement (e.g., SELECT XXXX, XXXX).
	/// </summary>
	protected internal IList<string> _field_Vector;

	/// <summary>
	/// List to specify the tables for joins.
	/// </summary>
	protected internal IList<string> _join_Vector;

	/// <summary>
	/// List to specify the join types for joins.
	/// </summary>
	protected internal IList<int> _join_type_Vector;

	/// <summary>
	/// List for specifying the ON clauses for joins.
	/// </summary>
	protected internal IList<string> _on_Vector;

	/// <summary>
	/// List for ORDER BY clauses used in the statement (e.g., ORDER BY XXXX, XXXX).
	/// </summary>
	protected internal IList<string> _order_by_Vector;

	/// <summary>
	/// List for table names used in the statement (e.g., FROM XXXX, XXXX).
	/// </summary>
	protected internal IList<string> _table_Vector;

	/// <summary>
	/// List for values to be inserted or updated with the statement.  Values may be Java objects
	/// (String, Integer, etc.) or a DMISelectStatement, which will do a select on the value, for
	/// example to select foreign key from the human-readable data value.
	/// </summary>
	protected internal IList<object> _values_Vector;

	/// <summary>
	/// List for where clauses used in the statement (e.g., WHERE XXXX, XXXX).
	/// </summary>
	protected internal IList<string> _where_Vector;

	/// <summary>
	/// Construct an SQL statement.  Typically a derived class instance (e.g., DMISelectStatement) is declared. </summary>
	/// <param name="dmi"> DMI instance to use (this is checked to properly format the statement for the database engine). </param>
	public DMIStatement(DMI dmi)
	{
		_dmi = dmi;
		_isSP = false;
		initialize();
	}

	/// <summary>
	/// Construct an SQL statement that will execute via a Stored Procedure. </summary>
	/// <param name="dmi"> DMI instance to use. </param>
	/// <param name="data"> DMIStoredProcedureData to use for controlling the procedure. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public DMIStatement(DMI dmi, DMIStoredProcedureData data) throws Exception
	public DMIStatement(DMI dmi, DMIStoredProcedureData data)
	{
		_dmi = dmi;
		_spData = data;
		_isSP = true;
		initialize();
		// parameter numbering starts at 1 for JDBC parameters, not 0.
		__paramNum = 1;
		__storedProcedureCallableStatement = _spData.getCallableStatement();

		if (_spData.hasReturnValue())
		{
			__paramNum = 2;
		}

		__spParameters = new string[_spData.getNumParameters()];
		for (int i = 0; i < __spParameters.Length; i++)
		{
			__spParameters[i] = "?";
		}
	}

	/// <summary>
	/// Add a field the statement. </summary>
	/// <param name="field"> Field to add to the statement. </param>
	public virtual void addField(string field)
	{
		_field_Vector.Add(field);
	}

	/// <summary>
	/// Adds the table to be INNER joined to a select query.  An ON clause will need
	/// to be set via the addJoinOn() method.
	/// </summary>
	public virtual void addInnerJoin(string tableName, string on)
	{
		_join_Vector.Add(tableName);
		_join_type_Vector.Add(new int?(_JOIN_INNER));
		_on_Vector.Add(on);
	}

	/// <summary>
	/// Adds the table to be LEFT joined to a select query.  An ON clause will need
	/// to be set via the addJoinOn() method.
	/// </summary>
	public virtual void addLeftJoin(string tableName, string on)
	{
		_join_Vector.Add(tableName);
		_join_type_Vector.Add(new int?(_JOIN_LEFT));
		_on_Vector.Add(on);
	}

	/// <summary>
	/// Adds the table to be RIGHT joined to a select query.  An ON clause will need
	/// to be set via the addJoinOn() method.
	/// </summary>
	public virtual void addRightJoin(string tableName, string on)
	{
		_join_Vector.Add(tableName);
		_join_type_Vector.Add(new int?(_JOIN_RIGHT));
		_on_Vector.Add(on);
	}

	/// <summary>
	/// Adds a null value to the values vector.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void addNullValue() throws Exception
	public virtual void addNullValue()
	{
		if (_isSP)
		{
			setNullValue(__paramNum++);
		}
		else
		{
			// this is for insert/update statements
			_values_Vector.Add(null);
		}
	}

	/// <summary>
	/// Add an ORDER BY clause to the statement. </summary>
	/// <param name="order_by_clause"> ORDER BY clause to add to the statement. </param>
	public virtual void addOrderByClause(string order_by_clause)
	{
		int size = _order_by_Vector.Count;
		string s = null;
		for (int i = 0; i < size; i++)
		{
			s = _order_by_Vector[i];
			if (s.Equals(order_by_clause, StringComparison.OrdinalIgnoreCase))
			{
				// already present in the order by list, do not add again.
				return;
			}
		}
		_order_by_Vector.Add(order_by_clause);
	}

	/// <summary>
	/// Adds a series of order by clauses to the statement at once. </summary>
	/// <param name="order_by_clauses"> list of String order by clauses to add. </param>
	public virtual void addOrderByClauses(IList<string> order_by_clauses)
	{
		for (int i = 0; i < order_by_clauses.Count; i++)
		{
			addOrderByClause(order_by_clauses[i]);
		}
	}

	/// <summary>
	/// Add a table the statement. </summary>
	/// <param name="table"> Table to add to the statement. </param>
	public virtual void addTable(string table)
	{
		_table_Vector.Add(table);
	}

	/// <summary>
	/// Adds a boolean value to the statement. </summary>
	/// <param name="value"> boolean value to add to the statement. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void addValue(boolean value) throws Exception
	public virtual void addValue(bool value)
	{
		if (_isSP)
		{
			setValue(value, __paramNum++);
		}
		else
		{
			_values_Vector.Add(new bool?(value));
		}
	}

	/// <summary>
	/// Adds a Boolean value to the statement. </summary>
	/// <param name="value"> Boolean value to add to the statement. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void addValue(System.Nullable<bool> value) throws Exception
	public virtual void addValue(bool? value)
	{
		addValue(value.Value);
	}

	/// <summary>
	/// Adds a date value to the statement, with a default date format of yyyy-MM-dd </summary>
	/// <param name="value"> date value to add to the statement </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void addValue(java.util.Date value) throws Exception
	public virtual void addValue(System.DateTime value)
	{
		if (_isSP)
		{
			setValue(value, __paramNum++);
		}
		else
		{
			addValue(value, DateTime.PRECISION_DAY);
		}
	}

	/// <summary>
	/// Adds a Date value to the statement. </summary>
	/// <param name="value"> Date value to add to the statement. </param>
	/// <param name="precision"> the DateTime PRECISION_* flag that determines how much of
	/// the Date will be formatted by DMIUtil.formatDateTime(). </param>
	/// <exception cref="Exception"> if there is an error formatting the date time (shouldn't happen). </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void addValue(java.util.Date value, int precision) throws Exception
	public virtual void addValue(System.DateTime value, int precision)
	{
		if (_isSP)
		{
			addValue(value);
			return;
		}
		_values_Vector.Add(DMIUtil.formatDateTime(_dmi, new DateTime(value, precision)));
	}

	/// <summary>
	/// Adds a DateTime value to the statement. </summary>
	/// <param name="value"> DateTime value to add to the statement. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void addValue(RTi.Util.Time.DateTime value) throws Exception
	public virtual void addValue(DateTime value)
	{
		if (_isSP)
		{
			setValue(value, __paramNum++);
		}
		else
		{
			_values_Vector.Add(value);
		}
	}

	/// <summary>
	/// Add a double value to the statement </summary>
	/// <param name="value"> double value to add to the statement </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void addValue(double value) throws Exception
	public virtual void addValue(double value)
	{
		if (_isSP)
		{
			setValue(value, __paramNum++);
		}
		else
		{
			_values_Vector.Add(new double?(value));
		}
	}

	/// <summary>
	/// Adds a Double value to the statement. </summary>
	/// <param name="value"> the Double value to add to the statement. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void addValue(System.Nullable<double> value) throws Exception
	public virtual void addValue(double? value)
	{
		if (value == null)
		{
			addNullValue();
		}
		else
		{
			addValueOrNull(value.Value);
		}
	}

	/// <summary>
	/// Add a float value to the statement </summary>
	/// <param name="value"> float value to add to the statement </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void addValue(float value) throws Exception
	public virtual void addValue(float value)
	{
		if (_isSP)
		{
			setValue(value, __paramNum++);
		}
		else
		{
			_values_Vector.Add(new float?(value));
		}
	}

	/// <summary>
	/// Adds a Float value to the statement. </summary>
	/// <param name="value"> the Float value to add to the statement. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void addValue(System.Nullable<float> value) throws Exception
	public virtual void addValue(float? value)
	{
		addValueOrNull(value.Value);
	}

	/// <summary>
	/// Add an integer value to the statement </summary>
	/// <param name="value"> int value to add to the statement </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void addValue(int value) throws Exception
	public virtual void addValue(int value)
	{
		if (_isSP)
		{
			setValue(value, __paramNum++);
		}
		else
		{
			_values_Vector.Add(new int?(value));
		}
	}

	/// <summary>
	/// Adds an Integer value to the statement. </summary>
	/// <param name="value"> Integer value to add to the statement. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void addValue(System.Nullable<int> value) throws Exception
	public virtual void addValue(int? value)
	{
		if (value == null)
		{
			addNullValue();
		}
		else
		{
			addValueOrNull(value.Value);
		}
	}

	/// <summary>
	/// Add a long value to the statement </summary>
	/// <param name="value"> long value to add to the statement </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void addValue(long value) throws Exception
	public virtual void addValue(long value)
	{
		if (_isSP)
		{
			setValue(value, __paramNum++);
		}
		else
		{
			_values_Vector.Add(new long?(value));
		}
	}

	/// <summary>
	/// Add a long value to the statement </summary>
	/// <param name="value"> long value to add to the statement </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void addValue(System.Nullable<long> value) throws Exception
	public virtual void addValue(long? value)
	{
		addValueOrNull(value.Value);
	}

	/// <summary>
	/// Add a String value to the statement </summary>
	/// <param name="value"> String value to add to the statement </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void addValue(String value) throws Exception
	public virtual void addValue(string value)
	{
		if (_isSP)
		{
			setValue(value, __paramNum++);
		}
		else
		{
			if (value.IndexOf('\'') > -1)
			{
				if ((_dmi.getDatabaseEngineType() == DMI.DBENGINE_SQLSERVER) || (_dmi.getDatabaseEngineType() == DMI.DBENGINE_ACCESS))
				{
					// Handle specifically because the following '' is documented
					_values_Vector.Add("'" + StringUtil.replaceString(value, "'", "''") + "'");
				}
				else
				{
					_values_Vector.Add("'" + StringUtil.replaceString(value, "'", "\\'") + "'");
				}
			}
			else
			{
				_values_Vector.Add("'" + value + "'");
			}
		}
	}

	/// <summary>
	/// Add a DMISelectStatement value to the statement </summary>
	/// <param name="value"> DMISelectStatement value to add to the statement - basically a nested select string that
	/// goes into the statement </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void addValue(DMISelectStatement value) throws Exception
	public virtual void addValue(DMISelectStatement value)
	{
		if (_isSP)
		{
			throw new Exception("Stored procedures do not support nested select in write statement.");
			//setValue(value, __paramNum++);
		}
		else
		{
			if (_dmi.getDatabaseEngineType() == DMI.DBENGINE_SQLSERVER)
			{
				// TODO SAM 2013-02-03 Need to evaluate whether any specific cleanup needs to be done internally
				// as in the following code so that the embedded select statement is properly formatted
				//if (value.indexOf('\'') > -1) {
				//    _values_Vector.add("'" + StringUtil.replaceString(value, "'", "''") + "'");
				//}   
				//else {
				//    _values_Vector.add("'" + value + "'");
				//}
				_values_Vector.Add("(" + value + ")");
			}
			else
			{
				_values_Vector.Add("(" + value + ")");
			}
		}
	}

	/// <summary>
	/// Checks the value with DMIUtil.isMissing() to see if it is missing and if so,
	/// calls addNullValue(); otherwise calls addValue().  This is done to insert
	/// values into fields which can take NULL values. </summary>
	/// <param name="value"> Date value to add to the statement </param>
	/// <param name="precision"> the precision in which to format the date. </param>
	/// <exception cref="Exception"> if there is an error formatting the date time. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void addValueOrNull(java.util.Date value, int precision) throws Exception
	public virtual void addValueOrNull(System.DateTime value, int precision)
	{
		if (DMIUtil.isMissing(value))
		{
			addNullValue();
		}
		else
		{
			addValue(value, precision);
		}
	}

	/// <summary>
	/// Checks the value with DMIUtil.isMissing() to see if it is missing and if so,
	/// calls addNullValue(); otherwise calls addValue().  This is done to insert
	/// values into fields which can take NULL values. </summary>
	/// <param name="value"> double value to add to the statement </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void addValueOrNull(double value) throws Exception
	public virtual void addValueOrNull(double value)
	{
		if (DMIUtil.isMissing(value))
		{
			addNullValue();
		}
		else
		{
			addValue(value);
		}
	}

	/// <summary>
	/// Checks the value with DMIUtil.isMissing() to see if it is missing and if so,
	/// calls addNullValue(); otherwise calls addValue().  This is done to insert
	/// values into fields which can take NULL values. </summary>
	/// <param name="value"> int value to add to the statement </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void addValueOrNull(int value) throws Exception
	public virtual void addValueOrNull(int value)
	{
		if (DMIUtil.isMissing(value))
		{
			addNullValue();
		}
		else
		{
			addValue(value);
		}
	}

	/// <summary>
	/// Checks the value with DMIUtil.isMissing() to see if it is missing and if so,
	/// calls addNullValue(); otherwise calls addValue().  This is done to insert
	/// values into fields which can take NULL values. </summary>
	/// <param name="value"> long value to add to the statement </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void addValueOrNull(long value) throws Exception
	public virtual void addValueOrNull(long value)
	{
		if (DMIUtil.isMissing(value))
		{
			addNullValue();
		}
		else
		{
			addValue(value);
		}
	}

	/// <summary>
	/// Checks the value with DMIUtil.isMissing() to see if it is missing and if so,
	/// calls addNullValue(); otherwise calls addValue().  This is done to insert
	/// values into fields which can take NULL values. </summary>
	/// <param name="value"> String value to add to the statement </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void addValueOrNull(String value) throws Exception
	public virtual void addValueOrNull(string value)
	{
		if (DMIUtil.isMissing(value))
		{
			addNullValue();
		}
		else
		{
			addValue(value);
		}
	}

	/// <summary>
	/// Add a WHERE clause to the statement. </summary>
	/// <param name="where_clause"> WHERE clause to add to the statement. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void addWhereClause(String where_clause) throws Exception
	public virtual void addWhereClause(string where_clause)
	{
		if (_isSP)
		{
			setValueFromWhereClause(where_clause);
		}
		else
		{
			_where_Vector.Add(where_clause);
		}
	}

	/// <summary>
	/// Adds a list of WHERE clauses to the statement. </summary>
	/// <param name="whereClauses"> list of String WHERE clauses to add. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void addWhereClauses(java.util.List<String> whereClauses) throws Exception
	public virtual void addWhereClauses(IList<string> whereClauses)
	{
		foreach (string whereClause in whereClauses)
		{
			if (!whereClause.Equals(""))
			{
				addWhereClause(whereClause);
			}
		}
	}

	/// <summary>
	/// Creates the string that can be pasted into a Query Analyzer to run the
	/// stored procedure exactly as it is executed internally by the DMI.  The result
	/// from this call is different from the createStoredProcedureCallString() call. </summary>
	/// <returns> the "exec ...." String. </returns>
	public virtual string createStoredProcedureString()
	{
		string callString = "";

		if (_spData != null)
		{
			// There may be a case where stored procedures are being used for the database but the current
			// statement is doing a direct query (such as on a published view).
			callString += "exec " + _spData.getProcedureName() + " ";

			int numParameters = _spData.getNumParameters();

			for (int i = 0; i < numParameters; i++)
			{
				if (i > 0)
				{
					callString += ", ";
				}
				callString += __spParameters[i];
			}
		}
		else
		{
			// TODO SAM 2012-09-07 Evaluate how to better handle case when stored procedure is not used
		}

		return callString;
	}

	/// <summary>
	/// Clean up for garbage collection. </summary>
	/// <exception cref="Throwable"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~DMIStatement()
	{
		_dmi = null;
		_spData = null;
		_autonumber_Vector = null;
		_field_Vector = null;
		_join_Vector = null;
		_join_type_Vector = null;
		_on_Vector = null;
		_order_by_Vector = null;
		_table_Vector = null;
		_values_Vector = null;
		_where_Vector = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the callable statement executed by the stored procedure.  This is done
	/// usually in order to close the statement in the two-parameter version of 
	/// DMI.closeResultSet(). </summary>
	/// <returns> the callable statement executed by the stored procedure. </returns>
	public virtual java.sql.Statement getCallableStatement()
	{
		return __storedProcedureCallableStatement;
	}

	/// <summary>
	/// Returns the DMI's stored procedure data (that specifies how the stored procedure is set up and run. </summary>
	/// <returns> the DMI's stored procedure data (that specifies how the stored procedure is set up and run. </returns>
	public virtual DMIStoredProcedureData getDMIStoredProcedureData()
	{
		return _spData;
	}

	/// <summary>
	/// Gets the return value from a stored procedure that returns a value as an int. </summary>
	/// <returns> the return value. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int getIntReturnValue() throws java.sql.SQLException
	public virtual int getIntReturnValue()
	{
		return __storedProcedureCallableStatement.getInt(1);
	}

	/// <summary>
	/// Gets the return value from a stored procedure that returns a value as a String. </summary>
	/// <returns> the return value. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String getStringReturnValue() throws java.sql.SQLException
	public virtual string getStringReturnValue()
	{
		return __storedProcedureCallableStatement.getString(1);
	}

	/// <summary>
	/// Initializes data members.
	/// </summary>
	private void initialize()
	{
		_autonumber_Vector = new List<string>();
		_field_Vector = new List<string>();
		_join_Vector = new List<string>();
		_join_type_Vector = new List<int>();
		_on_Vector = new List<string>();
		_order_by_Vector = new List<string>();
		_table_Vector = new List<string>();
		_values_Vector = new List<object>();
		_where_Vector = new List<string>();
	}

	/// <summary>
	/// Returns whether this statement is using a stored procedure or not. </summary>
	/// <returns> true if the statement is going to execute via stored procedure, false if not. </returns>
	public virtual bool isStoredProcedure()
	{
		return _isSP;
	}

	/// <summary>
	/// Removes a field from the statement. </summary>
	/// <param name="field"> the field to remove from the statement. </param>
	public virtual void removeField(string field)
	{
		_field_Vector.Remove(field);
	}

	/// <summary>
	/// Sets the stored procedure data to use.  If this data is set to a non-null value
	/// then the statement will execute with the stored procedure.  Otherwise, the
	/// statement will try to create a SQL string. </summary>
	/// <param name="data"> the stored procedure data to use. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setStoredProcedureData(DMIStoredProcedureData data) throws Exception
	public virtual void setStoredProcedureData(DMIStoredProcedureData data)
	{
		_spData = data;
		if (data == null)
		{
			_isSP = false;
			__storedProcedureCallableStatement = null;
		}
		else
		{
			_isSP = true;
			__storedProcedureCallableStatement = _spData.getCallableStatement();

			// Note re: __paramNum:
			// Parameter numbering in JDBC/Database interface classes
			// begins at 1.  When a stored procedure should expect a 
			// return statement, the return value is automatically placed
			// in position 1, and the passed-in parameters begin at 2.

			if (_spData.hasReturnValue())
			{
				__paramNum = 2;
			}
			else
			{
				__paramNum = 1;
			}

			__spParameters = new string[_spData.getNumParameters()];
			for (int i = 0; i < __spParameters.Length; i++)
			{
				__spParameters[i] = "?";
			}
		}
	}

	/// <summary>
	/// Sets a value in the specified parameter position. </summary>
	/// <param name="parameterNum"> the number of the parameter position (1+) to set. </param>
	/// <exception cref="Exception"> if the specified parameter is not a String type. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void setNullValue(int parameterNum) throws Exception
	private void setNullValue(int parameterNum)
	{
		__storedProcedureCallableStatement.setNull(parameterNum, _spData.getParameterType(parameterNum));
		if (_spData.hasReturnValue())
		{
			__spParameters[parameterNum - 2] = "NULL";
		}
		else
		{
			__spParameters[parameterNum - 1] = "NULL";
		}
	}

	/// <summary>
	/// Sets a value in the specified parameter position. </summary>
	/// <param name="param"> the parameter to pass in. </param>
	/// <param name="parameterNum"> the number of the parameter position (1+) to set. </param>
	/// <exception cref="Exception"> if the specified parameter is not a String type. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setValue(boolean param, int parameterNum) throws Exception
	public virtual void setValue(bool param, int parameterNum)
	{
		__storedProcedureCallableStatement.setBoolean(parameterNum, param);
		if (_spData.hasReturnValue())
		{
			__spParameters[parameterNum - 2] = "" + param;
		}
		else
		{
			__spParameters[parameterNum - 1] = "" + param;
		}
	}

	/// <summary>
	/// Sets a value in the specified parameter position. </summary>
	/// <param name="param"> the parameter to pass in. </param>
	/// <param name="parameterNum"> the number of the parameter position (1+) to set. </param>
	/// <exception cref="Exception"> if the specified parameter is not a String type. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setValue(java.util.Date param, int parameterNum) throws Exception
	public virtual void setValue(System.DateTime param, int parameterNum)
	{
		if (param == null)
		{
			setNullValue(parameterNum);
		}
		// Note:
		// the Date object is cast because the Date passed into this 
		// method is a java.util.Date, whereas the callable statement 
		// expects a javal.sql.Date, which is a subclass of java.util.Date.
		__storedProcedureCallableStatement.setDate(parameterNum, (new java.sql.Date(param.Ticks)));
		if (_spData.hasReturnValue())
		{
			__spParameters[parameterNum - 2] = "'" + param + "'";
		}
		else
		{
			__spParameters[parameterNum - 1] = "'" + param + "'";
		}
	}

	/// <summary>
	/// Sets a value in the specified parameter position. </summary>
	/// <param name="param"> the parameter to pass in. </param>
	/// <param name="parameterNum"> the number of the parameter position (1+) to set. </param>
	/// <exception cref="Exception"> if the specified parameter is not a String type. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setValue(RTi.Util.Time.DateTime param, int parameterNum) throws Exception
	public virtual void setValue(DateTime param, int parameterNum)
	{
		if (param == null)
		{
			setNullValue(parameterNum);
		}
		// Note:
		// the Date object is cast because the Date returned from getDate()
		// is a java.util.Date, whereas the callable statement expects a 
		// javal.sql.Date, which is a subclass of java.util.Date.
		// TODO SAM 2016-03-11 the following matches legacy behavior but handling of time zone may need additional evaluation
		__storedProcedureCallableStatement.setDate(parameterNum,(new java.sql.Date(param.getDate(TimeZoneDefaultType.LOCAL).Ticks)));
		if (_spData.hasReturnValue())
		{
			__spParameters[parameterNum - 2] = "'" + param + "'";
		}
		else
		{
			__spParameters[parameterNum - 1] = "'" + param + "'";
		}
	}

	/// <summary>
	/// Sets a value in the specified parameter position. </summary>
	/// <param name="param"> the parameter to pass in. </param>
	/// <param name="parameterNum"> the number of the parameter position (1+) to set. </param>
	/// <exception cref="Exception"> if the specified parameter is not a String type. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setValue(double param, int parameterNum) throws Exception
	public virtual void setValue(double param, int parameterNum)
	{
		__storedProcedureCallableStatement.setDouble(parameterNum, param);
		if (_spData.hasReturnValue())
		{
			__spParameters[parameterNum - 2] = "" + param;
		}
		else
		{
			__spParameters[parameterNum - 1] = "" + param;
		}
	}

	/// <summary>
	/// Sets a value in the specified parameter position. </summary>
	/// <param name="param"> the parameter to pass in. </param>
	/// <param name="parameterNum"> the number of the parameter position to (1+) set. </param>
	/// <exception cref="Exception"> if the specified parameter is not a String type. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setValue(float param, int parameterNum) throws Exception
	public virtual void setValue(float param, int parameterNum)
	{
		__storedProcedureCallableStatement.setFloat(parameterNum, param);
		if (_spData.hasReturnValue())
		{
			__spParameters[parameterNum - 2] = "" + param;
		}
		else
		{
			__spParameters[parameterNum - 1] = "" + param;
		}
	}

	/// <summary>
	/// Sets a value in the specified parameter position. </summary>
	/// <param name="param"> the parameter to pass in. </param>
	/// <param name="parameterNum"> the number of the parameter position (1+) to set. </param>
	/// <exception cref="Exception"> if the specified parameter is not a String type. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setValue(int param, int parameterNum) throws Exception
	public virtual void setValue(int param, int parameterNum)
	{
		__storedProcedureCallableStatement.setInt(parameterNum, param);
		if (_spData.hasReturnValue())
		{
			__spParameters[parameterNum - 2] = "" + param;
		}
		else
		{
			__spParameters[parameterNum - 1] = "" + param;
		}
	}

	/// <summary>
	/// Sets a value in the specified parameter position. </summary>
	/// <param name="param"> the parameter to pass in. </param>
	/// <param name="parameterNum"> the number of the parameter position (1+) to set. </param>
	/// <exception cref="Exception"> if the specified parameter is not a String type. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setValue(long param, int parameterNum) throws Exception
	public virtual void setValue(long param, int parameterNum)
	{
		__storedProcedureCallableStatement.setLong(parameterNum, param);
		if (_spData.hasReturnValue())
		{
			__spParameters[parameterNum - 2] = "" + param;
		}
		else
		{
			__spParameters[parameterNum - 1] = "" + param;
		}
	}

	/// <summary>
	/// Sets a value in the specified parameter position for stored procedure. </summary>
	/// <param name="param"> the parameter to pass in. </param>
	/// <param name="parameterNum"> the number of the parameter position (1+) to set. </param>
	/// <exception cref="Exception"> if the specified parameter is not a String type. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setValue(String param, int parameterNum) throws Exception
	public virtual void setValue(string param, int parameterNum)
	{
		if (string.ReferenceEquals(param, null))
		{
			setNullValue(parameterNum);
		}
		__storedProcedureCallableStatement.setString(parameterNum, param);
		if (_spData.hasReturnValue())
		{
			__spParameters[parameterNum - 2] = "'" + param + "'";
		}
		else
		{
			__spParameters[parameterNum - 1] = "'" + param + "'";
		}
	}

	/// <summary>
	/// Sets a value in the specified parameter position. </summary>
	/// <param name="param"> the parameter to pass in. </param>
	/// <param name="parameterNum"> the number of the parameter position (1+) to set. </param>
	/// <exception cref="Exception"> if the specified parameter is not a String type. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setValue(java.sql.Timestamp param, int parameterNum) throws Exception
	public virtual void setValue(Timestamp param, int parameterNum)
	{
		if (param == null)
		{
			setNullValue(parameterNum);
		}
		__storedProcedureCallableStatement.setTimestamp(parameterNum, (new Timestamp(param.getTime())));
		if (_spData.hasReturnValue())
		{
			__spParameters[parameterNum - 2] = "'" + param + "'";
		}
		else
		{
			__spParameters[parameterNum - 1] = "'" + param + "'";
		}
	}

	/// <summary>
	/// Sets a value in a stored procedure based on the column name in the 
	/// where clause.  The column name is split out from the where clause and
	/// then the DMIStoredProcedureData object is consulted to determine which
	/// parameter number the where clause maps to.  The value is also split out
	/// from the where clause and put in the appropriate stored procedure parameter
	/// position.  Where clauses are broken into column name and value by either an 
	/// equal sign ('='), " LIKE " or " IS NULL". </summary>
	/// <param name="where"> the where clause. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void setValueFromWhereClause(String where) throws Exception
	private void setValueFromWhereClause(string where)
	{
		bool isNull = false;
		string upper = where.ToUpper().Trim();
		where = where.Trim();

		// Find the position of the first equals sign. 

		int space = where.IndexOf("=", StringComparison.Ordinal);
		int spaceLen = 1;
		if (space == -1)
		{
			// If none can be found, then check to find the first instance of " LIKE "
			space = upper.IndexOf(" LIKE ", StringComparison.Ordinal);
			if (space == -1)
			{
				// If none can be found, check to see if the where clause is comparing against NULL.
				space = upper.IndexOf(" IS NULL", StringComparison.Ordinal);
				if (space == -1)
				{
					// If none of the above worked, then the column = value combination could not be determined. 
					throw new Exception("Cannot determine columns or value from where clause: '" + where + "'");
				}
				// " IS NULL" was found, so denote in the boolean that a NULL value will need to be set
				isNull = true;
			}
			// The length of the token separating column name and value 
			// is 1 character if it's just an equal sign, but because 
			// " LIKE " was found, the parsing will need to take into
			// account the larger width of the separator.
			spaceLen = 5;
		}

		string origParam = where.Substring(0, space).Trim();
		// See if the column was specified in the where clause as a 
		// table_name.column_name combination.  Remove the table name, if present.
		int index = origParam.IndexOf(".", StringComparison.Ordinal);
		if (index > -1)
		{
			origParam = origParam.Substring(index + 1);
		}

		// For SQL Server 2000, at least, all the parameter names in 
		// the stored procedure are preceded by an '@' sign.
		string param = "@" + origParam;

		// Iterate through the parameter names stored in the stored procedure
		// data and see if a match can be found for the column name.
		int num = _spData.getNumParameters();
		int pos = -1;
		string paramName = null;
		string paramList = "";
		for (int i = 0; i < num; i++)
		{
			if (i > 0)
			{
				paramList += ", ";
			}
			paramName = _spData.getParameterName(i);
			paramList += paramName;
			if (param.Equals(paramName, StringComparison.OrdinalIgnoreCase))
			{
				pos = i;
				break;
			}
		}

		if (pos == -1)
		{
			// No match was found in the stored procedure parameter names for the column
			throw new Exception("Couldn't find parameter '" + origParam + "', specified in where clause: '" + where + "'.\nKnown parameters are: '" + paramList + "'");
		}

		// Note that right now, pos is base-0, which is how the stored 
		// procedure data stored parameter information.

		// get out the type for the located parameter.
		int type = _spData.getParameterType(pos);
		string value = "";

		if (!isNull)
		{
			// determine the value that the column is being compared to
			where = where.Substring(space + spaceLen).Trim();
			value = where.Trim();
		}

		int len = value.Length;

		// pos is currently base-0, but for use in specifying the parameter
		// in the JDBC code, it needs to be incremented to be base-1.
		pos++;
		if (_spData.hasReturnValue())
		{
			// furthermore, if there is a return value for the stored 
			// procedure, the passed-in parameters are actually base-2.
			pos++;
		}

		if (isNull)
		{
			setNullValue(pos);
			return;
		}

		switch (type)
		{
			case java.sql.Types.VARCHAR:
				value = value.Substring(1, (len - 1) - 1);
				setValue(value, pos);
				break;
			case java.sql.Types.BIT:
				// Boolean
				// TODO (JTS - 2004-06-15) Bits might be stored in the database as 0 or 1,
				// not sure right now as we're not dealing with boolean parameters now. 
				break;
			case java.sql.Types.SMALLINT:
			case java.sql.Types.INTEGER:
				int? I = Convert.ToInt32(value);
				setValue(I.Value, pos);
				break;
			case java.sql.Types.BIGINT:
				long? L = Convert.ToInt64(value);
				setValue(L.Value, pos);
				break;
			case java.sql.Types.REAL:
				float? F = Convert.ToSingle(value);
				setValue(F.Value, pos);
				break;
			case java.sql.Types.DOUBLE:
			case java.sql.Types.FLOAT:
				double? D = Convert.ToDouble(value);
				setValue(D.Value, pos);
				break;
			case java.sql.Types.DATE:
				value = value.Trim();
				if (value.StartsWith("'", StringComparison.Ordinal))
				{
					value = value.Substring(1, (len - 1) - 1);
				}
				DateTime DT = DateTime.parse(value);
				// TODO SAM 2016-03-11 the following matches legacy behavior but handling of time zone may need additional evaluation
				setValue(new java.sql.Date(DT.getDate(TimeZoneDefaultType.LOCAL).Ticks), pos);
				break;
			case java.sql.Types.TIMESTAMP:
				value = value.Trim();
				if (value.StartsWith("'", StringComparison.Ordinal))
				{
					value = value.Substring(1, (len - 1) - 1);
				}
				DateTime DT2 = DateTime.parse(value);
				setValue(new Timestamp(DT2.getDate(TimeZoneDefaultType.LOCAL).Ticks), pos);
				break;
			default:
				throw new Exception("Unsupported type: " + type);
		}
	}

	}

}