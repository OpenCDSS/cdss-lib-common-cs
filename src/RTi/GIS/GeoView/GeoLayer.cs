using System;
using System.Collections.Generic;

// GeoLayer - class to hold a geographic layer

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

// ----------------------------------------------------------------------------
// GeoLayer - class to hold a geographic layer
// ----------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
// ----------------------------------------------------------------------------
// History:
//
// 27 Aug 1996	Steven A. Malers	Initial version.
//		Riverside Technology,
//		inc.
// 05 Mar 1997	Matthew J. Rutherford,	Added functions and members for use in
//		RTi			linked list, and getFile() routine.
// 18 Apr 1997	MJR, RTi		Added _entity_name, and _entity_type
//					data members also added the prototypes
//					for the functions used. Also added a
//					flag to draw attributes or not.
// 09 Jul 1997	Jay J. Fucetola		Converted to Java.
// 14 Jun 1999	SAM, RTi		Revisit code and clean up.
//					Change name from GeoCoverage to GeoLayer
//					to be more consistent with GIS
//					conventions.
// 28 Jun 1999	Catherine E.		Added attribute table.
//		Nutting-Lane, RTi
// 07 Jul 1999	SAM, RTi		Add _user_type for to allow a broad
//					classification of the layer to a user
//					type.  Add _props so that information
//					can be easily associated with the layer.
//					Add getShape and getRecord for index
//					lookups.
// 01 Sep 1999	SAM, RTi		Add reindex() to reset the index numbers
//					on shapes.
// 09 Nov 1999	CEN, RTi		Fixed computeLimits.  limits_found was
//					never set to true and once true, no 
//					comparison was made between temporary
//					limits and shape limits.
// 01 Aug 2001	SAM, RTi		Add removeAllAssociations() to set all
//					shape.associated_object values to null.
//					This is used when items in a layer have
//					been previously associated via a join
//					and now a new join is being done.
//					Change "user type" to "AppLayerType" to
//					be consistent with current
//					GeoViewProject conventions.
// 10 Aug 2001	SAM, RTi		Change so the class is not abstract.
//					This allows a general GeoLayer to be
//					created.
// 17 Sep 2001	SAM, RTi		Add the GRID layer type.  Change from
//					Table to DataTable.
// 25 Sep 2001	SAM, RTi		Add getShapeAttributeValue() to allow
//					lookup of an attribute.  This works
//					even if the attibutes were never read.
// 27 Sep 2001	SAM, RTi		Fill out the readLayer() method to
//					generically handle shapefile and Xmrg
//					files.  Add projection as a layer
//					property.
// 04 Oct 2001	SAM, RTi		Add deselectAllShapes().
// 					Add hasSelectedShapes() to help optimize
//					code when setting colors for display.
// 08 Oct 2001	SAM, RTi		Add getDataFormat().
// 2001-10-17	SAM, RTi		Add call to System.gc() in readLayer().
// 2001-11-26	SAM, RTi		Add ability to read NWSRFS geographic
//					data in NWSRFSLayer.  Add project()
//					method to be used to project after
//					data are read (called in
//					GeoViewProject).
// 2001-12-07	SAM, RTi		Add _selected_count to keep track of
//					how many shapes are selected.  This
//					is used by getNumSelected().
//					The _has_selected_shapes data and
//					hasSelectedShapes() have been
//					eliminated.  This has become necessary
//					because shape selections can now be
//					appended and reversed in the
//					GeoViewCanvas with the Ctrl-key.
//					Add constructor with just a PropList -
//					this allows a layer of shapes to be
//					dynamically created in memory.
// 2002-07-27	SAM, RTi		Add setAttributeTable().
// 2002-09-09	SAM, RTi		Add setShapesVisible().
// 2002-09-24	SAM, RTi		Add getAttributeMax() and
//					getAttributeMin().
// 2004-08-03	JTS, RTi		* Added setAttributeTableValue().
//					* Added getAttributeTableRowCount().
//					* Added getAttributeTableFieldCount().
// 2004-08-09	JTS, RTi		* Added getShapePrecisionValue().
//					* Added getShapeWidthValue().
// 2004-10-27	JTS, RTi		Implements Cloneable.
// 2005-04-27	JTS, RTi		Added all data members to finalize().
// 2006-06-15	SAM, RTi		Add setShapes().
// ----------------------------------------------------------------------------
// EndHeader

namespace RTi.GIS.GeoView
{

	using GRLimits = RTi.GR.GRLimits;
	using GRShape = RTi.GR.GRShape;
	using IOUtil = RTi.Util.IO.IOUtil;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;
	using MathUtil = RTi.Util.Math.MathUtil;
	using StringUtil = RTi.Util.String.StringUtil;
	using DataTable = RTi.Util.Table.DataTable;
	using TableRecord = RTi.Util.Table.TableRecord;

	// TODO SAM 2010-12-23 Need to convert GeoLayer to an interface and have implementations of the interface

