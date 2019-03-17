﻿using System;
using System.Collections.Generic;

// TSUtil_LookupTimeSeriesFromTable - lookup time series values from a time series and lookup table.

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

namespace RTi.TS
{

	using DataTransformationType = RTi.Util.Math.DataTransformationType;
	using MathUtil = RTi.Util.Math.MathUtil;
	using Message = RTi.Util.Message.Message;
	using StringDictionary = RTi.Util.String.StringDictionary;
	using StringUtil = RTi.Util.String.StringUtil;
	using DataTable = RTi.Util.Table.DataTable;
	using LookupMethodType = RTi.Util.Table.LookupMethodType;
	using OutOfRangeLookupMethodType = RTi.Util.Table.OutOfRangeLookupMethodType;
	using TableRecord = RTi.Util.Table.TableRecord;
	using DateTime = RTi.Util.Time.DateTime;
	using DateTimeWindow = RTi.Util.Time.DateTimeWindow;

	/// <summary>
	/// Lookup time series values from a time series and lookup table.
	/// </summary>
	public class TSUtil_LookupTimeSeriesFromTable
	{

	/// <summary>
	/// List of problems generated by this command that are warnings, guaranteed to be non-null.
	/// </summary>
	private IList<string> __problemsWarning = new List<string>();

	/// <summary>
	/// List of problems generated by this command that are failures, guaranteed to be non-null.
	/// </summary>
	private IList<string> __problemsFailure = new List<string>();

	/// <summary>
	/// Date/time column for effective date 0+ (or -1 to indicate no effective date column).
	/// </summary>
	private int __effectiveDateColumn = -1;

	/// <summary>
	/// Input time series to process.
	/// </summary>
	private TS __inputTS = null;

	/// <summary>
	/// Data table for lookup.
	/// </summary>
	private DataTable __lookupTable = null;

	/// <summary>
	/// Indicate whether input needs to be sorted (performance hit but necessary in some cases).
	/// </summary>
	private bool __sortInput = false;

	/// <summary>
	/// Output time series to process.
	/// </summary>
	private TS __outputTS = null;

	/// <summary>
	/// Lookup method type.
	/// </summary>
	private LookupMethodType __lookupMethodType = null;

	/// <summary>
	/// Out of range lookup method type.
	/// </summary>
	private OutOfRangeLookupMethodType __outOfRangeLookupMethodType = null;

	// TODO SAM 2012-02-11 Need to make an enumeration to avoid errors, but this requires
	// some additional standardization throughout
	/// <summary>
	/// Out of range notification method ("Ignore", "Warn", "Fail").
	/// </summary>
	private string __outOfRangeNotification = "Ignore";

	/// <summary>
	/// Lookup table column matching the input time series values, 0+.
	/// </summary>
	private int __value1Column = -1;

	/// <summary>
	/// Lookup table column matching the output time series values, 0+.
	/// </summary>
	private int __value2Column = -1;

	/// <summary>
	/// Data transformation type.
	/// </summary>
	private DataTransformationType __transformation = null;

	/// <summary>
	/// Data value to use when the original data value is <= 0 and a log transformation is used.
	/// </summary>
	private double? __leZeroLogValue = .001;

	/// <summary>
	/// Start of analysis (null to analyze all from input time series).
	/// </summary>
	private DateTime __analysisStart = null;

	/// <summary>
	/// End of analysis (null to analyze all from input time series).
	/// </summary>
	private DateTime __analysisEnd = null;

