// GeoViewAnnotationData - annotation data for map display

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

	/// <summary>
	/// This class provides for data management of GeoViewAnnotationRenderer instances and associated data so
	/// that the information can be used to provide a list of annotations in the GeoView interface and provide
	/// data back to the renderers when the annotations need to be rendered.
	/// </summary>
	public class GeoViewAnnotationData
	{

	/// <summary>
	/// Renderer for the data object.
	/// </summary>
	private GeoViewAnnotationRenderer __annotationRenderer = null;

	/// <summary>
	/// Object that will be rendered.
	/// </summary>
	private object __object = null;

	/// <summary>
	/// Label for the object (displayed in the GeoView).
	/// </summary>
	private string __label = null;

	/// <summary>
	/// Data limits for the rendered object (original data units, unprojected).
	/// </summary>
	private GRLimits __limits = null;

	/// <summary>
	/// Projection for the rendered object (data units).
	/// </summary>
	private GeoProjection __limitsProjection = null;

	/// <summary>
	/// Construct an instance from primitive data. </summary>
	/// <param name="annotationRenderer"> the object that will actually render the annotation (the rendering
	/// may be complex due to domain data) </param>
	/// <param name="object"> the data object to be rendered (domain object) </param>
	/// <param name="label"> the label to be shown on the map and in the annotation legend </param>
	/// <param name="limits"> the limits of the rendered data, to aid in zooming to the annotations (data units) </param>
	/// <param name="limitsProjection"> the projection for the data limits, needed to handle zooming (projection on the
	/// fly for specific objects in the data are handled through GeoRecord information with each object) </param>
	public GeoViewAnnotationData(GeoViewAnnotationRenderer annotationRenderer, object @object, string label, GRLimits limits, GeoProjection limitsProjection)
	{
		__annotationRenderer = annotationRenderer;
		__object = @object;
		__label = label;
		__limits = limits;
		__limitsProjection = limitsProjection;
	}

	/// <summary>
	/// Finalize before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~GeoViewAnnotationData()
	{
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the GeoViewAnnotationRenderer for the data. </summary>
	/// <returns> the GeoViewAnnotationRenderer for the data </returns>
	public virtual GeoViewAnnotationRenderer getGeoViewAnnotationRenderer()
	{
		return __annotationRenderer;
	}

	/// <summary>
	/// Return the label for the object. </summary>
	/// <returns> the label for the object </returns>
	public virtual string getLabel()
	{
		return __label;
	}

	/// <summary>
	/// Return the limits for the object. </summary>
	/// <returns> the limits for the object </returns>
	public virtual GRLimits getLimits()
	{
		return __limits;
	}

	/// <summary>
	/// Return the projection of the data limits. </summary>
	/// <returns> the projection of the data limits </returns>
	public virtual GeoProjection getLimitsProjection()
	{
		return __limitsProjection;
	}

	/// <summary>
	/// Return the object to be rendered. </summary>
	/// <returns> the object to be rendered </returns>
	public virtual object getObject()
	{
		return __object;
	}

	/// <summary>
	/// Return the string representation of the annotation - use the label.
	/// </summary>
	public override string ToString()
	{
		return __label;
	}

	}

}