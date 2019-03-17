// GenericWorksheetData - data object to use in a worksheet that uses the generic table model and cell renderer

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
// GenericWorksheetData - a data object to use in a worksheet that uses the
// generic table model and cell renderer.
// ----------------------------------------------------------------------------
// Copyright:  See the COPYRIGHT file.
// ----------------------------------------------------------------------------
// History:
// 2003-12-10	J. Thomas Sapienza, RTi	Initial version.
// 2005-04-26	JTS, RTi		Added finalize().
// ----------------------------------------------------------------------------

namespace RTi.Util.GUI
{

	using DMIUtil = RTi.DMI.DMIUtil;

	using IOUtil = RTi.Util.IO.IOUtil;

	/// <summary>
	/// This class is a data object that can be used in conjunction with the
	/// Generic_TableModel and Generic_CellRenderer classes to display data in a
	/// worksheet, without having to build a specialized table model and renderer 
	/// for the data  <b>For information on how to build a worksheet that uses
	/// generic data, see the documentation for Generic_TableModel</b>.<para>
	/// Currently, there might be problems working with internal data other than
	/// Strings, Integers, Doubles and Dates.
	/// </para>
	/// </summary>
	public class GenericWorksheetData
	{

	/// <summary>
	/// Actually holds the data.
	/// </summary>
	private object[] __data = null;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="columns"> the number of columns of data that will be stored in the 
	/// data object.  This doesn't include the column that holds the row number.<para>
	/// </para>
	/// For instance, in a worksheet that will look similar to the following:<para><pre>
	/// Row #       Name            Number
	/// --------------------------------
	/// 1           Fort Collins    100000
	/// 2           Greeley         50000
	/// 3           Denver          1000000
	/// </para>
	/// </pre><para>
	/// The number of columns in the Generic Data would be <b>2</b>, although the 
	/// worksheet would actually have 3 columns.  All the matters is the number of
	/// columns of actual data. </param>
	public GenericWorksheetData(int columns)
	{
		__data = new object[columns];
		for (int i = 0; i < columns; i++)
		{
			__data[i] = null;
		}
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~GenericWorksheetData()
	{
		IOUtil.nullArray(__data);
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the number of columns of data in this data object. </summary>
	/// <returns> the number of columns of data in this data object. </returns>
	public virtual int getColumnCount()
	{
		return __data.Length;
	}

	/// <summary>
	/// Returns a new GenericWorksheetData object that has all the same classes of
	/// data as the one instantiating it.  If a field has a String, Integer, Double,
	/// or Date, it will be filled in with the DMIUtil.MISSING_* version of that
	/// value.  Otherwise, the field will be filled with 'null'. </summary>
	/// <returns> a new GenericWorksheetData object that has all the same classes of
	/// data as the one instantiating it. </returns>
	public virtual GenericWorksheetData getEmptyGenericWorksheetData()
	{
		int count = getColumnCount();
		GenericWorksheetData d = new GenericWorksheetData(count);

		object o = null;
		for (int i = 0; i < count; i++)
		{
			o = getValueAt(i);
			if (o is string)
			{
				d.setValueAt(i, DMIUtil.MISSING_STRING);
			}
			else if (o is int?)
			{
				d.setValueAt(i, new int?(DMIUtil.MISSING_INT));
			}
			else if (o is double?)
			{
				d.setValueAt(i, new double?(DMIUtil.MISSING_DOUBLE));
			}
			else if (o is System.DateTime)
			{
				d.setValueAt(i, DMIUtil.MISSING_DATE);
			}
			else
			{
				d.setValueAt(i, null);
			}
		}

		return d;
	}

	/// <summary>
	/// Returns the value at the specified position.. </summary>
	/// <param name="pos"> the position at which to return data. </param>
	/// <returns> the value at the specified position. </returns>
	public virtual object getValueAt(int pos)
	{
		if (pos < 0 || pos > __data.Length)
		{
			return null;
		}

		return __data[pos];
	}

	/// <summary>
	/// Sets the value at the specified position. </summary>
	/// <param name="pos"> the position at which to set the value. </param>
	/// <param name="value"> the value to set. </param>
	public virtual void setValueAt(int pos, object value)
	{
		__data[pos] = value;
	}

	/// <summary>
	/// Returns a nice-looking String representation of all the data in this object. </summary>
	/// <returns> a nice-looking String representation of all the data in this object. </returns>
	public override string ToString()
	{
		string s = "";
		object o;
		for (int i = 0; i < __data.Length; i++)
		{
			o = getValueAt(i);
			if (o == null)
			{
				s += "NULL [class NULL]\n";
			}
			else
			{
				s += "" + (i + 1) + "]: ";
				if (o is string)
				{
					s += "'" + o + "'";
				}
				else
				{
					s += o;
				}
				s += "  [" + o.GetType() + "]\n";
			}
		}
		s += "\n";

		return s;
	}

	}

}