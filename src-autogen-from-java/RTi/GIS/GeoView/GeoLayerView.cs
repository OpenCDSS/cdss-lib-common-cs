using System;
using System.Collections.Generic;

// GeoLayerView - class to hold a geographic layer and view information

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
// GeoLayerView - class to hold a geographic layer and view information
// ============================================================================
// Copyright:	See the COPYRIGHT file.
// ============================================================================
// History:
//
// 14 Jun 1999	Steven A. Malers	Initial version.  This is
//		Riverside Technology,	somewhat equivalent to the
//		inc.			theme concept in ArcView.
// 07 Jul 1999	SAM, RTi		Add legend.  Add _is_visible.  Add
//					_label_field.
// 30 Aug 1999	SAM, RTi		Add PropList in constructor and add
//					property for Label.  Use the PropList
//					for label properties rather than
//					explicit data values.
// 27 Jun 2001	SAM, RTi		Add default legend text to be the name
//					of the layer file.  Need to define a
//					default symbol.  Add isSelected() to
//					indicate if layer view is selected
//					(e.g., highlighted in legend).
// 01 Aug 2001	SAM, RTi		Change so that the legend has only the
//					file name and not the leading directory.
// 17 Sep 2001	SAM, RTi		Move ESRIShapefile to this package.
// 27 Sep 2001	SAM, RTi		Recognize Xmgr files transparently
//					(currently by checking for a .xmrg file
//					extension).
// 2001-10-17	SAM, RTi		Set unused data to null to help
//					garbage collection.
// 2001-12-04	SAM, RTi		Update to use Swing.
// 2002-01-08	SAM, RTi		Change GeoViewCanvas to
//					GeoViewJComponent.
// ----------------------------------------------------------------------------
// 2003-05-06	J. Thomas Sapienza, RTi	Bring in line with the non-Swing
//					version of the code
// 2004-08-02	JTS, RTi		Added members and methods for managing
//					animated layers.
// 2004-10-27	JTS, RTi		Implements Cloneable.
// 2005-04-27	JTS, RTi		Added all data members to finalize().
// 2006-01-23	SAM, RTi		Skip null shapes in selectFeatures().
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------
// EndHeader

namespace RTi.GIS.GeoView
{

	using GRColor = RTi.GR.GRColor;
	using GRLegend = RTi.GR.GRLegend;
	using GRShape = RTi.GR.GRShape;
	using GRSymbol = RTi.GR.GRSymbol;

	using PropList = RTi.Util.IO.PropList;

	using Message = RTi.Util.Message.Message;

	using StringUtil = RTi.Util.String.StringUtil;

	using DataTable = RTi.Util.Table.DataTable;
	using TableRecord = RTi.Util.Table.TableRecord;

	using StopWatch = RTi.Util.Time.StopWatch;

	/// <summary>
	/// A GeoViewLayer joins information from a GeoLayer (raw data) and the view data
	/// that are controlled and displayed by a GeoView.  The GeoViewLayer object tracks
	/// the legend and other data.  This allows multiple GeoLayerViews to be
	/// constructed from the same GeoLayer.  The GeoLayerView is also then used to
	/// interact with the user because it understands the view and the data.
	/// The legend indicates what symbology should be used to render the view.
	/// </summary>
	public class GeoLayerView : ICloneable
	{

	/// <summary>
	/// For each of the attribute table fields, specifies whether the field should be
	/// drawn on the display or not.  
	/// </summary>
	private bool[] __animationFieldVisible = null;

	/// <summary>
	/// Indicates whether the layer is visible or not.
	/// </summary>
	private bool _is_visible = true;

	/// <summary>
	/// Indicates whether the layer is selected or not.  Normally this is used only for GUI interaction.
	/// </summary>
	private bool _is_selected = false;

	/// <summary>
	/// Whether this is an animated layer, where the shape symbols will change.
	/// </summary>
	private bool __isAnimated = false;

	// TODO (JTS - 2004-08-09) put the missing data things into a proplist

	/// <summary>
	/// The value that represents missing data.  Defaults to -999.0.
	/// </summary>
	private double __missingDouble = -999.0;

	/// <summary>
	/// The value that will replace missing data.  Defaults to -999.0.
	/// </summary>
	private double __missingDoubleReplacement = -999.0;

	/// <summary>
	/// GeoLayer used by the GeoView.
	/// </summary>
	private GeoLayer _layer = null;

	/// <summary>
	/// GeoViewJComponent that uses the GeoLayer.
	/// </summary>
	private GeoViewJComponent _view = null;

	/// <summary>
	/// Color to use for layer, including symbols.
	/// </summary>
	private GRLegend _legend = null;

	/// <summary>
	/// An array that points to the fields within the attribute table that are
	/// animated. This array does not correspond with all the fields in the attribute 
	/// table but will instead contain a series of values, such as:<para>
	/// <ul>
	/// <li>__animationFields[0] = 12</li>
	/// <li>__animationFields[1] = 13</li>
	/// <li>__animationFields[2] = 15</li>
	/// </ul>
	/// This means that fields 12, 13 and 15 (base-0) in the table are animated fields.
	/// 
	/// </para>
	/// </summary>
	private int[] __animationFields = null;

