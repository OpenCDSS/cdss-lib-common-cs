// ----------------------------------------------------------------------------
// DIADvisor_SensorDef.java - corresponds to DIADvisor SensorDef table
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2003-02-06	Steven A. Malers, RTi	Initial version.  Copy and modify
//					DIADvisor_SiteDef.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.DMI.DIADvisorDMI
{


	/// <summary>
	/// The DIADvisor_SensorDef class store data records from the DIADvisor SensorDef
	/// table.
	/// </summary>
	public class DIADvisor_SensorDef : DMIDataObject
	{

	// From table SensorDef, in order of table design

	protected internal int _SensorID = DMIUtil.MISSING_INT;
	protected internal int _SiteID = DMIUtil.MISSING_INT;
	protected internal string _Type = DMIUtil.MISSING_STRING;
	protected internal string _Group = DMIUtil.MISSING_STRING;
	protected internal string _Description = DMIUtil.MISSING_STRING;
	protected internal int _MaxCount = DMIUtil.MISSING_INT;
	protected internal int _MinCount = DMIUtil.MISSING_INT;
	protected internal int _PosDelta = DMIUtil.MISSING_INT;
	protected internal int _NegDelta = DMIUtil.MISSING_INT;
	protected internal string _RatingType = DMIUtil.MISSING_STRING;
	protected internal string _RatingInterpolation = DMIUtil.MISSING_STRING;
	protected internal double _RatingShift = DMIUtil.MISSING_DOUBLE;
	protected internal double _CalibrationOffset = DMIUtil.MISSING_DOUBLE;
	protected internal System.DateTime _CalibrationDate = DMIUtil.MISSING_DATE;
	protected internal double _Slope = DMIUtil.MISSING_DOUBLE;
	protected internal double _ReferenceLevel = DMIUtil.MISSING_DOUBLE;
	protected internal string _DisplayUnits = DMIUtil.MISSING_STRING;
	protected internal int _Decimal = DMIUtil.MISSING_INT;
	protected internal string _DisplayUnits2 = DMIUtil.MISSING_STRING;
	protected internal int _Decimal2 = DMIUtil.MISSING_INT;
	protected internal bool _InService = false;
	protected internal bool _Suspect = false;
	protected internal bool _Alarms = false;
	protected internal bool _Notify = false;
	protected internal double _Timeout = DMIUtil.MISSING_DOUBLE;
	protected internal bool _Children = false;
	protected internal System.DateTime _MostRecentTime = DMIUtil.MISSING_DATE;
	protected internal double _MostRecentData = DMIUtil.MISSING_DOUBLE;
	protected internal System.DateTime _LastValidTime = DMIUtil.MISSING_DATE;
	protected internal double _LastValidData = DMIUtil.MISSING_DOUBLE;
	protected internal double _LastCount = DMIUtil.MISSING_DOUBLE;
	protected internal string _Equation = DMIUtil.MISSING_STRING;
	protected internal string _Equation2 = DMIUtil.MISSING_STRING;
	protected internal System.DateTime _LastUpdate = DMIUtil.MISSING_DATE;

	/// <summary>
	/// DIADvisor_SensorDef constructor.
	/// </summary>
	public DIADvisor_SensorDef() : base()
	{
	}

	/// <summary>
	/// Cleans up variables when the class is disposed of.  Sets all the member
	/// variables (that aren't primitives) to null.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~DIADvisor_SensorDef()
	{
		_Type = null;
		_Group = null;
		_Description = null;
		_RatingType = null;
		_RatingInterpolation = null;
		_CalibrationDate = null;
		_DisplayUnits = null;
		_DisplayUnits2 = null;
		_MostRecentTime = null;
		_LastValidTime = null;
		_Equation = null;
		_Equation2 = null;
		_LastUpdate = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns _Alarms </summary>
	/// <returns> _Alarms </returns>
	public virtual bool getAlarms()
	{
		return _Alarms;
	}

	/// <summary>
	/// Returns _CalibrationDate </summary>
	/// <returns> _CalibrationDate </returns>
	public virtual System.DateTime getCalibrationDate()
	{
		return _CalibrationDate;
	}

	/// <summary>
	/// Returns _CalibrationOffset </summary>
	/// <returns> _CalibrationOffset </returns>
	public virtual double getCalibrationOffset()
	{
		return _CalibrationOffset;
	}

	/// <summary>
	/// Returns _Children </summary>
	/// <returns> _Children </returns>
	public virtual bool getChildren()
	{
		return _Children;
	}

	/// <summary>
	/// Returns _Decimal </summary>
	/// <returns> _Decimal </returns>
	public virtual int getDecimal()
	{
		return _Decimal;
	}

	/// <summary>
	/// Returns _Decimal2 </summary>
	/// <returns> _Decimal2 </returns>
	public virtual int getDecimal2()
	{
		return _Decimal2;
	}

	/// <summary>
	/// Returns _Description </summary>
	/// <returns> _Description </returns>
	public virtual string getDescription()
	{
		return _Description;
	}

	/// <summary>
	/// Returns _DisplayUnits </summary>
	/// <returns> _DisplayUnits </returns>
	public virtual string getDisplayUnits()
	{
		return _DisplayUnits;
	}

	/// <summary>
	/// Returns _DisplayUnits2 </summary>
	/// <returns> _DisplayUnits2 </returns>
	public virtual string getDisplayUnits2()
	{
		return _DisplayUnits2;
	}

	/// <summary>
	/// Returns _Equation </summary>
	/// <returns> _Equation </returns>
	public virtual string getEquation()
	{
		return _Equation;
	}

	/// <summary>
	/// Returns _Equation2 </summary>
	/// <returns> _Equation2 </returns>
	public virtual string getEquation2()
	{
		return _Equation2;
	}

	/// <summary>
	/// Returns _Group </summary>
	/// <returns> _Group </returns>
	public virtual string getGroup()
	{
		return _Group;
	}

	/// <summary>
	/// Returns _InService </summary>
	/// <returns> _InService </returns>
	public virtual bool getInService()
	{
		return _InService;
	}

	/// <summary>
	/// Returns _LastCount </summary>
	/// <returns> _LastCount </returns>
	public virtual double getLastCount()
	{
		return _LastCount;
	}

	/// <summary>
	/// Returns _LastUpdate </summary>
	/// <returns> _LastUpdate </returns>
	public virtual System.DateTime getLastUpdate()
	{
		return _LastUpdate;
	}

	/// <summary>
	/// Returns _LastValidData </summary>
	/// <returns> _LastValidData </returns>
	public virtual double getLastValidData()
	{
		return _LastValidData;
	}

	/// <summary>
	/// Returns _LastValidTime </summary>
	/// <returns> _LastValidTime </returns>
	public virtual System.DateTime getLastValidTime()
	{
		return _LastValidTime;
	}

	/// <summary>
	/// Returns _MostRecentData </summary>
	/// <returns> _MostRecentData </returns>
	public virtual double getMostRecentData()
	{
		return _MostRecentData;
	}

	/// <summary>
	/// Returns _MostRecentTime </summary>
	/// <returns> _MostRecentTime </returns>
	public virtual System.DateTime getMostRecentTime()
	{
		return _MostRecentTime;
	}

	/// <summary>
	/// Returns _MaxCount </summary>
	/// <returns> _MaxCount </returns>
	public virtual int getMaxCount()
	{
		return _MaxCount;
	}

	/// <summary>
	/// Returns _MinCount </summary>
	/// <returns> _MinCount </returns>
	public virtual int getMinCount()
	{
		return _MinCount;
	}

	/// <summary>
	/// Returns _NegDelta </summary>
	/// <returns> _NegDelta </returns>
	public virtual int getNegDelta()
	{
		return _NegDelta;
	}

	/// <summary>
	/// Returns _Notify </summary>
	/// <returns> _Notify </returns>
	public virtual bool getNotify()
	{
		return _Notify;
	}

	/// <summary>
	/// Returns _PosDelta </summary>
	/// <returns> _PosDelta </returns>
	public virtual int getPosDelta()
	{
		return _PosDelta;
	}

	/// <summary>
	/// Returns _RatingInterpolation </summary>
	/// <returns> _RatingInterpolation </returns>
	public virtual string getRatingInterpolation()
	{
		return _RatingInterpolation;
	}

	/// <summary>
	/// Returns _RatingShift </summary>
	/// <returns> _RatingShift </returns>
	public virtual double getRatingShift()
	{
		return _RatingShift;
	}

	/// <summary>
	/// Returns _RatingType </summary>
	/// <returns> _RatingType </returns>
	public virtual string getRatingType()
	{
		return _RatingType;
	}

	/// <summary>
	/// Returns _ReferenceLevel </summary>
	/// <returns> _ReferenceLevel </returns>
	public virtual double getReferenceLevel()
	{
		return _ReferenceLevel;
	}

	/// <summary>
	/// Return _SensorID </summary>
	/// <returns> _SensorID </returns>
	public virtual int getSensorID()
	{
		return _SensorID;
	}

	/// <summary>
	/// Return _SiteID </summary>
	/// <returns> _SiteID </returns>
	public virtual int getSiteID()
	{
		return _SiteID;
	}

	/// <summary>
	/// Returns _Slope </summary>
	/// <returns> _Slope </returns>
	public virtual double getSlope()
	{
		return _Slope;
	}

	/// <summary>
	/// Returns _Suspect </summary>
	/// <returns> _Suspect </returns>
	public virtual bool getSuspect()
	{
		return _Suspect;
	}

	/// <summary>
	/// Returns _Timeout </summary>
	/// <returns> _Timeout </returns>
	public virtual double getTimeout()
	{
		return _Timeout;
	}

	/// <summary>
	/// Returns _Type </summary>
	/// <returns> _Type </returns>
	public virtual string getType()
	{
		return _Type;
	}

	/// <summary>
	/// Set _Alarms </summary>
	/// <param name="Alarms"> value to assign to _Alarms. </param>
	public virtual void setAlarms(bool Alarms)
	{
		_Alarms = Alarms;
	}

	/// <summary>
	/// Set _CalibrationDate </summary>
	/// <param name="CalibrationDate"> value to assign to _CalibrationDate. </param>
	public virtual void setCalibrationDate(System.DateTime CalibrationDate)
	{
		_CalibrationDate = CalibrationDate;
	}

	/// <summary>
	/// Set _CalibrationOffset </summary>
	/// <param name="CalibrationOffset"> value to assign to _CalibrationOffset. </param>
	public virtual void setCalibrationOffset(double CalibrationOffset)
	{
		_CalibrationOffset = CalibrationOffset;
	}

	/// <summary>
	/// Set _Children </summary>
	/// <param name="Children"> value to assign to _Children. </param>
	public virtual void setChildren(bool Children)
	{
		_Children = Children;
	}

	/// <summary>
	/// Set _Decimal </summary>
	/// <param name="Decimal"> value to assign to _Decimal. </param>
	public virtual void setDecimal(int Decimal)
	{
		_Decimal = Decimal;
	}

	/// <summary>
	/// Set _Decimal2 </summary>
	/// <param name="Decimal2"> value to assign to _Decimal2. </param>
	public virtual void setDecimal2(int Decimal2)
	{
		_Decimal2 = Decimal2;
	}

	/// <summary>
	/// Set _Description </summary>
	/// <param name="Description"> value to assign to _Description. </param>
	public virtual void setDescription(string Description)
	{
		if (!string.ReferenceEquals(Description, null))
		{
			_Description = Description;
		}
	}

	/// <summary>
	/// Set _DisplayUnits </summary>
	/// <param name="DisplayUnits"> value to assign to _DisplayUnits. </param>
	public virtual void setDisplayUnits(string DisplayUnits)
	{
		if (!string.ReferenceEquals(DisplayUnits, null))
		{
			_DisplayUnits = DisplayUnits;
		}
	}

	/// <summary>
	/// Set _DisplayUnits2 </summary>
	/// <param name="DisplayUnits2"> value to assign to _DisplayUnits2. </param>
	public virtual void setDisplayUnits2(string DisplayUnits2)
	{
		if (!string.ReferenceEquals(DisplayUnits2, null))
		{
			_DisplayUnits2 = DisplayUnits2;
		}
	}

	/// <summary>
	/// Set _Equation </summary>
	/// <param name="Equation"> value to assign to _Equation. </param>
	public virtual void setEquation(string Equation)
	{
		if (!string.ReferenceEquals(Equation, null))
		{
			_Equation = Equation;
		}
	}

	/// <summary>
	/// Set _Equation2 </summary>
	/// <param name="Equation2"> value to assign to _Equation2. </param>
	public virtual void setEquation2(string Equation2)
	{
		if (!string.ReferenceEquals(Equation2, null))
		{
			_Equation2 = Equation2;
		}
	}

	/// <summary>
	/// Set _Group </summary>
	/// <param name="Group"> value to assign to _Group. </param>
	public virtual void setGroup(string Group)
	{
		if (!string.ReferenceEquals(Group, null))
		{
			_Group = Group;
		}
	}

	/// <summary>
	/// Set _InService </summary>
	/// <param name="InService"> value to assign to _InService. </param>
	public virtual void setInService(bool InService)
	{
		_InService = InService;
	}

	/// <summary>
	/// Set _LastCount </summary>
	/// <param name="LastCount"> value to assign to _LastCount. </param>
	public virtual void setLastCount(double LastCount)
	{
		_LastCount = LastCount;
	}

	/// <summary>
	/// Set _LastUpdate </summary>
	/// <param name="LastUpdate"> value to assign to _LastUpdate. </param>
	public virtual void setLastUpdate(System.DateTime LastUpdate)
	{
		_LastUpdate = LastUpdate;
	}

	/// <summary>
	/// Set _LastValidData </summary>
	/// <param name="LastValidData"> value to assign to _LastValidData. </param>
	public virtual void setLastValidData(double LastValidData)
	{
		_LastValidData = LastValidData;
	}

	/// <summary>
	/// Set _LastValidTime </summary>
	/// <param name="LastValidTime"> value to assign to _LastValidTime. </param>
	public virtual void setLastValidTime(System.DateTime LastValidTime)
	{
		_LastValidTime = LastValidTime;
	}

	/// <summary>
	/// Set _MaxCount </summary>
	/// <param name="MaxCount"> value to assign to _MaxCount. </param>
	public virtual void setMaxCount(int MaxCount)
	{
		_MaxCount = MaxCount;
	}

	/// <summary>
	/// Set _MinCount </summary>
	/// <param name="MinCount"> value to assign to _MinCount. </param>
	public virtual void setMinCount(int MinCount)
	{
		_MinCount = MinCount;
	}

	/// <summary>
	/// Set _MostRecentData </summary>
	/// <param name="MostRecentData"> value to assign to _MostRecentData. </param>
	public virtual void setMostRecentData(double MostRecentData)
	{
		_MostRecentData = MostRecentData;
	}

	/// <summary>
	/// Set _MostRecentTime </summary>
	/// <param name="MostRecentTime"> value to assign to _MostRecentTime. </param>
	public virtual void setMostRecentTime(System.DateTime MostRecentTime)
	{
		_MostRecentTime = MostRecentTime;
	}

	/// <summary>
	/// Set _NegDelta </summary>
	/// <param name="NegDelta"> value to assign to _NegDelta. </param>
	public virtual void setNegDelta(int NegDelta)
	{
		_NegDelta = NegDelta;
	}

	/// <summary>
	/// Set _Notify </summary>
	/// <param name="Notify"> value to assign to _Notify. </param>
	public virtual void setNotify(bool Notify)
	{
		_Notify = Notify;
	}

	/// <summary>
	/// Set _PosDelta </summary>
	/// <param name="PosDelta"> value to assign to _PosDelta. </param>
	public virtual void setPosDelta(int PosDelta)
	{
		_PosDelta = PosDelta;
	}

	/// <summary>
	/// Set _RatingInterpolation </summary>
	/// <param name="RatingInterpolation"> value to assign to _RatingInterpolation. </param>
	public virtual void setRatingInterpolation(string RatingInterpolation)
	{
		if (!string.ReferenceEquals(RatingInterpolation, null))
		{
			_RatingInterpolation = RatingInterpolation;
		}
	}

	/// <summary>
	/// Set _RatingShift. </summary>
	/// <param name="RatingShift"> value to assign to _RatingShift. </param>
	public virtual void setRatingShift(double RatingShift)
	{
		_RatingShift = RatingShift;
	}

	/// <summary>
	/// Set _RatingType </summary>
	/// <param name="RatingType"> value to assign to _RatingType. </param>
	public virtual void setRatingType(string RatingType)
	{
		if (!string.ReferenceEquals(RatingType, null))
		{
			_RatingType = RatingType;
		}
	}

	/// <summary>
	/// Set _ReferenceLevel </summary>
	/// <param name="ReferenceLevel"> value to assign to _ReferenceLevel. </param>
	public virtual void setReferenceLevel(double ReferenceLevel)
	{
		_ReferenceLevel = ReferenceLevel;
	}

	/// <summary>
	/// Set _SensorID </summary>
	/// <param name="SensorID"> value to assign to _SensorID. </param>
	public virtual void setSensorID(int SensorID)
	{
		_SensorID = SensorID;
	}

	/// <summary>
	/// Set _SiteID </summary>
	/// <param name="SiteID"> value to assign to _SiteID. </param>
	public virtual void setSiteID(int SiteID)
	{
		_SiteID = SiteID;
	}

	/// <summary>
	/// Set _Slope </summary>
	/// <param name="Slope"> value to assign to _Slope. </param>
	public virtual void setSlope(double Slope)
	{
		_Slope = Slope;
	}

	/// <summary>
	/// Set _Suspect </summary>
	/// <param name="Suspect"> value to assign to _Suspect. </param>
	public virtual void setSuspect(bool Suspect)
	{
		_Suspect = Suspect;
	}

	/// <summary>
	/// Set _Timeout. </summary>
	/// <param name="Timeout"> value to assign to _Timeout. </param>
	public virtual void setTimeout(double Timeout)
	{
		_Timeout = Timeout;
	}

	/// <summary>
	/// Set _Type </summary>
	/// <param name="Type"> value to assign to _Type. </param>
	public virtual void setType(string Type)
	{
		if (!string.ReferenceEquals(Type, null))
		{
			_Type = Type;
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

	} // End DIADvisor_SensorDef

}