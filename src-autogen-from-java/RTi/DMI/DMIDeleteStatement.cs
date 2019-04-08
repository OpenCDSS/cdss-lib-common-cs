using System.Text;

// DMIDeleteStatement - delete statement

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

	/// <summary>
	/// The DMIDeleteStatement class stores basic information about SQL delete 
	/// statements.  Currently all functionality is included in the base class but at
	/// some point data and behavior may be moved to derived classes.
	/// </summary>
	public class DMIDeleteStatement : DMIStatement
	{

	/// <summary>
	/// Construct a delete statement.
	/// </summary>
	public DMIDeleteStatement(DMI dmi) : base(dmi)
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
			throw new SQLException("Cannot use executeStoredProcedure() to " + "execute a DMIDeleteStatement that is not a stored procedure.");
		}
		__storedProcedureCallableStatement.executeUpdate();
	}

	/// <summary>
	/// Format the DELETE statement. </summary>
	/// <returns> the DELETE statement as a string. </returns>
	public override string ToString()
	{
		StringBuilder statement = new StringBuilder("DELETE ");

		int size = _table_Vector.Count;
		if (size > 0)
		{
			statement.Append(" FROM ");
			statement.Append(_table_Vector[0]);
		}

		size = _where_Vector.Count;
		if (size > 0)
		{
			statement.Append(" WHERE ");
			statement.Append(_where_Vector[0]);
			for (int i = 1; i < size; i++)
			{
				statement.Append(" AND " + _where_Vector[i]);
			}
		}
		return statement.ToString();
	}

	}

}