	/// <summary>
	/// The gui that controls the animation for this layer.  Null if the layer is not animated.
	/// </summary>
	private JFrame __controlJFrame = null;

	/// <summary>
	/// Properties for the GeoViewLayer, including labeling information.
	/// </summary>
	private PropList _props = null;

	// TODO (JTS - 2004-08-09) put into a proplist

	/// <summary>
	/// The name of the layer.
	/// </summary>
	private string __layerName = null;

	/// <summary>
	/// Construct and initialize to null data.
	/// The view will be set when this GeoLayerView is passed to GeoView.addLayerView.
	/// </summary>
	public GeoLayerView()
	{
		initialize();
	}

	/// <summary>
	/// Construct from a layer.  The view will be set when this GeoLayerView is passed to GeoView.addLayerView. </summary>
	/// <param name="layer"> GeoLayer instance. </param>
	public GeoLayerView(GeoLayer layer)
	{
		initialize();
		_layer = layer;
	}

	/// <summary>
	/// Construct from a layer file and properties.  The layer is first read and then
	/// default symbol properties are assigned based on the count of the layers.
	/// Currently attributes are NOT read. </summary>
	/// <param name="filename"> Name of ESRI shapefile. </param>
	/// <param name="props"> Properties for the layer view, for example as read from a GeoView project file
	/// (see overloaded version for description). </param>
	/// <param name="count"> Count of layers being added.  This affects the default symbols
	/// that are assigned.  The first value should be 1.  <b>This is not the
	/// GeoLayerView number in a GVP file.</b> </param>
	/// <exception cref="Exception"> if there is an error reading the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public GeoLayerView(String filename, RTi.Util.IO.PropList props, int count) throws Exception
	public GeoLayerView(string filename, PropList props, int count)
	{ // First read the file...
		setLayer(GeoLayer.readLayer(filename, props));
		setProperties(props);
		// Set default symbol, legend.  This will normally be reset (e.g., when reading in GeoViewProject)
		setDefaultLegend(count);
	}

	/// <summary>
	/// Construct from a layer and set properties.  The view will be set when this
	/// GeoLayerView is passed to GeoView.addLayerView. </summary>
	/// <param name="layer"> GeoLayer instance. </param>
	/// <param name="props"> Properties for the GeoLayerView.  The following properties are recognized:
	/// <para>
	/// 
	/// <table width=100% cellpadding=10 cellspacing=0 border=2>
	/// <tr>
	/// <td><b>Property</b></td>   <td><b>Description</b></td>   <td><b>Default</b></td>
	/// </tr
	/// 
	/// <tr>
	/// <td><b>Label</b></td>
	/// <td>Indicates how to label the view.  If the property is set to
	/// "UsingGeoViewListener", then the listener method "geoViewGetLabel" will be
	/// called for each shape that is drawn.  The application implementing the
	/// "geoViewGetLabel" method should determine an appropriate label and return the String for drawing.
	/// If the property is set to "UsingAttributeTable" then the
	/// "AttributeTableLabelField" property should also be defined.
	/// <td>No labels (just symbols, if a symbol type is defined).</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>AttributeTableLabelField</b></td>
	/// <td>Indicates the field from the attribute table to use for labelling.
	/// <td>Not defined.</td>
	/// </tr>
	/// 
	/// </table> </param>
	public GeoLayerView(GeoLayer layer, PropList props)
	{
		initialize();
		_layer = layer;
		// Default is just main part of the filename.
		File f = new File(layer.getFileName());
		//_legend = new GRLegend ( null, layer.getFileName() );
		_legend = new GRLegend(null, f.getName());
		f = null;
		if (props != null)
		{
			_props = props;
		}
	}

	/// <summary>
	/// Construct from a layer and specify the legend.  
	/// The view will be set when this GeoLayerView is passed to GeoViewJComponent.addLayerView. </summary>
	/// <param name="layer"> GeoLayer instance. </param>
	/// <param name="legend"> GRLayer instance. </param>
	public GeoLayerView(GeoLayer layer, GRLegend legend)
	{
		initialize();
		_layer = layer;
		_legend = legend;
	}

	/// <summary>
	/// Construct from a layer and specify the legend and other properties.
	/// The view will be set when this GeoLayerView is passed to GeoViewJComponent.addLayerView. </summary>
	/// <param name="layer"> GeoLayer instance. </param>
	/// <param name="legend"> GRLayer instance. </param>
	/// <param name="props"> Properties for the GeoLayerView.  See other constructors for a description. </param>
	public GeoLayerView(GeoLayer layer, GRLegend legend, PropList props)
	{
		initialize();
		_layer = layer;
		_legend = legend;
		if (props != null)
		{
			_props = props;
		}
	}

