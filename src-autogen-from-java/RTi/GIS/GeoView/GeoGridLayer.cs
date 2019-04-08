using System;
using System.Collections.Generic;

// GeoGridLayer - GeoLayer to store grid data

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

// -----------------------------------------------------------------------------
// GeoGridLayer - GeoLayer to store grid data
// -----------------------------------------------------------------------------
// Copyright: See the COPYRIGHT file
// -----------------------------------------------------------------------------
// History:
//
// 2001-10-02	Steven A. Malers, RTi	Create this class to handle generic
//					output of shapefiles.  Most other
//					functionality is in GeoLayer, GeoGrid,
//					GRGrid, and derived classes like
//					XmrgGridLayer.
// 2001-10-08	SAM, RTi		Review javadoc.
// 2002-12-19	SAM, RTi		Add setDataValue().
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// -----------------------------------------------------------------------------

namespace RTi.GIS.GeoView
{

	using GRShape = RTi.GR.GRShape;
	using DataTable = RTi.Util.Table.DataTable;
	using TableField = RTi.Util.Table.TableField;
	using TableRecord = RTi.Util.Table.TableRecord;

	/// <summary>
	/// The GeoGridLayer class extends GeoLayer and stores GeoGrid data using a Vector
	/// of GRGrid.  Although it is possible that a Vector of GRGrid could be saved,
	/// currently only a single GRGrid shape is typically stored in the shape list (e.g.,
	/// for use by XmrgGridLayer).  This class implements methods that can be used for
	/// any grid-based layer, such as saving the cells with > 0 data values as a shapefile.
	/// </summary>
	public class GeoGridLayer : GeoLayer
	{

	/// <summary>
	/// Grid to hold the data.
	/// </summary>
	private GeoGrid __grid = null;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="filename"> Name of layer file. </param>
	public GeoGridLayer(string filename) : base(filename)
	{
	}

	/// <summary>
	/// Clean up for garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~GeoGridLayer()
	{
		__grid = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Get the data value for a column and row.  This method should be defined in
	/// a derived class to take advantage of on-the-fly reading.  If not defined and
	/// accessed in a derived class, the GeoGrid.getDataValue() method is called. </summary>
	/// <param name="column"> Column of cell to read data for. </param>
	/// <param name="row"> Row of cell to read data for. </param>
	/// <exception cref="IOException"> if there is an error reading the data. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double getDataValue(int column, int row) throws java.io.IOException
	public virtual double getDataValue(int column, int row)
	{
		return __grid.getDataValue(column, row);
	}

	/// <summary>
	/// Returns the grid containing the data. </summary>
	/// <returns> the grid containing the data. </returns>
	public virtual GeoGrid getGrid()
	{
		return __grid;
	}

	/// <summary>
	/// Set the data value for a column and row.  This method should be defined in
	/// a derived class to take advantage of on-the-fly writing.  If not defined and
	/// accessed in a derived class, the GeoGrid.setDataValue() method is called. </summary>
	/// <param name="column"> Column of cell to read data for. </param>
	/// <param name="row"> Row of cell to read data for. </param>
	/// <param name="value"> Value to set for the row and cell. </param>
	/// <exception cref="IOException"> if there is an error setting the data value. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setDataValue(int column, int row, double value) throws java.io.IOException
	public virtual void setDataValue(int column, int row, double value)
	{
		__grid.setDataValue(column, row, value);
	}

	/// <summary>
	/// Set the grid containing the data. </summary>
	/// <returns> the grid containing the data. </returns>
	public virtual void setGrid(GeoGrid grid)
	{
		__grid = grid;
	}

	/// <summary>
	/// Write an ESRIShapefile.  This method exists in this class to allow a shapefile
	/// to be created for any GeoGridLayer.  The actual writing of the files occurs in
	/// ESRIShapefile but the packaging of the necessary data occurs in this class.
	/// Minimum and maximum data values can be specified to allow a range of cells to be written. </summary>
	/// <param name="filename"> Name of shapefile to write (with or without .shp). </param>
	/// <param name="to_projection"> Projection that data should be written. </param>
	/// <param name="use_data_limits"> If true, then the min_data_value and max_data_value
	/// values are checked.  Only cells with data in the limits are output. </param>
	/// <param name="min_data_value"> Minimum cell data value to consider when writing. </param>
	/// <param name="max_data_value"> Maximum cell data value to consider when writing. </param>
	/// <exception cref="IOException"> if there is an error writing the shapefile. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeShapefile(String filename, GeoProjection to_projection, boolean use_data_limits, double min_data_value, double max_data_value) throws java.io.IOException
	public override void writeShapefile(string filename, GeoProjection to_projection, bool use_data_limits, double min_data_value, double max_data_value)
	{ // Create the DataTable from the grid.  Note that GeoLayer (the base
		// class) has an _attribute_table object.  However, at this time it
		// is probably ok to just create one when we need rather than try to
		// carry around.  This may change if we allow the attribute table to
		// be viewed in a GUI, etc.

		IList<TableField> fields = new List<TableField> (3);
		fields.Add(new TableField(TableField.DATA_TYPE_INT, "COLUMN", 10, 0));
		fields.Add(new TableField(TableField.DATA_TYPE_INT, "ROW", 10, 0));
		fields.Add(new TableField(TableField.DATA_TYPE_DOUBLE, "VALUE", 10, 4));
		DataTable table = new DataTable(fields);
		fields = null;

		// Now create the polygons to write to the shapefile.  In the future it
		// might be possible to let the ESRIShapefile class know more about
		// the GRID shape but for now create a list of GRPolygon that can
		// be written in ESRIShapefile.

		IList<GRShape> shapes = new List<GRShape>(); // could optimize more
		int c = 0;
		double value = 0.0;
		TableRecord record = null;
		int min_row = __grid.getMinRow();
		int min_column = __grid.getMinColumn();
		int max_row = __grid.getMaxRow();
		int max_column = __grid.getMaxColumn();
		for (int r = min_row; r <= max_row; r++)
		{
			for (c = min_column; c <= max_column; c++)
			{
				try
				{
					value = getDataValue(c, r);
				}
				catch (Exception)
				{
					continue;
				}
				if (use_data_limits && ((value < min_data_value) || (value > max_data_value)))
				{
					continue;
				}
				// Create the shape...
				shapes.Add(__grid.getCellPolygon(c, r));
				// Create the attribute record...
				record = new TableRecord(3);
				record.addFieldValue(new int?(c));
				record.addFieldValue(new int?(r));
				record.addFieldValue(new double?(value));
				try
				{
					table.addRecord(record);
				}
				catch (Exception)
				{
					// Should never happen.
				}
			}
		}

		// Write the shapefile.  The "from" projection is just this layer's projection...

		ESRIShapefile.write(filename, table, shapes, getProjection(), to_projection);
	}

	}

}