	/// <summary>
	/// Window within the year to transfer data values.
	/// </summary>
	private DateTimeWindow __analysisWindow = null;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="inputTS"> input time series used for lookup </param>
	/// <param name="outputTS"> output time series used for lookup </param>
	/// <param name="lookupTable"> Data table being filled with time series.  Column names need to have been defined but the
	/// table is expected to be empty (no rows) </param>
	/// <param name="value1Column"> the column matching the input time series values for the lookup (0+) </param>
	/// <param name="sortInput"> indicate whether input column should be sorted (performance hit but necessary sometimes) </param>
	/// <param name="value2Column"> the column matching the output time series values for the lookup (0+) </param>
	/// <param name="effectiveDateColumn"> date/time column (0+) </param>
	/// <param name="lookupMethodType"> the lookup method type to use (if null, use INTERPOLATE) </param>
	/// <param name="outOfRangeLookupMethodType"> the lookup method for out of range values (if null use SET_MISSING) </param>
	/// <param name="outOfRangeNotification"> the type of notification when out of range values are estimated ("Ignore", "Warn",
	/// or "Fail"); if null use "Ignore" </param>
	/// <param name="transformation"> how to transform the data before the lookup (if null use NONE) </param>
	/// <param name="leZeroLogValue"> when using a log transformation,
	/// the value to use for data when the original data value is <=0 (if null use .001). </param>
	/// <param name="analysisStart"> first date/time to be transferred (if null, process input time series period) </param>
	/// <param name="analysisEnd"> last date/time to be transferred (if null, process input time series period) </param>
	/// <param name="analysisWindow"> window within a year to process (year generally set to 2000). </param>
	public TSUtil_LookupTimeSeriesFromTable(TS inputTS, TS outputTS, DataTable lookupTable, int value1Column, bool sortInput, int value2Column, int effectiveDateColumn, LookupMethodType lookupMethodType, OutOfRangeLookupMethodType outOfRangeLookupMethodType, string outOfRangeNotification, DataTransformationType transformation, double? leZeroLogValue, DateTime analysisStart, DateTime analysisEnd, DateTimeWindow analysisWindow)
	{ //String message;
		//String routine = getClass().getName() + ".constructor";
		// Save data members.
		if (inputTS == null)
		{
			throw new InvalidParameterException("Input time series is null.");
		}
		__inputTS = inputTS;
		if (outputTS == null)
		{
			throw new InvalidParameterException("Output time series is null.");
		}
		__outputTS = outputTS;
		if (lookupTable == null)
		{
			throw new InvalidParameterException("Lookup table is null.");
		}
		__lookupTable = lookupTable;
		if (lookupTable.getNumberOfRecords() < 2)
		{
			throw new InvalidParameterException("Lookup table must have at lest 2 rows.");
		}
		if ((value1Column < 0) || (value1Column >= lookupTable.getNumberOfFields()))
		{
			throw new InvalidParameterException("Value1 column (" + value1Column + ") is < 0 or < number of table columns - 1 (0+ indexm " + lookupTable.getNumberOfFields() + ").");
		}
		__value1Column = value1Column;
		if ((value2Column < 0) || (value2Column >= lookupTable.getNumberOfFields()))
		{
			throw new InvalidParameterException("Value2 column (" + value2Column + ") is < 0 or < number of table columns - 1 (0+ index, " + lookupTable.getNumberOfFields() + ").");
		}
		__sortInput = sortInput;
		__value2Column = value2Column;
		if (effectiveDateColumn >= 0)
		{
			throw new InvalidParameterException("Effective date column is not yet supported - future enhancement.");
		}
		__effectiveDateColumn = effectiveDateColumn;
		if (lookupMethodType == null)
		{
			lookupMethodType = LookupMethodType.INTERPOLATE;
		}
		__lookupMethodType = lookupMethodType;
		if (outOfRangeLookupMethodType == null)
		{
			outOfRangeLookupMethodType = OutOfRangeLookupMethodType.SET_MISSING;
		}
		__outOfRangeLookupMethodType = outOfRangeLookupMethodType;
		if (string.ReferenceEquals(outOfRangeNotification, null))
		{
			outOfRangeNotification = "Ignore";
		}
		__outOfRangeNotification = outOfRangeNotification;
		if (transformation == null)
		{
			transformation = DataTransformationType.NONE;
		}
		__transformation = transformation;
		if (leZeroLogValue == null)
		{
			leZeroLogValue = new double?(.001);
		}
		__leZeroLogValue = leZeroLogValue;
		__analysisStart = analysisStart;
		__analysisEnd = analysisEnd;
		__analysisWindow = analysisWindow;
		// Make sure that the time series are regular and of the same interval
		if (!TSUtil.intervalsMatch(inputTS, outputTS))
		{
			// TODO SAM 2012-02-10 Might be able to relax this constraint in the future
			throw new UnequalTimeIntervalException("Time series don't have the same interval - cannot perform lookup.");
		}
	}

