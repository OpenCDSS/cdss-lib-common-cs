using System;
using System.Collections.Generic;

// DMITableModel - table model for displaying data from the DMI

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
// DMITableModel.java - Table model for displaying data from the DMI in a 
// 	JTable
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2002-05-??i	J. Thomas Sapienza, RTi	Initial Version
// 2002-07-02	JTS, RTi		Changed the private member variables
//					from being final.  
//					Added the remove column method. 
//					DMITableModel also can now store a 
//					Vector of the tables used in 
//					generating it.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.DMI
{


	/// <summary>
	/// This class is a table model that allows data from a query to be easily 
	/// displayed in a JTable.
	/// TODO (JTS - 2006-05-22) I don't think this class is necessary anymore.  It has probably not been used
	/// in 4 years and should be removed.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class DMITableModel extends javax.swing.table.AbstractTableModel
	public class DMITableModel : AbstractTableModel
	{

	/// <summary>
	/// The number of columns in the table
	/// </summary>
	private int colCount;

	/// <summary>
	/// The number of rows in the table
	/// </summary>
	private int rowCount;

	/// <summary>
	/// List to hold the data to be displayed in the table
	/// </summary>
	private IList<IList<object>> data;

	/// <summary>
	/// List to hold the names of the columns in the table
	/// </summary>
	private IList<string> names;

	/// <summary>
	/// List to hold the name(s) of the table(s) in the table
	/// </summary>
	private IList<string> tableNames;

	/// <summary>
	/// Constructs a new DMITableModel with the given values and columns. </summary>
	/// <param name="values"> A list of lists that contains all the values to be
	/// displayed by the JTable that uses this Table Model.  Each vector contained
	/// inside this vector should contain objects that can be easily displayed in a JTable. </param>
	/// <param name="colNames"> A vector of strings containing the names of the columns
	/// to be shown by this table model </param>
	public DMITableModel(IList<IList<object>> values, IList<string> colNames)
	{
		data = values;
		names = colNames;

		// The values vector *could* be an empty vector.  This might happen
		// when no values were returned from a query, for instance.  The 
		// column names should still be shown, but with a blank row.  
		// So a vector of blank values is initialized so that the JTable will
		// at least show the column names.
		if (values.Count == 0)
		{
			IList<object> v = new List<object>(names.Count);
			for (int i = 0; i < names.Count; i++)
			{
				v.Add("");
			}
			data.Add(v);
			rowCount = 1;
			colCount = names.Count;
		}
		else
		{
			IList<object> v = (IList<object>)values[0];
			rowCount = values.Count;
			colCount = v.Count;
		}
	}

	/// <summary>
	/// Initializes an empty table model.  When this table model is used as the 
	/// model for a JTable, the JTable will be empty.
	/// </summary>
	public DMITableModel()
	{
		data = null;
		names = null;
		tableNames = null;
		rowCount = 0;
		colCount = 0;
	}

	/// <summary>
	/// Calculates how wide a column should be displayed at.  It sizes the column
	/// to display the largest value held in the column, or the column name (if
	/// that is larger than the largest value)
	/// </summary>
	public virtual int calculateWidth(int col)
	{
		int width = ((getColumnName(col).Length) * 10) + 10;

		for (int i = 0; i < rowCount; i++)
		{
			string s = "" + getValueAt(i, col);
			Console.WriteLine("s: '" + s + "' (" + s.Length + ")");
			int w = s.Length;
			w = (w * 10) + 10;
			if (w > width)
			{
				width = w;
			}
		}
		return (width);
	}

	/// <summary>
	/// Returns the kind of field in a specified column </summary>
	/// <param name="c"> the column number for which to get the class </param>
	/// <returns> the class of the specified column </returns>
	public virtual Type getColumnClass(int c)
	{
		return (getValueAt(0, c).GetType());
	}

	/// <summary>
	/// Returns the number of columns stored in the table model </summary>
	/// <returns> the number of columns stored in the table model </returns>
	public virtual int getColumnCount()
	{
		return (colCount);
	}

	/// <summary>
	/// Returns the name of the specified column </summary>
	/// <param name="column"> the column for which to return the name </param>
	/// <returns> the name of the specified column </returns>
	public virtual string getColumnName(int column)
	{
		return ((string)names[column]);
	}

	/// <summary>
	/// Returns the number of rows in the table model </summary>
	/// <returns> the number of rows in the table model </returns>
	public virtual int getRowCount()
	{
		return (rowCount);
	}

	/// <summary>
	/// Returns the names of the tables "involved" in the table model </summary>
	/// <returns> the names of the tables "involved" in the table model </returns>
	public virtual IList<string> getTableNames()
	{
		return tableNames;
	}

	/// <summary>
	/// Returns the object stored at the specified position </summary>
	/// <param name="row"> the row in which the object is located </param>
	/// <param name="col"> the column in which the object is located </param>
	/// <returns> the object stored at the specified position </returns>
	public virtual object getValueAt(int row, int col)
	{
		IList<object> v = (IList<object>)data[row];
		if (v[col] == null)
		{
			return "";
		}
		else
		{
			return v[col];
		}
	}

	/// <summary>
	/// Removes a column and all its associated data from the table model </summary>
	/// <param name="columnName"> the name of the column to be removed </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void removeColumn(String columnName) throws Exception
	public virtual void removeColumn(string columnName)
	{
		int columnNum = -1;

		for (int i = 0; i < colCount; i++)
		{
			string s = (string)names[i];
			if (s.Equals(columnName, StringComparison.OrdinalIgnoreCase))
			{
				columnNum = i;
			}
		}

		if (columnNum == -1)
		{
			throw new Exception("Column '" + columnName + "' not found in table model.");
		}

		for (int i = 0; i < rowCount; i++)
		{
			((IList<object>)data[i]).RemoveAt(columnNum);
		}

		colCount--;
		names.RemoveAt(columnNum);
	}

	/// <summary>
	/// Removes a column and all its associated data from the table model </summary>
	/// <param name="columnNum"> the number of the column to remove </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void removeColumn(int columnNum) throws Exception
	public virtual void removeColumn(int columnNum)
	{
		removeColumn(getColumnName(columnNum));
	}

	/// <summary>
	/// Sets the list containing the names of the tables "involved" in the table model </summary>
	/// <param name="tables"> a String vector of the names of the tables in the table model </param>
	public virtual void setTableNames(IList<string> tables)
	{
		tableNames = tables;
	}

	}

}