using System;
using System.Text;

// DMIWriteStatement - write statement

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

	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// The DMIWriteStatement class stores basic information about SQL write statements, allowing write statements
	/// to be constructed for execution.
	/// See HydroBaseDMI for many good examples of using this class.
	/// </summary>
	public class DMIWriteStatement : DMIStatement
	{

	/// <summary>
	/// Construct a select statement.
	/// </summary>
	public DMIWriteStatement(DMI dmi) : base(dmi)
	{
	}

	/// <summary>
	/// Executes this statement's stored procedure.  If this statement was not set
	/// up as a Stored Procedure, an exception will be thrown.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void executeStoredProcedure() throws java.sql.SQLException
	public virtual void executeStoredProcedure()
	{
		if (!isStoredProcedure())
		{
			throw new SQLException("Cannot use executeStoredProcedure() to " + "execute a DMIWriteStatement that is not a stored procedure.");
		}
		__storedProcedureCallableStatement.executeUpdate();
	}

	/// <summary>
	/// Removes the name of the table from fields stored in the SQL, if the fields have the table name. </summary>
	/// <param name="fieldName"> the field name to check for a table name and remove, if present. </param>
	/// <returns> the field name without the table name. </returns>
	private string removeTableName(string fieldName)
	{
		int loc = fieldName.IndexOf(".", StringComparison.Ordinal);
		return (fieldName.Substring(loc + 1));
	}

	/// <summary>
	/// Format the INSERT statement. </summary>
	/// <returns> the INSERT statement as a string. </returns>
	public virtual string toInsertString()
	{
		StringBuilder statement = new StringBuilder("INSERT INTO ");

		int size = _table_Vector.Count;
		if (size > 0)
		{
			statement.Append(_table_Vector[0]);
			statement.Append(" (");
		}
		else
		{
			Message.printWarning(2, "DMIWriteStatement.toInsertString", "No table specified to use in creation of SQL");
			return "";
		}

		size = _field_Vector.Count;
		if (size > 0)
		{
			statement.Append(DMIUtil.escapeField(_dmi,removeTableName(_field_Vector[0])));

			for (int i = 1; i < size; i++)
			{
				statement.Append(", " + DMIUtil.escapeField(_dmi,removeTableName(_field_Vector[i])));
			}
		}
		else
		{
			Message.printWarning(2, "DMIWriteStatement.toInsertString", "No fields specified to use in creation of SQL");
			return "";
		}

		statement.Append(") VALUES (");
		size = _values_Vector.Count;
		if (size > 0)
		{
			statement.Append(_values_Vector[0]);

			for (int i = 1; i < size; i++)
			{
				statement.Append(", " + _values_Vector[i]);
			}
		}
		else
		{
			Message.printWarning(2, "DMIWriteStatement.toInsertString", "No values specified to use in creation of SQL");
			return "";
		}

		statement.Append(")");

		return statement.ToString();
	}

	/// <summary>
	/// Format the WRITE statement. </summary>
	/// <returns> the WRITE statement as a string. </returns>
	public virtual string toUpdateString()
	{
		return toUpdateString(false);
	}

	/// <summary>
	/// Format the WRITE statement. </summary>
	/// <param name="tryBuildWhere"> if set to true and the write statement was never set a 
	/// where clause, this will try to build a where clause from the fields and values
	/// in the rest of the write statement.  In other words, it will create a write 
	/// statement that sets a row in the database to the row's same values.  This is
	/// used when doing UPDATE_INSERTs in the DMI. </param>
	/// <returns> the WRITE statement as a string. </returns>
	public virtual string toUpdateString(bool tryBuildWhere)
	{
		StringBuilder statement = new StringBuilder("UPDATE ");

		int size = _table_Vector.Count;
		if (size > 0)
		{
			statement.Append(_table_Vector[0]);
			statement.Append(" ");
		}
		else
		{
			Message.printWarning(2, "DMIWriteStatement.toUpdateString", "No table specified to use in creation of SQL");
			return "";
		}

		statement.Append("SET ");

		size = _field_Vector.Count;
		int size2 = _values_Vector.Count;

		if (size != size2)
		{
			Message.printWarning(2, "DMIWriteStatement.toUpdateString", "Can't build SQL with " + size + " column names " + "and " + size2 + " values to put in those columns.");
			return "";
		}
		else if (size > 0)
		{
			statement.Append(DMIUtil.escapeField(_dmi,_field_Vector[0]));
			statement.Append(" = ");
			statement.Append(_values_Vector[0]);

			for (int i = 1; i < size; i++)
			{
				statement.Append(", ");
				statement.Append(DMIUtil.escapeField(_dmi,_field_Vector[i]));
				statement.Append(" = ");
				statement.Append(_values_Vector[i]);
			}
		}

		statement.Append(" WHERE ");

		size = _where_Vector.Count;
		if (size > 0)
		{
			statement.Append(_where_Vector[0]);

			for (int i = 1; i < size; i++)
			{
				statement.Append(" AND ");
				statement.Append(_where_Vector[i]);
			}
		}
		else if ((_field_Vector.Count > 0) && tryBuildWhere)
		{
			statement.Append(DMIUtil.escapeField(_dmi,_field_Vector[0]));
			statement.Append(" = ");
			statement.Append(_values_Vector[0]);

			size = _field_Vector.Count;
			for (int i = 1; i < size; i++)
			{
				statement.Append(" AND ");
				statement.Append(DMIUtil.escapeField(_dmi, _field_Vector[i]));
				statement.Append(" = ");
				statement.Append(_values_Vector[i]);
			}
		}
		else
		{
			Message.printWarning(2, "DMIWriteStatement.toUpdateString", "No where clause specified for update SQL");
			return "";
		}

		return statement.ToString();
	}

	public override string ToString()
	{
		return "Insert string version: \n" + toInsertString() + "\nUpdate string version: \n" + toUpdateString(true);
	/*		
		String s= "Insert string version: \n" + toInsertString() + "\n\nUpdate "
			+ "string version: \n" + toUpdateString(true) + "\n";
		for (int i = 0; i < _table_Vector.size(); i++) {
			s += "Table (" +i+ "): '" + _table_Vector.elementAt(i) + "'\n";
		}
		for (int i = 0; i < _where_Vector.size(); i++) {
			s += "where (" +i+ "): '" + _where_Vector.elementAt(i) + "'\n";
		}
		for (int i = 0; i < _values_Vector.size(); i++) {
			s += "values (" +i+ "): '"+_values_Vector.elementAt(i) + "'\n";
		}
		for (int i = 0; i < _field_Vector.size(); i++) {
			s += "field (" +i+ "): '" + _field_Vector.elementAt(i) + "'\n";
		}
		return s;
	*/
	}

	}

}