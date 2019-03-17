using System;
using System.Collections.Generic;
using System.IO;

// NWSRFSLayer - class to read NWSRFS geo_data files

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

//-----------------------------------------------------------------------------
// NWSRFSLayer - class to read NWSRFS geo_data files
//-----------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//-----------------------------------------------------------------------------
// History:
//
// 2001-11-26	Steven A. Malers, RTi	Initial version to read select files.
// 2001-12-13	SAM, RTi		Add "swe_stations.dat" file, which is
//					the same format as the PCA stations
//					list.  Just use the same format rather
//					than trying to come up with something
//					new.
// 2002-10-28	SAM, RTi		Update to support Linux, which is
//					little endian.  If running on Linux, it
//					is assumed that the files were created
//					on Linux or another little endian
//					machine (maybe have a switch later)?
//					Fix so that lat/long coordinates in the
//					forecastpt.dat file are free format
//					(leading fields are fixed format).
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//-----------------------------------------------------------------------------

namespace RTi.GIS.GeoView
{

	using GRPoint = RTi.GR.GRPoint;
	using GRPolygon = RTi.GR.GRPolygon;
	using GRPolyline = RTi.GR.GRPolyline;
	using GRShape = RTi.GR.GRShape;
	using EndianDataInputStream = RTi.Util.IO.EndianDataInputStream;
	using IOUtil = RTi.Util.IO.IOUtil;
	using MathUtil = RTi.Util.Math.MathUtil;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using DataTable = RTi.Util.Table.DataTable;
	using TableField = RTi.Util.Table.TableField;
	using TableRecord = RTi.Util.Table.TableRecord;

	/// <summary>
	/// Class to store NWSRFS geo_data files, assumed to be ASCII or big-endian binary
	/// files (generated on UNIX).  The files are used by the IFP and other NWS
	/// applications.  The following files are read at this time.
	/// <pre>
	/// ascii/county.dat - county polylines
	/// ascii/fg_basin.dat - forecast group polygons
	/// ascii/forecastpt.dat - forecast points
	/// ascii/map_basin.dat - mean areal precipitation area polygons
	/// ascii/rfc_boundary.dat - single polygon for RFC boundary
	/// ascii/river.dat - river polylines
	/// ascii/state.dat - polygons for states
	/// ascii/swe_stations.dat - SWE stations, in PCA snow updating format
	/// 
	/// binary/county.bin - county polylines
	/// binary/fg_basin.dat - forecast group polygons
	/// binary/map_basin.bin - mean areal precipitation area polygons
	/// binary/rfc_boundary.bin - single polygon for RFC boundary
	/// binary/river.dat - river polylines
	/// binary/state.bin - polygons for states
	/// </pre>
	/// </summary>
	public class NwsrfsLayer : GeoLayer
	{

	/// <summary>
	/// Private flags to keep track of file type to minimize string checks.
	/// </summary>
	private readonly int COUNTY = 1;
	//private final int COOPS = 2;
	private readonly int FG_BASIN = 3;
	private readonly int FORECASTPT = 4;
	private readonly int MAP_BASIN = 5;
	//private final int NEXRAD = 6;
	private readonly int RFC_BOUNDARY = 7;
	private readonly int RIVER = 8;
	private readonly int STATE = 9;
	private readonly int SWE_STATIONS = 10;
	//private final int TOWN = 11;

	/// <summary>
	/// Construct a layer by reading the geo_data file.
	/// The file name should include the file extension. </summary>
	/// <param name="path"> File or URL path to a geo_data file. </param>
	/// <exception cref="IOException"> if there is an error reading the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public NwsrfsLayer(String path, boolean read_attributes) throws java.io.IOException
	public NwsrfsLayer(string path, bool read_attributes) : base(path)
	{
		initialize();
		setDataFormat("NWSRFS geo_data");
		try
		{
			read(path, read_attributes);
		}
		catch (IOException e)
		{
			Message.printWarning(2, "NwsrfsLayer", e);
			throw e;
		}
	}

