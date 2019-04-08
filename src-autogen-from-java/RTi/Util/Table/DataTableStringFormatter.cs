using System;
using System.Collections.Generic;

// DataTableStringFormatter - format table columns to create an output column string

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
	/// Format table columns to create an output column string.
	/// </summary>
	public class DataTableStringFormatter
	{

	/// <summary>
	/// Data table on which to perform math.
	/// </summary>
	private DataTable __table = null;

	/// <summary>
	/// Construct an instance using the table to operate on.
	/// </summary>
	public DataTableStringFormatter(DataTable table)
	{
		__table = table;
	}

	/// <summary>
	/// Format a string </summary>
	/// <param name="inputColumns"> the name of the first column to use as input </param>
	/// <param name="format"> the operator to execute for processing data </param>
	/// <param name="inputColumn2"> the name of the second column to use as input (if input2 is not specified), or null if not used </param>
	/// <param name="inputValue2"> the constant input to use as input (if inputColumn2 is not specified), or null if not used </param>
	/// <param name="outputColumn"> the name of the output column </param>
	/// <param name="insertBeforeColumn"> the name of the column before which to insert the new column </param>
	/// <param name="problems"> a list of strings indicating problems during processing </param>
	public virtual void format(string[] inputColumns, string format, string outputColumn, string insertBeforeColumn, IList<string> problems)
	{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		string routine = this.GetType().FullName + ".format";
		// Look up the columns for input and output (do input last since new column may be inserted)
		int insertBeforeColumnNum = -1;
		if ((!string.ReferenceEquals(insertBeforeColumn, null)) && !insertBeforeColumn.Equals(""))
		{
			try
			{
				insertBeforeColumnNum = __table.getFieldIndex(insertBeforeColumn);
			}
			catch (Exception)
			{
				problems.Add("Insert before column \"" + insertBeforeColumn + "\" not found in table \"" + __table.getTableID() + "\"");
			}
		}
		int outputColumnNum = -1;
		try
		{
			outputColumnNum = __table.getFieldIndex(outputColumn);
		}
		catch (Exception)
		{
			Message.printStatus(2, routine, "Output column \"" + outputColumn + "\" not found in table \"" + __table.getTableID() + "\" - automatically adding.");
			__table.addField(insertBeforeColumnNum, new TableField(TableField.DATA_TYPE_STRING,outputColumn,-1,-1), null);
			try
			{
				outputColumnNum = __table.getFieldIndex(outputColumn);
			}
			catch (Exception)
			{
				// Should not happen.
				problems.Add("Output column \"" + outputColumn + "\" not found in table \"" + __table.getTableID() + "\".  Error adding column.");
			}
		}
		int[] inputColumnNum = new int[inputColumns.Length];
		for (int i = 0; i < inputColumns.Length; i++)
		{
			inputColumnNum[i] = -1;
			try
			{
				inputColumnNum[i] = __table.getFieldIndex(inputColumns[i]);
			}
			catch (Exception)
			{
				problems.Add("Input column (1) \"" + inputColumns[i] + "\" not found in table \"" + __table.getTableID() + "\"");
			}
		}

		if (problems.Count > 0)
		{
			// Return if any problems were detected
			return;
		}

		// Loop through the records, get the input column objects, and format for output
		int nrec = __table.getNumberOfRecords();
		string outputVal = null;
		IList<object> values = new List<object>();
		for (int irec = 0; irec < nrec; irec++)
		{
			// Get the input values
			values.Clear();
			for (int iCol = 0; iCol < inputColumnNum.Length; iCol++)
			{
				if (inputColumnNum[iCol] < 0)
				{
					// Set the result to null
					values.Clear();
					break;
				}
				else
				{
					try
					{
						values.Add(__table.getFieldValue(irec, inputColumnNum[iCol]));
					}
					catch (Exception e)
					{
						problems.Add("Error getting value for input column (" + e + ").");
						values.Clear();
						break;
					}
				}
			}
			if (inputColumnNum.Length != values.Count)
			{
				// Don't have the right number of values from the number of specified input columns 
				outputVal = null;
			}
			else
			{
				//Message.printStatus(2, routine, "format=\"" + format + "\"" );
				//for ( int i = 0; i < values.size(); i++ ) {
				//    Message.printStatus(2, routine, "value=\"" + values.get(i) + "\"" );
				//}
				outputVal = StringUtil.formatString(values,format);
			}
			// Set the value...
			try
			{
				__table.setFieldValue(irec, outputColumnNum, outputVal);
			}
			catch (Exception e)
			{
				problems.Add("Error setting value (" + e + ").");
			}
		}
	}

	}

}