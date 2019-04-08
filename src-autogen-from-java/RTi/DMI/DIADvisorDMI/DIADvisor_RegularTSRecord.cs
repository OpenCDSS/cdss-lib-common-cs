// ----------------------------------------------------------------------------
// DIADvisor_RegularTSRecord.java - corresponds to DIADvisor regular interval
//					time series tables
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2003-02-16	Steven A. Malers, RTi	Initial version.  Copy and modify
//					DIADvisor_DataChron.
// 2003-04-04	SAM, RTi		Rename from DIADvisor_TSRecord.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.DMI.DIADvisorDMI
{


	/// <summary>
	/// The DIADvisor_RegularTSRecord class store data records from the DIADvisor
	/// Interval, Hour, and Day tables, which all have the same format.
	/// </summary>
	public class DIADvisor_RegularTSRecord : DMIDataObject
	{

	// From table Interval, Hour, or Day

	protected internal System.DateTime _StartTime = DMIUtil.MISSING_DATE;
	protected internal int _SensorID = DMIUtil.MISSING_INT;
	protected internal int _Count = DMIUtil.MISSING_INT;
	protected internal double _Value = DMIUtil.MISSING_DOUBLE;

	/// <summary>
	/// DIADvisor_RegularTSRecord constructor.
	/// </summary>
	public DIADvisor_RegularTSRecord() : base()
	{
	}

	/// <summary>
	/// Cleans up variables when the class is disposed of.  Sets all the member
	/// variables (that aren't primitives) to null.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~DIADvisor_RegularTSRecord()
	{
		_StartTime = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns _Count </summary>
	/// <returns> _Count </returns>
	public virtual int getCount()
	{
		return _Count;
	}

	/// <summary>
	/// Returns _SensorID </summary>
	/// <returns> _SensorID </returns>
	public virtual int getSensorID()
	{
		return _SensorID;
	}

	/// <summary>
	/// Returns _StartTime </summary>
	/// <returns> _StartTime </returns>
	public virtual System.DateTime getStartTime()
	{
		return _StartTime;
	}

	/// <summary>
	/// Returns _Value </summary>
	/// <returns> _Value </returns>
	public virtual double getValue()
	{
		return _Value;
	}

	/// <summary>
	/// Set _Count. </summary>
	/// <param name="Count"> value to assign to _Count. </param>
	public virtual void setCount(int Count)
	{
		_Count = Count;
	}

	/// <summary>
	/// Set _SensorID. </summary>
	/// <param name="SensorID"> value to assign to _SensorID. </param>
	public virtual void setSensorID(int SensorID)
	{
		_SensorID = SensorID;
	}

	/// <summary>
	/// Set _StartTime </summary>
	/// <param name="StartTime"> value to assign to _StartTime. </param>
	public virtual void setStartTime(System.DateTime StartTime)
	{
		_StartTime = StartTime;
	}

	/// <summary>
	/// Set _Value. </summary>
	/// <param name="Value"> value to assign to _Value. </param>
	public virtual void setValue(double Value)
	{
		_Value = Value;
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

	} // End DIADvisor_RegularTSRecord

}