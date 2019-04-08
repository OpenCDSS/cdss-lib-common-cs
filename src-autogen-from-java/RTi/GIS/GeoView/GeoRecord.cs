// GeoRecord - class to hold an attribute table record and reference to a shape

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
	using GRShape = RTi.GR.GRShape;
	using TableRecord = RTi.Util.Table.TableRecord;

	/// <summary>
	/// This class provides a record for geographic (shape) and associated tabular data.  GeoLayer
	/// and GeoLayerView references are also maintained in case mixed layers are used
	/// in a list.  Projection information must be retrieved from the layer view.
	/// </summary>
	public class GeoRecord
	{

	/// <summary>
	/// Shape for GeoRecord.
	/// </summary>
	protected internal GRShape _shape = null;

	/// <summary>
	/// Table record for GeoRecord.
	/// </summary>
	protected internal TableRecord _record = null;

	/// <summary>
	/// GeoLayer that the record is queried from.
	/// </summary>
	protected internal GeoLayer _layer = null;

	/// <summary>
	/// GeoLayerView that the record is queried from.
	/// </summary>
	protected internal GeoLayerView _layer_view = null;

	/// <summary>
	/// Construct an empty GeoRecord (null shape, table record, GeoLayer, and GeoLayerView).
	/// </summary>
	public GeoRecord()
	{
	}

	/// <summary>
	/// Construct a GeoRecord using the given shape, table record, GeoLayer, and GeoLayerView. </summary>
	/// <param name="shape"> GRShape associated with the record. </param>
	/// <param name="record"> TableRecord associated with the record. </param>
	/// <param name="layer"> GeoLayer associated with the record. </param>
	/// <param name="layer_view"> GeoLayerView associated with the record. </param>
	public GeoRecord(GRShape shape, TableRecord record, GeoLayer layer, GeoLayerView layer_view)
	{
		_layer = layer;
		_layer_view = layer_view;
		_record = record;
		_shape = shape;
	}

	/// <summary>
	/// Construct a GeoRecord using the given shape, table record, and layer. </summary>
	/// <param name="shape"> GRShape associated with the record. </param>
	/// <param name="record"> TableRecord associated with the record. </param>
	/// <param name="layer"> GeoLayer associated with the record. </param>
	public GeoRecord(GRShape shape, TableRecord record, GeoLayer layer)
	{
		_layer = layer;
		_record = record;
		_shape = shape;
	}

	/// <summary>
	/// Return the GeoLayer for the GeoRecord. </summary>
	/// <returns> the layer for the GeoRecord (can be null). </returns>
	public virtual GeoLayer getLayer()
	{
		return _layer;
	}

	/// <summary>
	/// Return the GeoLayerView for the GeoRecord. </summary>
	/// <returns> the layer view for the GeoRecord (can be null). </returns>
	public virtual GeoLayerView getLayerView()
	{
		return _layer_view;
	}

	/// <summary>
	/// Return the shape for the record. </summary>
	/// <returns> the shape for the GeoRecord (can be null). </returns>
	public virtual GRShape getShape()
	{
		return _shape;
	}

	/// <summary>
	/// Return the TableRecord for the record. </summary>
	/// <returns> the table record for the GeoRecord (can be null). </returns>
	public virtual TableRecord getTableRecord()
	{
		return _record;
	}

	/// <summary>
	/// Set the layer for the GeoRecord (can be null). </summary>
	/// <param name="layer"> GeoLayer for record. </param>
	public virtual void setLayer(GeoLayer layer)
	{
		_layer = layer;
	}

	/// <summary>
	/// Set the layer view for the GeoRecord (can be null). </summary>
	/// <param name="layer_view"> GeoLayerView for record. </param>
	public virtual void setLayerView(GeoLayerView layer_view)
	{
		_layer_view = layer_view;
	}

	/// <summary>
	/// Set the shape for the GeoRecord (can be null). </summary>
	/// <param name="shape"> GRShape for the record. </param>
	public virtual void setShape(GRShape shape)
	{
		_shape = shape;
	}

	/// <summary>
	/// Set the table record for the GeoRecord (can be null). </summary>
	/// <param name="record"> TableRecord for the record. </param>
	public virtual void setTableRecord(TableRecord record)
	{
		_record = record;
	}

	}

}