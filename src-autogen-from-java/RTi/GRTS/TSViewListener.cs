﻿using System.Collections.Generic;

// TSViewListener - listener to communicate between TS Views

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
// TSViewListener - listener to communicate between TS Views
// ----------------------------------------------------------------------------
// History:
//
// 24 Jun 1999	Steven A. Malers, RTi	Implemented code.
// 2002-01-17	SAM, RTi		Change name from TSViewListener to
//					TSViewListenerNoSwing to allow support
//					for Swing components.  Update the
//					methods to pass TSGraphNoSwing that the
//					event occurred in (this supports
//					multiple graphs on the same
//					TSGraphCanvas and will allow the data
//					limits for other drawing areas to be
//					reset).  Change so that GRShape is used
//					rather than GRLimits (similar to
//					RTi.GIS.GeoView.GeoViewListenerNoSwing).
// ===============================
// 2002-11-11	SAM, RTi		Copy AWT version and convert to Swing.
// 2003-06-03	SAM, RTi		Minor update - now using full Swing
//					TSGraphJComponent.
// ----------------------------------------------------------------------------

namespace RTi.GRTS
{

	using GRShape = RTi.GR.GRShape;
	using GRPoint = RTi.GR.GRPoint;

	/// <summary>
	/// This interface should be used to capture events from a TSView object (currently only for TSGraphJComponent).
	/// TSView classes will handle any needed state changes (like zooming, etc.).
	/// This interface works in conjunction with the
	/// TSGraphJComponent.setInteractionMode() settings.
	/// </summary>
	public abstract interface TSViewListener
	{
	/// <summary>
	/// TSView will call this method if the TSGraphJComponent mouse tracker is enabled. </summary>
	/// <param name="g"> TSGraph where the event occurred. </param>
	/// <param name="devpt"> Mouse position in device coordinates (in the native device coordinates). </param>
	/// <param name="datapt"> Mouse position in data coordinates. </param>
	void tsViewMouseMotion(TSGraph g, GRPoint devpt, GRPoint datapt);

	/// <summary>
	/// TSGraphJComponent will call this method if the TSGraphJComponent is in
	/// INTERACTION_SELECT mode and the mouse is released.  If the mouse movement is
	/// less than the selected tolerance, a GRPoint may be returned.  Otherwise, a
	/// GRLimits or GRArc can be returned.  <b>This method definition will be better
	/// defined when selects actually do something (e.g., are data points selected or
	/// are time series selected).</b> </summary>
	/// <param name="g"> TSGraph where the event occurred. </param>
	/// <param name="dev_shape"> Mouse limits in device coordinates (in the native device coordinates). </param>
	/// <param name="data_shape"> Mouse limits in data coordinates. </param>
	/// <param name="selected"> list of selected data (TSGraphJComponent uses list of TS). </param>
	void tsViewSelect(TSGraph g, GRShape dev_shape, GRShape data_shape, IList<object> selected);

	/// <summary>
	/// TSGraphJComponent will call this method if the TSGraphJComponent is in
	/// INTERACTION_ZOOM mode and the mouse is pressed, dragged, and released. </summary>
	/// <param name="g"> TSGraph where the event occurred. </param>
	/// <param name="dev_shape"> Mouse limits in device coordinates (in the native device coordinates). </param>
	/// <param name="data_shape"> Mouse limits in data coordinates. </param>
	void tsViewZoom(TSGraph g, GRShape dev_shape, GRShape data_shape);

	}

}