	/// <summary>
	/// This class holds a layer of geographically-referenced data, which can be point,
	/// polygon, image, etc.  GeoLayers are associated with visible attributes using
	/// a GeoLayerView.  The GeoLayer therefore contains mainly raw shape data as Vector
	/// of GRShape and shape attributes as a DataTable.  The GeoView class displays the layer views.
	/// This base class can be extended for the different layer formats (e.g., ESRI
	/// shapefiles) and the input/output code for these formats should be in the
	/// derived code.  The benefit of extending from GeoLayer is that layers can be
	/// treated similarly by higher-level code.  An example is that a layer's attributes
	/// can be retrieved using the getShapeAttributeValue() method.  This method may
	/// take values from memory or may cause a binary file read (often the case due to
	/// the large amount of data in GIS files).  The shape data are usually read into
	/// memory for fast redraws and region queries.  In the case of grid layers,
	/// attributes are stored in the GeoGrid object (rather than an attribute table)
	/// and special care can be taken.  The overall class hierarchy is shown below:
	/// <pre>
	///   GeoViewPanel (has GeoView, GeoLayerLegend, etc.) - high level GUI components
	/// 
	///         GeoView (has Vector of GeoLayerView) - GR library Canvas device
	/// 
	///               GeoLayerView (has a GeoLayer, GRLegend) - organize view of data
	/// 
	///                       GRLegend (has a GRSymbol) - legend information
	/// 
	///                       GeoLayer (Vector of GRShape, has DataTable) - raw data
	///                          |                            
	///                          |                            
	///                          |                              GRShape - shape data
	///                          |                                 |
	///                          |                               GRPoint--GRGrid--etc
	///                          |                                          |
	///                          |                                          |
	///                          |                                       GeoGrid 
	///                          |
	///                          |------------------GeoGridLayer (Shapes are GeoGrid)
	///                          |                       |
	///                          |                       |
	///                     ESRIShapefile          XmrgGridLayer
	///               (shapes are GRPoint,etc.)
	/// </pre>
	/// Common formats can also be added to the readLayer() method.
	/// If constructing a GeoLayer in memory:
	/// <ol>
	/// <li>	create the GeoLayer</li>
	/// <li>	call setShapeType() to set the proper shape type (1 shape type per GeoLayer)</li>
	/// <li>	call getShapes() to get the shape Vector</li>
	/// <li>	add the proper GRShape objects to the Vector</li>
	/// <li>	use the GeoLayer as needed (call computeLimits() to recompute the
	/// limits of the data, if necessary - note for ESRIShapefiles it is
	/// possible for the overall layer to have valid limits but individual
	/// shapes to have invalid limits like zeros)</li>
	/// </ol>
	/// </summary>
	public class GeoLayer : ICloneable
	{

	// TODO SAM 2010-12-29 implement open standard types and map ESRI to them
	/// <summary>
	/// Define layer types.  Use ESRI shapefile types where there is overlap.
	/// </summary>

	/// <summary>
	/// Unknown layer type (can occur in ESRI shapefile).
	/// </summary>
	public const int UNKNOWN = 0;

	/// <summary>
	/// Point (site) layer, stored in GRPoint (can occur in ESRI shapefile).
	/// </summary>
	public const int POINT = 1;

	/// <summary>
	/// Arc (polyline) layer, stored in GRPolylineList (can occur in ESRI shapefile).
	/// </summary>
	public const int ARC = 3;
	public const int LINE = ARC;

	/// <summary>
	/// Polygon layer, stored in GRPolygonList (can occur in ESRI shapefile).
	/// </summary>
	public const int POLYGON = 5;

	/// <summary>
	/// Multipoint layer, stored as GRPolypoint (can occur in ESRI shapefile).
	/// </summary>
	public const int MULTIPOINT = 8; // Or just use 1?

	/// <summary>
	/// Point with Z and M, as per shapefile type 11.
	/// </summary>
	public const int POINT_ZM = 11;

	/// <summary>
	/// PolylineZM list.
	/// </summary>
	public const int POLYLINE_ZM = 13;

	/// <summary>
	/// Big picture layer consisting of a layer with shape data and an additional
	/// table with attributes for additional analysis.  This is a special layer for
	/// displaying complex symbols at locations.  <b>This layer type will be phased out
	/// at some point in favor of more generic symbology (do not port to C++).</b>
	/// </summary>
	public const int BIG_PICTURE = 50;

	/// <summary>
	/// Grid data, stored as GRGrid (not ESRI grid).
	/// </summary>
	public const int GRID = 51;

	/// <summary>
	/// Type of shapes in layer (e.g., POINT).
	/// </summary>
	private int __shapeType = UNKNOWN;

	/// <summary>
	/// List of shape data.  Note that the index in the shape is used to
	/// cross-reference to the attribute table.  The index starts at 0, unlike the ESRI
	/// shapefiles, where the record numbers start at 1.
	/// </summary>
	private IList<GRShape> __shapes = new List<GRShape>();

	/// <summary>
	/// Overall limits of the layer (this can be reset using computeLimits() or may be
	/// set in the I/O code for a specific layer file type).
	/// </summary>
	private GRLimits __limits;

	/// <summary>
	/// Name of file for layer (currently layers are not constructed from database
	/// query - if so, the meaning of this data value may need to be modified).
	/// </summary>
	private string __fileName;

