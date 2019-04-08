using System.Text;

// GeoJSONGeometryFormatter - GeoJSON formatter for geometry

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
	using GRPoint = RTi.GR.GRPoint;
	using GRPolygon = RTi.GR.GRPolygon;
	using GRShape = RTi.GR.GRShape;

	/// <summary>
	/// This class formats shapes into GeoJSON feature text.  To use, declare an instance of this class
	/// and then call formatGeoJSON() to format a GRShape.
	/// </summary>
	public class GeoJSONGeometryFormatter
	{

	/// <summary>
	/// Number of spaces for indent levels, for nice formatting.
	/// </summary>
	private string indent = "";

	/// <summary>
	/// Construct a GeoJSONGeometryFormatter. </summary>
	/// <param name="indent"> number of spaces to indent for nice formatting </param>
	public GeoJSONGeometryFormatter(int indent)
	{
		StringBuilder sb = new StringBuilder();
		for (int i = 0; i < indent; i++)
		{
			sb.Append(" ");
		}
		this.indent = sb.ToString();
	}

	/// <summary>
	/// Format a GRPoint as a GeoJSON geometry string. </summary>
	/// <param name="point"> point object to process </param>
	/// <param name="niceFormat"> if true format with newlines to be more readable </param>
	private string formatPoint(GRPoint point, bool niceFormat, string lineStart)
	{
		StringBuilder b = new StringBuilder();
		string nl = "";
		string prefix0 = "";
		string prefix1 = "";
		if (niceFormat)
		{
			nl = "\n";
			if (!string.ReferenceEquals(lineStart, null))
			{
				prefix0 = lineStart;
				prefix1 = lineStart + this.indent;
			}
		}
		b.Append("{" + nl);
		b.Append(prefix1);
		b.Append("\"type\": \"Point\"," + nl);
		b.Append(prefix1);
		b.Append("\"coordinates\": [");
		b.Append(point.x);
		b.Append(", ");
		b.Append(point.y);
		b.Append("]" + nl);
		b.Append(prefix0);
		b.Append("}" + nl);
		return b.ToString();
	}

	/// <summary>
	/// Format a GRPolygon as a GeoJSON geometry string. </summary>
	/// <param name="polygon"> polygon object to process </param>
	/// <param name="niceFormat"> if true format with newlines to be more readable </param>
	private string formatPolygon(GRPolygon polygon, bool niceFormat, string lineStart)
	{
		StringBuilder b = new StringBuilder();
		string nl = "";
		string prefix0 = "";
		string prefix1 = "";
		string prefix2 = "";
		if (niceFormat)
		{
			nl = "\n";
			if (!string.ReferenceEquals(lineStart, null))
			{
				prefix0 = lineStart;
				prefix1 = lineStart + this.indent;
				prefix2 = prefix1 + this.indent;
			}
		}
		b.Append("{" + nl);
		b.Append(prefix1);
		b.Append("\"type\": \"Polygon\"," + nl);
		b.Append(prefix1);
		b.Append("\"coordinates\": [" + nl);
		int npts0 = polygon.npts - 1;
		for (int i = 0; i < polygon.npts; i++)
		{
			if (i == 0)
			{
				b.Append(prefix2);
				b.Append("[ ");
			}
			b.Append("[");
			b.Append(polygon.pts[i].x);
			b.Append(", ");
			b.Append(polygon.pts[i].y);
			b.Append("]");
			if (i != npts0)
			{
				b.Append(", ");
			}
			if ((i != 0) && (i != npts0) && niceFormat && ((i % 10) == 0))
			{
				// Maximum of 10 points on a line
				b.Append(nl);
				if ((i != 0) && (i != npts0))
				{
					b.Append(prefix2);
				}
			}
			if (i == npts0)
			{
				b.Append(" ]"); // Close array of points
			}
		}
		b.Append(nl + prefix1 + "]" + nl); // Close coordinates
		b.Append(prefix0);
		b.Append("}" + nl);
		return b.ToString();
	}

	/// <summary>
	/// Format a GRShape into a GeoJSON feature which is the text after the GeoJSON "geometry:" text.
	/// Recognized geometry types include:
	/// <ul>
	/// <li> GRPoint
	/// <li> GRPolygon</li>
	/// </ul> </summary>
	/// <param name="shape"> shape to format </param>
	/// <param name="niceFormat"> use newline at end of lines (improves readability but increases file size slightly) </param>
	/// <param name="lineStart"> a string with spaces to insert at the front of lines, to indent the geometry for nice formatting </param>
	/// <exception cref="UnrecognizedGeometryException"> if the geometry is not recognized. </exception>
	public virtual string format(GRShape shape, bool niceFormat, string lineStart)
	{
		if (shape is GRPoint)
		{
			return formatPoint((GRPoint)shape, niceFormat, lineStart);
		}
		else if (shape is GRPolygon)
		{
			return formatPolygon((GRPolygon)shape, niceFormat, lineStart);
		}
		else
		{
			throw new UnrecognizedGeometryException("Unrecognized geometry type " + shape.GetType().Name + " - don't know how to format GeoJSON");
		}
	}

	}

}