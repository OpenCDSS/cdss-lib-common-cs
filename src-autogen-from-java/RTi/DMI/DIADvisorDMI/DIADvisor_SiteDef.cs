// ----------------------------------------------------------------------------
// DIADvisor_SiteDef.java - corresponds to DIADvisor SiteDef table
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2003-02-02	Steven A. Malers, RTi	Initial version.  Copy and modify
//					RiversideDB_MeasLoc.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.DMI.DIADvisorDMI
{


	/// <summary>
	/// The DIADvisor_SiteDef class store data records from the DIADvisor SiteDef table.
	/// </summary>
	public class DIADvisor_SiteDef : DMIDataObject
	{

	// From table SiteDef

	protected internal string _SiteName = DMIUtil.MISSING_STRING;
	protected internal int _SiteID = DMIUtil.MISSING_INT;
	protected internal double _Latitude = DMIUtil.MISSING_DOUBLE;
	protected internal double _Longitude = DMIUtil.MISSING_DOUBLE;
	protected internal double _XCoord = DMIUtil.MISSING_DOUBLE;
	protected internal double _YCoord = DMIUtil.MISSING_DOUBLE;
	protected internal long _PKey = DMIUtil.MISSING_LONG;
	protected internal string _RepeaterGroup = DMIUtil.MISSING_STRING;
	protected internal double _Elevation = DMIUtil.MISSING_DOUBLE;
	protected internal string _SitePicture = DMIUtil.MISSING_STRING;
	protected internal string _Zone = DMIUtil.MISSING_STRING;
	protected internal string _FIPS = DMIUtil.MISSING_STRING;
	protected internal System.DateTime _LastUpdate = DMIUtil.MISSING_DATE;

	/// <summary>
	/// DIADvisor_SiteDef constructor.
	/// </summary>
	public DIADvisor_SiteDef() : base()
	{
	}

	/// <summary>
	/// Cleans up variables when the class is disposed of.  Sets all the member
	/// variables (that aren't primitives) to null.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~DIADvisor_SiteDef()
	{
		_SiteName = null;
		_RepeaterGroup = null;
		_SitePicture = null;
		_Zone = null;
		_FIPS = null;
		_LastUpdate = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns _Elevation </summary>
	/// <returns> _Elevation </returns>
	public virtual double getElevation()
	{
		return _Elevation;
	}

	/// <summary>
	/// Returns _FIPS </summary>
	/// <returns> _FIPS </returns>
	public virtual string getFIPS()
	{
		return _FIPS;
	}

	/// <summary>
	/// Returns _LastUpdate </summary>
	/// <returns> _LastUpdate </returns>
	public virtual System.DateTime getLastUpdate()
	{
		return _LastUpdate;
	}

	/// <summary>
	/// Returns _Latitude </summary>
	/// <returns> _Latitude </returns>
	public virtual double getLatitude()
	{
		return _Latitude;
	}

	/// <summary>
	/// Returns _Longitude </summary>
	/// <returns> _Longitude </returns>
	public virtual double getLongitude()
	{
		return _Longitude;
	}

	/// <summary>
	/// Return _PKey </summary>
	/// <returns> _PKey </returns>
	public virtual long getPKey()
	{
		return _PKey;
	}

	/// <summary>
	/// Return _RepeaterGroup </summary>
	/// <returns> _RepeaterGroup </returns>
	public virtual string getRepeaterGroup()
	{
		return _RepeaterGroup;
	}

	/// <summary>
	/// Return _SiteID </summary>
	/// <returns> _SiteID </returns>
	public virtual int getSiteID()
	{
		return _SiteID;
	}

	/// <summary>
	/// Return _SiteName </summary>
	/// <returns> _SiteName </returns>
	public virtual string getSiteName()
	{
		return _SiteName;
	}

	/// <summary>
	/// Return _SitePicture </summary>
	/// <returns> _SitePicture </returns>
	public virtual string getSitePicture()
	{
		return _SitePicture;
	}

	/// <summary>
	/// Returns _XCoord </summary>
	/// <returns> _XCoord </returns>
	public virtual double getXCoord()
	{
		return _XCoord;
	}

	/// <summary>
	/// Returns _YCoord </summary>
	/// <returns> _YCoord </returns>
	public virtual double getYCoord()
	{
		return _YCoord;
	}

	/// <summary>
	/// Return _Zone </summary>
	/// <returns> _Zone </returns>
	public virtual string getZone()
	{
		return _Zone;
	}

	/// <summary>
	/// Set _Elevation. </summary>
	/// <param name="Elevation"> value to assign to _Elevation. </param>
	public virtual void setElevation(double Elevation)
	{
		_Elevation = Elevation;
	}

	/// <summary>
	/// Set _FIPS </summary>
	/// <param name="FIPS"> value to assign to _FIPS. </param>
	public virtual void setFIPS(string FIPS)
	{
		if (!string.ReferenceEquals(FIPS, null))
		{
			_FIPS = FIPS;
		}
	}

	/// <summary>
	/// Set _LastUpdate </summary>
	/// <param name="LastUpdate"> value to assign to _LastUpdate. </param>
	public virtual void setLastUpdate(System.DateTime LastUpdate)
	{
		_LastUpdate = LastUpdate;
	}

	/// <summary>
	/// Set _Latitude. </summary>
	/// <param name="Latitude"> value to assign to _Latitude. </param>
	public virtual void setLatitude(double Latitude)
	{
		_Latitude = Latitude;
	}

	/// <summary>
	/// Set _Longitude. </summary>
	/// <param name="Longitude"> value to assign to _Longitude. </param>
	public virtual void setLongitude(double Longitude)
	{
		_Longitude = Longitude;
	}

	/// <summary>
	/// Set _PKey. </summary>
	/// <param name="PKey"> value to assign to _PKey. </param>
	public virtual void setPKey(long PKey)
	{
		_PKey = PKey;
	}

	/// <summary>
	/// Set _RepeaterGroup </summary>
	/// <param name="RepeaterGroup"> value to assign to _RepeaterGroup. </param>
	public virtual void setRepeaterGroup(string RepeaterGroup)
	{
		if (!string.ReferenceEquals(RepeaterGroup, null))
		{
			_RepeaterGroup = RepeaterGroup;
		}
	}

	/// <summary>
	/// Set _SiteID. </summary>
	/// <param name="SiteID"> value to assign to _SiteID. </param>
	public virtual void setSiteID(int SiteID)
	{
		_SiteID = SiteID;
	}

	/// <summary>
	/// Set _SiteName </summary>
	/// <param name="SiteName"> value to assign to _SiteName. </param>
	public virtual void setSiteName(string SiteName)
	{
		if (!string.ReferenceEquals(SiteName, null))
		{
			_SiteName = SiteName;
		}
	}

	/// <summary>
	/// Set _SitePicture </summary>
	/// <param name="SitePicture"> value to assign to _SitePicture. </param>
	public virtual void setSitePicture(string SitePicture)
	{
		if (!string.ReferenceEquals(SitePicture, null))
		{
			_SitePicture = SitePicture;
		}
	}

	/// <summary>
	/// Set _XCoord. </summary>
	/// <param name="XCoord"> value to assign to _XCoord. </param>
	public virtual void setXCoord(double XCoord)
	{
		_XCoord = XCoord;
	}

	/// <summary>
	/// Set _YCoord. </summary>
	/// <param name="YCoord"> value to assign to _YCoord. </param>
	public virtual void setYCoord(double YCoord)
	{
		_YCoord = YCoord;
	}

	/// <summary>
	/// Set _Zone </summary>
	/// <param name="Zone"> value to assign to _Zone. </param>
	public virtual void setZone(string Zone)
	{
		if (!string.ReferenceEquals(Zone, null))
		{
			_Zone = Zone;
		}
	}

	/// <summary>
	/// Return a string representation of this object. </summary>
	/// <returns> a string representation of this object. </returns>
	public override string ToString()
	{
		return "";
	/*
		return ( "RiversideDB_MeasLoc{" 		+ "\n" +
			"MeasLoc_num:  " + _MeasLoc_num		+ "\n" +
			"Geoloc_num:   " + _Geoloc_num		+ "\n" +
			"Identifier:   " + _Identifier		+ "\n" +
			"MeasLoc_name: " + _MeasLoc_name	+ "\n" +
			"Source_abbreb:" + _Source_abbrev	+ "\n" + 
			"Meas_loc_type:" + _Meas_loc_type	+ "\n" + 
			"Comment:      " + _Comment		+ "}"
		);
	*/
	}

	} // End DIADvisor_SiteDef

}