using System;
using System.Text;

// DMISelectStatement - select statement

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
	/// The DMISelectStatement class stores basic information about SQL select statements, allowing select statements to
	/// be formed and executed.
	/// See HydroBaseDMI for many good examples of using DMISelectStatement;
	/// TODO (JTS - 2006-05-23) Add some examples.
	/// </summary>
	public class DMISelectStatement : DMIStatement
	{

	/// <summary>
	/// Flag indicating whether a distinct select is executed.
	/// </summary>
	protected internal bool _distinct;

	/// <summary>
	/// Flag indicating whether the ORDER BY clause should be a GROUP BY clause, instead.
	/// </summary>
	protected internal bool _groupBy = false;

	/// <summary>
	/// Indicating whether a "top" clause should be used by indicating the number of rows to return.
	/// -1 means no "top" clause.
	/// @TODO SAM 2013-04-10 This capability appears to vary quite a bit between database vendors
	/// and is mainly being implemented to support SQL Server, although some other engines are checked
	/// for in toString().
	/// </summary>
	protected internal int _top = -1;

	/// <summary>
	/// Construct a select statement.
	/// </summary>
	public DMISelectStatement(DMI dmi) : base(dmi)
	{
		_distinct = false;
	}

	/// <summary>
	/// Executes this statement's stored procedure.  If this statement was not set
	/// up as a Stored Procedure, an exception will be thrown. </summary>
	/// <returns> the ResultSet that was returned from the query. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.sql.ResultSet executeStoredProcedure() throws java.sql.SQLException
	public virtual ResultSet executeStoredProcedure()
	{
		if (!isStoredProcedure())
		{
			throw new SQLException("Cannot use executeStoredProcedure() to " + "execute a DMISelectStatement that is not a stored procedure.");
		}
		return __storedProcedureCallableStatement.executeQuery();
	}

	/// <summary>
	/// Returns whether the ORDER BY clause should be a GROUP BY clause, instead. </summary>
	/// <returns> whether the ORDER BY clause should be a GROUP BY clause, instead. </returns>
	public virtual bool getGroupBy()
	{
		return _groupBy;
	}

	/// <summary>
	/// Indicate whether a distinct select statement is to be executed. </summary>
	/// <returns> true if a DISTINCT select will be executed. </returns>
	public virtual bool selectDistinct()
	{
		return _distinct;
	}

	/// <summary>
	/// Set whether a distinct select statement is to be executed. </summary>
	/// <param name="distinct"> If true, a DISTINCT select will be executed. </param>
	/// <returns> the value of the flag, after setting. </returns>
	public virtual bool selectDistinct(bool distinct)
	{
		_distinct = distinct;
		return _distinct;
	}

	/// <summary>
	/// Sets whether the ORDER BY clause should be a GROUP BY clause, instead. </summary>
	/// <param name="groupBy"> true or false </param>
	public virtual void setGroupBy(bool groupBy)
	{
		_groupBy = groupBy;
	}

	/// <summary>
	/// Sets whether a TOP clause should be used (the syntax will vary by database engine). </summary>
	/// <param name="top"> number of rows to return </param>
	public virtual void setTop(int top)
	{
		_top = top;
	}

	/// <summary>
	/// Formats the SELECT statement for Access databases. </summary>
	/// <returns> the SELECT statement as a string. </returns>
	private string toAccessString()
	{
		StringBuilder statement = new StringBuilder("SELECT ");

		if (_distinct)
		{
			statement.Append("DISTINCT ");
		}

		int size = _field_Vector.Count;
		if (size > 0)
		{
			statement.Append(_field_Vector[0]);
			for (int i = 1; i < size; i++)
			{
				statement.Append(", " + _field_Vector[i]);
			}
		}

		size = _table_Vector.Count;
		if (size > 0 && _join_Vector.Count == 0)
		{
			statement.Append(" FROM ");
			statement.Append(_table_Vector[0]);
			for (int i = 1; i < size; i++)
			{
				statement.Append(", " + _table_Vector[i]);
			}
		}

		size = _join_Vector.Count;
		if (size > 0)
		{
			int type = -1;
			string s = null;

			statement.Append(" FROM ");
			for (int i = 0; i < (size - 1); i++)
			{
				statement.Append("(");
			}

			statement.Append(_table_Vector[0]);

			for (int i = 0; i < size; i++)
			{
				type = ((int?)_join_type_Vector[i]).Value;
				if (type == _JOIN_INNER)
				{
					statement.Append(" INNER JOIN ");
				}
				else if (type == _JOIN_LEFT)
				{
					statement.Append(" LEFT JOIN ");
				}
				else if (type == _JOIN_RIGHT)
				{
					statement.Append(" RIGHT JOIN ");
				}
				s = _join_Vector[i];
				statement.Append(s + " ON ");
				s = _on_Vector[i];
				statement.Append(s);

				if (size > 1 && i < (size - 1))
				{
					// only do this for joins where joining more
					// than one thing, but do not do for the last
					// join in such an occasion
					statement.Append(")");
				}
			}
		}

		size = _where_Vector.Count;
		if (size > 0)
		{
			statement.Append(" WHERE ");
			statement.Append(_where_Vector[0]);
			for (int i = 1; i < size; i++)
			{
				statement.Append(" AND (" + _where_Vector[i] + ")");
			}
		}

		size = _order_by_Vector.Count;
		if (size > 0)
		{
			if (_groupBy)
			{
				statement.Append(" GROUP BY ");
			}
			else
			{
				statement.Append(" ORDER BY ");
			}
			statement.Append(_order_by_Vector[0]);
			for (int i = 1; i < size; i++)
			{
				statement.Append(", " + _order_by_Vector[i]);
			}
		}
		return statement.ToString();
	}

	/// <summary>
	/// Format the SELECT statement. </summary>
	/// <returns> the SELECT statement as a string. </returns>
	public override string ToString()
	{
		if (_dmi.getDatabaseEngine().Equals("Access", StringComparison.OrdinalIgnoreCase))
		{
			return toAccessString();
		}

		StringBuilder statement = new StringBuilder("SELECT ");

		if (_top > 0)
		{
			if (_dmi.getDatabaseEngineType() == DMI.DBENGINE_SQLSERVER)
			{
				statement.Append("TOP " + _top + " ");
			}
		}

		if (_distinct)
		{
			statement.Append("DISTINCT ");
		}

		int size = _field_Vector.Count;
		if (size > 0)
		{
			statement.Append(_field_Vector[0]);
			for (int i = 1; i < size; i++)
			{
				statement.Append(", " + DMIUtil.escapeField(_dmi,_field_Vector[i]));
			}
		}

		size = _table_Vector.Count;
		if (size > 0)
		{
			statement.Append(" FROM ");
			statement.Append(_table_Vector[0]);
			for (int i = 1; i < size; i++)
			{
				statement.Append(", " + _table_Vector[i]);
			}
		}

		size = _join_Vector.Count;
		if (size > 0)
		{
			int type = -1;
			string s = null;
			for (int i = 0; i < size; i++)
			{
				type = (_join_type_Vector[i]);
				if (type == _JOIN_INNER)
				{
					statement.Append(" INNER JOIN ");
				}
				else if (type == _JOIN_LEFT)
				{
					statement.Append(" LEFT JOIN ");
				}
				else if (type == _JOIN_RIGHT)
				{
					statement.Append(" RIGHT JOIN ");
				}
				s = _join_Vector[i];
				statement.Append(s + " ON ");
				s = _on_Vector[i];
				statement.Append(s);
			}
		}

		int whereSize = _where_Vector.Count;
		if (whereSize > 0)
		{
			statement.Append(" WHERE ");
			statement.Append(_where_Vector[0]);
			for (int i = 1; i < whereSize; i++)
			{
				statement.Append(" AND (" + _where_Vector[i] + ")");
			}
		}
		if (_top > 0)
		{
			if (_dmi.getDatabaseEngineType() == DMI.DBENGINE_ORACLE)
			{
				if (whereSize == 0)
				{
					statement.Append(" WHERE ");
				}
				else
				{
					statement.Append(" AND ");
				}
				statement.Append(" (ROWNUM <= " + _top + ")");
			}
		}

		size = _order_by_Vector.Count;
		if (size > 0)
		{
			if (_groupBy)
			{
				statement.Append(" GROUP BY ");
			}
			else
			{
				statement.Append(" ORDER BY ");
			}
			statement.Append(_order_by_Vector[0]);
			for (int i = 1; i < size; i++)
			{
				statement.Append(", " + _order_by_Vector[i]);
			}
		}

		if (_top > 0)
		{
			if (_dmi.getDatabaseEngineType() == DMI.DBENGINE_MYSQL)
			{
				statement.Append(" LIMIT " + _top);
			}
		}

		return statement.ToString();
	}

	}

}