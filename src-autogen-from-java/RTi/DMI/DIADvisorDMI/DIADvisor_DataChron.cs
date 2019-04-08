// ----------------------------------------------------------------------------
// DIADvisor_DataChron.java - corresponds to DIADvisor DataChron table
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
	/// The DIADvisor_DataChron class store data records from the DIADvisor DataChron
	/// table.
	/// </summary>
	public class DIADvisor_DataChron : DMIDataObject
	{

	// From table DataChron

	protected internal System.DateTime _DateTime = DMIUtil.MISSING_DATE;
	protected internal int _SensorID = DMIUtil.MISSING_INT;
	protected internal int _Count = DMIUtil.MISSING_INT;
	protected internal string _DataType = DMIUtil.MISSING_STRING;
	protected internal string _Source = DMIUtil.MISSING_STRING;
	protected internal double _DataValue = DMIUtil.MISSING_DOUBLE;
	protected internal double _DataValue2 = DMIUtil.MISSING_DOUBLE;
	protected internal long _SeqNum = DMIUtil.MISSING_LONG;
	protected internal string _Comment = DMIUtil.MISSING_STRING;

	/// <summary>
	/// DIADvisor_SiteDef constructor.
	/// </summary>
	public DIADvisor_DataChron() : base()
	{
	}

	/// <summary>
	/// Cleans up variables when the class is disposed of.  Sets all the member
	/// variables (that aren't primitives) to null.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~DIADvisor_DataChron()
	{
		_DateTime = null;
		_DataType = null;
		_Source = null;
		_Comment = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns _Comment </summary>
	/// <returns> _Comment </returns>
	public virtual string getComment()
	{
		return _Comment;
	}

	/// <summary>
	/// Returns _Count </summary>
	/// <returns> _Count </returns>
	public virtual int getCount()
	{
		return _Count;
	}

	/// <summary>
	/// Returns _DateTime </summary>
	/// <returns> _DateTime </returns>
	public virtual System.DateTime getDateTime()
	{
		return _DateTime;
	}

	/// <summary>
	/// Returns _DataType </summary>
	/// <returns> _DataType </returns>
	public virtual string getDataType()
	{
		return _DataType;
	}

	/// <summary>
	/// Returns _DataValue </summary>
	/// <returns> _DataValue </returns>
	public virtual double getDataValue()
	{
		return _DataValue;
	}

	/// <summary>
	/// Returns _DataValue2 </summary>
	/// <returns> _DataValue2 </returns>
	public virtual double getDataValue2()
	{
		return _DataValue2;
	}

	/// <summary>
	/// Returns _Source </summary>
	/// <returns> _Source </returns>
	public virtual string getSource()
	{
		return _Source;
	}

	/// <summary>
	/// Returns _SensorID </summary>
	/// <returns> _SensorID </returns>
	public virtual int getSensorID()
	{
		return _SensorID;
	}

	/// <summary>
	/// Returns _SeqNum </summary>
	/// <returns> _SeqNum </returns>
	public virtual long getSeqNum()
	{
		return _SeqNum;
	}

	/// <summary>
	/// Set _Comment </summary>
	/// <param name="Comment"> value to assign to _Comment. </param>
	public virtual void setComment(string Comment)
	{
		if (!string.ReferenceEquals(Comment, null))
		{
			_Comment = Comment;
		}
	}

	/// <summary>
	/// Set _Count. </summary>
	/// <param name="Count"> value to assign to _Count. </param>
	public virtual void setCount(int Count)
	{
		_Count = Count;
	}

	/// <summary>
	/// Set _DataType </summary>
	/// <param name="DataType"> value to assign to _DataType. </param>
	public virtual void setDataType(string DataType)
	{
		if (!string.ReferenceEquals(DataType, null))
		{
			_DataType = DataType;
		}
	}

	/// <summary>
	/// Set _DataValue. </summary>
	/// <param name="DataValue"> value to assign to _DataValue. </param>
	public virtual void setDataValue(double DataValue)
	{
		_DataValue = DataValue;
	}

	/// <summary>
	/// Set _DataValue2. </summary>
	/// <param name="DataValue2"> value to assign to _DataValue2. </param>
	public virtual void setDataValue2(double DataValue2)
	{
		_DataValue2 = DataValue2;
	}

	/// <summary>
	/// Set _DateTime </summary>
	/// <param name="DateTime"> value to assign to _DateTime. </param>
	public virtual void setDateTime(System.DateTime DateTime)
	{
		_DateTime = DateTime;
	}

	/// <summary>
	/// Set _SensorID. </summary>
	/// <param name="SensorID"> value to assign to _SensorID. </param>
	public virtual void setSensorID(int SensorID)
	{
		_SensorID = SensorID;
	}

	/// <summary>
	/// Set _Source </summary>
	/// <param name="Source"> value to assign to _Source. </param>
	public virtual void setSource(string Source)
	{
		if (!string.ReferenceEquals(Source, null))
		{
			_Source = Source;
		}
	}

	/// <summary>
	/// Set _SeqNum. </summary>
	/// <param name="SeqNum"> value to assign to _SeqNum. </param>
	public virtual void setSeqNum(long SeqNum)
	{
		_SeqNum = SeqNum;
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

	} // End DIADvisor_DataChron

}