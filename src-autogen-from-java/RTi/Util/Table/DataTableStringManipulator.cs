using System;
using System.Collections.Generic;

// DataTableStringManipulator - perform simple column-based string manipulation on a table

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
	using StringDictionary = RTi.Util.String.StringDictionary;
	using StringUtil = RTi.Util.String.StringUtil;
	using DateTime = RTi.Util.Time.DateTime;

	/// <summary>
	/// Perform simple column-based string manipulation on a table.
	/// </summary>
	public class DataTableStringManipulator
	{

	/// <summary>
	/// Data table on which to perform math.
	/// </summary>
	private DataTable __table = null;

	/// <summary>
	/// Filter to include rows.
	/// </summary>
	private StringDictionary __columnIncludeFilters = null;

	/// <summary>
	/// Filter to exclude rows.
	/// </summary>
	private StringDictionary __columnExcludeFilters = null;

	/// <summary>
	/// Construct an instance. </summary>
	/// <param name="table"> table that is being manipulated </param>
	/// <param name="columnIncludeFilters"> a list of filters that will be checked to include rows </param>
	/// <param name="columnIncludeFilters"> a list of filters that will be checked to exclude rows </param>
	public DataTableStringManipulator(DataTable table, StringDictionary columnIncludeFilters, StringDictionary columnExcludeFilters)
	{
		__table = table;
		__columnIncludeFilters = columnIncludeFilters;
		__columnExcludeFilters = columnExcludeFilters;
	}

	/// <summary>
	/// Get the list of operators that can be used.
	/// </summary>
	public static IList<DataTableStringOperatorType> getOperatorChoices()
	{
		IList<DataTableStringOperatorType> choices = new List<DataTableStringOperatorType>();
		choices.Add(DataTableStringOperatorType.APPEND);
		choices.Add(DataTableStringOperatorType.PREPEND);
		choices.Add(DataTableStringOperatorType.REPLACE);
		choices.Add(DataTableStringOperatorType.REMOVE);
		choices.Add(DataTableStringOperatorType.SPLIT);
		choices.Add(DataTableStringOperatorType.SUBSTRING);
		// TODO SAM 2015-04-29 Need to enable boolean
		//choices.add ( DataTableStringOperatorType.TO_BOOLEAN );
		choices.Add(DataTableStringOperatorType.TO_DATE);
		choices.Add(DataTableStringOperatorType.TO_DATE_TIME);
		choices.Add(DataTableStringOperatorType.TO_DOUBLE);
		choices.Add(DataTableStringOperatorType.TO_INTEGER);
		return choices;
	}

	/// <summary>
	/// Get the list of operators that can be performed. </summary>
	/// <returns> the operator display names as strings. </returns>
	public static IList<string> getOperatorChoicesAsStrings()
	{
		IList<DataTableStringOperatorType> choices = getOperatorChoices();
		IList<string> stringChoices = new List<string>();
		for (int i = 0; i < choices.Count; i++)
		{
			stringChoices.Add("" + choices[i]);
		}
		return stringChoices;
	}

	/// <summary>
	/// Perform a string manipulation. </summary>
	/// <param name="inputColumn1"> the name of the first column to use as input </param>
	/// <param name="operator"> the operator to execute for processing data </param>
	/// <param name="inputColumn2"> the name of the second column to use as input (if input2 is not specified), or null if not used </param>
	/// <param name="inputValue2"> the constant input to use as input (if inputColumn2 is not specified), or null if not used </param>
	/// <param name="inputValue3"> additional constant input to use as input, or null if not used </param>
	/// <param name="outputColumn"> the name of the output column </param>
	/// <param name="problems"> a list of strings indicating problems during processing </param>
	public virtual void manipulate(string inputColumn1, DataTableStringOperatorType @operator, string inputColumn2, string inputValue2, string inputValue3, string outputColumn, IList<string> problems)
	{
		string routine = this.GetType().Name + ".manipulate";
		// Construct the filter
		DataTableFilter filter = null;
		try
		{
			filter = new DataTableFilter(__table, __columnIncludeFilters, __columnExcludeFilters);
		}
		catch (Exception e)
		{
			// If any problems are detected then processing will be stopped below
			problems.Add(e.Message);
		}
		// Look up the columns for input and output
		int input1ColumnNum = -1;
		try
		{
			input1ColumnNum = __table.getFieldIndex(inputColumn1);
		}
		catch (Exception)
		{
			problems.Add("Input column (1) \"" + inputColumn1 + "\" not found in table \"" + __table.getTableID() + "\"");
		}
		int input2ColumnNum = -1;
		if (!string.ReferenceEquals(inputColumn2, null))
		{
			try
			{
				input2ColumnNum = __table.getFieldIndex(inputColumn2);
			}
			catch (Exception)
			{
				problems.Add("Input column (2) \"" + inputColumn2 + "\" not found in table \"" + __table.getTableID() + "\"");
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
			// Automatically add to the table, initialize with null
			// TODO SAM 2015-04-29 Need to enable Boolean
			//if ( operator == DataTableStringOperatorType.TO_BOOLEAN ) {
			//    __table.addField(new TableField(TableField.DATA_TYPE_BOOLEAN,outputColumn,-1,-1), null );
			//}
			if (@operator == DataTableStringOperatorType.TO_INTEGER)
			{
				__table.addField(new TableField(TableField.DATA_TYPE_INT,outputColumn,-1,-1), null);
			}
			else if ((@operator == DataTableStringOperatorType.TO_DATE) || (@operator == DataTableStringOperatorType.TO_DATE_TIME))
			{
				// Precision is handled by precision on individual date/time objects
				__table.addField(new TableField(TableField.DATA_TYPE_DATETIME,outputColumn,-1,-1), null);
			}
			else if (@operator == DataTableStringOperatorType.TO_DOUBLE)
			{
				__table.addField(new TableField(TableField.DATA_TYPE_DOUBLE,outputColumn,-1,6), null);
			}
			else
			{
				__table.addField(new TableField(TableField.DATA_TYPE_STRING,outputColumn,-1,-1), null);
			}
			try
			{
				outputColumnNum = __table.getFieldIndex(outputColumn);
			}
			catch (Exception)
			{
				// Should not happen.
				problems.Add("Output column \"" + outputColumn + "\" not found in table \"" + __table.getTableID() + "\".  Error addung column.");
			}
		}

		if (problems.Count > 0)
		{
			// Return if any problems were detected
			return;
		}

		// Check for special cases on input, for example ^ and $ are used with replace
		// In these cases, remove the special characters
		bool replaceStart = false;
		bool replaceEnd = false;
		if ((@operator == DataTableStringOperatorType.REPLACE) || (@operator == DataTableStringOperatorType.REMOVE))
		{
			if (!string.ReferenceEquals(inputValue2, null))
			{
				if (inputValue2.StartsWith("^", StringComparison.Ordinal))
				{
					replaceStart = true;
					inputValue2 = inputValue2.Substring(1, inputValue2.Length - 1);
				}
				else if (inputValue2.EndsWith("$", StringComparison.Ordinal))
				{
					replaceEnd = true;
					inputValue2 = inputValue2.Substring(0,inputValue2.Length - 1);
				}
				// Also replace "\s" with single space
				inputValue2 = inputValue2.Replace("\\s"," ");
			}
			if (@operator == DataTableStringOperatorType.REMOVE)
			{
				// Same as substring but second string is a space
				inputValue3 = "";
			}
			else
			{
				if (!string.ReferenceEquals(inputValue3, null))
				{
					// Also replace "\ " with single space, anywhere in the output
					inputValue3 = inputValue3.Replace("\\s"," ");
				}
			}
		}

		// Loop through the records
		int nrec = __table.getNumberOfRecords();
		object val = null;
		string input1Val = null;
		string input2Val = null;
		string input3Val = null;
		object outputVal = null;
		int maxChars = -1; // Maximum string length of output
		for (int irec = 0; irec < nrec; irec++)
		{
			// Check whether row should be included/excluded - "true" below indicates to throw exceptions
			try
			{
				if (!filter.includeRow(irec,true))
				{
					continue;
				}
			}
			catch (Exception e)
			{
				problems.Add(e.Message);
			}
			// Initialize the values
			input1Val = null;
			input2Val = null;
			input3Val = null;
			outputVal = null;
			// Get the input values
			try
			{
				val = __table.getFieldValue(irec, input1ColumnNum);
				if (val == null)
				{
					input1Val = null;
				}
				else
				{
					input1Val = "" + val; // Do this way so that even non-strings can be manipulated
				}
			}
			catch (Exception e)
			{
				problems.Add("Error getting value for input column 1 (" + e + ").");
				continue;
			}
			try
			{
				if (!string.ReferenceEquals(inputValue2, null))
				{
					input2Val = inputValue2;
				}
				else if (input2ColumnNum >= 0)
				{
					// Constant value was not given so get from column
					val = __table.getFieldValue(irec, input2ColumnNum);
					if (val == null)
					{
						input2Val = null;
					}
					else
					{
						input2Val = "" + val;
					}
				}
			}
			catch (Exception e)
			{
				problems.Add("Error getting value for input column 2 (" + e + ").");
				continue;
			}
			if (!string.ReferenceEquals(inputValue3, null))
			{
				// Only constant value is allowed (not from column)
				input3Val = inputValue3;
			}
			// Check for missing values and compute the output
			if (string.ReferenceEquals(input1Val, null))
			{
				// Output is null regardless of the operator
				outputVal = null;
			}
			else if (@operator == DataTableStringOperatorType.APPEND)
			{
				if (string.ReferenceEquals(input2Val, null))
				{
					outputVal = null;
				}
				else
				{
					outputVal = input1Val + input2Val;
				}
			}
			else if (@operator == DataTableStringOperatorType.PREPEND)
			{
				if (string.ReferenceEquals(input2Val, null))
				{
					outputVal = null;
				}
				else
				{
					outputVal = input2Val + input1Val;
				}
			}
			else if ((@operator == DataTableStringOperatorType.REPLACE) || (@operator == DataTableStringOperatorType.REMOVE))
			{
				if (@operator == DataTableStringOperatorType.REMOVE)
				{
					input3Val = ""; // Replace found string with empty string
				}
				// This is tricky because don't want to change unless there is a match.
				// Problems can occur if one call messes with data that another call previously changed.
				// Therefore need to handle with care depending on whether output column is the same as input column.
				if (input1ColumnNum == outputColumnNum)
				{
					// Default is output will be the same as input unless changed below
					outputVal = input1Val;
				}
				else
				{
					// Get the value of the output column before manipulation
					try
					{
						object o = __table.getFieldValue(irec, outputColumnNum);
						if (o == null)
						{
							outputVal = null;
						}
						else
						{
							outputVal = "" + o;
						}
					}
					catch (Exception)
					{
						outputVal = null;
					}
					if (outputVal == null)
					{
						// Probably first pass manipulating so set to input
						outputVal = input1Val;
					}
				}
				if ((!string.ReferenceEquals(input2Val, null)) && (!string.ReferenceEquals(input3Val, null)))
				{
					// Handle strings at beginning and end specifically
					if (replaceStart)
					{
						if (input1Val.StartsWith(input2Val, StringComparison.Ordinal))
						{
							if (input1Val.Length > input2Val.Length)
							{
								// Have longer string so have to replace part
								outputVal = input3Val + input1Val.Substring(input2Val.Length);
							}
							else
							{
								// Replace whole string
								outputVal = input3Val;
							}
						}
						// Else defaults to default output as determined above
					}
					else if (replaceEnd)
					{
						input2Val = input2Val.Substring(0,input2Val.Length);
						if (input1Val.EndsWith(input2Val, StringComparison.Ordinal))
						{
							if (input1Val.Length > input2Val.Length)
							{
								outputVal = input1Val.Substring(0,input1Val.Length - input2Val.Length) + input3Val;
							}
							else
							{
								outputVal = input3Val;
							}
						}
						// Else defaults to default output as determined above
					}
					else
					{
						// Simple replace - may not do anything if not matched
						string outputValTmp = input1Val.Replace(input2Val, input3Val);
						if (!outputValTmp.Equals(outputVal))
						{
							// Output was changed so update the value, otherwise leave previous output determined above
							outputVal = outputValTmp;
						}
					}
				}
			}
			else if (@operator == DataTableStringOperatorType.SPLIT)
			{
				// That parameter character positions are 1+ but internal positions are 0+
				// Split out a token where input2Value is the delimiter and input3Value is the integer position (1++)
				// TODO SAM 2016-06-16 Figure out how to error-handle better
				int input3ValInt = -1;
				try
				{
					input3ValInt = int.Parse(input3Val);
				}
				catch (Exception)
				{
					input3ValInt = -1;
				}
				if ((string.ReferenceEquals(input1Val, null)) || input1Val.Length == 0)
				{
					outputVal = "";
				}
				else
				{
					if (input3ValInt >= 0)
					{
						// First break the string list
						inputValue2 = inputValue2.Replace("\\s"," ");
						inputValue2 = inputValue2.Replace("\\n","\n");
						IList<string> tokens = StringUtil.breakStringList(input1Val,input2Val,0);
						int iCol = input3ValInt - 1;
						if (iCol < tokens.Count)
						{
							outputVal = tokens[iCol];
						}
						else
						{
							outputVal = "";
						}
					}
				}
			}
			else if (@operator == DataTableStringOperatorType.SUBSTRING)
			{
				// Note that parameter character positions are 1+ but internal positions are 0+
				// Extract a substring from the string based on character positions
				int input2ValInt = -1;
				int input3ValInt = -1;
				try
				{
					input2ValInt = int.Parse(input2Val);
				}
				catch (Exception)
				{
					input2ValInt = -1;
				}
				try
				{
					input3ValInt = int.Parse(input3Val);
				}
				catch (Exception)
				{
					input3ValInt = -1;
				}
				if ((input2ValInt >= 0) && (input3ValInt < 0))
				{
					// Substring to end of string
					if (input2ValInt > input1Val.Length)
					{
						outputVal = "";
					}
					else
					{
						outputVal = input1Val.Substring(input2ValInt - 1);
					}
				}
				else if ((input2ValInt >= 0) && (input3ValInt >= 0))
				{
					int input1ValLength = input1Val.Length;
					outputVal = "";
					if ((input2ValInt <= input1ValLength) && (input3ValInt <= input1ValLength))
					{
						outputVal = input1Val.Substring((input2ValInt - 1), input3ValInt - (input2ValInt - 1));
					}
				}
			}
			else if ((@operator == DataTableStringOperatorType.TO_DATE) || (@operator == DataTableStringOperatorType.TO_DATE_TIME))
			{
				try
				{
					outputVal = DateTime.parse(input1Val);
					// TODO SAM 2013-05-13 Evaluate whether this is needed since string should parse
					//if ( operator == DataTableStringOperatorType.TO_DATE ) {
					//    ((DateTime)outputVal).setPrecision(DateTime.PRECISION_DAY);
					//}
				}
				catch (Exception)
				{
					outputVal = null;
				}
			}
			else if (@operator == DataTableStringOperatorType.TO_DOUBLE)
			{
				try
				{
					outputVal = double.Parse(input1Val);
				}
				catch (System.FormatException)
				{
					outputVal = null;
				}
			}
			else if (@operator == DataTableStringOperatorType.TO_INTEGER)
			{
				try
				{
					outputVal = int.Parse(input1Val);
				}
				catch (System.FormatException)
				{
					outputVal = null;
				}
			}
			// Check the length of the string because may need to reset output column width
			if (input1ColumnNum == outputColumnNum)
			{
				if ((outputVal != null) && outputVal is string)
				{
					string s = (string)outputVal;
					if (s.Length > maxChars)
					{
						maxChars = s.Length;
					}
				}
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
			// Set the column width
			if (input1ColumnNum == outputColumnNum)
			{
				int width = __table.getFieldWidth(outputColumnNum);
				if (maxChars > width)
				{
					try
					{
						__table.setFieldWidth(outputColumnNum, maxChars);
					}
					catch (Exception)
					{
					}
				}
			}
		}
	}

	}

}