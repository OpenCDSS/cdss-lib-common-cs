using System;
using System.Collections.Generic;

// CsvPointLayer - GeoLayer corresponding to CSV point data file

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

namespace RTi.GIS.GeoView
{

	using GRLimits = RTi.GR.GRLimits;
	using GRPoint = RTi.GR.GRPoint;
	using GRShape = RTi.GR.GRShape;
	using PropList = RTi.Util.IO.PropList;
	using MathUtil = RTi.Util.Math.MathUtil;
	using Message = RTi.Util.Message.Message;
	using DataTable = RTi.Util.Table.DataTable;

	/// <summary>
	/// This class reads, stores, and writes CSV files that have geographic data.  Comments are lines that
	/// start with # and the first row should have "quoted" column names.  Heading and data columns should
	/// be separated by commas, with strings enclosed in quotes if they contain commas
	/// </summary>
	public class CsvPointLayer : GeoLayer
	{

	/// <summary>
	/// Construct the layer and read the file. </summary>
	/// <param name="filename">	Name of the csv file. </param>
	/// <param name="xColumnName"> the name of the column containing the x-coordinate </param>
	/// <param name="yColumnName"> the name of the column containing the y-coordinate </param>
	/// <param name="projection"> the projection for the CSV file </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public CsvPointLayer(String filename, String xColumnName, String yColumnName, GeoProjection projection) throws java.io.IOException
	public CsvPointLayer(string filename, string xColumnName, string yColumnName, GeoProjection projection) : base(filename)
	{
		setDataFormat("CSV");
		setShapeType(POINT);
		setProjection(projection);
		try
		{
			read(filename, xColumnName, yColumnName);
		}
		catch (Exception e)
		{
			// Rethrow as IOException...
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			string routine = this.GetType().FullName + ".CsvPointLayer";
			Message.printWarning(2, routine, "Error reading CSV (" + e + ").");
			Message.printWarning(3, routine, e);
			throw new IOException(e);
		}
	}

	/// <summary>
	/// Indicate whether a file is an CSV file.  Currently, the check consists only of
	/// checking for ".csv" at the end of the filename. </summary>
	/// <param name="filename"> name of file to evaluate. </param>
	/// <returns> true if the file is CSV file, false if not. </returns>
	public static bool isCsvPointFile(string filename)
	{
		if (filename.ToUpper().EndsWith(".CSV", StringComparison.Ordinal))
		{
			return true;
		}
		else
		{
			return false;
		}
	}
	/// <summary>
	/// Read the CSV file. </summary>
	/// <exception cref="IOException"> if there is an error reading the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void read(String filename, String xColumnName, String yColumnName) throws java.io.IOException, Exception
	private void read(string filename, string xColumnName, string yColumnName)
	{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		string routine = this.GetType().FullName + ".read";
		// Just read the table into a data table, this will the the data table used for the layer.
		//DataTable table = DataTable.parseDelimitedFile ( filename, ",", null, 0, true, 0 );
		PropList props = new PropList("DataTable");
		props.set("Delimiter", ","); // Default
		props.set("ColumnDataTypes", "Auto"); // Figure out column types from data
		props.set("CommentLineIndicator=#"); // Skip comment lines
		props.set("TrimInput=True"); // Trim strings after reading.
		props.set("TrimStrings=True"); // Trim strings after parsing
		DataTable table = DataTable.parseFile(filename, props);
		setAttributeTable(table);
		// The table must have columns for X and Y
		int xColumnNumber = table.getFieldIndex(xColumnName);
		if (xColumnNumber < 0)
		{
			throw new IOException("Unable to find \"X\" column in table from CSV file \"" + filename + "\" - unable to create layer.");
		}
		int yColumnNumber = table.getFieldIndex(yColumnName);
		if (yColumnNumber < 0)
		{
			throw new IOException("Unable to find \"Y\" column in table from CSV file \"" + filename + "\" - unable to create layer.");
		}
		// Loop through the table records and create shapes.  The coordinates are taken from X and Y values.
		IList<GRShape> shapeList = getShapes();
		double? x, y;
		object xObject, yObject;
		double xmax = -1.0e10;
		double ymax = -1.0e10;
		double xmin = 1.0e10;
		double ymin = 1.0e10;
		GRPoint point;
		int validShapeCount = 0; // Must have coordinates to be valid
		for (int i = 0; i < table.getNumberOfRecords(); i++)
		{
			x = null;
			y = null;
			xObject = table.getFieldValue(i, xColumnNumber);
			yObject = table.getFieldValue(i, yColumnNumber);
			// Check for Integer below because table parser may determine a column is an Integer
			// if no decimals are included in data.
			if (xObject != null)
			{
				if (xObject is double?)
				{
					x = (double?)xObject;
				}
				else if (xObject is int?)
				{
					x = new double?((int?)xObject);
				}
				xmin = MathUtil.min(xmin,x);
				xmax = MathUtil.max(xmax,x);
			}
			if (yObject != null)
			{
				if (yObject is double?)
				{
					y = (double?)yObject;
				}
				else if (yObject is int?)
				{
					y = new double?((int?)yObject);
				}
				ymin = MathUtil.min(ymin,y);
				ymax = MathUtil.max(ymax,y);
			}
			if ((x == null) || x.isNaN() || (y == null) || y.isNaN())
			{
				// Empty point...
				shapeList.Add(null);
			}
			else
			{
				// Use the given values...
				point = new GRPoint(x.Value, y.Value);
				point.index = i;
				shapeList.Add(point);
				++validShapeCount;
			}
		}
		// Set the layer limits...
		if (validShapeCount > 0)
		{
			setLimits(new GRLimits(xmin,ymin,xmax,ymax));
		}
		Message.printStatus(2, routine, "Read " + shapeList.Count + " shapes from \"" + filename + "\", of which " + validShapeCount + " had valid X, Y.");
	}

	}

}