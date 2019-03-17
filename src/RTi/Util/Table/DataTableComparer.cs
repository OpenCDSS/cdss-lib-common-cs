using System;
using System.Collections.Generic;
using System.Text;

// DataTableComparer - compare two tables for differences and create a new table that contains the comparison

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
	/// Compare two tables for differences and create a new table that contains the comparison.
	/// This table can be output to a simple HTML format to provide a visual way to find specific differences.
	/// The comparison is currently not very complicated.  Tables are assumed to have consistent column definitions
	/// and numbers of rows, although the comparison is done on strings so column types don't necessarily have to be
	/// the same.
	/// </summary>
	public class DataTableComparer
	{

	/// <summary>
	/// The first table to be compared.
	/// </summary>
	private DataTable __table1;

	/// <summary>
	/// The second table to be compared.
	/// </summary>
	private DataTable __table2;

	/// <summary>
	/// The list of column names to compare from the first table.
	/// </summary>
	private IList<string> __compareColumns1;

	/// <summary>
	/// The table positions for the columns being compared from the first table.
	/// </summary>
	private int[] __columnNumbers1;

	/// <summary>
	/// The list of column names to compare from the second table.
	/// </summary>
	private IList<string> __compareColumns2;

	/// <summary>
	/// The table positions for the columns being compared from the second table.
	/// </summary>
	private int[] __columnNumbers2;

	/// <summary>
	/// The name of the new table to be created.
	/// </summary>
	private string __newTableID = "";

	/// <summary>
	/// Whether to match columns by name (true) or order (false).
	/// </summary>
	private bool __matchColumnsByName = true;

	/// <summary>
	/// The precision to use when comparing floating point numbers.
	/// </summary>
	private int? __precision = null;

	/// <summary>
	/// The tolerance to use when comparing floating point numbers.
	/// </summary>
	private double? __tolerance = null;

	/// <summary>
	/// The comparison table that is created.  Can be null if not yet compared.
	/// </summary>
	private DataTable __comparisonTable;

	/// <summary>
	/// Array that indicates differences in the cells.  This is used for formatting output.
	/// It is an integer and not boolean because in the future more care may be implemented
	/// to allow tolerances in differences and consequently the table could be visualized with
	/// different colors depending on the level of difference.
	/// For each cell a value of 0 indicates no difference and 1 indicates different.
	/// </summary>
	private int[][] __differenceArray;

	/// <summary>
	/// Create the data table comparer instance and check for initialization problems. </summary>
	/// <param name="table1"> first table for comparison </param>
	/// <param name="compareColumns1"> list of column names from the first table to compare </param>
	/// <param name="excludeColumns1"> list of column names from the first table to exclude
	/// (removed from compareColumns1 if necessary) </param>
	/// <param name="table2"> second table for comparison </param>
	/// <param name="compareColumns2"> list of column names from the second table to compare </param>
	/// <param name="matchColumnsByName"> if true, then the column names are used to match columns for comparison, using the
	/// columns from the first table as the main list; if false, then columns are matched by column position </param>
	/// <param name="precision"> the number of digits (1+) after the decimal point to compare numbers in floating point columns
	/// (specify as null to ignore precision comparison) </param>
	/// <param name="tolerance"> the absolute value to check differences between floating point numbers (if not specified then
	/// values must be exact when checked to the precision) </param>
	/// <param name="newTableID"> name of new table to create with comparison results </param>
	public DataTableComparer(DataTable table1, IList<string> compareColumns1, IList<string> excludeColumns1, DataTable table2, IList<string> compareColumns2, bool matchColumnsByName, int? precision, double? tolerance, string newTableID)
	{
		// The tables being compared must not be null
		if (table1 == null)
		{
			throw new InvalidParameterException("The first table to compare is null.");
		}
		else
		{
			setTable1(table1);
		}
		if (table2 == null)
		{
			throw new InvalidParameterException("The second table to compare is null.");
		}
		else
		{
			setTable2(table2);
		}
		// Get the column names to compare, which will either be those passed in by calling code, or if not specified
		// will be the full list.
		if ((compareColumns1 == null) || (compareColumns1.Count == 0))
		{
			// Get the columns from the first table
			compareColumns1 = Arrays.asList(table1.getFieldNames());
			// Remove the columns to be ignored
			StringUtil.removeMatching(compareColumns1, excludeColumns1, true);
		}
		else
		{
			// Confirm that the columns exist
			StringBuilder warning = new StringBuilder();
			foreach (string column in compareColumns1)
			{
				try
				{
					table1.getFieldIndex(column);
				}
				catch (Exception)
				{
					warning.Append("; column1 to compare \"" + column + "\" does not exist in the first table");
				}
			}
			if (warning.Length > 0)
			{
				throw new InvalidParameterException("Some columns to compare in the first table do not exist:  " + warning + ".");
			}
		}
		setCompareColumns1(compareColumns1);
		if (compareColumns2 == null)
		{
			// Get the columns from the second table
			compareColumns2 = Arrays.asList(table2.getFieldNames());
		}
		else
		{
			// Confirm that the columns exist
			StringBuilder warning = new StringBuilder();
			foreach (string column in compareColumns2)
			{
				try
				{
					table2.getFieldIndex(column);
				}
				catch (Exception)
				{
					warning.Append("; column2 to compare \"" + column + "\" does not exist in the second table");
				}
			}
			if (warning.Length > 0)
			{
				throw new InvalidParameterException("Some columns to compare in the second table do not exist:  " + warning + ".");
			}
		}
		setCompareColumns2(compareColumns2);
		setMatchColumnsByName(matchColumnsByName);
		// The precision must be 0+
		if ((precision != null) && (precision < 0))
		{
			throw new InvalidParameterException("The precision (" + precision + ") if specified must be >= 0).");
		}
		setPrecision(precision);
		// The tolerance must be 0+
		if ((tolerance != null) && (tolerance < 0.0))
		{
			throw new InvalidParameterException("The tolerance (" + tolerance + ") if specified must be >= 0).");
		}
		setTolerance(tolerance);
		// The new table ID must be specified because the table use is controlled by the calling code and
		// an identifier conflict because of an assumed name should not be introduced here
		if ((string.ReferenceEquals(newTableID, null)) || newTableID.Equals(""))
		{
			throw new InvalidParameterException("The new table ID is null or blank.");
		}
		else
		{
			setNewTableID(newTableID);
		}
	}

	/// <summary>
	/// Perform the comparison, creating the output table.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void compare() throws Exception
	public virtual void compare()
	{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		string routine = this.GetType().FullName + ".compare";
		// At this point the inputs should be OK so create a new table that has columns that
		// include both of the original column names but are of type string
		DataTable table1 = getTable1();
		DataTable table2 = getTable2();
		DataTable comparisonTable = new DataTable();
		comparisonTable.setTableID(getNewTableID());
		IList<string> compareColumns1 = getCompareColumns1();
		IList<string> compareColumns2 = getCompareColumns2();
		// Table 1 is the master and consequently its indices will control the comparisons
		int[] columnNumbers1 = table1.getFieldIndices((string [])compareColumns1.ToArray());
		// Table 2 column numbers are first determined from the table...
		int[] columnNumbers2 = table2.getFieldIndices((string [])compareColumns2.ToArray());
		if (getMatchColumnsByName())
		{
			// Order in column2 may not be the same as was originally specified
			columnNumbers2 = new int[columnNumbers1.Length];
			// Loop through the first tables columns and find the matching column in the second table
			for (int i = 0; i < compareColumns1.Count; i++)
			{
				try
				{
					columnNumbers2[i] = table2.getFieldIndex(compareColumns1[i]);
					Message.printStatus(2,routine,"Column [" + i + "] \"" + compareColumns1[i] + "\" in table 1 matches column [" + columnNumbers2[i] + "] in table 2.");
				}
				catch (Exception)
				{
					columnNumbers2[i] = -1; // Column not matched
					Message.printStatus(2,routine,"Column [" + i + "] \"" + compareColumns1[i] + "\" in table 1 does not match any column in table 2.");
				}
			}
		}
		else
		{
			// Make sure that the second table column number array has at least as many elements as
			// the first table array
			if (columnNumbers2.Length < columnNumbers1.Length)
			{
				int[] columnNumbersTemp = new int[columnNumbers1.Length];
				for (int i = 0; i < columnNumbers1.Length; i++)
				{
					columnNumbersTemp[i] = -1; // default
				}
				// Copy original shorter array into first part of new array
				Array.Copy(columnNumbers2, 0, columnNumbersTemp, 0, columnNumbers2.Length);
				columnNumbers2 = columnNumbersTemp;
			}
		}
		setColumnNumbers1(columnNumbers1);
		setColumnNumbers2(columnNumbers2);
		string[] fieldFormats1 = table1.getFieldFormats(); // C-style formats to convert data to strings for comparison
		string[] fieldFormats2 = table2.getFieldFormats(); // These are in the position of the original table
		// If necessary, extend the array
		if (fieldFormats2.Length < fieldFormats1.Length)
		{
			string[] temp = new string[fieldFormats1.Length];
			for (int i = 0; i < fieldFormats1.Length; i++)
			{
				temp[i] = "";
			}
			Array.Copy(fieldFormats2, 0, temp, 0, fieldFormats2.Length);
			fieldFormats2 = temp;
		}
		int? precision = getPrecision();
		double? tolerance = getTolerance();
		if ((precision != null) && (precision >= 0))
		{
			// Update the field formats to use the requested precision, if a floating point field
			string fieldFormat = "%." + precision + "f";
			for (int i = 0; i < columnNumbers1.Length; i++)
			{
				if ((table1.getFieldDataType(columnNumbers1[i]) == TableField.DATA_TYPE_DOUBLE) || (table1.getFieldDataType(columnNumbers1[i]) == TableField.DATA_TYPE_FLOAT))
				{
					fieldFormats1[columnNumbers1[i]] = fieldFormat;
				}
			}
			for (int i = 0; i < columnNumbers2.Length; i++)
			{
				if (columnNumbers2[i] >= 0)
				{
					if ((table2.getFieldDataType(columnNumbers2[i]) == TableField.DATA_TYPE_DOUBLE) || (table2.getFieldDataType(columnNumbers2[i]) == TableField.DATA_TYPE_FLOAT))
					{
						fieldFormats2[columnNumbers2[i]] = fieldFormat;
					}
				}
			}
		}
		// Create an int array to track whether the cells are different (initial value is 0)
		// This is used as a style mask when formatting the HTML (where value of 1 indicates difference)
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: int[][] differenceArray = new int[table1.getNumberOfRecords()][compareColumns1.Count];
		int[][] differenceArray = RectangularArrays.RectangularIntArray(table1.getNumberOfRecords(), compareColumns1.Count);
		// Loop through the column lists, which should be the same size and define columns
		for (int icol = 0; icol < compareColumns1.Count; icol++)
		{
			// Define columns of type string (no width specified), where the column name will be a simple
			// concatenation of both column names, or one name if the column names for table1 and table2 match
			string colName1 = table1.getFieldName(columnNumbers1[icol]);
			string colName2 = ""; // Default for unmatched column - / will indicate difference in table names
			if (columnNumbers2[icol] >= 0)
			{
				colName2 = table2.getFieldName(columnNumbers2[icol]);
			}
			if (!colName1.Equals(colName2, StringComparison.OrdinalIgnoreCase))
			{
				// Show the column names from both tables
				colName1 += " / " + colName2;
			}
			int newField = comparisonTable.addField(new TableField(TableField.DATA_TYPE_STRING, colName1,-1), "");
			// Also set the column descriptions so the final results are easier to interpret
			string desc1 = table1.getTableField(columnNumbers1[icol]).getDescription();
			string desc2 = "";
			if (columnNumbers2[icol] >= 0)
			{
				desc2 = table2.getTableField(columnNumbers2[icol]).getDescription();
			}
			if (!desc1.Equals(desc2, StringComparison.OrdinalIgnoreCase))
			{
				desc1 += " / " + desc2;
			}
			comparisonTable.getTableField(newField).setDescription(desc1);
		}
		// Now loop through the records in table 1 and compare
		string formattedValue1;
		string formattedValue2;
		string formattedValue = null; // The comparison output
		object value1;
		object value2;
		string format1, format2;
		for (int irow = 0; irow < table1.getNumberOfRecords(); ++irow)
		{
			for (int icol = 0; icol < columnNumbers1.Length; icol++)
			{
				Message.printStatus(2, routine, "Comparing row [" + irow + "] columns [" + columnNumbers1[icol] + "] / [" + columnNumbers2[icol] + "]");
				// Get the value from the first table and format as a string for comparisons...
				value1 = null;
				if (columnNumbers1[icol] >= 0)
				{
					try
					{
						value1 = table1.getFieldValue(irow, columnNumbers1[icol]);
					}
					catch (Exception)
					{
						value1 = null;
					}
				}
				format1 = "";
				if ((value1 == null) || (columnNumbers1[icol] < 0))
				{
					formattedValue1 = "";
				}
				else
				{
					// TODO SAM 2010-12-18 Evaluate why trim is needed
					format1 = fieldFormats1[columnNumbers1[icol]];
					// Check for integer to format without trailing 0's.
					// First check for number, then for integer or infinity
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					if (((table1.getFieldDataType(columnNumbers1[icol]) == TableField.DATA_TYPE_DOUBLE) || (table1.getFieldDataType(columnNumbers1[icol]) == TableField.DATA_TYPE_FLOAT)) && (string.ReferenceEquals(value1.GetType().FullName, "Integer") || (double?) value1 == double.PositiveInfinity || (double?) value1 - (long)Math.Round((double?) value1, MidpointRounding.AwayFromZero) == 0))
					{
						formattedValue1 = StringUtil.formatString(value1,"%.0f").Trim();
					}
					else
					{
						formattedValue1 = StringUtil.formatString(value1,format1).Trim();
					}

				}
				// Get the value from the second table and format as a string for comparisons...
				// The rows in the second table must be in the same order
				// TODO SAM 2012-05-30 Enable sorting on table rows before comparison?
				value2 = null;
				if (columnNumbers2[icol] >= 0)
				{
					try
					{
						value2 = table2.getFieldValue(irow, columnNumbers2[icol]);
						//Message.printStatus(2,routine,"Value 2 from column [" + columnNumbers2[icol] + "] = " + value2);
					}
					catch (Exception)
					{
						value2 = null;
					}
				}
				format2 = "";
				if ((value2 == null) || (columnNumbers2[icol] < 0))
				{
					formattedValue2 = "";
				}
				else
				{
					format2 = fieldFormats2[columnNumbers2[icol]];
					// Check for integer to format without trailing 0's
					// First check for number, then for integer
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					if (((table2.getFieldDataType(columnNumbers2[icol]) == TableField.DATA_TYPE_DOUBLE) || (table2.getFieldDataType(columnNumbers2[icol]) == TableField.DATA_TYPE_FLOAT)) && (string.ReferenceEquals(value2.GetType().FullName, "Integer") || (double?) value2 == double.PositiveInfinity || (double?) value2 - (long)Math.Round((double?) value2, MidpointRounding.AwayFromZero) == 0))
					{
						formattedValue2 = StringUtil.formatString(value2,"%.0f").Trim();
					}
					else
					{
						formattedValue2 = StringUtil.formatString(value2,format2).Trim();
					}
				}
				// Default behavior is to compare strings so do this check first.
				if (formattedValue1.Equals(formattedValue2))
				{
					// Formatted values are the same so the output table value is just the formatted value
					formattedValue = formattedValue1;
				}
				else
				{
					// Show both values as "value1 / value2" and set the boolean indicating a difference
					if (((table1.getFieldDataType(columnNumbers1[icol]) == TableField.DATA_TYPE_DOUBLE) || (table1.getFieldDataType(columnNumbers1[icol]) == TableField.DATA_TYPE_FLOAT)) && (tolerance != null) && StringUtil.isDouble(formattedValue1) && StringUtil.isDouble(formattedValue2))
					{
						// Convert the formatted strings to doubles and compare the difference against the tolerance
						double dvalue1 = double.Parse(formattedValue1);
						double dvalue2 = double.Parse(formattedValue2);
						if (Math.Abs(dvalue1 - dvalue2) >= tolerance)
						{
							formattedValue = formattedValue1 + " / " + formattedValue2;
							differenceArray[irow][icol] = 1;
						}
						else
						{
							// Still show both values but don't set the difference flag since tolerance is met
							// Indicate that values compare within tolerance using ~
							formattedValue = formattedValue1 + " ~/~ " + formattedValue2;
						}
					}
					else
					{
						// Not floating point or floating point and no tolerance is specified so no need to do
						// additional comparison
						formattedValue = formattedValue1 + " / " + formattedValue2;
						differenceArray[irow][icol] = 1;
					}
				}
				// Set the field value, creating the row if necessary
				comparisonTable.setFieldValue(irow, icol, formattedValue, true);
				//Message.printStatus(2, "", "formattedValue1=\"" + formattedValue1 + "\" (format=" + format1 +
				//    ") formattedValue2=\"" + formattedValue2 + "\" (format=" + format2 +
				//    ") mask=" + differenceArray[irow][icol] );
			}
		}
		setComparisonTable(comparisonTable);
		setDifferenceArray(differenceArray);
	}

	/// <summary>
	/// Get the column numbers to compared from the first table.
	/// </summary>
	private int [] getColumnNumbers1()
	{
		return __columnNumbers1;
	}

	/// <summary>
	/// Get the column numbers to compared from the second table.
	/// </summary>
	private int [] getColumnNumbers2()
	{
		return __columnNumbers2;
	}

	/// <summary>
	/// Get the list of columns to be compared from the first table.
	/// </summary>
	private IList<string> getCompareColumns1()
	{
		return __compareColumns1;
	}

	/// <summary>
	/// Get the list of columns to be compared from the second table.
	/// </summary>
	private IList<string> getCompareColumns2()
	{
		return __compareColumns2;
	}

	/// <summary>
	/// Return the comparison table. </summary>
	/// <returns> the comparison table. </returns>
	public virtual DataTable getComparisonTable()
	{
		return __comparisonTable;
	}

	/// <summary>
	/// Return the difference array. </summary>
	/// <returns> the difference array. </returns>
	private int [][] getDifferenceArray()
	{
		return __differenceArray;
	}

	/// <summary>
	/// Return the count of the differences. </summary>
	/// <returns> the count of the differences. </returns>
	public virtual int getDifferenceCount()
	{
		int[][] differenceArray = getDifferenceArray();
		if (differenceArray == null)
		{
			return 0;
		}
		else
		{
			int differenceCount = 0;
			for (int irow = 0; irow < differenceArray.Length; irow++)
			{
				for (int icol = 0; icol < differenceArray[irow].Length; icol++)
				{
					if (differenceArray[irow][icol] > 0)
					{
						++differenceCount;
					}
				}
			}
			return differenceCount;
		}
	}

	/// <summary>
	/// Return whether to match the columns by name. </summary>
	/// <returns> true to match columns by name, false to match by order. </returns>
	private bool getMatchColumnsByName()
	{
		return __matchColumnsByName;
	}

	/// <summary>
	/// Return the identifier to be used for the new comparison table. </summary>
	/// <returns> the identifier to be used for the new comparison table. </returns>
	private string getNewTableID()
	{
		return __newTableID;
	}

	/// <summary>
	/// Return the precision to use for floating point comparisons. </summary>
	/// <returns> the precision to use for floating point comparisons. </returns>
	private int? getPrecision()
	{
		return __precision;
	}

	/// <summary>
	/// Return the first table being compared.
	/// </summary>
	public virtual DataTable getTable1()
	{
		return __table1;
	}

	/// <summary>
	/// Return the second table being compared.
	/// </summary>
	public virtual DataTable getTable2()
	{
		return __table2;
	}

	/// <summary>
	/// Return the tolerance to use for floating point comparisons. </summary>
	/// <returns> the tolerance to use for floating point comparisons. </returns>
	private double? getTolerance()
	{
		return __tolerance;
	}

	/// <summary>
	/// Set the column numbers being compared from the first table. </summary>
	/// <param name="columnNumbers1"> column numbers being compared from the first table </param>
	private void setColumnNumbers1(int[] columnNumbers1)
	{
		__columnNumbers1 = columnNumbers1;
	}

	/// <summary>
	/// Set the column numbers being compared from the second table. </summary>
	/// <param name="columnNumbers2"> column numbers being compared from the second table </param>
	private void setColumnNumbers2(int[] columnNumbers2)
	{
		__columnNumbers2 = columnNumbers2;
	}

	/// <summary>
	/// Set the list of columns being compared from the first table. </summary>
	/// <param name="compareColumns1"> list of columns being compared from the first table. </param>
	private void setCompareColumns1(IList<string> compareColumns1)
	{
		__compareColumns1 = compareColumns1;
	}

	/// <summary>
	/// Set the list of columns being compared from the second table. </summary>
	/// <param name="compareColumns2"> list of columns being compared from the second table. </param>
	private void setCompareColumns2(IList<string> compareColumns2)
	{
		__compareColumns2 = compareColumns2;
	}

	/// <summary>
	/// Set the comparison table created by this class. </summary>
	/// <param name="comparisonTable"> new comparison table. </param>
	private void setComparisonTable(DataTable comparisonTable)
	{
		__comparisonTable = comparisonTable;
	}

	/// <summary>
	/// Set the difference array. </summary>
	/// <param name="differenceArray"> the difference array. </param>
	private void setDifferenceArray(int[][] differenceArray)
	{
		__differenceArray = differenceArray;
	}

	/// <summary>
	/// Set whether to match columns by name. </summary>
	/// <param name="matchColumnsByName"> true to match by name, false to match by order. </param>
	private void setMatchColumnsByName(bool matchColumnsByName)
	{
		__matchColumnsByName = matchColumnsByName;
	}

	/// <summary>
	/// Set the name of the new comparison table being created. </summary>
	/// <param name="newTableID"> name of the new comparison table being compared. </param>
	private void setNewTableID(string newTableID)
	{
		__newTableID = newTableID;
	}

	/// <summary>
	/// Set the precision for floating point comparisons. </summary>
	/// <param name="precision"> the precision for floating point comparisons. </param>
	private void setPrecision(int? precision)
	{
		__precision = precision;
	}

	/// <summary>
	/// Set the first table being compared. </summary>
	/// <param name="table1"> first table being compared. </param>
	private void setTable1(DataTable table1)
	{
		__table1 = table1;
	}

	/// <summary>
	/// Set the second table being compared. </summary>
	/// <param name="table1"> second table being compared. </param>
	private void setTable2(DataTable table2)
	{
		__table2 = table2;
	}

	/// <summary>
	/// Set the tolerance for floating point comparisons. </summary>
	/// <param name="tolerance"> the tolerance for floating point comparisons. </param>
	private void setTolerance(double? tolerance)
	{
		__tolerance = tolerance;
	}

	// TODO SAM 2011-12-23 Enable colors that indicate amount of difference
	// Could be two colors (positive difference, negative difference) or shades based on degree of difference
	// (based on tolerance?).
	/// <summary>
	/// Write an HTML representation of the comparison table in which different cells are highlighted.
	/// This uses the generic DataTableHtmlWriter with a style mask for the different cells.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeHtmlFile(String htmlFile) throws Exception, java.io.IOException
	public virtual void writeHtmlFile(string htmlFile)
	{
		DataTableHtmlWriter tableWriter = new DataTableHtmlWriter(getComparisonTable());
		string[] styles = new string[] {"", "diff"};
		string customStyleText = ".diff { background-color:yellow; }\n";
		tableWriter.writeHtmlFile(htmlFile, true, null, getDifferenceArray(), styles, customStyleText);
	}

	}

}