	/// <summary>
	/// Determine whether the lookup table is sorted by the lookup value.
	/// Any nulls in data will result in a zero. </summary>
	/// <param name="lookupTable"> the lookup table </param>
	/// <param name="value1Column"> the column number for the lookup values (0+) </param>
	/// <returns> -1 if sorted descending (row 0=maximum), 1 (row 0=minimum), or zero (not sorted) </returns>
	private int checkLookupTableSortedAndNonNull(DataTable lookupTable, int value1Column)
	{
		double value, valuePrev;
		try
		{
			int countDescend = 0;
			int countAscend = 0;
			bool isNaN;
			valuePrev = getTableCellDouble(lookupTable, 0, value1Column);
			int nRows = lookupTable.getNumberOfRecords();
			for (int iRow = 1; iRow < nRows; iRow++)
			{
				value = getTableCellDouble(lookupTable,iRow, value1Column);
				isNaN = double.IsNaN(value);
				if (isNaN)
				{
					// TODO SAM 2014-01-21 evaluate what to do but for now just skip - won't be able to do lookup later unless endpoints are used
					++countDescend;
					++countAscend;
					//return 0;
				}
				if (value <= valuePrev)
				{
					++countDescend;
				}
				else if (value >= valuePrev)
				{
					++countAscend;
				}
				if (!isNaN)
				{
					valuePrev = value;
				}
			}
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","countAscend=" + countAscend + " countDescend=" + countDescend);
			}
			if ((countDescend + 1) == nRows)
			{
				return -1;
			}
			else if ((countAscend + 1) == nRows)
			{
				return 1;
			}
			else
			{
				return 0;
			}
		}
		catch (Exception)
		{
			// For getFieldValue exceptions
			return 0;
		}
	}

	// TODO SAM 2012-02-11 Need to handle the effective date and possibly shift
	/// <summary>
	/// Return the lookup table. </summary>
	/// <returns> the lookup table. </returns>
	private DataTable getLookupTable()
	{
		return __lookupTable;
	}

	/// <summary>
	/// Return the lookup table considering the effective date.
	/// The returned table will only have rows where the input and output values are not missing. </summary>
	/// <returns> the lookup table considering the effective date </returns>
	/// <param name="fullLookupTable"> the full lookup table, which may have multiple effective dates </param>
	/// <param name="lookupTablePrev"> previous lookup table, which may still be appropriate </param>
	/// <param name="date"> the date for which the lookup table is being retrieved </param>
	/// <param name="effectiveDateColumn"> the column in the full table that contains the effective date (0+) </param>
	private DataTable getLookupTableForEffectiveDate(DataTable fullLookupTable, int value1Column, int value2Column, DataTable lookupTablePrev, DateTime date, int effectiveDateColumn)
	{
		// For now always return the full lookup table since effective date is not supported
		// TODO SAM 2012-02-11 Need to enable effective date
		if (lookupTablePrev != null)
		{
			// Do this because for now we only need to do the processing below once.
			// Doing it each time step would introduce a performance hit.
			return lookupTablePrev;
		}
		// However, limit the lookup table to rows where the input and output columns are non-null
		string[] reqIncludeColumns = new string[2];
		reqIncludeColumns[0] = fullLookupTable.getFieldName(value1Column);
		reqIncludeColumns[1] = fullLookupTable.getFieldName(value2Column);
		string[] distinctColumns = null;
		Dictionary<string, string> columnMap = null;
		Dictionary<string, string> columnFilters = null;
		StringDictionary columnExcludeFilters = null;
		DataTable lookupTable = fullLookupTable.createCopy(fullLookupTable, "LookupTable", reqIncludeColumns, distinctColumns, columnMap, columnFilters, columnExcludeFilters);
		// Remove rows where the input or output values are missing.
		// Iterate from the end so that the index does not need to be adjusted
		TableRecord row;
		object o;
		for (int i = lookupTable.getNumberOfRecords() - 1; i >= 0; i--)
		{
			try
			{
				row = lookupTable.getRecord(i);
			}
			catch (Exception)
			{
				continue;
			}
			try
			{
				o = row.getFieldValue(0);
				if (o == null)
				{
					lookupTable.deleteRecord(i);
					continue;
				}
				if (o is double? && ((double?)o).isNaN())
				{
					lookupTable.deleteRecord(i);
					continue;
				}
			}
			catch (Exception)
			{
				continue;
			}
			try
			{
				o = row.getFieldValue(1);
				if (o == null)
				{
					lookupTable.deleteRecord(i);
					continue;
				}
				if (o is double? && ((double?)o).isNaN())
				{
					lookupTable.deleteRecord(i);
					continue;
				}
			}
			catch (Exception)
			{
				continue;
			}
		}
		return lookupTable;
	}

	/// <summary>
	/// Return a list of problems for the time series, failure messages. </summary>
	/// <returns> a list of problems for the time series, failure messages. </returns>
	public virtual IList<string> getProblemsFailure()
	{
		return __problemsFailure;
	}

