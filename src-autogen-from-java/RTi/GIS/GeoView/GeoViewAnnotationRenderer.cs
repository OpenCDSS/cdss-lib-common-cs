﻿// GeoViewAnnotationRenderer - render an annotation

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
	/// <summary>
	/// Objects that implement this interface can be added to the GeoViewJPanel to annotate the map by drawing
	/// additional objects on top of the map.  This is useful, for example, to highlight information beyond
	/// a normal selection.  For example, the annotation might show related information.
	/// </summary>
	public interface GeoViewAnnotationRenderer
	{

		/// <summary>
		/// This method will be called by the GeoViewJComponent when rendering the map, passing back the
		/// object from getAnnotationObject(). </summary>
		/// <param name="geoviewJComponent"> the map object </param>
		/// <param name="objectToRender"> the object to render as an annotation on the map </param>
		/// <param name="label"> the string that is used to label the annotation on the map </param>
		void renderGeoViewAnnotation(GeoViewJComponent geoviewJComponent, object objectToRender, string label);

		/// <summary>
		/// Return the object to render.
		/// </summary>
		//public Object getAnnotationObject ();

		/// <summary>
		/// Return the label for the object to render.  This will be listed in the GeoViewPanel.
		/// </summary>
		//public String getAnnotationLabel ();
	}

}