	/// <summary>
	/// Finalize before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~NwsrfsLayer()
	{
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the data value for a shape.  The object will be a Double, or String.
	/// Use the DataTable methods to get field formats for output. </summary>
	/// <param name="index"> Database record for shape. </param>
	/// <param name="field"> Attribute table field to use for data. </param>
	/// <exception cref="Exception"> if there is an error getting the value. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object getShapeAttributeValue(long index, int field) throws Exception
	public override object getShapeAttributeValue(long index, int field)
	{ // Get the data from the attribute table in the base class...
		return getAttributeTable().getFieldValue(index, field);
	}

	/// <summary>
	/// Initialize the object.  For now, just set the file names.  It is assumed that
	/// _props has been set to a non-null PropList that contains the input name.
	/// </summary>
	private void initialize()
	{
	}

	/// <summary>
	/// Determine whether the file is an NWSRFS layer file.  Checks are made as follows:
	/// <ol>
	/// <li>	If the file cannot be opened, return false.</li>
	/// <li>	If the file can be opened and the name is one of the following
	/// return true: county.bin, county.dat, fg_basin.bin, fg_basin.dat,
	/// forecastpt.dat,
	/// map_basin.bin, map_basin.dat, rfc_boundary.bin, rfc_boundary.dat,
	/// river.bin, river.dat, state.bin, state.dat.</li>
	/// <li>	Else, return false.</li>
	/// </ol> </summary>
	/// <returns> true if the file is an ESRIShapefile, false if not. </returns>
	/// <param name="filename"> Name of file to check, with or without the .shp extension. </param>
	public static bool isNwsrfsFile(string filename)
	{
		File file = new File(filename);
		string name = file.getName();
		bool check = false;
		if (name.Equals("county.bin") || name.Equals("county.dat") || name.Equals("fg_basin.bin") || name.Equals("fg_basin.dat") || name.Equals("forecastpt.dat") || name.Equals("map_basin.bin") || name.Equals("map_basin.dat") || name.Equals("rfc_boundary.bin") || name.Equals("rfc_boundary.dat") || name.Equals("river.bin") || name.Equals("river.dat") || name.Equals("state.bin") || name.Equals("state.dat") || name.Equals("swe_stations.dat"))
		{
			check = true;
		}
		file = null;
		name = null;
		return check;
	}

	/// <summary>
	/// Read the file.  The attributes will be read if originally requested. </summary>
	/// <param name="geodata_file"> Name of geo_data file to read. </param>
	/// <param name="read_attributes"> Indicates if the attribute table should be assigned. </param>
	/// <exception cref="IOException"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void read(String geodata_file, boolean read_attributes) throws java.io.IOException
	private void read(string geodata_file, bool read_attributes)
	{
		string routine = "Nwsrfs.read";

		// Check the file names and convert to an integer flag to increase
		// performance in code below...

		int filetype = 0;
		bool binary = false;
		File file = new File(geodata_file);
		string filename = file.getName();
		if (filename.Equals("county.dat"))
		{
			filetype = COUNTY;
		}
		else if (filename.Equals("county.bin"))
		{
			filetype = COUNTY;
			binary = true;
		}
		else if (filename.Equals("fg_basin.dat"))
		{
			filetype = FG_BASIN;
		}
		else if (filename.Equals("fg_basin.bin"))
		{
			filetype = FG_BASIN;
			binary = true;
		}
		else if (filename.Equals("forecastpt.dat"))
		{
			filetype = FORECASTPT;
		}
		else if (filename.Equals("map_basin.dat"))
		{
			filetype = MAP_BASIN;
		}
		else if (filename.Equals("map_basin.bin"))
		{
			filetype = MAP_BASIN;
			binary = true;
		}
		else if (filename.Equals("rfc_boundary.dat"))
		{
			filetype = RFC_BOUNDARY;
		}
		else if (filename.Equals("rfc_boundary.bin"))
		{
			filetype = RFC_BOUNDARY;
			binary = true;
		}
		else if (filename.Equals("river.dat"))
		{
			filetype = RIVER;
		}
		else if (filename.Equals("river.bin"))
		{
			filetype = RIVER;
			binary = true;
		}
		else if (filename.Equals("state.dat"))
		{
			filetype = STATE;
		}
		else if (filename.Equals("state.bin"))
		{
			filetype = STATE;
			binary = true;
		}
		else if (filename.Equals("swe_stations.dat"))
		{
			filetype = SWE_STATIONS;
		}

		// For now, do this brute force, sharing formats as much as possible.

		double xmin = 1.0e10;
		double xmax = -1.0e10;
		double ymin = 1.0e10;
		double ymax = -1.0e10;
		double xmin_layer = 1.0e10;
		double xmax_layer = -1.0e10;
		double ymin_layer = 1.0e10;
		double ymax_layer = -1.0e10;
		string @string = null;
		double x, y = 0.0;
		IList<string> tokens = null;
		DataTable attributeTable = getAttributeTable();
		IList<GRShape> shapes = getShapes();
		if (((filetype == COUNTY) || (filetype == FG_BASIN) || (filetype == MAP_BASIN) || (filetype == RFC_BOUNDARY) || (filetype == RIVER) || (filetype == STATE)) && !binary)
		{
			// ASCII file containing lat/long coordinates
			// Format is ID Name Order Npts
			// Lat +Long
			// Lat +Long
			// ...
			//
			// For counties, it is -Long, lat (is this a standard???)
			//
			// For MAP layer, might want to use GRPolygonList and put
			// multiple polygons under one shape.  Or, add an attribute to
			// include the main MAP area or segment?  For now, read all the
			// areas in as separate GRPolygon.
			if ((filetype == RIVER) || (filetype == STATE))
			{
				setShapeType(LINE);
			}
			else
			{
				setShapeType(POLYGON);
			}
			StreamReader @in = null;
			@in = new StreamReader(IOUtil.getInputStream(geodata_file));
			if (read_attributes)
			{
				IList<TableField> table_fields = new List<TableField>(1);
				if (filetype == COUNTY)
				{
					table_fields.Add(new TableField(TableField.DATA_TYPE_STRING,"ID",24));
					table_fields.Add(new TableField(TableField.DATA_TYPE_STRING,"COUNTY",24));
				}
				else if (filetype == FG_BASIN)
				{
					table_fields.Add(new TableField(TableField.DATA_TYPE_STRING,"FG",24));
				}
				else if (filetype == MAP_BASIN)
				{
					table_fields.Add(new TableField(TableField.DATA_TYPE_STRING,"MAP Area",24));
					table_fields.Add(new TableField(TableField.DATA_TYPE_STRING,"MAP Name",24));
				}
				if (filetype == RFC_BOUNDARY)
				{
					table_fields.Add(new TableField(TableField.DATA_TYPE_STRING,"RFC",24));
				}
				else if (filetype == RIVER)
				{
					table_fields.Add(new TableField(TableField.DATA_TYPE_STRING,"REACH",24));
					table_fields.Add(new TableField(TableField.DATA_TYPE_STRING,"NAME",24));
					table_fields.Add(new TableField(TableField.DATA_TYPE_DOUBLE,"ORDER",4,0));
				}
				else if (filetype == STATE)
				{
					table_fields.Add(new TableField(TableField.DATA_TYPE_STRING,"Abbreviation",3));
					table_fields.Add(new TableField(TableField.DATA_TYPE_STRING,"Name",24));
				}
				setAttributeTable(attributeTable = new DataTable(table_fields));
			}
			// Header line...
			int polycount = 0;
			while (true)
			{
				xmin = 1.0e10;
				xmax = -1.0e10;
				ymin = 1.0e10;
				ymax = -1.0e10;
				@string = @in.ReadLine();
				if (string.ReferenceEquals(@string, null))
				{
					break;
				}
				tokens = StringUtil.breakStringList(@string.Trim(), " ", 0);
				int size = 0;
				if ((tokens != null) && (tokens.Count > 3))
				{
					if (read_attributes)
					{
						// Only attribute is the name of the basin boundary...
						TableRecord record = new TableRecord(1);
						record.addFieldValue(tokens[0]);
						if (filetype == RIVER)
						{
							record.addFieldValue(tokens[1]);
							record.addFieldValue(Convert.ToDouble(tokens[2]));
						}
						else if ((filetype == STATE) || (filetype == COUNTY) || (filetype == FG_BASIN) || (filetype == MAP_BASIN))
						{
							record.addFieldValue(tokens[1]);
						}
						try
						{
							attributeTable.addRecord(record);
						}
						catch (Exception e)
						{
							Message.printWarning(2, routine, e);
						}
					}
					size = StringUtil.atoi((string)tokens[3]);
					GRPolygon polygon = null;
					GRPolyline polyline = null;
					if ((filetype == RIVER) || (filetype == STATE) || (filetype == COUNTY))
					{
						polyline = new GRPolyline(size);
					}
					else
					{
						polygon = new GRPolygon(size);
					}
					int ptcount = 0;
					for (int i = 0; i < size; i++)
					{
						@string = @in.ReadLine();
						if (string.ReferenceEquals(@string, null))
						{
							break;
						}
						tokens = StringUtil.breakStringList(@string.Trim(), " ", 0);
						if ((tokens != null) && (tokens.Count == 2))
						{
							if (filetype == COUNTY)
							{
								x = StringUtil.atod(tokens[0]);
								y = StringUtil.atod(tokens[1]);
							}
							else
							{
								y = StringUtil.atod(tokens[0]);
								x = -(StringUtil.atod(tokens[1]));
							}
							if ((filetype == RIVER) || (filetype == STATE) || (filetype == COUNTY))
							{
								polyline.setPoint(ptcount++, new GRPoint(x,y));
							}
							else
							{
								polygon.setPoint(ptcount++, new GRPoint(x,y));
							}
							xmin = MathUtil.min(x, xmin);
							xmax = MathUtil.max(x, xmax);
							ymin = MathUtil.min(y, ymin);
							ymax = MathUtil.max(y, ymax);
						}
					}
					if ((filetype == RIVER) || (filetype == STATE) || (filetype == COUNTY))
					{
						polyline.xmin = xmin;
						polyline.ymin = ymin;
						polyline.xmax = xmax;
						polyline.ymax = ymax;
						polyline.limits_found = true;
						polyline.index = polycount++;
						shapes.Add(polyline);
					}
					else
					{
						polygon.xmin = xmin;
						polygon.ymin = ymin;
						polygon.xmax = xmax;
						polygon.ymax = ymax;
						polygon.limits_found = true;
						polygon.index = polycount++;
						shapes.Add(polygon);
					}
					xmin_layer = MathUtil.min(xmin, xmin_layer);
					xmax_layer = MathUtil.max(xmax, xmax_layer);
					ymin_layer = MathUtil.min(ymin, ymin_layer);
					ymax_layer = MathUtil.max(ymax, ymax_layer);
				}
			}
			@in.Close();
			setProjection(new GeographicProjection());
		}
		else if (((filetype == COUNTY) || (filetype == FG_BASIN) || (filetype == MAP_BASIN) || (filetype == RFC_BOUNDARY) || (filetype == RIVER) || (filetype == STATE)) && binary)
		{
			// Binary file containing HRAP coordinates.
			// Format is ID XXX -1 Npts
			// HRAP-X HRAP-Y
			// HRAP-X HRAP-Y
			// ...
			//
			// For MAP layer, might want to use GRPolygonList and put
			// multiple polygons under one shape.  Or, add an attribute to
			// include the main MAP area or segment?  For now, read all the
			// areas in as separate GRPolygon.
			if ((filetype == RIVER) || (filetype == STATE) || (filetype == COUNTY))
			{
				setShapeType(LINE);
			}
			else
			{
				setShapeType(POLYGON);
			}
			EndianDataInputStream @in = new EndianDataInputStream(IOUtil.getInputStream(geodata_file));
			bool is_big_endian = IOUtil.isBigEndianMachine();
			if (read_attributes)
			{
				IList<TableField> table_fields = new List<TableField> (1);
				if (filetype == COUNTY)
				{
					table_fields.Add(new TableField(TableField.DATA_TYPE_STRING,"ID",24));
					table_fields.Add(new TableField(TableField.DATA_TYPE_STRING,"COUNTY",24));
				}
				else if (filetype == FG_BASIN)
				{
					table_fields.Add(new TableField(TableField.DATA_TYPE_STRING,"FG",24));
				}
				else if (filetype == MAP_BASIN)
				{
					table_fields.Add(new TableField(TableField.DATA_TYPE_STRING,"MAP Area",24));
					table_fields.Add(new TableField(TableField.DATA_TYPE_STRING,"MAP Name",24));
				}
				else if (filetype == RFC_BOUNDARY)
				{
					table_fields.Add(new TableField(TableField.DATA_TYPE_STRING,"RFC",24));
				}
				else if (filetype == RIVER)
				{
					table_fields.Add(new TableField(TableField.DATA_TYPE_STRING,"REACH",24));
					table_fields.Add(new TableField(TableField.DATA_TYPE_STRING,"NAME",24));
					table_fields.Add(new TableField(TableField.DATA_TYPE_DOUBLE,"ORDER",4,0));
				}
				else if (filetype == STATE)
				{
					table_fields.Add(new TableField(TableField.DATA_TYPE_STRING,"Abbreviation",3));
					table_fields.Add(new TableField(TableField.DATA_TYPE_STRING,"Name",24));
				}
				setAttributeTable(attributeTable = new DataTable(table_fields));
			}
			// Header line...
			int polycount = 0;
			string id, name;
			int npts, order, size = 0;
			while (true)
			{
				xmin = 1.0e10;
				xmax = -1.0e10;
				ymin = 1.0e10;
				ymax = -1.0e10;
				try
				{
					// Read the same whether big or little endian because assume to be 1-byte chars...
					id = @in.readString1(9).Trim();
					name = @in.readString1(21).Trim();
					if (is_big_endian)
					{
						order = @in.readInt();
						npts = @in.readInt();
					}
					else
					{
						order = @in.readLittleEndianInt();
						npts = @in.readLittleEndianInt();
					}
				}
				catch (Exception)
				{
					// No more data...
					break;
				}
				if (read_attributes)
				{
					// Only attribute is the name of the basin boundary...
					TableRecord record = new TableRecord(1);
					record.addFieldValue(id);
					if (filetype == RIVER)
					{
						record.addFieldValue(name);
						record.addFieldValue(new double?(order));
					}
					else if ((filetype == STATE) || (filetype == COUNTY) || (filetype == FG_BASIN) || (filetype == MAP_BASIN))
					{
						record.addFieldValue(name);
					}
					try
					{
						attributeTable.addRecord(record);
					}
					catch (Exception)
					{
						// Ignore for now..
					}
				}
				size = npts;
				GRPolygon polygon = null;
				GRPolyline polyline = null;
				if ((filetype == RIVER) || (filetype == STATE) || (filetype == COUNTY))
				{
					polyline = new GRPolyline(size);
				}
				else
				{
					polygon = new GRPolygon(size);
				}
				int ptcount = 0;
				for (int i = 0; i < size; i++)
				{
					try
					{
						if (is_big_endian)
						{
							x = @in.readFloat();
							y = @in.readFloat();
						}
						else
						{
							x = @in.readLittleEndianFloat();
							y = @in.readLittleEndianFloat();
						}
		//if ( filetype == RIVER )
	//Message.printStatus ( 1, "", "x = " + x + " y = " + y );
					}
					catch (Exception)
					{
						Message.printWarning(3, "", "Error reading x, y");
						break;
					}
					if ((filetype == RIVER) || (filetype == STATE) || (filetype == COUNTY))
					{
						polyline.setPoint(ptcount++, new GRPoint(x,y));
					}
					else
					{
						polygon.setPoint(ptcount++, new GRPoint(x,y));
					}
					xmin = MathUtil.min(x, xmin);
					xmax = MathUtil.max(x, xmax);
					ymin = MathUtil.min(y, ymin);
					ymax = MathUtil.max(y, ymax);
				}
				if ((filetype == RIVER) || (filetype == STATE) || (filetype == COUNTY))
				{
					polyline.xmin = xmin;
					polyline.ymin = ymin;
					polyline.xmax = xmax;
					polyline.ymax = ymax;
					polyline.limits_found = true;
					polyline.index = polycount++;
					shapes.Add(polyline);
				}
				else
				{
					polygon.xmin = xmin;
					polygon.ymin = ymin;
					polygon.xmax = xmax;
					polygon.ymax = ymax;
					polygon.limits_found = true;
					polygon.index = polycount++;
					shapes.Add(polygon);
				}
				xmin_layer = MathUtil.min(xmin, xmin_layer);
				xmax_layer = MathUtil.max(xmax, xmax_layer);
				ymin_layer = MathUtil.min(ymin, ymin_layer);
				ymax_layer = MathUtil.max(ymax, ymax_layer);
			}
			@in.close();
			setProjection(new HRAPProjection());
		}
		else if (filetype == FORECASTPT)
		{
			// Format is Name State ID Lat Long
			// ...
			setShapeType(POINT);
			StreamReader @in = null;
			@in = new StreamReader(IOUtil.getInputStream(geodata_file));
			if (read_attributes)
			{
				// Only attribute is the name of the basin boundary...
				IList<TableField> table_fields = new List<TableField>(3);
				table_fields.Add(new TableField(TableField.DATA_TYPE_STRING,"Name",30));
				table_fields.Add(new TableField(TableField.DATA_TYPE_STRING,"State",3));
				table_fields.Add(new TableField(TableField.DATA_TYPE_STRING,"FP",12));
				setAttributeTable(attributeTable = new DataTable(table_fields));
			}
			GRShape point = null;
			int count = 0;
			while (true)
			{
				@string = @in.ReadLine();
				if (string.ReferenceEquals(@string, null))
				{
					break;
				}
				// Get the coordinates from the end of the string (after column 47).
				tokens = StringUtil.breakStringList(@string.Substring(47).Trim()," ", StringUtil.DELIM_SKIP_BLANKS);
				if ((tokens == null) || (tokens.Count != 2))
				{
					continue;
				}
				y = StringUtil.atod((string)tokens[0]);
				x = StringUtil.atod((string)tokens[1]);
				point = new GRPoint(-x, y);
				xmin = MathUtil.min(-x, xmin);
				xmax = MathUtil.max(-x, xmax);
				ymin = MathUtil.min(y, ymin);
				ymax = MathUtil.max(y, ymax);
				if (read_attributes)
				{
					// Get the attributes from the first part of the line...
					IList<object> oTokens = StringUtil.fixedRead(@string.Trim(),"s20s7s20");
					TableRecord record = new TableRecord(3);
					if ((tokens == null) || (oTokens.Count != 3))
					{
						record.addFieldValue("");
						record.addFieldValue("");
						record.addFieldValue("");
					}
					else
					{
						record.addFieldValue(((string)oTokens[0]).Trim());
						record.addFieldValue(((string)oTokens[1]).Trim());
						record.addFieldValue(((string)oTokens[2]).Trim());
					}
					try
					{
						attributeTable.addRecord(record);
					}
					catch (Exception)
					{
						// Ignore for now..
					}
				}
				point.index = count++;
				point.limits_found = true;
				shapes.Add(point);
			}
			xmin_layer = xmin;
			xmax_layer = xmax;
			ymin_layer = ymin;
			ymax_layer = ymax;
			setProjection(new GeographicProjection());
		}
		else if (filetype == SWE_STATIONS)
		{
			// Format is ID,Lat,-Long,ElevU,Base station,Name
			// ...
			setShapeType(POINT);
			StreamReader @in = null;
			@in = new StreamReader(IOUtil.getInputStream(geodata_file));
			GRShape point = null;
			int count = 0;
			string elev = null;
			string units = null;
			int len = 0;
			while (true)
			{
				@string = @in.ReadLine();
				if (string.ReferenceEquals(@string, null))
				{
					break;
				}
				@string = @string.Trim();
				if (@string.Length == 0)
				{
					continue;
				}
				if (@string[0] == '#')
				{
					continue;
				}
				tokens = StringUtil.breakStringList(@string,",",0);
				if ((tokens == null) || (tokens.Count != 6))
				{
					continue;
				}
				elev = tokens[3].Trim();
				// Remove the characters from the elevation...
				len = elev.Length;
				units = "";
				for (int i = 0; i < len; i++)
				{
					if (!char.IsDigit(elev[i]))
					{
						elev = elev.Substring(0,i);
						units = elev.Substring(i);
						break;
					}
				}
				if ((count == 0) && read_attributes)
				{
					// Need to know the units to be able to set the attribute correctly...
					IList<TableField> table_fields = new List<TableField> (4);
					table_fields.Add(new TableField(TableField.DATA_TYPE_STRING, "ID",10));
					if (units.Length > 0)
					{
						table_fields.Add(new TableField(TableField.DATA_TYPE_DOUBLE,"ELEV_" + units.ToUpper(),6,1));
					}
					else
					{
						table_fields.Add(new TableField(TableField.DATA_TYPE_DOUBLE,"ELEV",6,1));
					}
					table_fields.Add(new TableField(TableField.DATA_TYPE_STRING,"BASESTA",18));
					table_fields.Add(new TableField(TableField.DATA_TYPE_STRING,"NAME",30));
					setAttributeTable(attributeTable = new DataTable(table_fields));
				}
				y = StringUtil.atod(tokens[1]);
				x = StringUtil.atod(tokens[2]);
				point = new GRPoint(x, y);
				xmin = MathUtil.min(x, xmin);
				xmax = MathUtil.max(x, xmax);
				ymin = MathUtil.min(y, ymin);
				ymax = MathUtil.max(y, ymax);
				if (read_attributes)
				{
					TableRecord record = new TableRecord(3);
					record.addFieldValue(tokens[0].Trim());
					record.addFieldValue(new double?(StringUtil.atod(elev)));
					record.addFieldValue(tokens[4].Trim());
					record.addFieldValue(tokens[5].Trim());
					try
					{
						attributeTable.addRecord(record);
					}
					catch (Exception)
					{
						// Ignore for now..
					}
				}
				point.index = count++;
				point.limits_found = true;
				shapes.Add(point);
			}
			xmin_layer = xmin;
			xmax_layer = xmax;
			ymin_layer = ymin;
			ymax_layer = ymax;
			setProjection(new GeographicProjection());
		}

		// Set the limits for the layer data...

		setLimits(xmin_layer, ymin_layer, xmax_layer, ymax_layer);

		Message.printStatus(2, routine, "Read " + shapes.Count + " shapes from \"" + geodata_file + "\".");
		Message.printStatus(2, routine, "Defined " + attributeTable.getNumberOfRecords() + " attribute table records.");
	}

	}

}