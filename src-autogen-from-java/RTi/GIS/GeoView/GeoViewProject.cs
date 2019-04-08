﻿using System;
using System.Collections.Generic;

// GeoViewProject - class to read, write, and manipulate GeoViewProject files

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
// GeoViewProject - class to read, write, and manipulate GeoViewProject files.
// ----------------------------------------------------------------------------
// History:
//
// 2001-10-10	Steven A. Malers, RTi	Add support for SymbolStyle property,
//					to replace SymbolType.
// 2001-11-27	SAM, RTi		Add ProjectAtRead property.
// 2001-12-04	SAM, RTi		Update to use Swing.
// 2002-01-08	SAM, RTi		Change GeoViewCanvas to
//					GeoViewJComponent.
// 2002-04-04	SAM, RTi		Update to recognize InitialExtent and
//					MaximumExtent.
// 2002-07-23	SAM, RTi		Change GRSymbol "pointSymbol" methods
//					to "style".
// ----------------------------------------------------------------------------
// 2003-05-06	J. Thomas Sapienza, RTi	* IOUtil.getFileSeparator replaced with
//					  File.separator
//					* Made class up-to-date compared with
//					  non-Swing code
// 2004-10-19	JTS, RTi		The geoview display now is explicitly
//					refreshed after all layer views are 
//					added in order to make sure the legend
//					is drawn correctly on the map.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.GIS.GeoView
{

	using GRColor = RTi.GR.GRColor;
	using GRLegend = RTi.GR.GRLegend;
	using GRSymbol = RTi.GR.GRSymbol;
	using GRText = RTi.GR.GRText;

	using IOUtil = RTi.Util.IO.IOUtil;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using StopWatch = RTi.Util.Time.StopWatch;

	/// <summary>
	/// The GeoViewProject class reads a GeoViewProject file and handles instantiation
	/// of GeoLayerViews.  Methods are also available to add the data to a GeoViewPanel
	/// display.  This class will evolve as resources allow to support all the proposed
	/// GeoView properties for reading and writing.
	/// </summary>
	public class GeoViewProject
	{

	private PropList _proplist = null;

	/// <summary>
	/// Construct from a GeoView project file (.gvp).  The project file is read into a
	/// PropList, which can be further processed with addToGeoView(). </summary>
	/// <param name="filename"> Name of gvp file to process. </param>
	/// <exception cref="Exception"> if there is an error processing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public GeoViewProject(String filename) throws Exception
	public GeoViewProject(string filename)
	{ // Just read in as a simple PropList...
		_proplist = new PropList("GeoViewProject");
		_proplist.setPersistentName(filename);
		_proplist.readPersistent();
	}

	/// <summary>
	/// Look through the project and add any layers that are appropriate for the
	/// GeoView.  This method is typically called when a user selects a GeoViewProject
	/// file or the file is automatically selected during program startup.  The layers
	/// are read into memory.  Later might need a way to share
	/// layers already in memory between layer views but for now assume most layer views will not share data. </summary>
	/// <param name="geoview"> GeoViewJComponent to add layers to. </param>
	/// <param name="ref_geoview"> Reference GeoViewJComponent to add layers to. </param>
	/// <param name="legend"> GeoViewLegendJPanel to add to. </param>
	public virtual void addToGeoView(GeoViewJComponent geoview, GeoViewJComponent ref_geoview, GeoViewLegendJTree legend)
	{
	//				GeoViewLegendJPanel legend )
		string routine = "GeoViewProject.addToGeoView";

		legend.emptyTree();

		// Display missing layers as empty layers with a red indicator.  This is helpful when
		// tracking down problems in the data.  If necessary can pass in as a parameter.
		bool displayMissingLayers = true;

		// See if there is a global data directory defined...
		string globalDataHome = _proplist.getValue("GeoView.GeoDataHome");
		if ((string.ReferenceEquals(globalDataHome, null)) || globalDataHome.Equals(".") || globalDataHome.Equals(""))
		{
			// No home is specified so get the directory from the proplist persistent name.
			// Note this will only work if the GeoViewProject was read from a file in the first place.
			// If not, then layers are probably being added through a GUI and the paths will be absolute
			// (until the issue of converting absolute paths to relative is attacked)...
			globalDataHome = ".";
			try
			{
				File f = new File(_proplist.getPersistentName());
				globalDataHome = f.getParent();
				if (!IOUtil.fileExists(globalDataHome))
				{
					globalDataHome = ".";
				}
			}
			catch (Exception)
			{
				// Ignore for now until we figure out how often it occurs.
			}
		}
		else
		{
			// Need to see if the global data home is a relative path.  If so then it needs to be appended
			// to the project file directory.
			File f2 = new File(globalDataHome);
			if (!globalDataHome.regionMatches(true,0,"http:",0,5) && !f2.isAbsolute())
			{
				// Relative path that needs to append on the home of the GVP file...
				File f = new File(_proplist.getPersistentName());
				// Reset the data home...
				globalDataHome = f.getParent() + File.separator + globalDataHome;
			}
		}
		Message.printStatus(2, routine, "Global data home is \"" + globalDataHome + "\"");
		// Loop requesting GeoLayer information
		string geoLayerFile;
		PropList layerViewProps = null;
		GeoLayerView layerView = null;
		string propValue = null;
		// Global GeoView properties...
		propValue = _proplist.getValue("GeoView.Color");
		if (!string.ReferenceEquals(propValue, null))
		{
			GRColor grc = GRColor.parseColor(propValue);
			geoview.setBackground(grc);
			if (ref_geoview != null)
			{
				ref_geoview.setBackground(grc);
			}
		}
		propValue = _proplist.getValue("GeoView.Projection");
		if (!string.ReferenceEquals(propValue, null))
		{
			try
			{
				geoview.setProjection(GeoProjection.parseProjection(propValue));
				if (ref_geoview != null)
				{
					ref_geoview.setProjection(GeoProjection.parseProjection(propValue));
				}
			}
			catch (Exception)
			{
				// Unknown projection...
				geoview.setProjection(new UnknownProjection());
			}
		}
		int ivisible = 0; // Count of visible layers (equal to i if no layers are skipped
		StopWatch timer = null;
		legend.setProjectNodeText(_proplist.getPersistentName());
		for (int i = 1; ; i++)
		{
			// Get the layer source...
			geoLayerFile = _proplist.getValue("GeoLayerView " + i + ".GeoLayer");
			if (string.ReferenceEquals(geoLayerFile, null))
			{
				// This is used to break out of the read.  Once a break
				// in the layer view numbers occurs, assume the end of the list is encountered.
				break;
			}
			// Make sure the layer view is supposed to be included...
			propValue = _proplist.getValue("GeoLayerView " + i + ".SkipLayerView");
			if ((!string.ReferenceEquals(propValue, null)) && propValue.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				Message.printStatus(2, routine, "Skipping GeoLayerView \"" + geoLayerFile + "\" because of SkipLayerView=true in project file");
				continue;
			}
			// Prepend the global directory if necessary...
			if (!IOUtil.fileReadable(geoLayerFile) && IOUtil.fileReadable(globalDataHome + File.separator + geoLayerFile))
			{
				geoLayerFile = globalDataHome + File.separator + geoLayerFile;
			}
			Message.printStatus(2, routine, "Path to layer file is \"" + geoLayerFile + "\"");
			// Add the layer...
			timer = new StopWatch();
			timer.start();
			//_status_TextField.setText ( "Adding layer..." );
			// Set properties for the layer view...
			layerViewProps = new PropList("forGeoLayerView");
			// Save the position so we can get to other properties later
			layerViewProps.set("Number", "" + i);
			layerViewProps.set("Label", "UsingGeoViewListener");
			propValue = _proplist.getValue("GeoLayerView " + i + ".ReadAttributes");
			if (!string.ReferenceEquals(propValue, null))
			{
				layerViewProps.set("ReadAttributes=" + propValue);
			}
			propValue = _proplist.getValue("GeoLayerView " + i + ".ReadAttributes");
			if (!string.ReferenceEquals(propValue, null))
			{
				layerViewProps.set("Name=" + propValue);
			}
			try
			{
				// Increment the count (will therefore be 1 for the first layer)...
				++ivisible;
				// Read the layer and create a layer view.  The legend is initialized to default values
				// without checking the project and will be further initialized in setLayerViewProperties().
				try
				{
					layerView = new GeoLayerView(geoLayerFile, layerViewProps, ivisible);
				}
				catch (Exception e1)
				{
					Message.printWarning(3, routine, "Error reading layer (" + e1 + ").");
					Message.printWarning(3, routine, e1);
					if (displayMissingLayers)
					{
						// Create an empty layer view
						Message.printStatus(2, routine, "Creating an empty layer as a placeholder.");
						layerView = GeoViewUtil.newLayerView(geoLayerFile, layerViewProps, ivisible);
					}
					else
					{
						// Re-throw the original exception and handle below
						Message.printStatus(2, routine, "Layer file \"" + geoLayerFile + "\" could not be read." + "  Initializing blank layer as placeholder.");
						layerView = GeoViewUtil.newLayerView(geoLayerFile, layerViewProps, ivisible);
						throw e1;
					}
				}
				// If we get to here, the layer was read so it is OK to leave "ivisible" as it was set above.
				// Set the view properties after reading the layer data (the index is used to look in the GVP file
				// so don't use "ivisible")...
				setLayerViewProperties(layerView, i);
			}
			catch (Exception e)
			{
				Message.printWarning(2, routine, "Unexpected error adding layer for \"" + geoLayerFile + "\" (" + e + ")");
				Message.printWarning(3, routine, e);
				if (displayMissingLayers)
				{
					// Create an empty layer view
					Message.printStatus(2, routine, "Layer file \"" + geoLayerFile + "\" could not be read." + "  Initializing blank layer as placeholder.");
					layerView = GeoViewUtil.newLayerView(geoLayerFile, layerViewProps, ivisible);
				}
				else
				{
					// The layer load was unsuccessful so decrement the count...
					--ivisible;
					// Go to next layer...
					continue;
				}
			}

			// Now add the layer view to the view...
			geoview.addLayerView(layerView);
			// If a reference geoview, only add if layer has been tagged as a reference layer...
			propValue = _proplist.getValue("GeoLayerView " + i + ".ReferenceLayer");
			if ((ref_geoview != null) && (!string.ReferenceEquals(propValue, null)) && propValue.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				ref_geoview.addLayerView(layerView);
			}
			timer.stop();
			Message.printStatus(1, routine,"Reading \"" + geoLayerFile + "\" took " + StringUtil.formatString(timer.getSeconds(),"%.2f") + " seconds.");
			Runtime runtime = Runtime.getRuntime();
			Message.printStatus(1, routine, "JVM Total memory = " + runtime.totalMemory() + " used memory = " + (runtime.totalMemory() - runtime.freeMemory()) + " free memory = " + runtime.freeMemory());
			runtime = null;
			//_status_TextField.setText ( "Finished adding layer.  Ready.");
			// If we got to here the layer could be added so add to the legend...
			legend.addLayerView(layerView, ivisible);
		}

		// refresh the geoview to ensure that the legend draws correctly
		// with all layers and with all proper limits
		geoview.redraw(true);
	}

	/// <summary>
	/// Clean up for garbage collection. </summary>
	/// <exception cref="Throwable"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~GeoViewProject()
	{
		_proplist = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Determine the number of symbols for a GeoLayerView.  This is accomplished
	/// by checking for a property "Symbol #.#.SymbolStyle".  If
	/// "Symbol #.1.SymbolStyle" is null, then assume 1 symbol. </summary>
	/// <param name="ilayer_view"> GeoLayerView index (starting at 1). </param>
	public virtual int getNumSymbolsForLayerView(int ilayer_view)
	{
		string prop_value;
		int i = 0;
		for (i = 1;; i++)
		{
			prop_value = _proplist.getValue("Symbol " + ilayer_view + "." + i + ".SymbolSize");
			if (string.ReferenceEquals(prop_value, null))
			{
				break;
			}
		}
		if (i == 1)
		{
			return 1;
		}
		else
		{
			return (i - 1);
		}
	}

	/// <summary>
	/// Return the property list associated with the GeoViewProject. </summary>
	/// <returns> the property list associated with the GeoViewProject. </returns>
	public virtual PropList getPropList()
	{
		return _proplist;
	}

	/// <summary>
	/// Set a layer view's properties by evaluating properties in the project file.
	/// It is assumed that default legend information has been set at construction of the legend. </summary>
	/// <param name="layerView"> GeoLayerView to set properties for. </param>
	/// <param name="index"> GeoLayerView index in GVP file (starting with 1). </param>
	private void setLayerViewProperties(GeoLayerView layerView, int index)
	{
		string routine = "GeoViewProject.setLayerViewProperties";
		// Get the layer shape type...
		int layerType = layerView.getLayer().getShapeType();
		// Get the layer...
		GeoLayer layer = layerView.getLayer();
		// Get the legend for the layer view...
		GRLegend legend = layerView.getLegend();
		// Get the default symbol, used to initialize each symbol below...
		GRSymbol defaultSymbol = legend.getSymbol();
		// Determine how many symbols will be used...
		int nsymbols = getNumSymbolsForLayerView(index);
		legend.setNumberOfSymbols(nsymbols);
		// Properties that apply to the entire layer...	
		// Get the projection for the layer...
		string propValue = _proplist.getValue("GeoLayerView " + index + ".Projection");
		if (!string.ReferenceEquals(propValue, null))
		{
			try
			{
				layer.setProjection(GeoProjection.parseProjection(propValue));
			}
			catch (Exception e)
			{
				Message.printWarning(2, routine, "Unrecognized projection \"" + propValue + "\" for GeoLayerView " + index);
				// Unknown projection...
				layer.setProjection(new UnknownProjection());
				Message.printWarning(2, routine, e);
			}
		}
		// Also set the application layer data if available...
		propValue = _proplist.getValue("GeoLayerView " + index + ".AppLayerType");
		if (!string.ReferenceEquals(propValue, null))
		{
			if (layer != null)
			{
				layer.setAppLayerType(propValue);
				// Also set in the layer property (debating whether this
				// is a layer or layer view data item).
				layerView.getPropList().set("AppLayerType=" + propValue);
			}
		}
		propValue = _proplist.getValue("GeoLayerView " + index + ".AppJoinField");
		if (!string.ReferenceEquals(propValue, null))
		{
			if (layer != null)
			{
				layerView.getPropList().set("AppJoinField=" + propValue);
			}
		}
		// Project the layer view's data if requested...
		propValue = _proplist.getValue("GeoLayerView " + index + ".ProjectAtRead");
		if (string.ReferenceEquals(propValue, null))
		{
			// Try the global property...
			propValue = _proplist.getValue("GeoView.ProjectAtRead");
		}
		if ((!string.ReferenceEquals(propValue, null)) && propValue.Equals("true", StringComparison.OrdinalIgnoreCase))
		{
			try
			{
				propValue = _proplist.getValue("GeoView.Projection");
				if (!string.ReferenceEquals(propValue, null))
				{
					Message.printStatus(1, routine, "Projecting to common projection...");
					layer.project(GeoProjection.parseProjection(propValue));
				}
			}
			catch (Exception)
			{
				Message.printWarning(2, routine, "Unable to parse projection \"" + propValue + "\"");
			}
		}
		// Set the name in the legend...
		propValue = _proplist.getValue("GeoLayerView " + index + ".Name");
		if (!string.ReferenceEquals(propValue, null))
		{
			legend.setText(propValue);
		}
		// Now loop through the symbols for the layer...
		GRSymbol symbol = null;
		IList<string> tokens = null;
		for (int isym = 0; isym < nsymbols; isym++)
		{
			// Get the symbol...
			symbol = new GRSymbol(defaultSymbol.getType(), defaultSymbol.getStyle(), defaultSymbol.getColor(), defaultSymbol.getOutlineColor(), defaultSymbol.getSize());
			// Transfer default symbol information...
			// Get the label information...
			// New style...
			propValue = _proplist.getValue("Symbol " + index + "." + (isym + 1) + ".LabelField");
			// Old style...
			if (string.ReferenceEquals(propValue, null))
			{
				propValue = _proplist.getValue("GeoLayerView " + index + ".LabelField");
			}
			if (!string.ReferenceEquals(propValue, null))
			{
				symbol.setLabelField(propValue);
			}
			// New style...
			propValue = _proplist.getValue("Symbol " + index + "." + (isym + 1) + ".LabelFormat");
			// Old style...
			if (string.ReferenceEquals(propValue, null))
			{
				propValue = _proplist.getValue("GeoLayerView " + index + ".LabelFormat");
			}
			if (!string.ReferenceEquals(propValue, null))
			{
				symbol.setLabelFormat(propValue);
			}
			// New style...
			propValue = _proplist.getValue("Symbol " + index + "." + (isym + 1) + ".LabelPosition");
			// Old style...
			if (string.ReferenceEquals(propValue, null))
			{
				propValue = _proplist.getValue("GeoLayerView " + index + ".LabelPosition");
			}
			if (!string.ReferenceEquals(propValue, null))
			{
				try
				{
					symbol.setLabelPosition(GRText.parseTextPosition(propValue));
				}
				catch (Exception)
				{
					// Unknown position...
					symbol.setLabelPosition(GRText.RIGHT | GRText.CENTER_Y);
				}
			}
			// Symbol color is always a property.  Depending on the
			// classification, more than one color may be specified...
			// Determine the classification type for symbols...
			// New style...
			propValue = _proplist.getValue("Symbol " + index + "." + (isym + 1) + ".SymbolClassification");
			// Old style...
			if (string.ReferenceEquals(propValue, null))
			{
				propValue = _proplist.getValue("GeoLayerView " + index + ".SymbolClassification");
			}
			if (!string.ReferenceEquals(propValue, null))
			{
				symbol.setClassificationType(propValue);
				//Message.printStatus ( 1, "",
				//"SAMX symbol classification " + index + " is " + symbol.getClassificationType() );
			}
			if (symbol.getClassificationType() == GRSymbol.CLASSIFICATION_SINGLE)
			{
				// Simple color for symbol (this is also the default)...
				// New style...
				propValue = _proplist.getValue("Symbol " + index + "." + (isym + 1) + ".Color");
				// Old style...
				if (string.ReferenceEquals(propValue, null))
				{
					propValue = _proplist.getValue("GeoLayerView " + index + ".Color");
				}
				if (!string.ReferenceEquals(propValue, null))
				{
					symbol.setColor(GRColor.parseColor(propValue));
				}
				// Size for symbol...
				// New style...
				propValue = _proplist.getValue("Symbol " + index + "." + (isym + 1) + ".SymbolSize");
				// Old style...
				if (string.ReferenceEquals(propValue, null))
				{
					propValue = _proplist.getValue("GeoLayerView " + index + ".SymbolSize");
				}
				if (!string.ReferenceEquals(propValue, null))
				{
					symbol.setSize(StringUtil.atod(propValue));
				}
			}
			else if (symbol.getClassificationType() == GRSymbol.CLASSIFICATION_SCALED_SYMBOL)
			{
				// Color for symbol may have more than one value...
				// New style...
				propValue = _proplist.getValue("Symbol " + index + "." + (isym + 1) + ".Color");
				// Old style...
				if (string.ReferenceEquals(propValue, null))
				{
					propValue = _proplist.getValue("GeoLayerView " + index + ".Color");
				}
				if (!string.ReferenceEquals(propValue, null))
				{
					// The color property can contain more than one color, separated by ";"...
					IList<string> v = StringUtil.breakStringList(propValue, ";", 0);
					int vsize = v.Count;
					if (vsize == 1)
					{
						symbol.setColor(GRColor.parseColor(propValue));
						symbol.setColor2(GRColor.parseColor(propValue));
					}
					else if (vsize == 2)
					{
						symbol.setColor(GRColor.parseColor(v[0]));
						symbol.setColor2(GRColor.parseColor(v[1]));
					}
				}
				// Size for symbol...
				// New style...
				propValue = _proplist.getValue("Symbol " + index + "." + (isym + 1) + ".SymbolSize");
				// Old style...
				if (string.ReferenceEquals(propValue, null))
				{
					propValue = _proplist.getValue("GeoLayerView " + index + ".SymbolSize");
				}
				if (!string.ReferenceEquals(propValue, null))
				{
					// The size can be specified as a single value
					// or an x and y value, separated by a comma...
					IList<string> v = StringUtil.breakStringList(propValue,",",0);
					int size = 0;
					if (v != null)
					{
						size = v.Count;
					}
					if (size == 1)
					{
						symbol.setSize(StringUtil.atod(v[0]));
					}
					else if (size == 2)
					{
						symbol.setSizeX(StringUtil.atod(v[0]));
						symbol.setSizeY(StringUtil.atod(v[1]));
					}
				}
				// Symbol class field...
				// New style...
				propValue = _proplist.getValue("Symbol " + index + "." + (isym + 1) + ".SymbolClassField");
				// Old style...
				if (string.ReferenceEquals(propValue, null))
				{
					propValue = _proplist.getValue("GeoLayerView " + index + ".SymbolClassField");
				}
				if (!string.ReferenceEquals(propValue, null))
				{
					symbol.setClassificationField(propValue);
				}
			}
			else if (symbol.getClassificationType() == GRSymbol.CLASSIFICATION_CLASS_BREAKS)
			{
				// Need to get the class breaks and colors...
				// The number of colors in the color table should match
				// the number of values in the class break.
				// New style...
				propValue = _proplist.getValue("Symbol " + index + "." + (isym + 1) + ".SymbolClassField");
				// Old style...
				if (string.ReferenceEquals(propValue, null))
				{
					propValue = _proplist.getValue("GeoLayerView " + index + ".SymbolClassField");
				}
				if (!string.ReferenceEquals(propValue, null))
				{
					// Can only specify class breaks if we know which field will be examined from the data...
					symbol.setClassificationField(propValue);
					// Get the color table.  The number of colors for this governs the maximum number of breaks
					// (so they are consistent).  The number of colors and breaks should normally be the same.
					// New style...
					propValue = _proplist.getValue("Symbol " + index + "." + (isym + 1) + ".ColorTable");
					// Old style...
					if (string.ReferenceEquals(propValue, null))
					{
						propValue = _proplist.getValue("GeoLayerView " + index + ".ColorTable");
					}
					int num_classes = 0;
					if (!string.ReferenceEquals(propValue, null))
					{
						IList<string> c = StringUtil.breakStringList(propValue, ";", StringUtil.DELIM_SKIP_BLANKS);
						num_classes = StringUtil.atoi((c[1]).Trim());
						symbol.setColorTable((c[0]).Trim(),num_classes);
					}
					// Get the class breaks.  Examine the classification field to determine whether the
					// array will be double, int, or String...
					double[] d = new double[num_classes];
					// Values are initialized to 0.0 by default...
					// New style...
					propValue = _proplist.getValue("Symbol " + index + "." + (isym + 1) + ".SymbolClassBreaks");
					// Old style...
					if (string.ReferenceEquals(propValue, null))
					{
						propValue = _proplist.getValue("GeoLayerView " + index + ".SymbolClassBreaks");
					}
					if (!string.ReferenceEquals(propValue, null))
					{
						IList<string> c = StringUtil.breakStringList(propValue,",", StringUtil.DELIM_SKIP_BLANKS);
						// For now always use double...
						for (int i = 0; i < num_classes; i++)
						{
							d[i] = StringUtil.atod((c[i]).Trim());
						}
						symbol.setClassificationData(d, true);
					}
				}
			}
			else
			{
				// GRSymbol.CLASSIFICATION_UNIQUE
				// Need to do some work to search the data for unique values...
			}
			if ((layerType == GeoLayer.POINT) || (layerType == GeoLayer.POINT_ZM) || (layerType == GeoLayer.MULTIPOINT))
			{
				// Symbol type...
				// Old convention...
				propValue = _proplist.getValue("GeoLayerView " + index + ".SymbolType");
				if (!string.ReferenceEquals(propValue, null))
				{
					Message.printWarning(2, routine, "The SymbolType GeoView project property is obsolete.  Use SymbolStyle.");
					try
					{
						symbol.setStyle(GRSymbol.toInteger(propValue));
					}
					catch (Exception)
					{
						symbol.setStyle(GRSymbol.SYM_PLUS);
					}
				}
				// Newer convention (need to also support for other shape types)...
				// New style...
				propValue = _proplist.getValue("Symbol " + index + "." + (isym + 1) + ".SymbolStyle");
				// Old style...
				if (string.ReferenceEquals(propValue, null))
				{
					propValue = _proplist.getValue("GeoLayerView " + index + ".SymbolStyle");
				}
				if (!string.ReferenceEquals(propValue, null))
				{
					try
					{
						symbol.setStyle(GRSymbol.toInteger(propValue));
					}
					catch (Exception)
					{
						symbol.setStyle(GRSymbol.SYM_PLUS);
					}
				}
			}
			else if ((layerType == GeoLayer.LINE) || (layerType == GeoLayer.POLYLINE_ZM))
			{
			}
			else if ((layerType == GeoLayer.POLYGON) || (layerType == GeoLayer.GRID))
			{
				// New style...
				propValue = _proplist.getValue("Symbol " + index + "." + (isym + 1) + ".OutlineColor");
				// Old style...
				if (string.ReferenceEquals(propValue, null))
				{
					propValue = _proplist.getValue("GeoLayerView " + index + ".OutlineColor");
				}
				if (!string.ReferenceEquals(propValue, null))
				{
					symbol.setOutlineColor(GRColor.parseColor(propValue));
				}
				// New style...
				propValue = _proplist.getValue("Symbol " + index + "." + (isym + 1) + ".IgnoreDataOutside");
				// Old style...
				if (string.ReferenceEquals(propValue, null))
				{
					propValue = _proplist.getValue("GeoLayerView " + index + ".IgnoreDataOutside");
				}
				if (!string.ReferenceEquals(propValue, null))
				{
					layerView.getPropList().set("IgnoreDataOutside=" + propValue);
				}
				// Get the symbol style.  If "Transparent", then the style should be specified:
				//
				// SymbolStyle = Transparent,127
				//
				// Newer convention (need to also support for other shape types)...
				// New style...
				propValue = _proplist.getValue("Symbol " + index + "." + (isym + 1) + ".SymbolStyle");
				// Old style...
				if (string.ReferenceEquals(propValue, null))
				{
					propValue = _proplist.getValue("GeoLayerView " + index + ".SymbolStyle");
				}
				// Default fill is FILL_SOLID but need to see if transparent...
				if ((!string.ReferenceEquals(propValue, null)) && propValue.regionMatches(true,0,"Transparent",0,11))
				{
					// Parse out...
					tokens = StringUtil.breakStringList(propValue, ",", StringUtil.DELIM_SKIP_BLANKS);
					if (tokens.Count > 1)
					{
						int transparency = StringUtil.atoi(tokens[1]);
						symbol.setTransparency(transparency);
					}
				}
			}
			// Now resave the information...
			legend.setSymbol(isym, symbol);
		}
		// Save the legend...
		layerView.setLegend(legend);
	}

	}

}