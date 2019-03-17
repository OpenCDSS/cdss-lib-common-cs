using System;
using System.Collections.Generic;

// BigPictureLayer - store data for big picture layer (multiple bar symbols)

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

//------------------------------------------------------------------------------
// BigPictureLayer - store data for big picture layer (multiple bar symbols)
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// Notes:	(1) Assumes first column is the id (String), second column is
//			the name (String), and remaining columns are doubles
//------------------------------------------------------------------------------
// History:
// 
// 29 Jun 1999	Catherine E.		Created initial version of class.
//		Nutting-Lane, RTi	
// 21 Sep 2001	Steven A. Malers, RTi	Change Table to DataTable.
// 2001-10-18	SAM, RTi		Review javadoc.  Add finalize().  Set
//					unused data to null.  Remove unused
//					classes in imports.
// 2002-06-20	SAM, RTi		Update to have a join field for the
//					related layers, which is used to relate
//					to other layers.  Previously the join
//					field was assumed to be the first field
//					(position 0) but this does not work with
//					shapefiles!
//------------------------------------------------------------------------------

namespace RTi.GIS.GeoView
{
	using DataTable = RTi.Util.Table.DataTable;
	using Message = RTi.Util.Message.Message;
	using GRLimits = RTi.GR.GRLimits;
	using GRShape = RTi.GR.GRShape;

	/// <summary>
	/// The BigPictureLayer is used with the StateMod GUI to display one or more bars
	/// associated with the StateMod delplt utility program output.  The functionality
	/// of this class will be migrated to the standard GeoView classes so that compound
	/// symbols can be more generally applied.
	/// </summary>
	public class BigPictureLayer : GeoLayer
	{

	private IList<GeoLayer> _parent_geolayers;
	private IList<string> _parent_join_fields;
	private DataTable _big_picture_table;
	private GRLimits _big_picture_limits;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="filename"> Comma-delimited file used to create the DataTable. </param>
	/// <param name="parent_geolayers"> Vector of GeoLayer being drawn.  These supply the
	/// spatial information that is matched against the attributes. </param>
	/// <param name="parent_join_fields"> Field names in the parent layers that contain the
	/// identifier field to join to. </param>
	/// <param name="bigPictureTable"> DataTable containing delimited data.  The first field
	/// is the identifier, the second the name, and 3+ contain data attributes that can be plotted. </param>
	public BigPictureLayer(string filename, IList<GeoLayer> parent_geolayers, IList<string> parent_join_fields, DataTable bigPictureTable) : base(filename)
	{
		initialize(parent_geolayers, parent_join_fields, bigPictureTable);
	}

	/// <summary>
	/// Calculates the max and min values for all data fields  (2...n)
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static RTi.GR.GRLimits computeBigPictureLimits(RTi.Util.Table.DataTable bigPictureTable) throws Exception
	public static GRLimits computeBigPictureLimits(DataTable bigPictureTable)
	{
		int num_records = bigPictureTable.getNumberOfRecords();
		int num_fields = bigPictureTable.getNumberOfFields();
		double ytmp;

		double ymin = ((double?)bigPictureTable.getFieldValue(0, 2)).Value;
		double ymax = ((double?)bigPictureTable.getFieldValue(0, 2)).Value;

		for (int i = 0; i < num_records; i++)
		{
			for (int j = 2; j < num_fields; j++)
			{
				ytmp = ((double?)bigPictureTable.getFieldValue(i, j)).Value;
				if (ytmp > ymax)
				{
					ymax = ytmp;
				}
				if (ytmp < ymin)
				{
					ymin = ytmp;
				}
			}
		}

		return new GRLimits(0, ymin, 1, ymax);
	}

	/// <summary>
	/// Clean up for garbage collection. </summary>
	/// <exception cref="Throwable"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~BigPictureLayer()
	{
		_parent_geolayers = null;
		_parent_join_fields = null;
		_big_picture_table = null;
		_big_picture_limits = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	public virtual DataTable getBigPictureTable()
	{
		return _big_picture_table;
	}

	private void initialize(IList<GeoLayer> parent_geolayers, IList<string> parent_join_fields, DataTable bigPictureTable)
	{
		_parent_geolayers = parent_geolayers;
		_parent_join_fields = parent_join_fields;
		_big_picture_table = bigPictureTable;
		setShapeType(BIG_PICTURE);
		try
		{
			GRLimits limits = parent_geolayers[0].getLimits();
			for (int i = 1; i < parent_geolayers.Count; i++)
			{
				limits = limits.max(parent_geolayers[i].getLimits());
			}
			setLimits(limits);
		}
		catch (Exception)
		{
			Message.printWarning(1, "BigPictureLayer", "Error computing drawing limits of big picture data.");
		}
		try
		{
			_big_picture_limits = computeBigPictureLimits(bigPictureTable);
		}
		catch (Exception)
		{
			Message.printWarning(1, "BigPictureLayer", "Problems computing limits of big picture data.");
		}
	}

	public virtual DataTable getAttributeTable(int index)
	{
		return _parent_geolayers[index].getAttributeTable();
	}

	public virtual GRLimits getBigPictureLimits()
	{
		return _big_picture_limits;
	}

	public virtual string getJoinField(int index)
	{
		return _parent_join_fields[index];
	}

	public virtual int getNumAssociatedLayers()
	{
		return _parent_geolayers.Count;
	}

	public virtual IList<GRShape> getShapes(int index)
	{
		return _parent_geolayers[index].getShapes();
	}

	}

}