	/// <summary>
	/// Return a list of problems for the time series, warning messages. </summary>
	/// <returns> a list of problems for the time series, warning messages. </returns>
	public virtual IList<string> getProblemsWarning()
	{
		return __problemsWarning;
	}

	/// <summary>
	/// Return the value of a lookup table cell as a double.
	/// This is needed because sometimes the lookup table column contains integers. </summary>
	/// <returns> the value of a lookup table cell as a double, or NaN if not a number. </returns>
	/// <param name="lookupTable"> the lookup table being processed </param>
	/// <param name="iRow"> the row being accessed </param>
	/// <param name="iColumn"> the column being accessed </param>
	private double getTableCellDouble(DataTable lookupTable, int iRow, int iColumn)
	{
		object o;
		try
		{
			o = lookupTable.getFieldValue(iRow, iColumn);
		}
		catch (Exception)
		{
			return Double.NaN;
		}
		if (o == null)
		{
			return Double.NaN;
		}
		else if (o is double?)
		{
			return (double?)o.Value;
		}
		else if (o is float?)
		{
			return (float?)o;
		}
		else if (o is int?)
		{
			return (double)(int?)o;
		}
		else if (o is short?)
		{
			return (double)(short?)o;
		}
		else
		{
			return Double.NaN;
		}
	}

	/// <summary>
	/// Lookup the last row for the input value that is less than or equal to the value being looked up.  This provides the lower bound. </summary>
	/// <param name="lookupTable"> the lookup table </param>
	/// <param name="lookupOrder"> the order of the lookup column (-1=descending, row 0 has max value; 1=ascending, row 0 has min value) </param>
	/// <param name="value1Column"> the column to use for the lookup (0+) </param>
	/// <param name="inputValue"> the value being looked up </param>
	private int lookupFloorRow(DataTable lookupTable, int lookupOrder, int value1Column, double inputValue)
	{
		double value;
		if (lookupOrder == 1)
		{
			// Ascending - start at last row and search up
			for (int iRow = lookupTable.getNumberOfRecords() - 1; iRow >= 0; iRow--)
			{
				try
				{
					value = getTableCellDouble(lookupTable, iRow, value1Column);
					if (double.IsNaN(value))
					{
						return -1;
					}
					if (value <= inputValue)
					{
						return iRow;
					}
				}
				catch (Exception)
				{
					return -1;
				}
			}
		}
		else if (lookupOrder == -1)
		{
			// Descending
			// Ascending - start at last row and search up
			for (int iRow = lookupTable.getNumberOfRecords() - 1; iRow >= 0; iRow--)
			{
				try
				{
					value = getTableCellDouble(lookupTable, iRow, value1Column);
					if (double.IsNaN(value))
					{
						return -1;
					}
					if (value <= inputValue)
					{
						return iRow;
					}
				}
				catch (Exception)
				{
					return -1;
				}
			}
		}
		return -1;
	}

