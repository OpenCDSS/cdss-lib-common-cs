// TSDataPosition - simple class for returning data position in internal array

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
// TSDataPosition - simple class for returning data position in internal array
// ----------------------------------------------------------------------------
// History:
//
// 24 Sep 1997	Steven A. Malers, RTi	Initial version.
// 13 Apr 1999	SAM, RTi		Add finalize.
// 2001-11-06	SAM, RTi		Review javadoc.  Verify that variables
//					are set to null when no longer used.
//					Change set methods to have void return
//					type.
// ----------------------------------------------------------------------------

namespace RTi.TS
{
	/// <summary>
	/// The TSDataPosition class is used by the getDataPosition methods in time series
	/// classes to return data positions within the data arrays.  This class supports
	/// up to three array dimensions for the position.  It is used by accessor functions
	/// in the TS library but should generally not be used outside the library.
	/// For example, if parallel data MonthTS and string StringMonthTS time series are
	/// maintained, TSDataPosition can be used to look up the position in one for
	/// access in another.  In this way, only MonthTS has the position lookup code.
	/// </summary>
	public class TSDataPosition
	{

	private int _position1;
	private int _position2;
	private int _position3;
	private bool _found;

	/// <summary>
	/// Default constructor.  Each dimension is initialized to -1.
	/// </summary>
	internal TSDataPosition()
	{
		_found = false;
		_position1 = -1;
		_position2 = -1;
		_position3 = -1;
	}

	/// <summary>
	/// Finalize before garbage collection. </summary>
	/// <exception cref="Throwable"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~TSDataPosition()
	{
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the first array dimension position. </summary>
	/// <returns> The first array dimension position. </returns>
	public virtual int getPosition1()
	{
		return _position1;
	}

	/// <summary>
	/// Return the second array dimension position. </summary>
	/// <returns> The second array dimension position. </returns>
	public virtual int getPosition2()
	{
		return _position2;
	}

	/// <summary>
	/// Return The third array dimension position. </summary>
	/// <returns> The third array dimension position. </returns>
	public virtual int getPosition3()
	{
		return _position3;
	}

	/// <summary>
	/// Determine whether the position has been found. </summary>
	/// <returns> true if the position has been found, false if not. </returns>
	public virtual bool positionFound()
	{
		return _found;
	}

	/// <summary>
	/// Set the array dimension position for the first dimension. </summary>
	/// <param name="position1"> The first array dimension position. </param>
	public virtual void setPosition(int position1)
	{
		_position1 = position1;
		_found = true;
	}

	/// <summary>
	/// Set the array dimension position for the first and second dimension. </summary>
	/// <param name="position1"> The first array dimension position. </param>
	/// <param name="position2"> The second array dimension position. </param>
	public virtual void setPosition(int position1, int position2)
	{
		_position1 = position1;
		_position2 = position2;
		_found = true;
	}

	/// <summary>
	/// Set the array dimension position for the first, second, and third dimension. </summary>
	/// <param name="position1"> The first array dimension position. </param>
	/// <param name="position2"> The second array dimension position. </param>
	/// <param name="position3"> The third array dimension position. </param>
	public virtual void setPosition(int position1, int position2, int position3)
	{
		_position1 = position1;
		_position2 = position2;
		_position3 = position3;
		_found = true;
	}

	} // End of TSDataPosition

}