	/// <summary>
	/// Count of the number of selected shapes.  This should be updated whenever a
	/// shapes _is_selected data member is modified.  The selected shapes may or may
	/// not be visible.  This data member must be updated if the shape data is edited
	/// (e.g., if shapes are selected are removed, update the selected count).
	/// </summary>
	private int __selectedCount;

	/// <summary>
	/// Application layer type for layer.  This is used to allow an application to
	/// somewhat generically associate layers with functionality.  For example, an
	/// AppLayerType property in the GeoViewProject file may be set to "Streamflow".  An
	/// application can then know that when a user is interacting with streamflow data
	/// that the Streamflow data layer should be highlighted in the view.
	/// </summary>
	private string __appLayerType;

	/// <summary>
	/// Data format (e.g., "ESRI Shapefile") - this is a descriptive label but is not compared for any logic.
	/// </summary>
	private string __dataFormat = "";

	/// <summary>
	/// Property list for layer properties.  This is not used much at this time (need
	/// to document each recognized property).
	/// </summary>
	private PropList __props;

	/// <summary>
	/// Projection for the layer (see classes extended from GeoProjection).
	/// </summary>
	private GeoProjection __projection = null;

	/// <summary>
	/// Table to store attribute information (id, location, etc.).  This may be a
	/// derived class like DbaseDataTable due to the special requirements of a layer file format.
	/// </summary>
	private DataTable __attributeTable;

	/// <summary>
	/// Construct a layer and initialize to defaults. </summary>
	/// <param name="props"> Properties for the layer (currently none are recognized). </param>
	public GeoLayer(PropList props)
	{
		initialize(null, props);
	}

	/// <summary>
	/// Construct a layer and initialize to defaults (derived class should construct
	/// from a file).  An empty PropList is created. </summary>
	/// <param name="filename"> File that is being read. </param>
	public GeoLayer(string filename)
	{
		initialize(filename, null);
	}

	/// <summary>
	/// Construct a layer and initialize to defaults (derived class should construct from a file). </summary>
	/// <param name="filename"> File that is being read. </param>
	/// <param name="props"> Properties for the layer (currently none are recognized). </param>
	public GeoLayer(string filename, PropList props)
	{
		initialize(filename, props);
	}

	/// <summary>
	/// Clones the object. </summary>
	/// <returns> a clone of the object. </returns>
	public virtual object clone()
	{
		GeoLayer l = null;
		try
		{
			l = (GeoLayer)base.clone();
		}
		catch (Exception)
		{
			return null;
		}

		if (__shapes != null)
		{
			int size = __shapes.Count;
			GRShape shape = null;
			l.__shapes = new List<GRShape>(size);
			for (int i = 0; i < size; i++)
			{
				shape = __shapes[i];
				l.__shapes.Add((GRShape)shape.clone());
			}
		}

		if (__limits != null)
		{
			l.__limits = (GRLimits)__limits.clone();
		}

		if (__props != null)
		{
			l.__props = new PropList(__props);
		}

		if (__projection != null)
		{
			l.__projection = (GeoProjection)__projection.clone();
		}

		if (__attributeTable != null)
		{
			l.__attributeTable = DataTable.duplicateDataTable(__attributeTable, true);
		}

		return l;
	}

	/// <summary>
	/// Compute the spatial limits of the layer.  Use getLimits() to retrieve the limits. </summary>
	/// <param name="include_invisible"> Indicate that invisible shapes should be considered in the limits computation. </param>
	/// <exception cref="Exception"> if the limits cannot be computed (e.g., all null data, all missing, etc.). </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void computeLimits(boolean include_invisible) throws Exception
	public virtual void computeLimits(bool include_invisible)
	{ // Loop through the shapes and get the overall limits...
		bool limits_found = false;
		int size = __shapes.Count;
		GRShape shape = null;
		double xmin = 0.0, xmax = 0.0, ymin = 0.0, ymax = 0.0;
		for (int i = 0; i < size; i++)
		{
			shape = __shapes[i];
			if (shape == null)
			{
				continue;
			}
			if (!shape.limits_found)
			{
				// Don't try to compute the shapes limits.  Just skip...
				continue;
			}
			if (!include_invisible && !shape.is_visible)
			{
				// Don't want invisible shapes so skip...
				continue;
			}
			// Limits for the shape are found and shape is to be considered...
			if (!limits_found)
			{
				// Initialize...
				xmin = shape.xmin;
				ymin = shape.ymin;
				xmax = shape.xmax;
				ymax = shape.ymax;
				limits_found = true;
			}
			else
			{
				if (shape.xmin < xmin)
				{
					xmin = shape.xmin;
				}
				if (shape.ymin < ymin)
				{
					ymin = shape.ymin;
				}
				if (shape.xmax > xmax)
				{
					xmax = shape.xmax;
				}
				if (shape.ymax > ymax)
				{
					ymax = shape.ymax;
				}
			}
		}
		// Now return...
		if (!limits_found)
		{
			throw new Exception("Cannot find GeoLayer limits");
		}
		__limits = new GRLimits(xmin, ymin, xmax, ymax);
	}

