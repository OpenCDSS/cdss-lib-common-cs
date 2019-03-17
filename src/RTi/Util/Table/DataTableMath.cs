using System;
using System.Collections.Generic;

// DataTableMath - perform simple column-based math operations on a table

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
	/// Perform simple column-based math operations on a table.
	/// </summary>
	public class DataTableMath
	{

	/// <summary>
	/// Data table on which to perform math.
	/// </summary>
	private DataTable __table = null;

	/// <summary>
	/// Construct an instance using the table to operate on.
	/// </summary>
	public DataTableMath(DataTable table)
	{
		__table = table;
	}

	/// <summary>
	/// Get the list of operators that can be used.
	/// </summary>
	public static IList<DataTableMathOperatorType> getOperatorChoices()
	{
		IList<DataTableMathOperatorType> choices = new List<DataTableMathOperatorType>();
		choices.Add(DataTableMathOperatorType.ADD);
		choices.Add(DataTableMathOperatorType.SUBTRACT);
		choices.Add(DataTableMathOperatorType.MULTIPLY);
		choices.Add(DataTableMathOperatorType.DIVIDE);
		//choices.add ( DataTableMathOperatorType.TO_DOUBLE ); // TODO SAM 2013-08-26 Need to enable below, with Integer as input for all
		choices.Add(DataTableMathOperatorType.TO_INTEGER);
		return choices;
	}

	/// <summary>
	/// Get the list of operators that can be performed. </summary>
	/// <returns> the operator display names as strings. </returns>
	public static IList<string> getOperatorChoicesAsStrings()
	{
		IList<DataTableMathOperatorType> choices = getOperatorChoices();
		IList<string> stringChoices = new List<string>();
		for (int i = 0; i < choices.Count; i++)
		{
			stringChoices.Add("" + choices[i]);
		}
		return stringChoices;
	}

	/// <summary>
	/// Perform a math calculation. </summary>
	/// <param name="input1"> the name of the first column to use as input </param>
	/// <param name="operator"> the operator to execute for processing data </param>
	/// <param name="input2"> the name of the second column to use as input, or a constant </param>
	/// <param name="output"> the name of the output column </param>
	/// <param name="nonValue"> value to assign when floating point numbers cannot be computed (null or Double.NaN) </param>
	/// <param name="problems"> a list of strings indicating problems during processing </param>
	public virtual void math(string input1, DataTableMathOperatorType @operator, string input2, string output, double? nonValue, IList<string> problems)
	{
		string routine = this.GetType().Name + ".math";
		// Look up the columns for input and output
		int input1Field = -1;
		int input1FieldType = -1;
		int input2FieldType = -1;
		int outputFieldType = -1;
		try
		{
			input1Field = __table.getFieldIndex(input1);
			input1FieldType = __table.getFieldDataType(input1Field);
		}
		catch (Exception)
		{
			problems.Add("Input column (1) \"" + input1 + "\" not found in table \"" + __table.getTableID() + "\"");
		}
		int input2Field = -1;
		double? input2ConstantDouble = null;
		int? input2ConstantInteger = null;
		if (@operator != DataTableMathOperatorType.TO_INTEGER)
		{
			// Need to get the second input to do the math
			if (StringUtil.isDouble(input2))
			{
				// Second input supplied as a double
				input2ConstantDouble = double.Parse(input2);
				input2FieldType = TableField.DATA_TYPE_DOUBLE;
			}
			if (StringUtil.isInteger(input2))
			{
				// Second input supplied as an integer - use instead of double (handle below if 1st and 2nd arguments are different)
				input2ConstantInteger = int.Parse(input2);
				input2FieldType = TableField.DATA_TYPE_INT;
			}
			if ((input2ConstantDouble == null) && (input2ConstantInteger == null))
			{
				// Second input supplied as a column name rather than constant number
				try
				{
					input2Field = __table.getFieldIndex(input2);
					input2FieldType = __table.getFieldDataType(input2Field);
				}
				catch (Exception)
				{
					problems.Add("Input column (2) \"" + input2 + "\" not found in table \"" + __table.getTableID() + "\"");
				}
			}
		}
		int outputField = -1;
		try
		{
			outputField = __table.getFieldIndex(output);
		}
		catch (Exception)
		{
			Message.printStatus(2, routine, "Output field \"" + output + "\" not found in table \"" + __table.getTableID() + "\" - automatically adding.");
			// Automatically add to the table, initialize with null (not nonValue)
			if ((@operator == DataTableMathOperatorType.TO_INTEGER) || ((input1FieldType == TableField.DATA_TYPE_INT) && (input2FieldType == TableField.DATA_TYPE_INT)))
			{
				outputField = __table.addField(new TableField(TableField.DATA_TYPE_INT,output,-1,-1), null);
			}
			else
			{
				// One or both output fields are floating point so default output to double
				outputField = __table.addField(new TableField(TableField.DATA_TYPE_DOUBLE,output,10,4), null);
			}
		}
		if ((input1FieldType != TableField.DATA_TYPE_INT) && (input1FieldType != TableField.DATA_TYPE_DOUBLE))
		{
			problems.Add("Input1 column type is not integer or double - cannot do math.");
		}
		if ((input2Field >= 0) && (input2FieldType != TableField.DATA_TYPE_INT) && (input2FieldType != TableField.DATA_TYPE_DOUBLE))
		{
			problems.Add("Input2 column type is not integer or double - cannot do math.");
		}

		if (problems.Count > 0)
		{
			// Return if any problems were detected
			return;
		}

		// Loop through the records
		int nrec = __table.getNumberOfRecords();
		object val;
		double? input1ValDouble = null;
		double? input2ValDouble = null;
		int? input1ValInteger = null;
		int? input2ValInteger = null;
		double? outputValDouble = Double.NaN;
		int? outputValInteger = null;
		for (int irec = 0; irec < nrec; irec++)
		{
			// Initialize the values
			input1ValDouble = Double.NaN;
			input2ValDouble = Double.NaN;
			outputValDouble = nonValue;
			input1ValInteger = null;
			input2ValInteger = null;
			outputValInteger = null;
			// Get the input values
			try
			{
				val = __table.getFieldValue(irec, input1Field);
				if (input1FieldType == TableField.DATA_TYPE_INT)
				{
					input1ValInteger = (int?)val;
				}
				else
				{
					input1ValDouble = (double?)val;
				}
			}
			catch (Exception e)
			{
				problems.Add("Error getting value for input field 1 (" + e + ").");
				continue;
			}
			if (input2Field >= 0)
			{
				try
				{
					// Second value is determined from table
					val = __table.getFieldValue(irec, input2Field);
					if (input2FieldType == TableField.DATA_TYPE_INT)
					{
						input2ValInteger = (int?)val;
					}
					else
					{
						input2ValDouble = (double?)val;
					}
				}
				catch (Exception e)
				{
					problems.Add("Error getting value for input field 2 (" + e + ").");
					continue;
				}
			}
			else
			{
				if (input2ConstantDouble != null)
				{
					// Second value was a constant
					input2ValDouble = input2ConstantDouble;
				}
				if (input2ConstantInteger != null)
				{
					// Second value was a constant
					input2ValInteger = input2ConstantInteger;
				}
			}
			// TODO SAM 2015-08-14 If at least one of the inputs is a double then the output is a double
			if (input1FieldType != input2FieldType)
			{
				// Make sure the input is cast properly
				if (input1FieldType == TableField.DATA_TYPE_INT)
				{
					if (input1ValInteger == null)
					{
						input1ValDouble = null;
					}
					else
					{
						input1ValDouble = 0.0 + input1ValInteger;
					}
				}
				if (input2FieldType == TableField.DATA_TYPE_INT)
				{
					if (input2ValInteger == null)
					{
						input2ValDouble = null;
					}
					else
					{
						input2ValDouble = 0.0 + input2ValInteger;
					}
				}
			}
			// Check for missing values and compute the output
			if (@operator == DataTableMathOperatorType.TO_INTEGER)
			{
				// Only need the first input
				// Set integer and double in case output table column is not configured properly
				if (input1FieldType == TableField.DATA_TYPE_DOUBLE)
				{
					if ((input1ValDouble == null) || input1ValDouble.isNaN())
					{
						outputValInteger = null;
						outputValDouble = nonValue;
					}
					else
					{
						outputValInteger = input1ValDouble.Value;
						outputValDouble = (double)input1ValDouble.Value;
					}
				}
				else if (input1FieldType == TableField.DATA_TYPE_INT)
				{
					if (input1ValInteger == null)
					{
						outputValInteger = null;
						outputValDouble = nonValue;
					}
					else
					{
						outputValInteger = input1ValInteger;
						outputValDouble = (double)input1ValInteger;
					}
				}
			}
			else
			{
				// The following operators need two input values to compute
				if ((input1FieldType == TableField.DATA_TYPE_DOUBLE) || (input2FieldType == TableField.DATA_TYPE_DOUBLE) || (input1FieldType != input2FieldType))
				{
					// Double input and double output (or mixed in which case double values were set above)
					outputFieldType = TableField.DATA_TYPE_DOUBLE;
					if ((input1ValDouble == null) || input1ValDouble.isNaN() || (input2ValDouble == null) || input2ValDouble.isNaN())
					{
						outputValDouble = nonValue;
					}
					else if (@operator == DataTableMathOperatorType.ADD)
					{
						outputValDouble = input1ValDouble + input2ValDouble;
					}
					else if (@operator == DataTableMathOperatorType.SUBTRACT)
					{
						outputValDouble = input1ValDouble - input2ValDouble;
					}
					else if (@operator == DataTableMathOperatorType.MULTIPLY)
					{
						outputValDouble = input1ValDouble * input2ValDouble;
					}
					else if (@operator == DataTableMathOperatorType.DIVIDE)
					{
						if (input2ValDouble == 0.0)
						{
							outputValDouble = nonValue;
						}
						else
						{
							outputValDouble = input1ValDouble / input2ValDouble;
						}
					}
				}
				else if ((input1FieldType == TableField.DATA_TYPE_INT) && (input2FieldType == TableField.DATA_TYPE_INT))
				{
					// Integer input and integer output
					outputFieldType = TableField.DATA_TYPE_INT;
					if ((input1ValInteger == null) || (input2ValInteger == null))
					{
						outputValInteger = null;
					}
					else if (@operator == DataTableMathOperatorType.ADD)
					{
						outputValInteger = input1ValInteger + input2ValInteger;
					}
					else if (@operator == DataTableMathOperatorType.SUBTRACT)
					{
						outputValInteger = input1ValInteger - input2ValInteger;
					}
					else if (@operator == DataTableMathOperatorType.MULTIPLY)
					{
						outputValInteger = input1ValInteger * input2ValInteger;
					}
					else if (@operator == DataTableMathOperatorType.DIVIDE)
					{
						if (input2ValInteger == 0.0)
						{
							outputValInteger = null;
						}
						else
						{
							outputValInteger = input1ValInteger / input2ValInteger;
						}
					}
				}
			}
			// Set the value...
			try
			{
				if (outputFieldType == TableField.DATA_TYPE_INT)
				{
					__table.setFieldValue(irec, outputField, outputValInteger);
				}
				else if (outputFieldType == TableField.DATA_TYPE_DOUBLE)
				{
					__table.setFieldValue(irec, outputField, outputValDouble);
				}
				else
				{
					// TODO SAM 2016-08-02 may need to support other output columns like strings
				}
			}
			catch (Exception e)
			{
				if (outputFieldType == TableField.DATA_TYPE_INT)
				{
					problems.Add("Error setting value in row [" + irec + "] to " + outputValInteger + " uniquetempvar.");
				}
				else if (outputFieldType == TableField.DATA_TYPE_DOUBLE)
				{
					problems.Add("Error setting value in row [" + irec + "] to " + outputValDouble + " uniquetempvar.");
				}
				else
				{
					problems.Add("Error setting value in row [" + irec + "] uniquetempvar.");
				}
			}
		}
	}

	}

}