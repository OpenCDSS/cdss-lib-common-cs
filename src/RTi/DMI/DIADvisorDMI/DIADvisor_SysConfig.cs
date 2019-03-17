// ----------------------------------------------------------------------------
// DIADvisor_SysConfig.java - corresponds to DIADvisor SysConfig table
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2003-03-30	Steven A. Malers, RTi	Initial version.  Copy and modify
//					RiversideDB_SiteDef.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.DMI.DIADvisorDMI
{


	/// <summary>
	/// The DIADvisor_SysConfig class stores data records from the DIADvisor SysConfig
	/// table.  This class is incomplete - only Interval is handled at this time.
	/// </summary>
	public class DIADvisor_SysConfig : DMIDataObject
	{

	protected internal string _Environment = DMIUtil.MISSING_STRING;
	protected internal string _Units = DMIUtil.MISSING_STRING;
	protected internal int _Alarms = DMIUtil.MISSING_INT;
	protected internal int _Notify = DMIUtil.MISSING_INT;
	protected internal int _Interval = DMIUtil.MISSING_INT;
	protected internal string _Group = DMIUtil.MISSING_STRING;
	protected internal double _RainScale = DMIUtil.MISSING_DOUBLE;
	protected internal int _AreaRain = DMIUtil.MISSING_INT;
	protected internal int _SubBasinRain = DMIUtil.MISSING_INT;
	protected internal int _RainDisplay = DMIUtil.MISSING_INT;
	protected internal string _Map = DMIUtil.MISSING_STRING;
	protected internal string _Palette = DMIUtil.MISSING_STRING;
	protected internal int _DuplicateTime = DMIUtil.MISSING_INT;
	protected internal string _SaveKey = DMIUtil.MISSING_STRING;
	protected internal string _ArchiveKey = DMIUtil.MISSING_STRING;
	protected internal string _DataKey = DMIUtil.MISSING_STRING;
	protected internal string _TableKey = DMIUtil.MISSING_STRING;
	protected internal int _KeepDays = DMIUtil.MISSING_INT;
	protected internal int _CopyDays = DMIUtil.MISSING_INT;
	protected internal System.DateTime _ArchiveTime = DMIUtil.MISSING_DATE;
	protected internal int _LastSeqNum = DMIUtil.MISSING_INT;
	protected internal System.DateTime _LastArch = DMIUtil.MISSING_DATE;
	protected internal string _ArchiveDb = DMIUtil.MISSING_STRING;
	protected internal string _CopyDb = DMIUtil.MISSING_STRING;
	protected internal System.DateTime _LastUpdate = DMIUtil.MISSING_DATE;
	protected internal double _FFGLow = DMIUtil.MISSING_DOUBLE;
	protected internal double _FFGHigh = DMIUtil.MISSING_DOUBLE;
	protected internal int _MWW = DMIUtil.MISSING_INT;
	protected internal int _AppHandle = DMIUtil.MISSING_INT;
	protected internal int _BuildGeo = DMIUtil.MISSING_INT;
	protected internal string _LocalEmailUser = DMIUtil.MISSING_STRING;
	protected internal string _LocalEmailPassword = DMIUtil.MISSING_STRING;
	protected internal int _BitmapExport = DMIUtil.MISSING_INT;
	protected internal string _BitmapExportFile = DMIUtil.MISSING_STRING;
	protected internal int _HTMLExport = DMIUtil.MISSING_INT;
	protected internal string _HTMLExportDirectory = DMIUtil.MISSING_STRING;
	protected internal System.DateTime _HTMLExportFrom = DMIUtil.MISSING_DATE;
	protected internal System.DateTime _HTMLExportTo = DMIUtil.MISSING_DATE;

	/// <summary>
	/// DIADvisor_SiteDef constructor.
	/// </summary>
	public DIADvisor_SysConfig() : base()
	{
	}

	/// <summary>
	/// Cleans up variables when the class is disposed of.  Sets all the member
	/// variables (that aren't primitives) to null.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~DIADvisor_SysConfig()
	{
		_Environment = null;
		_Units = null;
		_Group = null;
		_Map = null;
		_Palette = null;
		_SaveKey = null;
		_ArchiveKey = null;
		_DataKey = null;
		_TableKey = null;
		_ArchiveTime = null;
		_LastArch = null;
		_ArchiveDb = null;
		_CopyDb = null;
		_LastUpdate = null;
		_LocalEmailUser = null;
		_LocalEmailPassword = null;
		_BitmapExportFile = null;
		_HTMLExportDirectory = null;
		_HTMLExportFrom = null;
		_HTMLExportTo = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns _Interval </summary>
	/// <returns> _Interval </returns>
	public virtual int getInterval()
	{
		return _Interval;
	}

	/// <summary>
	/// Set _Interval. </summary>
	/// <param name="Interval"> value to assign to _Interval. </param>
	public virtual void setInterval(int Interval)
	{
		_Interval = Interval;
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

	} // End DIADvisor_SysConfig

}