	/// <summary>
	/// Deselect all the shapes in a layer.  This is useful, for example, when all
	/// shapes need to be deselected before a pending select operation.
	/// </summary>
	public virtual void deselectAllShapes()
	{
		GRShape shape = null;
		int size = __shapes.Count;
		for (int i = 0; i < size; i++)
		{
			shape = __shapes[i];
			if (shape == null)
			{
				continue;
			}
			if (shape.is_selected)
			{
				--__selectedCount;
			}
			shape.is_selected = false;
		}
	}

	/// <summary>
	/// Finalize before garbage collection. </summary>
	/// <exception cref="Throwable"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~GeoLayer()
	{
		__fileName = null;
		__shapes = null;
		__limits = null;
		__appLayerType = null;
		__dataFormat = null;
		__attributeTable = null;
		__props = null;
		__projection = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Compute the maximum value for a numeric attribute. </summary>
	/// <param name="field"> index of attribute table field to check (field must be a numeric field). </param>
	/// <param name="include_invisible"> If true all shapes will be considered.  If false, only
	/// visible shapes will be considered. </param>
	/// <returns> the maximum value for a numeric attribute or zero if there is an error. </returns>
	public virtual double getAttributeMax(int field, bool include_invisible)
	{
		int size = 0;
		if (__shapes != null)
		{
			size = __shapes.Count;
		}
		if (size == 0)
		{
			return 0.0;
		}
		GRShape shape = __shapes[0];
		double max = 0.0;
		try
		{
			max = StringUtil.atod(getShapeAttributeValue(shape.index, field).ToString());
			for (int i = 1; i < size; i++)
			{
				shape = __shapes[i];
				if (shape == null)
				{
					continue;
				}
				if (!include_invisible && !shape.is_visible)
				{
					// Don't want invisible shapes so skip...
					continue;
				}
				max = MathUtil.max(max, StringUtil.atod(getShapeAttributeValue(shape.index, field).ToString()));
			}
		}
		catch (Exception)
		{
			return 0.0;
		}
		return max;
	}

	/// <summary>
	/// Compute the minimum value for a numeric attribute. </summary>
	/// <param name="field"> index of attribute table field to check (field must be a numeric field). </param>
	/// <param name="include_invisible"> If true all shapes will be considered.  If false, only
	/// visible shapes will be considered. </param>
	/// <returns> the minimum value for a numeric attribute or zero if there is an error. </returns>
	public virtual double getAttributeMin(int field, bool include_invisible)
	{
		int size = 0;
		if (__shapes != null)
		{
			size = __shapes.Count;
		}
		if (size == 0)
		{
			return 0.0;
		}
		GRShape shape = __shapes[0];
		double min = 0.0;
		try
		{
			min = StringUtil.atod(getShapeAttributeValue(shape.index, field).ToString());
			for (int i = 1; i < size; i++)
			{
				shape = __shapes[i];
				if (shape == null)
				{
					continue;
				}
				if (!include_invisible && !shape.is_visible)
				{
					// Don't want invisible shapes so skip...
					continue;
				}
				min = MathUtil.min(min, StringUtil.atod(getShapeAttributeValue(shape.index, field).ToString()));
			}
		}
		catch (Exception)
		{
			return 0.0;
		}
		return min;
	}

	/// <summary>
	/// Return the application type set with setAppLayerType(). </summary>
	/// <returns> the application type for the layer. </returns>
	public virtual string getAppLayerType()
	{
		return __appLayerType;
	}

	/// <summary>
	/// Return the attribute table associated with the shapes.  Depending on the parameters set during
	/// the layer read/creation, this table may contain a header only or header and data records. </summary>
	/// <returns> Layer attribute table. </returns>
	public virtual DataTable getAttributeTable()
	{
		return __attributeTable;
	}

	/// <summary>
	/// Returns the number of fields in the attribute table.  If there is no attribute table, 0 is returned. </summary>
	/// <returns> the number of fields in the attribute table. </returns>
	public virtual int getAttributeTableFieldCount()
	{
		if (__attributeTable == null)
		{
			return 0;
		}
		return __attributeTable.getNumberOfFields();
	}

	/// <summary>
	/// Returns the number of rows in the attribute table.  If there is no attribute table, 0 is returned. </summary>
	/// <returns> the number of rows in the attribute table. </returns>
	public virtual int getAttributeTableRowCount()
	{
		if (__attributeTable == null)
		{
			return 0;
		}
		return __attributeTable.getNumberOfRecords();
	}

	/// <summary>
	/// Return the layer data format. </summary>
	/// <returns> the layer data format. </returns>
	public virtual string getDataFormat()
	{
		return __dataFormat;
	}

	/// <summary>
	/// Return the source file name for the layer. </summary>
	/// <returns> the source file for the layer. </returns>
	public virtual string getFileName()
	{
		return __fileName;
	}

	/// <summary>
	/// Return the limits of the layer (in the original data units). </summary>
	/// <returns> the limits of the layer. </returns>
	public virtual GRLimits getLimits()
	{
		return __limits;
	}

	/// <summary>
	/// Return the number of selected shapes. </summary>
	/// <returns> the number of selected shapes. </returns>
	public virtual int getNumSelected()
	{
		return __selectedCount;
	}

	/// <summary>
	/// Return the projection for the data.  This projection can be compared to the
	/// GeoView's projection to determine whether a conversion is necessary. </summary>
	/// <returns> Projection used for the layer. </returns>
	public virtual GeoProjection getProjection()
	{
		return __projection;
	}

	/// <summary>
	/// Return a property for the layer. </summary>
	/// <returns> the String value of a property for the layer.  This calls PropList.getValue(). </returns>
	public virtual string getPropValue(string key)
	{
		return __props.getValue(key);
	}

	/// <summary>
	/// Return the shape at a specific index. </summary>
	/// <returns> the shape for the layer, given the index (0-reference).
	/// Return null if the index is out of bounds. </returns>
	public virtual GRShape getShape(int index)
	{
		if ((index < 0) || (index > (__shapes.Count - 1)))
		{
			return null;
		}
		return __shapes[(int)index];
	}

	/// <summary>
	/// Return the data value for a shape.  This method should be overruled in derived
	/// classes because the I/O for the attribute information is different for different
	/// layer types and on-the-fly reads may be needed.  If a general GeoLayer is
	/// used (e.g., for in-memory manipulation) then this code will be called and will
	/// return the value if an attribute table is available. </summary>
	/// <returns> a feature attribute value as an object.  The calling code should check
	/// the attribute table's field data types to know how to cast the returned value. </returns>
	/// <param name="index"> Database record for shape (zero-based). </param>
	/// <param name="field"> Attribute table field to use for data (zero-based index). </param>
	/// <exception cref="Exception"> if an error occurs getting the value (e.g., error reading from the source file). </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object getShapeAttributeValue(long index, int field) throws Exception
	public virtual object getShapeAttributeValue(long index, int field)
	{
		if (__attributeTable != null)
		{
			return __attributeTable.getFieldValue(index, field);
		}
		else
		{
			return null;
		}
	}

	/// <summary>
	/// Returns the precision for the given attribute table field. </summary>
	/// <param name="index"> the database record </param>
	/// <param name="field"> the field for which to get the precision </param>
	/// <returns> the precision. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int getShapePrecisionValue(long index, int field) throws Exception
	public virtual int getShapePrecisionValue(long index, int field)
	{
		if (__attributeTable != null)
		{
			return __attributeTable.getFieldPrecision(field);
		}
		else
		{
			return 0;
		}
	}

	/// <summary>
	/// Returns the width for the given attribute table field. </summary>
	/// <param name="index"> the database record </param>
	/// <param name="field"> the field for which to get the width </param>
	/// <returns> the width. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int getShapeWidthValue(long index, int field) throws Exception
	public virtual int getShapeWidthValue(long index, int field)
	{
		if (__attributeTable != null)
		{
			return __attributeTable.getFieldWidth(field);
		}
		else
		{
			return 0;
		}
	}

	/// <summary>
	/// Return the list of shapes used in the layer.  This list can be added to
	/// externally when reading the shapes from a file. </summary>
	/// <returns> the list of shapes used by this layer. </returns>
	public virtual IList<GRShape> getShapes()
	{
		return __shapes;
	}

	/// <summary>
	/// Return the shape type defined in this class (e.g., POINT). </summary>
	/// <returns> the shape type. </returns>
	public virtual int getShapeType()
	{
		return __shapeType;
	}

	/// <summary>
	/// Return a table record for a requested index.  This method should be defined in
	/// derived classes, especially if on-the-fly data reads will occur. </summary>
	/// <param name="index"> index (0-reference). </param>
	/// <returns> the table record for the layer, given the record index.
	/// Return null if the index is out of bounds. </returns>
	public virtual TableRecord getTableRecord(int index)
	{
		if (__attributeTable == null)
		{
			return null;
		}
		if ((index < 0) || (index > (__attributeTable.getNumberOfRecords() - 1)))
		{
			return null;
		}
		try
		{
			return __attributeTable.getRecord(index);
		}
		catch (Exception e)
		{
			// Not sure why this would happen...
			string routine = "GeoLayer.getTableRecord";
			Message.printWarning(10, routine, "Unable to get attribute table record [" + index + "]");
			Message.printWarning(10, routine, e);
			return null;
		}
	}

	/// <summary>
	/// Initialize data.
	/// </summary>
	private void initialize(string filename, PropList props)
	{
		setFileName(filename);
		__limits = null;
		// Always assign some shapes so we don't have to check for null all the time...
		__shapes = new List<GRShape>();
		__shapeType = UNKNOWN;
		__attributeTable = null;
		__appLayerType = "";
		if (props == null)
		{
			// Construct a PropList using the filename as the name...
			__props = new PropList(filename);
		}
		else
		{
			// Use the properties that were passed in...
			__props = props;
		}
	}

	/// <summary>
	/// Indicate whether the layer data source is available, for example that the filename exists and is the correct
	/// format.  If the source does not exist, the layer is therefore empty and should typically be displayed, but may be
	/// shown with a special indicator and have actions (like "Browse to connect to data").
	/// </summary>
	public virtual bool isSourceAvailable()
	{
		// Currently all layers are file based so check to see if the file exists
		// TODO SAM 2009-07-02 Need to make this more sophisticated to check for format, etc.
		File file = new File(getFileName());
		if (file.exists())
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Project a layer, resulting in the raw data changing.  Note that if the data are
	/// saved, the projection will be different and some configuration files may need
	/// to be changed.  The projection is accomplished by calling
	/// GeoProjection.projectShape() for each shape in the layer.  The overall limits are also changed. </summary>
	/// <param name="projection"> to change to. </param>
	public virtual void project(GeoProjection projection)
	{
		if (!GeoProjection.needToProject(__projection, projection))
		{
			// No need to do anything...
			//Message.printStatus ( 1, "", "No need to project " +
			//_projection.getProjectionName() + " to " + projection.getProjectionName() );
			return;
		}
		// Loop through all the shapes and project them...
		int size = __shapes.Count;
		for (int i = 0; i < size; i++)
		{
			GeoProjection.projectShape(__projection, projection, __shapes[i], true);
		}
		// Now reset the limits...
		try
		{
			computeLimits(true);
		}
		catch (Exception)
		{
			// Should not matter.
		}
		// Now set the projection to the requested...
		setProjection(projection);
	}

	/// <summary>
	/// Read a recognized layer type, returning a GeoLayer object (that can be cast to
	/// the specific type if necessary).  This is a utility method to simplify reading
	/// GIS data.  The file type is determined by calling each file's is*() method
	/// (e.g., ESRIShapefile.isESRIShapefile()). </summary>
	/// <param name="filename"> Name of layer file to read. </param>
	/// <param name="props"> Properties to use during reading.  Currently the only one
	/// recognized is "ReadAttributes", which indicates whether shapefile attributes
	/// should be read (if not, attributes will be read on the fly).  The properties
	/// are passed directly to the layer type's read method (e.g., its constructor). </param>
	/// <returns> the GeoLayer read from the file, or null if an error. </returns>
	/// <exception cref="IOException"> if there is an error reading the layer. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static GeoLayer readLayer(String filename, RTi.Util.IO.PropList props) throws java.io.IOException
	public static GeoLayer readLayer(string filename, PropList props)
	{
		string routine = "GeoLayer.readLayer";
		if (ESRIShapefile.isESRIShapefile(filename))
		{
			Message.printStatus(2, routine, "Reading ESRI shapefile \"" + filename + "\"...");
			// Do this first because the filename for xmrg, etc. may match
			// the other criteria but still be a shapefile.
			PropList props2 = new PropList("ESRIShapefile");
			props2.set("InputName", filename);
			string propValue = props.getValue("ReadAttributes");
			if (!string.ReferenceEquals(propValue, null))
			{
				props2.set("ReadAttributes", propValue);
			}
			GeoLayer layer = new ESRIShapefile(props2);
			return layer;
		}
		else if (CsvPointLayer.isCsvPointFile(filename))
		{
			Message.printStatus(2, routine, "Reading CSV \"" + filename + "\"...");
			// Read the entire layer with attributes...
			string propValue = props.getValue("Projection");
			GeoProjection projection = null;
			if (!string.ReferenceEquals(propValue, null))
			{
				try
				{
					projection = GeoProjection.parseProjection(propValue);
				}
				catch (Exception e)
				{
					throw new IOException("Error parsing projection \"" + propValue + "\" uniquetempvar.");
				}
			}
			return new CsvPointLayer(filename, "X", "Y", projection);
		}
		else if (XmrgGridLayer.isXmrg(filename))
		{
			Message.printStatus(2, routine, "Reading XMRG grid \"" + filename + "\"...");
			// For now read the entire grid and then close the file...
			return new XmrgGridLayer(filename, true, false);
			// Test reading on the fly (this worked)...
			//return new XmrgGridLayer ( filename, false, false );
		}
		else if (NwsrfsLayer.isNwsrfsFile(filename))
		{
			Message.printStatus(2, routine, "Reading NWSRFS file \"" + filename + "\"...");
			// Read the entire layer with attributes...
			return new NwsrfsLayer(filename, true);
		}
		if (IOUtil.fileReadable(filename))
		{
			throw new IOException("Unrecognized layer format for file \"" + filename + "\"");
		}
		else
		{
			throw new IOException("File is not readable: \"" + filename + "\"");
		}
	}

	/// <summary>
	/// Refresh the layer.  This should normally be done periodically when editing
	/// data layers. The following actions occur:
	/// <ol>
	/// <li>	The select count is reset to match the total of selected shapes in the shape list.</li>
	/// <li>	The limits are recomputed.</li>
	/// </ol>
	/// This method may be updated in the future to help synchronize in-memory data with files (e.g., when editing).
	/// </summary>
	public virtual void refresh()
	{
		int size = __shapes.Count;
		GRShape shape;
		__selectedCount = 0;
		for (int i = 0; i < size; i++)
		{
			shape = __shapes[i];
			if (shape.is_selected)
			{
				++__selectedCount;
			}
		}
		shape = null;
		try
		{
			computeLimits(true);
		}
		catch (Exception)
		{
		}
	}

	/// <summary>
	/// Re-index the data for the layer.  This is useful if the initial data has been
	/// updated (shapes inserted or removed).  It is assumed that in such case, the
	/// shape and table information have been modified consistently.  The re-indexing
	/// operation loops through all shapes and resets the index in the shapes to be
	/// sequential (they are not resorted, the indexes are reset).
	/// </summary>
	public virtual void reindex()
	{
		int size = __shapes.Count;
		GRShape shape = null;
		for (int i = 0; i < size; i++)
		{
			// Just set the index to the loop index...
			shape = __shapes[i];
			shape.index = i;
		}
	}

	/// <summary>
	/// Set the shape.associated_object to null for all shapes in the layer.
	/// The associated object is used to link a shape to in-memory data that are not
	/// in the DataTable (e.g., a model data object).
	/// </summary>
	public virtual void removeAllAssociations()
	{
		int size = 0;
		if (__shapes != null)
		{
			size = __shapes.Count;
		}
		GRShape shape = null;
		for (int i = 0; i < size; i++)
		{
			shape = __shapes[i];
			shape.associated_object = null;
		}
	}

	/// <summary>
	/// Remove shapes that have a null associated object.  This is useful for filtering
	/// geographic data to only that in an application's data.  The indexes of the
	/// data are also reset.  This method should only be called when the DataTable
	/// attribute table is in memory because records from the table are also removed to
	/// ensure consistency with the shape vector. </summary>
	/// <param name="hide_only"> If true, then the unassociated shapes will just be hidden.
	/// This results in more shapes being in memory, but the shapes will be accessible
	/// if needed (e.g., for searches to add a feature and turn visible).  This also
	/// will allow attribute data to be read on the fly. </param>
	public virtual void removeUnassociatedShapes(bool hide_only)
	{
		int size = __shapes.Count;
		GRShape shape = null;
		IList<TableRecord> records = null;
		if (__attributeTable != null)
		{
			records = __attributeTable.getTableRecords();
		}
		for (int i = 0; i < size; i++)
		{
			shape = __shapes[i];
			if (shape.associated_object == null)
			{
				if (hide_only)
				{
					// Just set to not visible...
					shape.is_visible = false;
				}
				else
				{
					// Actually remove the shape...
					__shapes.RemoveAt(i);
					try
					{
						if (records != null)
						{
							records.RemoveAt(i);
						}
					}
					catch (Exception)
					{
						// Table not the same size??
						;
					}
					--size;
					--i;
				}
			}
		}
		if (!hide_only)
		{
			reindex();
		}
	}

	/// <summary>
	/// Set the application layer type.  This information can then be used by an
	/// application to turn on/off layers or skip layers during processing.  The type
	/// can be set using the AppLayerType property in the GeoView project file.
	/// An example is "Streamflow". </summary>
	/// <param name="appLayerType"> Application layer type. </param>
	public virtual void setAppLayerType(string appLayerType)
	{
		if (!string.ReferenceEquals(appLayerType, null))
		{
			__appLayerType = appLayerType;
		}
	}

	/// <summary>
	/// Set the attribute table associated with the shapes.  This is most often called
	/// when the attribute table is read first and then shapes are associated with the table. </summary>
	/// <param name="attribute_table"> Attribute table for the layer. </param>
	public virtual void setAttributeTable(DataTable attribute_table)
	{
		__attributeTable = attribute_table;
	}

	/// <summary>
	/// Set the shapes associated with the layer.  This is most often called
	/// when bulk manipulation of layers is occurring. </summary>
	/// <param name="shapes"> Shape list for the layer. </param>
	public virtual void setShapes(IList<GRShape> shapes)
	{
		__shapes = shapes;
	}

	/// <summary>
	/// Sets a value in the attribute table. </summary>
	/// <param name="row"> the row to set the value (0-based). </param>
	/// <param name="column"> the column to set the value (0-based). </param>
	/// <param name="value"> the value to set. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setAttributeTableValue(int row, int column, Object value) throws Exception
	public virtual void setAttributeTableValue(int row, int column, object value)
	{
		if (__attributeTable == null)
		{
			return;
		}
		__attributeTable.setFieldValue(row, column, value);
	}

	/// <summary>
	/// Set the data format label.
	/// </summary>
	public virtual void setDataFormat(string dataFormat)
	{
		__dataFormat = dataFormat;
	}

	/// <summary>
	/// Set the file name for the layer. </summary>
	/// <param name="fileName"> Name of layer input file. </param>
	public virtual void setFileName(string fileName)
	{
		if (!string.ReferenceEquals(fileName, null))
		{
			__fileName = fileName;
		}
	}

	/// <summary>
	/// Set the data limits for the layer. </summary>
	/// <param name="limits"> data limits for layer </param>
	public virtual void setLimits(GRLimits limits)
	{
		__limits = limits;
	}

	/// <summary>
	/// Set the data limits for the layer. </summary>
	/// <param name="x1"> Left X value (usually the minimum X value). </param>
	/// <param name="y1"> Bottom Y value (usually the minimum Y value). </param>
	/// <param name="x2"> Right X value (usually the maximum X value). </param>
	/// <param name="y2"> Top Y value (usually the maximum Y value). </param>
	public virtual void setLimits(double x1, double y1, double x2, double y2)
	{
		__limits = new GRLimits(x1, y1, x2, y2);
	}

	/// <summary>
	/// Set the number of selected shapes.  This should be called by code that is
	/// selecting and deselecting shapes in a layer.  This approach is use because it
	/// is is a performance hit to loop through the shapes to determine the count.
	/// If necessary, the refresh() method can be called. </summary>
	/// <param name="selected_count"> The number of selected shapes. </param>
	public virtual void setNumSelected(int selected_count)
	{
		__selectedCount = selected_count;
	}

	/// <summary>
	/// Set the projection for the layer data.  This is typically done when reading the
	/// layer from a project file because the project file indicates the projection
	/// (not the data file itself).  For example, ESRI shapefiles do not contain a
	/// projection and the projection must be specified in a project file. </summary>
	/// <param name="projection"> Projection for the layer. </param>
	public virtual void setProjection(GeoProjection projection)
	{
		__projection = projection;
	}

	/// <summary>
	/// Set the String value of a property for the layer.  This calls PropList.setValue(). </summary>
	/// <param name="key"> Key (variable) for the property. </param>
	/// <param name="value"> Value for the property. </param>
	public virtual void setPropValue(string key, string value)
	{
		__props.setValue(key, value);
	}

	/// <summary>
	/// Set all shapes visible or invisible.  This is useful, for example, when
	/// (un)selected shapes need to be (in)visible. </summary>
	/// <param name="is_visible"> If true, all shapes in the layer will be set to visible.  If
	/// false, all shapes will be set to invisible. </param>
	/// <param name="do_selected"> If true, apply the change to selected shapes.
	/// If false, do not change the visibility of selected shapes. </param>
	/// <param name="do_unselected"> If true, apply the change to unselected shapes.
	/// If false, do not change the visibility of unselected shapes. </param>
	public virtual void setShapesVisible(bool is_visible, bool do_selected, bool do_unselected)
	{
		foreach (GRShape shape in __shapes)
		{
			if (shape.is_selected && do_selected)
			{
				shape.is_visible = is_visible;
			}
			else if (!shape.is_selected && do_unselected)
			{
				shape.is_visible = is_visible;
			}
		}
	}

	/// <summary>
	/// Set the shape type (e.g., POINT).  The type is not currently checked for validity.
	/// </summary>
	public virtual void setShapeType(int shape_type)
	{
		__shapeType = shape_type;
	}

	/// <summary>
	/// Save the layer as a shapefile.  If necessary this method should be defined in
	/// derived classes so that specific data attributes, etc., can be handled.
	/// If not defined in a derived class, it is expected that the shapes and attribute
	/// table records can be saved to standard Shapefile formats.
	/// All visible, selected shapes are written in the specified projection. </summary>
	/// <param name="filename"> Name of file to write. </param>
	/// <param name="projection"> Projection to use for output data. </param>
	/// <exception cref="IOException"> if there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeShapefile(String filename, GeoProjection projection) throws java.io.IOException
	public virtual void writeShapefile(string filename, GeoProjection projection)
	{
		ESRIShapefile.write(filename, __attributeTable, __shapes, true, true, __projection, projection);
	}

	/// <summary>
	/// Save the layer as a shapefile.  If necessary this method should be defined in
	/// derived classes so that specific data attributes, etc., can be handled.
	/// If not defined in a derived class, it is expected that the shapes and attribute
	/// table records can be saved to standard Shapefile formats. </summary>
	/// <param name="filename"> Name of file to write. </param>
	/// <param name="visible_only"> If true, only visible shapes are written.  If false, all shapes are written. </param>
	/// <param name="selected_only"> If true, only selected shapes are written.  If false, all
	/// shapes are written (contingent on the other flag). </param>
	/// <param name="projection"> Projection to use for output data. </param>
	/// <exception cref="IOException"> if there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeShapefile(String filename, boolean visible_only, boolean selected_only, GeoProjection projection) throws java.io.IOException
	public virtual void writeShapefile(string filename, bool visible_only, bool selected_only, GeoProjection projection)
	{
		ESRIShapefile.write(filename, __attributeTable, __shapes, visible_only, selected_only, __projection, projection);
	}

	/// <summary>
	/// Save the layer as a shapefile.  This method should be defined in derived classes
	/// so that specific data attributes, etc., can be handled.  This method was
	/// implemented to handle grid data output and may not be appropriate for all other
	/// layer types.  If the design changes in the future, this method may be deprecated. </summary>
	/// <param name="filename"> Name of file to write. </param>
	/// <param name="projection"> Projection to use for output data. </param>
	/// <param name="use_data_limits"> If true, then the following parameters are used.  This
	/// is useful if a large grid is being processed down to a smaller size. </param>
	/// <param name="min_data_value"> Minimum data value to write (for a grid there is only one
	/// data value per grid cell). </param>
	/// <param name="max_data_value"> Maximum data value to write (for a grid there is only one
	/// data value per grid cell). </param>
	/// <exception cref="IOException"> if there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeShapefile(String filename, GeoProjection projection, boolean use_data_limits, double min_data_value, double max_data_value) throws java.io.IOException
	public virtual void writeShapefile(string filename, GeoProjection projection, bool use_data_limits, double min_data_value, double max_data_value)
	{
		Message.printWarning(2, "GeoLayer.writeShapefile", "This method should be defined in the derived class.");
	}

	}

}