	/// <summary>
	/// Set the output time series values by looking up from the time series and table.
	/// </summary>
	public virtual void lookupTimeSeriesValuesFromTable()
	{
		string message = "", routine = this.GetType().Name + ".lookupTimeSeriesValuesFromTable";
		// Create a new list of problems
		__problemsWarning = new List<string>();

		// If the output start and end are not specified, use the period from the input time series
		DateTime analysisStart = null, analysisEnd = null;
		if (__analysisStart == null)
		{
			analysisStart = new DateTime(__inputTS.getDate1());
		}
		else
		{
			analysisStart = new DateTime(__analysisStart);
		}
		if (__analysisEnd == null)
		{
			analysisEnd = new DateTime(__inputTS.getDate2());
		}
		else
		{
			analysisEnd = new DateTime(__analysisEnd);
		}
		DateTimeWindow analysisWindow = __analysisWindow;

		TS inputTS = __inputTS;
		TS outputTS = __outputTS;
		TSIterator tsi = null;
		try
		{
			tsi = inputTS.iterator(analysisStart, analysisEnd);
		}
		catch (Exception e)
		{
			// Should not happen
			throw new Exception("Error creating iterator (" + e + ").");
		}
		DateTime date = null;
		double inputValue, outputValue = 0.0;
		TSData tsdata;
		DataTable fullLookupTable = getLookupTable(); // TODO SAM 2012-02-11 Need to handle effectiveDate and return lookupOrder
		DataTable lookupTable = null; // Lookup table for the effective date
		DataTable lookupTablePrev = null; // The lookup table from the previous date, to optimize
		int lookupOrder = 0; // The order of the table lookup column
		LookupMethodType lookupMethodType = __lookupMethodType;
		OutOfRangeLookupMethodType outOfRangelookupMethodType = __outOfRangeLookupMethodType;
		string outOfRangeNotification = __outOfRangeNotification;
		bool outOfRangeNotifyWarn = false; // Default is to ignore out of range
		bool outOfRangeNotifyFail = false;
		if (outOfRangeNotification.ToUpper().IndexOf("WARN", StringComparison.Ordinal) >= 0)
		{
			outOfRangeNotifyWarn = true;
		}
		if (outOfRangeNotification.ToUpper().IndexOf("FAIL", StringComparison.Ordinal) >= 0)
		{
			outOfRangeNotifyFail = true;
		}
		int value1Column = __value1Column;
		bool sortInput = __sortInput;
		int value2Column = __value2Column;
		int effectiveDateColumn = __effectiveDateColumn;
		DataTransformationType transformation = __transformation;
		double leZeroLogValue = __leZeroLogValue.Value;
		IList<string> problemsWarning = __problemsWarning;
		IList<string> problemsFailure = __problemsFailure;
		double inputValueMin = 0;
		double inputValueMax = 0;
		double outputValueMin = 0;
		double outputValueMax = 0;
		double inputValue1 = 0, inputValue2 = 0, outputValue1 = 0, outputValue2 = 0;
		int lowRow = 0; // The lookupTable row that has a value <= to the input value
		int highRow = 0; // The lookupTable row that has a value >= the input value
		double missing = outputTS.getMissing();
		bool canSetOutput = false;
		int nRows = 0; // Number of rows in the lookup table
		int nRowsM1 = 0, nRowsM2 = 0;
		int setCount = 0;
		int lookupTableValue1Column = 0; // input column after extracting the specific lookup table
		int lookupTableValue2Column = 1; // output column after extracting the specific lookup table
		while ((tsdata = tsi.next()) != null)
		{
			date = tsi.getDate();
			if ((analysisWindow != null) && !analysisWindow.isDateTimeInWindow(date))
			{
				// Date is not in window so don't process the date...
				continue;
			}
			// Get the lookup table for the current date (might re-use the previous lookup)
			// In this table column 0 will be the input and column 1 will be the output
			lookupTable = getLookupTableForEffectiveDate(fullLookupTable, value1Column, value2Column, lookupTablePrev, date, effectiveDateColumn);
			if (lookupTable != lookupTablePrev)
			{
				if (sortInput)
				{
					lookupOrder = 1;
					lookupTable = sortTable(lookupTable, lookupTableValue1Column, lookupOrder);
				}
				else
				{
					lookupOrder = checkLookupTableSortedAndNonNull(lookupTable, lookupTableValue1Column);
				}
				if (lookupOrder == 0)
				{
					throw new Exception("Lookup table cannot be sorted.");
				}
				// Need to get some information about the table for further calculations
				nRows = lookupTable.getNumberOfRecords();
				if (Message.isDebugOn)
				{
					Message.printDebug(1,routine,"Lookup table is sorted in order " + lookupOrder + " and has " + nRows + " rows (missing data removed).");
				}
				nRowsM1 = nRows - 1;
				nRowsM2 = nRows - 2;
				try
				{
					if (lookupOrder == 1)
					{
						// Ascending
						inputValueMin = getTableCellDouble(lookupTable, 0, lookupTableValue1Column);
						inputValueMax = getTableCellDouble(lookupTable, nRowsM1, lookupTableValue1Column);
						outputValueMin = getTableCellDouble(lookupTable, 0, lookupTableValue2Column);
						outputValueMax = getTableCellDouble(lookupTable, nRowsM1, lookupTableValue2Column);
					}
					else
					{
						// Descending
						inputValueMin = getTableCellDouble(lookupTable, nRowsM1, lookupTableValue1Column);
						inputValueMax = getTableCellDouble(lookupTable, 0, lookupTableValue1Column);
						outputValueMin = getTableCellDouble(lookupTable, nRowsM1, lookupTableValue2Column);
						outputValueMax = getTableCellDouble(lookupTable, 0, lookupTableValue2Column);
					}
				}
				catch (Exception)
				{
					throw new Exception("Error looking up extreme values in lookup table.");
				}
			}
			// Save here in case there is a jump in logic
			lookupTablePrev = lookupTable;
			inputValue = tsdata.getDataValue();
			canSetOutput = true; // Set to false below if not able to compute output value
			if (inputTS.isDataMissing(inputValue))
			{
				// Can't process value
				canSetOutput = false;
			}
			else
			{
				// Have an input value to look up.
				// Some of this code is inlined - it is easier to understand the logic this way than having
				// complicated "if" statements or calling other methods to perform basic processing
				if (inputValue < inputValueMin)
				{
					if (Message.isDebugOn)
					{
						Message.printDebug(1,routine,"Input value " + inputValue + " is less than minimum input value " + inputValueMin);
					}
					if (outOfRangelookupMethodType == OutOfRangeLookupMethodType.SET_MISSING)
					{
						outputValue = missing;
					}
					else if (outOfRangelookupMethodType == OutOfRangeLookupMethodType.USE_END_VALUE)
					{
						outputValue = outputValueMin;
					}
					else if (outOfRangelookupMethodType == OutOfRangeLookupMethodType.EXTRAPOLATE)
					{
						try
						{
							if (lookupOrder == 1)
							{
								// Ascending so smallest values in row 0 - point the extrapolation past the min
								inputValue1 = getTableCellDouble(lookupTable, 1, lookupTableValue1Column);
								inputValue2 = getTableCellDouble(lookupTable, 0, lookupTableValue1Column);
								outputValue1 = getTableCellDouble(lookupTable, 1, lookupTableValue2Column);
								outputValue2 = getTableCellDouble(lookupTable, 0, lookupTableValue2Column);
							}
							else
							{
								// Descending so smallest values at end of table - point the extrapolation past the max
								inputValue1 = getTableCellDouble(lookupTable, nRowsM2, lookupTableValue1Column);
								inputValue2 = getTableCellDouble(lookupTable, nRowsM1, lookupTableValue1Column);
								outputValue1 = getTableCellDouble(lookupTable, nRowsM2, lookupTableValue2Column);
								outputValue2 = getTableCellDouble(lookupTable, nRowsM1, lookupTableValue2Column);
							}
						}
						catch (Exception)
						{
							// Should not happen
							problemsWarning.Add("Error looking up values from table for " + date + " value=" + inputValue);
							canSetOutput = false;
						}
						if (inputTS.isDataMissing(inputValue1) || inputTS.isDataMissing(inputValue2) || outputTS.isDataMissing(outputValue1) || outputTS.isDataMissing(outputValue2))
						{
							canSetOutput = false;
						}
						else
						{
							if (transformation == DataTransformationType.LOG)
							{
								if (inputValue <= 0)
								{
									inputValue = leZeroLogValue;
								}
								inputValue = Math.Log10(inputValue);
								if (inputValue1 <= 0)
								{
									inputValue1 = leZeroLogValue;
								}
								inputValue1 = Math.Log10(inputValue1);
								if (inputValue2 <= 0)
								{
									inputValue2 = leZeroLogValue;
								}
								inputValue2 = Math.Log10(inputValue2);
								if (outputValue1 <= 0)
								{
									outputValue1 = leZeroLogValue;
								}
								outputValue1 = Math.Log10(outputValue1);
								if (outputValue2 <= 0)
								{
									outputValue2 = leZeroLogValue;
								}
								outputValue2 = Math.Log10(outputValue2);
							}
							outputValue = MathUtil.interpolate(inputValue, inputValue1, inputValue2, outputValue1, outputValue2);
							if (transformation == DataTransformationType.LOG)
							{
								// Convert back to normal value
								outputValue = Math.Pow(10.0, outputValue);
							}
						}
					}
					// Check the notification, only when output is actually computed
					if (canSetOutput)
					{
						message = "Lookup value " + StringUtil.formatString(inputValue, "%.6f") +
						" for " + date + " is less than minimum lookup table value " +
						StringUtil.formatString(inputValueMin, "%.6f") + " - setting output to " + StringUtil.formatString(outputValue, "%.6f");
						if (outOfRangeNotifyWarn)
						{
							problemsWarning.Add(message);
						}
						else if (outOfRangeNotifyFail)
						{
							problemsFailure.Add(message);
						}
					}
				}
				else if (inputValue > inputValueMax)
				{
					if (Message.isDebugOn)
					{
						Message.printDebug(1,routine,"Input value " + inputValue + " is greater than maximum input value " + inputValueMax);
					}
					if (outOfRangelookupMethodType == OutOfRangeLookupMethodType.SET_MISSING)
					{
						outputValue = missing;
					}
					else if (outOfRangelookupMethodType == OutOfRangeLookupMethodType.USE_END_VALUE)
					{
						outputValue = outputValueMax;
					}
					else if (outOfRangelookupMethodType == OutOfRangeLookupMethodType.EXTRAPOLATE)
					{
						try
						{
							if (lookupOrder == 1)
							{
								// Ascending, max value at end of table, point extrapolation past end
								inputValue1 = getTableCellDouble(lookupTable, nRows - 2, lookupTableValue1Column);
								inputValue2 = getTableCellDouble(lookupTable, nRows - 1, lookupTableValue1Column);
								outputValue1 = getTableCellDouble(lookupTable, nRows - 2, lookupTableValue2Column);
								outputValue2 = getTableCellDouble(lookupTable, nRows - 1, lookupTableValue2Column);
							}
							else
							{
								// Descending, max value in row 0, point extrapolation past row 0
								inputValue1 = getTableCellDouble(lookupTable, 1, lookupTableValue1Column);
								inputValue2 = getTableCellDouble(lookupTable, 0, lookupTableValue1Column);
								outputValue1 = getTableCellDouble(lookupTable, 1, lookupTableValue2Column);
								outputValue2 = getTableCellDouble(lookupTable, 0, lookupTableValue2Column);
							}
						}
						catch (Exception)
						{
							// Should not happen
							problemsWarning.Add("Error looking up values from table for " + date + " value=" + inputValue);
							canSetOutput = false;
						}
						if (inputTS.isDataMissing(inputValue1) || inputTS.isDataMissing(inputValue2) || outputTS.isDataMissing(outputValue1) || outputTS.isDataMissing(outputValue2))
						{
							canSetOutput = false;
						}
						else
						{
							if (transformation == DataTransformationType.LOG)
							{
								if (inputValue <= 0)
								{
									inputValue = leZeroLogValue;
								}
								inputValue = Math.Log10(inputValue);
								if (inputValue1 <= 0)
								{
									inputValue1 = leZeroLogValue;
								}
								inputValue1 = Math.Log10(inputValue1);
								if (inputValue2 <= 0)
								{
									inputValue2 = leZeroLogValue;
								}
								inputValue2 = Math.Log10(inputValue2);
								if (outputValue1 <= 0)
								{
									outputValue1 = leZeroLogValue;
								}
								outputValue1 = Math.Log10(outputValue1);
								if (outputValue2 <= 0)
								{
									outputValue2 = leZeroLogValue;
								}
								outputValue2 = Math.Log10(outputValue2);
							}
							outputValue = MathUtil.interpolate(inputValue, inputValue1, inputValue2, outputValue1, outputValue2);
							if (transformation == DataTransformationType.LOG)
							{
								// Convert back to normal value
								outputValue = Math.Pow(10.0, outputValue);
							}
						}
					}
					// Check the notification, only when output is actually computed
					if (canSetOutput)
					{
						if (outOfRangeNotifyWarn || outOfRangeNotifyFail)
						{
							message = "Lookup value " + StringUtil.formatString(inputValue, "%.6f") +
							" for " + date + " is greater than maximum lookup table value " +
							StringUtil.formatString(inputValueMax, "%.6f") + " - setting output to " + StringUtil.formatString(outputValue, "%.6f");
						}
						if (outOfRangeNotifyWarn)
						{
							problemsWarning.Add(message);
						}
						else if (outOfRangeNotifyFail)
						{
							problemsFailure.Add(message);
						}
					}
				}
				else
				{
					// In the range of values so find the value to interpolate
					// Need to interpolate or otherwise look up.
					// Get the row where the value is less than or equal to the input value
					lowRow = lookupFloorRow(lookupTable, lookupOrder, lookupTableValue1Column, inputValue);
					if (lookupOrder == 1)
					{
						// Ascending
						highRow = lowRow + 1;
					}
					else
					{
						// Descending
						highRow = lowRow - 1;
					}
					try
					{
						inputValue1 = getTableCellDouble(lookupTable, lowRow, lookupTableValue1Column);
						inputValue2 = getTableCellDouble(lookupTable, highRow, lookupTableValue1Column);
						outputValue1 = getTableCellDouble(lookupTable, lowRow, lookupTableValue2Column);
						outputValue2 = getTableCellDouble(lookupTable, highRow, lookupTableValue2Column);
					}
					catch (Exception)
					{
						// Should not happen
						problemsWarning.Add("Error looking up values from table for " + date + " value=" + inputValue);
						canSetOutput = false;
					}
					if (Message.isDebugOn)
					{
						Message.printDebug(2,routine,"Value is in lookup table range lowRow=" + lowRow + " highRow=" + highRow + " inputValue1=" + inputValue1 + " inputValue2=" + inputValue2 + " outputValue1=" + outputValue1 + " outputValue2=" + outputValue2);
					}
					if (inputValue == inputValue1)
					{
						// Value is exactly on a lookup table value.
						outputValue = outputValue1;
					}
					else if (inputValue == inputValue2)
					{
						// Value is exactly on a lookup table value.
						outputValue = outputValue2;
					}
					else
					{
						// Need to do some type of estimate
						if (((lookupMethodType == LookupMethodType.PREVIOUS_VALUE) || (lookupMethodType == LookupMethodType.INTERPOLATE)) && (inputTS.isDataMissing(inputValue1) || inputTS.isDataMissing(inputValue2)))
						{
							canSetOutput = false;
						}
						if (((lookupMethodType == LookupMethodType.NEXT_VALUE) || (lookupMethodType == LookupMethodType.INTERPOLATE)) && (outputTS.isDataMissing(outputValue1) || outputTS.isDataMissing(outputValue2)))
						{
							canSetOutput = false;
						}
						if (!canSetOutput)
						{
							// Nothing to do
						}
						if (inputValue == inputValue1)
						{
							// Value is exactly on a lookup table value.  Need to handle special
							// Regardless of the lookup method, set to the output value
							outputValue = outputValue1;
						}
						if (lookupMethodType == LookupMethodType.INTERPOLATE)
						{
							if (transformation == DataTransformationType.LOG)
							{
								if (inputValue <= 0)
								{
									inputValue = leZeroLogValue;
								}
								inputValue = Math.Log10(inputValue);
								if (inputValue1 <= 0)
								{
									inputValue1 = leZeroLogValue;
								}
								inputValue1 = Math.Log10(inputValue1);
								if (inputValue2 <= 0)
								{
									inputValue2 = leZeroLogValue;
								}
								inputValue2 = Math.Log10(inputValue2);
								if (outputValue1 <= 0)
								{
									outputValue1 = leZeroLogValue;
								}
								outputValue1 = Math.Log10(outputValue1);
								if (outputValue2 <= 0)
								{
									outputValue2 = leZeroLogValue;
								}
								outputValue2 = Math.Log10(outputValue2);
							}
							outputValue = MathUtil.interpolate(inputValue, inputValue1, inputValue2, outputValue1, outputValue2);
							if (transformation == DataTransformationType.LOG)
							{
								// Convert back to normal value
								outputValue = Math.Pow(10.0, outputValue);
							}
						}
						else if (lookupMethodType == LookupMethodType.PREVIOUS_VALUE)
						{
							outputValue = outputValue1;
						}
						else if (lookupMethodType == LookupMethodType.NEXT_VALUE)
						{
							outputValue = outputValue2;
						}
					}
				}
			}
			if (canSetOutput)
			{
				++setCount;
				if (Message.isDebugOn)
				{
					Message.printDebug(1,routine,"Looked up output value " + outputValue + " from input value " + inputValue + " for " + date);
				}
				outputTS.setDataValue(date, outputValue);
			}
			else
			{
				// Can't compute output.  However, set the output to missing
				++setCount;
				if (Message.isDebugOn)
				{
					Message.printDebug(1,routine,"Unable to look up output value from input value " + inputValue + " for " + date + ", setting output to missing");
				}
				outputTS.setDataValue(date, missing);
			}
		}
		outputTS.addToGenesis("Set " + setCount + " values from lookup table \"" + lookupTable.getTableID() + "\" and input time series \"" + inputTS.getIdentifierString() + "\" in period " + analysisStart + " to " + analysisEnd);
	}

	/// <summary>
	/// Sort the lookup table by the input column.
	/// </summary>
	private DataTable sortTable(DataTable table, int sortCol, int sortOrder)
	{
		// Do not want to sort the original table.  Consequently copy the table and then sort
		DataTable tableSorted = table.createCopy(table, table.getTableID() + "-sorted", null, null, null, null, null);
		// Sort the table
		string[] sortCols = new string[1];
		sortCols[0] = table.getFieldName(sortCol);
		int[] sortOrderArray = new int[1];
		sortOrderArray[0] = sortOrder;
		tableSorted.sortTable(sortCols,sortOrderArray);
		return tableSorted;
	}

	}

}