using System.Collections.Generic;

// GeoViewListener - listener for GeoView map events

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

	/// <summary>
	/// This interface should be used to capture events from a GeoView. 
	/// GeoView itself will handle any needed state changes (like zooming, etc.).
	/// This interface works in conjunction with the GeoView.setInteractionMode()
	/// settings.  For select methods, GeoView returns a list of GeoRecord, even if
	/// the select is a point.  This will allow selections based on proximity, etc.
	/// </summary>
	public abstract interface GeoViewListener
	{
	/// <summary>
	/// GeoView will call this method for GeoLayerView where the "Label" property is
	/// set to "UsingGeoViewListener".  The implemented method should format the label
	/// according to the settings in the application.  The label string will then be
	/// used to label symbols, etc.  The first listener that returns a non-null string
	/// is assumed to be returning a valid label. </summary>
	/// <param name="record"> GeoRecord containing shape and layer of shape that is being
	/// labeled.  This object is reused; therefore, make a copy of data to guarantee persistence. </param>
	string geoViewGetLabel(GeoRecord record);

	/// <summary>
	/// GeoView will call this method if the GeoView is in INTERACTION_INFO mode
	/// and the mouse is released (the press point is returned if the mouse movement
	/// is less than the select tolerance). </summary>
	/// <param name="devpt"> Mouse position in device coordinates (in the native device coordinates). </param>
	/// <param name="datapt"> Mouse position in data coordinates. </param>
	/// <param name="selected"> list of selected data (GeoView uses Vector of GeoRecord). </param>
	// TODO (JTS - 2003-05-06) this one might need to be removed (from the old code)
	void geoViewInfo(GRPoint devpt, GRPoint datapt, IList<GeoRecord> selected);

	/// <summary>
	/// GeoView will call this method if the GeoView is in INTERACTION_INFO mode
	/// and the mouse is released (the press point is returned if the mouse movement
	/// is less than the select tolerance; otherwise the extent of the select region is used). </summary>
	/// <param name="dev_shape"> Mouse position (GRPoint, GRLimits, etc) in device coordinates
	/// (in the native device coordinates). </param>
	/// <param name="data_shape"> Mouse position in data coordinates. </param>
	/// <param name="selected"> list of selected data (GeoView uses Vector of GeoRecord). </param>
	void geoViewInfo(GRShape dev_shape, GRShape data_shape, IList<GeoRecord> selected);

	/// <summary>
	/// GeoView will call this method if the GeoView is in INTERACTION_INFO mode
	/// and the mouse is released (the limits are returned if the mouse movement
	/// is greater than the select tolerance). </summary>
	/// <param name="devlimits"> Mouse limits in device coordinates (in the native device coordinates). </param>
	/// <param name="datalimits"> Mouse limits in data coordinates. </param>
	/// <param name="selected"> list of selected data (GeoView uses Vector of GeoRecord). </param>
	void geoViewInfo(GRLimits devlimits, GRLimits datalimits, IList<GeoRecord> selected);

	/// <summary>
	/// GeoView will call this method if the GeoView mouse tracker is enabled. </summary>
	/// <param name="devpt"> Mouse position in device coordinates (in the native device coordinates). </param>
	/// <param name="datapt"> Mouse position in data coordinates. </param>
	void geoViewMouseMotion(GRPoint devpt, GRPoint datapt);

	/// <summary>
	/// GeoView will call this method if the GeoView is in INTERACTION_SELECT mode
	/// and the mouse is released (the press point is returned if the mouse movement
	/// is less than the select tolerance). </summary>
	/// <param name="devpt"> Mouse position in device coordinates (in the native device coordinates). </param>
	/// <param name="datapt"> Mouse position in data coordinates. </param>
	/// <param name="selected"> list of selected data (GeoView uses Vector of GeoRecord). </param>
	/// <param name="append"> Indicates whether the results should be appended to a previous select. </param>
	// TODO (JTS - 2003-05-06) this one is from the old code, it might be able to be removed
	void geoViewSelect(GRPoint devpt, GRPoint datapt, IList<GeoRecord> selected, bool append);

	/// <summary>
	/// GeoView will call this method if the GeoView is in INTERACTION_SELECT mode
	/// and the mouse is released (the limits are returned if the mouse movement
	/// is greater than the select tolerance). </summary>
	/// <param name="devlimits"> Mouse limits in device coordinates (in the native device coordinates). </param>
	/// <param name="datalimits"> Mouse limits in data coordinates. </param>
	/// <param name="selected"> list of selected data (GeoView uses Vector of GeoRecord). </param>
	/// <param name="append"> Indicates whether the results should be appended to a previous select. </param>
	// TODO (JTS - 2003-05-06) this one is from the old code, it might be able to be removed
	void geoViewSelect(GRLimits devlimits, GRLimits datalimits, IList<GeoRecord> selected, bool append);

	/// <summary>
	/// GeoView will call this method if the GeoView is in INTERACTION_SELECT mode
	/// and the mouse is released.  If the mouse movement is less than the selected
	/// tolerance, a GRPoint may be returned.  Otherwise, a GRLimits or GRArc can be returned. </summary>
	/// <param name="dev_shape"> Mouse position (GRPoint, GRLimits, etc.) in device coordinates
	/// (in the native device coordinates). </param>
	/// <param name="data_shape"> Mouse position in data coordinates. </param>
	/// <param name="selected"> list of selected data (GeoView uses Vector of GeoRecord). </param>
	/// <param name="append"> Indicates whether the results should be appended to a previous select. </param>
	void geoViewSelect(GRShape dev_shape, GRShape data_shape, IList<GeoRecord> selected, bool append);

	/// <summary>
	/// GeoView will call this method if the GeoView is in INTERACTION_ZOOM mode
	/// and the mouse is pressed, dragged, and released. </summary>
	/// <param name="devlimits"> Mouse limits in device coordinates (in the native device coordinates). </param>
	/// <param name="datalimits"> Mouse limits in data coordinates. </param>
	// TODO (JTS - 2003-05-06) this one is from the old code, it might be able to be removed
	void geoViewZoom(GRLimits devlimits, GRLimits datalimits);

	/// <summary>
	/// GeoView will call this method if the GeoView is in INTERACTION_ZOOM mode
	/// and the mouse is pressed, dragged, and released. </summary>
	/// <param name="dev_shape"> Mouse limits in device coordinates (in the native device coordinates). </param>
	/// <param name="data_shape"> Mouse limits in data coordinates. </param>
	void geoViewZoom(GRShape dev_shape, GRShape data_shape);

	}

}