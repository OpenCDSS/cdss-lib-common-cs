using System;
using System.Collections.Generic;
using System.IO;

// FixedFormatFileDataTableReader - class to read a fixed format file into a data table

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

namespace RTi.Util.Table
{

	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// Class to read a fixed format file into a data table.
	/// </summary>
	public class FixedFormatFileDataTableReader
	{

	/// <summary>
	/// Constructor.
	/// </summary>
	public FixedFormatFileDataTableReader()
	{
	}

	/// <summary>
	/// Read the specified file into a data table.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public DataTable readDataTable(String filename, String dataFormat, String [] columnNames) throws java.io.IOException
	public virtual DataTable readDataTable(string filename, string dataFormat, string[] columnNames)
	{
		string routine = this.GetType().Name + ".readDataTable";
		// Parse the format 
		IList<int> columnTypes = new List<int>();
		IList<int> columnWidths = new List<int>();
		StringUtil.fixedReadParseFormat(dataFormat, columnTypes, columnWidths);
		int[] columnTypeArray = new int[columnTypes.Count];
		int[] columnWidthArray = new int[columnWidths.Count];
		// Create a data table with the correct column types
		// TODO SAM 2014-03-30 Need to assign column names intelligently
		DataTable table = new DataTable();
		int columnCount = 0;
		for (int iCol = 0; iCol < columnTypes.Count; iCol++)
		{
			int columnType = columnTypes[iCol];
			columnTypeArray[iCol] = columnType;
			columnWidthArray[iCol] = columnWidths[iCol];
			if (columnType == StringUtil.TYPE_SPACE)
			{
				continue;
			}
			++columnCount;
			string columnName = "Column" + columnCount;
			if ((columnNames != null) && (columnNames.Length > 0) && (columnNames.Length >= columnCount))
			{
				columnName = columnNames[columnCount - 1];
			}
			if ((columnType == StringUtil.TYPE_CHARACTER) || (columnType == StringUtil.TYPE_STRING))
			{
				table.addField(new TableField(TableField.DATA_TYPE_STRING, columnName, columnWidths[iCol], -1), null);
			}
			else if (columnType == StringUtil.TYPE_FLOAT)
			{
				table.addField(new TableField(TableField.DATA_TYPE_FLOAT, columnName, columnWidths[iCol], 6), null);
			}
			else if (columnType == StringUtil.TYPE_DOUBLE)
			{
				table.addField(new TableField(TableField.DATA_TYPE_DOUBLE, columnName, columnWidths[iCol], 6), null);
			}
			else if (columnType == StringUtil.TYPE_INTEGER)
			{
				table.addField(new TableField(TableField.DATA_TYPE_INT, columnName, columnWidths[iCol], -1), null);
			}
		}
		StreamReader @in = null;
		string iline;
		int linecount = 0;
		IList<object> dataList = new List<object>(); // Will be reused for each line read
		try
		{
			@in = new StreamReader(filename);
			object o;
			while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
			{
				++linecount;
				// check for comments
				if (iline.StartsWith("#", StringComparison.Ordinal) || iline.Trim().Length == 0)
				{
					continue;
				}
				// The following will only return valid data types ("x" format is used only for spacing and not returned)
				StringUtil.fixedRead(iline, columnTypeArray, columnWidthArray, dataList);
				//Message.printStatus ( 2, routine, "Fixed read returned " + dataList.size() + " values");
				if (Message.isDebugOn)
				{
					Message.printDebug(50, routine, "Fixed read returned " + dataList.Count + " values");
				}
				TableRecord rec = new TableRecord();
				for (int iCol = 0; iCol < dataList.Count; iCol++)
				{
					o = dataList[iCol];
					if ((o != null) && (o is string))
					{
						rec.addFieldValue(((string)o).Trim());
					}
					else
					{
						rec.addFieldValue(o);
					}
				}
				table.addRecord(rec);
			}
		}
		catch (Exception e)
		{
			Message.printWarning(3, routine, "Error reading \"" + filename + "\" at line " + linecount);
			throw new IOException(e);
		}
		finally
		{
			if (@in != null)
			{
				@in.Close();
			}
		}
		return table;
	}

	}

}