	/// <summary>
	/// Clones this object.  Does not clone animation information. </summary>
	/// <returns> a clone of this object. </returns>
	public virtual object clone()
	{
		GeoLayerView l = null;
		try
		{
			l = (GeoLayerView)base.clone();
		}
		catch (Exception)
		{
			return null;
		}

		if (_layer != null)
		{
			l._layer = (GeoLayer)_layer.clone();
		}

		l._view = _view;

		if (_legend != null)
		{
			l._legend = (GRLegend)_legend.clone();
		}

		if (_props != null)
		{
			l._props = new PropList(_props);
		}

		return l;
	}

	/// <summary>
	/// Finalize before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~GeoLayerView()
	{
		_layer = null;
		_legend = null;
		_props = null;
		_view = null;
		__animationFieldVisible = null;
		__animationFields = null;
		__controlJFrame = null;
		__layerName = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the JFrame used to control animation on this layer. </summary>
	/// <returns> the JFrame used to control animation on this layer. </returns>
	public virtual JFrame getAnimationControlJFrame()
	{
		return __controlJFrame;
	}

	/// <summary>
	/// Returns the number of rows in the attribute table. </summary>
	/// <returns> the number of rows in the attribute table. </returns>
	public virtual int getAttributeTableRowCount()
	{
		return _layer.getAttributeTableRowCount();
	}

	/// <returns> label field used for labeling.  This returns the
	/// "AttributeTableLabelField" property value or null if not defined.
	/// TODO SAM 2009-07-01 NEED TO PHASE THIS OUT. </returns>
	public virtual string getLabelField()
	{
		return _props.getValue("AttributeTableLabelField");
	}

	/// <summary>
	/// Return the GeoLayer associated with the GeoLayerView. </summary>
	/// <returns> the GeoLayer associated with the GeoLayerView. </returns>
	public virtual GeoLayer getLayer()
	{
		return _layer;
	}

	/// <summary>
	/// Return the legend used for the GeoLayerView. </summary>
	/// <returns> the legend used for the GeoLayerView. </returns>
	public virtual GRLegend getLegend()
	{
		return _legend;
	}

	/// <summary>
	/// Returns the value that represents missing data for this layer.  Default is -999.0. </summary>
	/// <returns> the value that represents missing data for this layer. </returns>
	public virtual double getMissingDoubleValue()
	{
		return __missingDouble;
	}

	/// <summary>
	/// Returns the value that will replace missing data for this layer when drawn
	/// on the display.    Default is -999.0. </summary>
	/// <returns> the value that will replace missing data when data are drawn. </returns>
	public virtual double getMissingDoubleReplacementValue()
	{
		return __missingDoubleReplacement;
	}

	// TODO SAM 2006-03-02 Need to determine whether the name is a data member or from the PropList
	/// <summary>
	/// Returns the layer's name. </summary>
	/// <returns> the layer's name. </returns>
	public virtual string getName()
	{
		if (!string.ReferenceEquals(__layerName, null))
		{
			return __layerName;
		}
		string name = _props.getValue("Name");
		if (!string.ReferenceEquals(name, null))
		{
			return name;
		}
		if (_legend != null)
		{
			return _legend.getText();
		}
		return null;
	}

	/// <summary>
	/// Returns the number of fields to animate. </summary>
	/// <returns> the number of fields to animate. </returns>
	public virtual int getNumAnimationFields()
	{
		if (__animationFields == null)
		{
			return 0;
		}
		return __animationFields.Length;
	}

	/// <summary>
	/// Return the PropList used for the GeoLayerView. </summary>
	/// <returns> the PropList used for the GeoLayerView. </returns>
	public virtual PropList getPropList()
	{
		return _props;
	}

	/// <summary>
	/// Return the symbol used for the GeoLayerView.  This calls the getSymbol() method for the legend. </summary>
	/// <returns> the symbol used for the GeoLayerView or null if a legend is not defined. </returns>
	public virtual GRSymbol getSymbol()
	{
		if (_legend == null)
		{
			return null;
		}
		return _legend.getSymbol();
	}

	/// <summary>
	/// Return the GeoViewJComponent associated with the GeoLayerView. </summary>
	/// <returns> the GeoViewJComponent associated with the GeoLayerView. </returns>
	public virtual GeoViewJComponent getView()
	{
		return _view;
	}

	/// <summary>
	/// Initialize data.
	/// </summary>
	private void initialize()
	{
		_is_visible = true; // Default is that layer view is visible.
		_is_selected = false; // Default is that layer view is not specifically selected.
		_layer = null;
		_legend = null;
		_view = null;
		// Get an empty property list...
		_props = PropList.getValidPropList(null, "GeoLayerView");
	}

	/// <summary>
	/// Returns whether this is an animated layer. </summary>
	/// <returns> whether this is an animated layer. </returns>
	public virtual bool isAnimated()
	{
		return __isAnimated;
	}

	/// <summary>
	/// Returns whether or not a field is one being animated. </summary>
	/// <param name="num"> the number of the field in the attribute table. </param>
	/// <returns> true if the field is animated, false if not. </returns>
	public virtual bool isAnimatedField(int num)
	{
		if (__animationFields == null)
		{
			return false;
		}
		else
		{
			for (int i = 0; i < __animationFields.Length; i++)
			{
				if (__animationFields[i] == num)
				{
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Returns whether an animated field is visible or not. </summary>
	/// <param name="field"> the number of the field to check for visibility. </param>
	/// <returns> true if the field is visible, false if not. </returns>
	public virtual bool isAnimationFieldVisible(int field)
	{
		if (__animationFieldVisible == null)
		{
			setupAnimationFieldVisible();
			return true;
		}
		return __animationFieldVisible[field];
	}

	/// <summary>
	/// Indicate whether a layer view is selected. </summary>
	/// <returns> true if the layer is selected, false if not. </returns>
	public virtual bool isSelected()
	{
		return _is_selected;
	}

	/// <summary>
	/// Set whether a GeoViewLayer is selected or not. </summary>
	/// <returns> true if the layer is selected, false if not, after the reset. </returns>
	/// <param name="is_selected"> Indicates whether the layer view is selected or not. </param>
	public virtual bool isSelected(bool is_selected)
	{
		_is_selected = is_selected;
		return _is_selected;
	}

	/// <summary>
	/// Indicate whether a layer view is visible. </summary>
	/// <returns> true if the layer is visible, false if not. </returns>
	public virtual bool isVisible()
	{
		return _is_visible;
	}

	/// <summary>
	/// Set whether a GeoViewLayer is visible or not. </summary>
	/// <returns> true if the layer is visible, false if not, after the reset. </returns>
	/// <param name="is_visible"> Indicates whether the layer view is visible or not. </param>
	public virtual bool isVisible(bool is_visible)
	{
		_is_visible = is_visible;
		return _is_visible;
	}

	/// <summary>
	/// Select features in the layer view based on a check of an attribute value (e.g.,
	/// a string identifier).  This method is called from the GeoViewPanel
	/// selectAppFeatures() method but can be called at a lower level (e.g., to select
	/// all shapes that correspond to identifiers in a data set. </summary>
	/// <param name="feature_array"> The data attributes corresponding to the AppJoinField
	/// property saved with a GeoLayerView.  One or more values can be specified. </param>
	/// <param name="join_field"> Fields in the layer to search (e.g., the AppLayerField).  This
	/// can be multiple fields separated by commas. </param>
	/// <param name="append"> Indicates whether the selections should be added to previous
	/// selections.  <b>This feature is under development.</b> </param>
	/// <returns> list of GeoRecord for the selected features, or null if nothing is selected. </returns>
	public virtual IList<GeoRecord> selectFeatures(string[][] feature_array, string join_field, bool append)
	{
		string routine = "GeoLayerView.selectFeatures";
		// First try the old code...
		StopWatch timer = new StopWatch();
		/* To test old code...
		timer.start();
		Vector v = selectFeatures1 ( feature_array, join_field, append );
		timer.stop();
		Message.printStatus ( 1, routine, "Old select time = " +
		timer.getSeconds() );
		timer.clear();
		*/

	//System.out.println("\nGeoLayerView");

		timer.start();
		if (feature_array == null)
		{
	//System.out.println("   (null feature array)");
			return null;
		}
		int nfeature = feature_array.Length;
		if (nfeature == 0)
		{
	//System.out.println("   (nfeature == 0)");	
			return null;
		}
		// List of GeoRecord that will be returned...
		IList<GeoRecord> georecords = null;
		// Get the fields to search...
		if (string.ReferenceEquals(join_field, null))
		{
	//System.out.println("   (join field is null)");		
			return georecords;
		}
		IList<string> joinFieldList = StringUtil.breakStringList(join_field, ",", 0);
		if (joinFieldList == null)
		{
	//System.out.println("   (join fields vector is null)");
			return georecords;
		}
		// Now figure out what integer fields these are in the attribute data...
		int join_fields_size = joinFieldList.Count;
		int[] join_fields = new int[join_fields_size];
		string[] format_spec = new string[join_fields_size];
		DataTable table = _layer.getAttributeTable();
		TableRecord table_record = null;
		int ifeature;
		int ijf = 0;
					// found.
		for (ijf = 0; ijf < join_fields_size; ijf++)
		{
			try
			{
				join_fields[ijf] = table.getFieldIndex((string)joinFieldList[ijf]);
				format_spec[ijf] = table.getFieldFormat(join_fields[ijf]);
			}
			catch (Exception)
			{
				Message.printWarning(2, routine, "Join field \"" + (string)joinFieldList[ijf] + " not found in data layer");
				return georecords;
			}
		}
		// Loop through all the shapes in the layer to find the ones that have
		// attributes that match the selected shapes...
		IList<GRShape> shapes = _layer.getShapes();
		int nshapes = 0;
		if (shapes != null)
		{
			nshapes = shapes.Count;
		}
		string layer_name = _legend.getText();
		if (layer_name.Equals(""))
		{
			layer_name = _layer.getFileName();
		}
		Message.printStatus(1, routine, "Searching \"" + layer_name + "\" for matching features...");
		GRShape shape;
		object o;
		string formatted_attribute;
		bool allow_multiple_shapes = false; // Allow multiple shapes to
							// match the search criteria.
							// Support for true should be
							// added when 1 to many or
							// numeric field searches are
							// allowed
		// Create an array to hold a count of the number of fields for the
		// current shape that match for each requested feature.  Use the
		// smallest data size possible...
		sbyte[] shape_matches_feature = new sbyte[nfeature];

	//System.out.println("nshapes: " + nshapes);
		for (int ishape = 0; ishape < nshapes; ishape++)
		{
			shape = shapes[ishape];
			// Skip null shapes, generally due to missing coordinates...
			if (shape.type == GeoLayer.UNKNOWN)
			{
				continue;
			}
			// For now, skip features that have zeros for the max and min
			// values.  This occurs when null data come from a database into
			// shapefiles.  For now treat as missing data.  This prevents
			// more problems later.
			if (shape.xmin == 0.0)
			{
				continue;
			}
			// See if the value matches the value in the list that was
			// passed in.  Currently this is done with strings so it will be
			// a problem if floating point data fields are joined...
			//
			// First initialize the following array.  For the current shape
			// this indicates a count of how many requested feature
			// attributes are matched.  This is necessary because if more
			// than one attribute is compared, a count of the previous
			// loop results is needed.  If for any requested feature the
			// count of matches is the number of attributes, then the shape
			// matches.
			for (ifeature = 0; ifeature < nfeature; ifeature++)
			{
				shape_matches_feature[ifeature] = 0;
			}
			// Loop through each join field...
			for (ijf = 0; ijf < join_fields_size; ijf++)
			{
				// Get the data value from the attribute table.  Do this
				// in the outside loop so that if attributes are read
				// from a file, it is only read once, which should
				// improve performance...
				//if ( Message.isDebugOn ) {
				//	Message.printDebug ( 1, "",
				//	"Checking join field ["  + ijf + "]=" +
				//	(String)join_fields_Vector.elementAt(ijf) );
				//}
				try
				{
					o = table.getFieldValue(shape.index, join_fields[ijf]);
				}
				catch (Exception)
				{
					// Just skip...
					continue;
				}
				// Format the attribute value to a common format...
				formatted_attribute = StringUtil.formatString(o.ToString(), format_spec[ijf]).Trim();
	//System.out.println("   formatted_attribute: '" + formatted_attribute + "'");
				// Now loop through each feature that is being searched
				// for (each if the join fields will be checked in
				// an inner loop below)...
				for (ifeature = 0; ifeature < nfeature; ifeature++)
				{
					// If the previous field matches where not made,
					// there is no need to check other fields (the shape does not match).
					//
					// shape_matches_feature will = ijf if previous
					// attributes have matched.  The first time
					// through, shape_matches_feature will be 0 and ijf will be 0.
	/* Need to change this to see if NO first attribute for the shape matched.
					if ( shape_matches_feature[ifeature] < ijf ) {
						// Previous attribute for shape did not
						// match this feature's attributes so no reason to check more attributes...
						if ( Message.isDebugOn ) {
							Message.printDebug ( 1, "",
							"Previous attribute for " +
							"ifeature ["
							+ ifeature +"] did not match.");
						}
						break;
					}
	*/
					//if ( Message.isDebugOn ) {
					//	Message.printDebug ( 1, "",
					//	"comparing \"" +
					//	formatted_attribute + "\" to \""
					//	+ feature_array[ifeature][ijf]+"\"");
					//}
					if (formatted_attribute.Equals(feature_array[ifeature][ijf], StringComparison.OrdinalIgnoreCase))
					{
	//System.out.println("      f[" + ifeature + "][" + ijf + "]: '" + feature_array[ifeature][ijf] + "'");
						// The features matches...
						++shape_matches_feature[ifeature];
						//if ( Message.isDebugOn ) {
						//	Message.printDebug ( 1, "",
						//	"shape_matches_feature[" +
						//	ifeature + "]=" +
						//	shape_matches_feature[
						//	ifeature] );
						//}
						if (shape_matches_feature[ifeature] == join_fields_size)
						{
							// Matched all fields so add to
							// the match list...
							if (!append)
							{
								// Always select...
								if (!shape.is_selected)
								{
									_layer.setNumSelected(_layer.getNumSelected() + 1);
								}
								shape.is_selected = true;
							}
							else
							{
								// Reverse selection...
								if (!shape.is_selected)
								{
									shape.is_selected = true;
									_layer.setNumSelected(_layer.getNumSelected() + 1);
								}
								else
								{
									shape.is_selected = false;
									_layer.setNumSelected(_layer.getNumSelected() - 1);
								}
							}
							Message.printStatus(2, "", "Matched shape type=" + shape.type + " index=" + shape.index + " id=" + o.ToString());
							// Add to the GeoRecord list..
							if (georecords == null)
							{
								georecords = new List<GeoRecord>();
							}
							try
							{
								table_record = table.getRecord((int)shape.index);
							}
							catch (Exception)
							{
								table_record = null;
							}
							GeoRecord georecord = new GeoRecord(shape, table_record, _layer, this);
							georecords.Add(georecord);
							// Break out of the loop since a match was made...
							if (!allow_multiple_shapes)
							{
								break;
							}
						}
					}
				}
			}
		}
		timer.stop();
		Message.printStatus(1, routine, "Select took " + timer.getSeconds() + " seconds.");
		return georecords;
	}

	// TODO SAM 2007-05-09 Evaluate whether this is needed
	/// <summary>
	/// This version is optimized if 1 join field is used.
	/// Select features in the layer view based on a check of an attribute value (e.g.,
	/// a string identifier).  This method is called from the GeoViewPanel
	/// selectAppFeatures() method but can be called at a lower level (e.g., to select
	/// all shapes that correspond to identifiers in a data set. </summary>
	/// <param name="feature_array"> The data attributes corresponding to the AppJoinField
	/// property saved with a GeoLayerView.  One or more values can be specified. </param>
	/// <param name="join_field"> Fields in the layer to search (e.g., the AppLayerField).  This
	/// can be multiple fields separated by commas. </param>
	/// <param name="append"> Indicates whether the selections should be added to previous
	/// selections.  <b>This feature is under development.</b> </param>
	/// <returns> Vector of GeoRecord for the selected features, or null if nothing is selected. </returns>
	/*
	private Vector selectFeatures1 (String [][] feature_array, String join_field, boolean append )
	{	String routine = "GeoLayerView.selectFeatures";
		if ( feature_array == null ) {
			return null;
		}
		int nfeature = feature_array.length;
		if ( nfeature == 0 ) {
			return null;
		}
		// Vector of GeoRecord that will be returned...
		Vector georecords = null;
		// Get the fields to search...
		if ( join_field == null ) {
			return georecords;
		}
		Vector join_fields_Vector = StringUtil.breakStringList (
						join_field, ",", 0 );
		if ( join_fields_Vector == null ) {
			return georecords;
		}
		// Now figure out what integer fields these are in the attribute data...
		int join_fields_size = join_fields_Vector.size();
		int join_fields_size_m1 = join_fields_size - 1;
		int [] join_fields = new int[join_fields_size];
		String [] format_spec = new String[join_fields_size];
		DataTable table = _layer.getAttributeTable();
		TableRecord table_record = null;
		int ifeature;
		int ijf = 0;
		boolean allow_multiple_shapes = false;	// Allow multiple shapes to
							// match the search criteria.
							// Support for true should be
							// added when 1 to many or
							// numeric field searches are
							// allowed
		for ( ijf = 0; ijf < join_fields_size; ijf++ ) {
			try {	join_fields[ijf] = table.getFieldIndex(
				(String)join_fields_Vector.elementAt(ijf) );
				format_spec[ijf] = table.getFieldFormat(
					join_fields[ijf] );
			}
			catch ( Exception e ) {
				Message.printWarning ( 2, routine,
				"Join field \"" +
				(String)join_fields_Vector.elementAt(ijf) +
				" not found in data layer" );
				return georecords;
			}
		}
		// Loop through all the shapes in the layer to find the ones that have
		// attributes that match the selected shapes...
		Vector shapes = _layer.getShapes();
		int nshapes = 0;
		if ( shapes != null ) {
			nshapes = shapes.size();
		}
		String layer_name = _legend.getText();
		if ( layer_name.equals("") ) {
			layer_name = _layer.getFileName();
		}
		Message.printStatus ( 1, routine,
		"Searching \"" + layer_name + "\" for matching features..." );
		GRShape shape;
		Object o;
		for ( int ishape = 0; ishape < nshapes; ishape++ ) {
			shape = (GRShape)shapes.elementAt(ishape);
			// For now, skip features that have zeros for the max and min
			// values.  This occurs when null data come from a database into
			// shapefiles.  For now treat as missing data.  This prevents
			// more problems later.
			if ( shape.xmin == 0.0 ) {
				continue;
			}
			// See if the value matches the value in the list that was
			// passed in.  Currently this is done with strings so it will be
			// a problem if floating point data fields are joined...
			for (	ifeature = 0; ifeature < nfeature; ifeature++ ) {
				// Loop through each join field...
				for ( ijf = 0; ijf < join_fields_size; ijf++ ) {
					// Get the data value from the attribute table..
					try {	o = table.getFieldValue ( shape.index,
							join_fields[ijf] );
					}
					catch ( Exception e ) {
						// Just skip...
						continue;
					}
					if ( Message.isDebugOn ) {
						Message.printDebug ( 1, "",
						"comparing \"" +
						StringUtil.formatString( o.toString(),
						format_spec[ijf]).trim() + "\" to \"" +
						feature_array[ifeature][ijf]+"\"");
					}
					if (	StringUtil.formatString( o.toString(),
						format_spec[ijf]).trim(
						).equalsIgnoreCase(
						feature_array[ifeature][ijf]) ) {
						// The features matches...
						if ( ijf == join_fields_size_m1 ) {
							// Matched all fields so add to
							// the match list...
							if ( !append ) {
								// Always select...
								if (!shape.is_selected){
								_layer.setNumSelected (
								_layer.getNumSelected()
								+ 1 );
								}
								shape.is_selected =true;
							}
							else {	// Reverse selection...
								if(!shape.is_selected ){
								shape.is_selected =true;
								_layer.setNumSelected (
								_layer.getNumSelected()
								+ 1 );
								}
								else {
								shape.is_selected=false;
								_layer.setNumSelected (
								_layer.getNumSelected()
								- 1 );
								}
							}
	//						Message.printStatus ( 1, "",
	//						"Matched shape " +
	//						shape.index + " id=" +
	//						o.toString() );
							// Add to the GeoRecord Vector..
							if ( georecords == null ) {
								georecords = new
								Vector ( 10 );
							}
							try {	table_record =
								table.getRecord(
								(int)shape.index );
							}
							catch ( Exception e ) {
								table_record = null;
							}
							GeoRecord
							georecord =new GeoRecord(
								shape, table_record,
								_layer, this );
							georecords.addElement (
							georecord );
							// Have added so OK to break
							// out of the loop...
							break;
						}
					}
					else {	// Break out of loop since one of the
						// criteria was not met...
						break;
					}
				}
			}
		}
		return georecords;
	}
	*/

	/// <summary>
	/// Sets whether this is an animated layer or not.  Setting to 'true' does not
	/// automatically set up animation -- animation fields and the current step must 
	/// be set, and the attribute table must support it. </summary>
	/// <param name="animated"> whether this is an animated layer or not. </param>
	public virtual void setAnimated(bool animated)
	{
		__isAnimated = animated;
	}

	/// <summary>
	/// Sets the JFrame that is used to control the animation of this layer. </summary>
	/// <param name="jframe"> the JFrame that is used to control the layer animation. </param>
	public virtual void setAnimationControlJFrame(JFrame jframe)
	{
		__controlJFrame = jframe;
	}

	/// <summary>
	/// Sets the array of fields within the attribute table that will be animated.
	/// Setting to null disables animation. </summary>
	/// <param name="fields"> the fields within the attribute table that will be animated;
	/// do not have to be consecutively ordered. </param>
	public virtual void setAnimationFields(int[] fields)
	{
		__animationFields = fields;
		if (fields == null)
		{
			__isAnimated = false;
		}
	}

	/// <summary>
	/// Sets whether an animation field should be visible or not. </summary>
	/// <param name="field"> the number of the field to set visible or not. </param>
	/// <param name="visible"> whether to make the field's data visible or not. </param>
	public virtual void setAnimationFieldVisible(int field, bool visible)
	{
		if (__animationFieldVisible == null)
		{
			setupAnimationFieldVisible();
		}
		__animationFieldVisible[field] = visible;
	}

	/// <summary>
	/// Set default legend information.  This picks default colors and symbol sizes. </summary>
	/// <param name="count"> Count of layers that have been added (1 for first one added).  
	/// <b>This is not the GeoLayerView number in the GVP file.</b> </param>
	public virtual void setDefaultLegend(int count)
	{ // Now set default colors...
		// Add later...
		//_layer_view.setColor ( GRColor.getRandomColor() );
		// For now cycle through...
		// Size after add...
		double symsize = 6.0;
		int layerType = 0, symType = 0;
		GRSymbol symbol = null;
		GRColor color = null;
		GRColor outline_color = null;
		if ((count % 5) == 0)
		{
			color = GRColor.cyan;
		}
		else if ((count % 4) == 0)
		{
			color = GRColor.orange;
		}
		else if ((count % 3) == 0)
		{
			color = GRColor.red;
		}
		else if ((count % 2) == 0)
		{
			color = GRColor.blue;
		}
		else
		{
			color = GRColor.green;
		}
		// Set the symbol...
		//layer_view.setSymbol ( GR.SYM_FCIR + count - 1 );
		// Set the symbol size...
		//layer_view.setSymbolSize ( 6.0 );
		layerType = _layer.getShapeType();
		if ((layerType == GeoLayer.POINT) || (layerType == GeoLayer.POINT_ZM))
		{
			symType = GRSymbol.TYPE_POINT;
		}
		else if ((layerType == GeoLayer.LINE) || (layerType == GeoLayer.POLYLINE_ZM))
		{
			symType = GRSymbol.TYPE_LINE;
		}
		else if (layerType == GeoLayer.POLYGON)
		{
			symType = GRSymbol.TYPE_POLYGON;
			outline_color = GRColor.white;
		}
		else if (layerType == GeoLayer.GRID)
		{
			// Try for now...
			//outline_color = GRColor.black;
		}
		// Create a symbol.  The subtype currently only affects the point symbols...
		// Initial value...
		symbol = new GRSymbol(symType, (GRSymbol.SYM_FCIR + count - 1), color, outline_color, symsize);
		if (layerType == GeoLayer.GRID)
		{
			// Set a default color table assuming 10 colors...
			symbol.setColorTable("YellowToRed", 10);
			symbol.setClassificationType("ClassBreaks");
			// For now hard-code the colors just to see if this works...
			double[] data = new double[10];
			data[0] = 0.0;
			data[1] = 1.0;
			data[2] = 2.0;
			data[3] = 5.0;
			data[4] = 10.0;
			data[5] = 20.0;
			data[6] = 50.0;
			data[7] = 100.0;
			data[8] = 200.0;
			data[9] = 500.0;
			symbol.setClassificationData(data, false);
		}
		if (_legend == null)
		{
			_legend = new GRLegend(symbol);
		}
		else
		{
			_legend.setSymbol(symbol);
		}
		//_legend.setText ( _layer.getFileName() );
		// The legend for the layer view is either the name (e.g., from the
		// GeoView project GeoLayerView.Name property, or the filename.
		string prop_value = _props.getValue("Name");
		if ((!string.ReferenceEquals(prop_value, null)) && !prop_value.Equals(""))
		{
			_legend.setText(prop_value);
		}
		else
		{
			File f = new File(_layer.getFileName());
			_legend.setText(f.getName());
			f = null;
		}
	}

	/// <summary>
	/// Sets a value in the attribute table. </summary>
	/// <param name="row"> the row in which to set the value. </param>
	/// <param name="column"> the column in which to set the value. </param>
	/// <param name="value"> the value to set. </param>
	/// <exception cref="Exception"> if there is an error setting the value. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setAttributeTableValue(int row, int column, Object value) throws Exception
	public virtual void setAttributeTableValue(int row, int column, object value)
	{
		_layer.setAttributeTableValue(row, column, value);
	}

	/// <summary>
	/// Set the layer for the layer view.  This is protected because currently it is intended for use only within
	/// the package during object initialization.
	/// </summary>
	protected internal virtual void setLayer(GeoLayer layer)
	{
		_layer = layer;
	}

	/// <summary>
	/// Set the label field used for labeling.
	/// TODO SAM 2009-07-02 THIS NEEDS TO BE PHASED OUT. </summary>
	/// <param name="label_field"> Name of attribute field to use for labeling. </param>
	public virtual void setLabelField(string label_field)
	{
		if (!string.ReferenceEquals(label_field, null))
		{
			_props.setValue("AttributeTableLabelField", label_field);
		}
	}

	/// <summary>
	/// Set the legend to use with this GeoLayerView. </summary>
	/// <param name="legend"> GRLegend to use. </param>
	public virtual void setLegend(GRLegend legend)
	{
		_legend = legend;
	}

	/// <summary>
	/// Sets the value for missing data for this layer.  Default is -999.0. </summary>
	/// <param name="value"> the value for missing data for this layer.  </param>
	public virtual void setMissingDoubleValue(double value)
	{
		__missingDouble = value;
	}

	/// <summary>
	/// Sets the value that will replace missing data when the data are drawn on 
	/// the display.  This is used so that if missing data are found, the result can
	/// be adjusted to not be a large negative bar.  Default is -999.0. </summary>
	/// <param name="value"> the value to replace missing data values with when drawn. </param>
	public virtual void setMissingDoubleReplacementValue(double value)
	{
		__missingDoubleReplacement = value;
	}

	/// <summary>
	/// Sets the name of the layer. </summary>
	/// <param name="name"> the name of the layer. </param>
	public virtual void setName(string name)
	{
		__layerName = name;
	}

	/// <summary>
	/// Set the properties for the layer view.  This is protected because currently it is intended for use only within
	/// the package during object initialization.
	/// </summary>
	protected internal virtual void setProperties(PropList props)
	{
		_props = props;
	}

	/// <summary>
	/// Sets up the array for keeping track of whether animation fields are visible or not.
	/// </summary>
	private void setupAnimationFieldVisible()
	{
		int num = _layer.getAttributeTableFieldCount();
		__animationFieldVisible = new bool[num];
		for (int i = 0; i < num; i++)
		{
			__animationFieldVisible[i] = true;
		}
	}

	/// <summary>
	/// Set the GeoViewJComponent used with this GeoLayerView. </summary>
	/// <param name="view"> GeoViewJComponent to use with this GeoLayerView. </param>
	public virtual void setView(GeoViewJComponent view)
	{
		if (view != null)
		{
			_view = view;
		}
	}

	}

}