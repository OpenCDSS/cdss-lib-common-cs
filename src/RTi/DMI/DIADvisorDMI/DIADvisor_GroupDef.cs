// ----------------------------------------------------------------------------
// DIADvisor_GroupDef.java - corresponds to DIADvisor GroupDef table
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2003-04-06	Steven A. Malers, RTi	Initial version.  Copy and modify
//					RiversideDB_SiteDef.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.DMI.DIADvisorDMI
{

	/// <summary>
	/// The DIADvisor_GroupDef class store data records from the DIADvisor GroupDef
	/// table.
	/// </summary>
	public class DIADvisor_GroupDef : DMIDataObject
	{

	// From table GroupDef

	protected internal string _Group = DMIUtil.MISSING_STRING;
	protected internal string _Operation = DMIUtil.MISSING_STRING;
	protected internal string _Units1 = DMIUtil.MISSING_STRING;
	protected internal string _Units2 = DMIUtil.MISSING_STRING;
	protected internal int _Display = DMIUtil.MISSING_INT;

	/// <summary>
	/// DIADvisor_GroupDef constructor.
	/// </summary>
	public DIADvisor_GroupDef() : base()
	{
	}

	/// <summary>
	/// Cleans up variables when the class is disposed of.  Sets all the member
	/// variables (that aren't primitives) to null. </summary>
	/// <exception cref="Throwable"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~DIADvisor_GroupDef()
	{
		_Group = DMIUtil.MISSING_STRING;
		_Operation = DMIUtil.MISSING_STRING;
		_Units1 = DMIUtil.MISSING_STRING;
		_Units2 = DMIUtil.MISSING_STRING;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns _Display </summary>
	/// <returns> _Display </returns>
	public virtual int getDisplay()
	{
		return _Display;
	}

	/// <summary>
	/// Returns _Group </summary>
	/// <returns> _Group </returns>
	public virtual string getGroup()
	{
		return _Group;
	}

	/// <summary>
	/// Returns _Operation </summary>
	/// <returns> _Operation </returns>
	public virtual string getOperation()
	{
		return _Operation;
	}

	/// <summary>
	/// Returns _Units1 </summary>
	/// <returns> _Units1 </returns>
	public virtual string getUnits1()
	{
		return _Units1;
	}

	/// <summary>
	/// Returns _Units2 </summary>
	/// <returns> _Units2 </returns>
	public virtual string getUnits2()
	{
		return _Units2;
	}

	/// <summary>
	/// Set _Display. </summary>
	/// <param name="Display"> value to assign to _Display. </param>
	public virtual void setDisplay(int Display)
	{
		_Display = Display;
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
	/// Set _Operation </summary>
	/// <param name="Operation"> value to assign to _Operation. </param>
	public virtual void setOperation(string Operation)
	{
		if (!string.ReferenceEquals(Operation, null))
		{
			_Operation = Operation;
		}
	}

	/// <summary>
	/// Set _Units1 </summary>
	/// <param name="Units1"> value to assign to _Units1. </param>
	public virtual void setUnits1(string Units1)
	{
		if (!string.ReferenceEquals(Units1, null))
		{
			_Units1 = Units1;
		}
	}

	/// <summary>
	/// Set _Units2 </summary>
	/// <param name="Units2"> value to assign to _Units2. </param>
	public virtual void setUnits2(string Units2)
	{
		if (!string.ReferenceEquals(Units2, null))
		{
			_Units2 = Units2;
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

	} // End DIADvisor_GroupDef

}