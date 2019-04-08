// DataUnitsConversion - data units conversion class

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
// DataUnitsConversion - data units conversion class
// ----------------------------------------------------------------------------
// Copyright:  See the COPYRIGHT file.
// ----------------------------------------------------------------------------
// History:
//
// 13 Jan 1998	Steven A. Malers, RTi	Initial version.
// 19 Mar 1998	SAM, RTi		Add javadoc.
// 13 Apr 1999	SAM, RTi		Add finalize.  Clean up code.  Add
//					constructor that takes data.
// 2001-11-06	SAM, RTi		Review javadoc.  Verify that variables
//					are set to null when no longer used.
// 2001-12-09	SAM, RTi		Copy TSUnits* to Data* to allow general
//					use of classes.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.Util.IO
{
	/// <summary>
	/// The DataUnitsConversion class stores the conversion factors for changing from
	/// one set of units to another.  An instance is returned from
	/// DataUnits.getConversion.
	/// The DataUnits.getConversion() method normally initializes some of the fields in
	/// this class.  Currently, there are no plans to move getConversion() to this class
	/// because that function depends on DataUnits data. </summary>
	/// <seealso cref= DataUnits#getConversion </seealso>
	public class DataUnitsConversion
	{

	private double _add_factor; // Factor to add to convert from
						// _original_units to _new_units.
	private double _mult_factor; // Factor to multiply by to convert from
						// _original_units to _new_units.
	private string _original_units; // The original data units.
	private string _new_units; // The new data units (result of
						// applying the factors).

	/// <summary>
	/// Construct and set the multiplication factor to 1.0 and the add factor to 0.0.
	/// </summary>
	public DataUnitsConversion()
	{
		initialize();
	}

	/// <summary>
	/// Construct using data values.
	/// Construct using data values.  The add factor is set to zero. </summary>
	/// <param name="original_units"> Units before conversion. </param>
	/// <param name="new_units"> Units after conversion. </param>
	/// <param name="mult_factor"> Factor to multiply old units by to get new units. </param>
	/// <param name="add_factor"> Factor to add old units (after multiplication) to get
	/// new units. </param>
	public DataUnitsConversion(string original_units, string new_units, double mult_factor, double add_factor)
	{
		initialize();
		setOriginalUnits(original_units);
		setNewUnits(new_units);
		_mult_factor = mult_factor;
		_add_factor = add_factor;
	}

	/// <summary>
	/// Construct using data values.  The add factor is set to zero. </summary>
	/// <param name="original_units"> Units before conversion. </param>
	/// <param name="new_units"> Units after conversion. </param>
	/// <param name="mult_factor"> Factor to multiply old units by to get new units. </param>
	public DataUnitsConversion(string original_units, string new_units, double mult_factor)
	{
		initialize();
		setOriginalUnits(original_units);
		setNewUnits(new_units);
		_mult_factor = mult_factor;
		_add_factor = 0.0;
	}

	/// <summary>
	/// Copy constructor. </summary>
	/// <param name="conv"> DataUnitsConversion instance to copy. </param>
	public DataUnitsConversion(DataUnitsConversion conv)
	{
		initialize();
		setAddFactor(conv._add_factor);
		setMultFactor(conv._mult_factor);
		setOriginalUnits(conv._original_units);
		setNewUnits(conv._new_units);
	}

	/// <summary>
	/// Finalize before garbage collection. </summary>
	/// <exception cref="Throwable"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~DataUnitsConversion()
	{
		_original_units = null;
		_new_units = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the factor to add by to perform the conversion. </summary>
	/// <returns> The factor to add by to perform the conversion. </returns>
	public virtual double getAddFactor()
	{
		return _add_factor;
	}

	/// <summary>
	/// Return the factor to multiply by to perform the conversion. </summary>
	/// <returns> The factor to multiply by to perform the conversion. </returns>
	public virtual double getMultFactor()
	{
		return _mult_factor;
	}

	/// <summary>
	/// Return the new units (after conversion). </summary>
	/// <returns> The new units (after conversion). </returns>
	public virtual string getNewUnits()
	{
		return _new_units;
	}

	/// <summary>
	/// Return the original units (before conversion). </summary>
	/// <returns> The original units (before conversion). </returns>
	public virtual string getOriginalUnits()
	{
		return _original_units;
	}

	/// <summary>
	/// Initialize data members.
	/// </summary>
	private void initialize()
	{
		_original_units = "";
		_new_units = "";
		_add_factor = 0.0; // Results in no conversion.
		_mult_factor = 1.0;
	}

	/// <summary>
	/// Set the addition factor for the conversion (this is normally only needed for
	/// temperature conversions). </summary>
	/// <param name="add_factor"> The addition factor. </param>
	public virtual void setAddFactor(double add_factor)
	{
		_add_factor = add_factor;
	}

	/// <summary>
	/// Set the multiplication factor for the conversion. </summary>
	/// <param name="mult_factor"> The multiplication factor. </param>
	public virtual void setMultFactor(double mult_factor)
	{
		_mult_factor = mult_factor;
	}

	/// <summary>
	/// Set the new units (after conversion). </summary>
	/// <param name="new_units"> Data units after conversion. </param>
	public virtual void setNewUnits(string new_units)
	{
		if (string.ReferenceEquals(new_units, null))
		{
			return;
		}
		_new_units = new_units;
	}

	/// <summary>
	/// Set the original units (before conversion). </summary>
	/// <param name="original_units"> Data units before conversion. </param>
	public virtual void setOriginalUnits(string original_units)
	{
		if (string.ReferenceEquals(original_units, null))
		{
			return;
		}
		_original_units = original_units;
	}

	/// <summary>
	/// Return a string representation of the units (verbose output). </summary>
	/// <returns> A string representation of the units (verbose output). </returns>
	public override string ToString()
	{
		return "Conv:  \"" + _original_units + "\" to \"" + _new_units + "\", MultBy:" + _mult_factor + ", Add:" + _add_factor;
	}

	} // End of DataUnitsConversion

}