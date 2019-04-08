using System;
using System.Collections.Generic;

// GeoProjection - projection base class

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
// GeoProjection - projection base class
// ----------------------------------------------------------------------------
// History:
//
// 2001-10-01	Steven A. Malers, RTi	Add getProjectionNames() method.  Lists
//					of projections are handled differently
//					than other code.  Instead of hard-coding
//					static information, known projections
//					names are returned dynamically.  This
//					allows new projections to be defined in
//					the future and minimizes static data
//					space.
// 2001-10-08	SAM, RTi		Review javadoc.  Add copy constructor to
//					help with C++.
// 2001-11-27	SAM, RTi		Update projectShape() to handle shapes
//					other than GRPolygon.  Fix bug where
//					projectShape() needed to always reuse
//					the shape when calling the projection
//					methods.  Add asinz(), mlfn(),
//					adjust_lon(), e0fn(), e1fn(), e2fn(),
//					e3fn(), sign() methods consistent
//					with the GCTP package to support
//					UTMProjection and others.  Add _zone,
//					_units, _datam.
// 2001-12-07	SAM, RTi		Update projectShape() to handle
//					GRPolylineList and GRPolypoint.  This
//					should cover most common needs.
// 2001-12-09	SAM, RTi		Add getKilometersForUnit() to allow
//					scale conversions and output.  Support
//					GRArc in projectShape().  Add a static
//					global geographic_projection instance
//					to optimize code (often geographic
//					projections are needed during
//					conversions).
// 2004-10-27	J. Thomas Sapienza, RTi	Implements Cloneable.
// 2005-04-27	JTS, RTi		Added all member variables to finalize()
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.GIS.GeoView
{
	using Math;

	using GRArc = RTi.GR.GRArc;
	using GRLimits = RTi.GR.GRLimits;
	using GRPoint = RTi.GR.GRPoint;
	using GRPointZM = RTi.GR.GRPointZM;
	using GRPolygon = RTi.GR.GRPolygon;
	using GRPolygonList = RTi.GR.GRPolygonList;
	using GRPolyline = RTi.GR.GRPolyline;
	using GRPolylineList = RTi.GR.GRPolylineList;
	using GRPolylineZM = RTi.GR.GRPolylineZM;
	using GRPolylineZMList = RTi.GR.GRPolylineZMList;
	using GRPolypoint = RTi.GR.GRPolypoint;
	using GRShape = RTi.GR.GRShape;
	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// GeoProjection is a base class to be used for projections.  Currently this class
	/// only defines a couple of methods that need to be defined for each projection.
	/// As more familiarity with the projection requirements is gained, more data and
	/// methods will be added to this base class.  In the future, may add some type of
	/// query ability here so that an application can list the projections that are available.
	/// </summary>
	public abstract class GeoProjection : ICloneable
	{

	/// <summary>
	/// A single GeographicProjection instance is made available to allow code to be
	/// optimized.  The GeographicProjection is the base projection that is used when changing projections.
	/// </summary>
	public static GeographicProjection geographic_projection = new GeographicProjection();

	/// <summary>
	/// Datum used (e.g., "NAD83").
	/// </summary>
	protected internal string _datum = "";

	/// <summary>
	/// Eccentricity constant (this is a GCTP parameter).
	/// </summary>
	protected internal double _e0 = 0.0;

	/// <summary>
	/// Eccentricity constant (this is a GCTP parameter).
	/// </summary>
	protected internal double _e1 = 0.0;

	/// <summary>
	/// Eccentricity constant (this is a GCTP parameter).
	/// </summary>
	protected internal double _e2 = 0.0;

	/// <summary>
	/// Eccentricity constant (this is a GCTP parameter).
	/// </summary>
	protected internal double _e = 0.0;

	/// <summary>
	/// Eccentricity constant (this is a GCTP parameter).
	/// </summary>
	protected internal double _es = 0.0;

	/// <summary>
	/// Eccentricity constant (this is a GCTP parameter).
	/// </summary>
	protected internal double _esp = 0.0;

	/// <summary>
	/// Eccentricity constant (this is a GCTP parameter).
	/// </summary>
	protected internal double _e3 = 0.0;

	/// <summary>
	/// False easting (this is a GCTP parameter).
	/// </summary>
	protected internal double _false_easting = 0.0;

	/// <summary>
	/// False northing (this is a GCTP parameter).
	/// </summary>
	protected internal double _false_northing = 0.0;

	/// <summary>
	/// Spherical flag (this is a GCTP parameter).
	/// </summary>
	protected internal bool _ind = false;

	/// <summary>
	/// Central latitude for (this is a GCTP parameter).
	/// </summary>
	protected internal double _lat_origin = 0.0;

	/// <summary>
	/// Central longitude (meridian) (this is a GCTP parameter).
	/// </summary>
	protected internal double _lon_center = 0.0;

	/// <summary>
	/// Small value for m (this is a GCTP parameter).
	/// </summary>
	protected internal double _ml0 = 0.0;

	/// <summary>
	/// Projection name (e.g., "Geographic").  Currently only simple names are used but
	/// in the future longer names may be used as more formal handling of projections occurs.
	/// </summary>
	protected internal string _projection_name = "Unknown";

	/// <summary>
	/// The projection number is an internal number to keep track of and compare projections.
	/// </summary>
	protected internal int _projection_number = 0;

	/// <summary>
	/// Radius of sphere (GCTP parameter).
	/// </summary>
	protected internal double _radius = 0.0;

	/// <summary>
	/// Semi-major axis for spheroid (GCTP parameter).
	/// </summary>
	protected internal double _r_major = 0.0;

	/// <summary>
	/// Semi-minor axis for spheroid (GCTP parameter).
	/// </summary>
	protected internal double _r_minor = 0.0;

	/// <summary>
	/// Scale for projection (GCTP parameter).
	/// </summary>
	protected internal double _scale_factor = 1.0;

	/// <summary>
	/// Zone used by UTM and StatePlane projections.  This is used to look up
	/// default _central_longitude and other parameters.
	/// </summary>
	protected internal int _zone = 0;

	/// <summary>
	/// List of defined projections.  Currently this is a list of projections that have
	/// been defined but it does not list available projections.  Use
	/// getProjectionNames() to get available projections and
	/// getDefinedProjectionNames() to get those that are defined in memory.
	/// </summary>
	private static string[] _defined_projections = null;

	/// <summary>
	/// Construct a projection.  This method should be called by derived classes to
	/// set base class information.  Each time that a projection is constructed, its
	/// name is checked against existing projections in memory.  If there is not a
	/// match, then a new projection is added to the active list and a projection number
	/// is assigned.  The projection numbers should be used for comparisons for
	/// optimized on-the-fly projections.  Because these checks are done each time a
	/// projection is created, care should be taken to minimize the number of new
	/// projections that are created (e.g., don't create inside a loop when a single
	/// projection is sufficient).  In most cases, a projection will be created and
	/// associated with a layer when it is read and then the same projection can be
	/// used for conversions.  This procedure is a simple form of reference counting and
	/// will allow projections to be dynamically added (which are not in the recognized list). </summary>
	/// <param name="name"> Projection name ("Geographic", "HRAP", "UTM"). </param>
	public GeoProjection(string name)
	{
		_projection_name = name;
		// Add a new projection to the list if necessary...
		if (_defined_projections == null)
		{
			// Allocate a new list..
			_defined_projections = new string[1];
			_defined_projections[0] = name;
			_projection_number = 0;
		}
		else
		{ // Resize the list and add to the end...
			bool found = false;
			for (int i = 0; i < _defined_projections.Length; i++)
			{
				if (_defined_projections[i].Equals(name, StringComparison.OrdinalIgnoreCase))
				{
					// Already in the list...
					_projection_number = i;
					found = true;
					break;
				}
			}
			if (!found)
			{
				// Add a new projection.  Always add at the end.
				// If an alphabetized list of projections is needed,
				// the names can always be sorted later.
				string[] tmp = new string[_defined_projections.Length + 1];
				for (int i = 0; i < _defined_projections.Length; i++)
				{
					tmp[i] = _defined_projections[i];
				}
				tmp[_defined_projections.Length] = name;
				_projection_number = _defined_projections.Length;
				_defined_projections = tmp;
				tmp = null;
			}
		}
	}

	/// <summary>
	/// Copy constructor. </summary>
	/// <param name="p"> GeoProjection instance to copy. </param>
	public GeoProjection(GeoProjection p)
	{
		_projection_name = p._projection_name;
		_projection_number = p._projection_number;
		_e0 = p._e0;
		_e1 = p._e1;
		_e2 = p._e2;
		_e3 = p._e3;
		_e = p._e;
		_es = p._es;
		_esp = p._esp;
		_false_easting = p._false_easting;
		_false_northing = p._false_northing;
		_ind = p._ind;
		_lat_origin = p._lat_origin;
		_lon_center = p._lon_center;
		_ml0 = p._ml0;
		_radius = p._radius;
		_r_major = p._r_major;
		_r_minor = p._r_minor;
		_scale_factor = p._scale_factor;
		_zone = p._zone;
	}

	/*
	Adjust a longitude angle to range from -180 to 180 radians (method from GCTP package).
	@param x Angle in radians.
	*/
	protected internal static double adjust_lon(double x)
	{
		long count = 0;
		double TWO_PI = Math.PI * 2.0;
		double MAXLONG = 2147483647.0;
		double DBLLONG = 4.61168601e18;
		for (;;)
		{
			if (Math.Abs(x) <= Math.PI)
			{
				break;
			}
			else if (((long)Math.Abs(x / Math.PI)) < 2)
			{
				x = x - (sign(x) * TWO_PI);
			}
			else if (((long)Math.Abs(x / TWO_PI)) < MAXLONG)
			{
				x = x - (((long)(x / TWO_PI)) * TWO_PI);
			}
			else if (((long)Math.Abs(x / (MAXLONG * TWO_PI))) < MAXLONG)
			{
				x = x - (((long)(x / (MAXLONG * TWO_PI))) * (TWO_PI * MAXLONG));
			}
			else if (((long)Math.Abs(x / (DBLLONG * TWO_PI))) < MAXLONG)
			{
				x = x - (((long)(x / (DBLLONG * TWO_PI))) * (TWO_PI * DBLLONG));
			}
			else
			{
				x = x - (sign(x) * TWO_PI);
			}
			count++;
			if (count > 4)
			{
				break;
			}
		}
		return x;
	}

	/*
	Eliminate roundoff errors in asin.
	*/
	protected internal static double asinz(double con)
	{
		if (Math.Abs(con) > 1.0)
		{
			if (con > 1.0)
			{
				con = 1.0;
			}
			else
			{
				con = -1.0;
			}
		}
		return Math.Asin(con);
	}

	/// <summary>
	/// Clones the object. </summary>
	/// <returns> a clone of the object. </returns>
	public virtual object clone()
	{
		try
		{
			return (GeoProjection)base.clone();
		}
		catch (Exception)
		{
			return null;
		}
	}

	/// <summary>
	/// Compute the constant e0, which is used in a series for calculating the distance along a meridian. </summary>
	/// <returns> value of e0. </returns>
	/// <param name="x"> the eccentricity squared. </param>
	protected internal static double e0fn(double x)
	{
		return (1.0 - 0.25 * x * (1.0 + x / 16.0 * (3.0 + 1.25 * x)));
	}

	/// <summary>
	/// Compute the constant e1, which is used in a series for calculating the distance along a meridian. </summary>
	/// <returns> value of e1. </returns>
	/// <param name="x"> the eccentricity squared. </param>
	protected internal static double e1fn(double x)
	{
		return (0.375 * x * (1.0 + 0.25 * x * (1.0 + 0.46875 * x)));
	}

	/// <summary>
	/// Compute the constant e2, which is used in a series for calculating the distance along a meridian. </summary>
	/// <returns> value of e2. </returns>
	/// <param name="x"> the eccentricity squared. </param>
	protected internal static double e2fn(double x)
	{
		return (0.05859375 * x * x * (1.0 + 0.75 * x));
	}

	/// <summary>
	/// Compute the constant e3, which is used in a series for calculating the distance along a meridian. </summary>
	/// <returns> value of e3. </returns>
	/// <param name="x"> the eccentricity squared. </param>
	protected internal static double e3fn(double x)
	{
		return (x * x * x * (35.0 / 3072.0));
	}

	/// <summary>
	/// Determine if projections are equal.  Currently, the name, datum, and zone are the only items checked. </summary>
	/// <param name="other"> Other projection to compare to. </param>
	/// <returns> true if the projections are equal. </returns>
	public virtual bool Equals(GeoProjection other)
	{
		if (_datum.Equals(other._datum, StringComparison.OrdinalIgnoreCase) && _projection_name.Equals(other._projection_name, StringComparison.OrdinalIgnoreCase) && (_zone == other._zone))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Clean up memory for garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~GeoProjection()
	{
		_projection_name = null;
		_datum = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Get the number of kilometers for a unit of the projection grid.  The
	/// point that is used can be reused if necessary to increase performance.
	/// This method should be defined in derived classes.  The version in this base
	/// class always returns the original point. </summary>
	/// <param name="p"> As input, specifies the location (in projected units) at which to determine the scale. </param>
	/// <param name="reuse_point"> Indicates whether the point that is passed in should be
	/// re-used for the output (doing so saves memory). </param>
	public virtual GRPoint getKilometersForUnit(GRPoint p, bool reuse_point)
	{
		return p;
	}

	/// <summary>
	/// Get the projection name. </summary>
	/// <returns> the projection name. </returns>
	public virtual string getProjectionName()
	{
		return _projection_name;
	}

	/// <summary>
	/// Get the list of available projections or null if no projections have been defined. </summary>
	/// <param name="type"> If 0, lists the available projections known to the GeoView package.
	/// If 1, lists the defined projections (projections that have been instantiated during processing). </param>
	public static IList<string> getProjectionNames(int type)
	{
		if (type == 0)
		{
			IList<string> v = new List<string> (4);
			v.Add("Geographic");
			v.Add("HRAP");
			v.Add("UTM");
			v.Add("Unknown");
		}
		else if (type == 1)
		{
			if (_defined_projections == null)
			{
				return null;
			}
			IList<string> v = new List<string> (_defined_projections.Length);
			for (int i = 0; i < _defined_projections.Length; i++)
			{
				v.Add(_defined_projections[i]);
			}
			return v;
		}
		return null;
	}

	/// <summary>
	/// Get the projection number. </summary>
	/// <returns> the projection number. </returns>
	public virtual int getProjectionNumber()
	{
		return _projection_number;
	}

	/// <summary>
	/// Compute the distance along a meridian from the Equator to latitude phi.
	/// Method is from the GCTP package.
	/// </summary>
	protected internal static double mlfn(double e0, double e1, double e2, double e3, double phi)
	{
		return (e0 * phi - e1 * Math.Sin(2.0 * phi) + e2 * Math.Sin(4.0 * phi) - e3 * Math.Sin(6.0 * phi));
	}

	/// <summary>
	/// Determine whether a projection needs to be made. </summary>
	/// <returns> false if the projection numbers are the same or either projection is unknown, true otherwise. </returns>
	/// <param name="projection1"> First projection. </param>
	/// <param name="projection2"> Second projection. </param>
	public static bool needToProject(GeoProjection projection1, GeoProjection projection2)
	{
		if ((projection1 == null) || (projection2 == null))
		{
			return false;
		}
		if (projection1.getProjectionName().Equals("Unknown", StringComparison.OrdinalIgnoreCase) || projection2.getProjectionName().Equals("Unknown", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
	/*
		if ( projection1.getProjectionNumber() == projection2.getProjectionNumber() ) {
			return false;
		}
	*/
		// Rely on the more robust equals() method...
		if (projection1.Equals(projection2))
		{
			return false;
		}
		return true;
	}

	/// <summary>
	/// Parse a projection string and return an instance of the projection.
	/// Currently this only works for known projections but generic classes of
	/// projections with parameters, can be added in the future. </summary>
	/// <param name="projection_string"> String containing projection definition (e.g.,
	/// "Geographic").  Strings must adhere to the following to be recognized:
	/// <table width=100% cellpadding=10 cellspacing=0 border=2>
	/// <tr>
	/// <td><b>Projection</b></td>   <td><b>Description</b></td>   
	/// <td><b>Example</b></td>
	/// </tr
	/// 
	/// <tr>
	/// <td><b>Geographic</b></td>
	/// <td>Longitude, Latitude.</td>
	/// <td>Geographic</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>HRAP</b></td>
	/// <td>National Weather Service coordinate system.</td>
	/// <td>HRAP</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>UTM,Zone,Datum,FalseEasting,FalseNorthing,CentralLongitude,
	/// OriginLatitude,Scale</b><br>
	/// Specify no value to use the default.
	/// The default Datum is NAD83.
	/// The default FalseEasting is 500000.0.
	/// The default FalseNorthing is 500000.0.
	/// The default CentralLongitude is determined from the zone.
	/// The default OriginLatitude is 0.
	/// The default Scale is .9996.
	/// </td>
	/// <td>Universal Transvers Mercator</td>
	/// <td>UTM,19,NAD83,500000.0,0.0,,,.9996<br>
	/// UTM,19<br></td>
	/// </tr>
	/// 
	/// </table> </param>
	/// <exception cref="Exception"> if a projection cannot be determined from the string. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static GeoProjection parseProjection(String projection_string) throws Exception
	public static GeoProjection parseProjection(string projection_string)
	{
		if (projection_string.regionMatches(true,0,"Geographic",0,10))
		{
			return new GeographicProjection();
		}
		else if (projection_string.regionMatches(true,0,"HRAP",0,4))
		{
			return new HRAPProjection();
		}
		else if (projection_string.regionMatches(true,0,"Unknown",0,7))
		{
			return new UnknownProjection();
		}
		else if (projection_string.regionMatches(true,0,"UTM",0,3))
		{
			return UTMProjection.parse(projection_string);
		}
		else
		{
			throw new Exception("Unknown projection \"" + projection_string + "\"");
		}
	}

	/// <summary>
	/// Project latitude and longitude to the projection's coordinate system. </summary>
	/// <param name="p"> Point to project from latitude and longitude. </param>
	/// <param name="reuse_point"> Indicates whether the point that is passed in should be
	/// re-used for the output (doing so saves memory). </param>
	public virtual GRPoint project(GRPoint p, bool reuse_point)
	{
		Message.printStatus(2, "GeoProjection.project", "This method should be defined in the derived class.  Returning the original point.");
		return p;
	}

	/// <summary>
	/// Project a shape from one projection to another.  Note that GRArc radii are not currently projected. </summary>
	/// <param name="from"> Projection to convert from. </param>
	/// <param name="to"> Projection to convert to. </param>
	/// <param name="shape"> Shape to convert. </param>
	/// <param name="reuseShape"> Indicates whether shape should be reused (doing so saves memory resources). </param>
	public static GRShape projectShape(GeoProjection from, GeoProjection to, GRShape shape, bool reuseShape)
	{
		if (shape.type == GRShape.ARC)
		{
			GRArc arc = null;
			if (reuseShape)
			{
				arc = (GRArc)shape;
				// Need to do this so there will be clean start on getting limits...
				arc.limits_found = false;
			}
			else
			{
				arc = new GRArc((GRArc)shape);
			}
			from.unProject(arc.pt,true);
			to.project(arc.pt,true);
			// Also need to project the radii (do later - for now require radii to be pre-projected)...
			// This is necessary to recalculate the max/min values, which
			// ultimately get used when deciding if the shape should be drawn...
			arc.setPoint(arc.pt);
			if (reuseShape)
			{
				arc = null;
				return shape;
			}
			else
			{
				return arc;
			}
		}
		else if (shape.type == GRShape.POLYGON)
		{
			GRPolygon polygon = null;
			if (reuseShape)
			{
				polygon = (GRPolygon)shape;
				// Need to do this so there will be clean start on getting limits...
				polygon.limits_found = false;
			}
			else
			{
				polygon = new GRPolygon((GRPolygon)shape);
			}
			for (int i = 0; i < polygon.npts; i++)
			{
				from.unProject(polygon.pts[i],true);
				to.project(polygon.pts[i],true);
				// This is necessary to recalculate the max/min values, which ultimately get used when
				// deciding if the shape should be drawn...
				polygon.setPoint(i, polygon.pts[i]);
			}
			if (reuseShape)
			{
				polygon = null;
				return shape;
			}
			else
			{
				return polygon;
			}
		}
		else if (shape.type == GRShape.POLYGON_LIST)
		{
			GRPolygonList polygonlist = null;
			if (reuseShape)
			{
				polygonlist = (GRPolygonList)shape;
				// Need to do this so there will be clean start on getting limits...
				polygonlist.limits_found = false;
			}
			else
			{
				polygonlist = new GRPolygonList((GRPolygonList)shape);
			}
			// Loop through the polygons in the list and project each...
			for (int i = 0; i < polygonlist.npolygons; i++)
			{
				projectShape(from, to, polygonlist.polygons[i], true);
				// This is necessary to recalculate the max/min
				// values, which ultimately get used when deciding if the shape should be drawn...
				polygonlist.setPolygon(i, polygonlist.polygons[i]);
			}
			if (reuseShape)
			{
				polygonlist = null;
				return shape;
			}
			else
			{
				return polygonlist;
			}
		}
		else if (shape.type == GRShape.POLYLINE)
		{
			GRPolyline polyline = null;
			if (reuseShape)
			{
				polyline = (GRPolyline)shape;
				// Need to do this so there will be clean start on getting limits...
				polyline.limits_found = false;
			}
			else
			{
				polyline = new GRPolyline((GRPolyline)shape);
			}
			for (int i = 0; i < polyline.npts; i++)
			{
				from.unProject(polyline.pts[i],true);
				to.project(polyline.pts[i],true);
				// This is necessary to recalculate the max/min values, which ultimately get used when
				// deciding if the shape should be drawn...
				polyline.setPoint(i, polyline.pts[i]);
			}
			if (reuseShape)
			{
				polyline = null;
				return shape;
			}
			else
			{
				return polyline;
			}
		}
		else if (shape.type == GRShape.POLYLINE_ZM)
		{
			GRPolylineZM polyline = null;
			if (reuseShape)
			{
				polyline = (GRPolylineZM)shape;
				// Need to do this so there will be clean start on getting limits...
				polyline.limits_found = false;
			}
			else
			{
				polyline = new GRPolylineZM((GRPolylineZM)shape);
			}
			for (int i = 0; i < polyline.npts; i++)
			{
				from.unProject(polyline.pts[i],true);
				to.project(polyline.pts[i],true);
				// This is necessary to recalculate the max/min values, which ultimately get used when
				// deciding if the shape should be drawn...
				polyline.setPoint(i, polyline.pts[i]);
			}
			if (reuseShape)
			{
				polyline = null;
				return shape;
			}
			else
			{
				return polyline;
			}
		}
		else if (shape.type == GRShape.POLYLINE_LIST)
		{
			GRPolylineList polylinelist = null;
			if (reuseShape)
			{
				polylinelist = (GRPolylineList)shape;
				// Need to do this so there will be clean start on getting limits...
				polylinelist.limits_found = false;
			}
			else
			{
				polylinelist = new GRPolylineList((GRPolylineList)shape);
			}
			// Loop through the polylines in the list and project each...
			for (int i = 0; i < polylinelist.npolylines; i++)
			{
				projectShape(from, to, polylinelist.polylines[i],true);
				// This is necessary to recalculate the max/min
				// values, which ultimately get used when deciding if the shape should be drawn...
				polylinelist.setPolyline(i, polylinelist.polylines[i]);
			}
			if (reuseShape)
			{
				polylinelist = null;
				return shape;
			}
			else
			{
				return polylinelist;
			}
		}
		else if (shape.type == GRShape.POLYLINE_ZM_LIST)
		{
			GRPolylineZMList polylinelist = null;
			if (reuseShape)
			{
				polylinelist = (GRPolylineZMList)shape;
				// Need to do this so there will be clean start on getting limits...
				polylinelist.limits_found = false;
			}
			else
			{
				polylinelist = new GRPolylineZMList((GRPolylineZMList)shape);
			}
			// Loop through the polylines in the list and project each...
			for (int i = 0; i < polylinelist.npolylines; i++)
			{
				projectShape(from, to, polylinelist.polylines[i],true);
				// This is necessary to recalculate the max/min
				// values, which ultimately get used when deciding if the shape should be drawn...
				polylinelist.setPolyline(i, polylinelist.polylines[i]);
			}
			if (reuseShape)
			{
				polylinelist = null;
				return shape;
			}
			else
			{
				return polylinelist;
			}
		}
		else if (shape.type == GRShape.POINT)
		{
			GRPoint point = null;
			if (reuseShape)
			{
				point = (GRPoint)shape;
			}
			else
			{
				point = new GRPoint((GRPoint)shape);
			}
			from.unProject(point,true);
			to.project(point,true);
			if (reuseShape)
			{
				point = null;
				return shape;
			}
			else
			{
				return point;
			}
		}
		else if (shape.type == GRShape.POINT_ZM)
		{
			GRPointZM point = null;
			if (reuseShape)
			{
				point = (GRPointZM)shape;
			}
			else
			{
				point = new GRPointZM((GRPointZM)shape);
			}
			from.unProject(point,true);
			to.project(point,true);
			if (reuseShape)
			{
				point = null;
				return shape;
			}
			else
			{
				return point;
			}
		}
		else if (shape.type == GRShape.POLYPOINT)
		{
			GRPolypoint polypoint = null;
			if (reuseShape)
			{
				polypoint = (GRPolypoint)shape;
				// Need to do this so there will be clean start on getting limits...
				polypoint.limits_found = false;
			}
			else
			{
				polypoint = new GRPolypoint((GRPolypoint)shape);
			}
			for (int i = 0; i < polypoint.npts; i++)
			{
				from.unProject(polypoint.pts[i],true);
				to.project(polypoint.pts[i],true);
				// This is necessary to recalculate the max/min values, which ultimately get used when
				// deciding if the shape should be drawn...
				polypoint.setPoint(i, polypoint.pts[i]);
			}
			if (reuseShape)
			{
				polypoint = null;
				return shape;
			}
			else
			{
				return polypoint;
			}
		}
		else if (shape is GRLimits)
		{
			GRLimits limits = (GRLimits)shape;
			GRPoint pointMin = new GRPoint(limits.getMinX(),limits.getMinY());
			GRPoint pointMax = new GRPoint(limits.getMaxX(),limits.getMaxY());
			if (reuseShape)
			{
				limits = (GRLimits)shape;
			}
			else
			{
				limits = new GRLimits((GRLimits)shape);
			}
			from.unProject(pointMin,true);
			to.project(pointMin,true);
			from.unProject(pointMax,true);
			to.project(pointMax,true);
			if (reuseShape)
			{
				limits.setLeftX(pointMin.getX());
				limits.setBottomY(pointMin.getY());
				limits.setRightX(pointMax.getX());
				limits.setTopY(pointMax.getY());
				return limits;
			}
			else
			{
				limits = new GRLimits(pointMin.getX(),pointMin.getY(),pointMax.getX(),pointMax.getY());
				return limits;
			}
		}
		// For now just return
		return shape;
	}

	/// <summary>
	/// Set the spheroid information (_r_major, _r_minor, _radius) given the datum string.  This is called
	/// from the derived projections.  This code was taken from the GCTP sphdz() function. </summary>
	/// <param name="datum"> Datum string (currently only "NAD27" and "NAD83" are recognized. </param>
	protected internal virtual void setSpheroid(string datum)
	{
		if (datum.Equals("NAD27", StringComparison.OrdinalIgnoreCase))
		{
			// GCTP 0: Clarke 1866 (default)
			_datum = datum;
			_r_major = 6378206.4;
			_r_minor = 6356583.8;
			_radius = 6370997.0; // GCTP 19: Sphere of Radius 6370997 meters
		}
		else if (datum.Equals("NAD83", StringComparison.OrdinalIgnoreCase))
		{
			// GCTP 8: GRS 1980
			_datum = datum;
			_r_major = 6378137.0;
			_r_minor = 6356752.31414;
			_radius = 6370997.0; // GCTP 19: Sphere of Radius 6370997 meters
		}
		else
		{
			// GCTP 0: Clarke 1866 (default)
			_datum = "NAD27";
			_r_major = 6378206.4;
			_r_minor = 6356583.8;
			_radius = 6370997.0; // GCTP 19: Sphere of Radius 6370997 meters
		}
	}

	/// <summary>
	/// Return the sign of an argument. </summary>
	/// <returns> the sign of an argument. </returns>
	protected internal static int sign(double x)
	{
		if (x < 0.0)
		{
			return -1;
		}
		else
		{
			return 1;
		}
	}

	/// <summary>
	/// Return the name of the projection. </summary>
	/// <returns> the projection name as a String. </returns>
	public override string ToString()
	{
		return _projection_name;
	}

	/// <summary>
	/// Un-project coordinates back to latitude and longitude. </summary>
	/// <returns> the un-projected points. </returns>
	/// <param name="p"> Point to un-project to latitude and longitude. </param>
	/// <param name="reusePoint"> Indicates whether the point that is passed in should be
	/// re-used for the output (doing so saves memory). </param>
	public virtual GRPoint unProject(GRPoint p, bool reusePoint)
	{
		Message.printStatus(2, "GeoProjection.unProject", "This method should be defined in the derived class.  Returning the original point.");
		return p;
	}

	}

}